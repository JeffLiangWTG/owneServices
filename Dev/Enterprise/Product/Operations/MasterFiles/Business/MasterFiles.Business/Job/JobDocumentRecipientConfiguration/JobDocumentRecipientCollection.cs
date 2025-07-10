using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentRecipientCollection : NonPersistentBusinessObjectCollection<JobDocumentRecipientWrapperBase>
	{
		public JobDocumentRecipientCollection(BusinessObjectFactory factory, JobDocumentRecipientConfiguration configuration, bool allowNewAnRemove)
			: base(factory)
		{
			this.configuration = configuration;
			this.allowNewAnRemove = allowNewAnRemove;
		}
		readonly JobDocumentRecipientConfiguration configuration;
		readonly bool allowNewAnRemove;

		protected override bool AllowNewCore => allowNewAnRemove;

		protected override bool AllowRemoveCore => allowNewAnRemove;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var bizo = Factory.New<JobDocumentDelivery>();
			bizo.JDC_ParentID = configuration.DocumentSupportable.DocumentSupporter.BusinessObject.PK;
			bizo.JDC_ParentTableCode = configuration.DocumentSupportable.DocumentSupporter.BusinessObject.TablePrefix;
			bizo.Validation.ValidateAll();
			return new JobDocumentRecipientWrapperForJobDocumentDelivery(bizo, configuration);
		}

		public JobDocumentRecipientWrapperForJobDocumentExclusion AddNewExclusion(OrgDocument orgDocument)
		{
			var bizo = Factory.New<JobDocumentExclusion>();
			bizo.JDE_ParentID = configuration.DocumentSupportable.DocumentSupporter.BusinessObject.PK;
			bizo.JDE_ParentTableCode = configuration.DocumentSupportable.DocumentSupporter.BusinessObject.TablePrefix;
			bizo.JDE_OD_Document = orgDocument.PK;
			var wrapper = new JobDocumentRecipientWrapperForJobDocumentExclusion(bizo, configuration);
			Add(wrapper);
			return wrapper;
		}
	}
}
