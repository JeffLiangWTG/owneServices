namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("31")]
	[OutputBlock("31")]
	public abstract partial class AENS31 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS31()
			: base("31")
		{
		}

		/// <summary>
		/// An indication of the type of bond coverage required for the payment of duties, fees, and taxes, when required.
		/// 
		/// 8 = A continuous (multiple transaction) bond. 
		/// 9 = A single transaction bond (STB).
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString BondTypeCode;

		/// <summary>
		/// An indication as to the general purpose of the bond. 
		/// 
		/// B = The basic bond; the bond that secures the Entry and Entry Summary requirements. 
		/// A = An additional bond; the bond that secures an AD/CVD, PGA or any other aspect not covered by the basic bond requirement. 
		/// U = Substitution single transaction bond; this bond is a substitute for the bond presented at time of entry.
		/// E = Superseding single transaction bond; this bond supersedes the bond presented at time of entry.
		/// </summary>
		[MessageBlockString(1, 4, "M")]
		public ZString BondDesignationTypeCode;

		/// <summary>
		/// An indication that the continuous bond cited here supersedes or is a substitute for the bond presented at time of entry. 
		/// 
		/// Y = Continuous bond supersedes the bond presented at time of entry. 
		/// S = Substitution continuous bond replaces the bond presented at the time of entry.
		/// 
		/// Space fill if continuous yet not superseding, substitution, or if STB.
		/// </summary>
		[MessageBlockString(1, 5, "C")]
		public ZString ContinuousBondIndicator;

		/// <summary>
		/// Identification of the Surety company that has underwritten the bond.
		/// </summary>
		[MessageBlockString(3, 6, "M")]
		public ZString SuretyCompanyCode;

		/// <summary>
		/// STB coverage amount in whole U.S. dollars.
		/// 
		/// Space fill if continuous bond.
		/// </summary>
		[MessageBlockDecimal(10, 9, "C", 0)]
		public ZDecimal SingleTransactionBondAmount;

		/// <summary>
		/// The Surety Reference Number from CBP form 301 as assigned by the Surety company of the STB. 
		/// 
		/// Left justified; trailing spaces. Space fill if continuous bond.
		/// </summary>
		[MessageBlockString(10, 19, "O")]
		public ZString SingleTransactionBondProducerAccountNumber;
	}
}
