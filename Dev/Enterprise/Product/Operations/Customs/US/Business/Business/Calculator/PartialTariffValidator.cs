using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class PartialTariffValidator
	{
		public static bool IsValidPartialTariff(BusinessObjectFactory factory, ZString tariffNumber)
		{
			return factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, tariffNumber))
				!= null;
		}
	}
}
