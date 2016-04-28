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
using System.Net;
using System.IO;
using System.Json;
using Newtonsoft.Json;

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
            mPlaces.Add(new Place() { Time = "Unavailable", Money = "Unavailable", Address = "Unavailable" });

            ListViewAdapter adapter = new ListViewAdapter(this, mPlaces);
            mListView.Adapter = adapter;

            InitializeLocationManager();

            Button button = FindViewById<Button>(Resource.Id.button1);

            // When the user clicks the button ...
            button.Click += async (sender, e) => {

                // Get the latitude and longitude entered by the user and create a query.
                string url = "http://10.0.2.2:3000/api/locations";

                // Fetch the weather information asynchronously, 
                // parse the results, then update the screen:
                JsonValue json = await FetchWeatherAsync(url);
                ParseAndDisplay(json);


            };
        }

        private void ParseAndDisplay(JsonValue json)
        {
            // Extract the array of name/value results for the field name "weatherObservation". 
            JsonValue location = json["location"];
            mPlaces = new List<Place>();
            TimeSpan span;
            string timeSpent;

            foreach (JsonValue item in location)
            {
                span = TimeSpan.FromMinutes(item["time_spent"]);
                timeSpent = span.ToString(@"hh\:mm\:ss");

                mPlaces.Add(new Place() { Time = timeSpent, Money = "-"+item["money_spent"].ToString() + "€", Address = item["address"].ToString() });
            }

            ListViewAdapter adapter = new ListViewAdapter(this, mPlaces);
            mListView.Adapter = adapter;
        }

        internal class SMLocation
        {
           public double money_spent { get; set; }
           public double time_spent { get; set; }
           public string address { get; set; }
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

        private async Task<JsonValue> FetchWeatherAsync(string url)
        {
            // Create an HTTP web request using the URL:
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            // Send the request to the server and wait for the response:
            using (WebResponse response = await request.GetResponseAsync())
            {
                // Get a stream representation of the HTTP web response:
                using (Stream stream = response.GetResponseStream())
                {
                    // Use this stream to build a JSON document object:
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    Console.Out.WriteLine("Response: {0}", jsonDoc.ToString());

                    // Return the JSON document:
                    return jsonDoc;
                }
            }
        }



    }
}

