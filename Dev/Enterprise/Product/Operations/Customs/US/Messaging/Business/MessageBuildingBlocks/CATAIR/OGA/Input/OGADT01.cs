namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("DT01")]
	public abstract partial class OGADT01 : MessageBlock // Need to add interface for BIRD System
	{
		public OGADT01()
			: base("DT01")
		{
		}

		/// <summary>
		/// A code identifying the line number beginning with 001 and incremented by one for each subsequent DOT line number.
		/// </summary>
		[MessageBlockInt(3, 5, "M")]
		public ZInt DOTLineNumber;

		/// <summary>
		/// The box number as described on the NHTSA HS-7 form. If boxes one through nine are checked on the form, transmit 01 through 09. NOTE: Box 2 is no longer valid and is replaced by boxes 2A or 2B. Also note that Box 13 is not currently programmed in ACS and cannot be used.
		/// </summary>
		[MessageBlockString(2, 8, "M")]
		public ZString BoxNumber;

		/// <summary>
		/// A code of Y (yes) indicating that the filer certifies the data. No other code is accepted.
		/// </summary>
		[MessageBlockString(1, 10, "M")]
		public ZString BoxCertification;

		/// <summary>
		/// The passport number of the person importing the vehicle. The passport number is mandatory if box 05 of the HS-7 is checked.
		/// </summary>
		[MessageBlockString(19, 11, "C")]
		public ZString PassportNumber;

		/// <summary>
		/// The International Organization for Standardization (ISO) country code identifying the country of origin. The ISO country code is required if box 05, 06, or 12 of the HS-7 is checked.
		/// </summary>
		[MessageBlockString(2, 30, "C")]
		public ZString CountryISO;

		/// <summary>
		/// The number assigned to the DOT bond.
		/// </summary>
		[MessageBlockString(3, 32, "C")]
		public ZString DOTBondSuretyCode;

		/// <summary>
		/// A code of Y (yes) indicating that the importer has a copy of the prior approval letter and the official orders; otherwise, space fill. This code is mandatory if boxes 2B, 06, 07, or 12 of the HS-7 are checked.
		/// </summary>
		[MessageBlockString(1, 35, "C")]
		public ZString NHTSAPermissionLetterOfficialOrdersCertification;

		/// <summary>
		/// A code of Y (yes) indicating that the importer has a copy of the Importers Substantiating Statement and contract; otherwise, space fill. This code is mandatory if boxes 2B, 03, 07, 08 or 09 of the HS-7 are checked.
		/// </summary>
		[MessageBlockString(1, 36, "C")]
		public ZString ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter;

		/// <summary>
		/// A code identifying the merchandise being imported. Valid codes are:
		/// 
		/// V = Vehicle
		/// E = Equipment
		/// T = Tire
		/// 
		/// If “V” is selected in this position, a DT02 record may be required.
		/// </summary>
		[MessageBlockString(1, 37, "C")]
		public ZString ClarificationCode;

		/// <summary>
		/// If “T” is transmitted in Clarification Code field, then transmit the tire ID code.
		/// </summary>
		[MessageBlockString(3, 38, "O")]
		public ZString TireManufacturerIDCode;

		/// <summary>
		/// Brand name of the tire associated with the ID code.
		/// </summary>
		[MessageBlockString(20, 41, "O")]
		public ZString TireManufacturerBrandName;
	}
}
