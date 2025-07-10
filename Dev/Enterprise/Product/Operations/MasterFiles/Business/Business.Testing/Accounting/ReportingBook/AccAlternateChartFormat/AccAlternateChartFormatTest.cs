using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateChartFormat))]
	sealed class AccAlternateChartFormatTest : EnterpriseBusinessObjectTestCase
	{
		public void TestField()
		{
			var format = Factory.NewWithValidTestData<AccAlternateChartFormat>();
			AssertEquals(255, format.ANF_DescriptionInfo.MaxLength);
			AssertEquals(20, format.ANF_FormatInfo.MaxLength);
			AssertEquals(1, format.ANF_SeparatorInfo.MaxLength);
			AssertHasCustomAttribute<ListAttribute>(typeof(AccAlternateChartFormat), nameof(AccAlternateChartFormat.ANF_Separator), false, attr => attr.ListDataSourceMember == "Lookups.SeparatorList");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			return Creator.CreateAccAlternateChartFormat(chart, 1, "9", "Description", "-");
		}

		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;
	}
}
