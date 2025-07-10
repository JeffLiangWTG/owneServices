namespace Enterprise.Freight.Forwarding.Routing.S8.GUI.Test
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using CargoWise.Application;
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.GUI;
	using Enterprise.Freight.Forwarding.Routing.S8.Business;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.GUI.Testing;
	using NUnit.Framework;

	[TestedType(typeof(SchedulesConsolsForm))]
	public class RoutingSchedulesConsolsFormTest : ZFormBasherTest
	{
		#region Save Button

		public void TestSchedulesConsolsForm_SaveButton()
		{
			var multiDaysSelection = GetMultiDaysSelection();
			var departureDates = new List<ZDateTime>() { new ZDateTime(2018, 7, 6) };

			multiDaysSelection.CreateVoyagesSailings(departureDates);

			var sailingsExpected = multiDaysSelection.RoutingResponseHeaders.Cast<RoutingResponseHeader>().Single().Lines.Count * departureDates.Count;
			AssertEquals("Precondition - number of sailings generated is incorrect.", sailingsExpected, multiDaysSelection.SailingCollection.Count);

			var sailings = multiDaysSelection.SailingCollection;
			foreach (JobSailing sailing in sailings)
			{
				Assert("Precondition - sailing should not be in database.", !sailing.IsInDatabase);
			}

			// Generate consols.
			multiDaysSelection.ConsolDetails.ConsolsPerFlight = 10;
			multiDaysSelection.ActiveTab = RoutingMultiDaysSelection.CreateNewConsolsTabName;
			multiDaysSelection.Generate();
			AssertEquals("Precondition - The number of consols generated is incorrect.", Convert.ToInt32(multiDaysSelection.ConsolDetails.ConsolsPerFlight), multiDaysSelection.CreatedConsols.Count);

			foreach (CommonConsol consol in multiDaysSelection.CreatedConsols)
			{
				Assert("Precondition - consol should not be saved into database.", !consol.IsInDatabase);
				AssertNoErrors("Precondition - Expected no errors on the created consol's MAWB number so we should be able to save.", consol.JK_MasterBillNumInfo);
			}

			using (var schedulesConsolsForm = GetSchedulesConsolsForm(multiDaysSelection))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				schedulesConsolsForm.Show();

				Assert("Precondition - Save button should be visible.", GetSaveButtonForTest(schedulesConsolsForm).Visible);
				GetSaveButtonForTest(schedulesConsolsForm).PerformClick();

				AssertNull("Precondition - Expected no error messages on saving.", UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Save button should not be visible after saving performed.", !GetSaveButtonForTest(schedulesConsolsForm).Visible);
				AssertEquals("Contents of Cancel button should be changed to Close after saving attempted.", "Close", GetCancelButtonForTest(schedulesConsolsForm).Text);

				foreach (CommonConsol consol in multiDaysSelection.CreatedConsols)
				{
					Assert("Expected consol to be saved into database.", consol.IsInDatabase);
				}

				foreach (JobSailing sailing in sailings)
				{
					Assert("Expected schedule to be saved into database.", sailing.IsInDatabase);
				}
			}
		}

		public void TestSchedulesConsolsForm_SaveButton_WithValidationError()
		{
			var multiDaysSelection = GetMultiDaysSelection();
			var departureDates = new List<ZDateTime>() { new ZDateTime(2018, 7, 6) };

			multiDaysSelection.CreateVoyagesSailings(departureDates);

			var sailingsExpected = multiDaysSelection.RoutingResponseHeaders.Cast<RoutingResponseHeader>().Single().Lines.Count * departureDates.Count;
			AssertEquals("Precondition - number of sailings generated is incorrect.", sailingsExpected, multiDaysSelection.SailingCollection.Count);

			var sailings = multiDaysSelection.SailingCollection;
			foreach (JobSailing sailing in sailings)
			{
				Assert("Precondition - sailing should not be in database.", !sailing.IsInDatabase);
			}

			multiDaysSelection.ConsolDetails.ConsolsPerFlight = 1;
			multiDaysSelection.ActiveTab = RoutingMultiDaysSelection.CreateNewConsolsTabName;
			multiDaysSelection.Generate();
			AssertEquals("Precondition - The number of consols generated is incorrect.", Convert.ToInt32(multiDaysSelection.ConsolDetails.ConsolsPerFlight), multiDaysSelection.CreatedConsols.Count);

			foreach (CommonConsol consol in multiDaysSelection.CreatedConsols)
			{
				Assert("Precondition - consol should not be saved into database.", !consol.IsInDatabase);
				AssertNoErrors("Precondition - Expected no errors on the created consol's MAWB number so we should be able to save.", consol.JK_MasterBillNumInfo);
			}

			multiDaysSelection.CreatedConsols[0].JK_RL_NKLoadPort = ZString.Empty;

			using (var schedulesConsolsForm = GetSchedulesConsolsForm(multiDaysSelection))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				schedulesConsolsForm.Show();

				Assert("Precondition - Save button should be visible.", GetSaveButtonForTest(schedulesConsolsForm).Visible);
				GetSaveButtonForTest(schedulesConsolsForm).PerformClick();

				AssertEquals("Consolidations could not be created with errors.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var multiDaysSelection = GetMultiDaysSelection();
			return (Form)ObjectFactory.Get<Integration.Forwarding.ISchedulesConsolsForm>("ISchedulesConsolsForm", multiDaysSelection);
		}

		RoutingMultiDaysSelection GetMultiDaysSelection()
		{
			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J>";
			var requestedDate = new ZDateTime(2018, 7, 6);

			var header = new RoutingResponseHeader(testMessageLine, Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header
			};

			return RoutingMultiDaysSelection.Create(requestedDate, routingResponseHeaders, false, false, Factory);
		}

		ZChildForm GetSchedulesConsolsForm(RoutingMultiDaysSelection multiDaysSelection)
		{
			return (ZChildForm)ObjectFactory.Get<Integration.Forwarding.ISchedulesConsolsForm>("ISchedulesConsolsForm", multiDaysSelection);
		}

		ZButton GetSaveButtonForTest(ZChildForm form)
		{
			return (ZButton)form.Controls.Find("SaveButton", true)[0];
		}

		ZButton GetCancelButtonForTest(ZChildForm form)
		{
			return (ZButton)form.Controls.Find("CancelButton", true)[0];
		}

		#endregion
	}
}
