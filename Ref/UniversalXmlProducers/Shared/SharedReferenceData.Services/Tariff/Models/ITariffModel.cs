using System;
using System.Text;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public interface ITariffModel
	{
		string HJID { get; set; }
		string OpType { get; set; }
		DateTime? OpDate { get; set; }
		bool IsValid(StringBuilder errorCollector, string source);
		bool IsValidForCreate(StringBuilder errorCollector, string source);
		bool IsInChapter(string chapterFilter);
		bool IsChapterSpecific { get; }

		ITariffModel GetLatest(ITariffModel firstModel);
		bool ProcessUpdate(XElement element);
	}
}
