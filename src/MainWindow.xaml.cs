using Hardcodet.Wpf.TaskbarNotification;
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
        public MainWindow()
        {
            InitializeComponent();
            this.StateChanged += MainWindow_StateChanged;

            TaskbarIcon tbi = new TaskbarIcon();
            tbi.Icon = new Icon(SystemIcons.Exclamation, 40, 40);
            tbi.ToolTipText = "hello world";
            tbi.LeftClickCommand = new ShowMessageCommand(this);
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Hidden;
            } else
            {
                MessageBox.Show("New state: " + this.WindowState.ToString());
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
            // MessageBox.Show(parameter.ToString());
            // MessageBox.Show("Thanks for clicking on me!");
            window.Visibility = Visibility.Visible;
            // window
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
        private Window window;
    }
}
