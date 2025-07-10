using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class OGAIndicatorList
	{
		public static CodeDescriptionPairList GetWithoutDisclaim(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("USOGAIndicatorWithouDisclaimerList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Descriptions.Declared);
				return result;
			});
		}

		public static bool IsToBeDeclaredOrDisclaimed(string indicator)
		{
			return IsToBeDeclared(indicator) || IsToBeDisclaimed(indicator);
		}

		public static bool IsToBeDeclared(string indicator)
		{
			return indicator == OGAIndicatorList.Codes.Declared;
		}

		public static bool IsToBeDisclaimed(string indicator)
		{
			return indicator == OGAIndicatorList.Codes.Disclaimed;
		}

		public static ZString GetIdentifier(string indicator)
		{
			return IsToBeDisclaimed(indicator) ? "Disclaimed" : (IsToBeDeclared(indicator) ? "Declared" : "");
		}
	}
}
