namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE12")]
	public abstract partial class ASESE12 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE12()
			: base("SE12")
		{
		}

		/// <summary>
		/// An indication of the type of bond coverage required for the payment of duties, fees, and taxes, when required.
		/// 
		/// 8 = A continuous (multiple transaction) bond. 9 = A single transaction bond (STB).
		/// </summary>
		[MessageBlockString(1, 5, "M")]
		public ZString BondTypeCode;

		/// <summary>
		/// An indication as to the general purpose of the bond. Only accepted value is A.
		/// </summary>
		[MessageBlockString(1, 6, "M")]
		public ZString BondDesignationTypeCode;

		/// <summary>
		/// Identification of the Surety company that has underwritten the bond.
		/// </summary>
		[MessageBlockString(3, 8, "M")]
		public ZString SuretyCompanyCode;

		/// <summary>
		/// STB coverage amount in whole U.S. dollars.
		/// 
		/// The bond amount is to be reported in the following format:
		/// Left-justified
		/// Numeric only
		/// No leading Zeroes
		/// Maximum of 10N (10 digits)
		/// No commas, No decimal. (whole numbers only)
		/// Amount must be greater than Zero.
		/// </summary>
		[MessageBlockDecimal(10, 11, "M", 0)]
		public ZDecimal SingleTransactionBondAmount;

		/// <summary>
		/// The Surety Reference Number from CBP form 301 as assigned by the Surety company of the STB.
		/// 
		/// Left justified; trailing spaces. Space fill if continuous bond.
		/// </summary>
		[MessageBlockString(10, 21, "O")]
		public ZString SingleTransactionBondProducerAccountNumber;
	}
}
