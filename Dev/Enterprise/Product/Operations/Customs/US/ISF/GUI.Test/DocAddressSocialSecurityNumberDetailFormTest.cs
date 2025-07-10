using System.Windows.Forms;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.GUI.Testing
{
	[TestedType(typeof(DocAddressSocialSecurityNumberDetailForm))]
	sealed class DocAddressSocialSecurityNumberDetailFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var docAddress = Factory.New<ISFDocAddress>();
			return new DocAddressSocialSecurityNumberDetailForm(docAddress);
		}
	}
}
