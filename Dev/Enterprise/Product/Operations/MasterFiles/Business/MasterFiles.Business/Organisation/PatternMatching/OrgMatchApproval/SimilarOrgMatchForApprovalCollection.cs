using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class SimilarOrgMatchForApprovalCollection : NonPersistentBusinessObjectCollection<SimilarOrgMatchForApproval>
	{
		public SimilarOrgMatchForApprovalCollection(OrgMatchApproval matchApproval, OrgPatternMatchCollection similarOrgMatches) : base(matchApproval.Factory)
		{
			this.MatchApproval = matchApproval;
			foreach (OrgPatternMatch orgPatternMatch in similarOrgMatches)
			{
				SimilarOrgMatchForApproval similarOrgMatchToAdd = SimilarOrgMatchForApproval.New(matchApproval, orgPatternMatch);
				Add(similarOrgMatchToAdd);
			}
			SetReadOnlyIncludingChildren(true);
		}

		public readonly OrgMatchApproval MatchApproval;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException();
		}

		#region Creating a new collection element for AddNew/Cancel on binding

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return SimilarOrgMatchForApproval.New(MatchApproval, DummyOrgPatternMatchForAddNewCancelBinding);
		}

		OrgPatternMatch DummyOrgPatternMatchForAddNewCancelBinding
		{
			get
			{
				if (fDummyOrgPatternMatchForAddNewCancelBinding == null)
				{
					BusinessObjectFactory dummyFactory = new BusinessObjectFactory();
					fDummyOrgPatternMatchForAddNewCancelBinding = (OrgPatternMatch)dummyFactory.New(typeof(OrgPatternMatch));
					fDummyOrgPatternMatchForAddNewCancelBinding.OS_OH = dummyFactory.New(typeof(OrgHeader)).PK;
				}
				return fDummyOrgPatternMatchForAddNewCancelBinding;
			}
		}
		OrgPatternMatch fDummyOrgPatternMatchForAddNewCancelBinding;

		#endregion
	}
}
