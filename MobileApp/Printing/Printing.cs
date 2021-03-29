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
            if(adapter == null || !adapter.IsEnabled)
            {
                CrossToastPopUp.Current.ShowToastError("Bluetooth is not turned on.");
                return null;
            }

            ICollection<BluetoothDevice> devices = adapter.BondedDevices;
            BluetoothDevice printer = null;
            foreach(var device in devices)
            {
                if (device.Name == "V2")
                {
                    printer = device;
                    break;
                }
            }

            if (printer == null)
            {
                var status = "This device is not paired to the device. Both the printer, and your device must be connected to each other.";
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
        /// A new window is made to prompt users to select text file(s). For example, it is used in <see cref="MainPage.Print_Text(object, EventArgs)"/>
        /// </summary>
        /// <returns>IEnumerable of FileResult</returns>
        public static async Task<IEnumerable<FileResult>> SelectTextFilesAsync() // Working w/exceptions & comments
        {
            // To find out more about FileIO read: https://docs.microsoft.com/en-us/xamarin/essentials/file-picker?tabs=android
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "text/*" } },
            });

            var options = new PickOptions
            {
                PickerTitle = "Please select text file(s)",
                FileTypes = customFileType,
            };
            IEnumerable<FileResult> text_files = await FilePicker.PickMultipleAsync(options);
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
            // Don't read from file, everything that is predefined will be stored in memory.
            EPSON ep = new EPSON();
            foreach (var file in textFiles) {
                var path = file.FullPath;
                var textFile = File.Open(path, FileMode.Open, FileAccess.Read);

                int bufferSize = (int)textFile.Length;

                /* A buffer is a byte array that is empty, but data will be written to it.
                   And the data that is written will be sent over to the bluetooth device. */
                byte[] buffer = new byte[bufferSize];
                await textFile.ReadAsync(buffer, 0, bufferSize); // Write data to buffer

                // printing text from file
                var outputStream = _socket.OutputStream;
                await outputStream.WriteAsync(buffer, 0, buffer.Length); // This line is responsible for printing by sending data in buffer to the printer.
                buffer = null;

                // Setup invoice, eventually this will be dynamic.
                var outletName = "SPEAKEASY BAR AND BRISTOL";
                var invoice = $"INVOICE: 123456";
                var RM = 123456789;

                // Get the company logo
                var logoPath = MainPage.root + "logo.png";
                var logo = File.ReadAllBytes(logoPath);

                // Setup printing here, api provided by: https://github.com/lukevp/ESC-POS-.NET
                byte[] centerCode = ByteSplicer.Combine(
                    ep.PrintImage(logo, false, true, 372, color: 0),
                    ep.CenterAlign(),
                    ep.SetStyles(PrintStyle.DoubleWidth),
                    ep.SetStyles(PrintStyle.DoubleHeight),
                    ep.SetStyles(PrintStyle.DoubleHeight),
                    ep.PrintLine("============================="),
                    ep.PrintLine("|| TONG SAN CHAN SDN. BHD. ||"),
                    ep.PrintLine("============================="),
                    ep.SetStyles(PrintStyle.None),
                    ep.CenterAlign(),
                    ep.SetStyles(PrintStyle.Bold),
                    ep.PrintLine($"OUTLET: {outletName}"),
                    ep.SetStyles(PrintStyle.None),
                    ep.LeftAlign(),
                    ep.PrintLine(invoice),
                    ep.PrintLine($"RM: {RM}"),
                    ep.CenterAlign(),
                    ep.PrintLine("----------"),
                    ep.PrintLine("Thank you.")
                );


                try
                {
                    // Printing out custom text.
                    await outputStream.WriteAsync(centerCode, 0, centerCode.Length);
                }
                catch (Exception ex)
                {
                    CrossToastPopUp.Current.ShowToastError(ex.Message);
                    buffer = null;
                    _socket.Close();
                    return;
                }
            }
        }

        public static async Task PrintStringAsync(BluetoothSocket _socket, Dictionary<string, string> text)
        {
            try
            {
                var file = File.Open(MainPage.root + "test.txt", FileMode.Open, FileAccess.Read);
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
                FileStream imageFile = File.Open(path, FileMode.Open, FileAccess.Read);
                
                Bitmap originalImage = await BitmapFactory.DecodeStreamAsync(imageFile);
                Bitmap strippedImage = StripColor(originalImage); // Strips the image of ALL colour, and re-colourizes it all to black.

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
            var ep = new EPSON();
            try
            {
                var buffer = ByteSplicer.Combine(
                    ep.CenterAlign(),
                    ep.PrintLine("IMAGE"),
                    ep.PrintImage(File.ReadAllBytes(path), false, true, 372, color: 0)
                );

                var outputStream = _socket.OutputStream;
                await outputStream.WriteAsync(buffer, 0, buffer.Length);
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
