using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCodeMappingLocalModuleFilterValidation : OrgCodeMappingBaseModuleFilterValidation<OrgCodeMappingLocalModuleFilter>
	{
		public OrgCodeMappingLocalModuleFilterValidation(OrgCodeMappingLocalModuleFilter parent)
			: base(parent)
		{
		}

		public void ValidateLocalCode()
		{
			ValidateCalculatedProperty(parent.LocalCodeInfo);
		}

		protected void CheckLocalCode()
		{
			TypeValidation.CheckValidGuid(parent.LocalCodeInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLocalCode();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}
	}
}
