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
	partial class FWSWildlifeSourceList
	{
		public static ICodeDescriptionPairList GetList(BusinessObjectFactory factory, bool isExport = false)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("FWSWildlifeSourceList" + isExport, () =>
				{
					var result = new CodeDescriptionPairList();
					if (isExport)
					{
						result.AddPair(Codes.W, Descriptions.W);
						result.AddPair(Codes.R, Descriptions.R);
						result.AddPair(Codes.P2, Descriptions.P2);
						result.AddPair(Codes.F, Descriptions.F);
						result.AddPair(Codes.U6, Descriptions.U6);
						result.AddPair(Codes.C, Descriptions.C);
						result.AddPair(Codes.I, Descriptions.I);
						result.AddPair(Codes.D, Descriptions.D);
						result.AddPair(Codes.DOM, Descriptions.DOM);
						result.AddPair(Codes.X, Descriptions.X);
					}
					else
					{
						result.AddPair(Codes.C, Descriptions.C);
						result.AddPair(Codes.D, Descriptions.D);
						result.AddPair(Codes.F, Descriptions.F);
						result.AddPair(Codes.I, Descriptions.I);
						result.AddPair(Codes.P2, Descriptions.P2);
						result.AddPair(Codes.R, Descriptions.R);
						result.AddPair(Codes.W, Descriptions.W);
						result.AddPair(Codes.DOM, Descriptions.DOM);
						result.AddPair(Codes.U6, Descriptions.U6);
						result.AddPair(Codes.X, Descriptions.X);
					}

					return result;
				});
		}
	}
}
