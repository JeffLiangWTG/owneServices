using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public class GoodsNomenclatureProcessor : ProcessorBase<GoodsNomenclature>
	{
		public GoodsNomenclatureProcessor(IDateTimeProvider dateTimeProvider, IRefXmlBuilder[] builders) : this(dateTimeProvider, builders, new SectionHelper()) { }
		public GoodsNomenclatureProcessor(IDateTimeProvider dateTimeProvider, IRefXmlBuilder[] builders, SectionHelper sectionHelper) : base(dateTimeProvider, builders, new GoodsNomenclatureLoader())
		{
			this.sectionHelper = sectionHelper;
		}
		readonly SectionHelper sectionHelper;

		protected override void UpdateModelsCore(string chapterFilter, List<ITariffModel> referenceData, StringBuilder errorCollector)
		{
			var models = Models.Cast<GoodsNomenclature>().ToList();

			_ = Parallel.ForEach(models, m =>
			{
				var chapterCode = m.ItemId.Substring(0, 2);
				_ = int.TryParse(chapterCode, out var chapter);

				m.Chapter = chapterCode;
				m.SectionNumber = sectionHelper.GetSectionNumber(chapter);

				m.CleanId = CommonHelper.CleanCommodityCode(m.ItemId);

				if (string.IsNullOrWhiteSpace(m.Description))
				{
					m.Description = DescriuptionNotProvided;
				}
			});

			models.AddRange(CreateSectionModels(chapterFilter));

			models = CreateKeys(models);

			ReplaceHtmlTagsInDescriptions(models);

			FlagIsForMeasure(models, referenceData);

			Models = models.Cast<ITariffModel>().ToList();
		}

		List<GoodsNomenclature> CreateSectionModels(string chapterFilter)
		{
			var sectionModels = new List<GoodsNomenclature>();

			var sections = sectionHelper.GetSectionsByChapter(chapterFilter);

			sectionModels.AddRange(sections.Select(x => new GoodsNomenclature
			{
				ItemId = x.SectionNumber.ToString("00", CultureInfo.InvariantCulture),
				CleanId = x.SectionNumber.ToString("00", CultureInfo.InvariantCulture),
				Indent = 0,
				Chapter = x.SectionNumber.ToString("00", CultureInfo.InvariantCulture),
				Description = x.Description,
				SectionNumber = x.SectionNumber,
				ProductLineSuffix = "00",
				IsSection = true
			}));

			return sectionModels;
		}

		List<GoodsNomenclature> CreateKeys(List<GoodsNomenclature> models)
		{
			var gneList = models
				.OrderBy(x => x.ItemId)
				.ThenBy(x => x.ProductLineSuffix)
				.ThenBy(x => x.Indent)
				.ThenByDescending(x => x.CalcEndDate)
				.ThenBy(x => x.CalcStartDate)
				.ToList();

			PopulateKeys(gneList);
			GenerateKeys(gneList);

			ModelTracking("GoodsNomenclature CraeteKeys", gneList);

			return gneList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
		void PopulateKeys(List<GoodsNomenclature> keys)
		{
			// First Pass - Find Parent & Level
			for (int i = 0; i < keys.Count; i++)
			{
				var current = keys[i];
				current.Id = i + 1;

				GoodsNomenclature parent = null;

				for (int j = i - 1; j >= 0; j--)
				{
					var check = keys[j];

					if (check.Chapter == current.Chapter)
					{
						_ = int.TryParse(check.ProductLineSuffix, out var checkPLS);
						_ = int.TryParse(current.ProductLineSuffix, out var currentPLS);

						var activeDate = current.CalcActiveDate(DateTimeProvider);

						if (((check.Indent < current.Indent)
							 || (check.Indent == current.Indent && check.CleanId.Length < current.CleanId.Length)
							 || (check.Indent == current.Indent && check.CleanId.Length == current.CleanId.Length && checkPLS < currentPLS && current.Indent == 0)
							 )
							&& (activeDate >= check.CalcStartDate && activeDate <= check.CalcEndDate))
						{
							parent = check;
							break;
						}
					}
					else
					{
						break;
					}
				}

				current.ParentId = parent?.Id;
				current.Level = parent?.Level + 1 ?? 0;
			}

			var invalidKeys = keys.Where(x => x.Indent == 0 && !x.IsSection && x.CleanId.Length > 2 && x.ProductLineSuffix == "80" && !x.ParentId.HasValue);

			if (invalidKeys.Any())
			{
				var error = new StringBuilder();
				error.AppendLine("Incomplete GoodsNomenclature data:");
				error.AppendLine(CultureInfo.InvariantCulture, $"{invalidKeys.Count()} item(s) are missing parents (only first 10 included)");
				invalidKeys.Take(10).ToList().ForEach(x => error.AppendLine(CultureInfo.InvariantCulture, $"Item: {x.ItemId} ProductLineSuffix: {x.ProductLineSuffix} Indent: {x.Indent}"));

				throw new ApplicationException(error.ToString());
			}
		}

		static void GenerateKeys(List<GoodsNomenclature> keys)
		{
			// Second Pass - Generate Keys
			for (int i = 0; i < keys.Count; i++)
			{
				var current = keys[i];
				var key = string.Empty;

				if (current.Indent == 0) // Section / Chapter / Heading (opt) / Sub-Heading
				{
					if (current.IsSection)
					{
						key = $"{current.SectionNumber:00}";
					}
					else if (current.CleanId.Length == 2)
					{
						key = $"{current.SectionNumber:00}.{current.Chapter}";
					}
					else if (current.ProductLineSuffix != "80")
					{
						var keysInGroup = keys.Where(x => x.ParentId == current.ParentId && x.Level == current.Level).OrderBy(x => x.Id).ToList();
						var keyId = keysInGroup.IndexOf(current) + 1;

						var heading = $"{keyId:00}";
						key = $"{current.SectionNumber:00}.{current.Chapter}.{heading}";
					}
					else
					{
						var subHeading = current.ItemId.Substring(2, 2);
						var parent = keys[current.ParentId.Value - 1];

						var heading = string.Empty;
						if (parent.ProductLineSuffix != "80")
						{
							heading = parent.Key.Substring(6, 2);
						}

						key = $"{current.SectionNumber:00}.{current.Chapter}.{heading}.{subHeading}";
					}
				}
				else
				{
					if (current.Indent == 1 && int.TryParse(current.ItemId.Substring(4, 2), out var value) && value > 0) // special case to split 29 into .2.9 and 10, 20 etc into .1 .2
					{
						GoodsNomenclature parent = null;
						int? parentId = current.ParentId;

						while (parent == null && parentId.HasValue)
						{
							parent = keys[parentId.Value - 1];
							parentId = parent.ParentId;

							if (parent.CleanId.Length == 6)
							{
								parent = null;
							}
						}

						key = parent?.Key ?? $"{current.SectionNumber:00}.{current.Chapter}..{current.ItemId.Substring(2, 2)}";

						var part = current.ItemId.Substring(4, 2);
						key += $".{part.Substring(0, 1)}";
						if (value % 10 != 0)
						{
							key += $".{part.Substring(1, 1)}";
						}
					}
					else
					{
						if (!current.ParentId.HasValue)
						{
							key = $"{current.SectionNumber:00}.{current.Chapter}..{current.ItemId.Substring(2, 2)}";
						}
						else
						{
							key = keys[current.ParentId.Value - 1].Key;
						}

						var keysInGroup = keys.Where(x => x.ParentId == current.ParentId && x.Level == current.Level).OrderBy(x => x.Id).ToList();
						var keyId = keysInGroup.IndexOf(current);
						var itemSeq = (keyId + 1) * 10;

						if (keysInGroup.Count >= 10)
						{
							key += $".{itemSeq:000}";
						}
						else
						{
							key += $".{itemSeq:00}";
						}
					}
				}

				current.Key = key;
			}
		}

		void FlagIsForMeasure(List<GoodsNomenclature> models, List<ITariffModel> referenceData)
		{
			var dateTimeProvider = DateTimeProvider;
			var measures = referenceData?.OfType<Measure>() ?? new List<Measure>();

			foreach (var g in models.Where(x => x.Level == 0))
			{
				if (!g.IsForMeasure.HasValue)
				{
					UpdateMeasureFromChild(g, models, measures, false, dateTimeProvider);
				}
			}
		}

		(bool HasActiveChild, bool HasPartialChild) UpdateMeasureFromChild(GoodsNomenclature current, List<GoodsNomenclature> models, IEnumerable<Measure> measures, bool parentHasMeasure, IDateTimeProvider dateTimeProvider)
		{
			var hasMeasure = parentHasMeasure || measures.Any(x => x.ItemId == current.ItemId);
			var children = models.Where(x => x.ParentId == current.Id);

			var hasActiveChild = false;
			var hasPartialChild = false;

			if (children.Any())
			{
				foreach (var c in children)
				{
					var result = UpdateMeasureFromChild(c, models, measures, hasMeasure, dateTimeProvider);
					hasActiveChild |= result.HasActiveChild;
					hasPartialChild |= result.HasPartialChild;
				}
			}

			current.IsForMeasure = current.Level > 0 && current.ProductLineSuffix == "80" && current.CalcEndDate >= dateTimeProvider.UTCHistoricalDate && (!hasPartialChild || !hasActiveChild) && hasMeasure;
			current.IsForNomenclature = current.CalcEndDate >= dateTimeProvider.UTCHistoricalDate && hasPartialChild && !hasActiveChild; // Date overlaps with historical could apply to both

			return (hasActiveChild || (current.ProductLineSuffix == "80" && current.CalcStartDate <= DateTimeProvider.UTCDateTime && current.CalcEndDate >= DateTimeProvider.UTCDateTime),
					hasPartialChild || (current.ProductLineSuffix == "80" && current.CalcStartDate <= DateTimeProvider.UTCDateTime && current.CalcEndDate >= DateTimeProvider.UTCHistoricalDate));
		}

		static void ReplaceHtmlTagsInDescriptions(List<GoodsNomenclature> models)
		{
			models.ForEach((m) =>
			{
				m.Description = CommonHelper.CleanHtmlTags(m.Description);
			});
		}

		protected virtual void ModelTracking(string action, List<GoodsNomenclature> models) { } // Used to assist logging/debugging 

		const string DescriuptionNotProvided = "(Commodity description was not provided)";
	}
}
