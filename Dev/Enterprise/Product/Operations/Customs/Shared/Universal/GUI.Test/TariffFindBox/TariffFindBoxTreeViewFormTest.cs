using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(TariffFindBoxTreeViewForm))]
	class TariffFindBoxTreeViewFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestSearchResults()
		{
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11", yesterday, tomorrow, "DESC GROUP 1", "A");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "20", yesterday, tomorrow, "DESC GROUP 3", "C.D");
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "11.20.00", yesterday, tomorrow, "DESC TARIFF 1", compositeKey: "A.C.D");
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "20.00", yesterday, tomorrow, "DESC TARIFF 2", compositeKey: "C.D");
			Factory.Save();
			var helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.Eritrea, "TST", null, null);
			helper.ChapterHeadingTariff = "1111111";
			using (var form = new TariffFindBoxTreeViewForm(helper))
			{
				form.Show();
				var tree = (ZTreeView)form.Controls.Find("TariffTreeView", true).First();
				AssertEquals(0, tree.Nodes.Count);
				AssertEquals("There are no records that match your search.", form.MessageStatusBarPanel.Text);
			}

			helper.ChapterHeadingTariff = "11";
			using (var form = new TariffFindBoxTreeViewForm(helper))
			{
				form.Show();
				var tree = (ZTreeView)form.Controls.Find("TariffTreeView", true).First();
				AssertEquals(1, tree.Nodes.Count);
				AssertEquals("Found 1 record that matches your search criteria.", form.MessageStatusBarPanel.Text);
			}
		}

		[RequiresSTA]
		public void TestTruncateNodeText()
		{
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "11", yesterday, tomorrow, new ZString('W', 1024));
			Factory.Save();
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null);
			helper.ChapterHeadingTariff = "11";
			using (var form = new TariffFindBoxTreeViewForm(helper))
			{
				form.Show();
				var tree = form.Controls.Find("TariffTreeView", true)[0] as ZTreeView;
				AssertEquals("11 WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW...", tree.Nodes[0].Nodes[0].Text);
			}

			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "25", yesterday, tomorrow, new ZString("TARIFF DESCRIPTION \r\nFor TEST"));
			Factory.Save();
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null);
			helper.ChapterHeadingTariff = "25";
			using (var form = new TariffFindBoxTreeViewForm(helper))
			{
				form.Show();
				var tree = form.Controls.Find("TariffTreeView", true)[0] as ZTreeView;
				AssertEquals("Trim out new line characters", "25 TARIFF DESCRIPTION For TEST", tree.Nodes[0].Nodes[0].Text);
			}
		}

		[RequiresSTA]
		public void TestRemoveDuplicatedDescriptionHeirarchy()
		{
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11", yesterday, tomorrow, "DESC GROUP 1", "A");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "20", yesterday, tomorrow, "DESC GROUP 2", "A.B");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "30", yesterday, tomorrow, "DESC GROUP 2", "A.B.C");
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "11.20.30.00", yesterday, tomorrow, "DESC TARIFF 1", compositeKey: "A.B.C.D");
			Factory.Save();
			var helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.Eritrea, "TST", null, null);
			helper.ChapterHeadingTariff = "11";
			using (var form = new TariffFindBoxTreeViewForm(helper))
			{
				form.Show();
				var tree = (ZTreeView)form.Controls.Find("TariffTreeView", true).First();
				AssertEquals("11 DESC GROUP 1", tree.Nodes[0].Text);
				AssertEquals("30 DESC GROUP 2", tree.Nodes[0].Nodes[0].Text);
				AssertEquals("11.20.30.00 DESC TARIFF 1", tree.Nodes[0].Nodes[0].Nodes[0].Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Ethiopia, ZString.Empty, null, null);
			helper.HasChanges = false;
			return new TariffFindBoxTreeViewForm(helper);
		}

		protected override void SetUp()
		{
			base.SetUp();
			dataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			yesterday = ZDateTime.Now.AddDays(-1);
			tomorrow = ZDateTime.Now.AddDays(1);
		}

		Universal.Testing.UniversalReferenceTestDataHelper dataHelper;
		ZDateTime yesterday;
		ZDateTime tomorrow;
	}
}
