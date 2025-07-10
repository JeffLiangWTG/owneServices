using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	class CartageWorkSheetDocManagerInfo : DocManagerInfo
	{
		public CartageWorkSheetDocManagerInfo(CommonWorkSheet parent, ZString docManagerCode)
			: base(parent, docManagerCode)
		{
		}
	}
}
