using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class BaseCustomsQuantityConverterTest : Customs.Business.Testing.BaseCustomsQuantityConverterTest
	{
		protected override ZDecimal ExpectedCustomsQuantityOfTestCalculateCustomsQuantityWhenPartIsThere => 0m;
		protected override ZDecimal ExpectedCustomsQuantityOfTestCountrySpecificPartConversion => 0m;
		protected override ZDecimal ExpectedVolumeOfTestCalculateCountrySpecificQuantity => 0m;

		protected abstract BaseCustomsQuantityConverter GetBaseCustomsQuantityConverter(JobComInvoiceLine invoiceLine);
		protected abstract ZPropertyInfo GetQuantityPropertyInfo(JobComInvoiceLine invoiceLine);
		protected abstract ZPropertyInfo GetUnitPropertyInfo(JobComInvoiceLine invoiceLine);
		public abstract void TestQuantityDecimalPlace();

		public void TestCalculateFromNetWeightToCustomsQtyCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var converter = GetBaseCustomsQuantityConverter(invoiceLine);
			var unitPropInfo = GetUnitPropertyInfo(invoiceLine);
			invoiceLine.JI_NetWeight = 1000m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;

			CombineAssertions(() =>
			{
				unitPropInfo.SetValueFromString("BBB");
				Assert("CalculateFromNetWeightToCustomsQtyCore() will not be invoked because BBB cannot be converted to a valid weight unit.", !invoiceLine.CanConvertFromNetWeightToCustomsUnit("BBB"));

				unitPropInfo.SetValueFromString(Core.Constants.Weight.Kilograms);
				AssertEquals("KG", 1000m, converter.CalculateFromNetWeightToCustomsQtyCore());

				unitPropInfo.SetValueFromString(Core.Constants.Weight.Tonnes);
				AssertEquals("T", 1m, converter.CalculateFromNetWeightToCustomsQtyCore());

				unitPropInfo.SetValueFromString("KGM");
				AssertEquals("KGM", 1000m, converter.CalculateFromNetWeightToCustomsQtyCore());

				unitPropInfo.SetValueFromString("TNE");
				AssertEquals("TNE", 1m, converter.CalculateFromNetWeightToCustomsQtyCore());
			});
		}

		public override void TestCalculateCustomsQuantityWhenNetWeightIsPresent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var converter = GetBaseCustomsQuantityConverter(invoiceLine);
			var qtyPropInfo = GetQuantityPropertyInfo(invoiceLine);
			var unitPropInfo = GetUnitPropertyInfo(invoiceLine);

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 1000m;

			CombineAssertions(() =>
			{
				unitPropInfo.SetValueFromString("BBB");
				converter.CalculateFromNetWeightToCustomsQty();
				AssertEquals("BBB", 0m, qtyPropInfo.Value);

				unitPropInfo.SetValueFromString(Core.Constants.Weight.Tonnes);
				converter.CalculateFromNetWeightToCustomsQty();
				AssertEquals("T", 1m, qtyPropInfo.Value);

				unitPropInfo.SetValueFromString(Core.Constants.Weight.Kilograms);
				converter.CalculateFromNetWeightToCustomsQty();
				AssertEquals("KG", 1000m, qtyPropInfo.Value);

				unitPropInfo.SetValueFromString("TNE");
				converter.CalculateFromNetWeightToCustomsQty();
				AssertEquals("TNE", 1m, qtyPropInfo.Value);

				unitPropInfo.SetValueFromString("KGM");
				converter.CalculateFromNetWeightToCustomsQty();
				AssertEquals("KGM", 1000m, qtyPropInfo.Value);
			});
		}
	}
}
