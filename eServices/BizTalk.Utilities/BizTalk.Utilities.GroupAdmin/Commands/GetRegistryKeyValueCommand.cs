using System;
using System.Collections.Generic;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.Win32;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class GetRegistryKeyValueCommand : AbstractCommand
    {
        internal GetRegistryKeyValueCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the registry query
        /// </summary>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 2)
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 is -Url
            string urlParam = base._rawArgs[1]; 
            if (!urlParam.StartsWith("-Url:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 value is not blank
            string url = urlParam.Split(':')[1];
            if (String.IsNullOrEmpty(url))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 value is recognised
            List<string> validHives = new List<string>(Enum.GetNames(typeof(RegistryHiveLocator)));
            string[] urlParts = url.Split(new char[]{'\\','/'});
            if (urlParts.Length < 1 || !validHives.Contains(urlParts[0]))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                // Get the root hive
                bool branchKeyFound = true;
                RegistryHiveLocator rootHive = (RegistryHiveLocator)Enum.Parse(typeof(RegistryHiveLocator), urlParts[0]);
                RegistryKey rootKey = null;
                switch (rootHive)
                {
                    case RegistryHiveLocator.HKEY_CLASSES_ROOT: rootKey = Registry.ClassesRoot; break;
                    case RegistryHiveLocator.HKEY_CURRENT_CONFIG: rootKey = Registry.CurrentConfig; break;
                    case RegistryHiveLocator.HKEY_CURRENT_USER: rootKey = Registry.CurrentUser; break;
                    case RegistryHiveLocator.HKEY_LOCAL_MACHINE: rootKey = Registry.LocalMachine; break;
                    case RegistryHiveLocator.HKEY_USERS: rootKey = Registry.Users; break;
                }

                // Iterate and traverse the tree through the sub keys
                RegistryKey currentKey = rootKey;
                for (int i = 1; i < urlParts.Length - 1; i++)
                {
                    // Does the key exist?
                    List<string> subKeyNames = new List<string>(currentKey.GetSubKeyNames());
                    if (subKeyNames.Contains(urlParts[i]))
                    {
                        currentKey = currentKey.OpenSubKey(urlParts[i]);
                    }
                    else
                    {
                        branchKeyFound = false;
                        break;
                    }
                }

                if (branchKeyFound)
                {
                    // Does the key value exist?
                    string valueName = urlParts[urlParts.Length - 1];
                    List<string> valueNames = new List<string>(currentKey.GetValueNames());
                    if (valueNames.Contains(valueName))
                    {
                        //string value = string.Empty;
                        object rawValue = currentKey.GetValue(valueName);
                        RegistryValueKind valueKind = currentKey.GetValueKind(valueName);
                        _output.Add(String.Format("{0}: {1} ({2})", url, rawValue.ToString(), valueKind.ToString()));
                    }
                    else
                    {
                        branchKeyFound = false;
                    }
                }
                
                if (!branchKeyFound)
                {
                    _output.Add(String.Format("{0} value not found!", url));
                }
            }
            catch (Exception ex)
            {
                base.InitialiseOutput(OutputInitialiseOption.ForError);
                _output.Add(ex.ToString());
            }
        }

        /// <summary>
        /// Generate the usage to display to the user
        /// </summary>
        protected override void ReportUsage()
        {
            List<string> usage = new List<string>();
            usage.Add(base.GetExeName() + " " + CommandType.GetRegistryKeyValue.ToString() + " -Url:<Hive And Key Path>");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE HIVE LOCATORS -");
            foreach (string hive in Enum.GetNames(typeof(RegistryHiveLocator)))
            {
                usage.Add(hive);
            }
            usage.Add(string.Empty);
            usage.Add("Example:");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.GetRegistryKeyValue.ToString() +
                @" -Url:HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTSSvc$BizTalkServerApplication\MaxIOThreads");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
