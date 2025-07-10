using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	sealed class QuotedBookingContractAllocationGuidFindBoxTest : TestCaseWithFactory
	{
		public void TestGetNewPopupFormType()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			using (var findBox = new QuotedBookingContractAllocationGuidFindBoxForTest())
			{
				findBox.SetDataBinding(quotedBooking, "AllocationLinePK");
				using var popup = findBox.GetNewPopupForm_ForTest();
				AssertEquals("Popup form should return ContractAllocationFindBoxPopup", popup.GetType(), typeof(ContractAllocationFindBoxPopup));
			}
		}

		class QuotedBookingContractAllocationGuidFindBoxForTest : QuotedBookingContractAllocationGuidFindBox
		{
			public IFindBoxPopup GetNewPopupForm_ForTest() => GetNewPopupForm();
		}
	}
}
