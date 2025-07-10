using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business
{
	public interface ICusCodeAttributeProvider
	{
		RefCusCodeListAttribute GetCusCodeAttribute(string code);
	}
}
