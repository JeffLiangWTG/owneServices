namespace Enterprise.Freight.QuotedBookings.Business
{
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business;
	using Enterprise.Security;
	using static Enterprise.Rating.Business.RateOneOffShipment;

	public class QuoteInvoicingSupporter : JobInvoicingSupporter
	{
		public QuoteInvoicingSupporter(QuotedBooking parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly QuotedBooking Parent;

		public override ZDecimal ActualChargeable
		{
			get { return Parent.Chargeable; }
		}

		public override ZString ActualChargeableUnit
		{
			get { return Parent.ChargeableUnit; }
		}

		public override ZDecimal ActualVolume
		{
			get { return Parent.Volume; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return Parent.VolumeUnit; }
		}

		public override ZDecimal ActualWeight
		{
			get { return Parent.Weight; }
		}

		public override ZString ActualWeightUnit
		{
			get { return Parent.WeightUnit; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.OneOffQuote;
		}

		public override OrgHeader Consignee
		{
			get { return Parent.ConsigneeDocumentaryAddress.Organisation; }
		}

		public override OrgHeader Consignor
		{
			get { return Parent.ConsignorDocumentaryAddress.Organisation; }
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			var currentQuote = Parent.Quote.CurrentOneOffQuote;
			var carrier = currentQuote.Carrier;
			var creditor = currentQuote.Creditor;
			var possibleCarriers = currentQuote.PossibleCarriers;

			// If Carrier matches RateProviderOrgPK, return Creditor or Carrier
			if (carrier != null
				&& carrier.PK == defaultCreditorSetting.RateProviderOrgPK)
			{
				return creditor ?? carrier;
			}

			if (creditor != null
				&& creditor.PK == defaultCreditorSetting.RateProviderOrgPK)
			{
				return creditor;
			}

			// Check PossibleCarriers for a match on RateProviderOrgPK for Creditor
			var matchedCarrier = possibleCarriers
				.OfType<RateOneOffCarrier>()
				.FirstOrDefault(pc => pc.Creditor != null && pc.Creditor.PK == defaultCreditorSetting.RateProviderOrgPK);

			if (matchedCarrier != null)
			{
				return matchedCarrier.Creditor;
			}

			// Check PossibleCarriers for a match on RateProviderOrgPK for Carrier
			matchedCarrier = possibleCarriers
				.OfType<RateOneOffCarrier>()
				.FirstOrDefault(pc => pc.Carrier?.PK == defaultCreditorSetting.RateProviderOrgPK);

			if (matchedCarrier != null)
			{
				return matchedCarrier.Creditor ?? matchedCarrier.Carrier;
			}

			return base.GetDefaultCreditor(defaultCreditorSetting);
		}

		public override bool ProviderPreferred => false;

		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		public override bool IsQuote
		{
			get { return true; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.OneOffQuotation; }
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !Parent.IsInDatabaseIncludingChildren; }
		}

		public override RefUNLOCO Destination
		{
			get { return Parent.DestinationUNLOCO; }
		}

		public override ZDateTime ATA
		{
			get { return Parent.DeliveryClose; }
		}

		public override ZDateTime ATD
		{
			get { return Parent.PickupClose; }
		}

		public override ZDateTime ETA
		{
			get { return Parent.DeliveryOpen; }
		}

		public override ZDateTime ETD
		{
			get { return Parent.PickupReady; }
		}

		public override ZString HouseBillNumber
		{
			get { return ZString.Empty; }
		}

		public override PaymentTermInfos PaymentTerm
		{
			get { return Parent.Quote.CurrentOneOffQuote.RatingAdapter.PaymentTerm; }
		}

		public override bool IsImport
		{
			get { return Parent.IsImport(); }
		}

		public override bool IsExport
		{
			get { return Parent.IsExport(); }
		}

		public override bool IsDomestic
		{
			get { return Parent.IsDomestic(); }
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.OneOffQuoteJobInvoicing;
		}

		public override RefUNLOCO Origin
		{
			get { return Parent.OriginUNLOCO; }
		}

		public override ZString EditSecurityMessage
		{
			get { return EditSecurityMessageCore; }
		}

		protected virtual ZString EditSecurityMessageCore
		{
			get { return ZString.Empty; }
		}

		public override bool EditSecurityLock
		{
			get { return EditSecurityLockCore; }
		}

		protected virtual bool EditSecurityLockCore
		{
			get { return false; }
		}

		public override ZString TransportMode
		{
			get { return Parent.TransportMode; }
		}

		public override ZString ContainerMode
		{
			get { return FreightModeConverter.GetBookingContainerModeFromContainerMode(Parent.ContainerMode); }
		}

		public override int ContainerCount
		{
			get { return Parent.ContainerCount; }
		}

		public override ZString CustomsEntryNumberType => Parent.Booking?.CustomsEntryNumberType ?? base.CustomsEntryNumberType;

		public override ZString CommunityTransitStatus => Parent.Booking?.JS_CommunityTransitStatus ?? base.CommunityTransitStatus;
	}
}
