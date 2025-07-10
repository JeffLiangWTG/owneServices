using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public sealed class UnlocoHelper : IUnlocoHelper
	{
		public UnlocoHelper(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		readonly BusinessObjectFactory factory;

		public string GetCityCountry(string unlocoCode)
		{
			var result = new ZStringBuilder();

			var unloco = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, unlocoCode);
			if (unloco != null)
			{
				if (!unloco.RL_PortName.IsEmpty)
				{
					result.Append(unloco.RL_PortName);
				}

				var country = unloco.Country;
				if (country != null)
				{
					if (country.RN_StateProvinceValidationRule == CountryAddressValidationRuleList.Codes.MustBeEntered)
					{
						var state = factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.PK, unloco.RL_RW));
						if (state != null)
						{
							result.Append(state.RW_Code);
						}
					}

					result.Append(country.RN_Code);
				}
			}
			else
			{
				return unlocoCode;
			}

			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		public bool IsUnloco(string unlocoCode)
		{
			return !string.IsNullOrWhiteSpace(unlocoCode)
				&& factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, unlocoCode) != null;
		}
	}
}
