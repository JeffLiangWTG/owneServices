using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccSurchargeBasis))]
	public class AccSurchargeBasisUserControlTest : BasherTest
	{
		public override Form GetFormToBash() => CreateFormForTest();

		#region Implementation

		ZChildForm CreateFormForTest()
		{
			var accSurchargeConfiguration = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			Factory.Save();     // Required to prevent basher test failures due to HasChanges = true

			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new AccSurchargeBasisUserControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(accSurchargeConfiguration, nameof(AccSurchargeConfiguration.AccSurchargeBasises));
			return form;
		}

		#endregion
	}
}
