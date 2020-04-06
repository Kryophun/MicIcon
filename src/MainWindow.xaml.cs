using Hardcodet.Wpf.TaskbarNotification;
using NAudio.CoreAudioApi;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace MicIcon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NAudio.CoreAudioApi.MMDeviceEnumerator deviceEnum = new NAudio.CoreAudioApi.MMDeviceEnumerator();
        private NotificationClientImplementation notificationClient;
        private NAudio.CoreAudioApi.Interfaces.IMMNotificationClient notifyClient;

        public MainWindow()
        {
            InitializeComponent();
            this.StateChanged += MainWindow_StateChanged;

            TaskbarIcon tbi = new TaskbarIcon();
            tbi.Icon = new Icon(SystemIcons.Exclamation, 40, 40);
            tbi.ToolTipText = "hello world";
            tbi.LeftClickCommand = new ShowMessageCommand(this);

            notificationClient = new NotificationClientImplementation();
            notifyClient = (NAudio.CoreAudioApi.Interfaces.IMMNotificationClient)notificationClient;
            deviceEnum.RegisterEndpointNotificationCallback(notifyClient);
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

    class NotificationClientImplementation : NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
    {
        public void OnDefaultDeviceChanged(DataFlow dataFlow, Role deviceRole, string defaultDeviceId)
        {
            //Do some Work
            Console.WriteLine("OnDefaultDeviceChanged --> {0}", dataFlow.ToString());
        }

        public void OnDeviceAdded(string deviceId)
        {
            //Do some Work
            Console.WriteLine("OnDeviceAdded -->");
        }

        public void OnDeviceRemoved(string deviceId)
        {

            Console.WriteLine("OnDeviceRemoved -->");
            //Do some Work
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            Console.WriteLine("OnDeviceStateChanged\n Device Id -->{0} : Device State {1}", deviceId, newState);
            MessageBox.Show("Hello!");
            //Do some Work
        }

        public NotificationClientImplementation()
        {
            //_realEnumerator.RegisterEndpointNotificationCallback();
            if (System.Environment.OSVersion.Version.Major < 6)
            {
                throw new NotSupportedException("This functionality is only supported on Windows Vista or newer.");
            }
        }

        public void OnPropertyValueChanged(string deviceId, PropertyKey propertyKey)
        {
            //Do some Work
            //fmtid & pid are changed to formatId and propertyId in the latest version NAudio
            Console.WriteLine("OnPropertyValueChanged: formatId --> {0}  propertyId --> {1}", propertyKey.formatId.ToString(), propertyKey.propertyId.ToString());
        }
    }
}
