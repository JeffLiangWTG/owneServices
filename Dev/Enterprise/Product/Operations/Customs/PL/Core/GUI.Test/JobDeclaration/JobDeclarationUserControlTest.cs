using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(JobDeclarationUserControl))]
sealed class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
{
	public void TestDepartureTransportIDTextBoxVisibility_Default()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var userControl = new JobDeclarationUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			AssertEquals("userControl.DepartureTransportIDTextBox.Visible", false, userControl.DepartureTransportIDTextBox.Visible);
		}
	}
}
