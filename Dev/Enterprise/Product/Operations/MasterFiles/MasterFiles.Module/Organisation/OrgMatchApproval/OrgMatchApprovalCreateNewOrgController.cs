using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public class OrgMatchApprovalCreateNewOrgController : OrganisationController, IOrgMatchApprovalCreateNewOrgController
	{
		public OrgMatchApproval UnmatchedOrgMatchApproval
		{
			get { return fUnmatchedOrgMatchApproval; }
			set { fUnmatchedOrgMatchApproval = value; }
		}
		OrgMatchApproval fUnmatchedOrgMatchApproval;

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			OrgHeader result = (OrgHeader)base.GetNewBusinessEntityInLocalFactory();
			new OrgMatchApprovalNewOrgCreator(UnmatchedOrgMatchApproval, result).SetOrgDetailsAndApproveMatchOnSave();
			return result;
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.Edit;
		}
	}
}
