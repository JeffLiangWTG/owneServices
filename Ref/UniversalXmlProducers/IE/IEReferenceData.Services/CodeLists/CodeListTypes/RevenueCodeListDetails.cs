using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services
{
	public abstract class RevenueCodeListDetails
	{
		public virtual bool AllowCombination => false;

		public virtual bool IsPublished => true;

		public virtual UpdateType UpdateType => UpdateType.Full;
	}
}
