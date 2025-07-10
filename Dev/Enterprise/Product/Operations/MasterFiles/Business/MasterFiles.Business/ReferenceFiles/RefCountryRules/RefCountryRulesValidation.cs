using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryRulesValidation : AutoRefCountryRulesValidation
	{
		public RefCountryRulesValidation(AutoRefCountryRules parent) : base(parent)
		{
			CurrentCountryCode = Parent.CurrentCountryCode;
		}

		internal new RefCountryRules Parent => base.Parent as RefCountryRules;

		void AddErrorIfInvalid(ZPropertyInfo info)
		{
			if (!info.HasErrors())
			{
				if (Parent.R7_RN_NKOrigin.IsEmpty && Parent.R7_RN_NKDestination.IsEmpty)
				{
					info.AddError(Res.GetString("83165a19-1f70-4416-bea8-33defc0823cb", "At least one of Origin or Destination country/region should be entered"));
				}
				else if (Parent.R7_RN_NKOrigin != CurrentCountryCode && Parent.R7_RN_NKDestination != CurrentCountryCode)
				{
					info.AddError(Res.GetString("c05d3d14-aad4-44ae-b41c-ab145261435c", "At least one of Origin or Destination country/region should be '{0}'.", CurrentCountryCode));
				}
			}
		}

		public ZString CurrentCountryCode { get; set; }

		protected override void CheckR7_RN_NKDestination()
		{
			base.CheckR7_RN_NKDestination();

			ListValidation.ErrorIfInvalidCode(Parent.R7_RN_NKDestinationInfo);
			AddErrorIfInvalid(Parent.R7_RN_NKDestinationInfo);
			ValidateDestinationForUltimateConsigneeRule();
		}

		void ValidateDestinationForUltimateConsigneeRule()
		{
			if (Parent.R7_RN_NKDestination.IsEmpty
				&& (Parent.R7_UltimateConsigneeRule == Core.Constants.UltimateConsigneeRuleTypes.Codes.Mandatory || Parent.R7_UltimateConsigneeRule == Core.Constants.UltimateConsigneeRuleTypes.Codes.VerifyWithCarrierOrLocalAuthorities))
			{
				Parent.R7_RN_NKDestinationInfo.AddError(Res.GetString("f79a61a3-edce-43bd-9192-93f6850e83f9", "Destination is mandatory if Ultimate Consignee Required is set to '{0}' or '{1}'.",
					Core.Constants.UltimateConsigneeRuleTypes.Codes.Mandatory,
					Core.Constants.UltimateConsigneeRuleTypes.Codes.VerifyWithCarrierOrLocalAuthorities));
			}
		}

		protected override void CheckR7_RN_NKOrigin()
		{
			base.CheckR7_RN_NKOrigin();
			ListValidation.ErrorIfInvalidCode(Parent.R7_RN_NKOriginInfo);
			AddErrorIfInvalid(Parent.R7_RN_NKOriginInfo);
		}

		protected override void CheckR7_TransportMode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.R7_TransportModeInfo);
		}

		protected override void CheckR7_UltimateConsigneeRule()
		{
			base.CheckR7_UltimateConsigneeRule();
			ListValidation.ErrorIfInvalidCode(Parent.R7_UltimateConsigneeRuleInfo);
		}
	}
}
