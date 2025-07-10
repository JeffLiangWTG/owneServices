using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

class CustomsOfficesUserControlTest : TestCaseWithFactory
{
	public void TestCustomsOfficeDropBoxVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		using (var userControl = new CustomsOfficesUserControl())
		{
			userControl.HandleDeclarationControlVisibilityChanged();

			Assert("The office drop box should be visible.", userControl.Controls.Find("CustomsOfficeDropBox", true).First().Visible);

			Assert("The office find box should be invisible.", !userControl.Controls.Find("CustomsOfficeFindBox", true).First().Visible);
		}
	}

	public void TestCustomsOfficesGridVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		using (var form = new ZForm(declaration))
		using (var userControl = new JobDeclarationUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			var customsOfficesUserControl = (userControl.Controls.Find("CustomsOfficesUserControl", true).First() as ZDynamicControlCreationUserControl).HostedControl as CustomsOfficesUserControl;
			customsOfficesUserControl.HandleDeclarationControlVisibilityChanged();
			Assert("The grid should be visible for declaration type IMP.", customsOfficesUserControl.Controls.Find("CustomsOfficesGrid", true).First().Visible);
		}
	}
}
