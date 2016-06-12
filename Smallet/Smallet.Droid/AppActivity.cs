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
using System.Diagnostics;

namespace Smallet.Droid
{
    [Activity(Label = "SmalletActivity")]
    public class AppActivity : Activity, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        bool isGooglePlayServicesInstalled;
        GoogleApiClient apiClient;
        LocationRequest locRequest;
        static readonly string TAG = "X:" + typeof(AppActivity).Name;
        Location currentLocation, oldLocation;
        bool isCalculating, isStillRequest;
        View popup;
        public static bool isStill;
        public static List<Place> mPlaces;
        PendingIntent mActivityDetectionPendingIntent;
        ValidatePlace valPlaceFragment;
        AlertDialog alert;
        private Handler handler;
        View clickedPlace;
        Stopwatch stop;
        string currentTime;
        TimeSpan elapsedTime;
        int timeIntervalStop;
        List<Place> aePlaces;

        public void OnLocationChanged(Location location)
        {
            currentLocation = location;
            if (oldLocation == null)
                oldLocation = currentLocation;


            if (!isCalculating)
            {
                isCalculating = true;
                handler.PostDelayed(runnableTimed, timeIntervalStop);
            }
        }

        private async void runnableTimed()
        {
            double distance = Utilities.GetDistance(oldLocation.Latitude, oldLocation.Longitude, currentLocation.Latitude, currentLocation.Longitude);
            if (distance < 2 /*|| isStill*/)
            {
                if (!isStillRequest)
                {
                    DateTime now = DateTime.Now.ToLocalTime();
                    stop = new Stopwatch();
                    stop.Reset();
                    stop.Start();
                    currentTime = (string.Format("{0}", now));
                    isStillRequest = true;
                }
                Toast.MakeText(this, "Dentro desta dist " + distance, ToastLength.Long).Show();
            }
            else
            {
                if (isStillRequest)
                {
                    Toast.MakeText(this, "Apareci " + distance, ToastLength.Long).Show();
                    elapsedTime = stop.Elapsed;
                    stop.Stop();
                    GetResponse json = await Utilities.GetNearbyPlaces(oldLocation);

                    if (json.result == null)
                        Toast.MakeText(this, json.response, ToastLength.Long).Show();
                    else
                        ParseAndDisplay(json.result, oldLocation, currentTime);
                }
                isStillRequest = false;
                oldLocation = currentLocation;
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
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            
            handler = new Handler();
            isCalculating = false;
            isStill = false;
            isStillRequest = false;
            timeIntervalStop = 3000;

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



            mPlaces = new List<Place>();
            mPlaces.Add(new Place() { Time = "?", TimeSpent = "?", Name = "No Places yet", Address = "?", Money = "?" });


            valPlaceFragment = new ValidatePlace(mPlaces);
            AddTabToActionBar("Validate Places", 0, valPlaceFragment);
            AddTabToActionBar("View Places", 0, new ViewPlaces());
            AddTabToActionBar("Add Place", 0, new ManualAdd(mPlaces));


            SetContentView(Resource.Layout.Main);

        }


        void AddTabToActionBar(string labelResource, int iconResourceId, Fragment view)
        {
            var tab = this.ActionBar.NewTab();
            tab.SetText(labelResource);

            tab.TabSelected += delegate (object sender, ActionBar.TabEventArgs e)
            {
                var fragment = this.FragmentManager.FindFragmentById(Resource.Id.fragmentContainer);
                if (fragment != null)
                    e.FragmentTransaction.Remove(fragment);
                e.FragmentTransaction.Add(Resource.Id.fragmentContainer, view);
            };
            tab.TabUnselected += delegate (object sender, ActionBar.TabEventArgs e)
            {
                e.FragmentTransaction.Remove(view);
            };

            this.ActionBar.AddTab(tab);
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

        private void ParseAndDisplay(JsonValue json, Location loc, string currentTime)
        {
            JsonValue places = json["results"];
            mPlaces = new List<Place>();
            int seconds = timeIntervalStop / 1000;
            elapsedTime = elapsedTime.Add(new TimeSpan(0, 0, seconds));

            var timeSpent = elapsedTime.ToString(@"hh\:mm\:ss");

            foreach (JsonValue item in places)
            {
                mPlaces.Add(new Place() { Time = currentTime, Validated = false, Latitude = loc.Latitude.ToString(), Longitude = loc.Longitude.ToString(), Name = item["name"], TimeSpent = timeSpent, Money = "?", Address = item["vicinity"].ToString() });
            }

            valPlaceFragment.listPlaces = mPlaces;
            ListViewAdapter adapter = new ListViewAdapter(this, mPlaces);
            valPlaceFragment.mListView.Adapter = adapter;
        }

        public void AddValidationForm(View clickedPlace)
        {
            if (popup == null)
            {
                this.clickedPlace = clickedPlace;
                AlertDialog.Builder alertb = new AlertDialog.Builder(this);

                LayoutInflater inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
                popup = inflater.Inflate(Resource.Layout.ValidatePlace, null);

                var txtTime = clickedPlace.FindViewById<TextView>(Resource.Id.txtTimeSpent);
                var timeText = popup.FindViewById<EditText>(Resource.Id.editTextTime);
                timeText.Text = txtTime.Text;

                alertb.SetView(popup);

                alertb.SetTitle("Confirm place");
                alert = alertb.Create();

                Button button = popup.FindViewById<Button>(Resource.Id.buttonValConfirm);
                button.Click += ValConfirm_Click;
                alert.Show();
            }
            else
                popup = null;
        }

        private async void ValConfirm_Click(object sender, EventArgs e)
        {
            EditText timeText;
            EditText moneyText;
            try
            {
                timeText = popup.FindViewById<EditText>(Resource.Id.editTextTime);
                moneyText = popup.FindViewById<EditText>(Resource.Id.editTextMoney);
            }
            catch (Exception)
            {
                timeText = null;
                moneyText = null;
            }

            if (timeText.Text == null || timeText.Text == "" || moneyText.Text == null || moneyText.Text == "")
                Toast.MakeText(this, "Please fill in all fields", ToastLength.Short).Show();
            else
            {
                // ViewGroup lp1 = (ViewGroup)clickedPlace.Parent;
                // ViewGroup outView = (ViewGroup)clickedPlace.GetChildAt(0);
                var txtTime = clickedPlace.FindViewById<TextView>(Resource.Id.txtTimeSpent);
                var txtMoney = clickedPlace.FindViewById<TextView>(Resource.Id.txtMoneySpent);

                // outView = (ViewGroup)lp1.GetChildAt(1);
                var txtName = clickedPlace.FindViewById<TextView>(Resource.Id.txtName);
                var txtAddress = clickedPlace.FindViewById<TextView>(Resource.Id.txtAddress);

                if (txtTime != null && txtMoney != null)
                {
                    txtTime.Text = timeText.Text;
                    txtMoney.Text = moneyText.Text;
                }

                txtTime.Invalidate();
                txtMoney.Invalidate();

                string response = "";
                foreach (var place in mPlaces)
                {
                    if (!place.Validated)
                        if (txtAddress.Text == place.Address && txtName.Text == place.Name)
                        {
                            place.TimeSpent = timeText.Text;
                            place.Money = moneyText.Text;
                            place.Validated = true;
                            response = await Utilities.PostPlace(place);
                            break;
                        }
                }
                Toast.MakeText(this, response, ToastLength.Long).Show();

                //lp1.RemoveView(layout);
                alert.Hide();
            }
        }

        public void ManAddValidationForm(View clickedPlace, List<Place> places)
        {
            this.aePlaces = places;
            if (popup == null)
            {
                this.clickedPlace = clickedPlace;
                AlertDialog.Builder alertb = new AlertDialog.Builder(this);

                LayoutInflater inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
                popup = inflater.Inflate(Resource.Layout.ValidatePlace, null);

                var txtTime = clickedPlace.FindViewById<TextView>(Resource.Id.txtTimeSpent);
                var timeText = popup.FindViewById<EditText>(Resource.Id.editTextTime);
                timeText.Text = txtTime.Text;

                alertb.SetView(popup);

                alertb.SetTitle("Confirm place");
                alert = alertb.Create();

                Button button = popup.FindViewById<Button>(Resource.Id.buttonValConfirm);
                button.Click += ManValConfirm_Click;
                alert.Show();
            }
            else
                popup = null;
        }

        private async void ManValConfirm_Click(object sender, EventArgs e)
        {
            EditText timeText;
            EditText moneyText;
            try
            {
                timeText = popup.FindViewById<EditText>(Resource.Id.editTextTime);
                moneyText = popup.FindViewById<EditText>(Resource.Id.editTextMoney);
            }
            catch (Exception)
            {
                timeText = null;
                moneyText = null;
            }

            if (timeText.Text == null || timeText.Text == "" || moneyText.Text == null || moneyText.Text == "")
                Toast.MakeText(this, "Please fill in all fields", ToastLength.Short).Show();
            else
            {
                // ViewGroup lp1 = (ViewGroup)clickedPlace.Parent;
                // ViewGroup outView = (ViewGroup)clickedPlace.GetChildAt(0);
                var txtTime = clickedPlace.FindViewById<TextView>(Resource.Id.txtTimeSpent);
                var txtMoney = clickedPlace.FindViewById<TextView>(Resource.Id.txtMoneySpent);

                // outView = (ViewGroup)lp1.GetChildAt(1);
                var txtName = clickedPlace.FindViewById<TextView>(Resource.Id.txtName);
                var txtAddress = clickedPlace.FindViewById<TextView>(Resource.Id.txtAddress);

                if (txtTime != null && txtMoney != null)
                {
                    txtTime.Text = timeText.Text;
                    txtMoney.Text = moneyText.Text;
                }

                txtTime.Invalidate();
                txtMoney.Invalidate();

                string response = "";
                foreach (var place in aePlaces)
                {
                    if (!place.Validated)
                        if (txtAddress.Text == place.Address && txtName.Text == place.Name)
                        {
                            place.TimeSpent = timeText.Text;
                            place.Money = moneyText.Text;
                            place.Longitude = "0.0";
                            place.Latitude = "0.0";
                            place.Address = "\"" + place.Address + "\"";
                            place.Time = "00:00:00";
                            place.Validated = true;
                            response = await Utilities.PostPlace(place);
                            break;
                        }
                }
                Toast.MakeText(this, response, ToastLength.Long).Show();

                //lp1.RemoveView(layout);
                alert.Hide();
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

