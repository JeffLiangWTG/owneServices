using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class WarehouseCustomsFallbackDetail
	{
		public bool IsExWarehouse;
		public OrganizationAddress SupplierAddress;
		public IEnumerable<string> InvoiceLineAddInfosApplicableForInwardWarehousing;
		public IDictionary<ZString, UniversalAddInfo> FallbackAddInfos;
		public WarehouseCustomsFallbackDetail Clone()
		{
			return CloneCore();
		}

		protected virtual WarehouseCustomsFallbackDetail CloneCore()
		{
			return new WarehouseCustomsFallbackDetail()
			{
				IsExWarehouse = this.IsExWarehouse,
				SupplierAddress = this.SupplierAddress,
				InvoiceLineAddInfosApplicableForInwardWarehousing = this.InvoiceLineAddInfosApplicableForInwardWarehousing,
				FallbackAddInfos = this.FallbackAddInfos
			};
		}
	}
}
