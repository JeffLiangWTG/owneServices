namespace Enterprise.Freight.Forwarding.Routing.S8.GUI.Test
{
	using System.Windows.Forms;
	using CargoWise.Application;
	using CargoWise.Data;
	using CargoWise.Types;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.Freight.Forwarding.GUI;
	using Enterprise.Freight.Forwarding.Routing.S8.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.GUI.Testing;
	using Enterprise.ZArchitecture.Modules;
	using NUnit.Framework;

	[TestedType(typeof(BulkConsolCreationForm))]
	public class RoutingBulkConsolCreationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var requestDate = new ZDateTime(2018, 7, 6);
			var testMessageLine1 =
				"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/26 2018/10/06 1.3..6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
			var testMessageLine2 =
				"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/08 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header1,
				header2
			};

			var multiDaysSelection = RoutingMultiDaysSelection.Create(requestDate, routingResponseHeaders, false, false, Factory);
			return GetBulkConsolCreationFormForTest(multiDaysSelection);
		}

		RoutingMultiDaysSelection GetMultiDaysSelection(bool inlucdeWeeklyTimetable, bool importAndCreateMawb)
		{
			var requestedDate = new ZDateTime(2018, 7, 6);
			var testMessageLine =
				"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/08 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header
			};

			return RoutingMultiDaysSelection.Create(requestedDate, routingResponseHeaders, inlucdeWeeklyTimetable, importAndCreateMawb, Factory);
		}

		public void TestBulkConsolCreationForm_Recurrence()
		{
			var requestDate = new ZDateTime(2018, 7, 6);
			var testMessageLine1 =
				"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/26 2018/10/06 1.3..6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
			var testMessageLine2 =
				"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/08 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header1,
				header2
			};
			var routeMultiDaysSelection = RoutingMultiDaysSelection.Create(requestDate, routingResponseHeaders, false, true, Factory);
			using (var form = GetBulkConsolCreationFormForTest(routeMultiDaysSelection))
			{
				form.Show();
				var control = form.Controls.Find("createConsolsTabControl", true);
				AssertEquals(1, control.Length);
				AssertEquals(0, control[0].Top);
			}
		}

		#region ConsolDetailsForm

		// Include Weekly timetable ENABLED - show both Consolidation details and recurrence form.
		// Include Weekly timetable DISABLED - show Consolidation details only.
		public void TestBulkConsolCreationForm_ConsolDetailsForm_With_IncludeWeeklyTimetableEnabled()
		{
			var multiDaysSelection = GetMultiDaysSelection(true, true);

			using (var form = GetBulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();
				Assert("Precondition - multiDaysSelection.ImportAndCreateMAWB should be true.", GetMultiDaysSelectionData(form).ImportAndCreateMAWB);
				Assert("Precondition - multiDaysSelection.IncludeWeeklyTimetable should be true.", GetMultiDaysSelectionData(form).IncludeWeeklyTimetable);
				CombineAssertions(() =>
				{
					Assert("ImportSchedulesLabel should be displayed.", form.Controls.Find("MultipleFlightsLabel", true)[0].Visible);
					Assert("Recurrence controls should be displayed.", form.Controls.Find("RecurrencePatternGroupBox", true)[0].Visible);
					Assert("Range of Recurrence controls should be displayed.", form.Controls.Find("RangeOfRecurrenceGroupBox", true)[0].Visible);
					Assert("Consolidation Details controls should be displayed.", form.Controls.Find("ConsolidationDetailsGroupBox", true)[0].Visible);
				});

				form.Close();
			}
		}

		public void TestBulkConsolCreationForm_ConsolDetailsForm_With_IncludeWeeklyTimetableDisabled()
		{
			var multiDaysSelection = GetMultiDaysSelection(false, true);

			using (var form = GetBulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();
				Assert("Precondition - multiDaysSelection.ImportAndCreateMAWB should be true.", GetMultiDaysSelectionData(form).ImportAndCreateMAWB);
				Assert("Precondition - multiDaysSelection.IncludeWeeklyTimetable should be false.", !GetMultiDaysSelectionData(form).IncludeWeeklyTimetable);
				CombineAssertions(() =>
				{
					Assert("ImportSchedulesLabel should not be visible.", !form.Controls.Find("MultipleFlightsLabel", true)[0].Visible);
					Assert("Recurrence controls should not be visible.", !form.Controls.Find("RecurrencePatternGroupBox", true)[0].Visible);
					Assert("Range of Recurrence controls should not be visible.", !form.Controls.Find("RangeOfRecurrenceGroupBox", true)[0].Visible);
					Assert("Consolidation Details controls should be visible.", form.Controls.Find("ConsolidationDetailsGroupBox", true)[0].Visible);
				});

				form.Close();
			}
		}

		public void TestBulkConsolCreationForm_ConsolDetailsForm_IsNotVisible_IfOpenedThroughImportButton()
		{
			var multiDaysSelection = GetMultiDaysSelection(true, false);

			using (var form = GetBulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();
				Assert("Precondition - multiDaysSelection.ImportAndCreateMAWB should be false.", !GetMultiDaysSelectionData(form).ImportAndCreateMAWB);
				Assert("Precondition - multiDaysSelection.IncludeWeeklyTimetable should be true.", GetMultiDaysSelectionData(form).IncludeWeeklyTimetable);
				CombineAssertions(() =>
				{
					Assert("ImportSchedulesLabel should be visible.", form.Controls.Find("MultipleFlightsLabel", true)[0].Visible);
					Assert("Recurrence controls should be visible.", form.Controls.Find("RecurrencePatternGroupBox", true)[0].Visible);
					Assert("Range of Recurrence controls should be visible.", form.Controls.Find("RangeOfRecurrenceGroupBox", true)[0].Visible);
					Assert("Consolidation Details controls should be visible.", !form.Controls.Find("ConsolidationDetailsGroupBox", true)[0].Visible);
				});

				form.Close();
			}
		}

		public void TestMultiDaysSelectionForm_CreateFromConsols_ConfirmationMessageAppearsIfBothTabsHaveChanges()
		{
			var templateRefId = SetupTemplateRecord("A");

			var multiDaysSelection = GetMultiDaysSelection(true, true);
			multiDaysSelection.ConsolDetails.ConsolsPerFlight = 5;
			var template = multiDaysSelection.ConsolTemplateDetails.ConsolTemplates.AddNew();
			template.ConsolTemplateReferenceId = templateRefId;
			template.ConsolTemplateName = "A";
			template.ConsolsPerFlight = 2;

			using (var form = GetBulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();
				GetCreateConsolTabControl(form).SelectTab(0);
				GetSelectButtonForTest(form).PerformClick();
				Assert("Precondition - MultiDaysSelectionData.ConsolDetails.HasChanges should be true.", GetMultiDaysSelectionData(form).ConsolDetails.HasChanges);
				Assert("Precondition - MultiDaysSelectionData.ConsolTemplateDetails.HasChanges should be true.", GetMultiDaysSelectionData(form).ConsolTemplateDetails.HasChanges);

				form.Close();
				var lastMessage = UnitTestUserNotification.Instance.LastMessage.ToString();
				AssertContains("You are about to create new consols, but have entered data into the 'Create Consols From Templates' tab. Data entered into the inactive tab will be ignored - do you wish to continue?", lastMessage);
			}
		}

		public void TestMultiDaysSelectionForm_CreateFromConsolTemplates_ConfirmationMessageAppearsIfBothTabsHaveChanges()
		{
			var templateRefId = SetupTemplateRecord("A");

			var multiDaysSelection = GetMultiDaysSelection(true, true);
			multiDaysSelection.ConsolDetails.ConsolsPerFlight = 5;
			var template = multiDaysSelection.ConsolTemplateDetails.ConsolTemplates.AddNew();
			template.ConsolTemplateReferenceId = templateRefId;
			template.ConsolTemplateName = "A";
			template.ConsolsPerFlight = 2;

			using (var form = GetBulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();
				GetCreateConsolTabControl(form).SelectTab(1);
				GetSelectButtonForTest(form).PerformClick();
				Assert("Precondition - MultiDaysSelectionData.ConsolDetails.HasChanges should be true.", GetMultiDaysSelectionData(form).ConsolDetails.HasChanges);
				Assert("Precondition - MultiDaysSelectionData.ConsolTemplateDetails.HasChanges should be true.", GetMultiDaysSelectionData(form).ConsolTemplateDetails.HasChanges);

				form.Close();
				var lastMessage = UnitTestUserNotification.Instance.LastMessage.ToString();
				AssertContains("You are about to create consols from templates, but have entered data into the 'Create New Consols' tab. Data entered into the inactive tab will be ignored - do you wish to continue?", lastMessage);
			}
		}

		ZString SetupTemplateRecord(ZString templateName, bool isActive = true)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUBNE";

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = templateName;
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;
			templateRecord.STR_IsActive = isActive;

			var templateRecordProvider = consol as ITemplateRecordProvider;
			templateRecordProvider.IsTemplateRecord = true;
			templateRecordProvider.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider.SaveToTemplateRecord();
			}

			return templateRecord.STR_ReferenceId;
		}

		#endregion

		#region Validation

		public void TestBulkConsolCreationForm_Recurrence_IsNotValidatedWhenIncludeWeeklyTimetableUnticked()
		{
			var requestedDate = new ZDateTime(2018, 7, 6);
			var header = new RoutingResponseHeader("ERROR: No departure dates", Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header
			};

			var multiDaysSelection = RoutingMultiDaysSelection.Create(requestedDate, routingResponseHeaders, false, false, Factory);

			using (var form = GetBulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();
				GetSelectButtonForTest(form).PerformClick();
				Assert("No error should be raised.", !UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestBulkConsolCreationForm_Recurrence_IsValidatedWhenIncludeWeeklyTimetableTicked()
		{
			var requestedDate = new ZDateTime(2018, 7, 6);
			var header = new RoutingResponseHeader("ERROR: No departure dates", Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header
			};

			var multiDaysSelection = RoutingMultiDaysSelection.Create(requestedDate, routingResponseHeaders, true, false, Factory);

			using (var form = GetBulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();
				GetSelectButtonForTest(form).PerformClick();
				Assert("Expected error to be raised.", UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestBulkConsolCreationForm_ConsolDetails_IsValidatedWhenIncludeWeeklyTimetableUnticked()
		{
			var multiDaysSelection = GetMultiDaysSelection(false, true);

			multiDaysSelection.ConsolDetails.Chargeable = 10;
			multiDaysSelection.ConsolDetails.AirlinePrefix = "AV";
			multiDaysSelection.ConsolDetails.Weight = 1;
			multiDaysSelection.ConsolDetails.Volume = 1;
			multiDaysSelection.ConsolDetails.Shipments = -1;     // Incorrect number of shipments.
			multiDaysSelection.ConsolDetails.ResumeValidation();

			using (var form = GetBulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();
				GetSelectButtonForTest(form).PerformClick();
				Assert("Expected error to be raised.", UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestBulkConsolCreationForm_ConsolDetails_IsValidatedWhenIncludeWeeklyTimetableTicked()
		{
			var multiDaysSelection = GetMultiDaysSelection(true, true);

			multiDaysSelection.ConsolDetails.Chargeable = 10;
			multiDaysSelection.ConsolDetails.AirlinePrefix = "AV";
			multiDaysSelection.ConsolDetails.Weight = 1;
			multiDaysSelection.ConsolDetails.Volume = 1;
			multiDaysSelection.ConsolDetails.Shipments = -1;     // Incorrect number of shipments.
			multiDaysSelection.ConsolDetails.ResumeValidation();

			using (var form = GetBulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();
				GetSelectButtonForTest(form).PerformClick();
				Assert("Expected error to be raised.", UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestBulkConsolCreationForm_ConsolDetails_NotValidated()
		{
			var multiDaysSelection = GetMultiDaysSelection(true, false);

			multiDaysSelection.ConsolDetails.Chargeable = 10;
			multiDaysSelection.ConsolDetails.AirlinePrefix = "AV";
			multiDaysSelection.ConsolDetails.Weight = 1;
			multiDaysSelection.ConsolDetails.Volume = 1;
			multiDaysSelection.ConsolDetails.Shipments = -1;     // Incorrect number of shipments.
			multiDaysSelection.ConsolDetails.ResumeValidation();

			using (var form = GetBulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();
				GetSelectButtonForTest(form).PerformClick();
				Assert("No error should be raised.", !UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region Implementation

		protected override bool AllowHasChangesOnFormOpen => true;

		ZChildForm GetBulkConsolCreationFormForTest(RoutingMultiDaysSelection multiDaysSelection)
		{
			return (ZChildForm)ObjectFactory.Get<Integration.Forwarding.IBulkConsolCreationForm>("IBulkConsolCreationForm", multiDaysSelection);
		}

		ZButton GetSelectButtonForTest(ZChildForm form)
		{
			return (ZButton)form.Controls.Find("SelectButton", true)[0];
		}

		ZTemplateTabControl GetCreateConsolTabControl(ZChildForm form)
		{
			return (ZTemplateTabControl)form.Controls.Find("CreateConsolTabControl", true)[0];
		}

		MultiDaysSelection GetMultiDaysSelectionData(ZChildForm form)
		{
			return (MultiDaysSelection)form.BusinessEntity;
		}

		#endregion
	}
}
