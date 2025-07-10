using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class ComInvoiceReconciliator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ComInvoiceReconciliator(BaseJobDeclaration declaration, Order[] selectedOrders, ComInvReconciliationQuantityType quantityType)
			: base(new BusinessObjectFactory())
		{
			this.declaration = declaration;
			this.quantityType = quantityType;
			LoadComInvHeadersFromSelectedOrders(selectedOrders);
		}

#if DEBUG
		public virtual void ImportInvoices()
#else
		public void ImportInvoices()
#endif
		{
			using (ObjectFactory.Get<ISuppressHookHelper>().SuppressFieldOnChangeHook())
			{
				CopyInvoiceLinesPersistentValuesToDeclarationFactory();
				ProcessComInvHeaders();
				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		#region Properties

		public ComInvHeaderReconciliationCollection ComInvHeaders
		{
			get
			{
				if (fComInvHeaders == null)
				{
					fComInvHeaders = new ComInvHeaderReconciliationCollection(Factory);
				}

				return fComInvHeaders;
			}
		}

		#endregion

		#region Implementation

		#region Load ComInvHeader From Selected Orders

		void LoadComInvHeadersFromSelectedOrders(Order[] selectedOrders)
		{
			foreach (Order selectedOrder in selectedOrders)
			{
				var comInvOrder = Factory.Load<ComInvOrderReconciliation>(selectedOrder.PK);

				if (comInvOrder != null)
				{
					comInvOrder.QuantityType = quantityType;
					comInvOrder.JD_InvoiceNumber = selectedOrder.JD_InvoiceNumber;
					comInvOrder.JD_InvoiceDate = selectedOrder.JD_InvoiceDate;

					ComInvHeaderReconciliation comInvHeader = GetComInvHeader(comInvOrder);
					comInvHeader.AddComInvOrder(comInvOrder);
				}
			}
		}

		ComInvHeaderReconciliation GetComInvHeader(ComInvOrderReconciliation comInvOrder)
		{
			ComInvHeaderReconciliation result = ComInvHeaders[comInvOrder];

			if (result == null)
			{
				result = new ComInvHeaderReconciliation(Factory, comInvOrder.SupplierPK, comInvOrder.InvoiceNumber, comInvOrder.InvoiceDate, comInvOrder.JD_RX_NKOrderCurrency, comInvOrder.JD_IncoTerm);
				ComInvHeaders.Add(result);
			}

			return result;
		}

		#endregion

		#region Copy Order Changes To Declaration's Factory

		void CopyInvoiceLinesPersistentValuesToDeclarationFactory()
		{
			ComInvHeaders.CopyInvoiceLinesPersistentValuesToAnotherFactory(DeclarationFactory);
		}

		BusinessObjectFactory DeclarationFactory
		{
			get { return declaration == null ? null : declaration.Factory; }
		}

		#endregion

		#region Process Commercial Invoices

		void ProcessComInvHeaders()
		{
			foreach (ComInvHeaderReconciliation comInvHeader in ComInvHeaders)
			{
				ProcessComInvHeader(comInvHeader);
			}
		}

		void ProcessComInvHeader(ComInvHeaderReconciliation comInvHeader)
		{
			BaseJobComInvoiceHeader header = GetInvoiceHeader(comInvHeader);
			if (header != null)
			{
				AddComInvLinesToHeader(comInvHeader.InvoiceLines, header);
			}
		}

		#region Process Invoice Header

		BaseJobComInvoiceHeader GetInvoiceHeader(ComInvHeaderReconciliation comInvHeader)
		{
			BaseJobComInvoiceHeader result = null;
			if (comInvHeader != null)
			{
				result = GetMatchingInvoiceHeader(comInvHeader);

				if (result == null)
				{
					result = declaration.Invoices.AddNew();
					result.JZ_InvoiceNumber = comInvHeader.InvoiceNumber;
					result.JZ_OH_Supplier = comInvHeader.SupplierPK;
					result.JZ_IncoTerm = comInvHeader.IncoTerm;
					result.JZ_RX_NKInvoice_Currency = comInvHeader.CurrencyCode;
					result.JZ_InvoiceDate = comInvHeader.InvoiceDate;
				}
			}

			return result;
		}

		BaseJobComInvoiceHeader GetMatchingInvoiceHeader(ComInvHeaderReconciliation comInvHeader)
		{
			BaseJobComInvoiceHeader result = null;
			if (comInvHeader != null && comInvHeader.SupplierPK.IsValid)
			{
				ZQuery filter = new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, comInvHeader.InvoiceNumber);
				filter.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceDate, comInvHeader.InvoiceDate);
				filter.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, comInvHeader.SupplierPK);
				filter.AddToFilter(JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency, comInvHeader.CurrencyCode);
				filter.AddToFilter(JobComInvoiceHeaderSchema.JZ_IncoTerm, comInvHeader.IncoTerm);
				List<BaseJobComInvoiceHeader> matchInvoices = new List<BaseJobComInvoiceHeader>(declaration.Invoices.Find(filter));
				result = matchInvoices.Count > 0 ? matchInvoices[0] : null;
			}

			if ((result == null) && (declaration.Invoices.Count > 0) && (declaration.InvoiceLines.Count == 0))
			{
				result = GetEmptyInvoiceHeader(comInvHeader);
			}

			return result;
		}

		BaseJobComInvoiceHeader GetEmptyInvoiceHeader(ComInvHeaderReconciliation comInvHeader)
		{
			BaseJobComInvoiceHeader result = null;

			if (comInvHeader != null)
			{
				ZQuery filter = new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, ZString.Empty);
				List<BaseJobComInvoiceHeader> matchInvoices = new List<BaseJobComInvoiceHeader>(declaration.Invoices.Find(filter));
				result = matchInvoices.Count > 0 ? matchInvoices[0] : null;
			}

			if (result != null)
			{
				result.JZ_InvoiceNumber = comInvHeader.InvoiceNumber;
				result.JZ_InvoiceDate = comInvHeader.InvoiceDate;
				result.JZ_OH_Supplier = comInvHeader.SupplierPK;
				result.JZ_RX_NKInvoice_Currency = comInvHeader.CurrencyCode;
				result.JZ_IncoTerm = comInvHeader.IncoTerm;
			}

			return result;
		}

		#endregion

		#region Add Invoice Lines

		void AddComInvLinesToHeader(ComInvLineCollection comInvLines, BaseJobComInvoiceHeader header)
		{
			if (header != null)
			{
				foreach (ComInvOrderLineReconciliation comInvOrderLine in comInvLines)
				{
					BaseJobComInvoiceLine invoiceLine = GetInvoiceLineIfExists(header.PK, comInvOrderLine);
					if (invoiceLine == null && comInvOrderLine.IsValid)
					{
						invoiceLine = header.JobComInvoiceLines.AddNew();
					}

					if (invoiceLine != null)
					{
						header.JZ_InvoiceAmount -= invoiceLine.JI_LinePrice;
						UpdateInvoiceLineFromOrderLine(comInvOrderLine, invoiceLine);
						header.JZ_InvoiceAmount += invoiceLine.JI_LinePrice;
					}
				}
			}
		}

		BaseJobComInvoiceLine GetInvoiceLineIfExists(ZGuid headerPK, ComInvOrderLineReconciliation comInvOrderLine)
		{
			BaseJobComInvoiceLine result = null;

			ZQuery query = new ZQuery(JobComInvoiceLineSchema.JI_JO, comInvOrderLine.PK);
			query.FetchOnlyFromLocalCache = !comInvOrderLine.IsInDatabase;
			BaseJobComInvoiceLine line = DeclarationFactory.LoadTop1<BaseJobComInvoiceLine>(query);
			if (line != null && line.JI_JZ != headerPK)
			{
				line.JI_JO = ZGuid.Empty;
			}
			else
			{
				result = line;
			}
			return result;
		}

		protected virtual void UpdateInvoiceLineFromOrderLine(ComInvOrderLineReconciliation orderLine, BaseJobComInvoiceLine invoiceLine)
		{
			// Set general properties:
			((Integration.Customs.IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);
			// Now set recon-specific properties
			invoiceLine.JI_InvoiceQuantity = orderLine.JO_Recon_Quantity;
			invoiceLine.JI_LinePrice = orderLine.JO_Recon_LinePrice;
			invoiceLine.UnitPrice = orderLine.JO_Recon_ItemPrice;

			var originalOrderLineQuantity = (ZDecimal)orderLine.JO_QuantityInfo.OriginalValue;  // We need to use the value of JO_Quantity as it was when the recon form loaded, not as it is after the user adjusts the numbers. 
																								//	Otherwise if we used the (new, post-reconcile) value of JO_Quantity will only ever bring in 100% of the mass and volume, because the mass-per-unit will be incorrectly calculated. 
			var weightRatio = new ZDecimal(originalOrderLineQuantity > 0 ? orderLine.JO_ActualWeight / originalOrderLineQuantity : 1m);
			var volumeRatio = new ZDecimal(originalOrderLineQuantity > 0 ? orderLine.JO_ActualVolume / originalOrderLineQuantity : 1m);

			invoiceLine.JI_Weight = invoiceLine.JI_InvoiceQuantity * weightRatio;
			invoiceLine.JI_WeightUQ = orderLine.JO_UnitOfWeight;
			invoiceLine.JI_Volume = invoiceLine.JI_InvoiceQuantity * volumeRatio;
			invoiceLine.JI_VolumeUQ = orderLine.JO_UnitOfVolume;
		}

		#endregion

		#endregion

		readonly BaseJobDeclaration declaration;
		readonly ComInvReconciliationQuantityType quantityType;
		ComInvHeaderReconciliationCollection fComInvHeaders;

		#endregion

	}
}
