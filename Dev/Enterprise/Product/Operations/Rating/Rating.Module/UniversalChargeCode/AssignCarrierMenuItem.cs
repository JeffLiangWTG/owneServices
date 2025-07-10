using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using WiseRates.Constants;

namespace Enterprise.Rating.Module;

public class AssignCarrierMenuItem
{
	ZGrid grid;
	readonly BusinessObjectFactory factory = new ();
	readonly ZMenuItem menuItem;

	CarrierChargeCodeBizo Selected => grid?.GetCurrent() as CarrierChargeCodeBizo;
	string SelectedCarrierCode => Selected?.UCC_Carrier ?? "";

	string SelectedRateProvider => Selected?.UCC_RateProvider;

	public AssignCarrierMenuItem() => menuItem = new ZMenuItem(GetCaption(), OnClick);

	public void Attach(ZGrid grid)
	{
		this.grid = grid;
		var contextMenu = grid.ContextMenu;
		contextMenu.MenuItems.Add(menuItem);
		contextMenu.Popup += OnPopup;
	}

	void OnPopup(object sender, EventArgs e)
	{
		menuItem.Caption = GetCaption();
		menuItem.Enabled = MenuItemEnableCheck();
	}

	void OnClick(object sender, EventArgs e)
	{
		if (SelectedRateProvider == WRConstants.RateProviders.CargoSphere)
		{
			new AssignCarrierCodeCommand().Assign(SelectedCarrierCode, string.Empty);
		}
		else if (SelectedRateProvider == WRConstants.RateProviders.CargoGuide)
		{
			CreateAirlineAndAssignIATACodeCommand.AssignWithUserConfirmation(SelectedCarrierCode, null);
		}
	}

	MultilingualString GetCaption() =>
		SelectedRateProvider switch
		{
			WRConstants.RateProviders.CargoSphere => ResString.GetMultilingualString("ff69baee-9c3d-4e73-9f02-8d697c9beb1b", "Map SCAC/C1C '{0}' to Carrier.", SelectedCarrierCode),
			WRConstants.RateProviders.CargoGuide => ResString.GetMultilingualString("75733589-b216-4bb7-b6e2-e3c6d42cf278", "Map IATA '{0}' to Airline.", SelectedCarrierCode),
			_ => null
		};

	bool MenuItemEnableCheck()
	{
		if (Selected is not null && string.IsNullOrEmpty(SelectedCarrierCode))
		{
			return false;
		}

		return Selected is not null
		&& ((SelectedRateProvider == WRConstants.RateProviders.CargoSphere && !AccChargeCodeUniversalCodeMappingLookups.TryGetCarrierFromScac(factory, SelectedCarrierCode, out var _))
			|| (SelectedRateProvider == WRConstants.RateProviders.CargoGuide && !AccChargeCodeUniversalCodeMappingLookups.TryGetCarrierFromIata(factory, SelectedCarrierCode, out var _)));
	}
}
