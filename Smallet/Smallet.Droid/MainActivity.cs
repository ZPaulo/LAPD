using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Locations;
using System.Collections.Generic;
using Android.Util;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace Smallet.Droid
{
    [Activity(Label = "Smallet", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener
    {
        public async void OnLocationChanged(Location location)
        {
            currentLocation = location;
            if (currentLocation == null)
            {
                locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                locationText.Text = string.Format("{0:f6},{1:f6}", currentLocation.Latitude, currentLocation.Longitude);
                Address address = await ReverseGeocodeCurrentLocation();
                DisplayAddress(address);
            }
        }

        public void OnProviderDisabled(string provider)
        {
            throw new NotImplementedException();
        }

        public void OnProviderEnabled(string provider)
        {
            throw new NotImplementedException();
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            throw new NotImplementedException();
        }

        static readonly string TAG = "X:" + typeof(MainActivity).Name;
        TextView addressText;
        Location currentLocation;
        LocationManager locationManager;

        string locationProvider;
        TextView locationText;
        private List<Place> mPlaces;
        private ListView mListView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            ActionBar.SetDisplayShowHomeEnabled(false);
            ActionBar.SetDisplayShowTitleEnabled(false);
            ActionBar.SetCustomView(Resource.Layout.ActionBar);
            ActionBar.SetDisplayShowCustomEnabled(true);

            SetContentView(Resource.Layout.Main);

            mListView = FindViewById<ListView>(Resource.Id.myListView);

            mPlaces = new List<Place>();
            mPlaces.Add(new Place() { Time = "5 PM", Money = "- 50€", Address = "Rua das Nogueiras" });
            mPlaces.Add(new Place() { Time = "5 PM", Money = "- 50€", Address = "Rua das Nogueiras" });
            mPlaces.Add(new Place() { Time = "5 PM", Money = "- 50€", Address = "Rua das Nogueiras" });

            ListViewAdapter adapter = new ListViewAdapter(this, mPlaces);
            mListView.Adapter = adapter;

            InitializeLocationManager();
        }


        private void InitializeLocationManager()
        {
            locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                locationProvider = string.Empty;
            }
            Log.Debug(TAG, "Using " + locationProvider + ".");
        }

        protected override void OnResume()
        {
            base.OnResume();
            locationManager.RequestLocationUpdates(locationProvider, 2000, 1, this);
        }

        async void AddressButton_OnClick(object sender, EventArgs eventArgs)
        {
            if (currentLocation == null)
            {
                addressText.Text = "Can't determine the current address. Try again in a few minutes.";
                return;
            }

            Address address = await ReverseGeocodeCurrentLocation();
            DisplayAddress(address);
        }

        async Task<Address> ReverseGeocodeCurrentLocation()
        {
            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList =
                await geocoder.GetFromLocationAsync(currentLocation.Latitude, currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            return address;
        }

        void DisplayAddress(Address address)
        {
            if (address != null)
            {
                StringBuilder deviceAddress = new StringBuilder();
                for (int i = 0; i < address.MaxAddressLineIndex; i++)
                {
                    deviceAddress.AppendLine(address.GetAddressLine(i));
                }
                // Remove the last comma from the end of the address.
                addressText.Text = deviceAddress.ToString();
            }
            else
            {
                addressText.Text = "Unable to determine the address. Try again in a few minutes.";
            }
        }



    }
}

