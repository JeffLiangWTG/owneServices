using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business.Testing
{
	class QuotaCustomsUnitDefaultingStrategyTest : CustomsUnitDefaultingStrategyTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new QuotaCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(null));
		}

		public override void TestDefaultUOMs()
		{
			var refCusQuota = Factory.New<RefCusQuota>();
			refCusQuota.ZXQ_UnitOfMeasure = "KG";
			var orderNumber = "123";
			var strategy = new QuotaCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(
				line => line.JI_ConcessionOrder == orderNumber ? refCusQuota : null,
				(unitQty, list, countryCode, factory) => list.Contains("GR"));
			strategy.Initialise(InvoiceLine);

			CombineAssertions(() =>
			{
				InvoiceLine.JI_CustomsUnitQty = ZString.Empty;
				InvoiceLine.JI_ConcessionOrder = "321";
				AssertEquals("Different JI_ConcessionOrder", string.Empty, InvoiceLine.JI_CustomsUnitQty);
				InvoiceLine.JI_ConcessionOrder = orderNumber;
				AssertEquals("JI_CustomsUnitQty was empty", "KG", InvoiceLine.JI_CustomsUnitQty);
				InvoiceLine.JI_ConcessionOrder = ZString.Empty;
				InvoiceLine.JI_CustomsUnitQty = "L";
				InvoiceLine.JI_ConcessionOrder = orderNumber;
				AssertEquals("JI_CustomsUnitQty was not convertible", "KG", InvoiceLine.JI_CustomsSecondUnitQty);
				InvoiceLine.JI_ConcessionOrder = ZString.Empty;
				InvoiceLine.JI_CustomsSecondUnitQty = "L";
				InvoiceLine.JI_ConcessionOrder = orderNumber;
				AssertEquals("JI_CustomsUnitQty and JI_CustomsSecondUnitQty was not convertible", "KG", InvoiceLine.JI_CustomsThirdUnitQty);
				InvoiceLine.JI_ConcessionOrder = ZString.Empty;
				InvoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
				InvoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
				InvoiceLine.JI_CustomsUnitQty = "KG";
				InvoiceLine.JI_ConcessionOrder = orderNumber;
				AssertEquals("JI_CustomsUnitQty was kilogram", ZString.Empty, InvoiceLine.JI_CustomsSecondUnitQty);
				InvoiceLine.JI_ConcessionOrder = ZString.Empty;
				InvoiceLine.JI_CustomsUnitQty = "GR";
				InvoiceLine.JI_CustomsSecondUnitQty = "GR";
				InvoiceLine.JI_ConcessionOrder = orderNumber;
				AssertEquals("JI_CustomsUnitQty and JI_CustomsSecondUnitQty was convertible", ZString.Empty, InvoiceLine.JI_CustomsThirdUnitQty);
			});
		}

		public void TestDefaultUOMs_AdditionalCustomsUnitQtyInfos()
		{
			var refCusQuota = Factory.New<RefCusQuota>();
			refCusQuota.ZXQ_UnitOfMeasure = "KG";
			var orderNumber = "123";
			var strategy = new QuotaCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(
				line => line.JI_ConcessionOrder == orderNumber ? refCusQuota : null,
				(unitQty, list, countryCode, factory) => list.Contains("KG") || list.Contains("GR"));
			var strategyWithAdditionalCustomsUnitQtyInfos = new QuotaCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(
				line => line.JI_ConcessionOrder == orderNumber ? refCusQuota : null,
				(unitQty, list, countryCode, factory) => list.Contains("KG") || list.Contains("GR"), null,
				new[] { InvoiceLine.JI_CustomsFourthUnitQtyInfo });
			strategy.Initialise(InvoiceLine);

			CombineAssertions(() =>
			{
				InvoiceLine.JI_CustomsUnitQty = "L";
				InvoiceLine.JI_CustomsSecondUnitQty = "L";
				InvoiceLine.JI_CustomsThirdUnitQty = "L";
				InvoiceLine.JI_ConcessionOrder = orderNumber;
				AssertEquals("No additionalCustomsUnitQtyInfos", string.Empty, InvoiceLine.JI_CustomsFourthUnitQty);
				InvoiceLine.JI_ConcessionOrder = ZString.Empty;
				strategy.Deinitialise(InvoiceLine);
				strategyWithAdditionalCustomsUnitQtyInfos.Initialise(InvoiceLine);
				InvoiceLine.JI_ConcessionOrder = orderNumber;
				AssertEquals("With additionalCustomsUnitQtyInfos", "KG", InvoiceLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestDefaultUOMs_AdditionalValueChangedInfos()
		{
			var refCusQuota = Factory.New<RefCusQuota>();
			refCusQuota.ZXQ_UnitOfMeasure = "KG";
			var orderNumber = "123";
			InvoiceLine.JI_ConcessionOrder = orderNumber;
			var strategy = new QuotaCustomsUnitDefaultingStrategy<BaseJobComInvoiceLine>(
				line => line.JI_ConcessionOrder == orderNumber ? refCusQuota : null,
				(unitQty, list, countryCode, factory) => list.Contains("KG") || list.Contains("GR"),
				new[] { InvoiceLine.JI_LinePriceInfo });
			strategy.Initialise(InvoiceLine);

			CombineAssertions(() =>
			{
				AssertEquals("Before change", string.Empty, InvoiceLine.JI_CustomsUnitQty);
				InvoiceLine.JI_LinePrice = 1M;
				AssertEquals("After change", "KG", InvoiceLine.JI_CustomsUnitQty);
			});
		}
	}
}
