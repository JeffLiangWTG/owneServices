namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	using CargoWise.Types;

	// This message block has been removed from the latest spec but we still keep it here for old messages
	[InputBlock("SF36")]
	public partial class FTZSF36 : MessageBlock
	{
		public FTZSF36()
			: base("SF36")
		{
		}

		/// <summary>
		/// The city portion of the address where the entity is located.
		/// </summary>
		[MessageBlockString(35, 5, "M")]
		public ZString CityName;

		/// <summary>
		/// ISO subdivision code - http://www.unece.org/cefact/locode/service/sublocat.htm .
		/// </summary>
		[MessageBlockString(3, 40, "C")]
		public ZString CountrySubEntityCode;

		/// <summary>
		/// Postal code (i.e.: ZIP code in USA) Space fill if no postal code is available.
		/// </summary>
		[MessageBlockString(15, 49, "C")]
		public ZString PostalCode;

		/// <summary>
		/// The International Standards Organization (ISO) country code representing the country portion of the address. A list of ISO country codes is found in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 64, "M")]
		public ZString CountryCode;
	}
}
