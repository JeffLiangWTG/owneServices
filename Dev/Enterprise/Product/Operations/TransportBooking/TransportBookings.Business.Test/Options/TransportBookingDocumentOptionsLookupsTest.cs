using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class TransportBookingDocumentOptionsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBookingTemplatesNoFilter()
		{
			var bookingOptions = new TransportBookingDocumentOptions(null, Factory, DataContextType.DummyBusinessObject, DtbBookingDirection.DLV, false, "", "", false);
			var expectedTmpls = new DtbBookingTmplCollection(Factory, new ZQuery(DtbBookingTmplSchema.KT_IsActive, true));
			var actualTmpls = bookingOptions.Lookups.BookingTemplates;
			AssertContainsExactElementsInAnyOrder(expectedTmpls, actualTmpls);

			var bookingParent = Factory.New<DummyWithDtbBooking>();
			bookingParent.TransportBookingTemplateFilters = null;
			bookingOptions = new TransportBookingDocumentOptions(bookingParent, Factory, DataContextType.DummyBusinessObject, DtbBookingDirection.DLV, false, "", "", false);
			expectedTmpls = new DtbBookingTmplCollection(Factory, new ZQuery(DtbBookingTmplSchema.KT_IsActive, true));
			actualTmpls = bookingOptions.Lookups.BookingTemplates;
			AssertContainsExactElementsInAnyOrder(expectedTmpls, actualTmpls);
		}

		public void TestBookingTemplatesWithFilter()
		{
			var bookingParent = Factory.New<DummyWithDtbBooking>();
			var bookingOptions = new TransportBookingDocumentOptions(bookingParent, Factory, DataContextType.DummyBusinessObject, DtbBookingDirection.DLV, false, "", "", false);

			var bookingTmpl1 = Factory.New<DtbBookingTmpl>();
			bookingTmpl1.KT_Code = "ABCD";
			bookingTmpl1.KT_IsActive = true;

			var bookingTmpl2 = Factory.New<DtbBookingTmpl>();
			bookingTmpl2.KT_Code = "BCDE";
			bookingTmpl2.KT_IsActive = true;

			var bookingTmpl3 = Factory.New<DtbBookingTmpl>();
			bookingTmpl3.KT_Code = "CDEF";
			bookingTmpl3.KT_IsActive = true;

			var bookingTmpl4 = Factory.New<DtbBookingTmpl>();
			bookingTmpl4.KT_Code = "DEFG";
			bookingTmpl4.KT_IsActive = false;

			Factory.Save();

			bookingParent.TransportBookingTemplateFilters = new ZQuery(DtbBookingTmplSchema.KT_Code, SQLComparisonOperator.Equal, bookingTmpl2.KT_Code)
				.AddToFilter(JoinCondition.Or, DtbBookingTmplSchema.KT_Code, SQLComparisonOperator.Equal, bookingTmpl3.KT_Code)
				.AddToFilter(JoinCondition.Or, DtbBookingTmplSchema.KT_Code, SQLComparisonOperator.Equal, bookingTmpl4.KT_Code);
			AssertContainsExactElementsInAnyOrder(new[] { bookingTmpl2, bookingTmpl3 }, bookingOptions.Lookups.BookingTemplates);

			var notEqualsTmpl2AndTmpl3Query = new ZQuery(DtbBookingTmplSchema.KT_Code, SQLComparisonOperator.NotEqual, bookingTmpl2.KT_Code)
				.AddToFilter(JoinCondition.And, DtbBookingTmplSchema.KT_Code, SQLComparisonOperator.NotEqual, bookingTmpl3.KT_Code);
			bookingParent.TransportBookingTemplateFilters = notEqualsTmpl2AndTmpl3Query;
			AssertContainsExactElementsInAnyOrder(new DtbBookingTmplCollection(Factory, new ZQuery(DtbBookingTmplSchema.KT_IsActive, true).AddToFilter(notEqualsTmpl2AndTmpl3Query)), bookingOptions.Lookups.BookingTemplates);
		}

		public void TestNewTransportBookingDocumentOptionsLookups_WithNullParent()
		{
			AssertExceptionThrown<ArgumentNullException>("Parent cannot be null.", () => new TransportBookingDocumentOptionsLookups(null));
		}
	}
}
