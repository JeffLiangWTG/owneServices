using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class ExportCustomsManifestHeaderCollectionForTesting : ActiveBusinessObjectCollection<ExportCustomsManifestHeader>
	{
		public ExportCustomsManifestHeaderCollectionForTesting(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
