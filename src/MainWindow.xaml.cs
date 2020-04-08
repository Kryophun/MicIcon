using Hardcodet.Wpf.TaskbarNotification;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly ObservableCollection<string> inputDeviceNames = new ObservableCollection<string>();
        private MMDevice selectedInputDevice;

        private Icon unmutedIcon;
        private Icon mutedIcon;

        TaskbarIcon tbi = new TaskbarIcon();

        IMMNotificationClient notificationClient;

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

            var showUiMenuItem = new MenuItem();
            showUiMenuItem.Header = "Show UI";
            showUiMenuItem.Click += ShowUiMenuItem_Click;

            var exitMenuItem = new MenuItem();
            exitMenuItem.Header = "Exit";
            exitMenuItem.Click += ExitMenuItem_Click;

            tbi.ContextMenu = new ContextMenu();
            tbi.ContextMenu.Items.Add(showUiMenuItem);
            tbi.ContextMenu.Items.Add(exitMenuItem);

            inputDevicesComboBox.ItemsSource = inputDeviceNames;
            inputDevicesComboBox.SelectionChanged += InputDevicesComboBox_SelectionChanged;

            notificationClient = new DeviceChangedClient(new Action(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    RefreshDeviceList("");
                }));
            }));

            var deviceEnumerator = new MMDeviceEnumerator();
            deviceEnumerator.RegisterEndpointNotificationCallback(notificationClient);

            RefreshDeviceList(Properties.Settings.Default.LastDeviceName);

            Console.WriteLine($"Selected index: {inputDevicesComboBox.SelectedIndex}");
        }

        private void ShowUiMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RefreshDeviceList(string preferredSelectedDeviceFriendlyName)
        {
            string deviceToSelectIfPossible = preferredSelectedDeviceFriendlyName;

            if (inputDevicesComboBox.SelectedIndex != -1)
            {
                deviceToSelectIfPossible = inputDevices[inputDevicesComboBox.SelectedIndex].DeviceFriendlyName;
            }

            inputDevices.Clear();
            inputDeviceNames.Clear();
            var deviceEnumerator = new MMDeviceEnumerator();
            foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                Console.WriteLine($"{device.DeviceFriendlyName}-{device.FriendlyName}-{device.State.ToString()}");
                inputDevices.Add(device);
                inputDeviceNames.Add(device.DeviceFriendlyName);
            }

            if (!string.IsNullOrWhiteSpace(deviceToSelectIfPossible))
            {
                int foundIndex = inputDeviceNames.IndexOf(deviceToSelectIfPossible);
                if (foundIndex >= 0)
                {
                    if (inputDevicesComboBox.SelectedIndex != foundIndex)
                    {
                        inputDevicesComboBox.SelectedIndex = foundIndex;
                    }
                }
            } else if (inputDeviceNames.Count > 0)
            {
                inputDevicesComboBox.SelectedIndex = 0;
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
            if (WindowState == WindowState.Minimized)
            {
                Visibility = Visibility.Hidden;
            }
        }

        private void ShowWindow()
        {
            Visibility = Visibility.Visible;
            WindowState = WindowState.Normal;
            Activate();
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

    internal class DeviceChangedClient : IMMNotificationClient
    {
        private Action _onDeviceAddedOrRemoved;

        internal DeviceChangedClient(Action onDeviceAddedOrRemoved)
        {
            _onDeviceAddedOrRemoved = onDeviceAddedOrRemoved;
        }

        //
        // Summary:
        //     Default Device Changed
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {

        }
        //
        // Summary:
        //     Device Added
        public void OnDeviceAdded(string pwstrDeviceId)
        {
            _onDeviceAddedOrRemoved();
        }
        //
        // Summary:
        //     Device Removed
        public void OnDeviceRemoved(string deviceId)
        {
            _onDeviceAddedOrRemoved();
        }
        //
        // Summary:
        //     Device State Changed
        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            _onDeviceAddedOrRemoved();
        }
        //
        // Summary:
        //     Property Value Changed
        //
        // Parameters:
        //   pwstrDeviceId:
        //
        //   key:
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {

        }
    }
}
