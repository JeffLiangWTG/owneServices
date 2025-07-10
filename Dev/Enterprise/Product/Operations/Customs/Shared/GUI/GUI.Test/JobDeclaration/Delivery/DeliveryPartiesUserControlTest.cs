using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class DeliveryPartiesUserControlTest : TestCase
	{
		public void TestControls()
		{
			using (var control = new DeliveryPartiesUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("DeliveryOrPickupCartageCoAddressControl", control.FindSingleOrDefault<ZAddressControl>("DeliveryOrPickupCartageCoAddressControl"));
					AssertNotNull("DeliveryDocAddressControl", control.FindSingleOrDefault<ZDocAddressControl>("DeliveryDocAddressControl"));
				});
			}
		}

		public void TestCaptions()
		{
			using (var control = new DeliveryPartiesUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("DeliveryDocAddressControl", "Delivery Address", control.FindSingle<ZDocAddressControl>("DeliveryDocAddressControl").CaptionResourceString.Caption);
					AssertEquals("DeliveryCartageCoGroupBox", "Delivery Transport Company", control.FindSingle<ZGroupBox>("DeliveryCartageCoGroupBox").CaptionResourceString.Caption);
				});
			}
		}
	}
}
