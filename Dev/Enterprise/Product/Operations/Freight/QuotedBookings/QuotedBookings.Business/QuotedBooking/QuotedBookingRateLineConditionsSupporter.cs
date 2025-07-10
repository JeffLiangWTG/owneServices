using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingRateLineConditionsSupporter : RateLineConditionsSupporter
	{
		public QuotedBookingRateLineConditionsSupporter(QuotedBooking objectToWrap) : base(objectToWrap) { }

		QuotedBooking quotedBooking;
		QuotedBooking QuotedBooking
		{
			get { return quotedBooking ?? (quotedBooking = ObjectToWrap as QuotedBooking); }
		}

		IAutoRatingFreightConditionsSupportable BookingSupportable
		{
			get { return QuotedBooking.Booking != null ? QuotedBooking.Booking.RatingAdapter as IAutoRatingFreightConditionsSupportable : null; }
		}

		protected override OrgHeader GetDepartureCFS()
		{
			return BookingSupportable != null ? BookingSupportable.ConditionsSupporter.DepartureCFS : null;
		}

		protected override OrgHeader GetArrivalCFS()
		{
			return BookingSupportable != null ? BookingSupportable.ConditionsSupporter.ArrivalCFS : null;
		}

		protected override OrgHeader GetExportBroker()
		{
			return QuotedBooking.ExportBroker;
		}

		protected override OrgHeader GetImportBroker()
		{
			return QuotedBooking.ImportBroker;
		}

		protected override OrgHeader GetSendingAgent()
		{
			return null;
		}

		protected override OrgHeader GetReceivingAgent()
		{
			return null;
		}

		protected override OrgHeader GetControllingAgent()
		{
			return QuotedBooking.ControllingAgentDocumentaryAddress != null ? QuotedBooking.ControllingAgentDocumentaryAddress.Organisation : null;
		}

		protected override bool GetHasDangerousGoods()
		{
			return QuotedBooking.Booking != null && QuotedBooking.Booking.OuterPackLines.Cast<PackLine>().Any(p => p.UNDGs.Any());
		}

		public override bool? MeetsCondition(ZString condition, ZString conditionalExpression, Func<ZString, RateLineConditionsSupporter, bool?> isUserDefinedConditionMet, bool isOrgFrt, bool isDstFrt)
		{
			if (QuotedBooking.Booking == null)
			{
				switch (condition)
				{
					case RateLineConditions.OwnBrokerage:
					case RateLineConditions.HandOver:
					case RateLineConditions.ForwardingAndBrokerage:
					case RateLineConditions.OwnCFS:
						return true;
				}
			}

			return base.MeetsCondition(condition, conditionalExpression, isUserDefinedConditionMet, isOrgFrt, isDstFrt);
		}
	}
}
