using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderLine : AutoWhsVASOrderLine
	{
		public WhsVASOrderLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region VASOrder

		public WhsVASOrder VASOrder
		{
			get { return Factory.Load<WhsVASOrder>(WVL_WVO_VASOrder); }
		}

		#endregion

		#region Product

		public WhsProduct WarehouseProduct
		{
			get => Factory.GetCachedValue(string.Format(Culture.Invariant, "WhsVASOrderLine|WarehouseProduct|{0}", WVL_OP_Product.ToStringKey()), GetNewProduct); // Constant key used for factory cache
		}

		WhsProduct GetNewProduct()
		{
			var part = WVL_OP_Product.IsValid ? Product : null;
			return part != null ? WhsProduct.GetWhsProduct(part) : null;
		}

		#endregion

		#region ProductDescription

		[ResourceStringData("WhsVASOrderLine|ProductDescription", Caption = "Product Description", ShortCaption = "Product Desc.")]
		public ZString ProductDescription
		{
			get => WarehouseProduct?.Parent.OP_Desc ?? string.Empty;
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region WVL_LineNumber

		[ReadOnly(true)]
		public override ZInt WVL_LineNumber
		{
			get { return base.WVL_LineNumber; }
			set { base.WVL_LineNumber = value; }
		}

		#endregion

		#region WVL_ExpiryDate

		[ReadOnlyMember(nameof(ExpiryDateReadOnly))]
		public override ZDate WVL_ExpiryDate
		{
			get { return base.WVL_ExpiryDate; }
			set { base.WVL_ExpiryDate = value; }
		}

		bool ExpiryDateReadOnly
		{
			get
			{
				var result = true;
				var vasOrder = VASOrder;
				if (vasOrder != null)
				{
					var product = WarehouseProduct;
					var client = vasOrder.Client;
					result = product == null || client == null || !product.IsExpiryDateUsed(client) || product.IsAJulianBatchNumberAttributeUsed(client);
				}

				return result;
			}
		}

		#endregion

		#region WVL_PackingDate

		[ReadOnlyMember(nameof(PackingDateReadOnly))]
		public override ZDate WVL_PackingDate
		{
			get { return base.WVL_PackingDate; }
			set { base.WVL_PackingDate = value; }
		}

		bool PackingDateReadOnly
		{
			get
			{
				var vasOrder = VASOrder;
				return vasOrder == null || WarehouseProduct == null || !WarehouseProduct.IsPackingDateUsed(vasOrder.Client);
			}
		}

		#endregion

		#region Part Attributes

		#region WVL_PartAttrib1

		[ReadOnlyMember(nameof(PartAttrib1ReadOnly))]
		public override ZString WVL_PartAttrib1
		{
			get { return base.WVL_PartAttrib1; }
			set { base.WVL_PartAttrib1 = value; }
		}

		bool PartAttrib1ReadOnly
		{
			get { return PartAttribReadOnly(1); }
		}

		#endregion

		#region WVL_PartAttrib2

		[ReadOnlyMember(nameof(PartAttrib2ReadOnly))]
		public override ZString WVL_PartAttrib2
		{
			get { return base.WVL_PartAttrib2; }
			set { base.WVL_PartAttrib2 = value; }
		}

		bool PartAttrib2ReadOnly
		{
			get { return PartAttribReadOnly(2); }
		}

		#endregion

		#region WVL_PartAttrib3

		[ReadOnlyMember(nameof(PartAttrib3ReadOnly))]
		public override ZString WVL_PartAttrib3
		{
			get { return base.WVL_PartAttrib3; }
			set { base.WVL_PartAttrib3 = value; }
		}

		bool PartAttrib3ReadOnly
		{
			get { return PartAttribReadOnly(3); }
		}

		#endregion

		bool PartAttribReadOnly(int partAttrib)
		{
			var vasOrder = VASOrder;
			return vasOrder == null || (!WarehouseProduct?.IsPartAttributeUsed(vasOrder.Client, partAttrib) ?? true);
		}

		#region WVL_SerialNumber

		[ReadOnlyMember(nameof(SerialNumberReadOnly))]
		public override ZString WVL_SerialNumber
		{
			get { return base.WVL_SerialNumber; }
			set
			{
				base.WVL_SerialNumber = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWVL_Quantity();
				}
			}
		}

		bool SerialNumberReadOnly
		{
			get
			{
				var vasOrder = VASOrder;
				return vasOrder == null
					|| (!WarehouseProduct?.IsSerialNumberUsed(vasOrder.Client) ?? true);
			}
		}

		#endregion

		#endregion

		#region WVL_WVO_VASOrder

		[RelatedBusinessObject("VASOrder")]
		public override ZGuid WVL_WVO_VASOrder
		{
			get { return base.WVL_WVO_VASOrder; }
			set { base.WVL_WVO_VASOrder = value; }
		}

		#endregion

		#endregion

		// Overrides

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly || IsVASOrderReadOnly; }
			set { base.ReadOnly = value; }
		}

		bool IsVASOrderReadOnly
		{
			get
			{
				var vasOrder = VASOrder;
				return vasOrder != null && vasOrder.ReadOnly;
			}
		}

		#endregion

		#region Save

		public override void OnSaving()
		{
			base.OnSaving();

			var vasOrder = VASOrder;
			if (IsInDatabase && vasOrder != null && vasOrder.WVO_WD_TransferIntoServiceAreaInfo.OriginalValue.IsValid)
			{
				var areAnyPropertiesChanged = ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(propertyInfo => propertyInfo.HasChanges);
				if (areAnyPropertiesChanged)
				{
					throw new ZCannotSaveException(Res.GetString("WhsVASOrderLine|PropertiesMustNotBeChanged", "Cannot save as fields are modified after Into Service Area Transfer is created."),
						Res.GetString("WhsVASOrderLine|TransferringHasStarted", "VAS Order Transferring has Started."), ExceptionType.BusinessFailure);
				}
			}

			UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsVASOrder>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, WVL_WVO_VASOrder);
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsVASOrderLineFetchStrategy(this);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (IsInDatabase)
			{
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsVASOrder>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, WVL_WVO_VASOrder);
			}
			base.Delete();
		}

		#endregion

		// Interfaces

		#region ITemplateCopyable

		#region SupportsCloneCore

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get
			{
				var vasOrder = VASOrder;
				return base.CanDelete && (vasOrder == null || (!vasOrder.WVO_WD_TransferIntoServiceArea.IsValid && !vasOrder.IsCancelled));
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var vasOrder = VASOrder;
				return vasOrder.IsCancelled
					? ResString.GetMultilingualString("WhsVASOrderLine|ReasonForNotAbleToDelete-Cancelled", "VAS Order lines cannot be deleted on Inactive VAS Orders.")
					: ResString.GetMultilingualString("WhsVASOrderLine|ReasonForNotAbleToDelete", "Transfer has already started. VAS Order line cannot be deleted.");
			}
		}

		#endregion

		// Testing

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			WVL_Quantity = 1m;
		}
#endif
		#endregion
	}
}
