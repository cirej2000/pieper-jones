
module KbaHelper
  def get_question_type_and_array(kba_response, question_number)
    question = kba_response[:questions]

    return question[question_number][:question_type_id], question[question_number][:answer_choices]
  end

  # get the matched answers from each of the questions returned, build a list of
  # the question numbers (5 if no match)
  def ret_answers_list(kba_response, applicant)
    questions = kba_response[:questions]
    answers = []
    questions.each do |question|
      answers << find_answer(question, applicant).to_s
    end

    answers
  end

  # if we have a question type that maps to a list-based entity
  # (vehicle, previous street name, previous cities)
  def find_answer(question, applicant)
    # list of choices from the given question
    question_type = question[:question_type_id]
    answer_choices = question[:answer_choices]
    # this our 1 to N sized array matching the columns from our csv
    # to the question type
    columns = get_columns_by_question_type(question_type, applicant)
    return 5 if columns.nil?
    # we start with the assumption that we don't have a match
    answer_pick = 5
    # go through each of the answer choices until we hit a match or exhaust list
    answer_choices.take(4).each do |answer|
      if find_answer_by_column(answer, columns, question_type)
        answer_pick = answer[:id]
      end unless answer_pick != 5
    end

    answer_pick
  end

  # compare the answer value to the value in our column,
  # if a match return the true, otherwise return false
  def find_answer_by_column(answer, columns, question_type)
    match = false
    columns.each do |column|
      unless match || column.blank?
        # Columns 2, 4 and 6 contain int ranges so we need to pull the high and low
        if [2, 4, 6].include?(question_type.to_i)
          high, low = strip_amount_ranges(answer[:answer_text])
          # Then check to see if our target column is between these values.
          match = true unless column.to_i < low || column.to_i > high
          return match
        end

        if answer[:answer_text].to_s.include?(column.to_s) || \
           column.to_s.include?(answer[:answer_text].to_s)
          match = true
          return match
        end
      end
    end

    match
  end

  # Clean up our question text and get a high and low range for comparison
  def strip_amount_ranges(answer_text)
    match = /\$(\d*)\s-\s\$(\d*)/.match(answer_text)
    low = match[1].to_i
    high = match[2].to_i

    return high, low
  end

  # match our column or range of columns based on question type
  def get_columns_by_question_type(question_type, applicant)
    case question_type.to_i
    when 1 then [*applicant.open_mortgage]
    when 2 then [*applicant.open_mortgage_payment]
    when 3 then [*applicant.open_auto_loan]
    when 4 then [*applicant.open_auto_loan_payment]
    when 5 then [*applicant.open_student_loan]
    when 6 then [*applicant.open_student_loan_payment]
      # The following two are already arrays
    when 7 then applicant.previous_street_names
    when 8 then applicant.previous_cities
    when 12 then [*applicant.year_of_birth]
    when 13 then [*applicant.county_name]
    when 20 then [*applicant.household_name]
    when 21 then [*applicant.year_home_built]
    when 22 then [*applicant.home_value]
    when 23 then [*applicant.employer_name]
    when 24 then [*applicant.profession]
    when 25 then [*applicant.business_name]
    when 26 then [*applicant.education]
      # The next two are, again, arrays already
    when 27 then applicant.vehicle_make_models
    when 28 then applicant.vehicle_years
    when 41 then [*applicant.bedroom_count]
    when 101 then [*applicant.closed_mortgage]
    when 102 then [*applicant.closed_auto_loan]
    when 103 then [*applicant.closed_student_loan]
    end
  end

  def get_wrong_answers(answers, number_wrong)
    answers.take(number_wrong).each do |answer|
      wrong_answers << get_wrong_answer(answer)
    end
    (number_wrong..answers.size - 1).each do |index|
      wrong_answers << answers[index]
    end
  end

  def get_wrong_answer(answer_id)
    if answer_id == 5
      id = answer_id - 1
    else
      id = answer_id + 1
    end
    id
  end
end
