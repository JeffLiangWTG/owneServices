using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.Business;

public class PopulateMasterBillNumberForNeutralMasterLoadListService(
	ForwardingConsol consol,
	HVLVOriginLoadList loadList) : IAfterOnSavingBOProcessingService
{
	public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
	{
		loadList.HVL_MasterBillNumber = consol.JK_MasterBillNum;
	}

	public static void AddPopulateMasterBillNumberForNeutralMasterLoadListServiceIfRequired(ForwardingConsol consol, HVLVOriginLoadList loadList)
	{
		if (loadList.HVL_IsNeutralMaster)
		{
			var factory = loadList.Factory;
			var service = factory.ServiceContainer.GetAfterOnSavingService<PopulateMasterBillNumberForNeutralMasterLoadListService>();
			if (service == null)
			{
				service = new PopulateMasterBillNumberForNeutralMasterLoadListService(consol, loadList);
				factory.ServiceContainer.AddAfterOnSavingService(service);
			}

			factory.Saved += RemovePopulateMasterBillNumberForNeutralMasterLoadListService;
		}
	}

	static void RemovePopulateMasterBillNumberForNeutralMasterLoadListService(BusinessObjectFactory factory, bool savedSuccessfully)
	{
		var service = factory.ServiceContainer.GetAfterOnSavingService<PopulateMasterBillNumberForNeutralMasterLoadListService>();
		if (service != null)
		{
			factory.ServiceContainer.RemoveAfterOnSavingService<PopulateMasterBillNumberForNeutralMasterLoadListService>();
		}
	}
}
