using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5167MessageSendingDocumentWrapper))]
	sealed class N5167MessageSendingDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		N5167MessageSendingDocumentWrapper wrapper;
		N5167MessageSendingDocumentWrapperHelperTest helper;
		[ExpectNoExceptions]
		public void TestDeclarationID()
		{
			NUnit.Framework.Assert.That(wrapper.DeclarationID, NUnit.Framework.Is.EqualTo("AEB80860000003").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNewWhenEntryHeaderIsNull()
		{
			NUnit.Framework.Assert.That(new N5167MessageSendingDocumentWrapper(null, Factory), NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.DocumentWrappers.N5167MessageSendingDocumentWrapper)));
		}

		protected override void SetUp()
		{
			helper = new N5167MessageSendingDocumentWrapperHelperTest(Factory);
			wrapper = new N5167MessageSendingDocumentWrapper(helper.CusEntryHeader, Factory);
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			return new N5167MessageSendingDocumentWrapper(entryHeader, Factory);
		}

		[ExpectNoExceptions]
		public void TestControlInspectionStartDateTime()
		{
			NUnit.Framework.Assert.That(wrapper.ControlInspectionStartDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 08, 27, 11, 16, 32)));
		}

		[ExpectNoExceptions]
		public void TestExaminationPlaceID()
		{
			NUnit.Framework.Assert.That(wrapper.ExaminationPlaceID, NUnit.Framework.Is.EqualTo("AW03").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAgentChineseName()
		{
			NUnit.Framework.Assert.That(wrapper.AgentChineseName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAgentID()
		{
			NUnit.Framework.Assert.That(wrapper.AgentID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestDeclarationOfficeID()
		{
			NUnit.Framework.Assert.That(wrapper.DeclarationOfficeID, NUnit.Framework.Is.EqualTo("AW").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDeclarationID()
		{
			NUnit.Framework.Assert.That(wrapper.AdditionalDeclarationID, NUnit.Framework.Is.EqualTo("AEB80860000003").Using(CustomComparers.TypeComparison));
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			wrapper = new N5167MessageSendingDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.AdditionalDeclarationID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "AAA";
			entryHeader.CH_Status = "AWO";
			NUnit.Framework.Assert.That(wrapper.AdditionalDeclarationID, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison));
			declaration.EntryNumber = "BBB";
			NUnit.Framework.Assert.That(wrapper.AdditionalDeclarationID, NUnit.Framework.Is.EqualTo("BBB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDeclarationIDFormat()
		{
			NUnit.Framework.Assert.That(wrapper.AdditionalDeclarationIDFormat, NUnit.Framework.Is.EqualTo("AE/B8/08/600/00003").Using(CustomComparers.TypeComparison));
			var entryHeader = helper.CusEntryHeader;
			entryHeader.EntryNumber = "AEB8086000003";
			NUnit.Framework.Assert.That(wrapper.AdditionalDeclarationIDFormat, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTransportEquipments()
		{
			NUnit.Framework.Assert.That(wrapper.TransportEquipments.GetType(), NUnit.Framework.Is.EqualTo(typeof(BusinessObjectCollectionWrapper<TransportEquipment>)));
			NUnit.Framework.Assert.That(wrapper.TransportEquipments.Count, NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestModeOfCustomsClearance()
		{
			NUnit.Framework.Assert.That(wrapper.ModeOfCustomsClearance, NUnit.Framework.Is.EqualTo(ZString.Empty));
			helper.AddNewStatusC3MN5107Message();
			var wapperC3M = new N5167MessageSendingDocumentWrapper(helper.CusEntryHeader, Factory);
			NUnit.Framework.Assert.That(wapperC3M.ModeOfCustomsClearance, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			helper.AddNewStatusC3XN5107Message();
			var wapperC3X = new N5167MessageSendingDocumentWrapper(helper.CusEntryHeader, Factory);
			NUnit.Framework.Assert.That(wapperC3X.ModeOfCustomsClearance, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
		}

		internal sealed class N5167MessageSendingDocumentWrapperHelperTest
		{
			public N5167MessageSendingDocumentWrapperHelperTest(BusinessObjectFactory factory)
			{
				Factory = factory;
				GenerateModeData();
			}

			BusinessObjectFactory Factory
			{
				get;
			}

			void GenerateModeData()
			{
				var classification = Factory.New<Customs.Business.BaseCusClassification>();
				classification.CC_Description = "CUCKOO SQUEAKERS";
				classification.CC_LookupCode = "CKSQKS";
				classification.CC_TariffNum = "0000.00.00.00Y";
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_CustomsProfile = "AAA-BBB";
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
				declaration.JE_MessageSubType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_CustomsOffice = "AW";
				var service = declaration.DocsAndCartage.Services.AddNew();
				service.ES_ServiceCode = ServiceTypes.CommodityInspection;
				service.ES_ServiceNote = "123456";
				service.ES_Booked = ZDateTime.BrettsBirthday;
				service.ES_SubLocation = "123";
				var container1 = declaration.CusContainers.AddNew();
				container1.CO_ContainerNumber = "UUUU1234567";
				var refContainer1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
				container1.CO_RC = refContainer1.PK;
				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_CC = classification.PK;
				invoiceLine1.ContainersPivot.AddPivotFor(container1);
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
				CusEntryHeader = declaration.CustomsEntryHeaders[0];
				CusEntryHeader.EntryNumber = "AEB80860000003";
				CusEntryHeader.CH_Status = "AWO";
				entryInstruction.TW_ICIExamTime = new ZDateTime(2019, 08, 27, 11, 16, 32);
				entryInstruction.TW_ICIExamLocation = "AW03";
			}

			public void AddNewStatusC3MN5107Message()
			{
				message = CusEntryHeader.Messages.AddNew();
				message.EM_ApplicationCode = ApplicationCodeList.Codes.TWCustoms;
				message.EM_MessageNum = "TWIN1";
				message.EM_MessageText = n5107MessageStatusC3M;
				message.EM_MessageType = MessageTypeList.Codes.RFM;
			}

			public void AddNewStatusC3XN5107Message()
			{
				message = CusEntryHeader.Messages.AddNew();
				message.EM_ApplicationCode = ApplicationCodeList.Codes.TWCustoms;
				message.EM_MessageNum = "TWIN1";
				message.EM_MessageText = n5107MessageStatusC3X;
				message.EM_MessageType = MessageTypeList.Codes.RFM;
			}

			public CusEntryHeader CusEntryHeader;
			JobDeclaration declaration;
			TWMessage message;
			readonly ZString n5107MessageStatusC3M = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5107:R-00-05"" xsi:schemaLocation=""urn:wco:datamodel:TW:N5107:R-00-05 N5107.xsd"">
	<IssueDateTime>2019-01-30T12:04:55</IssueDateTime>
	<AdditionalInformation>
		<LimitDateTime>2019-01-31</LimitDateTime>
	</AdditionalInformation>
	<ContactOffice>
		<ID>*</ID>
	</ContactOffice>
	<Status>
		<NameCode>C3M</NameCode>
		<ReleaseDateTime></ReleaseDateTime>
	</Status>
	<Declaration>
		<ID>DA  08207F1025</ID>
		<TotalPackageQuantity>22</TotalPackageQuantity>
		<TypeCode>G1</TypeCode>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<BorderTransportMeans>
			<ArrivalDateTime>2019-01-30</ArrivalDateTime>
			<TypeCode>1</TypeCode>
		</BorderTransportMeans>
		<GoodsShipment>
			<Consignment>
				<tw_ManifestSerialNumber>0002</tw_ManifestSerialNumber>
				<BorderTransportMeans>
					<JourneyID>19002S</JourneyID>
					<tw_Registration>08F399</tw_Registration>
				</BorderTransportMeans>
				<TransportContractDocument>
					<ID>100810455882</ID>
					<TypeCode>704</TypeCode>
				</TransportContractDocument>
			</Consignment>
			<GovernmentAgencyGoodsItem>
				<SequenceNumeric>0</SequenceNumeric>
				<Error>
					<ValidationCode>A28</ValidationCode>
				</Error>
				<Error>
					<ValidationCode>G01</ValidationCode>
				</Error>
			</GovernmentAgencyGoodsItem>
		</GoodsShipment>
		<GovernmentProcedure>
			<tw_TransportTypeCode>1</tw_TransportTypeCode>
		</GovernmentProcedure>
		<Packaging>
			<TypeCode>PKG</TypeCode>
		</Packaging>
	</Declaration>
</Response>
";
			readonly ZString n5107MessageStatusC3X = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5107:R-00-05"" xsi:schemaLocation=""urn:wco:datamodel:TW:N5107:R-00-05 N5107.xsd"">
	<IssueDateTime>2019-01-30T12:04:55</IssueDateTime>
	<AdditionalInformation>
		<LimitDateTime>2019-01-31</LimitDateTime>
	</AdditionalInformation>
	<ContactOffice>
		<ID>*</ID>
	</ContactOffice>
	<Status>
		<NameCode>C3X</NameCode>
		<ReleaseDateTime></ReleaseDateTime>
	</Status>
	<Declaration>
		<ID>DA  08207F1025</ID>
		<TotalPackageQuantity>22</TotalPackageQuantity>
		<TypeCode>G1</TypeCode>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<BorderTransportMeans>
			<ArrivalDateTime>2019-01-30</ArrivalDateTime>
			<TypeCode>1</TypeCode>
		</BorderTransportMeans>
		<GoodsShipment>
			<Consignment>
				<tw_ManifestSerialNumber>0002</tw_ManifestSerialNumber>
				<BorderTransportMeans>
					<JourneyID>19002S</JourneyID>
					<tw_Registration>08F399</tw_Registration>
				</BorderTransportMeans>
				<TransportContractDocument>
					<ID>100810455882</ID>
					<TypeCode>704</TypeCode>
				</TransportContractDocument>
			</Consignment>
			<GovernmentAgencyGoodsItem>
				<SequenceNumeric>0</SequenceNumeric>
				<Error>
					<ValidationCode>A28</ValidationCode>
				</Error>
				<Error>
					<ValidationCode>G01</ValidationCode>
				</Error>
			</GovernmentAgencyGoodsItem>
		</GoodsShipment>
		<GovernmentProcedure>
			<tw_TransportTypeCode>1</tw_TransportTypeCode>
		</GovernmentProcedure>
		<Packaging>
			<TypeCode>PKG</TypeCode>
		</Packaging>
	</Declaration>
</Response>
";
		}
	}
}
