using System;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public class MeasureConditionCode : LoaderModel
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		protected override bool IsValidCore(StringBuilder errorCollector, string source)
		{
			var valid = true;
			var validationErrors = new StringBuilder();

			if (string.IsNullOrEmpty(Id))
			{
				validationErrors.Append("ConditionCode is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(Description))
			{
				validationErrors.Append("Description is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"MeasureConditionCode validation error. Key: '{Id}' Errors: '{validationErrors}' Source: '{source}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}

		public DateTime CalcStartDate => CommonHelper.CalcMinDate(StartDate);

		protected override void UpdateFromXElement(XElement element)
		{
			Id = element.Element("conditionCode").Value;
			Description = element.Element("measureConditionCodeDescription")?.Element("description")?.Value ?? string.Empty;
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityStartDate", x => StartDate = x);
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityEndDate", x => EndDate = x);
		}
	}
}
