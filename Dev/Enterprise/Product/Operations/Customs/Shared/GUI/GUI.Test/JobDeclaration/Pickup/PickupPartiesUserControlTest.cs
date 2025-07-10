using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class PickupPartiesUserControlTest : TestCase
	{
		public void TestControls()
		{
			using (var control = new PickupPartiesUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("DeliveryOrPickupCartageCoAddressControl", control.FindSingleOrDefault<ZAddressControl>("DeliveryOrPickupCartageCoAddressControl"));
					AssertNotNull("PickupDocAddressControl", control.FindSingleOrDefault<ZDocAddressControl>("PickupDocAddressControl"));
				});
			}
		}

		public void TestCaptions()
		{
			using (var control = new PickupPartiesUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("PickupDocAddressControl", "Pickup From", control.FindSingle<ZDocAddressControl>("PickupDocAddressControl").CaptionResourceString.Caption);
					AssertEquals("PickupCartageCoGroupBox", "Pickup Transport Company", control.FindSingle<ZGroupBox>("PickupCartageCoGroupBox").CaptionResourceString.Caption);
				});
			}
		}
	}
}
