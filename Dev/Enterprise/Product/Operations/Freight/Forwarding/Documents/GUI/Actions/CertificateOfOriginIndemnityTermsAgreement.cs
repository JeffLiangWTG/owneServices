using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Actions
{
	public class CertificateOfOriginIndemnityTermsAgreement : BaseTermsAgreement
	{
		public override string Type => "COI";
		public override bool IsCurrentUserAllowedToAcknowledgeAgreement => true;
		public override string ErrorMessageForAcknowledgementNotAllowed => null;

		protected override (string Title, string Contents, int VersionNo) GetLocalFallbackTerm()
			=> (Res.GetString("6f1462fd-97b0-472c-9869-1eda6cb4b50d", "WiseTech Certificate of Origin Indemnity User Terms"), DocumentsDataRegistry.Instance.CertOfOriginIndemnity.Value, 0);

		protected override bool IsLocalDisplayConditionSatisfied() => true;
	}
}
