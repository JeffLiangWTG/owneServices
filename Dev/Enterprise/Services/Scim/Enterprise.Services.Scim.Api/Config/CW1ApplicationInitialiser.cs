using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWiseOne.WebInfrastructure;
using Enterprise.Initialisation;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Services.Scim.Api.Config
{
	static class CW1ApplicationInitialiser
	{
		static Regex IISAppDomainNamePattern => new Regex("\\/LM\\/W3SVC\\/(?<site>\\d+)\\/(?<path>.+)-\\d+-\\d+$", RegexOptions.IgnoreCase);

		public static void Initialise(IAppSettings configurations)
		{
			InitializeDbConnection(configurations);

			if (!IsHostedInIIS())
			{
				Globals.IsConsoleSession = true;
				Initialiser.InitialiseServiceManager(null, false); // To be able to set temporary user context while on self hosted mode.
			}
			else
			{
				WebInitialiser.Initialise(enableErrorReport: true, new WebServicesEnvironmentProvider());
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1018:HttpApplicationRule", Justification = "OWIN is used, so EnterpriseHttpApplication.Application_Start is not going to be called.")]
		static void InitializeDbConnection(IAppSettings configurations)
		{
			DbConnection.ApplicationName = "ScimService";

			if (!IsHostedInIIS())
			{
				ValidateServerNameDbName(configurations);

				Db.InitializeDatabaseDetails(configurations.ServerName, configurations.DatabaseName, ApplicationType.Web);

				return;
			}

			var config = WebDbConfiguration.GetCurrentConfiguration();
			if (!string.IsNullOrEmpty(config.ServerName) && !string.IsNullOrEmpty(config.DatabaseName))
			{
				Db.InitializeDatabaseDetails(config.ServerName, config.DatabaseName, ApplicationType.Web);
			}
			else
			{
				ValidateServerNameDbName(configurations);

				Db.InitializeDatabaseDetails(configurations.ServerName, configurations.DatabaseName, ApplicationType.Web);
			}
		}

		static void ValidateServerNameDbName(IAppSettings configurations)
		{
			if (string.IsNullOrEmpty(configurations.ServerName) || string.IsNullOrEmpty(configurations.DatabaseName))
			{
				throw new ArgumentNullException(nameof(configurations), "ServerName and DatabaseName configuration not provided");
			}
		}

		internal static bool IsHostedInIIS()
		{
			return IISAppDomainNamePattern.IsMatch(AppDomain.CurrentDomain.FriendlyName);
		}
	}
}
