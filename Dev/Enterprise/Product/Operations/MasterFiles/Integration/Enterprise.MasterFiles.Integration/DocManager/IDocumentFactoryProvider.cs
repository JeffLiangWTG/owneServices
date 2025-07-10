using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDocumentFactoryProvider
	{
		IDocumentFactory GetFactory(BusinessObjectFactory factoryForEverythingExceptEDocs);
	}
}

#region Test
#if DEBUG

namespace Enterprise.MasterFiles.Integration.Testing
{
	public interface IDocumentFactoryProviderForTest
	{
		IDocumentFactoryForTest GetFactory(BusinessObjectFactory factoryForEverythingExceptEDocs);
	}
}

#endif
#endregion