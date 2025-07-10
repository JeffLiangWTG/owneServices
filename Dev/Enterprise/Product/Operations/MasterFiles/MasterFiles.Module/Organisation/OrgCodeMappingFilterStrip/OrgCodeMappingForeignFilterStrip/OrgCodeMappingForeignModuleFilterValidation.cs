using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCodeMappingForeignModuleFilterValidation : OrgCodeMappingBaseModuleFilterValidation<OrgCodeMappingForeignModuleFilter>
	{
		public OrgCodeMappingForeignModuleFilterValidation(OrgCodeMappingForeignModuleFilter parent)
			: base(parent)
		{
		}

		public void ValidateForeignCode()
		{
			ValidateCalculatedProperty(parent.ForeignCodeInfo);
		}

		protected void CheckForeignCode()
		{
			if (parent.ForeignCode.Length > OrgPatternMatchOverride.Schema.OO_ForeignCodeMaxLength)
			{
				var message = ResString.GetMultilingualString("0c067c41-8ab0-4bc7-aed7-8466f9571ee0", "Filter exceeds the maximum length for a Foreign Code ({0}).", OrgPatternMatchOverride.Schema.OO_ForeignCodeMaxLength);

				parent.ForeignCodeInfo.AddError(message);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateForeignCode();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}
	}
}
