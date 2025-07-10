using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ViewOneOffQuoteCollection))]
	public class ViewOneOffQuoteCollectionTest : ViewQuotedBookingCollectionTest
	{
		public override void TestModuleID()
		{
			AssertEquals(ModuleIDs.OneOffQuotes, ZMetaData.GetModuleId(Collection));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ViewOneOffQuoteCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ViewQuotedBooking>();
		}
	}
}
