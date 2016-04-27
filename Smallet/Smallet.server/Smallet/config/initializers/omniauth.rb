OmniAuth.config.logger = Rails.logger

Rails.application.config.middleware.use OmniAuth::Builder do
  provider :facebook, '264915407185959', '1bc01ce87fae91ce383eb2038bda9928', {:client_options => {:ssl => {:ca_file => Rails.root.join("cacert.pem").to_s}}}
end