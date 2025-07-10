using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services
{
	public interface IRevenueCodeListDetails
	{
		ApplicationType ApplicationType { get; }

		string Code { get; }

		string NameInFile { get; }

		string TableTitleInFile { get; }

		string CodeFormattingRegularExpression { get; }

		bool AllowCombination { get; }

		bool IsPublished { get; }

		UpdateType UpdateType { get; }
	}
}
