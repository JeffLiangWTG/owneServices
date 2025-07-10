using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	/// <summary>
	/// Tests for this class are in WhsPickableDocketTest and WhsOrderTest.
	/// </summary>
	public class BOMAutoCreateHelper
	{
		public BOMAutoCreateHelper(WhsPickableDocket parent)
		{
			Parent = parent;
		}

		protected BusinessObjectFactory Factory => Parent.Factory;
		protected readonly WhsPickableDocket Parent;

		#region IsAutoCreatingWorkOrders

		public bool IsAutoCreatingWorkOrders
		{
			get { return IsAutoCreatingWorkOrdersSemaphore.IsSuspended; }
		}

		AutoCreatingWorkOrdersSemaphore IsAutoCreatingWorkOrdersSemaphore
		{
			get
			{
				var semaphore = Factory.ServiceContainer.GetService<AutoCreatingWorkOrdersSemaphore>();
				if (semaphore == null)
				{
					semaphore = new AutoCreatingWorkOrdersSemaphore();
					Factory.ServiceContainer.AddService(semaphore);
				}
				return semaphore;
			}
		}

		class AutoCreatingWorkOrdersSemaphore : Semaphore, IService
		{
		}

		#endregion

		#region AutoCreateWorkOrders

		public void AutoCreateWorkOrders(INotifications notify)
		{
			AutoCreateWorkOrders(notify, true);
		}

		public void AutoCreateWorkOrders(INotifications notify, bool notifyIfNoWorkOrderRequired)
		{
			Argument.NotNull(notify, "notify");

			if (Parent.IsFinalised)
			{
				notify.AddError(Res.GetString("6f892cb5-275a-48c8-a183-d9d156861a90", "No Work Orders were created because the Order is already Finalized."));
			}
			else if (Parent.IsCancelled)
			{
				notify.AddError(Res.GetString("08dbf24d-4138-4f80-ad53-e357eb744bf3", "No Work Orders were created because the Order is Canceled."));
			}
			else if (!Parent.CurrentWorkOrders.Any())
			{
				AutoCreateWorkOrdersCore(notify, notifyIfNoWorkOrderRequired);
			}
			else
			{
				var e = new QueryUserYesNoEventArgs(WorkOrdersAlreadyExistMessage, false);
				notify.QueryUser(e);

				if (e.Response)
				{
					if (IsAnyChildWorkOrderBeingAssembled())
					{
						notify.AddError(Res.GetString("953398e7-0be3-40cb-8c08-12bae712fbe3", "One or more Work Orders attached to the Order are in progress. Cannot cancel or re-create Work Orders."));
					}
					else
					{
						RemoveAndDeleteNonBuiltWorkOrders();
						AutoCreateWorkOrdersCore(notify, notifyIfNoWorkOrderRequired);
					}
				}
			}
		}

		void AutoCreateWorkOrdersCore(INotifications notify, bool notifyIfNoWorkOrderRequired)
		{
			Argument.NotNull(notify, "notify");
			if (Parent.CurrentWorkOrders.Any())
			{
				throw new ArgumentException("Cannot call AutoCreateWorkOrdersCore() when the Order has existing (current) Work Orders.");
			}

			using (new SemaphoreManager(IsAutoCreatingWorkOrdersSemaphore))
			{
				AutoCreateChildWorkOrders(Parent);
			}

			ReloadRelatedJobsOnNextSave = true;

			if (Parent.CurrentWorkOrders.Any())
			{
				foreach (var workOrder in Parent.CurrentWorkOrders)
				{
					Parent.RegisterEditableChildObject(workOrder);
				}

				notify.Notify(new InfoNotification(WorkOrderCreationSuccessMsg));
			}
			else if (notifyIfNoWorkOrderRequired)
			{
				notify.Notify(new InfoNotification(Res.GetString("094a66be-a36c-42cd-bebe-14dab8e2c375", "Order has no BOM Shortfalls and does not require a Work Order.")));
			}
		}

		public bool ReloadRelatedJobsOnNextSave;
		public static string WorkOrderCreationSuccessMsg
		{
			get { return Res.GetString("5cd762d7-8768-4257-b039-9e25531a3bdf", "Successfully created Work Orders to build all shortfall items.\r\nAfter Saving the Order all Work Orders will be available on the Related Jobs tab."); }
		}

		#endregion

		#region AutoCreateChildWorkOrders

		/// <summary>
		/// Creates WorkOrders for any buildable product that is in shortfall below the first.
		/// 
		/// eg. If we have:
		/// 
		///		Bike x2
		///			Engine x2
		///				Block x2
		///				Piston x8
		///					Piston Head x8
		///					Piston Ring x8
		///					Piston Crank x8
		///			Wheel x4
		///				Rim x2
		///				Tyre x2
		///				
		/// AutoCreateChildWorkOrders(Bike) will create Engine + Wheel + Piston work orders if they are in shortfall.
		/// </summary>
		void AutoCreateChildWorkOrders(WhsPickableDocket parentDocket)
		{
			AutoCreateChildWorkOrders(parentDocket, parentDocket.WD_ExternalReferenceSplit);
		}

		byte AutoCreateChildWorkOrders(WhsPickableDocket parentDocket, byte currentSplitNo)
		{
			var nextSplitNo = (byte)(currentSplitNo + 1);

			foreach (WhsPickableDocketLine line in parentDocket.AllLines)
			{
				if (line.CanGenerateChildWorkOrder)
				{
					var shortfall = GetProductShortfall(line);
					if (shortfall > 0)
					{
						var result = Factory.New<WhsWorkOrder>();
						result.WD_WD_ParentDocket = parentDocket.PK;
						result.WD_OH_Client = Parent.WD_OH_Client;
						result.WD_WW_Whs = Parent.WD_WW_Whs;
						result.WD_ExternalReference = parentDocket.WD_ExternalReference;
						result.WD_ExternalReferenceSplit = nextSplitNo;
						result.WD_RequiredDate = parentDocket.WD_RequiredDate;

						var newLine = result.Lines.AddNew();
						newLine.WE_TransactionQuantity = shortfall;
						newLine.WE_OP = ZGuid.Empty;
						newLine.WE_OP = line.SupplierPart.PK; // to generate child lines

						nextSplitNo = AutoCreateChildWorkOrders(result, nextSplitNo); // recurse and build children
					}
				}
			}

			return nextSplitNo;
		}

		ZDecimal GetProductShortfall(WhsPickableDocketLine line)
		{
			ZDecimal result = 0m;

			var part = line.SupplierPart;
			if (part != null)
			{
				result = line.WE_TransactionQuantity - GetUnitsPickable(Parent.WD_OH_Client, part.PK, Parent.WD_WW_Whs, null);

				// also include items already picked
				if (line.PickableDocket.Pick != null)
				{
					result -= line.PickLines.GetQtyCommitted();
				}
			}

			return result;
		}

		internal decimal GetUnitsPickable(ZGuid clientPK, ZGuid partPK, ZGuid warehousePK, IPartAttributes attributesToMatch)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, clientPK);
			query.AddToFilter(WhsInventoryViewSchema.WI_OP, partPK);
			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

			var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsInventoryViewSchema.WI_WL);
			locationSubQuery.AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, warehousePK);
			query.AddSubQuery(locationSubQuery, JoinCondition.And);

			// Factory caches query results for same query text and it does not know that changing docketline values will affect WhsInventoryView.
			// so if we execute the same query a second time the factory will give old results instead of hitting the database.
			Factory.ClearQueryCache(WhsInventoryViewSchema.Constants.TableName);
			var inventories = Factory.Load<WhsInventoryView>(query);
			var inventoriesThatMatchAttribs = new List<WhsInventoryView>();

			var today = new Lazy<ZDateTime>(() => ZDateTime.Today);
			var isExpiryUsed = new Lazy<bool>(() =>
			{
				var client = Factory.Load<OrgHeader>(clientPK);
				return WhsProduct.GetWhsProduct(Factory, partPK).IsExpiryDateUsed(client);
			});

			// add fetch hints to avoid 1 DB hit per inventory
			foreach (var inv in inventories)
			{
				if (attributesToMatch == null ||
					(AttributeComparer.CompareWithIsEmptyCheck(attributesToMatch, inv)
					&&
					(!isExpiryUsed.Value || inv.WI_ExpiryDate > today.Value)))
				{
					inventoriesThatMatchAttribs.Add(inv);
					Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, inv.WI_WE_InDocketLine);
					Factory.AddFetchHint(OrgSupplierPartSchema.PK, inv.WI_OP);
				}
			}

			foreach (var inv in inventoriesThatMatchAttribs)
			{
				foreach (var pickLine in inv.AllPickLines)
				{
					Factory.AddFetchHint(WhsDocketLineSchema.PK, pickLine.WZ_WE_TransactionLine);
				}
			}

			return inventoriesThatMatchAttribs.Sum(i => i.WI_AvailableToPickQuantity);
		}

		#endregion

		#region RemoveAndDeleteNonBuiltWorkOrders

		void RemoveAndDeleteNonBuiltWorkOrders()
		{
			foreach (var workOrder in Parent.CurrentWorkOrders)
			{
				if (!workOrder.IsFinalisedOrCancelled)
				{
					workOrder.Delete();
				}
			}
		}

		#endregion

		#region WorkOrdersAlreadyExistMessage

		public string WorkOrdersAlreadyExistMessage
		{
			get
			{
				return Res.GetString("4436a5b1-cebd-4700-a364-91bdc61c6e7e", "Work Orders exist for this Order. Would you like to cancel the existing Work Orders and create new ones with updated build quantities?");
			}
		}

		#endregion

		#region IsAnyChildWorkOrderBeingAssembled

		public bool IsAnyChildWorkOrderBeingAssembled()
		{
			foreach (var workOrder in Parent.CurrentWorkOrders)
			{
				if ((workOrder.WD_DocketStatus != DocketStatus.Codes.Entered && workOrder.WD_DocketStatus != DocketStatus.Codes.New) || workOrder.BOM.IsAnyChildWorkOrderBeingAssembled())
				{
					return true;
				}
			}

			return false;
		}

		#endregion

#if DEBUG
		public IDisposable SuspendAutoCreatingWorkOrdersSemaphoreForTest() => new SemaphoreManager(IsAutoCreatingWorkOrdersSemaphore);
#endif
	}
}
