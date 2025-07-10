using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccChargeGovtChargeCodeOverrideConfigurationControl))]
	public class AccChargeApportionmentMethodOverrideControlTest : BasherTest
	{
		public override Form GetFormToBash() => CreateFormForTest();

		#region Implementation

		ZChildForm CreateFormForTest()
		{
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());

			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new AccChargeApportionmentMethodOverrideControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(chargeCode, nameof(AccChargeCode.ApportionmentMethodOverrides));
			return form;
		}

		#endregion
	}
}
