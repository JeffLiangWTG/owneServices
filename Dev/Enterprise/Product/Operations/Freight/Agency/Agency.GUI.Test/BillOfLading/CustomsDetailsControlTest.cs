using System.Reflection;
using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class CustomsDetailsControlTest : BaseAgencyTest
	{
		[UseDummyCustomsMessageStatusProvider]
		public void TestDetailsButton()
		{
			DummyCustomsMessageStatusProvider.Instance.ShouldShow = true;
			DummyCustomsMessageStatusProvider.Instance.UserFriendlyStatusMessage = "User Friendly Status Message";
			BillOfLading bill = Factory.New<BillOfLading>();
			using (ZForm form = new ZForm(bill))
			{
				CustomsDetailsControl control = new CustomsDetailsControl();
				form.Controls.Add(control);
				control.SetDataBinding(bill, "");
				form.Show();
				Button detailsButton = GetControl<Button>(control, "DetailsButton");
				detailsButton.PerformClick();
				AssertEquals("Information User Friendly Status Message", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		#region Implementation
		T GetControl<T>(CustomsDetailsControl control, string name)
		{
			return (T)typeof(CustomsDetailsControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}
		#endregion
	}
}
