using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.MasterFiles.Integration.Customs.NZ;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public sealed class RefCusPackListProvider : Enterprise.MasterFiles.Business.RefCusPackListProvider, IRefCusPackListProvider
	{
		public override CodeDescriptionPairList GetDeclarationPackTypeList(BusinessObjectFactory factory)
		{
			return factory.GetNull<Package>().PackTypeList;
		}
	}
}
