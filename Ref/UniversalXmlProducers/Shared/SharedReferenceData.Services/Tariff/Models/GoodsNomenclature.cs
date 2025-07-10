using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public class GoodsNomenclature : BaseModel
	{
		public string ItemId { get; set; }
		public string Description { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string ProductLineSuffix { get; set; }
		public int Indent { get; set; }
		public int SectionNumber { get; set; }
		public bool IsSection { get; set; }

		protected override bool IsValidCore(StringBuilder errorCollector, string source)
		{
			var valid = true;
			var validationErrors = new StringBuilder();

			if (ItemId?.Length != 10 || !long.TryParse(ItemId, out _))
			{
				validationErrors.Append("A valid ItemId is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"GoodsNomenclature validation error. Key: '{ItemId}' Errors: '{validationErrors}' Source: '{source}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}

		public override bool IsInChapter(string chapterFilter)
		{
			return ItemId?.StartsWith(chapterFilter, StringComparison.Ordinal) ?? false;
		}

		public override bool IsChapterSpecific => true;

		// Key Gem Fields
		public string Key { get; set; }
		public string CleanId { get; set; }
		public int Id { get; set; }
		public string Chapter { get; set; }
		public int? ParentId { get; set; }
		public int Level { get; set; }
		public bool? IsForMeasure { get; set; }
		public bool? IsForNomenclature { get; set; }

		public DateTime CalcActiveDate(IDateTimeProvider dateTimeProvider) => EndDate.HasValue ? CalcEndDate : dateTimeProvider.UTCDateTime.Date;

		public DateTime CalcEndDate => CommonHelper.CalcMaxDate(EndDate);
		public DateTime CalcStartDate => CommonHelper.CalcMinDate(StartDate);

		protected override void UpdateFromXElement(XElement element)
		{
			ItemId = element.Element("goodsNomenclatureItemId").Value;
			ProductLineSuffix = element.Element("produclineSuffix").Value;

			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityEndDate", x => EndDate = x);
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityStartDate", x => StartDate = x);

			UpdateDescriptionAndIndents(element);
		}

		internal void UpdateDescriptionAndIndents(XElement element)
		{
			descriptionPeriods.ProcessUpdate(element);
			Description = descriptionPeriods.Values.OrderByDescending(x => x.StartDate).FirstOrDefault()?.Descriptions?.FirstOrDefault()?.Description ?? Description;

			foreach (var mcElement in element.Elements("goodsNomenclatureIndents"))
			{
				var mcModel = new GoodsNomenclatureIndent();
				XmlElementReader.LoadMetaInfo(mcModel, mcElement);
				indents.ProcessUpdate(mcModel, mcElement);
			}
			var indent = indents.Values.OrderByDescending(x => x.StartDate).FirstOrDefault()?.NumberIndents;
			if (!string.IsNullOrEmpty(indent)&& int.TryParse(indents.Values.OrderByDescending(x => x.StartDate).FirstOrDefault()?.NumberIndents ?? string.Empty, out var intIndent))
			{
				Indent = intIndent;
			}
		}

		readonly DescriptionPeriods descriptionPeriods = new DescriptionPeriods("goodsNomenclature");
		readonly UpdatableElementList<GoodsNomenclatureIndent> indents = new UpdatableElementList<GoodsNomenclatureIndent>();

		public class GoodsNomenclatureIndent : BaseTariffModel
		{
			public DateTime? StartDate { get; set; }
			public string NumberIndents { get; set; }

			protected override void UpdateFromXElement(XElement element)
			{
				XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "validityStartDate", x => StartDate = x);
				NumberIndents = element.Element("numberIndents")?.Value ?? NumberIndents ?? string.Empty;
			}
		}
	}
}
