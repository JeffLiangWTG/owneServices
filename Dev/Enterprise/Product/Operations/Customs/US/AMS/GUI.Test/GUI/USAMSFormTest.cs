using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	[TestedType(typeof(USAMSForm))]
	sealed class USAMSFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = Enterprise.Customs.US.AMS.Business.DirectionTypeList.Codes.NVOCC;
			using (var form = new USAMSForm(header))
			{
				form.Show();
				AssertContains("NVOCC", form.FormCaption);
				form.FireSaveButton();
				AssertContains(header.BH_JobReference, form.FormCaption);
			}
		}

		public void TestSailingSynchronisation()
		{
			var header = Factory.New<CusInBondHeader>();
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			var sailing = Factory.New<JobSailing>();
			sailing.JX_JB = destination.PK;
			sailing.JX_JA = origin.PK;
			using (var form = new USAMSForm(header))
			{
				form.Show();
				var actionMenuItem = form.Menu.MenuItems.FindByText("AMS");
				var importFromSailingMenuItem = actionMenuItem.MenuItems.FindByText(USAMSForm.ImportBillsLinkedToSailing);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				importFromSailingMenuItem.PerformClick();
				AssertEquals(USAMSForm.NoSailingSelected, UnitTestUserNotification.Instance.LastMessage.Text);
				header.ChangeSailing(sailing.PK);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				importFromSailingMenuItem.PerformClick();
				AssertNotEquals(USAMSForm.NoSailingSelected, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSelectAndShowBill()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			using (var form = new USAMSForm(header))
			{
				form.Show();
				Application.DoEvents();
				var billsUserControl = GetControl<USAMSBillsUserControl>(form, "usamsBillsUserControl");
				var grid = GetControl<USAMSBillsUserControl, ZGrid>(billsUserControl, "BillsGrid");
				AssertEquals("hidden to begin with", false, grid.Visible);
				form.SelectAndShowBill(bill1.PK);
				AssertEquals("should be shown now", true, grid.Visible);
				AssertEquals("should have selected bill1", bill1, grid.ListManager.GetCurrent());
				form.SelectAndShowBill(bill2.PK);
				AssertEquals("should have selected bill2", bill2, grid.ListManager.GetCurrent());
			}
		}

		T GetControl<T>(USAMSForm form, string name)
			where T : Control
		{
			return GetControl<USAMSForm, T>(form, name);
		}

		T GetControl<P, T>(P parent, string name)
			where P : Control
			where T : Control
		{
			return (T)typeof(P).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parent);
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			container.Commodities.AddNew();
			Factory.Save();
			return new USAMSForm(header)
			{
				ControllerID = ControllerIDs.Customs.US.AMS
			};
		}
	}
}
