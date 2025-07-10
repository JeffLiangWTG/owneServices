using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing;

[TestedType(typeof(RefHarbourRateForm))]
sealed class RefHarbourRateFormBasherTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var factory = new BusinessObjectFactory();
		var refHarbourRate = factory.New<RefHarbourRate>();
		return new RefHarbourRateForm(refHarbourRate);
	}
}
