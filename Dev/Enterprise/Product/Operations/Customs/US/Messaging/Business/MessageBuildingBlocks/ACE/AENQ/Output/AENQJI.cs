namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[OutputBlock("JI")]
	public partial class AENQJI : MessageBlock
	{
		public AENQJI()
			: base("JI")
		{
		}

		/// <summary>
		/// A code representing the surety.
		/// </summary>
		[MessageBlockString(3, 3, "C")]
		public ZString SuretyCode;

		/// <summary>
		/// A code indicating whether a surety is designated as the primary surety.Valid codes are:
		/// 
		/// Y = Primary Surety
		/// N = Non-Primary Surety
		/// </summary>
		[MessageBlockString(1, 6, "C")]
		public ZString PrimarySuretyIndicator;

		/// <summary>
		/// Type of Bond:
		/// 
		/// 0 = No Bond Required
		/// 8 = Continuous Bond
		/// 9 = Single Entry Bond
		/// </summary>
		[MessageBlockString(1, 7, "C")]
		public ZString BondTypeCode;

		/// <summary>
		/// A code representing the general purpose of the bond and the action taken.Valid codes are:
		/// 
		/// N = CRM New Bond Added
		/// A = Additional Bond
		/// V = Bond Voided
		/// R = Bond Rider
		/// B = New Bond
		/// U = Substitution Bond
		/// T = Bond Terminated
		/// C = Bond Amount Adjusted
		/// E = Superseding Bond
		/// </summary>
		[MessageBlockString(1, 8, "C")]
		public ZString BondDesignationTypeCode;

		/// <summary>
		/// A code indicating if the entry has mor than one bond associated to it at the time
		/// of the entry summary.For example, and additional bond or a superseding bond.Valid codes are:
		/// 
		/// N = This is the only bond obligated.
		/// Y = There is at least one other bond obligated.
		/// Null = Not applicable. For a Recon Entry Type (09), there is no bond requirement, and this field will be null.
		/// </summary>
		[MessageBlockString(1, 9, "C")]
		public ZString MultipleBondsIndicator;

		/// <summary>
		/// A code representing the bond number.
		/// </summary>
		[MessageBlockString(9, 10, "C")]
		public ZString BondNumber;

		/// <summary>
		/// Amount associated with a Single Entry Bond. Two decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(15, 19, "C", 2)]
		public ZDecimal SingleEntryBondAmount;

		/// <summary>
		/// The total value of the Surety's liability in whole US dollars. This data element is right justified, may contain leading zeros, and does not include decimals.
		/// </summary>
		[MessageBlockDecimal(10, 34, "C", 0)]
		public ZDecimal SuretyLiabilityAmount;
	}

	[OutputBlock("JI", "01")]
	public partial class AENQJI_01 : MessageBlock
	{
		public AENQJI_01()
			: base("JI")
		{
		}

		/// <summary>
		/// A code representing the surety.
		/// </summary>
		[MessageBlockString(3, 4, "C")]
		public ZString SuretyCode;

		[MessageBlockString(28, 8, "C")]
		public ZString NarrativeText;
	}
}
