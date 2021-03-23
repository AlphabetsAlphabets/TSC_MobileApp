using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.Graphics;
using Android.Bluetooth;
using Xamarin.Essentials;

using Plugin.Toast;
using System.Diagnostics;

namespace MobileApp
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
            var isConnected = _socket.IsConnected;
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
                var file = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[(int)file.Length];
                
                await PrintText(file, buffer, _socket);
            }
            _socket.Close();
            _socket.Dispose();
        }

        public static async Task<IEnumerable<FileResult>> SelectImageFilesAsync(BluetoothSocket _socket)
        {
            // Figure out how to print images
            var CustomFileType =
                new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<String>>
                {
                    { DevicePlatform.Android, new[] {"image/*" } }
                });

            var options = new PickOptions
            {
                PickerTitle = "Please select image(s)",
                FileTypes = CustomFileType,
            };
            var image_files = await FilePicker.PickMultipleAsync(options);
            if (image_files == null) return null;

            return image_files;
        }

        public static async Task PrintImageFilesAsync(BluetoothSocket _socket, IEnumerable<FileResult> text_files) { 
            foreach(var _file in text_files) {
                var path = _file.FullPath;
                var file = System.IO.File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                byte[] buffer = new byte[(int)file.Length];
                
                await PrintImage(file, buffer, _socket);
            }
            _socket.Close();
            _socket.Dispose();
        }

        public static async Task PrintImageFilesAsync(String path, BluetoothSocket _socket)
        {
            try
            {
                var buffer = File.ReadAllBytes(path);
                await PrintImage(buffer, _socket);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: {ex}");
                return;
            }
        }

        private static async Task<Bitmap> StripColor(Bitmap image)
        {
            image = image.Copy(Bitmap.Config.Argb8888, true);
            try
            {
                image.EraseColor(0);
            }
            catch (Java.Lang.IllegalStateException isEx)
            {
                Debug.WriteLine($"ERROR:\n\n{isEx.Message}");
                return null;
            }

            return image;
        }

        private static async Task PrintImage(byte[] buffer, BluetoothSocket _socket)
        {
            await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        private static async Task PrintImage(FileStream file, byte[] buffer, BluetoothSocket _socket)
        {
            Bitmap _image = await BitmapFactory.DecodeStreamAsync(file);
            var image = await StripColor(_image);

            using (var memStream = new MemoryStream()) {
                await image.CompressAsync(Bitmap.CompressFormat.Jpeg, 0, memStream);
                buffer = memStream.ToArray();
                await file.ReadAsync(buffer, 0, buffer.Length); // Modifies the buffer IN PLACE
                try
                {
                    await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length); // This part is responsible for printing.
                }
                catch (Java.IO.IOException ioEx)
                {
                    _image.Dispose();
                    image.Dispose();

                    memStream.Close();
                    memStream.Dispose();

                    CrossToastPopUp.Current.ShowToastError($"{ioEx.Message}\nError: {ioEx}");
                    return;
                }
                image.Dispose();
                _image.Dispose();
            }
            
            var status = "Print job completed.";
            Debug.WriteLine(status);
            CrossToastPopUp.Current.ShowToastMessage(status);
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
