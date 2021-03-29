using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.Graphics;
using Android.Bluetooth;
using Xamarin.Essentials;

using Plugin.Toast;
using System.Diagnostics;

// Printing
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;

namespace MobileApp
{
    /// <summary>
    /// This class is host to functions that are related to print job operations.
    /// </summary>
    public static class Printing
    {
        /*
        TODO: 
        1. Combine print text and print images. 

        NOTE:
        This only works on an android device, and does not work on an iOS device.

        Connets to a Bluetooth printer called MTP-2, and prints stuff.
        Current the printer's name is MTP-2, it will be changed in the future,
        where the user gets to choose which device is the printer.
        
        It is possible to print images.
        */

        /// <summary>
        /// Connects to the bluetooth printer named MTP-2
        /// </summary>
        /// <returns>BluetoothSocket</returns>
        /// <exception cref="Java.IO.IOException">Thrown when attempting to connect to a bluetooth capable device that isn't turned on.</exception>
        public static BluetoothSocket ConnectToPrinter()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null || !adapter.IsEnabled)
            {
                CrossToastPopUp.Current.ShowToastError("Bluetooth is not turned on.");
                return null;
            }

            ICollection<BluetoothDevice> devices = adapter.BondedDevices;
            BluetoothDevice printer = null;
            foreach (var device in devices)
            {
                if (device.Name == "V2")
                {
                    printer = device;
                    break;
                }
            }

            if (printer == null)
            {
                var status = "Either you are connected to the printer, but it isn't connected to you, and vice versa.";
                CrossToastPopUp.Current.ShowToastError(status);
                return null;
            }
            Java.Util.UUID uuidOfPrinter = printer.GetUuids()[0].Uuid;
            BluetoothSocket _socket = printer.CreateRfcommSocketToServiceRecord(uuidOfPrinter);
            try
            {
                _socket.Connect();
            }
            catch (Java.IO.IOException ioEx)
            {
                string status = $"The device you are trying to connect to is turned off or unavailable\nDetailed: {ioEx.Message}";
                CrossToastPopUp.Current.ShowToastError(status);
                return null;
            }

            var isConnected = _socket.IsConnected;
            if (!isConnected)
            {
                CrossToastPopUp.Current.ShowToastError("Unable to connect to bluetooth device.");
                return null;
            }

            return _socket;

        }

        /// <summary>
        /// This is used to print text, used in <see cref="PrintPage.IssueReceipt(object, EventArgs)">IssueReceipt</see>.
        /// </summary>
        /// <param name="_socket"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task PrintStringAsync(BluetoothSocket _socket, Dictionary<string, string> text)
        {
            try
            {
                var file = File.Open(MainPage.root + "print.txt", FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[(int)file.Length];
                await file.ReadAsync(buffer, 0, (int)file.Length);

                var stream = _socket.OutputStream;
                await stream.WriteAsync(buffer, 0, buffer.Length);
                buffer = null;

                EPSON ep = new EPSON();
                var logoPath = MainPage.root + "logo.png";
                var logo = File.ReadAllBytes(logoPath);

                buffer = ByteSplicer.Combine(
                    ep.PrintImage(logo, true, true, 372, color: 0),
                    ep.CenterAlign(),
                    ep.SetStyles(PrintStyle.Underline),
                    ep.SetStyles(PrintStyle.DoubleHeight),
                    ep.PrintLine($"OUTLET: {text["outlet"]}"),
                    ep.SetStyles(PrintStyle.None),
                    ep.LeftAlign(),
                    ep.PrintLine($"Invoice number: {text["invoice"]}"),
                    ep.PrintLine($"Running number: {text["runNum"]}"),
                    ep.PrintLine($"Issued on: {DateTime.Now}"),
                    ep.FeedLines(2),
                    ep.CenterAlign(),
                    ep.PrintLine("Tel: 012-356-6789"),
                    ep.PrintLine("E-mail: email@provider.com"),
                    ep.FeedLines(4)
                );
                await stream.WriteAsync(buffer, 0, buffer.Length);
                buffer = null;
                file.Close();
                file.Dispose();
            }
            catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowToastError(ex.Message);
                return;
            }
        }
    }
}
        