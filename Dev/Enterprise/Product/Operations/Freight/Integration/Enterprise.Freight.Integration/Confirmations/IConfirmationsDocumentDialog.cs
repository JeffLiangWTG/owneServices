using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public interface IConfirmationsDocumentDialog
	{
		bool ShowDialogDisposeAndContinue(BusinessObjectCollection confirmsCollection, BusinessObject commonShipment);
	}
}
