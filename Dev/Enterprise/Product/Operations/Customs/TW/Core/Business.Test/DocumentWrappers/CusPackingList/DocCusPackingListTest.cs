using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class DocCusPackingListTest : DocBaseWrapperTest
	{
		protected OrgHeader SetUpOrgHeaderForAddressTest()
		{
			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "org01";
			var mainAddress = organization.Addresses.MainAddress;
			mainAddress.CompanyName = "Main CompanyName";
			mainAddress.Address1 = "Address 1";
			mainAddress.OA_Fax = "FAX";
			mainAddress.OA_Email = "Email";
			mainAddress.OA_Phone = "Phone";
			return organization;
		}

		public virtual void TestSellerAddressData()
		{
			var supplierOrganization = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.SupplierDocumentaryAddress.OrganisationPK = supplierOrganization.PK;

			var packingList = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);

			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", "Main CompanyName", cusPackingListWrapper.SellerAddressData.CompanyName);
				AssertEquals("Address", "ADDRESS 1 TAIWAN", cusPackingListWrapper.SellerAddressData.Address);
			});
		}

		public virtual void TestSellerDocAddress()
		{
			var supplierOrganization = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.SupplierDocumentaryAddress.OrganisationPK = supplierOrganization.PK;

			var packingList = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);

			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals(supplierOrganization.PK, (cusPackingListWrapper.SellerDocAddress.WrappedObject as TWJobDocAddress).OrganisationPK);
		}

		public virtual void TestBuyerAddressData()
		{
			var buyerOrganization = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.ImporterDocumentaryAddress.OrganisationPK = buyerOrganization.PK;

			var packingList = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);

			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", "Main CompanyName", cusPackingListWrapper.BuyerAddressData.CompanyName);
				AssertEquals("Address", "ADDRESS 1 TAIWAN", cusPackingListWrapper.BuyerAddressData.Address);
			});
		}

		public virtual void TestBuyerDocAddress()
		{
			var buyerOrganization = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.ImporterDocumentaryAddress.OrganisationPK = buyerOrganization.PK;

			var packingList = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);

			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals(buyerOrganization.PK, (cusPackingListWrapper.BuyerDocAddress.WrappedObject as TWJobDocAddress).OrganisationPK);
		}

		public virtual void TestMarksAndNumbers()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = jobdeclaration.Invoices.AddNew();
			invoice.TW_MarksAndNumbers = "Marks and numbers 1";
			invoice = jobdeclaration.Invoices.AddNew();
			invoice.TW_MarksAndNumbers = "Marks and numbers 2";

			var packingList = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("Marks and numbers 1\r\nMarks and numbers 2", cusPackingListWrapper.MarksAndNumbers);
		}

		public virtual void TestPortOfOriginName()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_RL_NKOrigin = "TWTPE";
			var packingList = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("Taiwan - Taipei", cusPackingListWrapper.PortOfOriginName);
		}

		public virtual void TestFinalDestinationName()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_RL_NKFinalDestination = "TWTPE";
			var packingList = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("Taiwan - Taipei", cusPackingListWrapper.FinalDestinationName);
		}

		public virtual void TestTransportation()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var packingList = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("AIR FREIGHT", cusPackingListWrapper.Transportation);
		}

		public virtual void TestNotify()
		{
			var org = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_OH_NotifyParty = org.PK;
			var packingList = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("FAX", cusPackingListWrapper.Notify.Fax);
			AssertEquals("Email", cusPackingListWrapper.Notify.Email);
			AssertEquals("Phone", cusPackingListWrapper.Notify.Phone);
		}

		public virtual void TestNotifyPartyDetails()
		{
			var org = SetUpOrgHeaderForAddressTest();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_OH_NotifyParty = org.PK;
			var packingList = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);
			var cusPackingListWrapper = CreatePackingListWrapper(packingList);
			AssertEquals("Main CompanyName", cusPackingListWrapper.NotifyPartyDetails.Name);
			AssertEquals("ADDRESS 1 TAIWAN", cusPackingListWrapper.NotifyPartyDetails.Address.Line);
		}

		[TestDate(2017, 12, 26)]
		public void TestSayTotalDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWCustomsPackUnits, "Taiwan Customs Pack Units");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWCustomsPackUnits, "ROL", "Roll", new ZDateTime(2017, 12, 25), new ZDateTime(2017, 12, 30));
			Factory.Save();

			var packageJob = packingListInternal.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_MarksAndNumbers = "1~40";
			package1.KP_PackageQty = 40;
			package1.KP_F3_NKPackType = "CTN";

			var package2 = packageJob.Packages.AddNew();
			package2.KP_MarksAndNumbers = "1~26";
			package2.KP_PackageQty = 26;
			package2.KP_F3_NKPackType = "CTN";
			AssertEquals("SAY TOTAL SIXTY SIX (66) CARTONS ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package2.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL SIXTY SIX (66) PACKAGES ONLY.(=FORTY (40) CARTONS AND TWENTY SIX (26) BOXES ONLY.)", CusPackingListWrapperInternal.SayTotalDescription);

			var package3 = packageJob.Packages.AddNew();
			package3.KP_MarksAndNumbers = "*";
			package3.KP_PackageQty = 1;
			package3.KP_F3_NKPackType = "PLT";

			var package4 = packageJob.Packages.AddNew();
			package4.KP_MarksAndNumbers = "*";
			package4.KP_PackageQty = 1;
			package4.KP_F3_NKPackType = "PLT";
			AssertEquals("SAY TOTAL TWO (2) PALLETS ONLY.(=FORTY (40) CARTONS AND TWENTY SIX (26) BOXES ONLY.)", CusPackingListWrapperInternal.SayTotalDescription);

			package4.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL TWO (2) PACKAGES ONLY.(=ONE (1) PALLET AND ONE (1) BOX ONLY.)", CusPackingListWrapperInternal.SayTotalDescription);

			package2.KP_F3_NKPackType = "CTN";
			AssertEquals("SAY TOTAL TWO (2) PACKAGES ONLY.(=ONE (1) PALLET AND ONE (1) BOX ONLY.)", CusPackingListWrapperInternal.SayTotalDescription);

			packingListInternal.CUL_PackageDescription = "(=24ROL)";
			AssertEquals("SAY TOTAL TWO (2) PACKAGES ONLY.(=TWENTY FOUR (24) ROLLS ONLY.)", CusPackingListWrapperInternal.SayTotalDescription);
		}

		public void TestSayTotalDescription_NumberOfPackTypes()
		{
			var packageJob = packingListInternal.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_PackageQty = 1;
			package1.KP_F3_NKPackType = ZString.Empty;
			AssertNullOrEmpty(CusPackingListWrapperInternal.SayTotalDescription);

			package1.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL ONE (1) BOX ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			var package2 = packageJob.Packages.AddNew();
			package2.KP_PackageQty = 1;
			package2.KP_F3_NKPackType = "CTN";
			AssertContains(" AND ", CusPackingListWrapperInternal.SayTotalDescription);
			AssertNotContains(",", CusPackingListWrapperInternal.SayTotalDescription);

			var package3 = packageJob.Packages.AddNew();
			package3.KP_PackageQty = 1;
			package3.KP_F3_NKPackType = "PKG";
			AssertContains(" AND ", CusPackingListWrapperInternal.SayTotalDescription);
			AssertContains(", ", CusPackingListWrapperInternal.SayTotalDescription);
		}

		public void TestSayTotalDescription_PluralForms()
		{
			var packageJob = packingListInternal.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_PackageQty = 1;
			package1.KP_F3_NKPackType = "BOX";

			var package2 = packageJob.Packages.AddNew();
			package2.KP_PackageQty = 1;
			package2.KP_F3_NKPackType = "CRT";

			var package3 = packageJob.Packages.AddNew();
			package3.KP_PackageQty = 1;
			package3.KP_F3_NKPackType = "CTN";

			var package4 = packageJob.Packages.AddNew();
			package4.KP_PackageQty = 1;
			package4.KP_F3_NKPackType = "PKG";

			var package5 = packageJob.Packages.AddNew();
			package5.KP_PackageQty = 1;
			package5.KP_F3_NKPackType = "PLT";

			var singularForms = new[] { "CRATE", "CARTON", "BOX", "PACKAGE", "PALLET" };
			var description = CusPackingListWrapperInternal.SayTotalDescription;
			foreach (var singularForm in singularForms)
			{
				AssertContains(singularForm, description);
			}

			package1.KP_PackageQty = 2;
			package2.KP_PackageQty = 2;
			package3.KP_PackageQty = 2;
			package4.KP_PackageQty = 2;
			package5.KP_PackageQty = 2;

			var pluralForms = new[] { "CRATES", "CARTONS", "BOXES", "PACKAGES", "PALLETS" };
			description = CusPackingListWrapperInternal.SayTotalDescription;
			foreach (var pluralForm in pluralForms)
			{
				AssertContains(pluralForm, description);
			}
		}

		public void TestSayTotalDescription_NumberInEnglish()
		{
			var packageJob = packingListInternal.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.KP_PackageQty = 3;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL THREE (3) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 14;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL FOURTEEN (14) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 50;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL FIFTY (50) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 52;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL FIFTY TWO (52) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 602;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL SIX HUNDRED AND TWO (602) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 3021;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL THREE THOUSAND, TWENTY ONE (3021) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 3821;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL THREE THOUSAND, EIGHT HUNDRED AND TWENTY ONE (3821) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 214121;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL TWO HUNDRED AND FOURTEEN THOUSAND, ONE HUNDRED AND TWENTY ONE (214121) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 1000003;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL ONE MILLION, THREE (1000003) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 234001000;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL TWO HUNDRED AND THIRTY FOUR MILLION, ONE THOUSAND (234001000) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);
		}

		public void TestSayTotalDescription_InvalidPackType()
		{
			var packageJob = packingListInternal.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.KP_PackageQty = 3;
			package.KP_F3_NKPackType = "X01";

			AssertNullOrEmpty(CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 14;
			package.KP_F3_NKPackType = "BOX";
			AssertEquals("SAY TOTAL FOURTEEN (14) BOXES ONLY.", CusPackingListWrapperInternal.SayTotalDescription);

			package.KP_PackageQty = 50;
			package.KP_F3_NKPackType = "X02";
			AssertNullOrEmpty(CusPackingListWrapperInternal.SayTotalDescription);
		}

		public void TestPackNumber()
		{
			AssertEquals("PackNumber", "Test Number", CusPackingListWrapperInternal.PackNumber);
		}

		public void TestRemarks()
		{
			packingListInternal.CUL_Remarks = @"Test Remarks1
Test Remarks2
Test Remarks3
Test Remarks4
Test Remarks5
Test Remarks6
Test Remarks7
Test Remarks8
Test Remarks9
Test Remarks10
Test Remarks11
Test Remarks12
Test Remarks13";
			AssertEquals("RemarksPart1", @"Test Remarks1
Test Remarks2
Test Remarks3
Test Remarks4
Test Remarks5
Test Remarks6
Test Remarks7
Test Remarks8
Test Remarks9
Test Remarks10
Test Remarks11
Test Remarks12
Test Remarks13", CusPackingListWrapperInternal.Remarks);
		}

		public void TestPackDate()
		{
			AssertEquals("PackDate", "2020-11-10", CusPackingListWrapperInternal.PackDate);
		}

		public void TestCustomAttribute1()
		{
			AssertEquals("CustomAttribute1", "Custom Attribute 1", CusPackingListWrapperInternal.CustomAttribute1);
		}

		public void TestCustomAttribute2()
		{
			AssertEquals("CustomAttribute2", "Custom Attribute 2", CusPackingListWrapperInternal.CustomAttribute2);
		}

		public void TestCustomFlag1()
		{
			AssertEquals("CustomFlag1", true, CusPackingListWrapperInternal.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			AssertEquals("CustomFlag2", false, CusPackingListWrapperInternal.CustomFlag2);
		}

		public void TestCustomDate1()
		{
			AssertEquals("CustomDate1", new ZDate(2021, 01, 22), CusPackingListWrapperInternal.CustomDate1);
		}

		public void TestCustomDate2()
		{
			AssertEquals("CustomDate2", new ZDate(2021, 01, 23), CusPackingListWrapperInternal.CustomDate2);
		}

		public void TestCustomDecimal1()
		{
			AssertEquals("CustomDecimal1", 20m, CusPackingListWrapperInternal.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			AssertEquals("CustomDecimal2", 22m, CusPackingListWrapperInternal.CustomDecimal2);
		}

		public void TestNetWeightSummary()
		{
			var packageJob = packingListInternal.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.NetWeight = 1;
			package.KP_WeightUQ = "KG";
			AssertEquals("NetWeightSummary", "1 KG\r\nvvvvvvvv", CusPackingListWrapperInternal.NetWeightSummary);

			package = packageJob.Packages.AddNew();
			package.NetWeight = 2;
			package.KP_WeightUQ = "LB";
			AssertEquals("NetWeightSummary", "1 KG\n2 LB\r\nvvvvvvvv", CusPackingListWrapperInternal.NetWeightSummary);

			package.KP_MarksAndNumbers = "*";
			AssertEquals("NetWeightSummary", "1 KG\r\nvvvvvvvv", CusPackingListWrapperInternal.NetWeightSummary);
		}

		public void TestGrossWeightSummary()
		{
			var packageJob = packingListInternal.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.KP_Weight = 1;
			package.KP_WeightUQ = "KG";
			AssertEquals("GrossWeightSummary", "1 KG\r\nvvvvvvvv", CusPackingListWrapperInternal.GrossWeightSummary);

			package = packageJob.Packages.AddNew();
			package.KP_Weight = 2;
			package.KP_WeightUQ = "LB";
			AssertEquals("GrossWeightSummary", "1 KG\n2 LB\r\nvvvvvvvv", CusPackingListWrapperInternal.GrossWeightSummary);
		}

		public void TestVolumeSummary()
		{
			var packageJob = packingListInternal.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.KP_Volume = 1;
			package.KP_VolumeUQ = "M3";
			AssertEquals("VolumeSummary", "1 CBM\r\nvvvvvvvvvv", CusPackingListWrapperInternal.VolumeSummary);

			package = packageJob.Packages.AddNew();
			package.KP_Volume = 2;
			package.KP_VolumeUQ = "CY";
			AssertEquals("VolumeSummary", "1 CBM\n2 CY\r\nvvvvvvvvvv", CusPackingListWrapperInternal.VolumeSummary);

			package.KP_MarksAndNumbers = "*";
			AssertEquals("VolumeSummary", "1 CBM\r\nvvvvvvvvvv", CusPackingListWrapperInternal.VolumeSummary);
		}

		public void TestPackTypeSummary()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine.JI_Description = "line1";
			invoieceLine.JI_InvoiceQuantity = 1m;
			invoieceLine.JI_InvoiceUQ = "BAG";

			var cusPackingList = (CusPackingList)dec.LoadOrCreateCusPackingList(Factory);
			cusPackingList.CUL_PackageDescription = "(=123CTN+123BOX)";
			var docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("PackTypeSummary", "(=123CTN\r\n+123BOX)\r\nvvvvvvvv", docCusPackingList.PackTypeSummary);

			cusPackingList.CUL_PackageDescription = ZString.Empty;
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertNullOrEmpty("PackTypeSummary", docCusPackingList.PackTypeSummary);

			var packageJob = cusPackingList.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_MarksAndNumbers = "A1";
			package1.KP_PackageQty = 1;
			package1.KP_F3_NKPackType = "BOX";

			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("PackTypeSummary", "1 BOX\r\nvvvvvvvv", docCusPackingList.PackTypeSummary);

			var package2 = packageJob.Packages.AddNew();
			package2.KP_MarksAndNumbers = "*";
			package2.KP_PackageQty = 1;
			package2.KP_F3_NKPackType = "PLT";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("PackTypeSummary", "1 PLT\r\n(=1BOX)\r\nvvvvvvvv", docCusPackingList.PackTypeSummary);

			package2.KP_MarksAndNumbers = "A2";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("PackTypeSummary", "2 PKG\r\n(=1BOX\r\n+1PLT)\r\nvvvvvvvv", docCusPackingList.PackTypeSummary);

			package2.KP_F3_NKPackType = "BOX";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("PackTypeSummary", "2 BOX\r\nvvvvvvvv", docCusPackingList.PackTypeSummary);

			cusPackingList.CUL_PackageDescription = "(=123 CTN+123 BOX)";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("PackTypeSummary", "2 BOX\r\n(=123CTN\r\n+123BOX)\r\nvvvvvvvv", docCusPackingList.PackTypeSummary);

			var package3 = packageJob.Packages.AddNew();
			package3.KP_MarksAndNumbers = "*";
			package3.KP_PackageQty = 1;
			package3.KP_F3_NKPackType = "PLT";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("PackTypeSummary", "1 PLT\r\n(=123CTN\r\n+123BOX)\r\nvvvvvvvv", docCusPackingList.PackTypeSummary);

			cusPackingList.CUL_PackageDescription = ZString.Empty;
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("PackTypeSummary", "1 PLT\r\n(=2BOX)\r\nvvvvvvvv", docCusPackingList.PackTypeSummary);

			var package4 = packageJob.Packages.AddNew();
			package4.KP_MarksAndNumbers = "*";
			package4.KP_PackageQty = 1;
			package4.KP_F3_NKPackType = "CTN";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("PackTypeSummary", "2 PKG\r\n(=1PLT\r\n+1CTN)\r\nvvvvvvvv", docCusPackingList.PackTypeSummary);

			package4.KP_F3_NKPackType = "PLT";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("PackTypeSummary", "2 PLT\r\n(=2BOX)\r\nvvvvvvvv", docCusPackingList.PackTypeSummary);
		}

		public void TestPackages()
		{
			var packageJob = packingListInternal.PackageJob;
			var package = packageJob.Packages.AddNew();
			var docPkgPackageCollection = CusPackingListWrapperInternal.Packages;
			AssertEquals("PackagesCount", 1, docPkgPackageCollection.Count);
			AssertEquals(true, docPkgPackageCollection.Cast<DocPkgPackage>().Any(c => c.WrappedObject == package));

			package = packageJob.Packages.AddNew();
			docPkgPackageCollection = CusPackingListWrapperInternal.Packages;
			AssertEquals("PackagesCount", 2, docPkgPackageCollection.Count);
			AssertEquals(true, docPkgPackageCollection.Cast<DocPkgPackage>().Any(c => c.WrappedObject == package));
		}

		public void TestQuantitySummary()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine1.JI_Description = "line1";
			invoieceLine1.JI_InvoiceQuantity = 1m;
			invoieceLine1.JI_InvoiceUQ = "BAG";
			var invoieceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine2.JI_Description = "line2";
			invoieceLine2.JI_InvoiceQuantity = 2m;
			invoieceLine2.JI_InvoiceUQ = "BBG";
			var invoieceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine3.JI_Description = "line3";
			invoieceLine3.JI_InvoiceQuantity = 3m;
			invoieceLine3.JI_InvoiceUQ = "BAG";
			var invoieceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine4.JI_Description = "line4";
			invoieceLine4.JI_InvoiceQuantity = 4m;
			invoieceLine4.JI_InvoiceUQ = "BBG";

			var cusPackingList = (CusPackingList)dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.PackableItemRelataions.RebuildElements();

			var item = cusPackingList.PackableItems.First();
			package.CustomsPackItem(item, item.CUI_PackableQty);
			var docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("1 BAG\r\nvvvvvvvvvv", docCusPackingList.QuantitySummary);

			item = cusPackingList.PackableItems.ElementAt(2);
			package.CustomsPackItem(item, item.CUI_PackableQty);
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("4 BAG\r\nvvvvvvvvvv", docCusPackingList.QuantitySummary);

			cusPackingList.PackableItems.Where(x => x.CUI_PackableUQ == "BBG").ForEach(x => package.CustomsPackItem(x, x.CUI_PackableQty));
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("4 BAG\n6 BBG\r\nvvvvvvvvvv", docCusPackingList.QuantitySummary);

			package.KP_MarksAndNumbers = "*";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertNullOrEmpty(docCusPackingList.QuantitySummary);
		}

		public void TestQuantitySummaryWithDifferenceDecimalplace()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine1.JI_Description = "line1";
			invoieceLine1.JI_InvoiceQuantity = 1m;
			invoieceLine1.JI_InvoiceUQ = "BAG";
			var invoieceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine2.JI_Description = "line2";
			invoieceLine2.JI_InvoiceQuantity = 2m;
			invoieceLine2.JI_InvoiceUQ = "BBG";

			var cusPackingList = (CusPackingList)dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.PackableItemRelataions.RebuildElements();

			var item = cusPackingList.PackableItems.First();
			package.CustomsPackItem(item, item.CUI_PackableQty);
			var docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("1 BAG\r\nvvvvvvvvvv", docCusPackingList.QuantitySummary);

			item = cusPackingList.PackableItems.ElementAt(1);
			package.CustomsPackItem(item, 0.3);
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("1.0 BAG\n0.3 BBG\r\nvvvvvvvvvv", docCusPackingList.QuantitySummary);

			package.CustomsPackItem(item, 0.33);
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("1.00 BAG\n0.63 BBG\r\nvvvvvvvvvv", docCusPackingList.QuantitySummary);

			package.CustomsPackItem(item, 0.333);
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals("1.000 BAG\n0.963 BBG\r\nvvvvvvvvvv", docCusPackingList.QuantitySummary);
		}

		public void TestMaxDecimalPlace()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine1.JI_Description = "line1";
			invoieceLine1.JI_InvoiceQuantity = 1m;
			invoieceLine1.JI_InvoiceUQ = "BAG";
			var invoieceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine2.JI_Description = "line2";
			invoieceLine2.JI_InvoiceQuantity = 2m;
			invoieceLine2.JI_InvoiceUQ = "BBG";

			var cusPackingList = (CusPackingList)dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.NetWeight = 1;
			package.KP_Weight = 2;
			package.KP_TareWeight = 1;
			package.KP_Volume = 4;
			package.PackableItemRelataions.RebuildElements();

			var item = cusPackingList.PackableItems.First();
			package.CustomsPackItem(item, item.CUI_PackableQty);
			var docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals(0, docCusPackingList.MaxPackedQtyDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxNetWeightDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxGrossWeightDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxVolumeDecimalPlace);

			item = cusPackingList.PackableItems.ElementAt(1);
			package.CustomsPackItem(item, 0.3);
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals(1, docCusPackingList.MaxPackedQtyDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxNetWeightDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxGrossWeightDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxVolumeDecimalPlace);

			package.KP_PackageQty = 2;
			package.KP_Weight = 2;
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals(2, docCusPackingList.MaxPackedQtyDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxNetWeightDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxGrossWeightDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxVolumeDecimalPlace);

			package.KP_PackageQty = 4;
			package.KP_Weight = 2;
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals(3, docCusPackingList.MaxPackedQtyDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxNetWeightDecimalPlace);
			AssertEquals(1, docCusPackingList.MaxGrossWeightDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxVolumeDecimalPlace);

			package.KP_PackageQty = 7;
			package.KP_Weight = 2;
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals(3, docCusPackingList.MaxPackedQtyDecimalPlace);
			AssertEquals(0, docCusPackingList.MaxNetWeightDecimalPlace);
			AssertEquals(3, docCusPackingList.MaxGrossWeightDecimalPlace);
			AssertEquals(3, docCusPackingList.MaxVolumeDecimalPlace);

			var packableItemRelataion1 = package.PackableItemRelataions.Cast<Customs.Business.CusPackageCusPackableItemRelation>().ElementAt(0);
			packableItemRelataion1.NetWeight = 7;
			packableItemRelataion1.NetWeightUQ = "KG";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals(0, docCusPackingList.MaxNetWeightDecimalPlace);

			var packableItemRelataion2 = package.PackableItemRelataions.Cast<Customs.Business.CusPackageCusPackableItemRelation>().ElementAt(0);
			packableItemRelataion2.NetWeight = 3.5;
			packableItemRelataion2.NetWeightUQ = "KG";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);
			AssertEquals(1, docCusPackingList.MaxNetWeightDecimalPlace);
		}

		public void TestPackedItemListSections()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine1.JI_Description = "line1";
			invoieceLine1.JI_InvoiceQuantity = 1m;
			invoieceLine1.JI_InvoiceUQ = "BAG";
			var invoieceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine2.JI_Description = "line2";
			invoieceLine2.JI_InvoiceQuantity = 2m;
			invoieceLine2.JI_InvoiceUQ = "BBG";
			var invoieceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine3.JI_Description = "line3";
			invoieceLine3.JI_InvoiceQuantity = 3m;
			invoieceLine3.JI_InvoiceUQ = "PKG";

			var cusPackingList = (CusPackingList)dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_Sequence = 1;
			package1.KP_PackageQty = 3;
			package1.KP_GoodsDescription = "testSummary1";
			package1.KP_MarksAndNumbers = "testPackNoInfo1";
			package1.NetWeight = 2m;
			package1.KP_Weight = 3m;
			package1.KP_TareWeight = 1m;
			package1.KP_WeightUQ = "BA";
			package1.KP_Length = 1.10M;
			package1.KP_Width = 1.2M;
			package1.KP_Height = 1.0M;
			package1.KP_DimensionUQ = "CM";
			package1.KP_Volume = 10.001;
			package1.KP_VolumeUQ = "L";
			package1.PackableItemRelataions.RebuildElements();

			var package2 = packageJob.Packages.AddNew();
			package2.KP_Sequence = 2;
			package2.KP_PackageQty = 5;
			package2.KP_GoodsDescription = "testSummary2";
			package2.KP_MarksAndNumbers = "testPackNoInfo2";
			package2.NetWeight = 4m;
			package2.KP_Weight = 4m;
			package2.KP_WeightUQ = "PK";
			package2.KP_Length = 1M;
			package2.KP_Width = 2M;
			package2.KP_Height = 3M;
			package2.KP_DimensionUQ = "CM";
			package2.KP_Volume = 11;
			package2.KP_VolumeUQ = "L";
			package2.PackableItemRelataions.RebuildElements();

			var package3 = packageJob.Packages.AddNew();
			package3.KP_Sequence = 2;
			package3.KP_PackageQty = 5;
			package3.KP_GoodsDescription = "testSummary2";
			package3.KP_MarksAndNumbers = "testPackNoInfo2";
			package3.NetWeight = 4m;
			package3.KP_Weight = 4m;
			package3.KP_WeightUQ = "PK";
			package3.KP_Length = 1M;
			package3.KP_Width = 2M;
			package3.KP_Height = 3M;
			package3.KP_DimensionUQ = "CM";
			package3.KP_Volume = 11;
			package3.KP_VolumeUQ = "L";
			package3.PackableItemRelataions.RebuildElements();

			var item1 = cusPackingList.PackableItems[0];
			var item2 = cusPackingList.PackableItems[1];
			var item3 = cusPackingList.PackableItems[2];
			package1.CustomsPackItem(item1, item1.CUI_PackableQty);
			package1.CustomsPackItem(item2, item2.CUI_PackableQty);
			package1.CustomsPackItem(item3, item3.CUI_PackableQty);
			var packableItemRelataions1 = package1.PackableItemRelataions[0];
			packableItemRelataions1.GoodsDescription = "testGoodsDescription1";
			var packableItemRelataions2 = package1.PackableItemRelataions[1];
			packableItemRelataions2.GoodsDescription = "testGoodsDescription2";
			var packableItemRelataions3 = package1.PackableItemRelataions[2];
			packableItemRelataions3.GoodsDescription = "testGoodsDescription3";
			var docCusPackingList = CreatePackingListWrapper(cusPackingList);

			CombineAssertions(() =>
			{
				var sections = docCusPackingList.PackedItemListSections;
				AssertEquals(5, sections.Count);
				AssertEquals("testSummary1", sections[0].Summary);
				AssertEquals("testPackNoInfo1", sections[0].PackNo);
				AssertEquals("testGoodsDescription1", sections[0].GoodsDescription);
				AssertEquals(@"@0.333 BAG
1.000 BAG", sections[0].PackedQtyInfo);
				AssertNullOrEmpty(sections[0].NetWeightInfo);
				AssertEquals(@"@1.0 BA
3.0 BA", sections[0].GrossWeightInfo);
				AssertEquals(@"@3.334 L
10.001 L
1.1*1.2*1 CM³", sections[0].VolumeInfo);
				AssertEquals(1, sections[0].PackageSequence);

				AssertNullOrEmpty(sections[1].Summary);
				AssertNullOrEmpty(sections[1].PackNo);
				AssertEquals("testGoodsDescription2", sections[1].GoodsDescription);
				AssertEquals(@"@0.667 BBG
2.000 BBG", sections[1].PackedQtyInfo);
				AssertNullOrEmpty(sections[1].NetWeightInfo);
				AssertNullOrEmpty(sections[1].GrossWeightInfo);
				AssertNullOrEmpty(sections[1].VolumeInfo);
				AssertEquals(0, sections[1].PackageSequence);

				AssertNullOrEmpty(sections[2].Summary);
				AssertNullOrEmpty(sections[2].PackNo);
				AssertEquals("testGoodsDescription3", sections[2].GoodsDescription);
				AssertEquals(@"@1.000 PKG
3.000 PKG", sections[2].PackedQtyInfo);
				AssertNullOrEmpty(sections[2].NetWeightInfo);
				AssertNullOrEmpty(sections[2].GrossWeightInfo);
				AssertNullOrEmpty(sections[2].VolumeInfo);
				AssertEquals(0, sections[2].PackageSequence);

				AssertEquals("testSummary2", sections[3].Summary);
				AssertEquals("testPackNoInfo2", sections[3].PackNo);
				AssertNullOrEmpty(sections[3].GoodsDescription);
				AssertNullOrEmpty(sections[3].PackedQtyInfo);
				AssertEquals(@"@0.8 PK
4.0 PK", sections[3].NetWeightInfo);
				AssertEquals(@"@0.8 PK
4.0 PK", sections[3].GrossWeightInfo);
				AssertEquals(@"@2.200 L
11.000 L
1*2*3 CM³", sections[3].VolumeInfo);
				AssertEquals(2, sections[3].PackageSequence);
			});

			package1.KP_MarksAndNumbers = "*";
			package2.KP_MarksAndNumbers = "*";
			package3.KP_MarksAndNumbers = "*";
			docCusPackingList = CreatePackingListWrapper(cusPackingList);

			CombineAssertions(() =>
			{
				var sections = docCusPackingList.PackedItemListSections;
				AssertEquals(5, sections.Count);
				AssertEquals("testSummary1", sections[0].Summary);
				AssertNullOrEmpty(sections[0].PackNo);
				AssertEquals("testGoodsDescription1", sections[0].GoodsDescription);
				AssertNullOrEmpty(sections[0].PackedQtyInfo);
				AssertNullOrEmpty(sections[0].NetWeightInfo);
				AssertEquals(@"@1.0 BA
3.0 BA", sections[0].GrossWeightInfo);
				AssertNullOrEmpty(sections[0].VolumeInfo);
				AssertEquals(1, sections[0].PackageSequence);

				AssertNullOrEmpty(sections[1].Summary);
				AssertNullOrEmpty(sections[1].PackNo);
				AssertEquals("testGoodsDescription2", sections[1].GoodsDescription);
				AssertNullOrEmpty(sections[1].PackedQtyInfo);
				AssertNullOrEmpty(sections[1].NetWeightInfo);
				AssertNullOrEmpty(sections[1].GrossWeightInfo);
				AssertNullOrEmpty(sections[1].VolumeInfo);
				AssertEquals(0, sections[1].PackageSequence);

				AssertNullOrEmpty(sections[2].Summary);
				AssertNullOrEmpty(sections[2].PackNo);
				AssertEquals("testGoodsDescription3", sections[2].GoodsDescription);
				AssertNullOrEmpty(sections[2].PackedQtyInfo);
				AssertNullOrEmpty(sections[2].NetWeightInfo);
				AssertNullOrEmpty(sections[2].GrossWeightInfo);
				AssertNullOrEmpty(sections[2].VolumeInfo);
				AssertEquals(0, sections[2].PackageSequence);

				AssertEquals("testSummary2", sections[3].Summary);
				AssertNullOrEmpty(sections[3].PackNo);
				AssertNullOrEmpty(sections[3].GoodsDescription);
				AssertNullOrEmpty(sections[3].PackedQtyInfo);
				AssertNullOrEmpty(sections[3].NetWeightInfo);
				AssertEquals(@"@0.8 PK
4.0 PK", sections[3].GrossWeightInfo);
				AssertNullOrEmpty(sections[3].VolumeInfo);
				AssertEquals(2, sections[3].PackageSequence);
			});
		}

		#region Implementation

		protected virtual DocCusPackingList CreatePackingListWrapper(CusPackingList packingList)
		{
			return new DocCusPackingList(packingList);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new DocCusPackingList(packingListInternal);
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
			packingListInternal = (CusPackingList)jobdeclaration.CreateCusPackingList(Factory);
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

		protected DocCusPackingList CusPackingListWrapperInternal => CreatePackingListWrapper(packingListInternal);
		protected CusPackingList packingListInternal;

		#endregion
	}
}
