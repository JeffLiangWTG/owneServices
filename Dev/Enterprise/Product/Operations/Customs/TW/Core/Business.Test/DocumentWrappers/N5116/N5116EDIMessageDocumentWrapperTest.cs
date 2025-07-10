using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5116EDIMessageDocumentWrapper))]
	sealed class N5116EDIMessageDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		N5116EDIMessageDocumentWrapper wrapper;

		[ExpectNoExceptions]
		public void TestTransportMode()
		{
			NUnit.Framework.Assert.That(wrapper.TransportMode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDocumentType()
		{
			NUnit.Framework.Assert.That(wrapper.DocumentType, NUnit.Framework.Is.EqualTo("G1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDeclarationID()
		{
			NUnit.Framework.Assert.That(wrapper.DeclarationID, NUnit.Framework.Is.EqualTo("AA/B1/08/234/81234").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestClassificationID()
		{
			NUnit.Framework.Assert.That(wrapper.ClassificationID, NUnit.Framework.Is.EqualTo("87088090003").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestReleaseTypeCode()
		{
			NUnit.Framework.Assert.That(wrapper.ReleaseTypeCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestArrivalDateTime()
		{
			NUnit.Framework.Assert.That(wrapper.ArrivalDateTime, NUnit.Framework.Is.EqualTo("2019/01/30").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMasterNumberAndHouseNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.MasterNumber, NUnit.Framework.Is.EqualTo("100810455882").Using(CustomComparers.TypeComparison), "MasterNumber");
				NUnit.Framework.Assert.That(wrapper.HouseNumber, NUnit.Framework.Is.EqualTo("211921566993").Using(CustomComparers.TypeComparison), "HouseNumber");
			});

			wrapper = GetN5116EDIMessageDocumentWrapper("1", "741", "703");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.MasterNumber, NUnit.Framework.Is.EqualTo("100810455882").Using(CustomComparers.TypeComparison), "MasterNumber");
				NUnit.Framework.Assert.That(wrapper.HouseNumber, NUnit.Framework.Is.EqualTo("211921566993").Using(CustomComparers.TypeComparison), "HouseNumber");
			});

			wrapper = GetN5116EDIMessageDocumentWrapper("1", "703", "704");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.MasterNumber, NUnit.Framework.Is.EqualTo("211921566993").Using(CustomComparers.TypeComparison), "MasterNumber");
				NUnit.Framework.Assert.That(wrapper.HouseNumber, NUnit.Framework.Is.EqualTo("100810455882").Using(CustomComparers.TypeComparison), "HouseNumber");
			});

			wrapper = GetN5116EDIMessageDocumentWrapper("1", "714", "741");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.MasterNumber, NUnit.Framework.Is.EqualTo("211921566993").Using(CustomComparers.TypeComparison), "MasterNumber");
				NUnit.Framework.Assert.That(wrapper.HouseNumber, NUnit.Framework.Is.EqualTo("100810455882").Using(CustomComparers.TypeComparison), "HouseNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestAgentID()
		{
			NUnit.Framework.Assert.That(wrapper.AgentID, NUnit.Framework.Is.EqualTo("207").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(wrapper.ChineseName, NUnit.Framework.Is.EqualTo("裕隆汽車製造股份有限公司").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEnglishName()
		{
			NUnit.Framework.Assert.That(wrapper.EnglishName, NUnit.Framework.Is.EqualTo("YULON MOTOR CO., LTD.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestImporterID()
		{
			NUnit.Framework.Assert.That(wrapper.ImporterID, NUnit.Framework.Is.EqualTo("03489200").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(wrapper.CustomsControlID, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTransportID()
		{
			NUnit.Framework.Assert.That(wrapper.TransportID, NUnit.Framework.Is.EqualTo("C6AV9").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestJourneyID()
		{
			NUnit.Framework.Assert.That(wrapper.JourneyID, NUnit.Framework.Is.EqualTo("19002S").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsLocationID()
		{
			NUnit.Framework.Assert.That(wrapper.GoodsLocationID, NUnit.Framework.Is.EqualTo("TXG0102C").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestWarehouseID()
		{
			NUnit.Framework.Assert.That(wrapper.WarehouseID, NUnit.Framework.Is.EqualTo("13011013").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalPackageQuantityAndUnit()
		{
			NUnit.Framework.Assert.That(wrapper.TotalPackageQuantityAndUnit, NUnit.Framework.Is.EqualTo("22 PKG").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestReleasedQuantityAndUnit()
		{
			NUnit.Framework.Assert.That(wrapper.ReleasedQuantityAndUnit, NUnit.Framework.Is.EqualTo("23 ACR").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUnreleasedQuantityAndUnit()
		{
			NUnit.Framework.Assert.That(wrapper.UnreleasedQuantityAndUnit, NUnit.Framework.Is.EqualTo("24 ACR").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestReleaseDateTime()
		{
			NUnit.Framework.Assert.That(wrapper.ReleaseDateTime, NUnit.Framework.Is.EqualTo("2019/01/25 14:30:56").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestOtherDeclarations()
		{
			NUnit.Framework.Assert.That(wrapper.OtherDeclarations, NUnit.Framework.Is.EqualTo("標記:\r\nMarksNumber Test\r\n貨櫃資料:\r\nTCNU4081834/TCNU4081835\r\n處理註記:\r\nY/M").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsClearance()
		{
			NUnit.Framework.Assert.That(wrapper.CustomsClearance, NUnit.Framework.Is.EqualTo("C2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsShortage()
		{
			NUnit.Framework.Assert.That(wrapper.IsShortage, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalCondition()
		{
			NUnit.Framework.Assert.That(wrapper.AdditionalCondition, NUnit.Framework.Is.EqualTo("A1\r\nA2").Using(CustomComparers.TypeComparison));
		}

		const string MessageTextForTest = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5116:R-00-05"" xsi:schemaLocation=""urn:wco:datamodel:TW:N5116:R-00-05 N5116.xsd"">
	<FunctionCode>9</FunctionCode>
	<AdditionalInformation>
		<StatementCode>A1</StatementCode>
	</AdditionalInformation>
	<AdditionalInformation>
		<StatementCode>A2</StatementCode>
	</AdditionalInformation>
	<Status>
		<NameCode>C2</NameCode>
		<ReleaseDateTime>2019-01-25T14:30:56</ReleaseDateTime>
		<tw_ReleaseTypeCode>1</tw_ReleaseTypeCode>
		<tw_TotalPackageQuantity>23</tw_TotalPackageQuantity>
	</Status>
	<tw_UnreleasedPackages>
		<tw_QuantityQuantity>24</tw_QuantityQuantity>
		<tw_TypeCode>ACR</tw_TypeCode>
	</tw_UnreleasedPackages>
	<Declaration>
		<ID>AAB10823481234</ID>
		<TotalGrossMassMeasure>5387</TotalGrossMassMeasure>
		<TotalPackageQuantity>22</TotalPackageQuantity>
		<TypeCode>G1</TypeCode>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<BorderTransportMeans>
			<ArrivalDateTime>2019-01-30</ArrivalDateTime>
			<TypeCode>{0}</TypeCode>
		</BorderTransportMeans>
		<GoodsShipment>
			<AdditionalInformation>
				<StatementCode>Y</StatementCode>
			</AdditionalInformation>
			<Consignment>
				<tw_ManifestSerialNumber>0002</tw_ManifestSerialNumber>
				<BorderTransportMeans>
					<ID>C6AV9</ID>
					<JourneyID>19002S</JourneyID>
					<tw_Registration>08F399</tw_Registration>
				</BorderTransportMeans>
				<GoodsLocation>
					<ID>TXG0102C</ID>
				</GoodsLocation>
				<TransportContractDocument>
					<ID>100810455882</ID>
					<TypeCode>{1}</TypeCode>
				</TransportContractDocument>
				<TransportContractDocument>
					<ID>211921566993</ID>
					<TypeCode>{2}</TypeCode>
				</TransportContractDocument>
				<TransportEquipment>
					<ID>TCNU4081834</ID>
					<tw_CurrentCode>Y</tw_CurrentCode>
				</TransportEquipment>
				<TransportEquipment>
					<ID>TCNU4081835</ID>
					<tw_CurrentCode>M</tw_CurrentCode>
				</TransportEquipment>
			</Consignment>
			<GovernmentAgencyGoodsItem>
				<Commodity>
					<Classification>
						<ID>87088090003</ID>
					</Classification>
				</Commodity>
			</GovernmentAgencyGoodsItem>
			<Warehouse>
				<ID>13011013</ID>
			</Warehouse>
		</GoodsShipment>
		<Importer>
			<ID>03489200</ID>
			<Name>YULON MOTOR CO., LTD.</Name>
			<tw_ChineseName>裕隆汽車製造股份有限公司</tw_ChineseName>
			<tw_CustomsControlID>96944490</tw_CustomsControlID>
			<tw_TypeCode>58</tw_TypeCode>
		</Importer>
		<Packaging>
			<MarksNumbers>MarksNumber Test</MarksNumbers>
			<TypeCode>PKG</TypeCode>
		</Packaging>
	</Declaration>
</Response>";

		protected override BusinessObject GetNewBusinessObject()
		{
			var message = Factory.NewWithValidTestData<N5116EDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IRM;
			return new N5116EDIMessageDocumentWrapper(message);
		}

		N5116EDIMessageDocumentWrapper GetN5116EDIMessageDocumentWrapper(ZString borderTransportMeansTypeCode, ZString transportContractDocumentTypeCode1, ZString transportContractDocumentTypeCode2)
		{
			var message = Factory.New<N5116EDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.IRM;
			message.EM_MessageText = string.Format(MessageTextForTest, borderTransportMeansTypeCode, transportContractDocumentTypeCode1, transportContractDocumentTypeCode2);
			return new N5116EDIMessageDocumentWrapper(message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = GetN5116EDIMessageDocumentWrapper("1", "704", "703");
		}
	}
}
