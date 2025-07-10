using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class ACE_FDABaseUQList
	{
		public static CodeDescriptionPairList GetFDABaseUQsCodeList(ZString programCode, ZBool isPNote)
		{
			var list = new CodeDescriptionPairList();
			switch (programCode)
			{
				case FDAProgramCodeList.Codes.BIO:
					return GetBIOBaseUQCodeList();
				case FDAProgramCodeList.Codes.COS:
					return GetCOSAndVMEBaseUQCodeList();
				case FDAProgramCodeList.Codes.DRU:
					return GetDRUBaseUQCodeList();
				case FDAProgramCodeList.Codes.FOO:
					if (isPNote)
					{
						return GetFooPNBaseUQCodeList();
					}
					else
					{
						return GetFooBaseUQCodeList();
					}
				case FDAProgramCodeList.Codes.DEV:
				case FDAProgramCodeList.Codes.RAD:
					list.AddPair(Codes.PCS, Descriptions.PCS);
					return list;
				case FDAProgramCodeList.Codes.TOB:
					list.AddPair(FDABaseUQList.Codes.BBL, FDABaseUQList.Descriptions.BBL);
					list.AddPair(FDABaseUQList.Codes.DOZ, FDABaseUQList.Descriptions.DOZ);
					list.AddPair(FDABaseUQList.Codes.DPC, FDABaseUQList.Descriptions.DPC);
					list.AddPair(FDABaseUQList.Codes.FOZ, FDABaseUQList.Descriptions.FOZ);
					list.AddPair(FDABaseUQList.Codes.GAL, FDABaseUQList.Descriptions.GAL);
					list.AddPair(FDABaseUQList.Codes.L, FDABaseUQList.Descriptions.L);
					list.AddPair(FDABaseUQList.Codes.ML, FDABaseUQList.Descriptions.ML);
					list.AddPair(FDABaseUQList.Codes.NO, FDABaseUQList.Descriptions.NO);
					list.AddPair(FDABaseUQList.Codes.PCS, FDABaseUQList.Descriptions.PCS);
					list.AddPair(FDABaseUQList.Codes.PTL, FDABaseUQList.Descriptions.PTL);
					list.AddPair(FDABaseUQList.Codes.QTL, FDABaseUQList.Descriptions.QTL);
					list.AddPair(FDABaseUQList.Codes.G, FDABaseUQList.Descriptions.G);
					list.AddPair(FDABaseUQList.Codes.KG, FDABaseUQList.Descriptions.KG);
					list.AddPair(FDABaseUQList.Codes.LB, FDABaseUQList.Descriptions.LB);
					return list;
				case FDAProgramCodeList.Codes.VME:
					return GetCOSAndVMEBaseUQCodeList();
				default:
					{
						list = new ACE_FDABaseUQList();
						return list;
					}
			}
		}

		public static CodeDescriptionPairList GetFDAUQsCodeList(ZString programCode, ZBool isPNote)
		{
			var list = new CodeDescriptionPairList();
			switch (programCode)
			{
				case FDAProgramCodeList.Codes.BIO:
					return GetBIOUQCodeListNew();
				case FDAProgramCodeList.Codes.COS:
					return GetCOSOrVMEOrDRUUQCodeList();
				case FDAProgramCodeList.Codes.DRU:
					return GetCOSOrVMEOrDRUUQCodeList();
				case FDAProgramCodeList.Codes.FOO:
					if (isPNote)
					{
						return GetFooPNUQCodeList();
					}
					else
					{
						return GetFooUQCodeList();
					}
				case FDAProgramCodeList.Codes.TOB:
					list.AddPair(Codes.AT, Descriptions.AT);
					list.AddPair(Codes.BL, Descriptions.BL);
					list.AddPair(Codes.BN, Descriptions.BN);
					list.AddPair(Codes.BX, Descriptions.BX);
					list.AddPair(Codes.CON, Descriptions.CON);
					list.AddPair(Codes.CTR, Descriptions.CTR);
					list.AddPair(Codes.DR, Descriptions.DR);
					list.AddPair(Codes.VI, Descriptions.VI);
					list.AddPair(Codes.VL, Descriptions.VL);
					list.AddPair(Codes.KIT, Descriptions.KIT);
					list.AddPair(Codes.CS, Descriptions.CS);
					list.AddPair(Codes.CT, Descriptions.CT);
					list.AddPair(Codes.PK, Descriptions.PK);
					return list;
				case FDAProgramCodeList.Codes.DEV:
				case FDAProgramCodeList.Codes.RAD:
					list.AddPair(Codes.CS, Descriptions.CS);
					list.AddPair(Codes.CT, Descriptions.CT);
					list.AddPair(Codes.BX, Descriptions.BX);
					list.AddPair(Codes.PK, Descriptions.PK);
					return list;
				case FDAProgramCodeList.Codes.VME:
					return GetCOSOrVMEOrDRUUQCodeList();
				default:
					var result = new FDAUQList();
					result.RemoveCode(FDAUQList.Codes.CB);
					result.RemoveCode(FDAUQList.Codes.RD);
					return result;
			}
		}

		static CodeDescriptionPairList GetBIOUQCodeListNew()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Codes.AE, Descriptions.AE);
			list.AddPair(Codes.AM, Descriptions.AM);
			list.AddPair(Codes.AP, Descriptions.AP);
			list.AddPair(Codes.AT, Descriptions.AT);
			list.AddPair(Codes.BA, Descriptions.BA);
			list.AddPair(Codes.BC, Descriptions.BC);
			list.AddPair(Codes.BG, Descriptions.BG);
			list.AddPair(Codes.BO, Descriptions.BO);
			list.AddPair(Codes.BQ, Descriptions.BQ);
			list.AddPair(Codes.BS, Descriptions.BS);
			list.AddPair(Codes.BV, Descriptions.BV);
			list.AddPair(Codes.BX, Descriptions.BX);
			list.AddPair(Codes.CA, Descriptions.CA);
			list.AddPair(Codes.CI, Descriptions.CI);
			list.AddPair(Codes.CON, Descriptions.CON);
			list.AddPair(Codes.CS, Descriptions.CS);
			list.AddPair(Codes.CT, Descriptions.CT);
			list.AddPair(Codes.CX, Descriptions.CX);
			list.AddPair(Codes.CY, Descriptions.CY);
			list.AddPair(Codes.DR, Descriptions.DR);
			list.AddPair(Codes.EN, Descriptions.EN);
			list.AddPair(Codes.FD, Descriptions.FD);
			list.AddPair(Codes.GB, Descriptions.GB);
			list.AddPair(Codes.MB, Descriptions.MB);
			list.AddPair(Codes.PAL, Descriptions.PAL);
			list.AddPair(Codes.PC, Descriptions.PC);
			list.AddPair(Codes.PK, Descriptions.PK);
			list.AddPair(Codes.SY, Descriptions.SY);
			list.AddPair(Codes.VI, Descriptions.VI);
			list.AddPair(Codes.TU, Descriptions.TU);
			list.AddPair(Codes.VP, Descriptions.VP);
			list.AddPair(Codes.VL, Descriptions.VL);
			return list;
		}

		static CodeDescriptionPairList GetBIOBaseUQCodeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Codes.AU, Descriptions.AU);
			list.AddPair(Codes.BAU, Descriptions.BAU);
			list.AddPair(Codes.CAP, Descriptions.CAP);
			list.AddPair(Codes.CG, Descriptions.CG);
			list.AddPair(Codes.FOZ, Descriptions.FOZ);
			list.AddPair(Codes.G, Descriptions.G);
			list.AddPair(Codes.GAL, Descriptions.GAL);
			list.AddPair(Codes.KG, Descriptions.KG);
			list.AddPair(Codes.L, Descriptions.L);
			list.AddPair(Codes.LB, Descriptions.LB);
			list.AddPair(Codes.MG, Descriptions.MG);
			list.AddPair(Codes.ML, Descriptions.ML);
			list.AddPair(Codes.MCG, Descriptions.MCG);
			list.AddPair(FDABaseUQList.Codes.NO, FDABaseUQList.Descriptions.NO);
			list.AddPair(Codes.OZ, Descriptions.OZ);
			list.AddPair(Codes.PCS, Descriptions.PCS);
			list.AddPair(Codes.PNU, Descriptions.PNU);
			list.AddPair(Codes.PTL, Descriptions.PTL);
			list.AddPair(Codes.QTL, Descriptions.QTL);
			list.AddPair(Codes.TAB, Descriptions.TAB);
			return list;
		}

		static CodeDescriptionPairList GetCOSAndVMEBaseUQCodeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Codes.AU, Descriptions.AU);
			list.AddPair(Codes.BAU, Descriptions.BAU);
			list.AddPair(FDABaseUQList.Codes.BBL, FDABaseUQList.Descriptions.BBL);
			list.AddPair(FDABaseUQList.Codes.BOL, FDABaseUQList.Descriptions.BOL);
			list.AddPair(FDABaseUQList.Codes.CAP, FDABaseUQList.Descriptions.CAP);
			list.AddPair(FDABaseUQList.Codes.CAR, FDABaseUQList.Descriptions.CAR);
			list.AddPair(FDABaseUQList.Codes.CFT, FDABaseUQList.Descriptions.CFT);
			list.AddPair(FDABaseUQList.Codes.CG, FDABaseUQList.Descriptions.CG);
			list.AddPair(FDABaseUQList.Codes.CM, FDABaseUQList.Descriptions.CM);
			list.AddPair(FDABaseUQList.Codes.CM3, FDABaseUQList.Descriptions.CM3);
			list.AddPair(FDABaseUQList.Codes.CYD, FDABaseUQList.Descriptions.CYD);
			list.AddPair(FDABaseUQList.Codes.DOZ, FDABaseUQList.Descriptions.DOZ);
			list.AddPair(FDABaseUQList.Codes.DPC, FDABaseUQList.Descriptions.DPC);
			list.AddPair(FDABaseUQList.Codes.DPR, FDABaseUQList.Descriptions.DPR);
			list.AddPair(FDABaseUQList.Codes.FOZ, FDABaseUQList.Descriptions.FOZ);
			list.AddPair(FDABaseUQList.Codes.FT, FDABaseUQList.Descriptions.FT);
			list.AddPair(Codes.G, Descriptions.G);
			list.AddPair(Codes.GAL, Descriptions.GAL);
			list.AddPair(FDABaseUQList.Codes.GR, FDABaseUQList.Descriptions.GR);
			list.AddPair(FDABaseUQList.Codes.KG, FDABaseUQList.Descriptions.KG);
			list.AddPair(FDABaseUQList.Codes.KM, FDABaseUQList.Descriptions.KM);
			list.AddPair(FDABaseUQList.Codes.KM2, FDABaseUQList.Descriptions.KM2);
			list.AddPair(FDABaseUQList.Codes.KM3, FDABaseUQList.Descriptions.KM3);
			list.AddPair(Codes.L, Descriptions.L);
			list.AddPair(Codes.LB, Descriptions.LB);
			list.AddPair(FDABaseUQList.Codes.LNM, FDABaseUQList.Descriptions.LNM);
			list.AddPair(FDABaseUQList.Codes.M, FDABaseUQList.Descriptions.M);
			list.AddPair(FDABaseUQList.Codes.M2, FDABaseUQList.Descriptions.M2);
			list.AddPair(FDABaseUQList.Codes.M3, FDABaseUQList.Descriptions.M3);
			list.AddPair(Codes.MG, Descriptions.MG);
			list.AddPair(Codes.MCG, Descriptions.MCG);
			list.AddPair(Codes.ML, Descriptions.ML);
			list.AddPair(FDABaseUQList.Codes.NO, FDABaseUQList.Descriptions.NO);
			list.AddPair(FDABaseUQList.Codes.OZ, FDABaseUQList.Descriptions.OZ);
			list.AddPair(Codes.PCS, Descriptions.PCS);
			list.AddPair(Codes.PNU, Descriptions.PNU);
			list.AddPair(FDABaseUQList.Codes.PRS, FDABaseUQList.Descriptions.PRS);
			list.AddPair(FDABaseUQList.Codes.PTL, FDABaseUQList.Descriptions.PTL);
			list.AddPair(FDABaseUQList.Codes.QTL, FDABaseUQList.Descriptions.QTL);
			list.AddPair(FDABaseUQList.Codes.SFT, FDABaseUQList.Descriptions.SFT);
			list.AddPair(FDABaseUQList.Codes.SQI, FDABaseUQList.Descriptions.SQI);
			list.AddPair(FDABaseUQList.Codes.STN, FDABaseUQList.Descriptions.STN);
			list.AddPair(FDABaseUQList.Codes.SUP, FDABaseUQList.Descriptions.SUP);
			list.AddPair(FDABaseUQList.Codes.SYD, FDABaseUQList.Descriptions.SYD);
			list.AddPair(FDABaseUQList.Codes.T, FDABaseUQList.Descriptions.T);
			list.AddPair(FDABaseUQList.Codes.TAB, FDABaseUQList.Descriptions.TAB);
			list.AddPair(FDABaseUQList.Codes.TON, FDABaseUQList.Descriptions.TON);
			list.AddPair(FDABaseUQList.Codes.TOZ, FDABaseUQList.Descriptions.TOZ);
			list.AddPair(Codes.YD, Descriptions.YD);
			return list;
		}

		static CodeDescriptionPairList GetDRUBaseUQCodeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(FDABaseUQList.Codes.BBL, FDABaseUQList.Descriptions.BBL);
			list.AddPair(FDABaseUQList.Codes.BOL, FDABaseUQList.Descriptions.BOL);
			list.AddPair(FDABaseUQList.Codes.CAP, FDABaseUQList.Descriptions.CAP);
			list.AddPair(FDABaseUQList.Codes.CFT, FDABaseUQList.Descriptions.CFT);
			list.AddPair(Codes.CTR, Descriptions.CTR);
			list.AddPair(Codes.CG, Descriptions.CG);
			list.AddPair(FDABaseUQList.Codes.CM, FDABaseUQList.Descriptions.CM);
			list.AddPair(FDABaseUQList.Codes.CM3, FDABaseUQList.Descriptions.CM3);
			list.AddPair(FDABaseUQList.Codes.CYD, FDABaseUQList.Descriptions.CYD);
			list.AddPair(FDABaseUQList.Codes.FOZ, FDABaseUQList.Descriptions.FOZ);
			list.AddPair(FDABaseUQList.Codes.FT, FDABaseUQList.Descriptions.FT);
			list.AddPair(FDABaseUQList.Codes.G, FDABaseUQList.Descriptions.G);
			list.AddPair(FDABaseUQList.Codes.GAL, FDABaseUQList.Descriptions.GAL);
			list.AddPair(FDABaseUQList.Codes.KG, FDABaseUQList.Descriptions.KG);
			list.AddPair(FDABaseUQList.Codes.KM, FDABaseUQList.Descriptions.KM);
			list.AddPair(FDABaseUQList.Codes.KM2, FDABaseUQList.Descriptions.KM2);
			list.AddPair(FDABaseUQList.Codes.KM3, FDABaseUQList.Descriptions.KM3);
			list.AddPair(FDABaseUQList.Codes.L, FDABaseUQList.Descriptions.L);
			list.AddPair(FDABaseUQList.Codes.LB, FDABaseUQList.Descriptions.LB);
			list.AddPair(FDABaseUQList.Codes.LNM, FDABaseUQList.Descriptions.LNM);
			list.AddPair(FDABaseUQList.Codes.M, FDABaseUQList.Descriptions.M);
			list.AddPair(FDABaseUQList.Codes.M2, FDABaseUQList.Descriptions.M2);
			list.AddPair(FDABaseUQList.Codes.M3, FDABaseUQList.Descriptions.M3);
			list.AddPair(FDABaseUQList.Codes.MG, FDABaseUQList.Descriptions.MG);
			list.AddPair(Codes.MCG, Descriptions.MCG);
			list.AddPair(FDABaseUQList.Codes.ML, FDABaseUQList.Descriptions.ML);
			list.AddPair(FDABaseUQList.Codes.OZ, FDABaseUQList.Descriptions.OZ);
			list.AddPair(FDABaseUQList.Codes.PCS, FDABaseUQList.Descriptions.PCS);
			list.AddPair(FDABaseUQList.Codes.PTL, FDABaseUQList.Descriptions.PTL);
			list.AddPair(FDABaseUQList.Codes.QTL, FDABaseUQList.Descriptions.QTL);
			list.AddPair(FDABaseUQList.Codes.STN, FDABaseUQList.Descriptions.STN);
			list.AddPair(FDABaseUQList.Codes.SUP, FDABaseUQList.Descriptions.SUP);
			list.AddPair(FDABaseUQList.Codes.T, FDABaseUQList.Descriptions.T);
			list.AddPair(FDABaseUQList.Codes.TAB, FDABaseUQList.Descriptions.TAB);
			list.AddPair(FDABaseUQList.Codes.TON, FDABaseUQList.Descriptions.TON);
			list.AddPair(FDABaseUQList.Codes.TOZ, FDABaseUQList.Descriptions.TOZ);
			return list;
		}

		#region FOO
		static CodeDescriptionPairList GetFooPNUQCodeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(FDAUQList.Codes.AE, FDAUQList.Descriptions.AE);
			list.AddPair(FDAUQList.Codes.AM, FDAUQList.Descriptions.AM);
			list.AddPair(FDAUQList.Codes.AP, FDAUQList.Descriptions.AP);
			list.AddPair(FDAUQList.Codes.AT, FDAUQList.Descriptions.AT);
			list.AddPair(FDAUQList.Codes.BA, FDAUQList.Descriptions.BA);
			list.AddPair(FDAUQList.Codes.BB, FDAUQList.Descriptions.BB);
			list.AddPair(FDAUQList.Codes.BC, FDAUQList.Descriptions.BC);
			list.AddPair(FDAUQList.Codes.BE, FDAUQList.Descriptions.BE);
			list.AddPair(FDAUQList.Codes.BF, FDAUQList.Descriptions.BF);
			list.AddPair(FDAUQList.Codes.BG, FDAUQList.Descriptions.BG);
			list.AddPair(FDAUQList.Codes.BH, FDAUQList.Descriptions.BH);
			list.AddPair(FDAUQList.Codes.BI, FDAUQList.Descriptions.BI);
			list.AddPair(FDAUQList.Codes.BJ, FDAUQList.Descriptions.BJ);
			list.AddPair(FDAUQList.Codes.BK, FDAUQList.Descriptions.BK);
			list.AddPair(FDAUQList.Codes.BL, FDAUQList.Descriptions.BL);
			list.AddPair(FDAUQList.Codes.BN, FDAUQList.Descriptions.BN);
			list.AddPair(FDAUQList.Codes.BO, FDAUQList.Descriptions.BO);
			list.AddPair(FDAUQList.Codes.BP, FDAUQList.Descriptions.BP);
			list.AddPair(FDAUQList.Codes.BQ, FDAUQList.Descriptions.BQ);
			list.AddPair(FDAUQList.Codes.BR, FDAUQList.Descriptions.BR);
			list.AddPair(FDAUQList.Codes.BS, FDAUQList.Descriptions.BS);
			list.AddPair(FDAUQList.Codes.BU, FDAUQList.Descriptions.BU);
			list.AddPair(FDAUQList.Codes.BV, FDAUQList.Descriptions.BV);
			list.AddPair(FDAUQList.Codes.BX, FDAUQList.Descriptions.BX);
			list.AddPair(FDAUQList.Codes.BZ, FDAUQList.Descriptions.BZ);
			list.AddPair(FDAUQList.Codes.CA, FDAUQList.Descriptions.CA);
			list.AddPair(FDAUQList.Codes.CAG, FDAUQList.Descriptions.CAG);
			list.AddPair(FDAUQList.Codes.CB, FDAUQList.Descriptions.CB);
			list.AddPair(FDAUQList.Codes.CC, FDAUQList.Descriptions.CC);
			list.AddPair(FDAUQList.Codes.CE, FDAUQList.Descriptions.CE);
			list.AddPair(FDAUQList.Codes.CF, FDAUQList.Descriptions.CF);
			list.AddPair(FDAUQList.Codes.CH, FDAUQList.Descriptions.CH);
			list.AddPair(FDAUQList.Codes.CI, FDAUQList.Descriptions.CI);
			list.AddPair(FDAUQList.Codes.CJ, FDAUQList.Descriptions.CJ);
			list.AddPair(FDAUQList.Codes.CK, FDAUQList.Descriptions.CK);
			list.AddPair(FDAUQList.Codes.CL, FDAUQList.Descriptions.CL);
			list.AddPair(FDAUQList.Codes.CO, FDAUQList.Descriptions.CO);
			list.AddPair(FDAUQList.Codes.CON, FDAUQList.Descriptions.CON);
			list.AddPair(FDAUQList.Codes.CP, FDAUQList.Descriptions.CP);
			list.AddPair(FDAUQList.Codes.CR, FDAUQList.Descriptions.CR);
			list.AddPair(FDAUQList.Codes.CS, FDAUQList.Descriptions.CS);
			list.AddPair(FDAUQList.Codes.CT, FDAUQList.Descriptions.CT);
			list.AddPair(FDAUQList.Codes.CU, FDAUQList.Descriptions.CU);
			list.AddPair(FDAUQList.Codes.CV, FDAUQList.Descriptions.CV);
			list.AddPair(FDAUQList.Codes.CX, FDAUQList.Descriptions.CX);
			list.AddPair(FDAUQList.Codes.CY, FDAUQList.Descriptions.CY);
			list.AddPair(FDAUQList.Codes.CZ, FDAUQList.Descriptions.CZ);
			list.AddPair(FDAUQList.Codes.DJ, FDAUQList.Descriptions.DJ);
			list.AddPair(FDAUQList.Codes.DP, FDAUQList.Descriptions.DP);
			list.AddPair(FDAUQList.Codes.DR, FDAUQList.Descriptions.DR);
			list.AddPair(FDAUQList.Codes.EN, FDAUQList.Descriptions.EN);
			list.AddPair(FDAUQList.Codes.FC, FDAUQList.Descriptions.FC);
			list.AddPair(FDAUQList.Codes.FD, FDAUQList.Descriptions.FD);
			list.AddPair(FDAUQList.Codes.FI, FDAUQList.Descriptions.FI);
			list.AddPair(FDAUQList.Codes.FL, FDAUQList.Descriptions.FL);
			list.AddPair(FDAUQList.Codes.FO, FDAUQList.Descriptions.FO);
			list.AddPair(FDAUQList.Codes.FR, FDAUQList.Descriptions.FR);
			list.AddPair(FDAUQList.Codes.GB, FDAUQList.Descriptions.GB);
			list.AddPair(FDAUQList.Codes.HG, FDAUQList.Descriptions.HG);
			list.AddPair(FDAUQList.Codes.HR, FDAUQList.Descriptions.HR);
			list.AddPair(FDAUQList.Codes.JC, FDAUQList.Descriptions.JC);
			list.AddPair(FDAUQList.Codes.JG, FDAUQList.Descriptions.JG);
			list.AddPair(FDAUQList.Codes.JR, FDAUQList.Descriptions.JR);
			list.AddPair(FDAUQList.Codes.JT, FDAUQList.Descriptions.JT);
			list.AddPair(FDAUQList.Codes.JY, FDAUQList.Descriptions.JY);
			list.AddPair(FDAUQList.Codes.KEG, FDAUQList.Descriptions.KEG);
			list.AddPair(FDAUQList.Codes.KIT, FDAUQList.Descriptions.KIT);
			list.AddPair(FDAUQList.Codes.MB, FDAUQList.Descriptions.MB);
			list.AddPair(FDAUQList.Codes.MC, FDAUQList.Descriptions.MC);
			list.AddPair(FDAUQList.Codes.MS, FDAUQList.Descriptions.MS);
			list.AddPair(FDAUQList.Codes.MT, FDAUQList.Descriptions.MT);
			list.AddPair(FDAUQList.Codes.NE, FDAUQList.Descriptions.NE);
			list.AddPair(FDAUQList.Codes.NS, FDAUQList.Descriptions.NS);
			list.AddPair(FDAUQList.Codes.NT, FDAUQList.Descriptions.NT);
			list.AddPair(FDAUQList.Codes.PA, FDAUQList.Descriptions.PA);
			list.AddPair(FDAUQList.Codes.PAL, FDAUQList.Descriptions.PAL);
			list.AddPair(FDAUQList.Codes.PC, FDAUQList.Descriptions.PC);
			list.AddPair(FDAUQList.Codes.PH, FDAUQList.Descriptions.PH);
			list.AddPair(FDAUQList.Codes.PK, FDAUQList.Descriptions.PK);
			list.AddPair(FDAUQList.Codes.PL, FDAUQList.Descriptions.PL);
			list.AddPair(FDAUQList.Codes.PO, FDAUQList.Descriptions.PO);
			list.AddPair(FDAUQList.Codes.PT, FDAUQList.Descriptions.PT);
			list.AddPair(FDAUQList.Codes.PU, FDAUQList.Descriptions.PU);
			list.AddPair(FDAUQList.Codes.PY, FDAUQList.Descriptions.PY);
			list.AddPair(FDAUQList.Codes.RG, FDAUQList.Descriptions.RG);
			list.AddPair(FDAUQList.Codes.RO, FDAUQList.Descriptions.RO);
			list.AddPair(FDAUQList.Codes.SA, FDAUQList.Descriptions.SA);
			list.AddPair(FDAUQList.Codes.SC, FDAUQList.Descriptions.SC);
			list.AddPair(FDAUQList.Codes.SD, FDAUQList.Descriptions.SD);
			list.AddPair(FDAUQList.Codes.SE, FDAUQList.Descriptions.SE);
			list.AddPair(FDAUQList.Codes.SH, FDAUQList.Descriptions.SH);
			list.AddPair(FDAUQList.Codes.SK, FDAUQList.Descriptions.SK);
			list.AddPair(FDAUQList.Codes.SL, FDAUQList.Descriptions.SL);
			list.AddPair(FDAUQList.Codes.SU, FDAUQList.Descriptions.SU);
			list.AddPair(FDAUQList.Codes.SW, FDAUQList.Descriptions.SW);
			list.AddPair(FDAUQList.Codes.SZ, FDAUQList.Descriptions.SZ);
			list.AddPair(FDAUQList.Codes.TB, FDAUQList.Descriptions.TB);
			list.AddPair(FDAUQList.Codes.TC, FDAUQList.Descriptions.TC);
			list.AddPair(FDAUQList.Codes.TD, FDAUQList.Descriptions.TD);
			list.AddPair(FDAUQList.Codes.TK, FDAUQList.Descriptions.TK);
			list.AddPair(FDAUQList.Codes.TN, FDAUQList.Descriptions.TN);
			list.AddPair(FDAUQList.Codes.TO, FDAUQList.Descriptions.TO);
			list.AddPair(FDAUQList.Codes.TR, FDAUQList.Descriptions.TR);
			list.AddPair(FDAUQList.Codes.TS, FDAUQList.Descriptions.TS);
			list.AddPair(FDAUQList.Codes.TU, FDAUQList.Descriptions.TU);
			list.AddPair(FDAUQList.Codes.TY, FDAUQList.Descriptions.TY);
			list.AddPair(FDAUQList.Codes.TZ, FDAUQList.Descriptions.TZ);
			list.AddPair(FDAUQList.Codes.VA, FDAUQList.Descriptions.VA);
			list.AddPair(FDAUQList.Codes.VG, FDAUQList.Descriptions.VG);
			list.AddPair(FDAUQList.Codes.VI, FDAUQList.Descriptions.VI);
			list.AddPair(FDAUQList.Codes.VL, FDAUQList.Descriptions.VL);
			list.AddPair(FDAUQList.Codes.VO, FDAUQList.Descriptions.VO);
			list.AddPair(FDAUQList.Codes.VP, FDAUQList.Descriptions.VP);
			list.AddPair(FDAUQList.Codes.VQ, FDAUQList.Descriptions.VQ);
			list.AddPair(FDAUQList.Codes.VR, FDAUQList.Descriptions.VR);
			list.AddPair(FDAUQList.Codes.VY, FDAUQList.Descriptions.VY);
			list.AddPair(FDAUQList.Codes.WB, FDAUQList.Descriptions.WB);
			return list;
		}

		static CodeDescriptionPairList GetFooPNBaseUQCodeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(FDABaseUQList.Codes.BBL, FDABaseUQList.Descriptions.BBL);
			list.AddPair(FDABaseUQList.Codes.BOL, FDABaseUQList.Descriptions.BOL);
			list.AddPair(FDABaseUQList.Codes.CAR, FDABaseUQList.Descriptions.CAR);
			list.AddPair(FDABaseUQList.Codes.CAP, FDABaseUQList.Descriptions.CAP);
			list.AddPair(FDABaseUQList.Codes.CFT, FDABaseUQList.Descriptions.CFT);
			list.AddPair(FDABaseUQList.Codes.CG, FDABaseUQList.Descriptions.CG);
			list.AddPair(FDABaseUQList.Codes.CM3, FDABaseUQList.Descriptions.CM3);
			list.AddPair(FDABaseUQList.Codes.CYD, FDABaseUQList.Descriptions.CYD);
			list.AddPair(FDABaseUQList.Codes.DOZ, FDABaseUQList.Descriptions.DOZ);
			list.AddPair(FDABaseUQList.Codes.DPC, FDABaseUQList.Descriptions.DPC);
			list.AddPair(FDABaseUQList.Codes.DPR, FDABaseUQList.Descriptions.DPR);
			list.AddPair(FDABaseUQList.Codes.FOZ, FDABaseUQList.Descriptions.FOZ);
			list.AddPair(FDABaseUQList.Codes.G, FDABaseUQList.Descriptions.G);
			list.AddPair(FDABaseUQList.Codes.GAL, FDABaseUQList.Descriptions.GAL);
			list.AddPair(FDABaseUQList.Codes.GR, FDABaseUQList.Descriptions.GR);
			list.AddPair(FDABaseUQList.Codes.KG, FDABaseUQList.Descriptions.KG);
			list.AddPair(FDABaseUQList.Codes.KM3, FDABaseUQList.Descriptions.KM3);
			list.AddPair(FDABaseUQList.Codes.L, FDABaseUQList.Descriptions.L);
			list.AddPair(FDABaseUQList.Codes.LB, FDABaseUQList.Descriptions.LB);
			list.AddPair(FDABaseUQList.Codes.M3, FDABaseUQList.Descriptions.M3);
			list.AddPair(Codes.MCG, Descriptions.MCG);
			list.AddPair(FDABaseUQList.Codes.MG, FDABaseUQList.Descriptions.MG);
			list.AddPair(FDABaseUQList.Codes.ML, FDABaseUQList.Descriptions.ML);
			list.AddPair(FDABaseUQList.Codes.NO, FDABaseUQList.Descriptions.NO);
			list.AddPair(FDABaseUQList.Codes.OZ, FDABaseUQList.Descriptions.OZ);
			list.AddPair(FDABaseUQList.Codes.PCS, FDABaseUQList.Descriptions.PCS);
			list.AddPair(FDABaseUQList.Codes.PRS, FDABaseUQList.Descriptions.PRS);
			list.AddPair(FDABaseUQList.Codes.PTL, FDABaseUQList.Descriptions.PTL);
			list.AddPair(FDABaseUQList.Codes.QTL, FDABaseUQList.Descriptions.QTL);
			list.AddPair(FDABaseUQList.Codes.STN, FDABaseUQList.Descriptions.STN);
			list.AddPair(FDABaseUQList.Codes.T, FDABaseUQList.Descriptions.T);
			list.AddPair(FDABaseUQList.Codes.TAB, FDABaseUQList.Descriptions.TAB);
			list.AddPair(FDABaseUQList.Codes.TON, FDABaseUQList.Descriptions.TON);
			list.AddPair(FDABaseUQList.Codes.TOZ, FDABaseUQList.Descriptions.TOZ);
			return list;
		}

		static CodeDescriptionPairList GetFooUQCodeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Codes.AE, Descriptions.AE);
			list.AddPair(Codes.AM, Descriptions.AM);
			list.AddPair(Codes.AP, Descriptions.AP);
			list.AddPair(Codes.AT, Descriptions.AT);
			list.AddPair(Codes.BA, Descriptions.BA);
			list.AddPair(Codes.BB, Descriptions.BB);
			list.AddPair(Codes.BC, Descriptions.BC);
			list.AddPair(Codes.BE, Descriptions.BE);
			list.AddPair(Codes.BF, Descriptions.BF);
			list.AddPair(Codes.BG, Descriptions.BG);
			list.AddPair(Codes.BH, Descriptions.BH);
			list.AddPair(Codes.BI, Descriptions.BI);
			list.AddPair(Codes.BJ, Descriptions.BJ);
			list.AddPair(Codes.BK, Descriptions.BK);
			list.AddPair(Codes.BL, Descriptions.BL);
			list.AddPair(Codes.BN, Descriptions.BN);
			list.AddPair(Codes.BO, Descriptions.BO);
			list.AddPair(Codes.BP, Descriptions.BP);
			list.AddPair(Codes.BQ, Descriptions.BQ);
			list.AddPair(Codes.BR, Descriptions.BR);
			list.AddPair(Codes.BS, Descriptions.BS);
			list.AddPair(Codes.BU, Descriptions.BU);
			list.AddPair(Codes.BV, Descriptions.BV);
			list.AddPair(Codes.BX, Descriptions.BX);
			list.AddPair(Codes.BZ, Descriptions.BZ);
			list.AddPair(Codes.CA, Descriptions.CA);
			list.AddPair(Codes.CAG, Descriptions.CAG);
			list.AddPair(FDAUQList.Codes.CB, FDAUQList.Descriptions.CB);
			list.AddPair(Codes.CC, Descriptions.CC);
			list.AddPair(Codes.CE, Descriptions.CE);
			list.AddPair(Codes.CF, Descriptions.CF);
			list.AddPair(Codes.CH, Descriptions.CH);
			list.AddPair(Codes.CI, Descriptions.CI);
			list.AddPair(Codes.CJ, Descriptions.CJ);
			list.AddPair(Codes.CK, Descriptions.CK);
			list.AddPair(Codes.CL, Descriptions.CL);
			list.AddPair(Codes.CO, Descriptions.CO);
			list.AddPair(Codes.CON, Descriptions.CON);
			list.AddPair(Codes.CP, Descriptions.CP);
			list.AddPair(Codes.CR, Descriptions.CR);
			list.AddPair(Codes.CS, Descriptions.CS);
			list.AddPair(Codes.CT, Descriptions.CT);
			list.AddPair(Codes.CU, Descriptions.CU);
			list.AddPair(Codes.CV, Descriptions.CV);
			list.AddPair(Codes.CX, Descriptions.CX);
			list.AddPair(Codes.CY, Descriptions.CY);
			list.AddPair(Codes.CZ, Descriptions.CZ);
			list.AddPair(Codes.DJ, Descriptions.DJ);
			list.AddPair(Codes.DP, Descriptions.DP);
			list.AddPair(Codes.DR, Descriptions.DR);
			list.AddPair(Codes.EN, Descriptions.EN);
			list.AddPair(FDAUQList.Codes.FC, FDAUQList.Descriptions.FC);
			list.AddPair(Codes.FD, Descriptions.FD);
			list.AddPair(Codes.FI, Descriptions.FI);
			list.AddPair(Codes.FL, Descriptions.FL);
			list.AddPair(Codes.FO, Descriptions.FO);
			list.AddPair(Codes.FR, Descriptions.FR);
			list.AddPair(Codes.GB, Descriptions.GB);
			list.AddPair(Codes.HG, Descriptions.HG);
			list.AddPair(Codes.HR, Descriptions.HR);
			list.AddPair(Codes.JC, Descriptions.JC);
			list.AddPair(Codes.JG, Descriptions.JG);
			list.AddPair(Codes.JR, Descriptions.JR);
			list.AddPair(Codes.JT, Descriptions.JT);
			list.AddPair(Codes.JY, Descriptions.JY);
			list.AddPair(Codes.KEG, Descriptions.KEG);
			list.AddPair(FDAUQList.Codes.KIT, FDAUQList.Descriptions.KIT);
			list.AddPair(Codes.MB, Descriptions.MB);
			list.AddPair(Codes.MC, Descriptions.MC);
			list.AddPair(Codes.MS, Descriptions.MS);
			list.AddPair(Codes.MT, Descriptions.MT);
			list.AddPair(Codes.NE, Descriptions.NE);
			list.AddPair(Codes.NS, Descriptions.NS);
			list.AddPair(Codes.NT, Descriptions.NT);
			list.AddPair(Codes.PA, Descriptions.PA);
			list.AddPair(Codes.PAL, Descriptions.PAL);
			list.AddPair(Codes.PC, Descriptions.PC);
			list.AddPair(Codes.PH, Descriptions.PH);
			list.AddPair(Codes.PK, Descriptions.PK);
			list.AddPair(Codes.PL, Descriptions.PL);
			list.AddPair(Codes.PO, Descriptions.PO);
			list.AddPair(Codes.PT, Descriptions.PT);
			list.AddPair(Codes.PU, Descriptions.PU);
			list.AddPair(Codes.PY, Descriptions.PY);
			list.AddPair(Codes.RG, Descriptions.RG);
			list.AddPair(Codes.RO, Descriptions.RO);
			list.AddPair(Codes.SA, Descriptions.SA);
			list.AddPair(Codes.SC, Descriptions.SC);
			list.AddPair(Codes.SD, Descriptions.SD);
			list.AddPair(Codes.SE, Descriptions.SE);
			list.AddPair(Codes.SH, Descriptions.SH);
			list.AddPair(Codes.SK, Descriptions.SK);
			list.AddPair(Codes.SL, Descriptions.SL);
			list.AddPair(Codes.SU, Descriptions.SU);
			list.AddPair(Codes.SW, Descriptions.SW);
			list.AddPair(Codes.SZ, Descriptions.SZ);
			list.AddPair(Codes.TB, Descriptions.TB);
			list.AddPair(Codes.TC, Descriptions.TC);
			list.AddPair(Codes.TD, Descriptions.TD);
			list.AddPair(Codes.TK, Descriptions.TK);
			list.AddPair(Codes.TN, Descriptions.TN);
			list.AddPair(Codes.TO, Descriptions.TO);
			list.AddPair(Codes.TR, Descriptions.TR);
			list.AddPair(Codes.TS, Descriptions.TS);
			list.AddPair(Codes.TU, Descriptions.TU);
			list.AddPair(Codes.TY, Descriptions.TY);
			list.AddPair(Codes.TZ, Descriptions.TZ);
			list.AddPair(Codes.VA, Descriptions.VA);
			list.AddPair(Codes.VG, Descriptions.VG);
			list.AddPair(Codes.VI, Descriptions.VI);
			list.AddPair(Codes.VL, Descriptions.VL);
			list.AddPair(Codes.VO, Descriptions.VO);
			list.AddPair(Codes.VP, Descriptions.VP);
			list.AddPair(Codes.VQ, Descriptions.VQ);
			list.AddPair(Codes.VR, Descriptions.VR);
			list.AddPair(Codes.VY, Descriptions.VY);
			list.AddPair(Codes.WB, Descriptions.WB);
			return list;
		}

		static CodeDescriptionPairList GetFooBaseUQCodeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(FDABaseUQList.Codes.BBL, FDABaseUQList.Descriptions.BBL);
			list.AddPair(FDABaseUQList.Codes.BOL, FDABaseUQList.Descriptions.BOL);
			list.AddPair(FDABaseUQList.Codes.CAR, FDABaseUQList.Descriptions.CAR);
			list.AddPair(FDABaseUQList.Codes.CAP, FDABaseUQList.Descriptions.CAP);
			list.AddPair(FDABaseUQList.Codes.CFT, FDABaseUQList.Descriptions.CFT);
			list.AddPair(FDABaseUQList.Codes.CG, FDABaseUQList.Descriptions.CG);
			list.AddPair(FDABaseUQList.Codes.CM3, FDABaseUQList.Descriptions.CM3);
			list.AddPair(FDABaseUQList.Codes.CYD, FDABaseUQList.Descriptions.CYD);
			list.AddPair(FDABaseUQList.Codes.DOZ, FDABaseUQList.Descriptions.DOZ);
			list.AddPair(FDABaseUQList.Codes.DPC, FDABaseUQList.Descriptions.DPC);
			list.AddPair(FDABaseUQList.Codes.DPR, FDABaseUQList.Descriptions.DPR);
			list.AddPair(FDABaseUQList.Codes.FOZ, FDABaseUQList.Descriptions.FOZ);
			list.AddPair(FDABaseUQList.Codes.G, FDABaseUQList.Descriptions.G);
			list.AddPair(FDABaseUQList.Codes.GAL, FDABaseUQList.Descriptions.GAL);
			list.AddPair(FDABaseUQList.Codes.GR, FDABaseUQList.Descriptions.GR);
			list.AddPair(FDABaseUQList.Codes.KG, FDABaseUQList.Descriptions.KG);
			list.AddPair(FDABaseUQList.Codes.KM3, FDABaseUQList.Descriptions.KM3);
			list.AddPair(FDABaseUQList.Codes.L, FDABaseUQList.Descriptions.L);
			list.AddPair(FDABaseUQList.Codes.LB, FDABaseUQList.Descriptions.LB);
			list.AddPair(FDABaseUQList.Codes.M3, FDABaseUQList.Descriptions.M3);
			list.AddPair(FDAUQList.Codes.MCG, FDAUQList.Descriptions.MCG);
			list.AddPair(FDABaseUQList.Codes.MG, FDAUQList.Descriptions.MG);
			list.AddPair(FDABaseUQList.Codes.ML, FDAUQList.Descriptions.ML);
			list.AddPair(FDABaseUQList.Codes.NO, FDABaseUQList.Descriptions.NO);
			list.AddPair(FDABaseUQList.Codes.OZ, FDABaseUQList.Descriptions.OZ);
			list.AddPair(FDABaseUQList.Codes.PCS, FDABaseUQList.Descriptions.PCS);
			list.AddPair(FDABaseUQList.Codes.PRS, FDABaseUQList.Descriptions.PRS);
			list.AddPair(FDABaseUQList.Codes.PTL, FDABaseUQList.Descriptions.PTL);
			list.AddPair(FDABaseUQList.Codes.QTL, FDABaseUQList.Descriptions.QTL);
			list.AddPair(FDABaseUQList.Codes.STN, FDABaseUQList.Descriptions.STN);
			list.AddPair(FDABaseUQList.Codes.T, FDABaseUQList.Descriptions.T);
			list.AddPair(FDABaseUQList.Codes.TAB, FDABaseUQList.Descriptions.TAB);
			list.AddPair(FDABaseUQList.Codes.TON, FDABaseUQList.Descriptions.TON);
			list.AddPair(FDABaseUQList.Codes.TOZ, FDABaseUQList.Descriptions.TOZ);
			return list;
		}

		#endregion

		static CodeDescriptionPairList GetCOSOrVMEOrDRUUQCodeList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Codes.AE, Descriptions.AE);
			list.AddPair(Codes.AM, Descriptions.AM);
			list.AddPair(Codes.AP, Descriptions.AP);
			list.AddPair(Codes.AT, Descriptions.AT);
			list.AddPair(Codes.BA, Descriptions.BA);
			list.AddPair(Codes.BB, Descriptions.BB);
			list.AddPair(Codes.BC, Descriptions.BC);
			list.AddPair(Codes.BD, Descriptions.BD);
			list.AddPair(Codes.BE, Descriptions.BE);
			list.AddPair(Codes.BF, Descriptions.BF);
			list.AddPair(Codes.BG, Descriptions.BG);
			list.AddPair(Codes.BH, Descriptions.BH);
			list.AddPair(Codes.BI, Descriptions.BI);
			list.AddPair(Codes.BJ, Descriptions.BJ);
			list.AddPair(Codes.BK, Descriptions.BK);
			list.AddPair(Codes.BL, Descriptions.BL);
			list.AddPair(Codes.BN, Descriptions.BN);
			list.AddPair(Codes.BO, Descriptions.BO);
			list.AddPair(Codes.BP, Descriptions.BP);
			list.AddPair(Codes.BQ, Descriptions.BQ);
			list.AddPair(Codes.BR, Descriptions.BR);
			list.AddPair(Codes.BS, Descriptions.BS);
			list.AddPair(Codes.BT, Descriptions.BT);
			list.AddPair(Codes.BU, Descriptions.BU);
			list.AddPair(Codes.BV, Descriptions.BV);
			list.AddPair(Codes.BX, Descriptions.BX);
			list.AddPair(Codes.BY, Descriptions.BY);
			list.AddPair(Codes.BZ, Descriptions.BZ);
			list.AddPair(Codes.CA, Descriptions.CA);
			list.AddPair(Codes.CAG, Descriptions.CAG);
			list.AddPair(FDAUQList.Codes.CB, FDAUQList.Descriptions.CB);
			list.AddPair(Codes.CC, Descriptions.CC);
			list.AddPair(Codes.CE, Descriptions.CE);
			list.AddPair(Codes.CF, Descriptions.CF);
			list.AddPair(Codes.CH, Descriptions.CH);
			list.AddPair(Codes.CI, Descriptions.CI);
			list.AddPair(Codes.CJ, Descriptions.CJ);
			list.AddPair(Codes.CK, Descriptions.CK);
			list.AddPair(Codes.CL, Descriptions.CL);
			list.AddPair(Codes.CO, Descriptions.CO);
			list.AddPair(Codes.CON, Descriptions.CON);
			list.AddPair(Codes.CP, Descriptions.CP);
			list.AddPair(Codes.CR, Descriptions.CR);
			list.AddPair(Codes.CS, Descriptions.CS);
			list.AddPair(Codes.CT, Descriptions.CT);
			list.AddPair(Codes.CTR, Descriptions.CTR);
			list.AddPair(Codes.CU, Descriptions.CU);
			list.AddPair(Codes.CV, Descriptions.CV);
			list.AddPair(Codes.CX, Descriptions.CX);
			list.AddPair(Codes.CY, Descriptions.CY);
			list.AddPair(Codes.CZ, Descriptions.CZ);
			list.AddPair(Codes.DJ, Descriptions.DJ);
			list.AddPair(Codes.DP, Descriptions.DP);
			list.AddPair(Codes.DR, Descriptions.DR);
			list.AddPair(Codes.EN, Descriptions.EN);
			list.AddPair(Codes.FC, Descriptions.FC);
			list.AddPair(Codes.FD, Descriptions.FD);
			list.AddPair(Codes.FI, Descriptions.FI);
			list.AddPair(Codes.FL, Descriptions.FL);
			list.AddPair(Codes.FO, Descriptions.FO);
			list.AddPair(Codes.FP, Descriptions.FP);
			list.AddPair(Codes.FR, Descriptions.FR);
			list.AddPair(Codes.GB, Descriptions.GB);
			list.AddPair(Codes.GI, Descriptions.GI);
			list.AddPair(Codes.GZ, Descriptions.GZ);
			list.AddPair(Codes.HG, Descriptions.HG);
			list.AddPair(Codes.HR, Descriptions.HR);
			list.AddPair(Codes.IN, Descriptions.IN);
			list.AddPair(Codes.IZ, Descriptions.IZ);
			list.AddPair(Codes.JC, Descriptions.JC);
			list.AddPair(Codes.JG, Descriptions.JG);
			list.AddPair(Codes.JR, Descriptions.JR);
			list.AddPair(Codes.JT, Descriptions.JT);
			list.AddPair(Codes.JY, Descriptions.JY);
			list.AddPair(Codes.KEG, Descriptions.KEG);
			list.AddPair(FDAUQList.Codes.KIT, FDAUQList.Descriptions.KIT);
			list.AddPair(Codes.LG, Descriptions.LG);
			list.AddPair(Codes.LZ, Descriptions.LZ);
			list.AddPair(Codes.MB, Descriptions.MB);
			list.AddPair(Codes.MC, Descriptions.MC);
			list.AddPair(Codes.MS, Descriptions.MS);
			list.AddPair(Codes.MT, Descriptions.MT);
			list.AddPair(Codes.MX, Descriptions.MX);
			list.AddPair(Codes.NE, Descriptions.NE);
			list.AddPair(Codes.NS, Descriptions.NS);
			list.AddPair(Codes.NT, Descriptions.NT);
			list.AddPair(Codes.PA, Descriptions.PA);
			list.AddPair(Codes.PAL, Descriptions.PAL);
			list.AddPair(Codes.PC, Descriptions.PC);
			list.AddPair(Codes.PG, Descriptions.PG);
			list.AddPair(Codes.PH, Descriptions.PH);
			list.AddPair(Codes.PI, Descriptions.PI);
			list.AddPair(Codes.PK, Descriptions.PK);
			list.AddPair(Codes.PL, Descriptions.PL);
			list.AddPair(Codes.PN, Descriptions.PN);
			list.AddPair(Codes.PO, Descriptions.PO);
			list.AddPair(Codes.PT, Descriptions.PT);
			list.AddPair(Codes.PU, Descriptions.PU);
			list.AddPair(Codes.PY, Descriptions.PY);
			list.AddPair(Codes.PZ, Descriptions.PZ);
			list.AddPair(FDAUQList.Codes.RD, FDAUQList.Descriptions.RD);
			list.AddPair(Codes.RG, Descriptions.RG);
			list.AddPair(Codes.RL, Descriptions.RL);
			list.AddPair(Codes.RO, Descriptions.RO);
			list.AddPair(Codes.RT, Descriptions.RT);
			list.AddPair(Codes.RZ, Descriptions.RZ);
			list.AddPair(Codes.SA, Descriptions.SA);
			list.AddPair(Codes.SC, Descriptions.SC);
			list.AddPair(Codes.SD, Descriptions.SD);
			list.AddPair(Codes.SE, Descriptions.SE);
			list.AddPair(Codes.SH, Descriptions.SH);
			list.AddPair(Codes.SK, Descriptions.SK);
			list.AddPair(Codes.SL, Descriptions.SL);
			list.AddPair(Codes.SM, Descriptions.SM);
			list.AddPair(Codes.ST, Descriptions.ST);
			list.AddPair(Codes.SU, Descriptions.SU);
			list.AddPair(Codes.SW, Descriptions.SW);
			list.AddPair(Codes.SZ, Descriptions.SZ);
			list.AddPair(Codes.SY, Descriptions.SY);
			list.AddPair(Codes.TB, Descriptions.TB);
			list.AddPair(Codes.TC, Descriptions.TC);
			list.AddPair(Codes.TD, Descriptions.TD);
			list.AddPair(Codes.TK, Descriptions.TK);
			list.AddPair(Codes.TN, Descriptions.TN);
			list.AddPair(Codes.TO, Descriptions.TO);
			list.AddPair(Codes.TR, Descriptions.TR);
			list.AddPair(Codes.TS, Descriptions.TS);
			list.AddPair(Codes.TU, Descriptions.TU);
			list.AddPair(Codes.TY, Descriptions.TY);
			list.AddPair(Codes.TZ, Descriptions.TZ);
			list.AddPair(Codes.VA, Descriptions.VA);
			list.AddPair(Codes.VG, Descriptions.VG);
			list.AddPair(Codes.VI, Descriptions.VI);
			list.AddPair(Codes.VL, Descriptions.VL);
			list.AddPair(Codes.VO, Descriptions.VO);
			list.AddPair(Codes.VP, Descriptions.VP);
			list.AddPair(Codes.VQ, Descriptions.VQ);
			list.AddPair(Codes.VR, Descriptions.VR);
			list.AddPair(Codes.VY, Descriptions.VY);
			list.AddPair(Codes.WB, Descriptions.WB);
			return list;
		}
	}
}
