using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class TopLevelPacksTotalsControl : ZUserControl
	{
		public TopLevelPacksTotalsControl()
		{
			InitializeComponent();
		}

		#region Implementation

		#region Hooks For Totals Update

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Shipment != null)
			{
				UnHookTopLevelPacks();
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Shipment != null)
			{
				HookTopLevelPacks();
			}
		}

		AgencyShipment Shipment
		{
			get { return (AgencyShipment)CurrentDataItem; }
		}

		#endregion

		#region Totals

		#region UpdateTotals

		void HookTopLevelPacks()
		{
			Shipment.TopLevelPacks.CountChanged += TopLevelPacks_CountChanged;
			foreach (AgencyShipmentContainer topLevelPack in Shipment.TopLevelPacks)
			{
				HookTopLevelPack(topLevelPack);
			}
		}

		void TopLevelPacks_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				HookTopLevelPack((AgencyShipmentContainer)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				UnHookTopLevelPack((AgencyShipmentContainer)e.BizObject);
			}

			UpdateTotals();
		}

		void UnHookTopLevelPacks()
		{
			Shipment.TopLevelPacks.CountChanged -= TopLevelPacks_CountChanged;
			foreach (AgencyShipmentContainer topLevelPack in Shipment.TopLevelPacks)
			{
				UnHookTopLevelPack(topLevelPack);
			}
		}

		void HookTopLevelPack(AgencyShipmentContainer topLevelPack)
		{
			topLevelPack.JC_ContainerCountInfo.ValueChanged += UpdateTotals;
			topLevelPack.JC_GrossVolumeInfo.ValueChanged += UpdateTotals;
			topLevelPack.JC_GrossVolumeUQInfo.ValueChanged += UpdateTotals;
			topLevelPack.JC_GrossWeightInfo.ValueChanged += UpdateTotals;
			topLevelPack.JC_GrossWeightUQInfo.ValueChanged += UpdateTotals;
		}

		void UnHookTopLevelPack(AgencyShipmentContainer topLevelPack)
		{
			topLevelPack.JC_ContainerCountInfo.ValueChanged -= UpdateTotals;
			topLevelPack.JC_GrossVolumeInfo.ValueChanged -= UpdateTotals;
			topLevelPack.JC_GrossVolumeUQInfo.ValueChanged -= UpdateTotals;
			topLevelPack.JC_GrossWeightInfo.ValueChanged -= UpdateTotals;
			topLevelPack.JC_GrossWeightUQInfo.ValueChanged -= UpdateTotals;
		}

		void UpdateTotals(object sender, EventArgs e)
		{
			UpdateTotals();
		}

		void UpdateTotals()
		{
			Shipment.TopLevelPacksTotalPacksInfo.RefreshBinding();
			Shipment.TopLevelPacksTotalVolumeInShipmentVolumeUnitInfo.RefreshBinding();
			Shipment.TopLevelPacksTotalWeightInShipmentWeightUnitInfo.RefreshBinding();
		}

		#endregion

		#endregion

		#endregion
	}
}
