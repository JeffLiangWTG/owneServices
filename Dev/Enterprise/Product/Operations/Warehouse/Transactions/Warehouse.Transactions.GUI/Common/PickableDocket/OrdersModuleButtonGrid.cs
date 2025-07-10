using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public abstract class OrdersModuleButtonGrid : ZModuleButtonGrid
	{
		protected abstract WhsPick GetPick();

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			SelectFirstRowIfOnlyRowInGrid();

			var selected = InnerGrid.SelectedElements;
			if (selected.Length == 0)
			{
				ShowNotSelectedMessage();
			}
			else if (IsDetachAllowed())
			{
				var errors = ZString.Empty;
				var detachedBusinessObjects = new List<BusinessObject>();
				var (editingItems, newItems, changedItems) = FilterSelectedElements(selected);
				if (editingItems.IsEmpty)
				{
					if (ShowConfirmDetachMessage() == DialogResult.Yes)
					{
						var (errorsFound, objectsToDetach) = DetachOrders(selected, newItems, changedItems);
						detachedBusinessObjects.AddRange(objectsToDetach);
						errors = errorsFound;
					}

					ShowDeleteError(errors);
				}

				ShowEditingError(editingItems);
				OnDetached(new ModuleButtonGridOnDetachedEventArgs(detachedBusinessObjects.Distinct().ToArray()));
			}
		}

		DialogResult ShowConfirmDetachMessage()
		{
			var result = DialogResult.Yes;
			if (DetachMessage != null)
			{
				result = Globals.Message.Show(Res.GetString("3899fcdd-381d-449c-afc8-40ddcc8e18c8", "{0}\r\nAll unsaved changes in detached orders will be canceled.", DetachMessage.Caption),
					Res.GetString("38c4e74a-2ba3-493d-ade2-c50505f77152", "Confirm Order Detach..."), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
			}

			return result;
		}

		(ZString, IEnumerable<BusinessObject>) DetachOrders(BusinessObject[] selected, IList<ZGuid> newItems, IList<ZGuid> changedItems)
		{
			var errors = string.Empty;
			var ordersToDetach = new List<BusinessObject>();
			foreach (WhsPickableDocket order in selected.ToArray())
			{
				switch (order)
				{
					case WhsPickableDocket finalisedOrder when finalisedOrder.IsFinalised:
						errors += System.Environment.NewLine + finalisedOrder.HumanReadableName + Res.GetString("debfc692-2be8-4ecc-8824-b3bfec5f527b", ": This order cannot be detached because it is finalized");
						break;

					case WhsPickableDocket newOrder when newItems.Contains(newOrder.PK):
						try
						{
							newOrder.Delete();
							DataSource.HasChanges = true;
						}
						catch (CannotDeleteException e1)
						{
							errors += System.Environment.NewLine + newOrder.HumanReadableName + ": " + e1.Message;
						}

						break;

					case WhsPickableDocket changedOrder when changedItems.Contains(changedOrder.PK):
						changedOrder.CancelChanges();
						(changedOrder as IBusinessObjectState).ClearHasChangesIncludingChildren();

						ordersToDetach.Add(changedOrder);
						DataSource.HasChanges = true;
						break;

					default:
						ordersToDetach.Add(order);
						DataSource.HasChanges = true;
						break;
				}
			}

			GetPick().RemoveOrders(ordersToDetach.Cast<WhsPickableDocket>());
			ordersToDetach.ForEach(o => RemoveWeakReferences(o));

			return (errors, ordersToDetach);
		}
	}
}
