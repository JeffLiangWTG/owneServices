using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.DataRegistry.Business;

namespace Enterprise.Customs.Business
{
	public class DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader newElement, BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
			this.newElement = newElement;
		}

		public void DefaultForNewElement()
		{
			if (declaration != null)
			{
				DefaultForNewElementCore();
			}
		}

		protected virtual void DefaultForNewElementCore()
		{
			IEnumerable<BaseJobComInvoiceHeader> invoices;
			if (declaration.SupportAdditionalInvoices)
			{
				invoices = declaration.Invoices.Where(i => !declaration.Invoices.IsAdditionalInvoice(i));
			}
			else
			{
				invoices = declaration.Invoices;
			}

			var previousInvoice = invoices.LastOrDefault();
			if (previousInvoice == null)
			{
				SetDefaultsForFirstInvoiceCore();
			}
			else
			{
				SetDefaultsForAdditionalInvoiceCore(previousInvoice);
			}
			SetDefaultIncoTerm(previousInvoice);

			if (newElement.JZ_RX_NKInvoice_Currency.IsEmpty &&
				(bool)CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.GetFallBackValueAtAllLevels(newElement.RegistryCompanyPK, newElement.RegistryBranchPK, Guid.Empty))
			{
				newElement.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			}
		}

		protected virtual void SetDefaultIncoTerm(BaseJobComInvoiceHeader previousInvoice)
		{
			if (previousInvoice == null)
			{
				if (declaration.Shipment != null)
				{
					newElement.JZ_IncoTerm = newElement.IncoTermConverter.GetConvertedIncoTerm(declaration.JE_ShipmentIncoTerm, declaration.IsImport);
				}
			}
			else
			{
				newElement.JZ_IncoTerm = previousInvoice.JZ_IncoTerm;
			}
		}

		protected virtual void SetDefaultsForFirstInvoiceCore()
		{
			if (newElement.JZ_JZ_GroupInvoiceFK.IsEmpty)
			{
				newElement.JZ_JZ_GroupInvoiceFK = declaration.TopGroupInvoice.PK;
			}
		}

		protected virtual void SetDefaultsForAdditionalInvoiceCore(BaseJobComInvoiceHeader previousInvoice)
		{
			if (newElement.JZ_JZ_GroupInvoiceFK.IsEmpty)
			{
				newElement.JZ_JZ_GroupInvoiceFK = previousInvoice.JZ_JZ_GroupInvoiceFK;
			}

			if (previousInvoice.JZ_OA_SupplierAddress.IsValid)
			{
				newElement.JZ_OA_SupplierAddress = previousInvoice.JZ_OA_SupplierAddress;
			}

			if (previousInvoice.JZ_OH_Supplier.IsValid)
			{
				newElement.JZ_OH_Supplier = previousInvoice.JZ_OH_Supplier;
			}

			newElement.JZ_RX_NKInvoice_Currency = previousInvoice.JZ_RX_NKInvoice_Currency;
		}

		protected readonly BaseJobComInvoiceHeader newElement;
		protected readonly BaseJobDeclaration declaration;
	}
}
