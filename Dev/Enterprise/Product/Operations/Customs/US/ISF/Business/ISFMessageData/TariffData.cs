using CargoWise.Types;

namespace Enterprise.Customs.US.ISF.Business
{
	class TariffData : ITariffData
	{
		public override int GetHashCode()
		{
			return CountryOfOrigin.GetHashCode() + HarmonizedTariffNumber.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			TariffData data = obj as TariffData;
			return data != null && data.CountryOfOrigin == CountryOfOrigin && data.HarmonizedTariffNumber == HarmonizedTariffNumber;
		}

		#region ITariffData Members

		public ZString CountryOfOrigin
		{
			get { return countryOfOrigin; }
			set { countryOfOrigin = value; }
		}
		ZString countryOfOrigin;

		public ZString HarmonizedTariffNumber
		{
			get { return harmonizedTariffNumber; }
			set { harmonizedTariffNumber = value; }
		}
		ZString harmonizedTariffNumber;

		#endregion
	}
}
