using System.Diagnostics;
using System.Drawing;
using Catprinter.Utils;
using InTheHand.Net;
using InTheHand.Net.Sockets;
namespace Catprinter
{
    class App
    {
        static async Task Main(String[] args)
        {
            Console.WriteLine("Generating Image Data");
            Bitmap img = ImageProcessing.ReadImg("test.png", PrinterCommands.PRINT_WIDTH, "floyd-steinberg", true);
            img.Save("processed_image.jpg");
            Console.WriteLine("Image Data Generated");
            Process.Start("explorer.exe","processed_image.jpg");

            Console.Write("Continue [Y,n]: ");
            if (Console.ReadLine().ToUpper().Equals("N"))
            {
                return;
            }

            Console.WriteLine("Finding Printer");
            BluetoothClient client = new BluetoothClient();
            IReadOnlyCollection<BluetoothDeviceInfo> devices = client.DiscoverDevices();
            BluetoothDeviceInfo printer = null;
            foreach (BluetoothDeviceInfo device in devices)
            {
                if (device.DeviceName == "X5h-0000")
                {
                    printer = device;
                    break;
                }
            }
            if (printer == null)
            {
                Console.WriteLine("Printer not found");
                return;
            }
            Console.WriteLine("Printer found");
            BluetoothAddress address = printer.DeviceAddress;
            IReadOnlyCollection<Guid> services = printer.InstalledServices;
            Guid spp = new Guid();

            Console.WriteLine("Name: " + printer.DeviceName);
            Console.WriteLine("Address: " + address);
            Console.WriteLine("Services:");
            foreach (Guid service in services)
            {
                Console.WriteLine(" - " + service.ToString().ToUpper());
                if (service.ToString().ToUpper().Equals("00001101-0000-1000-8000-00805F9B34FB"))
                {
                    spp = service;
                    Console.WriteLine("    -> SPP service found");
                    break;
                }
            }
            if (!spp.ToString().ToUpper().Equals("00001101-0000-1000-8000-00805F9B34FB"))
            {
                Console.WriteLine("SPP service not found");
                return;
            }

            Console.WriteLine("Connecting to printer");
            client.Connect(printer.DeviceAddress, spp);
            Stream stream = client.GetStream();
            Console.WriteLine("Connected to printer");
            Console.WriteLine("Generating and sending commands");
            stream.Write(PrinterCommands.GetImgPrintCmd(img,0xffff));
            Console.WriteLine("Commands sent");
            Thread.Sleep(30000);

            stream.Close();
            client.Close();

            Console.WriteLine("Connection closed");
            return;
        }
    }
}
