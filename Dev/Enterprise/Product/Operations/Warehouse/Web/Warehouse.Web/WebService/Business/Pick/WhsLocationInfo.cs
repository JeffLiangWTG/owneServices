using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService
{
#if DEBUG
	[Serializable]
#endif
	public class WhsLocationInfo : DataObjectInfo
	{
		public WhsLocationInfo()
			: this(Guid.Empty, string.Empty, string.Empty, string.Empty)
		{
		}

		public WhsLocationInfo(Guid locationPK, string locationString, string locationString_UserFriendly, string locationClass)
		{
			LocationPK = locationPK;
			LocationString = locationString;
			LocationString_UserFriendly = locationString_UserFriendly;
			LocationClass = locationClass;
		}

		public Guid LocationPK { get; set; }
		public string LocationString { get; set; }
		public string LocationString_UserFriendly { get; set; }
		public string LocationClass { get; set; }
	}
}
