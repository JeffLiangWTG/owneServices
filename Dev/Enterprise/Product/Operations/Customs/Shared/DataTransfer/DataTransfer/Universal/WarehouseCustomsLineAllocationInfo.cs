using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class WarehouseCustomsLineAllocationInfo : IWarehouseCustomsLineAllocationInfo
	{
		public WarehouseCustomsLineAllocationInfo(List<AddInfo> addInfos)
		{
			this.addInfos = addInfos;
		}
		readonly List<AddInfo> addInfos;

		public ZString AllocationKey => addInfos.GetZStringValue(Constants.AddInfoKeys.AllocationInfo.AllocationKey) ?? ZString.Empty;

		public ZDecimal Quantity => addInfos.GetZDecimalValue(Constants.AddInfoKeys.AllocationInfo.Quantity) ?? ZDecimal.Zero;
	}
}
