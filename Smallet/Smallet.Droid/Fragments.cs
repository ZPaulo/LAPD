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
        View view;
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

    class ManualAdd : Fragment
    {
        public ListView mListView;
        List<Place> listPlaces;

        public ManualAdd(List<Place> listPlaces)
        {
            this.listPlaces = listPlaces;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.AddListViewList, container, false);

            mListView = view.FindViewById<ListView>(Resource.Id.addListView);

            AddListViewAdapter adapter = new AddListViewAdapter(container.Context, listPlaces);
            mListView.Adapter = adapter;

            var txtSearch = view.FindViewById<AutoCompleteTextView>(Resource.Id.txtTextSearch);

            txtSearch.TextChanged += async delegate (object sender, Android.Text.TextChangedEventArgs e)
            {
                GetResponse json = await Utilities.SearchPlaces(txtSearch.Text);

                if (json.result == null)
                    Toast.MakeText(Application.Context, json.response, ToastLength.Long).Show();
                else
                    ParseAndDisplay(json.result);
            };

            return view;
        }

        private void ParseAndDisplay(JsonValue json)
        {
            JsonValue places = json["predictions"];


            listPlaces = new List<Place>();

            foreach (JsonValue item in places)
            {
                JsonValue terms = item["terms"];
                string name = terms[0]["value"];
                string address = "";
                for (int i = 1; i < terms.Count; i++)
                {
                    if (i != terms.Count - 1)
                        address += terms[i]["value"] + ", ";
                    else
                        address += terms[i]["value"];
                }
                listPlaces.Add(new Place()
                {
                    Validated = true,
                    Name = name,
                    TimeSpent = "?",
                    Time = "?",
                    Money = "?",
                    Address = address
                });
            }

            AddListViewAdapter adapter = new AddListViewAdapter(Application.Context, listPlaces);
            mListView.Adapter = adapter;
        }
    }

    class ViewPlaces : Fragment
    {
        public ListView mListView;
        List<Place> listPlaces;
        LayoutInflater inflater;
        ViewGroup cont;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            this.cont = container;

            var view = inflater.Inflate(Resource.Layout.ValidatePlaceList, container, false);

            mListView = view.FindViewById<ListView>(Resource.Id.ValListView);

            GetPlaces();

            return view;
        }

        private async void GetPlaces()
        {
            GetResponse json = await Utilities.GetAllPlaces();

            if(json.result == null)
                Toast.MakeText(cont.Context, json.response, ToastLength.Long).Show();
            else
                ParseAndDisplay(json.result);
        }

        private void ParseAndDisplay(JsonValue json)
        {
            JsonValue places = json;
            listPlaces = new List<Place>();

            foreach (JsonValue item in places)
            {
                listPlaces.Add(new Place() { Validated = true, Name = item["name"], TimeSpent = item["spent_time"], Time = item["time"], Money = item["money"], Address = item["address"].ToString() });
            }

            ListViewAdapter adapter = new ListViewAdapter(Application.Context, listPlaces);
            mListView.Adapter = adapter;
        }
    }
}