using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.Module.Test;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Workflow.Module.Test;

class ProcessFieldChangeRuleHookTests : TestCaseWithFactory
{
	public void TestSHPEventShouldNotFireDuringQuotedBookingInitialisation()
	{
		var rule = Factory.New<ProcessFieldChangeRule>();
		rule.PFR_ProcessType = "SHP";
		rule.PFR_GroupName = "Group1";
		rule.PFR_SE_NKEvent = AutoEvents.CustomisableEvent00Code;
		rule.PFR_Reference = "BLAH";

		rule.Fields.AddNew().PFL_FieldName = "JS_GoodsDescription";
		Factory.Save();

		var quoteOnlyController = new QuotedBookingControllerForTest(QuotedBookingState.BookingOnly);
		var quotedBooking = (QuotedBooking)quoteOnlyController.GetNewBusinessEntityInLocalFactoryForTest();
		quotedBooking.Booking.JS_GoodsDescription = "Beets";
		Factory.Save();

		AssertEquals("Expecting QBK should not fire customisable SHP module event", 0, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == AutoEvents.CustomisableEvent00Code));

		quoteOnlyController = new QuotedBookingControllerForTest(QuotedBookingState.AcceptedBookingWithQuote);
		quotedBooking = (QuotedBooking)quoteOnlyController.GetNewBusinessEntityInLocalFactoryForTest();
		quotedBooking.Booking.JS_GoodsDescription = "Beets";
		Factory.Save();

		AssertEquals("Expecting QBK should not fire customisable SHP module event", 0, quotedBooking.Booking.Logs.GetAllLogs().OfType<StmALog>().Count(log => log.SL_SE_NKEvent == AutoEvents.CustomisableEvent00Code));
	}
}
