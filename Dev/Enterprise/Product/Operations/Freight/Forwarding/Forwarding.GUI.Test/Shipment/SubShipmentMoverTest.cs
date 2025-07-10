using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class SubShipmentMoverTest : TestCaseWithFactory
	{
		#region Instance and Messages Tests

		[ExpectNoExceptions]
		public void TestCreateInstance()
		{
			using (var form = new Form())
			{
				SubShipmentMover mover = new SubShipmentMover(form);
				mover = new SubShipmentMover(form, null);
				mover = new SubShipmentMover(form, Factory);
			}
		}

		[RequiresSTA]
		public void TestShowTestShowShipmentWithChildEditableServiceStates()
		{
			using (var form = new Form())
			{
				form.Show();
				using (var formCloser = new FormCloserForTest())
				{
					SubShipmentMover mover = new SubShipmentMover(form, Factory);
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
					Factory.Save();

					mover.Execute(shipment, new List<ForwardingShipment>());
					AssertEquals("You cannot use this feature with standard shipments", UnitTestUserNotification.Instance.LastMessage.Text);

					shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
					Factory.Save();

					var sub1 = shipment.CoLoadShipments.AddNew();
					var sub2 = shipment.CoLoadShipments.AddNew();
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					mover.Execute(shipment, new List<ForwardingShipment>() { sub1, sub2 });
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

					AssertEquals("Should set Shipment State", ChildEditableServiceStates.Shipment, ChildEditableService.GetState(mover.Factory));
				}
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestPreValidationMessages()
		{
			using (var form = new Form())
			{
				form.Show();
				using (var formCloser = new FormCloserForTest())
				{
					SubShipmentMover mover = new SubShipmentMover(form, Factory);

					mover.Execute(null, new List<ForwardingShipment>());
					AssertEquals("Could not locate source shipment", UnitTestUserNotification.Instance.LastMessage.Text);

					mover.Execute(Factory.NewWithValidTestData<ForwardingShipment>(), new List<ForwardingShipment>());
					AssertEquals("Please save your changes before you continue", UnitTestUserNotification.Instance.LastMessage.Text);

					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
					Factory.Save();

					mover.Execute(shipment, new List<ForwardingShipment>());
					AssertEquals("You cannot use this feature with standard shipments", UnitTestUserNotification.Instance.LastMessage.Text);

					shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
					Factory.Save();

					mover.Execute(shipment, null);
					AssertEquals("Please select shipments you wish to move", UnitTestUserNotification.Instance.LastMessage.Text);

					mover.Execute(shipment, new List<ForwardingShipment>());
					AssertEquals("Please select shipments you wish to move", UnitTestUserNotification.Instance.LastMessage.Text);

					var sub1 = shipment.CoLoadShipments.AddNew();
					var sub2 = shipment.CoLoadShipments.AddNew();
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					mover.Execute(shipment, new List<ForwardingShipment>() { sub1, sub2 });
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestShowWarningIfMovedShipmentsHaveSubsAttached()
		{
			using (var form = new Form())
			{
				SubShipmentMover mover = new SubShipmentMover(form, Factory);

				var master = Factory.NewWithValidTestData<ForwardingShipment>();
				master.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
				var subLevel1 = master.CoLoadShipments.AddNew();
				subLevel1.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
				var subLevel2 = subLevel1.CoLoadShipments.AddNew();

				Factory.Save();

				mover.Execute(master, new List<ForwardingShipment>() { subLevel1 });
				Assert(!UnitTestUserNotification.Instance.LastMessage.WasNone);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("The following shipments have sub shipments attached"));
			}
		}

		#endregion

		#region Moving Shipments Tests

		public void TestMoveShipments()
		{
			using (var formCloser = new FormCloserForTest())
			{
				var mastershipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				mastershipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
				var subShipment1 = mastershipment1.CoLoadShipments.AddNew();
				var subShipment2 = mastershipment1.CoLoadShipments.AddNew();
				var subShipment2Sub1 = subShipment2.CoLoadShipments.AddNew();

				var mastershipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				mastershipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

				Factory.Save();

				SubShipmentMoverTestHelper helper = new SubShipmentMoverTestHelper();
				helper.CreateAndExecute(Factory, mastershipment1, mastershipment2, new List<ForwardingShipment>() { subShipment1, subShipment2 });

				var newForms = formCloser.GetNewForms();

				Assert("New shipment form should open", newForms.Count == 1 && typeof(ShipmentForm) == newForms[0].GetType());

				var destinationShipment = (newForms[0] as ShipmentForm).DataSource as ForwardingShipment;

				Assert("New shipment form should open with the destination shipment", destinationShipment != null && mastershipment2.PK == destinationShipment.PK);
				AssertEquals("Should have moved 2 subs to the destination shipment", 2, destinationShipment.CoLoadShipments.Count);

				var destinationShipmentSub1 = destinationShipment.Factory.Load<ForwardingShipment>(subShipment1.PK);
				var destinationShipmentSub2 = destinationShipment.Factory.Load<ForwardingShipment>(subShipment2.PK);
				AssertContainsExactElementsInAnyOrder("Should have moved 2 subs to the destination shipment",
					new ForwardingShipment[] { destinationShipmentSub1, destinationShipmentSub2 }, destinationShipment.CoLoadShipments);

				var destinationShipmentSub2Sub1 = destinationShipment.Factory.Load<ForwardingShipment>(subShipment2Sub1.PK);
				AssertContainsExactElementsInAnyOrder("Sub 2 still should have 1 sub attached", destinationShipmentSub2Sub1, destinationShipmentSub2.CoLoadShipments);

				var sourceShipment = destinationShipment.Factory.Load<ForwardingShipment>(mastershipment1.PK);

				AssertNotNull(sourceShipment);
				AssertEquals("Should have removed 2 subs from the source shipment", 0, sourceShipment.CoLoadShipments.Count);
			}
		}

		public void TestAddingMasterShipmentToConsolAddsAllSubs()
		{
			var mastershipment = Factory.NewWithValidTestData<ForwardingShipment>();
			mastershipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

			var subShipmentLevel1 = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipmentLevel1.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			mastershipment.CoLoadShipments.Add(subShipmentLevel1);

			var subShipmentLevel2 = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipmentLevel2.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			subShipmentLevel1.CoLoadShipments.Add(subShipmentLevel2);

			var subShipmentLevel3 = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipmentLevel2.CoLoadShipments.Add(subShipmentLevel3);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.Shipments.Add(mastershipment);
			Factory.Save();

			AssertEquals(4, consol.Shipments.Count);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { mastershipment, subShipmentLevel1, subShipmentLevel2, subShipmentLevel3 },
				consol.Shipments);
			AssertContainsExactElementsInAnyOrder(new ForwardingConsol[] { consol }, mastershipment.Consols);
			AssertContainsExactElementsInAnyOrder(new ForwardingConsol[] { consol }, subShipmentLevel1.Consols);
			AssertContainsExactElementsInAnyOrder(new ForwardingConsol[] { consol }, subShipmentLevel2.Consols);
			AssertContainsExactElementsInAnyOrder(new ForwardingConsol[] { consol }, subShipmentLevel3.Consols);
		}

		public void TestMoveShipmentsOnConsol()
		{
			using (var formCloser = new FormCloserForTest())
			{
				var mAS = FreightTestHelper.GetShipment<ForwardingShipment>("MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
				var sUB_1 = FreightTestHelper.GetShipment("SUB_1", mAS, Constants.ShipmentTypes.StandardHouse, Factory);
				var sUB_2 = FreightTestHelper.GetShipment("SUB_2", mAS, Constants.ShipmentTypes.AssemblyMaster, Factory);
				var sUB_21 = FreightTestHelper.GetShipment("SUB_21", sUB_2, Constants.ShipmentTypes.StandardHouse, Factory);

				var consol1 = FreightTestHelper.GetConsol<ForwardingConsol>("CON_1", Factory);
				consol1.Shipments.Add(mAS);

				var mAS_2 = FreightTestHelper.GetShipment<ForwardingShipment>("", Constants.ShipmentTypes.CoLoadMaster, Factory);

				var consol2 = FreightTestHelper.GetConsol<ForwardingConsol>("CON_2", Factory);
				consol2.Shipments.Add(mAS_2);

				FreightTestHelper.AssertShipmentCollection("Prerequisite", consol1.Shipments, mAS, sUB_1, sUB_2, sUB_21);
				FreightTestHelper.AssertShipmentCollection("Prerequisite", consol2.Shipments, mAS_2);

				Factory.Save();

				SubShipmentMoverTestHelper helper = new SubShipmentMoverTestHelper();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				helper.CreateAndExecute(Factory, mAS, mAS_2, new List<ForwardingShipment>() { sUB_1, sUB_2 });

				var newForms = formCloser.GetNewForms();
				Assert("New shipment form should open", newForms.Count == 1 && typeof(ShipmentForm) == newForms[0].GetType());

				var newMAS_2 = (newForms[0] as ShipmentForm).DataSource as ForwardingShipment;
				Assert("New shipment form should open with the destination shipment", newMAS_2 != null && mAS_2.PK == newMAS_2.PK);

				var newFactory = newMAS_2.Factory;

				var newSUB_1 = newFactory.Load<ForwardingShipment>(sUB_1.PK);
				var newSUB_2 = newFactory.Load<ForwardingShipment>(sUB_2.PK);
				FreightTestHelper.AssertShipmentCollection("Should have moved 2 subs to the destination shipment", newMAS_2.CoLoadShipments, newSUB_1, newSUB_2);

				var newSUB_21 = newFactory.Load<ForwardingShipment>(sUB_21.PK);
				FreightTestHelper.AssertShipmentCollection("Sub 2 still should have 1 sub attached", newSUB_2.CoLoadShipments, newSUB_21);

				var newConsol2 = newFactory.Load<ForwardingConsol>(consol2.PK);
				FreightTestHelper.AssertShipmentCollection("Should have moved all subs to the destination shipment consol", newConsol2.Shipments, newMAS_2, newSUB_1, newSUB_2, newSUB_21);

				var newMAS = newFactory.Load<ForwardingShipment>(mAS.PK);
				FreightTestHelper.AssertShipmentCollection("Should have removed all subs from the source shipment", newMAS.CoLoadShipments);

				var newConsol1 = newFactory.Load<ForwardingConsol>(consol1.PK);
				FreightTestHelper.AssertShipmentCollection("Should have removed all subs from the source shipment consol", newConsol1.Shipments, newMAS);
			}
		}

		public void TestMoveAlreadyMovedShipmentsOnConsol()
		{
			using (var formCloser = new FormCloserForTest())
			{
				var mAS_1 = FreightTestHelper.GetShipment<ForwardingShipment>("MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
				var sUB_1 = FreightTestHelper.GetShipment("SUB_1", mAS_1, Constants.ShipmentTypes.StandardHouse, Factory);
				var consol1 = FreightTestHelper.GetConsol<ForwardingConsol>("CON_1", Factory);
				consol1.Shipments.Add(mAS_1);

				var mAS_2 = FreightTestHelper.GetShipment<ForwardingShipment>("", Constants.ShipmentTypes.CoLoadMaster, Factory);
				var consol2 = FreightTestHelper.GetConsol<ForwardingConsol>("CON_2", Factory);
				consol2.Shipments.Add(mAS_2);

				var mAS_3 = FreightTestHelper.GetShipment<ForwardingShipment>("", Constants.ShipmentTypes.CoLoadMaster, Factory);
				var consol3 = FreightTestHelper.GetConsol<ForwardingConsol>("CON_3", Factory);
				consol3.Shipments.Add(mAS_3);

				FreightTestHelper.AssertShipmentCollection("Prerequisite", consol1.Shipments, mAS_1, sUB_1);
				FreightTestHelper.AssertShipmentCollection("Prerequisite", consol2.Shipments, mAS_2);
				Factory.Save();

				mAS_1.CoLoadShipments.Remove(sUB_1);
				mAS_2.CoLoadShipments.Add(sUB_1);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var helper2 = new SubShipmentMoverTestHelper();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				helper2.CreateAndExecute(Factory, mAS_1, mAS_3, new List<ForwardingShipment>() { sUB_1 });
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("The sub shipment SUB_1 was already moved to another shipment"));
			}
		}

		public void TestIsAllowedToAttachSubShipment()
		{
			var mAS_1 = FreightTestHelper.GetShipment<ForwardingShipment>("MAS_1", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var mAS_2 = FreightTestHelper.GetShipment<ForwardingShipment>("MAS_2", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var sUB = FreightTestHelper.GetShipment("SUB", mAS_1, Constants.ShipmentTypes.StandardHouse, Factory);

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			var message = "message";
			helper.Setup(m =>
					m.IsAllowedToAttachSubShipment(out message, It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>()))
				.Returns(false)
				.Callback(() =>
				{
					message = string.Empty;
					helper.Setup(m =>
							m.IsAllowedToAttachSubShipment(out message, It.IsAny<CommonShipment>(),
								It.IsAny<CommonShipment>()))
						.Returns(true);
				});

			helper.Setup(m => m.GetConsolsToDetachFromSubShipments(
					out message,
					It.IsAny<CommonConsol>(),
					It.IsAny<CommonShipment>(),
					It.IsAny<CommonShipment>(),
					It.IsAny<IEnumerable<CommonShipment>>()))
				.Returns(Array.Empty<CommonConsol>());

			var moverHelper = new SubShipmentMoverTestHelper();

			using (var formCloser = new FormCloserForTest())
			{
				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					moverHelper.CreateAndExecute(Factory, mAS_1, mAS_2, new List<ForwardingShipment>() { sUB });
					Assert("Should have not open new shipment form", formCloser.GetNewForms().Count == 0);
					Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Operation failed due to the following reason(s)"));

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					moverHelper.CreateAndExecute(Factory, mAS_1, mAS_2, new List<ForwardingShipment>() { sUB });
					Assert("Should have opened shipment form", formCloser.GetNewForms()[0] is ShipmentForm);
					Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("You're about to move 1 sub shipment(s)"));
				}
			}
		}

		public void TestDetachConsols()
		{
			var master1 = Factory.New<ForwardingShipment>();
			var master2 = Factory.New<ForwardingShipment>();
			master2.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var subShipment = master1.CoLoadShipments.AddNew();
			var consol = master1.Consols.AddNew();

			Factory.Save();

			var moverHelper = new SubShipmentMoverTestHelper();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var formCloser = new FormCloserForTest())
			{
				var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

				using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					string message;
					var consolsToDetach = Array.Empty<CommonConsol>();

					// Nothing to detach
					message = string.Empty;
					helper.Setup(m => m.IsAllowedToAttachSubShipment(out message, It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>())).Returns(true);
					helper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message, It.IsAny<CommonConsol>(), It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>(), It.IsAny<IEnumerable<CommonShipment>>())).Returns(consolsToDetach);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					moverHelper.CreateAndExecute(Factory, master1, master2, new List<ForwardingShipment>() { subShipment });
					Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("You're about to move 1 sub shipment(s)"));

					// Detach consols? NO
					helper.Setup(m => m.IsAllowedToAttachSubShipment(out message, It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>())).Returns(true);

					consolsToDetach = new CommonConsol[] { consol };
					message = "Detach consols?";
					helper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message, It.IsAny<CommonConsol>(), It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>(), It.IsAny<IEnumerable<CommonShipment>>())).Returns(consolsToDetach);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					moverHelper.CreateAndExecute(Factory, master1, master2, new List<ForwardingShipment>() { subShipment });
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages[1].Text.StartsWith("You're about to move 1 sub shipment(s)"));
					AssertEquals("Question Detach consols?", UnitTestUserNotification.Instance.LastMessage.ToString());

					// Detach consols? YES -> Not Allowed
					message = string.Empty;
					helper.Setup(m => m.IsAllowedToAttachSubShipment(out message, It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>())).Returns(true);

					consolsToDetach = new CommonConsol[] { consol };
					message = "Detach consols?";
					helper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message, It.IsAny<CommonConsol>(), It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>(), It.IsAny<IEnumerable<CommonShipment>>())).Returns(consolsToDetach);

					var detachRequest = new ShipmentConsolDetachRequest("NOT ALLOWED!", null, null, null);
					helper.Setup(m => m.IsAllowedToDetachConsols(It.IsAny<CommonShipment>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(detachRequest);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					moverHelper.CreateAndExecute(Factory, master1, master2, new List<ForwardingShipment>() { subShipment });
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages[2].Text.StartsWith("You're about to move 1 sub shipment(s)"));
					AssertEquals("Question Detach consols?", UnitTestUserNotification.Instance.PreviousMessages[1].ToString());
					AssertEquals("Information NOT ALLOWED!", UnitTestUserNotification.Instance.LastMessage.ToString().TrimEnd());

					// Detach consols? YES -> Allowed
					message = string.Empty;
					helper.Setup(m => m.IsAllowedToAttachSubShipment(out message, It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>())).Returns(true);

					consolsToDetach = new CommonConsol[] { consol };
					message = "Detach consols?";
					helper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message, It.IsAny<CommonConsol>(), It.IsAny<CommonShipment>(), It.IsAny<CommonShipment>(), It.IsAny<IEnumerable<CommonShipment>>())).Returns(consolsToDetach);

					detachRequest = new ShipmentConsolDetachRequest("", null, null, null);
					helper.Setup(m => m.IsAllowedToDetachConsols(It.IsAny<CommonShipment>(), It.IsAny<IEnumerable<CommonConsol>>())).Returns(detachRequest);

					helper.Setup(m => m.DetachShipmentsFromConsols(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>()));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					moverHelper.CreateAndExecute(Factory, master1, master2, new List<ForwardingShipment>() { subShipment });
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages[1].Text.StartsWith("You're about to move 1 sub shipment(s)"));
					AssertEquals("Question Detach consols?", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
		}

		public void TestDestinationShipmentHasChanges()
		{
			using (var formCloser = new FormCloserForTest())
			{
				var mastershipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				mastershipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
				var subShipment1 = mastershipment1.CoLoadShipments.AddNew();

				var mastershipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				mastershipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

				Factory.Save();

				SubShipmentMoverTestHelper helper = new SubShipmentMoverTestHelper();
				helper.CreateAndExecute(Factory, mastershipment1, mastershipment2, new List<ForwardingShipment>() { subShipment1 });

				var destinationShipment = (formCloser.GetNewForms()[0] as ShipmentForm).DataSource as ForwardingShipment;
				AssertEquals(true, destinationShipment.HasChanges);
			}
		}

		[RequiresSTA]
		public void TestDestinationShipmentCanBeOfAnyTransportMode()
		{
			using (var form = new Form())
			{
				form.Show();
				using (var formCloser = new FormCloserForTest())
				{
					var mastershipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
					mastershipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
					mastershipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

					var subShipment1 = mastershipment1.CoLoadShipments.AddNew();
					mastershipment1.JS_TransportMode = Core.Constants.TransportModes.Air;

					var mastershipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
					mastershipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
					mastershipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

					var mastershipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
					mastershipment3.JS_TransportMode = Core.Constants.TransportModes.Sea;
					mastershipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

					var mastershipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
					mastershipment4.JS_TransportMode = Core.Constants.TransportModes.Rail;
					mastershipment4.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

					var highVolumeLowValueShipment = Factory.NewWithValidTestData<ForwardingShipment>();
					highVolumeLowValueShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
					highVolumeLowValueShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

					var highVolumeLowValueLegacyShipment = Factory.NewWithValidTestData<ForwardingShipment>();
					highVolumeLowValueLegacyShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
					highVolumeLowValueLegacyShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

					Factory.Save();

					var mover = new SubShipmentMover(form, Factory);
					mover.Execute(mastershipment1, new List<ForwardingShipment>() { subShipment1 });

					var chooseShipmentPopup = formCloser.GetNewForms()[0] as BizObjectPopupFinder.ModulePopup;
					AssertNotNull("choose shipment popup not found", chooseShipmentPopup);

					var chooseShipmentFindBox = ReflectionUtil.GetPropertyValue(chooseShipmentPopup, "FindBox") as IFindBox;
					AssertNotNull("choose shipment find box not found (see EmbeddedModulePopup)", chooseShipmentFindBox);

					var shipmentsToChooseFrom = chooseShipmentFindBox.ListProvider as ModuleShipmentCollection;
					AssertNotNull("expected ListProvider to be of type ModuleShipmentCollection", chooseShipmentFindBox);

					shipmentsToChooseFrom.Load();

					AssertContainsExactElementsInAnyOrder("Should exclude STD, HLV and HLS shipments",
						new[] { mastershipment2, mastershipment3, mastershipment4 },
						shipmentsToChooseFrom);
				}
			}
		}

		[RequiresSTA]
		public void TestDestinationShipmentCannotBeOneOfShipmentsToMove()
		{
			using (var form = new Form())
			{
				form.Show();
				using (var formCloser = new FormCloserForTest())
				{
					var asmMasterShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
					asmMasterShipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
					asmMasterShipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

					var cldMasterShipment1 = asmMasterShipment1.CoLoadShipments.AddNew();
					cldMasterShipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
					cldMasterShipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

					var cldMastershipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
					cldMastershipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
					cldMastershipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

					Factory.Save();

					var mover = new SubShipmentMover(form, Factory);
					mover.Execute(asmMasterShipment1, new List<ForwardingShipment>() { cldMasterShipment1 });

					var chooseShipmentPopup = formCloser.GetNewForms()[0] as BizObjectPopupFinder.ModulePopup;
					var chooseShipmentFindBox = ReflectionUtil.GetPropertyValue(chooseShipmentPopup, "FindBox") as IFindBox;
					var shipmentsToChooseFrom = chooseShipmentFindBox.ListProvider as ModuleShipmentCollection;
					shipmentsToChooseFrom.Load();

					AssertContainsExactElementsInAnyOrder("Should exclude cldMasterShipment1 as this is the shipment being moved",
						new[] { cldMastershipment2 },
						shipmentsToChooseFrom);
				}
			}
		}

		public void TestCircularReferencesAreNotAllowed()
		{
			using (var formCloser = new FormCloserForTest())
			{
				var ship1 = Factory.NewWithValidTestData<ForwardingShipment>();
				ship1.JS_TransportMode = Core.Constants.TransportModes.Air;
				ship1.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
				ship1.JS_UniqueConsignRef = "SHIP1";

				var ship2 = ship1.CoLoadShipments.AddNew();
				ship2.JS_TransportMode = Core.Constants.TransportModes.Air;
				ship2.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
				ship2.JS_UniqueConsignRef = "SHIP2";

				var ship3 = ship2.CoLoadShipments.AddNew();
				ship3.JS_TransportMode = Core.Constants.TransportModes.Air;
				ship3.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
				ship3.JS_UniqueConsignRef = "SHIP3";

				var ship4 = ship3.CoLoadShipments.AddNew();
				ship4.JS_TransportMode = Core.Constants.TransportModes.Air;
				ship4.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
				ship4.JS_UniqueConsignRef = "SHIP4";

				Factory.Save();

				var helper = new SubShipmentMoverTestHelper();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				helper.CreateAndExecute(Factory, ship1, ship4, new List<ForwardingShipment>() { ship2 });

				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("The sub shipment SHIP2 cannot be moved to SHIP4 as it would create a circular reference."));
			}
		}

		#endregion

		#region Implementation

		class FormCloserForTest : IDisposable
		{
#if WINZOR
			readonly List<Guid> listAlreadyOpenedForms = new List<Guid>();

#else
			readonly List<IntPtr> listAlreadyOpenedForms = new List<IntPtr>();
#endif

			public FormCloserForTest()
			{
				GetOpenedForms();
			}

			void GetOpenedForms()
			{
				foreach (Form form in Application.OpenForms)
				{
#if WINZOR
					listAlreadyOpenedForms.Add(form.WinzorControlGuid);
#else
					listAlreadyOpenedForms.Add(form.Handle);

#endif
				}
			}

			void CloseForms()
			{
				foreach (var form in GetNewForms())
				{
					form.Close();
				}
			}

			public List<Form> GetNewForms()
			{
				List<Form> listNewForms = new List<Form>();
				foreach (Form form in Application.OpenForms)
				{
#if WINZOR

					if (!listAlreadyOpenedForms.Contains(form.WinzorControlGuid))
#else
					if (!listAlreadyOpenedForms.Contains(form.Handle))

#endif
					{
						listNewForms.Add(form);
					}
				}
				return listNewForms;
			}

			#region IDisposable Members

			public void Dispose()
			{
				CloseForms();
			}

			#endregion
		}

		class SubShipmentMoverTestHelper
		{
			SubShipmentMover Mover { get; set; }

			public void CreateAndExecute(BusinessObjectFactory factory, ForwardingShipment source, ForwardingShipment destination, List<ForwardingShipment> shipmentsToMove)
			{
				using (var form = new Form())
				{
					Mover = new SubShipmentMover(form, factory);

					SetProperty("SourceShipment", source);
					SetProperty("DestinationShipment", destination);
					SetProperty("ShipmentsToMove", shipmentsToMove);
					Invoke("MoveShipments", null);
				}
			}

			void SetProperty(string name, object value)
			{
				var pi = typeof(SubShipmentMover).GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic);
				pi.SetValue(Mover, value, null);
			}

			void Invoke(string methodName, object[] parameters)
			{
				var mi = typeof(SubShipmentMover).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
				mi.Invoke(Mover, parameters);
			}
		}

#endregion
	}
}
