//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCPSCRuleAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSCPSCRuleAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USCPSCRuleAddInfoValidation : AutoUSCPSCRuleAddInfoValidation
	{
		public USCPSCRuleAddInfoValidation(AutoUSCPSCRuleAddInfo parent) : base(parent)
		{
		}

		CPSCRule Rule
		{
			get { return (CPSCRule)Parent.Parent; }
		}

		protected override void CheckUS_CPSCAccreditedLabID()
		{
			base.CheckUS_CPSCAccreditedLabID();
			CheckLabIDAndTestLocation(Parent.US_CPSCAccreditedLabIDInfo);
			ValidateUS_OA_SafetyTestLocationAddress();
			ValidateUS_RuleCodes();
		}

		protected override void CheckUS_OA_SafetyTestLocationAddress()
		{
			base.CheckUS_OA_SafetyTestLocationAddress();
			CheckLabIDAndTestLocation(Parent.US_OA_SafetyTestLocationAddressInfo);

			var rule = Rule;
			if (rule != null && rule.SafetyTestLocationAddress != null && !Parent.US_OA_SafetyTestLocationAddressInfo.Notifications.HasMessageErrors())
			{
				OrganisationValidation.ValidatePGAContact(Parent.US_OA_SafetyTestLocationAddressInfo, OrgHeaderWrapper.New(rule.SafetyTestLocationAddress));
				OrganisationValidation.ValidateCharactorsForAddressDescription(rule.US_OA_SafetyTestLocationAddressInfo, rule.SafetyTestLocationAddress);
			}
			ValidateUS_CPSCAccreditedLabID();
			ValidateUS_RuleCodes();
		}

		void CheckLabIDAndTestLocation(ZPropertyInfo info)
		{
			var rule = Rule;
			var header = rule != null ? rule.Header : null;
			if (header != null && IsPGAValidation)
			{
				if (Parent.US_CPSCAccreditedLabID.IsEmpty && Parent.US_OA_SafetyTestLocationAddress.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
				}
			}
		}

		protected override void CheckUS_PreviousInspectionDate()
		{
			base.CheckUS_PreviousInspectionDate();
			if (Rule.Header != null && IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PreviousInspectionDateInfo);
			}
		}

		bool IsPGAValidation
		{
			get
			{
				var rule = Rule;
				var header = rule != null ? rule.Header : null;
				var invoiceLine = header == null ? null : header.InvoiceLine;
				var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}
	}
}
