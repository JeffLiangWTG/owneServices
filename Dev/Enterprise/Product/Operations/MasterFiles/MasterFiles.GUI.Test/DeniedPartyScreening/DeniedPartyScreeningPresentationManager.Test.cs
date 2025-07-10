using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;
using static Enterprise.Freight.Integration.CFS;
using static Enterprise.Integration.Forwarding;
using IForwardingConsol = Enterprise.Integration.Forwarding.IForwardingConsol;
using IForwardingShipment = Enterprise.Integration.Forwarding.IForwardingShipment;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class DeniedPartyPresentationManagerTest : TestCaseWithFactory
	{
		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery("delete from dbo.RefComplianceList");

			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}

		public void TestSaveResultStatusesOnlySaveOnce()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var resultStatuses = new List<DpsResultStatus>();
			resultStatuses.Add(new DpsResultStatus(consignmentHeader.Consignments.AddNew(), ScreeningStatusesList.Codes.Unknown));
			resultStatuses.Add(new DpsResultStatus(consignmentHeader.Consignments.AddNew(), ScreeningStatusesList.Codes.Unknown));
			var messageInfo = new DpsMessageInfo(new List<DpsSourceWithParties>(), false, 2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			new DeniedPartyScreeningPresentationManager().SaveResultStatuses(resultStatuses.ToArray(), messageInfo, newFactory);

			AssertEquals("Should only save once", 1, newFactory.SaveCount);
		}

		public void TestSaveResultStatusesSkipDeniedPartyStatusUpdatedLogForHVLVConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var resultStatuses = new List<DpsResultStatus>();
			resultStatuses.Add(new DpsResultStatus(consignmentHeader.Consignments.AddNew(), ScreeningStatusesList.Codes.Unknown));
			resultStatuses.Add(new DpsResultStatus(consignmentHeader.Consignments.AddNew(), ScreeningStatusesList.Codes.Unknown));
			var messageInfo = new DpsMessageInfo(new List<DpsSourceWithParties>(), false, 2);
			new DeniedPartyScreeningPresentationManager().SaveResultStatuses(resultStatuses.ToArray(), messageInfo, Factory);

			var hvcDPEQuery = new ZQuery(StmALogSchema.SL_Table, HVLVConsignmentSchema.Constants.TableName);
			hvcDPEQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdated.Code);

			var dpeLogsCount = Factory.Load<StmALog>(hvcDPEQuery).Length;

			AssertEquals("Should not create any DeniedPartyStatusUpdated log for HVLVConsignment", 0, dpeLogsCount);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateMenusExceptionOnNullArgument()
		{
			new DeniedPartyScreeningPresentationManager().CreateMenusForScreeningEntity(null);
		}

		public void TestCreateMenusForJob()
		{
			TestCreateMenusForJobCore(typeof(DummyWithQuotedBooking), expectedMenuAddedSuccessfully: true);
			TestCreateMenusForJobCore(typeof(DummyWithScreeningPartyProvider), expectedMenuAddedSuccessfully: true);
			TestCreateMenusForJobCore(typeof(DummyWithBothQuotedBookingAndComplianceRiskStatusProvider), expectedMenuAddedSuccessfully: true);
			TestCreateMenusForJobCore(typeof(DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider), expectedMenuAddedSuccessfully: true);

			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				TestCreateMenusForJobCore(typeof(DummyWithQuotedBooking), expectedMenuAddedSuccessfully: true);
				TestCreateMenusForJobCore(typeof(DummyWithScreeningPartyProvider), expectedMenuAddedSuccessfully: true);
				TestCreateMenusForJobCore(typeof(DummyWithBothQuotedBookingAndComplianceRiskStatusProvider), expectedMenuAddedSuccessfully: false);
				TestCreateMenusForJobCore(typeof(DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider), expectedMenuAddedSuccessfully: false);
			}
		}

		public void TestCreateMenusForJob_ThreeParameters()
		{
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(dummyForm, dummyForm.BusinessEntity, () => true);
				dummyForm.Show();
				var actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertNotNull("View Compliance Status menu item exist", actionsMenuItem.MenuItems.FindByText("View Compliance Status"));
				AssertNotNull("Resynchronize Screening Status menu item exist", actionsMenuItem.MenuItems.FindByText("Resynchronize Screening Status"));
			}
		}

		public void TestCreateMenusForJob_ViewComplianceStatusMenuItem_ScreeningNotEnabledMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(form, () => "test message");
				form.Show();
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("View Compliance Status").PerformClick();

				AssertEquals("test message", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestCreateModuleMenusForJob_ScreeningNotEnabledMessage()
		{
			using (var form = new Form())
			using (var module = new TestDummyModule(forJob: false, hasErrorMessage: true))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var resynchronizeMenuItem = module.FormActionMenu.FindByText("Resynchronize Screening Status", true);
				AssertNotNull("Resynchronize Screening Status menu item exist", resynchronizeMenuItem);

				resynchronizeMenuItem.PerformClick();
				AssertEquals("test message", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				var viewComplianceStatusMenuItem = module.FormActionMenu.FindByText("View Compliance Status", true);
				AssertNotNull("View Compliance Status menu item exist", viewComplianceStatusMenuItem);

				viewComplianceStatusMenuItem.PerformClick();
				AssertEquals("test message", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateModuleMenusExceptionOnNullArgument()
		{
			new DeniedPartyScreeningPresentationManager().CreateModuleMenusForJob(null, null);
		}

		[RequiresSTA]
		public void TestCreateModuleMenusForJob()
		{
			TestCreateModuleMenusForJobCore(typeof(DummyWithViewQuotedBooking),
				forBooking: true, expectedMenuAddedSuccessfully: true);
			TestCreateModuleMenusForJobCore(typeof(DummyWithScreeningPartyProvider),
				forBooking: false, expectedMenuAddedSuccessfully: true);
			TestCreateModuleMenusForJobCore(typeof(DummyWithBothViewQuotedBookingAndComplianceRiskStatusProvider),
				forBooking: true, expectedMenuAddedSuccessfully: true);
			TestCreateModuleMenusForJobCore(typeof(DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider),
				forBooking: false,	expectedMenuAddedSuccessfully: true);

			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				TestCreateModuleMenusForJobCore(typeof(DummyWithViewQuotedBooking),
					forBooking: true, expectedMenuAddedSuccessfully: true);
				TestCreateModuleMenusForJobCore(typeof(DummyWithScreeningPartyProvider),
					forBooking: false, expectedMenuAddedSuccessfully: true);
				TestCreateModuleMenusForJobCore(typeof(DummyWithBothViewQuotedBookingAndComplianceRiskStatusProvider),
					forBooking: true, expectedMenuAddedSuccessfully: false);
				TestCreateModuleMenusForJobCore(typeof(DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider),
					forBooking: false, expectedMenuAddedSuccessfully: false);
			}
		}

		public void TestReloadSafeShipNonPersistentBusinessObject()
		{
			var quotedBooking = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember("New",
				System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
				null, null, new object[] { Freight.Integration.QuoteBookingType.QuickBooking, Factory });
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			var newShipment = newFactory.Load<ForwardingShipment>(quotedBooking.ViewPK);
			newShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			newFactory.Save();

			(quotedBooking.ForwardingShipment as ForwardingShipment).JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var messageInfo = new DpsMessageInfo(null, true, 1);
			var resultStatus = new DpsResultStatus(newShipment, ScreeningStatusesList.Codes.Clear);

			AssertNoExceptionThrown(() => new DeniedPartyScreeningPresentationManagerForTest().ProcessScreenResultStatus_Exposed(quotedBooking.ForwardingShipment, resultStatus.Status, messageInfo));
		}

		public void TestPerformScreening_SyncDPELog()
		{
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
				var shipment = InitForwardingShipment(out var sourceBizOs);
				var shipmentBizo = shipment as BusinessObject;
				var latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
				AssertContains("|NEW=MAT|OLD=UNK|TYP=MAN", latestLog.SL_Reference);

				var parties = (shipment as IScreeningPartyProvider).ScreeningParties;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().PerformScreening(dummyForm, sourceBizOs.Cast<IDpsSourceWithParties>().ToList(), parties, false, false, false).GetAwaiter().GetResult();
				latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
				AssertContains("|NEW=CLR|OLD=MAT|TYP=MAN", latestLog.SL_Reference);
				AssertEquals("New Status: Clear, Old Status: Matched, Type: Manual Screen", latestLog.DisplayEventReference);
			}
		}

		[RequiresSTA]
		public void TestPerformScreening_SyncDPELogWithNoParties()
		{
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
				var shipment = InitForwardingShipment(out var sourceBizOs);
				var shipmentBizo = shipment as BusinessObject;
				var latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
				AssertContains("|NEW=MAT|OLD=UNK|TYP=MAN", latestLog.SL_Reference);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().PerformScreening(dummyForm, sourceBizOs.Cast<IDpsSourceWithParties>().ToList(), Array.Empty<ScreeningParty>(), false, false, false).GetAwaiter().GetResult();
				latestLog = shipmentBizo.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
				AssertEquals(@"There are no parties to screen for this record, either because all parties have already been screened or the selected parties are inactive.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("|NEW=CLR|OLD=MAT|TYP=MAN", latestLog.SL_Reference);
				AssertEquals("New Status: Clear, Old Status: Matched, Type: Manual Screen", latestLog.DisplayEventReference);
			}
		}

		IForwardingShipment InitForwardingShipment(out List<DpsSourceWithParties> sourceBizOs)
		{
			var shipment = Factory.New<IForwardingShipment>();
			var shipmentBizo = shipment as BusinessObject;
			sourceBizOs = new List<DpsSourceWithParties>
			{
				new DpsSourceWithParties(shipmentBizo, Array.Empty<ScreeningParty>())
			};

			var org = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OH_ExportBroker = org.PK;
			DpsWorkflowTrackingEvent.AddNew(shipmentBizo, ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Codes.Matched);
			Factory.Save();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();
			return shipment;
		}

		public void TestPerformScreening_DisagreeTerm()
		{
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

				new DeniedPartyScreeningPresentationManager().PerformScreening(dummyForm, null, Array.Empty<ScreeningParty>(), true, false, false).GetAwaiter().GetResult();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(TermsProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		public void TestPerformScreening_WithScreeningNotEnabledMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				AsyncTaskSynchronizer.Run(() => new DeniedPartyScreeningPresentationManager().PerformScreening(form, true, true, screeningNotEnabledMessage: () => "test message"));
				AssertEquals("test message", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestPerformScreening_SystemDefinedUnmatchedOrganization_ShouldShowMessage()
		{
			var complianceList = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList.RCL_IsExcluded = true;

			Factory.Save();

			var header = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);

			AssertEquals("Precondition : System Defined UNMATCHED Organization", "UNMATCHED", header.OH_Code);
			Assert("Precondition : UNMATCHED is system Defined", header.IsSystemDefinedOrganisation);

			using (var form = new ZOrganisationsForm(header))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var manager = new DeniedPartyScreeningPresentationManager();
				manager.CreateMenusForScreeningEntity(form);

				var menuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Screen (Full List)");
				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals("Unmatched Organizations are not included in Denied Party Screening and will not be screened. Please create a new Organization or replace with an existing Organization. Unmatched Organizations will remain Not Screened", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				menuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Screen");
				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals("Unmatched Organizations are not included in Denied Party Screening and will not be screened. Please create a new Organization or replace with an existing Organization. Unmatched Organizations will remain Not Screened", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPerformScreening_AgreeTerm()
		{
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().PerformScreening(dummyForm, null, Array.Empty<ScreeningParty>(), false, false, false).GetAwaiter().GetResult();
				AssertEquals(@"There are no parties to screen for this record, either because all parties have already been screened or the selected parties are inactive.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().PerformScreening(dummyForm, null, new ScreeningParty[] { ScreeningParty1 }, false, false, false).GetAwaiter().GetResult();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(PartyComplianceForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		public void TestPerformScreening_WithNoCandidates()
		{
			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().PerformScreening(dummyForm, null, Array.Empty<ScreeningParty>(), false, false, false).GetAwaiter().GetResult();

				AssertContains("There are no parties to screen for this record, either because all parties have already been screened or the selected parties are inactive.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(TermsProgressForm), ZFormModaliser.LastFormShownForTest.GetType());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().PerformScreening(dummyForm, null, new ScreeningParty[] { ScreeningParty1 }, false, false, false).GetAwaiter().GetResult();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(PartyComplianceForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		public void TestSaveResultStatus()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "DUMMYFORTEST";
			Factory.Save();
			AssertSaveResultStatus(orgHeader, ScreeningStatusesList.Codes.Matched, "|NEW=MAT|OLD=NOT|TYP=MAN", "NOT", "MAT");

			((IScreeningPartyProvider)orgHeader).ScreeningStatus = "MAT";
			Factory.Save();
			AssertSaveResultStatus(orgHeader, ScreeningStatusesList.Codes.Unknown, "|NEW=UNK|OLD=MAT|TYP=MAN", "MAT", "UNK");

			((IScreeningPartyProvider)orgHeader).ScreeningStatus = "UNK";
			Factory.Save();
			AssertSaveResultStatus(orgHeader, ScreeningStatusesList.Codes.Clear, "|NEW=CLR|OLD=UNK|TYP=MAN", "UNK", "CLR");

			((IScreeningPartyProvider)orgHeader).ScreeningStatus = "UNK";
			Factory.Save();
			AssertSaveResultStatus(orgHeader, ScreeningStatusesList.Codes.PermanentClear, "|NEW=CLP|OLD=UNK|TYP=MAN", "UNK", "CLP");

			((IScreeningPartyProvider)orgHeader).ScreeningStatus = "UNK";
			Factory.Save();
			AssertSaveResultStatus(orgHeader, ScreeningStatusesList.Codes.JobCleared, "|NEW=JCL|OLD=UNK|TYP=MAN", "UNK", "JCL");

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			Factory.Save();
			// For jobs, will record the final calculated screening status rather than use the status string.
			AssertSaveResultStatus(declaration, ScreeningStatusesList.Codes.Matched, "|NEW=CLR|OLD=NOT|TYP=MAN", "NOT", "CLR");
		}

		public void TestSaveResultStatus_TransportsWithTheSameVesselGetStatusAllUpdatedIfScreened()
		{
			var factory = new BusinessObjectFactory();
			var vessel = factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "vessel";
			var consol = factory.New<ForwardingConsol>();
			factory.Save();
			var transport1 = consol.Transports.AddNew();
			transport1.JW_Vessel = "vessel";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_Vessel = "vessel";

			factory.Save();

			var parties = new ScreeningParty[]
			{
				new ScreeningParty(transport1, string.Empty, transport1),
				new ScreeningParty(transport2, string.Empty, transport2 ),
			};

			var resultStatuses = new DpsResultStatus[] { new DpsResultStatus(transport1, ScreeningStatusesList.Codes.Matched) };
			var list = DpsSourceWithParties.GetSingleSourceList(consol, parties)?.Cast<DpsSourceWithParties>().ToList();
			var messageInfo = new DpsMessageInfo(list, false, 0);

			new DeniedPartyScreeningPresentationManagerForTest().SaveResultStatuses_Exposed(resultStatuses, messageInfo, new BusinessObjectFactory());

			AssertEquals(ScreeningStatusesList.Codes.Matched, transport1.JW_VesselScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.Matched, transport2.JW_VesselScreeningStatus);
		}

		void AssertSaveResultStatus(BusinessObject businessObject, string screeningStatus, string expectedReferenceStatus, string oldStatus, string newStatus)
		{
			var resultStatuses = new DpsResultStatus[] { new DpsResultStatus(businessObject, screeningStatus) };
			new DeniedPartyScreeningPresentationManagerForTest().SaveResultStatuses_Exposed(resultStatuses, null, new BusinessObjectFactory());
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var result = businessObject.GetLogs().Find(query);
			Assert("Denied Party Events exists with reference status: " + expectedReferenceStatus, result.Where(x => x.SL_Reference.Contains(expectedReferenceStatus)).Any());
			Assert("Denied Party Events contains correct old and new values ", result.Where(x => x.Parameters[Params.Codes.Old] == oldStatus && x.Parameters[Params.Codes.New] == newStatus).Any());
		}

		public void TestMacroDisplayEventReference()
		{
			AssertMacroDisplayEventReference(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.NotScreened, "New Status: Not Screened, Old Status: Clear, Type: Manual Screen");
			AssertMacroDisplayEventReference(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Codes.Matched, "New Status: Matched, Old Status: Matched, Type: Manual Screen");
			AssertMacroDisplayEventReference(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Clear, "New Status: Clear, Old Status: Not Screened, Type: Manual Screen");
			AssertMacroDisplayEventReference(ScreeningStatusesList.Codes.JobCleared, ScreeningStatusesList.Codes.Unknown, "New Status: Unknown, Old Status: Job Cleared, Type: Manual Screen");
			AssertMacroDisplayEventReference(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.JobCleared, "New Status: Job Cleared, Old Status: Clear, Type: Manual Screen");

			var org = Factory.New<OrgHeader>();
			org.GetLogs().AddNew(Events.DeniedPartyStatusUpdated, "ABC", new Dictionary<string, string>()
			{
				[Params.Codes.New] = "DUMMY",
				[Params.Codes.Old] = "DUMMY"
			}.ToArray());

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var result = org.GetLogs().Find(query);

			org = Factory.New<OrgHeader>();
			org.GetLogs().AddNew(Events.DeniedPartyStatusUpdated, "ABC");
			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			result = org.GetLogs().Find(query);
			AssertEquals("ABC", result.Single().DisplayEventReference);
		}

		public void TestIsCurrentScreeningStatusValid_WhenNotScreening()
		{
			using (new DeniedPartyScreeningHttpServiceForTest(false))
			{
				AssertIsCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Unknown, false, isScreeningEntity: false);
				AssertIsCurrentScreeningStatusValid(ScreeningStatusesList.Codes.NotScreened, false, isScreeningEntity: false);
				AssertIsCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Canceled, true, isScreeningEntity: false);
				AssertIsCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Clear, true, isScreeningEntity: false);
				AssertIsCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Matched, true, isScreeningEntity: false);
			}
		}

		public void TestIsCurrentScreeningStatusValid_WhenScreening()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			{
				AssertIsCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Unknown, false, isScreeningEntity: true);
				AssertIsCurrentScreeningStatusValid(ScreeningStatusesList.Codes.NotScreened, false, isScreeningEntity: true);
				AssertIsCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Canceled, false, isScreeningEntity: true);
				AssertIsCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Clear, false, isScreeningEntity: true);
				AssertIsCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Matched, false, isScreeningEntity: true);
			}
		}

		void AssertIsCurrentScreeningStatusValid(string status, bool expectedIsCurrentScreeningStatusValid, bool isScreeningEntity)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_ScreeningStatus = status;
			var party = new ScreeningParty(Parent, "OrgHeader", orgHeader);

			Factory.Save();

			var sourceBizOs = new List<IDpsSourceWithParties>()
			{
				new DpsSourceWithParties(orgHeader, new[] { new ScreeningParty(orgHeader, orgHeader.HumanReadableName, orgHeader) })
			};

			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				AsyncTaskSynchronizer.Run(() => new DeniedPartyScreeningPresentationManager().PerformScreening(dummyForm, sourceBizOs, new[] { party }, isScreeningEntity, false, true));
			}

			AssertEquals(expectedIsCurrentScreeningStatusValid, party.IsCurrentScreeningStatusValid);
		}

		public void TestPartyComplianceForm()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			var unknownParty = new ScreeningParty(parent, "", unknownHeader);
			unknownParty.IsCurrentScreeningStatusValid = true;

			var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
			matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var matchedParty = new ScreeningParty(parent, "", matchedHeader);
			matchedParty.IsCurrentScreeningStatusValid = true;

			var screeningParties = new[]
			{
				unknownParty, matchedParty
			};

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			{
				using (var form = new DeniedPartyScreeningPresentationManagerForTest().GetPartyComplianceForm_Exposed(screeningParties, null))
				{
					form.Show();
					(form.Controls.Find("CloseButton", true)[0] as ZButton).PerformClick();
					AssertEquals(false, unknownParty.IsCurrentScreeningStatusValid);
					AssertEquals(true, matchedParty.IsCurrentScreeningStatusValid);
				}

				using (var form = new DeniedPartyScreeningPresentationManagerForTest().GetPartyComplianceForm_Exposed(screeningParties, null))
				{
					form.Show();
					(form.Controls.Find("ScreenButton", true)[0] as ZButton).PerformClick();
					AssertEquals(false, unknownParty.IsCurrentScreeningStatusValid);
					AssertEquals(true, matchedParty.IsCurrentScreeningStatusValid);
				}

				using (var form = new DeniedPartyScreeningPresentationManagerForTest().GetPartyComplianceForm_Exposed(screeningParties, null))
				{
					form.Show();
					(form.Controls.Find("ForceRescreenButton", true)[0] as ZButton).PerformClick();
					AssertEquals(false, unknownParty.IsCurrentScreeningStatusValid);
					AssertEquals(false, matchedParty.IsCurrentScreeningStatusValid);
				}
			}
		}

		public void TestPartyComplianceForm_SelectedScreen()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();

			var unknownHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var unknownHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var unknownParty1 = new ScreeningParty(parent, "", unknownHeader1);
			unknownParty1.IsCurrentScreeningStatusValid = true;

			var unknownParty2 = new ScreeningParty(parent, "", unknownHeader2);
			unknownParty2.IsCurrentScreeningStatusValid = true;

			var matchedHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			matchedHeader1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var matchedParty1 = new ScreeningParty(parent, "", matchedHeader1);
			matchedParty1.IsCurrentScreeningStatusValid = true;

			var matchedHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			matchedHeader2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var matchedParty2 = new ScreeningParty(parent, "", matchedHeader2);
			matchedParty2.IsCurrentScreeningStatusValid = true;

			var permanentClearHeader = Factory.NewWithValidTestData<OrgHeader>();
			permanentClearHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			var permanentClearParty = new ScreeningParty(parent, "", permanentClearHeader);
			permanentClearParty.IsCurrentScreeningStatusValid = true;

			var screeningParties = new[]
			{
				unknownParty1, unknownParty2, matchedParty1, matchedParty2, permanentClearParty
			};

			var isRescreen = true;
			Task Actions(bool u)
			{
				isRescreen = u;
				return Task.CompletedTask;
			}

			using (var form = new DeniedPartyScreeningPresentationManagerForTest().GetPartyComplianceForm_Exposed(screeningParties, Actions))
			{
				form.Show();
				form.SelectedParties = new[] { unknownParty1 };
				form.PerformButtonClick = PartyComplianceButton.Screen;
				form.Close();
				AssertContainsExactElementsInExactOrder(new[] { false, true, true, true, true }, screeningParties.Select(u => u.IsCurrentScreeningStatusValid));
				AssertEquals(false, isRescreen);
			}

			using (var form = new DeniedPartyScreeningPresentationManagerForTest().GetPartyComplianceForm_Exposed(screeningParties, Actions))
			{
				form.Show();
				form.SelectedParties = new[] { matchedParty1, permanentClearParty };
				form.PerformButtonClick = PartyComplianceButton.ForceRescreen;
				form.Close();
				AssertContainsExactElementsInExactOrder(new[] { true, true, false, true, true }, screeningParties.Select(u => u.IsCurrentScreeningStatusValid));
				AssertEquals(true, isRescreen);
			}
		}

		public void TestSaveResultStatus_WithDeletedBizo()
		{
			AssertNoExceptionThrown(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				orgHeader.Delete();
				var manager = new DeniedPartyScreeningPresentationManagerForTest();
				manager.SaveResultStatuses_Exposed(new[] { new DpsResultStatus(orgHeader, ScreeningStatusesList.Codes.Clear) }, null, Factory);
			});
		}

		public void TestSaveResultStatus_WithNonIScreeningPartyProvider()
		{
			AssertNoExceptionThrown(() =>
			{
				var loadListConsol = Factory.New<ICFSLoadListConsol>() as BusinessObject;
				Factory.Save();
				var manager = new DeniedPartyScreeningPresentationManagerForTest();
				manager.SaveResultStatuses_Exposed(new[] { new DpsResultStatus(loadListConsol, ScreeningStatusesList.Codes.Clear) }, null, Factory);
			});
		}

		public void TestSaveScreenResultStatus_WithSaveConcurrency()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_ActualVolume = 100;
			shipment.JS_ScreeningStatus = "MAT";
			Factory.Save();

			var concurrencyFactory = new BusinessObjectFactory();
			concurrencyFactory.RefreshEnabled = false;
			var concurrencyShipment = concurrencyFactory.Load<IForwardingShipment>(shipment.PK);
			concurrencyShipment.JS_ActualVolume = 200;
			concurrencyFactory.Save();

			var resultStatus = new DpsResultStatus(shipment as BusinessObject, "CLR");
			var messageInfo = new DpsMessageInfoForTest(null, true, 1);
			var bizo = shipment as BusinessObject;
			new DeniedPartyScreeningPresentationManagerForTest().SaveResultStatuses_Exposed(new[] { resultStatus }, messageInfo, Factory);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var logs = bizo.GetLogs().Find(query);
			CombineAssertions(() =>
			{
				AssertEquals("MAT", shipment.JS_ScreeningStatus);
				AssertEquals(200M, shipment.JS_ActualVolume);
				AssertEquals(1, logs.Length);
				AssertEquals(1, messageInfo.FinalStatusList_Exposed.Count);
			});
		}

		public void TestSaveScreenResultStatus_ShouldNotThrowSaveConcurrencyException()
		{
			var consol = Factory.New<IForwardingConsol>();
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			var newConsol = newFactory.Load<IForwardingConsol>(consol.PK);
			newConsol.JK_MasterBillIssueDate = ZDateTime.SmallDateTimeNow;

			newFactory.Save();

			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var messageInfo = new DpsMessageInfo(null, true, 1);
			var resultStatus = new DpsResultStatus(shipment as BusinessObject, ScreeningStatusesList.Codes.Clear);

			AssertNoExceptionThrown(() => new DeniedPartyScreeningPresentationManagerForTest().ProcessScreenResultStatus_Exposed(shipment as BusinessObject, resultStatus.Status, messageInfo));
		}

		public void TestSaveScreenResultStatus_ShouldNotThrowCannotSaveException()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			Factory.Save();

			BusinessObjectFactory.SavingEventHandler onSavingHandler = f =>
			{
				throw new ZCannotSaveException("Test Cannot Save Exception", "");
			};
			Factory.Saving += onSavingHandler;
			try
			{
				var messageInfo = new DpsMessageInfo(null, true, 1);
				var resultStatus = new DpsResultStatus(shipment as BusinessObject, ScreeningStatusesList.Codes.Clear);

				AssertNoExceptionThrown(() => new DeniedPartyScreeningPresentationManagerForTest().ProcessScreenResultStatus_Exposed(shipment as BusinessObject, resultStatus.Status, messageInfo));
			}
			finally
			{
				Factory.Saving -= onSavingHandler;
			}
		}

		public void TestSaveCustomFieldsDoesNotThrowException()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var customValue1 = Factory.New<GenCustomAddOnValue>();
			customValue1.XV_ParentID = shipment.PK;
			customValue1.XV_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			customValue1.XV_Name = "Test";
			customValue1.XV_Type = "STR";
			customValue1.XV_Data = "abc";
			Factory.Save();

			AssertNotEquals("Precondition: ", ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);

			var customValue2 = shipment.Factory.New<GenCustomAddOnValue>();
			customValue2.XV_ParentID = shipment.PK;
			customValue2.XV_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			customValue2.XV_Name = "Test";
			customValue2.XV_Type = "STR";
			customValue2.XV_Data = "123";

			var resultStatus = new DpsResultStatus(shipment, ScreeningStatusesList.Codes.Clear);
			var messageInfo = new DpsMessageInfo(null, true, 1);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() =>
				{
					new DeniedPartyScreeningPresentationManagerForTest().SaveResultStatuses_Exposed(new[] { resultStatus }, messageInfo, Factory);
				});
				AssertNull(Factory.Load<GenCustomAddOnValue>(customValue1.PK));
				AssertNotNull(Factory.Load<GenCustomAddOnValue>(customValue2.PK));
				AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
			});
		}

		public void TestSaveScreenResultStatus_EntityTypeIsTransport()
		{
			var consol = Factory.New<IForwardingConsol>();
			var transport = Factory.New<ITransport>();
			transport.ParentType = consol.GetType();
			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();

			AssertEquals("Pre-Condition:", ScreeningStatusesList.Codes.NotScreened, ((IScreeningPartyForVessel)transport).CurrentScreeningStatus);
			var resultStatus = new DpsResultStatus(transport as BusinessObject, "CLR");
			var sourceBizOs = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties((transport as BusinessObject), new [] { new ScreeningParty((transport as BusinessObject),"Transport", transport as IScreeningPartyForVessel) })
			};

			var messageInfo = new DpsMessageInfo(sourceBizOs, true, 1);
			new DeniedPartyScreeningPresentationManagerForTest().ProcessScreenResultStatus_Exposed(transport as BusinessObject, resultStatus.Status, messageInfo);
			AssertEquals(ScreeningStatusesList.Codes.Clear, ((IScreeningPartyForVessel)transport).CurrentScreeningStatus);
		}

		public void TestOrgChange_WhenOrgUNKWhenVesselCLR_VesselToUNK()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG1";
			orgHeader.OH_FullName = "Organization For Test";
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_OH = orgHeader.PK;
			refVessel.RV_Code = "TESTV";
			Factory.Save();

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			refVessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			UpdateRelatedJobs(orgHeader);

			var query = new ZDBOnlyQuery(typeof(RefVessel));
			query.AddToFilter(RefVesselSchema.PK, refVessel.PK);
			query.ReLoadExistingRows = true;

			var loadedVessel = Factory.LoadTop1<RefVessel>(query);

			AssertEquals("UNK", loadedVessel.RV_ScreeningStatus);
		}

		public void TestOrgChange_WhenOrgMATWhenVesselCLR_VesselToMAT()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG1";
			orgHeader.OH_FullName = "Organization For Test";
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_OH = orgHeader.PK;
			refVessel.RV_Code = "TESTV";
			Factory.Save();

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			refVessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			UpdateRelatedJobs(orgHeader);

			var query = new ZDBOnlyQuery(typeof(RefVessel));
			query.AddToFilter(RefVesselSchema.PK, refVessel.PK);
			query.ReLoadExistingRows = true;

			var loadedVessel = Factory.LoadTop1<RefVessel>(query);

			AssertEquals("MAT", loadedVessel.RV_ScreeningStatus);
		}

		public void TestUpdateRelatedJobsCallJobsForVessel()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG1";
			orgHeader.OH_FullName = "Organization For Test";
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_OH = orgHeader.PK;
			refVessel.RV_Code = "TESTV";
			Factory.Save();

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			refVessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			var consol = Factory.New<IForwardingConsol>();
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var transport = consol.Transports_AddNew();
			transport.JW_Vessel = refVessel.RV_Code;
			transport.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Clear;

			Factory.Save();
			AssertEquals("Precondition", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);

			UpdateRelatedJobs(orgHeader);

			var query = new ZDBOnlyQuery(typeof(IForwardingConsol));
			query.AddToFilter(JobConsolSchema.PK, consol.PK);
			query.ReLoadExistingRows = true;

			AssertEquals("Consol's Screening Status", ScreeningStatusesList.Codes.Matched, Factory.LoadTop1<IForwardingConsol>(query).JK_ScreeningStatus);
		}

		public void TestUpdateVesselToCLRWhenLastScreeningLogsIsDPC()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG1";
			orgHeader.OH_FullName = "Organization For Test";
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_OH = orgHeader.PK;
			refVessel.RV_Code = "TESTV";
			Factory.Save();

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			refVessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			Factory.Save();

			var statusCollection = new StmEntityScreeningLogCollection(refVessel);
			var newLog = statusCollection.AddNew();
			newLog.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedClear;
			newLog.PJ_ParentID = refVessel.PK;
			Factory.Save();

			UpdateRelatedJobs(orgHeader);

			var query = new ZDBOnlyQuery(typeof(RefVessel));
			query.AddToFilter(RefVesselSchema.PK, refVessel.PK);
			query.ReLoadExistingRows = true;

			var loadedVessel = Factory.LoadTop1<RefVessel>(query);
			AssertEquals(ScreeningStatusesList.Codes.Clear, loadedVessel.RV_ScreeningStatus);
		}

		public void TestUpdateRelatedJobs_ShipmentWithDeclarationRegardlessOverrideOrNot()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TESTORG1";
			orgHeader1.OH_FullName = "Organization For Test 1";

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TESTORG2";
			orgHeader2.OH_FullName = "Organization For Test 2";

			Factory.Save();

			var shipment = Factory.New<IForwardingShipment>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_OH_Supplier = orgHeader1.PK;
			shipment.JS_OH_ImportBroker = orgHeader2.PK;

			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			orgHeader1.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			orgHeader2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			UpdateRelatedJobs(orgHeader1);

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
			declaration = newFactory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(declaration.PK);
			AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
			AssertEquals("'Override Freight Defaults' declaration has same screening status with shipment", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);

			declaration.JE_OverrideFreightDefaults = false;
			declaration.JE_OH_Supplier = orgHeader1.PK;
			newFactory.Save();

			UpdateRelatedJobs(orgHeader1);

			newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
			declaration = newFactory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(declaration.PK);
			AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
			AssertEquals("Not 'Override Freight Defaults' declaration has same screening status with shipment", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
		}

		public void TestNotUpdateVesselToCLRWhenLastScreeningLogsIsNotDPC()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG1";
			orgHeader.OH_FullName = "Organization For Test";
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_OH = orgHeader.PK;
			refVessel.RV_Code = "TESTV";
			Factory.Save();

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			refVessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			var statusCollection = new StmEntityScreeningLogCollection(refVessel);
			var newLog1 = statusCollection.AddNew();
			newLog1.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedClear;
			newLog1.PJ_ParentID = refVessel.PK;

			var newLog2 = statusCollection.AddNew();
			newLog2.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty;
			newLog2.PJ_ParentID = refVessel.PK;

			var newLog3 = statusCollection.AddNew();
			newLog3.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.UpdateRelatedJobs;
			newLog3.PJ_ParentID = refVessel.PK;

			var newLog4 = statusCollection.AddNew();
			newLog4.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled;
			newLog4.PJ_ParentID = refVessel.PK;

			Factory.Save();

			UpdateRelatedJobs(orgHeader);

			var query = new ZDBOnlyQuery(typeof(RefVessel));
			query.AddToFilter(RefVesselSchema.PK, refVessel.PK);
			query.ReLoadExistingRows = true;

			var loadedVessel = Factory.LoadTop1<RefVessel>(query);
			AssertEquals(ScreeningStatusesList.Codes.Matched, loadedVessel.RV_ScreeningStatus);
		}

		public void TestCreateMenusForScreeningEntityWithInactiveOrg()
		{
			var complianceList = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList.RCL_IsExcluded = true;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsActive = false;
			Factory.Save();

			var manager = new DeniedPartyScreeningPresentationManager();
			var tmpSecurityCore = GetTemporarySecurityCore();
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (var form = new ZOrganisationsForm(orgHeader))
			{
				manager.CreateMenusForScreeningEntity(form);
				var menu1 = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Screen (Full List)");
				AssertNotNull(menu1);

				var menu2 = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Screen");
				AssertNotNull(menu2);

				Env.Security.DpsAllowScreening.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var expectedMessage = string.Format(CultureInfo.InvariantCulture, "The selected {0} is inactive and its screening status cannot be modified.", orgHeader.HumanReadableName);

				menu1.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				menu2.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestScreeningJob_WhenSystemDefinedUnmatchedOrganization_ShouldShowInViewComplianceGrid()
		{
			var header = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = header.MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.DocAddresses.Add(jobDocAddress);

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(form);
				form.Show();
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("View Compliance Status").PerformClick();

				var unprocessedGrid = OpenedPartyComplianceForm.Controls.Find("UnprocessedWrappersGrid", true).Single() as ZGrid;
				var expectedWarning = "Unmatched Organizations are not included in Denied Party Screening and will not be screened. Please create a new Organization or replace with an existing Organization. Unmatched Organizations will remain Not Screened";
				AssertEquals(1, unprocessedGrid.List.Count);

				var unprocessedParty = unprocessedGrid.List[0] as PartyComplianceWrapper;
				AssertEquals(header.OH_Code, unprocessedParty.OrgCode);
				AssertEquals(true, unprocessedParty.HasRowWarnings);
				AssertEquals(expectedWarning, unprocessedParty.RowNotifications.First().Message);
			}
		}

		public void TestScreeningJob_WhenOneOrMoreScreeningPartiesAreInactive()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			org2.OH_Code = "ORG2";
			org1.OH_FullName = "ORGTEST1";
			org2.OH_FullName = "ORGTEST2";
			org1.OH_IsActive = true;
			org2.OH_IsActive = false;

			var job1DocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var job2DocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			job1DocAddress.E2_OA_Address = org1.MainAddress.PK;
			job2DocAddress.E2_OA_Address = org2.MainAddress.PK;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Code = "VESSEL1";
			vessel2.RV_Code = "VESSEL2";
			vessel1.RV_LloydsNumber = "1234567";
			vessel2.RV_LloydsNumber = "9308390";
			vessel1.RV_IsActive = true;
			vessel2.RV_IsActive = false;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.DocAddresses.Add(job1DocAddress);
			shipment.DocAddresses.Add(job2DocAddress);

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ETD = new ZDateTime(2021, 01, 23, 7, 35, 00);
			transport1.JW_ETA = new ZDateTime(2021, 01, 25, 15, 55, 00);
			transport1.JW_Vessel = vessel1.RV_Code;
			transport1.JW_VoyageFlight = "Voyage1";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_LegOrder = 1;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "AUBTB";
			transport2.JW_ETD = new ZDateTime(2021, 02, 01, 7, 35, 00);
			transport2.JW_ETA = new ZDateTime(2021, 02, 05, 15, 55, 00);
			transport2.JW_Vessel = vessel2.RV_Code;
			transport2.JW_VoyageFlight = "Voyage2";

			Factory.Save();

			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var form = new ZForm(shipment))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals("Org1 screening status", ScreeningStatusesList.Codes.NotScreened, org1.OH_ScreeningStatus);
					AssertEquals("Org2 screening status", ScreeningStatusesList.Codes.NotScreened, org2.OH_ScreeningStatus);
					AssertEquals("Vessel1 screening status", ScreeningStatusesList.Codes.NotScreened, vessel1.RV_ScreeningStatus);
					AssertEquals("Vessel2 screening status", ScreeningStatusesList.Codes.Unknown, vessel2.RV_ScreeningStatus);
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(form);
				form.Show();
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("View Compliance Status").PerformClick();

				var screenButton = OpenedPartyComplianceForm.Controls.Find("ScreenButton", true)[0] as ZButton;
				screenButton.PerformClick();

				CombineAssertions("After screening", () =>
				{
					AssertEquals("Org1 screening status", ScreeningStatusesList.Codes.Clear, org1.OH_ScreeningStatus);
					AssertEquals("Org2 screening status", ScreeningStatusesList.Codes.NotScreened, org2.OH_ScreeningStatus);
					AssertEquals("Vessel1 screening status", ScreeningStatusesList.Codes.Clear, vessel1.RV_ScreeningStatus);
					AssertEquals("Vessel2 screening status", ScreeningStatusesList.Codes.Unknown, vessel2.RV_ScreeningStatus);
				});
			}
		}

		public void TestScreeningJob_WhenVesselIsInactive_ShowInGrid()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "Vessel";
			vessel.RV_LloydsNumber = "1234567";
			vessel.RV_IsActive = false;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var transport = shipment.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_ETD = new ZDateTime(2021, 01, 23, 7, 35, 00);
			transport.JW_ETA = new ZDateTime(2021, 01, 25, 15, 55, 00);
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "Voyage";

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(form);
				form.Show();
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("View Compliance Status").PerformClick();

				var unprocessedWrappersGrid = OpenedPartyComplianceForm.Controls.Find("UnprocessedWrappersGrid", true).Single() as ZGrid;
				AssertEquals("There is one element in the grid", 1, unprocessedWrappersGrid.List.Count);
			}
		}

		public void TestScreeningJob_WhenVesselIsInactive_ShowRowWarning()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Code = "Vessel 1";
			vessel2.RV_Code = "Vessel 2";
			vessel1.RV_LloydsNumber = "1234567";
			vessel2.RV_LloydsNumber = "7654321";
			vessel1.RV_IsActive = true;
			vessel2.RV_IsActive = false;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ETD = new ZDateTime(2021, 01, 23, 7, 35, 00);
			transport1.JW_ETA = new ZDateTime(2021, 01, 25, 15, 55, 00);
			transport1.JW_Vessel = vessel1.RV_Code;
			transport1.JW_VoyageFlight = "Voyage 1";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_LegOrder = 1;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "AUBTB";
			transport2.JW_ETD = new ZDateTime(2021, 02, 01, 7, 35, 00);
			transport2.JW_ETA = new ZDateTime(2021, 02, 05, 15, 55, 00);
			transport2.JW_Vessel = vessel2.RV_Code;
			transport2.JW_VoyageFlight = "Voyage 2";

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(form);
				form.Show();
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("View Compliance Status").PerformClick();

				var unprocessedWrappersGrid = OpenedPartyComplianceForm.Controls.Find("UnprocessedWrappersGrid", true).Single() as ZGrid;
				AssertEquals("There are two elements in the grid", 2, unprocessedWrappersGrid.List.Count);

				var org1PartyStatusWrapper = unprocessedWrappersGrid.List[0] as PartyComplianceWrapper;
				var org2PartyStatusWrapper = unprocessedWrappersGrid.List[1] as PartyComplianceWrapper;

				CombineAssertions("After screening", () =>
				{
					AssertEquals("The party compliance wrapper is created from vessel1", "Vessel 1: Vessel", org1PartyStatusWrapper.ParentsDescription);
					AssertNoRowWarnings(org1PartyStatusWrapper);

					AssertEquals("The party compliance wrapper is created from vessel2", "Vessel 2: Vessel", org2PartyStatusWrapper.ParentsDescription);
					AssertHasRowWarning(org2PartyStatusWrapper, "This party is inactive and will not be screened.");
				});
			}
		}

		[RequiresSTA]
		public void TestScreeningJob_WhenOrgIsInactive_ShowInGrid()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "ORGTEST";
			org.OH_IsActive = false;

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = org.MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.DocAddresses.Add(jobDocAddress);

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(form);
				form.Show();
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("View Compliance Status").PerformClick();

				var unprocessedWrappersGrid = OpenedPartyComplianceForm.Controls.Find("UnprocessedWrappersGrid", true).Single() as ZGrid;
				AssertEquals("There is one element in the grid", 1, unprocessedWrappersGrid.List.Count);

				var orgPartyComplianceWrapper = unprocessedWrappersGrid.List[0] as PartyComplianceWrapper;
				AssertEquals("The org code is the same as the org created", org.OH_Code, orgPartyComplianceWrapper.OrgCode);

				form.Close();
			}
		}

		public void TestScreeningJob_WhenOrgIsInactive_ShowRowWarning()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "ORG1";
			org2.OH_Code = "ORG2";
			org1.OH_FullName = "ORGTEST1";
			org2.OH_FullName = "ORGTEST2";
			org1.OH_IsActive = true;
			org2.OH_IsActive = false;

			var job1DocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var job2DocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			job1DocAddress.E2_OA_Address = org1.MainAddress.PK;
			job2DocAddress.E2_OA_Address = org2.MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.DocAddresses.Add(job1DocAddress);
			shipment.DocAddresses.Add(job2DocAddress);

			Factory.Save();

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			using (var form = new ZForm(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(form);
				form.Show();
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("View Compliance Status").PerformClick();

				var unprocessedWrappersGrid = OpenedPartyComplianceForm.Controls.Find("UnprocessedWrappersGrid", true).Single() as ZGrid;
				var org1PartyStatusWrapper = unprocessedWrappersGrid.List[0] as PartyComplianceWrapper;
				var org2PartyStatusWrapper = unprocessedWrappersGrid.List[1] as PartyComplianceWrapper;

				CombineAssertions("After screening", () =>
				{
					AssertEquals("The party compliance wrapper is created from org1", org1.OH_Code, org1PartyStatusWrapper.OrgCode);
					AssertNoRowWarnings(org1PartyStatusWrapper);

					AssertEquals("The party compliance wrapper is created from org2", org2.OH_Code, org2PartyStatusWrapper.OrgCode);
					AssertHasRowWarning(org2PartyStatusWrapper, "This party is inactive and will not be screened.");
				});
			}
		}

		#region Screen Full List

		public void TestScreenFullListMenuForOrganizationOnForm_ShouldExist()
		{
			var complianceList = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList.RCL_IsExcluded = true;

			Factory.Save();

			var manager = new DeniedPartyScreeningPresentationManager();
			var orgHeader = Factory.New<OrgHeader>();
			using (var form = new BaseOrganisationsForm(orgHeader))
			{
				manager.CreateMenusForScreeningEntity(form);

				var menu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Screen (Full List)");

				AssertNotNull("Screen(Full List) menu item exist", menu);
			}
		}

		[RequiresSTA]
		public void TestScreenFullListMenuForOrganizationOnModule_ShouldExist()
		{
			var complianceList = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList.RCL_IsExcluded = true;

			Factory.Save();

			using (var form = new Form())
			using (var module = new TestDummyModule(false, false))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var menu = module.FormActionMenu.FindByText("Screen (Full List)", true);
				AssertNotNull("Screen (Full List) menu item exist", menu);
			}
		}

		public void TestScreenFullListMenuForJobOnForm_ShouldNotExist()
		{
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var shipment = Factory.New<IForwardingShipment>();
				shipment.JS_ActualVolume = 100;
				shipment.JS_ScreeningStatus = "MAT";

				CombineAssertions(() =>
				{
					AssertEquals(false, dummyForm.BusinessEntity is IScreeningPartyProvider);
					AssertEquals(true, shipment is IScreeningPartyProvider);
				});

				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(dummyForm, null, null);
				var screenMenu = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Screen (Full List)");
				AssertNull(screenMenu);
			}
		}

		#endregion

		#region Stand Alone

		public void TestStandAloneMenuItemsForOrganizationOnForm_ShouldNotExist()
		{
			var manager = new DeniedPartyScreeningPresentationManager();
			var orgHeader = Factory.New<OrgHeader>();
			using (var form = new BaseOrganisationsForm(orgHeader))
			{
				manager.CreateMenusForScreeningEntity(form);
				var menu1 = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Screen (Stand Alone)");
				var menu2 = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Screen (Stand Alone) (Full List)");

				AssertNull("Screen (Stand Alone) menu item not exist", menu1);
				AssertNull("Screen (Stand Alone) (Full List) menu item not exist", menu2);
			}
		}

		[RequiresSTA]
		public void TestStandAloneMenuItemsForOrganizationOnModule_ShouldExist()
		{
			var complianceList = Factory.NewWithValidTestData<RefComplianceList>();
			complianceList.RCL_IsExcluded = true;

			Factory.Save();

			using (var form = new Form())
			using (var module = new TestDummyModule(false, false))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var menu1 = module.FormActionMenu.FindByText("Screen (Stand Alone)", true);
				var menu2 = module.FormActionMenu.FindByText("Screen (Stand Alone) (Full List)", true);

				AssertNotNull("Screen (Stand Alone) menu item exist", menu1);
				AssertNotNull("Screen (Stand Alone) (Full List) menu item exist", menu2);
			}
		}

		#endregion

		#region Resynchronize Screening Status

		public void TestResynchronizeMenuCreated()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var manager = new DeniedPartyScreeningPresentationManager();

			using (var form = new ZForm(shipment))
			{
				manager.CreateMenusForJob(form);
				AssertNotNull(form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Resynchronize Screening Status"));
			}

			var orgHeader = Factory.New<OrgHeader>();
			using (var form = new ZForm(orgHeader))
			{
				manager.CreateMenusForScreeningEntity(form);
				AssertNull(form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Resynchronize Screening Status"));
			}
		}

		[RequiresSTA]
		public void TestResynchronizeMenuCreated_OnModule()
		{
			using (var form = new Form())
			using (var module = new TestDummyModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var resynchronizeMenuItem = module.FormActionMenu.FindByText("Resynchronize Screening Status", true);
				AssertNotNull("Resynchronize Screening Status menu item exist", resynchronizeMenuItem);

				resynchronizeMenuItem.PerformClick();
				AssertEquals("Please select at least 1 row to resynchronize.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var form = new Form())
			using (var module = new TestDummyModule(false))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var resynchronizeMenuItem = module.FormActionMenu.FindByText("Resynchronize Screening Status", true);
				AssertNull("Resynchronize Screening Status menu item not exist", resynchronizeMenuItem);
			}
		}

		public void TestPerformScreeningAsPerAllowFullScreenSecurityRight()
		{
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var shipment = InitForwardingShipment(out var sourceBizOs);
				var parties = (shipment as IScreeningPartyProvider).ScreeningParties;

				Env.Security.DpsAllowFullList.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				new DeniedPartyScreeningPresentationManager().PerformScreening(dummyForm, sourceBizOs.Cast<IDpsSourceWithParties>().ToList(), parties, false, false, true).GetAwaiter().GetResult();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResynchronizeMenuClick()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var manager = new DeniedPartyScreeningPresentationManager();

			using (var form = new ZForm(shipment))
			{
				manager.CreateMenusForJob(form);
				form.Show();
				var menu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Resynchronize Screening Status");
				AssertNotNull(menu);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.OrgDeniedPartyScreeningAllowResynchronize.IsAllowed = false;
				menu.PerformClick();
				AssertEquals(Env.Security.OrgDeniedPartyScreeningAllowResynchronize.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.OrgDeniedPartyScreeningAllowResynchronize.IsAllowed = true;
				menu.PerformClick();
				AssertEquals("Please save the form before screening for denied parties.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				Factory.Save();
				menu.PerformClick();
				AssertEquals($"{shipment.JS_UniqueConsignRef} is Job Cleared, no need for resynchronization.", UnitTestUserNotification.Instance.LastMessage.Text);

				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				Factory.Save();
				menu.PerformClick();
				AssertEquals("Resynchronize to CLR", ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
				AssertEquals($"{shipment.JS_UniqueConsignRef} screening status has been resynchronized from UNK to CLR.", UnitTestUserNotification.Instance.LastMessage.Text);

				menu.PerformClick();
				AssertEquals($"{shipment.JS_UniqueConsignRef} is synchronized, no need for resynchronization.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResynchronizeForModule()
		{
			var shipment1 = Factory.New<IForwardingShipment>();
			var shipment2 = Factory.New<IForwardingShipment>();
			var shipment3 = Factory.New<IForwardingShipment>();
			Factory.Save();

			shipment1.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			shipment2.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			shipment3.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			var manager = new DeniedPartyScreeningPresentationManagerForTest();
			manager.ResynchronizeScreeningStatusForModule_Exposed(new[] { (BusinessObject)shipment1, (BusinessObject)shipment2, (BusinessObject)shipment3 }, false);

			var expectedMessage = $"{shipment1.JS_UniqueConsignRef} is Job Cleared, no need for resynchronization." + System.Environment.NewLine
				+ $"{shipment2.JS_UniqueConsignRef} is synchronized, no need for resynchronization." + System.Environment.NewLine
				+ $"{shipment3.JS_UniqueConsignRef} screening status has been resynchronized from MAT to CLR.";

			CombineAssertions(() =>
			{
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ScreeningStatusesList.Codes.JobCleared, shipment1.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, shipment2.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, shipment3.JS_ScreeningStatus);
			});

			manager.ResynchronizeScreeningStatusForModule_Exposed(Array.Empty<BusinessObject>(), false);
			AssertEquals("Please select at least 1 row to resynchronize.", UnitTestUserNotification.Instance.LastMessage.Text);

			manager.ResynchronizeScreeningStatusForModule_Exposed(Array.Empty<BusinessObject>(), true);
			AssertEquals("One or more of the rows you have selected are not valid for screening.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestResynchronizeNotOverriddenDeclarationsFromClearToMatchIgnoreConcurrencyExceptionIfNonVerboseMode()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorPK = consignor.PK;

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_OverrideFreightDefaults = false;
			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			Factory.Save();

			declaration.JE_JS = shipment.PK;
			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
			});

			Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"
UPDATE dbo.JobDeclaration
SET
	JE_ScreeningStatus = 'UNK',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{0}'", declaration.PK));

			AssertNoExceptionThrown(() => new DeniedPartyScreeningPresentationManager().ResynchronizeScreeningStatus(false, new[] { (BusinessObject)shipment }, () => false));

			CombineAssertions(() =>
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ScreeningStatusesList.Codes.Matched, shipment.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
				AssertEquals("not saved to DB", true, declaration.HasChanges);
			});
		}

		public void TestNoNeedToResynchronizeForShipmentWithDeclarations()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			Factory.Save();

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);

			var manager = new DeniedPartyScreeningPresentationManagerForTest();
			manager.ResynchronizeScreeningStatusForModule_Exposed(new[] { (BusinessObject)shipment }, false);

			CombineAssertions(() =>
			{
				AssertEquals($"{shipment.JS_UniqueConsignRef} is synchronized, no need for resynchronization.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
			});
		}

		public void TestNoNeedToResynchronizeForTemplateRecord()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.TemplateRecord = Factory.NewWithValidTestData<StmTemplateRecord>();
			Factory.Save();

			shipment.TemplateRecord.STR_IsActive = false;
			shipment.HasChanges = false;

			var manager = new DeniedPartyScreeningPresentationManagerForTest();
			manager.ResynchronizeScreeningStatusForModule_Exposed(new[] { shipment }, false);

			var newFactory = new BusinessObjectFactory();
			var record = newFactory.Load<StmTemplateRecord>(shipment.TemplateRecord.PK);

			AssertEquals("STR_IsActive of Template Record should not be saved", true, record.STR_IsActive);
		}

		public void TestResynchronizeForShipmentWithActiveVessel_WhenChangedToInactiveVessel()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "Vessel";
			vessel.RV_LloydsNumber = "1234567";
			vessel.RV_IsActive = true;
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var transport = shipment.Transports.AddNew();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_ETD = new ZDateTime(2021, 01, 23, 7, 35, 00);
			transport.JW_ETA = new ZDateTime(2021, 01, 25, 15, 55, 00);
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "Voyage";

			Factory.Save();

			var manager = new DeniedPartyScreeningPresentationManagerForTest();
			manager.ResynchronizeScreeningStatusForModule_Exposed(new[] { (BusinessObject)shipment }, false);

			CombineAssertions(() =>
			{
				AssertEquals($@"{shipment.JS_UniqueConsignRef} is synchronized, no need for resynchronization.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);
			});

			vessel.RV_IsActive = false;
			Factory.Save();

			manager.ResynchronizeScreeningStatusForModule_Exposed(new[] { (BusinessObject)shipment }, false);

			CombineAssertions(() =>
			{
				AssertEquals($"{shipment.JS_UniqueConsignRef} screening status has been resynchronized from CLR to UNK.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ScreeningStatusesList.Codes.Unknown, shipment.JS_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
			});
		}

		public void TestPerformScreening_When_ScreeningEntity_Should_NotResyncScreeningStatus()
		{
			var orgClpStatus = Factory.New<OrgHeader>();
			orgClpStatus.OH_Code = "CLP";
			orgClpStatus.OH_FullName = "Organization With CLP Status";
			orgClpStatus.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			Factory.Save();

			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var form = new ZOrganisationsForm(orgClpStatus))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals("Organization with CLP status should have CLP status", ScreeningStatusesList.Codes.PermanentClear, orgClpStatus.OH_ScreeningStatus);
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				AsyncTaskSynchronizer.Run(() => new DeniedPartyScreeningPresentationManager().PerformScreening(form, true, false));

				CombineAssertions(() =>
				{
					AssertEquals("Organization with CLP status should remain CLP", ScreeningStatusesList.Codes.PermanentClear, orgClpStatus.OH_ScreeningStatus);
				});
			}
		}

		public void TestResynchroniseWithTwoInstance_ShouldUpdateJobScreeningStatus()
		{
			var orgConsignor = Factory.New<OrgHeader>();
			orgConsignor.OH_Code = "TESTORG1";
			orgConsignor.OH_FullName = "Organization For Test 1";

			var orgConsignee = Factory.New<OrgHeader>();
			orgConsignee.OH_Code = "TESTORG2";
			orgConsignee.OH_FullName = "Organization For Test 2";
			orgConsignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorPK = orgConsignor.PK;
			shipment.ConsigneePK = orgConsignee.PK;

			Factory.Save();

			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var form = new ZForm(shipment))
			{
				CombineAssertions("Preconditon", () =>
				{
					AssertEquals("Consignor Organization screening status is NOT", ScreeningStatusesList.Codes.NotScreened, orgConsignor.OH_ScreeningStatus);
					AssertEquals("Consignee Organization screening status is CLP", ScreeningStatusesList.Codes.PermanentClear, orgConsignee.OH_ScreeningStatus);
					AssertEquals("Shipment screening status is NOT", ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
				});

				Db.Connection.ExecuteNonQuery($"UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{orgConsignor.PK}'");

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				AsyncTaskSynchronizer.Run(() => new DeniedPartyScreeningPresentationManager().PerformScreening(form, false, true));

				CombineAssertions(() =>
				{
					AssertEquals("Consignor Organization screening status should change to CLR", ScreeningStatusesList.Codes.Clear, orgConsignor.OH_ScreeningStatus);
					AssertEquals("Consignee Organization screening status should reamain CLP", ScreeningStatusesList.Codes.PermanentClear, orgConsignee.OH_ScreeningStatus);
					AssertEquals("Shipment screening status should change to CLR", ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
				});
			}
		}

		#endregion

		public void TestScreeningJobWithParties_WhenJobHasChanges_ShouldNotShowPartyComplianceForm()
		{
			var orgConsignor = Factory.New<OrgHeader>();
			orgConsignor.OH_Code = "TESTORG1";
			orgConsignor.OH_FullName = "Organization For Test 1";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ScreeningStatus = "UNK";
			shipment.ConsignorPK = orgConsignor.PK;
			Factory.Save();

			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var form = new ZForm(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(form, shipment, parentEntityHasChanges: () => false);

				form.Show();
				shipment.JS_HouseBill = "test";

				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("View Compliance Status").PerformClick();

				AssertEquals("Should not be showing the Party Compliance form when parent has changes", "Please save the form before screening for denied parties.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestScreeningJobWithParties_WhenParentEntityHasChangesIsFalse_ShouldShowPartyComplianceForm()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ORG1";
			orgHeader.OH_FullName = "ORGTEST1";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_OA_Address = orgHeader.MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.DocAddresses.Add(docAddress);
			shipment.JS_ScreeningStatus = "UNK";

			Factory.Save();

			using (ObjectFactory.Substitute<IDpsManager>(new DpsManagerForTest()))
			using (var form = new ZForm(shipment))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(form, shipment, parentEntityHasChanges: () => false);

				form.Show();
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("View Compliance Status").PerformClick();

				AssertEquals("Should be showing the Party Compliance  form", typeof(PartyComplianceForm), ZFormModaliser.ActiveForm.GetType());

				form.Close();
			}
		}

		public void TestOverrideScreeningStatusWhenPartiesStatusClear_ShouldExcludeInScreeningParties()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TESTORG1";
			orgHeader1.OH_FullName = "Organization For Test 1";

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TESTORG2";
			orgHeader2.OH_FullName = "Organization For Test 2";
			orgHeader2.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OH_ImportBroker = orgHeader1.PK;
			shipment.JS_OH_ExportBroker = orgHeader2.PK;

			Factory.Save();

			Freight.Business.ChildEditableService.SetState(Factory, Enterprise.Freight.Integration.ChildEditableServiceStates.Shipment);

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				CombineAssertions("Preconditon", () =>
				{
					AssertEquals("Import Organization screening status is NOT", ScreeningStatusesList.Codes.NotScreened, orgHeader1.OH_ScreeningStatus);
					AssertEquals("Export Organization screening status is CLP", ScreeningStatusesList.Codes.PermanentClear, orgHeader2.OH_ScreeningStatus);
					AssertEquals("Shipment screening status is NOT", ScreeningStatusesList.Codes.NotScreened, shipment.JS_ScreeningStatus);
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				AsyncTaskSynchronizer.Run(() => new DeniedPartyScreeningPresentationManager().PerformScreening(shipmentForm, true, true));

				CombineAssertions(() =>
				{
					AssertEquals("Import Organization screening status should change to CLR", ScreeningStatusesList.Codes.Clear, orgHeader1.OH_ScreeningStatus);
					AssertEquals("Export Organization screening status should reamain CLP", ScreeningStatusesList.Codes.PermanentClear, orgHeader2.OH_ScreeningStatus);
					AssertEquals("Shipment screening status should change to CLR", ScreeningStatusesList.Codes.Clear, shipment.JS_ScreeningStatus);
				});
			}
		}

		public void TestPerformScreening_ComplianceRiskAction()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TESTORG1";
			orgHeader1.OH_FullName = "Organization For Test 1";

			Factory.Save();

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var synchronizeAndSaveRunTime = 0;
				var showMessageRunTime = false;
				var updateVisibilityRunTime = false;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var screeningParties = new[] { new ScreeningParty(orgHeader1, "Dummy", orgHeader1) };
				AsyncTaskSynchronizer.Run(() => new DeniedPartyScreeningPresentationManager().PerformScreening(form, DpsSourceWithParties.GetSingleSourceList(orgHeader1, screeningParties), screeningParties, true, false,
					complianceRiskAction: new ComplianceRiskAction(() => { synchronizeAndSaveRunTime++; }, (List<ZString> originalJobComplianceStatus) => { showMessageRunTime = true; }, () => { updateVisibilityRunTime = true; }, "CLR")));

				CombineAssertions("Compliance risk action", () =>
				{
					AssertEquals("Only Call Synchronize Once", 1, synchronizeAndSaveRunTime);
					AssertEquals(true, showMessageRunTime);
					AssertEquals(true, updateVisibilityRunTime);
				});

				form.Close();
			}
		}

		public void TestPerformScreening_ForComplianceRisk()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TESTORG1";
			orgHeader1.OH_FullName = "Organization For Test 1";

			Factory.Save();

			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var screeningParties = new[] { new ScreeningParty(orgHeader1, "Dummy", orgHeader1) };
				AsyncTaskSynchronizer.Run(() => new DeniedPartyScreeningPresentationManager().PerformScreening(form, DpsSourceWithParties.GetSingleSourceList(orgHeader1, screeningParties), screeningParties, false, false, complianceRiskAction: new ComplianceRiskAction(null, null, null, "CLR")));
				AssertEquals("Party Risk", (ZFormModaliser.LastFormShownForTest as PartyComplianceForm).FormCaption);
				form.Close();
			}
		}

		public void TestExecuteScreening()
		{
			using (new DeniedPartyScreeningHttpServiceForTest(false))
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var sourceWithParties = DpsSourceWithParties.GetSingleSourceList(orgHeader, new[] { new ScreeningParty(orgHeader, "Org", orgHeader) });
				var manager = new DeniedPartyScreeningPresentationManagerForTest();

				AssertNoExceptionThrown(() => AsyncTaskSynchronizer.Run(() => manager.ExecuteScreening_Exposed(dummyForm, sourceWithParties?.Cast<DpsSourceWithParties>().ToList(), false, true, ((DpsSourceWithParties)sourceWithParties[0]).ScreenParties.ToArray())));
			}
		}

		public void TestExecuteScreeningShouldSaveLogIntoDB()
		{
			using (new DeniedPartyScreeningHttpServiceForTest(false))
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var sourceWithParties = DpsSourceWithParties.GetSingleSourceList(orgHeader, new[] { new ScreeningParty(orgHeader, "Org", orgHeader) });
				var manager = new DeniedPartyScreeningPresentationManagerForTest();
				var statusCollection = new StmEntityScreeningLogCollection(orgHeader);

				AssertEquals("Precondition: ", false, statusCollection.Any());

				AsyncTaskSynchronizer.Run(() => manager.ExecuteScreening_Exposed(dummyForm, sourceWithParties?.Cast<DpsSourceWithParties>().ToList(), false, true, ((DpsSourceWithParties)sourceWithParties[0]).ScreenParties.ToArray()));

				AssertEquals(1, statusCollection.Count);
				AssertEquals(true, statusCollection[0].IsInDatabase);
			}
		}

		public void TestExecuteScreening_ScreenCancelled_DpsLogSaved()
		{
			using (new DeniedPartyScreeningHttpServiceForTest(false))
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var logs = new StmEntityScreeningLogCollection(orgHeader);
				AssertEquals(0, logs.Count);

				var sourceWithParties = DpsSourceWithParties.GetSingleSourceList(orgHeader, new[] { new ScreeningParty(orgHeader, "Org", orgHeader) });
				var manager = new DeniedPartyScreeningPresentationManagerV4ForSaveDpsCancelledLogTest();
				AsyncTaskSynchronizer.Run(() => manager.ExecuteScreening_Exposed(form, sourceWithParties?.Cast<DpsSourceWithParties>().ToList(), false, true, ((DpsSourceWithParties)sourceWithParties[0]).ScreenParties.ToArray()));
				AssertEquals(1, logs.Count);
				AssertEquals("Cancelled log exist", DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled, logs[0].PJ_Status);
				Assert("Cancelled log saved to database", logs[0].IsInDatabase);
				form.Close();
			}
		}

		class DeniedPartyScreeningPresentationManagerV4ForSaveDpsCancelledLogTest : DeniedPartyScreeningPresentationManager
		{
			public async Task ExecuteScreening_Exposed(Form parentForm, List<DpsSourceWithParties> sourceBizOs, bool isRescreen, bool isScreeningEntity, ScreeningParty[] uniqueParties) => await ExecuteScreening(parentForm, sourceBizOs, isRescreen, isScreeningEntity, false, uniqueParties);

			protected override (List<DpsResultStatus> ResultStatuses, List<BusinessObject> ChangedBizOs, bool IsNewDpsForm) ProcessResults(List<DpsResponseWithScreeningParty> resultItems, List<DpsSourceWithParties> sourceBizOs, bool v, BusinessObjectFactory factory, bool isRescreen, bool forceAllLists)
			{
				var logForTest = factory.New<StmEntityScreeningLog>();
				logForTest.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled;
				logForTest.PJ_ParentID = sourceBizOs[0].SourceBizO.PK;
				logForTest.PJ_ParentTableCode = sourceBizOs[0].SourceBizO.TablePrefix;

				return (new List<DpsResultStatus>(), new List<BusinessObject>(), false);
			}
		}

		public void TestExecuteScreening_EdiMessageSaved()
		{
			using (new DeniedPartyScreeningHttpServiceForTest(false))
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var sourceWithParties = DpsSourceWithParties.GetSingleSourceList(orgHeader, new[] { new ScreeningParty(orgHeader, "Org", orgHeader) });
				var manager = new DeniedPartyScreeningPresentationManagerForTest();
				var query = new ZDBOnlyQuery(typeof(DpsEDIMessage));
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DPSRequestMessage);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.JDC);
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.ScreeningRequest);
				query.OrderBy = $"{EDIMessageSchema.Constants.EM_MessageNum} DESC";

				var dpsEdiMessage = Factory.LoadTop1<DpsEDIMessage>(query);

				AssertEquals("Precondition: ", false, dpsEdiMessage != null && IsDpsEdiMessageForOrgExist(dpsEdiMessage, orgHeader));

				AsyncTaskSynchronizer.Run(() => manager.ExecuteScreening_Exposed(dummyForm, sourceWithParties?.Cast<DpsSourceWithParties>().ToList(), false, true, ((DpsSourceWithParties)sourceWithParties[0]).ScreenParties.ToArray()));

				dpsEdiMessage = Factory.LoadTop1<DpsEDIMessage>(query);
				Assert(IsDpsEdiMessageForOrgExist(dpsEdiMessage, orgHeader));
			}
		}

		bool IsDpsEdiMessageForOrgExist(DpsEDIMessage message, OrgHeader header)
		{
			var messageContent = JsonConvert.DeserializeObject<DpsEDIMessageContent>(message.EM_MessageData.ToUTF8());
			return messageContent.EntityType.Equals(OrgHeaderSchema.Constants.Prefix) && messageContent.ClientSpecifiedIdentifier.Equals(header.PK.ToGuid());
		}

		public void TestExecuteScreeningDoNotUpdateRelatedJobsIfRegistrySetToTrue()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader[OrgHeaderSchema.OH_Code] = "NEWCOD";
			Factory.Save();

			var sourceWithParties = DpsSourceWithParties.GetSingleSourceList(orgHeader, new[] { new ScreeningParty(orgHeader, "Org", orgHeader) });
			var consol = Factory.New<IForwardingConsol>();
			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			Factory.Save();

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.NotScreened, orgHeader.OH_ScreeningStatus);
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);

				var manager = new DeniedPartyScreeningPresentationManagerForTest();

				AsyncTaskSynchronizer.Run(() => manager.ExecuteScreening_Exposed(dummyForm, sourceWithParties?.Cast<DpsSourceWithParties>().ToList(), false, true, ((DpsSourceWithParties)sourceWithParties[0]).ScreenParties.ToArray()));
				AssertEquals(ScreeningStatusesList.Codes.Clear, orgHeader.OH_ScreeningStatus);

				((BusinessObject)consol).Reload();
				AssertEquals("Should not update related jobs if registry set to true.", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			}
		}

		[RequiresSTA]
		public void TestExecuteScreeningUpdateRelatedJobsIfRegistrySetToFalse()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader[OrgHeaderSchema.OH_Code] = "NEWCOD";
			Factory.Save();

			var sourceWithParties = DpsSourceWithParties.GetSingleSourceList(orgHeader, new[] { new ScreeningParty(orgHeader, "Org", orgHeader) });
			var consol = Factory.New<IForwardingConsol>();
			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;
			consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus = false;
			Factory.Save();

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.NotScreened, orgHeader.OH_ScreeningStatus);
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);

				var manager = new DeniedPartyScreeningPresentationManagerForTest();
				AsyncTaskSynchronizer.Run(() => manager.ExecuteScreening_Exposed(dummyForm, sourceWithParties?.Cast<DpsSourceWithParties>().ToList(), false, true, ((DpsSourceWithParties)sourceWithParties[0]).ScreenParties.ToArray()));
				AssertEquals(ScreeningStatusesList.Codes.Clear, orgHeader.OH_ScreeningStatus);

				((BusinessObject)consol).Reload();
				AssertEquals("Should update related jobs if registry set to false.", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);
			}
		}

		class DpsManagerForTest : IDpsManager
		{
			public IDpsServiceV4 GetDpsService(string url)
			{
				return new DpsServiceV4(new HttpClient());
			}
			public List<IDpsServiceV4> GetDpsServices(IDpsServiceV4 service)
			{
				var services = new List<IDpsServiceV4>();
				services.Add(new DpsServiceV4(new HttpClient()));
				return services;
			}

			public void DeactivateBillingEntities(Guid[] entityPKs)
			{
				throw new NotImplementedException();
			}

			public async Task<DpsResponse> Screen(DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching, IDpsServiceV4 service)
			{
				return await Task.FromResult(new DpsResponse { NameMatches = Array.Empty<NameMatchInfo>(), AddressMatches = Array.Empty<AddressMatchInfo>(), RegistrationCodeMatches = Array.Empty<RegistrationCodeMatchInfo>(), CountryMatches = Array.Empty<CountryMatchInfo>(), Profiles = Array.Empty<ProfileHeaderInfo>() });
			}
			public async Task<DpsResponse> GetScreenResult(DpsRequestHeaderWithAddressMatching dpsRequestHeaderWithAddressMatching, List<IDpsServiceV4> services)
			{
				return await Task.FromResult(new DpsResponse { NameMatches = Array.Empty<NameMatchInfo>(), AddressMatches = Array.Empty<AddressMatchInfo>(), RegistrationCodeMatches = Array.Empty<RegistrationCodeMatchInfo>(), CountryMatches = Array.Empty<CountryMatchInfo>(), Profiles = Array.Empty<ProfileHeaderInfo>() });
			}
		}

		#region Implementation

		void TestCreateMenusForJobCore(Type bizoTypeForTest, bool expectedMenuAddedSuccessfully)
		{
			using (var form = new ZForm(Factory.New(bizoTypeForTest)))
			{
				new DeniedPartyScreeningPresentationManager().CreateMenusForJob(form);
				form.Show();
				MenuItem actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];

				if (expectedMenuAddedSuccessfully)
				{
					AssertNotNull(actionsMenuItem.MenuItems.FindByText("View Compliance Status"));
					AssertNotNull(actionsMenuItem.MenuItems.FindByText("Resynchronize Screening Status"));
				}
				else
				{
					AssertNull(actionsMenuItem.MenuItems.FindByText("View Compliance Status"));
					AssertNull(actionsMenuItem.MenuItems.FindByText("Resynchronize Screening Status"));
				}

				form.Close();
			}
		}

		void TestCreateModuleMenusForJobCore(Type bizoTypeForTest, bool forBooking, bool expectedMenuAddedSuccessfully)
		{
			using (var form = new Form())
			using (var module = new FilterGridModuleForTest(bizoTypeForTest, forBooking))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				if (expectedMenuAddedSuccessfully)
				{
					AssertNotNull(module.FormActionMenu.FindByText("View Compliance Status", true));
					AssertNotNull(module.FormActionMenu.FindByText("Resynchronize Screening Status", true));
				}
				else
				{
					AssertNull(module.FormActionMenu.FindByText("View Compliance Status", true));
					AssertNull(module.FormActionMenu.FindByText("Resynchronize Screening Status", true));
				}

				form.Close();
			}
		}

		SecurityCore GetTemporarySecurityCore()
		{
			return new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}

		static int UpdateRelatedJobs(OrgHeader orgHeader)
		{
			DbCommand cmd = Db.Connection.Command("UpdateRelatedJobsForOrgOrDocAddress");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameter("@entityPK", SqlDbType.UniqueIdentifier, orgHeader.PK.ToGuid());
			cmd.AddParameter("@earliestDT", SqlDbType.DateTime, ZDateTime.Today.AddDays(-7).ToDateTime());
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
			cmd.AddParameter("@updateShipments", SqlDbType.Bit, 1);
			cmd.AddParameter("@updateConsols", SqlDbType.Bit, 1);
			cmd.AddParameter("@jobUpdatePeriod", SqlDbType.SmallDateTime, ZDateTime.UtcNow.AddMonths(-OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value).ToDateTime());
			cmd.AddParameter("@userCode", SqlDbType.VarChar, 3, GlbStaff.CurrentUser.GS_Code.ToString());
			cmd.AddParameter("@freightCpwEnabled", SqlDbType.Bit, 0);

			return cmd.ExecuteProcedureWithReturnValue();
		}

		ScreeningParty ScreeningParty1
		{
			get
			{
				if (fScreeningParty1 == null)
				{
					OrgHeader parent = Factory.NewWithValidTestData<OrgHeader>();
					OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
					fScreeningParty1 = new ScreeningParty(parent, "candidate", header);
				}
				return fScreeningParty1;
			}
		}
		ScreeningParty fScreeningParty1;

		OrgHeader Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = Factory.NewWithValidTestData<OrgHeader>();
					fParent.OH_FullName = "PARENT ORG";
					fParent.MainAddress.OA_Address1 = "Parent address";
					fParent.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				}
				return fParent;
			}
		}
		OrgHeader fParent;

		public class TestDummyModule : DummyFilterGridModule
		{
			readonly bool forJob;
			readonly bool forBookingJob;
			readonly bool hasErrorMessage;

			public BusinessObject[] SelectedBusinessObjectsForTest;

			public TestDummyModule(bool forJob = true, bool forBookingJob = false, bool hasErrorMessage = false)
			{
				this.forJob = forJob;
				this.forBookingJob = forBookingJob;
				this.hasErrorMessage = hasErrorMessage;
			}

			public override BusinessObject[] GetSelectedBusinessObjects()
			{
				if (hasErrorMessage)
				{
					SelectedBusinessObjectsForTest = new[] { Factory.New<DummyBusinessObject>() };
				}

				return SelectedBusinessObjectsForTest ?? base.GetSelectedBusinessObjects();
			}

			public ZDisplayGrid Grid_Exposed
			{
				get { return Grid; }
			}

			protected override MenuItem[] GetNewActionMenuItems()
			{
				List<MenuItem> results = new List<MenuItem>(base.GetNewActionMenuItems());
				Manager = new DeniedPartyScreeningPresentationManager();

				if (forBookingJob)
				{
					Manager.CreateModuleMenusForJob(this, results, u => (Array.Empty<BusinessObject>(), true));
				}
				else if (forJob)
				{
					Manager.CreateModuleMenusForJob(this, results);
				}
				else if (hasErrorMessage)
				{
					Manager.CreateModuleMenusForJob(this, results, screeningNotEnabledMessage: () => "test message");
				}
				else
				{
					Manager.CreateModuleMenusForScreeningEntity(this, results);
				}

				return results.ToArray();
			}

			public DeniedPartyScreeningPresentationManager Manager { get; set; }
		}

		public class FilterGridModuleForTest : TestDummyModule
		{
			public FilterGridModuleForTest(Type typeForTest, bool forBooking)
				: base(forJob: true, forBookingJob: forBooking)
			{
				typeOfTopLevelBusinessObjectForTest = typeForTest;
			}

			readonly Type typeOfTopLevelBusinessObjectForTest;

			protected override Type TypeOfTopLevelBusinessObjectCore => typeOfTopLevelBusinessObjectForTest;
		}

		void AssertMacroDisplayEventReference(string rawScreeningStatus, string toScreeningStatus, string expectStr)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			org.OH_ScreeningStatus = rawScreeningStatus;
			Factory.Save();

			var resultStatuses = new[] { new DpsResultStatus(org, toScreeningStatus) };
			new DeniedPartyScreeningPresentationManagerForTest().SaveResultStatuses_Exposed(resultStatuses, null, new BusinessObjectFactory());
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeniedPartyStatusUpdatedCode);
			var result = org.GetLogs().Find(query);
			AssertEquals(expectStr, result.Single().DisplayEventReference);
		}

		class DeniedPartyScreeningPresentationManagerForTest : DeniedPartyScreeningPresentationManager
		{
			public void ProcessScreenResultStatus_Exposed(BusinessObject bizo, string status, DpsMessageInfo messageInfo) => ProcessScreenResultStatus(bizo, status, messageInfo);

			public void SaveResultStatuses_Exposed(DpsResultStatus[] resultStatuses, DpsMessageInfo messageInfo, BusinessObjectFactory factory) => SaveResultStatuses(resultStatuses, messageInfo, factory);

			public PartyComplianceForm GetPartyComplianceForm_Exposed(ScreeningParty[] uniqueParties, Func<bool, Task> showMessageIfNoPartiesToScreenAction, ComplianceRiskAction complianceRiskAction = null) => GetPartyComplianceForm(uniqueParties, showMessageIfNoPartiesToScreenAction, complianceRiskAction);

			public void ResynchronizeScreeningStatusForModule_Exposed(BusinessObject[] selectedBusinessObjects, bool hasInvalidItems) => ResynchronizeScreeningStatusForModule(selectedBusinessObjects, hasInvalidItems);

			public async Task ExecuteScreening_Exposed(Form parentForm, List<DpsSourceWithParties> sourceBizOs, bool isRescreen, bool isScreeningEntity, ScreeningParty[] uniqueParties) => await ExecuteScreening(parentForm, sourceBizOs, isRescreen, isScreeningEntity, false, uniqueParties);
		}

		class DpsMessageInfoForTest : DpsMessageInfo
		{
			public DpsMessageInfoForTest(List<DpsSourceWithParties> sourceBizOs, bool shouldShowMessage, int screenedItemsCount) : base(sourceBizOs, shouldShowMessage, screenedItemsCount) { }
			public List<StatusInfo> FinalStatusList_Exposed => FinalStatusList;
		}

		ZForm OpenedPartyComplianceForm => (ZForm)ZApplication.GetOpenForms().Where(f => f.Name == "PartyComplianceForm").FirstOrDefault();

		public class DummyWithQuotedBooking : DummyBusinessObject, IQuotedBooking
		{
			public DummyWithQuotedBooking(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZGuid ClientPK { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ZGuid ViewPK => throw new NotImplementedException();

			public ZString UniqueConsignRef { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public ZString TransportMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public ZString Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public IOrgHeader ControllingCustomer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ZGuid CartagePK => throw new NotImplementedException();

			public ZGuid SailingJX { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public IDependentBusinessObjectCollection QuotedBookingContainers => throw new NotImplementedException();

			public ZString PackingMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public BusinessObject ForwardingShipment => throw new NotImplementedException();

			public BusinessObject JobSailing => throw new NotImplementedException();

			public BusinessObject Quote => throw new NotImplementedException();

			public IJobHeader Job => throw new NotImplementedException();

			public ZString Name => throw new NotImplementedException();

			public IOrgHeader ContractServiceProvider => throw new NotImplementedException();

			public IRatingContract CarrierContract => throw new NotImplementedException();

			public IRatingContractAllocationLine AllocationRoute => throw new NotImplementedException();

			public ZDateTime ETD => throw new NotImplementedException();

			public ZString LoadPort => throw new NotImplementedException();

			public ZString DischargePort => throw new NotImplementedException();

			public ZString VoyageFlight => throw new NotImplementedException();

			public ZString Vessel => throw new NotImplementedException();

			public IEnumerable<IForwardingContainer> Containers => throw new NotImplementedException();

			public IOrgHeader Client => throw new NotImplementedException();

			public IOrgHeader Consignee => throw new NotImplementedException();

			public IOrgHeader Consignor => throw new NotImplementedException();

			public ZString Via => throw new NotImplementedException();

			ZString IQuotedBooking.Origin => throw new NotImplementedException();

			ZString IQuotedBooking.Destination => throw new NotImplementedException();
		}

		public class DummyWithViewQuotedBooking : DummyBusinessObject, IViewQuotedBooking, IViewComplianceRiskStatusProvider
		{
			public DummyWithViewQuotedBooking(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			#region IDeniedPartyScreeningPartyProvider Members

			public ZGuid VB_JS { get; set; }

			public ZGuid VB_TH { get; set; }

			public ZGuid VB_GC { get; set; }

			public IJobHeader Job { get; }

			IComplianceItemRiskStatusProvider IViewComplianceRiskStatusProvider.GetProviderBusinessObject() => throw new NotImplementedException();

			#endregion
		}

		public class DummyWithScreeningPartyProvider : DummyBusinessObject, IScreeningPartyProvider
		{
			public DummyWithScreeningPartyProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IDeniedPartyScreeningPartyProvider Members

			public ScreeningParty[] ScreeningParties
			{
				get { return Array.Empty<ScreeningParty>(); }
			}

			public ZString ScreeningStatus { get; set; }

			#endregion

			public ZString GetWorstScreeningStatus()
			{
				throw new NotImplementedException();
			}

			public ZString GetWorstScreeningStatusUnlessManuallyCleared()
			{
				throw new NotImplementedException();
			}
		}

		public class DummyWithBothQuotedBookingAndComplianceRiskStatusProvider : DummyWithQuotedBooking, ICompliancePartyRiskStatusProvider, IComplianceLocationRiskStatusProvider, IComplianceCommodityRiskStatusProvider
		{
			public DummyWithBothQuotedBookingAndComplianceRiskStatusProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDateTime EffectiveDate => throw new NotImplementedException();

			public IEnumerable<IScreeningParty> Parties => throw new NotImplementedException();

			public IEnumerable<IComplianceLocation> Locations => throw new NotImplementedException();

			public IEnumerable<IComplianceCommodity> Commodities => throw new NotImplementedException();

			public IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders => throw new NotImplementedException();

			public IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders => throw new NotImplementedException();

			public ZGuid ParentID => throw new NotImplementedException();

			public ZString ParentTableCode => throw new NotImplementedException();

			public bool SupportInitializingComplianceAssessmentStatusWorkflow => throw new NotImplementedException();

			public ComplianceRiskSupport ComplianceRiskSupport => throw new NotImplementedException();

			public Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public Func<DocumentDeliveryResultForComplianceWorkflow> CheckComplianceAssessmentRequirements { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo => throw new NotImplementedException();

			public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

			public ZBool IsEnabledComplianceWise => true;

			public ZBool IsEditingCommoditySupported => true;

			CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => throw new NotImplementedException();
		}

		[ViewComplianceRiskStatusProvider(ProviderBusinessObjectType = typeof(DummyWithBothQuotedBookingAndComplianceRiskStatusProvider))]
		public class DummyWithBothViewQuotedBookingAndComplianceRiskStatusProvider : DummyWithViewQuotedBooking
		{
			public DummyWithBothViewQuotedBookingAndComplianceRiskStatusProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public class DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider : DummyWithScreeningPartyProvider, ICompliancePartyRiskStatusProvider, IComplianceLocationRiskStatusProvider, IComplianceCommodityRiskStatusProvider
		{
			public DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public IEnumerable<IScreeningParty> Parties => throw new NotImplementedException();

			public IEnumerable<IComplianceLocation> Locations => throw new NotImplementedException();

			public IEnumerable<IComplianceCommodity> Commodities => throw new NotImplementedException();

			public IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders => throw new NotImplementedException();

			public IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders => throw new NotImplementedException();

			public ZDateTime EffectiveDate => throw new NotImplementedException();

			public ZGuid ParentID => throw new NotImplementedException();

			public ZString ParentTableCode => throw new NotImplementedException();

			public ComplianceRiskSupport ComplianceRiskSupport => throw new NotImplementedException();

			public Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public Func<DocumentDeliveryResultForComplianceWorkflow> CheckComplianceAssessmentRequirements { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo => throw new NotImplementedException();

			public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

			public ZBool IsEnabledComplianceWise => true;

			public ZBool IsEditingCommoditySupported => true;

			CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => throw new NotImplementedException();
		}

		#endregion
	}
}
