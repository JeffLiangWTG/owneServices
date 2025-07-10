using CargoWise.Types;
using CargoWise.UniversalCopy;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public static class QuotedBookingHelper
	{
		public enum ReceiverOrganization
		{
			PackDepot,
			CTO,
			SeaCTO,
		}

		public enum DeliveryOrganization
		{
			UnpackDepot,
			CTO
		}

		public static ReceiverOrganization GetReceiverOrganizationTypeFromMode(ZString quotedBookingMode)
		{
			switch (quotedBookingMode)
			{
				case Core.Constants.RateMode.FCL:
					return ReceiverOrganization.SeaCTO;

				case Core.Constants.RateMode.LSE:
				case Core.Constants.RateMode.ULD:
				case Core.Constants.RateMode.LCL:
				case Core.Constants.RateMode.LRO:
				case Core.Constants.RateMode.FTL:
				case Core.Constants.RateMode.LRA:
					return ReceiverOrganization.PackDepot;

				case Core.Constants.RateMode.FRO:
				case Core.Constants.RateMode.COU:
				case Core.Constants.RateMode.FRA:
					return ReceiverOrganization.CTO;

				default:
					return ReceiverOrganization.PackDepot;
			}
		}

		public static (string DeliveryOrgTitle, string PickupOrgTitle) GetPickupDeliveryOrgTitles(ZString quotedBookingMode)
		{
			var pickupOrgTitle = string.Empty;
			var deliveryOrgTitle = string.Empty;

			switch (QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(quotedBookingMode))
			{
				case QuotedBookingHelper.ReceiverOrganization.SeaCTO:
				case QuotedBookingHelper.ReceiverOrganization.CTO:
					pickupOrgTitle = Res.GetString("QuotedBookings|ForwardingBookingAdditionalDetailsControl|PickupCTO", "Pickup CTO");
					deliveryOrgTitle = Res.GetString("QuotedBookings|ForwardingBookingAdditionalDetailsControl|DeliveryCTO", "Delivery CTO");
					break;

				case QuotedBookingHelper.ReceiverOrganization.PackDepot:
					pickupOrgTitle = Res.GetString("QuotedBookings|ForwardingBookingAdditionalDetailsControl|PickupCFS", "Pickup CFS");
					deliveryOrgTitle = Res.GetString("QuotedBookings|ForwardingBookingAdditionalDetailsControl|DeliveryCFS", "Delivery CFS");
					break;

				default:
					pickupOrgTitle = Res.GetString("QuotedBookings|ForwardingBookingAdditionalDetailsControl|PickupCFS", "Pickup CFS");
					deliveryOrgTitle = Res.GetString("QuotedBookings|ForwardingBookingAdditionalDetailsControl|DeliveryCFS", "Delivery CFS");
					break;
			}

			return (deliveryOrgTitle, pickupOrgTitle);
		}

		public static DeliveryOrganization GetDeliveryOrganizationTypeFromMode(ZString quotedBookingMode)
		{
			switch (quotedBookingMode)
			{
				case Core.Constants.RateMode.LSE:
				case Core.Constants.RateMode.ULD:
				case Core.Constants.RateMode.LCL:
				case Core.Constants.RateMode.LRO:
				case Core.Constants.RateMode.FTL:
				case Core.Constants.RateMode.LRA:
					return DeliveryOrganization.UnpackDepot;

				case Core.Constants.RateMode.FCL:
				case Core.Constants.RateMode.FRO:
				case Core.Constants.RateMode.COU:
				case Core.Constants.RateMode.FRA:
					return DeliveryOrganization.CTO;

				default:
					return DeliveryOrganization.UnpackDepot;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public static string ValidateUniversalCopyPreconditions(CopyTemplateTree configurationTree, QuotedBooking quotedBooking)
		{
			var innerNode = (EntityCopyTemplateNode)configurationTree.InnerNode;
			var quoteNode = ((RelatedEntityCopyTemplateNode)innerNode.Nodes.Find(node => node.Name == "Quote"));
			var bookingNode = ((RelatedEntityCopyTemplateNode)innerNode.Nodes.Find(node => node.Name == "Booking"));

			//New logic: Validation failure in any case where we'd try to make something that lacks both a quote AND a booking.

			if ((quoteNode == null || quoteNode.CopyMethod == RelatedEntityCopyMethod.None) && (bookingNode == null || bookingNode.CopyMethod == RelatedEntityCopyMethod.None))
			{
				return Res.GetString("fc8aa791-34e3-464c-a6c3-dacdb4136505",
					"This copy template cannot be used to copy the {0}, because the template does not copy a Quote or Booking.",
					quotedBooking.HumanReadableNameWithoutID);
			}

			if (quotedBooking.Booking == null && quotedBooking.Quote == null)
			{
				return Res.GetString("8deeeaa1-6bd4-473e-9245-e153246b81e1",
					"This copy template cannot be used to copy the {0} because the {0} is missing a Quote and a Booking.",
					quotedBooking.HumanReadableNameWithoutID);
			}

			if ((quoteNode == null || quoteNode.CopyMethod == RelatedEntityCopyMethod.None) && quotedBooking.Booking == null)
			{
				return Res.GetString("37D4B9A3-6F28-441C-96C0-2664DDD25790",
					"This copy template copies only Booking but the {0} is missing a Booking, and thus cannot create a valid copy.",
					quotedBooking.HumanReadableNameWithoutID);
			}

			if ((bookingNode == null || bookingNode.CopyMethod == RelatedEntityCopyMethod.None) && quotedBooking.Quote == null)
			{
				return Res.GetString("BEC81965-5CE3-4B8F-BD3B-DAE3DB775FE2",
					"This copy template copies only Quote but the {0} is missing a Quote, and thus cannot create a valid copy.",
					quotedBooking.HumanReadableNameWithoutID);
			}

			return null;
		}
	}
}
