using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsUserControl))]
	sealed class CompanyCredentialsUserControlTest : BasherTest
	{
		public override Form GetFormToBash()
		{
			var form = new ZForm();
			form.CaptionRenderingEnabled = true;

			var userControl = new CompanyCredentialsUserControl();
			userControl.Dock = DockStyle.Fill;

			form.Controls.Add(userControl);
			var provider = (TWGlbCompanyWrapperProvider)GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.Taiwan);
			var glbCompany = Factory.New<GlbCompany>();
			glbCompany.GC_Code = "ZAC";
			form.SetDataBinding(provider.GetWrapper(glbCompany), "");

			return form;
		}
	}
}
