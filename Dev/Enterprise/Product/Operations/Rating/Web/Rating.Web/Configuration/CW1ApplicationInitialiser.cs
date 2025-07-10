using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CargoWise.Data;
using Enterprise.Initialisation;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Web.Configuration
{
	static class CW1ApplicationInitialiser
	{
		static Regex IISAppDomainNamePattern => new Regex("\\/LM\\/W3SVC\\/(?<site>\\d+)\\/(?<path>.+)-\\d+-\\d+$", RegexOptions.IgnoreCase);

		public static void Initialise(IRatesAPIsAppSettings configurations)
		{
			InitializeDbConnection(configurations);

			if (!IsHostedInIIS())
			{
				Globals.IsConsoleSession = true;
				Initialiser.InitialiseServiceManager(null, false); // To be able to set temporary user context while on self hosted mode.
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1018:HttpApplicationRule", Justification = "OWIN is used, so EnterpriseHttpApplication.Application_Start is not going to be called.")]
		static void InitializeDbConnection(IRatesAPIsAppSettings configurations)
		{
			DbConnection.ApplicationName = "RatingWebServices";

			if (!IsHostedInIIS())
			{
				if (!string.IsNullOrEmpty(configurations.ServerName) && !string.IsNullOrEmpty(configurations.DatabaseName))
				{
					Db.InitializeDatabaseDetails(configurations.ServerName, configurations.DatabaseName, CargoWise.DataProtection.ApplicationType.Web);
				}
				return;
			}
		}

		internal static bool IsHostedInIIS()
		{
			return IISAppDomainNamePattern.IsMatch(AppDomain.CurrentDomain.FriendlyName);
		}
	}
}
