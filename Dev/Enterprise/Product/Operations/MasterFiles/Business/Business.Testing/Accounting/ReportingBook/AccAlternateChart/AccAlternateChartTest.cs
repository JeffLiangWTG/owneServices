using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateChart))]
	sealed class AccAlternateChartTest : EnterpriseBusinessObjectTestCase
	{
		public void TestField()
		{
			var chart = Factory.New<AccAlternateChart>();
			AssertEquals(255, chart.AAC_DescriptionInfo.MaxLength);
			AssertEquals(3, chart.AAC_BalanceSheetStyleInfo.MaxLength);
			AssertEquals(10, chart.AAC_CodeInfo.MaxLength);
			AssertEquals(3, chart.AAC_ReportOrderInfo.MaxLength);
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(AccAlternateChart), nameof(AccAlternateChart.AAC_IsFixedLength), false, attr => attr.Member == "AAC_ReadOnly");
			AssertHasCustomAttribute<ListAttribute>(typeof(AccAlternateChart), nameof(AccAlternateChart.AAC_BalanceSheetStyle), false, attr => attr.ListDataSourceMember == "Lookups.BaseBalanceSheetStyleList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AccAlternateChart), nameof(AccAlternateChart.AAC_ReportOrder), false, attr => attr.ListDataSourceMember == "Lookups.ReportOrderList");
			Assert(!chart.AAC_ReadOnlyForTest);
			Assert(!chart.AlternateChartFormats.ReadOnly);
			var alternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccount.AGA_AAC_AlternateChart = chart.PK;
			Assert(chart.AAC_ReadOnlyForTest);

			var chart2 = Factory.New<AccAlternateChart>();
			alternateGLAccount.AGA_AAC_AlternateChart = chart2.PK;
			Assert(chart2.AlternateChartFormats.ReadOnly);
		}

		public void TestDelete()
		{
			var chart = Factory.New<AccAlternateChart>();
			var format = Creator.CreateAccAlternateChartFormat(chart, 1, "9", "1", "-");
			chart.Delete();
			Assert(chart.IsDeleted);
			Assert(format.IsDeleted);

			var chart2 = Factory.NewWithValidTestData<AccAlternateChart>();
			var alternateGLAccount = Factory.NewWithValidTestData<AccAlternateGLAccount>();
			alternateGLAccount.AGA_AAC_AlternateChart = chart2.PK;

			var exp = AssertExceptionThrown<NotSupportedException>(() => chart2.Delete());

			AssertEquals("Can't delete Alternate Chart of Account because Reporting Books or Alternate GL Accounts are using this chart. Please remove this chart from all Reporting Books and delete all its Alternate GL Account before deleting.", exp.Message);
		}

		public void TestNoAuditLogs()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chart.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				chart.AAC_Description = "TEST LOG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chart.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestAAC_IsGlobal_ReadOnly()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			AssertEquals("AAC_IsGlobal should not be readOnly.", false, chart.AAC_IsGlobalInfo.ReadOnly);

			Factory.Save();
			AssertEquals("AAC_IsGlobal should be readOnly.", true, chart.AAC_IsGlobalInfo.ReadOnly);
		}

		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;
	}
}
