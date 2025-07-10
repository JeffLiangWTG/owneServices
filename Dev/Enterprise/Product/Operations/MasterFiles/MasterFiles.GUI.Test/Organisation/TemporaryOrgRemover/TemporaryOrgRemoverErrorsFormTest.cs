using System.Windows.Forms;
using Enterprise.MasterFiles.Business.TemporaryOrgRemover;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TemporaryOrgRemoverErrorsForm))]
	sealed class TemporaryOrgRemoverErrorsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TemporaryOrgRemoverErrorsForm(new Remover());
		}
	}
}
