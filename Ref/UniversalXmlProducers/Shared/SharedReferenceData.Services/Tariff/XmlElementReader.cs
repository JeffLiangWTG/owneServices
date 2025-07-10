using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff
{
	public abstract class XmlElementReader
	{
		static internal void LoadMetaInfo(ITariffModel model, XElement element)
		{
			model.HJID = element.Element("hjid")?.Value;
			var meta = element.Element("metainfo");
			if (meta != null)
			{
				model.OpType = meta.Element("opType")?.Value ?? "";
				model.OpDate = GetDateTimeFromElement(meta, "transactionDate");
			}
		}

		static internal protected DateTime? GetDateTimeFromElement(XElement element, string elementName)
		{
			DateTime? dt = null;

			if (element?.Element(elementName)?.Value is string value)
			{
				dt = DateTime.Parse(value, CultureInfo.CurrentCulture);
			}

			return dt;
		}

		static internal protected void UpdateDateTimeFromElementIfPresent(XElement element, string elementName, Action<DateTime?> setValue)
		{
			if (element?.Element(elementName)?.Value is string value)
			{
				setValue(DateTime.Parse(value, CultureInfo.CurrentCulture));
			}
		}

		static internal protected void UpdateDecimalFromElementIfPresent(XElement element, string elementName, Action<decimal> setValue)
		{
			if (element?.Element(elementName)?.Value != null)
			{
				setValue(decimal.Parse(element.Element(elementName)?.Value, CultureInfo.CurrentCulture));
			}
		}

		static internal protected void UpdateIntFromElementIfPresent(XElement element, string elementName, Action<int> setValue)
		{
			if (element?.Element(elementName)?.Value != null)
			{
				setValue(int.Parse(element.Element(elementName)?.Value, CultureInfo.CurrentCulture));
			}
		}
	}

	public delegate void PostFileActionDelegate(IFileDetails file);

	public abstract class XmlElementReader<T> : XmlElementReader
		where T : ITariffModel
	{
		public List<ITariffModel> ProcessXml(string chapterFilter, IReadOnlyCollection<IFileDetails> files, StringBuilder errorCollector, PostFileActionDelegate processingAction)
		{
			ChapterFilter = chapterFilter;
			Models = new UpdatableElementList<T>();
			ErrorCollector = errorCollector;

			foreach (var file in files)
			{
				Source = Path.GetFileName(file.Filename);
				var hasData = false;

				using (var fileStream = new FileStream(file.Filename, FileMode.Open, FileAccess.Read))
				{
					using (var xmlReader = XmlReader.Create(fileStream))
					{
						if (xmlReader.ReadToFollowing(ParentElement))
						{
							using (var detailReader = xmlReader.ReadSubtree())
							{
								while (detailReader.ReadToFollowing(ElementName))
								{
									if (XNode.ReadFrom(detailReader) is XElement element)
									{
										hasData |= ProcessElement(element);
									}
								}
							}
						}
					}
				}

				if (hasData)
				{
					processingAction.Invoke(file);
				}
			}

			return Models.Values.Cast<ITariffModel>().ToList();
		}

		protected string ChapterFilter { get; private set; }
		protected UpdatableElementList<T> Models { get; private set; }
		protected StringBuilder ErrorCollector { get; private set; }
		protected string Source { get; private set; }

		protected bool ProcessElement(XElement element)
		{
			var hasData = false;
			var model = CreateMinimumModelFromXElement(element);
			LoadMetaInfo(model, element);

			if ((IsValidElement(element) || model.OpType == MetaInfoOpTypes.Deleted) && model.IsInChapter(ChapterFilter) && model.IsValid(ErrorCollector, Source))
			{
				hasData |= ProcessElementCore(model, element);
			}
			return hasData;
		}

		protected virtual bool ProcessElementCore(T model, XElement element)
		{
			return Models.ProcessUpdate(model, element, ErrorCollector, Source);
		}

		protected abstract string ParentElement { get; }
		protected abstract string ElementName { get; }

		protected abstract T CreateMinimumModelFromXElement(XElement element);

		protected virtual bool IsValidElement(XElement element) => true;
	}
}
