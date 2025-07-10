using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(CostsComparerForm))]
	public class CostsComparerFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestFormFiresValidation()
		{
			using (CostsComparerForm testForm = new CostsComparerForm())
			{
				testForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.NextButton.PerformClick();
				AssertEquals("Costs", testForm.MainTabControl.SelectedTab.Text);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.PreviousButton.PerformClick();
				testForm.Comparer.Mode = "LSE";
				testForm.NextButton.PerformClick();
				AssertEquals("Costs", testForm.MainTabControl.SelectedTab.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestWizardButtons()
		{
			using (CostsComparerForm testForm = new CostsComparerForm())
			{
				testForm.Show();

				testForm.MainTabControl.SelectedTab = (ZTabPage)testForm.MainTabControl.TabPages[0];
				Assert(!testForm.PreviousButton.Visible);
				Assert(testForm.PreviousButton.Enabled);
				AssertEquals("Previous", testForm.PreviousButton.Text);
				Assert(testForm.NextButton.Visible);
				Assert(testForm.NextButton.Enabled);
				AssertEquals("Compare Costs", testForm.NextButton.Text);

				testForm.Comparer.Mode = "LSE";
				testForm.MainTabControl.SelectedTab = (ZTabPage)testForm.MainTabControl.TabPages[1];
				Assert(testForm.PreviousButton.Visible);
				Assert(testForm.PreviousButton.Enabled);
				AssertEquals("Filter", testForm.PreviousButton.Text);
				Assert(!testForm.NextButton.Visible);
				Assert(testForm.NextButton.Enabled);
				AssertEquals("Next", testForm.NextButton.Text);
			}
		}

		public void TestSummaryColumns()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var line1a = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			line1a.Calculator["MIN"] = (ZDecimal)100m;
			line1a.Calculator["-45"] = (ZDecimal)5m;
			line1a.Calculator["+45"] = (ZDecimal)4m;
			line1a.Calculator["+100"] = (ZDecimal)3m;
			line1a.Calculator["+250"] = (ZDecimal)2m;
			line1a.Calculator["+500"] = (ZDecimal)1.9m;
			line1a.Calculator["+1000"] = (ZDecimal)1.85m;
			var item1 = line1a.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 1200m, (ZDecimal)1.80m);
			item1.TM_FlatAmount = 3;

			var line1b = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USSFO").RateLines[0];
			line1b.RateLineItems.RemoveAndDeleteAll();
			line1b.Calculator["MIN"] = (ZDecimal)100m;
			line1b.Calculator["-45"] = (ZDecimal)6m;
			line1b.Calculator["+45"] = (ZDecimal)5m;
			line1b.Calculator["+100"] = (ZDecimal)4m;
			line1b.Calculator["+300"] = (ZDecimal)2.5m;
			line1b.Calculator["+500"] = (ZDecimal)2.1m;
			line1b.Calculator["+1000"] = (ZDecimal)2m;
			var item2 = line1b.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 1200m, (ZDecimal)1.80m);
			item2.TM_FlatAmount = 3;

			var entry1c = costing1.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			Factory.Save();

			using (var testForm = new CostsComparerForm())
			{
				testForm.Show();

				testForm.Comparer.Mode = "LSE";
				testForm.MainTabControl.SelectedTab = (ZTabPage)testForm.MainTabControl.TabPages[1];
				AssertEquals(28, testForm.CostsGrid.Columns.Count);
				var prefix = CostsComparerForm.SummaryColumnCaption;
				AssertEquals("Min", testForm.CostsGrid.Columns[prefix + "1"].ColumnStyle.HeaderText);
				AssertEquals("-45", testForm.CostsGrid.Columns[prefix + "2"].ColumnStyle.HeaderText);
				AssertEquals("+45", testForm.CostsGrid.Columns[prefix + "3"].ColumnStyle.HeaderText);
				AssertEquals("+100", testForm.CostsGrid.Columns[prefix + "4"].ColumnStyle.HeaderText);
				AssertEquals("+250", testForm.CostsGrid.Columns[prefix + "5"].ColumnStyle.HeaderText);
				AssertEquals("+300", testForm.CostsGrid.Columns[prefix + "6"].ColumnStyle.HeaderText);
				AssertEquals("+500", testForm.CostsGrid.Columns[prefix + "7"].ColumnStyle.HeaderText);
				AssertEquals("+1000", testForm.CostsGrid.Columns[prefix + "8"].ColumnStyle.HeaderText);
				AssertEquals("+1200", testForm.CostsGrid.Columns[prefix + "9"].ColumnStyle.HeaderText);

				var style = (ZTextBoxColumnStyleInfo)testForm.CostsGrid.ColumnStyles[24];
				AssertEquals("ZDecimal", ((IOverridablePropertyDescriptor)style).PropertyDescriptor.PropertyType.Name);

				style = (ZTextBoxColumnStyleInfo)testForm.CostsGrid.ColumnStyles[27];
				AssertEquals("ZString", ((IOverridablePropertyDescriptor)style).PropertyDescriptor.PropertyType.Name);

				testForm.MainTabControl.SelectedTab = (ZTabPage)testForm.MainTabControl.TabPages[0];
				testForm.Comparer.Mode = "LCL";
				testForm.MainTabControl.SelectedTab = (ZTabPage)testForm.MainTabControl.TabPages[1];
				AssertEquals(21, testForm.CostsGrid.Columns.Count);
				AssertEquals("Min", testForm.CostsGrid.Columns[prefix + "1"].ColumnStyle.HeaderText);
				AssertEquals("Per Unit", testForm.CostsGrid.Columns[prefix + "2"].ColumnStyle.HeaderText);
			}
		}

		public void TestVisibilityOfRateLinesAndSecurityMessage()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			Costing costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			RateEntry deniedEntry = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			deniedEntry.RateLines[0].Calculator["MIN"] = (ZDecimal)100m;
			deniedEntry.TI_OH_Consignee = setupResult.DeniedOrg.PK;

			RateEntry allowedEntry = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USSFO");
			allowedEntry.RateLines[0].RateLineItems.RemoveAndDeleteAll();
			allowedEntry.RateLines[0].Calculator["MIN"] = (ZDecimal)200m;
			allowedEntry.TI_OH_Consignee = setupResult.AllowedOrg.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.CachingEnabled = false;
				using (var form = new CostsComparerForm())
				{
					form.Show();

					form.Comparer.Mode = "LSE";
					form.MainTabControl.SelectedTab = (ZTabPage)form.MainTabControl.TabPages[1];

					AssertEquals(2, form.CostsGrid.List.Count);

					var deniedEntryPosition = ((CostsComparerEntry)form.CostsGrid.List[0]).Entry.PK == deniedEntry.PK
											? 0
											: 1;
					var allowedEntryPosition = 1 - deniedEntryPosition; //Switch between 0 and 1

					form.CostsGrid.ListManager.Position = deniedEntryPosition;
					Assert(form.SecurityMessageLabel.Visible);
					Assert(!form.RateLinesAndItemsControl.Visible);

					form.CostsGrid.ListManager.Position = allowedEntryPosition;
					Assert(!form.SecurityMessageLabel.Visible);
					Assert(form.RateLinesAndItemsControl.Visible);
				}
			}
		}

		public void TestLastMessageWhenNumberOfRecordsMoreThanCostComparisonLinesNumber()
		{
			var costsComparer = new Mock<CostsComparer> { CallBase = true };

			var collectionToReturn = new CostsComparerEntryCollection(costsComparer.Object, Factory);

			RatingDataRegistry.Instance.CostComparisonLinesNumber.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty,
				Guid.Empty, 100);

			for (int i = 0; i < RatingDataRegistry.Instance.CostComparisonLinesNumber.Value + 1; i++)
			{
				collectionToReturn.AddNew();
			}

			costsComparer.Setup(m => m.LoadCosts());
			costsComparer.Setup(m => m.Costs).Returns(collectionToReturn);

			using (var testForm = new CostsComparerForm(costsComparer.Object))
			{
				testForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Comparer.Mode = "LSE";
				testForm.NextButton.PerformClick();
				AssertEquals("Costs", testForm.MainTabControl.SelectedTab.Text);
				string correctMessageText =
					string.Format("Too many records to display. Only first {0} will be shown.",
						RatingDataRegistry.Instance.CostComparisonLinesNumber.Value);
				AssertEquals(correctMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			costsComparer.VerifyAll();
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new CostsComparerForm();
		}

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}
