using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDocumentDeliveryCreditControlManager
	{
		ZString GetDocumentDeliveryStatusForCreditManagement(BusinessObject businessObject, string documentDescription, ZGuid menuPK, bool creditCheckEnabled = true, string documentDirection = "");

		bool ShouldStopDelivery(BusinessObject businessObject);
	}
}
