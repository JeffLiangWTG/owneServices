using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class MiscOptionsLayoutUserControlTest : TestCaseWithFactory
{
	public void TestSupportingInformationUserControl()
	{
		using (var control = new MiscOptionsLayoutUserControl())
		{
			AssertType<SupportingInformationControl>(control.SupportingInformationUserControl);
		}
	}

	public void TestControls()
	{
		using (var control = new MiscOptionsLayoutUserControl())
		{
			AssertType<ZDropEdit>(control.ExciseZDropEdit);
			AssertType<ZDropEdit>(control.VATZDropEdit);
		}
	}
}
