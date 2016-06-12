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
using System.Json;

namespace Smallet.Droid
{
    public class ValidatePlace : Fragment
    {
        public ListView mListView;
        public List<Place> listPlaces;
        View  view;
        ViewGroup container;
        LayoutInflater inflater;

        public ValidatePlace(List<Place> listPlaces)
        {
            this.listPlaces = listPlaces;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.container = container;
            base.OnCreateView(inflater, container, savedInstanceState);
            this.inflater = inflater;

            view = inflater.Inflate(Resource.Layout.ValidatePlaceList, container, false);

            mListView = view.FindViewById<ListView>(Resource.Id.ValListView);


            ListViewAdapter adapter = new ListViewAdapter(container.Context, listPlaces);
            mListView.Adapter = adapter;

            return view;
        }
        
    }

    class ViewPlaces : Fragment
    {
        public ListView mListView;
        List<Place> listPlaces;
        LayoutInflater inflater;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.ValidatePlaceList, container, false);

            mListView = view.FindViewById<ListView>(Resource.Id.ValListView);

            GetPlaces();

            return view;
        }

        private async void GetPlaces()
        {
            JsonValue json = await Utilities.GetAllPlaces();

            ParseAndDisplay(json);
        }

        private void ParseAndDisplay(JsonValue json)
        {
            JsonValue places = json;
            listPlaces = new List<Place>();

            foreach (JsonValue item in places)
            {
                listPlaces.Add(new Place() { Validated = true, Name = item["name"],TimeSpent = item["spent_time"] ,Time = item["time"], Money = item["money"], Address = item["address"].ToString() });
            }

            ListViewAdapter adapter = new ListViewAdapter(Application.Context, listPlaces);
            mListView.Adapter = adapter;
        }
    }
}