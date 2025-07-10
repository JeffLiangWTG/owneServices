using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	[DebuggerDisplay("Tariff = {Tariff}")]
	class SecondaryTariffLineWrapper : ISecondaryTariffLine, IDutyData
	{
		public SecondaryTariffLineWrapper(CusEntryLine entryLine)
		{
			if (entryLine == null)
			{
				throw new ArgumentNullException(nameof(entryLine));
			}
			this.entryLine = entryLine;
			this.dutyData = entryLine;
		}
		readonly CusEntryLine entryLine;
		readonly IDutyData dutyData;

		#region ISecondaryTariffLine Members

		public ZString Tariff
		{
			get { return dutyData.Tariff; }
		}

		public bool IsCombineSecondaryTariffLine
		{
			get { return dutyData.IsCombineSecondaryTariffLine; }
		}

		public IEnumerable<IDutyData> CombineAllLines
		{
			get { return dutyData.CombineAllLines; }
		}

		public ZString SpecialProgramsIndicatorSecondary
		{
			get { return dutyData.SpecialProgramsIndicatorSecondary; }
		}

		public ZDecimal Duty
		{
			get { return entryLine.DutyAmount; }
		}

		public ZDecimal Quantity1
		{
			get { return RoundedQuantity.GetRoundedQuantity1(entryLine, dutyData.Quantity1); }
		}

		public bool IsCombineLine
		{
			get { return dutyData.IsCombinedLine(); }
		}

		public bool IsDisclaimSanction
		{
			get { return ((ICusEntryLine)entryLine).IsDisclaimSanction; }
		}

		public ZString UQ1
		{
			get { return dutyData.UQ1; }
		}

		public ZDecimal Quantity2
		{
			get { return RoundedQuantity.GetRoundedQuantity2(entryLine, dutyData.Quantity2); }
		}

		public ZString UQ2
		{
			get { return dutyData.UQ2; }
		}

		public ZDecimal Quantity3
		{
			get { return RoundedQuantity.GetRoundedQuantity3(entryLine, dutyData.Quantity3); }
		}

		public ZString UQ3
		{
			get { return dutyData.UQ3; }
		}

		public ZDecimal ValueInUSD
		{
			get
			{
				var result = dutyData.CustomsValue;
				var invoiceLine = entryLine.RandomLine;
				if (CalculateDutyForSetsHelper.IsCombinedXLine(invoiceLine))
				{
					if (dutyData.IsSecondaryTariffLine && !entryLine.US_SupLine)
					{
						result = CalculateDutyForSetsHelper.GetSetsCustomsValueForXVLine(new EntryLineIEntryLineOrInvoiceLineDutyData(entryLine));
					}
				}
				else if (entryLine.IsChildLine && dutyData.IsSetVLine && CalculateDutyForSetsHelper.Is9903Tariff(invoiceLine.US_SupTariff) && !CalculateDutyForSetsHelper.Is9903Tariff(dutyData.Tariff))
				{
					var parentLine = entryLine.ParentLine;
					if (parentLine != null)
					{
						result = ((IDutyData)parentLine).CustomsValue;
					}
				}
				return result;
			}
		}

		public ZString SpecialProgramsIndicatorPrimaryOrCountry
		{
			get
			{
				ZString result = SpecialProgramsIndicatorPrimary;

				if (result.IsEmpty)
				{
					result = SpecialProgramsIndicatorCountry;
				}

				return result;
			}
		}

		public ZDecimal GrossWeightInKilograms
		{
			get
			{
				return ((ICusEntryLine)entryLine).GrossWeightInKilograms;
			}
		}

		#endregion

		#region IDutyData Members

		public BusinessObjectFactory Factory
		{
			get { return entryLine.Factory; }
		}

		public ZString SpecialProgramsIndicatorCountry
		{
			get { return dutyData.SpecialProgramsIndicatorCountry; }
		}

		public ZString SpecialProgramsIndicatorPrimary
		{
			get { return dutyData.SpecialProgramsIndicatorPrimary; }
		}

		public ZString CountryOfOrigin
		{
			get { return ZString.Empty; }
		}

		public ZDate DateForDutyCalculation
		{
			get { return dutyData.DateForDutyCalculation; }
		}

		public ZDecimal CustomsValue
		{
			get { return ValueInUSD; }
		}

		public ZDecimal SupCustomsValue
		{
			get { return dutyData.SupCustomsValue; }
		}

		public ZString SelectedRateType
		{
			get { return dutyData.SelectedRateType; }
		}

		public ZString EntryType
		{
			get { return dutyData.EntryType; }
		}

		public ZBool ShouldIncludePGAInMessage(ZBool isCertified, ZString pgaCode)
		{
			return ((IGovernmentAgencies)entryLine).ShouldIncludePGAInMessage(isCertified, pgaCode);
		}

		public bool IsClearedInPR
		{
			get { return dutyData.IsClearedInPR; }
		}

		public bool IsAMSFeeExempt
		{
			get { return dutyData.IsAMSFeeExempt; }
		}

		public bool IsRaspberryFeeExempt
		{
			get { return dutyData.IsRaspberryFeeExempt; }
		}

		public ZBool IsSetXLine
		{
			get { return dutyData.IsSetXLine; }
		}

		public ZBool IsSetVLine
		{
			get { return dutyData.IsSetVLine; }
		}

		public bool IsCottonFeeExemptIndicated
		{
			get { return dutyData.IsCottonFeeExemptIndicated; }
		}

		public bool HasCottonCertificate
		{
			get { return dutyData.HasCottonCertificate; }
		}

		public IDutyData ParentTariffLine
		{
			get { return dutyData.ParentTariffLine; }
		}

		public bool IsSecondaryTariffLine
		{
			get { return dutyData.IsSecondaryTariffLine; }
		}

		public bool IsDomesticMerchandise
		{
			get { return dutyData.IsDomesticMerchandise; }
		}

		public USCTariff ImportTariff
		{
			get { return dutyData.ImportTariff; }
		}

		ZDecimal IDutyData.ValueForADD
		{
			get { return dutyData.ValueForADD; }
		}

		ZDecimal IDutyData.ADDDepositRate
		{
			get { return dutyData.ADDDepositRate; }
		}

		ZDecimal IDutyData.ValueForCVD
		{
			get { return dutyData.ValueForCVD; }
		}

		ZDecimal IDutyData.CVDDepositRate
		{
			get { return dutyData.CVDDepositRate; }
		}

		ZDecimal IDutyData.ADDQuantity
		{
			get { return dutyData.ADDQuantity; }
		}

		ZString IDutyData.ADDCaseRateTypeQualifier
		{
			get { return dutyData.ADDCaseRateTypeQualifier; }
		}

		ZDecimal IDutyData.CVDQuantity
		{
			get { return dutyData.CVDQuantity; }
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get { return dutyData.ADDutyManual; }
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get { return dutyData.CVDutyManual; }
		}

		ZString IDutyData.CVDCaseRateTypeQualifier
		{
			get { return dutyData.CVDCaseRateTypeQualifier; }
		}

		bool IDutyData.HasTextileCategoryNo
		{
			get { return dutyData.HasTextileCategoryNo; }
		}

		bool IDutyData.IsCombineSecondaryTariffLine
		{
			get { return dutyData.IsCombineSecondaryTariffLine; }
		}

		IEnumerable<IDutyData> IDutyData.CombineChildLines
		{
			get { return dutyData.CombineChildLines; }
		}

		IDutyData IDutyData.CombineParentLine
		{
			get { return dutyData.CombineParentLine; }
		}

		IReadOnlyList<ZString> IDutyData.SupTariffs
		{
			get { return dutyData.SupTariffs; }
		}

		#endregion

		#region IOGA Members

		ZString IGovernmentAgencies.CommercialDescription
		{
			get { return ((IGovernmentAgencies)entryLine).CommercialDescription; }
		}

		IList<IPriorNoticeLine> IOGA.FDA
		{
			get { return ((IOGA)entryLine).FDA; }
		}

		ZString IOGA.FDAIndicator
		{
			get { return ((IOGA)entryLine).FDAIndicator; }
		}

		IEnumerable<IDOT> IOGA.DOT
		{
			get { return ((IOGA)entryLine).DOT; }
		}

		ZString IOGA.DOTIndicator
		{
			get { return ((IOGA)entryLine).DOTIndicator; }
		}

		#endregion

		#region IGovernmentAgencies Members

		ZString IPGAGovernmentAgenciesCommon.Tariff
		{
			get { return dutyData.Tariff; }
		}

		ZInt IPGAGovernmentAgenciesCommon.LineNumber
		{
			get { return entryLine.CL_LineNumber; }
		}

		IEnumerable<ILaceyActCommon> IGovernmentAgencies.LaceyActData
		{
			get { return ((IGovernmentAgencies)entryLine).LaceyActData; }
		}

		ZString IGovernmentAgenciesIndicators.LaceyActIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).LaceyActIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.LaceyActDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).LaceyActDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.OMCIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).OMCIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.OMCDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).OMCDisclaimReason; }
		}

		IEnumerable<IOMCHeader> IGovernmentAgencies.OMCHeaders
		{
			get { return ((IGovernmentAgencies)entryLine).OMCHeaders; }
		}

		ZString IGovernmentAgenciesIndicators.ODSIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).ODSIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.ODSDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).ODSDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.PSTIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).PSTIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.PSTDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).PSTDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.PSTDisclaimProgram
		{
			get { return ((IGovernmentAgencies)entryLine).PSTDisclaimProgram; }
		}

		ZString IGovernmentAgenciesIndicators.VNEIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).VNEIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.VNEDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).VNEDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.FSISIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).FSISIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.FSISDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).FSISDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NMFS370Indicator
		{
			get { return ((IGovernmentAgencies)entryLine).NMFS370Indicator; }
		}

		ZString IGovernmentAgenciesIndicators.NMFS370DisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).NMFS370DisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSAMRIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).NMFSAMRIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSAMRDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).NMFSAMRDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSHMSIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).NMFSHMSIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSHMSDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).NMFSHMSDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSSIMIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).NMFSSIMIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSCOAIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).NMFSCOAIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.ACEFDAIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).ACEFDAIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.ACEFDADisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).ACEFDADisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.TSCAIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).TSCAIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.TSCADisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).TSCADisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.ATFIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).ATFIndicator; }
		}

		public IEnumerable<IATFData> ATFLines
		{
			get
			{
				return ((IGovernmentAgencies)entryLine).ATFLines;
			}
		}

		ZString IGovernmentAgenciesIndicators.NHTSAIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).NHTSAIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.NHTSADisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).NHTSADisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.DDTCIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).DDTCIndicator; }
		}

		IDDTCData IGovernmentAgencies.DDTCData
		{
			get
			{
				return ((IGovernmentAgencies)entryLine).DDTCData;
			}
		}

		IEnumerable<IVNEData> IGovernmentAgencies.EPA_VNELines
		{
			get
			{
				return ((IGovernmentAgencies)entryLine).EPA_VNELines;
			}
		}

		public IEnumerable<IPSTData> EPA_PSTLines
		{
			get
			{
				return ((IGovernmentAgencies)entryLine).EPA_PSTLines;
			}
		}

		ITSCAData IGovernmentAgencies.EPA_TSCAData
		{
			get { return ((IGovernmentAgencies)entryLine).EPA_TSCAData; }
		}

		IPGADataCorrection IGovernmentAgencies.TSCADataCorrection
		{
			get { return ((IGovernmentAgencies)entryLine).TSCADataCorrection; }
		}

		IPGADataCorrection IGovernmentAgencies.ODSDataCorrection
		{
			get { return ((IGovernmentAgencies)entryLine).ODSDataCorrection; }
		}

		IEnumerable<IFSISLine> IGovernmentAgencies.FSISLines
		{
			get { return ((IGovernmentAgencies)entryLine).FSISLines; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFS370Lines
		{
			get { return ((IGovernmentAgencies)entryLine).NMFS370Lines; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSAMRLines
		{
			get { return ((IGovernmentAgencies)entryLine).NMFSAMRLines; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSHMSLines
		{
			get { return ((IGovernmentAgencies)entryLine).NMFSHMSLines; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSSIMLines
		{
			get { return ((IGovernmentAgencies)entryLine).NMFSSIMLines; }
		}

		IEnumerable<INMFSLine> IGovernmentAgencies.NMFSCOALines
		{
			get { return ((IGovernmentAgencies)entryLine).NMFSCOALines; }
		}

		IEnumerable<IFDAData> IGovernmentAgencies.FDALines
		{
			get { return ((IGovernmentAgencies)entryLine).FDALines; }
		}

		IEnumerable<INHTSAHeader> IGovernmentAgencies.NHTSALines
		{
			get { return ((IGovernmentAgencies)entryLine).NHTSALines; }
		}

		ZString IGovernmentAgenciesIndicators.TTBIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).TTBIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.TTBDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).TTBDisclaimReason; }
		}

		IEnumerable<ITTBLine> IGovernmentAgencies.TTBLines
		{
			get { return ((IGovernmentAgencies)entryLine).TTBLines; }
		}

		ZString IGovernmentAgenciesIndicators.APHISIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).APHISIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.APHISDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).APHISDisclaimReason; }
		}

		IEnumerable<IAPHISHeader> IGovernmentAgencies.APHISHeaders
		{
			get { return ((IGovernmentAgencies)entryLine).APHISHeaders; }
		}

		ZString IGovernmentAgenciesIndicators.FWSIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).FWSIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.FWSDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).FWSDisclaimReason; }
		}

		IEnumerable<IFWSHeader> IGovernmentAgencies.FWSHeaders
		{
			get { return ((IGovernmentAgencies)entryLine).FWSHeaders; }
		}

		ZString IGovernmentAgenciesIndicators.AMSIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).AMSIndicator; }
		}

		IEnumerable<IAMSData> IGovernmentAgencies.AMSLines
		{
			get { return ((IGovernmentAgencies)entryLine).AMSLines; }
		}

		ZString IGovernmentAgenciesIndicators.AMSDisclaimProgram
		{
			get { return ((IGovernmentAgencies)entryLine).AMSDisclaimProgram; }
		}

		ZString IGovernmentAgenciesIndicators.AMSDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).AMSDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NOPIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).NOPIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.NOPDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).NOPDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.CPSCIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).CPSCIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.CPSCDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).CPSCDisclaimReason; }
		}

		IEnumerable<ICPSCHeader> IGovernmentAgencies.CPSCHeaders
		{
			get { return ((IGovernmentAgencies)entryLine).CPSCHeaders; }
		}

		ZString IGovernmentAgenciesIndicators.DEAIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).DEAIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.DEADisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).DEADisclaimReason; }
		}

		IEnumerable<IDEAHeader> IGovernmentAgencies.DEAHeaders
		{
			get { return ((IGovernmentAgencies)entryLine).DEAHeaders; }
		}

		ZString IGovernmentAgenciesIndicators.HFCIndicator
		{
			get { return ((IGovernmentAgencies)entryLine).HFCIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.HFCDisclaimReason
		{
			get { return ((IGovernmentAgencies)entryLine).HFCDisclaimReason; }
		}

		IEnumerable<IHFCHeader> IGovernmentAgencies.EPA_HFCHeaders
		{
			get { return ((IGovernmentAgencies)entryLine).EPA_HFCHeaders; }
		}

		#endregion

		#region IPGALineNumbers Members

		IPGALineNumbers ParentEntryLineStartNumbers
		{
			get { return entryLine.ParentLine; }
		}

		ZInt IPGALineNumbers.EPAStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.EPAStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.EPAStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.FSISStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.FSISStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.FSISStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.NMFSStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.NMFSStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.NMFSStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.FDAStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.FDAStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.FDAStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.TTBStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.TTBStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.TTBStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.NHTSAStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.NHTSAStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.NHTSAStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.AMSStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.AMSStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.AMSStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.APHStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.APHStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.APHStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.FWSStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.FWSStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.FWSStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.ATFStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.ATFStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.ATFStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.CPSCStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.CPSCStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.CPSCStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.OMCStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.OMCStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.OMCStartLineNumber = value;
				}
			}
		}

		ZInt IPGALineNumbers.DEAStartLineNumber
		{
			get { return ParentEntryLineStartNumbers != null ? ParentEntryLineStartNumbers.DEAStartLineNumber : ZInt.Zero; }
			set
			{
				if (ParentEntryLineStartNumbers != null)
				{
					ParentEntryLineStartNumbers.DEAStartLineNumber = value;
				}
			}
		}

		void IPGALineNumbers.ClearPGALineNumbers()
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
