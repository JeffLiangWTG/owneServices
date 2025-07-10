using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Bonded;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOutwardsProcessor
	{
		#region Process

		public IWhsWarehouseTransaction Process(BusinessObjectFactory factory, IWhsWarehouseTransaction input, bool continueIfError)
		{
			Initialise(factory, input, continueIfError);
			ProcessCore();
			return (input.HasErrors && !continueIfError) ? input : Output;
		}

		protected virtual void ProcessCore()
		{
			// Sequence:	
			//	1.	Delete all data created from a previous run for the same job (ExternalPK).
			//	2.	Ensure all mandatory data has been supplied in the input transaction.
			//	3.	Create orders to enable the picking of requested stock.
			//	4.	Use the standard picking engine to pick the orders just created.
			//	5.	Build the output transaction from the pick details and return it to user. 

			//	Important: 
			//		This processor does not implement any picking logic. This processor only builds 
			//		orders, which are then picked using the standard picking engine.
			//
			//		In the case of errors, the original input transaction is returned with errors 
			//		describing the problem.

			DeletePreviousData();
			CheckForRequiredData();
			CreateOrders();
			PickOrders();
			BuildOutput();
		}

		#endregion

		#region Initialise

		protected virtual void Initialise(BusinessObjectFactory factory, IWhsWarehouseTransaction input, bool continueIfError)
		{
			if (input == null)
			{
				throw new ArgumentNullException(nameof(input));
			}

			if (factory == null)
			{
				factory = new BusinessObjectFactory((NoResString)"Factory cannot be null"); // Developers related message
			}

			this.Factory = factory;
			this.Input = input;
			this.ContinueIfError = continueIfError;
			OutputClient = (OrgHeader)input.Client;

			OrderList = new WhsOrderCollection(factory, new AdhocCollectionRelationship(typeof(WhsOrder)));
			Output = GetOutput();
		}

		#endregion

		#region Delete Previous Data

		protected virtual void DeletePreviousData()
		{
			WhsOutwardsProcessorCancel processor = new WhsOutwardsProcessorCancel();
			processor.Cancel(Factory, Input.ExternalPK);

			//#warning to be implemented on IWhsWarehouseTransaction implemntation
			Input.Problems.ErrorList.Clear();
			if (Input.Lines != null)
			{
				foreach (IWhsWarehouseTransactionLine line in Input.Lines)
				{
					line.WarehouseProblems.ErrorList.Clear();
					line.QuantityProblems.ErrorList.Clear();
					line.EntryKeyProblems.ErrorList.Clear();
					line.PartAttrib1Problems.ErrorList.Clear();
					line.PartAttrib2Problems.ErrorList.Clear();
					line.PartAttrib3Problems.ErrorList.Clear();
				}
			}
		}

		#endregion

		#region Validation

		protected virtual void CheckForRequiredData()
		{
			if (Input.Lines == null || Input.Lines.Count < 1)
			{
				Input.Problems.ErrorList.Add(new NoLinesError().Message);
			}
			else
			{
				foreach (IWhsWarehouseTransactionLine line in Input.Lines)
				{
					if (line.EntryKey.IsEmpty && line.PartAttrib1.IsEmpty && line.PartAttrib2.IsEmpty && line.PartAttrib3.IsEmpty)
					{
						if (line.Product == null)
						{
							line.QuantityProblems.ErrorList.Add(new MissingLineDataError().Message);
						}
						else if (line.Quantity == 0m)
						{
							line.QuantityProblems.ErrorList.Add(new MissingQuantityError().Message);
						}
					}
				}
			}
		}

		protected virtual void BuildOrdersValidation()
		{
		}

		#endregion

		#region Build Orders

		protected void CreateOrders()
		{
			// build orders and order lines so that stock required by the input transaction can be picked

			if (ProcessCanContinue && Input.Lines != null)
			{
				foreach (IWhsWarehouseTransactionLine line in Input.Lines)
				{
					BuildOrderLines(line);
				}

				BuildOrdersValidation();
			}
		}

		void BuildOrderLines(IWhsWarehouseTransactionLine line)
		{
			// convert product from dbo.OrgSupplierPart to OrgSupplierPart
			OrgSupplierPart prod = null;
			if (line.Product != null)
			{
				prod = Factory.Load<OrgSupplierPart>(line.Product.PK);
			}

			// load all stock, in all warehouses, that matches attributes on Line
			var query = WhsInventoryFilterBuilder.BuildFilter((OrgHeader)Input.Client, prod, null, null, null, "", WhsBondedWarehouseAttribute.BuildKey(line.EntryKey, line.EntryLineNumber),
				ZDate.Empty, ZDate.Empty, line.PartAttrib1, line.PartAttrib2, line.PartAttrib3, line.SerialNumber, ZDateTimeOffset.Empty);

			var inventories = Factory.Load<WhsInventoryView>(query);

			foreach (var inv in inventories)
			{
				Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, inv.WI_WE_InDocketLine);
				Factory.AddFetchHint(OrgSupplierPartSchema.PK, inv.WI_OP);
			}

			foreach (var inv in inventories)
			{
				foreach (var pickLine in inv.AllPickLines)
				{
					Factory.AddFetchHint(WhsDocketLineSchema.PK, pickLine.WZ_WE_TransactionLine);
				}
			}

			// filter out stock in unwanted warehouses, and provide a list of warehouses that may be 
			// holding the stock the input transaction is requesting. this list of warehouses can then
			// be shown to the user or logged by a batch process.
			var stockBeforeWarehousesFiltered = inventories.Any(i => i.WI_AvailableToPickQuantity > 0);

			// optimize the inventory collection order for pick allocation
			Array.Sort(inventories, new SortInventoryForPicking());

			var warehousesWithStock = GetListOfWarehousesWithMatchingStock(line, inventories);
			var filteredInventories = FilterWarehouses(line, inventories);
			var unitsPickable = filteredInventories.Sum(i => i.WI_AvailableToPickQuantity);

			// if we have enough stock then create the order lines, else error
			if (unitsPickable > 0 && unitsPickable >= line.Quantity)
			{
				CreateOrderLines(line, filteredInventories);
			}
			else
			{
				AddShortfallError(line, unitsPickable, stockBeforeWarehousesFiltered, warehousesWithStock);
			}
		}

		void CreateOrderLines(IWhsWarehouseTransactionLine line, IEnumerable<WhsInventoryView> inventories)
		{
			OrderLineAllocationData allocationData = new OrderLineAllocationData(line, inventories);

			BuildOrderLinesFromStockBasedOnFIFO(allocationData);

			if (allocationData.QtyRemaining > 0m)
			{
				AddShortfallError(line, line.Quantity - allocationData.QtyRemaining, true, null);
			}
		}

		void BuildOrderLinesFromStockBasedOnFIFO(OrderLineAllocationData allocationData)
		{
			// build order lines to allocate the oldest stock first

			// this code loops thru inventory and accumulates units until the requested quantity can be fulfilled.
			// then an order line is created. more than one order line may be created if during the iteration there 
			// is a break in warehouse, product or entry key.

			// the inventory collection has been previously sorted into a sequence where the most desired 
			// stock is at the top of the collection - same sort as normal pick process - FIFO, so oldest
			// stock is allocated first, but possibly overriden by expiry / packing dates.

			foreach (WhsInventoryView inventory in allocationData.Inventory)
			{
				if (IsStockOkForAllocation(inventory))
				{
					allocationData.QtyAvailable = inventory.WI_AvailableToPickQuantity - GetUnitsAlreadyAllocated(inventory);
					if (allocationData.QtyAvailable > 0m && AccumulateQuantityAndCreateOrderLineIfRequired(allocationData, inventory))
					{
						break;
					}
				}
			}

			// create order line for stock allocated in above loop
			if (allocationData.QtyAllocated > 0m)
			{
				CreateOrderLine(allocationData);
			}
		}

		protected virtual bool IsStockOkForAllocation(WhsInventoryView inventory)
		{
			return true;
		}

		bool AccumulateQuantityAndCreateOrderLineIfRequired(OrderLineAllocationData allocationData, WhsInventoryView inventory)
		{
			// order lines must be broken down by at least warehouse, product and entry key
			// we need to break down by entry key because they are treated differently than part attributes.
			// there must be a transaction for all entry key movements, so the entry key must be filled in on
			// the order line, even if it wasn't specifed on the input transaction, where as part attributes 
			// can be left blank and filled on WhsAttributes

			if (inventory.Warehouse != allocationData.PrevWarehouse ||
				inventory.SupplierPart != allocationData.PrevProduct ||
				inventory.WI_BondedEntryKey != allocationData.PrevEntryKey)
			{
				// break in critical fields, so create an order line for what we have accumulated so far
				CreateOrderLine(allocationData);

				allocationData.PrevWarehouse = inventory.Warehouse;
				allocationData.PrevProduct = inventory.SupplierPart;
				allocationData.PrevEntryKey = inventory.WI_BondedEntryKey;
			}

			// accumulate units from inventory until we have fulfilled the qty ordered
			allocationData.QtyAllocated += allocationData.QtyAvailable;

			// return true if enough units have been allocated
			return (allocationData.QuantitySpecified && allocationData.QtyAllocated >= allocationData.QtyRemaining);
		}

		void CreateOrderLine(OrderLineAllocationData allocationData)
		{
			if (allocationData.QtyAllocated > 0m)
			{
				ZDecimal quantity = allocationData.QuantitySpecified ? (ZDecimal)System.Math.Min(allocationData.QtyAllocated, allocationData.QtyRemaining) : allocationData.QtyAllocated;
				ZString entryKey = allocationData.PrevEntryKey; // WhsBondedWarehouseAttribute.BuildKey(Line.EntryKey, Line.EntryLineNumber); 

				IWhsWarehouseTransactionLine line = allocationData.Line;
				CreateOrderLine(line, allocationData.PrevWarehouse, allocationData.PrevProduct, quantity, entryKey, line.PartAttrib1, line.PartAttrib2, line.PartAttrib3);

				allocationData.QtyRemaining -= quantity;
				allocationData.QtyAllocated = 0m;
			}
		}

		protected virtual void CreateOrderLine(IWhsWarehouseTransactionLine line, WhsWarehouse warehouse, OrgSupplierPart part, ZDecimal quantity, string entryKey, string partAttrib1, string partAttrib2, string partAttrib3)
		{
			WhsOrder order = GetOrder(warehouse);
			WhsOrderLine orderLine = GetMatchingOrderLine(order, part, entryKey, partAttrib1, partAttrib2, partAttrib3);
			if (orderLine == null)
			{
				orderLine = order.Lines.AddNew();
				orderLine.WE_OP = part.PK;
				orderLine.WE_BondedEntryKey = entryKey;
				orderLine.WE_PartAttrib1 = partAttrib1.ToUpper(Culture.Current);
				orderLine.WE_PartAttrib2 = partAttrib2.ToUpper(Culture.Current);
				orderLine.WE_PartAttrib3 = partAttrib3.ToUpper(Culture.Current);
				orderLine.WE_TransactionQuantity = quantity;
				orderLine.RelatedInLineForExternalProcessing = line; // currently unused
			}
			else
			{
				orderLine.WE_TransactionQuantity += quantity;
			}
		}

		WhsOrder GetOrder(WhsWarehouse warehouse)
			=> FindOrder(warehouse)
				?? CreateOrder(warehouse);

		WhsOrderLine GetMatchingOrderLine(WhsOrder order, OrgSupplierPart part, string entryKey, string partAttrib1, string partAttrib2, string partAttrib3)
		{
			WhsOrderLine result = null;

			foreach (WhsOrderLine line in order.Lines)
			{
				if (line.WE_OP == part.PK && line.WE_BondedEntryKey == entryKey &&
					line.WE_PartAttrib1 == partAttrib1 && line.WE_PartAttrib2 == partAttrib2 &&
					line.WE_PartAttrib3 == partAttrib3)
				{
					result = line;
					break;
				}
			}

			return result;
		}

		ZDecimal GetUnitsAlreadyAllocated(WhsInventoryView inventory)
		{
			ZDecimal unitsAlreadyAllocated = 0m;

			foreach (WhsOrder order in OrderList)
			{
				foreach (WhsOrderLine orderLine in order.Lines)
				{
					if (orderLine.Docket.WD_WW_Whs == inventory.Warehouse.PK &&
						orderLine.WE_OP == inventory.WI_OP &&
						orderLine.WE_BondedEntryKey == inventory.WI_BondedEntryKey &&
						(orderLine.WE_PartAttrib1.IsEmpty || orderLine.WE_PartAttrib1 == inventory.WI_PartAttrib1) &&
						(orderLine.WE_PartAttrib2.IsEmpty || orderLine.WE_PartAttrib2 == inventory.WI_PartAttrib2) &&
						(orderLine.WE_PartAttrib3.IsEmpty || orderLine.WE_PartAttrib3 == inventory.WI_PartAttrib3))
					{
						unitsAlreadyAllocated += orderLine.WE_TransactionQuantity;
					}
				}
			}

			return unitsAlreadyAllocated;
		}

		WhsOrder CreateOrder(WhsWarehouse warehouse)
		{
			WhsOrder order = OrderList.AddNew();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = OutputClient.PK;
			order.WD_ExWhsJobGuid = Input.ExternalPK;
			order.WD_ExternalReference = CalcUniqueReference();
			order.ConsigneePK = OutputClient.PK;
			order.ConsigneeAddressPK = OutputClient.Addresses.MainAddress.PK;
			order.WD_RequiredDate = warehouse.GetWarehouseBranchLocalDateTimeOffset(ZDateTime.Today.AddDays(1));
			order.WD_DocketSubType = OrderType.Codes.Customs;
			return order;
		}

		StringCollection GetListOfWarehousesWithMatchingStock(IWhsWarehouseTransactionLine line, IEnumerable<WhsInventoryView> inventories)
		{
			var userSpecifiedWarehouse = FindWarehouse(line.Warehouse);
			var warehousesWithStock = new List<string>();

			foreach (var inventory in inventories)
			{
				var warehouse = inventory.Warehouse;
				if (warehouse != userSpecifiedWarehouse &&
					!warehousesWithStock.Contains(warehouse.WW_WarehouseNameMultilingual))
				{
					warehousesWithStock.Add(warehouse.WW_WarehouseNameMultilingual);
				}
			}

			warehousesWithStock.Sort();

			var result = new StringCollection();
			result.AddRange(warehousesWithStock.ToArray());

			return result;
		}

		string CalcUniqueReference()
		{
			string reference = Input.Reference.IsEmpty ? Input.ExternalPK.ToString() : Input.Reference.ToString();
			reference += "-" + OrderList.Count.ToString(Culture.Invariant);
			int len = reference.Length;
			int maxLen = WhsDocketSchema.WD_ExternalReference.MaxLength;
			return reference.Substring(System.Math.Max(0, len - maxLen), System.Math.Min(len, maxLen));
		}

		IEnumerable<WhsInventoryView> FilterWarehouses(IWhsWarehouseTransactionLine line, IReadOnlyList<WhsInventoryView> inventories)
		{
			// filter out warehouses not specified by user
			var userSpecifiedWarehouse = FindWarehouse(line.Warehouse);

			if (inventories.Count > 0 && (OutputClient == null || userSpecifiedWarehouse != null))
			{
				if (OutputClient == null)
				{
					OutputClient = inventories[0].Client;
				}

				inventories = (from inv in inventories
											 let location = inv.Location
											 where location != null && (userSpecifiedWarehouse == null || location.Row.WR_WW_Whs == userSpecifiedWarehouse.PK)
											 select inv).ToArray();
			}

			return inventories;
		}

		#endregion

		#region Pick Orders

		void PickOrders()
		{
			if (ProcessCanContinue && OrderList.Count > 0)
			{
				// there will be one order per warehouse in OrderList
				// if one pick fails, then all picks are cancelled

				try
				{
					// #warning will need to change this try block to handle ContinueIfError properly

					foreach (WhsOrder order in OrderList)
					{
						var iDataImporting = (ISupportDataImporting)order;
						using (new DisposableAction(
							() => iDataImporting.IsImportingData = true,
							() => iDataImporting.IsImportingData = false))
						{
							// use the standard picking engine to pick the orders
							var pick = Factory.New<WhsPick>();
							pick.CreatePick_OBSOLETE(order);

							CheckPick(order);
						}
					}
				}
				catch (PickFailedException e)
				{
					Input.Problems.ErrorList.Add(Res.GetString("653f6709-3bd2-4602-8c03-371ee697a107", "Pick Process Failed: {0}", e.Message));
					CancelAllPicks();
				}
			}
		}

		void CheckPick(WhsOrder order)
		{
			if (order.Pick != null && !order.Pick.HasErrors)
			{
				order.FinaliseDocket();
			}

			if (!order.IsFinalised)
			{
				AddCouldNotFinaliseOrderError(order);
				CancelAllPicks();
			}
		}

		void CancelAllPicks()
		{
			foreach (WhsOrder order in OrderList)
			{
				WhsPick pick = order.Pick;
				if (pick != null)
				{
					pick.CancelPick();
				}
			}
		}

		#endregion

		#region Build Output

		void BuildOutput()
		{
			if (ProcessCanContinue)
			{
				BuildOutputLines();

				if (Output.Lines.Count > 0)
				{
					PostBuildOutputLines();
				}
				else
				{
					Input.Problems.ErrorList.Add(Res.GetString("1894d7a3-3c74-437c-81d8-4e737a222879", "No stock could be found"));
				}
			}
		}

		void BuildOutputLines()
		{
			// build the output transaction from the pick lines. this output transaction is
			// returned to the user of this processor suppling all relevant information

			List<WhsOrder> removeList = new List<WhsOrder>();
			foreach (WhsOrder order1 in OrderList)
			{
				if (order1.Pick != null)
				{
					foreach (BusinessObject data in GetCollectionToBuildOutput(order1))
					{
						Output.Lines.Add(BuildOutputLine(data));
					}
				}
				else
				{
					removeList.Add(order1);
				}
			}

			// Tidy up by removing orders with no pickings.
			foreach (WhsOrder order in removeList)
			{
				order.Delete();
			}
		}

		protected virtual void PostBuildOutputLines() { }

		protected IBusinessObjectCollection GetCollectionToBuildOutput(WhsOrder order)
		{
			return order.Lines;
		}

		protected virtual WhsWarehouseTransaction GetOutput()
		{
			return new WhsWarehouseTransaction();
		}

		protected virtual WhsWarehouseTransactionLineCollection GetOutputLines()
		{
			return new WhsWarehouseTransactionLineCollection();
		}

		protected WhsWarehouseTransactionLine BuildOutputLine(BusinessObject sourceData)
		{
			return new WhsBondedWarehouseTransactionLine((WhsOrderLine)sourceData);
		}

		#endregion

		#region Errors

		void AddShortfallError(IWhsWarehouseTransactionLine line, ZDecimal qtyFound, bool stockBeforeWarehousesFiltered, StringCollection warehousesWithStock)
		{
			AddLineError(line, new ShortfallError(qtyFound, stockBeforeWarehousesFiltered, warehousesWithStock).Message);
		}

		void AddCouldNotFinaliseOrderError(WhsOrder order)
		{
			string error = Res.GetString("e3d47ed7-b09d-4cb3-837d-573283d89689", "Could not finalize Order");
			if (order.Notifications.GetErrors().GetUniqueMessageList().Length > 0)
			{
				error += ": " + order.Notifications.GetErrors().GetUniqueMessageList()[0];
			}
			Input.Problems.ErrorList.Add(error);
		}

		void AddLineError(IWhsWarehouseTransactionLine line, string message)
		{
			if (line.Quantity > 0)
			{
				line.QuantityProblems.ErrorList.Add(message);
			}

			if (!line.PartAttrib1.IsEmpty)
			{
				line.PartAttrib1Problems.ErrorList.Add(message);
			}

			if (!line.PartAttrib2.IsEmpty)
			{
				line.PartAttrib2Problems.ErrorList.Add(message);
			}

			if (!line.PartAttrib3.IsEmpty)
			{
				line.PartAttrib3Problems.ErrorList.Add(message);
			}

			if (!line.EntryKey.IsEmpty)
			{
				line.EntryKeyProblems.ErrorList.Add(message);
			}

			if (line.Warehouse != null)
			{
				line.WarehouseProblems.ErrorList.Add(message);
			}
		}

		#endregion

		#region Implementation

		WhsOrder FindOrder(WhsWarehouse whs)
		{
			WhsOrder result = null;
			foreach (WhsOrder order in OrderList)
			{
				if (order.WD_WW_Whs == whs.PK)
				{
					result = order;
					break;
				}
			}
			return result;
		}

		// find warehouse from dbo.orgaddress
		WhsWarehouse FindWarehouse(IOrgAddress address)
		{
			WhsWarehouse result = null;
			if (address != null)
			{
				ZQuery filter = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, address.PK);
				result = Factory.LoadTop1<WhsWarehouse>(filter);
			}
			return result;
		}

		protected virtual bool ProcessCanContinue
		{
			get { return ContinueIfError || !Input.HasErrors; }
		}

		public class OrderLineAllocationData
		{
			public OrderLineAllocationData(IWhsWarehouseTransactionLine line, IEnumerable<WhsInventoryView> inventories)
			{
				this.Line = line;

				QtyRemaining = line.Quantity;
				QuantitySpecified = (line.Quantity > 0m);
				QtyAllocated = 0m;
				QtyAvailable = 0m;

				var inventoriesArray = inventories.ToArray();
				if (inventoriesArray.Length > 0)
				{
					var inventory = inventoriesArray[0];
					PrevWarehouse = inventory.Warehouse;
					PrevProduct = inventory.SupplierPart;
					PrevEntryKey = inventory.WI_BondedEntryKey;
				}

				Inventory = inventoriesArray;
			}

			public ZDecimal QtyRemaining;
			public ZDecimal QtyAllocated;
			public ZDecimal QtyAvailable;
			public ZString PrevEntryKey;
			public WhsWarehouse PrevWarehouse;
			public OrgSupplierPart PrevProduct;
			public readonly bool QuantitySpecified;
			public IEnumerable<WhsInventoryView> Inventory { get; }
			public readonly IWhsWarehouseTransactionLine Line;
		}

		#region OrderList
#if DEBUG
		public
#else
		protected 
#endif
 WhsOrderCollection OrderList;

		#endregion

		protected IWhsWarehouseTransaction Input;
		protected WhsWarehouseTransaction Output;
		protected BusinessObjectFactory Factory;
		protected bool ContinueIfError;
		protected OrgHeader OutputClient;

		#endregion
	}

	public class WhsOutwardsProcessorCancel
	{
		#region Cancel

		public void Cancel(BusinessObjectFactory factory, ZGuid externalPK)
		{
			this.Factory = factory;
			this.ExternalPK = externalPK;

			DeleteDataCreatedByExternalJob();
		}

		#endregion

		#region Implementation

		protected virtual void DeleteDataCreatedByExternalJob()
		{
			CancelPreviousPicksAndOrders();
		}

		void CancelPreviousPicksAndOrders()
		{
			var filter = new ZQuery(WhsDocketSchema.WD_ExWhsJobGuid, SQLComparisonOperator.Equal, ExternalPK);
			var orders = new WhsOrderCollection(Factory, filter);

			foreach (WhsOrder order in orders)
			{
				var pick = order.Pick;
				if (pick != null && !pick.IsCancelled)
				{
					pick.CancelPick();
					pick.Delete();
				}
			}

			orders.DeleteAll();
		}

		protected ZGuid ExternalPK;
		protected BusinessObjectFactory Factory;

		#endregion
	}

	#region Errors

	public abstract class Error
	{
		public abstract string Message { get; }
	}

	public class NoLinesError : Error
	{
		public override string Message { get { return Res.GetString("f0061ef1-5a80-4333-9075-ba8c9e0e144b", "The entry must have at least one line"); } }
	}

	public class MissingLineDataError : Error
	{
		public override string Message { get { return Res.GetString("0cf116a6-2d30-4460-9443-56facfebb5f1", "This line is missing the product, entry number or part attribute. A line must have at least the product or entry number or a part attribute before it can be processed"); } }
	}

	public class MissingQuantityError : Error
	{
		public override string Message { get { return Res.GetString("21687802-dd9a-488e-b80a-788363ff8733", "If a product is selected then you must also enter a quantity"); } }
	}

	public class ShortfallError : Error
	{
		public ShortfallError()
			: this(0m)
		{
		}

		public ShortfallError(ZDecimal qtyFound)
			: this(qtyFound, false, null)
		{
		}

		public ShortfallError(ZDecimal qtyFound, bool stockFoundBeforeWarehousesFiltered, StringCollection warehousesWithStock)
		{
			fMessage = Res.GetString("7aad8e61-3c04-44a1-87ac-914035b496ed", "Not enough stock could be found to fulfill this line. {0} units were found.", qtyFound);
			if (stockFoundBeforeWarehousesFiltered && warehousesWithStock != null)
			{
				if (warehousesWithStock.Count > 0)
				{
					string error = " " + Res.GetString("9394857c-7348-4c17-b5e4-c4699e1b0b7a", "Some stock was found in the following warehouses:") + " ";
					foreach (string whs in warehousesWithStock)
					{
						error += whs + ", ";
					}

					fMessage += error.Substring(0, error.Length - 2);
				}
			}
		}

		public override string Message { get { return fMessage; } }
		readonly string fMessage;
	}

	[Serializable]
	public class CouldNotProcessReceiveException : WhsException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developers related message")]
		public CouldNotProcessReceiveException()
			: base("Could not setup Receive Data")
		{
		}

#if NETFRAMEWORK
		protected CouldNotProcessReceiveException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion
}
