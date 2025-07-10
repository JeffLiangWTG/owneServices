using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReleaseLine : NonPersistentBusinessObject<WhsReleaseLineValidation>, IPackableItemParent, IPartAttributes, IPartAttributeValidationConsumer, ISerialSplittableLine
	{
		public WhsReleaseLine(WhsPickableDocketLine orderLine, bool isForComponentLines = false)
			: base(GetFactoryWithNullCheck(orderLine))
		{
			if (orderLine.IsDeleted)
			{
				throw new ArgumentException("Should not wrap Deleted Objects.");
			}

			IsForComponentLines = isForComponentLines;
			OrderLine = orderLine;
		}

		static BusinessObjectFactory GetFactoryWithNullCheck(WhsPickableDocketLine orderLine)
		{
			return Argument.NotNull(orderLine, nameof(orderLine)).Factory;
		}

		internal readonly WhsPickableDocketLine OrderLine;

		public static class Schema
		{
			public const string ExpiryDate = "ExpiryDate";
			public const string PackingDate = "PackingDate";
			public const string PartAttribute1 = "PartAttribute1";
			public const string PartAttribute2 = "PartAttribute2";
			public const string PartAttribute3 = "PartAttribute3";
			public const string SerialNumber = "SerialNumber";
			public const string Quantity = "Quantity";
			public const string UnreleasedQty = "UnreleasedQty";
		}

		#region Related Entities

		#region Client

		public OrgHeader Client => PickableDocket?.Client;

		#endregion

		#region CustomFieldsForOrderLineAccessor

		public IAccessBusinessObject CustomFieldsForOrderLineAccessor
		{
			get { return Factory.GetCachedValue("WhsReleaseLine|CustomFieldsForOrderLineAccessor|" + OrderLine.PK, () => new CustomFieldsAccessor(OrderLine)); }
		}

		sealed class CustomFieldsAccessor : IAccessBusinessObject
		{
			public CustomFieldsAccessor(WhsPickableDocketLine orderLine)
			{
				OrderLine = Argument.NotNull(orderLine, nameof(orderLine));
			}

			readonly WhsPickableDocketLine OrderLine;

			object IAccessBusinessObject.this[string propertyName]
			{
				get
				{
					if (!ValidCustomFields.Contains(propertyName))
					{
						throw new InvalidOperationException("Should only use the CustomFieldsForOrderLineAccessor to access Custom Fields.");
					}

					return OrderLine[propertyName];
				}
			}

			bool IAccessBusinessObject.IsPropertyReadOnly(string propertyName)
			{
				return ((IAccessBusinessObject)OrderLine).IsPropertyReadOnly(propertyName);
			}

			HashSet<string> ValidCustomFields
			{
				get { return OrderLine.Factory.GetCachedValue("CustomFieldsAccessor|ValidCustomFields", () => new HashSet<string>(GetValidPropertyNames())); }
			}

			IEnumerable<string> GetValidPropertyNames()
			{
				foreach (CustomLabelInfoBase customLabel in new WhsDocketLine.CustomLabelsProvider(null).GetCustomFields(null, OrderLine.Factory))
				{
					yield return customLabel.PropertyName;
				}
			}
		}

		#endregion

		#region ParentCollection

		/// <summary>
		/// Do NOT access this property if the Release Line is not in the Collection
		/// </summary>
		public WhsReleaseLineCollection ParentCollection
		{
			get { return base.ParentCollections.OfType<WhsReleaseLineCollection>().Single(); }
		}

		#endregion

		#region PickableDocket

		public WhsPickableDocket PickableDocket
		{
			get { return OrderLine.PickableDocket; }
		}

		#endregion

		#region Product

		public WhsProduct Product
		{
			get { return OrderLine.Product; }
		}

		#endregion

		#region SupplierPart

		public OrgSupplierPart SupplierPart
		{
			get { return OrderLine.SupplierPart; }
		}

		#endregion

		#endregion

		#region Properties

		#region DecimalPlaces

		public ZInt DecimalPlaces
		{
			get { return Product?.Parent.OP_CountDecimalPlaces ?? ZInt.Zero; }
		}

		#endregion

		#region KeyForPacking

		public GroupingKey KeyForPacking => InventoryGroupingKey.New(this, OrderLine);

		#endregion

		#region OrderPK

		public ZGuid OrderPK => OrderLine.WE_WD;

		#endregion

		#region OrderLinePK

		internal ZGuid OrderLinePK => OrderLine.PK;

		#endregion

		#region Part Attributes

		#region OrderedPartAttribute1

		public ZString OrderedPartAttribute1
		{
			get { return OrderLine.WE_PartAttrib1; }
		}

		#endregion

		#region OrderedPartAttribute2

		public ZString OrderedPartAttribute2
		{
			get { return OrderLine.WE_PartAttrib2; }
		}

		#endregion

		#region OrderedPartAttribute3

		public ZString OrderedPartAttribute3
		{
			get { return OrderLine.WE_PartAttrib3; }
		}

		#endregion

		#region OrderedSerialNumber

		public ZString OrderedSerialNumber
		{
			get { return OrderLine.WE_SerialNumber; }
		}

		#endregion

		#region OrderedPackingDate

		public ZDate OrderedPackingDate
		{
			get { return OrderLine.WE_PackingDate; }
		}

		#endregion

		#region OrderedExpiryDate

		public ZDate OrderedExpiryDate
		{
			get { return OrderLine.WE_ExpiryDate; }
		}

		#endregion

		#region ProductPK

		internal ZGuid ProductPK
		{
			get { return Product?.Parent.PK ?? ZGuid.Empty; }
		}

		#endregion

		#region OrderedQuantity

		public ZDecimal OrderedQuantity
		{
			get { return OrderLine.WE_TransactionQuantity; }
		}

		#endregion

		#region PartAttribute1

		[MaxLength(WhsPickLine.Schema.WZ_ReleaseCapturedPartAttrib1MaxLength)]
		[ReadOnlyMember(nameof(PartAttrib1ReadOnly))]
		public ZString PartAttribute1
		{
			get { return partAttribute1; }
			set
			{
				if (PartAttribute1 != value)
				{
					SetPartAttribute(ref partAttribute1, PartAttributeNumber.One, PartAttribute1Info, value);
				}
				else
				{
					PartAttribute1Info.RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidatePartAttribute1();
				}
			}
		}

		public ZPropertyInfo PartAttribute1Info
		{
			get { return GetZPropertyInfo(Schema.PartAttribute1); }
		}

		protected bool PartAttrib1ReadOnly
		{
			get { return PartAttribReadOnly(1); }
		}

		ZString partAttribute1;

		#endregion

		#region PartAttribute2

		[MaxLength(WhsPickLine.Schema.WZ_ReleaseCapturedPartAttrib2MaxLength)]
		[ReadOnlyMember(nameof(PartAttrib2ReadOnly))]
		public ZString PartAttribute2
		{
			get { return partAttribute2; }
			set
			{
				if (PartAttribute2 != value)
				{
					SetPartAttribute(ref partAttribute2, PartAttributeNumber.Two, PartAttribute2Info, value);
				}
				else
				{
					PartAttribute2Info.RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidatePartAttribute2();
				}
			}
		}

		public ZPropertyInfo PartAttribute2Info
		{
			get { return GetZPropertyInfo(Schema.PartAttribute2); }
		}

		protected bool PartAttrib2ReadOnly
		{
			get { return PartAttribReadOnly(2); }
		}

		ZString partAttribute2;

		#endregion

		#region PartAttribute3

		[MaxLength(WhsPickLine.Schema.WZ_ReleaseCapturedPartAttrib3MaxLength)]
		[ReadOnlyMember(nameof(PartAttrib3ReadOnly))]
		public ZString PartAttribute3
		{
			get { return partAttribute3; }
			set
			{
				if (PartAttribute3 != value)
				{
					SetPartAttribute(ref partAttribute3, PartAttributeNumber.Three, PartAttribute3Info, value);
				}
				else
				{
					PartAttribute3Info.RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidatePartAttribute3();
				}
			}
		}

		public ZPropertyInfo PartAttribute3Info
		{
			get { return GetZPropertyInfo(Schema.PartAttribute3); }
		}

		protected bool PartAttrib3ReadOnly
		{
			get { return PartAttribReadOnly(3); }
		}

		ZString partAttribute3;

		#endregion

		#region SerialNumber

		[ResourceStringData("WhsReleaseLine|SerialNumber", Caption = "Serial Number", ShortCaption = "Serial #")]
		[MaxLength(WhsPickLine.Schema.WZ_ReleaseCapturedSerialNumberMaxLength)]
		[ReadOnlyMember(nameof(SerialNumberReadOnly))]
		public ZString SerialNumber
		{
			get { return serialNumber; }
			set
			{
				if (SerialNumber != value)
				{
					SetPartAttribute(ref serialNumber, PartAttributeNumber.SerialNumber, SerialNumberInfo, value);

					if (Quantity > 1 && !SerialNumber.IsEmpty)
					{
						Quantity = 1;
					}
				}
				else
				{
					SerialNumberInfo.RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSerialNumber();
				}
			}
		}

		public ZPropertyInfo SerialNumberInfo => GetZPropertyInfo(Schema.SerialNumber);

		protected bool SerialNumberReadOnly
		{
			get
			{
				var product = Product;
				return product == null
					|| !product.IsSerialNumberReleaseCaptured(Client)
					|| IsPacked
					|| IsFinalisedOrCancelledReadOnly
					|| IsForComponentLines;
			}
		}

		ZString serialNumber;

		#endregion

		#region PackingDate

		[ResourceStringData("WhsReleaseLine|PackingDate", Caption = "Packing Date", ShortCaption = "Packing")]
		public ZDate PackingDate => packingDate;

#if DEBUG
		/// <summary>
		/// DO NOT use except for Tests! This will be removed later
		/// </summary>
		internal void SetPackingDateForTesting(ZDate packing)
		{
			SetPartAttribute(ref packingDate, PartAttributeNumber.None, PackingDateInfo, packing);
		}
#endif

		public ZPropertyInfo PackingDateInfo
		{
			get { return GetZPropertyInfo(Schema.PackingDate); }
		}

		ZDate packingDate;

		#endregion

		#region ExpiryDate

		[ResourceStringData("WhsReleaseLine|ExpiryDate", Caption = "Expiry Date", ShortCaption = "Expiry")]
		public ZDate ExpiryDate => expiryDate;

#if DEBUG
		/// <summary>
		/// DO NOT use except for Tests! This will be removed later
		/// </summary>
		internal void SetExpiryDateForTesting(ZDate expiry)
		{
			SetPartAttribute(ref expiryDate, PartAttributeNumber.None, ExpiryDateInfo, expiry);
		}
#endif

		public ZPropertyInfo ExpiryDateInfo
		{
			get { return GetZPropertyInfo(Schema.ExpiryDate); }
		}

		ZDate expiryDate;

		#endregion

		#region SetPartAttribute

		void SetPartAttribute<T>(ref T nonPersistentValue, PartAttributeNumber partAttribNo, ZPropertyInfo info, T value)
			where T : IZType
		{
			var oldAttributes = AttributeParts.New(this);
			SetNonPersistentPropertyValue(info, ref nonPersistentValue, value);

			ReleaseLinesOnPickManager.NotifyAttributeChange(Factory, this, OrderLine, oldAttributes);

			// we must update Release Captured Attributes before changing Quantity
			AttributesChanged?.Invoke(this, new UpdateAttributesEventArgs(oldAttributes, partAttribNo));
		}

		#endregion

		public event EventHandler<UpdateAttributesEventArgs> AttributesChanged;

		#endregion

		#region Quantity

		[ReadOnlyMember(nameof(IsFinalisedOrCancelledReadOnly))]
		[ResourceStringData("WhsReleaseLine|Quantity", Caption = "Quantity Met", ShortCaption = "Quantity")]
		public ZDecimal Quantity
		{
			get { return quantity; }
			set
			{
				bool invalidQuantityWasSet = InvalidQuantityThatWasReversed.HasValue;
				InvalidQuantityThatWasReversed = null;

				var previousQuantity = Quantity;
				if (previousQuantity != value)
				{
					bool isValidQuantity = SetQuantityAndOrderFields(previousQuantity, value);
					if (isValidQuantity && !OrderLine.IsValidationSuspended) // no point validating shortfall if we're going to revert the quantity
					{
						OrderLine.Validation.ValidateWE_ShortfallQuantityCached();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateQuantity();
					}

					if (!isValidQuantity)
					{
						// Unfortunately whenever a Cell is changed in a ZGrid, the Architecture will Validate the Property bound to the cell,
						// even though property setters already call Validation. In any case, this is impossible to circumvent currently.
						// This means that even though the Quantity property is validated with the Quantity the user was trying to set before
						// being reverted, as soon as the cell changes the Validation Error will be cleared because the CellChange will re-validate
						// the Quantity after it's been reverted. This means we have to persist the Reverted Qty for Validation purposes Yuck!
						// This also means the user cannot remove the error without typing over the cell with the same value or running validate all.
						InvalidQuantityThatWasReversed = value;

						// reset value to return to valid state
						SetNonPersistentPropertyValue(QuantityInfo, ref quantity, previousQuantity);
					}
				}
				else
				{
					QuantityInfo.RefreshBinding();

					// reset validation error
					if (invalidQuantityWasSet && !IsValidationSuspended)
					{
						Validation.ValidateQuantity();
					}
				}
			}
		}

		/// <summary>
		/// DO NOT USE! Read Quantity setter to see the reason this property exists.
		/// </summary>
		internal ZDecimal? InvalidQuantityThatWasReversed { get; private set; }

		/// <summary>
		/// This will return false if the Quantity will need to be reverted.
		/// </summary>
		bool SetQuantityAndOrderFields(ZDecimal previousQuantity, ZDecimal value)
		{
			bool isValidQuantity = true;

			SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value);

			if (!SuspendUpdatingSumOfUnitsMetAndOrderFieldsSemaphore.IsSuspended)
			{
				// if changing the quantity will result in a state that can't be managed we will need to revert the change
				var proposedQty = Quantity;
				if (IsPacked && proposedQty < GetPackedQty()) // attempt to change release line qty to less than what is already packed
				{
					isValidQuantity = false;
				}
				else
				{
					ParentCollection.UpdateSumOfUnitsMet(this, previousQuantity);
					UpdateOrderTotalsAndCalculateExtendedLinePrice(OrderLine, proposedQty - previousQuantity);
				}
			}

			return isValidQuantity;
		}

		internal static void UpdateOrderTotalsAndCalculateExtendedLinePrice(WhsPickableDocketLine pickableDocketLine, ZDecimal quantity)
		{
			if (quantity != 0m)
			{
				pickableDocketLine.PickableDocket?.UpdateReleaseTotals(pickableDocketLine.SupplierPart, quantity);
				pickableDocketLine.CalculateExtendedLinePrice();
			}
		}

		/// <summary>
		/// Used to short circuit a bit of logic when changing Quantity, typically used when splitting Release Lines as we know the aggregated Qty won't change.
		/// </summary>
		internal IDisposable SuspendUpdatingSumOfUnitsMetAndOrderFields()
		{
			return new SemaphoreManager(SuspendUpdatingSumOfUnitsMetAndOrderFieldsSemaphore);
		}

		public ZPropertyInfoDecimal QuantityInfo
		{
			get { return (ZPropertyInfoDecimal)GetZPropertyInfo(Schema.Quantity); }
		}

		ZDecimal quantity;

		#endregion

		#region TotalQuantityOrderedFromComponents

		public ZDecimal TotalQuantityOrderedFromComponents
		{
			get { return OrderLine.TotalQuantityOrderedFromComponents; }
		}

		public ZDecimal TotalPickLineQuantityFromComponents => OrderLine.TotalPickLineQuantityFromComponents;

		#endregion

		#region UnitsUQ

		public ZString UnitsUQ
		{
			get { return SupplierPart?.OP_StockKeepingUnit ?? Constants.PkgUnit.Unit; }
		}

		#endregion

		#region UnitPriceAfterDiscount

		public ZDecimal UnitPriceAfterDiscount => OrderLine.WE_UnitPriceAfterDiscount;

		#endregion

		#region UnreleasedQty

		[ReadOnly(true)]
		[ResourceStringData("WhsReleaseLine|UnreleasedQty", Caption = "Unreleased Qty")]
		public ZDecimal UnreleasedQty
		{
			get { return unreleasedQty; }
			set { SetNonPersistentPropertyValue(UnreleasedQtyInfo, ref unreleasedQty, value); }
		}

		public ZPropertyInfo UnreleasedQtyInfo
		{
			get { return GetZPropertyInfo(Schema.UnreleasedQty); }
		}

		ZDecimal unreleasedQty;

		#endregion

		#region IsForComponentLines

		public readonly bool IsForComponentLines;

		#endregion

		bool PartAttribReadOnly(int attribNo)
		{
			var product = Product;
			return product == null || !product.IsPartAttribReleaseCaptured(Client, attribNo) || IsPacked || IsFinalisedOrCancelledReadOnly || IsForComponentLines;
		}

		bool IsFinalisedOrCancelledReadOnly
		{
			get
			{
				var pickableDocket = PickableDocket;
				return pickableDocket == null || pickableDocket.IsPickFinalised || pickableDocket.IsCancelled;
			}
		}

		#endregion

		#region Flags

		#region AllAttributesEmpty

		public bool AllAttributesEmpty
		{
			get
			{
				return
					ExpiryDate.IsEmpty &&
					PackingDate.IsEmpty &&
					SerialNumber.IsEmpty &&
					PartAttribute1.IsEmpty &&
					PartAttribute2.IsEmpty &&
					PartAttribute3.IsEmpty;
			}
		}

		#endregion

		#region IsBOMProductPickedOnSalesOrder

		public bool IsBOMProductPickedOnSalesOrder
		{
			get { return OrderLine.IsBOMProductPickedOnSalesOrder; }
		}

		#endregion

		#region IsPacked

		public bool IsPacked
		{
			get
			{
				var parentCollection = ParentCollection;
				return !parentCollection.IsNonCommittedCollectionElement(this) && !parentCollection.IsDuplicateReleaseLine(this) && (PackageJob?.IsPacked(this) ?? false);
			}
		}

		#endregion

		#endregion

		#region Functions

		#region GetPackedQty

		public ZDecimal GetPackedQty()
		{
			return PackageJob?.GetPackedQty(this) ?? ZDecimal.Zero;
		}

		PkgPackageJob PackageJob
		{
			get { return OrderLine is WhsOrderLine ? Factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, OrderLine.WE_WD)) : null; }
		}

		#endregion

		#region GetPartAttribute

		public ZString GetPartAttribute(PartAttributeNumber partAttribNo)
		{
			switch (partAttribNo)
			{
				case PartAttributeNumber.One:
					return PartAttribute1;
				case PartAttributeNumber.Two:
					return PartAttribute2;
				case PartAttributeNumber.Three:
					return PartAttribute3;
				case PartAttributeNumber.SerialNumber:
					return SerialNumber;

				default:
					return ZString.Empty;
			}
		}

		#endregion

		#region SuspendUpdatingSumOfUnitsMetAndOrderFieldsSemaphore

		Semaphore SuspendUpdatingSumOfUnitsMetAndOrderFieldsSemaphore
		{
			get { return suspendUpdatingSumOfUnitsMetAndOrderFieldsSemaphore ?? (suspendUpdatingSumOfUnitsMetAndOrderFieldsSemaphore = new Semaphore()); }
		}

		Semaphore suspendUpdatingSumOfUnitsMetAndOrderFieldsSemaphore;

		#endregion

		#region IsSerialisedProduct

		public bool IsSerialisedProduct()
		{
			var client = Client;
			var product = Product;
			return client != null && product != null && product.IsSerialNumberUsed(client);
		}

		bool IsReleaseCapturedSerialisedProduct()
		{
			var client = Client;
			var product = Product;
			return client != null && product != null && product.IsSerialNumberReleaseCaptured(client);
		}

		#endregion

		#region IsForOrderLine

		internal bool IsForOrderLine(WhsPickableDocketLine orderLine) => OrderLine == orderLine;

		#endregion

		#region SetupReleaseLine

		public void SetupReleaseLine(
			string partAttrib1,
			string partAttrib2,
			string partAttrib3,
			ZDate expiry,
			ZDate packing,
			string serialNum)
		{
			partAttribute1 = partAttrib1;
			partAttribute2 = partAttrib2;
			partAttribute3 = partAttrib3;
			serialNumber = serialNum;
			expiryDate = expiry;
			packingDate = packing;
		}

		#endregion

		#region AdjustOutQuantityMet

		internal static ActionResult AdjustOutInventoryAndSaveInOtherFactory(ZGuid orderLinePK, string key, ZDecimal quantityToAdjust, IPickedStockAdjuster pickedStockAdjuster, IMultilingualString reduceStockReason, BusinessObjectFactory factory)
		{
			ActionResult result;

			var orderLineInOtherFactory = factory.Load<WhsOrderLine>(orderLinePK);
			var releaseLine = orderLineInOtherFactory.ReleaseLines.Cast<WhsReleaseLine>().SingleOrDefault(rl => WhsReleaseLineCollection.GetKey(rl) == key);

			if (releaseLine == null)
			{
				result = ActionResult.Failure(Res.GetString("f04d1f9d-523f-4f9f-a449-e64c852d8bc2", "Another user has modified the Allocated Stock on this Order.\r\nClose and re-open the form, then attempt to '{0}' again.", reduceStockReason));
			}
			else if (releaseLine.IsPickFinalised)
			{
				result = ActionResult.Failure(Res.GetString("5b20aba0-84f3-4b1c-afca-ad8ab4b12ddf", "Another user has finalized the Pick, cannot '{0}'.", reduceStockReason));
			}
			else if (orderLineInOtherFactory.Order.IsLoadingOrLoadedOrDeparted)
			{
				result = ActionResult.Failure(Res.GetString("a1fe427d-7fb5-40c4-a74e-fa1bcfb5e5fc", "Order is Loading, Loaded or Departed, cannot '{0}'.", reduceStockReason));
			}
			else
			{
				var parentCollection = releaseLine.ParentCollection;
				var items = parentCollection.GetPackableItems(releaseLine).OfType<IReducibleItem>().Where(item => item.CanBeReduced).ToArray();

				if (items.Length == 0)
				{
					result = ActionResult.Failure(Res.GetString("3755d0e7-7a3d-4ce4-b741-f79a19e0d108", "Another user has modified related data, cannot '{0}'.", reduceStockReason));
				}
				else
				{
					var reduceStockResult = ReduceStock(items, quantityToAdjust, pickedStockAdjuster);
					if (reduceStockResult.IsSuccess)
					{
						using (parentCollection.SuspendUnreleasedQtyChange())
						{
							releaseLine.Quantity -= quantityToAdjust;
						}

						if (releaseLine.Quantity == 0)
						{
							releaseLine.Delete();
						}

						result = ActionResult.Success();
					}
					else
					{
						result = reduceStockResult;
					}
				}
			}
			return result;
		}

		static ActionResult ReduceStock(IReducibleItem[] items, ZDecimal quantityToReduceOnThisReleaseLine, IPickedStockAdjuster pickedStockAdjuster)
		{
			foreach (var item in items)
			{
				ZDecimal quantityToLoseOnPackableItem = Math.Min(quantityToReduceOnThisReleaseLine, item.Quantity);
				var reduceStockResult = item.ReduceStock(quantityToLoseOnPackableItem, pickedStockAdjuster);
				if (reduceStockResult.IsSuccess)
				{
					quantityToReduceOnThisReleaseLine -= quantityToLoseOnPackableItem;
				}

				if (!reduceStockResult.IsSuccess || quantityToReduceOnThisReleaseLine == 0)
				{
					return reduceStockResult;
				}
			}

			throw new InvalidOperationException(FormattableString.Invariant($"Should never reach this Code. there are '{items.Length}' items.")); // if reach this code number of items helps to find the issue
		}

		#region IsValidToReduceQuantityMet

		public ActionResult IsValidToReduceQuantityMet(IMultilingualString functionDescription) // tested in ReleaseLineGridUserControl and here via SetQuantityLost method
		{
			ActionResult result;

			var order = OrderLine.PickableDocket;
			var pick = order.Pick;
			if (pick.IsFinalised)
			{
				result = ActionResult.Failure(Res.GetString("22040638-1aa8-4640-8979-4c0541c9d5cb", "Cannot '{0}' on a finalized Pick.", functionDescription));
			}
			else if (pick.HasChanges)
			{
				result = ActionResult.Failure(Res.GetString("F78224E9-ADFD-4D57-A46E-DA701109E2C9", "Save pick before attempting to '{0}'.", functionDescription));
			}
			else if (order.Warehouse.WW_IsVirtualWarehouse)
			{
				result = ActionResult.Failure(Res.GetString("82d0a029-faef-4a04-8f15-0abae8e5386b", "Cannot '{0}' on a Pick in a Virtual Warehouse.", functionDescription));
			}
			else if (ParentCollection.GetPackableItems(this).OfType<IReducibleItem>().All(item => !item.CanBeReduced))
			{
				var message = IsBOMProductPickedOnSalesOrder
					? Res.GetString("3a277dda-923a-40e5-bd7a-476883ff4f96", "Stock must be picked (kits only) or not packed to '{0}'.", functionDescription)
					: Res.GetString("F7E4D1FD-3C0D-45DE-9BDC-5408F1A2C995", "Stock must be picked or not packed to '{0}'.", functionDescription);
				result = ActionResult.Failure(message);
			}
			else
			{
				result = ActionResult.Success();
			}

			return result;
		}

		#endregion

		#endregion

		#region GetOtherReleaseCapturedSerialNumbersQuery

		// tested in WhsReleaseLineValidation and Enterprise.Warehouse.Web.WebService.Testing.LoadAllCapturedSerialsForProductFromOtherPicksTest
		public static ZDBOnlyQuery GetOtherReleaseCapturedSerialNumbersQuery(ZGuid clientPK, ZGuid productPK, ZGuid excemptPickPK, ZGuid excemptOrderLinePK, ZQuery partAttributesFilter)
		{
			var pickSubQuery = new ZDBOnlySubQuery(typeof(WhsPick), WhsDocketSchema.WD_WP);
			pickSubQuery.AddToFilter(WhsPickSchema.WP_PickStatus, SQLComparisonOperator.NotEqual, PickStatus.Codes.Finalised);
			if (!excemptPickPK.IsEmpty)
			{
				pickSubQuery.AddToFilter(WhsPickSchema.PK, SQLComparisonOperator.NotEqual, excemptPickPK);
			}

			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsPickableDocket), WhsDocketLineSchema.WE_WD);
			docketSubQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, clientPK);
			docketSubQuery.AddSubQuery(pickSubQuery, JoinCondition.And);

			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickableDocketLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			if (WarehouseDataRegistry.Instance.EnforceSerialUniquenessByProduct)
			{
				docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_OP, productPK);
			}
			docketLineSubQuery.AddSubQuery(docketSubQuery, JoinCondition.And);

			var releaseCapturedQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
			if (!excemptOrderLinePK.IsEmpty)
			{
				releaseCapturedQuery.AddToFilter(WhsPickLineSchema.WZ_WE_TransactionLine, SQLComparisonOperator.NotEqual, excemptOrderLinePK);
			}

			releaseCapturedQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

			if (partAttributesFilter != null)
			{
				releaseCapturedQuery.AddToFilter(partAttributesFilter);
			}

			return releaseCapturedQuery;
		}

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted && !IsInDelete)
			{
				try
				{
					IsInDelete = true; // Delete() may get recursively called when setting Qty to 0

					using (GetValidationSuspender())
					{
						Quantity = 0m; // To reduce units on order
					}

					base.Delete();
					ParentCollection.DeleteReleaseLine(this);
				}
				finally
				{
					IsInDelete = false;
				}
			}
		}

		internal void DeleteObjectOnRemove_DoNotUse()
		{
			base.Delete();
		}

		bool IsInDelete;

		#endregion

		#region Validation

		#region EnableLightValidationIfAvailable

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		#endregion

		#region GetNewValidation

		public override WhsReleaseLineValidation GetNewValidation()
		{
			var result = new WhsReleaseLineValidation(this);
			((IValidationInternals)result).Add(new PickableDocketLineValidation(this));
			return result;
		}

		// Tested in WhsReleaseLineValidationTest
		class PickableDocketLineValidation : ZValidation
		{
			public PickableDocketLineValidation(WhsReleaseLine releaseLine)
				: base(releaseLine)
			{
			}

			WhsPickableDocketLine OrderLine
			{
				get { return ((WhsReleaseLine)ParentFilter).OrderLine; }
			}

			protected void CheckExpiryDate()
			{
				CheckValidationOnSumOfUnitsMet();
			}

			protected void CheckPackingDate()
			{
				CheckValidationOnSumOfUnitsMet();
			}

			protected void CheckPartAttribute1()
			{
				CheckValidationOnSumOfUnitsMet();
			}

			protected void CheckPartAttribute2()
			{
				CheckValidationOnSumOfUnitsMet();
			}

			protected void CheckPartAttribute3()
			{
				CheckValidationOnSumOfUnitsMet();
			}

			protected void CheckQuantity()
			{
				CheckValidationOnSumOfUnitsMet();
			}

			void CheckValidationOnSumOfUnitsMet()
			{
				if (!OrderLine.IsValidationSuspended)
				{
					OrderLine.Validation.ValidateSumOfUnitsMet();
				}
			}

			public override Type AutoValidationType
			{
				get { return typeof(PickableDocketLineValidation); }
			}

			public override void ValidateAll()
			{
				throw new InvalidOperationException("Should not be calling ValidateAll() on PiggyBacked Validation.");
			}
		}

		#endregion

		#region IsValidationEnabled

		// tested in WhsReleaseLineValidationTest
		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && IsValidationEnabledInternal(propertyInfo);
		}

		bool IsValidationEnabledInternal(ZPropertyInfo propertyInfo)
		{
			switch (propertyInfo.Name)
			{
				case Schema.ExpiryDate:
				case Schema.PackingDate:
				case Schema.PartAttribute1:
				case Schema.PartAttribute2:
				case Schema.PartAttribute3:
				case Schema.SerialNumber:
					return !IsPickFinalised;
				case Schema.Quantity:
					return IsUnitValidationRequired;

				default:
					return true;
			}
		}

		bool IsPickFinalised => Pick?.IsFinalised ?? false;

		/// <summary>
		/// Validation for this Non-Persistent Bizo should only ever run on a Release Screen, but we'll keep this here just to be safe.
		/// </summary>
		bool IsUnitValidationRequired
		{
			get
			{
				var order = PickableDocket as WhsOrder;
				return (order == null || !order.IsSavedFromOrderForm); // turn off this validation if Order is saved from OrderEntryForm.
			}
		}

		internal WhsPick Pick => PickableDocket?.Pick;

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			InvalidQuantityThatWasReversed = null; // When running ValidateAll() or Saving the Form we should Clear this Field to allow for proper Validation.
			base.RunPreSaveValidationCore();
		}

		#endregion

		// interfaces

		#region ICanDelete Members

		public sealed override bool CanDelete
		{
			get { return !ReleaseLineHasDistinctNonRCAttributes && !IsPacked && !ParentCollection.IsReleaseLinePickedFromPutawayLocation(this); }
		}

		bool ReleaseLineHasDistinctNonRCAttributes
		{
			get
			{
				return
					!(Quantity + UnreleasedQty == 0m) // If both Quantity and UnreleasedQty are zero OR sum to zero this line is meaningless and can for sure be deleted.
					&& ReleaseLinesOnPickManager.ReleaseLineHasDistinctNonRCAttributes(Factory, this, OrderLine);
			}
		}

		public sealed override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString reason;

				if (IsPacked)
				{
					reason = ResString.GetMultilingualString("5a57a5f6-ff9f-4a17-b03b-1a2eed5d146b", "This item is packed and cannot be deleted.");
				}
				else if (ReleaseLineHasDistinctNonRCAttributes)
				{
					reason = Quantity == 0m && UnreleasedQty > 0m
						? ResString.GetMultilingualString("3315c60d-c8a8-4257-be8f-e439ce103a6f", "This stock is not fully Allocated, either increase the Quantity or Allocate the stock to another Order Line.")
						: ResString.GetMultilingualString("842daf52-ee00-4b23-9c23-482521409989", "This item is Allocated to this Order Line and cannot be deleted, reduce the Quantity instead to allow allocations to another Order Line.");
				}
				else
				{
					reason = ResString.GetMultilingualString("35427301-533b-4b9e-b308-4f62c94f0705", "This Release Captured Attribute is picked and cannot be deleted.");
				}

				return reason;
			}
		}

		#endregion

		#region IPackableItemParent Members

		#region AdditionalProperties

		ICustomPropertyContainer IPackableItemParent.AdditionalProperties
		{
			get
			{
				if (additionalProperties == null)
				{
					var partAttributeManager = GetPartAttributeManager();
					if (partAttributeManager != null)
					{
						additionalProperties = new CustomPropertyContainer<WhsReleaseLine>();
						additionalProperties.AddCustomProperty(partAttributeManager.PartAttributeName1, typeof(ZString), a => a.PartAttribute1);
						additionalProperties.AddCustomProperty(partAttributeManager.PartAttributeName2, typeof(ZString), a => a.PartAttribute2);
						additionalProperties.AddCustomProperty(partAttributeManager.PartAttributeName3, typeof(ZString), a => a.PartAttribute3);
						additionalProperties.AddCustomProperty((NoResString)"Serial Number", partAttributeManager.SerialNumberName, typeof(ZString), a => a.SerialNumber); // Constant Field Name

						additionalProperties.AddCustomProperty((NoResString)"Expiry", Res.GetString("73b921ff-fb9f-4fa4-821f-6bb1ef445ea5", "Expiry"), typeof(ZDateTime), a => a.ExpiryDate); // Constant Field Name
						additionalProperties.AddCustomProperty((NoResString)"Packing", Res.GetString("8ef52f13-c752-4f32-a65f-b30b32737aa7", "Packing"), typeof(ZDateTime), a => a.PackingDate); // Constant Field Name
					}
				}

				return additionalProperties;
			}
		}

		PartAttributeManager GetPartAttributeManager()
		{
			return PickableDocket?.Client?.PartAttributeManager;
		}

		CustomPropertyContainer<WhsReleaseLine> additionalProperties;

		#endregion

		#region AutoPack

		ZString IPackableItemParent.AutoPackPackageType => OrderLine.WE_F3_NKPackType;

		ZDecimal IPackableItemParent.AutoPackQtyPerPackage
		{
			get
			{
				ZDecimal result = 0m;

				var part = SupplierPart;
				if (part != null)
				{
					result = part.UnitConverter.Convert(1, OrderLine.WE_F3_NKPackType, part.OP_StockKeepingUnit);
				}

				return result;
			}
		}

		#endregion

		ZString IPackableItemParent.Code => OrderLine.ProductCode;

		#region Description

		ZString IPackableItemParent.Description => OrderLine.ProductDesc;

		ZString IPackableItemParent.DescriptionSupplement
		{
			get
			{
				var result = new ZStringBuilder();

				var partAttributeManager = GetPartAttributeManager();
				if (partAttributeManager != null)
				{
					result.AppendIfNotEmpty(partAttributeManager.PartAttributeName1 + ": ", PartAttribute1);
					result.AppendIfNotEmpty(partAttributeManager.PartAttributeName2 + ": ", PartAttribute2);
					result.AppendIfNotEmpty(partAttributeManager.PartAttributeName3 + ": ", PartAttribute3);
					result.AppendIfNotEmpty(partAttributeManager.SerialNumberName + ": ", SerialNumber);
					result.AppendIfNotEmpty(Res.GetString("7d04d70b-cc50-42fc-a1bb-f51428ff71bb", "EXP:") + " ", ExpiryDate.ToShortDateString());
					result.AppendIfNotEmpty(Res.GetString("f048c368-15a2-464f-a166-2389219444b2", "PKD:") + " ", PackingDate.ToShortDateString());
				}

				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		ZString IPackableItemParent.DescriptionSupplementSeparator => "--";

		#endregion

		#region PackableItems

		IEnumerable<IPackableItem> IPackableItemParent.PackableItems => ParentCollection.GetPackableItems(this);

		#endregion

		#region RefreshPackableItems

		void IPackableItemParent.RefreshPackableItems()
		{
			var query = new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, OrderLine.PK);
			query.AddToFilter(WhsPickLineSchema.WZ_Units, SQLComparisonOperator.GreaterThan, 0m);
			query.FetchOnlyFromLocalCache = true;

			var pickLinesWithChanges = Factory.Load<WhsPickLine>(query).Where(p => p.HasChanges);
			foreach (var pickLine in pickLinesWithChanges)
			{
				ParentCollection.MergePickLine(pickLine);
			}
		}

		#endregion

		#region Quantities / Units

		ZDecimal IPackableItemParent.TotalQty => Quantity;
		ZString IPackableItemParent.TotalQtyUQ => UnitsUQ;

		ZDecimal IPackableItemParent.WeightPerUnit
		{
			get { return SupplierPart?.OP_Weight ?? ZDecimal.Zero; }
		}

		ZString IPackableItemParent.WeightUQ
		{
			get { return SupplierPart?.OP_WeightUQ ?? ZString.Empty; }
		}

		Money IPackableItemParent.UnitPrice
		{
			get
			{
				if (OrderLine.SumOfUnitsMet == 0)
				{
					throw new ArgumentException("Sum of units met cannot be 0.");
				}

				var amount = OrderLine.WE_ExtendedLinePrice / OrderLine.SumOfUnitsMet;
				var currencyCode = !OrderLine.WE_RX_NKUnitPriceCurrency.IsEmpty
					? OrderLine.WE_RX_NKUnitPriceCurrency
					: OrderLine.Docket.GetMostCommonOrderCurrency();

				var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
				return new Money(amount, currency ?? GlbCompany.CurrentCompany.LocalCurrency);
			}
		}

		#endregion

		#region IsMatch

		BarcodeMatch IPackableItemParent.IsMatch(string barcode)
		{
			BarcodeMatch result = BarcodeMatch.No;

			// first look for a barcode
			var part = SupplierPart;
			var orgBarcode = part.PartBarcodes.Cast<OrgSupplierPartBarcode>().FirstOrDefault(b => b.PH_Barcode.EqualsIgnoringCase(barcode));
			if (orgBarcode != null)
			{
				// don't pack into UNT and don't pack into the SKU
				if (orgBarcode.PH_F3_NKPackType.IsEmpty || orgBarcode.PH_F3_NKPackType.EqualsIgnoringCase(Constants.PkgUnit.Unit) || orgBarcode.PH_F3_NKPackType.EqualsIgnoringCase(part.OP_StockKeepingUnit))
				{
					result = BarcodeMatch.Yes;
				}
				// convert from the scanned package type to the SKU
				else
				{
					var unitsToPack = part.UnitConverter.Convert(1, orgBarcode.PH_F3_NKPackType, part.OP_StockKeepingUnit);
					result = new BarcodeMatch(true, orgBarcode.PH_F3_NKPackType, unitsToPack);
				}
			}
			// fallback to the part num
			else if (part.OP_PartNum.EqualsIgnoringCase(barcode))
			{
				result = BarcodeMatch.Yes;
			}

			return result;
		}

		#endregion

		#endregion

		#region IPartAttributes Members

		ZString IPartAttributes.PartAttrib1 => PartAttribute1;
		ZString IPartAttributes.PartAttrib2 => PartAttribute2;
		ZString IPartAttributes.PartAttrib3 => PartAttribute3;
		ZString IPartAttributes.SerialNumber => SerialNumber;
		ZDate IPartAttributes.ExpiryDate => ExpiryDate;
		ZDate IPartAttributes.PackingDate => PackingDate;

		#endregion

		#region IPartAttributeValidationConsumer Members

		public bool IsRegisteredForUniqueSerialNumberChecking => Product?.IsSerialNumberReleaseCaptured(Client) ?? false;

		public bool IsValidForUniqueSerialNumberChecking(ZString serialNumber)
		{
			var result = false;

			if (!serialNumber.IsEmpty)
			{
				var client = Client;
				result = client != null && (Product?.IsSerialNumberReleaseCaptured(client) ?? false);
			}

			return result;
		}

		public bool IsSerialNumberUsedOnThis(ZString serialNumber)
		{
			return SerialNumber == serialNumber && IsValidForUniqueSerialNumberChecking(serialNumber);
		}

		bool IPartAttributeValidationConsumer.IsSerialNumberUsedOnSiblings(ZString serialNumber)
		{
			var parentCollection = ParentCollection;
			return parentCollection.Cast<WhsReleaseLine>().Any(r => r.PK != PK && !parentCollection.IsDuplicateReleaseLine(r) && r.IsSerialNumberUsedOnThis(serialNumber));
		}

		bool IPartAttributeValidationConsumer.IsInventoryAdjustedOutOnSiblings(WhsInventoryView inventory) => false;

		#endregion

		#region ISerialSplittableLine

		bool ISerialSplittableLine.IsFinalised => IsPickFinalised;

		ZDecimal ISerialSplittableLine.Units => Quantity;

		bool ISerialSplittableLine.IsSplittableProduct => IsReleaseCapturedSerialisedProduct();

		public void SplitWhenSerialNumberExists()
		{
			decimal units = Quantity;

			var parentCollection = ParentCollection;
			if (units > 1 && IsReleaseCapturedSerialisedProduct() && !parentCollection.IsNonCommittedCollectionElement(this) && !IsPacked)
			{
				using (PickableDocket.GetValidationSuspender())
				{
					using (SuspendUpdatingSumOfUnitsMetAndOrderFields())
					using (GetValidationSuspender())
					{
						Quantity = 1m;
					}

					// We want to reduce any Release Captured Attributes for the line being split if they exist, so we call UpdateSumOfUnitsMet().
					// However we don't want to change the SumOfUnitsMet total so we pass in the Quantity as the previous Quantity.
					parentCollection.UpdateSumOfUnitsMet(this, Quantity);

					using (parentCollection.SuspendSettingDefaults())
					{
						for (var i = 1; i < units; i++)
						{
							var newLine = parentCollection.AddNew();

							using (newLine.SuspendUpdatingSumOfUnitsMetAndOrderFields())
							using (newLine.GetValidationSuspender())
							{
								newLine.Quantity = 1m;
								WhsReleaseLineCollection.CopyNonReleaseCapturedAttributes(newLine, this);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
