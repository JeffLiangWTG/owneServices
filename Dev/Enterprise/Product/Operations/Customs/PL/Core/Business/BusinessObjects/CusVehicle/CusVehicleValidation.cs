using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class CusVehicleValidation : EU.Business.CusVehicleValidation
{
	public CusVehicleValidation(AutoCusVehicle parent) : base(parent)
	{
	}

	public new CusVehicle Parent => (CusVehicle)base.Parent;

	protected override void CheckCVH_ModelYear()
	{
		base.CheckCVH_ModelYear();
		if (!Parent.CVH_ModelYear.IsEmpty)
		{
			var modelYearParsed = ZInt.TryParse(Parent.CVH_ModelYear, out var modelYear);
			if (!modelYearParsed || modelYear <= 1900)
			{
				Parent.CVH_ModelYearInfo.AddError(Res.GetString("PLCusVehicleValidation|InvalidDateFormat", "Invalid year entered. Valid year must be greater than 1900."));
			}
			else if (modelYear > ZDateTime.Now.Year)
			{
				Parent.CVH_ModelYearInfo.AddWarning(Res.GetString("PLCusVehicleValidation|DateInTheFuture", "Date is in the future."));
			}
		}
		else if (Parent.IsCarDetailsDataRequired)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CVH_ModelYearInfo);
		}
	}

	protected override void CheckCVH_VehicleIdentificationNumber()
	{
		base.CheckCVH_VehicleIdentificationNumber();
		if (Parent.IsCarDetailsDataRequired)
		{
			MandatoryValidation.WarnIfNotEntered(Parent.CVH_VehicleIdentificationNumberInfo);
			if (Parent.CVH_VehicleIdentificationNumber.IsEmpty)
			{
				Parent.CVH_VehicleIdentificationNumberInfo.AddWarning(Res.GetString("PLCusEngineValidation|CheckCVH_VehicleIdentificationNumber|placeholderWillBeUsed", "'{0}' will be used in the XML message.", Constants.CarInformationEmptyElementPlaceholder));
			}

			CheckRuleR432();
		}
	}

	void CheckRuleR432()
	{
		if (IsImportDeclaration && Parent.CVH_VehicleIdentificationNumber.ContainsAnyChar("IOQ"))
		{
			Parent.CVH_VehicleIdentificationNumberInfo.AddMessageError(Res.GetString("PLCusEngineValidation|CheckCVH_VehicleIdentificationNumber|InvalidVIN", "(R432) Invalid VIN number (I, O, Q letters are forbidden)"));
		}
	}

	bool IsImportDeclaration => Parent.InvoiceLine?.Declaration?.IsImport ?? false;
}
