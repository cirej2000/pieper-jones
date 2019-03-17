require_relative('../helpers/utility_helper')
require 'date'

# used to hold and transfer data from our leads stored in our
# csv data-driven file.
class LeadKBAObject
  attr_reader :loan_amount, :fico_score, :first_name, :last_name,
              :date_of_birth, :house_number, :direction, :street_name,
              :street_type, :apt_no, :city, :state, :zip, :housing_payment,
              :ssn, :income_annual, :final_decision, :pricing_tier,
              :adverse_action1, :adverse_action2, :adverse_action3,
              :adverse_action4, :medium_term, :medium_interest, :medium_apr,
              :medium_payment, :reference_num, :fico_v2_score, :phone,
              :pid_opening_score, :pid_validation_score, :pid_theft_score,
              :pid_fpd_score, :street_address1, :adverse_actions,
              :previous_street_names, :previous_cities, :county_name,
              :open_mortgage, :open_mortgage_payment, :open_auto_loan,
              :open_auto_loan_payment, :open_student_loan,
              :open_student_loan_payment, :closed_mortgage, :closed_auto_loan,
              :closed_student_loan, :vehicle_make_models, :vehicle_years,
              :profession, :education, :business_name, :employer_name,
              :home_value, :year_home_built, :number_of_bedrooms, :offer_code

  include UtilityHelper

  def initialize(records)
    @loan_amount = records[0]
    @fico_score = records[1].nil? ? '' : records[1].chomp
    @first_name = records[2].chomp
    @last_name = records[3].chomp
    # Grab date from our leads record and reformat it
    date = DateTime.parse(records[4].chomp)
    @date_of_birth = date.strftime('%Y-%m-%d')
    @house_number = records[5].nil? ? '' : records[5].chomp
    @direction = records[6].nil? ? '' : records[6].chomp
    @street_name = records[7].nil? ? '' : records[7].chomp
    @street_type = records[8].nil? ? '' : records[8].chomp
    @apt_no = records[9].nil? ? '' : records[9].chomp
    @city = records[10].chomp
    @state = records[11].chomp
    @zip = records[12].chomp
    @housing_payment = records[13].chomp
    @ssn = records[14].chomp
    @income_annual = records[15].chomp
    @final_decision = records[16].chomp
    @pricing_tier = records[17].chomp
    # TODO: can I remove these individual adverse actions?
    @adverse_action1 = records[18].nil? ? '' : records[18].chomp
    @adverse_action2 = records[19].nil? ? '' : records[19].chomp
    @adverse_action3 = records[20].nil? ? '' : records[20].chomp
    @adverse_action4 = records[21].nil? ? '' : records[21].chomp
    @medium_term = records[22].nil? ? '' : records[22].chomp
    @medium_interest = records[23].nil? ? '' : records[23].chomp
    @medium_apr = records[24].chomp
    @medium_payment = records[25].chomp
    @reference_num = records[26].chomp
    @fico_v2_score = records[27].chomp
    @phone = records['PHONE1'].nil? ? '' : records['PHONE1'].chomp
    @pid_opening_score = records['PID Acct Opening V2 Score']
    @pid_validation_score = records['PID Acct Opening V2 Validation Score']
    @pid_theft_score = records['PID Acct Opening ID Theft V2 Score']
    @pid_fpd_score = records['PID Acct Opening FPD V2 Score']
    @decision = records['Decision'].chomp
    @street_address1 = "#{@house_number} #{@direction} #{@street_name}\
                        #{@street_type} #{@apt_no}"
    @adverse_actions = []
    (18..21).each do |num|
      @adverse_actions << records[num].chomp unless records[num].nil?
    end
    @previous_street_names = [records['PREV STREET NAME']]
    (65..69).step(2).each do |name_index|
      unless records[name_index].nil?
        @previous_street_names << records[name_index].chomp
      end
    end
    @previous_cities = []
    (66..70).step(2).each do |city_index|
      unless records[city_index].nil?
        @previous_cities << records[city_index].chomp
      end
    end
    @year_of_birth = records['DOB/YOB'].nil? ? '' : records['DOB/YOB'].chomp
    @open_mortgage = if records['OPEN MORTGAGE'].nil?
                       ''
                     else
                       records['OPEN MORTGAGE'].chomp
                     end
    @open_mortgage_payment = if records['OPEN MTG PYMT'].nil?
                               ''
                             else
                               records['OPEN MTG PYMT'].chomp
                             end
    @closed_mortgage = if records['CLOSED MORTGAGE'].nil?
                         ''
                       else
                         records['CLOSED MORTGAGE'].chomp
                       end
    @open_auto_loan = if records['OPEN AUTO LOAN'].nil?
                        ''
                      else
                        records['OPEN AUTO LOAN'].chomp
                      end
    @open_auto_loan_payment = if records['OPEN AUTO PYMT'].nil?
                                ''
                              else
                                records['OPEN AUTO PYMT'].chomp
                              end
    @closed_auto_loan = if records['CLOSED AUTO LOAN'].nil?
                          ''
                        else
                          records['CLOSED AUTO LOAN'].chomp
                        end
    @open_student_loan = if records['OPEN STUDENT LOAN'].nil?
                           ''
                         else
                           records['OPEN STUDENT LOAN'].chomp
                         end
    @open_student_loan_payment = if records['OPEN STUDENT LOAN PYMT'].nil?
                                   ''
                                 else
                                   records['OPEN STUDENT LOAN PYMT'].chomp
                                 end
    @closed_student_loan = if records['CLOSED STUDENT LOAN'].nil?
                             ''
                           else
                             records['CLOSED STUDENT LOAN'].chomp
                           end
    @vehicle_make_models = []
    (84..90).step(3).each do |make_index|
      unless records[make_index].nil?
        @vehicle_make_models << "#{records[make_index].chomp}\
        #{records[make_index + 1].chomp}"
      end
    end
    @vehicle_years = []
    (86..92).step(3).each do |vehicle_year_index|
      unless records[vehicle_year_index].nil?
        @vehicle_years << records[vehicle_year_index].chomp
      end
    end
    if records['PROFESSION'].nil? || records['PROFESSION'].chomp.empty?
      @profession = 'Salesperson'
    else
      @profession = records['PROFESSION'].chomp
    end
    @education = records['EDUCATION']
    @business_name = records['BUSINESS NAME']
    if records['EMPLOYER NAME'].nil? || records['EMPLOYER NAME'].chomp.empty?
      @employer_name = 'ACME Corp'
    else
      @employer_name = records['EMPLOYER NAME']
    end
    @home_value = if records['HOME VALUE'].nil?
                    ''
                  else
                    records['HOME VALUE'].chomp.to_i
                  end
    @year_home_built = records['YEAR HOME BUILT']
    @number_of_bedrooms = if records['HOME BEDROOM COUNT'].nil?
                            ''
                          else
                            records['HOME BEDROOM COUNT'].chomp.to_i
                          end
    @county_name = if records['COUNTY NAME'].nil?
                     ''
                   else
                     records['COUNTY NAME'].chomp
                   end
    @house_name = records['HOUSEHOLD NAME1']
  end
end
