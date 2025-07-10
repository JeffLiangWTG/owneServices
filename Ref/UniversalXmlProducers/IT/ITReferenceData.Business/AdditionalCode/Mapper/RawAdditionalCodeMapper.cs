using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public sealed class RawAdditionalCodeMapper : IRawAdditionalCodeMapper
	{
		RefCusCodeList IRawAdditionalCodeMapper.GetMapping(IRawAdditionalCode rawAdditionalCode)
		{
			Argument.NotNull(rawAdditionalCode, nameof(rawAdditionalCode));

			return new RefCusCodeList
			{
				ZZD_Code = rawAdditionalCode.Code,
				ZZD_Description = rawAdditionalCode.Description,
				ZZD_StartDate = rawAdditionalCode.StartDate.GetDateOrFallbackToMinSmallDateTime(),
				ZZD_EndDate = rawAdditionalCode.EndDate.GetDateOrFallbackToMaxSmallDateTime(),
			};
		}
	}
}
