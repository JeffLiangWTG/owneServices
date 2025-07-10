using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	sealed class QuotedBookingContractAllocationCodeFindBoxTest : TestCaseWithFactory
	{
		public void TestGetNewPopupFormType()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			using (var findBox = new QuotedBookingContractAllocationCodeFindBoxForTest())
			{
				findBox.SetDataBinding(quotedBooking, "CarrierContractNumber");
				using var popup = findBox.GetNewPopupForm_ForTest();
				AssertEquals("Popup form should return ContractAllocationFindBoxPopup", popup.GetType(), typeof(ContractAllocationFindBoxPopup));
			}
		}

		class QuotedBookingContractAllocationCodeFindBoxForTest : QuotedBookingContractAllocationCodeFindBox
		{
			public IFindBoxPopup GetNewPopupForm_ForTest() => GetNewPopupForm();
		}
	}
}
