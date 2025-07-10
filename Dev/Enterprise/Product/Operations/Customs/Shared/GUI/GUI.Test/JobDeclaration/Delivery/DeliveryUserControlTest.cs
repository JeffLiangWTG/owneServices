using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class DeliveryUserControlTest : TestCase
	{
		public void TestControls()
		{
			using (var form = new ZForm())
			using (var control = new DeliveryUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("DeliveryDetailsUserControl", typeof(DeliveryDetailsUserControl), control.FindSingleOrDefault<ZDynamicControlCreationUserControl>("DeliveryDetailsUserControl").UserControlType);
					AssertEquals("DeliveryPartiesUserControl", typeof(DeliveryPartiesUserControl), control.FindSingleOrDefault<ZDynamicControlCreationUserControl>("DeliveryPartiesUserControl").UserControlType);
				});
			}
		}
	}
}
