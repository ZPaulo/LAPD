using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;
using System.Json;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Smallet.Droid
{
    public static class Utilities
    {
        static string googlePlacesUrl = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?";
        static string googlePacesAuto = "https://maps.googleapis.com/maps/api/place/autocomplete/json?";
        static string googlePlacesKey = "AIzaSyBY3mbMtbMoBVSLPRDAdgxEw0K10PeBEzg";
        static string serverUrl = "https://smartwallet.herokuapp.com/api/";

        public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371; // Radius of the earth in km
            double dLat = deg2rad(lat2 - lat1);  // deg2rad below
            double dLon = deg2rad(lon2 - lon1);
            double a =
              Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
              Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
              ;
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = R * c; // Distance in km
            return d * 1000;
        }

        static double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }

        public static async Task<JsonValue> SearchPlaces(string data)
        {
            string url = googlePacesAuto + "input=" + data + "&key=" + googlePlacesKey;
            System.Diagnostics.Debug.WriteLine(url);
            JsonValue json = await FetchPlacesAsync(url);
            return json;
        }

        public static async Task<JsonValue> GetAllPlaces()
        {
            string url = serverUrl + "place";
            JsonValue json = await FetchPlacesAsync(url);
            return json;
        }

        public static async Task<JsonValue> GetNearbyPlaces(Location loc)
        {
            string latitude = loc.Latitude.ToString().Replace(',','.');
            string longitude = loc.Longitude.ToString().Replace(',', '.');
            string url = googlePlacesUrl + "location=" + latitude + "," + longitude + "&radius=" + 50 + "&key=" + googlePlacesKey;
            JsonValue json = await FetchPlacesAsync(url);
            return json;
        }

        static private async Task<JsonValue> FetchPlacesAsync(string url)
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

        public static async void PostPlace(Place place)
        {
            string latitude = place.Latitude.Replace(',', '.');
            string longitude = place.Longitude.Replace(',', '.');
            string data = "{\"name\":\"" + place.Name + "\",\"address\":" + place.Address + ",\"latitude\":" + latitude + ",\"longitude\":" + longitude + ",\"money\":" + place.Money + ",\"iduser\":" + 1 + ",\"spent_time\":\"" + place.TimeSpent + "\",\"time\":\"" + place.Time + "\"}";
            //var postData = new List<KeyValuePair<string, string>>();
            //postData.Add(new KeyValuePair<string, string>("name", place.Name));
            //postData.Add(new KeyValuePair<string, string>("address", place.Address));
            //postData.Add(new KeyValuePair<string, string>("latitude", place.Latitude));
            //postData.Add(new KeyValuePair<string, string>("longitude", place.Longitude));
            //postData.Add(new KeyValuePair<string, string>("money", place.Money));
            //postData.Add(new KeyValuePair<string, string>("iduser", "1"));
            //postData.Add(new KeyValuePair<string, string>("spent_time", place.Time));
            //postData.Add(new KeyValuePair<string, string>("time", "0"));

            System.Diagnostics.Debug.WriteLine(data);
            string response = await MakePostRequest(data);
            System.Diagnostics.Debug.WriteLine("asdasdasd "+response);

        }

        public static async Task<string> MakePostRequest(string data/*, string cookie*/, bool isJson = true)
        {
            string url = serverUrl + "place";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json";

            request.Method = "POST";
            //request.Headers["Cookie"] = cookie;
            var stream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(data);
                writer.Flush();
                writer.Dispose();
            }
                var response = await request.GetResponseAsync();
                var respStream = response.GetResponseStream();
                using (StreamReader sr = new StreamReader(respStream))
                {
                    //Need to return this response 
                    return sr.ReadToEnd();
                }
        }
    }
}