using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator
{
	public static class EUNDescriptionMapping
	{
		private static SortedDictionary<string, EUNMapping> EUNMappings = new SortedDictionary<string, EUNMapping>
		{
			{"AIRWORTHINESS TARIFF SUSPENSION",new EUNMapping() { TariffType= "IMP",  MeasureType="119", Preference=new[]{"119" }} }
			,{"AUTONOMOUS SUSPENSION UNDER END-USE",new EUNMapping() { TariffType= "IMP",  MeasureType="115", Preference=new[]{"115" }} }
			,{"AUTONOMOUS TARIFF SUSPENSION",new EUNMapping() { TariffType= "IMP",  MeasureType="112", Preference=new [] {"110"} }}
			,{"CUSTOMS UNION DUTY",new EUNMapping() { TariffType= "IMP",  MeasureType="106", Preference=new[]{"400" } }}
			,{"CUSTOMS UNION QUOTA",new EUNMapping() { TariffType= "IMP",  MeasureType="147", Preference=new[]{"420" }} }
			,{"DECLARATION OF SUBHEADING SUBMITTED TO END-USE PROVISIONS",new EUNMapping() { TariffType= "IMP",  MeasureType="464", Preference=new [] {""}, HasAdditionalRule=true }}
			,{"NON PREFERENTIAL DUTY UNDER END-USE",new EUNMapping() { TariffType= "IMP",  MeasureType="105", Preference=new[]{"140" }} }
			,{"NON PREFERENTIAL TARIFF QUOTA",new EUNMapping() { TariffType= "IMP",  MeasureType="122", Preference=new [] {"120","125","128"} }}
			,{"NON PREFERENTIAL TARIFF QUOTA UNDER END-USE",new EUNMapping() { TariffType= "IMP",  MeasureType="123", Preference=new [] {"123"} }}
			,{"PREFERENCE UNDER END-USE",new EUNMapping() { TariffType= "IMP",  MeasureType="145", Preference=new [] {""} }}
			,{"PREFERENTIAL SUSPENSION",new EUNMapping() { TariffType= "IMP",  MeasureType="141", Preference=new [] {"310"} }}
			,{"PREFERENTIAL TARIFF QUOTA",new EUNMapping() { TariffType= "IMP",  MeasureType="143", Preference=new [] {"320","325"}, HasAdditionalRule=true }}
			,{"PREFERENTIAL TARIFF QUOTA UNDER END-USE",new EUNMapping() { TariffType= "IMP",  MeasureType="146", Preference=new [] {"223","323"} }}
			,{"SUSPENSION - GOODS FOR CERTAIN CATEGORIES OF SHIPS, BOATS AND OTHER VESSELS AND FOR DRILLING OR PRODUCTION PLATFORMS",new EUNMapping() { TariffType= "IMP",  MeasureType="117", Preference=new [] {"140"} }}
			,{"TARIFF PREFERENCE",new EUNMapping() { TariffType= "IMP",  MeasureType="142", Preference=new [] {"200","300"} }}
			,{"THIRD COUNTRY DUTY",new EUNMapping() { TariffType= "IMP",  MeasureType="103", Preference=new [] {"100"} }}
		};

		public static EUNMapping GetEUNMapping(string description)
		{
			Argument.NotNull(description, nameof(description));
			EUNMappings.TryGetValue(description.ToUpper(), out EUNMapping eunMapping);
			return eunMapping;
		}
	}
}
