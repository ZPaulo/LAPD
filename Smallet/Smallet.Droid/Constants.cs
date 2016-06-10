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
using Android.Gms.Location;

namespace Smallet.Droid
{
    public static class Constants
    {
        public const string PackageName = "com.xamarin.activityrecognition";
        public const string BroadcastAction = PackageName + ".BROADCAST_ACTION";
        public const string ActivityExtra = PackageName + ".ACTIVITY_EXTRA";
        public const string SharedPreferencesName = PackageName + ".SHARED_PREFERENCES";
        public const string ActivityUpdatesRequestedKey = PackageName + ".ACTIVITY_UPDATES_REQUESTED";
        public const string DetectedActivities = PackageName + ".DETECTED_ACTIVITIES";
        public const long DetectionIntervalInMilliseconds = 100;

        internal static readonly int[] MonitoredActivities = {
            DetectedActivity.Still,
            DetectedActivity.OnFoot,
            DetectedActivity.Walking,
            DetectedActivity.Running,
            DetectedActivity.OnBicycle,
            DetectedActivity.InVehicle,
            DetectedActivity.Tilting,
            DetectedActivity.Unknown
        };

        public static string GetActivityString(Context context, int detectedActivityType)
        {
            var resources = context.Resources;
            switch (detectedActivityType)
            {
                case DetectedActivity.InVehicle:
                    return resources.GetString(Resource.String.in_vehicle);
                case DetectedActivity.OnBicycle:
                    return resources.GetString(Resource.String.on_bicycle);
                case DetectedActivity.OnFoot:
                    return resources.GetString(Resource.String.on_foot);
                case DetectedActivity.Running:
                    return resources.GetString(Resource.String.running);
                case DetectedActivity.Still:
                    return resources.GetString(Resource.String.still);
                case DetectedActivity.Tilting:
                    return resources.GetString(Resource.String.tilting);
                case DetectedActivity.Unknown:
                    return resources.GetString(Resource.String.unknown);
                case DetectedActivity.Walking:
                    return resources.GetString(Resource.String.walking);
                default:
                    return resources.GetString(Resource.String.unidentifiable_activity, new[] { new Java.Lang.Integer(detectedActivityType) });
            }
        }
    }
}