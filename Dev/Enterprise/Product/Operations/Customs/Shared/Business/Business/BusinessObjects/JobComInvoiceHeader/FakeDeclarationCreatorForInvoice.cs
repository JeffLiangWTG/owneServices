using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public sealed class FakeDeclarationCreatorForInvoice
	{
		public FakeDeclarationCreatorForInvoice(BaseJobComInvoiceHeader invoice)
		{
			this.invoice = Argument.NotNull(invoice, "Invoice");
			createDeclaration = !invoice.UseNewStandaloneInvoiceData;
			Enable();
		}

		void Enable()
		{
			invoice.FetchStrategy.FetchForLoadChildEditableObjects();
			if (createDeclaration)
			{
				using (invoice.SuspendSettingHasChanges())
				using (invoice.SuspendDataChangeByFakeDeclaration())
				using (Declaration.SuspendDataChangeByFakeDeclaration())
				using (Declaration.SuspendSettingHasChanges())
				using (Declaration.SuspendMarkApportionmentDirty())
				using (Declaration.SuspendWeightApportionment())
				{
					if (invoice.IsInDatabase)
					{
						Declaration.JE_OH_Importer = invoice.JZ_OH_Buyer;
					}

					invoice.JZ_JE = Declaration.PK;
					invoice.JZ_JZ_GroupInvoiceFK = GroupInvoice.PK;
					Declaration.JE_ClusterKey = invoice.JZ_ClusterKey;
					Declaration.JE_MessageType = invoice.JZ_MessageType;
					Declaration.DefaultValueForFakeDeclaration();
					Declaration.FilteredInvoiceLines.HasChangesChanged += FilteredInvoiceLinesOnHasChangesChanged;

					invoice.JZ_ClusterKeyInfo.ValueChanged +=
						(sender, args) => Declaration.JE_ClusterKey = invoice.JZ_ClusterKey;
				}

				Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
				Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			if (!invoice.IsDeleted)
			{
				using (invoice.SuspendSettingHasChanges())
				using (invoice.SuspendDataChangeByFakeDeclaration())
				using (Declaration.SuspendDataChangeByFakeDeclaration())
				using (Declaration.SuspendMarkApportionmentDirty())
				{
					listChangedSuspender = ((ISingleElementListInternal)invoice).SuspendListChanged();

					invoice.JZ_JZ_GroupInvoiceFK = ZGuid.Empty;
					invoice.JZ_JE = ZGuid.Empty;
				}
			}
		}
		IDisposable listChangedSuspender;

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			try
			{
				if (!invoice.IsDeleted && !GroupInvoice.IsDeleted && !Declaration.IsDeleted)
				{
					using (invoice.SuspendSettingHasChanges())
					using (invoice.SuspendDataChangeByFakeDeclaration())
					using (Declaration.SuspendDataChangeByFakeDeclaration())
					using (Declaration.SuspendMarkApportionmentDirty())
					{
						invoice.JZ_JZ_GroupInvoiceFK = GroupInvoice.PK;
						invoice.JZ_JE = Declaration.PK;
					}
				}
			}
			finally
			{
				if (listChangedSuspender != null)
				{
					listChangedSuspender.Dispose();
				}
			}
		}

		void FilteredInvoiceLinesOnHasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			var invoice = this.invoice;

			if (e.ObjectJustWasChanged && invoice != null && !invoice.IsDeleted && !((IBusinessObjectState)invoice).HasChangesNotIncludingChildren)
			{
				invoice.HasChanges = true;
			}
		}

		public ICommonInvoiceDataProvider HeaderData
		{
			get
			{
				if (fHeaderData == null)
				{
					fHeaderData = createDeclaration ? CreateFakeDeclaration() : invoice.StandaloneCommonInvoiceProvider;
				}
				return fHeaderData;
			}
		}
		ICommonInvoiceDataProvider fHeaderData;

		BaseJobDeclaration Declaration => (BaseJobDeclaration)HeaderData;

		BaseJobDeclaration CreateFakeDeclaration()
		{
			var declaration = (BaseJobDeclaration)Factory.New(invoice?.GetDeclarationTypeForFakeDeclarationCreatorForInvoice() ?? typeof(BaseJobDeclaration));
			declaration.MakeNonPersistent();
			declaration.JobComInvoiceGroupHeaders[0].MakeNonPersistent();
			declaration.JobComInvoiceGroupHeaders[0].HasChanges = false;
			declaration.DocsAndCartage.MakeNonPersistent();
			declaration.SupplierDocumentaryAddress.MakeNonPersistent();
			declaration.SupplierPickupAddress.MakeNonPersistent();
			declaration.ImporterDocumentaryAddress.MakeNonPersistent();
			declaration.ImporterDeliveryAddress.MakeNonPersistent();
			declaration.SetShouldOverrideNotes(true);

			foreach (JobDocAddress docAddress in declaration.DocAddresses)
			{
				docAddress.MakeNonPersistent();
				docAddress.HasChanges = false;
			}
			declaration.HasChanges = false;

			return declaration;
		}

		BaseJobComInvoiceGroupHeader GroupInvoice
		{
			get { return Declaration.JobComInvoiceGroupHeaders[0]; }
		}

		BusinessObjectFactory Factory
		{
			get { return invoice.Factory; }
		}

		readonly bool createDeclaration;
		readonly BaseJobComInvoiceHeader invoice;
	}
}
