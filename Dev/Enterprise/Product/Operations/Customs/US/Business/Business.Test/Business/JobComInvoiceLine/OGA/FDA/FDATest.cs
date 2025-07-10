using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDA))]
	public class FDATest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<FDA>
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<FDA>();
			originalBO.AffirmationCodes.AddNew();

			var newBO = (FDA)originalBO.Clone();

			AssertEquals(1, newBO.AffirmationCodes.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (FDA)originalBO.Clone(new BusinessObjectCloneArgs(fac, Array.Empty<string>(), typeof(FDA), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.AffirmationCodes[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.AffirmationCodes[0].Factory.GetHashCode());
		}

		public void TestSetFDADefaultValueForBaseQty()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invliceLine = invoice.InvoiceLines.AddNew();
			invliceLine.JI_InvoiceQuantity = 1000m;
			invliceLine.JI_InvoiceUQ = "KG";
			invliceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			Factory.Save();

			var fDAOnInvoiceLine = invliceLine.FDAs.AddNew();
			AssertEquals("KG", fDAOnInvoiceLine.US_FDAMeasure1);
			AssertEquals(1000m, fDAOnInvoiceLine.US_FDAQty1);
		}

		public void TestSetDefaultValue()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 100;
			invoiceLine.JI_InvoiceUQ = AESUnitOfMeasureList.Codes.Case;
			var fda1 = invoiceLine.FDAs.AddNew();
			fda1.US_FDAMeasure1 = FDAUQList.Codes.CS;
			AssertEquals(fda1.US_FDAQty1, invoiceLine.JI_InvoiceQuantity);
		}

		public void TestUS_OA_FDAFEI_ZAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "ADDRESS 2";
			address2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "001123233", Core.Constants.CountryCodes.UnitedStates);
			var fda = Factory.New<FDA>();
			fda.US_OA_FDAFEI_ZAddress.OrgPK = org.PK;
			AssertEquals("US_OA_FDAFEI_ZAddress.AddressFK", address2.PK, fda.US_OA_FDAFEI_ZAddress.AddressFK);
			AssertEquals("US_OA_FDAFEI", address2.PK, fda.US_OA_FDAFEI);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			ICusCodeDataTypeSupporter supporter = FDA;
			supporter.AssertType(typeof(AffirmationCode), CusCodeDataTypeList.Codes.AffirmationCode);
			supporter.AssertType(null, "ZZ!");

			var affirmationCode = FDA.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = "A";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<Customs.Business.CusCodeData>(affirmationCode.PK);
			AssertEquals(typeof(AffirmationCode), codeData.GetType());
		}

		public void TestShowManufacturerIDInAddressList()
		{
			FDA fda = Factory.New<FDA>();
			AddressListOverriderTest.AssertZAddressShowManufacturerIDInAddressList(this, fda.US_FDAManufacturerAddress_ZAddress);
		}

		public void TestIPriorNoticeLineMembers()
		{
			InvoiceHeader.JZ_OH_Buyer = Factory.New<OrgHeader>().PK;
			InvoiceLine.JI_OA_ConsigneeAddress = Factory.New<OrgHeader>().MainAddress.PK;
			InvoiceLine.JI_Tariff = "1010101010";
			AssertEquals("1010101010", BTALine.HarmonizedTariffNumber);
			AssertEquals(invoiceHeader.Importer, BTALine.Importer);
			AssertEquals(InvoiceLine.ConsigneeOrgAddress, BTALine.Consignee);
		}

		public void TestHumanReadableNameCore()
		{
			AssertEquals("FDA", FDA.HumanReadableName);
		}

		public void TestFDAAdmissibilityReviewDONOTSUBMIT()
		{
			AssertEquals(false, FDA.FDAAdmissibilityReviewDONOTSUBMIT);
			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewDONOTSUBMITTariff;
			AssertEquals(true, FDA.FDAAdmissibilityReviewDONOTSUBMIT);
		}

		public void TestFDAAdmissibilityReviewMayBeRequired()
		{
			AssertEquals(false, FDA.FDAAdmissibilityReviewMayBeRequired);
			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			AssertEquals(true, FDA.FDAAdmissibilityReviewMayBeRequired);
		}

		public void TestFDAAdmissibilityReviewRequired()
		{
			AssertEquals(false, FDA.FDAAdmissibilityReviewRequired);
			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			AssertEquals(true, FDA.FDAAdmissibilityReviewRequired);
		}

		public void TestFDAPriorNoticeAndAdmissibilityReviewMayBeRequired()
		{
			AssertEquals(false, FDA.FDAPriorNoticeAndAdmissibilityReviewMayBeRequired);
			InvoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeMayBeRequiredTariff;
			AssertEquals(true, FDA.FDAPriorNoticeAndAdmissibilityReviewMayBeRequired);
		}

		public void TestFDAPriorNoticeAndAdmissibilityReviewRequired()
		{
			AssertEquals(false, FDA.FDAPriorNoticeAndAdmissibilityReviewRequired);
			InvoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			AssertEquals(true, FDA.FDAPriorNoticeAndAdmissibilityReviewRequired);
		}

		public void TestClone()
		{
			FDA.US_PNC = "TEST";
			FDA.US_FDAConfirmDate = ZDateTime.Now;
			FDA.US_UC_NKFDAProduction = Core.Constants.CountryCodes.FaeroeIslands;

			FDA newFDA = (FDA)FDA.Clone();
			AssertEquals("", newFDA.US_PNC);
			AssertEquals(ZDateTime.Empty, newFDA.US_FDAConfirmDate);
			AssertEquals(Core.Constants.CountryCodes.FaeroeIslands, newFDA.US_UC_NKFDAProduction);
		}

		public void TestIBTALine()
		{
			FDA.US_FDACommercialDesc = "Dried bone";
			AssertEquals("Dried bone", BTALine.CommercialDescription);

			FDA.US_FDALineNo = 2;
			AssertEquals(2, BTALine.FDALineNumber);

			FDA.US_FDAValue = 9987.5m;
			AssertEquals(9988m, BTALine.ValueInWholeDollars);

			FDA.US_FDAProductCode = "123456";
			AssertEquals("123456", BTALine.FDAProductCode);

			FDA.US_FDACargoStorageCode = CargoStorageCodeList.Codes.AmbientTemperature;
			AssertEquals(CargoStorageCodeList.Codes.AmbientTemperature, BTALine.CargoStorageStatus);

			FDA.US_UC_NKFDAProduction = "MX";
			AssertEquals("MX", BTALine.CountryOfProduction);

			OrgHeader manufacturer = Factory.New<OrgHeader>();
			OrgCusCode cusCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "123456789");
			InvoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("123456789", BTALine.ManufacturerNumber);

			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgAddress address = supplier.Addresses.AddNew();
			cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "987654321");
			InvoiceHeader.JZ_OA_SupplierAddress = address.PK;
			AssertEquals("987654321", BTALine.SupplierOrShipperNumber);

			OrgHeader importer = Factory.New<OrgHeader>();
			cusCode = importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "987654321");
			Declaration.JE_OH_Importer = importer.PK;

			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "8073hjksde");
			FDA.US_OA_FDAFEI = ultimateConsignee.MainAddress.PK;
			AssertEquals("FEI number", "008073hjksde", BTALine.ConsigneeFEI);

			FDA.US_TradeBrandName = "Life is good";
			AssertEquals("Brand Name", "Life is good", BTALine.TradeOrBrandName);

			InvoiceLine.Declaration.US_FDAContactName = "John";
			InvoiceLine.Declaration.US_FDAContactPhoneNo = "0288888888";
			InvoiceLine.Declaration.US_FDAContactEmail = "john@company.org";

			AssertEquals("John", BTALine.ContactName);
			AssertEquals("0288888888", BTALine.ContactPhone);
			AssertEquals("john@company.org", BTALine.ContactEmail);

			InvoiceLine.InvoiceHeader.US_FDAContactName = "Chuck";
			InvoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "3189800200";
			InvoiceLine.Declaration.US_FDAContactEmail = "Chuck@company.org";

			AssertEquals("Chuck", BTALine.ContactName);
			AssertEquals("3189800200", BTALine.ContactPhone);
			AssertEquals("Chuck@company.org", BTALine.ContactEmail);

			FDA.US_ContainerDim1 = 10m;
			FDA.US_ContainerDim2 = 20m;
			FDA.US_ContainerDim3 = 30m;
			AssertEquals("First dimension", 10m, BTALine.FirstDimension);
			AssertEquals("Second dimension", 20m, BTALine.SecondDimension);
			AssertEquals("Third dimension", 30m, BTALine.ThirdDimension);

			FDA.US_FDAQty1 = 1.3243424m;
			FDA.US_FDAMeasure1 = "1";

			FDA.US_FDAQty2 = 2.7887m;
			FDA.US_FDAMeasure2 = "2";

			FDA.US_FDAQty3 = 3.68754654m;
			FDA.US_FDAMeasure3 = "3";

			FDA.US_FDAQty4 = 4.112333m;
			FDA.US_FDAMeasure4 = "4";

			FDA.US_FDAQty5 = 5.87878m;
			FDA.US_FDAMeasure5 = "5";

			FDA.US_FDAQty6 = 6.78878m;
			FDA.US_FDAMeasure6 = "6";

			AssertFDAQty(BTALine.OrderedQtyUQs[5], 1.32m, "1");
			AssertFDAQty(BTALine.OrderedQtyUQs[4], 2.79m, "2");
			AssertFDAQty(BTALine.OrderedQtyUQs[3], 3.69m, "3");
			AssertFDAQty(BTALine.OrderedQtyUQs[2], 4.11m, "4");
			AssertFDAQty(BTALine.OrderedQtyUQs[1], 5.88m, "5");
			AssertFDAQty(BTALine.OrderedQtyUQs[0], 6.79m, "6");

			FDA.AffirmationCodes.RemoveAll();

			FDA.AffirmationCodes.AddNew("AAA", "AA");
			FDA.AffirmationCodes.AddNew("CCC", "C");

			var affirmationCodes = BTALine.AffirmationCodes;
			AssertEquals("2 elements", 2, affirmationCodes.Count);
			AssertEquals("AAA", affirmationCodes[0].CY_Code);
			AssertEquals("AA", affirmationCodes[0].CY_Data);
			AssertEquals("CCC", affirmationCodes[1].CY_Code);
			AssertEquals("C", affirmationCodes[1].CY_Data);
		}

		void AssertFDAQty(FDAQtyUQPair fdaQtyUQPair, ZDecimal qtyExpected, ZString uQExpected)
		{
			AssertEquals(qtyExpected, fdaQtyUQPair.Qty);
			AssertEquals(uQExpected, fdaQtyUQPair.UQ);
		}

		public void TestFDAQtyRunningTotal()
		{
			FDA.US_FDAQty1 = 1.114446m;
			FDA.US_FDAMeasure1 = "BX";
			FDA.US_FDAQty2 = 2.87987m;
			FDA.US_FDAQty3 = 3.121231m;
			FDA.US_FDAQty4 = 4.446454m;
			FDA.US_FDAQty5 = 5.8754512m;
			FDA.US_FDAQty6 = 6.1112m;
			AssertEquals("FDAQtyRunningTotal", "Total 1594.58 BX", FDA.FDAQtyRunningTotal);

			FDA.US_FDAQty3 = 9.75;
			AssertEquals("FDAQtyRunningTotal", "Total 4983.08 BX", FDA.FDAQtyRunningTotal);

			FDA.US_FDAQty6 = 0;
			AssertEquals("FDAQtyRunningTotal", "Total 815.56 BX", FDA.FDAQtyRunningTotal);

			FDA.US_FDAQty1 = 1.378;
			FDA.US_FDAMeasure1 = "CT";
			FDA.US_FDAQty2 = 144;
			FDA.US_FDAQty3 = 0;
			FDA.US_FDAQty4 = 0;
			FDA.US_FDAQty5 = 0;
			FDA.US_FDAQty6 = 0;
			AssertEquals("FDAQtyRunningTotal", "Total 198.72 CT", FDA.FDAQtyRunningTotal);

			FDA.US_FDAQty1 = 24;
			FDA.US_FDAMeasure1 = "PR";
			FDA.US_FDAQty2 = 144;
			FDA.US_FDAMeasure2 = "BX";
			AssertEquals("FDAQtyRunningTotal", "Total 3456.00 PR", FDA.FDAQtyRunningTotal);

			FDA.US_FDAQty1 = 5000;
			FDA.US_FDAMeasure1 = "BX";
			FDA.US_FDAQty2 = 15000;
			FDA.US_FDAQty3 = 8000;
			FDA.US_FDAQty4 = 10000;
			FDA.US_FDAQty5 = 30000;
			FDA.US_FDAQty6 = 7000;
			AssertEquals("FDAQtyRunningTotal", "Large Qty - please check", FDA.FDAQtyRunningTotal);
		}

		public void TestUS_FDAShipperAddress()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1234";
			supplier1.OH_FullName = "SUPPLIER 1";

			var invoicer1 = Factory.New<OrgHeader>();
			invoicer1.OH_Code = "INVOICER1234";
			invoicer1.OH_FullName = "INVOICER 1";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			FDA.US_FDACommercialDesc = "Test";
			InvoiceHeader.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("FDAShipper should fall back to supplier", supplier1.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("Likewise on the FDA line the FDAShipper should fall back to invoice supplier", supplier1.MainAddress.PK, FDA.US_FDAShipperAddress);

			declaration.JE_OA_InvoicerAddress = invoicer1.MainAddress.PK;
			AssertEquals("FDAShipper should default to invoicer org in preference", invoicer1.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("Likewise on the FDA line should default to invoicer org in preference", invoicer1.MainAddress.PK, FDA.US_FDAShipperAddress);
		}

		public void TestUS_FDAShipperAddressIsEffective()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1";
			supplier1.OH_FullName = "SUPPLIER 1";

			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_Code = "SUPPLIER2";
			supplier2.OH_FullName = "SUPPLIER 2";

			var supplier3 = Factory.New<OrgHeader>();
			supplier3.OH_Code = "SUPPLIER3";
			supplier3.OH_FullName = "SUPPLIER 3";

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			InvoiceHeader.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("FDAShipper should be supplier 1 effectively", supplier1.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("FDAShipper on FDA line should be as above", supplier1.MainAddress.PK, FDA.US_FDAShipperAddress);

			Declaration.JE_OA_InvoicerAddress = supplier2.MainAddress.PK;
			AssertEquals("FDAShipper should now be supplier 2 effectively", supplier2.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("FDAShipper on FDA line should be as above", supplier2.MainAddress.PK, FDA.US_FDAShipperAddress);

			InvoiceHeader.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("FDAShipper should remain as Invoicer (supplier 2) effectively", supplier2.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("FDAShipper on FDA line should be as above", supplier2.MainAddress.PK, FDA.US_FDAShipperAddress);

			InvoiceHeader.JZ_OA_FDAShipperAddress = supplier3.MainAddress.PK;
			AssertEquals("FDAShipper should now be supplier 3 actually", supplier3.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("FDAShipper on FDA line should be as above", supplier3.MainAddress.PK, FDA.US_FDAShipperAddress);

			Declaration.JE_OA_InvoicerAddress = supplier3.MainAddress.PK;
			AssertEquals("FDAShipper should now be supplier 3 effectively as value is same as entered in FDAShipper field", supplier3.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("FDAShipper on FDA line should be as above", supplier3.MainAddress.PK, FDA.US_FDAShipperAddress);

			invoiceHeader.JZ_OA_FDAShipperAddress = ZGuid.Empty;
			Declaration.JE_OA_InvoicerAddress = ZGuid.Empty;
			AssertEquals("FDAShipper should revert to Supplier (supplier 1) effectively again", supplier1.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("FDAShipper on FDA line should be as above", supplier1.MainAddress.PK, FDA.US_FDAShipperAddress);

			FDA.US_FDAShipperAddress = supplier3.MainAddress.PK;
			AssertEquals("FDAShipper on invoice s/b effective", supplier1.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("FDAShipper on FDA line should have different value", supplier3.MainAddress.PK, FDA.US_FDAShipperAddress);

			InvoiceHeader.JZ_OA_FDAShipperAddress = supplier3.MainAddress.PK;
			AssertEquals("FDAShipper on invoice s/b value entered", supplier3.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("FDAShipper on FDA line should still be supplier 3", supplier3.MainAddress.PK, FDA.US_FDAShipperAddress);

			Declaration.JE_OA_InvoicerAddress = supplier3.MainAddress.PK;
			AssertEquals("FDAShipper on invoice should now be supplier 3 effectively as value is same as entered in FDAShipper field", supplier3.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("FDAShipper on FDA line should still be as above", supplier3.MainAddress.PK, FDA.US_FDAShipperAddress);

			FDA.US_FDAShipperAddress = ZGuid.Empty;
			invoiceHeader.JZ_OA_FDAShipperAddress = ZGuid.Empty;
			Declaration.JE_OA_InvoicerAddress = supplier1.MainAddress.PK;
			AssertEquals("FDAShipper on invoice should be effective", supplier1.MainAddress.PK, InvoiceHeader.JZ_OA_FDAShipperAddress);
			AssertEquals("FDAShipper on FDA line should be effective", supplier1.MainAddress.PK, FDA.US_FDAShipperAddress);
		}

		public void TestUS_FDAManufacturerAddress()
		{
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			OrgCusCode cusCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "123456789");
			InvoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			FDA.US_FDAManufacturerAddress = ZGuid.Empty;
			AssertEquals(InvoiceLine.JI_OA_ManufacturerAddress, FDA.US_FDAManufacturerAddress);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1" + new Random().Next(1000000).ToString();
			org1.OH_FullName = "DUMMY ORG FOR TESTING";
			FDA.US_FDAManufacturerAddress = org1.MainAddress.PK;
			AssertEquals(org1.MainAddress.PK, FDA.US_FDAManufacturerAddress);

			OrgHeader manufacturer2 = Factory.New<OrgHeader>();
			FDA.US_FDAManufacturerAddress = manufacturer2.MainAddress.PK;
			AssertEquals(manufacturer2.PK, FDA.US_FDAManufacturerAddress_ZAddress.OrgPK);
		}

		public void TestInvalidGUIDStoredInAddInfoDoesNotRaiseFormatException()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.FDAs.AddNew();
			fda.B7_AddInfoData = "FDAManufacturerAddress=THISISNOTAGUID";
			AssertEquals(ZGuid.Invalid, fda.US_FDAManufacturerAddress);
		}

		public void TestManufacturerAddress()
		{
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			FDA.US_FDAManufacturerAddress = ZGuid.Empty;
			AssertEquals(null, FDA.ManufacturerAddress);

			FDA.US_FDAManufacturerAddress = manufacturer.MainAddress.PK;
			AssertNotNull(FDA.ManufacturerAddress);
		}

		public void TestSetManufacturerDefaults()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.FillWithValidTestData();
			manufacturer.OH_RL_NKClosestPort = "ADALV";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber, "12345678912", Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedStates));

			FDA.US_FDAManufacturerAddress = manufacturer.MainAddress.PK;

			var wrapper = OrgHeaderWrapper.New(manufacturer);
			wrapper.ZO_ProducerFirmType = ProducerFirmTypeList.Codes.G;
			wrapper.ZO_MFRRegExempt = FDAPriorNoticeExemptCodeList.Codes.L;

			Factory.Save();
			FDA.SetManufacturerDefaults();

			AssertEquals("AD", FDA.US_UC_NKFDAProduction);
			AssertEquals("12345678912", FDA.US_PFR);
			AssertEquals(ProducerFirmTypeList.Codes.G, FDA.US_PFT);
			AssertEquals(FDAPriorNoticeExemptCodeList.Codes.L, FDA.US_FME);
		}

		public void TestIFDALine()
		{
			FDA.US_FDACommercialDesc = "Dried bone";
			AssertEquals("Dried bone", FDALine.CommercialDescription);
			FDA.US_FDALineNo = 2;
			AssertEquals(2, FDALine.FDALineNumber);
			FDA.US_FDAValue = 9987.5m;
			AssertEquals(9988m, FDALine.ValueInWholeDollars);
			FDA.US_FDAProductCode = "123456";
			AssertEquals("123456", FDALine.FDAProductCode);
			FDA.US_FDACargoStorageCode = CargoStorageCodeList.Codes.AmbientTemperature;
			AssertEquals(CargoStorageCodeList.Codes.AmbientTemperature, FDALine.CargoStorageStatus);
			FDA.US_UC_NKFDAProduction = "MX";
			AssertEquals("MX", FDALine.CountryOfProduction);

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "123456789");
			InvoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("123456789", FDALine.ManufacturerNumber);

			var fdaShipper = Factory.NewWithValidTestData<OrgHeader>();
			var fdaShipperaddress = fdaShipper.Addresses.AddNew();
			fdaShipperaddress.OA_Address1 = "test address";
			cusCode = fdaShipperaddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU938457590X");
			InvoiceHeader.JZ_OA_FDAShipperAddress = fdaShipperaddress.PK;
			Factory.Save();
			AssertEquals("Supplier or Shipper number falling back to FDA Shipper on Invoice", "AU938457590X", FDALine.SupplierOrShipperNumber);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			cusCode = importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "987654321");
			Declaration.JE_OH_Importer = importer.PK;

			var ultimateConsignee = Factory.NewWithValidTestData<OrgHeader>();
			ultimateConsignee.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "8073hjksde");
			FDA.US_OA_FDAFEI = ultimateConsignee.MainAddress.PK;
			AssertEquals("FEI number", "008073hjksde", FDALine.ConsigneeFEI);

			FDA.US_TradeBrandName = "Life is good";
			AssertEquals("Brand Name", "Life is good", FDALine.TradeOrBrandName);

			InvoiceLine.Declaration.US_FDAContactName = "John";
			InvoiceLine.Declaration.US_FDAContactPhoneNo = "0288888888";
			InvoiceLine.Declaration.US_FDAContactEmail = "john@company.org";

			AssertEquals("John", FDALine.ContactName);
			AssertEquals("0288888888", FDALine.ContactPhone);
			AssertEquals("john@company.org", FDALine.ContactEmail);

			InvoiceLine.InvoiceHeader.US_FDAContactName = "Chuck";
			InvoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "3189800200";
			InvoiceLine.Declaration.US_FDAContactEmail = "Chuck@company.org";

			AssertEquals("Chuck", FDALine.ContactName);
			AssertEquals("3189800200", FDALine.ContactPhone);
			AssertEquals("Chuck@company.org", FDALine.ContactEmail);

			FDA.US_ContainerDim1 = 10m;
			FDA.US_ContainerDim2 = 20m;
			FDA.US_ContainerDim3 = 30m;
			AssertEquals("First dimension", 10m, FDALine.FirstDimension);
			AssertEquals("Second dimension", 20m, FDALine.SecondDimension);
			AssertEquals("Third dimension", 30m, FDALine.ThirdDimension);

			FDA.US_FDAQty1 = 1;
			FDA.US_FDAMeasure1 = "1";

			FDA.US_FDAQty2 = 2;
			FDA.US_FDAMeasure2 = "2";

			FDA.US_FDAQty3 = 3;
			FDA.US_FDAMeasure3 = "3";

			FDA.US_FDAQty4 = 4;
			FDA.US_FDAMeasure4 = "4";

			FDA.US_FDAQty5 = 5;
			FDA.US_FDAMeasure5 = "5";

			FDA.US_FDAQty6 = 6;
			FDA.US_FDAMeasure6 = "6";

			for (int i = 0; i < 6; i++)
			{
				FDAQtyUQPair fdaQtyUQPair = FDALine.OrderedQtyUQs[i];
				AssertEquals(((ZDecimal)6 - i), fdaQtyUQPair.Qty);
				AssertEquals((6 - i).ToString(), fdaQtyUQPair.UQ);
			}

			FDA.AffirmationCodes.RemoveAll();

			FDA.AffirmationCodes.AddNew("AAA", "AA");
			FDA.AffirmationCodes.AddNew("CCC", "C");

			var affirmationCodes = FDALine.AffirmationCodes;
			AssertEquals("2 elements", 2, affirmationCodes.Count);
			AssertEquals("AAA", affirmationCodes[0].CY_Code);
			AssertEquals("AA", affirmationCodes[0].CY_Data);
			AssertEquals("CCC", affirmationCodes[1].CY_Code);
			AssertEquals("C", affirmationCodes[1].CY_Data);
		}

		public void TestSupplierOrShipperNumber1()
		{
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = supplier.Addresses.AddNew();
			address.OA_Address1 = "test address";
			OrgCusCode cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "987654321");
			InvoiceHeader.JZ_OA_SupplierAddress = address.PK;
			AssertEquals("Supplier or Shipper number last fall back to Invoice Supplier", "987654321", FDALine.SupplierOrShipperNumber);
		}

		public void TestSupplierOrShipperNumber2()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var address = supplier.Addresses.AddNew();
			address.OA_Address1 = "test address";
			var cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "987654321");
			InvoiceHeader.JZ_OA_SupplierAddress = address.PK;

			var fdaShipper = Factory.NewWithValidTestData<OrgHeader>();
			var fdaShipperaddress = fdaShipper.Addresses.AddNew();
			fdaShipperaddress.OA_Address1 = "test address";
			cusCode = fdaShipperaddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU938457590X");
			InvoiceHeader.JZ_OA_FDAShipperAddress = fdaShipperaddress.PK;
			AssertEquals("Supplier or Shipper number fall back to FDA Shipper on Invoice", "AU938457590X", FDALine.SupplierOrShipperNumber);
		}

		public void TestSupplierOrShipperNumber3()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var address = supplier.Addresses.AddNew();
			address.OA_Address1 = "test address";
			var cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "987654321");
			InvoiceHeader.JZ_OA_SupplierAddress = address.PK;

			var fdaShipper = Factory.NewWithValidTestData<OrgHeader>();
			var fdaShipperaddress = fdaShipper.Addresses.AddNew();
			fdaShipperaddress.OA_Address1 = "test address";
			cusCode = fdaShipperaddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU938457590X");
			InvoiceHeader.JZ_OA_FDAShipperAddress = fdaShipperaddress.PK;

			var fdaShipperOnLine = Factory.NewWithValidTestData<OrgHeader>();
			var fdaShipperOnLineaddress = fdaShipperOnLine.Addresses.AddNew();
			fdaShipperOnLineaddress.OA_Address1 = "test address";
			cusCode = fdaShipperOnLineaddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "NZ038948573T");
			InvoiceHeader.JZ_OA_FDAShipperAddress = fdaShipperOnLineaddress.PK;
			AssertEquals("Supplier or Shipper number from FDA Shipper on FDA Line", "NZ038948573T", FDALine.SupplierOrShipperNumber);
		}

		public void TestFDAValues()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				Declaration.US_EnableSPN = true;

				InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				Assert(FDA.US_FDAValueInfo.ReadOnly);

				InvoiceLine.JI_LinePrice = 1000m;
				FDA.US_InvCurrFDAValue = 1000m;
				AssertEquals("ApportionmentDirty", "...", FDA.FDAValue);
				Declaration.ResumeApportionment();
				AssertEquals("Invoice Currency FDA Value should be same as FDAValue when in USD", FDA.US_FDAValue, FDA.US_InvCurrFDAValue);
				AssertEquals("ApportionmentDirty", "1000", FDA.FDAValue);

				InvoiceLine.JI_LinePrice = 1500m;
				FDA.US_InvCurrFDAValue = 1500m;
				Declaration.ResumeApportionment();
				AssertEquals("Entering Invoice Currency FDA Value should default FDAValue", 1500m, FDA.US_FDAValue);

				InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
				InvoiceHeader.JZ_InvoiceCurrExRate = 0.9287m;
				InvoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = true;
				InvoiceLine.JI_LinePrice = 1000m;
				FDA.US_InvCurrFDAValue = 1000m;
				Declaration.ResumeApportionment();
				AssertEquals("Invoice Currency FDA Value should be calculated from FDAValue", 929m, FDA.US_FDAValue);

				InvoiceLine.JI_LinePrice = 1500m;
				FDA.US_InvCurrFDAValue = 1500m;
				Declaration.ResumeApportionment();
				AssertEquals("Entering Invoice Currency FDA Value should calculate USD FDAValue", 1393m, FDA.US_FDAValue);
			}
		}

		public void TestContainerNumbers()
		{
			CreateContainersAndRailCars();
			AssertNotNull(BTALine.ContainerNumbers);

			foreach (ZString containerNo in BTALine.ContainerNumbers)
			{
				AssertEquals("CRUX432890", containerNo);
			}
		}

		public void TestRailCarNumbers()
		{
			CreateContainersAndRailCars();
			AssertNotNull(BTALine.RailCarNumbers);

			foreach (ZString railCarNo in BTALine.RailCarNumbers)
			{
				AssertEquals("RR-30945", railCarNo);
			}
		}

		public void TestInvCurrencyCode()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "";
			AssertEquals("InvCurrencyCode", "", FDA.InvCurrencyCode);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.France;
			AssertEquals("InvCurrencyCode", "EUR", FDA.InvCurrencyCode);
		}

		public void TestValidateBillsForFDALines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_HouseBill = "HB10020023";
			declaration.JE_MasterBill = "12599675660";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillNum = "HB2";
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillNum = "MB10050";
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.FDAs.AddNew();
			var fdaBillsAvailable = fda.BillsAvailable;
			AssertEquals("Bills available", 4, fdaBillsAvailable.Count);

			fdaBillsAvailable[1].IsForFDALine = true;
			fdaBillsAvailable[2].IsForFDALine = true;
			fdaBillsAvailable[3].IsForFDALine = true;
			AssertNoMessageError("No Notifications expected, because Stand Alone Prior Notice not selected", fdaBillsAvailable[0].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertNoMessageError("No Notifications expected, because Stand Alone Prior Notice not selected", fdaBillsAvailable[1].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertNoMessageError("No Notifications expected, because Stand Alone Prior Notice not selected", fdaBillsAvailable[2].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertNoMessageError("No Notifications expected, because Stand Alone Prior Notice not selected", fdaBillsAvailable[3].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);

			declaration.US_EnableSPN = true;
			fdaBillsAvailable = fda.BillsAvailable;
			fdaBillsAvailable[0].ValidateIsForFDALine();
			fdaBillsAvailable[1].ValidateIsForFDALine();
			fdaBillsAvailable[2].ValidateIsForFDALine();
			fdaBillsAvailable[3].ValidateIsForFDALine();

			AssertNoMessageError(fdaBillsAvailable[0].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertHasMessageError(fdaBillsAvailable[1].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertHasMessageError(fdaBillsAvailable[2].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertHasMessageError(fdaBillsAvailable[3].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);

			fdaBillsAvailable[2].IsForFDALine = false;
			fdaBillsAvailable[0].ValidateIsForFDALine();
			fdaBillsAvailable[1].ValidateIsForFDALine();
			fdaBillsAvailable[2].ValidateIsForFDALine();
			fdaBillsAvailable[3].ValidateIsForFDALine();
			AssertNoMessageError(fdaBillsAvailable[0].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertHasMessageError(fdaBillsAvailable[1].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertNoMessageError(fdaBillsAvailable[2].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertHasMessageError(fdaBillsAvailable[3].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);

			fdaBillsAvailable[1].IsForFDALine = false;
			fdaBillsAvailable[0].ValidateIsForFDALine();
			fdaBillsAvailable[1].ValidateIsForFDALine();
			fdaBillsAvailable[2].ValidateIsForFDALine();
			fdaBillsAvailable[3].ValidateIsForFDALine();
			AssertNoMessageError(fdaBillsAvailable[0].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertNoMessageError(fdaBillsAvailable[1].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertNoMessageError(fdaBillsAvailable[2].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
			AssertNoMessageError(fdaBillsAvailable[3].IsForFDALineInfo, FDARelatedBillsGenPivotValidation.OnlyOneBillCanBeSelectedForPriorNotice);
		}

		public void TestCopyRelatedBillsAndContainerInInvoiceLineWhenCopyLineEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.JE_ContainerMode = ContainerModeList.Codes.Containerized;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MasterBill = "12599675660";

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillNum = "12599675660";
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "AAAB4321524";
			container1.CO_Weight = 1.0m;
			container1.CO_WeightUQ = "KG";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "AAAB4321530";
			container2.CO_Weight = 2.0m;
			container2.CO_WeightUQ = "KG";

			var invoice = declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;
			collection.CopyLastLineDetailsToNewLines = true;
			var invoiceLine = collection.AddNew();

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = false;

			var fda = invoiceLine.FDAs.AddNew();

			invoiceLine.FDAs[0].BillsAvailable[0].IsForFDALine = true;

			var affCode1 = fda.AffirmationCodes.AddNew();
			affCode1.CY_Code = "ACC";
			affCode1.CY_Data = "2";

			var affCode2 = fda.AffirmationCodes.AddNew();
			affCode2.CY_Code = "SDT";
			affCode2.CY_Data = "4";

			var invoiceLineCopy = collection.AddNew();

			AssertEquals(1, invoiceLine.FDAs.Count);
			AssertEquals(1, invoiceLineCopy.FDAs.Count);

			AssertEquals("Affirmation Code 1 should be copied from previous line", affCode1.CY_Code, invoiceLineCopy.FDAs[0].AffirmationCodes[0].CY_Code);
			AssertEquals("Affirmation value 1 should be copied from previous line", affCode1.CY_Data, invoiceLineCopy.FDAs[0].AffirmationCodes[0].CY_Data);
			AssertEquals("Affirmation Code 2 should be copied from previous line", affCode2.CY_Code, invoiceLineCopy.FDAs[0].AffirmationCodes[1].CY_Code);
			AssertEquals("Affirmation value 1 should be copied from previous line", affCode2.CY_Data, invoiceLineCopy.FDAs[0].AffirmationCodes[1].CY_Data);

			var containerForInvoiceLine = invoiceLine.FDAs[0].ContainersForInvoiceLine[0];
			AssertEquals(container1.CO_ContainerNumber, containerForInvoiceLine.ContainerNumber);
			Assert(containerForInvoiceLine.IsForFDALine);
			AssertEquals(container1.CO_ContainerNumber, invoiceLine.FDAs[0].ContainersForFDALine[0].Container.CO_ContainerNumber);

			var billAvailable = invoiceLine.FDAs[0].BillsAvailable[0];
			AssertContains(masterBill.CU_BillNum, billAvailable.BillNumber);
			Assert(billAvailable.IsForFDALine);
			AssertEquals(masterBill.CU_BillNum, invoiceLine.FDAs[0].BillsForFDALine[0].Relation2Object.CU_BillNum);

			AssertContains("Bill number should be copied from previous line", masterBill.CU_BillNum, invoiceLineCopy.FDAs[0].BillsAvailable[0].BillNumber);
			Assert("Bill is related should be copied from previous line", invoiceLineCopy.FDAs[0].BillsAvailable[0].IsForFDALine);
			AssertEquals("Bill number should be copied from previous line", masterBill.CU_BillNum, invoiceLineCopy.FDAs[0].BillsForFDALine[0].Relation2Object.CU_BillNum);

			containerForInvoiceLine = invoiceLineCopy.FDAs[0].ContainersForInvoiceLine[0];
			AssertEquals("Container number should be copied from previous line", container1.CO_ContainerNumber, containerForInvoiceLine.ContainerNumber);
			Assert("Container is related should be copied from previous line", containerForInvoiceLine.IsForFDALine);
			AssertEquals("Container number should be copied from previous line", container1.CO_ContainerNumber, invoiceLineCopy.FDAs[0].ContainersForFDALine[0].Container.CO_ContainerNumber);
		}

		public void TestCopyRelatedContainerInInvoiceLineWhenCopyLineEnabledEnsureNoDuplicateContainerPivots()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = ContainerModeList.Codes.Containerized;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "PIXU2357849";
			container1.CO_WeightUQ = "KG";

			var invoice = declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;
			collection.CopyLastLineDetailsToNewLines = true;
			var invoiceLine = collection.AddNew();

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			var fda = invoiceLine.FDAs.AddNew();

			var invoiceLineCopy = collection.AddNew();

			AssertEquals(1, invoiceLine.FDAs.Count);
			AssertEquals(1, invoiceLineCopy.FDAs.Count);

			var containerForInvoiceLine = invoiceLine.FDAs[0].ContainersForInvoiceLine[0];
			AssertEquals(container1.CO_ContainerNumber, containerForInvoiceLine.ContainerNumber);
			Assert(containerForInvoiceLine.IsForFDALine);
			AssertEquals(container1.CO_ContainerNumber, invoiceLine.FDAs[0].ContainersForFDALine[0].Container.CO_ContainerNumber);

			containerForInvoiceLine = invoiceLineCopy.FDAs[0].ContainersForInvoiceLine[0];
			AssertEquals("Container number should be copied from previous line", container1.CO_ContainerNumber, containerForInvoiceLine.ContainerNumber);
			Assert("Container is related should be copied from previous line", containerForInvoiceLine.IsForFDALine);
			AssertEquals("Container number should be copied from previous line", container1.CO_ContainerNumber, invoiceLineCopy.FDAs[0].ContainersForFDALine[0].Container.CO_ContainerNumber);

			AssertNotEquals("Pivot to invoice line should be unique", invoiceLine.ContainersPivot[0].C2_JI, invoiceLineCopy.ContainersPivot[0].C2_JI);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
		}

		[ExpectNoExceptions]
		public void TestValidateBillsForFDALinesForDeletedFDA()
		{
			var invoiceLine = InvoiceLine;

			declaration.JE_HouseBill = "HB10020023";
			declaration.JE_MasterBill = "12599675660";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillNum = "HB2";
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillNum = "MB10050";
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			var fda = InvoiceLine.FDAs.AddNew();
			var fdaBillsAvailable = fda.BillsAvailable;
			AssertEquals("Bills available", 4, fdaBillsAvailable.Count);
			fdaBillsAvailable[2].IsForFDALine = true;
			fdaBillsAvailable[3].IsForFDALine = true;
			fda.Delete();
		}

		public void TestRelatedBillForFDALineAfterCopying()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB001";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1806900100";
			var fda = invoiceLine.FDAs.AddNew();
			fda.BillsAvailable[0].IsForFDALine = true;
			Factory.Save();

			var clonedDec = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("FDA line copied", 1, clonedDec.InvoiceLines[0].FDAs.Count);

			var bill = Factory.New<Bill>();
			clonedDec.Bills.Add(bill);
			bill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB002Test";

			var fdaBillsAvailable = clonedDec.InvoiceLines[0].FDAs[0].BillsAvailable;
			AssertEquals(1, fdaBillsAvailable.Count);
			var billAvailable = fdaBillsAvailable[0];
			Assert(!billAvailable.IsForFDALine);
		}

		public void TestRemovePivots()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB001";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1806900100";
			var fda = invoiceLine.FDAs.AddNew();
			fda.BillsAvailable[0].IsForFDALine = true;
			Factory.Save();

			var query = new ZQuery(GenPivotSchema.XX_Relation1ID, fda.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, declaration.Bills[0].PK);
			AssertNotNull(Factory.LoadTop1<GenPivot>(query));

			invoiceHeader.JZ_JE = ZGuid.Empty;
			Factory.Save();
			AssertNull(Factory.LoadTop1<GenPivot>(query));
		}

		[ExpectNoExceptions]
		public void TestLoadDifferentObjectsFromSameRow()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB001";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1806900100";
			var fda = invoiceLine.FDAs.AddNew();
			fda.BillsAvailable[0].IsForFDALine = true;
			Factory.Save();

			var query = new ZQuery(GenPivotSchema.XX_Relation1ID, fda.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, declaration.Bills[0].PK);
			Factory.LoadTop1<GenPivot>(query);
			Factory.LoadTop1<FDARelatedBillsGenPivot>(query);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return FDA;
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			return dec.Invoices.AddNew().InvoiceLines.AddNew().FDAs.AddNew();
		}

		void CreateContainersAndRailCars()
		{
			FDARelatedContainer relatedContainer = SetRelatedContainer("CRUX432890", "US1", "20");
			FDA.ContainersForInvoiceLine.Add(relatedContainer);
			relatedContainer.IsForFDALine = true;
			AssertEquals(2, FDA.ContainersForFDALine.Count);

			FDARelatedContainer relatedRailCar = SetRelatedContainer("RR-30945", "US2", USContainerCodeList.Codes.RR);
			FDA.ContainersForInvoiceLine.Add(relatedRailCar);
			relatedRailCar.IsForFDALine = true;
			AssertEquals(3, FDA.ContainersForFDALine.Count);
		}

		FDARelatedContainer SetRelatedContainer(string containerNumber, string containerCode, string uSContainerCode)
		{
			CusContainer container = Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;

			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = containerCode;
			refContainer.SetCountrySpecificContainerCode(uSContainerCode, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container.CO_RC = refContainer.PK;

			CusContainerInvoiceLinePivot containerInvoiceLinePivot = Factory.New<CusContainerInvoiceLinePivot>();
			containerInvoiceLinePivot.C2_CO = container.PK;

			FDARelatedContainer relatedContainer = new FDARelatedContainer(FDA);
			relatedContainer.SetContainerInvoiceLinePivot(containerInvoiceLinePivot);

			return relatedContainer;
		}

		FDA FDA
		{
			get { return fda ?? (fda = InvoiceLine.FDAs.AddNew()); }
		}
		FDA fda;

		IPriorNoticeLine BTALine
		{
			get { return FDA; }
		}

		IFDALine FDALine
		{
			get { return FDA; }
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		JobComInvoiceHeader InvoiceHeader
		{
			get { return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoiceHeader;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}

				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
