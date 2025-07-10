using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class JobRequiredDocumentAllDocumentsReceivedEventLoggerTest<T> : CollectionEventLoggerTest<DummyRequiredDocumentsParent, T> where T : JobRequiredDocumentAllDocumentsReceivedEventLogger
	{
		protected override BusinessObject NewCollectionElement(DummyRequiredDocumentsParent shipment)
		{
			JobRequiredDocument result = shipment.RequiredDocuments.AddNew();
			result.EQ_DocUsage = ImportOrExport;
			return result;
		}

		protected override BusinessObject LoadCollectionElementFromOtherFactory(BusinessObjectFactory factory, BusinessObject businessObject)
		{
			JobRequiredDocument result = (JobRequiredDocument)base.LoadCollectionElementFromOtherFactory(factory, businessObject);
			result.ParentType = Parent.GetType();
			return result;
		}

		protected abstract ZString ImportOrExport { get; }
	}
}
