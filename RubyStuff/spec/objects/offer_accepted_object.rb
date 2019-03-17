
class OfferAcceptedObject
  attr_reader :offer

  def initialize(offer)
    @offer = {
      lead_state: 'offer_accepted',
      lead: {
        selected_loan_amount: offer.amount,
        selected_term: offer.term_months.to_s,
        selected_interest_rate: offer.interest_rate.to_s,
        selected_apr: offer.apr,
        selected_monthly_payment: offer.monthly_payment,
        selected_origination_fee_rate: offer.origination_fee_rate.to_d.round(1)
                                       .to_s('F'),
        selected_origination_fee: offer.origination_fee.to_d.round(1).to_s('F')
      }
    }
  end
end
