using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class DummyICusEntryLine : ICusEntryLine, ISecondaryTariffLine, IEntryLineOrInvoiceLineDutyData, IFeeCalculationDataProvider, ITSCAData
	{
		public DummyICusEntryLine(BusinessObjectFactory factory, DummyICusEntryHeader header)
		{
			this.Factory = factory;
			this.header = header;
		}

		public readonly BusinessObjectFactory Factory;
		readonly DummyICusEntryHeader header;

		public DummyICusEntryLine ParentLine { get; set; }

		public ZString ImportTariffCode { get; set; }

		public USCTariff ImportTariff
		{
			get
			{
				if (importTariff == null || importTariff.UE_Tariff != ImportTariffCode)
				{
					importTariff = new USCTariff.Loader(Factory).LoadBestMatch(ImportTariffCode, ZDateTime.Today);
				}
				return importTariff;
			}
		}
		USCTariff importTariff;

		public ZString CountryOfOrigin { get; set; }

		public bool HasMPF { get; set; }

		public ZString CountryOfExport { get; set; }

		public ZString WoolLicense { get; set; }

		public ZString MiscellaneousPermitLicenseNumber { get; set; }

		public ZString VisaNumber { get; set; }

		public ZString CBTPACertificationNumber { get; set; }

		public ZDecimal Value { get; set; }

		public List<DummyFee> Fees
		{
			get { return fees ?? (fees = new List<DummyFee>()); }
		}
		List<DummyFee> fees;

		public List<DummyICusEntryLine> SecondaryLines
		{
			get { return secondaryLines ?? (secondaryLines = new List<DummyICusEntryLine>()); }
		}
		List<DummyICusEntryLine> secondaryLines;

		public ZString TextileCategoryNumber
		{
			get;
			set;
		}

		public bool IsCombineLine { get; set; }

		public bool IsDisclaimSanction { get; set; }

		#region ICusEntryLine Members

		ZBool ICusEntryLine.IsSupLine
		{
			get { return ZBool.False; }
		}

		ICusEntryLine ICusEntryLine.ParentLine
		{
			get { return ParentLine; }
		}

		public ZString ExportTariffCode
		{
			get { return ZString.Empty; }
		}

		public ZString NAFTATariff
		{
			get { return ZString.Empty; }
		}

		public ZDecimal NAFTADutyRate
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal NAFTADutyFGN
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal NAFTADutyUS
		{
			get { return ZDecimal.Zero; }
		}

		public ZString ImportFTZNumber
		{
			get { return ZString.Empty; }
		}

		public ZDecimal GrossWeightInKilograms
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal ADDSpecificDepositValue
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal CVDSpecificDepositValue
		{
			get { return ZDecimal.Zero; }
		}

		public ZString ADDCaseRateTypeQualifier { get { return ZString.Empty; } }
		public ZDecimal ADDQuantity { get { return ZDecimal.Zero; } }

		public ZString CVDCaseRateTypeQualifier { get { return ZString.Empty; } }
		public ZDecimal CVDQuantity { get { return ZDecimal.Zero; } }

		public ZDecimal Charges
		{
			get { return 50m; }
		}

		public ZString PortOfLading
		{
			get { return "60267"; }
		}

		public ZString ZoneStatus
		{
			get { return ZString.Empty; }
		}

		public ZDate PrivilegedStatusFilingDate
		{
			get { return ZDate.Empty; }
		}

		public ZInt FTZLineItemQuantity
		{
			get { return ZInt.Zero; }
		}

		public ZBool NAFTANetCostIndicator
		{
			get { return ZBool.False; }
		}

		public ZShort InvDelimter
		{
			get { return ZShort.Zero; }
		}

		public ZString PreImportationReviewProgramRulingsType
		{
			get { return ZString.Empty; }
		}

		public ZString PreImportationReviewProgramRulingsNumber
		{
			get { return ZString.Empty; }
		}

		public IEnumerable<ZString> CommercialDescriptions
		{
			get { yield return "TEST"; }
		}

		public ZString SpecialProgramsIndicatorPrimary
		{
			get { return ZString.Empty; }
		}

		public ZDecimal Quantity1
		{
			get { return ZDecimal.Zero; }
		}

		public ZString UnitOfMeasure1
		{
			get { return ImportTariff != null ? ImportTariff.UE_Unit1 : ZString.Empty; }
		}

		public ZDecimal Quantity2
		{
			get { return ZDecimal.Zero; }
		}

		public ZBool PGAExpeditedReleaseIndicator
		{
			get { return ZBool.False; }
		}

		public ZString UnitOfMeasure2
		{
			get { return ImportTariff != null ? ImportTariff.UE_Unit2 : ZString.Empty; }
		}

		public ZDecimal Quantity3
		{
			get { return ZDecimal.Zero; }
		}

		public ZString UnitOfMeasure3
		{
			get { return ImportTariff != null ? ImportTariff.UE_Unit3 : ZString.Empty; }
		}

		public ZDate DateOfExportation
		{
			get { return ZDateTime.Today.AddDays(-7).Date; }
		}

		public ZBool RelatedPartyIndicator
		{
			get { return ZBool.False; }
		}

		public ZString SpecialProgramsIndicatorCountry
		{
			get { return ZString.Empty; }
		}

		public ZString SpecialProgramsIndicatorSecondary
		{
			get;
			set;
		}

		public ZDate DateOfExportationFromCountryOfOrigin
		{
			get { return ZDate.Empty; }
		}

		public ZDecimal VisaQuantity
		{
			get { return ZDecimal.Zero; }
		}

		public ZString VisaUnitOfMeasure
		{
			get { return ZString.Empty; }
		}

		public ZString AgricultureLicenseNumber
		{
			get { return ZString.Empty; }
		}

		public ZString CottonCertificateNumberOrganicExemptionCertificateNumber
		{
			get { return ZString.Empty; }
		}

		public ZString ChinaHongKongSWPMIndicator
		{
			get { return ZString.Empty; }
		}

		public ZString CanadianExportCertificateSugar
		{
			get { return ZString.Empty; }
		}

		public ZBool IsSoftwoodLumberLine
		{
			get { return ZBool.False; }
		}

		public ZBool IsSoftwoodLumberImporterDeclaration
		{
			get { return ZBool.False; }
		}

		public ZDecimal SoftwoodLumberExportPrice
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal SoftwoodLumberExportCharges
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal CountervailingDuty
		{
			get { return ZDecimal.Zero; }
		}

		public ZString CountervailingCaseNumber
		{
			get { return ZString.Empty; }
		}

		public ZString AntidumpingCaseNumber
		{
			get { return ZString.Empty; }
		}

		public ZDecimal AntidumpingDuty
		{
			get { return ZDecimal.Zero; }
		}

		public ZString ManufacturerSupplierCode
		{
			get { return "ZABHPBIL1001RAN"; }
		}

		public ZString GetRelevantTariffForFee(string feeCode)
		{
			return Tariff;
		}

		public ZDecimal ExciseTax
		{
			get
			{
				ZDecimal result = 0;

				foreach (IFee fee in Fees)
				{
					if (CusFeeCodeConstants.IsExciseTax(fee.Code))
					{
						result += fee.Amount;
					}
				}

				return result;
			}
		}

		public ZDecimal CVDDepositRate
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal ADDDepositRate
		{
			get { return ZDecimal.Zero; }
		}

		public ZBool BondedCountervailingDuty
		{
			get { return ZBool.False; }
		}

		public ZBool BondedAntidumpingDuty
		{
			get { return ZBool.False; }
		}

		IEnumerable<IFee> ICusEntryLine.Fees
		{
			get
			{
				foreach (IFee fee in Fees)
				{
					if (!CusFeeCodeConstants.IsExciseTax(fee.Code))
					{
						yield return fee;
					}
				}
			}
		}

		public IEnumerable<ISecondaryTariffLine> SecondaryTariffLines
		{
			get { return new TypedEnumerable<ISecondaryTariffLine>(SecondaryLines); }
		}

		public ZDecimal SecondCustomsQuantity
		{
			get { return ZDecimal.Zero; }
		}

		public ZString SecondCustomsUnitQty
		{
			get { return ImportTariff != null ? ImportTariff.UE_Unit2 : ZString.Empty; }
		}

		public ZDecimal ThirdCustomsQuantity
		{
			get { return ZDecimal.Zero; }
		}

		public ZString ThirdCustomsUnitQty
		{
			get { return ImportTariff != null ? ImportTariff.UE_Unit3 : ZString.Empty; }
		}

		public ZDate DateForDutyCalculation
		{
			get { return ZDate.Today; }
		}

		public ZString SelectedRateType
		{
			get { return ZString.Empty; }
		}

		public ZString FTZCurrentTariff
		{
			get { return ZString.Empty; }
		}

		ZBool ICusEntryLine.IsDisclaimSanction
		{
			get { return false; }
		}

		public IEnumerable<ISanctionsAdditionalInfo> SanctionsAdditionalInfos
		{
			get { return Enumerable.Empty<ISanctionsAdditionalInfo>(); }
		}

		#endregion

		#region ICusEntryLine Members

		public ZDecimal BondedWarehouseQuantity
		{
			get { return ZDecimal.Zero; }
		}

		public ZString BondedWarehouseUnitQuantity
		{
			get { return ZString.Empty; }
		}

		public ZDecimal CL_CustomsValue
		{
			get { return Value; }
		}

		public ZDecimal CL_DutyPercent
		{
			get { return ZDecimal.Zero; }
		}

		public ZShort CL_LineNumber
		{
			get;
			set;
		}

		public ZString CL_ParentTrailer
		{
			get { return ZString.Empty; }
		}

		public ZDecimal CL_WarehouseUnitValue
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal CustomsQuantity
		{
			get { return ZDecimal.Zero; }
		}

		public ZString CustomsUnitQty
		{
			get { return ImportTariff != null ? ImportTariff.UE_Unit1 : ZString.Empty; }
		}

		public Money CustomsValue
		{
			get { return new Money(CL_CustomsValue, JobDeclaration.GetLocalCurrency()); }
		}

		public ZString Description
		{
			get { return "TEST"; }
		}

		public ZDecimal DutyAmount
		{
			get;
			set;
		}

		public ZString DutyRateDescription
		{
			get { return ZString.Empty; }
		}

		public ZString ExtendedCommercialDescription
		{
			get { return ZString.Empty; }
		}

		public ZString FormattedTariff
		{
			get { return new TariffFormatter().DisplayFormat(ImportTariffCode); }
		}

		public ZDecimal GSTVATAmount
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal GSTVATDeferred
		{
			get { return ZDecimal.Zero; }
		}

		public Customs.Business.InvoiceLinesForEntryLineCollection InvoiceLines
		{
			get { return null; }
		}

		public Customs.Business.BaseJobComInvoiceLine RandomLine
		{
			get { return null; }
		}

		public ZString Tariff
		{
			get { return ImportTariffCode; }
		}

		public Money TotalLinePrice
		{
			get { return Money.Empty; }
		}

		public ZDecimal TotalLinePriceInLocalCurrency
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region IOGA Members

		ZString IGovernmentAgencies.CommercialDescription
		{
			get { return ZString.Empty; }
		}

		public IList<IFCC> FCC
		{
			get { return System.Array.Empty<IFCC>(); }
		}

		public ZString FCCIndicator
		{
			get { return ZString.Empty; }
		}

		public IList<IPriorNoticeLine> FDA
		{
			get { return System.Array.Empty<IPriorNoticeLine>(); }
		}

		public ZString FDAIndicator
		{
			get { return ZString.Empty; }
		}

		public IEnumerable<IDOT> DOT
		{
			get { return System.Array.Empty<IDOT>(); }
		}

		public ZString DOTIndicator
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IGovernmentAgencies Members

		ZInt IPGAGovernmentAgenciesCommon.LineNumber
		{
			get { return CL_LineNumber; }
		}

		public IEnumerable<ILaceyActCommon> LaceyActData
		{
			get { return System.Array.Empty<ILaceyActCommon>(); }
		}

		ZString IGovernmentAgenciesIndicators.LaceyActIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.LaceyActDisclaimReason
		{
			get { return ZString.Empty; }
		}

		public IEnumerable<IOMCHeader> OMCHeaders
		{
			get { return System.Array.Empty<IOMCHeader>(); }
		}

		ZString IGovernmentAgenciesIndicators.OMCIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.OMCDisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.ODSIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.PSTIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.VNEIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.FSISIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.FSISDisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.ODSDisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.PSTDisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.PSTDisclaimProgram
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.TSCADisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.TSCAIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.VNEDisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFS370Indicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFS370DisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSAMRIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSAMRDisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSHMSIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSHMSDisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSSIMIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSCOAIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.ACEFDADisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.CPSCIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.CPSCDisclaimReason
		{
			get { return ZString.Empty; }
		}

		IEnumerable<ICPSCHeader> IGovernmentAgencies.CPSCHeaders
		{
			get { return Enumerable.Empty<ICPSCHeader>(); }
		}

		ZString IGovernmentAgenciesIndicators.APHISIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.APHISDisclaimReason
		{
			get { return ZString.Empty; }
		}

		IEnumerable<IAPHISHeader> IGovernmentAgencies.APHISHeaders
		{
			get { return Enumerable.Empty<IAPHISHeader>(); }
		}

		ZString IGovernmentAgenciesIndicators.FWSIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.FWSDisclaimReason
		{
			get { return ZString.Empty; }
		}

		IEnumerable<IFWSHeader> IGovernmentAgencies.FWSHeaders
		{
			get { return Enumerable.Empty<IFWSHeader>(); }
		}
		IDDTCData IGovernmentAgencies.DDTCData
		{
			get { return null; }
		}

		IEnumerable<IVNEData> IGovernmentAgencies.EPA_VNELines
		{
			get { return System.Array.Empty<IVNEData>(); }
		}

		IEnumerable<IFSISLine> IGovernmentAgencies.FSISLines
		{
			get { return Enumerable.Empty<IFSISLine>(); }
		}

		public IEnumerable<IPSTData> EPA_PSTLines
		{
			get { return System.Array.Empty<IPSTData>(); }
		}

		public IEnumerable<INMFSLine> NMFS370Lines
		{
			get { return System.Array.Empty<INMFSLine>(); }
		}

		public IEnumerable<INMFSLine> NMFSAMRLines
		{
			get { return System.Array.Empty<INMFSLine>(); }
		}

		public IEnumerable<INMFSLine> NMFSHMSLines
		{
			get { return System.Array.Empty<INMFSLine>(); }
		}

		public IEnumerable<INMFSLine> NMFSSIMLines
		{
			get { return System.Array.Empty<INMFSLine>(); }
		}

		public IEnumerable<INMFSLine> NMFSCOALines
		{
			get { return System.Array.Empty<INMFSLine>(); }
		}

		public IEnumerable<IFDAData> FDALines
		{
			get { return System.Array.Empty<IFDAData>(); }
		}

		ZString IGovernmentAgenciesIndicators.ACEFDAIndicator
		{
			get { return ZString.Empty; }
		}

		ITSCAData IGovernmentAgencies.EPA_TSCAData
		{
			get { return this; }
		}

		IPGADataCorrection IGovernmentAgencies.ODSDataCorrection
		{
			get { return null; }
		}

		IPGADataCorrection IGovernmentAgencies.TSCADataCorrection
		{
			get { return null; }
		}

		ZString IGovernmentAgenciesIndicators.TTBIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.TTBDisclaimReason
		{
			get { return ZString.Empty; }
		}

		IEnumerable<ITTBLine> IGovernmentAgencies.TTBLines
		{
			get { return Enumerable.Empty<ITTBLine>(); }
		}

		ZString ITSCAData.TSCACertificationCode
		{
			get { return ZString.Empty; }
		}

		ZString ITSCAData.ContactName
		{
			get { return ZString.Empty; }
		}

		ZString ITSCAData.ContactPhone
		{
			get { return ZString.Empty; }
		}

		ZString ITSCAData.ContactEmail
		{
			get { return ZString.Empty; }
		}
		ZDate ITSCAData.CertifySignatureDate
		{
			get { return ZDate.Empty; }
			set { }
		}

		ZInt ITSCAData.TSCALineNumber
		{
			get { return ZInt.Zero; }
			set { }
		}

		ZInt ITSCAData.ODSLineNumber
		{
			get { return ZInt.Zero; }
			set { }
		}

		ZString ITSCAData.DeclarationCertificate
		{
			get { return ""; }
		}

		ZString IGovernmentAgenciesIndicators.AMSIndicator
		{
			get { return ZString.Empty; }
		}

		IEnumerable<IAMSData> IGovernmentAgencies.AMSLines
		{
			get { return System.Array.Empty<IAMSData>(); }
		}

		ZString IGovernmentAgenciesIndicators.AMSDisclaimProgram
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NOPIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NOPDisclaimReason
		{
			get { return ZString.Empty; }
		}

		IEnumerable<INHTSAHeader> IGovernmentAgencies.NHTSALines
		{
			get { return System.Array.Empty<INHTSAHeader>(); }
		}

		ZString IGovernmentAgenciesIndicators.ATFIndicator
		{
			get { return ZString.Empty; }
		}

		public IEnumerable<IATFData> ATFLines
		{
			get { return System.Array.Empty<IATFData>(); }
		}

		ZString IGovernmentAgenciesIndicators.NHTSAIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.NHTSADisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.AMSDisclaimReason
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.DDTCIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.DEAIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.DEADisclaimReason
		{
			get { return ZString.Empty; }
		}

		IEnumerable<IDEAHeader> IGovernmentAgencies.DEAHeaders
		{
			get { return Enumerable.Empty<IDEAHeader>(); }
		}

		ZString IGovernmentAgenciesIndicators.HFCIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IGovernmentAgenciesIndicators.HFCDisclaimReason
		{
			get { return ZString.Empty; }
		}

		IEnumerable<IHFCHeader> IGovernmentAgencies.EPA_HFCHeaders
		{
			get { return Enumerable.Empty<IHFCHeader>(); }
		}

		ZBool IGovernmentAgencies.ShouldIncludePGAInMessage(ZBool isCertified, ZString pgaCode)
		{
			return false;
		}

		#endregion

		#region ISecondaryTariffLine Members

		ZDecimal ISecondaryTariffLine.Duty
		{
			get { return DutyAmount; }
		}

		ZDecimal ISecondaryTariffLine.Quantity1
		{
			get { return CustomsQuantity; }
		}

		ZString ISecondaryTariffLine.UQ1
		{
			get { return CustomsUnitQty; }
		}

		ZDecimal ISecondaryTariffLine.Quantity2
		{
			get { return SecondCustomsQuantity; }
		}

		ZString ISecondaryTariffLine.UQ2
		{
			get { return SecondCustomsUnitQty; }
		}

		ZDecimal ISecondaryTariffLine.Quantity3
		{
			get { return ThirdCustomsQuantity; }
		}

		ZString ISecondaryTariffLine.UQ3
		{
			get { return ThirdCustomsUnitQty; }
		}

		ZDecimal ISecondaryTariffLine.ValueInUSD
		{
			get { return Value; }
		}

		ZDecimal ISecondaryTariffLine.SupCustomsValue
		{
			get { return ZDecimal.Zero; }
		}

		ZString ISecondaryTariffLine.SpecialProgramsIndicatorPrimaryOrCountry
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IEntryLineOrInvoiceLineDutyData Members

		ZGuid IEntryLineOrInvoiceLineDutyData.PK
		{
			get { return ZGuid.Empty; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyFreeSPIClaimed
		{
			get { return false; }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsRecon
		{
			get { return false; }
		}

		ZDecimal IEntryLineOrInvoiceLineDutyData.TotalCustomsValueIncludingSecondaryLines
		{
			get
			{
				ZDecimal result = Value;

				foreach (ISecondaryTariffLine secondaryLine in SecondaryLines)
				{
					result += secondaryLine.ValueInUSD;
				}

				return result;
			}
		}

		ZString IEntryLineOrInvoiceLineDutyData.CalculateException { get; set; }

		ZDecimal IDutyData.ValueForADD
		{
			get { return ADDSpecificDepositValue; }
		}

		ZDecimal IDutyData.ADDDepositRate
		{
			get { return ADDDepositRate; }
		}

		ZDecimal IDutyData.ValueForCVD
		{
			get { return CVDSpecificDepositValue; }
		}

		ZDecimal IDutyData.CVDDepositRate
		{
			get { return CVDDepositRate; }
		}

		IEnumerable<IEntryLineOrInvoiceLineDutyData> IEntryLineOrInvoiceLineDutyData.SecondaryLines
		{
			get { return new TypedEnumerable<IEntryLineOrInvoiceLineDutyData>(SecondaryLines); }
		}

		IEnumerable<IDutyData> IEntryLineOrInvoiceLineDutyData.ChildLines
		{
			get { return new TypedEnumerable<IDutyData>(SecondaryLines); }
		}

		IEntryLineOrInvoiceLineDutyData IEntryLineOrInvoiceLineDutyData.ParentLine
		{
			get { return null; }
		}

		void IEntryLineOrInvoiceLineDutyData.SetDutyResult(IDutyResult dutyResult)
		{
			DutyAmount = dutyResult.TotalAmount.Amount;
		}

		void IEntryLineOrInvoiceLineDutyData.SetDutyFeeChargeAmount(string chargeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			if (chargeCode == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)
			{
				DutyAmount = amount;
			}
			else
			{
				((IFeeCalculationDataProvider)this).SetFeeResult(chargeCode, amount, feeCalculationInternalData);
			}
		}

		ZDecimal IEntryLineOrInvoiceLineDutyData.GetDutyFeeChargeAmount(string chargeCode)
		{
			if (chargeCode == Core.Constants.Customs.CusEntryFeeTypes.DutyAmount)
			{
				return DutyAmount;
			}
			else
			{
				IEntryLineOrInvoiceLineDutyData parentLine = ParentLine;

				if (parentLine != null)
				{
					return parentLine.GetDutyFeeChargeAmount(chargeCode);
				}
				else
				{
					DummyFee fee = Fees.Find(x => x.Code == chargeCode);
					return fee != null ? fee.Amount : ZDecimal.Zero;
				}
			}
		}

		void IEntryLineOrInvoiceLineDutyData.StoreAdjustedDerivedCustomsValue(ZDecimal derivedCustomsValue)
		{
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateInvoiceLinesLinkToEntryLineForDerivedDutyCalculation(ZGuid newEntryLinePK)
		{
		}

		IEnumerable<IFeeCalculationDataProvider> IEntryLineOrInvoiceLineDutyData.FeeDataProviders
		{
			get { yield return this; }
		}

		void IEntryLineOrInvoiceLineDutyData.RollUpFees(IDutyDataLineHeader entry)
		{
			foreach (IFee fee in Fees)
			{
				entry.FeeAndCharges.UpdateOrAddCharge(fee.Code, fee.Amount);
			}
		}

		void IEntryLineOrInvoiceLineDutyData.UpdateLineAndHeaderFeeAmountLessThanThreshold(ZString feeType, ZDecimal thresholdAmount)
		{
		}

		bool IEntryLineOrInvoiceLineDutyData.IsDutyOverridden
		{
			get { return false; }
		}

		IEnumerable<IDutyData> IFeeCalculationDataProvider.SecondaryLines
		{
			get { return SecondaryLines.Cast<IDutyData>(); }
		}

		bool IEntryLineOrInvoiceLineDutyData.IsMPFOverridden
		{
			get { return false; }
		}

		#endregion

		#region IDutyData Members

		ZString IDutyData.Tariff
		{
			get { return Tariff; }
		}

		USCTariff IDutyData.ImportTariff
		{
			get { return ImportTariff; }
		}

		ZDate IDutyData.DateForDutyCalculation
		{
			get { return ZDate.Today; }
		}

		ZDecimal IDutyData.Quantity1
		{
			get { return CustomsQuantity; }
		}

		ZString IDutyData.UQ1
		{
			get { return CustomsUnitQty; }
		}

		ZDecimal IDutyData.Quantity2
		{
			get { return SecondCustomsQuantity; }
		}

		ZString IDutyData.UQ2
		{
			get { return SecondCustomsUnitQty; }
		}

		ZDecimal IDutyData.Quantity3
		{
			get { return ThirdCustomsQuantity; }
		}

		ZString IDutyData.UQ3
		{
			get { return ThirdCustomsUnitQty; }
		}

		ZDecimal IDutyData.CustomsValue
		{
			get { return Value; }
		}

		ZDecimal IDutyData.SupCustomsValue
		{
			get { return ZDecimal.Zero; }
		}

		BusinessObjectFactory IDutyData.Factory
		{
			get { return Factory; }
		}

		IDutyData IDutyData.ParentTariffLine
		{
			get { return ParentLine; }
		}

		bool IDutyData.IsCottonFeeExemptIndicated
		{
			get { return CottonCertificateNumberOrganicExemptionCertificateNumber == CottonFeeCalculator.ExemptCottonFeeCertificate; }
		}

		public ZBool IsSetXLine
		{
			get { return SpecialProgramsIndicatorSecondary == "X"; }
		}

		public ZBool IsSetVLine
		{
			get { return SpecialProgramsIndicatorSecondary == "V"; }
		}

		bool IDutyData.IsAMSFeeExempt
		{
			get { return CottonCertificateNumberOrganicExemptionCertificateNumber == "ORGANIC"; }
		}

		bool IDutyData.IsRaspberryFeeExempt
		{
			get { return false; }
		}

		bool IDutyData.HasCottonCertificate
		{
			get { return !CottonCertificateNumberOrganicExemptionCertificateNumber.IsEmpty; }
		}

		ZString IDutyData.EntryType
		{
			get { return header.EntryType; }
		}

		bool IDutyData.IsClearedInPR
		{
			get { return false; }
		}

		bool IDutyData.IsSecondaryTariffLine
		{
			get { return ParentLine != null && SpecialProgramsIndicatorSecondary != "V"; }
		}

		bool IDutyData.IsDomesticMerchandise
		{
			get { return false; }
		}

		bool IDutyData.HasTextileCategoryNo
		{
			get { return false; }
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get { return null; }
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get { return null; }
		}

		bool IDutyData.IsCombineSecondaryTariffLine
		{
			get { return false; }
		}

		IReadOnlyList<ZString> IDutyData.SupTariffs
		{
			get { return Array.Empty<ZString>(); }
		}

		IEnumerable<IDutyData> IDutyData.CombineChildLines
		{
			get { return System.Array.Empty<IDutyData>(); }
		}

		IEnumerable<IDutyData> IDutyData.CombineAllLines
		{
			get { return System.Array.Empty<IDutyData>(); }
		}

		IDutyData IDutyData.CombineParentLine
		{
			get { return null; }
		}

		#endregion

		#region IFeeCalculationDataProvider Members

		bool IFeeCalculationDataProvider.IsACS => false;

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			return false;
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			IFeeCalculationDataProvider parentLine = ParentLine;

			if (parentLine != null)
			{
				parentLine.SetFeeResult(feeCode, amount, feeCalculationInternalData);
			}
			else
			{
				DummyFee fee = Fees.Find(x => x.Code == feeCode);

				if (fee == null && amount > 0)
				{
					fee = new DummyFee();
					fee.Code = feeCode;

					Fees.Add(fee);
				}

				if (fee != null)
				{
					fee.Amount += amount;
				}
			}
		}

		ZString IFeeCalculationDataProvider.GetSelectedRateType(string feeCode)
		{
			return ZString.Empty;
		}

		ZDateTime IFeeCalculationDataProvider.DateForMPFCalculation
		{
			get { return ZDateTime.Today; }
		}

		ZDecimal? IFeeCalculationDataProvider.OverriddenTaxRate
		{
			get { return null; }
		}

		ZString IFeeCalculationDataProvider.OverriddenTaxRateUQ
		{
			get { return ZString.Empty; }
		}

		ZString IFeeCalculationDataProvider.TaxCode
		{
			get { return ZString.Empty; }
		}

		ZString IFeeCalculationDataProvider.TaxComputationCode
		{
			get { return ZString.Empty; }
		}

		ZString IFeeCalculationDataProvider.TaxRateType
		{
			get { return ZString.Empty; }
		}

		ZDecimal IFeeCalculationDataProvider.TaxRateQuantity
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal IFeeCalculationDataProvider.DairyQty
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region IPGALineNumbers Members

		ZInt IPGALineNumbers.EPAStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.FSISStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.NMFSStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.FDAStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.TTBStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.NHTSAStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.AMSStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.APHStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.FWSStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.ATFStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.CPSCStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.OMCStartLineNumber
		{
			get { return 0; }
			set { }
		}

		ZInt IPGALineNumbers.DEAStartLineNumber
		{
			get { return 0; }
			set { }
		}

		void IPGALineNumbers.ClearPGALineNumbers()
		{
		}

		#endregion
	}
}
