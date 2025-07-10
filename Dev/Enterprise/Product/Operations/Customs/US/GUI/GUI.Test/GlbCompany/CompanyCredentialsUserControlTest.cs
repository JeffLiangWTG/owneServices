using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
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
			var provider = (GlbCompanyWrapperProvider)GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.UnitedStates);
			var glbCompany = Factory.New<GlbCompany>();
			glbCompany.GC_Code = "ZAC";
			form.Controls.Add(userControl);
			form.SetDataBinding(provider.GetWrapper(glbCompany), "");

			return form;
		}
	}
}
