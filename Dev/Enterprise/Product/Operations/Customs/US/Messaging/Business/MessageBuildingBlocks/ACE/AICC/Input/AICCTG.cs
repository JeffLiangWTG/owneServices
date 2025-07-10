namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("TG")]
	public partial class AICCTG : MessageBlock
	{
		public AICCTG()
			: base("TG")
		{
		}

		/// <summary>
		/// A 6-digit North American Industry Classification Systems code unique to the importer.
		/// </summary>
		[MessageBlockString(6, 3, "O")]
		public ZString NAICSCode;

		/// <summary>
		/// A 9-digit code Dun & Bradstreet identification number unique to the importer.
		/// </summary>
		[MessageBlockString(9, 9, "O")]
		public ZString DUNS;

		/// <summary>
		/// Entry Filer's identification code (as assigned by CBP).
		/// </summary>
		[MessageBlockString(3, 18, "O")]
		public ZString FilerCode;

		/// <summary>
		/// The year the importer was officially established.
		/// </summary>
		[MessageBlockString(4, 21, "O")]
		public ZString YearEstablished;

		/// <summary>
		/// The state code where the importer's Certificate or Articles of Incorporation was filed.
		/// This will be used as the importer's Certificate or Articles of Incorporation locator ID.
		/// </summary>
		[MessageBlockString(2, 25, "O")]
		public ZString State;

		/// <summary>
		/// The International Organization for Standardization (ISO) country code representing the country where the importer's Certificate or Articles of Incorporation was filed. Provide the appropriate two-position ISO code for all foreign addresses including Canada. Valid ISO country codes are listed in ISO 3166-1, located on the www.ISO.org website.
		/// 
		/// This will be used as the importer's Certificate or Articles of Incorporation locator ID.
		/// </summary>
		[MessageBlockString(2, 27, "O")]
		public ZString CountryISOCode;

		/// <summary>
		/// A reference number where the importer's Certificate or Articles of Incorporation was filed.
		/// </summary>
		[MessageBlockString(30, 29, "O")]
		public ZString Reference;

		/// <summary>
		/// Standard Carrier Alpha Code. The SCAC of the vessel operator that is transporting the container.
		/// </summary>
		[MessageBlockString(4, 59, "O")]
		public ZString SCACIdentifier;

		/// <summary>
		/// Facilities Information and Resources Management System. The FIRMS code of the location where the cargo is currently stored. 
		/// </summary>
		[MessageBlockString(4, 63, "O")]
		public ZString LocationOfGoods;
	}
}
