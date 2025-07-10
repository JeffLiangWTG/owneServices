using System.Windows.Forms;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(CarModelModulePopup))]
class CarModelModulePopupTest : ZArchitecture.GUI.Internal.Testing.EmbeddModulePopupBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var invLine = Factory.New<JobComInvoiceLine>();
		return new CarModelModulePopup(invLine, new DummyFilterGridModule());
	}
}
