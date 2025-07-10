namespace Enterprise.Freight.Business
{
	internal class NoActionSailingScheduleDataVendor : SailingScheduleDataVendor
	{
		protected override bool IsEnabledCore
		{
			get { return false; }
		}

		protected override bool IsVendorDataCurrentCore
		{
			get { return false; }
		}

		public override string Status
		{
			get { return ""; }
		}

		protected override void UpdateVoyageOriginCore(VoyageOrigin origin)
		{
		}

		protected override void UpdateVoyageDestinationCore(VoyageDestination destination)
		{
		}
	}
}
