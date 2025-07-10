using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.VersionReport
{
	public static class VersionReportBuilder
	{
		public static VersionReport CreateCurrent(string licenceUsage, bool isLicenceUsageRequest)
		{
			return CreateVersionReport(ZDateTime.UtcNow, ReleaseInfo.Instance.VersionNumber.ToString(), true, isLicenceUsageRequest, licenceUsage);
		}

		public static VersionReport CreateDelivered(string deliveredVersion)
		{
			return CreateVersionReport(ZDateTime.UtcNow, deliveredVersion, false, false, null);
		}

		public static class VersionReportType
		{
			public const string CurrentVersionReport = "CurrentVersionReport";
			public const string DeliveredVersionReport = "DeliveredVersionReport";
		}

		static List<String> GetAdditionalDatabaseServerInfoList()
		{
			List<String> list = new List<string>();

			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					try
					{
						var serverInfo = GetDbServerSystemInfo(adminConnection);
						list.AddRange(serverInfo);
						list.AddRange(GetDbConfigInfo(adminConnection));
					}
					catch (SqlException)
					{
						list.Clear();
						list.AddRange(GetDbServerSystemInfo(adminConnection));
						list.AddRange(GetDbConfigInfo(adminConnection));
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException()) { }

			return list;
		}

		static IEnumerable<string> GetDbConfigInfo(AdminConnection adminConnection)
		{
			var result = string.Empty;
			using (var command = adminConnection.Command($"EXEC ep_DbConfigInfo '{adminConnection.CurrentDatabase}'"))
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					foreach (VersionReport.DBCONFIG_INFO_FIELDS field in Enum.GetValues(typeof(VersionReport.DBCONFIG_INFO_FIELDS)))
					{
						var fieldPosition = (int)field;
						if (VersionReport.DbConfigFieldKeys.ContainsKey(field) && fieldPosition < reader.FieldCount && reader[fieldPosition] != null)
						{
							var fieldName = VersionReport.DbConfigFieldKeys[field];
							var value = fieldName + ": " + reader[fieldPosition];
							yield return value;
						}
					}
				}
			}
		}

		static IEnumerable<string> GetDbServerSystemInfo(AdminConnection adminConnection)
		{
			using (var command = adminConnection.Command("EXEC ep_SystemInfo"))
			{
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						foreach (VersionReport.VERSION_INFO_FIELDS field in Enum.GetValues(typeof(VersionReport.VERSION_INFO_FIELDS)))
						{
							int fieldPosition = (int)field;
							if (VersionReport.VersionFieldKeys.ContainsKey(field) && fieldPosition < reader.FieldCount && reader[fieldPosition] != null)
							{
								string fieldName = VersionReport.VersionFieldKeys[field];
								string value = fieldName + ": " + reader[fieldPosition];

								// There's one special case regarding the processor description, which is composed of 2 values
								if (field == VersionReport.VERSION_INFO_FIELDS.ProcessorDescription)
								{
									var vendorPosition = (int)VersionReport.VERSION_INFO_FIELDS.ProcessorVendor;
									if (vendorPosition < reader.FieldCount && reader[vendorPosition] != null)
									{
										value += " " + reader[vendorPosition];
									}
								}

								yield return value;
							}
						}
					}
				}
			}
		}

		static VersionReport CreateVersionReport(ZDateTime dateTimeToUse,
			string versionText,
			bool isCurrent,
			bool isLicenceUsageRequest,
			string licenceUsage)
		{
			Enterprise.ZArchitecture.Environment.RegistryItemDictionary.Instance.PurgeAll();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			return new VersionReport(registrationKey, isCurrent,
				Db.Connection.ServerNameReportedByDatabase,
				Db.DatabaseName,
				versionText,
				isCurrent ? ReleaseInfo.Instance.ReleaseRing : "",
				dateTimeToUse,
				(isCurrent && !isLicenceUsageRequest) ? GetAdditionalDatabaseServerInfoList() : null,
				licenceUsage);
		}
	}
}
