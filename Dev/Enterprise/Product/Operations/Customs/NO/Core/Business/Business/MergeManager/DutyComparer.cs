using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.NO.Business
{
	public static class DutyComparer
	{
		[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1012:Opening braces should be spaced correctly", Justification = "Pattern matching")]
		public static Comparison<IDutyCategory> Comparison => (IDutyCategory xFee, IDutyCategory yFee) =>
		{
			return (xFee, yFee) switch
			{
				({ IsCustomsDuty: true }, _) => yFee.IsCustomsDuty ? CompareAlphabethically() : -1,
				(_, { IsCustomsDuty: true }) => 1,

				({ IsAgriculturalDuty: true }, _) => yFee.IsAgriculturalDuty ? CompareAlphabethically() : -1,
				(_, { IsAgriculturalDuty: true }) => 1,

				({ IsVAT: true }, _) => yFee.IsVAT ? CompareAlphabethically() : 1,
				(_, { IsVAT: true }) => -1,

				_ => CompareAlphabethically(),
			};
			int CompareAlphabethically() => string.Compare(xFee.DutyCode, yFee.DutyCode);
		};
	}
}
