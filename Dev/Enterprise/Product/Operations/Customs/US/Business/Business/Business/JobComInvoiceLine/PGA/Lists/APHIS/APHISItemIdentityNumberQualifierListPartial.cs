using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class APHISItemIdentityNumberQualifierList
	{
		public static CodeDescriptionPairList GetListWithoutBouquet(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<CodeDescriptionPairList>("APHISItemIdentityNumberQualifierListWithoutBouquet", () =>
				{
					var result = new APHISItemIdentityNumberQualifierList();
					result.RemoveCode(Codes.BQG);
					result.SortByDescription();
					return result;
				});
		}
	}
}
