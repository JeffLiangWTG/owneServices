using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class DefaultSetterForInvoiceHeader : Customs.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(JobComInvoiceHeader child, JobDeclaration declaration)
			: base(child, declaration)
		{
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override void DefaultForNewElementCore()
		{
			base.DefaultForNewElementCore();

			if (declaration.IsImport)
			{
				SetDefaultForImport();
			}
			else if (declaration.IsExport)
			{
				SetDefaultForExport();
			}
			else if (declaration.IsDrawback)
			{
				SetDefaultForDrawback();
			}
			else if (declaration.IsRecon)
			{
				SetDefaultForRecon();
			}
		}

		void SetDefaultForImport()
		{
			var child = (JobComInvoiceHeader)newElement;

			if (child.US_TransactionsRelated.IsEmpty)
			{
				child.US_TransactionsRelated = GetRelatedPartyIndicatorIfPossible(child.SupplierBuyerLink);
			}

			if (child.US_FirstSale.IsEmpty && child.SupplierBuyerLink != null)
			{
				var addInfo = child.SupplierBuyerLink != null ? child.SupplierBuyerLink.GetAddInfo() : null;
				child.US_FirstSale = addInfo != null ? addInfo.ZO_FirstSale : ZString.Empty;
			}
			declaration.MarkReconIndicatorsDirty();
		}

		void SetDefaultForExport()
		{
			var newChild = (JobComInvoiceHeader)newElement;

			newChild.JZ_InvoiceNumber = declaration.JE_DeclarationReference;
			newChild.US_USPPI.ZO_OH_Organisation = ZGuid.Empty;
			newChild.US_ExportUltimateConsignee.ZO_OH_Organisation = declaration.JE_OH_Importer;
			newChild.US_IntermediateConsignee.ZO_OH_Organisation = declaration.JE_OH_Consignee;
			var decPickupAddress = declaration.SupplierPickupAddress;
			var countryCode = decPickupAddress?.Country?.RN_Code ?? ZString.Empty;
			if (!decPickupAddress.IsEmpty && countryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				newChild.SupplierPickupAddress.SynchroniseWithParent(decPickupAddress);
			}

			var previousRow = declaration.Invoices.Count == 0 ? null : declaration.Invoices[declaration.Invoices.Count - 1];
			if (previousRow != null)
			{
				newChild.CopyValueFromForExport(previousRow);
			}
		}

		void SetDefaultForDrawback()
		{
			var newChild = (JobComInvoiceHeader)newElement;
			newChild.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			newChild.JZ_IncoTerm = TermsOfDeliveryList.Codes.FOB;
			newChild.JZ_InvoiceNumber = declaration.JE_DeclarationReference;
		}

		void SetDefaultForRecon()
		{
			var newChild = (JobComInvoiceHeader)newElement;
			newChild.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		}

		ZString GetRelatedPartyIndicatorIfPossible(OrgSupplierBuyerLink link)
		{
			return (link != null && !link.OL_RelatedParty.IsEmpty) ? link.OL_RelatedParty : ZString.Empty;
		}
	}
}
