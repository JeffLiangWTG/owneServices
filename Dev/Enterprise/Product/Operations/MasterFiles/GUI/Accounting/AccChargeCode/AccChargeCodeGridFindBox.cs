using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI;

public class AccChargeCodeGridFindBox : ZGridFindBox
{
	ZGrid Grid => Parent as ZGrid;

	AccChargeCodeUniversalCodeMapping CurrentDataSource => Grid?.DataSource is null
		? null
		: (AccChargeCodeUniversalCodeMapping)BindingContext[Grid.DataSource, Grid.DataMember].GetCurrent();

	protected override IFindBoxPopup GetNewPopupForm()
	{
		if (CurrentDataSource != null)
		{
			ModuleID = CurrentDataSource.AUP_Type == AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Carrier
				? ModuleIDs.CarrierChargeCode
				: ModuleIDs.UniversalChargeCode;
		}

		var module = NewModuleFromModuleID();
		if (module is null)
		{
			return null;
		}

		module.OverrideModuleDecisionProvider(GetModuleDecisionProvider(module));
		module.FilterBusinessObject.ParentModuleID = ParentModuleID;
		module.FilterBusinessObject.ParentType = ParentType;
		return CreateEmbeddedPopup(module);
	}

	protected override void OnPopupSelected(IFindBoxPopup popup, BusinessObject[] selectedBusinessObjects)
	{
		base.OnPopupSelected(popup, selectedBusinessObjects);

		if (CurrentDataSource is not null)
		{
			CurrentDataSource.UpdatePropertiesFromChargeCode((MappedChargeCodeBizo)selectedBusinessObjects[0]);
		}
	}
}
