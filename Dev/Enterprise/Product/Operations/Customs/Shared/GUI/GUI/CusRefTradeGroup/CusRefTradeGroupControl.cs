using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusRefTradeGroupControl : ZUserControl
	{
		public CusRefTradeGroupControl()
		{
			InitializeComponent();
			TradeGroupCountryGrid.AfterBind -= TradeGroupCountryGrid_AfterBind;
			TradeGroupCountryGrid.AfterBind += TradeGroupCountryGrid_AfterBind;
		}

		void TradeGroupCountryGrid_AfterBind(object sender, EventArgs e)
		{
			if (sender is ZGrid tradeGroupCountryGrid && tradeGroupCountryGrid.Columns[CusRefTradeGroupCountry.Schema.CRA_RN_NKTradeGroupCountryCode]?.ColumnStyle is ZCodeFindBoxColumnStyle tradeGroupCountryCodeFindBoxColumnStyle)
			{
				tradeGroupCountryCodeFindBoxColumnStyle.PopupSelected -= TradeGroupCountryCodeFindBoxColumnStyle_PopupSelected;
				tradeGroupCountryCodeFindBoxColumnStyle.PopupSelected += TradeGroupCountryCodeFindBoxColumnStyle_PopupSelected;
			}
		}

		void TradeGroupCountryCodeFindBoxColumnStyle_PopupSelected(object sender, ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs e)
		{
			var tradeGroupCountries = e.SelectedBusinessObjects;
			if (tradeGroupCountries.Length > 1 && TradeGroupCountryGrid.GetCurrent() is CusRefTradeGroupCountry tradeGroupCountry)
			{
				var list = TradeGroupCountryGrid.List;
				var existingList = list.OfType<CusRefTradeGroupCountry>()
					.Where(x => x.PK != tradeGroupCountry.PK)
					.Select(x => x.CRA_RN_NKTradeGroupCountryCode).Distinct().ToHashSet();
				var needToAddList = tradeGroupCountries.Skip(1).OfType<ICodeDescription>()
					.Where(x => !existingList.Contains((ZString)x.Code)).ToArray();
				if (needToAddList.Length > 0)
				{
					var collection = (CusRefTradeGroupCountryCollection)list;
					if (tradeGroupCountry.IsRowDeletedOrDetachedOrNull)
					{
						((ICancelAddNew)collection).EndNew(collection.IndexOf(tradeGroupCountry)); // need to commit this row as adding new records will cause incorrect refresh binding
					}
					TradeGroup.OnTradeGroupCountriesSelected(needToAddList);
				}
			}
		}

		protected CusRefTradeGroup TradeGroup => (CusRefTradeGroup)CurrentDataItem;
	}
}
