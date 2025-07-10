namespace Enterprise.MasterFiles.Business
{
	public interface IMergeAction
	{
		void Merge(OrgHeader oldOrg, OrgHeader newOrg, OrganisationMergerActionOnSave action);
	}
}
