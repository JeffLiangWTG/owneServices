namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("GE21")]
	public partial class AGE21 : MessageBlock
	{
		public AGE21()
			: base("GE21")
		{
		}

		/// <summary>
		/// If reported this field must always equal MF.
		///
		/// This field is reported to indicate that commercial entity may act as Manufacturer/Producer.
		/// </summary>
		[MessageBlockString(2, 5, "O")]
		public ZString ManufacturerRole;

		/// <summary>
		/// If reported this field must always equal SH.
		///
		/// This field is reported to indicate that commercial entity may act as Shipper.
		/// </summary>
		[MessageBlockString(2, 7, "O")]
		public ZString ShipperRole;

		/// <summary>
		/// If reported this field must always equal SE.
		///
		/// This field is reported to indicate that commercial entity may act as Seller.
		/// </summary>
		[MessageBlockString(2, 9, "O")]
		public ZString SellerRole;

		/// <summary>
		/// If reported this field must always equal EX.
		///
		/// This field is reported to indicate that commercial entity may act as Exporter.
		/// </summary>
		[MessageBlockString(2, 11, "O")]
		public ZString ExporterRole;

		/// <summary>
		/// If reported this field must always equal PK.
		///
		/// This field is reported to indicate that commercial entity may act as Packager.
		/// </summary>
		[MessageBlockString(2, 13, "O")]
		public ZString PackagerRole;

		/// <summary>
		/// If reported this field must always equal DR.
		///
		/// This field is reported to indicate that commercial entity may act as Distributor.
		/// </summary>
		[MessageBlockString(2, 15, "O")]
		public ZString DistributorRole;
	}
}
