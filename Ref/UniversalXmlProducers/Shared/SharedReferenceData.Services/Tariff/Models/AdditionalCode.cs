using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public class AdditionalCode : BaseModel
	{
		public string Code { get; set; }
		public string CodeType { get; set; }
		public string Description { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public IEnumerable<DescriptionPeriods.DescriptionModel> Descriptions => descriptionPeriods.Values.OrderByDescending(x => x.StartDate).FirstOrDefault()?.Descriptions ?? Enumerable.Empty<DescriptionPeriods.DescriptionModel>();

		public override bool IsChapterSpecific => false;
		public override bool IsInChapter(string chapterFilter) => true;

		protected override bool IsValidCore(StringBuilder errorCollector, string source)
		{
			var valid = true;
			var validationErrors = new StringBuilder();

			if (string.IsNullOrEmpty(Code))
			{
				validationErrors.Append("Code is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"AdditionalCode validation error. Key: '{CodeType}{Code}' Errors: '{validationErrors}' Source: '{source}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}

		protected override bool IsValidForCreateCore(StringBuilder errorCollector, string source)
		{
			var valid = true;
			var validationErrors = new StringBuilder();

			if (string.IsNullOrEmpty(CodeType))
			{
				validationErrors.Append("CodeType is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(Description))
			{
				validationErrors.Append("Description is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"AdditionalCode validation error. Key: '{CodeType}{Code}' Errors: '{validationErrors}' Source: '{source}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}

		public DateTime CalcStartDate => CommonHelper.CalcMinDate(StartDate);

		protected override void UpdateFromXElement(XElement element)
		{
			Code = element.Element("additionalCodeCode").Value;
			CodeType = element.Element("additionalCodeType")?.Element("additionalCodeTypeId").Value ?? CodeType ?? string.Empty;

			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityStartDate", x => StartDate = x);
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityEndDate", x => EndDate = x);

			descriptionPeriods.ProcessUpdate(element);
			Description = Descriptions.FirstOrDefault()?.Description ?? string.Empty;
		}

		internal AdditionalCode SetDescriptions(IEnumerable<DescriptionPeriods.DescriptionModel> descriptions)
		{
			descriptionPeriods.SetDescriptions(descriptions);
			return this;
		}

		readonly DescriptionPeriods descriptionPeriods = new DescriptionPeriods("additionalCode");
	}
}
