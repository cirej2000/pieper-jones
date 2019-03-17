
class KbaRequestObject
  attr_reader :kba

  def initialize(applicant, guid, kba_session_id)
    @kba = {
      pj_uid: guid,
      kba_session_id: kba_session_id,
      applicant: {
        first_name: applicant.first_name,
        last_name: applicant.last_name,
        social_security_number: applicant.ssn,
        date_of_birth: applicant.date_of_birth,
        street_address1: applicant.street_address1,
        city: applicant.city,
        state: applicant.state,
        postal_code: applicant.zip
      }
    }
  end
end
