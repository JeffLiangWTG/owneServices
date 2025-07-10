using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;
using Enterprise.LandedCosting.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.LandedCosting.GUI.Testing
{
	sealed class LandedCostingMenuTest : TestCaseWithFactory
	{
		public void TestRefreshLandedCostingWhenThereIsAJob()
		{
			TestHelper testHelper = new TestHelper(Factory);
			LandedCostHeader lCHeader = testHelper.GetLCHeaderWithNoChargeRowsToDefault();

			using (LandedCostingMenu lCMenu = new LandedCostingMenu(() => ""))
			{
				lCMenu.LCHeaderDelegate = () => lCHeader;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					LandCostInput lCInput = lCHeader.CostInputs.AddNew();//one row added
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					lCMenu.RefreshLandedCostingData.PerformClick();
					AssertEquals("there was one cost input row and it was not deleted", 1, lCHeader.CostInputs.Count);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					lCMenu.RefreshLandedCostingData.PerformClick();
					AssertEquals("there was one cost input row and it is deleted", 0, lCHeader.CostInputs.Count);

					lCInput = lCHeader.CostInputs.AddNew();//one row added
					lCInput.LI_IsUserEntered = true;

					LandCostInput lCInput2 = lCHeader.CostInputs.AddNew();
					lCInput2.LI_IsUserEntered = false;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					lCMenu.RefreshLandedCostingDataWithoutLosingUserEnteredData.PerformClick();
					AssertEquals("there was one cost input row that is user entered and is kept", 1, lCHeader.CostInputs.Count);
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

#if !WINZOR
		[TestDate(2005, 10, 25)]
		public void TestRunLCMenu()
		{
			TestHelper testHelper = new TestHelper(Factory);
			LandedCostHeader lCHeader = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();

			using (ZForm form = new ZForm(lCHeader))
			{
				using (LandedCostingMenu lCMenu = new LandedCostingMenu(() => ""))
				{
					form.Menu.MenuItems.Add(lCMenu);

					lCMenu.LCHeaderDelegate = delegate
					{ return lCHeader; };

					AssertEquals("There are no histories", 0, lCHeader.Histories.Count);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					lCMenu.RunLCMenu.PerformClick();
					AssertEquals("There are two histories generated", 2, lCHeader.Histories.Count);
					AssertEquals("Today is set to LCHeader.LT_DateOfProcessing", new ZDateTime(2005, 10, 25).ToShortDateString(), lCHeader.LT_DateOfProcessing.ToShortDateString());
					AssertEquals(true, lCHeader.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);// yes to previous LC result being wiped out
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);// yes to saving changes
					lCMenu.RunLCMenu.PerformClick();
					AssertEquals("Changes are saved", false, lCHeader.HasChanges);
				}
			}
		}
#endif

		public void TestRunLandedCostingWhenNoErrorsExist()
		{
			TestHelper testHelper = new TestHelper(Factory);
			LandedCostHeader lCHeader = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();
			AssertEquals("Date of processing", ZDateTime.Empty, lCHeader.LT_DateOfProcessing);

			using (LandedCostingMenu lCMenu = new LandedCostingMenu(() => ""))
			{
				AssertEquals("PreCondition:LC history", 0, lCHeader.Histories.Count);
				lCMenu.LCHeaderDelegate = delegate
				{ return lCHeader; };
				lCMenu.RunLCMenu.PerformClick();
				AssertEquals("Processed", true, lCHeader.LT_DateOfProcessing.IsValid);
				AssertEquals("LC history generated", true, lCHeader.Histories.Count > 0);
			}
		}

		public void TestRunLandedCostingWhenErrorExist()
		{
			TestHelper testHelper = new TestHelper(Factory);
			LandedCostHeader lCHeader = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();

			StatusCheckResult result = new LCDistributionStatusChecker(lCHeader).GetStatus();
			AssertNull("Job is in a valid status", result.Message);

			testHelper.LCInput3.LI_DistributeCostBy = MasterFiles.Business.CostDistributionMechanismList.Codes.ActualVolume;

			using (LandedCostingMenu lCMenu = new LandedCostingMenu(() => ""))
			{
				lCMenu.LCHeaderDelegate = delegate
				{ return lCHeader; };

				result = new LCDistributionStatusChecker(lCHeader).GetStatus();
				AssertEquals("PreCondition", true, result.IsError);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("PreCondition:LC history", 0, lCHeader.Histories.Count);

				lCMenu.RunLCMenu.PerformClick();
				ZString text = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Error", true, text.Contains(LCDistributionStatusChecker.DistributionFieldErrorMessage));
				AssertEquals("There is an error condition so no LC output is generated", 0, lCHeader.Histories.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testHelper.Ultimate1.VolumeExposed = 10m;
				lCMenu.RunLCMenu.PerformClick();
				text = UnitTestUserNotification.Instance.PreviousMessages[1].Text;
				AssertEquals("Warning", true, text.Contains(LCDistributionStatusChecker.DistributionFieldWarningMessage));
				AssertEquals("Agreed to run therefore should have a LC output", true, lCHeader.Histories.Count > 0);
			}
		}
	}
}
