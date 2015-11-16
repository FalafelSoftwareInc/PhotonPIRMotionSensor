using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PIRMotionStream
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string PHOTON1DEVICEID = "{YOURDEVICEID}";
        const string ACCESS_TOKEN = "{YOURACCESSTOKEN}";
        SolidColorBrush MotionColor = new SolidColorBrush(Colors.Red);
        SolidColorBrush ClearColor = new SolidColorBrush(Colors.Green);
        public MainPage()
        {
            this.InitializeComponent();
        }
        private async void Open_Stream_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            string url = String.Format("https://api.particle.io/v1/devices/{0}/events/Motion?access_token={1}", PHOTON1DEVICEID, ACCESS_TOKEN);
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    int blankRead = 0;
                    while (blankRead <= 5)
                    {
                        var str = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(str))
                        {
                            ++blankRead;
                            if (blankRead > 1)
                            {
                                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                {
                                    streamResultTextBox.Text = "Blank: " + blankRead.ToString();
                                });
                            }
                        }
                        else if (str == "event: Motion")
                        {
                            blankRead = 0;
                            var data = await reader.ReadLineAsync();
                            var jsonData = JsonConvert.DeserializeObject<ParticleEvent>(data.Substring(data.IndexOf("data:") + 5));
                            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                streamResultTextBox.Text = jsonData.data > 2048 ? "Motion" : "Clear";// "Data: " + jsonData.data;
                                streamResultTextBox.Foreground = jsonData.data > 2048 ? MotionColor : ClearColor;
                            });
                        }
                    }
                }
            }
            (sender as Button).IsEnabled = true;
        }
    }
    public class ParticleEvent
    {
        public int data { get; set; }
        public string ttl { get; set; }
        public string published_at { get; set; }
        public string coreid { get; set; }
    }
}
