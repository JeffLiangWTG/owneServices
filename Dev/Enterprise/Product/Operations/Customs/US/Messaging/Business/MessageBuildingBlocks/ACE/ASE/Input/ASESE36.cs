namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE36")]
	public abstract partial class ASESE36 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE36()
			: base("SE36")
		{
		}

		/// <summary>
		/// The city portion of the address where the entity is located.
		/// </summary>
		[MessageBlockString(35, 5, "M", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString CityName;

		/// <summary>
		/// ISO subdivision code. Space fill if not applicable.
		/// 
		/// A list of country codes and their subdivision codes may be found at: 
		/// http://www.unece.org/cefact/locode/service/location.html
		/// </summary>
		[MessageBlockString(3, 40, "C")]
		public ZString CountrySubEntityCode;

		/// <summary>
		/// Postal code (i.e. ZIP code in USA). Space fill if no postal code is available.
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
