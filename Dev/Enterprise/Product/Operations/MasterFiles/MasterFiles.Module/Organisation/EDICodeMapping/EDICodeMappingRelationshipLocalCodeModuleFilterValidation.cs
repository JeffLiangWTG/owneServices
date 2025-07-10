using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class EDICodeMappingRelationshipLocalCodeModuleFilterValidation : ModuleTextFilterValidation
	{
		public EDICodeMappingRelationshipLocalCodeModuleFilterValidation(EDICodeMappingRelationshipLocalCodeModuleFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRelationship();
			ValidateOrgCoGuid();
			ValidateOrgCoName();
		}

		public void ValidateRelationship()
		{
			ValidateCalculatedProperty(parent.RelationshipInfo);
		}

		protected void CheckRelationship()
		{
			ListValidation.ErrorIfInvalidCode(parent.RelationshipInfo);
		}

		public void ValidateOrgCoName()
		{
			ValidateCalculatedProperty(parent.OrgCoNameInfo);
		}

		protected void CheckOrgCoName()
		{
			ListValidation.WarnIfInvalidCode(parent.OrgCoNameInfo);
		}

		public void ValidateOrgCoGuid()
		{
			ValidateCalculatedProperty(parent.OrgCoGuidInfo);
		}

		protected void CheckOrgCoGuid()
		{
			ListValidation.ErrorIfInvalidPK(parent.OrgCoGuidInfo);
		}

		readonly EDICodeMappingRelationshipLocalCodeModuleFilter parent;
	}
}
