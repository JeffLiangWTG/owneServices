using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class TransitJobInvoicingPlugInTest<T> : TestCaseWithFactory
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ITransitJobInvoicingPlugIn
	{
		#region TestJobHeaderMembers

		public void TestJobHeaderMembers()
		{
			var consignment = Factory.New<T>();
			consignment[JobIDSchemaColumn] = "C1";
			var plugin = new TransitJobInvoicingPlugIn<T>(consignment);
			AssertEquals("C1", ((IJobNumber)plugin).JobNumber);
			AssertEquals(Factory, ((IJobHeaderParentCore)plugin).Factory);
			AssertEquals(consignment.TableName, ((IJobHeaderParentCore)plugin).TableName);
			AssertEquals(consignment.IsInDatabase, ((IJobHeaderParentCore)plugin).IsInDatabase);
			AssertEquals(consignment.IsDeleted, ((IJobHeaderParent)plugin).IsDeleted);
		}

		#endregion

		protected virtual SchemaStringColumn JobIDSchemaColumn { get; }
	}

	public class WhsItemReceiveConsignmentInvoicingPlugInTest : TransitJobInvoicingPlugInTest<WhsItemReceiveConsignment>
	{
		protected override SchemaStringColumn JobIDSchemaColumn => WhsItemReceiveConsignmentSchema.WRC_JobID;
	}

	public class WhsItemReceiveTransportationUnitInvoicingPlugInTest : TransitJobInvoicingPlugInTest<WhsItemReceiveTransportationUnit>
	{
		protected override SchemaStringColumn JobIDSchemaColumn => WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber;
	}

	public class WhsItemDispatchConsignmentInvoicingPlugInTest : TransitJobInvoicingPlugInTest<WhsItemDispatchConsignment>
	{
		protected override SchemaStringColumn JobIDSchemaColumn => WhsItemDispatchConsignmentSchema.WDC_JobID;
	}

	public class WhsItemDispatchTransportationUnitInvoicingPlugInTest : TransitJobInvoicingPlugInTest<WhsItemDispatchTransportationUnit>
	{
		protected override SchemaStringColumn JobIDSchemaColumn => WhsItemDispatchTransportationUnitSchema.WDH_ReferenceNumber;
	}
}
