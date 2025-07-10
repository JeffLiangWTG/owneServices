using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public interface IRawAdditionalCodeMapper
	{
		RefCusCodeList GetMapping(IRawAdditionalCode rawAdditionalCode);
	}
}
