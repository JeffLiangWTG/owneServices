using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	internal class TariffOutput
	{
		public string Tariff { get; set; }
		public string TariffDescription { get; set; }
		public string CompositeKey { get; set; }
		public bool AllowOverride { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is TariffOutput tariff) {
				return Tariff == tariff.Tariff &&
				       TariffDescription == tariff.TariffDescription &&
				       CompositeKey == tariff.CompositeKey &&
				       AllowOverride == tariff.AllowOverride;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Tariff, TariffDescription, CompositeKey, AllowOverride);
		}
	}
}
