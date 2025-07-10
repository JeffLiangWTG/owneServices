using System;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public abstract class BaseModel : ITariffModel
	{
		public string HJID { get; set; }
		public string OpType { get; set; }
		public DateTime? OpDate { get; set; }

		public abstract bool IsChapterSpecific { get; }
		public abstract bool IsInChapter(string chapterFilter);
		protected abstract bool IsValidCore(StringBuilder errorCollector, string source);
		protected virtual bool IsValidForCreateCore(StringBuilder errorCollector, string source) => true;

		public bool IsValid(StringBuilder errorCollector, string source)
		{
			var valid = !string.IsNullOrEmpty(OpType) && OpDate.HasValue;

			if (!valid)
			{
				errorCollector.AppendLine(CultureInfo.InvariantCulture, $"MetaInfo data is missing for HJID: {HJID}");
			}

			return valid && IsValidCore(errorCollector, source);
		}

		public bool IsValidForCreate(StringBuilder errorCollector, string source)
		{
			return IsValidForCreateCore(errorCollector, source);
		}

		public ITariffModel GetLatest(ITariffModel firstModel)
		{
			if (firstModel.GetType() == this.GetType() &&
				firstModel.OpDate.HasValue &&
				firstModel.OpDate > (OpDate ?? DateTime.MinValue))
			{
				return firstModel;
			}
			return this;
		}

		public bool ProcessUpdate(XElement element)
		{
			var meta = element.Element("metainfo");
			if (meta != null)
			{
				var opType = meta.Element("opType")?.Value;
				var opDate = XmlElementReader.GetDateTimeFromElement(meta, "transactionDate");
				var hjid = element.Element("hjid")?.Value;

				if ((OpDate == null || (opDate != null && opDate.Value >= OpDate)) && hjid == HJID && (opType == MetaInfoOpTypes.Created || opType == MetaInfoOpTypes.Updated || opType == MetaInfoOpTypes.Deleted))
				{
					OpDate = opDate;

					if (opType == MetaInfoOpTypes.Deleted)
					{
						ProcessDelete();
					}
					else
					{
						UpdateFromXElement(element);
					}
					return true;
				}
			}
			return false;
		}

		protected virtual void UpdateFromXElement(XElement element) { }
		protected virtual void ProcessDelete() { OpType = MetaInfoOpTypes.Deleted; }
	}
}
