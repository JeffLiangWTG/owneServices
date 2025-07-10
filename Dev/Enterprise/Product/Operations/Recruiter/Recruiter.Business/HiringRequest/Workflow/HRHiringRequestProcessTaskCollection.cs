using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRHiringRequestProcessTaskCollection : ProcessTaskCollection
	{
		public HRHiringRequestProcessTaskCollection(HRHiringRequest parent)
			: base(parent)
		{
		}

		public new HRHiringRequestProcessTask this[int index]
		{
			get { return (HRHiringRequestProcessTask)Elements[index]; }
		}

		public new HRHiringRequestProcessTask AddNew()
		{
			return (HRHiringRequestProcessTask)base.AddNew();
		}

		public new HRHiringRequest Parent
		{
			get { return (HRHiringRequest)base.Parent; }
		}
	}
}
