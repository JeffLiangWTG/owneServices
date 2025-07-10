using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public class CusEngineValidation : Customs.Business.CusEngineValidation
{
	public CusEngineValidation(CusEngine parent) : base(parent)
	{
	}

	public new CusEngine Parent => (CusEngine)base.Parent;

	ZBool IsCarDetailsDataRequired => Parent.Vehicle?.IsCarDetailsDataRequired ?? false;

	protected override void CheckCEG_EngineType()
	{
		base.CheckCEG_EngineType();
		if (!Parent.CEG_EngineType.IsEmpty || IsCarDetailsDataRequired)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEG_EngineTypeInfo, Parent.Lookups.FuelTypeList);
		}
	}

	protected override void CheckCEG_EngineNumber()
	{
		base.CheckCEG_EngineNumber();
		if (IsCarDetailsDataRequired)
		{
			MandatoryValidation.WarnIfNotEntered(Parent.CEG_EngineNumberInfo);
			if (Parent.CEG_EngineNumber.IsEmpty)
			{
				Parent.CEG_EngineNumberInfo.AddWarning(Res.GetString("PLCusEngineValidation|CheckCEG_EngineNumber|placeholderWillBeUsed", "'{0}' will be used in the XML message.", Constants.CarInformationEmptyElementPlaceholder));
			}
		}
	}

	protected override void CheckCEG_CapacityCC()
	{
		base.CheckCEG_CapacityCC();
		MandatoryValidation.CheckNotNegative(Parent.CEG_CapacityCCInfo);
	}
}
