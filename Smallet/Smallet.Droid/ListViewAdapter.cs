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
	public class ListViewAdapter : BaseAdapter<Place>
	{
		public List<Place> mItems;
		private Context mContext;
        View row;
		public ListViewAdapter (Context context, List<Place> items)
		{
			mItems = items;
			mContext = context;
		}

		public override int Count {
			get {
				return mItems.Count;
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override Place this[int position]
		{
			get{ return mItems [position];}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			row = convertView;

			if (row == null) {
				row = LayoutInflater.From (mContext).Inflate (Resource.Layout.ListViewRow, null, false);
			}
			TextView txtTime = row.FindViewById<TextView> (Resource.Id.txtTime);
			txtTime.Text = mItems [position].Time;

			TextView txtMoney= row.FindViewById<TextView> (Resource.Id.txtMoneySpent);
			txtMoney.Text = mItems [position].Money;

			TextView txtAddress = row.FindViewById<TextView> (Resource.Id.txtAddress);
			txtAddress.Text = mItems [position].Address;

            TextView txtName = row.FindViewById<TextView>(Resource.Id.txtName);
            txtName.Text = mItems[position].Name;
            return row;
		}
    }

}

