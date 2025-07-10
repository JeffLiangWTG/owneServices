using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC044CGoodsMeasureProvider : GoodsMeasureProvider
{
	public CC044CGoodsMeasureProvider(NctsCommonCargoDesc item) : base(item)
	{
	}

	public override decimal GrossMassMeasure => item is NctsUnloadedCargoDesc unloadedItem && unloadedItem.ArrivalCargoDescParent != null && unloadedItem.ArrivalCargoDescParent.GrossMassInKilograms != unloadedItem.GrossMassInKilograms
				? unloadedItem.GrossMassInKilograms
				: (item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW ? item.GrossMassInKilograms : decimal.Zero);

	public override decimal NetNetWeightMeasure => item is NctsUnloadedCargoDesc unloadedItem && unloadedItem.ArrivalCargoDescParent != null && unloadedItem.ArrivalCargoDescParent.NetMassInKilograms != unloadedItem.NetMassInKilograms
											? (NctsDataRetrieveMethods.NetMassInKilogramsNullableByPreviousDocument(unloadedItem)) ?? decimal.Zero
											: (item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW ? item.NetMassInKilograms : decimal.Zero);
}
