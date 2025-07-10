using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HROnBoardingProcessTaskCollection : ProcessTaskCollection
	{
		public HROnBoardingProcessTaskCollection(HROnBoarding parent)
		: base(parent)
		{
		}

		public new HROnBoardingProcessTask this[int index]
		{
			get { return (HROnBoardingProcessTask)Elements[index]; }
		}

		public new HROnBoardingProcessTask AddNew()
		{
			return (HROnBoardingProcessTask)base.AddNew();
		}

		public new HROnBoarding Parent
		{
			get { return (HROnBoarding)base.Parent; }
		}
	}
}
