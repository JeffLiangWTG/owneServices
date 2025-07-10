using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI
{
	public class CreditReportTermsAndCondition : BaseTermsAgreement
	{
		public override string Type => "CCS";

		public override bool IsCurrentUserAllowedToAcknowledgeAgreement => true;

		public override string ErrorMessageForAcknowledgementNotAllowed => null;

		protected override bool IsLocalDisplayConditionSatisfied() => true;
	}
}
