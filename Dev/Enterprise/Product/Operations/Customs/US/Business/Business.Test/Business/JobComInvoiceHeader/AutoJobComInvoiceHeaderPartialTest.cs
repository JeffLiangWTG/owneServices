using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AutoJobComInvoiceHeaderPartialTest : TestCaseWithFactory
	{
		public void TestDefaultCountryOfOriginFromManufacturer()
		{
			USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			((IRegistryItemInternals)USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport).DeleteValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			OrgHeader org = Factory.New<OrgHeader>();
			org.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			AssertNotEquals(Core.Constants.CountryCodes.NewZealand, invoice.US_UC_NKCountryOfOrigin);
			invoice.JZ_OA_ManufacturerAddress = org.MainAddress.PK;
			AssertEquals(Core.Constants.CountryCodes.NewZealand, invoice.US_UC_NKCountryOfOrigin);
			USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			invoice.US_UC_NKCountryOfOrigin = ZString.Empty;
			invoice.JZ_OA_ManufacturerAddress = org.MainAddress.PK;
			AssertNotEquals(Core.Constants.CountryCodes.NewZealand, invoice.US_UC_NKCountryOfOrigin);
		}

		public void TestDefaultCountryOfOriginFromSupplier()
		{
			USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			((IRegistryItemInternals)USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport).DeleteValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Singapore;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfExport = ZString.Empty;
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfExport);
			AssertNotEquals(Core.Constants.CountryCodes.NewZealand, invoice.US_UC_NKCountryOfOrigin);
			invoice.JZ_OA_SupplierAddress = org1.MainAddress.PK;
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfExport);
			AssertEquals(Core.Constants.CountryCodes.NewZealand, invoice.US_UC_NKCountryOfOrigin);
			invoice.JZ_OA_ManufacturerAddress = org1.MainAddress.PK;
			invoice.JZ_OA_SupplierAddress = org2.MainAddress.PK;
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfExport);
			AssertEquals(Core.Constants.CountryCodes.NewZealand, invoice.US_UC_NKCountryOfOrigin);
			USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			invoice.US_UC_NKCountryOfExport = ZString.Empty;
			invoice.US_UC_NKCountryOfOrigin = ZString.Empty;
			invoice.JZ_OA_SupplierAddress = org1.MainAddress.PK;
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfExport);
			AssertEquals(ZString.Empty, invoice.US_UC_NKCountryOfOrigin);
		}

		public void TestDefaultExporterAddress()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "USLAX";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_RL_NKClosestPort = "USLAX";
			var midAddress = org2.Addresses.AddNew();
			OrgCusCode cusCode = midAddress.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			cusCode.OK_CustomsRegNo = "XA12345678";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Exporter = org1.PK;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals(org1.MainAddress.PK, invoice.JZ_OA_ExporterAddress);
			AssertEquals(org1.PK, invoice.JZ_OA_ExporterAddress_ZAddress.OrgPK);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_OH_Exporter = org2.PK;
			AssertEquals(midAddress.PK, invoice.JZ_OA_ExporterAddress);
			AssertNotEquals(org2.MainAddress.PK, invoice.JZ_OA_ExporterAddress);
			AssertEquals(org2.PK, invoice.JZ_OA_ExporterAddress_ZAddress.OrgPK);
		}

		public void TestShowManufacturerIDInAddressList()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			AddressListOverriderTest.AssertZAddressShowManufacturerIDInAddressList(this, invoice.JZ_OA_InvoicerDocAddress_ZAddress);
			AddressListOverriderTest.AssertZAddressShowManufacturerIDInAddressList(this, invoice.JZ_OA_FDAShipperAddress_ZAddress);
			AddressListOverriderTest.AssertZAddressShowManufacturerIDInAddressList(this, invoice.JZ_OA_ManufacturerAddress_ZAddress);
			AddressListOverriderTest.AssertZAddressShowManufacturerIDInAddressList(this, invoice.JZ_OA_SupplierAddress_ZAddress);
		}

		public void TestLoadCurrencyFromSupplierDetailsAndImporterDefaults()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			supplier.OH_FullName = "TEST SUPPLIER";
			supplier.MiscServ.OM_RX_NKEXDefCurrency = "GBP";
			supplier.OH_RL_NKClosestPort = "USCHI";
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMP";
			importer.OH_FullName = "TEST IMPORTER";
			importer.OH_RL_NKClosestPort = "CNSHA";
			var link = supplier.SupplierLinks.AddNew();
			link.OL_OH_Supplier = supplier.PK;
			link.OL_OH_Buyer = importer.PK;
			link.OL_RX_NKDefaultCurrency = "CNY";
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Currency defaults from shipper details.", "GBP", invoice.JZ_RX_NKInvoice_Currency);
			invoice.JZ_OH_Buyer = importer.PK;
			AssertEquals("Currency defaults from shipper>Consignee/Buyer/Importer Defaults.", "CNY", invoice.JZ_RX_NKInvoice_Currency);
			var declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Currency defaults from shipper details.", "GBP", invoice.JZ_RX_NKInvoice_Currency);
			invoice.JZ_OH_Buyer = importer.PK;
			AssertEquals("Currency defaults from shipper>Consignee/Buyer/Importer Defaults.", "CNY", invoice.JZ_RX_NKInvoice_Currency);
		}
	}
}
