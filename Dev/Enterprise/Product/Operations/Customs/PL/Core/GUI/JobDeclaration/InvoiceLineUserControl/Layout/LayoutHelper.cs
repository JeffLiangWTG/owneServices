using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.PL.GUI;

static class LayoutHelper
{
	internal static ResourceStringData GetSecondQuantityCalcDropEditCaption(JobComInvoiceLine invoiceLine)
	{
		var result = Res.GetData("d4d697f7-e13d-412a-84ae-6f59e30c6873", "Additional Qty 1");

		if (invoiceLine?.UniversalTariff is TariffView tariff && tariff.UnitsOfMeasure.Any(x => x.ZZ8_Type == Constants.UnitOfMeasureTypes.AdditionalUOMType))
		{
			result = Res.GetData("66f5e85c-d32f-4847-8e3b-6b616447f723", "[41] Supp. Qty");
		}
		return result;
	}
}
