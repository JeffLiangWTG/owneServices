using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class IDataContextDataObjectExtensionsTest : TestCase
	{
		#region TestContainsHoldCode

		public void TestContainsHoldCode()
		{
			var dataContext = DataContextFactory.New();
			AssertEquals(false, dataContext.ContainsHoldCode());

			dataContext.SetWorkflowInfo(new WorkflowInfo());
			AssertEquals(false, dataContext.ContainsHoldCode());

			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			AssertEquals(false, dataContext.ContainsHoldCode());

			dataContext = DataContextFactory.New();
			var bwrRole = new RecipientRoleDetail { Type = RecipientRoleType.BWR };
			var bcoRoleWithHold = new RecipientRoleDetail { Type = RecipientRoleType.BCO, ServiceCode = ServiceCodeType.HLD };
			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { bwrRole, bcoRoleWithHold } });
			AssertEquals("Second Recipient Role has Hold Code.", true, dataContext.ContainsHoldCode());
		}

		#endregion

		#region TestIsWarehouseBondedChangeOfOwnership

		public void TestIsWarehouseBondedChangeOfOwnership()
		{
			AssertEquals(false, ((IDataContextDataObject)null).IsWarehouseBondedChangeOfOwnership());

			var dataContext = DataContextFactory.New();
			AssertEquals(false, dataContext.IsWarehouseBondedChangeOfOwnership());

			dataContext.SetWorkflowInfo(new WorkflowInfo());
			AssertEquals(false, dataContext.IsWarehouseBondedChangeOfOwnership());

			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			AssertEquals(false, dataContext.IsWarehouseBondedChangeOfOwnership());

			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BCO }.ToRecipientRoleDetails() });
			AssertEquals("Second Recipient Role is BCO.", true, dataContext.IsWarehouseBondedChangeOfOwnership());
		}

		#endregion

		#region TestIsWarehouseBondedChangeOfRegime

		public void TestIsWarehouseBondedChangeOfRegime()
		{
			AssertEquals(false, ((IDataContextDataObject)null).IsWarehouseBondedChangeOfRegime());

			var dataContext = DataContextFactory.New();
			AssertEquals(false, dataContext.IsWarehouseBondedChangeOfRegime());

			dataContext.SetWorkflowInfo(new WorkflowInfo());
			AssertEquals(false, dataContext.IsWarehouseBondedChangeOfRegime());

			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			AssertEquals(false, dataContext.IsWarehouseBondedChangeOfRegime());

			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BCR }.ToRecipientRoleDetails() });
			AssertEquals("Second Recipient Role is BCR.", true, dataContext.IsWarehouseBondedChangeOfRegime());
		}

		#endregion

		#region TestIsWarehouseBondedChangeOfInventory

		public void TestIsWarehouseBondedChangeOfInventory()
		{
			AssertEquals(false, ((IDataContextDataObject)null).IsWarehouseBondedChangeOfInventory());

			var dataContext = DataContextFactory.New();
			AssertEquals(false, dataContext.IsWarehouseBondedChangeOfInventory());

			dataContext.SetWorkflowInfo(new WorkflowInfo());
			AssertEquals(false, dataContext.IsWarehouseBondedChangeOfInventory());

			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			AssertEquals(false, dataContext.IsWarehouseBondedChangeOfInventory());

			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BCO }.ToRecipientRoleDetails() });
			AssertEquals("Second Recipient Role is BCO.", true, dataContext.IsWarehouseBondedChangeOfInventory());

			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BCR }.ToRecipientRoleDetails() });
			AssertEquals("Second Recipient Role is BCR.", true, dataContext.IsWarehouseBondedChangeOfInventory());
		}

		#endregion
	}
}
