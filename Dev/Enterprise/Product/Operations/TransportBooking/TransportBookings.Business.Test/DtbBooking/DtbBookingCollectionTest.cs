using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using static Enterprise.ZArchitecture.Business.ModuleTextFilter.ComparisonConstants;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business.Testing
{
	public abstract class DtbBookingCollectionTest<TBookingCollection> : ActiveBusinessObjectCollectionTestCase<TBookingCollection>
		where TBookingCollection : DtbBookingCollection
	{
		public void TestAllowNew()
		{
			var consolidation = Helper.CreateConsolidation();

			AssertEquals(true, ((IBindingList)new DtbBookingCollection(Factory)).AllowNew);
			AssertEquals(true, ((IBindingList)new DtbBookingCollection(consolidation)).AllowNew);

			var collection = new DtbBookingCollection(Factory);
			var collectionAsList = (IBindingList)collection;

			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.SingleJob);
			collection.AllowNewCore = true;
			AssertEquals(true, collectionAsList.AllowNew);

			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.MultiJob);
			AssertEquals(false, collection.ReadOnly);

			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.SingleJob);
			collection.AllowNewCore = false;
			AssertEquals(false, collectionAsList.AllowNew);
		}

		public void TestAllowNewForMasterBooking()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_IsMaster = true;
			var collection = new DtbBookingCollectionForTest(consolidation);

			AssertEquals("AllowNew should be false when booking is master", false, collection.AllowNewExposed);
		}

		public void TestAllowNewForNonMasterBooking()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_IsMaster = false;
			var collection = new DtbBookingCollectionForTest(consolidation);

			AssertEquals("AllowNew should be true when booking is not master", true, collection.AllowNewExposed);
		}

		public void TestIsAnyBookingHeld()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.IsAnyBookingHeld);

			var booking1 = AddNewBooking(collection);
			var booking2 = AddNewBooking(collection);

			booking1.KM_Status = TransportStatuses.Codes.Available;
			booking2.KM_Status = TransportStatuses.Codes.Available;
			AssertEquals(false, collection.IsAnyBookingHeld);

			booking1.KM_Status = TransportStatuses.Codes.Held;
			booking2.KM_Status = TransportStatuses.Codes.Available;
			AssertEquals(true, collection.IsAnyBookingHeld);
		}

		public void TestIsDelivered()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.IsDelivered);

			var booking1 = AddNewBooking(collection);
			var booking2 = AddNewBooking(collection);

			booking1.KM_Status = TransportStatuses.Codes.Available;
			booking2.KM_Status = TransportStatuses.Codes.Delivered;
			AssertEquals(false, collection.IsDelivered);

			booking1.KM_Status = TransportStatuses.Codes.Delivered;
			booking2.KM_Status = TransportStatuses.Codes.Delivered;
			AssertEquals(true, collection.IsDelivered);
		}

		public void TestIsPickedUp()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.IsPickedUp);

			var booking1 = AddNewBooking(collection);
			var booking2 = AddNewBooking(collection);

			booking1.KM_Status = TransportStatuses.Codes.Available;
			booking2.KM_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals(false, collection.IsPickedUp);

			booking1.KM_Status = TransportStatuses.Codes.PickedUp;
			booking2.KM_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals(true, collection.IsPickedUp);

			booking1.KM_Status = TransportStatuses.Codes.PickedUp;
			booking2.KM_Status = TransportStatuses.Codes.Delivered;
			AssertEquals("If instructions are either picked up or delivered, IsPickedUp should return true.", true, collection.IsPickedUp);
		}

		public void TestIsServiceCommenced()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.IsServiceCommenced);

			var booking1 = AddNewBooking(collection);
			var booking2 = AddNewBooking(collection);

			booking1.KM_Status = TransportStatuses.Codes.Available;
			booking2.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			AssertEquals(false, collection.IsServiceCommenced);

			booking1.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			booking2.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			AssertEquals(true, collection.IsServiceCommenced);
		}

		public void TestIsActionRequired()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.IsActionRequired);

			var booking1 = AddNewBooking(collection);
			var booking2 = AddNewBooking(collection);

			booking1.KM_Status = TransportStatuses.Codes.Available;
			booking2.KM_Status = TransportStatuses.Codes.ActionRequired;
			AssertEquals(false, collection.IsActionRequired);

			booking1.KM_Status = TransportStatuses.Codes.ActionRequired;
			booking2.KM_Status = TransportStatuses.Codes.ActionRequired;
			AssertEquals(true, collection.IsActionRequired);
		}

		public void TestCollectionUsingConsolConstructorDoesNotNeedFactorySave()
		{
			var bookingConsolidation = Helper.CreateConsolidation();
			bookingConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			var booking = bookingConsolidation.Bookings.AddNew();

			var collection = new DtbBookingCollection(bookingConsolidation);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { booking }, collection);
		}

		public void TestRelationship()
		{
			// single job consolidation

			var consolidationSingleJob = Helper.CreateConsolidation();
			var collectionSingleJob = new DtbBookingCollection(consolidationSingleJob);

			var bookingSingleJob = collectionSingleJob.AddNew();
			AssertEquals(consolidationSingleJob.PK, bookingSingleJob.KM_KB_Booking);
			AssertEquals(true, bookingSingleJob.KM_KB_BookingConsolidationMultiJob.IsEmpty);

			// multi job consolidation

			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();
			var collectionMultiJob = new DtbBookingCollection(consolidationMultiJob);

			var bookingMultiJob = collectionMultiJob.AddNew();
			AssertEquals(consolidationMultiJob.PK, bookingMultiJob.KM_KB_BookingConsolidationMultiJob);
			AssertEquals(true, bookingMultiJob.KM_KB_Booking.IsEmpty);
		}

		public void TestCollectionIncludesOnlyCorrectTypesOfJobs()
		{
			var bookingConsolidation = (DtbBookingConsolidation)Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			var booking = bookingConsolidation.Bookings.AddNew();

			var hvlvBookingConsolidation = (DtbBookingConsolidation)Factory.New<IDtbBookingConsolidation>();
			hvlvBookingConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.HighVolumeLowValue;
			var hvlvBooking = hvlvBookingConsolidation.Bookings.AddNew();

			var consignmentConsolidation = (DtbTransportConsolidation)Factory.New<IDtbConsignmentConsolidation>();
			var consignment = consignmentConsolidation.Bookings.AddNew();
			Factory.Save();

			var collection = base.GetCollectionToTest();
			var expected = new[] { booking, hvlvBooking, consignment }.Where(o => GetParentType_ForTesting() == ((Common.AutoDtbBooking)o).KM_JobType);
			AssertContainsExactElementsInAnyOrder(expected, collection);
		}

		public void TestDtbBookingCollection()
		{
			var booking = GetBookingToAddToTheCollection();
			Factory.Save();

			var collection = GetCollectionToTest();
			if (((IBindingList)collection).AllowNew)
			{
				collection.Add(booking);
			}

			AssertContainsExactElementsInAnyOrder(new[] { booking }, collection);
			AssertContainsExactElementsInAnyOrder(new[] { booking }, collection.Typed);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var booking = GetBookingToAddToTheCollection();
			Factory.Save(); // Consolidation + Booking must be in database
			return booking;
		}

		protected virtual DtbBooking AddNewBooking(DtbBookingCollection collection)
		{
			return collection.AddNew();
		}

		DtbBooking GetBookingToAddToTheCollection()
		{
			return Helper.CreateBooking();
		}

		string GetParentType_ForTesting()
		{
			return TransportConsolidationJobTypes.Codes.Booking;
		}

		class DtbBookingCollectionForTest : DtbBookingCollection
		{
			public DtbBookingCollectionForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public DtbBookingCollectionForTest(DtbBookingConsolidation consolidation) : base(consolidation)
			{
			}

			public DtbBookingCollectionForTest(DtbBooking masterBooking) : base(masterBooking)
			{
			}

			public DtbBookingCollectionForTest(BusinessObjectFactory factory, ICollectionRelationship relationship) : base(factory, relationship)
			{
			}

			public DtbBookingCollectionForTest(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
			{
			}

			public bool AllowNewExposed => AllowNew;
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}

	[TestedType(typeof(DtbBookingCollection))]
	public class DtbBookingCollectionTest : DtbBookingCollectionTest<DtbBookingCollection>
	{
		protected override DtbBookingCollection GetCollectionToTest()
		{
			var consolidation = Helper.CreateConsolidation();
			return new DtbBookingCollection(consolidation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;
		}

		protected override void TearDown()
		{
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
			base.TearDown();
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;
	}

	[TestedType(typeof(DtbBookingCollectionForFindBox))]
	public class DtbBookingCollectionForFindBoxTest : DtbBookingCollectionTest<DtbBookingCollectionForFindBox>
	{
		public void TestFilterBusinessObjectDefaults()
		{
			// consolidation with transport co

			var transportCo = Helper.CreateOrganisation("ABC");
			var collection = new DtbBookingCollectionForFindBox(Factory, transportCo);
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.TransportCompany + ":Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingConsolidated + ":Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingStatus + ":Property"));
			AssertSubBookingFiltersHaveNotBeenAdded(collection);

			var filterDefault = collection.FilterBusinessObjectDefaults[FilterNameConstants.TransportCompany + ":Property"];
			AssertEquals("Property", filterDefault.PropertyName);
			AssertEquals(transportCo.PK, filterDefault.Value);

			// consolidation with no transport co

			var collectionWithNoTransportCo = new DtbBookingCollectionForFindBox(Factory);
			var filterDefaultWithNoTransportCo = collectionWithNoTransportCo.FilterBusinessObjectDefaults[FilterNameConstants.TransportCompany + ":Property"];
			AssertEquals("Property", filterDefaultWithNoTransportCo.PropertyName);
			AssertEquals(ZGuid.Empty, filterDefaultWithNoTransportCo.Value);
			AssertSubBookingFiltersHaveNotBeenAdded(collectionWithNoTransportCo);
		}

		public void TestAdditionalFilter()
		{
			var transportCoABC = Helper.CreateOrganisation("ABC");
			var transportCoXYZ = Helper.CreateOrganisation("XYZ");
			var bookingABC = Helper.CreateBooking(transportCoABC);
			var bookingXYZ = Helper.CreateBooking(transportCoXYZ);
			var booking123 = Helper.CreateBooking(transportCoXYZ);
			booking123.BillingPartyAddress.OrganisationPK = transportCoABC.PK;

			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			consol[JobConsolSchema.JK_RL_NKLoadPort] = "AUBNE";
			consol[JobConsolSchema.JK_RL_NKDischargePort] = "USLAX";
			consol[JobConsolSchema.JK_IsCancelled] = false;
			consol[JobConsolSchema.JK_IsForwarding] = true;
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)consol);
			var booking456 = Helper.CreateBooking(consolidation);
			booking456.Address.OrganisationPK = transportCoABC.PK;

			Factory.Save();

			// consolidation with transport co ABC
			var collection = new DtbBookingCollectionForFindBox(Factory, transportCoABC);
			AssertContainsExactElementsInAnyOrder("Should only contain bookings for transport company ABC, including parent type CON", new DtbBooking[] { bookingABC, booking456 }, collection);

			// consolidation with no transport co
			var collectionWithNoTransportCo = new DtbBookingCollectionForFindBox(Factory);
			AssertContainsExactElementsInAnyOrder("Should contain bookings for any transport company, including parent type CON", new DtbBooking[] { bookingABC, bookingXYZ, booking123, booking456 }, collectionWithNoTransportCo);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var transportCoABC = Helper.CreateOrganisation("ABC");
			var transportCoXYZ = Helper.CreateOrganisation("XYZ");
			var bookingABC = Helper.CreateBooking(transportCoABC);
			var bookingXYZ = Helper.CreateBooking(transportCoXYZ);
			Factory.Save();

			var collectionInternals = (IActiveBusinessObjectCollection)new DtbBookingCollectionForFindBox(Factory, transportCoABC);
			AssertEquals(true, collectionInternals.GetAllNotificationsWhenAdditionalFilterNotMet(bookingABC).Contains("Please choose another Transport Booking"));
			AssertEquals("This Booking does not have the same Transport Company as the Consolidation (ABC).", collectionInternals.GetAllNotificationsWhenAdditionalFilterNotMet(bookingXYZ));
		}

		void AssertSubBookingFiltersHaveNotBeenAdded(DtbBookingCollectionForFindBox collection)
		{
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.CarrierBookingAgent + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingConsolidationTemplate + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingDirection + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingConsolidationJobDirection + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingIsMaster + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.ConsolidatedBookingID + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingIsSub + ":Property"));
		}

		protected override DtbBooking AddNewBooking(DtbBookingCollection collection)
		{
			return (DtbBooking)GetNewElementToAddToTheCollection();
		}

		protected override DtbBookingCollectionForFindBox GetCollectionToTest()
		{
			return new DtbBookingCollectionForFindBox(Factory);
		}
	}

	[TestedType(typeof(DtbBookingCollectionForSubBookingsFindBox))]
	public class DtbBookingCollectionForSubBookingsFindBoxTest : ActiveBusinessObjectCollectionTestCase<DtbBookingCollectionForSubBookingsFindBox>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var booking = Helper.CreateBooking();
			booking.KM_JobType = "BKG";
			booking.KM_IsMaster = false;
			booking.KM_Status = "AVL";
			booking.KM_IsAgentBooking = false;

			Factory.Save();

			return booking;
		}

		public void TestFilterBusinessObjectDefaults_WithoutMasterBooking()
		{
			var transportCo = Helper.CreateOrganisation("ABC");
			var transportCoAddress = Helper.CreateOrgAddress(Factory, transportCo, "ABC");
			Factory.Save();
			var collection = new DtbBookingCollectionForSubBookingsFindBox(Factory, transportCoAddress, false);
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingStatus + ":Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.TransportCompany + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingIsMaster + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingIsSub + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.CarrierBookingAgent + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingConsolidationTemplate + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingConsolidationJobDirection + ":Property"));
			Assert(!collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingTransportMode + ":Property"));

			var filterDefault = collection.FilterBusinessObjectDefaults[FilterNameConstants.TransportCompany + ":Property"];
			AssertEquals(transportCoAddress.Header.PK, filterDefault.Value);
		}

		public void TestFilterBusinessObjectDefaults_WithMasterBookingAndCarrierBookingAgent()
		{
			TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var orgABC = Helper.CreateOrganisation("ABC");
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			var orgAddressABC = Helper.CreateOrgAddress(Factory, orgABC, "ABC");
			masterBooking.CarrierBookingAgentDocAddress.E2_OA_Address = orgAddressABC.PK;
			Factory.Save();
			var collection = new DtbBookingCollectionForSubBookingsFindBox(masterBooking, false);
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingStatus + ":Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingIsMaster + ":Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingIsSub + ":Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.CarrierBookingAgent + ":Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingConsolidationTemplate + ":Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingConsolidationJobDirection + ":Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor(FilterNameConstants.BookingTransportMode + ":Property"));

			var filterDefault = collection.FilterBusinessObjectDefaults[FilterNameConstants.TransportCompany + ":ComparisonOperator"];
			AssertEquals((ZString)IsBlank, filterDefault.Value);
			filterDefault = collection.FilterBusinessObjectDefaults[FilterNameConstants.CarrierBookingAgent + ":Property"];
			AssertEquals(masterBooking.CarrierBookingAgentDocAddress.Address.Header.PK, filterDefault.Value);
		}

		public void TestAdditionalFilter_KM_IsMaster()
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = true;
			Factory.Save();

			var collection = GetCollectionForAttachingConsolidationsNotSupported();
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should not match booking", Array.Empty<DtbBooking>(), collection);
			AssertContains("Should provide reason why booking is not matched", "Booking is a Master Booking.\r\nThis Booking has a Master Version greater than zero.", iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(booking));
		}

		public void TestAdditionalFilter_KM_KM_MasterBooking()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			var subBooking = Helper.CreateBooking();
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			Factory.Save();

			var collection = GetCollectionForAttachingSubsToMaster(masterBooking, false);
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should not match either sub booking or master booking", Array.Empty<DtbBooking>(), collection);
			AssertContains("Should provide reason why sub booking is not matched", "This Booking is already attached to a Master Booking.\r\nThis Booking has a Master Version greater than zero.", iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(subBooking));
		}

		public void TestAdditionalFilter_KM_MasterBookingVersion()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 43;

			var subBooking = Helper.CreateBooking();
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBooking.KM_MasterBookingVersion = 42;
			Factory.Save();

			var collection = GetCollectionForAttachingSubsToMaster(masterBooking, false);
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should not match either sub booking or master booking", Array.Empty<DtbBooking>(), collection);
			AssertContains("Should provide reason why sub booking is not matched", "This Booking is already attached to a Master Booking.\r\nThis Booking has a Master Version greater than zero.", iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(subBooking));
		}

		public void TestAdditionalFilter_MultiJob()
		{
			// single job consolidation

			var consolidationSingleJob = Helper.CreateConsolidation();
			var collectionSingleJob = new DtbBookingCollection(consolidationSingleJob);

			var bookingSingleJob = collectionSingleJob.AddNew();
			AssertEquals("Precondition", false, consolidationSingleJob.IsMultiBooking);
			AssertEquals("Precondition", consolidationSingleJob.PK, bookingSingleJob.KM_KB_Booking);
			AssertEquals("Precondition", true, bookingSingleJob.KM_KB_BookingConsolidationMultiJob.IsEmpty);

			// multi job consolidation

			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();
			var collectionMultiJob = new DtbBookingCollection(consolidationMultiJob);

			var bookingMultiJob = Helper.CreateBooking();
			collectionMultiJob.Add(bookingMultiJob);
			AssertEquals("Precondition", true, consolidationMultiJob.IsMultiBooking);
			AssertEquals("Precondition", consolidationMultiJob.PK, bookingMultiJob.KM_KB_BookingConsolidationMultiJob);
			AssertEquals("Precondition: Required to save record", false, bookingMultiJob.KM_KB_Booking.IsEmpty);

			// Not tested: Scenario where KM_KB_BookingConsolidationMultiJob is populated with a consolidation where IsMultiBooking is false

			Factory.Save();

			var collection = GetCollectionForAttachingConsolidationsNotSupported();
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should exclude the booking with a multijob", new DtbBooking[] { bookingSingleJob }, collection);
			AssertContains("Should provide reason why booking is not matched", "This Booking is part of a Consolidated Booking.", iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(bookingMultiJob));
		}

		public void TestAdditionalFilter_KM_Status()
		{
			var bookings = new List<DtbBooking>();
			foreach (var code in new TransportStatuses().GetAllCodes())
			{
				if (code == TransportStatuses.Codes.Available)
				{
					continue;
				}
				var booking = Helper.CreateBooking();
				booking.KM_Status = code;

				bookings.Add(booking);
			}
			var availableBooking = Helper.CreateBooking();
			Factory.Save();

			var collection = GetCollectionForAttachingConsolidationsNotSupported();
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should exclude bookings which do not have a status of available", new DtbBooking[] { availableBooking }, collection);
			foreach (var nonavailableBooking in bookings)
			{
				AssertContains("Should provide reason why booking is not matched", "This Booking does not have a status of Available.", iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(nonavailableBooking));
			}
		}

		public void TestAdditionalFilter_KM_IsAgentBooking()
		{
			var agentBooking = Helper.CreateBooking();
			agentBooking.KM_IsAgentBooking = true;

			var nonAgentBooking = Helper.CreateBooking();
			nonAgentBooking.KM_IsAgentBooking = false;

			Factory.Save();

			var collection = GetCollectionForAttachingConsolidationsNotSupported();
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should exclude the agent booking", new DtbBooking[] { nonAgentBooking }, collection);
			AssertContains("Should provide reason why booking is not matched", "This Booking is an Agent Booking.", iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(agentBooking));
		}

		public void TestAdditionalFilter_KM_TransportMode()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_TransportMode = "ROA";

			var matchingBooking = Helper.CreateBooking();
			matchingBooking.KM_TransportMode = "ROA";

			var nonMatchingBooking = Helper.CreateBooking();
			nonMatchingBooking.KM_TransportMode = "RAI";

			Factory.Save();

			var collection = GetCollectionForAttachingSubsToMaster(masterBooking, false);
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should only match bookings with the same booking transport mode.", new DtbBooking[] { matchingBooking }, collection);
			AssertContains("Should provide reason why booking is not matched", "This Booking has a different booking transport mode to the Master Booking.", iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(nonMatchingBooking));
		}

		public void TestAdditionalFilter_KM_KT_NKBookingTemplateWhenNotBlank()
		{
			TestAdditionalFilterForBookingTemplateCore("PLCW", "LC2C");
		}

		public void TestAdditionalFilter_KM_KT_NKBookingTemplateWhenBlank()
		{
			TestAdditionalFilterForBookingTemplateCore("", "LC2C");
		}

		void TestAdditionalFilterForBookingTemplateCore(string masterTemplate, string nonMatchingTemplate)
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_KT_NKBookingTemplate = masterTemplate;

			var matchingBooking = Helper.CreateBooking();
			matchingBooking.KM_KT_NKBookingTemplate = masterTemplate;

			var nonMatchingBooking = Helper.CreateBooking();
			nonMatchingBooking.KM_KT_NKBookingTemplate = nonMatchingTemplate;

			Factory.Save();

			var collection = GetCollectionForAttachingSubsToMaster(masterBooking, false);
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should only match bookings with the same booking template.", new DtbBooking[] { matchingBooking }, collection);
			AssertContains("Should provide reason why booking is not matched", "This Booking has a different template to the Master Booking.", iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(nonMatchingBooking));
		}

		public void TestAdditionalFilter_KM_DirectionWhenNotBlank()
		{
			TestAdditionalFilterForBookingDirectionCore("PIC", "DLV");
		}

		public void TestAdditionalFilter_KM_DirectionWhenBlank()
		{
			TestAdditionalFilterForBookingDirectionCore("", "DLV");
		}

		void TestAdditionalFilterForBookingDirectionCore(string masterDirection, string nonMatchingDirection)
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_Direction = masterDirection;

			var matchingBooking = Helper.CreateBooking();
			matchingBooking.KM_Direction = masterDirection;

			var nonMatchingBooking = Helper.CreateBooking();
			nonMatchingBooking.KM_Direction = nonMatchingDirection;

			Factory.Save();

			var collection = GetCollectionForAttachingSubsToMaster(masterBooking, false);
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should only match bookings with the same KM_Direction.", new DtbBooking[] { matchingBooking }, collection);
			AssertContains("Should provide reason why booking is not matched", "This Booking has a different booking direction to the Master Booking.", iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(nonMatchingBooking));
		}

		public void TestAdditionalFilter_ConsolidationKB_JobDirectionWhenNotBlank()
		{
			TestAdditionalFilterForBookingConsolidationDirectionCore("PIC", "DLV");
		}

		public void TestAdditionalFilter_ConsolidationKB_JobDirectionWhenBlank()
		{
			TestAdditionalFilterForBookingConsolidationDirectionCore("", "DLV");
		}

		void TestAdditionalFilterForBookingConsolidationDirectionCore(string masterDirection, string nonMatchingDirection)
		{
			var masterConsolidation = Helper.CreateConsolidation();
			masterConsolidation.KB_JobDirection = masterDirection;
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;

			var matchingConsolidation = Helper.CreateConsolidation();
			matchingConsolidation.KB_JobDirection = masterDirection;
			var matchingBooking = Helper.CreateBooking(matchingConsolidation);

			var nonMatchingConsolidation = Helper.CreateConsolidation();
			nonMatchingConsolidation.KB_JobDirection = nonMatchingDirection;
			var nonMatchingBooking = Helper.CreateBooking(nonMatchingConsolidation);

			Factory.Save();

			var collection = GetCollectionForAttachingSubsToMaster(masterBooking, false);
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should only match bookings whose consolidations have the same KB_JobDirection.", new DtbBooking[] { matchingBooking }, collection);
			AssertContains("Should provide reason why booking is not matched", "This Booking's consolidation has a different job direction from that of the Master Booking's consolidation.", iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(nonMatchingBooking));
		}

		public void TestAdditionalFilter_CarrierBookingAgentForBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnCarrierBookingAgentAddress = delegate(DtbBooking booking) { return booking.CarrierBookingAgentDocAddress; };
			TestAdditionalFilterMasterAttachmentUsingOrgsForBlanksCore(returnCarrierBookingAgentAddress, "Carrier Booking Agent", "This Booking has a different Carrier Booking Agent from the Master Booking ().");
		}

		public void TestAdditionalFilter_CarrierBookingAgentForNonBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnCarrierBookingAgentAddress = delegate(DtbBooking booking) { return booking.CarrierBookingAgentDocAddress; };
			TestAdditionalFilterMasterAttachmentUsingOrgsForNonBlanksCore(returnCarrierBookingAgentAddress, "Carrier Booking Agent", "This Booking has a different Carrier Booking Agent from the Master Booking (ABC).");
		}

		public void TestAdditionalFilter_TransportCompanyForBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnTransportCoAddress = delegate(DtbBooking booking) { return booking.Address; };
			TestAdditionalFilterMasterAttachmentUsingOrgsForBlanksCore(returnTransportCoAddress, "Transport Company", "This Booking has a different Transport Company from the Master Booking ().");
		}

		public void TestAdditionalFilter_TransportCompanyForNonBlanks()
		{
			Func<DtbBooking, JobDocAddress> returnTransportCoAddress = delegate(DtbBooking booking) { return booking.Address; };
			TestAdditionalFilterMasterAttachmentUsingOrgsForNonBlanksCore(returnTransportCoAddress, "Transport Company", "This Booking has a different Transport Company from the Master Booking (ABC).");
		}

		void TestAdditionalFilterMasterAttachmentUsingOrgsForBlanksCore(Func<DtbBooking, JobDocAddress> getInstructionType, string testItem, string warningMessage)
		{
			var orgABC = Helper.CreateOrganisation("ABC");
			var orgAddressABC = Helper.CreateOrgAddress(Factory, orgABC, "ABC");

			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			var nonBlankBooking = Helper.CreateBooking();

			getInstructionType(nonBlankBooking).E2_OA_Address = orgAddressABC.PK;

			var blankBooking = Helper.CreateBooking();

			Factory.Save();

			var collection = GetCollectionForAttachingSubsToMaster(masterBooking, false);
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should only match bookings with the same " + testItem + " address.", new DtbBooking[] { blankBooking }, collection);
			AssertContains("Should provide reason why booking is not matched", warningMessage, iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(nonBlankBooking));
		}

		void TestAdditionalFilterMasterAttachmentUsingOrgsForNonBlanksCore(Func<DtbBooking, JobDocAddress> getInstructionType, string testItem, string warningMessage)
		{
			var orgABC = Helper.CreateOrganisation("ABC");
			var orgAddressABC = Helper.CreateOrgAddress(Factory, orgABC, "ABC");
			var orgAddressABC2 = Helper.CreateOrgAddress(Factory, orgABC, "ABD");
			var orgDEF = Helper.CreateOrganisation("DEF");
			var orgAddressDEF = Helper.CreateOrgAddress(Factory, orgDEF, "DEF");

			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;

			var matchingBooking = Helper.CreateBooking();
			var nonMatchingBooking = Helper.CreateBooking();
			var matchingOrgHeaderNonMatchingAddress = Helper.CreateBooking();
			var blankBooking = Helper.CreateBooking();

			getInstructionType(masterBooking).E2_OA_Address = orgAddressABC.PK;
			getInstructionType(matchingBooking).E2_OA_Address = orgAddressABC.PK;
			getInstructionType(matchingOrgHeaderNonMatchingAddress).E2_OA_Address = orgAddressABC2.PK;
			getInstructionType(nonMatchingBooking).E2_OA_Address = orgAddressDEF.PK;

			Factory.Save();

			var collection = GetCollectionForAttachingSubsToMaster(masterBooking, false);
			var iActiveBusinessObjectCollection = (IActiveBusinessObjectCollection)collection;

			AssertContainsExactElementsInAnyOrder("Should only match bookings with the same " + testItem + " address.", new DtbBooking[] { matchingBooking }, collection);
			AssertContains("Should provide reason why booking is not matched", warningMessage, iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(nonMatchingBooking));
			AssertContains("Should provide reason why booking is not matched", warningMessage, iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(blankBooking));
			AssertContains("Should provide reason why booking is not matched", warningMessage, iActiveBusinessObjectCollection.GetAllNotificationsWhenAdditionalFilterNotMet(matchingOrgHeaderNonMatchingAddress));
		}

		public void TestAdditionalFilter_IsExtraValidationSuspendedConditionIgnoresExtraValidation()
		{
			var orgABC = Helper.CreateOrganisation("ABC");
			var orgAddressABC = Helper.CreateOrgAddress(Factory, orgABC, "ABC");
			var orgDEF = Helper.CreateOrganisation("DEF");
			var orgAddressDEF = Helper.CreateOrgAddress(Factory, orgDEF, "DEF");

			var masterBookingConsolidation = Helper.CreateConsolidation();
			masterBookingConsolidation.KB_JobDirection = "DST";
			var masterBooking = Helper.CreateBooking(masterBookingConsolidation);
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			masterBooking.KM_KT_NKBookingTemplate = "IFDR";
			masterBooking.KM_Direction = "DST";

			var nonMatchingConsolidation = Helper.CreateConsolidation();
			nonMatchingConsolidation.KB_JobDirection = "LOC";
			var nonMatchingBooking = Helper.CreateBooking(nonMatchingConsolidation);
			nonMatchingBooking.KM_KT_NKBookingTemplate = "LC2C";
			nonMatchingBooking.KM_Direction = "LOC";

			masterBooking.CarrierBookingAgentDocAddress.E2_OA_Address = orgAddressABC.PK;
			nonMatchingBooking.CarrierBookingAgentDocAddress.E2_OA_Address = orgAddressDEF.PK;

			Factory.Save();

			var collection = GetCollectionForAttachingSubsToMaster(masterBooking, true);

			AssertContainsExactElementsInAnyOrder("Should match the booking in this case, as validation for the template, direction and carrier booking agent is suspended.", new DtbBooking[] { nonMatchingBooking }, collection);
		}

		DtbBookingCollectionForSubBookingsFindBox GetCollectionForAttachingSubsToMaster(DtbBooking masterBooking, bool isExtraValidationSuspended)
		{
			return new DtbBookingCollectionForSubBookingsFindBox(masterBooking, isExtraValidationSuspended);
		}

		DtbBookingCollectionForSubBookingsFindBox GetCollectionForAttachingConsolidationsNotSupported()
		{
			return new DtbBookingCollectionForSubBookingsFindBox(Factory, null, false);
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		DtbBooking masterBooking_Field;

		protected override DtbBookingCollectionForSubBookingsFindBox GetCollectionToTest()
		{
			masterBooking_Field = Helper.CreateBooking();
			masterBooking_Field.KM_IsMaster = true;
			return new DtbBookingCollectionForSubBookingsFindBox(masterBooking_Field, false);
		}
	}
}
