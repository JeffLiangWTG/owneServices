using System;

using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsPickFaceInfo : DataObjectInfo
	{
		public WhsPickFaceInfo()
			: this("", "", "", false)
		{
		}

		public WhsPickFaceInfo(string locationString, string locationString_UserFriendly, string clientCode, bool shouldRetainPalletIDs)
		{
			LocationString = locationString;
			LocationString_UserFriendly = locationString_UserFriendly;
			ClientCode = clientCode;
			ShouldRetainPalletIDs = shouldRetainPalletIDs;
		}

		public string LocationString { get; set; }
		public string LocationString_UserFriendly { get; set; }
		public string ClientCode { get; set; }
		public bool ShouldRetainPalletIDs { get; set; }
	}
}
