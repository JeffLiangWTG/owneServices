using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class InvoiceLinePartClassificationTariffDescriptionSyncroniser
	{
		public InvoiceLinePartClassificationTariffDescriptionSyncroniser(IInvoiceLinePartClassificationTariffDescriptionSyncroniser invoiceLine)
		{
			Argument.NotNull(invoiceLine, "InvoiceLine");
			this.InvoiceLine = invoiceLine;
		}
		protected readonly IInvoiceLinePartClassificationTariffDescriptionSyncroniser InvoiceLine;
		bool ShouldSynchroniseExtendedCommercialDescription
		{
			get { return InvoiceLine.IsExtendedCommercialDescriptionEnabled; }
		}

		public ZBool IsDefaultDescription
		{
			get
			{
				ZString invoiceLineDescription = DefaultInvoiceLineDescription;
				ZString invoiceLineExtraInfoForClassification = ShouldSynchroniseExtendedCommercialDescription ? InvoiceLine.ExtraInfoForClassification : ZString.Empty;
				return ((invoiceLineDescription.IsEmpty && invoiceLineExtraInfoForClassification.IsEmpty)
					|| (invoiceLineDescription == lastPartDescription.ToUpper() && invoiceLineExtraInfoForClassification.EqualsIgnoringCase(lastPartExtendedCommercialDescription))
					|| invoiceLineDescription == InvoiceLine.ClassificationDescription.ToUpper()
					|| invoiceLineDescription == InvoiceLine.TariffDescription.Left(InvoiceLine.DescriptionInfo.MaxLength).Trim().ToUpper());
			}
		}

		ZString lastPartDescription;
		ZString lastPartExtendedCommercialDescription;

		public void RefreshPartGeneratedLineDescription()
		{
			RefreshPartGeneratedLineDescription(InvoiceLine.PartDescription, InvoiceLine.PartExtendedCommercialDescription);
		}

		internal void RefreshPartGeneratedLineDescription(ZString descriptionToSet, ZString extendedDescriptionToSet)
		{
			lastPartDescription = descriptionToSet;
			lastPartExtendedCommercialDescription = ShouldSynchroniseExtendedCommercialDescription ? extendedDescriptionToSet : ZString.Empty;
		}

		public void SetDescription()
		{
			BaseJobDeclaration declaration = InvoiceLine.Declaration;
			OrgSupplierPart part = InvoiceLine.Part;
			BaseCusClassification classification = InvoiceLine.Classification;

			bool alwaysUseClassificationDescription = declaration != null && declaration.AlwaysUseClassificationDescription;
			ZString calculatedDescription = ZString.Empty;
			ZString calculatedExtendedDescription = ZString.Empty;

			if (!alwaysUseClassificationDescription)
			{
				calculatedDescription = InvoiceLine.PartDescription;
				if (!calculatedDescription.IsEmpty)
				{
					calculatedExtendedDescription = InvoiceLine.PartExtendedCommercialDescription;
				}
			}
			if (calculatedDescription.IsEmpty)
			{
				calculatedDescription = InvoiceLine.ClassificationDescription;
			}
			if (calculatedDescription.IsEmpty)
			{
				calculatedDescription = InvoiceLine.TariffDescription;
			}
			SetInvoiceLineDescription(calculatedDescription);
			if (ShouldSynchroniseExtendedCommercialDescription)
			{
				InvoiceLine.ExtraInfoForClassification = calculatedExtendedDescription.Trim().ToUpper();
			}
		}

		protected virtual void SetInvoiceLineDescription(ZString calculatedDescription)
		{
			InvoiceLine.Description = calculatedDescription
				.Trim()
				.ToUpper()
				.StripNonWesternEuropeanCharacters()
				.Left(InvoiceLine.DescriptionInfo.MaxLength);
		}

		protected virtual ZString DefaultInvoiceLineDescription => InvoiceLine.Description;
	}
}
