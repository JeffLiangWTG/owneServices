using System.Windows.Forms;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(ProofOfPaymentBulkUpdateForm))]
	sealed class ProofOfPaymentBulkUpdateFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ProofOfPaymentBulkUpdateForm(new CusEntryPayInfoBulkUpdateBusinessObject(Factory));
		}
	}
}
