using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	internal sealed class MessagingValidationMenu : ZMenuItem
	{
		public MessagingValidationMenu(ZForm form)
			: base(ResString.GetMultilingualString("ad1e5874-c519-4100-9e3d-e0c40326a69b", "Include Validation For"))
		{
			this.form = form;

			strategies = MessagingValidationStrategyFactory.GetStrategies();
			items = new KMenuItem[strategies.Length];

			for (int i = 0; i < items.Length; i++)
			{
				items[i] = NewMenuItem(strategies[i]);
			}

			MenuItems.AddRange(items);
		}

		protected override void OnPopup(EventArgs e)
		{
			IBusiness businessEntity;
			BusinessObjectFactory factory;

			if (form != null && (businessEntity = form.BusinessEntity) != null && (factory = businessEntity.Factory) != null)
			{
				for (int i = 0; i < items.Length; i++)
				{
					items[i].Checked = strategies[i].IsRegistered(factory);
				}
			}
			else
			{
				for (int i = 0; i < items.Length; i++)
				{
					items[i].Checked = false;
				}
			}

			base.OnPopup(e);
		}

		KMenuItem NewMenuItem(MessageValidationStrategy strategy)
		{
			KMenuItem item = null;

			item = new ZMenuItem(strategy.Name, delegate
			{ Toggle(item, strategy); });
			return item;
		}

		void Toggle(KMenuItem item, MessageValidationStrategy strategy)
		{
			IBusiness businessEntity;
			BusinessObjectFactory factory;

			if (form != null && (businessEntity = form.BusinessEntity) != null && (factory = businessEntity.Factory) != null)
			{
				if (item.Checked)
				{
					strategy.Unregister(factory);
					strategy.IsEnabled = false;
				}
				else
				{
					strategy.Register(factory);
					strategy.IsEnabled = true;
				}

				MessagingValidationStrategyFactory.SaveCustomiseStrategies();
				form.ValidateChildren();
			}
		}

		readonly ZForm form;
		readonly KMenuItem[] items;
		readonly MessageValidationStrategy[] strategies;
	}
}


