using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business;

public class CusEngineCollection<TCusEngine, TCusEngineParent> : DependentBusinessObjectCollection<TCusEngine, TCusEngineParent>, ICusEngineCollection<TCusEngine, TCusEngineParent>
	where TCusEngine : CusEngine
	where TCusEngineParent : BusinessObject, ICusEngineParent
{
	public CusEngineCollection(TCusEngineParent master) : base(master)
	{
	}

	protected override ZQuery CreateAdditionalFilter()
	{
		var result = base.CreateAdditionalFilter();
		if (Master.EngineRelationship == EngineRelationshipType.None)
		{
			result.IsNoResultQuery = true;
		}
		return result;
	}

	protected override SchemaGuidColumn FKSchemaColumnInDependent
	{
		get { return CusEngineSchema.CEG_ParentID; }
	}

	protected override bool AllowNewCore => Master.EngineRelationship switch
	{
		EngineRelationshipType.One => Count == 0,
		EngineRelationshipType.Many => true,
		_ => false
	};
}
