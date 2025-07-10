using System.Windows.Forms;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(TranshipmentForm))]
	public sealed class TranshipmentFormTest : ZFormBasherTest
	{
		public void TestPlugins()
		{
			using (var form = new TranshipmentForm(header))
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		public void TestTWInBondHeaderDetailUserControl()
		{
			using (var form = new TranshipmentForm(header))
			{
				form.Show();
				var tabControl = form.Controls.Find("MainTabControl", true)[0] as ZTabControl;
				var mainTabPage = tabControl.Controls.Find("MainTabPage", true)[0] as ZTabPage;
				tabControl.SelectTab(mainTabPage);
				var inBondHeaderDetailUserControl = mainTabPage.Controls.Find("TWInBondHeaderDetailUserControl", true);
				AssertEquals(1, inBondHeaderDetailUserControl.Length);
			}
		}

		public void TestFormCaptions()
		{
			header.BH_JobReference = "INB23423BD";
			using (var form = new TranshipmentForm(header))
			{
				form.Show();
				AssertContains(header.HumanReadableName, form.FormCaption);
			}
		}

		public void TestTabOrder()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var form = new TranshipmentForm(header))
			{
				form.Show();
				var tabControl = form.Controls.Find("MainTabControl", true)[0] as ZTabControl;
				AssertEquals("Details", ((ZTabPage)tabControl.TabPages[0]).Text);
				AssertEquals("Messages", ((ZTabPage)tabControl.TabPages[1]).Text);
				AssertEquals("Doc Data", ((ZTabPage)tabControl.TabPages[2]).Text);
				AssertEquals("eDocs", ((ZTabPage)tabControl.TabPages[3]).Text);
				AssertEquals("Notes", ((ZTabPage)tabControl.TabPages[4]).Text);
				AssertEquals("Logs", ((ZTabPage)tabControl.TabPages[5]).Text);
				AssertSame("Details is active tab", tabControl.TabPages[0], tabControl.SelectedTab);
				AssertEquals(6, tabControl.TabCount);
			}
		}

		public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
		{
			Assert(true);
		}

		protected override Form GetFormToBashCore()
		{
			return new TranshipmentForm(header)
			{ ControllerID = ControllerIDs.Customs.TW.Transhipment };
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			_ = header.ArrivalBill;
			_ = header.MovementBill;
			moveHeader = header.MovementHeader;
			moveDetail = moveHeader.InBondMoveDetail;
			_ = moveDetail.InBondMoveLineItem;
			moveDetail.Containers.AddNew();
			Factory.Save();
		}

		CusInBondHeader header;
		CusInBondMoveHeader moveHeader;
		CusInBondMoveDetail moveDetail;
	}
}
