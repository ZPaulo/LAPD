Rails.application.routes.draw do
  
    get 'auth/:provider/callback', to: 'sessions#create'
    get 'auth/failure', to: redirect('/')
    get 'signout', to: 'sessions#destroy', as: 'signout'

    get "api/locations", :to => 'location#get_locations'
    get "api/location/:id/lat-lon", :to => 'location#get_longitude_latitude'
    get "api/location/:id/name", :to => 'location#get_name'
    get "api/location/:id/address", :to => 'location#get_address'
    get "api/location/info", :to => 'location#get_location_info'
 

    resources :sessions, only: [:create, :destroy]
    resource :home, only: [:show]

    root to: "home#show"
end

