using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbBranchForm_CredentialUserControl))]
	sealed class GlbBranchForm_CredentialUserControlTest : BasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Fiji;

		public override Form GetFormToBash()
		{
			var form = new ZChildForm { CaptionRenderingEnabled = true };
			form.Controls.Add(new GlbBranchForm_CredentialUserControl { Dock = DockStyle.Fill });
			form.SetDataBinding(Branch.CertificateCredentialsTaxCore, string.Empty);
			return form;
		}

		#region GlbBranch

		protected override void SetUp()
		{
			base.SetUp();

			Branch.Factory.Save();
		}

		GlbBranch Branch => glbBranch ?? (glbBranch = Factory.NewWithValidTestData<GlbBranch>());
		GlbBranch glbBranch;

		#endregion

	}
}
