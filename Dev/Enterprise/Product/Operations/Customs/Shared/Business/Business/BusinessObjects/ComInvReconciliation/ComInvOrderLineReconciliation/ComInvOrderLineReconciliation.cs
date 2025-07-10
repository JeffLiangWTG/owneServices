using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public enum ComInvReconciliationQuantityType
	{
		OrderQuantity,
		ReceivedQuantity,
		InvoiceQuantity
	}

	public class ComInvOrderLineReconciliation : OrderLine
	{
		#region Schema

		public new class Schema : AutoJobOrderLine.Schema
		{
			public const string JO_Recon_Quantity = "JO_Recon_Quantity";
			public const string JO_Recon_ItemPrice = "JO_Recon_ItemPrice";
			public const string JO_Recon_LinePrice = "JO_Recon_LinePrice";
			public const string JO_OrderNumberAndSplit = "JO_OrderNumberAndSplit";
		}

		#endregion

		public ComInvOrderLineReconciliation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			QuantityType = ComInvReconciliationQuantityType.OrderQuantity;
			SetDefaultJO_Recon_ItemPrice();
		}

		#region Clone

		public void CopyPersistentValuesToAnotherFactory(BusinessObjectFactory anotherFactory)
		{
			ComInvOrderLineReconciliation comInvOrderLine = anotherFactory.Load<ComInvOrderLineReconciliation>(PK);
			if (comInvOrderLine == null)
			{
				anotherFactory.ImportFromAnotherFactory(this);
			}
			else
			{
				comInvOrderLine.CopyPersistentValuesFrom(this);
			}
		}

		protected override Type GetOrderType()
		{
			return typeof(ComInvOrderReconciliation);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			ComInvOrderLineReconciliation result = (ComInvOrderLineReconciliation)base.CloneInternal(args);
			result.fJO_Recon_ItemPrice = fJO_Recon_ItemPrice;
			result.fJO_Recon_LinePrice = fJO_Recon_LinePrice;
			result.fQuantityType = fQuantityType;
			return result;
		}

		#endregion

		#region Properties

		#region QuantityType

		public ComInvReconciliationQuantityType QuantityType
		{
			get { return fQuantityType; }
			set
			{
				if (fQuantityType != value)
				{
					fQuantityType = value;
					RecalculateJO_Recon_LinePrice();
				}
			}
		}

		ComInvReconciliationQuantityType fQuantityType;

		#endregion

		#region JO_Recon_Quantity

		[DecimalPlaces(2)]
		public ZDecimal JO_Recon_Quantity
		{
			get
			{
				ZDecimal result = JO_Quantity;

				if (QuantityType == ComInvReconciliationQuantityType.ReceivedQuantity)
				{
					result = JO_QtyReceived;
				}
				else if (QuantityType == ComInvReconciliationQuantityType.InvoiceQuantity)
				{
					result = JO_QtyInvoiced;
				}

				return result;
			}
			set
			{
				if (QuantityType == ComInvReconciliationQuantityType.ReceivedQuantity)
				{
					JO_QtyReceived = value;
				}
				else if (QuantityType == ComInvReconciliationQuantityType.InvoiceQuantity)
				{
					JO_QtyInvoiced = value;
				}
				else
				{
					JO_Quantity = value;
				}

				JO_Recon_QuantityInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateJO_Recon_Quantity();
				}

				if (!IsCopying)
				{
					RecalculateJO_Recon_LinePrice();
				}
			}
		}

		public ZPropertyInfo JO_Recon_QuantityInfo
		{
			get { return GetZPropertyInfo(Schema.JO_Recon_Quantity); }
		}

		#endregion

		#region JO_Recon_ItemPrice

		[DecimalPlaces(4)]
		public ZDecimal JO_Recon_ItemPrice
		{
			get { return fJO_Recon_ItemPrice; }
			set
			{
				bool isDiff = (JO_Recon_ItemPrice != value);
				SetNonPersistentPropertyValue(JO_Recon_ItemPriceInfo, ref fJO_Recon_ItemPrice, value);
				if (isDiff)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateJO_Recon_ItemPrice();
					}

					if (!IsCopying)
					{
						RecalculateJO_Recon_LinePrice();
					}
				}
			}
		}

		ZDecimal fJO_Recon_ItemPrice;

		public ZPropertyInfo JO_Recon_ItemPriceInfo
		{
			get { return GetZPropertyInfo(Schema.JO_Recon_ItemPrice); }
		}

		#endregion

		#region JO_Recon_LinePrice

		[DecimalPlaces(2)]
		public ZDecimal JO_Recon_LinePrice
		{
			get { return fJO_Recon_LinePrice; }
			set
			{
				bool isDiff = fJO_Recon_LinePrice != value;
				SetNonPersistentPropertyValue(JO_Recon_LinePriceInfo, ref fJO_Recon_LinePrice, value);
				if (isDiff)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateJO_Recon_LinePrice();
					}

					if (!IsCopying)
					{
						RecalculateJO_Recon_ItemPrice();
					}
				}
			}
		}

		ZDecimal fJO_Recon_LinePrice;

		public ZPropertyInfo JO_Recon_LinePriceInfo
		{
			get { return GetZPropertyInfo(Schema.JO_Recon_LinePrice); }
		}

		#endregion

		#region JO_OrderNumberAndSplit

		public ZString JO_OrderNumberAndSplit
		{
			get { return Order == null ? ZString.Empty : Order.JD_OrderNumberAndSplit; }
		}

		public ZPropertyInfo JO_OrderNumberAndSplitInfo
		{
			get { return GetZPropertyInfo(Schema.JO_OrderNumberAndSplit); }
		}

		#endregion

		#region IsValid

		public bool IsValid
		{
			get { return JO_Recon_Quantity != 0; }
		}

		#endregion

		#endregion

		#region Overrides

		[DecimalPlaces(4)]
		public override ZDecimal JO_ItemPrice
		{
			get { return base.JO_ItemPrice; }
			set { base.JO_ItemPrice = value; }
		}

		#endregion

		#region Validation

		public new ComInvOrderLineReconciliationValidation Validation
		{
			get { return (ComInvOrderLineReconciliationValidation)base.Validation; }
		}

		protected override JobOrderLineValidation GetNewValidation()
		{
			return new ComInvOrderLineReconciliationValidation(this);
		}

		#endregion

		#region Implementation

		void SetDefaultJO_Recon_ItemPrice()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery query = new ZQuery(JobComInvoiceLineSchema.JI_JO, PK);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			BaseJobComInvoiceLine jobComInvoiceLine = newFactory.LoadTop1<BaseJobComInvoiceLine>(query);
			if (jobComInvoiceLine != null)
			{
				JO_Recon_ItemPrice = jobComInvoiceLine.UnitPrice;
			}
			else
			{
				JO_Recon_ItemPrice = JO_ItemPrice;
			}
		}

		int CurrencyDecimals
		{
			get { return (Order == null || Order.OrderCurrency == null) ? 4 : Order.OrderCurrency.Decimals; }
		}

		void RecalculateJO_Recon_LinePrice()
		{
			ZDecimal newLineValue = Utilities.Round(JO_Recon_ItemPrice * JO_Recon_Quantity, CurrencyDecimals);
			if (JO_Recon_LinePrice != newLineValue)
			{
				JO_Recon_LinePrice = newLineValue;
			}
		}

		void RecalculateJO_Recon_ItemPrice()
		{
			if (JO_Recon_Quantity > 0)
			{
				ZDecimal newItemValue = Utilities.Round(JO_Recon_LinePrice / JO_Recon_Quantity, 4);
				if (JO_Recon_ItemPrice != newItemValue)
				{
					JO_Recon_ItemPrice = newItemValue;
				}
			}
		}

		#endregion
	}
}
