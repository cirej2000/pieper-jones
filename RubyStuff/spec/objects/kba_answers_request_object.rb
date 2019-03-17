
class KbaAnswersRequestObject
  attr_reader :kba_answers

  def initialize(kba_session_id, answers)
    @kba_answers = "{\"kba_session_id\":\"#{kba_session_id}\",\
                  \"answer_ids\":#{answers}}"
  end
end
