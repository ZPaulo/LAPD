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
using Android.Util;
using Android.Support.V4.Content;
using Android.Support.V4.App;

namespace Smallet.Droid
{
    [Service(Exported = false)]
    public class DetectedActivitiesIntentService : IntentService
    {
        protected const string TAG = "activity-detection-intent-service";

        public DetectedActivitiesIntentService() : base(TAG)
        {
        }

        protected override void OnHandleIntent(Intent intent)
        {
            var result = ActivityRecognitionResult.ExtractResult(intent);

            handleDetectedActivities(result.ProbableActivities);
        }

        private void handleDetectedActivities(IList<DetectedActivity> probableActivities)
        {
            bool canTrust = true;
            foreach (DetectedActivity activity in probableActivities)
            {
                switch (activity.Type)
                {
                    case DetectedActivity.Still:
                        {
                            System.Diagnostics.Debug.WriteLine("ActivityRecogition", "Still: " + activity.Confidence);
                            if (activity.Confidence >= 30 && canTrust)
                            {
                                MainActivity.isStill = true;
                            }
                            break;
                        }
                    default:
                        if (activity.Confidence >= 30)
                        {
                            canTrust = false;
                            MainActivity.isStill = false;
                        }
                        break;
                }
            }
        }
        
    }
}