using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationDocumentCollection : ActiveBusinessObjectCollection<HRJobApplicationDocument>
	{
		public HRJobApplicationDocumentCollection(HRJobApplication application)
			: base(application)
		{
		}
	}
}
