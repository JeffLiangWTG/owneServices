using System;
using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class ComInvHeaderReconciliation : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string InvoiceNumber = "InvoiceNumber";
			public const string InvoiceDate = "InvoiceDate";
			public const string CurrencyCode = "CurrencyCode";
			public const string SupplierCode = "SupplierCode";
			public const string InvoiceTotal = "InvoiceTotal";
			public const string InvoiceLinesTotal = "InvoiceLinesTotal";
			public const string IncoTerm = "IncoTerm";
		}

		#endregion

		public ComInvHeaderReconciliation(BusinessObjectFactory factory, ZGuid supplierPK, ZString invoiceNumber, ZDateTime invoiceDate, ZString currencyCode, ZString incoTerm)
			: base(factory)
		{
			SupplierPK = supplierPK;
			InvoiceNumber = invoiceNumber;
			CurrencyCode = currencyCode;
			IncoTerm = incoTerm;
			fInvoiceTotal = 0m;
			if (invoiceDate.IsValid)
			{
				InvoiceDate = invoiceDate;
			}
		}

		#region Properties

		#region InvoiceNumber

		public ZString InvoiceNumber { get; }

		public ZPropertyInfo InvoiceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceNumber); }
		}

		#endregion

		#region InvoiceDate

		public ZDateTime InvoiceDate { get; }

		public ZPropertyInfo InvoiceDateInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceDate); }
		}

		#endregion

		#region Currency

		public RefCurrency Currency
		{
			get
			{
				return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCode);
			}
		}

		public ZString CurrencyCode { get; }

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CurrencyCode); }
		}

		#endregion

		#region Supplier

		public OrgHeader Supplier => Factory.Load<OrgHeader>(SupplierPK);

		public readonly ZGuid SupplierPK;

		public ZString SupplierCode
		{
			get { return Supplier != null ? Supplier.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo SupplierCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SupplierCode); }
		}

		#endregion

		#region InvoiceTotal

		public ZDecimal InvoiceTotal
		{
			get { return fInvoiceTotal; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceTotalInfo, ref fInvoiceTotal, value);
				if (!IsValidationSuspended)
				{
					ValidateInvoiceTotal();
				}
			}
		}
		ZDecimal fInvoiceTotal;

		public void ValidateInvoiceTotal()
		{
			InvoiceTotalInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(InvoiceTotalInfo, 19, 4);
			ValidateLinesSumEqualToInvoiceTotal();
		}

		public ZPropertyInfo InvoiceTotalInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceTotal); }
		}

		#endregion

		#region IncoTerm

		public ZString IncoTerm { get; }

		public ZPropertyInfo IncoTermInfo
		{
			get { return GetZPropertyInfo(Schema.IncoTerm); }
		}

		#endregion

		#region InvoiceLinesTotal

		public ZDecimal InvoiceLinesTotal
		{
			get { return GetInvoiceLinesTotal(InvoiceLines); }
		}

		public ZPropertyInfo InvoiceLinesTotalInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceLinesTotal); }
		}

		protected ZDecimal GetInvoiceLinesTotal(IEnumerable lines)
		{
			ZDecimal result = 0m;

			foreach (ComInvOrderLineReconciliation line in lines)
			{
				result += line.JO_Recon_LinePrice;
			}

			return result;
		}

		#endregion

		#region InvoiceLines

		public ComInvLineCollection InvoiceLines
		{
			get
			{
				if (fInvoiceLines == null)
				{
					fInvoiceLines = new ComInvLineCollection(Factory);
					LoadComInvOrderLineReconciliations();
				}

				return fInvoiceLines;
			}
		}

		ComInvLineCollection fInvoiceLines;

		void LoadComInvOrderLineReconciliations()
		{
			foreach (ComInvOrderReconciliation order in ComInvOrders)
			{
				AddInvoiceLines(order);
			}
		}

		void AddInvoiceLines(ComInvOrderReconciliation order)
		{
			var orderlines = order.OrderLines;
			InvoiceLines.AddRange(orderlines);
			SetLinePriceEvent(orderlines);
		}

		void SetLinePriceEvent(IEnumerable lines)
		{
			foreach (ComInvOrderLineReconciliation line in lines)
			{
				line.JO_Recon_LinePriceInfo.ValueChanged += new EventHandler(InvoiceLinePrice_HasChanged);
			}
		}

		void InvoiceLinePrice_HasChanged(object sender, EventArgs e)
		{
			if (!IsValidationSuspended)
			{
				ValidateInvoiceTotal();
			}
		}

		#endregion

		#region ComInvOrders

		public ComInvOrderReconciliationCollection ComInvOrders
		{
			get
			{
				if (fOrders == null)
				{
					fOrders = new ComInvOrderReconciliationCollection(Factory);
				}
				return fOrders;
			}
		}

		public void AddComInvOrder(ComInvOrderReconciliation comInvOrder)
		{
			ComInvOrders.Add(comInvOrder);
			AddInvoiceLines(comInvOrder);
			InvoiceTotal += GetInvoiceLinesTotal(comInvOrder.OrderLines);
		}

		ComInvOrderReconciliationCollection fOrders;

		#endregion

		#endregion

		#region Implementation

		void ValidateLinesSumEqualToInvoiceTotal()
		{
			int currencyDecimals = (Currency == null) ? 2 : Currency.Decimals;
			ZDecimal totalDifferences = Utilities.Round(InvoiceTotal - InvoiceLinesTotal, currencyDecimals);
			if (totalDifferences != 0m)
			{
				string sumTotalWarning = Res.GetString("74480ad6-59fc-4fd8-9f9d-300b71d9d626", "Invoice Header amount does not equal the Total Lines Amount. It is out by {0}", totalDifferences.ToString(currencyDecimals));
				if (!InvoiceTotalInfo.GetWarnings().Contains(sumTotalWarning))
				{
					InvoiceTotalInfo.AddWarning(sumTotalWarning);
				}
			}
		}

		#endregion
	}
}
