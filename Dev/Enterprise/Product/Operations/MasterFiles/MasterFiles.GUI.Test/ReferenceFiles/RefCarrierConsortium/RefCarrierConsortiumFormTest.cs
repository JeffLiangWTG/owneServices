using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefCarrierConsortiumForm))]
	sealed class RefCarrierConsortiumFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefCarrierConsortiumForm(Factory.New<RefCarrierConsortium>());
		}
	}
}
