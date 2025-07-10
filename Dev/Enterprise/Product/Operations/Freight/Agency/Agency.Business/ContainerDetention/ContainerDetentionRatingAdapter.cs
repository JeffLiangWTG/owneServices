using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.Agency.Business
{
	// We only Implement IAutoRating here because auto-rating won't work without it.
	// This job should not match anything, instead it's the additional jobs that should do
	// all the matching.
	class ContainerDetentionRatingAdapter : RatingAdapter<ContainerDetention>
	{
		public ContainerDetentionRatingAdapter(ContainerDetention containerDetention)
			: base(containerDetention)
		{
		}

		#region IAutoRating Members

		public override AdapterType AdapterType => AdapterType.ContainerDetention;

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new JobDatesProvider<ContainerDetention>(Parent); }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.AgencyDetentionInvoice; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override AutoRatingStatusInfo StatusInformation
		{
			get
			{
				if (Parent.Movements.Count == 0)
				{
					return new AutoRatingStatusInfo(false, Res.GetString("6ba3bb5c-6319-45b1-8dbe-4b9cc2a6b5f3", "There are no containers relating to this detention job."));
				}
				else
				{
					return new AutoRatingStatusInfo(true, "");
				}
			}
		}

		#endregion

		#region IAutoRatingFreightInfo Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override FreightMode FreightMode
		{
			get { return FreightMode.SEA | FreightMode.Containerised; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public override ZString ContainerMode
		{
			get { return Core.Constants.ContainerModes.FCL; }
		}

		public override Directions JobDirection
		{
			get
			{
				switch (Parent.NC_DetentionType)
				{
					case DetentionInvoiceType.Codes.Import:
						return Directions.Import;
					case DetentionInvoiceType.Codes.Export:
						return Directions.Export;
					default:
						return Directions.Unknown;
				}
			}
		}

		public override RateType RateTypeToUse
		{
			get
			{
				switch (Parent.NC_DetentionType)
				{
					case DetentionInvoiceType.Codes.Import:
						return RateType.ShippingExportDetention;
					case DetentionInvoiceType.Codes.Export:
						return RateType.ShippingImportDetention;

					default:
						return 0;
				}
			}
		}

		#endregion
	}
}
