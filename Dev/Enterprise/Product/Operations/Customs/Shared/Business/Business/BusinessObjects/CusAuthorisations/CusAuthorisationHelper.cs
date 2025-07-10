using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public static class CusAuthorisationHelper
	{
		public static CodeDescriptionPairList GetCachedAuthorizationNumbersForAddresses(BusinessObjectFactory factory, ZString countryCode, ZString[] types, ZGuid[] authorizationAddresses, ZDate transactionDate)
		{
			var effectiveAuthorizationAddresses = authorizationAddresses.Where(x => !x.IsEmpty).OrderBy(x => x).ToArray();
			if (effectiveAuthorizationAddresses.Length == 0)
			{
				return new CodeDescriptionPairList();
			}
			else
			{
				return factory.GetCachedValue(string.Join("|", "GetCachedAuthorizationNumbersForAddresses", countryCode, string.Join("_", types.OrderBy(x => x)), string.Join("_", effectiveAuthorizationAddresses), transactionDate), () =>
				   {
					   var result = new CodeDescriptionPairList();
					   result.AddPairsIfNotExist(CusAuthorisationHeader.Loader.GetAuthorisationsForAddresses(factory, countryCode, types, transactionDate, effectiveAuthorizationAddresses));
					   result.Sort();
					   return result;
				   });
			}
		}
	}
}
