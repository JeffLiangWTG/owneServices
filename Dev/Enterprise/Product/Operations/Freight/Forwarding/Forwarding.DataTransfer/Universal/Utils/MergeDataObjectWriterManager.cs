using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class MergeDataObjectWriterManager
	{
		public static void AddBizObjData(IDataWritingManager manager, UniversalShipment shipmentData, BusinessObject bizObjToMergeWith, bool includeParent, bool includeChildren)
		{
			if (bizObjToMergeWith != null)
			{
				var dataContextManager = bizObjToMergeWith.GetUniversalDataContextManager() as IShipmentDataContextManager;
				if (dataContextManager != null)
				{
					var shipmentWriter = dataContextManager.GetShipmentDataObjectWriter(manager) as IMergeDataObjectWriter;
					if (shipmentWriter != null)
					{
						var hierarchyWriter = shipmentWriter as IHierarchicalDataObjectWriter;
						if (hierarchyWriter != null)
						{
							hierarchyWriter.IncludeChildren = includeChildren;
							hierarchyWriter.IncludeParent = includeParent;
						}

						shipmentWriter.MergeData(shipmentData, bizObjToMergeWith);
					}
				}
			}
		}
	}
}

