using System;
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
    public class EditListViewAdapter : BaseAdapter<Place>
    {
        public List<Place> mItems;
        private Context mContext;

        public EditListViewAdapter(Context context, List<Place> items)
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
            View row = convertView;

            if (row == null)
            {
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.EditListView, null, false);
            }
            TextView txtTimeDate = row.FindViewById<TextView>(Resource.Id.txtTimeDate);
            txtTimeDate.Text = mItems[position].Time;

            TextView txtTimeSpent = row.FindViewById<TextView>(Resource.Id.txtTimeSpent);
            txtTimeSpent.Text = mItems[position].TimeSpent;

            TextView txtMoney = row.FindViewById<TextView>(Resource.Id.txtMoneySpent);
            txtMoney.Text = mItems[position].Money;

            TextView txtAddress = row.FindViewById<TextView>(Resource.Id.txtAddress);
            txtAddress.Text = mItems[position].Address;

            TextView txtName = row.FindViewById<TextView>(Resource.Id.txtName);
            txtName.Text = mItems[position].Name;

            Button Edit = row.FindViewById<Button>(Resource.Id.EditButton);
            Button Remove = row.FindViewById<Button>(Resource.Id.RemoveButton);


            Edit.Click += EditBut_Click;
            Remove.Click += RemoveBut_Click;

            return row;
        }

        public void EditBut_Click(object sender, EventArgs e)
        {
            var but = (View)sender;
            View parent = (View)but.Parent;
            View grandpa = (View)parent.Parent;
           
            ((AppActivity)mContext).EditValidationForm(grandpa,mItems);
        }

        public void RemoveBut_Click(object sender, EventArgs e)
        {
            var but = (View)sender;
            View parent = (View)but.Parent;
            View grandpa = (View)parent.Parent;
            ((AppActivity)mContext).RemoveValidationForm(grandpa,mItems);
        }
    }

}

