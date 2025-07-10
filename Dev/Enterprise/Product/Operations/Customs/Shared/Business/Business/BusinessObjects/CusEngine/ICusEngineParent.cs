using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business;

public interface ICusEngineParent
{
	EngineRelationshipType EngineRelationship { get; }
	IBusinessObjectCollection Engines { get; }
}
