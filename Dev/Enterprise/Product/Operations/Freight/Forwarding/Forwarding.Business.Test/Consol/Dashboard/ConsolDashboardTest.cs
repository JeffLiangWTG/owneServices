using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ConsolDashboard))]
	sealed class ConsolDashboardTest : NonPersistentBusinessObjectTestCase
	{
		#region Ctor

		public void TestCtor_EnsureShipmentsAndConsolsShouldBeReadonly()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var consol1 = Factory.New<ForwardingConsol>();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(shipment1);
			AssertEquals("Shipment collection should not be readonly - to allow it to be used in module grid", false, dashboard.Shipments.ReadOnly);
			AssertEquals("Shipments in shipment collection should be forced to be readonly", true, shipment1.ReadOnly);

			dashboard.Consols.Add(consol1);
			AssertEquals("Consol collection should not be readonly - to allow it to be used in module grid", false, dashboard.Consols.ReadOnly);
			AssertEquals("Consols in Consol collection should be forced to be readonly", true, consol1.ReadOnly);
		}

		#endregion

		#region Add/Remove

		public void TestAddShipments_RefreshConcurrentChanges()
		{
			var shipment1 = CreateShipment("s1");
			shipment1.JS_GoodsDescription = "Value to override";

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(shipment1);
			dashboard.Shipments.Remove(shipment1);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentReloadedByAnotherFactory = anotherFactory.Load<ForwardingShipment>(shipment1.PK);
			shipmentReloadedByAnotherFactory.JS_GoodsDescription = "This should have changed";

			anotherFactory.Save();
			AssertEquals(0, dashboard.Shipments.Count);

			dashboard.Shipments.Add(shipment1);

			var shipmentsOnDashboard = dashboard.Shipments.Cast<ForwardingShipment>();
			CombineAssertions("The added shipment should have the updated value from the other instance", delegate
			{
				AssertEquals(false, dashboard.HasUnsavedChanges);
				AssertEquals(1, shipmentsOnDashboard.Count());
				AssertEquals("This should have changed", shipmentsOnDashboard.First().JS_GoodsDescription);
			});
		}

		public void TestAddConsols()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();
			var consol4 = Factory.New<ForwardingConsol>();

			var dashboard = new ConsolDashboard(Factory);
			AssertEquals("Prerequisite: dashboard has no consols", false, dashboard.Consols.Any());

			dashboard.AddConsols(new[] { consol1, consol2, null });
			AssertContainsExactElementsInAnyOrder(
				new[] { consol1, consol2 }, dashboard.Consols.Cast<ForwardingConsol>());

			dashboard.AddConsols(new[] { consol1, null, consol2, consol3 });
			AssertContainsExactElementsInAnyOrder(
				new[] { consol1, consol2, consol3 }, dashboard.Consols.Cast<ForwardingConsol>());
		}

		public void TestAddConsols_RefreshConcurrentChanges()
		{
			var consol1 = CreateConsol("Consol1");
			consol1.JK_BookingReference = "Value to override";
			consol1.RunPreSaveValidation();
			AssertEquals("Prerequisite: consol should have no errors", false, consol1.HasErrors);

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol1 });
			dashboard.RemoveConsols(new[] { consol1 });

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolReloadedInAnotherFactory = anotherFactory.Load<ForwardingConsol>(consol1.PK);
			consolReloadedInAnotherFactory.JK_BookingReference = "This should have changed";

			anotherFactory.Save();
			AssertEquals(0, dashboard.Consols.Count);

			dashboard.AddConsols(new[] { consol1 });

			CombineAssertions("The added consol should have the updated value from the other instance", delegate
			{
				AssertEquals(false, dashboard.HasUnsavedChanges);
				AssertEquals(1, dashboard.Consols.Count);
				AssertEquals("This should have changed", dashboard.Consols.Cast<ForwardingConsol>().First().JK_BookingReference);
			});
		}

		public void TestRemoveConsols()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();
			var consol4 = Factory.New<ForwardingConsol>();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol1, consol2, consol3 });

			dashboard.RemoveConsols(new[] { consol2, consol3, null, consol4 });

			AssertContainsExactElementsInAnyOrder(
				new[] { consol1 }, dashboard.Consols.Cast<ForwardingConsol>());
		}

		#endregion

		#region Create Consol

		public void TestCreateConsol()
		{
			Func<ForwardingShipment> createShipment = () =>
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";

				return shipment;
			};

			var shipment1 = createShipment();
			var shipment2 = createShipment();

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.AddRange(new[] { shipment1, shipment2 });

			var creationFactory = new BusinessObjectFactory();
			var consol = dashboard.CreateConsol(creationFactory, new[] { shipment1.PK, shipment2.PK });

			AssertEquals("Factory from method parameter used to create consol", creationFactory, consol.Factory);
			AssertEquals("Created consol should not be saved yet", false, consol.IsInDatabase);

			AssertContainsExactElementsInAnyOrder("Shipments have been reloaded in consol factory and attached to the consol",
				new[] { shipment1.PK, shipment2.PK },
				consol.Shipments.Select(shipment => shipment.PK));

			CombineAssertions("Essential properties populated from first shipment", () =>
			{
				AssertEquals(Core.Constants.TransportModes.Air, consol.JK_TransportMode);
				AssertEquals(Core.Constants.ContainerModes.ULD, consol.JK_ConsolMode);
				AssertEquals("AUSYD", consol.JK_RL_NKLoadPort);
				AssertEquals("NZAKL", consol.JK_RL_NKDischargePort);
			});
		}

		#endregion

		#region Attach

		public void TestTryAttach_CheckConsolAndShipmentsAreOnDashboard()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "Shipment2";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "Shipment3";

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "Consol2";

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(shipment1);
			dashboard.AddConsols(new[] { consol1 });

			var result = dashboard.TryAttach(consol2, new[] { shipment1, shipment2, shipment3 });
			AssertEquals(true, result.HasErrors);

			string expectedError = @"Not found on dashboard:
Consol Consol2
Shipment Shipment2
Shipment Shipment3";

			AssertEquals(expectedError, result.Errors);
			AssertEquals(false, result.AcceptedShipments.Any());
		}

		public void TestTryAttach_RefreshConcurrentChanges()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.JS_GoodsDescription = "Value to override";

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(shipment1);
			dashboard.AddConsols(new[] { consol1 });

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentReloadedInOtherFactory = anotherFactory.Load<ForwardingShipment>(shipment1.PK);
			shipmentReloadedInOtherFactory.JS_GoodsDescription = "This should have changed";

			anotherFactory.Save();

			AssertEquals(0, consol1.Shipments.Count);

			var result = dashboard.TryAttach(consol1, new[] { shipment1 });

			AssertEquals(true, result.AcceptedShipments.Any());
			CombineAssertions("The consol should now have a shipment with the updated value from the other instance", delegate
			{
				AssertEquals(false, result.HasErrors);
				AssertEquals(1, result.AcceptedShipments.Count());
				AssertEquals(1, consol1.Shipments.Count);
				AssertEquals("This should have changed", consol1.Shipments.Cast<ForwardingShipment>().First().JS_GoodsDescription);
			});
		}

		public void TestTryAttach_StandardAttachChecksAreExecuted()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "Shipment2";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "Shipment3";

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.AddRange(new[] { shipment1, shipment2, shipment3 });
			dashboard.AddConsols(new[] { consol1 });

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);
			using (DashboardShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				var shipmentsToAttach = new[] { shipment1, shipment2 };
				helper.Setup(m => m.IsAllowedToAttachShipment(consol1, shipment1))
					.Returns(new ShipmentConsolAttachRequest(() => "Error1", () => "Warning1"));

				helper.Setup(m => m.IsAllowedToAttachShipment(consol1, shipment2))
					.Returns(new ShipmentConsolAttachRequest(() => "Error2", null));

				helper.Setup(m =>
						m.CheckDatesWithinRange(It.IsAny<IEnumerable<CommonShipment>>(),
							It.IsAny<IEnumerable<CommonConsol>>()))
					.Returns(new Collection<string>());

				helper.Setup(m => m.CheckShipmentAndConsolComplianceRisk(It.IsAny<IEnumerable<CommonShipment>>(), It.IsAny<IEnumerable<CommonConsol>>()))
					.Returns(string.Empty);

				var result = dashboard.TryAttach(consol1, shipmentsToAttach);
				AssertEquals(string.Join(System.Environment.NewLine, "Error1", "Error2"), result.Errors);
				AssertEquals("", result.Warnings);

				AssertContainsExactElementsInAnyOrder(
					Array.Empty<ForwardingShipment>(), result.AcceptedShipments);

				AssertContainsExactElementsInAnyOrder(
					new[] { shipment1, shipment2 }, result.RejectedShipments);

				AssertContainsExactElementsInAnyOrder("No shipments have been attached to consol",
					Array.Empty<ForwardingShipment>(), consol1.Shipments.Cast<ForwardingShipment>());

				AssertContainsExactElementsInAnyOrder("No shipments have been removed from available shipment list",
					new[] { shipment1, shipment2, shipment3 }, dashboard.Shipments.Cast<ForwardingShipment>());

				helper.Setup(m => m.IsAllowedToAttachShipment(consol1, shipment1))
					.Returns(new ShipmentConsolAttachRequest(null, () => "Warning1"));

				// The below extra checks would be run when shipment is attached to the consol

				helper.Setup(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(),
						It.IsAny<IEnumerable<CommonConsol>>()))
					.Returns(string.Empty);

				helper.Setup(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(),
						It.IsAny<IEnumerable<CommonConsol>>()))
					.Returns(string.Empty);

				helper.Setup(m => m.IsAllowedToAttachShipment(consol1, shipment2))
					.Returns(new ShipmentConsolAttachRequest(() => "Error2", null));

				helper.Setup(m => m.IsAllowedToAttachShipment(consol1, shipment3))
					.Returns(new ShipmentConsolAttachRequest(null, null));

				helper.Setup(m =>
						m.CheckDatesWithinRange(It.IsAny<IEnumerable<CommonShipment>>(),
							It.IsAny<IEnumerable<CommonConsol>>()))
					.Returns(new Collection<string>());

				helper.Setup(m => m.CheckShipmentAndConsolComplianceRisk(It.IsAny<IEnumerable<CommonShipment>>(),
						It.IsAny<IEnumerable<CommonConsol>>()))
					.Returns(string.Empty);

				result = dashboard.TryAttach(consol1, new[] { shipment1, shipment2, shipment3 });

				AssertEquals(string.Join(System.Environment.NewLine, "Error2"), result.Errors);
				AssertEquals("Warning1", result.Warnings);

				AssertContainsExactElementsInAnyOrder(
					new[] { shipment1, shipment3 }, result.AcceptedShipments);

				AssertContainsExactElementsInAnyOrder(
					new[] { shipment2 }, result.RejectedShipments);

				AssertContainsExactElementsInAnyOrder("Accepted shipments have been attached to consol",
					new[] { shipment1, shipment3 }, consol1.Shipments.Cast<ForwardingShipment>());

				AssertContainsExactElementsInAnyOrder("Accepted shipments have been removed from available shipment list",
					new[] { shipment2 }, dashboard.Shipments.Cast<ForwardingShipment>());
			}
		}

		public void TestTryAttach_UsesBusyIndicatorIfAvailable()
		{
			AssertBusyIndicatorWasUsed(() =>
			{
				var shipment = Factory.New<ForwardingShipment>();
				var consol = Factory.New<ForwardingConsol>();

				var dashboard = new ConsolDashboard(Factory);
				dashboard.Shipments.Add(shipment);
				dashboard.AddConsols(new[] { consol });

				var result = dashboard.TryAttach(consol, new[] { shipment });
			});
		}

		public void TestTryAttach_WrongDatesCancel()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = Factory.New<ForwardingConsol>();

			var today = ZDateTime.Today;
			consol.Transports.MostInterestingTransport.JW_ETA = today.AddDays(10);
			shipment.JS_E_ARV = today;

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(shipment);
			dashboard.AddConsols(new[] { consol });

			dashboard.DatesOutsideRange += (messages) => ZDialogResult.Cancel;
			var result = dashboard.TryAttach(consol, new[] { shipment });
			AssertEquals(0, result.AcceptedShipments.Count());
			AssertEquals(1, result.RejectedShipments.Count());
		}

		public void TestTryAttach_WrongDatesNo()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = Factory.New<ForwardingConsol>();

			var today = ZDateTime.Today;
			consol.Transports.MostInterestingTransport.JW_ETA = today.AddDays(10);
			shipment.JS_E_ARV = today;

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(shipment);
			dashboard.AddConsols(new[] { consol });

			dashboard.DatesOutsideRange += (messages) => ZDialogResult.No;
			var result = dashboard.TryAttach(consol, new[] { shipment });
			AssertEquals(1, result.AcceptedShipments.Count());
			AssertEquals(0, result.RejectedShipments.Count());
			AssertEquals(today, shipment.JS_E_ARV);
		}

		public void TestTryAttach_WrongDatesYes()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = Factory.New<ForwardingConsol>();

			var today = ZDateTime.Today;
			consol.Transports.MostInterestingTransport.JW_ETA = today.AddDays(10);
			shipment.JS_E_ARV = today;

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(shipment);
			dashboard.AddConsols(new[] { consol });

			dashboard.DatesOutsideRange += (messages) => ZDialogResult.Yes;
			var result = dashboard.TryAttach(consol, new[] { shipment });
			AssertEquals(1, result.AcceptedShipments.Count());
			AssertEquals(0, result.RejectedShipments.Count());
			AssertEquals(today.AddDays(10), shipment.JS_E_ARV);
		}

		public void TestTryAttach_SubShipmentIsSkippedWhenAttachingAlongWithItsMaster()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			var subShipment = masterShipment.CoLoadShipments.AddNew();
			var consol = Factory.New<ForwardingConsol>();

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(masterShipment);
			dashboard.Shipments.Add(subShipment);
			dashboard.AddConsols(new[] { consol });

			var result = dashboard.TryAttach(consol, new[] { subShipment, masterShipment });
			AssertEquals(1, result.AcceptedShipments.Count());
			AssertEquals(1, result.RejectedShipments.Count());
			AssertContainsExactElementsInAnyOrder("Both master shipment and sub-shipment have been attached to consol",
				new[] { masterShipment, subShipment }, consol.Shipments.Cast<ForwardingShipment>());
		}

		#endregion

		#region Detach

		public void TestTryDetach_CheckConsolAndShipmentsAreOnDashboard()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "Shipment2";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "Shipment3";

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";

			consol1.Shipments.Add(shipment1);

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(shipment1);
			dashboard.AddConsols(new[] { consol1 });

			var result = dashboard.TryDetach(consol1, new[] { shipment1, shipment2, shipment3 });
			AssertEquals(true, result.HasErrors);

			string expectedError = @"Not found on dashboard:
Shipment Shipment2
Shipment Shipment3";

			AssertEquals(expectedError, result.Errors);
			AssertEquals(false, result.AcceptedShipments.Any());
		}

		public void TestTryDetach_RefreshConcurrentChanges()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "Shipment1";
			shipment1.JS_GoodsDescription = "Value to override";

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";

			consol1.Shipments.Add(shipment1);

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(shipment1);
			dashboard.AddConsols(new[] { consol1 });

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentReloadedInOtherFactory = anotherFactory.Load<ForwardingShipment>(shipment1.PK);
			shipmentReloadedInOtherFactory.JS_GoodsDescription = "This should have changed";

			anotherFactory.Save();

			AssertEquals(1, consol1.Shipments.Count);

			var result = dashboard.TryDetach(consol1, new[] { shipment1 });

			AssertEquals(true, result.AcceptedShipments.Any());
			CombineAssertions("The shipment should no longer be attached to the consol, and should have the updated value on the dashboard", delegate
			{
				AssertEquals(false, result.HasErrors);
				AssertEquals(1, result.AcceptedShipments.Count());
				AssertEquals(0, consol1.Shipments.Count);
				AssertEquals("This should have changed", dashboard.Shipments.Cast<ForwardingShipment>().First().JS_GoodsDescription);
			});
		}

		public void TestTryDetach_StandardDetachChecksAreExecuted()
		{
			var creationFactory = new BusinessObjectFactory();

			var shipment11 = creationFactory.NewWithValidTestData<ForwardingShipment>();
			var shipment12 = creationFactory.NewWithValidTestData<ForwardingShipment>();
			var shipment13 = creationFactory.NewWithValidTestData<ForwardingShipment>();

			var consol1 = creationFactory.NewWithValidTestData<ForwardingConsol>();
			consol1.Shipments.AddRange(shipment11, shipment12);

			creationFactory.Save();

			var standaloneShipment = Factory.NewWithValidTestData<ForwardingShipment>();

			consol1 = Factory.Load<ForwardingConsol>(consol1.PK);

			shipment11 = Factory.Load<ForwardingShipment>(shipment11.PK);
			shipment12 = Factory.Load<ForwardingShipment>(shipment12.PK);
			shipment13 = Factory.Load<ForwardingShipment>(shipment13.PK);

			consol1.Shipments.Add(shipment13);

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(standaloneShipment);
			dashboard.AddConsols(new[] { consol1 });

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);
			using (DashboardShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				helper.Setup(m => m.IsAllowedToDetachShipments(consol1, new[] { shipment11, shipment12 }))
					.Returns(new ShipmentConsolDetachRequest("Error1\r\nError2", null, null, null));

				var result = dashboard.TryDetach(consol1, new[] { shipment11, shipment12, shipment13 });
				AssertEquals(string.Join(System.Environment.NewLine, "Error1", "Error2"), result.Errors);
				AssertEquals("", result.Warnings);

				AssertContainsExactElementsInAnyOrder(
					new[] { shipment13 }, result.AcceptedShipments);

				AssertContainsExactElementsInAnyOrder(
					new[] { shipment11, shipment12 }, result.RejectedShipments);

				AssertContainsExactElementsInAnyOrder("One shipment have been detached",
					new[] { shipment11, shipment12 }, consol1.Shipments.Cast<ForwardingShipment>());

				AssertContainsExactElementsInAnyOrder("Detached shipment have been added to available shipment list",
					new[] { standaloneShipment, shipment13 }, dashboard.Shipments.Cast<ForwardingShipment>());

				helper.Setup(m => m.IsAllowedToDetachShipments(consol1, new[] { shipment11, shipment12 }))
					.Returns(new ShipmentConsolDetachRequest("", null, null, null));

				result = dashboard.TryDetach(consol1, new[] { shipment11, shipment12 });
				AssertEquals("", result.Errors);

				AssertContainsExactElementsInAnyOrder(
					new[] { shipment11, shipment12 }, result.AcceptedShipments);

				AssertContainsExactElementsInAnyOrder(
					Array.Empty<ForwardingShipment>(), result.RejectedShipments);

				AssertContainsExactElementsInAnyOrder("Shipments have been detached from consol",
					Array.Empty<ForwardingShipment>(), consol1.Shipments.Cast<ForwardingShipment>());

				AssertContainsExactElementsInAnyOrder("Shipments have been added to available shipment list",
					new[] { shipment11, shipment12, shipment13, standaloneShipment }, dashboard.Shipments.Cast<ForwardingShipment>());
			}
		}

		public void TestTryDetach_UsesBusyIndicatorIfAvailable()
		{
			AssertBusyIndicatorWasUsed(() =>
			{
				var shipment = Factory.New<ForwardingShipment>();
				var consol = Factory.New<ForwardingConsol>();

				consol.Shipments.Add(shipment);

				var dashboard = new ConsolDashboard(Factory);
				dashboard.AddConsols(new[] { consol });

				var result = dashboard.TryDetach(consol, new[] { shipment });
			});
		}

		public void TestTryDetach_ExcludeSubShipmentsFromBeingAddedToShipmentsGrid()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "Master/Lead";
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "Sub-shipment";

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";

			consol1.Shipments.Add(shipment1);
			consol1.Shipments.Add(shipment2);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol1 });

			AssertEquals(2, consol1.Shipments.Count);

			var result = dashboard.TryDetach(consol1, new[] { shipment1, shipment2 }, new[] { shipment2 });

			AssertEquals(true, result.AcceptedShipments.Any());
			CombineAssertions("Both Master/Lead shipment and Sub-shipment should no longer be attached to the consol, and sub-shipment should not be added to available shipment list", delegate
			{
				AssertEquals(false, result.HasErrors);
				AssertEquals(2, result.AcceptedShipments.Count());
				AssertEquals(0, consol1.Shipments.Count);
				AssertContainsExactElementsInAnyOrder("Only master/lead shipment has been added to available shipment list",
					new[] { shipment1 }, dashboard.Shipments.Cast<ForwardingShipment>());
			});
		}

		#endregion

		#region Save

		public void TestRecreateShipmentConfiguration()
		{
			var shipment11 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment12 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment13 = Factory.NewWithValidTestData<ForwardingShipment>();

			var standaloneShipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddRange(shipment11, shipment12, shipment13);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.RecreateShipmentConfiguration(consol, new[] { shipment11.PK, standaloneShipment.PK });

			AssertContainsExactElementsInAnyOrder(
				new[] { shipment11.PK, standaloneShipment.PK },
				consol.Shipments.Select(shipment => shipment.PK));

			dashboard.RecreateShipmentConfiguration(consol, new[] { shipment12.PK });

			AssertContainsExactElementsInAnyOrder(
				new[] { shipment12.PK },
				consol.Shipments.Select(shipment => shipment.PK));

			dashboard.RecreateShipmentConfiguration(consol, Array.Empty<ZGuid>());

			AssertContainsExactElementsInAnyOrder(
				Array.Empty<ZGuid>(),
				consol.Shipments.Select(shipment => shipment.PK));
		}

		public void TestSave_ConsolsWithConcurrentChangesByAnotherUser_NotSavedWhenHasConflicts()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");
			var shipment3 = CreateShipment("s3");
			var shipment4 = CreateShipment("s4");
			var shipment5 = CreateShipment("s5");
			var shipment6 = CreateShipment("s6");

			var consol1 = CreateConsol("Consol1");
			consol1.Shipments.AddRange(shipment1, shipment2, shipment5);
			consol1.RunPreSaveValidation();
			AssertEquals("Prerequisite: consol should have no errors", false, consol1.HasErrors);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol1 });
			dashboard.Shipments.AddRange(shipment3, shipment6);

			dashboard.TryAttach(consol1, new[] { shipment3, shipment6 });
			dashboard.TryDetach(consol1, new[] { shipment1, shipment5 });

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var consolReloadedInAnotherFactory = anotherFactory.Load<ForwardingConsol>(consol1.PK);
			consolReloadedInAnotherFactory.Shipments.Remove(shipment2.PK);
			consolReloadedInAnotherFactory.Shipments.Remove(shipment5.PK);
			consolReloadedInAnotherFactory.Shipments.AddRange(shipment4, shipment6);

			anotherFactory.Save();

			var result = dashboard.Save();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Saved consols",
					Array.Empty<ZGuid>(), result.Saved.Select(snapshot => snapshot.ConsolPK));

				AssertEquals("Has one unsaved consol", 1, result.Unsaved.Count());
				var unsavedConsolSnapshot = result.ModifiedByAnotherUser.FirstOrDefault();
				AssertEquals("Not saved as consol have been modified by another user", consol1.PK, unsavedConsolSnapshot.ConsolPK);

				AssertEquals("Has four conflict errors", 4, unsavedConsolSnapshot.Notifications.GetErrors().Count());
				AssertContainsExactElementsInAnyOrder(
					"Contains conflict details",
					new[]
					{
						"Shipment s3 - Attached by current user",
						"Shipment s1 - Detached by current user",
						"Shipment s4 - Attached by another user",
						"Shipment s2 - Detached by another user"
					},
					unsavedConsolSnapshot.Notifications.GetErrors().Select(notification => notification.Message));

				AssertContainsExactElementsInAnyOrder(
					"Snapshot contains all conflict shipments",
					new[]
					{
						shipment1.PK,
						shipment2.PK,
						shipment3.PK,
						shipment4.PK,
						shipment6.PK
					},
					unsavedConsolSnapshot.ShipmentPKs);
			});

			AssertEquals("Dashboard has unsaved changes", true, dashboard.HasUnsavedChanges);
		}

		public void TestSave_ConsolsWithConcurrentChangesByAnotherUser_SavedWhenHasNoConflicts()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");

			var consol1 = CreateConsol("Consol1");
			consol1.Shipments.Add(shipment1);
			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol1 });
			dashboard.Shipments.Add(shipment2);

			dashboard.TryAttach(consol1, new[] { shipment2 });
			dashboard.TryDetach(consol1, new[] { shipment1 });

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var consolReloadedInAnotherFactory = anotherFactory.Load<ForwardingConsol>(consol1.PK);
			consolReloadedInAnotherFactory.Shipments.Remove(shipment1.PK);
			consolReloadedInAnotherFactory.Shipments.Add(shipment2);

			anotherFactory.Save();

			var result = dashboard.Save();
			AssertContainsExactElementsInAnyOrder("Saved consols", new[] { consol1.PK }, result.Saved.Select(snapshot => snapshot.ConsolPK));
			AssertEquals("Dashboard has unsaved changes", false, dashboard.HasUnsavedChanges);
		}

		[ExpectNoExceptions]
		public void TestSave_NoExceptionWhenIsGrossWeightOverriddenCannotBeUpdated()
		{
			var shipment = CreateShipment("s1");
			var consol = CreateConsol("Consol1");
			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol });
			dashboard.Shipments.Add(shipment);

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "AAJ"));

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolReloadedInAnotherFactory = anotherFactory.Load<ForwardingConsol>(consol.PK);

			var container = consolReloadedInAnotherFactory.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100012";
			container.JC_RC = refContainer.PK;
			anotherFactory.Save();

			dashboard.TryAttach(consol, new[] { shipment });
			var result = dashboard.Save();

			AssertContainsExactElementsInAnyOrder("Saved consols", new[] { consol.PK }, result.Saved.Select(snapshot => snapshot.ConsolPK));
			AssertEquals("Dashboard has unsaved changes", false, dashboard.HasUnsavedChanges);
		}

		public void TestRecreateShipmentConfigurationReturnsErrors()
		{
			var shipment = CreateShipment("s1");
			var consol1 = CreateConsol("c1");
			consol1.JK_AgentType = Core.Constants.AgentType.Direct;
			consol1.Shipments.Add(shipment);

			var consol2 = CreateConsol("c1");

			var preconditionResult = DashboardShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachShipment(consol2, shipment);

			AssertEquals("Precondition: Error exists", "The shipment s1 is already attached to at least one other Direct Consol. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).", preconditionResult.Errors);

			var dashboard = new ConsolDashboard(Factory);
			var errors = dashboard.RecreateShipmentConfiguration(consol2, new[] { shipment.PK });
			AssertEquals("Recreating Shipment Configuration returns attach errors", "The shipment s1 is already attached to at least one other Direct Consol. A shipment on a Direct Consol can only be attached to Direct Consols (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).", errors.First().Message);
		}

		public void TestSave_AllSaved()
		{
			var shipment11 = CreateShipment("11");
			var shipment12 = CreateShipment("12");
			var shipment13 = CreateShipment("13");
			var shipment21 = CreateShipment("21");
			var shipment22 = CreateShipment("22");
			var shipment23 = CreateShipment("23");
			var shipment31 = CreateShipment("31");
			var shipment32 = CreateShipment("32");

			var standaloneShipment1 = CreateShipment("s1");
			var standaloneShipment2 = CreateShipment("s2");
			var standaloneShipment3 = CreateShipment("s3");
			var standaloneShipment4 = CreateShipment("s4");

			var consol1 = CreateConsol("Consol1");
			consol1.Shipments.AddRange(shipment11, shipment12, shipment13);

			var consol2 = CreateConsol("Consol2");
			consol2.Shipments.AddRange(shipment21, shipment22, shipment23);

			var consol3 = CreateConsol("Consol3");
			consol3.Shipments.AddRange(shipment31, shipment32);

			consol1.RunPreSaveValidation();
			AssertEquals("Prerequisite: consol should have no errors", false, consol1.HasErrors);

			consol2.RunPreSaveValidation();
			AssertEquals("Prerequisite: consol should have no errors", false, consol2.HasErrors);

			consol3.RunPreSaveValidation();
			AssertEquals("Prerequisite: consol should have no errors", false, consol3.HasErrors);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol1, consol2, consol3 });
			dashboard.Shipments.AddRange(new[] { standaloneShipment1, standaloneShipment2, standaloneShipment3 });

			AssertEquals("Dashboard should not have unsaved changes", false, dashboard.HasUnsavedChanges);

			dashboard.TryDetach(consol1, new[] { shipment13 });
			dashboard.TryAttach(consol1, new[] { standaloneShipment1, standaloneShipment2 });

			dashboard.TryDetach(consol2, new[] { shipment22, shipment23 });
			dashboard.TryAttach(consol2, new[] { standaloneShipment3 });

			AssertEquals("Dashboard has unsaved changes", true, dashboard.HasUnsavedChanges);

			var result = dashboard.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Dashboard should not have unsaved changes", false, dashboard.HasUnsavedChanges);

				AssertEquals("All consols have been saved", true, result.AllConsolsHaveBeenSaved);
				AssertContainsExactElementsInAnyOrder("Only consols with changes have been processed",
					new[] { consol1.PK, consol2.PK }, result.ConsolSnapshots.Select(snapshot => snapshot.ConsolPK));

				AssertContainsExactElementsInAnyOrder("Consol without changes was not affected",
					new[] { shipment31.PK, shipment32.PK }, consol3.Shipments.Select(shipment => shipment.PK));

				var consol1Snapshot = result.Saved.Single(snapshot => snapshot.ConsolPK == consol1.PK);
				AssertConsolSaveResult(consol1Snapshot,
					expectedShapshotShipmentPKs: new[] { shipment11.PK, shipment12.PK, standaloneShipment1.PK, standaloneShipment2.PK },
					expectedDatabaseShipmentPKs: new[] { shipment11.PK, shipment12.PK, standaloneShipment1.PK, standaloneShipment2.PK });

				var consol2Snapshot = result.Saved.Single(snapshot => snapshot.ConsolPK == consol2.PK);
				AssertConsolSaveResult(consol2Snapshot,
					expectedShapshotShipmentPKs: new[] { shipment21.PK, standaloneShipment3.PK },
					expectedDatabaseShipmentPKs: new[] { shipment21.PK, standaloneShipment3.PK });
			});
		}

		public void TestWorkflowAndTemplateApplicationDeferredDefault()
		{
			AssertEquals("Deferred by default for testing. Need to turn off for patch back to GP1", true, WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolidationPlanningBoardSave.Value);
		}

		public void TestTemplateApplicationDeferredOnSave()
		{
			AssertTemplateApplicationDeferredOnSave(true);
		}

		public void TestTemplateApplicationEnabledOnSave()
		{
			AssertTemplateApplicationDeferredOnSave(false);
		}

		public void AssertTemplateApplicationDeferredOnSave(bool deferred)
		{
			WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolidationPlanningBoardSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, deferred);

			var shipment = CreateShipment("s1");
			var consol = CreateConsol("Consol1");
			Factory.Save();

			CreateShipmentTemplate();
			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol });
			dashboard.Shipments.AddRange(new[] { shipment });
			dashboard.TryAttach(consol, new[] { shipment });

			shipment.Logs.CancelAll();
			AssertEquals("No Template Applied Event", 0, shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode && !l.SL_IsCancelled).Count());
			dashboard.Save();
			AssertEquals("Template should only be applied when !deferred", !deferred, shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode && !l.SL_IsCancelled).Any());
		}

		public void TestFireWorkflowDeferredOnSave()
		{
			AssertFireWorkflowDeferredOnSave(true);
		}

		public void TestFireWorkflowEnabledOnSave()
		{
			AssertFireWorkflowDeferredOnSave(false);
		}

		public void AssertFireWorkflowDeferredOnSave(bool deferred)
		{
			WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolidationPlanningBoardSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, deferred);

			var shipment = CreateShipment("s1");
			var consol = CreateConsol("Consol1");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol });
			dashboard.Shipments.AddRange(new[] { shipment });
			dashboard.TryAttach(consol, new[] { shipment });

			consol.Logs.CancelAll();
			shipment.Logs.CancelAll();
			AssertEquals("No current logs", 0, shipment.Logs.Find(l => !l.SL_IsCancelled).Count() + shipment.Logs.Find(l => !l.SL_IsCancelled).Count());

			dashboard.Save();

			var shpLogs = shipment.Logs.Find(l => !l.SL_IsCancelled);
			var conLogs = consol.Logs.Find(l => !l.SL_IsCancelled);

			CombineAssertions("All logs raised on save are deferred (for performance reasons)", () =>
			{
				AssertEquals("Logs exist", true, shipment.Logs.Find(l => !l.SL_IsCancelled).Any());
				AssertEquals("Logs exist", true, consol.Logs.Find(l => !l.SL_IsCancelled).Any());
				AssertEquals("There should be no logs where SL_FireWorkflow != deferred", 0, shipment.Logs.Find(l => !l.SL_IsCancelled && l.SL_FireWorkflow != deferred).Count());
				AssertEquals("There should be no logs where SL_FireWorkflow != deferred", 0, consol.Logs.Find(l => !l.SL_IsCancelled && l.SL_FireWorkflow != deferred).Count());
			});
		}

		public void TestSave_ConsolsWithValidationErrorsAreNotSaved()
		{
			var shipment11 = CreateShipment("11");
			var shipment21 = CreateShipment("21");
			var shipment22 = CreateShipment("22");

			var standaloneShipment1 = CreateShipment("s1");
			var standaloneShipment2 = CreateShipment("s2");

			standaloneShipment2.JS_GoodsDescription = "Injecting a blatantly invalid value";
			standaloneShipment2.JS_RL_NKOrigin = "#FUUU";

			var consol1 = CreateConsol("Consol1");
			consol1.Shipments.AddRange(shipment11);

			var consol2 = CreateConsol("Consol2");
			consol2.Shipments.AddRange(shipment21, shipment22);

			consol1.RunPreSaveValidation();
			AssertEquals("Prerequisite: consol should have no errors", false, consol1.HasErrors);

			consol2.RunPreSaveValidation();
			AssertEquals("Prerequisite: consol should have no errors", false, consol2.HasErrors);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol1, consol2 });
			dashboard.Shipments.AddRange(new[] { standaloneShipment1, standaloneShipment2 });

			AssertEquals("Dashboard should not have unsaved changes", false, dashboard.HasUnsavedChanges);

			dashboard.TryAttach(consol1, new[] { standaloneShipment1 });
			dashboard.TryAttach(consol2, new[] { standaloneShipment2 });

			AssertEquals("Dashboard has unsaved changes", true, dashboard.HasUnsavedChanges);

			var result = dashboard.Save();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Saved consols",
					new[] { consol1.PK }, result.Saved.Select(snapshot => snapshot.ConsolPK));

				AssertContainsExactElementsInAnyOrder("Not saved due to validation errors",
					new[] { consol2.PK }, result.ValidationErrors.Select(snapshot => snapshot.ConsolPK));

				var consol1Snapshot = result.Saved.Single(snapshot => snapshot.ConsolPK == consol1.PK);
				AssertConsolSaveResult(consol1Snapshot,
					expectedShapshotShipmentPKs: new[] { shipment11.PK, standaloneShipment1.PK },
					expectedDatabaseShipmentPKs: new[] { shipment11.PK, standaloneShipment1.PK });

				var consol2Snapshot = result.ValidationErrors.Single(snapshot => snapshot.ConsolPK == consol2.PK);
				AssertEquals("Error notifications",
					true,
					consol2Snapshot.Notifications.Any(notification => notification.Message.Contains("Origin")));

				AssertConsolSaveResult(consol2Snapshot,
					expectedShapshotShipmentPKs: new[] { shipment21.PK, shipment22.PK, standaloneShipment2.PK },
					expectedDatabaseShipmentPKs: new[] { shipment21.PK, shipment22.PK });
			});

			AssertEquals("Dashboard has unsaved changes", true, dashboard.HasUnsavedChanges);

			dashboard.RemoveCapturedSnapshot(consol2);
			AssertEquals("Dashboard does not have unsaved changes", false, dashboard.HasUnsavedChanges);
		}

		public void TestSave_ConsolsWithPreAllocationExceededAndRestricted()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Weight.Action = PreAllocationCheck.Actions.Restriction;
			checks.Weight.Percentage = 90m;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, checks);

			var standaloneShipment1 = CreateShipment("s1");
			standaloneShipment1.JS_ActualWeight = 2000m;
			standaloneShipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var consol1 = CreateConsol("Consol1");
			consol1.JK_TotalShipmentActWeightCheck = 2000m;
			consol1.WeightVerificationUnit = Core.Constants.Weight.Kilograms;

			consol1.RunPreSaveValidation();
			AssertEquals("Prerequisite: consol should have no errors", false, consol1.HasErrors);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol1 });
			dashboard.Shipments.AddRange(new[] { standaloneShipment1 });

			AssertEquals("Dashboard should not have unsaved changes", false, dashboard.HasUnsavedChanges);

			dashboard.TryAttach(consol1, new[] { standaloneShipment1 });

			AssertEquals("Dashboard has unsaved changes", true, dashboard.HasUnsavedChanges);

			var result = dashboard.Save();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Not saved due to validation errors",
					new[] { consol1.PK }, result.ValidationErrors.Select(snapshot => snapshot.ConsolPK));

				var consol1Snapshot = result.ValidationErrors.Single(snapshot => snapshot.ConsolPK == consol1.PK);
				AssertEquals("Error notifications",
					true,
					consol1Snapshot.Notifications.Any(notification => notification.Message.Contains("Pre-allocated values exceed the registry specified percentage.")));
			});

			AssertEquals("Dashboard has unsaved changes", true, dashboard.HasUnsavedChanges);
		}

		public void TestSave_DetachShipmentWithHavingDuplicatePortError()
		{
			var shipment = CreateShipment("S1");
			var consol1 = CreateConsol("C1");
			var consol2 = CreateConsol("C2");
			shipment.Consols.AddRange(consol1, consol2);

			Factory.Save();
			var dashboard = new ConsolDashboard(Factory);
			dashboard.AddConsols(new[] { consol1 });
			dashboard.Shipments.Add(shipment);

			consol1.MarkAsNeedingValidation();
			consol1.RunPreSaveValidation();

			const string duplicateLoadPortMessage = "Shipment already on a Consol with same Port of Loading.";
			const string duplicateDiscPortMessage = "Shipment already on a Consol with same Port of Discharge.";
			AssertHasRowError(shipment, duplicateLoadPortMessage);
			AssertHasRowError(shipment, duplicateDiscPortMessage);
			AssertEquals("Dashboard should not have unsaved changes", false, dashboard.HasUnsavedChanges);

			var result = dashboard.TryDetach(consol1, new[] { shipment });

			AssertEquals(true, result.AcceptedShipments.Any());
			CombineAssertions("The shipment should no longer be attached to the consol, and should have the updated value on the dashboard", delegate
			{
				AssertEquals(false, result.HasErrors);
				AssertEquals(1, result.AcceptedShipments.Count());
				AssertEquals(0, consol1.Shipments.Count);
				AssertEquals(1, shipment.Consols.Count);
			});

			AssertEquals("Dashboard should have unsaved changes", true, dashboard.HasUnsavedChanges);
			var saveResult = dashboard.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Dashboard should not have unsaved changes", false, dashboard.HasUnsavedChanges);
				AssertEquals("All consols have been saved", true, saveResult.AllConsolsHaveBeenSaved);
				AssertNoRowError(shipment, duplicateLoadPortMessage);
				AssertNoRowError(shipment, duplicateDiscPortMessage);
			});
		}

		public void TestSave_UsesBusyIndicatorIfAvailable()
		{
			AssertBusyIndicatorWasUsed(() =>
			{
				var consol = Factory.New<ForwardingConsol>();

				var dashboard = new ConsolDashboard(Factory);
				dashboard.AddConsols(new[] { consol });

				dashboard.Save();
			});
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolDashboard(Factory);
		}

		void AssertBusyIndicatorWasUsed(Action dashboardAction)
		{
			var busyIndicatorProvider = new Mock<IBusyIndicatorProvider>(MockBehavior.Strict);

			bool busyIndicatorDisposed = false;

			busyIndicatorProvider.Setup(m => m.NewBusyIndicator())
				.Returns(new DisposableAction(() => busyIndicatorDisposed = false, () => busyIndicatorDisposed = true));

			Factory.SetValue(() => busyIndicatorProvider.Object);

			dashboardAction();

			AssertEquals("BusyIndicator should have been used and then disposed", true, busyIndicatorDisposed);
		}

		void AssertConsolSaveResult(DashboardConsolSnapshot snapshot, ZGuid[] expectedShapshotShipmentPKs, ZGuid[] expectedDatabaseShipmentPKs)
		{
			AssertContainsExactElementsInAnyOrder("Consol snapshot had correct shipment configuration",
				expectedShapshotShipmentPKs,
				snapshot.ShipmentPKs);

			var consol = new BusinessObjectFactory().Load<ForwardingConsol>(snapshot.ConsolPK);

			AssertContainsExactElementsInAnyOrder("Saved consol have correct shipment configuration",
				expectedDatabaseShipmentPKs,
				consol.Shipments.Select(shipment => shipment.PK));
		}

		ForwardingShipment CreateShipment(string uniqueConsignRef = null)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			shipment.ConsignorPK = Consignor.PK;
			shipment.ConsigneePK = Consignee.PK;

			shipment.JS_UniqueConsignRef = uniqueConsignRef;

			shipment.RunPreSaveValidation();

			AssertEquals("Prerequisite: shipment should have no errors; please adjust setup if this test fails",
				"",
				shipment.GetErrors().ToUniqueMessageListString());

			return shipment;
		}

		ForwardingConsol CreateConsol(string uniqueConsignRef = null)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			consol.Transports[0].JW_VoyageFlight = "QF512";
			consol.Transports[0].JW_ETD = ZDateTime.Today;

			consol.JK_UniqueConsignRef = uniqueConsignRef;

			consol.RunPreSaveValidation();

			AssertEquals("Prerequisite: consol should have no errors; please adjust setup if this test fails",
				"",
				consol.GetErrors().ToUniqueMessageListString());

			return consol;
		}

		ProcessTaskTemplate CreateShipmentTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			template.WorkflowItems.Tasks.AddNew();
			return template;
		}

		OrgHeader Consignor
		{
			get
			{
				if (consignor == null)
				{
					consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_FullName = "CONSIGNOR";
					consignor.MainAddress.OA_Address1 = "Consignor Address";
					consignor.OH_IsConsignor = true;
				}

				return consignor;
			}
		}
		OrgHeader consignor;

		OrgHeader Consignee
		{
			get
			{
				if (consignee == null)
				{
					consignee = Factory.NewWithValidTestData<OrgHeader>();
					consignee.OH_FullName = "CONSIGNEE";
					consignee.MainAddress.OA_Address1 = "Consignee Address";
					consignee.OH_IsConsignee = true;
				}

				return consignee;
			}
		}
		OrgHeader consignee;

		#endregion
	}
}
