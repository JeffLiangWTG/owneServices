using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public abstract class OrgCodeMappingBaseModuleFilterValidation : ModuleTextFilterValidation
	{
		protected OrgCodeMappingBaseModuleFilterValidation(OrgCodeMappingBaseModuleFilter parent)
		: base(parent)
		{
			this.parent = parent;
		}

		protected readonly OrgCodeMappingBaseModuleFilter parent;

		public void ValidateRelationshipType()
		{
			ValidateCalculatedProperty(parent.RelationshipTypeInfo);
		}

		protected void CheckRelationshipType()
		{
			ListValidation.ErrorIfInvalidCode(parent.RelationshipTypeInfo);
		}

		public void ValidateContext()
		{
			ValidateCalculatedProperty(parent.ContextInfo);
		}

		protected void CheckContext()
		{
			ListValidation.ErrorIfInvalidCode(parent.ContextInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRelationshipType();
			ValidateContext();
		}

		public override Type AutoValidationType => GetType();
	}

	public abstract class OrgCodeMappingBaseModuleFilterValidation<T> : OrgCodeMappingBaseModuleFilterValidation where T : OrgCodeMappingBaseModuleFilter
	{
		protected OrgCodeMappingBaseModuleFilterValidation(T parent)
			: base(parent)
		{
			this.parent = parent;
		}

		protected readonly new T parent;
	}
}
