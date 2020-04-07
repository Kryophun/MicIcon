using Hardcodet.Wpf.TaskbarNotification;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// TODO: Save preferred device across runs

namespace MicIcon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<MMDevice> inputDevices = new List<MMDevice>();
        private readonly List<string> inputDeviceNames = new List<string>();
        private MMDevice selectedInputDevice;

        private Icon unmutedIcon;
        private Icon mutedIcon;

        TaskbarIcon tbi = new TaskbarIcon();

        public MainWindow()
        {
            InitializeComponent();
            this.StateChanged += MainWindow_StateChanged;

            DataContext = this;

            using (Stream stream = Application.GetResourceStream(new Uri("/mic.ico", UriKind.Relative)).Stream)
            {
                unmutedIcon = new Icon(stream);
            }
            using (Stream stream = Application.GetResourceStream(new Uri("/mutedmic.ico", UriKind.Relative)).Stream)
            {
                mutedIcon = new Icon(stream);
            }

            tbi.LeftClickCommand = new ShowMessageCommand(this);

            inputDevicesComboBox.ItemsSource = inputDeviceNames;
            inputDevicesComboBox.SelectionChanged += InputDevicesComboBox_SelectionChanged;

            RefreshDeviceList(Properties.Settings.Default.LastDeviceName);

            Console.WriteLine($"Selected index: {inputDevicesComboBox.SelectedIndex}");
        }

        private void RefreshDeviceList(string preferredSelectedDeviceFriendlyName)
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                Console.WriteLine($"{device.DeviceFriendlyName}-{device.FriendlyName}-{device.State.ToString()}");
                inputDevices.Add(device);
                inputDeviceNames.Add(device.DeviceFriendlyName);
            }

            if (inputDevicesComboBox.SelectedIndex == -1)
            {
                if (!string.IsNullOrWhiteSpace(preferredSelectedDeviceFriendlyName))
                {
                    int foundIndex = inputDeviceNames.FindIndex((string possibleMatch) => possibleMatch == preferredSelectedDeviceFriendlyName);
                    if (foundIndex >= 0)
                    {
                        inputDevicesComboBox.SelectedIndex = foundIndex;
                    }
                } else if (inputDeviceNames.Count > 0)
                {
                    inputDevicesComboBox.SelectedIndex = 0;
                }
            }
        }

        private void InputDevicesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (selectedInputDevice != null)
            {
                selectedInputDevice.AudioEndpointVolume.OnVolumeNotification -= AudioEndpointVolume_OnVolumeNotification;
            }

            int selectedIndex = ((ComboBox)e.Source).SelectedIndex;
            if (selectedIndex >= 0)
            {
                Console.WriteLine($"Hello {selectedIndex}");
                selectedInputDevice = inputDevices[selectedIndex];
                selectedInputDevice.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
                RefreshIconWithMuteState();
                Properties.Settings.Default.LastDeviceName = selectedInputDevice.DeviceFriendlyName;
                Properties.Settings.Default.Save();
            }
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            RefreshIconWithMuteState();
        }

        private void RefreshIconWithMuteState()
        {
            Console.WriteLine($"IS MUTED? {selectedInputDevice.AudioEndpointVolume.Mute}");
            Application.Current.Dispatcher.Invoke(new Action(() => {
                tbi.Icon = selectedInputDevice.AudioEndpointVolume.Mute ? mutedIcon : unmutedIcon;
                tbi.ToolTipText = selectedInputDevice.FriendlyName + " is " + (selectedInputDevice.AudioEndpointVolume.Mute ? "" : "NOT ") + "muted";
            }));
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Hidden;
            }
        }
    }

    public class ShowMessageCommand : ICommand
    {
        public ShowMessageCommand(Window window)
        {
            this.window = window;
        }

        public void Execute(object parameter)
        {
            window.Visibility = Visibility.Visible;
            window.WindowState = WindowState.Normal;
            window.Activate();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
        private Window window;
    }

}
