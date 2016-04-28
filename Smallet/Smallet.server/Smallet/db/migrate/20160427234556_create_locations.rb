class CreateLocations < ActiveRecord::Migration
  def change
    create_table :locations do |t|
      t.string :name
      t.string :address
      t.float :latitude
      t.float :longitude
      t.float :money_spent
      t.float :time_spent	
      t.timestamps null: false
    end
  end
end
