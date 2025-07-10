using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(LocalCountryCustomsInterfaceUserControl))]
	sealed class LocalCountryCustomsInterfaceUserControlTest : RegistryZUserControlTestCase
	{
		public void TestABMInterfaceActivatedLabel_Visible()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DE1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			Factory.Save();

			var localCountryCustomsInterface = new LocalCountryCustomsInterface();
			localCountryCustomsInterface.CurrentFallbackLevel = new FallbackLevel(company, null, null);

			CombineAssertions(() =>
			{
				using (var form = new ZForm())
				using (var control = new LocalCountryCustomsInterfaceUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(localCountryCustomsInterface, null);
					AssertEquals("default - ABMInterface not activated", false, control.ABMInterfaceActivatedLabel.Visible);

					CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "Activated");
					localCountryCustomsInterface = new LocalCountryCustomsInterface();
					localCountryCustomsInterface.CurrentFallbackLevel = new FallbackLevel(company, null, null);
					control.SetDataBinding(localCountryCustomsInterface, null);
					AssertEquals("ABMInterface activated", true, control.ABMInterfaceActivatedLabel.Visible);
				}
			});
		}

		public void TestABMInterfaceActivatedLabel_Caption()
		{
			using (var control = new LocalCountryCustomsInterfaceUserControl())
			{
				AssertEquals("ABM Interface is configured for this company!", control.ABMInterfaceActivatedLabel.CaptionResourceString.Caption);
			}
		}

		public void TestSubmissionTypeDropEdit_Caption()
		{
			CombineAssertions(() =>
			{
				using (var control = new LocalCountryCustomsInterfaceUserControl())
				{
					var captionResourceString = control.SubmissionTypeDropEdit.CaptionResourceString;
					AssertEquals("Caption", "Submission Type", captionResourceString.Caption);
					AssertEquals("MediumCaption", "Submit Type", captionResourceString.MediumCaption);
				}
			});
		}

		protected override IBusiness GetNewBusinessEntity() => new LocalCountryCustomsInterface();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((LocalCountryCustomsInterfaceUserControl)control).RecipientIDTextBox.ReadOnly;
	}
}
