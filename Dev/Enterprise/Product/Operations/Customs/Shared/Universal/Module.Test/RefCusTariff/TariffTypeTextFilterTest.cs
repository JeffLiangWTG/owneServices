using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(TariffTypeTextFilter))]
	class TariffTypeTextFilterTest : ModuleTextFilterTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var testList = new CodeDescriptionPairList();
			testList.AddPair("1", "A");
			testList.AddPair("2", "B");
			return new TariffTypeTextFilter("TestDescription", (SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery(), testList);
		}

		public void TestValidation_OnlyWhenEqualNotEqual()
		{
			var tester = GetNewBusinessObject() as TariffTypeTextFilter;
			CombineAssertions(() =>
			{
				tester.SqlComparisonOperator = SQLComparisonOperator.Contains;
				tester.Property = "1";
				AssertNoNotifications(tester.PropertyInfo);
				tester.Property = "2";
				AssertNoNotifications(tester.PropertyInfo);
				tester.Property = "3";
				AssertNoNotifications(tester.PropertyInfo);
				tester.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				tester.Property = "1";
				AssertNoNotifications(tester.PropertyInfo);
				tester.Property = "2";
				AssertNoNotifications(tester.PropertyInfo);
				tester.Property = "3";
				AssertNoNotifications(tester.PropertyInfo);
				tester.SqlComparisonOperator = SQLComparisonOperator.Equal;
				tester.Property = "1";
				AssertNoNotifications(tester.PropertyInfo);
				tester.Property = "2";
				AssertNoNotifications(tester.PropertyInfo);
				tester.Property = "3";
				AssertHasWarningContaining(tester.PropertyInfo, ListValidation.InvalidCodeMessage);
				tester.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				tester.Property = "1";
				AssertNoNotifications(tester.PropertyInfo);
				tester.Property = "2";
				AssertNoNotifications(tester.PropertyInfo);
				tester.Property = "3";
				AssertHasWarningContaining(tester.PropertyInfo, ListValidation.InvalidCodeMessage);
			}

			);
		}
	}
}
