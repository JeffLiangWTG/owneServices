using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Internal
{
	public class DocAddressGrid : ZGrid
	{
		public DocAddressGrid()
		{
			ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			ContextMenu.Popup += new EventHandler(ContextMenu_Popup);
		}

		#region Context Menu Popup

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			RemoveAddressMenuItems();
			AddAddressMenuItems();
		}

		void RemoveAddressMenuItems()
		{
			for (int i = ContextMenu.MenuItems.Count - 1; i >= 0; i--)
			{
				MenuItem menuItem = ContextMenu.MenuItems[i];

				if (AddressMenuItems.Contains(menuItem))
				{
					ContextMenu.MenuItems.Remove(menuItem);
					menuItem.Dispose();
				}
			}

			AddressMenuItems.Clear();
		}

		void AddAddressMenuItems()
		{
			IDocAddresses host = ParentPlugIn.Host;

			foreach (DocAddressType supportedAddressType in host.SupportedAddressTypes)
			{
				bool maxExceeded = false;
				string addressDescription = DocAddressTypes.GetPair(ParentPlugIn.Factory, supportedAddressType).Description;

				//warning IDocAddressesObsolete will be removed when Proxying DocAddresses is fixed!
				JobDocAddressRequirement requirement = ParentPlugIn.Host is IDocAddressesObsolete ? null : host.GetDocAddressRequirement(supportedAddressType);
				int max = requirement != null ? requirement.DefaultMax : 1;

				if (max > 0)
				{
					int count = 0;
					foreach (JobDocAddress address in GridCollection)
					{
						if (address.DocAddressType == supportedAddressType)
						{
							count++;
						}
					}

					maxExceeded = count >= max;
				}

				string menuText = !maxExceeded  ? Res.GetString("DocAddressGrid|Add", "Add {0}", addressDescription) : Res.GetString("DocAddressGrid|AlreadyExists", "{0} already exists", addressDescription);
				MenuItem menuItem = new ZMenuItem(menuText, new EventHandler(OnAddAddressMenuClick));
				menuItem.Enabled = !maxExceeded && !((BusinessObject)ParentPlugIn.Host).ReadOnly;
				menuItem.Tag = supportedAddressType;
				ContextMenu.MenuItems.Add(menuItem);
				AddressMenuItems.Add(menuItem);
			}
		}

		List<MenuItem> AddressMenuItems
		{
			get
			{
				if (fAddressMenuItems == null)
				{
					fAddressMenuItems = new List<MenuItem>();
				}
				return fAddressMenuItems;
			}
		}

		List<MenuItem> fAddressMenuItems;

		#endregion

		#region Context Menu 'Add Address' click

		void OnAddAddressMenuClick(object sender, EventArgs e)
		{
			MenuItem menuItem = (MenuItem)sender;
			DocAddressType addressType = (DocAddressType)menuItem.Tag;

			AddAddress(addressType);
		}

		void AddAddress(DocAddressType docAddressType)
		{
			JobDocAddress addressToAdd = null;

			//To remove when change over to LocalCartage 2.0 (1.0 uses the docaddresses from it's parent - Naughty!)
			IDocAddressesObsolete obsoleteParent = ParentPlugIn.Host as IDocAddressesObsolete;
			if (obsoleteParent != null)
			{
				addressToAdd = obsoleteParent.GetDocAddress(docAddressType);
			}
			else
			{
				JobDocAddressRequirement requirement = ParentPlugIn.Host.GetDocAddressRequirement(docAddressType);
				if (requirement == null)
				{
					addressToAdd = ParentPlugIn.Host.DocAddresses.FindOrCreateWithDocAddressType(docAddressType);
				}
				else
				{
					if (requirement.DefaultMax == 1)
					{
						addressToAdd = ParentPlugIn.Host.DocAddresses.FindOrCreateWithRequirement(requirement);
					}
					else
					{
						JobDocAddress[] addressesWithType = ParentPlugIn.Host.DocAddresses.FindDocAddressesByType(docAddressType);

						foreach (JobDocAddress docAddress in addressesWithType)
						{
							if (!GridCollection.Contains(docAddress) || docAddress.IsEmpty)
							{
								addressToAdd = docAddress;
								break;
							}
						}

						if (addressToAdd == null)
						{
							if (requirement.DefaultMax == 0 || addressesWithType.Length < requirement.DefaultMax)
							{
								addressToAdd = ParentPlugIn.Host.DocAddresses.CreateWithRequirement(requirement);
							}
							else
							{
								throw new NotSupportedException("Too many DocAddresses with this DocAddressType:" + DocAddressTypes.GetCode(ParentPlugIn.Factory, docAddressType));
							}
						}
					}
				}
			}

			if (addressToAdd != null)
			{
				GridCollection.Add(addressToAdd);
				ListManager.Position = ListManager.List.IndexOf(addressToAdd);
				OnAddressAdded(addressToAdd);
				ParentPlugIn.Host?.DocAddresses?.RaiseDocAddressCollectionChanged(addressToAdd.DocAddressType);
			}
		}

		void OnAddressAdded(JobDocAddress address)
		{
			if (AddressAdded != null)
			{
				AddressAdded(new AddAddressEventArgs(address));
			}
		}

		JobDocAddressCollectionForPlugin GridCollection
		{
			get { return (JobDocAddressCollectionForPlugin)List; }
		}

		#region AddressAdded event

		public event AddressAddedEventHandler AddressAdded;

		public delegate void AddressAddedEventHandler(AddAddressEventArgs e);

		public class AddAddressEventArgs : EventArgs
		{
			public AddAddressEventArgs(JobDocAddress address)
			{
				this.Address = address;
			}

			public readonly JobDocAddress Address;
		}

		#endregion

		#endregion

		#region Refresh Addresses

		public void RefreshAddresses()
		{
			ParentPlugIn.RefreshAddresses();
		}

		#endregion

		#region Parent PlugIn + TabPage

		DocAddressesPlugIn ParentPlugIn
		{
			get { return (DocAddressesPlugIn)ParentTabPage.PlugIn; }
		}

		ZTabPagePlugIn ParentTabPage
		{
			get
			{
				ZTabPagePlugIn result = null;
				Control parentControl = Parent;

				while (result == null && parentControl != null)
				{
					parentControl = parentControl.Parent;
					result = parentControl as ZTabPagePlugIn;
				}

				return result;
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (fAddressMenuItems != null)
				{
					fAddressMenuItems.Clear();
				}
			}
		}

		#endregion
	}
}
