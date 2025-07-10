using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(SealNumberForm))]
	sealed class SealNumbersFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore() => new SealNumberForm(new SealNumberBusinessObjectCollection(ZString.Empty));
	}
}
