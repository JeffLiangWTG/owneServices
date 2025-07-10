using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestsSubclassesOf(typeof(StaffCredentialsUserControl))]
	public abstract class StaffCredentialsUserControlTest : BasherTest
	{
		public override sealed Form GetFormToBash()
		{
			var provider = GlbStaffWrapperProvider.GetProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var form = new ZChildForm();

			form.CaptionRenderingEnabled = true;

			var userControl = (StaffCredentialsUserControl)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));

			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(provider.GetWrapper(glbStaff), "");

			return form;
		}

		protected override void SetUp()
		{
			base.SetUp();
			glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_Code = "ZAC";
		}
		protected GlbStaff glbStaff;
	}
}
