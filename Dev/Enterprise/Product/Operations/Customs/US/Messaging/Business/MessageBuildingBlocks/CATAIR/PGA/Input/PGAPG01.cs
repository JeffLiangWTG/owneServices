namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("PG01")]
	public abstract partial class PGAPG01 : MessageBlock // Need to add interface for BIRD System
	{
		public PGAPG01()
			: base("PG01")
		{
		}

		/// <summary>
		/// Number required by PGA’s beginning with 001 within a CBP line and sequentially incremented on subsequent PG01 records, if applicable, identifying the PGA product or CAS# more specifically than the HTS.
		/// </summary>
		[MessageBlockInt(3, 5, "M")]
		public ZInt PGALineItemNumber;

		/// <summary>
		/// Two Character Code that identifies the PGA. Refer to Appendix Q of this publication for valid codes.
		/// </summary>
		[MessageBlockString(2, 8, "M")]
		public ZString AgencyQualifier1;

		/// <summary>
		/// Two Character Code that identifies the PGA.
		/// </summary>
		[MessageBlockString(2, 10, "C")]
		public ZString AgencyQualifier2;

		/// <summary>
		/// Two Character Code that identifies the PGA.
		/// </summary>
		[MessageBlockString(2, 12, "C")]
		public ZString AgencyQualifier3;

		/// <summary>
		/// Two Character Code that identifies the PGA.
		/// </summary>
		[MessageBlockString(2, 14, "C")]
		public ZString AgencyQualifier4;

		/// <summary>
		/// Two Character Code that identifies the PGA.
		/// </summary>
		[MessageBlockString(2, 16, "C")]
		public ZString AgencyQualifier5;

		/// <summary>
		/// Two Character Code that identifies the PGA.
		/// </summary>
		[MessageBlockString(2, 18, "C")]
		public ZString AgencyQualifier6;

		/// <summary>
		/// Code that describes the commodity with more specificity than the HTS.
		/// </summary>
		[MessageBlockString(19, 20, "C")]
		public ZString ProductCode;

		/// <summary>
		/// The number used to identify specific chemical products.
		/// </summary>
		[MessageBlockString(12, 39, "C")]
		public ZString ChemicalAbstractsServiceCASNumber;

		/// <summary>
		/// Three character code indicating the intended use of the product. Refer to Appendix R of this publication for valid codes.
		/// </summary>
		[MessageBlockString(3, 51, "C")]
		public ZString IntendedUseCode;

		/// <summary>
		/// If “Other” (O4) code is used, then a free text description of the intended use is mandatory.
		/// </summary>
		[MessageBlockString(20, 54, "C")]
		public ZString IntendedUseDescription;
	}
}
