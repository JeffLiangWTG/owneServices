using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public class DescriptionPeriods
	{
		public DescriptionPeriods(string elementPrefix)
		{
			this.elementPrefix = elementPrefix;
		}

		public IEnumerable<DescriptionPeriodModel> Values => descriptionPeriods.Values;

		public void ProcessUpdate(XElement element)
		{
			foreach (var descriptionPeriodElement in element.Elements(elementPrefix + "DescriptionPeriod"))
			{
				var descriptionPeriod = new DescriptionPeriodModel(elementPrefix);
				XmlElementReader.LoadMetaInfo(descriptionPeriod, descriptionPeriodElement);
				descriptionPeriods.ProcessUpdate(descriptionPeriod, descriptionPeriodElement);
			}
		}

		internal void SetDescriptions(IEnumerable<DescriptionModel> descriptions)
		{
			descriptionPeriods.Add(new[] { new DescriptionPeriodModel(elementPrefix) { HJID = "8888", StartDate = new DateTime(1980, 1, 1, 12, 13, 0) }.SetDescriptions(descriptions) });
		}

		readonly string elementPrefix;
		readonly UpdatableElementList<DescriptionPeriodModel> descriptionPeriods = new UpdatableElementList<DescriptionPeriodModel>();

		public class DescriptionPeriodModel : BaseTariffModel
		{
			public DescriptionPeriodModel(string elementPrefix = null)
			{
				this.elementPrefix = elementPrefix;
			}

			public DateTime? StartDate { get; set; }
			public DateTime? EndDate { get; set; }
			public IEnumerable<DescriptionModel> Descriptions => descriptions.Values;

			protected override void UpdateFromXElement(XElement element)
			{
				XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityEndDate", x => EndDate = x);
				XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityStartDate", x => StartDate = x);
				foreach (var descriptionElement in element.Elements(elementPrefix + "Description"))
				{
					var descriptionModel = new DescriptionModel();
					XmlElementReader.LoadMetaInfo(descriptionModel, descriptionElement);
					descriptions.ProcessUpdate(descriptionModel, descriptionElement);
				}
			}

			internal DescriptionPeriodModel SetDescriptions(IEnumerable<DescriptionModel> descriptions)
			{
				this.descriptions.Add(descriptions);
				return this;
			}

			readonly string elementPrefix;
			readonly UpdatableElementList<DescriptionModel> descriptions = new UpdatableElementList<DescriptionModel>();
		}

		public class DescriptionModel : BaseTariffModel
		{
			public string LanguageCode { get; set; }
			public string Description { get; set; }

			protected override void UpdateFromXElement(XElement element)
			{
				Description = element.Element("description")?.Value ?? Description ?? string.Empty;

				var languageElement = element.Elements("language")?.OrderByDescending(x => x.Element("languageId")?.Value).FirstOrDefault();
				LanguageCode = languageElement?.Element("languageId")?.Value ?? LanguageCode ?? string.Empty;
			}
		}
	}
}
