using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderDocManagerInfo : DocManagerInfo, ISupportReadOnlyOverride
	{
		public WhsVASOrderDocManagerInfo(WhsVASOrder vasOrder)
			: base(vasOrder, Core.Constants.DocManagerCodes.WarehouseVASOrder)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var vasOrder = (WhsVASOrder)BusinessEntity;
			return vasOrder.Client == null ? Array.Empty<BusinessObject>() : new[] { vasOrder.Client };
		}

		public override bool ReadOnly => fReadOnly;

		void ISupportReadOnlyOverride.SetReadOnly(bool readOnly)
		{
			fReadOnly = readOnly;
		}

		bool fReadOnly;
	}
}
