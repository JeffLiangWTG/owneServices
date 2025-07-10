using System.Linq;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktakeWorkflowDescriptor))]
	class WhsStocktakeWorkflowDescriptorTest : WorkflowDescriptorTestCase<WhsStocktakeWorkflowDescriptor>
	{
		#region ExpectingTasksToBeCompanySpecific

		protected override bool ExpectingTasksToBeCompanySpecific => false;

		#endregion

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Warehouse Stocktake", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestDocumentBusinessContext

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.WhsStocktake, WorkflowDescriptor.DocumentBusinessContext.Single());
		}

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.WhsStocktakeWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		#endregion

		#region TestRequiresWarehouse

		protected override bool RequiresWarehouseExpectedResult => true;

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
			=> MessageRecipientPartyType.OrgProxy
			| MessageRecipientPartyType.Email
			| MessageRecipientPartyType.Client
			| MessageRecipientPartyType.Warehouse;

		#endregion

		#region Implementation

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var whs = Helper.CreateWarehouse("WHS");
			whs.WarehouseAddress.OA_OH = WarehouseOrg.PK;
			var stocktake = Helper.CreateWhsStocktake(ClientOrg, whs);
			return new IWorkflowProvider[] { stocktake };
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
