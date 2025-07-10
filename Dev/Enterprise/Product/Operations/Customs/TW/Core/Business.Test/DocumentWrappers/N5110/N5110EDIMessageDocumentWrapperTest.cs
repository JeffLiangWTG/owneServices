using System.Drawing;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5110EDIMessageDocumentWrapper))]
	sealed class N5110EDIMessageDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		N5110EDIMessageDocumentWrapper wrapper;
		N5110EDIMessage message;

		protected override BusinessObject GetNewBusinessObject()
		{
			var message = Factory.NewWithValidTestData<N5110EDIMessage>();
			return new N5110EDIMessageDocumentWrapper(message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<N5110EDIMessage>();
			message.EM_MessageText = MessageTextForTest;
			wrapper = new N5110EDIMessageDocumentWrapper(message);
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeName()
		{
			NUnit.Framework.Assert.That(wrapper.CustomsOfficeName, NUnit.Framework.Is.EqualTo("台中關").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestImporterChineseName()
		{
			NUnit.Framework.Assert.That(wrapper.ImporterChineseName, NUnit.Framework.Is.EqualTo("友聯車材製造股份有限公司").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestImporterEnglishName()
		{
			NUnit.Framework.Assert.That(wrapper.ImporterEnglishName, NUnit.Framework.Is.EqualTo("UNI AUTO PARTS MANUFACTURE CO., LTD.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestImporterName()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.ImporterName, NUnit.Framework.Is.EqualTo("UNI AUTO PARTS MANUFACTURE CO., LTD. 友聯車材製造股份有限公司").Using(CustomComparers.TypeComparison));

				message.EM_MessageText = MessageTextForTest2;
				wrapper = new N5110EDIMessageDocumentWrapper(message);
				NUnit.Framework.Assert.That(wrapper.ImporterName, NUnit.Framework.Is.EqualTo("友聯車材製造股份有限公司").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestImporterID()
		{
			NUnit.Framework.Assert.That(wrapper.ImporterID, NUnit.Framework.Is.EqualTo("11104755").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAgentID()
		{
			NUnit.Framework.Assert.That(wrapper.AgentID, NUnit.Framework.Is.EqualTo("207").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDeclarationID()
		{
			NUnit.Framework.Assert.That(wrapper.DeclarationID, NUnit.Framework.Is.EqualTo("DA  08207F1027").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFeeReferenceID()
		{
			NUnit.Framework.Assert.That(wrapper.DutyTaxFeeReferenceID, NUnit.Framework.Is.EqualTo("DAI11180217670").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsInspection()
		{
			var message1 = Factory.New<N5110EDIMessage>();
			message1.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C3M</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>HHXTA19017336J</ID><TypeCode>704</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>9830</AdValoremTaxBaseAmount><TypeCode>A10</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>9421</AdValoremTaxBaseAmount><TypeCode>B40</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper1 = new N5110EDIMessageDocumentWrapper(message1);
			var message2 = Factory.New<N5110EDIMessage>();
			message2.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C3X</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>HHXTA19017336J</ID><TypeCode>704</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>9830</AdValoremTaxBaseAmount><TypeCode>A10</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>9421</AdValoremTaxBaseAmount><TypeCode>B40</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper2 = new N5110EDIMessageDocumentWrapper(message2);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.IsInspection, NUnit.Framework.Is.EqualTo(YesNoList.Codes.No).Using(CustomComparers.TypeComparison), "Response -> Status -> NameCode -> Value is 'C1'");
				NUnit.Framework.Assert.That(wrapper1.IsInspection, NUnit.Framework.Is.EqualTo(YesNoList.Codes.Yes).Using(CustomComparers.TypeComparison), "Response -> Status -> NameCode -> Value is 'C3M'");
				NUnit.Framework.Assert.That(wrapper2.IsInspection, NUnit.Framework.Is.EqualTo(YesNoList.Codes.Yes).Using(CustomComparers.TypeComparison), "Response -> Status -> NameCode -> Value is 'C3X'");
			});
		}

		[ExpectNoExceptions]
		public void TestBillOfLadingNumber()
		{
			var message1 = Factory.New<N5110EDIMessage>();
			message1.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>741</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>9830</AdValoremTaxBaseAmount><TypeCode>A10</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>9421</AdValoremTaxBaseAmount><TypeCode>B40</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper1 = new N5110EDIMessageDocumentWrapper(message1);
			var message2 = Factory.New<N5110EDIMessage>();
			message2.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>740</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>9830</AdValoremTaxBaseAmount><TypeCode>A10</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>9421</AdValoremTaxBaseAmount><TypeCode>B40</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper2 = new N5110EDIMessageDocumentWrapper(message2);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.BillOfLadingNumber, NUnit.Framework.Is.EqualTo("HHXTA19017336J").Using(CustomComparers.TypeComparison), "Response -> Declaration -> GoodsShipment -> Consignment -> TransportContractDocument -> TypeCode is 704");
				NUnit.Framework.Assert.That(wrapper1.BillOfLadingNumber, NUnit.Framework.Is.EqualTo("AAAA").Using(CustomComparers.TypeComparison), "Response -> Declaration -> GoodsShipment -> Consignment -> TransportContractDocument -> TypeCode is 741");
				NUnit.Framework.Assert.That(wrapper2.BillOfLadingNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Response -> Declaration -> GoodsShipment -> Consignment -> TransportContractDocument -> TypeCode is 740");
			});
		}

		[ExpectNoExceptions]
		public void TestHouseBillOfLadingNumber()
		{
			var message1 = Factory.New<N5110EDIMessage>();
			message1.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>741</TypeCode></TransportContractDocument><TransportContractDocument><ID>HBL0002</ID><TypeCode>714</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>9830</AdValoremTaxBaseAmount><TypeCode>A10</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>9421</AdValoremTaxBaseAmount><TypeCode>B40</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper1 = new N5110EDIMessageDocumentWrapper(message1);
			var message2 = Factory.New<N5110EDIMessage>();
			message2.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>740</TypeCode></TransportContractDocument><TransportContractDocument><ID>HBL0003</ID><TypeCode>715</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>9830</AdValoremTaxBaseAmount><TypeCode>A10</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>9421</AdValoremTaxBaseAmount><TypeCode>B40</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper2 = new N5110EDIMessageDocumentWrapper(message2);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.HouseBillOfLadingNumber, NUnit.Framework.Is.EqualTo("HBL0001").Using(CustomComparers.TypeComparison), "Response -> Declaration -> GoodsShipment -> Consignment -> TransportContractDocument -> TypeCode is 703");
				NUnit.Framework.Assert.That(wrapper1.HouseBillOfLadingNumber, NUnit.Framework.Is.EqualTo("HBL0002").Using(CustomComparers.TypeComparison), "Response -> Declaration -> GoodsShipment -> Consignment -> TransportContractDocument -> TypeCode is 714");
				NUnit.Framework.Assert.That(wrapper2.HouseBillOfLadingNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Response -> Declaration -> GoodsShipment -> Consignment -> TransportContractDocument -> TypeCode is 715");
			});
		}

		[ExpectNoExceptions]
		public void TestDueDateTime()
		{
			NUnit.Framework.Assert.That(wrapper.DueDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2024, 06, 18)));
		}

		public void DueDateTimeInTaiwanCalendar()
		{
			NUnit.Framework.Assert.That(wrapper.DueDateTimeInTaiwanCalendar, NUnit.Framework.Is.EqualTo("113/06/18").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIssueDateTime()
		{
			message.EM_SystemCreateTimeUtc = new ZDateTime(2019, 01, 01);
			NUnit.Framework.Assert.That(wrapper.IssueDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 1, 1)));
		}

		[ExpectNoExceptions]
		public void TestObligationGuaranteeReferenceID()
		{
			NUnit.Framework.Assert.That(wrapper.ObligationGuaranteeReferenceID, NUnit.Framework.Is.EqualTo("AAI11180217679").Using(CustomComparers.TypeComparison));

			message.EM_MessageText = MessageTextForTest2;
			wrapper = new N5110EDIMessageDocumentWrapper(message);
			NUnit.Framework.Assert.That(wrapper.ObligationGuaranteeReferenceID, NUnit.Framework.Is.EqualTo("HBL0001").Using(CustomComparers.TypeComparison));
		}
		[ExpectNoExceptions]
		public void TestBankAccountID()
		{
			NUnit.Framework.Assert.That(wrapper.BankAccountID, NUnit.Framework.Is.EqualTo("AAAA").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestArrivalDateTime()
		{
			NUnit.Framework.Assert.That(wrapper.ArrivalDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2019, 1, 30)));
		}
		[ExpectNoExceptions]
		public void TestTotalPackageQuantity()
		{
			NUnit.Framework.Assert.That(wrapper.TotalPackageQuantity, NUnit.Framework.Is.EqualTo(4m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDTYAmount()
		{
			NUnit.Framework.Assert.That(wrapper.DTYAmount, NUnit.Framework.Is.EqualTo(300m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestVATAmount()
		{
			NUnit.Framework.Assert.That(wrapper.VATAmount, NUnit.Framework.Is.EqualTo(700m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTPFAmount()
		{
			NUnit.Framework.Assert.That(wrapper.TPFAmount, NUnit.Framework.Is.EqualTo(1800m).Using(CustomComparers.TypeComparison));
		}
		[ExpectNoExceptions]
		public void TestAdditionalFeeTypeAndAmount()
		{
			var dutyTaxFeeCodeList = Factory.GetCachedValue<DutyTaxFeeCodeList>();
			var message = "Response -> Declaration -> GoodsShipment -> DutyTaxFee -> TypeCode is";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.AdditionalFeeType1, NUnit.Framework.Is.EqualTo(dutyTaxFeeCodeList.GetDescriptionFromCode("A20")).Using(CustomComparers.TypeComparison), $"{message} A20");
				NUnit.Framework.Assert.That(wrapper.AdditionalFeeType2, NUnit.Framework.Is.EqualTo(dutyTaxFeeCodeList.GetDescriptionFromCode("A30")).Using(CustomComparers.TypeComparison), $"{message} A30");
				NUnit.Framework.Assert.That(wrapper.AdditionalFeeType3, NUnit.Framework.Is.EqualTo(dutyTaxFeeCodeList.GetDescriptionFromCode("A40")).Using(CustomComparers.TypeComparison), $"{message} A40");
				NUnit.Framework.Assert.That(wrapper.AdditionalFeeType4, NUnit.Framework.Is.EqualTo(dutyTaxFeeCodeList.GetDescriptionFromCode("A50")).Using(CustomComparers.TypeComparison), $"{message} A50");

				NUnit.Framework.Assert.That(wrapper.AdditionalFeeAmount1, NUnit.Framework.Is.EqualTo(800m).Using(CustomComparers.TypeComparison), $"{message} A20");
				NUnit.Framework.Assert.That(wrapper.AdditionalFeeAmount2, NUnit.Framework.Is.EqualTo(900m).Using(CustomComparers.TypeComparison), $"{message} A30");
				NUnit.Framework.Assert.That(wrapper.AdditionalFeeAmount3, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), $"{message} A40");
				NUnit.Framework.Assert.That(wrapper.AdditionalFeeAmount4, NUnit.Framework.Is.EqualTo(1100m).Using(CustomComparers.TypeComparison), $"{message} A50");
			});

			var message1 = Factory.New<N5110EDIMessage>();
			message1.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>740</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>100</AdValoremTaxBaseAmount><TypeCode>B19</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>200</AdValoremTaxBaseAmount><TypeCode>B69</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>300</AdValoremTaxBaseAmount><TypeCode>B79</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper1 = new N5110EDIMessageDocumentWrapper(message1);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper1.AdditionalFeeType1, NUnit.Framework.Is.EqualTo(dutyTaxFeeCodeList.GetDescriptionFromCode("B19")).Using(CustomComparers.TypeComparison), $"{message} B19");
				NUnit.Framework.Assert.That(wrapper1.AdditionalFeeType2, NUnit.Framework.Is.EqualTo(dutyTaxFeeCodeList.GetDescriptionFromCode("B69")).Using(CustomComparers.TypeComparison), $"{message} B69");
				NUnit.Framework.Assert.That(wrapper1.AdditionalFeeType3, NUnit.Framework.Is.EqualTo(dutyTaxFeeCodeList.GetDescriptionFromCode("B79")).Using(CustomComparers.TypeComparison), $"{message} B79");
				NUnit.Framework.Assert.That(wrapper1.AdditionalFeeType4, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), $"{message} empty");

				NUnit.Framework.Assert.That(wrapper1.AdditionalFeeAmount1, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), $"{message} B19");
				NUnit.Framework.Assert.That(wrapper1.AdditionalFeeAmount2, NUnit.Framework.Is.EqualTo(200m).Using(CustomComparers.TypeComparison), $"{message} B69");
				NUnit.Framework.Assert.That(wrapper1.AdditionalFeeAmount3, NUnit.Framework.Is.EqualTo(300m).Using(CustomComparers.TypeComparison), $"{message} B79");
				NUnit.Framework.Assert.That(wrapper1.AdditionalFeeAmount4, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), $"{message} empty");
			});
		}

		[ExpectNoExceptions]
		public void TestAmountZone()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.AmountZone1, NUnit.Framework.Is.EqualTo(wrapper.DTYAmount).Using(CustomComparers.TypeComparison), "AmountZone1");
				NUnit.Framework.Assert.That(wrapper.AmountZone2, NUnit.Framework.Is.EqualTo(wrapper.VATAmount).Using(CustomComparers.TypeComparison), "AmountZone2");
				NUnit.Framework.Assert.That(wrapper.AmountZone3, NUnit.Framework.Is.EqualTo(wrapper.TPFAmount).Using(CustomComparers.TypeComparison), "AmountZone3");
				NUnit.Framework.Assert.That(wrapper.AmountZone4, NUnit.Framework.Is.EqualTo(wrapper.AdditionalFeeAmount1).Using(CustomComparers.TypeComparison), "AmountZone4");
				NUnit.Framework.Assert.That(wrapper.AmountZone5, NUnit.Framework.Is.EqualTo(wrapper.AdditionalFeeAmount2).Using(CustomComparers.TypeComparison), "AmountZone5");
				NUnit.Framework.Assert.That(wrapper.AmountZone6, NUnit.Framework.Is.EqualTo(wrapper.AdditionalFeeAmount3).Using(CustomComparers.TypeComparison), "AmountZone6");
				NUnit.Framework.Assert.That(wrapper.AmountZone7, NUnit.Framework.Is.EqualTo(wrapper.AdditionalFeeAmount4).Using(CustomComparers.TypeComparison), "AmountZone7");

				NUnit.Framework.Assert.That(wrapper.AmountZoneCaption1, NUnit.Framework.Is.EqualTo("進口稅").Using(CustomComparers.TypeComparison), "AmountZoneCaption1");
				NUnit.Framework.Assert.That(wrapper.AmountZoneCaption2, NUnit.Framework.Is.EqualTo("營業稅").Using(CustomComparers.TypeComparison), "AmountZoneCaption2");
				NUnit.Framework.Assert.That(wrapper.AmountZoneCaption3, NUnit.Framework.Is.EqualTo("進口推貿費").Using(CustomComparers.TypeComparison), "AmountZoneCaption3");
				NUnit.Framework.Assert.That(wrapper.AmountZoneCaption4, NUnit.Framework.Is.EqualTo(wrapper.AdditionalFeeType1).Using(CustomComparers.TypeComparison), "AmountZoneCaption4");
				NUnit.Framework.Assert.That(wrapper.AmountZoneCaption5, NUnit.Framework.Is.EqualTo(wrapper.AdditionalFeeType2).Using(CustomComparers.TypeComparison), "AmountZoneCaption5");
				NUnit.Framework.Assert.That(wrapper.AmountZoneCaption6, NUnit.Framework.Is.EqualTo(wrapper.AdditionalFeeType3).Using(CustomComparers.TypeComparison), "AmountZoneCaption6");
				NUnit.Framework.Assert.That(wrapper.AmountZoneCaption7, NUnit.Framework.Is.EqualTo(wrapper.AdditionalFeeType4).Using(CustomComparers.TypeComparison), "AmountZoneCaption7");
			});
		}

		[ExpectNoExceptions]
		public void TestTotalDutyTaxFeeAmount()
		{
			NUnit.Framework.Assert.That(wrapper.TotalDutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(23100m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestOtherChargeDeductionAmount()
		{
			NUnit.Framework.Assert.That(wrapper.OtherChargeDeductionAmount, NUnit.Framework.Is.EqualTo(18843m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGovernmentAgencyImage()
		{
			var governmentAgencyIDPrefix = "R99";
			for (var id = 1; id <= 100; id++)
			{
				var governmentAgencyID = governmentAgencyIDPrefix + id.ToString().PadLeft(2, '0');
				var message = Factory.New<N5110EDIMessage>();
				message.EM_MessageText = $@"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>740</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>100</AdValoremTaxBaseAmount><TypeCode>B19</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>200</AdValoremTaxBaseAmount><TypeCode>B69</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>300</AdValoremTaxBaseAmount><TypeCode>B79</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>{governmentAgencyID}</ID></ResponsibleGovernmentAgency></Declaration></Response>";
				var messageString = $"The government gency id is {governmentAgencyID}";
				var wrapper = new N5110EDIMessageDocumentWrapper(message);
				if (id >= 1 && id <= 99)
				{
					NUnit.Framework.Assert.That(ConvertImageToString(wrapper.GovernmentAgencyImage), NUnit.Framework.Is.EqualTo(ConvertImageToString(GetImage(governmentAgencyID))), messageString);
				}
				else if (id >= 100)
				{
					NUnit.Framework.Assert.That(wrapper.GovernmentAgencyImage, NUnit.Framework.Is.EqualTo(default(Image)), "messageString - should be [null]");
				}
			}
		}

		string ConvertImageToString(Image image)
		{
			var result = ZString.Empty;
			using (var ms = new MemoryStream())
			{
				image.Save(ms, image.RawFormat);
				result = System.Text.Encoding.UTF8.GetString(ms.ToArray());
			}
			return result;
		}

		Image GetImage(ZString governmentAgencyID)
		{
			ZString path = @"Enterprise.Customs.TW.Business.DocumentWrappers.N5110.Resources.";
			Image image = null;
			if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9901AndR9920(governmentAgencyID))
			{
				path += "GovernmentAgencyImageForIDIsBetweenR9901AndR9920.png";
			}
			else if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9921AndR9950(governmentAgencyID))
			{
				path += "GovernmentAgencyImageForIDIsBetweenR9921AndR9950.png";
			}
			else if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9951AndR9980(governmentAgencyID))
			{
				path += "GovernmentAgencyImageForIDIsBetweenR9951AndR9980.png";
			}
			else if (GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9981AndR9999(governmentAgencyID))
			{
				path += "GovernmentAgencyImageForIDIsBetweenR9981AndR9999.png";
			}
			else
			{
				path = ZString.Empty;
			}

			if (!path.IsEmpty)
			{
				using (var stream = typeof(N5110EDIMessageDocumentWrapper).Assembly.GetManifestResourceStream(path))
				{
					if (stream != null)
					{
						image = Image.FromStream(stream);
					}
				}
			}
			return image;
		}

		[ExpectNoExceptions]
		public void TestAccountNo()
		{
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(listType, "CustomsOffice");
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, listType, "CA", "臺北關業務一組", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, listType, "DA", "臺中關", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.BankAccountNo, "Bank Account Number", listType, Core.Constants.CountryCodes.Taiwan);
			codeList.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.BankAccountNo, "00000000000150");
			codeList2.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.BankAccountNo, "00000000000110");
			Factory.Save();

			NUnit.Framework.Assert.That(wrapper.AccountNo, NUnit.Framework.Is.EqualTo("NNNN").Using(CustomComparers.TypeComparison));

			message.EM_MessageText = MessageTextForTest2;
			wrapper = new N5110EDIMessageDocumentWrapper(message);
			NUnit.Framework.Assert.That(wrapper.AccountNo, NUnit.Framework.Is.EqualTo("00000000000110").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBarcode()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.Barcode1, NUnit.Framework.Is.EqualTo("1306186AW").Using(CustomComparers.TypeComparison), "The barcode 1");
				NUnit.Framework.Assert.That(wrapper.Barcode2, NUnit.Framework.Is.EqualTo("R9901DAI11180217670").Using(CustomComparers.TypeComparison), "The barcode 2");
				NUnit.Framework.Assert.That(wrapper.Barcode3, NUnit.Framework.Is.EqualTo("611RD0000023100").Using(CustomComparers.TypeComparison), "The barcode 3");
			});
			var message1 = Factory.New<N5110EDIMessage>();
			message1.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime></DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>740</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>100</AdValoremTaxBaseAmount><TypeCode>B19</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>200</AdValoremTaxBaseAmount><TypeCode>B69</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>300</AdValoremTaxBaseAmount><TypeCode>B79</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper1 = new N5110EDIMessageDocumentWrapper(message1);

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper1.Barcode1, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 1 when DueDateTime is empty");
				NUnit.Framework.Assert.That(wrapper1.Barcode2, NUnit.Framework.Is.EqualTo("R9981DAI11180217670").Using(CustomComparers.TypeComparison), "The barcode 2");
				NUnit.Framework.Assert.That(wrapper1.Barcode3, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 3");
			});

			var message2 = Factory.New<N5110EDIMessage>();
			message2.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode></tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>740</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>100</AdValoremTaxBaseAmount><TypeCode>B19</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>200</AdValoremTaxBaseAmount><TypeCode>B69</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>300</AdValoremTaxBaseAmount><TypeCode>B79</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper2 = new N5110EDIMessageDocumentWrapper(message2);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper2.Barcode1, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 1 when collection type is empty");
				NUnit.Framework.Assert.That(wrapper2.Barcode2, NUnit.Framework.Is.EqualTo("R9981DAI11180217670").Using(CustomComparers.TypeComparison), "The barcode 2");
				NUnit.Framework.Assert.That(wrapper2.Barcode3, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 3");
			});

			var message3 = Factory.New<N5110EDIMessage>();
			message3.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_CollectionTypeCode></tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>740</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>100</AdValoremTaxBaseAmount><TypeCode>B19</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>200</AdValoremTaxBaseAmount><TypeCode>B69</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>300</AdValoremTaxBaseAmount><TypeCode>B79</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID></ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper3 = new N5110EDIMessageDocumentWrapper(message3);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper3.Barcode1, NUnit.Framework.Is.EqualTo("0802136AW").Using(CustomComparers.TypeComparison), "The barcode 1");
				NUnit.Framework.Assert.That(wrapper3.Barcode2, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 2 when government agency is empty");
				NUnit.Framework.Assert.That(wrapper3.Barcode3, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 3");
			});

			var message4 = Factory.New<N5110EDIMessage>();
			message4.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>19251</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID></ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_CollectionTypeCode></tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>740</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>100</AdValoremTaxBaseAmount><TypeCode>B19</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>200</AdValoremTaxBaseAmount><TypeCode>B69</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>300</AdValoremTaxBaseAmount><TypeCode>B79</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper4 = new N5110EDIMessageDocumentWrapper(message4);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper4.Barcode1, NUnit.Framework.Is.EqualTo("0802136AW").Using(CustomComparers.TypeComparison), "The barcode 1");
				NUnit.Framework.Assert.That(wrapper4.Barcode2, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 2 when duty and tax fee reference id is empty");
				NUnit.Framework.Assert.That(wrapper4.Barcode3, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "The barcode 3");
			});

			var message5 = Factory.New<N5110EDIMessage>();
			message5.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05""><Status><NameCode>C1</NameCode></Status><Declaration><ID>DA08207F1027</ID><TotalPackageQuantity>4</TotalPackageQuantity><TypeCode>G1</TypeCode><Agent><ID>207</ID><RoleCode>CB</RoleCode><tw_SubBoxID>0</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-01-30</ArrivalDateTime></BorderTransportMeans><DutyTaxFee><tw_TotalDutyTaxFeeAmount>0</tw_TotalDutyTaxFeeAmount><Payment><DueDateTime>2019-02-13</DueDateTime><ReferenceID>DAI11180217670</ReferenceID><tw_CollectionTypeCode>6AW</tw_CollectionTypeCode><tw_CollectionTypeCode></tw_CollectionTypeCode><tw_IssueReasonCode>2</tw_IssueReasonCode></Payment></DutyTaxFee><GoodsShipment><Consignment><BorderTransportMeans><ID>BVCM7</ID><JourneyID>19017/19018</JourneyID></BorderTransportMeans><TransportContractDocument><ID>AAAA</ID><TypeCode>740</TypeCode></TransportContractDocument></Consignment><CustomsValuation><OtherChargeDeductionAmount>188436</OtherChargeDeductionAmount></CustomsValuation><DutyTaxFee><AdValoremTaxBaseAmount>100</AdValoremTaxBaseAmount><TypeCode>B19</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>200</AdValoremTaxBaseAmount><TypeCode>B69</TypeCode></DutyTaxFee><DutyTaxFee><AdValoremTaxBaseAmount>300</AdValoremTaxBaseAmount><TypeCode>B79</TypeCode></DutyTaxFee><GovernmentAgencyGoodsItem><Commodity><Classification><ID>73269090906</ID></Classification></Commodity></GovernmentAgencyGoodsItem></GoodsShipment><Importer><ID>11104755</ID><Name>UNIAUTOPARTSMANUFACTURECO.,LTD.</Name><tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName><tw_TypeCode>58</tw_TypeCode></Importer><Packaging><TypeCode>PKG</TypeCode></Packaging><ResponsibleGovernmentAgency><ID>R9981</ID></ResponsibleGovernmentAgency></Declaration></Response>";
			var wrapper5 = new N5110EDIMessageDocumentWrapper(message5);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper5.Barcode1, NUnit.Framework.Is.EqualTo("0802136AW").Using(CustomComparers.TypeComparison), "The barcode 1");
				NUnit.Framework.Assert.That(wrapper5.Barcode2, NUnit.Framework.Is.EqualTo("R9981DAI11180217670").Using(CustomComparers.TypeComparison), "The barcode 2");
				NUnit.Framework.Assert.That(wrapper5.Barcode3, NUnit.Framework.Is.EqualTo("611RA0000000000").Using(CustomComparers.TypeComparison), "The barcode 3 when total duty and tax fee amount is 0");
			});
		}

		public const string MessageTextForTest = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05"">
	<Status>
		<NameCode>C1</NameCode>
	</Status>
	<BankAccount>
		<ID>AAAA</ID>
		<ReferenceID>NNNN</ReferenceID>
	</BankAccount>
	<Declaration>
		<ID>DA  08207F1027</ID>
		<TotalPackageQuantity>4</TotalPackageQuantity>
		<TypeCode>G1</TypeCode>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<BorderTransportMeans>
			<ArrivalDateTime>2019-01-30</ArrivalDateTime>
		</BorderTransportMeans>
		<DutyTaxFee>
			<tw_TotalDutyTaxFeeAmount>23100</tw_TotalDutyTaxFeeAmount>
			<Payment>
				<DueDateTime>2024-06-18</DueDateTime>
				<ReferenceID>DAI11180217670</ReferenceID>
				<tw_CollectionTypeCode>6AW</tw_CollectionTypeCode>
				<tw_IssueReasonCode>2</tw_IssueReasonCode>
				<ObligationGuarantee>
					<ReferenceID>AAI11180217679</ReferenceID>
				</ObligationGuarantee>
			</Payment>
		</DutyTaxFee>
		<GoodsShipment>
			<Consignment>
				<BorderTransportMeans>
					<ID>BVCM7</ID>
					<JourneyID>19017/19018</JourneyID>
				</BorderTransportMeans>
				<TransportContractDocument>
					<ID>HHXTA19017336J</ID>
					<TypeCode>704</TypeCode>
				</TransportContractDocument>
				<TransportContractDocument>
					<ID>HBL0001</ID>
					<TypeCode>703</TypeCode>
				</TransportContractDocument>
			</Consignment>
			<CustomsValuation>
				<OtherChargeDeductionAmount>18843</OtherChargeDeductionAmount>
			</CustomsValuation>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>100</AdValoremTaxBaseAmount>
				<TypeCode>A10</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>200</AdValoremTaxBaseAmount>
				<TypeCode>A19</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>300</AdValoremTaxBaseAmount>
				<TypeCode>B40</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>400</AdValoremTaxBaseAmount>
				<TypeCode>B49</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>500</AdValoremTaxBaseAmount>
				<TypeCode>B51</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>600</AdValoremTaxBaseAmount>
				<TypeCode>B52</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>700</AdValoremTaxBaseAmount>
				<TypeCode>B59</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>800</AdValoremTaxBaseAmount>
				<TypeCode>A20</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>900</AdValoremTaxBaseAmount>
				<TypeCode>A30</TypeCode>
			</DutyTaxFee>

			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1000</AdValoremTaxBaseAmount>
				<TypeCode>A40</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1100</AdValoremTaxBaseAmount>
				<TypeCode>A50</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1200</AdValoremTaxBaseAmount>
				<TypeCode>B10</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1300</AdValoremTaxBaseAmount>
				<TypeCode>B31</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1400</AdValoremTaxBaseAmount>
				<TypeCode>B32</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1500</AdValoremTaxBaseAmount>
				<TypeCode>B60</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1600</AdValoremTaxBaseAmount>
				<TypeCode>C10</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1700</AdValoremTaxBaseAmount>
				<TypeCode>C20</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1800</AdValoremTaxBaseAmount>
				<TypeCode>B19</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1900</AdValoremTaxBaseAmount>
				<TypeCode>B69</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>2000</AdValoremTaxBaseAmount>
				<TypeCode>B79</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>2100</AdValoremTaxBaseAmount>
				<TypeCode>B89</TypeCode>
			</DutyTaxFee>
			<GovernmentAgencyGoodsItem>
				<Commodity>
					<Classification>
						<ID>73269090906</ID>
					</Classification>
				</Commodity>
			</GovernmentAgencyGoodsItem>
		</GoodsShipment>
		<Importer>
			<ID>11104755</ID>
			<Name>UNI AUTO PARTS MANUFACTURE CO., LTD.</Name>
			<tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName>
			<tw_TypeCode>58</tw_TypeCode>
		</Importer>
		<Packaging>
			<TypeCode>PKG</TypeCode>
		</Packaging>
		<ResponsibleGovernmentAgency>
			<ID>R9901</ID>
		</ResponsibleGovernmentAgency>
	</Declaration>
</Response>
";

		public const string MessageTextForTest2 = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Response xmlns:tsw=""urn:SingleWindow:TW"" xmlns:ds=""urn:wco:datamodel:WCO:DS:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:twds=""urn:wco:datamodel:TW:DS:1"" xmlns:ccts=""urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2"" xmlns=""urn:wco:datamodel:TW:N5110:R-00-05"">
	<Status>
		<NameCode>C1</NameCode>
	</Status>
	<BankAccount>
		<ID>AAAA</ID>
		<ReferenceID></ReferenceID>
	</BankAccount>
	<Declaration>
		<ID>DA  08207F1027</ID>
		<TotalPackageQuantity>4</TotalPackageQuantity>
		<TypeCode>G1</TypeCode>
		<Agent>
			<ID>207</ID>
			<RoleCode>CB</RoleCode>
			<tw_SubBoxID>0</tw_SubBoxID>
		</Agent>
		<BorderTransportMeans>
			<ArrivalDateTime>2019-01-30</ArrivalDateTime>
		</BorderTransportMeans>
		<DutyTaxFee>
			<tw_TotalDutyTaxFeeAmount>23100</tw_TotalDutyTaxFeeAmount>
			<Payment>
				<DueDateTime>2007-12-18</DueDateTime>
				<ReferenceID>DAI11180217670</ReferenceID>
				<tw_CollectionTypeCode>6AW</tw_CollectionTypeCode>
				<tw_IssueReasonCode>2</tw_IssueReasonCode>
				<ObligationGuarantee>
					<ReferenceID></ReferenceID>
				</ObligationGuarantee>
			</Payment>
		</DutyTaxFee>
		<GoodsShipment>
			<Consignment>
				<BorderTransportMeans>
					<ID>BVCM7</ID>
					<JourneyID>19017/19018</JourneyID>
				</BorderTransportMeans>
				<TransportContractDocument>
					<ID>HHXTA19017336J</ID>
					<TypeCode>704</TypeCode>
				</TransportContractDocument>
				<TransportContractDocument>
					<ID>HBL0001</ID>
					<TypeCode>703</TypeCode>
				</TransportContractDocument>
			</Consignment>
			<CustomsValuation>
				<OtherChargeDeductionAmount>18843</OtherChargeDeductionAmount>
			</CustomsValuation>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>100</AdValoremTaxBaseAmount>
				<TypeCode>A10</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>200</AdValoremTaxBaseAmount>
				<TypeCode>A19</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>300</AdValoremTaxBaseAmount>
				<TypeCode>B40</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>400</AdValoremTaxBaseAmount>
				<TypeCode>B49</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>500</AdValoremTaxBaseAmount>
				<TypeCode>B51</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>600</AdValoremTaxBaseAmount>
				<TypeCode>B52</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>700</AdValoremTaxBaseAmount>
				<TypeCode>B59</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>800</AdValoremTaxBaseAmount>
				<TypeCode>A20</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>900</AdValoremTaxBaseAmount>
				<TypeCode>A30</TypeCode>
			</DutyTaxFee>

			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1000</AdValoremTaxBaseAmount>
				<TypeCode>A40</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1100</AdValoremTaxBaseAmount>
				<TypeCode>A50</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1200</AdValoremTaxBaseAmount>
				<TypeCode>B10</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1300</AdValoremTaxBaseAmount>
				<TypeCode>B31</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1400</AdValoremTaxBaseAmount>
				<TypeCode>B32</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1500</AdValoremTaxBaseAmount>
				<TypeCode>B60</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1600</AdValoremTaxBaseAmount>
				<TypeCode>C10</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1700</AdValoremTaxBaseAmount>
				<TypeCode>C20</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1800</AdValoremTaxBaseAmount>
				<TypeCode>B19</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>1900</AdValoremTaxBaseAmount>
				<TypeCode>B69</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>2000</AdValoremTaxBaseAmount>
				<TypeCode>B79</TypeCode>
			</DutyTaxFee>
			<DutyTaxFee>
				<AdValoremTaxBaseAmount>2100</AdValoremTaxBaseAmount>
				<TypeCode>B89</TypeCode>
			</DutyTaxFee>
			<GovernmentAgencyGoodsItem>
				<Commodity>
					<Classification>
						<ID>73269090906</ID>
					</Classification>
				</Commodity>
			</GovernmentAgencyGoodsItem>
		</GoodsShipment>
		<Importer>
			<ID>11104755</ID>
			<Name></Name>
			<tw_ChineseName>友聯車材製造股份有限公司</tw_ChineseName>
			<tw_TypeCode>58</tw_TypeCode>
		</Importer>
		<Packaging>
			<TypeCode>PKG</TypeCode>
		</Packaging>
		<ResponsibleGovernmentAgency>
			<ID>R9901</ID>
		</ResponsibleGovernmentAgency>
	</Declaration>
</Response>
";
	}
}
