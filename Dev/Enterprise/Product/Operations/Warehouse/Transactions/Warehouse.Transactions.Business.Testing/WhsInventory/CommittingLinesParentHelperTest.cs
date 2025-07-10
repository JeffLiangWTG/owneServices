using System;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class CommittingLinesParentHelperTest<T> : WhsTestCaseWithFactory
			where T : BusinessObject, ILineWithCommittedPickLines
	{
		#region TestUncommitExcessInventoryAndCommitRequiredInventory

		public void TestUncommitExcessInventoryAndCommitRequiredInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var parent = GetNewParent(data);
			var helper = GetNewHelper(parent);
			var line1 = GetNewLine(parent, data.Part1, inventory.Location);
			var line2 = GetNewLine(parent, data.Part1, inventory.Location);
			line1.TransactionQty = 4m;
			line2.TransactionQty = 6m;
			AssertEquals("Precondition: No stock committed.", 0m, line1.GetQtyCommittedToThisLine());
			AssertEquals("Precondition: No stock committed.", 0m, line2.GetQtyCommittedToThisLine());

			using (SetIsInPreSaveValidationHack(parent))
			{
				helper.UncommitExcessInventoryAndCommitRequiredInventory(new[] { line1, line2 });
				AssertEquals("Stock should be committed.", 4m, line1.GetQtyCommittedToThisLine());
				AssertEquals("Stock should be committed.", 6m, line2.GetQtyCommittedToThisLine());
			}

			SetProduct(line1, data.Part2);
			AssertEquals("Stock should still be committed.", 4m, line1.GetQtyCommittedToThisLine());
			AssertEquals("Stock should still be committed.", 6m, line2.GetQtyCommittedToThisLine());

			using (SetIsInPreSaveValidationHack(parent))
			{
				helper.UncommitExcessInventoryAndCommitRequiredInventory(new[] { line1, line2 });
				AssertEquals("Product has changed so stock should be uncommitted.", 0m, line1.GetQtyCommittedToThisLine());
				AssertEquals("Stock should still be committed.", 6m, line2.GetQtyCommittedToThisLine());
			}
		}

		#endregion

		#region TestUncommitExcessInventoryAndCommitRequiredInventoryWithValidationSuspended

		public void TestUncommitExcessInventoryAndCommitRequiredInventoryWithValidationSuspended()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var parent = GetNewParent(data);
			var helper = GetNewHelper(parent);
			var line = GetNewLine(parent, data.Part1, inventory.Location);
			line.TransactionQty = 4m;
			AssertEquals("Precondition: No stock committed.", 0m, line.GetQtyCommittedToThisLine());

			parent.IgnoreValidationSuspended = true;
			bool isValidationSuspendedDuringProcess = false;
			line.PickLines.CountChanged += (sender, e) =>
			{
				isValidationSuspendedDuringProcess = parent.IsValidationSuspended;
			};

			using (SetIsInPreSaveValidationHack(parent))
			{
				helper.UncommitExcessInventoryAndCommitRequiredInventoryWithValidationSuspended(new[] { line });
				AssertEquals("Stock was committed.", 4m, line.GetQtyCommittedToThisLine());
				AssertEquals("Validation should be suspended during process.", true, isValidationSuspendedDuringProcess);
			}
		}

		#endregion

		#region Implementation

		/// <summary>
		/// This uses reflection to set the internal field to say that the BizO is in PreSaveValidation. This is done as there is
		/// no other way to set this flag in code other than calling RunPreSaveValidation(), which already calls 'UncommitExcessInventoryAndCommitRequiredInventory()'
		/// for that parent, and thus we are unable to test this class without doing the below.
		/// </summary>
		IDisposable SetIsInPreSaveValidationHack(BusinessObject parent)
		{
			var fieldInfo = typeof(BusinessObject).GetField("preSaveValidationDepthCount", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
			if ((int)fieldInfo.GetValue(parent) == 0)
			{
				fieldInfo.SetValue(parent, 1);
			}

			return new DisposableAction(() => fieldInfo.SetValue(parent, 0));
		}

		protected abstract BusinessObject GetNewParent(TestDataSimpleEnvironment data);
		protected abstract T GetNewLine(BusinessObject parent, OrgSupplierPart part, WhsLocation location);
		protected abstract CommittingLinesParentHelper<T> GetNewHelper(BusinessObject parent);

		protected abstract void SetProduct(T line, OrgSupplierPart differentPart);

		#endregion
	}
}
