namespace Enterprise.Freight.Business
{
	public interface ISharedGuiQueryProvider
	{
		bool ConfirmBOLPrinting(CommonShipment shipment);
	}
}
