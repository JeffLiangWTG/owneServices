using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace BizTalkBindingsManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = state;
        }

        private void SetPasswordButton(object sender, RoutedEventArgs e)
        {
            BindingElement element = (BindingElement)((Button)sender).DataContext;
            SetPassword window = new SetPassword(element);
            window.ShowDialog();
        }

        private void LoadTFSButton(object sender, RoutedEventArgs e)
        {
            state.LoadTFS();
        }

        private void LoadBTButton(object sender, RoutedEventArgs e)
        {
            state.LoadBT();
        }

        private void SelectAllButton(object sender, RoutedEventArgs e)
        {
            state.ToggleSelectAll();
        }

        readonly State state = new State();
    }
}
