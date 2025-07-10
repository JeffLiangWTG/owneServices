using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public abstract class BaseHVLVMenuGroup : ZMenuItem
	{
		public BaseHVLVMenuGroup(MultilingualString caption, ForwardingShipment shipment, bool buildChildrenMenuItemsOnCreation = true)
			: base(caption)
		{
			this.shipment = shipment;
			this.buildChildrenMenuItemsOnCreation = buildChildrenMenuItemsOnCreation;

			if (this.buildChildrenMenuItemsOnCreation)
			{
				BuildChildrenMenuItems();
			}

			this.Select += HandleSelect;
		}

		protected readonly ForwardingShipment shipment;
		protected readonly bool buildChildrenMenuItemsOnCreation;

		protected abstract void BuildChildrenMenuItems();

		protected override void OnPopup(EventArgs e)
		{
			UpdateVisibilityAndCaption();
			base.OnPopup(e);
		}

		public virtual void UpdateVisibilityAndCaption()
		{
			MenuItems.OfType<BaseHVLVMenuGroup>().ForEach(x => x.UpdateVisibilityAndCaption());
			MenuItems.OfType<BaseHVLVMenuItem>().ForEach(x => x.UpdateVisibilityAndCaption());
		}

		void HandleSelect(Object sender, EventArgs e)
		{
			if (!buildChildrenMenuItemsOnCreation && MenuItems.Count == 0)
			{
				BuildChildrenMenuItems();
			}
		}
	}
}
