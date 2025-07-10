using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using IUSForeignPortCollectionProvider = Enterprise.Integration.Customs.US.IUSForeignPortCollectionProvider;

namespace Enterprise.Customs.US.Business
{
	public class USForeignPortCollectionProvider : CollectionProviderWithCodeSupport, IUSForeignPortCollectionProvider
	{
		public USForeignPortCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.ForeignPort;

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new USCForeignPortCollection(BusinessObjectFactory, USCForeignPortWrapper.Type.Common);
		}

		public override int MaxLength => USCForeignPortSchema.UH_Code.MaxLength;
	}
}
