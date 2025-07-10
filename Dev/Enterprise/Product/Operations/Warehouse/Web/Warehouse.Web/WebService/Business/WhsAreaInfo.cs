using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsAreaInfo : DataObjectInfo
	{
		#region Constructors

		public WhsAreaInfo()
			: this("", "", Guid.Empty)
		{
		}

		public WhsAreaInfo(string name, string description, Guid areaPK)
		{
			Name = name;
			Description = description;
			AreaPK = areaPK;
		}

		#endregion

		#region Properties

		public string Name { get; set; }

		public string Description { get; set; }

		public Guid AreaPK { get; set; }

		#endregion
	}
}
