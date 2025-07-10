using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public partial class LocationList
	{
		public static CodeDescriptionPairList GetLocationList(string port, string area)
		{
			var list = new CodeDescriptionPairList();

			if (!string.IsNullOrEmpty(port) && string.IsNullOrEmpty(area))
			{
				foreach (ICodeDescription areaCodeDescription in AreaList.GetAreaList(port))
				{
					list.AddRange(GetLocationList(port, areaCodeDescription.Code));
				}
			}

			switch (port)
			{
				case "FRBOD":
					switch (area)
					{
						case AreaList.Codes.ANO:
							list.AddPair(Codes.RL950, Descriptions.RL950);
							break;
						case AreaList.Codes.BAS:
							list.AddPair(Codes.BASBAL, Descriptions.BASBAL);
							list.AddPair(Codes.BASSEA, Descriptions.BASSEA);
							list.AddPair(Codes.BASVAT, Descriptions.BASVAT);
							list.AddPair(Codes.FORESA, Descriptions.FORESA);
							list.AddPair(Codes.RL413, Descriptions.RL413);
							list.AddPair(Codes.RL414, Descriptions.RL414);
							list.AddPair(Codes.RL415, Descriptions.RL415);
							list.AddPair(Codes.RL416, Descriptions.RL416);
							list.AddPair(Codes.RL417, Descriptions.RL417);
							list.AddPair(Codes.RL431, Descriptions.RL431);
							list.AddPair(Codes.RL434, Descriptions.RL434);
							list.AddPair(Codes.RL435, Descriptions.RL435);
							list.AddPair(Codes.RL436, Descriptions.RL436);
							list.AddPair(Codes.RL449, Descriptions.RL449);
							break;
						case AreaList.Codes.BEC:
							list.AddPair(Codes.RL511, Descriptions.RL511);
							list.AddPair(Codes.RL512, Descriptions.RL512);
							break;
						case AreaList.Codes.BRU:
							list.AddPair(Codes.BRUVAL, Descriptions.BRUVAL);
							list.AddPair(Codes.EGETRA, Descriptions.EGETRA);
							list.AddPair(Codes.SCHENK, Descriptions.SCHENK);
							list.AddPair(Codes.STAC, Descriptions.STAC);
							break;
						case AreaList.Codes.BYE:
							list.AddPair(Codes.RL600, Descriptions.RL600);
							list.AddPair(Codes.RL602, Descriptions.RL602);
							break;
						case AreaList.Codes.EFLV:
							list.AddPair(Codes.PROLOI, Descriptions.PROLOI);
							break;
						case AreaList.Codes.LVE:
							list.AddPair(Codes.LVETCSO, Descriptions.LVETCSO);
							break;
						case AreaList.Codes.PAP:
							list.AddPair(Codes.RL700, Descriptions.RL700);
							list.AddPair(Codes.RL710, Descriptions.RL710);
							break;
					}
					break;
				case "FRDKK":
					switch (area)
					{
						case AreaList.Codes.DGS:
							list.AddPair(Codes.LDCTM, Descriptions.LDCTM);
							break;
						case AreaList.Codes.DKKE:
							list.AddPair(Codes.BRKSTE, Descriptions.BRKSTE);
							list.AddPair(Codes.CEMEX, Descriptions.CEMEX);
							list.AddPair(Codes.DPC, Descriptions.DPC);
							list.AddPair(Codes.F10COG, Descriptions.F10COG);
							list.AddPair(Codes.F10EQIOM, Descriptions.F10EQIOM);
							list.AddPair(Codes.F10GCMR, Descriptions.F10GCMR);
							list.AddPair(Codes.F10STE, Descriptions.F10STE);
							list.AddPair(Codes.F12BAR, Descriptions.F12BAR);
							list.AddPair(Codes.F12COG, Descriptions.F12COG);
							list.AddPair(Codes.F12STE, Descriptions.F12STE);
							list.AddPair(Codes.F13COG, Descriptions.F13COG);
							list.AddPair(Codes.F13STE, Descriptions.F13STE);
							list.AddPair(Codes.F6GCMR, Descriptions.F6GCMR);
							list.AddPair(Codes.F9COG, Descriptions.F9COG);
							list.AddPair(Codes.F9STE, Descriptions.F9STE);
							list.AddPair(Codes.LEMBRK, Descriptions.LEMBRK);
							list.AddPair(Codes.NORCER, Descriptions.NORCER);
							list.AddPair(Codes.PANAMAR, Descriptions.PANAMAR);
							list.AddPair(Codes.PRODAM, Descriptions.PRODAM);
							list.AddPair(Codes.PROF12, Descriptions.PROF12);
							list.AddPair(Codes.PROF13, Descriptions.PROF13);
							list.AddPair(Codes.QDMTAMAL, Descriptions.QDMTAMAL);
							list.AddPair(Codes.QDMTAMF, Descriptions.QDMTAMF);
							list.AddPair(Codes.QESCBAR, Descriptions.QESCBAR);
							list.AddPair(Codes.QESCSTE, Descriptions.QESCSTE);
							list.AddPair(Codes.QGSAMAL, Descriptions.QGSAMAL);
							list.AddPair(Codes.QGSAMF, Descriptions.QGSAMF);
							list.AddPair(Codes.QGSIMALU, Descriptions.QGSIMALU);
							list.AddPair(Codes.QMVAMAL, Descriptions.QMVAMAL);
							list.AddPair(Codes.QMVAMF, Descriptions.QMVAMF);
							list.AddPair(Codes.QPOLSTE, Descriptions.QPOLSTE);
							list.AddPair(Codes.QSOLAMAL, Descriptions.QSOLAMAL);
							list.AddPair(Codes.QSOLAMF, Descriptions.QSOLAMF);
							list.AddPair(Codes.RL440, Descriptions.RL440);
							list.AddPair(Codes.RUBIS, Descriptions.RUBIS);
							list.AddPair(Codes.SRD, Descriptions.SRD);
							list.AddPair(Codes.TMV2DEST, Descriptions.TMV2DEST);
							list.AddPair(Codes.TOSF12, Descriptions.TOSF12);
							list.AddPair(Codes.TOSF13, Descriptions.TOSF13);
							list.AddPair(Codes.TOTAL, Descriptions.TOTAL);
							list.AddPair(Codes.VERSALIS, Descriptions.VERSALIS);
							break;
						case AreaList.Codes.DKKH:
							list.AddPair(Codes.CMAEFD, Descriptions.CMAEFD);
							list.AddPair(Codes.CMARED, Descriptions.CMARED);
							list.AddPair(Codes.CMATLN, Descriptions.CMATLN);
							list.AddPair(Codes.DEKTLN, Descriptions.DEKTLN);
							list.AddPair(Codes.DESDEL, Descriptions.DESDEL);
							list.AddPair(Codes.DESDESM, Descriptions.DESDESM);
							list.AddPair(Codes.DESSOF, Descriptions.DESSOF);
							list.AddPair(Codes.DESSOT, Descriptions.DESSOT);
							list.AddPair(Codes.DESTLN, Descriptions.DESTLN);
							list.AddPair(Codes.DEWNVR, Descriptions.DEWNVR);
							list.AddPair(Codes.DEWRYS, Descriptions.DEWRYS);
							list.AddPair(Codes.DHL, Descriptions.DHL);
							list.AddPair(Codes.DHLDEN, Descriptions.DHLDEN);
							list.AddPair(Codes.DHLDESM, Descriptions.DHLDESM);
							list.AddPair(Codes.DHLEFD, Descriptions.DHLEFD);
							list.AddPair(Codes.DHLSIN, Descriptions.DHLSIN);
							list.AddPair(Codes.DHLTLN, Descriptions.DHLTLN);
							list.AddPair(Codes.DSVDEN, Descriptions.DSVDEN);
							list.AddPair(Codes.DSVLDK, Descriptions.DSVLDK);
							list.AddPair(Codes.DSVMGF, Descriptions.DSVMGF);
							list.AddPair(Codes.DSVSIN, Descriptions.DSVSIN);
							list.AddPair(Codes.FRIGA25, Descriptions.FRIGA25);
							list.AddPair(Codes.GEODIS, Descriptions.GEODIS);
							list.AddPair(Codes.GONDES, Descriptions.GONDES);
							list.AddPair(Codes.GONDUN, Descriptions.GONDUN);
							list.AddPair(Codes.GONFRO, Descriptions.GONFRO);
							list.AddPair(Codes.GONHGDK1, Descriptions.GONHGDK1);
							list.AddPair(Codes.GONOGA, Descriptions.GONOGA);
							list.AddPair(Codes.GONSOT, Descriptions.GONSOT);
							list.AddPair(Codes.GONTLN, Descriptions.GONTLN);
							list.AddPair(Codes.LEMMAR, Descriptions.LEMMAR);
							list.AddPair(Codes.LEMTLN, Descriptions.LEMTLN);
							list.AddPair(Codes.NAVDEL, Descriptions.NAVDEL);
							list.AddPair(Codes.NAVDUN, Descriptions.NAVDUN);
							list.AddPair(Codes.NAVMGF, Descriptions.NAVMGF);
							list.AddPair(Codes.NORDAU, Descriptions.NORDAU);
							list.AddPair(Codes.PRODEL, Descriptions.PRODEL);
							list.AddPair(Codes.PROLOG, Descriptions.PROLOG);
							list.AddPair(Codes.PROMAR, Descriptions.PROMAR);
							list.AddPair(Codes.PROOGA, Descriptions.PROOGA);
							list.AddPair(Codes.PROSOT, Descriptions.PROSOT);
							list.AddPair(Codes.PROSPI, Descriptions.PROSPI);
							list.AddPair(Codes.PROSP2, Descriptions.PROSP2);
							list.AddPair(Codes.PROTLN, Descriptions.PROTLN);
							list.AddPair(Codes.ROGDEL, Descriptions.ROGDEL);
							list.AddPair(Codes.ROGDPD, Descriptions.ROGDPD);
							list.AddPair(Codes.ROMDEL, Descriptions.ROMDEL);
							list.AddPair(Codes.ROMDES, Descriptions.ROMDES);
							list.AddPair(Codes.ROMMGF, Descriptions.ROMMGF);
							list.AddPair(Codes.SAGBSL, Descriptions.SAGBSL);
							list.AddPair(Codes.SAGEFD, Descriptions.SAGEFD);
							list.AddPair(Codes.SAGEXT, Descriptions.SAGEXT);
							list.AddPair(Codes.SAGMGF, Descriptions.SAGMGF);
							list.AddPair(Codes.SAGOGA, Descriptions.SAGOGA);
							list.AddPair(Codes.SAGTLN, Descriptions.SAGTLN);
							list.AddPair(Codes.SCARDEL, Descriptions.SCARDEL);
							list.AddPair(Codes.SDVDPE, Descriptions.SDVDPE);
							list.AddPair(Codes.SDVDPM, Descriptions.SDVDPM);
							list.AddPair(Codes.SDVEFD, Descriptions.SDVEFD);
							list.AddPair(Codes.SDVMGF, Descriptions.SDVMGF);
							list.AddPair(Codes.SDVSIN, Descriptions.SDVSIN);
							list.AddPair(Codes.SDVTHU, Descriptions.SDVTHU);
							list.AddPair(Codes.SOGBSL, Descriptions.SOGBSL);
							list.AddPair(Codes.SOGCDK, Descriptions.SOGCDK);
							list.AddPair(Codes.SOGEFD, Descriptions.SOGEFD);
							list.AddPair(Codes.SOGNVR, Descriptions.SOGNVR);
							list.AddPair(Codes.SOGRYS, Descriptions.SOGRYS);
							list.AddPair(Codes.SOGTLN, Descriptions.SOGTLN);
							list.AddPair(Codes.TOSDEL, Descriptions.TOSDEL);
							list.AddPair(Codes.TOSLOG, Descriptions.TOSLOG);
							list.AddPair(Codes.TOSOGA, Descriptions.TOSOGA);
							list.AddPair(Codes.TOSPI2, Descriptions.TOSPI2);
							list.AddPair(Codes.TOSSOT, Descriptions.TOSSOT);
							list.AddPair(Codes.TOSSPI, Descriptions.TOSSPI);
							list.AddPair(Codes.TOSTLN, Descriptions.TOSTLN);
							list.AddPair(Codes.TRADEL, Descriptions.TRADEL);
							list.AddPair(Codes.TRAEFD, Descriptions.TRAEFD);
							list.AddPair(Codes.TRAMGF, Descriptions.TRAMGF);
							list.AddPair(Codes.TRATLN, Descriptions.TRATLN);
							break;
						case AreaList.Codes.DKKO:
							list.AddPair(Codes.CMADAI, Descriptions.CMADAI);
							list.AddPair(Codes.CMADUF, Descriptions.CMADUF);
							list.AddPair(Codes.CMADUN, Descriptions.CMADUN);
							list.AddPair(Codes.DAILYFRE, Descriptions.DAILYFRE);
							list.AddPair(Codes.DESDAI, Descriptions.DESDAI);
							list.AddPair(Codes.DHLDAI, Descriptions.DHLDAI);
							list.AddPair(Codes.DUNFRESH, Descriptions.DUNFRESH);
							list.AddPair(Codes.FLANTDF, Descriptions.FLANTDF);
							list.AddPair(Codes.GONDAI, Descriptions.GONDAI);
							list.AddPair(Codes.GONLOR, Descriptions.GONLOR);
							list.AddPair(Codes.LORBARRA, Descriptions.LORBARRA);
							list.AddPair(Codes.LORSTE, Descriptions.LORSTE);
							list.AddPair(Codes.METHAN, Descriptions.METHAN);
							list.AddPair(Codes.PIF, Descriptions.PIF);
							list.AddPair(Codes.PRODAI, Descriptions.PRODAI);
							list.AddPair(Codes.QPOAMAL, Descriptions.QPOAMAL);
							list.AddPair(Codes.QPOAMF, Descriptions.QPOAMF);
							list.AddPair(Codes.QPODESTO, Descriptions.QPODESTO);
							list.AddPair(Codes.QPOSEA, Descriptions.QPOSEA);
							list.AddPair(Codes.RL702, Descriptions.RL702);
							list.AddPair(Codes.RTINTO, Descriptions.RTINTO);
							list.AddPair(Codes.RYSSEN, Descriptions.RYSSEN);
							list.AddPair(Codes.TOSDAI, Descriptions.TOSDAI);
							list.AddPair(Codes.TTOMDAI, Descriptions.TTOMDAI);
							break;
						case AreaList.Codes.LLE:
							list.AddPair(Codes.LCT, Descriptions.LCT);
							break;
					}
					break;
				case "MQFDF":
					switch (area)
					{
						case AreaList.Codes.BLF:
							list.AddPair(Codes.BLFSARA, Descriptions.BLFSARA);
							list.AddPair(Codes.BLF634, Descriptions.BLF634);
							break;
						case AreaList.Codes.CLF:
							list.AddPair(Codes.CALIF, Descriptions.CALIF);
							list.AddPair(Codes.CLFSARA, Descriptions.CLFSARA);
							break;
						case AreaList.Codes.COHE:
							list.AddPair(Codes.COHEMOUI, Descriptions.COHEMOUI);
							list.AddPair(Codes.COHESARA, Descriptions.COHESARA);
							break;
						case AreaList.Codes.GCA:
							list.AddPair(Codes.GCAAUT, Descriptions.GCAAUT);
							list.AddPair(Codes.GCAFER, Descriptions.GCAFER);
							list.AddPair(Codes.GCASNM, Descriptions.GCASNM);
							break;
						case AreaList.Codes.HPT:
							list.AddPair(Codes.AGS, Descriptions.AGS);
							list.AddPair(Codes.BIOMETAL, Descriptions.BIOMETAL);
							list.AddPair(Codes.BOLLORE, Descriptions.BOLLORE);
							list.AddPair(Codes.CHALONO, Descriptions.CHALONO);
							list.AddPair(Codes.DOUGLAS, Descriptions.DOUGLAS);
							list.AddPair(Codes.FRIGODOM, Descriptions.FRIGODOM);
							list.AddPair(Codes.GEODIS, Descriptions.GEODIS);
							list.AddPair(Codes.IES, Descriptions.IES);
							list.AddPair(Codes.LOGIDOM, Descriptions.LOGIDOM);
							list.AddPair(Codes.POMPIERE, Descriptions.POMPIERE);
							list.AddPair(Codes.SCHENKER, Descriptions.SCHENKER);
							list.AddPair(Codes.SETCARGO, Descriptions.SETCARGO);
							list.AddPair(Codes.SMTL, Descriptions.SMTL);
							list.AddPair(Codes.SOMARTRA, Descriptions.SOMARTRA);
							list.AddPair(Codes.SOMOTRAN, Descriptions.SOMOTRAN);
							list.AddPair(Codes.TMADIK, Descriptions.TMADIK);
							list.AddPair(Codes.TMARTI, Descriptions.TMARTI);
							list.AddPair(Codes.TRANSMAD, Descriptions.TRANSMAD);
							list.AddPair(Codes.TTOM, Descriptions.TTOM);
							break;
						case AreaList.Codes.HYD:
							list.AddPair(Codes.BATAUT, Descriptions.BATAUT);
							list.AddPair(Codes.HYDAUT, Descriptions.HYDAUT);
							list.AddPair(Codes.HYDCHA, Descriptions.HYDCHA);
							list.AddPair(Codes.HYDGMM, Descriptions.HYDGMM);
							list.AddPair(Codes.HYDMAN, Descriptions.HYDMAN);
							list.AddPair(Codes.HYDSOM, Descriptions.HYDSOM);
							list.AddPair(Codes.HYDTRA, Descriptions.HYDTRA);
							list.AddPair(Codes.HYDVOI, Descriptions.HYDVOI);
							break;
						case AreaList.Codes.LAM:
							list.AddPair(Codes.AUTRE, Descriptions.AUTRE);
							list.AddPair(Codes.DFCARGO, Descriptions.DFCARGO);
							list.AddPair(Codes.TRANSAIR, Descriptions.TRANSAIR);
							break;
						case AreaList.Codes.MAR:
							list.AddPair(Codes.MARPORT, Descriptions.MARPORT);
							list.AddPair(Codes.MARSNM, Descriptions.MARSNM);
							break;
						case AreaList.Codes.PDC:
							list.AddPair(Codes.PDCAUT, Descriptions.PDCAUT);
							list.AddPair(Codes.PDCSARA, Descriptions.PDCSARA);
							break;
						case AreaList.Codes.PDG:
							list.AddPair(Codes.PDGGMM, Descriptions.PDGGMM);
							list.AddPair(Codes.PDGMAN, Descriptions.PDGMAN);
							list.AddPair(Codes.PDGSOMAR, Descriptions.PDGSOMAR);
							list.AddPair(Codes.PDGTRANS, Descriptions.PDGTRANS);
							break;
						case AreaList.Codes.PET1:
							list.AddPair(Codes.MOUIPET1, Descriptions.MOUIPET1);
							break;
						case AreaList.Codes.PET2:
							list.AddPair(Codes.MOUIPET2, Descriptions.MOUIPET2);
							break;
						case AreaList.Codes.QDT:
							list.AddPair(Codes.QDTSOM, Descriptions.QDTSOM);
							break;
						case AreaList.Codes.QW:
							break;
						case AreaList.Codes.RAD:
							list.AddPair(Codes.RADAUT, Descriptions.RADAUT);
							break;
						case AreaList.Codes.ROB:
							list.AddPair(Codes.ROBERTPO, Descriptions.ROBERTPO);
							break;
					}
					break;
				case "NCNOU":
					switch (area)
					{
						case AreaList.Codes.BUG:
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							break;
						case AreaList.Codes.DNB:
							list.AddPair(Codes.AMRPPIMP, Descriptions.AMRPPIMP);
							list.AddPair(Codes.AMRVRAC, Descriptions.AMRVRAC);
							list.AddPair(Codes.AMSIMP, Descriptions.AMSIMP);
							list.AddPair(Codes.AMSVRAC, Descriptions.AMSVRAC);
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							list.AddPair(Codes.SLNVRAC, Descriptions.SLNVRAC);
							list.AddPair(Codes.TRANSIMP, Descriptions.TRANSIMP);
							list.AddPair(Codes.TRAVRAC, Descriptions.TRAVRAC);
							break;
						case AreaList.Codes.DUS:
							list.AddPair(Codes.AMRPPIMP, Descriptions.AMRPPIMP);
							list.AddPair(Codes.AMRVRAC, Descriptions.AMRVRAC);
							list.AddPair(Codes.CMAIMP, Descriptions.CMAIMP);
							list.AddPair(Codes.CMAVRAC, Descriptions.CMAVRAC);
							list.AddPair(Codes.TRANSIMP, Descriptions.TRANSIMP);
							list.AddPair(Codes.TRAVRAC, Descriptions.TRAVRAC);
							break;
						case AreaList.Codes.KOU:
							list.AddPair(Codes.AMRPPIMP, Descriptions.AMRPPIMP);
							list.AddPair(Codes.AMRVRAC, Descriptions.AMRVRAC);
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							list.AddPair(Codes.NCSVRAC, Descriptions.NCSVRAC);
							break;
						case AreaList.Codes.KRB:
							list.AddPair(Codes.AMRPPIMP, Descriptions.AMRPPIMP);
							list.AddPair(Codes.AMRVRAC, Descriptions.AMRVRAC);
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							break;
						case AreaList.Codes.NAK:
							list.AddPair(Codes.AMRPPIMP, Descriptions.AMRPPIMP);
							list.AddPair(Codes.AMRVRAC, Descriptions.AMRVRAC);
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							break;
						case AreaList.Codes.NEP:
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							list.AddPair(Codes.NCSVRAC, Descriptions.NCSVRAC);
							list.AddPair(Codes.TRANSIMP, Descriptions.TRANSIMP);
							list.AddPair(Codes.TRAVRAC, Descriptions.TRAVRAC);
							break;
						case AreaList.Codes.NOO:
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							break;
						case AreaList.Codes.NOU:
							list.AddPair(Codes.ACT, Descriptions.ACT);
							list.AddPair(Codes.AEC, Descriptions.AEC);
							list.AddPair(Codes.AMRPP, Descriptions.AMRPP);
							list.AddPair(Codes.AMRPPIMP, Descriptions.AMRPPIMP);
							list.AddPair(Codes.AMRPPV, Descriptions.AMRPPV);
							list.AddPair(Codes.AMSUD, Descriptions.AMSUD);
							list.AddPair(Codes.BALLANDE, Descriptions.BALLANDE);
							list.AddPair(Codes.BOLLORE, Descriptions.BOLLORE);
							list.AddPair(Codes.CMACGM, Descriptions.CMACGM);
							list.AddPair(Codes.COTRANS, Descriptions.COTRANS);
							list.AddPair(Codes.DEMPAC, Descriptions.DEMPAC);
							list.AddPair(Codes.DHL, Descriptions.DHL);
							list.AddPair(Codes.GEODIS, Descriptions.GEODIS);
							list.AddPair(Codes.GIEIMP, Descriptions.GIEIMP);
							list.AddPair(Codes.GIEV, Descriptions.GIEV);
							list.AddPair(Codes.IES, Descriptions.IES);
							list.AddPair(Codes.LTN, Descriptions.LTN);
							list.AddPair(Codes.MOANA, Descriptions.MOANA);
							list.AddPair(Codes.MOANAIMP, Descriptions.MOANAIMP);
							list.AddPair(Codes.MOANAV, Descriptions.MOANAV);
							list.AddPair(Codes.NOUTRA, Descriptions.NOUTRA);
							list.AddPair(Codes.PACLOG, Descriptions.PACLOG);
							list.AddPair(Codes.SATIMP, Descriptions.SATIMP);
							list.AddPair(Codes.SATO, Descriptions.SATO);
							list.AddPair(Codes.SATOIMP, Descriptions.SATOIMP);
							list.AddPair(Codes.SATOV, Descriptions.SATOV);
							list.AddPair(Codes.SATV, Descriptions.SATV);
							list.AddPair(Codes.SDVTTI, Descriptions.SDVTTI);
							list.AddPair(Codes.SLN, Descriptions.SLN);
							list.AddPair(Codes.SOCA, Descriptions.SOCA);
							list.AddPair(Codes.TDM, Descriptions.TDM);
							list.AddPair(Codes.TRANSA, Descriptions.TRANSA);
							list.AddPair(Codes.TRANSAV, Descriptions.TRANSAV);
							list.AddPair(Codes.TRANSIMP, Descriptions.TRANSIMP);
							list.AddPair(Codes.TWD, Descriptions.TWD);
							break;
						case AreaList.Codes.NUM:
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							break;
						case AreaList.Codes.OIE:
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							break;
						case AreaList.Codes.PAA:
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							list.AddPair(Codes.NCSVRAC, Descriptions.NCSVRAC);
							break;
						case AreaList.Codes.PNY:
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							list.AddPair(Codes.CMAIMP, Descriptions.CMAIMP);
							list.AddPair(Codes.CMAVRAC, Descriptions.CMAVRAC);
							list.AddPair(Codes.NCSVRAC, Descriptions.NCSVRAC);
							list.AddPair(Codes.TRANSIMP, Descriptions.TRANSIMP);
							list.AddPair(Codes.TRAVRAC, Descriptions.TRAVRAC);
							break;
						case AreaList.Codes.POR:
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							list.AddPair(Codes.NCSVRAC, Descriptions.NCSVRAC);
							break;
						case AreaList.Codes.POU:
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							break;
						case AreaList.Codes.PYA:
							list.AddPair(Codes.AMRPPIMP, Descriptions.AMRPPIMP);
							list.AddPair(Codes.AMRVRAC, Descriptions.AMRVRAC);
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							break;
						case AreaList.Codes.TON:
							list.AddPair(Codes.BALIMP, Descriptions.BALIMP);
							list.AddPair(Codes.BALVRAC, Descriptions.BALVRAC);
							break;
						case AreaList.Codes.TUD:
							list.AddPair(Codes.AMRPPIMP, Descriptions.AMRPPIMP);
							list.AddPair(Codes.AMRVRAC, Descriptions.AMRVRAC);
							break;
						case AreaList.Codes.VAV:
							list.AddPair(Codes.NCSVRAC, Descriptions.NCSVRAC);
							list.AddPair(Codes.TRANSIMP, Descriptions.TRANSIMP);
							list.AddPair(Codes.TRAVRAC, Descriptions.TRAVRAC);
							break;
					}
					break;
				case "FRBES":
				case "FRLRT":
				case "FRSML":
					switch (area)
					{
						case AreaList.Codes.BES:
							list.AddPair(Codes.AGRO1, Descriptions.AGRO1);
							list.AddPair(Codes.AGRO2, Descriptions.AGRO2);
							list.AddPair(Codes.CTRR1, Descriptions.CTRR1);
							list.AddPair(Codes.CTRR2, Descriptions.CTRR2);
							list.AddPair(Codes.DIVERS1, Descriptions.DIVERS1);
							list.AddPair(Codes.DIVERS2, Descriptions.DIVERS2);
							list.AddPair(Codes.DIVERS3, Descriptions.DIVERS3);
							list.AddPair(Codes.INDUS1, Descriptions.INDUS1);
							list.AddPair(Codes.INDUS2, Descriptions.INDUS2);
							list.AddPair(Codes.SABLE1, Descriptions.SABLE1);
							list.AddPair(Codes.SABLE2, Descriptions.SABLE2);
							list.AddPair(Codes.VRACLIQ, Descriptions.VRACLIQ);
							list.AddPair(Codes.VRAC1, Descriptions.VRAC1);
							list.AddPair(Codes.VRAC2, Descriptions.VRAC2);
							list.AddPair(Codes.VRAC3, Descriptions.VRAC3);
							list.AddPair(Codes.VRAC4, Descriptions.VRAC4);
							break;
						case AreaList.Codes.BESE:
							list.AddPair(Codes.BOLLOG1, Descriptions.BOLLOG1);
							list.AddPair(Codes.BOLLOG2, Descriptions.BOLLOG2);
							list.AddPair(Codes.BWS1, Descriptions.BWS1);
							list.AddPair(Codes.BWS2, Descriptions.BWS2);
							list.AddPair(Codes.CUMMINS, Descriptions.CUMMINS);
							list.AddPair(Codes.DOUXSA, Descriptions.DOUXSA);
							list.AddPair(Codes.FAUVESAS, Descriptions.FAUVESAS);
							list.AddPair(Codes.KLM, Descriptions.KLM);
							list.AddPair(Codes.MANUPORT, Descriptions.MANUPORT);
							list.AddPair(Codes.NAVALG1, Descriptions.NAVALG1);
							list.AddPair(Codes.NICOT1, Descriptions.NICOT1);
							list.AddPair(Codes.NICOT2, Descriptions.NICOT2);
							list.AddPair(Codes.SDMO1, Descriptions.SDMO1);
							list.AddPair(Codes.SDMO2, Descriptions.SDMO2);
							list.AddPair(Codes.SDMO3, Descriptions.SDMO3);
							list.AddPair(Codes.SDMO4, Descriptions.SDMO4);
							list.AddPair(Codes.SDMO5, Descriptions.SDMO5);
							list.AddPair(Codes.SDMO6, Descriptions.SDMO6);
							list.AddPair(Codes.SODISE, Descriptions.SODISE);
							list.AddPair(Codes.SODISE2, Descriptions.SODISE2);
							list.AddPair(Codes.SODISE3, Descriptions.SODISE3);
							list.AddPair(Codes.SYNUTRA1, Descriptions.SYNUTRA1);
							list.AddPair(Codes.SYNUTRA2, Descriptions.SYNUTRA2);
							break;
						case AreaList.Codes.LRT:
							list.AddPair(Codes.FERRY1, Descriptions.FERRY1);
							list.AddPair(Codes.FERRY2, Descriptions.FERRY2);
							list.AddPair(Codes.FRIGO1, Descriptions.FRIGO1);
							list.AddPair(Codes.FRIGO2, Descriptions.FRIGO2);
							list.AddPair(Codes.HYDRO, Descriptions.HYDRO);
							list.AddPair(Codes.M230M1, Descriptions.M230M1);
							list.AddPair(Codes.M230M2, Descriptions.M230M2);
							list.AddPair(Codes.RORO1, Descriptions.RORO1);
							list.AddPair(Codes.RORO2, Descriptions.RORO2);
							list.AddPair(Codes.SABLE, Descriptions.SABLE);
							list.AddPair(Codes.SILO1, Descriptions.SILO1);
							list.AddPair(Codes.SILO2, Descriptions.SILO2);
							list.AddPair(Codes.TPLEIN1, Descriptions.TPLEIN1);
							list.AddPair(Codes.TPLEIN2, Descriptions.TPLEIN2);
							break;
						case AreaList.Codes.SML:
							list.AddPair(Codes.B2, Descriptions.B2);
							list.AddPair(Codes.B3, Descriptions.B3);
							list.AddPair(Codes.B4, Descriptions.B4);
							list.AddPair(Codes.B5, Descriptions.B5);
							list.AddPair(Codes.B6, Descriptions.B6);
							list.AddPair(Codes.B7, Descriptions.B7);
							list.AddPair(Codes.DT1, Descriptions.DT1);
							list.AddPair(Codes.DT12, Descriptions.DT12);
							list.AddPair(Codes.DT2, Descriptions.DT2);
							list.AddPair(Codes.DT2ET3, Descriptions.DT2ET3);
							list.AddPair(Codes.DT3, Descriptions.DT3);
							list.AddPair(Codes.DT3ET4, Descriptions.DT3ET4);
							list.AddPair(Codes.DT4, Descriptions.DT4);
							list.AddPair(Codes.DT6, Descriptions.DT6);
							list.AddPair(Codes.JC1, Descriptions.JC1);
							list.AddPair(Codes.JC2, Descriptions.JC2);
							list.AddPair(Codes.JC3, Descriptions.JC3);
							list.AddPair(Codes.JC4, Descriptions.JC4);
							list.AddPair(Codes.V10, Descriptions.V10);
							list.AddPair(Codes.V8, Descriptions.V8);
							list.AddPair(Codes.V9, Descriptions.V9);
							break;
					}
					break;
				case "FRSET":
					switch (area)
					{
						case AreaList.Codes.SET:
							list.AddPair(Codes.BETAIL3, Descriptions.BETAIL3);
							list.AddPair(Codes.BETAIL4, Descriptions.BETAIL4);
							list.AddPair(Codes.CONV1, Descriptions.CONV1);
							list.AddPair(Codes.CONV2, Descriptions.CONV2);
							list.AddPair(Codes.CONV3, Descriptions.CONV3);
							list.AddPair(Codes.CONV4, Descriptions.CONV4);
							list.AddPair(Codes.FERRY1, Descriptions.FERRY1);
							list.AddPair(Codes.FERRY2, Descriptions.FERRY2);
							list.AddPair(Codes.FERRY3, Descriptions.FERRY3);
							list.AddPair(Codes.FERRY4, Descriptions.FERRY4);
							list.AddPair(Codes.PC1, Descriptions.PC1);
							list.AddPair(Codes.PC2, Descriptions.PC2);
							list.AddPair(Codes.PC3, Descriptions.PC3);
							list.AddPair(Codes.PC4, Descriptions.PC4);
							list.AddPair(Codes.PP1, Descriptions.PP1);
							list.AddPair(Codes.PP2, Descriptions.PP2);
							list.AddPair(Codes.PP3, Descriptions.PP3);
							list.AddPair(Codes.PP4, Descriptions.PP4);
							list.AddPair(Codes.PVN1, Descriptions.PVN1);
							list.AddPair(Codes.PVN2, Descriptions.PVN2);
							list.AddPair(Codes.PVN3, Descriptions.PVN3);
							list.AddPair(Codes.PVN4, Descriptions.PVN4);
							list.AddPair(Codes.REM1, Descriptions.REM1);
							list.AddPair(Codes.REM2, Descriptions.REM2);
							list.AddPair(Codes.SINTAX, Descriptions.SINTAX);
							list.AddPair(Codes.VRAC, Descriptions.VRAC);
							list.AddPair(Codes.VRAC1, Descriptions.VRAC1);
							list.AddPair(Codes.VRAC2, Descriptions.VRAC2);
							break;
					}
					break;
				case "GFDDC":
					switch (area)
					{
						case AreaList.Codes.CAY:
							list.AddPair(Codes.AIRFRA, Descriptions.AIRFRA);
							list.AddPair(Codes.GSAF, Descriptions.GSAF);
							list.AddPair(Codes.PISTE, Descriptions.PISTE);
							break;
						case AreaList.Codes.DDC:
							list.AddPair(Codes.DDCGMP, Descriptions.DDCGMP);
							list.AddPair(Codes.DDCGTM, Descriptions.DDCGTM);
							list.AddPair(Codes.DDCRHEA, Descriptions.DDCRHEA);
							list.AddPair(Codes.DDCRLP, Descriptions.DDCRLP);
							list.AddPair(Codes.DDCSOG, Descriptions.DDCSOG);
							list.AddPair(Codes.DDCSOM, Descriptions.DDCSOM);
							list.AddPair(Codes.MER, Descriptions.MER);
							break;
						case AreaList.Codes.DDCA:
							list.AddPair(Codes.ANNDEX, Descriptions.ANNDEX);
							list.AddPair(Codes.CHRONO, Descriptions.CHRONO);
							list.AddPair(Codes.CTS, Descriptions.CTS);
							list.AddPair(Codes.DHLINT, Descriptions.DHLINT);
							list.AddPair(Codes.GEODIS, Descriptions.GEODIS);
							list.AddPair(Codes.GONDRAND, Descriptions.GONDRAND);
							list.AddPair(Codes.GPX, Descriptions.GPX);
							list.AddPair(Codes.IDG, Descriptions.IDG);
							list.AddPair(Codes.OCX, Descriptions.OCX);
							list.AddPair(Codes.SAMEG, Descriptions.SAMEG);
							list.AddPair(Codes.SCHENK, Descriptions.SCHENK);
							list.AddPair(Codes.SDVG, Descriptions.SDVG);
							list.AddPair(Codes.SOMAT, Descriptions.SOMAT);
							list.AddPair(Codes.TEROSI, Descriptions.TEROSI);
							list.AddPair(Codes.TTOM, Descriptions.TTOM);
							break;
						case AreaList.Codes.DDCE:
							list.AddPair(Codes.ABCHEE, Descriptions.ABCHEE);
							list.AddPair(Codes.ASL, Descriptions.ASL);
							list.AddPair(Codes.CTS, Descriptions.CTS);
							list.AddPair(Codes.FGM, Descriptions.FGM);
							list.AddPair(Codes.GEO, Descriptions.GEO);
							list.AddPair(Codes.GON, Descriptions.GON);
							list.AddPair(Codes.MEG, Descriptions.MEG);
							list.AddPair(Codes.SAM, Descriptions.SAM);
							list.AddPair(Codes.SCH, Descriptions.SCH);
							list.AddPair(Codes.SDV, Descriptions.SDV);
							list.AddPair(Codes.SOM, Descriptions.SOM);
							list.AddPair(Codes.TER, Descriptions.TER);
							list.AddPair(Codes.TRD, Descriptions.TRD);
							list.AddPair(Codes.TTOM, Descriptions.TTOM);
							break;
						case AreaList.Codes.KOU:
							list.AddPair(Codes.ENDEL, Descriptions.ENDEL);
							list.AddPair(Codes.KOUGML, Descriptions.KOUGML);
							list.AddPair(Codes.KOUTSO, Descriptions.KOUTSO);
							break;
						case AreaList.Codes.KOUE:
							list.AddPair(Codes.ARI, Descriptions.ARI);
							break;
						case AreaList.Codes.SGEO:
							list.AddPair(Codes.GOND, Descriptions.GOND);
							list.AddPair(Codes.PIRAPR, Descriptions.PIRAPR);
							list.AddPair(Codes.PIRGOND, Descriptions.PIRGOND);
							list.AddPair(Codes.PIRLPG, Descriptions.PIRLPG);
							list.AddPair(Codes.PIROSS, Descriptions.PIROSS);
							list.AddPair(Codes.PIRSCH, Descriptions.PIRSCH);
							list.AddPair(Codes.PONTAPR, Descriptions.PONTAPR);
							list.AddPair(Codes.PONTLPG, Descriptions.PONTLPG);
							list.AddPair(Codes.PONTOSS, Descriptions.PONTOSS);
							list.AddPair(Codes.PONTSCH, Descriptions.PONTSCH);
							list.AddPair(Codes.SCHENK, Descriptions.SCHENK);
							list.AddPair(Codes.SGEO1, Descriptions.SGEO1);
							break;
						case AreaList.Codes.SLM:
							list.AddPair(Codes.BAC, Descriptions.BAC);
							list.AddPair(Codes.SLM1, Descriptions.SLM1);
							break;
					}
					break;
			}

			list.Sort();
			return list;
		}
	}
}
