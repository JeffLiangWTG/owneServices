using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;
using ISelectionItem = Enterprise.Customs.ASYCUDA.Business.ISelectionItem;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(MessageChooser))]
	sealed class MessageChooserTest : NonPersistentBusinessObjectTestCase
	{
		public void TestKeepCycleFieldsConsistentIfNeeded()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill1 = header.Bills.AddNew();
			bill1.CycleDate = new ZDateTime(2018, 6, 6);
			bill1.CycleNumber = "6";
			var bill2 = header.Bills.AddNew();
			bill2.CycleDate = new ZDateTime(2018, 7, 7);
			bill2.CycleNumber = "7";
			var bill3 = header.Bills.AddNew();
			bill3.CycleDate = new ZDateTime(2018, 9, 9);
			bill3.CycleNumber = "9";
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			chooser.CycleDate = new ZDateTime(2018, 8, 8);
			chooser.CycleNumber = "8";
			var chooserItem1 = chooser.ChooserItems[0];
			var chooserItem2 = chooser.ChooserItems[1];
			chooserItem1.Checked = true;
			chooserItem2.Checked = true;
			chooser.KeepCycleFieldsConsistentIfNeeded();
			AssertEquals(new ZDateTime(2018, 8, 8), bill1.CycleDate);
			AssertEquals("8", bill1.CycleNumber);
			AssertEquals(new ZDateTime(2018, 8, 8), bill2.CycleDate);
			AssertEquals("8", bill2.CycleNumber);
			AssertEquals(new ZDateTime(2018, 9, 9), bill3.CycleDate);
			AssertEquals("9", bill3.CycleNumber);
			AssertNoExceptionThrown(() =>
			{
				bill1.Delete();
				chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
				chooser.CycleDate = new ZDateTime(2018, 8, 8);
				chooser.CycleNumber = "8";
				chooser.KeepCycleFieldsConsistentIfNeeded();
			});
		}

		public void TestCycleDate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill1 = header.Bills.AddNew();
			bill1.CycleDate = new ZDateTime(2018, 6, 6);
			bill1.CycleNumber = "6";
			var bill2 = header.Bills.AddNew();
			bill2.CycleDate = new ZDateTime(2018, 7, 7);
			bill2.CycleNumber = "7";
			var bill3 = header.Bills.AddNew();
			bill3.CycleDate = new ZDateTime(2018, 7, 7);
			bill3.CycleNumber = "7";
			bill3.Validation.ValidateAll();
			Assert(!bill1.HasMessageErrors);
			Assert(!bill2.HasMessageErrors);
			Assert(bill3.HasMessageErrors);
			Env.Security.GlobalManifestSendWithMessageErrors.IsAllowed = false;
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2, bill3 }, true);
			chooser.CycleNumber = "7";
			chooser.CycleDate = new ZDateTime(2018, 7, 7);
			var chooserItem1 = chooser.ChooserItems[0];
			var chooserItem2 = chooser.ChooserItems[1];
			var chooserItem3 = chooser.ChooserItems[2];
			AssertEquals(false, chooserItem1.Checked);
			AssertEquals(true, chooserItem2.Checked);
			AssertEquals(false, chooserItem3.Checked);
			Env.Security.GlobalManifestSendWithMessageErrors.IsAllowed = true;
			bill1.Validation.ValidateAll();
			Assert(bill1.HasMessageErrors);
			chooser.CycleNumber = "6";
			chooser.CycleDate = new ZDateTime(2018, 6, 6);
			AssertEquals(false, chooserItem1.Checked);
			AssertEquals(false, chooserItem2.Checked);
			AssertEquals(false, chooserItem3.Checked);
		}

		public void TestCycleNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill1 = header.Bills.AddNew();
			bill1.CycleDate = new ZDateTime(2018, 6, 6);
			bill1.CycleNumber = "6";
			var bill2 = header.Bills.AddNew();
			bill2.CycleDate = new ZDateTime(2018, 7, 7);
			bill2.CycleNumber = "7";
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			chooser.CycleDate = new ZDateTime(2018, 7, 7);
			chooser.CycleNumber = "7";
			var chooserItem1 = chooser.ChooserItems[0];
			var chooserItem2 = chooser.ChooserItems[1];
			AssertEquals(false, chooserItem1.Checked);
			AssertEquals(true, chooserItem2.Checked);
		}

		public void TestRequiresCycleFields()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			var messageChooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			AssertEquals(true, messageChooser.RequiresCycleFields);
			messageChooser = new MessageChooser(header, new ISelectionItem[] { bill }, false);
			AssertEquals(false, messageChooser.RequiresCycleFields);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			return new MessageChooser(header, new ISelectionItem[] { bill }, true);
		}
	}
}
