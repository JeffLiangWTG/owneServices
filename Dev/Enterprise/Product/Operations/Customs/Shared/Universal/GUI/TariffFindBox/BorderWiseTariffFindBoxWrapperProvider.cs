using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.GUI
{
	internal class BorderWiseTariffFindBoxWrapperProvider
	{
		const string ExcludedDataGrouping = "WCO";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static field is a readonly type")]
		static readonly string[] countryCodeOverrideList = { Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes };

		public static IFindBoxPopup GetFindBoxWrapper(BusinessObject dataSource, string countryCode, string cw1TariffType, ZDateTime effectiveDate, string dataGrouping)
		{
			if (dataGrouping.Equals(ExcludedDataGrouping, StringComparison.InvariantCultureIgnoreCase))
			{
				return null;
			}

			var additionalDataForBorderWise = GetAdditionalDataForBorderWise(dataSource, countryCode, cw1TariffType, effectiveDate, dataGrouping);

			if (additionalDataForBorderWise != null)
			{
				return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(additionalDataForBorderWise, null);
			}

			return null;
		}

		public static AdditionalDataForBorderWise GetAdditionalDataForBorderWise(BusinessObject dataSource, string countryCode, string cw1TariffType, ZDateTime effectiveDate, string dataGrouping)
		{
			if (dataSource != null)
			{
				var factory = dataSource.Factory ?? new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseTariffFindBoxWrapperProvider) };

				var tariffTypeMappings = ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(factory, dataGrouping, RefCusMapTypeList.Codes.BORDERWISE, ZDateTime.Now);

				tariffTypeMappings.TryGetValue(cw1TariffType, out var tariffTypeMapping);

				if (string.IsNullOrWhiteSpace(tariffTypeMapping))
				{
					switch (cw1TariffType.ToUpperInvariant())
					{
						case ClassificationTypeList.Codes.Import:
							tariffTypeMapping = "I";
							break;
						case ClassificationTypeList.Codes.Export:
							tariffTypeMapping = "E";
							break;
					}
				}

				var possibleCountryOverride = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, dataGrouping);

				var countryCodeOverride = possibleCountryOverride != null || countryCodeOverrideList.Contains(dataGrouping) ? dataGrouping : string.Empty;

				return AdditionalDataForBorderWise.GetAdditionalDataFrom(tariffTypeMapping, effectiveDate, countryCodeOverride: countryCodeOverride);
			}

			return null;
		}
	}
}
