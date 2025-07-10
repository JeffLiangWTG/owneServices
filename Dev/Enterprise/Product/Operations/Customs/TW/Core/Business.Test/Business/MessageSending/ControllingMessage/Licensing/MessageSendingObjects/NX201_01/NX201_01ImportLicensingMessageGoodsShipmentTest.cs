using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_01ImportLicensingMessageGoodsShipment))]
	sealed class NX201_01ImportLicensingMessageGoodsShipmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBuyer()
		{
			var importEntryHeader = CusEntryHeaderTest.GetImportFobEntryHeaderTestCase(Factory);
			var importDeclaration = importEntryHeader.Declaration;
			var importHeader = importDeclaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var buyerAddress = importHeader.ImporterDocumentaryAddress;
			buyerAddress.E2_AddressOverride = true;
			IGoodsShipment goodsShipment = new NX201_01ImportLicensingMessageGoodsShipment(importHeader);
			NUnit.Framework.Assert.That(goodsShipment.Buyer, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)), "Should be null - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestConsignee()
		{
			var consigneeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
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
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			IGoodsShipment goodsShipment = new NX201_01ImportLicensingMessageGoodsShipment(header);
			header.Declaration.JE_OH_Consignee = consigneeOrgHeader.PK;
			NUnit.Framework.Assert.That(goodsShipment.Consignee, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)), "Should be null - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItemType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			IGoodsShipment goodsShipment = new NX201_01ImportLicensingMessageGoodsShipment(header);
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.First(), NUnit.Framework.Is.TypeOf<NX201_01LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem>());
		}
	}
}
