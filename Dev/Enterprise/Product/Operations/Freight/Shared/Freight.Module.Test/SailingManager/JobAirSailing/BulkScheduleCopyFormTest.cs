using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(BulkScheduleCopyForm))]
	sealed class BulkScheduleCopyFormTest : ZFormBasherTest
	{
		public void TestCheckedListBoxSize()
		{
			int expectedHeight = ControlDpiScalingHelper.NewScaledSize(352, 31, true).Height;
			using (BulkScheduleCopyForm bulkCopyForm = new BulkScheduleCopyForm(new BulkCopyCriteriaForTest(Sailing.PK)))
			{
				var checkedList = (ZCheckedListBox)bulkCopyForm.Controls.Find("WeekDaysCheckedList", true)[0];
				var height = checkedList.Height;
				AssertGreaterThanOrEqualTo("CheckedListBox Height", height, expectedHeight);
			}
		}

		public void TestActionButton_Click_Validation()
		{
			using (BulkScheduleCopyForm testForm = new BulkScheduleCopyForm(new BulkCopyCriteriaForTest(Sailing.PK)))
			{
				BulkCopyCriteria criteria = (BulkCopyCriteria)testForm.BusinessEntity;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				RepititionSelection selection = testForm.BulkCopy.RepititionSelection;

				testForm.Show();
				selection.UseDailyPattern = false;
				selection.UseMonthlyPattern = true;
				selection.DayOfTheMonth = 32;
				selection.FromDate = ZDateTime.Now;
				selection.ToDate = ZDateTime.Now;
				selection.FromFirstETD = true;
				selection.RecurrenceInterval = 1;

				testForm.PerformActionClick();
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestActionButton_Click_NoSchedulesCreated()
		{
			using (BulkScheduleCopyForm testForm = new BulkScheduleCopyForm(new BulkCopyCriteriaForTest(Sailing.PK)))
			{
				BulkCopyCriteria criteria = (BulkCopyCriteria)testForm.BusinessEntity;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				RepititionSelection selection = testForm.BulkCopy.RepititionSelection;

				testForm.Show();
				selection.UseMonthlyPattern = true;
				selection.UseDailyPattern = false;
				selection.DayOfTheMonth = ZDateTime.Now.Day == 1 ? 2 : 1;
				selection.FromDate = ZDateTime.Now;
				selection.ToDate = ZDateTime.Now;
				selection.FromFirstETD = true;
				selection.RecurrenceInterval = 5;

				testForm.PerformActionClick();
				AssertEquals("Based on the bulk copy criteria specified, no schedules have been created. This is likely to be because schedules already exist for all the dates specified.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestActionButton_Click_SchedulesCreated()
		{
			using (BulkScheduleCopyForm testForm = new BulkScheduleCopyForm(new BulkCopyCriteriaForTest(Sailing.PK)))
			{
				BulkCopyCriteria criteria = (BulkCopyCriteria)testForm.BusinessEntity;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				RepititionSelection selection = testForm.BulkCopy.RepititionSelection;

				testForm.Show();
				selection.UseMonthlyPattern = false;
				selection.UseDailyPattern = true;
				selection.FromDate = ZDateTime.Now;
				selection.ToDate = ZDateTime.Now.AddDays(7);
				selection.FromFirstETD = true;
				selection.RecurrenceInterval = 1;

				testForm.PerformActionClick();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertEquals("", ErrorReporter.LastKeyReported);
		}

		public void TestCopyShipmentsCheckBoxAndCopyRoutingCheckBoxNotVisibleIsCopyShipmentsAreReadOnly()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			var bulkCopyCriteria = new BulkCopyCriteriaForTest(Sailing.PK);
			AssertEquals(true, bulkCopyCriteria.ConsolDetails.CopyShipments_ReadOnly);
			AssertEquals(true, bulkCopyCriteria.ConsolDetails.CopyRoutings_ReadOnly);

			using (var form = new BulkScheduleCopyForm(bulkCopyCriteria))
			{
				form.Show();
				AssertEquals(false, form.Controls.Find("CopyShipmentsCheckBox", true)[0].Visible);
				AssertEquals(false, form.Controls.Find("CopyRoutingCheckBox", true)[0].Visible);
			}

			bulkCopyCriteria = new BulkCopyCriteriaForTest(Sailing.PK);
			bulkCopyCriteria.ConsolDetails.TemplateConsolPK = consol.PK;
			bulkCopyCriteria.ConsolDetails.CreateConsol = true;

			AssertEquals(false, bulkCopyCriteria.ConsolDetails.CopyShipments_ReadOnly);
			AssertEquals(false, bulkCopyCriteria.ConsolDetails.CopyRoutings_ReadOnly);

			using (var form = new BulkScheduleCopyForm(bulkCopyCriteria))
			{
				form.Show();
				AssertEquals(true, form.Controls.Find("CopyShipmentsCheckBox", true)[0].Visible);
				AssertEquals(true, form.Controls.Find("CopyRoutingCheckBox", true)[0].Visible);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			BaseJobSailing sailing = Factory.New<BaseJobSailing>();
			return new BulkScheduleCopyForm(new BulkCopyCriteriaForTest(sailing.PK));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Sailing = CreateSailing();
		}

		BaseJobSailing CreateSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "CX123";

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2006, 12, 12, 12, 0, 0);
			origin.JA_DocumentaryCutoff = new ZDateTime(2006, 12, 13, 13, 45, 0);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = new ZDateTime(2006, 12, 13, 10, 59, 0);

			voyage.GenerateSailings();
			BaseJobSailing sailing = voyage.Sailings[0];
			sailing.JX_DepotReceivalCommences = new ZDateTime(2006, 10, 12, 12, 0, 0);
			sailing.JX_DepotCutOff = sailing.JX_DepotReceivalCommences;
			sailing.JX_DepotStorageDate = sailing.JX_JA_DocumentaryCutoff;
			Factory.Save();

			return sailing;
		}

		BaseJobSailing Sailing;

		#endregion
	}
}
