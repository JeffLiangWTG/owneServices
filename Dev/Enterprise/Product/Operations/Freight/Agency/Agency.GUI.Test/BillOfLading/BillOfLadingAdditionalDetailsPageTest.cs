using System.Reflection;
using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class BillOfLadingAdditionalDetailsPageTest : BaseAgencyTest
	{
		[UseDummyCustomsMessageStatusProvider]
		public void TestShowCustomsDetails()
		{
			BillOfLading bill = Factory.New<BillOfLading>();
			DummyCustomsMessageStatusProvider.Instance.ShouldShow = true;
			using (ZForm form = new ZForm(bill))
			{
				BillOfLadingAdditionalDetailsPage control = new BillOfLadingAdditionalDetailsPage();
				form.Controls.Add(control);
				control.SetDataBinding(bill, "");
				form.Show();
				Control customsDetailPanel = GetControl<Control>(control, "customsDetailPanel");
				AssertEquals("customsDetailPanel.Visible", true, customsDetailPanel.Visible);
			}

			DummyCustomsMessageStatusProvider.Instance.ShouldShow = false;
			using (ZForm form = new ZForm(bill))
			{
				BillOfLadingAdditionalDetailsPage control = new BillOfLadingAdditionalDetailsPage();
				form.Controls.Add(control);
				control.SetDataBinding(bill, "");
				form.Show();
				Control customsDetailPanel = GetControl<Control>(control, "customsDetailPanel");
				AssertEquals("customsDetailPanel.Visible", false, customsDetailPanel.Visible);
			}
		}

		#region Implementation
		T GetControl<T>(BillOfLadingAdditionalDetailsPage control, string name)
		{
			return (T)typeof(BillOfLadingAdditionalDetailsPage).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}
		#endregion
	}
}
