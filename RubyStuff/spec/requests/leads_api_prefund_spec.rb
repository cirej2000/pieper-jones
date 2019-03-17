require_relative('../spec_helper')
require_relative('../helpers/kba_helper')

describe 'leads api pre-fund flow' do
  CONFIG = YAML.load_file(File.expand_path('../config/leads_api.yml',
                                           File.dirname(__FILE__)))
  env = ENV['ENV'] ? ENV['ENV'] : 'dev'
  @file_path = CONFIG[env]['PRE_FUND_FILE']
  # TODO: roll the next three lines into a single loop as suggested by ScottL
  @file_reader = CSV.read(File.expand_path(@file_path, File.dirname(__FILE__)),
                          headers: true)
  @lead_records = @file_reader.by_row!

  before(:all) do
    @url = CONFIG[env]['LEADS_API_URL'] + CONFIG['LEADS_API_PATH']
    @doc_url = CONFIG[env]['LEADS_API_URL'] + CONFIG['DOCUMENT_API_PATH']
    @rails_url = CONFIG[env]['LEADS_RAILS_URL']
    @rails_api_url = CONFIG[env]['LEADS_RAILS_URL'] + CONFIG['LEADS_RAILS_API_PATH']
    @kba_url = CONFIG[env]['KBA_API_URL'] + CONFIG['KBA_API_PATH']
    @kba_answers_url = CONFIG[env]['KBA_API_URL'] + CONFIG['KBA_ANSWERS_PATH']
    @fraud_shield_url = CONFIG[env]['KBA_API_URL'] + CONFIG['FRAUD_SHIELD_API_PATH']
    @log = Logger.new(STDOUT, 'leads-api-test.log', 'daily')
    @log.level = Logger::INFO
  end

  before do
    @mid_offer = nil
    @continue = true
  end

  # Start looping through our leads
  @lead_records.each do |lead_record|
    describe 'get leads to pre-fund' do
      lead = LeadKBAObject.new(lead_record)
      lead_name = "#{lead.first_name} #{lead.last_name}"
      include KbaHelper

      it "should go through the #{lead.final_decision} flow for #{lead_name}" do
        # 1.  Create Lead --- Lead State Collected
        body = leads_api_create_lead(lead, @url)
        expect_status(200)
        expect_json_keys('lead', [:pj_uid])
        pj_uid = body[:lead][:pj_uid]
        @log.info("Starting leads flow for lead #{lead_name}. pj_guid:  #{pj_uid}")

        # 1a. Get our session_id and X-CSRF-TOKEN fields from the lead_rails call with the pj_guid
        get "#{@rails_url}/#{pj_uid}"
        headers = response.headers
        session_id = get_session_id(headers[:set_cookie][0].split(';'))
        csrf_token = get_csrf_token(response.body.to_s)

        # 2. Run Prequal Shown Request using lead-rails
        request_payload = RailsPrequalShownObject.new(pj_uid).request_object
        patch "#{@rails_api_url}/#{pj_uid}/leads", request_payload,
              'Cookie' => "_session_id=#{session_id}", 'X-CSRF-TOKEN' => "#{csrf_token}"
        expect_status(200)
        expect_json('lead.pj_uid', pj_uid)
        expect_json('lead_state', 'pre_qual_shown')

        # 3. Patch the lead to pre_qual_collected using lead_rails
        # (registers a user and gets pj_user_slug)
        pre_qual_coll_obj = RailsPrequalCollectedObject.new(lead, pj_uid)
        request_payload = pre_qual_coll_obj.prequal
        patch "#{@rails_api_url}/#{pj_uid}/leads", request_payload,
              'Cookie' => "_session_id=#{session_id}", 'X-CSRF-TOKEN' => "#{csrf_token}"
        expect_status(200)
        expect_json('lead.pj_uid', pj_uid)
        expect_json('lead.final_decision', lead.final_decision)

        # Now let's do our response validations by expected final_decision
        if 'Review' == lead.final_decision
          expect_json('lead_state', 'review')
          @continue = false
        elsif 'Decline' == lead.final_decision
          expect_json('lead_state', 'decline')
          expect(validate_fico_score(json_body, lead))
            .to be_truthy unless lead.fico_score.empty?
          size_match, actions_match = validate_adverse_actions(json_body, lead)
          expect(size_match).to be_truthy
          expect(actions_match).to be_truthy
          @continue = false
        elsif 'Approve' == lead.final_decision
          expect_json('lead_state', 'offer_generated')
          expect_json('lead.pricing_tier', "t#{lead.pricing_tier}")
          expect(validate_fico_score(json_body, lead)).to be_truthy
          expect_json_keys('lead', [:offers])
          # Now let's grab the mid offer with the median term in months
          @mid_offer = get_target_offer(json_body[:lead][:offers],
                                        lead.medium_term)
          term_match, interest_match, apr_match, payment_match =
           validate_mid_offer(@mid_offer, lead)
          expect(term_match).to be_truthy
          expect(interest_match).to be_truthy
          expect(apr_match).to be_truthy
          expect(payment_match).to be_truthy
        end

        if @continue

          # 4. Offer Accepted
          json = OfferAcceptedObject.new(@mid_offer).offer
          patch "#{@url}/#{pj_uid}", json
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'offer_accepted')

          # 5. Loan Details
          payload = LoanDetailRequestObject.new(lead).details
          patch "#{@url}/#{pj_uid}", payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'offer_accepted')

          # 5a. Truth In Lending Shown
          payload = PatchRequestObject.new('til_shown').request_object
          patch "#{@url}/#{pj_uid}", payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          # expect_json('lead_state', 'offer_accepted')

          # 5a. Truth In Lending Accepted
          payload = PatchRequestObject.new('til_accepted').request_object
          patch "#{@url}/#{pj_uid}", payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])

          # 5c. Credit Score Notice Shown
          payload = PatchRequestObject.new('credit_score_notice_shown').request_object
          patch "#{@url}/#{pj_uid}", payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          # expect_json('lead_state', 'offer_accepted')

          # 5d. Credit Score Notice Accepted
          payload = PatchRequestObject.new('credit_score_notice_shown').request_object
          patch "#{@url}/#{pj_uid}", payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])

          # 6. Collect ACH
          payload = CollectAchRequestObject.new(lead).ach
          patch "#{@url}/#{pj_uid}", payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'ach_collected')

          # 7. Finalize Loan
          payload = PatchRequestObject.new('loan_app_finalized').request_object
          patch "#{@url}/#{pj_uid}", payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'kba_disclosure_pending')

          # 8 - Fraud Shield
          payload = FraudShieldRequestObject.new(lead, pj_uid).f_shield
          response = post @fraud_shield_url, payload

          # 8a.  Get our kba_session_id to be used in subsequent kba requests
          kba_session_id = json_body[:kba_session_id]

          # 9.  Send our kba request for list of questions
          kba_request = KbaRequestObject.new(lead, pj_uid, kba_session_id).kba
          post @kba_url, kba_request
          questions = json_body
          expect(questions).to be_truthy
          kba_session_id = questions[:kba_session_id]

          # 9a. Generate our list of correct answers from our questions data
          answers = ret_answers_list(questions, lead)

          # 10.  Send our answers to kba/answers endpoint.
          answer_request = JSON.parse(KbaAnswersRequestObject.new(kba_session_id, answers).kba_answers)
          post @kba_answers_url, answer_request

          # 10a. if we get a progressive question, then let's answer it
          if json_body[:status_code] == 300
            questions = json_body
            kba_session_id = questions[:kba_session_id]
            answers = ret_answers_list(questions, lead)
            answer_request = JSON.parse(KbaAnswersRequestObject.new(kba_session_id, answers).kba_answers)
            post @kba_answers_url, answer_request
          end

          # 10b. Either way, progressive or no, we should end up here
          expect_status(200)
          expect_json('kiq.accept_refer_code', 'ACC')
          expect_json('kiq.final_decision', 'XXX')

          # 11. Back to leads-api:  Let's send the KBA_SUCCESS status when we get here
          request_payload = PatchRequestObject.new('kba_success').request_object
          patch "#{@url}/#{pj_uid}", request_payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'kba_success')

          # 12. Agent Verified request
          request_payload = PatchRequestObject.new('agent_verified').request_object
          patch "#{@url}/#{pj_uid}", request_payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'agent_verified')

          # 13. Loan Confirmation request
          request_payload = PatchRequestObject.new('loan_confirmation_shown').request_object
          patch "#{@url}/#{pj_uid}", request_payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'agent_verification_pending')

          # 14. Borrower e-sign pending
          request_payload = PatchRequestObject.new('e_sign_borrower_pending').request_object
          patch "#{@url}/#{pj_uid}", request_payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'e_sign_borrower_pending')

          # 15. Borrower e-sign shown
          request_payload = PatchRequestObject.new('e_sign_borrower_shown').request_object
          patch "#{@url}/#{pj_uid}", request_payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'e_sign_borrower_shown')

          # 16. Borrower e-sign complete
          payload = ESignBorrowerCompleteObject.new(lead).e_sign
          patch "#{@url}/#{pj_uid}", payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'e_sign_borrower_signed')

          # 17. Promissory Pending
          # request_payload = PatchRequestObject.new('e_sign_promissory_pending').request_object
          # patch "#{@url}/#{pj_uid}", request_payload
          # expect_status(200)
          # expect_json('lead.pj_uid', pj_uid)
          # expect_json_keys('lead', [:offers])
          # expect_json('lead_state', 'e_sign_promissory_pending')

          # 18. Promissory Shown
          request_payload = PatchRequestObject.new('e_sign_promissory_shown').request_object
          patch "#{@url}/#{pj_uid}", request_payload
          expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          expect_json('lead_state', 'e_sign_promissory_shown')

          # 19. Sign promissory note
          payload = ESignDocumentsObject.new(lead).e_sign
          response = patch "#{@url}/#{pj_uid}", payload
          # expect_status(200)
          expect_json('lead.pj_uid', pj_uid)
          expect_json_keys('lead', [:offers])
          # expect_json('lead_state', 'pre_funding')

          # 19c. Validate promissory note dates
          loan_payment_info = json_body[:lead][:loan_payment_info]
          term_months = @mid_offer.term_months
          first_payment_date, maturity_date = get_loan_payment_dates(term_months, json_body)
          first_payment_date_actual = loan_payment_info[:amortization_schedule][0][:normalized_payment_date]
          maturity_date_actual = loan_payment_info[:amortization_schedule][term_months - 1][:normalized_payment_date]
          expect(Date.parse(first_payment_date_actual)).to eql(first_payment_date), 'bad first_payment date match'
          expect(Date.parse(maturity_date_actual)).to eql(maturity_date), 'loan maturity date mismatch'
        end
      end
    end
  end
end
