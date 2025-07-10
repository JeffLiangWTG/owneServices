
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgMatchApprovalNewOrgCreator
	{
		public OrgMatchApprovalNewOrgCreator(OrgMatchApproval unmatchedOrgMatchApproval, OrgHeader newOrganisation)
		{
			this.UnmatchedOrgMatchApproval = unmatchedOrgMatchApproval;
			this.NewOrganisation = newOrganisation;
		}

		public readonly OrgMatchApproval UnmatchedOrgMatchApproval;
		public readonly OrgHeader NewOrganisation;

		public void SetOrgDetailsAndApproveMatchOnSave()
		{
			if (UnmatchedOrgMatchApproval != null)
			{
				UnmatchedOrgMatchApproval.CopyDetailsToOrganisation(NewOrganisation);
			}
			NewOrganisation.Factory.Saving += new BusinessObjectFactory.SavingEventHandler(OnFactory_Saving);
		}

		void OnFactory_Saving(BusinessObjectFactory factory)
		{
			CommitNewOrganisationInLocalFactory();
		}

		void CommitNewOrganisationInLocalFactory()
		{
			OrgMatchApproval orgMatchApprovalInThisFactory = (OrgMatchApproval)NewOrganisation.Factory.Load(typeof(OrgMatchApproval), UnmatchedOrgMatchApproval.PK);

			try
			{
				orgMatchApprovalInThisFactory.ApproveMatchBySupervisor(NewOrganisation);
			}
			catch (OrgMatchApproval.MatchingException ex)
			{
				throw new ZCannotSaveException(ex.Message, "Cannot save");
			}
		}
	}
}
