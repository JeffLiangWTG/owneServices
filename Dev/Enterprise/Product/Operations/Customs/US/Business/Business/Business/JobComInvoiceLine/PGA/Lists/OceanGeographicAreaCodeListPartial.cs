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
	public partial class OceanGeographicAreaCodeList
	{
		public static ICodeDescriptionPairList GetListFor370Program(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("370OceanGeographicAreaCodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Codes.CAR, Descriptions.CAR);
					result.AddPair(Codes.EA, Descriptions.EA);
					result.AddPair(Codes.ETP, Descriptions.ETP);
					result.AddPair(Codes.IND, Descriptions.IND);
					result.AddPair(Codes.NP, Descriptions.NP);
					result.AddPair(Codes.OTH, Descriptions.OTH);
					result.AddPair(Codes.SP, Descriptions.SP);
					result.AddPair(Codes.WA, Descriptions.WA);
					result.AddPair(Codes.WP, Descriptions.WP);
					return result;
				});
		}
	}
}
