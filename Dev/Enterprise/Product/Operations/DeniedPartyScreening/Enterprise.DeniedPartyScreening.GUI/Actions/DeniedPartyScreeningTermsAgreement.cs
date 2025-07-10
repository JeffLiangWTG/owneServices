using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class DeniedPartyScreeningTermsAgreement : BaseTermsAgreement
	{
		public override string Type => "DPS";

		public override bool IsCurrentUserAllowedToAcknowledgeAgreement => true;

		public override string ErrorMessageForAcknowledgementNotAllowed => null;

		protected override bool IsLocalDisplayConditionSatisfied() => DpsSecurityRights.IsGrantedScreeningWithShowError();

		protected override (string Title, string Contents, int VersionNo) GetLocalFallbackTerm() => (Res.GetString("070C371E-E5B5-4468-80BA-F76045494561", "WiseTech Denied Party Screening Service User Terms"),
																									OrganisationsDataRegistry.Instance.DeniedPartyScreeningUserTerms.Value,
																									0);

		public static new async Task<bool> HasBeenAcknowledged(Form parentForm)
		{
			var checker = new TermsAcknowledgementChecker(new DeniedPartyScreeningTermsAgreement(), parentForm);

			return await checker.CheckTermAcknowledged();
		}
	}
}
