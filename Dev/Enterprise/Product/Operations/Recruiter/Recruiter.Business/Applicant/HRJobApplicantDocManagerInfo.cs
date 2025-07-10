using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicantDocManagerInfo : DocManagerInfo
	{
		public HRJobApplicantDocManagerInfo(HRJobApplicant parent, ZString docManagerCode)
			: base(parent, docManagerCode)
		{
			UseBusinessEntityFactoryAsInternal = true;
		}

		protected override BusinessObject[] GetRelatedObjects()
			=> ((HRJobApplicant)BusinessEntity).Applications.ToArray();
	}
}
