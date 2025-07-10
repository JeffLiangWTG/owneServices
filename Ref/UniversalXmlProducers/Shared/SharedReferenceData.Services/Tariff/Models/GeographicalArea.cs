using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public class GeographicalArea : BaseModel
	{
		public string GeographicalAreaId { get; set; }
		public string Description { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public IEnumerable<GeographicalAreaCountry> Countries => countries.Values;
		public IEnumerable<DescriptionPeriods.DescriptionModel> Descriptions => descriptionPeriods.Values.OrderByDescending(x => x.StartDate).FirstOrDefault()?.Descriptions ?? Enumerable.Empty<DescriptionPeriods.DescriptionModel>();

		protected override bool IsValidCore(StringBuilder errorCollector, string source)
		{
			var valid = true;
			var validationErrors = new StringBuilder();

			if (string.IsNullOrEmpty(GeographicalAreaId))
			{
				validationErrors.Append("GeographicalAreaId is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"GeographicalArea validation error. Key: '{GeographicalAreaId}' Errors: '{validationErrors}' Source: '{source}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}

		protected override bool IsValidForCreateCore(StringBuilder errorCollector, string source)
		{
			var valid = true;
			var validationErrors = new StringBuilder();

			if (string.IsNullOrEmpty(Description))
			{
				validationErrors.Append("Description is required. ");
				valid = false;
			}

			if (Countries?.Any(x => string.IsNullOrEmpty(x.GeographicalAreaHjid)) ?? false)
			{
				validationErrors.Append("GeographicalAreaHjid is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"GeographicalArea validation error. Key: '{GeographicalAreaId}' Errors: '{validationErrors}' Source: '{source}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}

		public override bool IsInChapter(string chapterFilter) => true;

		public override bool IsChapterSpecific => false;

		public DateTime CalcStartDate => CommonHelper.CalcMinDate(StartDate);
		public DateTime CalcEndDate => CommonHelper.CalcMaxDate(EndDate);

		protected override void UpdateFromXElement(XElement element)
		{
			GeographicalAreaId = element.Element("geographicalAreaId")?.Value;
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityEndDate", x => EndDate = x);
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityStartDate", x => StartDate = x);

			foreach (var mcElement in element.Elements("geographicalAreaMembership"))
			{
				var mcModel = new GeographicalAreaCountry();
				XmlElementReader.LoadMetaInfo(mcModel, mcElement);
				countries.ProcessUpdate(mcModel, mcElement, null, null);
			}

			descriptionPeriods.ProcessUpdate(element);
			Description = Descriptions.FirstOrDefault()?.Description ?? string.Empty;
		}

		internal GeographicalArea SetCountries(IEnumerable<GeographicalAreaCountry> countries)
		{
			this.countries.Add(countries);
			return this;
		}

		internal GeographicalArea SetDescriptions(IEnumerable<DescriptionPeriods.DescriptionModel> descriptions)
		{
			descriptionPeriods.SetDescriptions(descriptions);
			return this;
		}

		readonly UpdatableElementList<GeographicalAreaCountry> countries = new UpdatableElementList<GeographicalAreaCountry>();
		readonly DescriptionPeriods descriptionPeriods = new DescriptionPeriods("geographicalArea");

		public class GeographicalAreaCountry : BaseTariffModel
		{
			public string GeographicalAreaHjid { get; set; }
			public DateTime? StartDate { get; set; }
			public DateTime? EndDate { get; set; }

			// Calculated
			public string CountryCode { get; set; }
			public string Description { get; set; }

			public DateTime CalcStartDate => CommonHelper.CalcMinDate(StartDate);
			public DateTime CalcEndDate => CommonHelper.CalcMaxDate(EndDate);

			protected override void UpdateFromXElement(XElement element)
			{
				GeographicalAreaHjid = element.Element("geographicalAreaGroupSid")?.Value;
				XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityEndDate", x => EndDate = x);
				XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityStartDate", x => StartDate = x);
			}
		}
	}
}
