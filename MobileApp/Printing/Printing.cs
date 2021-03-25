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

namespace MobileApp
{
    /// <summary>
    /// This class is host to functions that are related to print job operations.
    /// </summary>
    public static class Printing
    {
        /*
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
        public static async Task<BluetoothSocket> ConnectToPrinterAsync()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if(adapter == null || !adapter.IsEnabled)
            {
                CrossToastPopUp.Current.ShowToastError("Bluetooth is not turned on.");
                return null;
            }
            var devices = adapter.BondedDevices;
            BluetoothDevice printer = null;
            foreach(var device in devices)
            {
                if (device.Name == "MTP-2")
                {
                    printer = device;
                    break;
                }
            }
            var uuidOfPrinter = printer.GetUuids()[0].Uuid;
            var _socket = printer.CreateRfcommSocketToServiceRecord(uuidOfPrinter);
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
        /// A new window is made to prompt users to select text file(s). For example, it is used in <see cref="MainPage.Print_Text(object, EventArgs)"/>
        /// </summary>
        /// <returns>IEnumerable of FileResult</returns>
        public static async Task<IEnumerable<FileResult>> SelectTextFilesAsync() // Working w/exceptions & comments
        {
            // To find out more about FileIO read: https://docs.microsoft.com/en-us/xamarin/essentials/file-picker?tabs=android
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
        /// Sends a command to the printer to print text content from text file(s). For example, it is used in <see cref="MainPage.Print_Text(object, EventArgs)"/>
        /// </summary>
        /// <param name="_socket">BluetoothSocket</param>
        /// <param name="text_files">IEnumerable of FileResults</param>
        /// <exception cref="Exception">Occurs whenever there is any exception</exception>
        public static async Task PrintTextFilesAsync(BluetoothSocket _socket, IEnumerable<FileResult> textFiles) { 
            foreach(var file in textFiles) {
                var path = file.FullPath;
                var textFile = File.Open(path, FileMode.Open, FileAccess.Read);

                int bufferSize = (int)textFile.Length;

                /* A buffer is a byte array that is empty, but data will be written to it.
                   And the data that is written will be sent over to the bluetooth device. */
                byte[] buffer = new byte[bufferSize * 2];
                string centerAlignmentByteCode = "1A 5B 00";
                //buffer[0] = Convert.ToByte("27");
                //buffer[1] = Convert.ToByte("97");
                //buffer[2] = Convert.ToByte("49");
                try
                {
                    var byteCode = Encoding.ASCII.GetBytes(centerAlignmentByteCode);
                    for (int i = 0; i < byteCode.Length; i++)
                    {
                        var code = byteCode[i];
                        buffer[i] = code;
                    };
                }
                catch (Exception byteException)
                {
                    Debug.WriteLine("BYTE EXCEPTION!");
                    var message = byteException.Message;
                    Debug.WriteLine($"Message: {message}");


                    Debug.WriteLine("Can't convert to byte.");
                    return;
                }

                await textFile.ReadAsync(buffer, 0, bufferSize); // Write data to buffer
                try
                {
                    await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length); // This line is responsible for printing by sending data in buffer to the printer.
                    buffer = null;
                }
                catch (Exception ex)
                {
                    CrossToastPopUp.Current.ShowToastError(ex.Message);
                    buffer = null;
                    return;
                }
            }
            _socket.Close();
            _socket.Dispose();
        }

        /// <summary>
        /// Creates a new window that allows the user to select images. Used in <see cref="MainPage.Print_Images(object, EventArgs)"/>
        /// </summary>
        /// <returns>IEnumerable of FileResult</returns>
        public static async Task<IEnumerable<FileResult>> SelectImageFilesAsync()
        {
            // Selecting custom file types that you can pick, read more about file IO: https://docs.microsoft.com/en-us/xamarin/essentials/file-picker?tabs=android
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

        /// <summary>
        /// Sends an array of bytes to the bluetooth printer, and initiates printing. For example, it is used in <see cref="MainPage.Print_Images(object, EventArgs)"/>
        /// </summary>
        /// <param name="_socket">Bluetooth socket of the device</param>
        /// <param name="imageFiles">An IEnumerable of FileResult</param>
        /// <returns>Task</returns>
        public static async Task PrintImageFilesAsync(BluetoothSocket _socket, IEnumerable<FileResult> imageFiles)
        { 
            foreach(var file in imageFiles) {
                var path = file.FullPath;
                var imageFile = File.Open(path, FileMode.Open, FileAccess.Read);
                
                Bitmap originalImage = await BitmapFactory.DecodeStreamAsync(imageFile);
                var strippedImage = StripColor(originalImage); // Strips the image of ALL colour, and re-colourizes it all to black.

                using (var memStream = new MemoryStream()) {
                    int bufferLength = (int)imageFile.Length;
                    byte[] buffer = new byte[bufferLength];

                    await strippedImage.CompressAsync(Bitmap.CompressFormat.Png, 0, memStream); // Writes image data to memStream
                    buffer = memStream.ToArray(); // Writes data in memStream to buffer
                    //await imageFile.ReadAsync(buffer, 0, buffer.Length); // Modifies the buffer IN PLACE
                    try
                    {
                        var outputStream = _socket.OutputStream;
                        await outputStream.WriteAsync(buffer, 0, buffer.Length); // This part is responsible for printing.
                        await outputStream.FlushAsync();
                        byte[] response = new byte[buffer.Length];
                    }
                    catch (Java.IO.IOException ioEx)
                    {
                        _socket.Close();

                        CrossToastPopUp.Current.ShowToastError($"{ioEx.Message}\nError: {ioEx}");
                    }
                }
                var status = "Print job completed.";
                Debug.WriteLine(status);
                CrossToastPopUp.Current.ShowToastMessage(status);
            }
        }

        public static async Task PrintImageFilesAsync(String path, BluetoothSocket _socket)
        {
            try
            {
                var imageFile = File.Open(path, FileMode.Open, FileAccess.Read);
                int bufferLength = (int)imageFile.Length;
                var buffer = new byte[bufferLength];

                var originalImage = await BitmapFactory.DecodeFileAsync(path);
                var strippedImage = StripColor(originalImage);

                using (var memStream = new MemoryStream())
                {
                    await strippedImage.CompressAsync(Bitmap.CompressFormat.Png, 0, memStream);
                    buffer = memStream.ToArray(); 

                    await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            } catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex}\n\n{ex.Message}");
            }
        }

        /// <summary>
        /// Strips the image of all colour, which results in a black image. For example, it is used in <see cref="PrintImageFilesAsync(BluetoothSocket, IEnumerable{FileResult})"/>
        /// </summary>
        /// <param name="image">The image to remove the color from</param>
        /// <returns>Bitmap</returns>
        private static Bitmap StripColor(Bitmap image)
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

    }
}
