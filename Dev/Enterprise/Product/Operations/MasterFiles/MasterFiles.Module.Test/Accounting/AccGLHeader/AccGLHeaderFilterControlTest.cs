using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccGLHeaderFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestShowAlternateGLAccounts_HasGLAccountSelectionAndEntry()
		{
			var gLHeader = TestObjectCreator.CreateAccGLHeader("1111.11.11", AccountTypeComboBoxConstants.ProfitAndLossAccount);
			var chart = TestObjectCreator.CreateAlternateChart("MGT", "Management Reporting", isGlobal: true);
			var format = TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			Factory.Save();

			var alternateGLAccountForGLHeader = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", description: "AlternateGLAccount");
			var attribute = TestObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccountForGLHeader, gLHeader.PK, 1, "OCG", "OCG");
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid());

			AssertShowAlternateGLAccounts(true);
		}

		[RequiresSTA]
		public void TestShowAlternateGLAccounts_NoGLAccountSelectionAndEntry()
		{
			AssertShowAlternateGLAccounts(false);
		}

		void AssertShowAlternateGLAccounts(bool hasGLAccountSelectionAndEntry)
		{
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			using (var form = new ZForm(transactionHeader))
			{
				var genericCharges = new AccGLHeaderCollection(Factory);
				var filterBO = new AccGLHeaderFilterBusinessObject();

				var control = new AccGLHeaderFilterControl(genericCharges, filterBO);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var filteredGrid = form.FindSingleOrDefault<ZGrid>("FilteredGrid");

				var alternateAccounts = filteredGrid.GetColumnStyle("AlternateAccounts");
				AssertNotNull(alternateAccounts);
				AssertEquals(true, hasGLAccountSelectionAndEntry ? alternateAccounts.IsVisible : alternateAccounts.IsUnavailable);
			}
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
