using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Module;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.QuotedBookings.Business.QuotedBookingToShipmentConverter;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	[TestedType(typeof(QuotedBookingFilterStripBusinessObject))]
	public class QuotedBookingFilterStripBusinessObjectTest : BaseQuotedBookingFilterStripBusinessObjectTest
	{
		public void TestEmptyConstructorIsValid()
		{
			var emptyConstructor = new QuotedBookingFilterStripBusinessObject()
				.GetType()
				.GetConstructor(Type.EmptyTypes);

			AssertNotNull("Please do not remove parameterless constructor - for reference see: WI00229255", emptyConstructor);
		}

		#region Numbers

		public void TestCompanyTariffLevelOverrideFilter()
		{
			var filter = GetNewFilterStripBusinessObject();
			var companyTariffLevelOverrideFilter = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.NumbersAndReferences.CompanyTariffLevelOverride]);
			companyTariffLevelOverrideFilter.IsActive = true;
			var glbTariff = Factory.New<GlobalTariff>();
			var glbTariff2 = Factory.New<GlobalTariff>();

			var booking1 = CreateBookingOnly();
			var booking2 = CreateBookingOnly();
			var booking3 = CreateBookingOnly();
			var unacceptedBWQ = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var acceptedBWQ = CreateQuotedBooking();
			var oneOffQuote = CreateQuoteOnly();
			booking1.CompanyTariffLevel = "";
			booking2.CompanyTariffLevel = "1";
			booking3.CompanyTariffLevel = "2";
			unacceptedBWQ.CompanyTariffLevel = "2";
			acceptedBWQ.CompanyTariffLevel = "2";
			oneOffQuote.CompanyTariffLevel = "2";
			Factory.Save();
			companyTariffLevelOverrideFilter.Property = "2";
			AssertBookingCollectionIsFiltered("Searching for QuotedBooking whose Quote.CompanyTariffLevelOverride is 2", filter, booking3, acceptedBWQ, unacceptedBWQ);
			companyTariffLevelOverrideFilter.Property = "1";
			AssertBookingCollectionIsFiltered("Searching for QuotedBooking whose Quote.CompanyTariffLevelOverride is 1", filter, booking2);
			companyTariffLevelOverrideFilter.Property = "0";
			AssertBookingCollectionIsFiltered("Searching for QuotedBooking whose Quote.CompanyTariffLevelOverride is 0", filter, booking1);
		}

		#region TestHouseBillNo

		public void TestHouseBillNo()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();

			QuotedBooking quoteOnly = CreateQuoteOnly();

			QuotedBooking bookingOnly1 = CreateBookingOnly();
			QuotedBooking bookingOnly2 = CreateBookingOnly();

			quotedBooking1.Booking.JS_HouseBill = "quotedbook";
			quotedBooking2.Booking.JS_HouseBill = "quoted";
			quotedBooking3.Booking.JS_HouseBill = "";

			bookingOnly1.Booking.JS_HouseBill = "booking";
			bookingOnly2.Booking.JS_HouseBill = "";

			Factory.Save();

			var filter = (QuotedBookingFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			ModuleTextFilter houseBillNoFilter = ((ModuleTextFilter)filter["House Bill #"]);

			houseBillNoFilter.Property = "quotedbook";
			houseBillNoFilter.IsActive = true;
			AssertBookingCollectionIsFiltered(filter, quotedBooking1);

			houseBillNoFilter.Property = "booking";
			AssertBookingCollectionIsFiltered(filter, bookingOnly1);

			houseBillNoFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertBookingCollectionIsFiltered(filter, quotedBooking1, quotedBooking2, bookingOnly1);

			houseBillNoFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertBookingCollectionIsFiltered(filter, quotedBooking3, bookingOnly2);
		}

		#endregion

		#region TestCFSRefNo

		public void TestCFSRefNo()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();
			QuotedBooking quotedBooking4 = CreateQuotedBooking();
			QuotedBooking quotedBooking5 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_CFSReference = "P00000001";
			quotedBooking1.Booking.JS_TransportMode = Core.Constants.ContainerModes.LCL;
			quotedBooking2.Booking.JS_CFSReference = "P00000001";
			quotedBooking2.Booking.JS_TransportMode = Core.Constants.ContainerModes.FCL;
			quotedBooking3.Booking.JS_CFSReference = "P00000002";
			quotedBooking3.Booking.JS_TransportMode = Core.Constants.ContainerModes.FCL;
			quotedBooking5.Booking.JS_CFSReference = "P00000004";
			quotedBooking5.Booking.JS_TransportMode = Core.Constants.ContainerModes.LCL;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["CFS Ref #"]).Property = "P00000001";
			((ModuleTextFilter)filter["CFS Ref #"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[1].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[1].QuotedBooking.Booking.PK);

			((ModuleTextFilter)filter["CFS Ref #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking4.PK }, collection.Select(element => element.PK));

			((ModuleTextFilter)filter["CFS Ref #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking1.PK, quotedBooking2.PK, quotedBooking3.PK, quotedBooking5.PK }, collection.Select(element => element.PK));
		}

		#endregion

		#region TestDirectMAWBNo

		public void TestDirectMAWBNo()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();
			QuotedBooking quotedBooking4 = CreateQuotedBooking();
			QuotedBooking quotedBooking5 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_IsDirectBooking = true;
			quotedBooking1.Booking.JS_HouseBill = "1111";
			quotedBooking2.Booking.JS_IsDirectBooking = false;
			quotedBooking2.Booking.JS_HouseBill = "1111";
			quotedBooking3.Booking.JS_IsDirectBooking = true;
			quotedBooking3.Booking.JS_HouseBill = "3333";
			quotedBooking4.Booking.JS_IsDirectBooking = true;
			quotedBooking4.Booking.JS_HouseBill = "";
			quotedBooking5.Booking.JS_IsDirectBooking = false;
			quotedBooking5.Booking.JS_HouseBill = "";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleNumberFilter)filter["Direct MAWB #"]).Property = "1111";
			((ModuleNumberFilter)filter["Direct MAWB #"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleNumberFilter)filter["Direct MAWB #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(filter.Filter);

			ZGuid[] expectedBlankPKs = new ZGuid[] { quotedBooking4.PK };
			AssertContainsExactElementsInAnyOrder(expectedBlankPKs, collection.Select(element => element.PK));

			((ModuleNumberFilter)filter["Direct MAWB #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(filter.Filter);

			ZGuid[] expectedNotBlankPKs = new ZGuid[] { quotedBooking1.PK, quotedBooking3.PK };
			AssertContainsExactElementsInAnyOrder(expectedNotBlankPKs, collection.Select(element => element.PK));
		}

		#endregion

		#region TestFlightVoyageNoAndVessel

		public void TestFlightVoyageNoAndVessel()
		{
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "FlyDutchman";

			JobVoyage voyage1 = CreateVoyage(Core.Constants.TransportModes.Sea, TestVessel1, "Voyage");
			JobVoyage voyage2 = CreateVoyage(Core.Constants.TransportModes.Sea, vessel2, "VoyDancing");

			JobSailing sailing1 = GetOrCreateSailing(voyage1, HomePort, OverseasPort);
			JobSailing sailing2 = GetOrCreateSailing(voyage2, HomePort, OverseasPort);

			QuotedBooking quotedBooking1 = CreateQuotedBookingWithSailing(sailing1);
			QuotedBooking quotedBooking2 = CreateQuotedBookingWithSailing(sailing2);

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Consolidated/Converted"]).Visibility = FilterVisibility.Visible;

			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).Vessel = TestVessel1.RV_Name;
			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).Vessel = "utch";
			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 ViewQuotedBookings", 1, collection.Count);
			AssertEquals("Should have QuotedBooking2's Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking2's Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).VoyageFlightNo = "Dan";
			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 ViewQuotedBookings", 1, collection.Count);
			AssertEquals("Should have QuotedBooking2's Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking2's Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).Vessel = "";
			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).VoyageFlightNo = "";
			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 ViewQuotedBookings", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder("Should have both QuotedBooking Quotes", new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, collection.Select(x => x.QuotedBooking.Quote.PK));
			AssertContainsExactElementsInAnyOrder("Should have both QuotedBooking Bookings", new[] { quotedBooking1.Booking.PK, quotedBooking2.Booking.PK }, collection.Select(x => x.QuotedBooking.Booking.PK));

			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded no ViewQuotedBookings", 0, collection.Count);
		}

		public void TestFlightVoyageNoAndVessel_ConvertedBooking()
		{
			var voyage1 = CreateVoyage(Core.Constants.TransportModes.Sea, TestVessel1, "Voyage");
			var sailing1 = GetOrCreateSailing(voyage1, HomePort, OverseasPort);
			var quotedBooking1 = CreateQuotedBookingWithSailing(sailing1);

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Consolidated/Converted"]).Property = QuotedBookingFilterStripBusinessObject.ConsolidatedStatus.Code.All;
			((ModuleTextFilter)filter["Consolidated/Converted"]).IsActive = true;

			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).Vessel = TestVessel1.RV_Name;
			((VoyageVesselModuleFilter)filter["Flight/Voyage # and Vessel"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("pre: filter should match initially", 1, collection.Count);

			quotedBooking1.ShipmentStatus = ShipmentStatusList.Codes.Booked;
			var converter = new QuotedBookingToShipmentConverter(quotedBooking1, BookingToShipmentConversionSource.WorkflowTrigger);
			AssertEquals(false, converter.HasAnyErrors(out string _));
			converter.ConvertBookingToShipment(quotedBooking1.Booking);
			Factory.Save();

			collection.Load(filter.Filter);
			AssertEquals("filter should match after converting to shipment", 1, collection.Count);

			var shipment = Factory.Load<ForwardingShipment>(quotedBooking1.Booking.PK);
			var newConsol = Factory.New<ForwardingConsol>();
			ConsolStandardAloneShipmentRelationshipHelper.MakeConsolFromStandaloneShipment(newConsol, shipment); // called from ConsolModuleButtonGrid
			Factory.Save();

			collection.Load(filter.Filter);
			AssertEquals("filter should match after adding a consol to the shipment", 1, collection.Count);
		}

		#endregion

		#region TestShippersRefNo

		public void TestShippersRefNo()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();
			QuotedBooking quotedBooking4 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_BookingReference = "1111";
			quotedBooking2.Booking.JS_BookingReference = "2222";
			quotedBooking3.Booking.JS_BookingReference = "3333";
			quotedBooking4.Booking.JS_BookingReference = "";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleNumberFilter)filter["Shippers Ref #"]).Property = "1111";
			((ModuleNumberFilter)filter["Shippers Ref #"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 ViewQuotedBookings", 1, collection.Count);
			AssertEquals("Should have QuotedBooking1's Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking1's Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleNumberFilter)filter["Shippers Ref #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking4.PK }, collection.Select(element => element.PK));

			((ModuleNumberFilter)filter["Shippers Ref #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking1.PK, quotedBooking2.PK, quotedBooking3.PK }, collection.Select(element => element.PK));
		}

		#endregion

		#region TestBookingNo

		public void TestBookingNo()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_UniqueConsignRef = "S1010No";
			quotedBooking2.Booking.JS_UniqueConsignRef = "S10101010";
			quotedBooking3.Booking.JS_UniqueConsignRef = "S1010Nup";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleNumberFilter)filter["Booking #"]).Property = "S10101010";
			((ModuleNumberFilter)filter["Booking #"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 QuotedBooking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking2's Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking2's Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			QuotedBooking bookingOnly = CreateBookingOnly();
			QuotedBooking quoteOnly = CreateQuoteOnly();
			QuotedBooking quotedBooking4 = CreateQuotedBooking();
			Factory.Save();

			((ModuleNumberFilter)filter["Booking #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			((ModuleNumberFilter)filter["Booking #"]).Property = "";
			collection.Load(filter.Filter);

			ZGuid[] expectedPKs = new ZGuid[] { quotedBooking1.PK, quotedBooking2.PK, quotedBooking3.PK, bookingOnly.PK, quotedBooking4.PK };
			AssertContainsExactElementsInAnyOrder(expectedPKs, collection.Select(element => element.PK));

			((ModuleNumberFilter)filter["Booking #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(filter.Filter);

			AssertEquals("Should not load any ViewQuotedBookings", 0, collection.Count);
		}

		#endregion

		#region TestContainerNo

		public void TestContainerNo()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();

			quotedBooking1.QuotedBookingContainers.AddNew().JC_ContainerNum = "";
			quotedBooking2.QuotedBookingContainers.AddNew().JC_ContainerNum = "CONT121314";
			quotedBooking3.QuotedBookingContainers.AddNew().JC_ContainerNum = "CONT222222";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleNumberFilter)filter["Container #"]).Property = "CONT121314";
			((ModuleNumberFilter)filter["Container #"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 ViewQuotedBookings", 1, collection.Count);
			AssertEquals("Should have QuotedBooking2's Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking2's Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			Factory.Save();

			((ModuleNumberFilter)filter["Container #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(filter.Filter);

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking1.PK, }, collection.Select(element => element.PK));

			((ModuleNumberFilter)filter["Container #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking2.PK, quotedBooking3.PK }, collection.Select(element => element.PK));
		}

		#endregion

		#region TestAdditionReferenceNumber

		public void TestAdditionalReferenceNumber()
		{
			CustomsReferenceNumberTypeCollection customsReferenceNumberTypeCollection = new CustomsReferenceNumberTypeCollection();
			customsReferenceNumberTypeCollection.Add("AAA", (NoResString)"AAA code");
			customsReferenceNumberTypeCollection.Add("BBB", (NoResString)"BBB code");

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customsReferenceNumberTypeCollection);

			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_UniqueConsignRef = "Booking1";
			AddAdditionalReference(quotedBooking1, "AU", "AAA", "11111");
			AddAdditionalReference(quotedBooking1, "AU", "BBB", "22222");

			QuotedBooking quickBooking1 = CreateBookingOnly();
			quickBooking1.Booking.JS_UniqueConsignRef = "Booking2";
			AddAdditionalReference(quickBooking1, "AU", "BBB", "22222");

			QuotedBooking quickBooking2 = CreateBookingOnly();
			quickBooking2.Booking.JS_UniqueConsignRef = "Booking3";

			QuotedBooking quotedBooking4 = CreateQuoteOnly();
			quotedBooking4.Job.JH_JobNum = "Booking4";

			QuotedBooking quotedBooking5 = CreateQuotedBooking();
			quotedBooking5.Booking.JS_UniqueConsignRef = "Booking5";
			AddAdditionalReference(quotedBooking5, "AU", "CCC", "");
			AddAdditionalReference(quotedBooking5, "US", "AAA", "");

			QuotedBooking quickBooking3 = CreateBookingOnly();
			quickBooking3.Booking.JS_UniqueConsignRef = "Booking6";
			AddAdditionalReference(quickBooking3, "AU", "DDD", "");

			Factory.Save();

			Asserter.AddFieldOfInterest("AU:AAA", (q) => GetAdditionalReferenceValue(q, "AU", "AAA"));
			Asserter.AddFieldOfInterest("AU:BBB", (q) => GetAdditionalReferenceValue(q, "AU", "BBB"));
			Asserter.AddFieldOfInterest("AU:CCC", (q) => GetAdditionalReferenceValue(q, "AU", "CCC"));
			Asserter.AddFieldOfInterest("AU:DDD", (q) => GetAdditionalReferenceValue(q, "AU", "DDD"));
			Asserter.AddFieldOfInterest("US:AAA", (q) => GetAdditionalReferenceValue(q, "US", "AAA"));

			ViewQuotedBooking viewQuotedBooking1 = GetView(quotedBooking1);
			ViewQuotedBooking viewQuotedBooking2 = GetView(quickBooking1);
			ViewQuotedBooking viewQuotedBooking3 = GetView(quickBooking2);
			ViewQuotedBooking viewQuotedBooking4 = GetView(quotedBooking4);
			ViewQuotedBooking viewQuotedBooking5 = GetView(quotedBooking5);
			ViewQuotedBooking viewQuotedBooking6 = GetView(quickBooking3);

			Asserter.AddToScope(viewQuotedBooking1);
			Asserter.AddToScope(viewQuotedBooking2);
			Asserter.AddToScope(viewQuotedBooking3);
			Asserter.AddToScope(viewQuotedBooking4);
			Asserter.AddToScope(viewQuotedBooking5);
			Asserter.AddToScope(viewQuotedBooking6);

			var filterStrip = GetNewFilterStripBusinessObject();
			ReferenceNumberFilter filter = (ReferenceNumberFilter)filterStrip[QuotedBookingFilterStripBusinessObject.Descriptions.NumbersAndReferences.AdditionalReferenceNumber];
			filter.IsActive = true;

			filter.Property = "";
			Asserter.AssertMatches("empty", filterStrip.Filter, viewQuotedBooking1, viewQuotedBooking2, viewQuotedBooking3, viewQuotedBooking5, viewQuotedBooking6);

			filter.Property = "1";
			Asserter.AssertMatches("1", filterStrip.Filter, viewQuotedBooking1);

			filter.Property = "11111";
			Asserter.AssertMatches("11111", filterStrip.Filter, viewQuotedBooking1);

			filter.Property = "22222";
			Asserter.AssertMatches("22222", filterStrip.Filter, viewQuotedBooking1, viewQuotedBooking2);

			filter.Property = "XXX";
			Asserter.AssertMatches("XXX", filterStrip.Filter);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filter.Type = "BBB";
			Asserter.AssertMatches("is blank", filterStrip.Filter, viewQuotedBooking3, viewQuotedBooking5, viewQuotedBooking6);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Asserter.AssertMatches("is not blank", filterStrip.Filter, viewQuotedBooking1, viewQuotedBooking2);

			quickBooking1.Job.Dispose();
			quickBooking2.Job.Dispose();
			quickBooking3.Job.Dispose();
		}

		#endregion

		#region TestQuoteNo

		public void TestQuoteNo()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();
			QuotedBooking quoteOnly = CreateQuoteOnly();

			quotedBooking1.Quote.TH_QuoteNumber = "QB001001";
			quotedBooking2.Quote.TH_QuoteNumber = "QB001002";
			quotedBooking3.Quote.TH_QuoteNumber = "QB001003";
			quoteOnly.Quote.TH_QuoteNumber = "Q0000001";

			Factory.Save();

			FilterStripBusinessObject filter = GetNewFilterStripBusinessObject();
			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(new BusinessObjectFactory());

			((ModuleNumberFilter)filter["Quote #"]).IsActive = true;
			((ModuleNumberFilter)filter["Quote #"]).Property = "Q0000001";
			collection.Load(filter.Filter);
			AssertEquals("Should not load the One Off Quote", 0, collection.Count);

			((ModuleNumberFilter)filter["Quote #"]).Property = "QB001002";
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 QuotedBooking", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking2.PK }, collection.Select(element => element.PK));

			((ModuleNumberFilter)filter["Quote #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			((ModuleNumberFilter)filter["Quote #"]).Property = "Q";
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 3 ViewQuotedBookings", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking1.PK, quotedBooking2.PK, quotedBooking3.PK }, collection.Select(element => element.PK));

			((ModuleNumberFilter)filter["Quote #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			((ModuleNumberFilter)filter["Quote #"]).Property = "2";
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 2 ViewQuotedBookings", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking1.PK, quotedBooking3.PK }, collection.Select(element => element.PK));

			((ModuleNumberFilter)filter["Quote #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			((ModuleNumberFilter)filter["Quote #"]).Property = "";
			collection.Load(filter.Filter);
			ZGuid[] expectedPKs = new ZGuid[] { quotedBooking1.PK, quotedBooking2.PK, quotedBooking3.PK };
			AssertEquals("Should have loaded 3 ViewQuotedBookings", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(expectedPKs, collection.Select(element => element.PK));

			((ModuleNumberFilter)filter["Quote #"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(filter.Filter);
			AssertEquals("Should not load any ViewQuotedBookings", 0, collection.Count);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var packLine1 = quotedBooking1.Booking.OuterPackLines.AddNew();
			packLine1.JL_F3_NKPackType = "CTN";
			packLine1.JL_PackageCount = 1;
			var product1 = packLine1.Products.AddNew();
			product1.D2_ProductCode = "AAA1";

			var quotedBooking2 = CreateQuotedBooking();
			var packLine2 = quotedBooking2.Booking.OuterPackLines.AddNew();
			packLine2.JL_F3_NKPackType = "CTN";
			packLine2.JL_PackageCount = 2;
			var product2 = packLine2.Products.AddNew();
			product2.D2_ProductCode = "AAA2";

			Factory.Save();

			FilterStripBusinessObject filter = GetNewFilterStripBusinessObject();
			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(new BusinessObjectFactory());

			((ModuleTextFilter)filter["Product Code"]).IsActive = true;
			((ModuleTextFilter)filter["Product Code"]).Property = "AAA1";
			collection.Load(filter.Filter);
			AssertEquals("StartsWith AAA1 should load the Booking", 1, collection.Count);

			((ModuleTextFilter)filter["Product Code"]).IsActive = true;
			((ModuleTextFilter)filter["Product Code"]).Property = "AAA2";
			collection.Load(filter.Filter);
			AssertEquals("StartsWith AAA2 should load the Booking", 1, collection.Count);

			((ModuleTextFilter)filter["Product Code"]).IsActive = true;
			((ModuleTextFilter)filter["Product Code"]).Property = "AAA";
			collection.Load(filter.Filter);
			AssertEquals("StartsWith AAA should load the 2 Booking", 2, collection.Count);

			((ModuleTextFilter)filter["Product Code"]).IsActive = true;
			((ModuleTextFilter)filter["Product Code"]).Property = "AAA";
			((ModuleTextFilter)filter["Product Code"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection.Load(filter.Filter);
			AssertEquals("Equal AAA should load the Booking", 0, collection.Count);
		}

		#endregion

		#region TestClientContractNumber

		public void TestClientContractNumber()
		{
			using (FreightConfigurationRegistry.Instance.EnableClientContractNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var quotedBooking_123 = CreateQuotedBooking();
				var bookingOnly_123 = CreateBookingOnly();

				quotedBooking_123.TryLoadOrCreateJob();
				bookingOnly_123.TryLoadOrCreateJob();

				quotedBooking_123.Job.JH_ClientContractNumber = "12345";
				bookingOnly_123.Job.JH_ClientContractNumber = "12345";

				var quotedBooking_234 = CreateQuotedBooking();
				var bookingOnly_234 = CreateBookingOnly();

				quotedBooking_234.TryLoadOrCreateJob();
				bookingOnly_234.TryLoadOrCreateJob();

				quotedBooking_234.Job.JH_ClientContractNumber = "23451";
				bookingOnly_234.Job.JH_ClientContractNumber = "23451";

				Factory.Save();

				var results = new ViewQuotedBookingCollection(Factory);
				var filter = (ModuleNumberFilter)FilterStripBizO["Client Contract #"];
				filter.IsActive = true;

				filter.Property = "123";
				results.Load(FilterStripBizO.Filter);
				AssertContainsExactQuotedBookingsInAnyOrder("Should only load 123 quotedBooking and booking", new[] { quotedBooking_123, bookingOnly_123 }, results);

				filter.Property = "234";
				results.Load(FilterStripBizO.Filter);
				AssertContainsExactQuotedBookingsInAnyOrder("Should only load 234 quotedBooking and booking", new[] { quotedBooking_234, bookingOnly_234 }, results);
			}
		}

		public void TestClientContractNumber_NumberIsLoadedFromCompanyContext()
		{
			using (FreightConfigurationRegistry.Instance.EnableClientContractNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var quotedBooking = CreateQuotedBooking();
				var bookingOnly = CreateBookingOnly();

				quotedBooking.TryLoadOrCreateJob();
				bookingOnly.TryLoadOrCreateJob();

				quotedBooking.Job.JH_ClientContractNumber = "12345";
				bookingOnly.Job.JH_ClientContractNumber = "12345";

				Factory.Save();

				var results = new ViewQuotedBookingCollection(Factory);
				var filter = (ModuleNumberFilter)FilterStripBizO["Client Contract #"];
				filter.IsActive = true;

				filter.Property = "123";
				results.Load(FilterStripBizO.Filter);
				AssertContainsExactQuotedBookingsInAnyOrder("Should contain both quotedBooking and booking", new[] { quotedBooking, bookingOnly }, results);

				var newBranch = Factory.NewWithValidTestData<GlbBranch>();
				var department = Factory.NewWithValidTestData<GlbDepartment>();

				Factory.Save();

				var otherFactory = new BusinessObjectFactory();

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), newBranch.PK.ToGuid(), department.PK.ToGuid()))
				{
					var loadedQuotedBooking = otherFactory.Load<QuotedBooking>(quotedBooking.PK);
					loadedQuotedBooking.TryLoadOrCreateJob();

					var loadedBooking = otherFactory.Load<QuotedBooking>(bookingOnly.PK);
					loadedBooking.TryLoadOrCreateJob();

					loadedQuotedBooking.Job.JH_ClientContractNumber = "3456";
					loadedBooking.Job.JH_ClientContractNumber = "3456";

					results = new ViewQuotedBookingCollection(Factory);
					filter = (ModuleNumberFilter)FilterStripBizO["Client Contract #"];
					filter.IsActive = true;
					filter.Property = "123";

					results.Load(FilterStripBizO.Filter);

					AssertContainsExactElementsInAnyOrder("No bookings should be loaded - different context", Array.Empty<ViewQuotedBooking>(), results);

					otherFactory.Save();
				}

				quotedBooking.TryLoadOrCreateJob();
				bookingOnly.TryLoadOrCreateJob();

				results = new ViewQuotedBookingCollection(Factory);
				filter = (ModuleNumberFilter)FilterStripBizO["Client Contract #"];
				filter.IsActive = true;
				filter.Property = "123";

				results.Load(FilterStripBizO.Filter);

				AssertContainsExactQuotedBookingsInAnyOrder("Original bookings should be loaded now that context is back", new[] { quotedBooking, bookingOnly }, results);
			}
		}

		#endregion

		#endregion

		#region Status And Flags

		#region TestActiveStatus

		public void TestActiveStatus()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();
			QuotedBooking quotedBooking4 = CreateQuotedBooking();
			QuotedBooking quoteOnly = CreateQuoteOnly();

			quotedBooking1.Booking.JS_IsCancelled = false;
			quotedBooking1.Quote.TH_IsCancelled = false;
			quotedBooking2.Booking.JS_IsCancelled = false;
			quotedBooking2.Quote.TH_IsCancelled = true;
			quotedBooking3.Booking.JS_IsCancelled = true;
			quotedBooking3.Quote.TH_IsCancelled = false;
			quotedBooking4.Booking.JS_IsCancelled = true;
			quotedBooking4.Quote.TH_IsCancelled = true;
			quoteOnly.Quote.TH_IsCancelled = false;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Active Status"]).Property = OrgConstants.FilterControl.ActiveStatus.Code.ActiveClients;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(otherFactory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 ViewQuotedBookings", 1, collection.Count);
			AssertEquals("Should have QuotedBooking1's Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking1's Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			filter = new QuotedBookingFilterStripBusinessObject();
			((ModuleTextFilter)filter["Active Status"]).Property = OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients;

			collection = new ViewQuotedBookingCollection(otherFactory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 3 ViewQuotedBookings", 3, collection.Count);
			AssertCollectionContains(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking2.Quote.PK), collection);
			AssertCollectionContains(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking3.Quote.PK), collection);
			AssertCollectionContains(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking4.Quote.PK), collection);

			((ModuleTextFilter)filter["Active Status"]).Property = OrgConstants.FilterControl.ActiveStatus.Code.AllClients;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 4 ViewQuotedBookings", 4, collection.Count);

			((ModuleTextFilter)filter["Active Status"]).Property = ZString.Empty;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 4 ViewQuotedBookings", 4, collection.Count);
		}

		#endregion

		#region TestIsHazardous

		public void TestIsHazardous()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var quotedBooking2 = CreateQuotedBooking();

			var packLine1 = quotedBooking1.Booking.OuterPackLines.AddNew();
			var packLine2 = quotedBooking2.Booking.OuterPackLines.AddNew();

			var hazardousCommodity = Factory.New<RefCommodityCode>();
			hazardousCommodity.RH_Code = "DED";
			hazardousCommodity.RH_IsHazardous = true;

			var safeCommodity = Factory.New<RefCommodityCode>();
			safeCommodity.RH_Code = "SAF";
			safeCommodity.RH_IsHazardous = false;

			packLine1.JL_RH_NKCommodityCode = hazardousCommodity.RH_Code;
			packLine2.JL_RH_NKCommodityCode = safeCommodity.RH_Code;

			Factory.Save();

			var results = new ViewQuotedBookingCollection(Factory);
			var shipmentFilter = (ModuleFlagsFilter)FilterStripBizO[BaseQuotedBookingFilterStripBusinessObject.Descriptions.StatusAndFlags.IsHazardous];
			shipmentFilter.IsActive = true;

			shipmentFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering bookings for 'Is Hazardous' = true", () =>
			{
				AssertEquals("Result should contain quotedBooking1 as packLine1 has a hazardous commodity.", true, results.Contains(quotedBooking1.PK));
				AssertEquals("Result should not contain quotedBooking2 as packLine2 has a safe commodity.", false, results.Contains(quotedBooking2.PK));
			});

			shipmentFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = false", () =>
			{
				AssertEquals("Result should not contain quotedBooking1 as packLine1 has a hazardous commodity.", false, results.Contains(quotedBooking1.PK));
				AssertEquals("Result should contain quotedBooking2 as packLine2 has a safe commodity.", true, results.Contains(quotedBooking2.PK));
			});
		}

		#endregion

		#region TestConsolidated/Converted

		public void TestConsolidatedOrConverted()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();
			var quotedBooking4 = CreateQuoteOnly();

			var consol = Factory.New<ForwardingConsol>();
			var buildConsolHelper = new BuildConsolHelper();
			buildConsolHelper.MakeConsolFromBookingOrStandaloneShipment(consol, quotedBooking1.Booking.PK);

			quotedBooking3.Booking.JS_IsForwardRegistered = true;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Consolidated/Converted"]).Property = QuotedBookingFilterStripBusinessObject.ConsolidatedStatus.Code.Cons;
			((ModuleTextFilter)filter["Consolidated/Converted"]).IsActive = true;

			var otherFactory = new BusinessObjectFactory();

			var collection = new ViewQuotedBookingCollection(otherFactory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 ViewQuotedBookings", 2, collection.Count);
			Assert("Should have QuotedBooking1's Quote and Booking", collection.Cast<ViewQuotedBooking>().Any(x => x.QuotedBooking.Quote.PK == quotedBooking1.Quote.PK &&
				x.QuotedBooking.Booking.PK == quotedBooking1.Booking.PK));
			Assert("Should have QuotedBooking3's Quote and Booking", collection.Cast<ViewQuotedBooking>().Any(x => x.QuotedBooking.Quote.PK == quotedBooking3.Quote.PK &&
				x.QuotedBooking.Booking.PK == quotedBooking3.Booking.PK));

			((ModuleTextFilter)filter["Consolidated/Converted"]).Property = QuotedBookingFilterStripBusinessObject.ConsolidatedStatus.Code.Ncons;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 ViewQuotedBookings", 1, collection.Count);
			AssertCollectionContains(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking2.Quote.PK), collection);

			((ModuleTextFilter)filter["Consolidated/Converted"]).Property = QuotedBookingFilterStripBusinessObject.ConsolidatedStatus.Code.All;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 3 ViewQuotedBookings", 3, collection.Count);
		}

		#endregion

		#region TestQuoteAndOrBooking

		public void TestQuoteAndOrBooking()
		{
			QuotedBooking quotedBooking1 = CreateBookingOnly();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuoteOnly();

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Quote And/Or Booking"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);

			((ModuleTextFilter)filter["Quote And/Or Booking"]).Property = "BOO";
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 QuotedBooking", 1, collection.Count);
			AssertEquals(quotedBooking1.PK, collection[0].QuotedBooking.PK);

			((ModuleTextFilter)filter["Quote And/Or Booking"]).Property = "BWQ";
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 ViewQuotedBooking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking2's Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking2's Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleTextFilter)filter["Quote And/Or Booking"]).Property = "BOQ";
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 2 ViewQuotedBookings", 2, collection.Count);
			AssertCollectionContains(new ZQuery(ViewQuotedBookingSchema.VB_JS, quotedBooking1.Booking.PK), collection);
			AssertCollectionContains(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking2.Quote.PK), collection);
		}

		#endregion

		#region Shipment Status Filter

		public void TestShipmentStatusFilter()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();
			var bookingOnly = CreateBookingOnly();

			quotedBooking1.ShipmentStatus = "";
			quotedBooking2.ShipmentStatus = "EBK";
			quotedBooking3.ShipmentStatus = "WEB";
			bookingOnly.ShipmentStatus = "EBK";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Shipment Status"];
			filter.Property = "";
			filter.IsActive = true;

			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			var pks = collection.Select(x => x.PK);

			AssertEquals("Should contain quotedBooking1", true, pks.Contains(quotedBooking1.PK));
			AssertEquals("Should contain quotedBooking2", true, pks.Contains(quotedBooking2.PK));
			AssertEquals("Should contain quotedBooking3", true, pks.Contains(quotedBooking3.PK));
			AssertEquals("should contain bookingOnly", true, pks.Contains(bookingOnly.PK));

			filter.Property = "EBK";
			collection.Load(FilterStripBizO.Filter);
			pks = collection.Select(x => x.PK);

			AssertEquals("Should not contain quotedBooking1", false, pks.Contains(quotedBooking1.PK));
			AssertEquals("Should contain quotedBooking2", true, pks.Contains(quotedBooking2.PK));
			AssertEquals("Should not contain quotedBooking3", false, pks.Contains(quotedBooking3.PK));
			AssertEquals("Should contain bookingOnly", true, pks.Contains(bookingOnly.PK));

			filter.Property = "WEB";
			collection.Load(FilterStripBizO.Filter);
			pks = collection.Select(x => x.PK);

			AssertEquals("Should not contain quotedBooking1", false, pks.Contains(quotedBooking1.PK));
			AssertEquals("Should not contain quotedBooking2", false, pks.Contains(quotedBooking2.PK));
			AssertEquals("Should contain quotedBooking3", true, pks.Contains(quotedBooking3.PK));
			AssertEquals("Should not contain bookingOnly", false, pks.Contains(bookingOnly.PK));
		}

		public void TestShipmentStatusFilter_Available()
		{
			AssertNotNull("Filter is available as registry is on", FilterStripBizO["Shipment Status"]);
		}

		public void TestShipmentStatusList()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var filter = (QuotedBookingFilterStripBusinessObject)FilterStripBizO;
			AssertEquals(shipment.Lookups.JS_ShipmentStatus_List.Count + 6, filter.ShipmentStatusList.Count);

			foreach (CodeDescriptionPair pair in shipment.Lookups.JS_ShipmentStatus_List)
			{
				AssertCollectionContains(pair.Code, filter.ShipmentStatusList.GetAllCodes());
				AssertEquals(pair.Description, filter.ShipmentStatusList.GetDescriptionFromCode(pair.Code));
			}

			AssertCollectionContains(ShipmentStatusList.Codes.WebBooking, filter.ShipmentStatusList.GetAllCodes());
			AssertEquals(ShipmentStatusList.Descriptions.WebBooking, filter.ShipmentStatusList.GetDescriptionFromCode(ShipmentStatusList.Codes.WebBooking));

			AssertCollectionContains(ShipmentStatusList.Codes.EBookingCancellationRequest, filter.ShipmentStatusList.GetAllCodes());
			AssertEquals(ShipmentStatusList.Descriptions.EBookingCancellationRequest, filter.ShipmentStatusList.GetDescriptionFromCode(ShipmentStatusList.Codes.EBookingCancellationRequest));

			AssertCollectionContains(ShipmentStatusList.Codes.BookingCancelled, filter.ShipmentStatusList.GetAllCodes());
			AssertEquals(ShipmentStatusList.Descriptions.BookingCancelled, filter.ShipmentStatusList.GetDescriptionFromCode(ShipmentStatusList.Codes.BookingCancelled));
		}

		#endregion

		protected override string[] GetExpectedFilterDescriptions() => new[]
		{
			"Additional Reference #",
			"Quote #",
			"House Bill #",
			"Booking #",
			"Common Numbers and References",
			"CFS Ref #",
			"Container #",
			"Direct MAWB #",
			"Shippers Ref #",
			"Product Code",
			"Job Local Reference",
			"Flight/Voyage # and Vessel",
			"Active Status",
			"Consolidated/Converted",
			"Shipment Status",
			"Is Hazardous",
			"Quote And/Or Booking",
			"Is Temperature Controlled",
			"Invoicing Job Status",
			"Invoiced / Charges",
			"Additional Ref Num Issue Date",
			"Booking Date",
			"Client Req. ETA",
			"Estimated Pickup",
			"Pickup Required By",
			"Estimated Delivery Date",
			"Delivery Required By",
			"ETD",
			"ETA",
			"Job Open or Close",
			"Job Open",
			"Job Close",
			"Job Revenue Recognition Date",
			"Load / Discharge",
			"Origin / Destination",
			"Delivery Address Post Code",
			"Pickup Address Post Code",
			"Client Name",
			"Sales Representative",
			"Client Related Parties",
			"Consignor Related Parties",
			"Consignee Related Parties",
			"Booking Party Name",
			"Booking Party",
			"Delivery Agent",
			"Pickup Agent",
			"Controlling Customer",
			"Controlling Agent",
			"Import Broker",
			"Export Broker",
			"CFS",
			"Consignor",
			"Consignee",
			"Carrier",
			"Creditor",
			"Client",
			"Pickup Transport",
			"Job Branch",
			"Job Department",
			"Job Operation Staff",
			"Job Sales Staff",
			"Job Branch Management Code",
			"Creating User",
			"Last Edit User",
			"Created Time",
			"Last Edit Time",
			"Created On Web/Internal",
			"Mode",
			"Service Level",
			"DG Class / DG Substance",
			"Service Type / Date Booked",
			"Service Type / Date Completed",
			"HBL Delivery Mode",
			"AP Invoice #",
			"Charges with Debtor",
			"Charges with Creditor",
			"AR Transaction #",
			"Supplier Cost Reference",
			"Job Cost Amount",
			"Job Accrual Amount",
			"Job Profit Amount",
			"Job Revenue Amount",
			"Job WIP Amount",
			"Job WIP Amount (Excluding Deferred Charges)",
			"Job WIP Amount (Deferred Charges Only)",
			"Job Margin %",
			"Jobs with outstanding Accruals",
			"Jobs without any outstanding Accrual",
			"Jobs with outstanding WIPs",
			"Jobs without any outstanding WIP",
			"Milestone Date",
			"Milestone Completed",
			"Next Milestone",
			"Last Completed Milestone",
			"Any Open Task Assigned To",
			"Next Task Assigned To",
			"Tasks",
			"Exceptions",
			"Milestones",
			"Triggers",
			"Custom SQL Filter",
			"Company Tariff Level Override",
			"FMC Tariff ID",
			"Commodity Code",
			"Transport Mode",
			"Container Mode",
			"Profit/Loss Reason",
			"CO2e (kg)",
		};

		#endregion

		#region Dates

		#region TestBookingDate

		public void TestBookingDate()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_A_BKD = ZDateTime.Now.AddDays(-2);
			quotedBooking2.Booking.JS_A_BKD = ZDateTime.Now;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Booking Date"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Booking Date"]).Property2 = ZDateTime.Now.AddDays(+1);
			((ModuleDateFilter)filter["Booking Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Booking Date"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

			((ModuleDateFilter)filter["Booking Date"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Booking Date"]).Property2 = ZDateTime.Now.AddDays(-1);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Booking Date"]).Property1 = ZDateTime.Now.AddDays(-1);
			((ModuleDateFilter)filter["Booking Date"]).Property2 = ZDateTime.Now.AddDays(+1);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Booking Date"]).Property1 = ZDateTime.Now.AddDays(-5);
			((ModuleDateFilter)filter["Booking Date"]).Property2 = ZDateTime.Now.AddDays(-3);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);
		}

		#endregion

		#region TestClientReqETA

		public void TestClientReqETA()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_ClientRequestedETA = ZDateTime.Now.AddDays(-2);
			quotedBooking2.Booking.JS_ClientRequestedETA = ZDateTime.Now;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Client Req. ETA"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Client Req. ETA"]).Property2 = ZDateTime.Now.AddDays(+1);
			((ModuleDateFilter)filter["Client Req. ETA"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Client Req. ETA"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

			((ModuleDateFilter)filter["Client Req. ETA"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Client Req. ETA"]).Property2 = ZDateTime.Now.AddDays(-1);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Client Req. ETA"]).Property1 = ZDateTime.Now.AddDays(-1);
			((ModuleDateFilter)filter["Client Req. ETA"]).Property2 = ZDateTime.Now.AddDays(+1);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Client Req. ETA"]).Property1 = ZDateTime.Now.AddDays(-5);
			((ModuleDateFilter)filter["Client Req. ETA"]).Property2 = ZDateTime.Now.AddDays(-3);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Bookings", 0, collection.Count);
		}

		#endregion

		#region TestEstimatedPickup

		public void TestEstimatedPickup()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();

			quotedBooking1.Booking.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Now.AddDays(-2);
			quotedBooking2.Booking.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Now;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Estimated Pickup"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Estimated Pickup"]).Property2 = ZDateTime.Now.AddDays(+1);
			((ModuleDateFilter)filter["Estimated Pickup"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Estimated Pickup"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

			((ModuleDateFilter)filter["Estimated Pickup"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Estimated Pickup"]).Property2 = ZDateTime.Now.AddDays(-1);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Estimated Pickup"]).Property1 = ZDateTime.Now.AddDays(-1);
			((ModuleDateFilter)filter["Estimated Pickup"]).Property2 = ZDateTime.Now.AddDays(+1);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Estimated Pickup"]).Property1 = ZDateTime.Now.AddDays(-5);
			((ModuleDateFilter)filter["Estimated Pickup"]).Property2 = ZDateTime.Now.AddDays(-3);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);
		}

		#endregion

		#region TestPickupRequiredBy

		public void TestPickupRequiredBy()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();

			quotedBooking1.Booking.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Now.AddDays(-2);
			quotedBooking2.Booking.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Now;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Pickup Required By"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Pickup Required By"]).Property2 = ZDateTime.Now.AddDays(+1);
			((ModuleDateFilter)filter["Pickup Required By"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Pickup Required By"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

			((ModuleDateFilter)filter["Pickup Required By"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Pickup Required By"]).Property2 = ZDateTime.Now.AddDays(-1);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Pickup Required By"]).Property1 = ZDateTime.Now.AddDays(-1);
			((ModuleDateFilter)filter["Pickup Required By"]).Property2 = ZDateTime.Now.AddDays(+1);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Pickup Required By"]).Property1 = ZDateTime.Now.AddDays(-5);
			((ModuleDateFilter)filter["Pickup Required By"]).Property2 = ZDateTime.Now.AddDays(-3);

			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);
		}

		#endregion

		#region TestEstimatedDeliveryDate(

		public void TestEstimatedDeliveryDate()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();

			quotedBooking1.Booking.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now.AddDays(-2);
			quotedBooking2.Booking.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Now;

			Factory.Save();

			QuotedBookingFilterStripBusinessObject filter = new QuotedBookingFilterStripBusinessObject();
			((ModuleDateFilter)filter["Estimated Delivery Date"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Estimated Delivery Date"]).Property2 = ZDateTime.Now.AddDays(+1);
			((ModuleDateFilter)filter["Estimated Delivery Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Estimated Delivery Date"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

			((ModuleDateFilter)filter["Estimated Delivery Date"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Estimated Delivery Date"]).Property2 = ZDateTime.Now.AddDays(-1);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Estimated Delivery Date"]).Property1 = ZDateTime.Now.AddDays(-1);
			((ModuleDateFilter)filter["Estimated Delivery Date"]).Property2 = ZDateTime.Now.AddDays(+1);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Estimated Delivery Date"]).Property1 = ZDateTime.Now.AddDays(-5);
			((ModuleDateFilter)filter["Estimated Delivery Date"]).Property2 = ZDateTime.Now.AddDays(-3);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);
		}

		#endregion

		#region TestDeliveryRequiredBy

		public void TestDeliveryRequiredBy()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();

			quotedBooking1.Booking.DocsAndCartage.JP_DeliveryRequiredBy = ZDateTime.Now.AddDays(-2);
			quotedBooking2.Booking.DocsAndCartage.JP_DeliveryRequiredBy = ZDateTime.Now;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["Delivery Required By"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Delivery Required By"]).Property2 = ZDateTime.Now.AddDays(+1);
			((ModuleDateFilter)filter["Delivery Required By"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Delivery Required By"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

			((ModuleDateFilter)filter["Delivery Required By"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["Delivery Required By"]).Property2 = ZDateTime.Now.AddDays(-1);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Delivery Required By"]).Property1 = ZDateTime.Now.AddDays(-1);
			((ModuleDateFilter)filter["Delivery Required By"]).Property2 = ZDateTime.Now.AddDays(+1);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["Delivery Required By"]).Property1 = ZDateTime.Now.AddDays(-5);
			((ModuleDateFilter)filter["Delivery Required By"]).Property2 = ZDateTime.Now.AddDays(-3);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);
		}

		#endregion

		#region TestETD

		public void TestETD()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_E_DEP = ZDateTime.Now.AddDays(-2);
			quotedBooking2.Booking.JS_E_DEP = ZDateTime.Now;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["ETD"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["ETD"]).Property2 = ZDateTime.Now.AddDays(+1);
			((ModuleDateFilter)filter["ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["ETD"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

			((ModuleDateFilter)filter["ETD"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["ETD"]).Property2 = ZDateTime.Now.AddDays(-1);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["ETD"]).Property1 = ZDateTime.Now.AddDays(-1);
			((ModuleDateFilter)filter["ETD"]).Property2 = ZDateTime.Now.AddDays(+1);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["ETD"]).Property1 = ZDateTime.Now.AddDays(-5);
			((ModuleDateFilter)filter["ETD"]).Property2 = ZDateTime.Now.AddDays(-3);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);
		}

		#endregion

		#region TestETA

		public void TestETA()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_E_ARV = ZDateTime.Now.AddDays(-2);
			quotedBooking2.Booking.JS_E_ARV = ZDateTime.Now;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter["ETA"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["ETA"]).Property2 = ZDateTime.Now.AddDays(+1);
			((ModuleDateFilter)filter["ETA"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["ETA"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

			((ModuleDateFilter)filter["ETA"]).Property1 = ZDateTime.Now.AddDays(-3);
			((ModuleDateFilter)filter["ETA"]).Property2 = ZDateTime.Now.AddDays(-1);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["ETA"]).Property1 = ZDateTime.Now.AddDays(-1);
			((ModuleDateFilter)filter["ETA"]).Property2 = ZDateTime.Now.AddDays(+1);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleDateFilter)filter["ETA"]).Property1 = ZDateTime.Now.AddDays(-5);
			((ModuleDateFilter)filter["ETA"]).Property2 = ZDateTime.Now.AddDays(-3);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);
		}

		#endregion

		#region TestDeliveryDueDate

		public void TestDeliveryDueDateAvailability()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var filter = GetNewFilterStripBusinessObject();
				AssertNotNull("Delivery Due Date should be available when CalculateDeliveryDueDate registry is enabled", filter["Delivery Due Date"]);
			}
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var filter = GetNewFilterStripBusinessObject();
				AssertNull("Delivery Due Date should be available when CalculateDeliveryDueDate registry is disabled", filter["Delivery Due Date"]);
			}
		}
		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		public void TestDeliveryDueDate()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var quotedBooking1 = CreateQuotedBooking();
				var quotedBooking2 = CreateQuotedBooking();

				quotedBooking1.Booking.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "mock reason";
				quotedBooking2.Booking.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "mock reason";

				quotedBooking1.Booking.JS_DeliveryDueDate = ZDateTime.Now.AddDays(-2);
				quotedBooking2.Booking.JS_DeliveryDueDate = ZDateTime.Now;

				Factory.Save();

				var filter = GetNewFilterStripBusinessObject();
				var deliveryDueDateFilter = (ModuleDateFilter)filter["Delivery Due Date"];
				AssertNotNull("Pre-condition: Delivery Due Date filter should exist", deliveryDueDateFilter);
				deliveryDueDateFilter.Property1 = ZDateTime.Now.AddDays(-3);
				deliveryDueDateFilter.Property2 = ZDateTime.Now.AddDays(+1);
				deliveryDueDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				deliveryDueDateFilter.IsActive = true;

				var collection = new ViewQuotedBookingCollection(Factory);
				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

				deliveryDueDateFilter.Property1 = ZDateTime.Now.AddDays(-3);
				deliveryDueDateFilter.Property2 = ZDateTime.Now.AddDays(-1);
				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
				AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
				AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

				deliveryDueDateFilter.Property1 = ZDateTime.Now.AddDays(-1);
				deliveryDueDateFilter.Property2 = ZDateTime.Now.AddDays(+1);
				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
				AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
				AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

				deliveryDueDateFilter.Property1 = ZDateTime.Now.AddDays(-5);
				deliveryDueDateFilter.Property2 = ZDateTime.Now.AddDays(-3);
				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);
			}
		}

		#endregion

		public void TestAdditionalReferenceNumberIssueDate()
		{
			CustomsReferenceNumberTypeCollection customsReferenceNumberTypeCollection = new CustomsReferenceNumberTypeCollection();
			customsReferenceNumberTypeCollection.Add("AAA", (NoResString)"AAA code");
			customsReferenceNumberTypeCollection.Add("BBB", (NoResString)"BBB code");

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customsReferenceNumberTypeCollection);

			QuotedBooking quotedBooking1 = CreateQuotedBooking();

			CusEntryNumber number = quotedBooking1.Booking.Numbers.AddNew();
			number.CE_EntryType = "AAA";
			number.CE_EntryNum = "11111";
			number.CE_IssueDate = new ZDateTime(2011, 5, 10);

			number = quotedBooking1.Booking.Numbers.AddNew();
			number.CE_EntryType = "BBB";
			number.CE_EntryNum = "22222";

			QuotedBooking quotedBooking2 = CreateBookingOnly();

			number = quotedBooking2.Booking.Numbers.AddNew();
			number.CE_EntryType = "BBB";
			number.CE_EntryNum = "22222";
			number.CE_IssueDate = new ZDateTime(2011, 5, 20);

			QuotedBooking quotedBooking3 = CreateBookingOnly();
			number = quotedBooking3.Booking.Numbers.AddNew();
			number.CE_EntryType = "BBB";
			number.CE_EntryNum = "22222";

			QuotedBooking quotedBooking4 = CreateBookingOnly();
			QuotedBooking quotedBooking5 = CreateQuoteOnly();

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleDateFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.Dates.AdditionalRefNumIssueDate]).PropertySearch = ModuleDateFilter.HasDateEntered;
			((ModuleDateFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.Dates.AdditionalRefNumIssueDate]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.PK, quotedBooking2.PK }, collection.Select((elem) => elem.PK));

			((ModuleDateFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.Dates.AdditionalRefNumIssueDate]).PropertySearch = ModuleDateFilter.HasNoDateEntered;
			collection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.PK, quotedBooking3.PK }, collection.Select((elem) => elem.PK));

			((ModuleDateFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.Dates.AdditionalRefNumIssueDate]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.Dates.AdditionalRefNumIssueDate]).Property1 = ZDateTime.Invalid;
			((ModuleDateFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.Dates.AdditionalRefNumIssueDate]).Property2 = ZDateTime.Invalid;
			collection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.PK, quotedBooking2.PK, quotedBooking3.PK }, collection.Select((elem) => elem.PK));

			((ModuleDateFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.Dates.AdditionalRefNumIssueDate]).Property1 = new ZDateTime(2011, 5, 10);
			((ModuleDateFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.Dates.AdditionalRefNumIssueDate]).Property2 = new ZDateTime(2011, 5, 20);
			collection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.PK, quotedBooking2.PK }, collection.Select((elem) => elem.PK));

			((ModuleDateFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.Dates.AdditionalRefNumIssueDate]).Property1 = new ZDateTime(2011, 6, 10);
			((ModuleDateFilter)filter[QuotedBookingFilterStripBusinessObject.Descriptions.Dates.AdditionalRefNumIssueDate]).Property2 = new ZDateTime(2011, 6, 20);
			collection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(Array.Empty<ZGuid>(), collection.Select((elem) => elem.PK));
		}

		#endregion

		#region Locations

		#region TestLoadDischarge

		public void TestLoadDischarge()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();
			var quotedBooking4 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_RL_NKLoadPort = "AUSYD";
			quotedBooking1.Booking.JS_RL_NKDischargePort = "CNSHA";
			quotedBooking2.Booking.JS_RL_NKLoadPort = "AUSYD";
			quotedBooking2.Booking.JS_RL_NKDischargePort = "NZAKL";
			quotedBooking3.Booking.JS_RL_NKLoadPort = "CNSHA";
			quotedBooking3.Booking.JS_RL_NKDischargePort = "NZAKL";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var collection = new ViewQuotedBookingCollection(Factory);

			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 4 View Quoted Booking", 4, collection.Count);

			((ModuleLocationFilter)filter["Load / Discharge"]).Property1 = "AUSYD";
			((ModuleLocationFilter)filter["Load / Discharge"]).IsActive = true;
			collection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking1.PK, quotedBooking2.PK }, collection.Select(element => element.PK));

			((ModuleLocationFilter)filter["Load / Discharge"]).Property2 = "NZAKL";
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleLocationFilter)filter["Load / Discharge"]).Property1 = "";
			collection.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking2.PK, quotedBooking3.PK }, collection.Select(element => element.PK));
		}

		#endregion

		#region TestOriginDestination

		public void TestOriginDestination()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_RL_NKOrigin = "AUSYD";
			quotedBooking1.Booking.JS_RL_NKDestination = "USLAX";
			quotedBooking2.Booking.JS_RL_NKOrigin = "AUBNE";
			quotedBooking2.Booking.JS_RL_NKDestination = "SGSIN";

			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var inbom = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "INBOM");

			OrgSales testSale1 = TestOrg.SalesCollection.AddNew();
			testSale1.OW_OriginID = uslax.PK;
			testSale1.OW_DestinationID = inbom.PK;
			testSale1.OW_OH_Buyer = TestOrg.PK;
			OrgTradeDetail detail1 = testSale1.TradeDetails.AddNew();
			detail1.PA_TradeMode = "AIR";
			detail1.PA_TradeType = "FCL";
			detail1.ProspectDetail.PAP_RS_NKServiceLevel = "D2D";
			detail1.ProspectDetail.PAP_RH_NKCommodityCode = "REF";

			OrgSales testSale2 = TestOrg.SalesCollection.AddNew();
			testSale2.OW_OriginID = inbom.PK;
			testSale2.OW_DestinationID = uslax.PK;
			testSale2.OW_OH_Buyer = TestOrg.PK;
			OrgTradeDetail detail2 = testSale2.TradeDetails.AddNew();
			detail2.PA_TradeMode = "AIR";
			detail2.PA_TradeType = "LCL";
			detail2.ProspectDetail.PAP_RS_NKServiceLevel = "D2D";
			detail2.ProspectDetail.PAP_RH_NKCommodityCode = "REF";

			quotedBooking1.Quote.ImportTradeDetailData(detail1);
			quotedBooking2.Quote.ImportTradeDetailData(detail2);

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleLocationFilter)filter["Origin / Destination"]).Property1 = "AUSYD";
			((ModuleLocationFilter)filter["Origin / Destination"]).IsActive = true;

			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleLocationFilter)filter["Origin / Destination"]).Property1 = "AUSYD";
			((ModuleLocationFilter)filter["Origin / Destination"]).Property2 = "USLAX";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleLocationFilter)filter["Origin / Destination"]).Property1 = "AU";
			((ModuleLocationFilter)filter["Origin / Destination"]).Property2 = "";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

			((ModuleLocationFilter)filter["Origin / Destination"]).Property1 = "AU";
			((ModuleLocationFilter)filter["Origin / Destination"]).Property2 = "SGSIN";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleLocationFilter)filter["Origin / Destination"]).Property1 = "AUSR";
			((ModuleLocationFilter)filter["Origin / Destination"]).Property2 = "";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);

			((ModuleLocationFilter)filter["Origin / Destination"]).Property1 = "AUSR";
			((ModuleLocationFilter)filter["Origin / Destination"]).Property2 = "USAR";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);
		}

		#endregion

		#endregion

		#region Billing Filters

		public void TestAPInvoiceNumberFilter()
		{
			AssertNotNull(FilterStripBizO["AP Invoice #"]);

			QuotedBooking quotedBooking1 = CreateBookingOnly();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuoteOnly();
			QuotedBooking quotedBooking4 = CreateQuotedBooking();
			QuotedBooking quotedBooking5 = CreateBookingOnly();

			JobHeader quotedBooking1Job = AddJobForBusinessObject(quotedBooking1.Booking);
			AddTransactionLineToBusinessObject(quotedBooking1Job, LedgerTypes.AccountsPayable, "00001001");
			AddTransactionLineToBusinessObject(quotedBooking2.Job, LedgerTypes.AccountsPayable, "00001002");
			AddTransactionLineToBusinessObject(quotedBooking3.Job, LedgerTypes.AccountsPayable, "QUOTE001");

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStripBizO["AP Invoice #"];
			filter.IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			ZGuid[] expected = new[] { quotedBooking1.PK, quotedBooking2.PK, quotedBooking4.PK, quotedBooking5.PK };
			ZGuid[] actual = collection.Select(x => x.PK).ToArray();
			AssertContainsExactElementsInAnyOrder("Expected to return all quoted bookings by default except One Off Quote", expected, actual);

			filter.Property = "00001001";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(quotedBooking1.PK));

			filter.Property = "00001002";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(quotedBooking2.PK));

			filter.Property = "QUOTE001";
			collection.Load(FilterStripBizO.Filter);

			AssertEquals(false, collection.Contains(quotedBooking3.PK));

			filter.Property = "00001005";
			collection.Load(FilterStripBizO.Filter);

			Assert("There should not be any matching quoted bookings", !collection.Any());

			filter.Property = "00001";
			collection.Load(FilterStripBizO.Filter);

			expected = new[] { quotedBooking1.PK, quotedBooking2.PK };
			actual = collection.Select(x => x.PK).ToArray();

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestARTransactionNumberFilter()
		{
			AssertNotNull(FilterStripBizO["AR Transaction #"]);

			QuotedBooking quotedBooking1 = CreateBookingOnly();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuoteOnly();
			QuotedBooking quotedBooking4 = CreateQuotedBooking();
			QuotedBooking quotedBooking5 = CreateBookingOnly();

			JobHeader quotedBooking1Job = AddJobForBusinessObject(quotedBooking1.Booking);
			AddTransactionLineToBusinessObject(quotedBooking1Job, LedgerTypes.AccountsReceivable, "00001003");
			AddTransactionLineToBusinessObject(quotedBooking2.Job, LedgerTypes.AccountsReceivable, "00001004");
			AddTransactionLineToBusinessObject(quotedBooking4.Job, LedgerTypes.AccountsReceivable, "QUOTE002");

			Factory.Save();

			ModuleFountainFilter filter = (ModuleFountainFilter)FilterStripBizO["AR Transaction #"];
			filter.IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			ZGuid[] expected = new[] { quotedBooking1.PK, quotedBooking2.PK, quotedBooking4.PK, quotedBooking5.PK };
			ZGuid[] actual = collection.Select(x => x.PK).ToArray();
			AssertContainsExactElementsInAnyOrder("Expected to return all quoted bookings except OneOffQuote by default", expected, actual);

			filter.Property = "00001003";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(quotedBooking1.PK));

			filter.Property = "00001004";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(quotedBooking2.PK));

			filter.Property = "QUOTE002";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(quotedBooking4.PK));

			filter.Property = "00001005";
			collection.Load(FilterStripBizO.Filter);

			Assert("There should not be any matching quoted bookings", !collection.Any());
		}

		public void TestSupplierCostReferenceFilter()
		{
			AssertNotNull(FilterStripBizO["Supplier Cost Reference"]);

			QuotedBooking quotedBooking1 = CreateBookingOnly();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuoteOnly();
			QuotedBooking quotedBooking4 = CreateBookingOnly();
			QuotedBooking quotedBooking5 = CreateQuotedBooking();

			JobHeader quotedBooking1Job = AddJobForBusinessObject(quotedBooking1.Booking);
			JobHeader quotedBooking4Job = AddJobForBusinessObject(quotedBooking4.Booking);
			AddTransactionLineToBusinessObject(quotedBooking1Job, LedgerTypes.AccountsPayable, "00001001", "Pending");
			AddTransactionLineToBusinessObject(quotedBooking2.Job, LedgerTypes.AccountsPayable, "00001002", "Pending tax");
			AddTransactionLineToBusinessObject(quotedBooking4Job, LedgerTypes.AccountsPayable, "00001004", "784-152");
			AddTransactionLineToBusinessObject(quotedBooking1Job, LedgerTypes.AccountsReceivable, "00002001", "BORK");
			AddTransactionLineToBusinessObject(quotedBooking3.Job, LedgerTypes.AccountsReceivable, "00002003", "Pending");
			AddTransactionLineToBusinessObject(quotedBooking4Job, LedgerTypes.AccountsPayable, "00002004");

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStripBizO["Supplier Cost Reference"];
			filter.IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			ZGuid[] expected = new[] { quotedBooking1.PK, quotedBooking2.PK, quotedBooking4.PK, quotedBooking5.PK };
			ZGuid[] actual = collection.Select(x => x.PK).ToArray();
			AssertContainsExactElementsInAnyOrder("Expected to return all quoted bookings except One Off Quote by default", expected, actual);

			filter.Property = "Pending";
			collection.Load(FilterStripBizO.Filter);

			expected = new[] { quotedBooking1.PK, quotedBooking2.PK };
			actual = collection.Select(x => x.PK).ToArray();
			AssertContainsExactElementsInAnyOrder(expected, actual);

			filter.Property = "Pending tax";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(quotedBooking2.PK));

			filter.Property = "Bork";
			collection.Load(FilterStripBizO.Filter);

			Assert(collection.Contains(quotedBooking1.PK));

			filter.Property = "invalid";
			collection.Load(FilterStripBizO.Filter);

			Assert("There should not be any matching quoted bookings", !collection.Any());
		}

		#endregion

		#region Organisations Staff

		#region TestSalesRep

		public void TestSalesRep()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "AAA";

			var booking1 = CreateBookingOnly();
			var booking2 = CreateBookingOnly();
			var booking3 = CreateBookingOnly();

			var quotedBooking1 = CreateQuotedBooking();
			var quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();

			var quote1 = CreateQuoteOnly();
			var quote2 = CreateQuoteOnly();
			var quote3 = CreateQuoteOnly();

			booking1.TryLoadOrCreateJob();
			booking2.TryLoadOrCreateJob();
			booking3.TryLoadOrCreateJob();

			quotedBooking1.TryLoadOrCreateJob();
			quotedBooking2.TryLoadOrCreateJob();
			quotedBooking3.TryLoadOrCreateJob();

			quote1.TryLoadOrCreateJob();
			quote2.TryLoadOrCreateJob();
			quote3.TryLoadOrCreateJob();

			booking1.Job.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			booking2.Job.JH_GS_NKRepSales = ZString.Empty;
			booking3.Job.JH_GS_NKRepSales = staff1.GS_Code;

			quotedBooking1.Job.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			quotedBooking2.Job.JH_GS_NKRepSales = ZString.Empty;
			quotedBooking3.Job.JH_GS_NKRepSales = staff1.GS_Code;

			quote1.Job.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			quote2.Job.JH_GS_NKRepSales = ZString.Empty;
			quote3.Job.JH_GS_NKRepSales = staff1.GS_Code;

			Factory.Save();

			var collection = new ViewQuotedBookingCollection(Factory);
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterStrip[BaseQuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.SalesRepresentative];
			filter.IsActive = true;

			#region Exact comparison
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
			filter.Property = staff1.GS_Code;
			collection.Load(filterStrip.Filter);
			AssertContainsExactQuotedBookingsInAnyOrder("Should match bookings with SalesRep = staff1", new QuotedBooking[] { booking3, quotedBooking3 }, collection);

			filter.Property = GlbStaff.CurrentUser.GS_Code;
			collection.Load(filterStrip.Filter);
			AssertContainsExactQuotedBookingsInAnyOrder("Should match bookings with SalesRep = current user", new QuotedBooking[] { booking1, quotedBooking1 }, collection);
			#endregion

			#region IsBlank comparison
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsBlank;
			filter.Property = ZString.Empty;
			collection.Load(filterStrip.Filter);
			AssertContainsExactQuotedBookingsInAnyOrder("Should match bookings with blank SalesRep", new QuotedBooking[] { booking2, quotedBooking2 }, collection);
			#endregion

			#region IsNotBlank comparison
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsNotBlank;
			filter.Property = ZString.Empty;
			collection.Load(filterStrip.Filter);
			AssertContainsExactQuotedBookingsInAnyOrder("Should match bookings with SalesRep that is not blank", new QuotedBooking[] { booking1, booking3, quotedBooking1, quotedBooking3 }, collection);
			#endregion

			#region NotEqual comparison
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.NotEqual;
			filter.Property = staff1.GS_Code;
			collection.Load(filterStrip.Filter);
			AssertContainsExactQuotedBookingsInAnyOrder("Should match bookings with SalesRep != staff1", new QuotedBooking[] { booking1, booking2, quotedBooking1, quotedBooking2 }, collection);
			#endregion

			#region CurrentUser comparison
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.CurrentUser;
			filter.Property = ZString.Empty;
			collection.Load(filterStrip.Filter);
			AssertContainsExactQuotedBookingsInAnyOrder("Should match bookings with SalesRep = current user", new QuotedBooking[] { booking1, quotedBooking1 }, collection);
			#endregion

			#region FiltersMatch comparison
			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", staff1.GS_Code);
			collection.Load(filterStrip.Filter);
			AssertContainsExactQuotedBookingsInAnyOrder("Should match bookings with SalesRep = staff1", new QuotedBooking[] { booking3, quotedBooking3 }, collection);
			#endregion
		}

		public void TestSalesRep_AllowedComparisonOperators()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			ModuleNkFilter filter = (ModuleNkFilter)filterStrip[QuotedBookingFilterStripBusinessObject.Descriptions.OrganisationsStaff.SalesRepresentative];
			AssertEquals("The Sales Rep filter should use comparison operators", true, filter.HasComparisonOperator);
			string[] expectedOperators = new string[7];
			expectedOperators[0] = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			expectedOperators[1] = ModuleTextFilter.ComparisonConstants.IsBlank;
			expectedOperators[2] = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			expectedOperators[3] = ModuleTextFilter.ComparisonConstants.NotEqual;
			expectedOperators[4] = ModuleTextFilter.ComparisonConstants.Exact;
			expectedOperators[5] = ModuleTextFilter.ComparisonConstants.CurrentUser;
			expectedOperators[6] = string.Empty;
			AssertContainsExactElementsInAnyOrder(expectedOperators, filter.AllowedComparisonOperators);
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();
			Quote quote = Factory.New<Quote>();
			quote.TH_OneTimeQuote = true;

			quotedBooking1.Booking.JS_OA_BookedShippingLineAddress = TestOrg.MainAddress.PK;
			quotedBooking2.Booking.JS_OA_BookedShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			quotedBooking3.Booking.JS_OA_BookedShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			quotedBooking3.Quote.CurrentOneOffQuote.TT_OH_Carrier = TestOrg.PK;
			quote.CurrentOneOffQuote.TT_OH_Carrier = TestOrg.PK;

			Factory.Save();

			QuotedBookingFilterStripBusinessObject filter = new QuotedBookingFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Carrier"]).Property = TestOrg.PK;
			((ModuleGuidFilter)filter["Carrier"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should only contain quotedBooking1 and quote", quotedBooking1.PK, collection[0].PK);

			((ModuleGuidFilter)filter["Carrier"]).ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			collection.Load(filter.Filter);
			AssertEquals("Should contain quotedBooking2 and 3", 2, collection.Count);
		}

		public void TestCarrier_BlankOperators()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();
			Quote quote1 = Factory.New<Quote>();
			quote1.TH_OneTimeQuote = true;
			Quote quote2 = Factory.New<Quote>();
			quote2.TH_OneTimeQuote = true;

			quotedBooking1.Booking.JS_OA_BookedShippingLineAddress = TestOrg.MainAddress.PK;
			quotedBooking2.Booking.JS_OA_BookedShippingLineAddress = TestOrg2.MainAddress.PK;
			quote1.CurrentOneOffQuote.TT_OH_Carrier = TestOrg.PK;
			quote2.CurrentOneOffQuote.TT_OH_Carrier = TestOrg2.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var orgFilter = (ModuleGuidFilter)filter["Carrier"];
			orgFilter.Property = ZGuid.Empty;
			orgFilter.IsActive = true;
			orgFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 3 Quote", quotedBooking3.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			orgFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			var quotePks = GetQuotePks(collection);
			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, quotePks);
		}

		#endregion

		#region TestClient

		public void TestClient()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking(TestOrg.PK);
			QuotedBooking quotedBooking2 = CreateQuotedBooking(TestOrg2.PK);

			quotedBooking1.Job.LocalChargesPK = TestOrg.PK;
			quotedBooking2.Quote.TH_OH = TestOrg2.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Client"]).Property = TestOrg.PK;
			((ModuleGuidFilter)filter["Client"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleGuidFilter)filter["Client"]).Property = TestOrg2.PK;
			((ModuleGuidFilter)filter["Client"]).IsActive = true;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleGuidFilter)filter["Client"]).ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);
		}

		public void TestClient_BlankOperators()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking(TestOrg.PK);
			QuotedBooking quotedBooking2 = CreateQuotedBooking(TestOrg2.PK);
			QuotedBooking quotedBooking3 = CreateQuotedBooking(TestOrg3.PK);

			quotedBooking1.Job.LocalChargesPK = TestOrg.PK;
			quotedBooking2.Quote.TH_OH = TestOrg2.PK;
			quotedBooking3.Job.LocalChargesPK = ZGuid.Empty;
			quotedBooking3.Quote.TH_OH = ZGuid.Empty;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Client"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)filter["Client"]).IsActive = true;
			((ModuleGuidFilter)filter["Client"]).ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have quotedBooking3 Quote", quotedBooking3.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			((ModuleGuidFilter)filter["Client"]).ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsNotBlank;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, GetQuotePks(collection));
		}

		#endregion

		#region TestClientName

		public void TestClientNameUseEqual()
		{
			var data = GetClientNameData();

			var property = "Test Client #1";
			var message = "Should have loaded 3 View Quoted Bookings";
			var expected = data["Client1"].Where(x => x.Booking != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Equal, message, expected);

			property = "Test Client #2";
			expected = data["Client2"].Where(x => x.Booking != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Equal, message, expected);

			property = "Test Client";
			message = "Should have loaded nothing.";
			expected = Array.Empty<QuotedBooking>();

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Equal, message, expected);

			property = string.Empty;
			message = "Should have loaded 7 View Quoted Bookings";
			expected = data.Values.SelectMany(vale => vale).Where(x => x.Booking != null).ToArray();

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Equal, message, expected);
		}

		public void TestClientNameUseNotEqual()
		{
			var data = GetClientNameData();

			var property = "Test Client #1";
			var message = "Should have loaded 4 View Quoted Bookings";
			var expected = data["Client2"].Union(data["Empty"]).Where(x => x.Booking != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotEqual, message, expected);

			property = "Test Client #2";
			expected = data["Client1"].Union(data["Empty"]).Where(x => x.Booking != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotEqual, message, expected);

			property = "Test Client";
			message = "Should have loaded 7 View Quoted Bookings";
			expected = data.Values.SelectMany(vale => vale).Where(x => x.Booking != null).ToArray();

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotEqual, message, expected);

			property = string.Empty;
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotEqual, message, expected);
		}

		public void TestClientNameUseContainsAndStartWith()
		{
			var data = GetClientNameData();

			var property = "Test Client #1";
			var message = "Should have loaded 3 View Quoted Bookings excluding One Off Quote";
			IEnumerable<QuotedBooking> expected = data["Client1"].Where(x => x.Booking != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Contains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.StartsWith, message, expected);

			property = "Test Client #2";
			expected = data["Client2"].Where(x => x.Booking != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Contains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.StartsWith, message, expected);

			property = "Test Client";
			message = "Should have loaded 6 View Quoted Bookings";
			expected = data["Client1"].Union(data["Client2"]).Where(x => x.Booking != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Contains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.StartsWith, message, expected);

			property = "AnyAddress";
			message = "Should have loaded nothing.";
			expected = Array.Empty<QuotedBooking>();

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.Contains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.StartsWith, message, expected);
		}

		public void TestClientNameUseNotContainsAndDoesNotStartWith()
		{
			var data = GetClientNameData();

			var property = "Test Client #1";
			var message = "Should have loaded 4 View Quoted Bookings";
			var expected = data["Client2"].Union(data["Empty"]).Where(x => x.Booking != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotContains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.DoesNotStartWith, message, expected);

			property = "Test Client #2";
			expected = data["Client1"].Union(data["Empty"]).Where(x => x.Booking != null);

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotContains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.DoesNotStartWith, message, expected);

			property = "Test Client";
			message = "Should have loaded 1 View Quoted Booking";
			expected = data["Empty"];

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotContains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.DoesNotStartWith, message, expected);

			property = "AnyAddress";
			expected = data.Values.SelectMany(vale => vale).Where(x => x.Booking != null).ToArray();
			message = "Should have loaded 7 View Quoted Bookings";

			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.NotContains, message, expected);
			CheckCollectionForUseClientNameFilter(property, SQLComparisonOperator.DoesNotStartWith, message, expected);
		}

		public void TestClientNameUseIsBlankAndIsNotBlank()
		{
			var data = GetClientNameData();

			var message = "Should have loaded 1 View Quoted Booking";
			IEnumerable<QuotedBooking> expected = data["Empty"];

			CheckCollectionForUseClientNameFilter(string.Empty, SpecialComparisonOperator.IsBlank, message, expected);

			message = "Should have loaded 6 View Quoted Bookings";
			expected = data["Client1"].Union(data["Client2"]).Where(x => x.Booking != null);

			CheckCollectionForUseClientNameFilter(string.Empty, SpecialComparisonOperator.IsNotBlank, message, expected);
		}

		#endregion

		#region TestBookingParty

		public void TestBookingParty()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking(TestOrg.PK);
			QuotedBooking quotedBooking2 = CreateQuotedBooking(TestOrg2.PK);

			quotedBooking1.BookingPartyDocumentaryAddress.E2_OA_Address = TestOrg.MainAddress.PK;
			quotedBooking2.BookingPartyDocumentaryAddress.E2_OA_Address = TestOrg2.MainAddress.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Booking Party"]).Property = TestOrg.PK;
			((ModuleGuidFilter)filter["Booking Party"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			filter = new QuotedBookingFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Booking Party"]).Property = TestOrg2.PK;
			((ModuleGuidFilter)filter["Booking Party"]).IsActive = true;

			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleGuidFilter)filter["Booking Party"]).ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);
		}

		public void TestBookingParty_BlankOperators()
		{
			DocumentaryAddressAssert_BlankOperators("BookingPartyDocumentaryAddress", "Booking Party");
		}

		public void TestBookingPartyFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("BookingPartyDocumentaryAddress", "Booking Party", false);
		}

		public void TestBookingPartyFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("BookingPartyDocumentaryAddress", "Booking Party", false);
		}

		public void TestBookingPartyFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Booking Party");
		}

		List<ZGuid> GetQuotePks(ViewQuotedBookingCollection collection)
		{
			return collection.Select(quotedBooking => quotedBooking.QuotedBooking.Quote.PK).ToList();
		}

		#endregion

		#region TestCreditor

		public void TestCreditor()
		{
			var quotedBooking1 = CreateQuotedBooking(TestOrg.PK);
			var quotedBooking2 = CreateQuotedBooking(TestOrg2.PK);

			quotedBooking1.Creditor = TestOrg.PK;
			quotedBooking2.Creditor = TestOrg2.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Creditor"]).Property = TestOrg.PK;
			((ModuleGuidFilter)filter["Creditor"]).IsActive = true;

			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			filter = new QuotedBookingFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Creditor"]).Property = TestOrg2.PK;
			((ModuleGuidFilter)filter["Creditor"]).IsActive = true;

			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleGuidFilter)filter["Creditor"]).ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);
			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			filter = new QuotedBookingFilterStripBusinessObject();
			((ModuleGuidFilter)filter["Creditor"]).Property = TestOrg3.PK;
			((ModuleGuidFilter)filter["Creditor"]).IsActive = true;
			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Bookings", 0, collection.Count);
		}

		#endregion

		#region TestBookingPartyName

		public void TestBookingPartyName()
		{
			var quotedBooking1 = CreateQuotedBooking(TestOrg.PK);
			var quotedBooking2 = CreateQuotedBooking(TestOrg2.PK);

			quotedBooking1.BookingPartyDocumentaryAddress.E2_OA_Address = TestOrg.MainAddress.PK;
			quotedBooking2.BookingPartyDocumentaryAddress.E2_OA_Address = TestOrg2.MainAddress.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Booking Party Name"]).Property = "Test Client #1";
			((ModuleTextFilter)filter["Booking Party Name"]).IsActive = true;

			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 ViewQuotedBookings", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			filter = new QuotedBookingFilterStripBusinessObject();
			((ModuleTextFilter)filter["Booking Party Name"]).Property = "Test Client #2";
			((ModuleTextFilter)filter["Booking Party Name"]).IsActive = true;

			collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 ViewQuotedBookings", 1, collection.Count);
			AssertCollectionContains(new ZQuery(ViewQuotedBookingSchema.VB_TH, quotedBooking2.Quote.PK), collection);
		}

		#endregion

		#region TestConsignorConsignee

		public void TestConsignor_MultilingualDescription()
		{
			var filter = new QuotedBookingFilterStripBusinessObject();
			AssertEquals("Consignor", ((ModuleGuidFilter)filter["Consignor"]).MultilingualDescription);

			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (NoResString)"Sender");
			filter = new QuotedBookingFilterStripBusinessObject();
			AssertEquals("Sender", ((ModuleGuidFilter)filter["Consignor"]).MultilingualDescription);
		}

		public void TestConsignor()
		{
			var originalActiveStatus = TestOrg3.OH_IsActive;

			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();

			quotedBooking1.Booking.ConsignorPK = TestOrg.PK;
			quotedBooking2.Quote.CurrentOneOffQuote.PickUpDocAddress.Address.OA_OH = TestOrg2.PK;
			TestOrg3.OH_IsActive = false;
			quotedBooking3.Booking.ConsignorPK = TestOrg3.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Consignor"];
			filter.IsActive = true;
			filter.Property = TestOrg.PK;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 1", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.Property = TestOrg2.PK;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 2", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("2 items", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking1.PK, quotedBooking3.PK }, collection.Select(element => element.PK));

			AssertNoWarning(filter.PropertyInfo, "Organization is in-active.");
			filter.Property = TestOrg3.PK;
			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			collection.Load(FilterStripBizO.Filter);
			AssertHasWarning(filter.PropertyInfo, "Organization is in-active.");
			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 3", quotedBooking3.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			TestOrg3.OH_IsActive = originalActiveStatus;
		}

		public void TestConsignor_BlankOperators()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();

			quotedBooking1.Booking.ConsignorPK = TestOrg.PK;
			quotedBooking2.Quote.CurrentOneOffQuote.PickUpDocAddress.Address.OA_OH = TestOrg2.PK;
			quotedBooking3.Booking.ConsignorPK = ZGuid.Empty;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Consignor"];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 items", 1, collection.Count);
			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("2 items", 2, collection.Count);
			var pks = GetQuotePks(collection);
			AssertContainsExactElementsInAnyOrder("contains quotedBooking1, quotedBooking2", new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, pks);
		}

		public void TestConsignorFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("ConsignorDocumentaryAddress", "Consignor", false);
		}

		public void TestConsignorFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("ConsignorDocumentaryAddress", "Consignor", false);
		}

		public void TestConsignorFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Consignor");
		}

		public void TestConsignee()
		{
			var originalActiveStatus = TestOrg3.OH_IsActive;

			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();

			quotedBooking1.Booking.ConsigneePK = TestOrg.PK;
			quotedBooking2.Booking.ConsigneePK = TestOrg2.PK;
			TestOrg3.OH_IsActive = false;
			quotedBooking3.Booking.ConsigneePK = TestOrg3.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			filter.IsActive = true;
			filter.Property = TestOrg.PK;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 1", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.Property = TestOrg2.PK;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 2", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("2 items", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking1.PK, quotedBooking3.PK }, collection.Select(element => element.PK));

			AssertNoWarning(filter.PropertyInfo, "Organization is in-active.");
			filter.Property = TestOrg3.PK;
			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			collection.Load(FilterStripBizO.Filter);
			AssertHasWarning(filter.PropertyInfo, "Organization is in-active.");
			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 3", quotedBooking3.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			TestOrg3.OH_IsActive = originalActiveStatus;
		}

		public void TestConsignee_BlankOperators()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuotedBooking();

			quotedBooking1.Booking.ConsigneePK = TestOrg.PK;
			quotedBooking2.Booking.ConsigneePK = TestOrg2.PK;
			quotedBooking3.Booking.ConsigneePK = ZGuid.Empty;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 items", 1, collection.Count);
			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("2 items", 2, collection.Count);
			var pks = GetQuotePks(collection);
			AssertContainsExactElementsInAnyOrder("contains quotedBooking1, quotedBooking2", new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, pks);
		}

		public void TestConsigneeFilterLookupList()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.ConsigneePK = TestOrg.PK;

			Factory.Save();

			var consigneeFilter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			consigneeFilter.IsActive = true;
			consigneeFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			consigneeFilter.Property = TestOrg.PK;

			AssertContainsExactElementsInAnyOrder(consigneeFilter.List, new ConsigneeCollection(Factory));

			var consigneeCollection = new ViewQuotedBookingCollection(Factory);
			consigneeCollection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, consigneeCollection.Count);
			AssertEquals("Quoted Booking 1", quotedBooking1.Quote.PK, consigneeCollection[0].QuotedBooking.Quote.PK);
		}

		public void TestConsigneeFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("ConsigneeDocumentaryAddress", "Consignee", false);
		}

		public void TestConsigneeFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("ConsigneeDocumentaryAddress", "Consignee", false);
		}

		public void TestConsigneeFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Consignee");
		}

		#endregion

		#region Test Related Parties Filters

		public void TestConsignorRelatedPartiesFilter()
		{
			var quotedBooking1 = GetQuotedBookingWithConsignor("con1");
			var quotedBooking2 = GetQuotedBookingWithConsignor("con2");
			var emptyQuotedBooking = GetQuotedBooking("Empty");

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(quotedBooking1.QuotedBooking.Consignor, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(quotedBooking1.QuotedBooking.Consignor, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(quotedBooking2.QuotedBooking.Consignor, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(quotedBooking2.QuotedBooking.Consignor, party3);

			var newQuotedBooking1 = GetQuotedBookingWithConsignor("consignor1");
			var newQuotedBooking2 = GetQuotedBookingWithConsignor("consignor2");
			var newQuotedBooking3 = GetQuotedBookingWithConsignor("consignor3");
			var newQuotedBooking4 = GetQuotedBookingWithConsignor("consignor4");
			var newQuotedBooking5 = GetQuotedBookingWithConsignor("consignor5");
			var newQuotedBooking6 = GetQuotedBookingWithConsignor("consignor6");

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newQuotedBooking1.QuotedBooking.Consignor, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newQuotedBooking2.QuotedBooking.Consignor, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newQuotedBooking3.QuotedBooking.Consignor, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newQuotedBooking4.QuotedBooking.Consignor, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newQuotedBooking5.QuotedBooking.Consignor, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newQuotedBooking6.QuotedBooking.Consignor, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;

			Factory.Save();

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO["Consignor Related Parties"];

			Asserter.AssertMatches("Empty Filter", filter, quotedBooking1, quotedBooking2, emptyQuotedBooking, newQuotedBooking1, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, quotedBooking1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, quotedBooking1, quotedBooking2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, quotedBooking2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newQuotedBooking1, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newQuotedBooking1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newQuotedBooking3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newQuotedBooking5);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newQuotedBooking6);
		}

		public void TestConsigneeRelatedPartiesFilter()
		{
			var quotedBooking1 = GetQuotedBookingWithConsignee("con1");
			var quotedBooking2 = GetQuotedBookingWithConsignee("con2");
			var emptyQuotedBooking = GetQuotedBooking("Empty");

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(quotedBooking1.QuotedBooking.Consignee, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(quotedBooking1.QuotedBooking.Consignee, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(quotedBooking2.QuotedBooking.Consignee, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(quotedBooking2.QuotedBooking.Consignee, party3);

			var newQuotedBooking1 = GetQuotedBookingWithConsignee("consignee1");
			var newQuotedBooking2 = GetQuotedBookingWithConsignee("consignee2");
			var newQuotedBooking3 = GetQuotedBookingWithConsignee("consignee3");
			var newQuotedBooking4 = GetQuotedBookingWithConsignee("consignee4");
			var newQuotedBooking5 = GetQuotedBookingWithConsignee("consignee5");
			var newQuotedBooking6 = GetQuotedBookingWithConsignee("consignee6");

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newQuotedBooking1.QuotedBooking.Consignee, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newQuotedBooking2.QuotedBooking.Consignee, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newQuotedBooking3.QuotedBooking.Consignee, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newQuotedBooking4.QuotedBooking.Consignee, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newQuotedBooking5.QuotedBooking.Consignee, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newQuotedBooking6.QuotedBooking.Consignee, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;

			Factory.Save();

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO["Consignee Related Parties"];

			Asserter.AssertMatches("Empty Filter", filter, quotedBooking1, quotedBooking2, emptyQuotedBooking, newQuotedBooking1, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, quotedBooking1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, quotedBooking1, quotedBooking2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, quotedBooking2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newQuotedBooking1, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newQuotedBooking1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newQuotedBooking3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newQuotedBooking5);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newQuotedBooking6);
		}

		public void TestLocalClientRelatedPartiesFilter()
		{
			var quotedBooking1 = GetQuotedBookingWithClient("client1");
			var quotedBooking2 = GetQuotedBookingWithClient("client2");
			var emptyQuotedBooking = GetQuotedBooking("Empty");

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(quotedBooking1.QuotedBooking.Job.LocalCharges, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(quotedBooking1.QuotedBooking.Job.LocalCharges, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(quotedBooking2.QuotedBooking.Job.LocalCharges, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(quotedBooking2.QuotedBooking.Job.LocalCharges, party3);

			var newQuotedBooking1 = GetQuotedBookingWithClient("newClient1");
			var newQuotedBooking2 = GetQuotedBookingWithClient("newClient2");
			var newQuotedBooking3 = GetQuotedBookingWithClient("newClient3");
			var newQuotedBooking4 = GetQuotedBookingWithClient("newClient4");
			var newQuotedBooking5 = GetQuotedBookingWithClient("newClient5");
			var newQuotedBooking6 = GetQuotedBookingWithClient("newClient6");

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newQuotedBooking1.QuotedBooking.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newQuotedBooking2.QuotedBooking.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newQuotedBooking3.QuotedBooking.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newQuotedBooking4.QuotedBooking.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newQuotedBooking5.QuotedBooking.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newQuotedBooking6.QuotedBooking.Job.LocalCharges, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Core.Constants.ContainerModes.LCL;

			Factory.Save();

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO["Client Related Parties"];

			Asserter.AssertMatches("Empty Filter", filter, quotedBooking1, quotedBooking2, emptyQuotedBooking, newQuotedBooking1, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, quotedBooking1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, quotedBooking1, quotedBooking2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, quotedBooking2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newQuotedBooking1, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newQuotedBooking1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newQuotedBooking2, newQuotedBooking3, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newQuotedBooking3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newQuotedBooking4, newQuotedBooking5, newQuotedBooking6);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newQuotedBooking5);

			filter.TransportMode = Core.Constants.TransportModes.Sea;
			filter.ContainerMode = Core.Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newQuotedBooking6);
		}

		#endregion

		#region TestPickupAgentQuery

		public void TestPickupAgentQuery()
		{
			DocumentaryAddressAssert("PickupAgentDocumentaryAddress", "Pickup Agent");
		}

		public void TestPickupAgentQuery_BlankOperators()
		{
			DocumentaryAddressAssert_BlankOperators("PickupAgentDocumentaryAddress", "Pickup Agent");
		}

		public void TestPickupAgentFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("PickupAgentDocumentaryAddress", "Pickup Agent", true);
		}

		public void TestPickupAgentFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("PickupAgentDocumentaryAddress", "Pickup Agent", true);
		}

		public void TestPickupAgentFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Pickup Agent");
		}

		void DocumentaryAddressAssert(string bookingField, string filterName)
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;

			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			JobDocAddress agentAddress1 = (JobDocAddress)quotedBooking1.Booking[bookingField];
			agentAddress1.OrganisationPK = org1.PK;

			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			JobDocAddress agentAddress2 = (JobDocAddress)quotedBooking2.Booking[bookingField];
			agentAddress2.OrganisationPK = org2.PK;

			var quotedBooking3 = CreateQuotedBooking();
			var agentAddress3 = (JobDocAddress)quotedBooking3.Booking[bookingField];
			agentAddress3.OrganisationPK = org3.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[filterName];
			filter.IsActive = true;
			filter.Property = org1.PK;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 1", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.Property = org2.PK;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 2", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("2 items", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking1.PK, quotedBooking3.PK }, collection.Select(element => element.PK));

			AssertNoWarning(filter.PropertyInfo, "Organization is in-active.");
			filter.Property = org3.PK;
			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			collection.Load(FilterStripBizO.Filter);
			AssertHasWarning(filter.PropertyInfo, "Organization is in-active.");
			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 1", quotedBooking3.Quote.PK, collection[0].QuotedBooking.Quote.PK);
		}

		void DocumentaryAddressAssert_BlankOperators(string bookingField, string filterName)
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			JobDocAddress agentAddress1 = (JobDocAddress)quotedBooking1.Booking[bookingField];
			agentAddress1.OrganisationPK = TestOrg.PK;

			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			JobDocAddress agentAddress2 = (JobDocAddress)quotedBooking2.Booking[bookingField];
			agentAddress2.OrganisationPK = TestOrg2.PK;
			QuotedBooking quotedBooking3 = CreateQuotedBooking();

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[filterName];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 3", quotedBooking3.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("2 items", 2, collection.Count);
			var pks = GetQuotePks(collection);
			AssertContainsExactElementsInAnyOrder("Contains quotedBooking1 and quotedBooking2", new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, pks);
		}

		#endregion

		#region TestControllingCustomer

		public void TestControllingCustomer()
		{
			DocumentaryAddressAssert("ControllingCustomerAddress", "Controlling Customer");
		}

		public void TestControllingCustomer_BlankOperators()
		{
			DocumentaryAddressAssert_BlankOperators("ControllingCustomerAddress", "Controlling Customer");
		}

		public void TestControllingCustomerFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("ControllingCustomerAddress", "Controlling Customer", true);
		}

		public void TestControllingCustomerFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("ControllingCustomerAddress", "Controlling Customer", true);
		}

		public void TestControllingCustomerFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Controlling Customer");
		}

		#endregion

		#region TestControlling Agent

		public void TestControllingAgent()
		{
			DocumentaryAddressAssert("ControllingAgentDocumentaryAddress", "Controlling Agent");
		}

		public void TestControllingAgent_BlankOperators()
		{
			DocumentaryAddressAssert_BlankOperators("ControllingAgentDocumentaryAddress", "Controlling Agent");
		}

		public void TestControllingAgentFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("ControllingAgentDocumentaryAddress", "Controlling Agent", false);
		}

		public void TestControllingAgentFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("ControllingAgentDocumentaryAddress", "Controlling Agent", false);
		}

		public void TestControllingAgentFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Controlling Agent");
		}

		#endregion

		#region TestDeliveryAgentQuery

		public void TestDeliveryAgentQuery()
		{
			DocumentaryAddressJobShipmentColumnAssert("JS_OH_DeliveryAgent", "Delivery Agent");
		}

		public void TestDeliveryAgentQuery_BlankOperators()
		{
			DocumentaryAddressJobShipmentColumnAssert_BlankOperators("JS_OH_DeliveryAgent", "Delivery Agent");
		}

		public void DocumentaryAddressJobShipmentColumnAssert(string shipmentColumn, string fieldName)
		{
			var originalActiveStatus = TestOrg3.OH_IsActive;

			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking[shipmentColumn] = TestOrg.PK;

			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking[shipmentColumn] = TestOrg2.PK;

			TestOrg3.OH_IsActive = false;
			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking[shipmentColumn] = TestOrg3.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[fieldName];
			filter.IsActive = true;
			filter.Property = TestOrg.PK;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 1", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.Property = TestOrg2.PK;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 2", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("2 items", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { quotedBooking1.PK, quotedBooking3.PK }, collection.Select(element => element.PK));

			AssertNoWarning(filter.PropertyInfo, "Organization is in-active.");
			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			filter.Property = TestOrg3.PK;
			collection.Load(FilterStripBizO.Filter);
			AssertHasWarning(filter.PropertyInfo, "Organization is in-active.");
			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 3", quotedBooking3.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			TestOrg3.OH_IsActive = originalActiveStatus;
		}

		public void DocumentaryAddressJobShipmentColumnAssert_BlankOperators(string shipmentColumn, string fieldName)
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking[shipmentColumn] = TestOrg.PK;

			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking[shipmentColumn] = TestOrg2.PK;

			QuotedBooking quotedBooking3 = CreateQuotedBooking();

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[fieldName];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("contains quotedBooking1", quotedBooking3.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("should contain quotedBooking1 and quotedBooking2", 2, collection.Count);
			var quotePks = GetQuotePks(collection);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, quotePks);
		}

		#endregion

		#region CFS

		public void TestCFS()
		{
			var originalActiveStatus = TestOrg3.OH_IsActive;

			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.ExportReceivingDepot_ZAddress.OrgPK = TestOrg.PK;
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.ExportReceivingDepot_ZAddress.OrgPK = TestOrg2.PK;

			TestOrg3.OH_IsActive = false;
			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.ExportReceivingDepot_ZAddress.OrgPK = TestOrg3.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["CFS"];
			filter.IsActive = true;
			filter.Property = TestOrg.PK;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 1", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.Property = TestOrg2.PK;

			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 2", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			AssertNoWarning(filter.PropertyInfo, "Organization is in-active.");
			filter.Property = TestOrg3.PK;
			collection.Load(FilterStripBizO.Filter);
			AssertHasWarning(filter.PropertyInfo, "Organization is in-active.");
			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 2", quotedBooking3.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			TestOrg3.OH_IsActive = originalActiveStatus;
		}

		public void TestCFS_BlankOperators()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.ExportReceivingDepot_ZAddress.OrgPK = TestOrg.PK;
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.ExportReceivingDepot_ZAddress.OrgPK = TestOrg2.PK;
			QuotedBooking quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.ExportReceivingDepot_ZAddress.OrgPK = ZGuid.Empty;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["CFS"];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("1 item", 1, collection.Count);
			AssertEquals("Quoted Booking 3", quotedBooking3.Quote.PK, collection[0].QuotedBooking.Quote.PK);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("2 items", 2, collection.Count);
			var pks = GetQuotePks(collection);
			AssertContainsExactElementsInAnyOrder("Contains quotedBooking1 and quotedBooking2", new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, pks);
		}

		#endregion

		#region Import / Export broker

		public void TestImportBrokerQuery()
		{
			DocumentaryAddressJobShipmentColumnAssert("JS_OH_ImportBroker", "Import Broker");
		}

		public void TestImportBrokerQuery_BlankOperators()
		{
			DocumentaryAddressJobShipmentColumnAssert_BlankOperators("JS_OH_ImportBroker", "Import Broker");
		}

		public void TestExportBrokerQuery()
		{
			DocumentaryAddressJobShipmentColumnAssert("JS_OH_ExportBroker", "Export Broker");
		}

		public void TestExportBrokerQuery_BlankOperators()
		{
			DocumentaryAddressJobShipmentColumnAssert_BlankOperators("JS_OH_ExportBroker", "Export Broker");
		}

		#endregion

		#region AssertDocAddressMultiValueQuery

		void AssertDocAddressMultiValueQuery_Equal(ZString docAddressName, ZString moduleGuidFilterName, ZBool isBookingProperty)
		{
			var orgHeader1 = GetOrgHeader("APRIS");
			orgHeader1.OH_IsConsignee = true;
			orgHeader1.OH_IsForwarder = true;
			orgHeader1.OH_IsConsignor = true;

			var orgHeader2 = GetOrgHeader("KYZZZ");
			orgHeader2.OH_IsConsignee = true;
			orgHeader2.OH_IsForwarder = true;
			orgHeader2.OH_IsConsignor = true;

			var orgHeader3 = GetOrgHeader("ABCDE");
			orgHeader3.OH_IsConsignee = true;
			orgHeader3.OH_IsForwarder = true;
			orgHeader3.OH_IsConsignor = false;
			orgHeader3.OH_IsActive = false;

			var orgHeader4 = GetOrgHeader("XYZAB");
			orgHeader4.OH_IsConsignee = true;
			orgHeader4.OH_IsForwarder = true;
			orgHeader4.OH_IsConsignor = true;

			var quotedBooking1 = CreateQuotedBooking();
			var address1 = (isBookingProperty ? quotedBooking1.Booking[docAddressName] : quotedBooking1[docAddressName]) as JobDocAddress;
			address1.OrganisationPK = orgHeader1.PK;

			var quotedBooking2 = CreateQuotedBooking();
			var address2 = (isBookingProperty ? quotedBooking2.Booking[docAddressName] : quotedBooking2[docAddressName]) as JobDocAddress;
			address2.OrganisationPK = orgHeader2.PK;

			var quotedBooking3 = CreateQuotedBooking();
			var address3 = (isBookingProperty ? quotedBooking3.Booking[docAddressName] : quotedBooking3[docAddressName]) as JobDocAddress;
			address3.OrganisationPK = orgHeader3.PK;

			var quotedBooking4 = CreateQuotedBooking();
			var address4 = (isBookingProperty ? quotedBooking4.Booking[docAddressName] : quotedBooking4[docAddressName]) as JobDocAddress;
			address4.OrganisationPK = orgHeader4.PK;

			Factory.Save();

			var filter1 = (ModuleGuidFilter)FilterStripBizO[moduleGuidFilterName];
			filter1.Property = orgHeader1.PK;
			filter1.IsActive = true;

			var filter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter2.Property = orgHeader2.PK;
			filter2.IsActive = true;

			var filter3 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter3.Property = orgHeader3.PK;
			filter3.IsActive = true;

			filter1.OrCategory = FilterOrCategory.Red;
			filter2.OrCategory = FilterOrCategory.Red;
			filter3.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				"VB_JS IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO4_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH in (@CWO5_, @CWO6_, @CWO7_))))",
				FilterStripBizO.Filter.FilterString);

			var quotedBookings = new ViewQuotedBookingCollection(Factory);
			quotedBookings.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Booking.PK, quotedBooking2.Booking.PK, quotedBooking3.Booking.PK }, quotedBookings.Select(x => x.QuotedBooking.Booking.PK));
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK, quotedBooking3.Quote.PK }, quotedBookings.Select(x => x.QuotedBooking.Quote.PK));

			var pickupAgentFilter4 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			pickupAgentFilter4.Property = orgHeader4.PK;
			pickupAgentFilter4.IsActive = true;
			pickupAgentFilter4.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				"VB_JS IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO4_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH in (@CWO5_, @CWO6_, @CWO7_, @CWO8_))))",
				FilterStripBizO.Filter.FilterString);

			quotedBookings.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Booking.PK, quotedBooking2.Booking.PK, quotedBooking3.Booking.PK, quotedBooking4.Booking.PK }, quotedBookings.Select(x => x.QuotedBooking.Booking.PK));
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK, quotedBooking3.Quote.PK, quotedBooking4.Quote.PK }, quotedBookings.Select(x => x.QuotedBooking.Quote.PK));
		}

		void AssertDocAddressMultiValueQuery_NotEqual(ZString docAddressName, ZString moduleGuidFilterName, ZBool isBookingProperty)
		{
			var orgHeader1 = GetOrgHeader("APRIS");
			orgHeader1.OH_IsConsignee = true;
			orgHeader1.OH_IsForwarder = true;
			orgHeader1.OH_IsConsignor = true;

			var orgHeader2 = GetOrgHeader("KYZZZ");
			orgHeader2.OH_IsConsignee = true;
			orgHeader2.OH_IsForwarder = true;
			orgHeader2.OH_IsConsignor = true;

			var orgHeader3 = GetOrgHeader("ABCDE");
			orgHeader3.OH_IsConsignee = true;
			orgHeader3.OH_IsForwarder = true;
			orgHeader3.OH_IsConsignor = false;
			orgHeader3.OH_IsActive = false;

			var orgHeader4 = GetOrgHeader("XYZAB");
			orgHeader4.OH_IsConsignee = true;
			orgHeader4.OH_IsForwarder = true;
			orgHeader4.OH_IsConsignor = true;

			var quotedBooking1 = CreateQuotedBooking();
			var address1 = (isBookingProperty ? quotedBooking1.Booking[docAddressName] : quotedBooking1[docAddressName]) as JobDocAddress;
			address1.OrganisationPK = orgHeader1.PK;

			var quotedBooking2 = CreateQuotedBooking();
			var address2 = (isBookingProperty ? quotedBooking2.Booking[docAddressName] : quotedBooking2[docAddressName]) as JobDocAddress;
			address2.OrganisationPK = orgHeader2.PK;

			var quotedBooking3 = CreateQuotedBooking();
			var address3 = (isBookingProperty ? quotedBooking3.Booking[docAddressName] : quotedBooking3[docAddressName]) as JobDocAddress;
			address3.OrganisationPK = orgHeader3.PK;

			var quotedBooking4 = CreateQuotedBooking();
			var address4 = (isBookingProperty ? quotedBooking4.Booking[docAddressName] : quotedBooking4[docAddressName]) as JobDocAddress;
			address4.OrganisationPK = orgHeader4.PK;

			Factory.Save();

			var filter1 = (ModuleGuidFilter)FilterStripBizO[moduleGuidFilterName];
			filter1.Property = orgHeader1.PK;
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter1.IsActive = true;

			var filter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter2.Property = orgHeader2.PK;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter2.IsActive = true;

			AssertContains("Generated filter SQL",
				"VB_JS IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO2_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH not in (@CWO3_, @CWO4_))))",
				FilterStripBizO.Filter.FilterString);

			var quotedBookings = new ViewQuotedBookingCollection(Factory);
			quotedBookings.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking3.Booking.PK, quotedBooking4.Booking.PK }, quotedBookings.Select(x => x.QuotedBooking.Booking.PK));
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking3.Quote.PK, quotedBooking4.Quote.PK }, quotedBookings.Select(x => x.QuotedBooking.Quote.PK));

			var filter3 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter3.Property = orgHeader3.PK;
			filter3.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter3.IsActive = true;

			var filter4 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter4.Property = orgHeader4.PK;
			filter4.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter4.IsActive = true;

			AssertContains("Generated filter SQL",
				"VB_JS IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO2_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH not in (@CWO3_, @CWO4_, @CWO5_, @CWO6_))))",
				FilterStripBizO.Filter.FilterString);

			quotedBookings.Load(FilterStripBizO.Filter);

			AssertEquals(0, quotedBookings.Count);
		}

		void AssertDocAddressMultiValueQuery_OtherScenario(ZString moduleGuidFilterName)
		{
			// The CanGroup condition for ModuleTextFilter is
			// OrCategory != FilterOrCategory.None && SqlComparisonOperator == SQLComparisonOperator.Equal || OrCategory == FilterOrCategory.None && SqlComparisonOperator == SQLComparisonOperator.NotEqual

			var orgHeader1 = GetOrgHeader("APRIS");
			orgHeader1.OH_IsConsignee = true;
			orgHeader1.OH_IsForwarder = true;
			orgHeader1.OH_IsConsignor = true;

			var orgHeader2 = GetOrgHeader("KYZZZ");
			orgHeader2.OH_IsConsignee = true;
			orgHeader2.OH_IsForwarder = true;
			orgHeader2.OH_IsConsignor = true;

			Factory.Save();

			var filter1 = (ModuleGuidFilter)FilterStripBizO[moduleGuidFilterName];
			filter1.Property = orgHeader1.PK;
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter1.IsActive = true;
			filter1.OrCategory = FilterOrCategory.Red;

			var filter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter2.Property = orgHeader2.PK;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter2.IsActive = true;
			filter1.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				"VB_JS IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO2_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH <> @CWO3_))",
				FilterStripBizO.Filter.FilterString);
			AssertContains("Generated filter SQL",
				"VB_JS IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO2_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH <> @CWO8_))",
				FilterStripBizO.Filter.FilterString);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString,
					Regex.Escape("VB_JS NOT IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO2_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH <> @CWO3_))")).Count);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString,
					Regex.Escape("VB_JS IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO2_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH <> @CWO3_))")).Count);

			filter1.OrCategory = FilterOrCategory.None;
			filter2.OrCategory = FilterOrCategory.None;

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			AssertContains("Generated filter SQL",
				"VB_JS IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO2_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH = @CWO3_))",
				FilterStripBizO.Filter.FilterString);
			AssertContains("Generated filter SQL",
				"VB_JS IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO2_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH = @CWO6_))",
				FilterStripBizO.Filter.FilterString);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString,
					Regex.Escape("VB_JS NOT IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO2_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH <> @CWO3_))")).Count);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString,
					Regex.Escape("VB_JS IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_ParentTableCode = 'JS' and E2_AddressType = @CWO2_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH <> @CWO3_))")).Count);
		}

		#endregion

		#endregion

		#region Audit Information

		#region TestCreatingUser

		public void TestCreatingUser()
		{
			var disableTriggerSql = @"
IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_RatingHeader_AuditDetailsAreNotMissing_Insert' AND Object_Name(parent_id) = 'RatingHeader')
	ALTER TABLE dbo.RatingHeader DISABLE TRIGGER TG_RatingHeader_AuditDetailsAreNotMissing_Insert
IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_RatingHeader_AuditDetailsAreNotMissing_Update' AND Object_Name(parent_id) = 'RatingHeader')
	ALTER TABLE dbo.RatingHeader DISABLE TRIGGER TG_RatingHeader_AuditDetailsAreNotMissing_Update";

			Db.Connection.ExecuteNonQuery(disableTriggerSql);

			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			GlbStaff userZB = Factory.NewWithValidTestData<GlbStaff>();
			userZB.GS_Code = "ZB";
			quotedBooking1.Booking.JS_SystemCreateUser = "ZA";
			quotedBooking1.Quote.TH_SystemCreateUser = "ZB";

			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_SystemCreateUser = "ZC";
			quotedBooking2.Quote.TH_SystemCreateUser = "ZD";

			QuotedBooking quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Quote.TH_SystemCreateUser = "ZE";

			QuotedBooking quotedBooking4 = CreateQuotedBooking();
			quotedBooking4.Booking.JS_SystemCreateUser = "ZF";

			QuotedBooking bookingOnly = CreateBookingOnly();
			bookingOnly.Booking.JS_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;

			QuotedBooking bookingOnly2 = CreateBookingOnly();

			QuotedBooking quoteOnly = CreateQuoteOnly();
			quoteOnly.Quote.TH_SystemCreateUser = "BB";

			QuotedBooking quoteOnly2 = CreateQuoteOnly();

			QuotedBooking quoteOnly3 = CreateQuoteOnly();
			quoteOnly3.Quote.TH_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			quoteOnly2.Quote.TH_SystemCreateUser = ZString.Empty;
			quotedBooking4.Quote.TH_SystemCreateUser = ZString.Empty;
			bookingOnly2.Booking.JS_SystemCreateUser = ZString.Empty;
			quotedBooking3.Booking.JS_SystemCreateUser = ZString.Empty;
			Factory.Save();

			Action<string, string, string, QuotedBooking[]> assertFiltering = (message, userNK, comparisonOperator, expectedQuotedBookings) =>
			{
				var filter = GetNewFilterStripBusinessObject();
				((ModuleNkFilter)filter["Creating User"]).Property = userNK;
				((ModuleNkFilter)filter["Creating User"]).ComparisonOperator = comparisonOperator;
				((ModuleNkFilter)filter["Creating User"]).IsActive = true;

				var collection = new ViewQuotedBookingCollection(Factory);
				collection.Load(filter.Filter);
				AssertContainsExactElementsInAnyOrder(message, expectedQuotedBookings.Select(qb => qb.PK).ToArray(), collection.Select(vqb => vqb.PK).ToArray());
			};

			Action<string, string, string, QuotedBooking> assertFilteringContains = (message, userNK, comparisonOperator, expectedQuotedBooking) =>
			{
				var filter = GetNewFilterStripBusinessObject();
				((ModuleNkFilter)filter["Creating User"]).Property = userNK;
				((ModuleNkFilter)filter["Creating User"]).ComparisonOperator = comparisonOperator;
				((ModuleNkFilter)filter["Creating User"]).IsActive = true;

				var collection = new ViewQuotedBookingCollection(Factory);
				collection.Load(filter.Filter);

				AssertCollectionContains(message, expectedQuotedBooking.PK, collection.Select(vqb => vqb.PK).ToArray());
			};

			Action<string, string, string, QuotedBooking> assertFilteringNotContains = (message, userNK, comparisonOperator, expectedQuotedBooking) =>
			{
				var filter = GetNewFilterStripBusinessObject();
				((ModuleNkFilter)filter["Creating User"]).Property = userNK;
				((ModuleNkFilter)filter["Creating User"]).ComparisonOperator = comparisonOperator;
				((ModuleNkFilter)filter["Creating User"]).IsActive = true;

				var collection = new ViewQuotedBookingCollection(Factory);
				collection.Load(filter.Filter);

				AssertCollectionNotContains(message, expectedQuotedBooking.PK, collection.Select(vqb => vqb.PK).ToArray());
			};

			#region Exact comparison
			assertFiltering("Quotedbooking not found via booking field", "ZA", ModuleNkFilter.ComparisonConstants.Exact, Array.Empty<QuotedBooking>());
			assertFiltering("Quotedbooking found via quote field", "ZB", ModuleNkFilter.ComparisonConstants.Exact, new QuotedBooking[] { quotedBooking1 });

			assertFiltering("Quotedbooking not found via booking field", "ZC", ModuleNkFilter.ComparisonConstants.Exact, Array.Empty<QuotedBooking>());
			assertFiltering("Quotedbooking found via quote field", "ZD", ModuleNkFilter.ComparisonConstants.Exact, new QuotedBooking[] { quotedBooking2 });

			assertFiltering("QuickBooking found via booking field", GlbStaff.CurrentUser.GS_Code, ModuleNkFilter.ComparisonConstants.Exact, new QuotedBooking[] { bookingOnly });
			assertFiltering("Spot quote not found via quote field", "BB", ModuleNkFilter.ComparisonConstants.Exact, Array.Empty<QuotedBooking>());

			assertFiltering("No matching results", "ZZ", ModuleNkFilter.ComparisonConstants.Exact, Array.Empty<QuotedBooking>());
			#endregion

			#region IsBlank comparison
			assertFilteringNotContains("Do not include Quoted bookings when booking creator is blank, but quote creator is not", ZString.Empty, ModuleNkFilter.ComparisonConstants.IsBlank, quotedBooking3);
			assertFilteringContains("Include QuickBooking when the book creator is blank", ZString.Empty, ModuleNkFilter.ComparisonConstants.IsBlank, bookingOnly2);
			assertFilteringNotContains("Exclude Spot quotes from results even when the quote creator is blank", ZString.Empty, ModuleNkFilter.ComparisonConstants.IsBlank, quoteOnly2);
			assertFilteringContains("Include quoted bookings when the quote creator is blank", ZString.Empty, ModuleNkFilter.ComparisonConstants.IsBlank, quotedBooking4);
			#endregion

			#region IsNotBlank comparison
			assertFilteringContains("Include quoted bookings when the quote creator is not blank", ZString.Empty, ModuleNkFilter.ComparisonConstants.IsNotBlank, quotedBooking3);
			assertFilteringContains("Include QuickBookings when the quote creator is null and the booking creator is not blank", ZString.Empty, ModuleNkFilter.ComparisonConstants.IsNotBlank, bookingOnly);
			assertFiltering("Include quoted bookings when creator is not blank", ZString.Empty, ModuleNkFilter.ComparisonConstants.IsNotBlank, new QuotedBooking[] { quotedBooking1, quotedBooking2, quotedBooking3, bookingOnly });
			#endregion

			#region NotEqual comparison
			assertFiltering("Include quoted bookings not matching the provided code", GlbStaff.CurrentUser.GS_Code, ModuleNkFilter.ComparisonConstants.NotEqual, new QuotedBooking[] { quotedBooking1, quotedBooking2, quotedBooking3, quotedBooking4, bookingOnly2 });
			#endregion

			#region CurrentUser comparison
			assertFiltering("Include quoted bookings when the quote creator is the current user", ModuleNkFilter.ComparisonConstants.CurrentUser, ModuleNkFilter.ComparisonConstants.CurrentUser, new QuotedBooking[] { bookingOnly });
			#endregion

			#region FiltersMatch comparison
			var filter1 = GetNewFilterStripBusinessObject();
			((ModuleNkFilter)filter1["Creating User"]).Property = ZString.Empty;
			((ModuleNkFilter)filter1["Creating User"]).ComparisonOperator = ModuleNkFilter.ComparisonConstants.FiltersMatch;
			((ModuleNkFilter)filter1["Creating User"]).IsActive = true;
			((ModuleNkFilter)filter1["Creating User"]).SelectedFilters.AddTextFilterStrip("Code", "ZB");

			var collection1 = new ViewQuotedBookingCollection(Factory);
			collection1.Load(filter1.Filter);
			AssertCollectionContains(quotedBooking1.PK, collection1.Select(vqb => vqb.PK).ToArray());
			#endregion
		}

		public void TestCreatingUser_AllowedComparisonOperators()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			ModuleNkFilter filter = (ModuleNkFilter)filterStrip["Creating User"];
			AssertEquals("The Creating User filter should use comparison operators", true, filter.HasComparisonOperator);
			string[] expectedOperators = new string[7];
			expectedOperators[0] = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			expectedOperators[1] = ModuleTextFilter.ComparisonConstants.IsBlank;
			expectedOperators[2] = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			expectedOperators[3] = ModuleTextFilter.ComparisonConstants.NotEqual;
			expectedOperators[4] = ModuleTextFilter.ComparisonConstants.Exact;
			expectedOperators[5] = ModuleTextFilter.ComparisonConstants.CurrentUser;
			expectedOperators[6] = string.Empty;
			AssertContainsExactElementsInAnyOrder(expectedOperators, filter.AllowedComparisonOperators);
		}

		public void TestCreatingUser_WorksWithCommaInCode()
		{
			using (RawDataRegistry.Instance.MultiSearchSeparator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ","))
			{
				var user = Factory.NewWithValidTestData<GlbStaff>();
				user.GS_Code = "A,B";

				var quotedBooking = CreateQuotedBooking();
				quotedBooking.Quote.TH_SystemCreateUser = "A,B";

				var quotedBooking2 = CreateQuotedBooking();
				quotedBooking2.Quote.TH_SystemCreateUser = "C,D";

				Factory.Save();

				var filter = GetNewFilterStripBusinessObject();
				((ModuleNkFilter)filter["Creating User"]).Property = "A,B";
				((ModuleNkFilter)filter["Creating User"]).ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
				((ModuleNkFilter)filter["Creating User"]).IsActive = true;

				var collection = new ViewQuotedBookingCollection(Factory);
				collection.Load(filter.Filter);

				AssertEquals("Found staff with comma separated name", 1, collection.Count);
				AssertEquals("booking PK", quotedBooking.PK, collection[0].QuotedBooking.PK);
			}
		}

		#endregion

		#region TestLastEditUser

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestLastEditUser()
		{
			var initialUserContext = Env.CurrentUserContext;
			try
			{
				var userZA = Factory.NewWithValidTestData<GlbStaff>();
				userZA.GS_Code = "ZA";
				userZA.GS_LoginName = "user1";
				var userZB = Factory.NewWithValidTestData<GlbStaff>();
				userZB.GS_Code = "ZB";
				userZB.GS_LoginName = "user2";
				var userZD = Factory.NewWithValidTestData<GlbStaff>();
				userZD.GS_Code = "ZD";
				userZD.GS_LoginName = "user3";
				var userZE = Factory.NewWithValidTestData<GlbStaff>();
				userZE.GS_Code = "ZE";
				userZE.GS_LoginName = "user4";
				var userBlank = Factory.NewWithValidTestData<GlbStaff>();
				userBlank.GS_Code = ZString.Empty;
				userBlank.GS_LoginName = "user5";
				Factory.Save();

				var quotedBooking1 = CreateBookingOnly();
				var quotedBooking2 = CreateQuotedBooking();
				var quotedBooking3 = CreateQuotedBooking();
				var quotedBooking4 = CreateQuotedBooking();
				Factory.Save(); // FIXME: Force saving as edisupport initially?

				Env.SetUserContext(new UserContext(userZA.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				quotedBooking1.Booking.JS_SystemLastEditUser = "ZA";
				Factory.Save();

				Env.SetUserContext(new UserContext(userZB.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				quotedBooking2.Quote.TH_SystemLastEditUser = "ZB";
				Factory.Save();

				Env.SetUserContext(new UserContext(userZD.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				quotedBooking3.Booking.JS_SystemLastEditUser = "ZD";
				quotedBooking3.Quote.TH_SystemLastEditUser = "ZD";
				Factory.Save();

				Env.SetUserContext(new UserContext(userBlank.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				quotedBooking4.Booking.JS_SystemLastEditUser = ZString.Empty;
				quotedBooking4.Quote.TH_SystemLastEditUser = ZString.Empty;
				Factory.Save();

				var filterStrip = GetNewFilterStripBusinessObject();
				var filter = (ModuleNkFilter)filterStrip["Last Edit User"];
				filter.IsActive = true;
				var collection = new ViewQuotedBookingCollection(Factory);

				#region Exact comparison

				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
				filter.Property = "ZA";
				collection.Load(filterStrip.Filter);

				AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
				AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

				filter.Property = "ZD";
				collection.Load(filterStrip.Filter);

				AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
				AssertEquals("Should have QuotedBooking 3 Booking", quotedBooking3.Booking.PK, collection[0].QuotedBooking.Booking.PK);

				filter.Property = "ZB";
				collection.Load(filterStrip.Filter);

				AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
				AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

				filter.Property = "ZE";
				collection.Load(filterStrip.Filter);

				AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);

				#endregion

				#region NotEqual comparison

				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.NotEqual;
				filter.Property = userZB.GS_Code;
				collection.Load(filterStrip.Filter);

				AssertContainsExactQuotedBookingsInAnyOrder("Should include all bookings not last edited by user 'ZB'.",
					new QuotedBooking[] { quotedBooking1, quotedBooking3, quotedBooking4 },
					collection);

				filter.Property = userZA.GS_Code;
				collection.Load(filterStrip.Filter);

				AssertContainsExactQuotedBookingsInAnyOrder("Should include all bookings not last edited by user 'ZA'.",
					new QuotedBooking[] { quotedBooking2, quotedBooking3, quotedBooking4 },
					collection);

				#endregion

				#region CurrentUser comparison

				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.CurrentUser;
				filter.Property = ZString.Empty;

				Env.SetUserContext(new UserContext(userZD.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				collection.Load(filterStrip.Filter);
				AssertContainsExactQuotedBookingsInAnyOrder("Should contain QuotedBookings last edited by user 'ZD'", new QuotedBooking[] { quotedBooking3 }, collection);

				Env.SetUserContext(new UserContext(userZA.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				filter.InvalidateCachedQuery();
				collection.Load(filterStrip.Filter);
				AssertContainsExactQuotedBookingsInAnyOrder("Should contain QuotedBookings last edited by user 'ZA'", new QuotedBooking[] { quotedBooking1 }, collection);

				#endregion

				#region IsBlank comparison

				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsBlank;
				filter.Property = ZString.Empty;
				collection.Load(filterStrip.Filter);
				AssertNotEquals("Precondition: a user code was autogenerated for userBlank", ZString.Empty, userBlank.GS_Code);
				AssertGreaterThan("Precondition: a user code was autogenerated for userBlank", userBlank.GS_Code.Length, 0);
				AssertContainsExactQuotedBookingsInAnyOrder("Given IsBlank operator, 0 QuotedBookings should be loaded", Array.Empty<QuotedBooking>(), collection);

				#endregion

				#region IsNotBlank comparison

				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsNotBlank;
				filter.Property = ZString.Empty;
				collection.Load(filterStrip.Filter);

				AssertContainsExactQuotedBookingsInAnyOrder("IsNotBlank operator should include all bookings will be loaded",
					new QuotedBooking[] { quotedBooking1, quotedBooking2, quotedBooking3, quotedBooking4 },
					collection);

				#endregion

				#region FiltersMatch comparison

				filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.FiltersMatch;
				filter.SelectedFilters.AddTextFilterStrip("Code", userZD.GS_Code);
				collection.Load(filterStrip.Filter);
				AssertContainsExactQuotedBookingsInAnyOrder("FiltersMatch operator should use the SelectedFitlers", new QuotedBooking[] { quotedBooking3 }, collection);

				#endregion
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		void AssertContainsExactQuotedBookingsInAnyOrder(string message, IEnumerable<QuotedBooking> quotedBookings, ViewQuotedBookingCollection viewQuotedBookings)
		{
			AssertContainsExactElementsInAnyOrder(message, quotedBookings.Select(qb => qb.PK).ToArray(), viewQuotedBookings.Select(vqb => vqb.PK).ToArray());
		}

		public void TestLastEditUser_AllowedComparisonOperators()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			ModuleNkFilter filter = (ModuleNkFilter)filterStrip["Last Edit User"];
			AssertEquals("The Last Edit User filter should use comparison operators", true, filter.HasComparisonOperator);
			string[] expectedOperators = new string[7];
			expectedOperators[0] = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			expectedOperators[1] = ModuleTextFilter.ComparisonConstants.IsBlank;
			expectedOperators[2] = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			expectedOperators[3] = ModuleTextFilter.ComparisonConstants.NotEqual;
			expectedOperators[4] = ModuleTextFilter.ComparisonConstants.Exact;
			expectedOperators[5] = ModuleTextFilter.ComparisonConstants.CurrentUser;
			expectedOperators[6] = string.Empty;
			AssertContainsExactElementsInAnyOrder(expectedOperators, filter.AllowedComparisonOperators);
		}

		public void TestLastEditUser_WorksWithCommaInCode()
		{
			using (RawDataRegistry.Instance.MultiSearchSeparator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ","))
			{
				var user = Factory.NewWithValidTestData<GlbStaff>();
				user.GS_Code = "A,B";
				user.GS_LoginName = "A,B";
				Factory.Save();

				QuotedBooking quotedBooking = null;
				using (CurrentUserChanger.SwitchToNewUserTemporarily("A,B"))
				{
					user = (GlbStaff)EnvProxy.Instance.CurrentUser;
					quotedBooking = CreateQuotedBooking(); // last edit user will be 'A,B'
					Factory.Save();
				}

				CreateQuotedBooking(); // last edit user will be support ('E')
				Factory.Save();

				var filter = GetNewFilterStripBusinessObject();
				((ModuleNkFilter)filter["Last Edit User"]).Property = "A,B";
				((ModuleNkFilter)filter["Last Edit User"]).IsActive = true;

				var collection = new ViewQuotedBookingCollection(Factory);
				collection.Load(filter.Filter);

				AssertEquals("Found staff with comma separated name", 1, collection.Count);
				AssertEquals("booking PK", quotedBooking.PK, collection[0].QuotedBooking.PK);
			}
		}

		public void TestLastEditUser_SpotQuote()
		{
			CreateTestUser("ZA", "userZA");
			CreateTestUser("ZB", "userZB");
			Factory.Save();

			QuotedBooking booking1;

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZA"))
			{
				booking1 = CreateQuoteOnly();
				booking1.Mode = "FRO";
				Factory.Save();
				AssertEquals("Precondition - 1 result for ZA", 0, FetchLastEditUserFilterResults("ZA").Count);
				AssertEquals("Precondition - 0 results for ZB", 0, FetchLastEditUserFilterResults("ZB").Count);
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily("userZB"))
			{
				booking1.Mode = "LSE";
				Factory.Save();
				AssertEquals("Precondition - 1 results for ZB", 0, FetchLastEditUserFilterResults("ZB").Count);
				AssertEquals("Precondition - 1 result for ZA", 0, FetchLastEditUserFilterResults("ZA").Count);
			}
		}

		#endregion

		#region TestCreateTime

		[TestDate(2014, 12, 10)]
		[TestUtcOffset(0, 10, 0)]
		public void TestCreatedTime()
		{
			var now = ZDateTime.UtcNow;

			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_SystemCreateTimeUtc = now;
			quotedBooking1.Quote.TH_SystemCreateTimeUtc = now.AddDays(-2);

			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_SystemCreateTimeUtc = now.AddDays(-4);
			quotedBooking2.Quote.TH_SystemCreateTimeUtc = now.AddDays(-6);

			var quoteOnly = CreateQuoteOnly();
			quoteOnly.Quote.TH_SystemCreateTimeUtc = now.AddDays(-8);

			var bookingOnly1 = CreateBookingOnly();
			bookingOnly1.Booking.JS_SystemCreateTimeUtc = now.AddDays(-10);

			var bookingOnly2 = CreateBookingOnly();
			bookingOnly2.Booking.JS_SystemCreateTimeUtc = now.AddDays(-10).AddHours(-5);

			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO["Created Time"];
			filter.IsActive = true;

			AssertBookingCollectionIsFiltered("No dates entered should return all results with bookings", FilterStripBizO, quotedBooking1, quotedBooking2, bookingOnly1, bookingOnly2);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = now;
			filter.Property2 = now;

			AssertBookingCollectionIsFiltered("quotedBooking1 is no found as we only look at booking when there is quote", FilterStripBizO);

			filter.Property1 = now.AddDays(-6);
			filter.Property2 = now.AddDays(-5);

			AssertBookingCollectionIsFiltered("Quotedbooking found via quote field", FilterStripBizO, quotedBooking2);

			filter.Property1 = now.AddDays(-2);
			filter.Property2 = now.AddDays(-2);

			AssertBookingCollectionIsFiltered("Quotedbooking found via quote field", FilterStripBizO, quotedBooking1);

			filter.Property1 = now.AddDays(-9);
			filter.Property2 = now.AddDays(-7);

			AssertBookingCollectionIsFiltered("Spot quote not found via quote field", FilterStripBizO);

			filter.Property1 = now.AddDays(-10);
			filter.Property2 = now.AddDays(-10);

			AssertBookingCollectionIsFiltered("QuickBooking found via booking field", FilterStripBizO, bookingOnly1);

			filter.Property1 = now.AddDays(-20);
			filter.Property2 = now.AddDays(-15);

			AssertBookingCollectionIsFiltered("No matching results", FilterStripBizO);
		}

		#endregion

		#region TestLastEditTime

		[TestDate(2014, 12, 10)]
		[TestUtcOffset(0, 0, 0)]
		public void TestLastEditTime()
		{
			var quotedBooking = CreateQuotedBooking();
			var timeNow = ZDateTime.UtcNow;

			Factory.Save();

			var lastEditFilter = (ModuleDateFilter)FilterStripBizO["Last Edit Time"];
			lastEditFilter.Property1 = timeNow.AddDays(+1);
			lastEditFilter.Property2 = timeNow.AddDays(+1);
			lastEditFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			lastEditFilter.IsActive = true;

			AssertBookingCollectionIsFiltered("Should have loaded 0 View Quoted Booking", FilterStripBizO);

			lastEditFilter.Property1 = timeNow;
			lastEditFilter.Property2 = timeNow;

			AssertBookingCollectionIsFiltered("Should have 1 Quoted Booking as the last edit time is added by default", FilterStripBizO, quotedBooking);

			lastEditFilter.Property1 = timeNow.AddDays(-3);
			lastEditFilter.Property2 = timeNow.AddDays(-1);

			AssertBookingCollectionIsFiltered("Should have loaded 0 View Quoted Booking", FilterStripBizO);
		}

		#endregion

		#region TestCreatedOn

		public void TestCreatedOn()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBooking();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();

			quotedBooking1.Booking.JS_SystemCreateUser = "ZZ";
			quotedBooking1.Quote.TH_SystemCreateUser = "ZZ";
			quotedBooking2.Booking.JS_SystemCreateUser = "ZD";
			quotedBooking2.Quote.TH_SystemCreateUser = "ZD";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Created On Web/Internal"]).Property = "WEB";
			((ModuleTextFilter)filter["Created On Web/Internal"]).IsActive = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Quote", quotedBooking1.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 1 Booking", quotedBooking1.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleTextFilter)filter["Created On Web/Internal"]).Property = "ENT";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleTextFilter)filter["Created On Web/Internal"]).Property = "ALL";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 2, collection.Count);
		}

		#endregion

		#endregion

		#region IsTemperatureControlled

		public void TestIsTemperatureControlled()
		{
			QuotedBooking quotedBooking1 = CreateBookingOnly();
			QuotedBooking quotedBooking2 = CreateQuotedBooking();
			QuotedBooking quotedBooking3 = CreateQuoteOnly();

			var outerPackLine = quotedBooking1.Booking.OuterPackLines.AddNew();
			outerPackLine.JL_RequiresTemperatureControl = true;

			var innerPackLine = quotedBooking2.Booking.InnerPackLines.AddNew();
			innerPackLine.JL_RequiresTemperatureControl = true;

			Factory.Save();

			var filterManager = GetNewFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterManager["Is Temperature Controlled"];
			filter.IsActive = true;
			filter.Property0 = true;

			ViewQuotedBookingCollection collection = new ViewQuotedBookingCollection(Factory);

			collection.Load(filterManager.Filter);
			AssertEquals("Should have loaded 1 QuotedBooking", 1, collection.Count);
			AssertEquals(quotedBooking1.PK, collection[0].QuotedBooking.PK);
		}

		#endregion

		#region Modes And Types

		#region TestModes

		public void TestModes()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();
			var quotedBooking4 = CreateQuotedBooking();

			quotedBooking1.Mode = Core.Constants.RateMode.LSE;
			quotedBooking2.Mode = Core.Constants.RateMode.FRA;
			quotedBooking3.Mode = Core.Constants.RateMode.FCL;
			quotedBooking4.Mode = Core.Constants.RateMode.LCL;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Mode"]).Property = Core.Constants.RateMode.RAI;
			((ModuleTextFilter)filter["Mode"]).IsActive = true;

			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 2 Quote", quotedBooking2.Quote.PK, collection[0].QuotedBooking.Quote.PK);
			AssertEquals("Should have QuotedBooking 2 Booking", quotedBooking2.Booking.PK, collection[0].QuotedBooking.Booking.PK);

			((ModuleTextFilter)filter["Mode"]).Property = Core.Constants.RateMode.SEA;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 2 View Quoted Booking", 2, collection.Count);
			Assert("Should have QuotedBooking 3 Quote and Booking", collection.Cast<ViewQuotedBooking>().Any(x => x.QuotedBooking.Quote.PK == quotedBooking3.Quote.PK &&
				x.QuotedBooking.Booking.PK == quotedBooking3.Booking.PK));
			Assert("Should have QuotedBooking 4 Quote and Booking", collection.Cast<ViewQuotedBooking>().Any(x => x.QuotedBooking.Quote.PK == quotedBooking4.Quote.PK &&
				x.QuotedBooking.Booking.PK == quotedBooking4.Booking.PK));

			((ModuleTextFilter)filter["Mode"]).Property = Core.Constants.RateMode.FCL;
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			Assert("Should have QuotedBooking 3 Quote and Booking", collection.Cast<ViewQuotedBooking>().Any(x => x.QuotedBooking.Quote.PK == quotedBooking3.Quote.PK &&
				x.QuotedBooking.Booking.PK == quotedBooking3.Booking.PK));
		}

		#endregion

		#region Transport Mode and Container Mode

		public void TestBookingWithQuoteTransportModeAndContainerModeFilter()
		{
			var filter = GetNewFilterStripBusinessObject();
			var filterBookingWithQuoteTransportMode = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.ModesAndTypes.TransportMode]);
			var filterBookingWithQuoteContainerMode = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.ModesAndTypes.ContainerMode]);
			filterBookingWithQuoteTransportMode.IsActive = true;
			filterBookingWithQuoteContainerMode.IsActive = true;
			var containerModeList = QuotedBooking.GetContainerModes(string.Empty, bookingOnly: true);
			AssertContainsExactElementsInAnyOrder("Container modes do not match", filterBookingWithQuoteContainerMode.List, containerModeList);
			var transportModeList = QuotedBooking.GetNewTransportModes();
			AssertContainsExactElementsInAnyOrder("Trasport modes do not match", filterBookingWithQuoteTransportMode.List, transportModeList);
			var quotedBooking1 = CreateQuotedBooking();
			var quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();

			quotedBooking1.TransportMode = "AIR";
			quotedBooking1.ContainerMode = "LSE";
			quotedBooking2.TransportMode = "AIR";
			quotedBooking2.ContainerMode = "ULD";
			quotedBooking3.TransportMode = "RAI";
			quotedBooking3.ContainerMode = "LCL";
			Factory.Save();

			filterBookingWithQuoteTransportMode.Property = "AIR";
			AssertBookingCollectionIsFiltered("BWQ with Transport Mode 'AIR' should be picked.", filter, quotedBooking1, quotedBooking2);

			filterBookingWithQuoteContainerMode.Property = "LSE";
			AssertBookingCollectionIsFiltered("BWQ with Transport Mode 'AIR' and Container Mode 'LSE' should be picked.", filter, quotedBooking1);

			filterBookingWithQuoteTransportMode.Property = "RAI";
			filterBookingWithQuoteContainerMode.Property = "LCL";
			AssertBookingCollectionIsFiltered("BWQ with Transport Mode 'RAI' and Container Mode 'LCL' should be picked.", filter, quotedBooking3);
		}

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var bookingOnly = CreateBookingOnly();

			quotedBooking1.Quote.CurrentOneOffQuote.TT_RS_NKServiceLevel = "AAA";
			bookingOnly.Booking.JS_RS_NKServiceLevel = "BBB";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			((ModuleNkFilter)filter["Service Level"]).Property = "AAA";
			((ModuleNkFilter)filter["Service Level"]).IsActive = true;

			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 0 View Quoted Booking", 0, collection.Count);

			((ModuleNkFilter)filter["Service Level"]).Property = "BBB";
			collection.Load(filter.Filter);

			AssertEquals("Should have loaded 1 View Quoted Booking", 1, collection.Count);
			AssertEquals("Should have QuotedBooking 1 Booking", bookingOnly.Booking.PK, collection[0].QuotedBooking.Booking.PK);
		}

		public void TestServiceTypeDateFilter()
		{
			var bookingOnly = CreateBookingOnly();

			var jobService = bookingOnly.Services.AddNew();
			jobService.ES_Booked = ZDateTime.BrettsBirthday;
			jobService.ES_Completed = ZDateTime.BrettsBirthday.AddDays(10);
			jobService.ES_ServiceCode = "FUM";
			Factory.Save();

			var bookedFilter = (ServiceTypeDateFilter)FilterStripBizO["Service Type / Date Booked"];
			bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			bookedFilter.IsActive = true;
			var completedFilter = (ServiceTypeDateFilter)FilterStripBizO["Service Type / Date Completed"];
			completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			completedFilter.IsActive = true;

			var results = new ViewQuotedBookingCollection(Factory);

			bookedFilter.JobServiceType = "MUF";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("ES_ServiceCode is not matched so there is no data.", false, results.Any());

			bookedFilter.JobServiceType = "FUM";
			bookedFilter.Property2 = ZDateTime.BrettsBirthday.AddDays(-1);
			results.Load(FilterStripBizO.Filter);
			AssertEquals("ES_Booked is not matched so there is no data.", false, results.Any());

			bookedFilter.Property2 = ZDateTime.BrettsBirthday;
			completedFilter.Property1 = ZDateTime.BrettsBirthday.AddDays(11);
			results.Load(FilterStripBizO.Filter);
			AssertEquals("ES_Completed is not matched so there is no data.", false, results.Any());

			completedFilter.Property1 = ZDateTime.BrettsBirthday.AddDays(10);
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should have one data", 1, results.Count);
			AssertEquals(true, results.Contains(bookingOnly));
		}

		#endregion

		#region TestDGClassDGSubstance

		public void TestDGClassDGSubstance()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var packline1 = quotedBooking1.Booking.OuterPackLines.AddNew();
			var undg1 = packline1.UNDGs.AddNew();
			undg1.DI_IMOClass = "1";

			var subsPivot1 = Factory.New<UNDGSubstancePivot>();
			subsPivot1.DP_UNNO = "9999";
			subsPivot1.DP_Variant = "a";
			subsPivot1.DP_ParentId = undg1.PK;
			subsPivot1.DP_ParentTableCode = undg1.TablePrefix;
			subsPivot1.DP_Standard = "IMO";
			subsPivot1.DP_IsDefault = true;

			var quotedBooking2 = CreateQuotedBooking();
			var packline2 = quotedBooking2.Booking.OuterPackLines.AddNew();
			var undg2 = packline2.UNDGs.AddNew();
			undg2.DI_IMOClass = "1.2B";

			var subsPivot2 = Factory.New<UNDGSubstancePivot>();
			subsPivot2.DP_UNNO = "9999";
			subsPivot2.DP_Variant = "b";
			subsPivot2.DP_ParentId = undg2.PK;
			subsPivot2.DP_ParentTableCode = undg2.TablePrefix;
			subsPivot2.DP_Standard = "IMO";
			subsPivot2.DP_IsDefault = true;

			var quotedBooking3 = CreateQuotedBooking();
			var packline3 = quotedBooking3.Booking.OuterPackLines.AddNew();
			var undg3 = packline3.UNDGs.AddNew();
			undg3.DI_IMOClass = "3";

			var subsPivot3 = Factory.New<UNDGSubstancePivot>();
			subsPivot3.DP_UNNO = "9999";
			subsPivot3.DP_Variant = "c";
			subsPivot3.DP_ParentId = undg3.PK;
			subsPivot3.DP_ParentTableCode = undg3.TablePrefix;
			subsPivot3.DP_Standard = "IMO";
			subsPivot3.DP_IsDefault = true;

			var quotedBooking4 = CreateQuotedBooking();
			var packline4 = quotedBooking4.Booking.OuterPackLines.AddNew();
			packline4.JL_Description = "point blank";

			var quotedBooking5 = CreateQuotedBooking();

			Factory.Save();

			DGClassDGSubstanceFilter bookingFilter = (DGClassDGSubstanceFilter)FilterStripBizO["DG Class / DG Substance"];
			ViewQuotedBookingCollection results = new ViewQuotedBookingCollection(Factory);

			bookingFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Not filtered - QuotedBooking1 should be included", true, results.Contains(quotedBooking1.PK));
			AssertEquals("Not filtered - QuotedBooking2 should be included", true, results.Contains(quotedBooking2.PK));
			AssertEquals("Not filtered - QuotedBooking3 should be included", true, results.Contains(quotedBooking3.PK));

			bookingFilter.DGClass = "1";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("QuotedBooking1 should be in the collection", true, results.Contains(quotedBooking1.PK));
			AssertEquals("QuotedBooking2 should be in the collection", true, results.Contains(quotedBooking2.PK));
			AssertEquals("QuotedBooking3 does not have a 1 in its DG Class", false, results.Contains(quotedBooking3.PK));

			bookingFilter.DGClass = "1.2B";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("QuotedBooking1 does not have a 1.2B DG Class", false, results.Contains(quotedBooking1.PK));
			AssertEquals("QuotedBooking2 should be in the collection", true, results.Contains(quotedBooking2.PK));
			AssertEquals("QuotedBooking3 does not have a 1.2B DG Class", false, results.Contains(quotedBooking3.PK));

			bookingFilter.DGClass = "1";
			bookingFilter.DGSubstance = "9999a";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("QuotedBooking1 should be in the collection", true, results.Contains(quotedBooking1.PK));
			AssertEquals("QuotedBooking2 doesnt have a 123a DG Substance", false, results.Contains(quotedBooking2.PK));
			AssertEquals("QuotedBooking3 does not have a 1 in its DG Class or 123a DG Substance", false, results.Contains(quotedBooking3.PK));

			bookingFilter.DGClass = "1.2B";
			bookingFilter.DGSubstance = "9999b";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("QuotedBooking1 doesnt have a 123b DG Substance", false, results.Contains(quotedBooking1.PK));
			AssertEquals("QuotedBooking2 should be in the collection", true, results.Contains(quotedBooking2.PK));
			AssertEquals("QuotedBooking3 does not have a 1.2B DG Class or 123b DG Substance", false, results.Contains(quotedBooking3.PK));

			bookingFilter.DGClass = ZString.Empty;
			bookingFilter.DGSubstance = ZString.Empty;

			bookingFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("QuotedBooking1 should be in the collection", true, results.Contains(quotedBooking1.PK));
			AssertEquals("QuotedBooking2 should be in the collection", true, results.Contains(quotedBooking2.PK));
			AssertEquals("QuotedBooking3 should be in the collection", true, results.Contains(quotedBooking3.PK));
			AssertEquals("QuotedBooking4 should not be in the collection", false, results.Contains(quotedBooking4.PK));
			AssertEquals("QuotedBooking5 should not be in the collection", false, results.Contains(quotedBooking5.PK));

			bookingFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("QuotedBooking1 should not be in the collection", false, results.Contains(quotedBooking1.PK));
			AssertEquals("QuotedBooking2 should not be in the collection", false, results.Contains(quotedBooking2.PK));
			AssertEquals("QuotedBooking3 should not be in the collection", false, results.Contains(quotedBooking3.PK));
			AssertEquals("QuotedBooking4 should be in the collection", true, results.Contains(quotedBooking4.PK));
			AssertEquals("QuotedBooking5 should be in the collection", true, results.Contains(quotedBooking5.PK));

			bookingFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			bookingFilter.DGClass = "1.2";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("QuotedBooking1 should be in the collection", true, results.Contains(quotedBooking1.PK));
			AssertEquals("QuotedBooking2 should not be in the collection because it has 1.2B DG Substance", false, results.Contains(quotedBooking2.PK));
			AssertEquals("QuotedBooking3 should be in the collection", true, results.Contains(quotedBooking3.PK));
			AssertEquals("QuotedBooking4 should be in the collection", true, results.Contains(quotedBooking4.PK));
			AssertEquals("QuotedBooking5 should be in the collection", true, results.Contains(quotedBooking5.PK));

			bookingFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			bookingFilter.DGClass = "1.2";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("QuotedBooking1 should be in the collection", true, results.Contains(quotedBooking1.PK));
			AssertEquals("QuotedBooking2 should be in the collection", true, results.Contains(quotedBooking2.PK));
			AssertEquals("QuotedBooking3 should be in the collection", true, results.Contains(quotedBooking3.PK));
			AssertEquals("QuotedBooking4 should be in the collection", true, results.Contains(quotedBooking4.PK));
			AssertEquals("QuotedBooking5 should be in the collection", true, results.Contains(quotedBooking5.PK));

			bookingFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			bookingFilter.DGClass = ZString.Empty;
			bookingFilter.DGSubstance = "9999b";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("QuotedBooking1 should be in the collection", true, results.Contains(quotedBooking1.PK));
			AssertEquals("QuotedBooking2 should not be in the collection because it has 9999b substance", false, results.Contains(quotedBooking2.PK));
			AssertEquals("QuotedBooking3 should be in the collection", true, results.Contains(quotedBooking3.PK));
			AssertEquals("QuotedBooking4 should be in the collection", true, results.Contains(quotedBooking4.PK));
			AssertEquals("QuotedBooking5 should be in the collection", true, results.Contains(quotedBooking5.PK));

			bookingFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			bookingFilter.DGClass = ZString.Empty;
			bookingFilter.DGSubstance = "XXXX";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("QuotedBooking1 should be in the collection", true, results.Contains(quotedBooking1.PK));
			AssertEquals("QuotedBooking2 should be in the collection", true, results.Contains(quotedBooking2.PK));
			AssertEquals("QuotedBooking3 should be in the collection", true, results.Contains(quotedBooking3.PK));
			AssertEquals("QuotedBooking4 should not be in the collection because it is blank", false, results.Contains(quotedBooking4.PK));
			AssertEquals("QuotedBooking5 should not be in the collection", false, results.Contains(quotedBooking5.PK));
		}

		#endregion

		#endregion

		#region WorkFlow

		public void TestCompletedMilestoneFilter()
		{
			QuotedBooking quotedBooking = CreateQuotedBooking();
			QuotedBooking oneOffQuote = CreateQuoteOnly();

			ProcessTask milestone = quotedBooking.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "DSN";

			ProcessTask milestone2 = oneOffQuote.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "DSN";

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();

			WorkflowModuleTextFilter milestoneCompletedFilter = (WorkflowModuleTextFilter)filter["Milestone Completed"];
			milestoneCompletedFilter.MilestoneEvent = "DSN";
			milestoneCompletedFilter.Property = "Not Completed";
			milestoneCompletedFilter.IsActive = true;

			ViewQuotedBooking[] quotedBookings = Factory.Load<ViewQuotedBooking>(filter.Filter);

			AssertEquals(1, quotedBookings.Length);
			if (FilterStripBizO is QuotedBookingFilterStripBusinessObject)
			{
				AssertEquals(quotedBooking.Quote, quotedBookings[0].QuotedBooking.Quote);
			}
			else
			{
				AssertEquals(oneOffQuote.Quote, quotedBookings[0].QuotedBooking.Quote);
			}
		}

		#endregion

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<ViewQuotedBooking>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.QuickBookingCRMSecurity);
		}

		#endregion

		#region PostCode tests

		public void TestDeliveryPostCodeFilters()
		{
			QuotedBooking quotedBooking1 = CreateQuotedBookingWithDeliveryAddress("ABC1", "2045");
			QuotedBooking quotedBooking2 = CreateQuotedBookingWithDeliveryAddress("ABC2", "2000");
			QuotedBooking quotedBooking3 = CreateQuotedBookingWithDeliveryAddress("ABC3", "4325");

			Factory.Save();

			var collection = new ViewQuotedBookingCollection(Factory);

			ModuleTextFilter postcodeFilter = (ModuleTextFilter)FilterStripBizO["Delivery Address Post Code"];
			postcodeFilter.IsActive = true;

			postcodeFilter.Property = "2045";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK }, GetQuotePks(collection));

			postcodeFilter.Property = "20";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, GetQuotePks(collection));

			postcodeFilter.Property = "4325";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking3.Quote.PK }, GetQuotePks(collection));

			quotedBooking3.Booking.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			quotedBooking3.Booking.ConsigneeDeliveryAddress.E2_Postcode = "2076";

			Factory.Save();

			postcodeFilter.Property = "20";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK, quotedBooking3.Quote.PK }, GetQuotePks(collection));
		}

		QuotedBooking CreateQuotedBookingWithDeliveryAddress(string orgHeaderCode, string postcode)
		{
			QuotedBooking booking = CreateQuotedBooking();
			ForwardingShipment shipment = booking.Booking;

			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = orgHeaderCode;

			shipment.ConsigneePK = header.PK;

			OrgAddress address = header.MainAddress;
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			address.OA_Address1 = "28 Long St";
			address.OA_PostCode = postcode;

			return booking;
		}

		public void TestPickupPostCodeFilters()
		{
			var quotedBooking1 = CreateQuotedBookingWithPickupAddress("ABC1", "2045");
			var quotedBooking2 = CreateQuotedBookingWithPickupAddress("ABC2", "2000");
			var quotedBooking3 = CreateQuotedBookingWithPickupAddress("ABC3", "4325");

			Factory.Save();

			var collection = new ViewQuotedBookingCollection(Factory);

			ModuleTextFilter postcodeFilter = (ModuleTextFilter)FilterStripBizO["Pickup Address Post Code"];
			postcodeFilter.IsActive = true;

			postcodeFilter.Property = "2045";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK }, GetQuotePks(collection));

			postcodeFilter.Property = "20";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, GetQuotePks(collection));

			postcodeFilter.Property = "4325";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking3.Quote.PK }, GetQuotePks(collection));

			quotedBooking3.Booking.ConsignorPickupAddress.E2_AddressOverride = true;
			quotedBooking3.Booking.ConsignorPickupAddress.E2_Postcode = "2076";

			Factory.Save();

			postcodeFilter.Property = "20";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK, quotedBooking3.Quote.PK }, GetQuotePks(collection));
		}

		QuotedBooking CreateQuotedBookingWithPickupAddress(string orgHeaderCode, string postcode)
		{
			QuotedBooking booking = CreateQuotedBooking();
			ForwardingShipment shipment = booking.Booking;

			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = orgHeaderCode;

			shipment.ConsignorPK = header.PK;

			OrgAddress address = header.MainAddress;
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Pickup);
			address.OA_Address1 = "28 Long St";
			address.OA_PostCode = postcode;

			return booking;
		}

		#endregion

		#region TestPickupTransportCompany

		public void TestPickupTransportCompany()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CCCC";
			orgHeader3.OH_IsActive = false;

			var orgAddr1 = orgHeader1.MainAddress;
			var orgAddr2 = orgHeader2.MainAddress;
			var orgAddr3 = orgHeader3.MainAddress;

			quotedBooking1.Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddr1.PK;
			quotedBooking2.Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddr2.PK;
			quotedBooking3.Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddr3.PK;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filter["Pickup Transport"];
			orgFilter.Property = orgHeader1.PK;
			orgFilter.IsActive = true;

			var collection = new ViewQuotedBookingCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK }, GetQuotePks(collection));

			orgFilter.Property = Guid.NewGuid();
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);

			orgFilter.Property = orgHeader1.PK;
			orgFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			collection.Load(filter.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking2.Quote.PK, quotedBooking3.Quote.PK }, GetQuotePks(collection));

			AssertNoWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			orgFilter.Property = orgHeader3.PK;
			orgFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			collection.Load(filter.Filter);
			AssertHasWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking3.Quote.PK }, GetQuotePks(collection));
		}

		public void TestPickupTransportCompany_BlankOperators()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			var orgAddr1 = orgHeader1.MainAddress;
			var orgAddr2 = orgHeader2.MainAddress;

			quotedBooking1.Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddr1.PK;
			quotedBooking2.Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddr2.PK;

			Factory.Save();

			var collection = new ViewQuotedBookingCollection(Factory);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Pickup Transport"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Contains 1 booking", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking3.Quote.PK }, GetQuotePks(collection));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Contains quotedBooking1 & quotedBooking2", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, GetQuotePks(collection));
		}

		#region Test Common Numbers Filter

		public void TestCommonNumbersFilter()
		{
			var quotedBooking1 = CreateQuotedBooking();
			var quotedBooking2 = CreateQuotedBooking();
			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00000123";
			quotedBooking2.Booking.JS_HouseBill = "1234";
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Common Numbers and References"];
			var collection = new ViewQuotedBookingCollection(Factory);

			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK, quotedBooking3.Quote.PK }, GetQuotePks(collection));

			filter.Property = "123";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking1.Quote.PK, quotedBooking2.Quote.PK }, GetQuotePks(collection));

			filter.Property = "1234";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { quotedBooking2.Quote.PK }, GetQuotePks(collection));
		}

		#endregion

		#endregion

		#region CO2e

		public void TestQuotedBookingCO2eFilter()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var booking1 = CreateQuotedBookingForCO2e(Core.Constants.ContainerModes.Loose, 200m, 2000m, Core.Constants.Weight.Kilograms);
				booking1.SetTotalCO2e(400m);
				var booking2 = CreateQuotedBookingForCO2e(Core.Constants.ContainerModes.LCL, 2000m, 0.3m, Core.Constants.Weight.Tonnes);
				booking2.SetTotalCO2e(600m);
				var booking3 = CreateQuotedBookingForCO2e(Core.Constants.ContainerModes.FCL, 2000m, 0.4m, Core.Constants.Weight.Tonnes,
					packs: new[] { (100m, Core.Constants.Weight.Kilograms), (0.3m, Core.Constants.Weight.Tonnes) });
				booking3.SetTotalCO2e(800m);
				var booking4 = CreateQuotedBookingForCO2e(Core.Constants.ContainerModes.ULD, 1059.6m, 1m, Core.Constants.Weight.Tonnes,
					containers: new[] { ("20FR", 2), ("40FR", 1) });
				booking4.SetTotalCO2e(1059.6m);
				var booking5 = CreateQuotedBookingForCO2e(Core.Constants.ContainerModes.FTL, 212.72m, 10m, Core.Constants.Weight.Tonnes,
					packs: new[] { (100m, Core.Constants.Weight.Kilograms), (0.3m, Core.Constants.Weight.Tonnes) },
					containers: new[] { ("20FR", 2), ("40FR", 1) });
				booking5.SetTotalCO2e(2127.2m);
				var booking6 = CreateQuotedBookingForCO2e(Core.Constants.ContainerModes.Loose, 1111.2727m, 2000m, Core.Constants.Weight.Kilograms);
				booking6.SetTotalCO2e(2222.5454m);
				var booking7 = CreateQuotedBooking();

				Factory.Save();
				var filter = (CO2eStatusAndCO2eKgRangeNumberFilter)FilterStripBizO[BaseQuotedBookingFilterStripBusinessObject.Descriptions.NumbersAndReferences.CO2e];
				filter.CO2eStatus = CO2eStatusList.Codes.Current;

				AssertWeightRangeResult(filter, 0, 390, Array.Empty<QuotedBooking>());
				AssertWeightRangeResult(filter, 400, 799, new[] { booking1, booking2 });
				AssertWeightRangeResult(filter, 601, 1100, new[] { booking3, booking4 });
				AssertWeightRangeResult(filter, 2127, 2127.2m, new[] { booking5 });
				AssertWeightRangeResult(filter, 2222.54m, 2222.55m, new[] { booking6 });
				AssertWeightRangeResult(filter, 2222.542m, 2222.546m, new[] { booking6 });

				((ICO2eProvider)booking1).SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
				Factory.Save();
				filter.CO2eStatus = CO2eStatusList.Codes.NotCurrent;
				AssertWeightRangeResult(filter, 0, 0, new[] { booking1 });

				((ICO2eProvider)booking2).SetCO2eStatus(CO2eStatusList.Codes.Pending);
				Factory.Save();
				filter.CO2eStatus = CO2eStatusList.Codes.Pending;
				AssertWeightRangeResult(filter, 0, 0, new QuotedBooking[] { booking2 });

				((ICO2eProvider)booking3).SetCO2eStatus(CO2eStatusList.Codes.Rejected);
				Factory.Save();
				filter.CO2eStatus = CO2eStatusList.Codes.Rejected;
				AssertWeightRangeResult(filter, 0, 0, new QuotedBooking[] { booking3 });

				filter.CO2eStatus = CO2eStatusList.Codes.NotCalculated;
				AssertWeightRangeResult(filter, 0, 0, new QuotedBooking[] { booking7 });

				((ICO2eProvider)booking1).SetCO2eStatus(CO2eStatusList.Codes.Current);
				Factory.Save();
				filter.CO2eStatus = CO2eStatusList.Codes.Current;
				AssertWeightRangeResult(filter, 400, 799, new[] { booking1 });

				((ICO2eProvider)booking1).SetCO2eStatus(CO2eStatusList.Codes.Pending);
				((ICO2eProvider)booking3).SetCO2eStatus(CO2eStatusList.Codes.Pending);
				Factory.Save();
				filter.CO2eStatus = CO2eStatusList.Codes.Pending;
				AssertWeightRangeResult(filter, 0, 0, new[] { booking1, booking2, booking3 });
			}

			QuotedBooking CreateQuotedBookingForCO2e(string mode, decimal cO2ePerTonneInKg, decimal shipmentWeight, string shipmentWeightUQ, (decimal PackWeight, string PackWeightUQ)[] packs = null, (string containerType, int containerCount)[] containers = null)
			{
				var quotedBooking = CreateQuotedBooking();
				quotedBooking.Mode = mode;
				quotedBooking.Booking.JS_ActualWeight = shipmentWeight;
				quotedBooking.Booking.JS_UnitOfWeight = shipmentWeightUQ;
				quotedBooking.SetCO2ePerTonneInKg(cO2ePerTonneInKg);
				quotedBooking.Booking.OuterPackLines.RemoveAndDeleteAll();
				quotedBooking.QuotedBookingContainers.RemoveAndDeleteAll();

				if (packs != null)
				{
					foreach (var (packWeight, packWeightUQ) in packs)
					{
						var pack = quotedBooking.Booking.OuterPackLines.AddNew();
						pack.JL_ActualWeight = packWeight;
						pack.JL_ActualWeightUQ = packWeightUQ;
					}
				}

				if (containers != null)
				{
					var refContainerLoader = new RefContainer.Loader(Factory);
					foreach (var (containerType, containerCount) in containers)
					{
						var container = quotedBooking.QuotedBookingContainers.AddNew();
						container.JC_ContainerCount = (short)containerCount;
						container.JC_RC = refContainerLoader.LoadFromCode(containerType).PK;
					}
				}

				return quotedBooking;
			}

			void AssertWeightRangeResult(ModuleNumberRangeFilter filter, ZDecimal property1, ZDecimal property2, QuotedBooking[] expectedQuotedBookings)
			{
				filter.IsActive = true;
				filter.Property1 = property1;
				filter.Property2 = property2;

				var collection = new ViewQuotedBookingCollection(Factory);
				collection.Load(filter.Query);
				AssertContainsExactQuotedBookingsInAnyOrder($"CO2e between {property1} and {property2}", expectedQuotedBookings, collection);
			}
		}

		#endregion

		public void TestFMCTariffIDFilter()
		{
			var filter = GetNewFilterStripBusinessObject();
			var fmcTariffIDFilter = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.NumbersAndReferences.FMCTariffID]);
			fmcTariffIDFilter.IsActive = true;

			var booking1 = CreateBookingOnly();
			var booking2 = CreateBookingOnly();
			var booking3 = CreateBookingOnly();
			var unacceptedBWQ = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var acceptedBWQ = CreateQuotedBooking();
			var oneOffQuote = CreateQuoteOnly();
			booking1.FMCTariffID = "ABCD";
			booking2.FMCTariffID = "EBCD";
			booking3.FMCTariffID = "";
			unacceptedBWQ.FMCTariffID = "ABCD";
			acceptedBWQ.FMCTariffID = "ABED";
			oneOffQuote.FMCTariffID = "ABCD";
			Factory.Save();

			AssertEquals("FMCTariffID MaxLength", 4, fmcTariffIDFilter.MaxLength);

			SetUpFilter("A", ModuleTextFilter.ComparisonConstants.StartsWith);
			AssertBookingCollectionIsFiltered("Searching for QB & BWQ whose Booking.FMCTariffID starts with A", filter, booking1, acceptedBWQ, unacceptedBWQ);

			SetUpFilter("E", ModuleTextFilter.ComparisonConstants.Contains);
			AssertBookingCollectionIsFiltered("Searching for QB & BWQ whose Booking.FMCTariffID contains E", filter, booking2, acceptedBWQ);

			SetUpFilter("", ModuleTextFilter.ComparisonConstants.IsBlank);
			AssertBookingCollectionIsFiltered("Searching for QB & BWQ whose Booking.FMCTariffID is blank", filter, booking3);

			void SetUpFilter(string property, string comparisonOperator)
			{
				fmcTariffIDFilter.Property = property;
				fmcTariffIDFilter.ComparisonOperator = comparisonOperator;
			}
		}

		public void TestCommodityCodeFilter()
		{
			var commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";
			var commodityCode2 = Factory.New<RefCommodityCode>();
			commodityCode2.RH_Code = "COM2";
			var commodityCode3 = Factory.New<RefCommodityCode>();
			commodityCode3.RH_Code = "COM3";

			var filter = GetNewFilterStripBusinessObject();
			var commodityCodeFilter = ((ModuleNkFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.NumbersAndReferences.CommodityCode]);
			commodityCodeFilter.IsActive = true;

			var booking1 = CreateBookingOnly();
			var booking2 = CreateBookingOnly();
			var booking3 = CreateBookingOnly();
			var unacceptedBWQ = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var acceptedBWQ = CreateQuotedBooking();
			var oneOffQuote = CreateQuoteOnly();
			booking1.Commodity = commodityCode1.RH_Code;
			booking2.Commodity = commodityCode2.RH_Code;
			booking3.Commodity = commodityCode3.RH_Code;
			unacceptedBWQ.Commodity = commodityCode1.RH_Code;
			acceptedBWQ.Commodity = commodityCode2.RH_Code;
			oneOffQuote.Commodity = commodityCode3.RH_Code;
			Factory.Save();

			commodityCodeFilter.Property = commodityCode1.RH_Code;
			AssertBookingCollectionIsFiltered("Searching for QB & BWQ whose Booking.Commodity is COM1", filter, booking1, unacceptedBWQ);

			commodityCodeFilter.Property = commodityCode2.RH_Code;
			AssertBookingCollectionIsFiltered("Searching for QB & BWQ whose Booking.Commodity is COM2", filter, booking2, acceptedBWQ);

			commodityCodeFilter.Property = commodityCode3.RH_Code;
			AssertBookingCollectionIsFiltered("Searching for QB & BWQ whose Booking.Commodity is COM3", filter, booking3);
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Job.JH_ProfitLossReasonCode = "ND1";

			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Job.JH_ProfitLossReasonCode = "CD1";

			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Job.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var results = new ViewQuotedBookingCollection(Factory);

			var filter = GetNewFilterStripBusinessObject();

			var profitLossReasonFilter = (ModuleTextFilter)filter["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			AssertBookingCollectionIsFiltered("Result Should include quotedBooking1", filter, quotedBooking1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			AssertBookingCollectionIsFiltered("Result Should include quotedBooking1", filter, quotedBooking1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			AssertBookingCollectionIsFiltered("Result Should include quotedBooking1 and quotedBooking2", filter, quotedBooking1, quotedBooking2);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			AssertBookingCollectionIsFiltered("Result Should include quotedBooking2 and quotedBooking3", filter, quotedBooking2, quotedBooking3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			results.Load(FilterStripBizO.Filter);
			AssertBookingCollectionIsFiltered("Result Should include quotedBooking2 and quotedBooking3", filter, quotedBooking2, quotedBooking3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			AssertBookingCollectionIsFiltered("Result Should include quotedBooking2 and quotedBooking3", filter, quotedBooking2, quotedBooking3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			AssertBookingCollectionIsFiltered("Result Should include quotedBooking3", filter, quotedBooking3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			AssertBookingCollectionIsFiltered("Result Should include quotedBooking1and quotedBooking2", filter, quotedBooking1, quotedBooking2);
		}

		public void TestFilterHBLDeliveryMode_GivenBookingWithHBLDeliveryMode_ThenBookingCollectionShouldBeFiltered()
		{
			var quotedBooking = CreateQuotedBooking();
			quotedBooking.ContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var hBLDeliveryModeFilter = ((ModuleTextFilter)filter[BaseQuotedBookingFilterStripBusinessObject.Descriptions.ModesAndTypes.HBLDeliveryMode]);
			hBLDeliveryModeFilter.IsActive = true;
			hBLDeliveryModeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			hBLDeliveryModeFilter.Property = Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT;
			AssertBookingCollectionIsFiltered("Result should include Booking with HBL Delivery Mode:DOOR_ARPT", filter, quotedBooking);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var filterStripBO = new QuotedBookingFilterStripBusinessObject();
			filterStripBO.QueryObjectType = typeof(ViewQuotedBooking);
			return filterStripBO;
		}

		#endregion
	}
}
