using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Actions
{
	public class CertificateOfOriginIndemnityTermsAgreementChecker : ICertificateOfOriginIndemnityTermsAgreementChecker
	{
		public async Task<TermAcknowledged> CheckTermAcknowledged(Form parentForm)
		{
			var agreement = new CertificateOfOriginIndemnityTermsAgreement();
			var checker = new TermsAcknowledgementChecker(agreement, parentForm);

			await checker.CheckTermAcknowledged();

			return new TermAcknowledged(agreement.HasBeenAcknowledged, agreement.VersionNo);
		}
	}
}
