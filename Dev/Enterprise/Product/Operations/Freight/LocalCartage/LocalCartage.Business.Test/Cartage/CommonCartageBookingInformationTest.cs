using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using BookingCodes = Enterprise.Freight.Business.FreightConstants.LocalCartageBookingStatus.Codes;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartageBookingInformation))]
	public class CommonCartageBookingInformationTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestBookingCommentMaxLength()
		{
			AssertEquals("Booking comment Max Length", 120, BookingInformation.BookingCommentInfo.MaxLength);
			CommonCartageBookingInformation bookingInfo = new CommonCartageBookingInformation(Cartage);
			try
			{
				bookingInfo.BookingComment = "x".PadRight(121, 'x');
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		public void TestConstructor()
		{
			AddUpdateEventToCartageAndSave(BookingCodes.BookingAccepted + "-" + FreightConstants.LocalCartageBookingStatus.Description.BookingAccepted);
			CommonCartageBookingInformation bookingInfo = new CommonCartageBookingInformation(Cartage);
			AssertEquals("BookingAccepted after instantiating", ZBool.True, bookingInfo.BookingAccepted);
			AssertEquals("BookingAccepted after instantiating", ZBool.False, bookingInfo.BookingRejected);
			AddUpdateEventToCartageAndSave(BookingCodes.BookingRejected + "-" + FreightConstants.LocalCartageBookingStatus.Description.BookingRejected);
			bookingInfo = new CommonCartageBookingInformation(Cartage);
			AssertEquals("BookingRejected after instantiating", ZBool.True, bookingInfo.BookingRejected);
		}

		public void TestSetDefaults()
		{
			AssertEquals("SetDefaults()", true, BookingInformation.BookingStatus.IsEmpty);
		}

		public void TestBookingAccepted()
		{
			AssertEquals("BookingAccepted", ZBool.False, BookingInformation.BookingAccepted);
			BookingInformation.BookingAccepted = ZBool.True;
			AssertEquals("BookingAccepted", ZBool.True, BookingInformation.BookingAccepted);
		}

		public void TestBookingRejected()
		{
			AssertEquals("BookingRejected", ZBool.False, BookingInformation.BookingRejected);
			BookingInformation.BookingRejected = ZBool.True;
			AssertEquals("BookingRejected", ZBool.True, BookingInformation.BookingRejected);
		}

		public void TestBookingStatus()
		{
			AddUpdateEventToCartageAndSave(BookingCodes.BookingAccepted + "-" + FreightConstants.LocalCartageBookingStatus.Description.BookingAccepted);
			BookingInformation.BookingStatus = "123";
			AssertEquals("Should have notifications", true, BookingInformation.BookingStatusInfo.HasNotifications());
			AssertEquals("Precondition: can select status", true, BookingInformation.CanSelectStatus);
			BookingInformation.BookingStatus = FreightConstants.LocalCartageBookingStatus.Codes.WorkCommenced;
			AssertEquals("Should not have notifications", true, !BookingInformation.BookingStatusInfo.HasNotifications());
		}

		public void TestCalculatedBookingStatusWhenBookingAccepted()
		{
			AddUpdateEventToCartageAndSave(BookingCodes.BookingAccepted + "-" + FreightConstants.LocalCartageBookingStatus.Description.BookingAccepted);
			CommonCartageBookingInformation bookingInfo = new CommonCartageBookingInformation(Cartage);
			bookingInfo.BookingStatus = FreightConstants.LocalCartageBookingStatus.Codes.WorkCommenced;
			AssertEquals("Should be booking status", bookingInfo.BookingStatus, bookingInfo.CalculatedBookingStatus);
			bookingInfo.BookingStatus = ZString.Empty;
			AssertEquals("Should be 'BookingAccepted'", BookingCodes.BookingAccepted, bookingInfo.CalculatedBookingStatus);
		}

		public void TestCalculatedBookingStatusWhenBookingRejected()
		{
			AddUpdateEventToCartageAndSave(BookingCodes.BookingRejected + "-" + FreightConstants.LocalCartageBookingStatus.Description.BookingRejected);
			AssertEquals("Should be 'BookingAccepted'", BookingCodes.BookingRejected, BookingInformation.CalculatedBookingStatus);
		}

		public void TestBookingComment()
		{
			BookingInformation.BookingComment = "some comments";
			AssertEquals("Booking comment", "some comments", BookingInformation.BookingComment);
		}

		public void TestCanSelectStatus()
		{
			AssertEquals("CanSelectStatus)", false, BookingInformation.CanSelectStatus);
			AddUpdateEventToCartageAndSave(BookingCodes.BookingAccepted + "-" + FreightConstants.LocalCartageBookingStatus.Description.BookingAccepted);
			AssertEquals("CanSelectStatus)", true, BookingInformation.CanSelectStatus);
		}

		public void TestCanSelectStatusWhenCartageWithParent()
		{
			SetParentToCartage();
			AssertEquals("CanSelectStatus)", true, BookingInformation.CanSelectStatus);
		}

		public void TestCanExposeBookingAction()
		{
			AssertEquals("CanExposeBookingAction)", true, BookingInformation.CanExposeBookingAction);
		}

		public void TestBookingActionList_WhenCanSelectStatus()
		{
			AddUpdateEventToCartageAndSave(BookingCodes.BookingAccepted + "-" + FreightConstants.LocalCartageBookingStatus.Description.BookingAccepted);
			AssertEquals("Precondition", true, BookingInformation.CanSelectStatus);
			AssertEquals("Should be CartageJobBookingStatusList", FreightCodePairLists.CartageJobBookingStatusList().GetType(), BookingInformation.BookingStatusList.GetType());
		}

		public void TestBookingActionList_CartageWhenHasParent()
		{
			SetParentToCartage();
			AssertEquals("Should be CartageJobFWDBookingActionList", FreightCodePairLists.CartageJobFWDBookingActionList().GetType(), BookingInformation.BookingStatusList.GetType());
		}

		public void TestBookingActionList()
		{
			AssertEquals("Should be CartageJobBookingActionList", FreightCodePairLists.CartageJobBookingActionList().GetType(), BookingInformation.BookingStatusList.GetType());
		}

		CommonCartageBookingInformation BookingInformation
		{
			get
			{
				if (fBookingInformation == null)
				{
					fBookingInformation = new CommonCartageBookingInformation(Cartage);
				}

				return fBookingInformation;
			}
		}

		CommonCartageBookingInformation fBookingInformation;

		CommonCartage Cartage
		{
			get
			{
				if (fCartage == null)
				{
					fCartage = Factory.New<CommonCartage>();
				}

				return fCartage;
			}
		}

		CommonCartage fCartage;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Cartage.BookingInformation;
		}

		void AddUpdateEventToCartageAndSave(ZString reference)
		{
			Cartage.Logs.AddNew(Events.StatusUpdated, reference);
			Factory.Save();
		}

		void SetParentToCartage()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonConsol consol = Factory.New<CommonConsol>();
			Cartage.JJ_ParentID = shipment.PK;
			Cartage.JJ_ForeignKeyToConsol = consol.PK;
		}
	}
}
