using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	sealed class MenuBuilderHelperTest : ASYCUDA.GUI.Testing.MenuBuilderHelperTest
	{
		public void TestShowMutexLockInfoWhenSending()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var orgHeader = GlbCompany.CurrentCompany.OrgProxy;
				orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00001", Core.Constants.CountryCodes.Singapore);
				Factory.Save();
				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				var newBill2 = factory.Load<AsycudaBill>(bill2.PK);
				using (var form = new ZForm(header))
				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					try
					{
						newBill2.SendToManifestMutex.Lock();
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(bill2.SendToManifestMutex.IsLocked);
						UnitTestUserNotification.Instance.ClearMessages();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						var sendToManifest = menu.MenuItems.FindByText("Send &Manifest");
						sendToManifest.PerformClick();
						AssertContains("Messages are currently being generated and sent by", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(bill2.SendToManifestMutex.IsLocked);
					}
					finally
					{
						if (newBill2.SendToManifestMutex.HasLock)
						{
							newBill2.SendToManifestMutex.Unlock();
						}
					}
				}
			}
		}

		public void TestShowMutexLockInfoWhenCancelling()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var orgHeader = GlbCompany.CurrentCompany.OrgProxy;
				orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00001", Core.Constants.CountryCodes.Singapore);
				Factory.Save();
				packedItem1.API_MessageStatus = "AWA";
				packedItem2.API_MessageStatus = "AWA";
				var bill3 = header.Bills.AddNew();
				var pack3 = bill3.Packs.AddNew();
				var packedItem3 = pack3.PackedItem;
				packedItem3.API_MessageStatus = "";
				Factory.Save();
				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				var newBill2 = factory.Load<AsycudaBill>(bill2.PK);
				var newBill3 = factory.Load<AsycudaBill>(bill3.PK);
				using (var form = new ZForm(header))
				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					var sendToManifest = menu.MenuItems.FindByText("&Cancel Manifest");
					var sendBill = menu.MenuItems.FindByText("Cancel Bills");
					try
					{
						newBill2.SendToManifestMutex.Lock();
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(bill2.SendToManifestMutex.IsLocked);
						Assert(!bill3.SendToManifestMutex.IsLocked);
						UnitTestUserNotification.Instance.ClearMessages();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						sendToManifest.PerformClick();
						AssertContains("Messages are currently being generated and sent by", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(bill2.SendToManifestMutex.IsLocked);
						Assert(!bill3.SendToManifestMutex.IsLocked);
						UnitTestUserNotification.Instance.ClearMessages();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						sendBill.PerformClick();
						AssertContains("Messages are currently being generated and sent by", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(bill2.SendToManifestMutex.IsLocked);
						Assert(!bill3.SendToManifestMutex.IsLocked);
					}
					finally
					{
						if (newBill2.SendToManifestMutex.HasLock)
						{
							newBill2.SendToManifestMutex.Unlock();
						}
					}

					try
					{
						newBill3.SendToManifestMutex.Lock();
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(!bill2.SendToManifestMutex.IsLocked);
						Assert(bill3.SendToManifestMutex.IsLocked);
						UnitTestUserNotification.Instance.ClearMessages();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						sendToManifest.PerformClick();
						AssertNotContains("Messages are currently being generated and sent by", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(!bill2.SendToManifestMutex.IsLocked);
						Assert(bill3.SendToManifestMutex.IsLocked);
						UnitTestUserNotification.Instance.ClearMessages();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						sendBill.PerformClick();
						AssertNotContains("Messages are currently being generated and sent by", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(!bill2.SendToManifestMutex.IsLocked);
						Assert(bill3.SendToManifestMutex.IsLocked);
					}
					finally
					{
						if (newBill3.SendToManifestMutex.HasLock)
						{
							newBill3.SendToManifestMutex.Unlock();
						}
					}
				}
			}
		}

		public void TestShowMutexLockInfoWhenAmending()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var orgHeader = GlbCompany.CurrentCompany.OrgProxy;
				orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00001", Core.Constants.CountryCodes.Singapore);
				Factory.Save();
				header.AMA_ManifestType = "MGE";
				packedItem1.API_MessageStatus = MessageStatusCodeList.Codes.Updated;
				var registrationEntry1 = packedItem1.CustomsEntryNumbers.AddNew();
				registrationEntry1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
				registrationEntry1.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
				registrationEntry1.CE_EntryNum = "1";
				packedItem2.API_MessageStatus = MessageStatusCodeList.Codes.Updated;
				var registrationEntry2 = packedItem2.CustomsEntryNumbers.AddNew();
				registrationEntry2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
				registrationEntry2.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
				registrationEntry2.CE_EntryNum = "2";
				var bill3 = header.Bills.AddNew();
				var pack3 = bill3.Packs.AddNew();
				var packedItem3 = pack3.PackedItem;
				packedItem3.API_MessageStatus = "";
				Factory.Save();
				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				var newBill2 = factory.Load<AsycudaBill>(bill2.PK);
				var newBill3 = factory.Load<AsycudaBill>(bill3.PK);
				using (var form = new ZForm(header))
				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					var sendToManifest = menu.MenuItems.FindByText("&Amend Bills");
					try
					{
						newBill2.SendToManifestMutex.Lock();
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(bill2.SendToManifestMutex.IsLocked);
						Assert(!bill3.SendToManifestMutex.IsLocked);
						UnitTestUserNotification.Instance.ClearMessages();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						sendToManifest.PerformClick();
						AssertContains("Messages are currently being generated and sent by", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(bill2.SendToManifestMutex.IsLocked);
						Assert(!bill3.SendToManifestMutex.IsLocked);
					}
					finally
					{
						if (newBill2.SendToManifestMutex.HasLock)
						{
							newBill2.SendToManifestMutex.Unlock();
						}
					}

					try
					{
						newBill3.SendToManifestMutex.Lock();
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(!bill2.SendToManifestMutex.IsLocked);
						Assert(bill3.SendToManifestMutex.IsLocked);
						UnitTestUserNotification.Instance.ClearMessages();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
						sendToManifest.PerformClick();
						AssertNotContains("Messages are currently being generated and sent by", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(!bill1.SendToManifestMutex.IsLocked);
						Assert(!bill2.SendToManifestMutex.IsLocked);
						Assert(bill3.SendToManifestMutex.IsLocked);
					}
					finally
					{
						if (newBill3.SendToManifestMutex.HasLock)
						{
							newBill3.SendToManifestMutex.Unlock();
						}
					}
				}
			}
		}

		public void TestSendManifestMenuItemConcurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var orgHeader = GlbCompany.CurrentCompany.OrgProxy;
				orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN00001", Core.Constants.CountryCodes.Singapore);
				header.AMA_ManifestType = "MGE";
				Factory.Save();
				using (var form = new ZForm(header))
				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					var sendManifest = menu.MenuItems.FindByText("&Send Manifest");
					AssertNotNull(sendManifest);
					int chooserItemCount = 0;
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
					ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
					{
						var itemSelectionDialog = (AsycudaItemSelectionDialog)dialog;
						try
						{
							chooserItemCount = ((MessageChooser)itemSelectionDialog.DataSource).ChooserItems.Count;
							itemSelectionDialog.CancelButton.PerformClick();
						}
						finally
						{
							itemSelectionDialog?.Close();
						}
					});
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					sendManifest.PerformClick();
					AssertEquals("Chooser displays both bills", 2, chooserItemCount);
					var otherFactory = new BusinessObjectFactory();
					otherFactory.RefreshEnabled = false;
					otherFactory.Load<AsycudaPackedItem>(packedItem1.PK).API_MessageStatus = MessageStatusCodeList.Codes.Sent;
					otherFactory.Save();
					menu.OnPopup(EventArgs.Empty);
					sendManifest = menu.MenuItems.FindByText("&Send Manifest");
					AssertNotNull(sendManifest);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					sendManifest.PerformClick();
					AssertEquals("Chooser should know one bill was sent in the other factory and not display it", 1, chooserItemCount);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			sgACCESSEnable = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			header = Factory.New<AsycudaManifestHeaderForTest>();
			header.AMA_ManifestType = "MGI";
			bill1 = header.Bills.AddNew();
			bill2 = header.Bills.AddNew();
			pack1 = bill1.Packs.AddNew();
			pack2 = bill2.Packs.AddNew();
			packedItem1 = pack1.PackedItem;
			packedItem2 = pack2.PackedItem;
			Factory.Save();
		}

		protected override void TearDown()
		{
			sgACCESSEnable?.Dispose();
			base.TearDown();
		}

		IDisposable sgACCESSEnable;
		AsycudaManifestHeaderForTest header;
		AsycudaBill bill1;
		AsycudaBill bill2;
		AsycudaPack pack1;
		AsycudaPack pack2;
		AsycudaPackedItem packedItem1;
		AsycudaPackedItem packedItem2;
		sealed class AsycudaManifestHeaderForTest : Business.AsycudaManifestHeader
		{
			public AsycudaManifestHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper() => new SGMessageSendingNotificationHelperTest(this);
		}

		sealed class SGMessageSendingNotificationHelperTest : MessageSendingNotificationHelper
		{
			public SGMessageSendingNotificationHelperTest(AsycudaManifestHeader header) : base(header)
			{
			}

			protected override ZString GetExtraMessageSendingNotificationCore() => ZString.Empty;
		}
	}
}
