using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsRMAOrderLine : NonPersistentBusinessObject
	{
		#region Constructor

		WhsRMAOrderLine(WhsDocketLine parentInventory, WhsOrder order, ZDecimal quantity, ZDecimal availableQty, bool shouldCopyBOMLinks = false)
			: base(parentInventory.Factory)
		{
			ProductPK = parentInventory.WE_OP;
			Quantity = quantity;
			QuantityToReturn = ZDecimal.Zero;
			ExpiryDate = parentInventory.WE_ExpiryDate;
			PackingDate = parentInventory.WE_PackingDate;
			PartAttrib1 = parentInventory.WE_PartAttrib1;
			PartAttrib2 = parentInventory.WE_PartAttrib2;
			PartAttrib3 = parentInventory.WE_PartAttrib3;
			SerialNumber = parentInventory.WE_SerialNumber;
			originalWarehousePK = order.WD_WW_Whs;
			ExternalReference = order.WD_ExternalReference;
			parentOrderPK = order.PK;
			parentInventoryPK = parentInventory.PK;
			ShouldCopyBOMLinks = shouldCopyBOMLinks;
			Key = WhsRMAHelper.GetKey(parentInventory);
			AvailableQtyToReturn = availableQty;
		}

		#region GetWhsRMAOrderLine

		public static WhsRMAOrderLine GetWhsRMAOrderLine(WhsDocketLine parentInventory, WhsOrder order, ZDecimal quantity, ZDecimal availableQty, bool shouldCopyBOMLinks = false)
		{
			Argument.NotNull(parentInventory, nameof(parentInventory));
			Argument.NotNull(order, nameof(order));
			Argument.NotNull(quantity, nameof(quantity));

			return new WhsRMAOrderLine(parentInventory, order, quantity, availableQty, shouldCopyBOMLinks);
		}

		#endregion
		public ZGuid OriginalWarehousePK => originalWarehousePK;
		readonly ZGuid originalWarehousePK;

		#endregion

		#region Related Entities

		#region Product

		public OrgSupplierPart Product => Factory.Load<OrgSupplierPart>(ProductPK);

		#endregion

		#region ParentOrder

		public WhsOrder ParentOrder => Factory.Load<WhsOrder>(parentOrderPK);

		#endregion

		#region ParentInventory

		public WhsDocketLine ParentInventory => Factory.Load<WhsDocketLine>(parentInventoryPK);

		#endregion

		#endregion

		#region Properties

		#region ProductPK

		[RelatedBusinessObject("Product")]
		[List("PartCollection")]
		public ZGuid ProductPK
		{
			get => productPK;
			private set => SetNonPersistentPropertyValue(ProductPKInfo, ref productPK, value);
		}

		public ZPropertyInfo ProductPKInfo => GetZPropertyInfo(nameof(ProductPK));

		ZGuid productPK;

		public OrgSupplierPartCollection PartCollection => new OrgSupplierPartCollection(Factory);

		#endregion

		#region Quantity

		public ZDecimal Quantity
		{
			get => quantity;
			private set => SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value);
		}

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(nameof(Quantity));
		ZDecimal quantity;

		#endregion

		#region QuantityToReturn

		public ZDecimal QuantityToReturn
		{
			get => quantityToReturn;
			set
			{
				SetNonPersistentPropertyValue(QuantityToReturnInfo, ref quantityToReturn, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateQuantityToReturn();
				}
				QuantityToReturnInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo QuantityToReturnInfo => GetZPropertyInfo(nameof(QuantityToReturn));
		ZDecimal quantityToReturn;

		#endregion

		#region WhsOverride

		[List("Lookups.Warehouses")]
		[ResourceStringData("WhsRMAOrderLine|WhsOverride", Caption = "Warehouse Override", ShortCaption = "Whs Ovr.", MediumCaption = "Whs Override")]
		public ZGuid WhsOverride
		{
			get => whsOverride;
			set
			{
				SetNonPersistentPropertyValue(WhsOverrideInfo, ref whsOverride, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateWhsOverride();
				}
			}
		}

		public ZPropertyInfo WhsOverrideInfo => GetZPropertyInfo(nameof(WhsOverride));

		ZGuid whsOverride;
		
		#endregion

		#region Attributes

		#region ExpiryDate

		public ZDate ExpiryDate
		{
			get => expiryDate;
			private set => SetNonPersistentPropertyValue(ExpiryDateInfo, ref expiryDate, value);
		}

		public ZPropertyInfo ExpiryDateInfo => GetZPropertyInfo(nameof(ExpiryDate));
		ZDate expiryDate;

		#endregion

		#region PackingDate

		public ZDate PackingDate
		{
			get => packingDate;
			private set => SetNonPersistentPropertyValue(PackingDateInfo, ref packingDate, value);
		}

		public ZPropertyInfo PackingDateInfo => GetZPropertyInfo(nameof(PackingDate));
		ZDate packingDate;

		#endregion

		#region PartAttrib1

		public ZString PartAttrib1
		{
			get => partAttrib1;
			private set => SetNonPersistentPropertyValue(PartAttrib1Info, ref partAttrib1, value);
		}

		public ZPropertyInfo PartAttrib1Info => GetZPropertyInfo(nameof(PartAttrib1));
		ZString partAttrib1;

		#endregion

		#region PartAttrib2

		public ZString PartAttrib2
		{
			get => partAttrib2;
			private set => SetNonPersistentPropertyValue(PartAttrib2Info, ref partAttrib2, value);
		}

		public ZPropertyInfo PartAttrib2Info => GetZPropertyInfo(nameof(PartAttrib2));
		ZString partAttrib2;

		#endregion

		#region PartAttrib3

		public ZString PartAttrib3
		{
			get => partAttrib3;
			private set => SetNonPersistentPropertyValue(PartAttrib3Info, ref partAttrib3, value);
		}

		public ZPropertyInfo PartAttrib3Info => GetZPropertyInfo(nameof(PartAttrib3));
		ZString partAttrib3;

		#endregion

		public ZString SerialNumber
		{
			get => serialNumber;
			private set => SetNonPersistentPropertyValue(SerialNumberInfo, ref serialNumber, value);
		}

		public ZPropertyInfo SerialNumberInfo => GetZPropertyInfo(nameof(SerialNumber));
		ZString serialNumber;

		#endregion

		#region ShouldCopyBOMLinks

		public bool ShouldCopyBOMLinks { get; }

		#endregion

		#region ExternalReference

		public ZString ExternalReference { get; }

		#endregion

		public string Key { get; }

		public ZDecimal AvailableQtyToReturn
		{
			get => availableQtyToReturn;
			private set => SetNonPersistentPropertyValue(AvailableQtyToReturnInfo, ref availableQtyToReturn, value);
		}

		public ZPropertyInfo AvailableQtyToReturnInfo => GetZPropertyInfo(nameof(AvailableQtyToReturn));
		ZDecimal availableQtyToReturn;

		#endregion

		#region Field

		readonly ZGuid parentOrderPK;

		readonly ZGuid parentInventoryPK;

		#endregion

		#region Validation

		public WhsRMAOrderLineValidation Validation => GetNewValidation();

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		protected virtual WhsRMAOrderLineValidation GetNewValidation()
		{
			return new WhsRMAOrderLineValidation(this);
		}

		#endregion

		#region Lookups

		public WhsRMAOrderLineLookups Lookups => lookups ?? (lookups = new WhsRMAOrderLineLookups(this));

		WhsRMAOrderLineLookups lookups;

		#endregion
	}
}
