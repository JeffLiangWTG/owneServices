namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	using CargoWise.Types;
	// N03 is different for N00 to N01

	[InputBlock("N03")]
	[OutputBlock("N03")]
	public partial class INPN03ForN01 : MessageBlock
	{
		public INPN03ForN01()
			: base("N03")
		{
		}

		/// <summary>
		/// The telephone or telex number of the Notify Party, if available. Place the word TELEX before the telex number. For the telephone number, insert only the number. If neither number is available, the N03 record may contain a fourth address line.
		/// </summary>
		[MessageBlockString(35, 4, "M")]
		public ZString NotifyPartyTelephoneOrTelexNumber;
	}

	[InputBlock("N03")]
	[OutputBlock("N03")]
	public partial class INPN03 : MessageBlock
	{
		public INPN03()
			: base("N03")
		{
		}

		/// <summary>
		/// Free form text for the name of the city.
		/// </summary>
		[MessageBlockString(19, 4, "C", IsSpecialReplacingBehaviourOfInvalidCharacterOn = true)]
		public ZString CityName;

		/// <summary>
		/// The U.S. postal code for the State/Province.
		/// </summary>
		[MessageBlockString(2, 23, "C", IsSpecialReplacingBehaviourOfInvalidCharacterOn = true)]
		public ZString StateProvinceCode;

		/// <summary>
		/// A code representing the international postal zone code (ZIP in U.S.).
		/// </summary>
		[MessageBlockString(9, 25, "C", IsSpecialReplacingBehaviourOfInvalidCharacterOn = true)]
		public ZString PostalCode;

		/// <summary>
		/// An ISO code identifying the country of origin. See Appendix G for valid ISO Country codes.
		/// </summary>
		[MessageBlockString(2, 34, "C")]
		public ZString CountryCode;
	}
}
