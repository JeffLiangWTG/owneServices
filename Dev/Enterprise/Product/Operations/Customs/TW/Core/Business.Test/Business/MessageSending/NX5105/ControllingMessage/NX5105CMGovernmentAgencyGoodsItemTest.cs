using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105CMGovernmentAgencyGoodsItemTest : NX5105GoodsShipment_GovernmentAgencyGoodsItemAbstractTest<NX5105CMGovernmentAgencyGoodsItem, NX5105CMManufacturerWrapper>
	{
		[ExpectNoExceptions]
		public override void TestCheckNotApplicableProperties()
		{
			InvoiceLine.JI_CEI = EntryInstruction.PK;
			var item = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			TestTWCreator.AddInvoiceLineReservedFields(InvoiceLine as JobComInvoiceLine);
			NUnit.Framework.Assert.That(item.AdditionalInformations, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)));
			SharedHelperTest.AssertAdditionalInformations(item.AdditionalInformations);
			NUnit.Framework.Assert.That(item.CommoditySpecification, NUnit.Framework.Is.EqualTo(default(ICommoditySpecification)));
			NUnit.Framework.Assert.That(item.ControlInspectionStartDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			NUnit.Framework.Assert.That(item.ExaminationPlace, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.TransportEquipments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ITransportEquipment>)));
			NUnit.Framework.Assert.That(item.AdditionalDeclaration, NUnit.Framework.Is.EqualTo(default(IAdditionalDeclaration)));
		}

		[ExpectNoExceptions]
		public override void TestGoodsLicensingStatisticalMeasure()
		{
			InvoiceLine.JI_CEI = EntryInstruction.PK;
			InvoiceLine.JI_CustomsThirdQuantity = 999.9m;
			InvoiceLine.JI_CustomsThirdUnitQty = "KGM";
			var goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			var goodsLicensingStatisticalMeasure = goodItem.GoodsLicensingStatisticalMeasure;
			NUnit.Framework.Assert.That(goodsLicensingStatisticalMeasure.LicensingQuantity, NUnit.Framework.Is.EqualTo(999.9m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(goodsLicensingStatisticalMeasure.StatisticalUnitCode, NUnit.Framework.Is.EqualTo("KGM").Using(CustomComparers.TypeComparison));
			InvoiceLine.JI_CustomsThirdQuantity = ZDecimal.Zero;
			InvoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
			NUnit.Framework.Assert.That(goodItem.Manufacturer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public override void TestGovernmentAgencyGoodsItem_Commodity()
		{
			InvoiceLine.JI_CEI = EntryInstruction.PK;
			var goodItem = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			NUnit.Framework.Assert.That(goodItem.Commodity, NUnit.Framework.Is.TypeOf(typeof(NX5105CMCommodity)));
		}

		[ExpectNoExceptions]
		public void TestShippingIdentifications()
		{
			var invoice = EntryLine.InvoiceLines.AddNew(typeof(JobComInvoiceLine)) as JobComInvoiceLine;
			invoice.JI_CEI = EntryInstruction.PK;
			var shippingIdData = invoice.ShippingIdentificationDataCollection.AddNew();
			shippingIdData.TW_ManufacturedLotNo = "1234";
			shippingIdData.TW_ExpirationDate = new ZDateTime(2020, 05, 01);
			shippingIdData.TW_ProductLotNoAmount = 20m;
			shippingIdData.TW_ManufacturedDate = new ZDateTime(2020, 04, 01);
			var shippingIdData2 = invoice.ShippingIdentificationDataCollection.AddNew();
			shippingIdData2.TW_ManufacturedLotNo = "4567";
			shippingIdData2.TW_ExpirationDate = new ZDateTime(2020, 05, 02);
			shippingIdData2.TW_ProductLotNoAmount = 40m;
			shippingIdData2.TW_ManufacturedDate = new ZDateTime(2020, 04, 02);
			var item = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			NUnit.Framework.Assert.That(item.ShippingIdentifications, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<IShippingIdentification>)));
			var data = item.ShippingIdentifications.First();
			NUnit.Framework.Assert.That(data.LotNumberID, NUnit.Framework.Is.EqualTo("1234").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(data.ProductBestBeforeDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2020, 05, 01)));
			NUnit.Framework.Assert.That(data.ProductLotNumberAmount, NUnit.Framework.Is.EqualTo(20m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(data.ProductManufacturedDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2020, 04, 01)));
			data = item.ShippingIdentifications.Skip(1).First();
			NUnit.Framework.Assert.That(data.LotNumberID, NUnit.Framework.Is.EqualTo("4567").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(data.ProductBestBeforeDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2020, 05, 02)));
			NUnit.Framework.Assert.That(data.ProductLotNumberAmount, NUnit.Framework.Is.EqualTo(40m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(data.ProductManufacturedDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2020, 04, 02)));
		}

		[ExpectNoExceptions]
		public void TestApprovalDocument()
		{
			var invoice = EntryLine.InvoiceLines.AddNew(typeof(JobComInvoiceLine)) as JobComInvoiceLine;
			invoice.JI_CEI = EntryInstruction.PK;
			invoice.ExemptionCode = "1";
			invoice.TypeApprovalCertificateNo = "5555";
			invoice.TypeApprovalAuthorizedParty = "6666";
			invoice.TypeApprovalPartyIdentifier = "7777";
			var item = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			NUnit.Framework.Assert.That(item.ApprovalDocument, NUnit.Framework.Is.Not.EqualTo(default(ILPCODetail)));
			NUnit.Framework.Assert.That(item.ApprovalDocument.LPCOExemptionCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.ApprovalDocument.LPCOID, NUnit.Framework.Is.EqualTo("5555").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.ApprovalDocument.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("6666").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.ApprovalDocument.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo("7777").Using(CustomComparers.TypeComparison));
			invoice.ExemptionCode = ZString.Empty;
			invoice.TypeApprovalCertificateNo = ZString.Empty;
			invoice.TypeApprovalAuthorizedParty = ZString.Empty;
			invoice.TypeApprovalPartyIdentifier = ZString.Empty;
			item = GetGovernmentAgencyGoodsItem(EntryLine, EntryLine.RandomLine);
			NUnit.Framework.Assert.That(item.ApprovalDocument, NUnit.Framework.Is.Not.EqualTo(default(ILPCODetail)));
			NUnit.Framework.Assert.That(item.ApprovalDocument.LPCOExemptionCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(item.ApprovalDocument.LPCOID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(item.ApprovalDocument.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(item.ApprovalDocument.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestMedicalInstrument()
		{
			CombineAssertions(() =>
			{
				var entryLine = Factory.New<CusEntryLine>();
				var entryInstruction = Factory.New<CusEntryInstruction>();
				var invoiceLine = entryLine.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.CertificateNo = "1121";
				invoiceLine.AuthorizedPerson = "adsfas";
				invoiceLine.PartyIdentifier = "121";
				var goodItem = GetGovernmentAgencyGoodsItem(entryLine, entryLine.RandomLine);
				NUnit.Framework.Assert.That(goodItem.MedicalInstrument, NUnit.Framework.Is.TypeOf(typeof(LPCODetailWrapper)));
				NUnit.Framework.Assert.That(goodItem.MedicalInstrument.LPCOID, NUnit.Framework.Is.EqualTo("1121").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodItem.MedicalInstrument.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("adsfas").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodItem.MedicalInstrument.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo("121").Using(CustomComparers.TypeComparison));
			});

			CombineAssertions(() =>
			{
				var entryLine = Factory.New<CusEntryLine>();
				var entryInstruction = Factory.New<CusEntryInstruction>();
				var invoiceLine = entryLine.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.CertificateNo = "1121";
				invoiceLine.AuthorizedPerson = "adsfas";
				invoiceLine.PartyIdentifier = PartyIdentifierCodeList.Codes._53;
				var goodItem = GetGovernmentAgencyGoodsItem(entryLine, entryLine.RandomLine);
				NUnit.Framework.Assert.That(goodItem.MedicalInstrument.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("NOadsfas").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodItem.MedicalInstrument.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison));
			});
		}

		protected override IGovernmentAgencyGoodsItem GetGovernmentAgencyGoodsItem(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine)
		{
			return new NX5105CMGovernmentAgencyGoodsItem(cusEntryLine, invoiceLine);
		}
	}
}
