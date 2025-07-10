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
	public partial class APHISProgramCodeList
	{
		public static ICodeDescriptionPairList GetActiveList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("APHISProgramCodeListActive", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Codes.AAC, Descriptions.AAC);
					result.AddPair(Codes.ABS, Descriptions.ABS);
					result.AddPair(Codes.APQ, Descriptions.APQ);
					result.AddPair(Codes.AVS, Descriptions.AVS);
					return result;
				});
		}
	}
}
