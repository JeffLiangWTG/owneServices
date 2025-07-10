using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public class HsnTariffDTYRateRule
	{
		public HsnTariffDTYRateRule(string tariffCode,
			IEnumerable<HsnTariffDTYRatePreferenceRule> preferenceRules,
			IEnumerable<HsnTariffDTYRateFootnoteRule> footnoteRules
		)
		{
			TariffCode = tariffCode;
			PreferenceRules = preferenceRules.ToArray();
			FootnoteRules = footnoteRules.ToArray();
		}

		public string TariffCode { get; }

		/// <summary>
		/// Contains the default rules defined at the top of the sheet.
		/// </summary>
		public IReadOnlyCollection<HsnTariffDTYRatePreferenceRule> PreferenceRules { get; }

		/// <summary>
		/// Contains the footnote rules defined at the bottom of the sheet.
		/// </summary>
		public IReadOnlyCollection<HsnTariffDTYRateFootnoteRule> FootnoteRules { get; }
	}
}
