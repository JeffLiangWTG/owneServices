using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	using Enterprise.Core;
	class InvoiceLineMeasurementGetterTest : TestCaseWithFactory
	{
		public void TestFallbackOrder()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			InvoiceLineMeasurementGetter measurementGetter = new InvoiceLineMeasurementGetter(new MeasurementUQList());
			AssertEquals("TryGetMeasurementWithFallback", false, measurementGetter.TryGetMeasurementWithFallback(invoiceLine, out var value, out var unit));
			AssertEquals("out value", 0, value);
			AssertEquals("out unit", string.Empty, unit);

			invoiceLine.JI_Volume = 34;
			invoiceLine.JI_VolumeUQ = Constants.Volume.CubicMetres;
			AssertEquals("TryGetMeasurementWithFallback", true, measurementGetter.TryGetMeasurementWithFallback(invoiceLine, out value, out unit));
			AssertEquals("out value", 34, value);
			AssertEquals("out unit", MeasurementUQList.Codes.cubicMetres, unit);

			invoiceLine.JI_Weight = 123;
			invoiceLine.JI_WeightUQ = Constants.Weight.Kilograms;
			AssertEquals("TryGetMeasurementWithFallback", true, measurementGetter.TryGetMeasurementWithFallback(invoiceLine, out value, out unit));
			AssertEquals("out value", 123, value);
			AssertEquals("out unit", MeasurementUQList.Codes.kilograms, unit);

			invoiceLine.JI_CustomsQuantity = 17;
			invoiceLine.JI_CustomsUnitQty = StatisticalUQList.Codes.Litres;
			AssertEquals("TryGetMeasurementWithFallback", true, measurementGetter.TryGetMeasurementWithFallback(invoiceLine, out value, out unit));
			AssertEquals("out value", 17, value);
			AssertEquals("out unit", MeasurementUQList.Codes.litres, unit);

			invoiceLine.JI_MAF_MeasurementValue = 43;
			invoiceLine.JI_MAF_MeasurementUQ = MeasurementUQList.Codes.egg;
			AssertEquals("TryGetMeasurementWithFallback", true, measurementGetter.TryGetMeasurementWithFallback(invoiceLine, out value, out unit));
			AssertEquals("out value", 43, value);
			AssertEquals("out unit", MeasurementUQList.Codes.egg, unit);
		}
	}
}
