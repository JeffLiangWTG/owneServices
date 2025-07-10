using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using WTG.StaticAnalysis.Annotation;
using static CargoWise.Definitions.LicenceFeatureCodeList;

namespace Enterprise.MasterFiles.Business.VersionReport
{
	public class VersionReport
	{
		#region Construction / Extraction of Xml Data

		public VersionReport(IProductRegistrationKey regKey, bool isVerbose,
			string dbServerName, string dbName, string currentVersion, string releaseRing, ZDateTime currentDate, List<String> additionalInfoList, string licenceUsage)
		{
			this.additionalDatabaseSystemInfoList = additionalInfoList;
			this.licenceUsage = licenceUsage;

			this.databaseNumber = regKey.DatabaseNumber;
			this.enterpriseCode = regKey.EnterpriseCode;
			this.physicalServerID = regKey.ServerCode;
			this.dbServerName = dbServerName;
			this.dbName = dbName;
			this.currentVersion = currentVersion;
			this.currentDate = currentDate;
			this.currentRelease = ReleaseInfo.GetReleaseDisplayText(releaseRing, new VersionNumber(currentVersion));
			this.preferredUpgradeMethod = FindPreferredUpgradeMethod();
			this.dbServerSecurityMode = FindDBServerSecurityMode();
			this.sqlServerName = FindSQLServerName(dbServerName);
			this.sqlServerInstanceName = FindSQLInstanceName(dbServerName);
			this.internalPOP3EmailAddress = Env.Registry.MailboxEmailAddress;
			this.internalPOP3UserName = Env.Registry.MailboxUserName;
			this.internalPOP3MailServer = Env.Registry.MailServer;
			this.internalMailServerPort = Env.Registry.MailServerPort;
			this.internalSMTPMailServer = Env.Registry.SMTPServer;
			this.internalSMTPPort = Env.Registry.SMTPPort;
			this.GetAllLogAndDataFilesForAllRelatedDatabases(LogAndDataFiles);
			if (isVerbose)
			{
				AddCompanies(LoadCompaniesExcludingDemo());
			}
			this.databaseBackupPath = Env.Registry.BackupDirectoryPath;
			this.encryptedSystemExpirationKey = Env.Registry.LegacyEncryptedSystemRegistrationKey;
			this.encryptedRegistrationKey = Env.Registry.RawRegistry.EncryptedRegistrationKey.Value;
			GetDocumentEngineStats();
			this.outboundEAdaptorUrl = eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.Value;
			this.tokenAuthenticationEnabled = SystemDataRegistry.Instance.OIDCConfig.Value.IsOIDCEnabled;
			GetUpgradeScheduleInfo();
			this.featureControlRule = WebDataRegistry.Instance.FeatureControlRuleContent.Value;
			this.supportedFeatureCodes = PopulateSupportedFeatureCodes();
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory() { RefreshEnabled = false }); }
		}
		BusinessObjectFactory factory;

		#region Basic Version Only Report

		public static VersionReport CreateBasic(IProductRegistrationKey regKey, string dbServerName, string dbName, string currentVersion, ZDateTime currentTimeUtc)
		{
			return new VersionReport(regKey, dbServerName, dbName, currentVersion, currentTimeUtc);
		}

		protected VersionReport(IProductRegistrationKey regKey, string dbServerName, string dbName, string currentVersion, ZDateTime currentTimeUtc)
		{
			this.databaseNumber = regKey.DatabaseNumber;
			this.enterpriseCode = regKey.EnterpriseCode;
			this.physicalServerID = regKey.ServerCode;
			this.dbServerName = dbServerName;
			this.dbName = dbName;
			this.currentVersion = currentVersion;
			this.currentDate = currentTimeUtc;
			this.sqlServerName = FindSQLServerName(dbServerName);
			this.sqlServerInstanceName = FindSQLInstanceName(dbServerName);
			this.encryptedRegistrationKey = Env.Registry.RawRegistry.EncryptedRegistrationKey.Value;
			isBasic = true;
		}

		readonly bool isBasic;

		#endregion

#if DEBUG
		public static VersionReport CreateForTest(string xml)
		{
			return new VersionReport(xml);
		}

		protected VersionReport(string xmlData)
		{
			ExtractVersionInfoForTest(xmlData);
		}

		void ExtractVersionInfoForTest(ZString xmlData)
		{
			try
			{
				using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlData)))
				using (XmlTextReader xmlParser = new XmlTextReader(stream))
				{
					while (xmlParser.Read())
					{
						HandleDatabaseNumber(xmlParser);
						HandleEnterpriseCode(xmlParser);
						HandlePhysicalServerID(xmlParser);
						HandleDBServerName(xmlParser);
						HandleDBName(xmlParser);
						HandleCurrentVersion(xmlParser);
						HandleCurrentDate(xmlParser);
						HandleCurrentRelease(xmlParser);
						HandlePreferredUpgradeMethod(xmlParser);
						HandleDBServerSecurityMode(xmlParser);
						HandlePublicEmailAddressForUpdate(xmlParser);
						HandleSQLServerName(xmlParser);
						HandleSQLServerInstanceName(xmlParser);
						HandlePOP3(xmlParser);
						HandleSMTP(xmlParser);
						HandleDatabaseFileNameList(xmlParser);
						HandleCompanyList(xmlParser);
						HandleDatabaseBackupPath(xmlParser);
						HandleEncryptedSystemExpirationKey(xmlParser);
						HandleEncryptedRegistrationKey(xmlParser);
						HandleDocEnginePrintingStats(xmlParser);
						HandleSqlServerVersionDetails(xmlParser);
						HandleAdditionalDatabaseSystemInfoList(xmlParser);
						HandleLicenceUsage(xmlParser);
						HandleTokenAuthentication(xmlParser);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string message = xmlData.IsEmpty ? "xmlData is empty." : string.Format("xmlData is not valid.\r\n\r\nxmlData:\r\n\r\n{0}", xmlData);
				throw new InvalidOperationException(message, ex);
			}
		}

		#region Node Handlers

		void HandleDatabaseNumber(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DatabaseNumber)
			{
				int.TryParse(xmlParser.ReadString(), out databaseNumber);
			}
		}

		void HandleEnterpriseCode(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.EnterpriseCode)
			{
				enterpriseCode = xmlParser.ReadString();
			}
		}

		void HandlePhysicalServerID(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.PhysicalServerID)
			{
				physicalServerID = xmlParser.ReadString();
			}
		}

		void HandleDBServerName(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DBServerName)
			{
				dbServerName = xmlParser.ReadString();
			}
		}

		void HandleDBName(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DBName)
			{
				dbName = xmlParser.ReadString();
			}
		}

		void HandleCurrentVersion(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.CurrentVersion)
			{
				currentVersion = xmlParser.ReadString();
			}
		}

		void HandleCurrentDate(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.CurrentDate)
			{
				var s = xmlParser.ReadString();
				DateTime result;
				if (DateTime.TryParseExact(s, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result))
				{
					currentDate = result;
				}
				else
				{
					ZDateTime.TryParseISO8601Date(s, out currentDate);
					currentDate = new ZDateTime(currentDate, DateTimeKind.Utc);
				}
			}
		}

		void HandleCurrentRelease(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.CurrentRelease)
			{
				currentRelease = xmlParser.ReadString();
			}
		}

		void HandlePreferredUpgradeMethod(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.PreferredUpgradeMethod)
			{
				preferredUpgradeMethod = xmlParser.ReadString();
			}
		}

		void HandleDBServerSecurityMode(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DBServerSecurityMode)
			{
				dbServerSecurityMode = xmlParser.ReadString();
			}
		}

		void HandlePublicEmailAddressForUpdate(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.PublicEmailAddressForUpdate)
			{
				internalPOP3EmailAddress = xmlParser.ReadString(); // Deprecated PublicEmailAddressForUpdate XML Tag. Remains for backwards compatibility
			}
		}

		void HandleSQLServerName(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.SQLServerName)
			{
				sqlServerName = xmlParser.ReadString();
			}
		}

		void HandleSQLServerInstanceName(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.SQLServerInstanceName)
			{
				sqlServerInstanceName = xmlParser.ReadString();
			}
		}

		void HandlePOP3(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.POP3)
			{
				do
				{
					xmlParser.Read();
					switch (xmlParser.Name)
					{
						case ElementNames.MailboxEmailAddress:
							internalPOP3EmailAddress = xmlParser.ReadString();
							break;
						case ElementNames.UserName:
							internalPOP3UserName = xmlParser.ReadString();
							break;
						case ElementNames.MailServer:
							internalPOP3MailServer = xmlParser.ReadString();
							break;
						case ElementNames.Port:
							internalMailServerPort = ZInt.Parse(xmlParser.ReadString());
							break;
					}
				}
				while (xmlParser.Name != ElementNames.POP3);
			}
		}

		void HandleSMTP(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.SMTP)
			{
				do
				{
					xmlParser.Read();
					switch (xmlParser.Name)
					{
						case ElementNames.MailServer:
							internalSMTPMailServer = xmlParser.ReadString();
							break;
						case ElementNames.Port:
							internalSMTPPort = ZInt.Parse(xmlParser.ReadString());
							break;
					}
				}
				while (xmlParser.Name != ElementNames.SMTP);
			}
		}

		void HandleDatabaseFileNameList(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DBFileNameList)
			{
				do
				{
					xmlParser.Read();
					if (xmlParser.Name == ElementNames.DBFileName)
					{
						LogAndDataFiles.Add(xmlParser.ReadString());
					}
				}
				while (xmlParser.Name != ElementNames.DBFileNameList);
			}
		}

		void HandleCompanyList(XmlTextReader parser)
		{
			if (parser.Name == ElementNames.CompanyList)
			{
				do
				{
					parser.Read();
					if (parser.Name == ElementNames.Company)
					{
						var co = new CompanyReport();
						co.Parse(parser);
						CompanyList.Add(co);
					}
				}
				while (parser.Name != ElementNames.CompanyList);
			}
		}

		void HandleDatabaseBackupPath(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DatabaseBackupPath)
			{
				databaseBackupPath = xmlParser.ReadString();
			}
		}

		void HandleEncryptedSystemExpirationKey(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.EncryptedSystemExpirationKey)
			{
				encryptedSystemExpirationKey = xmlParser.ReadString();
			}
		}

		void HandleEncryptedRegistrationKey(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.EncryptedRegistrationKey)
			{
				encryptedRegistrationKey = xmlParser.ReadString();
			}
		}

		void HandleDocEnginePrintingStats(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.DocEngineStats)
			{
				do
				{
					xmlParser.Read();
					if (xmlParser.Name == ElementNames.DocEngineActivePrintersCount)
					{
						activePrintersCount = ZInt.Parse(xmlParser.ReadString());
					}
				}
				while (xmlParser.Name != ElementNames.DocEngineStats);
			}
		}

		public DbConnection.SqlServerEdition SqlServerEdition
		{
			get { return sqlServerEdition; }
		}
		DbConnection.SqlServerEdition sqlServerEdition;

		public ZString SqlServerFullVersionText
		{
			get { return sqlServerFullVersionText; }
		}
		ZString sqlServerFullVersionText;

		public string SqlServerVersion
		{
			get { return sqlServerVersion; }
		}
		string sqlServerVersion;

		void HandleSqlServerVersionDetails(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.SqlServerVersionDetails)
			{
				xmlParser.ReadStartElement();
				xmlParser.ReadElementString(); // placeholder for defunct Architecture data. Needs to remain to support old systems (forever)
				string sqlServerEditionText = xmlParser.ReadElementString();
				switch (sqlServerEditionText)
				{
					// Check for old enum values (changed in WI00020517)
					case "Enterprise":
					case "Developer":
						sqlServerEdition = DbConnection.SqlServerEdition.EnterpriseDeveloper;
						break;
					case "Standard":
						sqlServerEdition = DbConnection.SqlServerEdition.StandardWorkgroup;
						break;
					case "Desktop":
						sqlServerEdition = DbConnection.SqlServerEdition.Express;
						break;
					default:
						sqlServerEdition = (DbConnection.SqlServerEdition)Enum.Parse(typeof(DbConnection.SqlServerEdition), sqlServerEditionText, true);
						break;
				}

				var xmlVersionString = xmlParser.ReadElementString();
				sqlServerVersion = SqlServerVersionNumber.SupportedVersions.Any(x => x.Generation.Name.Equals(xmlVersionString, StringComparison.OrdinalIgnoreCase))
					? xmlVersionString
					: "Other";

				sqlServerFullVersionText = xmlParser.ReadElementString();
				xmlParser.ReadEndElement();
			}
		}

		void HandleAdditionalDatabaseSystemInfoList(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.AdditionalDatabaseSystemInfoList)
			{
				do
				{
					xmlParser.Read();
					if (xmlParser.Name == ElementNames.AdditionalDatabaseSystemInfo)
					{
						AdditionalDatabaseSystemInfoList.Add(xmlParser.ReadString());
					}
				}
				while (xmlParser.Name != ElementNames.AdditionalDatabaseSystemInfoList);
			}
		}

		void HandleLicenceUsage(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.LicenceUsage)
			{
				licenceUsage = xmlParser.ReadString();
			}
		}

		void HandleTokenAuthentication(XmlTextReader xmlParser)
		{
			if (xmlParser.Name == ElementNames.TokenAuthenticationEnabled
				&& ZBool.TryParse(xmlParser.ReadString(), out var enabled))
			{
				tokenAuthenticationEnabled = enabled;
			}
		}

		#endregion
#endif

		#endregion

		#region System Information

		#region Fields retrieved from database

		// Fields returned by call to ep_SystemInfo
		public enum VERSION_INFO_FIELDS
		{
			SystemManufacturer = 0,
			BiosReleaseDate = 1,
			PhysicalMemoryMb = 2,
			NumberOfProcessors = 3,
			NumberOfLogicalProcessors = 4,
			ProcessorDescription = 5,
			ProcessorVendor = 6,
			ProcessorSpeedMhz = 7,
			OsName = 8,
			OsVersion = 9,
			IsOrHasVirtualMachine = 10,
			VirtualMachineType = 11
		}

		// Fields returned by call to ep_DbConfigInfo
		public enum DBCONFIG_INFO_FIELDS
		{
			MAXDOP = 0,
			CostOfParallelism = 1,
			OptimizeForAdHocWorkloads = 2,
			MinMemory = 3,
			MaxMemory = 4,
			TotalAvailableMemory = 5,
			TraceFlags = 6,
			DbAlwaysOn = 7,
			ParameterizationForced = 8,
			AutoCreate = 9,
			AutoUpdate = 10,
			AutoUpdateAsync = 11,
			CompatibilityLevel = 12,
			Cardinality = 13
		}

		#region SuppressResourceStringsCheckRegion

		// Keys to present the fields returned
		public const string OSNameKey = "OS Name";
		public const string OSVersionKey = "OS Version";
		public const string SystemManufacturerKey = "System Manufacturer";
		public const string BIOSVersionKey = "BIOS Version";
		public const string TotalPhysicalMemoryKey = "Total Physical Memory";
		public const string ProcessorsKey = "Processor(s)";
		public const string ProcessorTypeKey = "Processor(s)[01]";
		public const string SystemModelKey = "System Model";
		public const string VirtualMachineKey = "Virtual Machine";
		public const string HypervisorKey = "(Hypervisor)";

		public const string MAXDOP = "MAXDOP";
		public const string CostOfParallelism = "CostOfParallelism";
		public const string OptimizeForAdHocWorkloads = "OptimizeForAdHocWorkloads";
		public const string MinMemory = "MinMemory";
		public const string MaxMemory = "MaxMemory";
		public const string TotalAvailableMemory = "TotalAvailableMemory";
		public const string TraceFlags = "TraceFlags";
		public const string DbAlwaysOn = "DbAlwaysOn";
		public const string ParameterizationForced = "ParameterizationForced";
		public const string AutoCreate = "AutoCreate";
		public const string AutoUpdate = "AutoUpdate";
		public const string AutoUpdateAsync = "AutoUpdateAsync";
		public const string CompatibilityLevel = "CompatibilityLevel";
		public const string Cardinality = "Cardinality";

		#endregion // SuppressResourceStringsCheckRegion

		// Conversion between SP columns and field keys
		public static readonly Dictionary<VERSION_INFO_FIELDS, string> VersionFieldKeys = new Dictionary<VERSION_INFO_FIELDS, string>()
		{
			{ VersionReport.VERSION_INFO_FIELDS.SystemManufacturer, SystemManufacturerKey },
			{ VersionReport.VERSION_INFO_FIELDS.BiosReleaseDate, BIOSVersionKey },
			{ VersionReport.VERSION_INFO_FIELDS.PhysicalMemoryMb, TotalPhysicalMemoryKey },
			{ VersionReport.VERSION_INFO_FIELDS.NumberOfLogicalProcessors, ProcessorsKey },
			{ VersionReport.VERSION_INFO_FIELDS.ProcessorDescription, ProcessorTypeKey },
			{ VersionReport.VERSION_INFO_FIELDS.OsName, OSNameKey },
			{ VersionReport.VERSION_INFO_FIELDS.OsVersion, OSVersionKey },
			{ VersionReport.VERSION_INFO_FIELDS.IsOrHasVirtualMachine, VirtualMachineKey },
			{ VersionReport.VERSION_INFO_FIELDS.VirtualMachineType, HypervisorKey }
		};

		// Conversion between SP columns and config keys
		[ThreadSafe]
		public static readonly Dictionary<DBCONFIG_INFO_FIELDS, string> DbConfigFieldKeys = new Dictionary<DBCONFIG_INFO_FIELDS, string>()
		{
			{ VersionReport.DBCONFIG_INFO_FIELDS.MAXDOP, MAXDOP },
			{ VersionReport.DBCONFIG_INFO_FIELDS.CostOfParallelism, CostOfParallelism },
			{ VersionReport.DBCONFIG_INFO_FIELDS.OptimizeForAdHocWorkloads, OptimizeForAdHocWorkloads },
			{ VersionReport.DBCONFIG_INFO_FIELDS.MinMemory, MinMemory },
			{ VersionReport.DBCONFIG_INFO_FIELDS.MaxMemory, MaxMemory },
			{ VersionReport.DBCONFIG_INFO_FIELDS.TotalAvailableMemory, TotalAvailableMemory },
			{ VersionReport.DBCONFIG_INFO_FIELDS.TraceFlags, TraceFlags },
			{ VersionReport.DBCONFIG_INFO_FIELDS.DbAlwaysOn, DbAlwaysOn },
			{ VersionReport.DBCONFIG_INFO_FIELDS.ParameterizationForced, ParameterizationForced },
			{ VersionReport.DBCONFIG_INFO_FIELDS.AutoCreate, AutoCreate },
			{ VersionReport.DBCONFIG_INFO_FIELDS.AutoUpdate, AutoUpdate },
			{ VersionReport.DBCONFIG_INFO_FIELDS.AutoUpdateAsync, AutoUpdateAsync },
			{ VersionReport.DBCONFIG_INFO_FIELDS.CompatibilityLevel, CompatibilityLevel },
			{ VersionReport.DBCONFIG_INFO_FIELDS.Cardinality, Cardinality },
		};

		#endregion

		ZString FindPreferredUpgradeMethod()
		{
			// Not used.
			return "DEF";
		}

		ZString FindDBServerSecurityMode()
		{
			var result = DatabaseSecurityModePairList.Codes.OpenMode;

			if (!EnvProxy.IsHostedWithCargowise)
			{
				bool? isDatabaseSecurityOpen = Db.Instance.IsDatabaseSecurityOpen();

				if (isDatabaseSecurityOpen.HasValue)
				{
					if (!isDatabaseSecurityOpen.Value)
					{
						result = DatabaseSecurityModePairList.Codes.Locked;
					}
				}
				else
				{
					result = DatabaseSecurityModePairList.Codes.Indeterminate;
				}
			}

			return result;
		}

		ZString FindSQLServerName(string fullyQualifiedDatabaseName)
		{
			return ExtractServerAndInstanceFromDatabase(fullyQualifiedDatabaseName)[0];
		}

		ZString FindSQLInstanceName(string fullyQualifiedDatabaseName)
		{
			return ExtractServerAndInstanceFromDatabase(fullyQualifiedDatabaseName)[1];
		}

		string[] ExtractServerAndInstanceFromDatabase(string fullyQualifiedDatabaseName)
		{
			string[] result = null;

			if (fullyQualifiedDatabaseName.IndexOf('\\') > -1)
			{
				result = fullyQualifiedDatabaseName.Split('\\');
			}
			else
			{
				result = new string[] { fullyQualifiedDatabaseName, "" };
			}

			return result;
		}

		void GetAllLogAndDataFilesForAllRelatedDatabases(StringCollectionX logAndDataFiles)
		{
			foreach (string databaseName in Db.Connection.GetDatabases(DatabaseType.Operational))
			{
				logAndDataFiles.AddRange(Db.Connection.GetDbFiles(databaseName));
			}
		}

		void GetDocumentEngineStats()
		{
			DocumentEngineVersionRetriever docEngInfo = new DocumentEngineVersionRetriever();
			activePrintersCount = docEngInfo.ActivePrintersCount;
		}

		void GetUpgradeScheduleInfo()
		{
			var taskQuerier = ObjectFactory.Get<IServiceManagerQuerier>();
			var stateQuerier = ObjectFactory.Get<IServiceTaskScheduleStateQuerier>();

			var taskExists = taskQuerier.TryGetServiceTaskNextRunTime("UPG", out var nextRunTime);
			_ = stateQuerier.TryGetServiceTaskScheduleState("UPG", out var scheduleState);

			if (taskExists)
			{
				nextRunTimeUtcUPG = nextRunTime.HasValue ? new ZDateTimeOffset(nextRunTime.Value).ToZDateTime() : ZDateTime.Empty;
				scheduleStateUPG = !string.IsNullOrEmpty(scheduleState) ? scheduleState : "";
			}

			taskExists = taskQuerier.TryGetServiceTaskNextRunTime("MUG", out nextRunTime);
			_ = stateQuerier.TryGetServiceTaskScheduleState("MUG", out scheduleState);

			if (taskExists)
			{
				nextRunTimeUtcMUG = nextRunTime.HasValue ? new ZDateTimeOffset(nextRunTime.Value).ToZDateTime() : ZDateTime.Empty;
				scheduleStateMUG = !string.IsNullOrEmpty(scheduleState) ? scheduleState : "";
			}
		}

		#endregion

		#region VersionReport.XML

		string reportXml;
		public ZString XML
		{
			get
			{
				if (reportXml == null)
				{
					using (MemoryStream stream = new MemoryStream())
					using (XmlTextWriter writer = new XmlTextWriter(stream, new UTF8Encoding()))
					{
						WriteXMLHeader(writer);
						WriteXMLBody(writer);
						WriteXMLFooter(writer);
						writer.Flush();
						reportXml = Encoding.UTF8.GetString(stream.ToArray());
					}
				}

				return reportXml;
			}
		}

		public void WriteXMLHeader(XmlTextWriter writer)
		{
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument();
		}

		public void WriteXMLBody(XmlTextWriter writer)
		{
			writer.WriteStartElement(ElementNames.VersionReport);
			writer.WriteElementString(ElementNames.DatabaseNumber, DatabaseNumber.ToString());

			writer.WriteElementString(ElementNames.EnterpriseCode, EnterpriseCode);
			writer.WriteElementString(ElementNames.PhysicalServerID, PhysicalServerID);
			writer.WriteElementString(ElementNames.DBServerName, DBServerName);
			writer.WriteElementString(ElementNames.DBName, DBName);

			writer.WriteElementString(ElementNames.CurrentVersion, CurrentVersion);
			writer.WriteElementString(ElementNames.CurrentDate, CurrentDate.ToString("o").ToUpper());

			if (!isBasic)
			{
				writer.WriteElementString(ElementNames.CurrentRelease, CurrentRelease);
				writer.WriteElementString(ElementNames.PreferredUpgradeMethod, PreferredUpgradeMethod);
				writer.WriteElementString(ElementNames.DBServerSecurityMode, DBServerSecurityMode);
			}

			writer.WriteElementString(ElementNames.SQLServerName, SQLServerName);
			writer.WriteElementString(ElementNames.SQLServerInstanceName, SQLServerInstanceName);

			if (!isBasic)
			{
				WritePOP3Details(writer);
				WriteSMTPDetails(writer);
				WriteDBFileNameList(writer);
				WriteCompanies(writer);
				writer.WriteElementString(ElementNames.DatabaseBackupPath, DatabaseBackupPath);
				writer.WriteElementString(ElementNames.EncryptedSystemExpirationKey, EncryptedSystemExpirationKey);
			}

			writer.WriteElementString(ElementNames.EncryptedRegistrationKey, EncryptedRegistrationKey);

			if (!isBasic)
			{
				WriteDocEngineStats(writer);
				WriteSqlServerVersionDetails(writer);
				WriteAdditionalDatabaseSystemInfoList(writer);
				WriteLicenceUsage(writer);
			}

			if (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.Value != null)
			{
				writer.WriteElementString(ElementNames.OutboundEAdaptorUrl, eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.Value);
			}

			if (!isBasic)
			{
				writer.WriteElementString(ElementNames.NextRunTimeUtcUPG, NextRunTimeUtcUPG.ToString("o").ToUpper());
				writer.WriteElementString(ElementNames.NextRunTimeUtcMUG, NextRunTimeUtcMUG.ToString("o").ToUpper());
				writer.WriteElementString(ElementNames.ScheduleStateUPG, ScheduleStateUPG);
				writer.WriteElementString(ElementNames.ScheduleStateMUG, ScheduleStateMUG);

				writer.WriteElementString(ElementNames.TokenAuthenticationEnabled, TokenAuthenticationEnabled.ToString());

				writer.WriteElementString(ElementNames.FeatureControlRule, FeatureControlRule);

				WriteSupportedFeatureCodes(writer);
			}
		}

		public void WriteXMLFooter(XmlTextWriter writer)
		{
			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		void WritePOP3Details(XmlTextWriter writer)
		{
			writer.WriteStartElement(ElementNames.POP3);
			writer.WriteElementString(ElementNames.MailboxEmailAddress, InternalPOP3EmailAddress);
			writer.WriteElementString(ElementNames.UserName, InternalPOP3UserName);
			writer.WriteElementString(ElementNames.MailServer, InternalPOP3MailServer);
			writer.WriteElementString(ElementNames.Port, InternalMailServerPort.ToString());
			writer.WriteEndElement();
		}

		void WriteSMTPDetails(XmlTextWriter writer)
		{
			writer.WriteStartElement(ElementNames.SMTP);
			writer.WriteElementString(ElementNames.MailServer, InternalSMTPMailServer);
			writer.WriteElementString(ElementNames.Port, InternalSMTPPort.ToString());
			writer.WriteEndElement();
		}

		void WriteDBFileNameList(XmlTextWriter writer)
		{
			if (LogAndDataFiles.Count > 0)
			{
				writer.WriteStartElement(ElementNames.DBFileNameList);
				foreach (string databaseFileName in LogAndDataFiles)
				{
					writer.WriteElementString(ElementNames.DBFileName, databaseFileName);
				}
				writer.WriteEndElement();
			}
		}

		void WriteDocEngineStats(XmlTextWriter writer)
		{
			writer.WriteStartElement(ElementNames.DocEngineStats);
			writer.WriteElementString(ElementNames.DocEngineActivePrintersCount, ActivePrintersCount.ToString());
			writer.WriteEndElement();
		}

		void WriteSqlServerVersionDetails(XmlTextWriter writer)
		{
			DbConnection connection = Db.Connection;
			writer.WriteStartElement(ElementNames.SqlServerVersionDetails);
			writer.WriteElementString(ElementNames.SqlServerCpuArchitecture, "X64");
			writer.WriteElementString(ElementNames.SqlServerEdition, connection.ServerEdition.ToString());
			writer.WriteElementString(ElementNames.SqlServerVersion, connection.ServerVersionNumber.SqlServerGeneration);
			writer.WriteElementString(ElementNames.SqlServerFullVersionText, connection.ServerFullVersionText);
			writer.WriteEndElement();
		}

		void WriteAdditionalDatabaseSystemInfoList(XmlTextWriter writer)
		{
			if (AdditionalDatabaseSystemInfoList.Count > 0)
			{
				writer.WriteStartElement(ElementNames.AdditionalDatabaseSystemInfoList);
				foreach (string info in AdditionalDatabaseSystemInfoList)
				{
					writer.WriteElementString(ElementNames.AdditionalDatabaseSystemInfo, info);
				}
				writer.WriteEndElement();
			}
		}

		void WriteLicenceUsage(XmlTextWriter writer)
		{
			if (licenceUsage != null)
			{
				writer.WriteElementString(ElementNames.LicenceUsage, licenceUsage);
			}
		}

		#endregion

		#region Version Report Properties

		public int DatabaseNumber
		{
			get { return databaseNumber; }
		}
		int databaseNumber;

		public ZString EnterpriseCode
		{
			get { return enterpriseCode; }
		}
		ZString enterpriseCode;

		public ZString PhysicalServerID
		{
			get { return physicalServerID; }
		}
		ZString physicalServerID;

		public ZString DBServerName
		{
			get { return dbServerName; }
		}
		ZString dbServerName;

		public ZString DBName
		{
			get { return dbName; }
		}
		ZString dbName;

		public ZString CurrentVersion
		{
			get { return currentVersion; }
		}
		ZString currentVersion;

		public ZString CurrentRelease
		{
			get { return currentRelease; }
		}
		ZString currentRelease;

		public ZDateTime CurrentDate
		{
			get { return currentDate; }
		}
		ZDateTime currentDate;

		public ZString PreferredUpgradeMethod
		{
			get { return preferredUpgradeMethod; }
		}
		ZString preferredUpgradeMethod;

		public ZString DBServerSecurityMode
		{
			get { return dbServerSecurityMode; }
		}
		ZString dbServerSecurityMode;

		public ZString SQLServerName
		{
			get { return sqlServerName; }
		}
		ZString sqlServerName;

		public ZString SQLServerInstanceName
		{
			get { return sqlServerInstanceName; }
		}
		ZString sqlServerInstanceName;

		public ZString InternalPOP3EmailAddress
		{
			get { return internalPOP3EmailAddress; }
		}
		ZString internalPOP3EmailAddress;

		public ZString InternalPOP3UserName
		{
			get { return internalPOP3UserName; }
		}
		ZString internalPOP3UserName;

		public ZString InternalPOP3MailServer
		{
			get { return internalPOP3MailServer; }
		}
		ZString internalPOP3MailServer;

		public ZInt InternalMailServerPort
		{
			get { return internalMailServerPort; }
		}
		ZInt internalMailServerPort;

		public ZString InternalSMTPMailServer
		{
			get { return internalSMTPMailServer; }
		}
		ZString internalSMTPMailServer;

		public ZInt InternalSMTPPort
		{
			get { return internalSMTPPort; }
		}
		ZInt internalSMTPPort;

		public ZString OutboundEAdaptorUrl
		{
			get { return outboundEAdaptorUrl; }
		}
		readonly ZString outboundEAdaptorUrl;

		public List<String> AdditionalDatabaseSystemInfoList
		{
			get { return additionalDatabaseSystemInfoList ?? (additionalDatabaseSystemInfoList = new List<string>()); }
		}
		List<String> additionalDatabaseSystemInfoList;

		public string LicenceUsage
		{
			get { return licenceUsage; }
		}
		string licenceUsage;

		public StringCollectionX LogAndDataFiles
		{
			get
			{
				if (logAndDataFiles == null)
				{
					logAndDataFiles = new StringCollectionX();
				}

				return logAndDataFiles;
			}
		}
		StringCollectionX logAndDataFiles;

		public List<CompanyReport> CompanyList
		{
			get { return companyList ?? (companyList = new List<CompanyReport>()); }
		}
		List<CompanyReport> companyList;

		public ZString DatabaseBackupPath
		{
			get { return databaseBackupPath; }
		}
		ZString databaseBackupPath;

		public ZString EncryptedSystemExpirationKey
		{
			get { return encryptedSystemExpirationKey; }
		}
		ZString encryptedSystemExpirationKey;

		public ZString EncryptedRegistrationKey
		{
			get { return encryptedRegistrationKey; }
		}
		ZString encryptedRegistrationKey;

		public ZInt ActivePrintersCount
		{
			get { return activePrintersCount; }
		}
		ZInt activePrintersCount;

		public ZDateTime NextRunTimeUtcUPG
		{
			get { return nextRunTimeUtcUPG; }
		}
		ZDateTime nextRunTimeUtcUPG;

		public ZDateTime NextRunTimeUtcMUG
		{
			get { return nextRunTimeUtcMUG; }
		}
		ZDateTime nextRunTimeUtcMUG;

		public ZString ScheduleStateUPG
		{
			get { return scheduleStateUPG; }
		}
		ZString scheduleStateUPG;

		public ZString ScheduleStateMUG
		{
			get { return scheduleStateMUG; }
		}
		ZString scheduleStateMUG;

		public ZBool TokenAuthenticationEnabled
		{
			get { return tokenAuthenticationEnabled; }
		}
		ZBool tokenAuthenticationEnabled;

		public ZString FeatureControlRule
		{
			get { return featureControlRule; }
		}
		readonly ZString featureControlRule;

		IEnumerable<LicenceFeatureCodePair> SupportedFeatureCodes => supportedFeatureCodes;
		readonly IEnumerable<LicenceFeatureCodePair> supportedFeatureCodes;

		#endregion

		#region Element Names

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name, Element name - not a res string")]
		public static class ElementNames
		{
			public const string VersionReport = "VersionReport";
			public const string DatabaseNumber = "DatabaseNumber";
			public const string EnterpriseCode = "EnterpriseCode";
			public const string PhysicalServerID = "PhysicalServerID";
			public const string DBServerName = "DBServerName";
			public const string DBName = "DBName";
			public const string CurrentVersion = "CurrentVersion";
			public const string CurrentDate = "CurrentDate";
			public const string CurrentRelease = "CurrentRelease";
			public const string PreferredUpgradeMethod = "PreferredUpgradeMethod";
			public const string DBServerSecurityMode = "DBServerSecurityMode";
			public const string PublicEmailAddressForUpdate = "PublicEmailAddressForUpdate";
			public const string SQLServerName = "SQLServerName";
			public const string SQLServerInstanceName = "SQLServerInstanceName";
			public const string POP3 = "POP3";
			public const string SMTP = "SMTP";
			public const string UserName = "UserName";
			public const string MailServer = "MailServer";
			public const string MailboxEmailAddress = "EmailAddress";
			public const string Port = "Port";
			public const string DBFileNameList = "DBFileNameList";
			public const string DBFileName = "DBFileName";
			public const string CompanyList = "CompanyList";
			public const string Company = "Company";
			public const string DatabaseBackupPath = "DatabaseBackupPath";
			public const string EncryptedSystemExpirationKey = "SystemHealthData";
			public const string EncryptedRegistrationKey = "RegistrationData";
			public const string DocEngineStats = "DocEngine";
			public const string DocEngineActivePrintersCount = "ActivePrintersCount";
			public const string SqlServerVersionDetails = "SqlServerVersionDetails";
			public const string SqlServerCpuArchitecture = "SqlServerCpuArchitecture";
			public const string SqlServerEdition = "SqlServerEdition";
			public const string SqlServerFullVersionText = "SqlServerFullVersionText";
			public const string SqlServerVersion = "SqlServerVersion";
			public const string AdditionalDatabaseSystemInfoList = "AdditionalDatabaseSystemInfoList";
			public const string AdditionalDatabaseSystemInfo = "SystemInfo";
			public const string LicenceUsage = "LicenceUsage";
			public const string OutboundEAdaptorUrl = "OutboundEAdaptorUrl";
			public const string NextRunTimeUtcUPG = "NextRunTimeUtcUPG";
			public const string NextRunTimeUtcMUG = "NextRunTimeUtcMUG";
			public const string ScheduleStateUPG = "ScheduleStateUPG";
			public const string ScheduleStateMUG = "ScheduleStateMUG";
			public const string TokenAuthenticationEnabled = "TokenAuthenticationEnabled";
			public const string FeatureControlRule = "FeatureControlRule";
			public const string SupportedFeatureCodes = "SupportedFeatureCodes";
			public const string FeatureCodePair = "FeatureCodePair";
			public const string FeatureCode = "FeatureCode";
			public const string FeatureDescription = "FeatureDescription";
			public const string FeatureStage = "FeatureStage";
		}

		#endregion

		#region Company

		void WriteCompanies(XmlTextWriter writer)
		{
			if (companyList != null && companyList.Count > 0)
			{
				writer.WriteStartElement(ElementNames.CompanyList);
				foreach (var company in companyList)
				{
					company.Write(writer);
				}
				writer.WriteEndElement();
			}
		}

		protected IEnumerable<GlbCompany> LoadCompaniesExcludingDemo()
		{
			return new GlbCompanyCollection(Factory, new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode));
		}

		protected void AddCompanies(IEnumerable<GlbCompany> companiesToInclude)
		{
			foreach (var company in companiesToInclude)
			{
				var report = new CompanyReport(company);
				CompanyList.Add(report);
			}
		}

		#endregion

		#region Feature Code

		protected virtual IEnumerable<LicenceFeatureCodePair> PopulateSupportedFeatureCodes()
		{
			return LicenceFeatureCodeList.GetLicenceFeatureCodePairs();
		}

		void WriteSupportedFeatureCodes(XmlTextWriter writer)
		{
			writer.WriteStartElement(ElementNames.SupportedFeatureCodes);
			foreach (var codePair in SupportedFeatureCodes)
			{
				writer.WriteStartElement(ElementNames.FeatureCodePair);
				writer.WriteElementString(ElementNames.FeatureCode, codePair.Code);
				writer.WriteElementString(ElementNames.FeatureDescription, codePair.Description);
				writer.WriteElementString(ElementNames.FeatureStage, codePair.Stage.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		#endregion
	}
}
