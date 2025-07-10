using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.GUI
{
	public class CommercialInvoiceEDIMenu : Customs.GUI.CommercialInvoiceEDIMenu
	{
		public CommercialInvoiceEDIMenu()
		{
			if (DisplayCreatePackingListMenuOption)
			{
				packingListMenuItem = new ZMenuItem(GetPackingListMenuItemCaption(), CreatePackingListMenuItem_Click);
			}
			MenuItems.Clear();
			MenuItems.AddRange(MenuItemsInOrder().ToArray());
		}

		IEnumerable<MenuItem> MenuItemsInOrder()
		{
			yield return autoApportionWeightMenuItem;
			yield return allocateRemainingWeightMenuItem;
			yield return copyPreviousInvoiceLineMenuItem;
			if (packingListMenuItem != null)
			{
				yield return packingListMenuItem;
			}
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			autoApportionWeightMenuItem.Visible = (Declaration as JobDeclaration)?.IsWeightApportionmentSupported ?? true;
			if (packingListMenuItem != null)
			{
				packingListMenuItem.Caption = GetPackingListMenuItemCaption();
			}
		}

		JobComInvoiceHeader InvoiceHeader => Declaration?.Invoices.FirstOrDefault() as JobComInvoiceHeader;

		bool DisplayCreatePackingListMenuOption => TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.Value;

		protected ZMenuItem packingListMenuItem;

		ResourceString GetPackingListMenuItemCaption() => InvoiceHeader?.HasCusPackingList ?? false ? ResString.GetMultilingualString("14236D96-EE95-43B8-B23D-A54C646C656B", "Edit Packing List") : ResString.GetMultilingualString("C62DE482-A03B-42B9-8975-4C9C48482D41", "Create Packing List");

		void CreatePackingListMenuItem_Click(object sender, EventArgs e)
		{
			if (PreSaveForm() && InvoiceHeader.IsInDatabase)
			{
				var factoryForPackingList = new BusinessObjectFactory();
				factoryForPackingList.Saved += FactoryForPackingList_Saved;
				factoryForPackingList.ChildFactories.Add(InvoiceHeader.Factory);

				var packingList = InvoiceHeader.LoadCusPackingList(factoryForPackingList);

				try
				{
					if (packingList == null)
					{
						packingList = CreateNewPackingList(factoryForPackingList);
					}

					if (packingList != null)
					{
						packingList.AddDefaultPackageIfNeeded();

						var controller = ZControllerFactory.Create(ControllerIDs.Customs.CusPackingList);
						controller.SetFormsModalTo(Form);
						var form = packingList.IsInDatabase ? controller.ShowEditForm(packingList) : controller.ShowFormForNewEntity(packingList);
						if (form != null)
						{
							form.Closed += PackingListForm_Closed;
						}
#if DEBUG
						if (Globals.IsTest)
						{
							LastController = controller;
						}
#endif
					}
				}
				catch (Exception)
				{
					UnlockMutexIfLockedByThisInstance();
					throw;
				}
			}
		}

		bool PreSaveForm()
		{
			return Customs.GUI.PlugIn.CustomsPlugIn.FormPreSaved(InvoiceHeader, Form);
		}

		CusPackingList CreateNewPackingList(BusinessObjectFactory factoryForPackingList)
		{
			CusPackingList packingList = null;

			if (!MutexForCreatePackingList.IsLocked)
			{
				if (MutexForCreatePackingList.Lock())
				{
					packingList = InvoiceHeader.CreateCusPackingList(factoryForPackingList);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("8B755880-E633-46D0-BBB0-58F91B19D6BC", "Someone else is already in the process of creating a Packing List.\r\nYou should be able to access the Packing List when the person has saved the record. Please try later."));
			}

			return packingList;
		}

		void FactoryForPackingList_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= FactoryForPackingList_Saved;
			factory.ChildFactories.Clear();
			UnlockMutexIfLockedByThisInstance();
		}

		void PackingListForm_Closed(object sender, EventArgs e)
		{
			UnlockMutexIfLockedByThisInstance();
		}

		void UnlockMutexIfLockedByThisInstance()
		{
			if (MutexForCreatePackingList.HasLock)
			{
				MutexForCreatePackingList.Unlock();
			}
		}

		ZGlobalMutex MutexForCreatePackingList
		{
			get { return mutex ?? (mutex = new ZGlobalMutex(MutexIDs.CusPackingListMutex, InvoiceHeader.PK.ToString())); }
		}
		ZGlobalMutex mutex;

#if DEBUG
		public ZController LastController;
#endif
	}
}
