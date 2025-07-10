using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveProductSummary : NonPersistentBusinessObject<WhsReceiveProductSummaryValidation>
	{
		public WhsReceiveProductSummary(BusinessObjectFactory factory, ZGuid clientPk, ZGuid warehousePK,
			ZGuid productPk, ZBool isBlindReceive, ZString productCode, ZString productDescription,
			ZString receiveCategory, ZDecimal expectedQty, ZDecimal receivedQty)
			: base(factory)
		{
			ClientPk = clientPk;
			WarehousePK = warehousePK;
			ProductPk = productPk;
			IsBlindReceive = isBlindReceive;
			ProductCode = productCode;
			ProductDescription = productDescription;
			ReceiveCategory = receiveCategory;
			ExpectedQuantity = expectedQty;
			ReceivedQuantity = receivedQty;
		}

		public abstract class Schema
		{
			public const string ProductCode = nameof(ProductCode);
			public const string ProductDescription = nameof(ProductDescription);
			public const string ExpectedQuantity = nameof(ExpectedQuantity);
			public const string ReceivedQuantity = nameof(ReceivedQuantity);
		}

		[ResourceStringData("WhsReceiveProductSummary|ProductCode", Caption = "Product")]
		public ZString ProductCode { get; private set; }

		[ResourceStringData("WhsReceiveProductSummary|ProductDescription", Caption = "Product Description", MediumCaption = "Product Desc.")]
		public ZString ProductDescription { get; private set; }

		[ResourceStringData("WhsReceiveProductSummary|ExpectedQuantity", Caption = "Expected Quantity", MediumCaption = "Expected Qty", ShortCaption = "Exp. Qty")]
		public ZDecimal ExpectedQuantity { get; private set; }

		[ResourceStringData("WhsReceiveProductSummary|ReceivedQuantity", Caption = "Received Quantity", MediumCaption = "Received Qty", ShortCaption = "Rec. Qty")]
		public ZDecimal ReceivedQuantity
		{
			get { return receivedQuantity; }
			set
			{
				SetNonPersistentPropertyValue(ReceivedQuantityInfo, ref receivedQuantity, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateReceivedQuantity();
				}
			}
		}

		ZDecimal receivedQuantity;

		public ZPropertyInfo ReceivedQuantityInfo => GetZPropertyInfo(Schema.ReceivedQuantity);

		public ZGuid ProductPk { get; private set; }

		public ZGuid ClientPk { get; private set; }

		public ZGuid WarehousePK { get; private set; }

		public ZBool IsBlindReceive { get; private set; }

		public ZString ReceiveCategory { get; private set; }

		public OrgSupplierPart Product => Factory.Load<OrgSupplierPart>(ProductPk);

		public OrgHeader Client => Factory.Load<OrgHeader>(ClientPk);

		#region Validation

		public override WhsReceiveProductSummaryValidation GetNewValidation() => new WhsReceiveProductSummaryValidation(this);

		#endregion
	}
}
