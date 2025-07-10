using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class ChassisJobComInvLineRefsCollection : JobComInvLineRefsCollection<ChassisJobComInvLineRefs>
	{
		public ChassisJobComInvLineRefsCollection(BusinessObject parent) : base(parent, JobComInvLineRefsType.Codes.Chassis)
		{
		}
	}
}
