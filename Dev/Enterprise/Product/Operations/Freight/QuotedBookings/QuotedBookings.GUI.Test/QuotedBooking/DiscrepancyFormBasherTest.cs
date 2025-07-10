using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	[TestedType(typeof(DiscrepancyForm))]
	public class DiscrepancyFormBasherTest : ZFormBasherTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			QuotedBooking result = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			result.TryLoadOrCreateJob();
			result.Job.JH_GE = Env.CurrentDepartment.PK;
			Factory.Save();
			return new DiscrepancyForm(result, false);
		}
		#endregion
	}
}
