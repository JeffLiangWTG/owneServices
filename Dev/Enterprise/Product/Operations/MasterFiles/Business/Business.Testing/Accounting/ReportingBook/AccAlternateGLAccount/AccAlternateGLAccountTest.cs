using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateGLAccount))]
	sealed class AccAlternateGLAccountTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCashFlowTypeAndStatisticalUnits()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "1111");
			var glHeader = Creator.CreateAccGLHeader("10.00.1010", Core.Constants.AccountType.BalanceSheetAccount, cashFlowType: CashFlowCodeLists.Codes.CSH, units: "KG");
			Factory.Save();
			AssertEquals("", alternateGLAccount.CashFlowType);
			AssertEquals("", alternateGLAccount.StatisticalUnits);

			Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK);
			Factory.Save();
			AssertEquals(CashFlowCodeLists.Codes.CSH, alternateGLAccount.CashFlowType);
			AssertEquals("KG", alternateGLAccount.StatisticalUnits);

			glHeader.AG_CashFlowType = CashFlowCodeLists.Codes.O06;
			glHeader.AG_StatisticalUnits = "U";
			Factory.Save();
			AssertEquals(CashFlowCodeLists.Codes.O06, alternateGLAccount.CashFlowType);
			AssertEquals("U", alternateGLAccount.StatisticalUnits);
		}

		public void TestCanDeleteAndReasonForNotAbleToDelete()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "10.00.1100");
			var newAlternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "20.00.1100", alternateNum: alternateGLAccount.PK, consolidate: alternateGLAccount.PK, percentNum: alternateGLAccount.PK, totalReference: alternateGLAccount.PK);
			Factory.Save();

			AssertEquals(@"The Alternate Account 10.00.1100 cannot be deleted because it is used as Alternate Number in Alternate Account: 20.00.1100.
The Alternate Account 10.00.1100 cannot be deleted because it is used as Consolidate in Alternate Account: 20.00.1100.
The Alternate Account 10.00.1100 cannot be deleted because it is used as Percent Number in Alternate Account: 20.00.1100.
The Alternate Account 10.00.1100 cannot be deleted because it is used as Total Reference in Alternate Account: 20.00.1100.", alternateGLAccount.ReasonForNotAbleToDelete);
			AssertEquals(false, alternateGLAccount.CanDelete);

			newAlternateGLAccount.Delete();
			Factory.Save();

			AssertEquals(string.Empty, alternateGLAccount.ReasonForNotAbleToDelete);
			AssertEquals(true, alternateGLAccount.CanDelete);
		}

		public void TestDeleteMultipleGLAccounts()
		{
			var alternateGLAccount1 = Creator.CreateAccAlternateGlAccount(Chart.PK, "10.00.1100", totalLevel: 2, percentNum: Guid.NewGuid(), consolidate: Guid.NewGuid(), alternateNum: Guid.NewGuid());
			alternateGLAccount1.AGA_Description = "GL Account Test 1";

			var alternateGLAccount2 = Creator.CreateAccAlternateGlAccount(Chart.PK, "20.00.1100", totalLevel: 2, percentNum: Guid.NewGuid(), consolidate: Guid.NewGuid(), alternateNum: Guid.NewGuid());
			alternateGLAccount2.AGA_Description = "GL Account Test 2";

			var list = new List<BusinessObject>();
			list.Add(alternateGLAccount1);
			list.Add(alternateGLAccount2);

			var deleter = new BusinessObjectMultipleDeleter(list.ToArray());
			AssertMultilineASCIIEquals("The selected records will be deleted:\r\n10.00.1100 - GL Account Test 1\r\n20.00.1100 - GL Account Test 2",
				deleter.GetConfirmationMessage(BusinessObjectMultipleDeleterAction.Delete));
		}

		public void TestConsolidationNum_ReadOnly()
		{
			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "3F99", Core.Constants.AccountType.Total, description: "DES2", drCR: "DR", printSequence: 2);

			Assert(!alternateGLAccount.AGA_AGA_ConsolidationNumInfo.ReadOnly);

			alternateGLAccount.AGA_AccountType = Core.Constants.AccountType.Header;
			Assert(alternateGLAccount.AGA_AGA_ConsolidationNumInfo.ReadOnly);

			alternateGLAccount.AGA_AccountType = Core.Constants.AccountType.Note;
			Assert(alternateGLAccount.AGA_AGA_ConsolidationNumInfo.ReadOnly);

			alternateGLAccount.AGA_AccountType = Core.Constants.AccountType.Rollup;
			Assert(alternateGLAccount.AGA_AGA_ConsolidationNumInfo.ReadOnly);

			alternateGLAccount.AGA_AccountType = Core.Constants.AccountType.Group;
			Assert(alternateGLAccount.AGA_AGA_ConsolidationNumInfo.ReadOnly);

			alternateGLAccount.AGA_AccountType = Core.Constants.AccountType.ChartOnly;
			Assert(alternateGLAccount.AGA_AGA_ConsolidationNumInfo.ReadOnly);

			alternateGLAccount.AGA_AccountType = Core.Constants.AccountType.Consolidation;
			Assert(alternateGLAccount.AGA_AGA_ConsolidationNumInfo.ReadOnly);
		}

		public void TestHasChildAccounts()
		{
			var alternateGLAccount1 = Creator.CreateAccAlternateGlAccount(Chart.PK, "1101");
			Creator.CreateAccAlternateGlAccount(Chart.PK, "1102");
			var alternateGLAccount2 = Creator.CreateAccAlternateGlAccount(Chart.PK, "00000000");
			Factory.Save();
			AssertEquals(false, alternateGLAccount1.HasChildAccounts());

			Creator.CreateAccAlternateGlAccount(Chart.PK, "11011101");
			Creator.CreateAccAlternateGlAccount(Chart.PK, "00001101");
			Factory.Save();
			AssertEquals(true, alternateGLAccount1.HasChildAccounts());
			AssertEquals(true, alternateGLAccount2.HasChildAccounts());
		}

		public void TestNoAuditLogs()
		{
			var account = creator.CreateAccAlternateGlAccount(Chart.PK, "11", accountType: Core.Constants.AccountType.Header);

			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, account.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				account.AGA_Description = "TEST LOG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				account.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestRelatedAuditChildren()
		{
			var account = creator.CreateAccAlternateGlAccount(Chart.PK, "11", accountType: Core.Constants.AccountType.Header);
			AssertEquals(1, account.RelatedAuditChildren.Count());
			AssertContainsExactElementsInAnyOrder(new string[] { "AAA_AGA_AlternateGLAccount" }, account.RelatedAuditChildren.Select(x => x.KeyColumn.Name));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Chart = Creator.CreateAlternateChart("111");
			Creator.CreateAccAlternateChartFormat(Chart, 1, "9999");
			Factory.Save();
		}

		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;
		AccAlternateChart Chart;
	}
}
