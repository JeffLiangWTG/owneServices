using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.LVS.Business
{
	public static class StandAloneDeclarationConverter
	{
		public static int PublishDeclarationUniversalXMLIndividual(CusUSLVConsignment consignment, bool shouldCallFactorySave = true)
		{
			var convertingConsignment = new[] { consignment };
			return PublishDeclarationUniversalXMLCore(consignment, shouldCallFactorySave, "USLVConsignmentToDeclarationDataWriter", convertingConsignment);
		}

		public static int PublishDeclarationUniversalXMLCombined(CusUSLVConsignment consignment, bool shouldCallFactorySave = true)
		{
			var tciLogs = consignment.Shipment.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);
			var combinedLog = tciLogs.FirstOrDefault(l => l.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason) && reason == LVSConstants.ConvertToDeclarationReason.CombineConsignments);
			var combinedReference = string.Format("|{0}={1}", CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, LVSConstants.ConvertToDeclarationReason.ConsignmentsCombined);
			combinedLog.UpdateReference(combinedReference);

			var combinedConsignments = consignment.Shipment.CombinedConsignmentsToConvert;
			return PublishDeclarationUniversalXMLCore(consignment, shouldCallFactorySave, "USLVConsignmentCombinedToDeclarationDataWriter", combinedConsignments);
		}

		static int PublishDeclarationUniversalXMLCore(CusUSLVConsignment consignment, bool shouldCallFactorySave, string writerName, IEnumerable<CusUSLVConsignment> convertingConsignments)
		{
			try
			{
				using (DisposableEnvironment.ForBranch(consignment.Shipment.RegistryBranchPK))
				{
					var writer = ObjectFactory.Get<ITopLevelDataObjectWriter>(
					writerName, new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, consignment.Shipment)));
					var dataObject = writer.GetDataObject(consignment);
					dataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
					var declarationContextManager = DataContextType.CustomsDeclaration.GetUniversalDataContextManager() as IShipmentDataContextManagerInternal;
					var universalFactory = new UniversalObjectFactory(consignment.Factory);
					var declarationReader = declarationContextManager.GetShipmentDataObjectReader(dataObject, new XMLDummyLogger(), universalFactory);
					using (new DisposableAction(() => consignment.Factory.SuspendValidation(), () => consignment.Factory.ResumeValidation()))
					{
						var bizObj = declarationReader.ReadIntoTopLevelBusinessObject();
						if (bizObj is BaseJobDeclaration declaration)
						{
							if (shouldCallFactorySave)
							{
								consignment.Factory.Save();
							}
							else
							{
								using (var transactionManager = ((IDbConnected)declaration.Factory).Connection.BeginTransactionWithManager())
								{
									try
									{
										declaration.PopulateJE_DeclarationReferenceIfNeeded();
										transactionManager.CommitTransaction();
									}
									catch
									{
										transactionManager.RollbackTransaction();
										throw;
									}
								}
							}

							var declarationReference = declaration.JE_DeclarationReference;
							if (!declarationReference.IsEmpty)
							{
								foreach (var convertingConsignment in convertingConsignments)
								{
									convertingConsignment.ULB_IsActive = false;
									convertingConsignment.CE_EntryLineReference = declarationReference.Left(Common.AutoCusEntryNum.Schema.CE_EntryLineReferenceMaxLength);
									convertingConsignment.SetReadOnlyIncludingChildren(true);
									convertingConsignment.ULB_ConvertAction = ZString.Empty;

									if (shouldCallFactorySave)
									{
										convertingConsignment.Factory.Save();
									}
								}

								return 1;
							}
						}

						return 0;
					}
				}
			}
			catch (ZSaveException)
			{
				return 0;
			}
		}

		public class XMLDummyLogger : IXmlImportLogger
		{
			public void Log(LogType type, string message)
			{
			}

			public void LogBoth(LogType type, string message)
			{
			}

			public bool IsUpdatingConsol { get; set; }

			public bool HasIgnoredModule { get; set; }

			public bool OrgMatchingDisabled => false;

			public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKeyy)
			{
			}

			public void FireDataImportedToBusinessObject(BusinessObject targetBO)
			{
			}

			public void LogErrorToServiceTaskOnly(string message)
			{
			}

			public ITopLevelDataObject TopLevelDataObject { get; set; }

			public IDataContextDataObject TopLevelDataContext
			{
				get { return TopLevelDataObject?.DataContext; }
			}

			public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }

			public IEnumerable<ISimpleLog> Logs => logs;
			readonly List<ISimpleLog> logs = new List<ISimpleLog>();
		}
	}
}
