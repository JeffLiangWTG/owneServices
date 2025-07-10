using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceHeaderFilteredActiveCollection : ActiveBusinessObjectCollection<JobComInvoiceHeader>
	{
		public InvoiceHeaderFilteredActiveCollection(JobDeclaration declaration)
			: base(declaration.Factory, declaration, new ZQuery(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false), JobComInvoiceHeaderSchema.JZ_JE)
		{
			this.JobDeclaration = declaration;
		}

		public readonly JobDeclaration JobDeclaration;

		public override IDisposable SuspendAdditionallyForImport()
		{
			return new Customs.Business.InvoiceHeaderImportSuspender(JobDeclaration);
		}

		public bool IsNonCommittedElementExposed(JobComInvoiceHeader header) => base.IsNonCommittedElement(header);

		#region Implementation

		protected override void SetDefaultsForNewElementCore(JobComInvoiceHeader newElement)
		{
			IDisposable suspender = JobDeclaration != null ? JobDeclaration.SuspendMarkApportionmentDirty() : null;

			base.SetDefaultsForNewElementCore(newElement);

			new DefaultSetterForInvoiceHeader(newElement, JobDeclaration).DefaultForNewElement();

			if (suspender != null)
			{
				suspender.Dispose();
			}
		}

		protected override bool MatchesFilterCore(JobComInvoiceHeader element, bool fetchOnlyFromLocalCache)
		{
			bool result = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);

			if (result)
			{
				result = JobDeclaration.ReconDeclaration == null || //for normal jobs
					JobDeclaration.ReconDeclaration.SelectedOriginalEntry.IsEmpty ||
					element.US_CH_ReconEntry == JobDeclaration.ReconDeclaration.SelectedOriginalEntry;
			}
			return result;
		}

		protected override void OnAdded(JobComInvoiceHeader businessObject)
		{
			base.OnAdded(businessObject);
			if (businessObject.IsAttachedToPersistentDeclaration && businessObject.JobDeclaration.IsENSFormalImport)
			{
				MessageAttacheesAddedOrDeletedEvent.InvokeMessageAttacheeAddedOrDeletedService(Factory, businessObject, MessageAttacheeActionType.Added);
			}
		}

		#endregion

	}
}
