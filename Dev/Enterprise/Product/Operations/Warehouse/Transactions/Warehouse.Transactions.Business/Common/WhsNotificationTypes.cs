using System;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Serializable]
	public class ReceiveErrorTypes : WhsErrorTypes
	{
		public ReceiveErrorTypes(string message)
			: base(message)
		{
		}

		public static WhsErrorTypes NoLocationsDefined
		{
			get
			{
				return new ReceiveErrorTypes(Res.GetString("6a1d16f4-cbc2-4d81-9755-d418571aa573",
@"There are no locations defined for this warehouse, or there are no locations with a suitable status for putaway.
There needs to be at least one location in the warehouse that does NOT have a status of Held, Damaged or Void.
The Putaway cannot continue"));
			}
		}

		public static WhsErrorTypes CannotPerformThisOperationBecauseReceiveWasCreatedFromWorkOrder => new WhsErrorTypes(Res.GetString("0d877861-47a7-4a09-9dcc-eee06667d441", "Cannot perform this operation because the job was created from a Work Order."));
		public static WhsErrorTypes CannotPerformThisOperationBecauseReceiveWasCreatedFromPickOrder => new WhsErrorTypes(Res.GetString("362f6b78-df54-4695-8e78-8a9ecfcf14c0", "Cannot perform this operation because the job was created from a Pick Order."));
		public static WhsErrorTypes CannotPerformThisOperationBecauseDocketIsHeldByCustoms => new WhsErrorTypes(Res.GetString("315af6b1-cbeb-4104-bdf5-44dbdaf068a6", "Cannot perform this operation because the job is currently held by Customs."));
		public static new WhsErrorTypes NoLinesEntered { get { return new ReceiveErrorTypes(Res.GetString("0fb91d74-2f83-4ff1-831e-f1c0583e1a38", "Please enter the product lines before creating the putaway")); } }
		public static WhsErrorTypes NoPutawayCreatedBeforeCancelPutaway { get { return new WhsErrorTypes(Res.GetString("e4647be0-0b31-4497-9103-55dc1551d27d", "No Putaway has been created yet")); } }
		public static WhsErrorTypes NoPutawayCreatedBeforeFinalise { get { return new WhsErrorTypes(Res.GetString("24864232-0227-40a3-bfb9-f0f18269b5eb", "This docket cannot be finalized until a putaway is created")); } }
		public static WhsErrorTypes NoPutawayCreatedOnSplit { get { return new WhsErrorTypes(Res.GetString("9a11a3a7-fea7-4856-9dd1-82faea397465", "You cannot split inventory until the putaway is created")); } }
		public static WhsErrorTypes NoInventorySelected { get { return new WhsErrorTypes(Res.GetString("0a94ab20-5ae4-4ced-a26e-2a7443aa3761", "Please select and inventory line to split")); } }
		public static WhsErrorTypes PutawayZErrorMessageBox { get { return new WhsErrorTypes("ZErrorMessageBox", Res.GetString("b2bf7ec2-f5e3-413d-82d4-742801ce3df8", "Putaway")); } }
		public static WhsErrorTypes NotEveryLineHasLocationSetForSplit { get { return new WhsErrorTypes(Res.GetString("34d74818-b458-4a84-bd45-ea5936a590c5", "All locations must be assigned before you can split this receipt.")); } }
		public static WhsErrorTypes SplitByAreaTypeRequiresDifferentAreaTypes { get { return new WhsErrorTypes(Res.GetString("c19f00e8-a117-4f62-a217-77fac8f320bd", "This Receipt cannot be split by Area Type because only one Area Type is used.")); } }
		public static WhsErrorTypes SplitByQuantityRequiresLinesWithSplitQuantities { get { return new WhsErrorTypes(Res.GetString("db7708bd-cea4-4221-9ab8-c2f9030fa166", "This Receipt cannot be split by Quantity because no Split Quantities have been entered.")); } }
		public static WhsErrorTypes SplitByQuantitiesFullySet { get { return new WhsErrorTypes(Res.GetString("4385eb49-3766-442e-b497-b6cf70a87456", "This Receipt cannot be split because all split quantities equal all original quantities, so splitting would leave this receive with no lines left (empty)")); } }
		public static WhsErrorTypes CreateASNLineRequiresReceiptToBeSaved { get { return new WhsErrorTypes(Res.GetString("e2936a55-5040-4aa6-ad1a-4ab3ff9a6881", "Please save your receipt before creating ASN lines.")); } }
		public static WhsErrorTypes SplitByQuantityRequiresReceiptToBeSaved { get { return new WhsErrorTypes(Res.GetString("FBAAA35E-E5E9-453A-A517-4C0FC52FCE51", "Please save your receipt before splitting by quantity.")); } }
		public static WhsErrorTypes SplitByAreaTypeRequiresReceiptToBeSaved { get { return new WhsErrorTypes(Res.GetString("375AC57A-64D7-417D-8E71-7347B5B529DA", "Please save your receipt before splitting by area type.")); } }
		public static WhsErrorTypes ContainsReceiveLinesWithPutawayTransfers { get { return new WhsErrorTypes(Res.GetString("0b497dcb-9af3-4320-911e-90d3d0f937c4", "Cannot perform this operation because at least one line linked to a Putaway Transfer")); } }
		public static WhsErrorTypes CalculateOversAndUndersRequiresReceiptToHaveNoErrors { get { return new WhsErrorTypes(Res.GetString("0C457A4D-6E51-42E6-89AF-0F5AEEB128FE", "This receipt has errors. Please correct the errors and try again.")); } }
		public static WhsErrorTypes NoLinesWithHoldCode { get { return new WhsErrorTypes(Res.GetString("43D9C661-BD9D-45F6-9647-A9C5559748A2", "Cannot perform this operation because no Hold Code has been entered.")); } }
		public static WhsErrorTypes NoValidLines { get { return new WhsErrorTypes(Res.GetString("183868DF-A333-462B-AEC9-9ACBEC560351", "Cannot perform this operation because selected lines are invalid.")); } }
	}

	[Serializable]
	public class OrderErrorTypes : WhsErrorTypes
	{
		public OrderErrorTypes(string message)
			: base(message)
		{
		}

		public static WhsErrorTypes DataGeneratedFromContainers(string count) { return new OrderErrorTypes(Res.GetString("930bf967-bc09-42ca-9b97-9dc0b559d2e3", "{0} Order Lines have been created from container numbers.", count)); }
		public static WhsErrorTypes DataGeneratedFromContainersHasErrors(string count) { return new OrderErrorTypes(Res.GetString("25501db5-179f-4299-8856-d5d24f7d880a", "{0} Order Lines have been created from container numbers but there are some errors. Please check error icons.", count)); }
		public static WhsErrorTypes CannotPerformThisOperationBecauseOrderIsFinalisedOrCancelledOrPicked { get { return new OrderErrorTypes(Res.GetString("e1b0f21f-0f15-4729-a974-88f907d017fa", "Cannot perform this operation because the Order is either Finalized, Canceled or Picked")); } }

		public static WhsErrorTypes CannotPerformThisOperationBecauseOrderIsPartiallyOrFullyPicked => new OrderErrorTypes(Res.GetString("0557A555-9BFA-43EB-AE04-3E7CB41A2FB4", "Cannot perform this operation because the Order is partially or fully picked."));

		public static WhsErrorTypes CannotPerformThisOperationBecauseOrderIsBeingPicked => new OrderErrorTypes(Res.GetString("3f2e5e1a-be7e-4335-b655-d4b3863a6c9a", "Cannot perform this operation because the Order is being picked."));

		public static WhsErrorTypes CannotPerformThisOperationBecauseOrderIsCartonisedOrBeingCartonised => new OrderErrorTypes(Res.GetString("1098d4c9-725c-4b9a-a71c-02c286b30e21", "Cannot perform this operation because the Pick is Cartonized or being Cartonized."));

		public static WhsErrorTypes CannotPerformThisOperationBecauseOrderHasPackagesAssignedToTrolleyJob => new OrderErrorTypes(Res.GetString("2A5F07C6-734E-48D5-8D85-C88D252C0912", "Cannot perform this operation because the order has packages assigned to a trolley job."));

		public static WhsErrorTypes CannotPerformThisOperationBecauseOrderHasPackagesAssignedToPickByLabelJob => new OrderErrorTypes(Res.GetString("A4F580A1-35C8-44D1-9A51-E73241AE6090", "Cannot perform this operation because the order has packages assigned to a pick by label job."));
	}

	[Serializable]
	public class TransferErrorTypes : WhsErrorTypes
	{
		public TransferErrorTypes(string message)
			: base(message)
		{
		}

		public static WhsErrorTypes NoDestinationLocationSelected { get { return new TransferErrorTypes(Res.GetString("989ea66a-db93-4ad8-aeee-a049e9af3e77", "There are no Destination Locations entered or selected for allocation")); } }
		public static WhsErrorTypes CannotPerformThisOnOutboundDockDoorTransfer { get { return new TransferErrorTypes(Res.GetString("cc1e1bd9-ca14-42bc-abb6-2c9ee4bf58ce", "Cannot perform this operation on an Outbound Dock Door Transfer.")); } }
		public static WhsErrorTypes NoDocketIDSpecified { get { return new TransferErrorTypes(Res.GetString("06212c2b-3433-451f-ba31-f637656800fe", "Please save this Transfer before Generating Pallet IDs")); } }
		public static WhsErrorTypes CanOnlyPerformThisOnInternalTransfer { get { return new TransferErrorTypes(Res.GetString("0acb1997-dd15-4d6a-adb0-1c084fdecd02", "Can only perform this operation on an internal Transfer.")); } }
	}

	[Serializable]
	public class PickErrorTypes : WhsErrorTypes
	{
		public PickErrorTypes(string message)
			: base(message)
		{
		}

		public static WhsErrorTypes PickZErrorMessageBox { get { return new WhsErrorTypes("ZErrorMessageBox", Res.GetString("8b9c7535-b866-4e37-8912-06348fc2eb2c", "Pick")); } }
		public static WhsErrorTypes StockWasUnAllocatedFromOrders { get { return new PickErrorTypes(Res.GetString("1e9de571-b1e0-410f-9c29-493f0f0c03f4", "The stock allocated to the order(s) you just detached is now available for picking (uncommitted)")); } }
		public static WhsErrorTypes PickContainsMixedInventoryOrders { get { return new PickErrorTypes(Res.GetString("f3e1ec13-7bf2-4891-b798-ae9dd652a496", "Pick must not contain orders with mixed (held and available) inventory.")); } }
		public static WhsErrorTypes CannotPerformThisOperationBecausePickIsReadyForPlanningOrPlanned => new OrderErrorTypes(Res.GetString("c36da1ba-0478-45ee-af37-54d3f82a9130", "Cannot perform this operation because the Pick is Ready For Planning or Planned."));
		public static WhsErrorTypes DocketTypeMismatch(string expectedTypeDesc) => new PickErrorTypes(Res.GetString("1b5c7b85-c2fb-4141-b8a7-0093efda6bf5", "This Pick can only be used with {0} job types. Attach an order of this type, or cancel the Pick and create a new one to proceed.", expectedTypeDesc));
		public static WhsErrorTypes DocketMismatchExpectAvailableInventory => new PickErrorTypes(Res.GetString("f1b8aa78-a963-4b83-b41b-7a312863e78d", "This Pick can only be used with orders not containing held inventory. Attach an order of this type, or cancel the Pick and create a new one to proceed."));
		public static WhsErrorTypes DocketMismatchExpectHeldInventory => new PickErrorTypes(Res.GetString("53813e75-edf9-49f1-a406-da668902d141", "This Pick can only be used with orders for held inventory. Attach an order of this type, or cancel the Pick and create a new one to proceed."));
	}

	[Serializable]
	public class VASOrderErrorTypes : WhsErrorTypes
	{
		public VASOrderErrorTypes(string message)
			: base(message)
		{
		}

		public static WhsErrorTypes NoLocationsDefined
		{
			get
			{
				return new VASOrderErrorTypes(
Res.GetString("619e9cab-1f6b-4577-9651-2d96d7cb873d", @"There are no locations defined for this warehouse, or there are no locations with a suitable status to transfer to.
There needs to be at least one location in the warehouse that does NOT have a status of Held, Damaged or Void and the Area Types must match.

The Transfer Out will need to have its Locations manually entered."));
			}
		}
	}
}
