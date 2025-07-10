using System.Windows.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	[TestedType(typeof(SignatureForm))]
	public class SignatureFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var leg = Factory.New<CommonCartageLeg>();
			return new SignatureForm(leg);
		}
	}
}
