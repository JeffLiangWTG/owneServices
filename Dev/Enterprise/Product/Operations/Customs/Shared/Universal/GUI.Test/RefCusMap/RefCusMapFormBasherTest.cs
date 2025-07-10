using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(RefCusMapForm))]
	class RefCusMapFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var code = factory.New<ZZRefCusMapCombined>();
			return new RefCusMapForm(code);
		}
	}
}
