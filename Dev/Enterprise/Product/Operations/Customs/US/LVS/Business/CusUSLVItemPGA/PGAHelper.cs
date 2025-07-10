using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.LVS.Business
{
	public class PGAHelper
	{
		public PGAHelper(CusUSLVItemPGA uSLVItemPGA)
		{
			this.uSLVItemPGA = uSLVItemPGA;
		}

		readonly CusUSLVItemPGA uSLVItemPGA;

		public ZString DisclaimReason
		{
			get
			{
				return GetDisclaimReason();
			}
		}

		public static (string Agency, string AgencyProgram) GetAgencyViaKey(string key)
		{
			var code = DisclaimReasonAndIndicatorCodePairList.FirstOrDefault(u => u.DisclaimReason == key);
			return (code.Agency, code.AgencyProgram);
		}

		ZString GetDisclaimReason()
		{
			return DisclaimReasonAndIndicatorCodePairList.FirstOrDefault(u => u.Agency == uSLVItemPGA.ULP_Agency && u.AgencyProgram == uSLVItemPGA.ULP_AgencyProgram).DisclaimReason ?? string.Empty;
		}

		public static ZString GetIndicatorCode(ZString agency, ZString agencyProgram)
		{
			return DisclaimReasonAndIndicatorCodePairList.FirstOrDefault(u => u.Agency == agency && u.AgencyProgram == agencyProgram).IndicatorCode ?? string.Empty;
		}

		[ThreadStatic]
		static List<(string Agency, string AgencyProgram, string DisclaimReason, string IndicatorCode)> disclaimReasonAndIndicatorCodePairList;
		static List<(string Agency, string AgencyProgram, string DisclaimReason, string IndicatorCode)> DisclaimReasonAndIndicatorCodePairList
		{
			get
			{
				return disclaimReasonAndIndicatorCodePairList ?? (disclaimReasonAndIndicatorCodePairList = new List<(string Agency, string AgencyProgram, string DisclaimReason, string IndicatorCode)>
				{
					("APH", "AVS", LVSConstants.AddInfoConstants.APHISDisclaimReason, LVSConstants.AddInfoConstants.APHISInd),
					("CPS", "CPS", LVSConstants.AddInfoConstants.CPSCDisclaimReason, LVSConstants.AddInfoConstants.CPSCInd),
					("DEA", "DEA", LVSConstants.AddInfoConstants.DEADisclaimReason, LVSConstants.AddInfoConstants.DEAInd),
					("NHT", "OFF", LVSConstants.AddInfoConstants.NHTDisclaimReason, LVSConstants.AddInfoConstants.NHTSAIndicator),
					("EPA", "ODS", LVSConstants.AddInfoConstants.ODSDisclaimReason, LVSConstants.AddInfoConstants.ODSInd),
					("EPA", "PS1", LVSConstants.AddInfoConstants.PSTDisclaimReason, LVSConstants.AddInfoConstants.PSTIndicator),
					("EPA", "TS1", LVSConstants.AddInfoConstants.TSCADisclaimReason, LVSConstants.AddInfoConstants.TSCAInd),
					("EPA", "VNE", LVSConstants.AddInfoConstants.VNEDisclaimReason, LVSConstants.AddInfoConstants.VNEInd),
					("FDA", "FDA", LVSConstants.AddInfoConstants.FDADisclaimReason, LVSConstants.AddInfoConstants.FDAIndicator),
					("FWS", "FWS", LVSConstants.AddInfoConstants.FWSDisclaimReason, LVSConstants.AddInfoConstants.FWSInd),
					("APH", "APL", LVSConstants.AddInfoConstants.LaceyDisclaimReason, LVSConstants.AddInfoConstants.LaceyIndicator),
					("NMF", "370", LVSConstants.AddInfoConstants.NMFS370DisclaimReason, LVSConstants.AddInfoConstants.NMFS370Ind),
					("NMF", "AMR", LVSConstants.AddInfoConstants.NMFSAMRDisclaimReason, LVSConstants.AddInfoConstants.NMFSAMRInd),
					("NMF", "HMS", LVSConstants.AddInfoConstants.NMFSHMSDisclaimReason, LVSConstants.AddInfoConstants.NMFSHMSInd),
					("OMC", "OMC", LVSConstants.AddInfoConstants.OMCDisclaimReason, LVSConstants.AddInfoConstants.OMCInd),
					("TTB", "TOB", LVSConstants.AddInfoConstants.TTBDisclaimReason, LVSConstants.AddInfoConstants.TTBInd),
					("AMS", "MO8", LVSConstants.AddInfoConstants.AMSDisclaimReason, LVSConstants.AddInfoConstants.AMSInd),
					("NOP", "OR1", LVSConstants.AddInfoConstants.NOPDisclaimReason, LVSConstants.AddInfoConstants.NOPInd),
					("FSI", "FSI", LVSConstants.AddInfoConstants.FSISDisclaimReason, LVSConstants.AddInfoConstants.FSISInd),
					("ATF", "AFT", LVSConstants.AddInfoConstants.ATFDisclaimReason, LVSConstants.AddInfoConstants.ATFInd),
					("DTC", "DTC", LVSConstants.AddInfoConstants.DDTCDisclaimReason, LVSConstants.AddInfoConstants.DDTCInd),
					("NMF", "SIM", LVSConstants.AddInfoConstants.NMFSSIMPDisclaimReason, LVSConstants.AddInfoConstants.NMFSSIMPInd),
				});
			}
		}
	}
}
