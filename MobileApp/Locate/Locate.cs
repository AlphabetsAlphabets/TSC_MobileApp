using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Essentials;
using Plugin.Toast;

namespace MobileApp
{
    /// <summary>
    /// This class hosts functions that is related to calculating if a user is within a client's compound.
    /// </summary>
    class Locate
    {
        private static Location user_location = null;
        private static List<String> names = null;
        private static List<double> lat_one = null;
        private static List<double> lon_one = null;
        private static List<double> lat_two = null;
        private static List<double> lon_two = null;

        /// <summary>
        /// Makes a request to the API to get location information of all clients' shops
        /// </summary>
        /// <param name="uri">URI of the request, the URI is defined in <see cref="MainPage.uri"></see></param>
        /// <returns></returns>
        private static async Task GetClientLocationsAsync(string uri)
        {
            try
            {
                Locale apiResponse = await Request.Get_Location(uri);
                names = apiResponse.Name;
                lat_one = apiResponse.Lat_One; lon_one = apiResponse.Lon_One;
                lat_two = apiResponse.Lat_Two; lon_two = apiResponse.Lon_Two;
            }
            catch (TimeoutException)
            {
                Debug.WriteLine("Unable to make request to api.");
                CrossToastPopUp.Current.ShowToastError("Unable to make request to api.");
                return;
            }
        }

        /// <summary>
        /// Gets the user's current location
        /// </summary>
        /// <returns></returns>
        private static async Task<Location> GetUserLocationAsync()
        {
            try
            {
                GeolocationRequest geolocationRequest = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var cts = new CancellationTokenSource();
                user_location = await Geolocation.GetLocationAsync(geolocationRequest, cts.Token);

                return user_location;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                Debug.WriteLine("Location services not enabled.");
                CrossToastPopUp.Current.ShowToastError($"Location services is turned off.\nDetail: {fneEx.Message}");
                return null;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Debug.WriteLine("Feature not supported.");
                CrossToastPopUp.Current.ShowToastError($"Feature is not supported on this device.\nDetailed: {fnsEx.Message}");
                return null;
            }
            catch (PermissionException pEx)
            {
                string error_message = "You have not given this app permission to access this device's location.";
                CrossToastPopUp.Current.ShowToastError(error_message + $"\nDetailed: {pEx.Message}");
                return null;

            }
            catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowToastError(ex.Message);
                return null;
            }
        }
        /// <summary>
        /// Checks whether or not a person is in a client's shop. A HTTP GET request is made to the api. 
        /// </summary>
        /// <param name="uri">The request URI</param>
        /// <returns></returns>
        public static async Task<String> IsUserNearClientAsync(string uri)
        {
            string status = "";
            /*
            For this to work you need to turn on the api. The endpoint is location.py in Web API/env-api/endpoints/location.py 
            Make sure that the table tlocations exists in the schema tsc_office. The information contained within the table looks
            like this (https://imgur.com/a/QF9HMVt) The location's name, two sets of latitudes (lat_one, lat_two) and two sets of longitudes (lon_one, lon_two)
             */
            user_location = await GetUserLocationAsync();
            await GetClientLocationsAsync(uri);

            if (user_location == null) return "Location services is not turned on.";
            var user_coordinate = new Location(user_location.Latitude, user_location.Longitude); // User's current location

            for (int i = 0; i < names.Count; i++)
            {
                // Two points in the client's shop
                Location origin = new Location(lat_one[i], lon_one[i]);
                Location end = new Location(lat_two[i], lon_two[i]);

                // Gets the diameter, then the radius from it.
                double diameter = Math.Abs(Location.CalculateDistance(origin, end, DistanceUnits.Kilometers));
                double radius = diameter / 2;

                // The midpoint is the circle's centre
                double mp_lat = (lat_one[i] + lat_two[i]) / 2;
                double mp_lon = (lon_one[i] + lon_two[i]) / 2;

                Location midpoint_of_shop = new Location(mp_lat, mp_lon);
                double user_relative_distance_from_circle_centre = Math.Abs(Location.CalculateDistance(user_coordinate, midpoint_of_shop, DistanceUnits.Kilometers));
                bool inCompound = (user_relative_distance_from_circle_centre <= radius);
                if (inCompound)
                {
                    status = $"You are {user_relative_distance_from_circle_centre * 1000} meters away from {names[i]}\nTolerance: {radius * 1000} meters";
                    return status;
                }
            }
            status = "You are not in the area of any shops/dealers";
            return status;
        }
    }
}
