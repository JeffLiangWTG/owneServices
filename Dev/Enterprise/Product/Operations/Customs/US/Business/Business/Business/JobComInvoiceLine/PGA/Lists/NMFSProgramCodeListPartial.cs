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
	public partial class NMFSProgramCodeList
	{
		public static ICodeDescriptionPairList GetListFor(BusinessObjectFactory factory, bool is370, bool isAMR, bool isHMS, bool isSIMP, bool isCOA, bool isExport)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>(string.Format("NMFSProgramCodeList{0}_{1}_{2}_{3}_{4}_{5}", is370, isAMR, isHMS, isSIMP, isCOA, isExport), () =>
				{
					var result = new CodeDescriptionPairList();
					if (isExport)
					{
						result.AddPair(Codes.AMR, Descriptions.AMR);
						result.AddPair(Codes.HMS, Descriptions.HMS);
					}
					else
					{
						if (isSIMP)
						{
							result.AddPair(Codes.SIM, Descriptions.SIM);
						}
						if (is370)
						{
							result.AddPair(Codes._370, Descriptions._370);
						}
						if (isAMR)
						{
							result.AddPair(Codes.AMR, Descriptions.AMR);
						}
						if (isHMS)
						{
							result.AddPair(Codes.HMS, Descriptions.HMS);
						}
						if (isCOA)
						{
							result.AddPair(Codes.COA, Descriptions.COA);
						}
					}
					return result;
				});
		}
	}
}
