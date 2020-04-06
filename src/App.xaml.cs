using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using System.Windows;

namespace MicIcon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            TaskbarIcon tbi = new TaskbarIcon();
            tbi.Icon = new Icon(SystemIcons.Exclamation, 40, 40);
            tbi.ToolTipText = "hello world";
        }
    }
}
