using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business
{
	public class HRJobAdPlacementLookups : AutoHRJobAdPlacementLookups
	{
		public HRJobAdPlacementLookups(AutoHRJobAdPlacement parent) : base(parent)
		{
		}

		#region  AdPlacementPublicationsList

		public ReadOnlyCodeDescriptionPairList AdPlacementPublicationsList
		{
			get { return RecruiterDataRegistry.Instance.AdPlacementPublicationsList.Value; }
		}

		#endregion
	}
}
