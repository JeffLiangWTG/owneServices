using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(RefCusRulingForm))]
	class RefCusRulingFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var ruling = factory.New<ZZRefCusRulingCombined>();
			return new RefCusRulingForm(ruling);
		}
	}
}
