namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	// duplicate record in document
	[InputBlock("PG16")]
	public abstract partial class PGAPG16 : MessageBlock // Need to add interface for BIRD System
	{
		public PGAPG16()
			: base("PG16")
		{
		}

		/// <summary>
		/// This code identifies what role the country had in regards to a product, for example, harvest, grown, produced, processed, sold, or raw material. Also, includes a code for vessel flag to identify the country where the vessel is registered.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString SourceTypeCode;

		/// <summary>
		/// A two letter code that identifies where the harvest, growth, production, processing sale, catch or raw material took place. Valid codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 8, "M")]
		public ZString CountryCode;

		/// <summary>
		/// The state, province or geographical location (example –North Atlantic Sea) where the harvest, growth, production, sale, catch or raw material took place.
		/// </summary>
		[MessageBlockString(20, 10, "C")]
		public ZString GeographicLocation;

		/// <summary>
		/// The date range when the processing for the product occurred. The date range can be “MMDDYY” (month, day, year) or “MMDDYYMMDDYY” (month, day, year, month, day year) format.
		/// </summary>
		[MessageBlockString(12, 30, "C")]
		public ZString RangeOfProcessingDate; // needs string

		/// <summary>
		/// The code identifying what method of processing used to make the product. This includes fishing/method to catch fish.
		/// </summary>
		[MessageBlockString(4, 42, "C")]
		public ZString ProcessingType;

		/// <summary>
		/// Text describing the processing methods. Mandatory if “Other” is used
		/// </summary>
		[MessageBlockString(35, 46, "C")]
		public ZString ProcessingDescription;
	}
}
