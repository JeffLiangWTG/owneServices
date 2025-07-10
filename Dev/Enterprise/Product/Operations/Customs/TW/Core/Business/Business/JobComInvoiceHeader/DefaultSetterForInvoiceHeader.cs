using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business
{
	public class DefaultSetterForInvoiceHeader : Customs.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader child, BaseJobDeclaration declaration)
			: base(child, declaration)
		{
		}

		new JobDeclaration declaration => (JobDeclaration)base.declaration;

		new JobComInvoiceHeader newElement => (JobComInvoiceHeader)base.newElement;

		protected override void DefaultForNewElementCore()
		{
			base.DefaultForNewElementCore();
			var invoiceHeader = newElement;
			var decl = declaration;
			invoiceHeader.JZ_NoOfPacks = MathHelper.CalculateDefaultQuantity(new ZDecimal(decl.JE_TotalNoOfPacks), decl.Invoices.TotalNoOfPacks);
			invoiceHeader.JZ_IncoTerm = IncoTerms.GetMappedOfficialIncoterm(decl.JE_ShipmentIncoTerm);
			invoiceHeader.JZ_Description = decl.JE_GoodsDescription;
			DefaultDocumentaryAddressFromSource(decl.SupplierDocumentaryAddress, invoiceHeader.SupplierDocumentaryAddress);
			DefaultDocumentaryAddressFromSource(decl.ImporterDocumentaryAddress, invoiceHeader.BuyerDocumentaryAddress);
		}

		protected override void SetDefaultsForAdditionalInvoiceCore(BaseJobComInvoiceHeader previousInvoice)
		{
			base.SetDefaultsForAdditionalInvoiceCore(previousInvoice);
			newElement.JZ_RelatedIndicator = previousInvoice.JZ_RelatedIndicator;
		}

		void DefaultDocumentaryAddressFromSource(TWJobDocAddress sourceDocAddress, TWJobDocAddress targetDocAddress)
		{
			if (!sourceDocAddress.IsEmpty)
			{
				var copyArgs = new BusinessObjectCloneArgs(new[] { TWJobDocAddress.Schema.E2_AddressType });
				targetDocAddress.CopyPersistentValuesFrom(sourceDocAddress, copyArgs);
				if (sourceDocAddress.E2_AddressOverride)
				{
					if (sourceDocAddress.LocalAddress is TWJobDocAddress sourceLocalAddress)
					{
						targetDocAddress.LocalAddress?.CopyPersistentValuesFrom(sourceLocalAddress, copyArgs);
					}
					targetDocAddress.IDCodeType = sourceDocAddress.IDCodeType;
					targetDocAddress.IDCode = sourceDocAddress.IDCode;
					targetDocAddress.AEOCode = sourceDocAddress.AEOCode;
					targetDocAddress.CBPCodeType = sourceDocAddress.CBPCodeType;
					targetDocAddress.CBPCode = sourceDocAddress.CBPCode;
					targetDocAddress.TPCCode = sourceDocAddress.TPCCode;
				}
			}
		}
	}
}
