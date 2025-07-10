using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusOutturnHeaderDocManagerInfo : DocManagerInfo
	{
		public CusOutturnHeaderDocManagerInfo(CusOutturnHeader parent, ZString docManagerCode)
			: base(parent, docManagerCode)
		{ }
	}
}
