using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public sealed class RefCusPackListProvider : MasterFiles.Business.RefCusPackListProvider, IRefCusPackListProvider
	{
		public override CodeDescriptionPairList GetCIPCustomsPackList(BusinessObjectFactory factory, ZString country)
		{
			return factory.GetCachedValue<USCusUQList>();
		}

		public override CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory)
		{
			return factory.GetNull<Package>().Lookups.PackTypeList;
		}
	}
}
