using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEFDA))]
	public class ACEFDATest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ACEFDA>
	{
		public void TestGetterOfUS_OA_ShipperAddress_EnableDocAddressForFDAIsFalse()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			var fda = invoiceLine.ACE_FDALines.AddNew();

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				invoice.JZ_OA_SupplierAddress = org.MainAddress.PK;
				AssertEquals(org.PK, fda.ShipperOrgPK);
				AssertEquals(org.MainAddress.PK, fda.US_OA_ShipperAddress);
				Factory.Save();
				Assert(!fda.B7_AddInfoData.Contains("OA_ShipperAddress"));
			}
		}

		public void TestGetterOfUS_DeliverToPartyAddress_EnableDocAddressForFDAIsFalse()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			var fda = invoiceLine.ACE_FDALines.AddNew();

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				invoiceLine.JI_OA_ShipToPartyAddress = org.MainAddress.PK;
				AssertEquals(org.PK, fda.DeliverToPartyOrgPK);
				AssertEquals(org.MainAddress.PK, fda.US_DeliverToPartyAddress);
				Factory.Save();
				Assert(!fda.B7_AddInfoData.Contains("DeliverToPartyAddress"));
			}
		}

		public void TestUS_OA_ShipperAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			var fda = invoiceLine.ACE_FDALines.AddNew();

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				_ = fda.US_OA_ShipperAddress;
				AssertNull(fda.ShipperDocAddress);
				fda.US_OA_ShipperAddress = org.MainAddress.PK;
				AssertEquals(org.MainAddress.PK, fda.ShipperDocAddress.E2_OA_Address);
				AssertEquals(org.PK, fda.ShipperOrgPK);
				fda.ShipperOrgPK = ZGuid.Empty;
				AssertNull(fda.ShipperDocAddress);
			}

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				fda.US_OA_ShipperAddress = org.MainAddress.PK;
				AssertEquals(org.PK, fda.ShipperOrgPK);
				Factory.Save();
				Assert(fda.B7_AddInfoData.Contains("OA_ShipperAddress"));
			}
		}

		public void TestDefaultAddressesWhenFDAcopiedFromProduct()
		{
			var orgShipper = Factory.NewWithValidTestData<OrgHeader>();
			orgShipper.OH_Code = "##SHP";
			var orgDelivery = Factory.NewWithValidTestData<OrgHeader>();
			orgDelivery.OH_Code = "@@DLV";
			var orgImp = Factory.NewWithValidTestData<OrgHeader>();
			orgImp.OH_Code = "%%IMP";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "MAXOB";
			product.OP_StockKeepingUnit = "CS";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = orgImp.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "4421906000";
			pivot.CD_ACEFDAIndicator = "D";

			var fdaLineOnProduct = pivot.ACEFDAs.AddNew();
			fdaLineOnProduct.US_Description = "MAX";
			fdaLineOnProduct.US_IntendedUseCode = "015.00";
			AssertEquals(Guid.Empty, fdaLineOnProduct.US_OA_ShipperAddress_ZAddress.OrgPK);
			AssertEquals(Guid.Empty, fdaLineOnProduct.US_DeliverToPartyAddress_ZAddress.OrgPK);

			Factory.Save();

			var conversion = product.PartUnits.AddNew();
			conversion.OF_PackType = "L";
			conversion.OF_QuantityInParent = 8m;
			conversion.OF_ParentPackType = "CS";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = orgImp.PK;
			declaration.JE_OH_Supplier = orgShipper.PK;
			declaration.ShipperOrgPK = orgShipper.PK;
			declaration.ShipToPartyOrgPK = orgDelivery.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_InvCurrValue = 30m;
			AssertEquals(orgShipper.PK, fda.ShipperOrgPK);
			AssertEquals(orgDelivery.PK, fda.DeliverToPartyOrgPK);
			AssertEquals("", fda.US_Description);

			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.ACE_FDALines[0].Refresh();
			AssertNotNull(invoiceLine.Part);
			AssertNotNull(invoiceLine.Part.PivotsForBinding);
			AssertEquals("loaded by Product", "015.00", invoiceLine.ACE_FDALines[0].US_IntendedUseCode);
			AssertEquals(orgShipper.PK, invoiceLine.ACE_FDALines[0].ShipperOrgPK);
			AssertEquals(orgDelivery.PK, invoiceLine.ACE_FDALines[0].DeliverToPartyOrgPK);
		}

		public void TestDefaultAddressesWhenFDAcopiedFromProduct_OwnerOrgPK()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgShipper = Factory.NewWithValidTestData<OrgHeader>();
				orgShipper.OH_Code = "##SHP";
				var orgDelivery = Factory.NewWithValidTestData<OrgHeader>();
				orgDelivery.OH_Code = "@@DLV";
				var orgImp = Factory.NewWithValidTestData<OrgHeader>();
				orgImp.OH_Code = "%%IMP";

				var product = Factory.New<OrgSupplierPart>();
				product.OP_PartNum = "MAXOB";
				product.OP_StockKeepingUnit = "CS";

				var relOrg = product.RelatedOrganisations.AddNew();
				relOrg.OU_OH = orgImp.PK;
				relOrg.OU_Relationship = "OWN";

				var importTariff = Factory.New<USCTariff>();
				importTariff.UE_Tariff = "0000000000";
				importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
				importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
				importTariff.UE_PGACodes = "FD3";

				var pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_TariffNum = importTariff.UE_Tariff;
				pivot.CD_ACEFDAIndicator = "D";

				var fdaLineOnProduct = pivot.ACEFDAs.AddNew();
				fdaLineOnProduct.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				fdaLineOnProduct.US_Description = "MAX";
				fdaLineOnProduct.US_IntendedUseCode = "015.00";
				AssertEquals(Guid.Empty, fdaLineOnProduct.US_ProducerAddress_ZAddress.OrgPK);

				Factory.Save();

				var conversion = product.PartUnits.AddNew();
				conversion.OF_PackType = "L";
				conversion.OF_QuantityInParent = 8m;
				conversion.OF_ParentPackType = "CS";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OA_DeclarantAddress = orgImp.MainAddress.PK;
				declaration.JE_OH_Importer = orgImp.PK;
				declaration.JE_OH_Supplier = orgShipper.PK;
				declaration.ShipperOrgPK = orgShipper.PK;
				declaration.ShipToPartyOrgPK = orgDelivery.PK;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				invoiceLine.JI_Tariff = importTariff.UE_Tariff;
				invoiceLine.US_FDAIndicator = "D";
				var fda = invoiceLine.ACE_FDALines.AddNew();
				fda.US_InvCurrValue = 30m;
				AssertEquals(Guid.Empty, fda.OwnerOrgPK);
				AssertEquals("", fda.US_Description);

				invoiceLine.JI_PartNo = product.OP_PartNum;
				invoiceLine.ACE_FDALines[0].Refresh();
				AssertNotNull(invoiceLine.Part);
				AssertNotNull(invoiceLine.Part.PivotsForBinding);
				AssertEquals(orgImp.PK, invoiceLine.ACE_FDALines[0].OwnerOrgPK);
			}
		}

		public void TestDefaultAddressesWhenFDAcopiedFromProduct_ProducerOrgPK()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgShipper = Factory.NewWithValidTestData<OrgHeader>();
				orgShipper.OH_Code = "##SHP";
				var orgDelivery = Factory.NewWithValidTestData<OrgHeader>();
				orgDelivery.OH_Code = "@@DLV";
				var orgImp = Factory.NewWithValidTestData<OrgHeader>();
				orgImp.OH_Code = "%%IMP";

				var product = Factory.New<OrgSupplierPart>();
				product.OP_PartNum = "MAXOB";
				product.OP_StockKeepingUnit = "CS";

				var relOrg = product.RelatedOrganisations.AddNew();
				relOrg.OU_OH = orgImp.PK;
				relOrg.OU_Relationship = "OWN";

				var pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_TariffNum = "4421906000";
				pivot.CD_ACEFDAIndicator = "D";

				var fdaLineOnProduct = pivot.ACEFDAs.AddNew();
				fdaLineOnProduct.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
				fdaLineOnProduct.US_Description = "MAX";
				fdaLineOnProduct.US_IntendedUseCode = "015.00";
				AssertEquals(Guid.Empty, fdaLineOnProduct.US_ProducerAddress_ZAddress.OrgPK);

				Factory.Save();

				var conversion = product.PartUnits.AddNew();
				conversion.OF_PackType = "L";
				conversion.OF_QuantityInParent = 8m;
				conversion.OF_ParentPackType = "CS";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OA_DeclarantAddress = orgImp.MainAddress.PK;
				declaration.JE_OH_Importer = orgImp.PK;
				declaration.JE_OH_Supplier = orgShipper.PK;
				declaration.ShipperOrgPK = orgShipper.PK;
				declaration.ShipToPartyOrgPK = orgDelivery.PK;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.US_FDAIndicator = "D";
				var fda = invoiceLine.ACE_FDALines.AddNew();
				fda.US_InvCurrValue = 30m;

				AssertEquals(Guid.Empty, fda.ProducerOrgPK);
				AssertEquals("", fda.US_Description);

				invoiceLine.JI_PartNo = product.OP_PartNum;

				invoiceLine.ACE_FDALines[0].Refresh();
				AssertNotNull(invoiceLine.Part);
				AssertNotNull(invoiceLine.Part.PivotsForBinding);
				AssertEquals(orgImp.PK, invoiceLine.ACE_FDALines[0].ProducerOrgPK);
			}
		}

		public void TestUS_DeliverToPartyAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			var fda = invoiceLine.ACE_FDALines.AddNew();

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				_ = fda.US_DeliverToPartyAddress;
				AssertNull(fda.DeliverToPartyDocAddress);
				fda.US_DeliverToPartyAddress = org.MainAddress.PK;
				AssertEquals(org.MainAddress.PK, fda.DeliverToPartyDocAddress.E2_OA_Address);
				AssertEquals(org.PK, fda.DeliverToPartyOrgPK);
				fda.DeliverToPartyOrgPK = ZGuid.Empty;
				AssertNull(fda.DeliverToPartyDocAddress);
			}

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				fda.US_DeliverToPartyAddress = org.MainAddress.PK;
				AssertEquals(org.PK, fda.DeliverToPartyOrgPK);
				Factory.Save();
				Assert(fda.B7_AddInfoData.Contains("DeliverToPartyAddress"));
			}
		}

		public void TestUS_FDAImporterAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			var fda = invoiceLine.ACE_FDALines.AddNew();

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				_ = fda.US_FDAImporterAddress;
				AssertNull(fda.FDAImporterDocAddress);
				fda.US_FDAImporterAddress = org.MainAddress.PK;
				AssertEquals(org.MainAddress.PK, fda.FDAImporterDocAddress.E2_OA_Address);
				AssertEquals(org.PK, fda.FDAImporterOrgPK);
				fda.FDAImporterOrgPK = ZGuid.Empty;
				AssertNull(fda.FDAImporterDocAddress);
			}

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				fda.US_FDAImporterAddress = org.MainAddress.PK;
				AssertEquals(org.PK, fda.FDAImporterOrgPK);
				Factory.Save();
				Assert(fda.B7_AddInfoData.Contains("FDAImporter"));
			}
		}

		public void TestUS_FSVPImporterAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			var fda = invoiceLine.ACE_FDALines.AddNew();

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				_ = fda.US_FSVPImporterAddress;
				AssertNull(fda.FSVPImporterDocAddress);
				fda.US_FSVPImporterAddress = org.MainAddress.PK;
				AssertEquals(org.MainAddress.PK, fda.FSVPImporterDocAddress.E2_OA_Address);
				AssertEquals(org.PK, fda.FSVPImporterOrgPK);
				fda.FSVPImporterOrgPK = ZGuid.Empty;
				AssertNull(fda.FSVPImporterDocAddress);
			}

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				fda.US_FSVPImporterAddress = org.MainAddress.PK;
				AssertEquals(org.PK, fda.FSVPImporterOrgPK);
				Factory.Save();
				Assert(fda.B7_AddInfoData.Contains("FSVPImporter"));
			}
		}

		public void TestUS_FSVPImporterAddress_WhenImporterNotUS()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			AssertNotNull(declaration.IOR);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProductCode = "23-2878";
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			AssertEquals(ZGuid.Empty, fda.FSVPImporterOrgPK);

			var address = org.Addresses.AddNew();
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProductCode = "23-2878";
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			AssertEquals(org.PK, fda.FSVPImporterOrgPK);
		}

		public void TestDefaultDocAddresses()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipper = Factory.NewWithValidTestData<OrgHeader>();
				var shipperAddress2 = shipper.Addresses.AddNew();
				shipperAddress2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "1234567", "US");
				var deliveryToParty = Factory.NewWithValidTestData<OrgHeader>();
				var deliveryToPartyAddress2 = deliveryToParty.Addresses.AddNew();
				deliveryToPartyAddress2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "2345678", "US");
				var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
				var manufacturerAddress2 = manufacturer.Addresses.AddNew();
				manufacturerAddress2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "4567890", "US");

				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.US_FDAIndicator = "D";
				invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
				invoiceLine.JI_OA_ShipToPartyAddress = deliveryToParty.MainAddress.PK;
				invoice.JZ_OA_SupplierAddress = shipper.MainAddress.PK;

				var fda = invoiceLine.ACE_FDALines.AddNew();
				fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				var shipperDocAddress = fda.ShipperDocAddress;
				AssertNotNull(shipperDocAddress);
				AssertEquals("Default from Invoice.JZ_OA_SupplierAddress", shipper.MainAddress.PK, shipperDocAddress.E2_OA_Address);
				AssertEquals("Default from Invoice.JZ_OA_SupplierAddress", shipper.MainAddress.PK, fda.US_OA_ShipperAddress);
				var deliveryToPartyDocAddress = fda.DeliverToPartyDocAddress;
				AssertNotNull(deliveryToPartyDocAddress);
				AssertEquals("Default from InvoiceLine.JI_OA_ShipToPartyAddress", deliveryToParty.MainAddress.PK, deliveryToPartyDocAddress.E2_OA_Address);
				AssertEquals("Default from InvoiceLine.JI_OA_ShipToPartyAddress", deliveryToParty.MainAddress.PK, fda.US_DeliverToPartyAddress);
				var manufacturerDocAddress = fda.DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer);
				AssertNotNull(manufacturerDocAddress);
				AssertEquals("Default from InvoiceLine.JI_OA_ManufacturerAddress", manufacturer.MainAddress.PK, manufacturerDocAddress.E2_OA_Address);

				invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
				fda.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
				manufacturerDocAddress = fda.DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer);
				AssertNull("InvoiceLine.JI_OA_ManufacturerAddress has no value", manufacturerDocAddress);

				fda.ShipperOrgPK = ZGuid.Empty;
				fda.ShipperOrgPK = shipper.PK;
				shipperDocAddress = fda.ShipperDocAddress;
				AssertEquals("Default the address that has DUN", shipperAddress2.PK, shipperDocAddress.E2_OA_Address);
				AssertEquals("Default the address that has DUN", shipperAddress2.PK, fda.US_OA_ShipperAddress);
				fda.DeliverToPartyOrgPK = ZGuid.Empty;
				fda.DeliverToPartyOrgPK = deliveryToParty.PK;
				deliveryToPartyDocAddress = fda.DeliverToPartyDocAddress;
				AssertEquals("Default the address that has DUN", deliveryToPartyAddress2.PK, deliveryToPartyDocAddress.E2_OA_Address);
				AssertEquals("Default the address that has DUN", deliveryToPartyAddress2.PK, fda.US_DeliverToPartyAddress);
				manufacturerDocAddress = fda.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
				manufacturerDocAddress.OrganisationPK = manufacturer.PK;
				AssertEquals("Default the address that has DUN", manufacturerAddress2.PK, manufacturerDocAddress.E2_OA_Address);
			}
		}

		public void TestUS_OA_GoodsOwner()
		{
			var declaration = Factory.New<JobDeclaration>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			var fda = invoiceLine.ACE_FDALines.AddNew();

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				_ = fda.US_OwnerAddress;
				AssertNull(fda.GoodsOwnerDocAddress);

				fda.US_OwnerAddress = org.MainAddress.PK;
				AssertEquals(org.MainAddress.PK, fda.GoodsOwnerDocAddress.E2_OA_Address);
				AssertEquals(org.PK, fda.OwnerOrgPK);

				fda.OwnerOrgPK = ZGuid.Empty;
				AssertNull(fda.GoodsOwnerDocAddress);
			}

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				fda.US_OwnerAddress = org.MainAddress.PK;
				AssertEquals(org.PK, fda.OwnerOrgPK);

				Factory.Save();
				Assert(fda.B7_AddInfoData.Contains("OwnerAddress"));
			}
		}

		public void TestDefaultDocAddresses_GoodsOwner()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
				declaration.US_EnableENS = true;
				declaration.US_CertifyCargoRelease = true;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.US_FDAIndicator = "D";

				var fdaLine1 = invoiceLine.ACE_FDALines.AddNew();
				declaration.US_FDAADTA = ZDateTime.Empty;
				fdaLine1.US_FDAForcePN = true;
				fdaLine1.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				var goodsOwnerDocAddress = fdaLine1.GoodsOwnerDocAddress;
				AssertNull(goodsOwnerDocAddress);

				declaration.JE_OA_DeclarantAddress = importer.MainAddress.PK;
				var fdaLine2 = invoiceLine.ACE_FDALines.AddNew();
				fdaLine2.US_FDAForcePN = true;
				fdaLine2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				goodsOwnerDocAddress = fdaLine2.GoodsOwnerDocAddress;
				AssertNotNull(goodsOwnerDocAddress);
				AssertEquals("Default from the address of Declaration.IOR that has DUN", importer.MainAddress.PK, goodsOwnerDocAddress.E2_OA_Address);
				AssertEquals("Default from the address of Declaration.IOR that has DUN", importer.MainAddress.PK, fdaLine2.US_OwnerAddress);
			}
		}

		public void TestUS_ProducerAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			var fda = invoiceLine.ACE_FDALines.AddNew();

			TestCase(FDAProgramCodeList.Codes.DEV, () => fda.InitialImporterDocAddress);
			TestCase(FDAProgramCodeList.Codes.DRU, () => fda.SponsorDocAddress);
			TestCase(FDAProgramCodeList.Codes.TOB, () => fda.ManufacturerDocAddress);
			void TestCase(string program, Func<JobDocAddress> docAddressSupplier)
			{
				fda.US_ProgramCode = program;
				using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					_ = fda.US_ProducerAddress;
					AssertNull($"{program} EnableDocAddressForFDA, JobDocAddress default null", docAddressSupplier());

					fda.US_ProducerAddress = org.MainAddress.PK;
					AssertEquals($"{program} EnableDocAddressForFDA, JobDocAddress.E2_OA_Address from US_ProducerAddress", org.MainAddress.PK, docAddressSupplier().E2_OA_Address);
					AssertEquals($"{program} EnableDocAddressForFDA, ProducerOrgPK from US_ProducerAddress_ZAddress", org.PK, fda.ProducerOrgPK);

					fda.ProducerOrgPK = ZGuid.Empty;
					AssertNull($"{program} EnableDocAddressForFDA, JobDocAddress removed", docAddressSupplier());
				}

				using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					fda.US_ProducerAddress = org.MainAddress.PK;
					AssertEquals($"{program} DisableDocAddressForFDA, ProducerOrgPK from US_ProducerAddress_ZAddress", org.PK, fda.ProducerOrgPK);

					Factory.Save();
					Assert($"{program} DisableDocAddressForFDA, B7_AddInfoData contains ProducerAddress", fda.B7_AddInfoData.Contains("ProducerAddress"));
				}
			}

			fda.DocAddresses.RemoveAll();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				_ = fda.US_ProducerAddress;
				AssertNull($"Not ProducerAddress Relevant, Default InitialImporterDocAddress", fda.InitialImporterDocAddress);
				AssertNull($"Not ProducerAddress Relevant, Default SponsorDocAddress", fda.SponsorDocAddress);
				AssertNull($"Not ProducerAddress Relevant, Default ManufacturerDocAddress", fda.ManufacturerDocAddress);

				fda.US_ProducerAddress = org.MainAddress.PK;
				AssertNull($"Not ProducerAddress Relevant, InitialImporterDocAddress won't be populated", fda.InitialImporterDocAddress);
				AssertNull($"Not ProducerAddress Relevant, SponsorDocAddress won't be populated", fda.SponsorDocAddress);
				AssertNull($"Not ProducerAddress Relevant, ManufacturerDocAddress won't be populated", fda.ManufacturerDocAddress);
			}
		}

		public void TestDefaultDocAddresses_InitialImporter()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "ABC";
				importer.OH_FullName = "ABC INC.";
				var addressWithDUN = importer.Addresses.AddNew();
				addressWithDUN.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123", Core.Constants.CountryCodes.UnitedStates);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				var fdaLine1 = invoiceLine.ACE_FDALines.AddNew();
				fdaLine1.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
				var initialImporterDocAddress = fdaLine1.InitialImporterDocAddress;
				AssertNull(initialImporterDocAddress);

				declaration.JE_OA_DeclarantAddress = importer.MainAddress.PK;
				var fdaLine2 = invoiceLine.ACE_FDALines.AddNew();
				fdaLine2.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
				initialImporterDocAddress = fdaLine2.InitialImporterDocAddress;
				AssertNotNull(initialImporterDocAddress);
				AssertEquals("Default from the address of Declaration.IOR that has DUN", addressWithDUN.PK, initialImporterDocAddress.E2_OA_Address);
				AssertEquals("Default from the address of Declaration.IOR that has DUN", addressWithDUN.PK, fdaLine2.US_ProducerAddress);
			}
		}

		public void TestDocAddressesCountChanged_ShouldValidateProgramCode()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var shipper = orgHeader.Addresses.AddNew();
				FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ShipperRequired);

				FDA.US_OA_ShipperAddress = shipper.PK;
				AssertNoMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ShipperRequired);

				FDA.DocAddresses.RemoveDocAddressViaDocAddressType(DocAddressType.Shipper);
				AssertHasMessageError(FDA.US_ProgramCodeInfo, USACEFDAAddInfoInvoiceLineValidation.ShipperRequired);
			}
		}

		public void TestPGAContactsFromDocAddress()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var iFDADetails = (IFDAData)FDA;
				var foreignExporter = Factory.New<OrgHeader>();
				foreignExporter.MainAddress.OA_Address1 = "MAIN ADDRESS";

				var address1 = foreignExporter.Addresses.AddNew();
				address1.OA_Address1 = "ADDRESS 1";
				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Shipper);
				FDA.ShipperDocAddress.E2_OA_Address = address1.PK;
				AssertEquals("ADDRESS 1", iFDADetails.ShipperContact.CompanyAddress.AddressLine1);

				var address2 = foreignExporter.Addresses.AddNew();
				address2.OA_Address1 = "ADDRESS 2";
				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
				FDA.ManufacturerDocAddress.E2_OA_Address = address2.PK;
				AssertEquals("ADDRESS 2", iFDADetails.ManufacturerContact.CompanyAddress.AddressLine1);

				var address3 = foreignExporter.Addresses.AddNew();
				address3.OA_Address1 = "ADDRESS 3";
				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsDeliveredTo);
				FDA.DeliverToPartyDocAddress.E2_OA_Address = address3.PK;
				AssertEquals("ADDRESS 3", iFDADetails.DeliveryPartyContact.CompanyAddress.AddressLine1);

				var address4 = foreignExporter.Addresses.AddNew();
				address4.OA_Address1 = "ADDRESS 4";
				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ImporterDocumentaryAddress);
				FDA.FDAImporterDocAddress.E2_OA_Address = address4.PK;
				AssertEquals("ADDRESS 4", iFDADetails.FDAImporterContact.CompanyAddress.AddressLine1);

				var address5 = foreignExporter.Addresses.AddNew();
				address5.OA_Address1 = "ADDRESS 5";
				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.FSVPImporter);
				FDA.FSVPImporterDocAddress.E2_OA_Address = address5.PK;
				AssertEquals("ADDRESS 5", iFDADetails.FSVPImporterContact.CompanyAddress.AddressLine1);

				var address7 = foreignExporter.Addresses.AddNew();
				address7.OA_Address1 = "ADDRESS 7";
				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsOwner);
				FDA.GoodsOwnerDocAddress.E2_OA_Address = address7.PK;
				AssertEquals("ADDRESS 7", iFDADetails.OwnerContact.CompanyAddress.AddressLine1);

				var address8 = foreignExporter.Addresses.AddNew();
				address8.OA_Address1 = "ADDRESS 8";
				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsLocation);
				FDA.LocationOfGoodsDocAddress.E2_OA_Address = address8.PK;
				AssertEquals("ADDRESS 8", iFDADetails.LocationOfGoodsContact.CompanyAddress.AddressLine1);

				AssertNull("Not TOB should be null", iFDADetails.ProducerContact);
				FDA.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
				var address6 = foreignExporter.Addresses.AddNew();
				address6.OA_Address1 = "ADDRESS 6";
				FDA.ManufacturerDocAddress.E2_OA_Address = address6.PK;
				AssertEquals("ADDRESS 6", iFDADetails.ProducerContact.CompanyAddress.AddressLine1);
			}
		}

		public void TestDefaultDocAddressContact_AfterOrgHeaderChange()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var party = Factory.New<OrgHeader>();
				var mainAddress = party.MainAddress;
				mainAddress.OA_RN_NKCountryCode = "US";
				DeclarationTestHelper.AddPGAContact(party.MainAddress, "AAA", "WTG", null, "pga@test.com", null);
				DeclarationTestHelper.AddFSVPContact(party.MainAddress, "BBB", "WTG", null, "fsvp@test.com", null);

				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsOwner);
				FDA.GoodsOwnerDocAddress.OrganisationPK = party.PK;
				AssertEquals(FDA.GoodsOwnerDocAddress.Contact.Email, "pga@test.com");

				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsLocation);
				FDA.LocationOfGoodsDocAddress.OrganisationPK = party.PK;
				AssertEquals(FDA.LocationOfGoodsDocAddress.Contact.Email, "pga@test.com");

				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Shipper);
				FDA.ShipperDocAddress.OrganisationPK = party.PK;
				AssertEquals(FDA.ShipperDocAddress.Contact.Email, "pga@test.com");

				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
				FDA.ManufacturerDocAddress.OrganisationPK = party.PK;
				AssertEquals(FDA.ManufacturerDocAddress.Contact.Email, "pga@test.com");

				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsDeliveredTo);
				FDA.DeliverToPartyDocAddress.OrganisationPK = party.PK;
				AssertEquals(FDA.DeliverToPartyDocAddress.Contact.Email, "pga@test.com");

				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ImporterDocumentaryAddress);
				FDA.FDAImporterDocAddress.OrganisationPK = party.PK;
				AssertEquals(FDA.FDAImporterDocAddress.Contact.Email, "pga@test.com");

				_ = FDA.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.FSVPImporter);
				FDA.FSVPImporterDocAddress.OrganisationPK = party.PK;
				AssertEquals(FDA.FSVPImporterDocAddress.Contact.Email, "fsvp@test.com");
			}
		}

		public void TestDefaultFSVPImporter()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var fsvpImporter = Factory.NewWithValidTestData<OrgHeader>();
				var fsvpImporterAddress2 = fsvpImporter.Addresses.AddNew();
				fsvpImporterAddress2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "3456789", "US");

				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.US_FDAIndicator = "D";
				declaration.JE_OA_DeclarantAddress = fsvpImporter.MainAddress.PK;

				var fda = invoiceLine.ACE_FDALines.AddNew();
				fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
				fda.US_ProductCode = "54AAQ13";
				var affirmationCode = fda.AffirmationCodes.AddNew();
				affirmationCode.CY_Code = "HTS";
				affirmationCode.CY_Data = "01021404";
				fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;

				var fsvpImporterDocAddress = fda.FSVPImporterDocAddress;
				AssertNotNull(fsvpImporterDocAddress);
				AssertEquals("Default from the address of Declaration.IOR that has DUN", fsvpImporterAddress2.PK, fsvpImporterDocAddress.E2_OA_Address);
				AssertEquals("Default from the address of Declaration.IOR that has DUN", fsvpImporterAddress2.PK, fda.US_FSVPImporterAddress);
			}
		}

		public void TestDefaultFDAImporter()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var fdaImporter = Factory.NewWithValidTestData<OrgHeader>();
				var fdaImporterAddress2 = fdaImporter.Addresses.AddNew();
				fdaImporterAddress2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "3456789", "US");

				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.US_FDAIndicator = "D";
				declaration.JE_OA_DeclarantAddress = fdaImporter.MainAddress.PK;

				var fda = invoiceLine.ACE_FDALines.AddNew();
				fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;

				var fdaImporterDocAddress = fda.FDAImporterDocAddress;
				AssertNotNull(fdaImporterDocAddress);
				AssertEquals("Default from the address of Declaration.IOR that has DUN", fdaImporterAddress2.PK, fdaImporterDocAddress.E2_OA_Address);
				AssertEquals("Default from the address of Declaration.IOR that has DUN", fdaImporterAddress2.PK, fda.US_FDAImporterAddress);
			}
		}

		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<ACEFDA>();
			originalBO.AffirmationCodes.AddNew();
			originalBO.Lots.AddNew();
			originalBO.ProductConstituentElements.AddNew();
			originalBO.Licenses.AddNew();

			var newBO = (ACEFDA)originalBO.Clone();

			AssertEquals(1, newBO.AffirmationCodes.Count);
			AssertEquals(1, newBO.Lots.Count);
			AssertEquals(1, newBO.ProductConstituentElements.Count);
			AssertEquals(1, newBO.Licenses.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (ACEFDA)originalBO.Clone(new BusinessObjectCloneArgs(fac, Array.Empty<string>(), typeof(ACEFDA), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.AffirmationCodes[0].Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Lots[0].Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.ProductConstituentElements[0].Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Licenses[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.AffirmationCodes[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.Lots[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.ProductConstituentElements[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.Licenses[0].Factory.GetHashCode());
		}

		public void TestShouldForcePriorNotice()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryType = "11";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.CAFTABenefitsApplicable;
			Factory.Save();

			invoiceLine.US_FDAIndicator = "D";
			var fdaLine1 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine1.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fdaLine1.US_FDAForcePN = false;
			Assert(!fdaLine1.IsPriorNotice);

			fdaLine1.US_ProductCode = "71AYA01";
			AssertEquals(true, fdaLine1.ShouldForcePriorNotice);
			Assert(fdaLine1.US_FDAForcePN);

			fdaLine1.US_FDAForcePN = false;
			fdaLine1.US_ProductCode = "17AA101";
			AssertEquals(true, fdaLine1.ShouldForcePriorNotice);
			Assert(fdaLine1.US_FDAForcePN);

			fdaLine1.US_FDAForcePN = false;
			fdaLine1.US_ProductCode = "52DIY01";
			AssertEquals(true, fdaLine1.ShouldForcePriorNotice);
			Assert(fdaLine1.US_FDAForcePN);

			fdaLine1.US_FDAForcePN = false;
			fdaLine1.US_ProductCode = "54AAQ13";
			AssertEquals(true, fdaLine1.ShouldForcePriorNotice);
			Assert(fdaLine1.US_FDAForcePN);
		}

		public void TestDefaultFDADate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.JE_DateOfArrival = ZDate.BrettsBirthday;
			declaration.US_EntryDate = ZDate.BrettsBirthday.AddDays(10);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			Factory.Save();

			invoiceLine.US_FDAIndicator = "D";
			var fdaLine1 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine1.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			declaration.US_FDAADTA = ZDateTime.Empty;
			fdaLine1.US_FDAForcePN = true;
			Assert(fdaLine1.IsPriorNotice);
			AssertEquals(ZDate.BrettsBirthday, declaration.US_FDAADTA.Date);

			declaration.US_FDAADTA = ZDateTime.Empty;
			fdaLine1.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			AssertEquals(ZDate.BrettsBirthday.AddDays(10), declaration.US_FDAADTA.Date);

			declaration.US_FDAADTA = ZDate.Empty;
			invoiceLine.US_ATFInd = "D";
			var atfLine = invoiceLine.ATFLines.AddNew();
			AssertEquals(ZDate.BrettsBirthday.AddDays(10), declaration.US_FDAADTA.Date);

			declaration.US_FDAADTA = ZDate.Empty;
			invoiceLine.US_DEAInd = "D";
			var deaLine = invoiceLine.DEAHeaders.AddNew();
			AssertEquals(ZDate.BrettsBirthday.AddDays(10), declaration.US_FDAADTA.Date);

			declaration.US_FDAADTA = ZDate.Empty;
			invoiceLine.US_AMSInd = "D";
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = "MO1";
			AssertEquals(ZDate.BrettsBirthday.AddDays(10), declaration.US_FDAADTA.Date);

			declaration.US_FDAADTA = ZDate.Empty;
			invoiceLine.US_AMSInd = "C";
			invoiceLine.US_AMSDisclaimProgram = "MO7";
			AssertEquals(ZDate.BrettsBirthday.AddDays(10), declaration.US_FDAADTA.Date);
		}

		public void TestUnitConverterDataProviderWithoutProduct()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TST";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";
			product.OP_StockKeepingUnit = "CS";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var conversion = product.PartUnits.AddNew();
			conversion.OF_PackType = "L";
			conversion.OF_QuantityInParent = 8m;
			conversion.OF_ParentPackType = "CS";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_ACEFDAIndicator = "D";

			var fdaOnProductForm = pivot.ACEFDAs.AddNew();
			fdaOnProductForm.US_ProgramCode = "BIO";
			fdaOnProductForm.US_Qty1 = 750m;
			fdaOnProductForm.US_UQ1 = "ML";
			fdaOnProductForm.US_Qty2 = 12m;
			fdaOnProductForm.US_UQ2 = "BO";
			fdaOnProductForm.US_Qty3 = 0m;
			fdaOnProductForm.US_UQ3 = "CS";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "CS";

			invoiceLine.JI_PartNo = "Test";
			Assert(invoiceLine.aceFDALines.Count > 0);
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("Total 90000.00 ML", invoiceLine.aceFDALines[0].FDAQtyRunningTotal);
		}

		public void TestNoDefaultFSVPWhenIORIsUnmatched()
		{
			var importer = OrgHeader.UnmatchOrg(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.IOROrgPK = importer.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			fdaLine.US_ProductCode = "21SFC08";
			fdaLine.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			Assert(fdaLine.FSVPImporterOrgPK.IsEmpty);
		}

		public void TestFDANumberWithUnknownDUNSNumber()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TST";
			importer.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, USACEFDAAddInfoValidation.DUNSUnknown, "US");
			importer.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "123456789", "US");

			FDA.US_FDAImporterAddress = importer.MainAddress.PK;
			FDA.US_FSVPImporterAddress = importer.MainAddress.PK;

			AssertEquals("123456789", FDA.FDAImporterNumber.Number);
			AssertEquals(OrgCusCodeForFDA.FEIEntityIdentificationCode, FDA.FDAImporterNumber.ID);

			AssertEquals(USACEFDAAddInfoValidation.DUNSUnknown, FDA.FSVPImporterNumber.Number);
			AssertEquals(OrgCusCodeForFDA.DUNSEntityIdentificationCode, FDA.FSVPImporterNumber.ID);
		}

		public void TestCS00466699_CannotDeserialize_US_CanDim1_16th()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			FDA.US_DimUQ = "I";
			FDA.US_CanDim1_16th = 10;
			FDA.US_CanDim2_16th = 8;
			FDA.US_CanDim3_16th = 9;
			AssertNoExceptionThrown(() => Factory.Save());

			var fadAddInfo = Factory.Load<CusAddInfo>(FDA.PK);
			Assert(!fadAddInfo.B7_AddInfoData.Contains("CanDim1_16th"));
			Assert(!fadAddInfo.B7_AddInfoData.Contains("CanDim2_16th"));
			Assert(!fadAddInfo.B7_AddInfoData.Contains("CanDim3_16th"));

			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(FDA.InvoiceLine.Declaration.PK);
			var invoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault();
			AssertNoExceptionThrown(() => invoiceLine.ACE_FDALines.Cast<ACEFDA>().FirstOrDefault());
		}

		public void TestUS_CanDim_16thWithValue10()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
			FDA.US_ContainerDimType = CylindricalRectangularList.Codes.Cylindrical;
			FDA.US_DimUQ = "I";
			FDA.US_CanDim1Inch = 20;
			FDA.US_CanDim1_16th = 7;
			FDA.US_CanDim2Inch = 10;
			FDA.US_CanDim2_16th = 10;
			FDA.US_CanDim3Inch = 30;
			FDA.US_CanDim3_16th = 12;

			Factory.Save();
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(FDA.InvoiceLine.Declaration.PK);
			var invoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault();
			var fda = invoiceLine.ACE_FDALines.Cast<ACEFDA>().FirstOrDefault();
			AssertNotNull("Load a FDA", fda);
			AssertEquals("US_CanDim1_16thIn - 7", 7, fda.US_CanDim1_16thInfo.Value);
			AssertEquals("US_CanDim2_16thIn - 10", 10, fda.US_CanDim2_16thInfo.Value);
			AssertEquals("US_CanDim3_16thIn - 12", 12, fda.US_CanDim3_16thInfo.Value);
		}

		public void TestGetDefaultUnitValueFromBaseQty()
		{
			FDA.US_Qty1 = 10m;
			FDA.US_Qty2 = 20m;
			FDA.US_InvCurrValue = 40m;
			var result = FDA.GetDefaultUnitValueFromBaseQty();
			Assert(result.HasValue);
			AssertEquals(0.2m, result.Value);

			FDA.US_Qty1 = 0.05m;
			FDA.US_Qty2 = 0.02m;
			FDA.US_Qty3 = 0.03m;
			AssertNoExceptionThrown(() => result = FDA.GetDefaultUnitValueFromBaseQty());
			Assert(!result.HasValue);
		}

		public void TestPGALineReadOnly()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_ALG;
			FDA.US_ProductCode = "62JAA12";
			Factory.Save();
			FDA.OnLoaded();
			Assert(!FDA.ReadOnly);

			FDA.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			FDA.OnLoaded();
			Assert(FDA.ReadOnly);

			FDA.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			FDA.OnLoaded();
			Assert(!FDA.ReadOnly);

			FDA.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			FDA.OnLoaded();
			Assert(FDA.ReadOnly);

			FDA.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			FDA.OnLoaded();
			Assert(FDA.ReadOnly);
		}

		public void TestUS_ContainerDimType_ReadOnly()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			AssertEquals(false, FDA.US_ContainerDimTypeInfo.ReadOnly);
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			AssertEquals(true, FDA.US_ContainerDimTypeInfo.ReadOnly);
		}

		public void TestUS_DimUQ_ReadOnly()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			AssertEquals(false, FDA.US_DimUQInfo.ReadOnly);
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			AssertEquals(true, FDA.US_DimUQInfo.ReadOnly);
		}

		public void TestIsPriorNotice()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			Factory.Save();

			var fdaLine1 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine1.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fdaLine1.US_FDAForcePN = true;
			Assert(fdaLine1.IsPriorNotice);
			Assert(fdaLine1.IsPriorNoticeForAutoRating);

			var fdaLine2 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			Assert(!fdaLine2.IsPriorNotice);
			Assert(!fdaLine2.IsPriorNoticeForAutoRating);

			fdaLine1.US_PNC = "";
			fdaLine1.US_PND = false;
			Assert(fdaLine1.IsPriorNotice);
			Assert(fdaLine1.IsPriorNoticeForAutoRating);

			fdaLine1.US_PND = true;
			Assert(!fdaLine1.IsPriorNotice);
			Assert(!fdaLine1.IsPriorNoticeForAutoRating);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			fdaLine1.US_PND = false;
			Assert(!fdaLine1.IsPriorNotice);
			Assert(!fdaLine1.IsPriorNoticeForAutoRating);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			fdaLine1.US_PNC = "PNC0001";
			Assert(!fdaLine1.IsPriorNotice);
			Assert(!fdaLine1.IsPriorNoticeForAutoRating);

			fdaLine1.US_IsPNCFromMsg = true;
			Assert(!fdaLine1.IsPriorNotice);
			Assert(fdaLine1.IsPriorNoticeForAutoRating);

			fdaLine1.US_PNC = "";
			fdaLine1.US_PND = false;
			Assert(fdaLine1.IsPriorNotice);

			declaration.US_GoodsFromFTZ = "B815";
			Assert(fdaLine1.IsPriorNotice);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Assert(!fdaLine1.IsPriorNotice);
		}

		public void TestSetFDAQuantityDefaultsByInvoice()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableSPN = true;

			Assert("PReCondition", declaration.CanHavePGAFDA);

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
			var fdaOnProduct = pivot.ACEFDAs.AddNew();
			fdaOnProduct.US_ProductCode = "123456";
			fdaOnProduct.US_UQ1 = "KG";
			fdaOnProduct.US_Qty1 = 5m;
			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 1000m;
			invoiceLine.JI_InvoiceUQ = "KG";

			AssertEquals(1, invoiceLine.ACE_FDALines.Count);
			AssertEquals("Macth the Base FDA UQ, and only has base FDA", 1000m, invoiceLine.ACE_FDALines[0].US_Qty1);

			fdaOnProduct.US_UQ2 = "CA";
			fdaOnProduct.US_Qty2 = 20m;
			fdaOnProduct.US_UQ3 = "CS";
			fdaOnProduct.US_Qty3 = 0m;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = "Test";
			invoiceLine2.JI_InvoiceQuantity = 800m;
			invoiceLine2.JI_InvoiceUQ = "CS";
			AssertEquals(1, invoiceLine2.ACE_FDALines.Count);
			AssertEquals("Match Last FDA UQ", 800m, invoiceLine2.ACE_FDALines[0].US_Qty3);

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = "Test";
			invoiceLine3.JI_InvoiceQuantity = 1000m;
			invoiceLine3.JI_InvoiceUQ = "KG";
			AssertEquals(1, invoiceLine3.ACE_FDALines.Count);
			AssertEquals("Macth the base FDA and the last FDA qty is empty", invoiceLine3.JI_InvoiceQuantity / fdaOnProduct.US_Qty1 / fdaOnProduct.US_Qty2, invoiceLine3.ACE_FDALines[0].US_Qty3);

			fdaOnProduct.US_UQ3 = "EA";
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_PartNo = "Test";
			invoiceLine4.JI_InvoiceQuantity = 1000m;
			invoiceLine4.JI_InvoiceUQ = "CS";
			AssertEquals(1, invoiceLine4.ACE_FDALines.Count);
			AssertEquals(0m, invoiceLine4.ACE_FDALines[0].US_Qty3);
			invoiceLine4.ACE_FDALines[0].US_UQ3 = "CS";
			AssertEquals("when the Measure changed the default can occur", 1000m, invoiceLine4.ACE_FDALines[0].US_Qty3);

			fdaOnProduct.US_Qty1 = 0m;
			fdaOnProduct.US_Qty2 = 0m;
			fdaOnProduct.US_Qty3 = 0m;
			var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine5.JI_PartNo = "Test";
			invoiceLine5.JI_InvoiceUQ = "KG";
			invoiceLine5.JI_InvoiceQuantity = 1000m;
			AssertEquals(1, invoiceLine5.ACE_FDALines.Count);
			AssertEquals(0m, invoiceLine5.ACE_FDALines[0].US_Qty3);
			AssertEquals(1000m, invoiceLine5.JI_InvoiceQuantity);
		}

		public void TestSetFDADefaultValueForBaseQty()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1000m;
			invoiceLine.JI_InvoiceUQ = "CS";
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			Factory.Save();

			var fDAOnInvoiceLine = invoiceLine.ACE_FDALines.AddNew();
			AssertEquals("CS", fDAOnInvoiceLine.US_UQ1);
			AssertEquals(1000m, fDAOnInvoiceLine.US_Qty1);
		}

		public void TestFDAFirmTypeWithAgencyCodeFoo()
		{
			var importer = Factory.New<OrgHeader>();
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "MANF3";
			var manufacturer2MainAddress = manufacturer.MainAddress;
			manufacturer2MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber, "456328756");

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test";

			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			pivot.CD_ACEFDAIndicator = OGAIndicatorList.Codes.Declared;
			var newLine = pivot.ACEFDAs.AddNew();
			newLine.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			newLine.ManufacturerOrgPK = manufacturer.PK;
			newLine.US_ProducerType = ZString.Empty;
			AssertEquals("", newLine.US_PFR);

			newLine.US_PFR = ZString.Empty;
			newLine.US_ProducerType = ProducerFirmTypeList.Codes.M;
			AssertEquals("", newLine.US_PFR);
			newLine.US_ProducerType = ProducerFirmTypeList.Codes.G;
			AssertEquals("", newLine.US_PFR);

			newLine.US_PFR = "123123123";
			AssertEquals("123123123", newLine.US_PFR);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableSPN = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 1000m;
			invoiceLine.JI_InvoiceUQ = "KG";

			AssertEquals(invoiceLine.US_FDAIndicator, OGAIndicatorList.Codes.Declared);
			AssertEquals(1, invoiceLine.ACE_FDALines.Count);
			AssertEquals("after load product, it should be empty", FDAProgramCodeList.Codes.FOO, invoiceLine.ACE_FDALines[0].US_ProgramCode);
			AssertEquals("Should be M", ProducerFirmTypeList.Codes.G, invoiceLine.ACE_FDALines[0].US_ProducerType);
			AssertEquals("123123123", invoiceLine.ACE_FDALines[0].US_PFR);

			invoiceLine.ACE_FDALines[0].US_PFR = ZString.Empty;
			invoiceLine.ACE_FDALines[0].US_ProducerType = ProducerFirmTypeList.Codes.M;
			AssertEquals("456328756", invoiceLine.ACE_FDALines[0].US_PFR);

			invoiceLine.ACE_FDALines[0].US_PFR = ZString.Empty;
			invoiceLine.ACE_FDALines[0].US_ProducerType = ProducerFirmTypeList.Codes.C;
			AssertEquals("456328756", invoiceLine.ACE_FDALines[0].US_PFR);

			invoiceLine.ACE_FDALines[0].US_PFR = ZString.Empty;
			invoiceLine.ACE_FDALines[0].US_ProducerType = ProducerFirmTypeList.Codes.G;
			AssertEquals("456328756", invoiceLine.ACE_FDALines[0].US_PFR);
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
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 100;
			invoiceLine.JI_InvoiceUQ = AESUnitOfMeasureList.Codes.Case;
			var fda1 = invoiceLine.ACE_FDALines.AddNew();
			fda1.US_UQ1 = FDAUQList.Codes.CS;
			AssertEquals(fda1.US_Qty1, invoiceLine.JI_InvoiceQuantity);
		}

		public void TestItemIdentityNumberQualifier()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			FDA.US_ItemIdentityNumber = "123456";
			FDA.US_ItemIdentityNumberQualifier = "VIN";
			var iFDADetails = (IFDAData)FDA;
			AssertEquals("123456", iFDADetails.ItemIdentityNumber);
			AssertEquals("VIN", iFDADetails.ItemIdentityNumberQualifier);
		}

		public void TestCloneInternal()
		{
			var invoiceLine = FDA.InvoiceLine;
			var fda1 = invoiceLine.ACE_FDALines.AddNew();
			var affcode = fda1.AffirmationCodes.AddNew();
			affcode.CY_Code = "ABC";

			fda1.Lots.RemoveAndDeleteAll();
			var lot = fda1.Lots.AddNew();
			lot.US_DegreeType = "C";

			var constituentElements = fda1.ProductConstituentElements.AddNew();
			constituentElements.US_SpeciesName = "SP";

			var newFDA = (ACEFDA)fda1.Clone();

			AssertEquals(newFDA.AffirmationCodes.Count, 1);
			AssertEquals(newFDA.AffirmationCodes[0].CY_Code, "ABC");

			AssertEquals(newFDA.Lots.Count, 1);
			AssertEquals(newFDA.Lots[0].US_DegreeType, "C");

			AssertEquals(newFDA.ProductConstituentElements.Count, 1);
			AssertEquals(newFDA.ProductConstituentElements[0].US_SpeciesName, "SP");
		}

		public void TestClone()
		{
			var invoiceLine = FDA.InvoiceLine;

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_ALG;
			FDA.US_ProductCode = "62JAA12";
			FDA.US_Description = "MORE";
			FDA.US_ProdCountry = "DE";
			FDA.US_SourceCountry = "DE";
			FDA.FDAImporterOrgPK = ZGuid.Empty;
			FDA.US_FDAImporterAddress = ZGuid.Empty;
			FDA.US_IntendedUseCode = IntendedUseCodesList.Codes.ForIntroductionOrReintroductionIntoTheWild;
			FDA.US_OwnerAddress = ZGuid.Empty;
			FDA.US_ProducerType = ProducerFirmTypeList.Codes.M;
			FDA.US_LocationOfGoodsAddress = ZGuid.Empty;
			FDA.US_ManufacturerAddress = ZGuid.Empty;
			FDA.US_PNC = "AS32";
			FDA.ShipperOrgPK = ZGuid.Empty;
			FDA.US_OA_ShipperAddress = ZGuid.Empty;

			FDA.US_Qty1 = 10m;
			FDA.US_UQ1 = FDAUQList.Codes.HR;
			FDA.US_Qty2 = 10m;
			FDA.US_UQ2 = FDAUQList.Codes.AE;
			FDA.US_Qty3 = 10m;
			FDA.US_UQ3 = FDAUQList.Codes.AM;
			FDA.US_Qty4 = 10m;
			FDA.US_UQ4 = FDAUQList.Codes.AP;
			FDA.US_Qty5 = 10m;
			FDA.US_UQ5 = FDAUQList.Codes.AT;
			FDA.US_Qty6 = 10m;
			FDA.US_UQ6 = FDAUQList.Codes.BA;

			var affirmationCode = FDA.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = "CY";
			affirmationCode.CY_Data = "NO DATA";

			var lot = FDA.Lots.AddNew();
			lot.US_TemperatureQualifier = TemperatureQualifierList.Codes.Flashpoint;
			lot.US_Temperature = 5m;
			lot.US_DegreeType = DegreeTypeList.Codes.Fahrenheit;
			lot.US_LocationOfTemp = "C";
			lot.US_LotNumber = "55";

			var productConstituentElements = FDA.ProductConstituentElements.AddNew();
			productConstituentElements.US_PGANameOfTheConstituentElement = "TOLUENE";
			productConstituentElements.US_PGAQuantityOfConstituentElement = 1m;
			productConstituentElements.US_PGAUnitOfMeasure = "KG";
			productConstituentElements.US_PGAPercentOfConstituentElement = 15m;
			productConstituentElements.ProducerOrgPK = ZGuid.Empty;
			productConstituentElements.US_OA_ProducerAddress = ZGuid.Empty;

			var license = FDA.Licenses.AddNew();
			license.US_CountryCode = "DE";
			license.US_Number = "AHA121";
			license.US_StateCode = "BY";
			license.US_StateDescription = "BAVARIA";

			invoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var acefdaNew = invoiceLine.ACE_FDALines.AddNew();

			AssertEquals("PGALineStatusProgramCode must be the same", FDA.US_ProgramCode, acefdaNew.US_ProgramCode);
			AssertEquals("ProcessingCode must be the same", FDA.US_ProcessingCode, acefdaNew.US_ProcessingCode);
			AssertEquals("ProductCode must be the same", FDA.US_ProductCode, acefdaNew.US_ProductCode);
			AssertEquals("Description must be the same", FDA.US_Description, acefdaNew.US_Description);
			AssertEquals("ProdCountry must be the same", FDA.US_ProdCountry, acefdaNew.US_ProdCountry);
			AssertEquals("SourceCountry must be the same", FDA.US_SourceCountry, acefdaNew.US_SourceCountry);
			AssertEquals("FDAImporterOrgPK must be the same", FDA.FDAImporterOrgPK, acefdaNew.FDAImporterOrgPK);
			AssertEquals("FDAImporterAddress must be the same", FDA.US_FDAImporterAddress, acefdaNew.US_FDAImporterAddress);
			AssertEquals("IntendedUseCode must be the same", FDA.US_IntendedUseCode, acefdaNew.US_IntendedUseCode);
			AssertEquals("OwnerAddress must be the same", FDA.US_OwnerAddress, acefdaNew.US_OwnerAddress);
			AssertEquals("ProducerType must be the same", FDA.US_ProducerType, acefdaNew.US_ProducerType);
			AssertEquals("LocationOfGoodsAddress must be the same", FDA.US_LocationOfGoodsAddress, acefdaNew.US_LocationOfGoodsAddress);
			AssertEquals("ManufacturerAddress must be the same", FDA.US_ManufacturerAddress, acefdaNew.US_ManufacturerAddress);
			AssertEquals("PNC must be the same", FDA.US_PNC, acefdaNew.US_PNC);
			AssertEquals("ShipperOrgPK must be the same", FDA.ShipperOrgPK, acefdaNew.ShipperOrgPK);
			AssertEquals("OA_ShipperAddress must be the same", FDA.US_OA_ShipperAddress, acefdaNew.US_OA_ShipperAddress);

			// reference to CS00429067. All the qtys need to be cloned when copy the FDA line from product to invoice line.
			// Even when clone the invoice line, since the invoice qty is cloned, all the FDA line qtys should copy over as well.
			AssertEquals("Qty1 need to be cloned", 10m, acefdaNew.US_Qty1);
			AssertEquals("UQ1", FDAUQList.Codes.HR, acefdaNew.US_UQ1);
			AssertEquals("Qty2 need to be cloned", 10m, acefdaNew.US_Qty2);
			AssertEquals("UQ2", FDAUQList.Codes.AE, acefdaNew.US_UQ2);
			AssertEquals("Qty3 need to be cloned", 10m, acefdaNew.US_Qty3);
			AssertEquals("UQ3", FDAUQList.Codes.AM, acefdaNew.US_UQ3);
			AssertEquals("Qty4 need to be cloned", 10m, acefdaNew.US_Qty4);
			AssertEquals("UQ4", FDAUQList.Codes.AP, acefdaNew.US_UQ4);
			AssertEquals("Qty5 need to be cloned", 10m, acefdaNew.US_Qty5);
			AssertEquals("UQ5", FDAUQList.Codes.AT, acefdaNew.US_UQ5);
			AssertEquals("Qty6 need to be cloned", 10m, acefdaNew.US_Qty6);
			AssertEquals("UQ6", FDAUQList.Codes.BA, acefdaNew.US_UQ6);

			AssertEquals("AffirmationCode: CY_Code must be equal", FDA.AffirmationCodes[0].CY_Code, acefdaNew.AffirmationCodes[0].CY_Code);
			AssertEquals("AffirmationCode: CY_Data must be equal", FDA.AffirmationCodes[0].CY_Data, acefdaNew.AffirmationCodes[0].CY_Data);

			AssertEquals("Lot: TemperatureQualifier must be equal", FDA.Lots[0].US_TemperatureQualifier, acefdaNew.Lots[0].US_TemperatureQualifier);
			AssertEquals("Lot: Temperature must be equal", FDA.Lots[0].US_Temperature, acefdaNew.Lots[0].US_Temperature);
			AssertEquals("Lot: DegreeType must be equal", FDA.Lots[0].US_DegreeType, acefdaNew.Lots[0].US_DegreeType);
			AssertEquals("Lot: LocationOfTemp must be equal", FDA.Lots[0].US_LocationOfTemp, acefdaNew.Lots[0].US_LocationOfTemp);
			AssertEquals("Lot: LotNumber must be equal", FDA.Lots[0].US_LotNumber, acefdaNew.Lots[0].US_LotNumber);

			AssertEquals("ProductConstituentElement: PGANameOfTheConstituentElement must be equal", FDA.ProductConstituentElements[0].US_PGANameOfTheConstituentElement, acefdaNew.ProductConstituentElements[0].US_PGANameOfTheConstituentElement);
			AssertEquals("ProductConstituentElement: PGAQuantityOfConstituentElement must be equal", FDA.ProductConstituentElements[0].US_PGAQuantityOfConstituentElement, acefdaNew.ProductConstituentElements[0].US_PGAQuantityOfConstituentElement);
			AssertEquals("ProductConstituentElement: PGAUnitOfMeasure must be equal", FDA.ProductConstituentElements[0].US_PGAUnitOfMeasure, acefdaNew.ProductConstituentElements[0].US_PGAUnitOfMeasure);
			AssertEquals("ProductConstituentElement: PGAPercentOfConstituentElement must be equal", FDA.ProductConstituentElements[0].US_PGAPercentOfConstituentElement, acefdaNew.ProductConstituentElements[0].US_PGAPercentOfConstituentElement);
			AssertEquals("ProductConstituentElement: ProducerOrgPK must be equal", FDA.ProductConstituentElements[0].ProducerOrgPK, acefdaNew.ProductConstituentElements[0].ProducerOrgPK);
			AssertEquals("ProductConstituentElement: OA_ProducerAddress must be equal", FDA.ProductConstituentElements[0].US_OA_ProducerAddress, acefdaNew.ProductConstituentElements[0].US_OA_ProducerAddress);

			AssertEquals("License: CountryCode must be equal", FDA.Licenses[0].US_CountryCode, acefdaNew.Licenses[0].US_CountryCode);
			AssertEquals("License: Number must be equal", FDA.Licenses[0].US_Number, acefdaNew.Licenses[0].US_Number);
			AssertEquals("License: StateCode must be equal", FDA.Licenses[0].US_StateCode, acefdaNew.Licenses[0].US_StateCode);
			AssertEquals("License: StateDescription must be equal", FDA.Licenses[0].US_StateDescription, acefdaNew.Licenses[0].US_StateDescription);

			var clonedFDA = (ACEFDA)FDA.Clone();

			AssertEquals(clonedFDA.AffirmationCodes.Count, 1);
			AssertEquals(clonedFDA.AffirmationCodes[0].CY_Code, "CY");
			AssertEquals(clonedFDA.Lots.Count, 1);
			AssertEquals(clonedFDA.ProductConstituentElements.Count, 1);
			AssertEquals(clonedFDA.Licenses.Count, 1);
		}

		public void TestActiveIngredientProducers()
		{
			var producer = Factory.New<OrgHeader>();
			var address1 = producer.Addresses.AddNew();
			var address2 = producer.Addresses.AddNew();

			var activeIngredient1 = FDA.ProductConstituentElements.AddNew();
			activeIngredient1.US_OA_ProducerAddress = address1.PK;

			var activeIngredient2 = FDA.ProductConstituentElements.AddNew();
			activeIngredient2.US_OA_ProducerAddress = address1.PK;

			var activeIngredient3 = FDA.ProductConstituentElements.AddNew();
			activeIngredient3.US_OA_ProducerAddress = address2.PK;

			var result = ((IFDAData)FDA).ActiveIngredientProducers;
			AssertEquals(2, result.Count());
		}

		public void TestProperties()
		{
			var manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			var manufacturer2MainAddress = manufacturer2.MainAddress;
			manufacturer2MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456328756");

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.VME_ADR;
			Assert(FDA.IsProductConstituentElementRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_CCW;
			Assert(!FDA.IsProductConstituentElementRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_ALG;
			Assert(!FDA.IsProductConstituentElementRequired);
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_XEN;
			Assert(!FDA.IsProductConstituentElementRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_OTC;
			Assert(!FDA.IsProductConstituentElementRequired);

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";

			var invoiceLine = FDA.InvoiceLine;
			invoiceLine.JI_Tariff = importTariff.UE_Tariff;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "FDE" + new Random().Next(1000000).ToString();
			org.OH_FullName = "Dummy Delivery Org";
			org.MainAddress.OA_Address1 = "Address 1";
			invoiceLine.US_UC_NKCountryOfOrigin = "XA";
			invoiceLine.US_UC_NKCountryOfExport = "XA";
			invoiceLine.JI_OA_ShipToPartyAddress = org.MainAddress.PK;
			var fda2 = invoiceLine.ACE_FDALines.AddNew();
			fda2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			AssertEquals("CA", fda2.US_ProdCountry);
			AssertEquals("CA", fda2.US_ShipmentCountry);
			AssertEquals(org.MainAddress.PK, fda2.US_DeliverToPartyAddress);

			fda2.US_DeliverToPartyAddress = manufacturer2MainAddress.PK;
			AssertEquals(manufacturer2MainAddress.PK, fda2.US_DeliverToPartyAddress);
			fda2.Factory.Save();
			Assert(fda2.B7_AddInfoData.Contains("DeliverToPartyAddress"));

			fda2.US_DeliverToPartyAddress = invoiceLine.Declaration.ImporterDeliveryAddress.E2_OA_Address;
			fda2.Factory.Save();
			Assert(!fda2.B7_AddInfoData.Contains("DeliverToPartyAddress"));
		}

		public void TestFDASetDefaultValue()
		{
			var invoiceLine = FDA.InvoiceLine;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.MainAddress.OA_Address1 = "MAIN ADDRESS";

			var address = manufacturer.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "001245789652");
			address.OA_Address1 = "ACTUAL ADDRESS";

			invoiceLine.JI_OA_ManufacturerAddress = address.PK;
			invoiceLine.Declaration.US_FDAAPC = "2704";
			invoiceLine.Declaration.US_SchDArrival = "3972";

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			AssertEquals(ProducerFirmTypeList.Codes.M, FDA.US_ProducerType);
			AssertEquals(ZGuid.Empty, FDA.US_ProducerAddress);

			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber, "12345678");
			var fda2 = invoiceLine.ACE_FDALines.AddNew();
			fda2.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			AssertEquals("", fda2.US_ProducerType);
			AssertEquals(invoiceLine.JI_OA_ManufacturerAddress, fda2.US_ProducerAddress);
			AssertEquals("", fda2.US_PFR);

			fda2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			AssertEquals("12345678", fda2.US_PFR);

			fda2.US_PFR = "790001245";
			fda2.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			AssertEquals("12345678", fda2.US_PFR);
		}

		public void TestFDADefaultManufacturerAddress()
		{
			var invoiceLine = FDA.InvoiceLine;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.MainAddress.OA_Address1 = "MAIN ADDRESS";

			OrgHeaderWrapper manufacturerWrapped = OrgHeaderWrapper.New(manufacturer);
			manufacturerWrapped.ZO_ProducerFirmType = ProducerFirmTypeList.Codes.C;

			var address = manufacturer.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "001245789652");
			address.OA_Address1 = "ACTUAL ADDRESS";

			invoiceLine.JI_OA_ManufacturerAddress = address.PK;

			FDA.US_ProgramCode = "TOB";
			AssertEquals("Manufacturer Address should not be defaulted when setting the programCode to TOB", ZGuid.Empty, FDA.US_ManufacturerAddress);
			AssertEquals("Firm Type should not be defaulted when setting the programCode to TOB", ZString.Empty, FDA.US_ProducerType);

			FDA.US_ProgramCode = "FOO";
			AssertEquals("Manufacturer Address should be defaulted when setting the programCode", address.PK, FDA.US_ManufacturerAddress);
			AssertEquals("Firm Type should be defaulted when setting the Manufacturer Address", ProducerFirmTypeList.Codes.C, FDA.US_ProducerType);
		}

		public void TestIFDADataMembers()
		{
			var invoiceLine = FDA.InvoiceLine;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.MainAddress.OA_Address1 = "MAIN ADDRESS";

			var address = manufacturer.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "1245789652");
			address.OA_Address1 = "ACTUAL ADDRESS";

			invoiceLine.JI_OA_ManufacturerAddress = address.PK;
			invoiceLine.Declaration.US_FDAAPC = "2704";
			invoiceLine.Declaration.US_SchDArrival = "3972";
			invoiceLine.Declaration.US_SchDEntry = "3902";

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_IntendedUseDescr = "THIS IS VERY LONG DESC";
			var iFDADetails = (IFDAData)FDA;
			AssertEquals("47", iFDADetails.ManufacturerNumber.ID);
			AssertEquals("1245789652", iFDADetails.ManufacturerNumber.Number);
			AssertEquals("ACTUAL ADDRESS", iFDADetails.ManufacturerContact.CompanyAddress.AddressLine1);
			AssertEquals("THIS IS VERY LONG DES", iFDADetails.IntendedUseDescription);

			var manufacturer2 = Factory.New<OrgHeader>();
			var manufacturer2MainAddress = manufacturer2.MainAddress;
			manufacturer2MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "456328756");

			FDA.US_ManufacturerAddress = manufacturer2MainAddress.PK;
			AssertEquals("16", iFDADetails.ManufacturerNumber.ID);
			AssertEquals("456328756", iFDADetails.ManufacturerNumber.Number);
			AssertEquals("3902", iFDADetails.ArrivalLocation);

			AssertEquals(false, iFDADetails.IsSubmitterRelevant);
			AssertEquals(false, iFDADetails.IsTransmitterRelevant);

			FDA.US_CanDim1 = 14.05m;
			AssertEquals("1405", iFDADetails.CanDimensions1);
			FDA.US_CanDim2 = 8m;
			AssertEquals(" 800", iFDADetails.CanDimensions2);
			FDA.US_CanDim3 = 6.08m;
			AssertEquals(" 608", iFDADetails.CanDimensions3);
			FDA.US_CanDim3 = 0.02m;
			AssertEquals(" 002", iFDADetails.CanDimensions3);
			FDA.US_CanDim3 = 0.20m;
			AssertEquals(" 020", iFDADetails.CanDimensions3);
			FDA.US_CanDim3 = 0.25m;
			AssertEquals(" 025", iFDADetails.CanDimensions3);

			var declaration = invoiceLine.Declaration;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "ADMIRALENGRACHT66666688888888";

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			importTariff.UE_PGACodes = "FD4";
			FDA.InvoiceLine.JI_Tariff = importTariff.UE_Tariff;

			AssertEquals("ADMIRALENGRACHT66666", iFDADetails.AffirmationOfCompliance.FirstOrDefault(x => x.Key == ACE_AffirmationOfComplianceList.Codes.VES).Value);

			var fda2 = invoiceLine.ACE_FDALines.AddNew();
			fda2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda2.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			AssertEquals(SourceTypeCodesList.Codes.PlaceOfGrowth, ((IFDAData)fda2).ProductionGrowthCountryQualifier);
			AssertEquals(true, iFDADetails.IsSubmitterRelevant);
			AssertEquals(true, iFDADetails.IsTransmitterRelevant);

			fda2.US_ProducerType = ProducerFirmTypeList.Codes.M;
			fda2.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_FEE;
			AssertEquals(SourceTypeCodesList.Codes.CountryOfProduction, ((IFDAData)fda2).ProductionGrowthCountryQualifier);

			fda2.US_ProducerType = ProducerFirmTypeList.Codes.C;
			AssertEquals(SourceTypeCodesList.Codes.PlaceOfGrowth, ((IFDAData)fda2).ProductionGrowthCountryQualifier);

			fda2.US_PNC = "1234";
			AssertEquals(SourceTypeCodesList.Codes.CountryOfProduction, ((IFDAData)fda2).ProductionGrowthCountryQualifier);

			var foreignExporter = Factory.New<OrgHeader>();
			foreignExporter.MainAddress.OA_Address1 = "MAIN ADDRESS";

			var foreignExporterAddress = foreignExporter.Addresses.AddNew();
			foreignExporterAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "001245789652");
			foreignExporterAddress.OA_Address1 = "ACTUAL ADDRESS";
			invoiceLine.JI_OA_ExporterAddress = foreignExporterAddress.PK;

			var foreignExporterAddress2 = foreignExporter.Addresses.AddNew();
			foreignExporterAddress2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "00846530000");
			foreignExporterAddress2.OA_Address1 = "ADDRESS 2";
			FDA.US_OA_ShipperAddress = foreignExporterAddress2.PK;

			AssertEquals("47", iFDADetails.ShipperNumber.ID);
			AssertEquals("00846530000", iFDADetails.ShipperNumber.Number);
			AssertEquals("ADDRESS 2", iFDADetails.ShipperContact.CompanyAddress.AddressLine1);
		}

		public void TestSetFDAQty()
		{
			var invoiceLine = FDA.InvoiceLine;
			invoiceLine.JI_LinePrice = 120m;
			AssertEquals(120m, FDA.US_InvCurrValue);
			invoiceLine.JI_LinePrice = 140m;
			AssertEquals(140m, FDA.US_InvCurrValue);

			invoiceLine.Declaration.US_EnableENS = true;
			invoiceLine.Declaration.US_EntryFilerCode = "XJ5";
			invoiceLine.Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("140", FDA.US_TotalUSDValue);
			AssertEquals(140m, FDA.US_TotalValue);
		}

		public void TestUS_InvCurrValueWithTwoScale()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryType = "11";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.CAFTABenefitsApplicable;
			invoiceLine.US_FDAIndicator = "D";
			var fdaLine1 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine1.US_InvCurrValue = 124.88m;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var job1 = factory.Load<JobDeclaration>(declaration.PK);
			var invoiceLine1 = job1.InvoiceLines[0];
			var fda1 = invoiceLine1.ACE_FDALines[0];
			AssertEquals(124.88m, fda1.US_InvCurrValue);
		}

		public void TestFDAQtyRunningTotal()
		{
			FDA.US_Qty1 = 1.11m;
			FDA.US_UQ1 = "BX";
			FDA.US_Qty2 = 2.88m;
			FDA.US_Qty3 = 3.12m;
			FDA.US_Qty4 = 4.45m;
			FDA.US_Qty5 = 5.88m;
			FDA.US_Qty6 = 6.11m;
			AssertEquals("FDAQtyRunningTotal", "Total 1594.58 BX", FDA.FDAQtyRunningTotal);

			FDA.US_Qty3 = 9.75;
			AssertEquals("FDAQtyRunningTotal", "Total 4983.08 BX", FDA.FDAQtyRunningTotal);

			FDA.US_Qty6 = 0;
			AssertEquals("FDAQtyRunningTotal", "Total 815.56 BX", FDA.FDAQtyRunningTotal);

			FDA.US_Qty1 = 1.38;
			FDA.US_UQ1 = "CT";
			FDA.US_Qty2 = 144;
			FDA.US_Qty3 = 0;
			FDA.US_Qty4 = 0;
			FDA.US_Qty5 = 0;
			FDA.US_Qty6 = 0;
			AssertEquals("FDAQtyRunningTotal", "Total 198.72 CT", FDA.FDAQtyRunningTotal);

			FDA.US_Qty1 = 24;
			FDA.US_UQ1 = "PR";
			FDA.US_Qty2 = 144;
			FDA.US_UQ2 = "BX";
			AssertEquals("FDAQtyRunningTotal", "Total 3456.00 PR", FDA.FDAQtyRunningTotal);

			FDA.US_Qty1 = 5000;
			FDA.US_UQ1 = "BX";
			FDA.US_Qty2 = 15000;
			FDA.US_Qty3 = 8000;
			FDA.US_Qty4 = 10000;
			FDA.US_Qty5 = 30000;
			FDA.US_Qty6 = 7000;
			AssertEquals("FDAQtyRunningTotal", "Large Qty - please check", FDA.FDAQtyRunningTotal);
		}

		public void TestDefaultAffirmationCodes()
		{
			var invoiceLine = FDA.InvoiceLine;

			var seller = Factory.New<OrgHeader>();
			seller.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FDAForeignSellerRegistrationNumber, "124578965");

			invoiceLine.JI_OA_Seller = seller.MainAddress.PK;

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			AssertEquals(1, FDA.AffirmationCodes.Count);
			AssertEquals(ACE_AffirmationOfComplianceList.Codes.REG, FDA.AffirmationCodes[0].CY_Code);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_PVE;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080000;
			Assert(FDA.AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.DA));
			FDA.US_IntendedUseCode = ZString.Empty;

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_HCT;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._082000;
			Assert(FDA.AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.HRN));

			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_BLO;
			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._180016;
			Assert(FDA.AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.BLN));
			Assert(FDA.AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.STN));

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			Assert(!FDA.AffirmationCodes.ContainsCode(ACE_AffirmationOfComplianceList.Codes.FSR));

			FDA.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080012;
			var fsr = FDA.AffirmationCodes.OfType<ACEAffirmationCode>().FirstOrDefault(aoc => aoc.CY_Code == ACE_AffirmationOfComplianceList.Codes.FSR);
			AssertNotNull("A FSR AOC should be defaulted from the Seller", fsr);
			AssertEquals("Registration number FSR should be pulled from Seller", "124578965", fsr.CY_Data);
		}

		public void TestFoodFacilityAffirmationCodes()
		{
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			importTariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.InvoiceLine.JI_Tariff = importTariff.UE_Tariff;
			FDA.InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			FDA.US_FDAForcePN = true;
			FDA.US_ProducerType = ProducerFirmTypeList.Codes.C;
			FDA.US_PFR = "123";
			var affirmationCodes = ((IFDAData)FDA).AffirmationOfCompliance;
			var affirmationCode = affirmationCodes.First(x => x.Key == ACE_AffirmationOfComplianceList.Codes.CFR);
			AssertNotNull(affirmationCode);
			AssertEquals("123", affirmationCode.Value);

			FDA.US_ProducerType = ProducerFirmTypeList.Codes.G;
			affirmationCodes = ((IFDAData)FDA).AffirmationOfCompliance;
			affirmationCode = affirmationCodes.First(x => x.Key == ACE_AffirmationOfComplianceList.Codes.GFR);
			AssertNotNull(affirmationCode);
			AssertEquals("123", affirmationCode.Value);

			FDA.US_ProducerType = ProducerFirmTypeList.Codes.M;
			affirmationCodes = ((IFDAData)FDA).AffirmationOfCompliance;
			affirmationCode = affirmationCodes.First(x => x.Key == ACE_AffirmationOfComplianceList.Codes.PFR);
			AssertNotNull(affirmationCode);
			AssertEquals("123", affirmationCode.Value);

			FDA.US_ProducerType = ZString.Empty;
			affirmationCodes = ((IFDAData)FDA).AffirmationOfCompliance;
			affirmationCode = affirmationCodes.First(x => x.Key == ACE_AffirmationOfComplianceList.Codes.PFR);
			AssertNotNull(affirmationCode);
			AssertEquals("123", affirmationCode.Value);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			ICusAddInfoTypeSupporter supporter = FDA;
			supporter.AssertType(typeof(Lot), CusAddInfoTypeAttribute.Codes.USLot);
			supporter.AssertType(typeof(FDALicense), CusAddInfoTypeAttribute.Codes.USFDALicense);
			supporter.AssertType(typeof(ScientificData), CusAddInfoTypeAttribute.Codes.USSCI);
			supporter.AssertType(typeof(ConstituentElement), CusAddInfoTypeAttribute.Codes.USPGA);
			supporter.AssertType(null, "ZZ!");

			var lot = FDA.Lots.AddNew();
			lot.US_LotNumber = "TEST";

			var license = FDA.Licenses.AddNew();
			license.US_Number = "No1";

			var constituentElement = FDA.ProductConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "TEST";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			CusAddInfo addInfo = newFactory.Load<CusAddInfo>(lot.PK);
			AssertEquals(typeof(Lot), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(license.PK);
			AssertEquals(typeof(FDALicense), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(constituentElement.PK);
			AssertEquals(typeof(ConstituentElement), addInfo.GetType());
		}

		public void TestICusCodeDataTypeSupporter()
		{
			ICusCodeDataTypeSupporter supporter = FDA;
			supporter.AssertType(typeof(ACEAffirmationCode), CusCodeDataTypeList.Codes.AffirmationCode);
			supporter.AssertType(null, "ZZ!");

			var affirmationCode = FDA.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = "HTS";
			affirmationCode.CY_Data = "01021404";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(affirmationCode.PK);
			AssertEquals(typeof(ACEAffirmationCode), codeData.GetType());
		}

		public void TestDimensions()
		{
			var iFDADetails = (IFDAData)FDA;

			FDA.US_CanDim1 = 14.05m;
			AssertEquals("1405", iFDADetails.CanDimensions1);
			AssertEquals(14, FDA.US_CanDim1Inch);
			AssertEquals(5, FDA.US_CanDim1_16th);

			FDA.US_CanDim1 = 0.02m;
			AssertEquals(" 002", iFDADetails.CanDimensions1);
			AssertEquals(0, FDA.US_CanDim1Inch);
			AssertEquals(2, FDA.US_CanDim1_16th);

			FDA.US_CanDim1 = 0.20m;
			AssertEquals(" 020", iFDADetails.CanDimensions1);
			AssertEquals(0, FDA.US_CanDim1Inch);
			AssertEquals(20, FDA.US_CanDim1_16th);

			FDA.US_CanDim1 = 0.25m;
			AssertEquals(" 025", iFDADetails.CanDimensions1);
			AssertEquals(0, FDA.US_CanDim1Inch);
			AssertEquals(25, FDA.US_CanDim1_16th);

			FDA.US_CanDim2 = 8m;
			AssertEquals(" 800", iFDADetails.CanDimensions2);
			AssertEquals(8, FDA.US_CanDim2Inch);
			AssertEquals(0, FDA.US_CanDim2_16th);

			FDA.US_CanDim2 = 0.02m;
			AssertEquals(" 002", iFDADetails.CanDimensions2);
			AssertEquals(0, FDA.US_CanDim2Inch);
			AssertEquals(2, FDA.US_CanDim2_16th);

			FDA.US_CanDim2 = 0.20m;
			AssertEquals(" 020", iFDADetails.CanDimensions2);
			AssertEquals(0, FDA.US_CanDim2Inch);
			AssertEquals(20, FDA.US_CanDim2_16th);

			FDA.US_CanDim1 = 0.25m;
			AssertEquals(" 025", iFDADetails.CanDimensions1);
			AssertEquals(0, FDA.US_CanDim1Inch);
			AssertEquals(25, FDA.US_CanDim1_16th);

			FDA.US_CanDim3 = 6.08m;
			AssertEquals(" 608", iFDADetails.CanDimensions3);
			AssertEquals(6, FDA.US_CanDim3Inch);
			AssertEquals(8, FDA.US_CanDim3_16th);

			FDA.US_CanDim3 = 0.02m;
			AssertEquals(" 002", iFDADetails.CanDimensions3);
			AssertEquals(0, FDA.US_CanDim3Inch);
			AssertEquals(2, FDA.US_CanDim3_16th);

			FDA.US_CanDim3 = 0.20m;
			AssertEquals(" 020", iFDADetails.CanDimensions3);
			AssertEquals(0, FDA.US_CanDim3Inch);
			AssertEquals(20, FDA.US_CanDim3_16th);

			FDA.US_CanDim3 = 0.25m;
			AssertEquals(" 025", iFDADetails.CanDimensions3);
			AssertEquals(0, FDA.US_CanDim3Inch);
			AssertEquals(25, FDA.US_CanDim3_16th);
		}

		public void TestIsDeletedOrBeingDeleted()
		{
			IPGADataCorrection correction = null;
			AssertEquals(false, correction.IsDeletedOrBeingDeleted());
			correction = FDA;
			AssertEquals(false, correction.IsDeletedOrBeingDeleted());
			FDA.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			AssertEquals(true, correction.IsDeletedOrBeingDeleted());
			FDA.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;
			AssertEquals(true, correction.IsDeletedOrBeingDeleted());
			FDA.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			AssertEquals(true, correction.IsDeletedOrBeingDeleted());
			FDA.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			AssertEquals(false, correction.IsDeletedOrBeingDeleted());
			FDA.Delete();
			AssertEquals(true, correction.IsDeletedOrBeingDeleted());
		}

		public void TestDeliveryPartyContact()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			orgHeader.MainAddress.OA_Address1 = "MAIN ADDRESS";
			var newAddressOne = orgHeader.Addresses.AddNew();
			newAddressOne.OA_Address1 = "IAN TEST ADDRESS ONE";
			var newAddressTwo = orgHeader.Addresses.AddNew();
			newAddressTwo.OA_Address1 = "IAN TEST ADDRESS TWO";

			FDA.InvoiceLine.JI_OA_ConsigneeAddress = newAddressOne.PK;
			var deliveryParty = ((IFDAData)FDA).DeliveryPartyContact;
			AssertEquals("IAN TEST ADDRESS ONE", deliveryParty.CompanyAddress.AddressLine1);
			FDA.InvoiceLine.JI_OA_ConsigneeAddress = orgHeader.MainAddress.PK;
			FDA.US_DeliverToPartyAddress = newAddressTwo.PK;
			deliveryParty = ((IFDAData)FDA).DeliveryPartyContact;
			AssertEquals("IAN TEST ADDRESS TWO", deliveryParty.CompanyAddress.AddressLine1);
		}

		public void TestProductCode()
		{
			FDA.US_ProductCode = "0289F81";
			AssertEquals(true, FDA.IsLACF);
			AssertEquals(false, FDA.IsAcidified);
			AssertEquals(true, FDA.US_FDAForcePN);

			FDA.US_ProductCode = "4102I76";
			AssertEquals(false, FDA.IsLACF);
			AssertEquals(true, FDA.IsAcidified);
			AssertEquals(true, FDA.US_FDAForcePN);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_PGACodes = "FD3";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(5);
			FDA.InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			FDA.US_ProductCode = "71AYA01";
			AssertEquals(false, FDA.US_FDAForcePN);

			tariff.UE_PGACodes = "FD4";
			FDA.InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			FDA.US_ProductCode = "52DIY01";
			AssertEquals(false, FDA.US_FDAForcePN);

			FDA.InvoiceLine.JI_Tariff = ZString.Empty;
			FDA.InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			FDA.US_ProductCode = "71AYA01";
			AssertEquals(false, FDA.US_FDAForcePN);
		}

		public void TestIsFSVPImpRequired()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_PNC = "aaa";
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			AssertEquals(true, FDA.IsFSVPImpRequired);

			FDA.US_PNC = "";
			FDA.US_PND = true;
			AssertEquals(true, FDA.IsFSVPImpRequired);

			FDA.US_PND = false;
			AssertEquals(true, FDA.IsFSVPImpRequired);

			FDA.US_FDAForcePN = false;
			AssertEquals(true, FDA.IsFSVPImpRequired);

			FDA.US_FDAForcePN = true;
			AssertEquals(true, FDA.IsFSVPImpRequired);

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.COS;
			AssertEquals(false, FDA.IsFSVPImpRequired);
		}

		public void TestDRUProcessingCode()
		{
			FDA.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			AssertEquals("FDA DRU has 5 items", "INV, OTC, PHN, PRE, RND, 804", FDA.AddInfoLookups.ProcessingCodeList.CodesAsString);
		}

		public void TestDefaultFSVIImporter()
		{
			var orgHeader = Factory.New<OrgHeader>();
			FDA.InvoiceLine.Declaration.IOROrgPK = orgHeader.PK;

			FDA.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			FDA.US_PNC = "aaa";
			FDA.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;

			Assert(FDA.FSVPImporterOrgPK.IsEmpty);

			FDA.US_ProductCode = "123456";

			AssertEquals(orgHeader.PK, FDA.FSVPImporterOrgPK);

			var orgHeader2 = Factory.New<OrgHeader>();
			var address = orgHeader2.Addresses.AddNew();
			FDA.US_FSVPImporterAddress = address.PK;

			AssertEquals(orgHeader2.PK, FDA.FSVPImporterOrgPK);
		}

		public void TestFSVPImporterNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "100000000");
			FDA.US_FSVPImporterAddress = address.PK;

			AssertEquals("16", FDA.FSVPImporterNumber.ID);
			AssertEquals("100000000", FDA.FSVPImporterNumber.Number);

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var docAddress = FDA.FSVPImporterDocAddress ?? FDA.DocAddresses.CreateWithAddressType(DocAddressType.FSVPImporter);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				docAddress.E2_GovRegNum = "100000001";

				AssertEquals("16", FDA.FSVPImporterNumber.ID);
				AssertEquals("100000001", FDA.FSVPImporterNumber.Number);

				docAddress.E2_AddressOverride = false;
				docAddress.OrganisationPK = orgHeader.PK;

				AssertEquals("16", FDA.FSVPImporterNumber.ID);
				AssertEquals("100000000", FDA.FSVPImporterNumber.Number);
			}
		}

		public void TestManufacturerNumber()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var docAddress = FDA.ManufacturerDocAddress ?? FDA.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				docAddress.E2_GovRegNum = "200000001";

				AssertEquals("16", FDA.ManufacturerNumber.ID);
				AssertEquals("200000001", FDA.ManufacturerNumber.Number);

				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
				docAddress.E2_GovRegNum = "200000002";

				AssertEquals("47", FDA.ManufacturerNumber.ID);
				AssertEquals("200000002", FDA.ManufacturerNumber.Number);
			}
		}

		public void TestDeliveryPartyNumber()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var docAddress = FDA.DeliverToPartyDocAddress ?? FDA.DocAddresses.CreateWithAddressType(DocAddressType.GoodsDeliveredTo);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				docAddress.E2_GovRegNum = "300000001";

				AssertEquals("16", FDA.DeliveryPartyNumber.ID);
				AssertEquals("300000001", FDA.DeliveryPartyNumber.Number);

				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
				docAddress.E2_GovRegNum = "300000002";

				AssertEquals("47", FDA.DeliveryPartyNumber.ID);
				AssertEquals("300000002", FDA.DeliveryPartyNumber.Number);
			}
		}

		public void TestFDAImporterNumber()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var docAddress = FDA.FDAImporterDocAddress ?? FDA.DocAddresses.CreateWithAddressType(DocAddressType.ImporterDocumentaryAddress);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				docAddress.E2_GovRegNum = "400000001";

				AssertEquals("16", FDA.FDAImporterNumber.ID);
				AssertEquals("400000001", FDA.FDAImporterNumber.Number);

				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
				docAddress.E2_GovRegNum = "400000002";

				AssertEquals("47", FDA.FDAImporterNumber.ID);
				AssertEquals("400000002", FDA.FDAImporterNumber.Number);
			}
		}

		public void TestProducerNumber()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNoExceptionThrown(() => { _ = FDA.ProducerNumber; });

				FDA.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
				var docAddress = FDA.InitialImporterDocAddress ?? FDA.DocAddresses.CreateWithAddressType(DocAddressType.InitialImporter);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				docAddress.E2_GovRegNum = "500000001";

				AssertEquals("16", FDA.ProducerNumber.ID);
				AssertEquals("500000001", FDA.ProducerNumber.Number);

				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
				docAddress.E2_GovRegNum = "500000002";

				AssertEquals("47", FDA.ProducerNumber.ID);
				AssertEquals("500000002", FDA.ProducerNumber.Number);
			}
		}

		public void TestOwnerNumber()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var docAddress = FDA.GoodsOwnerDocAddress ?? FDA.DocAddresses.CreateWithAddressType(DocAddressType.GoodsOwner);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				docAddress.E2_GovRegNum = "600000001";

				AssertEquals("16", FDA.OwnerNumber.ID);
				AssertEquals("600000001", FDA.OwnerNumber.Number);

				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
				docAddress.E2_GovRegNum = "600000002";

				AssertEquals("47", FDA.OwnerNumber.ID);
				AssertEquals("600000002", FDA.OwnerNumber.Number);
			}
		}

		public void TestLocationOfGoodsNumber()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var docAddress = FDA.LocationOfGoodsDocAddress ?? FDA.DocAddresses.CreateWithAddressType(DocAddressType.GoodsLocation);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				docAddress.E2_GovRegNum = "700000001";

				AssertEquals("16", FDA.LocationOfGoodsNumber.ID);
				AssertEquals("700000001", FDA.LocationOfGoodsNumber.Number);

				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
				docAddress.E2_GovRegNum = "700000002";

				AssertEquals("47", FDA.LocationOfGoodsNumber.ID);
				AssertEquals("700000002", FDA.LocationOfGoodsNumber.Number);
			}
		}

		public void TestShipperNumber()
		{
			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var docAddress = FDA.ShipperDocAddress ?? FDA.DocAddresses.CreateWithAddressType(DocAddressType.Shipper);
				docAddress.E2_AddressOverride = true;
				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				docAddress.E2_GovRegNum = "800000001";

				AssertEquals("16", FDA.ShipperNumber.ID);
				AssertEquals("800000001", FDA.ShipperNumber.Number);

				docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
				docAddress.E2_GovRegNum = "800000002";

				AssertEquals("47", FDA.ShipperNumber.ID);
				AssertEquals("800000002", FDA.ShipperNumber.Number);
			}
		}

		public void TestGoodsFromFTZ()
		{
			var declaration = FDA.InvoiceLine.Declaration;
			declaration.US_GoodsFromFTZ = "B815";
			AssertEquals(ZString.Empty, ((IFDAData)FDA).GoodsFromFTZ);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals("B815", ((IFDAData)FDA).GoodsFromFTZ);
		}

		public void TestIUnitConverterDataProviderType()
		{
			var unitConverter = new ACEFDA.UnitConverterDataProviderWithoutProduct(FDA.InvoiceLine);
			AssertEquals("Pack Conversion Type should be Commercial Invoice", RPTypeList.Codes.CommercialInvoice, ((IUnitConverterDataProvider)unitConverter).Type);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return FDA;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine.ACE_FDALines.AddNew();
		}

		ACEFDA FDA
		{
			get
			{
				if (fda == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableCRL = true;

					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					fda = invoiceLine.ACE_FDALines.AddNew();
				}
				return fda;
			}
		}
		ACEFDA fda;
	}
}
