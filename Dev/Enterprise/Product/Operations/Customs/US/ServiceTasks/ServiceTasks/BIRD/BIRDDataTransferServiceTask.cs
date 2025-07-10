using System;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Codes.BIRDDataTransfer,
	Enterprise.Customs.US.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.BIRDDataTransfer,
	"USC",
	typeof(Enterprise.Customs.US.ServiceTasks.BIRDDataTransferServiceTask),
	RequiresCompanyInCountry = Constants.CountryCodes.UnitedStates + "," + Constants.CountryCodes.PuertoRico,
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "30seconds"
	)]

namespace Enterprise.Customs.US.ServiceTasks
{
	public class BIRDDataTransferServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			INotifications notifications = ServiceLogger.GetTaskNotificationSubscriber();

			bool hasConfigurations = false;
			var glbCompanies = GetActionCompanies();
			foreach (var glbCompany in glbCompanies)
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(glbCompany.FirstActiveBranch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						bool hasConfigurationsForThisCompany = CheckAnyEnvironments(notifications, glbCompany);
						hasConfigurations |= hasConfigurationsForThisCompany;

						SqlApplicationLock mutex;
						if (hasConfigurationsForThisCompany && GetDbConnection().TryGetLock(MutexPrefix + glbCompany.PK, out mutex))
						{
							using (mutex)
							{
								ProcessBRDFile(glbCompany, glbCompanies);
							}
						}
					});
				}
			}

			if (glbCompanies.Length > 0 && !hasConfigurations)
			{
				notifications.AddWarning(BIRDDataImport);
			}

			new DataTransfer.BIRDExportProcessor().Execute(ServiceLogger.GetTaskNotificationSubscriber());
		}

		public virtual void ProcessBRDFile(GlbCompany glbCompany, GlbCompany[] glbCompanies)
		{
			INotifications notifications = ServiceLogger.GetTaskNotificationSubscriber();
			if (CheckEnvironments(notifications, glbCompany, glbCompanies))
			{
				new DataTransfer.BIRDImportProcessor().Execute(ServiceLogger.GetTaskNotificationSubscriber());
			}
			if (CheckFTPEnvironments(notifications, glbCompany, glbCompanies))
			{
				new DataTransfer.BIRDImportProcessor().ExecuteByFtp(notifications);
			}
		}

		public virtual DbConnection GetDbConnection()
		{
			return Db.Connection;
		}

		public virtual string MutexPrefix
		{
			get { return "BIRD_"; }
		}

		bool CheckEnvironments(INotifications notifications, GlbCompany glbCompany, GlbCompany[] glbCompanies)
		{
			bool result = true;
			if (ShouldImportViaLocalDirectory(glbCompany))
			{
				string directory = Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				string backupDirectory = Registry.Business.SystemDataRegistry.Instance.BIRDImportBackupDirectory.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

				result &= CheckDirectory(directory, BIRDImportDirectoryRegistry, notifications);
				result &= CheckDirectory(backupDirectory, BIRDImportBackupDirectoryRegistry, notifications);

				if (directory == backupDirectory)
				{
					result = false;
					notifications.AddWarning("Import Directory and Backup Directory specified in the Registry are identical.");
				}
				if (result)
				{
					foreach (var company in glbCompanies)
					{
						if (glbCompany.PK != company.PK && directory.Equals(Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty)))
						{
							notifications.AddError(string.Format(BIRDDirAllSame, glbCompany.GC_Code, company.GC_Code));
							result = false;
							break;
						}
					}
				}
			}
			else
			{
				result = false;
			}

			return result;
		}

		bool CheckDirectory(string directory, string registry, INotifications notifications)
		{
			bool result = true;

			if (string.IsNullOrEmpty(directory.Trim()))
			{
				result = false;
				notifications.AddWarning(string.Format(BIRDImportDirectoryIsNotSet, registry));
			}
			else
			{
				try
				{
					if (!Directory.Exists(directory))
					{
						result = false;
						notifications.AddWarning(string.Format(BIRDImportDirectoryDoesNotExist, registry));
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = false;
					notifications.AddWarning(ex.Message);
				}
			}

			return result;
		}

		bool CheckFTPEnvironments(INotifications notifications, GlbCompany glbCompany, GlbCompany[] glbCompanies)
		{
			bool result = true;
			if (ShouldImportViaFTP(glbCompany))
			{
				string ftpServerAddress = Registry.Business.SystemDataRegistry.Instance.BIRDFTPServerAddress.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (string.IsNullOrEmpty(ftpServerAddress))
				{
					result = false;
					notifications.AddWarning(BIRDFTPServerAddress);
				}
				string ftpRemoteDirectory = Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (result && !string.IsNullOrEmpty(ftpRemoteDirectory))
				{
					result = CheckPath(notifications, glbCompany);
				}

				string ftpUser = Registry.Business.SystemDataRegistry.Instance.BIRDFTPUserName.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (string.IsNullOrEmpty(ftpUser))
				{
					result = false;
					notifications.AddWarning(BIRDFTPUserName);
				}

				if (string.IsNullOrEmpty(Registry.Business.SystemDataRegistry.Instance.BIRDFTPPassword.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)))
				{
					result = false;
					notifications.AddWarning(BIRDFTPPassword);
				}
				if (result)
				{
					string ftpFileExtension = Registry.Business.SystemDataRegistry.Instance.BIRDFTPFileExtension.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					foreach (var company in glbCompanies)
					{
						if (glbCompany.PK != company.PK
							&& ftpServerAddress.Equals(Registry.Business.SystemDataRegistry.Instance.BIRDFTPServerAddress.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
							&& ftpRemoteDirectory.Equals(Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
							&& ftpFileExtension.Equals(Registry.Business.SystemDataRegistry.Instance.BIRDFTPFileExtension.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
							)
						{
							notifications.AddError(string.Format(BIRDFtpAllSame, glbCompany.GC_Code, company.GC_Code));
							result = false;
							break;
						}
					}
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

#if DEBUG
		internal
#endif
		bool CheckPath(INotifications notifications, GlbCompany glbCompany)
		{
			bool result = false;
			string serverName = Registry.Business.SystemDataRegistry.Instance.BIRDFTPServerAddress.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) + "/" +
													Registry.Business.SystemDataRegistry.Instance.BIRDFTPRemoteDirectory.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			string ftpProtocol = Uri.UriSchemeFtp + Uri.SchemeDelimiter;
			if (!serverName.StartsWith(ftpProtocol, StringComparison.OrdinalIgnoreCase))
			{
				serverName = ftpProtocol + serverName;
			}
			result = Uri.IsWellFormedUriString(serverName, UriKind.Absolute);
			if (!result)
			{
				notifications.AddWarning(BIRDFTPPath);
			}
			return result;
		}

#if DEBUG
		internal
#endif
		bool CheckAnyEnvironments(INotifications notifications, GlbCompany glbCompany)
		{
			return ShouldImportViaFTP(glbCompany) || ShouldImportViaLocalDirectory(glbCompany);
		}

		bool ShouldImportViaFTP(GlbCompany glbCompany)
		{
			return !string.IsNullOrEmpty(Registry.Business.SystemDataRegistry.Instance.BIRDFTPServerAddress.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
				|| !string.IsNullOrEmpty(Registry.Business.SystemDataRegistry.Instance.BIRDFTPUserName.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
				|| !string.IsNullOrEmpty(Registry.Business.SystemDataRegistry.Instance.BIRDFTPPassword.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		bool ShouldImportViaLocalDirectory(GlbCompany glbCompany)
		{
			return !string.IsNullOrEmpty(Registry.Business.SystemDataRegistry.Instance.BIRDImportDirectory.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
				|| !string.IsNullOrEmpty(Registry.Business.SystemDataRegistry.Instance.BIRDImportBackupDirectory.GetValueWithoutFallback(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public virtual GlbCompany[] GetActionCompanies()
		{
			return GlbCompany.GetActiveCompanies(company => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(company.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates).ToArray();
		}

		public const string BIRDDirAllSame = "There are two companys {0} and {1} entered the same BIRDImportDirectory.Please reset it (Registry > System > Data Import Settings > Customs Declarations > United States of America)";
		public const string BIRDFtpAllSame = "There are two companys {0} and {1} entered the same BIRD FTP Setting.Please reset it (System > Data Import Setting > Customs Declarations > United States of America > BIRD FTP Setting)";

		public const string BIRDFTPServerAddress = "Please enter the FTP Server Address. This can be entered in the Registry (System > Data Import Setting > Customs Declarations > United States of America > BIRD FTP Setting -> FTP Server Address)";
		public const string BIRDFTPUserName = "Please enter the FTP User Name. This can be entered in the Registry (System > Data Import Setting > Customs Declarations > United States of America > BIRD FTP Setting > FTP User Name)";
		public const string BIRDFTPPassword = "Please enter the FTP Password. This can be entered in the Registry (System > Data Import Setting > Customs Declarations > United States of America > BIRD FTP Setting > FTP Password)";
		public const string BIRDFTPPath = "Please reset FTP Server Address and FTP Remote Directory Name.this should be formated in the following format: ftp://{SERVER _NAME}/{Directory}.This can be entered in the Registry(Registry > System > Data Import Settings > Customs Declarations > United States of America > BIRD FTP Setting)";

		public const string BIRDImportDirectoryRegistry = "Registry > System > Data Import Settings > Customs Declarations > United States of America > BIRD Import File Directory";
		public const string BIRDImportBackupDirectoryRegistry = "Registry > System > Data Import Settings > Customs Declarations > United States of America > BIRD Import File Backup Directory";

		public const string BIRDImportDirectoryIsNotSet = "A directory has not been set in a registry, {0}. Please create a directory, set it at the registry and restart this service task.";
		public const string BIRDImportDirectoryDoesNotExist = "The directory specified in {0} does not exist.";

		public const string BIRDDataImport = "Please enter the BIRDImportDirectory information or enter the BIRD FTP Settings.This can be entered in the Registry(Registry > System > Data Import Settings > Customs Declarations > United States of America)";
	}
}
