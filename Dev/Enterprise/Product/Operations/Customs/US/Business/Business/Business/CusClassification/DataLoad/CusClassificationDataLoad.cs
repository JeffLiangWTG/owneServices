using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class ClassificationDataLoad : ClassificationDataLoad<CusClassification>
	{
		public void ImportClassificationData(string dataLocation)
		{
			ImportData(dataLocation, "Classification");
		}

		protected override CusClassification LoadClassificationIfItExists(ZString lookupCode, ZString type)
		{
			return new CusClassification.Loader(Factory).Load(lookupCode, type);
		}

		protected override IEnumerable<string> GetCountrySpecificFieldNames()
		{
			return new List<string>();
		}

		protected override void ProcessCountrySpecificData(ClassificationDataToLoad dataToLoad, CusClassification classification)
		{
		}

		protected override ZString GetTariffDescription(ZString tariff, ZString type)
		{
			var tariffType = type == CusClassification.ClassificationType.IMP ? Universal.Constants.TariffTypes.HarmonizedSystem : Universal.Constants.TariffTypes.ScheduleB;
			return Factory.GetTariff(tariffType, tariff, ZDateTime.Today)?.Description ?? ZString.Empty;
		}
	}
}
