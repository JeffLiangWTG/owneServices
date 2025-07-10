using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCVisaTariffCollection : DependentBusinessObjectCollection<USCVisaTariff, USCVisa>
	{
		public USCVisaTariffCollection(USCVisa visa)
			: base(visa)
		{
		}

		public USCVisaTariff GetOrCreateFor(ZString tariff)
		{
			USCVisaTariff finalResult = Find(tariff);

			if (finalResult == null)
			{
				finalResult = AddNew();
				finalResult.UK_Tariff = tariff;
			}

			return finalResult;
		}

		public USCVisaTariff Find(ZString tariff)
		{
			USCVisaTariff[] result = (USCVisaTariff[])Find(new ZQuery(USCVisaTariffSchema.UK_Tariff, tariff));
			return result.Length > 0 ? result[0] : null;
		}
	}
}
