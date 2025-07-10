using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ICharge
	{
		bool IsBillInLocalCurrency { get; }
		bool IsBillInInvoiceCurrency { get; }

		decimal LocalSellInvoiceAmt { get; }
		decimal LocalSellGSTAmt { get; }
		decimal SellInvoiceCurrencyExRate { get; }
		decimal OSSellInvoiceAmt { get; }
		decimal CFXAmt { get; }

		ZGuid JR_OH_CostAccount { get; set; }
		ZString JR_Calc_CostRatingBehavior { get; set; }
	}
}
