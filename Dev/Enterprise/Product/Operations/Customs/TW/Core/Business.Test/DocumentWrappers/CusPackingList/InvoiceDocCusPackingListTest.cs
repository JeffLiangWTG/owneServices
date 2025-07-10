using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceDocCusPackingList))]
	sealed class InvoiceDocCusPackingListTest : DocCusPackingListTest
	{
		public override void TestSellerAddressData()
		{
			var supplierOrganization = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = jobdeclaration.Invoices.AddNew();
			invoice.SupplierDocumentaryAddress.OrganisationPK = supplierOrganization.PK;

			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;

			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", "Main CompanyName", cusPackingListWrapper.SellerAddressData.CompanyName);
				AssertEquals("Address", "ADDRESS 1 TAIWAN", cusPackingListWrapper.SellerAddressData.Address);
			});
		}

		public override void TestSellerDocAddress()
		{
			var supplierOrganization = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = jobdeclaration.Invoices.AddNew();
			invoice.SupplierDocumentaryAddress.OrganisationPK = supplierOrganization.PK;

			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;

			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals(supplierOrganization.PK, (cusPackingListWrapper.SellerDocAddress.WrappedObject as TWJobDocAddress).OrganisationPK);
		}

		public override void TestBuyerAddressData()
		{
			var buyerOrganization = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = jobdeclaration.Invoices.AddNew();
			invoice.BuyerDocumentaryAddress.OrganisationPK = buyerOrganization.PK;

			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;

			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", "Main CompanyName", cusPackingListWrapper.BuyerAddressData.CompanyName);
				AssertEquals("Address", "ADDRESS 1 TAIWAN", cusPackingListWrapper.BuyerAddressData.Address);
			});
		}

		public override void TestBuyerDocAddress()
		{
			var buyerOrganization = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = jobdeclaration.Invoices.AddNew();
			invoice.BuyerDocumentaryAddress.OrganisationPK = buyerOrganization.PK;

			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;

			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals(buyerOrganization.PK, (cusPackingListWrapper.BuyerDocAddress.WrappedObject as TWJobDocAddress).OrganisationPK);
		}

		public override void TestMarksAndNumbers()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice1 = jobdeclaration.Invoices.AddNew();
			invoice1.JZ_MarksAndNumbers = "Marks and numbers 1";
			var invoice2 = jobdeclaration.Invoices.AddNew();
			invoice2.JZ_MarksAndNumbers = "Marks and numbers 2";

			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice1.PK;

			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("Marks and numbers 1", cusPackingListWrapper.MarksAndNumbers);
		}

		public override void TestPortOfOriginName()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_RL_NKOrigin = "TWTPE";
			var invoice = jobdeclaration.Invoices.AddNew();
			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("Taiwan - Taipei", cusPackingListWrapper.PortOfOriginName);
		}

		public void TestPortOfOriginNameWithInvoiceNotAttachedToDeclaration()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var transport = invoice.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport.JW_RL_NKLoadPortForBinding = "TWTPE";
			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("Taiwan - Taipei", cusPackingListWrapper.PortOfOriginName);
		}

		public override void TestFinalDestinationName()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_RL_NKFinalDestination = "TWTPE";
			var invoice = jobdeclaration.Invoices.AddNew();
			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("Taiwan - Taipei", cusPackingListWrapper.FinalDestinationName);
		}

		public void TestFinalDestinationNameWithInvoiceNotAttachedToDeclaration()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var transport = invoice.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport.JW_RL_NKDiscPortForBinding = "TWTPE";
			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("Taiwan - Taipei", cusPackingListWrapper.FinalDestinationName);
		}

		public override void TestTransportation()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = jobdeclaration.Invoices.AddNew();
			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("AIR FREIGHT", cusPackingListWrapper.Transportation);
		}

		public void TestTransportationWithInvoiceNotAttachedToDeclaration()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var transport = invoice.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport.JW_RL_NKLoadPort = "TWKEL";
			transport.JW_RL_NKDiscPort = "USLAX";
			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("AIR FREIGHT", cusPackingListWrapper.Transportation);
		}

		public override void TestNotify()
		{
			var org = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_OH_NotifyParty = org.PK;
			var invoice = jobdeclaration.Invoices.AddNew();
			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("FAX", cusPackingListWrapper.Notify.Fax);
			AssertEquals("Email", cusPackingListWrapper.Notify.Email);
			AssertEquals("Phone", cusPackingListWrapper.Notify.Phone);
		}

		public override void TestNotifyPartyDetails()
		{
			var org = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_OH_NotifyParty = org.PK;
			var invoice = jobdeclaration.Invoices.AddNew();
			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("Main CompanyName", cusPackingListWrapper.NotifyPartyDetails.Name);
			AssertEquals("ADDRESS 1 TAIWAN", cusPackingListWrapper.NotifyPartyDetails.Address.Line);
		}

		protected override DocCusPackingList CreatePackingListWrapper(CusPackingList packingList)
		{
			return InvoiceDocCusPackingList.New(packingList, packingList.Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return InvoiceDocCusPackingList.New(packingListInternal, packingListInternal.Factory);
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ZHT", "繁體中文");
			helper.CreateNewOrGetExistingCusCodeType("TWPUM", "Taiwan Packing Units of Measurement");

			var cusCodeList = helper.CreateNewOrGetExistingCusCodeList("TW", "TWPUM", "BOX", "Box", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListLanguage(cusCodeList, "ZHT", "箱");

			helper.CreateNewOrGetExistingCusCodeList("TW", "TWPUM", "CRT", "Crate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", "TWPUM", "CTN", "Carton", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", "TWPUM", "PKG", "Package", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("TW", "TWPUM", "PLT", "Pallet", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = jobdeclaration.Invoices.AddNew();
			packingListInternal = invoice.CreateCusPackingList(Factory);
			packingListInternal.CUL_Remarks = "Test Remarks";
			packingListInternal.CUL_PackingListNumber = "Test Number";
			packingListInternal.CUL_PackingListDate = new ZDate(2020, 11, 10);
			packingListInternal.CUL_CustomAttribute1 = "Custom Attribute 1";
			packingListInternal.CUL_CustomAttribute2 = "Custom Attribute 2";
			packingListInternal.CUL_CustomFlag1 = true;
			packingListInternal.CUL_CustomFlag2 = false;
			packingListInternal.CUL_CustomDate1 = new ZDate(2021, 01, 22);
			packingListInternal.CUL_CustomDate2 = new ZDate(2021, 01, 23);
			packingListInternal.CUL_CustomDecimal1 = 20m;
			packingListInternal.CUL_CustomDecimal2 = 22m;
		}
	}
}
