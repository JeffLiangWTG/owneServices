using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class TTBExemptionCodeList
	{
		public static ICodeDescriptionPairList GetListForPermit(BusinessObjectFactory factory, ZString programType)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("TTBExemptionCodeListPermit_" + programType, () =>
				{
					CodeDescriptionPairList result = null;
					switch (programType)
					{
						case TTBProgramCodeList.Codes.Beverage:
							result = new CodeDescriptionPairList();
							result.AddPair(Codes.TTBEX1, Descriptions.TTBEX1);
							result.AddPair(Codes.TTBEX2, Descriptions.TTBEX2);
							result.AddPair(Codes.TTBEX14, Descriptions.TTBEX14);
							result.AddPair(Codes.TTBEX15, Descriptions.TTBEX15);
							break;
						case TTBProgramCodeList.Codes.DistilledSpirits:
							result = new CodeDescriptionPairList();
							result.AddPair(Codes.TTBEX1, Descriptions.TTBEX1);
							result.AddPair(Codes.TTBEX5, Descriptions.TTBEX5);
							result.AddPair(Codes.TTBEX14, Descriptions.TTBEX14);
							result.AddPair(Codes.TTBEX15, Descriptions.TTBEX15);
							break;
						case TTBProgramCodeList.Codes.Tobacco:
							result = new CodeDescriptionPairList();
							result.AddPair(Codes.TTBEX1, Descriptions.TTBEX1);
							result.AddPair(Codes.TTBEX6, Descriptions.TTBEX6);
							result.AddPair(Codes.TTBEX13, Descriptions.TTBEX13);
							result.AddPair(Codes.TTBEX15, Descriptions.TTBEX15);
							break;
						case TTBProgramCodeList.Codes.Wine:
							result = new CodeDescriptionPairList();
							result.AddPair(Codes.TTBEX1, Descriptions.TTBEX1);
							result.AddPair(Codes.TTBEX3, Descriptions.TTBEX3);
							result.AddPair(Codes.TTBEX4, Descriptions.TTBEX4);
							result.AddPair(Codes.TTBEX14, Descriptions.TTBEX14);
							result.AddPair(Codes.TTBEX15, Descriptions.TTBEX15);
							break;
						default:
							result = new TTBExemptionCodeList();
							break;
					}
					return result;
				});
		}

		public static ICodeDescriptionPairList GetListForCOLA(BusinessObjectFactory factory, ZString programType)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("TTBExemptionCodeListCOLA_" + programType, () =>
			{
				CodeDescriptionPairList result = null;
				switch (programType)
				{
					case TTBProgramCodeList.Codes.Beverage:
						result = new CodeDescriptionPairList();
						result.AddPair(Codes.TTBEX2, Descriptions.TTBEX2);
						result.AddPair(Codes.TTBEX7, Descriptions.TTBEX7);
						result.AddPair(Codes.TTBEX8, Descriptions.TTBEX8);
						result.AddPair(Codes.TTBEX11, Descriptions.TTBEX11);
						result.AddPair(Codes.TTBEX12, Descriptions.TTBEX12);
						break;
					case TTBProgramCodeList.Codes.DistilledSpirits:
						result = new CodeDescriptionPairList();
						result.AddPair(Codes.TTBEX5, Descriptions.TTBEX5);
						result.AddPair(Codes.TTBEX7, Descriptions.TTBEX7);
						result.AddPair(Codes.TTBEX9, Descriptions.TTBEX9);
						result.AddPair(Codes.TTBEX12, Descriptions.TTBEX12);
						break;
					case TTBProgramCodeList.Codes.Wine:
						result = new CodeDescriptionPairList();
						result.AddPair(Codes.TTBEX3, Descriptions.TTBEX3);
						result.AddPair(Codes.TTBEX4, Descriptions.TTBEX4);
						result.AddPair(Codes.TTBEX7, Descriptions.TTBEX7);
						result.AddPair(Codes.TTBEX10, Descriptions.TTBEX10);
						result.AddPair(Codes.TTBEX12, Descriptions.TTBEX12);
						break;
					default:
						result = new CodeDescriptionPairList();
						break;
				}
				return result;
			});
		}
	}
}
