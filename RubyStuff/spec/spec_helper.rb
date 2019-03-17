require 'csv'
require 'yaml'
require 'rspec'
require 'rspec/core'
require 'rspec/retry'
require 'rspec_junit_formatter'
require 'logger'
require 'bigdecimal'
require 'bigdecimal/util'
require_relative('helpers/leads_api_helper')
require 'airborne'

RSpec.configure do |c|
  c.output_stream = 'test-reports/specs_junit.xml'
  c.formatter = RspecJunitFormatter
  c.include LeadsApiHelper
end
