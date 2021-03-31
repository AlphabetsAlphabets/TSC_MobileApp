using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.Bluetooth;

using Plugin.Toast;

// Printing
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using Android.OS;
using System.Threading;
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Content;

namespace MobileApp
{


    /// <summary>
    /// This class is host to functions that are related to print job operations.
    /// </summary>
    public class Printing : CountDownTimer
    {
        public Task ConnectTask { get; private set; }
        public CancellationTokenSource Token { get; } = new CancellationTokenSource();
        // Note: This only works on an Android device.

        /* This is for setting timeout. Read more:
         * 1. https://docs.microsoft.com/en-us/dotnet/api/android.os.countdowntimer?view=xamarin-android-sdk-9
           2. https://developer.android.com/reference/android/os/CountDownTimer
        */
        public Printing(long millisInFuture, long countDownInterval) : base(millisInFuture, countDownInterval)
        {

        }

        public override void OnFinish()
        {
            System.Diagnostics.Debug.WriteLine("ON FINISH!");
        }

        public override void OnTick(long millisUntilFinished)
        {
        }

        /// <summary>
        /// Connects to the bluetooth printer
        /// </summary>
        /// <returns>BluetoothSocket</returns>
        /// <exception cref="Java.IO.IOException">Thrown when attempting to connect to a bluetooth capable device that isn't turned on.</exception>
        public BluetoothSocket ConnectToPrinter()
        {
            string status = "";
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null || !adapter.IsEnabled)
            {
                CrossToastPopUp.Current.ShowToastError("Bluetooth is not turned on.");
                return null;
            }


            ICollection<BluetoothDevice> devices = adapter.BondedDevices;
            BluetoothDevice printer = null;
            BluetoothSocket _socket = null;
            Java.Util.UUID uuidOfPrinter = null;
            foreach (var device in devices)
            {
                printer = device;
                var res = printer.FetchUuidsWithSdp();
                ParcelUuid[] uuid = printer.GetUuids();
                if (uuid == null && device.Name == "InnerPrinter")
                {
                    Java.Util.UUID innerPrinterUuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
                    _socket = printer.CreateRfcommSocketToServiceRecord(innerPrinterUuid);
                    _socket.Connect();

                    return _socket;
                }
                else continue;
                uuidOfPrinter = uuid[0].Uuid;
                _socket = printer.CreateRfcommSocketToServiceRecord(uuidOfPrinter);
                var outStream = _socket.OutputStream;
                var inStream = _socket.InputStream;

                bool canTimeout = outStream.CanTimeout;
                if (canTimeout) outStream.WriteTimeout = 3000;

                /*
                To tell if a BT device is a printer it must have a stream that you can write to, but a stream that you cannot read from.
                It's UUID must match the generic UUID of ALL BT printers, the generic UUID being: 00001101-0000-1000-8000-00805f9b34fb
                */
                var isWritable = outStream.CanWrite;
                var isReadable = outStream.CanRead;

                var isInStreamWritable = inStream.CanWrite;


                // Evaluates to true if the stream is writable but not readable, and that the input stream is also not writable
                var canWriteButNotRead = (isWritable && !isReadable && !isInStreamWritable);

                // Evaluates to true if the device has the generic UUID of a printer.
                var hasGenericUuidOfPrinter = (uuidOfPrinter.ToString() == "00001101-0000-1000-8000-00805f9b34fb");

                var deviceIsPrinter = (canWriteButNotRead && hasGenericUuidOfPrinter);

                // Evalues to true if canWriteButeNotRead is false, and isGenericUuidOfPrinter is false, so if it's ever true the device is not a printer
                if (!deviceIsPrinter)
                {
                    _socket.Close();
                    _socket.Dispose();
                    continue;
                };

                if (printer == null)
                {
                    status = "Either you are connected to the printer, but it isn't connected to you, and vice versa.";
                    CrossToastPopUp.Current.ShowToastError(status);
                    return null;
                }

                try
                {
                    _socket.Connect();

                    return _socket;
                }
                catch (Java.IO.IOException ioEx)
                {
                    _socket.Close();
                    continue;
                }
            }
            status = "None of the device you are paired to are printers.";
            CrossToastPopUp.Current.ShowToastError(status);
            return null;
        }
        

        /// <summary>
            /// This is used to print text, used in <see cref="PrintPage.IssueReceipt(object, EventArgs)">IssueReceipt</see>.
            /// </summary>
            /// <param name="_socket"></param>
            /// <param name="text"></param>
            /// <returns></returns>
        public async Task PrintStringAsync(BluetoothSocket _socket, Dictionary<string, string> text)
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
