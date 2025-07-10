using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(PackableItemSplitPartsForm))]
	sealed class PackableItemSplitPartsFormTest : ZFormBasherTest
	{
		public void TestMinItemsCheck()
		{
			using (var form = new PackableItemSplitPartsForm(PackableItemsSplitter))
			{
				form.Show();
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Error Please enter at least two items.", UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var part1 = PackableItemsSplitter.PackableItemParts.AddNew();
				part1.GoodsDescription = "Part 1";
				part1.PackableQuantity = 101m;
				part1.PackableUQ = "CTN";
				part1.NetWeight = 100m;
				part1.NetWeightUQ = Core.Constants.Weight.Kilograms;
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Error Please enter at least two items.", UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var part2 = PackableItemsSplitter.PackableItemParts.AddNew();
				part2.GoodsDescription = "Part 2";
				part2.PackableQuantity = 143m;
				part2.PackableUQ = "CTN";
				part2.NetWeight = 200m;
				part2.NetWeightUQ = Core.Constants.Weight.Kilograms;
				okButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore() => (Form)Activator.CreateInstance(FormToBashType, new object[] { PackableItemsSplitter });

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		PackableItemsSplitter PackableItemsSplitter
		{
			get
			{
				if (fPackableItemsSplitter == null)
				{
					var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					var invoice = declaration.Invoices.AddNew();
					var mockInvoiceLine = Factory.NewMoq<BaseJobComInvoiceLine>();
					var packableItemUQList = new CodeDescriptionPairList();
					packableItemUQList.AddPair("CTN", "Carton");
					mockInvoiceLine.Setup(x => x.GetPackableItemUQList()).Returns(packableItemUQList);
					var invoiceLine = mockInvoiceLine.Object;
					invoice.JobComInvoiceLines.Add(invoiceLine);
					invoiceLine.JI_JZ = invoice.PK;
					Factory.Save();
					var packingList = declaration.LoadOrCreateCusPackingList(Factory);
					var packages = packingList.PackageJob.Packages.AddNew();
					var packagesRelation = packages.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
					fPackableItemsSplitter = new PackableItemsSplitter(packages, packagesRelation);
				}
				return fPackableItemsSplitter;
			}
		}
		PackableItemsSplitter fPackableItemsSplitter;
	}
}
