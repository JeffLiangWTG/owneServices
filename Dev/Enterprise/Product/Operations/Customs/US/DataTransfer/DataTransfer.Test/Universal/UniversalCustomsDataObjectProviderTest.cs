using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static NUnit.Framework.XmlAssertions;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestSupportALCCusCodeDataForCusAddInfo()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var codes = provider.TableSpecificCusCodeDataTypeList(CusAddInfoSchema.Constants.Prefix);
			AssertEquals(CusCodeDataTypeList.Descriptions.AMSLotCode, codes.GetDescriptionFromCode(CusCodeDataTypeList.Codes.AMSLotCode));
		}

		public void TestHFCHeaderAndHFCDetailAreSupported()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			Assert(provider.TableSpecificCusAddInfoTypeList(JobComInvoiceLineSchema.Constants.Prefix).ContainsCode(CusAddInfoTypeAttribute.Codes.USHFCHeader));
			AssertEquals("PGA HFC Header", provider.TableSpecificCusAddInfoTypeList(JobComInvoiceLineSchema.Constants.Prefix).GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.USHFCHeader));

			Assert(provider.TableSpecificCusAddInfoTypeList(CusAddInfoSchema.Constants.Prefix).ContainsCode(CusAddInfoTypeAttribute.Codes.USHFCDetail));
			AssertEquals("PGA HFC Detail", provider.TableSpecificCusAddInfoTypeList(CusAddInfoSchema.Constants.Prefix).GetDescriptionFromCode(CusAddInfoTypeAttribute.Codes.USHFCDetail));
		}

		public void TestGetNewAirManifestDataObjectReaders()
		{
			var reader = new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(
				new UniversalDataBuss.DataObjects.Universal.Shipment(),
				new UniversalDataBuss.DataObjects.Universal.Shipment(),
				new TestErrorLogger(),
				Factory,
				false);
			Assert(!reader.Any());

			reader = new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(
				new UniversalDataBuss.DataObjects.Universal.Shipment(),
				new UniversalDataBuss.DataObjects.Universal.Shipment()
				{
					ShipmentType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair()
					{
						Code = ShipmentTypes.HighVolumeLowValue
					}
				},
				new TestErrorLogger(),
				Factory,
				false);
			AssertEquals("Enterprise.Customs.US.LVS.DataTransfer.Universal.eTailUSLVClearanceDataObjectReader", reader.Single().GetType().FullName);
		}

		public void TestGetNewCusSCAOceanBillDataObjectReaders()
		{
			var readers = new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(
				new UniversalDataBuss.DataObjects.Universal.Shipment(),
				new UniversalDataBuss.DataObjects.Universal.Shipment(),
				new TestErrorLogger(),
				Factory);
			AssertEquals(0, readers.Count());

			readers = new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(
				new UniversalDataBuss.DataObjects.Universal.Shipment(),
				new UniversalDataBuss.DataObjects.Universal.Shipment()
				{
					ShipmentType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair()
					{
						Code = ShipmentTypes.HighVolumeLowValue
					}
				},
				new TestErrorLogger(),
				Factory);
			AssertEquals("Enterprise.Customs.US.LVS.DataTransfer.Universal.eTailUSLVClearanceDataObjectReader", readers.FirstOrDefault().GetType().FullName);
		}

		public void TestGetNewDeclarationDataObjectWriter()
		{
			var writer = new UniversalCustomsDataObjectProvider().GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobDeclaration>())));
			AssertEquals(typeof(DeclarationDataObjectWriter), writer.GetType());
		}

		public void TestGetNewJobDeclarationDataObjectReader()
		{
			var reader = new UniversalCustomsDataObjectProvider().GetNewJobDeclarationDataObjectReader(new UniversalDataBuss.DataObjects.Universal.Shipment(), new TestErrorLogger(), Factory, null);
			AssertEquals(typeof(JobDeclarationDataObjectReader), reader.GetType());
		}

		[TestDate(2012, 10, 19, 11, 12, 10, 50)]
		public void TestPublishDeclarationUniversalEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MasterBill = "MB100031";
			declaration.JE_HouseBill = "HB1000460";
			declaration.JE_RL_NKFinalDestination = "CATOR";
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();

			var communicationMode = forwarder.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationMode.EK_Destination = "DummyDestination";
			communicationMode.EK_FileFormat = "ALL";
			communicationMode.EK_Module = "BRK";
			declaration.JE_OH_Forwarder = forwarder.PK;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders[0].EntryNumber = "10000201";
			declaration.US_UI_NKCarrierSCAC = "CARU";
			declaration.JE_OH_ShippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "CCCB", "US").PK;
			Factory.SaveForTesting();

			var decllogs = declaration.GetLogs();
			var precondition = declaration.GetLogs().DatabaseCount;
			declaration.Logs.AddNew(Events.ExportCustomsCleared);

			var provider = new UniversalCustomsDataObjectProvider();
			AssertNoExceptionThrown(delegate
			{ provider.PublishDeclarationUniversalEvent(declaration); });

			var query = new ZQuery(StmALogSchema.SL_Parent, declaration.PK);
			var logsLoaded = Factory.Load<StmALog>(query);
			AssertEquals("3 logs after publishing event - 3 existing (ADD, EDT, ECC) and 1 new (DEX)", precondition + 2, logsLoaded.Length);

			var eventCreated = logsLoaded.FirstOrDefault(x => x.SL_SE_NKEvent == Events.DataExportCode);
			AssertNotNull(eventCreated);

			var messageText = eventCreated.RelatedEDIMessage.Message.EM_MessageText;
			AssertIsXml("Universal Event as xml", messageText)
				.HavingExactlyOneChildNode("Event/ContextCollection/Context",
						node => node.HavingExactlyOneChildNode(child =>
							child.WithName("Type")
								 .WithValue("MBOLNumber")
						)
						.HavingExactlyOneChildNode(child =>
							child.WithName("Value")
								 .WithValue(declaration.JE_MasterBill)
						)
				).HavingExactlyOneChildNode("Event/ContextCollection/Context",
						node => node.HavingExactlyOneChildNode(child =>
							child.WithName("Type")
								 .WithValue("HBOLNumber")
						)
						.HavingExactlyOneChildNode(child =>
							child.WithName("Value")
								 .WithValue(declaration.JE_HouseBill)
						)
				).HavingExactlyOneChildNode("Event/ContextCollection/Context",
						node => node.HavingExactlyOneChildNode(child =>
							child.WithName("Type")
								 .WithValue("HBOLDestinationUNLOCO")
						)
						.HavingExactlyOneChildNode(child =>
							child.WithName("Value")
								 .WithValue(declaration.JE_RL_NKFinalDestination)
						)
				);

			AssertNoExceptionThrown(delegate
			{ provider.PublishDeclarationUniversalEvent(declaration); });

			logsLoaded = Factory.Load<StmALog>(query);
			AssertEquals("5 logs after second universal event published - new DEX event only", precondition + 3, logsLoaded.Length);
		}

		[TestDate(2012, 10, 19, 11, 12, 10, 50)]
		public void TestPublishDeclarationUniversalEvent_Sea()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MasterBill = "MB100031";
			declaration.JE_HouseBill = "HB1000460";
			declaration.JE_RL_NKFinalDestination = "CATOR";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();

			var communicationMode = forwarder.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationMode.EK_Destination = "DummyDestination";
			communicationMode.EK_FileFormat = "ALL";
			communicationMode.EK_Module = "BRK";
			declaration.JE_OH_Forwarder = forwarder.PK;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders[0].EntryNumber = "10000201";
			declaration.US_UI_NKCarrierSCAC = "CARU";
			declaration.JE_OH_ShippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "CCCB", "US").PK;
			Factory.SaveForTesting();

			var decllogs = declaration.GetLogs();
			var precondition = declaration.GetLogs().DatabaseCount;
			declaration.Logs.AddNew(Events.ExportCustomsCleared);

			var provider = new UniversalCustomsDataObjectProvider();
			AssertNoExceptionThrown(delegate
			{ provider.PublishDeclarationUniversalEvent(declaration); });

			var query = new ZQuery(StmALogSchema.SL_Parent, declaration.PK);
			var logsLoaded = Factory.Load<StmALog>(query);
			AssertEquals("3 logs after publishing event - 3 existing (ADD, EDT, ECC) and 1 new (DEX)", precondition + 2, logsLoaded.Length);

			var eventCreated = logsLoaded.FirstOrDefault(x => x.SL_SE_NKEvent == Events.DataExportCode);
			AssertNotNull(eventCreated);

			var messageText = eventCreated.RelatedEDIMessage.Message.EM_MessageText;
			AssertIsXml("Universal Event as xml", messageText)
				.HavingExactlyOneChildNode("Event/ContextCollection/Context",
					node => node.HavingExactlyOneChildNode(child =>
						child.WithName("Type")
							 .WithValue("MBOLNumber")
					)
					.HavingExactlyOneChildNode(child =>
						child.WithName("Value")
							 .WithValue(declaration.JE_MasterBill)
					)
				).HavingExactlyOneChildNode("Event/ContextCollection/Context",
					node => node.HavingExactlyOneChildNode(child =>
						child.WithName("Type")
							 .WithValue("HBOLNumber")
					)
					.HavingExactlyOneChildNode(child =>
						child.WithName("Value")
							 .WithValue(declaration.JE_HouseBill)
					)
				).HavingExactlyOneChildNode("Event/ContextCollection/Context",
					node => node.HavingExactlyOneChildNode(child =>
						child.WithName("Type")
							 .WithValue("HBOLDestinationUNLOCO")
					)
					.HavingExactlyOneChildNode(child =>
						child.WithName("Value")
							 .WithValue(declaration.JE_RL_NKFinalDestination)
					)
				);

			AssertNoExceptionThrown(delegate
			{ provider.PublishDeclarationUniversalEvent(declaration); });

			logsLoaded = Factory.Load<StmALog>(query);
			AssertEquals("5 logs after second universal event published - new DEX event only", precondition + 3, logsLoaded.Length);
		}

		[TestDate(2012, 10, 19, 11, 12, 10, 50)]
		public void TestPublishDeclarationUniversalEvent_Use_US_UI_NKCarrierSCAC_Sea()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "MB100031";
			declaration.JE_HouseBill = "HB1000460";
			declaration.JE_RL_NKFinalDestination = "CATOR";
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();

			var communicationMode = forwarder.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationMode.EK_Destination = "DummyDestination";
			communicationMode.EK_FileFormat = "ALL";
			communicationMode.EK_Module = "BRK";
			declaration.JE_OH_Forwarder = forwarder.PK;

			declaration.JE_OH_ShippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "CCCB", "US").PK;
			declaration.US_UI_NKCarrierSCAC = "CARU";
			Factory.SaveForTesting();

			declaration.Logs.AddNew(new EventValue(AutoEvents.ExportCustomsCleared));

			var provider = new UniversalCustomsDataObjectProvider();
			AssertNoExceptionThrown(delegate
			{ provider.PublishDeclarationUniversalEvent(declaration); });

			var query = new ZQuery(StmALogSchema.SL_Parent, declaration.PK);
			var logsLoaded = Factory.Load<StmALog>(query);

			var eventCreated = logsLoaded.FirstOrDefault(x => x.SL_SE_NKEvent == Events.DataExportCode);
			AssertNotNull(eventCreated);

			var messageText = eventCreated.RelatedEDIMessage.Message.EM_MessageText;
			AssertIsXml("Universal Event as xml", messageText)
				.HavingExactlyOneChildNode("Event/ContextCollection/Context",
					node => node.HavingExactlyOneChildNode(child =>
						child.WithName("Type")
							 .WithValue("CarrierCode")
					)
					.HavingExactlyOneChildNode(child =>
						child.WithName("Value")
							 .WithValue(declaration.US_UI_NKCarrierSCAC)
					)
				).HavingExactlyOneChildNode("Event/ContextCollection/Context",
					node => node.HavingExactlyOneChildNode(child =>
						child.WithName("Type")
							 .WithValue("MBOLNumber")
					)
					.HavingExactlyOneChildNode(child =>
						child.WithName("Value")
							 .WithValue(declaration.JE_MasterBill)
					)
				).HavingExactlyOneChildNode("Event/ContextCollection/Context",
					node => node.HavingExactlyOneChildNode(child =>
						child.WithName("Type")
							 .WithValue("HBOLNumber")
					)
					.HavingExactlyOneChildNode(child =>
						child.WithName("Value")
							 .WithValue(declaration.JE_HouseBill)
					)
				);
		}

		public void TestTableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(JobDeclarationSchema.Constants.Prefix, ""));
		}

		public void TestGetNewUniversalDataObjectReaderHelper()
		{
			var helper = new UniversalCustomsDataObjectProvider().GetNewUniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(typeof(UniversalDataObjectReaderHelper), helper.GetType());
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}

		public void TestTableSpecificCusCodeDataCodeList()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AccountingClassFeeCode, RefCusCodeListTypes.Codes.AccountingClassFeeCode);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.Pecan, "Pecan Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.ChristmasTree, "Christmas Tree Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			factory.Save();

			var codeDescriptionPairList = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataCodeList(CusEntryHeaderSchema.Constants.Prefix);
			AssertEquals(true, codeDescriptionPairList.ContainsCode(Core.Constants.USCustoms.FeeCodes.Pecan));
			AssertEquals(true, codeDescriptionPairList.ContainsCode(Core.Constants.USCustoms.FeeCodes.ChristmasTree));
		}
	}
}
