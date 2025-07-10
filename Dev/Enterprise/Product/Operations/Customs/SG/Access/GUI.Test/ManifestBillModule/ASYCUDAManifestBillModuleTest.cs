using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Module;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(ASYCUDAManifestBillModule))]
	sealed class ASYCUDAManifestBillModuleTest : ASYCUDA.Module.Testing.ASYCUDAManifestBillModuleAbstractTest
	{
		public override void TestExceptionsFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public override void TestMilestonesFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public override void TestAutoAddedTaskStatusFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public override void TestTasksFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public override void TestTriggersFilter()
		{
			Assert("Not available on Manifest Bill module", true);
		}

		public void TestSendToCustomsMenuItem()
		{
			(AsycudaManifestHeader header1, AsycudaBill header1Bill1, AsycudaBill header1Bill2) = MultiManifestBillSenderTest.CreateManifest(Factory, "JOB3", "MB1");
			(AsycudaManifestHeader header2, AsycudaBill header2Bill1, AsycudaBill header2Bill2) = MultiManifestBillSenderTest.CreateManifest(Factory, "JOB1", "MB3");
			(AsycudaManifestHeader header3, AsycudaBill header3Bill1, AsycudaBill header3Bill2) = MultiManifestBillSenderTest.CreateManifest(Factory, "JOB2", "MB2");
			Factory.Save();
			using (ZForm moduleForm = new ZForm())
			using (var module = new ManifestBillModule())
			{
				var filterControl = module.EmbeddedControl;
				moduleForm.Controls.Add(filterControl);
				moduleForm.Show();
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();
				Application.DoEvents();
				var viewCollection = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.Customs.ASYCUDA.SGAccess.ManifestBill);
				AssertEquals(6, viewCollection.Count);
				UnitTestUserNotification.Instance.ClearMessages();
				var sendMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Send Import Manifests to Customs");
				sendMenuItem.PerformClick();
				AssertEquals(ManifestBillModule.SelectAtLeastOneBillNotSentToCustoms, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				module.DisplayGrid.SelectAllElements((x) => x.PK != header2Bill2.PK);
				sendMenuItem.PerformClick();
				var dialog = ZFormModaliser.LastFormShownDialogForTest;
				AssertType<MultiManifestBillSenderDialog>(dialog);
				var lastBizObj = (MultiManifestBillSender)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				var masterDatas = lastBizObj.MasterDatas.ToArray();
				AssertEquals(3, masterDatas.Length);
				AssertEquals(3, masterDatas.Length);
				MultiManifestBillSenderTest.AssertMasterData(masterDatas[0], header2, new[] { header2Bill1 });
				MultiManifestBillSenderTest.AssertMasterData(masterDatas[1], header3, new[] { header3Bill2, header3Bill1 });
				MultiManifestBillSenderTest.AssertMasterData(masterDatas[2], header1, new[] { header1Bill2, header1Bill1 });
				var header2Bill3 = header2.Bills.AddNew();
				var pack = header2Bill3.Packs.AddNew();
				var packItem = pack.PackedItem;
				Factory.Save();
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();
				Application.DoEvents();
				var factory = new BusinessObjectFactory();
				var newheader2Bill2 = factory.Load<AsycudaBill>(header2Bill2.PK);
				var newheader2Bill3 = factory.Load<AsycudaBill>(header2Bill3.PK);
				try
				{
					UnitTestUserNotification.Instance.ClearMessages();
					module.DisplayGrid.SelectAllElements((x) => x.PK == header2Bill2.PK || x.PK == header2Bill3.PK);
					sendMenuItem.PerformClick();
					AssertNotContains("One or more of the bills selected is currently being sent by another user. Do you want to send the rest?", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(!header2Bill2.SendToManifestMutex.IsLocked);
					Assert(!header2Bill3.SendToManifestMutex.IsLocked);
					newheader2Bill2.SendToManifestMutex.Lock();
					newheader2Bill3.SendToManifestMutex.Lock();
					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, send the rest
					module.DisplayGrid.SelectAllElements((x) => x.PK == header2Bill2.PK || x.PK == header2Bill3.PK);
					sendMenuItem.PerformClick();
					AssertContains("Messages are currently being generated and sent by", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(header2Bill2.SendToManifestMutex.IsLocked);
					Assert(header2Bill3.SendToManifestMutex.IsLocked);
					if (newheader2Bill2.SendToManifestMutex.HasLock)
					{
						newheader2Bill2.SendToManifestMutex.Unlock();
					}

					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, send the rest
					module.DisplayGrid.SelectAllElements((x) => x.PK == header2Bill2.PK || x.PK == header2Bill3.PK);
					sendMenuItem.PerformClick();
					AssertContains("One or more of the bills selected is currently being sent by another user. Do you want to send the rest?", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(!header2Bill2.SendToManifestMutex.IsLocked);
					Assert(header2Bill3.SendToManifestMutex.IsLocked);
					lastBizObj = (MultiManifestBillSender)ZFormModaliser.LastIBusinessShownOnDialogForTest;
					masterDatas = lastBizObj.MasterDatas.ToArray();
					AssertEquals(1, masterDatas.Length);
					MultiManifestBillSenderTest.AssertMasterData(masterDatas[0], header2, new[] { header2Bill2 });
				}
				finally
				{
					if (newheader2Bill2.SendToManifestMutex.HasLock)
					{
						newheader2Bill2.SendToManifestMutex.Unlock();
					}

					if (newheader2Bill3.SendToManifestMutex.HasLock)
					{
						newheader2Bill3.SendToManifestMutex.Unlock();
					}
				}
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.ASYCUDA.SGAccess.ManifestBill;
	}
}
