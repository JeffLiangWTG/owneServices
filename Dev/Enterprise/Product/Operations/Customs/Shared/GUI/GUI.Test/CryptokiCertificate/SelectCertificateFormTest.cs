using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Certificates.Testing
{
	[TestedType(typeof(SelectCertificateForm))]
	sealed class SelectCertificateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new SelectCertificateForm(new List<CryptokiCertificate>());
	}
}
