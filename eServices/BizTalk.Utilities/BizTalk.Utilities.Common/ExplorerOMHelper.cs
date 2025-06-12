using System;
using System.Management;

namespace BizTalk.Utilities.Common
{
    public static class ExplorerOMHelper
    {
        private const string CONNECTION_STRING_FORMAT = "Data Source={0};Initial Catalog={1};Integrated Security=SSPI;Connect Timeout=60;";

        /// <summary>
        /// Use WMI to get the server and databasenames of the registered group
        /// </summary>
        /// <returns>A formatted connection string to access the management database</returns>
        public static string GetConnectionString()
        {
            string serverName = string.Empty;
            string databaseName = string.Empty;

            using (ManagementObjectSearcher search = new ManagementObjectSearcher("ROOT\\MicrosoftBizTalkServer", "SELECT * FROM MSBTS_GroupSetting"))
            {
                using (ManagementObjectCollection results = search.Get())
                {
                    if (results.Count != 1)
                    {
                        throw new Exception("No single BizTalk Group could be found.  Please register only one BizTalk Group in the BizTalk management Console before continuning.");
                    }

                    foreach (ManagementObject btsGroup in results)
                    {
                        serverName = btsGroup["MgmtDbServerName"].ToString();
                        databaseName = btsGroup["MgmtDbName"].ToString();
                    }
                }
            }

            return String.Format(CONNECTION_STRING_FORMAT, serverName, databaseName);
        }
    }
}
