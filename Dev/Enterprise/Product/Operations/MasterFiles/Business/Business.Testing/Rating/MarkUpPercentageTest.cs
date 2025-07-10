using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Testing
{
	[TestedType(typeof(MarkUpPercentage))]
	sealed class MarkUpPercentageTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			MarkUpPercentagesCollection collection = new MarkUpPercentagesCollection(Factory);
			MarkUpPercentage bizO = new MarkUpPercentage(MarkUpPercentage.ALL, ZString.Empty, 0m, 0m, 0m, collection);
			return bizO;
		}

		public void TestMarkUpPercentage()
		{
			MarkUpPercentage elem = (MarkUpPercentage)GetNewBusinessObject();
			elem.Location = "AUSYD";
			elem.Percentage = 5.45m;
			AssertEquals("Location", "AUSYD", elem.Location);
			AssertEquals("Percentage", 5.45m, elem.Percentage);

			MarkUpPercentage elem2 = MarkUpPercentage.FromString(elem.ToString(), null);
			AssertEquals("Location", "AUSYD", elem2.Location);
			AssertEquals("Percentage", 5.45m, elem2.Percentage);

			Assert(elem.MinimumInfo.ReadOnly);
			Assert(elem.PerUnitInfo.ReadOnly);
			elem.Mode = "AIR";
			Assert(!elem.MinimumInfo.ReadOnly);
			Assert(!elem.PerUnitInfo.ReadOnly);
		}

		public void TestMarkUpPercentageValidation()
		{
			MarkUpPercentage elem = (MarkUpPercentage)GetNewBusinessObject();

			elem.Location = "AUSYD";
			elem.Mode = "AIR";
			elem.Percentage = 5.45m;
			elem.Minimum = 50m; 
			AssertHasError(elem.PercentageInfo, "You cannot specify a Percentage uplift if you have already specified a Minimum or Per Unit uplift.");
			Assert(!elem.IsValid);

			elem.Minimum = 0m;
			elem.PerUnit = 5m;
			AssertHasError(elem.PercentageInfo, "You cannot specify a Percentage uplift if you have already specified a Minimum or Per Unit uplift.");
			Assert(!elem.IsValid);

			elem.Percentage = 0m;
			elem.Minimum = 50m;
			elem.Percentage = 5.45m;
			AssertHasError(elem.PercentageInfo, "You cannot specify a Percentage uplift if you have already specified a Minimum or Per Unit uplift.");
			Assert(!elem.IsValid);

			elem.Minimum = 0m;
			elem.PerUnit = 5m;
			elem.Percentage = 6m;
			AssertHasError(elem.PercentageInfo, "You cannot specify a Percentage uplift if you have already specified a Minimum or Per Unit uplift.");
			Assert(!elem.IsValid);

			elem.Minimum = 0m;
			elem.PerUnit = 0m;
			elem.Percentage = 7m;
			AssertNoErrors(elem.PercentageInfo);
			Assert(elem.IsValid);

			elem.Minimum = 60m;
			elem.PerUnit = 4m;
			elem.Percentage = 0m;
			AssertNoErrors(elem.PercentageInfo);
			Assert(elem.IsValid);
		}
	}
}
