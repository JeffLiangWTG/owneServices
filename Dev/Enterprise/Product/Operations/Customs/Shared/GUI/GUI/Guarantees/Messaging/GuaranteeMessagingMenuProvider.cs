using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class GuaranteeMessagingMenuProvider
	{
		public static GuaranteeMessagingMenuProvider GetProvider(BaseCusGuaranteeHeader header)
		{
			var messagingMenuProviders = ObjectFactory.Get<Hashtable>("GuaranteeMessagingMenuProviders");
			var messagingMenuProviderHandle = (ObjectHandle)messagingMenuProviders[header.CPH_RN_NKCountryCode.ToString()];
			return (GuaranteeMessagingMenuProvider)messagingMenuProviderHandle?.GetObject(header) ?? new GuaranteeMessagingMenuProvider(header);
		}

		public GuaranteeMessagingMenuProvider(BaseCusGuaranteeHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		public IEnumerable<ZMenuItem> CreateMenuItems()
		{
			foreach (var menuItem in CreateMenuItemsCore())
			{
				yield return menuItem;
			}
		}

		protected virtual IEnumerable<ZMenuItem> CreateMenuItemsCore()
		{
			return new List<ZMenuItem>();
		}

		public virtual void RefreshMenu()
		{
		}

		protected void SetMenuItemVisibility(ZMenuItem menuItem, Func<bool> isVisible)
		{
			if (menuItem != null)
			{
				menuItem.Visible = isVisible();
			}
		}

		protected BaseCusGuaranteeHeader Header { get; }
	}
}
