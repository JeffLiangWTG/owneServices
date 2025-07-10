using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationAttemptProcessTaskCollection : ProcessTaskCollection
	{
		public GlbAccreditationAttemptProcessTaskCollection(GlbAccreditationAttempt attempt)
			: base(attempt)
		{
		}

		public new GlbAccreditationAttemptProcessTask this[int index]
		{
			get { return (GlbAccreditationAttemptProcessTask)Elements[index]; }
		}

		public new GlbAccreditationAttemptProcessTask AddNew()
		{
			return (GlbAccreditationAttemptProcessTask)base.AddNew();
		}

		public new GlbAccreditationAttempt Parent
		{
			get { return (GlbAccreditationAttempt)base.Parent; }
		}
	}
}
