using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using static Xamarin.Essentials.Permissions;

namespace NearSatMobile
{
    public partial class MainPage : ContentPage
    {

        #region Variables

        private string satelliteName;
        public string SatelliteName
        {
            get { return satelliteName; }
            set
            {
                if(closest.name == null)
                {
                    //Nothing
                }
                else
                {
                    satelliteName = closest.name;
                }
                
                OnPropertyChanged(nameof(SatelliteName));
            }
        }

        private double satelliteDistance;
        public string SatelliteDistance
        {
            get { return satelliteDistance.ToString(); }
            set
            {
                if(closest.distance.ToString() == null)
                {
                    //Nothing
                }
                else
                {
                    satelliteDistance = closest.distance;
                }
                
                OnPropertyChanged(nameof(SatelliteDistance));
            }
        }

        private double userLong;
        public double UserLongitude
        {
            get { return userLong; }
            set
            {
                userLong = userCoords.longitude;

                OnPropertyChanged(nameof(UserLongitude));
            }
        }

        private double userLat;
        public double UserLatitude
        {
            get { return userLat; }
            set
            {
                userLat = userCoords.latitude;

                OnPropertyChanged(nameof(UserLatitude));
            }
        }

        private double userAlt;
        public double UserAltitude
        {
            get { return userAlt; }
            set
            {
                userAlt = userCoords.altitude;

                OnPropertyChanged(nameof(UserAltitude));
            }
        }

        private bool _isFindVisible;
        public bool IsFindVisible
        {
            get
            {
                return _isFindVisible;
            }
            set
            {
                _isFindVisible = value;
                OnPropertyChanged(nameof(IsFindVisible));
            }
        }        

        public Satellite closest;
        public CoordGroup userCoords = new CoordGroup();

        public static bool isBackground = false;
        private bool hasClickedFind = false;

        #endregion

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            //GetMediaAsync();               

            closest = new Satellite("Oi, you need to click the button", 69);

            if (SatelliteCalculations.UpdateTLEData())
            {
                IsFindVisible = true;          
            }

            UserLatitude = userCoords.latitude;
            UserLongitude = userCoords.longitude;
            UserAltitude = userCoords.altitude;

        }

        public async Task<CoordGroup> GetLocation()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    userCoords.altitude = (double)location.Altitude;
                    userCoords.latitude = (double)location.Latitude;
                    userCoords.longitude = (double)location.Longitude;

                    UserAltitude = (double)location.Altitude;
                    UserLongitude = (double)location.Longitude;
                    UserLatitude = (double)location.Latitude;

                    return userCoords;
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("Warning", "Feature not supported - ERROR CODE: " + fnsEx.Message, "Ok");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                await DisplayAlert("Warning", "Feature not enabled - ERROR CODE: " + fneEx.Message, "Ok");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("Warning", "Permissions are not set up - ERROR CODE: " + pEx.Message, "Ok");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", "Unable to get location - ERROR CODE: " + ex.Message, "Ok");
            }

            await DisplayAlert("Alert", "Unable to get location", "Ok");
            return null;
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            if (!hasClickedFind)
            {
                await GetLocation();

                await Task.Run(DisplaySatellite);

                hasClickedFind = true;
            }           
        }

        async Task DisplaySatellite()
        {
            while (true)
            {
                closest = await SatelliteCalculations.FindClosestSatellite(userCoords.latitude, userCoords.longitude, userCoords.altitude);

                SatelliteName = closest.name;
                SatelliteDistance = closest.distance.ToString() + " km";

                System.Threading.Thread.Sleep(100);
            }          
        }
    }
}
