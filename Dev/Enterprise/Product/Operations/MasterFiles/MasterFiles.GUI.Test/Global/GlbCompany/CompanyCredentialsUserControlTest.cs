using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestsSubclassesOf(typeof(CompanyCredentialsUserControl))]
	public abstract class CompanyCredentialsUserControlTest : BasherTest
	{
		public override Form GetFormToBash()
		{
			var form = new ZChildForm { CaptionRenderingEnabled = true };
			var userControl = (CompanyCredentialsUserControl)Activator.CreateInstance(BashType);

			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(Provider.GetWrapper(Company), string.Empty);

			return form;
		}

		#region GlbExternalPassword

		protected GlbCompanyWrapperProvider Provider => provider ?? (provider = GlbCompanyWrapperProvider.GetProvider(CountryToTestAgainst));
		GlbCompanyWrapperProvider provider;

		protected abstract string CountryToTestAgainst { get; }

		#endregion

		#region GlbCompany

		protected GlbCompany Company
		{
			get
			{
				if (glbCompany == null)
				{
					glbCompany = Factory.New<GlbCompany>();
					glbCompany.GC_Code = "ZAC";
				}
				return glbCompany;
			}
		}
		GlbCompany glbCompany;

		#endregion
	}
}
