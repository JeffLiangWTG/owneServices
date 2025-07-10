using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Customs.PL.GUI;

public class CarModelFindBox : IFindBox
{
	public CarModelFindBox(JobComInvoiceLine parentInvoiceLine, ZForm parentForm)
	{
		this.parentInvoiceLine = parentInvoiceLine;
		allCarModels = this.parentInvoiceLine.Lookups.PLMarkModelCollection;
		this.parentForm = parentForm;
	}

	readonly JobComInvoiceLine parentInvoiceLine;
	internal readonly ZZRefCusCodeListCombinedCollection allCarModels;
	readonly ZForm parentForm;

	EmbeddedModulePopup popup;

	public string Code { get; set; }

	public string Description { get; set; }

	public IFindBoxListProvider ListProvider => allCarModels;

	public IFindBoxPopup PopupForm => popup;

	public void ShowModule()
	{
		var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.Universal.ZZRefCusCodeList);
		if (module.SecurityCheckpoint.IsAllowed)
		{
			module.OverrideModuleDecisionProvider(new PopupModuleDecisionProvider(this));
			popup = new CarModelModulePopup(parentInvoiceLine, module);
			Code = parentInvoiceLine.MarkModelCode;
			Description = parentInvoiceLine.JI_MarkModel;
			((ZFilterStripCommonControl)module.EmbeddedControl).RunSearchOnEnteringAModuleOverride = !Code.IsNullOrEmpty() || !Description.IsNullOrEmpty();
			popup.ShowModal(this, parentForm);
			popup.FocusFirstRecord();
		}
		else
		{
			module.SecurityCheckpoint.ShowError();
			module.Dispose();
		}
	}
}
