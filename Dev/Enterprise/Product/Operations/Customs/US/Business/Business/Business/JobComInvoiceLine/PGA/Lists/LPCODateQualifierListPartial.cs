using CargoWise.EntityFramework;
using CargoWise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class LPCODateQualifierList
	{
		public static ICodeDescriptionPairList GetListForAPHIS(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("APHISLPCODateQualifierList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Codes.ExpirationDate, Descriptions.ExpirationDate);
					result.AddPair(Codes.DateIssuedOrSigned, Descriptions.DateIssuedOrSigned);
					return result;
				});
		}
	}
}
