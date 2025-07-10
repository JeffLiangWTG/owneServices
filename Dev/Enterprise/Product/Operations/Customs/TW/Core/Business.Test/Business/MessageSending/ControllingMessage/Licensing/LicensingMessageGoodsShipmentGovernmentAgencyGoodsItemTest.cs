using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem))]
	sealed class LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGoodsMeasure()
		{
			InvoiceLine.JI_NetWeight = 99000M;
			InvoiceLine.JI_NetWeightUQ = "G";
			InvoiceLine.JI_InvoiceQuantity = 88M;
			InvoiceLine.JI_InvoiceUQ = "UQ";
			InvoiceLine.JI_CustomsThirdQuantity = ZDecimal.Zero;
			InvoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;

			var goodsMeasure = GoodsShipmentGovernmentAgencyGoodsItem.GoodsMeasure;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsMeasure.NetWeightMeasure, NUnit.Framework.Is.EqualTo(99M).Using(CustomComparers.TypeComparison), "NetWeightMeasure");
				NUnit.Framework.Assert.That(goodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(88M).Using(CustomComparers.TypeComparison), "TariffQuantity when JI_CustomsThirdQuantity and JI_CustomsThirdUnitQty are Empty");
				NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("UQ").Using(CustomComparers.TypeComparison), "UnitCode when JI_CustomsThirdQuantity and JI_CustomsThirdUnitQty are Empty");

				InvoiceLine.JI_CustomsThirdUnitQty = "KG";
				goodsMeasure = GoodsShipmentGovernmentAgencyGoodsItem.GoodsMeasure;
				NUnit.Framework.Assert.That(goodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(88M).Using(CustomComparers.TypeComparison), "TariffQuantity when JI_CustomsThirdQuantity is Empty");
				NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("UQ").Using(CustomComparers.TypeComparison), "UnitCode when JI_CustomsThirdQuantity is Empty");

				InvoiceLine.JI_CustomsThirdQuantity = 100M;
				InvoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
				goodsMeasure = GoodsShipmentGovernmentAgencyGoodsItem.GoodsMeasure;
				NUnit.Framework.Assert.That(goodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(88M).Using(CustomComparers.TypeComparison), "TariffQuantity when JI_CustomsThirdUnitQty is Empty");
				NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("UQ").Using(CustomComparers.TypeComparison), "UnitCode when JI_CustomsThirdUnitQty is Empty");

				InvoiceLine.JI_CustomsThirdQuantity = 100M;
				InvoiceLine.JI_CustomsThirdUnitQty = "KG";
				goodsMeasure = GoodsShipmentGovernmentAgencyGoodsItem.GoodsMeasure;
				NUnit.Framework.Assert.That(goodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(100M).Using(CustomComparers.TypeComparison), "TariffQuantity");
				NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("KG").Using(CustomComparers.TypeComparison), "UnitCode");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsStatisticalMeasure()
		{
			NUnit.Framework.Assert.That(((IGovernmentAgencyGoodsItem)GoodsShipmentGovernmentAgencyGoodsItem).GoodsStatisticalMeasure, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IGoodsStatisticalMeasure)));
			InvoiceLine.JI_CustomsSecondQuantity = 10M;
			InvoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			NUnit.Framework.Assert.That(((IGovernmentAgencyGoodsItem)GoodsShipmentGovernmentAgencyGoodsItem).GoodsStatisticalMeasure, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IGoodsStatisticalMeasure)));

			InvoiceLine.JI_CustomsSecondQuantity = 0M;
			InvoiceLine.JI_CustomsSecondUnitQty = "KG";
			NUnit.Framework.Assert.That(((IGovernmentAgencyGoodsItem)GoodsShipmentGovernmentAgencyGoodsItem).GoodsStatisticalMeasure, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IGoodsStatisticalMeasure)));

			InvoiceLine.JI_CustomsSecondQuantity = 10M;
			var goodsStatisticalMeasure = ((IGovernmentAgencyGoodsItem)GoodsShipmentGovernmentAgencyGoodsItem).GoodsStatisticalMeasure;
			NUnit.Framework.Assert.That(goodsStatisticalMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(10M).Using(CustomComparers.TypeComparison), "TariffQuantity");
			NUnit.Framework.Assert.That(goodsStatisticalMeasure.StatisticalUnitCode, NUnit.Framework.Is.EqualTo("KG").Using(CustomComparers.TypeComparison), "StatisticalUnitCode");
		}

		[ExpectNoExceptions]
		public void TestApprovalDocument()
		{
			InvoiceLine.ExemptionCode = "E";
			InvoiceLine.TypeApprovalCertificateNo = "5555";
			InvoiceLine.TypeApprovalAuthorizedParty = "6666";
			InvoiceLine.TypeApprovalPartyIdentifier = "7777";
			CombineAssertions(() =>
			{
				var approvalDocument = ((IGovernmentAgencyGoodsItem)GoodsShipmentGovernmentAgencyGoodsItem).ApprovalDocument;
				NUnit.Framework.Assert.That(approvalDocument.LPCOExemptionCode, NUnit.Framework.Is.EqualTo("E").Using(CustomComparers.TypeComparison), "LPCOExemptionCode");
				NUnit.Framework.Assert.That(approvalDocument.LPCOID, NUnit.Framework.Is.EqualTo("5555").Using(CustomComparers.TypeComparison), "LPCOID");
				NUnit.Framework.Assert.That(approvalDocument.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("6666").Using(CustomComparers.TypeComparison), "LPCOAuthorizedParty.ID");
				NUnit.Framework.Assert.That(approvalDocument.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo("7777").Using(CustomComparers.TypeComparison), "LPCOAuthorizedParty.TypeCode");
			});
		}

		[ExpectNoExceptions]
		public void TestCommoditySpecification()
		{
			InvoiceLine.JI_Compositions = "X2";
			NUnit.Framework.Assert.That(((IGovernmentAgencyGoodsItem)GoodsShipmentGovernmentAgencyGoodsItem).CommoditySpecification.ElementDescription, NUnit.Framework.Is.EqualTo("X2").Using(CustomComparers.TypeComparison), "ElementDescription");
		}

		[ExpectNoExceptions]
		public void TestShippingIdentification()
		{
			var shippingIdentification1 = InvoiceLine.ShippingIdentificationDataCollection.AddNew();
			shippingIdentification1.TW_ManufacturedLotNo = "T015101";
			shippingIdentification1.TW_ExpirationDate = new ZDateTime(2025, 1, 1);
			shippingIdentification1.TW_ProductLotNoAmount = 10M;
			shippingIdentification1.TW_ManufacturedDate = new ZDateTime(2024, 1, 1);

			var shippingIdentification2 = InvoiceLine.ShippingIdentificationDataCollection.AddNew();
			shippingIdentification2.TW_ManufacturedLotNo = "T015102";
			shippingIdentification2.TW_ExpirationDate = new ZDateTime(2025, 1, 2);
			shippingIdentification2.TW_ProductLotNoAmount = 20M;
			shippingIdentification2.TW_ManufacturedDate = new ZDateTime(2024, 1, 2);

			var shippingIdentifications = ((IGovernmentAgencyGoodsItem)GoodsShipmentGovernmentAgencyGoodsItem).ShippingIdentifications.ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(shippingIdentifications.Length, NUnit.Framework.Is.EqualTo(2), "Should has 2 shippingIdentifications");
				NUnit.Framework.Assert.That(shippingIdentifications[0].LotNumberID, NUnit.Framework.Is.EqualTo("T015101").Using(CustomComparers.TypeComparison), "shippingIdentification[0].LotNumberID");
				NUnit.Framework.Assert.That(shippingIdentifications[0].ProductBestBeforeDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2025, 1, 1)), "shippingIdentification[0].ProductBestBeforeDateTime");
				NUnit.Framework.Assert.That(shippingIdentifications[0].ProductLotNumberAmount, NUnit.Framework.Is.EqualTo(10M).Using(CustomComparers.TypeComparison), "shippingIdentification[0].ProductLotNumberAmount");
				NUnit.Framework.Assert.That(shippingIdentifications[0].ProductManufacturedDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2024, 1, 1)), "shippingIdentification[0].ProductManufacturedDate");

				NUnit.Framework.Assert.That(shippingIdentifications[1].LotNumberID, NUnit.Framework.Is.EqualTo("T015102").Using(CustomComparers.TypeComparison), "shippingIdentification[1].LotNumberID");
				NUnit.Framework.Assert.That(shippingIdentifications[1].ProductBestBeforeDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2025, 1, 2)), "shippingIdentification[1].ProductBestBeforeDateTime");
				NUnit.Framework.Assert.That(shippingIdentifications[1].ProductLotNumberAmount, NUnit.Framework.Is.EqualTo(20M).Using(CustomComparers.TypeComparison), "shippingIdentification[1].ProductLotNumberAmount");
				NUnit.Framework.Assert.That(shippingIdentifications[1].ProductManufacturedDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2024, 1, 2)), "shippingIdentification[1].ProductManufacturedDate");
			});
		}

		[ExpectNoExceptions]
		public void TestOrigin()
		{
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Taiwan;
			NUnit.Framework.Assert.That(GoodsShipmentGovernmentAgencyGoodsItem.Origin.CountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Taiwan).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsLicensingStatisticalMeasure()
		{
			var goodsLicensingStatisticalMeasure = GoodsShipmentGovernmentAgencyGoodsItem.GoodsLicensingStatisticalMeasure;
			Header.TW1_ControllingAgency = ControllingAgencyList.Codes._20;
			InvoiceLine.JI_CustomsThirdQuantity = 405m;
			InvoiceLine.JI_CustomsThirdUnitQty = "PKG";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsLicensingStatisticalMeasure.LicensingQuantity, NUnit.Framework.Is.EqualTo(405m).Using(CustomComparers.TypeComparison), "LicensingQuantity");
				NUnit.Framework.Assert.That(goodsLicensingStatisticalMeasure.StatisticalUnitCode, NUnit.Framework.Is.EqualTo("PKG").Using(CustomComparers.TypeComparison), "StatisticalUnitCode");
				NUnit.Framework.Assert.That(goodsLicensingStatisticalMeasure.ResponsibleGovernmentAgency, NUnit.Framework.Is.EqualTo(ControllingAgencyList.Codes._20).Using(CustomComparers.TypeComparison), "ResponsibleGovernmentAgency");
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			var additionalInformations = GoodsShipmentGovernmentAgencyGoodsItem.AdditionalInformations;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(additionalInformations.Count(), NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(additionalInformations.Any(x => x.StatementCode == "A" && x.StatementDescription == "A1"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(additionalInformations.Any(x => x.StatementCode == "B" && x.StatementDescription == "B1"), NUnit.Framework.Is.True);
			});
		}

		[ExpectNoExceptions]
		public void TestPreBondedDocument()
		{
			InvoiceLine.PreviousBondedEntryNumber = "A123";
			InvoiceLine.PreviousBondedEntryLineNumber = 25;
			var doc = ((IGovernmentAgencyGoodsItem)GoodsShipmentGovernmentAgencyGoodsItem).PreBondedDocument;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(doc.ID, NUnit.Framework.Is.EqualTo("A123").Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(doc.LineNumeric, NUnit.Framework.Is.EqualTo(25).Using(CustomComparers.TypeComparison), "LineNumeric");
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocuments()
		{
			var docs = ((IGovernmentAgencyGoodsItem)GoodsShipmentGovernmentAgencyGoodsItem).AdditionalDocuments;
			NUnit.Framework.Assert.That(docs.Single().SequenceNumeric, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison), "tw_SequenceNumeric");
		}

		[ExpectNoExceptions]
		public void TestManufacturer()
		{
			var manufacturerDocAddress = InvoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			manufacturerDocAddress.FRICode = "96944490";
			IGovernmentAgencyGoodsItem iGovernmentAgencyGoodsItem = GoodsShipmentGovernmentAgencyGoodsItem;
			CombineAssertions(() =>
			{
				var iGovernmentAgencyGoodsItemManufacturer = iGovernmentAgencyGoodsItem.Manufacturer;
				NUnit.Framework.Assert.That(iGovernmentAgencyGoodsItemManufacturer, NUnit.Framework.Is.TypeOf<LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemManufacturer>());
				NUnit.Framework.Assert.That(iGovernmentAgencyGoodsItemManufacturer.ID, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison), "ID");
			});
		}

		[ExpectNoExceptions]
		public void TestMedicalInstrument()
		{
			IGovernmentAgencyGoodsItem iGovernmentAgencyGoodsItem = GoodsShipmentGovernmentAgencyGoodsItem;
			CombineAssertions(() =>
			{
				InvoiceLine.CertificateNo = "1121";
				InvoiceLine.AuthorizedPerson = "adsfas";
				InvoiceLine.PartyIdentifier = "121";
				NUnit.Framework.Assert.That(iGovernmentAgencyGoodsItem.MedicalInstrument, NUnit.Framework.Is.TypeOf<LPCODetailWrapper>());
				NUnit.Framework.Assert.That(iGovernmentAgencyGoodsItem.MedicalInstrument.LPCOID, NUnit.Framework.Is.EqualTo("1121").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(iGovernmentAgencyGoodsItem.MedicalInstrument.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("adsfas").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(iGovernmentAgencyGoodsItem.MedicalInstrument.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo("121").Using(CustomComparers.TypeComparison));
			});

			CombineAssertions(() =>
			{
				InvoiceLine.CertificateNo = "1121";
				InvoiceLine.AuthorizedPerson = "adsfas";
				InvoiceLine.PartyIdentifier = PartyIdentifierCodeList.Codes._53;
				NUnit.Framework.Assert.That(iGovernmentAgencyGoodsItem.MedicalInstrument.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("NOadsfas").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(iGovernmentAgencyGoodsItem.MedicalInstrument.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestPackaging()
		{
			var goodsItem = (IGovernmentAgencyGoodsItem)GoodsShipmentGovernmentAgencyGoodsItem;
			InvoiceLine.JI_PackagingQTY = 123;
			InvoiceLine.JI_PackagingUQ = "PKG";
			CombineAssertions(() =>
			{
				foreach (var messageType in new string[] { ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX401, ControllingMessageTypeList.Codes.NX603 })
				{
					Header.TW1_ControllingMessageType = messageType;
					var packaging = goodsItem.Packaging;
					NUnit.Framework.Assert.That(packaging.QuantityQuantity, NUnit.Framework.Is.EqualTo(123M).Using(CustomComparers.TypeComparison), $"[{messageType}]: QuantityQuantity");
					NUnit.Framework.Assert.That(packaging.TypeCode, NUnit.Framework.Is.EqualTo("PKG").Using(CustomComparers.TypeComparison), $"[{messageType}]: TypeCode");
				}
			});
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ??= Factory.NewWithValidTestData<JobDeclaration>();

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = (JobComInvoiceLine)Declaration.Invoices.AddNew().InvoiceLines.AddNew();
					invoiceLine.AssignCMHeaderToInvoices(Header);
					var reservedFields1 = invoiceLine.ReservedFields.AddNew();
					reservedFields1.CY_Code = "A";
					reservedFields1.CY_Data = "A1";
					var reservedFields2 = invoiceLine.ReservedFields.AddNew();
					reservedFields2.CY_Code = "B";
					reservedFields2.CY_Data = "B1";
				}
				return invoiceLine;
			}
		}

		CusTWControllingMessageHeader header;
		CusTWControllingMessageHeader Header => header ??= Declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();

		LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem goodsShipmentGovernmentAgencyGoodsItem;
		LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem GoodsShipmentGovernmentAgencyGoodsItem => goodsShipmentGovernmentAgencyGoodsItem ??= new LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(1, Header, InvoiceLine);
	}
}
