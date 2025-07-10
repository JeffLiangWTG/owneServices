
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	[TestedType(typeof(NonDependentNZCClassificationCollection))]
	public class NonDependentNZCClassificationCollectionTest : BusinessObjectCollectionTestCase
	{
		#region TestDateRange
		public void TestDateRange()
		{
			NonDependentNZCClassificationCollection classifications = new NonDependentNZCClassificationCollection(Factory);

			classifications.Load("3302.10.79.00B");
			AssertEquals("Classifications.DateRange.DateActiveFrom", new ZDateTime(2000, 12, 7), classifications.DateRange.DateActiveFrom);
			AssertEquals("Classifications.DateRange.DateActiveTo", new ZDateTime(3000, 12, 31), classifications.DateRange.DateActiveTo);

			classifications.Load("6110.11.09.11J");
			AssertEquals("Classifications.DateRange.DateActiveFrom", new ZDateTime(2002, 1, 7), classifications.DateRange.DateActiveFrom);
			AssertEquals("Classifications.DateRange.DateActiveTo", new ZDateTime(2005, 6, 30), classifications.DateRange.DateActiveTo);

			classifications.Load("2009.49.00.11G");
			AssertEquals("Classifications.DateRange.DateActiveFrom", new ZDateTime(2002, 1, 1), classifications.DateRange.DateActiveFrom);
			AssertEquals("Classifications.DateRange.DateActiveTo", new ZDateTime(3000, 12, 31), classifications.DateRange.DateActiveTo);
		}
		#endregion

		#region TestLoadPassingTariffCode
		public void TestLoadPassingTariffCode()
		{
			NonDependentNZCClassificationCollection classifications = new NonDependentNZCClassificationCollection(Factory);
			classifications.Load("0000.00.00.00K");
			AssertEquals("Classifications.Count", 0, classifications.Count);
			NZCClassification classification = NZCClassification.New(Factory);
			classification.U0_Tariff = "0000.00.00.00K";
			classifications.Load("0000.00.00.00K");
			AssertEquals("Classifications.Count", 1, classifications.Count);
		}
		#endregion

		#region GetCollectionToTest
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NonDependentNZCClassificationCollection(Factory);
		}
		#endregion
	}
}
