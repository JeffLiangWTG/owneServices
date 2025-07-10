using System;

namespace Enterprise.Warehouse.Web.WebService.Common
{
#if DEBUG
	[Serializable]
#endif
	public abstract class DataObjectInfo
	{
		#region Constructors

		protected DataObjectInfo()
		{
		}

		#endregion
	}
}
