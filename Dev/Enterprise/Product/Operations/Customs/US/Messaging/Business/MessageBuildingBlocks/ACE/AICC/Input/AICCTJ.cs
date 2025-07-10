namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("TJ")]
	public partial class AICCTJ : MessageBlock
	{
		public AICCTJ()
			: base("TJ")
		{
		}

		/// <summary>
		/// A two digit Number where if record TI exist in the CATAIR message, then the TJ record must follow TI record and TJ Line Item Number must match respective Line Item Number of TI.
		/// </summary>
		[MessageBlockInt(2, 3, "M")]
		public ZInt LineItem;

		/// <summary>
		/// The numerical code used to identify a
		/// Passport.
		/// </summary>
		[MessageBlockString(13, 5, "O")]
		public ZString Passport;

		/// <summary>
		/// The date (MMDDYYYY) in which the Passport expires.
		/// Required if the Passport # is provided.
		/// </summary>
		[MessageBlockDate(18, "C", "MMddyyyy")]
		public ZDate ExpirationDate;

		/// <summary>
		/// The 2 character country code where the Passport was issued. Valid ISO country codes are listed in ISO 3166-1, located on the www.ISO.org website.
		/// 
		/// Required if Passport # is provided.
		/// </summary>
		[MessageBlockString(2, 26, "C")]
		public ZString CountryOfIssuance;

		/// <summary>
		/// A 1-digit numerical code to identify the type of passport.
		/// 
		/// Required if Passport # is provided.
		/// </summary>
		[MessageBlockString(1, 28, "C")]
		public ZString PassportType;

		/// <summary>
		/// Provide a phone number to contact the owner or officer within the importer's company.
		/// 
		/// If applicable, provide country code.
		/// </summary>
		[MessageBlockString(15, 29, "M")]
		public ZString Phone;

		/// <summary>
		/// If applicable, provide a phone extension to contact the owner or officer within the importer's company.
		/// </summary>
		[MessageBlockString(6, 44, "O")]
		public ZString Extension;

		/// <summary>
		/// Provide an email to contact the owner or officer within the importer's company.
		/// 
		/// If there is not enough room, provide overflow with Record Identifier TN. Maximum length is 100X.
		/// </summary>
		[MessageBlockString(30, 50, "M")]
		public ZString Email;
	}
}
