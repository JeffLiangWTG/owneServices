using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ConsolShipmentModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestNewButton_Click()
		{
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("consol", Factory);
			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			string message = "message";
			helper.Setup(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);
			helper.Setup(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);

			helper.SetupSequence(m => m.IsAllowedToAddNewShipment(out message, It.IsAny<CommonConsol>()))
				.Returns(false)
				.Returns(true)
				.CallBase();

			var errorMessage = "Error message";
			var noneMessage = "None ";

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();

				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>().First();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals("Should be Error", errorMessage,
						UnitTestUserNotification.Instance.LastMessage.ToString());

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					using (form.Grid.LastShownZForm)
					{
						AssertEquals("Should be NO Errors", noneMessage,
							UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		public void TestGivenConsolWithSCN_WhenClickingNewButton_ThenShipmentModeShouldBeSynchronizedToSCN()
		{
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("consol", Factory);
			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;
			Factory.Save();

			var message = "message";

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);
			helper.Setup(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);
			helper.Setup(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);

			helper.SetupSequence(m => m.IsAllowedToAddNewShipment(out message, It.IsAny<CommonConsol>()))
				.Returns(false)
				.Returns(true)
				.CallBase();

			var errorMessage = "Error message";
			var noneMessage = "None ";

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();

				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>().First();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals("Should be Error", errorMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should no new Shipment", 0, consol.Shipments.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					using (form.Grid.LastShownZForm)
					{
						AssertEquals("Should be NO Errors", noneMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

						var newShipment = form.Grid.LastShownZForm.BusinessEntityForPersistingForm as CommonShipment;
						AssertEquals("Given consol with SCN, when clicking new button, then shipment mode should be synchronized to SCN", Constants.ContainerModes.ShippersConsol, newShipment.JS_PackingMode);
					}
				}
			}
		}

		public void TestAttachButton_Click()
		{
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("consol", Factory);
			Factory.Save();
			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			var attachRequest = new ShipmentConsolAttachRequest(null, null);
			var attachRequestWithErrors = new ShipmentConsolAttachRequest(() => "ERROR?", null);

			helper.SetupSequence(m => m.IsAllowedToAttachShipment(It.IsAny<CommonConsol>(), It.IsAny<CommonShipment>()))
				.Returns(attachRequest)
				.Returns(attachRequestWithErrors)
				.CallBase();

			string noneMessage = "None ";

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals("Should be NO Errors", noneMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					using (form.Grid.Attacher.LastShownAttachPopupForTesting)
					{
						AssertEquals("Should have Errors", "Error ERROR?", UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		public void TestAttacher()
		{
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("C00001001", Factory);
			Factory.Save();

			var mAS_1 = FreightTestHelper.GetShipment<ForwardingShipment>("MAS_1", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var sUB_1 = FreightTestHelper.GetShipment("SUB_1", mAS_1, Constants.ShipmentTypes.StandardHouse, Factory);

			var mAS_2 = FreightTestHelper.GetShipment<ForwardingShipment>("MAS_2", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var sUB_2 = FreightTestHelper.GetShipment("SUB_2", mAS_2, Constants.ShipmentTypes.StandardHouse, Factory);

			var dIR = FreightTestHelper.GetShipment<ForwardingShipment>("DIR", Constants.ShipmentTypes.StandardHouse, Factory);
			var dirConsol = dIR.Consols.AddNew();
			dirConsol.JK_AgentType = Constants.AgentType.Direct;

			var sTD = FreightTestHelper.GetShipment<ForwardingShipment>("STD", Constants.ShipmentTypes.StandardHouse, Factory);

			string questionToSkip = @"Question Of the shipments you are trying to attach to the consol C00001001, there are shipments that cannot be attached for the following reasons:
The shipment DIR is already attached to at least one other Direct Consol. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).";

			string questionToSkipSubShipments = @"Question The shipments that you are trying to attach are sub-shipments:
SUB_2 is a sub-shipment of master/lead MAS_2

Only these shipments, without their masters, will be attached to C00001001 consol.

If you would like to attach these shipments, their masters/leads and all sub-shipments of their masters/leads to this consol, you need to attach the master/lead shipments to this consol instead.

Press [Yes] if you would like to continue.
Press [No] if you would like to skip this shipments and apply all other selected shipments.
Press [Cancel] to cancel operation.";

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				var selected = new List<BusinessObject>() { mAS_1, sUB_1, sUB_2, dIR, sTD };
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals("Shipments should NOT be added", false, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip shipments", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertShipmentCollection("selected shipments should NOT be changed", selected, mAS_1, sUB_1, sUB_2, dIR, sTD);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals("Shipments should NOT be added", false, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip sub-shipments", questionToSkipSubShipments, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertShipmentCollection("selected shipments should NOT be changed", selected, mAS_1, sUB_1, sUB_2, dIR, sTD);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("Shipments should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip sub-shipments", questionToSkipSubShipments, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertShipmentCollection("selected shipments should be changed", selected, mAS_1, sTD);

				selected = new List<BusinessObject>() { mAS_1, sUB_1, sUB_2, dIR, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals("Shipments should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip sub-shipments", questionToSkipSubShipments, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertShipmentCollection("selected shipments should be changed", selected, mAS_1, sUB_2, sTD);
			}
		}

		public void TestAttacher_WhenShipmentDatesDontMuchConsolDates()
		{
			var today = ZDateTime.Today;
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("CONSOL", Factory);
			consol.Transports.MostInterestingTransport.JW_ETA = today.AddDays(10);
			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("SHIPMENT", Constants.ShipmentTypes.StandardHouse, Factory);
			shipment.JS_E_ARV = today;

			var question = @"None There's inconsistency between ETD/ETA of Shipments and Consols you are trying to link.
See details below:
Shipment SHIPMENT has estimated arrival date before Consol CONSOL ETA.

How would you like to proceed?

Press [Yes] to attach and update the Shipments dates to match the Consols dates.
Press [No] to attach Shipments but do not update Shipments dates.
Press [Cancel] to cancel operation.";

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				var selected = new List<BusinessObject>() { shipment };
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				Assert(!form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Assert(form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());
				Assert(!shipment.IsSuppressedETAETDOnAttachToConsol);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Assert(form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());
				Assert(shipment.IsSuppressedETAETDOnAttachToConsol);
			}
		}

		public void TestAttacher_ForStandAloneShipment()
		{
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("C00001001", Factory);
			Factory.Save();

			var mas1 = FreightTestHelper.GetShipment<ForwardingShipment>("MAS_1", Constants.ShipmentTypes.AssemblyMaster, Factory);

			var sub1 = FreightTestHelper.GetShipment("SUB_1", mas1, Constants.ShipmentTypes.StandardHouse, Factory);
			sub1.JS_IsBooking = ZBool.True;
			sub1.JS_CFSReference = "CFS Ref1";

			var voyage = Factory.New<JobVoyage>();
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = "USLAX";

			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sub1.JS_JX = sailing.PK;

			Factory.Save();

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.Grid.Attacher.LastShownAttachPopupForTesting.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { sub1 });
				form.Grid.Attacher.LastShownAttachPopupForTesting.Dispose();
				AssertEquals("CFS Ref1", consol.JK_BookingReference);
				AssertEquals(true, consol.IsAttachedToStandAloneShipment);
			}
		}

		public void TestAttacher_ChecksComplianceRisk_WhenConsolIsRisky()
		{
			AssertCheckComplianceRiskWhenConsolIsRisky(ComplianceRiskStatusCodeList.Codes.Blocked, ComplianceRiskStatusCodeList.Codes.Clear);
			AssertCheckComplianceRiskWhenConsolIsRisky(ComplianceRiskStatusCodeList.Codes.OverrideClear, ComplianceRiskStatusCodeList.Codes.Clear);
		}

		public void TestAttacher_ChecksComplianceRisk_WhenShipmentIsRisky()
		{
			AssertCheckComplianceRiskWhenShipmentIsRisky(ComplianceRiskStatusCodeList.Codes.Clear, ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertCheckComplianceRiskWhenShipmentIsRisky(ComplianceRiskStatusCodeList.Codes.OverrideClear, ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertCheckComplianceRiskWhenShipmentIsRisky(ComplianceRiskStatusCodeList.Codes.Clear, ComplianceRiskStatusCodeList.Codes.OverrideClear);
		}

		public void TestDetachButton_Click()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();

			Factory.Save();

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();

				form.Grid.InnerGrid.Select(0);
				form.Grid.InnerGrid.Select(1);

				var selectedShipments = new[] { shipment1, shipment2 };

				var helper = new Mock<IShipmentVsConsolGUIMessageHelper>(MockBehavior.Strict);

				using (ShipmentVsConsolGUIMessageHelper.OverrideHelperInstance(helper.Object))
				{
					// NOT Allowed
					helper
						.Setup(m => m.IsAllowedToDetachShipments(FreightShipmentVsConsolMessageHelper.Instance,
							consol,
							It.Is<IEnumerable<CommonShipment>>(p => AssertShipments(p, selectedShipments))))
						.Returns(false);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					detachButton.PerformClick();
					Assert("Base detach dialog was not shown", UnitTestUserNotification.Instance.LastMessage.WasNone);

					helper
						.Setup(m => m.IsAllowedToDetachShipments(FreightShipmentVsConsolMessageHelper.Instance,
							consol,
							It.Is<IEnumerable<CommonShipment>>(p => AssertShipments(p, selectedShipments))))
						.Returns(true);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var collection = (TopLevelShipmentCollection)form.Grid.Collection;
					var shipmentCollection = (ManyToManyShipmentCollection)collection.CollectionToFilter;
					shipmentCollection.ParentConsolRefreshBindingCount = 0;

					UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
					detachButton.PerformClick();
					Assert("Base detach dialog was shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(form.Grid.DetachMessage.Caption));
					AssertEquals(1, shipmentCollection.ParentConsolRefreshBindingCount);
				}
			}
		}

		public void TestDetachButton_Click_WithNewConsolAndShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();

				form.Grid.InnerGrid.Select(0);
				form.Grid.InnerGrid.Select(1);

				var selectedShipments = new[] { shipment1 };

				var helper = new Mock<IShipmentVsConsolGUIMessageHelper>(MockBehavior.Strict);

				using (ShipmentVsConsolGUIMessageHelper.OverrideHelperInstance(helper.Object))
				{
					// NOT Allowed
					helper
						.Setup(m => m.IsAllowedToDetachShipments(
							FreightShipmentVsConsolMessageHelper.Instance,
							consol,
							It.Is<IEnumerable<CommonShipment>>(p => AssertShipments(p, selectedShipments)
							)))
						.Returns(false);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					detachButton.PerformClick();
					Assert("Base detach dialog was not shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
					helper
						.Setup(m => m.IsAllowedToDetachShipments(
							FreightShipmentVsConsolMessageHelper.Instance,
							consol,
							It.Is<IEnumerable<CommonShipment>>(p => AssertShipments(p, selectedShipments)
							)))
						.Returns(true);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var collection = (TopLevelShipmentCollection)form.Grid.Collection;
					var shipmentCollection = (ManyToManyShipmentCollection)collection.CollectionToFilter;
					shipmentCollection.ParentConsolRefreshBindingCount = 0;

					UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
					detachButton.PerformClick();
					Assert("Base detach dialog was shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(form.Grid.DetachMessage.Caption));
					AssertEquals(1, shipmentCollection.ParentConsolRefreshBindingCount);
				}
			}
		}

		bool AssertShipments(IEnumerable<CommonShipment> commonShipments, ForwardingShipment[] shipments)
		{
			AssertContainsExactElementsInAnyOrder(commonShipments, shipments);
			return true;
		}

		public void TestDetachButton_Click_After_AttachButton_Click()
		{
			var consol1 = FreightTestHelper.GetConsol<ForwardingConsol>("C00001001", Factory);
			var consol2 = FreightTestHelper.GetConsol<ForwardingConsol>("C00001002", Factory);
			var shipment1 = FreightTestHelper.GetShipment<ForwardingShipment>("S00001001", Constants.ShipmentTypes.StandardHouse, Factory);

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_JobNum = "Job1";
			jobHeader1.JH_ParentTableCode = "JS";
			jobHeader1.JH_ParentID = shipment1.PK;

			consol2.Shipments.Add(shipment1);

			Factory.Save();

			AssertEquals("Precondition: Shipment.HasChanges should be false", false, shipment1.HasChanges);
			AssertEquals("Precondition: There should be no shipments attached", 0, consol1.Shipments.Count);

			using (var form = new MockConsolForm3(consol1))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var attachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				attachButton.PerformClick();

				var selected = new List<BusinessObject>() { shipment1 };
				form.Grid.Attacher.Attach(selected);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNotNull("Shipment should be added to Consol.Shipments collection", consol1.Shipments.FindByPK(shipment1.PK));

				Thread.Sleep(50);

				form.Grid.InnerGrid.Select(consol1.Shipments.IndexOf(x => x.PK == shipment1.PK));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();
				detachButton.PerformClick();
				AssertNull("Shipment should be removed from Consol.Shipments collection", consol1.Shipments.FindByPK(shipment1.PK));

				Thread.Sleep(50);

				form.Grid.Attacher.Attach(selected);
				AssertNotNull("Shipment should be added to Consol.Shipments collection", consol1.Shipments.FindByPK(shipment1.PK));

				Thread.Sleep(50);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				detachButton.PerformClick();
				AssertNull("Shipment should be removed from Consol.Shipments collection", consol1.Shipments.FindByPK(shipment1.PK));

				Thread.Sleep(50);

				Factory.Save();

				Thread.Sleep(50);

				var otherFactory = new BusinessObjectFactory();
				var shipmentInOtherFactory = otherFactory.Load<CommonShipment>(shipment1.PK);
				var attachedLog = shipmentInOtherFactory.Logs.MostRecentLogByEventTime(Events.Attached);
				var detachedLog = shipmentInOtherFactory.Logs.MostRecentLogByEventTime(Events.Detached);
				Assert("Detached log should be posted after the attached log", detachedLog.SL_EventTime > attachedLog.SL_EventTime);
			}
		}

		[RequiresSTA]
		public void TestDetachButton_Click_After_AttachButton_Click_WithChanges()
		{
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("C00001001", Factory);
			var shipment1 = FreightTestHelper.GetShipment<ForwardingShipment>("S00001001", Constants.ShipmentTypes.StandardHouse, Factory);
			consol.Transports[0].JW_ETD = new ZDateTime(2021, 7, 3);

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_JobNum = "Job1";
			jobHeader1.JH_ParentTableCode = "JS";
			jobHeader1.JH_ParentID = shipment1.PK;

			Factory.Save();

			AssertEquals("Precondition: Shipment.HasChanges should be false", false, shipment1.HasChanges);
			AssertEquals("Precondition: There should be no shipments attached", 0, consol.Shipments.Count);

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var attachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				attachButton.PerformClick();

				var selected = new List<BusinessObject>() { shipment1 };
				form.Grid.Attacher.Attach(selected);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNotNull("Shipment should be added to Consol.Shipments collection", consol.Shipments.FindByPK(shipment1.PK));
				AssertEquals("Shipment JS_E_DEP should be set", consol.Transports[0].JW_ETD, shipment1.JS_E_DEP);
				AssertEquals("Shipment JS_E_DEP should have changes", true, shipment1.JS_E_DEPInfo.HasChanges);

				form.Grid.InnerGrid.Select(consol.Shipments.IndexOf(x => x.PK == shipment1.PK));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();
				detachButton.PerformClick();
				AssertNotNull("Shipment should NOT be removed from Consol.Shipments collection", consol.Shipments.FindByPK(shipment1.PK));

				Factory.Save();
				AssertEquals("Shipment JS_E_DEP should be set", consol.Transports[0].JW_ETD, shipment1.JS_E_DEP);

				var otherFactory = new BusinessObjectFactory();
				var shipmentInOtherFactory = otherFactory.Load<CommonShipment>(shipment1.PK);
				AssertEquals("There should be one departure log saved", 1, shipmentInOtherFactory.Logs.Find(x => x.SL_SE_NKEvent == Events.DepartureCode).Count());
				AssertEquals("There should be one attached log saved", 1, shipmentInOtherFactory.Logs.Find(x => x.SL_SE_NKEvent == Events.AttachedCode).Count());
				AssertEquals("There should be no detached logs saved", 0, shipmentInOtherFactory.Logs.Find(x => x.SL_SE_NKEvent == Events.DetachedCode).Count());
			}
		}

		public void TestDetachButton_Click_DeleteComplianceRisk_NotInDataBase()
		{
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("C00001001", Factory);
			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("S00001001", Constants.ShipmentTypes.StandardHouse, Factory);
			consol.Shipments.Add(shipment);
			Factory.Save();
			var shipmentComplianceRisk = Factory.New<ComplianceRiskStatus>();
			shipmentComplianceRisk.COR_ParentID = shipment.PK;
			shipmentComplianceRisk.CopyFromBooking = true;
			shipmentComplianceRisk.COR_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipmentComplianceRisk.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			shipmentComplianceRisk.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			shipmentComplianceRisk.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			var shipmentCommodityDetail = Factory.New<ComplianceCommodityDetail>();
			shipmentCommodityDetail.CCD_COR_ComplianceRisk = shipmentComplianceRisk.PK;
			shipmentCommodityDetail.CCD_HarmonizedCode = "123456";
			shipmentCommodityDetail.CCD_RN_NKOrigin = "AU";
			shipmentCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			AssertEquals(true, shipment.IsInDatabase);
			AssertEquals(false, shipmentComplianceRisk.IsInDatabase);

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				form.Grid.InnerGrid.Select(consol.Shipments.IndexOf(x => x.PK == shipment.PK));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();
				detachButton.PerformClick();

				var query1 = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK);
				query1.FetchOnlyFromLocalCache = true;
				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(query1);
				AssertNull("Compliance Risk has been removed from cache.", complianceRiskStatus);

				var query2 = new ZQuery(ComplianceCommodityDetailSchema.CCD_COR_ComplianceRisk, shipmentComplianceRisk.PK);
				query2.FetchOnlyFromLocalCache = true;
				var commodity = Factory.LoadTop1<ComplianceCommodityDetail>(query2);
				AssertNull("Commodity has been removed from cache.", commodity);
			}
		}

		[RequiresSTA]
		public void TestDetachButton_Click_RemainComplianceRisk_InDataBase()
		{
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("C00001001", Factory);
			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("S00001001", Constants.ShipmentTypes.StandardHouse, Factory);
			var shipmentComplianceRisk = Factory.New<ComplianceRiskStatus>();
			shipmentComplianceRisk.COR_ParentID = shipment.PK;
			shipmentComplianceRisk.CopyFromBooking = true;
			shipmentComplianceRisk.COR_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipmentComplianceRisk.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			shipmentComplianceRisk.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			shipmentComplianceRisk.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			shipmentComplianceRisk.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;
			consol.Shipments.Add(shipment);
			Factory.Save();

			AssertEquals(true, shipment.IsInDatabase);
			AssertEquals(true, shipmentComplianceRisk.IsInDatabase);

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				form.Grid.InnerGrid.Select(consol.Shipments.IndexOf(x => x.PK == shipment.PK));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();
				detachButton.PerformClick();

				var query = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK);
				query.FetchOnlyFromLocalCache = true;
				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(query);
				AssertNotNull("Compliance Risk is still remained in cache.", complianceRiskStatus);
			}
		}

		public void TestMasterChanged()
		{
			// Functionality is implemented in ShipmentCollectionHelper

			var oldMaster = Factory.New<ForwardingShipment>();
			var subShipment = oldMaster.CoLoadShipments.AddNew();

			var newMaster = Factory.New<ForwardingShipment>();
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddRange(oldMaster, newMaster);

			Factory.Save();

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();

				var helper = new Mock<IShipmentVsConsolGUIMessageHelper>(MockBehavior.Strict);

				using (ShipmentVsConsolGUIMessageHelper.OverrideHelperInstance(helper.Object))
				{
					helper
						.Setup(m => m.OnShipmentMasterChanged(It.IsAny<IShipmentVsConsolMessageHelper>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<CommonConsol>(),
							It.IsAny<MasterChangedEventArgs>()))
						.Callback(() => Assert(true));

					var collectionHelper = new ShipmentCollectionHelper(consol.Shipments, consol);
					subShipment.JS_JS_ColoadMasterShipment = newMaster.PK;
				}
			}
		}

		public void TestDisallowAddToGrid()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = true;
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				AssertEquals(true, consol.GridShipments.AllowNew);

				consol.UpdateAWBPrinted();
				AssertEquals(true, consol.GridShipments.AllowNew);
			}

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				AssertEquals(true, consol.GridShipments.AllowNew);

				consol.UpdateAWBPrinted();
				AssertEquals(false, consol.GridShipments.AllowNew);
			}
		}

		[ExpectNoExceptions]
		public void TestCleanUp()
		{
			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertNull("Prerequisite", GetEventHandler("OnPrintFinalMaster", consol));

			using (var form = new MockConsolForm2(consol))
			{
				form.Show();

				AssertNotNull(GetEventHandler("OnPrintFinalMaster", consol));
				AssertEquals(1, GetEventHandler("OnPrintFinalMaster", consol).GetInvocationList().Length);
			}

			AssertNull(GetEventHandler("OnPrintFinalMaster", consol));
		}

		[ExpectNoExceptions]
		public void TestShipmentsJobHeaderCleanUp()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			AssertNull("Prerequisite", shipment.ShipmentJobHeader);
			ZGlobalMutex mutex = null;

			using (var form = new MockConsolForm2(consol))
			{
				form.Show();

				shipment.CreateShipmentJobHeaderWithMutex();
				mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);
				AssertNotNull("Job Header Mutex", mutex);
				AssertEquals(true, mutex.IsLocked);
			}

			AssertEquals(false, mutex.IsLocked);
		}

		public void TestBKGNumberColumn()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				var shipmentGrid = (ConsolShipmentModuleButtonGrid)form.Controls.Find("ShipmentModuleButtonGrid", true)[0];
				List<string> shipmentGridColumnNames = shipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(col => col.ColumnName).ToList();
				AssertCollectionContains("Should contains BKG Number column", "BKGNumber", shipmentGridColumnNames);
			}

			var referenceNumbers = FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value;
			referenceNumbers.Remove(referenceNumbers.OfType<CustomsReferenceNumberType>().First(n => n.Code == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG));
			using (FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceNumbers))
			{
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					var shipmentGrid = (ConsolShipmentModuleButtonGrid)form.Controls.Find("ShipmentModuleButtonGrid", true)[0];
					List<string> shipmentGridColumnNames = shipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(col => col.ColumnName).ToList();
					AssertCollectionNotContains("Should not contains BKG Number column", "BKGNumber", shipmentGridColumnNames);
				}
			}
		}

		#region Pack Line Inspection Type Code

		public void TestUpdatePackLineInspectionTypeCode_Yes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "USLAX";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "USLAX";

				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_InspectionTypeCode = "UNK";

				using (var form = new ConsolForm(consol))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					shipment.JS_InspectionTypeCode = "MAI";

					AssertEquals("Should have displayed a dialog", "Question Do you want to apply this Inspection Type to all Pack Lines on this Shipment?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
					AssertEquals("Pack is updated", "MAI", packline.JL_InspectionTypeCode);
				}
			}
		}

		public void TestUpdatePackLineInspectionTypeCode_No()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "USLAX";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "USLAX";

				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_InspectionTypeCode = "UNK";

				using (var form = new ConsolForm(consol))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					shipment.JS_InspectionTypeCode = "MAI";

					AssertEquals("Should have displayed a dialog", "Question Do you want to apply this Inspection Type to all Pack Lines on this Shipment?", UnitTestUserNotification.Instance.PreviousMessages[0].ToString());
					AssertEquals("Pack is not updated", "UNK", packline.JL_InspectionTypeCode);
				}
			}
		}

		#endregion

		#region Implementation

		void AssertCheckComplianceRiskWhenConsolIsRisky(ZString consolComplianceRiskType, ZString shipmentComplianceRiskType)
		{
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("CONSOL", Factory);
			var consolComplianceRisk = new ComplianceRiskPlugInBusinessObject(consol);
			consolComplianceRisk.ComplianceRiskStatus.COR_OverallRisk = consolComplianceRiskType;

			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("SHIPMENT", Constants.ShipmentTypes.StandardHouse, Factory);
			var shipmentComplianceRisk = new ComplianceRiskPlugInBusinessObject(shipment);
			shipmentComplianceRisk.ComplianceRiskStatus.COR_OverallRisk = shipmentComplianceRiskType;

			var question = @"None Warning - Consol CONSOL has a Job Compliance Status of ‘Risk’ or ‘Override Clear’ within their Compliance Risk tab which may cause delays to the Shipment SHIPMENT.

Are you sure you want to proceed?

Click No – to cancel the operation.
Click Yes – to attach the shipment.";

			AssertAttachRaisesWarningMessage(consol, shipment, question);
		}

		void AssertCheckComplianceRiskWhenShipmentIsRisky(ZString consolComplianceRiskType, ZString shipmentComplianceRiskType)
		{
			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("CONSOL", Factory);
			var consolComplianceRisk = new ComplianceRiskPlugInBusinessObject(consol);
			consolComplianceRisk.ComplianceRiskStatus.COR_OverallRisk = consolComplianceRiskType;

			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("SHIPMENT", Constants.ShipmentTypes.StandardHouse, Factory);
			var shipmentComplianceRisk = new ComplianceRiskPlugInBusinessObject(shipment);
			shipmentComplianceRisk.ComplianceRiskStatus.COR_OverallRisk = shipmentComplianceRiskType;

			var question = @"None Warning - Shipment (SHIPMENT) has a Job Compliance Status of ‘Risk’ or ‘Override Clear’ within their Compliance Risk tab.
Attaching Shipment(s) with that Risk Status to a Consolidation may put other Shipments linked to the Consolidation at risk.

Are you sure you want to proceed?
Click No – to cancel the operation and review the shipment.
Click Yes – to attach the shipment and flag the Consol as ‘Risk’.";

			AssertAttachRaisesWarningMessage(consol, shipment, question);
		}

		void AssertAttachRaisesWarningMessage(ForwardingConsol consol, ForwardingShipment shipment, string question)
		{
			using (var form = new MockConsolForm3(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				var selected = new List<BusinessObject>() { shipment };
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Assert(!form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Assert(form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		MulticastDelegate GetEventHandler(string eventName, object source)
		{
			return (MulticastDelegate)source.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(source);
		}

		public class MockConsolForm : ConsolForm
		{
			public MockConsolForm(ForwardingConsol businessEntity)
				: base(businessEntity)
			{
			}

			public ZGridWithoutColumnStylesSerialisation InnerGrid
			{
				get { return shipmentGrid.InnerGrid; }
			}

			public ConsolShipmentModuleButtonGrid shipmentGrid;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				shipmentGrid = new ConsolShipmentModuleButtonGrid();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.ColumnName = "JS_UniqueConsignRef";

				shipmentGrid.BindToGridList = "GridShipments";
				shipmentGrid.BindToFindBoxList = "Shipments_List";
				shipmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
				Controls.Add(shipmentGrid);
			}
		}

		class MockConsolForm2 : ZForm
		{
			public MockConsolForm2(ForwardingConsol businessEntity)
				: base(businessEntity)
			{
			}

			public ConsolShipmentModuleButtonGrid ShipmentGrid;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				ShipmentGrid = new ConsolShipmentModuleButtonGrid();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.ColumnName = "JS_UniqueConsignRef";

				ShipmentGrid.BindToGridList = "GridShipments";
				ShipmentGrid.BindToFindBoxList = "Shipments_List";
				ShipmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
				Controls.Add(ShipmentGrid);
			}
		}

		class MockConsolForm3 : ZForm
		{
			public MockConsolForm3(ForwardingConsol businessEntity)
				: base(businessEntity)
			{
			}

			public ConsolShipmentModuleButtonGridForTest Grid;

			public ZGridWithoutColumnStylesSerialisation InnerGrid
			{
				get { return Grid.InnerGrid; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid = new ConsolShipmentModuleButtonGridForTest();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.ColumnName = "JS_UniqueConsignRef";

				Grid.BindToGridList = "GridShipments";
				Grid.BindToFindBoxList = "Shipments_List";
				Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
				Controls.Add(Grid);
			}
		}

		class ConsolShipmentModuleButtonGridForTest : ConsolShipmentModuleButtonGrid
		{
			public ConsolShipmentModuleButtonGridAttacherForTest Attacher { get; set; }

			protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			{
				return Attacher ?? (Attacher = new ConsolShipmentModuleButtonGridAttacherForTest(this.ParentConsol, destinationCollection, findBoxList, moduleID));
			}

			public new IBusinessObjectCollection Collection
			{
				get
				{
					return base.Collection;
				}
			}
		}

		class ConsolShipmentModuleButtonGridAttacherForTest : ConsolShipmentModuleButtonGrid.ConsolShipmentModuleButtonGridAttacher
		{
			public ConsolShipmentModuleButtonGridAttacherForTest(ForwardingConsol consol, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(consol, destinationCollection, findBoxList, moduleID)
			{
			}

			public bool CheckSelected(List<BusinessObject> selected)
			{
				return CheckAttaching(selected);
			}

			public void Attach(List<BusinessObject> list)
			{
				if (CheckAttaching(list))
				{
					AttachItemsCore(DestinationCollection, list);
					OnAttached();
				}
			}
		}

		#endregion
	}

	[TestedType(typeof(ConsolShipmentModuleButtonGrid))]
	class ConsolShipmentModuleButtonGridModuleButtonGridTest : ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
	{
	}
}
