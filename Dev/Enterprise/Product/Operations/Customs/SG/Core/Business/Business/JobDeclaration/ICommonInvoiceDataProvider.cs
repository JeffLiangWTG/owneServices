namespace Enterprise.Customs.SG.V4.Business
{
	public interface ICommonInvoiceDataProvider : Customs.Business.ICommonInvoiceDataProvider
	{
		bool IsTradeNet4Point1 { get; }
	}
}
