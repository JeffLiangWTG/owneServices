using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(RelatedTransportBookingsOfJobDeclarationFilter))]
	sealed class RelatedTransportBookingsOfJobDeclarationFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFilter()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = "IMP";
			var dec2 = Factory.New<BaseJobDeclaration>();
			dec2.JE_MessageType = "EXP";
			var shipment = Factory.New<ForwardingShipment>();
			dec2.JE_JS = shipment.PK;
			var dec3 = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = "IMP";
			var bookingConsolidation0 = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation0.KB_ParentID = dec.PK;
			bookingConsolidation0.KB_ParentTableCode = "JE";
			bookingConsolidation0.KB_JobDirection = "PIC";
			var bookingConsolidation1 = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation1.KB_ParentID = dec.PK;
			bookingConsolidation1.KB_ParentTableCode = "JE";
			bookingConsolidation1.KB_JobDirection = "DLV";
			var bookingConsolidation2 = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation2.KB_ParentID = dec2.PK;
			bookingConsolidation2.KB_ParentTableCode = "JE";
			bookingConsolidation2.KB_JobDirection = "PIC";
			var bookingConsolidation3 = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation3.KB_ParentID = shipment.PK;
			bookingConsolidation3.KB_ParentTableCode = "JS";
			bookingConsolidation3.KB_JobDirection = "PIC";
			var bookingConsolidation4 = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation4.KB_ParentID = dec3.PK;
			bookingConsolidation4.KB_ParentTableCode = "JE";
			bookingConsolidation4.KB_JobDirection = "DLV";
			var booking0 = Factory.New<DtbBooking>();
			booking0.KM_JobID = "TB000";
			booking0.KM_KB_Booking = bookingConsolidation0.PK;
			var booking1 = Factory.New<DtbBooking>();
			booking1.KM_JobID = "TB001";
			booking1.KM_KB_Booking = bookingConsolidation1.PK;
			var booking2 = Factory.New<DtbBooking>();
			booking2.KM_JobID = "TB002";
			booking2.KM_KB_Booking = bookingConsolidation2.PK;
			var booking3 = Factory.New<DtbBooking>();
			booking3.KM_JobID = "TB003";
			booking3.KM_KB_Booking = bookingConsolidation3.PK;
			var booking4 = Factory.New<DtbBooking>();
			booking4.KM_JobID = "TT001";
			booking4.KM_KB_Booking = bookingConsolidation4.PK;
			Factory.Save();
			var filterStripBiz0 = new JobDeclarationFilterBusinessObject();
			var filter = (RelatedTransportBookingsOfJobDeclarationFilter)filterStripBiz0["Related Transport Bookings"];
			filter.IsActive = true;
			var jobDeclarations = new BaseJobDeclarationCollection(Factory);
			filter.SelectedFilters.AddTextFilterStrip("Booking ID", "TB001");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { dec }, jobDeclarations);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { dec2, dec3 }, jobDeclarations);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertEquals(0, jobDeclarations.Count);
			filter.Clear();
			filter.SelectedFilters.AddTextFilterStrip("Booking ID", "TB");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { dec, dec2 }, jobDeclarations);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { dec3 }, jobDeclarations);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { dec, dec2 }, jobDeclarations);
			filter.Clear();
			filter.SelectedFilters.AddTextFilterStrip("Booking ID", "TB003");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { dec2 }, jobDeclarations);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { dec, dec3 }, jobDeclarations);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertEquals(0, jobDeclarations.Count);
		}

		public void TestAllMatch_ShouldUseParameterisedTextForSubQuery()
		{
			var filterBizo = new JobDeclarationFilterBusinessObject();
			var filter = filterBizo.AddFilterStrip<RelatedTransportBookingsOfJobDeclarationFilter>("Related Transport Bookings");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			filter.SelectedFilters.AddTextFilterStrip("Booking ID", "TB001");
			var sql = filterBizo.Filter.ParameterisedText.ParameterisedQueryText;
			AssertContains("The sub query should use parameters rather than string literals. SAD!" + System.Environment.NewLine + sql, "@FOREIGN", sql);
			var parameters = filterBizo.Filter.Params;
			AssertEquals("The renamed parameters must actually be declared, too, or the query obviously won't work. SAD!", true, parameters.Any(x => x.ParameterName.StartsWith("@FOREIGN")));
		}

		protected override BusinessObject GetNewBusinessObject() => new RelatedTransportBookingsOfJobDeclarationFilter("Test", () => new ArrayList());
	}
}
