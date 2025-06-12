using System.Windows;

namespace BizTalkBindingsManager
{
    public partial class SetPassword : Window
    {
        public SetPassword(BindingElement element)
        {
            InitializeComponent();
            DataContext = element;
        }

        private void Ok(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
