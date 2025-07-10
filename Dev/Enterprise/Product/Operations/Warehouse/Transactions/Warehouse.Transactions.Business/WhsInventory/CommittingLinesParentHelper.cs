using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class CommittingLinesParentHelper<T>
		where T : ILineWithCommittedPickLines
	{
		protected CommittingLinesParentHelper(BusinessObject parent)
		{
			Parent = Argument.NotNull(parent, "parent");
		}

		readonly BusinessObject Parent;

		/// <summary>
		/// If a property is validated inside the Inventory Committer when it makes changes it will not be run during 
		/// RunPreSaveValidation() and validation should be run when everything is set rather than half-way through the changes.
		/// </summary>
		public void UncommitExcessInventoryAndCommitRequiredInventoryWithValidationSuspended(IEnumerable<T> lines)
		{
			var ignore = Parent.IgnoreValidationSuspended;
			if (ignore)
			{
				Parent.IgnoreValidationSuspended = false;
			}

			try
			{
				using (Parent.GetValidationSuspender())
				{
					UncommitExcessInventoryAndCommitRequiredInventory(lines);
				}
			}
			finally
			{
				if (ignore)
				{
					Parent.IgnoreValidationSuspended = true;
				}
			}
		}

		/// <summary>
		/// Only use this when Finalising the Parent
		/// </summary>
		public void UncommitExcessInventoryAndCommitRequiredInventory(IEnumerable<T> lines)
		{
			var factory = Parent.Factory;
			foreach (var line in lines)
			{
				factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, line.PK); // Tested in TransferEntryForm
				factory.AddFetchHint(WhsDocketLineSchema.WE_WE_MatchingLine, line.PK);
			}

			UncommitOverPickedOrNonMatchingInventory(lines); // freeing unmatched or over-picked inventory in case other Transaction Lines will need to commit it.
			CommitInventory(lines); // Committing Inventory sets Per Package Quantity for US Bonded Transactions which is required during validation
		}

		void UncommitOverPickedOrNonMatchingInventory(IEnumerable<T> lines)
		{
			foreach (var line in lines)
			{
				UncommitOverPickedOrNotMatchingInventory(line);
			}
		}

		void CommitInventory(IEnumerable<T> lines)
		{
			foreach (var line in lines)
			{
				CommitInventory(line);
			}
		}

		protected abstract void UncommitOverPickedOrNotMatchingInventory(T line);
		protected abstract void CommitInventory(T line);
	}
}
