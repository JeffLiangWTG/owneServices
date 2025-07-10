using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(ModuleSingleLocationFilter))]
	public class ModuleSingleLocationFilterTest : ModuleFilterTestCase<ModuleSingleLocationFilter>
	{
		public void TestIncludesInternationalZonesWhenTypeIsIncluded()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "AUSR";
			dummy.Z0_Description = "International Zone";
			Factory.Save();

			Filter.Property = "AUSR";
			AssertCollection(dummy, true, "Should match by International Zone Code");

			Filter.Property = "AUMEL";
			AssertCollection(dummy, true, "Should find Melbourne in the Australian Zone");
		}

		public void TestLocationCanFilterOn2LetterCountryCode()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "AUMEL";
			dummy.Z0_Description = "Melbourne UNLOCO";
			Factory.Save();

			Filter.Property = "AUMEL";
			AssertCollection(dummy, true);

			Filter.Property = "AU";
			AssertCollection(dummy, true);
		}

		void AssertCollection(DummyBusinessObject dummy, bool isExpected, string message = "")
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(Filter.Query);

			if (isExpected)
			{
				AssertCollectionContains(message, dummy, collection);
			}
			else
			{
				AssertCollectionNotContains(message, dummy, collection);
			}
		}

		protected override FilterCategory ExpectedDefaultCategory =>
			FilterCategories.Locations;

		protected override ZString ExpectedDescription => "boo";

		protected override ModuleSingleLocationFilter GetNewModuleFilter() =>
			new ModuleSingleLocationFilter(
				"boo",
				(NoResString)"boo",
				DummyBizoSchema.Z0_Code,
				DummyModuleIDs.Dummy,
				new RatingLocationCollection(Factory));

		public override void TestIsExpensiveQuery() =>
			Assert(true);
	}
}
