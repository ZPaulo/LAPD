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
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Support.V4.Content;
using Java.Util;

namespace Smallet.Droid
{
    [Activity(Label = "Smallet", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        bool isGooglePlayServicesInstalled;
        GoogleApiClient apiClient;
        LocationRequest locRequest;
        static readonly string TAG = "X:" + typeof(MainActivity).Name;
        Location currentLocation, oldLocation;
        bool isCalculating;
        public static bool isStill;
        private List<Place> mPlaces;
        private ListView mListView;
        PendingIntent mActivityDetectionPendingIntent;
        Button mRequestActivityUpdatesButton;
        Button mRemoveActivityUpdatesButton;
        private Handler handler;

        public void OnLocationChanged(Location location)
        {
            currentLocation = location;
            if (oldLocation == null)
                oldLocation = currentLocation;
            

            if (!isCalculating)
            {
                isCalculating = true;
                handler.PostDelayed(runnableTimed, 3000);
            }
        }

        private void runnableTimed()
        {
            double distance = Utilities.GetDistance(oldLocation.Latitude, oldLocation.Longitude, currentLocation.Latitude, currentLocation.Longitude);
            if (distance < 15 || isStill)
            {
                Toast.MakeText(this, "Dentro desta dist " + distance, ToastLength.Long).Show();
            }
            else
            {
                oldLocation = currentLocation;
                Toast.MakeText(this, "Fora desta dist " + distance, ToastLength.Long).Show();
            }
            isCalculating = false;
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
            handler = new Handler();
            isCalculating = false;
            isStill = false;
            SetContentView(Resource.Layout.main_activity);

            mRequestActivityUpdatesButton = FindViewById<Button>(Resource.Id.request_activity_updates_button);
            mRemoveActivityUpdatesButton = FindViewById<Button>(Resource.Id.remove_activity_updates_button);

            mRequestActivityUpdatesButton.Click += RequestActivityUpdatesButtonHandler;
            mRemoveActivityUpdatesButton.Click += RemoveActivityUpdatesButtonHandler;


            SetButtonsEnabledState();

            isGooglePlayServicesInstalled = IsGooglePlayServicesInstalled();
            if (isGooglePlayServicesInstalled)
            {
                // pass in the Context, ConnectionListener and ConnectionFailedListener
                apiClient = new GoogleApiClient.Builder(this, this, this)
                    .AddConnectionCallbacks(this)
                    .AddOnConnectionFailedListener(this)
                    .AddApi(LocationServices.API)
                    .AddApi(ActivityRecognition.API)
                    .Build();

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

            //SetContentView(Resource.Layout.Main);

            //mListView = FindViewById<ListView>(Resource.Id.myListView);
            //locationText = FindViewById<TextView>(Resource.Id.locationText);
            //addressText = FindViewById<TextView>(Resource.Id.txtAddress);
            //mPlaces = new List<Place>();
            //mPlaces.Add(new Place() { Time = "Unavailable", Money = "Unavailable", Address = "Unavailable" });

            //ListViewAdapter adapter = new ListViewAdapter(this, mPlaces);
            //mListView.Adapter = adapter;

            //Button button = FindViewById<Button>(Resource.Id.button1);

            //// When the user clicks the button ...
            //button.Click += async (sender, e) =>
            //{

            //    // Get the latitude and longitude entered by the user and create a query.
            //    string url = "http://10.0.2.2:3000/api/locations";

            //    // Fetch the weather information asynchronously, 
            //    // parse the results, then update the screen:
            //    JsonValue json = await FetchWeatherAsync(url);
            //    ParseAndDisplay(json);


            //};
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

            locRequest.SetFastestInterval(1000);
            locRequest.SetInterval(2000);

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
            await Android.Gms.Location.ActivityRecognition.ActivityRecognitionApi.RequestActivityUpdates(
                    apiClient,
                    10000,
                    ActivityDetectionPendingIntent
                );
        }

        public void OnConnectionSuspended(int cause)
        {
            Log.Info("LocationClient", "Now disconnected from client " + cause);
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Log.Info("LocationClient", "Connection failed, attempting to reach google play services");
        }

        public async void RequestActivityUpdatesButtonHandler(object sender, EventArgs e)
        {
            if (!apiClient.IsConnected)
            {
                Toast.MakeText(this, GetString(Resource.String.not_connected),
                    ToastLength.Short).Show();
                return;
            }
            await Android.Gms.Location.ActivityRecognition.ActivityRecognitionApi.RequestActivityUpdates(
                    apiClient,
                    3000,
                    ActivityDetectionPendingIntent
                );
            HandleResult();
        }

        public async void RemoveActivityUpdatesButtonHandler(object sender, EventArgs e)
        {
            if (!apiClient.IsConnected)
            {
                Toast.MakeText(this, GetString(Resource.String.not_connected), ToastLength.Short).Show();
                return;
            }
            await Android.Gms.Location.ActivityRecognition.ActivityRecognitionApi.RemoveActivityUpdates(
                    apiClient,
                    ActivityDetectionPendingIntent
                );
            HandleResult();
        }

        PendingIntent ActivityDetectionPendingIntent
        {
            get
            {
                if (mActivityDetectionPendingIntent != null)
                {
                    return mActivityDetectionPendingIntent;
                }
                var intent = new Intent(this, typeof(DetectedActivitiesIntentService));

                return PendingIntent.GetService(this, 0, intent, PendingIntentFlags.UpdateCurrent);
            }
        }


        public void HandleResult()
        {
            bool requestingUpdates = !UpdatesRequestedState;
            UpdatesRequestedState = requestingUpdates;

            SetButtonsEnabledState();

            Toast.MakeText(
                this,
                GetString(requestingUpdates ? Resource.String.activity_updates_added : Resource.String.activity_updates_removed),
                ToastLength.Short
            ).Show();

        }

        void SetButtonsEnabledState()
        {
            if (UpdatesRequestedState)
            {
                mRequestActivityUpdatesButton.Enabled = false;
                mRemoveActivityUpdatesButton.Enabled = true;
            }
            else
            {
                mRequestActivityUpdatesButton.Enabled = true;
                mRemoveActivityUpdatesButton.Enabled = false;
            }
        }

        ISharedPreferences SharedPreferencesInstance
        {
            get
            {
                return GetSharedPreferences(Constants.SharedPreferencesName, FileCreationMode.Private);
            }
        }

        bool UpdatesRequestedState
        {
            get
            {
                return SharedPreferencesInstance.GetBoolean(Constants.ActivityUpdatesRequestedKey, false);
            }
            set
            {
                SharedPreferencesInstance.Edit().PutBoolean(Constants.ActivityUpdatesRequestedKey, value).Commit();
            }
        }

    }
}

