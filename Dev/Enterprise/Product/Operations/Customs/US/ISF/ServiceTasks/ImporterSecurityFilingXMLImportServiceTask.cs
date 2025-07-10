using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.ISF.DataTransfer;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005
#pragma warning restore IDE0079

[assembly: HostedService("XSF", "Importer Security Filing XML Import Service", "ISF",
	typeof(Enterprise.Customs.US.ISF.ServiceTasks.ImporterSecurityFilingXMLImporter),
	MinimumPeriod = "1Minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.US.ISF.ServiceTasks
{
	public class ImporterSecurityFilingXMLImporter : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach ((ZGuid branchPK, ZString xmlDirectory) in GetBranchPKsWithXmlDirectory())
			{
				token.ThrowIfCancellationRequested();
				if (!xmlDirectory.IsEmpty)
				{
					using (DisposableEnvironment.ForBranch(branchPK.ToGuid()))
					{
						RunTaskHandleEmailSendFailure(() =>
						{
							DoImport(xmlDirectory, token);
						});
					}
				}
			}
		}

		void DoImport(ZString xmlDirectory, CancellationToken token)
		{
			try
			{
				var importer = new ImporterSecurityFilingXmlDataImporter();
				foreach (FileInfo xmlFileInfo in GetXMLFilesInCreateOrder(xmlDirectory))
				{
					token.ThrowIfCancellationRequested();
					string xmlFile = xmlFileInfo.FullName;
					NotificationBuffer buffer = new NotificationBuffer();
					importer.ImportData(xmlFile, buffer, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ImporterSecurityFilingXMLImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, xmlFile));
					bool isFailure = buffer.HasErrors;
					if (!SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.Value || isFailure)
					{
						EmailDef email = new EmailDef();
						string subjectStatus = "";
						if (isFailure)
						{
							email.Attachments.Add(new AttachmentDef(xmlFile));
							subjectStatus = " (Failure)";
						}
						email.Subject = "ISF XML File Import" + subjectStatus;
						email.Body = "Importing Importer Security Filing XML file: " + xmlFile + System.Environment.NewLine + buffer.AsString;
						Env.OutgoingCustomsMailManager.CreateAndSave(email, ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty),
							GroupSourceLocator.GetFromRegistryItem(ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup));
					}
					File.Delete(xmlFile);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new HostedServiceException("Importing XML files from '" + xmlDirectory + "'", ex);
			}
		}

		FileInfo[] GetXMLFilesInCreateOrder(ZString xmlDirectory)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(xmlDirectory);
			List<FileInfo> result = new List<FileInfo>(directoryInfo.GetFiles("*.xml"));
			result.Sort(new FilesDateComparer());
			return result.ToArray();
		}

		static IEnumerable<(ZGuid branchPK, ZString xmlDirectory)> GetBranchPKsWithXmlDirectory()
		{
			var factory = new BusinessObjectFactory();
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load($@"SELECT {GlbBranchSchema.Constants.PK}, CAST({StmDataSchema.Constants.SD_BinaryValue} AS NVARCHAR(MAX)) XmlDirectory
							FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}
							INNER JOIN {GlbBranchSchema.Constants.SqlSchemaName}.{GlbBranchSchema.Constants.TableName} ON {StmDataSchema.Constants.SD_Owner} = {GlbBranchSchema.Constants.PK} AND {GlbBranchSchema.Constants.GB_IsActive} = 1
							WHERE {StmDataSchema.Constants.SD_Name} = 'ImporterSecurityFilingDataImportDirectory' AND {StmDataSchema.Constants.SD_BinaryValue} <> 0x");

			return collection.Select(x => (new ZGuid(x[GlbBranchSchema.Constants.PK]), new ZString(x["XmlDirectory"])));
		}

		[HostedServiceRequirement]
		public static string CheckRegistryHasBeenConfigured()
		{
			var registryConfigured = Convert.ToBoolean(CargoWise.Data.Db.Connection.ExecuteScalar($@"IF EXISTS (SELECT NULL
							FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}
							INNER JOIN {GlbBranchSchema.Constants.SqlSchemaName}.{GlbBranchSchema.Constants.TableName} ON {StmDataSchema.Constants.SD_Owner} = {GlbBranchSchema.Constants.PK} AND {GlbBranchSchema.Constants.GB_IsActive} = 1
							WHERE {StmDataSchema.Constants.SD_Name} = 'ImporterSecurityFilingDataImportDirectory' AND {StmDataSchema.Constants.SD_BinaryValue} <> 0x) SELECT 1 ELSE SELECT 0"));
			return registryConfigured ? string.Empty : RegistryHasNotBeenConfigured;
		}

		internal const string RegistryHasNotBeenConfigured = "The registry setting 'System -> Data Import Settings -> Importer Security Filing -> Folder to scan for ISF XML files' has not been configured.";

		class FilesDateComparer : IComparer<FileInfo>
		{
			#region IComparer<FileInfo> Members

			public int Compare(FileInfo x, FileInfo y)
			{
				int iResult;
				if (x.LastWriteTime == y.LastWriteTime)
				{
					iResult = 0;
				}
				else if (x.LastWriteTime > y.LastWriteTime)
				{
					iResult = 1;
				}
				else
				{
					iResult = -1;
				}
				return iResult;
			}

			#endregion
		}
	}
}
