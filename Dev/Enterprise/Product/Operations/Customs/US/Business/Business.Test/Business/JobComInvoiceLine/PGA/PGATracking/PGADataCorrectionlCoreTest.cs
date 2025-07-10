using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ICusEntryLine = Enterprise.Customs.US.Business.MessageBuilders.ICusEntryLine;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestsSubclassesOf(typeof(IPGADataCorrection))]
	public abstract class PGADataCorrectionlCoreTest<T> : DeclarationTestHelper
			where T : IPGADataCorrection
	{
		public void TestPGATrackingIsUnregisterWhenDetach()
		{
			var pga = GetNewPGA();
			SetPGAStatus(pga, PGATrackingStatusList.Codes.Added);
			invoice.JZ_JE = ZGuid.Empty;
			invoiceLine.JI_Description = invoiceLine.JI_Description + "HELLO";
			AssertEquals(PGATrackingStatusList.Codes.Added, pga.TrackingStatusInfo.Value);
		}

		public void TestStatusIsUpdatedWhenParentIsChanged()
		{
			var pga = GetNewPGA();
			SetPGAStatus(pga, PGATrackingStatusList.Codes.Added);
			invoiceLine.JI_Description = "HELLO WORLD";
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, pga.TrackingStatusInfo.Value);
		}

		public void TestStatusIsUpdatedWhenPGAIsChanged()
		{
			var pga = GetNewPGA();
			SetPGAStatus(pga, PGATrackingStatusList.Codes.Added);
			SetPGAData(pga);
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, pga.TrackingStatusInfo.Value);
		}

		protected abstract void SetPGAData(T pga);

		public void TestInvoiceLineFieldsAreSetupCorrectly()
		{
			var pga = GetNewPGA();
			SetPGAStatus(pga, PGATrackingStatusList.Codes.Added);
			var fields = pga.GetRelatedInvoiceLineFields().Concat(new[] { JobComInvoiceLine.Schema.JI_Description }).Distinct().ToList();
			CombineAssertions(() =>
			{
				foreach (var excludeField in pga.GetIndicatorFields().Concat(pga.GetDislaimReasonFields()))
				{
					if (fields.Contains(excludeField))
					{
						Fail(excludeField + " should not be in Invoice Line related fields");
					}
				}
			});
			Factory.Save();

			foreach (var field in fields)
			{
				CombineAssertions(field, () =>
				{
					var newFactory = new BusinessObjectFactory();
					var invoiceLineInDiffFactory = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
					var info = invoiceLineInDiffFactory.ZPropertyInfoHash.GetPropertySafe(field);
					ZPropertyInfoTestHelper.SetValue(info);
					var pgaInDiffFactory = GetPGAInDiffFactory(newFactory, pga);
					AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, pgaInDiffFactory.TrackingStatusInfo.Value);
				});
			}
		}

		public void TestInvoiceFieldsAreSetupCorrectly()
		{
			var pga = GetNewPGA();
			var fields = pga.GetRelatedInvoiceFields();
			if (fields.Length > 0)
			{
				SetPGAStatus(pga, PGATrackingStatusList.Codes.Added);
				Factory.Save();

				foreach (var field in fields)
				{
					CombineAssertions(field, () =>
					{
						var newFactory = new BusinessObjectFactory();
						var invoiceInDiffFactory = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
						var info = invoiceInDiffFactory.ZPropertyInfoHash.GetPropertySafe(field);
						ZPropertyInfoTestHelper.SetValue(info);
						var pgaInDiffFactory = GetPGAInDiffFactory(newFactory, pga);
						AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, pgaInDiffFactory.TrackingStatusInfo.Value);
					});
				}
			}
			Assert("All is good", true);
		}

		public void TestContainerFieldsAreSetupCorrectly()
		{
			var pga = GetNewPGA();
			var fields = pga.GetRelatedContainerFields();
			if (fields.Length > 0)
			{
				SetPGAStatus(pga, PGATrackingStatusList.Codes.Added);
				Factory.Save();

				foreach (var field in fields)
				{
					CombineAssertions(field, () =>
					{
						var newFactory = new BusinessObjectFactory();
						var containerInDiffFactory = newFactory.Load<CusContainer>(container.PK);
						var info = containerInDiffFactory.ZPropertyInfoHash.GetPropertySafe(field);
						ZPropertyInfoTestHelper.SetValue(info);
						var pgaInDiffFactory = GetPGAInDiffFactory(newFactory, pga);
						AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, pgaInDiffFactory.TrackingStatusInfo.Value);
					});
				}
			}
			Assert("All is good", true);
		}

		public void TestDeclarationFieldsAreSetupCorrectly()
		{
			var pga = GetNewPGA();
			var fields = pga.GetRelatedDeclarationFields();
			if (fields.Length > 0)
			{
				SetPGAStatus(pga, PGATrackingStatusList.Codes.Added);
				Factory.Save();

				foreach (var field in fields)
				{
					CombineAssertions(field, () =>
					{
						var newFactory = new BusinessObjectFactory();
						var declarationInDiffFactory = newFactory.Load<JobDeclaration>(declaration.PK);
						var info = declarationInDiffFactory.ZPropertyInfoHash.GetPropertySafe(field);
						ZPropertyInfoTestHelper.SetValue(info);
						var pgaInDiffFactory = GetPGAInDiffFactory(newFactory, pga);
						AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, pgaInDiffFactory.TrackingStatusInfo.Value);
					});
				}
			}
			Assert("All is good", true);
		}

		public void TestUseFetchHints()
		{
			invoice.Delete();
			for (var j = 1; j < 10; j++)
			{
				invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV" + j.ToString();
				for (var k = 1; k < 3; k++)
				{
					invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var previousPGA = default(T);
					for (var i = 1; i < 4; i++)
					{
						var pga = GetNewPGA();
						if (previousPGA != null && object.ReferenceEquals(previousPGA, pga))
						{
							break;
						}

						previousPGA = pga;
					}
				}
			}

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var originalMessage = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			originalMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			originalMessage.EM_Status = EDIMessage.Status.Sent;
			originalMessage.EM_MessageNum = "HYEDUSCMT_148588";

			var responseMessage = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageText =
				"B001101SV9AX                                               HYEDUSCMT_148588     " +
				"Y  1101SV9AX00005";
			responseMessage.EM_MessageNum = "HYEDUSCMT_148588";
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>
			{
				{ JobDeclarationSchema.Constants.TableName, 1 },
				{ CusEntryHeaderSchema.Constants.TableName, 1 },
				{ JobComInvoiceHeaderSchema.Constants.TableName, 1 },
				{ JobComInvoiceLineSchema.Constants.TableName, 1 },
				{ CusAddInfoSchema.Constants.TableName, 1 },
			};
			using (AssertDbHitsForAllFactories("PGA Correction", expectedDbHits,
				hitTolerance: 6,
				useOnlyNewFactories: true,
				acceptableVariance: 1))
			{
				var newFactory = new BusinessObjectFactory();
				var declarationInDiffFactory = newFactory.Load<JobDeclaration>(declaration.PK);
				var responseMessageInDiffFactory = newFactory.Load<MQEDIMessage>(responseMessage.PK);
				var manager = new PGALinesDataCorrectionManager(responseMessageInDiffFactory, declarationInDiffFactory);
				manager.UpdatePGALines(true);
			}
		}

		public void TestStatusForMessageCreationAndProcessing()
		{
			declaration.US_EnableCRL = false;
			declaration.US_EnableENS = true;
			declaration.US_PGAExpeditedRelease = true;
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var pga = SetupDataForMessageCreationAndProcessingTesting();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var originalMessage = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			originalMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			originalMessage.EM_Status = EDIMessage.Status.Sent;
			originalMessage.EM_MessageNum = "HYEDUSCMT_148588";

			var responseMessage = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageText =
				"B001101SV9AX                                               HYEDUSCMT_148588     " +
				"Y  1101SV9AX00005";
			responseMessage.EM_MessageNum = "HYEDUSCMT_148588";

			var entryLine = invoiceLine.CusEntryLine;
			var signed = new Mock<IAcknowledgeAndSign>();
			signed.Setup(m => m.US_CertifyCargoRelease).Returns(true);
			signed.Setup(m => m.CertifyTIB).Returns(false);
			invoiceLine[pga.GetIndicatorFields()[0]] = OGAIndicatorList.Codes.Declared;
			pga.TrackingStatusInfo.Value = ZString.Empty;
			originalMessage.EM_MessageText = GenerateBlocks(entry, entryLine, signed.Object, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			AssertEquals(PGATrackingStatusList.Codes.Adding, pga.TrackingStatusInfo.Value);

			var manager = new PGALinesDataCorrectionManager(responseMessage, declaration);
			manager.UpdatePGALines(false);
			AssertEquals(PGATrackingStatusList.Codes.Added, pga.TrackingStatusInfo.Value);
			pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.Adding;
			manager.UpdatePGALines(true);
			AssertEquals("", pga.TrackingStatusInfo.Value);

			pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.ToBeUpdated;
			originalMessage.EM_MessageText = GenerateBlocks(entry, entryLine, signed.Object, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			AssertEquals(PGATrackingStatusList.Codes.Updating, pga.TrackingStatusInfo.Value);
			manager.UpdatePGALines(false);
			AssertEquals(PGATrackingStatusList.Codes.Added, pga.TrackingStatusInfo.Value);
			pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.Updating;
			manager.UpdatePGALines(true);
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, pga.TrackingStatusInfo.Value);

			pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.ToBeDeleted;
			originalMessage.EM_MessageText = GenerateBlocks(entry, entryLine, signed.Object, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			AssertEquals(PGATrackingStatusList.Codes.Deleting, pga.TrackingStatusInfo.Value);

			originalMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			manager.UpdatePGALines(false);
			AssertEquals(ZString.Empty, pga.TrackingStatusInfo.Value);
			pga.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.Deleting;
			manager.UpdatePGALines(true);
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, pga.TrackingStatusInfo.Value);
		}

		ZString GenerateBlocks(CusEntryHeader entry, IGovernmentAgenciesCommon entryLine, IAcknowledgeAndSign signed, ZString applicationIdentifier)
		{
			var generator = new ACEInputBlockControlGenerator(entry);
			generator.B.ApplicationIdentifierCode = applicationIdentifier;
			generator.AddMessageBlocks(GetBlocks(entryLine, signed, true));
			return generator.Serialise();
		}

		public void TestParentFieldsAreSetupForTracking()
		{
			var pgas = GetEntryForParentFieldsAreSetupForTracking();
			try
			{
				accessedColumnsOnOtherObjects = new Dictionary<string, bool>();
				T pga = pgas[0];
				var entryLines = declaration.ActiveEntryHeaders.SimplifiedEntry.MergedLines;
				Factory.AccessingPersistentValueForTesting += bo_AccessingPersistentValueForTesting;

				var signed = new Mock<IAcknowledgeAndSign>();
				signed.Setup(m => m.US_CertifyCargoRelease).Returns(true);
				signed.Setup(m => m.CertifyTIB).Returns(false);

				foreach (ICusEntryLine entryLine in entryLines)
				{
					GetBlocks(entryLine, signed.Object);
					foreach (var secondaryTariffLine in entryLine.SecondaryTariffLines)
					{
						GetBlocks(secondaryTariffLine, signed.Object);
					}
				}
				var properties = GetPropertiesOfInterface();
				foreach (var pgaCheck in pgas)
				{
					foreach (var property in properties)
					{
						property.GetValue(pgaCheck, null);
					}
				}

				Factory.AccessingPersistentValueForTesting -= bo_AccessingPersistentValueForTesting;

				foreach (var key in ParentColumnsUsedInPGABlocksCreatorToExcludeFromTracking)
				{
					accessedColumnsOnOtherObjects.Remove(key);
				}

				if (pga.GetType().Name.Equals("ACEFDA"))
				{
					var key = GetFieldAccessKey("JE_OH_FDASubmitter", declaration);
					accessedColumnsOnOtherObjects[key] = true;
				}

				CombineAssertions("Field Accessed", () =>
				{
					var fields = pga.GetRelatedInvoiceLineFields().ToList();
					fields.AddRange(pga.GetIndicatorFields());
					fields.AddRange(pga.GetDislaimReasonFields());
					AssertParentFieldsAreSetupForTracking(fields.ToArray(), invoiceLine);
					AssertParentFieldsAreSetupForTracking(pga.GetRelatedInvoiceFields(), invoice);
					AssertParentFieldsAreSetupForTracking(pga.GetRelatedContainerFields(), container);
					AssertParentFieldsAreSetupForTracking(pga.GetRelatedDeclarationFields(), declaration);
					AssertAllParentsFieldsAreSetupForTracking();
				});
			}
			finally
			{
				accessedColumnsOnOtherObjects.Clear();
				accessedColumnsOnOtherObjects = null;
				Factory.AccessingPersistentValueForTesting -= bo_AccessingPersistentValueForTesting;
			}
		}

		PropertyInfo[] GetPropertiesOfInterface()
		{
			var interfaceType = GetInterfaceType();
			AssertEquals("interfaceType.IsInterface", true, interfaceType.IsInterface);
			Assert("Interface should be implemented on ", interfaceType.IsAssignableFrom(typeof(T)));
			return interfaceType.GetProperties().Where(x => x.CanRead && x.DeclaringType == interfaceType).ToArray();
		}

		protected abstract Type GetInterfaceType();

		void AssertAllParentsFieldsAreSetupForTracking()
		{
			var errors = new ZStringBuilder(accessedColumnsOnOtherObjects.Keys.Where(x => !FieldsToExcludeFromTracking(x)));
			if (!errors.IsEmpty)
			{
				Fail("These fields were acccessed but are not tracked:\r\n\r\n" + errors.ToStringWithNewLineBetweenAppends());
			}
		}

		protected virtual bool FieldsToExcludeFromTracking(string fieldName)
		{
			return fieldName.StartsWith("Enterprise.MasterFiles.Business.JobDocAddress.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.GlbBranch.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.GlbCompany.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.OrgAddress.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.OrgAddressCapability.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.OrgCompanyData.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.OrgHeader.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.OrgCusCode.OK_CustomsRegNo")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.OrgMiscServ.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.OrgRateTariffLevel.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.RefCountry.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.RefCountryStates.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.RefCurrency.")
				|| fieldName.StartsWith("Enterprise.MasterFiles.Business.RefUNLOCO.")
				|| fieldName.EndsWith("AddInfoJobComInvoiceLine.US_SupTariff")
				|| fieldName.EndsWith("AddInfoJobDeclaration.US_CargoReleaseType")
				|| fieldName.EndsWith("AddInfoJobDeclaration.US_EnableCRL")
				|| fieldName.EndsWith("AddInfoJobDeclaration.US_EnableENS")
				|| fieldName.EndsWith("AddInfoJobDeclaration.US_EnableSPN")
				|| fieldName.EndsWith("AddInfoJobDeclaration.US_EntryType")
				|| fieldName.EndsWith("AddInfoJobDeclaration.US_TariffType")
				|| fieldName.EndsWith("AddInfoJobDeclaration.US_PGAExpeditedRelease")
				|| fieldName.EndsWith("AddInfoJobComInvoiceHeader.US_TariffType")
				|| fieldName.EndsWith("JobComInvoiceLine.US_TariffType")
				|| fieldName.EndsWith("CusContainerInvoiceLinePivot.C2_CO")
				|| fieldName.EndsWith("JobComInvoiceLine.JI_JZ")
				|| fieldName.EndsWith("JobComInvoiceLine.JI_Tariff")
				|| fieldName.EndsWith("JobComInvoiceLine.JI_ParentID")
				|| fieldName.EndsWith("AddInfoJobComInvoiceLine.US_TSCALineNumber")
				|| fieldName.EndsWith("AddInfoJobComInvoiceLine.US_ODSLineNumber")
				|| fieldName.EndsWith("JobComInvoiceHeader.JZ_JE")
				|| fieldName.EndsWith("JobDeclaration.JE_ApplicationCode")
				|| fieldName.EndsWith("JobDeclaration.JE_GB")
				|| fieldName.EndsWith("JobDeclaration.JE_GC")
				|| fieldName.EndsWith("JobDeclaration.JE_MessageType")
				|| fieldName.EndsWith("CusEntryLine.CL_CH")
				|| fieldName.EndsWith("CusEntryLine.US_SupLine")
				|| fieldName.EndsWith("CusEntryHeader.CH_JE")
				|| fieldName.EndsWith("CusEntryHeader.CH_MessageType")
				|| fieldName.EndsWith("JobComInvoiceHeader.JZ_JZ_GroupInvoiceFK")
				|| fieldName.EndsWith("JobDeclaration.US_F_PNMode")//O is no longer valid
				;
		}

		IEnumerable<ZString> ParentColumnsUsedInPGABlocksCreatorToExcludeFromTracking
		{
			get
			{
				var columnsToExcludeOnInvoiceLine = new List<ZString>()
				{
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NOPInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_ODSInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_TSCAInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_VNEInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_PSTIndicator",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_FSISInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFS370Ind",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFS370DisclaimReason",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSAMRInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSAMRDisclaimReason",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSHMSInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSHMSDisclaimReason",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSSIMPInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NMFSCOAInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_DDTCInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_TTBInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_OMCInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_NHTSAIndicator",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_AMSInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_APHISInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_ATFInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_LaceyIndicator",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_FWSInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_CPSCInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_DEAInd",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_FDAIndicator",
					"Enterprise.Customs.US.Business.JobComInvoiceLine.JI_Description",
					"Enterprise.Customs.US.Business.AddInfoJobDeclaration.US_F_PNMode",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_TSCATrackingStatus",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_ODSTrackingStatus",
					"Enterprise.Customs.US.Business.AddInfoJobComInvoiceLine.US_HFCInd"
				};

				foreach (var column in PGARelatedParentColumnUsedInPGABlocksCreator())
				{
					columnsToExcludeOnInvoiceLine.Remove(column);
				}

				return columnsToExcludeOnInvoiceLine;
			}
		}

		protected virtual List<ZString> PGARelatedParentColumnUsedInPGABlocksCreator()
		{
			return new List<ZString>();
		}

		void AssertParentFieldsAreSetupForTracking(string[] fieldNames, BusinessObject parent)
		{
			var errors = new ZStringBuilder();
			foreach (var fieldName in fieldNames)
			{
				var bizObj = parent;
				var info = parent.ZPropertyInfoHash.GetPropertySafe(fieldName);
				var foundField = false;
				if (info != null)
				{
					var wrappedInfo = info as ZWrappedPropertyInfo;
					if (wrappedInfo != null)
					{
						info = wrappedInfo.InnerInfo;
					}
					var key = GetFieldAccessKey(fieldName, info.BizObj);
					if (foundField = accessedColumnsOnOtherObjects.ContainsKey(key))
					{
						accessedColumnsOnOtherObjects.Remove(key);
					}
				}

				if (!foundField)
				{
					errors.Append(fieldName);
				}
			}
			if (!errors.IsEmpty)
			{
				Fail("Please setup the data so that the following fields of " + parent.GetType().FullName + " is accessed:\r\n\r\n" + errors.ToStringWithNewLineBetweenAppends());
			}
		}

		MessageBlock[] GetBlocks(IGovernmentAgenciesCommon entryLine, IAcknowledgeAndSign signed, bool enableTracking = false)
		{
			return PGABlocksCreator.BuildPGABlocks(entryLine, signed, enableTracking).ToArray();
		}

		protected virtual T SetupDataForMessageCreationAndProcessingTesting()
		{
			return GetNewPGA();
		}

		T[] GetEntryForParentFieldsAreSetupForTracking()
		{
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var result = SetupDataForParentFieldsAreSetupForTracking();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			return result.ToArray();
		}

		protected virtual T[] SetupDataForParentFieldsAreSetupForTracking()
		{
			return new T[] { GetNewPGA() };
		}

		Dictionary<string, bool> accessedColumnsOnOtherObjects;

		void bo_AccessingPersistentValueForTesting(BusinessObject bo, DataColumn column)
		{
			if (!IsTypeToIgnoreAccessCheck(bo))
			{
				var key = GetFieldAccessKey(column.ColumnName, bo);
				accessedColumnsOnOtherObjects[key] = true;
			}
		}

		protected abstract bool IsTypeToIgnoreAccessCheck(BusinessObject bo);

		ZString GetFieldAccessKey(string columnName, BusinessObject bo)
		{
			return bo.GetType().FullName + "." + columnName;
		}

		protected void SetPGAStatus(T pga, ZString status)
		{
			pga.TrackingStatusInfo.Value = status;
		}

		protected abstract T GetNewPGA();
		protected abstract T GetPGAInDiffFactory(BusinessObjectFactory factory, T pga);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT1";
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
		}

		protected CusContainer container;
		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
	}
}
