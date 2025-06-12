using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace BizTalkBindingsManager
{
    public class BindingElement : INotifyPropertyChanged
    {
        public BindingElement(string name, bool tfs, bool bt)
        {
            Name = name;
            TFS = tfs;
            BT = bt;

            // TODO: Create a password object for each password the BindingElement requires.
            // When there is a corresponding .pwd file, set the passwordField to "****"
            // Otherwise set the passwordField to ""
            Passwords.Add(new Password { Name = "Another Password", PasswordField = "****"  });
            Passwords.Add(new Password { Name = "A Password", PasswordField = "" });
        }

        bool selected = false;
        public bool Selected {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                NotifyPropertyChanged();
            }
        }

        public string Type { get; } = "Send Port";
        public string Name { get; set; }
        public bool TFS { get; set; }
        public bool BT { get; set; }
        public bool Synced { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<Password> Passwords { get; set; } = new List<Password>();
    }

    public class Password
    {
        public string Name { get; set; }
        public string PasswordField { get; set; }
    }
}
