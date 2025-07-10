using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseCusVehicleCollectionTest<TCusVehicle, TJobComInvoiceLine, TJobDeclaration> : BusinessObjectCollectionTestCase
		where TCusVehicle : CusVehicle
		where TJobComInvoiceLine : BaseJobComInvoiceLine
		where TJobDeclaration : BaseJobDeclaration
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<TJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			return (BusinessObjectCollection)invoiceLine.Vehicles;
		}
	}

	[TestedType(typeof(CusVehicleCollection<CusVehicle, BaseJobComInvoiceLineForTesting>))]
	public class CusVehicleCollectionTestBaseOnly : BaseCusVehicleCollectionTest<CusVehicle, BaseJobComInvoiceLine, BaseJobDeclaration>
	{
		public void TestParentTableCodeDefaultForNewChild()
		{
			var vehicles = GetCollectionToTestRelationship();
			vehicles.Master.VehicleRelationshipForTesting = VehicleRelationshipType.One;
			var vehicle = vehicles.AddNew();
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, vehicle.CVH_ParentTableCode);
		}

		public void TestParentDefaultForNewChild()
		{
			var vehicles = GetCollectionToTestRelationship();
			var vehicle = vehicles.AddNew();
			AssertNotNull(vehicle.Parent);
			AssertEquals(vehicles.Master, vehicle.Parent);
		}

		public void TestNoQueryResultWhenVehicleRelationshipIsNone()
		{
			var vehicles = GetCollectionToTestRelationship();
			vehicles.Master.VehicleRelationshipForTesting = VehicleRelationshipType.None;
			AssertEquals("VehicleRelationshipType.None sets filter query IsNoResultQuery to true", true, vehicles.CompleteFilter.IsNoResultQuery);
		}

		public void TestAllowNewWhenVehicleRelationshipIsNone()
		{
			var vehicles = GetCollectionToTestRelationship();
			vehicles.Master.VehicleRelationshipForTesting = VehicleRelationshipType.None;
			AssertEquals("VehicleRelationshipType.None does not allow new row", false, vehicles.AllowNew);
		}

		public void TestAllowNewWhenVehicleRelationshipIsOne()
		{
			var vehicles = GetCollectionToTestRelationship();
			vehicles.Master.VehicleRelationshipForTesting = VehicleRelationshipType.One;
			AssertEquals("VehicleRelationshipType.One allows new row when count is 0", true, vehicles.AllowNew);

			_ = vehicles.AddNew();
			AssertEquals("VehicleRelationshipType.One does not allow new row when count is not 0", false, vehicles.AllowNew);
		}

		public void TestAllowNewWhenVehicleRelationshipIsMany()
		{
			var vehicles = GetCollectionToTestRelationship();
			vehicles.Master.VehicleRelationshipForTesting = VehicleRelationshipType.Many;
			AssertEquals("VehicleRelationshipType.Many allows new row when count is 0", true, vehicles.AllowNew);

			_ = vehicles.AddNew();
			AssertEquals("Vehicles Count is 1", 1, vehicles.Count);
			AssertEquals("VehicleRelationshipType.Many allows new row when count is 1", true, vehicles.AllowNew);

			_ = vehicles.AddNew();
			AssertEquals("Vehicles Count is 2", 2, vehicles.Count);
			AssertEquals("VehicleRelationshipType.Many allows new row when count is 2", true, vehicles.AllowNew);
		}

		ICusVehicleCollection<CusVehicle, BaseJobComInvoiceLineForTesting> GetCollectionToTestRelationship()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLineForTesting>();
			return invoiceLine.Vehicles;
		}
	}
}

