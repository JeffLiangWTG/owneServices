using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface ITariffData
	{
		ZGuid PK { get; }
		bool IsNomenclatureGroup { get; }
		ZString CompositeKey { get; }
		ZString TariffCode { get; }
		ZString GetDescription(ZString languageCode);
	}

	public class TariffData : NonPersistentBusinessObject, ITariffData
	{
		public static class Schema
		{
			public const string Description = "Description";
		}

		public ZString CompositeKey { get; set; }
		public ZString Description { get; set; }
		ZString ITariffData.GetDescription(ZString languageCode) => Description;
		public bool IsNomenclatureGroup { get; set; }
		public ZString TariffCode { get { return Other; } }
		const string Other = "OTHER";
	}
}
