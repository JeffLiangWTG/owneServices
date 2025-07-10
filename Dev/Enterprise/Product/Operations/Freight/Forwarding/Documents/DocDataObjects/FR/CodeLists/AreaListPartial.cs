using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	partial class AreaList
	{
		public static CodeDescriptionPairList GetAreaList(string portCode)
		{
			var list = new CodeDescriptionPairList();
			switch (portCode)
			{
				case "FRBOD":
					list.AddPair(Codes.ANO, Descriptions.ANO);
					list.AddPair(Codes.BAF, Descriptions.BAF);
					list.AddPair(Codes.BAS, Descriptions.BAS);
					list.AddPair(Codes.BEC, Descriptions.BEC);
					list.AddPair(Codes.BRG, Descriptions.BRG);
					list.AddPair(Codes.BRU, Descriptions.BRU);
					list.AddPair(Codes.BYE, Descriptions.BYE);
					list.AddPair(Codes.EFLV, Descriptions.EFLV);
					list.AddPair(Codes.LVE, Descriptions.LVE);
					list.AddPair(Codes.PAP, Descriptions.PAP);
					list.AddPair(Codes.QUE, Descriptions.QUE);
					list.AddPair(Codes.RAD, Descriptions.RAD);
					list.AddPair(Codes.ZFLV, Descriptions.ZFLV);
					break;
				case "FRDKK":
					list.AddPair(Codes.DGS, Descriptions.DGS);
					list.AddPair(Codes.DKKE, Descriptions.DKKE);
					list.AddPair(Codes.DKKH, Descriptions.DKKH);
					list.AddPair(Codes.DKKO, Descriptions.DKKO);
					list.AddPair(Codes.LLE, Descriptions.LLE);
					break;
				case "MQFDF":
					list.AddPair(Codes.BAT, Descriptions.BAT);
					list.AddPair(Codes.BLF, Descriptions.BLF);
					list.AddPair(Codes.CLF, Descriptions.CLF);
					list.AddPair(Codes.COHE, Descriptions.COHE);
					list.AddPair(Codes.GCA, Descriptions.GCA);
					list.AddPair(Codes.HPT, Descriptions.HPT);
					list.AddPair(Codes.HYD, Descriptions.HYD);
					list.AddPair(Codes.LAM, Descriptions.LAM);
					list.AddPair(Codes.MAR, Descriptions.MAR);
					list.AddPair(Codes.PDC, Descriptions.PDC);
					list.AddPair(Codes.PDG, Descriptions.PDG);
					list.AddPair(Codes.PET1, Descriptions.PET1);
					list.AddPair(Codes.PET2, Descriptions.PET2);
					list.AddPair(Codes.PSI, Descriptions.PSI);
					list.AddPair(Codes.QDT, Descriptions.QDT);
					list.AddPair(Codes.QW, Descriptions.QW);
					list.AddPair(Codes.RAD, Descriptions.RAD);
					list.AddPair(Codes.RADE, Descriptions.RADE);
					list.AddPair(Codes.ROB, Descriptions.ROB);
					list.AddPair(Codes.SCIC, Descriptions.SCIC);
					break;
				case "NCNOU":
					list.AddPair(Codes.AMD, Descriptions.AMD);
					list.AddPair(Codes.BUG, Descriptions.BUG);
					list.AddPair(Codes.CAN, Descriptions.CAN);
					list.AddPair(Codes.DNB, Descriptions.DNB);
					list.AddPair(Codes.DUM, Descriptions.DUM);
					list.AddPair(Codes.DUS, Descriptions.DUS);
					list.AddPair(Codes.GOR, Descriptions.GOR);
					list.AddPair(Codes.HLU, Descriptions.HLU);
					list.AddPair(Codes.HNG, Descriptions.HNG);
					list.AddPair(Codes.ICA, Descriptions.ICA);
					list.AddPair(Codes.ILP, Descriptions.ILP);
					list.AddPair(Codes.IOU, Descriptions.IOU);
					list.AddPair(Codes.KOC, Descriptions.KOC);
					list.AddPair(Codes.KOU, Descriptions.KOU);
					list.AddPair(Codes.KRB, Descriptions.KRB);
					list.AddPair(Codes.LIF, Descriptions.LIF);
					list.AddPair(Codes.MEE, Descriptions.MEE);
					list.AddPair(Codes.MON, Descriptions.MON);
					list.AddPair(Codes.MTD, Descriptions.MTD);
					list.AddPair(Codes.NAK, Descriptions.NAK);
					list.AddPair(Codes.NEP, Descriptions.NEP);
					list.AddPair(Codes.NGO, Descriptions.NGO);
					list.AddPair(Codes.NOO, Descriptions.NOO);
					list.AddPair(Codes.NOU, Descriptions.NOU);
					list.AddPair(Codes.NUM, Descriptions.NUM);
					list.AddPair(Codes.OIE, Descriptions.OIE);
					list.AddPair(Codes.PAA, Descriptions.PAA);
					list.AddPair(Codes.PAM, Descriptions.PAM);
					list.AddPair(Codes.PDE, Descriptions.PDE);
					list.AddPair(Codes.PNY, Descriptions.PNY);
					list.AddPair(Codes.POR, Descriptions.POR);
					list.AddPair(Codes.POU, Descriptions.POU);
					list.AddPair(Codes.PYA, Descriptions.PYA);
					list.AddPair(Codes.TGI, Descriptions.TGI);
					list.AddPair(Codes.THI, Descriptions.THI);
					list.AddPair(Codes.TON, Descriptions.TON);
					list.AddPair(Codes.TUD, Descriptions.TUD);
					list.AddPair(Codes.UVE, Descriptions.UVE);
					list.AddPair(Codes.VAV, Descriptions.VAV);
					list.AddPair(Codes.WAL, Descriptions.WAL);
					list.AddPair(Codes.YAT, Descriptions.YAT);
					list.AddPair(Codes._4NB, Descriptions._4NB);
					break;
				case "FRBES":
				case "FRLRT":
				case "FRSML":
					list.AddPair(Codes.BES, Descriptions.BES);
					list.AddPair(Codes.BESE, Descriptions.BESE);
					list.AddPair(Codes.LRT, Descriptions.LRT);
					list.AddPair(Codes.SML, Descriptions.SML);
					break;
				case "FRSET":
					list.AddPair(Codes.SET, Descriptions.SET);
					break;
				case "GFDDC":
					list.AddPair(Codes.CAY, Descriptions.CAY);
					list.AddPair(Codes.DDC, Descriptions.DDC);
					list.AddPair(Codes.DDCA, Descriptions.DDCA);
					list.AddPair(Codes.DDCE, Descriptions.DDCE);
					list.AddPair(Codes.KOU, Descriptions.KOU);
					list.AddPair(Codes.KOUE, Descriptions.KOUE);
					list.AddPair(Codes.SGEO, Descriptions.SGEO);
					list.AddPair(Codes.SLM, Descriptions.SLM);
					break;
			}

			return list;
		}
	}
}
