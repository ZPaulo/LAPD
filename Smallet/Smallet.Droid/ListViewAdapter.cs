﻿using System;
using Android.Widget;
using System.Collections.Generic;
using System.Runtime;
using System.Text;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Content;


namespace Smallet.Droid
{
    public class ListViewAdapter : BaseAdapter<Place>
    {
        public List<Place> mItems;
        private Context mContext;
        View row, popup;
        ViewGroup rowParent;

        public ListViewAdapter(Context context, List<Place> items)
        {
            mItems = items;
            mContext = context;
        }

        public override int Count
        {
            get
            {
                return mItems.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Place this[int position]
        {
            get { return mItems[position]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            row = convertView;
            rowParent = parent;
            if (row == null)
            {
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.ListViewRow, null, false);
            }
            TextView txtTime = row.FindViewById<TextView>(Resource.Id.txtTimeDate);
            txtTime.Text = mItems[position].Time;

            TextView txtTimeSpent = row.FindViewById<TextView>(Resource.Id.txtTimeSpent);
            txtTimeSpent.Text = mItems[position].TimeSpent;

            TextView txtMoney = row.FindViewById<TextView>(Resource.Id.txtMoneySpent);
            txtMoney.Text = mItems[position].Money;

            TextView txtAddress = row.FindViewById<TextView>(Resource.Id.txtAddress);
            txtAddress.Text = mItems[position].Address;

            TextView txtName = row.FindViewById<TextView>(Resource.Id.txtName);
            txtName.Text = mItems[position].Name;

            Button valBut = row.FindViewById<Button>(Resource.Id.buttonVal);
            Button rejBut = row.FindViewById<Button>(Resource.Id.buttonRej);

            if (valBut != null && rejBut != null)
            {
                rejBut.Click += RejBut_Click;
                valBut.Click += ValBut_Click;
            }

            if (mItems[position].Validated)
            {
                if (valBut != null && rejBut != null)
                {
                    valBut.Visibility = ViewStates.Gone;
                    rejBut.Visibility = ViewStates.Gone;
                    rejBut.Click -= RejBut_Click;
                    valBut.Click -= ValBut_Click;
                }
            }
            return row;
        }

        public void ValBut_Click(object sender, EventArgs e)
        {
            var but = (View)sender;
            View parent = (View)but.Parent;
            View grandpa = (View)parent.Parent;
            ((AppActivity)mContext).AddValidationForm(grandpa);
        }

        private void RejBut_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var parent = (ViewGroup)button.Parent;
            if (parent != null)
            {
                var pparent = (ViewGroup)parent.Parent;
                if (pparent != null)
                {
                    var txtName = pparent.FindViewById<TextView>(Resource.Id.txtName);
                    var txtAddress = pparent.FindViewById<TextView>(Resource.Id.txtAddress);
                    Place realPlace = null;
                    foreach (var place in mItems)
                    {
                        if (!place.Validated)
                            if (txtAddress.Text == place.Address && txtName.Text == place.Name)
                            {
                                realPlace = place;
                            }
                    }
                    if (realPlace != null)
                        mItems.Remove(realPlace);

                    pparent.RemoveView(parent);
                }
            }
        }
    }

}

