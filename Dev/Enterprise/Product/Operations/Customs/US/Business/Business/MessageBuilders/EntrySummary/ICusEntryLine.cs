using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IFCC : IOGALine
	{
		ZString ImportConditionNumber { get; }
		ZBool ImportConditionNumberQuantityApproval { get; }
		ZInt FCCLineNumber { get; set; }
		ZString FCCIdentifier { get; }
		ZString TradeName { get; }
		ZString ModelTypeNumber { get; }
		ZDecimal FCCQuantity { get; }
		ZBool WithholdFromPublicInspectionRequested { get; }
		ZString CommercialDescription { get; }
	}

	public interface IDOT : IOGALine
	{
		ZString CommercialDescription { get; }
		ZString BoxNumber { get; }
		ZString BoxCertification { get; }
		ZString PassportNumber { get; }
		ZString CountryISO { get; }
		ZString DOTBondSuretyCode { get; }
		ZBool NHTSAPermissionLetterOfficialOrdersCertification { get; }
		ZBool ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter { get; }
		ZString ClarificationCode { get; }
		ZString TireManufacturerIDCode { get; }
		ZString TireManufacturerBrandName { get; }
		IEnumerable<IDOTVIN> VINs { get; }
	}

	public interface IDOTVIN
	{
		ZString MakeOfVehicle { get; }
		ZString Model { get; }
		ZInt Year { get; }
		ZString VehicleIdentificationNumber { get; }
		ZString NHTSARegisteredImporterRINumber { get; }
		ZString VehicleEligibilityNumber { get; }
	}

	//objects which have OGA details like CusEntryLine, JobComInvoiceLine
	public interface IOGA
	{
		/// <summary>
		/// FD01 + FD02 + FD03 + FD04 + FD05
		/// </summary>
		IList<IPriorNoticeLine> FDA { get; }
		ZString FDAIndicator { get; }

		/// <summary>
		/// DT01 + DT02 + (DT03 one day)
		/// </summary>
		IEnumerable<IDOT> DOT { get; }
		ZString DOTIndicator { get; }
	}

	//IFCC, IDOT, IFDALine
	public interface IOGALine
	{
		ZString CommercialDesc { get; set; }
	}

	public interface ICargoReleaseCusEntryLine : ICusEntryLine
	{
		ZString UltimateConsigneeNumber { get; }
	}

	public interface ICusEntryLine : Customs.Business.ICusEntryLine, IGovernmentAgenciesCommon
	{
		ICusEntryLine ParentLine { get; }
		ZString ImportTariffCode { get; }
		ZString ExportTariffCode { get; }
		ZString NAFTATariff { get; }
		ZDecimal NAFTADutyRate { get; }
		ZDecimal NAFTADutyFGN { get; }
		ZDecimal NAFTADutyUS { get; }
		ZString ImportFTZNumber { get; }

		ZString CountryOfOrigin { get; }
		ZDecimal GrossWeightInKilograms { get; }
		ZDecimal ADDSpecificDepositValue { get; }
		ZDecimal CVDSpecificDepositValue { get; }
		ZDecimal Charges { get; }
		ZString PortOfLading { get; }
		ZString ZoneStatus { get; }
		ZDate PrivilegedStatusFilingDate { get; }
		ZBool NAFTANetCostIndicator { get; }
		ZInt FTZLineItemQuantity { get; }

		/// <summary>
		/// The last line of an invoice indicates the invoice sequence
		/// </summary>
		ZShort InvDelimter { get; }

		// ENS43
		ZString PreImportationReviewProgramRulingsType { get; }
		ZString PreImportationReviewProgramRulingsNumber { get; }
		IEnumerable<ZString> CommercialDescriptions { get; }

		// ENS50
		ZString SpecialProgramsIndicatorPrimary { get; }
		ZDecimal Quantity1 { get; }
		ZString UnitOfMeasure1 { get; }
		ZDecimal Quantity2 { get; }
		ZString UnitOfMeasure2 { get; }
		ZDecimal Quantity3 { get; }
		ZString UnitOfMeasure3 { get; }
		ZString CountryOfExport { get; }
		ZDate DateOfExportation { get; }
		ZBool RelatedPartyIndicator { get; }
		ZString SpecialProgramsIndicatorCountry { get; }
		ZString SpecialProgramsIndicatorSecondary { get; }
		ZBool IsSupLine { get; }

		// ENS51
		ZDate DateOfExportationFromCountryOfOrigin { get; }
		ZString VisaNumber { get; }
		ZString TextileCategoryNumber { get; }
		ZDecimal VisaQuantity { get; }
		ZString VisaUnitOfMeasure { get; }
		ZString AgricultureLicenseNumber { get; }
		ZString CottonCertificateNumberOrganicExemptionCertificateNumber { get; }

		// ENS52
		ZString ChinaHongKongSWPMIndicator { get; }
		ZString CanadianExportCertificateSugar { get; }
		ZString WoolLicense { get; }
		ZString CBTPACertificationNumber { get; }
		ZString MiscellaneousPermitLicenseNumber { get; }
		ZBool IsSoftwoodLumberLine { get; }
		ZBool IsSoftwoodLumberImporterDeclaration { get; }
		ZDecimal SoftwoodLumberExportPrice { get; }
		ZDecimal SoftwoodLumberExportCharges { get; }

		// ENS60
		ZDecimal CountervailingDuty { get; }
		ZString CountervailingCaseNumber { get; }
		ZString AntidumpingCaseNumber { get; }
		ZDecimal AntidumpingDuty { get; }
		ZString ManufacturerSupplierCode { get; }
		ZDecimal ExciseTax { get; }
		ZDecimal CVDDepositRate { get; }
		ZDecimal ADDDepositRate { get; }
		ZBool BondedCountervailingDuty { get; }
		ZBool BondedAntidumpingDuty { get; }
		ZString ADDCaseRateTypeQualifier { get; }
		ZString CVDCaseRateTypeQualifier { get; }

		// FTZ
		ZString FTZCurrentTariff { get; }

		// ENS62
		IEnumerable<IFee> Fees { get; }
		ZString GetRelevantTariffForFee(string feeType);

		// ENS 70 + 80 + 81
		IEnumerable<ISecondaryTariffLine> SecondaryTariffLines { get; }

		ZDecimal SecondCustomsQuantity { get; }
		ZString SecondCustomsUnitQty { get; }
		ZDecimal ThirdCustomsQuantity { get; }
		ZString ThirdCustomsUnitQty { get; }
		ZDate DateForDutyCalculation { get; }
		ZString SelectedRateType { get; }

		ZBool IsDisclaimSanction { get; }
		IEnumerable<ISanctionsAdditionalInfo> SanctionsAdditionalInfos { get; }
	}

	public interface IACECusEntryLine : ICusEntryLine
	{
		// ENS 40
		ZString ArticleSetIndicator { get; }
		ZString CL_LineNumberFormatted { get; }
		ZString FeeExemptionCode { get; }
		ZString ADDCVDNonReimbursementStatement { get; }

		// ENS 47
		ZString SoldToPartyID { get; }
		ZString DeliveredToPartyCode { get; }
		ZString ForeignExporterMID { get; }

		// ENS 50
		ZDecimal SupCustomsValue { get; }

		// ENS 52
		IEnumerable<KeyValuePair<ZString, ZString>> LicenceTypeAndNumbers { get; }

		// ENS 53
		ZDecimal ADDQuantity { get; }
		ZString ADDDecID { get; }
		ZBool IsADDBonded { get; }

		ZDecimal CVDQuantity { get; }
		ZBool IsCVDBonded { get; }

		// ENS 54
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IEnumerable<KeyValuePair<ZString, ZString>> AdditionalDeclarationDetails { get; }

		// ENS 60
		ZString IRTaxCode { get; }
		ZBool IRTaxMandatory { get; }

		// CW02
		IEnumerable<ICensusWarningOverride> CensusWarningOverrideCodes { get; }
	}

	public interface IGovernmentAgenciesCommon : IGovernmentAgencies, IOGA, IPGALineNumbers
	{
	}

	public interface IPGAGovernmentAgenciesCommon : IGovernmentAgenciesCommon
	{
		ZInt LineNumber { get; }
		ZString Tariff { get; }
	}

	public interface ICensusWarningOverride
	{
		ZString ConditionCode { get; }
		ZString OverrideCode { get; }
	}

	public interface ISanctionsAdditionalInfo
	{
		ZString RecordID { get; }
		ZString RecordType { get; }
		ZString FieldName { get; }
		ZString FieldValue { get; }
	}

	public interface ISimplifiedEntryLine : ICusEntryLine
	{
		IEnumerable<ISimplifiedEntryOrganisationDetails> Entities { get; }
	}

	public interface IPGALineNumbers
	{
		ZInt EPAStartLineNumber { get; set; }
		ZInt FSISStartLineNumber { get; set; }
		ZInt NMFSStartLineNumber { get; set; }
		ZInt FDAStartLineNumber { get; set; }
		ZInt TTBStartLineNumber { get; set; }
		ZInt NHTSAStartLineNumber { get; set; }
		ZInt AMSStartLineNumber { get; set; }
		ZInt APHStartLineNumber { get; set; }
		ZInt FWSStartLineNumber { get; set; }
		ZInt ATFStartLineNumber { get; set; }
		ZInt CPSCStartLineNumber { get; set; }
		ZInt OMCStartLineNumber { get; set; }
		ZInt DEAStartLineNumber { get; set; }

		void ClearPGALineNumbers();
	}
}
