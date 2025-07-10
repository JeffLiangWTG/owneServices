using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("ZI")]
	[OutputBlock("ZI")]
	public abstract partial class BRDZI : MessageBlock // Need to add interface for BIRD System
	{
		public BRDZI()
			: base("ZI")
		{
		}

		/// <summary>
		/// 001-999
		/// </summary>
		[MessageBlockShort(3, 3, "M")]
		public ZShort InvoiceSequence;

		/// <summary>
		/// ISO Currency Code
		/// </summary>
		[MessageBlockString(3, 6, "M")]
		public ZString CurrencyCode;

		[MessageBlockDecimal(11, 9, "M", 2)]
		public ZDecimal InvoiceValue;

		[MessageBlockDecimal(7, 20, "M", 6)]
		public ZDecimal ExchangeRates;

		[MessageBlockDecimal(11, 27, "M", 2)]
		public ZDecimal ChargeAdjustment;

		/// <summary>
		/// A = Add
		/// D = Deduction
		/// </summary>
		[MessageBlockString(1, 38, "M")]
		public ZString ChargeAdjustmentDirection;

		[MessageBlockDecimal(11, 39, "M", 2)]
		public ZDecimal MarketValueAdjustment;

		/// <summary>
		/// A = Add
		/// D = Deduction
		/// </summary>
		[MessageBlockString(1, 50, "M")]
		public ZString MarketValueDirection;
	}
}
