using CargoWise.RefDbRepo.Common.Argument;
using static CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument.Constants;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public static class Extensions
	{
		public static bool BelongsToUnitedNationEdifactCategory(this IRawSupportingDocument rawSupportingDocument)
		{
			Argument.NotNull(rawSupportingDocument, nameof(rawSupportingDocument));

			return rawSupportingDocument.Code?.StartsWith(Categories.UnitedNationEdifactPrefix, System.StringComparison.OrdinalIgnoreCase) ?? false;
		}
	}
}
