using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;
using Newtonsoft.Json.Linq;
using Org.Json;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Smallet.Droid;
using Android.Widget;
using System.Json;

namespace Smallet.Droid
{
    [Activity(Label = "Login")]
    public class LoginActivity : Activity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback
    {
        bool succesfull;
        private ICallbackManager callBackManager;
        public static string token;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            FacebookSdk.SdkInitialize(this.ApplicationContext);

            // Set our view from the "main" layout resource

            callBackManager = CallbackManagerFactory.Create();

            SetContentView(Resource.Layout.Login);
            LoginButton button = FindViewById<LoginButton>(Resource.Id.login_button);

            button.SetReadPermissions("public_profile");
            button.RegisterCallback(callBackManager, this);

            try
            {
                if (AccessToken.CurrentAccessToken != null)
                {
                    JsonValue json = await Utilities.GetUser(AccessToken.CurrentAccessToken.Token);
                    AppActivity.userID = Int32.Parse(json["id"]);
                    System.Diagnostics.Debug.WriteLine(AppActivity.userID);

                    Intent intent = new Intent(this, typeof(AppActivity));
                    StartActivity(intent);
                }
                else
                    System.Diagnostics.Debug.WriteLine("Goodbye");
            }
            catch (System.Exception)
            {
                System.Diagnostics.Debug.WriteLine("Goodbye"); ;
            }
            

        }

        public void OnCancel()
        {
            //throw new NotImplementedException();
        }

        public void OnError(FacebookException p0)
        {
            //throw new NotImplementedException();
        }

        public void OnSuccess(Java.Lang.Object p0)
        {
            LoginResult loginResult = p0 as LoginResult;
            token = loginResult.AccessToken.Token;
            Console.WriteLine(token);

            GraphRequest request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);

            Bundle parameters = new Bundle();
            parameters.PutString("fields", "id,name");
            request.Parameters = parameters;
            request.ExecuteAsync();

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            callBackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        public void OnCompleted(JSONObject json, GraphResponse response)
        {
            string data = json.ToString();
            Console.WriteLine(data);
            succesfull = false;
            JToken token = JObject.Parse(data);
            string name = (string)token.SelectToken("name");
            string id = (string)token.SelectToken("id");
            Console.WriteLine("NAME:" + name);
            Console.WriteLine("ID:" + id);

            Console.WriteLine("Enviando User do the database...");
            Task.WaitAll(PostUser(name, id));

            if (succesfull)
            {
                Intent intent = new Intent(this, typeof(AppActivity));
                StartActivity(intent);
            }

        }

        public async Task PostUser(string name, string id)
        {
            // There are 2 ways to use await.  Here is the short way.
            string json = await RequestUserAsync(name, id);
            Console.WriteLine(json);

            int startPos = 6;
            try
            {
                int length = json.IndexOf("}") - startPos;
                string sub = json.Substring(startPos, length);
                AppActivity.userID = Int32.Parse(sub);
                Console.WriteLine(AppActivity.userID);
                succesfull = true;
            }
            catch (System.Exception)
            {
                Toast.MakeText(Application.Context, "Couldn't login with server", ToastLength.Long).Show();
            }
        }

        public static async Task<string> RequestUserAsync(string name, string id)
        {

            //var accept = "application/json";
            Uri uri = new Uri("https://smartwallet.herokuapp.com/api/users");
            //var auth = "ApiKey humma-numma-na:letters-and-numbers";

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(uri);

            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                string json = "{\"name\":\"" + name + "\",\"facebook_token\":\"" + token + "\",\"facebook_id\":\"" + id + "\"}";


                streamWriter.Write(json);
                streamWriter.Flush();
            }


            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
        }
    }




}



