using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(UNDGSubstanceCFR))]
	sealed class UNDGSubstanceCFRTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<UNDGSubstanceCFR>();
		}

		public void TestCFR_Calc_ReportableKiloQuantity()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstanceCFR>();
			substance.CFR_ReportableQuantity = 10.00;
			substance.CFR_ReportableQuantityUnit = "lb";
			AssertEquals(new ZDecimal(4.535924), substance.CFR_Calc_ReportableKiloQuantity);

			substance.CFR_ReportableQuantity = 0.00;
			substance.CFR_ReportableQuantityUnit = "lb";
			AssertEquals(new ZDecimal(0.00), substance.CFR_Calc_ReportableKiloQuantity);

			substance.CFR_ReportableQuantity = 1000.00;
			substance.CFR_ReportableQuantityUnit = "g";
			AssertEquals(new ZDecimal(1.00), substance.CFR_Calc_ReportableKiloQuantity);

			substance.CFR_ReportableQuantity = 1.00;
			substance.CFR_ReportableQuantityUnit = "dt";
			AssertEquals(new ZDecimal(100.00), substance.CFR_Calc_ReportableKiloQuantity);

			substance.CFR_ReportableQuantity = 2.00;
			substance.CFR_ReportableQuantityUnit = "T";
			AssertEquals(new ZDecimal(2000.00), substance.CFR_Calc_ReportableKiloQuantity);
		}

		public void TestCFR_Calc_ReportableKiloQuantityUnit()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstanceCFR>();
			AssertEquals("kg", substance.CFR_Calc_ReportableKiloQuantityUnit);
		}
	}
}
