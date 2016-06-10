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
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.Gms.Location;

namespace Smallet.Droid
{
    [Activity(Label = "Smallet", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        bool isGooglePlayServicesInstalled;
        GoogleApiClient apiClient;
        LocationRequest locRequest;
        static readonly string TAG = "X:" + typeof(MainActivity).Name;
        TextView addressText;
        Location currentLocation;
        TextView locationText;
        private List<Place> mPlaces;
        private ListView mListView;

        public void OnLocationChanged(Location location)
        {
            currentLocation = location;
            if (currentLocation == null)
            {
                locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                locationText.Text = string.Format("{0:f6},{1:f6}", currentLocation.Latitude, currentLocation.Longitude);
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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            isGooglePlayServicesInstalled = IsGooglePlayServicesInstalled();
            if (isGooglePlayServicesInstalled)
            {
                // pass in the Context, ConnectionListener and ConnectionFailedListener
                apiClient = new GoogleApiClient.Builder(this, this, this).AddApi(LocationServices.API).Build();

                // generate a location request that we will pass into a call for location updates
                locRequest = new LocationRequest();

            }
            else
            {
                Log.Error("OnCreate", "Google Play Services is not installed");
                Toast.MakeText(this, "Google Play Services is not installed", ToastLength.Long).Show();
                Finish();
            }

            // Set our view from the "main" layout resource
            ActionBar.SetDisplayShowHomeEnabled(false);
            ActionBar.SetDisplayShowTitleEnabled(false);
            ActionBar.SetCustomView(Resource.Layout.ActionBar);
            ActionBar.SetDisplayShowCustomEnabled(true);

            SetContentView(Resource.Layout.Main);

            mListView = FindViewById<ListView>(Resource.Id.myListView);
            locationText = FindViewById<TextView>(Resource.Id.locationText);
            addressText = FindViewById<TextView>(Resource.Id.txtAddress);
            mPlaces = new List<Place>();
            mPlaces.Add(new Place() { Time = "Unavailable", Money = "Unavailable", Address = "Unavailable" });

            ListViewAdapter adapter = new ListViewAdapter(this, mPlaces);
            mListView.Adapter = adapter;

            Button button = FindViewById<Button>(Resource.Id.button1);

            // When the user clicks the button ...
            button.Click += async (sender, e) =>
            {

                // Get the latitude and longitude entered by the user and create a query.
                string url = "http://10.0.2.2:3000/api/locations";

                // Fetch the weather information asynchronously, 
                // parse the results, then update the screen:
                JsonValue json = await FetchWeatherAsync(url);
                ParseAndDisplay(json);


            };
        }

        bool IsGooglePlayServicesInstalled()
        {
            int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                Log.Info("MainActivity", "Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                string errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error("ManActivity", "There is a problem with Google Play Services on this device: {0} - {1}", queryResult, errorString);

                // Show error dialog to let user debug google play services
            }
            return false;
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

                mPlaces.Add(new Place() { Time = timeSpent, Money = "-" + item["money_spent"].ToString() + "€", Address = item["address"].ToString() });
            }

            ListViewAdapter adapter = new ListViewAdapter(this, mPlaces);
            mListView.Adapter = adapter;
        }

        protected override void OnResume()
        {
            base.OnResume();
            apiClient.Connect();

            locRequest.SetPriority(100);

            locRequest.SetFastestInterval(500);
            locRequest.SetInterval(1000);

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

        public async void OnConnected(Bundle connectionHint)
        {
            await LocationServices.FusedLocationApi.RequestLocationUpdates(apiClient, locRequest, this);
            Log.Info("LocationClient", "Now connected to client");
        }

        public void OnConnectionSuspended(int cause)
        {
            Log.Info("LocationClient", "Now disconnected from client " + cause);
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Log.Info("LocationClient", "Connection failed, attempting to reach google play services");
        }
    }
}

