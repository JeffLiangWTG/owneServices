using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdHocServiceJobDocManagerInfo : DocManagerInfo
	{
		public WhsAdHocServiceJobDocManagerInfo(WhsAdHocServiceJob adHocServiceJob)
			: base(adHocServiceJob, Core.Constants.DocManagerCodes.WarehouseAdHocServiceJob)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>();
			var serviceJob = (WhsAdHocServiceJob)BusinessEntity;

			if (serviceJob.Client != null)
			{
				result.Add(serviceJob.Client);
			}

			return result.ToArray();
		}
	}
}
