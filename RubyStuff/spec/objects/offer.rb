# noinspection RubyTooManyInstanceVariablesInspection
class Offer
  attr_reader :type, :origination_fee_rate, :origination_fee, :segment,
              :pricing_tier, :above_prime_max
  attr_accessor :amount, :term_months, :apr, :monthly_payment, :interest_rate

  def initialize(offer)
    @type = offer[:type]
    @amount = offer[:amount]
    @term_months = offer[:term_months]
    @apr = offer[:apr]
    @interest_rate = offer[:interest_rate]
    @origination_fee_rate = offer[:origination_fee_rate]
    @monthly_payment = offer[:monthly_payment]
    @origination_fee = offer[:origination_fee]
    @segment = offer[:segment]
    @pricing_tier = offer[:pricing_tier]
    @above_prime_max = offer[:above_prime_max]
  end
end
