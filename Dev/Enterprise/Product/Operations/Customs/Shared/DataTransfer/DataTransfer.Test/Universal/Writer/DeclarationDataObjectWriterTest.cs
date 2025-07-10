using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static NUnit.Framework.XmlAssertions;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		[TestDate(2024, 12, 25)]
		public void TestExportDeclarationFromShipment()
		{
			var factory = new BusinessObjectFactory();
			using (factory.AddDisposableService())
			{
				var filter = factory.New<EDIMessageContentFilter>();
				filter.ECF_Name = "ABC";
				filter.UniversalShipment.AdditionalConfiguration.PrimaryDataSource = EDIMessageContentPrimaryDataSource.Codes.Brokerage;

				var purpose = factory.New<EDIMessagePurpose>();
				purpose.EMP_Code = "FFF";
				purpose.EMP_Description = "FFF";
				purpose.EMP_ECF_Filter = filter.PK;

				var shipment = factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				var trigger = ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Enterprise.Core.Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Broker;
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_MessagePurpose = purpose.EMP_Code;

				var declaration = factory.New<BaseJobDeclaration>();
				declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
				factory.Save();

				var shipmentDataContextManager = (IShipmentDataContextManager)ObjectFactory.Get("ForwardingShipmentDataContextManager");
				ProcessUniversalXml(shipment, action, (outboundSessionTracker) => shipmentDataContextManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, shipment) { PurposeCode = action.PQ_MessagePurpose })));
				ProcessUniversalXml(declaration, action, (outboundSessionTracker) => new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration) { PurposeCode = action.PQ_MessagePurpose })));

				var messages = factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(2, messages.Length);
				AssertEquals(messages[0].EM_MessageText, messages[1].EM_MessageText);
			}
		}

		[TestDate(2024, 12, 25)]
		public void TestExportDeclarationFromShipmentWithNoDeclaration()
		{
			var factory = new BusinessObjectFactory();
			using (factory.AddDisposableService())
			{
				var filter = factory.New<EDIMessageContentFilter>();
				filter.ECF_Name = "ABC";
				filter.UniversalShipment.AdditionalConfiguration.PrimaryDataSource = EDIMessageContentPrimaryDataSource.Codes.Brokerage;

				var purpose = factory.New<EDIMessagePurpose>();
				purpose.EMP_Code = "FFF";
				purpose.EMP_Description = "FFF";
				purpose.EMP_ECF_Filter = filter.PK;

				var shipment = factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				var trigger = ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Enterprise.Core.Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Broker;
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_MessagePurpose = purpose.EMP_Code;

				var shipmentDataContextManager = (IShipmentDataContextManager)ObjectFactory.Get("ForwardingShipmentDataContextManager");
				ProcessUniversalXml(shipment, action, (outboundSessionTracker) => shipmentDataContextManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, shipment) { PurposeCode = action.PQ_MessagePurpose })));
				var message = factory.LoadTop1<EDIMessage>(new ZQuery());
				AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertIsXml(message.EM_MessageText)
				.HavingExactlyOneDescendantNode(node => node.WithName("DataSourceCollection")
					.HavingExactlyOneChildNode(node => node.WithName("DataSource")
					.HavingExactlyOneChildNode(node => node.WithName("Type")
					.WithValue("ForwardingShipment"))));
			}
		}

		static void ProcessUniversalXml(BusinessObject bo, ProcessTaskNotification action, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter)
		{
			var logger = new TestLogger();
			var logBO = bo.GetLogs().AddNew(new EventValue(Events.Received));
			var actionWrapper = new ActionWrapper(action, bo, Lazy.Create<IStmALog>(() => logBO));

			UniversalXmlWorkflowProcessorBuilder.New(actionWrapper
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null))
				, dataWriterGetter
				, bo).Process(logger);
		}

		public void TestDeclarationExportAddInfoChildData_KeepOnlySomeData()
		{
			var declaration = Factory.New<JobDeclarationAddInfoChildSupporter>();
			var addInfoChild = (DummyBusinessObject)declaration.AddInfoChild;
			addInfoChild.Z0_Guid = declaration.PK;
			addInfoChild.Z0_Code = "T12";
			addInfoChild.Z0_Description = "HELLO WORLD";
			addInfoChild.Z0_Number = 1540;
			addInfoChild.Z0_AnotherDecimal = 3450.235m;
			addInfoChild.Z0_Short = 8963;
			addInfoChild.Z0_Date = new ZDateTime(2021, 05, 10);
			addInfoChild.Z0_Decimal = 2635m;
			declaration.AddInfo.UZ_Short = 6394;
			declaration.AddInfo.UZ_Date = new ZDateTime(2021, 04, 14, 5, 30, 50);
			declaration.AddInfo.UZ_Decimal = 1554m;
			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			CombineAssertions(() =>
			{
				AssertEquals("Code", "T12", declarationData.AddInfoCollection.GetZStringValue(DummyBizoSchema.Constants.Z0_Code.Substring(3)));
				AssertEquals("Description", "HELLO WORLD", declarationData.AddInfoCollection.GetZStringValue(DummyBizoSchema.Constants.Z0_Description.Substring(3)));
				AssertEquals("Number", 1540, declarationData.AddInfoCollection.GetZIntValue(DummyBizoSchema.Constants.Z0_Number.Substring(3)));
				AssertEquals("AnotherDecimal", 3450.235m, declarationData.AddInfoCollection.GetZDecimalValue(DummyBizoSchema.Constants.Z0_AnotherDecimal.Substring(3)));
				AssertEquals("Short should come from AddInfo", (short)6394, declarationData.AddInfoCollection.GetZShortValue(DummyBizoSchema.Constants.Z0_Short.Substring(3)));
				AssertEquals("Date should come from AddInfo", new ZDateTime(2021, 04, 14, 5, 30, 50), declarationData.AddInfoCollection.GetZDateTimeValue(DummyBizoSchema.Constants.Z0_Date.Substring(3)));
				AssertEquals("Decimal should come from AddInfo", 1554m, declarationData.AddInfoCollection.GetZDecimalValue(DummyBizoSchema.Constants.Z0_Decimal.Substring(3)));
			});
		}

		public void TestDeclarationExportAddInfoChildData()
		{
			var declaration = Factory.New<JobDeclarationAddInfoChildSupporter>();
			var addInfoChild = (DummyBusinessObject)declaration.AddInfoChild;
			addInfoChild.Z0_Guid = declaration.PK;
			addInfoChild.Z0_Code = "T12";
			addInfoChild.Z0_Description = "HELLO WORLD";
			addInfoChild.Z0_Number = 1540;
			addInfoChild.Z0_AnotherDecimal = 3450.235m;
			addInfoChild.Z0_Short = 8963;
			declaration.AddInfo.UZ_Short = 6394;
			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals("Code", "T12", declarationData.AddInfoCollection.GetZStringValue(DummyBizoSchema.Constants.Z0_Code.Substring(3)));
			AssertEquals("Description", "HELLO WORLD", declarationData.AddInfoCollection.GetZStringValue(DummyBizoSchema.Constants.Z0_Description.Substring(3)));
			AssertEquals("Number", 1540, declarationData.AddInfoCollection.GetZIntValue(DummyBizoSchema.Constants.Z0_Number.Substring(3)));
			AssertEquals("AnotherDecimal", 3450.235m, declarationData.AddInfoCollection.GetZDecimalValue(DummyBizoSchema.Constants.Z0_AnotherDecimal.Substring(3)));
			AssertEquals("Short should come from AddInfo as keeping existing data is true", (short)6394, declarationData.AddInfoCollection.GetZShortValue(DummyBizoSchema.Constants.Z0_Short.Substring(3)));
		}

		public void TestDeclarationExportsAll3NotifyParties()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			var notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			var notifyParty3 = Factory.NewWithValidTestData<OrgHeader>();

			declaration.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;
			declaration.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty2.PK;
			declaration.NotifyParty3DocumentaryAddress.OrganisationPK = notifyParty3.PK;

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			var notifyPartyDataObject = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.NotifyParty));
			AssertNotNull("Notify Party", notifyPartyDataObject.Address1);
			AssertEquals("NotifyParty", notifyPartyDataObject.AddressType);
			var notifyParty2DataObject = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.NotifyParty2));
			AssertNotNull("Notify Party 2", notifyParty2DataObject.Address1);
			AssertEquals("NotifyParty2", notifyParty2DataObject.AddressType);
			var notifyParty3DataObject = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.NotifyParty3));
			AssertNotNull("Notify Party 3", notifyParty3DataObject.Address1);
			AssertEquals("NotifyParty3", notifyParty3DataObject.AddressType);
		}

		public void TestShouldPopulateAttachedDocumentCollection()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_CustomsProfile = "123";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			Assert(!writer.ShouldPopulateAttachedDocumentCollection);
		}

		public void TestPopulateCustomsProfileIdentifier_IT()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_CustomsProfile = "123";

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var result = writer.GetDataObject(declaration);
				AssertEquals("123", result.CustomsProfileIdentifier.Value);
				AssertEquals("Node", result.CustomsProfileIdentifier.Type);
			}
		}

		public void TestPopulateCustomsProfileIdentifier_GB()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_CustomsProfile = "123";

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var result = writer.GetDataObject(declaration);
				AssertEquals("123", result.CustomsProfileIdentifier.Value);
				AssertEquals("AgentCode", result.CustomsProfileIdentifier.Type);
			}
		}

		public void TestExportDataNotCausingStackOverflow()
		{
			var decLVS = Factory.BOFactory.New<BaseJobDeclaration>();
			decLVS.JE_MessageType = "LVS";
			decLVS.JE_HouseBill = "HBLVS32132";
			decLVS.JE_MasterBill = "MBLVS02342";
			var invoiceLVS1 = decLVS.Invoices.AddNew();
			var invoiceLVS1Line = invoiceLVS1.JobComInvoiceLines.AddNew();
			var invoiceLVS2 = decLVS.Invoices.AddNew();
			var invoiceLVS2Line = invoiceLVS2.JobComInvoiceLines.AddNew();
			var decLVX = Factory.BOFactory.New<BaseJobDeclaration>();
			decLVX.JE_MessageType = "LVX";
			decLVX.JE_HouseBill = "HBLVX32132";
			decLVX.JE_MasterBill = "MBLVX02342";
			var invoiceLVX1 = decLVX.Invoices.AddNew();
			var invoiceLVX1Line = invoiceLVX1.JobComInvoiceLines.AddNew();
			invoiceLVX1.AttachToAdditionalDeclaration(decLVS);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var decMock = newFactory.BOFactory.LoadMoq<BaseJobDeclaration>(decLVS.PK);
			bool loadData = true;

			decMock.Setup(m => m.JE_MessageSubType)
				.Returns(ZString.Empty)
				.Callback(() =>
				{
					if (loadData)
					{
						loadData = false;
						newFactory.Load<CommonJobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, decLVS.PK));
						newFactory.Load<Bill>(new ZQuery(CusDecHouseBillSchema.CU_JE, decLVS.PK));
					}
				});

			var dec = decMock.Object;
			using (((IExternalFetchHintSupporter)newFactory.BOFactory).SetupCreator())
			{
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
				AssertNoExceptionThrown(() => _ = writer.GetDataObject(dec));
			}
		}

		public void TestForwardingOrdersAreExported()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var order = declaration.AttachedOrders.AddNew();
			order.JD_OrderNumber = "ORDER ME";
			order.JD_OrderNumberSplit = new ZByte(2);

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);

			AssertNotNull(declarationData);
			AssertNotNull(declarationData.RelatedShipmentCollection);
			AssertEquals(1, declarationData.RelatedShipmentCollection.Count);

			var orderData = declarationData.RelatedShipmentCollection[0];
			AssertEquals("ORDER ME", orderData.Order.OrderNumber);
			AssertEquals(new ZByte(2), orderData.Order.OrderNumberSplit);
		}

		public void TestIATALoadPortIsExported()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_IATALoadPort = "SYD";

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var declarationData = writer.GetDataObject(declaration);

				AssertEquals("SYD", declarationData.CustomsValuationPort.Code);
				AssertEquals("Sydney", declarationData.CustomsValuationPort.Description);
			}
		}

		public void TestExtraParentDetailsAreExportedInAdditionalBill()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var mb1 = declaration.Bills.AddNew();
			mb1.CU_BillNum = "MB1";
			mb1.CU_BillType = BillTypeList.Codes.MasterBill;
			mb1.CU_NoOfPacks = 10m;
			var mb1hb1 = mb1.ChildBills.AddNew();
			mb1hb1.CU_BillNum = "HB1";
			mb1hb1.CU_NoOfPacks = 10m;
			var mb1hb1sb1 = mb1hb1.ChildBills.AddNew();
			mb1hb1sb1.CU_BillNum = "SB1";
			mb1hb1sb1.CU_NoOfPacks = 10m;
			var mb2 = declaration.Bills.AddNew();
			mb2.CU_BillNum = "MB2";
			mb2.CU_BillType = BillTypeList.Codes.MasterBill;
			mb2.CU_NoOfPacks = 15m;
			var mb2hb1 = mb2.ChildBills.AddNew();
			mb2hb1.CU_BillNum = "HB1";
			mb2hb1.CU_NoOfPacks = 15m;
			var mb2hb1sb1 = mb2hb1.ChildBills.AddNew();
			mb2hb1sb1.CU_BillNum = "SB1";
			mb2hb1sb1.CU_NoOfPacks = 15m;
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertEquals(6, declarationData.AdditionalBillCollection.Count);
			var mb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "MB1" && x.BillType.Code.Value == WayBillTypeList.Codes.Master && x.NoOfPacks == 10m && x.AddInfoCollection == null);
			var mb1hb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "HB1" && x.BillType.Code.Value == WayBillTypeList.Codes.House && x.NoOfPacks == 10m && x.ParentBillNumber.Value == "MB1" && x.AddInfoCollection == null);
			var mb1hb1sb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "SB1" && x.BillType.Code.Value == WayBillTypeList.Codes.SubHouse && x.NoOfPacks == 10m && x.ParentBillNumber.Value == "HB1" && x.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber).Value == "MB1");
			var mb2Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "MB2" && x.BillType.Code.Value == WayBillTypeList.Codes.Master && x.NoOfPacks == 15m && x.AddInfoCollection == null);
			var mb2hb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "HB1" && x.BillType.Code.Value == WayBillTypeList.Codes.House && x.NoOfPacks == 15m && x.ParentBillNumber.Value == "MB2" && x.AddInfoCollection == null);
			var mb2hb1sb1Data = declarationData.AdditionalBillCollection.First(x => x.BillNumber.Value == "SB1" && x.BillType.Code.Value == WayBillTypeList.Codes.SubHouse && x.NoOfPacks == 15m && x.ParentBillNumber.Value == "HB1" && x.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber).Value == "MB2");
		}

		public void TestShipToPartyAddressInDeclarationIsExported()
		{
			var shipToParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			shipToParty.OH_IsConsignor = ZBool.True;

			var shipToPartyAddress = shipToParty.Addresses.AddNew(OrgAddressType.Office, false);
			shipToPartyAddress.OA_Address1 = "ADR";
			shipToPartyAddress.OA_City = "AE";
			shipToPartyAddress.OA_PostCode = "0123";

			var declarationBOToExport = Factory.New<BaseJobDeclaration>();
			declarationBOToExport.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToExport.JE_MasterBill = "MYMASTER";
			declarationBOToExport.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToExport.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToExport.JE_SystemCreateTimeUtc = ZDateTime.Now;

			declarationBOToExport.JE_OA_ShipToPartyAddress = shipToPartyAddress.PK;

			Factory.SaveForTesting();

			IMergeDataObjectWriter writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declarationBOToExport)));
			var declarationData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			writer.MergeData(declarationData, declarationBOToExport);

			var shipToPartyDataObject = declarationData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ShipToParty));
			AssertEquals("shipToPartyDataObject.Address1", "ADR", shipToPartyDataObject.Address1);
		}

		public void TestExportMessagingApplicationCode()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNotEquals(ZString.Empty, declaration.JE_ApplicationCode);

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertNotNull("declarationData.MessagingApplicationCode", declarationData.MessagingApplicationCode);
			AssertEquals("declarationData.MessagingApplicationCode.Code", declaration.JE_ApplicationCode, declarationData.MessagingApplicationCode.Code);
			AssertEquals("declarationData.MessagingApplicationCode.Description", declaration.Lookups.ApplicationCodeList.GetDescriptionFromCode(declaration.JE_ApplicationCode), declarationData.MessagingApplicationCode.Description);

			declaration.JE_ApplicationCode = ZString.Empty;
			declarationData = writer.GetDataObject(declaration);
			AssertNull("declarationData.MessagingApplicationCode", declarationData.MessagingApplicationCode);
		}

		public void TestLandedCostingMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = "CONT32423";
				var groupHeader = declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
				groupHeader.JZ_InvoiceNumber = "Group 1";
				var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1000m;
				invoiceLine.JI_InvoiceQuantity = 10m;

				var collection = new LandedCostingGroupCollection();
				var landedCostingGroup1 = collection.AddNew();
				landedCostingGroup1.GroupID = 1;
				landedCostingGroup1.GroupName = "ONE";
				landedCostingGroup1.CostDistributionCode = CostDistributionMechanismList.Codes.Actual;

				var landedCostingGroup2 = collection.AddNew();
				landedCostingGroup2.GroupID = 2;
				landedCostingGroup2.GroupName = "TWO";
				landedCostingGroup2.CostDistributionCode = CostDistributionMechanismList.Codes.Item;

				var landedCostingGroup3 = collection.AddNew();
				landedCostingGroup3.GroupID = 3;
				landedCostingGroup3.GroupName = "THREE";
				landedCostingGroup3.CostDistributionCode = CostDistributionMechanismList.Codes.ActualVolume;

				FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				var processedTime = ZDateTime.Now;
				var landedCostingHeader = Factory.BOFactory.New<LandedCosting.ILandedCostHeader>();
				landedCostingHeader.LT_DateOfProcessing = processedTime;
				landedCostingHeader.LT_ParentID = declaration.PK;
				landedCostingHeader.LT_ParentTableCode = declaration.TablePrefix;
				landedCostingHeader.LT_LandedCostType = "ACT";

				var chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
				var chargeCode1 = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);
				chargeCodeQuery.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, chargeCode1.PK);
				var chargeCode2 = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);
				chargeCodeQuery.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, chargeCode2.PK);
				var chargeCode3 = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);

				var landedCostInput1 = (LandedCosting.ILandCostInput)((IBusinessObjectCollection)landedCostingHeader["CostInputs"]).AddNew();
				landedCostInput1.LI_ParentID = groupHeader.PK;
				landedCostInput1.LI_ParentTableCode = groupHeader.TablePrefix;
				landedCostInput1.LI_AC_ChargeCode = chargeCode1.PK;
				landedCostInput1.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.NewZealand;
				landedCostInput1.LI_LandedCostGroup = 1;
				landedCostInput1.LI_ChargeDescription = "Charge Group Header";
				landedCostInput1.LI_DistributeCostBy = CostDistributionMechanismList.Codes.ActualVolume;
				landedCostInput1.LI_CostAmount = 1500m;
				landedCostInput1.LI_ServiceExRate = 0.8734m;

				var landedCostInput2 = (LandedCosting.ILandCostInput)((IBusinessObjectCollection)landedCostingHeader["CostInputs"]).AddNew();
				landedCostInput2.LI_ParentID = invoice.PK;
				landedCostInput2.LI_ParentTableCode = invoice.TablePrefix;
				landedCostInput2.LI_AC_ChargeCode = chargeCode2.PK;
				landedCostInput2.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.Australia;
				landedCostInput2.LI_LandedCostGroup = 2;
				landedCostInput2.LI_ChargeDescription = "Charge Invoice";
				landedCostInput2.LI_DistributeCostBy = CostDistributionMechanismList.Codes.ActualWeight;
				landedCostInput2.LI_CostAmount = 1200m;
				landedCostInput2.LI_ServiceExRate = 0.7865m;

				var landedCostInput3 = (LandedCosting.ILandCostInput)((IBusinessObjectCollection)landedCostingHeader["CostInputs"]).AddNew();
				landedCostInput3.LI_ParentID = invoiceLine.PK;
				landedCostInput3.LI_ParentTableCode = invoiceLine.TablePrefix;
				landedCostInput3.LI_AC_ChargeCode = chargeCode3.PK;
				landedCostInput3.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.Singapore;
				landedCostInput3.LI_LandedCostGroup = 3;
				landedCostInput3.LI_ChargeDescription = "Charge Invoice Line";
				landedCostInput3.LI_DistributeCostBy = CostDistributionMechanismList.Codes.Item;
				landedCostInput3.LI_CostAmount = 2000m;
				landedCostInput3.LI_ServiceExRate = 1.568m;

				var landedCostInput4 = (LandedCosting.ILandCostInput)((IBusinessObjectCollection)landedCostingHeader["CostInputs"]).AddNew();
				landedCostInput4.LI_ParentID = invoiceLine.PK;
				landedCostInput4.LI_ParentTableCode = invoiceLine.TablePrefix;
				landedCostInput4.LI_AC_ChargeCode = chargeCode1.PK;
				landedCostInput4.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.Australia;
				landedCostInput4.LI_LandedCostGroup = 4;
				landedCostInput4.LI_ChargeDescription = "Charge Invoice Line 2";
				landedCostInput4.LI_DistributeCostBy = CostDistributionMechanismList.Codes.LineValue;
				landedCostInput4.LI_CostAmount = 2500m;
				landedCostInput4.LI_ServiceExRate = 0.7896m;

				var landedCostInput5 = (LandedCosting.ILandCostInput)((IBusinessObjectCollection)landedCostingHeader["CostInputs"]).AddNew();
				landedCostInput5.LI_ParentID = container.PK;
				landedCostInput5.LI_ParentTableCode = container.TablePrefix;
				landedCostInput5.LI_AC_ChargeCode = chargeCode2.PK;
				landedCostInput5.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.Lao;
				landedCostInput5.LI_LandedCostGroup = 2;
				landedCostInput5.LI_ChargeDescription = "Charge Container";
				landedCostInput5.LI_DistributeCostBy = CostDistributionMechanismList.Codes.ActualWeight;
				landedCostInput5.LI_CostAmount = 560m;
				landedCostInput5.LI_ServiceExRate = 0.8965m;

				var landedCostingHistory = (LandedCosting.ILandedCostHistory)((IBusinessObjectCollection)landedCostingHeader["Histories"]).AddNew();
				landedCostingHistory.LH_ParentID = invoiceLine.PK;
				landedCostingHistory.LH_ParentTableCode = invoiceLine.TablePrefix;
				AddChildItemToLandedCostHistory(landedCostingHistory, "ENT", 20m);
				AddChildItemToLandedCostHistory(landedCostingHistory, "GR1", 100m);
				AddChildItemToLandedCostHistory(landedCostingHistory, "GR2", 200m);
				AddChildItemToLandedCostHistory(landedCostingHistory, "GR3", 300m);
				AddChildItemToLandedCostHistory(landedCostingHistory, "GR4", 400m);
				AddChildItemToLandedCostHistory(landedCostingHistory, "GR5", 500m);
				AddChildItemToLandedCostHistory(landedCostingHistory, "GR6", 0m);
				AddChildItemToLandedCostHistory(landedCostingHistory, "MSC", 0m);
				AddChildItemToLandedCostHistory(landedCostingHistory, "ST1", 5m);
				AddChildItemToLandedCostHistory(landedCostingHistory, "ST2", 6m);
				landedCostingHistory.LH_LandedCostMarginPercent1 = 8m;
				landedCostingHistory.LH_LandedCostMarginPercent2 = 9m;
				landedCostingHistory.LH_LandedCostMarginPercent3 = 10m;
				AddChildItemToLandedCostHistory(landedCostingHistory, "OTH", 300m);
				landedCostingHistory.LH_LandedCostHistoryLineType = "ACT";

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var declarationData = writer.GetDataObject(declaration);
				AssertEquals("declarationData.ContainerCollection.Count", 1, declarationData.ContainerCollection.Count);
				var containerData = declarationData.ContainerCollection[0];
				AssertEquals("containerData.TransportLogisticsCostCollection.Count", 1, containerData.TransportLogisticsCostCollection.Count);
				AssertTransportLogisticsCostContents(containerData.TransportLogisticsCostCollection[0], CodeDescriptionPairForTesting.New(chargeCode2.AC_Code, chargeCode2.AC_Desc), "Charge Container", 560m, CodeDescriptionPairForTesting.New(Core.Constants.CurrencyCodes.Lao, "Lao Kip"), CodeDescriptionPairForTesting.New(CostDistributionMechanismList.Codes.ActualWeight, CostDistributionMechanismList.Descriptions.ActualWeight), CodeDescriptionPairForTesting.New("2", "TWO"), 0.8965m);
				AssertNotNull("declarationData.CommercialInfo", declarationData.CommercialInfo);
				AssertEquals("declarationData.CommercialInfo.DateOfLandedCostProcessing", processedTime, declarationData.CommercialInfo.DateOfLandedCostProcessing);
				AssertNull("declarationData.CommercialInfo.TransportLogisticsCostCollection", declarationData.CommercialInfo.TransportLogisticsCostCollection);
				AssertEquals("declarationData.CommercialInfo.SubGroupCollection.Count", 1, declarationData.CommercialInfo.SubGroupCollection.Count);
				var subGroupData = declarationData.CommercialInfo.SubGroupCollection[0];
				AssertNull("subGroupData.DateOfLandedCostProcessing", subGroupData.DateOfLandedCostProcessing);
				AssertEquals("subGroupData.TransportLogisticsCostCollection.Count", 1, subGroupData.TransportLogisticsCostCollection.Count);
				AssertTransportLogisticsCostContents(subGroupData.TransportLogisticsCostCollection[0], CodeDescriptionPairForTesting.New(chargeCode1.AC_Code, chargeCode1.AC_Desc), "Charge Group Header", 1500m, CodeDescriptionPairForTesting.New(Core.Constants.CurrencyCodes.NewZealand, "New Zealand Dollar"), CodeDescriptionPairForTesting.New(CostDistributionMechanismList.Codes.ActualVolume, CostDistributionMechanismList.Descriptions.ActualVolume), CodeDescriptionPairForTesting.New("1", "ONE"), 0.8734m);
				AssertEquals("subGroupData.CommercialInvoiceCollection.Count", 1, subGroupData.CommercialInvoiceCollection.Count);
				var invoiceData = subGroupData.CommercialInvoiceCollection[0];
				AssertEquals("invoiceData.TransportLogisticsCostCollection.Count", 1, invoiceData.TransportLogisticsCostCollection.Count);
				AssertTransportLogisticsCostContents(invoiceData.TransportLogisticsCostCollection[0], CodeDescriptionPairForTesting.New(chargeCode2.AC_Code, chargeCode2.AC_Desc), "Charge Invoice", 1200m, CodeDescriptionPairForTesting.New(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), CodeDescriptionPairForTesting.New(CostDistributionMechanismList.Codes.ActualWeight, CostDistributionMechanismList.Descriptions.ActualWeight), CodeDescriptionPairForTesting.New("2", "TWO"), 0.7865m);
				AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 1, invoiceData.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				AssertEquals("invoiceLineData.TransportLogisticsCostCollection.Count", 2, invoiceLineData.TransportLogisticsCostCollection.Count);
				AssertTransportLogisticsCostContents(invoiceLineData.TransportLogisticsCostCollection[0], CodeDescriptionPairForTesting.New(chargeCode3.AC_Code, chargeCode3.AC_Desc), "Charge Invoice Line", 2000m, CodeDescriptionPairForTesting.New(Core.Constants.CurrencyCodes.Singapore, "Singapore Dollar"), CodeDescriptionPairForTesting.New(CostDistributionMechanismList.Codes.Item, CostDistributionMechanismList.Descriptions.Item), CodeDescriptionPairForTesting.New("3", "THREE"), 1.568m);
				AssertTransportLogisticsCostContents(invoiceLineData.TransportLogisticsCostCollection[1], CodeDescriptionPairForTesting.New(chargeCode1.AC_Code, chargeCode1.AC_Desc), "Charge Invoice Line 2", 2500m, CodeDescriptionPairForTesting.New(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), CodeDescriptionPairForTesting.New(CostDistributionMechanismList.Codes.LineValue, CostDistributionMechanismList.Descriptions.LineValue), CodeDescriptionPairForTesting.New("4", null), 0.7896m);
				AssertLandedCostDetailContents(invoiceLineData.LandedCostDetail, 100m, 33.1m, 150m, 8m, 9m, 10m);
				AssertEquals("invoiceLineData.LandedCostDetail.LandedLineCostItemCollection.Count", 11, invoiceLineData.LandedCostDetail.LandedLineCostItemCollection.Count);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[0], CodeDescriptionPairForTesting.New("ENT", "Entry Fees"), 20m);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[1], CodeDescriptionPairForTesting.New("GR1", "ONE"), 100m);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[2], CodeDescriptionPairForTesting.New("GR2", "TWO"), 200m);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[3], CodeDescriptionPairForTesting.New("GR3", "THREE"), 300m);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[4], CodeDescriptionPairForTesting.New("GR4", "Landed Cost Group 4"), 400m);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[5], CodeDescriptionPairForTesting.New("GR5", "Landed Cost Group 5"), 500m);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[6], CodeDescriptionPairForTesting.New("GR6", "Landed Cost Group 6"), 0m);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[7], CodeDescriptionPairForTesting.New("MSC", "Misc"), 0m);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[8], CodeDescriptionPairForTesting.New("ST1", "MPF"), 5m);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[9], CodeDescriptionPairForTesting.New("ST2", "HMF"), 6m);
				AssertLandedLineCostItemContents(invoiceLineData.LandedCostDetail.LandedLineCostItemCollection[10], CodeDescriptionPairForTesting.New("OTH", "OTH Duty"), 300m);
			}
		}

		public void TestJobDocsAndCartageAreExported()
		{
			var declarationBO = SetupJobDeclaration(Factory.BOFactory.New<BaseJobDeclaration>());

			var docsAndCartage = declarationBO.DocsAndCartage;
			if (docsAndCartage == null)
			{
				JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(declarationBO);
			}

			docsAndCartage = ShipmentDataObjectWriterTest.SetupJobDocsAndCartage(declarationBO.DocsAndCartage, Core.Constants.FCLEquipmentNeeded.Trailer, Core.Constants.AWB.Dimensions.PKS, Core.Constants.FCLEquipmentNeeded.SideLoader, "JAY");
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declarationBO)));
			var declarationData = writer.GetDataObject(declarationBO);
			AssertNotNull("Precondition: declarationData", declarationData);

			var localProcessingData = declarationData.LocalProcessing;
			AssertNotNull(localProcessingData);
			ShipmentDataObjectWriterTest.AssertContents(localProcessingData, GetCodeDescriptionPair(Core.Constants.FCLEquipmentNeeded.Trailer, FCLEquipmentNeededList.Descriptions.Trailer), GetCodeDescriptionPair(Core.Constants.AWB.Dimensions.PKS, "Dimensions of outer Packs only"), GetCodeDescriptionPair(Core.Constants.FCLEquipmentNeeded.SideLoader, FCLEquipmentNeededList.Descriptions.SideLoader), GetCodeDescriptionPair("JAY", null));
		}

		public void TestManufacturerAddressInDeclarationIsExported()
		{
			void Setter(BaseJobDeclaration declaration, OrgAddress address)
			{
				declaration.JE_OH_Manufacturer = address.OA_OH;
				declaration.JE_OA_ManufacturerAddress = address.PK;
			}

			AssertAddressIsExported(DocAddressType.Manufacturer, Setter, false, true, false);
		}

		public void TestSellerAddressInDeclarationIsExported()
		{
			AssertAddressIsExported(Constants.AddressTypes.Seller, (dec, address) => dec.JE_OA_SellerAddress = address.PK, false, true, false);
		}

		public void TestSoldToPartyAddressInDeclarationIsExported()
		{
			AssertAddressIsExported(Constants.AddressTypes.SoldToParty, (dec, address) => dec.JE_OA_SoldToPartyAddress = address.PK, true, false, false);
		}

		public void TestExporterInDeclarationIsExported()
		{
			AssertAddressIsExported(DocAddressType.Exporter, (dec, address) => dec.JE_OH_Exporter = address.OA_OH, true, false);
		}

		public void TestBuyingAgentInDeclarationIsExported()
		{
			AssertAddressIsExported(Constants.AddressTypes.BuyingAgent, (dec, address) => dec.JE_OH_BuyingAgent = address.OA_OH, true, false);
		}

		public void TestConsigneeAddressInDeclarationIsExported()
		{
			AssertAddressIsExported(Constants.AddressTypes.UltimateConsignee, (dec, address) => dec.JE_OA_ConsigneeAddress = address.PK, true, false);
		}

		public void TestConsigneeInDeclarationIsExported()
		{
			AssertAddressIsExported(Constants.AddressTypes.IntermediateConsignee, (dec, address) => dec.JE_OH_Consignee = address.OA_OH, true, false);
		}

		public void TestSellingingAgentInDeclarationIsExported()
		{
			AssertAddressIsExported(Constants.AddressTypes.SellingAgent, (dec, address) => dec.JE_OH_SellingAgent = address.OA_OH, false, true);
		}

		public void TestBuyerInDeclarationIsExported()
		{
			AssertAddressIsExported(DocAddressType.BuyerDocumentaryAddress, (dec, address) => dec.JE_OH_Buyer = address.OA_OH, true, false);
		}

		public void TestControllingAgentInDeaclarationIsExported()
		{
			AssertAddressIsExported(DocAddressType.ControllingAgent, (dec, address) => dec.JE_OH_ControllingAgent = address.OA_OH, true, false);
		}

		public void TestControllingCustomerInDeaclarationIsExported()
		{
			AssertAddressIsExported(DocAddressType.ControllingCustomer, (dec, address) => dec.JE_OH_ControllingCustomer = address.OA_OH, true, false);
		}

		public void TestExternalBrokerInDeaclarationIsExported()
		{
			AssertAddressIsExported(DocAddressType.ExternalBroker, (dec, address) => dec.JE_OH_ExternalBroker = address.OA_OH, true, false);
		}

		string PreparePurposeCode(string primaryDataSource)
		{
			var factory = new BusinessObjectFactory();
			var filter = factory.New<EDIMessageContentFilter>();
			filter.ECF_Name = "ABC";
			filter.UniversalShipment.AdditionalConfiguration.PrimaryDataSource = primaryDataSource;

			var purpose = factory.New<EDIMessagePurpose>();
			purpose.EMP_Code = "FFF";
			purpose.EMP_Description = "FFF";
			purpose.EMP_ECF_Filter = filter.PK;

			factory.Save();
			return purpose.EMP_Code;
		}

		public void TestMergeDataWithPrimarySourceAsBrokerage()
		{
			var purposeCode = PreparePurposeCode(EDIMessageContentPrimaryDataSource.Codes.Brokerage);
			AssertIMergeDataObjectWriter_MergeData_UseDeclarationData_WithPopulatedShipment(true, true, purposeCode);
			AssertIMergeDataObjectWriter_MergeData_UseDeclarationData_WithPopulatedShipment(false, true, purposeCode);
			AssertIMergeDataObjectWriter_MergeData_UseDeclarationData_WithEmptyShipment(true, true, purposeCode);
			AssertIMergeDataObjectWriter_MergeData_UseDeclarationData_WithEmptyShipment(false, true, purposeCode);
		}

		public void TestMergeDataWithPrimarySourceAsShipment()
		{
			var purposeCode = PreparePurposeCode(EDIMessageContentPrimaryDataSource.Codes.Shipment);
			AssertIMergeDataObjectWriter_MergeData_UseShipmentData_WithPopulatedShipment(true, purposeCode);
			AssertIMergeDataObjectWriter_MergeData_UseShipmentData_WithPopulatedShipment(false, purposeCode);
			AssertIMergeDataObjectWriter_MergeData_UseShipmentData_WithEmptyShipment(true, purposeCode);
			AssertIMergeDataObjectWriter_MergeData_UseShipmentData_WithEmptyShipment(false, purposeCode);
		}

		public void TestMergeDataWithoutPrimarySource()
		{
			var purposeCode = PreparePurposeCode("");
			AssertIMergeDataObjectWriter_MergeData_UseDeclarationData_WithPopulatedShipment(true, false, purposeCode);
			AssertIMergeDataObjectWriter_MergeData_UseShipmentData_WithPopulatedShipment(false, purposeCode);
			AssertIMergeDataObjectWriter_MergeData_UseDeclarationData_WithEmptyShipment(true, false, purposeCode);
			AssertIMergeDataObjectWriter_MergeData_UseShipmentData_WithEmptyShipment(false, purposeCode);
		}

		public void TestMergeDataWithDifferentUseBrokerageDataFirst()
		{
			AssertIMergeDataObjectWriter_MergeData_UseDeclarationData_WithPopulatedShipment(true, false);
			AssertIMergeDataObjectWriter_MergeData_UseShipmentData_WithPopulatedShipment(false);
			AssertIMergeDataObjectWriter_MergeData_UseDeclarationData_WithEmptyShipment(true, false);
			AssertIMergeDataObjectWriter_MergeData_UseShipmentData_WithEmptyShipment(false);
		}

		void AssertIMergeDataObjectWriter_MergeData_UseDeclarationData_WithEmptyShipment(bool useBrokerageDataFirststring, bool isPrimarySourceAsBrokerage, string purposeCode = null)
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, useBrokerageDataFirststring);
				var declarationBO = CreateDeclarationForMerge();
				var throwAway = declarationBO.ImporterDeliveryAddress;
				throwAway = declarationBO.SupplierPickupAddress;

				IMergeDataObjectWriter writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declarationBO) { PurposeCode = purposeCode }));
				var declarationData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				writer.MergeData(declarationData, declarationBO);
				AssertContents(declarationData, "MB1HB1", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), GetCodeDescriptionPair(Core.Constants.ContainerModes.Containerised, "Containerized"), 1,
					"FUNNY GOODS", GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, "Export"), GetCodeDescriptionPair("ST1", null), 100, GetCodeDescriptionPair(Core.Constants.PkgUnit.Piece, "Piece"),
					10.50m, GetCodeDescriptionPair(Core.Constants.Volume.CubicFeet, "Cubic Feet"), 1506.682m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(Core.Constants.TransportModes.Sea, "Sea Freight"),
					GetCodeDescriptionPair(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), GetCodeDescriptionPair(SeaLocalPort2.RL_Code, SeaLocalPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName), GetCodeDescriptionPair(SeaForeignPort2.RL_Code, SeaForeignPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort3.RL_Code, SeaForeignPort3.RL_PortName),
					"APL EMERALD", "V23W", GetCodeDescriptionPair(GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), GetCodeDescriptionPair("", null), ZBool.True,
					GetCodeDescriptionPair("EFT", null), GetCodeDescriptionPair(OrgConstants.MergeInvoiceLines.Tariff, "Tariff"), GetCodeDescriptionPair("OS1", null), GetCodeDescriptionPair("MS1", null), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.AwaitingFDACorrection, null),
					GetCodeDescriptionPair("CC1", null), 112, 307, GetCodeDescriptionPair("WR1", null), "7819369",
					GetCodeDescriptionPair("SP", "Spare parts for the vessel/aircraft"), "AGREF123", "OWN324", "F234", GetCodeDescriptionPair(PaymentPartyCodeDescriptionList.Codes.Broker, PaymentPartyCodeDescriptionList.Descriptions.Broker), GetCodeDescriptionPair(MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, MasterFiles.Business.Customs.PaidByCodeList.Descriptions.BRK),
					GetCodeDescriptionPair("FOB", "Free On Board"), 789.012m, GetCodeDescriptionPair(ScreeningStatusesList.Codes.Unknown, "Unknown"), GetCodeDescriptionPair("AB", null));

				AssertNotNull("declarationData.AdditionalBillCollection", declarationData.AdditionalBillCollection);
				AssertEquals("declarationData.AdditionalBillCollection.Count", 2, declarationData.AdditionalBillCollection.Count);
				AssertContents(declarationData.AdditionalBillCollection[0], "MB1", GetCodeDescriptionPair(WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master), new ZDateTime(2011, 5, 20), null, true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(declarationData.AdditionalBillCollection[1], "MB1HB1", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), new ZDateTime(2011, 5, 21), "MB1", true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));

				AssertNotNull("declarationData.ContainerCollection", declarationData.ContainerCollection);
				AssertEquals("declarationData.ContainerCollection.Count", 1, declarationData.ContainerCollection.Count);
				var containerData = declarationData.ContainerCollection[0];
				AssertContents(containerData, "CONT1", 1500.50m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), "C1S1", "C1S2", GetCodeDescriptionPair(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModeDescriptions.LCL), 10m, 11m, 12m, GetCodeDescriptionPair("CLR", null), null);
				AssertNotNull("containerData.CustomizedFieldCollection", containerData.CustomizedFieldCollection);
				AssertEquals("containerData.CustomizedFieldCollection.Count", 4, containerData.CustomizedFieldCollection.Count);
				containerData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING1", "COATT1");
				containerData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE1", new ZDateTime(2011, 8, 18).ToISO8601String());
				containerData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL1", "30.2");
				containerData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "FLAG1", "true");

				if (isPrimarySourceAsBrokerage)
				{
					AssertContents(declarationData.PackingLineCollection[0], "CONT1", "MB1HB1", GetCodeDescriptionPair("HWB", "House Waybill"), "MARKS 1", 10, GetCodeDescriptionPair("BOX", null), 11, 9, "SHIPPING");
				}
				else
				{
					AssertNull("declarationData.PackingLineCollection should be whatever the shipment for non-standalone", declarationData.PackingLineCollection);
				}

				AssertNull("declarationData.TransportLegCollection should not be populated as it should come from Shipment", declarationData.TransportLegCollection);
				AssertNull("declarationData.NoteCollection should not be populated as it should come from Shipment", declarationData.NoteCollection);

				AssertNotNull("declarationData.OrganizationAddressCollection", declarationData.OrganizationAddressCollection);
				AssertEquals("declarationData.OrganizationAddressCollection.Count", 6, declarationData.OrganizationAddressCollection.Count);
				AssertOrganizationBO_CRAHOLSYD("SupplierDocumentaryAddress", declarationData.OrganizationAddressCollection[4], "SupplierDocumentaryAddress", true);
				AssertOrganizationBO_WUFSHIJNB("ImporterDocumentaryAddress", declarationData.OrganizationAddressCollection[5], "ImporterDocumentaryAddress", true);
				AssertOrganizationBO_WUFSHIJNB("Supplier", declarationData.OrganizationAddressCollection[0], "Supplier");
				AssertOrganizationBO_CRAHOLSYD("Importer", declarationData.OrganizationAddressCollection[1], "Importer");
				AssertOrganizationBO_WUFSHIJNB("Forwarder", declarationData.OrganizationAddressCollection[2], "Forwarder");
				AssertOrganizationBO_CRAHOLSYD("ShippingLine", declarationData.OrganizationAddressCollection[3], "ShippingLine");

				AssertNotNull("declarationData.CommercialInfo", declarationData.CommercialInfo);
				AssertNotNull("declarationData.ChargeCollection", declarationData.CommercialInfo.CommercialChargeCollection);
				AssertEquals("declarationData.CommercialInfo.ChargeCollection.Count", 2, declarationData.CommercialInfo.CommercialChargeCollection.Count);
				var localCurrency = GetCodeDescriptionPair(declarationBO.LocalCurrency.RX_Code, declarationBO.LocalCurrency.RX_Desc);
				AssertContents(declarationData.CommercialInfo.CommercialChargeCollection[0], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertContents(declarationData.CommercialInfo.CommercialChargeCollection[1], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertNotNull("declarationData.InvoiceCollection", declarationData.CommercialInfo.CommercialInvoiceCollection);
				AssertEquals("declarationData.CommercialInfo.InvoiceCollection.Count", 1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);
				var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection[0];
				AssertContentsWithLookingAtChildren(invoiceData, "INV3243", AssertOrganizationBO_WUFSHIJNB, AssertOrganizationBO_CRAHOLSYD, 3420.34m, GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), new ZDateTime(2011, 4, 3), GetCodeDescriptionPair(Core.Constants.IncoTerms.FreeOnBoard, "Free On Board"), 14.72m, GetCodeDescriptionPair(Core.Constants.Volume.CubicMetres, "Cubic Meters"), 2.53m, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), 11.11m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 1.25m, 1.50m, "P12345", 1500m, 1.75m, new ZDateTime(2011, 3, 3), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearElectronicInvoiceOriginal, null), 10m);
				AssertNotNull("invoiceData.CommercialChargeCollection", invoiceData.CommercialChargeCollection);
				AssertEquals("invoiceData.CommercialChargeCollection.Count", 4, invoiceData.CommercialChargeCollection.Count);
				AssertContents(invoiceData.CommercialChargeCollection[0], ZBool.False, 10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceData.CommercialChargeCollection[1], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceData.CommercialChargeCollection[2], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceData.CommercialChargeCollection[3], ZBool.False, 100m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));

				AssertNotNull("Precondition: invoiceData.CommercialInvoiceLineCollection", invoiceData.CommercialInvoiceLineCollection);
				AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 1, invoiceData.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				AssertContents(invoiceLineData, 1, "1010101010", "1010.10.10 10", "GOODS", 1040.50m, GetCodeDescriptionPair(Core.Constants.PkgUnit.Box, "Box"), 4162551.46m, 4000.53m, "PART12", 3.2m, GetCodeDescriptionPair(Core.Constants.Volume.CubicYards, "Cubic Yards"), 202.92m, GetCodeDescriptionPair(Core.Constants.Weight.Hectograms, "Hectograms"), "ORDER1", 1.555m, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), 10m, GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null), GetCodeDescriptionPair(Core.Constants.CountryCodes.Australia, "Australia"), GetCodeDescriptionPair(CommodityCode1.RH_Code, CommodityCode1.RH_Description), GetCodeDescriptionPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk), "MK0001");
				AssertNotNull("invoiceLineData.CommercialChargeCollection", invoiceLineData.CommercialChargeCollection);
				AssertEquals("invoiceLineData.CommercialChargeCollection.Count", 4, invoiceLineData.CommercialChargeCollection.Count);
				AssertContents(invoiceLineData.CommercialChargeCollection[0], ZBool.False, 10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceLineData.CommercialChargeCollection[1], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertContents(invoiceLineData.CommercialChargeCollection[2], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertContents(invoiceLineData.CommercialChargeCollection[3], ZBool.False, 100m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));

				AssertNotNull("declarationData.EntryNumberCollection", declarationData.EntryNumberCollection);
				AssertEquals("declarationData.EntryNumberCollection.Count", 1, declarationData.EntryNumberCollection.Count);
				AssertContents(declarationData.EntryNumberCollection[0], GetCodeDescriptionPair(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Desc), true, "IMPDEC", new ZDateTime(2011, 7, 5), "IMP123", GetCodeDescriptionPair(JobMessageTypeList.Codes.Import, null));

				AssertNotNull("declarationData.AdditionalReferenceCollection", declarationData.AdditionalReferenceCollection);
				AssertEquals("declarationData.AdditionalReferenceCollection.Count", 1, declarationData.AdditionalReferenceCollection.Count);
				AssertContents(declarationData.AdditionalReferenceCollection[0], "ITDEC", new ZDateTime(2011, 6, 3), "IT123", GetCodeDescriptionPair(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, UnitedStatesAdditionalReferenceNumberTypes.Descriptions.IT));

				AssertNotNull("declarationData.DateCollection", declarationData.DateCollection);
				AssertEquals("declarationData.DateCollection.Count", 10, declarationData.DateCollection.Count);
				AssertContents(declarationData.DateCollection[9], DateType.BillIssued, new ZDateTime(2011, 5, 21), ZBool.False);
				AssertContents(declarationData.DateCollection[0], DateType.Departure, new ZDateTime(2011, 7, 17), ZBool.True);
				AssertContents(declarationData.DateCollection[1], DateType.LoadingDate, new ZDateTime(2011, 6, 12), ZBool.False);
				AssertContents(declarationData.DateCollection[2], DateType.FirstArrivalInCountry, new ZDateTime(2011, 6, 11), ZBool.False);
				AssertContents(declarationData.DateCollection[3], DateType.DischargeDate, new ZDateTime(2011, 6, 10), ZBool.False);
				AssertContents(declarationData.DateCollection[4], DateType.Arrival, new ZDateTime(2011, 7, 13), ZBool.True);
				AssertContents(declarationData.DateCollection[5], DateType.EntrySubmitted, new ZDateTime(2011, 7, 14), ZBool.False);
				AssertContents(declarationData.DateCollection[6], DateType.EntryAuthorisation, new ZDateTime(2011, 7, 15), ZBool.False);
				AssertContents(declarationData.DateCollection[7], DateType.WarehouseRelease, new ZDateTime(2011, 7, 16), ZBool.False);
				AssertContents(declarationData.DateCollection[8], DateType.EntryDate, new ZDateTime(2011, 7, 18), ZBool.False);

				AssertNotNull("declarationData.EntryHeaderCollection", declarationData.EntryHeaderCollection);
				AssertEquals("declarationData.EntryHeaderCollection.Count", 1, declarationData.EntryHeaderCollection.Count);
				var entryHeaderData = declarationData.EntryHeaderCollection[0];
				AssertContentsWithLookingAtChildren(entryHeaderData, new ZDateTime(2011, 2, 3), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, null), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, null), new ZDateTime(2011, 2, 2), 1404.24m, GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export), "BDG34332", new ZDate(2011, 2, 7));

				AssertNotNull("entryHeaderData.EntryHeaderChargeCollection", entryHeaderData.EntryHeaderChargeCollection);
				AssertEquals("entryHeaderData.EntryHeaderChargeCollection.Count", 1, entryHeaderData.EntryHeaderChargeCollection.Count);
				var entryHeaderChargeData = entryHeaderData.EntryHeaderChargeCollection[0];
				AssertContents(entryHeaderChargeData, 236.45m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, null));

				AssertNotNull("entryHeaderData.EntryNumberCollection", entryHeaderData.EntryNumberCollection);
				AssertEquals("entryHeaderData.EntryNumberCollection.Count", 1, entryHeaderData.EntryNumberCollection.Count);
				var entryNumberData = entryHeaderData.EntryNumberCollection[0];
				AssertContents(entryNumberData, false, "REFERENCE", "CE00001", GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export));

				AssertNotNull("entryHeaderData.EntryLineCollection", entryHeaderData.EntryLineCollection);
				AssertEquals("entryHeaderData.EntryLineCollection.Count", 1, entryHeaderData.EntryLineCollection.Count);
				var entryLineData = entryHeaderData.EntryLineCollection[0];
				AssertContentsWithLookingAtChildren(entryLineData, "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), 72.23m, GetCodeDescriptionPair(EntryLineStatusList.Codes.Active, EntryLineStatusList.Descriptions.Active));

				AssertNotNull("Precondition: entryLineData.EntryLineChargeCollection", entryLineData.EntryLineChargeCollection);
				AssertEquals("entryLineData.EntryLineChargeCollection.Count", 1, entryLineData.EntryLineChargeCollection.Count);
				AssertContents(entryLineData.EntryLineChargeCollection[0], 102.23m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.Blueberry, null));
			}
		}

		void AssertIMergeDataObjectWriter_MergeData_UseDeclarationData_WithPopulatedShipment(bool useBrokerageDataFirststring, bool isPrimarySourceAsBrokerage, string purposeCode = null)
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, useBrokerageDataFirststring);
				var declarationBO = CreateDeclarationForMerge();
				var throwAway = declarationBO.ImporterDeliveryAddress;
				throwAway = declarationBO.SupplierPickupAddress;
				IMergeDataObjectWriter writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declarationBO) { PurposeCode = purposeCode }));
				var declarationData = CreatePopulatedShipmentDataForMerge();
				writer.MergeData(declarationData, declarationBO);
				AssertContents(declarationData, "MB1HB1", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), GetCodeDescriptionPair(Core.Constants.ContainerModes.Containerised, "Containerized"), 1,
					"FUNNY GOODS", GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, "Export"), GetCodeDescriptionPair("ST1", null), 100, GetCodeDescriptionPair(Core.Constants.PkgUnit.Piece, "Piece"),
					10.50m, GetCodeDescriptionPair(Core.Constants.Volume.CubicFeet, "Cubic Feet"), 1506.682m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(Core.Constants.TransportModes.Sea, "Sea Freight"),
					GetCodeDescriptionPair(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), GetCodeDescriptionPair(SeaLocalPort2.RL_Code, SeaLocalPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName), GetCodeDescriptionPair(SeaForeignPort2.RL_Code, SeaForeignPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort3.RL_Code, SeaForeignPort3.RL_PortName),
					"APL EMERALD", "V23W", GetCodeDescriptionPair(GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), GetCodeDescriptionPair("", null), ZBool.True,
					GetCodeDescriptionPair("EFT", null), GetCodeDescriptionPair(OrgConstants.MergeInvoiceLines.Tariff, "Tariff"), GetCodeDescriptionPair("OS1", null), GetCodeDescriptionPair("MS1", null), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.AwaitingFDACorrection, null),
					GetCodeDescriptionPair("CC1", null), 112, 307, GetCodeDescriptionPair("WR1", null), "7819369",
					GetCodeDescriptionPair("SP", "Spare parts for the vessel/aircraft"), "AGREF123", "OWN324", "F234", GetCodeDescriptionPair(PaymentPartyCodeDescriptionList.Codes.Broker, PaymentPartyCodeDescriptionList.Descriptions.Broker), GetCodeDescriptionPair(MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, MasterFiles.Business.Customs.PaidByCodeList.Descriptions.BRK),
					GetCodeDescriptionPair("FOB", "Free On Board"), 789.012m, GetCodeDescriptionPair(ScreeningStatusesList.Codes.Unknown, "Unknown"), GetCodeDescriptionPair("AB", null));

				AssertNotNull("declarationData.AdditionalBillCollection", declarationData.AdditionalBillCollection);
				AssertEquals("declarationData.AdditionalBillCollection.Count", 2, declarationData.AdditionalBillCollection.Count);
				AssertContents(declarationData.AdditionalBillCollection[0], "MB1", GetCodeDescriptionPair(WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master), new ZDateTime(2011, 5, 20), null, true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(declarationData.AdditionalBillCollection[1], "MB1HB1", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), new ZDateTime(2011, 5, 21), "MB1", true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));

				AssertNotNull("declarationData.ContainerCollection", declarationData.ContainerCollection);
				AssertEquals("declarationData.ContainerCollection.Count", 1, declarationData.ContainerCollection.Count);
				var containerData = declarationData.ContainerCollection[0];
				AssertContents(containerData, "CONT1", 1500.50m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), "C1S1", "C1S2", GetCodeDescriptionPair(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModeDescriptions.LCL), 10m, 11m, 12m, GetCodeDescriptionPair("CLR", null), null);
				AssertNotNull("containerData.CustomizedFieldCollection", containerData.CustomizedFieldCollection);
				AssertEquals("containerData.CustomizedFieldCollection.Count", 4, containerData.CustomizedFieldCollection.Count);
				containerData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING1", "COATT1");
				containerData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE1", new ZDateTime(2011, 8, 18).ToISO8601String());
				containerData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL1", "30.2");
				containerData.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "FLAG1", "true");

				AssertNotNull("declarationData.PackingLineCollection", declarationData.PackingLineCollection);
				AssertEquals("declarationData.PackingLineCollection.Count", 1, declarationData.PackingLineCollection.Count);

				if (isPrimarySourceAsBrokerage)
				{
					AssertContents(declarationData.PackingLineCollection[0], "CONT1", "MB1HB1", GetCodeDescriptionPair("HWB", "House Waybill"), "MARKS 1", 10, GetCodeDescriptionPair("BOX", null), 11, 9, "SHIPPING");
					AssertNull("declarationData.TransportLegCollection", declarationData.TransportLegCollection);
					AssertNull("declarationData.NoteCollection", declarationData.NoteCollection);
				}
				else
				{
					AssertContents(declarationData.PackingLineCollection[0], "CNT3459", "HB38987", GetCodeDescriptionPair("GDH", "GDH DESCRIPTION"), "SHP MARKS", 101, GetCodeDescriptionPair("P3", "P3 DESC"), 99, 88, "SHP SYMBOL");
					AssertNotNull("declarationData.TransportLegCollection", declarationData.TransportLegCollection);
					AssertEquals("declarationData.TransportLegCollection.Count", 1, declarationData.TransportLegCollection.Count);
					AssertContents(declarationData.TransportLegCollection[0], TransportMode.Sea, "VESSEL 3234", "V598", GetCodeDescriptionPair("LOAD8", "LOAD8 NAME"), GetCodeDescriptionPair("DISC9", "DISC9 NAME"));
					AssertNotNull("declarationData.NoteCollection", declarationData.NoteCollection);
					AssertEquals("declarationData.NoteCollection.Count", 1, declarationData.NoteCollection.Count);
					AssertContents(declarationData.NoteCollection[0], true, "SHIP DESC", "WHAT IS THIS");
				}

				AssertNotNull("declarationData.OrganizationAddressCollection", declarationData.OrganizationAddressCollection);
				AssertEquals("declarationData.OrganizationAddressCollection.Count", 7, declarationData.OrganizationAddressCollection.Count);
				AssertAddress("LocalClient", declarationData.OrganizationAddressCollection[6], "LocalClient", "BOBORGCODE", "BOB THE BUILDER", null, null, null, null, null, null, null, null, null, null, null, null);
				AssertOrganizationBO_CRAHOLSYD("SupplierDocumentaryAddress", declarationData.OrganizationAddressCollection[4], "SupplierDocumentaryAddress", true);
				AssertOrganizationBO_WUFSHIJNB("ImporterDocumentaryAddress", declarationData.OrganizationAddressCollection[5], "ImporterDocumentaryAddress", true);
				AssertOrganizationBO_WUFSHIJNB("Supplier", declarationData.OrganizationAddressCollection[0], "Supplier");
				AssertOrganizationBO_CRAHOLSYD("Importer", declarationData.OrganizationAddressCollection[1], "Importer");
				AssertOrganizationBO_WUFSHIJNB("Forwarder", declarationData.OrganizationAddressCollection[2], "Forwarder");
				AssertOrganizationBO_CRAHOLSYD("ShippingLine", declarationData.OrganizationAddressCollection[3], "ShippingLine");

				AssertNotNull("declarationData.CommercialInfo", declarationData.CommercialInfo);
				AssertNotNull("declarationData.ChargeCollection", declarationData.CommercialInfo.CommercialChargeCollection);
				AssertEquals("declarationData.CommercialInfo.ChargeCollection.Count", 2, declarationData.CommercialInfo.CommercialChargeCollection.Count);
				var localCurrency = GetCodeDescriptionPair(declarationBO.LocalCurrency.RX_Code, declarationBO.LocalCurrency.RX_Desc);
				AssertContents(declarationData.CommercialInfo.CommercialChargeCollection[0], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertContents(declarationData.CommercialInfo.CommercialChargeCollection[1], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertNotNull("declarationData.InvoiceCollection", declarationData.CommercialInfo.CommercialInvoiceCollection);
				AssertEquals("declarationData.CommercialInfo.InvoiceCollection.Count", 1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);
				var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection[0];
				AssertContentsWithLookingAtChildren(invoiceData, "INV3243", AssertOrganizationBO_WUFSHIJNB, AssertOrganizationBO_CRAHOLSYD, 3420.34m, GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), new ZDateTime(2011, 4, 3), GetCodeDescriptionPair(Core.Constants.IncoTerms.FreeOnBoard, "Free On Board"), 14.72m, GetCodeDescriptionPair(Core.Constants.Volume.CubicMetres, "Cubic Meters"), 2.53m, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), 11.11m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 1.25m, 1.50m, "P12345", 1500m, 1.75m, new ZDateTime(2011, 3, 3), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearElectronicInvoiceOriginal, null), 10m);
				AssertNotNull("invoiceData.CommercialChargeCollection", invoiceData.CommercialChargeCollection);
				AssertEquals("invoiceData.CommercialChargeCollection.Count", 4, invoiceData.CommercialChargeCollection.Count);
				AssertContents(invoiceData.CommercialChargeCollection[0], ZBool.False, 10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceData.CommercialChargeCollection[1], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceData.CommercialChargeCollection[2], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceData.CommercialChargeCollection[3], ZBool.False, 100m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));

				AssertNotNull("Precondition: invoiceData.CommercialInvoiceLineCollection", invoiceData.CommercialInvoiceLineCollection);
				AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 1, invoiceData.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				AssertContents(invoiceLineData, 1, "1010101010", "1010.10.10 10", "GOODS", 1040.50m, GetCodeDescriptionPair(Core.Constants.PkgUnit.Box, "Box"), 4162551.46m, 4000.53m, "PART12", 3.2m, GetCodeDescriptionPair(Core.Constants.Volume.CubicYards, "Cubic Yards"), 202.92m, GetCodeDescriptionPair(Core.Constants.Weight.Hectograms, "Hectograms"), "ORDER1", 1.555m, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), 10m, GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null), GetCodeDescriptionPair(Core.Constants.CountryCodes.Australia, "Australia"), GetCodeDescriptionPair(CommodityCode1.RH_Code, CommodityCode1.RH_Description), GetCodeDescriptionPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk), "MK0001");
				AssertNotNull("invoiceLineData.CommercialChargeCollection", invoiceLineData.CommercialChargeCollection);
				AssertEquals("invoiceLineData.CommercialChargeCollection.Count", 4, invoiceLineData.CommercialChargeCollection.Count);
				AssertContents(invoiceLineData.CommercialChargeCollection[0], ZBool.False, 10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceLineData.CommercialChargeCollection[1], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertContents(invoiceLineData.CommercialChargeCollection[2], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertContents(invoiceLineData.CommercialChargeCollection[3], ZBool.False, 100m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));

				AssertNotNull("declarationData.EntryNumberCollection", declarationData.EntryNumberCollection);
				AssertEquals("declarationData.EntryNumberCollection.Count", 1, declarationData.EntryNumberCollection.Count);
				AssertContents(declarationData.EntryNumberCollection[0], GetCodeDescriptionPair(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Desc), true, "IGD4545", new ZDateTime(2011, 7, 5), "IMP123", GetCodeDescriptionPair(JobMessageTypeList.Codes.Import, null));

				AssertNotNull("declarationData.AdditionalReferenceCollection", declarationData.AdditionalReferenceCollection);
				AssertEquals("declarationData.AdditionalReferenceCollection.Count", 1, declarationData.AdditionalReferenceCollection.Count);
				AssertContents(declarationData.AdditionalReferenceCollection[0], "ITDEC", new ZDateTime(2011, 6, 3), "IT123", GetCodeDescriptionPair(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, UnitedStatesAdditionalReferenceNumberTypes.Descriptions.IT));

				AssertNotNull("declarationData.DateCollection", declarationData.DateCollection);
				AssertEquals("declarationData.DateCollection.Count", 10, declarationData.DateCollection.Count);
				AssertContents(declarationData.DateCollection[9], DateType.BillIssued, new ZDateTime(2011, 5, 21), ZBool.False);
				AssertContents(declarationData.DateCollection[0], DateType.Departure, new ZDateTime(2011, 7, 17), ZBool.True);
				AssertContents(declarationData.DateCollection[1], DateType.LoadingDate, new ZDateTime(2011, 6, 12), ZBool.False);
				AssertContents(declarationData.DateCollection[2], DateType.FirstArrivalInCountry, new ZDateTime(2011, 6, 11), ZBool.False);
				AssertContents(declarationData.DateCollection[3], DateType.DischargeDate, new ZDateTime(2011, 6, 10), ZBool.False);
				AssertContents(declarationData.DateCollection[4], DateType.Arrival, new ZDateTime(2011, 7, 13), ZBool.True);
				AssertContents(declarationData.DateCollection[5], DateType.EntrySubmitted, new ZDateTime(2011, 7, 14), ZBool.False);
				AssertContents(declarationData.DateCollection[6], DateType.EntryAuthorisation, new ZDateTime(2011, 7, 15), ZBool.False);
				AssertContents(declarationData.DateCollection[7], DateType.WarehouseRelease, new ZDateTime(2011, 7, 16), ZBool.False);
				AssertContents(declarationData.DateCollection[8], DateType.EntryDate, new ZDateTime(2011, 7, 18), ZBool.False);

				AssertNotNull("declarationData.EntryHeaderCollection", declarationData.EntryHeaderCollection);
				AssertEquals("declarationData.EntryHeaderCollection.Count", 1, declarationData.EntryHeaderCollection.Count);
				var entryHeaderData = declarationData.EntryHeaderCollection[0];
				AssertContentsWithLookingAtChildren(entryHeaderData, new ZDateTime(2011, 2, 3), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, null), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, null), new ZDateTime(2011, 2, 2), 1404.24m, GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export), "BDG34332", new ZDate(2011, 2, 7));

				AssertNotNull("entryHeaderData.EntryLineCollection", entryHeaderData.EntryLineCollection);
				AssertEquals("entryHeaderData.EntryLineCollection.Count", 1, entryHeaderData.EntryLineCollection.Count);
				var entryLineData = entryHeaderData.EntryLineCollection[0];
				AssertContentsWithLookingAtChildren(entryLineData, "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), 72.23m, GetCodeDescriptionPair(EntryLineStatusList.Codes.Active, EntryLineStatusList.Descriptions.Active));

				AssertNotNull("entryHeaderData.EntryHeaderChargeCollection", entryHeaderData.EntryHeaderChargeCollection);
				AssertEquals("entryHeaderData.EntryHeaderChargeCollection.Count", 1, entryHeaderData.EntryHeaderChargeCollection.Count);
				var entryHeaderChargeData = entryHeaderData.EntryHeaderChargeCollection[0];
				AssertContents(entryHeaderChargeData, 236.45m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, null));

				AssertNotNull("entryHeaderData.EntryNumberCollection", entryHeaderData.EntryNumberCollection);
				AssertEquals("entryHeaderData.EntryNumberCollection.Count", 1, entryHeaderData.EntryNumberCollection.Count);
				var entryNumberData = entryHeaderData.EntryNumberCollection[0];
				AssertContents(entryNumberData, false, "REFERENCE", "CE00001", GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export));

				AssertNotNull("entryHeaderData.EntryLineCollection", entryHeaderData.EntryLineCollection);
				AssertEquals("entryHeaderData.EntryLineCollection.Count", 1, entryHeaderData.EntryLineCollection.Count);
				entryLineData = entryHeaderData.EntryLineCollection[0];
				AssertContentsWithLookingAtChildren(entryLineData, "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), 72.23m, GetCodeDescriptionPair(EntryLineStatusList.Codes.Active, EntryLineStatusList.Descriptions.Active));

				AssertNotNull("Precondition: entryLineData.EntryLineChargeCollection", entryLineData.EntryLineChargeCollection);
				AssertEquals("entryLineData.EntryLineChargeCollection.Count", 1, entryLineData.EntryLineChargeCollection.Count);
				AssertContents(entryLineData.EntryLineChargeCollection[0], 102.23m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.Blueberry, null));
			}
		}

		void AssertIMergeDataObjectWriter_MergeData_UseShipmentData_WithEmptyShipment(bool useBrokerageDataFirststring, string purposeCode = null)
		{
			eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, useBrokerageDataFirststring);
			var declarationBO = CreateDeclarationForMerge(JobMessageTypeList.Codes.Import);
			var containerBO = declarationBO.CusContainers[0];
			var processedTime = ZDateTime.Now;
			var landedCostingHeader = Factory.BOFactory.New<LandedCosting.ILandedCostHeader>();
			landedCostingHeader.LT_DateOfProcessing = processedTime;
			landedCostingHeader.LT_ParentID = declarationBO.PK;
			landedCostingHeader.LT_ParentTableCode = declarationBO.TablePrefix;
			landedCostingHeader.LT_LandedCostType = "ACT";

			var chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var chargeCode = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);

			var landedCostInput = (LandedCosting.ILandCostInput)((IBusinessObjectCollection)landedCostingHeader["CostInputs"]).AddNew();
			landedCostInput.LI_ParentID = containerBO.PK;
			landedCostInput.LI_ParentTableCode = containerBO.TablePrefix;
			landedCostInput.LI_AC_ChargeCode = chargeCode.PK;
			landedCostInput.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.NewZealand;
			landedCostInput.LI_LandedCostGroup = 1;
			landedCostInput.LI_ChargeDescription = "Charge Container";
			landedCostInput.LI_DistributeCostBy = CostDistributionMechanismList.Codes.ActualVolume;
			landedCostInput.LI_CostAmount = 1500m;
			landedCostInput.LI_ServiceExRate = 0.8734m;

			var throwAway = declarationBO.ImporterDeliveryAddress;
			throwAway = declarationBO.SupplierPickupAddress;
			IMergeDataObjectWriter writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declarationBO) { PurposeCode = purposeCode }));
			var declarationData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			declarationData.SetContainerCollection(() => new DataObjectList<Container>(new[] { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT1" } }));
			writer.MergeData(declarationData, declarationBO);
			AssertContents(declarationData, null, null, GetCodeDescriptionPair(Core.Constants.ContainerModes.Containerised, "Containerized"), 1,
				"FUNNY GOODS", GetCodeDescriptionPair(JobMessageTypeList.Codes.Import, JobMessageTypeList.Descriptions.Import), GetCodeDescriptionPair("ST1", null), 100, GetCodeDescriptionPair(Core.Constants.PkgUnit.Piece, "Piece"),
				10.50m, GetCodeDescriptionPair(Core.Constants.Volume.CubicFeet, "Cubic Feet"), 1506.682m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(Core.Constants.TransportModes.Sea, "Sea Freight"),
				GetCodeDescriptionPair(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), GetCodeDescriptionPair(SeaLocalPort2.RL_Code, SeaLocalPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName), GetCodeDescriptionPair(SeaForeignPort2.RL_Code, SeaForeignPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort3.RL_Code, SeaForeignPort3.RL_PortName),
				"APL EMERALD", "V23W", GetCodeDescriptionPair(GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), GetCodeDescriptionPair("", null), ZBool.True,
				GetCodeDescriptionPair("EFT", null), GetCodeDescriptionPair(OrgConstants.MergeInvoiceLines.Tariff, "Tariff"), GetCodeDescriptionPair("OS1", null), GetCodeDescriptionPair("MS1", null), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.AwaitingFDACorrection, null),
				GetCodeDescriptionPair("CC1", null), 112, 307, GetCodeDescriptionPair("WR1", null), "7819369",
				GetCodeDescriptionPair("SP", "Spare parts for the vessel/aircraft"), "AGREF123", "OWN324", "F234", GetCodeDescriptionPair(PaymentPartyCodeDescriptionList.Codes.Broker, PaymentPartyCodeDescriptionList.Descriptions.Broker), GetCodeDescriptionPair(MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, MasterFiles.Business.Customs.PaidByCodeList.Descriptions.BRK),
				GetCodeDescriptionPair("FOB", "Free On Board"), 789.012m, GetCodeDescriptionPair(ScreeningStatusesList.Codes.Unknown, "Unknown"), GetCodeDescriptionPair("AB", null));

			AssertNotNull("declarationData.AdditionalBillCollection", declarationData.AdditionalBillCollection);
			AssertEquals("declarationData.AdditionalBillCollection.Count", 2, declarationData.AdditionalBillCollection.Count);
			AssertContents(declarationData.AdditionalBillCollection[0], "MB1", GetCodeDescriptionPair(WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master), new ZDateTime(2011, 5, 20), null, true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[1], "MB1HB1", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), new ZDateTime(2011, 5, 21), "MB1", true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));

			AssertNotNull("declarationData.ContainerCollection", declarationData.ContainerCollection);
			AssertEquals("declarationData.ContainerCollection.Count", 1, declarationData.ContainerCollection.Count);
			var containerData = declarationData.ContainerCollection[0];
			AssertEquals("containerData.ContainerNumber", "CONT1", containerData.ContainerNumber);
			AssertNotEquals("containerData.Seal", containerBO.CO_Seal, containerData.Seal);
			AssertNotNull("containerData.TransportLogisticsCostCollection", containerData.TransportLogisticsCostCollection);
			AssertTransportLogisticsCostContents(containerData.TransportLogisticsCostCollection[0], CodeDescriptionPairForTesting.New(chargeCode.AC_Code, chargeCode.AC_Desc), "Charge Container", 1500m, CodeDescriptionPairForTesting.New(Core.Constants.CurrencyCodes.NewZealand, "New Zealand Dollar"), CodeDescriptionPairForTesting.New(CostDistributionMechanismList.Codes.ActualVolume, CostDistributionMechanismList.Descriptions.ActualVolume), CodeDescriptionPairForTesting.New("1", "Origin Charges"), 0.8734m);
			AssertNull("declarationData.PackingLineCollection should be null since is not populate in Shipment", declarationData.PackingLineCollection);
			AssertNull("declarationData.TransportLegCollection should not be populated as it should come from Shipment", declarationData.TransportLegCollection);
			AssertNull("declarationData.NoteCollection should not be populated as it should come from Shipment", declarationData.NoteCollection);

			AssertNotNull("declarationData.OrganizationAddressCollection", declarationData.OrganizationAddressCollection);
			AssertEquals("declarationData.OrganizationAddressCollection.Count", 6, declarationData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_CRAHOLSYD("SupplierDocumentaryAddress", declarationData.OrganizationAddressCollection[0], "SupplierDocumentaryAddress", true);
			AssertOrganizationBO_WUFSHIJNB("ImporterDocumentaryAddress", declarationData.OrganizationAddressCollection[1], "ImporterDocumentaryAddress", true);
			AssertOrganizationBO_WUFSHIJNB("Supplier", declarationData.OrganizationAddressCollection[2], "Supplier");
			AssertOrganizationBO_CRAHOLSYD("Importer", declarationData.OrganizationAddressCollection[3], "Importer");
			AssertOrganizationBO_WUFSHIJNB("Forwarder", declarationData.OrganizationAddressCollection[4], "Forwarder");
			AssertOrganizationBO_CRAHOLSYD("ShippingLine", declarationData.OrganizationAddressCollection[5], "ShippingLine");

			AssertNotNull("declarationData.CommercialInfo", declarationData.CommercialInfo);
			AssertNotNull("declarationData.ChargeCollection", declarationData.CommercialInfo.CommercialChargeCollection);
			AssertEquals("declarationData.CommercialInfo.ChargeCollection.Count", 2, declarationData.CommercialInfo.CommercialChargeCollection.Count);
			var localCurrency = GetCodeDescriptionPair(declarationBO.LocalCurrency.RX_Code, declarationBO.LocalCurrency.RX_Desc);
			AssertContents(declarationData.CommercialInfo.CommercialChargeCollection[0], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(declarationData.CommercialInfo.CommercialChargeCollection[1], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertNotNull("declarationData.InvoiceCollection", declarationData.CommercialInfo.CommercialInvoiceCollection);
			AssertEquals("declarationData.CommercialInfo.InvoiceCollection.Count", 1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);
			declarationBO.ResumeApportionment();

			var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection[0];
			AssertContentsWithLookingAtChildren(invoiceData, "INV3243", AssertOrganizationBO_WUFSHIJNB, AssertOrganizationBO_CRAHOLSYD, 3420.34m, GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), new ZDateTime(2011, 4, 3), GetCodeDescriptionPair(Core.Constants.IncoTerms.FreeOnBoard, "Free On Board"), 14.72m, GetCodeDescriptionPair(Core.Constants.Volume.CubicMetres, "Cubic Meters"), 2.53m, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), 11.11m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 1.25m, 1.50m, "P12345", 1500m, 1.75m, new ZDateTime(2011, 3, 3), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearElectronicInvoiceOriginal, null), 10m);
			AssertNotNull("invoiceData.CommercialChargeCollection", invoiceData.CommercialChargeCollection);
			AssertEquals("invoiceData.CommercialChargeCollection.Count", 4, invoiceData.CommercialChargeCollection.Count);
			AssertContents(invoiceData.CommercialChargeCollection[0], ZBool.False, 10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceData.CommercialChargeCollection[1], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceData.CommercialChargeCollection[2], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceData.CommercialChargeCollection[3], ZBool.False, 100m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));

			AssertNotNull("Precondition: invoiceData.CommercialInvoiceLineCollection", invoiceData.CommercialInvoiceLineCollection);
			AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 1, invoiceData.CommercialInvoiceLineCollection.Count);
			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
			AssertContents(invoiceLineData, 1, "1010101010", "1010.10.10 10", "GOODS", 1040.50m, GetCodeDescriptionPair(Core.Constants.PkgUnit.Box, "Box"), 4162551.46m, 4000.53m, "PART12", 3.2m, GetCodeDescriptionPair(Core.Constants.Volume.CubicYards, "Cubic Yards"), 202.92m, GetCodeDescriptionPair(Core.Constants.Weight.Hectograms, "Hectograms"), "ORDER1", 1.555m, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), 10m, GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null), GetCodeDescriptionPair(Core.Constants.CountryCodes.Australia, "Australia"), GetCodeDescriptionPair(CommodityCode1.RH_Code, CommodityCode1.RH_Description), GetCodeDescriptionPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk), "MK0001");
			AssertNotNull("invoiceLineData.CommercialChargeCollection", invoiceLineData.CommercialChargeCollection);
			AssertEquals("invoiceLineData.CommercialChargeCollection.Count", 4, invoiceLineData.CommercialChargeCollection.Count);
			AssertContents(invoiceLineData.CommercialChargeCollection[0], ZBool.False, 10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoiceLineData.CommercialChargeCollection[1], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(invoiceLineData.CommercialChargeCollection[2], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(invoiceLineData.CommercialChargeCollection[3], ZBool.False, 100m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));

			AssertNotNull("declarationData.EntryNumberCollection", declarationData.EntryNumberCollection);
			AssertEquals("declarationData.EntryNumberCollection.Count", 1, declarationData.EntryNumberCollection.Count);
			AssertContents(declarationData.EntryNumberCollection[0], GetCodeDescriptionPair(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Desc), true, "IMPDEC", new ZDateTime(2011, 7, 5), "IMP123", GetCodeDescriptionPair(JobMessageTypeList.Codes.Import, null));

			AssertNotNull("declarationData.AdditionalReferenceCollection", declarationData.AdditionalReferenceCollection);
			AssertEquals("declarationData.AdditionalReferenceCollection.Count", 1, declarationData.AdditionalReferenceCollection.Count);
			AssertContents(declarationData.AdditionalReferenceCollection[0], "ITDEC", new ZDateTime(2011, 6, 3), "IT123", GetCodeDescriptionPair(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, UnitedStatesAdditionalReferenceNumberTypes.Descriptions.IT));

			AssertNotNull("declarationData.DateCollection", declarationData.DateCollection);
			AssertEquals("declarationData.DateCollection.Count", 10, declarationData.DateCollection.Count);
			AssertContents(declarationData.DateCollection[0], DateType.BillIssued, new ZDateTime(2011, 5, 21), ZBool.False);
			AssertContents(declarationData.DateCollection[1], DateType.Departure, new ZDateTime(2011, 7, 17), ZBool.True);
			AssertContents(declarationData.DateCollection[2], DateType.LoadingDate, new ZDateTime(2011, 6, 12), ZBool.False);
			AssertContents(declarationData.DateCollection[3], DateType.FirstArrivalInCountry, new ZDateTime(2011, 6, 11), ZBool.False);
			AssertContents(declarationData.DateCollection[4], DateType.DischargeDate, new ZDateTime(2011, 6, 10), ZBool.False);
			AssertContents(declarationData.DateCollection[5], DateType.Arrival, new ZDateTime(2011, 7, 13), ZBool.True);
			AssertContents(declarationData.DateCollection[6], DateType.EntrySubmitted, new ZDateTime(2011, 7, 14), ZBool.False);
			AssertContents(declarationData.DateCollection[7], DateType.EntryAuthorisation, new ZDateTime(2011, 7, 15), ZBool.False);
			AssertContents(declarationData.DateCollection[8], DateType.WarehouseRelease, new ZDateTime(2011, 7, 16), ZBool.False);
			AssertContents(declarationData.DateCollection[9], DateType.EntryDate, new ZDateTime(2011, 7, 18), ZBool.False);

			AssertNotNull("declarationData.EntryHeaderCollection", declarationData.EntryHeaderCollection);
			AssertEquals("declarationData.EntryHeaderCollection.Count", 1, declarationData.EntryHeaderCollection.Count);
			var entryHeaderData = declarationData.EntryHeaderCollection[0];
			AssertContentsWithLookingAtChildren(entryHeaderData, new ZDateTime(2011, 2, 3), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, null), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, null), new ZDateTime(2011, 2, 2), 1404.24m, GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export), "BDG34332", new ZDate(2011, 2, 7));

			AssertNotNull("entryHeaderData.EntryLineCollection", entryHeaderData.EntryLineCollection);
			AssertEquals("entryHeaderData.EntryLineCollection.Count", 1, entryHeaderData.EntryLineCollection.Count);
			var entryLineData = entryHeaderData.EntryLineCollection[0];
			AssertContentsWithLookingAtChildren(entryLineData, "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), 72.23m, GetCodeDescriptionPair(EntryLineStatusList.Codes.Active, EntryLineStatusList.Descriptions.Active));

			AssertNotNull("entryHeaderData.EntryHeaderChargeCollection", entryHeaderData.EntryHeaderChargeCollection);
			AssertEquals("entryHeaderData.EntryHeaderChargeCollection.Count", 1, entryHeaderData.EntryHeaderChargeCollection.Count);
			var entryHeaderChargeData = entryHeaderData.EntryHeaderChargeCollection[0];
			AssertContents(entryHeaderChargeData, 236.45m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, null));

			AssertNotNull("entryHeaderData.EntryNumberCollection", entryHeaderData.EntryNumberCollection);
			AssertEquals("entryHeaderData.EntryNumberCollection.Count", 1, entryHeaderData.EntryNumberCollection.Count);
			var entryNumberData = entryHeaderData.EntryNumberCollection[0];
			AssertContents(entryNumberData, false, "REFERENCE", "CE00001", GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export));

			AssertNotNull("entryHeaderData.EntryLineCollection", entryHeaderData.EntryLineCollection);
			AssertEquals("entryHeaderData.EntryLineCollection.Count", 1, entryHeaderData.EntryLineCollection.Count);
			entryLineData = entryHeaderData.EntryLineCollection[0];
			AssertContentsWithLookingAtChildren(entryLineData, "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), 72.23m, GetCodeDescriptionPair(EntryLineStatusList.Codes.Active, EntryLineStatusList.Descriptions.Active));

			AssertNotNull("Precondition: entryLineData.EntryLineChargeCollection", entryLineData.EntryLineChargeCollection);
			AssertEquals("entryLineData.EntryLineChargeCollection.Count", 1, entryLineData.EntryLineChargeCollection.Count);
			AssertContents(entryLineData.EntryLineChargeCollection[0], 102.23m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.Blueberry, null));
		}

		void AssertIMergeDataObjectWriter_MergeData_UseShipmentData_WithPopulatedShipment(bool useBrokerageDataFirststring, string purposeCode = null)
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, useBrokerageDataFirststring);
				var declarationBO = CreateDeclarationForMerge();
				var throwAway = declarationBO.ImporterDeliveryAddress;
				throwAway = declarationBO.SupplierPickupAddress;
				IMergeDataObjectWriter writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declarationBO) { PurposeCode = purposeCode }));
				var declarationData = CreatePopulatedShipmentDataForMerge();
				writer.MergeData(declarationData, declarationBO);
				AssertContents(declarationData, "SB3423", GetCodeDescriptionPair(WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse), GetCodeDescriptionPair(Core.Constants.ContainerModes.Containerised, "CONTAINER MODE"), 2,
					"SHIPMENT GOODS", GetCodeDescriptionPair("SHP", "SHIPMENT"), GetCodeDescriptionPair("SBT", "SHIPMENT SUB TYPE"), 123, GetCodeDescriptionPair("P1", "P1 DESCRIPTION"),
					13.32m, GetCodeDescriptionPair("V1", "V1 DESRIPTION"), 1984.43m, GetCodeDescriptionPair("W1", "W1 DESCRIPTION"), GetCodeDescriptionPair("T12", "T12 DESCRIPTION"),
					GetCodeDescriptionPair("ORG12", "ORG12 NAME"), GetCodeDescriptionPair("LOAD2", "LOAD2 NAME"), GetCodeDescriptionPair("1ARV2", "1ARV2 NAME"), GetCodeDescriptionPair("DIS43", "DIS43 NAME"), GetCodeDescriptionPair("DES98", "DES98 NAME"),
					"SHP VESSEL", "V531", GetCodeDescriptionPair("BR1", "BR1 NAME"), GetCodeDescriptionPair("SL1", "SL1 DESCRIPTION"), ZBool.False,
					GetCodeDescriptionPair("EM1", "EM1 DESCRIPTION"), GetCodeDescriptionPair("MB4", "MB4 DESCRIPTION"), GetCodeDescriptionPair("OS8", "OS8 DESCRIPTION"), GetCodeDescriptionPair("MS9", "MS9 DESCRIPTION"), GetCodeDescriptionPair("ES2", "ES2 DESCRIPTION"),
					GetCodeDescriptionPair("CS3", "CS3 DESCRIPTION"), 435, 306, GetCodeDescriptionPair("WS4", "WS4 DESCRIPTION"), "LYD2342",
					GetCodeDescriptionPair("EGT", "EGT DESCRIPTION"), "SHP AGT REF", "SHP OWN REF", "32", GetCodeDescriptionPair("PM4", "PM4 DESCRIPTION"), GetCodeDescriptionPair(MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, MasterFiles.Business.Customs.PaidByCodeList.Descriptions.BRK),
					GetCodeDescriptionPair("SIT", "SIT DESCRIPTION"), 1365.84m, GetCodeDescriptionPair("SS5", "SS5 DESCRIPTION"), GetCodeDescriptionPair("AC", "TEST AC Origin Territory"));
				declarationBO.ResumeApportionment();

				AssertNotNull("declarationData.AdditionalBillCollection", declarationData.AdditionalBillCollection);
				AssertEquals("declarationData.AdditionalBillCollection.Count", 2, declarationData.AdditionalBillCollection.Count);
				AssertContents(declarationData.AdditionalBillCollection[0], "MB1", GetCodeDescriptionPair(WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master), new ZDateTime(2011, 5, 20), null, true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(declarationData.AdditionalBillCollection[1], "MB1HB1", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), new ZDateTime(2011, 5, 21), "MB1", true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));

				AssertNotNull("declarationData.ContainerCollection", declarationData.ContainerCollection);
				AssertEquals("declarationData.ContainerCollection.Count", 1, declarationData.ContainerCollection.Count);
				var containerData = declarationData.ContainerCollection[0];
				AssertContents(containerData, "CONT1", 3492.5m, GetCodeDescriptionPair("WG4", "WG4 DESCRIPTION"), "S123", "S343", GetCodeDescriptionPair("FCL", "FULL LOAD"), 120m, 130m, 140m, GetCodeDescriptionPair("CM1", "CM1 DESC"), GetCodeDescriptionPair("S3", "S3 DESC"));
				AssertNull("containerData.CustomizedFieldCollection", containerData.CustomizedFieldCollection);

				AssertNotNull("declarationData.PackingLineCollection", declarationData.PackingLineCollection);
				AssertEquals("declarationData.PackingLineCollection.Count", 1, declarationData.PackingLineCollection.Count);
				AssertContents(declarationData.PackingLineCollection[0], "CNT3459", "HB38987", GetCodeDescriptionPair("GDH", "GDH DESCRIPTION"), "SHP MARKS", 101, GetCodeDescriptionPair("P3", "P3 DESC"), 99, 88, "SHP SYMBOL");

				AssertNotNull("declarationData.TransportLegCollection", declarationData.TransportLegCollection);
				AssertEquals("declarationData.TransportLegCollection.Count", 1, declarationData.TransportLegCollection.Count);
				AssertContents(declarationData.TransportLegCollection[0], TransportMode.Sea, "VESSEL 3234", "V598", GetCodeDescriptionPair("LOAD8", "LOAD8 NAME"), GetCodeDescriptionPair("DISC9", "DISC9 NAME"));

				AssertNotNull("declarationData.NoteCollection", declarationData.NoteCollection);
				AssertEquals("declarationData.NoteCollection.Count", 1, declarationData.NoteCollection.Count);
				AssertContents(declarationData.NoteCollection[0], true, "SHIP DESC", "WHAT IS THIS");

				AssertNotNull("declarationData.OrganizationAddressCollection", declarationData.OrganizationAddressCollection);
				AssertEquals("declarationData.OrganizationAddressCollection.Count", 7, declarationData.OrganizationAddressCollection.Count);
				AssertAddress("LocalClient", declarationData.OrganizationAddressCollection[0], "LocalClient", "BOBORGCODE", "BOB THE BUILDER", null, null, null, null, null, null, null, null, null, null, null, null);
				AssertAddress("SupplierDocumentaryAddress", declarationData.OrganizationAddressCollection[1], "SupplierDocumentaryAddress", "WENORGCODE", "WENDY THE DESTROYER", null, null, null, null, null, null, null, null, null, null, null, null);
				AssertOrganizationBO_WUFSHIJNB("ImporterDocumentaryAddress", declarationData.OrganizationAddressCollection[2], "ImporterDocumentaryAddress", true);
				AssertOrganizationBO_WUFSHIJNB("Supplier", declarationData.OrganizationAddressCollection[3], "Supplier");
				AssertOrganizationBO_CRAHOLSYD("Importer", declarationData.OrganizationAddressCollection[4], "Importer");
				AssertOrganizationBO_WUFSHIJNB("Forwarder", declarationData.OrganizationAddressCollection[5], "Forwarder");
				AssertOrganizationBO_CRAHOLSYD("ShippingLine", declarationData.OrganizationAddressCollection[6], "ShippingLine");

				AssertNotNull("declarationData.CommercialInfo", declarationData.CommercialInfo);
				AssertNotNull("declarationData.ChargeCollection", declarationData.CommercialInfo.CommercialChargeCollection);
				AssertEquals("declarationData.CommercialInfo.ChargeCollection.Count", 2, declarationData.CommercialInfo.CommercialChargeCollection.Count);
				var localCurrency = GetCodeDescriptionPair(declarationBO.LocalCurrency.RX_Code, declarationBO.LocalCurrency.RX_Desc);
				AssertContents(declarationData.CommercialInfo.CommercialChargeCollection[0], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertContents(declarationData.CommercialInfo.CommercialChargeCollection[1], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertNotNull("declarationData.InvoiceCollection", declarationData.CommercialInfo.CommercialInvoiceCollection);
				AssertEquals("declarationData.CommercialInfo.InvoiceCollection.Count", 1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);
				declarationBO.ResumeApportionment();
				var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection[0];
				AssertContentsWithLookingAtChildren(invoiceData, "INV3243", AssertOrganizationBO_WUFSHIJNB, AssertOrganizationBO_CRAHOLSYD, 3420.34m, GetCodeDescriptionPair(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), new ZDateTime(2011, 4, 3), GetCodeDescriptionPair(Core.Constants.IncoTerms.FreeOnBoard, "Free On Board"), 14.72m, GetCodeDescriptionPair(Core.Constants.Volume.CubicMetres, "Cubic Meters"), 2.53m, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), 11.11m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(Common.ChargeExchangeRateTypeList.Codes.FixedRate, Common.ChargeExchangeRateTypeList.Descriptions.FixedRate), 1.25m, 1.50m, "P12345", 1500m, 1.75m, new ZDateTime(2011, 3, 3), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearElectronicInvoiceOriginal, null), 10m);
				AssertNotNull("invoiceData.CommercialChargeCollection", invoiceData.CommercialChargeCollection);
				AssertEquals("invoiceData.CommercialChargeCollection.Count", 4, invoiceData.CommercialChargeCollection.Count);
				AssertContents(invoiceData.CommercialChargeCollection[0], ZBool.False, 10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceData.CommercialChargeCollection[1], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceData.CommercialChargeCollection[2], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceData.CommercialChargeCollection[3], ZBool.False, 100m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));

				AssertNotNull("Precondition: invoiceData.CommercialInvoiceLineCollection", invoiceData.CommercialInvoiceLineCollection);
				AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 1, invoiceData.CommercialInvoiceLineCollection.Count);
				var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
				AssertContents(invoiceLineData, 1, "1010101010", "1010.10.10 10", "GOODS", 1040.50m, GetCodeDescriptionPair(Core.Constants.PkgUnit.Box, "Box"), 4162551.46m, 4000.53m, "PART12", 3.2m, GetCodeDescriptionPair(Core.Constants.Volume.CubicYards, "Cubic Yards"), 202.92m, GetCodeDescriptionPair(Core.Constants.Weight.Hectograms, "Hectograms"), "ORDER1", 1.555m, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), 10m, GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, null), GetCodeDescriptionPair(Core.Constants.CountryCodes.Australia, "Australia"), GetCodeDescriptionPair(CommodityCode1.RH_Code, CommodityCode1.RH_Description), GetCodeDescriptionPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk), "MK0001");
				AssertNotNull("invoiceLineData.CommercialChargeCollection", invoiceLineData.CommercialChargeCollection);
				AssertEquals("invoiceLineData.CommercialChargeCollection.Count", 4, invoiceLineData.CommercialChargeCollection.Count);
				AssertContents(invoiceLineData.CommercialChargeCollection[0], ZBool.False, 10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
				AssertContents(invoiceLineData.CommercialChargeCollection[1], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertContents(invoiceLineData.CommercialChargeCollection[2], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
				AssertContents(invoiceLineData.CommercialChargeCollection[3], ZBool.False, 100m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));

				AssertNotNull("declarationData.EntryNumberCollection", declarationData.EntryNumberCollection);
				AssertEquals("declarationData.EntryNumberCollection.Count", 1, declarationData.EntryNumberCollection.Count);
				AssertContents(declarationData.EntryNumberCollection[0], GetCodeDescriptionPair(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Desc), false, "IMPDEC", new ZDateTime(2012, 4, 27), "IMP123", GetCodeDescriptionPair(JobMessageTypeList.Codes.Import, null));

				AssertNotNull("declarationData.AdditionalReferenceCollection", declarationData.AdditionalReferenceCollection);
				AssertEquals("declarationData.AdditionalReferenceCollection.Count", 1, declarationData.AdditionalReferenceCollection.Count);
				AssertContents(declarationData.AdditionalReferenceCollection[0], "HB349", new ZDateTime(2012, 7, 4), "IT123", GetCodeDescriptionPair(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, UnitedStatesAdditionalReferenceNumberTypes.Descriptions.IT));

				AssertNotNull("declarationData.DateCollection", declarationData.DateCollection);
				AssertEquals("declarationData.DateCollection.Count", 10, declarationData.DateCollection.Count);
				AssertContents(declarationData.DateCollection[0], DateType.Departure, new ZDateTime(2012, 7, 14), ZBool.True);
				AssertContents(declarationData.DateCollection[1], DateType.LoadingDate, new ZDateTime(2012, 7, 13), ZBool.False);
				AssertContents(declarationData.DateCollection[2], DateType.DischargeDate, new ZDateTime(2012, 8, 12), ZBool.False);
				AssertContents(declarationData.DateCollection[3], DateType.Arrival, new ZDateTime(2012, 8, 10), ZBool.True);
				AssertContents(declarationData.DateCollection[4], DateType.BillIssued, new ZDateTime(2011, 5, 21), ZBool.False);
				AssertContents(declarationData.DateCollection[5], DateType.FirstArrivalInCountry, new ZDateTime(2011, 6, 11), ZBool.False);
				AssertContents(declarationData.DateCollection[6], DateType.EntrySubmitted, new ZDateTime(2011, 7, 14), ZBool.False);
				AssertContents(declarationData.DateCollection[7], DateType.EntryAuthorisation, new ZDateTime(2011, 7, 15), ZBool.False);
				AssertContents(declarationData.DateCollection[8], DateType.WarehouseRelease, new ZDateTime(2011, 7, 16), ZBool.False);
				AssertContents(declarationData.DateCollection[9], DateType.EntryDate, new ZDateTime(2011, 7, 18), ZBool.False);

				AssertNotNull("declarationData.EntryHeaderCollection", declarationData.EntryHeaderCollection);
				AssertEquals("declarationData.EntryHeaderCollection.Count", 1, declarationData.EntryHeaderCollection.Count);
				var entryHeaderData = declarationData.EntryHeaderCollection[0];
				AssertContentsWithLookingAtChildren(entryHeaderData, new ZDateTime(2011, 2, 3), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, null), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, null), new ZDateTime(2011, 2, 2), 1404.24m, GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export), "BDG34332", new ZDate(2011, 2, 7));

				AssertNotNull("entryHeaderData.EntryLineCollection", entryHeaderData.EntryLineCollection);
				AssertEquals("entryHeaderData.EntryLineCollection.Count", 1, entryHeaderData.EntryLineCollection.Count);
				var entryLineData = entryHeaderData.EntryLineCollection[0];
				AssertContentsWithLookingAtChildren(entryLineData, "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), 72.23m, GetCodeDescriptionPair(EntryLineStatusList.Codes.Active, EntryLineStatusList.Descriptions.Active));

				AssertNotNull("entryHeaderData.EntryHeaderChargeCollection", entryHeaderData.EntryHeaderChargeCollection);
				AssertEquals("entryHeaderData.EntryHeaderChargeCollection.Count", 1, entryHeaderData.EntryHeaderChargeCollection.Count);
				var entryHeaderChargeData = entryHeaderData.EntryHeaderChargeCollection[0];
				AssertContents(entryHeaderChargeData, 236.45m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, null));

				AssertNotNull("entryHeaderData.EntryNumberCollection", entryHeaderData.EntryNumberCollection);
				AssertEquals("entryHeaderData.EntryNumberCollection.Count", 1, entryHeaderData.EntryNumberCollection.Count);
				var entryNumberData = entryHeaderData.EntryNumberCollection[0];
				AssertContents(entryNumberData, false, "REFERENCE", "CE00001", GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export));

				AssertNotNull("entryHeaderData.EntryLineCollection", entryHeaderData.EntryLineCollection);
				AssertEquals("entryHeaderData.EntryLineCollection.Count", 1, entryHeaderData.EntryLineCollection.Count);
				entryLineData = entryHeaderData.EntryLineCollection[0];
				AssertContentsWithLookingAtChildren(entryLineData, "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), 72.23m, GetCodeDescriptionPair(EntryLineStatusList.Codes.Active, EntryLineStatusList.Descriptions.Active));

				AssertNotNull("Precondition: entryLineData.EntryLineChargeCollection", entryLineData.EntryLineChargeCollection);
				AssertEquals("entryLineData.EntryLineChargeCollection.Count", 1, entryLineData.EntryLineChargeCollection.Count);
				AssertContents(entryLineData.EntryLineChargeCollection[0], 102.23m, GetCodeDescriptionPair(Core.Constants.USCustoms.FeeCodes.Blueberry, null));
			}
		}

		public void TestCustomFieldsOnDeclarationPluggedIntoShipmentAreExported()
		{
			eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			CustomFieldsOnDeclarationPluggedIntoShipmentAreExported(true);

			eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			CustomFieldsOnDeclarationPluggedIntoShipmentAreExported(false);
		}

		public void TestCustomFieldsOnDeclarationAreExported()
		{
			eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			CustomFieldsOnDeclarationAreExported();

			eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			CustomFieldsOnDeclarationAreExported();
		}

		public void TestDeclarationMappings()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var declarationMock = CreateJobDeclarationMock();
				var declaration = SetupJobDeclaration(declarationMock.Object);
				Factory.SaveForTesting();
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
				var declarationData = writer.GetDataObject(declaration);

				declaration.ResumeApportionment();

				AssertContents(declarationData);
			}
		}

		public void TestNoteCollectionMapping()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RS_NKServiceLevel = "STD";
			var declarationIsPluggedIntoShipment = Factory.New<BaseJobDeclaration>();
			declarationIsPluggedIntoShipment.JE_JS = shipment.PK;
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "Shipment Marks And Numbers test");

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declarationIsPluggedIntoShipment)) { FilteredDataContextType = DataContextType.CustomsDeclaration });
			var declarationData = writer.GetDataObject(declarationIsPluggedIntoShipment);
			AssertContents(declarationData.NoteCollection[0], false, "Marks & Numbers", "Shipment Marks And Numbers test");

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "Declaration Marks And Numbers test");
			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)) { FilteredDataContextType = DataContextType.CustomsDeclaration });
			declarationData = writer.GetDataObject(declaration);
			AssertContents(declarationData.NoteCollection[0], false, "Marks & Numbers", "Declaration Marks And Numbers test");
		}

		public void TestContainerInvoiceLinePivotMapping()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "OB1023232";
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CONT3";
			var masterBill = declaration.PrimaryMasterBill;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "IVN325432";
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Description = "BOB 1";
			var pivots = invoiceLine1.ContainersForInvoiceLinesForBindingOnly;
			var pivot = pivots.FindByContainer(container1);
			pivot.IsForInvoiceLine = true;
			pivot.GrossWeightInKG = 1000m;
			pivot.NetWeightInKG = 900m;
			pivot.SplitValue = 1500m;
			pivot.PackQty = 110;
			pivot = pivots.FindByContainer(container3);
			pivot.IsForInvoiceLine = true;
			pivot.GrossWeightInKG = 3000m;
			pivot.NetWeightInKG = 2700m;
			pivot.SplitValue = 3300m;
			pivot.PackQty = 330;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Description = "BOB 2";
			pivots = invoiceLine2.ContainersForInvoiceLinesForBindingOnly;
			pivot = pivots.FindByContainer(container1);
			pivot.IsForInvoiceLine = true;
			pivot.GrossWeightInKG = 100m;
			pivot.NetWeightInKG = 90m;
			pivot.SplitValue = 150m;
			pivot.PackQty = 11;

			Factory.SaveForTesting();
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertNotNull("declarationData.CommercialInfo", declarationData.CommercialInfo);
			AssertNotNull("declarationData.CommercialInfo.CommercialInvoiceCollection", declarationData.CommercialInfo.CommercialInvoiceCollection);
			AssertEquals("declarationData.CommercialInfo.CommercialInvoiceCollection.Count", 1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection[0];
			AssertNotNull("invoiceData.CommercialInvoiceLineCollection", invoiceData.CommercialInvoiceLineCollection);
			AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 2, invoiceData.CommercialInvoiceLineCollection.Count);
			var invoiceLine1Data = invoiceData.CommercialInvoiceLineCollection.FirstOrDefault(x => x.Description.GetValueOrDefault() == "BOB 1");
			var invoiceLine2Data = invoiceData.CommercialInvoiceLineCollection.FirstOrDefault(x => x.Description.GetValueOrDefault() == "BOB 2");
			if (invoiceLine1Data.Link.GetValueOrDefault() == 2)
			{
				AssertEquals("invoiceLine1Data.Link", 2, invoiceLine1Data.Link);
				AssertEquals("invoiceLine2Data.Link", 1, invoiceLine2Data.Link);
			}
			else
			{
				AssertEquals("invoiceLine1Data.Link", 1, invoiceLine1Data.Link);
				AssertEquals("invoiceLine2Data.Link", 2, invoiceLine2Data.Link);
			}
			AssertNotNull("declarationData.PackingLineCollection", declarationData.PackingLineCollection);
			AssertEquals("declarationData.PackingLineCollection.Count", 3, declarationData.PackingLineCollection.Count);
			var packingLine1Data = declarationData.PackingLineCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT1");
			AssertNotNull("packingLine1Data.PackedItemCollection", packingLine1Data.PackedItemCollection);
			AssertEquals("packingLine1Data.PackedItemCollection.Count", 2, packingLine1Data.PackedItemCollection.Count);
			AssertContents(packingLine1Data.PackedItemCollection.FirstOrDefault(x => x.CommercialInvoiceLineLink == invoiceLine1Data.Link), 1000m, 900m, 1500m, 110m);
			AssertContents(packingLine1Data.PackedItemCollection.FirstOrDefault(x => x.CommercialInvoiceLineLink == invoiceLine2Data.Link), 100m, 90m, 150m, 11m);
			var packingLine2Data = declarationData.PackingLineCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT2");
			var packedItems2 = packingLine2Data.PackedItemCollection;
			Assert("packingLine2Data.PackedItemCollection - null or empty", packedItems2 == null || packedItems2.Count == 0);
			var packingLine3Data = declarationData.PackingLineCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT3");
			AssertNotNull("packingLine3Data.PackedItemCollection", packingLine3Data.PackedItemCollection);
			AssertEquals("packingLine3Data.PackedItemCollection.Count", 1, packingLine3Data.PackedItemCollection.Count);
			AssertContents(packingLine3Data.PackedItemCollection.FirstOrDefault(x => x.CommercialInvoiceLineLink == invoiceLine1Data.Link), 3000m, 2700, 3300m, 330m);
		}

		public void TestPopulateContainerFromShipmentIfDeclarationWithNoContainer()
		{
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org2.PK;
			shipment.JS_HouseBill = "HB9863521";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var container = shipment.Consols.AddNew().Containers.AddNew();
			container.AddPackLine(shipment.OuterPackLines.AddNew());
			container.JC_ContainerNum = "123456";
			AssertEquals(1, shipment.Containers.Count());

			var declarationOnShipment = Factory.New<BaseJobDeclaration>();
			declarationOnShipment.JE_JS = shipment.PK;
			declarationOnShipment.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationOnShipment.ShipmentSynchroniser.Synchronise(true);

			AssertEquals(0, declarationOnShipment.CusContainers.Count);

			var org = declarationOnShipment.ConfigOrg;
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomAttribute1, "STRING1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomDate1, "DATE1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomDecimal1, "DECIMAL1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomFlag1, "FLAG1");

			IMergeDataObjectWriter writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declarationOnShipment)));
			var declarationData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			writer.MergeData(declarationData, declarationOnShipment);

			AssertEquals(1, declarationData.ContainerCollection.Count);
			AssertEquals("123456", declarationData.ContainerCollection[0].ContainerNumber);
		}

		public void TestCountrySpecificEntryNumberMapping()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = USBranch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryNumber = SetupCusEntryNumber(Factory.New<CusEntryNumber>(), "IMP123", "ENS", "IMPDEC", true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2011, 7, 5));
			entryNumber.Parent = declaration;
			Factory.SaveForTesting();
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);
			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertNotNull("declarationData.EntryNumberCollection", declarationData.EntryNumberCollection);
			AssertEquals("declarationData.EntryNumberCollection.Count", 1, declarationData.EntryNumberCollection.Count);
			AssertContents(declarationData.EntryNumberCollection[0], GetCodeDescriptionPair(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Desc), true, "IMPDEC", new ZDateTime(2011, 7, 5), "IMP123", GetCodeDescriptionPair("ENS", "Entry Summary"));
		}

		public void TestPackingDetailForAirJob()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = SetupJobDeclaration(declarationMock.Object);
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "QF133";
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertContents(declarationData, "MB1HB1", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), GetCodeDescriptionPair(Core.Constants.ContainerModes.Containerised, "Containerized"), 0,
				"FUNNY GOODS", GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export), GetCodeDescriptionPair("ST1", "STANDARD"), 100, GetCodeDescriptionPair(Core.Constants.PkgUnit.Piece, "Piece"),
				10.50m, GetCodeDescriptionPair(Core.Constants.Volume.CubicFeet, "Cubic Feet"), 1506.682m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(Core.Constants.TransportModes.Air, "Air Freight"),
				GetCodeDescriptionPair(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), GetCodeDescriptionPair(SeaLocalPort2.RL_Code, SeaLocalPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName), GetCodeDescriptionPair(SeaForeignPort2.RL_Code, SeaForeignPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort3.RL_Code, SeaForeignPort3.RL_PortName),
				"", "QF133", GetCodeDescriptionPair(GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), GetCodeDescriptionPair(ServiceLevel1.RS_Code, ServiceLevel1.RS_Description), ZBool.True,
				GetCodeDescriptionPair("EFT", "EFT DATA"), GetCodeDescriptionPair(OrgConstants.MergeInvoiceLines.Tariff, "Tariff"), GetCodeDescriptionPair("OS1", "OPERATIONAL STATUS 1"), GetCodeDescriptionPair("MS1", "MESSAGE STATUS 1"), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.AwaitingFDACorrection, CustomsEntryStatusList.Descriptions.AwaitingFDACorrection),
				GetCodeDescriptionPair("CC1", "CONSOLIDATED STATUS 1"), 112, 307, GetCodeDescriptionPair("WR1", "WAREHOUSERELEASE STATUS 1"), "",
				GetCodeDescriptionPair("SP", "Spare parts for the vessel/aircraft"), "AGREF123", "OWN324", "F234", GetCodeDescriptionPair(PaymentPartyCodeDescriptionList.Codes.Broker, PaymentPartyCodeDescriptionList.Descriptions.Broker), GetCodeDescriptionPair(MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, MasterFiles.Business.Customs.PaidByCodeList.Descriptions.BRK),
				GetCodeDescriptionPair("FOB", "Free On Board"), 789.012m, GetCodeDescriptionPair(ScreeningStatusesList.Codes.Matched, "Matched"), GetCodeDescriptionPair("AB", "TEST AB Origin Territory"));
			AssertNull(declarationData.PackingLineCollection);
		}

		public void TestDeclarationCusAddInfoAndCusCodeDataMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
				var declarationCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)declaration;
				Type itDocType = null;
				declarationCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USITDoc, out itDocType);
				var itDoc = (CusAddInfo)Factory.New(itDocType);
				itDoc.B7_ParentID = declaration.PK;
				itDoc.B7_ParentTableCode = declaration.TablePrefix;
				itDoc.B7_AddInfoData = USITDocAddInfoSchema.Constants.US_7512OpenArea.Substring(3) + "=AREA1234";
				Type ogaDispositionType = null;
				declarationCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USOGADisposition, out ogaDispositionType);
				var ogaDisposition = (CusAddInfo)Factory.New(ogaDispositionType);
				ogaDisposition.B7_ParentID = declaration.PK;
				ogaDisposition.B7_ParentTableCode = declaration.TablePrefix;
				ogaDisposition.B7_AddInfoData = USOGADispositionDataAddInfoSchema.Constants.US_Code.Substring(3) + "=I3";
				var declarationCusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)declaration;
				var contractNumberString = "CNN";
				Type contractNumberType = null;
				declarationCusCodeDataTypeSupporter.GetCusCodeDataTypes().TryGetValue(contractNumberString, out contractNumberType);
				var contractNumber = (CusCodeData)Factory.New(contractNumberType);
				contractNumber.Parent = declaration;
				contractNumber.CY_Code = "ABC";
				contractNumber.CY_Data = "100";
				Factory.SaveForTesting();
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));

				var declarationData = writer.GetDataObject(declaration);
				AssertEquals("declarationData.AddInfoGroupCollection.Count", 2, declarationData.AddInfoGroupCollection.Count);
				AssertContents(declarationData.AddInfoGroupCollection[0], GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.USOGADisposition, "OGA Disposition"), new List<AddInfo>(new[] { new AddInfo() { Key = USOGADispositionDataAddInfoSchema.Constants.US_Code.Substring(3), Value = "I3" } }));
				AssertContents(declarationData.AddInfoGroupCollection[1], GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.USITDoc, "IT Doc"), new List<AddInfo>(new[] { new AddInfo() { Key = USITDocAddInfoSchema.Constants.US_7512OpenArea.Substring(3), Value = "AREA1234" } }));

				AssertEquals("declarationData.CustomsReferenceCollection.Count", 1, declarationData.CustomsReferenceCollection.Count);
				AssertContents(declarationData.CustomsReferenceCollection[0], GetCodeDescriptionPair(contractNumberString, "Contract Number"), GetCodeDescriptionPair("ABC", null), "100", 0, ZBool.False);
			}
		}

		public void TestBillCusAddInfoAndCusCodeDataMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
				declaration.JE_MasterBill = "MB232";
				var bill = declaration.PrimaryMasterBill;
				var billCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)bill;
				Type itDocType = null;
				billCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USITDoc, out itDocType);
				var itDoc = (CusAddInfo)Factory.New(itDocType);
				itDoc.B7_ParentID = bill.PK;
				itDoc.B7_ParentTableCode = bill.TablePrefix;
				itDoc.B7_AddInfoData = USITDocAddInfoSchema.Constants.US_7512OpenArea.Substring(3) + "=AREA1234";
				Type itNumberType = null;
				billCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USITNumber, out itNumberType);
				var itNumber = (CusAddInfo)Factory.New(itNumberType);
				itNumber.B7_ParentID = bill.PK;
				itNumber.B7_ParentTableCode = bill.TablePrefix;
				itNumber.B7_AddInfoData = USITNumberAddInfoSchema.Constants.US_ITNumber.Substring(3) + "=IT123";
				var billCusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)bill;
				var houseBillRefNoString = "HBR";
				var housebillRefNoOceanBill = "OM";
				Type houseBillRefNoType = null;
				billCusCodeDataTypeSupporter.GetCusCodeDataTypes().TryGetValue(houseBillRefNoString, out houseBillRefNoType);
				var houseBillRefNo = (CusCodeData)Factory.New(houseBillRefNoType);
				houseBillRefNo.Parent = bill;
				houseBillRefNo.CY_Code = housebillRefNoOceanBill;
				houseBillRefNo.CY_Data = "OM23423";
				Factory.SaveForTesting();
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));

				var declarationData = writer.GetDataObject(declaration);
				var billData = declarationData.AdditionalBillCollection[0];
				AssertEquals("billData.AddInfoGroupCollection.Count", 2, billData.AddInfoGroupCollection.Count);
				AssertContents(billData.AddInfoGroupCollection[0], GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.USITNumber, "IT Number"), new List<AddInfo>(new[] { new AddInfo() { Key = USITNumberAddInfoSchema.Constants.US_ITNumber.Substring(3), Value = "IT123" } }));
				AssertContents(billData.AddInfoGroupCollection[1], GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.USITDoc, "IT Doc"), new List<AddInfo>(new[] { new AddInfo() { Key = USITDocAddInfoSchema.Constants.US_7512OpenArea.Substring(3), Value = "AREA1234" } }));

				AssertEquals("billData.CustomsReferenceCollection.Count", 1, billData.CustomsReferenceCollection.Count);
				AssertContents(billData.CustomsReferenceCollection[0], GetCodeDescriptionPair(houseBillRefNoString, "House Bill Reference No"), GetCodeDescriptionPair(housebillRefNoOceanBill, "Ocean Manifest"), "OM23423", 0, ZBool.False);
			}
		}

		public void TestContainerCusAddInfoMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = "CONT123";
				var containerCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)container;
				Type dispositionType = null;
				containerCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDisposition, out dispositionType);
				var disposition = (CusAddInfo)Factory.New(dispositionType);
				disposition.B7_ParentID = container.PK;
				disposition.B7_ParentTableCode = container.TablePrefix;
				disposition.B7_AddInfoData = USDispositionDataAddInfoSchema.Constants.US_Code.Substring(3) + "=12";
				Factory.SaveForTesting();
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));

				var declarationData = writer.GetDataObject(declaration);
				var containerData = declarationData.ContainerCollection[0];
				AssertEquals(CollectionContent.Complete, declarationData.ContainerCollection.Content);
				AssertEquals("containerData.AddInfoGroupCollection.Count", 1, containerData.AddInfoGroupCollection.Count);
				AssertContents(containerData.AddInfoGroupCollection[0], GetCodeDescriptionPair(CusAddInfoTypeAttribute.Codes.USDisposition, "Disposition"), new List<AddInfo>(new[] { new AddInfo() { Key = USDispositionDataAddInfoSchema.Constants.US_Code.Substring(3), Value = "12" } }));
			}
		}

		public void TestExportParentBillNumWhenParentIsNull()
		{
			var declaration = Factory.BOFactory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "H798534789345";
			AssertNull("PreCondition", declaration.PrimaryHouseBill.ParentBill);
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));

			var declarationData = writer.GetDataObject(declaration);
			var billData = declarationData.AdditionalBillCollection[0];

			Assert("When parent bill is null", !billData.ParentBillNumber.HasValue);

			declaration.JE_MasterBill = "M798534789345";
			declaration.JE_MasterBill = "";
			AssertNotNull("PreCondition", declaration.PrimaryHouseBill.ParentBill);
			Factory.SaveForTesting();

			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));

			declarationData = writer.GetDataObject(declaration);
			billData = declarationData.AdditionalBillCollection[0];

			Assert("When parent bill is not null, but has a blank number", billData.ParentBillNumber.HasValue);
			AssertEquals(ZString.Empty, billData.ParentBillNumber.Value);
		}

		public void TestCusAgentIsMapped()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "Z!";
			staff.GS_FullName = "DUMMY BOB";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GS_NKCusAgent = "Z!";
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			var cusAgent = declarationData.CustomsBroker;
			AssertNotNull("cusAgent", cusAgent);
			AssertEquals("cusAgent.Code", "Z!", cusAgent.Code);
			AssertEquals("cusAgent.Name", "DUMMY BOB", cusAgent.Name);
		}

		public void TestLocalCartageCompanyIsMapped()
		{
			var orgCRAHOLSYD = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var orgWUFSHIJNB = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = orgWUFSHIJNB.MainAddress.PK;
			declaration.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgCRAHOLSYD.MainAddress.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("PreCondition", orgCRAHOLSYD.MainAddress.PK, declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			var pickupLocalCartage = declarationData.OrganizationAddressCollection.Find(x => x.AddressType.GetValueOrDefault() == AddressTypes.PickupLocalCartage);
			var deliveryLocalCartage = declarationData.OrganizationAddressCollection.Find(x => x.AddressType.GetValueOrDefault() == AddressTypes.DeliveryLocalCartage);
			AssertNotNull("pickupLocalCartage", pickupLocalCartage);
			AssertOrganizationBO_CRAHOLSYD("pickupLocalCartage", pickupLocalCartage, AddressTypes.PickupLocalCartage);
			AssertNull("deliveryLocalCartage should not be used for Export", deliveryLocalCartage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("PreCondition", orgWUFSHIJNB.MainAddress.PK, declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			declarationData = writer.GetDataObject(declaration);
			pickupLocalCartage = declarationData.OrganizationAddressCollection.Find(x => x.AddressType.GetValueOrDefault() == AddressTypes.PickupLocalCartage);
			deliveryLocalCartage = declarationData.OrganizationAddressCollection.Find(x => x.AddressType.GetValueOrDefault() == AddressTypes.DeliveryLocalCartage);
			AssertNull("pickupLocalCartage should not be used for Import", pickupLocalCartage);
			AssertNotNull("deliveryLocalCartage", deliveryLocalCartage);
			AssertOrganizationBO_WUFSHIJNB("deliveryLocalCartage", deliveryLocalCartage, AddressTypes.DeliveryLocalCartage);
		}

		public void TestIContainerParentDataObjectWriter()
		{
			var declaration = Factory.BOFactory.New<BaseJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT123";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT456";

			Factory.SaveForTesting();

			var writer = new ContainerTopLevelDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, container)));
			var data = writer.GetDataObject(container.JobContainer);
			AssertEquals("ContainerCollection.Count", 1, data.ContainerCollection.Count);
			AssertEquals("Should contain container", "CONT123", data.ContainerCollection[0].ContainerNumber);
			AssertEquals("Content should be Partial", CollectionContent.Partial, data.ContainerCollection.Content);
		}

		public void TestLloydsIMO()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = SetupJobDeclaration(declarationMock.Object);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "Dummy Vessel";
			declaration.JE_LloydsIMO = ZString.Empty;

			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);

			AssertEquals("Has Empty Lloyds", ZString.Empty, declarationData.LloydsIMO);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "hello";
			vessel.RV_LloydsNumber = "666666";
			declaration.JE_VesselName = vessel.RV_Code;

			Factory.SaveForTesting();

			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			declarationData = writer.GetDataObject(declaration);

			AssertEquals("Has Valid Lloyds", vessel.RV_LloydsNumber, declarationData.LloydsIMO);
		}

		public void TestPolpulateBillDetails()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MasterBill = "TESTMB";
			dec.JE_HouseBill = "TESTHB";
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
			var declarationData = writer.GetDataObject(dec);
			var billData = declarationData.AdditionalBillCollection.GetAdditionalBill("TESTHB", WayBillTypeList.Codes.House);
			AssertNotNull(billData);

			dec.JE_HouseBill = ZString.Empty;
			Factory.SaveForTesting();

			writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dec)));
			declarationData = writer.GetDataObject(dec);
			billData = declarationData.AdditionalBillCollection.GetAdditionalBill(ZString.Empty, WayBillTypeList.Codes.House);
			AssertNotNull(billData);
		}

		public void TestDataPopulatedtFromContext_CustomsDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			SetupJobDeclaration(declaration);
			var declarationData = writer.GetDataObject(declaration);
			AssertEquals("Sender local client exists", true, SenderLocalClientExists(declarationData));

			var shipment = Factory.New<ForwardingShipment>();
			var declarationOnShipment = Factory.New<BaseJobDeclaration>();
			declarationOnShipment.JE_JS = shipment.PK;
			//This happens when integration with third party happens.
			IMergeDataObjectWriter writer2 = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)) { FilteredDataContextType = DataContextType.CustomsDeclaration });
			var declarationData2 = CreatePopulatedShipmentDataForMerge();
			writer2.MergeData(declarationData2, declarationOnShipment);
			AssertEquals("Sender local client exists", true, SenderLocalClientExists(declarationData));
		}

		public void TestPopulateEntryInstructions()
		{
			CombineAssertions(() =>
			{
				var testDeclaration = Factory.NewWithValidTestData<BaseJobDeclarationWithEntryInstructions>();
				var testIns1 = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testIns1.CEI_Style = "11";
				var testIns2 = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testIns2.CEI_Style = "12";
				var testIns3 = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testIns3.CEI_Style = "13";
				var testHeader = testDeclaration.Invoices.AddNew();
				var testLine1 = testHeader.InvoiceLines.AddNew();
				var testLine2 = testHeader.InvoiceLines.AddNew();
				testLine2.JI_CEI = testIns3.PK;
				var testLine3 = testHeader.InvoiceLines.AddNew();
				testLine3.JI_CEI = testIns2.PK;

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, testDeclaration)));
				var result = writer.GetDataObject(testDeclaration);

				AssertEquals(3, result.EntryInstructionCollection.Count);
				AssertEquals(1, result.EntryInstructionCollection[0].Link);
				AssertEquals(2, result.EntryInstructionCollection[1].Link);
				AssertEquals(3, result.EntryInstructionCollection[2].Link);
				AssertEquals(1, result.CommercialInfo.CommercialInvoiceCollection.Count);
				var invLineCollection = result.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;
				AssertEquals(3, invLineCollection.Count);
				AssertEquals("Linking1", null, invLineCollection[0].EntryInstructionLink);
				AssertEquals("Linking2", 3, invLineCollection[1].EntryInstructionLink);
				AssertEquals("Linking3", 2, invLineCollection[2].EntryInstructionLink);
			});
		}

		public void TestPopulateEntryInstructions_NoEntryInstructions()
		{
			CombineAssertions(() =>
			{
				var testDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var testIns1 = Factory.NewWithValidTestData<CusEntryInstruction>();
				testIns1.CEI_JE = testDeclaration.PK;
				testIns1.CEI_Style = "11";
				var testIns2 = Factory.NewWithValidTestData<CusEntryInstruction>();
				testIns2.CEI_JE = testDeclaration.PK;
				testIns2.CEI_Style = "12";
				var testIns3 = Factory.NewWithValidTestData<CusEntryInstruction>();
				testIns3.CEI_JE = testDeclaration.PK;
				testIns3.CEI_Style = "13";
				var testHeader = testDeclaration.Invoices.AddNew();
				var testLine1 = testHeader.InvoiceLines.AddNew();
				var testLine2 = testHeader.InvoiceLines.AddNew();
				testLine2.JI_CEI = testIns3.PK;
				var testLine3 = testHeader.InvoiceLines.AddNew();
				testLine3.JI_CEI = testIns2.PK;

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, testDeclaration)));
				var result = writer.GetDataObject(testDeclaration);

				AssertEquals(null, result.EntryInstructionCollection);
				AssertEquals(1, result.CommercialInfo.CommercialInvoiceCollection.Count);
				var invLineCollection = result.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;
				AssertEquals(3, invLineCollection.Count);
				AssertEquals("Linking1", null, invLineCollection[0].EntryInstructionLink);
				AssertEquals("Linking2", null, invLineCollection[1].EntryInstructionLink);
				AssertEquals("Linking3", null, invLineCollection[2].EntryInstructionLink);
			});
		}

		public void TestFetchHintCreatorsForUSJobDeclaration()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var xml = string.Format(@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			          <Type>CustomsDeclaration</Type>
			          <Key>B00001000</Key>
			        </DataTarget>
			      </DataTargetCollection>

			      <Company>
			        <Code>{0}</Code>
			        <Name>{1}</Name>
			      </Company>
			      <EnterpriseID>{2}</EnterpriseID>
			      <ServerID>{3}</ServerID>
			    </DataContext>
			  </ShipmentRequest>
			</UniversalShipmentRequest>
			", GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name, registrationKey.EnterpriseCode, registrationKey.ServerCode);

			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new UniversalDataBuss.Management.SimpleLogger());
			var handler = new UniversalShipmentRequestHandler(xmlSessionTracker);
			var request = handler.CreateRequestMessage();
			var response = handler.CreateResponseMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(xml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			jobDeclaration[JobDeclarationSchema.JE_DeclarationReference] = "B00001000";
			jobDeclaration[JobDeclarationSchema.JE_MessageType] = "IMP";
			jobDeclaration[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;
			Factory.SaveForTesting();

			var responseMessageSaver = new UniversalResponseSaver(request, response, xmlSessionTracker);
			using (var result = handler.Process(request, responseMessageSaver))
			{
				var logParent = new BusinessObjectFactory().Load<BaseJobDeclaration>(jobDeclaration.PK) as IStmALogParent;
				var exportLog = logParent.Logs.MostRecentLogByEventTime(Events.DataExport);
				AssertNotNull("Expecting a DEX event", exportLog);
				AssertEquals("DEX event should be linked to response message", ((IEDIMessage)response).PK, exportLog.RelatedEDIMessage.Message.PK);

				result.ResponseMessageText.Position = 0;
				AssertContains(@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>CustomsDeclaration</Type>
          <Key>B00001000</Key>
        </DataSource>
      </DataSourceCollection>", new StreamReader(result.ResponseMessageText).ReadToEnd());
			}
		}

		public void TestDeclarationIssueDateNotOverwriteShipmentIssueDate()
		{
			eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBillIssueDate = new ZDateTime(2011, 1, 11);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "mb";
			bill.CU_BillType = "MB";
			bill.CU_IssueDate = new ZDateTime(2012, 2, 22);
			Factory.SaveForTesting();
			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, true);
			var dataObject = writer.GetDataObject(shipment);
			var issueDate = dataObject.DateCollection.FirstOrDefault(DateType.BillIssued, ZBool.False);
			AssertEquals("IssueDate", new ZDateTime(2012, 2, 22), issueDate.Value);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBillIssueDate = new ZDateTime(2011, 1, 11);
				declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_JS = shipment.PK;
				declaration.JE_OverrideFreightDefaults = true;
				bill = declaration.Bills.AddNew();
				bill.CU_BillNum = "mb";
				bill.CU_BillType = "MB";
				bill.CU_IssueDate = new ZDateTime(2012, 2, 22);
				Factory.SaveForTesting();
				writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true, true);
				dataObject = writer.GetDataObject(shipment);
				issueDate = dataObject.DateCollection.FirstOrDefault(DateType.BillIssued, ZBool.False);
				AssertEquals("IssueDate", new ZDateTime(2011, 1, 11), issueDate.Value);
			}
		}

		public void TestCustomsOffice()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_CustomsOffice = "TTTSST";
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			CombineAssertions(() =>
			{
				AssertNotNull(result.CustomsOffice);
				AssertEquals("TTTSST", result.CustomsOffice.Code);
				AssertEquals(null, result.CustomsOffice.Description);
			});
		}

		public void TestImporterOfRecord()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var expContact = testOrg.Contacts.AddNew();
			expContact.OC_ContactName = "EXPBRK";
			expContact.OC_Email = "EXPBRK@CW1.COM";
			var expDoc = expContact.Documents.AddNew();
			expDoc.OD_DocumentGroup = "BRE";
			var impContact = testOrg.Contacts.AddNew();
			impContact.OC_ContactName = "IMPBRK";
			impContact.OC_Email = "IMPBRK@CW1.COM";
			var impDoc = impContact.Documents.AddNew();
			impDoc.OD_DocumentGroup = "BRI";
			Factory.SaveForTesting();

			CombineAssertions("Export", () =>
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = "EXP";
				declaration.JE_OA_DeclarantAddress = testOrg.MainAddress.PK;
				Factory.SaveForTesting();
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var result = writer.GetDataObject(declaration);
				AssertEquals(1, result.OrganizationAddressCollection.Count(x => x.AddressType.Value == AddressTypes.Declarant));
				var orgData = result.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == AddressTypes.Declarant);
			});
			CombineAssertions("Import", () =>
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_OA_DeclarantAddress = testOrg.MainAddress.PK;
				Factory.SaveForTesting();
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var result = writer.GetDataObject(declaration);
				AssertEquals(1, result.OrganizationAddressCollection.Count(x => x.AddressType.Value == AddressTypes.Declarant));
				var orgData = result.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == AddressTypes.Declarant);
			});
		}

		public void TestLocationAtClearance()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_LocationOfGoods = "TTST HELLO WORLD THAT IS A LONG STR";
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			AssertEquals("TTST HELLO WORLD THAT IS A LONG STR", result.LocationAtClearance.Code);
			AssertEquals(ZString.Empty, result.LocationAtClearance.Description);
		}

		public void TestSubLocationAtClearance()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_SubLocationOfGoods = "TTST HELLO WORLD THAT IS A LONG STR";
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			AssertEquals("TTST HELLO WORLD THAT IS A LONG STR", result.SubLocationAtClearance.Code);
			AssertEquals(ZString.Empty, result.SubLocationAtClearance.Description);
		}

		public void TestTransportNationality()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Australia;
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			AssertEquals("TransportNationality.Code", Core.Constants.CountryCodes.Australia, result.TransportNationality.Code);
			AssertEquals("TransportNationality.Name", "Australia", result.TransportNationality.Name);
		}

		public void TestAdditionalTerms()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_ShipmentIncoTermPlace = "BOB THE BUILDER's PLACE";
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			AssertEquals("AdditionalTerms", "BOB THE BUILDER's PLACE", result.AdditionalTerms);
		}

		public void TestDeclarantType()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_DeclarantType = "IFD";
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			AssertEquals("DeclarantType", "IFD", result.DeclarantType.Code);
		}

		public void TestServiceRate()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var service1 = declaration.Services.AddNew();
			service1.ES_ServiceCode = "SC1";
			service1.ES_ServiceCount = 1;
			service1.ES_ServiceRate = 123.45;
			service1.ES_RX_NKServiceRateCurrency = "GBP";
			service1.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Hour;
			var service2 = declaration.Services.AddNew();
			service1.ES_ServiceCode = "SC2";
			service1.ES_ServiceCount = 2;
			service2.ES_ServiceRate = 234.56;
			service2.ES_RX_NKServiceRateCurrency = "USD";
			service2.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.FlatRate;

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			AssertEquals(2, result.LocalProcessing.AdditionalServiceCollection.Count);
			for (var i = 0; i < declaration.Services.Count; i++)
			{
				var target = result.LocalProcessing.AdditionalServiceCollection[i];
				var expected = declaration.Services[i];
				AssertEquals(expected.ES_ServiceCode, target.ServiceCode.Code);
				AssertEquals(expected.ES_ServiceCount, target.ServiceCount);
				AssertEquals(expected.ES_ServiceRate, target.ServiceRate);
				AssertEquals(expected.ES_RX_NKServiceRateCurrency, target.ServiceRateCurrency);
				AssertEquals(expected.ES_MeasurementBasis, target.MeasurementBasis);
			}
		}

		public void TestRepresentativeOrg()
		{
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			AssertEquals(1, result.OrganizationAddressCollection.Count(x => x.AddressType.Value == nameof(DocAddressType.Representative)));
		}

		public void TestUniqueConsignmentReference()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_UCR = "2GB836911119000-LLL25646";
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			AssertEquals("UniqueConsignmentReference", "2GB836911119000-LLL25646", result.UniqueConsignmentReference);
		}

		public void TestCarrierCodeOutputAsDummyCarrierOrganizationAddress()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var currentCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				var wrongCountryCode = Core.Constants.CountryCodes.China;

				var testOrgWithWrongCCCCountry = Factory.New<OrgHeader>();
				testOrgWithWrongCCCCountry.OH_Code = wrongCountryCode + "#@";
				testOrgWithWrongCCCCountry.OH_FullName = wrongCountryCode + " CC NAME";
				testOrgWithWrongCCCCountry.MainAddress.OA_Address1 = "ADDRESS 1";
				testOrgWithWrongCCCCountry.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CC" + wrongCountryCode, wrongCountryCode);
				var testOrg_CC = Factory.New<OrgHeader>();
				testOrg_CC.OH_Code = currentCountryCode + "#@";
				testOrg_CC.OH_FullName = currentCountryCode + " CC NAME";
				testOrg_CC.MainAddress.OA_Address1 = "ADDRESS 1";
				testOrg_CC.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CC" + currentCountryCode, currentCountryCode);
				var testOrg_C2 = Factory.New<OrgHeader>();
				testOrg_C2.OH_Code = currentCountryCode + "#2";
				testOrg_C2.OH_FullName = currentCountryCode + " C2 NAME";
				testOrg_C2.MainAddress.OA_Address1 = "ADDRESS 1";
				testOrg_C2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "C2" + currentCountryCode, currentCountryCode);
				Factory.SaveForTesting();

				CombineAssertions("Carrier with wrong country", () =>
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_CarrierCode = "CC" + wrongCountryCode;
					var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
					var result = writer.GetDataObject(declaration);
					AssertEquals(2, result.OrganizationAddressCollection.Count);
					var organizationAddressData = result.OrganizationAddressCollection[0];
					AssertNull(organizationAddressData.CompanyName);
					AssertEquals(1, organizationAddressData.RegistrationNumberCollection.Count);
					var registrationNumberData = organizationAddressData.RegistrationNumberCollection[0];
					AssertEquals(OrgCusCode.CodeTypes.CarrierCode, registrationNumberData.Type.Code);
					AssertEquals(currentCountryCode, registrationNumberData.CountryOfIssue.GetCodeAsUpperCase());
					AssertEquals("CC" + wrongCountryCode, registrationNumberData.Value);
				});

				CombineAssertions("Carrier with right country", () =>
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_CarrierCode = "CC" + currentCountryCode;
					var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
					var result = writer.GetDataObject(declaration);
					AssertEquals(2, result.OrganizationAddressCollection.Count);
					var organizationAddressData = result.OrganizationAddressCollection[0];
					AssertEquals(currentCountryCode + " CC NAME", organizationAddressData.CompanyName);
					AssertEquals(1, organizationAddressData.RegistrationNumberCollection.Count);
					var registrationNumberData = organizationAddressData.RegistrationNumberCollection[0];
					AssertEquals(OrgCusCode.CodeTypes.CarrierCode, registrationNumberData.Type.Code);
					AssertEquals(currentCountryCode, registrationNumberData.CountryOfIssue.GetCodeAsUpperCase());
					AssertEquals("CC" + currentCountryCode, registrationNumberData.Value);
				});
			}
		}

		public void TestPopulatePortNameWhenPortIsNotInLookupsList()
		{
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "ABC12";
			port.RL_PortName = "NANJING";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "ABC12";
			declaration.JE_RL_NKPortOfLoading = "ABC12";
			declaration.JE_RL_NKPortOfFirstArrival = "ABC12";
			declaration.JE_RL_NKPortOfArrival = "ABC12";
			declaration.JE_RL_NKFinalDestination = "ABC12";

			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);
			CombineAssertions(() =>
			{
				AssertEquals("PortOfOrigin.Code", "ABC12", shipment.PortOfOrigin.Code);
				AssertEquals("PortOfOrigin.Name", "NANJING", shipment.PortOfOrigin.Name);

				AssertEquals("PortOfOrigin.PortOfLoading", "ABC12", shipment.PortOfLoading.Code);
				AssertEquals("PortOfOrigin.PortOfLoading", "NANJING", shipment.PortOfLoading.Name);

				AssertEquals("PortOfFirstArrival.Code", "ABC12", shipment.PortOfFirstArrival.Code);
				AssertEquals("PortOfFirstArrival.Name", "NANJING", shipment.PortOfFirstArrival.Name);

				AssertEquals("PortOfDischarge.Code", "ABC12", shipment.PortOfDischarge.Code);
				AssertEquals("PortOfDischarge.Name", "NANJING", shipment.PortOfDischarge.Name);

				AssertEquals("PortOfDestination.PortOfLoading", "ABC12", shipment.PortOfDestination.Code);
				AssertEquals("PortOfDestination.PortOfLoading", "NANJING", shipment.PortOfDestination.Name);
			});
		}

		public void TestGoodsOrigin()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Australia;
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			AssertEquals("AU", result.GoodsOrigin.Code);
			AssertEquals("Australia", result.GoodsOrigin.Description);
		}

		public void TestGoodsDestination()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Germany;
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var result = writer.GetDataObject(declaration);
			AssertEquals("DE", result.GoodsDestination);
		}

		public void TestRealFieldsMappedToAddInfo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals("declaration.SupportUseOwnerRefAsQuarantineRefUsage", true, declaration.SupportUseOwnerRefAsQuarantineRefUsage);
				declaration.JE_TransportModeInland = Core.Constants.TransportCodes.Road;
				declaration.JE_UseOwnerRefAsQuarantineRef = ZBool.True;
				Factory.SaveForTesting();
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var result = writer.GetDataObject(declaration);
				AssertEquals("InlandModeOfTransport", Core.Constants.TransportCodes.Road, result.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.InlandModeOfTransport));
				AssertEquals("UseOwnerRefAsQuarantineRef", YesNoList.Codes.Yes, result.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.UseOwnerRefAsQuarantineRef));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals("declaration.SupportUseOwnerRefAsQuarantineRefUsage for AU", true, declaration.SupportUseOwnerRefAsQuarantineRefUsage);
				declaration.JE_UseOwnerRefAsQuarantineRef = ZBool.True;
				Factory.SaveForTesting();
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var result = writer.GetDataObject(declaration);
				AssertEquals("UseOwnerRefAsQuarantineRef for AU", YesNoList.Codes.Yes, result.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.UseOwnerRefAsQuarantineRef));
			}
		}

		public void TestDummyPackingLineForInvoiceLineContainerLink()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_TransportMode = "SEA";
				var container1 = declaration.CusContainers.AddNew();
				container1.CO_ContainerNumber = "CONT001";
				var container2 = declaration.CusContainers.AddNew();
				container2.CO_ContainerNumber = "CONT002";
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "INV001";
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				invoiceLine1.JI_Description = "DESC1";
				var invoiceLine2 = invoice1.InvoiceLines.AddNew();
				invoiceLine2.JI_Description = "DESC2";
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceNumber = "INV002";
				var invoiceLine3 = invoice2.InvoiceLines.AddNew();
				invoiceLine3.JI_Description = "DESC3";
				container1.InvoiceLinePivotCollection.AddNew(invoiceLine1);
				container1.InvoiceLinePivotCollection.AddNew(invoiceLine3);
				container2.InvoiceLinePivotCollection.AddNew(invoiceLine2);
				container2.InvoiceLinePivotCollection.AddNew(invoiceLine3);

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var result = writer.GetDataObject(declaration);
				CombineAssertions(() =>
				{
					AssertEquals(2, result.ContainerCollection.Count);
					AssertEquals(2, result.PackingLineCollection.Count);
					var link1 = result.CommercialInfo.CommercialInvoiceCollection.FirstOrDefault(x => x.InvoiceNumber.Value == "INV001").CommercialInvoiceLineCollection.FirstOrDefault(x => x.Description.Value == "DESC1").Link.Value;
					var link2 = result.CommercialInfo.CommercialInvoiceCollection.FirstOrDefault(x => x.InvoiceNumber.Value == "INV001").CommercialInvoiceLineCollection.FirstOrDefault(x => x.Description.Value == "DESC2").Link.Value;
					var link3 = result.CommercialInfo.CommercialInvoiceCollection.FirstOrDefault(x => x.InvoiceNumber.Value == "INV002").CommercialInvoiceLineCollection.FirstOrDefault(x => x.Description.Value == "DESC3").Link.Value;
					AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 2, 3 }, new ZInt[] { link1, link2, link3 });
					var contlink1 = result.PackingLineCollection[0];
					AssertEquals("CONT001", contlink1.ContainerNumber);
					AssertEquals(2, contlink1.PackedItemCollection.Count);
					AssertEquals(link1, contlink1.PackedItemCollection[0].CommercialInvoiceLineLink);
					AssertEquals(link3, contlink1.PackedItemCollection[1].CommercialInvoiceLineLink);
					var contlink2 = result.PackingLineCollection[1];
					AssertEquals("CONT002", contlink2.ContainerNumber);
					AssertEquals(2, contlink2.PackedItemCollection.Count);
					AssertEquals(link2, contlink2.PackedItemCollection[0].CommercialInvoiceLineLink);
					AssertEquals(link3, contlink2.PackedItemCollection[1].CommercialInvoiceLineLink);
				});
			}
		}

		public void TestParentChildPackagesSwitchingSupportsParentPackage()
		{
			var declarationMock = CreateJobDeclarationMock();
			var declaration = SetupJobDeclaration(declarationMock.Object);
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertEquals("declarationData.PackingLineCollection.Count", 3, declarationData.PackingLineCollection.Count);
			AssertNull("declarationData.PackingLineCollection[0].PackingLineCollection", declarationData.PackingLineCollection[0].PackingLineCollection);
			AssertNull("declarationData.PackingLineCollection[1].PackingLineCollection", declarationData.PackingLineCollection[1].PackingLineCollection);
			AssertNull("declarationData.PackingLineCollection[2].PackingLineCollection", declarationData.PackingLineCollection[2].PackingLineCollection);

			declarationMock.Setup(m => m.SupportsParentPackage).Returns(true);
			var writer2 = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var declarationData2 = writer.GetDataObject(declaration);
			AssertEquals("declarationData2.PackingLineCollection.Count", 3, declarationData2.PackingLineCollection.Count);
			AssertNotNull("declarationData2.PackingLineCollection[0].PackingLineCollection", declarationData2.PackingLineCollection[0].PackingLineCollection);
			AssertNotNull("declarationData2.PackingLineCollection[1].PackingLineCollection", declarationData2.PackingLineCollection[1].PackingLineCollection);
			AssertNotNull("declarationData2.PackingLineCollection[2].PackingLineCollection", declarationData2.PackingLineCollection[2].PackingLineCollection);
		}

		public void TestParentChildPackagesAreExportedWhenSupportsParentPackage()
		{
			var declarationMock = CreateJobDeclarationMock();
			declarationMock.Setup(m => m.SupportsParentPackage).Returns(true);
			var declaration = SetupJobDeclaration(declarationMock.Object);
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var declarationData = writer.GetDataObject(declaration);

			AssertEquals("declarationData.PackingLineCollection.Count", 3, declarationData.PackingLineCollection.Count);
			AssertContents(declarationData.PackingLineCollection[0], "CONT1", "MB1", GetCodeDescriptionPair("MWB", "Master Waybill"), "MARKS 1", 10, GetCodeDescriptionPair("BOX", "Box"), 11, 9, "SHIPPING");
			AssertNotNull("declarationData.PackingLineCollection[0].PackingLineCollection", declarationData.PackingLineCollection[0].PackingLineCollection);
			AssertEquals("declarationData.PackingLineCollection.Count", 0, declarationData.PackingLineCollection[0].PackingLineCollection.Count);

			AssertContents(declarationData.PackingLineCollection[1], "CONT2", "MB1HB2", GetCodeDescriptionPair("HWB", "House Waybill"), "MARKS 2", 20, GetCodeDescriptionPair("PKG", "Package"), 19, 21, "SHIPPING 2");
			AssertEquals("declarationData.PackingLineCollection[1].PackingLineCollection", 2, declarationData.PackingLineCollection[1].PackingLineCollection.Count);
			AssertContents(declarationData.PackingLineCollection[1].PackingLineCollection[0], "CONT2", "MB1HB2", GetCodeDescriptionPair("HWB", "House Waybill"), "CHILDMARKS 1", 40, GetCodeDescriptionPair("PKG", "Package"), 39, 41, "CHILDSHIPPING 1");
			AssertContents(declarationData.PackingLineCollection[1].PackingLineCollection[1], "CONT2", "MB1HB2", GetCodeDescriptionPair("HWB", "House Waybill"), "CHILDMARKS 2", 50, GetCodeDescriptionPair("PCE", "Piece"), 49, 51, "CHILDSHIPPING 2");

			AssertContents(declarationData.PackingLineCollection[2], "CONT1", "MB2HB1SB1", GetCodeDescriptionPair("SWB", "Sub-House Waybill"), "MARKS 3", 30, GetCodeDescriptionPair("PCE", "Piece"), 29, 31, "SHIPPING 3");
			AssertEquals("declarationData.PackingLineCollection[2].PackingLineCollection", 1, declarationData.PackingLineCollection[2].PackingLineCollection.Count);
			AssertContents(declarationData.PackingLineCollection[2].PackingLineCollection[0], "CONT1", "MB2HB1SB1", GetCodeDescriptionPair("SWB", "Sub-House Waybill"), "CHILDMARKS 3", 60, GetCodeDescriptionPair("BOX", "Box"), 59, 61, "CHILDSHIPPING 3");
		}

		public void TestChildrenOfChildAreExported()
		{
			var declarationMock = CreateJobDeclarationMock();
			declarationMock.Setup(m => m.SupportsParentPackage).Returns(true);
			var declaration = SetupJobDeclaration(declarationMock.Object);
			Factory.SaveForTesting();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declaration)));
			var declarationData = writer.GetDataObject(declaration);

			AssertEquals("declarationData.PackingLineCollection.Count", 3, declarationData.PackingLineCollection.Count);
			AssertEquals("declarationData.PackingLineCollection[1].PackingLineCollection", 2, declarationData.PackingLineCollection[1].PackingLineCollection.Count);
			AssertNotNull("declarationData.PackingLineCollection[1].PackingLineCollection[0].PackingLineCollection", declarationData.PackingLineCollection[1].PackingLineCollection[0].PackingLineCollection);
			AssertEquals("declarationData.PackingLineCollection[1].PackingLineCollection[0].PackingLineCollection.Count", 1, declarationData.PackingLineCollection[1].PackingLineCollection[0].PackingLineCollection.Count);
		}

		public void TestUniversalCustomsMessagingEntryHeaders()
		{
			var declarationBO = Factory.NewWithValidTestData<BaseJobDeclarationWithEntryInstructions>();
			var entry1 = declarationBO.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "REF1";
			var entry2 = declarationBO.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "REF2";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declarationBO)));
			var dataObject1 = writer.GetDataObject(declarationBO);
			AssertEquals(2, dataObject1.EntryHeaderCollection.Count);
			AssertNotNull(dataObject1.EntryHeaderCollection.FirstOrDefault(header => header.Reference.Equals("REF1")));
			AssertNotNull(dataObject1.EntryHeaderCollection.FirstOrDefault(header => header.Reference.Equals("REF2")));

			var writer2 = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, declarationBO)));
			writer2.SetEntryHeaderPKsToPopulate(new List<ZGuid> { entry1.PK });
			var dataObject2 = writer2.GetDataObject(declarationBO);

			AssertEquals(1, dataObject2.EntryHeaderCollection.Count);
			AssertNotNull(dataObject2.EntryHeaderCollection.Where(header => header.Reference.Equals("REF1")));
		}

		public void TestUniversalCustomsShipmentLodgmentMessaging_TransportLegCollection()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "HKHKG";

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			var transport2 = shipment.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "HKHKG";

			var declarationBO = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declarationBO.JE_MessageType = "AQS";
			declarationBO.JE_JS = shipment.PK;
			var entry1 = declarationBO.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "1234";

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declarationBO)) { FilteredDataContextType = DataContextType.CustomsDeclaration });
			var universalShipmentDataObject = writer.GetDataObject(declarationBO);
			AssertEquals(2, universalShipmentDataObject.TransportLegCollection.Count);
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
			CreateCusFeeCodeDescriptionPairListForTest();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}

		void CreateCusFeeCodeDescriptionPairListForTest()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AccountingClassFeeCode, RefCusCodeListTypes.Codes.AccountingClassFeeCode);
			new[] { Core.Constants.USCustoms.FeeCodes.Avocado, Core.Constants.USCustoms.FeeCodes.Beef, Core.Constants.USCustoms.FeeCodes.Blueberry,
				Core.Constants.USCustoms.FeeCodes.Coffee, Core.Constants.USCustoms.FeeCodes.Cotton, Core.Constants.USCustoms.FeeCodes.DairyFee,
				Core.Constants.USCustoms.FeeCodes.DistilledSpirits, Core.Constants.USCustoms.FeeCodes.DutiableMail, Core.Constants.USCustoms.FeeCodes.FreshLimes,
				Core.Constants.USCustoms.FeeCodes.HMF, Core.Constants.USCustoms.FeeCodes.Honey, Core.Constants.USCustoms.FeeCodes.Mango,
				Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge,
				Core.Constants.USCustoms.FeeCodes.Mushroom, Core.Constants.USCustoms.FeeCodes.OtherAgencies, Core.Constants.USCustoms.FeeCodes.OtherExcise,
				Core.Constants.USCustoms.FeeCodes.Pork, Core.Constants.USCustoms.FeeCodes.Potato, Core.Constants.USCustoms.FeeCodes.Raspberry,
				Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, Core.Constants.USCustoms.FeeCodes.Sorghum, Core.Constants.USCustoms.FeeCodes.Sugar,
				Core.Constants.USCustoms.FeeCodes.Tobacco, Core.Constants.USCustoms.FeeCodes.Watermelon, Core.Constants.USCustoms.FeeCodes.Wines }.
				ForEach(x => helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode, x, x + " Desc from DB", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6)));
			factory.Save();
		}

		UniversalDataObjectWriterHelper currentCompanyHelper;
		UniversalDataObjectWriterHelper CurrentCompanyHelper => currentCompanyHelper ?? (currentCompanyHelper = new UniversalDataObjectWriterHelper(Factory.BOFactory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

		OrgCustomLabels AddCustomsLabel(OrgHeader org, ZString fieldName, ZString caption)
		{
			var label = org.CustomLabels.AddNew();
			label.OT_FieldName = fieldName;
			label.OT_Caption = caption;
			return label;
		}

		void SetupPartAttrib1(OrgHeader org, ZString name, ZString type)
		{
			var misc = org.MiscServ;
			misc.OM_IMPartAttrib1Name = name;
			misc.OM_IMPartAttrib1Type = type;
		}

		void SetupPartAttrib2(OrgHeader org, ZString name, ZString type)
		{
			var misc = org.MiscServ;
			misc.OM_IMPartAttrib2Name = name;
			misc.OM_IMPartAttrib2Type = type;
		}

		void SetupPartAttrib3(OrgHeader org, ZString name, ZString type)
		{
			var misc = org.MiscServ;
			misc.OM_IMPartAttrib3Name = name;
			misc.OM_IMPartAttrib3Type = type;
		}

		Mock<BaseJobDeclaration> CreateJobDeclarationMock()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			var lookupsMock = new Mock<JobDeclarationLookups>(declarationMock.Object);
			lookupsMock.CallBase = true;
			declarationMock.Protected().Setup<JobDeclarationLookups>("GetNewLookups").Returns(lookupsMock.Object);
			lookupsMock.Setup(m => m.EntryStatusList).Returns(new CustomsEntryStatusList());
			var eftModeList = new CodeDescriptionPairList();
			eftModeList.AddPair("EFT", "EFT DATA");
			lookupsMock.Setup(m => m.EFTModeList).Returns(eftModeList);
			var operationalStatusList = new CodeDescriptionPairList();
			operationalStatusList.AddPair("OS1", "OPERATIONAL STATUS 1");
			lookupsMock.Setup(m => m.OperationalStatusList).Returns(operationalStatusList);
			var messageStatusList = new CodeDescriptionPairList();
			messageStatusList.AddPair("MS1", "MESSAGE STATUS 1");
			lookupsMock.Setup(m => m.MessageStatusList).Returns(messageStatusList);
			var consolidatedCargoStatusList = new CodeDescriptionPairList();
			consolidatedCargoStatusList.AddPair("CC1", "CONSOLIDATED STATUS 1");
			lookupsMock.Setup(m => m.ConsolidatedCargoStatusList).Returns(consolidatedCargoStatusList);
			var messageSubTypeList = new CodeDescriptionPairList();
			messageSubTypeList.AddPair("ST1", "STANDARD");
			lookupsMock.Setup(m => m.MessageSubTypeList).Returns(messageSubTypeList);
			var warehouseTransactionStatusList = new CodeDescriptionPairList();
			warehouseTransactionStatusList.AddPair("WR1", "WAREHOUSERELEASE STATUS 1");
			lookupsMock.Setup(m => m.WarehouseTransactionStatusList).Returns(warehouseTransactionStatusList);
			var goodsOriginList = new CodeDescriptionPairList();
			goodsOriginList.AddPair("AB", "TEST AB Origin Territory");
			lookupsMock.Setup(m => m.GoodsOrigin).Returns(goodsOriginList);

			return declarationMock;
		}

		GlbCompany usCompany;
		GlbCompany USCompany
		{
			get
			{
				if (usCompany == null)
				{
					usCompany = Factory.New<GlbCompany>();
					usCompany.GC_Code = "US@";
					usCompany.GC_Name = "US Company Test";
					usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
					usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				}
				return usCompany;
			}
		}

		GlbBranch usBranch;
		GlbBranch USBranch
		{
			get
			{
				if (usBranch == null)
				{
					usBranch = USCompany.Branches.AddNew();
					usBranch.GB_Code = "US@";
					usBranch.GB_BranchName = "US Branch Test";
					usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				}
				return usBranch;
			}
		}

		RefContainer refContainer1;
		RefContainer RefContainer1
		{
			get
			{
				if (refContainer1 == null)
				{
					refContainer1 = Factory.New<RefContainer>();
					refContainer1.RC_Code = "Z!Z1";
					refContainer1.RC_Height = 10m;
					refContainer1.RC_Width = 11m;
					refContainer1.RC_Length = 12m;
				}
				return refContainer1;
			}
		}

		RefContainer refContainer2;
		RefContainer RefContainer2
		{
			get
			{
				if (refContainer2 == null)
				{
					refContainer2 = Factory.New<RefContainer>();
					refContainer2.RC_Code = "Z!Z2";
					refContainer2.RC_Height = 20m;
					refContainer2.RC_Width = 21m;
					refContainer2.RC_Length = 22m;
				}
				return refContainer2;
			}
		}

		Mock<BaseCusContainer> CreateCusContainerMock(BaseJobDeclaration declaration)
		{
			var result = Factory.NewMoq<BaseCusContainer>();
			var container = result.Object;
			declaration.CusContainers.Add(container);
			var lookupsMock = new Mock<CusContainerLookups>(container);
			lookupsMock.CallBase = true;
			var containerSizeList = new CodeDescriptionPairList();
			containerSizeList.AddPair("20", "Container 20!");
			containerSizeList.AddPair("21", "Container 21!");
			lookupsMock.Setup(m => m.ContainerSizeList).Returns(containerSizeList);
			var messageStatusList = new CodeDescriptionPairList();
			messageStatusList.AddPair("CLR", "MESSAGE STATUS CLEAR");
			messageStatusList.AddPair("MS1", "MESSAGE STATUS 1");
			lookupsMock.Setup(m => m.MessageStatusList).Returns(messageStatusList);

			result.Protected().Setup<CusContainerLookups>("GetNewLookups").Returns(lookupsMock.Object);
			return result;
		}

		BaseCusContainer SetupCusContainer(BaseCusContainer container, ZString containerNumber, ZDecimal weight, ZString weightUQ, ZString seal, ZString secondSeal, ZString containerMode, ZGuid refContainerTypePK, ZString messageStatus, ZString containerSize)
		{
			container.CO_ContainerNumber = containerNumber;
			container.CO_Weight = weight;
			container.CO_WeightUQ = weightUQ;
			container.CO_Seal = seal;
			container.CO_SecondSeal = secondSeal;
			container.CO_FCL_LCL_AIR = containerMode;
			container.CO_RC = refContainerTypePK;
			container.CO_MessageStatus = messageStatus;
			container.CO_ContainerSize = containerSize;
			return container;
		}

		BasePackage SetupPackage(BasePackage package, ZString containerNumber, ZString billNumber, ZString marksAndNos, ZInt packQty, ZString packType, ZInt inBondPackQty, ZInt outerPacks, ZString shippingSymbol)
		{
			package.CW_ContainerNoOrEquipmentNo = containerNumber;
			package.CW_HouseBill = billNumber;
			package.CW_MarksAndNos = marksAndNos;
			package.CW_PackQty = packQty;
			package.CW_PackType = packType;
			package.CW_InBondPackQty = inBondPackQty;
			package.CW_OuterPacks = outerPacks;
			package.CW_ShippingSymbol = shippingSymbol;
			return package;
		}

		BaseJobDeclaration SetupJobDeclaration(BaseJobDeclaration declaration)
		{
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			org2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_OH_Importer = org2.PK;
			var supplierAddresss = declaration.DocAddresses.FindByDocAddressType(DocAddressType.SupplierDocumentaryAddress);
			supplierAddresss.E2_OA_Address = org1.MainAddress.PK;
			var importerAddresss = declaration.DocAddresses.FindByDocAddressType(DocAddressType.ImporterDocumentaryAddress);
			importerAddresss.E2_OA_Address = org2.MainAddress.PK;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_ContainerCount = 2;
			declaration.JE_GoodsDescription = "FUNNY GOODS";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = "ST1";
			declaration.JE_TotalNoOfPacks = 100;
			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Piece;
			declaration.JE_TotalVolume = 10.50m;
			declaration.JE_TotalVolumeUnit = Core.Constants.Volume.CubicFeet;
			declaration.JE_TotalWeight = 1506.682m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.Transports.RemoveAndDeleteAll();
			var transport1 = SetupTransport(declaration.Transports.AddNew(), Core.Constants.TransportModes.Rail, "", "SEK323", SeaLocalPort1.RL_Code, SeaLocalPort2.RL_Code);
			var transport2 = SetupTransport(declaration.Transports.AddNew(), Core.Constants.TransportModes.Sea, "APL EMERALD", "V23W", SeaLocalPort2.RL_Code, SeaForeignPort1.RL_Code);
			var transport3 = SetupTransport(declaration.Transports.AddNew(), Core.Constants.TransportModes.Sea, "APL EMERALD", "V24W", SeaForeignPort1.RL_Code, SeaForeignPort2.RL_Code);
			var transport4 = SetupTransport(declaration.Transports.AddNew(), Core.Constants.TransportModes.Road, "", "CAR-123", SeaForeignPort2.RL_Code, SeaForeignPort3.RL_Code);
			declaration.JE_RL_NKOrigin = SeaLocalPort1.RL_Code;
			declaration.JE_RL_NKPortOfLoading = SeaLocalPort2.RL_Code;
			declaration.JE_RL_NKPortOfFirstArrival = SeaForeignPort1.RL_Code;
			declaration.JE_RL_NKPortOfArrival = SeaForeignPort2.RL_Code;
			declaration.JE_RL_NKFinalDestination = SeaForeignPort3.RL_Code;
			declaration.JE_VesselName = "APL EMERALD";
			declaration.JE_VoyageFlightNo = "V23W";

			declaration.JE_RS_NKServiceLevel = ServiceLevel1.RS_Code;
			declaration.JE_IsPersonalEffects = ZBool.True;
			declaration.JE_EFTMode = "EFT";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_OperationalStatus = "OS1";
			declaration.JE_MessageStatus = "MS1";
			declaration.JE_EntryStatus = CustomsEntryStatusList.Codes.AwaitingFDACorrection;
			declaration.JE_ConsolidatedCargoStatus = "CC1";
			declaration.JE_TotalNoOfPieces = 112;
			declaration.JE_LandedPieces = 307;
			declaration.JE_ExportGoodsType = "SP";
			declaration.JE_AgentsReference = "AGREF123";
			declaration.JE_OwnerRef = "OWN324";
			declaration.JE_Folio = "F234";
			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			declaration.JE_PaidBy = MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK;
			declaration.JE_ShipmentIncoTerm = "FOB";
			declaration.JE_TotalNoOfPacksDecimal = 789.012m;
			declaration.JE_GoodsOrigin = "AB";

			declaration.JE_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;

			declaration.JE_MasterBill = "MB1";
			var masterBill1 = declaration.PrimaryMasterBill;
			masterBill1.CU_IssueDate = new ZDateTime(2011, 5, 20);
			masterBill1.CU_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;

			declaration.JE_HouseBill = "MB1HB1";
			var masterBill1HouseBill1 = declaration.PrimaryHouseBill;
			masterBill1HouseBill1.CU_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			masterBill1HouseBill1.CU_IssueDate = new ZDateTime(2011, 5, 21);
			((IColumnIndexer)((IBusinessObjectInternals)masterBill1HouseBill1).Row).SetValue(CusDecHouseBillSchema.CU_GUIPresentationRecord, ZBool.False); // Should not be used
			var masterBill1HouseBill1SubHouse1 = masterBill1HouseBill1.ChildBills.AddNew();
			masterBill1HouseBill1SubHouse1.CU_BillNum = "MB1HB1SB1";
			masterBill1HouseBill1SubHouse1.CU_IssueDate = new ZDateTime(2011, 5, 22);
			var masterBill1HouseBill1SubHouse2 = masterBill1HouseBill1.ChildBills.AddNew();
			masterBill1HouseBill1SubHouse2.CU_BillNum = "MB1HB1SB2";
			masterBill1HouseBill1SubHouse2.CU_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			masterBill1HouseBill1SubHouse2.CU_IssueDate = new ZDateTime(2011, 5, 23);

			var masterBill1HouseBill2 = masterBill1.ChildBills.AddNew();
			masterBill1HouseBill2.CU_BillNum = "MB1HB2";
			masterBill1HouseBill2.CU_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			masterBill1HouseBill2.CU_IssueDate = new ZDateTime(2011, 5, 24);
			var masterBill1HouseBill2SubHouse1 = masterBill1HouseBill2.ChildBills.AddNew();
			masterBill1HouseBill2SubHouse1.CU_BillNum = "MB1HB2SB1";
			masterBill1HouseBill2SubHouse1.CU_IssueDate = new ZDateTime(2011, 5, 25);

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MB2";
			masterBill2.CU_IssueDate = new ZDateTime(2011, 5, 26);
			var masterBill2HouseBill1 = masterBill2.ChildBills.AddNew();
			masterBill2HouseBill1.CU_BillNum = "MB2HB1";
			masterBill2HouseBill1.CU_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			masterBill2HouseBill1.CU_IssueDate = new ZDateTime(2011, 5, 27);
			masterBill2HouseBill1.CU_NoOfPacks = 13.50m;
			masterBill2HouseBill1.CU_PackType = Core.Constants.PkgUnit.Box;

			var masterBill2HouseBill1SubHouse1Mock = Factory.NewMoq<Bill>();
			var masterBill2HouseBill1SubHouse1 = masterBill2HouseBill1SubHouse1Mock.Object;
			var masterBill2HouseBill1SubHouse1LookupsMock = new Mock<CusDecHouseBillLookups>(masterBill2HouseBill1SubHouse1);
			var messageStatusList = new CodeDescriptionPairList();
			messageStatusList.AddPair("CLR", "CLEAR");
			masterBill2HouseBill1SubHouse1LookupsMock.Setup(m => m.MessageStatusList).Returns(messageStatusList);
			var masterBill2HouseBill1SubHouse1Lookups = masterBill2HouseBill1SubHouse1LookupsMock.Object;
			masterBill2HouseBill1SubHouse1Mock.Protected().Setup<CusDecHouseBillLookups>("GetNewLookups").Returns(masterBill2HouseBill1SubHouse1Lookups);
			masterBill2HouseBill1SubHouse1.CU_JE = declaration.PK;
			masterBill2HouseBill1SubHouse1.CU_CU_ParentBill = masterBill2HouseBill1.PK;
			masterBill2HouseBill1.ChildBills.Add(masterBill2HouseBill1SubHouse1);
			masterBill2HouseBill1SubHouse1.CU_BillNum = "MB2HB1SB1";
			masterBill2HouseBill1SubHouse1.CU_BillType = BillTypeList.Codes.SubHouseBill;
			masterBill2HouseBill1SubHouse1.CU_IssueDate = new ZDateTime(2011, 5, 28);
			masterBill2HouseBill1SubHouse1.CU_Status = "CLR";

			// The following bills should be included in the xml as the Bill Number is empty
			var masterBill2HouseBill1SubHouse2EmptyNumber = masterBill2HouseBill1.ChildBills.AddNew();
			masterBill2HouseBill1SubHouse2EmptyNumber.CU_BillNum = ZString.Empty;
			masterBill2HouseBill1SubHouse2EmptyNumber.CU_IssueDate = new ZDateTime(2011, 5, 29);
			masterBill2HouseBill1SubHouse2EmptyNumber.CU_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;

			var masterBill2HouseBill2EmptyNumber = masterBill2.ChildBills.AddNew();
			masterBill2HouseBill2EmptyNumber.CU_BillNum = ZString.Empty;
			masterBill2HouseBill2EmptyNumber.CU_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			masterBill2HouseBill2EmptyNumber.CU_IssueDate = new ZDateTime(2011, 5, 30);

			var masterBill3EmptyNumber = declaration.Bills.AddNew();
			masterBill3EmptyNumber.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill3EmptyNumber.CU_BillNum = ZString.Empty;
			masterBill3EmptyNumber.CU_IssueDate = new ZDateTime(2011, 5, 31);

			declaration.JE_DateOfArrival = new ZDateTime(2011, 6, 10);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2011, 6, 11);
			declaration.JE_ExportDate = new ZDateTime(2011, 6, 12);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2011, 7, 13);
			declaration.JE_EntrySubmittedDate = new ZDateTime(2011, 7, 14);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2011, 7, 15);
			declaration.JE_WarehouseReleaseDate = new ZDateTime(2011, 7, 16);
			declaration.JE_DateAtOrigin = new ZDateTime(2011, 7, 17);
			declaration.JE_EntryDate = new ZDate(2011, 7, 18);

			var localCurrencyCode = declaration.LocalCurrencyCode;

			// Add mock for message status
			var container1Mock = CreateCusContainerMock(declaration);
			var container1 = SetupCusContainer(container1Mock.Object, "CONT1", 1500.50m, Core.Constants.Weight.Kilograms, "C1S1", "C1S2", Core.Constants.ContainerModes.LCL, RefContainer1.PK, "CLR", "21");
			var container2Mock = CreateCusContainerMock(declaration);
			var container2 = SetupCusContainer(container2Mock.Object, "CONT2", 2.250m, Core.Constants.Weight.Tonnes, "C2S1", "C2S2", Core.Constants.ContainerModes.FCL, RefContainer2.PK, "MS1", "20");
			var org = declaration.ConfigOrg;
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomAttribute1, "STRING1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomDate1, "DATE1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomDecimal1, "DECIMAL1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomFlag1, "FLAG1");
			container2.CO_CustomAttrib1 = "COATT1";
			container2.CO_CustomDate1 = new ZDateTime(2011, 8, 18);
			container2.CO_CustomDecimal1 = 30.2m;
			container2.CO_CustomFlag1 = ZBool.True;

			declaration.Packages.RemoveAndDeleteAll();
			declaration.PackingGroups.RemoveAndDeleteAll();
			var package1 = SetupPackage(CreatePackageMock(declaration).Object, container1.CO_ContainerNumber, "MB:MB1", "MARKS 1", 10, Core.Constants.PkgUnit.Box, 11, 9, "SHIPPING");
			var package2 = SetupPackage(CreatePackageMock(declaration).Object, container2.CO_ContainerNumber, "HB:MB1HB2 (MB:MB1)", "MARKS 2", 20, Core.Constants.PkgUnit.Package, 19, 21, "SHIPPING 2");
			var package3 = SetupPackage(CreatePackageMock(declaration).Object, container1.CO_ContainerNumber, "SH:MB2HB1SB1 (HB:MB2HB1)", "MARKS 3", 30, Core.Constants.PkgUnit.Piece, 29, 31, "SHIPPING 3");
			var childPackage1 = SetupPackage(CreatePackageMock(declaration).Object, container2.CO_ContainerNumber, "HB:MB1HB2 (MB:MB1)", "CHILDMARKS 1", 40, Core.Constants.PkgUnit.Package, 39, 41, "CHILDSHIPPING 1");
			childPackage1.CW_CW_Parent = package2.PK;
			var childPackage2 = SetupPackage(CreatePackageMock(declaration).Object, container2.CO_ContainerNumber, "HB:MB1HB2 (MB:MB1)", "CHILDMARKS 2", 50, Core.Constants.PkgUnit.Piece, 49, 51, "CHILDSHIPPING 2");
			childPackage2.CW_CW_Parent = package2.PK;
			var childPackage3 = SetupPackage(CreatePackageMock(declaration).Object, container1.CO_ContainerNumber, "SH:MB2HB1SB1 (HB:MB2HB1)", "CHILDMARKS 3", 60, Core.Constants.PkgUnit.Box, 59, 61, "CHILDSHIPPING 3");
			childPackage3.CW_CW_Parent = package3.PK;
			var childChildPackage1 = SetupPackage(CreatePackageMock(declaration).Object, container2.CO_ContainerNumber, "HB:MB1HB2 (MB:MB1)", "CHILDCHILDMARKS 1", 70, Core.Constants.PkgUnit.Package, 69, 71, "CHILDCHILDSHIPPING 1");
			childChildPackage1.CW_CW_Parent = childPackage1.PK;

			var note1 = declaration.Notes.AddNew(true, "SILLY DATA 2", "GOODBYE WORLD");
			var note2 = declaration.Notes.AddNew(true, "SILLY DATA 1", "HELLO WORLD");

			var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = org2.MainAddress.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var topGroupInvoice = declaration.TopGroupInvoice;
			var groupCharge1 = topGroupInvoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 1000m, localCurrencyCode);
			var groupCharge2 = topGroupInvoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 500m, localCurrencyCode);
			var invoice1Mock = CreateInvoiceMock(declaration);
			var invoice1 = SetupJobComInvoiceHeader(invoice1Mock.Object);
			var charge1 = invoice1.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, localCurrencyCode);
			var charge2 = invoice1.Charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 50m, localCurrencyCode);
			var lineCharge1 = invoice1.JobComInvoiceLines[0].Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 10m, localCurrencyCode);
			var invoice2Mock = CreateInvoiceMock(declaration);
			var invoice2 = SetupJobComInvoiceHeader2(invoice2Mock.Object);

			var additionalReferenceNumber1 = SetupCusEntryNumber(declaration.AdditionalReferenceNumbers.AddNew(), "IT123", UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, "ITDEC", false, Core.Constants.CountryCodes.UnitedStates, new ZDateTime(2011, 6, 3));
			var additionalReferenceNumber2 = SetupCusEntryNumber(declaration.AdditionalReferenceNumbers.AddNew(), "CCN123", CanadaAdditionalReferenceNumberTypes.Codes.CCN, "CCNDEC", true, Core.Constants.CountryCodes.Canada, new ZDateTime(2011, 6, 5));

			var entryNumber1 = SetupCusEntryNumber(Factory.New<CusEntryNumber>(), "IMP123", JobMessageTypeList.Codes.Import, "IMPDEC", true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2011, 7, 5));
			entryNumber1.Parent = declaration;
			var entryNumber2 = SetupCusEntryNumber(Factory.New<CusEntryNumber>(), "EXP123", JobMessageTypeList.Codes.Export, "EXPDEC", false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2011, 7, 4));
			entryNumber2.Parent = declaration;

			var entryHeaderMock1 = CreateCusEntryHeaderMock(declaration);
			var entryHeader1 = SetupCusEntryHeader(entryHeaderMock1.Object);

			var entryHeaderMock2 = CreateCusEntryHeaderMock(declaration);
			var entryHeader2 = SetupCusEntryHeaderWithRelatedEntry(entryHeaderMock2.Object);
			declaration.WarehouseTransactionStatus = "WR1";

			return declaration;
		}

		RefServiceLevel ServiceLevel1
		{
			get
			{
				if (serviceLevel1 == null)
				{
					serviceLevel1 = Factory.New<RefServiceLevel>();
					serviceLevel1.RS_Code = "U3!";
					serviceLevel1.RS_Description = "US SERVICE LEVEL 1";
				}
				return serviceLevel1;
			}
		}
		RefServiceLevel serviceLevel1;

		CodeDescriptionPairList PackTypeList
		{
			get
			{
				return Factory.GetCachedValue("DummyPackTypeList",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Core.Constants.PkgUnit.Box, "Box");
						result.AddPair(Core.Constants.PkgUnit.Package, "Package");
						result.AddPair(Core.Constants.PkgUnit.Piece, "Piece");
						return result;
					});
			}
		}

		Mock<BasePackage> CreatePackageMock(BaseJobDeclaration declaration)
		{
			var packageMock = Factory.NewMoq<BasePackage>();
			var package = packageMock.Object;
			packageMock.Setup(m => m.PackTypeList).Returns(PackTypeList);
			declaration.Packages.Add(package);
			return packageMock;
		}

		RefUNLOCO SeaLocalPort1
		{
			get
			{
				if (seaLocalPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					seaLocalPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaLocalPort1;
			}
		}
		RefUNLOCO seaLocalPort1;

		RefUNLOCO SeaLocalPort2
		{
			get
			{
				if (seaLocalPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, SeaLocalPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					seaLocalPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaLocalPort2;
			}
		}
		RefUNLOCO seaLocalPort2;

		RefUNLOCO SeaForeignPort1
		{
			get
			{
				if (seaForeignPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					seaForeignPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaForeignPort1;
			}
		}
		RefUNLOCO seaForeignPort1;

		RefUNLOCO SeaForeignPort2
		{
			get
			{
				if (seaForeignPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, SeaForeignPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					seaForeignPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaForeignPort2;
			}
		}
		RefUNLOCO seaForeignPort2;

		RefUNLOCO SeaForeignPort3
		{
			get
			{
				if (seaForeignPort3 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, new[] { SeaForeignPort1.PK, SeaForeignPort2.PK });
					query.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, true);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					seaForeignPort3 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return seaForeignPort3;
			}
		}
		RefUNLOCO seaForeignPort3;

		Transport SetupTransport(Transport transport, ZString transportMode, ZString vessel, ZString voyageFlight, ZString loadPort, ZString discPort)
		{
			transport.JW_TransportMode = transportMode;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyageFlight;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_RL_NKDiscPort = discPort;
			return transport;
		}

		void AssertContents(Shipment declarationData)
		{
			AssertContents(declarationData, "MB1HB1", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), GetCodeDescriptionPair(Core.Constants.ContainerModes.Containerised, "Containerized"), 2,
				"FUNNY GOODS", GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Descriptions.Export), GetCodeDescriptionPair("ST1", "STANDARD"), 100, GetCodeDescriptionPair(Core.Constants.PkgUnit.Piece, "Piece"),
				10.50m, GetCodeDescriptionPair(Core.Constants.Volume.CubicFeet, "Cubic Feet"), 1506.682m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), GetCodeDescriptionPair(Core.Constants.TransportModes.Sea, "Sea Freight"),
				GetCodeDescriptionPair(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), GetCodeDescriptionPair(SeaLocalPort2.RL_Code, SeaLocalPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName), GetCodeDescriptionPair(SeaForeignPort2.RL_Code, SeaForeignPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort3.RL_Code, SeaForeignPort3.RL_PortName),
				"APL EMERALD", "V23W", GetCodeDescriptionPair(GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), GetCodeDescriptionPair(ServiceLevel1.RS_Code, ServiceLevel1.RS_Description), ZBool.True,
				GetCodeDescriptionPair("EFT", "EFT DATA"), GetCodeDescriptionPair(OrgConstants.MergeInvoiceLines.Tariff, "Tariff"), GetCodeDescriptionPair("OS1", "OPERATIONAL STATUS 1"), GetCodeDescriptionPair("MS1", "MESSAGE STATUS 1"), GetCodeDescriptionPair(CustomsEntryStatusList.Codes.AwaitingFDACorrection, CustomsEntryStatusList.Descriptions.AwaitingFDACorrection),
				GetCodeDescriptionPair("CC1", "CONSOLIDATED STATUS 1"), 112, 307, GetCodeDescriptionPair("WR1", "WAREHOUSERELEASE STATUS 1"), "7819369",
				GetCodeDescriptionPair("SP", "Spare parts for the vessel/aircraft"), "AGREF123", "OWN324", "F234", GetCodeDescriptionPair(PaymentPartyCodeDescriptionList.Codes.Broker, PaymentPartyCodeDescriptionList.Descriptions.Broker), GetCodeDescriptionPair(MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, MasterFiles.Business.Customs.PaidByCodeList.Descriptions.BRK),
				GetCodeDescriptionPair("FOB", "Free On Board"), 789.012m, GetCodeDescriptionPair(ScreeningStatusesList.Codes.Matched, "Matched"), GetCodeDescriptionPair("AB", "TEST AB Origin Territory"));

			AssertNotNull("declarationData.AdditionalBillCollection", declarationData.AdditionalBillCollection);
			AssertEquals("declarationData.AdditionalBillCollection.Count", 12, declarationData.AdditionalBillCollection.Count);
			AssertContents(declarationData.AdditionalBillCollection[0], "MB1", GetCodeDescriptionPair(WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master), new ZDateTime(2011, 5, 20), null, true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[1], "MB1HB1", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), new ZDateTime(2011, 5, 21), "MB1", true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[2], "MB1HB1SB1", GetCodeDescriptionPair(WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse), new ZDateTime(2011, 5, 22), "MB1HB1", false, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[3], "MB1HB1SB2", GetCodeDescriptionPair(WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse), new ZDateTime(2011, 5, 23), "MB1HB1", true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[4], "MB1HB2", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), new ZDateTime(2011, 5, 24), "MB1", true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[5], "MB1HB2SB1", GetCodeDescriptionPair(WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse), new ZDateTime(2011, 5, 25), "MB1HB2", false, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[6], "MB2", GetCodeDescriptionPair(WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master), new ZDateTime(2011, 5, 26), null, false, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[7], "MB2HB1", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), new ZDateTime(2011, 5, 27), "MB2", true, GetCodeDescriptionPair(ZString.Empty, null), 13.50m, GetCodeDescriptionPair(Core.Constants.PkgUnit.Box, "Box"));
			AssertContents(declarationData.AdditionalBillCollection[8], "MB2HB1SB1", GetCodeDescriptionPair(WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse), new ZDateTime(2011, 5, 28), "MB2HB1", false, GetCodeDescriptionPair("CLR", "CLEAR"), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[9], ZString.Empty, GetCodeDescriptionPair(WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse), new ZDateTime(2011, 5, 29), "MB2HB1", true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[10], ZString.Empty, GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), new ZDateTime(2011, 5, 30), "MB2", true, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(declarationData.AdditionalBillCollection[11], ZString.Empty, GetCodeDescriptionPair(WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master), new ZDateTime(2011, 5, 31), null, false, GetCodeDescriptionPair(ZString.Empty, null), ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));

			AssertNotNull("declarationData.ContainerCollection", declarationData.ContainerCollection);
			AssertEquals("declarationData.ContainerCollection.Count", 2, declarationData.ContainerCollection.Count);
			var container1 = declarationData.ContainerCollection[0];
			AssertContents(container1, "CONT1", 1500.50m, GetCodeDescriptionPair(Core.Constants.Weight.Kilograms, "Kilograms"), "C1S1", "C1S2", GetCodeDescriptionPair(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModeDescriptions.LCL), 10m, 11m, 12m, GetCodeDescriptionPair("CLR", "MESSAGE STATUS CLEAR"), null);
			AssertNotNull("container1.CustomizedFieldCollection", container1.CustomizedFieldCollection);
			container1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING1", "");
			container1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE1", "");
			container1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL1", "0");
			container1.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "FLAG1", "false");

			var container2 = declarationData.ContainerCollection[1];
			AssertContents(container2, "CONT2", 2.250m, GetCodeDescriptionPair(Core.Constants.Weight.Tonnes, "Tonnes"), "C2S1", "C2S2", GetCodeDescriptionPair(Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModeDescriptions.FCL), 20m, 21m, 22m, GetCodeDescriptionPair("MS1", "MESSAGE STATUS 1"), null);
			AssertNotNull("container2.CustomizedFieldCollection", container2.CustomizedFieldCollection);
			container2.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING1", "COATT1");
			container2.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE1", new ZDateTime(2011, 8, 18).ToISO8601String());
			container2.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL1", "30.2");
			container2.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "FLAG1", "true");

			AssertNotNull("declarationData.PackingLineCollection", declarationData.PackingLineCollection);
			AssertEquals("declarationData.PackingLineCollection.Count", 3, declarationData.PackingLineCollection.Count);
			AssertContents(declarationData.PackingLineCollection[0], "CONT1", "MB1", GetCodeDescriptionPair(WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master), "MARKS 1", 10, GetCodeDescriptionPair(Core.Constants.PkgUnit.Box, "Box"), 11, 9, "SHIPPING");
			AssertContents(declarationData.PackingLineCollection[1], "CONT2", "MB1HB2", GetCodeDescriptionPair(WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House), "MARKS 2", 20, GetCodeDescriptionPair(Core.Constants.PkgUnit.Package, "Package"), 19, 21, "SHIPPING 2");
			AssertContents(declarationData.PackingLineCollection[2], "CONT1", "MB2HB1SB1", GetCodeDescriptionPair(WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse), "MARKS 3", 30, GetCodeDescriptionPair(Core.Constants.PkgUnit.Piece, "Piece"), 29, 31, "SHIPPING 3");

			AssertNotNull("declarationData.TransportLegCollection", declarationData.TransportLegCollection);
			AssertEquals("declarationData.TransportLegCollection.Count", 4, declarationData.TransportLegCollection.Count);
			AssertContents(declarationData.TransportLegCollection[0], TransportMode.Rail, "", "SEK323", GetCodeDescriptionPair(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), GetCodeDescriptionPair(SeaLocalPort2.RL_Code, SeaLocalPort2.RL_PortName));
			AssertContents(declarationData.TransportLegCollection[1], TransportMode.Sea, "APL EMERALD", "V23W", GetCodeDescriptionPair(SeaLocalPort2.RL_Code, SeaLocalPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName));
			AssertContents(declarationData.TransportLegCollection[2], TransportMode.Sea, "APL EMERALD", "V24W", GetCodeDescriptionPair(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName), GetCodeDescriptionPair(SeaForeignPort2.RL_Code, SeaForeignPort2.RL_PortName));
			AssertContents(declarationData.TransportLegCollection[3], TransportMode.Road, "", "CAR-123", GetCodeDescriptionPair(SeaForeignPort2.RL_Code, SeaForeignPort2.RL_PortName), GetCodeDescriptionPair(SeaForeignPort3.RL_Code, SeaForeignPort3.RL_PortName));

			AssertNotNull("declarationData.NoteCollection", declarationData.NoteCollection);
			AssertEquals("declarationData.NoteCollection.Count", 2, declarationData.NoteCollection.Count);
			AssertContents(declarationData.NoteCollection[0], true, "SILLY DATA 1", "HELLO WORLD");
			AssertContents(declarationData.NoteCollection[1], true, "SILLY DATA 2", "GOODBYE WORLD");

			AssertNotNull("declarationData.OrganizationAddressCollection", declarationData.OrganizationAddressCollection);
			AssertEquals("declarationData.OrganizationAddressCollection.Count", 5, declarationData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("SupplierDocumentaryAddress", declarationData.OrganizationAddressCollection[1], "SupplierDocumentaryAddress", true);
			AssertOrganizationBO_WUFSHIJNB("SupplierPickupDeliveryAddress", declarationData.OrganizationAddressCollection[2], "SupplierPickupDeliveryAddress", false);
			AssertOrganizationBO_CRAHOLSYD("ImporterDocumentaryAddress", declarationData.OrganizationAddressCollection[3], "ImporterDocumentaryAddress", true);
			AssertOrganizationBO_CRAHOLSYD("ImporterPickupDeliveryAddress", declarationData.OrganizationAddressCollection[4], "ImporterPickupDeliveryAddress", false);
			AssertOrganizationBO_CRAHOLSYD("SendersLocalClient", declarationData.OrganizationAddressCollection[0], "SendersLocalClient");

			AssertNotNull("declarationData.CommercialInfo", declarationData.CommercialInfo);
			AssertNotNull("declarationData.ChargeCollection", declarationData.CommercialInfo.CommercialChargeCollection);
			AssertEquals("declarationData.CommercialInfo.ChargeCollection.Count", 2, declarationData.CommercialInfo.CommercialChargeCollection.Count);

			AssertNotNull("declarationData.InvoiceCollection", declarationData.CommercialInfo.CommercialInvoiceCollection);
			AssertEquals("declarationData.CommercialInfo.InvoiceCollection.Count", 2, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoice1 = declarationData.CommercialInfo.CommercialInvoiceCollection[0];
			var refCurrency = BaseJobComInvoiceHeader.GetLocalCurrencyFor(null);
			var localCurrency = GetCodeDescriptionPair(refCurrency.RX_Code, refCurrency.RX_Desc);

			AssertContents(declarationData.CommercialInfo.CommercialChargeCollection[0], ZBool.False, 1000m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(declarationData.CommercialInfo.CommercialChargeCollection[1], ZBool.False, 500m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));

			AssertContents(invoice1);
			AssertNotNull("invoice1.CommercialChargeCollection", invoice1.CommercialChargeCollection);
			AssertEquals("invoice1.CommercialChargeCollection.Count", 5, invoice1.CommercialChargeCollection.Count);
			AssertContents(invoice1.CommercialChargeCollection[0], ZBool.False, 50m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.AdditionCharge, Common.CustomsChargeTypeList.Descriptions.AdditionCharge, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Collect, "Collect"));
			AssertContents(invoice1.CommercialChargeCollection[1], ZBool.False, 10m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.Discount, Common.CustomsChargeTypeList.Descriptions.Discount, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoice1.CommercialChargeCollection[2], ZBool.False, 449.34m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoice1.CommercialChargeCollection[3], ZBool.False, 224.67m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, GetCodeDescriptionPair(ZString.Empty, null));
			AssertContents(invoice1.CommercialChargeCollection[4], ZBool.False, 100m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OtherCharges, Common.CustomsChargeTypeList.Descriptions.OtherCharges, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			var invoice2 = declarationData.CommercialInfo.CommercialInvoiceCollection[1];
			AssertContents2(invoice2);
			AssertNotNull("invoice2.CommercialChargeCollection", invoice2.CommercialChargeCollection);
			AssertEquals("invoice2.CommercialChargeCollection.Count", 4, invoice2.CommercialChargeCollection.Count);

			AssertContents(invoice2.CommercialChargeCollection[0], ZBool.False, 550.66m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasFreight, Common.CustomsChargeTypeList.Descriptions.OverseasFreight, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));
			AssertContents(invoice2.CommercialChargeCollection[2], ZBool.False, 275.33m, GetCodeDescriptionPair(Common.CustomsChargeTypeList.Codes.OverseasInsurance, Common.CustomsChargeTypeList.Descriptions.OverseasInsurance, 35), localCurrency, GetCodeDescriptionPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value), GetCodeDescriptionPair(ZString.Empty, null), 1, GetCodeDescriptionPair(ApportionmentTypeList.Codes.PartialApportionment, ApportionmentTypeList.Descriptions.PartialApportionment), ZBool.True, ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, GetCodeDescriptionPair(Core.Constants.PaymentType.Prepaid, "Prepaid"));

			AssertNotNull("declarationData.EntryNumberCollection", declarationData.EntryNumberCollection);
			AssertEquals("declarationData.EntryNumberCollection.Count", 2, declarationData.EntryNumberCollection.Count);

			AssertContents(declarationData.EntryNumberCollection[0], GetCodeDescriptionPair(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Desc), true, "IMPDEC", new ZDateTime(2011, 7, 5), "IMP123", GetCodeDescriptionPair(JobMessageTypeList.Codes.Import, null));
			AssertContents(declarationData.EntryNumberCollection[1], GetCodeDescriptionPair(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Desc), false, "EXPDEC", new ZDateTime(2011, 7, 4), "EXP123", GetCodeDescriptionPair(JobMessageTypeList.Codes.Export, null));

			AssertNotNull("declarationData.AdditionalReferenceCollection", declarationData.AdditionalReferenceCollection);
			AssertEquals("declarationData.AdditionalReferenceCollection.Count", 2, declarationData.AdditionalReferenceCollection.Count);
			AssertContents(declarationData.AdditionalReferenceCollection[0], "ITDEC", new ZDateTime(2011, 6, 3), "IT123", GetCodeDescriptionPair(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, UnitedStatesAdditionalReferenceNumberTypes.Descriptions.IT));
			AssertContents(declarationData.AdditionalReferenceCollection[1], "CCNDEC", new ZDateTime(2011, 6, 5), "CCN123", GetCodeDescriptionPair(CanadaAdditionalReferenceNumberTypes.Codes.CCN, CanadaAdditionalReferenceNumberTypes.Descriptions.CCN));

			AssertNotNull("declarationData.DateCollection", declarationData.DateCollection);
			AssertEquals("declarationData.DateCollection.Count", 10, declarationData.DateCollection.Count);
			AssertContents(declarationData.DateCollection[9], DateType.BillIssued, new ZDateTime(2011, 5, 21), ZBool.False);
			AssertContents(declarationData.DateCollection[0], DateType.Departure, new ZDateTime(2011, 7, 17), ZBool.True);
			AssertContents(declarationData.DateCollection[1], DateType.LoadingDate, new ZDateTime(2011, 6, 12), ZBool.False);
			AssertContents(declarationData.DateCollection[2], DateType.FirstArrivalInCountry, new ZDateTime(2011, 6, 11), ZBool.False);
			AssertContents(declarationData.DateCollection[3], DateType.DischargeDate, new ZDateTime(2011, 6, 10), ZBool.False);
			AssertContents(declarationData.DateCollection[4], DateType.Arrival, new ZDateTime(2011, 7, 13), ZBool.True);
			AssertContents(declarationData.DateCollection[5], DateType.EntrySubmitted, new ZDateTime(2011, 7, 14), ZBool.False);
			AssertContents(declarationData.DateCollection[6], DateType.EntryAuthorisation, new ZDateTime(2011, 7, 15), ZBool.False);
			AssertContents(declarationData.DateCollection[7], DateType.WarehouseRelease, new ZDateTime(2011, 7, 16), ZBool.False);
			AssertContents(declarationData.DateCollection[8], DateType.EntryDate, new ZDateTime(2011, 7, 18), ZBool.False);

			AssertNotNull("declarationData.EntryHeaderCollection", declarationData.EntryHeaderCollection);
			AssertEquals("declarationData.EntryHeaderCollection.Count", 2, declarationData.EntryHeaderCollection.Count);
			var entryHeader1 = declarationData.EntryHeaderCollection[0];
			var entryHeader2 = declarationData.EntryHeaderCollection[1];
			if (entryHeader2.RelatedEntryHeaderCollection != null)
			{
				entryHeader1 = declarationData.EntryHeaderCollection[1];
				entryHeader2 = declarationData.EntryHeaderCollection[0];
			}
			AssertCusEntryHeaderWithRelatedEntry(entryHeader1);
			AssertContents(entryHeader2);
		}

		void AssertContents(PackedItem packedItemData, ZDecimal? grossWeight, ZDecimal? netWeight, ZDecimal? goodsValue, ZDecimal? packedQuantity)
		{
			CombineAssertions(delegate
			{
				AssertEquals("packedItemData.GrossWeight", grossWeight, packedItemData.GrossWeight);
				AssertEquals("packedItemData.GrossWeightUnit.Code", Core.Constants.Weight.Kilograms, packedItemData.GrossWeightUnit.Code);
				AssertEquals("packedItemData.NetWeight", netWeight, packedItemData.NetWeight);
				AssertEquals("packedItemData.NetWeightUnit.Code", Core.Constants.Weight.Kilograms, packedItemData.NetWeightUnit.Code);
				AssertEquals("packedItemData.GoodsValue", goodsValue, packedItemData.GoodsValue);
				AssertEquals("packedItemData.PackedQuantity", packedQuantity, packedItemData.PackedQuantity);
			});
		}

		void AssertContents(Date dateDataObject, DateType type, ZDateTime dateTime, ZBool isEstimate)
		{
			AssertNotNull("Precondition: dateDataObject", dateDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("dateDataObject.Type", type, dateDataObject.Type);
				AssertEquals("dateDataObject.Value", dateTime, dateDataObject.Value);
				AssertEquals("dateDataObject.IsEstimate", isEstimate, dateDataObject.IsEstimate);
			});
		}

		void AssertContents(AdditionalReference additionalReferenceDataObject, ZString contextInformation, ZDateTime issueDate, ZString referenceNumber, ICodeDescription type)
		{
			AssertNotNull("Precondition: additionalReferenceDataObject", additionalReferenceDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("entryNumberDataObject.ContextInformation", contextInformation, additionalReferenceDataObject.ContextInformation);
				AssertEquals("entryNumberDataObject.IssueDate", issueDate, additionalReferenceDataObject.IssueDate);
				AssertEquals("entryNumberDataObject.ReferenceNumber", referenceNumber, additionalReferenceDataObject.ReferenceNumber);
				AssertNotNull("entryNumberDataObject.Type", additionalReferenceDataObject.Type);
				AssertEquals("entryNumberDataObject.Type.Code", type.Code, additionalReferenceDataObject.Type.Code);
				AssertEquals("entryNumberDataObject.Type.Description", type.Description, additionalReferenceDataObject.Type.Description);
			});
		}

		void AssertContents(EntryNumber entryNumberDataObject, ICodeDescription countryOfIssue, ZBool entryIsSystemGenerated, ZString entryLineReference, ZDateTime issueDate, ZString number, ICodeDescription type)
		{
			AssertNotNull("Precondition: entryNumberDataObject", entryNumberDataObject);
			CombineAssertions(delegate
			{
				AssertNotNull("entryNumberDataObject.CountryOfIssue", entryNumberDataObject.CountryOfIssue);
				AssertEquals("entryNumberDataObject.CountryOfIssue.Code", countryOfIssue.Code, entryNumberDataObject.CountryOfIssue.Code);
				AssertEquals("entryNumberDataObject.CountryOfIssue.Name", countryOfIssue.Description, entryNumberDataObject.CountryOfIssue.Name);
				AssertEquals("entryNumberDataObject.EntryIsSystemGenerated", entryIsSystemGenerated, entryNumberDataObject.EntryIsSystemGenerated);
				AssertEquals("entryNumberDataObject.IssueDate", issueDate, entryNumberDataObject.IssueDate);
				AssertEquals("entryNumberDataObject.Number", number, entryNumberDataObject.Number);
				AssertNotNull("entryNumberDataObject.Type", entryNumberDataObject.Type);
				AssertEquals("entryNumberDataObject.Type.Code", type.Code, entryNumberDataObject.Type.Code);
				AssertEquals("entryNumberDataObject.Type.Description", type.Description, entryNumberDataObject.Type.Description);
			});
		}

		void AssertContents(Note noteData, ZBool isCustomDescription, ZString description, ZString noteText)
		{
			AssertNotNull("Precondition: noteData", noteData);
			CombineAssertions(delegate
			{
				AssertEquals("noteData.IsCustomDescription", isCustomDescription, noteData.IsCustomDescription);
				AssertEquals("noteData.Description", description, noteData.Description);
				AssertEquals("noteData.NoteText", noteText, noteData.NoteText);
			});
		}

		void AssertContents(TransportLeg transportLegData, TransportMode transportMode, ZString vesselName, ZString voyageFlightNo, ICodeDescription portOfLoading, ICodeDescription portOfDischarge)
		{
			AssertNotNull("Precondition: transportLegData", transportLegData);
			CombineAssertions(delegate
			{
				AssertEquals("transportLegData.TransportMode", transportMode, transportLegData.TransportMode);
				AssertEquals("transportLegData.VesselName", vesselName, transportLegData.VesselName);
				AssertEquals("transportLegData.VoyageFlightNo", voyageFlightNo, transportLegData.VoyageFlightNo);
				AssertNotNull("transportLegData.PortOfLoading", transportLegData.PortOfLoading);
				AssertEquals("transportLegData.PortOfLoading.Code", portOfLoading.Code, transportLegData.PortOfLoading.Code);
				AssertEquals("transportLegData.PortOfLoading.Name", portOfLoading.Description, transportLegData.PortOfLoading.Name);
				AssertNotNull("transportLegData.PortOfDischarge", transportLegData.PortOfDischarge);
				AssertEquals("transportLegData.PortOfDischarge.Code", portOfDischarge.Code, transportLegData.PortOfDischarge.Code);
				AssertEquals("transportLegData.PortOfDischarge.Name", portOfDischarge.Description, transportLegData.PortOfDischarge.Name);
			});
		}

		void AssertContents(PackingLine packingLineData, ZString? containerNumber, ZString billNumber, ICodeDescription billType, ZString marksAndNos, ZLong packQty, ICodeDescription packType, ZInt inBondPackQty, ZInt outerPacks, ZString shippingSymbol)
		{
			AssertNotNull("Precondition: packingLineData", packingLineData);
			CombineAssertions(delegate
			{
				AssertEquals("packingLineData.ContainerNumber", containerNumber, packingLineData.ContainerNumber);
				AssertEquals("packingLineData.BillNumber", billNumber, packingLineData.BillNumber);
				AssertNotNull("packingLineData.BillType", packingLineData.BillType);
				AssertEquals("packingLineData.BillType.Code", billType.Code, packingLineData.BillType.Code);
				AssertEquals("packingLineData.BillType.Description", billType.Description, packingLineData.BillType.Description);
				AssertEquals("packingLineData.MarksAndNos", marksAndNos, packingLineData.MarksAndNos);
				AssertEquals("packingLineData.PackQty", packQty, packingLineData.PackQty);
				AssertNotNull("packingLineData.PackType", packingLineData.PackType);
				AssertEquals("packingLineData.PackType.Code", packType.Code, packingLineData.PackType.Code);
				AssertEquals("packingLineData.PackType.Description", packType.Description, packingLineData.PackType.Description);
				AssertEquals("packingLineData.InBondPackQty", inBondPackQty, packingLineData.InBondPackQty);
				AssertEquals("packingLineData.CustomsOuterPacks", outerPacks, packingLineData.CustomsOuterPacks);
				AssertEquals("packingLineData.ShippingSymbol", shippingSymbol, packingLineData.ShippingSymbol);
			});
		}

		void AssertContents(Container containerData, ZString containerNumber, ZDecimal goodsWeight, ICodeDescription weightUnit, ZString seal, ZString secondSeal, ICodeDescription containerMode, ZDecimal totalHeight, ZDecimal totalWidth, ZDecimal totalLength, ICodeDescription messageStatus, ICodeDescription containerSize)
		{
			AssertNotNull("Precondition: containerData", containerData);
			CombineAssertions(delegate
			{
				AssertEquals("containerData.ContainerNumber", containerNumber, containerData.ContainerNumber);
				AssertEquals("containerData.GoodsWeight", goodsWeight, containerData.GoodsWeight);
				AssertNotNull("containerData.WeightUnit", containerData.WeightUnit);
				AssertEquals("containerData.WeightUnit.Code", weightUnit.Code, containerData.WeightUnit.Code);
				AssertEquals("containerData.WeightUnit.Description", weightUnit.Description, containerData.WeightUnit.Description);
				AssertEquals("containerData.Seal", seal, containerData.Seal);
				AssertEquals("containerData.SecondSeal", secondSeal, containerData.SecondSeal);
				AssertNotNull("containerData.FCL_LCL_AIR", containerData.FCL_LCL_AIR);
				AssertEquals("containerData.FCL_LCL_AIR.Code", containerMode.Code, containerData.FCL_LCL_AIR.Code);
				AssertEquals("containerData.FCL_LCL_AIR.Description", containerMode.Description, containerData.FCL_LCL_AIR.Description);
				AssertEquals("containerData.TotalHeight", totalHeight, containerData.TotalHeight);
				AssertEquals("containerData.TotalWidth", totalWidth, containerData.TotalWidth);
				AssertEquals("containerData.TotalLength", totalLength, containerData.TotalLength);
				AssertNotNull("containerData.MessageStatus", containerData.MessageStatus);
				AssertEquals("containerData.MessageStatus.Code", messageStatus.Code, containerData.MessageStatus.Code);
				AssertEquals("containerData.MessageStatus.Description", messageStatus.Description, containerData.MessageStatus.Description);
				if (containerSize == null)
				{
					AssertNull(containerData.CustomsContainerSize);
				}
				else
				{
					AssertNotNull("containerData.CustomsContainerSize", containerData.CustomsContainerSize);
					AssertEquals("containerData.CustomsContainerSize.Code", containerSize.Code, containerData.CustomsContainerSize.Code);
					AssertEquals("containerData.CustomsContainerSize.Description", containerSize.Description, containerData.CustomsContainerSize.Description);
				}
			});
		}

		void AssertContents(AdditionalBill additionalBillData, ZString billNumber, ICodeDescription billType, ZDateTime issueDate, ZString? parentBillNumber, bool hasAddInfoCollection, ICodeDescription messageStatus, ZDecimal noOfPacks, ICodeDescription packType)
		{
			AssertNotNull("Precondition: additionalBillData", additionalBillData);
			CombineAssertions(delegate
			{
				AssertEquals("additionalBillData.BillNumber", billNumber, additionalBillData.BillNumber);
				AssertNotNull("additionalBillData.BillType", additionalBillData.BillType);
				AssertEquals("additionalBillData.BillType.Code", billType.Code, additionalBillData.BillType.Code);
				AssertEquals("additionalBillData.BillType.Description", billType.Description, additionalBillData.BillType.Description);
				AssertEquals("additionalBillData.IssueDate", issueDate, additionalBillData.IssueDate);
				AssertEquals("additionalBillData.ParentBillNumber", parentBillNumber, additionalBillData.ParentBillNumber);

				if (hasAddInfoCollection)
				{
					AssertNotNull("additionalBillData.AddInfoCollection", additionalBillData.AddInfoCollection);
					if (billType.Code == WayBillTypeList.Codes.SubHouse)
					{
						var addInfoCollection = new List<AddInfo>(additionalBillData.AddInfoCollection);
						var addInfo = addInfoCollection.FirstOrDefault(x => x.Key.Value == Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber);
						AssertNotNull("additionalBillData.AddInfoCollection should contain one item for MasterBillNumber", addInfo);
						addInfoCollection.Remove(addInfo);
						AddInfoCollectionCreatorTest.AssertContents(addInfoCollection);
					}
					else
					{
						AddInfoCollectionCreatorTest.AssertContents(additionalBillData.AddInfoCollection);
					}
				}
				else
				{
					if (billType.Code == WayBillTypeList.Codes.SubHouse)
					{
						AssertEquals("additionalBillData.AddInfoCollection should contain one item for MasterBillNumber", 1, additionalBillData.AddInfoCollection.Count);
						AssertEquals("additionalBillData.AddInfoCollection should contain one item for MasterBillNumber", Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber, additionalBillData.AddInfoCollection[0].Key);
					}
					else
					{
						AssertNull("additionalBillData.AddInfoCollection", additionalBillData.AddInfoCollection);
					}
				}

				AssertNotNull("additionalBillData.MessageStatus", additionalBillData.MessageStatus);
				AssertEquals("additionalBillData.MessageStatus.Code", messageStatus.Code, additionalBillData.MessageStatus.Code);
				AssertEquals("additionalBillData.MessageStatus.Description", messageStatus.Description, additionalBillData.MessageStatus.Description);
				AssertEquals("additionalBillData.NoOfPacks", noOfPacks, additionalBillData.NoOfPacks);
				AssertNotNull("additionalBillData.PackType", additionalBillData.PackType);
				AssertEquals("additionalBillData.PackType.Code", packType.Code, additionalBillData.PackType.Code);
				AssertEquals("additionalBillData.PackType.Description", packType.Description, additionalBillData.PackType.Description);
			});
		}

		void AssertContents(Shipment declarationData, ZString? wayBillNumber, ICodeDescription wayBillType, ICodeDescription customsContainerMode, ZInt containerCount,
			ZString goodsDescription, ICodeDescription messageType, ICodeDescription messageSubType, ZInt outerPacks, ICodeDescription outerPacksPackageType,
			ZDecimal totalVolume, ICodeDescription totalVolumeUnit, ZDecimal totalWeight, ICodeDescription totalWeightUnit, ICodeDescription transportMode,
			ICodeDescription portOfOrigin, ICodeDescription portOfLoading, ICodeDescription portOfFirstArrival, ICodeDescription portOfDischarge, ICodeDescription portOfDestination,
			ZString vesselName, ZString voyageFlightNo, ICodeDescription branch, ICodeDescription serviceLevel, ZBool isPersonalEffects,
			ICodeDescription eftMode, ICodeDescription mergeBy, ICodeDescription operationalStatus, ICodeDescription messageStatus, ICodeDescription entryStatus,
			ICodeDescription consolidatedCargoStatus, ZInt totalNoOfPieces, ZInt totalNoOfPiecesLanded, ICodeDescription warehouseReleaseStatus, ZString lloydsIMO,
			ICodeDescription exportGoodsType, ZString agentsReference, ZString ownerReference, ZString folio, ICodeDescription paymentMethod, ICodeDescription paidBy,
			ICodeDescription shipmentIncoTerm, ZDecimal totalNoOfPacksDecimal, ICodeDescription screeningStatus, ICodeDescription goodsOrigin)
		{
			AssertNotNull("Precondition: declarationData", declarationData);

			CombineAssertions(delegate
			{
				AssertEquals("declarationData.WayBillNumber", wayBillNumber, declarationData.WayBillNumber);
				if (!wayBillNumber.HasValue)
				{
					AssertNull("declarationData.WayBillType", declarationData.WayBillType);
				}
				else
				{
					AssertNotNull("declarationData.WayBillType", declarationData.WayBillType);
					AssertEquals("declarationData.WayBillType.Code", wayBillType.Code, declarationData.WayBillType.Code);
					AssertEquals("declarationData.WayBillType.Description", wayBillType.Description, declarationData.WayBillType.Description);
				}
				AssertNotNull("declarationData.CustomsContainerMode", declarationData.CustomsContainerMode);
				AssertEquals("declarationData.CustomsContainerMode.Code", customsContainerMode.Code, declarationData.CustomsContainerMode.Code);
				AssertEquals("declarationData.CustomsContainerMode.Description", customsContainerMode.Description, declarationData.CustomsContainerMode.Description);
				AssertEquals("declarationData.ContainerCount", containerCount, declarationData.ContainerCount);
				AssertEquals("declarationData.GoodsDescription", goodsDescription, declarationData.GoodsDescription);

				AssertNotNull("declarationData.MessageType", declarationData.MessageType);
				AssertEquals("declarationData.MessageType.Code", messageType.Code, declarationData.MessageType.Code);
				AssertEquals("declarationData.MessageType.Description", messageType.Description, declarationData.MessageType.Description);

				AssertNotNull("declarationData.MessageSubType", declarationData.MessageSubType);
				AssertEquals("declarationData.MessageSubType.Code", messageSubType.Code, declarationData.MessageSubType.Code);
				AssertEquals("declarationData.MessageSubType.Description", messageSubType.Description, declarationData.MessageSubType.Description);

				AssertEquals("declarationData.OuterPacks", outerPacks, declarationData.OuterPacks);
				AssertNotNull("declarationData.OuterPacksPackageType", declarationData.OuterPacksPackageType);
				AssertEquals("declarationData.OuterPacksPackageType.Code", outerPacksPackageType.Code, declarationData.OuterPacksPackageType.Code);
				AssertEquals("declarationData.OuterPacksPackageType.Description", outerPacksPackageType.Description, declarationData.OuterPacksPackageType.Description);

				AssertEquals("declarationData.TotalVolume", totalVolume, declarationData.TotalVolume);
				AssertNotNull("declarationData.TotalVolumeUnit", declarationData.TotalVolumeUnit);
				AssertEquals("declarationData.TotalVolumeUnit.Code", totalVolumeUnit.Code, declarationData.TotalVolumeUnit.Code);
				AssertEquals("declarationData.TotalVolumeUnit.Description", totalVolumeUnit.Description, declarationData.TotalVolumeUnit.Description);
				AssertEquals("declarationData.TotalWeight", totalWeight, declarationData.TotalWeight);
				AssertNotNull("declarationData.TotalWeightUnit", declarationData.TotalWeightUnit);
				AssertEquals("declarationData.TotalWeightUnit.Code", totalWeightUnit.Code, declarationData.TotalWeightUnit.Code);
				AssertEquals("declarationData.TotalWeightUnit.Description", totalWeightUnit.Description, declarationData.TotalWeightUnit.Description);
				AssertNotNull("declarationData.TransportMode", declarationData.TransportMode);
				AssertEquals("declarationData.TransportMode.Code", transportMode.Code, declarationData.TransportMode.Code);
				AssertEquals("declarationData.TransportMode.Description", transportMode.Description, declarationData.TransportMode.Description);
				AssertNotNull("declarationData.PortOfOrigin", declarationData.PortOfOrigin);
				AssertEquals("declarationData.PortOfOrigin.Code", portOfOrigin.Code, declarationData.PortOfOrigin.Code);
				AssertEquals("declarationData.PortOfOrigin.Name", portOfOrigin.Description, declarationData.PortOfOrigin.Name);
				AssertNotNull("declarationData.PortOfLoading", declarationData.PortOfLoading);
				AssertEquals("declarationData.PortOfLoading.Code", portOfLoading.Code, declarationData.PortOfLoading.Code);
				AssertEquals("declarationData.PortOfLoading.Name", portOfLoading.Description, declarationData.PortOfLoading.Name);
				AssertNotNull("declarationData.PortOfFirstArrival", declarationData.PortOfFirstArrival);
				AssertEquals("declarationData.PortOfFirstArrival.Code", portOfFirstArrival.Code, declarationData.PortOfFirstArrival.Code);
				AssertEquals("declarationData.PortOfFirstArrival.Name", portOfFirstArrival.Description, declarationData.PortOfFirstArrival.Name);
				AssertNotNull("declarationData.PortOfDischarge", declarationData.PortOfDischarge);
				AssertEquals("declarationData.PortOfDischarge.Code", portOfDischarge.Code, declarationData.PortOfDischarge.Code);
				AssertEquals("declarationData.PortOfDischarge.Name", portOfDischarge.Description, declarationData.PortOfDischarge.Name);
				AssertNotNull("declarationData.PortOfDestination", declarationData.PortOfDestination);
				AssertEquals("declarationData.PortOfDiPortOfDestinationscharge.Code", portOfDestination.Code, declarationData.PortOfDestination.Code);
				AssertEquals("declarationData.PortOfDestination.Name", portOfDestination.Description, declarationData.PortOfDestination.Name);
				AssertEquals("declarationData.VesselName", vesselName, declarationData.VesselName);
				AssertEquals("declarationData.VoyageFlightNo", voyageFlightNo, declarationData.VoyageFlightNo);
				AssertNotNull("declarationData.Branch", declarationData.Branch);
				AssertEquals("declarationData.Branch.Code", branch.Code, declarationData.Branch.Code);
				AssertEquals("declarationData.Branch.Name", branch.Description, declarationData.Branch.Name);
				AssertNotNull("declarationData.ServiceLevel", declarationData.ServiceLevel);
				AssertEquals("declarationData.ServiceLevel.Code", serviceLevel.Code, declarationData.ServiceLevel.Code);
				AssertEquals("declarationData.ServiceLevel.Description", serviceLevel.Description, declarationData.ServiceLevel.Description);
				AssertEquals("declarationData.IsPersonalEffects", isPersonalEffects, declarationData.IsPersonalEffects);
				AssertNotNull("declarationData.EFTMode", declarationData.EFTMode);
				AssertEquals("declarationData.EFTMode.Code", eftMode.Code, declarationData.EFTMode.Code);
				AssertEquals("declarationData.EFTMode.Description", eftMode.Description, declarationData.EFTMode.Description);
				AssertNotNull("declarationData.MergeBy", declarationData.MergeBy);
				AssertEquals("declarationData.MergeBy.Code", mergeBy.Code, declarationData.MergeBy.Code);
				AssertEquals("declarationData.MergeBy.Description", mergeBy.Description, declarationData.MergeBy.Description);
				AssertNotNull("declarationData.OperationalStatus", declarationData.OperationalStatus);
				AssertEquals("declarationData.OperationalStatus.Code", operationalStatus.Code, declarationData.OperationalStatus.Code);
				AssertEquals("declarationData.OperationalStatus.Description", operationalStatus.Description, declarationData.OperationalStatus.Description);
				AssertNotNull("declarationData.MessageStatus", declarationData.MessageStatus);
				AssertEquals("declarationData.MessageStatus.Code", messageStatus.Code, declarationData.MessageStatus.Code);
				AssertEquals("declarationData.MessageStatus.Description", messageStatus.Description, declarationData.MessageStatus.Description);
				AssertNotNull("declarationData.EntryStatus", declarationData.EntryStatus);
				AssertEquals("declarationData.EntryStatus.Code", entryStatus.Code, declarationData.EntryStatus.Code);
				AssertEquals("declarationData.EntryStatus.Description", entryStatus.Description, declarationData.EntryStatus.Description);
				AssertNotNull("declarationData.ConsolidatedCargoStatus", declarationData.ConsolidatedCargoStatus);
				AssertEquals("declarationData.ConsolidatedCargoStatus.Code", consolidatedCargoStatus.Code, declarationData.ConsolidatedCargoStatus.Code);
				AssertEquals("declarationData.ConsolidatedCargoStatus.Description", consolidatedCargoStatus.Description, declarationData.ConsolidatedCargoStatus.Description);
				AssertEquals("declarationData.TotalNoOfPieces", totalNoOfPieces, declarationData.TotalNoOfPieces);
				AssertEquals("declarationData.TotalNoOfPiecesLanded", totalNoOfPiecesLanded, declarationData.TotalNoOfPiecesLanded);
				AssertNotNull("declarationData.WarehouseReleaseStatus", declarationData.WarehouseReleaseStatus);
				AssertEquals("declarationData.WarehouseReleaseStatus.Code", warehouseReleaseStatus.Code, declarationData.WarehouseReleaseStatus.Code);
				AssertEquals("declarationData.WarehouseReleaseStatus.Description", warehouseReleaseStatus.Description, declarationData.WarehouseReleaseStatus.Description);
				AssertEquals("declarationData.LloydsIMO", lloydsIMO, declarationData.LloydsIMO);
				AssertNotNull("declarationData.ExportGoodsType", declarationData.ExportGoodsType);
				AssertEquals("declarationData.ExportGoodsType.Code", exportGoodsType.Code, declarationData.ExportGoodsType.Code);
				AssertEquals("declarationData.ExportGoodsType.Description", exportGoodsType.Description, declarationData.ExportGoodsType.Description);
				AssertEquals("declarationData.AgentsReference", agentsReference, declarationData.AgentsReference);
				AssertEquals("declarationData.OwnerRef", ownerReference, declarationData.OwnerRef);
				AssertEquals("declarationData.Folio", folio, declarationData.Folio);
				AssertNotNull("declarationData.PaymentMethod", declarationData.PaymentMethod);
				AssertNotNull("declarationData.PaidBy", declarationData.PaidBy);
				AssertEquals("declarationData.PaymentMethod.Code", paymentMethod.Code, declarationData.PaymentMethod.Code);
				AssertEquals("declarationData.PaymentMethod.Description", paymentMethod.Description, declarationData.PaymentMethod.Description);
				AssertEquals("declarationData.PaidBy.Code", paidBy.Code, declarationData.PaidBy.Code);
				AssertEquals("declarationData.PaidBy.Description", paidBy.Description, declarationData.PaidBy.Description);
				AssertNotNull("declarationData.ShipmentIncoTerm", declarationData.ShipmentIncoTerm);
				AssertEquals("declarationData.ShipmentIncoTerm.Code", shipmentIncoTerm.Code, declarationData.ShipmentIncoTerm.Code);
				AssertEquals("declarationData.ShipmentIncoTerm.Description", shipmentIncoTerm.Description, declarationData.ShipmentIncoTerm.Description);
				AssertEquals("declarationData.TotalNoOfPacksDecimal", totalNoOfPacksDecimal, declarationData.TotalNoOfPacksDecimal);
				AssertNotNull("declarationData.ScreeningStatus", declarationData.ScreeningStatus);
				AssertEquals("declarationData.ScreeningStatus.Code", screeningStatus.Code, declarationData.ScreeningStatus.Code);
				AssertEquals("declarationData.ScreeningStatus.Description", screeningStatus.Description, declarationData.ScreeningStatus.Description);
				AssertNotNull("declarationData.GoodsOrigin", declarationData.GoodsOrigin);
				AssertEquals("declarationData.GoodsOrigin.Code", goodsOrigin.Code, declarationData.GoodsOrigin.Code);
				AssertEquals("declarationData.GoodsOrigin.Description", goodsOrigin.Description, declarationData.GoodsOrigin.Description);

				AssertNotNull("Precondition: declarationData.AddInfoCollection", declarationData.AddInfoCollection);
				AddInfoCollectionCreatorTest.AssertContents(declarationData.AddInfoCollection);
			});
		}

		ICodeDescription GetCodeDescriptionPair(string code, string description, int maxLength = 0)
		{
			return CodeDescriptionPairForTesting.New(code, description, maxLength);
		}

		Shipment CreatePopulatedShipmentDataForMerge()
		{
			var declarationData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			declarationData.DataContext = DataContextFactory.New();
			declarationData.DataContext.AddDataSource(DataContextType.ForwardingShipment, "SHIPREF001");
			declarationData.WayBillNumber = "SB3423";
			declarationData.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse, Description = WayBillTypeList.Descriptions.SubHouse };
			declarationData.CustomsContainerMode = new ContainerMode() { Code = "CNT", Description = "CONTAINER MODE" };
			declarationData.ContainerCount = 2;
			declarationData.GoodsDescription = "SHIPMENT GOODS";
			declarationData.MessageType = new UniversalXml.CodeDescriptionPair() { Code = "SHP", Description = "SHIPMENT" };
			declarationData.MessageSubType = new UniversalXml.CodeDescriptionPair() { Code = "SBT", Description = "SHIPMENT SUB TYPE" };
			declarationData.OuterPacks = 123;
			declarationData.OuterPacksPackageType = new PackageType() { Code = "P1", Description = "P1 DESCRIPTION" };
			declarationData.TotalVolume = 13.32m;
			declarationData.TotalVolumeUnit = new UnitOfVolume() { Code = "V1", Description = "V1 DESRIPTION" };
			declarationData.TotalWeight = 1984.43m;
			declarationData.TotalWeightUnit = new UnitOfWeight() { Code = "W1", Description = "W1 DESCRIPTION" };
			declarationData.TransportMode = new UniversalXml.CodeDescriptionPair() { Code = "T12", Description = "T12 DESCRIPTION" };
			declarationData.PortOfOrigin = new UNLOCO() { Code = "ORG12", Name = "ORG12 NAME" };
			declarationData.PortOfLoading = new UNLOCO() { Code = "LOAD2", Name = "LOAD2 NAME" };
			declarationData.PortOfFirstArrival = new UNLOCO() { Code = "1ARV2", Name = "1ARV2 NAME" };
			declarationData.PortOfDischarge = new UNLOCO() { Code = "DIS43", Name = "DIS43 NAME" };
			declarationData.PortOfDestination = new UNLOCO() { Code = "DES98", Name = "DES98 NAME" };
			declarationData.VesselName = "SHP VESSEL";
			declarationData.VoyageFlightNo = "V531";
			declarationData.Branch = new Branch() { Code = "BR1", Name = "BR1 NAME" };
			declarationData.ServiceLevel = new ServiceLevel() { Code = "SL1", Description = "SL1 DESCRIPTION" };
			declarationData.IsPersonalEffects = false;
			declarationData.EFTMode = new UniversalXml.CodeDescriptionPair() { Code = "EM1", Description = "EM1 DESCRIPTION" };
			declarationData.MergeBy = new UniversalXml.CodeDescriptionPair() { Code = "MB4", Description = "MB4 DESCRIPTION" };
			declarationData.OperationalStatus = new UniversalXml.CodeDescriptionPair() { Code = "OS8", Description = "OS8 DESCRIPTION" };
			declarationData.MessageStatus = new UniversalXml.CodeDescriptionPair() { Code = "MS9", Description = "MS9 DESCRIPTION" };
			declarationData.EntryStatus = new EntryStatus() { Code = "ES2", Description = "ES2 DESCRIPTION" };
			declarationData.ConsolidatedCargoStatus = new UniversalXml.CodeDescriptionPair() { Code = "CS3", Description = "CS3 DESCRIPTION" };
			declarationData.TotalNoOfPieces = 435;
			declarationData.TotalNoOfPiecesLanded = 306;
			declarationData.WarehouseReleaseStatus = new UniversalXml.CodeDescriptionPair() { Code = "WS4", Description = "WS4 DESCRIPTION" };
			declarationData.LloydsIMO = "LYD2342";
			declarationData.ExportGoodsType = new UniversalXml.CodeDescriptionPair() { Code = "EGT", Description = "EGT DESCRIPTION" };
			declarationData.AgentsReference = "SHP AGT REF";
			declarationData.OwnerRef = "SHP OWN REF";
			declarationData.Folio = "32";
			declarationData.PaymentMethod = new UniversalXml.CodeDescriptionPair() { Code = "PM4", Description = "PM4 DESCRIPTION" };
			declarationData.PaidBy = new UniversalXml.CodeDescriptionPair() { Code = "BRK", Description = "Broker" };
			declarationData.ShipmentIncoTerm = new UniversalXml.IncoTerm() { Code = "SIT", Description = "SIT DESCRIPTION" };
			declarationData.TotalNoOfPacksDecimal = 1365.84m;
			declarationData.ScreeningStatus = new UniversalXml.CodeDescriptionPair() { Code = "SS5", Description = "SS5 DESCRIPTION" };
			declarationData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection("GHD=434*WHAT=WHERE"));
			declarationData.GoodsOrigin = new UniversalXml.CodeDescriptionPair() { Code = "AC", Description = "TEST AC Origin Territory" };

			var bill = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB1HB1",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House },
				IssueDate = new ZDateTime(2012, 4, 23, 4, 52, 2),
				MessageStatus = new UniversalXml.CodeDescriptionPair() { Code = "S23", Description = "S23 DESCRIPTION" },
				NoOfPacks = 43,
				PackType = new PackageType() { Code = "PT4", Description = "PT4 DESCRIPTION" },
				ParentBillNumber = "MD3234"
			};
			bill.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection("GHD=434*WHAT=WHERE"));
			declarationData.SetAdditionalBillCollection(() => new List<AdditionalBill> { bill });

			declarationData.SetContainerCollection(() =>
			{
				return new DataObjectList<Container>()
				{
					new Container(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ContainerNumber = "CONT1",
						GoodsWeight = 3492.5m,
						WeightCapacity = 3534m,
						WeightUnit = new UnitOfWeight() { Code = "WG4", Description = "WG4 DESCRIPTION" },
						Seal = "S123",
						SecondSeal = "S343",
						FCL_LCL_AIR = new ContainerMode() { Code = "FCL", Description = "FULL LOAD" },
						TotalHeight = 120m,
						TotalWidth = 130m,
						TotalLength = 140m,
						MessageStatus = new UniversalXml.CodeDescriptionPair() { Code = "CM1", Description = "CM1 DESC" },
						CustomsContainerSize = new CodeDescriptionPair2Char() { Code = "S3", Description = "S3 DESC" }
					}
				};
			});

			declarationData.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			declarationData.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				HarmonisedCode = "32432233",
				ContainerNumber = "CNT3459",
				BillNumber = "HB38987",
				BillType = new WayBillType() { Code = "GDH", Description = "GDH DESCRIPTION" },
				GoodsDescription = "PACK GOODS DESC",
				MarksAndNos = "SHP MARKS",
				PackQty = 101,
				PackType = new PackageType() { Code = "P3", Description = "P3 DESC" },
				InBondPackQty = 99,
				CustomsOuterPacks = 88,
				ShippingSymbol = "SHP SYMBOL"
			});

			declarationData.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			declarationData.TransportLegCollection.Add(new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransportMode = TransportMode.Sea,
				VesselName = "VESSEL 3234",
				VoyageFlightNo = "V598",
				PortOfLoading = new UNLOCO() { Code = "LOAD8", Name = "LOAD8 NAME" },
				PortOfDischarge = new UNLOCO() { Code = "DISC9", Name = "DISC9 NAME" },
			});

			declarationData.SetDateCollection(() =>
			{
				var dates = new List<Date>();
				dates.Add(Date.New(DateType.Departure, ZBool.True, new ZDateTime(2012, 7, 14)));
				dates.Add(Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2012, 7, 13)));
				dates.Add(Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2012, 8, 12)));
				dates.Add(Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2012, 8, 10)));
				return dates;
			});

			declarationData.SetNoteCollection(() => new DataObjectList<Note>());
			declarationData.NoteCollection?.Add(new Note() { Description = "SHIP DESC", IsCustomDescription = true, NoteText = "WHAT IS THIS" });

			declarationData.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ArrivalCartageRef = "Arrival Cartage Ref"
			};

			declarationData.SetOrganizationAddressCollection(() =>
			{
				var orgAddresses = new List<OrganizationAddress>();
				orgAddresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = "LocalClient",
					AddressShortCode = "BOBADDCODE",
					OrganizationCode = "BOBORGCODE",
					CompanyName = "BOB THE BUILDER"
				});

				var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				orgAddresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = "SupplierDocumentaryAddress",
					AddressShortCode = "WENADDCODE",
					OrganizationCode = "WENORGCODE",
					CompanyName = "WENDY THE DESTROYER"
				});
				return orgAddresses;
			});

			declarationData.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			declarationData.AdditionalReferenceCollection.Add(new AdditionalReference()
			{
				ReferenceNumber = "IT123",
				Type = new EntryType() { Code = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, Description = UnitedStatesAdditionalReferenceNumberTypes.Descriptions.IT },
				ContextInformation = "HB349",
				IssueDate = new ZDateTime(2012, 7, 4)
			});

			declarationData.SetEntryNumberCollection(() => new List<EntryNumber>());
			declarationData.EntryNumberCollection.Add(new EntryNumber()
			{
				Number = "IMP123",
				Type = new EntryType() { Code = JobMessageTypeList.Codes.Import },
				CountryOfIssue = new Country() { Code = GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Name = GlbCompany.CurrentCompany.Country.RN_Desc },
				EntryIsSystemGenerated = false,
				EntryLineReference = "IGD4545",
				EntryStatus = new EntryStatus() { Code = "ES9", Description = "ES9 DESC" },
				ExpiryDate = new ZDateTime(2012, 4, 30),
				IssueDate = new ZDateTime(2012, 4, 27)
			});

			return declarationData;
		}

		BaseJobDeclaration CreateDeclarationForMerge(string messageType = null)
		{
			var shipment = Factory.BOFactory.New<ForwardingShipment>();
			shipment.JS_RS_NKServiceLevel = "";
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var declarationBO = Factory.BOFactory.New<BaseJobDeclaration>();
			declarationBO.JE_OH_Supplier = org1.PK;
			declarationBO.JE_OH_Importer = org2.PK;
			declarationBO.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declarationBO.JE_ContainerCount = 2;
			declarationBO.JE_GoodsDescription = "FUNNY GOODS";
			declarationBO.JE_MessageType = messageType ?? JobMessageTypeList.Codes.Export;
			declarationBO.JE_MessageSubType = "ST1";
			declarationBO.JE_TotalNoOfPacks = 100;
			declarationBO.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Piece;
			declarationBO.JE_TotalVolume = 10.50m;
			declarationBO.JE_TotalVolumeUnit = Core.Constants.Volume.CubicFeet;
			declarationBO.JE_TotalWeight = 1506.682m;
			declarationBO.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			declarationBO.JE_TransportMode = declarationBO.TransportModeSeaCodeForTesting;
			declarationBO.Transports.RemoveAndDeleteAll();
			declarationBO.JE_RL_NKOrigin = SeaLocalPort1.RL_Code;
			declarationBO.JE_RL_NKPortOfLoading = SeaLocalPort2.RL_Code;
			declarationBO.JE_RL_NKPortOfFirstArrival = SeaForeignPort1.RL_Code;
			declarationBO.JE_RL_NKPortOfArrival = SeaForeignPort2.RL_Code;
			declarationBO.JE_RL_NKFinalDestination = SeaForeignPort3.RL_Code;
			declarationBO.JE_VesselName = "APL EMERALD";
			declarationBO.JE_VoyageFlightNo = "V23W";
			declarationBO.JE_RS_NKServiceLevel = ServiceLevel1.RS_Code;
			declarationBO.JE_IsPersonalEffects = ZBool.True;
			declarationBO.JE_EFTMode = "EFT";
			declarationBO.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declarationBO.JE_OperationalStatus = "OS1";
			declarationBO.JE_MessageStatus = "MS1";
			declarationBO.JE_EntryStatus = CustomsEntryStatusList.Codes.AwaitingFDACorrection;
			declarationBO.JE_ConsolidatedCargoStatus = "CC1";
			declarationBO.JE_TotalNoOfPieces = 112;
			declarationBO.JE_LandedPieces = 307;
			declarationBO.JE_ExportGoodsType = "SP";
			declarationBO.JE_AgentsReference = "AGREF123";
			declarationBO.JE_OwnerRef = "OWN324";
			declarationBO.JE_Folio = "F234";
			declarationBO.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			declarationBO.JE_PaidBy = MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK;
			declarationBO.JE_ShipmentIncoTerm = "FOB";
			declarationBO.JE_TotalNoOfPacksDecimal = 789.012m;
			declarationBO.JE_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			declarationBO.JE_GoodsOrigin = "AB";

			declarationBO.JE_MasterBill = "MB1";
			var masterBillBO = declarationBO.PrimaryMasterBill;
			masterBillBO.CU_IssueDate = new ZDateTime(2011, 5, 20);
			masterBillBO.CU_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;

			declarationBO.JE_HouseBill = "MB1HB1";
			var houseBillBO = declarationBO.PrimaryHouseBill;
			houseBillBO.CU_AddInfo = AddInfoCollectionCreatorTest.AddInfoStringForTesting;
			houseBillBO.CU_IssueDate = new ZDateTime(2011, 5, 21);

			declarationBO.JE_DateOfArrival = new ZDateTime(2011, 6, 10);
			declarationBO.JE_DateOfFirstArrival = new ZDateTime(2011, 6, 11);
			declarationBO.JE_ExportDate = new ZDateTime(2011, 6, 12);
			declarationBO.JE_DateAtFinalDestination = new ZDateTime(2011, 7, 13);
			declarationBO.JE_EntrySubmittedDate = new ZDateTime(2011, 7, 14);
			declarationBO.JE_EntryAuthorisationDate = new ZDateTime(2011, 7, 15);
			declarationBO.JE_WarehouseReleaseDate = new ZDateTime(2011, 7, 16);
			declarationBO.JE_DateAtOrigin = new ZDateTime(2011, 7, 17);
			declarationBO.JE_EntryDate = new ZDate(2011, 7, 18);

			var localCurrencyCode = declarationBO.LocalCurrencyCode;

			var containerBO = SetupCusContainer(declarationBO.CusContainers.AddNew(), "CONT1", 1500.50m, Core.Constants.Weight.Kilograms, "C1S1", "C1S2", Core.Constants.ContainerModes.LCL, RefContainer1.PK, "CLR", "21");
			var org = declarationBO.ConfigOrg;
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomAttribute1, "STRING1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomDate1, "DATE1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomDecimal1, "DECIMAL1");
			AddCustomsLabel(org, Core.Constants.CustomLabels.CusContainer.CustomFlag1, "FLAG1");
			containerBO.CO_CustomAttrib1 = "COATT1";
			containerBO.CO_CustomDate1 = new ZDateTime(2011, 8, 18);
			containerBO.CO_CustomDecimal1 = 30.2m;
			containerBO.CO_CustomFlag1 = ZBool.True;

			var packageBO = SetupPackage(declarationBO.Packages[0], containerBO.CO_ContainerNumber, "HB:MB1HB1 (MB:MB1)", "MARKS 1", 10, Core.Constants.PkgUnit.Box, 11, 9, "SHIPPING");

			var noteBO = declarationBO.Notes.AddNew(true, "SILLY DATA 2", "GOODBYE WORLD");

			var job = new JobHeader.Loader(declarationBO).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = org2.MainAddress.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var topGroupInvoiceBO = declarationBO.TopGroupInvoice;
			var groupChargeBO1 = topGroupInvoiceBO.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 1000m, localCurrencyCode);
			var groupChargeBO2 = topGroupInvoiceBO.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 500m, localCurrencyCode);
			var invoiceBO = SetupJobComInvoiceHeaderOnly(topGroupInvoiceBO.JobComInvoiceHeaders.AddNew(), "INV3243", org1.PK, org2.PK, 3420.34m, Core.Constants.CurrencyCodes.Australia, new ZDateTime(2011, 4, 3), Core.Constants.IncoTerms.FreeOnBoard, 14.72m, Core.Constants.Volume.CubicMetres, 2.53m, Core.Constants.Weight.Tonnes, 11.11m, Core.Constants.Weight.Kilograms, Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.25m, 1.50m, "P12345", 1500m, 1.75m, new ZDateTime(2011, 3, 3), CustomsEntryStatusList.Codes.ClearElectronicInvoiceOriginal, 10m);
			var invoiceLineBO = SetupJobComInvoiceLine(invoiceBO.JobComInvoiceLines.AddNew(), 1, "1010101010", "GOODS", 1040.50m, Core.Constants.PkgUnit.Box, 4140.53m, 4000.53m, "PART12", 3.2m, Core.Constants.Volume.CubicYards, 202.92m, Core.Constants.Weight.Hectograms, "ORDER1", 1.555m, Core.Constants.Weight.Tonnes, 10m, Core.Constants.PkgUnit.Package, 15m, Core.Constants.PkgUnit.Dozen, Core.Constants.CountryCodes.Australia, CommodityCode1.RH_Code, Core.Constants.ContainerModes.BreakBulk, "MK0001");
			var chargeBO = invoiceBO.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, localCurrencyCode);
			var lineChargeBO = invoiceLineBO.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 10m, localCurrencyCode);

			var additionalReferenceNumberBO = SetupCusEntryNumber(declarationBO.AdditionalReferenceNumbers.AddNew(), "IT123", UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, "ITDEC", false, Core.Constants.CountryCodes.UnitedStates, new ZDateTime(2011, 6, 3));

			var entryNumberBO = SetupCusEntryNumber(Factory.New<CusEntryNumber>(), "IMP123", JobMessageTypeList.Codes.Import, "IMPDEC", true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2011, 7, 5));
			entryNumberBO.Parent = declarationBO;

			var entryHeaderBO = SetupCusEntryHeaderOnly(declarationBO.CustomsEntryHeaders.AddNew(), new ZDateTime(2011, 2, 3), CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, CustomsEntryStatusList.Codes.ClearEntrySummaryOriginal, new ZDateTime(2011, 2, 2), 1404.24m, JobMessageTypeList.Codes.Export, "BDG34332", new ZDate(2011, 2, 7));
			SetupCusEntryHeaderCharges(entryHeaderBO.Charges.AddNew(), 236.45m, Core.Constants.USCustoms.FeeCodes.CountervailingDuty);
			var cusEntryNumberBO = SetupCusEntryNumber(Factory.BOFactory, "CE00001", JobMessageTypeList.Codes.Export, "REFERENCE", false);
			cusEntryNumberBO.Parent = entryHeaderBO;

			var entryLineBO = SetupCusEntryLineOnly(entryHeaderBO.MergedLines.AddNew(), "1010101010", 340.23m, 3, "HELLO WORLD", 391.53m, "KG", 72.23m, "ACT");
			SetupCusEntryLineFee(entryLineBO.Fees.AddNew(), 102.23m, Core.Constants.USCustoms.FeeCodes.Blueberry);

			declarationBO.SupplierDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			declarationBO.ImporterDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			declarationBO.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = org1.MainAddress.PK;
			declarationBO.DocsAndCartage.JP_OA_PickupCartageCoAddr = org2.MainAddress.PK;
			declarationBO.JE_OH_Forwarder = org1.PK;
			declarationBO.JE_OH_ShippingLine = org2.PK;

			declarationBO.JE_JS = shipment.PK;
			declarationBO.WarehouseTransactionStatus = "WR1";
			ErrorReporter.Clear();
			Factory.SaveForTesting();
			declarationBO.ResumeApportionment();
			return declarationBO;
		}

		void AssertLandedCostDetailContents(LandedCostDetail landedCostDetailData, ZDecimal goodsItemCostPerUnit, ZDecimal customsCostPerUnit, ZDecimal transportAndLogisticsCostPerUnit, ZDecimal markUp1, ZDecimal markUp2, ZDecimal markUp3)
		{
			AssertNotNull("Precondition: landedCostDetailData", landedCostDetailData);

			CombineAssertions(delegate
			{
				AssertEquals("landedCostDetailData.GoodsItemCostPerUnit", goodsItemCostPerUnit, landedCostDetailData.GoodsItemCostPerUnit);
				AssertEquals("landedCostDetailData.CustomsCostPerUnit", customsCostPerUnit, landedCostDetailData.CustomsCostPerUnit);
				AssertEquals("landedCostDetailData.TransportAndLogisticsCostPerUnit", transportAndLogisticsCostPerUnit, landedCostDetailData.TransportAndLogisticsCostPerUnit);
				AssertEquals("landedCostDetailData.MarkUp1", markUp1, landedCostDetailData.MarkUp1);
				AssertEquals("landedCostDetailData.MarkUp2", markUp2, landedCostDetailData.MarkUp2);
				AssertEquals("landedCostDetailData.MarkUp3", markUp3, landedCostDetailData.MarkUp3);
			});
		}

		void AssertLandedLineCostItemContents(LandedLineCostItem landedLineCostItemData, ICodeDescription costType, ZDecimal costAmount)
		{
			AssertNotNull("Precondition: landedLineCostItemData", landedLineCostItemData);

			CombineAssertions(delegate
			{
				AssertNotNull("landedLineCostItemData.CostType", landedLineCostItemData.CostType);
				AssertEquals("landedLineCostItemData.CostType.Code", costType.Code, landedLineCostItemData.CostType.Code);
				AssertEquals("landedLineCostItemData.CostType.Description", costType.Description, landedLineCostItemData.CostType.Description);
				AssertEquals("landedLineCostItemData.CostAmount", costAmount, landedLineCostItemData.CostAmount);
			});
		}

		void AssertTransportLogisticsCostContents(TransportLogisticsCost transportLogisticsCostData, ICodeDescription chargeCode, ZString? chargeDescription, ZDecimal? costAmount, ICodeDescription costCurrency, ICodeDescription distributeCostBy, ICodeDescription landedCostGroup, ZDecimal? serviceExRate)
		{
			AssertNotNull("Precondition: transportLogisticsCostData", transportLogisticsCostData);

			CombineAssertions(delegate
			{
				if (chargeCode == null)
				{
					AssertNull("transportLogisticsCostData.ChargeCode", transportLogisticsCostData.ChargeCode);
				}
				else
				{
					AssertNotNull("transportLogisticsCostData.ChargeCode", transportLogisticsCostData.ChargeCode);
					AssertEquals("transportLogisticsCostData.ChargeCode.Code", chargeCode.Code, transportLogisticsCostData.ChargeCode.Code);
					AssertEquals("transportLogisticsCostData.ChargeCode.Description", chargeCode.Description, transportLogisticsCostData.ChargeCode.Description);
				}
				AssertEquals("transportLogisticsCostData.ChargeDescription", chargeDescription, transportLogisticsCostData.ChargeDescription);
				AssertEquals("transportLogisticsCostData.CostAmount", costAmount, transportLogisticsCostData.CostAmount);
				if (costCurrency == null)
				{
					AssertNull("transportLogisticsCostData.CostCurrency", transportLogisticsCostData.CostCurrency);
				}
				else
				{
					AssertNotNull("transportLogisticsCostData.CostCurrency", transportLogisticsCostData.CostCurrency);
					AssertEquals("transportLogisticsCostData.CostCurrency.Code", costCurrency.Code, transportLogisticsCostData.CostCurrency.Code);
					AssertEquals("transportLogisticsCostData.CostCurrency.Description", costCurrency.Description, transportLogisticsCostData.CostCurrency.Description);
				}
				if (distributeCostBy == null)
				{
					AssertNull("transportLogisticsCostData.DistributeCostBy", transportLogisticsCostData.DistributeCostBy);
				}
				else
				{
					AssertNotNull("transportLogisticsCostData.DistributeCostBy", transportLogisticsCostData.DistributeCostBy);
					AssertEquals("transportLogisticsCostData.DistributeCostBy.Code", distributeCostBy.Code, transportLogisticsCostData.DistributeCostBy.Code);
					AssertEquals("transportLogisticsCostData.DistributeCostBy.Description", distributeCostBy.Description, transportLogisticsCostData.DistributeCostBy.Description);
				}
				if (landedCostGroup == null)
				{
					AssertNull("transportLogisticsCostData.LandedCostGroup", transportLogisticsCostData.LandedCostGroup);
				}
				else
				{
					AssertNotNull("transportLogisticsCostData.LandedCostGroup", transportLogisticsCostData.LandedCostGroup);
					AssertEquals("transportLogisticsCostData.LandedCostGroup.Code", landedCostGroup.Code, transportLogisticsCostData.LandedCostGroup.Code);
					AssertEquals("transportLogisticsCostData.LandedCostGroup.Description", landedCostGroup.Description, transportLogisticsCostData.LandedCostGroup.Description);
				}
				AssertEquals("transportLogisticsCostData.ServiceExRate", serviceExRate, transportLogisticsCostData.ServiceExRate);
			});
		}

		void AddChildItemToLandedCostHistory(LandedCosting.ILandedCostHistory landedCostingHistory, string costType, decimal costAmount)
		{
			var landedLineCostItem = Factory.BOFactory.New<LandedCosting.ILandedLineCostItem>();
			landedLineCostItem.LZ_CostAmount = costAmount;
			landedLineCostItem.LZ_CostType = costType;
			landedLineCostItem.LZ_LH = landedCostingHistory.PK;
		}

		void AssertAddressIsExported(DocAddressType addressType, Action<BaseJobDeclaration, OrgAddress> setter, bool isConsignee, bool isCosignor, bool isMainAddress = true)
		{
			AssertAddressIsExported(addressType.ToString(), setter, isConsignee, isCosignor, isMainAddress);
		}

		void AssertAddressIsExported(string addressType, Action<BaseJobDeclaration, OrgAddress> setter, bool isConsignee, bool isCosignor, bool isMainAddress = true)
		{
			var org = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			org.OH_IsConsignee = isConsignee;
			org.OH_IsConsignor = isCosignor;

			var address = isMainAddress ? org.MainAddress : org.Addresses.AddNew(OrgAddressType.Office, false);
			address.OA_Address1 = "ADR";
			address.OA_City = "AE";
			address.OA_PostCode = "0123";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MYMASTER";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;

			setter(declaration, address);

			Factory.SaveForTesting();

			IMergeDataObjectWriter writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));

			var declarationData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			writer.MergeData(declarationData, declaration);

			var dataObject = declarationData.OrganizationAddressCollection.FirstOrDefault(addressType);

			AssertEquals($"{addressType}.Address1", "ADR", dataObject.Address1);
			AssertEquals($"{addressType}.AddressShortCode", "ADR", dataObject.AddressShortCode.Value);
		}

		void CustomFieldsOnDeclarationPluggedIntoShipmentAreExported(bool useBrokerageDataFirstWhenExportUniversalXML)
		{
			ActivateFreightDataRegistryCustomAttributes();
			SetUpWorkflowCustomFields(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, "ShipmentWFField");
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.SetUserDefinedValue("Are you Happy?", ZBool.True);
			shipmentBO.SetUserDefinedValue("The Date You Are Happy", ZDateTime.Today);
			AddWorkflowCustomValue(shipmentBO.PK, "JS", "WFField", AddOnColumnDataType.Codes.String, "custom field from shipment workflow template");
			AddWorkflowCustomValue(shipmentBO.PK, "JS", "ShipmentWFField", AddOnColumnDataType.Codes.Integer, "10");

			var shipmentWriter = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
			var shipmentData = shipmentWriter.GetDataObject(shipmentBO);
			var shipmentCustomFields = shipmentData.CustomizedFieldCollection;
			AssertEquals("shipmentCustomFields.Count", 12, shipmentCustomFields.Count);
			AssertCustomFieldWasExported_ForwardingShipmentUserDefinedValue(shipmentCustomFields);
			AssertCustomFieldWasExported_RegistryCustomAttributes(shipmentCustomFields, true);
			AssertCustomFieldWasExported_ShipmentCustomFieldsFromWFTemplate(shipmentCustomFields);

			CreateJobDeclarationForExport(shipmentBO);

			shipmentData = shipmentWriter.GetDataObject(shipmentBO);
			shipmentCustomFields = shipmentData.CustomizedFieldCollection;
			var count = useBrokerageDataFirstWhenExportUniversalXML ? 28 : 19;
			AssertEquals("shipmentCustomFields.Count", count, shipmentCustomFields.Count);
			AssertCustomFieldWasExported_ForwardingShipmentUserDefinedValue(shipmentCustomFields);
			AssertCustomFieldWasExported_RegistryCustomAttributes(shipmentCustomFields, false); // Custom Attributes on declaration/shipment which were set value on declaration and no duplicate export
			AssertCustomFieldWasExported_ShipmentCustomFieldsFromWFTemplate(shipmentCustomFields);
			AssertCustomFieldWasExported_JobDeclarationUserDefinedValue(shipmentCustomFields);
			AssertCustomFieldWasExported_JobDeclarationCustomFieldsFromWFTemplate(shipmentCustomFields);
			if (useBrokerageDataFirstWhenExportUniversalXML)
			{
				AssertCustomFieldWasExported_JobDeclarationSupplier(shipmentCustomFields);
				AssertCustomFieldWasExported_JobDeclarationDocNote(shipmentCustomFields);
			}
		}

		void CustomFieldsOnDeclarationAreExported()
		{
			ActivateFreightDataRegistryCustomAttributes();
			var declarationBO = CreateJobDeclarationForExport();

			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declarationBO)));
			var declarationData = writer.GetDataObject(declarationBO);
			var customFields = declarationData.CustomizedFieldCollection;
			AssertEquals("JobDeclaration customFields.Count", 24, customFields.Count);
			AssertCustomFieldWasExported_JobDeclarationUserDefinedValue(customFields);
			AssertCustomFieldWasExported_JobDeclarationCustomFieldsFromWFTemplate(customFields);
			AssertCustomFieldWasExported_RegistryCustomAttributes(customFields, false);
			AssertCustomFieldWasExported_JobDeclarationSupplier(customFields);
			AssertCustomFieldWasExported_JobDeclarationDocNote(customFields);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				var countryData = declarationBO.Supplier.CountryDataCollectionForThisCompany.AddNew();
				countryData.OV_EXApprovalNumber = "RC423234";
				countryData.OV_EXApprovedOrMajorExporter = "AC";

				writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declarationBO)));
				declarationData = writer.GetDataObject(declarationBO);
				customFields = declarationData.CustomizedFieldCollection;
				AssertEquals("JobDeclaration customFields.Count", 25, customFields.Count);
				AssertCustomFieldWasExported_JobDeclarationUserDefinedValue(customFields);
				AssertCustomFieldWasExported_JobDeclarationCustomFieldsFromWFTemplate(customFields);
				AssertCustomFieldWasExported_RegistryCustomAttributes(customFields, false);
				AssertCustomFieldWasExported_JobDeclarationSupplier(customFields);
				AssertCustomFieldWasExported_JobDeclarationDocNote(customFields);
				customFields.AssertCustomFieldWasExported(DataType.String, DeclarationDataObjectWriter.SDFields.ApprovalNumber, "RC423234");
			}
		}

		BaseJobDeclaration CreateJobDeclarationForExport(ForwardingShipment shipment = null)
		{
			SetUpWorkflowCustomFields(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode, "DeclarationWFField");
			var declarationMock = CreateJobDeclarationMock();
			var declarationBO = SetupJobDeclaration(declarationMock.Object);
			if (shipment != null)
			{
				declarationBO.JE_JS = shipment.PK;
				declarationBO.JE_OverrideFreightDefaults = true;
			}

			// Custom fields can be set in three ways as below and we can get all of them by ICustomFieldProvider.GetCustomBusinessObject() on BO
			// setup user defined fields and the data saved in GenAddOnColumn
			declarationBO.SetUserDefinedValue("Are you Happy?", ZBool.True);
			declarationBO.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			declarationBO.SetUserDefinedValue("The Happy Number", new ZInt(42));
			declarationBO.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			declarationBO.SetUserDefinedValue("The Date You Are Happy", ZDateTime.BrettsBirthday);

			// setup workflow template custom fields and the data saved in GenCustomAddOnValue
			AddWorkflowCustomValue(declarationBO.PK, "JE", "DeclarationWFField", AddOnColumnDataType.Codes.Integer, "90");
			AddWorkflowCustomValue(declarationBO.PK, "JE", "WFField", AddOnColumnDataType.Codes.String, "custom field from declaration workflow template");

			// setup registry custom attributes and the data saved in JobDocsAndCartage
			SetupFreightDataRegistryCustomAttributes(declarationBO);

			SetupProFormaInvoiceDataIncludingSupplier(declarationBO);

			return declarationBO;
		}

		void ActivateFreightDataRegistryCustomAttributes()
		{
			var registryInstance = FreightDataRegistry.Instance;
			var emptyGuid = Guid.Empty;
			registryInstance.ShipmentCustomDate1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("First Date", ""));
			registryInstance.ShipmentCustomDate2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Last Date", ""));
			registryInstance.ShipmentCustomDecimalNo1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Deci Deca", ""));
			registryInstance.ShipmentCustomDecimalNo2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("+ 1 point zero", ""));
			registryInstance.ShipmentCustomFlag1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Flag this!", ""));
			registryInstance.ShipmentCustomFlag2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Flagger", ""));
			registryInstance.ShipmentCustomText1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Textual context", ""));
			registryInstance.ShipmentCustomText2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Customs are customary", ""));
		}

		void SetupFreightDataRegistryCustomAttributes(BaseJobDeclaration declarationBO)
		{
			declarationBO.DocsAndCartage.JP_CustomAttrib1 = "HELLO";
			declarationBO.DocsAndCartage.JP_CustomAttrib2 = "GOODBYE";
			declarationBO.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2011, 1, 1);
			declarationBO.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2011, 1, 2);
			declarationBO.DocsAndCartage.JP_CustomDecimal1 = 0.3;
			declarationBO.DocsAndCartage.JP_CustomDecimal2 = 1.3;
			declarationBO.DocsAndCartage.JP_CustomFlag1 = true;
			declarationBO.DocsAndCartage.JP_CustomFlag2 = true;
		}

		void SetUpWorkflowCustomFields(ZString processType, ZString fieldName)
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var factory = new BusinessObjectFactory();
			var template = factory.New<ProcessTaskTemplate>();
			template.P0_Name = processType + " Task Template";
			template.P0_ProcessType = processType;

			MasterFilesTestHelper.CreateCustomField(template, "WFField", AddOnColumnDataType.Codes.String);
			MasterFilesTestHelper.CreateCustomField(template, fieldName, AddOnColumnDataType.Codes.Integer);
			factory.Save();
		}

		void SetupProFormaInvoiceDataIncludingSupplier(BaseJobDeclaration declarationBO)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "ABC";
			orgHeader.MainAddress.OA_Address1 = "NJ";
			orgHeader.ExportersBankName = "TestBankName";
			orgHeader.ExportersBankAccount = "BK00001";
			orgHeader.ExportersSwiftCode = "SFT00001";
			orgHeader.MethodOfPayment = "Payment001";
			orgHeader.AdditionalInformation = "Additional Information";
			declarationBO.JE_OH_Supplier = orgHeader.PK;

			declarationBO.DocNote.SetSystemDefinedFieldValue(DeclarationDataObjectWriter.SDFields.InsurancePolicyNumber, "ISPY00001");
			declarationBO.DocNote.SetSystemDefinedFieldValue(DeclarationDataObjectWriter.SDFields.InsuredValue, "ISV00001");
			declarationBO.DocNote.SetSystemDefinedFieldValue(DeclarationDataObjectWriter.SDFields.LetterOfCreditDate, "CreditDate");
			declarationBO.DocNote.SetSystemDefinedFieldValue(DeclarationDataObjectWriter.SDFields.LetterOfCreditNumber, "CreditNumber");
		}

		void AddWorkflowCustomValue(ZGuid sourceBOId, ZString tableCode, ZString customValueName, ZString customValueType, ZString value)
		{
			GenCustomAddOnValue customAddOnValue = Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = sourceBOId;
			customAddOnValue.XV_ParentTableCode = tableCode;
			customAddOnValue.XV_Name = customValueName;
			customAddOnValue.XV_Type = customValueType;
			customAddOnValue.XV_Data = value;
		}

		void AssertCustomFieldWasExported_ForwardingShipmentUserDefinedValue(List<CustomizedField> customFields)
		{
			CombineAssertions(() =>
			{
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.Today.ToISO8601String());
			});
		}

		void AssertCustomFieldWasExported_JobDeclarationUserDefinedValue(List<CustomizedField> customFields)
		{
			CombineAssertions(() =>
			{
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "The Happy Number", "42");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.BrettsBirthday.ToISO8601String());
			});
		}

		void AssertCustomFieldWasExported_RegistryCustomAttributes(List<CustomizedField> customFields, bool isDefaultValue)
		{
			CombineAssertions(() =>
			{
				customFields.AssertCustomFieldWasExported(DataType.String, "Textual context", isDefaultValue ? "" : "HELLO");
				customFields.AssertCustomFieldWasExported(DataType.String, "Customs are customary", isDefaultValue ? "" : "GOODBYE");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "First Date", isDefaultValue ? "" : new ZDateTime(2011, 1, 1).ToISO8601String());
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "Last Date", isDefaultValue ? "" : new ZDateTime(2011, 1, 2).ToISO8601String());
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "Deci Deca", isDefaultValue ? "0" : "0.3");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "+ 1 point zero", isDefaultValue ? "0" : "1.3");
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Flag this!", isDefaultValue ? "false" : "true");
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Flagger", isDefaultValue ? "false" : "true");
			});
		}

		void AssertCustomFieldWasExported_JobDeclarationSupplier(List<CustomizedField> customFields)
		{
			CombineAssertions(() =>
			{
				customFields.AssertCustomFieldWasExported(DataType.String, "ExportersBankName", "TestBankName");
				customFields.AssertCustomFieldWasExported(DataType.String, "ExportersBankAccount", "BK00001");
				customFields.AssertCustomFieldWasExported(DataType.String, "ExportersSwiftCode", "SFT00001");
				customFields.AssertCustomFieldWasExported(DataType.String, "MethodOfPayment", "Payment001");
				customFields.AssertCustomFieldWasExported(DataType.String, "Additional Information", "Additional Information");
			});
		}

		void AssertCustomFieldWasExported_JobDeclarationDocNote(List<CustomizedField> customFields)
		{
			CombineAssertions(() =>
			{
				customFields.AssertCustomFieldWasExported(DataType.String, DeclarationDataObjectWriter.SDFields.InsurancePolicyNumber, "ISPY00001");
				customFields.AssertCustomFieldWasExported(DataType.String, DeclarationDataObjectWriter.SDFields.InsuredValue, "ISV00001");
				customFields.AssertCustomFieldWasExported(DataType.String, DeclarationDataObjectWriter.SDFields.LetterOfCreditDate, "CreditDate");
				customFields.AssertCustomFieldWasExported(DataType.String, DeclarationDataObjectWriter.SDFields.LetterOfCreditNumber, "CreditNumber");
			});
		}

		void AssertCustomFieldWasExported_JobDeclarationCustomFieldsFromWFTemplate(List<CustomizedField> customFields)
		{
			CombineAssertions(() =>
			{
				customFields.AssertCustomFieldWasExported(DataType.String, "WFField", "custom field from declaration workflow template");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "DeclarationWFField", "90");
			});
		}

		void AssertCustomFieldWasExported_ShipmentCustomFieldsFromWFTemplate(List<CustomizedField> customFields)
		{
			CombineAssertions(() =>
			{
				customFields.AssertCustomFieldWasExported(DataType.String, "WFField", "custom field from shipment workflow template");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "ShipmentWFField", "10");
			});
		}

		bool SenderLocalClientExists(Shipment declarationData)
		{
			bool sendersLocalClientExists = false;

			foreach (var orgAddress in declarationData.OrganizationAddressCollection)
			{
				if (orgAddress.AddressType.Equals(AddressTypes.SendersLocalClient))
				{
					sendersLocalClientExists = true;
				}
			}
			return sendersLocalClientExists;
		}
	}
}
