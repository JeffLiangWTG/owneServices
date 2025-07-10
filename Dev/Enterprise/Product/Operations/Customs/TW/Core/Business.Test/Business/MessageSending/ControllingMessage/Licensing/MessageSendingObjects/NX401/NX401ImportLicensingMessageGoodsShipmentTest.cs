using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401ImportLicensingMessageGoodsShipment))]
	sealed class NX401ImportLicensingMessageGoodsShipmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			IGoodsShipment goodsShipment = new NX401ImportLicensingMessageGoodsShipment(header);
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX401ImportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem>());
		}

		[ExpectNoExceptions]
		public void TestConsignee()
		{
			var consigneeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11111111", Core.Constants.CountryCodes.Taiwan);
			var consigneeAddress = consigneeOrgHeader.Addresses.AddNew();
			consigneeAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			consigneeAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			consigneeAddress.OA_IsActive = true;
			consigneeAddress.OA_Language = Core.SharedConstants.Languages.English;
			consigneeAddress.OA_CompanyNameOverride = "Consignee Company";
			consigneeAddress.OA_Address1 = "54321 address line1.";
			consigneeAddress.OA_Address2 = "54321 address line2.";
			consigneeAddress.OA_City = "TAIPEI";
			consigneeAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Consignee = consigneeOrgHeader.PK;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			CombineAssertions(() =>
			{
				IGoodsShipment goodsShipment = new NX401ImportLicensingMessageGoodsShipment(header);
				var consignee = goodsShipment.Consignee;
				NUnit.Framework.Assert.That(consignee.ID, NUnit.Framework.Is.EqualTo("11111111").Using(CustomComparers.TypeComparison), "ID is not expected");
				NUnit.Framework.Assert.That(consignee.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "TypeCode is not expected");
				NUnit.Framework.Assert.That(consignee.Name, NUnit.Framework.Is.EqualTo("Consignee Company").Using(CustomComparers.TypeComparison), "Name is not expected");
				NUnit.Framework.Assert.That(consignee.Address.Line, NUnit.Framework.Is.EqualTo("54321 ADDRESS LINE1. 54321 ADDRESS LINE2. TAIPEI TAIWAN").Using(CustomComparers.TypeComparison), "Address.Line is not expected");
				NUnit.Framework.Assert.That(consignee.Address.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison), "Address.CountryCode is not expected");
			});
		}

		[ExpectNoExceptions]
		public void TestItemChargeAmount()
		{
			var importEntryHeader = CusEntryHeaderTest.GetImportFobEntryHeaderTestCase(Factory);
			var importDeclaration = importEntryHeader.Declaration;
			var importHeader = importDeclaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			IGoodsShipment goodsShipment = new NX401ImportLicensingMessageGoodsShipment(importHeader);
			NUnit.Framework.Assert.That(goodsShipment.ItemChargeAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "Import");
		}

		[ExpectNoExceptions]
		public void TestExporter()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.CompanyName = "Exporter Company Name";
			var supplierDocAddress = header.SupplierDocumentaryAddress;
			supplierDocAddress.OrganisationPK = orgHeader.PK;
			supplierDocAddress.E2_OA_Address = address.PK;
			IGoodsShipment goodsShipment = new NX401ImportLicensingMessageGoodsShipment(header);
			NUnit.Framework.Assert.That(goodsShipment.Exporter, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)), "Exporter should be null when IMP - should be [null]");
		}
	}
}
