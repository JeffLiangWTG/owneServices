using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class BaseTradeNetPermitTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTradeNetVersion()
		{
			AssertEquals("TradeNetVersion", "041", Permit.TradeNetVersion);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPermitNumber()
		{
			AssertEquals("PermitNumber", "IN6I100178S", Permit.PermitNumber);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUniqueRef()
		{
			InNonPaymentPermit.Declaration.Header.UniqueReferenceNumber.SequenceNumeric = "701";
			AssertEquals("UniqueRef", "199702247W 20200520 0701", Permit.UniqueRef);
			InNonPaymentPermit.Declaration.Header.UniqueReferenceNumber.SequenceNumeric = "8000";
			AssertEquals("UniqueRef", "199702247W 20200520 8000", Permit.UniqueRef);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageType()
		{
			AssertEquals("MessageType", "IN-NON-PAYMENT PERMIT", Permit.MessageType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeclarationType()
		{
			AssertEquals("DeclarationType", "DESTRUCTION", Permit.DeclarationType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImporter()
		{
			AssertEquals("Importer", "", Permit.Importer);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExporter()
		{
			AssertEquals("Exporter", "", Permit.Exporter);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHandlingAgent()
		{
			AssertEquals("HandlingAgent", "", Permit.HandlingAgent);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInwardCarrierAgent()
		{
			AssertEquals("InwardCarrierAgent", "", Permit.InwardCarrierAgent);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutwardCarrierAgent()
		{
			AssertEquals("OutwardCarrierAgent", "", Permit.OutwardCarrierAgent);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNameOfCompany()
		{
			AssertEquals("NameOfCompany", "TESTING1 Declaring Agent Party INTERNET", Permit.NameOfCompany);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEntityIdentOfCompany()
		{
			AssertEquals("EntityIdentOfCompany", string.Empty, Permit.EntityIdentOfCompany);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeclarantName()
		{
			AssertEquals("DeclarantName", "TESTING1", Permit.DeclarantName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeclarantCode()
		{
			AssertEquals("DeclarantCode", "XXX6666Z", Permit.DeclarantCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTelNb()
		{
			AssertEquals("TelNb", "63111111", Permit.TelNb);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestManufacturerName()
		{
			AssertEquals("ManufacturerName", "TEST Manufacturer Party Name", Permit.ManufacturerName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPortOfLoading()
		{
			AssertEquals("PortOfLoading", "HKHKG", Permit.PortOfLoading);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPortOfDischarge()
		{
			AssertEquals("PortOfDischarge", "THBKK", Permit.PortOfDischarge);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNextPortOfCall()
		{
			AssertEquals("NextPortOfCall", "THBKA", Permit.NextPortOfCall);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFinalPortOfCall()
		{
			AssertEquals("FinalPortOfCall", "THBKB", Permit.FinalPortOfCall);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCountryOfFinalDest()
		{
			AssertEquals("CountryOfFinalDest", "THAILAND", Permit.CountryOfFinalDest);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPlaceOfReleaseName()
		{
			AssertEquals("PlaceOfReleaseName", "CHANGI FTZ", Permit.PlaceOfReleaseName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPlaceOfReleaseCode()
		{
			AssertEquals("PlaceOfReleaseCode", "CZ", Permit.PlaceOfReleaseCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPlaceOfReceiptName()
		{
			AssertEquals("PlaceOfReceiptName", "SENOKO INCINERATION PLANT, 30 ATTAP VALLEY ROAD", Permit.PlaceOfReceiptName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPlaceOfReceiptCode()
		{
			AssertEquals("PlaceOfReceiptCode", "DUMP", Permit.PlaceOfReceiptCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidityPeriodFrom()
		{
			AssertEquals("ValidityPeriodFrom", new ZDate("14-Feb-11 00:00:00"), Permit.ValidityPeriodFrom);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidityPeriodTo()
		{
			AssertEquals("ValidityPeriodTo", new ZDate("26-Sep-06 00:00:00"), Permit.ValidityPeriodTo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalGrossWt()
		{
			AssertEquals("TotalGrossWt", "1.780/KGM".PadLeft(19, ' '), Permit.TotalGrossWt);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalOuterPack()
		{
			AssertEquals("TotalOuterPack", "1/PKG".PadLeft(12, ' '), Permit.TotalOuterPack);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalCustomsDUTPayable()
		{
			AssertEquals("TotalCustomsDUTPayable", 5.18m, Permit.TotalCustomsDUTPayable);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalOtherTaxPayable()
		{
			AssertEquals("TotalOtherTaxPayable", 6.75m, Permit.TotalOtherTaxPayable);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalExciseDUTPayable()
		{
			AssertEquals("TotalExciseDUTPayable", 8.00m, Permit.TotalExciseDUTPayable);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalGstAmount()
		{
			AssertEquals("TotalGstAmount", 97.30m, Permit.TotalGstAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalAmountPayable()
		{
			AssertEquals("TotalAmountPayable", 12.00m, Permit.TotalAmountPayable);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCargoPackingType()
		{
			AssertEquals("CargoPackingType", "OTHER NON-CONTAINERIZED", Permit.CargoPackingType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInVesName()
		{
			AssertEquals("InVesName", "CN HKG", Permit.InVesName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInVoyageFlightNumber()
		{
			AssertEquals("InVoyageFlightNumber", "SQ857", Permit.InVoyageFlightNumber);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInOBLMawbNb()
		{
			AssertEquals("InOBLMawbNb", "61835279576", Permit.InOBLMawbNb);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestArrivalDate()
		{
			AssertEquals("ArrivalDate", new ZDate("06-Feb-04 00:00:00"), Permit.ArrivalDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutVesName()
		{
			AssertEquals("OutVesName", "ACX IYO", Permit.OutVesName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutVesLocation()
		{
			AssertEquals("OutVesLocation", "", Permit.OutVesLocation);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutVoyageFlightNumber()
		{
			AssertEquals("OutVoyageFlightNumber", "074N", Permit.OutVoyageFlightNumber);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTowingVesselName()
		{
			AssertEquals("TowingVesselName", "", Permit.TowingVesselName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutOBLMawbNb()
		{
			AssertEquals("OutOBLMawbNb", "NYKASGN92858", Permit.OutOBLMawbNb);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDepartureDate()
		{
			AssertEquals("DepartureDate", new ZDate("14-Feb-11"), Permit.DepartureDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLicenceNo()
		{
			AssertEquals("LicenceNo", "AE/001688/2005/T", Permit.LicenceNo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCertificateNo()
		{
			AssertEquals("CertificateNo", "647836", Permit.CertificateNo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomsProcedureCodes()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "", "110", "", "1000", "AEO (IPT GST)", "IPT");
			Factory.Save();
			AssertEquals("CustomsProcedureCodes", "AEO", Permit.CustomsProcedureCodes);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideMawbLine()
		{
			AssertEquals("HideMawbLine", false, Permit.HideMawbLine);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideHawbLine()
		{
			AssertEquals("HideHawbLine", false, Permit.HideHawbLine);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideCifFobValue()
		{
			AssertEquals("HideCifFobValue", false, Permit.HideCifFobValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideLspValue()
		{
			AssertEquals("HideLspValue", false, Permit.HideLspValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideGstValue()
		{
			AssertEquals("HideGstValue", false, Permit.HideGstValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideDutQtyWtVolValue()
		{
			AssertEquals("HideDutQtyWtVolValue", false, Permit.HideDutQtyWtVolValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideUnitPriceValue()
		{
			AssertEquals("HideUnitPriceValue", false, Permit.HideUnitPriceValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideExciseValue()
		{
			AssertEquals("HideExciseValue", false, Permit.HideExciseValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideDutyValue()
		{
			AssertEquals("HideDutyValue", false, Permit.HideDutyValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHideOtherTaxValue()
		{
			AssertEquals("HideOtherTaxValue", false, Permit.HideOtherTaxValue);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTradersRemark()
		{
			AssertArrayEqualsByElements("TradersRemarks", new ZString[] { "REF: D/O", "NO: 000016185" }, Permit.TradersRemark);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAmendDateAndFields()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			var path = BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\XML\INPUPT.XML";
			message.EM_MessageText = System.IO.File.ReadAllText(path);
			Factory.Save();
			var tradeNetPermit = new BaseTradeNetPermit(Factory, message.TradenetResponse.OutboundMessage.InNonPaymentUpdatePermit);
			AssertEquals("AmendDate", new ZDate("2011-02-14"), tradeNetPermit.AmendDate);
			AssertArrayEqualsByElements("AmendFields", new ZString[] { "DEPARTURE DATE" }, tradeNetPermit.AmendFields);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConsignmentDetails()
		{
			AssertNotNull("ConsignmentDetails", Permit.ConsignmentDetails);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContainerIdentifiers()
		{
			AssertNotNull("ContainerIdentifiers", Permit.ContainerIdentifiers);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImporterNameLine1()
		{
			AssertEquals("ImporterNameLine1", "TESTING1 Importer Party INTERNET", Permit.ImporterNameLine1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImporterNameLine2()
		{
			AssertEquals("ImporterNameLine2", "", Permit.ImporterNameLine2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImporterUEN()
		{
			AssertEquals("ImporterUEN", "IP001", Permit.ImporterUEN);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExporterNameLine1()
		{
			AssertEquals("ExporterNameLine1", "TESTING1 Exporter Party INTERNET", Permit.ExporterNameLine1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExporterNameLine2()
		{
			AssertEquals("ExporterNameLine2", "", Permit.ExporterNameLine2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExporterUEN()
		{
			AssertEquals("ExporterUEN", "EP001", Permit.ExporterUEN);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHandlingAgentNameLine1()
		{
			AssertEquals("HandlingAgentNameLine1", "TEST Handling Agent Party Name", Permit.HandlingAgentNameLine1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHandlingAgentNameLine2()
		{
			AssertEquals("HandlingAgentNameLine2", "", Permit.HandlingAgentNameLine2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHandlingAgentNameLine3()
		{
			AssertEquals("HandlingAgentNameLine3", "", Permit.HandlingAgentNameLine3);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHandlingAgentUEN()
		{
			AssertEquals("HandlingAgentUEN", "AHP0001", Permit.HandlingAgentUEN);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInwardCarrierAgentNameLine1()
		{
			AssertEquals("InwardCarrierAgentNameLine1", "SATS CARGO SERVICES PTE LTD", Permit.InwardCarrierAgentNameLine1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInwardCarrierAgentNameLine2()
		{
			AssertEquals("InwardCarrierAgentNameLine2", "", Permit.InwardCarrierAgentNameLine2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInwardCarrierAgentNameLine3()
		{
			AssertEquals("InwardCarrierAgentNameLine3", "", Permit.InwardCarrierAgentNameLine3);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutwardCarrierAgentNameLine1()
		{
			AssertEquals("OutwardCarrierAgentNameLine1", "MARINA LOGISTICS (S) PTE LTD", Permit.OutwardCarrierAgentNameLine1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutwardCarrierAgentNameLine2()
		{
			var bo = Factory.New<JobDeclaration>();
			bo.CreateNewFactory();
			AssertEquals("OutwardCarrierAgentNameLine2", "", Permit.OutwardCarrierAgentNameLine2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOutwardCarrierAgentNameLine3()
		{
			AssertEquals("OutwardCarrierAgentNameLine3", "", Permit.OutwardCarrierAgentNameLine3);
		}

		BaseTradeNetPermit Permit => permit ?? (permit = new BaseTradeNetPermit(Factory, InNonPaymentPermit));
		BaseTradeNetPermit permit;

		InNonPaymentPermit InNonPaymentPermit
		{
			get
			{
				if (inNonPaymentPermit == null)
				{
					var message = Factory.New<SGXmlEDIMessage>();
					var path = BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\XML\CommonPermit.XML";
					message.EM_MessageText = System.IO.File.ReadAllText(path);
					Factory.Save();
					inNonPaymentPermit = message.TradenetResponse.OutboundMessage.InNonPaymentPermit;
				}

				return inNonPaymentPermit;
			}
		}

		InNonPaymentPermit inNonPaymentPermit;
	}
}
