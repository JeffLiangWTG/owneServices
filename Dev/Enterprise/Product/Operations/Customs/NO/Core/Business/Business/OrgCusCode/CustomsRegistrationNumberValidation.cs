using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business;

sealed class CustomsRegistrationNumberValidation
{
	internal void CheckGBRCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
	{
		NorwayRegistrationNumberValidator.ValidateGBR(customsRegNoInfo, number, ErrorType);
	}

	internal void CheckMVACustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
	{
		NorwayRegistrationNumberValidator.ValidateMVA(customsRegNoInfo, number, ErrorType);
	}

	internal void CheckEMDCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
	{
		NorwayRegistrationNumberValidator.ValidateEMD(customsRegNoInfo, number, WarningOrMessageError);
	}

	INotificationType ErrorType => CargoWise.ComponentModel.NotificationType.Error;

	INotificationType WarningOrMessageError => CargoWise.ComponentModel.NotificationType.Warning;
}
