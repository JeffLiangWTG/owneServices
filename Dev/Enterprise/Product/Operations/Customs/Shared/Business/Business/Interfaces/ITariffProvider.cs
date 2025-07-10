using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ITariffProvider
	{
		ZString Tariff { get; }
		ZPropertyInfo TariffInfo { get; }
	}
}
