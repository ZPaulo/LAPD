class LocationController < ApplicationController

   def get_locations
     location = Location.all
     if !location.blank?
       render :json => {:success => "true", :location => location}
     else
       render :json => {:success => "false"}
     end
   end

    def get_longitude_latitude
     location = Location.find_by_id params[:id]
     if !location.blank?
       render :json => {:success => "true", :longitude => location.longitude, :latitude => location.latitude}
     else
       render :json => {:success => "false"}
     end
   end
 
   def get_name
     location = Location.find_by_id params[:id]
     if !location.blank?
       render :json => {:success => "true", :location => location.name}
     else
       render :json => {:success => "false"}
     end
   end

	def get_address
     location = Location.find_by_id params[:id]
     if !location.blank?
       render :json => {:success => "true", :location => location.address}
     else
       render :json => {:success => "false"}
     end
   end

   	def get_location_info
     location = Location.find_by_id params[:id]
     if !location.blank?
       render :json => {:success => "true", :time_spent => location.time_spent, :money_spent => location.money_spent, :address => location.address}
     else
       render :json => {:success => "false"}
     end
   end

 end
