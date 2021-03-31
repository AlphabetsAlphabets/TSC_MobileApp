using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.Bluetooth;
using Plugin.Toast;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PrintPage : ContentPage
    {
        public PrintPage()
        {
            InitializeComponent();
        }

        private async void IssueReceipt(object sender, EventArgs e)
        {
            var outletName = outlet.Text;
            var invoiceNumber = invoice.Text;
            var runningNumber = runNum.Text;

            Dictionary<string, string> text = new Dictionary<string, string>
            {
                { "outlet", outletName },
                { "invoice", invoiceNumber },
                { "runNum", runningNumber}
            };

            var printer = new Printing(3000, 1000);
            BluetoothSocket socket = printer.ConnectToPrinter();
            if (socket == null) return;

            await printer.PrintStringAsync(socket, text);
            socket.Close();
            socket.Dispose();
        }
    }
}