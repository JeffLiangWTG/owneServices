using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.PL.GUI;

public class CarModelModulePopup : EmbeddedModulePopup
{
	public CarModelModulePopup(JobComInvoiceLine parentInvoiceLine, ZFilterGridModule module)
		: base(module)
	{
		this.invoiceLine = parentInvoiceLine;
	}

	readonly JobComInvoiceLine invoiceLine;

	protected override void HandleSelection(BusinessObject[] selectedBizObjs)
	{
		if (selectedBizObjs != null && selectedBizObjs.Length == 1 && selectedBizObjs[0] is ZZRefCusCodeListCombined selected)
		{
			invoiceLine.JI_MarkModel = selected.ZZD_Description;
		}

		Dispose();
	}
}
