using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingOrderTest : OrderTest
	{
		#region Milestones

		public void TestMilestones()
		{
			var testOrder = Factory.NewWithValidTestData<TrackingOrder>();
			AssertNotNull(testOrder.Milestones);
			AssertEquals(0, testOrder.Milestones.Count);

			var milestone1 = testOrder.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testOrder.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			testOrder.Factory.Save();
			AssertEquals(0, testOrder.Milestones.Count);

			testOrder.ReloadMilestones();
			AssertEquals(2, testOrder.Milestones.Count);
		}

		public void TestEditModeForOrder()
		{
			AssertEquals("Precondition: default registry value", MilestoneVisibilityList.Codes.All, WebDataRegistry.Instance.MilestoneVisibility.Value);

			WebDataRegistry.Instance.MilestoneSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneSortOrderList.Codes.Chronological);

			var testOrder = Factory.NewWithValidTestData<TrackingOrder>();

			var now = ZDateTimeOffset.Now;

			ProcessTask unpublished = testOrder.WorkflowItems.Milestones.AddNew();
			unpublished.P9_Description = "Unpublished";
			unpublished.TriggerConditions.TriggerEventCode = "AAA";
			unpublished.P9_IsPublished = false;
			AssertEquals(unpublished.Status, "");

			ProcessTask completed = testOrder.WorkflowItems.Milestones.AddNew();
			completed.P9_Description = "Completed 1";
			completed.TriggerConditions.TriggerEventCode = "BBB";
			completed.P9_IsPublished = true;
			completed.SetMilestoneActualDateForTest(now.AddHours(-1));
			completed.SetMilestoneScheduledDateForTest(now);

			AssertEquals(completed.Status, "Completed");

			ProcessTask lastCompleted = testOrder.WorkflowItems.Milestones.AddNew();
			lastCompleted.P9_Description = "Completed 2";
			lastCompleted.TriggerConditions.TriggerEventCode = "CCC";
			lastCompleted.P9_IsPublished = true;
			lastCompleted.SetMilestoneActualDateForTest(now.AddHours(-1));
			lastCompleted.SetMilestoneScheduledDateForTest(now);
			AssertEquals(lastCompleted.Status, "Completed");

			ProcessTask pending = testOrder.WorkflowItems.Milestones.AddNew();
			pending.P9_Description = "Pending";
			pending.TriggerConditions.TriggerEventCode = "DDD";
			pending.P9_IsPublished = true;
			pending.SetMilestoneScheduledDateForTest(now);
			AssertEquals(pending.Status, "Pending");

			TrackingMilestoneCollection collection = new TrackingMilestoneCollection(testOrder);
			AssertEquals(3, collection.Count);

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.CompletedMilestonesOnly);
			collection = new TrackingMilestoneCollection(testOrder);
			AssertEquals(2, collection.Count);

			IUpdatableMilestoneEventsProvider updMilestoneEventsProvider = testOrder;

			updMilestoneEventsProvider.UpdatableMilestoneEventCodes.Add("AAA");
			updMilestoneEventsProvider.UpdatableMilestoneEventCodes.Add("CCC");
			updMilestoneEventsProvider.UpdatableMilestoneEventCodes.Add("DDD");

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.CompletedMilestonesOnly);
			TrackingMilestoneCollection editableMilestonesCollection = testOrder.EditableMilestones;
			AssertEquals(2, editableMilestonesCollection.Count);
			AssertEquals(editableMilestonesCollection[0].EventCode, "CCC");
			AssertEquals(editableMilestonesCollection[1].EventCode, "DDD");

			Factory.Save();

			var notifier = new BusinessObjectChangesEmailNotifier(testOrder);

			editableMilestonesCollection[0].ActualDate = now.AddHours(-1);
			editableMilestonesCollection[1].ActualDate = now.AddHours(-1);

			Factory.Save();

			var propertyInfos = testOrder.GetPropertiesForEmailReporting();
			var emailedMilestones = propertyInfos.Where(x => x.HumanReadableName.ToString().StartsWith("Milestone:")).OrderByDescending(x => x.HasChanges).ToArray();
			AssertEquals(2, emailedMilestones.Length);

			var originalValueMilestone0 = string.Join(System.Environment.NewLine, "Description: Pending",
				string.Format("Estimated Date: {0}", WebDateTimeFormatter.GetFormattedDate(now, ZDateTimePickerFormat.Long)),
				"Actual Date: ",
				"Status: Pending");

			var updatedValueMilestone0 = string.Join(System.Environment.NewLine, "Description: Pending",
				string.Format("Estimated Date: {0}", WebDateTimeFormatter.GetFormattedDate(now, ZDateTimePickerFormat.Long)),
				string.Format("Actual Date: {0}", WebDateTimeFormatter.GetFormattedDate(now.AddHours(-1), ZDateTimePickerFormat.Long)),
				"Status: Completed");

			var originalValueMilestone1 = string.Join(System.Environment.NewLine, "Description: Completed 2",
				string.Format("Estimated Date: {0}", WebDateTimeFormatter.GetFormattedDate(now, ZDateTimePickerFormat.Long)),
				string.Format("Actual Date: {0}", WebDateTimeFormatter.GetFormattedDate(now.AddHours(-1), ZDateTimePickerFormat.Long)),
				"Status: Completed");

			var updatedValueMilestone1 = string.Join(System.Environment.NewLine, "Description: Completed 2",
				string.Format("Estimated Date: {0}", WebDateTimeFormatter.GetFormattedDate(now, ZDateTimePickerFormat.Long)),
				string.Format("Actual Date: {0}", WebDateTimeFormatter.GetFormattedDate(now.AddHours(-1), ZDateTimePickerFormat.Long)),
				"Status: Completed");

			AssertEquals(originalValueMilestone0, emailedMilestones[0].OriginalValue);
			AssertEquals(updatedValueMilestone0, emailedMilestones[0].UpdatedValue);
			AssertEquals(originalValueMilestone1, emailedMilestones[1].OriginalValue);
			AssertEquals(updatedValueMilestone1, emailedMilestones[1].UpdatedValue);
		}

		#endregion

		protected override Order NewOrder()
		{
			return Factory.New<TrackingOrder>();
		}

		public void TestServiceLevels()
		{
			bool initialValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = false;
				TrackingOrder order = Factory.New<TrackingOrder>();
				AssertEquals(typeof(ActiveServiceLevelCollection).ToString(), order.JD_RS_List.GetType().ToString());

				Globals.IsWeb = true;
				Assert(order.JD_RS_List is WebServiceLevelCollection);

				AssertEquals(new ZQuery(), order.GetTemporaryPublishedServiceLevelQuery());

				ZString temporaryLevelDescription = "testDescription";
				order.TemporaryServiceLevelDescription = temporaryLevelDescription;
				AssertEquals(new ZQuery(RefServiceLevelSchema.RS_Description, temporaryLevelDescription).LiteralTextSqlFormatted, order.GetTemporaryPublishedServiceLevelQuery().LiteralTextSqlFormatted);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		#region TestNotificationOptions

		public void TestNotificationOptions()
		{
			AssertEquals(WebDataRegistry.Instance.OrderNotificationOptions, ((IBizOChangesEmailNotification)NewOrder()).NotificationSendingRule);
		}

		#endregion

		#region Test Order Status Change Notification

		public void TestOrderStatusChangeNotification()
		{
			var testOrder = Factory.NewWithValidTestData<TrackingOrder>();
			var notifier = new BusinessObjectChangesEmailNotifier(testOrder);
			testOrder.JD_OrderStatus = Constants.OrderStatus.Cancelled;

			Factory.Save();

			var propertyInfos = testOrder.GetPropertiesForEmailReporting();
			var emailedOrderStatus = propertyInfos.SingleOrDefault(x => x.HumanReadableName.ToString().StartsWith("Order Status"));

			AssertEquals(Constants.OrderStatus.Incomplete, emailedOrderStatus.OriginalValue);
			AssertEquals(Constants.OrderStatus.Cancelled, emailedOrderStatus.UpdatedValue);
		}

		#endregion

		#region Test IsCancelled

		public void TestIsCancelled()
		{
			var order = NewOrder() as TrackingOrder;
			Assert("Not cancelled by default", !order.IsCancelled);

			order.JD_OrderStatus = Constants.OrderStatus.Cancelled;
			Assert("Cancelled if Order Status is Cancelled", order.IsCancelled);

			order.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			order.JD_IsCancelled = true;
			Assert("Cancelled if JD_IsCancelled is true", order.IsCancelled);
		}

		#endregion

		#region TestEventBranch

		public void TestEventBranch()
		{
			TrackingOrder testBizO = NewOrder() as TrackingOrder;
			IBizOChangesEmailNotification testBizOwithNotifier = testBizO;

			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			testBizO.BuyerPK = buyer.PK;

			JobDocAddress address = testBizO.DocAddresses.AddNew(DocAddressType.ControllingCustomer);
			OrgHeader controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_RL_NKClosestPort = "AUMEL";
			address.OrganisationPK = controllingCustomer.PK;

			AssertNull("Nothing was set -- should return null", testBizOwithNotifier.EventBranch);

			var controllingCustomerBranch = CreateBranch(controllingCustomer, "USDAL", true);
			Factory.Save();

			AssertEquals("controllingCustomerBranch", controllingCustomerBranch, testBizOwithNotifier.EventBranch);

			address.OrganisationPK = ZGuid.Empty;
			GlbBranch branch1 = CreateBranch(buyer, "AUMEL", false);
			GlbBranch branch2 = CreateBranch(buyer, "NZAKL", false);
			GlbBranch branch3 = CreateBranch(buyer, "USPTQ", false);
			buyer.OH_RL_NKClosestPort = "NZAKL";
			Factory.Save();

			AssertEquals("No controlling branches and no port of discharge so it searches by buyer's closest port", branch2, testBizOwithNotifier.EventBranch);

			testBizO.JD_RL_NKPortOfDischarge = "USPTQ";
			AssertEquals("No controlling branches so it searches by port of discharge", branch3, testBizOwithNotifier.EventBranch);

			GlbBranch branch5 = CreateBranch(buyer, "USNYC", true);
			Factory.Save();
			AssertEquals("It searches by controlling branch for buyer", branch5, testBizOwithNotifier.EventBranch);
		}

		GlbBranch CreateBranch(OrgHeader org, string homePort, bool setControllingBranch)
		{
			OrgCompanyData orgCompanyData = org.CompanyDataCollection.AddNew();

			GlbCompany glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_IsActive = true;

			GlbBranch glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch.GB_GC = glbCompany.PK;
			glbBranch.GB_IsActive = true;
			glbBranch.GB_RL_NKHomePort = homePort;

			orgCompanyData.OB_GC = glbCompany.PK;

			if (setControllingBranch)
			{
				orgCompanyData.OB_GB_ControllingBranch = glbBranch.PK;
			}

			return glbBranch;
		}

		#endregion

		#region TestPlannedVoyages

		void AssertPlannedVoyageContent(ZString voyageType, PlannedVoyage expected, PlannedVoyage actual)
		{
			AssertEquals(string.Format("Wrong VoyageType on {0} planned voyage", voyageType), expected.VoyageType, actual.VoyageType);
			AssertEquals(string.Format("Wrong Vessel on {0} planned voyage", voyageType), expected.Vessel, actual.Vessel);
			AssertEquals(string.Format("Wrong Voyage on {0} planned voyage", voyageType), expected.Voyage, actual.Voyage);
			AssertEquals(string.Format("Wrong ETD on {0} planned voyage", voyageType), expected.ETD, actual.ETD);
			AssertEquals(string.Format("Wrong ETA on {0} planned voyage", voyageType), expected.ETA, actual.ETA);
		}

		public void TestZeroPlannedVoyages()
		{
			bool previousState = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				TrackingOrder order = NewOrder() as TrackingOrder;
				AssertEquals("Should be no voyages", 0, order.PlannedVoyages.Count);
			}
			finally
			{
				Globals.IsWeb = previousState;
			}
		}

		public void TestSinglePlannedVoyage()
		{
			bool previousState = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				TrackingOrder order = NewOrder() as TrackingOrder;
				order.JD_TransportMode = Constants.TransportModes.Air;

				ZDateTime departureDate = ZDateTime.SmallDateTimeNow.AddDays(-6);
				ZDateTime arrivalDate = ZDateTime.SmallDateTimeNow.AddDays(-1);

				order.JD_RV_NKDepartureVessel = "ALEKSANDROV";
				order.JD_Milestone_E_DEP = departureDate;
				order.JD_Milestone_E_ARV = arrivalDate;
				order.JD_RV_NKArrivalVessel = "ALEKSANDROV";
				order.JD_DepartureVoyage = "444";
				order.JD_ArrivalVoyage = "444";

				AssertEquals("Should be one voyage", 1, order.PlannedVoyages.Count);
				AssertPlannedVoyageContent("single",
										new PlannedVoyage
										{
											VoyageType = "Arrival",
											Vessel = "ALEKSANDROV",
											Voyage = "444",
											ETD = departureDate,
											ETA = arrivalDate
										},
										order.PlannedVoyages[0]);
			}
			finally
			{
				Globals.IsWeb = previousState;
			}
		}

		public void TestTwoPlannedVoyages()
		{
			bool previousState = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				TrackingOrder testOrder = NewOrder() as TrackingOrder;

				ZDateTime departureDate1 = ZDateTime.SmallDateTimeNow.AddDays(-6);
				ZDateTime arrivalDate1 = ZDateTime.SmallDateTimeNow.AddDays(-5);
				ZDateTime departureDate3 = ZDateTime.SmallDateTimeNow.AddDays(-4);
				ZDateTime arrivalDate3 = ZDateTime.SmallDateTimeNow.AddDays(-3);

				testOrder.JD_RV_NKDepartureVessel = "ALEKSANDROV";
				testOrder.JD_DepartureVoyage = "111";
				testOrder.JD_Milestone_E_DEP = departureDate1;
				testOrder.JD_E_ARV_1stIntermediate = arrivalDate1;

				testOrder.JD_RV_NKArrivalVessel = "ALEKSANDROV3";
				testOrder.JD_ArrivalVoyage = "333";
				testOrder.JD_E_DEP_3 = departureDate3;
				testOrder.JD_Milestone_E_ARV = arrivalDate3;

				AssertEquals("Should be two voyages", 2, testOrder.PlannedVoyages.Count);

				AssertPlannedVoyageContent("1/2",
										new PlannedVoyage
										{
											VoyageType = "Departure",
											Vessel = "ALEKSANDROV",
											Voyage = "111",
											ETD = departureDate1,
											ETA = arrivalDate1
										},
										testOrder.PlannedVoyages[0]);

				AssertPlannedVoyageContent("2/2",
										new PlannedVoyage
										{
											VoyageType = "Arrival",
											Vessel = "ALEKSANDROV3",
											Voyage = "333",
											ETD = departureDate3,
											ETA = arrivalDate3
										},
										testOrder.PlannedVoyages[1]);
			}
			finally
			{
				Globals.IsWeb = previousState;
			}
		}

		public void TestThreePlannedVoyages()
		{
			bool previousState = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				TrackingOrder order = NewOrder() as TrackingOrder;

				ZDateTime departureDate1 = ZDateTime.SmallDateTimeNow.AddDays(-6);
				ZDateTime arrivalDate1 = ZDateTime.SmallDateTimeNow.AddDays(-5);
				ZDateTime departureDate2 = ZDateTime.SmallDateTimeNow.AddDays(-4);
				ZDateTime arrivalDate2 = ZDateTime.SmallDateTimeNow.AddDays(-3);
				ZDateTime departureDate3 = ZDateTime.SmallDateTimeNow.AddDays(-1);
				ZDateTime arrivalDate3 = ZDateTime.SmallDateTimeNow.AddDays(-2);

				order.JD_RV_NKDepartureVessel = "ALEKSANDROV";
				order.JD_DepartureVoyage = "111";
				order.JD_Milestone_E_DEP = departureDate1;
				order.JD_E_ARV_1stIntermediate = arrivalDate1;

				order.JD_RV_NKIntermediateVessel = "ALEKSANDROV2";
				order.JD_IntermediateVoyage = "222";
				order.JD_E_DEP_2 = departureDate2;
				order.JD_E_ARV_2ndIntermediate = arrivalDate2;

				order.JD_RV_NKArrivalVessel = "ALEKSANDROV3";
				order.JD_ArrivalVoyage = "333";
				order.JD_E_DEP_3 = departureDate3;
				order.JD_Milestone_E_ARV = arrivalDate3;

				AssertEquals("Should be three voyages", 3, order.PlannedVoyages.Count);

				AssertPlannedVoyageContent("1/3",
										new PlannedVoyage
										{
											VoyageType = "Departure",
											Vessel = "ALEKSANDROV",
											Voyage = "111",
											ETD = departureDate1,
											ETA = arrivalDate1
										},
										order.PlannedVoyages[0]);

				AssertPlannedVoyageContent("2/3",
										new PlannedVoyage
										{
											VoyageType = "Intermediate",
											Vessel = "ALEKSANDROV2",
											Voyage = "222",
											ETD = departureDate2,
											ETA = arrivalDate2
										},
										order.PlannedVoyages[1]);

				AssertPlannedVoyageContent("3/3",
										new PlannedVoyage
										{
											VoyageType = "Arrival",
											Vessel = "ALEKSANDROV3",
											Voyage = "333",
											ETD = departureDate3,
											ETA = arrivalDate3
										},
										order.PlannedVoyages[2]);
			}
			finally
			{
				Globals.IsWeb = previousState;
			}
		}

		public void TestPlannedVoyagesAreBeingSuppressed()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				TrackingOrder order = NewOrder() as TrackingOrder;
				order.JD_TransportMode = Constants.TransportModes.Sea;
				order.JD_RV_NKDepartureVessel = "ALEKSANDROV";
				order.JD_DepartureVoyage = "111";
				order.JD_RV_NKIntermediateVessel = "ALEKSANDROV2";
				order.JD_IntermediateVoyage = "222";
				order.JD_RV_NKArrivalVessel = "ALEKSANDROV3";
				order.JD_ArrivalVoyage = "333";

				order.JD_Milestone_E_DEP = ZDateTime.Now.AddDays(2);
				order.JD_E_ARV_1stIntermediate = ZDateTime.Now.AddDays(3);
				order.JD_E_DEP_2 = ZDateTime.Now.AddDays(4);
				order.JD_E_ARV_2ndIntermediate = ZDateTime.Now.AddDays(5);
				order.JD_E_DEP_3 = ZDateTime.Now.AddDays(6);
				order.JD_Milestone_E_ARV = ZDateTime.Now.AddDays(7);

				AssertEquals("Should be three voyages because of Sea mode", 3, order.PlannedVoyages.Count);

				order.JD_TransportMode = Constants.TransportModes.Air;

				AssertEquals("Should be three voyages because air mode now unsuppressed by default", 3, order.PlannedVoyages.Count);

				order.JD_Milestone_E_DEP = ZDateTime.Now.AddDays(-2);

				AssertEquals("Should be three voyages because departure date has passed", 3, order.PlannedVoyages.Count);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		#endregion

		public void TestJD_OrderStatusDesc()
		{
			TrackingOrder order = NewOrder() as TrackingOrder;
			order.JD_OrderStatus = "INC";
			AssertEquals("JD_OrderStatusDesc", "Incomplete", order.JD_OrderStatusDesc);
			order.JD_OrderStatus = "PLC";
			AssertEquals("JD_OrderStatusDesc", new CodeDescriptionPairList(OLookUpEditType.OrderHeaderStatus).GetDescriptionFromCode("PLC"), order.JD_OrderStatusDesc);
			order.JD_OrderStatus = "CNF";
			AssertEquals("JD_OrderStatusDesc", "Confirmed", order.JD_OrderStatusDesc);
			order.JD_OrderStatus = "SHP";
			AssertEquals("JD_OrderStatusDesc", "Shipped", order.JD_OrderStatusDesc);
			order.JD_OrderStatus = "PRT";
			AssertEquals("JD_OrderStatusDesc", "Part Delivered", order.JD_OrderStatusDesc);
			order.JD_OrderStatus = "DLV";
			AssertEquals("JD_OrderStatusDesc", "Delivered", order.JD_OrderStatusDesc);
			order.JD_OrderStatus = "CAN";
			AssertEquals("JD_OrderStatusDesc", new CodeDescriptionPairList(OLookUpEditType.OrderHeaderStatus).GetDescriptionFromCode("CAN"), order.JD_OrderStatusDesc);
		}

		public void TestSuppressedFields()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				TrackingOrder order = (TrackingOrder)NewOrder();
				order.JD_TransportMode = Constants.TransportModes.Sea;
				order.JD_Milestone_E_DEP = ZDateTime.Now.AddDays(1);
				order.JD_Milestone_E_ARV = ZDateTime.Now.AddDays(2);
				order.JD_Milestone_A_DEP = ZDateTime.Now.AddDays(1);
				order.JD_Milestone_A_ARV = ZDateTime.Now.AddDays(2);
				order.JD_E_DEP_2 = ZDateTime.Now.AddDays(3);
				order.JD_E_ARV_2ndIntermediate = ZDateTime.Now.AddDays(4);
				order.JD_ArrivalVoyage = "12345";
				order.JD_RV_NKArrivalVessel = "ORIENTAL PHOENIX";
				order.JD_IntermediateVoyage = "45678";
				order.JD_RV_NKIntermediateVessel = "ORIENTAL STAR";

				AssertEquals("Should not be suppressed on sea shipments.", order.JD_Milestone_E_DEP, order.ETDWithSuppression);
				AssertEquals("Should not be suppressed on sea shipments.", order.JD_Milestone_E_ARV, order.ETAWithSuppression);
				AssertEquals("Should not be suppressed on sea shipments.", order.JD_Milestone_A_DEP, order.ATDWithSuppression);
				AssertEquals("Should not be suppressed on sea shipments.", order.JD_Milestone_A_ARV, order.ATAWithSuppression);
				AssertEquals("Should not be suppressed on sea shipments.", order.JD_E_DEP_2, order.IntermediateETDWithSuppression);
				AssertEquals("Should not be suppressed on sea shipments.", order.JD_E_ARV_2ndIntermediate, order.IntermediateETAWithSuppression);
				AssertEquals("Should not be suppressed on sea shipments.", order.JD_ArrivalVoyage, order.ArrivalVoyageWithSuppression);
				AssertEquals("Should not be suppressed on sea shipments.", order.JD_IntermediateVoyage, order.IntermediateVoyageWithSuppression);

				order.JD_TransportMode = Constants.TransportModes.Air;
				order.JD_RV_NKArrivalVessel = "ORIENTAL PHOENIX";
				order.JD_RV_NKIntermediateVessel = "ORIENTAL STAR";

				AssertEquals("Should not be suppressed on air shipments.", order.JD_Milestone_E_DEP, order.ETDWithSuppression);
				AssertEquals("Should not be suppressed on air shipments.", order.JD_Milestone_E_ARV, order.ETAWithSuppression);
				AssertEquals("Should not be suppressed on air shipments.", order.JD_Milestone_A_DEP, order.ATDWithSuppression);
				AssertEquals("Should not be suppressed on air shipments.", order.JD_Milestone_A_ARV, order.ATAWithSuppression);
				AssertEquals("Should not be suppressed on air shipments.", order.JD_E_DEP_2, order.IntermediateETDWithSuppression);
				AssertEquals("Should not be suppressed on air shipments.", order.JD_E_ARV_2ndIntermediate, order.IntermediateETAWithSuppression);
				AssertEquals("Should not be suppressed on air shipments.", order.JD_ArrivalVoyage, order.ArrivalVoyageWithSuppression);
				AssertEquals("Should not be suppressed on air shipments.", order.JD_IntermediateVoyage, order.IntermediateVoyageWithSuppression);

				SuppressionForTest.CacheObjectClear();
				SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, true);

				AssertEquals("Should be suppressed.", Suppression.SuppressedDate, order.ETDWithSuppression);
				AssertEquals("Should be suppressed.", Suppression.SuppressedDate, order.ETAWithSuppression);
				AssertEquals("Should be suppressed.", Suppression.SuppressedDate, order.ATDWithSuppression);
				AssertEquals("Should be suppressed.", Suppression.SuppressedDate, order.ATAWithSuppression);
				AssertEquals("Should be suppressed.", Suppression.SuppressedDate, order.IntermediateETDWithSuppression);
				AssertEquals("Should be suppressed.", Suppression.SuppressedDate, order.IntermediateETAWithSuppression);
				AssertEquals("Should be suppressed.", "*SUPPRESSED*", order.ArrivalVoyageWithSuppression);
				AssertEquals("Should be suppressed.", "*SUPPRESSED*", order.IntermediateVoyageWithSuppression);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		public void TestGenerateOrderLineDetailsForEmailReporting()
		{
			TrackingOrder order = Factory.New<TrackingOrder>();
			OrderLine orderLine1 = Factory.New<OrderLine>();

			orderLine1.JO_LineNo = 1;
			orderLine1.JO_Partno = "COOLRIDGE";
			orderLine1.JO_Description = "water";
			orderLine1.JO_InnerPacks = 1;
			orderLine1.JO_OuterPacks = 1;
			orderLine1.JO_Quantity = 1;
			orderLine1.JO_QtyInvoiced = 1;
			orderLine1.JO_QtyReceived = 1;
			orderLine1.JO_F3_NKPackType = "UNT";
			orderLine1.JO_ItemPrice = 1;
			orderLine1.JO_LinePrice = 1;

			ZString expectedString = string.Format("Part #: COOLRIDGE{0}Description: water{0}Inner Packs: 1{0}Outer Packs: 1{0}Qty Ordered: 1{0}Qty Invoiced: 1{0}Qty Received: 1{0}Qty Remaining: 0{0}Unit of Qty: UNT{0}Item Price: 1{0}Total Price: 1", System.Environment.NewLine);

			AssertEquals("OrderLine Details Generation", expectedString, order.GenerateOrderLineDetailsForEmailReporting(orderLine1));
		}

		#region GoodsAvailableAtAddress, GoodsDeliveredToAddress and JD_MasterWayBill

		public void TestShipmentRelatedProperties()
		{
			TrackingOrder testTrackingOrder = Factory.New<TrackingOrder>();
			testTrackingOrder.JD_MasterWaybill = "MBLTEST";

			OrgHeader testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "TEST1";
			testOrg1.OH_FullName = "Test Org 1";
			testTrackingOrder.BuyerPK = testOrg1.PK;

			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_Code = "TEST2";
			testOrg2.OH_FullName = "Test Org 2";
			testTrackingOrder.SupplierPK = testOrg2.PK;

			var pickupAddress = Factory.NewWithValidTestData<OrgAddress>();
			pickupAddress.OA_Code = "Test Pickup Address";

			var deliveryAddress = Factory.NewWithValidTestData<OrgAddress>();
			deliveryAddress.OA_Code = "Test Delivery Address";

			testTrackingOrder.GoodsAvailableAtAddress.E2_OA_Address = pickupAddress.PK;
			testTrackingOrder.GoodsDeliveredToAddress.E2_OA_Address = deliveryAddress.PK;

			AssertEquals("The pickup address set must equal the pickup address retrieved.", "Test Pickup Address", testTrackingOrder.GoodsAvailableAtAddress.Address.OA_Code);
			AssertEquals("The delivery address set must equal the delivery address retrieved.", "Test Delivery Address", testTrackingOrder.GoodsDeliveredToAddress.Address.OA_Code);
			AssertEquals("The master waybill set must equal the master waybill retrieved.", "MBLTEST", testTrackingOrder.JD_MasterWaybill);

			TrackingShipment testShipment = Factory.New<TrackingShipment>();

			TrackingConsol consol = testShipment.Consols.AddNew();
			consol.JK_MasterBillNum = "MBLTEST2";
			Factory.Save();

			OrgAddress testAddress1 = Factory.New<OrgAddress>();
			testAddress1.OA_Address1 = "TEST ADDRESS 1 ";
			testOrg1.Addresses.Add(testAddress1);

			OrgAddress testAddress2 = Factory.New<OrgAddress>();
			testAddress2.OA_Address1 = "TEST ADDRESS 2 ";
			testOrg2.Addresses.Add(testAddress2);

			testShipment.ConsignorPickupAddress.E2_OA_Address = testAddress1.PK;
			testShipment.ConsigneeDeliveryAddress.E2_OA_Address = testAddress2.PK;

			BaseJobDeclaration testDeclaration = Factory.New<BaseJobDeclaration>();
			testDeclaration.JE_JS = testShipment.PK;

			testTrackingOrder.JD_JS = testShipment.PK;
			testTrackingOrder.JD_JE = testDeclaration.PK;

			Factory.Save();

			AssertNotNull("The attached shipment must be present on the order.", testTrackingOrder.Shipment);
			AssertNotNull("The attached shipment declaration must be present on the order.", testTrackingOrder.ShipOrDec);

			AssertEquals("The pickup address of the attached shipment must be retrieved.", "Test Pickup Address", testTrackingOrder.GoodsAvailableAtAddress.Address.OA_Code);
			AssertEquals("The delivery address of the attached shipment must be retrieved.", "Test Delivery Address", testTrackingOrder.GoodsDeliveredToAddress.Address.OA_Code);
			AssertEquals("The master bill number of the attached shipment must be retrieved.", "MBLTEST2", testTrackingOrder.JD_MasterWaybill);
		}

		#endregion

		#region TestContainers

		public void TestPlannedContainerNumbers()
		{
			var order = Factory.New<TrackingOrder>();
			order.PlannedContainers.AddNew().J1_ContainerNumber = "Planned1";
			order.PlannedContainers.AddNew().J1_ContainerNumber = "Planned2";

			var otherOrder = Factory.New<TrackingOrder>();
			otherOrder.PlannedContainers.AddNew().J1_ContainerNumber = "Planned3";

			AssertEquals(2, order.PlannedContainerNumbers.Count);
			AssertPlannedContainers(order.PlannedContainers[0].J1_ContainerNumber, order.PlannedContainerNumbers, true);
			AssertPlannedContainers(order.PlannedContainers[1].J1_ContainerNumber, order.PlannedContainerNumbers, true);
		}

		void AssertPlannedContainers(string expectedContainerName, PKDescriptionCollection testedCollection, bool doNotCreateHyperLink)
		{
			foreach (PKDescription container in testedCollection)
			{
				if (expectedContainerName == container.Description)
				{
					AssertEquals("Hyperlink is incorrect", doNotCreateHyperLink, container.DoNotCreateHyperLink);
					return;
				}
			}
			Assert("Container " + expectedContainerName + " could not be found", false);
		}

		public void TestContainers()
		{
			var order = Factory.New<TrackingOrder>();
			order.PlannedContainers.AddNew().J1_ContainerNumber = "Planned";

			AssertContainers(order.PlannedContainers[0].J1_ContainerNumber, order.Containers, true);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.CusContainers.AddNew().CO_ContainerNumber = "Declaration";
			order.JD_JE = declaration.PK;

			AssertContainers(declaration.CusContainers[0].CO_ContainerNumber, order.Containers, true);

			var shipment = Factory.New<TrackingShipment>();
			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			var line = shipment.OuterPackLines.AddNew();
			container.JC_ContainerNum = "Shipment";
			line.Containers.RemoveAll();
			line.JL_FreightMode = FreightConstants.OuterPackType;
			line.Containers.Add(container);
			order.JD_JS = shipment.PK;

			AssertContainers(shipment.Containers.First().JC_ContainerNum, order.Containers, false);
		}

		void AssertContainers(string expectedContainerName, PKDescriptionCollection testedCollection, bool doNotCreateHyperLink)
		{
			AssertEquals("Container exists", 1, testedCollection.Count);
			AssertEquals("Container name", expectedContainerName, testedCollection[0].Description);
			AssertEquals("Hyperlink should be created", doNotCreateHyperLink, testedCollection[0].DoNotCreateHyperLink);
		}

		#endregion

		#region TestProducts

		public void TestProducts()
		{
			TrackingOrder order = Factory.NewWithValidTestData<TrackingOrder>();
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			order.BuyerPK = buyer.PK;

			Assert("Precondition: no products attached", order.Products.Count == 0);

			order.OrderLines.Add(GetOrderLine(1, "Shoes", buyer.PK, supplier.PK));
			order.OrderLines.Add(GetOrderLine(2, "T-shirts", buyer.PK, supplier.PK));
			order.OrderLines.Add(GetOrderLine(3, "T-shirts"));

			Factory.Save();

			AssertEquals(2, order.Products.Count);
		}

		public void TestProductWithDuplicatedCode()
		{
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();

			MasterFiles.Business.OrgSupplierPart productWithOwner = Factory.NewWithValidTestData<MasterFiles.Business.OrgSupplierPart>();
			productWithOwner.OP_PartNum = "BNB";
			productWithOwner.OP_Desc = "BNB With Owner1";

			OrgPartRelation relation = productWithOwner.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_OP = productWithOwner.PK;

			MasterFiles.Business.OrgSupplierPart productWithOwnerAndSupplier = Factory.NewWithValidTestData<MasterFiles.Business.OrgSupplierPart>();
			productWithOwnerAndSupplier.OP_PartNum = "BNB";
			productWithOwnerAndSupplier.OP_Desc = "BNB With Owner2 And Supplier";

			OrgPartRelation relation1 = productWithOwnerAndSupplier.RelatedOrganisations.AddNew();
			relation1.OU_OH = buyer2.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation1.OU_OP = productWithOwnerAndSupplier.PK;

			OrgPartRelation relation2 = productWithOwnerAndSupplier.RelatedOrganisations.AddNew();
			relation2.OU_OH = supplier.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation2.OU_OP = productWithOwnerAndSupplier.PK;

			TrackingOrder order = Factory.NewWithValidTestData<TrackingOrder>();
			order.BuyerPK = buyer2.PK;
			order.SupplierPK = supplier.PK;

			Assert("Precondition: no products attached", order.Products.Count == 0);

			OrderLine line = Factory.NewWithValidTestData<OrderLine>();
			line.JO_LineNo = 1;
			line.JO_Partno = productWithOwnerAndSupplier.OP_PartNum;

			order.OrderLines.Add(line);

			TrackingOrder order1 = Factory.NewWithValidTestData<TrackingOrder>();
			order1.BuyerPK = buyer.PK;

			Assert("Precondition: no products attached", order1.Products.Count == 0);

			OrderLine line1 = Factory.NewWithValidTestData<OrderLine>();
			line1.JO_LineNo = 1;
			line1.JO_Partno = productWithOwner.OP_PartNum;

			order1.OrderLines.Add(line1);

			Factory.Save();

			AssertEquals(1, order.Products.Count);
			AssertEquals("BNB", order.Products[0].OP_PartNum);
			AssertEquals("BNB With Owner2 And Supplier", order.Products[0].OP_Desc);

			AssertEquals(1, order1.Products.Count);
			AssertEquals("BNB", order1.Products[0].OP_PartNum);
			AssertEquals("BNB With Owner1", order1.Products[0].OP_Desc);
		}

		OrderLine GetOrderLine(int lineNo, string productName, ZGuid buyer, ZGuid supplier)
		{
			MasterFiles.Business.OrgSupplierPart product = Factory.NewWithValidTestData<MasterFiles.Business.OrgSupplierPart>();
			product.OP_PartNum = productName;

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_OP = product.PK;

			OrgPartRelation relation1 = product.RelatedOrganisations.AddNew();
			relation1.OU_OH = supplier;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation1.OU_OP = product.PK;

			return GetOrderLine(lineNo, product.OP_PartNum);
		}

		OrderLine GetOrderLine(int lineNo, ZString partNum)
		{
			OrderLine line = Factory.NewWithValidTestData<OrderLine>();
			line.JO_LineNo = lineNo;
			line.JO_Partno = partNum;

			return line;
		}

		#endregion

		#region TestShipOrDecProperties

		public void TestShipOrDecProperties()
		{
			TrackingOrder order = Factory.New<TrackingOrder>();
			order.JD_RS_NKServiceLevel_NI = "STD";

			Assert("ShipOrDecPK is empty if no shipment is attached", order.ShipOrDecPK.IsEmpty);
			Assert("ShipOrDecNumber is empty if no shipment is attached", order.ShipOrDecNumber.IsEmpty);
			Assert("ShipOrDecTableName is empty if no shipment is attached", string.IsNullOrEmpty(order.ShipOrDecTableName));
			AssertEquals("Service Level is pulled from Order", order.ServiceLevel, "STD");

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_BookingReference = "001";
			shipment.JS_RS_NKServiceLevel = "EXP";
			order.JD_JS = shipment.PK;

			AssertEquals("ShipOrDecPK", order.ShipOrDec.PersistentBizOPK, order.ShipOrDecPK);
			AssertEquals("ShipOrDecNumber", order.ShipOrDec.Number, order.ShipOrDecNumber);
			AssertEquals("ShipOrDecTableName", shipment.TableName, order.ShipOrDecTableName);
			AssertEquals("Service Level is pulled from Shipment", order.ServiceLevel, "EXP");
		}

		#endregion

		#region TestPickupDeliveryAddressAsString

		void SetupOrgAddressForTest(OrgAddress address, ZString prefix)
		{
			address.OA_CompanyNameOverride = prefix + "CompanyName";
			address.OA_Address1 = prefix + "Add1";
			address.OA_Code = prefix + "code";
			address.OA_Address2 = prefix + "Add2";
			address.OA_City = prefix + "City";
			address.OA_State = prefix + "State";
		}

		public void TestPickupDeliveryAddressAsStringWithAddressInOrder()
		{
			var pUAddress = Factory.NewWithValidTestData<OrgAddress>();
			var dAddress = Factory.NewWithValidTestData<OrgAddress>();
			SetupOrgAddressForTest(pUAddress, "PU");
			SetupOrgAddressForTest(dAddress, "D");

			var order = Factory.New<TrackingOrder>();
			var shipment = Factory.New<TrackingShipment>();
			order.JD_JS = shipment.PK;
			order.SupplierPK = pUAddress.OA_OH;
			order.BuyerPK = dAddress.OA_OH;

			order.GoodsDeliveredToAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_CompanyName = "";
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "Delivery Address";
			shipment.ConsigneeDeliveryAddress.E2_City = "Delivery City";
			shipment.ConsigneeDeliveryAddress.E2_State = "Delivery";
			shipment.ConsigneeDeliveryAddress.E2_Postcode = "1111";

			order.GoodsAvailableAtAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.ConsignorPickupAddress.E2_CompanyName = "";
			shipment.ConsignorPickupAddress.E2_Address1 = "Pickup Address";
			shipment.ConsignorPickupAddress.E2_City = "Pickup City";
			shipment.ConsignorPickupAddress.E2_State = "Pickup";
			shipment.ConsignorPickupAddress.E2_Postcode = "1111";

			AssertEquals(shipment.DeliverToFullAddress, order.DeliveryAddressAsString);
			AssertEquals(shipment.PickupFromFullAddress, order.PickupAddressAsString);

			order.GoodsAvailableAtAddress.E2_OA_Address = pUAddress.PK;
			order.GoodsDeliveredToAddress.E2_OA_Address = dAddress.PK;
			AssertEquals("PUCOMPANYNAME PUADD1 PUADD2 PUCITY PUSTATE", order.PickupAddressAsString);
			AssertEquals("DCOMPANYNAME DADD1 DADD2 DCITY DSTATE", order.DeliveryAddressAsString);
		}

		public void TestPickupDeliveryAddressAsString_DoesNotLookupByCode()
		{
			var pUAddress = Factory.NewWithValidTestData<OrgAddress>();
			var dAddress = Factory.NewWithValidTestData<OrgAddress>();
			SetupOrgAddressForTest(pUAddress, "PU");
			SetupOrgAddressForTest(dAddress, "D");

			var order = Factory.New<TrackingOrder>();
			order.GoodsAvailableAtAddress.E2_OA_Address = pUAddress.PK;
			order.GoodsDeliveredToAddress.E2_OA_Address = dAddress.PK;

			AssertEquals("PUCOMPANYNAME PUADD1 PUADD2 PUCITY PUSTATE", order.PickupAddressAsString);
			AssertEquals("DCOMPANYNAME DADD1 DADD2 DCITY DSTATE", order.DeliveryAddressAsString);
		}

		public void TestPickupDeliveryAddressAsString_Caching()
		{
			// If SupplierPK/BuyerPK changes, address should be reloaded because 2 orgs could have addresses w/ the same code.
			var pUAddress = Factory.NewWithValidTestData<OrgAddress>();
			var dAddress = Factory.NewWithValidTestData<OrgAddress>();
			var pUOtherAddress = Factory.NewWithValidTestData<OrgAddress>();
			var dOtherAddress = Factory.NewWithValidTestData<OrgAddress>();
			SetupOrgAddressForTest(pUAddress, "PU");
			SetupOrgAddressForTest(dAddress, "D");
			SetupOrgAddressForTest(pUOtherAddress, "PUOTHER");
			SetupOrgAddressForTest(dOtherAddress, "DOTHER");
			pUOtherAddress.OA_Code = "PUcode";
			dOtherAddress.OA_Code = "Dcode";

			var order = Factory.New<TrackingOrder>();
			order.SupplierPK = pUAddress.OA_OH;
			order.BuyerPK = dAddress.OA_OH;
			order.GoodsAvailableAtAddress.E2_OA_Address = pUAddress.PK;
			order.GoodsDeliveredToAddress.E2_OA_Address = dAddress.PK;

			AssertEquals("PUCOMPANYNAME PUADD1 PUADD2 PUCITY PUSTATE", order.PickupAddressAsString);
			AssertEquals("DCOMPANYNAME DADD1 DADD2 DCITY DSTATE", order.DeliveryAddressAsString);

			order.SupplierPK = pUOtherAddress.OA_OH;
			order.BuyerPK = dOtherAddress.OA_OH;

			AssertEquals("HEADER #2", order.PickupAddressAsString);
			AssertEquals("HEADER #2", order.DeliveryAddressAsString);
		}

		public void TestPickupDeliveryAddressAsString()
		{
			TrackingOrder order = Factory.New<TrackingOrder>();
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			order.JD_JS = shipment.PK;

			AssertPickupDeliveryAddressAsString("Pickup Address", shipment.ConsignorDocumentaryAddress, order, "PickupAddressAsString");
			AssertPickupDeliveryAddressAsString("Delivery Address", shipment.ConsigneeDocumentaryAddress, order, "DeliveryAddressAsString");
		}

		void AssertPickupDeliveryAddressAsString(string message, JobDocAddress jobDocAddress, TrackingOrder order, ZString orderPropertyName)
		{
			jobDocAddress.E2_CompanyName = "Company";
			jobDocAddress.E2_Address1 = "Address1";
			jobDocAddress.E2_State = "State";
			jobDocAddress.E2_City = "City";

			AddressFormatter formatter = new AddressFormatter(jobDocAddress.Factory, jobDocAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(message, ArrayToTextConverter.ConvertToCommaSeparatedMultilineText("Company", formatter.PostalAddressAsASingleLineWithoutCompanyName()), order[orderPropertyName]);
		}

		#endregion

		#region TestConsolsAsString

		public void TestConsolsAsString()
		{
			TrackingOrder order = Factory.New<TrackingOrder>();
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			order.JD_JS = shipment.PK;

			shipment.Consols.AddNew().JK_UniqueConsignRef = "Console 1";
			shipment.Consols.AddNew().JK_UniqueConsignRef = "Console 2";

			AssertEquals(ArrayToTextConverter.ConvertToCommaSeparatedMultilineText("Console 1", "Console 2"), order.ConsolsAsString);
		}

		#endregion

		#region TestCreatedOn

		public void TestCreatedOn()
		{
			TrackingOrder order = Factory.New<TrackingOrder>();

			AssertEquals("Not created on-line", "Internal", order.CreatedOn);
			order.JD_SystemCreateUser = "ZZ";
			AssertEquals("Created on-line", "Web", order.CreatedOn);
		}

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			TrackingOrder order = Factory.New<TrackingOrder>();

			Assert("Service level is empty if no shipment is attached", order.ServiceLevel.IsEmpty);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			shipment.JS_RS_NKServiceLevel = "D2D";
			order.JD_JS = shipment.PK;

			AssertEquals("Service level is not empty if a shipment is attached", shipment.JS_RS_NKServiceLevel, order.ServiceLevel);
		}

		#endregion

		#region TestDeliverAddress_List

		public void TestDeliverAddress_List()
		{
			var testOrg = Factory.New<OrgHeader>();

			var deliverAddress1 = testOrg.Addresses.AddNew();
			deliverAddress1.OA_Code = "dlv1";
			deliverAddress1.OA_IsActive = true;

			var deliverAddress2 = testOrg.Addresses.AddNew();
			deliverAddress2.OA_Code = "dlv2";
			deliverAddress2.OA_IsActive = true;

			var deliverAddress3 = testOrg.Addresses.AddNew();
			deliverAddress3.OA_Code = "dlv3";
			deliverAddress3.OA_IsActive = false;

			var deliverAddress4 = testOrg.Addresses.AddNew();
			deliverAddress4.OA_Code = "dlv4";
			deliverAddress4.OA_IsActive = false;

			var deliverAddress5 = testOrg.Addresses.AddNew();
			deliverAddress5.OA_Code = "dlv5";
			deliverAddress5.OA_IsActive = false;

			OrgAddressDependentCollection addresses;

			var order = Factory.New<TrackingOrder>();
			addresses = order.DeliverAddress_List;

			AssertEquals("No addresses", 0, addresses.Count);

			order.GoodsDeliveredToAddress.E2_OA_Address = deliverAddress1.PK;

			addresses = order.DeliverAddress_List;
			// NB: The OrgHeader always has a Postal Address associated with it.
			AssertEquals("3 active addresses", 3, addresses.Count);

			order.GoodsDeliveredToAddress.E2_AddressOverride = true;
			order.GoodsDeliveredToAddress.E2_CompanyName = "Test Company";
			order.GoodsDeliveredToAddress.E2_Address1 = "Test Address";

			addresses = order.DeliverAddress_List;
			AssertEquals("1 address", 1, addresses.Count);
		}

		#endregion

		#region TestPickupAddress_List

		public void TestPickupAddress_List()
		{
			var testOrg = Factory.New<OrgHeader>();
			var pickupAddress = testOrg.Addresses.AddNew();
			pickupAddress.OA_Code = "pku";
			pickupAddress.OA_IsActive = true;

			var pickupAddress2 = testOrg.Addresses.AddNew();
			pickupAddress2.OA_Code = "pku2";
			pickupAddress2.OA_IsActive = true;

			var pickupAddress3 = testOrg.Addresses.AddNew();
			pickupAddress3.OA_Code = "pku3";
			pickupAddress3.OA_IsActive = false;

			var pickupAddress4 = testOrg.Addresses.AddNew();
			pickupAddress4.OA_Code = "pku4";
			pickupAddress4.OA_IsActive = false;

			var pickupAddress5 = testOrg.Addresses.AddNew();
			pickupAddress5.OA_Code = "pku5";
			pickupAddress5.OA_IsActive = false;

			OrgAddressDependentCollection addresses;

			var order = Factory.New<TrackingOrder>();
			addresses = order.PickupAddress_List;
			addresses.Load();

			AssertEquals("No addresses", 0, addresses.Count);

			order.GoodsAvailableAtAddress.E2_OA_Address = pickupAddress.PK;

			addresses = order.PickupAddress_List;
			addresses.Load();
			// NB: The OrgHeader always has a Postal Address associated with it.
			AssertEquals("3 active addresses", 3, addresses.Count);

			order.GoodsAvailableAtAddress.E2_AddressOverride = true;
			order.GoodsAvailableAtAddress.E2_CompanyName = "Test Company";
			order.GoodsAvailableAtAddress.E2_Address1 = "Test Address";

			addresses = order.PickupAddress_List;
			AssertEquals("1 address", 1, addresses.Count);
		}

		#endregion
	}
}
