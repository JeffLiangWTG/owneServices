using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class SRDbUpdaterRegistration : ISRDbUpdaterRegistration
	{
		public ISRDbDataSetUpdater[] Get(IServerProxy proxy, IDBHelper dbHelper, IErrorReportingClientWrapper errorReportingWrapper, ILogger logger)
		{
			try
			{
				var versionControlManager = new RefVersionControlManager(dbHelper);
				versionControlManager.IsMainDb = false;
				var scriptVersion = 0;
				int.TryParse(dbHelper.LoadDbExtendedProperty(RefDataBaseUpgrader.RefDbVersionExtendedPropertyName, null), out scriptVersion);
				AllDataSetScripts allscripts;
				using (var client = proxy.CreateHttpClient())
				{
					allscripts = Task.Run(() => proxy.GetAllDataSetScripts(client, scriptVersion)).GetAwaiter().GetResult();
				}
				return allscripts.Scripts.Select(x => new SRDbDataSetUpdater(x, allscripts.DataVersion, dbHelper, proxy, versionControlManager, logger)).ToArray();
			}
			catch (Exception ex)
			{
				logger.WriteError(ex.Message);
				if (errorReportingWrapper.PostCrashReport(ex))
				{
					logger.WriteLine((NoResString)"Error report sent");
				}
				return null;
			}
		}
	}
}
