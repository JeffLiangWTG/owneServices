using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Module.Testing
{
	[TestedType(typeof(PalletTransactionModule))]
	internal class PalletTransactionModuleTest : ZModuleBasherTest
	{
		public void TestMarkAsExported()
		{
			var list = new List<PkgPalletTransaction>();
			var tran1 = Factory.NewWithValidTestData<PkgPalletTransaction>();
			tran1.KTR_Status = PalletTransactionStatusList.Codes.Held;
			list.Add(tran1);

			var tran2 = Factory.NewWithValidTestData<PkgPalletTransaction>();
			tran2.KTR_Status = PalletTransactionStatusList.Codes.Held;
			list.Add(tran2);

			var tran3 = Factory.NewWithValidTestData<PkgPalletTransaction>();
			tran3.KTR_Status = PalletTransactionStatusList.Codes.Held;

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			PalletTransactionModule.MarkExportedItemsAsProcessed(Enumerable.Empty<PkgPalletTransaction>(), Factory);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			PalletTransactionModule.MarkExportedItemsAsProcessed(list, Factory);
			AssertEquals("Do you want to mark the 2 exported pallet transactions as Exported, so that they can be excluded from future imports?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(PalletTransactionStatusList.Codes.Held, tran1.KTR_Status);
			AssertEquals(PalletTransactionStatusList.Codes.Held, tran2.KTR_Status);
			AssertEquals(PalletTransactionStatusList.Codes.Held, tran3.KTR_Status);
			AssertEquals(false, tran1.HasChanges);
			AssertEquals(false, tran2.HasChanges);
			AssertEquals(false, tran3.HasChanges);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PalletTransactionModule.MarkExportedItemsAsProcessed(list, Factory);
			AssertEquals("Do you want to mark the 2 exported pallet transactions as Exported, so that they can be excluded from future imports?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(PalletTransactionStatusList.Codes.Processed, tran1.KTR_Status);
			AssertEquals(PalletTransactionStatusList.Codes.Processed, tran2.KTR_Status);
			AssertEquals(PalletTransactionStatusList.Codes.Held, tran3.KTR_Status);
			AssertEquals(false, tran1.HasChanges);
			AssertEquals(false, tran2.HasChanges);
			AssertEquals(false, tran3.HasChanges);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.PalletTransaction;
		}
	}
}
