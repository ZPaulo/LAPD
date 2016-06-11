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
        View popup;
        public static bool isStill;
        public static List<Place> mPlaces;
        private ListView mListView;
        PendingIntent mActivityDetectionPendingIntent;
        AlertDialog alert;
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

        private async void runnableTimed()
        {
            double distance = Utilities.GetDistance(oldLocation.Latitude, oldLocation.Longitude, currentLocation.Latitude, currentLocation.Longitude);
            if (distance < 15 || isStill)
            {
                JsonValue json = await Utilities.GetNearbyPlaces(currentLocation);
                ParseAndDisplay(json);
                Toast.MakeText(this, "Dentro desta dist " + distance, ToastLength.Long).Show();
            }
            else
            {
                oldLocation = currentLocation;
                Toast.MakeText(this, "Fora desta dist " + distance, ToastLength.Long).Show();
            }
            //isCalculating = false;
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

            SetContentView(Resource.Layout.Main);

            mListView = FindViewById<ListView>(Resource.Id.myListView);
            mPlaces = new List<Place>();
            mPlaces.Add(new Place() { Time = "Unavailable", Money = "Unavailable", Address = "Unavailable" });

            ListViewAdapter adapter = new ListViewAdapter(this, mPlaces);
            mListView.Adapter = adapter;
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
            JsonValue places = json["results"];
            mPlaces = new List<Place>();
            TimeSpan span;
            string timeSpent;

            foreach (JsonValue item in places)
            {
                //span = TimeSpan.FromMinutes(item["time_spent"]);
                //timeSpent = span.ToString(@"hh\:mm\:ss");

                mPlaces.Add(new Place() {Name = item["name"],Time ="Unknown", Money = "Unknown", Address = item["vicinity"].ToString() });
            }

            ListViewAdapter adapter = new ListViewAdapter(this, mPlaces);
            mListView.Adapter = adapter;
        }

        [Java.Interop.Export("OnClick")]
        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.buttonVal:
                    AlertDialog.Builder alertb = new AlertDialog.Builder(this);

                    LayoutInflater inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
                    popup = inflater.Inflate(Resource.Layout.ValidatePlace, null);

                    alertb.SetTitle("Confirm place");
                    alertb.SetView(popup);
                    alert = alertb.Create();

                    alert.Show();
                    break;
                case Resource.Id.buttonRej:
                    break;
                case Resource.Id.buttonValConfirm:
                    var timeText = popup.FindViewById<EditText>(Resource.Id.editTextMoney);
                    var moneyText = popup.FindViewById<EditText>(Resource.Id.editTextMoney);
                    if (timeText.Text == null || timeText.Text == "" || moneyText.Text == null || moneyText.Text == "")
                        Toast.MakeText(this, "Please fill in all fields", ToastLength.Short).Show();
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Fazer post com " + timeText.Text + " " + moneyText.Text);
                        ViewGroup layout = (ViewGroup)v.Parent;
                        var txtTime = layout.FindViewById<TextView>(Resource.Id.txtTime);
                        var txtMoney = layout.FindViewById<TextView>(Resource.Id.txtMoneySpent);

                        if (txtTime != null && txtMoney != null)
                        {
                            txtTime.Text = timeText.Text;
                            txtMoney.Text = moneyText.Text;
                        }


                        var button = layout.FindViewById<Button>(Resource.Id.buttonRej);
                        layout.RemoveView(button);
                        layout.Invalidate();
                        alert.Hide();
                    }
                    break;
                default:
                    break;
            }
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
                    60000,
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
    }
}

