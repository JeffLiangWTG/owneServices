using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(CombineBookings))]
	public class CombineBookingsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCombineContainers()
		{
			var containerRef1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			var containerRef2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var container1 = MasterBooking.QuotedBookingContainers.AddNew();
			container1.JC_ContainerCount = 3;
			container1.JC_ContainerNum = "CONTAINER1";
			container1.JC_RC = containerRef1.PK;

			var container2 = MasterBooking.QuotedBookingContainers.AddNew();
			container2.JC_ContainerCount = 4;
			container2.JC_RC = containerRef2.PK;

			var container3 = OtherBooking1.QuotedBookingContainers.AddNew();
			container3.JC_ContainerNum = "CONTAINER1";
			container3.JC_ContainerCount = 7;
			container3.JC_RC = containerRef1.PK;

			var container4 = OtherBooking1.QuotedBookingContainers.AddNew();
			container4.JC_ContainerNum = "CONTAINER4";
			container4.JC_ContainerCount = 8;
			container4.JC_RC = containerRef1.PK;

			var container5 = OtherBooking2.QuotedBookingContainers.AddNew();
			container5.JC_ContainerCount = 15;
			container5.JC_RC = containerRef2.PK;

			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);
			var viewQuotedBooking2 = ViewQuotedBooking.LoadOrCreate(OtherBooking2);

			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking2);

			Combine.Combine(Factory);

			Factory.Save();

			AssertEquals(4, MasterBooking.QuotedBookingContainers.Count);

			AssertEquals("CONTAINER1", MasterBooking.QuotedBookingContainers[0].JC_ContainerNum);
			AssertEquals(3, (int)MasterBooking.QuotedBookingContainers[0].JC_ContainerCount);
			AssertEquals(containerRef1.PK, MasterBooking.QuotedBookingContainers[0].JC_RC);

			AssertEquals("", MasterBooking.QuotedBookingContainers[1].JC_ContainerNum);
			AssertEquals(4, (int)MasterBooking.QuotedBookingContainers[1].JC_ContainerCount);
			AssertEquals(containerRef2.PK, MasterBooking.QuotedBookingContainers[1].JC_RC);

			var cloneContainer4 = MasterBooking.QuotedBookingContainers.Cast<ForwardingContainer>()
						.First(x => x.JC_ContainerNum == "CONTAINER4"
								&& x.JC_ContainerCount == 8
								&& x.JC_RC == containerRef1.PK);
			AssertNotNull("container4", cloneContainer4);

			var cloneContainer5 = MasterBooking.QuotedBookingContainers.Cast<ForwardingContainer>()
						.First(x => x.JC_ContainerNum == ""
								&& x.JC_ContainerCount == 15
								&& x.JC_RC == containerRef2.PK);
			AssertNotNull("container5", cloneContainer5);
		}

		public void TestCombinePackLines()
		{
			var packline1 = MasterBooking.Booking.OuterPackLines.AddNew();
			packline1.JL_Description = "packline1";

			var packline2 = MasterBooking.Booking.OuterPackLines.AddNew();
			packline2.JL_Description = "packline2";

			var packline3 = OtherBooking1.Booking.OuterPackLines.AddNew();
			packline3.JL_Description = "packline3";

			var packline4 = OtherBooking2.Booking.OuterPackLines.AddNew();
			packline4.JL_Description = "packline4";

			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);
			var viewQuotedBooking2 = ViewQuotedBooking.LoadOrCreate(OtherBooking2);

			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking2);

			Combine.Combine(Factory);

			AssertContainsExactElementsInAnyOrder("MasterBooking should have all the packlines"
												, new string[] { "packline1", "packline2", "packline3", "packline4" },
												Array.ConvertAll(MasterBooking.Booking.OuterPackLines.ToArray<ForwardingPackLine>(), (p) => p.JL_Description.ToString()));
		}

		public void TestCancelOtherBooking()
		{
			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);
			var viewQuotedBooking2 = ViewQuotedBooking.LoadOrCreate(OtherBooking2);

			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking2);

			Combine.Combine(Factory);

			Assert(OtherBooking1.Booking.IsCancelled);
			Assert(OtherBooking2.IsCancelled);
		}

		public void TestCombineTotalWeightAndVolume()
		{
			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);
			var viewQuotedBooking2 = ViewQuotedBooking.LoadOrCreate(OtherBooking2);

			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking2);

			MasterBooking.Booking.JS_ActualWeight = 18000;
			MasterBooking.Booking.JS_UnitOfWeight = Constants.Weight.Kilograms;
			MasterBooking.Booking.JS_ActualVolume = 12;
			MasterBooking.Booking.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			OtherBooking1.Booking.JS_ActualWeight = 20;
			OtherBooking1.Booking.JS_UnitOfWeight = Constants.Weight.Tonnes;
			OtherBooking1.Booking.JS_ActualVolume = 10;
			OtherBooking1.Booking.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			OtherBooking2.Booking.JS_ActualWeight = 25000;
			OtherBooking2.Booking.JS_UnitOfWeight = Constants.Weight.Kilograms;
			OtherBooking2.Booking.JS_ActualVolume = 0.015;
			OtherBooking2.Booking.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			Factory.Save();

			bool factorySaved = false;
			var combineFactory = new BusinessObjectFactory();
			combineFactory.Saving += delegate
			{ factorySaved = true; };

			AssertEquals("precondition: saving should not be called", false, factorySaved);
			Combine.Combine(combineFactory);
			AssertEquals("Combine() should not have saved the factory", false, factorySaved);

			combineFactory.Save();
			AssertEquals("combineFactory has saved", true, factorySaved);

			var anotherFactory = new BusinessObjectFactory();
			var masterBookingInAnotherFactory = anotherFactory.Load<QuotedBooking>(MasterBooking.PK);

			AssertEquals("MasterBooking Weight", 63000m, masterBookingInAnotherFactory.Weight);
			AssertEquals("MasterBooking Weight Unit", Constants.Weight.Kilograms, masterBookingInAnotherFactory.WeightUnit);
			AssertEquals("MasterBooking Volume", 37m, masterBookingInAnotherFactory.Volume);
			AssertEquals("MasterBooking Volume Unit", Constants.Volume.CubicMetres, masterBookingInAnotherFactory.VolumeUnit);
		}

		public void TestDefaultFilters()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "1234";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "CNSHA";

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var booking = Factory.New<ForwardingShipment>();
			booking.JS_TransportMode = "AIR";
			booking.JS_PackingMode = "ULD";
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "CNSHA";
			booking.JS_JX = sailing.PK;
			booking.JS_RL_NKLoadPort = "NZAKL";
			booking.JS_RL_NKDischargePort = "USJFK";
			booking.ConsigneePK = consignee.PK;
			booking.ConsignorPK = consignor.PK;
			booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.OH_Carrier = carrier.PK;
			quotedBooking.ClientPK = client.PK;

			Factory.Save();

			var combine = new CombineBookings(quotedBooking);

			var collection = combine.OtherViewQuotedBookingsLookups;

			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.TransportMode, "Property", (ZString)"AIR");
			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.ContainerMode, "Property", (ZString)"ULD");
			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.VoyageVessel, "VoyageFlightNo", (ZString)"1234");
			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.VoyageVessel, "Vessel", (ZString)"MAJAPAHIT");
			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.LoadDischarge, "Property1", (ZString)"NZAKL");
			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.LoadDischarge, "Property2", (ZString)"USJFK");
			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.OriginDestination, "Property1", (ZString)"AUSYD");
			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.OriginDestination, "Property2", (ZString)"CNSHA");
			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.BookingParty, "Property", bookingParty.PK);
			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.Carrier, "Property", carrier.PK);
			AssertHasDefault(collection, ViewQuotedBookingDefalutFilterProvider.FilterNames.Client, "Property", client.PK);
		}

		public void TestCombineNotes()
		{
			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);

			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);

			var note1 = MasterBooking.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "note1");
			var note2 = MasterBooking.Notes.AddNew(false, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, "note2");
			var note3 = MasterBooking.Notes.AddNew(false, PredefinedNoteTypes.Instance.FaxEmailTransmissionLog.Description, "note3");
			var note4 = OtherBooking1.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "note4");
			var note5 = OtherBooking1.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "note5");
			var note6 = OtherBooking1.Notes.AddNew(false, PredefinedNoteTypes.Instance.FaxEmailTransmissionLog.Description, "note6");

			Assert(PredefinedNoteTypes.Instance.DetailedGoodsDescription.IsOnlyOneAllowed);
			Assert(PredefinedNoteTypes.Instance.MarksAndNumbers.IsOnlyOneAllowed);
			Assert(!PredefinedNoteTypes.Instance.FaxEmailTransmissionLog.IsOnlyOneAllowed);
			Assert(PredefinedNoteTypes.Instance.HandlingInstructions.IsOnlyOneAllowed);

			Factory.Save();

			var combineFactory = new BusinessObjectFactory();
			Combine.Combine(combineFactory);

			var masterBookingInCombineFactory = combineFactory.Load<QuotedBooking>(MasterBooking.PK);
			var otherBooking1InCombineFactory = combineFactory.Load<QuotedBooking>(OtherBooking1.PK);

			AssertContainsExactElementsInAnyOrder(
				"MasterBooking should have a note of each type.",
				new string[]
				{
					$"{PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description} - note1",
					$"{PredefinedNoteTypes.Instance.MarksAndNumbers.Description} - note2",
					$"{PredefinedNoteTypes.Instance.FaxEmailTransmissionLog.Description} - note3",
					$"{PredefinedNoteTypes.Instance.HandlingInstructions.Description} - note5",
					$"{PredefinedNoteTypes.Instance.FaxEmailTransmissionLog.Description} - note6"
				},
				Array.ConvertAll(masterBookingInCombineFactory.Notes.GetAllNotes().ToArray<StmNote>(), (s) => s.ST_Description + " - " + s.ST_NoteDataAsText));

			AssertContainsExactElementsInAnyOrder(
				"OtherBooking1 should keep it's notes",
				BusinessObjectEqualityComparer<StmNote>.IgnoreFactoryComparer,
				(s) => s.ST_Description + " - " + s.ST_NoteText,
				new StmNote[] { note4, note5, note6 },
				otherBooking1InCombineFactory.Notes.GetAllNotes().ToArray<StmNote>());
		}

		public void TestCombineServices()
		{
			var service1 = MasterBooking.Services.AddIfNotExists(Constants.FreightServiceType.Codes.Fumigation);
			var service2 = MasterBooking.Services.AddIfNotExists(Constants.FreightServiceType.Codes.Cleaning);

			var service3 = OtherBooking1.Services.AddIfNotExists(Constants.FreightServiceType.Codes.Cleaning);
			var service4 = OtherBooking2.Services.AddIfNotExists(Constants.FreightServiceType.Codes.CustomsHold);

			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);
			var viewQuotedBooking2 = ViewQuotedBooking.LoadOrCreate(OtherBooking2);

			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking2);

			Factory.Save();

			var combineFactory = new BusinessObjectFactory();
			Combine.Combine(combineFactory);

			var masterBookingInCombineFactory = combineFactory.Load<QuotedBooking>(MasterBooking.PK);
			var otherBooking1InCombineFactory = combineFactory.Load<QuotedBooking>(OtherBooking1.PK);
			var otherBooking2InCombineFactory = combineFactory.Load<QuotedBooking>(OtherBooking2.PK);

			AssertContainsExactElementsInAnyOrder(
				"MasterBooking has combined all the sevices which are different",
				new string[]
				{
					Constants.FreightServiceType.Codes.Fumigation,
					Constants.FreightServiceType.Codes.Cleaning,
					Constants.FreightServiceType.Codes.CustomsHold
				},
				masterBookingInCombineFactory.Services.Cast<JobService>().Select(x => x.ES_ServiceCode));

			AssertContainsExactElementsInAnyOrder(
				"OtherBooking1 should keep it's services",
				new string[]
				{
					Constants.FreightServiceType.Codes.Cleaning,
				},
				otherBooking1InCombineFactory.Services.Cast<JobService>().Select(x => x.ES_ServiceCode));

			AssertContainsExactElementsInAnyOrder(
				"OtherBooking2 should keep it's services",
				new string[]
				{
					Constants.FreightServiceType.Codes.CustomsHold,
				},
				otherBooking2InCombineFactory.Services.Cast<JobService>().Select(x => x.ES_ServiceCode));
		}

		public void TestCombineReferenceNumbers()
		{
			var cusEntryNumber1 = MasterBooking.Booking.Numbers.AddNew();
			cusEntryNumber1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference;
			cusEntryNumber1.CE_EntryNum = "OAG-1";
			cusEntryNumber1.CE_RN_NKCountryCode = "AU";

			var cusEntryNumber2 = MasterBooking.Booking.Numbers.AddNew();
			cusEntryNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			cusEntryNumber2.CE_EntryNum = "AMS-1";
			cusEntryNumber1.CE_RN_NKCountryCode = "AU";

			var cusEntryNumber3 = OtherBooking1.Booking.Numbers.AddNew();
			cusEntryNumber3.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference;
			cusEntryNumber3.CE_EntryNum = "OAG-1";
			cusEntryNumber3.CE_RN_NKCountryCode = "AU";

			var cusEntryNumber4 = OtherBooking1.Booking.Numbers.AddNew();
			cusEntryNumber4.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference;
			cusEntryNumber4.CE_EntryNum = "OAG-1";
			cusEntryNumber4.CE_RN_NKCountryCode = "CN";

			var cusEntryNumber5 = OtherBooking1.Booking.Numbers.AddNew();
			cusEntryNumber5.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference;
			cusEntryNumber5.CE_EntryNum = "OAG-2";
			cusEntryNumber5.CE_RN_NKCountryCode = "AU";

			var cusEntryNumber6 = OtherBooking1.Booking.Numbers.AddNew();
			cusEntryNumber6.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference;
			cusEntryNumber6.CE_EntryNum = "OAG-2";
			cusEntryNumber6.CE_RN_NKCountryCode = "CN";

			var cusEntryNumber7 = OtherBooking1.Booking.Numbers.AddNew();
			cusEntryNumber7.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			cusEntryNumber7.CE_EntryNum = "AMS-1";
			cusEntryNumber7.CE_RN_NKCountryCode = "AU";

			var cusEntryNumber8 = OtherBooking1.Booking.Numbers.AddNew();
			cusEntryNumber8.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			cusEntryNumber8.CE_EntryNum = "AMS-1";
			cusEntryNumber8.CE_RN_NKCountryCode = "CN";

			var cusEntryNumber9 = OtherBooking1.Booking.Numbers.AddNew();
			cusEntryNumber9.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			cusEntryNumber9.CE_EntryNum = "AMS-2";
			cusEntryNumber9.CE_RN_NKCountryCode = "AU";

			Factory.Save();

			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);

			var combineFactory = new BusinessObjectFactory();
			Combine.Combine(combineFactory);

			var masterBookingInCombineFactory = combineFactory.Load<QuotedBooking>(MasterBooking.PK);
			var otherBooking1InCombineFactory = combineFactory.Load<QuotedBooking>(OtherBooking1.PK);

			AssertContainsExactElementsInAnyOrder(
				"MasterBooking should have combined all numbers",
				new string[]
				{
					$"{CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference}, OAG-1, AU",
					$"{CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS}, AMS-1, AU",
					$"{CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference}, OAG-1, CN",
					$"{CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS}, AMS-1, CN"
				},
				Array.ConvertAll(masterBookingInCombineFactory.Booking.Numbers.ToArray<CusEntryNumber>(), (s) => s.CE_EntryType + ", " + s.CE_EntryNum + ", " + s.CE_RN_NKCountryCode));

			AssertContainsExactElementsInAnyOrder(
				"OtherBooking1 should keep it's numbers",
				new ZGuid[]
				{
					cusEntryNumber3.PK,
					cusEntryNumber4.PK,
					cusEntryNumber5.PK,
					cusEntryNumber6.PK,
					cusEntryNumber7.PK,
					cusEntryNumber8.PK,
					cusEntryNumber9.PK
				},
				otherBooking1InCombineFactory.Booking.Numbers.Cast<CusEntryNumber>().Select(x => x.PK));
		}

		public void TestComboneAttachedOrders()
		{
			AssertEquals(0, MasterBooking.Booking.AttachedOrders.Count);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "Buyer";

			var attachedOrder1 = OtherBooking1.Booking.AttachedOrders.AddNew();
			attachedOrder1.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			attachedOrder1.JD_BookingConfRef = "Ref001";

			var attachedOrder2 = OtherBooking1.Booking.AttachedOrders.AddNew();
			attachedOrder2.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			attachedOrder2.JD_BookingConfRef = "Ref002";

			Factory.Save();

			AssertEquals(2, OtherBooking1.Booking.AttachedOrders.Count);

			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);

			var combineFactory = new BusinessObjectFactory();
			Combine.Combine(combineFactory);

			var masterBookingInCombineFactory = combineFactory.Load<QuotedBooking>(MasterBooking.PK);
			var otherBookingInCombineFactory = combineFactory.Load<QuotedBooking>(OtherBooking1.PK);

			AssertEquals(0, otherBookingInCombineFactory.Booking.AttachedOrders.Count);
			AssertEquals(2, masterBookingInCombineFactory.Booking.AttachedOrders.Count);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					attachedOrder1.PK,
					attachedOrder2.PK
				},
				masterBookingInCombineFactory.Booking.AttachedOrders.Select(x => x.PK));
		}

		public void TestComboneAttachedWarehouseOrders()
		{
			AssertEquals(0, MasterBooking.Booking.AttachedWarehouseOrders.Count);

			var warehouseOrder1 = (BusinessObject)Factory.New<IWhsOrder>();
			warehouseOrder1.FillWithValidTestData();
			OtherBooking1.Booking.AttachedWarehouseOrders.Add(warehouseOrder1);

			var warehouseOrder2 = (BusinessObject)Factory.New<IWhsOrder>();
			warehouseOrder2.FillWithValidTestData();
			OtherBooking1.Booking.AttachedWarehouseOrders.Add(warehouseOrder2);

			Factory.Save();

			AssertEquals(2, OtherBooking1.Booking.AttachedWarehouseOrders.Count);

			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);

			var combineFactory = new BusinessObjectFactory();
			Combine.Combine(combineFactory);

			var masterBookingInCombineFactory = combineFactory.Load<QuotedBooking>(MasterBooking.PK);
			var otherBookingInCombineFactory = combineFactory.Load<QuotedBooking>(OtherBooking1.PK);

			AssertEquals(0, otherBookingInCombineFactory.Booking.AttachedWarehouseOrders.Count);
			AssertEquals(2, masterBookingInCombineFactory.Booking.AttachedWarehouseOrders.Count);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					warehouseOrder1.PK,
					warehouseOrder2.PK
				},
				masterBookingInCombineFactory.Booking.AttachedWarehouseOrders.ToArray().Select(o => o.PK));
		}

		public void TestCombinedOrderItemsRefs()
		{
			addOrderReference(OtherBooking1, "Ref1");
			addOrderReference(OtherBooking1, "Ref2");
			addOrderReference(OtherBooking1, "Ref3");
			addOrderReference(OtherBooking2, "Ref2");
			addOrderReference(OtherBooking2, "Ref4");
			addOrderReference(MasterBooking, "Ref1");
			addOrderReference(MasterBooking, "Ref2");

			Factory.Save();

			AssertEquals(2, MasterBooking.Booking.DocsAndCartage.OrderItems.Count);
			AssertEquals(3, OtherBooking1.Booking.DocsAndCartage.OrderItems.Count);
			AssertEquals(2, OtherBooking2.Booking.DocsAndCartage.OrderItems.Count);

			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);
			var viewQuotedBooking2 = ViewQuotedBooking.LoadOrCreate(OtherBooking2);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking2);

			var combineFactory = new BusinessObjectFactory();
			Combine.Combine(combineFactory);

			combineFactory.Save();

			var reloadedFactory = new BusinessObjectFactory();

			var masterBookingInReloadedFactory = reloadedFactory.Load<QuotedBooking>(MasterBooking.PK);

			AssertEquals("There should be 4 order references", 4, masterBookingInReloadedFactory.Booking.DocsAndCartage.JP_OrderItemsAsString.Split(',').Length);

			AssertContainsExactElementsInAnyOrder(
				"MasterBooking should have all the order references without duplication",
				new string[]
				{
					"Ref1",
					"Ref2",
					"Ref3",
					"Ref4",
				},
				masterBookingInReloadedFactory.Booking.DocsAndCartage.JP_OrderItemsAsString.Split(','));

			void addOrderReference(QuotedBooking quotedBooking, ZString orderReference)
			{
				var item = quotedBooking.Booking.DocsAndCartage.OrderItems.AddNew();
				item.FillWithValidTestData();
				item.JT_OrderReference = orderReference;
			}
		}

		public void TestMoveEDocs()
		{
			AddEdoc(MasterBooking, "eDoc1");
			AddEdoc(OtherBooking1, "eDoc2");
			AddEdoc(OtherBooking2, "eDoc3");

			Factory.Save();

			var viewQuotedBooking1 = ViewQuotedBooking.LoadOrCreate(OtherBooking1);
			var viewQuotedBooking2 = ViewQuotedBooking.LoadOrCreate(OtherBooking2);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking1);
			Combine.OtherViewQuotedBookings.Add(viewQuotedBooking2);

			var combineFactory = new BusinessObjectFactory();
			Combine.Combine(combineFactory);

			var masterBookingInCombineFactory = combineFactory.Load<QuotedBooking>(MasterBooking.PK);
			var otherBooking1InCombineFactory = combineFactory.Load<QuotedBooking>(OtherBooking1.PK);
			var otherBooking2InCombineFactory = combineFactory.Load<QuotedBooking>(OtherBooking2.PK);

			AssertContainsExactElementsInAnyOrder(
				"MasterBooking has moved all the eDocs",
				new string[]
				{
					"eDoc1",
					"eDoc2",
					"eDoc3"
				},
				((IDocManagerSupport)masterBookingInCombineFactory.Booking).DocManagerInfo.AllEDocs.Cast<IeDoc>().Select(edoc => edoc.FileName));

			AssertContainsExactElementsInAnyOrder(
				"OtherBooking1 should keep the old eDocs",
				new string[]
				{
					"eDoc2"
				},
				((IDocManagerSupport)otherBooking1InCombineFactory.Booking).DocManagerInfo.AllEDocs.Cast<IeDoc>().Select(edoc => edoc.FileName));

			AssertContainsExactElementsInAnyOrder(
				"OtherBooking2 should keep the old eDocs",
				new string[]
				{
					"eDoc3"
				},
				((IDocManagerSupport)otherBooking2InCombineFactory.Booking).DocManagerInfo.AllEDocs.Cast<IeDoc>().Select(edoc => edoc.FileName));
		}

		void AddEdoc(QuotedBooking quotedBooking, string eDocName)
		{
			var docManagerInfo = ((IDocManagerSupport)quotedBooking.Booking).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), eDocName, "INV");
			docManagerInfo.Save();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Combine;
		}

		CombineBookings Combine
		{
			get { return combine ?? (combine = new CombineBookings(MasterBooking)); }
		}
		CombineBookings combine;

		QuotedBooking MasterBooking
		{
			get
			{
				if (masterBooking == null)
				{
					var booking = Factory.New<ForwardingShipment>();
					var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
					masterBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
				}

				return masterBooking;
			}
		}
		QuotedBooking masterBooking;

		QuotedBooking OtherBooking1
		{
			get
			{
				if (otherBooking1 == null)
				{
					var booking = Factory.New<ForwardingShipment>();
					var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
					otherBooking1 = QuotedBooking.New(quote.PK, booking.PK, Factory);
				}

				return otherBooking1;
			}
		}
		QuotedBooking otherBooking1;

		QuotedBooking OtherBooking2
		{
			get
			{
				if (otherBooking2 == null)
				{
					var booking = Factory.New<ForwardingShipment>();
					var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
					otherBooking2 = QuotedBooking.New(quote.PK, booking.PK, Factory);
				}

				return otherBooking2;
			}
		}
		QuotedBooking otherBooking2;

		#endregion
	}
}
