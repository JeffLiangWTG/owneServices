using System.Reflection;
using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.CMDMessaging.Testing
{
	[TestedType(typeof(GHACaptureForm))]
	sealed class GHACaptureFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = (GHACaptureForm)GetFormToBash())
			{
				AssertEquals("Select GHA", form.FormHeading);
			}
		}

		public void TestOKButtonClicked()
		{
			using (var form = (GHACaptureForm)GetFormToBash())
			{
				var oKButton = (ZButton)typeof(GHACaptureForm).GetField("oKBoundButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
				var gHACapture = (GHACapture)form.BusinessEntity;
				gHACapture.GHA = "TEST";
				form.Show();
				AssertEquals(true, form.Visible);
				oKButton.PerformClick();
				AssertHasErrors(gHACapture.GHAInfo);
				AssertEquals(true, form.Visible);
				gHACapture.GHA = Core.Constants.SGCustoms.GHA.SATS;
				oKButton.PerformClick();
				AssertNoErrors(gHACapture.GHAInfo);
				AssertEquals(false, form.Visible);
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(Core.Constants.SGCustoms.GHA.SATS, gHACapture.GHA);
			}
		}

		public void TestCancelButtonClicked()
		{
			using (var form = (GHACaptureForm)GetFormToBash())
			{
				var cancelButton = (ZButton)typeof(GHACaptureForm).GetField("cancelBoundButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
				var gHACapture = (GHACapture)form.BusinessEntity;
				gHACapture.GHA = "TEST";
				form.Show();
				AssertEquals(true, form.Visible);
				cancelButton.PerformClick();
				AssertEquals(false, form.Visible);
				AssertEquals("", gHACapture.GHA);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var cMDShipmentWrapper = new CMDShipmentWrapper(shipment);
			return new GHACaptureForm(new GHACapture(cMDShipmentWrapper));
		}
	}
}
