using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	class TestFilteredGridLoaderWithSqlException : FilteredGridLoader
	{
		public TestFilteredGridLoaderWithSqlException(FilterStripBusinessObject filterBusinessObject,
			ResultCountMessage handler,
			IModuleDecisionProvider provider,
			ModuleIdentifier moduleId,
			Func<BusinessObjectFactory> createFactory, Type typeOfElements)
			: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
		{
		}

		public bool ThrowLimitException { get; set; }

		protected override BusinessObject[] LoadCollectionCore(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			if (ThrowLimitException)
			{
				ThrowLimitException = false;
				// use reflection to Construct our desired SQL exception.
				throw SqlExceptionTestHelper.NewSqlException(8649);
			}

			return base.LoadCollectionCore(factory, type, query);
		}
	}
}
