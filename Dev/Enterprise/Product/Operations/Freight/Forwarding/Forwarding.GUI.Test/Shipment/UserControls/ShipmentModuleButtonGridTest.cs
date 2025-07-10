using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Licensing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ShipmentModuleButtonGridTest : TestCaseWithFactory
	{
		#region TestEditShipmentLicenceCheckpoint

		public void TestEditShipmentLicenceCheckpoint()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var relatedShipment = shipment.CoLoadShipments.AddNew();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			((BusinessObject)declaration)[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;

			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.JobShipment.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(shipment))
			{
				AssertGridEditLicenceCheckpoint(form, Env.Licence.Forwarder);
			}
		}

		void AssertGridEditLicenceCheckpoint(ZForm parentForm, LicenceCheckpoint expected)
		{
			using (var userControl = new ShipmentModuleButtonGridControl())
			{
				userControl.SetDataBinding(parentForm.BusinessEntity, "");
				parentForm.Controls.Add(userControl);

				var grid = userControl.ShipmentModuleButtonGrid;
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
			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("shipment", Constants.ShipmentTypes.CoLoadMaster, Factory);

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			var message = "message";
			helper
				.SetupSequence(m => m.IsAllowedToAddNewSubShipment(out message, It.IsAny<CommonShipment>()))
				.Returns(false)
				.Returns(true)
				.CallBase();

			string errorMessage = "Error message";
			string noneMessage = "None ";

			using (var form = new MockConsolForm2(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>().First();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals("Should be Error", errorMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					using (form.Grid.LastShownZForm)
					{
						AssertEquals("Should be NO Errors", noneMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		public void TestAttachButton_Click()
		{
			var shipment = FreightTestHelper.GetShipment<ForwardingShipment>("shipment", Constants.ShipmentTypes.CoLoadMaster, Factory);

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			var message = "message";
			helper.SetupSequence(m =>
					m.IsAllowedToAttachSubShipment(out message, It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>()))
				.Returns(false)
				.Returns(true)
				.CallBase();

			string errorMessage = "Error message";
			string noneMessage = "None ";

			using (var form = new MockConsolForm2(shipment))
			{
				form.Show();
				var toolStrip = form.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals("Should be Error", errorMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					using (form.Grid.Attacher.LastShownAttachPopupForTesting)
					{
						AssertEquals("Should be NO Errors", noneMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		public void TestAttacher()
		{
			var mAS = FreightTestHelper.GetShipment<ForwardingShipment>("MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var sUB_1 = FreightTestHelper.GetShipment<ForwardingShipment>("SUB_1", Constants.ShipmentTypes.StandardHouse, Factory);
			var sUB_2 = FreightTestHelper.GetShipment<ForwardingShipment>("SUB_2", Constants.ShipmentTypes.StandardHouse, Factory);

			Factory.Save();

			string questionToSkip = "Among of the shipments you are trying to attach to the master-shipment MAS, there are shipments that cannot be attached and will be skipped for the following reasons:";

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			var message = "message";
			helper.SetupSequence(m =>
					m.IsAllowedToAttachSubShipment(out message, It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>()))
				.Returns(false)
				.Returns(true)
				.Returns(false)
				.Returns(true)
				.CallBase();

			using (var form = new MockConsolForm2(mAS))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();

				button.PerformClick();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					var selected = new List<BusinessObject>() { sUB_1, sUB_2 };
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					AssertEquals("Shipments should NOT be added", false, form.Grid.Attacher.CheckSelected(selected));
					Assert("Should be ask to skip shipments", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(questionToSkip));
					FreightTestHelper.AssertShipmentCollection("selected shipments should NOT be changed", selected, sUB_1, sUB_2);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					AssertEquals("Shipments should NOT be added", true, form.Grid.Attacher.CheckSelected(selected));
					Assert("Should be ask to skip shipments", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(questionToSkip));
					FreightTestHelper.AssertShipmentCollection("selected shipments should be changed", selected, sUB_2);
				}
			}
		}

		[RequiresSTA]
		public void TestAttacher_ForStandAloneShipment_MatchOriginConsol()
		{
			var mas = FreightTestHelper.GetShipment<ForwardingShipment>("MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);

			var sub1 = FreightTestHelper.GetShipment<ForwardingShipment>("SUB_1", Constants.ShipmentTypes.StandardHouse, Factory);
			sub1.JS_IsBooking = ZBool.True;
			sub1.JS_CFSReference = "CFS Ref1";
			sub1.JS_RL_NKOrigin = "AUSYD";

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

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKLoadPort = "HKHKG";
			mas.Consols.Add(consol1);

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKLoadPort = "AUSYD";
			mas.Consols.Add(consol2);

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>();

			var message = "message";
			helper.Setup(m => m.IsAllowedToAttachSubShipment(out message, mas, sub1)).Returns(true);
			helper.Setup(m => m.CheckRelatedReceivingAgents(new[] { mas }, new[] { consol1 })).Returns(string.Empty);
			helper.Setup(m => m.CheckRelatedSendingAgents(new[] { mas }, new[] { consol1 })).Returns(string.Empty);

			var attachRequest = new ShipmentConsolAttachRequest(null, null);
			helper.Setup(m => m.IsAllowedToAttachConsol(sub1, consol1)).Returns(attachRequest);

			using (var form = new MockConsolForm2(mas))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();

				button.PerformClick();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					form.Grid.Attacher.LastShownAttachPopupForTesting.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { sub1 });
					form.Grid.Attacher.LastShownAttachPopupForTesting.Dispose();
					AssertEquals("CFS Ref1", consol2.JK_BookingReference);
					AssertEquals(true, consol2.IsAttachedToStandAloneShipment);
				}
			}
		}

		[TestDate(2016, 11, 7)]
		public void TestAttacher_ForStandAloneShipment_MatchEarliestConsol()
		{
			var today = ZDateTime.Today;

			var mas = FreightTestHelper.GetShipment<ForwardingShipment>("MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);

			var sub1 = FreightTestHelper.GetShipment<ForwardingShipment>("SUB_1", Constants.ShipmentTypes.StandardHouse, Factory);
			sub1.JS_IsBooking = ZBool.True;
			sub1.JS_CFSReference = "CFS Ref1";
			sub1.JS_RL_NKOrigin = "AUSYD";

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

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKLoadPort = "HKHKG";
			var transport1 = consol1.Transports[0];
			transport1.JW_ETD = today.AddDays(-1);
			mas.Consols.Add(consol1);

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKLoadPort = "USLAX";
			var transport2 = consol2.Transports[0];
			transport2.JW_ETD = today.AddDays(-2);
			mas.Consols.Add(consol2);

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>();

			string message = "message";

			// Make sure to ignore arguments on all of these, or else test failures will cause a stack overflow. Very difficult to debug.
			helper.Setup(m => m.IsAllowedToAttachSubShipment(out message, mas, sub1)).Returns(true);
			helper.Setup(m => m.CheckRelatedReceivingAgents(new[] { mas }, new[] { consol1 })).Returns(string.Empty);
			helper.Setup(m => m.CheckRelatedSendingAgents(new[] { mas }, new[] { consol1 })).Returns(string.Empty);

			var attachRequest = new ShipmentConsolAttachRequest(null, null);
			helper.Setup(h => h.IsAllowedToAttachConsol(sub1, consol1)).Returns(attachRequest);

			using (var form = new MockConsolForm2(mas))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					form.Grid.Attacher.LastShownAttachPopupForTesting.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { sub1 });
					form.Grid.Attacher.LastShownAttachPopupForTesting.Dispose();
					AssertEquals("CFS Ref1", consol2.JK_BookingReference);
					AssertEquals(true, consol2.IsAttachedToStandAloneShipment);
				}
			}
		}

		public void TestDetachButton_Click_Maintain()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			using (MockShipmentForm shipmentForm = new MockShipmentForm(Factory.NewWithValidTestData<ForwardingShipment>()))
			{
				Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = false;
				shipmentForm.DetachButton_PerformClick();
				AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainShipmentAllowDetachingSubShipments), UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				shipmentForm.DetachButton_PerformClick();
				AssertNotEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainShipmentAllowDetachingSubShipments), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestDetachButton_Click()
		{
			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;

			var mAS = FreightTestHelper.GetShipment<ForwardingShipment>("MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var sUB_1 = FreightTestHelper.GetShipment("SUB_1", mAS, Constants.ShipmentTypes.StandardHouse, Factory);
			var sUB_2 = FreightTestHelper.GetShipment("SUB_2", mAS, Constants.ShipmentTypes.StandardHouse, Factory);
			var sUB_3 = FreightTestHelper.GetShipment("SUB_3", mAS, Constants.ShipmentTypes.StandardHouse, Factory);
			var sUB_4 = FreightTestHelper.GetShipment("SUB_4", mAS, Constants.ShipmentTypes.StandardHouse, Factory);
			var consol = Factory.New<ForwardingConsol>();

			Factory.Save();

			using (var form = new MockConsolForm2(mAS))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();

				var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					string message;
					CommonConsol[] consolsToDetach = Array.Empty<CommonConsol>();

					// Nothing to detach
					message = "";
					helper
						.Setup(m => m.GetConsolsToDetachFromSubShipments(
							out message,
							It.IsAny<CommonConsol>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<IEnumerable<CommonShipment>>()))
						.Returns(consolsToDetach);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					form.InnerGrid.SelectSingleElement(sUB_1);
					detachButton.PerformClick();
					Assert("Base detach dialog was shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(form.Grid.DetachMessage.Caption));

					// Detach consols? NO
					consolsToDetach = new CommonConsol[] { consol };
					message = "Detach consols?";
					helper
						.Setup(m => m.GetConsolsToDetachFromSubShipments(
							out message,
							It.IsAny<CommonConsol>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<IEnumerable<CommonShipment>>()))
						.Returns(consolsToDetach);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					form.InnerGrid.SelectSingleElement(sUB_2);
					detachButton.PerformClick();

					Assert("Base detach dialog was shown", UnitTestUserNotification.Instance.PreviousMessages[1].Text.StartsWith(form.Grid.DetachMessage.Caption));
					AssertEquals("Question Detach consols?", UnitTestUserNotification.Instance.LastMessage.ToString());

					// Detach consols? YES, allowed
					consolsToDetach = new CommonConsol[] { consol };
					message = "Detach consols?";
					helper.Setup(m => m.GetConsolsToDetachFromSubShipments(
							out message,
							It.IsAny<CommonConsol>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<IEnumerable<CommonShipment>>()))
						.Returns(consolsToDetach);

					var detachRequest = new ShipmentConsolDetachRequest("", null, null, null);
					helper.Setup(m =>
							m.IsAllowedToDetachConsols(It.IsAny<CommonShipment>(),
								It.IsAny<IEnumerable<CommonConsol>>()))
						.Returns(detachRequest);
					helper
						.Setup(m => m.DetachShipmentsFromConsols(
							It.IsAny<IEnumerable<CommonShipment>>(),
							It.IsAny<IEnumerable<CommonConsol>>()));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					form.InnerGrid.SelectSingleElement(sUB_3);
					detachButton.PerformClick();

					Assert("Base detach dialog was shown", UnitTestUserNotification.Instance.PreviousMessages[1].Text.StartsWith(form.Grid.DetachMessage.Caption));
					AssertEquals("Question Detach consols?", UnitTestUserNotification.Instance.LastMessage.ToString());

					consolsToDetach = new CommonConsol[] { consol };
					message = "Detach consols?";
					helper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message, It.IsAny<CommonConsol>(),
						It.IsAny<CommonShipment>(),
						It.IsAny<CommonShipment>(),
						It.IsAny<IEnumerable<CommonShipment>>()))
						.Returns(consolsToDetach);

					detachRequest = new ShipmentConsolDetachRequest("NOT ALLOWED!", null, null, null);
					helper
						.Setup(m => m.IsAllowedToDetachConsols(
							It.IsAny<CommonShipment>(),
							It.IsAny<IEnumerable<CommonConsol>>()))
						.Returns(detachRequest);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					form.InnerGrid.SelectSingleElement(sUB_4);
					detachButton.PerformClick();

					Assert("Base detach dialog was shown", UnitTestUserNotification.Instance.PreviousMessages[2].Text.StartsWith(form.Grid.DetachMessage.Caption));
					AssertEquals("Question Detach consols?", UnitTestUserNotification.Instance.PreviousMessages[1].ToString());
					AssertEquals("Information NOT ALLOWED!", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
		}

		public void TestDetachButton_DirectConsol_AssemblyWithSubShipments_NoDetachConsols()
		{
			var masterShipment = FreightTestHelper.GetShipment<ForwardingShipment>("masterShipment", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var subShipment = FreightTestHelper.GetShipment("subShipment", masterShipment, Constants.ShipmentTypes.StandardHouse, Factory);
			var consol = Factory.New<ForwardingConsol>();

			masterShipment.Consols.Add(consol);
			Factory.Save();

			using (var form = new MockConsolForm2(masterShipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();

				var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					string message;
					CommonConsol[] consolsToDetach = Array.Empty<CommonConsol>();

					consolsToDetach = new CommonConsol[] { consol };
					message = "Detach consols?";
					helper.Setup(m => m.GetConsolsToDetachFromSubShipments(
							out message,
							It.IsAny<CommonConsol>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<IEnumerable<CommonShipment>>()))
						.Returns(consolsToDetach);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					form.InnerGrid.SelectSingleElement(subShipment);
					detachButton.PerformClick();

					Assert("Base detach dialog was shown", UnitTestUserNotification.Instance.PreviousMessages[1].Text.StartsWith(form.Grid.DetachMessage.Caption));
					AssertEquals("Question Detach consols?", UnitTestUserNotification.Instance.LastMessage.ToString());
					Assert("Not detaching consol from subshipment has made consol invalid.", consolsToDetach.All(c => !c.HasErrors));
					masterShipment.CoLoadShipments.Add(subShipment);
					consol.JK_AgentType = Constants.AgentType.Direct;
					Factory.Save();
					consolsToDetach = new CommonConsol[] { consol };
					message = "Detach consols?";
					helper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message, It.IsAny<CommonConsol>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<CommonShipment>(),
							It.IsAny<IEnumerable<CommonShipment>>()))
						.Returns(consolsToDetach);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					form.InnerGrid.SelectSingleElement(subShipment);
					detachButton.PerformClick();

					Assert("Base detach dialog was shown", UnitTestUserNotification.Instance.PreviousMessages[1].Text.StartsWith(form.Grid.DetachMessage.Caption));
					AssertEquals("Question Detach consols?", UnitTestUserNotification.Instance.LastMessage.ToString());
					Assert("Not detaching consol from subshipment has made consol invalid.", consolsToDetach.Any(c => c.HasErrors));
				}
			}
		}

		public void TestDetachButton_Click_UnsavedSubShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CONSOL12345";
			var masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment1 = masterShipment.CoLoadShipments.AddNew();

			Factory.Save();

			var subShipment2 = masterShipment.CoLoadShipments.AddNew();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);
			string message;
			helper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message, It.IsAny<CommonConsol>(),
					It.IsAny<CommonShipment>(),
					It.IsAny<CommonShipment>(),
					It.Is<IEnumerable<CommonShipment>>(x =>
						x.Cast<ForwardingShipment>().Any() && x.Cast<ForwardingShipment>().All(s => s.IsInDatabase))))
				.Returns(Array.Empty<CommonConsol>());

			ChildEditableService.SetState(masterShipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new MockShipmentForm(masterShipment))
			{
				shipmentForm.Show();
				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					shipmentForm.shipmentGrid.InnerGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					shipmentForm.DetachButton_PerformClick();

					const string expectedLastMessage = @"Are you sure you want to detach the selected records? New records will be deleted.
All unsaved changes in detached items will be canceled.";
					AssertEquals(expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region TotalsPanel

		public void TestTotalsPanel()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S01";
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "PKG";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S02";
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_F3_NKPackType = "PKG";

			Factory.Save();

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();

				AssertEquals("Pre-condition", 2, form.InnerGrid.ListManager.Count);
				Assert(!form.Grid.ShowTotalsPanel);
				Assert("TotalsPanel should not be visible by default", !form.Grid.TotalsPanel.Visible);

				form.Grid.ShowTotalsPanel = true;
				var totalsLabel = form.Grid.Controls.Find("totalsLabel", true).OfType<ZLabel>().First();
				var totalsValuesLabel = form.Grid.Controls.Find("totalsValuesLabel", true).OfType<ZLabel>().First();
				AssertEquals("Pack Line Totals", totalsLabel.Text);
				AssertEquals("No shipments are selected", "Packs: 0 PKG              Weight: 0.00 KG          Volume: 0.000 M3      Chargeable: 0.000 KG", totalsValuesLabel.Text);

				form.PerformRowHeaderClick(0);
				Assert("Shipment 1 is selected", totalsValuesLabel.Text.Contains("Packs: 1 PKG"));

				form.PerformRowHeaderClick(1);
				Assert("Shipment 2 is selected", totalsValuesLabel.Text.Contains("Packs: 2 PKG"));

				form.InnerGrid.Select(0);
				form.InnerGrid.Select(1);
				form.PerformRowHeaderMouseEvent("OnMouseUp", 0);
				Assert("Both shipments are selected", totalsValuesLabel.Text.Contains("Packs: 3 PKG"));
			}
		}

		public void TestTotalsPanel_Attach_Detach()
		{
			var defaultTotalsValuesLabel = "Packs: 0 PKG              Weight: 0.00 KG          Volume: 0.000 M3      Chargeable: 0.000 KG";
			var consol = Factory.New<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S01";
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "PKG";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S02";
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_F3_NKPackType = "PKG";

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			using (var form = new MockConsolForm3(consol))
			{
				form.Show();

				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();
				var attachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();

				AssertEquals("Pre-condition", 2, form.InnerGrid.ListManager.Count);
				Assert(!form.Grid.ShowTotalsPanel);
				Assert("TotalsPanel should not be visible by default", !form.Grid.TotalsPanel.Visible);

				form.Grid.ShowTotalsPanel = true;
				var totalsValuesLabel = form.Grid.Controls.Find("totalsValuesLabel", true).OfType<ZLabel>().First();
				AssertEquals("No shipments are selected", defaultTotalsValuesLabel, totalsValuesLabel.Text);

				form.PerformRowHeaderClick(0);
				Assert("Shipment 1 is selected", totalsValuesLabel.Text.Contains("Packs: 1 PKG"));

				detachButton.PerformClick();
				AssertEquals("Shipment 1 is detached", defaultTotalsValuesLabel, totalsValuesLabel.Text);

				form.PerformRowHeaderClick(1);
				Assert("Shipment 2 is selected", totalsValuesLabel.Text.Contains("Packs: 2 PKG"));

				attachButton.PerformClick();
				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					form.Grid.Attacher.LastShownAttachPopupForTesting.Dispose();
				}
			}
		}

		#endregion

		#region Implementation

		public class MockShipmentForm : ShipmentForm
		{
			public MockShipmentForm(ForwardingShipment businessEntity)
				: base(businessEntity)
			{
				InitializeComponent();
			}

			public void DetachButton_PerformClick()
			{
				ReflectionUtil.InvokeMethod(shipmentGrid, "DetachButton_Click", new object[] { this, EventArgs.Empty });
			}

			public void NewButton_PerformClick()
			{
				ReflectionUtil.InvokeMethod(shipmentGrid, "NewButton_Click", new object[] { this, EventArgs.Empty });
			}

			public ShipmentModuleButtonGrid shipmentGrid;

			new void InitializeComponent()
			{
				base.InitializeComponent();
				shipmentGrid = new ShipmentModuleButtonGrid();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.ColumnName = "JS_UniqueConsignRef";

				shipmentGrid.BindToGridList = "CoLoadShipments";
				shipmentGrid.BindToFindBoxList = "Lookups.CoLoadShipment_List";
				shipmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
				Controls.Add(shipmentGrid);
			}
		}

		class MockConsolForm2 : ZForm
		{
			public MockConsolForm2(ForwardingShipment businessEntity)
				: base(businessEntity)
			{
			}

			public ShipmentModuleButtonGridForTest Grid;

			public ZGridWithoutColumnStylesSerialisation InnerGrid
			{
				get { return Grid.InnerGrid; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid = new ShipmentModuleButtonGridForTest();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.ColumnName = "JS_UniqueConsignRef";

				Grid.BindToGridList = "CoLoadShipments";
				Grid.BindToFindBoxList = "Lookups.CoLoadShipment_List";
				Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
				Controls.Add(Grid);
			}
		}

		class MockConsolForm3 : ZForm
		{
			public MockConsolForm3(ForwardingConsol businessEntity)
				: base(businessEntity)
			{
			}

			public ShipmentModuleButtonGridForTest Grid;

			public ZGridWithoutColumnStylesSerialisation InnerGrid
			{
				get { return Grid.InnerGrid; }
			}

			public void PerformRowHeaderClick(int rowIndex)
			{
				InnerGrid.PerformMouseDownForTest(rowIndex, 1);
				PerformRowHeaderMouseEvent("OnMouseUp", rowIndex);
			}

			public void PerformRowHeaderMouseEvent(string eventName, int rowIndex)
			{
				var rowToClick = InnerGrid.GetRowNotificationRectangle(rowIndex);
				var eventMethodInfo = InnerGrid.GetType().GetMethod(eventName, BindingFlags.NonPublic | BindingFlags.Instance);
				var args = new MouseEventArgs(MouseButtons.Left, 1, rowToClick.X, rowToClick.Y, 0);
				eventMethodInfo.Invoke(InnerGrid, new object[] { args });
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid = new ShipmentModuleButtonGridForTest();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.ColumnName = "JS_UniqueConsignRef";

				Grid.BindToGridList = "Shipments";
				Grid.BindToFindBoxList = "GridShipments";
				Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
				Controls.Add(Grid);
			}
		}

		class ShipmentModuleButtonGridForTest : ShipmentModuleButtonGrid
		{
			public ZToolStripButton EditButton
			{
				get
				{
					var toolStrip = Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
					return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).OfType<ZToolStripButton>().First();
				}
			}

			public ShipmentModuleButtonGridAttacherForTest Attacher { get; set; }

			protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			{
				return Attacher ?? (Attacher = new ShipmentModuleButtonGridAttacherForTest(this.ParentShipment, destinationCollection, findBoxList, moduleID));
			}
		}

		class ShipmentModuleButtonGridAttacherForTest : ShipmentModuleButtonGrid.ShipmentModuleButtonGridAttacher
		{
			public ShipmentModuleButtonGridAttacherForTest(ForwardingShipment parentShipment, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(parentShipment, destinationCollection, findBoxList, moduleID)
			{
			}

			public bool CheckSelected(List<BusinessObject> selected)
			{
				return CheckAttaching(selected);
			}
		}

		class ShipmentModuleButtonGridControl : ZUserControl
		{
			public ShipmentModuleButtonGridControl()
			{
				InitializeComponent();
			}

			void InitializeComponent()
			{
				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo() { ColumnName = "zTextBoxColumnStyleInfo1" };
				this.ShipmentModuleButtonGrid = new ShipmentModuleButtonGridForTest();

				this.BindingSource.DataSourceType = typeof(ForwardingConsol);

				ShipmentModuleButtonGrid.BindToGridList = "CoLoadShipments";
				ShipmentModuleButtonGrid.BindToFindBoxList = "Lookups.CoLoadShipment_List";
				ShipmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				this.Controls.Add(ShipmentModuleButtonGrid);
			}

			internal ShipmentModuleButtonGridForTest ShipmentModuleButtonGrid;
		}

		#endregion
	}
}
