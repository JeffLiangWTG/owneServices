using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public interface IJobApplicationDependantCollection : IActiveBusinessObjectCollection
	{
		new HRJobApplication AddNew();
		new HRJobApplication this[int i] { get; }
		void SetOverrideAllowNew(bool value);
	}

	public class HRJobApplicationDependentCollection : ActiveBusinessObjectCollection<HRJobApplication>, IJobApplicationDependantCollection
	{
		public HRJobApplicationDependentCollection(HRJobApplicant parent) : base(parent)
		{
		}

		public HRJobApplicationDependentCollection(HRRecruitmentJobCampaign parent) : base(parent)
		{
		}

		public void SetOverrideAllowNew(bool value)
			=> allowNew = value;

		protected override bool AllowNew => allowNew;

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		bool allowNew = false;

		protected override void OnAdded(HRJobApplication application)
		{
			base.OnAdded(application);
		}
	}
}
