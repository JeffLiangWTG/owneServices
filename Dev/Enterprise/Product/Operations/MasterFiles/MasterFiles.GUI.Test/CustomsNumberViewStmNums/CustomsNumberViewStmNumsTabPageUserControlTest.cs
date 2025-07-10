using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ZChildForm))]
	sealed class CustomsNumberViewStmNumsTabPageUserControlTest : ZFormBasherTest
	{
		#region Implementation

		ZChildForm Form
		{
			get
			{
				if (form == null)
				{
					((CustomsNumberViewStmNumsGuiProviderForTesting)Company.CustomsNumberProvider).getUserControlForTesting = GetUserControlForTesting;
					form = new ZChildForm(Company);
					form.Size = new System.Drawing.Size(1024, 768);
					form.CaptionRenderingEnabled = true;
					var control = new CustomsNumberViewStmNumsTabPageUserControl();
					control.Dock = DockStyle.Fill;
					form.Controls.Add(control);
					form.SetDataBinding(Company, "");
				}
				return form;
			}
		}
		ZChildForm form;

		Control GetUserControlForTesting(CustomsNumberViewStmNumsCompanyProviderForTest provider)
		{
			return new CustomsNumberViewStmNumsCompanyUserControl(provider.CustomsNumberWrappers);
		}

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;

		protected override Form GetFormToBashCore()
		{
			return Form;
		}

		protected override void SetUp()
		{
			base.SetUp();
			countrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea);
			providerSetup = providerSetup ?? new CustomsNumberViewStmNumsGuiProviderForTestingSetUp();
		}
		static IDisposable providerSetup;
		IDisposable countrySetter;

		protected override void TearDown()
		{
			if (providerSetup != null)
			{
				providerSetup.Dispose();
				providerSetup = null;
			}
			if (form != null)
			{
				form.Dispose();
				form = null;
			}
			if (countrySetter != null)
			{
				countrySetter.Dispose();
				countrySetter = null;
			}
			base.TearDown();
		}

		#endregion
	}
}
