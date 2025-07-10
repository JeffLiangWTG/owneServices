using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	partial class RefCusRulingConfigTypes
	{
		public static CodeDescriptionPairList GetUppercaseCodeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("RefCusRulingConfigTypesWithUpperCaseCodeList", () =>
			{
				var result = new CodeDescriptionPairList();
				foreach (ICodeDescription pair in new RefCusRulingConfigTypes())
				{
					result.AddPair(pair.Code.ToUpper(), pair.Description);
				}

				return result;
			});
		}
	}
}
