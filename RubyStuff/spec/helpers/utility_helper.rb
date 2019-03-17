
module UtilityHelper
  def generate_random_email
    val = rand(999_999_999_9).to_s.center(10, rand(9).to_s)
    @generate_email_value = "qa+#{val}@pj.com"
  end

  def get_median(list)
    len = list.length
    @median = ((len - 1) / 2 + len / 2) / 2
  end

  def get_session_id(cookies)
    session_id = ''
    cookies.select do |cookie|
      session_id = cookie.split('=')[1] if cookie.start_with?('_session_id')
    end
    session_id
  end

  def get_csrf_token(response)
    token = response.match('^.*?<meta content="(.+)" name="csrf-token" />.*$')
    token.captures[0]
  end
end
