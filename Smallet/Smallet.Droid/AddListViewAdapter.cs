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
    public class AddListViewAdapter : BaseAdapter<Place>
    {
        public List<Place> mItems;
        private Context mContext;

        public AddListViewAdapter(Context context, List<Place> items)
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
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.AddListView, null, false);
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
            
            Button Validate = row.FindViewById<Button>(Resource.Id.ValidateButton);


            if (Validate != null)
            {
                Validate.Click += ValBut_Click;
            }

            if (mItems[position].Validated)
            {
                if (Validate != null )
                {
                    Validate.Visibility = ViewStates.Gone;

                    Validate.Click -= ValBut_Click;
                }
            }
            

            return row;
        }

        public void ValBut_Click(object sender, EventArgs e)
        {
            var but = (View)sender;
            View parent = (View)but.Parent;
            View grandpa = (View)parent.Parent;
            View greatGrandpa = (View)grandpa.Parent;
            ((AppActivity)mContext).ManAddValidationForm(greatGrandpa,mItems);
        }
    }

}

