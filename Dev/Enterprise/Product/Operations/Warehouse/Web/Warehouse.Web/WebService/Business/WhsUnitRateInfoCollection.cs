using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsUnitRateInfoCollection : DataObjectInfoCollection<WhsUnitRateInfo>
	{
		#region Constructors

		public WhsUnitRateInfoCollection(OrgPartUnitCollection units)
		{
			foreach (OrgPartUnit unit in units)
			{
				var unitInfo = new WhsUnitRateInfo(unit);
				this.Add(unitInfo);
			}
		}

		public WhsUnitRateInfoCollection()
		{
		}

		#endregion
	}
}
