using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class AssignedJobComInvLineRefsCollection : JobComInvLineRefsCollection<AssignedJobComInvLineRefs>
	{
		public AssignedJobComInvLineRefsCollection(BusinessObject parent) : base(parent, JobComInvLineRefsType.Codes.AssignedNumber)
		{
		}
	}
}
