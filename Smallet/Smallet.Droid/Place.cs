using System;
using System.Json;

namespace Smallet.Droid
{
	public class Place
	{
		public string TimeSpent{ get; set;}
		public string Money{ get; set;}
		public string Address{ get; set;}
        public string Name { get; set; }
        public bool Validated { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Time { get; set; }
        public Place ()
		{
		}
	}


    public class GetResponse
    {
        public GetResponse(JsonValue result, string response)
        {
            this.result = result;
            this.response = response;
        }
        public JsonValue result { get; set; }
        public string response { get; set; }
    }
}

