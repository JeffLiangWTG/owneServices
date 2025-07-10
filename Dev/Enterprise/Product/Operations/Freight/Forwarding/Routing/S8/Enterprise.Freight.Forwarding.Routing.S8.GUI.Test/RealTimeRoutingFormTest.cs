using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI.Test
{
	[TestedType(typeof(RealTimeRoutingForm))]
	public class RealTimeRoutingFormTest : ZFormBasherTest
	{
		public void TestSelectedRouting_DialogResult_None()
		{
			using (RealTimeRoutingForm form = new RealTimeRoutingForm(new RoutingManager(Factory)))
			{
				form.Show();

				RoutingResponseHeaderCollection list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				RoutingResponseHeader response1 = list.AddNew();
				RoutingResponseHeader response2 = list.AddNew();
				form.FilterControl.FilteredGrid.Select(1);
				form.Close();
				AssertNull(form.ChosenRouting);
			}
		}

		public void TestSelectedRouting_DialogResult_OK()
		{
			using (RealTimeRoutingForm form = new RealTimeRoutingForm(new RoutingManager(Factory)))
			{
				form.Show();

				RoutingResponseHeaderCollection list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				RoutingResponseHeader response1 = list.AddNew();
				RoutingResponseHeader response2 = list.AddNew();
				form.FilterControl.FilteredGrid.Select(1);
				form.DialogResult = DialogResult.OK;
				form.Close();
				AssertEquals(response2, form.ChosenRouting);
			}
		}

		public void TestDisplayMode()
		{
			using (RealTimeRoutingForm form = new RealTimeRoutingForm(new RoutingManager(Factory)))
			{
				form.Show();
				AssertEquals("DisplayMode should be Browse to prevent asking unrelated questions about save on form close", ODisplayMode.Browse, form.DisplayMode);
			}
		}

		#region Import Button

		public void TestImportButton_Click_CheckUserHasSecurityRightsToCreateNewSailings()
		{
			var routingManager = new RoutingManager(Factory);
			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.Show();

				var isAllowedNew = Env.Security.FlightScheduleNew.IsAllowed;
				var isAllowedCreateFromJob = Env.Security.FlightScheduleCreateFromJob.IsAllowed;
				using (new DisposableAction(() =>
				{
					Env.Security.FlightScheduleNew.IsAllowed = isAllowedNew;
					Env.Security.FlightScheduleCreateFromJob.IsAllowed = isAllowedCreateFromJob;
				}))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Env.Security.FlightScheduleNew.IsAllowed = false;
					form.ImportButton.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(Env.Security.FlightScheduleNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Env.Security.FlightScheduleNew.IsAllowed = true;
					Env.Security.FlightScheduleCreateFromJob.IsAllowed = false;
					form.FlightScheduleCreateFromJob = true;
					form.ImportButton.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(Env.Security.FlightScheduleCreateFromJob.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.FlightScheduleCreateFromJob = false;
					form.ImportButton.PerformClick();
					AssertNotEquals(Env.Security.FlightScheduleCreateFromJob.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestImportButton_Click_OneRouteHasBeenImported()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(!header.AnyLineHasMultipleAircraftTypes);

			var departureDate = new ZDateTime(2018, 7, 10);

			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "HKHKG",
						DepartureDate = departureDate
					}
				},
				IncludeWeeklyTimetable = false
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.Show();

				var list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				list.Add(header);
				form.FilterControl.FilteredGrid.Select(0);

				Assert("IsImporting should be false before importing.", !form.IsImporting);
				form.ImportButton.PerformClick();
				Assert("IsImporting should be false as ChosenRouting is not set.", !form.IsImporting);

				form.Closing += (sender, e) => Assert("ChosenRouting has been import", form.IsImporting);

				form.DialogResult = DialogResult.OK;
				form.Close();
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);

			AssertEquals(1, newVoyages.Length);

			using (var schedulesConsolsForm = (ZChildForm)Application.OpenForms.OfType<ISchedulesConsolsForm>().Single())
			{
				var multiDaysSelection = schedulesConsolsForm.BusinessEntity as RoutingMultiDaysSelection;

				var sailingsExpected = header.Lines.Count;
				AssertEquals("Precondition - incorrect number of sailings generated.", sailingsExpected, multiDaysSelection.SailingCollection.Count);

				for (int i = 0; i < header.Lines.Count; i++)
				{
					AssertSailing(header.Lines[i], departureDate, multiDaysSelection.SailingCollection[i]);
				}

				schedulesConsolsForm.Close();
			}
		}

		public void TestImportButton_ImportAndSelectOneRoute()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "MU712");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA712 should not exist.", 0, voyages.Length);

			var testMessageLine = "000 SYD HGH 10:45   11:20   20:05 MU           2018/06/22 2018/10/05 ..3.5.7 <SYD 1 HGH     11:20   20:05 MU   712    332     0  4849                                        ..3.5.7 18/06/22 18/10/05 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(!header.AnyLineHasMultipleAircraftTypes);

			var departureDate = new ZDateTime(2018, 6, 26);
			var mockSelectedDepartDateTime = new ZDateTime(2018, 06, 27);

			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "CNHGH",
						DepartureDate = departureDate
					}
				}
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.OpenedFromRoutingPlugin = true;

				form.MockSelectedDepartDateTime = mockSelectedDepartDateTime;

				form.Show();

				var list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				list.Add(header);
				form.FilterControl.FilteredGrid.Select(0);

				Assert("IsImporting should be false before importing.", !form.IsImporting);
				form.ImportButton.PerformClick();

				Assert("IsImporting should be false after importing is done.", !form.IsImporting);

				form.Close();
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "MU712");
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);

			AssertEquals(1, newVoyages.Length);
			var jobVoyage = (JobVoyage)(newVoyages[0]);
			AssertEquals(new ZDateTime(2018, 6, 27, 11, 20, 0), jobVoyage.JV_FlightDate);

			using (var schedulesConsolsForm = (ZChildForm)Application.OpenForms.OfType<ISchedulesConsolsForm>().Single())
			{
				var multiDaysSelection = schedulesConsolsForm.BusinessEntity as RoutingMultiDaysSelection;
				var sailingsExpected = header.Lines.Count;
				AssertEquals("Precondition - incorrect number of sailings generated.", sailingsExpected, multiDaysSelection.SailingCollection.Count);

				for (int i = 0; i < header.Lines.Count; i++)
				{
					AssertSailing(header.Lines[i], mockSelectedDepartDateTime, multiDaysSelection.SailingCollection[i]);
				}

				schedulesConsolsForm.Dispose();
			}
		}

		public void TestImportButton_ImportAndCancelSelection()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "MU712");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number MU712 should not exist.", 0, voyages.Length);

			var testMessageLine = "000 SYD HGH 10:45   11:20   20:05 MU           2018/06/22 2018/10/05 ..3.5.7 <SYD 1 HGH     11:20   20:05 MU   712    332     0  4849                                        ..3.5.7 18/06/22 18/10/05 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(!header.AnyLineHasMultipleAircraftTypes);

			var departureDate = new ZDateTime(2018, 6, 26);

			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "CNHGH",
						DepartureDate = departureDate
					}
				}
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.AllowMultipleSelectionForTest = false;

				form.MockSelectedDepartDateTime = ZDateTime.Empty;

				form.Show();

				var list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				list.Add(header);
				form.FilterControl.FilteredGrid.Select(0);

				Assert("IsImporting should be false before importing.", !form.IsImporting);
				form.ImportButton.PerformClick();
				Assert("IsImporting should be false after importing is done.", !form.IsImporting);

				form.Close();
			}

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "MU712");
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);

			AssertEquals(0, newVoyages.Length);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportButton_ImportAndSelectMultiRoutes()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "MU750");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number MU750 should not exist.", 0, voyages.Length);

			query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "QF5003");
			voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number QF5003 should not exist.", 0, voyages.Length);

			var testMessageLine = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/06 1..4.6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(!header.AnyLineHasMultipleAircraftTypes);
			AssertEquals("Precondition - header should contain a single transport line.", 1, header.Lines.Count);

			var departureDate = new ZDateTime(2018, 7, 6);

			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "CNWUH",
						DepartureDate = departureDate
					}
				}
			};

			var mockPossibleDepartureDates = new List<ZDateTime>()
			{
				new ZDateTime(2018, 7, 9),
				new ZDateTime(2018, 8, 9)
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.AllowMultipleSelectionForTest = true;

				form.MockPossbileDepartureDates = mockPossibleDepartureDates;

				form.Show();

				var list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				list.Add(header);
				form.FilterControl.FilteredGrid.Select(0);

				Assert("IsImporting should be false before importing.", !form.IsImporting);
				form.ImportButton.PerformClick();

				Assert("IsImporting should be false after importing is done.", !form.IsImporting);

				form.Close();
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "MU750");
			var newVoyages = newFactory.Load<JobVoyage>(newQuery).OrderBy(jv => jv.JV_FlightDate).ToArray();

			AssertEquals(2, newVoyages.Length);

			var orderedVoyages = newVoyages.OrderBy(voyage => voyage.JV_FlightDate).ToArray();

			AssertEquals(new ZDateTime(2018, 7, 9, 11, 20, 0), orderedVoyages[0].JV_FlightDate);
			AssertEquals(new ZDateTime(2018, 8, 9, 11, 20, 0), orderedVoyages[1].JV_FlightDate);

			using (var schedulesConsolsForm = (ZChildForm)Application.OpenForms.OfType<ISchedulesConsolsForm>().Single())
			{
				var multiDaysSelection = schedulesConsolsForm.BusinessEntity as RoutingMultiDaysSelection;
				var sailingsExpected = header.Lines.Count * mockPossibleDepartureDates.Count;
				var sailings = multiDaysSelection.SailingCollection;
				AssertEquals("Precondition - incorrect number of sailings generated.", sailingsExpected, sailings.Count);

				AssertSailing(header.Lines[0], mockPossibleDepartureDates[0], sailings[0]);
				AssertSailing(header.Lines[0], mockPossibleDepartureDates[1], sailings[1]);

				schedulesConsolsForm.Dispose();
			}
		}

		public void TestImportButton_ImportAndCancelMultiSelection()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "MU750");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number MU750 should not exist.", 0, voyages.Length);

			query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "QF5003");
			voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number QF5003 should not exist.", 0, voyages.Length);

			var testMessageLine1 = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/06 1..4.6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
			Assert(!header1.AnyLineHasMultipleAircraftTypes);

			var testMessageLine2 = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/06 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);
			Assert(!header1.AnyLineHasMultipleAircraftTypes);

			var departureDate = new ZDateTime(2018, 7, 6);

			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "CNWUH",
						DepartureDate = departureDate
					}
				}
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.AllowMultipleSelectionForTest = true;

				form.MockPossbileDepartureDates = null;

				form.Show();

				var list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				list.Add(header1);
				list.Add(header2);
				form.FilterControl.FilteredGrid.Select(1);

				Assert("IsImporting should be false before importing.", !form.IsImporting);
				form.ImportButton.PerformClick();

				Assert("IsImporting should be false after importing is done.", !form.IsImporting);

				form.Close();
			}

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "QF5003");
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);

			AssertEquals(0, newVoyages.Length);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportButton_Click_OneRouteHasBeenImported_WithMultipleAircraftTypes()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437  332/334   0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(header.AnyLineHasMultipleAircraftTypes);

			var departureDate = new ZDateTime(2018, 7, 10);

			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "HKHKG",
						DepartureDate = departureDate
					}
				},
				IncludeWeeklyTimetable = false
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.Show();

				var list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				list.Add(header);
				form.FilterControl.FilteredGrid.Select(0);

				Assert("IsImporting should be false before importing.", !form.IsImporting);
				form.ImportButton.PerformClick();
				Assert("IsImporting should be false after importing is done.", !form.IsImporting);

				form.Close();
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);

			AssertEquals(1, newVoyages.Length);

			Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Note there are multiple aircraft types listed against this flight number, please verify aircraft type with your airline."));
			using (var schedulesConsolsForm = (ZChildForm)Application.OpenForms.OfType<ISchedulesConsolsForm>().Single())
			{
				var multiDaysSelection = schedulesConsolsForm.BusinessEntity as RoutingMultiDaysSelection;

				var sailingsExpected = header.Lines.Count;
				AssertEquals("Precondition - incorrect number of sailings generated.", sailingsExpected, multiDaysSelection.SailingCollection.Count);

				for (int i = 0; i < header.Lines.Count; i++)
				{
					AssertSailing(header.Lines[i], departureDate, multiDaysSelection.SailingCollection[i]);
				}

				schedulesConsolsForm.Dispose();
			}
		}

		public void TestImportButton_Click_TwoRoutesHaveBeenImported()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			query.AddToFilter(JoinCondition.Or, JobVoyageSchema.JV_VoyageFlight, "BA9438");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437/BA9438 should not exist.", 0, voyages.Length);

			var testMessageLine1 = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
			var testMessageLine2 = "100 SYD HKG 11:15   08:00   16:15 QF CX        <SYD 3 MEL 1   08:00   09:35 BA  9438    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   09:50   16:15 BA  4139    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);

			var departureDate = new ZDateTime(2018, 7, 10);

			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "HKHKG",
						DepartureDate = departureDate
					}
				},
				IncludeWeeklyTimetable = false
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.Show();

				var list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				list.Add(header1);
				list.Add(header2);
				form.FilterControl.FilteredGrid.SelectAllElements();

				Assert("IsImporting should be false before importing.", !form.IsImporting);
				form.ImportButton.PerformClick();
				Assert("IsImporting should be false after importing is done.", !form.IsImporting);

				form.Close();
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			newQuery.AddToFilter(JoinCondition.Or, JobVoyageSchema.JV_VoyageFlight, "BA9438");
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);

			AssertEquals(2, newVoyages.Length);

			using (var schedulesConsolsForm = (ZChildForm)Application.OpenForms.OfType<ISchedulesConsolsForm>().Single())
			{
				var multiDaysSelection = schedulesConsolsForm.BusinessEntity as RoutingMultiDaysSelection;
				var sailingsExptected = header1.Lines.Count + header2.Lines.Count;
				var sailings = multiDaysSelection.SailingCollection;
				AssertEquals("Precondition - number of sailings imported is incorrect.", sailingsExptected, sailings.Count);

				AssertSailing(header1.Lines[0], departureDate, sailings[0]);
				AssertSailing(header1.Lines[1], departureDate, sailings[1]);
				AssertSailing(header2.Lines[0], departureDate, sailings[2]);
				AssertSailing(header2.Lines[1], departureDate, sailings[3]);

				schedulesConsolsForm.Dispose();
			}
		}

		void AssertSailing(RoutingResponseLine line, ZDateTime departureDate, JobSailing sailing)
		{
			var expectedOrigin = RoutingUpdaterHelper.GetUNLOCOFromIATACode(line.Origin, Factory);
			var expectedDest = RoutingUpdaterHelper.GetUNLOCOFromIATACode(line.Destination, Factory);
			var expectedDepartureDate = RoutingUpdaterHelper.UpdateDateTime(departureDate, line.DepartureTime);
			var expectedArrivalDate = RoutingUpdaterHelper.UpdateDateTime(departureDate, line.ArrivalTime);

			AssertEquals(expectedOrigin, sailing.JX_JA_RL_NKPortOfLoading);
			AssertEquals(expectedDest, sailing.JX_JB_RL_NKPortOfDischarge);
			AssertEquals(expectedDepartureDate, sailing.JX_JA_E_DEP);
			AssertEquals(expectedArrivalDate, sailing.JX_JB_E_ARV);
		}

		public void TestImportButton_Click_SingleSelectionButTwoRoutesSelected()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			query.AddToFilter(JoinCondition.Or, JobVoyageSchema.JV_VoyageFlight, "BA9438");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437/BA9438 should not exist.", 0, voyages.Length);

			var testMessageLine1 = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
			var testMessageLine2 = "100 SYD HKG 11:15   08:00   16:15 QF CX        <SYD 3 MEL 1   08:00   09:35 BA  9438    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   09:50   16:15 BA  4139    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);

			var departureDate = new ZDateTime(2016, 10, 4);

			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "HKHKG",
						DepartureDate = departureDate
					}
				}
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.Show();
				form.OpenedFromRoutingPlugin = true;

				var list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				list.Add(header1);
				list.Add(header2);
				form.FilterControl.FilteredGrid.SelectAllElements();

				form.ImportButton.PerformClick();
			}

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			newQuery.AddToFilter(JoinCondition.Or, JobVoyageSchema.JV_VoyageFlight, "BA9438");
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);

			AssertEquals("Please select only one route.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(0, newVoyages.Length);
		}

		public void TestImportButton_WhenEmptySelection()
		{
			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "HKHKG",
						DepartureDate = new ZDateTime(2016, 10, 4)
					}
				}
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.Show();
				AssertEquals("Nothing should be selected", true, form.FilterControl.FilteredGrid.SelectedElements.Cast<RoutingResponseHeader>().ToArray().IsNullOrEmpty());

				form.ImportButton.PerformClick();

				AssertEquals("Please select an item from the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region DoubleClick Headers

		public void TestDoubleClickHeaders()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Prerequisite: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(!header.AnyLineHasMultipleAircraftTypes);

			var departureDate = new ZDateTime(2016, 10, 4);

			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "HKHKG",
						DepartureDate = departureDate
					}
				}
			};

			using (var form = new RealTimeRoutingFormForTest(routingManager))
			{
				form.Show();

				var list = (RoutingResponseHeaderCollection)form.FilterControl.FilteredGrid.List;
				list.Add(header);
				form.FilterControl.FilteredGrid.Select(0);

				Assert("IsImporting should be false before double clicking header.", !form.IsImporting);
				form.FilterControl.FilteredGrid.PerformDoubleClickForTest();
				Assert("IsImporting should be false after double clicking header.", !form.IsImporting);

				form.Close();
			}

			voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("The voyage with flight number BA9437 should not exist since double-clicking won't create voyages.", 0, voyages.Length);
		}

		#endregion

		#region Import & Create MAWB

		public void TestImportAndCreateMAWBsButton_IsEnabled_WhenOpenedNotFromRoutingPlugin()
		{
			using (var form = new RealTimeRoutingFormForTest(new RoutingManager(Factory)))
			{
				form.Show();
				CombineAssertions(() =>
				{
					Assert("ImportAndCreateMAWBs Button should be visible when form is not opened through Routing Plugin.", form.ImportAndCreateMAWBsButtonForTest.Visible);
				});

				form.Close();
			}
		}

		public void TestImportAndCreateMAWBsButton_IsDisabled_WhenOpenedFromRoutingPlugin()
		{
			using (var form = new RealTimeRoutingFormForTest(new RoutingManager(Factory)))
			{
				form.OpenedFromRoutingPlugin = true;
				form.Show();
				CombineAssertions(() =>
				{
					Assert("ImportAndCreateMAWBs Button should not be visible when form is opened through Routing Plugin.", !form.ImportAndCreateMAWBsButtonForTest.Visible);
				});

				form.Close();
			}
		}

		public void TestImportAndCreateMAWBsButton_OpensFormWithProperDefaultValues()
		{
			var testMessageLine = "100 SYD HKG 11:15   07:00 1+15:15 QF CX KL     <SYD 1 CAN     10:45   18:30 KL  4406    330     0  4664                                        1234567 17/06/01 17/09/30 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);

			var departureDate = new ZDateTime(2018, 7, 10);
			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "HKHKG",
						DepartureDate = departureDate
					}
				}
			};

			using (var form = new RealTimeRoutingFormForTest2(routingManager))
			{
				routingManager.IncludeWeeklyTimetable = false;

				form.Show();
				form.FilterControl.FilteredGrid.List.Add(header);
				form.FilterControl.FilteredGrid.Select(0);
				form.ImportAndCreateMAWBsButtonForTest.PerformClick();

				using (var bulkConsolCreationForm = (ZChildForm)Application.OpenForms.OfType<IBulkConsolCreationForm>().Single())
				{
					var multiDaysSelectionData = GetMultiDaysSelectionData(bulkConsolCreationForm);
					CombineAssertions(() =>
					{
						AssertEquals("Airline Prefix should default from Carrier 1.", ZString.Empty, multiDaysSelectionData.ConsolDetails.AirlinePrefix);
						AssertEquals("Number of Consols per flight should be 1 by default.", (ZShort)1, multiDaysSelectionData.ConsolDetails.ConsolsPerFlight);
						AssertEquals("Number of Shipments should be 0 by default.", (ZShort)0, multiDaysSelectionData.ConsolDetails.Shipments);
						AssertEquals("Weight value should be 0 by default.", 0m, multiDaysSelectionData.ConsolDetails.Weight);
						AssertEquals("Volume value should be 0 by default.", 0m, multiDaysSelectionData.ConsolDetails.Volume);
						AssertEquals("Chargeable value should be 0 by default.", 0m, multiDaysSelectionData.ConsolDetails.Chargeable);
						Assert("AllocateNeutralMasterAutomatically should be unchecked by default.", !multiDaysSelectionData.ConsolDetails.AllocateNeutralMaster);
						Assert("HasChanges should be false when form is opened.", !multiDaysSelectionData.HasChanges);
					});
				}

				form.Close();
			}
		}

		public void TestImportAndCreateMAWBsButton_ForMultipleFlightSchedules()
		{
			var testMessageLine1 = "100 SYD HKG 11:15   07:00 1+15:15 QF CX KL     <SYD 1 CAN     10:45   18:30 KL  4406    330     0  4664                                        1234567 17/06/01 17/09/30 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);

			var testMessageLine2 = "100 SYD HKG 11:15   08:00   16:15 QF CX        <SYD 3 MEL 1   08:00   09:35 BA  9438    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   09:50   16:15 BA  4139    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);

			var departureDate = new ZDateTime(2018, 7, 10);
			var routingManager = new RoutingManager(Factory)
			{
				Requests = new[]
				{
					new RoutingRequest(Factory)
					{
						OriginUNLOCOCode = "AUSYD",
						DestinationUNLOCOCode = "HKHKG",
						DepartureDate = departureDate
					}
				}
			};

			using (var form = new RealTimeRoutingFormForTest2(routingManager))
			{
				routingManager.IncludeWeeklyTimetable = false;

				form.Show();
				form.FilterControl.FilteredGrid.List.Add(header1);
				form.FilterControl.FilteredGrid.List.Add(header2);
				form.FilterControl.FilteredGrid.Select(0);
				form.FilterControl.FilteredGrid.Select(1);
				form.ImportAndCreateMAWBsButtonForTest.PerformClick();

				Assert(form.AllowMultipleSelectionForTest);

				using (var bulkConsolCreationForm = (ZChildForm)Application.OpenForms.OfType<IBulkConsolCreationForm>().Single())
				{
					bulkConsolCreationForm.Close();
				}

				form.Close();
			}
		}

		#endregion

		public void TestPerformSearch_OriginAndDestinationAreBothZone()
		{
			var zone1 = Factory.NewWithValidTestData<RefZoneHeader>();
			zone1.FZ_Code = "AUXX";
			zone1.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Schedules;
			var zone2 = Factory.NewWithValidTestData<RefZoneHeader>();
			zone2.FZ_Code = "AUYY";
			zone2.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Schedules;
			Factory.Save();

			using (var form = new RealTimeRoutingFormForTest(new RoutingManager(Factory)))
			{
				form.Show();

				var originFilter = (ModuleNkFilter)form.Filter["Origin"];
				originFilter.IsActive = true;
				originFilter.Property = "AUXX";
				var destFilter = (ModuleNkFilter)form.Filter["Destination"];
				destFilter.IsActive = true;
				destFilter.Property = "AUYY";
				var departureFilter = (ModuleSingleDateFilter)form.Filter["Departure Date"];
				departureFilter.IsActive = true;
				departureFilter.Property1 = ZDateTime.Today;

				form.FilterControl.FirePerformSearch();
				AssertEquals("International Zone code can be used only in one of the location fields at a time, while the other has to contain UNLOCO.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new RealTimeRoutingForm(new RoutingManager(Factory));
		}

		MultiDaysSelection GetMultiDaysSelectionData(ZChildForm bulkConsolCreationForm)
		{
			return (MultiDaysSelection)bulkConsolCreationForm.BusinessEntity;
		}

		static ZChildForm GetSchedulesConsolsFormForTest(RoutingMultiDaysSelection multiDaysSelection)
		{
			return (ZChildForm)ObjectFactory.Get<ISchedulesConsolsForm>("ISchedulesConsolsForm", multiDaysSelection);
		}

		class RealTimeRoutingFormForTest : RealTimeRoutingFormWrapper
		{
			public RealTimeRoutingFormForTest(RoutingManager routingManager)
				: base(routingManager)
			{
			}

			public new ZButton ImportButton => base.ImportButton;

			protected override ZDateTime SelectDepartureDate(RoutingResponseHeader selectedHeader)
			{
				return MockSelectedDepartDateTime;
			}

			protected override BusinessObjectFactory GetNewFactory()
			{
				return BusinessEntity.Factory;
			}

			public ZDateTime MockSelectedDepartDateTime { get; set; }

			protected override IEnumerable<ZDateTime> SelectDepartureDates(RoutingMultiDaysSelection multiDaysSelection)
			{
				return MockPossbileDepartureDates;
			}

			public IEnumerable<ZDateTime> MockPossbileDepartureDates { get; set; }

			protected override DialogResult ShowSchedulesConsolsForm(RoutingMultiDaysSelection multiDaysSelection)
			{
				var form = GetSchedulesConsolsFormForTest(multiDaysSelection);
				form.Show();

				return DialogResult.OK;
			}
		}

		class RealTimeRoutingFormForTest2 : RealTimeRoutingFormWrapper
		{
			public RealTimeRoutingFormForTest2(RoutingManager routingManager)
				: base(routingManager)
			{
			}

			protected override IEnumerable<ZDateTime> ShowDialogAndGetResult(RoutingMultiDaysSelection multiDaysSelection)
			{
				var form = (ZChildForm)GetBulkConsolCreationForm(multiDaysSelection);

				form.Show();

				return multiDaysSelection.DepartureDates;
			}
		}
		#endregion
	}

	public class RealTimeRoutingFormWrapper : RealTimeRoutingForm
	{
		public RealTimeRoutingFormWrapper(RoutingManager routingManager)
				: base(routingManager)
		{
		}
		public ZButton ImportAndCreateMAWBsButtonForTest
		{
			get { return base.ImportAndCreateMAWBsButton; }
		}

		public bool AllowMultipleSelectionForTest
		{
			get { return base.AllowMultipleSelection; }
			set { base.AllowMultipleSelection = value; }
		}
	}
}

