using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationProcessTaskCollection : ProcessTaskCollection
	{
		public HRJobApplicationProcessTaskCollection(HRJobApplication application)
			: base(application)
		{
		}
		public HRJobApplicationProcessTaskCollection(HRJobApplication application, ZQuery additionalFilter)
			: base(application, additionalFilter)
		{
		}

		public new HRJobApplicationProcessTask this[int index]
		{
			get { return (HRJobApplicationProcessTask)Elements[index]; }
		}

		public new HRJobApplicationProcessTask AddNew()
		{
			return (HRJobApplicationProcessTask)base.AddNew();
		}
	}
}
