using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsPickFaceInfoCollection : DataObjectInfoCollection<WhsPickFaceInfo>
	{
		public WhsPickFaceInfoCollection()
		{
		}

		public WhsPickFaceInfoCollection(WhsProduct product)
		{
			foreach (var pickFace in product.PickFaces)
			{
				var location = pickFace.Location;
				Add(new WhsPickFaceInfo(location?.WLV_LocationString ?? "", location?.WLV_LocationString_UserFriendly ?? "", pickFace.Client.OH_Code, location?.LocationType.WLT_RetainPalletIDsInFixedPickFaces ?? false));
			}
		}
	}
}
