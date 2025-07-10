using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	partial class BillReferenceList
	{
		public static CodeDescriptionPairList GetCachedValue(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AMSBillReferenceListACEM1", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes._2K, Descriptions._2K);
				result.AddPair(Codes.BL, Descriptions.BL);
				result.AddPair(Codes.BM, Descriptions.BM);
				result.AddPair(Codes.BN, Descriptions.BN);
				result.AddPair(Codes.CG, Descriptions.CG);
				result.AddPair(Codes.CN, Descriptions.CN);
				result.AddPair(Codes.CO, Descriptions.CO);
				result.AddPair(Codes.CR, Descriptions.CR);
				result.AddPair(Codes.CSK, Descriptions.CSK);
				result.AddPair(Codes.CUB, Descriptions.CUB);
				result.AddPair(Codes.CX, Descriptions.CX);
				result.AddPair(Codes.ED, Descriptions.ED);
				result.AddPair(Codes.FEN, Descriptions.FEN);
				result.AddPair(Codes.FN, Descriptions.FN);
				result.AddPair(Codes.FP, Descriptions.FP);
				result.AddPair(Codes.GB, Descriptions.GB);
				result.AddPair(Codes.GR, Descriptions.GR);
				result.AddPair(Codes.HS, Descriptions.HS);
				result.AddPair(Codes.IN, Descriptions.IN);
				result.AddPair(Codes.LT, Descriptions.LT);
				result.AddPair(Codes.MA, Descriptions.MA);
				result.AddPair(Codes.MB, Descriptions.MB);
				result.AddPair(Codes.OB, Descriptions.OB);
				result.AddPair(Codes.OL, Descriptions.OL);
				result.AddPair(Codes.OM, Descriptions.OM);
				result.AddPair(Codes.OW, Descriptions.OW);
				result.AddPair(Codes.PK, Descriptions.PK);
				result.AddPair(Codes.PN, Descriptions.PN);
				result.AddPair(Codes.PO, Descriptions.PO);
				result.AddPair(Codes.RC, Descriptions.RC);
				result.AddPair(Codes.S7, Descriptions.S7);
				result.AddPair(Codes.SI, Descriptions.SI);
				result.AddPair(Codes.SO, Descriptions.SO);
				result.AddPair(Codes.ST, Descriptions.ST);
				result.AddPair(Codes.SW, Descriptions.SW);
				result.AddPair(Codes.ULC, Descriptions.ULC);
				result.AddPair(Codes.UT, Descriptions.UT);
				result.AddPair(Codes.VA, Descriptions.VA);
				result.AddPair(Codes.WU, Descriptions.WU);
				result.AddPair(Codes.WY, Descriptions.WY);
				result.AddPair(Codes.XC, Descriptions.XC);
				result.AddPair(Codes.XP, Descriptions.XP);
				result.AddPair(Codes.ZE, Descriptions.ZE);
				result.AddPair(Codes.ZZ, Descriptions.ZZ);
				return result;
			});
		}

		public static CodeDescriptionPairList GetCachedACEM1InBondList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.US.AMS.Messaging.Business.ACEM1InBondList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Codes._2K, Descriptions._2K);
					result.AddPair(Codes.BL, Descriptions.BL);
					result.AddPair(Codes.BM, Descriptions.BM);
					result.AddPair(Codes.BN, Descriptions.BN);
					result.AddPair(Codes.CG, Descriptions.CG);
					result.AddPair(Codes.CN, Descriptions.CN);
					result.AddPair(Codes.CO, Descriptions.CO);
					result.AddPair(Codes.CR, Descriptions.CR);
					result.AddPair(Codes.CUB, Descriptions.CUB);
					result.AddPair(Codes.CX, Descriptions.CX);
					result.AddPair(Codes.ED, Descriptions.ED);
					result.AddPair(Codes.FEN, Descriptions.FEN);
					result.AddPair(Codes.FN, Descriptions.FN);
					result.AddPair(Codes.FP, Descriptions.FP);
					result.AddPair(Codes.GB, Descriptions.GB);
					result.AddPair(Codes.GR, Descriptions.GR);
					result.AddPair(Codes.HS, Descriptions.HS);
					result.AddPair(Codes.IN, Descriptions.IN);
					result.AddPair(Codes.LT, Descriptions.LT);
					result.AddPair(Codes.MA, Descriptions.MA);
					result.AddPair(Codes.MB, Descriptions.MB);
					result.AddPair(Codes.OM, Descriptions.OM);
					result.AddPair(Codes.OW, Descriptions.OW);
					result.AddPair(Codes.PK, Descriptions.PK);
					result.AddPair(Codes.PN, Descriptions.PN);
					result.AddPair(Codes.PO, Descriptions.PO);
					result.AddPair(Codes.SI, Descriptions.SI);
					result.AddPair(Codes.SO, Descriptions.SO);
					result.AddPair(Codes.ST, Descriptions.ST);
					result.AddPair(Codes.SW, Descriptions.SW);
					result.AddPair(Codes.VA, Descriptions.VA);
					result.AddPair(Codes.WU, Descriptions.WU);
					result.AddPair(Codes.WY, Descriptions.WY);
					result.AddPair(Codes.XC, Descriptions.XC);
					result.AddPair(Codes.XP, Descriptions.XP);
					result.AddPair(Codes.ZE, Descriptions.ZE);
					result.AddPair(Codes.ZZ, Descriptions.ZZ);
					return result;
				});
		}
	}
}
