using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class PGAGovernmentAgenciesWithPGACorrection : IPGAGovernmentAgenciesCommon
	{
		public PGAGovernmentAgenciesWithPGACorrection(IACECusEntryLine parentEntryLine, IPGAGovernmentAgenciesCommon entryLine)
		{
			this.entryLine = entryLine;
			this.parentEntryLine = parentEntryLine;
		}
		readonly IPGAGovernmentAgenciesCommon entryLine;
		readonly IACECusEntryLine parentEntryLine;

		#region IPGAGovernmentAgenciesCommon Members

		ZInt IPGAGovernmentAgenciesCommon.LineNumber
		{
			get { return entryLine.LineNumber; }
		}

		ZString IPGAGovernmentAgenciesCommon.Tariff
		{
			get { return entryLine.Tariff; }
		}

		ZString IGovernmentAgencies.CommercialDescription
		{
			get { return entryLine.CommercialDescription; }
		}

		ZBool IGovernmentAgencies.ShouldIncludePGAInMessage(ZBool isCertified, ZString pgaCode)
		{
			return entryLine.ShouldIncludePGAInMessage(isCertified, pgaCode);
		}

		IEnumerable<IFDAData> IGovernmentAgencies.FDALines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.FDALines, x => x.FDALines); }
		}

		ZString IGovernmentAgenciesIndicators.ACEFDADisclaimReason
		{
			get { return entryLine.ACEFDADisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.ACEFDAIndicator
		{
			get { return entryLine.ACEFDAIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.CPSCDisclaimReason
		{
			get { return entryLine.CPSCDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.CPSCIndicator
		{
			get { return entryLine.CPSCIndicator; }
		}

		IEnumerable<ICPSCHeader> IGovernmentAgencies.CPSCHeaders
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.CPSCHeaders, x => x.CPSCHeaders); }
		}

		ZString IGovernmentAgenciesIndicators.AMSDisclaimProgram
		{
			get { return entryLine.AMSDisclaimProgram; }
		}

		ZString IGovernmentAgenciesIndicators.AMSDisclaimReason
		{
			get { return entryLine.AMSDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.AMSIndicator
		{
			get { return entryLine.AMSIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.NOPIndicator
		{
			get { return entryLine.NOPIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.NOPDisclaimReason
		{
			get { return entryLine.NOPDisclaimReason; }
		}

		IEnumerable<IAMSData> IGovernmentAgencies.AMSLines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.AMSLines, x => x.AMSLines); }
		}

		ZString IGovernmentAgenciesIndicators.APHISDisclaimReason
		{
			get { return entryLine.APHISDisclaimReason; }
		}

		IEnumerable<IAPHISHeader> IGovernmentAgencies.APHISHeaders
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.APHISHeaders, PGALinesForPGACorrection.GetAllAPHISLines); }
		}

		ZString IGovernmentAgenciesIndicators.APHISIndicator
		{
			get { return entryLine.APHISIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.ATFIndicator
		{
			get { return entryLine.ATFIndicator; }
		}

		IEnumerable<IATFData> IGovernmentAgencies.ATFLines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.ATFLines, x => x.ATFLines); }
		}

		IDDTCData IGovernmentAgencies.DDTCData
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => PGALinesForPGACorrection.GetDDTCData(x), x => PGALinesForPGACorrection.GetDDTCData(x)).FirstOrDefault(); }
		}

		ZString IGovernmentAgenciesIndicators.DDTCIndicator
		{
			get { return entryLine.DDTCIndicator; }
		}

		IEnumerable<IPSTData> IGovernmentAgencies.EPA_PSTLines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.EPA_PSTLines, PGALinesForPGACorrection.GetAllEPALines); }
		}

		ITSCAData IGovernmentAgencies.EPA_TSCAData
		{
			get { return entryLine.EPA_TSCAData; }
		}

		IEnumerable<IVNEData> IGovernmentAgencies.EPA_VNELines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.EPA_VNELines, PGALinesForPGACorrection.GetAllEPALines); }
		}

		ZString IGovernmentAgenciesIndicators.FSISDisclaimReason
		{
			get { return entryLine.FSISDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.FSISIndicator
		{
			get { return entryLine.FSISIndicator; }
		}

		IEnumerable<IFSISLine> IGovernmentAgencies.FSISLines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.FSISLines, x => x.FSISLines); }
		}

		ZString IGovernmentAgenciesIndicators.OMCDisclaimReason
		{
			get { return entryLine.OMCDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.OMCIndicator
		{
			get { return entryLine.OMCIndicator; }
		}

		IEnumerable<IOMCHeader> IGovernmentAgencies.OMCHeaders
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.OMCHeaders, x => x.OMCHeaders); }
		}

		ZString IGovernmentAgenciesIndicators.FWSDisclaimReason
		{
			get { return entryLine.FWSDisclaimReason; }
		}

		IEnumerable<IFWSHeader> IGovernmentAgencies.FWSHeaders
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.FWSHeaders, x => x.FWSHeaders); }
		}

		ZString IGovernmentAgenciesIndicators.FWSIndicator
		{
			get { return entryLine.FWSIndicator; }
		}

		IEnumerable<ILaceyActCommon> IGovernmentAgencies.LaceyActData
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.LaceyActData, PGALinesForPGACorrection.GetAllAPHISLines); }
		}

		ZString IGovernmentAgenciesIndicators.LaceyActDisclaimReason
		{
			get { return entryLine.LaceyActDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.LaceyActIndicator
		{
			get { return entryLine.LaceyActIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.NHTSADisclaimReason
		{
			get { return entryLine.NHTSADisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NHTSAIndicator
		{
			get { return entryLine.NHTSAIndicator; }
		}

		IEnumerable<INHTSAHeader> IGovernmentAgencies.NHTSALines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.NHTSALines, x => x.NHTSALines); }
		}

		ZString IGovernmentAgenciesIndicators.NMFS370DisclaimReason
		{
			get { return entryLine.NMFS370DisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NMFS370Indicator
		{
			get { return entryLine.NMFS370Indicator; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFS370Lines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.NMFS370Lines, PGALinesForPGACorrection.GetAllNMFSLines); }
		}

		ZString IGovernmentAgenciesIndicators.NMFSAMRDisclaimReason
		{
			get { return entryLine.NMFSAMRDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSAMRIndicator
		{
			get { return entryLine.NMFSAMRIndicator; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSAMRLines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.NMFSAMRLines, PGALinesForPGACorrection.GetAllNMFSLines); }
		}

		ZString IGovernmentAgenciesIndicators.NMFSHMSDisclaimReason
		{
			get { return entryLine.NMFSHMSDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSHMSIndicator
		{
			get { return entryLine.NMFSHMSIndicator; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSHMSLines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.NMFSHMSLines, PGALinesForPGACorrection.GetAllNMFSLines); }
		}

		ZString IGovernmentAgenciesIndicators.NMFSSIMIndicator
		{
			get { return entryLine.NMFSSIMIndicator; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSSIMLines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.NMFSSIMLines, PGALinesForPGACorrection.GetAllNMFSLines); }
		}

		ZString IGovernmentAgenciesIndicators.NMFSCOAIndicator
		{
			get { return entryLine.NMFSCOAIndicator; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSCOALines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.NMFSCOALines, PGALinesForPGACorrection.GetAllNMFSLines); }
		}

		ZString IGovernmentAgenciesIndicators.ODSDisclaimReason
		{
			get { return entryLine.ODSDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.ODSIndicator
		{
			get { return entryLine.ODSIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.PSTDisclaimProgram
		{
			get { return entryLine.PSTDisclaimProgram; }
		}

		ZString IGovernmentAgenciesIndicators.PSTDisclaimReason
		{
			get { return entryLine.PSTDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.PSTIndicator
		{
			get { return entryLine.PSTIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.TSCADisclaimReason
		{
			get { return entryLine.TSCADisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.TSCAIndicator
		{
			get { return entryLine.TSCAIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.TTBDisclaimReason
		{
			get { return entryLine.TTBDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.TTBIndicator
		{
			get { return entryLine.TTBIndicator; }
		}

		IEnumerable<ITTBLine> IGovernmentAgencies.TTBLines
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.TTBLines, x => x.TTBLines); }
		}

		ZString IGovernmentAgenciesIndicators.VNEDisclaimReason
		{
			get { return entryLine.VNEDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.VNEIndicator
		{
			get { return entryLine.VNEIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.DEADisclaimReason
		{
			get { return entryLine.DEADisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.DEAIndicator
		{
			get { return entryLine.DEAIndicator; }
		}

		IEnumerable<IDEAHeader> IGovernmentAgencies.DEAHeaders
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.DEAHeaders, x => x.DEAHeaders); }
		}

		ZString IGovernmentAgenciesIndicators.HFCDisclaimReason
		{
			get { return entryLine.HFCDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.HFCIndicator
		{
			get { return entryLine.HFCIndicator; }
		}

		IEnumerable<IHFCHeader> IGovernmentAgencies.EPA_HFCHeaders
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => x.EPA_HFCHeaders, PGALinesForPGACorrection.GetAllEPALines); }
		}

		IPGADataCorrection IGovernmentAgencies.TSCADataCorrection
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => new IPGADataCorrection[] { x.TSCADataCorrection }, PGALinesForPGACorrection.GetAllEPALines).FirstOrDefault(); }
		}

		IPGADataCorrection IGovernmentAgencies.ODSDataCorrection
		{
			get { return PGALinesForPGACorrection.GetPGALines(parentEntryLine, entryLine, x => new IPGADataCorrection[] { x.ODSDataCorrection }, PGALinesForPGACorrection.GetAllEPALines).FirstOrDefault(); }
		}

		#endregion

		#region OGAs that do not implement IPGADataCorrection

		IList<IPriorNoticeLine> IOGA.FDA
		{
			get { return entryLine.FDA.Cast<IPriorNoticeLine>().ToList(); }
		}

		ZString IOGA.FDAIndicator
		{
			get { return entryLine.FDAIndicator; }
		}

		IEnumerable<IDOT> IOGA.DOT
		{
			get { return entryLine.DOT.Cast<IDOT>(); }
		}

		ZString IOGA.DOTIndicator
		{
			get { return entryLine.DOTIndicator; }
		}

		#endregion

		#region IPGALineNumbers Members

		ZInt IPGALineNumbers.EPAStartLineNumber
		{
			get { return entryLine.EPAStartLineNumber; }
			set { entryLine.EPAStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.FSISStartLineNumber
		{
			get { return entryLine.FSISStartLineNumber; }
			set { entryLine.FSISStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.NMFSStartLineNumber
		{
			get { return entryLine.NMFSStartLineNumber; }
			set { entryLine.NMFSStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.FDAStartLineNumber
		{
			get { return entryLine.FDAStartLineNumber; }
			set { entryLine.FDAStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.TTBStartLineNumber
		{
			get { return entryLine.TTBStartLineNumber; }
			set { entryLine.TTBStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.NHTSAStartLineNumber
		{
			get { return entryLine.NHTSAStartLineNumber; }
			set { entryLine.NHTSAStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.AMSStartLineNumber
		{
			get { return entryLine.AMSStartLineNumber; }
			set { entryLine.AMSStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.APHStartLineNumber
		{
			get { return entryLine.APHStartLineNumber; }
			set { entryLine.APHStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.FWSStartLineNumber
		{
			get { return entryLine.FWSStartLineNumber; }
			set { entryLine.FWSStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.ATFStartLineNumber
		{
			get { return entryLine.ATFStartLineNumber; }
			set { entryLine.ATFStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.CPSCStartLineNumber
		{
			get { return entryLine.CPSCStartLineNumber; }
			set { entryLine.CPSCStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.OMCStartLineNumber
		{
			get { return entryLine.OMCStartLineNumber; }
			set { entryLine.OMCStartLineNumber = value; }
		}

		ZInt IPGALineNumbers.DEAStartLineNumber
		{
			get { return entryLine.DEAStartLineNumber; }
			set { entryLine.DEAStartLineNumber = value; }
		}

		void IPGALineNumbers.ClearPGALineNumbers()
		{
			throw new NotSupportedException();
		}

		#endregion

		public bool HasLinesToSend()
		{
			var iThis = (IPGAGovernmentAgenciesCommon)this;
			return
				iThis.AMSLines.Any() ||
				iThis.APHISHeaders.Any() ||
				iThis.ATFLines.Any() ||
				iThis.OMCHeaders.Any() ||
				iThis.EPA_PSTLines.Any() ||
				iThis.EPA_VNELines.Any() ||
				iThis.EPA_HFCHeaders.Any() ||
				iThis.FDALines.Any() ||
				iThis.FSISLines.Any() ||
				iThis.FWSHeaders.Any() ||
				iThis.LaceyActData.Any() ||
				iThis.NHTSALines.Any() ||
				iThis.NMFS370Lines.Any() ||
				iThis.NMFSAMRLines.Any() ||
				iThis.NMFSSIMLines.Any() ||
				iThis.NMFSCOALines.Any() ||
				iThis.CPSCHeaders.Any() ||
				iThis.NMFSHMSLines.Any() ||
				iThis.TTBLines.Any() ||
				iThis.DEAHeaders.Any() ||
				iThis.DDTCData != null ||
				iThis.TSCADataCorrection != null ||
				iThis.ODSDataCorrection != null;
		}
	}

	public static class CusEntryHeaderPGACorrectionExtensionMethods
	{
		public static IEnumerable<IACECusEntryLine> GetEntryLinesWithPGAToSend(this IACECusEntryHeader entryHeader)
		{
			var orderedEntryLines = entryHeader.EntryLines.OrderBy(x => x.CL_LineNumber);

			foreach (IACECusEntryLine entryLine in orderedEntryLines)
			{
				var hasParentLineFound = false;
				var lineToSend = new PGAGovernmentAgenciesWithPGACorrection(entryLine, (IPGAGovernmentAgenciesCommon)entryLine);

				if (lineToSend.HasLinesToSend())
				{
					yield return entryLine;
					hasParentLineFound = true;
				}

				if (!hasParentLineFound)
				{
					foreach (ISecondaryTariffLine secondaryLine in entryLine.SecondaryTariffLines)
					{
						lineToSend = new PGAGovernmentAgenciesWithPGACorrection(entryLine, secondaryLine);

						if (lineToSend.HasLinesToSend())
						{
							yield return entryLine;
							break;
						}
					}
				}
			}
		}
	}

	public static class IPGAGovernmentAgenciesCommonExtensionMethods
	{
		public static ZBool HasODSOrTSCALinesToBeSent(this IPGAGovernmentAgenciesCommon entryLine)
		{
			return (OGAIndicatorList.IsToBeDeclared(entryLine.ODSIndicator) && entryLine.ODSDataCorrection != null) || (OGAIndicatorList.IsToBeDeclared(entryLine.TSCAIndicator) && entryLine.TSCADataCorrection != null);
		}

		public static ZBool HasNHTSALinesToBeSent(this IPGAGovernmentAgenciesCommon entryLine)
		{
			return OGAIndicatorList.IsToBeDeclared(entryLine.NHTSAIndicator) && entryLine.NHTSALines.Any();
		}

		public static ZBool HasPGARequiringDeclarationDate(this IPGAGovernmentAgenciesCommon entryLine)
		{
			return (OGAIndicatorList.IsToBeDeclared(entryLine.VNEIndicator) && entryLine.EPA_VNELines.Any())
				|| (OGAIndicatorList.IsToBeDeclared(entryLine.NHTSAIndicator) && entryLine.NHTSALines.Any())
				|| (OGAIndicatorList.IsToBeDeclared(entryLine.PSTIndicator) && entryLine.EPA_PSTLines.Any())
				|| (OGAIndicatorList.IsToBeDeclared(entryLine.FSISIndicator) && entryLine.FSISLines.Any())
				|| (OGAIndicatorList.IsToBeDeclared(entryLine.LaceyActIndicator) && entryLine.LaceyActData.Any())
				|| (OGAIndicatorList.IsToBeDeclared(entryLine.FWSIndicator) && entryLine.FWSHeaders.Any());
		}
	}
}
