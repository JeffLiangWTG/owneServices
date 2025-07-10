using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationDocManagerInfo : DocManagerInfo
	{
		public HRJobApplicationDocManagerInfo(HRJobApplication parent, ZString docManagerCode)
			: base(parent, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>();
			var application = (HRJobApplication)BusinessEntity;

			if (application.Applicant != null)
			{
				result.Add(application.Applicant);
				var listOfApplications = application.Applicant.Applications.ToArray<HRJobApplication>();
				result.AddRange(listOfApplications.Where(x => application != x));
			}
			return result.ToArray();
		}
	}
}
