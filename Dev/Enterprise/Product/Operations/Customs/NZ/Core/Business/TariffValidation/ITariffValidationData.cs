using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NZ.Business.TariffValidation
{
	public interface ITariffValidationData
	{
		ZString TariffCode { get; }
		ZPropertyInfo TariffCodeInfo { get; }
		ITariff TariffBO { get; }
		ZString PartsOfTariffCode { get; }
		ZPropertyInfo PartsOfTariffCodeInfo { get; }
		ITariff PartsOfTariffBO { get; }
		AllowableTariffCodeTypes AllowableTariffCodeTypes { get; }
		int PermitCodeCount { get; }
		BusinessObjectFactory Factory { get; }
		bool EmptyTariffIsFullError { get; }
		ZDateTime DateForDutyRate { get; }
		bool EmptyTariffIsAllowed { get; }
		void ValidateTariffCode();
		void ValidatePartsOfTariffCode();
	}
}
