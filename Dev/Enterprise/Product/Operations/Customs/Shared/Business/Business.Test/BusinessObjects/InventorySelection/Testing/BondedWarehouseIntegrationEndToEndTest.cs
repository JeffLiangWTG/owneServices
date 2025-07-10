using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BondedWarehouseIntegrationEndToEndTest<TDeclaration, TInvoiceLine, TPart, TClassification, TPivot> : TestCaseWithFactory
			where TDeclaration : BaseJobDeclaration
			where TInvoiceLine : BaseJobComInvoiceLine
			where TPart : OrgSupplierPart
			where TClassification : BaseCusClassification
			where TPivot : BaseCusClassPartPivot
	{
		#region Inventory Management Test

		public void TestAutomationIsDisabled()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
				TInvoiceLine inwardInvoiceLine;
				TInvoiceLine inwardInvoiceLine2;
				GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
				var messageInitiator = GetNewMessageInitiator();
				var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
				inwardDeclaration.MessageInitiator = messageInitiator;
				inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
				inwardDeclaration.DoMerge();
				inwardJob.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				var inwardEntryKey = GetEntryKey(inwardJob);
				var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
				var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
				Factory.Save();
				if (ShouldTestInward)
				{
					var originalInwardMessage = SendInwardOriginalMessageViaMenu(inwardJob, inwardDeclaration);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, inwardJob.WarehouseTransactionStatus);
					inwardEntryKey = GetEntryKey(inwardJob);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo2, 0m);
					AssertAdditionalForInwardBeforeOriginalResponseAutomationIsDisabled(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, originalInwardMessage);
					var holdNotes = inwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("No hold notes", 0, holdNotes.Length);

					var originalInwardResponseMessage = CreateInwardOriginalResponseMessage(originalInwardMessage, true);
					originalInwardResponseMessage.Factory.Save();
					ProcessResponse(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, originalInwardMessage, originalInwardResponseMessage);
					originalInwardResponseMessage = new BusinessObjectFactory().Load<EDIMessage>(originalInwardResponseMessage.PK);
					AssertEquals("originalInwardResponseMessage.EM_LinkUniqueID", originalInwardMessage.EM_LinkUniqueID, originalInwardResponseMessage.EM_LinkUniqueID);
					inwardJob = (IWarehouseIntegrationSupporter)originalInwardResponseMessage.Factory.Load(inwardJob.GetType(), inwardJob.PK);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, inwardJob.WarehouseTransactionStatus);
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardEntryKey = GetEntryKey(inwardJob);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo2, 0m);

					var amendmentInwardMessage = SendInwardAmendmentMessageViaMenu(inwardJob, inwardDeclaration);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, inwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo2, 0m);
					AssertAdditionalForInwardBeforeAmendmentResponseAutomationIsDisabled(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, amendmentInwardMessage);

					var amendmentInwardResponseMessage = CreateInwardAmendmentResponseMessage(amendmentInwardMessage, true);
					Factory.Save();
					ProcessResponse(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, amendmentInwardMessage, amendmentInwardResponseMessage);
					amendmentInwardResponseMessage = new BusinessObjectFactory().Load<EDIMessage>(amendmentInwardResponseMessage.PK);
					AssertEquals("amendmentInwardResponseMessage.EM_LinkUniqueID", amendmentInwardMessage.EM_LinkUniqueID, amendmentInwardResponseMessage.EM_LinkUniqueID);
					inwardJob = (IWarehouseIntegrationSupporter)amendmentInwardResponseMessage.Factory.Load(inwardJob.GetType(), inwardJob.PK);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, inwardJob.WarehouseTransactionStatus);
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo2, 0m);

					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo2, 300m);
					inwardJob.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					inwardJob.Factory.Save();

					var withdrawalInwardMessage = SendInwardWithdrawalMessageViaMenu(inwardJob, inwardDeclaration);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, inwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryKey, inwardEntryLineNo2, 300m);
					AssertAdditionalForInwardBeforeWithdrawalResponseAutomationIsDisabled(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, withdrawalInwardMessage);

					var withdrawalInwardResponseMessage = CreateInwardWithdrawalResponseMessage(withdrawalInwardMessage, true);
					withdrawalInwardResponseMessage.Factory.Save();
					ProcessResponse(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, withdrawalInwardMessage, withdrawalInwardResponseMessage);
					withdrawalInwardResponseMessage = new BusinessObjectFactory().Load<EDIMessage>(withdrawalInwardResponseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalInwardMessage.EM_LinkUniqueID, withdrawalInwardResponseMessage.EM_LinkUniqueID);
					inwardJob = (IWarehouseIntegrationSupporter)withdrawalInwardResponseMessage.Factory.Load(inwardJob.GetType(), inwardJob.PK);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, inwardJob.WarehouseTransactionStatus);
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
				}
				else
				{
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, false);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					inwardEntryKey = GetEntryKey(inwardJob);
				}
				var bondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
				var bondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);

				if (ShouldTestOutward)
				{
					var outwardJob = GetNewOutwardJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine outwardInvoiceLine;
					TInvoiceLine outwardInvoiceLine2;
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					var outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					outwardDeclaration.MessageInitiator = messageInitiator;
					outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					outwardDeclaration.DoMerge();
					outwardJob.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					outwardJob.Factory.Save();

					var originalOutwardMessage = SendOutwardOriginalMessageViaMenu(outwardJob, outwardDeclaration);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);
					AssertAdditionalForOutwardBeforeOriginalResponseAutomationIsDisabled(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, originalOutwardMessage, bondedEntryKey, bondedEntryKey2);

					var originalOutwardResponseMessage = CreateOutwardOriginalResponseMessage(originalOutwardMessage, true);
					originalOutwardResponseMessage.Factory.Save();
					ProcessResponse(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, originalOutwardMessage, originalOutwardResponseMessage);
					originalOutwardResponseMessage = new BusinessObjectFactory().Load<EDIMessage>(originalOutwardResponseMessage.PK);
					AssertEquals("originalOutwardResponseMessage.EM_LinkUniqueID", originalOutwardMessage.EM_LinkUniqueID, originalOutwardResponseMessage.EM_LinkUniqueID);
					outwardJob = (IWarehouseIntegrationSupporter)originalOutwardResponseMessage.Factory.Load(outwardJob.GetType(), outwardJob.PK);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, outwardJob.WarehouseTransactionStatus);
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);

					var amendmentOutwardMessage = SendOutwardAmendmentMessageViaMenu(outwardJob, outwardDeclaration);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);
					AssertAdditionalForOutwardBeforeAmendmentResponseAutomationIsDisabled(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, amendmentOutwardMessage, bondedEntryKey, bondedEntryKey2);

					var amendmentOutwardResponseMessage = CreateOutwardAmendmentResponseMessage(amendmentOutwardMessage, true);
					amendmentOutwardResponseMessage.Factory.Save();
					ProcessResponse(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, amendmentOutwardMessage, amendmentOutwardResponseMessage);
					amendmentOutwardResponseMessage = new BusinessObjectFactory().Load<EDIMessage>(amendmentOutwardResponseMessage.PK);
					AssertEquals("amendmentOutwardResponseMessage.EM_LinkUniqueID", amendmentOutwardMessage.EM_LinkUniqueID, amendmentOutwardResponseMessage.EM_LinkUniqueID);
					outwardJob = (IWarehouseIntegrationSupporter)amendmentOutwardResponseMessage.Factory.Load(outwardJob.GetType(), outwardJob.PK);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, outwardJob.WarehouseTransactionStatus);
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);

					outwardJob.PublishShipmentForWHSOutward(false);
					outwardJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					outwardJob.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					outwardJob.Factory.Save();

					var withdrawalOutwardMessage = SendOutwardWithdrawalMessageViaMenu(outwardJob, outwardDeclaration);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					AssertAdditionalForOutwardBeforeWithdrawalResponseAutomationIsDisabled(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, withdrawalOutwardMessage, bondedEntryKey, bondedEntryKey2);

					var withdrawalOutwardResponseMessage = CreateOutwardWithdrawalResponseMessage(withdrawalOutwardMessage, true);
					withdrawalOutwardResponseMessage.Factory.Save();
					ProcessResponse(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, withdrawalOutwardMessage, withdrawalOutwardResponseMessage);
					withdrawalOutwardResponseMessage = new BusinessObjectFactory().Load<EDIMessage>(withdrawalOutwardResponseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalOutwardMessage.EM_LinkUniqueID, withdrawalOutwardResponseMessage.EM_LinkUniqueID);
					outwardJob = (IWarehouseIntegrationSupporter)withdrawalOutwardResponseMessage.Factory.Load(outwardJob.GetType(), outwardJob.PK);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, outwardJob.WarehouseTransactionStatus);
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
				}
			}
		}

		protected virtual void AssertAdditionalForOutwardBeforeWithdrawalResponseAutomationIsDisabled(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage withdrawalOutwardMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
		}

		protected virtual void AssertAdditionalForOutwardBeforeAmendmentResponseAutomationIsDisabled(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage amendmentOutwardMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
		}

		protected virtual void AssertAdditionalForOutwardBeforeOriginalResponseAutomationIsDisabled(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage originalOutwardMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
		}

		protected abstract void ProcessResponse(IWarehouseIntegrationSupporter job, TDeclaration declaration, TInvoiceLine invoiceLine, TInvoiceLine invoiceLine2, EDIMessage outgoingMessage, EDIMessage incomingMessage);

		protected virtual void AssertAdditionalForInwardBeforeWithdrawalResponseAutomationIsDisabled(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage withdrawalInwardMessage)
		{
		}

		protected virtual void AssertAdditionalForInwardBeforeAmendmentResponseAutomationIsDisabled(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage amendmentInwardMessage)
		{
		}

		protected virtual void AssertAdditionalForInwardBeforeOriginalResponseAutomationIsDisabled(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage originalInwardMessage)
		{
		}

		public void TestBondedWarehouseTransactionReportFields()
		{
			// Setup Inward Job
			// Update Bonded Whs
			// Check warehouse fields

			// Setup Outward Job
			// Update Bonded Whs
			// Check warehouse fields

			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
				TInvoiceLine inwardInvoiceLine;
				TInvoiceLine inwardInvoiceLine2;
				GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
				var messageInitiator = GetNewMessageInitiator();
				var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
				inwardDeclaration.MessageInitiator = messageInitiator;
				inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
				inwardDeclaration.DoMerge();
				SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, false);
				Factory.Save();
				var publishResult = inwardJob.PublishShipmentForWHSInward(false);
				inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
				var inwardEntryKey = GetEntryKey(inwardJob);
				var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
				var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
				var bondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
				var bondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);
				AssertWarehouseTransactionData(publishResult, ZString.Empty, bondedEntryKey, inwardEntryKey, inwardEntryLineNo, bondedEntryKey2, inwardEntryKey, inwardEntryLineNo2);

				if (ShouldTestOutward)
				{
					var outwardJob = GetNewOutwardJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine outwardInvoiceLine;
					TInvoiceLine outwardInvoiceLine2;
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					var outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					outwardDeclaration.MessageInitiator = messageInitiator;
					outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					outwardDeclaration.DoMerge();
					SetupOutwardOriginalClearState(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2);
					Factory.Save();
					publishResult = outwardJob.PublishShipmentForWHSOutward(false);
					outwardJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					var outwardEntryKey = GetEntryKey(outwardJob);
					var outwardEntryLineNo = outwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var outwardEntryLineNo2 = outwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					AssertNotEquals("outwardEntryKey should be different to inwardEntryKey", outwardEntryKey, inwardEntryKey);
					AssertWarehouseTransactionData(publishResult, outwardEntryKey, bondedEntryKey, outwardEntryKey, outwardEntryLineNo, bondedEntryKey2, outwardEntryKey, outwardEntryLineNo2);
				}
			}
		}

		protected void AssertWarehouseTransactionData(PublishToUniversalResult publishResult, ZString customerReference, ZString bondedEntryKey1, ZString entryKey1, ZShort entryLineNo1, ZString? bondedEntryKey2 = null, ZString? entryKey2 = null, ZShort? entryLineNo2 = null)
		{
			AssertWarehouseTransactionData((IWhsDocket)publishResult.FindJobIfExists(), customerReference, bondedEntryKey1, entryKey1, entryLineNo1, bondedEntryKey2, entryKey2, entryLineNo2);
		}

		protected void AssertWarehouseTransactionData(IWhsDocket docket, ZString customerReference, ZString bondedEntryKey1, ZString entryKey1, ZShort entryLineNo1, ZString? bondedEntryKey2 = null, ZString? entryKey2 = null, ZShort? entryLineNo2 = null)
		{
			AssertNotNull(docket);
			AssertEquals("docket.WD_CustomerReference", customerReference, docket.WD_CustomerReference);
			var docketLineQuery = new ZQuery(WhsDocketLineSchema.WE_WD, docket.PK);
			docketLineQuery.OrderBy = WhsDocketLineSchema.WE_BondedEntryKey.Name;
			var docketLines = Factory.Load<IWhsDocketLine>(docketLineQuery);
			AssertWarehouseTransactionData(docketLines[0], bondedEntryKey1, entryKey1, entryLineNo1);
			if (bondedEntryKey2.HasValue)
			{
				AssertWarehouseTransactionData(docketLines[1], bondedEntryKey2.Value, entryKey2.Value, entryLineNo2.Value);
			}
		}

		protected void AssertWarehouseTransactionData(IWhsDocketLine docketLine, ZString bondedEntryKey, ZString entryKey, ZShort entryLineNo)
		{
			AssertEquals("docketLine.WE_BondedEntryKey", bondedEntryKey, docketLine.WE_BondedEntryKey);
			var docketLineAttribute = Factory.LoadTop1<IWhsBondedWarehouseAttribute>(new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, docketLine.PK));
			AssertEquals("docketLineAttribute.WB_EntryKey", entryKey, docketLineAttribute.WB_EntryKey);
			AssertEquals("docketLineAttribute.WB_EntryLineNo", entryLineNo, docketLineAttribute.WB_EntryLineNo);
		}

		#region Inward Testing

		public void TestInwardFieldsValidation()
		{
			if (ShouldTestInward)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, false);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var bondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var bondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);
					inwardInvoiceLine.JI_PartNo = ZString.Empty;
					Factory.Save();
					SendInwardAmendmentMessageViaMenu(inwardJob, inwardDeclaration);
					AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);

					inwardJob.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					Factory.Save();
					messageInitiator.InvalidOperationText = null;
					SendInwardAmendmentMessageViaMenu(inwardJob, inwardDeclaration);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);

					inwardInvoiceLine.JI_PartNo = helper.Part.OP_PartNum;
					inwardJob.WarehouseTransactionStatus = ZString.Empty;
					Factory.Save();
					AssertAdditionalInwardFieldsValidation(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
				}
			}
			else
			{
				Assert("Inward is not tested", true);
			}
		}

		protected abstract EDIMessage SendInwardOriginalMessageViaMenu(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration);
		protected virtual void AssertAdditionalInwardFieldsValidation(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2)
		{
		}

		public void TestHandleInwardOnOriginalError()
		{
			if (ShouldTestInward)
			{
				// Setup Inward Job
				// Create Original Message
				// Create Error Orginal Message
				// Check that no inward was created

				var helper = new WhsDataTestHelper(Factory);
				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					Factory.Save();
					var originalMessage = SendInwardOriginalMessageViaMenu(inwardJob, inwardDeclaration);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, inwardJob.WarehouseTransactionStatus);
					AssertNotNull("DEX for inward original", inwardJob.Logs.MostRecentLogByEventTime(Events.DataExport));
					var entryKey = GetEntryKey(inwardJob);
					var entryLineNo1 = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var entryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 0m);
					AssertAdditionalForInwardBeforeOriginalResponse(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, originalMessage);
					var holdNotes = inwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals(1, holdNotes.Length);
					var holdNote = holdNotes[0];
					AssertEquals("holdNote.ReadOnly", true, holdNote.ReadOnly);
					AssertNotNull(inwardJob.GetLastHoldUniversalShipmentFromNote());

					var responseMessage = CreateInwardOriginalResponseMessage(originalMessage, false);
					Factory.Save();
					ProcessResponseAndAssertInwardOnOriginalError(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, originalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					inwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(inwardJob.GetType(), inwardJob.PK);
					AssertEquals("inwardJob.WarehouseTransactionStatus", ZString.Empty, inwardJob.WarehouseTransactionStatus);
					entryKey = GetEntryKey(inwardJob);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 0m);
					AssertEquals("Hold notes should have been deleted", 0, inwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
				}
			}
			else
			{
				Assert("Inward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertInwardOnOriginalError(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage);
		protected abstract EDIMessage CreateInwardOriginalResponseMessage(EDIMessage originalMessage, bool isSuccess);
		protected virtual void AssertAdditionalForInwardBeforeOriginalResponse(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage originalMessage)
		{
		}

		public void TestHandleInwardOnOriginalClear()
		{
			if (ShouldTestInward)
			{
				// Setup Inward Job
				// Create Original Message
				// Create Clear Orginal Message
				// Check that inward was created

				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					Factory.Save();
					var originalMessage = SendInwardOriginalMessageViaMenu(inwardJob, inwardDeclaration);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, inwardJob.WarehouseTransactionStatus);
					AssertNotNull("DEX for inward original", inwardJob.Logs.MostRecentLogByEventTime(Events.DataExport));
					var entryKey = GetEntryKey(inwardJob);
					var entryLineNo1 = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var entryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 0m);
					AssertAdditionalForInwardBeforeOriginalResponse(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, originalMessage);

					var responseMessage = CreateInwardOriginalResponseMessage(originalMessage, true);
					Factory.Save();
					ProcessResponseAndAssertInwardOnOriginalClear(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, originalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					inwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(inwardJob.GetType(), inwardJob.PK);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					entryKey = GetEntryKey(inwardJob);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 300m);

					var subQuery = new ZDBOnlySubQuery(typeof(IWhsReceiveLine), WhsDocketLineSchema.WE_WD);
					subQuery.AddToFilter(WhsDocketLineSchema.WE_BondedEntryKey, SQLComparisonOperator.StartsWith, entryKey);
					var query = new ZDBOnlyQuery(typeof(IWhsReceive));
					query.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");
					query.AddToFilter(WhsDocketSchema.WD_DocketStatus, "FIN");
					query.AddSubQuery(WhsDocketSchema.PK, subQuery, JoinCondition.And);
					query.OrderBy = WhsDocketSchema.WD_SystemCreateTimeUtc.Name + " DESC, " + WhsDocketSchema.WD_ExternalReferenceSplit.Name + " DESC";
					AssertWarehouseTransactionData(Factory.LoadTop1<IWhsReceive>(query), ZString.Empty, GetBondedEntryKey(entryKey, entryLineNo1), entryKey, entryLineNo1, GetBondedEntryKey(entryKey, entryLineNo2), entryKey, entryLineNo2);
				}
			}
			else
			{
				Assert("Inward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertInwardOnOriginalClear(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage);

		public void TestHandleInwardOnAmendmentError()
		{
			if (ShouldTestInward)
			{
				// Setup Inward Job
				// Update Bonded Whs
				// Change Quantity
				// Create Amendment Message
				// Create Error Amendment Message
				// Check that same inventory amount exists

				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var entryKey = GetEntryKey(inwardJob);
					var entryLineNo1 = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var entryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 300m);
					UpdateQuantity(inwardInvoiceLine, 200m);
					UpdateQuantity(inwardInvoiceLine2, 150m);
					inwardDeclaration.DoMerge();
					Factory.Save();

					var dataExortLogs = inwardJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					var amendmentMessage = SendInwardAmendmentMessageViaMenu(inwardJob, inwardDeclaration);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, inwardJob.WarehouseTransactionStatus);
					var dataExortLogsAfter = inwardJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					AssertEquals("One new DEX Log", dataExortLogs.Length + 1, dataExortLogsAfter.Length);
					var dataExportLog = dataExortLogsAfter[0];
					AssertCollectionNotContains(dataExportLog, dataExortLogs);
					AssertNotNull("Should be a HOLD xml", dataExportLog.RelatedEDIMessage.DataContext.RecipientRoleCollection.First(x => x.Code.HasValue && x.Code.Value == RecipientRoleType.BWI && x.ServiceCode.HasValue && x.ServiceCode.Value == ServiceCodeType.HLD));
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 0m);
					AssertAdditionalForInwardBeforeAmendmentResponse(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, amendmentMessage);

					var responseMessage = CreateInwardAmendmentResponseMessage(amendmentMessage, false);
					UpdateQuantity(inwardInvoiceLine, 230m);
					UpdateQuantity(inwardInvoiceLine2, 120m);
					Factory.Save();
					ProcessResponseAndAssertInwardOnAmendmentError(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, amendmentMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", amendmentMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					inwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(inwardJob.GetType(), inwardJob.PK);
					AssertEquals("inwardJob.WarehouseTransactionStatus", ExpectedInwardAmendmentErrorWHSStatus, inwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, ExpectedInwardAmendmentErrorInventoryQuantity);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, ExpectedInwardAmendmentErrorInventoryQuantity2);
				}
			}
			else
			{
				Assert("Inward is not tested", true);
			}
		}

		protected virtual ZString ExpectedInwardAmendmentErrorWHSStatus => WarehouseTransactionStatusList.Codes.InwardUpdated;
		protected virtual ZDecimal ExpectedInwardAmendmentErrorInventoryQuantity => 100m;
		protected virtual ZDecimal ExpectedInwardAmendmentErrorInventoryQuantity2 => 300m;

		protected abstract void ProcessResponseAndAssertInwardOnAmendmentError(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage);
		protected abstract EDIMessage CreateInwardAmendmentResponseMessage(EDIMessage amendmentMessage, bool isSuccess);
		protected virtual void AssertAdditionalForInwardBeforeAmendmentResponse(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage amendmentMessage)
		{
		}

		protected abstract EDIMessage SendInwardAmendmentMessageViaMenu(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration);
		protected virtual void UpdateQuantity(TInvoiceLine invoiceLine, ZDecimal quantity)
		{
			invoiceLine.JI_BondedWhsQuantity = quantity;
		}

		protected abstract void SetupInwardOriginalClearState(IWarehouseIntegrationSupporter inwardjob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, bool createOriginalResponseMessage = true);

		public void TestHandleInwardOnAmendmentClear()
		{
			if (ShouldTestInward)
			{
				// Setup Inward Job
				// Update Bonded Whs
				// Change Quantity
				// Create Amendment Message
				// Create Clear Amendment Message
				// Check that inventory amount was updated

				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var entryKey = GetEntryKey(inwardJob);
					var entryLineNo1 = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var entryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 300m);
					UpdateQuantity(inwardInvoiceLine, 200m);
					UpdateQuantity(inwardInvoiceLine2, 150m);
					inwardDeclaration.DoMerge();
					Factory.Save();

					var dataExortLogs = inwardJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					var amendmentMessage = SendInwardAmendmentMessageViaMenu(inwardJob, inwardDeclaration);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, inwardJob.WarehouseTransactionStatus);
					var dataExortLogsAfter = inwardJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					AssertEquals("One new DEX Log", dataExortLogs.Length + 1, dataExortLogsAfter.Length);
					var dataExportLog = dataExortLogsAfter[0];
					AssertCollectionNotContains(dataExportLog, dataExortLogs);
					AssertNotNull("Should be a HOLD xml", dataExportLog.RelatedEDIMessage.DataContext.RecipientRoleCollection.First(x => x.Code.HasValue && x.Code.Value == RecipientRoleType.BWI && x.ServiceCode.HasValue && x.ServiceCode.Value == ServiceCodeType.HLD));
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 0m);
					AssertAdditionalForInwardBeforeAmendmentResponse(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, amendmentMessage);

					var responseMessage = CreateInwardAmendmentResponseMessage(amendmentMessage, true);
					UpdateQuantity(inwardInvoiceLine, 230m);
					UpdateQuantity(inwardInvoiceLine2, 120m);
					Factory.Save();
					ProcessResponseAndAssertInwardOnAmendmentClear(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, amendmentMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", amendmentMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					inwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(inwardJob.GetType(), inwardJob.PK);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, inwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 200m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 150m);
				}
			}
			else
			{
				Assert("Inward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertInwardOnAmendmentClear(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage);

		public void TestHandleInwardOnWithdrawalError()
		{
			if (ShouldTestInward)
			{
				// Setup Inward Job
				// Update Bonded Whs
				// Create Withdrawal Message
				// Create Error Withdrawal Message
				// Check that same inventory amount exists

				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var entryKey = GetEntryKey(inwardJob);
					var entryLineNo1 = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var entryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 300m);
					UpdateQuantity(inwardInvoiceLine, 200m);
					UpdateQuantity(inwardInvoiceLine2, 150m);
					inwardDeclaration.DoMerge();
					Factory.Save();

					var dataExortLogs = inwardJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					var withdrawalMessage = SendInwardWithdrawalMessageViaMenu(inwardJob, inwardDeclaration);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, inwardJob.WarehouseTransactionStatus);
					var dataExortLogsAfter = inwardJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					AssertEquals("One new DEX Log", dataExortLogs.Length + 1, dataExortLogsAfter.Length);
					var dataExportLog = dataExortLogsAfter[0];
					AssertCollectionNotContains(dataExportLog, dataExortLogs);
					AssertEquals("dataExportLog.RelatedEDIMessage.Message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, dataExportLog.RelatedEDIMessage.Message.EM_MessageSubType);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 0m);
					AssertAdditionalForInwardBeforeWithdrawalResponse(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, withdrawalMessage);

					var responseMessage = CreateInwardWithdrawalResponseMessage(withdrawalMessage, false);
					Factory.Save();
					ProcessResponseAndAssertInwardOnWithdrawalError(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, withdrawalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					inwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(inwardJob.GetType(), inwardJob.PK);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, inwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 300m);
				}
			}
			else
			{
				Assert("Inward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertInwardOnWithdrawalError(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage);
		protected abstract EDIMessage CreateInwardWithdrawalResponseMessage(EDIMessage withdrawalMessage, bool isSuccess);
		protected abstract EDIMessage SendInwardWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration);
		protected virtual void AssertAdditionalForInwardBeforeWithdrawalResponse(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage withdrawalMessage)
		{
		}

		public void TestHandleInwardOnWithdrawalClear()
		{
			if (ShouldTestInward)
			{
				// Setup Inward Job
				// Update Bonded Whs
				// Create Withdrawal Message
				// Create Clear Withdrawal Message
				// Check that Inward was cancelled

				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var entryKey = GetEntryKey(inwardJob);
					var entryLineNo1 = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var entryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 300m);
					UpdateQuantity(inwardInvoiceLine, 200m);
					UpdateQuantity(inwardInvoiceLine2, 150m);
					inwardDeclaration.DoMerge();
					Factory.Save();

					var dataExortLogs = inwardJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					var withdrawalMessage = SendInwardWithdrawalMessageViaMenu(inwardJob, inwardDeclaration);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, inwardJob.WarehouseTransactionStatus);
					var dataExortLogsAfter = inwardJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					AssertEquals("One new DEX Log", dataExortLogs.Length + 1, dataExortLogsAfter.Length);
					var dataExportLog = dataExortLogsAfter[0];
					AssertCollectionNotContains(dataExportLog, dataExortLogs);
					AssertEquals("dataExportLog.RelatedEDIMessage.Message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, dataExportLog.RelatedEDIMessage.Message.EM_MessageSubType);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 0m);
					AssertAdditionalForInwardBeforeWithdrawalResponse(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, withdrawalMessage);

					var responseMessage = CreateInwardWithdrawalResponseMessage(withdrawalMessage, true);
					Factory.Save();
					ProcessResponseAndAssertInwardOnWithdrawalClear(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2, withdrawalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					inwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(inwardJob.GetType(), inwardJob.PK);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceled, inwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo1, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryKey, entryLineNo2, 0m);
				}
			}
			else
			{
				Assert("Inward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertInwardOnWithdrawalClear(IWarehouseIntegrationSupporter inwardJob, TDeclaration inwardDeclaration, TInvoiceLine inwardInvoiceLine, TInvoiceLine inwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage);

		#endregion

		#region Outward Testing

		public void TestOutwardFieldsValidation()
		{
			if (ShouldTestOutward)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var bondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var bondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);

					var outwardJob = GetNewOutwardJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine outwardInvoiceLine;
					TInvoiceLine outwardInvoiceLine2;
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					var outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					outwardDeclaration.MessageInitiator = messageInitiator;
					outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					outwardDeclaration.DoMerge();
					SetupOutwardOriginalClearState(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2);
					Factory.Save();
					outwardJob.PublishShipmentForWHSOutward(false);
					outwardJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					UpdateQuantity(outwardInvoiceLine, 70m);
					outwardDeclaration.DoMerge();
					outwardInvoiceLine.JI_PartNo = ZString.Empty;
					Factory.Save();
					SendOutwardAmendmentMessageViaMenu(outwardJob, outwardDeclaration);
					AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);

					outwardJob.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					Factory.Save();
					messageInitiator.InvalidOperationText = null;
					SendOutwardAmendmentMessageViaMenu(outwardJob, outwardDeclaration);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);

					outwardInvoiceLine.JI_PartNo = helper.Part.OP_PartNum;
					outwardJob.WarehouseTransactionStatus = ZString.Empty;
					Factory.Save();
					AssertAdditionalOutwardFieldsValidation(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2);
				}
			}
			else
			{
				Assert("Outward is not tested", true);
			}
		}

		protected abstract EDIMessage SendOutwardOriginalMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration);
		protected virtual void AssertAdditionalOutwardFieldsValidation(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2)
		{
		}

		public void TestHandleOutwardOnOriginalError()
		{
			if (ShouldTestOutward)
			{
				// Setup Inward Job
				// Update Bonded Whs

				// Setup Outward Job
				// Create Original Message
				// Create Error Orginal Message
				// Check that no outward was created

				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var bondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var bondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);

					var outwardJob = GetNewOutwardJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine outwardInvoiceLine;
					TInvoiceLine outwardInvoiceLine2;
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					var outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					outwardDeclaration.MessageInitiator = messageInitiator;
					outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					outwardDeclaration.DoMerge();
					Factory.Save();
					var originalMessage = SendOutwardOriginalMessageViaMenu(outwardJob, outwardDeclaration);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					AssertAdditionalForOutwardBeforeOriginalResponse(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, originalMessage, bondedEntryKey, bondedEntryKey2);

					var responseMessage = CreateOutwardOriginalResponseMessage(originalMessage, false);
					Factory.Save();
					ProcessResponseAndAssertOutwardOnOriginalError(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, originalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					outwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(outwardJob.GetType(), outwardJob.PK);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);
				}
			}
			else
			{
				Assert("Outward is not tested", true);
			}
		}

		protected abstract EDIMessage CreateOutwardOriginalResponseMessage(EDIMessage originalMessage, bool isSuccess);
		protected abstract void ProcessResponseAndAssertOutwardOnOriginalError(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage);
		protected virtual void AssertAdditionalForOutwardBeforeOriginalResponse(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage originalMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
		}

		public void TestHandleOutwardOnOriginalClear()
		{
			if (ShouldTestOutward)
			{
				// Setup Inward Job
				// Update Bonded Whs

				// Setup Outward Job
				// Create Original Message
				// Create Clear Orginal Message
				// Check that outward was created

				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var bondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var bondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);

					var outwardJob = GetNewOutwardJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine outwardInvoiceLine;
					TInvoiceLine outwardInvoiceLine2;
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					var outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					outwardDeclaration.MessageInitiator = messageInitiator;
					outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					outwardDeclaration.DoMerge();
					Factory.Save();
					var originalMessage = SendOutwardOriginalMessageViaMenu(outwardJob, outwardDeclaration);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					AssertAdditionalForOutwardBeforeOriginalResponse(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, originalMessage, bondedEntryKey, bondedEntryKey2);

					var responseMessage = CreateOutwardOriginalResponseMessage(originalMessage, true);
					Factory.Save();
					ProcessResponseAndAssertOutwardOnOriginalClear(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, originalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					outwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(outwardJob.GetType(), outwardJob.PK);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);

					var subQuery = new ZDBOnlySubQuery(typeof(IWhsDocketLine), WhsDocketLineSchema.WE_WD);
					subQuery.AddToFilter(WhsDocketLineSchema.WE_BondedEntryKey, SQLComparisonOperator.StartsWith, inwardEntryKey);
					var query = new ZDBOnlyQuery(typeof(IWhsOrder));
					query.AddToFilter(WhsDocketSchema.WD_DocketType, "ORD");
					query.AddToFilter(WhsDocketSchema.WD_FinalisedDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
					query.AddSubQuery(WhsDocketSchema.PK, subQuery, JoinCondition.And);
					query.OrderBy = WhsDocketSchema.WD_SystemCreateTimeUtc.Name + " DESC, " + WhsDocketSchema.WD_ExternalReferenceSplit.Name + " DESC";
					var outwardEntryKey = GetEntryKey(outwardJob);
					var outwardEntryLineNo = outwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var outwardEntryLineNo2 = outwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					AssertNotEquals("outwardEntryKey should be different to inwardEntryKey", outwardEntryKey, inwardEntryKey);
					AssertWarehouseTransactionData(Factory.LoadTop1<IWhsOrder>(query), outwardEntryKey, bondedEntryKey, outwardEntryKey, outwardEntryLineNo, bondedEntryKey2, outwardEntryKey, outwardEntryLineNo2);
				}
			}
			else
			{
				Assert("Outward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertOutwardOnOriginalClear(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage);

		public void TestHandleOutwardOnAmendmentError()
		{
			if (ShouldTestOutward)
			{
				// Setup Inward Job
				// Update Bonded Whs

				// Setup Outward Job
				// Update Bonded Whs
				// Change Quantity
				// Create Amendment Message
				// Create Error Amendment Message
				// Check that same inventory amount exists

				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var bondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var bondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);

					var outwardJob = GetNewOutwardJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine outwardInvoiceLine;
					TInvoiceLine outwardInvoiceLine2;
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					var outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					outwardDeclaration.MessageInitiator = messageInitiator;
					outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					outwardDeclaration.DoMerge();
					SetupOutwardOriginalClearState(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2);
					Factory.Save();
					outwardJob.PublishShipmentForWHSOutward(false);
					outwardJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					UpdateQuantity(outwardInvoiceLine, 70m);
					UpdateQuantity(outwardInvoiceLine2, 120m);
					outwardDeclaration.DoMerge();
					Factory.Save();
					var holdNotes = outwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("No hold notes", 0, holdNotes.Length);

					var amendmentMessage = SendOutwardAmendmentMessageViaMenu(outwardJob, outwardDeclaration);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 30m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					holdNotes = outwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("Hold note has been created", 1, holdNotes.Length);
					AssertAdditionalForOutwardBeforeAmendmentResponse(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, amendmentMessage, bondedEntryKey, bondedEntryKey2);

					var responseMessage = CreateOutwardAmendmentResponseMessage(amendmentMessage, false);
					UpdateQuantity(outwardInvoiceLine, 80m);
					UpdateQuantity(outwardInvoiceLine2, 90m);
					Factory.Save();
					ProcessResponseAndAssertOutwardOnAmendmentError(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, amendmentMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", amendmentMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					outwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(outwardJob.GetType(), outwardJob.PK);
					AssertEquals("outwardJob.WarehouseTransactionStatus", ExpectedOutwardAmendmentErrorWHSStatus, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, ExpectedOutwardAmendmentErrorInventoryQuantity);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, ExpectedOutwardAmendmentErrorInventoryQuantity2);
					AssertEquals("Hold notes should have been deleted", 0, outwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
				}
			}
			else
			{
				Assert("Outward is not tested", true);
			}
		}

		protected virtual ZString ExpectedOutwardAmendmentErrorWHSStatus => WarehouseTransactionStatusList.Codes.OutwardUpdated;
		protected virtual ZDecimal ExpectedOutwardAmendmentErrorInventoryQuantity => 40m;
		protected virtual ZDecimal ExpectedOutwardAmendmentErrorInventoryQuantity2 => 150m;

		protected abstract void ProcessResponseAndAssertOutwardOnAmendmentError(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage);
		protected abstract EDIMessage CreateOutwardAmendmentResponseMessage(EDIMessage amendmentMessage, bool isSuccess);
		protected abstract EDIMessage SendOutwardAmendmentMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration);
		protected abstract void SetupOutwardOriginalClearState(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, bool createOriginalResponseMessage = true);
		protected virtual void AssertAdditionalForOutwardBeforeAmendmentResponse(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage amendmentMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
		}

		public void TestHandleOutwardOnAmendmentClear()
		{
			if (ShouldTestOutward)
			{
				// Setup Inward Job
				// Update Bonded Whs

				// Setup Outward Job
				// Update Bonded Whs
				// Change Quantity
				// Create Amendment Message
				// Create Clear Amendment Message
				// Check that inventory amount was updated
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var bondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var bondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);

					var outwardJob = GetNewOutwardJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine outwardInvoiceLine;
					TInvoiceLine outwardInvoiceLine2;
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					var outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					outwardDeclaration.MessageInitiator = messageInitiator;
					outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					outwardDeclaration.DoMerge();
					SetupOutwardOriginalClearState(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2);
					Factory.Save();
					outwardJob.PublishShipmentForWHSOutward(false);
					outwardJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					UpdateQuantity(outwardInvoiceLine, 70m);
					UpdateQuantity(outwardInvoiceLine2, 120m);
					outwardDeclaration.DoMerge();
					Factory.Save();
					var holdNotes = outwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("No hold notes", 0, holdNotes.Length);

					var amendmentMessage = SendOutwardAmendmentMessageViaMenu(outwardJob, outwardDeclaration);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 30m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					holdNotes = outwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("Hold note has been created", 1, holdNotes.Length);
					AssertAdditionalForOutwardBeforeAmendmentResponse(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, amendmentMessage, bondedEntryKey, bondedEntryKey2);

					var responseMessage = CreateOutwardAmendmentResponseMessage(amendmentMessage, true);
					UpdateQuantity(outwardInvoiceLine, 80m);
					UpdateQuantity(outwardInvoiceLine2, 90m);
					Factory.Save();
					ProcessResponseAndAssertOutwardOnAmendmentClear(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, amendmentMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", amendmentMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					outwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(outwardJob.GetType(), outwardJob.PK);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 30m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 180m);
					AssertEquals("Hold notes should have been deleted", 0, outwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
				}
			}
			else
			{
				Assert("Outward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertOutwardOnAmendmentClear(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage);

		public void TestHandleOutwardOnWithdrawalError()
		{
			if (ShouldTestOutward)
			{
				// Setup Inward Job
				// Update Bonded Whs

				// Setup Outward Job
				// Update Bonded Whs
				// Create Withdrawal Message
				// Create Error Withdrawal Message
				// Check that same inventory amount exists

				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var bondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var bondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);

					var outwardJob = GetNewOutwardJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine outwardInvoiceLine;
					TInvoiceLine outwardInvoiceLine2;
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					var outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					outwardDeclaration.MessageInitiator = messageInitiator;
					outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					outwardDeclaration.DoMerge();
					SetupOutwardOriginalClearState(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2);
					Factory.Save();
					outwardJob.PublishShipmentForWHSOutward(false);
					outwardJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					UpdateQuantity(outwardInvoiceLine, 70m);
					UpdateQuantity(outwardInvoiceLine2, 120m);
					outwardDeclaration.DoMerge();
					Factory.Save();

					var withdrawalMessage = SendOutwardWithdrawalMessageViaMenu(outwardJob, outwardDeclaration);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					AssertAdditionalForOutwardBeforeWithdrawalResponse(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, withdrawalMessage, bondedEntryKey, bondedEntryKey2);

					var responseMessage = CreateOutwardWithdrawalResponseMessage(withdrawalMessage, false);
					Factory.Save();
					ProcessResponseAndAssertOutwardOnWithdrawalError(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, withdrawalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					outwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(outwardJob.GetType(), outwardJob.PK);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
				}
			}
			else
			{
				Assert("Outward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertOutwardOnWithdrawalError(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage);
		protected abstract EDIMessage CreateOutwardWithdrawalResponseMessage(EDIMessage withdrawalMessage, bool isSuccess);
		protected abstract EDIMessage SendOutwardWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration);
		protected virtual void AssertAdditionalForOutwardBeforeWithdrawalResponse(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage withdrawalMessage, ZString bondedEntryKey, ZString bondedEntryKey2)
		{
		}

		public void TestHandleOutwardOnWithdrawalClear()
		{
			if (ShouldTestOutward)
			{
				// Setup Inward Job
				// Update Bonded Whs

				// Setup Outward Job
				// Update Bonded Whs
				// Create Withdrawal Message
				// Create Clear Withdrawal Message
				// Check that Outward was cancelled

				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var bondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var bondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);

					var outwardJob = GetNewOutwardJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine outwardInvoiceLine;
					TInvoiceLine outwardInvoiceLine2;
					GetInvoiceLines(outwardJob, out outwardInvoiceLine, out outwardInvoiceLine2);
					var outwardDeclaration = (TDeclaration)outwardInvoiceLine.Declaration;
					outwardDeclaration.MessageInitiator = messageInitiator;
					outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					outwardDeclaration.DoMerge();
					SetupOutwardOriginalClearState(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2);
					Factory.Save();
					outwardJob.PublishShipmentForWHSOutward(false);
					outwardJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					UpdateQuantity(outwardInvoiceLine, 70m);
					UpdateQuantity(outwardInvoiceLine2, 120m);
					outwardDeclaration.DoMerge();
					Factory.Save();

					var withdrawalMessage = SendOutwardWithdrawalMessageViaMenu(outwardJob, outwardDeclaration);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 150m);
					AssertAdditionalForOutwardBeforeWithdrawalResponse(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, withdrawalMessage, bondedEntryKey, bondedEntryKey2);

					var responseMessage = CreateOutwardWithdrawalResponseMessage(withdrawalMessage, true);
					Factory.Save();
					ProcessResponseAndAssertOutwardOnWithdrawalClear(outwardJob, outwardDeclaration, outwardInvoiceLine, outwardInvoiceLine2, withdrawalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					outwardJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(outwardJob.GetType(), outwardJob.PK);
					AssertEquals("outwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, outwardJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 300m);
				}
			}
			else
			{
				Assert("Outward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertOutwardOnWithdrawalClear(IWarehouseIntegrationSupporter outwardJob, TDeclaration outwardDeclaration, TInvoiceLine outwardInvoiceLine, TInvoiceLine outwardInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage);

		#endregion

		#region Change of Ownership Inward Only Testing

		public void TestChangeOfOwnershipInwardOnlyFieldsValidation()
		{
			// Setup Change Of Ownership Job
			// Remove owner product
			// Assert that bonded warehousing cannot be done

			if (ShouldTestChangeOfOwnershipInwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, false);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), true);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSInward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var newOwnerProductInfo = ((IChangeOfOwnershipLineDetails)changeOfOwnershipInvoiceLine).NewOwnerProductCodeInfo;
					newOwnerProductInfo.Value = ZString.Empty;
					changeOfOwnershipInvoiceLine.JI_PartNo = ZString.Empty;
					Factory.Save();
					SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingChangeOfOwnershipRequiresAnOwnerProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					changeOfOwnershipJob.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					Factory.Save();
					messageInitiator.InvalidOperationText = null;
					SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingChangeOfOwnershipRequiresAnOwnerProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					newOwnerProductInfo.Value = helper.OwnerPart.OP_PartNum;
					changeOfOwnershipInvoiceLine.JI_PartNo = helper.Part.OP_PartNum;
					changeOfOwnershipJob.WarehouseTransactionStatus = ZString.Empty;
					Factory.Save();
					AssertAdditionalChangeOfOwnershipInwardOnlyFieldsValidation(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
				}
			}
			else
			{
				Assert("Change of Ownership Inward Only is not tested", true);
			}
		}

		protected virtual void AssertAdditionalChangeOfOwnershipInwardOnlyFieldsValidation(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2)
		{
		}

		public void TestChangeOfOwnershipInwardOnlyOnOriginalError()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Create Original Message
			// Create Error Orginal Message
			// Check that Inward inventory not affected and no new inventory created for Owner

			if (ShouldTestChangeOfOwnershipInwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, false);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), true);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();
					var originalMessage = SendChangeOfOwnershipOriginalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					AssertNotNull("Has DEX for inward original", changeOfOwnershipJob.Logs.MostRecentLogByEventTime(Events.DataExport));
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					AssertAdditionalForChangeOfOwnershipBeforeOriginalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);
					var holdNotes = changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals(1, holdNotes.Length);
					var holdNote = holdNotes[0];
					AssertEquals("holdNote.ReadOnly", true, holdNote.ReadOnly);
					AssertNotNull(changeOfOwnershipJob.GetLastHoldUniversalShipmentFromNote());

					var responseMessage = CreateChangeOfOwnershipOriginalResponseMessage(originalMessage, false);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnOriginalError(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", ZString.Empty, changeOfOwnershipJob.WarehouseTransactionStatus);
					changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					AssertEquals("Hold notes should have been deleted", 0, inwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
				}
			}
			else
			{
				Assert("Change of Ownership Inward Only is not tested", true);
			}
		}

		public void TestChangeOfOwnershipInwardOnlyOnOriginalClear()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Create Original Message
			// Create Error Orginal Message
			// Check that Inward inventory not affected and new inventory created for Owner

			if (ShouldTestChangeOfOwnershipInwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					var importer = changeOfOwnershipDeclaration.Importer;
					SetWarehouseAutomation(importer, false);
					var owner = GetNewOwner(changeOfOwnershipJob);
					SetWarehouseAutomation(owner, true);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();
					var originalMessage = SendChangeOfOwnershipOriginalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					AssertNotNull("Has DEX for inward original", changeOfOwnershipJob.Logs.MostRecentLogByEventTime(Events.DataExport));
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					AssertAdditionalForChangeOfOwnershipBeforeOriginalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);
					var holdNotes = changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals(1, holdNotes.Length);
					var holdNote = holdNotes[0];
					AssertEquals("holdNote.ReadOnly", true, holdNote.ReadOnly);
					AssertNotNull(changeOfOwnershipJob.GetLastHoldUniversalShipmentFromNote());

					var responseMessage = CreateChangeOfOwnershipOriginalResponseMessage(originalMessage, true);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnOriginalClear(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m, importer);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m, importer);
					AssertEquals("Hold notes should have been deleted", 0, inwardJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
				}
			}
			else
			{
				Assert("Change of Ownership Inward Only is not tested", true);
			}
		}

		public void TestChangeOfOwnershipInwardOnlyOnAmendmentError()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Update Bonded Whs
			// Change Quantity
			// Create Amendment Message
			// Create Error Amendment Message
			// Check that Inward inventory not affected and that same inventory amount exists for Owner

			if (ShouldTestChangeOfOwnershipInwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, false);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), true);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSInward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();

					var dataExortLogs = changeOfOwnershipJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					var amendmentMessage = SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, changeOfOwnershipJob.WarehouseTransactionStatus);
					var dataExortLogsAfter = changeOfOwnershipJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					AssertEquals("One new DEX Log", dataExortLogs.Length + 1, dataExortLogsAfter.Length);
					var dataExportLog = dataExortLogsAfter[0];
					AssertCollectionNotContains(dataExportLog, dataExortLogs);
					AssertNotNull("Should be a HOLD xml", dataExportLog.RelatedEDIMessage.DataContext.RecipientRoleCollection.First(x => x.Code.HasValue && x.Code.Value == RecipientRoleType.BWI && x.ServiceCode.HasValue && x.ServiceCode.Value == ServiceCodeType.HLD));
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					AssertAdditionalForChangeOfOwnershipBeforeAmendmentResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipAmendmentResponseMessage(amendmentMessage, false);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 230m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnAmendmentError(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", amendmentMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
				}
			}
			else
			{
				Assert("Change of Ownership Inward Only is not tested", true);
			}
		}

		public void TestChangeOfOwnershipInwardOnlyOnAmendmentClear()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Update Bonded Whs
			// Change Quantity
			// Create Amendment Message
			// Create Error Amendment Message
			// Check that Inward inventory not affected and that Owner's inventory has been updated correctly

			if (ShouldTestChangeOfOwnershipInwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					var importer = changeOfOwnershipDeclaration.Importer;
					SetWarehouseAutomation(importer, false);
					var owner = GetNewOwner(changeOfOwnershipJob);
					SetWarehouseAutomation(owner, true);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSInward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();

					var dataExortLogs = changeOfOwnershipJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					var amendmentMessage = SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, changeOfOwnershipJob.WarehouseTransactionStatus);
					var dataExortLogsAfter = changeOfOwnershipJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					AssertEquals("One new DEX Log", dataExortLogs.Length + 1, dataExortLogsAfter.Length);
					var dataExportLog = dataExortLogsAfter[0];
					AssertCollectionNotContains(dataExportLog, dataExortLogs);
					AssertNotNull("Should be a HOLD xml", dataExportLog.RelatedEDIMessage.DataContext.RecipientRoleCollection.First(x => x.Code.HasValue && x.Code.Value == RecipientRoleType.BWI && x.ServiceCode.HasValue && x.ServiceCode.Value == ServiceCodeType.HLD));
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					AssertAdditionalForChangeOfOwnershipBeforeAmendmentResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipAmendmentResponseMessage(amendmentMessage, true);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 230m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnAmendmentClear(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", amendmentMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 70m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 120m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m, importer);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m, importer);
				}
			}
			else
			{
				Assert("Change of Ownership Inward Only is not tested", true);
			}
		}

		public void TestChangeOfOwnershipInwardOnlyOnWithdrawalError()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change Of Ownership Job
			// Update Bonded Whs
			// Create Withdrawal Message
			// Create Error Withdrawal Message
			// Check that Inward inventory not affected and that Owner's inventory is the same as before messaging

			if (ShouldTestChangeOfOwnershipInwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, false);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), true);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSInward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();

					var dataExortLogs = changeOfOwnershipJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					var withdrawalMessage = SendChangeOfOwnershipWithdrawalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, changeOfOwnershipJob.WarehouseTransactionStatus);
					var dataExortLogsAfter = changeOfOwnershipJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					AssertEquals("One new DEX Log", dataExortLogs.Length + 1, dataExortLogsAfter.Length);
					var dataExportLog = dataExortLogsAfter[0];
					AssertCollectionNotContains(dataExportLog, dataExortLogs);
					AssertEquals("dataExportLog.RelatedEDIMessage.Message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, dataExportLog.RelatedEDIMessage.Message.EM_MessageSubType);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					AssertAdditionalForChangeOfOwnershipBeforeWithdrawalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipWithdrawalResponseMessage(withdrawalMessage, false);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 230m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalError(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
				}
			}
			else
			{
				Assert("Change of Ownership Inward Only is not tested", true);
			}
		}

		public void TestChangeOfOwnershipInwardOnlyOnWithdrawalClear()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Update Bonded Whs
			// Create Withdrawal Message
			// Create Clear Withdrawal Message
			// Check that Inward inventory not affected and that Owner's inventory is cancelled

			if (ShouldTestChangeOfOwnershipInwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					var importer = changeOfOwnershipDeclaration.Importer;
					SetWarehouseAutomation(importer, false);
					var owner = GetNewOwner(changeOfOwnershipJob);
					SetWarehouseAutomation(owner, true);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSInward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();

					var dataExortLogs = changeOfOwnershipJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					var withdrawalMessage = SendChangeOfOwnershipWithdrawalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, changeOfOwnershipJob.WarehouseTransactionStatus);
					var dataExortLogsAfter = changeOfOwnershipJob.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode)).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray();
					AssertEquals("One new DEX Log", dataExortLogs.Length + 1, dataExortLogsAfter.Length);
					var dataExportLog = dataExortLogsAfter[0];
					AssertCollectionNotContains(dataExportLog, dataExortLogs);
					AssertEquals("dataExportLog.RelatedEDIMessage.Message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, dataExportLog.RelatedEDIMessage.Message.EM_MessageSubType);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
					AssertAdditionalForChangeOfOwnershipBeforeWithdrawalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipWithdrawalResponseMessage(withdrawalMessage, true);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 230m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalClear(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceled, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m, importer);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m, importer);
				}
			}
			else
			{
				Assert("Change of Ownership Inward Only is not tested", true);
			}
		}

		#endregion

		#region Change of Ownership Outward Only Testing

		public void TestChangeOfOwnershipOutwardOnlyFieldsValidation()
		{
			// Setup Change Of Ownership Job
			// Remove owner's part
			// Assert that bonded warehousing should not be affected
			// Remove importer's part
			// Assert that bonded warehousing cannot be done

			if (ShouldTestChangeOfOwnershipOutwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, true);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSOutward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJobJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					changeOfOwnershipDeclaration.DoMerge();
					changeOfOwnershipInvoiceLine.JI_PartNo = ZString.Empty;
					var newOwnerProductCodeInfo = ((IChangeOfOwnershipLineDetails)changeOfOwnershipInvoiceLine).NewOwnerProductCodeInfo;
					newOwnerProductCodeInfo.Value = ZString.Empty;
					Factory.Save();
					SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingChangeOfOwnershipRequiresAnOwnerProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);

					changeOfOwnershipJob.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					Factory.Save();
					messageInitiator.InvalidOperationText = null;
					SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingChangeOfOwnershipRequiresAnOwnerProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);

					changeOfOwnershipInvoiceLine.JI_PartNo = helper.Part.OP_PartNum;
					newOwnerProductCodeInfo.Value = helper.OwnerPart.OP_PartNum;
					changeOfOwnershipJob.WarehouseTransactionStatus = ZString.Empty;
					Factory.Save();
					AssertAdditionalChangeOfOwnershipOutwardOnlyFieldsValidation(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
				}
			}
			else
			{
				Assert("Change of Ownership Outward Only is not tested", true);
			}
		}

		protected virtual void AssertAdditionalChangeOfOwnershipOutwardOnlyFieldsValidation(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2)
		{
		}

		public void TestChangeOfOwnershipOutwardOnlyOnOriginalError()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Create Original Message
			// Create Error Orginal Message
			// Check that no outward was created for importer and that no inward was created for owner

			if (ShouldTestChangeOfOwnershipOutwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, true);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();
					var originalMessage = SendChangeOfOwnershipOriginalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					AssertAdditionalForChangeOfOwnershipBeforeOriginalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipOriginalResponseMessage(originalMessage, false);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnOriginalError(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, changeOfOwnershipJob.WarehouseTransactionStatus);
					changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
				}
			}
			else
			{
				Assert("Change of Ownership Outward Only is not tested", true);
			}
		}

		public void TestChangeOfOwnershipOutwardOnlyOnOriginalClear()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Create Original Message
			// Create Clear Orginal Message
			// Check that outward was created for importer
			// Check that no inward was created for owner

			if (ShouldTestChangeOfOwnershipOutwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					var importer = changeOfOwnershipDeclaration.Importer;
					SetWarehouseAutomation(importer, true);
					var owner = GetNewOwner(changeOfOwnershipJob);
					SetWarehouseAutomation(owner, false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();
					var originalMessage = SendChangeOfOwnershipOriginalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					AssertAdditionalForChangeOfOwnershipBeforeOriginalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipOriginalResponseMessage(originalMessage, true);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnOriginalClear(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m, importer);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m, importer);
				}
			}
			else
			{
				Assert("Change of Ownership Outward Only is not tested", true);
			}
		}

		public void TestChangeOfOwnershipOutwardOnlyOnAmendmentError()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Update Bonded Whs to create outward for importer and inward for owner
			// Change Quantity
			// Create Amendment Message
			// Check that owner's inward inventory hasn't been affected
			// Create Error Amendment Message
			// Check that same inventory amount exists for Importer
			// Check that owner's inward inventory hasn't been affected

			if (ShouldTestChangeOfOwnershipOutwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, true);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					changeOfOwnershipJob.PublishShipmentForWHSInward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);

					changeOfOwnershipJob.PublishShipmentForWHSOutward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();
					var holdNotes = changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("No hold notes", 0, holdNotes.Length);

					var amendmentMessage = SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 30m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					holdNotes = changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("Hold note has been created", 1, holdNotes.Length);
					AssertAdditionalForChangeOfOwnershipBeforeAmendmentResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipAmendmentResponseMessage(amendmentMessage, false);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 80m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 90m);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnAmendmentError(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", amendmentMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					AssertEquals("Hold notes should have been deleted", 0, changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
				}
			}
			else
			{
				Assert("Change of Ownership Outward Only is not tested", true);
			}
		}

		public void TestChangeOfOwnershipOutwardOnlyOnAmendmentClear()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Update Bonded Whs to create outward for importer and inward for owner
			// Change Quantity
			// Create Amendment Message
			// Check that owner's inward inventory hasn't been affected
			// Create Clear Amendment Message
			// Check that inventory amount was updated for Importer
			// Check that owner's inward inventory hasn't been affected

			if (ShouldTestChangeOfOwnershipOutwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					var importer = changeOfOwnershipDeclaration.Importer;
					SetWarehouseAutomation(importer, true);
					var owner = GetNewOwner(changeOfOwnershipJob);
					SetWarehouseAutomation(owner, false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					changeOfOwnershipJob.PublishShipmentForWHSInward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);

					changeOfOwnershipJob.PublishShipmentForWHSOutward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();
					var holdNotes = changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("No hold notes", 0, holdNotes.Length);

					var amendmentMessage = SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 30m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					holdNotes = changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("Hold note has been created", 1, holdNotes.Length);
					AssertAdditionalForChangeOfOwnershipBeforeAmendmentResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipAmendmentResponseMessage(amendmentMessage, true);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 80m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 90m);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnAmendmentClear(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", amendmentMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 30m, importer);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 180m, importer);
					AssertEquals("Hold notes should have been deleted", 0, changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
				}
			}
			else
			{
				Assert("Change of Ownership Outward Only is not tested", true);
			}
		}

		public void TestChangeOfOwnershipOutwardOnlyOnWithdrawalError()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change Of Ownership Job
			// Update Bonded Whs to create outward for importer and inward for owner
			// Change Quantity
			// Create Withdrawal Message
			// Check that owner's inward inventory hasn't been affected
			// Create Error Withdrawal Message
			// Check that same inventory amount exists for Importer
			// Check that owner's inward inventory hasn't been affected

			if (ShouldTestChangeOfOwnershipOutwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, true);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					changeOfOwnershipJob.PublishShipmentForWHSInward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);

					changeOfOwnershipJob.PublishShipmentForWHSOutward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();

					var withdrawalMessage = SendChangeOfOwnershipWithdrawalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					AssertAdditionalForChangeOfOwnershipBeforeWithdrawalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipWithdrawalResponseMessage(withdrawalMessage, false);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalError(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
				}
			}
			else
			{
				Assert("Change of Ownership Outward Only is not tested", true);
			}
		}

		public void TestChangeOfOwnershipOutwardOnlyOnWithdrawalClear()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Update Bonded Whs to create outward for importer and inward for owner
			// Change Quantity
			// Create Withdrawal Message
			// Check that owner's inward inventory hasn't been affected
			// Create Clear Withdrawal Message
			// Check that Importer's Outward was cancelled
			// Check that owner's inward inventory hasn't been affected

			if (ShouldTestChangeOfOwnershipOutwardOnly)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, false);
					var importer = changeOfOwnershipDeclaration.Importer;
					SetWarehouseAutomation(importer, true);
					var owner = GetNewOwner(changeOfOwnershipJob);
					SetWarehouseAutomation(owner, false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					changeOfOwnershipJob.PublishShipmentForWHSInward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);

					changeOfOwnershipJob.PublishShipmentForWHSOutward(false);
					changeOfOwnershipJob.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();

					var withdrawalMessage = SendChangeOfOwnershipWithdrawalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					AssertAdditionalForChangeOfOwnershipBeforeWithdrawalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipWithdrawalResponseMessage(withdrawalMessage, true);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalClear(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m, importer);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m, importer);
				}
			}
			else
			{
				Assert("Change of Ownership Outward Only is not tested", true);
			}
		}

		#endregion

		#region Change of Ownership Testing

		public void TestChangeOfOwnershipFieldsValidation()
		{
			// Setup Change Of Ownership Job
			// Remove owner
			// Assert that bonded warehousing cannot be done

			if (ShouldTestChangeOfOwnership)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, true);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, false);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSChangeOfOwnership(false);
					AssertEquals("changeOfOwnershipDeclaration.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					changeOfOwnershipDeclaration.DoMerge();
					changeOfOwnershipInvoiceLine.JI_PartNo = ZString.Empty;
					Factory.Save();
					messageInitiator.InvalidOperationText = null;
					SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingChangeOfOwnershipRequiresAnOwnerProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);

					changeOfOwnershipInvoiceLine.JI_PartNo = helper.Part.OP_PartNum;
					((IChangeOfOwnershipLineDetails)changeOfOwnershipInvoiceLine).NewOwnerProductCodeInfo.Value = ZString.Empty;
					Factory.Save();
					messageInitiator.InvalidOperationText = null;
					SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingChangeOfOwnershipRequiresAnOwnerProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);

					changeOfOwnershipJob.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					Factory.Save();
					messageInitiator.InvalidOperationText = null;
					SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingChangeOfOwnershipRequiresAnOwnerProduct("Inventory Management"), messageInitiator.InvalidOperationText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);

					((IChangeOfOwnershipLineDetails)changeOfOwnershipInvoiceLine).NewOwnerProductCodeInfo.Value = helper.OwnerPart.OP_PartNum;
					changeOfOwnershipJob.WarehouseTransactionStatus = ZString.Empty;
					Factory.Save();
					AssertAdditionalChangeOfOwnershipFieldsValidation(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
				}
			}
			else
			{
				Assert("Change of Ownership Outward is not tested", true);
			}
		}

		protected virtual void AssertAdditionalChangeOfOwnershipFieldsValidation(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2)
		{
		}

		public void TestChangeOfOwnershipOnOriginalError()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Create Original Message
			// Check that outward was created for Importer
			// Check that no inward was created for Owner
			// Create Error Orginal Message
			// Check that outward for Importer was cancelled
			// Check that no inward was created for Owner

			if (ShouldTestChangeOfOwnership)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, true);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, false);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();
					var originalMessage = SendChangeOfOwnershipOriginalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					AssertAdditionalForChangeOfOwnershipBeforeOriginalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipOriginalResponseMessage(originalMessage, false);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnOriginalError(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled, changeOfOwnershipJob.WarehouseTransactionStatus);
					changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);
				}
			}
			else
			{
				Assert("Change of Ownership Outward is not tested", true);
			}
		}

		protected virtual void AssertAdditionalForChangeOfOwnershipBeforeOriginalResponse(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage originalMessage, ZString inwardBondedEntryKey, ZString inwardBondedEntryKey2, ZString changeOfOwnershipBondedEntryKey, ZString changeOfOwnershipBondedEntryKey2)
		{
		}

		protected abstract void ProcessResponseAndAssertChangeOfOwnershipOnOriginalError(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage);
		protected abstract EDIMessage CreateChangeOfOwnershipOriginalResponseMessage(EDIMessage originalMessage, bool isSuccess);
		protected abstract EDIMessage SendChangeOfOwnershipOriginalMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration);

		public void TestChangeOfOwnershipOnOriginalClear()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Create Original Message
			// Check that outward was created for Importer
			// Check that no inward was created for Owner
			// Create Clear Orginal Message
			// Check that outward was created for importer
			// Check that inward was created for owner

			if (ShouldTestChangeOfOwnership)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, true);
					var importer = changeOfOwnershipDeclaration.Importer;
					SetWarehouseAutomation(importer, false);
					var owner = GetNewOwner(changeOfOwnershipJob);
					SetWarehouseAutomation(owner, false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();
					var originalMessage = SendChangeOfOwnershipOriginalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					AssertAdditionalForChangeOfOwnershipBeforeOriginalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipOriginalResponseMessage(originalMessage, true);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnOriginalClear(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, originalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", originalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m, importer);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m, importer);
				}
			}
			else
			{
				Assert("Change of Ownership Outward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertChangeOfOwnershipOnOriginalClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage originalMessage, EDIMessage responseMessage);

		public void TestChangeOfOwnershipOnAmendmentError()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Update Bonded Whs
			// Check that outward was created for Importer
			// Check that inward was created for Owner
			// Change Quantity
			// Create Amendment Message
			// Check that Importer's outward has been amended and is on hold
			// Check that Onwer's inward has been amended and is on hold
			// Create Error Amendment Message
			// Check that Importer's outward has been rolled back to pre-amendment
			// Check that Onwer's inward has been rolled back to pre-amendment

			if (ShouldTestChangeOfOwnership)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, true);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, false);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSChangeOfOwnership(false);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();
					var holdNotes = changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("No hold notes", 0, holdNotes.Length);

					var amendmentMessage = SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 30m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					holdNotes = changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("Hold note has been created", 1, holdNotes.Length);
					AssertAdditionalForChangeOfOwnershipBeforeAmendmentResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipAmendmentResponseMessage(amendmentMessage, false);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnAmendmentError(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", amendmentMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdated, changeOfOwnershipJob.WarehouseTransactionStatus);
					changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					AssertEquals("Hold notes should have been deleted", 0, changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
				}
			}
			else
			{
				Assert("Change of Ownership Outward is not tested", true);
			}
		}

		protected virtual void AssertAdditionalForChangeOfOwnershipBeforeAmendmentResponse(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage amendmentMessage, ZString inwardBondedEntryKey, ZString inwardBondedEntryKey2, ZString changeOfOwnershipBondedEntryKey, ZString changeOfOwnershipBondedEntryKey2)
		{
		}

		protected abstract void ProcessResponseAndAssertChangeOfOwnershipOnAmendmentError(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage);
		protected abstract EDIMessage CreateChangeOfOwnershipAmendmentResponseMessage(EDIMessage amendmentMessage, bool isSuccess);
		protected abstract EDIMessage SendChangeOfOwnershipAmendmentMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration);
		protected abstract void SetupChangeOfOwnershipOriginalClearState(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2);

		public void TestChangeOfOwnershipOnAmendmentClear()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Update Bonded Whs
			// Check that outward was created for Importer
			// Check that inward was created for Owner
			// Change Quantity
			// Create Amendment Message
			// Check that Importer's outward has been amended and is on hold
			// Check that Onwer's inward has been amended and is on hold
			// Create Error Amendment Message
			// Check that Importer's outward has been updated
			// Check that Onwer's inward has been updated

			if (ShouldTestChangeOfOwnership)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, true);
					var importer = changeOfOwnershipDeclaration.Importer;
					SetWarehouseAutomation(importer, false);
					var owner = GetNewOwner(changeOfOwnershipJob);
					SetWarehouseAutomation(owner, false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSChangeOfOwnership(false);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();
					var holdNotes = changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("No hold notes", 0, holdNotes.Length);

					var amendmentMessage = SendChangeOfOwnershipAmendmentMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 30m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					holdNotes = changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription);
					AssertEquals("Hold note has been created", 1, holdNotes.Length);
					AssertAdditionalForChangeOfOwnershipBeforeAmendmentResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipAmendmentResponseMessage(amendmentMessage, true);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnAmendmentClear(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, amendmentMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", amendmentMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdated, changeOfOwnershipJob.WarehouseTransactionStatus);
					changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 70m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 120m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 30m, importer);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 180m, importer);
					AssertEquals("Hold notes should have been deleted", 0, changeOfOwnershipJob.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Length);
				}
			}
			else
			{
				Assert("Change of Ownership Outward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertChangeOfOwnershipOnAmendmentClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage amendmentMessage, EDIMessage responseMessage);

		public void TestChangeOfOwnershipOnWithdrawalError()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Update Bonded Whs
			// Check that outward was created for Importer
			// Check that inward was created for Owner
			// Change Quantity
			// Create Withdrawal Message
			// Check that Importer's outward is on hold
			// Check that Onwer's inward is on hold
			// Create Error Amendment Message
			// Check that Importer's outward hold is removed
			// Check that Onwer's inward hold is removed

			if (ShouldTestChangeOfOwnership)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, true);
					SetWarehouseAutomation(changeOfOwnershipDeclaration.Importer, false);
					SetWarehouseAutomation(GetNewOwner(changeOfOwnershipJob), false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSChangeOfOwnership(false);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();

					var withdrawalMessage = SendChangeOfOwnershipWithdrawalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					AssertAdditionalForChangeOfOwnershipBeforeWithdrawalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipWithdrawalResponseMessage(withdrawalMessage, false);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalError(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdated, changeOfOwnershipJob.WarehouseTransactionStatus);
					changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
				}
			}
			else
			{
				Assert("Change of Ownership Outward is not tested", true);
			}
		}

		protected virtual void AssertAdditionalForChangeOfOwnershipBeforeWithdrawalResponse(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage withdrawalMessage, ZString inwardBondedEntryKey, ZString inwardBondedEntryKey2, ZString changeOfOwnershipBondedEntryKey, ZString changeOfOwnershipBondedEntryKey2)
		{
		}

		protected abstract void ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalError(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage);
		protected abstract EDIMessage CreateChangeOfOwnershipWithdrawalResponseMessage(EDIMessage withdrawalMessage, bool isSuccess);
		protected abstract EDIMessage SendChangeOfOwnershipWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration);

		public void TestChangeOfOwnershipOnWithdrawalClear()
		{
			// Setup Inward Job
			// Update Bonded Whs

			// Setup Change of Ownership Job
			// Update Bonded Whs
			// Check that outward was created for Importer
			// Check that inward was created for Owner
			// Change Quantity
			// Create Withdrawal Message
			// Check that Importer's outward is on hold
			// Check that Onwer's inward is on hold
			// Create Error Amendment Message
			// Check that Importer's outward hold is removed
			// Check that Onwer's inward hold is removed

			if (ShouldTestChangeOfOwnership)
			{
				var helper = new WhsDataTestHelper(Factory);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var inwardJob = GetNewInwardJob("B00001230", 100m, 300m);
					TInvoiceLine inwardInvoiceLine;
					TInvoiceLine inwardInvoiceLine2;
					GetInvoiceLines(inwardJob, out inwardInvoiceLine, out inwardInvoiceLine2);
					var messageInitiator = GetNewMessageInitiator();
					var inwardDeclaration = (TDeclaration)inwardInvoiceLine.Declaration;
					inwardDeclaration.MessageInitiator = messageInitiator;
					inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					inwardDeclaration.DoMerge();
					SetupInwardOriginalClearState(inwardJob, inwardDeclaration, inwardInvoiceLine, inwardInvoiceLine2);
					Factory.Save();
					inwardJob.PublishShipmentForWHSInward(false);
					inwardJob.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
					AssertEquals("inwardJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardJob.WarehouseTransactionStatus);
					var inwardEntryKey = GetEntryKey(inwardJob);
					var inwardEntryLineNo = inwardInvoiceLine.CusEntryLine.CL_LineNumber;
					var inwardEntryLineNo2 = inwardInvoiceLine2.CusEntryLine.CL_LineNumber;
					var inwardBondedEntryKey = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo);
					var inwardBondedEntryKey2 = GetBondedEntryKey(inwardEntryKey, inwardEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

					var changeOfOwnershipJob = GetNewChangeOfOwnershipJob("B00001240", inwardEntryKey, 60m, 150m);
					TInvoiceLine changeOfOwnershipInvoiceLine;
					TInvoiceLine changeOfOwnershipInvoiceLine2;
					GetInvoiceLines(changeOfOwnershipJob, out changeOfOwnershipInvoiceLine, out changeOfOwnershipInvoiceLine2);
					var changeOfOwnershipDeclaration = (TDeclaration)changeOfOwnershipInvoiceLine.Declaration;
					SetWarehouseAutomation(changeOfOwnershipJob.WarehouseAddress?.Header, true);
					var importer = changeOfOwnershipDeclaration.Importer;
					SetWarehouseAutomation(importer, false);
					var owner = GetNewOwner(changeOfOwnershipJob);
					SetWarehouseAutomation(owner, false);
					changeOfOwnershipDeclaration.MessageInitiator = messageInitiator;
					changeOfOwnershipDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					changeOfOwnershipDeclaration.DoMerge();
					SetupChangeOfOwnershipOriginalClearState(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2);
					Factory.Save();
					changeOfOwnershipJob.PublishShipmentForWHSChangeOfOwnership(false);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated, changeOfOwnershipJob.WarehouseTransactionStatus);
					var changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					var changeOfOwnershipEntryLineNo1 = changeOfOwnershipInvoiceLine.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipEntryLineNo2 = changeOfOwnershipInvoiceLine2.CusEntryLine.CL_LineNumber;
					var changeOfOwnershipBondedEntryKey = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo1);
					var changeOfOwnershipBondedEntryKey2 = GetBondedEntryKey(changeOfOwnershipEntryKey, changeOfOwnershipEntryLineNo2);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 60m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 150m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					UpdateQuantity(changeOfOwnershipInvoiceLine, 70m);
					UpdateQuantity(changeOfOwnershipInvoiceLine2, 120m);
					changeOfOwnershipDeclaration.DoMerge();
					Factory.Save();

					var withdrawalMessage = SendChangeOfOwnershipWithdrawalMessageViaMenu(changeOfOwnershipJob, changeOfOwnershipDeclaration);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding, changeOfOwnershipJob.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 40m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 150m);
					AssertAdditionalForChangeOfOwnershipBeforeWithdrawalResponse(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, inwardBondedEntryKey, inwardBondedEntryKey2, changeOfOwnershipBondedEntryKey, changeOfOwnershipBondedEntryKey2);

					var responseMessage = CreateChangeOfOwnershipWithdrawalResponseMessage(withdrawalMessage, true);
					Factory.Save();
					ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalClear(changeOfOwnershipJob, changeOfOwnershipDeclaration, changeOfOwnershipInvoiceLine, changeOfOwnershipInvoiceLine2, withdrawalMessage, responseMessage);
					responseMessage = new BusinessObjectFactory().Load<EDIMessage>(responseMessage.PK);
					AssertEquals("responseMessage.EM_LinkUniqueID", withdrawalMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
					changeOfOwnershipJob = (IWarehouseIntegrationSupporter)responseMessage.Factory.Load(changeOfOwnershipJob.GetType(), changeOfOwnershipJob.PK);
					AssertEquals("changeOfOwnershipJob.WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled, changeOfOwnershipJob.WarehouseTransactionStatus);
					changeOfOwnershipEntryKey = GetEntryKey(changeOfOwnershipJob);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey, 0m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(changeOfOwnershipBondedEntryKey2, 0m, owner);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey, 100m, importer);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m, importer);
				}
			}
			else
			{
				Assert("Change of Ownership Outward is not tested", true);
			}
		}

		protected abstract void ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, TDeclaration changeOfOwnershipDeclaration, TInvoiceLine changeOfOwnershipInvoiceLine, TInvoiceLine changeOfOwnershipInvoiceLine2, EDIMessage withdrawalMessage, EDIMessage responseMessage);

		#endregion

		#endregion

		#region Implementation

		protected ZString GetBondedEntryKey(ZString entryKey, ZShort entryLineNo)
		{
			return entryKey + "-" + entryLineNo;
		}

		protected virtual void SetupInvoiceLine(BaseJobComInvoiceLine invoiceLine, TPart part, ZDecimal quantity)
		{
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_BondedWhsQuantity = quantity;
		}

		protected virtual bool ShouldTestInward => true;
		protected virtual bool ShouldTestOutward => true;
		protected virtual bool ShouldTestChangeOfOwnership => false;
		protected bool ShouldTestChangeOfOwnershipInwardOnly => ShouldTestChangeOfOwnership && ShouldTestInward;
		protected bool ShouldTestChangeOfOwnershipOutwardOnly => ShouldTestChangeOfOwnership && ShouldTestOutward;

		protected GlbGroup PostMasterGroup
		{
			get
			{
				if (postMasterGroup == null)
				{
					postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
					postMasterGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";
					var groupNotification = new AutoBillingGroupNotification();
					groupNotification.SendGroupPK = postMasterGroup.PK;
					Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
				}
				return postMasterGroup;
			}
		}
		GlbGroup postMasterGroup;

		protected GlbStaff CurrentStaff
		{
			get
			{
				if (currentStaff == null)
				{
					currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
					currentStaff.GS_EmailAddress = "staff@cargowise.com";
				}
				return currentStaff;
			}
		}
		GlbStaff currentStaff;

		protected virtual SendsMessagesToCustomsShutterUpperer GetNewMessageInitiator()
		{
			return new SendsMessagesToCustomsShutterUpperer(false);
		}

		protected abstract void GetInvoiceLines(IWarehouseIntegrationSupporter job, out TInvoiceLine invoiceLine, out TInvoiceLine invoiceLine2);
		protected abstract IWarehouseIntegrationSupporter GetNewInwardJob(ZString jobReference, ZDecimal quantity1, ZDecimal quantity2);
		protected abstract IWarehouseIntegrationSupporter GetNewOutwardJob(ZString jobReference, ZString inwardEntryKey, ZDecimal quantity1, ZDecimal quantity2);
		protected abstract IWarehouseIntegrationSupporter GetNewChangeOfOwnershipJob(ZString jobReference, ZString inwardEntryKey, ZDecimal quantity1, ZDecimal quantity2);
		protected abstract OrgHeader GetNewOwner(IWarehouseIntegrationSupporter changeOfOwnershipJob);

		protected void SetWarehouseAutomation(OrgHeader org, bool isEnabled)
		{
			if (org != null)
			{
				org.CompanyData.OB_IMUsedBondedWhs = isEnabled;
			}
		}

		protected virtual ZString GetEntryKey(IWarehouseIntegrationSupporter job)
		{
			return job.EntryNumber;
		}

		protected WhsDataTestHelper<TDeclaration, TPart, TClassification, TPivot> helper { get; private set; }

		protected virtual WhsDataTestHelper<TDeclaration, TPart, TClassification, TPivot> CreateNewHelper()
		{
			return new WhsDataTestHelper<TDeclaration, TPart, TClassification, TPivot>(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var post = PostMasterGroup;
			helper = CreateNewHelper();
		}

		#endregion
	}
}
