using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccSurchargeConfiguration))]
	public class AccSurchargeConfigurationUserControlTest : BasherTest
	{
		public override Form GetFormToBash() => CreateFormForTest();

		#region Implementation

		ZChildForm CreateFormForTest()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();     // Required to prevent basher test failures due to HasChanges = true

			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new AccSurchargeConfigurationUserControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(company, nameof(GlbCompany.AccSurchargeConfigurations));
			return form;
		}

		#endregion
	}
}
