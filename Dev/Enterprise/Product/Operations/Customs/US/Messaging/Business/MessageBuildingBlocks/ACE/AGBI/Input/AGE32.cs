namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("GE32")]
	public partial class AGE32 : MessageBlock
	{
		public AGE32()
			: base("GE32")
		{
		}

		/// <summary>
		/// The city portion of the address where the entity is located.
		/// </summary>
		[MessageBlockString(35, 5, "M")]
		public ZString City;

		/// <summary>
		/// The state/province portion of the address where the entity is located.
		/// </summary>
		[MessageBlockString(24, 40, "O")]
		public ZString StateProvince;

		/// <summary>
		/// The International Standards Organization (ISO) country code representing the country portion of the address.
		/// A list of ISO country codes is found in CATAIR Appendix B.
		/// </summary>
		[MessageBlockString(2, 64, "M")]
		public ZString Country;

		/// <summary>
		/// Postal Code
		/// </summary>
		[MessageBlockString(15, 66, "O")]
		public ZString PostalCode;
	}
}
