using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;

namespace Smallet.Droid
{
    [Activity(Label = "Smallet", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
		private List<Place> mPlaces;
		private ListView mListView;

        protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			ActionBar.SetDisplayShowHomeEnabled (false);
			ActionBar.SetDisplayShowTitleEnabled (false);
			ActionBar.SetCustomView (Resource.Layout.ActionBar);
			ActionBar.SetDisplayShowCustomEnabled (true);

			SetContentView (Resource.Layout.Main);

			mListView = FindViewById<ListView> (Resource.Id.myListView);

			mPlaces = new List<Place> ();
			mPlaces.Add (new Place(){Time="5 PM", Money ="- 50€", Address= "Rua das Nogueiras"});
			mPlaces.Add (new Place(){Time="5 PM", Money ="- 50€", Address= "Rua das Nogueiras"});
			mPlaces.Add (new Place(){Time="5 PM", Money ="- 50€", Address= "Rua das Nogueiras"});

			ListViewAdapter adapter = new ListViewAdapter (this, mPlaces);
			mListView.Adapter = adapter;

		}


    }
}

