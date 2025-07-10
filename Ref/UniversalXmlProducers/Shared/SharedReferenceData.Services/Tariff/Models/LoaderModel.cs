using System.Text;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models
{
	public abstract class LoaderModel : BaseModel
	{
		public override bool IsChapterSpecific => false;

		public override  bool IsInChapter(string chapterFilter) => true;
	}
}
