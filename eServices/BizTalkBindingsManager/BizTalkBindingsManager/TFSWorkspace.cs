using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace BizTalkBindingsManager
{
    public class TFSWorkspace
    {
        public static ObservableCollection<TFSWorkspace> Load()
        {
            var workspaces = new ObservableCollection<TFSWorkspace>();
            var server = TfsTeamProjectCollectionFactory
                .GetTeamProjectCollection(new Uri("http://tfs.wtg.zone:8080/tfs/CargoWise"))
                .GetService<VersionControlServer>();
            var workspacesQuery = server.QueryWorkspaces(null, Environment.UserDomainName + @"\" + Environment.UserName, Environment.MachineName);
            foreach (var workspace in workspacesQuery)
            {
                foreach (var workingFolder in workspace.Folders)
                {
                    if (workingFolder.ServerItem == "$/eServices")
                    {
                        workspaces.Add(new TFSWorkspace(workspace.Name, workingFolder.LocalItem));
                    }
                }
            }

            return workspaces;
        }

        public TFSWorkspace(string name, string eServicesFolder)
        {
            Name = name;
            EservicesFolder = eServicesFolder;
        }

        public List<BindingElement> LoadSendPorts()
        {
            var sendPorts = new List<BindingElement>();
            var sendPortsDir = EservicesFolder + @"\Operation\Bindings\SendPorts";

            foreach (var filePath in Directory.EnumerateFiles(sendPortsDir, "*.xml"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                sendPorts.Add(new BindingElement(fileName, true, false));
            }
            return sendPorts;
        }

        public List<BindingElement> LoadReceivePorts()
        {
            var receivePorts = new List<BindingElement>();
            var receivePortsDir = EservicesFolder + @"\Operation\Bindings\ReceivePorts";
            foreach (var filePath in Directory.EnumerateFiles(receivePortsDir, "*.xml"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                receivePorts.Add(new BindingElement(fileName, true, false));
            }
            return receivePorts;
        }

        public List<BindingElement> LoadOrchestrations()
        {
            var orchestrations = new List<BindingElement>();
            var orchestrationsDir = EservicesFolder + @"\Operation\Bindings\Orchestrations";
            foreach (var filePath in Directory.EnumerateFiles(orchestrationsDir, "*.xml"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                orchestrations.Add(new BindingElement(fileName, true, false));
            }
            return orchestrations;
        }

        public List<BindingElement> LoadParties()
        {
            var parties = new List<BindingElement>();
            var partiesDir = EservicesFolder + @"\Operation\Bindings\Parties";
            foreach (var filePath in Directory.EnumerateFiles(partiesDir, "*.xml"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                parties.Add(new BindingElement(fileName, true, false));
            }
            return parties;
        }

        public List<BindingElement> LoadAgreements()
        {
            var agreements = new List<BindingElement>();
            var agreementsDir = EservicesFolder + @"\Operation\Bindings\Agreements";
            foreach (var filePath in Directory.EnumerateFiles(agreementsDir, "*.xml"))
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                agreements.Add(new BindingElement(fileName, true, false));
            }
            return agreements;
        }

        public string Name { get; set; }
        public string EservicesFolder { get; set; }
    }
}
