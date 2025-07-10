
namespace Enterprise.Customs.US.Business
{
	static class AssociationsOfCountries
	{
		internal enum Agreement { None, Cartagena, ASEAN, CARICOM, WAEMU, SADC, SAARC }

		internal static Agreement GetAgreement(string countryCode)
		{
			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Bolivia:
				case Core.Constants.CountryCodes.Colombia:
				case Core.Constants.CountryCodes.Ecuador:
				case Core.Constants.CountryCodes.Peru:
				case Core.Constants.CountryCodes.Venezuela:
					return Agreement.Cartagena;

				case Core.Constants.CountryCodes.Cambodia:
				case Core.Constants.CountryCodes.Indonesia:
				case Core.Constants.CountryCodes.Philippines:
				case Core.Constants.CountryCodes.Thailand:
					return Agreement.ASEAN;

				case Core.Constants.CountryCodes.Belize:
				case Core.Constants.CountryCodes.Dominica:
				case Core.Constants.CountryCodes.Grenada:
				case Core.Constants.CountryCodes.Guyana:
				case Core.Constants.CountryCodes.Jamaica:
				case Core.Constants.CountryCodes.Montserrat:
				case Core.Constants.CountryCodes.SaintKittsAndNevis:
				case Core.Constants.CountryCodes.SaintLucia:
				case Core.Constants.CountryCodes.SaintVincentAndTheGrenadin:
				case Core.Constants.CountryCodes.TrinidadAndTobago:
					return Agreement.CARICOM;

				case Core.Constants.CountryCodes.Benin:
				case Core.Constants.CountryCodes.BurkinaFaso:
				case Core.Constants.CountryCodes.CoteDivoire:
				case Core.Constants.CountryCodes.GuineaBissau:
				case Core.Constants.CountryCodes.Mali:
				case Core.Constants.CountryCodes.Niger:
				case Core.Constants.CountryCodes.Senegal:
				case Core.Constants.CountryCodes.Togo:
					return Agreement.WAEMU;

				case Core.Constants.CountryCodes.Botswana:
				case Core.Constants.CountryCodes.Mauritius:
				case Core.Constants.CountryCodes.Tanzania:
					return Agreement.SADC;

				case Core.Constants.CountryCodes.Bangladesh:
				case Core.Constants.CountryCodes.Bhutan:
				case Core.Constants.CountryCodes.India:
				case Core.Constants.CountryCodes.Nepal:
				case Core.Constants.CountryCodes.Pakistan:
				case Core.Constants.CountryCodes.SriLanka:
					return Agreement.SAARC;

				default:
					return Agreement.None;
			}
		}
	}
}
