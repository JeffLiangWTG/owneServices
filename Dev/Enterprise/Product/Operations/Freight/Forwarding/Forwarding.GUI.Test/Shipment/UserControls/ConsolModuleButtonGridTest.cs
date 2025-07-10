using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ConsolModuleButtonGridTest : TestCaseWithFactory
	{
		#region TestEditConsolLicenceCheckpoint

		public void TestEditConsolLicenceCheckpoint()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			((BusinessObject)declaration)[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;

			var consol = shipment.Consols.AddNew();
			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.JobShipment.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(shipment))
			{
				AssertEditLicenceCheckpoint(form, Env.Licence.Forwarder);
			}
		}

		void AssertEditLicenceCheckpoint(ZForm parentForm, LicenceCheckpoint expected)
		{
			using (var userControl = new ConsolModuleButtonGridControl())
			{
				parentForm.Controls.Add(userControl);

				var grid = userControl.ConsolModuleButtonGrid;
				grid.SelectFirstRowIfOnlyRowInGrid();
				grid.EditButton.PerformClick();

				using (var shownForm = (ZForm)grid.LastShownZForm)
				{
					AssertEquals("ContainsCheckpoint(" + expected.Name + ")", true, shownForm.LicensedComponentManager.ContainsCheckpoint(expected));
				}
			}
		}

		#endregion

		public void TestNewButton_Click()
		{
			var master = FreightTestHelper.GetShipment<ForwardingShipment>("S00001002", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("S00001001", Constants.ShipmentTypes.AssemblyMaster, Factory);

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			string message = "message";

			// Has errors
			helper.SetupSequence(m => m.IsAllowedToAddNewConsol(out message, It.IsAny<CommonShipment>()))
				.Returns(false)
				.Returns(true)
				.Returns(true)
				.Returns(true)
				.CallBase();

			// No master
			helper.Setup(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);
			helper.Setup(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(string.Empty);

			helper.SetupSequence(m => m.IsAllowedToAddNewShipment(out message, It.IsAny<CommonConsol>()))
				.Returns(false)
				.Returns(false)
				.CallBase(); // Last shown form is Consol Form that set up GridShipments.AllowAddNew

			string errorMessage = "Error message";
			string questionIsSubShipment = @"Question The shipment S00001001 is a sub-shipment of the master/lead shipment S00001002. A new consol will be attached to this shipment only, without attaching to its master.

If you would like to attach a new consol to this shipment, its master/lead and all sub-shipments of its master/lead, you need to attach a new consol to the master/lead shipment S00001002 instead.";

			string noneMessage = "None ";

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>().First();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					// Has errors
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals("Should be Error", errorMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

					// No master
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					using (form.Grid.LastShownZForm)
					{
						AssertEquals("Should be NO Errors", noneMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					}

					// Has master - Cancel
					shipment.JS_JS_ColoadMasterShipment = master.PK;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					button.PerformClick();
					AssertEquals("Should be IsSubShipment Question", questionIsSubShipment, UnitTestUserNotification.Instance.LastMessage.ToString());

					// Has master - OK
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					button.PerformClick();
					using (form.Grid.LastShownZForm)
					{
						AssertEquals("Should be IsSubShipment Question", questionIsSubShipment, UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		public void TestNewButton_Click_ForStandAloneShipment()
		{
			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("S00001001", Constants.ShipmentTypes.StandardHouse, Factory);
			shipment.JS_IsBooking = ZBool.True;
			shipment.JS_CFSReference = "CFS Ref1";
			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			var message = "message";

			helper.Setup(m => m.IsAllowedToAddNewConsol(out message, It.IsAny<CommonShipment>())).Returns(true);
			helper.SetupSequence(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>()))
				.Returns(string.Empty)
				.Returns(string.Empty)
				.CallBase();
			helper.SetupSequence(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>()))
				.Returns(string.Empty)
				.Returns(string.Empty);
			helper.Setup(m => m.IsAllowedToAddNewShipment(out message, It.IsAny<CommonConsol>())).Returns(false);

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				shipment.Consols.RemoveAll();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>().First();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();

					using (form.Grid.LastShownZForm)
					{
						form.Grid.LastShownZForm.Show();
						var newConsol = form.Grid.LastShownZForm.BusinessEntityForPersistingForm as CommonConsol;
						AssertEquals("CFS Ref1", newConsol.JK_BookingReference);
					}
				}
			}
		}

		public void TestGivenShipmentWithSCN_WhenClickingNewButton_ThenConsolModeShouldBeSynchronizedToSCN()
		{
			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("S00001001", Constants.ShipmentTypes.StandardHouse, Factory);
			shipment.JS_PackingMode = Constants.ContainerModes.ShippersConsol;
			Factory.Save();

			var message = "";

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);
			helper.Setup(m => m.IsAllowedToAddNewConsol(out message, It.IsAny<CommonShipment>())).Returns(true);
			helper.SetupSequence(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>()))
				.Returns(string.Empty)
				.Returns(string.Empty)
				.CallBase();
			helper.SetupSequence(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>()))
				.Returns(string.Empty)
				.Returns(string.Empty);
			helper.Setup(m => m.IsAllowedToAddNewShipment(out message, It.IsAny<CommonConsol>())).Returns(false);

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				shipment.Consols.RemoveAll();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>().First();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();

					using (form.Grid.LastShownZForm)
					{
						form.Grid.LastShownZForm.Show();
						var newConsol = form.Grid.LastShownZForm.BusinessEntityForPersistingForm as CommonConsol;
						AssertEquals("Given shipment with SCN, when clicking new button, then consol mode should be synchronized to SCN", Constants.ContainerModes.ShippersConsol, newConsol.JK_ConsolMode);
					}
				}
			}
		}

		public void TestAttachButton_Click()
		{
			var master = FreightTestHelper.GetShipment<ForwardingShipment>("S00001002", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("S00001001", Constants.ShipmentTypes.AssemblyMaster, Factory);

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			var attachRequestWithErrors = new ShipmentConsolAttachRequest(() => "ERROR?", null);

			var attachRequest = new ShipmentConsolAttachRequest(null, null);

			helper.SetupSequence(m => m.IsAllowedToAttachConsol(It.IsAny<CommonShipment>(), It.IsAny<CommonConsol>()))
				.Returns(attachRequestWithErrors) // Has errors
				.Returns(attachRequest) // No master
				.Returns(attachRequest) // Has master - Cancel
				.Returns(attachRequest) // Has master - OK
				.CallBase();

			string questionIsSubShipment = @"Question The shipment S00001001 is a sub-shipment of the master/lead shipment S00001002. A new consol will be attached to this shipment only, without attaching to its master.

If you would like to attach a new consol to this shipment, its master/lead and all sub-shipments of its master/lead, you need to attach a new consol to the master/lead shipment S00001002 instead.";

			string noneMessage = "None ";

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					// Has errors
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals("Should have errors", "Error ERROR?", UnitTestUserNotification.Instance.LastMessage.ToString());

					// No master
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					using (form.Grid.Attacher.LastShownAttachPopupForTesting)
					{
						AssertEquals("Should be NO Errors", noneMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					}

					// Has master - Cancel
					shipment.JS_JS_ColoadMasterShipment = master.PK;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					button.PerformClick();
					AssertEquals("Should be IsSubShipment Question", questionIsSubShipment, UnitTestUserNotification.Instance.LastMessage.ToString());

					// Has master - OK
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					button.PerformClick();
					using (form.Grid.Attacher.LastShownAttachPopupForTesting)
					{
						AssertEquals("Should be IsSubShipment Question", questionIsSubShipment, UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		[TestDate(2013, 02, 21)]
		public void TestAttacher()
		{
			var today = ZDateTimeOffset.Today;

			var consol = FreightTestHelper.GetConsol<ForwardingConsol>("C00001001", Factory);

			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("S00001001", Constants.ShipmentTypes.AssemblyMaster, Factory);
			shipment.Consols.Add(consol);

			var sUB = FreightTestHelper.GetShipment("SUB", shipment, Constants.ShipmentTypes.StandardHouse, Factory);

			//printed final master
			var pFM = FreightTestHelper.GetConsol<ForwardingConsol>("PFM", Factory);
			pFM.Logs.AddNew(AutoEvents.DocumentSent, ForwardingConsol.MAWBPrintedLogReference, today);
			pFM.UpdateAWBPrinted();

			//past cut off date
			var pCD = FreightTestHelper.GetConsol<ForwardingConsol>("PCD", Factory);
			pCD.JK_ConsolCutOffDateLocal = today.AddDays(-1).ToZDateTime();

			//direct with shipment
			var dIR = FreightTestHelper.GetConsol<ForwardingConsol>("DIR", Factory);
			dIR.JK_AgentType = Constants.AgentType.Direct;
			dIR.Shipments.AddNew();

			//direct with NO shipment
			var dIR_EMPTY = FreightTestHelper.GetConsol<ForwardingConsol>("DIR_EMPTY", Factory);
			dIR_EMPTY.JK_AgentType = Constants.AgentType.Direct;

			//standard consol
			var sTD = FreightTestHelper.GetConsol<ForwardingConsol>("STD", Factory);

			Factory.Save();

			string commonMessage = "Question Of the consols you are trying to attach to the shipment S00001001, there are consols that cannot be attached for the following reasons:";

			string errorPrintedFinalMaster = string.Format(
				"Cannot attach the consol PFM to the shipment S00001001 as the Master Bill for PFM has already been printed on {0}.\r\n\r\n{1}",
				today.ToZDateTime(),
				Env.Security.GetErrorMessageForNotAllowed(Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster));

			string errorPastCutOffDate = string.Format(
				"Cannot attach the consol PCD to the shipment S00001001 as the consol Cut Off Date has passed.\r\n\r\n{0}",
				Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolAttachDetachShipmentAfterCutOffDate));

			string errorIsDirectConsol = "Cannot attach the Direct consol DIR to the shipment S00001001 because the consol already has another shipment attached.";

			string errorShipmentCanHaveDirectConsol = "Cannot attach the Direct consol DIR_EMPTY to the shipment S00001001 because the shipment is not a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no master shipment.";

			string errorIsDirectShipment = "The shipment S00001001 is already attached to at least one other Direct Consol. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).";

			string errorConsolCanHaveDirectShipment = "Cannot attach the Direct consol DIR_EMPTY to the shipment S00001001 because the shipment is already attached to at least one other Non-Direct Consol.";

			string questionToSkip = string.Format(
				"{0}\r\n{1}\r\n{2}",
				commonMessage,
				errorPastCutOffDate,
				errorPrintedFinalMaster);

			string questionToAttachDirect = "Question You can not attach both Direct and Non-Direct consols to the same shipment. Would you like to ignore all Non-Direct consols?\r\n\r\nSelect Yes to attach only Direct Consols.\r\nSelect No to attach only Non-Direct Consols.\r\nSelect Cancel to abort the operation.";

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				var selected = new List<BusinessObject>() { pFM, pCD, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals("Consols should NOT be added", false, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has NOT changed", selected, pFM, pCD, sTD);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, sTD);

				consol.Shipments.RemoveAll();
				FreightTestHelper.AssertConsolCollection("shipment should have NO consols", shipment.Consols);
				FreightTestHelper.AssertShipmentCollection("consol should have NO shipments", consol.Shipments);
				FreightTestHelper.AssertShipmentCollection("shipment should have sub-shipment", shipment.CoLoadShipments, sUB);
				selected = new List<BusinessObject>() { pFM, pCD, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, sTD);

				shipment.CoLoadShipments.RemoveAll();
				FreightTestHelper.AssertShipmentCollection(shipment.CoLoadShipments);
				questionToSkip = string.Format("{0}\r\n{1}\r\n{2}", commonMessage, errorPastCutOffDate, errorPrintedFinalMaster);
				selected = new List<BusinessObject>() { pFM, pCD, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, sTD);

				Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = true;
				questionToSkip = string.Format("{0}\r\n{1}", commonMessage, errorPastCutOffDate);
				selected = new List<BusinessObject>() { pFM, pCD, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, pFM, sTD);

				Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
				selected = new List<BusinessObject>() { pFM, pCD, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, pFM, pCD, sTD);

				questionToSkip = string.Format("{0}\r\n{1}", commonMessage, errorIsDirectConsol);
				selected = new List<BusinessObject>() { dIR, dIR_EMPTY };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, dIR_EMPTY);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
				questionToSkip = string.Format("{0}\r\n{1}\r\n{2}", commonMessage, errorIsDirectConsol, errorShipmentCanHaveDirectConsol);
				selected = new List<BusinessObject>() { dIR, dIR_EMPTY };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should not be added", false, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
				questionToSkip = string.Format("{0}\r\n{1}", commonMessage, errorIsDirectConsol);
				selected = new List<BusinessObject>() { dIR, dIR_EMPTY };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, dIR_EMPTY);

				shipment.Consols.Add(consol);
				consol.JK_AgentType = Constants.AgentType.Direct;
				questionToSkip = string.Format("{0}\r\n{1}", commonMessage, errorIsDirectShipment);
				selected = new List<BusinessObject>() { sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should not be added", false, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected);

				consol.JK_AgentType = Constants.AgentType.Agent;
				questionToSkip = string.Format("{0}\r\n{1}", commonMessage, errorConsolCanHaveDirectShipment);
				selected = new List<BusinessObject>() { dIR_EMPTY };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should not be added", false, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected);

				shipment.Consols.RemoveAll();
				selected = new List<BusinessObject>() { sTD, dIR_EMPTY };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to choose Direct or Non-Direct", questionToAttachDirect, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, dIR_EMPTY);

				selected = new List<BusinessObject>() { sTD, dIR_EMPTY };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to choose Direct or Non-Direct", questionToAttachDirect, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, sTD);
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

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				var selected = new List<BusinessObject>() { consol };
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

		[TestDate(2016, 11, 7)]
		public void TestAttacher_ForStandAloneShipment_MatchEarliestConsol()
		{
			var today = ZDateTime.Today;

			var voyage = Factory.New<JobVoyage>();
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = "ADALV";

			var destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = "USLAX";

			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			Factory.Save();

			var sub = FreightTestHelper.GetShipment<ForwardingShipment>("SUB", Constants.ShipmentTypes.StandardHouse, Factory);
			sub.JS_RL_NKOrigin = "ADALV";
			sub.JS_IsBooking = ZBool.True;
			sub.JS_CFSReference = "CFS Ref1";
			sub.JS_JX = sailing.PK;

			//printed final master
			var pfm = FreightTestHelper.GetConsol<ForwardingConsol>("PFM", Factory);
			pfm.JK_RL_NKLoadPort = "HKHKG";
			var transport1 = pfm.Transports[0];
			transport1.JW_ETD = today.AddDays(-1);

			//past cut off date
			var pcd = FreightTestHelper.GetConsol<ForwardingConsol>("PCD", Factory);
			pcd.JK_ConsolCutOffDateLocal = today.AddDays(-2);
			pcd.JK_RL_NKLoadPort = "NZAKL";
			var transport2 = pcd.Transports[0];
			transport2.JW_ETD = today.AddDays(-2);

			//standard consol
			var std = FreightTestHelper.GetConsol<ForwardingConsol>("STD", Factory);
			std.JK_RL_NKLoadPort = "AUSYD";
			var transport3 = std.Transports[0];
			transport3.JW_ETD = today.AddDays(-3);

			Factory.Save();

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;

			using (var form = new MockShipmentForm(sub))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
				var selected = new List<BusinessObject>() { pfm, pcd, std };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.Grid.Attacher.LastShownAttachPopupForTesting.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { pfm, pcd, std });
				form.Grid.Attacher.LastShownAttachPopupForTesting.Dispose();
				AssertEquals("CFS Ref1", std.JK_BookingReference);
				AssertEquals(true, std.IsAttachedToStandAloneShipment);
			}
		}

		[TestDate(2016, 11, 7)]
		public void TestAttacher_ForStandAloneShipment_MatchConsolOrigin()
		{
			var today = ZDateTime.Today;

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

			Factory.Save();

			var sub = FreightTestHelper.GetShipment<ForwardingShipment>("SUB", Constants.ShipmentTypes.StandardHouse, Factory);
			sub.JS_IsBooking = ZBool.True;
			sub.JS_CFSReference = "CFS Ref1";
			sub.JS_RL_NKOrigin = "AUSYD";
			sub.JS_JX = sailing.PK;

			//printed final master
			var pfm = FreightTestHelper.GetConsol<ForwardingConsol>("PFM", Factory);
			pfm.JK_RL_NKLoadPort = "HKHKG";
			pfm.JK_RL_NKDischargePort = "NZAKL";
			var transport1 = pfm.Transports[0];
			transport1.JW_ETD = today.AddDays(-3);

			//past cut off date
			var pcd = FreightTestHelper.GetConsol<ForwardingConsol>("PCD", Factory);
			pcd.JK_ConsolCutOffDateLocal = today.AddDays(-1);
			pcd.JK_RL_NKLoadPort = "NZAKL";
			var transport2 = pcd.Transports[0];
			transport2.JW_ETD = today.AddDays(-2);

			//standard consol
			var std = FreightTestHelper.GetConsol<ForwardingConsol>("STD", Factory);
			std.JK_RL_NKLoadPort = "AUSYD";
			var transport3 = std.Transports[0];
			transport3.JW_ETD = today.AddDays(-1);

			Factory.Save();

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;

			using (var form = new MockShipmentForm(sub))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
				var selected = new List<BusinessObject>() { pfm, pcd, std };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.Grid.Attacher.LastShownAttachPopupForTesting.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { pfm, pcd, std });
				form.Grid.Attacher.LastShownAttachPopupForTesting.Dispose();
				AssertEquals("CFS Ref1", std.JK_BookingReference);
				AssertEquals(true, std.IsAttachedToStandAloneShipment);
			}
		}

		public void TestAttach_ChecksComplianceRisk_WhenConsolIsRisky()
		{
			AssertCheckComplianceRiskWhenConsolIsRisky(ComplianceRiskStatusCodeList.Codes.Blocked, ComplianceRiskStatusCodeList.Codes.Clear);
			AssertCheckComplianceRiskWhenConsolIsRisky(ComplianceRiskStatusCodeList.Codes.OverrideClear, ComplianceRiskStatusCodeList.Codes.Clear);
		}

		public void TestAttach_ChecksComplianceRisk_WhenShipmentIsRisky()
		{
			AssertCheckComplianceRiskWhenShipmentIsRisky(ComplianceRiskStatusCodeList.Codes.Clear, ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertCheckComplianceRiskWhenShipmentIsRisky(ComplianceRiskStatusCodeList.Codes.OverrideClear, ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertCheckComplianceRiskWhenShipmentIsRisky(ComplianceRiskStatusCodeList.Codes.Clear, ComplianceRiskStatusCodeList.Codes.OverrideClear);
		}

		public void TestDetachButton_Click()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();
			var consol3 = shipment.Consols.AddNew();

			Factory.Save();

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();

				form.Grid.InnerGrid.Select(0);
				form.Grid.InnerGrid.Select(1);

				var selectedConsols = new[] { consol1, consol2 };

				var helper = new Mock<IShipmentVsConsolGUIMessageHelper>(MockBehavior.Strict);

				using (ShipmentVsConsolGUIMessageHelper.OverrideHelperInstance(helper.Object))
				{
					// NOT Allowed
					helper
						.Setup(m => m.IsAllowedToDetachConsols(
							FreightShipmentVsConsolMessageHelper.Instance,
							shipment,
							It.Is<IEnumerable<CommonConsol>>(x => AssertElements(x, selectedConsols))))
						.Returns(false);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					detachButton.PerformClick();
					Assert("Base detach dialog was not shown", UnitTestUserNotification.Instance.LastMessage.WasNone);

					// Allowed
					helper
						.Setup(m => m.IsAllowedToDetachConsols(
							FreightShipmentVsConsolMessageHelper.Instance,
							shipment,
							It.Is<IEnumerable<CommonConsol>>(x => AssertElements(x, selectedConsols))))
						.Returns(true);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					detachButton.PerformClick();
					Assert("Base detach dialog was shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(form.Grid.DetachMessage.Caption));
				}
			}
		}

		bool AssertElements(IEnumerable<CommonConsol> commonConsols, ForwardingConsol[] selectedConsols)
		{
			AssertContainsExactElementsInAnyOrder(commonConsols, selectedConsols);
			return true;
		}

		#region Helper Methods

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
			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				var selected = new List<BusinessObject>() { consol };
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Assert(!form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Assert(form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		#endregion

		#region Test Class

		class MockShipmentForm : ZForm
		{
			public MockShipmentForm(ForwardingShipment businessEntity)
				: base(businessEntity)
			{
			}

			public ConsolModuleButtonGridForTest Grid;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid = new ConsolModuleButtonGridForTest();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.ColumnName = "JK_UniqueConsignRef";

				Grid.BindToFindBoxList = "Lookups+Consols_List";
				Grid.BindToGridList = "Consols";
				Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				Controls.Add(Grid);
			}
		}

		class ConsolModuleButtonGridForTest : ConsolModuleButtonGrid
		{
			public ZToolStripButton EditButton
			{
				get
				{
					var toolStrip = Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
					return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).OfType<ZToolStripButton>().First();
				}
			}

			public ConsolModuleButtonGridAttacherForTest Attacher { get; set; }

			protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			{
				return Attacher ?? (Attacher = new ConsolModuleButtonGridAttacherForTest(this.ParentShipment, destinationCollection, findBoxList, moduleID));
			}
		}

		class ConsolModuleButtonGridAttacherForTest : ConsolModuleButtonGrid.ConsolModuleButtonGridAttacher
		{
			public ConsolModuleButtonGridAttacherForTest(CommonShipment shipment, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(shipment, destinationCollection, findBoxList, moduleID)
			{
			}

			public bool CheckSelected(List<BusinessObject> selected)
			{
				return CheckAttaching(selected);
			}
		}

		class ConsolModuleButtonGridControl : ZUserControl
		{
			public ConsolModuleButtonGridControl()
			{
				InitializeComponent();
			}

			void InitializeComponent()
			{
				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo() { ColumnName = "zTextBoxColumnStyleInfo1" };
				this.ConsolModuleButtonGrid = new ConsolModuleButtonGridForTest();

				this.BindingSource.DataSourceType = typeof(ForwardingConsol);

				ConsolModuleButtonGrid.BindToFindBoxList = "Lookups.Consols_List";
				ConsolModuleButtonGrid.BindToGridList = "Consols";
				ConsolModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				this.Controls.Add(ConsolModuleButtonGrid);
			}

			internal ConsolModuleButtonGridForTest ConsolModuleButtonGrid;
		}

		#endregion
	}

	[TestedType(typeof(ConsolModuleButtonGrid))]
	class ConsolModuleButtonGridModuleButtonGridTest : ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
	{
	}
}
