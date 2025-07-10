using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(PackingListForm))]
	sealed class PackingListFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				AssertEquals($"Customs Packing List - {packingList.PackageJob.KJ_JobID}", form.FormCaption);
			}
		}

		public void TestPlugIns()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public void TestPackingListDetailsUserControl()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				AssertEquals(typeof(PackingListDetailsUserControl), form.PackingListDetailsDynamicUserControl.UserControlType);
			}
		}

		protected override Form GetFormToBashCore() => new PackingListForm(Factory.NewWithValidTestData<CusPackingList>());

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var packageJob = packingList.PackageJob;
			packageJob.Packages.AddNew();
			Factory.Save();
		}
		CusPackingList packingList;
	}
}
