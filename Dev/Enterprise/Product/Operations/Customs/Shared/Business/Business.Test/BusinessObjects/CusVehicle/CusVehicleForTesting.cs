using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	public class CusVehicleForTesting : CusVehicle
	{
		public CusVehicleForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override EngineRelationshipType EngineRelationship => EngineRelationshipForTesting ?? base.EngineRelationship;

		public EngineRelationshipType? EngineRelationshipForTesting { get; set; }

		public new ICusEngineCollection<CusEngine, CusVehicleForTesting> Engines => (ICusEngineCollection<CusEngine, CusVehicleForTesting>)base.Engines;

		protected override ICusEngineCollection<CusEngine, CusVehicle> GetNewCusEngineCollection() => new CusEngineCollection<CusEngine, CusVehicleForTesting>(this);
	}
}
