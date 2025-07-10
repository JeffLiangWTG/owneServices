using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoJobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUltimateConsigneeTypeList()
		{
			AssertEquals("UltimateConsigneeTypeList", typeof(UltimateConsigneeTypeList), invoice.AddInfoLookups.UltimateConsigneeTypeList.GetType());
		}

		public void TestSPIList()
		{
			AssertEquals("SPICompleteList", typeof(SPICompleteList), invoice.AddInfoLookups.SPIList.GetType());
		}

		public void TestUS_TariffTypeList()
		{
			AssertEquals("US_TariffTypeList", typeof(TariffTypeList), invoice.AddInfoLookups.US_TariffTypeList.GetType());
		}

		public void TestUS_PaymentTermsList()
		{
			AssertEquals(typeof(PaymentTermsTypeList), invoice.AddInfoLookups.US_PaymentTermsList.GetType());
		}

		public void TestUS_TermsOfDeliveryLocationIndicatorList()
		{
			AssertEquals(typeof(TermsOfDeliveryLocationCodeIndicators), invoice.AddInfoLookups.US_TermsOfDeliveryLocationIndicatorList.GetType());
		}

		public void TestUS_TermsOfDeliveryLocationList()
		{
			invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ScheduleD;
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), invoice.AddInfoLookups.US_TermsOfDeliveryLocationList.GetType());
			invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ScheduleK;
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), invoice.AddInfoLookups.US_TermsOfDeliveryLocationList.GetType());
			invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ISOCountryCode;
			AssertEquals(typeof(USCCountryCollection), invoice.AddInfoLookups.US_TermsOfDeliveryLocationList.GetType());
			invoice.US_TermsOfDeliveryLocationIndicator = "Z";
			AssertEquals(typeof(USCCountryCollection), invoice.AddInfoLookups.US_TermsOfDeliveryLocationList.GetType());
		}

		[TestDate(2018, 11, 20)]
		public void TestEntries()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "Z1Z2Z3Z4";
			declaration.IOROrgPK = importer1.PK;
			declaration.US_SchDEntry = "2305";
			invoice.US_ReleaseEntryNumber = "SV911112222";
			var entries = invoice.AddInfoLookups.Entries;
			AssertEquals("11112222", entries.FilterBusinessObjectDefaults["Entry Number (ENS):Property"].Value);
			AssertEquals(importer1.PK, entries.FilterBusinessObjectDefaults["Importer of Record:Property"].Value);
			AssertEquals("2305", entries.FilterBusinessObjectDefaults["Entry Port:Property"].Value);
			AssertEquals(new ZDateTime(2018, 11, 5), entries.FilterBusinessObjectDefaults["Release Date:Property1"].Value);
			AssertEquals(ZDateTime.Today, entries.FilterBusinessObjectDefaults["Release Date:Property2"].Value);
		}

		public void TestUS_LicenseType_List()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30 });
			var invoice = Factory.New<JobComInvoiceHeader>();
			var uS_LicenseType_List = invoice.AddInfoLookups.US_LicenseType_List;
			uS_LicenseType_List.Load();
			AssertEquals(USAESLicenseCode.Codes.C30, uS_LicenseType_List.Cast<ZZRefCusCodeListCombined>().FirstOrDefault().ZZD_Code);
		}

		[TestDate(2000, 11, 11)]
		public void TestSplitShipmentDetailsList()
		{
			void SetupSplit(ITAndSplitDetails split, ZString carrier, ZString flight, ZDateTime arrival)
			{
				split.US_CarrierCode = carrier;
				split.US_FlightNumber = flight;
				split.US_ArrivalDate = arrival;
				split.US_ITNumber = "1";
			}

			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				var houseBill1 = declaration.Bills.AddNew();
				houseBill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
				houseBill1.CU_BillNum = "H43289";
				houseBill1.US_SESplitShip = true;
				SetupSplit(houseBill1.ITAndSplitDetails.AddNew(), "C2", "FLT11", ZDateTime.Today);
				SetupSplit(houseBill1.ITAndSplitDetails.AddNew(), "C2", "FLT12", ZDateTime.Today.AddDays(10));
				var houseBill2 = declaration.Bills.AddNew();
				houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
				houseBill2.CU_BillNum = "H33289";
				houseBill2.US_SESplitShip = true;
				SetupSplit(houseBill2.ITAndSplitDetails.AddNew(), "C3", "FLT51", ZDateTime.Today);
				SetupSplit(houseBill2.ITAndSplitDetails.AddNew(), "C3", "FLT52", ZDateTime.Today.AddDays(10));
				var invoice = declaration.Invoices.AddNew();
				AssertEquals("List should be empty when bill is not selected", 0, invoice.AddInfoLookups.SplitShipmentDetailsList.Count);
				invoice.JZ_CU_RelatedHouseBill = houseBill1.PK;
				AssertContainsExactElementsInAnyOrder(new[] { "C2/FLT11/11-NOV-00", "C2/FLT12/21-NOV-00" }, invoice.AddInfoLookups.SplitShipmentDetailsList.GetAllCodes());
				invoice.JZ_CU_RelatedHouseBill = houseBill2.PK;
				AssertContainsExactElementsInAnyOrder(new[] { "C3/FLT51/11-NOV-00", "C3/FLT52/21-NOV-00" }, invoice.AddInfoLookups.SplitShipmentDetailsList.GetAllCodes());
				houseBill2.US_SESplitShip = false;
				AssertEquals("List should be empty when bill is not split", 0, invoice.AddInfoLookups.SplitShipmentDetailsList.Count);
			});
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
		}
	}
}
