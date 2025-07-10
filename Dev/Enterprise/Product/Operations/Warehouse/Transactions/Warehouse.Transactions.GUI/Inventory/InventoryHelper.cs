using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public static class InventoryHelper
	{
		#region ViewReceipt

		public static void ViewReceipt(WhsDocketLine docketLine)
		{
			if (docketLine != null)
			{
				var docket = docketLine.DocketOriginal;

				if (docket != null)
				{
					IZForm form = ShowFormForDocket(docketLine, docket);

					#region Test
#if DEBUG
					lastShowForm = new IZForm[1] { form };
#endif
					#endregion
				}
			}
		}

		#endregion

		static IZForm ShowFormForDocket(WhsDocketLine docketLine, WhsDocket docket)
		{
			IZForm form = null;
			if (docket.WD_DocketType == CodeLists.DocketType.Codes.Receive)
			{
				form = ZControllerFactory.Create(ControllerIDs.WhsReceive).ShowEditForm((WhsReceive)docket);
				if (form != null) // form will be null if security denied
				{
					((ReceiveEntryForm)form).SelectInventoryLine(docketLine.PK);
				}
			}
			else if (docket.WD_DocketType == CodeLists.DocketType.Codes.Adjustment)
			{
				form = ZControllerFactory.Create(ControllerIDs.WhsAdjustment).ShowEditForm((WhsAdjustment)docket);
			}
			else if (docket.WD_DocketType == CodeLists.DocketType.Codes.Transfer)
			{
				form = ZControllerFactory.Create(ControllerIDs.WhsTransfer).ShowEditForm((WhsTransfer)docket);
			}
			return form;
		}

		#region ViewStatus

		public static void ViewStatus(WhsDocketLine docketLine, INotifications notifications)
		{
			if (docketLine != null)
			{
				var status = docketLine.WE_CurrentInventoryStatus;
				var (docket, message) = GetStatusMessage(status, docketLine);

				if (!status.Equals(InventoryStatus.Codes.Held))
				{
					QueryUserYesNoEventArgs eventArgs = new QueryUserYesNoEventArgs(string.Concat(message, "\r\n", Res.GetString("4C26B66C-D824-421F-B31E-524C91E08518", "Would you like to open the Job?")), true);
					notifications.QueryUser(eventArgs);
					if (eventArgs.Response)
					{
						IZForm form = null;
						if (docket.WD_DocketType != DocketType.Codes.Order)
						{
							form = ShowFormForDocket(docketLine, docket);
						}
						else
						{
							form = ZControllerFactory.Create(ControllerIDs.WhsOrder).ShowEditForm((WhsOrder)docket);
						}

						#region Test
#if DEBUG
						lastShowForm = new IZForm[1] { form };
#endif
						#endregion
					}
				}
				else
				{
					notifications.AddInformation(message);
				}
			}
		}

		#region GetOrderFromDocketLine

		static WhsDocket GetOrderFromDocketLine(WhsDocketLine docketLine)
		{
			var pickLinesQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLinesQuery.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, docketLine.PK);

			var orderLinesQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			orderLinesQuery.AddSubQuery(pickLinesQuery, JoinCondition.And);

			var orderQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			orderQuery.AddSubQuery(orderLinesQuery, JoinCondition.And);

			return docketLine.Factory.LoadTop1<WhsDocket>(orderQuery);
		}

		#endregion

		#region GetStatusMessage

		static (WhsDocket, string) GetStatusMessage(ZString status, WhsDocketLine docketLine)
		{
			var docket = new Lazy<WhsDocket>(() => docketLine.Docket);
			switch (status)
			{
				case InventoryStatus.Codes.Available:
					var docketOriginal = docketLine.DocketOriginal;
					return (docketOriginal, Res.GetString("CFC7D8DA-067C-4268-8817-B1DDBE89F819", "Inventory is {0} for Receipt {1}", docketLine.Lookups.InventoryStatuses.GetDescriptionFromCode(status), docketOriginal.WD_DocketID));
				case InventoryStatus.Codes.Received:
				case InventoryStatus.Codes.Arrived:
				case InventoryStatus.Codes.Pending:
					return (docket.Value, Res.GetString("CFC7D8DA-067C-4268-8817-B1DDBE89F819", "Inventory is {0} for Receipt {1}", docketLine.Lookups.InventoryStatuses.GetDescriptionFromCode(status), docket.Value.WD_DocketID));
				case InventoryStatus.Codes.Putaway:
					return (docket.Value, docket.Value.WD_DocketType == DocketType.Codes.Transfer
						? Res.GetString("29B3604B-D391-4819-89A8-0E3368C6C57E", "Inventory is Putaway for Putaway Transfer {0}", docket.Value.WD_DocketID)
						: Res.GetString("DF147BEB-A8DC-4CF6-A016-0F96EF2BFB5E", "Inventory is Putaway for Receive {0}", docket.Value.WD_DocketID));
				case InventoryStatus.Codes.Held:
					var collection = docketLine.Lookups.InventoryHeldCodeCollection.ToArray();
					var heldCodeDes = collection.FirstOrDefault(c => c.Code == docketLine.WE_WHC_NKCurrentInventoryHeldCode).Description;
					return string.IsNullOrEmpty(docketLine.WE_CurrentHoldReason)
						? (docket.Value, Res.GetString("E8079730-6F0C-4848-827F-71B6E459554D", "Inventory is Held with Hold Code {0}", heldCodeDes))
						: (docket.Value, Res.GetString("E64235D8-6792-40CA-8853-CF394FB7947B", "Inventory is Held with Hold Code {0} for Reason {1}", heldCodeDes, docketLine.WE_CurrentHoldReason));
				case InventoryStatus.Codes.InTransit:
					return (docket.Value, Res.GetString("6F5FADE1-6824-469E-BB57-5963D2FC21CC", "Inventory is In-Transit for Transfer {0}", docket.Value.WD_DocketID));
				case InventoryStatus.Codes.PuttingAway:
					return (docket.Value, Res.GetString("B531212C-FEAA-44E3-BDFE-6CAD6E226746", "Inventory is Putting Away for Putaway Transfer {0}", docket.Value.WD_DocketID));
				case InventoryStatus.Codes.Staged:
				case InventoryStatus.Codes.ReadyToPack:
					var order = GetOrderFromDocketLine(docketLine);
					return (order, Res.GetString("FDB334F3-0B90-4DBE-8DA1-65073BEDAA1B", "Inventory is {0} for Order {1}", docketLine.Lookups.InventoryStatuses.GetDescriptionFromCode(status), order.WD_DocketID));
				default:
					throw new ArgumentException($"Inventory status {status} is invalid.");
			}
		}

		#endregion

		#endregion

		public static void ViewReleaseForPackage(WhsOrder order)
		{
			IZForm form = null;

			form = ZControllerFactory.Create(ControllerIDs.WhsRelease).ShowEditForm(order.Pick);

			((ReleaseEntryForm)form).SetInitialOrderToSelectInGrid(order.PK);

			#region Test
#if DEBUG
			lastShowForm = new IZForm[1] { form };
#endif
			#endregion
		}

		public static void ViewRelease(WhsDocketLine inventoryLine)
		{
			if (inventoryLine.PickAllocations.Count > 0)
			{
				var pickAllocations = inventoryLine.PickAllocations;

				#region Test
#if DEBUG
				var formCount = 0;
				lastShowForm = new IZForm[pickAllocations.Count];
#endif
				#endregion

				if (pickAllocations.Count > 1)
				{
					Globals.Message.ShowInformation(Res.GetString("ebba0f17-fcfb-4db5-9edc-bf3e81856388", "This inventory is committed to {0} different picks. A Release form will be opened for each pick", pickAllocations.Count));
				}

				foreach (var pick in pickAllocations)
				{
					IZForm form;

					if (pick.IsWorkOrderPick)
					{
						form = ZControllerFactory.Create(ControllerIDs.WhsPicking).ShowEditForm(pick);
					}
					else
					{
						form = ZControllerFactory.Create(ControllerIDs.WhsRelease).ShowEditForm(pick);
					}

					#region Test
#if DEBUG
					lastShowForm[formCount++] = form;
#endif
					#endregion
				}
			}
			else
			{
				Globals.Message.ShowError(NoReleaseErrorMsg);
			}
		}

		public static void ViewCrossDock(WhsDocketLine inventoryLine)
		{
			if (inventoryLine.ReservedPickLines.Count > 0)
			{
				#region Test
#if DEBUG
				var formCount = 0;
				lastShowForm = new IZForm[inventoryLine.ReservedPickLines.Count];
#endif
				#endregion

				if (inventoryLine.ReservedPickLines.Count > 1)
				{
					Globals.Message.ShowInformation(Res.GetString("f886125d-2772-4edd-92da-a65de999852c", "This inventory is allocated to {0} different orders. An Order Entry form will be opened for each allocation", inventoryLine.ReservedPickLines.Count));
				}

				foreach (var pickLine in inventoryLine.ReservedPickLines)
				{
					var form = (OrderEntryForm)ZControllerFactory.Create(ControllerIDs.WhsOrder).ShowEditForm((WhsOrder)pickLine.DocketLine.Docket);

					#region Test
#if DEBUG
					lastShowForm[formCount++] = form;
#endif
					#endregion
				}
			}
			else
			{
				Globals.Message.ShowError(NoCrossDockErrorMsg);
			}
		}

		#region ShowWhyInventoryIsCommitted

		public static void ShowWhyInventoryIsCommitted(WhsDocketLine docketLine)
		{
			Globals.Message.ShowInformation(docketLine.WhyIsThisInventoryCommitted(), Res.GetString("9d81589d-dae0-4ee0-bb6f-66bd28a72b50", "Committed Units"));
		}

		public static void ShowWhyInventoryIsCommitted(WhsDocketLine[] docketLines)
		{
			var msgBuilder = new ZStringBuilder();
			foreach (var docketLine in docketLines)
			{
				msgBuilder.Append(docketLine.WhyIsThisInventoryCommitted());
			}

			Globals.Message.ShowInformation(msgBuilder.ToStringWithNewLineBetweenAppends(), Res.GetString("9d81589d-dae0-4ee0-bb6f-66bd28a72b50", "Committed Units"));
		}

		#endregion

		public static string NoReleaseErrorMsg => Res.GetString("348ae365-426d-40cc-94c9-a19faddbe55e", "This inventory item is not allocated to any picks.");
		public static string NoCrossDockErrorMsg => Res.GetString("a94354fc-35f4-4838-aa25-ca2c78f3fd66", "This inventory item is not allocated to any orders.");

#if DEBUG
		[ThreadStatic]
		public static IZForm[] lastShowForm;
#endif
	}
}
