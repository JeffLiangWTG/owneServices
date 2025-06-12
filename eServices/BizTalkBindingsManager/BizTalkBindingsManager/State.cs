using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BizTalkBindingsManager
{
    public class State : INotifyPropertyChanged
    {
        public State()
        {
            UpdateSelectSummary();

            // Restore last used TFSWorkspace
            var name = Properties.Settings.Default.TFSWorkspace;
            foreach (var workspace in TFSWorkspaces)
            {
                if (workspace.Name == name)
                {
                    TFSWorkspace = workspace;
                }
            }
        }

        public void LoadTFS()
        {
            if (TFSWorkspace != null)
            {
                ClearTFS(SendPorts);
                ClearTFS(ReceivePorts);
                ClearTFS(Orchestrations);
                ClearTFS(Parties);
                ClearTFS(Agreements);

                CollateBindingElements(SendPorts, TFSWorkspace.LoadSendPorts());
                CollateBindingElements(ReceivePorts, TFSWorkspace.LoadReceivePorts());
                CollateBindingElements(Orchestrations, TFSWorkspace.LoadOrchestrations());
                CollateBindingElements(Parties, TFSWorkspace.LoadParties());
                CollateBindingElements(Agreements, TFSWorkspace.LoadAgreements());

                Properties.Settings.Default.TFSWorkspace = TFSWorkspace.Name;
                Properties.Settings.Default.Save();
            }
        }

        public void LoadBT()
        {
            ClearBT(SendPorts);
            ClearBT(ReceivePorts);
            ClearBT(Orchestrations);
            ClearBT(Parties);
            ClearBT(Agreements);

            // CollateBindingElements(SendPorts, biztalk.LoadSendPorts()); // TODO
        }

        void ClearTFS(ObservableCollection<BindingElement> elements)
        {
            // Remove all TFS data from elements
            foreach (var element in elements)
            {
                element.TFS = false;
                element.Synced = false;
            }

            // Remove any elements that no longer belong to TFS or BT.
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (!elements[i].BT) {
                    elements.RemoveAt(i);
                }
            }
        }

        void ClearBT(ObservableCollection<BindingElement> elements)
        {
            // Remove all BT data from elements
            foreach (var element in elements)
            {
                element.BT = false;
                element.Synced = false;
            }

            // Remove any elements that no longer belong to TFS or BT.
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (!elements[i].TFS) {
                    elements.RemoveAt(i);
                }
            }
        }

        void CollateBindingElements(ObservableCollection<BindingElement> destination, List<BindingElement> source)
        {
            foreach (var element in source)
            {
                var matchFound = false;
                foreach (var elementDest in destination)
                {
                    if (elementDest.Name == element.Name)
                    {
                        matchFound = true;
                        // elementDest.Combine(element); // TODO
                        break;
                    }
                }

                if (!matchFound)
                {
                    element.PropertyChanged += new PropertyChangedEventHandler(BindingElement_PropertyChanged);
                    destination.Add(element);
                }
            }
        }

        public void ToggleSelectAll()
        {
            if (AllBindingElements().All(element => element.Selected))
            {
                foreach(var element in AllBindingElements())
                {
                    element.Selected = false;
                }
            }
            else
            {
                foreach(var element in AllBindingElements())
                {
                    element.Selected = true;
                }
            }
        }

        IEnumerable<BindingElement> AllBindingElements()
        {
            return SendPorts.Concat(ReceivePorts).Concat(Orchestrations).Concat(Parties).Concat(Agreements);
        }

        private void BindingElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateSelectSummary();
        }

        void UpdateSelectSummary()
        {
            var sendPortCount = SendPorts.Where(element => element.Selected).Count();
            var receivePortCount = ReceivePorts.Where(element => element.Selected).Count();
            var orchestrationCount = Orchestrations.Where(element => element.Selected).Count();
            var partyCount = Parties.Where(element => element.Selected).Count();
            var agreementCount = Agreements.Where(element => element.Selected).Count();

            SelectSummary = string.Format("Selected - Send Ports: {0}, Receive Ports {1}, Orchestrations {2}, Parties {3}, Agreements {4}", sendPortCount, receivePortCount, orchestrationCount, partyCount, agreementCount);
            NotifyPropertyChanged("SelectSummary");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string BTServer {
            get
            {
                return Properties.Settings.Default.BTAddress;
            }
            set {
                Properties.Settings.Default.BTAddress = value;
                Properties.Settings.Default.Save();
            }
        }

        public string SelectSummary { get; private set; } = "";

        public TFSWorkspace TFSWorkspace { get; set; }
        public ObservableCollection<TFSWorkspace> TFSWorkspaces { get; set; } = TFSWorkspace.Load();
        public ObservableCollection<BindingElement> SendPorts { get; set; } = new ObservableCollection<BindingElement>();
        public ObservableCollection<BindingElement> ReceivePorts { get; set; } = new ObservableCollection<BindingElement>();
        public ObservableCollection<BindingElement> Orchestrations { get; set; } = new ObservableCollection<BindingElement>();
        public ObservableCollection<BindingElement> Parties { get; set; } = new ObservableCollection<BindingElement>();
        public ObservableCollection<BindingElement> Agreements { get; set; } = new ObservableCollection<BindingElement>();
    }
}
