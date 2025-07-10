using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public static class IMODangerousGoodsSummaryWriter
	{
		public static string WriteIMOSummary(this IDangerousGood dangerousGood)
		{
			var data = new string[]
			{
				dangerousGood.UNNumber(),
				dangerousGood.ProperShippingName,
				dangerousGood.TechnicalNameInBrackets(),
				dangerousGood.IMOClassDescription(),
				dangerousGood.SubLabel1Description(),
				dangerousGood.PackingGroupDescription(),
				dangerousGood.FlashPointDescription(),
				dangerousGood.MarinePollutant?.Code,
				dangerousGood.LimitedQuantityDescription(),
			}.Concat(dangerousGood.GetRadioactiveSummary());

			return string.Join(", ", data.Where(s => !s.IsNullOrEmpty()));
		}

		static string TechnicalNameInBrackets(this IDangerousGood dangerousGood) =>
			dangerousGood.TechnicalName.IsEmpty
				? string.Empty
				: FormattableString.Invariant($"({dangerousGood.TechnicalName})"); // Fixed format text for document

		static string IMOClassDescription(this IDangerousGood dangerousGood) =>
			string.IsNullOrWhiteSpace(dangerousGood.IMOClass)
				? string.Empty
				: FormattableString.Invariant($"class {dangerousGood.IMOClass}"); // Fixed format text for document

		static string SubLabel1Description(this IDangerousGood dangerousGood) =>
			string.IsNullOrWhiteSpace(dangerousGood.SubLabel1)
				? string.Empty
				: FormattableString.Invariant($"({dangerousGood.SubLabel1})"); // Fixed format text for document
	}
}
