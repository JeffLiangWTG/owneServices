using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondHeaderDocManagerInfo : DocManagerInfo
	{
		public CusInBondHeaderDocManagerInfo(BusinessObject parent)
			: base(parent, Core.Constants.DocManagerCodes.USAMS)
		{
		}
	}
}
