using System.Threading.Tasks;
using System.Windows.Forms;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Actions
{
	public interface ICertificateOfOriginIndemnityTermsAgreementChecker
	{
		Task<TermAcknowledged> CheckTermAcknowledged(Form parentForm);
	}
}
