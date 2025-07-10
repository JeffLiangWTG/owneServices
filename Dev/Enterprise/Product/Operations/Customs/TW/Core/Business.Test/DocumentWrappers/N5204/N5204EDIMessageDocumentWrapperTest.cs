using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5204EDIMessageDocumentWrapper))]
	sealed class N5204EDIMessageDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		N5204EDIMessageDocumentWrapper wrapper;

		[ExpectNoExceptions]
		public void TestFields()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.DeclarationID, NUnit.Framework.Is.EqualTo("DA/  /08/207/H1013").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.TransportMode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ClassificationID, NUnit.Framework.Is.EqualTo("73262000900").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.CarrierID, NUnit.Framework.Is.EqualTo("1105293").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ExporterName, NUnit.Framework.Is.EqualTo("UNI AUTO PARTS MANUFACTURE CO., LTD.").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ExporterID, NUnit.Framework.Is.EqualTo("11104755").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.AgentID, NUnit.Framework.Is.EqualTo("207").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.CallSignID, NUnit.Framework.Is.EqualTo("9V7586").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.JourneyID, NUnit.Framework.Is.EqualTo("N134").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.GoodsLocationID, NUnit.Framework.Is.EqualTo("TXG0342C").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ReleasedQuantityAndUnit, NUnit.Framework.Is.EqualTo("6 ACR").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.UnreleasedQuantityAndUnit, NUnit.Framework.Is.EqualTo("24 ACR").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.ReleaseDateTime, NUnit.Framework.Is.EqualTo("2019/01/28 17:02:44").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.EquipmentCurrentCode, NUnit.Framework.Is.EqualTo("Y/M").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.NameCode, NUnit.Framework.Is.EqualTo("C1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Registration, NUnit.Framework.Is.EqualTo("08F328").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.StatementCode, NUnit.Framework.Is.EqualTo("4\r\n3").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.MarksNumbersAndTransportEquipment, NUnit.Framework.Is.EqualTo("標記:\r\nSANKYO MIZUSHIMA C/NO:1-6 MADE IN TAIWAN\r\n貨櫃資料:\r\nWHLU0235618/TCNU4081835").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestMasterNumberAndHouseNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.MasterNumber, NUnit.Framework.Is.EqualTo("6666666666").Using(CustomComparers.TypeComparison), "MasterNumber");
				NUnit.Framework.Assert.That(wrapper.HouseNumber, NUnit.Framework.Is.EqualTo("9999999999").Using(CustomComparers.TypeComparison), "HouseNumber");
			});

			wrapper = GetN5204EDIMessageDocumentWrapper("1", "741", "703");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.MasterNumber, NUnit.Framework.Is.EqualTo("6666666666").Using(CustomComparers.TypeComparison), "MasterNumber");
				NUnit.Framework.Assert.That(wrapper.HouseNumber, NUnit.Framework.Is.EqualTo("9999999999").Using(CustomComparers.TypeComparison), "HouseNumber");
			});

			wrapper = GetN5204EDIMessageDocumentWrapper("1", "703", "704");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.MasterNumber, NUnit.Framework.Is.EqualTo("9999999999").Using(CustomComparers.TypeComparison), "MasterNumber");
				NUnit.Framework.Assert.That(wrapper.HouseNumber, NUnit.Framework.Is.EqualTo("6666666666").Using(CustomComparers.TypeComparison), "HouseNumber");
			});

			wrapper = GetN5204EDIMessageDocumentWrapper("1", "714", "741");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.MasterNumber, NUnit.Framework.Is.EqualTo("9999999999").Using(CustomComparers.TypeComparison), "MasterNumber");
				NUnit.Framework.Assert.That(wrapper.HouseNumber, NUnit.Framework.Is.EqualTo("6666666666").Using(CustomComparers.TypeComparison), "HouseNumber");
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var message = Factory.NewWithValidTestData<N5204EDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.ERM;
			return new N5204EDIMessageDocumentWrapper(message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = GetN5204EDIMessageDocumentWrapper("1", "704", "703");
		}

		N5204EDIMessageDocumentWrapper GetN5204EDIMessageDocumentWrapper(ZString borderTransportMeansTypeCode, ZString transportContractDocumentTypeCode1, ZString transportContractDocumentTypeCode2)
		{
			var message = Factory.New<N5204EDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.ERM;
			message.EM_MessageText = string.Format(MessageTextForTest, borderTransportMeansTypeCode, transportContractDocumentTypeCode1, transportContractDocumentTypeCode2);
			return new N5204EDIMessageDocumentWrapper(message);
		}

		const string MessageTextForTest = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5204:R-00-03"" xmlns:tsw=""urn:SingleWindow:TW"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""urn:wco:datamodel:TW:N5204:R-00-03 N5204.xsd"">
	<FunctionCode>9</FunctionCode>
	<AdditionalInformation>
		<StatementCode>4</StatementCode>
	</AdditionalInformation>
	<AdditionalInformation>
		<StatementCode>3</StatementCode>
	</AdditionalInformation>
	<Status>
		<NameCode>C1</NameCode>
		<ReleaseDateTime>2019-01-28T17:02:44</ReleaseDateTime>
		<tw_TotalPackageQuantity>6</tw_TotalPackageQuantity>
	</Status>
	<tw_UnreleasedPackages>
		<tw_QuantityQuantity>24</tw_QuantityQuantity>
		<tw_TypeCode>ACR</tw_TypeCode>
	</tw_UnreleasedPackages>
	<Declaration>
		<ID>DA  08207H1013</ID>
		<TotalGrossMassMeasure>2401.6</TotalGrossMassMeasure>
		<TypeCode>G5</TypeCode>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<BorderTransportMeans>
			<TypeCode>{0}</TypeCode>
		</BorderTransportMeans>
		<GoodsShipment>
			<Consignment>
				<tw_ShippingOrderNumber>0027</tw_ShippingOrderNumber>
				<BorderTransportMeans>
					<JourneyID>N134</JourneyID>
					<tw_CallSignID>9V7586</tw_CallSignID>
					<tw_Registration>08F328</tw_Registration>
				</BorderTransportMeans>
				<Carrier>
					<ID>1105293</ID>
				</Carrier>
				<DepartureTransportMeans>
					<Name>WAN HAI 273</Name>
				</DepartureTransportMeans>
				<GoodsLocation>
					<ID>TXG0342C</ID>
				</GoodsLocation>
				<TransportContractDocument>
					<ID>6666666666</ID>
					<TypeCode>{1}</TypeCode>
				</TransportContractDocument>
				<TransportContractDocument>
					<ID>9999999999</ID>
					<TypeCode>{2}</TypeCode>
				</TransportContractDocument>
				<TransportEquipment>
					<ID>WHLU0235618</ID>
					<tw_CurrentCode>Y</tw_CurrentCode>
				</TransportEquipment>
				<TransportEquipment>
					<ID>TCNU4081835</ID>
					<tw_CurrentCode>M</tw_CurrentCode>
				</TransportEquipment>
			</Consignment>
			<Exporter>
				<ID>11104755</ID>
				<Name>UNI AUTO PARTS MANUFACTURE CO., LTD.</Name>
				<tw_TypeCode>58</tw_TypeCode>
			</Exporter>
			<GovernmentAgencyGoodsItem>
				<Commodity>
					<Classification>
						<ID>73262000900</ID>
					</Classification>
				</Commodity>
				<Error>
					<ValidationCode></ValidationCode>
				</Error>
			</GovernmentAgencyGoodsItem>
		</GoodsShipment>
		<Packaging>
			<MarksNumbers>SANKYO MIZUSHIMA C/NO:1-6 MADE IN TAIWAN</MarksNumbers>
			<TypeCode>PLT</TypeCode>
		</Packaging>
		<BorderTranspotMeans>
			<TypeCode></TypeCode>
		</BorderTranspotMeans>
	</Declaration>
</Response>
";
	}
}
