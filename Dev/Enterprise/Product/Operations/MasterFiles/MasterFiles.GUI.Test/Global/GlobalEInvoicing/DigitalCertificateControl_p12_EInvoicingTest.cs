using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class DigitalCertificateControl_p12_EInvoicingTest : DigitalCertificateControl_p12Test
	{
		public void TestAllowRegister()
		{
			using (var control = (DigitalCertificateControl_p12_EInvoicing)GetNewControl())
			{
				control.AllowRegister = true;
				Assert(control.FindSingle<ZButton>("RegisterButton").Visible);
				AssertEquals("Register", control.FindSingle<ZButton>("RegisterButton").CaptionResourceString.Caption);
				control.AllowRegister = false;
				Assert(!control.FindSingle<ZButton>("RegisterButton").Visible);
			}
		}

		protected override DigitalCertificateControl_p12 GetNewControl()
		{
			return new DigitalCertificateControl_p12_EInvoicing();
		}
	}
}
