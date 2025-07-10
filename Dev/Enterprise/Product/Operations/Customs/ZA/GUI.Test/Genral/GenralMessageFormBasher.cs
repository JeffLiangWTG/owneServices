using System.Windows.Forms;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(GenralMessageForm))]
	class GenralMessageFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new GenralMessageForm(Factory.New<GENRALMessage>());
	}
}
