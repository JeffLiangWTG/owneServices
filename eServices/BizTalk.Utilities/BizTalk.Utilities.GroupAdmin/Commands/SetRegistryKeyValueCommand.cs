using System;
using System.Collections.Generic;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.Win32;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetRegistryKeyValueCommand : AbstractCommand
    {
        internal SetRegistryKeyValueCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the registry update
        /// </summary>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 4)
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

            // Check param 3 is -ValueType
            string valueTypeParam = base._rawArgs[2];
            if (!valueTypeParam.StartsWith("-ValueType:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 3 value is a valid value type
            string valueType = valueTypeParam.Split(':')[1];
            List<string> validValueTypes = new List<string>(Enum.GetNames(typeof(RegistryValueType)));
            if (!validValueTypes.Contains(valueType))
            {
                this.ReportUsage();
                return;
            }

            // Check param 4 is -Value
            string valueParam = base._rawArgs[3];
            if (!valueParam.StartsWith("-Value:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 4 value is not blank
            string value = valueParam.Split(':')[1];
            if (String.IsNullOrEmpty(value))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                // Get the root hive
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
                    if (!subKeyNames.Contains(urlParts[i]))
                    {
                        // Create the key
                        currentKey.CreateSubKey(urlParts[i]);
                    }
                    // Open subkey as writeable
                    currentKey = currentKey.OpenSubKey(urlParts[i], true);
                }

                RegistryValueType typedValueType = (RegistryValueType)Enum.Parse(typeof(RegistryValueType), valueType);
                RegistryValueKind valueKind = MapRegistryValueType(typedValueType);

                // Create/set the value
                string valueName = urlParts[urlParts.Length - 1];
                currentKey.SetValue(valueName, value, MapRegistryValueType(typedValueType));
                _output.Add(String.Format("{0}: value set to {1} ({2})", url, value, valueKind.ToString()));
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
            usage.Add(base.GetExeName() + " " + CommandType.SetRegistryKeyValue.ToString() + 
                " -Url:<Hive And Key Path> -ValueType:<Registry Value Type> -Value:<Value>");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE HIVE LOCATORS -");
            foreach (string hive in Enum.GetNames(typeof(RegistryHiveLocator)))
            {
                usage.Add(hive);
            }
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE VALUE TYPES -");
            foreach (string valueType in Enum.GetNames(typeof(RegistryValueType)))
            {
                usage.Add(valueType);
            }
            usage.Add(string.Empty);
            usage.Add("Example:");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetRegistryKeyValue.ToString() +
                @" -Url:HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BTSSvc$BizTalkServerApplication\MaxIOThreads" +
                " -ValueType:" + RegistryValueType.REG_DWORD.ToString() + " -Value:100");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }

        protected RegistryValueKind MapRegistryValueType(RegistryValueType valueType)
        {
            RegistryValueKind valueKind = RegistryValueKind.Unknown;
            switch (valueType)
            {
                case RegistryValueType.REG_DWORD: valueKind = RegistryValueKind.DWord; break;
                case RegistryValueType.REG_SZ: valueKind = RegistryValueKind.String; break;
            }
            return valueKind;
        }
    }
}
