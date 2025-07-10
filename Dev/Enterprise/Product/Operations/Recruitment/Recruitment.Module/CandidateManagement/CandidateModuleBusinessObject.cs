using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Module.CandidateManagement;

namespace Enterprise.Recruitment.Module
{
	public class CandidateModuleBusinessObject : NonPersistentBusinessObject
	{
		public CandidateModuleBusinessObject(CandidateBusinessObjectCollection collection) : base(collection.Factory)
		{
			Collection = collection;

			var factory = collection.Factory;
			DummyForBinding = new Candidate(factory.GetNull<HRJobApplication>()) { IsNull = true };
		}

		public CandidateBusinessObjectCollection Collection { get; }
		public Candidate DummyForBinding { get; }
	}
}
