using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	public abstract class CYDJobInvoicingPlugInTest<T> : TestCaseWithFactory
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		IJobInvoicingPlugIn
	{
		#region TestJobHeaderMembers

		public void TestJobHeaderMembers()
		{
			var yard = Factory.New<T>();
			yard[JobIDSchemaColumn] = "C1";

			var plugin = new CYDJobInvoicingPlugIn<T>(yard);

			AssertEquals("C1", ((IJobNumber)plugin).JobNumber);
			AssertEquals(Factory, ((IJobHeaderParentCore)plugin).Factory);
			AssertEquals(TableName, yard.TableName);
			AssertEquals(false, yard.IsInDatabase);
			AssertEquals(false, yard.IsDeleted);
		}

		#endregion

		protected abstract SchemaStringColumn JobIDSchemaColumn { get; }

		protected abstract string TableName { get; }
	}

	public class CYDPickupHeaderInvoicingPlugInTest : CYDJobInvoicingPlugInTest<CYDPickupHeader>
	{
		protected override SchemaStringColumn JobIDSchemaColumn => CYDPickupHeaderSchema.YPH_JobNumber;

		protected override string TableName => CYDPickupHeaderSchema.Constants.TableName;
	}

	public class CYDReleaseAdviceInvoicingPlugInTest : CYDJobInvoicingPlugInTest<CYDReleaseAdvice>
	{
		protected override SchemaStringColumn JobIDSchemaColumn => CYDReleaseAdviceSchema.YRE_JobNumber;

		protected override string TableName => CYDReleaseAdviceSchema.Constants.TableName;
	}

	public class CYDReceiveAdviceInvoicingPlugInTest : CYDJobInvoicingPlugInTest<CYDReceiveAdvice>
	{
		protected override SchemaStringColumn JobIDSchemaColumn => CYDReceiveAdviceSchema.YRA_JobNumber;

		protected override string TableName => CYDReceiveAdviceSchema.Constants.TableName;
	}

	public class CYDTransportationUnitJobInvoicingPlugInTest : CYDJobInvoicingPlugInTest<CYDTransportationUnit>
	{
		protected override SchemaStringColumn JobIDSchemaColumn => CYDTransportationUnitSchema.YTU_TransportationUnitID;

		protected override string TableName => CYDTransportationUnitSchema.Constants.TableName;
	}
}
