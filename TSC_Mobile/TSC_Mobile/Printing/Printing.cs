using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.Graphics;
using Android.Bluetooth;
using Xamarin.Essentials;

using Plugin.Toast;

namespace TSC_Mobile
{
    public static class Printing
    {
        /*
        Connets to a Bluetooth printer called MTP-2, and prints stuff.
        Current the printer's name is MTP-2, it will be changed in the future,
        where the user gets to choose which device is the printer.
        
        NEVER PRINT IMAGES (https://imgur.com/a/wW4be9x)
        - Unable to print images
        */

        /// <summary>
        /// Connects to the bluetooth printer named MTP-2
        /// </summary>
        /// <returns>BluetoothSocket</returns>
        public static async Task<BluetoothSocket> ConnectToPrinterAsync()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if(adapter == null || !adapter.IsEnabled)
            {
                CrossToastPopUp.Current.ShowToastError("Bluetooth is not turned on.");
                return null;
            }
            var devices = adapter.BondedDevices;
            BluetoothDevice device = null;
            foreach(var _device in devices)
            {
                if (_device.Name == "MTP-2")
                {
                    device = _device;
                    break;
                }
            }
            var isConnected = device.CreateBond();
            if (!isConnected)
            {
                CrossToastPopUp.Current.ShowToastError("Unable to connect to bluetooth device.");
                return null;
            }

            var uuid = device.GetUuids()[0].Uuid;
            var _socket = device.CreateRfcommSocketToServiceRecord(uuid);
            try
            {
                await _socket.ConnectAsync();
            }
            catch (Java.IO.IOException ioEx)
            {
                string status = $"The device you are trying to connect to is turned off or unavailable\nDetailed: {ioEx.Message}";
                CrossToastPopUp.Current.ShowToastError(status);
                return null;
            }
            isConnected = _socket.IsConnected;
            if (!isConnected)
            {
                CrossToastPopUp.Current.ShowToastError("Unable to connect to bluetooth device.");
                return null;
            }

            return _socket;

        }

        /// <summary>
        /// A new window is made to prompt users to select text file(s)
        /// </summary>
        /// <param name="_socket">BluetoothSocket</param>
        /// <returns>IEnumerable of FileResult</returns>
        public static async Task<IEnumerable<FileResult>> SelectTextFilesAsync(BluetoothSocket _socket) // Working w/exceptions & comments
        {
            var customFileType =
                new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "text/*" } },
                });

            var options = new PickOptions
            {
                PickerTitle = "Please select text file(s)",
                FileTypes = customFileType,
            };
            var text_files = await FilePicker.PickMultipleAsync(options);
            if (text_files == null) return null; // The only time this is null is when the user explicitly cancels the operation. So no error message is needed.

            return text_files;
        }

        /// <summary>
        /// Sends a command to the printer to print text content from text file(s)
        /// </summary>
        /// <param name="_socket">BluetoothSocket</param>
        /// <param name="text_files">IEnumerable of FileResults</param>
        public static async Task PrintTextFilesAsync(BluetoothSocket _socket, IEnumerable<FileResult> text_files) { 
            foreach(var _file in text_files) {
                var path = _file.FullPath;
                var file = File.Open(path, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[(int)file.Length];
                
                await PrintText(file, buffer, _socket);
            }
            _socket.Close();
            _socket.Dispose();
        }

        private static async Task PrintImagesAsync(BluetoothSocket _socket, IEnumerable<FileStream> images)
        {
            // Figure out how to print images
        }

        private static async Task PrintImage(FileStream file, byte[] buffer, BluetoothSocket _socket)
        {
            // Doesn't actually work
            Bitmap image = null;
            var _image = await BitmapFactory.DecodeStreamAsync(file);

            var hasAlpha = _image.HasAlpha;
            if (hasAlpha) image = _image.Copy(Bitmap.Config.Argb8888, true);
            else image = _image;
            _image.Dispose();

            try
            {
                image.EraseColor(0);
            }
            catch (Java.Lang.IllegalStateException isEx)
            {
                image.Dispose();
                CrossToastPopUp.Current.ShowToastError($"Colour cannot be erased from the image.\nDetailed: {isEx.Message}");
                return;
            }

            var memStream = new MemoryStream();
            await image.CompressAsync(Bitmap.CompressFormat.Png, 0, memStream);
            buffer = memStream.ToArray();
            await file.ReadAsync(buffer, 0, buffer.Length); // Modifies the buffer IN PLACE
            try
            {
                await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length); // This part is responsible for printing.
            }
            catch (Java.IO.IOException ioEx)
            {
                memStream.Close();
                memStream.Dispose();

                CrossToastPopUp.Current.ShowToastError($"{ioEx.Message}\nError: {ioEx.ToString()}");
                return;
            }
        }

        private static async Task PrintText(FileStream file, byte[] buffer, BluetoothSocket _socket)
        {
            /*
             writes a buffer to the printer, so it has text content to write.
             */
            await file.ReadAsync(buffer, 0, (int)file.Length);
            try
            {
                await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowToastError(ex.Message);
                return;
            }
        }
    }
}
