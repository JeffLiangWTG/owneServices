using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class ATFCategoryCodeList
	{
		public static bool IsFELRequired(ZString code)
		{
			return code == Codes.API
				   || code == Codes.DDE
				   || code == Codes.ESP
				   || code == Codes.EXP
				   || code == Codes.EXX
				   || code == Codes.FWK
				   || code == Codes.INC
				   || code == Codes.TRA;
		}

		public static bool IsFFLRequired(ZString code)
		{
			return code == Codes.ADD
				   || code == Codes.AMM
				   || code == Codes.AMP
				   || code == Codes.AOW
				   || code == Codes.AP
				   || code == Codes.API
				   || code == Codes.ARP
				   || code == Codes.AW
				   || code == Codes.BAC
				   || code == Codes.BMB
				   || code == Codes.C
				   || code == Codes.DD
				   || code == Codes.DDE
				   || code == Codes.DDF
				   || code == Codes.ESP
				   || code == Codes.GRN
				   || code == Codes.HTZ
				   || code == Codes.IN
				   || code == Codes.INC
				   || code == Codes.LAU
				   || code == Codes.MG
				   || code == Codes.MIN
				   || code == Codes.MIS
				   || code == Codes.MTR
				   || code == Codes.NSA
				   || code == Codes.NSG
				   || code == Codes.NSP
				   || code == Codes.PI
				   || code == Codes.RE
				   || code == Codes.REC
				   || code == Codes.RI
				   || code == Codes.ROC
				   || code == Codes.SBR
				   || code == Codes.SBS
				   || code == Codes.SG
				   || code == Codes.SI
				   || code == Codes.SR
				   || code == Codes.SREK
				   || code == Codes.SS
				   || code == Codes.SSA
				   || code == Codes.SSAP
				   || code == Codes.TRA
				   || code == Codes.TRP
				   || code == Codes.UNK
				   || code == Codes.WHD;
		}

		public static bool IsPermitRequired(ZString code)
		{
			return code != Codes.EXP
				   && code != Codes.FWK
				   && !IsNoneTypeRequired(code);
		}

		public static bool IsAECARequired(ZString code)
		{
			return code != Codes.AW
				   && code != Codes.DD
				   && code != Codes.EXP
				   && code != Codes.FWK
				   && code != Codes.REC
				   && code != Codes.SG
				   && code != Codes.SR
				   && code != Codes.SREK
				   && code != Codes.SS
				   && code != Codes.SSA
				   && code != Codes.SSAP
				   && code != Codes.SSBL
				   && code != Codes.WHP
				   && !IsNoneTypeRequired(code);
		}

		static bool IsNoneTypeRequired(ZString code)
		{
			return code == Codes.NW
				|| code == Codes.SSAX
				|| code == Codes.SSP;
		}
	}
}
