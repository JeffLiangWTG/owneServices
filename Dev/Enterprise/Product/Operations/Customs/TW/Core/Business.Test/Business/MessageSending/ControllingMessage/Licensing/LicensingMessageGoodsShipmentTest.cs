using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageGoodsShipment))]
	sealed class LicensingMessageGoodsShipmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			declaration.JE_ExportDate = new ZDateTime(2020, 1, 1);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalDocument>)), "AdditionalDocuments - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.TotalCIFAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero), "TotalCIFAmount");
				NUnit.Framework.Assert.That(goodsShipment.Consignment, NUnit.Framework.Is.TypeOf<LicensingMessageGoodsShipmentConsignment>(), "Consignment Type");
				NUnit.Framework.Assert.That(goodsShipment.CustomsValuation, NUnit.Framework.Is.EqualTo(default(ICustomsValuation)), "CustomsValuation - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.DeliveryDestinationName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "DeliveryDestinationName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(goodsShipment.DutyTaxFees, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IGoodsShipmentDutyTaxFee>)), "DutyTaxFees - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.TradeTermsConditionCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TradeTermsConditionCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(goodsShipment.UCR.ToString(), NUnit.Framework.Is.Null.Or.Empty, "UCR - should be [null] or [empty]");
				NUnit.Framework.Assert.That(goodsShipment.GoodsMeasures, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IGoodsMeasure>)), "GoodsMeasures - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)), "AdditionalInformations - should be [null]");
				NUnit.Framework.Assert.That(goodsShipment.ExitDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2020, 1, 1)), "ExitDateTime");
			});

			CombineAssertions("Consignor", () =>
			{
				var consignor = goodsShipment.Consignor;
				NUnit.Framework.Assert.That(consignor.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.ChineseName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ChineseName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.TypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.CustomsControlID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CustomsControlID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.PaymentOnAccountBusinessID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "PaymentOnAccountBusinessID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.RoleCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "RoleCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.SubBoxID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "SubBoxID - should be [null] or [empty]");
				var address = consignor.Address;
				NUnit.Framework.Assert.That(address.Line.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address Line - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.ChineseLine.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address ChineseLine - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address CountryCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.CountrySubDivisionID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address CountrySubDivisionID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.CountrySubDivisionName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address CountrySubDivisionName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(ILPCOAuthorizedParty)), "LPCOAuthorizedParty - should be [null]");
				NUnit.Framework.Assert.That(consignor.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ICommunication>)), "Communications - should be [null]");
				NUnit.Framework.Assert.That(consignor.ContactName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ContactName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.OwnerName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "OwnerName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.MainManufacturer.ToString(), NUnit.Framework.Is.Null.Or.Empty, "MainManufacturer - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.UndertakeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "UndertakeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignor.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)), "AdditionalInformations - should be [null]");
			});

			CombineAssertions("NotifyParty", () =>
			{
				var notifyParty = goodsShipment.NotifyParty;
				NUnit.Framework.Assert.That(notifyParty.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.ChineseName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ChineseName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.TypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.CustomsControlID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CustomsControlID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.PaymentOnAccountBusinessID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "PaymentOnAccountBusinessID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.RoleCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "RoleCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.SubBoxID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "SubBoxID - should be [null] or [empty]");
				var address = notifyParty.Address;
				NUnit.Framework.Assert.That(address.Line.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address Line - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.ChineseLine.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address ChineseLine - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address CountryCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.CountrySubDivisionID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address CountrySubDivisionID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.CountrySubDivisionName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address CountrySubDivisionName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(ILPCOAuthorizedParty)), "LPCOAuthorizedParty - should be [null]");
				NUnit.Framework.Assert.That(notifyParty.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ICommunication>)), "Communications - should be [null]");
				NUnit.Framework.Assert.That(notifyParty.ContactName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ContactName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.OwnerName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "OwnerName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.MainManufacturer.ToString(), NUnit.Framework.Is.Null.Or.Empty, "MainManufacturer - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.UndertakeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "UndertakeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(notifyParty.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)), "AdditionalInformations - should be [null]");
			});

			CombineAssertions("Exporter", () =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var address = orgHeader.Addresses.AddNew();
				address.CompanyName = "Exporter Company Name";
				var supplierDocAddress = header.SupplierDocumentaryAddress;
				supplierDocAddress.OrganisationPK = orgHeader.PK;
				supplierDocAddress.E2_OA_Address = address.PK;

				NUnit.Framework.Assert.That(goodsShipment.Exporter, NUnit.Framework.Is.TypeOf<LicensingMessagePartyDetailsWrapper>(), "Exporter Type");
				NUnit.Framework.Assert.That(goodsShipment.Exporter.Name, NUnit.Framework.Is.EqualTo("Exporter Company Name").Using(CustomComparers.TypeComparison), "Exporter is referenced to CusTWControllingMessageHeader.SupplierDocumentaryAddress");
			});
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestItemChargeAmount()
		{
			var importEntryHeader = CusEntryHeaderTest.GetImportFobEntryHeaderTestCase(Factory);
			var importDeclaration = importEntryHeader.Declaration;
			var importHeader = importDeclaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			IGoodsShipment goodsShipment = new LicensingMessageGoodsShipment(importHeader);
			NUnit.Framework.Assert.That(goodsShipment.ItemChargeAmount, NUnit.Framework.Is.EqualTo(16000m).Using(CustomComparers.TypeComparison), "ItemChargeAmount");
		}

		[ExpectNoExceptions]
		public void TestBuyer()
		{
			NUnit.Framework.Assert.That(goodsShipment.Buyer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)), "Buyer should be null when not overrided - should be [null]");

			var buyerAddress = header.ImporterDocumentaryAddress;
			buyerAddress.E2_AddressOverride = true;
			NUnit.Framework.Assert.That(goodsShipment.Buyer, NUnit.Framework.Is.TypeOf<Buyer>());
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

			header.Declaration.JE_OH_Consignee = consigneeOrgHeader.PK;
			CombineAssertions(() =>
			{
				var consignee = goodsShipment.Consignee;
				NUnit.Framework.Assert.That(consignee.Name, NUnit.Framework.Is.EqualTo("Consignee Company").Using(CustomComparers.TypeComparison), "Name is not expected");
				NUnit.Framework.Assert.That(consignee.Address.Line, NUnit.Framework.Is.EqualTo("54321 ADDRESS LINE1. 54321 ADDRESS LINE2. TAIPEI TAIWAN").Using(CustomComparers.TypeComparison), "Address.Line is not expected");
				NUnit.Framework.Assert.That(consignee.Address.CountryCode, NUnit.Framework.Is.EqualTo("TW").Using(CustomComparers.TypeComparison), "Address.CountryCode is not expected");
			});
		}

		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItems()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLines = invoice.InvoiceLines;
			var invoiceLine1 = (JobComInvoiceLine)invoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoiceLines.AddNew();
			invoiceLine1.AssignCMHeaderToInvoices(header);
			invoiceLine2.AssignCMHeaderToInvoices(header);
			goodsShipment = new LicensingMessageGoodsShipment(header);
			var governmentAgencyGoodsItems = goodsShipment.GovernmentAgencyGoodsItems;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(governmentAgencyGoodsItems.Count(), NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(governmentAgencyGoodsItems.ElementAt(0), NUnit.Framework.Is.TypeOf<LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem>());
				NUnit.Framework.Assert.That(governmentAgencyGoodsItems.ElementAt(1), NUnit.Framework.Is.TypeOf<LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem>());
			});
		}

		[ExpectNoExceptions]
		public void TestSeller()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			goodsShipment = new LicensingMessageGoodsShipment(header);
			NUnit.Framework.Assert.That(goodsShipment.Seller, NUnit.Framework.Is.TypeOf<LicensingMessagePartyDetailsWrapper>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			goodsShipment = new LicensingMessageGoodsShipment(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		IGoodsShipment goodsShipment;
	}
}
