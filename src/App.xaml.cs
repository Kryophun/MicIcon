using NAudio.CoreAudioApi;
using System.Windows;

namespace MicIcon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1 && string.Compare(e.Args[0], "-togglemute", true /*ignoreCase*/) == 0) {
                ToggleMuteOnSelectedDevice();
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                MainWindow wnd = new MainWindow();
                wnd.Show();
            }
        }

        private void ToggleMuteOnSelectedDevice()
        {
            string persistedDeviceName = MicIcon.Properties.Settings.Default.LastDeviceName;

            if (string.IsNullOrWhiteSpace(persistedDeviceName))
            {
                MessageBox.Show("No default input device is set; run this app in UI mode first to select a preferred device");
            } else
            {
                var deviceEnumerator = new MMDeviceEnumerator();
                foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                {
                    if (device.DeviceFriendlyName == persistedDeviceName)
                    {
                        device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
                        return;
                    }
                }

                MessageBox.Show($"Could not find active input device named {persistedDeviceName}, please run this app in UI mode and select a different preferred device");
            }
        }
    }
}
