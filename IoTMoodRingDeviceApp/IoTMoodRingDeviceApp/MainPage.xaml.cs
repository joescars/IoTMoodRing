using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IoTMoodRingDeviceApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int GREEN_LED_PIN = 5;
        private const int RED_LED_PIN = 6;
        private const int BLUE_LED_PIN = 13;
        private int[] LEDS = new int[] { GREEN_LED_PIN, RED_LED_PIN, BLUE_LED_PIN };
        private Dictionary<int, GpioPin> GpioPIns = new Dictionary<int, GpioPin>();

        private GpioController gpio;

        public MainPage()
        {
            this.InitializeComponent();

            this.gpio = GpioController.GetDefault();

            // Initialize LEDs
            InitializeLEDs();

            TestLEDS();

            TurnLEDsOff();

            GetMessages();

        }

        private void TurnLEDOn(int pinId)
        {
            var pin = GpioPIns[pinId];
            pin.Write(GpioPinValue.Low);
        }

        private void TurnLEDOff(int pinId)
        {
            var pin = GpioPIns[pinId];
            pin.Write(GpioPinValue.High);
        }

        private void InitializeLEDs()
        {
            foreach (int pinId in this.LEDS)
            {
                var pin = gpio.OpenPin(pinId);
                pin.SetDriveMode(GpioPinDriveMode.Output);

                // Cache the GpioPin Object
                GpioPIns.Add(pinId, pin);
            }

            TurnLEDsOff();
        }

        private void TurnLEDsOff()
        {
            foreach(int pin in this.LEDS)
            {
                this.TurnLEDOff(pin);
            }
        }

        // IOT HUB CODE //
        private async void GetMessages()
        {
            //this.TurnLEDOn(RED_LED_PIN);

            while (true)
            {
                string result = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
                Random rnd1 = new Random();

                if (result != null)
                {
                    //output for debugging
                    myOutput.Text = result;

                    this.TurnLEDsOff();

                    if (result == "happy")
                    {
                        this.TurnLEDOn(GREEN_LED_PIN);
                    }
                    else if (result == "angry")
                    {
                        for(int i = 0; i < 8; i++)
                        {
                            this.TurnLEDOn(RED_LED_PIN);
                            Task.Delay(100).Wait();
                            this.TurnLEDOff(RED_LED_PIN);
                            Task.Delay(100).Wait();
                        }

                        this.TurnLEDOn(RED_LED_PIN);

                    }
                    else if (result == "neutral")
                    {
                        this.TurnLEDOn(BLUE_LED_PIN);
                    }
                    else if (result == "test")
                    {
                        TestLEDS();
                    }
                    else if (result == "off")
                    {
                        this.TurnLEDsOff();
                    }
                    //int rndLed = rnd1.Next(3);                    
                    //this.TurnLEDsOff();
                    //TurnLEDOn(LEDS[rndLed]);
                }
                //Console.WriteLine(result);
                Task.Delay(1000).Wait();
            }

            

        }

        private void TestLEDS()
        {

            this.TurnLEDOn(GREEN_LED_PIN);
            Task.Delay(300).Wait();
            this.TurnLEDOn(RED_LED_PIN);
            Task.Delay(300).Wait();
            this.TurnLEDOn(BLUE_LED_PIN);
            Task.Delay(300).Wait();
            TurnLEDsOff();
            Task.Delay(300).Wait();

            this.TurnLEDOn(GREEN_LED_PIN);
            Task.Delay(200).Wait();
            this.TurnLEDOn(RED_LED_PIN);
            Task.Delay(200).Wait();
            this.TurnLEDOn(BLUE_LED_PIN);
            Task.Delay(200).Wait();
            TurnLEDsOff();
            Task.Delay(200).Wait();

            this.TurnLEDOn(GREEN_LED_PIN);
            Task.Delay(100).Wait();
            this.TurnLEDOn(RED_LED_PIN);
            Task.Delay(100).Wait();
            this.TurnLEDOn(BLUE_LED_PIN);
            Task.Delay(100).Wait();
            TurnLEDsOff();
            Task.Delay(100).Wait();

        }
    }
}
