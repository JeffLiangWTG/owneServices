using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101GoodsShipmentGovernmentAgencyGoodsItemOnlyMandatoryTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNX101GoodsShipmentGovernmentAgencyGoodsItemProperties()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			invoiceLine.JI_OriginCriteria = "A";
			invoiceLine.JI_PTCriteria = "B";
			invoiceLine.JI_ManufacturerRelationship = "M";
			invoiceLine.JI_PTCriteria2 = "B2";
			invoiceLine.JI_PermitQty = 100M;
			invoiceLine.JI_PermitUQ = "KG";
			invoiceLine.JI_CustomPermitUQ = "SET";
			var header = entryInstruction.ControllingMessageHeaders.AddNew();

			IGovernmentAgencyGoodsItem governmentAgencyGoodsItem = new NX101GoodsShipmentGovernmentAgencyGoodsItemOnlyMandatory(1, header, invoiceLine, "");
			var goodsMeasure = governmentAgencyGoodsItem.GoodsMeasure;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.CriteriaCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "CriteriaCode");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.PreferentialCriteria, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "PreferentialCriteria");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.ProducerCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "ProducerCode");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.OtherCriteria, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "OtherCriteria");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Packaging, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPackaging)), "Packaging - should be [null]");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Manufacturer, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)), "Manufacturer - should be [null]");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.AdditionalDeclaration, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalDeclaration)), "AdditionalDeclaration - should be [null]");
				NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Commodity, NUnit.Framework.Is.TypeOf<NX101GoodsShipmentGovernmentAgencyGoodsItemCommodityOnlyMandatory>());
				NUnit.Framework.Assert.That(goodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "goodsMeasure.TariffQuantity");
				NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("KG").Using(CustomComparers.TypeComparison), "goodsMeasure.UnitCode");
				NUnit.Framework.Assert.That(goodsMeasure.CustomUnitCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "goodsMeasure.CustomUnitCode");
			});
		}
	}
}
