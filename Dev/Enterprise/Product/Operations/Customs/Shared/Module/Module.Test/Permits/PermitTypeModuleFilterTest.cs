using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(PermitTypeModuleFilter))]
	public class PermitTypeModuleFilterTest : ModuleFilterTestCase<PermitTypeModuleFilter>
	{
		public void TestConstructer_NullParameter()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PermitTypeModuleFilter("null GetList", (val0, val1, val2) => new ZQuery(), null));
		}

		public void TestPermitTypes()
		{
			var tester = new PermitTypeModuleFilterForTest("DESC", (val0, val1, val2) => new ZQuery());
			AssertEquals(2, tester.PermitTypes.Count);
			Assert(tester.PermitTypes.ContainsCode("IMP"));
		}

		public void TestPermitSubTypes()
		{
			var tester = new PermitTypeModuleFilterForTest("DESC", (val0, val1, val2) => new ZQuery());
			AssertEquals(1, tester.PermitSubTypes.Count);
			Assert(((CodeDescriptionPairList)tester.PermitSubTypes).ContainsCode("LVE"));
		}

		public void TestAdditionalValidationOnCodes()
		{
			var tester = new DummyPermitTypeModuleFilter("DESC", (val0, val1, val2) => new ZQuery());
			tester.Property1 = "EXP";
			tester.Property2 = "";
			AssertNoNotifications(tester.Property1Info);
			AssertNoNotifications(tester.Property2Info);
			tester.Property2 = "LVE";
			AssertNoNotifications(tester.Property1Info);
			AssertNoNotifications(tester.Property2Info);
			tester.Property1 = "IMP";
			tester.Property2 = "";
			AssertNoNotifications(tester.Property1Info);
			AssertNoNotifications(tester.Property2Info);
			tester.Property2 = "XX";
			AssertNoNotifications(tester.Property1Info);
			AssertHasWarningOfInvalidCodeOnly(tester.Property2Info);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override PermitTypeModuleFilter GetNewModuleFilter() => new PermitTypeModuleFilterForTest("moo", (val0, val1, val2) => new ZQuery());

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.ModesAndTypes;

		static void AssertHasWarningOfInvalidCodeOnly(ZPropertyInfo info)
		{
			AssertNoErrors(info);
			AssertNoMessageErrors(info);
			AssertHasWarningContaining(info, ListValidation.InvalidCodeMessage);
		}

		sealed class DummyPermitTypeModuleFilter : PermitTypeModuleFilterForTest
		{
			public DummyPermitTypeModuleFilter(ZString description, GetPermitTypeQuery queryDelegate) : base(description, queryDelegate)
			{
			}

			protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			{
				return new DummyPermitTypeModuleFilter("moo", (val0, val1, val2) => new ZQuery());
			}

			protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
			{
				var filter = (DummyPermitTypeModuleFilter)filterToCopyFrom;
				Property1 = filter.Property1;
				Property2 = filter.Property2;
			}
		}
	}
}
