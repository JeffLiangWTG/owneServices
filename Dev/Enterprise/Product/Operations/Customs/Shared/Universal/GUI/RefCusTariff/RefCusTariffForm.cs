using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable SA1508 // Closing braces should not be preceded by blank line

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCusTariffForm : ZForm
	{
		RefCusTariffUserControl refCusTariffUserControl1;
		Core.Forms.ZPostingButtonsUserControl zPostingButtonsUserControl2;

		public RefCusTariffForm(TariffView dataSource) : base(dataSource)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, zPostingButtonsUserControl2);
			MainMenu.MenuItems.Add(1, new IncludeGeneralTariffsMenuItem(dataSource.ChildTariffs));
		}

		public new TariffView BusinessEntity => (TariffView)base.BusinessEntity;

		protected override void SetReadOnlyIncludingChildren()
		{
			this.SetReadOnlyIncludingChildren(new List<string>(new[]
			{
				"EffectiveDateDateEdit", "EffectiveDataGroupingDropEdit", "RatesApplyToCountryFindBox"
			}));
		}

		public override string FormCaption
		{
			get { return BusinessEntity?.HumanReadableName ?? Res.GetString("RefCusTariffForm|FormCaption", "Global Tariffs"); }
		}

		protected override void SaveToRecentItems()
		{
		}
	}

	public class IncludeGeneralTariffsMenuItem : ZMenuItem
	{
		public IncludeGeneralTariffsMenuItem(TariffRelationshipViewCollection dataSource) : base(ResString.GetMultilingualString("A1C79E94-B4AD-4866-9479-C099B4CCF3DA", "&View"))
		{
			collectionView = dataSource;
			AddIncludeGeneralChildTariffsMenuItem(this);
		}

		readonly TariffRelationshipViewCollection collectionView;

		public void AddIncludeGeneralChildTariffsMenuItem(MenuItem parentMenu)
		{
			var item = new ZMenuItem(ResString.GetMultilingualString("279C9A1F-CDE9-48CA-9F99-FC2F0C8BDF91", "&Include General Child Tariffs"), OnClick);
			parentMenu.Popup += (sender, args) => ParentMenu_Popup(item);
			parentMenu.MenuItems.Add(item);
		}

		void OnClick(object sender, EventArgs e)
		{
			base.OnClick(e);
			collectionView.ExcludeGeneralTariffs = !collectionView.ExcludeGeneralTariffs;
#if WINZOR
			ParentMenu_Popup(sender);
#endif
		}

		void ParentMenu_Popup(object clickedItem)
		{
			if (collectionView != null)
			{
				var item = clickedItem as ZMenuItem;
				if (item != null)
				{
					item.Checked = !collectionView.ExcludeGeneralTariffs;
				}
				else
				{
					var item2 = clickedItem as ZToolStripMenuItem;
					if (item2 != null)
					{
						item2.Checked = !collectionView.ExcludeGeneralTariffs;
					}
				}
			}
		}
	}
}
