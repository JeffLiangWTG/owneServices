using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

public abstract class BaseCusEngineCollectionTest<TCusEngineParent> : BusinessObjectCollectionTestCase
	where TCusEngineParent : BusinessObject, ICusEngineParent
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var parent = Factory.New<TCusEngineParent>();
		return (BusinessObjectCollection)parent.Engines;
	}
}

[TestedType(typeof(CusEngineCollection<CusEngine, CusVehicle>))]
public class CusEngineCollectionTestBaseOnly : BaseCusEngineCollectionTest<CusVehicle>
{
	public void TestParentTableCodeDefaultForNewChild()
	{
		var engines = GetCollectionToTestRelationship();
		engines.Master.EngineRelationshipForTesting = EngineRelationshipType.One;
		var engine = engines.AddNew();
		AssertEquals(CusVehicleSchema.Constants.Prefix, engine.CEG_ParentTableCode);
	}

	public void TestParentDefaultForNewChild()
	{
		var vehicle = Factory.New<CusVehicle>();
		var engines = vehicle.Engines;
		var engine = engines.AddNew();
		AssertNotNull(engine.Parent);
		AssertEquals(engines.Master, engine.Parent);
	}

	public void TestNoQueryResultWhenEngineRelationshipIsNone()
	{
		var engines = GetCollectionToTestRelationship();
		engines.Master.EngineRelationshipForTesting = EngineRelationshipType.None;
		AssertEquals("EngineRelationshipType.None sets filter query IsNoResultQuery to true", true, engines.CompleteFilter.IsNoResultQuery);
	}

	public void TestAllowNewWhenEngineRelationshipIsNone()
	{
		var engines = GetCollectionToTestRelationship();
		engines.Master.EngineRelationshipForTesting = EngineRelationshipType.None;
		AssertEquals("EngineRelationshipType.None does not allow new row", false, engines.AllowNew);
	}

	public void TestAllowNewWhenEngineRelationshipIsOne()
	{
		var engines = GetCollectionToTestRelationship();
		engines.Master.EngineRelationshipForTesting = EngineRelationshipType.One;
		AssertEquals("EngineRelationshipType.One allows new row when count is 0", true, engines.AllowNew);

		_ = engines.AddNew();
		AssertEquals("EngineRelationshipType.One does not allow new row when count is not 0", false, engines.AllowNew);
	}

	public void TestAllowNewWhenEngineRelationshipIsMany()
	{
		var engines = GetCollectionToTestRelationship();
		engines.Master.EngineRelationshipForTesting = EngineRelationshipType.Many;
		AssertEquals("EngineRelationshipType.Many allows new row when count is 0", true, engines.AllowNew);

		_ = engines.AddNew();
		AssertEquals("Engines Count is 1", 1, engines.Count);
		AssertEquals("EngineRelationshipType.Many allows new row when count is 1", true, engines.AllowNew);

		_ = engines.AddNew();
		AssertEquals("Engines Count is 2", 2, engines.Count);
		AssertEquals("EngineRelationshipType.Many allows new row when count is 2", true, engines.AllowNew);
	}

	ICusEngineCollection<CusEngine, CusVehicleForTesting> GetCollectionToTestRelationship()
	{
		var vehicle = Factory.New<CusVehicleForTesting>();
		return vehicle.Engines;
	}
}

