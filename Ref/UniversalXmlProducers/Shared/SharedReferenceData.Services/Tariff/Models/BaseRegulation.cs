using System;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public class BaseRegulation : RegulationBaseModel
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime? EffectiveEndDate { get; set; }

		public string CompleteAbrogationRegulationId { get; set; }
		public string ExplicitAbrogationRegulationId { get; set; }
		public string RelatedAntidumpingRegulationId { get; set; }
		public int ReplacementIndicator { get; set; }

		protected override bool IsValidCore(StringBuilder errorCollector, string source)
		{
			var valid = true;
			var validationErrors = new StringBuilder();

			if (string.IsNullOrEmpty(RegulationId))
			{
				validationErrors.Append("RegulationId is required. ");
				valid = false;
			}

			if (!StartDate.HasValue)
			{
				validationErrors.Append("StartDate is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"BaseRegulation validation error. Key: '{RegulationId}' Errors: '{validationErrors}' Source: '{source}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}

		public DateTime CalcStartDate => CommonHelper.CalcMinDate(StartDate);

		protected override void UpdateFromXElement(XElement element)
		{
			RegulationId = element.Element("baseRegulationId")?.Value ?? RegulationId ?? string.Empty;
			RegulationRoleTypeId = element.Element("regulationRoleType")?.Element("regulationRoleTypeId")?.Value ?? RegulationRoleTypeId ?? string.Empty;
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityStartDate", x => StartDate = x);
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityEndDate", x => EndDate = x);
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "effectiveEndDate", x => EffectiveEndDate = x);

			CompleteAbrogationRegulationId = element.Element("completeAbrogationRegulation")?.Element("completeAbrogationRegulationId")?.Value ?? CompleteAbrogationRegulationId ?? string.Empty;
			ExplicitAbrogationRegulationId = element.Element("explicitAbrogationRegulation")?.Element("explicitAbrogationRegulationId")?.Value ?? ExplicitAbrogationRegulationId ?? string.Empty;
			RelatedAntidumpingRegulationId = element.Element("relatedAntidumpingRegulationId")?.Value ?? RelatedAntidumpingRegulationId ?? string.Empty;

			XmlElementReader.UpdateIntFromElementIfPresent(element, "replacementIndicator", x => ReplacementIndicator = x);
		}
	}
}
