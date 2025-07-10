namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("53")]
	[OutputBlock("53")]
	public abstract partial class AENS53 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS53()
			: base("53")
		{
		}

		/// <summary>
		/// The identifying number of the case. Do not include hyphens ('-').
		/// </summary>
		[MessageBlockString(10, 3, "M")]
		public ZString CaseNumber;

		/// <summary>
		/// An indication as to whether the duty amount is to be collected as a cash deposit or considered as under a bond as prescribed by the case. 
		/// 
		/// B = The duty amount is to be covered under a surety bond. 
		/// C = The duty amount is to be collected as a cash deposit.
		/// </summary>
		[MessageBlockString(1, 13, "M")]
		public ZString BondCashClaimCode;

		/// <summary>
		/// The applicable deposit rate, prescribed by the case, which corresponds to the Case Rate Type Qualifier Code. 
		/// 
		/// Two decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(8, 14, "M", 2)]
		public ZDecimal CaseDepositRate;

		/// <summary>
		/// The 'type' of rate prescribed by the case, which corresponds to the applicable deposit rate. 
		/// 
		/// A = The case rate is an ad valorem rate. 
		/// S = The case rate is a specific rate.
		/// </summary>
		[MessageBlockString(1, 22, "M")]
		public ZString CaseRateTypeQualifierCode;

		/// <summary>
		/// Value of the article, to be used exclusively and in lieu of any other article value for the specific purpose of calculating any estimated antidumping or countervailing duty (when an ad valorem rate applies), reported in whole U.S. dollars. 
		/// 
		/// Zero fill if there is no deposit value/does not apply.
		/// </summary>
		[MessageBlockDecimal(10, 25, "C", 0, FillType.AlwaysZeroFill)]
		public ZDecimal ADCVDValueOfGoodsAmount;

		/// <summary>
		/// Total number of primary units (which corresponds to the UOM Code prescribed by the case) to be used for calculating the estimated antidumping or countervailing duty (when a specific rate applies). 
		/// 
		/// Four decimal places are implied. Zero fill if quantity not required/does not apply.
		/// </summary>
		[MessageBlockDecimal(12, 35, "C", 4)]
		public ZDecimal ADCVDQuantity;

		/// <summary>
		/// The duty amount estimated relevant to the case in U.S. dollars and cents. 
		/// 
		/// Two decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(10, 47, "M", 2)]
		public ZDecimal ADCVDDutyAmount;

		/// <summary>
		/// Identifier of the blanket AD/CVD Non-Reimbursement Declaration that includes this Case Number. 
		/// 
		/// Space fill if no declaration is made.
		/// </summary>
		[MessageBlockString(10, 57, "C")]
		public ZString ADDNonReimbursementDeclarationIdentifier; //This only applies to ADD now.
	}
}
