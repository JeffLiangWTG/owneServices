using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.LicenseManager;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.STU;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.NumberFountain;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ACEMessageBuildingBlocks = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using IBillDetails = Enterprise.Freight.Forwarding.Business.IBillDetails;
using Logs = Enterprise.ZArchitecture.Business.Logs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[SystemDefinedValues]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.USJobDeclaration)]
	[GlowDataDefinition("IUSJobDeclaration")]
	[VisualizableDocumentsSupportable("JobDeclarationUSVisualizableDocumentSupporter")]
	[UniversalCopyWithExtendedEntities(StartCopyMethod = "StartUniversalCopy", FinishCopyMethod = "AfterUniversalCopy")]
	public partial class JobDeclaration : AutoJobDeclaration,
		Integration.Customs.US.IJobDeclaration,
		IMessageActionHeader,
		IBackDoorSavingSupportableBizObj,
		IMessageAttacheeInDeclaration,
		ILandedCostHeader,
		IInvoicesProvider,
		IDutyDataLineHeaderProvider,
		IDispositionCodeDateParent,
		IDocumentDataStateManager,
		IBox29Supportable,
		IPPQForm368NoticeOfArrivalSupportable,
		IBondDetailsDefault,
		IPrelimStatementDetailsDefault,
		IClientBranchDesignationDefault,
		IJobInvoicingPlugIn,
		IPriorNoticeHeader,
		IPriorNoticeProcessor,
		IFDACorrectionHeader,
		IMessageFailStatusManager,
		Integration.Customs.ICusAddInfoTypeSupporter,
		ICargoManifestStatusQueryData,
		ICustomsJobInfo,
		ICBPEDIMessageMessageTextNumberPlaceHolderFiller,
		IFZEventHeader,
		ICurrencyConverterDataProvider,
		IStatementDeleteTransaction,
		Freight.Integration.ICusInBondParent,
		IWorkflowProviderEvent,
		Integration.Customs.ICusCodeDataTypeSupporter,
		IMessageNotificationsProvider,
		IStatementLineDeclaration,
		IDocAddresses,
		IUSDISHost,
		IDISHostProvider,
		IDeclarationProvider,
		IApportionInvoiceHolder,
		IAddInfoWithConcurrencyResolverSupporter,
		ICusEntryNumberParent,
		ICusDispositionParent,
		ICargoManifestStatusQueryHeader,
		IDeclarationDutyDataProvider,
		ILiquidationProvider,
		IDeclaration
	{
		#region Constants

		public static class Constants
		{
			public static class DisallowEntryNumberAllocation
			{
				public static ZString EntryNumberAlreadyAllocated(string entryNumber)
				{
					return ZString.Format("An Entry Number ({0}) is already allocated to an entry on this job.\nOnce an entry number is used, a new entry number cannot be allocated.", entryNumber);
				}
			}

			public static class FTZControlNumberAllocation
			{
				public static ZString FTZControlNumberAlreadyAllocated(string controlNumber)
				{
					return ZString.Format("An FTZ Control Number ({0}) is already allocated to this job.\nOnce an FTZ Control Number is used, a new FTZ Control Number cannot be allocated.", controlNumber);
				}

				public static ZString FTZControlNumberRangeIsEmpty
				{
					get { return "There are no more FTZ Admission Control Numbers available.\r\nPlease go to Importer > Details > Config > Number Ranges to allocate more numbers."; }
				}
			}

			public static class USFilterConstants
			{
				public const string ShipmentType = "Shipment Type";
				public const string ReleaseStatus = "Release Status";
				public const string ImporterOfRecord = "Importer of Record";
				public const string UltinateConsignee = "Consignee/Ultimate Consignee";
				public const string PortOfEntry = "Entry Port";
				public const string ConsolidatedJobNo = "Consolidated Job #";
				public const string ReleaseDate = "Release Date";
			}

			internal const string MessageErrorOrWarningGrossWeightNotAllowedWhenMOTIsPHC = "Gross weight is not allowed when Mode Of Transport is �PHC - Passenger Hand Carried�.";

			public const string ReconMessageDataModel = "USR";

			public static class GenAddOnColumnFieldName
			{
				public const string BLUStatus = "US_BLUStatus";
				public const string FDAMsgStatus = "US_FDAMsgStatus";
				public const string FDAStatus = "US_FDAStatus";
				public const string ReleaseStatus = "US_ReleaseStatus";
				public const string US_PSDAccepted = "US_PSDAccepted";
				public const string FTZAdmissionStatus = "US_FTZAdmissionStatus";
				public const string FTZArrivalStatus = "US_FTZArrivalStatus";
				public const string FTZConcurrenceStatus = "US_FTZConcurrenceStatus";
				public const string FTZDeliveryOfGoodsStatus = "US_FTZDeliveryOfGoodsStatus";
				public const string FTZUnconcurrenceStatus = "US_FTZUnconcurrenceStatus";
				public const string FTZPTTStatus = "US_FTZPTTStatus";
				public const string IsPGATrackingEnabled = "IsPGATrackingEnabled";
				public const string US_PGAReplaceUpdateNeeded = "US_PGAReplaceUpdateNeeded";
				public const string US_PGACorrectionStatus = "US_PGACorrectionStatus";
				public const string US_QuotaStatus = "US_QuotaStatus";
				public const string US_ConsolidatedJobNumber = "US_ConsolidatedJobNumber";
				public const string US_ConsolidatedEntryNumber = "US_ConsolidatedEntryNumber";
			}

			internal const string Multiple = "ON PACKING TAB";
			internal const string UpdatedDatesLog = "Declaration Dates changed during upgrade";
			internal const string FTZRoutingDetailsFormat = @"^\d{4}[a-zA-Z0-9]{3}(\d{2})?$";

			internal const string Complete = "Complete";
			internal const string Incomplete = "Incomplete";
		}

		public static class GenAddOnColumnMaxLength
		{
			public const int BLUStatus = 3;
			public const int FDAMsgStatus = 3;
			public const int FDAStatus = 2;
			public const int US_PGACorrectionStatus = 3;
			public const int FTZAdmissionStatus = 3;
			public const int FTZConcurrenceStatus = 3;
			public const int FTZDeliveryOfGoodsStatus = 3;
			public const int FTZUnconcurrenceStatus = 3;
			public const int FTZArrivalStatus = 3;
			public const int FTZPTTStatus = 3;
			public const int StatusNotificationDispositionCode = 1;
			public const int US_QuotaStatus = 3;
		}

		#endregion

		#region ModelViewSchema

		public static class ModelViewSchema
		{
			public const string TableName = "USJobDeclaration";
			public const string TableName_Recon = "USReconJobDeclaration";
			public const string PK = JobDeclaration.Schema.PK;
			public const string JE_CRLAction = "JE_CRLAction";
			public const string JE_ENSAction = "JE_ENSAction";
		}

		#endregion

		#region Schema

		public new class Schema : AutoJobDeclaration.Schema
		{
			public const string MasterBillIssueLabelText = "MasterBillIssueLabelText";
			public const string JE_MasterBillIssuerSCAC = "JE_MasterBillIssuerSCAC";
			public const string JE_HouseBillIssuerSCAC = "JE_HouseBillIssuerSCAC";
			public const string InvoiceNumberLabelText = "InvoiceNumberLabelText";
			public const string JE_MasterBillExpressTracking = "JE_MasterBillExpressTracking";
			public const string CarrierSCAC = "CarrierSCAC";
			public const string ITNumbers = "ITNumbers";
			public const string JE_PrimaryITNumber = "JE_PrimaryITNumber";
			public const string BLUStatus = "BLUStatus";
			public const string CargoReleaseStatus = "CargoReleaseStatus";
			public const string CargoReleaseStatusDesc = "CargoReleaseStatusDesc";
			public const string SimplifiedEntryBillStatus = "SimplifiedEntryBillStatus";
			public const string SimplifiedEntryBillStatusDescription = "SimplifiedEntryBillStatusDescription";
			public const string HLDOrEXMStatus = "HLDOrEXMStatus";
			public const string EntrySummaryStatus = "EntrySummaryStatus";
			public const string EntrySummaryStatusDesc = "EntrySummaryStatusDesc";
			public const string EntrySubmittedDate = "EntrySubmittedDate";
			public const string ExportStatus = "ExportStatus";
			public const string ExportStatusDesc = "ExportStatusDesc";
			public const string InBondClosedDate = "InBondClosedDate";
			public const string InBondEntryTypes = "InBondEntryTypes";
			public const string FDAStatusDescription = "FDAStatusDescription";
			public const string ITEntryNo = "ITEntryNo";
			public const string DecEntryNumber = "DecEntryNumber";
			public const string JE_Calc_ReconJob = "JE_Calc_ReconJob";
			public const string ManufacturerNameAndID = "ManufacturerNameAndID";
			public const string ElectronicInvoiceStatus = "ElectronicInvoiceStatus";
			public const string ElectronicInvoiceStatusDescription = "ElectronicInvoiceStatusDescription";
			public const string US_7512OpenArea = ITDoc.Schema.US_7512OpenArea;
			public const string FDAStatus = "FDAStatus";
			public const string FDAMsgStatus = "FDAMsgStatus";
			public const string FDAMsgStatusDescription = "FDAMsgStatusDescription";
			public const string JE_OH_FDASubmitter = "JE_OH_FDASubmitter";
			public const string JE_OA_InvoicerAddress = "JE_OA_InvoicerAddress";

			public const string US_7501IOR = "US_7501IOR";
			public const string US_PPQForm368Box13A = PPQForm368Data.Schema.US_PPQForm368Box13A;
			public const string US_PPQForm368Box13B = PPQForm368Data.Schema.US_PPQForm368Box13B;
			public const string US_PPQForm368Box13C = PPQForm368Data.Schema.US_PPQForm368Box13C;
			public const string IsPPQForm368Box13Compatible = ForwardingShipment.Schema.IsPPQForm368Box13Compatible;
			public const string IsPGARecapDocumentToBeShown = ForwardingShipment.Schema.IsPGARecapDocumentToBeShown;
			public const string IsProofOfReleaseDocumentToBeShown = ForwardingShipment.Schema.IsProofOfReleaseDocumentToBeShown;
			public const string RelatedStatementPK = "RelatedStatementPK";
			public const string StatementPaidDate = "StatementPaidDate";
			public const string TotalPayable = "TotalPayable";
			public const string StatementNo = "StatementNo";
			public const string StatementStatus = "StatementStatus";
			public const string StatementStatusDesc = "StatementStatusDesc";
			public const string PaymentStatus = "PaymentStatus";
			public const string PaymentStatusDesc = "PaymentStatusDesc";
			public const string BLUStatusDescription = "BLUStatusDescription";
			public const string LiquidationDate = "LiquidationDate";
			public const string EarliestExportDate = "EarliestExportDate";

			public const string IORName = "IORName";
			public const string UltimateConsigneeName = "UltimateConsigneeName";
			public const string SPIAuditDate = "SPIAuditDate";
			public const string SPIAuditUser = "SPIAuditUser";
			public const string SPIAuditUserName = "SPIAuditUserName";
			public const string SPIAuditReference = "SPIAuditReference";
			public const string FDAAuditDate = "FDAAuditDate";
			public const string FDAAuditUser = "FDAAuditUser";
			public const string FDAAuditUserName = "FDAAuditUserName";
			public const string FDAAuditReference = "FDAAuditReference";
			public const string CWAuditDate = "CWAuditDate";
			public const string CWAuditUser = "CWAuditUser";
			public const string CWAuditUserName = "CWAuditUserName";
			public const string CWAuditReference = "CWAuditReference";

			public const string ReleaseStatus = "ReleaseStatus";
			public const string ReleaseStatusDesc = "ReleaseStatusDesc";
			public const string OtherReconIndicatorDescription = "OtherReconIndicatorDescription";
			public const string JE_OH_CBPBroker = "JE_OH_CBPBroker";
			public const string IsSplitShipment = "IsSplitShipment";

			public const string TIBExpiryDate = "TIBExpiryDate";
			public const string TIBNumOfExtensions = "TIBNumOfExtensions";
			public const string TIBClosedDate = "TIBClosedDate";
			public const string TIBClosedUser = "TIBClosedUser";
			public const string TIBClosedUserName = "TIBClosedUserName";
			public const string TIBClosedReference = "TIBClosedReference";
			public const string BrokerToPayIndicator = "BrokerToPayIndicator";

			public const string ISFBillStatus = "ISFBillStatus";
			public const string ISFBillStatusDescription = "ISFBillStatusDescription";
			public const string ShipperReferenceNumber = "BGMReferences";

			public const string FTZAdmissionNumber = "FTZAdmissionNumber";
			public const string FTZAdmissionNumberFormatted = "FTZAdmissionNumberFormatted";
			public const string AdmissionStatus = "AdmissionStatus";
			public const string AdmissionStatusDescription = "AdmissionStatusDescription";
			public const string FTZArrivalStatus = "FTZArrivalStatus";
			public const string FTZArrivalStatusDescription = "FTZArrivalStatusDescription";
			public const string FTZConcurrenceStatus = "FTZConcurrenceStatus";
			public const string FTZConcurrenceStatusDescription = "FTZConcurrenceStatusDescription";
			public const string FTZDeliveryOfGoodsStatus = "FTZDeliveryOfGoodsStatus";
			public const string FTZDeliveryOfGoodsStatusDescription = "FTZDeliveryOfGoodsStatusDescription";
			public const string FTZUnconcurrenceStatus = "FTZUnconcurrenceStatus";
			public const string FTZUnconcurrenceStatusDescription = "FTZUnconcurrenceStatusDescription";
			public const string FTZPTTStatus = "FTZPTTStatus";
			public const string FTZPTTStatusDescription = "FTZPTTStatusDescription";

			public const string US_AnticipatedLiquidationDate = "US_AnticipatedLiquidationDate";
			public const string US_CollectionDate = "US_CollectionDate";

			public const string WHSInvLineFilter = "WHSInvLineFilter";
			public const string WHSPackageFilter = "WHSPackageFilter";
			public const string WHSProductFilter = "WHSProductFilter";

			public const int US_WHSEntryNumberMaxLength = 8;

			public const string DISStatus = "DISStatus";
			public const string DISStatusDescription = "DISStatusDescription";
			public const string IncompleteDispositionsCode = "IncompleteDispositionsCode";
			public const string IncompleteDispositionsDescription = "IncompleteDispositionsDescription";

			public const string ImporterEIN = "ImporterEIN";
			public const string ImporterOfRecordEIN = "ImporterOfRecordEIN";

			public const string PGAStatus = "PGAStatus";
			public const string PGAStatusDesc = "PGAStatusDesc";
			public const string US_PGACorrectionStatusDesc = "US_PGACorrectionStatusDesc";
			public const string US_QuotaStatusDesc = "US_QuotaStatusDesc";

			public const string CurrentBillNumber = "CurrentBillNumber";
			public const string IOROrgPK = "IOROrgPK";

			public const string FTZControlNumber = "FTZControlNumber";
			public const string FTZYear = "FTZYear";
			public const string FTZZoneID = "FTZZoneID";

			public const int FTZAdmissionNumberMaxLenth = 20;
			public const int FTZControlNumberMaxLength = 8;
			public const int FTZYearMaxLength = 2;
			public const int VesselNameLength = 20;
			public const int FTZVesselNameLength = 23;
			public const int FTZZoneIDMaxLength = OrganisationViewStmNums.Schema.SN_ZoneIDPrefixMaxLength;
			public const int OldFTZZoneIDMaxLength = OrganisationViewStmNums.Schema.OldSN_ZoneIDPrefixMaxLength;

			public const string USD_RetailSalesSubstitutionIndicator = JobUSDeclaration.Schema.USD_RetailSalesSubstitutionIndicator;
			public const string USD_OH_ForeignPrincipalParty = JobUSDeclaration.Schema.USD_OH_ForeignPrincipalParty;

			public const string AESResponseCode = "AESResponseCode";
			public const string AESResponseCodeDescription = "AESResponseCodeDescription";
			public const string AESSeverity = "AESSeverity";
			public const string AESSeverityDescription = "AESSeverityDescription";
		}

		#endregion

		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation Modes

		ValidationModesCalculator ValidationModesCalculator
		{
			get { return new ValidationModesCalculator(this); }
		}

		public void RecalculateValidationModesOnDeclaration()
		{
			RecalculateValidationModesOnDeclaration(false);
		}

		public void RecalculateValidationModesOnDeclaration(bool suspendMarkAsNeedingValidation)
		{
			ValidationModesCalculator.RecalculateValidationModesOnDeclaration(suspendMarkAsNeedingValidation);
		}

		public bool US_EnableCRL_ReadOnly
		{
			get { return IsCRL_CargoReleaseType_ReadOnly; }
		}

		public bool US_CargoReleaseType_ReadOnly
		{
			get { return IsCRL_CargoReleaseType_ReadOnly || !IsACE && !US_EnableCRL || (IsACE && US_EnableCRL) || IsACEJobWithoutENSAndCRL; }
		}

		internal bool IsACEJobWithoutENSAndCRL
		{
			get { return IsACE && !US_EnableENS && !US_EnableCRL; }
		}

		public bool IsStandalonePNTypeOfBLN
		{
			get { return US_SPNIDType == StandAlonePriorNoticeIDTypeList.Codes.BLN; }
		}

		public bool IsACEStandalonePNWithoutENSAndCRL
		{
			get { return US_EnableSPN && IsACEJobWithoutENSAndCRL; }
		}

		public bool US_SERefNo_ReadOnly
		{
			get { return true; }
		}

		public bool US_SEReasonCode_ReadOnly
		{
			get { return true; }
		}

		public bool US_SEMultiCargoDispInd_ReadOnly
		{
			get { return true; }
		}

		bool IsCRL_CargoReleaseType_ReadOnly
		{
			get
			{
				var isRLFForACS = !IsACE && IsRemoteLocationFiling;

				return isRLFForACS || EntryTypeList.HasCargoEnteredUSTerritory(US_EntryType) && !IsACEReWarehouse;
			}
		}

		public bool IsEnableTwoStepProcess
		{
			get { return USCustomsDataRegistry.Instance.EnableTwoStepsProcess.GetFallBackValueAtAllLevels(CompanyPK.ToGuid(), base.Branch.PK.ToGuid(), Guid.Empty); }
		}

		public bool ShouldUpdateDeclarationWithCargoReleaseResults
		{
			get { return USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.GetFallBackValueAtAllLevels(CompanyPK.ToGuid(), base.Branch.PK.ToGuid(), Guid.Empty); }
		}

		internal bool IsACEReWarehouse
		{
			get { return IsACE && IsReWarehouse; }
		}

		public override ZBool US_EnableCRL
		{
			get { return base.US_EnableCRL; }
			set
			{
				bool hasChanges = base.US_EnableCRL != value;

				var oldCertificationMode = IsACECargoCertificationMode;

				base.US_EnableCRL = value;
				if (hasChanges && !IsCopying)
				{
					if (US_EnableCRL && IsImport && !IsFTZAdmission)
					{
						US_CertifyCargoRelease = false;
					}
					if (US_EnableCRL && IsEnableTwoStepProcess && IsImport)
					{
						US_EnableENS = true;
					}
					UpdateCargoReleaseValidationMode();
					var eventSuspender = IsACE ? new CargoReleaseTypeChangingEventSuspender(this) : (IDisposable)new DisposableObject();
					try
					{
						MessageTypeBasedValueDefaulter.SetCertifyCargoReleaseAndDefaultCargoReleaseTypeIfNecessary();
					}
					finally
					{
						eventSuspender.Dispose();
					}
					RefreshInvoiceLinesOGAPGAIndicators(oldCertificationMode != IsACECargoCertificationMode);
				}
				else
				{
					US_EnableCRLInfo.RefreshBinding();
				}
			}
		}

		public BorderCargoPortCollection BCRPortsFromRegistry
		{
			get { return USCustomsDataRegistry.Instance.BorderCargoReleasePorts.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty); }
		}

		public override ZBool US_EnableSPN
		{
			get { return base.US_EnableSPN; }
			set
			{
				bool hasChanges = base.US_EnableSPN != value;
				var oldCertificationMode = IsACECargoCertificationMode;

				base.US_EnableSPN = value;

				if (!IsCopying && hasChanges)
				{
					ValidationModesCalculator.UpdateValidationModes(ValidationModes.StandAlonePriorNotice, value);
					RefreshInvoiceLinesOGAPGAIndicators(oldCertificationMode != IsACECargoCertificationMode);

					if (!US_EnableSPN && !US_SPNIDType.IsEmpty)
					{
						US_SPNIDType = ZString.Empty;
					}

					if (US_EnableSPN)
					{
						if (US_F_PNMode.IsEmpty && IsFTZAdmission)
						{
							US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
						}
					}
					else if (!US_F_PNMode.IsEmpty)
					{
						US_F_PNMode = ZString.Empty;
					}
				}
				else
				{
					US_EnableSPNInfo.RefreshBinding();
				}
			}
		}

		public override ZBool US_CertifyCargoRelease
		{
			get { return base.US_CertifyCargoRelease; }
			set
			{
				bool hasChanges = base.US_CertifyCargoRelease != value;
				base.US_CertifyCargoRelease = value;
				if (hasChanges && !IsCopying)
				{
					if (US_CertifyCargoRelease)
					{
						if (ReleaseStatus.IsEmpty)
						{
							ReleaseStatus = CRLReleaseStatusList.Codes.NRL;
						}
					}
					else
					{
						if (ReleaseStatus == CRLReleaseStatusList.Codes.NRL)
						{
							ReleaseStatus = ZString.Empty;
						}
					}
					UpdateCargoReleaseValidationMode();
				}
			}
		}

		public bool UseScheduleB
		{
			get { return IsExport && US_TariffType != TariffTypeList.Codes.HTS; }
		}

		public bool UseHTSForExport
		{
			get { return IsExport && US_TariffType == TariffTypeList.Codes.HTS; }
		}

		public ValidationModes ValidationModes
		{
			get
			{
				if (!fValidationModes.HasValue)
				{
					RecalculateValidationModesOnDeclaration(true);
				}
				return fValidationModes.Value;
			}
			set
			{
				bool hasChanges = fValidationModes != value;
				fValidationModes = value;
				if (hasChanges)
				{
					MarkAsNeedingValidationForMajorDataChange();
				}
			}
		}
		ValidationModes? fValidationModes;

		public bool IsEntrySummaryOrCargoReleaseValidationMode
		{
			get { return IsEntrySummaryValidationMode || IsCargoReleaseValidationMode; }
		}

		public bool IsCargoReleaseValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.CargoRelease); }
		}

		public bool IsACECargoReleaseValidationMode
		{
			get { return IsACECargoCertificationMode && IsCargoReleaseValidationMode; }
		}

		public bool IsACEFDARelevant
		{
			get { return CanHavePGAFDA; }
		}

		public bool IsNHTSARelevant
		{
			get { return IsACECargoCertificationMode; }
		}

		public bool IsEntrySummaryValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.EntrySummary); }
		}

		public bool IsStandAlonePriorNoticeMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.StandAlonePriorNotice); }
		}

		public bool IsFDAValidationMode
		{
			get
			{
				return IsStandAlonePriorNoticeMode ||
					(JE_MessageType == JobMessageTypeList.Codes.Import && (US_EnableCRL || US_CertifyCargoRelease || IsACEFDARelevant));
			}
		}

		internal bool IsFTZAdmissionValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.FTZAdmissionValidationMode); }
		}

		internal bool IsFTZEventsValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.FTZEventsValidationMode); }
		}

		internal bool IsFTZPTTValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.FTZPTTValidationMode); }
		}

		public bool RequiresPriorNoticeReporting
		{
			get
			{
				CachedProperty<bool> cached = cachedRequiresPriorNoticeReporting ?? (cachedRequiresPriorNoticeReporting = new CachedProperty<bool>(Factory, () => Invoices.RequiresPriorNoticeReporting));
				return cached.Value;
			}
		}
		CachedProperty<bool> cachedRequiresPriorNoticeReporting;

		/// <summary>
		/// This property should only be used on known Prior Notice reporting declarations as the default result is true.
		/// </summary>
		public bool HaveAllFDAPNCsBeenReceived
		{
			get
			{
				if (haveAllFDAPNCsBeenReceivedCached == null)
				{
					haveAllFDAPNCsBeenReceivedCached = new CachedProperty<bool>(Factory, delegate
					{
						bool result = true;

						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							if (OGAIndicatorList.IsToBeDeclared(invoiceLine.US_FDAIndicator))
							{
								foreach (FDA fda in invoiceLine.FDAs)
								{
									if (!fda.HasPNCorPND)
									{
										result = false;
										break;
									}
								}

								foreach (ACEFDA acefda in invoiceLine.ACE_FDALines)
								{
									if (!acefda.HasPNCorPND)
									{
										result = false;
										break;
									}
								}
							}
						}

						return result;
					});
				}
				return haveAllFDAPNCsBeenReceivedCached.Value;
			}
		}
		CachedProperty<bool> haveAllFDAPNCsBeenReceivedCached;

		public bool HasFDATariffsToBeDeclared
		{
			get
			{
				CachedProperty<bool> cached = cachedHasFDATariffsToBeDeclared ?? (cachedHasFDATariffsToBeDeclared = new CachedProperty<bool>(Factory, () => Invoices.HasFDATariffsToBeDeclared));
				return cached.Value;
			}
		}
		CachedProperty<bool> cachedHasFDATariffsToBeDeclared;

		public bool HasOGATariffsToBeDeclared
		{
			get
			{
				CachedProperty<bool> cached = cachedHasOGATariffsToBeDeclared ?? (cachedHasOGATariffsToBeDeclared = new CachedProperty<bool>(Factory, () => Invoices.HasOGATariffsToBeDeclared));
				return cached.Value;
			}
		}
		CachedProperty<bool> cachedHasOGATariffsToBeDeclared;

		public bool IsFDAPriorNoticeValidationRequired
		{
			get { return !US_PSC && IsFDAValidationMode && RequiresPriorNoticeReporting && !HaveAllFDAPNCsBeenReceived; }
		}

		public bool IsFWSEDSValidationRequired => PGAFlags.HasInvoiceLinesWithFWS && PGAFlags.HasInvoiceLinesWithFWSProcessingCodeWithEDS;

		internal bool HasAnyPGADataEitherDeclaredOrDisclaimed
		{
			get
			{
				if (hasAnyPGADataWithDOrC == null)
				{
					hasAnyPGADataWithDOrC = new CachedProperty<bool>(Factory, delegate
					{
						foreach (JobComInvoiceHeader invoice in Invoices)
						{
							if (invoice.HasAnyPGADataEitherDeclaredOrDisclaimed)
							{
								return true;
							}
						}
						return false;
					}
					);
				}
				return hasAnyPGADataWithDOrC.Value;
			}
		}
		CachedProperty<bool> hasAnyPGADataWithDOrC;

		#endregion

		#region Boolean Flags

		internal bool GetIsSettingDefaultValues() => base.IsSettingDefaultValues;

		protected override bool IsDeclarationIntegratedCore() => IsInterface;

		protected override bool ShowSubmitMenuItemCore() => IsInterface;

		public bool IsInsuranceFunctionEnable => IsACE && IsSingleTransactionBond;

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		internal bool ShouldCalculateMPFAndDutyDate => shouldCalculateMPFAndDutyDateIndex > 0;
		byte shouldCalculateMPFAndDutyDateIndex;

		internal bool ShouldCalculateMPFAndDutyDateForImmediateDelivery => shouldCalculateMPFAndDutyDateForImmediateDeliveryIndex > 0;
		byte shouldCalculateMPFAndDutyDateForImmediateDeliveryIndex;

		internal IDisposable ReCalculateMPFAndDutyDate()
		{
			return new DisposableAction(
				() =>
				{
					if (!US_ImmediateDelivery)
					{
						shouldCalculateMPFAndDutyDateIndex++;
					}
					else
					{
						shouldCalculateMPFAndDutyDateForImmediateDeliveryIndex++;
					}
				},
				() =>
				{
					if (!US_ImmediateDelivery)
					{
						shouldCalculateMPFAndDutyDateIndex--;
					}
					else
					{
						shouldCalculateMPFAndDutyDateForImmediateDeliveryIndex--;
					}
				});
		}

		public override ZBool BondedWarehouseEditable
		{
			get { return (IsImport && EntryTypeList.IsWarehouseType(US_EntryType)) || (IsConsumptionFTZ && SupportsBondedWarehousing) || IsExport || IsExWarehouse || IsFTZAdmission; }
		}

		protected override bool AreTransportDetailsIrrelevant
		{
			get
			{
				return base.AreTransportDetailsIrrelevant
					|| IsReconMessageType
					|| IsDrawback
					|| JE_MessageType == JobMessageTypeList.MoreCodes.Protest;
			}
		}

		protected override bool DeclarationMessagesHaveBeenSentCore(bool reloadMessages)
		{
			foreach (CusEntryHeader entry in ActiveEntryHeaders)
			{
				var nonBirdMessages = entry.Messages.OfType<MQEDIMessage>().Where(x => !x.IsBIRDTransaction);
				if (nonBirdMessages.Any())
				{
					return true;
				}
			}

			return false;
		}

		protected override string GetReasonForNotAbleToUpdateCore()
		{
			var result = string.Empty;
			if (IsImport)
			{
				var entrySummaryEntry = ActiveEntryHeaders.EntrySummaryEntry;
				if (entrySummaryEntry != null && entrySummaryEntry.Messages.OfType<MQEDIMessage>().Any(x => !x.IsBIRDTransaction) && DeclarationMessagesHaveBeenSent())
				{
					var cannotUpdateAsMessageHasBeenSent = Res.GetString("3f06e513-29ac-421b-8b4f-89c5e466be9d", "The communication process with Customs has started for this Declaration.");
					return string.Format(CultureInfo.InvariantCulture, "{0} Job Number: {1}", cannotUpdateAsMessageHasBeenSent, JobNumber); // to show error messges
				}
			}
			return result;
		}

		public bool IsACE
		{
			get { return IsImport && (JE_ApplicationCode == JobApplicationCodeList.Codes.ACE || (IsFormalImport && JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Interfaced)); }
		}

		public ZBool IsSplitShipment
		{
			get { return IsImport && Bills.Any(x => ((Bill)x).US_SESplitShip); }
		}

		public bool AreSplitDetailsRelevant
		{
			get { return IsFTZSplitDetailsRelevant || IsImport && IsACECargoCertificationMode && !IsSea && !IsConsumptionFTZ; }
		}

		public bool IsFTZSplitDetailsRelevant
		{
			get { return IsRegularFTZAdmission && IsAir; }
		}

		public bool IsNonAMSRelevant
		{
			get { return IsImport && IsACECargoCertificationMode && !IsReWarehouse; }
		}

		public bool IsACECargoCertificationAndFixedTransportRelevant
		{
			get { return IsImport && IsACECargoCertificationMode && IsFixedTransportInstallations; }
		}

		public bool IsACECargoRelease
		{
			get { return IsACE && US_EnableCRL && US_CargoReleaseType == CargoReleaseTypeList.Codes.SE; }
		}

		public bool IsACECargoCertificationMode
		{
			get { return (IsImport && !IsFTZAdmission || IsAdvanceShippingNotice) && !IsACSCargoCertificationMode; }
		}

		public bool IsACSCargoCertificationMode
		{
			get { return (IsImport && JE_ApplicationCode == JobApplicationCodeList.Codes.ACS) || (IsACE && US_CargoReleaseType == CargoReleaseTypeList.Codes.ACS); }
		}

		public bool IsImportENSORCRLOrASN
		{
			get { return IsImport && !IsFTZAdmission && (US_EnableENS || US_EnableCRL) || IsAdvanceShippingNotice; }
		}

		public static bool IsValidFTZRoutingDetails(string routingDetails)
		{
			return Regex.IsMatch(routingDetails, Constants.FTZRoutingDetailsFormat);
		}

		bool IsAdvanceShippingNotice
		{
			get { return !IsPersistent && JE_MessageType == JobMessageTypeList.MoreCodes.AdvanceShippingNotice; }
		}

		internal bool ShouldPrintProductNumber7501
		{
			get
			{
				return (JE_MergeBy == OrgConstants.MergeInvoiceLines.NotMerge ||
					JE_MergeBy == OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription ||
					JE_MergeBy == OrgConstants.MergeInvoiceLines.PartNumber ||
					JE_MergeBy == OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription) &&
					ImporterWrapper != null && ImporterWrapper.ZO_ENSPrintProduct;
			}
		}

		public bool CopyLastFDADetailsToNewLine;
		public bool CopyLastPGADetailsToNewLine;

		public bool IsQuota
		{
			get { return US_EntryType == EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa || US_EntryType == EntryTypeList.Codes.ConsumptionQuotaVisa; }
		}

		public bool IsLiveEntry
		{
			get { return US_LiveEntryIndicator == YesNoDefaultList.Codes.Yes; }
		}

		public bool IsLineGroupingSupported
		{
			get { return IsImport && (US_EnableAII || US_IsInvoiceByRequest); }
		}

		public bool IsWarehouseEntryType
		{
			get { return (IsENSFormalImport || IsFTZAdmission) && EntryTypeList.IsWarehouseType(US_EntryType); }
		}

		public bool IsExWarehouseEntryType
		{
			get { return IsENSFormalImport && EntryTypeList.IsExWarehouseType(US_EntryType); }
		}

		public bool IsENSFormalImportAndConsumptionFTZ
		{
			get { return IsENSFormalImport && IsConsumptionFTZ; }
		}

		public bool IsSoldEnRoute
		{
			get { return IsExport && US_SoldEnRouteIndicator == YesNoDefaultList.Codes.Yes; }
		}

		public bool IsUSTerritoryTreatedAsDomesticState
		{
			get { return IsExport && (US_SchDLoading.StartsWith("49") || US_SchDLoading.StartsWith("51")); } // Valid Export jobs from Puerto Rico or Virgin Island to US
		}

		public bool IsConsumptionFTZ
		{
			get { return (US_EntryType == EntryTypeList.Codes.ConsumptionFTZ) && IsFormalImport; }
		}

		public bool IsTemporaryImportationBond
		{
			get { return US_EntryType == EntryTypeList.Codes.TemporaryImportationBond; }
		}

		public bool IsRemoteLocationFiling
		{
			get { return US_EntryMode == EntryModeList.Codes.RLF; }
		}

		public bool IsPairedPortProgram
		{
			get { return US_EntryMode == EntryModeList.Codes.Paired; }
		}

		public bool IsCommercialShipment
		{
			get { return !IsPersonalShipment; }
		}

		public bool IsPersonalShipment
		{
			get { return US_ConsolidatedInformalIndicator == ConsolidatedInformalList.Codes.Personal; }
		}

		public bool IsSampleShipment
		{
			get { return US_ConsolidatedInformalIndicator == ConsolidatedInformalList.Codes.Samples; }
		}

		public bool IsIndividualBasisPayment
		{
			get { return US_PaymentType == PaymentTypeList.Codes.IndividualBasis; }
		}

		public bool IsManualPayment
		{
			get { return US_PaymentType.IsEmpty || IsIndividualBasisPayment; }
		}

		public bool IsReWarehouse
		{
			get { return IsImport && US_EntryType == EntryTypeList.Codes.ReWarehouse; }
		}

		public bool IsRelevantFor(ZString cH_MessageType)
		{
			switch (cH_MessageType)
			{
				case CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease:
					return IsImport && US_EnableCRL && IsBorderMovement && !IsACE;
				case CusEntryHeaderMessageTypeList.Codes.CargoRelease:
					return IsImport && US_EnableCRL && !IsBorderMovement && !IsACE;
				case CusEntryHeaderMessageTypeList.Codes.EntrySummary:
					return IsENSFormalImport;
				case CusEntryHeaderMessageTypeList.Codes.InBond:
					return IsInBond;
				case CusEntryHeaderMessageTypeList.Codes.Export:
					return IsExport;
				case CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone:
					return IsFTZAdmission;
				case CusEntryHeaderMessageTypeList.Codes.ACECargoRelease:
					return IsACECargoCertificationMode;
				default:
					return false;
			}
		}

		public bool IsSchDArrivalAllowed
		{
			get { return !IsExport || IsSea || (IsAir && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(US_RN_NKCountryOfDestination) == Core.Constants.CountryCodes.UnitedStates); }
		}

		public bool IsSchDPortOfLoadingRequired
		{
			get { return IsSea || IsBorderWaterBorne; }
		}

		public bool HasMultipleFWSExporters
		{
			get
			{
				if (hasMultipleFWSExportersCached == null)
				{
					hasMultipleFWSExportersCached = new CachedProperty<bool>(Factory, () =>
					{
						ZGuid? firstExporterFound = null;
						foreach (JobComInvoiceHeader invoice in Invoices)
						{
							foreach (JobComInvoiceLine line in invoice.InvoiceLines)
							{
								if (!firstExporterFound.HasValue || firstExporterFound.Value.IsEmpty)
								{
									firstExporterFound = line.FWSHeaders.OfType<FWSHeader>()
										.FirstOrDefault(x => !x.US_OA_FWSExporterAddress.IsEmpty)
										?.US_OA_FWSExporterAddress;
								}

								if (firstExporterFound.HasValue && line.FWSHeaders.OfType<FWSHeader>()
									.Any(x => !x.US_OA_FWSExporterAddress.IsEmpty && x.US_OA_FWSExporterAddress != firstExporterFound.Value))
								{
									return true;
								}
							}
						}

						return false;
					});
				}

				return hasMultipleFWSExportersCached.Value;
			}
		}
		CachedProperty<bool> hasMultipleFWSExportersCached;

		public bool HasMultipleSEDs
		{
			get { return IsExport && ActiveEntryHeaders.Count > 1; }
		}

		public bool ShouldDefaultCountriesOfOriginAndExportForImport
		{
			get { return IsImport && USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty) && !ShouldSynchroniseWithShipment(); }
		}

		public bool HasFDAReportingRequirements
		{
			get
			{
				return InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.HasFDAReportingRequirement);
			}
		}

		public bool IsUSMLEntry
		{
			get
			{
				if (isUSMLEntryCached == null)
				{
					isUSMLEntryCached = new CachedProperty<bool>(Factory, delegate
					{
						bool usmlEntry = !US_DDTCUSMLCategoryCode.IsEmpty;
						if (!usmlEntry)
						{
							foreach (JobComInvoiceLine invoiceLine in Declaration.InvoiceLines)
							{
								if (!invoiceLine.US_DDTCUSMLCategoryCode.IsEmpty) // these is an effective field
								{
									usmlEntry = true;
									break;
								}
							}
						}

						return usmlEntry;
					});
				}
				return isUSMLEntryCached.Value;
			}
		}
		CachedProperty<bool> isUSMLEntryCached;

		public void MarkInvoiceLineAsNeedingValidationIncludingChildren()
		{
			LoadInvoiceLinesChildrenIfNeeded();
			InvoiceLines.MarkAsNeedingValidationIncludingChildren();
		}

		public void LoadInvoiceLinesChildrenForPossiblyDeletionIfNeeded()
		{
			if (!hasLoadedInvoiceLinesChildrenForPossiblyDeletion)
			{
				hasLoadedInvoiceLinesChildrenForPossiblyDeletion = true;
				hasLoadedInvoiceLinesChildren = true;
				LoadInvoiceLinesChildrenCore((b) =>
				{
					b.FetchStrategy.FetchForDelete();
				});
			}
		}
		bool hasLoadedInvoiceLinesChildrenForPossiblyDeletion;

		public void LoadInvoiceLinesChildrenIfNeeded()
		{
			if (!hasLoadedInvoiceLinesChildren)
			{
				hasLoadedInvoiceLinesChildren = true;
				LoadInvoiceLinesChildrenCore();
			}
		}
		bool hasLoadedInvoiceLinesChildren;

		void LoadInvoiceLinesChildrenCore(Action<BusinessObject> addAdditionalFetchHints = null)
		{
			AddJobComInvoiceLineFetchHintsIfNeeded();
			LoadChildEditableObjectsForChild(InvoiceLines.Where(x => x.IsInDatabase).ToArray(), addAdditionalFetchHints: addAdditionalFetchHints);
		}
		public bool CanHavePGAFDA
		{
			get
			{
				return IsImport
					&& (
						IsACECargoCertificationMode
						|| IsACE && US_EnableSPN && !IsImportENSORCRLOrASN
						|| IsACE && US_CargoReleaseType.IsEmpty
						|| IsFTZAdmission && US_EnableSPN && US_F_PNMode == PriorNoticeModeCodeList.Codes.P
						);
			}
		}

		public bool IsGovernmentImporter
		{
			get
			{
				OrgHeader importer = Importer;
				return importer != null && ImporterTypeList.IsGovernmentImporter(OrgHeaderWrapper.New(importer).ZO_ImporterType);
			}
		}

		public override ZBool IsContainerised
		{
			get { return Core.Constants.ContainerModes.IsContainerised(JE_ContainerMode); }
		}

		public bool IsACEAutoRoadAndPedTransportMode
		{
			get { return IsACE && (JE_TransportMode == TransportTypeList.Codes.Auto || JE_TransportMode == TransportTypeList.Codes.Road || JE_TransportMode == TransportTypeList.Codes.Pedestrian); }
		}

		public override ZBool IsRoad
		{
			get { return JE_TransportMode == TransportTypeList.Codes.Auto || JE_TransportMode == TransportTypeList.Codes.Road || JE_TransportMode == TransportTypeList.Codes.Truck; }
		}

		public bool IsTruck
		{
			get { return JE_TransportMode == TransportTypeList.Codes.Truck; }
		}

		public bool IsHandCarry
		{
			get { return JE_TransportMode == TransportTypeList.Codes.PassengerHandCarried; }
		}

		public bool IsBorderWaterBorne
		{
			get { return JE_TransportMode == TransportTypeList.Codes.BorderWaterBorne; }
		}

		public bool IsFixedTransportInstallations
		{
			get { return JE_TransportMode == TransportTypeList.Codes.FixedTransportInstallations; }
		}

		public bool IsHandCarryAir
		{
			get { return IsHandCarry && JE_MasterBillIssuerSCAC.Length == 2; }
		}

		public bool IsHandCarryNonAir => IsHandCarry && JE_MasterBillIssuerSCAC.Length != 2;

		public bool IsBorderMovement
		{
			get { return US_CargoReleaseType == CargoReleaseTypeList.Codes.BCR; }
		}

		public bool IsElectronicInvoice
		{
			get { return IsImport && US_EnableAII; }
		}

		public bool IsENSFormalImport
		{
			get { return IsImport && US_EnableENS; }
		}

		public bool IsInBond
		{
			get { return IsImport && US_EnableINB; }
		}

		public ZBool HasAutoCondition
		{
			get
			{
				CachedProperty<ZBool> cached = cachedHasAutoCondition ?? (cachedHasAutoCondition = new CachedProperty<ZBool>(Factory, () => (ZBool)Declaration.InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.SupTariffMatchesAutoCondition)));
				return cached.Value;
			}
		}
		CachedProperty<ZBool> cachedHasAutoCondition;

		public bool IsCargoRelease
		{
			get { return IsImport && US_EnableCRL; }
		}

		public bool IsCargoReleaseWithoutFormalEntry
		{
			get { return IsCargoRelease && !US_EnableENS; }
		}

		public bool IsSimplifiedEntryWithoutFormalEntry
		{
			get { return IsACECargoRelease && !US_EnableENS; }
		}

		public bool IsInBondOnly
		{
			get { return IsImport && US_EnableINB && !US_EnableCRL && !US_EnableENS; }
		}

		public bool NoImportMessagingModesEnabled
		{
			get { return IsFormalImport && !US_EnableENS && !US_EnableCRL && !US_EnableINB; }
		}

		public bool IsEntryFilerCodeRequired
		{
			get { return (IsImport && !IsInBondOnly) || IsDrawback; }
		}

		public bool IsWithoutBondType
		{
			get { return US_BondType == BondTypeList.Codes.NoBondRequired || US_BondType.IsEmpty; }
		}

		public ZBool IsSingleTransactionBond
		{
			get { return US_BondType == BondTypeList.Codes.SingleTransactionBond; }
		}

		public ZBool IsAdditionalSingleTransactionBond
		{
			get { return US_BondType2 == BondTypeList.Codes.SingleTransactionBond; }
		}

		public ZBool FDACCNStateCodeVisible
		{
			get { return (US_FDACANType == FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle); }
		}

		public bool IsHMFApplicable
		{
			get { return US_IsHMFApplicable == YesNoDefaultList.Codes.Yes; }
		}

		public ZBool IsPGAEntryHasBeenLodgedAtCustoms
		{
			get
			{
				if (US_PGAExpeditedRelease)
				{
					return FormalEntry != null && FormalEntry.HasBeenLodgedAtCustoms;
				}
				else
				{
					var seEntry = ActiveEntryHeaders.SimplifiedEntry;
					return seEntry != null && seEntry.HasBeenLodgedAtCustoms;
				}
			}
		}

		public ZString PGAExpeditedInEntrySummaryIndicator
		{
			get
			{
				if (cachedPGAExpeditedInEntrySummaryIndicator == null)
				{
					cachedPGAExpeditedInEntrySummaryIndicator = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;
						if (IsWeeklyEstimateConsumptionFTZ)
						{
							result = "F";
						}
						else if (US_PGAExpeditedRelease || EntryTypeList.IsExWarehouseOrReWarehouseType(US_EntryType) && PGAFlags.HasInvoiceLinesWithPGA)
						{
							result = "Y";
						}

						return result;
					});
				}

				return cachedPGAExpeditedInEntrySummaryIndicator.Value;
			}
		}
		CachedProperty<ZString> cachedPGAExpeditedInEntrySummaryIndicator;

		/// <summary>
		/// ***Use with caution***
		/// A major bottleneck when there are a lot of invoice lines
		/// </summary>
		public bool HasLineLevelUltimateConsignees
		{
			get
			{
				if (hasLineLevelUltimateConsigneesCached == null)
				{
					hasLineLevelUltimateConsigneesCached = new CachedProperty<bool>(Factory, delegate
					{
						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							ZGuid lineUltimateConsignee = invoiceLine.JI_OA_ConsigneeAddress;

							if (!lineUltimateConsignee.IsEmpty && lineUltimateConsignee != JE_OA_ConsigneeAddress)
							{
								return true;
							}
						}
						return false;
					});
				}
				return hasLineLevelUltimateConsigneesCached.Value;
			}
		}
		CachedProperty<bool> hasLineLevelUltimateConsigneesCached;

		public bool CarrierCodeMarkedForRequest { get; set; }

		public bool IsCarrierCodeCandidateForUpdate
		{
			get { return !US_UI_NKCarrierSCAC.IsEmpty && StringChecker.IsLettersAndNumbersAndSpaces(US_UI_NKCarrierSCAC) && ImportingCarrier == null; }
		}

		public USCarrierCombined ImportingCarrier
		{
			get
			{
				USCarrierCombined result = null;
				if (!US_UI_NKCarrierSCAC.IsEmpty)
				{
					result = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, US_UI_NKCarrierSCAC));
				}
				return result;
			}
		}

		#region Ultimate Consignee
		public IAddressDetails EffectiveUltimateConsigneeWrapperAddressDetails
		{
			get { return ConsigneeOrgAddress.GetCustomsAddressDetailsFallingBackToMainAddress(); }
		}

		public ZString EffectiveUltimateConsigneeAddressLine2
		{
			get
			{
				var result = ZString.Empty;

				var ultConsigneeAddressDetails = EffectiveUltimateConsigneeWrapperAddressDetails;
				if (ultConsigneeAddressDetails != null)
				{
					result = ultConsigneeAddressDetails.AddressLine2.IsEmpty ? EUCCityStatePostCodeCountry : ultConsigneeAddressDetails.AddressLine2;
				}

				return result;
			}
		}

		public ZString EffectiveUltimateConsigneeCityStatePostCodeCountry
		{
			get
			{
				var ultConsigneeAddressDetails = EffectiveUltimateConsigneeWrapperAddressDetails;
				return (ultConsigneeAddressDetails != null && !ultConsigneeAddressDetails.AddressLine2.IsEmpty) ? EUCCityStatePostCodeCountry : ZString.Empty;
			}
		}

		ZString EUCCityStatePostCodeCountry
		{
			get
			{
				var builder = new ZStringBuilder();
				var ultConsigneeAddressDetails = EffectiveUltimateConsigneeWrapperAddressDetails;
				if (ultConsigneeAddressDetails != null)
				{
					builder.AppendIfNotEmpty(ultConsigneeAddressDetails.City);
					builder.AppendIfNotEmpty(ultConsigneeAddressDetails.State);
					builder.AppendIfNotEmpty(ultConsigneeAddressDetails.PostCode);
					builder.AppendIfNotEmpty(ultConsigneeAddressDetails.Country);
				}
				return builder.ToStringWithDelimiterBetweenAppends("   ");
			}
		}

		public bool IsUSUltimateConsignee
		{
			get { return ConsigneeOrgAddress != null && ConsigneeOrgAddress.OH_RL_NKClosestPort.StartsWith(Core.Constants.CountryCodes.UnitedStates); }
		}
		#endregion

		public override ZBool IsImport
		{
			get { return base.IsImport || JobMessageTypeList.IsImport(JE_MessageType); }
		}

		public override bool IsImportByExternalBroker
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.ImportByExternalBroker; }
		}

		public override bool ShouldAutoRatingForExternalBroker => IsImport;

		public ZBool IsFormalImport
		{
			get { return base.IsImport || JE_MessageType == JobMessageTypeList.Codes.Miscellaneous; }
		}

		public bool IsMasterBillRelevant
		{
			get { return TransportTypeList.IsMasterBillRelevant(JE_TransportMode); }
		}

		/// <param name="birdApplicationID">Either 7501 or 3461</param>
		public bool CreatedViaBIRD(string birdApplicationID)
		{
			ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImportCode);
			query.AddToFilter(StmALogSchema.SL_Reference, ApplicationIdentifierCodeList.Codes.BIRDTransaction + ":" + birdApplicationID);
			return Logs.Find(query).Length > 0;
		}

		internal bool IsHouseBillSCACRequired
		{
			get { return US_EntryType != EntryTypeList.Codes.ConsumptionFTZ && TransportTypeList.IsHouseBillSCACMandatory(JE_TransportMode); }
		}

		internal bool IsMasterBillSCACRequired
		{
			get { return US_EntryType != EntryTypeList.Codes.ReWarehouse && TransportTypeList.IsMasterBillSCACMandatory(JE_TransportMode); }
		}

		public bool ShowDeactivatedEntries
		{
			get { return showDeactivatedEntries && IsExport; }
			set
			{
				var shouldRefresh = showDeactivatedEntries != value;
				showDeactivatedEntries = value;
				if (shouldRefresh)
				{
					ActiveBusinessObjectCollection<CusEntryHeader>.RefreshAll(Factory);
				}
			}
		}
		bool showDeactivatedEntries = true;

		internal ZBool TaxToBeDeferred
		{
			get { return US_TaxDeferIndicator == TaxDeferIndicatorList.Codes.DeferredTax; }
		}

		internal ZBool DeferredTaxToBePaidByEFT
		{
			get { return US_TaxDeferIndicator == TaxDeferIndicatorList.Codes.DeferredTaxWithEFT; }
		}

		internal ZBool TaxDeferred
		{
			get { return TaxToBeDeferred || DeferredTaxToBePaidByEFT; }
		}
		internal bool IsEstimatedEnteredValueRequired
		{
			get
			{
				return IsCargoReleaseWithoutFormalEntry &&
					(IsACECargoRelease || (IsCargoRelease && !IsBorderMovement)) &&
					Invoices.Sum(x => x.JZ_InvoiceAmount) == ZDecimal.Zero;
			}
		}

		public ZBool IsACEENTStandAlonePriorNotice
		{
			get { return IsACE && IsStandAlonePriorNoticeMode && US_SPNIDType == StandAlonePriorNoticeIDTypeList.Codes.ENT; }
		}

		public ZBool IsACEBLNStandAlonePriorNotice
		{
			get { return IsACE && IsStandAlonePriorNoticeMode && US_SPNIDType == StandAlonePriorNoticeIDTypeList.Codes.BLN; }
		}

		public ZBool IsFTZFTZStandAlonePriorNotice
		{
			get { return IsFTZPGAStandAlonePriorNotice && US_SPNIDType == StandAlonePriorNoticeIDTypeList.Codes.FTZ; }
		}

		public ZBool IsFTZBLNStandAlonePriorNotice
		{
			get { return IsFTZPGAStandAlonePriorNotice && US_SPNIDType == StandAlonePriorNoticeIDTypeList.Codes.BLN; }
		}

		protected override ZBool IsBillIssueDateVisibleCore
		{
			get { return false; }
		}

		public ZBool IsLowValue
		{
			get { return US_EntryType == EntryTypeList.Codes.LowValue; }
		}

		internal bool IsSeaAndIsAMSHBREffective
		{
			get { return IsSea && ZZCustomsFunctionality.IsAMSHBREffective; }
		}

		#endregion

		#region New Properties

		#region Invoicer Address

		public JobDocAddress InvoicerDocumentaryAddress
		{
			get
			{
				if (fInvoicerDocumentaryAddress == null || fInvoicerDocumentaryAddress.IsDeleted)
				{
					fInvoicerDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.InvoicerAddress);
					fInvoicerDocumentaryAddress.OnRelationshipFieldsChanged += new EventHandler(InvoicerDocumentaryAddress_OnRelationshipFieldsChanged);
				}
				return fInvoicerDocumentaryAddress;
			}
		}
		JobDocAddress fInvoicerDocumentaryAddress;

		void InvoicerDocumentaryAddress_OnRelationshipFieldsChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
			Declaration.Invoices.MarkAsNeedingValidation();
			Declaration.InvoiceLines.MarkAsNeedingValidation();
		}

		[RelatedBusinessObject(nameof(InvoicerAddress))]
		[List(nameof(JE_OA_InvoicerAddress_ZAddress) + "+" + nameof(ZAddress.OrgAddress_List))]
		public ZGuid JE_OA_InvoicerAddress
		{
			get
			{
				return InvoicerDocumentaryAddress.E2_OA_Address;
			}
			set
			{
				var oldValue = JE_OA_InvoicerAddress;
				InvoicerDocumentaryAddress.E2_OA_Address = value;

				if (!IsCopying && oldValue != JE_OA_InvoicerAddress)
				{
					ClearInvoiceInvoicerDocumentaryAddressIfSame(value);
					Declaration.Invoices.MarkAsNeedingValidation();
				}

				Validation.ValidateJE_OA_InvoicerAddress();
				JE_OA_InvoicerAddressInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JE_OA_InvoicerAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JE_OA_InvoicerAddress); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress JE_OA_InvoicerAddress_ZAddress
		{
			get
			{
				if (fJE_OA_InvoicerAddress_ZAddress == null)
				{
					fJE_OA_InvoicerAddress_ZAddress = new ZAddress(JE_OA_InvoicerAddressInfo);
					fJE_OA_InvoicerAddress_ZAddress.IsOrgVisible = true;
					fJE_OA_InvoicerAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
					fJE_OA_InvoicerAddress_ZAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;
				}
				return fJE_OA_InvoicerAddress_ZAddress;
			}
		}
		ZAddress fJE_OA_InvoicerAddress_ZAddress;

		public OrgAddress InvoicerAddress
		{
			get { return Factory.Load<OrgAddress>(JE_OA_InvoicerAddress); }
		}

		void ClearInvoiceInvoicerDocumentaryAddressIfSame(ZGuid invoicerAddress)
		{
			foreach (var invoice in Invoices.OfType<JobComInvoiceHeader>())
			{
				var invoiceInvoicerAddress = invoice.InvoicerDocumentaryAddress.E2_OA_Address;
				if (!invoiceInvoicerAddress.IsEmpty && invoicerAddress == invoiceInvoicerAddress)
				{
					invoice.JZ_OA_InvoicerDocAddress = ZGuid.Empty;
				}
				else
				{
					invoice.JZ_OA_InvoicerDocAddressInfo.RefreshBinding();
					invoice.JZ_OA_FDAShipperAddressInfo.RefreshBinding();
				}
			}
		}

		#endregion

		public ZBool ShouldUpdateConsolidatedJobNo { get; set; }

		public ZString VesselName
		{
			get { return IsSea ? JE_VesselName : ZString.Empty; }
		}

		#region WHSInvLineFilter
		[List(nameof(PackableInvoiceLines))]
		public ZGuid WHSInvLineFilter
		{
			get { return whsInvLineFilter; }
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (SetNonPersistentPropertyValue(WHSInvLineFilterInfo, ref whsInvLineFilter, value))
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateWHSInvLineFilter();
						}
						WHSInvLineFilterInfo.RefreshBinding();
						WHSPackFilteredLines.Rebuild();
					}
				}
			}
		}
		ZGuid whsInvLineFilter;

		public ZPropertyInfo WHSInvLineFilterInfo
		{
			get { return GetZPropertyInfo(Schema.WHSInvLineFilter); }
		}
		#endregion

		#region WHSProductFilter
		[List(nameof(PackableInvoiceLines) + "." + nameof(PackableInvoiceLineAdhocCollection.UniqueProductList))]
		[MaxLength(JobComInvoiceLine.Schema.JI_PartNoMaxLength)]
		public ZString WHSProductFilter
		{
			get { return whsProductFilter; }
			set
			{
				using (SuspendSettingHasChanges())
				{
					value = value.TrimEnd(' ', '\t');
					CheckMaximumLength(WHSProductFilterInfo, value);
					if (SetNonPersistentPropertyValue(WHSProductFilterInfo, ref whsProductFilter, value))
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateWHSProductFilter();
						}
						WHSProductFilterInfo.RefreshBinding();
						WHSPackFilteredLines.Rebuild();
					}
				}
			}
		}
		ZString whsProductFilter;

		public ZPropertyInfo WHSProductFilterInfo
		{
			get { return GetZPropertyInfo(Schema.WHSProductFilter); }
		}
		#endregion

		#region WHSPackageFilter
		[List(nameof(WHSPacks))]
		public ZGuid WHSPackageFilter
		{
			get { return whsPackageFilter; }
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (SetNonPersistentPropertyValue(WHSPackageFilterInfo, ref whsPackageFilter, value))
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateWHSPackageFilter();
						}
						WHSPackageFilterInfo.RefreshBinding();
						WHSPackFilteredLines.Rebuild();
					}
				}
			}
		}
		ZGuid whsPackageFilter;

		public ZPropertyInfo WHSPackageFilterInfo
		{
			get { return GetZPropertyInfo(Schema.WHSPackageFilter); }
		}
		#endregion

		public ZBool ShouldSaveEDocsMasterFactoryTogether { get; set; }

		#region CBPF4811ReferenceNumber

		public ZString CBPF4811ReferenceNumber
		{
			get
			{
				var result = ZString.Empty;
				var notifyParty = this.NotifyParty;
				if (notifyParty != null)
				{
					result = notifyParty.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates,
						OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber);
				}
				else
				{
					var wrapper = this.IsDrawback ? this.ImporterWrapper : this.IORWrapper;
					if (wrapper != null)
					{
						result = wrapper.ZO_NPID;
					}
				}
				return result;
			}
		}

		#endregion

		public ZBool IsPGATrackingEnabled
		{
			get
			{
				if (!isPGATrackingEnabledCached.HasValue)
				{
					isPGATrackingEnabledCached = this.GetSystemDefinedValue<ZBool>(Constants.GenAddOnColumnFieldName.IsPGATrackingEnabled);
				}
				return isPGATrackingEnabledCached.Value;
			}
			set
			{
				if (isPGATrackingEnabledCached != value)
				{
					isPGATrackingEnabledCached = value;
					this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.IsPGATrackingEnabled, isPGATrackingEnabledCached);
				}
			}
		}
		ZBool? isPGATrackingEnabledCached;

		public ZDateTime US_PSDAccepted
		{
			get
			{
				var data = this.GetSystemDefinedValue<ZString>(Constants.GenAddOnColumnFieldName.US_PSDAccepted);
				var result = ZDateTime.Empty;
				if (!data.IsEmpty)
				{
					ZDateTime.TryParseISO8601Date(data, out result);
				}
				return result;
			}
			set
			{
				var data = value.IsValid ? value.SqlFormat : ZString.Empty;
				this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.US_PSDAccepted, data);
			}
		}

		internal bool IsWeeklyEstimateConsumptionFTZ
		{
			get { return IsConsumptionFTZ && IsWeeklyEstimateFilingDate; }
		}

		protected override bool IsBondedWarehousePermitEnabledCore
		{
			get { return IsWeeklyEstimateConsumptionFTZ; }
		}

		public bool IsPGAValidationOn()
		{
			var result = IsACE && US_PGAExpeditedRelease;

			if (IsExWarehouseEntryType)
			{
				result |= IsEntrySummaryValidationMode;
			}
			else if (IsLowValue)
			{
				result |= IsACECargoRelease;
			}
			else
			{
				result |= IsACECargoReleaseValidationMode;
			}

			return result;
		}

		public ZDate EffectiveDateForBond
		{
			get { return new DutyFeeDateCalculator().GetDutyFeeDateForBond(this); }
		}

		internal ZDateTime DateForMPFCalculation
		{
			get { return new DutyFeeDateCalculator().GetDutyFeeDateForMPF(this); }
		}

		internal ZDateTime DateForFeeCalculation
		{
			get { return new DutyFeeDateCalculator().GetDutyFeeDate(this); }
		}

		internal bool IsChargeMandatory
		{
			get
			{
				return TransportMode != TransportTypeList.Codes.PassengerHandCarried
					&& !EntryTypeList.IsInformal(US_EntryType);
			}
		}

		public bool HasABondedWarehousingLineWithoutWHSPackDetails
		{
			get
			{
				if (hasABondedWarehousingLineWithoutWHSPackDetailsCached == null)
				{
					hasABondedWarehousingLineWithoutWHSPackDetailsCached = new CachedProperty<bool>(Factory, GetHasABondedWarehousingLineWithoutWHSPackDetails);
				}
				return hasABondedWarehousingLineWithoutWHSPackDetailsCached.Value;
			}
		}
		CachedProperty<bool> hasABondedWarehousingLineWithoutWHSPackDetailsCached;

		bool GetHasABondedWarehousingLineWithoutWHSPackDetails()
		{
			var result = IsWHSUniversalXMLActive && IsInwardBondedWarehousingEnabled;
			if (result)
			{
				result = false;
				var isExWarehouse = IsExWarehouse;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					if (invoiceLine.JI_PartNo_CanBeSetByCustomer && invoiceLine.SupplierPart != null && invoiceLine.JI_Calc_BondedWhsQuantity.IsEmpty)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		#region US_7512OpenArea

		public virtual ZString US_7512OpenArea
		{
			get { return ITDoc.US_7512OpenArea; }
			set
			{
				var oldValue = ITDoc.US_7512OpenArea;
				ITDoc.US_7512OpenArea = value;
				if (!IsCopying && oldValue != ITDoc.US_7512OpenArea)
				{
					ClearBillValuesIfSame(ITDoc.US_7512OpenArea);
				}
			}
		}

		void ClearBillValuesIfSame(ZString decValue)
		{
			if (IsPersistent)
			{
				foreach (Bill bill in Bills)
				{
					ZString billValue = bill.US_7512OpenArea;

					if (billValue.Equals(decValue))
					{
						bill.US_7512OpenArea = ZString.Empty;
					}
				}
			}
		}

		ITDoc ITDoc
		{
			get
			{
				if (fITDoc == null || fITDoc.IsDeleted)
				{
					if (fITDoc != null)
					{
						UnRegisterEditableChildObject(fITDoc);
					}
					fITDoc = LoadOrCreateITDoc(Factory, IsInDatabase, PK, TablePrefix);
					RegisterEditableChildObject(fITDoc);
				}
				return fITDoc;
			}
		}
		ITDoc fITDoc;

		internal static ITDoc LoadOrCreateITDoc(BusinessObjectFactory factory, bool isInDatabase, ZGuid pK, string tablePrefix)
		{
			var query = new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USITDoc);
			query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, tablePrefix);
			query.AddToFilter(CusAddInfoSchema.B7_ParentID, pK);
			query.FetchOnlyFromLocalCache = !isInDatabase;
			var result = factory.LoadTop1<ITDoc>(query);
			if (result == null)
			{
				result = factory.New<ITDoc>();
				using (result.SuspendSettingHasChanges())
				{
					result.B7_ParentTableCode = tablePrefix;
					result.B7_ParentID = pK;
				}
			}
			return result;
		}

		#endregion

		#region Audit Logs

		StmALog GetAnAuditLog(ZQuery filter)
		{
			foreach (StmALog log in AuditLogs)
			{
				if (log.MatchesFilter(filter))
				{
					return log;
				}
			}
			return null;
		}

		LogsForNominatedEvent AuditLogs
		{
			get { return auditLogs ?? (auditLogs = new LogsForNominatedEvent(Logs, Events.RecordAudited, true)); }
		}
		LogsForNominatedEvent auditLogs;

		public ZDateTime SPIAuditDate
		{
			get { return SPIAuditLog != null ? SPIAuditLog.SL_EventTime : ZDateTime.Empty; }
		}

		public ZString SPIAuditUser
		{
			get { return SPIAuditLog != null ? SPIAuditLog.SL_GS_NKUser : ZString.Empty; }
		}

		public ZString SPIAuditReference
		{
			get { return SPIAuditLog != null ? SPIAuditLog.SL_Reference : ZString.Empty; }
		}

		public ZString SPIAuditUserName
		{
			get { return SPIAuditLog != null && SPIAuditLog.User != null ? SPIAuditLog.User.GS_FullName : ZString.Empty; }
		}

		public ZDateTime CWAuditDate
		{
			get { return CWAuditLog != null ? CWAuditLog.SL_EventTime : ZDateTime.Empty; }
		}

		public ZString CWAuditUser
		{
			get { return CWAuditLog != null ? CWAuditLog.SL_GS_NKUser : ZString.Empty; }
		}

		public ZString CWAuditReference
		{
			get { return CWAuditLog != null ? CWAuditLog.SL_Reference : ZString.Empty; }
		}

		public ZString CWAuditUserName
		{
			get { return CWAuditLog != null && CWAuditLog.User != null ? CWAuditLog.User.GS_FullName : ZString.Empty; }
		}

		public ZDateTime FDAAuditDate
		{
			get { return FDAAuditLog != null ? FDAAuditLog.SL_EventTime : ZDateTime.Empty; }
		}

		public ZString FDAAuditUser
		{
			get { return FDAAuditLog != null ? FDAAuditLog.SL_GS_NKUser : ZString.Empty; }
		}

		public ZString FDAAuditReference
		{
			get { return FDAAuditLog != null ? FDAAuditLog.SL_Reference : ZString.Empty; }
		}

		public ZString FDAAuditUserName
		{
			get { return FDAAuditLog != null && FDAAuditLog.User != null ? FDAAuditLog.User.GS_FullName : ZString.Empty; }
		}

		StmALog SPIAuditLog
		{
			get { return spuAuditLog ?? (spuAuditLog = GetAnAuditLog(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + AuditFieldsList.Codes.SPI))); }
		}
		StmALog spuAuditLog;

		StmALog FDAAuditLog
		{
			get { return fdaAuditLog ?? (fdaAuditLog = GetAnAuditLog(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + AuditFieldsList.Codes.FDA))); }
		}
		StmALog fdaAuditLog;

		StmALog CWAuditLog
		{
			get { return cwAuditLog ?? (cwAuditLog = GetAnAuditLog(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + AuditFieldsList.Codes.CensusWarning))); }
		}
		StmALog cwAuditLog;

		public ZDateTime TIBClosedDate
		{
			get { return TIBClosedLog != null ? TIBClosedLog.SL_EventTime : ZDateTime.Empty; }
		}

		public ZString TIBClosedUser
		{
			get { return TIBClosedLog != null ? TIBClosedLog.SL_GS_NKUser : ZString.Empty; }
		}

		public ZString TIBClosedUserName
		{
			get { return TIBClosedLog != null && TIBClosedLog.User != null ? TIBClosedLog.User.GS_FullName : ZString.Empty; }
		}

		public ZString TIBClosedReference
		{
			get { return TIBClosedLog != null ? TIBClosedLog.SL_Reference : ZString.Empty; }
		}

		StmALog TIBClosedLog
		{
			get
			{
				return GetAnStatusUpdateLog(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Enterprise.ZArchitecture.Business.Internal.BusinessObjectLogger.PrefixIndicator + AuditFieldsList.Codes.TIB));
			}
		}

		protected StmALog GetAnStatusUpdateLog(ZQuery filter)
		{
			foreach (StmALog log in StatusUpdateLogs)
			{
				if (log.MatchesFilter(filter))
				{
					return log;
				}
			}
			return null;
		}

		LogsForNominatedEvent StatusUpdateLogs
		{
			get { return statusUpdateLogs ?? (statusUpdateLogs = new LogsForNominatedEvent(Logs, Events.StatusUpdated)); }
		}
		LogsForNominatedEvent statusUpdateLogs;

		#endregion

		#region Temporary Importation Properties

		public ZDateTime TIBExpiryDate
		{
			get { return FormalEntry != null ? FormalEntry.US_TIBExpiryDate : ZDateTime.Empty; }
		}

		public ZString TIBNumOfExtensions
		{
			get { return FormalEntry != null ? FormalEntry.US_TIBNumOfExtensions.ToString() : ""; }
		}

		public CusEntryHeader FormalEntry
		{
			get { return ActiveEntryHeaders.EntrySummaryEntry; }
		}

		public ZBool PrintSocialSecurityNumberOnDocument { get => isSSNMenuSelected; set => isSSNMenuSelected = value; }
		ZBool isSSNMenuSelected;

		#endregion

		public ZDecimal CustomsValue
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				foreach (JobComInvoiceHeader invoiceHeader in Invoices)
				{
					foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
					{
						result += invoiceLine.JI_CustomsValue;
					}
				}

				return result;
			}
		}

		public void CalculateTotalEnteredValue()
		{
			Declaration.US_TotalEnteredValue = Declaration.ActiveEntryHeaders.EntrySummaryEntry?.TotalEnteredValue ?? ZDecimal.Zero;
		}

		public void DefaultBondAmountForSingleTransactionBond()
		{
			if (US_BondType == BondTypeList.Codes.SingleTransactionBond)
			{
				if (ActiveEntryHeaders.EntrySummaryEntry != null)
				{
					Declaration.US_BondAmount = new BondDetailsDefaulter().DetermineSEBBondAmount(ActiveEntryHeaders.EntrySummaryEntry, Declaration.US_BondCalcCode);
				}
			}
			else
			{
				Declaration.US_BondAmount = 0m;
			}
		}

		public ZBool US_7501IOR
		{
			get { return !US_7501Agent; }
			set
			{
				US_7501Agent = !value;
				US_7501IORInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_7501IORInfo
		{
			get { return GetZPropertyInfo(Schema.US_7501IOR, "IOR"); }
		}

		public ZString OtherReconIndicatorDescription
		{
			get { return this.AddInfoLookups.OtherReconIssueList.GetDescriptionFromCode(US_OtherReconIndicator); }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.JobDeclarationList))]
		public ZString JE_Calc_ReconJob
		{
			get
			{
				if (!je_Calc_ReconJob.HasValue)
				{
					je_Calc_ReconJob = ZString.Empty;

					if (US_NAFTAReconIndicator || !US_OtherReconIndicator.IsEmpty)
					{
						ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobDeclaration));

						ZDBOnlySubQuery entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
						entryQuery.AddToFilter(CusEntryHeaderSchema.CH_CH_PrimeEntry, ActiveEntryHeaders.GetPKs(CusEntryHeaderMessageTypeList.Codes.EntrySummary));
						entryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry);
						query.AddSubQuery(entryQuery, JoinCondition.And);

						JobDeclaration[] reconDeclarations = Factory.Load<JobDeclaration>(query);

						List<string> reconJobs = new List<string>();
						foreach (JobDeclaration reconDeclaration in reconDeclarations)
						{
							reconJobs.Add(reconDeclaration.JE_DeclarationReference);
						}

						je_Calc_ReconJob = new ZStringBuilder(reconJobs.ToArray()).ToStringWithDelimiterBetweenAppends(DelimiterBetweenReconJobReference);
					}
				}
				return je_Calc_ReconJob.Value;
			}
		}
		ZString? je_Calc_ReconJob;
		const string DelimiterBetweenReconJobReference = ", ";

		public ZPropertyInfo JE_Calc_ReconJobInfo
		{
			get { return GetZPropertyInfo(Schema.JE_Calc_ReconJob); }
		}

		public IOrganisationDetails ManufacturerDetails
		{
			get { return OrganisationDetails.New(JE_OA_ManufacturerAddressInfo, OrgMatchedCustomsRegNoType.MID); }
		}

		public IOrganisationDetails InvoicerDetails
		{
			get { return OrganisationDetails.New(JE_OA_InvoicerAddressInfo, OrgMatchedCustomsRegNoType.MID); }
		}

		public ZString JE_MasterBillTruncated
		{
			get { return BillValidator.GetTruncatedBillNumber(JE_MasterBill); }
		}

		internal protected virtual ZString BrokerReferenceNumberCore
		{
			get
			{
				var result = ZString.Empty;
				if (IsACECargoCertificationMode && US_PSC && USCustomsDataRegistry.Instance.EntryFiler.Value.EntryFilerCode != US_EntryFilerCode)
				{
					result = US_BRDRefNo;
				}
				else
				{
					result = US_BRDRefNo.IsEmpty ? JE_DeclarationReference : US_BRDRefNo;
				}
				return result.Right(9);
			}
		}

		public ZString BrokerReferenceNumber
		{
			get { return BrokerReferenceNumberCore; }
		}

		public ZString JE_Calc_USTransportMode => TransportModeCalculator.CalculateUSTransportMode(JE_TransportMode, JE_ContainerMode);

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ElectronicInvoiceStatusList))]
		[MaxLength(3)]
		public ZString ElectronicInvoiceStatus
		{
			get
			{
				if (electronicInvoiceStatusCalculator == null)
				{
					electronicInvoiceStatusCalculator = new ElectronicInvoiceStatusCalculator(this);
				}
				return electronicInvoiceStatusCalculator.ElectronicInvoiceStatus;
			}
		}
		ElectronicInvoiceStatusCalculator electronicInvoiceStatusCalculator;

		public ZPropertyInfo ElectronicInvoiceStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ElectronicInvoiceStatus); }
		}

		public bool ElectronicInvoiceStatus_ReadOnly
		{
			get { return true; }
		}

		public ZString ElectronicInvoiceStatusDescription
		{
			get { return Lookups.ElectronicInvoiceStatusList.GetDescriptionFromCode(ElectronicInvoiceStatus); }
		}

		public ZPropertyInfo ElectronicInvoiceStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ElectronicInvoiceStatusDescription); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.SchKList))]
		public override ZString US_SchKINBFinalForeignDest
		{
			get { return base.US_SchKINBFinalForeignDest; }
			set { base.US_SchKINBFinalForeignDest = value; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.SpecialKList))]
		public override ZString US_SpecialKINBFinalForeignDest
		{
			get { return base.US_SpecialKINBFinalForeignDest; }
			set { base.US_SpecialKINBFinalForeignDest = value; }
		}

		#region JE_MasterBillIssuerSCAC

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.USCarrierList))]
		[MaxLength(4)]
		public ZString JE_MasterBillIssuerSCAC
		{
			get => PrimaryMasterBill?.US_UI_NKBillIssuerSCAC ?? ZString.Empty;
			set
			{
				bool hasChanged = JE_MasterBillIssuerSCAC != value;
				if (hasChanged && !IsCopying)
				{
					Bill bill = Bills.CreatePrimaryBillIfNull(BillTypeList.Codes.MasterBill);
					bill.US_UI_NKBillIssuerSCAC = value;

					if (!IsSynchronisingBill)
					{
						DefaultNumberOfPacksToManifestQtyAndUQIfRequired();
					}

					if (IsAir && IsImport)
					{
						var airwayBillPrefix = MasterBillCarrier != null ? MasterBillCarrier.UI_AirwayBillPrefix : ZString.Empty;

						if (JE_MasterBill.IsEmpty || JE_MasterBill.Length == 3)
						{
							JE_MasterBill = airwayBillPrefix;
						}
					}

					UpdateUS_UI_NKCarrierSCACIfNeeded();
				}
				JE_MasterBillIssuerSCACInfo.RefreshBinding();
				Validation.ValidateJE_MasterBillIssuerSCAC();
			}
		}

		public ZPropertyInfo JE_MasterBillIssuerSCACInfo
		{
			get { return GetZPropertyInfo(Schema.JE_MasterBillIssuerSCAC); }
		}

		public ZString MasterBillIssuerSCACCode => PrimaryMasterBill?.US_UI_NKBillIssuerSCAC ?? ZString.Empty;

		public USCarrierCombined MasterBillCarrier
		{
			get
			{
				var carriers = Factory.Load<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, JE_MasterBillIssuerSCAC));
				if (carriers.Length == 1)
				{
					return carriers[0];
				}
				return null;
			}
		}

		public bool MasterBillCarrierHasNumericBillPrefix
		{
			get { return MasterBillCarrier is USCarrierCombined masterBillCarrier && !masterBillCarrier.UI_AirwayBillPrefix.IsEmpty && masterBillCarrier.UI_AirwayBillPrefix.IsNumbersOnlyOrEmpty; }
		}

		internal void SetMasterBillForSouthOriginatedTruckShipment()
		{
			if (JE_MasterBill.IsEmpty && IsEntryNumberToBeDefaultedToMasterBill && !ImportEntryNumber.IsEmpty)
			{
				JE_MasterBill = US_EntryFilerCode + ImportEntryNumber;
			}
		}

		internal bool IsEntryNumberToBeDefaultedToMasterBill
		{
			get
			{
				var isEntryPortInTheRegistry = BCRPortsFromRegistry.OfType<BorderCargoPort>().Any(x => x.PortCode == US_SchDEntry && x.Location == LocationList.Codes.South);
				return IsImport && !IsConsumptionFTZ && IsTruck && isEntryPortInTheRegistry;
			}
		}

		internal bool JE_MasterBillIssuerSCAC_ReadOnly
		{
			get
			{
				return ShouldSynchroniseWithShipment();
			}
		}

		#endregion

		#region JE_HouseBillIssuerSCAC

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.USCarrierList))]
		[MaxLength(4)]
		public ZString JE_HouseBillIssuerSCAC
		{
			get => PrimaryHouseBill?.US_UI_NKBillIssuerSCAC ?? ZString.Empty;
			set
			{
				bool hasChanged = JE_HouseBillIssuerSCAC != value;
				if (hasChanged && !IsCopying)
				{
					var bill = Bills.CreatePrimaryBillIfNull(BillTypeList.Codes.HouseBill);
					bill.US_UI_NKBillIssuerSCAC = value;

					if (!IsSynchronisingBill)
					{
						DefaultNumberOfPacksToManifestQtyAndUQIfRequired();
					}

					ReattachPrimaryITNumberIfNeeded();
				}
				JE_HouseBillIssuerSCACInfo.RefreshBinding();
				Validation.ValidateJE_HouseBillIssuerSCAC();
				Validation.ValidateJE_HouseBill();
			}
		}

		internal bool JE_HouseBillIssuerSCAC_ReadOnly
		{
			get
			{
				return ShouldSynchroniseWithShipment();
			}
		}

		public ZPropertyInfo JE_HouseBillIssuerSCACInfo
		{
			get { return GetZPropertyInfo(Schema.JE_HouseBillIssuerSCAC); }
		}

		#endregion

		#region JE_MasterBillExpressTracking

		[ResourceStringData("Enterprise.Customs.US.Business.JobDeclaration|JE_MasterBillExpressTracking", Caption = "Is Express Carrier Tracking Number?")]
		public ZBool JE_MasterBillExpressTracking
		{
			get => PrimaryMasterBill?.US_ExpressTracking ?? ZBool.False;
			set
			{
				var isExpressTracking = JE_MasterBillExpressTracking;
				if (isExpressTracking != value && !IsCopying)
				{
					var bill = Bills.CreatePrimaryBillIfNull(BillTypeList.Codes.MasterBill);
					bill.US_ExpressTracking = value;

					if (!JE_MasterBillExpressTracking)
					{
						JE_MasterBill = ZString.Empty;
						SetMasterBillForSouthOriginatedTruckShipment();
						SetMasterBillForHandCarriedTransport();
					}
				}

				JE_MasterBillExpressTrackingInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_MasterBillExpressTrackingInfo
		{
			get { return PrimaryMasterBill != null ? GetWrappedZPropertyInfo(Schema.JE_MasterBillExpressTracking, x => PrimaryMasterBill.US_ExpressTrackingInfo) : GetZPropertyInfo(Schema.JE_MasterBillExpressTracking); }
		}

		#endregion

		#region US_NonAMS

		public override ZBool US_NonAMS
		{
			get { return base.US_NonAMS; }
			set
			{
				var oldValue = US_NonAMS;
				base.US_NonAMS = value;
				if (oldValue != value && !IsCopying)
				{
					ClearExpressTrackingIfRequired();
				}
			}
		}

		#endregion

		protected override void MarkAsNeedingValidationForMajorDataChangeCore()
		{
			base.MarkAsNeedingValidationForMajorDataChangeCore();
			Packages.MarkAsNeedingValidation();
		}

		protected override void AddFetchHintsForMarkAsNeedingValidationCore()
		{
			base.AddFetchHintsForMarkAsNeedingValidationCore();
			Packages.ForEach(c => c.FetchForLoadChildEditableObjectsIfNeeded());
		}

		public override ZString JE_EntryStatusDescription
		{
			get
			{
				var result = base.JE_EntryStatusDescription;
				if (IsDrawback && !result.IsEmpty)
				{
					result = JE_EntryStatus + " - " + result;
				}

				return result;
			}
		}

		public override ZString JE_MessageStatusDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (Lookups.MessageStatusList.ContainsCode(JE_MessageStatus))
				{
					result = Lookups.MessageStatusList.GetDescriptionFromCode(JE_MessageStatus);
				}
				if (IsDrawback && !result.IsEmpty)
				{
					result = JE_MessageStatus + " - " + result;
				}

				return result;
			}
		}

		public SummaryEntryStatusCalculator SummaryEntryStatusCalculator
		{
			get { return summaryEntryStatusCalculator ?? (summaryEntryStatusCalculator = new SummaryEntryStatusCalculator(this)); }
		}
		SummaryEntryStatusCalculator summaryEntryStatusCalculator;

		public ZString CountryPortOfLoading
		{
			get { return CountryNameCalculator.CalculateCountryNameFrom(JE_RL_NKPortOfLoading, US_SchDLoading, Factory); }
		}

		#region Dec EntryNo
		// This is required for separation of Entry Number from IT Entry Number for Status form
		// DeclarationNumber which is an option to use here has the problem of returning ALL entry numbers for any active header - hence a stand-alone IT entry would show the IT entry number in both entry number fields...
		// DeclarationNumber is used for the module filter - hence needs to show ALL entry numbers.
		// If EntryNumbersAsCommaDelimitedString is determined as no longer being required - then this could be removed & DeclarationNumber used.

		[BusinessObjectTestExclude]
		[MaxLength(8)]
		public ZString DecEntryNumber
		{
			get
			{
				ZString result = "";

				if (HasMultipleSEDs)
				{
					result = AESDirectCustomsEntryStatus.Descriptions.MultipleEntriesStatus;
				}
				else if (!ImportEntryNumber.IsEmpty)
				{
					result = ImportEntryNumber;
				}

				return result;
			}
			set
			{
				if (IsImport)
				{
					ImportEntryNumber = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateDecEntryNumber();
					}
				}
				else if (!value.IsEmpty)
				{
					throw new NotSupportedException("Setter of DecEntryNumber is currently only supported for Import.");
				}
			}
		}

		public ZPropertyInfo DecEntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.DecEntryNumber); }
		}

		public bool DecEntryNumber_ReadOnly
		{
			get
			{
				var result = true;

				if (IsImportByExternalBroker)
				{
					result = false;

					foreach (CusEntryHeader entry in ActiveEntryHeaders)
					{
						if (entry.HasTransactionsWithCustoms)
						{
							result = true;
							break;
						}
					}
				}
				else if (US_PSC && !IsEntryFiledByCurrentCompanyForPSC)
				{
					result = GetImportEntriesWithTransactionsWithCustoms().Any();
				}

				return result;
			}
		}

		/// <summary>
		/// Used to make US_EntryFilerCode and entry number read/write. Should not depend on those fields.
		/// </summary>
		internal bool IsEntryFiledByCurrentCompanyForPSC
		{
			get { return Declaration.JE_EntryAuthorisationDate.IsValid; }
		}

		internal bool IsPSCFilingOfEntriesByOtherFiler
		{
			get { return US_PSC && IsFilerDifferentToCurrentCompanyFiler; }
		}

		bool IsFilerDifferentToCurrentCompanyFiler
		{
			get { return !US_EntryFilerCode.IsEmpty && !US_EntryFilerCode.EqualsIgnoringCase(ProcessingPortCodeAndFilerFinder.GetEntryFilerCodeFromRegistry(Branch)); }
		}

		#endregion

		#region IT EntryNo

		public ZString ITEntryNo
		{
			get
			{
				ZString itEntryNo = ZString.Empty;
				if (IsInBond)
				{
					foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
					{
						if (entryHeader.IsInBond)
						{
							if (itEntryNo.IsEmpty)
							{
								itEntryNo = entryHeader.EntryNumber;
								break;
							}
						}
					}
				}

				return itEntryNo;
			}
		}

		public ZPropertyInfo ITEntryNoInfo
		{
			get { return GetZPropertyInfo(Schema.ITEntryNo); }
		}

		#endregion

		#region IT Numbers

		public List<ZString> ITNumbersFromBills
		{
			get
			{
				List<ZString> iTNumbers = new List<ZString>();

				foreach (Bill bill in Declaration.Bills)
				{
					foreach (ITAndSplitDetails itNo in bill.ITAndSplitDetails)
					{
						if (!itNo.US_ITNumber.IsEmpty)
						{
							iTNumbers.Add(itNo.US_ITNumber);
						}
					}
				}
				return iTNumbers;
			}
		}

		[BusinessObjectTestExclude]
		[MaxLength(11)]
		[ReadOnlyMember(nameof(JE_PrimaryITNumber_ReadOnly))]
		public ZString JE_PrimaryITNumber
		{
			get
			{
				var result = ZString.Empty;

				int count = ListOfUniqueITNumbersFromLowestBills.Count;
				if (count == 1)
				{
					result = ListOfUniqueITNumbersFromLowestBills[0];
				}
				else if (count > 1)
				{
					result = Constants.Multiple;
				}

				return result;
			}
			set
			{
				var oldValue = JE_PrimaryITNumber;
				if (!IsCopying && oldValue != value)
				{
					if (PrimaryMasterBill == null && PrimaryHouseBill == null)
					{
						Bills.CreatePrimaryBillIfNull(BillTypeList.Codes.MasterBill);
					}

					this.LowestBills.Rebuild();
					if (ListOfUniqueITNumbersFromLowestBills.Count == 0)
					{
						foreach (Bill bill in LowestBills)
						{
							var iTAndSplitDetail = bill.ITAndSplitDetails.AddNew();
							using (iTAndSplitDetail.GetValidationSuspender())
							{
								iTAndSplitDetail.US_ITNumber = value;
							}
						}
					}
					else if (ListOfUniqueITNumbersFromLowestBills.Count == 1)
					{
						foreach (Bill bill in LowestBills)
						{
							if (bill.ITAndSplitDetails.Count > 0)
							{
								bill.ITAndSplitDetails[0].US_ITNumber = value;
							}
							else
							{
								var iTAndSplitDetail = bill.ITAndSplitDetails.AddNew();
								using (iTAndSplitDetail.GetValidationSuspender())
								{
									iTAndSplitDetail.US_ITNumber = value;
								}
							}
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJE_PrimaryITNumber();
					}
				}

				JE_PrimaryITNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JE_PrimaryITNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JE_PrimaryITNumber); }
		}

		bool JE_PrimaryITNumber_ReadOnly
		{
			get { return ShouldSynchroniseWithShipment() || ListOfUniqueITNumbersFromLowestBills.Count > 1; }
		}

		public ZString PrimaryITNumber
		{
			get { return JE_PrimaryITNumber == Constants.Multiple ? (ZString)USConstants.MultipleValueIndicator : JE_PrimaryITNumber; }
		}

		List<ZString> ListOfUniqueITNumbersFromLowestBills
		{
			get
			{
				if (fListOfUniqueITNumbersFromLowestBills == null)
				{
					fListOfUniqueITNumbersFromLowestBills = new CachedProperty<List<ZString>>(Factory, delegate
					{
						var uniqueITNumbers = new List<ZString>();

						foreach (Bill bill in LowestBills)
						{
							foreach (ITAndSplitDetails itNo in bill.ITAndSplitDetails)
							{
								if (!itNo.US_ITNumber.IsEmpty && !uniqueITNumbers.Contains(itNo.US_ITNumber))
								{
									uniqueITNumbers.Add(itNo.US_ITNumber);
								}
							}
						}
						return uniqueITNumbers;
					});
				}
				return fListOfUniqueITNumbersFromLowestBills.Value;
			}
		}
		CachedProperty<List<ZString>> fListOfUniqueITNumbersFromLowestBills;

		#endregion

		#region Ports

		static ZString GetPortWithDesc(IForeignRegionalDistrictPort port, ZString code)
		{
			var result = ZString.Empty;
			if (port == null)
			{
				result = code;
			}
			else
			{
				result = port.PortCode + " " + port.PortName;
			}
			return result;
		}

		public ZString SchDLoadingWithDesc
		{
			get { return GetPortWithDesc(SchDLoadingPort, US_SchDLoading); }
		}

		public IForeignRegionalDistrictPort SchDLoadingPort
		{
			get { return CountryNameCalculator.GetForeignOrRegionalPort(US_SchDLoading, Factory); }
		}

		public IForeignRegionalDistrictPort SchDArrivalPort
		{
			get { return IsExport ? CountryNameCalculator.GetForeignPort(US_SchDArrival, USCForeignPortWrapper.Type.AES, Factory) : CountryNameCalculator.GetForeignOrRegionalPort(US_SchDArrival, Factory); }
		}

		public IForeignRegionalDistrictPort SchDUSDestinationPort
		{
			get { return IsExport ? CountryNameCalculator.GetForeignPort(US_SchDUSDestination, USCForeignPortWrapper.Type.AES, Factory) : CountryNameCalculator.GetForeignOrRegionalPort(US_SchDUSDestination, Factory); }
		}

		public IForeignRegionalDistrictPort SchDINBExportPort
		{
			get { return CountryNameCalculator.GetForeignOrRegionalPort(US_SchDINBExport, Factory); }
		}

		public IForeignRegionalDistrictPort SchDINBArrivalPort
		{
			get { return CountryNameCalculator.GetForeignOrRegionalPort(US_SchDINBArrival, Factory); }
		}

		public IForeignRegionalDistrictPort SchDPresentationPort
		{
			get { return CountryNameCalculator.GetForeignOrRegionalPort(US_ITPresentationPort, Factory); }
		}

		public IForeignRegionalDistrictPort SchKFinalForeignDestinationPort
		{
			get { return CountryNameCalculator.GetForeignOrRegionalPort(US_SchKINBFinalForeignDest, Factory); }
		}

		public ZString SchDEntryWithDesc
		{
			get { return GetPortWithDesc(SchDUSPortOfEntry, US_SchDEntry); }
		}

		public IForeignRegionalDistrictPort SchDUSPortOfEntry
		{
			get { return CountryNameCalculator.GetForeignOrRegionalPort(US_SchDEntry, Factory); }
		}

		#endregion

		#region Filter Strip properties

		protected override ZString BGMReferencesCore
		{
			get { return IsExport ? base.BGMReferencesCore : ZString.Empty; }
		}

		#region Carrier SCAC

		public ZString CarrierSCAC
		{
			get
			{
				var returnVal = US_UI_NKCarrierSCAC;
				if (IsExport && returnVal.IsEmpty)
				{
					returnVal = ShippingLineSCACCode;
				}
				return returnVal;
			}
		}

		public ZPropertyInfo CarrierSCACInfo
		{
			get { return GetZPropertyInfo(Schema.CarrierSCAC); }
		}

		#endregion

		#region CargoRelease Status

		public bool HasCargoReleaseBeenCertified
		{
			get { return Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(entryHeader => entryHeader.HasCargoReleaseBeenCertified); }
		}

		public bool IsCargoReleaseBeingCertified
		{
			get { return Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(entryHeader => entryHeader.IsCargoReleaseBeingCertified); }
		}

		public ZString CargoReleaseStatus
		{
			get
			{
				ZString cargoReleaseStatus = ZString.Empty;
				if (IsCargoRelease || IsACECargoCertificationMode)
				{
					foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
					{
						if (entryHeader.IsCargoRelease || entryHeader.IsBorderCargoRelease || entryHeader.IsACECargoRelease)
						{
							if (cargoReleaseStatus.IsEmpty)
							{
								cargoReleaseStatus = entryHeader.CH_Status;
							}
							else
							{
								cargoReleaseStatus = ImportEntryStatusList.Codes.MUL;
								break;
							}
						}
					}
				}

				return cargoReleaseStatus;
			}
		}

		public ZPropertyInfo CargoReleaseStatusInfo
		{
			get { return GetZPropertyInfo(Schema.CargoReleaseStatus); }
		}

		public bool CargoReleaseStatus_ReadOnly
		{
			get { return true; }
		}

		public ZString CargoReleaseStatusDesc
		{
			get { return CargoReleaseStatus == ImportEntryStatusList.Codes.MUL ? ImportEntryStatusList.Descriptions.MUL : ImportMessageStatus.GetDescriptionFromCode(CargoReleaseStatus); }
		}

		public ZPropertyInfo CargoReleaseStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.CargoReleaseStatusDesc); }
		}

		#endregion

		#region Simplified Entry Bill Status

		public ZString SimplifiedEntryBillStatus
		{
			get
			{
				if (!simplifiedEntryBillStatus.HasValue)
				{
					var result = new ZStringBuilder();
					if (IsACECargoCertificationMode)
					{
						var billStatus = new List<ZString>();
						billStatus.AddRange(Bills.Cast<Bill>().SelectMany(x => x.GetACECargoReleaseBillStatus()).Distinct());

						if (billStatus.Count > 5)
						{
							result.Append(ImportEntryStatusList.Codes.MUL);
						}
						else
						{
							billStatus.ForEach(x => result.Append(x));
						}
					}

					simplifiedEntryBillStatus = result.ToStringWithDelimiterBetweenAppends(",");
				}
				return simplifiedEntryBillStatus.Value;
			}
		}
		internal ZString? simplifiedEntryBillStatus;

		public ZString SimplifiedEntryBillStatusDescription
		{
			get
			{
				if (!simplifiedEntryBillStatusDescription.HasValue)
				{
					var result = new ZStringBuilder();
					var seBillStatus = SimplifiedEntryBillStatus;
					if (seBillStatus == ImportEntryStatusList.Codes.MUL)
					{
						result.Append("Multiple Bills Status(More than five)");
					}
					else if (!seBillStatus.IsEmpty)
					{
						var existStatuses = seBillStatus.Split(',').ToList();
						var list = Lookups.CargoManifestStatusDispositionList;
						foreach (var status in existStatuses)
						{
							var desc = list.GetDescriptionFromCode(status);
							result.Append(ZString.Format("{0}-{1}", status, desc));
						}
					}
					simplifiedEntryBillStatusDescription = result.ToStringWithDelimiterBetweenAppends(",");
				}
				return simplifiedEntryBillStatusDescription.Value;
			}
		}
		internal ZString? simplifiedEntryBillStatusDescription;

		#endregion

		#region Hold or Exam Status
		public ZString HLDOrEXMStatus
		{
			get
			{
				if (!hldOrEXMStatus.HasValue)
				{
					hldOrEXMStatus = ZString.Empty;
					if (Bills.Count > 0)
					{
						if (Bills.Cast<Bill>().Any(x => x.HLDOrEXMStatus == "Y"))
						{
							hldOrEXMStatus = "Y";
						}
						else if (Bills.Cast<Bill>().All(x => x.HLDOrEXMStatus == "N"))
						{
							hldOrEXMStatus = "N";
						}
					}
				}
				return hldOrEXMStatus.Value;
			}
		}
		internal ZString? hldOrEXMStatus;
		#endregion

		#region EntrySummary Status

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MessageStatusList))]
		public ZString EntrySummaryStatus
		{
			get
			{
				ZString entrySummaryStatus = ZString.Empty;
				if (IsENSFormalImport)
				{
					foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
					{
						if (entryHeader.IsFormalEntry)
						{
							if (entrySummaryStatus.IsEmpty)
							{
								entrySummaryStatus = entryHeader.CH_Status;
							}
							else
							{
								entrySummaryStatus = ImportEntryStatusList.Codes.MUL;
								break;
							}
						}
					}
				}
				return entrySummaryStatus;
			}
		}

		public ZPropertyInfo EntrySummaryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.EntrySummaryStatus); }
		}

		public ZString EntrySummaryStatusDesc
		{
			get { return IsENSFormalImport ? (EntrySummaryStatus == ImportEntryStatusList.Codes.MUL ? ImportEntryStatusList.Descriptions.MUL : ImportMessageStatus.GetDescriptionFromCode(EntrySummaryStatus)) : ""; }
		}

		public ZPropertyInfo EntrySummaryStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.EntrySummaryStatusDesc); }
		}

		#endregion

		#region InBond Closed Date

		public ZString InBondClosedDate
		{
			get
			{
				var inBondClosedDate = ZString.Empty;
				if (HasInBond)
				{
					inBondClosedDate = InBondHeader.InBondClosedDate;
				}
				return inBondClosedDate;
			}
		}

		#region InBond Entry Types

		public ZString InBondEntryTypes
		{
			get
			{
				var inBondEntryType = ZString.Empty;
				if (HasInBond)
				{
					inBondEntryType = InBondHeader.InBondEntryTypes;
				}
				return inBondEntryType;
			}
		}

		#endregion

		#region IT Departure Status

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ITStatusList))]
		public ZString ITDepartureStatus
		{
			get { return EntryStatusesAndErrors.ITDepartureStatus; }
		}

		public ZPropertyInfo ITDepartureStatusInfo
		{
			get { return GetZPropertyInfo(nameof(ITDepartureStatus)); }
		}

		public ZString ITDepartureStatusDesc
		{
			get { return IsInBond ? ImportMessageStatus.GetDescriptionFromCode(ITDepartureStatus) : ""; }
		}

		public ZPropertyInfo ITDepartureStatusDescInfo
		{
			get { return GetZPropertyInfo(nameof(ITDepartureStatusDesc)); }
		}

		#endregion

		#region IT Arrival Status

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ITStatusList))]
		public ZString ITArrivalStatus
		{
			get { return EntryStatusesAndErrors.ITArrivalStatus; }
		}

		public ZPropertyInfo ITArrivalStatusInfo
		{
			get { return GetZPropertyInfo(nameof(ITArrivalStatus)); }
		}

		public ZString ITArrivalStatusDesc
		{
			get { return IsInBond ? ImportMessageStatus.GetDescriptionFromCode(ITArrivalStatus) : ""; }
		}

		public ZPropertyInfo ITArrivalStatusDescInfo
		{
			get { return GetZPropertyInfo(nameof(ITArrivalStatusDesc)); }
		}

		#endregion

		#region IT Export Status

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ITStatusList))]
		public ZString ITExportStatus
		{
			get { return EntryStatusesAndErrors.ITExportStatus; }
		}

		public ZPropertyInfo ITExportStatusInfo
		{
			get { return GetZPropertyInfo(nameof(ITExportStatus)); }
		}

		public ZString ITExportStatusDesc
		{
			get { return IsInBond ? ImportMessageStatus.GetDescriptionFromCode(ITExportStatus) : ""; }
		}

		public ZPropertyInfo ITExportStatusDescInfo
		{
			get { return GetZPropertyInfo(nameof(ITExportStatusDesc)); }
		}

		#endregion

		#region IT TOL Status

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ITStatusList))]
		public ZString ITTOLStatus
		{
			get { return EntryStatusesAndErrors.ITTOLStatus; }
		}

		public ZPropertyInfo ITTOLStatusInfo
		{
			get { return GetZPropertyInfo(nameof(ITTOLStatus)); }
		}

		public ZString ITTOLStatusDesc
		{
			get { return IsInBond ? ImportMessageStatus.GetDescriptionFromCode(ITTOLStatus) : ""; }
		}

		public ZPropertyInfo ITTOLStatusDescInfo
		{
			get { return GetZPropertyInfo(nameof(ITTOLStatusDesc)); }
		}

		#endregion

		#endregion

		#region Export Status

		public ZString ExportStatus
		{
			get
			{
				ZString exportStatus = ZString.Empty;

				if (IsExport)
				{
					foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
					{
						if (entryHeader.IsExport)
						{
							if (exportStatus.IsEmpty)
							{
								exportStatus = entryHeader.CH_Status;
							}
							else if (entryHeader.CH_Status != exportStatus)
							{
								exportStatus = AESDirectCustomsEntryStatus.Codes.MultipleEntriesStatus;
								break;
							}
						}
					}
				}

				return exportStatus;
			}
		}

		public ZPropertyInfo ExportStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ExportStatus); }
		}

		public ZString ExportStatusDesc
		{
			get { return IsExport ? ExportMessageStatus.GetDescriptionFromCode(ExportStatus) : ""; }
		}

		public ZPropertyInfo ExportStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.ExportStatusDesc); }
		}

		#endregion

		#region Has 9802 Tariff

		public bool Has9802Tariff
		{
			get
			{
				if (!has9802TariffCached.HasValue)
				{
					has9802TariffCached = false;

					foreach (JobComInvoiceLine invLine in InvoiceLines)
					{
						if (invLine.US_SupTariff.StartsWith("9802"))
						{
							has9802TariffCached = true;
							break;
						}
					}
				}

				return has9802TariffCached.Value;
			}
		}
		bool? has9802TariffCached;

		public void RefreshHas9802Tariff()
		{
			has9802TariffCached = null;
		}

		#endregion

		public ZDateTime EntrySubmittedDate
		{
			get
			{
				ZDateTime entrySubmittedDate = ZDateTime.Empty;

				var entryHeader = this.IsFTZAdmission ? Declaration.ActiveEntryHeaders.FTZEntry : Declaration.ActiveEntryHeaders.EntrySummaryEntry;
				if (entryHeader != null)
				{
					entrySubmittedDate = entryHeader.CH_EntrySubmittedDate;
				}

				return entrySubmittedDate;
			}
		}

		public ZPropertyInfo EntrySubmittedDateInfo
		{
			get { return GetZPropertyInfo(Schema.EntrySubmittedDate); }
		}

		public override ZDateTime JE_EntryAuthorisationDate
		{
			get { return base.JE_EntryAuthorisationDate; }
			set
			{
				var oldValue = JE_EntryAuthorisationDate;

				base.JE_EntryAuthorisationDate = value;

				if (oldValue != JE_EntryAuthorisationDate)
				{
					LogOrCancelCustomsCleared();

					if (IsImport || IsImportByExternalBroker)
					{
						CalculatePaymentDueDate();
						US_DeferredTaxDueDate = new AddInfoJobDeclarationWorkingDate().GenerateDeferredTaxDueDate(Declaration);

						if (JE_EntryAuthorisationDate.IsValid)
						{
							if (!IsCargoReleaseEntryReleaseStatusChangingSuspended)
							{
								ReleaseStatus = CRLReleaseStatusList.Codes.REL;
							}

							bool isPermitNeeded = IsFTZWeeklyEstimateIntegrationEnabled;
							if (isPermitNeeded && oldValue.IsEmpty && !(FindRelatedPermits().Count > 0))
							{
								var lineGroups = new PermitEntryLineGrouping(this).GetEntryLineGroups();
								var permit = new PermitCreator(this);

								permit.CreatePermits(lineGroups);
							}
						}
						else
						{
							if (!IsCargoReleaseEntryReleaseStatusChangingSuspended)
							{
								if (US_CertifyCargoRelease)
								{
									ReleaseStatus = CRLReleaseStatusList.Codes.NRL;
								}
								else
								{
									ReleaseStatus = ZString.Empty;
								}
							}
						}
					}
					if (!IsCopying)
					{
						InvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		void LogOrCancelCustomsCleared()
		{
			if (JE_EntryAuthorisationDate.IsValid)
			{
				LogCustomsClearedIfNeeded();
			}
			else
			{
				var mostRecentClearedLog = LogsOfDeclarationOrShipment.MostRecentLogByEventTimeExcludingEstimated(CustomsClearedEventType);
				if (mostRecentClearedLog != null)
				{
					mostRecentClearedLog.Cancel();
				}
			}
		}

		protected override void LogCustomsClearedCore()
		{
			LogsOfDeclarationOrShipment.AddNew(CustomsClearedEventType, ClearanceEventReference, DateTimeParser.GetFromJobBranchCurrentTime(Branch));
		}

		/// <summary>
		/// only send the statement update request if there is a formal entry on the job, else it is irrelevant.
		/// </summary>
		public void TrySendStatementUpdate()
		{
			new StatementDateChangeRequest(this).GenerateStatementDateChangeRequestIfNecessary();
		}

		public bool JE_EntryAuthorisationDate_ReadOnly
		{
			get
			{
				bool isImportJob = JE_MessageType == JobMessageTypeList.Codes.Import || IsImportByExternalBroker;

				return IsExport ||
					(isImportJob &&
					!US_ManEntry &&
					(HasCargoReleaseBeenCertified || DispositionCodes.HasCode(CargoReleaseProcessingResultList.Codes.ReleaseDateUpdate) ||
					DispositionCodes.HasCode(CargoReleaseProcessingResultList.Codes.Released)));
			}
		}

		void CalculatePaymentDueDate()
		{
			if (EntrySummaryStatus != ImportMessageStatusList.Codes.ClearEntrySummaryDelete && !IsReconMessageType)
			{
				US_PaymentDueDate = new AddInfoJobDeclarationWorkingDate().GeneratePaymentDueDate(GetBaseDateToCalculatePSDOn());
			}
		}

		public ZDateTime StatementPaidDate
		{
			get { return RelatedStatement != null ? RelatedStatement.B2_PaymentAuthorizationDate : ZDateTime.Empty; }
		}

		[ReadOnlyMember(nameof(US_PaymentDate_ReadOnly))]
		public override ZDateTime US_PaymentDate
		{
			get
			{
				ZDateTime result = base.US_PaymentDate;

				if (result.IsEmpty)
				{
					result = StatementPaidDate;
				}

				return result;
			}
			set { base.US_PaymentDate = value; }
		}

		bool US_PaymentDate_ReadOnly
		{
			get { return US_PaymentType != PaymentTypeList.Codes.IndividualBasis || Declaration.RelatedStatement != null; }
		}

		public ZString StatementNo
		{
			get { return RelatedStatement != null ? RelatedStatement.B2_StatementNumber : ZString.Empty; }
		}
		public ZBool StatementIsPaid
		{
			get { return RelatedStatement != null ? RelatedStatement.IsPaid : ZBool.False; }
		}

		public ZBool StatementIsPMSType
		{
			get { return RelatedStatement != null ? RelatedStatement.IsPeriodicDailyStatement : ZBool.False; }
		}

		public ZString StatementStatus
		{
			get { return RelatedStatement != null ? RelatedStatement.B2_Status : ZString.Empty; }
		}

		public ZString StatementStatusDesc
		{
			get { return RelatedStatement != null ? Lookups.StatementStatusList.GetDescriptionFromCode(RelatedStatement.B2_Status) : ""; }
		}

		public ZString PaymentStatus
		{
			get { return RelatedStatement != null ? RelatedStatement.B2_PaymentStatus : ZString.Empty; }
		}

		public ZString PaymentStatusDesc
		{
			get { return RelatedStatement != null ? Lookups.PaymentStatusList.GetDescriptionFromCode(RelatedStatement.B2_PaymentStatus) : ""; }
		}

		#region ManufacturerNameAndID
		public ZString ManufacturerNameAndID
		{
			get
			{
				if (manufacturerNameAndIDCached == null)
				{
					manufacturerNameAndIDCached = new CachedProperty<ZString>(Factory, delegate
					{
						return ManufacturerAddress != null ? ManufacturerAddress.EffectiveCompanyNameTruncated + " (" + ManufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates) + ")" : "";
					});
				}
				return manufacturerNameAndIDCached.Value.ToUpper();
			}
		}
		CachedProperty<ZString> manufacturerNameAndIDCached;

		public ZPropertyInfo ManufacturerNameAndIDInfo
		{
			get { return GetZPropertyInfo(Schema.ManufacturerNameAndID); }
		}

		#endregion

		ImportMessageStatusList ImportMessageStatus
		{
			get { return importMessageStatus ?? (importMessageStatus = new ImportMessageStatusList()); }
		}
		ImportMessageStatusList importMessageStatus;

		internal AESDirectCustomsEntryStatus ExportMessageStatus
		{
			get { return exportMessageStatus ?? (exportMessageStatus = new AESDirectCustomsEntryStatus()); }
		}
		AESDirectCustomsEntryStatus exportMessageStatus;

		public ZString BLUStatusDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (Lookups.BLUStatusList.ContainsCode(BLUStatus))
				{
					result = Lookups.BLUStatusList.GetDescriptionFromCode(BLUStatus);
				}

				return result;
			}
		}

		public ZPropertyInfo BLUStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BLUStatusDescription); }
		}

		public ZString FDAMsgStatusDescription
		{
			get { return Lookups.FDAMsgStatusList.GetDescriptionFromCode(FDAMsgStatus) ?? ZString.Empty; }
		}

		public ZPropertyInfo FDAMsgStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.FDAMsgStatusDescription); }
		}

		public ZString ReleaseStatusDesc
		{
			get { return new CRLReleaseStatusList().GetDescriptionFromCode(ReleaseStatus); }
		}

		[ReadOnly(true)]
		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ReleaseStatusList))]
		public ZString ReleaseStatus
		{
			get { return AddOnReleaseStatus != null ? AddOnReleaseStatus.XA_Data : ZString.Empty; }
			set
			{
				AddOnColumnStatus(AddOnReleaseStatus, Constants.GenAddOnColumnFieldName.ReleaseStatus, value, ReleaseStatusInfo);
				LogCSHForHVLVStandAloneDeclaration();
			}
		}

		GenAddOnColumn AddOnReleaseStatus
		{
			get
			{
				if (addOnReleaseStatus == null)
				{
					addOnReleaseStatus = new CachedProperty<GenAddOnColumn>(Factory, delegate
					{
						return GetAddOnStatus(Constants.GenAddOnColumnFieldName.ReleaseStatus);
					});
				}
				return addOnReleaseStatus.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnReleaseStatus;

		public ZPropertyInfo ReleaseStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ReleaseStatus); }
		}

		public override ZString CustomsClearanceStatus => ReleaseStatus;

		#region Accounting Total Outstanding/Invoiced/Billed Amounts

		protected override List<ZGuid> EntryChargeTypeCodesToMatchCore
		{
			get { return Factory.GetCachedValue<EntryChargeTypeList>().GetAllChargeCodePKsOf(Branch.Company.PK).ToList(); }
		}

		#endregion

		public ZDateTime EarliestExportDate
		{
			get
			{
				var result = ZDateTime.Empty;
				if (IsExport)
				{
					foreach (JobComInvoiceLine line in InvoiceLines)
					{
						if (result == ZDateTime.Empty || line.US_DateOfExport < result)
						{
							result = line.US_DateOfExport;
						}
					}
				}
				return result;
			}
		}

		#endregion

		public ZString ImporterOfRecordNumber
		{
			get { return OrgHeaderWrapper.GetCustomsRelatedCode(IOR, OrgMatchedCustomsRegNoType.EIN); }
		}

		public ZString ImporterOfRecordNumberForDocument
		{
			get
			{
				return IOR == null ? ZString.Empty : IOR.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, GetParamsForRegNo);
			}
		}

		ZString[] GetParamsForRegNo => PrintSocialSecurityNumberOnDocument
			? new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber }
			: new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber };

		public ZDecimal TotalPayable
		{
			get { return ActiveEntryHeaders.EntrySummaryEntry != null ? ActiveEntryHeaders.EntrySummaryEntry.CH_TotalPaid : 0; }
		}

		public bool HasDiscrepancyInDisbursementAmount
		{
			get
			{
				var result = false;
				var entryHeader = ActiveEntryHeaders.EntrySummaryEntry;
				if (entryHeader != null)
				{
					var lastAcceptedENSMessage = PGADispositionProviderExtensionMethods.GetLastENSClearedMessage(Declaration);
					if (lastAcceptedENSMessage != null)
					{
						var calculatedAmountFromMessage = GetTotaldutyFeeAmountFromAnAcceptedMessage(lastAcceptedENSMessage);
						result = Math.Abs(entryHeader.TotalPayableIncludingDeferredTax - calculatedAmountFromMessage) != 0m;
					}
				}

				return result;
			}
		}

		protected ZDecimal GetTotaldutyFeeAmountFromAnAcceptedMessage(MQEDIMessage acceptedMessage)
		{
			var result = ZDecimal.Zero;
			if (acceptedMessage != null)
			{
				var ens90Block = acceptedMessage.MessageBlock.MessageBlocks.OfType<ACEMessageBuildingBlocks.AENS90>().FirstOrDefault();
				if (ens90Block != null)
				{
					result = ens90Block.GrandTotalADDutyAmount + ens90Block.GrandTotalCVDutyAmount + ens90Block.GrandTotalDutyAmount + ens90Block.GrandTotalIRTaxAmount + ens90Block.GrandTotalOtherRevenueAmount + ens90Block.GrandTotalUserFeeAmount;
				}
			}
			return result;
		}

		#region Type

		public ReconDeclaration ReconDeclaration
		{
			get { return reconDeclaration; }
			set
			{
				if (ReconDeclaration != null && ReconDeclaration != value)
				{
					throw new InvalidOperationException("ReconDeclaration is supposed to wrap one instance of JobDeclaration");
				}
				reconDeclaration = value;
			}
		}
		ReconDeclaration reconDeclaration;

		public bool IsRecon
		{
			get { return ReconDeclaration != null; }
		}

		public bool IsReconMessageType
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.Recon; }
		}

		public bool IsProtest
		{
			get { return Protest != null; }
		}

		public bool IsProtestMessageType
		{
			get { return JE_MessageType == JobMessageTypeList.MoreCodes.Protest; }
		}

		public Protest.Protest Protest
		{
			get;
			set;
		}

		#endregion

		public JobComInvoiceHeader RandomHeader
		{
			get { return InvoiceLines.Select(x => ((JobComInvoiceLine)x).InvoiceHeader).FirstOrDefault() ?? Factory.GetNull<JobComInvoiceHeader>(); }
		}

		#region CBPBrokerDocumentaryAddress
		public JobDocAddress CBPBrokerDocumentaryAddress
		{
			get
			{
				if (fCBPBrokerDocumentaryAddress == null || fCBPBrokerDocumentaryAddress.IsDeleted)
				{
					fCBPBrokerDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.CBPBroker);
					OnCBPBrokerDocumentaryAddressFoundOrCreated();
				}
				return fCBPBrokerDocumentaryAddress;
			}
		}
		JobDocAddress fCBPBrokerDocumentaryAddress;

		protected void OnCBPBrokerDocumentaryAddressFoundOrCreated()
		{
			fCBPBrokerDocumentaryAddress.OnRelationshipFieldsChanged += new EventHandler(CBPBrokerDocumentaryAddress_OnRelationshipFieldsChanged);
		}

		void CBPBrokerDocumentaryAddress_OnRelationshipFieldsChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ExternalBrokers))]
		[RelatedBusinessObject("CBPBroker")]
		public ZGuid JE_OH_CBPBroker
		{
			get
			{
				return CBPBrokerDocumentaryAddress.E2_OA_Address == ZGuid.Invalid
					? ZGuid.Invalid
					: CBPBrokerDocumentaryAddress.Address != null ? CBPBrokerDocumentaryAddress.Address.OA_OH : ZGuid.Empty;
			}
			set
			{
				if (value == ZGuid.Invalid)
				{
					CBPBrokerDocumentaryAddress.E2_OA_Address = value;
				}
				else
				{
					OrgHeader organisation = Factory.Load<OrgHeader>(value);
					CBPBrokerDocumentaryAddress.E2_OA_Address = organisation == null ? ZGuid.Empty : organisation.MainAddress.PK;
				}

				Validation.ValidateJE_OH_CBPBroker();
				JE_OH_CBPBrokerInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_OH_CBPBrokerInfo
		{
			get { return GetZPropertyInfo(Schema.JE_OH_CBPBroker); }
		}

		public OrgHeader CBPBroker
		{
			get { return Factory.Load<OrgHeader>(JE_OH_CBPBroker); }
		}

		#endregion

		public bool HasBIRDCommunicationMode()
		{
			return ExternalBroker != null && ExternalBroker.EDICommunicationsModes.FindByModule(EDICommunicationsMode.Modules.US_BIRD).Length > 0;
		}

		public string DeclarationReferenceAppendedByFormattedEntryNumber
		{
			get { return Declaration.JE_DeclarationReference + " / " + CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(US_EntryFilerCode, ImportEntryNumber); }
		}

		public ZString ISFBillStatus
		{
			get
			{
				if (isfBillStatusCached == null)
				{
					isfBillStatusCached = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;
						if (IsSea && IsImport)
						{
							var hasBlank = false;
							foreach (Bill bill in LowestBills)
							{
								var billStatus = bill.ISFBillStatus;
								if (billStatus.IsEmpty)
								{
									hasBlank = true;
								}
								else
								{
									if (result.IsEmpty)
									{
										result = billStatus;
									}
									else if (result != billStatus)
									{
										result = Common.US.ISF.ISFStatusHelper.Multiple;
									}

									if (result == Common.US.ISF.ISFStatusHelper.Multiple)
									{
										break;
									}
								}
							}
							if (!result.IsEmpty && hasBlank && result != Common.US.ISF.ISFStatusHelper.Multiple)
							{
								result += ",No Status";
							}
						}
						return result;
					}
					);
				}

				return isfBillStatusCached.Value;
			}
		}
		CachedProperty<ZString> isfBillStatusCached;

		public ZPropertyInfo ISFBillStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ISFBillStatus); }
		}

		public ZString ISFBillStatusDescription
		{
			get
			{
				if (isfBillStatusDescriptionCached == null)
				{
					isfBillStatusDescriptionCached = new CachedProperty<ZString>(Factory, delegate
					{
						var status = ISFBillStatus;
						var statuses = status.Split(',');
						var extraMessage = "";
						if (statuses.Length > 1 && statuses[1].EqualsIgnoringCase("No Status"))
						{
							status = statuses[0];
							extraMessage = " Also there is a bill without any status.";
						}
						return Common.US.ISF.ISFStatusHelper.GetISFBillStatusDescription(Factory, status) + extraMessage;
					});
				}
				return isfBillStatusDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> isfBillStatusDescriptionCached;

		public ZPropertyInfo ISFBillStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ISFBillStatusDescription); }
		}

		internal ZString WarehouseAddressFirmsCode
		{
			get { return WarehouseAddress?.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates) ?? ZString.Empty; }
		}

		protected override void WarehouseDocAddress_ValueChanged(object sender, EventArgs e)
		{
			base.WarehouseDocAddress_ValueChanged(sender, e);
			MarkAsNeedingValidation();
		}

		public string ResetDutyCalculationDate()
		{
			var result = "";
			var ensEntry = ActiveEntryHeaders.EntrySummaryEntry;
			if (ensEntry != null)
			{
				var oldDutyCalcDate = ensEntry.US_DutyCalcDate.ToShortDateString().ToUpperInvariant();
				using (ReCalculateMPFAndDutyDate())
				{
					var newDutyCalcDate = DateForFeeCalculation;
					var newDutyCalcDateString = newDutyCalcDate.ToShortDateString().ToUpperInvariant();
					if (oldDutyCalcDate == newDutyCalcDateString)
					{
						result = string.Format(CultureInfo.CurrentCulture, DutyCalculationDateDidNotChange, newDutyCalcDateString);
					}
					else
					{
						result = string.Format(CultureInfo.CurrentCulture, DutyCalculationDateChanged, oldDutyCalcDate, newDutyCalcDateString);
						ensEntry.US_DutyCalcDate = newDutyCalcDate;
						Logs.AddNew(Events.ResetEntryMessageItemFunction, string.Format(CultureInfo.CurrentCulture, DutyCalculationDateChangedLog, oldDutyCalcDate, newDutyCalcDateString));
					}
				}
			}
			return result;
		}

		const string DutyCalculationDateDidNotChange = "No change to Duty Calculation Date ({0}) has been found.";
		const string DutyCalculationDateChanged = "Duty Calculation Date has been changed from ({0}) to ({1}).";
		const string DutyCalculationDateChangedLog = "DUTY CALC DATE CHANGED {0} TO {1}";

		public void RefreshExchangeRates()
		{
			RefreshExRateToLatestRateAvailable();
		}

		protected override bool ShouldRefreshExchangeRates
		{
			get
			{
				bool result = ValuationDatesChanged;

				if (!result && IsImport)
				{
					var crlEntry = ActiveEntryHeaders.CargoReleaseEntry ?? ActiveEntryHeaders.SimplifiedEntry;

					var hasTransactionsForCRL = crlEntry != null && crlEntry.HasTransactionsWithCustoms && !crlEntry.HasBeenWithdrawn;
					var hasTransactionsForENS = ActiveEntryHeaders.EntrySummaryEntry != null && ActiveEntryHeaders.EntrySummaryEntry.HasTransactionsWithCustoms && !ActiveEntryHeaders.EntrySummaryEntry.HasBeenWithdrawn;

					result = !hasTransactionsForCRL && !hasTransactionsForENS;
				}

				return result;
			}
		}

		protected override void BeforeExRatesRefreshed(IEnumerable<ICurrencyProvider> currencyProviders)
		{
			base.BeforeExRatesRefreshed(currencyProviders);

			ValuationDateTracker.SetLatestRateDate(this, currencyProviders);
		}

		protected override void AfterExRatesRefreshed(IEnumerable<ICurrencyProvider> currencyProviders)
		{
			base.AfterExRatesRefreshed(currencyProviders);
			valuationDatesDirty = false;

			foreach (ICurrencyProvider currencyProvider in currencyProviders)
			{
				currencyProvider.ValidateCurrencyCode();
			}
		}

		internal bool ShouldRefreshExchangeRatesExposed
		{
			get { return ShouldRefreshExchangeRates; }
		}

		internal bool IsCargoReleaseForWeeklyEstimate
		{
			get { return IsACECargoRelease && IsWeeklyEstimateConsumptionFTZ; }
		}

		internal bool IsFTZWeeklyEstimateIntegrationEnabled
		{
			get { return IsCargoReleaseForWeeklyEstimate && SupportsBondedWarehousing; }
		}

		internal bool IsFTZIntegrationEnabled
		{
			get { return IsConsumptionFTZ && SupportsBondedWarehousing; }
		}

		internal bool IsWarehouseFirmsCodeUniqueForWeeklyEstimateIntegration
		{
			get
			{
				var result = true;

				if (IsFTZWeeklyEstimateIntegrationEnabled && !WarehouseAddressFirmsCode.IsEmpty)
				{
					var query = new ZQuery();
					query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
					query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.FIRMSCode);
					query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, WarehouseAddressFirmsCode);
					query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, SQLComparisonOperator.NotEqual, WarehouseDocAddress.E2_OA_Address);

					var firmsCodes = Factory.Load<OrgCusCode>(query);
					result = !firmsCodes.Any(x => x.PremisesAddress != null && x.PremisesAddress.OA_IsActive);
				}

				return result;
			}
		}

		internal void MarkValuationDatesDirty()
		{
			valuationDatesDirty = true;

			MarkApportionmentDirty();
		}
		bool valuationDatesDirty;

		internal bool ValuationDatesChanged
		{
			get { return valuationDatesDirty; }
		}

		#region Anticipated Liquidation Entry Properties

		public ZDateTime US_AnticipatedLiquidationDate
		{
			get
			{
				var result = US_ALDate;
				if (FormalEntry != null)
				{
					result = FormalEntry.US_ALDate;
				}
				else if (IsRecon && ReconDeclaration != null && ReconDeclaration.ReconEntry != null)
				{
					result = ReconDeclaration.ReconEntry.US_AnticipatedLiquidationDate;
				}

				return result;
			}
		}

		public ZDecimal US_AnticipatedLiquidatedDuty
		{
			get
			{
				var result = US_ALDuty;
				if (FormalEntry != null)
				{
					result = FormalEntry.US_ALDuty;
				}
				else if (IsRecon && ReconDeclaration != null && ReconDeclaration.ReconEntry != null)
				{
					result = ReconDeclaration.ReconEntry.US_AnticipatedLiquidatedDuty;
				}

				return result;
			}
		}

		public ZDateTime US_CollectionDate
		{
			get { return FormalEntry != null ? FormalEntry.US_CollectionDate : ZDateTime.Empty; }
		}

		#endregion

		internal List<MQEDIMessage> TransmittedStatementMessagesInDescOrder
		{
			get
			{
				var messages = new List<MQEDIMessage>(new TypedEnumerable<MQEDIMessage>(Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction, ACEApplicationIdentifierCodeList.Codes.StatementUpdate }, EDIMessage.Direction.Transmit)));
				messages.Sort((x, y) => y.EM_SystemCreateTimeUtc.CompareTo(x.EM_SystemCreateTimeUtc));
				return messages;
			}
		}

		internal bool HasCurrentElectronicRelease
		{
			get
			{
				var result = false;
				var dispositionDate = Declaration.GetLatestDispositionDate();

				foreach (DispositionData disposition in Declaration.DispositionCodes)
				{
					if (disposition.US_DispositionDate == dispositionDate)
					{
						if (disposition.US_Code == CargoReleaseProcessingResultList.Codes.ReleaseRemovedFurtherDocReviewRequired
							|| disposition.US_Code == CargoReleaseProcessingResultList.Codes.ReleaseDateUpdate && disposition.US_ReleaseOrigin == ReleaseOriginCodeList.Codes.ReleaseDateRemoved)
						{
							result = false;
							break;
						}

						if (disposition.US_Code == CargoReleaseProcessingResultList.Codes.PaperlessEntry)
						{
							result = true;
						}

						if (IsElectronicInvoicing)
						{
							if (disposition.US_Code == CargoReleaseProcessingResultList.Codes.CondReleaseGenExam
								|| disposition.US_Code == CargoReleaseProcessingResultList.Codes.CondReleaseSpecDocReview
								|| disposition.US_Code == CargoReleaseProcessingResultList.Codes.EntryDetained
								|| disposition.US_Code == CargoReleaseProcessingResultList.Codes.OverrideToGeneral)
							{
								result = true;
							}
						}
					}
				}

				return result;
			}
		}

		internal bool IsElectronicInvoicing
		{
			get { return IsRemoteLocationFiling && !IsACECargoCertificationMode || US_EnableAII || US_IsInvoiceByRequest; }
		}

		public bool IsNonAMSJob
		{
			get { return TransportTypeList.IsNonAMSBillType(JE_TransportMode); }
		}

		public ZBool IsFTZPGAStandAlonePriorNotice
		{
			get { return IsFTZAdmission && US_EnableSPN && US_F_PNMode == PriorNoticeModeCodeList.Codes.P; }
		}

		public ZString FDASubmitterOrgContactPresentation
		{
			get
			{
				var result = ZString.Empty;
				if (FDASubmitter != null)
				{
					var fdaSubmitterWrapped = OrgHeaderWrapper.New(FDASubmitter);
					var name = ((IPGAContactDetails)fdaSubmitterWrapped).Name;
					if (!name.IsEmpty)
					{
						result = FDASubmitter.OH_FullName.Left(15) + ", " + name;
					}
					else
					{
						result = FDASubmitter.OH_FullName;
					}
				}
				return result;
			}
		}

		public ZString ImporterEIN
		{
			get
			{
				var importer = Importer;
				return importer != null ? importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates) : ZString.Empty;
			}
		}

		public ZString ImporterOfRecordEIN
		{
			get
			{
				var importerOfRecord = IOR;
				return importerOfRecord != null ? importerOfRecord.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates) : ZString.Empty;
			}
		}

		public ZBool IsExpressTrackingNumberRelevant
		{
			get { return IsFormalImport && IsAir && IsACECargoCertificationMode && !IsConsumptionFTZ && US_EntryType != EntryTypeList.Codes.LowValue && !US_NonAMS; }
		}

		public ZBool IsUnknownCarrierSCACForExport
		{
			get { return IsExport && (IsTruck || IsRail) && US_UI_NKCarrierSCAC == "UNKN" && ShippingLine == null; }
		}

		internal ZDate US_MPFCalcDate
		{
			get
			{
				var result = ZDate.Empty;

				if (ActiveEntryHeaders.EntrySummaryEntry is CusEntryHeader entryHeader)
				{
					if (!US_ImmediateDelivery || (entryHeader.HasBeenLodgedAtCustoms && !ShouldCalculateMPFAndDutyDateForImmediateDelivery))
					{
						result = entryHeader.US_MPFCalcDate.Date;
					}
				}

				if (result.IsEmpty && US_ImmediateDelivery)
				{
					result = ZDate.Today;
				}

				return result;
			}
		}

		#endregion

		#region Overriden Properties

		public override ZString US_DDTCUSMLCategoryCode
		{
			get { return base.US_DDTCUSMLCategoryCode; }
			set
			{
				base.US_DDTCUSMLCategoryCode = value;
				if (US_JurisdictionNumber_ReadOnly)
				{
					US_JurisdictionNumber = ZString.Empty;
				}
			}
		}

		bool US_JurisdictionNumber_ReadOnly => US_DDTCUSMLCategoryCode != USMLCategoryCodes.Codes.MiscellaneousArticles;

		[ReadOnlyMember(nameof(US_JurisdictionNumber_ReadOnly))]
		public override ZString US_JurisdictionNumber
		{
			get { return base.US_JurisdictionNumber; }
			set { base.US_JurisdictionNumber = value; }
		}

		public override ZGuid JE_OH_Forwarder
		{
			get { return base.JE_OH_Forwarder; }
			set
			{
				base.JE_OH_Forwarder = value;

				InvoiceLines.OfType<JobComInvoiceLine>().ForEach(line => line.ACE_FDALines.OfType<ACEFDA>().ForEach(fda => fda.RefreshUS_OA_ShipperAddress_ZAddress()));
			}
		}

		protected override void ImporterDeliveryAddressChanged(ZGuid oldAddressPK, ZGuid newAddressPK)
		{
			base.ImporterDeliveryAddressChanged(oldAddressPK, newAddressPK);

			InvoiceLines.OfType<JobComInvoiceLine>().ForEach(line => line.ACE_FDALines.OfType<ACEFDA>().ForEach(fda => fda.RefreshDeliverToPartyZAddress()));
		}

		protected override ZBool IsReciprocalRatesCore
		{
			get { return IsReciprocalRatesConstant; }
		}

		internal static bool IsReciprocalRatesConstant
		{
			get { return true; }
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return LocalCurrencyConstantCode; }
		}

		public static ZString LocalCurrencyConstantCode
		{
			get { return Enterprise.Core.Constants.CurrencyCodes.UnitedStates; }
		}

		internal static RefCurrency GetLocalCurrency()
		{
			return RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, LocalCurrencyConstantCode);
		}

		public override ZGuid JE_JS
		{
			get { return base.JE_JS; }
			set
			{
				ZGuid oldValue = JE_JS;
				base.JE_JS = value;

				if (IsExport && !IsCopying && oldValue != JE_JS)
				{
					Invoices.MarkAsNeedingValidation();
				}
			}
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			Invoices.MarkAsNeedingValidation();
		}

		#region IDocAddresses Overrides

		protected override DocAddressType[] SupportedAddressTypesCore
		{
			get
			{
				return
					IsRecon
						? base.SupportedAddressTypesCore.Concat(new[] { DocAddressType.ClaimantAddress }).ToArray()
						: base.SupportedAddressTypesCore.Concat(new[] { DocAddressType.CBPBroker, DocAddressType.FDASubmitter }).ToArray();
			}
		}
		#endregion

		public override IEnumerable<string> StmALogProxyFieldsNames
		{
			get
			{
				foreach (var fieldName in base.StmALogProxyFieldsNames)
				{
					yield return fieldName;
				}

				yield return Schema.SPIAuditDate;
				yield return Schema.SPIAuditReference;
				yield return Schema.SPIAuditUser;
				yield return Schema.SPIAuditUserName;
				yield return Schema.FDAAuditDate;
				yield return Schema.FDAAuditReference;
				yield return Schema.FDAAuditUser;
				yield return Schema.FDAAuditUserName;
				yield return Schema.CWAuditDate;
				yield return Schema.CWAuditReference;
				yield return Schema.CWAuditUser;
				yield return Schema.CWAuditUserName;
			}
		}

		public ZString CreationSource { get; set; }

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				var result = base.CustomLogReferenceSuffix;
				if (!IsInDatabase && !CreationSource.IsEmpty)
				{
					result = "[CreationSource = " + CreationSource + "]";
				}
				return result;
			}
		}

		public override ZDateTime JE_ExportDate
		{
			get { return base.JE_ExportDate; }
			set
			{
				ZDateTime oldValue = JE_ExportDate;
				base.JE_ExportDate = value.Date;

				if (!IsCopying && oldValue != value.Date)
				{
					MarkValuationDatesDirty();
				}
			}
		}

		protected override bool ShouldSetExchangeRatesOnChangeOfExportDate
		{
			get { return true; }
		}

		protected override bool IsDateAtOriginGreaterThanExportDate => JE_DateAtOrigin.Date > JE_ExportDate;

		public override ZDateTime DateOfValuation
		{
			get { return ValuationDateTracker.GetEffectiveDate(IsImport ? US_LatestRateDate : ZDateTime.Empty, DateOfValuation_ExportDate); }
		}

		internal ZDateTime DateOfValuation_ExportDate
		{
			get { return US_DateOfExport.IsValid ? US_DateOfExport : base.DateOfValuation; }
		}

		public override ZString JE_MasterBillForGenericWrapper
		{
			get { return IncludeSCACInBillNum ? (ZString)(JE_MasterBillIssuerSCAC + JE_MasterBill) : base.JE_MasterBillForGenericWrapper; }
		}

		public override ZString JE_HouseBillForGenericWrapper
		{
			get { return IncludeSCACInBillNum ? (ZString)(JE_HouseBillIssuerSCAC + JE_HouseBill) : base.JE_HouseBillForGenericWrapper; }
		}

		internal bool IncludeSCACInBillNum
		{
			get { return IsImport && (JE_TransportMode == TransportTypeList.Codes.Sea || JE_TransportMode == TransportTypeList.Codes.Rail); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobDeclaration|US_FDAContactName", Caption = "Broker PGA Contact Name")]
		public override ZString US_FDAContactName
		{
			get { return base.US_FDAContactName; }
			set
			{
				if (!IsCopying && US_FDAContactName != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDAContactName = value;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobDeclaration|US_FDAContactPhoneNo", Caption = "Broker PGA Contact Phone")]
		public override ZString US_FDAContactPhoneNo
		{
			get { return base.US_FDAContactPhoneNo; }
			set
			{
				if (!IsCopying && US_FDAContactPhoneNo != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDAContactPhoneNo = value;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobDeclaration|US_FDAContactEmail", Caption = "Broker PGA Contact Email")]
		public override ZString US_FDAContactEmail
		{
			get { return base.US_FDAContactEmail; }
			set
			{
				if (!IsCopying && US_FDAContactEmail != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDAContactEmail = value;
			}
		}

		public override ZString US_CargoReleaseType
		{
			get { return base.US_CargoReleaseType; }
			set
			{
				bool hasChanges = base.US_CargoReleaseType != value;
				var oldCertificationMode = IsACECargoCertificationMode;

				base.US_CargoReleaseType = value;

				if (hasChanges && !IsCopying)
				{
					if (!IsDataChangeSuspendedByFakeDeclaration)
					{
						UpdateCargoReleaseValidationMode();
						ModifySplitDetailsAndNonAMSFlagsIfRequired();
						ClearPipelineNameFieldIfRequired();
						ClearFTZCurrentTariffIfRequired();
						SetMasterBillForHandCarriedTransport();
					}

					if (!IsCargoReleaseTypeChangingEventSuspended)
					{
						RefreshInvoiceLinesOGAPGAIndicators(oldCertificationMode != IsACECargoCertificationMode);
					}
				}
				else
				{
					US_CargoReleaseTypeInfo.RefreshBinding();
				}
			}
		}

		internal void UpdateCargoReleaseValidationMode()
		{
			ValidationModesCalculator.UpdateValidationModes(ValidationModes.CargoRelease, US_EnableCRL || US_CertifyCargoRelease);
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.US_ConsolidatedInformalList))]
		public override ZString US_ConsolidatedInformalIndicator
		{
			get { return base.US_ConsolidatedInformalIndicator; }
			set { base.US_ConsolidatedInformalIndicator = value; }
		}

		public override ZString JE_ApplicationCode
		{
			get { return base.JE_ApplicationCode; }
			set
			{
				bool hasChanges = base.JE_ApplicationCode != value;

				if (!IsDrawback)
				{
					var oldCertificationMode = IsACECargoCertificationMode;
					var oldIsACE = IsACE;

					base.JE_ApplicationCode = value;

					if (hasChanges)
					{
						if (!IsDataChangeSuspendedByFakeDeclaration)
						{
							DefaultEntryFilerCode();
							DefaultFromImporterForACEIfNeeded();
							MapValuesBetweenACEAndACS();
							DefaultUS_EntryModeIfNecessary();

							if (US_EnableCRL_ReadOnly)
							{
								US_EnableCRL = false;
							}

							if (oldIsACE != IsACE)
							{
								US_CertifyCargoRelease = IsImport && !IsFTZAdmission && US_EnableCRL && !IsACE;
							}

							var eventSuspender = IsACE ? new CargoReleaseTypeChangingEventSuspender(this) : (IDisposable)new DisposableObject();
							try
							{
								if (JE_ApplicationCode == JobApplicationCodeList.Codes.ACS)
								{
									US_CargoReleaseType = ZString.Empty;
								}
								MessageTypeBasedValueDefaulter.SetCertifyCargoReleaseAndDefaultCargoReleaseTypeIfNecessary();
							}
							finally
							{
								eventSuspender.Dispose();
							}

							ModifySplitDetailsAndNonAMSFlagsIfRequired();
							ClearPipelineNameFieldIfRequired();
							ClearFTZCurrentTariffIfRequired();
							RefreshBondDispositionCodeIfRequired();
							SetMasterBillForHandCarriedTransport();
							ClearExpressTrackingIfRequired();
						}
						RefreshInvoiceLinesOGAPGAIndicators(oldCertificationMode != IsACECargoCertificationMode);

						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							invoiceLine.NHTSALines.MarkAsNeedingValidationIncludingChildren();
							invoiceLine.APHISHeaders.MarkAsNeedingValidationIncludingChildren();
						}
						Invoices.ForEach(invoice => invoice.InvalidateJZ_Calc_LinesEnteredCache());
					}
					else
					{
						JE_ApplicationCodeInfo.RefreshBinding();
					}
				}
				else
				{
					base.JE_ApplicationCode = value;
					SetDefaultValueForDrawback();
				}
			}
		}

		void RefreshBondDispositionCodeIfRequired()
		{
			if (!IsACE)
			{
				this.US_BondDispositionCode = ZString.Empty;
				this.US_BondDispositionCode2 = ZString.Empty;
			}
		}

		public ZString CurrentUserFullName => Env.CurrentUser?.FullName ?? ZString.Empty;

		public ZString CurrentUserEmailAddress => Env.CurrentUser?.EmailAddress ?? ZString.Empty;

		public ZString CurrentUserWorkPhone => Env.CurrentUser?.WorkPhone ?? ZString.Empty;

		public override ZString US_F_PNMode
		{
			get { return base.US_F_PNMode; }
			set
			{
				bool hasChanges = US_F_PNMode != value;

				if (!IsCopying && US_F_PNMode != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}

				var oldCertificationMode = IsACECargoCertificationMode;

				if (hasChanges)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
					base.US_F_PNMode = value;
				}

				if (hasChanges && !IsCopying)
				{
					RefreshInvoiceLinesOGAPGAIndicators(oldCertificationMode != IsACECargoCertificationMode);
				}
				else
				{
					US_F_PNModeInfo.RefreshBinding();
				}
			}
		}

		#region Suspend AddInfo Property Setting

		int addInfoPropertySettingSuspenderIndex;

		bool IsAddInfoPropertySettingSuspended => addInfoPropertySettingSuspenderIndex > 0;

		public IDisposable SuspendAddInfoPropertySetting()
		{
			return new AddInfoPropertySettingSuspender(this);
		}

		class AddInfoPropertySettingSuspender : IDisposable
		{
			public AddInfoPropertySettingSuspender(JobDeclaration declaration)
			{
				this.declaration = declaration;
				this.declaration.addInfoPropertySettingSuspenderIndex++;
			}

			readonly JobDeclaration declaration;

			#region IDisposable Members

			public void Dispose()
			{
				declaration.addInfoPropertySettingSuspenderIndex--;
			}

			#endregion
		}
		#endregion

		bool IsCargoReleaseTypeChangingEventSuspended
		{
			get { return cargoReleaseTypeChangingEventSuspenderIndex > 0; }
		}

		class CargoReleaseTypeChangingEventSuspender : IDisposable
		{
			public CargoReleaseTypeChangingEventSuspender(JobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.cargoReleaseTypeChangingEventSuspenderIndex++;
			}

			readonly JobDeclaration declaration;
			void IDisposable.Dispose()
			{
				declaration.cargoReleaseTypeChangingEventSuspenderIndex--;
			}
		}
		byte cargoReleaseTypeChangingEventSuspenderIndex;

		bool IsCargoReleaseEntryReleaseStatusChangingSuspended
		{
			get { return cargoReleaseEntryReleaseStatusChangingSuspenderIndex > 0; }
		}

		internal class CargoReleaseEntryReleaseStatusChangingSuspender : IDisposable
		{
			public CargoReleaseEntryReleaseStatusChangingSuspender(JobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.cargoReleaseEntryReleaseStatusChangingSuspenderIndex++;
			}
			readonly JobDeclaration declaration;

			void IDisposable.Dispose()
			{
				declaration.cargoReleaseEntryReleaseStatusChangingSuspenderIndex--;
			}
		}
		byte cargoReleaseEntryReleaseStatusChangingSuspenderIndex;

		void RefreshInvoiceLinesOGAPGAIndicators(bool certificationModeChanged)
		{
			var data = (USLinkedToDeclarationData)GetNewLinkedToDeclarationData();
			var isPersistent = IsPersistent;
			if (isPersistent && certificationModeChanged)
			{
				if (data.CanHavePGAFDA)
				{
					FDAStatus = ZString.Empty;
					FDAMsgStatus = ZString.Empty;
				}
				else
				{
					pgaStatus = ZString.Empty;
				}
			}

			foreach (JobComInvoiceLine invoiceLine in InvoiceLines.ToArray())
			{
				if (!invoiceLine.IsDeleted)
				{
					if (isPersistent && certificationModeChanged)
					{
						invoiceLine.UpdateOGAPGADetailsWhenCertificationModeChanges(data);
					}
					invoiceLine.OGAAgencyRequirements.Populate();
				}
			}
		}

		protected override LinkedToDeclarationData GetNewLinkedToDeclarationDataCore()
		{
			return new USLinkedToDeclarationData(this);
		}

		void MapValuesBetweenACEAndACS()
		{
			if (!IsDataChangeSuspendedByFakeDeclaration)
			{
				if (!IsACE)
				{
					US_PSC = false;
				}

				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					invoiceLine.MapValuesBetweenACEAndACS();
				}
			}
		}

		public override ZGuid JE_OH_Consignee
		{
			get { return base.JE_OH_Consignee; }
			set
			{
				ZGuid oldValue = JE_OH_Consignee;
				base.JE_OH_Consignee = value;

				if (oldValue != value)
				{
					((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = true;

					foreach (JobComInvoiceHeader header in Invoices)
					{
						if (header.JZ_OH_Consignee.IsEmpty)
						{
							header.JZ_OH_Consignee = JE_OH_Consignee;
						}
					}
				}
			}
		}

		public override bool IsNonTransportDeclarationType
		{
			get { return IsConsumptionFTZ || IsDrawback || IsReWarehouse || base.IsNonTransportDeclarationType; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.US_TariffTypeList))]
		public override ZString US_TariffType
		{
			get { return base.US_TariffType; }
			set
			{
				ZString oldValue = US_TariffType;
				base.US_TariffType = value;
				if (!IsCopying)
				{
					var newValue = US_TariffType;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_TariffType);
						RefreshTariff();
					}
				}
			}
		}

		public override ZString US_EntryMode
		{
			get { return base.US_EntryMode; }
			set
			{
				bool hasChanges = (base.US_EntryMode != value);
				base.US_EntryMode = value;
				if (!IsCopying && hasChanges)
				{
					if (IsRemoteLocationFiling)
					{
						if (!IsACE)
						{
							US_EnableCRL = false;
							US_CertifyCargoRelease = true;
						}

						US_PreparerDistrictPort = USCustomsDataRegistry.Instance.PreparerDistrictPort.GetValueWithoutFallback(Guid.Empty, RegistryBranchPK, Guid.Empty);
						US_PreparerOfficeCode = new ZString(USCustomsDataRegistry.Instance.PreparerOfficeCode.GetValueWithoutFallback(Guid.Empty, RegistryBranchPK, Guid.Empty)).Left(US_PreparerOfficeCodeInfo.MaxLength);
					}
					else
					{
						US_PreparerDistrictPort = ZString.Empty;
						US_PreparerOfficeCode = ZString.Empty;
					}
					MessageTypeBasedValueDefaulter.DefaultCargoReleaseTypeIfNecessary();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.US_ECCNList))]
		public override ZString US_ECCN
		{
			get { return base.US_ECCN; }
			set
			{
				ZString oldValue = US_ECCN;
				base.US_ECCN = value;
				if (!IsCopying)
				{
					var newValue = US_ECCN;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_ECCN);
						if (IsExport)
						{
							Invoices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public bool HasAvailableECCNNumbers => !US_LicenseType.IsEmpty && LicenseValidationHelper.HasAvailableECCNNumber(Factory, US_LicenseType);

		public override ZString US_ExportCode
		{
			get { return base.US_ExportCode; }
			set
			{
				ZString oldValue = US_ExportCode;
				base.US_ExportCode = value;
				if (!IsCopying)
				{
					var newValue = US_ExportCode;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_ExportCode);
						if (IsExport)
						{
							Invoices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		[ReadOnlyMember(nameof(US_LicenseNo_ReadOnly))]
		public override ZString US_LicenseNo
		{
			get { return base.US_LicenseNo; }
			set
			{
				ZString oldValue = US_LicenseNo;
				base.US_LicenseNo = value;
				if (!IsCopying)
				{
					var newValue = US_LicenseNo;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_LicenseNo, null, true);
						if (IsExport)
						{
							Invoices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		bool US_LicenseNo_ReadOnly
		{
			get
			{
				var result = false;
				if (IsExport)
				{
					var exportDate = this.GetEffectiveDateForECR();
					if (exportDate.IsValid)
					{
						result = UniversalReferenceDataHelper.GetLicenseNoReadOnly_LicenseNoIsNotEmpty(Factory, US_LicenseType, exportDate, US_LicenseNo);
					}
				}
				return result;
			}
		}

		[AddInfoDesc(nameof(SchDEntryDescription))]
		public override ZString US_SchDEntry
		{
			get { return base.US_SchDEntry; }
			set
			{
				var hasChanges = (base.US_SchDEntry != value);
				if (!IsCopying && hasChanges)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_SchDEntry = value;

				if (hasChanges && !IsCopying && !value.IsEmpty)
				{
					DefaultUS_EntryModeIfNecessary();
					if (!IsDataSyncFromShipment)
					{
						JE_RL_NKFinalDestination = USScheduleResolver.MatchingUNLOCO(value, Factory);
					}

					DefaultCargoReleaseTypeIfNecessary();
					DefaultDataRelatedToTruck();
					DefaultEntryDateIfNecessary();
					SetMasterBillForSouthOriginatedTruckShipment();
				}
			}
		}

		void DefaultEntryDateIfNecessary()
		{
			if (!US_SchDEntry.IsEmpty)
			{
				if (US_SchDEntry == US_SchDArrival)
				{
					US_EntryDate = JE_DateOfArrival;
				}
				else
				{
					var transports = GetListOfTransports();
					var unlocos = GetAllUNLOCOMatches(US_SchDEntry);
					if (transports.Any())
					{
						foreach (var unlocoCode in unlocos)
						{
							var matchedTransports = transports.Where(o => unlocoCode == o.JW_RL_NKDiscPort);
							if (matchedTransports.Count() == 1)
							{
								var transport = matchedTransports.First();
								US_EntryDate = transport.JW_ATA.IsEmpty ? transport.JW_ETA : transport.JW_ATA;
								break;
							}
						}
					}
				}
			}
		}

		IEnumerable<Transport> GetListOfTransports()
		{
			if (Shipment != null)
			{
				return Shipment.TransportsIncludingRelated.Cast<Transport>();
			}
			return Transports.Cast<Transport>();
		}

		IEnumerable<ZString> GetAllUNLOCOMatches(ZString portCode)
		{
			var query = new ZQuery(RefLocoMapSchema.RY_RN, Core.Constants.CountryGuids.UnitedStates);
			query.AddToFilter(RefLocoMapSchema.RY_LocalPortCode, portCode);
			var locoMaps = Factory.Load<RefLocoMap>(query);

			return locoMaps?.Select(map => map.RY_RL_NKLocoPort).Distinct();
		}

		//Important to go through AddInfoLookups as it might have an extra filter.
		public ZString SchDEntryDescription
		{
			get
			{
				var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_SchDEntry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				return port?.ZZD_Description ?? ZString.Empty;
			}
		}

		public ZString SchDEntryState
		{
			get
			{
				var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_SchDEntry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				return port?.GetAttribute(RefCusCodeListAttributeTypes.Codes.State) ?? ZString.Empty;
			}
		}

		public bool IsClearedInPR
		{
			get { return SchDEntryState == USStateList.Codes.PuertoRico; }
		}

		void DefaultCargoReleaseTypeIfNecessary()
		{
			if (IsImport)
			{
				MessageTypeBasedValueDefaulter.DefaultCargoReleaseTypeIfNecessary();
			}
		}

		#region FDASubmitter

		public JobDocAddress FDASubmitterDocumentaryAddress
		{
			get
			{
				if (fFDASubmitterDocumentaryAddress == null || fFDASubmitterDocumentaryAddress.IsDeleted)
				{
					fFDASubmitterDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.FDASubmitter);
					fFDASubmitterDocumentaryAddress.OnRelationshipFieldsChanged += new EventHandler(FDASubmitterDocumentaryAddress_OnRelationshipFieldsChanged);
				}
				return fFDASubmitterDocumentaryAddress;
			}
		}
		JobDocAddress fFDASubmitterDocumentaryAddress;

		void FDASubmitterDocumentaryAddress_OnRelationshipFieldsChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.ImporterOfRecordList))]
		[RelatedBusinessObject("FDASubmitter")]
		public ZGuid JE_OH_FDASubmitter
		{
			get
			{
				return FDASubmitterDocumentaryAddress.E2_OA_Address == ZGuid.Invalid
					? ZGuid.Invalid
					: FDASubmitterDocumentaryAddress.Address != null ? FDASubmitterDocumentaryAddress.Address.OA_OH : ZGuid.Empty;
			}
			set
			{
				var oldValue = JE_OH_FDASubmitter;
				if (!IsCopying && JE_OH_FDASubmitter != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}

				if (value == ZGuid.Invalid)
				{
					FDASubmitterDocumentaryAddress.E2_OA_Address = value;
				}
				else
				{
					OrgHeader organisation = Factory.Load<OrgHeader>(value);
					FDASubmitterDocumentaryAddress.E2_OA_Address = organisation == null ? ZGuid.Empty : organisation.MainAddress.PK;
				}
				Validation.ValidateJE_OH_FDASubmitter();
				JE_OH_FDASubmitterInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JE_OH_FDASubmitterInfo
		{
			get { return GetZPropertyInfo(Schema.JE_OH_FDASubmitter); }
		}

		public OrgHeader FDASubmitter
		{
			get { return Factory.Load<OrgHeader>(JE_OH_FDASubmitter); }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.USCarrierList))]
		public override ZString US_UI_NKCarrierSCAC
		{
			get { return base.US_UI_NKCarrierSCAC; }
			set
			{
				var oldValue = US_UI_NKCarrierSCAC;
				base.US_UI_NKCarrierSCAC = value;
				if (!IsCopying && oldValue != US_UI_NKCarrierSCAC)
				{
					UpdateShippingLineIfNeeded();

					CarrierCodeMarkedForRequest = IsCarrierCodeCandidateForUpdate;

					DefaultDataRelatedToTruck();

					SetEmptyValueTo_US_CarrierName_IfRequired();
				}
			}
		}

		[RelatedBusinessObject(nameof(DesignatedExamSite))]
		public override ZString US_DES
		{
			get { return base.US_DES; }
			set { base.US_DES = value; }
		}

		public ZZRefCusCodeListCombined DesignatedExamSite
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_DES, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.USStateList))]
		public override ZString US_DestinationState
		{
			get { return base.US_DestinationState; }
			set { base.US_DestinationState = value; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.US_YesNoList))]
		public override ZString US_7501Purchased
		{
			get { return base.US_7501Purchased; }
			set { base.US_7501Purchased = value; }
		}

		public override ZGuid JE_OA_ConsigneeAddress
		{
			get { return base.JE_OA_ConsigneeAddress; }
			set
			{
				var oldValue = JE_OA_ConsigneeAddress;
				if (!IsCopying && oldValue != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.JE_OA_ConsigneeAddress = value;
				if (oldValue != JE_OA_ConsigneeAddress)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OA_ConsigneeAddress);
				}

				if (!IsCopying)
				{
					var newValue = JE_OA_ConsigneeAddress;
					if (oldValue != newValue)
					{
						if (!IsCreatedFromUSLowValue)
						{
							ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress);
						}
						SetDestinationStateFromUC();
						if (newValue.IsValid && USCustomsDataRegistry.Instance.DoDefaultShipTo.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
						{
							JE_OA_ShipToPartyAddress = value;
						}
					}
				}
			}
		}

		void SetDestinationStateFromUC()
		{
			if (USCustomsDataRegistry.Instance.DefaultDestState.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
			{
				if (US_DestinationState.IsEmpty && ConsigneeOrgAddress != null)
				{
					var uCState = ConsigneeOrgAddress.GetAddressWithFallback(Enterprise.ZArchitecture.Business.AddressType.DLV).OA_State;   // Delivery, Pickup, then Main

					if (uCState.Length > 2)
					{
						uCState = Lookups.USStatesList.GetCodeFromDescription(uCState);
					}

					if (uCState.IsEmpty && ConsigneeOrgAddress.UNLOCO != null && ConsigneeOrgAddress.UNLOCO.CountryStates != null)
					{
						uCState = ConsigneeOrgAddress.UNLOCO.CountryStates.RW_Code;
					}

					US_DestinationState = uCState.Left(US_DestinationStateInfo.MaxLength);
				}
			}
		}

		public ZString UltimateConsigneeName
		{
			get { return ConsigneeOrgAddress != null ? ConsigneeOrgAddress.OH_FullNameTruncated : ZString.Empty; }
		}

		public override ZString JE_MergeBy
		{
			get { return base.JE_MergeBy; }
			set
			{
				bool hasChanges = base.JE_MergeBy != value;
				base.JE_MergeBy = value;
				if (hasChanges && !IsCopying)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_OH_ShippingLine
		{
			get { return base.JE_OH_ShippingLine; }
			set
			{
				ZGuid oldValue = JE_OH_ShippingLine;
				if (!IsCopying && JE_OH_ShippingLine != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.JE_OH_ShippingLine = value;
				if (oldValue != JE_OH_ShippingLine && !IsCopying)
				{
					UpdateUS_UI_NKCarrierSCACIfNeeded();
					SetEmptyValueTo_US_CarrierName_IfRequired();
				}
			}
		}

		public override ZString JE_ContainerMode
		{
			get { return base.JE_ContainerMode; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_ContainerMode))
				{
					bool hasChanges = base.JE_ContainerMode != value;
					base.JE_ContainerMode = value;
					if (hasChanges && !IsCopying)
					{
						InvoiceLines.MarkAsNeedingValidation();
						PackingGroups.MarkAsNeedingValidationIncludingChildren();

						SetDefaultsToUS_Box29IncludeContainers();
					}
				}
			}
		}

		protected override bool ShouldCreateInvoiceForExWarehouse
		{
			get { return false; }
		}

		public bool US_PaymentDueDate_ReadOnly
		{
			get { return true; }
		}

		public override ZString US_PaymentType
		{
			get { return base.US_PaymentType; }
			set
			{
				bool hasChanges = base.US_PaymentType != value;
				base.US_PaymentType = value;
				if (hasChanges && !IsCopying)
				{
					new ClientBranchDesignationDefaulter().Default(this);
					new DeclarationPrelimStatementDetailsDefaulter().Default(this);
					BrokerToPayIndicator = new PaymentDetailsDefaulter().GetDefaultBrokerToPayIndicatorBasedOnPaymentType(US_PaymentType);
				}
			}
		}

		public override ZGuid JE_OH_Importer
		{
			get { return base.JE_OH_Importer; }
			set
			{
				var oldValue = JE_OH_Importer;
				if (!IsCopying && oldValue != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.JE_OH_Importer = value;
				if (!IsCopying)
				{
					var newValue = JE_OH_Importer;
					if (oldValue != newValue)
					{
						fImporterWrapper = null;
						Invoices.MarkAsNeedingValidation();
						ClearDeliveryOrderValuesIfSame(newValue, DeliveryOrderHeader.Schema.US_OH_Shipper);
						DefaultBondedWarehouseAddressFromImporterFTZ();
						DefaultFromImporterIfNeeded();
						RefreshWHSPackLinesAndMarkForValidation();
						MessageTypeBasedValueDefaulter.DefaultValueFromImporter(oldValue);
					}
				}
			}
		}

		void RefreshWHSPackLinesAndMarkForValidation()
		{
			if (IsWHSUniversalXMLActive && !IsSettingDefaultValues)
			{
				SetupPackableInvoiceLinesIfNeeded();
				if (PackableInvoiceLines.Count > 0)
				{
#pragma warning disable CA2021 // Do not call Enumerable.Cast<T> or Enumerable.OfType<T> with incompatible types
					foreach (var invoiceLine in PackableInvoiceLines.OfType<WHSPackLine>().Select(x => x.InvoiceLine).Distinct())
					{
						invoiceLine.RefreshWHSPackLines();
					}
#pragma warning restore CA2021 // Do not call Enumerable.Cast<T> or Enumerable.OfType<T> with incompatible types
				}
				WHSPacks.MarkAsNeedingValidation();
				WHSPackLines.MarkAsNeedingValidation();
			}
		}

		void DefaultFromImporterIfNeeded()
		{
			var importer = Importer;

			if (importer != null)
			{
				DefaultIOROrgPK();

				if (IsImport)
				{
					if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(importer.MainAddress.OA_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
					{
						JE_OA_ConsigneeAddress = importer.MainAddress.PK;
					}

					var importerWrapper = ImporterWrapper;
					if (importerWrapper != null && importerWrapper.ConsigneeAddress != null)
					{
						JE_OA_ConsigneeAddress = importerWrapper.ZO_OA_ConsigneeAddress;
					}

					if (importerWrapper != null && importerWrapper.ShipToAddress != null)
					{
						JE_OA_ShipToPartyAddress = importerWrapper.ZO_OA_ShipToAddress;
					}

					JE_OH_FDASubmitter = JE_OH_Importer;
				}
				else
				{
					JE_OA_ConsigneeAddress = ZGuid.Empty;
					JE_OA_ShipToPartyAddress = ZGuid.Empty;
					JE_OH_FDASubmitter = ZGuid.Empty;
				}

				Set7501DeclarationDefaultsBasedFromOrg(importer);

				DefaultFromImporterForACEIfNeeded();

				DefaultJE_ApplicationCode();

				DefaultBondedWarehouseAddressFromImporterFTZWhenEmpty();
			}
		}

		void DefaultIOROrgPK()
		{
			if (IsDrawback || USCustomsDataRegistry.Instance.DoDefaultImporterOfRecord.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
			{
				IOROrgPK = IsImport || IsDrawback ? JE_OH_Importer : ZGuid.Empty;
			}
		}

		void DefaultFromImporterForACEIfNeeded()
		{
			if (USCustomsDataRegistry.Instance.DoDefaultSoldToParty.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
			{
				JE_OA_SoldToPartyAddress = IsACE && Importer != null ? Importer.MainAddress.PK : ZGuid.Empty;
			}
		}

		void DefaultBondedWarehouseAddressFromImporterFTZWhenEmpty()
		{
			if (WarehouseDocAddress.E2_OA_Address.IsEmpty)
			{
				DefaultBondedWarehouseAddressFromImporterFTZ();
			}
		}

		void DefaultBondedWarehouseAddressFromImporterFTZ()
		{
			OrgAddress newAddress = null;

			if (IsImport && EntryTypeList.IsWarehouseRelated(US_EntryType))
			{
				var address = Importer?.CountryData?.OV_OA_WarehouseAddress;
				if (address.HasValue && address.Value.IsValid)
				{
					newAddress = Factory.Load<OrgAddress>(address.Value);
				}
			}

			WarehouseDocAddress.OrganisationPK = (newAddress != null) ? newAddress.OA_OH : ZGuid.Empty;
			WarehouseDocAddress.E2_OA_Address = (newAddress != null) ? newAddress.PK : ZGuid.Empty;
		}

		public override ZString EntryDetailsInARInvoice
		{
			get
			{
				return IUSCustomsChargeEntryExtensions.GetFormattedEntryReferenceForHeader(US_EntryFilerCode, ImportEntryNumber, IsPaidByImporter, US_PaymentType, US_PaymentDueDate.Date);
			}
		}

		internal bool IsPaidByBroker
		{
			get
			{
				bool result = false;
				if (JE_PaymentMethod == Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker)
				{
					result = true;
				}
				else if (BrokerToPayIndicator.IsEmpty)
				{
					result = PaymentTypeList.IsPaidByBroker(US_PaymentType);
				}
				return result;
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.US_YesNoList))]
		[MaxLength(1)]
		public ZString BrokerToPayIndicator
		{
			get
			{
				ZString result = ZString.Empty;
				if (JE_PaymentMethod == Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker)
				{
					result = YesNoDefaultList.Codes.Yes;
				}
				if (JE_PaymentMethod == Customs.Business.PaymentPartyCodeDescriptionList.Codes.Importer)
				{
					result = YesNoDefaultList.Codes.No;
				}
				return result;
			}
			set
			{
				bool hasChanges = BrokerToPayIndicator != value;
				if (hasChanges && !IsCopying)
				{
					if (value == YesNoDefaultList.Codes.Yes)
					{
						JE_PaymentMethod = Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
					}
					else if (value == YesNoDefaultList.Codes.No)
					{
						JE_PaymentMethod = Customs.Business.PaymentPartyCodeDescriptionList.Codes.Importer;
					}
					else
					{
						JE_PaymentMethod = ZString.Empty;
					}
				}

				BrokerToPayIndicatorInfo.RefreshBinding();
				Validation.ValidateBrokerToPayIndicator();
			}
		}

		public ZPropertyInfo BrokerToPayIndicatorInfo
		{
			get { return GetZPropertyInfo(Schema.BrokerToPayIndicator); }
		}

		internal bool IsPaidByImporter
		{
			get
			{
				return !US_PaymentType.IsEmpty && US_PaymentType != PaymentTypeList.Codes.IndividualBasis && !IsPaidByBroker;
			}
		}

		#region 7501 Declaration Defaults

		void Set7501DeclarationDefaultsBasedFromOrg(OrgHeader org)
		{
			if (IsFormalImport)
			{
				US_7501Agent = org == null || !org.IsProxyOrgOfAnyCompany();

				if (org != null)
				{
					US_7501Purchased = OrgHeaderWrapper.New(org).ZO_Purchased;
				}
			}
			else
			{
				US_7501Agent = false;
				US_7501Purchased = ZString.Empty;
			}
		}

		#endregion

		public bool IsExpressCarrierTracking => IsExpressTrackingNumberRelevant && JE_MasterBillExpressTracking;

		[ResourceStringData("2b29aed5-3832-4edf-be61-05782c51d9b8", Caption = "Mail Reference", IsApplicableMember = nameof(IsPost))]
		[ResourceStringData("058f638e-c19e-4a8e-acb0-da686a22852c", Caption = "Batch/Ticket No.", IsApplicableMember = nameof(IsACECargoCertificationAndFixedTransportRelevant))]
		[ResourceStringData("10039408-ee96-417b-96fc-9b7e6d01ff21", Caption = "Express Carrier Tracking No.", IsApplicableMember = nameof(IsExpressCarrierTracking))]
		public override ZString JE_MasterBill
		{
			get { return base.JE_MasterBill; }
			set
			{
				ZString oldValue = base.JE_MasterBill;
				base.JE_MasterBill = value;
				if (!IsCopying && JE_MasterBill != oldValue)
				{
					UpdateTransportReferenceIfApplicable(true);
					UpdateUS_UI_NKCarrierSCACIfNeeded();

					if (!IsSynchronisingBill)
					{
						DefaultNumberOfPacksToManifestQtyAndUQIfRequired();
					}
				}
			}
		}

		public override ZString JE_HouseBill
		{
			get { return base.JE_HouseBill; }
			set
			{
				ZString oldValue = base.JE_HouseBill;
				base.JE_HouseBill = value;
				if (JE_HouseBill != oldValue)
				{
					UpdateTransportReferenceIfApplicable(false);
					if (!IsSynchronisingBill)
					{
						DefaultNumberOfPacksToManifestQtyAndUQIfRequired();
					}
				}

				ReattachPrimaryITNumberIfNeeded();
			}
		}

		void ReattachPrimaryITNumberIfNeeded()
		{
			bool shouldBeReattached = PrimaryMasterBill != null &&
										PrimaryMasterBill.ITAndSplitDetails.Count > 0 &&
										JE_PrimaryITNumber != JobDeclaration.Constants.Multiple && !IsRail;

			if (shouldBeReattached && PrimaryHouseBill is Bill primaryHouseBill)
			{
				ITAndSplitDetails itNumber = primaryHouseBill.ITAndSplitDetails.AddNew();
				itNumber.US_ITNumber = PrimaryMasterBill.ITAndSplitDetails[0].US_ITNumber;
				PrimaryMasterBill.ITAndSplitDetails.DeleteAll();
			}
		}

		public override ZString JE_TransportMode
		{
			get { return base.JE_TransportMode; }
			set
			{
				ZString oldValue = base.JE_TransportMode;
				if (!IsCopying && oldValue != value)
				{
					Declaration?.PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.JE_TransportMode = value;

				if (JE_TransportMode != oldValue)
				{
					SetEmptyValueIfRequired();

					if (IsStandAlone && USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty) && (IsTruck || base.IsAir))
					{
						JE_ContainerMode = ContainerModeList.Codes.NonContainerized;
					}
					DefaultDataFromSupplierImporterLinkTransportMode(SupplierImporterLink);
					SetDefaultsToUS_Box29IncludeContainers();
					SetIsHMFApplicable();

					UpdateTransportReferenceIfApplicable(false);
					Invoices.MarkAsNeedingValidation();
					if (IsImport)
					{
						if (IsFormalImport)
						{
							Declaration.AddInfoValidation.ValidateUS_InbondType();
							MessageTypeBasedValueDefaulter.DefaultCargoReleaseTypeIfNecessary();
						}
						SetMasterBillForSouthOriginatedTruckShipment();
						SetMasterBillForHandCarriedTransport();
					}

					ModifySplitDetailsAndNonAMSFlagsIfRequired();
					UpdateMonthlyFilingDetailsIfRequired();
					ClearPipelineNameFieldIfRequired();
					ClearStandAlonePriorNoticeIfRequired();
					DefaultDataRelatedToTruck();
					ClearExpressTrackingIfRequired();
					this.LowestBills.Rebuild();
					this.FilteredBills.Rebuild();
				}
			}
		}

		protected override TransportSupporter GetNewTransportSupporter()
		{
			return new JobDeclarationTransportSupporter(this);
		}

		void SetMasterBillForHandCarriedTransport()
		{
			if (IsACECargoCertificationMode && JE_MasterBill.IsEmpty && IsPersistent && !IsSetMasterBillForHandCarriedTransportSuspended)
			{
				switch (JE_TransportMode)
				{
					case TransportTypeList.Codes.PassengerHandCarried:
						JE_MasterBill = "HANDCARRIED";
						break;
					case TransportTypeList.Codes.Auto:
						JE_MasterBill = "AUTO";
						break;
					case TransportTypeList.Codes.Pedestrian:
						JE_MasterBill = "PEDESTRIAN";
						break;
					case TransportTypeList.Codes.Road:
						JE_MasterBill = "ROAD, OTHER";
						break;
				}
			}
		}

		#region SetMasterBillForHandCarriedTransportSuspender
		public bool IsSetMasterBillForHandCarriedTransportSuspended
		{
			get { return suspendSetMasterBillForHandCarriedTransportIndex > 0; }
		}
		int suspendSetMasterBillForHandCarriedTransportIndex;

		public IDisposable SuspendSetMasterBillForHandCarriedTransport()
		{
			return new SetMasterBillForHandCarriedTransportSuspender(this);
		}

		class SetMasterBillForHandCarriedTransportSuspender : IDisposable
		{
			public SetMasterBillForHandCarriedTransportSuspender(JobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.suspendSetMasterBillForHandCarriedTransportIndex++;
			}

			public void Dispose()
			{
				declaration.suspendSetMasterBillForHandCarriedTransportIndex--;
			}

			readonly JobDeclaration declaration;
		}
		#endregion

		public override ZString JE_VesselName
		{
			get { return base.JE_VesselName; }
			set
			{
				if (!IsCopying && JE_VesselName != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.JE_VesselName = value;
			}
		}

		public bool IsTripID => IsRail || IsRoad || (!(IsHandCarry || IsAir || IsSea) && IsACE);

		[ResourceStringData("BB71BDD1-D9A4-4D94-8755-03293B63D421-HandCarryAir", Caption = "Flight/Folio", IsApplicableMember = nameof(IsHandCarryAir))]
		[ResourceStringData("BB71BDD1-D9A4-4D94-8755-03293B63D421-HandCarryNonAir", Caption = "Voyage", IsApplicableMember = nameof(IsHandCarryNonAir))]
		[ResourceStringData("BB71BDD1-D9A4-4D94-8755-03293B63D421-Rail", Caption = "Trip ID", IsApplicableMember = nameof(IsTripID))]
		public override ZString JE_VoyageFlightNo
		{
			get { return base.JE_VoyageFlightNo; }
			set
			{
				if (!IsCopying && JE_VoyageFlightNo != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.JE_VoyageFlightNo = value;
			}
		}

		void ModifySplitDetailsAndNonAMSFlagsIfRequired()
		{
			if (!AreSplitDetailsRelevant)
			{
				if (!IsACECargoCertificationMode)
				{
					US_SESplitRel = ZString.Empty;
				}

				Bills.ToList().ForEach(x => ((Bill)x).RemoveSplitDetailsIfRequired());
			}

			ClearFTZSplitDetailsIfRequired(Guid.Empty);
			ModifyNonAMSFlags();
		}

		void ModifyNonAMSFlags()
		{
			US_NonAMS = IsNonAMSRelevant && (IsNonAMSJob || !US_GeneralOrderNo.IsEmpty);
		}

		void UpdateMonthlyFilingDetailsIfRequired()
		{
			if (!IsImport || !IsFixedTransportInstallations)
			{
				US_MonthlyFiling = false;
			}
		}

		void ClearPipelineNameFieldIfRequired()
		{
			if (!IsImport || !IsACECargoCertificationMode || !IsFixedTransportInstallations)
			{
				US_PipelineName = ZString.Empty;
			}
		}

		void ClearStandAlonePriorNoticeIfRequired()
		{
			if (!IsImport || IsFixedTransportInstallations)
			{
				US_EnableSPN = false;
			}
		}

		void ClearFTZCurrentTariffIfRequired()
		{
			if (!IsImport || !IsConsumptionFTZ || !IsACECargoCertificationMode)
			{
				InvoiceLines.ForEach(line => ((JobComInvoiceLine)line).US_FTZCurrentTariff = ZString.Empty);
			}
		}

		void ClearExpressTrackingIfRequired()
		{
			if (!IsExpressTrackingNumberRelevant)
			{
				JE_MasterBillExpressTracking = false;
			}
		}

		internal void ClearFTZSplitDetailsIfRequired(ZGuid houseBillPK)
		{
			if (!IsFTZSplitDetailsRelevant || !houseBillPK.IsEmpty)
			{
				foreach (JobComInvoiceHeader invoice in Invoices)
				{
					if (houseBillPK.IsEmpty || invoice.JZ_CU_RelatedHouseBill == houseBillPK)
					{
						invoice.US_SplitShipmentDetail = ZString.Empty;
					}
				}
			}
		}

		public override ZBool US_MonthlyFiling
		{
			get { return base.US_MonthlyFiling; }
			set
			{
				base.US_MonthlyFiling = value;

				if (!US_MonthlyFiling)
				{
					US_PayableMPF = ZDecimal.Zero;
				}
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal US_PayableMPF
		{
			get { return base.US_PayableMPF; }
			set { base.US_PayableMPF = value; }
		}

		protected override string DefaultTransportMode
		{
			get
			{
				string result = base.DefaultTransportMode;

				if (result == TransportTypeList.Codes.Road)
				{
					result = TransportTypeList.Codes.Truck;
				}

				return result;
			}
		}

		public override ZString US_F_AdmissionType
		{
			get => base.US_F_AdmissionType;
			set
			{
				var oldValue = US_F_AdmissionType;
				base.US_F_AdmissionType = value;
				if (oldValue != value && !IsCopying)
				{
					ModifySplitDetailsAndNonAMSFlagsIfRequired();
				}
			}
		}

		protected void SetEmptyValueIfRequired()
		{
			SetEmptyValueTo_JE_ContainerMode_IfRequired();
			SetEmptyValueTo_JE_MasterBill_IfRequired();
			SetEmptyValueTo_JE_VoyageFlightNo_IfRequired();
			SetEmptyValueTo_JE_Folio_IfRequired();
			SetEmptyValueTo_Vessel_IfRequired();
			SetEmptyValueTo_US_SchDArrival_IfRequired();
			SetEmptyValueTo_US_CarrierName_IfRequired();
		}

		void SetEmptyValueTo_JE_ContainerMode_IfRequired()
		{
			if (!IsContainerSupported)
			{
				JE_ContainerMode = ZString.Empty;
			}
		}

		void SetEmptyValueTo_JE_MasterBill_IfRequired()
		{
			if (!(IsSea || IsRail || IsPost || IsTruck || (IsAir && !Env.Registry.IsExpress) || IsConsumptionFTZ || IsFixedTransportInstallations))
			{
				JE_MasterBill = ZString.Empty;
			}
		}

		void SetEmptyValueTo_JE_VoyageFlightNo_IfRequired()
		{
			if (!(IsSea || IsAir || IsRoad || IsTruck) && !IsACE)
			{
				JE_VoyageFlightNo = ZString.Empty;
			}
		}

		void SetEmptyValueTo_JE_Folio_IfRequired()
		{
			if (!IsAir)
			{
				JE_Folio = ZString.Empty;
			}
		}

		void SetEmptyValueTo_Vessel_IfRequired()
		{
			if (!IsSea)
			{
				JE_VesselName = ZString.Empty;
				JE_LloydsIMO = ZString.Empty;
			}
		}

		void SetEmptyValueTo_US_SchDArrival_IfRequired()
		{
			if (!IsSchDArrivalAllowed)
			{
				US_SchDArrival = ZString.Empty;
			}
		}

		void SetEmptyValueTo_US_CarrierName_IfRequired()
		{
			if (!IsUnknownCarrierSCACForExport)
			{
				US_CarrierName = ZString.Empty;
			}
		}

		protected void SetDefaultsToUS_Box29IncludeContainers()
		{
			if (!IsReconMessageType)
			{
				US_Box29IncludeContainers = (base.JE_TransportMode == TransportTypeList.Codes.Sea && IsContainerised);
			}
		}

		void SetIsHMFApplicable()
		{
			if (!IsReconMessageType)
			{
				US_IsHMFApplicable = new HMFApplicableDefaulter().GetCalculatedHMFApplicable(JE_TransportMode, US_EntryType, US_SchDArrival);
			}
		}

		public override ZString JE_RL_NKFinalDestination
		{
			get { return base.JE_RL_NKFinalDestination; }
			set
			{
				ZString oldValue = JE_RL_NKFinalDestination;
				base.JE_RL_NKFinalDestination = value;
				if (!IsCopying && oldValue != JE_RL_NKFinalDestination)
				{
					var countryCode = GetCountryFromPort(FinalDestination);
					if (countryCode == base.US_RN_NKCountryOfDestination)
					{
						US_RN_NKCountryOfDestination = ZString.Empty;
					}
				}
			}
		}

		public override ZString US_UC_NKCountryOfExport
		{
			get { return base.US_UC_NKCountryOfExport; }
			set
			{
				ZString oldValue = US_UC_NKCountryOfExport;
				if (!IsCopying && oldValue != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_UC_NKCountryOfExport = value;
				if (!IsCopying && IsImport)
				{
					var newValue = US_UC_NKCountryOfExport;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_UC_NKCountryOfExport);
					}
				}
			}
		}

		public override ZString JE_RL_NKPortOfLoading
		{
			get { return base.JE_RL_NKPortOfLoading; }
			set
			{
				ZString oldValue = JE_RL_NKPortOfLoading;
				base.JE_RL_NKPortOfLoading = value;
				if (!IsCopying && oldValue != JE_RL_NKPortOfLoading)
				{
					if (!JE_RL_NKPortOfLoading.IsEmpty)
					{
						if (US_RL_NKPortOfExport.IsEmpty && IsExport)
						{
							US_RL_NKPortOfExport = JE_RL_NKPortOfLoading;
						}

						DefaultCountryOfExport(oldValue);

						DefaultScheduleDKLoading();
					}

					if (!IsExport)
					{
						InvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

#if DEBUG
		public override ZString GetPotentialOriginPortNames()
		{
			return USScheduleResolver.MatchingUNLOCO(US_SchDLoading, Factory);
		}
#endif

		void DefaultCountryOfExport(ZString oldValue)
		{
			if (ShouldDefaultCountriesOfOriginAndExportForImport)
			{
				var oldCountryOfExportDefault = oldValue.Left(2);

				if (US_UC_NKCountryOfExport.IsEmpty || US_UC_NKCountryOfExport == oldCountryOfExportDefault)
				{
					US_UC_NKCountryOfExport = JE_RL_NKPortOfLoading.Left(2);
				}
			}
		}

		void DefaultScheduleDKLoading()
		{
			PortOfLadingDefaulter.DefaultPort();
		}

		protected override void SetPortOfLoading(ZString origin)
		{
		}

		protected override void SetPortOfArrival(ZString arrival)
		{
		}

		public ZBool US_SchDLoadingTypeIsDropEdit => PortOfLadingDefaulter.HasMultipleMappingPorts;

		[List(nameof(AddInfoLookups) + "." + nameof(USAddInfoLookups.LoadingSchDList))]
		public override ZString US_SchDLoading
		{
			get { return base.US_SchDLoading; }
			set
			{
				bool hasChanges = base.US_SchDLoading != value;
				base.US_SchDLoading = value;
				if (!IsDataSyncFromShipment && !IsCopying && hasChanges)
				{
					PortOfLadingDefaulter.DefaultUNLOCO();
				}

				if (IsExport && hasChanges)
				{
					DefaultInBondTypeIfRequired();
				}
			}
		}

		public override ZString JE_RL_NKPortOfArrival
		{
			get { return base.JE_RL_NKPortOfArrival; }
			set
			{
				ZString oldValue = JE_RL_NKPortOfArrival;
				base.JE_RL_NKPortOfArrival = value;
				if (!IsCopying && oldValue != JE_RL_NKPortOfArrival)
				{
					if (IsSchDArrivalAllowed)
					{
						if (!IsSettingSchDArrival && US_SchDArrivalTypeIsDropEdit)
						{
							US_SchDArrival = string.Empty;
						}
						PortOfArrivalDefaulter.DefaultPort();
					}
				}
			}
		}

		public ZBool US_SchDArrivalTypeIsDropEdit => PortOfArrivalDefaulter.HasMultipleMappingPorts;

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.DischargeSchDList))]
		public override ZString US_SchDArrival
		{
			get { return base.US_SchDArrival; }
			set
			{
				bool hasChanges = base.US_SchDArrival != value;
				if (!IsCopying && hasChanges)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_SchDArrival = value;
				using (SettingSchDArrivalInProgress())
				{
					if (!IsDataSyncFromShipment && !IsCopying && hasChanges)
					{
						PortOfArrivalDefaulter.DefaultUNLOCO();
					}
				}

				SetIsHMFApplicable();
				DefaultDataRelatedToTruck();
				DefaultEntryDateIfNecessary();
			}
		}

		byte settingSchDArrivalSetterIndex;
		public IDisposable SettingSchDArrivalInProgress()
		{
			return new SettingSchDArrivalSetter(this);
		}

		bool IsSettingSchDArrival
		{
			get { return settingSchDArrivalSetterIndex > 0; }
		}

		class SettingSchDArrivalSetter : IDisposable
		{
			public SettingSchDArrivalSetter(JobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.settingSchDArrivalSetterIndex++;
			}

			readonly JobDeclaration declaration;

			void IDisposable.Dispose()
			{
				declaration.settingSchDArrivalSetterIndex--;
			}
		}

		public override ZString US_RL_NKPortOfExport
		{
			get { return base.US_RL_NKPortOfExport; }
			set
			{
				ZString oldValue = US_RL_NKPortOfExport;
				base.US_RL_NKPortOfExport = value;
				if (!IsCopying && oldValue != US_RL_NKPortOfExport)
				{
					PortOfExportDefaulter.DefaultPort();
				}
			}
		}

		#region UNLOCO_USPortsDefaulter

		UNLOCO_USPortsDefaulter PortOfLadingDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(IsExport ? Schedule.D : Schedule.K, JE_RL_NKPortOfLoading, JE_TransportMode, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return IsExport ?
						USPortLookupsHelper.GetRegionDistrictPorts(Factory, refLocoMapCodes) :
						USPortLookupsHelper.GetForeignPorts(Factory, refLocoMapCodes);
				}

				return fPortOfLadingDefaulter ?? (fPortOfLadingDefaulter =
					new UNLOCO_USPortsDefaulter(Factory, US_SchDLoadingInfo, JE_RL_NKPortOfLoadingInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter fPortOfLadingDefaulter;

		UNLOCO_USPortsDefaulter PortOfArrivalDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(IsExport && US_RN_NKCountryOfDestination != Core.Constants.CountryCodes.PuertoRico && US_RN_NKCountryOfDestination != Core.Constants.CountryCodes.UnitedStates ? Schedule.K : Schedule.D, JE_RL_NKPortOfArrival, JE_TransportMode, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return IsImport ?
						USPortLookupsHelper.GetRegionDistrictPorts(Factory, refLocoMapCodes) :
						USPortLookupsHelper.GetForeignPorts(Factory, refLocoMapCodes);
				}

				return fPortOfArrivalDefaulter ?? (fPortOfArrivalDefaulter =
					new UNLOCO_USPortsDefaulter(Factory, US_SchDArrivalInfo, JE_RL_NKPortOfArrivalInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter fPortOfArrivalDefaulter;

		UNLOCO_USPortsDefaulter PortOfExportDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.D, US_RL_NKPortOfExport, JE_TransportMode, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return IsExport ? USPortLookupsHelper.GetCustomsOfficeCodes(Factory, refLocoMapCodes) : null;
				}

				return fPortOfExportDefaulter ?? (fPortOfExportDefaulter =
					new UNLOCO_USPortsDefaulter(Factory, US_SchDExportInfo, US_RL_NKPortOfExportInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter fPortOfExportDefaulter;

		#endregion

		public ZBool US_SchDExportTypeIsDropEdit => PortOfExportDefaulter.HasMultipleMappingPorts;

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.SchDExportList))]
		public override ZString US_SchDExport
		{
			get { return base.US_SchDExport; }
			set
			{
				bool hasChanges = base.US_SchDExport != value;
				base.US_SchDExport = value;
				if (!IsDataSyncFromShipment && !IsCopying && hasChanges)
				{
					PortOfExportDefaulter.DefaultUNLOCO();
				}

				if (IsExport && hasChanges)
				{
					DefaultInBondTypeIfRequired();
				}
			}
		}

		public ZString US_SchDExportDescription
		{
			get
			{
				var result = (ZString)Core.Constants.FindBoxMessages.InvalidSelection.GetUnresolvedValue();
				var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, US_SchDExport);
				query.AddToFilter(AddInfoLookups.ExportRegionDistrictPorts.CompleteFilter, JoinCondition.And);
				var port = Factory.LoadTop1<ZZRefCusCodeListCombined>(query);
				if (port != null)
				{
					result = port.ZZD_Description;
				}
				return result;
			}
		}

		public bool IsContainerSupported
		{
			get { return IsInBond || IsTruck || IsRail || IsAir || IsSea; }
		}

		public override ZBool ContainersAlwaysRequired
		{
			get
			{
				if (IsENSFormalImport && !IsInBond && !IsElectronicInvoice)
				{
					return false;
				}
				else
				{
					return !IsExWarehouse && !IsConsumptionFTZ && IsContainerised && (IsTruck || IsRail || IsSea);
				}
			}
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList result = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(Bills);
				var cusInBondHeader = (EnterpriseBusinessObject)InBondHeader;
				if (cusInBondHeader != null)
				{
					result.Add(cusInBondHeader);
					result.AddRange(cusInBondHeader.BusinessObjectsWithRelatedEvents);
				}
				return (BusinessObject[])result.ToArray(typeof(BusinessObject));
			}
		}

		public Integration.Customs.US.InBond.ICusInBondHeader InBondHeader
		{
			get { return fInBondHeader ?? (fInBondHeader = (Integration.Customs.US.InBond.ICusInBondHeader)this.GetInBondHeader(CusInBondApplicationCodeList.Codes.InBond, false)); }
		}
		Integration.Customs.US.InBond.ICusInBondHeader fInBondHeader;

		public ZBool HasInBond
		{
			get => InBondHeader != null;
		}

		public override ZDateTime US_DateOfExport
		{
			get
			{
				return GetEffectiveValueToReturn(base.US_DateOfExport, Schema.JE_ExportDate);
			}
			set
			{
				ZDateTime oldValue = US_DateOfExport;
				base.US_DateOfExport = value;
				if (!IsCopying)
				{
					var newValue = US_DateOfExport;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_DateOfExport);
						MarkValuationDatesDirty();
					}
				}
			}
		}

		public override ZBool IsExWarehouse
		{
			get { return IsFormalImport && EntryTypeList.IsExWarehouseType(US_EntryType); }
		}

		public override bool AddingWorkflowByBaseJobDeclaration
		{
			get { return !IsReconMessageType && JE_MessageType != JobMessageTypeList.MoreCodes.Protest; }
		}

		public override ZString JE_MessageType
		{
			get { return base.JE_MessageType; }
			set
			{
				ZString oldValue = base.JE_MessageType;

				if (IsImport && ENSEntryNumber != null && !((ICusEntryNumberParent)this).CanBeChangedOrDeleted(ENSEntryNumber, out var _) && ShowMessageOnGUI != null)
				{
					ShowMessageOnGUI(this, new ShowMessageOnGUIEventArgs("Warning", Res.GetString("4c181486-22c0-482c-914d-09cb256b33cb", "You cannot change the shipment type as the entry number {0} {1} is already lodged at customs.", ENSEntryNumber.CE_EntryType, ENSEntryNumber.CE_EntryNum)));
					return;
				}

				base.JE_MessageType = value;

				if (!IsCopying && oldValue != JE_MessageType)
				{
					if (!IsDataChangeSuspendedByFakeDeclaration)
					{
						if (!IsFormalImport)
						{
							US_IsInvoiceByRequest = false;
							US_EnableENS = false;
							US_EnableCRL = false;
							US_EnableAII = false;
							US_CertifyCargoRelease = false;
							US_InbondType = ZString.Empty;
							ImportEntryNumber = ZString.Empty;
						}

						if (IsImport)
						{
							MarkValuationDatesDirty();
						}

						UpdateTransportReferenceIfApplicable(false);
						DefaultTariffTypeIfNeeded();
						DefaultEntryFilerCode();
						DefaultForImport();
						DefaultStateOfOriginIfPossible();
						DefaultDataFromSupplierImporterLink();
						RefreshIncotermAndChargeFactory();
						SetAESCommodityFilingOption();
						DefaultNumberOfPacksToManifestQtyAndUQIfRequired();
						SetMasterBillForSouthOriginatedTruckShipment();
						SetDefaultValueForDrawback();

						if (IsImport)
						{
							iorWrapper = null;
							if (IsFormalImport)
							{
								new BondDetailsDefaulter().Default(this, US_BondType);
							}

							DefaultScheduleDKLoading();
						}
						else if (IsExport && US_LicenseType.IsEmpty &&
							USCustomsDataRegistry.Instance.DefaultNLRLicenseType.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
						{
							US_LicenseType = USAESLicenseCode.Codes.C33;
						}

						DefaultJE_ApplicationCode();

						ReSynchronizeExportOrganisationIfNeeded();

						ClearExportFieldsIfNeeded();

						RecalculateNoDutyCalcFlag();

						if (IsReconMessageType || IsProtest || IsDrawback)
						{
							JE_TransportMode = ZString.Empty;
						}
						SetDefaultValuesForFTZ();
						RefreshWHSPackLinesAndMarkForValidation();
						UpdateMonthlyFilingDetailsIfRequired();
						var isImport = IsImport;
						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							if (isImport)
							{
								invoiceLine.LineGroupingRanges.MarkAsNeedingValidation();
							}
							invoiceLine.TTBLines.MarkAsNeedingValidation();
							invoiceLine.FDAs.MarkAsNeedingValidation();
							invoiceLine.NHTSALines.MarkAsNeedingValidationIncludingChildren();
						}

						if (oldValue == JobMessageTypeList.Codes.FTZ && !IsFTZAdmission)
						{
							if (FTZCusEntryNum is CusEntryNumber ftzCusEntryNum && !HasFTZTransactionsWithCustoms)
							{
								ftzCusEntryNum.CE_EntryNum = ZString.Empty;
							}
							FilteredInvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.US_LicenseType = ZString.Empty);
						}

						ClearInvoiceValuesIfSame(JE_OH_Importer, JobComInvoiceHeader.Schema.JZ_OH_Buyer, null, true);
						DefaultFromImporterIfNeeded();
						ModifySplitDetailsAndNonAMSFlagsIfRequired();
						US_ConsolACE = ZBool.False;
						if (!SetterSuspender.IsSetterSuspended(JobDeclaration.SuspendKey_DefaultEntrySummaryAndCargoReleaseForImport))
						{
							DefaultEntrySummaryAndCargoReleaseForImport();
						}
						Invoices.ForEach(invoice => invoice.InvalidateJZ_Calc_LinesEnteredCache());
					}

					ReSynchronizeExportOrganisationIfNeeded();
				}
			}
		}

		public override bool IsMessageTypeChangeAnError
		{
			get
			{
				if (HasFTZTransactionsWithCustoms)//FTZ validation implement at here because it only happen when current message type is NOT FTZ.
				{
					return true;
				}
				foreach (CusEntryHeader entry in ActiveEntryHeaders)
				{
					if (entry.HasTransactionsWithCustoms)
					{
						return true;
					}
				}
				return false;
			}
		}

		public const string SuspendKey_DefaultEntrySummaryAndCargoReleaseForImport = "SuspendKey_DefaultEntrySummaryAndCargoReleaseForImport";

		void SetDefaultValuesForFTZ()
		{
			if (IsFTZAdmission)
			{
				US_EntryType = ZString.Empty;//see the getter

				if (US_F_DeliveryCode.IsEmpty)
				{
					US_F_DeliveryCode = FTZDeliveryCodeList.Codes.FinalManifestPortionReported;
				}

				if (FTZYear.IsEmpty)
				{
					FTZYear = (ZDateTime.Today.Year % 100).ToString().PadLeft(2, '0');
				}
				ValidationModes = Business.ValidationModes.FTZAdmissionValidationMode;
			}
			else
			{
				ValidationModesCalculator.UpdateValidationModes(ValidationModes.FTZAdmissionValidationMode, false);
			}
		}

		void SetDefaultValueForDrawback()
		{
			if (IsDrawback && US_DRWPurpose.IsEmpty)
			{
				US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			}
		}

		public override void DefaultValueForFakeDeclaration()
		{
			base.DefaultValueForFakeDeclaration();

			DefaultJE_ApplicationCode();
			if (Importer != null)
			{
				DefaultIOROrgPK();
			}
		}

		protected override bool SupportMultipleBuiltInTypes => IsFormalImport;

		protected override void DefaultJE_ApplicationCode()
		{
			if (!IsDefaultingMessageModeSuspended)
			{
				var defaultValue = JobApplicationCodeList.Codes.ACE;
				if (!IsRecon && !IsDrawback)
				{
					var customsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty);
					if (IsLocalCountryCustomsInterfaceEmpty(customsInterface))
					{
						if (IsExport || IsFTZAdmission)
						{
							defaultValue = DeclarationApplicationCodeList.Codes.Builtin;
						}
					}
					else
					{
						var submissionType = customsInterface.SubmissionType;
						if ((submissionType == DeclarationApplicationCodeList.Codes.Builtin || submissionType == DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted) && (IsExport || IsFTZAdmission))
						{
							defaultValue = DeclarationApplicationCodeList.Codes.Builtin;
						}
						else if (submissionType == DeclarationApplicationCodeList.Codes.Interfaced || submissionType == DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted)
						{
							defaultValue = DeclarationApplicationCodeList.Codes.Interfaced;
						}
					}

					if (IsFormalImport && !JE_ApplicationCode.IsEmpty && JE_ApplicationCode != DeclarationApplicationCodeList.Codes.Builtin && defaultValue != DeclarationApplicationCodeList.Codes.Interfaced)
					{
						defaultValue = JE_ApplicationCode;
					}
				}

				if (defaultValue != JE_ApplicationCode)
				{
					JE_ApplicationCode = defaultValue;
				}
			}
		}

		protected override string SubmissionTypeBuiltinCode => JobApplicationCodeList.Codes.ACE;

		byte suspendDefaultingMessageModeIndex;
		public IDisposable SuspendDefaultingMessageMode()
		{
			return new DefaultingMessageModeIndexSuspender(this);
		}

		bool IsDefaultingMessageModeSuspended
		{
			get { return suspendDefaultingMessageModeIndex > 0; }
		}

		class DefaultingMessageModeIndexSuspender : IDisposable
		{
			public DefaultingMessageModeIndexSuspender(JobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.suspendDefaultingMessageModeIndex++;
			}

			readonly JobDeclaration declaration;

			void IDisposable.Dispose()
			{
				declaration.suspendDefaultingMessageModeIndex--;
			}
		}

		public void RecalculateNoDutyCalcFlag()
		{
			US_NoDutyCalc = IsImportByExternalBroker && CreatedViaBIRD(BIRDApplicationCodeList.Codes.EntrySummary);
		}

		protected override ZBool ShouldSetDefaultValuesFromSupplier
		{
			get
			{
				var result = base.ShouldSetDefaultValuesFromSupplier;

				if (IsExport && GlbDepartment.CurrentDepartment.GE_Export && !GlbDepartment.CurrentDepartment.GE_Import)
				{
					result = Supplier != null && Supplier.MainAddress != null && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Supplier.MainAddress.OA_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates;
				}

				return result;
			}
		}

		protected override void DefaultMessageTypeFromSupplierOrImporter(MessageTypeDefaultingTriggerSource source)
		{
			if (!IsRecon)
			{
				bool hasEntriesWithMessages = false;

				foreach (CusEntryHeader entry in ActiveEntryHeaders)
				{
					if (entry.HasTransactionsWithCustoms)
					{
						hasEntriesWithMessages = true;
						break;
					}
				}

				if (!hasEntriesWithMessages)
				{
					base.DefaultMessageTypeFromSupplierOrImporter(source);
				}
			}
		}

		public override ZGuid JE_GB
		{
			get { return base.JE_GB; }
			set
			{
				bool changed = base.JE_GB != value;
				base.JE_GB = value;
				if (changed && !IsCopying)
				{
					new ClientBranchDesignationDefaulter().Default(this);
					Packages.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_GC
		{
			get { return base.JE_GC; }
			set
			{
				bool hasChanged = base.JE_GC != value;
				base.JE_GC = value;
				if (hasChanged)
				{
					Packages.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZInt JE_TotalNoOfPacks
		{
			get { return base.JE_TotalNoOfPacks; }
			set
			{
				ZInt oldValue = JE_TotalNoOfPacks;
				base.JE_TotalNoOfPacks = value;
				if (!IsCopying && oldValue != JE_TotalNoOfPacks)
				{
					DefaultNumberOfPacksToManifestQtyAndUQIfRequired();
				}
			}
		}

		public override ZString JE_TotalNoOfPacksPackType
		{
			get { return base.JE_TotalNoOfPacksPackType; }
			set
			{
				ZString oldValue = JE_TotalNoOfPacksPackType;
				base.JE_TotalNoOfPacksPackType = value;
				if (!IsCopying && oldValue != JE_TotalNoOfPacksPackType)
				{
					DefaultNumberOfPacksToManifestQtyAndUQIfRequired();

					if (FilteredInvoices.Count > 0)
					{
						RefreshBindingForFilteredInvoices();
					}
				}
			}
		}

		protected void RefreshBindingForFilteredInvoices()
		{
			FilteredInvoices.RefreshBinding();
		}

		void DefaultEntryFilerCode()
		{
			if ((IsImport || IsDrawback || IsRecon || IsReconMessageType) && Branch != null && !IsImportByExternalBroker)
			{
				US_EntryFilerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty).EntryFilerCode;
			}
			else
			{
				US_EntryFilerCode = "";
			}
		}

		public ZString ShippingLineSCACCode => IsAir ? (ShippingLine?.MiscServ?.AirlineTwoCharacterCode ?? ZString.Empty) : ShippingLine?.USLocalCustomsCarrierCode(IsTruck) ?? ZString.Empty;

		public ZString CarrierCodeFor3641Doc
		{
			get
			{
				return !JE_MasterBillIssuerSCAC.IsEmpty ? JE_MasterBillIssuerSCAC : GetCarrierCode();
			}
		}

		public ZString CarrierCodeForEntrySummary
		{
			get { return US_UI_NKCarrierSCAC; }
		}

		public override ZString CarrierCode
		{
			get { return GetCarrierCode(); }
		}

		public override bool CarrierCodeHasChanges
		{
			get
			{
				return US_UI_NKCarrierSCACInfo.HasChanges;
			}
		}

		ZString GetCarrierCode()
		{
			ZString result = ZString.Empty;

			if (IsExport)
			{
				result = US_UI_NKCarrierSCAC;
				if (result.IsEmpty && this.IsCarrierCodeRequired())
				{
					if (IsAir)
					{
						if (ShippingLine != null && ShippingLine.MiscServ.Airline != null)
						{
							var airline = ShippingLine.MiscServ.Airline;
							result = airline.RM_TwoCharacterCode;
						}
					}
					else
					{
						result = ShippingLineSCACCode;
					}
				}
			}
			else
			{
				result = US_UI_NKCarrierSCAC;
			}

			return result;
		}

		[BusinessObjectTestExclude]
		public override ZString DeclarationNumber
		{
			get
			{
				ZString result = DecEntryNumber;

				if (result.IsEmpty)
				{
					result = ActiveEntryHeaders.EntryNumbersAsCommaDelimitedString;
				}

				return result;
			}
			set
			{
				if (IsDrawback)
				{
					ImportEntryNumber = value;
				}

				Validation.ValidateDeclarationNumber();
				DeclarationNumberInfo.RefreshBinding();
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.US_TaxDeferIndicatorList))]
		public override ZString US_TaxDeferIndicator
		{
			get { return base.US_TaxDeferIndicator; }
			set
			{
				base.US_TaxDeferIndicator = value;
				if (!TaxDeferred)
				{
					US_DeferredTaxDueDate = ZDateTime.Empty;
				}
				else if (US_DeferredTaxDueDate.IsEmpty)
				{
					US_DeferredTaxDueDate = new AddInfoJobDeclarationWorkingDate().GenerateDeferredTaxDueDate(this);
				}
			}
		}

		public bool US_DeferredTaxDueDate_ReadOnly
		{
			get { return !TaxDeferred; }
		}

		internal bool IsBulkLiquorTaxDeferred
		{
			get { return US_TaxDeferIndicator == TaxDeferIndicatorList.Codes.BulkLiquorDeferred; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.USStateList))]
		public override ZString US_StateOfOrigin
		{
			get { return base.US_StateOfOrigin; }
			set
			{
				ZString oldValue = US_StateOfOrigin;
				base.US_StateOfOrigin = value;
				if (!IsCopying)
				{
					var newValue = US_StateOfOrigin;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_StateOfOrigin);
						if (IsExport)
						{
							Invoices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZString US_RoutedTransaction
		{
			get { return base.US_RoutedTransaction; }
			set
			{
				ZString oldValue = US_RoutedTransaction;
				base.US_RoutedTransaction = value;
				if (!IsCopying)
				{
					var newValue = US_RoutedTransaction;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_RoutedTransaction);
						if (IsExport)
						{
							Invoices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZString US_RN_NKCountryOfDestination
		{
			get
			{
				var result = base.US_RN_NKCountryOfDestination;
				return result.IsEmpty ? GetCountryFromPort(FinalDestination) : result;
			}
			set
			{
				var oldValue = US_RN_NKCountryOfDestination;
				base.US_RN_NKCountryOfDestination = value == GetCountryFromPort(FinalDestination) ? ZString.Empty : value;
				if (!IsCopying && oldValue != US_RN_NKCountryOfDestination)
				{
					SetEmptyValueTo_US_SchDArrival_IfRequired();
				}
			}
		}

		public override ZString US_EntryType
		{
			get { return IsFTZAdmission ? (ZString)EntryTypeList.Codes.WarehouseFTZ : base.US_EntryType; }
			set
			{
				var oldValue = US_EntryType;
				if (oldValue != value)
				{
					var isOldValueConsumptionFTZ = IsConsumptionFTZ;
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
					base.US_EntryType = value;
					if (!IsCopying && oldValue != US_EntryType)
					{
						ResetContainersAndEquipmentsOnDeclaration_List();
					}
					if (US_DomesticCargo && oldValue == EntryTypeList.Codes.Warehouse)
					{
						US_DomesticCargo = false;
					}

					if (US_EnableCRL_ReadOnly)
					{
						US_EnableCRL = false;
					}

					if (EntryTypeList.HasCargoEnteredUSTerritory(US_EntryType))
					{
						if (US_EntryType != EntryTypeList.Codes.ReWarehouse)
						{
							US_CertifyCargoRelease = false;
						}
						US_InbondType = ZString.Empty;
						US_IsHMFApplicable = ZString.Empty;
					}

					SetIsHMFApplicable();

					if (EntryTypeList.IsQuotaVisa(US_EntryType))
					{
						US_LiveEntryIndicator = ZString.Empty;
					}

					if (IsImport)
					{
						DefaultUS_EntryModeIfNecessary();

						if (oldValue == EntryTypeList.Codes.LowValue || value == EntryTypeList.Codes.LowValue)
						{
							new BondDetailsDefaulter().Default(this, ZString.Empty);
						}

						if (IsSingleTransactionBond && US_BondCalcCode != SEBCalculationList.Codes.MAN)
						{
							if (IsTemporaryImportationBond)
							{
								US_BondCalcCode = SEBCalculationList.Codes.TIB;
							}
							else if (US_EntryType == EntryTypeList.Codes.TradeFair)
							{
								US_BondCalcCode = SEBCalculationList.Codes.MAN;
								US_BondAmount = 0;
							}
							else if (US_EntryType == EntryTypeList.Codes.PermanentExhibition)
							{
								US_BondCalcCode = SEBCalculationList.Codes.EXH;
							}
							else
							{
								US_BondCalcCode = SEBCalculationList.Codes.DEF;
							}
						}
						else
						{
							US_BondCalcCode = ZString.Empty;
						}

						MarkReconIndicatorsDirty();
						ModifyNonAMSFlags();
						MessageTypeBasedValueDefaulter.DefaultCargoReleaseTypeIfNecessary();
						DefaultBondedWarehouseAddressFromImporterFTZWhenEmpty();
						DefaultFDADateOnDeclarationLevel();
						ClearExpressTrackingIfRequired();

						if (!SetterSuspender.IsSetterSuspended(JobDeclaration.SuspendKey_DefaultEntrySummaryAndCargoReleaseForImport))
						{
							DefaultEntrySummaryAndCargoReleaseForImport();
						}
					}

					RefreshWHSPackLinesAndMarkForValidation();

					if (!IsCopying && EntryTypeList.IsQuotaVisa(oldValue) != EntryTypeList.IsQuotaVisa(US_EntryType))
					{
						ApportionInvoiceWeight(null);
						InvoiceLines.MarkAsNeedingValidation();
					}

					if (InvoiceLines.Count > 0)
					{
						var isConsumptionFTZ = IsConsumptionFTZ;
						var isDrawback = IsDrawback;
						var entryTypeChangeFromOrToConsumptionFTZ = isOldValueConsumptionFTZ || isConsumptionFTZ;

						InvoiceLines.OfType<JobComInvoiceLine>().ForEach(line =>
						{
							if (!isConsumptionFTZ)
							{
								line.US_ZoneStatus = ZString.Empty;
								line.US_FTZCurrentTariff = ZString.Empty;
							}
							if (isDrawback && line.ShouldReCalculateDrawbackData)
							{
								var claims = line.Claims;
								if (!isConsumptionFTZ)
								{
									claims.HMFClaim.DefaultDeclaredAmount();
									claims.MPFClaim.DefaultDeclaredAmount();
								}
								claims.IRTaxClaim.Default_99ClaimedDutyAndCalculatedAmount();
							}

							if (entryTypeChangeFromOrToConsumptionFTZ)
							{
								line.RecalculateSupTariffsWhenCriteriaChanges();
							}
						});
					}

					SetRejectedMerchandiseReason(US_EntryType);
					SetProvisionSectionForACEDrawback(US_EntryType);

					if (EntryTypeList.IsADD_CVDInvolved(US_EntryType))
					{
						SetDefaultDeductADDCVDDutyForInvoices();
					}
				}
			}
		}
		bool reconIndicatorsDirty;

		void SetDefaultDeductADDCVDDutyForInvoices()
		{
			Invoices.OfType<JobComInvoiceHeader>().ForEach(header =>
			{
				header.SetDefaultDeductADDCVDDutyIfApplicable();
			});
		}

		internal void MarkReconIndicatorsDirty()
		{
			if (IsFormalImport && !US_FixRecon)
			{
				reconIndicatorsDirty = true;
			}
		}

		public bool IsReconIndicatorsDirty
		{
			get { return reconIndicatorsDirty; }
		}

		void SetRejectedMerchandiseReason(ZString entryType)
		{
			if (IsACEDrawback)
			{
				switch (entryType)
				{
					case ACEDrawbackProvisionsList.Codes._03:
					case ACEDrawbackProvisionsList.Codes._17:
						US_DRWRejectedMerchandiseReason = DrawbackRejectedMerchandiseReasonList.Codes.NCS;
						break;
					case ACEDrawbackProvisionsList.Codes._04:
					case ACEDrawbackProvisionsList.Codes._18:
						US_DRWRejectedMerchandiseReason = DrawbackRejectedMerchandiseReasonList.Codes.SWC;
						break;
					case ACEDrawbackProvisionsList.Codes._05:
					case ACEDrawbackProvisionsList.Codes._19:
						US_DRWRejectedMerchandiseReason = DrawbackRejectedMerchandiseReasonList.Codes.DTI;
						break;
					default:
						US_DRWRejectedMerchandiseReason = ZString.Empty;
						break;
				}
			}
		}

		void SetProvisionSectionForACEDrawback(ZString entryType)
		{
			if (IsACEDrawback)
			{
				US_DRWSection = ACEDrawbackProvisionsList.GetSectionDescriptionFromCode(Factory, entryType).Left(AutoUSAddInfo.Schema.US_DRWSectionMaxLength);
			}
		}

		void DefaultUS_EntryModeIfNecessary()
		{
			if (IsImport)
			{
				var userBranch = Enterprise.MasterFiles.Business.GlbStaff.CurrentUser.HomeBranch;
				var isElectronicFilingAllowed = EntryTypeList.IsElectronicFilingAllowed(IsACE, US_EntryType);
				var branchPortsRelations = USCustomsDataRegistry.Instance.BranchDistrictPortRelationship.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty);
				var mappedPorts = branchPortsRelations.Cast<BranchDistrictPort>().Where(x => x.BranchPK == userBranch.PK);

				if (userBranch != null && !US_SchDEntry.IsEmpty && mappedPorts.Any())
				{
					var mapping = mappedPorts.FirstOrDefault(x => US_SchDEntry.StartsWith(x.PortCode, StringComparison.CurrentCulture));
					var isRLF = mapping == null;
					US_EntryMode = isRLF && (US_EntryType.IsEmpty || isElectronicFilingAllowed) ? EntryModeList.Codes.RLF : "";
				}
				else
				{
					if (!US_EntryType.IsEmpty && !isElectronicFilingAllowed)
					{
						US_EntryMode = "";
					}
				}
			}
			else
			{
				US_EntryMode = "";
			}
		}

		public override ZString US_GeneralOrderNo
		{
			get { return base.US_GeneralOrderNo; }
			set
			{
				base.US_GeneralOrderNo = value;
				ModifyNonAMSFlags();
			}
		}

		public override ZBool US_EnableENS
		{
			get { return base.US_EnableENS; }
			set
			{
				bool hasChanges = base.US_EnableENS != value;
				base.US_EnableENS = value;

				if (hasChanges)
				{
					if (US_EnableENS && IsImport && !IsFTZAdmission && IsACE && !US_EnableCRL)
					{
						var eventSuspender = US_EnableSPN ? (IDisposable)new CargoReleaseTypeChangingEventSuspender(this) : new DisposableObject();
						try
						{
							US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
						}
						finally
						{
							eventSuspender.Dispose();
						}
					}

					if (US_EnableENS && IsImport && !IsExWarehouse && !IsACECargoRelease && !IsFixedTransportInstallations)
					{
						if (IsEnableTwoStepProcess)
						{
							US_EnableCRL = true;
						}
						else
						{
							US_CertifyCargoRelease = true;
						}
					}

					if (!IsDrawback)
					{
						ValidationModesCalculator.UpdateValidationModes(ValidationModes.EntrySummary, US_EnableENS);
						RefreshInvoiceLinesOGAPGAIndicators(false);
					}

					SetupPackableInvoiceLinesIfNeeded();
				}
			}
		}

		public bool US_InbondType_ReadOnly
		{
			get { return IsExWarehouse; }
		}

		public override ZString US_InbondType
		{
			get { return base.US_InbondType; }
			set
			{
				ZString oldValue = US_InbondType;
				base.US_InbondType = value;
				if (!IsCopying)
				{
					var newValue = US_InbondType;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_InbondType);
						if (IsExport)
						{
							Invoices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZString US_ImportEntryNo
		{
			get { return base.US_ImportEntryNo; }
			set
			{
				ZString oldValue = US_ImportEntryNo;
				base.US_ImportEntryNo = value;
				if (!IsCopying)
				{
					var newValue = US_ImportEntryNo;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_ImportEntryNo);
						if (IsExport)
						{
							Invoices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZString US_HazardousCargo
		{
			get { return base.US_HazardousCargo; }
			set
			{
				ZString oldValue = US_HazardousCargo;
				base.US_HazardousCargo = value;
				if (!IsCopying)
				{
					var newValue = US_HazardousCargo;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_HazardousCargo);
						if (IsExport)
						{
							Invoices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZString US_ForeignTradeZone
		{
			get { return base.US_ForeignTradeZone; }
			set
			{
				ZString oldValue = US_ForeignTradeZone;
				base.US_ForeignTradeZone = value;
				if (!IsCopying)
				{
					var newValue = US_ForeignTradeZone;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_ForeignTradeZone);
						if (IsExport)
						{
							Invoices.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZString US_TransactionsRelated
		{
			get { return base.US_TransactionsRelated; }
			set
			{
				if (!IsReconMessageType)
				{
					ZString oldValue = US_TransactionsRelated;
					base.US_TransactionsRelated = value;
					if (!IsCopying)
					{
						var newValue = US_TransactionsRelated;
						if (oldValue != newValue)
						{
							ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.US_TransactionsRelated);
							if (IsExport)
							{
								Invoices.MarkAsNeedingValidation();
							}
						}
					}
				}
			}
		}

		public override ZDateTime US_EntryDate
		{
			get { return base.US_EntryDate; }
			set
			{
				bool hasChanges = base.US_EntryDate != value;
				if (hasChanges)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_EntryDate = value;
				if (hasChanges && !IsCopying)
				{
					InvoiceLines.MarkAsNeedingValidation();
					new DeclarationPrelimStatementDetailsDefaulter().Default(this);
					CalculatePaymentDueDate();

					DefaultDestinationDateFromEntryDate();
					US_DeferredTaxDueDate = new AddInfoJobDeclarationWorkingDate().GenerateDeferredTaxDueDate(Declaration);
				}
			}
		}

		void DefaultDestinationDateFromEntryDate()
		{
			if ((JE_DateAtFinalDestination.IsEmpty || US_EntryDate > JE_DateAtFinalDestination) && US_EntryDate.IsValidSmallDateTime)
			{
				JE_DateAtFinalDestination = US_EntryDate;
			}
		}

		public override ZString US_LicenseType
		{
			get { return base.US_LicenseType; }
			set
			{
				ZString oldValue = US_LicenseType;
				base.US_LicenseType = value;
				if (oldValue != value && !IsCopying)
				{
					ClearInvoiceValuesIfSame(value, JobComInvoiceHeader.Schema.US_LicenseType);
					if (IsExport)
					{
						Invoices.MarkAsNeedingValidation();
						InvoiceLines.MarkAsNeedingValidation();

						var exportDate = this.GetEffectiveDateForECR();
						if (exportDate.IsValid)
						{
							var licenseNo = UniversalReferenceDataHelper.GetLicenseNo(Factory, value, exportDate);
							US_LicenseNo = licenseNo.SubstringSafe(0, AutoUSAddInfo.Schema.US_LicenseNoMaxLength);
						}
					}
				}
			}
		}

		public override ZGuid JE_OH_Supplier
		{
			get { return base.JE_OH_Supplier; }
			set
			{
				ZGuid oldValue = JE_OH_Supplier;
				if (oldValue != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.JE_OH_Supplier = value;
				if (!IsCopying && oldValue != JE_OH_Supplier)
				{
					Invoices.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
					USDeclaration.MarkAsNeedingValidation();
					MessageTypeBasedValueDefaulter.DefaultValueFromSupplier(oldValue);
				}
			}
		}

		#region IOROrgPK

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.ImporterOfRecordList))]
		[RelatedBusinessObject("IOR")]
		public ZGuid IOROrgPK
		{
			get { return base.JE_OA_DeclarantAddress_ZAddress.OrgPK; }
			set { base.JE_OA_DeclarantAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo IOROrgPKInfo => GetWrappedZPropertyInfo(Schema.IOROrgPK, x => JE_OA_DeclarantAddress_ZAddress.OrgPKInfo);

		#endregion

		public override ZGuid JE_OA_DeclarantAddress
		{
			get { return base.JE_OA_DeclarantAddress; }
			set
			{
				bool hasChanges = base.JE_OA_DeclarantAddress != value;
				if (!IsCopying && hasChanges)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
					Invoices.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
				}

				base.JE_OA_DeclarantAddress = value;

				if (hasChanges && !IsCopying)
				{
					iorWrapper = null;

					if (!IsACEDrawback || US_AcceleratedClaimInd)
					{
						new BondDetailsDefaulter().Default(this, US_BondType);
					}

					if (IsFormalImport)
					{
						new PaymentDetailsDefaulter().Default(this);
						DefaultReconciliationDetailsFromIOR();
						MarkReconIndicatorsDirty();
					}
					if (IsFormalImport || IsDrawback)
					{
						DefaultNotifyParty();
					}
				}
			}
		}

		void DefaultNotifyParty()
		{
			var iorWrapper = IORWrapper;
			var notifyParty = iorWrapper != null ? iorWrapper.NotifyParty : null;
			if (notifyParty != null)
			{
				JE_OH_NotifyParty = notifyParty.PK;
			}
		}

		public ZString IORName
		{
			get { return IOR != null ? IOR.OH_FullNameTruncated : ZString.Empty; }
		}

		public override ZDateTime US_PresentationDate
		{
			get { return base.US_PresentationDate; }
			set
			{
				bool hasChanges = base.US_PresentationDate != value;
				if (hasChanges)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_PresentationDate = value;

				if (hasChanges && !IsCopying)
				{
					new DeclarationPrelimStatementDetailsDefaulter().Default(this);
					CalculatePaymentDueDate();
				}
			}
		}

		public bool US_PresentationDate_ReadOnly
		{
			get { return US_EntryDateElectionCode == EntryDateElectionCodeList.Codes.ArrivalDate; }
		}

		void DefaultPresentationDateIfNeeded()
		{
			if (US_PresentationDate.IsEmpty && US_EstimatedEntryDate.IsValid && IsWeeklyEstimateFilingDate)
			{
				US_PresentationDate = US_EstimatedEntryDate;
			}
		}

		public ZDate EntryDate
		{
			get { return Declaration.JE_EntryAuthorisationDate.IsEmpty ? Declaration.US_PresentationDate.Date : Declaration.JE_EntryAuthorisationDate.Date; }
		}

		public override ZDateTime US_EstimatedEntryDate
		{
			get { return base.US_EstimatedEntryDate; }
			set
			{
				bool hasChanges = base.US_EstimatedEntryDate != value;
				if (hasChanges)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_EstimatedEntryDate = value;
				if (hasChanges && !IsCopying)
				{
					new DeclarationPrelimStatementDetailsDefaulter().Default(this);
					CalculatePaymentDueDate();
					US_DeferredTaxDueDate = new AddInfoJobDeclarationWorkingDate().GenerateDeferredTaxDueDate(Declaration);
					DefaultPresentationDateIfNeeded();
				}
			}
		}

		public override ZString US_LiveEntryIndicator
		{
			get { return base.US_LiveEntryIndicator; }
			set
			{
				bool hasChanges = base.US_LiveEntryIndicator != value;
				base.US_LiveEntryIndicator = value;
				if (hasChanges && !IsCopying)
				{
					if (US_PresentationDate.IsValid || US_EstimatedEntryDate.IsValid || JE_EntryAuthorisationDate.IsValid || US_EntryDate.IsValid)
					{
						CalculatePaymentDueDate();
					}
				}
			}
		}

		public OrgHeaderWrapper IORWrapper
		{
			get
			{
				if (iorWrapper == null && IOR != null)
				{
					iorWrapper = OrgHeaderWrapper.New(IOR);
				}

				return iorWrapper;
			}
		}
		OrgHeaderWrapper iorWrapper;

		public OrgHeaderWrapper ImporterWrapper
		{
			get
			{
				if (fImporterWrapper == null && Importer != null)
				{
					fImporterWrapper = OrgHeaderWrapper.New(Importer);
				}

				return fImporterWrapper;
			}
		}
		OrgHeaderWrapper fImporterWrapper;

		public OrgHeaderWrapper UltimateConsigneeWrapper
		{
			get
			{
				if (ultimateConsigneeWrapper == null && ConsigneeOrgAddress != null)
				{
					ultimateConsigneeWrapper = OrgHeaderWrapper.New(ConsigneeOrgAddress);
				}

				return ultimateConsigneeWrapper;
			}
		}
		OrgHeaderWrapper ultimateConsigneeWrapper;

		void DefaultReconciliationDetailsFromIOR()
		{
			if (IOR != null && EntryTypeList.IsValidForRecon(US_EntryType))
			{
				US_FileTheirOwnRecon = IORWrapper.ZO_FileTheirOwnRecon;
			}
		}

		public override ZString US_BondCalcCode
		{
			get { return base.US_BondCalcCode; }
			set
			{
				bool hasChanges = base.US_BondCalcCode != value;
				base.US_BondCalcCode = value;

				if (!US_BondCalcCode.IsEmpty && hasChanges && !IsCopying && IsFormalImport)
				{
					DefaultBondAmountForSingleTransactionBond();
				}
			}
		}

		public override ZString US_BondType
		{
			get { return base.US_BondType; }
			set
			{
				if (!IsAddInfoPropertySettingSuspended)
				{
					var hasChanges = base.US_BondType != value;
					base.US_BondType = value;

					DefaultBondCalcCode();

					if (hasChanges && !IsCopying)
					{
						if (!US_BondType.IsEmpty && (IsFormalImport || IsDrawback))
						{
							new BondDetailsDefaulter().DefaultWhenBondTypeChanges(this, US_BondType);

							RefreshBondDetailFields();
						}

						DefaultDesignationCode();

						ClearBondDetailFields(Schema.US_BondType, Schema.US_BondAmount, Schema.US_BondProducerAccNo, Schema.US_SuretyCode, Schema.US_BondSuperseding, Schema.US_BondDispositionCode, Schema.US_CBPBondNo);
					}
				}
			}
		}

		[ReadOnly(true)]
		public override ZString US_InsuranceDisposition
		{
			get => base.US_InsuranceDisposition;
			set => base.US_InsuranceDisposition = value;
		}

		public ZString InsuranceDispositionDescription
		{
			get
			{
				var returnVal = ZString.Empty;
				switch (US_InsuranceDisposition)
				{
					case InsuranceDispositionCodeList.Codes.AcceptedByCBP:
						returnVal = "Accepted";
						break;
					case InsuranceDispositionCodeList.Codes.DataAcceptedBySuretyPendingReview:
						returnVal = "Pending Review";
						break;
					case InsuranceDispositionCodeList.Codes.BondRejectedByCBPSeeAttachedErrors:
						returnVal = "Rejected";
						break;
					case InsuranceDispositionCodeList.Codes.DataErrorsSeeAttachedErrors:
						returnVal = "Data Error";
						break;
					case InsuranceDispositionCodeList.Codes.SentToSurety:
						returnVal = "Sent";
						break;
				}
				return returnVal;
			}
		}

		void DefaultDesignationCode()
		{
			if (IsACE || IsACEDrawback)
			{
				if (US_BondType == BondTypeList.Codes.SingleTransactionBond && US_BondDesignationCode.IsEmpty)
				{
					US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;
				}
				else if (US_BondType == BondTypeList.Codes.ContinuousBond && !US_BondDesignationCode.IsEmpty)
				{
					US_BondDesignationCode = ZString.Empty;
				}
			}
		}

		public override ZString US_BondDesignationCode
		{
			get
			{
				var result = base.US_BondDesignationCode;

				if (result.IsEmpty && US_BondType == BondTypeList.Codes.ContinuousBond)
				{
					result = BondDesignationCodeList.Codes.BasicBond;
				}

				return result;
			}
			set
			{
				base.US_BondDesignationCode = value;
			}
		}

		void DefaultBondCalcCode()
		{
			if (IsSingleTransactionBond)
			{
				if (US_BondCalcCode.IsEmpty)
				{
					if (IsTemporaryImportationBond)
					{
						US_BondCalcCode = US_TIBMotorVehicles == YesNoList.Codes.Yes && US_TIBMVNonConforming ? SEBCalculationList.Codes.MSC : SEBCalculationList.Codes.TIB;
					}
					else if (US_EntryType == EntryTypeList.Codes.PermanentExhibition)
					{
						US_BondCalcCode = SEBCalculationList.Codes.EXH;
					}
					else if (US_BondType == BondTypeList.Codes.SingleTransactionBond && (HasFDATariffsToBeDeclared || HasOGATariffsToBeDeclared))
					{
						US_BondCalcCode = SEBCalculationList.Codes.MAN;
					}
					else
					{
						US_BondCalcCode = SEBCalculationList.Codes.DEF;
					}
				}
			}
			else
			{
				US_BondCalcCode = ZString.Empty;
			}
		}

		void RefreshBondDetailFields()
		{
			US_BondAmountInfo.RefreshBinding();
			US_BondProducerAccNoInfo.RefreshBinding();
			US_SuretyCodeInfo.RefreshBinding();
		}

		void ClearBondDetailFields(string bondTypeFieldName, string bondAmountFieldName, string accNoFieldName, string suretyCodeFieldName, string bondSuperseding, string dispositionCodeFieldName, string bondNoFileName)
		{
			var bondType = (ZString)this[bondTypeFieldName];

			if (!BondTypeList.IsRelevantFor(bondType, bondAmountFieldName))
			{
				this[bondAmountFieldName] = ZDecimal.Zero;
			}

			if (!BondTypeList.IsRelevantFor(bondType, accNoFieldName))
			{
				this[accNoFieldName] = ZString.Empty;
			}

			if (!BondTypeList.IsRelevantFor(bondType, suretyCodeFieldName))
			{
				this[suretyCodeFieldName] = ZString.Empty;
			}

			if (!string.IsNullOrEmpty(bondSuperseding) && !BondTypeList.IsRelevantFor(bondType, bondSuperseding))
			{
				this[bondSuperseding] = ZString.Empty;
			}

			if (!BondTypeList.IsRelevantFor(bondType, dispositionCodeFieldName))
			{
				this[dispositionCodeFieldName] = ZString.Empty;
			}

			if (!BondTypeList.IsRelevantFor(bondType, bondNoFileName))
			{
				this[bondNoFileName] = ZString.Empty;
			}
		}

		[ReadOnlyMember(nameof(US_BondType2_ReadOnly))]
		public override ZString US_BondType2
		{
			get { return base.US_BondType2; }
			set
			{
				bool hasChanges = base.US_BondType2 != value;
				base.US_BondType2 = value;

				if (hasChanges && !IsCopying)
				{
					ClearBondDetailFields(Schema.US_BondType2, Schema.US_BondAmount2, Schema.US_BondProducerAccNo2, Schema.US_ADDCVDSuretyCode, "", Schema.US_BondDispositionCode2, Schema.US_CBPBondNo2);
				}
			}
		}

		bool US_BondType2_ReadOnly
		{
			get { return Declaration != null && Declaration.IsLowValue; }
		}

		protected override bool IsCustomsHeaderAmendmentATotalReplacement
		{
			get { return true; }
		}

		protected override bool IsCustomsLineAmendmentATotalReplacement
		{
			get { return true; }
		}

		public override ZGuid JE_OA_ManufacturerAddress
		{
			get
			{
				return base.JE_OA_ManufacturerAddress;
			}
			set
			{
				ZGuid oldValue = base.JE_OA_ManufacturerAddress;
				if (!IsCopying && oldValue != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}

				base.JE_OA_ManufacturerAddress = value;
				if (oldValue != JE_OA_ManufacturerAddress)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OA_ManufacturerAddress);
				}

				Invoices.MarkAsNeedingValidation();
				InvoiceLines.MarkAsNeedingValidation();

				if (!IsCopying)
				{
					var newValue = base.JE_OA_ManufacturerAddress;
					if (oldValue != newValue && !IsCreatedFromUSLowValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.JZ_OA_ManufacturerAddress);
					}
				}
			}
		}

		public override ZGuid JE_OH_NotifyParty
		{
			get { return base.JE_OH_NotifyParty; }
			set
			{
				var oldValue = base.JE_OH_NotifyParty;
				base.JE_OH_NotifyParty = value;
				if (oldValue != JE_OH_NotifyParty)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OH_NotifyParty);
				}
			}
		}

		public override ZGuid JE_OA_SellerAddress
		{
			get { return base.JE_OA_SellerAddress; }
			set
			{
				var oldValue = JE_OA_SellerAddress;
				base.JE_OA_SellerAddress = value;
				if (oldValue != JE_OA_SellerAddress)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OA_SellerAddress);
				}

				if (!IsCopying)
				{
					var newValue = JE_OA_SellerAddress;
					if (oldValue != newValue)
					{
						if (!IsCreatedFromUSLowValue)
						{
							ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.JZ_OA_SellerAddress);
						}
						Invoices.MarkAsNeedingValidation();
						InvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid JE_OH_SellingAgent
		{
			get { return base.JE_OH_SellingAgent; }
			set
			{
				var oldValue = JE_OH_SellingAgent;
				base.JE_OH_SellingAgent = value;
				if (!IsCopying)
				{
					var newValue = JE_OH_SellingAgent;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.JZ_OH_SellingAgent);
					}
				}
			}
		}

		public override ZGuid JE_OH_Buyer
		{
			get { return base.JE_OH_Buyer; }
			set
			{
				var oldValue = JE_OH_Buyer;
				base.JE_OH_Buyer = value;
				if (oldValue != JE_OH_Buyer)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OH_Buyer);
				}

				if (!IsCopying)
				{
					var newValue = JE_OH_Buyer;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.BuyerOrgPK);
					}
				}
			}
		}

		public override ZGuid JE_OH_BuyingAgent
		{
			get { return base.JE_OH_BuyingAgent; }
			set
			{
				var oldValue = JE_OH_BuyingAgent;
				base.JE_OH_BuyingAgent = value;
				if (!IsCopying)
				{
					var newValue = JE_OH_BuyingAgent;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.JZ_OH_BuyerAgent);
					}
				}
			}
		}

		public override ZGuid JE_OH_ExternalBroker
		{
			get { return base.JE_OH_ExternalBroker; }
			set
			{
				var oldValue = base.JE_OH_ExternalBroker;
				base.JE_OH_ExternalBroker = value;

				if (oldValue != JE_OH_ExternalBroker)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OH_ExternalBroker);
				}
			}
		}

		public override ZGuid JE_OH_ControllingAgent
		{
			get { return base.JE_OH_ControllingAgent; }
			set
			{
				var oldValue = base.JE_OH_ControllingAgent;
				base.JE_OH_ControllingAgent = value;

				if (oldValue != JE_OH_ControllingAgent)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OH_ControllingAgent);
				}
			}
		}

		public override ZGuid JE_OH_ControllingCustomer
		{
			get { return base.JE_OH_ControllingCustomer; }
			set
			{
				var oldValue = base.JE_OH_ControllingCustomer;
				base.JE_OH_ControllingCustomer = value;

				if (oldValue != JE_OH_ControllingCustomer)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OH_ControllingCustomer);
				}
			}
		}

		public override ZGuid JE_OH_Exporter
		{
			get { return base.JE_OH_Exporter; }
			set
			{
				var oldValue = JE_OH_Exporter;
				if (!IsCopying && oldValue != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.JE_OH_Exporter = value;
				if (oldValue != JE_OH_Exporter)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OH_Exporter);
				}

				if (!IsCopying)
				{
					var newValue = JE_OH_Exporter;
					if (oldValue != newValue)
					{
						Invoices.MarkAsNeedingValidation();
						InvoiceLines.MarkAsNeedingValidation();
						MessageTypeBasedValueDefaulter.DefaultValueFromExporter(oldValue);
					}
				}
			}
		}

		public override ZGuid JE_OA_SoldToPartyAddress
		{
			get { return base.JE_OA_SoldToPartyAddress; }
			set
			{
				var oldValue = JE_OA_SoldToPartyAddress;
				base.JE_OA_SoldToPartyAddress = value;
				if (oldValue != JE_OA_SoldToPartyAddress)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OA_SoldToPartyAddress);
				}

				if (!IsCopying)
				{
					var newValue = JE_OA_SoldToPartyAddress;
					if (oldValue != newValue && !IsCreatedFromUSLowValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.JZ_OA_SoldToPartyAddress);
					}
					Invoices.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_OA_ShipToPartyAddress
		{
			get { return base.JE_OA_ShipToPartyAddress; }
			set
			{
				var oldValue = JE_OA_ShipToPartyAddress;
				base.JE_OA_ShipToPartyAddress = value;
				if (oldValue != JE_OA_ShipToPartyAddress)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OA_ShipToPartyAddress);
				}

				if (!IsCopying)
				{
					var newValue = JE_OA_ShipToPartyAddress;
					if (oldValue != newValue)
					{
						if (!IsCreatedFromUSLowValue)
						{
							ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.JZ_OA_ShipToPartyAddress);
						}
						Invoices.MarkAsNeedingValidation();
						InvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid JE_OA_DistributorAddress
		{
			get { return base.JE_OA_DistributorAddress; }
			set
			{
				var oldValue = JE_OA_DistributorAddress;
				base.JE_OA_DistributorAddress = value;
				if (oldValue != JE_OA_DistributorAddress)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OA_DistributorAddress);
				}

				if (!IsCopying)
				{
					var newValue = JE_OA_DistributorAddress;
					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.JZ_OA_DistributorAddress);
						Invoices.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid JE_OA_PackagerAddress
		{
			get { return base.JE_OA_PackagerAddress; }
			set
			{
				var oldValue = JE_OA_PackagerAddress;
				base.JE_OA_PackagerAddress = value;
				if (!IsCopying)
				{
					var newValue = JE_OA_PackagerAddress;
					if (oldValue != JE_OA_PackagerAddress)
					{
						ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OA_PackagerAddress);
					}

					if (oldValue != newValue)
					{
						ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.JZ_OA_PackagerAddress);
						Invoices.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid JE_OA_ShipperAddress
		{
			get { return base.JE_OA_ShipperAddress; }
			set
			{
				var oldValue = JE_OA_ShipperAddress;
				base.JE_OA_ShipperAddress = value;
				if (oldValue != JE_OA_ShipperAddress)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, JE_ScreeningStatus, Factory, JE_OA_ShipperAddress);
				}

				if (!IsCopying)
				{
					var newValue = JE_OA_ShipperAddress;
					if (oldValue != newValue)
					{
						if (!IsCreatedFromUSLowValue)
						{
							ClearInvoiceValuesIfSame(newValue, JobComInvoiceHeader.Schema.JZ_OA_ShipperAddress);
						}
						Invoices.MarkAsNeedingValidation();
					}
				}
			}
		}

		internal void ClearInvoiceValuesIfSame(IZType decValue, string invoiceFieldName, Action<JobComInvoiceHeader> extraAction = null, bool isClearForciblyWhenIsDefaultValue = false)
		{
			if (IsPersistent)
			{
				EffectiveValueManager.ClearValueIfSame(decValue, invoiceFieldName, Invoices.Cast<JobComInvoiceHeader>(), extraAction, isClearForciblyWhenIsDefaultValue);
			}
		}

		T GetEffectiveValueToReturn<T>(T baseValue, string fieldName) where T : IZType
		{
			T result = baseValue;

			if (result.IsEmpty)
			{
				result = (T)this[fieldName];
			}

			return result;
		}

		public override ZString US_IsHMFApplicable
		{
			get { return base.US_IsHMFApplicable; }
			set
			{
				ZString oldValue = base.US_IsHMFApplicable;
				base.US_IsHMFApplicable = value;

				if (Declaration.IsImport && US_IsHMFApplicable != oldValue && US_IsHMFApplicable == YesNoDefaultList.Codes.No)
				{
					foreach (JobComInvoiceHeader header in Declaration.Invoices)
					{
						foreach (JobComInvoiceLine line in header.JobComInvoiceLines)
						{
							line.FeeCusCodes.RemoveFee(Core.Constants.USCustoms.FeeCodes.HMF);
						}
					}
				}
			}
		}

		public override ZString US_PreparerDistrictPort
		{
			get { return base.US_PreparerDistrictPort; }
			set
			{
				bool hasChanged = value != US_PreparerDistrictPort;
				base.US_PreparerDistrictPort = value;
				if (hasChanged)
				{
					Bills.MarkAsNeedingValidation();
				}
			}
		}

		public override Guid RegistryBranchPK
		{
			get
			{
				if (registryBranchPKCached == null)
				{
					registryBranchPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryBranchPK();
					});
				}
				return registryBranchPKCached.Value;
			}
		}
		CachedProperty<Guid> registryBranchPKCached;

		Guid GetRegistryBranchPK()
		{
			return base.RegistryBranchPK;
		}

		public override Guid RegistryCompanyPK
		{
			get
			{
				if (registryCompanyPKCached == null)
				{
					registryCompanyPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryCompanyPK();
					});
				}
				return registryCompanyPKCached.Value;
			}
		}
		CachedProperty<Guid> registryCompanyPKCached;

		Guid GetRegistryCompanyPK()
		{
			return base.RegistryCompanyPK;
		}

		protected override List<ScreeningParty> GetScreeningParties()
		{
			var result = base.GetScreeningParties();
			result.Add(new ScreeningParty(this, Res.GetString("0a877c0e-5a38-4912-b310-eef3a8773bf8", "Intermediate Consignee"), IntermConsignee));

			foreach (JobComInvoiceHeader invoice in Invoices)
			{
				result.Add(new ScreeningParty(this, Res.GetString("61294808-d969-409f-8200-586816588d23", "Invoice Ultimate Supplier"), invoice.USPPIDocAddress));
				result.Add(new ScreeningParty(this, Res.GetString("1a6473f7-6f2f-4e30-9095-8ccbfecbf4ac", "Invoice Ultimate Consignee"), invoice.UltimateConsigneeDocAddress));
				result.Add(new ScreeningParty(this, Res.GetString("a9dfc747-278e-4810-b6f5-09063dd0d227", "Invoice Intermediate Consignee"), invoice.IntermediateConsigneeDocAddress));
				result.Add(new ScreeningParty(this, Res.GetString("fce1b68a-f8fd-4b9d-b204-aacfb98c740e", "Invoice Pickup Address"), invoice.SupplierPickupAddress));
			}

			result.Add(new ScreeningParty(this, Res.GetString("5a609109-850c-4977-a7ea-360f012aaf15", "Consignee Address"), ConsigneeAddress?.Header));
			result.Add(new ScreeningParty(this, Res.GetString("4be7d60a-2c28-4fed-a33f-8b841616b61c", "Exporter"), Exporter));
			result.Add(new ScreeningParty(this, Res.GetString("62266af3-9aa2-4796-a2b6-3bdfb31e3367", "Seller"), SellerAddress?.Header));
			result.Add(new ScreeningParty(this, Res.GetString("db8bc843-2af0-49e7-9f93-5fe15007bd8c", "Shipper"), ShipperAddress?.Header));
			result.Add(new ScreeningParty(this, Res.GetString("216f2993-47f7-4f09-bc63-623ebbcab820", "Distributor"), DistributorAddress?.Header));
			result.Add(new ScreeningParty(this, Res.GetString("e375eae4-d1ad-4c5a-8a17-859a60f9b2aa", "Buyer"), Buyer));
			result.Add(new ScreeningParty(this, Res.GetString("aa190ee2-ded6-4976-b35e-b5fef19612bc", "Packager"), PackagerAddress?.Header));
			result.Add(new ScreeningParty(this, Res.GetString("29ed7473-bca8-4f94-9daf-6b3282a23c1a", "External Broker"), ExternalBroker));
			result.Add(new ScreeningParty(this, Res.GetString("1cfcceec-41a7-45d5-8176-3fa207ffec71", "Controlling Agent"), ControllingAgent));
			result.Add(new ScreeningParty(this, Res.GetString("36343152-46ba-4a55-9ae7-392d174dff45", "Controlling Customer"), ControllingCustomer));
			result.Add(new ScreeningParty(this, Res.GetString("8c2753bb-4421-47cd-acb0-f385ecbc6be5", "Sold To Party"), SoldToPartyAddress?.Header));
			result.Add(new ScreeningParty(this, Res.GetString("b5bc4a23-86b2-47f7-97b5-e23cbdfbfd6c", "Ship To Party"), ShipToPartyAddress?.Header));
			result.Add(new ScreeningParty(this, Res.GetString("631e1f61-d534-4337-a5ce-34d67cde7276", "Manufacturer"), ManufacturerAddress?.Header));
			result.Add(new ScreeningParty(this, Res.GetString("b1525a09-a502-4a21-9a89-24076874e311", "4811 Party"), NotifyParty));
			return result;
		}

		protected override void GetScreeningStatus(List<ZString> statuses)
		{
			base.GetScreeningStatus(statuses);
			AddScreeningStatusToList(statuses, IntermConsignee);
			foreach (JobComInvoiceHeader invoice in Invoices)
			{
				using (invoice.SuspendMarkingAsNeedingValidation())
				{
					statuses.Add(ScreeningStatusUpdater.GetWorstScreeningStatus(invoice.USPPIDocAddress.Cast<IScreeningPartyProvider>()));
					statuses.Add(ScreeningStatusUpdater.GetWorstScreeningStatus(invoice.UltimateConsigneeDocAddress.Cast<IScreeningPartyProvider>()));
					statuses.Add(ScreeningStatusUpdater.GetWorstScreeningStatus(invoice.IntermediateConsigneeDocAddress.Cast<IScreeningPartyProvider>()));
					statuses.Add(ScreeningStatusUpdater.GetWorstScreeningStatus(invoice.SupplierPickupAddress.Cast<IScreeningPartyProvider>()));
				}
			}

			AddScreeningStatusToList(statuses, ConsigneeAddress?.Header);
			AddScreeningStatusToList(statuses, Exporter);
			AddScreeningStatusToList(statuses, SellerAddress?.Header);
			AddScreeningStatusToList(statuses, ShipperAddress?.Header);
			AddScreeningStatusToList(statuses, DistributorAddress?.Header);
			AddScreeningStatusToList(statuses, Buyer);
			AddScreeningStatusToList(statuses, PackagerAddress?.Header);
			AddScreeningStatusToList(statuses, ExternalBroker);
			AddScreeningStatusToList(statuses, ControllingAgent);
			AddScreeningStatusToList(statuses, ControllingCustomer);
			AddScreeningStatusToList(statuses, SoldToPartyAddress?.Header);
			AddScreeningStatusToList(statuses, ShipToPartyAddress?.Header);
			AddScreeningStatusToList(statuses, ManufacturerAddress?.Header);
			AddScreeningStatusToList(statuses, NotifyParty);
		}

		protected override ZString ClearanceEventReference
		{
			get
			{
				var result = base.ClearanceEventReference;

				if (ActiveEntryHeaders.ReconciliationEntry != null)
				{
					result = "Reconciliation";
				}
				else if (IsFTZAdmission)
				{
					result = "FTZ";
				}

				return result;
			}
		}

		public override ZBool US_IsFinalWHS
		{
			get { return base.US_IsFinalWHS; }
			set
			{
				base.US_IsFinalWHS = value;
				if (US_IsFinalWHS && US_QtyInWHBeforeWithdrawal > 0)
				{
					US_QtyBeingWithdrawn = US_QtyInWHBeforeWithdrawal;
					US_QtyInWHAfterWithdrawal = 0;
				}
			}
		}

		public override ZDecimal US_QtyBeingWithdrawn
		{
			get { return base.US_QtyBeingWithdrawn; }
			set
			{
				base.US_QtyBeingWithdrawn = value;
				US_QtyInWHAfterWithdrawal = US_QtyInWHBeforeWithdrawal - US_QtyBeingWithdrawn;
			}
		}

		protected override bool HasUpdatedDatesLog
		{
			get
			{
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ETAUpdateReplacedByARVEventCode);
				query.AddToFilter(StmALogSchema.SL_Reference, Constants.UpdatedDatesLog);
				return Logs.Find(query).Length > 0;
			}
		}

		[DecimalPlaces(0)]
		[ReadOnlyMember(nameof(US_BondAmount2_ReadOnly))]
		public override ZDecimal US_BondAmount2
		{
			get { return base.US_BondAmount2; }
			set { base.US_BondAmount2 = System.Math.Ceiling(value); }
		}

		bool US_BondAmount2_ReadOnly
		{
			get { return Declaration != null && Declaration.IsLowValue; }
		}

		[DecimalPlaces(0)]
		[ReadOnlyMember(nameof(US_BondAmount_ReadOnly))]
		public override ZDecimal US_BondAmount
		{
			get { return base.US_BondAmount; }
			set
			{
				if (!IsAddInfoPropertySettingSuspended)
				{
					base.US_BondAmount = System.Math.Ceiling(value);
				}
			}
		}

		bool US_BondAmount_ReadOnly
		{
			get
			{
				bool result = true;
				if (Declaration != null)
				{
					if ((Declaration.US_EntryType == EntryTypeList.Codes.TemporaryImportationBond ||
						 Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond) &&
						 Declaration.US_BondCalcCode == SEBCalculationList.Codes.MAN)
					{
						result = false;
					}
					else if (Declaration.US_BondType == BondTypeList.Codes.ContinuousBond && !HasAnyContinuousBondForIOR)
					{
						result = false;
					}
					else if (Declaration.IsACEDrawback)
					{
						result = false;
					}
				}

				return result;
			}
		}

		[ReadOnlyMember(nameof(US_BondProducerAccNo_ReadOnly))]
		public override ZString US_BondProducerAccNo
		{
			get => base.US_BondProducerAccNo;
			set
			{
				if (!IsAddInfoPropertySettingSuspended)
				{
					base.US_BondProducerAccNo = value;
				}
			}
		}

		bool US_BondProducerAccNo_ReadOnly
		{
			get { return Declaration != null && Declaration.US_BondType == BondTypeList.Codes.ContinuousBond && HasAnyContinuousBondForIOR; }
		}

		[ReadOnlyMember(nameof(US_BondProducerAccNo2_ReadOnly))]
		public override ZString US_BondProducerAccNo2
		{
			get => base.US_BondProducerAccNo2;
			set => base.US_BondProducerAccNo2 = value;
		}

		bool US_BondProducerAccNo2_ReadOnly
		{
			get { return Declaration != null && Declaration.IsLowValue; }
		}

		[ReadOnlyMember(nameof(US_SuretyCode_ReadOnly))]
		public override ZString US_SuretyCode
		{
			get => base.US_SuretyCode;
			set => base.US_SuretyCode = value;
		}

		bool US_SuretyCode_ReadOnly
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && !declaration.IsRecon && ((declaration.IsWithoutBondType && (declaration.IsLowValue || declaration.IsDrawback)) ||
				(declaration.US_BondType == BondTypeList.Codes.ContinuousBond && HasAnyContinuousBondForIOR));
			}
		}

		bool HasAnyContinuousBondForIOR
		{
			get { return Declaration != null && Declaration.IORWrapper != null && Declaration.IORWrapper.BondDetails.HasContinuousBond; }
		}

		[ReadOnlyMember(nameof(US_BondDispositionCode2_ReadOnly))]
		public override ZString US_BondDispositionCode2
		{
			get => base.US_BondDispositionCode2;
			set => base.US_BondDispositionCode2 = value;
		}

		bool US_BondDispositionCode2_ReadOnly
		{
			get { return Declaration != null && Declaration.IsLowValue; }
		}

		[ReadOnlyMember(nameof(US_CBPBondNo2_ReadOnly))]
		public override ZString US_CBPBondNo2
		{
			get => base.US_CBPBondNo2;
			set => base.US_CBPBondNo2 = value;
		}

		bool US_CBPBondNo2_ReadOnly
		{
			get { return Declaration != null && Declaration.IsLowValue; }
		}

		[ReadOnlyMember(nameof(US_BondWaiverCode_ReadOnly))]
		public override ZString US_BondWaiverCode
		{
			get => base.US_BondWaiverCode;
			set => base.US_BondWaiverCode = value;
		}

		bool US_BondWaiverCode_ReadOnly
		{
			get { return IsACEDrawback && !US_AcceleratedClaimInd || (Declaration != null && Declaration.IsLowValue); }
		}

		public override ZDateTime JE_DateOfArrival
		{
			get { return base.JE_DateOfArrival; }
			set
			{
				var oldValue = JE_DateOfArrival;
				if (!IsCopying && oldValue != value.Date)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.JE_DateOfArrival = value.Date;

				if (!IsCopying && oldValue != JE_DateOfArrival)
				{
					InvoiceLines.MarkAsNeedingValidation();
					UpdateCargoManifestQueryMessagesOnSaving = IsACE || IsFTZAdmission;
				}

				if (IsFTZAdmission && JE_DateOfFirstArrival.IsEmpty)
				{
					JE_DateOfFirstArrival = JE_DateOfArrival;
				}
			}
		}

		public override ZDateTime JE_DateOfFirstArrival
		{
			get { return base.JE_DateOfFirstArrival; }
			set
			{
				base.JE_DateOfFirstArrival = value.Date;
			}
		}

		public override ZDateTime US_PaymentDueDate
		{
			get { return base.US_PaymentDueDate; }
			set
			{
				bool hasChanged = value != US_PaymentDueDate;
				base.US_PaymentDueDate = value;
				if (hasChanged && !IsCopying)
				{
					US_DeferredTaxDueDate = new AddInfoJobDeclarationWorkingDate().GenerateDeferredTaxDueDate(Declaration);
				}
			}
		}

		public override ZString US_EntryDateElectionCode
		{
			get { return base.US_EntryDateElectionCode; }
			set
			{
				var oldValue = US_EntryDateElectionCode;
				base.US_EntryDateElectionCode = value;
				if (!IsCopying && oldValue != US_EntryDateElectionCode)
				{
					US_PresentationDate = ZDateTime.Empty;
					DefaultPresentationDateIfNeeded();
				}
			}
		}

		public bool US_NonAMS_ReadOnly
		{
			get { return !IsNonAMSRelevant || IsNonAMSJob || IsNonTransportDeclarationType || JE_MasterBillExpressTracking; }
		}

		protected override ZString WorkflowImportOrExport
		{
			get
			{
				var result = base.WorkflowImportOrExport;

				if (IsFTZAdmission)
				{
					result = JobMessageTypeList.Codes.FTZ;
				}
				else if (IsImportByExternalBroker)
				{
					result = JobMessageTypeList.Codes.ImportByExternalBroker;
				}

				return result;
			}
		}

		[MaxLength(4)]
		public override ZString US_FDAAPC
		{
			get { return base.US_FDAAPC; }
			set { base.US_FDAAPC = value; }
		}

		[MaxLength(1)]
		public ZString US_PGAReplaceUpdateNeeded
		{
			get { return this.GetSystemDefinedValue<ZString>(Constants.GenAddOnColumnFieldName.US_PGAReplaceUpdateNeeded); }
			set
			{
				var oldValue = US_PGAReplaceUpdateNeeded;
				if (oldValue != value)
				{
					CheckMaximumLength(US_PGAReplaceUpdateNeededInfo, value);
					this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.US_PGAReplaceUpdateNeeded, value);
					US_PGAReplaceUpdateNeededInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo US_PGAReplaceUpdateNeededInfo
		{
			get { return GetZPropertyInfo(Constants.GenAddOnColumnFieldName.US_PGAReplaceUpdateNeeded); }
		}

		[MaxLength(GenAddOnColumnMaxLength.US_PGACorrectionStatus)]
		public ZString US_PGACorrectionStatus
		{
			get { return this.GetSystemDefinedValue<ZString>(Constants.GenAddOnColumnFieldName.US_PGACorrectionStatus); }
			set
			{
				var oldValue = US_PGACorrectionStatus;
				if (oldValue != value)
				{
					CheckMaximumLength(US_PGACorrectionStatusInfo, value);
					this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.US_PGACorrectionStatus, value);
					US_PGACorrectionStatusInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo US_PGACorrectionStatusInfo
		{
			get { return GetZPropertyInfo(Constants.GenAddOnColumnFieldName.US_PGACorrectionStatus); }
		}

		public ZString US_PGACorrectionStatusDesc
		{
			get { return Factory.GetCachedValue<PGACorrectionStatusList>().GetDescriptionFromCode(US_PGACorrectionStatus); }
		}

		public ZPropertyInfo US_PGACorrectionStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_PGACorrectionStatusDesc); }
		}

		#region Quota Status

		[MaxLength(GenAddOnColumnMaxLength.US_QuotaStatus)]
		public ZString US_QuotaStatus
		{
			get { return this.GetSystemDefinedValue<ZString>(Constants.GenAddOnColumnFieldName.US_QuotaStatus); }
			set
			{
				var oldValue = US_QuotaStatus;
				if (oldValue != value)
				{
					CheckMaximumLength(US_QuotaStatusInfo, value);
					this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.US_QuotaStatus, value);
					US_QuotaStatusInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo US_QuotaStatusInfo
		{
			get { return GetZPropertyInfo(Constants.GenAddOnColumnFieldName.US_QuotaStatus); }
		}

		public ZString US_QuotaStatusDesc
		{
			get { return CargoReleaseProcessingResultList.GetListForQuotaStatus(Factory).GetDescriptionFromCode(US_QuotaStatus); }
		}

		public ZPropertyInfo US_QuotaStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_QuotaStatusDesc); }
		}

		#endregion

		public override ZBool US_ImmediateDelivery
		{
			get { return base.US_ImmediateDelivery; }
			set
			{
				var hasChanges = value != US_ImmediateDelivery;
				base.US_ImmediateDelivery = value;
				if (hasChanges && !IsCopying)
				{
					if (US_ImmediateDelivery && IsACECargoCertificationMode)
					{
						US_EnableCRL = true;
					}
				}
			}
		}

		protected override ZBool SupportValidateCustomsMessagingCore
		{
			get { return true; }
		}

		protected override BusinessObject GetEntityToValidateCore(string triggerAction)
		{
			if (IsReconMessageType)
			{
				return ReconDeclaration ?? new ReconDeclaration(this);
			}

			return base.GetEntityToValidateCore(triggerAction);
		}

		public override ZString JE_GS_NKCusAgent
		{
			get { return base.JE_GS_NKCusAgent; }
			set
			{
				var oldValue = JE_GS_NKCusAgent;
				base.JE_GS_NKCusAgent = value;
				if (!IsCopying && oldValue != JE_GS_NKCusAgent && JE_MessageType != JobMessageTypeList.Codes.Recon)
				{
					DefaultFDAContact();
				}
			}
		}

		#endregion

		#region AESTIR Message That Needs To Be Withdrawn

		public bool HasAESTIRMessageThatNeedsToBeWithdrawn
		{
			get
			{
				return CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => x.HasAESTIRMessageThatNeedsWithdrawn) != null;
			}
		}

		public IDisposable ClearAndSuspendAESTIRMessageThatNeedsToBeWithdrawnFlagging()
		{
			var result = SuspendAESTIRMessageThatNeedsToBeWithdrawnFlagging();
			foreach (CusEntryHeader entry in CustomsEntryHeaders)
			{
				entry.ClearAESTIRMessageThatNeedToBeWithdrawnFlag();
			}
			return result;
		}

		public IDisposable SuspendAESTIRMessageThatNeedsToBeWithdrawnFlagging()
		{
			return new AESTIRMessageThatNeedsToBeWithdrawnFlaggingSuspender(this);
		}

		public bool IsAESTIRMessageThatNeedsToBeWithdrawnFlaggingSuspended
		{
			get { return aESTIRMessageThatNeedsToBeWithdrawnFlaggingSuspenderIndex > 0; }
		}

		byte aESTIRMessageThatNeedsToBeWithdrawnFlaggingSuspenderIndex;
		class AESTIRMessageThatNeedsToBeWithdrawnFlaggingSuspender : IDisposable
		{
			public AESTIRMessageThatNeedsToBeWithdrawnFlaggingSuspender(JobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.aESTIRMessageThatNeedsToBeWithdrawnFlaggingSuspenderIndex++;
			}

			readonly JobDeclaration declaration;

			#region IDisposable Members

			public void Dispose()
			{
				declaration.aESTIRMessageThatNeedsToBeWithdrawnFlaggingSuspenderIndex--;
			}

			#endregion
		}

		#endregion

		#region Accounting Integration

		protected override void OnIntegratedWithAccountingSuccessfully()
		{
			base.OnIntegratedWithAccountingSuccessfully();
			US_JobReadyForPost = false;
		}

		protected override bool IsJobReadyForPost
		{
			get { return US_JobReadyForPost; }
		}

		protected override bool IsIntegrationWithAccountingSupported
		{
			get { return IsENSFormalImport && !US_PSC; }
		}

		#endregion

		#region Overriden Methods

		public bool IsUniversalCopying { get; private set; }

		protected void StartUniversalCopy()
		{
			IsUniversalCopying = true;
		}

		protected override void AfterUniversalCopy()
		{
			base.AfterUniversalCopy();

			var iorPK = DeclarantAddress?.OA_OH ?? ZGuid.Empty;
			if (iorPK != IOROrgPK)
			{
				IOROrgPK = iorPK;
			}

			if (!IsReconMessageType)
			{
				US_IssueCode = ZString.Empty;
			}

			if (Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>() is BusinessObjectUniversalCopyFactoryService factoryCopyService)
			{
				factoryCopyService.AddOnCopyFinishedAction(() =>
				{
					IsUniversalCopying = false;
				});
			}
		}

		protected override void DefaultFinalDestinationPortFromImporter()
		{
			if (!IsRecon)
			{
				base.DefaultFinalDestinationPortFromImporter();
			}
		}

		protected override bool DoMergeCore(ISendsMessagesToCustoms notifier)
		{
			RefreshExRateToLatestRateAvailableIfNeeded();
			return base.DoMergeCore(notifier);
		}

		protected override ZString OtherReferenceNumber
		{
			get
			{
				return JE_PrimaryITNumber;
			}
		}

		protected override ZString OtherReferenceNumberCaption
		{
			get
			{
				return "ITN";
			}
		}

		protected override void ThrowAwayMergeCore()
		{
			try
			{
				SuspendDeleteEntryLogging = true;
				base.ThrowAwayMergeCore();
			}
			finally
			{
				SuspendDeleteEntryLogging = false;
			}
		}
		internal bool SuspendDeleteEntryLogging;

		protected override bool ShouldLogCustomsCommencedDatail => IsImport && !IsExport && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates;

		protected override void PopulateBrokerWhenLogCustomsCommenced(StmALog mostRecentCommencedLog)
		{
			if (CurrentUserIsABroker)
			{
				JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			}
		}

		protected override bool CurrentUserIsABroker => !GlbStaff.CurrentUser.GS_IsSystemAccount && !USCustomsDataRegistry.Instance.EntryDeclarant.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);

		protected override BaseJobDeclarationInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new JobDeclarationInvoicingSupporter(this);
		}

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			ZValidation result;
			switch (JE_MessageType)
			{
				case JobMessageTypeList.Codes.Import:
					result = new ImportJobDocAddressValidation(addressToValidate, this);
					break;
				default:
					{
						if (IsRecon)
						{
							result = new ReconJobDocAddressValidation(addressToValidate, this);
						}
						else if (IsFTZAdmission)
						{
							result = new FTZJobDocAddressValidation(addressToValidate);
						}
						else
						{
							result = base.PiggyBackedDocAddressValidation(addressToValidate);
						}
					}
					break;
			}
			return result;
		}

		protected override void WarehouseDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			base.WarehouseDocAddress_DocAddressChanged(sender, e);

			if (WarehouseDocAddress != null && WarehouseDocAddress.Organisation != null)
			{
				var warehouseOrgWrapper = OrgHeaderWrapper.New(WarehouseDocAddress.Organisation);
				if (US_US_NKLocationOfGoods.IsEmpty)
				{
					US_US_NKLocationOfGoods = warehouseOrgWrapper.ZO_FIRMS;
				}

				ZString ftzNum = warehouseOrgWrapper.ZO_ZoneID + warehouseOrgWrapper.ZO_SubZone + warehouseOrgWrapper.ZO_Site;
				if (FTZZoneID.IsEmpty && !ftzNum.IsEmpty)
				{
					FTZZoneID = ftzNum;
				}
			}
			if (string.IsNullOrEmpty(US_F_RoutingDetails) || US_F_RoutingDetails == "??")
			{
				DefaultRoutingDetails();
			}
		}

		protected override void ContainerTerminalOperatorDocAddress_ValueChanged(object sender, EventArgs e)
		{
			base.ContainerTerminalOperatorDocAddress_ValueChanged(sender, e);

			SynchroniseFIRMS(ContainerTerminalOperatorDocAddress);
		}

		protected override void DepotDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			base.DepotDocAddress_DocAddressChanged(sender, e);

			SynchroniseFIRMS(DepotDocAddress);
		}

		protected override void ContainerYardDocAddress_ValueChanged(object sender, EventArgs e)
		{
			base.ContainerYardDocAddress_ValueChanged(sender, e);

			SynchroniseFIRMS(ContainerYardDocAddress);
		}

		void SynchroniseFIRMS(JobDocAddress jobDocAddress)
		{
			if (US_US_NKLocationOfGoods.IsEmpty && IsImport && !jobDocAddress.IsEmpty)
			{
				US_US_NKLocationOfGoods = jobDocAddress.Organisation?.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates).SubstringSafe(0, AutoUSAddInfo.Schema.US_US_NKLocationOfGoodsMaxLength) ?? ZString.Empty;
			}
		}

		protected override ZString GetTransportModeGeneric()
		{
			switch (JE_TransportMode)
			{
				case TransportTypeList.Codes.Air:
					return TransportTypeGenericList.Codes.Air;

				case TransportTypeList.Codes.Sea:
				case TransportTypeList.Codes.BorderWaterBorne:
					return TransportTypeGenericList.Codes.Sea;

				case TransportTypeList.Codes.Road:
				case TransportTypeList.Codes.Auto:
				case TransportTypeList.Codes.Truck:
				case TransportTypeList.Codes.Pedestrian:
					return TransportTypeGenericList.Codes.Road;

				case TransportTypeList.Codes.Rail:
					return TransportTypeGenericList.Codes.Rail;

				case TransportTypeList.Codes.Mail:
					return TransportTypeGenericList.Codes.PostMail;

				case TransportTypeList.Codes.FixedTransportInstallations:
				case TransportTypeList.Codes.PassengerHandCarried:
					return TransportTypeGenericList.Codes.Other;
			}
			return ZString.Empty;
		}

		protected override void OnApportioned()
		{
			base.OnApportioned();
			if (IsEntrySummaryValidationMode)
			{
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					invoiceLine.Validation.ValidateJI_Calc_FreightInInvoiceCurr();
				}
			}

			if (IsImport && !IsOGAValueGoingToBeCalculated)
			{
				new OGAInvValueConverter().Convert(this);

				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					invoiceLine.RefreshOGAValueValidation();
				}
			}
		}

		internal bool IsOGAValueGoingToBeCalculated
		{
			get { return IsImport && (US_EnableENS || US_EnableCRL || IsFTZAdmission) && CustomsEntryHeaders.Count > 0; }
		}

		public override void Delete()
		{
			LoadChildEditableObjects();
			DispositionCodes.RemoveAndDeleteAll();
			OGADispositionCodes.RemoveAndDeleteAll();
			DeliveryOrderHeaders.RemoveAndDeleteAll();
			if (fPPQForm368Data != null)
			{
				fPPQForm368Data.Delete();
			}
			UnlockImportEntryNumberAllocationMutex();
			UnlockFTZAdmissionNumberAllocationMutex();
			base.Delete();
		}

		public void UnlockImportEntryNumberAllocationMutex()
		{
			if (importEntryNumberAllocationMutex != null && importEntryNumberAllocationMutex.IsLocked && importEntryNumberAllocationMutex.HasLock)
			{
				importEntryNumberAllocationMutex.Unlock();
			}
		}

		internal void AddCarrierFetchHintsIfNeeded()
		{
			if (!hasCarrierFetchHintsBeingAdded)
			{
				hasCarrierFetchHintsBeingAdded = true;
				var carrierCodes = new List<ZString>();
				AddToListIfNotEmpty(carrierCodes, US_UI_NKCarrierSCAC);
				foreach (Bill bill in Bills)
				{
					AddToListIfNotEmpty(carrierCodes, bill.US_UI_NKBillIssuerSCAC);
				}
				if (carrierCodes.Count > 0)
				{
					carrierCodes.ForEach((x) =>
					{
						Factory.AddFetchHint(USCarrierCombinedSchema.UI_Code, x);
					});
				}
			}
		}
		bool hasCarrierFetchHintsBeingAdded;

		public void AddFDAFetchHintsIfNeeded()
		{
			if (!hasInvoiceLineCusAddInfoFetchHintsBeingAdded && !hasFDAFetchHintsBeingAdded)
			{
				hasFDAFetchHintsBeingAdded = true;
				AddJobComInvoiceLineFetchHintsIfNeeded();
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					var cusAddInfoQuery = new ZQuery(CusAddInfoSchema.B7_ParentID, line.PK);
					Factory.AddFetchHint(typeof(FDA), new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USFDA), cusAddInfoQuery);
				}
			}
		}
		bool hasFDAFetchHintsBeingAdded;

		internal void AddInvoiceLineCusAddInfoFetchHintsIfNeeded()
		{
			if (!hasInvoiceLineCusAddInfoFetchHintsBeingAdded)
			{
				hasInvoiceLineCusAddInfoFetchHintsBeingAdded = true;
				AddJobComInvoiceLineFetchHintsIfNeeded();
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, line.PK);
				}
			}
		}
		bool hasInvoiceLineCusAddInfoFetchHintsBeingAdded;

		internal void AddTariffFetchHintsIfNeeded()
		{
			if (!hasTariffFetchHintsBeingAdded)
			{
				hasTariffFetchHintsBeingAdded = true;
				var tariffs = new List<ZString>();
				var isRecon = IsRecon;
				AddJobComInvoiceLineFetchHintsIfNeeded();
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					AddToListIfNotEmpty(tariffs, invoiceLine.JI_Tariff);
					AddToListIfNotEmpty(tariffs, invoiceLine.US_SupTariff);
					if (isRecon)
					{
						AddToListIfNotEmpty(tariffs, invoiceLine.US_R_OrigTariff);
						AddToListIfNotEmpty(tariffs, invoiceLine.US_R_OrigSupTariff);
					}
				}
				tariffs.ForEach((x) =>
				{
					Factory.AddFetchHint(typeof(USCTariff), USCTariffSchema.UE_Tariff, x);
				});
			}
		}
		bool hasTariffFetchHintsBeingAdded;

		void AddToListIfNotEmpty(List<ZString> list, ZString code)
		{
			if (!code.IsEmpty && !list.Contains(code))
			{
				list.Add(code);
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (GlbCompany.CurrentCompany.PK == CompanyPK)
			{
				if (needToRequestTariffs)
				{
					needToRequestTariffs = false;
					if (IsRecon)
					{
						DeleteOnSaveFail(ReferenceFileRequester.RequestTariffs(ReconDeclaration, false));
					}
					else
					{
						DeleteOnSaveFail(ReferenceFileRequester.RequestTariffs(this, false));
					}
				}

				if (needToRequestCarrierCodesForBill)
				{
					needToRequestCarrierCodesForBill = false;
					ReferenceFileRequester.RequestCarrierCodesForBill(this);
				}

				if (allocateEntryNumberOnSaving)
				{
					AllocateNextImportEntryNumber();
				}

				if (AllocateFTZControlNumberOnSaving)
				{
					AllocateNextFTZControlNumberyNumber();
				}

				RecalculateReconIndicators();
			}
#if DEBUG
			if (OnFactorySavingHandlerForTest != null)
			{
				OnFactorySavingHandlerForTest();
			}
#endif
		}

		void DeleteOnSaveFail(IEnumerable<EDIMessage> messages)
		{
			foreach (EDIMessage message in messages)
			{
				message.Saved += DeleteOnFail;
			}
		}

		void DeleteOnFail(Enterprise.Messaging.Business.EDIMessage message, bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				message.Delete();
			}
			message.Saved -= DeleteOnFail;
		}

		internal void SetNeedToRequestTariffs()
		{
			needToRequestTariffs |= (IsImport || IsRecon || UseHTSForExport);
		}
		bool needToRequestTariffs;

		internal void SetNeedToRequestCarrierCodesForBill()
		{
			needToRequestCarrierCodesForBill |= IsImport && !IsACEAutoRoadAndPedTransportMode;
		}
		bool needToRequestCarrierCodesForBill;

#if DEBUG
		public Action OnFactorySavingHandlerForTest;
#endif

		public void RecalculateReconIndicators()
		{
			if (reconIndicatorsDirty)
			{
				ReconIssueCalculator.RecalculateIndicators(this);
				reconIndicatorsDirty = false;
			}
		}

		public bool LockImportEntryNumberAllocationMutex
		{
			get { return ImportEntryNumberAllocationMutex.IsLocked ? (bool)ImportEntryNumberAllocationMutex.HasLock : ImportEntryNumberAllocationMutex.Lock(); }
		}

		public string GetImportEntryNumberAllocationMutexLockInfo() => ImportEntryNumberAllocationMutex.GetMutexLockByInfo();

		void AllocateNextImportEntryNumber()
		{
			ZString entryNumber;

			if (EntryNumberGenerator.TryGetNextEntryNumber(Branch, US_EntryFilerCode, out entryNumber))
			{
				ImportEntryNumber = entryNumber;
				ENSEntryNumber.CE_EntryIsSystemGenerated = true;
			}
		}

		ZGlobalMutex ImportEntryNumberAllocationMutex
		{
			get { return importEntryNumberAllocationMutex ?? (importEntryNumberAllocationMutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "ENS" + PK.ToString())); }
		}
		ZGlobalMutex importEntryNumberAllocationMutex;

		public override void OnSaving()
		{
			base.OnSaving();
			if (IsDrawback)
			{
				FillInDrawbackDeclarationNumberIfRequired();
			}
			else if (IsExport && !ImportEntryNumber.IsEmpty)
			{
				ImportEntryNumber = ZString.Empty;
			}

			if (AssignAESShipmentNumberOnSaving && !string.IsNullOrEmpty(JobNumber))
			{
				LineMerger.UpdateShipmentNoAndStatus(this);
			}

			if (!IsInDatabase && (IsACE || IsFTZAdmission))
			{
				CargoManifestStatusQuerySender.MarkForAutoSendingIfEligible(Declaration, false);
			}

			if (UpdateCargoManifestQueryMessagesOnSaving)
			{
				CargoManifestStatusQuerySender.UpdateHeldUntilDateIfEligible(Declaration, true);
			}

			if (IsImport)
			{
				if (JE_ApplicationCodeInfo.HasChanges)
				{
					var entrySummary = ActiveEntryHeaders.EntrySummaryEntry;
					if (entrySummary != null && (entrySummary.HasBeenLodgedAtCustoms || entrySummary.IsWaitingForResponse))
					{
						ErrorReporter.ReportOnce("ApplicationCode has changed when 7501 has transactions with customs");
					}
				}

				if (JE_EntryAuthorisationDate.IsValid && JE_EntryAuthorisationDateInfo.HasChanges)
				{
					var formalEntry = FormalEntry;
					if (formalEntry != null && formalEntry.HasBeenLodgedAtCustoms && !formalEntry.HasBeenWithdrawn)
					{
						TrySendStatementUpdate();
					}
					else
					{
						new DeclarationPrelimStatementDetailsDefaulter().Default(this);
					}
				}
			}

			if (IsFTZAdmission && FTZYear.IsEmpty)
			{
				FTZYear = ZDate.Today.ToString("yy", System.Globalization.CultureInfo.InvariantCulture);
			}

			GetAddInfo().ReportUnexpectedProperties();
		}
		internal bool AssignAESShipmentNumberOnSaving;
		internal bool UpdateCargoManifestQueryMessagesOnSaving;

		internal void PopulatePaymentDueDateIfNeeded()
		{
			if (US_PaymentDueDate.IsEmpty)
			{
				CalculatePaymentDueDate();
			}
		}

		protected override void PopulateDataModelIfNeededCore()
		{
			if (!IsInDatabase && IsReconMessageType)
			{
				JE_DataModel = Constants.ReconMessageDataModel;
			}
			else
			{
				base.PopulateDataModelIfNeededCore();
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				AssignAESShipmentNumberOnSaving = false;
				UpdateCargoManifestQueryMessagesOnSaving = false;
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			allocateEntryNumberOnSaving = false;
			if (saveSucceeded)
			{
				UnlockImportEntryNumberAllocationMutex();
				UnlockFTZAdmissionNumberAllocationMutex();
			}
			else if (AllocateFTZControlNumberOnSaving)
			{
				FTZControlNumber = ZString.Empty;
			}
			AllocateFTZControlNumberOnSaving = false;
		}

		ReferenceFileRequester ReferenceFileRequester
		{
			get { return referenceFileRequester ?? (referenceFileRequester = new ReferenceFileRequester(Factory)); }
		}
		ReferenceFileRequester referenceFileRequester;

		protected override void DeriveExportDeclarationStatus()
		{
			var entryStatus = ZString.Empty;
			var messageStatus = ZString.Empty;

			if (CustomsEntryHeaders.Count > 0)
			{
				entryStatus = CustomsEntryHeaders[0].CH_EntryStatus;
				messageStatus = CustomsEntryHeaders[0].CH_Status;
				if (CustomsEntryHeaders.Cast<CusEntryHeader>().Any(header => header.CH_EntryStatus != entryStatus || header.CH_Status != messageStatus))
				{
					entryStatus = AESDirectCustomsEntryStatus.Codes.MultipleEntriesStatus;
					messageStatus = AESDirectCustomsEntryStatus.Codes.MultipleEntriesStatus;
				}
			}

			JE_EntryStatus = entryStatus;
			JE_MessageStatus = messageStatus;
		}

		protected override void DeriveImportDeclarationStatus()
		{
			if (!IsFTZAdmission)
			{
				JE_MessageStatus = SummaryEntryStatusCalculator.SummaryMessageStatus;
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.EntryStatusList))]
		public override ZString JE_EntryStatus
		{
			get { return base.JE_EntryStatus; }
			set { base.JE_EntryStatus = value; }
		}

		public bool JE_EntryStatus_ReadOnly
		{
			get { return true; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MessageStatusList))]
		public override ZString JE_MessageStatus
		{
			get { return base.JE_MessageStatus; }
			set
			{
				var oldValue = base.JE_MessageStatus;
				base.JE_MessageStatus = value;
				if (!IsCopying && oldValue != JE_MessageStatus)
				{
					if (IsProtest || IsACEDrawback)
					{
						AddMessageLog(oldValue);
					}
				}
			}
		}

		void AddMessageLog(ZString oldStatus)
		{
			var currentStatus = Declaration.JE_MessageStatus;
			if (StatusList.IsStatusClear(currentStatus))
			{
				LogManager.AddAClearLogIfNecessary(oldStatus, currentStatus, StatusList);
			}
		}

		internal StatusLogManager LogManager
		{
			get { return fLogManager ?? (fLogManager = new StatusLogManager(Logs, Branch)); }
		}
		StatusLogManager fLogManager;

		internal bool HasBeenLodgedAtCustoms
		{
			get { return HasBeenLodgedAtCustomsIsRelevantForDeclaration && LogManager.HasAClearLog(GetMessagesTypesRightFor(JE_MessageType), StatusList); }
		}

		bool HasBeenLodgedAtCustomsIsRelevantForDeclaration
		{
			get { return IsProtest || IsACEDrawback; }
		}

		internal bool HasBeenWithdrawn
		{
			get { return LogManager.HasAWithdrawnLog; }
		}

		ImportMessageStatusList.MessageType GetMessagesTypesRightFor(ZString messageType)
		{
			switch (JE_MessageType)
			{
				case JobMessageTypeList.MoreCodes.Protest:
					return ImportMessageStatusList.MessageType.Protest;
				default:
					return ImportMessageStatusList.MessageType.Undefined;
			}
		}

		internal IStatusList StatusList
		{
			get { return (IStatusList)Lookups.MessageStatusList; }
		}

		public bool JE_MessageStatus_ReadOnly
		{
			get { return true; }
		}

		protected override IEnumerable<ZString> GetMessageTypesForDocumentFilter()
		{
			foreach (var messageType in base.GetMessageTypesForDocumentFilter())
			{
				yield return messageType;
			}

			if (IsFTZAdmission)
			{
				yield return JobMessageTypeList.Codes.FTZ;
			}

			if (IsInBond)
			{
				yield return CusEntryHeaderMessageTypeList.Codes.InBond;
			}
		}

		protected override ZString GetMessageTypeForDocumentFilter()
		{
			var result = base.GetMessageTypeForDocumentFilter();

			if (IsFTZAdmission)
			{
				result = JobMessageTypeList.Codes.FTZ;
			}
			return result;
		}

		protected override void LogEventIfJE_EntryStatusChangedCore()
		{
			if (!IsProtest)
			{
				base.LogEventIfJE_EntryStatusChangedCore();
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobDeclarationFetchStrategy(this);
		}

		public override ZString WorkflowProviderCoreCode
		{
			get
			{
				if (IsDrawback)
				{
					return WorkflowDescriptors.DrawBackWorkflowDescriptorCode;
				}
				else if (IsReconMessageType)
				{
					return ReconWorkflowHelper.WorkflowType;
				}
				else if (IsProtestMessageType)
				{
					return ProtestWorkflowHelper.WorkflowType;
				}
				return base.WorkflowProviderCoreCode;
			}
		}

		public override ColumnValueRanker GetRankerForTemplate()
		{
			if (IsDrawback)
			{
				var result = new ColumnValueRanker();
				result.Add(ProcessTaskTemplateSchema.P0_GB, JE_GB, ZGuid.Empty);

				var clientList = new List<IZType>();
				if (JE_OH_Importer.IsValid)
				{
					clientList.Add(JE_OH_Importer);
				}
				var job = new JobHeader.Loader(this).Load();
				if (job != null)
				{
					clientList.Add(job.LocalChargesPK);
				}
				clientList.Add(ZGuid.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, clientList.ToArray());

				return result;
			}
			else if (IsReconMessageType)
			{
				return ReconWorkflowHelper.GetTemplateSelectionCriteria(this);
			}
			else if (IsProtestMessageType)
			{
				return ProtestWorkflowHelper.GetTemplateSelectionCriteria(this);
			}
			return base.GetRankerForTemplate();
		}

		public override ProcessTaskCollection WorkflowItems
		{
			get
			{
				ProcessTaskCollection result;
				if (IsProtest)
				{
					result = Protest.WorkflowItems; // Why is this the JobDeclarationCollection?
				}
				else if (IsRecon)
				{
					result = ReconDeclaration.WorkflowItems;
				}
				else
				{
					result = base.WorkflowItems;
				}
				return result;
			}
		}

		protected override ZString GetContainerModeForDeclarationCore(ZString transportMode, ZString shipmentPackingMode)
		{
			var result = ZString.Empty;

			if (transportMode == Core.Constants.TransportModes.Air || transportMode == Core.Constants.TransportModes.AirSea || transportMode == Core.Constants.TransportModes.Rail)
			{
				var containersCount = CusContainers.Count;

				if (containersCount == 0 && Shipment != null && RelevantConsol != null && ShipmentSynchroniser.IsEnabled)
				{
					containersCount = Shipment.ContainersOnConsol(RelevantConsol).Count;
				}

				result = containersCount > 0 ? Core.Constants.ContainerModes.Containerised : Core.Constants.ContainerModes.NonContainerised;
			}
			else
			{
				result = GetContainerModeOfCNTorNCTFromShipmentMode(shipmentPackingMode);
			}

			return result;
		}

		#endregion

		public event EventHandler<LoadChildEditableObjectsProgressEventArgs> OnPGARelatedDataLoadProgress;
		public event EventHandler<ShowMessageOnGUIEventArgs> ShowMessageOnGUI;

		public delegate bool MarkPGAStatusToBeDeletedWarningCheckerDelegate(string warning);
		public MarkPGAStatusToBeDeletedWarningCheckerDelegate MarkPGAStatusToBeDeletedWarningChecker;

		#region Implementation
		protected override void SetupDataOnDocsAndCartageFirstSet()
		{
			var docsAndCartage = DocsAndCartage;
			var zAddress = docsAndCartage.JP_OA_DeliveryCartageCoAddr_ZAddress;
			if (zAddress.OrgPKValidation == null)
			{
				zAddress.OrgPKValidation = JP_OA_DeliveryCartageCoAddr_ZAddress_OrgPKValidation;
			}

			docsAndCartage.JP_OA_DeliveryCartageCoAddrInfo.ValueChanged -= JP_OA_DeliveryCartageCoAddrInfoValueChanged;
			docsAndCartage.JP_OA_DeliveryCartageCoAddrInfo.ValueChanged += JP_OA_DeliveryCartageCoAddrInfoValueChanged;
		}

		void JP_OA_DeliveryCartageCoAddr_ZAddress_OrgPKValidation(ZPropertyInfo info)
		{
			var validation = new FTZDocsAndCartageValidation(DocsAndCartage);
			validation.CheckJP_OA_DeliveryCartageCoAddr(info);
		}

		void JP_OA_DeliveryCartageCoAddrInfoValueChanged(object sender, EventArgs e)
		{
			if (!IsCopying && IsFTZAdmission)
			{
				var newValue = DeliveryOrPickupCartageCoPK;
				ClearBillValuesIfSame(newValue, Bill.Schema.US_F_OH_PTTCarrier);
			}
			DeliveryOrPickupCartageCoPKInfo.RefreshBinding();
		}

		protected override ZString GetDefaultCartageContainerMode => JE_TransportMode == TransportTypeList.Codes.Truck ? (ZString)Enterprise.Core.Constants.ContainerModes.LCL : base.GetDefaultCartageContainerMode;

		protected override JobDocsAndCartageValidation PiggyBackedValidationCore
		{
			get
			{
				var validation = base.PiggyBackedValidationCore;
				if (!IsDeleted && IsFTZAdmission)
				{
					validation = new FTZDocsAndCartageValidation(DocsAndCartage);
				}
				return validation;
			}
		}

		protected override SynchronizeWithOrders GetNewSynchronizeWithOrders()
		{
			return new FTZSynchronizeWithOrders(this);
		}

		protected override bool IsInvoiceQuantityRequiredForBondedWarehouse
		{
			get { return true; }
		}

		protected override bool IsBondedWhsQuantityRequiredForBondedWarehouse
		{
			get { return false; }
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetWarehouseEntries()
		{
			if (IsFTZAdmission)
			{
				var ftzEntry = ActiveEntryHeaders.FTZEntry;
				if (ftzEntry != null)
				{
					yield return ftzEntry;
				}
			}
			else
			{
				var entrySummaryEntry = ActiveEntryHeaders.EntrySummaryEntry;
				if (entrySummaryEntry != null)
				{
					yield return entrySummaryEntry;
				}
			}
		}

		protected override ZString GetWarehouseTransactionStatus()
		{
			return SingleWarehouseEntry?.CH_WarehouseTransactionStatus ?? ZString.Empty;
		}

		protected override bool IsValidWarehouseEntry(Customs.Business.CusEntryHeader entry)
		{
			return base.IsValidWarehouseEntry(entry) && entry.CH_MessageType == (IsFTZAdmission ? CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone : CusEntryHeaderMessageTypeList.Codes.EntrySummary);
		}

		protected override DeclarationInventorySelectionHeader GetNewInventorySelectionHeader()
		{
			return new InventorySelectionHeader(this);
		}

		protected override bool ShouldUpdateOutwardLinesWithInventoryDetailsCore
		{
			get { return false; }
		}

		internal bool HasPGARelatedDataLoadProgress
		{
			get { return OnPGARelatedDataLoadProgress != null; }
		}

		internal void PGARelatedDataLoadProgress(object sender, LoadChildEditableObjectsProgressEventArgs e)
		{
			OnPGARelatedDataLoadProgress(sender, e);
		}

		internal PGADataChangeTracker PGATrackerHelper
		{
			get { return pgaTrackerHelper ?? (pgaTrackerHelper = new PGADataChangeTracker(this)); }
		}
		PGADataChangeTracker pgaTrackerHelper;

		protected override CodeDescriptionPairList GetAdditionalReferenceNumberTypeListCore(ZString category, ZString countryCode)
		{
			return Factory.GetCachedValue<CusEntryHeaderMessageTypeList>();
		}

		void Bills_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!Bills.IsLoading)
			{
				var addFetchHintsForPivot = false;
				if (!addedFDAFetchHintsForBillsCountChanged)
				{
					addedFDAFetchHintsForBillsCountChanged = true;
					AddFDAFetchHintsIfNeeded();
					addFetchHintsForPivot = true;
				}
				var fdas = new List<FDA>();
				foreach (var fda in InvoiceLines.OfType<JobComInvoiceLine>().SelectMany(x => x.FDAs.OfType<FDA>().Where(y => !y.IsDeleted)))
				{
					if (addFetchHintsForPivot)
					{
						fdas.Add(fda);
						var query = new ZQuery(GenPivotSchema.XX_Relation1ID, fda.PK);
						query.AddToFilter(GenPivotSchema.XX_RelationType, FDARelatedBillsGenPivot.RelationType);
						Factory.AddFetchHint(GenPivotSchema.Instance, query);
					}
					else
					{
						fda.SetDefaultFDARelatedBill();
					}
				}
				foreach (var fda in fdas)
				{
					fda.SetDefaultFDARelatedBill();
				}
			}
		}
		bool addedFDAFetchHintsForBillsCountChanged;

		protected override bool EntriesExistAndAllHaveEntryNumbersCore
		{
			get { return !(IsFTZAdmission ? FTZAdmissionNumber : IsImport ? ImportEntryNumber : ZString.Empty).IsEmpty; }
		}

		protected override bool GetHasLinesForInwardBondedWarehousing()
		{
			bool result;
			if (IsImportByExternalBroker)
			{
				result = GetHasLinesForInwardBondedWarehousingForImportByExternalBroker();
			}
			else if (IsFTZAdmission)
			{
				result = GetHasLinesForInwardBondedWarehousingForFTZAdmission();
			}
			else
			{
				result = base.GetHasLinesForInwardBondedWarehousing();
			}
			return result;
		}

		bool GetHasLinesForInwardBondedWarehousingForFTZAdmission()
		{
			return HasLineGoingIntoAnAutomatedBondedWarehouse;
		}

		bool GetHasLinesForInwardBondedWarehousingForImportByExternalBroker()
		{
			return HasLineGoingIntoABondedWarehouseAndWHSEntryLineNo;
		}

		ZBool HasLineGoingIntoABondedWarehouseAndWHSEntryLineNo
		{
			get
			{
				if (hasLineGoingIntoABondedWarehouseAndWHSEntryLineNo == null)
				{
					hasLineGoingIntoABondedWarehouseAndWHSEntryLineNo = new CachedProperty<ZBool>(Factory, GetHasLineGoingIntoABondedWarehouseAndWHSEntryLineNo);
				}
				return hasLineGoingIntoABondedWarehouseAndWHSEntryLineNo.Value;
			}
		}
		CachedProperty<ZBool> hasLineGoingIntoABondedWarehouseAndWHSEntryLineNo;

		ZBool GetHasLineGoingIntoABondedWarehouseAndWHSEntryLineNo()
		{
			return IsInwardBondedWarehousingEnabled && InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.IsGoingIntoBondedWarehouse && !x.US_WHSEntryLineNo.IsEmpty);
		}

		protected override Customs.Business.BondedWarehousingHelper GetNewBondedWarehousingHelper()
		{
			return new BondedWarehousingHelper(this);
		}

		protected override bool IsImporterDocumentaryAddressRequiredForWarehouseValidationCore
		{
			get { return IsEntrySummaryValidationMode || IsFTZAdmissionValidationMode; }
		}

		protected override bool IsSupplierDocumentaryAddressRequiredForWarehouseValidationCore => false;

		protected override bool IsWarehouseDocAddressRequiredForWarehouseValidation
		{
			get { return IsEntrySummaryValidationMode || IsFTZAdmissionValidationMode; }
		}

		internal static string OnlyWarehouseWithProductWarehouseTypeCanBeUsedForInward(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "The Warehouse address selected is not a Product Warehouse ('PRW') type; a Product Warehouse Type is required for {0} integration when Entry Type is either '21' or '22'.\r\nPlease select another address that indicates a Product Warehouse.", term);
		}

		internal static string OnlyWarehouseWithFTZProductWarehouseTypeCanBeUsedForFTZOutward(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "The Warehouse address selected is not a Free Trade Zone Product Warehouse ('FTZ') type; a Free Trade Zone Product Warehouse Type is required for {0} integration when Entry Type is '06'.\r\nPlease select another address that indicates a Free Trade Zone Product Warehouse.", term);
		}

		internal static string OnlyWarehouseWithProductWarehouseTypeCanBeUsedForOutward(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "The Warehouse address selected is not a Product Warehouse ('PRW') type; a Product Warehouse Type is required for {0} integration when Entry Type is either '31', '32', '34' or '38'.\r\nPlease select another address that indicates a Product Warehouse.", term);
		}

		internal static string OnlyFTZProductWarehouseTypeCanBeUsedForFTZAdmission(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "The Warehouse address selected is not a Free Trade Zone Product Warehouse ('FTZ') type; a Free Trade Zone Product Warehouse type is required for {0} integration.\r\nPlease select another address that indicates a Free Trade Zone Product Warehouse.", term);
		}

		internal static string FTZAdmissionControlNumberComponentIsRequiredForBondedWarehousing(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "Zone, Sub zone, Site and Control Number are required for {0} integration.", term);
		}
		internal static string WarehouseEntryFilerCodeAndNumberAreRequiredForBondedWarehousing(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "Warehouse Entry Filer Code and Entry Number are required for {0} integration.", term);
		}
		internal static string InvoiceLineMarkedForBondedWarehousingLineRequiresWHSPackDetails(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "An Invoice Line marked for {0} must have a Package Quantity specified; not all Invoice Lines marked for {0} have a Package Quantity specified.", term);
		}
		internal static string BondedWarehousingLineRequiresWHSEntryLine(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "An Invoice Line marked for {0} must have a Entry Line Number specified; not all Invoice Lines marked for {0} have an Entry Line Number specified.", term);
		}
		public static string BondedWarehousingLineRequiresWHSEntryNumberAndWHSEntryLine(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "An Invoice Line marked for {0} must have a Warehouse Entry Number and a Warehouse Entry Line Number specified; not all Invoice Lines marked for {0} have a Warehouse Entry Number and a Warehouse Entry Line Number specified.", term);
		}

		protected override void WarehouseDocAddressRequirement_ValidateOrganisationPK_Additional(JobDocAddress parent)
		{
			base.WarehouseDocAddressRequirement_ValidateOrganisationPK_Additional(parent);
			if (IsInwardBondedWarehousingEnabled)
			{
				var whsWarehouse = parent.Address.GetWhsWarehouse();
				if (whsWarehouse != null)
				{
					if (IsFTZAdmission)
					{
						if (whsWarehouse.WW_WarehouseType != WarehouseConstants.WarehouseType.FreeTradeZone)
						{
							parent.OrganisationPKInfo.AddMessageError(OnlyFTZProductWarehouseTypeCanBeUsedForFTZAdmission(TermNameForBondedWarehouse));
						}
					}
					else
					{
						if (whsWarehouse.WW_WarehouseType != WarehouseConstants.WarehouseType.Product)
						{
							parent.OrganisationPKInfo.AddMessageError(OnlyWarehouseWithProductWarehouseTypeCanBeUsedForInward(TermNameForBondedWarehouse));
						}
					}
				}
			}
			else if (IsOutwardBondedWarehousingEnabled)
			{
				var whsWarehouse = parent.Address.GetWhsWarehouse();
				if (whsWarehouse != null)
				{
					if (IsConsumptionFTZ)
					{
						if (whsWarehouse.WW_WarehouseType != WarehouseConstants.WarehouseType.FreeTradeZone)
						{
							parent.OrganisationPKInfo.AddMessageError(OnlyWarehouseWithFTZProductWarehouseTypeCanBeUsedForFTZOutward(TermNameForBondedWarehouse));
						}
					}
					else
					{
						if (whsWarehouse.WW_WarehouseType != WarehouseConstants.WarehouseType.Product)
						{
							parent.OrganisationPKInfo.AddMessageError(OnlyWarehouseWithProductWarehouseTypeCanBeUsedForOutward(TermNameForBondedWarehouse));
						}
					}
				}
			}
		}

		protected override ICodeDescriptionPairList GetBillTypeListCore()
		{
			var prefix = IsFTZAdmission ? "FTZ" : "OTHER";

			return Factory.GetCachedValue("USBillTypeList" + prefix,
			delegate
			{
				var list = new BillTypeList();
				if (prefix == "FTZ")
				{
					list.RemoveCode(BillTypeList.Codes.SubHouseBill);
				}
				return list;
			});
		}

		protected new MessageTypeBasedValueDefaulter MessageTypeBasedValueDefaulter
		{
			get { return (MessageTypeBasedValueDefaulter)base.MessageTypeBasedValueDefaulter; }
		}

		protected override Customs.Business.MessageTypeBasedValueDefaulter GetNewMessageTypeBasedValueDefaulter()
		{
			return new MessageTypeBasedValueDefaulter(this);
		}

		ZString GetCountryFromPort(RefUNLOCO port)
		{
			ZString result = ZString.Empty;
			if (port != null)
			{
				result = port.RL_RN_NKCountryCode;
			}
			return result;
		}

		protected override bool IsUNDGSupportedOnInvoiceLines
		{
			get { return IsImport; }
		}

		void ClearDeliveryOrderValuesIfSame(IZType decValue, string deliveryFieldName)
		{
			if (IsPersistent)
			{
				foreach (DeliveryOrderHeader header in DeliveryOrderHeaders)
				{
					IZType headerValue = (IZType)header[deliveryFieldName];

					if (headerValue.Equals(decValue))
					{
						using (header.SuspendEffectiveValue(deliveryFieldName, decValue))
						{
							header[deliveryFieldName] = headerValue.Default;
						}

						ZPropertyInfo infoToRefresh = header.ZPropertyInfoHash[deliveryFieldName];
						if (infoToRefresh != null)
						{
							infoToRefresh.RefreshBinding();
						}
					}
				}
			}
		}

		void AddOnColumnStatus(GenAddOnColumn addOnStatus, ZString statusName, ZString value, ZPropertyInfo statusFieldInfo)
		{
			if (!IsCopying)
			{
				var oldValue = addOnStatus?.XA_Data ?? ZString.Empty;
				if (value.IsEmpty)
				{
					if (addOnStatus != null && !addOnStatus.IsDeleted)
					{
						addOnStatus.Delete();
					}
				}
				else
				{
					CheckMaximumLength(statusFieldInfo, value);
					if (addOnStatus != null)
					{
						addOnStatus.XA_Data = value;
					}
					else
					{
						var addOn = Factory.New<GenAddOnColumn>();
						addOn.XA_ParentID = Declaration.PK;
						addOn.XA_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
						addOn.XA_Name = statusName;
						addOn.XA_Type = AddOnColumnDataType.Codes.String;
						addOn.XA_Data = value;
					}
				}

				statusFieldInfo.RefreshBinding(oldValue);
			}
		}

		GenAddOnColumn GetAddOnStatus(ZString statusName)
		{
			ZQuery addOnStatusQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, Declaration.PK);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, statusName);
			addOnStatusQuery.FetchOnlyFromLocalCache = !Declaration.IsInDatabase;
			GenAddOnColumn addOn = Factory.LoadTop1<GenAddOnColumn>(addOnStatusQuery);
			return addOn;
		}

		protected override ZString? GetImporterEquipment()
		{
			return IsRecon ? ZString.Empty : base.GetImporterEquipment();
		}

		protected override bool IsNotPersistentActiveProductInternal(BaseJobComInvoiceLine line)
		{
			return
				!line.JI_PartNo.IsEmpty
				&& line.Part == null
				&& line.ParentTariffLine == null
				&& !line.JI_Description.IsEmpty
				&& (!line.JI_CC.IsEmpty || !line.JI_Tariff.IsEmpty);
		}

		protected override NumberGeneratorContext GetNewNumberGeneratorContext()
		{
			return new NumberGeneratorContext(RegistryCompanyPK, RegistryBranchPK, GlbDepartment.CurrentDepartment.PK);
		}

		protected override void SupplierPickupAddressChanged(ZGuid oldAddressPK, ZGuid newAddressPK)
		{
			base.SupplierPickupAddressChanged(oldAddressPK, newAddressPK);
			if (IsExport)
			{
				if (SupplierPickupAddress.OrganisationPK == JE_OH_Supplier)
				{
					ClearInvoiceValuesIfSame(newAddressPK, JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress);
				}

				RefreshInvoicesPicupAddress(newAddressPK);
				DefaultStateOfOriginIfPossible();
				Invoices.MarkAsNeedingValidation();
				Invoices.RefreshBinding();
			}
		}

		void RefreshInvoicesPicupAddress(ZGuid newAddressPK)
		{
			foreach (var invoice in Invoices.Cast<JobComInvoiceHeader>())
			{
				var fPicupAddress = invoice.SupplierPickupAddress;
				if (newAddressPK != ZGuid.Empty && fPicupAddress != null && fPicupAddress.E2_OA_Address != newAddressPK)
				{
					invoice.RefreshPickupAddress();
				}
			}
		}

		void RefreshInvoicesUSPPIDocAddress(ZGuid oldOrgPK)
		{
			foreach (var invoice in Invoices.Cast<JobComInvoiceHeader>())
			{
				var uSPPIDocAddress = invoice.USPPIDocAddress;
				if (uSPPIDocAddress.OrganisationPK == oldOrgPK || uSPPIDocAddress.OrganisationPK == JE_OH_Supplier || (!oldOrgPK.IsEmpty && uSPPIDocAddress.OrganisationPK.IsEmpty))
				{
					invoice.RefreshUSPPIDocAddress();
				}
			}
		}

		void DefaultStateOfOriginIfPossible()
		{
			if (IsExport && Supplier != null)
			{
				StateOfOriginDefaulter.Default(US_StateOfOriginInfo, SupplierPickupAddress);
			}
		}

		void ReSynchronizeExportOrganisationIfNeeded()
		{
			bool shouldSynchronize = IsExport;
			foreach (JobComInvoiceHeader invoice in Invoices)
			{
				invoice.ReSynchronizeExportOrganisationIfNeeded(shouldSynchronize);
			}
		}

		void DefaultDataFromSupplierImporterLink()
		{
			OrgSupplierBuyerLink supplierBuyerLink = SupplierImporterLink;
			if (supplierBuyerLink != null)
			{
				DefaultDataFromSupplierImporterLinkTransportMode(supplierBuyerLink);
				if (!supplierBuyerLink.OL_RelatedParty.IsEmpty)
				{
					US_TransactionsRelated = supplierBuyerLink.OL_RelatedParty;
				}

				MarkReconIndicatorsDirty();
			}
		}

		void DefaultDataRelatedToTruck()
		{
			if (IsTruck && IsImport)
			{
				if (JE_ExportDate.IsEmpty)
				{
					JE_ExportDate = ZDateTime.Today;
				}
				if (JE_DateOfArrival.IsEmpty)
				{
					JE_DateOfArrival = ZDateTime.Today;
				}
				if (US_EntryDate.IsEmpty)
				{
					US_EntryDate = ZDateTime.Today;
				}

				if (US_EntryMode != EntryModeList.Codes.RLF && US_SchDEntry.IsEmpty && !US_SchDArrival.IsEmpty)
				{
					US_SchDEntry = US_SchDArrival;
				}

				if (!US_UI_NKCarrierSCAC.IsEmpty && JE_MasterBillIssuerSCAC.IsEmpty)
				{
					JE_MasterBillIssuerSCAC = US_UI_NKCarrierSCAC;
				}

				DefaultEntrySummaryAndCargoReleaseForTruck();
			}
		}

		void DefaultEntrySummaryAndCargoReleaseForTruck()
		{
			var matchedBorderCargoPort = BCRPortsFromRegistry.OfType<BorderCargoPort>().FirstOrDefault(x => x.PortCode == US_SchDEntry);
			var crProcess = matchedBorderCargoPort?.CRProcess ?? ZString.Empty;
			if (crProcess == CRProcessList.Codes.OneStep && US_EntryType != EntryTypeList.Codes.LowValue)
			{
				US_EnableENS = true;
				US_CertifyCargoRelease = true;
			}
			else if (crProcess == CRProcessList.Codes.TwoStep)
			{
				US_EnableCRL = true;
			}
		}

		void DefaultDataFromSupplierImporterLinkTransportMode(OrgSupplierBuyerLink supplierBuyerLink)
		{
			if (supplierBuyerLink != null && !JE_TransportMode.IsEmpty && IsImport)
			{
				OrgSupBuyLinkTrnMode linkTrnMode = supplierBuyerLink.OrgSupBuyLinkTrnModes.Find(GetMappedTransportMode(), ZString.Empty);
				if (linkTrnMode != null)
				{
					if (!linkTrnMode.PF_USPortOfLading.IsEmpty)
					{
						US_SchDLoading = linkTrnMode.PF_USPortOfLading;
					}
					if (IsSchDArrivalAllowed && !linkTrnMode.PF_USPortOfUnLading.IsEmpty)
					{
						US_SchDArrival = linkTrnMode.PF_USPortOfUnLading;
					}
					if (US_EntryType.IsEmpty)
					{
						US_EntryType = supplierBuyerLink.GetAddInfo().ZO_EntryType;
					}

					JE_OH_ShippingLine = linkTrnMode.PF_OH_CarrierLine;

					if (supplierBuyerLink.Buyer != null)
					{
						var arrivalLocationPK = linkTrnMode.PF_OA_CustomsControlledArrivalLocation;
						var examSitePK = linkTrnMode.PF_OA_CustomsExamSite;

						var firmsCode = supplierBuyerLink.Buyer.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates, arrivalLocationPK);
						var examSite = supplierBuyerLink.Buyer.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates, examSitePK);

						if (!firmsCode.IsEmpty)
						{
							US_US_NKLocationOfGoods = firmsCode.Left(4);
						}

						if (!examSite.IsEmpty)
						{
							US_US_NKCentralizedExamSite = examSite.Left(4);
						}
					}
				}
			}
		}
		ZString GetMappedTransportMode()
		{
			ZString result = ZString.Empty;

			switch (JE_TransportMode)
			{
				case TransportTypeList.Codes.Air:
					result = Core.Constants.TransportModes.Air;
					break;
				case TransportTypeList.Codes.BorderWaterBorne:
				case TransportTypeList.Codes.FixedTransportInstallations:
				case TransportTypeList.Codes.Mail:
				case TransportTypeList.Codes.PassengerHandCarried:
				case TransportTypeList.Codes.Pedestrian:
					result = Core.Constants.TransportModes.Other;
					break;
				case TransportTypeList.Codes.Rail:
					result = Core.Constants.TransportModes.Rail;
					break;
				case TransportTypeList.Codes.Auto:
				case TransportTypeList.Codes.Road:
				case TransportTypeList.Codes.Truck:
					result = Core.Constants.TransportModes.Road;
					break;
				case TransportTypeList.Codes.Sea:
					result = Core.Constants.TransportModes.Sea;
					break;
				default:
					result = Core.Constants.TransportModes.Unknown;
					break;
			}

			return result;
		}

		protected override void OnOverrideFreightDefaultSet()
		{
			base.OnOverrideFreightDefaultSet();
			DefaultNumberOfPacksToManifestQtyAndUQIfRequired();
		}

		internal void DefaultNumberOfPacksToManifestQtyAndUQIfRequired()
		{
			if (!IsImportingData && IsPackingInformationRelevant && LowestBills.Count == 1)
			{
				if (JE_TotalNoOfPacks > 0 || !JE_TotalNoOfPacksPackType.IsEmpty || (Declaration.IsACECargoCertificationMode && (Declaration.IsHandCarry || Declaration.IsACEAutoRoadAndPedTransportMode)))
				{
					var bill = GetOrCreateBillToDefaultPackDetailsTo();
					if (JE_TotalNoOfPacks > 0)
					{
						bill.CU_NoOfPacks = new ZDecimal(JE_TotalNoOfPacks);
					}
					if (!JE_TotalNoOfPacksPackType.IsEmpty)
					{
						bill.CU_PackType = JE_TotalNoOfPacksPackType;
					}

					if (Declaration.IsACECargoCertificationMode && (Declaration.IsHandCarry || Declaration.IsACEAutoRoadAndPedTransportMode) && JE_TotalNoOfPacks.IsEmpty)
					{
						bill.CU_NoOfPacks = 1m;
					}
				}
			}
		}

		protected override Customs.Business.Bill GetOrCreateBillToDefaultPackDetailsTo()
		{
			if (PrimaryMasterBill is Bill primaryMasterBill && primaryMasterBill.IsLowestBill)
			{
				return primaryMasterBill;
			}

			return base.GetOrCreateBillToDefaultPackDetailsTo();
		}

		internal void DefaultInBondTypeIfRequired()
		{
			if (US_SchDLoading == US_SchDExport && US_InbondType.IsEmpty)
			{
				US_InbondType = InbondTypeList.Codes.MerchandiseNOTShippedInbond;
			}
		}

		internal void UpdateUS_UI_NKCarrierSCACIfNeeded()
		{
			if (IsImport || IsExport)
			{
				var shippingLine = ShippingLine;
				var defaultCarrierCode = ZString.Empty;

				if (shippingLine != null)
				{
					defaultCarrierCode = ShippingLineSCACCode;
				}

				if (defaultCarrierCode.IsEmpty)
				{
					defaultCarrierCode = JE_MasterBillIssuerSCAC;
				}

				if (!defaultCarrierCode.IsEmpty)
				{
					US_UI_NKCarrierSCAC = defaultCarrierCode;
				}
			}
		}

		void UpdateShippingLineIfNeeded()
		{
			if (JE_OH_ShippingLine.IsEmpty && !US_UI_NKCarrierSCAC.IsEmpty)
			{
				var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);
				query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
				query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, US_UI_NKCarrierSCAC);
				var orgCusCodes = Factory.Load<OrgCusCode>(query);

				var activeOrgs = from OrgCusCode cusCode in orgCusCodes
								 where cusCode.Organisation != null && cusCode.Organisation.OH_IsActive
								 select cusCode.Organisation;

				if (activeOrgs.Take(2).Count() == 1)
				{
					JE_OH_ShippingLine = activeOrgs.First().PK;
				}
			}
		}

		protected override JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new USJobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		protected override void ResetValuesOnTemplateCopyAfterClone(BaseJobDeclaration declaration1, CloneType cloneType)
		{
			base.ResetValuesOnTemplateCopyAfterClone(declaration1, cloneType);

			var declaration = (JobDeclaration)declaration1;
			declaration.DefaultEntryFilerCode();
			declaration.US_TransportReference = ZString.Empty;
			declaration.JE_OwnerRef = ZString.Empty;
			declaration.JE_TotalWeight = ZDecimal.Zero;
			declaration.JE_TotalVolume = ZDecimal.Zero;
			declaration.JE_TotalVolumeUnit = ZString.Empty;
			declaration.JE_TotalNoOfPacks = ZInt.Zero;
			declaration.BLUStatus = ZString.Empty;
			declaration.FDAStatus = ZString.Empty;
			declaration.FDAMsgStatus = !declaration.CanHavePGAFDA && declaration.HasFDATariffsToBeDeclared ? FDAStatusList.Codes.REQ : "";
			declaration.US_BRDRefNo = ZString.Empty;
			declaration.ReleaseStatus = declaration.US_CertifyCargoRelease ? CRLReleaseStatusList.Codes.NRL : "";
			declaration.US_PaperlessEntry = ZString.Empty;
			declaration.US_AESBGMRefLastUsed = ZString.Empty;
			declaration.JE_EntryStatus = ZString.Empty;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			declaration.US_CheckNo = ZString.Empty;
			declaration.US_LatestRateDate = ZDateTime.Empty;
			declaration.US_Paid = ZString.Empty;
			declaration.US_PSC = false;
			declaration.US_FixPSD = false;
			declaration.US_AuditSpiLastLogDate = ZDateTime.Empty;
			declaration.US_AuditSpiLastLogReference = ZString.Empty;
			declaration.US_AuditSpiLastLogUser = ZString.Empty;
			declaration.US_AuditFdaLastLogDate = ZDateTime.Empty;
			declaration.US_AuditFdaLastLogReference = ZString.Empty;
			declaration.US_AuditFdaLastLogUser = ZString.Empty;
			declaration.US_StatusTibLastLogDate = ZDateTime.Empty;
			declaration.US_StatusTibLastLogReference = ZString.Empty;
			declaration.US_StatusTibLastLogUser = ZString.Empty;
			declaration.JE_CustomsCommencedDate = ZDateTime.Empty;
			declaration.JE_GS_NKCustomsCommencedUser = ZString.Empty;
			declaration.US_SESplitRel = ZString.Empty;
			declaration.US_DeferredTaxDueDate = ZDateTime.Empty;
			declaration.US_FixDefTaxDueDate = false;
			declaration.AdmissionStatus = ZString.Empty;
			declaration.FTZArrivalStatus = ZString.Empty;
			declaration.FTZConcurrenceStatus = ZString.Empty;
			declaration.FTZDeliveryOfGoodsStatus = ZString.Empty;
			declaration.FTZPTTStatus = ZString.Empty;
			declaration.US_BondDispositionCode = ZString.Empty;
			declaration.US_BondDispositionCode2 = ZString.Empty;
			declaration.US_CBPBondNo = ZString.Empty;
			declaration.US_CBPBondNo2 = ZString.Empty;
			declaration.US_NoDutyCalc = false;
			declaration.US_PGAReplaceUpdateNeeded = ZString.Empty;
			declaration.US_PGACorrectionStatus = ZString.Empty;
			declaration.US_QuotaStatus = ZString.Empty;
			declaration.US_ConsolidatedJobNumber = ZString.Empty;
			declaration.US_ConsolidatedEntryNumber = ZString.Empty;
			declaration.US_InsuranceDisposition = ZString.Empty;
			declaration.US_FTZConcurrenceQty = ZDecimal.Zero;

			if (!GlbStaff.CurrentUser.GS_IsSystemAccount && !USCustomsDataRegistry.Instance.EntryDeclarant.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty))
			{
				declaration.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			}

			if (declaration.US_IsAIIRequested)
			{
				declaration.US_IsAIIRequested = false;
			}

			if (declaration.US_ConsolACE)
			{
				declaration.US_ConsolACE = false;
			}

			if (declaration.HasCreatedJE_OA_DeclarantAddress_ZAddress)
			{
				declaration.JE_OA_DeclarantAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(declaration.DeclarantAddress?.OA_OH ?? ZGuid.Empty);
			}

			ResetValuesOnGroupInvoiceForTemplateCopy(declaration.TopGroupInvoice);

			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				ResetValuesOnInvoiceForTemplateCopy(invoice);
			}

			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				ResetValuesOnInvoiceLineForTemplateCopy(invoiceLine);
			}

			if (!declaration.IsACEDrawback || declaration.US_AcceleratedClaimInd)
			{
				new BondDetailsDefaulter().Default(declaration, declaration.US_BondType);
			}

			if (declaration.packableInvoiceLines != null)
			{
				declaration.SetupPackableInvoiceLinesIfNeeded();
			}
		}

		void ResetValuesOnInvoiceLineForTemplateCopy(JobComInvoiceLine invoiceLine)
		{
			using (invoiceLine.GetValidationSuspender())
			using (invoiceLine.SuspendSettingHasChanges())
			{
				invoiceLine.JI_Weight = ZDecimal.Zero;
				invoiceLine.JI_WeightUQ = ZString.Empty;
				invoiceLine.JI_Volume = ZDecimal.Zero;
				invoiceLine.JI_VolumeUQ = ZString.Empty;
				invoiceLine.JI_InvoiceQuantity = ZDecimal.Zero;
				invoiceLine.JI_InvoiceUQ = ZString.Empty;
				invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
				invoiceLine.JI_LinePrice = ZDecimal.Zero;
				invoiceLine.JI_CustomsSecondQuantity = ZDecimal.Zero;
				invoiceLine.JI_CustomsThirdQuantity = ZDecimal.Zero;
				invoiceLine.US_ManifestQty = ZInt.Zero;
				invoiceLine.JI_NetWeight = ZDecimal.Zero;
				invoiceLine.JI_NetWeightUQ = ZString.Empty;
				invoiceLine.US_CBTPACertificateNo = ZString.Empty;
				invoiceLine.US_CAExportCertificate = ZString.Empty;
				invoiceLine.US_AgricultureLicNo = ZString.Empty;
				invoiceLine.US_WoolLicenceNo = ZString.Empty;
				invoiceLine.US_CottonCertificateNo = ZString.Empty;
				invoiceLine.US_MiscPermitNo = ZString.Empty;
				invoiceLine.US_PIRPRulingType = ZString.Empty;
				invoiceLine.US_PIRPRulingNo = ZString.Empty;
				invoiceLine.US_PIRPRulingType = ZString.Empty;
				invoiceLine.US_WHSEntryLineNo = ZShort.Zero;
				invoiceLine.JI_BondedWhsQuantity = ZDecimal.Zero;
				invoiceLine.HasChanges = false;
			}
		}

		void ResetValuesOnInvoiceForTemplateCopy(JobComInvoiceHeader invoice)
		{
			using (invoice.GetValidationSuspender())
			using (invoice.SuspendSettingHasChanges())
			{
				invoice.JZ_InvoiceNumber = ZString.Empty;
				invoice.JZ_InvoiceDate = ZDateTime.Empty;
				invoice.US_DateOfExport = ZDateTime.Empty;
				invoice.JZ_InvoiceAmount = ZDecimal.Zero;
				invoice.JZ_Weight = ZDecimal.Zero;
				invoice.JZ_WeightUQ = ZString.Empty;
				invoice.JZ_Volume = ZDecimal.Zero;
				invoice.JZ_VolumeUQ = ZString.Empty;

				foreach (JobComInvCharge charge in invoice.Charges)
				{
					charge.J7_Amount = ZDecimal.Zero;
					charge.HasChanges = false;
				}
			}
			invoice.HasChanges = false;
		}

		void ResetValuesOnGroupInvoiceForTemplateCopy(JobComInvoiceGroupHeader groupInvoice)
		{
			using (groupInvoice.GetValidationSuspender())
			using (groupInvoice.SuspendSettingHasChanges())
			{
				groupInvoice.JZ_InvoiceDate = ZDateTime.Empty;
				groupInvoice.JZ_InvoiceAmount = ZDecimal.Zero;
				groupInvoice.JZ_Weight = ZDecimal.Zero;
				groupInvoice.JZ_WeightUQ = ZString.Empty;
				groupInvoice.JZ_Volume = ZDecimal.Zero;
				groupInvoice.JZ_VolumeUQ = ZString.Empty;
			}
			groupInvoice.HasChanges = false;
		}

		void DefaultTariffTypeIfNeeded()
		{
			if (IsExport && US_TariffType.IsEmpty)
			{
				US_TariffType = USCustomsDataRegistry.Instance.ExportDefaultTariffType.Value;
			}
		}

		void UpdateTransportReferenceIfApplicable(bool overrideEvenIfEmpty)
		{
			if (!IsCopying && IsExport && !ShouldSynchroniseWithShipment() && (overrideEvenIfEmpty || US_TransportReference.IsEmpty))
			{
				if (IsAir || IsRail || IsTruck)
				{
					var transportReference = JE_MasterBill;

					if (IsAir)
					{
						if (transportReference.IsEmpty)
						{
							transportReference = JE_HouseBill;
						}
						else if (transportReference.Length > 3 && !transportReference.Contains("-"))
						{
							transportReference = transportReference.Left(3) + "-" + transportReference.SubstringSafe(3);
						}
					}

					US_TransportReference = transportReference.Left(US_TransportReferenceInfo.MaxLength);
				}
			}
		}

		void SetAESCommodityFilingOption()
		{
			US_CommodityFilingOption = IsExport ? AESCommodityFilingOptionList.Codes._2Predeparture : "";
		}

		void ClearExportFieldsIfNeeded()
		{
			if (!IsExport)
			{
				US_StateOfOrigin = ZString.Empty;
			}
			else
			{
				US_DestinationState = ZString.Empty;

				if (!JE_OA_ManufacturerAddress.IsEmpty)
				{
					JE_OA_ManufacturerAddress_ZAddress.OrgPK = ZGuid.Empty;
				}

				Invoices.ForEach(invoice =>
				{
					if (!invoice.JZ_OA_ManufacturerAddress.IsEmpty)
					{
						invoice.JZ_OA_ManufacturerAddress_ZAddress.OrgPK = ZGuid.Empty;
					}

					invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>().ForEach(line =>
					{
						if (!line.JI_OA_ManufacturerAddress.IsEmpty)
						{
							line.JI_OA_ManufacturerAddress_ZAddress.OrgPK = ZGuid.Empty;
						}
					});
				});
			}
			SetEmptyValueTo_US_SchDArrival_IfRequired();
			SetEmptyValueTo_US_CarrierName_IfRequired();
		}

		void DefaultForImport()
		{
			if (IsImport)
			{
				if (US_TaxDeferIndicator.IsEmpty)
				{
					US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax;
				}

				if (!USCustomsDataRegistry.Instance.EntryDeclarant.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
				{
					if (!GlbStaff.CurrentUser.GS_IsSystemAccount)
					{
						JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
					}
				}

				DefaultFromImporterIfNeeded();
				DefaultCountryOfExport(ZString.Empty);
			}
		}
		void DefaultEntrySummaryAndCargoReleaseForImport()
		{
			if (IsACE)
			{
				if (JE_MessageType == JobMessageTypeList.Codes.Import)
				{
					if (US_EntryType == EntryTypeList.Codes.LowValue || US_EntryType == EntryTypeList.Codes.ConsumptionFTZ)
					{
						US_EnableENS = false;
						US_EnableCRL = true;
					}
					else if (IsTruck)
					{
						DefaultEntrySummaryAndCargoReleaseForTruck();
					}
					else
					{
						US_EnableENS = true;
					}
				}
				else
				{
					US_EnableENS = false;
					US_CertifyCargoRelease = false;
				}
			}
		}

		protected override void SetDefaultValues()
		{
			using (SetSettingDefaultValuesInProgress())
			{
				base.SetDefaultValues();
				SetAESCommodityFilingOption();
				US_ExportCode = ExportInformationCodeList.Codes.OS;
			}
		}

		protected override string DefaultTotalNoOfPacksPackType
		{
			get { return ZString.Empty; }
		}

		public void DefaultRoutingDetails()
		{
			if (WarehouseDocAddress != null && WarehouseDocAddress.Address != null)
			{
				var query1 = new ZQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, WarehouseDocAddress.Address.PK);
				var query2 = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.ABIRoutingCode);
				var query = new ZQuery(query1, query2);
				var orgCusCode = Factory.LoadTop1<OrgCusCode>(query);
				if (orgCusCode != null)
				{
					if (!IsValidFTZRoutingDetails(orgCusCode.OK_CustomsRegNo))
					{
						US_F_RoutingDetails = "??";
					}
					else
					{
						US_F_RoutingDetails = orgCusCode.OK_CustomsRegNo.Substring(0, orgCusCode.OK_CustomsRegNo.Length);
					}
				}
			}
		}

		internal bool MayRequireFDA
		{
			get { return InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.HasFDAReportingRequirement); }
		}

		public void DefaultFDAContact()
		{
			var fdaContactPK = USCustomsDataRegistry.Instance.BranchFDAContact.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
			var fdaContact = Factory.Load<GlbStaff>(fdaContactPK) ?? Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, JE_GS_NKCusAgent);

			if (fdaContact != null)
			{
				US_FDAContactName = fdaContact.GS_FullName.Left(100);
				US_FDAContactPhoneNo = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(fdaContact).Left(AutoUSAddInfo.Schema.US_FDAContactPhoneNoMaxLength);
				US_FDAContactEmail = fdaContact.GS_EmailAddress.Left(US_FDAContactEmailInfo.MaxLength);
			}
		}

		public ZDateTime GetValidReleaseDate()
		{
			ZDateTime validReleaseDate = ZDateTime.Empty;
			var workingDays = CustomsWorkingDays.GetInstance(new BusinessObjectFactory { NameForDebugging = "WorkingDays" });
			if (workingDays != null)
			{
				validReleaseDate = workingDays.GetAnotherStandardWorkingDay(ZDate.Today.ToDateTime(), -10);
			}
			return validReleaseDate;
		}

		public override ZBool US_ConsolACE
		{
			get { return base.US_ConsolACE; }
			set
			{
				var hasChanges = base.US_ConsolACE != value;
				base.US_ConsolACE = value;
				if (!IsCopying && hasChanges)
				{
					new DeclarationPrelimStatementDetailsDefaulter().Default(this);
					if (!US_ConsolACE)
					{
						foreach (JobComInvoiceHeader invoice in Invoices)
						{
							invoice.US_ReleaseEntryNumber = ZString.Empty;
							invoice.AddInfoValidation.ValidateUS_ReleaseEntryNumber();
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.JobDeclarationList))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobDeclaration|US_ConsolidatedJobNumber", Caption = "Consolidated Job No.")]
		[BusinessObjectTestExclude]
		public ZString US_ConsolidatedJobNumber
		{
			get { return this.GetSystemDefinedValue<ZString>(Constants.GenAddOnColumnFieldName.US_ConsolidatedJobNumber); }
			set
			{
				CheckMaximumLength(US_ConsolidatedJobNumberInfo, value);
				this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.US_ConsolidatedJobNumber, value);
				US_ConsolidatedJobNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_ConsolidatedJobNumberInfo
		{
			get { return GetZPropertyInfo(Constants.GenAddOnColumnFieldName.US_ConsolidatedJobNumber); }
		}

		public ZString US_ConsolidatedEntryNumber
		{
			get { return this.GetSystemDefinedValue<ZString>(Constants.GenAddOnColumnFieldName.US_ConsolidatedEntryNumber); }
			set
			{
				CheckMaximumLength(US_ConsolidatedEntryNumbereInfo, value);
				this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.US_ConsolidatedEntryNumber, value);
				US_ConsolidatedEntryNumbereInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_ConsolidatedEntryNumbereInfo
		{
			get { return GetZPropertyInfo(Constants.GenAddOnColumnFieldName.US_ConsolidatedEntryNumber); }
		}

		[RelatedBusinessObject(nameof(GoodsFromFTZ))]
		public override ZString US_GoodsFromFTZ
		{
			get { return base.US_GoodsFromFTZ; }
			set
			{
				var oldValue = US_GoodsFromFTZ;
				base.US_GoodsFromFTZ = value;
				if (oldValue != value && !IsCopying)
				{
					if (US_EntryType == EntryTypeList.Codes.Warehouse)
					{
						DefaultFDADateOnDeclarationLevel();
					}
				}
			}
		}

		public ZZRefCusCodeListCombined GoodsFromFTZ
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_GoodsFromFTZ, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		public bool HasInvoiceLinesWithFDAPriorNotice
		{
			get { return InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.ACE_FDALines.Cast<ACEFDA>().Any(y => y.IsPriorNotice)); }
		}

		public void DefaultFDADateOnDeclarationLevel()
		{
			if (IsImport)
			{
				if (MayRequireFDA || PGAFlags.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2 || PGAFlags.HasInvoiceLinesWithATF || PGAFlags.HasInvoiceLinesWithDEA)
				{
					var updateValue = US_EntryDate;
					if (HasInvoiceLinesWithFDAPriorNotice)
					{
						updateValue = JE_DateOfArrival;
					}

					if (!updateValue.IsEmpty && US_FDAADTA.IsEmpty)
					{
						US_FDAADTA = updateValue;
					}
				}
			}
		}

		protected override ExportStatementCreator CreateNewExportStatementCreator(ExportStatementSetting exportStatementSetting)
		{
			return new DeclarationExportStatementCreator(this, exportStatementSetting);
		}

		protected override bool ShouldCopyPortOfArrivalToFinalDestination
		{
			get { return false; }
		}

		protected override ZDate GetDateForDutyRateCore()
		{
			if (IsExport)
			{
				return ZDate.Today;
			}
			throw new InvalidOperationException("You cannot use duty rate date at declaration level. Use invoice line or entry level.");
		}

		protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			base.JE_MessageTypeChanged(oldValue, newValue);
			if (IsExport)
			{
				if (IsStandAlone)
				{
					US_HazardousCargo = YesNoDefaultList.Codes.No;
				}
				US_RoutedTransaction = !USCustomsDataRegistry.Instance.ExportDefaultRoutedTransaction.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty) ? string.Empty : YesNoDefaultList.Codes.No;
				US_DestinationState = ZString.Empty;
				RefreshInvoicesUSPPIDocAddress(JE_OH_Supplier);
			}
			else
			{
				US_TransactionsRelated = "";
			}
			if (IsDrawback || IsReconMessageType || JE_MessageType == JobMessageTypeList.MoreCodes.Protest)
			{
				JE_TotalNoOfPacksPackType = ZString.Empty;
			}
		}

		protected override void JE_OH_ImporterChanged(ZGuid oldValue, ZGuid newValue)
		{
			base.JE_OH_ImporterChanged(oldValue, newValue);
			DefaultDataFromSupplierImporterLink();
			SetupPackableInvoiceLinesIfNeeded();
			MarkReconIndicatorsDirty();
		}

		protected override void JE_OH_SupplierChanged(ZGuid oldValue, ZGuid newValue)
		{
			if (IsExport)
			{
				RefreshInvoicesUSPPIDocAddress(oldValue);
			}

			base.JE_OH_SupplierChanged(oldValue, newValue);
			OrgHeader supplier = Supplier;
			if (supplier != null)
			{
				DefaultDataFromSupplierImporterLink();
			}
			MarkReconIndicatorsDirty();
		}

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("US"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		protected override Customs.Business.MergeManager GetMergeManager()
		{
			return new MergeManager(this);
		}

		public ZBool RequiresBGMReferenceGeneration
		{
			get
			{
				ZBool result = false;

				if (IsExport && ActiveEntryHeaders.Count > 0)
				{
					foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
					{
						if (entry.CH_BGMReference.IsEmpty || entry.IsCurrentlyWithdrawn)
						{
							result = true;
							break;
						}
					}
				}

				return result;
			}
		}

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		protected override bool IsPackingInformationRelevantCore
		{
			get { return (IsImport && !IsExWarehouse) || IsFTZAdmission; }
		}

		public override ZDateTime US_FDAADTA
		{
			get { return base.US_FDAADTA; }
			set
			{
				if (!IsCopying && US_FDAADTA != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDAADTA = value;
			}
		}

		[RelatedBusinessObject(nameof(LocationOfGoods))]
		public override ZString US_US_NKLocationOfGoods
		{
			get { return base.US_US_NKLocationOfGoods; }
			set
			{
				if (!IsCopying && US_US_NKLocationOfGoods != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_US_NKLocationOfGoods = value;
			}
		}

		public ZZRefCusCodeListCombined LocationOfGoods
		{
			get
			{
				return Factory.GetCachedValue(ZString.Format("USFIRMS{0}_{1}_{2}", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, US_US_NKLocationOfGoods, ZDateTime.Today), () =>
				{
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_US_NKLocationOfGoods, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);
				});
			}
		}

		public ZString FIRMSAddress
		{
			get
			{
				return LocationOfGoods?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityAddress) ?? ZString.Empty;
			}
		}

		public ZString FIRMSCity
		{
			get
			{
				return LocationOfGoods?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.City) ?? ZString.Empty;
			}
		}

		public ZString FIRMSState
		{
			get
			{
				return LocationOfGoods?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.State) ?? ZString.Empty;
			}
		}

		public ZString FIRMSZIPCode
		{
			get
			{
				return LocationOfGoods?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ZIPCode) ?? ZString.Empty;
			}
		}

		public ZString FIRMSCountry
		{
			get
			{
				return LocationOfGoods?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Country) ?? ZString.Empty;
			}
		}

		public ZString FormattedLocationOfGoodsMassage
		{
			get 
			{
				return (LocationOfGoods != null) ? ZString.Format("{0}, {1} {2} {3} {4}", FIRMSAddress, FIRMSCity, FIRMSState, FIRMSZIPCode, FIRMSCountry) : ZString.Empty;
			}
		}

		[RelatedBusinessObject(nameof(InspectionFIRMS))]
		public override ZString US_InspecFirms
		{
			get { return base.US_InspecFirms; }
			set
			{
				if (!IsCopying && US_InspecFirms != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_InspecFirms = value;
			}
		}

		public ZZRefCusCodeListCombined InspectionFIRMS
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_InspecFirms, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		public override ZDateTime US_InspecDate
		{
			get { return base.US_InspecDate; }
			set
			{
				if (!IsCopying && US_InspecDate != value)
				{
					PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_InspecDate = value;
			}
		}

		public void ReCalculateENSAction()
		{
			var result = ZString.Empty;
			IMessageAttachee messageAttachee = IsDrawback ? this : (IsACERecon ? ActiveEntryHeaders.ReconciliationEntry : ActiveEntryHeaders.EntrySummaryEntry);

			if (messageAttachee != null)
			{
				var messages = messageAttachee.Messages.OfType<MQEDIMessage>()
					.Where(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification);
				if (messages.Any())
				{
					result = messages.Any(x => !x.IsComplete) ? Constants.Incomplete : Constants.Complete;
				}
			}

			US_ENSAction = result;
		}

		public void ReCalculateCRLAction()
		{
			var result = ZString.Empty;

			var messages = ActiveEntryHeaders.SimplifiedEntry?.Messages.OfType<MQEDIMessage>()
				.Where(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus
					&& x.EM_ApplicationCode == EDIMessage.ApplicationCodes.USCustomsImport
					&& x.EM_ApplicationReference == ReferenceIdentifierQualifierCodeList.Codes.CMT);
			if (messages != null && messages.Any())
			{
				result = messages.Any(x => !x.IsComplete) ? Constants.Incomplete : Constants.Complete;
			}

			US_CRLAction = result;
		}

		#endregion

		#region IRatingSupporter Members

		public RatingAdaptersProvider AdaptersProvider
		{
			get
			{
				RatingAdaptersProvider result = null;
				if (IsReconMessageType)
				{
					result = new ReconDeclarationRatingAdaptersProvider(ReconDeclaration ?? new ReconDeclaration(this));
				}
				else if (IsProtestMessageType)
				{
					result = new Protest.ProtestRatingAdaptersProvider(Protest ?? new Protest.Protest(this));
				}

				return result ?? new JobDeclarationRatingAdaptersProvider(this);
			}
		}

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new JobDeclarationRatingAdapter<JobDeclaration>(this);
		}

		#endregion

		#region Related Objects

		public GlbExternalPassword GetCredential()
		{
			GlbExternalPassword result = null;

			var insuranceAgent = US_InsuranceAgent;

			if (!insuranceAgent.IsEmpty)
			{
				var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
				result = staffWrapper.PasswordCollection.Cast<GlbExternalPassword>().FirstOrDefault(c => c.GP_MailBoxID == insuranceAgent);

				if (result == null)
				{
					var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
					result = companyWrapper.PasswordCollection.Cast<GlbExternalPassword>().FirstOrDefault(c => c.GP_MailBoxID == insuranceAgent);
				}
			}

			return result;
		}

		public IDisposable SuspendDefaultingSecondaryTariffLines()
		{
			return new DefaultSecondaryTariffLinesSuspender(this);
		}

		public bool IsDefaultSecondaryTariffLinesSuspended
		{
			get { return var_DefaultSecondaryTariffLinesIndex > 0; }
		}

		int var_DefaultSecondaryTariffLinesIndex;
		class DefaultSecondaryTariffLinesSuspender : IDisposable
		{
			public DefaultSecondaryTariffLinesSuspender(JobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.var_DefaultSecondaryTariffLinesIndex++;
			}

			readonly JobDeclaration declaration;

			#region IDisposable Members

			public void Dispose()
			{
				declaration.var_DefaultSecondaryTariffLinesIndex--;
			}

			#endregion
		}

		#region Related Statement

		public CusStatementHeader RelatedStatement
		{
			get
			{
				if (relatedStatementCached == null)
				{
					relatedStatementCached = new CachedProperty<CusStatementHeader>(Factory, delegate
					{
						var entryNumber = ImportEntryNumber;
						return entryNumber.IsEmpty || Branch == null ? null : new CusStatementHeader.Loader(Factory).Load(US_EntryFilerCode, entryNumber, Branch.GB_GC);
					});
				}
				return relatedStatementCached.Value;
			}
		}
		CachedProperty<CusStatementHeader> relatedStatementCached;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.StatementList))]
		public ZGuid RelatedStatementPK
		{
			get
			{
				CusStatementHeader result = RelatedStatement;
				return result != null ? result.PK : ZGuid.Empty;
			}
		}

		public ZPropertyInfo RelatedStatementPKInfo
		{
			get { return GetZPropertyInfo(Schema.RelatedStatementPK); }
		}

		#endregion

		#endregion

		#region Inventory Management

		public override event EventHandler OnBondedWarehouseRelatedFieldChanged
		{
			add
			{
				base.OnBondedWarehouseRelatedFieldChanged += value;
				US_EnableENSInfo.ValueChanged -= value;
				US_EnableENSInfo.ValueChanged += value;
				US_EntryTypeInfo.ValueChanged -= value;
				US_EntryTypeInfo.ValueChanged += value;
			}
			remove
			{
				base.OnBondedWarehouseRelatedFieldChanged -= value;
				US_EnableENSInfo.ValueChanged -= value;
				US_EntryTypeInfo.ValueChanged -= value;
			}
		}

		protected override bool IsInwardBondedWarehousingEnabledCore
		{
			get { return IsWarehouseEntryType; }
		}

		protected override bool IsOutwardBondedWarehousingEnabledCore
		{
			get { return IsExWarehouseEntryType || IsENSFormalImportAndConsumptionFTZ; }
		}

		void SetupPackableInvoiceLinesIfNeeded()
		{
			if (packableInvoiceLines != null)
			{
				if (IsInwardBondedWarehousingEnabled)
				{
					packableInvoiceLines.HookEventsAndReload();
				}
				else
				{
					packableInvoiceLines.UnHookEvents();
				}
			}
		}

		public PackableInvoiceLineAdhocCollection PackableInvoiceLines
		{
			get
			{
				if (packableInvoiceLines == null)
				{
					packableInvoiceLines = new PackableInvoiceLineAdhocCollection(this);
					SetupPackableInvoiceLinesIfNeeded();
				}
				return packableInvoiceLines;
			}
		}
		PackableInvoiceLineAdhocCollection packableInvoiceLines;

		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public WHSPackLineFilteredCollection WHSPackFilteredLines
		{
			get
			{
				if (whsPackFilteredLines == null)
				{
					whsPackFilteredLines = new WHSPackLineFilteredCollection(this);
				}
				return whsPackFilteredLines;
			}
		}
		WHSPackLineFilteredCollection whsPackFilteredLines;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public WHSPackLineCollection WHSPackLines
		{
			get
			{
				if (whsPackLines == null)
				{
					whsPackLines = new WHSPackLineCollection(this);
					whsPackLines.Load();
					RegisterEditableChildObject(whsPackLines);
				}
				return whsPackLines;
			}
		}
		WHSPackLineCollection whsPackLines;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public WHSPackCollection WHSPacks
		{
			get
			{
				if (whsPacks == null)
				{
					whsPacks = new WHSPackCollection(this);
					whsPacks.Load();
					RegisterEditableChildObject(whsPacks);
				}
				return whsPacks;
			}
		}
		WHSPackCollection whsPacks;

		protected override bool SupportsBondedWarehousingCore
		{
			get { return IsWHSUniversalXMLActive && !IsRecon && !IsProtest && !IsDrawback; }
		}

		#endregion

		#region Collections

		public bool HasInactiveMessages
		{
			get
			{
				return IsImport &&
					(CustomsEntryHeaders.Count > ActiveEntryHeaders.Count
					|| Messages.Find(new ZQuery(EDIMessageSchema.EM_Status, EDIMessage.Status.Discarded)).Length > 0);
			}
		}

		/// <summary>
		/// This is the collection Messages tab is bound to. Container/Bill/CusEntryHeader/Declaration/Liquidation are shown
		/// </summary>
		[ChildEditable(true)]
		public MessageActionRelatedRecordWrapperCollection InBondRelatedRecords
		{
			get
			{
				if (fInBondWPRelatedRecords == null)
				{
					fInBondWPRelatedRecords = new MessageActionRelatedRecordWrapperCollection(this, new GetMessageAttacheesToBuild(GetMessageAttacheesToShowInMessagesTab));
					RegisterEditableChildObject(fInBondWPRelatedRecords);
				}
				return fInBondWPRelatedRecords;
			}
		}
		MessageActionRelatedRecordWrapperCollection fInBondWPRelatedRecords;

		IEnumerable<IMessageAttachee> GetMessageAttacheesToShowInMessagesTab(MessagesToShowCollection.MessagesStatus messageStatus)
		{
			LoadFetchHintForEDIMessage(messageStatus);
			var query = new ZQuery(EDIMessageSchema.EM_Status, messageStatus == MessagesToShowCollection.MessagesStatus.InactiveOnly ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, EDIMessage.Status.Discarded);

			if (Messages.Count > 0 && Messages.Any(x => !((MQEDIMessage)x).IsBIRDTransaction))
			{
				if (Messages.Find(query).Length > 0)
				{
					yield return this;
				}
			}

			if (!IsDrawback)
			{
				foreach (CusEntryHeader entry in CustomsEntryHeaders)
				{
					if (messageStatus == MessagesToShowCollection.MessagesStatus.ActiveOnly && entry.IsActive ||
						messageStatus == MessagesToShowCollection.MessagesStatus.InactiveOnly && !entry.IsActive)
					{
						if ((entry.Messages.Count > 0 && entry.Messages.Any(x => !((MQEDIMessage)x).IsBIRDTransaction)) || entry.IsEBondMessageAttacher)
						{
							yield return entry;
						}
					}
				}

				foreach (CusContainer container in CusContainers)
				{
					if (container.Messages.Find(query).Length > 0)
					{
						yield return container;
					}
				}

				foreach (Bill bill in Bills)
				{
					if (bill.Messages.Find(query).Length > 0)
					{
						yield return bill;
					}
				}

				foreach (JobComInvoiceHeader invoice in Invoices)
				{
					if (invoice.Messages.Find(query).Length > 0)
					{
						yield return invoice;
					}
				}

				var hasBIRDMessage = Messages.Any(x => ((MQEDIMessage)x).IsBIRDTransaction) ||
					CustomsEntryHeaders.Any(entry => entry.Messages.Any(message => ((MQEDIMessage)message).IsBIRDTransaction)) ||
					Liquidations.Any(liq => liq.Message != null && liq.Message.IsBIRDTransaction);

				if (hasBIRDMessage)
				{
					yield return BIRDMessageAttachee;
				}
			}

			if (messageStatus == MessagesToShowCollection.MessagesStatus.ActiveOnly)
			{
				if (Liquidations.Count > 0 && Liquidations.Any(liq => liq.Message != null && !liq.Message.IsBIRDTransaction))
				{
					yield return LiquidationWithMessagesToShow;
				}
			}
		}

		void LoadFetchHintForEDIMessage(MessagesToShowCollection.MessagesStatus messageStatus)
		{
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, PK);
			foreach (CusEntryHeader entry in CustomsEntryHeaders)
			{
				if (messageStatus == MessagesToShowCollection.MessagesStatus.ActiveOnly && entry.IsActive ||
					messageStatus == MessagesToShowCollection.MessagesStatus.InactiveOnly && !entry.IsActive)
				{
					Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, entry.PK);
				}
			}

			foreach (CusContainer container in CusContainers)
			{
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, container.PK);
			}

			foreach (Bill bill in Bills)
			{
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, bill.PK);
			}

			foreach (JobComInvoiceHeader invoice in Invoices)
			{
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, invoice.PK);
			}

			if (messageStatus == MessagesToShowCollection.MessagesStatus.ActiveOnly)
			{
				foreach (CusLiquidation liq in Liquidations)
				{
					Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, liq.PK);
				}
			}
		}

		internal BIRDMessageAttachee BIRDMessageAttachee
		{
			get { return birdMessageAttachee ?? (birdMessageAttachee = new BIRDMessageAttachee(this)); }
		}
		BIRDMessageAttachee birdMessageAttachee;

		LiquidationWithMessagesToShow LiquidationWithMessagesToShow
		{
			get { return liquidationWithMessagesToShow ?? (liquidationWithMessagesToShow = new LiquidationWithMessagesToShow(Liquidations)); }
		}
		LiquidationWithMessagesToShow liquidationWithMessagesToShow;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public ContractNumberCollection ContractNumbers
		{
			get
			{
				if (contractNumbers == null)
				{
					contractNumbers = new ContractNumberCollection(this);
					contractNumbers.Load();
					RegisterEditableChildObject(contractNumbers);
				}
				return contractNumbers;
			}
		}
		ContractNumberCollection contractNumbers;

		[ChildEditable(true)]
		public EntryHeaderWithDeactivatedCollection EntryHeadersWithOptionalDeactivated
		{
			get
			{
				if (entryHeadersWithOptionalDeactivated == null)
				{
					entryHeadersWithOptionalDeactivated = new EntryHeaderWithDeactivatedCollection(this);
					RegisterEditableChildObject(entryHeadersWithOptionalDeactivated);
				}
				return entryHeadersWithOptionalDeactivated;
			}
		}
		EntryHeaderWithDeactivatedCollection entryHeadersWithOptionalDeactivated;

		#region Dispositions

		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public DispositionDataCollection DispositionCodes
		{
			get
			{
				if (fDispositionCodes == null)
				{
					fDispositionCodes = new DispositionDataCollection(this);
					fDispositionCodes.Load();
					if (fDispositionCodes.Count == 0 && CRLProcessingResultsMessages.Length > 0)
					{
						EntryStatusesAndErrors.UpdateDispositionCodesFromMessage(CRLProcessingResultsMessages, fDispositionCodes);
					}
					fDispositionCodes.Sort(DispositionData.Schema.US_DispositionDate, ListSortDirection.Descending);
				}

				return fDispositionCodes;
			}
		}
		DispositionDataCollection fDispositionCodes;

		public StatusErrorsDataViewCollection DispositionCodesView
		{
			get
			{
				if (dispositionCodesView == null)
				{
					dispositionCodesView = new StatusErrorsDataViewCollection(Factory);
					dispositionCodesView.Populate(DispositionCodes.OfType<DispositionData>());
				}
				return dispositionCodesView;
			}
		}
		StatusErrorsDataViewCollection dispositionCodesView;

		public ZDateTime GetLatestDispositionDate()
		{
			ZDateTime result = ZDateTime.Empty;

			foreach (DispositionData disposition in DispositionCodes)
			{
				if (result.IsEmpty || disposition.US_DispositionDate > result)
				{
					result = disposition.US_DispositionDate;
				}
			}

			return result;
		}

		Enterprise.Messaging.Business.EDIMessage[] CRLProcessingResultsMessages
		{
			get
			{
				if (crlProcessingResultsMessages == null)
				{
					crlProcessingResultsMessages = Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults }, EDIMessage.Direction.Receive);
				}
				return crlProcessingResultsMessages;
			}
		}
		Enterprise.Messaging.Business.EDIMessage[] crlProcessingResultsMessages;

		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public DispositionDataCollection FTZDispositionCodes
		{
			get
			{
				if (ftzDispositionCodes == null)
				{
					ftzDispositionCodes = new DispositionDataCollection(this);
					ftzDispositionCodes.Load();
					if (ftzDispositionCodes.Count == 0 && FTZResponseMessages.Length > 0)
					{
						EntryStatusesAndErrors.UpdateFTZDispositionCodes(FTZResponseMessages, ftzDispositionCodes);
					}
				}
				return ftzDispositionCodes;
			}
		}
		DispositionDataCollection ftzDispositionCodes;

		Enterprise.Messaging.Business.EDIMessage[] FTZResponseMessages
		{
			get
			{
				if (ftzResponseMessages == null)
				{
					ftzResponseMessages = Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport,
						new ZString[] { ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone }, EDIMessage.Direction.Receive);
					ftzResponseMessages.OrderByDescending(x => x.EM_SystemCreateTimeUtc);
				}
				return ftzResponseMessages;
			}
		}
		Enterprise.Messaging.Business.EDIMessage[] ftzResponseMessages;

		#endregion

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public DeliveryOrderHeaderCollection DeliveryOrderHeaders
		{
			get
			{
				if (fDeliveryOrderHeaders == null)
				{
					fDeliveryOrderHeaders = new DeliveryOrderHeaderCollection(this);
					fDeliveryOrderHeaders.Load();
					RegisterEditableChildObject(fDeliveryOrderHeaders);
				}
				return fDeliveryOrderHeaders;
			}
		}
		DeliveryOrderHeaderCollection fDeliveryOrderHeaders;

		public BusinessObjectCollection PortOfLadingMappings
		{
			get { return PortOfLadingDefaulter.MappingPorts; }
		}

		public BusinessObjectCollection PortOfExportRefLocoMappings
		{
			get { return PortOfExportDefaulter.MappingPorts; }
		}

		public BusinessObjectCollection PortOfArrivalRefLocoMappings
		{
			get { return PortOfArrivalDefaulter.MappingPorts; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public LinkedEntryCollection LinkedEntryNumbers
		{
			get
			{
				if (linkedEntryNumbers == null)
				{
					linkedEntryNumbers = new LinkedEntryCollection(this);
					linkedEntryNumbers.Load();
					RegisterEditableChildObject(linkedEntryNumbers);
				}

				return linkedEntryNumbers;
			}
		}
		LinkedEntryCollection linkedEntryNumbers;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public USDeclarationFSISLineCollection FSISLines
		{
			get
			{
				if (fFSISLines == null)
				{
					fFSISLines = GetUSFSISLineCompleteCollection();
					fFSISLines.Load();
					RegisterEditableChildObject(fFSISLines);
				}
				return fFSISLines;
			}
		}
		USDeclarationFSISLineCollection fFSISLines;

		protected USDeclarationFSISLineCollection GetUSFSISLineCompleteCollection()
		{
			return new USDeclarationFSISLineCollection(this);
		}

		public IReadOnlyList<USInvoiceLineFSISLine> FSISLinesForPrint
		{
			get
			{
				if (fsisLinesForPrint == null)
				{
					fsisLinesForPrint = Array.Empty<USInvoiceLineFSISLine>();
				}
				return fsisLinesForPrint;
			}
			set
			{
				fsisLinesForPrint = value;
			}
		}
		IReadOnlyList<USInvoiceLineFSISLine> fsisLinesForPrint;

		#endregion

		PGAIndicators pgaFlags;
		internal PGAIndicators PGAFlags => pgaFlags ?? (pgaFlags = new PGAIndicators(this));

		#region

		public void ResetToOriginal()
		{
			Declaration.JE_MessageStatus = "";
			foreach (CusEntryHeader oneEntry in Declaration.CustomsEntryHeaders)
			{
				if (oneEntry.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.EntrySummary ||
					oneEntry.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ACECargoRelease)
				{
					oneEntry.CH_Status = "";
				}

				var logs = oneEntry.Logs.GetAllLogs();
				var cesLogs = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, ZArchitecture.Business.Events.CustomsEntryStatus.Code));
				foreach (var oneLog in cesLogs)
				{
					((StmALog)oneLog).Cancel();
				}
			}
		}

		#endregion

		#region Refresh Tariff

		public void RefreshTariff()
		{
			InvoiceLines.RefreshTariff();
			HasChanges = true;
		}

		#endregion

		#region IStatementLineDeclaration Members

		bool IStatementLineDeclaration.HasEntryBeenWithdrawn
		{
			get
			{
				var entry = ActiveEntryHeaders.EntrySummaryEntry;
				return entry != null && entry.HasBeenWithdrawn;
			}
		}

		ZString IStatementLineDeclaration.BrokerReferenceNumber
		{
			get { return BrokerReferenceNumberCore; }
		}

		ZString IStatementLineDeclaration.EntryNumber
		{
			get { return ImportEntryNumber; }
		}

		ZString IStatementLineDeclaration.ReleaseStatus
		{
			get { return ReleaseStatus; }
		}

		ZString IStatementLineDeclaration.ReleaseStatusDescription
		{
			get { return ReleaseStatusDesc; }
		}

		#endregion

		#region IInBondActionHeader Members

		IReadOnlyList<IMessageAttacheeInDeclaration> IMessageActionHeader.MessageAttachees
		{
			get
			{
				ArrayList result = new ArrayList();
				result.Add(this);
				if (!IsDrawback)
				{
					result.AddRange(CustomsEntryHeaders);
					result.AddRange(Bills);
					result.AddRange(CusContainers);

					ITNumberCollection.Populate();
					result.AddRange(ITNumberCollection);

					if (IsElectronicInvoice)
					{
						result.AddRange(Invoices);
					}
				}
				return (IMessageAttacheeInDeclaration[])result.ToArray(typeof(IMessageAttacheeInDeclaration));
			}
		}

		internal ITNumberCollection ITNumberCollection
		{
			get { return itNumberCollection ?? (itNumberCollection = new ITNumberCollection(this)); }
		}
		ITNumberCollection itNumberCollection;

		#endregion

		#region ICargoManifestStatusQueryData Members

		CargoManifestQueryActionType ICargoManifestStatusQueryData.QueryActionType
		{
			get { return CargoManifestQueryActionType.InBond; }
		}

		string ICargoManifestStatusQueryData.TableCode
		{
			get { return JobDeclarationSchema.Constants.Prefix; }
		}

		ZString ICargoManifestStatusQueryData.HumanFriendlyReference
		{
			get { return JE_DeclarationReference; }
		}

		ZString ICargoManifestStatusQueryData.JobReferenceNumber
		{
			get { return JE_DeclarationReference; }
		}

		ZString ICargoManifestStatusQueryData.EntryOrInBondNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.MasterAirWayBillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.HouseAirWayBillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.BillIssuerCode
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.BillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		bool ICargoManifestStatusQueryData.HasPGAData
		{
			get { return PGAFlags.HasInvoiceLinesWithPGA; }
		}

		void ICargoManifestStatusQueryData.LinkToMessage(EDIMessage message)
		{
			Messages.Add(message);
		}

		ZGuid ICargoManifestStatusQueryData.MessageAttacheePK
		{
			get { return PK; }
		}

		internal string ProcessingPortCodeForQuery
		{
			get { return USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty); }
		}

		ZBool ICargoManifestStatusQueryData.IsRelevantFor(ZString actionCode)
		{
			return false;
		}

		#endregion

		#region IBackDoorSavingSupportableBizObj Members

		AmendmentWithdrawalReason IBackDoorSavingSupportableBizObj.GetAmendmentWithdrawalReason()
		{
			return new AmendmentWithdrawalReason();
		}

		bool IBackDoorSavingSupportableBizObj.SupportBackDoorForSavingWhenAmendmentDetected
		{
			get { return IsImport; }
		}

		#endregion

		#region IMessageManageableBizObj Members

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return GetImportMessageManagerForAmendmentDetection();
		}

		protected
#if DEBUG
 virtual
#endif
 JobDeclarationImportMessageManager GetImportMessageManagerForAmendmentDetection()
		{
			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(this, ImportMessageSendingMessageType.Replacement);
			return new JobDeclarationImportMessageManager(this, actions);
		}

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return false; }//turned off as there are many types of messages to send for a formal entry amendment.
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return !MergeManager.RequiresMerge || DoMerge() ? ContinueWithDetection.Yes : ContinueWithDetection.No;
		}

		#endregion

		#region IMessageAttacheeInDeclaration Members

		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages
		{
			get { return new ZGuid[] { PK }; }
		}

		bool IMessageAttacheeInDeclaration.IsActive
		{
			get { return true; }
		}

		MessageAttacheeRecordType IMessageAttacheeInDeclaration.RecordType
		{
			get { return MessageAttacheeRecordType.JobDeclaration; }
		}

		ZString IMessageAttacheeInDeclaration.RecordTypeDescription
		{
			get { return MessageAttacheeRecordTypeDescriptions.Declaration; }
		}

		public ZString EntryFilerCode
		{
			get { return US_EntryFilerCode; }
		}

		public ZString ProcessingDistrictPort
		{
			get { return ProcessingDistrictPortCore; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return TransportMode; }
		}

		/// <summary>
		/// This is virtual for mocking only. Do not override this
		/// </summary>
#if DEBUG
		protected virtual
#endif
		ZString ProcessingDistrictPortCore
		{
			get
			{
				ZString result = "";
				if (IsRemoteLocationFiling || IsPSCFilingOfEntriesByOtherFiler)
				{
					result = US_SchDEntry;
				}
				else
				{
					result = GetProcessingPortCodeFromRegistry(IsFTZAdmission, US_SchDEntry, RegistryCompanyPK, RegistryBranchPK);
				}

				return result;
			}
		}

		public static ZString GetProcessingPortCodeFromRegistry(ZBool isFTZAdmission, ZString schDEntry, Guid registryCompanyPK, Guid registryBranchPK, bool supportRLF = true)
		{
			var result = ZString.Empty;
			if (!isFTZAdmission)
			{
				result = USCustomsDataRegistry.Instance.EntryProcessingPortMappings.GetFallBackValueAtAllLevels(registryCompanyPK, Guid.Empty, Guid.Empty).GetMappedProcessingPort(schDEntry);
			}

			if (result.IsEmpty)
			{
				if (!isFTZAdmission && !schDEntry.IsEmpty && supportRLF && USCustomsDataRegistry.Instance.StatementProcessingPortEqualPortofEntryNonRLF.GetFallBackValueAtAllLevels(registryCompanyPK, registryBranchPK, Guid.Empty))
				{
					result = schDEntry;
				}
				else
				{
					result = (ZString)USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(registryCompanyPK, registryBranchPK, Guid.Empty);
				}
			}
			return result;
		}

		public ZString ProcessingOfficeCode
		{
			get { return USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(RegistryCompanyPK, Guid.Empty, Guid.Empty); }
		}

		ZString IMessageAttacheeInDeclaration.HumanFriendlyReference
		{
			get { return JE_DeclarationReference; }
		}

		ZDateTime IMessageAttacheeInDeclaration.ReleaseDate
		{
			get { return JE_EntryAuthorisationDate; }
		}

		ZDateTime IMessageAttacheeInDeclaration.TIBExpiryDate
		{
			get { return ZDateTime.Empty; }
		}

		ZInt IMessageAttacheeInDeclaration.TIBNumOfExtensions
		{
			get { return 0; }
		}

		ZString IMessageAttacheeInDeclaration.EntryStatus
		{
			get { return ZString.Empty; }
		}

		IEnumerable<INotification> IMessageAttacheeInDeclaration.GetBusinessLayerNotificationsToAddToWrapper()
		{
			yield break;
		}

		ZString IMessageAttacheeInDeclaration.EntryNumber
		{
			get { return ImportEntryNumber; }
		}

		ZString IMessageAttacheeInDeclaration.JobReferenceNumber
		{
			get { return JE_DeclarationReference; }
		}

		ZGuid IMessageAttacheeInDeclaration.DeclarationPK
		{
			get { return PK; }
		}

		#endregion

		#region IMessageAttachee Members

		ZString IMessageAttachee.MessageStatus
		{
			get { return ZString.Empty; }
			set
			{
				//if you want to set a status for query messages, please have a field in AddInfo
				if (Factory.GetCachedValue<MessageStatusListBLU>().ContainsCode(value))
				{
					BLUStatus = value;
				}
			}
		}

		ZString IMessageAttacheeInDeclaration.MessageStatusDescription
		{
			get { return JE_MessageStatusDescription; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return this; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return JE_DeclarationReference; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return Logs; }
		}

		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return RegistryCompanyPK; }
		}

		#endregion

		protected override bool RequiresOrderNumbersOnDocsCore()
		{
			return !IsRecon && !IsDrawback && base.RequiresOrderNumbersOnDocsCore();
		}

		protected override bool RequiresOrderTrackLinkCore()
		{
			return !IsRecon && !IsDrawback && base.RequiresOrderTrackLinkCore();
		}

		public override void MakeNonPersistent()
		{
			base.MakeNonPersistent();
			USDeclaration.MakeNonPersistent();
		}

		public bool HasDeactivatedAESEntryOriginalRejected()
		{
			if (IsExport)
			{
				foreach (CusEntryHeader entry in CustomsEntryHeaders)
				{
					if (entry.CH_Status == AESDirectCustomsEntryStatus.Codes.Error &&
						!entry.HasBeenLodgedAtCustoms
						&& entry.US_IsDeactivationHasChanges)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool AreAllElectronicInvoicesClear()
		{
			foreach (JobComInvoiceHeader invHeader in Invoices)
			{
				EDIMessage responseMessage = (EDIMessage)invHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse, EDIMessage.Direction.Receive);
				if (responseMessage != null)
				{
					if (!((MQEDIMessage)responseMessage).IsAIICleared)
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		///
		/// For Broker Company Name & Address details on CBP forms: use Customs Address of Record details first, then fallback to Org proxy details... Refer WI00030524
		///
		public IAddressDetails BranchIAddressDetails
		{
			get
			{
				IAddressDetails result = ((IPGAContactDetails)OrgHeaderWrapper.New(BrokerCustomsAddressOfRecord))?.CompanyAddress;

				if (result == null)
				{
					result = Branch;

					var orgProxy = Branch.OrgProxy;
					if (orgProxy != null)
					{
						if (orgProxy.MainAddress != null)
						{
							result = ((IPGAContactDetails)OrgHeaderWrapper.New(orgProxy.MainAddress))?.CompanyAddress;
						}
					}
				}

				return result;
			}
		}

		public OrgAddress BrokerCustomsAddressOfRecord
		{
			get
			{
				if (fCustomsAddressOfRecord == null || fCustomsAddressOfRecord.IsDeleted)
				{
					var branch = Declaration.Branch;
					var orgProxy = branch == null ? null : branch.OrgProxy;
					if (orgProxy != null)
					{
						fCustomsAddressOfRecord = orgProxy.GetCustomsAddressOfRecord();
					}
				}

				return fCustomsAddressOfRecord;
			}
		}
		OrgAddress fCustomsAddressOfRecord;

		public IAddressDetails ImporterWrapperAddressDetails
		{
			get
			{
				var ior = IOR;
				var organization = ior ?? Importer;
				return organization.GetCustomsAddressDetailsFallingBackToMainAddress();
			}
		}

		#region FSIS 9540 and PPQ Form 368 Document

		OrgAddress BrokerCustomsAddressOfRecordOrMainAddress
		{
			get { return Declaration.Branch.OrgProxy != null ? Declaration.Branch.OrgProxy.GetCustomsAddressDetailsFallingBackToMainAddress() : null; }
		}

		public ZString BrokerAddressOfRecord
		{
			get
			{
				var address = BrokerCustomsAddressOfRecordOrMainAddress;
				return address == null ? string.Empty : new AddressFormatter(Factory, address, Branch.Company, false).PostalAddress();
			}
		}

		public ZString BrokerAddressOfRecordFor368
		{
			get
			{
				var result = ZString.Empty;
				if (BrokerCustomsAddressOfRecord != null && !BrokerCustomsAddressOfRecord.OA_Address2.IsEmpty)
				{
					result = BrokerCustomsAddressOfRecord.EffectiveCompanyNameTruncated + "\n" + BrokerCustomsAddressOfRecord.OA_Address1 + ", " + BrokerCustomsAddressOfRecord.OA_Address2 + "\n" + BrokerCustomsAddressOfRecord.OA_City + " " + BrokerCustomsAddressOfRecord.OA_State + " " + BrokerCustomsAddressOfRecord.OA_PostCode;
				}
				else
				{
					result = BrokerAddressOfRecord;
				}

				return result;
			}
		}

		public ZString BrokerAddressOfRecordPhone
		{
			get
			{
				var address = BrokerCustomsAddressOfRecordOrMainAddress;
				return address == null ? ZString.Empty : address.OA_Phone;
			}
		}

		public ZString BrokerAddressOfRecordFax
		{
			get
			{
				var address = BrokerCustomsAddressOfRecordOrMainAddress;
				return address == null ? ZString.Empty : address.OA_Fax;
			}
		}

		public Image FSISBrokerSignatureImage
		{
			get
			{
				if (USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
				{
					var declarant =
					USCustomsDataRegistry.Instance.EntryDeclarant.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty)
						|| GlbStaff.CurrentUser.GS_IsSystemAccount
						? CusAgent : GlbStaff.CurrentUser;

					ZGuid signatoryPK = USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
					var signatory = signatoryPK.IsValid ? Factory.Load<GlbStaff>(signatoryPK) : null;
					if (signatory != null)
					{
						return signatory.SignatureImage;
					}
					else if (declarant != null)
					{
						return declarant.SignatureImage;
					}
				}

				return null;
			}
		}

		public ZString UltimateConsigneeContactName
		{
			get
			{
				var consigneeWrapper = UltimateConsigneeWrapper;
				return consigneeWrapper == null ? ZString.Empty : ((IPGAContactDetails)consigneeWrapper).Name;
			}
		}

		public ZString ImporterOfRecordContactName
		{
			get
			{
				var importerWrapper = IORWrapper;
				return importerWrapper == null ? ZString.Empty : ((IPGAContactDetails)importerWrapper).Name;
			}
		}

		#endregion

		#region Transportation Entry Document Properties

		public ZString PortOf
		{
			get
			{
				IForeignRegionalDistrictPort portOf = CountryNameCalculator.GetForeignOrRegionalPort(ProcessingDistrictPort, Factory);
				return portOf == null ? ZString.Empty : portOf.PortName;
			}
		}

		public ZString WarehouseNameAndFIRMSCode
		{
			get { return (LocationOfGoods != null) ? LocationOfGoods.ZZD_Description + " " + LocationOfGoods.ZZD_Code : ""; }
		}

		public OrgHeader InBondEnteredOrImportedBy
		{
			get
			{
				ZGuid organizationPK = ZGuid.Empty;

				if (Declaration.Branch != null && !Declaration.Branch.GB_OH_OrgProxy.IsEmpty)
				{
					organizationPK = Declaration.Branch.GB_OH_OrgProxy;
				}
				else
				{
					organizationPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				}

				return Factory.Load<OrgHeader>(organizationPK);
			}
		}

		public ZBool IsAttorneyInFact
		{
			get { return USCustomsDataRegistry.Instance.IsAttorneyInFact.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty); }
		}

		public ZBool IsInTransit
		{
			get { return IsImport && (US_InbondType == EntryTypeList.Codes.ImmediateExportation || US_InbondType == EntryTypeList.Codes.TransportationExportation); }
		}

		public ZString ImporterCustomsClientNumber
		{
			get { return OrgHeaderWrapper.GetCustomsRelatedCode(Importer, OrgMatchedCustomsRegNoType.EIN); }
		}

		public ZString UltimateConsigneeCustomsClientNumber
		{
			get { return OrgHeaderWrapper.GetCustomsRelatedCode(ConsigneeOrgAddress, OrgMatchedCustomsRegNoType.EIN); }
		}

		public ZString UltimateConsigneeCustomsClientNumberForDocument
		{
			get
			{
				return ConsigneeOrgAddress == null ? ZString.Empty : ConsigneeOrgAddress.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, GetParamsForRegNo);
			}
		}

		public ZBool HasMultipleConsignees
		{
			get
			{
				CachedProperty<ZBool> cached = cachedHasMultipleConsignees ?? (cachedHasMultipleConsignees = new CachedProperty<ZBool>(Factory, delegate
				{
					bool result = false;
					ZGuid consignee = ZGuid.Empty;

					foreach (JobComInvoiceLine invoiceLine in Declaration.InvoiceLines)
					{
						if (!consignee.IsEmpty && invoiceLine.JI_OA_ConsigneeAddress != consignee)
						{
							result = true;
							break;
						}

						consignee = invoiceLine.JI_OA_ConsigneeAddress;
					}

					return result;
				}));

				return cached.Value;
			}
		}
		CachedProperty<ZBool> cachedHasMultipleConsignees;

		public ZString WeightInPounds
		{
			get { return ""; }
		}

		public ZString Duty
		{
			get { return ""; }
		}

		public ZString Rate
		{
			get { return ""; }
		}

		public ZString ValueDollarsOnly
		{
			get { return ""; }
		}

		#endregion

		#region ILandedCostHeader Members

		// TODO: Will be replaced with GenericLandedCostingConfig
		DutyTaxEntryFee ILandedCostHeader.TotalDutyTaxEntryFeeItems
		{
			get
			{
				if (dutyTaxEntryFeeCached == null)
				{
					dutyTaxEntryFeeCached = new CachedProperty<DutyTaxEntryFee>(Factory, delegate
					{
						DutyTaxEntryFee result = new DutyTaxEntryFee();
						foreach (CusEntryHeader entryHeader in ActiveEntryHeaders)
						{
							if (entryHeader.IsFormalEntry)
							{
								ICusEntryHeader entry = entryHeader;
								result[CustomsDisbursementChargeCode.TotalDuty] += entry.TotalEstimatedDuty;
								result[CustomsDisbursementChargeCode.Excise] += entry.TotalEstimatedTax;
								result[CustomsDisbursementChargeCode.OtherOrFlatDutyAmount] += entry.TotalAntidumpingDuty + entry.TotalCountervailingDuty;

								foreach (CusEntryHeaderCharges charge in entryHeader.Charges.GetCharges())
								{
									if (!CusFeeCodeConstants.IsExciseTax(charge.C1_ChargeType))//cost into Excise
									{
										if (charge.C1_ChargeType == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)
										{
											result[CustomsDisbursementChargeCode.SpecialTax1] += charge.C1_ChargeAmount;
										}
										else if (charge.C1_ChargeType == Core.Constants.USCustoms.FeeCodes.HMF)
										{
											result[CustomsDisbursementChargeCode.SpecialTax2] += charge.C1_ChargeAmount;
										}
										else
										{
											result[CustomsDisbursementChargeCode.EntryFees] += charge.C1_ChargeAmount;
										}
									}
								}
							}
						}
						return result;
					});
				}
				return dutyTaxEntryFeeCached.Value;
			}
		}
		CachedProperty<DutyTaxEntryFee> dutyTaxEntryFeeCached;

		IComparer ILandedCostHeader.LineComparer
		{
			get { return new InvoiceLineComparer(); }
		}

		protected override bool IsEntryClearCore
		{
			get { return ActiveEntryHeaders.EntrySummaryEntry != null && ActiveEntryHeaders.EntrySummaryEntry.IsClearedEntry; }
		}

		#endregion

		public override object GetService(Type serviceType)
		{
			if (serviceType == typeof(ICustomsCharges))
			{
				if (IsReconMessageType)
				{
					var reconJob = ReconDeclaration ?? new ReconDeclaration(this);
					return reconJob.GetService(serviceType);
				}
				else
				{
					return new InterfaceImplementation.JobDeclarationCustomsCharges(this);
				}
			}
			return base.GetService(serviceType);
		}

		public ICodeDescriptionPairList DISDocumentIDList
		{
			get
			{
				if (disDocumentIDListCached == null)
				{
					var provider = ObjectFactory.Get<IUSDISDocumentIDsProvider>();
					disDocumentIDListCached = provider.GetDocumentIDList(this);
				}
				return disDocumentIDListCached;
			}
		}
		ICodeDescriptionPairList disDocumentIDListCached;

		public void RefreshDISDocumentIDList()
		{
			disDocumentIDListCached = null;
		}

		public bool CheckDISDocumentHasBeenAcceptedByFormType(ZString documentLabel)
		{
			var provider = ObjectFactory.Get<IUSDISDocumentIDsProvider>();
			return provider.HasBeenAccepted(this, documentLabel);
		}

		#region IInvoicesProvider Members

		ZBool IInvoicesProvider.ShouldElectronicInvoicesBeVisible
		{
			get { return IsFormalImport && (US_EnableAII || !IsPersistent); }
		}

		bool IInvoicesProvider.IsConsumptionFTZ
		{
			get { return this.IsConsumptionFTZ; }
		}

		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
		{
			return new DeclarationValueChangedAnnouncer(this);
		}

		bool IInvoicesProvider.IsExWarehouse
		{
			get { return IsExWarehouse; }
		}

		public ZGuid SelectedOriginalEntry => ZGuid.Empty;

		void IInvoicesProvider.AddDefaultInvoice()
		{
			if ((IsExport || IsDrawback) && Invoices.Count == 0)
			{
				Invoices.AddNew();
			}
		}

		#endregion

		#region IDutyDataLineHeaderProvider Members

		IEnumerable<IDutyDataLineHeader> IDutyDataLineHeaderProvider.EntriesToCalculateDutyFeeTax
		{
			get
			{
				foreach (CusEntryHeader entry in ActiveEntryHeaders)
				{
					if (entry.IsCustomsChargeToBeCalculated)
					{
						yield return entry;
					}
				}
			}
		}

		bool IDutyDataLineHeaderProvider.IsCustomsChargeRelevantForDecType(string code)
		{
			bool result = true;

			if (IsFTZAdmission)
			{
				result = code == Core.Constants.USCustoms.FeeCodes.HMF && !IsTemporaryDeposit;
			}

			return result;
		}

		ZDecimal? IDutyDataLineHeaderProvider.OverridenTotalMPFPayable
		{
			get
			{
				if (IsConsolidatedMonthlyFilingOverPipeline)
				{
					return US_PayableMPF;
				}

				return null;
			}
		}

		public bool IsConsolidatedMonthlyFilingOverPipeline
		{
			get { return IsImport && IsFixedTransportInstallations && US_MonthlyFiling; }
		}

		#endregion

		#region IDispositionCodeDateParent Members

		CodeDescriptionPairList IDispositionCodeDateParent.DispositionCodeDescriptionList
		{
			get
			{
				var isACEDisposition = IsACECargoCertificationMode || CanHavePGAFDA;
				var prefix = IsFTZAdmission ? "FTZ" : (isACEDisposition ? "ACE" : "OTHER");

				return Factory.GetCachedValue("DispositionCodeDescriptionList" + prefix,
				delegate
				{
					var list = new CodeDescriptionPairList();
					if (IsFTZAdmission)
					{
						list = new DispositionList();
					}
					else if (isACEDisposition)
					{
						list = UniversalReferenceDataHelper.GetDispositionCodeDescriptionList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO60RecordDispCode);
					}
					else
					{
						list = new CargoReleaseProcessingResultList();
					}
					return list;
				});
			}
		}

		string IDispositionCodeDateParent.GetDispositionDescriptionBasedOnSource(ZString dispositionSource, ZString code)
		{
			return ((IDispositionCodeDateParent)this).DispositionCodeDescriptionList.GetDescriptionFromCode(code);
		}

		#endregion

		#region Import Entry Number

		public bool DoesImportEntryNumberNeedsToBeSpecified
		{
			get
			{
				ReloadImportEntryNumber();
				return (IsImport && !US_EntryFilerCode.IsEmpty && !IsImportByExternalBroker && !IsFTZAdmission
					&& (US_EnableENS || US_EnableCRL) && ImportEntryNumber.IsEmpty);
			}
		}

		public void ReloadImportEntryNumber()
		{
			if (IsInDatabase)
			{
				var oldValue = (ensEntryNumber == null || ensEntryNumber.IsDeleted) ? ZString.Empty : ensEntryNumber.CE_EntryNum;
				if (ensEntryNumber == null || ensEntryNumber.IsDeleted)
				{
					ensEntryNumber = CusEntryNumber.Load(this, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Core.Constants.CountryCodes.UnitedStates, true); // reload from db
				}
				else if (ensEntryNumber.IsInDatabase)
				{
					ensEntryNumber.Reload();
				}
				if (ensEntryNumber != null && !ensEntryNumber.IsDeleted && ensEntryNumber.CE_EntryNum != oldValue)
				{
					DecEntryNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		public void ReloadExistingDataRelatedToImportEntryNumberAllocation()
		{
			if (IsInDatabase)
			{
				ReloadImportEntryNumber();
				if (ensEntryNumber != null && !ensEntryNumber.CE_EntryNum.IsEmpty)
				{
					var query = new ZQuery(CustomsEntryHeaders.CompleteFilter);
					query.ReLoadExistingRows = true;
					foreach (var entry in Factory.Load<CusEntryHeader>(query))
					{
						if (entry.IsRelatedToENSEntry || entry.IsFormalEntry)
						{
							if (entry.Logs is ILogsInternals logs)
							{
								logs.ReloadFromDB();
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// this entry number has been created to allow allocation of an entry number to a job before the entry is sent.
		/// This entry number is added to the CusEntryNum table, and when entry numbers are allocated as part of the saving of cusEntryHeader,
		/// the process first checks to see whether this number exists. If it does, then this will become the entry number for the entry.
		/// </summary>
		public string DisallowAllocateImportEntryNumber
		{
			get
			{
				var result = ACEEntryStmNumsSetting.GetAnyReasonForNotAbleToAllocateNumber(Branch, EntryFilerCode);

				if (string.IsNullOrEmpty(result))
				{
					foreach (CusEntryHeader entry in GetImportEntriesWithTransactionsWithCustoms())
					{
						if (!entry.EntryNumber.IsEmpty)
						{
							result = Constants.DisallowEntryNumberAllocation.EntryNumberAlreadyAllocated(entry.EntryNumber);
							break;
						}
					}
				}

				return result;
			}
		}

		public void AllocateEntryNumber(string userEnteredEntryNumber)
		{
			if (string.IsNullOrEmpty(userEnteredEntryNumber))
			{
				allocateEntryNumberOnSaving = true;
			}
			else
			{
				ImportEntryNumber = userEnteredEntryNumber;
				ENSEntryNumber.CE_EntryIsSystemGenerated = false;
			}
		}
		bool allocateEntryNumberOnSaving;

		[UniversalCopyValueOnlyProperty]
		public ZString InwardWarehouseEntryNumber
		{
			get
			{
				var result = ZString.Empty;
				if (IsENSFormalImport && EntryTypeList.IsWarehouseType(US_EntryType))
				{
					result = ImportEntryNumber;
				}
				return result;
			}
		}

		public ZString ImportEntryNumber
		{
			get { return !IsFTZAdmission && !IsExport && ENSEntryNumber != null ? ENSEntryNumber.CE_EntryNum : ZString.Empty; }
			set
			{
				if (ImportEntryNumber != value)
				{
					var ensEntryNumber = ENSEntryNumber ?? CreateNewENSCusEntryNum();
					ensEntryNumber.CE_EntryNum = value;
					ensEntryNumber.CE_EntryIsSystemGenerated = !IsImportByExternalBroker;
					SetMasterBillForSouthOriginatedTruckShipment();

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}

					UpdateConsolidatedEntryNoForReleaseEntry();
				}

				DecEntryNumberInfo.RefreshBinding();
			}
		}

		internal void UpdateConsolidatedEntryNoForReleaseEntry()
		{
			if (IsImport)
			{
				var entryNo = US_EntryFilerCode + DecEntryNumber;
				foreach (JobComInvoiceHeader invoice in Invoices)
				{
					if (!invoice.US_ReleaseEntryNumber.IsEmpty)
					{
						invoice.SetConsolidatedEntryNoForReleaseEntry(entryNo);
					}
				}
			}
		}

		public CusEntryNumber ENSEntryNumber
		{
			get
			{
				if (ensEntryNumber == null || ensEntryNumber.IsDeleted)
				{
					ensEntryNumber = CusEntryNumber.Load(this, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Core.Constants.CountryCodes.UnitedStates);
				}
				return ensEntryNumber;
			}
		}
		CusEntryNumber ensEntryNumber;

		CusEntryNumber CreateNewENSCusEntryNum()
		{
			ensEntryNumber = CusEntryNumber.New(this, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Core.Constants.CountryCodes.UnitedStates);
			return ensEntryNumber;
		}

		public CusEntryNumber INBEntryNumber
		{
			get
			{
				if (inbEntryNumber == null || inbEntryNumber.IsDeleted)
				{
					inbEntryNumber = CusEntryNumber.Load(this, CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates);
				}
				return inbEntryNumber;
			}
		}
		CusEntryNumber inbEntryNumber;

		public ZString FormattedEntryNumber
		{
			get { return CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(EntryFilerCode, ImportEntryNumber); }
		}

		#endregion

		#region IDocumentDataStateManager Members

		string IDocumentDataStateManager.DataStateErrorMessage
		{
			get { return dataStateErrorMessage; }
		}
		string dataStateErrorMessage;

		DocumentDataStateManagerResult IDocumentDataStateManager.Evaluate(IStmMenuItem commandAboutToBeRun)
		{
			isDataStateValid = true;
			dataStateErrorMessage = "";
			var result = DocumentDataStateManagerResult.NotApplicable;
			if (commandAboutToBeRun != null)
			{
				if (commandAboutToBeRun.SU_MenuName.Contains(DocumentNames.CustomsDeliveryOrder))
				{
					result = CheckDocumentState(DeliveryOrderHeaders.OfType<DeliveryOrderHeader>().Any(x => x.US_ShouldPrint), "No Customs Delivery Order has been marked for printing.");
				}
			}
			return result;
		}

		DocumentDataStateManagerResult CheckDocumentState(bool hasDocumentMarkForPrinting, string errorMessage)
		{
			var result = DocumentDataStateManagerResult.NotApplicable;
			if (!Globals.IsUserInteractive)
			{
				if (hasDocumentMarkForPrinting)
				{
					result = DocumentDataStateManagerResult.Pass;
				}
				else
				{
					isDataStateValid = false;
					dataStateErrorMessage = errorMessage;
					result = DocumentDataStateManagerResult.Fail;
				}
			}
			return result;
		}

		bool IDocumentDataStateManager.IsDataStateValid
		{
			get { return isDataStateValid; }
		}
		bool isDataStateValid;

		#endregion

		#region IPPQForm368NoticeOfArrivalSupportable Members

		void IPPQForm368NoticeOfArrivalSupportable.UpdateDefaultData(PPQForm368NoticeOfArrivalData noticeOfArrivalData)
		{
			noticeOfArrivalData.US_PPQForm368Box13A = PPQForm368MarksAndBills.Left(noticeOfArrivalData.US_PPQForm368Box13AInfo.MaxLength);
			noticeOfArrivalData.US_PPQForm368Box13B = PPQForm368QuantityAndNetWeight.Left(noticeOfArrivalData.US_PPQForm368Box13BInfo.MaxLength);
			noticeOfArrivalData.US_PPQForm368Box13C = PPQForm368Commodity.Left(noticeOfArrivalData.US_PPQForm368Box13CInfo.MaxLength);
		}

		public ZString PPQForm368MarksAndBills
		{
			get
			{
				var builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(JE_MarksAndNumbers);

				foreach (Bill bill in Bills)
				{
					builder.AppendIfNotEmpty(bill.CU_BillTypeAndNum);
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString PPQForm368QuantityAndNetWeight
		{
			get
			{
				var builder = new ZStringBuilder();
				if (!JE_TotalNoOfPacks.IsEmpty)
				{
					builder.Append(JE_TotalNoOfPacks + " " + Lookups.JE_TotalNoOfPacksPackType_List.GetDescriptionFromCode(JE_TotalNoOfPacksPackType));
				}

				var netWeightFromInvoiceLines = PPQForm368NetWeight;
				if (!netWeightFromInvoiceLines.IsEmpty)
				{
					builder.Append(netWeightFromInvoiceLines.ToString());
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString PPQForm368Commodity
		{
			get
			{
				var builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(JE_GoodsDescription);

				foreach (CusContainer container in CusContainers)
				{
					builder.Append(container.CO_ContainerNumber);
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		ZWeight PPQForm368NetWeight
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					if (!invoiceLine.JI_NetWeight.IsEmpty)
					{
						result += invoiceLine.NetWeightInKG;
					}
				}
				return new ZWeight(result, Core.Constants.Weight.Kilograms);
			}
		}

		public ZString US_PPQForm368Box13A
		{
			get { return PPQForm368Data != null ? PPQForm368Data.US_PPQForm368Box13A : ZString.Empty; }
			set
			{
				if (!value.IsEmpty && PPQForm368Data == null)
				{
					fPPQForm368Data = CreatePPQForm368Data();
					RegisterEditableChildObject(fPPQForm368Data);
				}

				if (PPQForm368Data != null)
				{
					PPQForm368Data.US_PPQForm368Box13A = value;
				}
			}
		}

		public ZString US_PPQForm368Box13B
		{
			get { return PPQForm368Data != null ? PPQForm368Data.US_PPQForm368Box13B : ZString.Empty; }
			set
			{
				if (!value.IsEmpty && PPQForm368Data == null)
				{
					fPPQForm368Data = CreatePPQForm368Data();
					RegisterEditableChildObject(fPPQForm368Data);
				}

				if (PPQForm368Data != null)
				{
					PPQForm368Data.US_PPQForm368Box13B = value;
				}
			}
		}

		public ZString US_PPQForm368Box13C
		{
			get { return PPQForm368Data != null ? PPQForm368Data.US_PPQForm368Box13C : ZString.Empty; }
			set
			{
				if (!value.IsEmpty && PPQForm368Data == null)
				{
					fPPQForm368Data = CreatePPQForm368Data();
					RegisterEditableChildObject(fPPQForm368Data);
				}

				if (PPQForm368Data != null)
				{
					PPQForm368Data.US_PPQForm368Box13C = value;
				}
			}
		}

		//This is used by a document menu filter to hide a legacy PPQ form. Name change needs to be reflected in menu filters and ForwardingShipment.
		public ZBool IsPPQForm368Box13Compatible
		{
			get { return !US_PPQForm368Box13A.IsEmpty || !US_PPQForm368Box13B.IsEmpty || !US_PPQForm368Box13C.IsEmpty; }
		}

		public ZBool IsPGARecapDocumentToBeShown
		{
			get { return IsACECargoCertificationMode; }
		}

		public ZBool IsProofOfReleaseDocumentToBeShown
		{
			get { return IsACECargoCertificationMode; }
		}

		PPQForm368Data PPQForm368Data
		{
			get
			{
				if (fPPQForm368Data == null || fPPQForm368Data.IsDeleted || fPPQForm368Data.Parent != this)
				{
					if (fPPQForm368Data != null)
					{
						UnRegisterEditableChildObject(fPPQForm368Data);
					}
					fPPQForm368Data = LoadPPQForm368Data();

					if (fPPQForm368Data != null)
					{
						RegisterEditableChildObject(fPPQForm368Data);
					}
				}
				return fPPQForm368Data;
			}
		}
		PPQForm368Data fPPQForm368Data;

		PPQForm368Data LoadPPQForm368Data()
		{
			ZQuery query = new ZQuery(CusAddInfoSchema.B7_ParentID, PK);
			query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, TablePrefix);
			query.AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USPPQForm368Data);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			return Factory.LoadTop1<PPQForm368Data>(query);
		}

		PPQForm368Data CreatePPQForm368Data()
		{
			var result = Factory.New<PPQForm368Data>();
			result.B7_ParentID = PK;
			return result;
		}

		#endregion

		#region Status

		#region 7501 Status

		#region ENSStatusDate

		public ZDateTime ENSStatusDate
		{
			get { return EntryStatusesAndErrors.ENSStatusDate; }
		}

		public ZPropertyInfo ENSStatusDateInfo
		{
			get { return GetZPropertyInfo(nameof(ENSStatusDate)); }
		}

		#endregion

		public ErrorsRecordCollection ENSERecords
		{
			get { return EntryStatusesAndErrors.ENSERecords; }
		}

		public ErrorsRecordCollection ENSE0Records
		{
			get { return EntryStatusesAndErrors.ENSE0Records; }
		}

		public ZString ENSTransmitCount
		{
			get { return EntryStatusesAndErrors.ENSTransmitCount; }
		}

		public ZString ENSRejectCount
		{
			get { return EntryStatusesAndErrors.ENSRejectCount; }
		}

		#region ENSErrorsExist

		public ZString ENSErrorsExist
		{
			get { return EntryStatusesAndErrors.ENSErrorsExist; }
		}

		public ZPropertyInfo ENSErrorsExistInfo
		{
			get { return GetZPropertyInfo(nameof(ENSErrorsExist)); }
		}

		#endregion

		public ErrorsRecordCollection ENSStatusNotifications
		{
			get { return EntryStatusesAndErrors.ENSStatusNotifications; }
		}

		public ZString ENSStatusNotificationsReqFurtherActions
		{
			get { return EntryStatusesAndErrors.DoesENSStatusNotificationExistRequiringActions ? RequiresActions7501Msg : ZString.Empty; }
		}

		internal ZString RequiresActions7501Msg
		{
			get { return Declaration.IsACE || Declaration.IsRecon ? "Ent Sum Requires Actions" : "7501 Requires Actions"; }
		}

		internal void MarkENSStatusElectronicInvoicingActionCompletedIfNecessary()
		{
			if (EntryStatusesAndErrors.DoesENSStatusNotificationExistRequiringActions)
			{
				bool? isAllClear = null;

				foreach (JobComInvoiceHeader invoice in Invoices)
				{
					isAllClear = invoice.HasAIIBeenLodgedAtCustoms;
					if (!isAllClear.Value)
					{
						break;
					}
				}

				if (isAllClear.HasValue && isAllClear.Value)
				{
					EntryStatusesAndErrors.MarkENSStatusElectronicInvoicingActionCompleted();
				}
			}
		}

		public ZString IncompleteDispositionsCode
		{
			get
			{
				if (dispositionsCodes == null)
				{
					dispositionsCodes = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;
						var incompleteUCMessages = ENSStatusNotifications.Cast<ErrorsRecord>().Where(x => x.ReferenceOnAction == EM_ActionStatusList.Codes.Incomplete).Select(x => x.DispositionCode).Distinct().Take(2).ToArray();
						if (incompleteUCMessages.Length > 1)
						{
							result = ImportEntryStatusList.Codes.MUL;
						}
						else if (incompleteUCMessages.Length == 1)
						{
							result = incompleteUCMessages.FirstOrDefault();
						}
						return result;
					});
				}
				return dispositionsCodes.Value;
			}
		}
		CachedProperty<ZString> dispositionsCodes;

		public ZString IncompleteDispositionsDescription
		{
			get
			{
				if (dispositionsDescription == null)
				{
					dispositionsDescription = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;
						if (IncompleteDispositionsCode == ImportEntryStatusList.Codes.MUL)
						{
							result = MulDispositionDescription;
						}
						else
						{
							result = ENSStatusDispositionCodeListLoader.GetENSStatusDispositionCodeList(Factory).GetDescriptionFromCode(IncompleteDispositionsCode);
						}
						return result;
					});
				}
				return dispositionsDescription.Value;
			}
		}

		CachedProperty<ZString> dispositionsDescription;
		internal const string MulDispositionDescription = "Multiple Incomplete Dispositions Description-See 'Status' tab";

		#endregion

		#region CRL Status

		#region CRLMessageStatusDate

		public ZDateTime CRLMessageStatusDate
		{
			get { return EntryStatusesAndErrors.CRLMessageStatusDate; }
		}

		public ZPropertyInfo CRLMessageStatusDateInfo
		{
			get { return GetZPropertyInfo(nameof(CRLMessageStatusDate)); }
		}

		public ErrorsRecordCollection CargoReleaseRecords
		{
			get { return EntryStatusesAndErrors.CargoReleaseRecords; }
		}

		public ErrorsRecordCollection COTariffRecords
		{
			get { return EntryStatusesAndErrors.COTariffRecords; }
		}

		public ErrorsRecordCollection ACECargoRelReferenceData
		{
			get { return EntryStatusesAndErrors.ACECargoRelReferenceData; }
		}

		public ZString CRInformationExists
		{
			get
			{
				if (!crInformationExists.HasValue)
				{
					crInformationExists = ACECargoRelReferenceData.Cast<ErrorsRecord>().Any(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.CMT || x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.RSN) ? crInformationExistsText : "";
				}
				return crInformationExists.Value;
			}
		}
		ZString? crInformationExists;

		internal const string crInformationExistsText = "CR Information Exists";

		public ZString CRLTransmitCount
		{
			get { return EntryStatusesAndErrors.CRLTransmitCount; }
		}

		public ZString CRLRejectCount
		{
			get { return EntryStatusesAndErrors.CRLRejectCount; }
		}

		#endregion

		#region CRLErrorsExist

		public ZString CRLErrorsExist
		{
			get { return EntryStatusesAndErrors.CRLErrorsExist; }
		}

		public ZPropertyInfo CRLErrorsExistInfo
		{
			get { return GetZPropertyInfo(nameof(CRLErrorsExist)); }
		}

		#endregion

		#endregion

		#region FDA Msg Status

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.FDAStatusList))]
		[ReadOnly(true)]
		[MaxLength(GenAddOnColumnMaxLength.FDAMsgStatus)]
		public ZString FDAMsgStatus
		{
			get { return AddOnFDAMsgStatus != null ? AddOnFDAMsgStatus.XA_Data : ZString.Empty; }
			set { AddOnColumnStatus(AddOnFDAMsgStatus, Constants.GenAddOnColumnFieldName.FDAMsgStatus, value, FDAMsgStatusInfo); }
		}

		GenAddOnColumn AddOnFDAMsgStatus
		{
			get
			{
				if (addOnFDAMsgStatus == null)
				{
					addOnFDAMsgStatus = new CachedProperty<GenAddOnColumn>(Factory, delegate
					{
						return GetAddOnStatus(Constants.GenAddOnColumnFieldName.FDAMsgStatus);
					});
				}
				return addOnFDAMsgStatus.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnFDAMsgStatus;

		public ZPropertyInfo FDAMsgStatusInfo
		{
			get { return GetZPropertyInfo(Schema.FDAMsgStatus); }
		}

		internal void UpdateFDAMsgStatus(int noOfNewFDADeclaredLines)
		{
			if (noOfFDALines.HasValue)
			{
				noOfFDALines += noOfNewFDADeclaredLines;
			}
			else
			{
				noOfFDALines = InvoiceLines.Cast<JobComInvoiceLine>().Count(x => x.IsFDADeclared);
			}
			var newFDAStatus = noOfFDALines > 0 ? FDAStatusList.Codes.REQ : FDAStatusList.Codes.NR;
			if (FDAMsgStatus != newFDAStatus)
			{
				using (((ISingleElementListInternal)this).SuspendListChanged())
				{
					FDAMsgStatus = newFDAStatus;
				}
			}
		}
		int? noOfFDALines;

		#endregion

		#region FDA Status

		[MaxLength(GenAddOnColumnMaxLength.FDAStatus)]
		public ZString FDAStatus
		{
			get { return AddOnFDAStatus != null ? AddOnFDAStatus.XA_Data : ZString.Empty; }
			set { AddOnColumnStatus(AddOnFDAStatus, Constants.GenAddOnColumnFieldName.FDAStatus, value, FDAStatusInfo); }
		}

		GenAddOnColumn AddOnFDAStatus
		{
			get
			{
				if (addOnFDAStatus == null)
				{
					addOnFDAStatus = new CachedProperty<GenAddOnColumn>(Factory, delegate
					{
						return GetAddOnStatus(Constants.GenAddOnColumnFieldName.FDAStatus);
					});
				}
				return addOnFDAStatus.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnFDAStatus;

		public ZPropertyInfo FDAStatusInfo
		{
			get { return GetZPropertyInfo(Schema.FDAStatus); }
		}

		public ZString FDAStatusDescription
		{
			get { return Factory.GetCachedValue<FDAEntryLevelDispositionCodeList>().GetDescriptionFromCode(FDAStatus); }
		}

		#endregion

		#region BLU Msg Status

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BLUStatusList))]
		[MaxLength(GenAddOnColumnMaxLength.BLUStatus)]
		public ZString BLUStatus
		{
			get { return AddOnBLUStatus != null ? AddOnBLUStatus.XA_Data : ZString.Empty; }
			set { AddOnColumnStatus(AddOnBLUStatus, Constants.GenAddOnColumnFieldName.BLUStatus, value, BLUStatusInfo); }
		}

		GenAddOnColumn AddOnBLUStatus
		{
			get
			{
				if (addOnBLUStatus == null)
				{
					addOnBLUStatus = new CachedProperty<GenAddOnColumn>(Factory, delegate
					{
						return GetAddOnStatus(Constants.GenAddOnColumnFieldName.BLUStatus);
					});
				}
				return addOnBLUStatus.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnBLUStatus;

		public ZPropertyInfo BLUStatusInfo
		{
			get { return GetZPropertyInfo(Schema.BLUStatus); }
		}

		public bool BLUStatus_ReadOnly
		{
			get { return true; }
		}

		public ZDateTime BLUMessageStatusDate
		{
			get { return EntryStatusesAndErrors.BLUMessageStatusDate; }
		}

		public ZPropertyInfo BLUMessageStatusDateInfo
		{
			get { return GetZPropertyInfo(nameof(BLUMessageStatusDate)); }
		}

		#endregion

		#region AII Status

		#region AIIRequested

		[MaxLength(100)]
		public ZString AIIRequested
		{
			get { return EntryStatusesAndErrors.AIIRequested; }
		}

		public ZPropertyInfo AIIRequestedInfo
		{
			get { return GetZPropertyInfo(nameof(AIIRequested)); }
		}

		#endregion

		#region AIIRejected

		[MaxLength(100)]
		public ZString AIIRejectedReason
		{
			get { return EntryStatusesAndErrors.AIIRejectedReason; }
		}

		public ZPropertyInfo AIIRejectedReasonInfo
		{
			get { return GetZPropertyInfo(nameof(AIIRejectedReason)); }
		}

		#endregion

		#region AIIUDate

		public ZDateTime AIIUDate
		{
			get { return EntryStatusesAndErrors.AIIUDate; }
		}

		public ZPropertyInfo AIIUDateInfo
		{
			get { return GetZPropertyInfo(nameof(AIIUDate)); }
		}

		#endregion

		#region AIIURecords

		public AIIURecordCollection AIIURecords
		{
			get { return EntryStatusesAndErrors.AIIURecords; }
		}

		public ZString AIIErrorsExist
		{
			get { return EntryStatusesAndErrors.AIIErrorsExist; }
		}

		public ZPropertyInfo AIIErrorsExistInfo
		{
			get { return GetZPropertyInfo(nameof(AIIErrorsExist)); }
		}

		#endregion

		public override ZBool US_IsAIIRequested
		{
			get { return base.US_IsAIIRequested; }
			set
			{
				if (!IsCopying)
				{
					base.US_IsAIIRequested = value;
				}
			}
		}

		#endregion

		#region OGA Status

		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public OGADispositionDataCollection OGADispositionCodes
		{
			get
			{
				if (fOGADispositionCodes == null)
				{
					fOGADispositionCodes = new OGADispositionDataCollection(this);
					fOGADispositionCodes.Load();
					if (fOGADispositionCodes.Count == 0 && LastCRLProcessingResultsMessage != null && LastCRLProcessingResultsMessage.EM_MessageType != ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus && LastCRLProcessingResultsMessage.EM_MessageType != ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)
					{
						EntryStatusesAndErrors.UpdateOGADispositionCodesFromMessage(LastCRLProcessingResultsMessage);
					}
				}

				return fOGADispositionCodes;
			}
		}
		OGADispositionDataCollection fOGADispositionCodes;

		[ChildEditable]
		public CusDispositionCollection EntryPGACusDispositions
		{
			get
			{
				if (fCusDisposition == null)
				{
					fCusDisposition = new CusDispositionCollection(this);
					fCusDisposition.Load();
					RegisterEditableChildObject(fCusDisposition);
				}
				return fCusDisposition;
			}
		}
		CusDispositionCollection fCusDisposition;

		public ZString PGAStatus
		{
			get
			{
				if (!pgaStatus.HasValue)
				{
					var result = new ZStringBuilder();
					foreach (var entryPgaDispositionDataCode in EntryPGACusDispositions.Cast<CusDisposition>())
					{
						result.Append(ZString.Format("{0} - {1}", entryPgaDispositionDataCode.CDI_StatusKey, entryPgaDispositionDataCode.CDI_Status));
					}
					pgaStatus = result.ToStringWithDelimiterBetweenAppends(";");
				}
				return pgaStatus.Value;
			}
		}
		ZString? pgaStatus;

		public ZString PGAStatusDesc
		{
			get
			{
				if (!pgaStatusDesc.HasValue)
				{
					var result = new ZStringBuilder();
					foreach (var entryPgaDispositionDataCode in EntryPGACusDispositions.Cast<CusDisposition>())
					{
						result.Append(ZString.Format("{0} - {1}", entryPgaDispositionDataCode.CDI_StatusKey, Lookups.DispositionCodeList.GetDescriptionFromCode(entryPgaDispositionDataCode.CDI_Status)));
					}
					pgaStatusDesc = result.ToStringWithDelimiterBetweenAppends(";");
				}
				return pgaStatusDesc.Value;
			}
		}
		ZString? pgaStatusDesc;

		MQEDIMessage LastCRLProcessingResultsMessage
		{
			get
			{
				if (crlProcessingResultsMessage == null)
				{
					crlProcessingResultsMessage = new CachedProperty<MQEDIMessage>(Factory,
						delegate
						{
							List<MQEDIMessage> messages = new List<MQEDIMessage>();
							if (ActiveEntryHeaders.SimplifiedEntry != null)
							{
								var messageSO = (MQEDIMessage)ActiveEntryHeaders.SimplifiedEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus, EDIMessage.Direction.Receive);
								if (messageSO != null)
								{
									messages.Add(messageSO);
								}
							}

							var messageRR = (MQEDIMessage)Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults, EDIMessage.Direction.Receive);
							if (messageRR != null)
							{
								messages.Add(messageRR);
							}

							if (FormalEntry != null)
							{
								var messageC1 = (MQEDIMessage)FormalEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, EDIMessage.Direction.Receive);
								if (messageC1 != null)
								{
									messages.Add(messageC1);
								}
							}

							return messages.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
						});
				}
				return crlProcessingResultsMessage.Value;
			}
		}
		CachedProperty<MQEDIMessage> crlProcessingResultsMessage;

		MQEDIMessage LastAcceptedENSMessage
		{
			get
			{
				MQEDIMessage result = null;
				var entryHeader = FormalEntry;
				if (entryHeader != null && !entryHeader.HasBeenWithdrawn)
				{
					var lastResponseMessage = entryHeader.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse }, EDIMessage.Direction.Receive)
							.Cast<MQEDIMessage>()
							.Where(x => x.IsENSCleared)
							.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();

					if (lastResponseMessage != null)
					{
						result = lastResponseMessage.OriginalMessage;
					}
				}

				return result;
			}
		}

		internal ZString LastAcceptedEntryType
		{
			get
			{
				var result = ZString.Empty;
				var lastAcceptedENSMessage = LastAcceptedENSMessage;
				if (lastAcceptedENSMessage != null)
				{
					var ens10 = lastAcceptedENSMessage.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
					result = ens10.EntryTypeCode;
				}
				return result;
			}
		}

		#region OGADispositionsExist

		public ZString OGADispositionsExist
		{
			get { return OGADispositionCodes.Count > 0 ? "OGA/PGA Dispositions Exist" : ""; }
		}

		public ZPropertyInfo OGADispositionsExistInfo
		{
			get { return GetZPropertyInfo(nameof(OGADispositionsExist)); }
		}

		#endregion

		#region COTariffExist

		public ZString COTariffExist
		{
			get { return EntryStatusesAndErrors.COTariffExist; }
		}

		public ZPropertyInfo COTariffExistInfo
		{
			get { return GetZPropertyInfo(nameof(COTariffExist)); }
		}

		#endregion

		#endregion

		#region BOL Update Status

		[List(nameof(Bills))]
		public ZString CurrentBillNumber
		{
			get { return currentBillNumber; }
			set
			{
				var oldValue = CurrentBillNumber;
				currentBillNumber = value;
				if (oldValue != CurrentBillNumber && !IsCopying)
				{
					ReLoadBLUL7Records();
				}
			}
		}
		ZString currentBillNumber;

		public bool CurrentBillNumber_ReadOnly
		{
			get { return !IsACECargoCertificationMode; }
		}

		void ReLoadBLUL7Records()
		{
			BLUL7Records.RemoveAll();
			var billNumber = Declaration.CurrentBillNumber;
			var bill = Declaration.Bills.FindByBillUniqueCode(billNumber);
			var matchBillPk = billNumber.IsEmpty || bill == null ? ZGuid.Empty : bill.PK;

			var matchedBills = Declaration.Bills.Cast<Bill>().Where(x => matchBillPk.IsEmpty || x.PK == matchBillPk);
			foreach (var matchedBill in matchedBills)
			{
				if (matchedBill != null && matchedBill.DispositionCodes.Count > 0)
				{
					BLUL7Records.AddRange(EntryStatusesAndErrors.LoadBillErrorsRecord(matchedBill));
				}
			}

			BLUL7Records.RefreshBinding();
		}

		public ErrorsRecordCollection BLUL7Records
		{
			get { return EntryStatusesAndErrors.BLUL7Records; }
		}

		public ZString BLUErrorsExist
		{
			get { return EntryStatusesAndErrors.BLUErrorsExist; }
		}

		public ZPropertyInfo BLUErrorsExistInfo
		{
			get { return GetZPropertyInfo(nameof(BLUErrorsExist)); }
		}

		#endregion

		#region IT Status

		public DeclarationEntriesStatusesAndErrorCollections EntryStatusesAndErrors
		{
			get { return fEntryStatusesAndErrors ?? (fEntryStatusesAndErrors = new DeclarationEntriesStatusesAndErrorCollections(Declaration)); }
		}
		DeclarationEntriesStatusesAndErrorCollections fEntryStatusesAndErrors;

		#region ITStatusDate

		public ZDateTime ITStatusDate
		{
			get { return EntryStatusesAndErrors.ITStatusDate; }
		}

		public ZPropertyInfo ITStatusDateInfo
		{
			get { return GetZPropertyInfo(nameof(ITStatusDate)); }
		}

		#endregion

		#region ITErrorRecords

		public ErrorsRecordCollection ITQT95Records
		{
			get { return EntryStatusesAndErrors.ITQT95Records; }
		}

		public ErrorsRecordCollection ITWT95ExportRecords
		{
			get { return EntryStatusesAndErrors.ITWT95ExportRecords; }
		}

		public ErrorsRecordCollection ITWT95ArrivalRecords
		{
			get { return EntryStatusesAndErrors.ITWT95ArrivalRecords; }
		}

		public ErrorsRecordCollection ITWT95TOLRecords
		{
			get { return EntryStatusesAndErrors.ITWT95TOLRecords; }
		}

		#endregion

		#region ITErrorsExist

		public ZString ITErrorsExist
		{
			get
			{
				ZString iTErrorsLabel = ZString.Empty;
				if (ITQT95Records == null || ITWT95ExportRecords == null)
				{
					EntryStatusesAndErrors.SplitITErrors();
				}

				if (EntryStatusesAndErrors.ITErrorResponsesExist)
				{
					iTErrorsLabel = "IT Errors Exist";
				}

				return iTErrorsLabel;
			}
		}

		public ZPropertyInfo ITErrorsExistInfo
		{
			get { return GetZPropertyInfo(nameof(ITErrorsExist)); }
		}

		#endregion

		#endregion

		#region Liquidation Status

		public ZDateTime LiquidationDate
		{
			get { return Liquidations.GetMostRecentLiquidationDate(); }
		}

		public ZString LiquidationType
		{
			get { return Liquidations.GetMostRecentLiquidationType(); }
		}

		#endregion

		#region AES Disposition

		public ZString AESResponseCode => string.Join(", ", MostRecentAESDispositions.Select(x => x.CDI_Status));

		public ZString AESResponseCodeDescription => HasMultipleAESDispositions ? AES.AESConstants.Multiple : GetAESDispositionCodeDesc(AESResponseCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AESResponseCode);

		public ZString AESSeverity => string.Join(", ", MostRecentAESDispositions.Select(x => x.CDI_StatusKey));

		public ZString AESSeverityDescription => HasMultipleAESDispositions ? AES.AESConstants.Multiple : GetAESDispositionCodeDesc(AESSeverity, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AESSeverityIndicator);

		bool HasMultipleAESDispositions => MostRecentAESDispositions.Count > 1;

		List<CusDisposition> MostRecentAESDispositions
		{
			get
			{
				return fMostRecentAESDispositions ??= ActiveEntryHeaders.Cast<CusEntryHeader>()
					.Where(x => !x.AESCusDispositions.IsNullOrEmpty()).Select(x => x.AESCusDispositions.Cast<CusDisposition>().OrderByDescending(y => y.CDI_SystemCreateTimeUtc).FirstOrDefault()).ToList();
			}
		}
		List<CusDisposition> fMostRecentAESDispositions;

		ZString GetAESDispositionCodeDesc(ZString code, ZString codeType)
		{
			return Factory.GetCachedValue(ZString.Format("USJobDeclarationAESDispositionCodeDesc{0}_{1}", codeType, code), () =>
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, code, Core.Constants.CountryCodes.UnitedStates, codeType, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;
			});
		}

		#endregion

		#endregion

		#region IBondDetailsDefault Members

		IDisposable IBondDetailsDefault.SuspendAddInfoPropertySetting
		{
			get { return SuspendAddInfoPropertySetting(); }
		}

		ZBool IBondDetailsDefault.IsReconMessageType
		{
			get { return IsReconMessageType; }
		}

		OrgHeaderWrapper IBondDetailsDefault.IORWrapper
		{
			get { return IORWrapper; }
		}

		ZString IBondDetailsDefault.ActivityCode
		{
			get { return IsDrawback ? ActivityCodeList.Codes._1a : ActivityCodeList.Codes._1; }
		}

		ZDateTime IBondDetailsDefault.EffectiveDate
		{
			get { return EffectiveDateForBond; }
		}

		ZString IBondDetailsDefault.EntryType
		{
			get { return US_EntryType; }
		}

		ZDecimal IBondDetailsDefault.US_BondAmount
		{
			set { US_BondAmount = value; }
		}

		ZString IBondDetailsDefault.US_BondType
		{
			get { return US_BondType; }
			set { US_BondType = value; }
		}

		ZString IBondDetailsDefault.US_BondProducerAccNo
		{
			set { US_BondProducerAccNo = value; }
		}

		ZString IBondDetailsDefault.US_SuretyCode
		{
			set { US_SuretyCode = value; }
		}

		#endregion

		#region IPrelimStatementDetailsDefault Members

		bool IPrelimStatementDetailsDefault.FixPSD
		{
			get { return US_FixPSD; }
		}

		bool IPrelimStatementDetailsDefault.RegistryAllowsDefaulting
		{
			get { return USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty).DoDefaultPrelimStatementPrintDate; }
		}

		ZDate IPrelimStatementDetailsDefault.BaseDateToCalculateOn
		{
			get { return GetBaseDateToCalculatePSDOn(); }
		}

		int IPrelimStatementDetailsDefault.DaysToAddToStatementDate
		{
			get
			{
				var statementDetails = (IPrelimStatementDetailsDefault)this;
				return new AddInfoJobDeclarationWorkingDate().DaysToAddToStatementDate(statementDetails.Branch.Company.PK, statementDetails.Branch.PK, IOR);
			}
		}

		GlbBranch IPrelimStatementDetailsDefault.Branch
		{
			get { return Branch ?? Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK); }
		}

		bool IPrelimStatementDetailsDefault.ShouldValidatePastDate
		{
			get { return !US_PaymentDate.IsValid; }
		}

		string IPrelimStatementDetailsDefault.DatePrecedenceMessage
		{
			get { return PrelimStatementPrintDateValidator.FormalDeclarationPrecedence; }
		}

		ZDate GetBaseDateToCalculatePSDOn()
		{
			var result = ZDateTime.Today;

			ZDateTime releaseDate;
			if (US_ConsolACE && GetPSDFromEarliestReleasedEntry() is JobDeclaration earliestEntry)
			{
				releaseDate = earliestEntry.JE_EntryAuthorisationDate;
			}
			else
			{
				releaseDate = JE_EntryAuthorisationDate;
			}

			if (US_PresentationDate.IsValid || US_EstimatedEntryDate.IsValid || releaseDate.IsValid)
			{
				if (US_PresentationDate.IsValid && !releaseDate.IsValid)
				{
					result = US_PresentationDate;
				}
				else if (releaseDate.IsValid && (!IsLiveEntry || !US_EstimatedEntryDate.IsValid || releaseDate.Date < US_EstimatedEntryDate.Date))
				{
					result = releaseDate;
				}
				else if (US_EstimatedEntryDate.IsValid)
				{
					result = US_EstimatedEntryDate;
				}
			}
			else if (US_EntryDate.IsValid && US_EntryDate > result)
			{
				result = US_EntryDate;
			}
			return result.Date;
		}

		JobDeclaration GetPSDFromEarliestReleasedEntry()
		{
			JobDeclaration earliestEntry = null;
			foreach (JobComInvoiceHeader invoice in Declaration.Invoices)
			{
				var declaration = invoice.ReleaseDeclaration;
				if (declaration != null && (earliestEntry == null || declaration.JE_EntryAuthorisationDate < earliestEntry.JE_EntryAuthorisationDate))
				{
					earliestEntry = declaration;
				}
			}

			return earliestEntry;
		}

		#endregion

		#region IClientBranchDesignationDefault Members

		GlbBranch IClientBranchDesignationDefault.Branch
		{
			get { return Branch ?? Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK); }
		}

		#endregion

		#region IPriorNoticeHeader Members

		OrgHeader IPriorNoticeHeader.Submitter
		{
			get { return FDASubmitter; }
		}

		ZString IPriorNoticeHeader.PortOfArrival
		{
			get { return US_SchDArrival; }
		}

		ZDateTime IPriorNoticeHeader.DateOfArrival
		{
			get { return US_FDAADTA.Date; }
		}

		ZString IPriorNoticeHeader.AnticipatedPortOfCrossing
		{
			get { return US_FDAAPC; }
		}

		ZDateTime IPriorNoticeHeader.TimeOfArrival
		{
			get { return US_FDAADTA; }
		}

		ZString IPriorNoticeHeader.VoyageFlightNumber
		{
			get { return VoyageFlightNumberForMessaging; }
		}

		internal ZString VoyageFlightNumberForMessaging
		{
			get { return VoyageFlightNumber.Left(5); }
		}

		public ZString VoyageFlightNumber
		{
			get
			{
				ZString result = JE_VoyageFlightNo;

				if (IsAir)
				{
					if (FlightNoHasNumericAirlineCode)
					{
						result = result.SubstringSafe(2);
					}
					else
					{
						result = Regex.Replace(result, "^[a-z]{0,2}", "", RegexOptions.IgnoreCase);
					}
				}

				return result.KeepAlphanumericCharacters();
			}
		}

		ZString IPriorNoticeHeader.EntryNumberLast8Digits
		{
			get
			{
				string result = ImportEntryNumber;
				return string.IsNullOrEmpty(result) ? MQEDIMessage.USEntryNumberPlaceHolder : result;
			}
		}

		ZString IPriorNoticeHeader.EntryType
		{
			get { return US_EntryType; }
		}

		ZString IPriorNoticeHeader.ModeOfTransportationCode
		{
			get { return JE_Calc_USTransportMode; }
		}

		ZString IPriorNoticeHeader.LocationOfGoodsFIRMSCode
		{
			get { return US_US_NKLocationOfGoods; }
		}

		ZString IPriorNoticeHeader.FTZAdmissionNumber
		{
			get { return IsImport && IsConsumptionFTZ ? JE_MasterBill : ZString.Empty; }
		}

		ZString IPriorNoticeHeader.ImportingCarrierSCAC
		{
			get { return US_UI_NKCarrierSCAC; }
		}

		ZString IPriorNoticeHeader.ImportingAirCarrier3LetterCode
		{
			get
			{
				ZString result = "";

				if (Declaration.IsAir)
				{
					RefAirline airline = RefAirline.LoadFromAirline2LetterCode(Factory, Declaration.US_UI_NKCarrierSCAC);
					if (airline != null)
					{
						result = airline.RM_ThreeLetterCode;
					}
				}

				return result;
			}
		}

		ZString IPriorNoticeHeader.FDAMsgStatus
		{
			set { FDAMsgStatus = value; }
		}

		ZBool IPriorNoticeHeader.IsSurface
		{
			get { return !IsAir; }
		}

		ZString IPriorNoticeHeader.CarrierType
		{
			get { return US_FDACANType; }
		}

		ZString IPriorNoticeHeader.CarrierName
		{
			get { return US_FDACAN; }
		}

		ZString IPriorNoticeHeader.CarrierCountry
		{
			get { return US_FDACCN; }
		}

		IReadOnlyList<IPriorNoticeLine> IPriorNoticeHeader.StandalonePriorNoticeLines
		{
			get
			{
				if (priorNoticeLinesCached == null)
				{
					priorNoticeLinesCached = new CachedProperty<IPriorNoticeLine[]>(Factory, delegate
					{
						List<IPriorNoticeLine> result = new List<IPriorNoticeLine>();
						if (RequiresPriorNoticeReporting)
						{
							List<JobComInvoiceLine> invoiceLines = new List<JobComInvoiceLine>(InvoiceLines.ToArray<JobComInvoiceLine>());
							invoiceLines.Sort(new InvoiceLineComparer());
							foreach (JobComInvoiceLine invoiceLine in invoiceLines)
							{
								if (invoiceLine.HasFDAData)
								{
									List<IPriorNoticeLine> fdas = new List<IPriorNoticeLine>();
									fdas.AddRange(GetFDAsForStandalonePriorNoticeLines(invoiceLine));
									fdas.Sort(new Comparison<IPriorNoticeLine>((x, y) => x.FDALineNumber.CompareTo(y.FDALineNumber)));
									result.AddRange(fdas);
								}
							}
						}
						return result.ToArray();
					});
				}
				return priorNoticeLinesCached.Value;
			}
		}
		CachedProperty<IPriorNoticeLine[]> priorNoticeLinesCached;

		IEnumerable<FDA> GetFDAsForStandalonePriorNoticeLines(JobComInvoiceLine invoiceLine)
		{
			bool isRequired = invoiceLine.JI_FDARequirementCode == OGARequirementList.Codes.FD3 ||
				 invoiceLine.JI_FDARequirementCode == OGARequirementList.Codes.FD4;
			foreach (FDA fda in invoiceLine.FDAs)
			{
				if (fda.US_PNC.IsEmpty && !fda.US_PND)
				{
					if (fda.US_FDAForcePN || isRequired)
					{
						yield return fda;
					}
				}
			}
		}

		public ZString CarrierNameLabel
		{
			get
			{
				ZString result = "Carrier Name";

				if (US_FDACANType == FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle)
				{
					result = "Vehicle License";
				}
				else if (US_FDACANType == FDACarrierTypeList.Codes.PrivatelyOwnedFNVehicle)
				{
					result = "State/Province";
				}

				return result;
			}
		}

		public ZString CarrierCountryLabel
		{
			get
			{
				ZString result = "Country";

				if (US_FDACANType == FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle)
				{
					result = "State";
				}

				return result;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.FDACANTypes))]
		public override ZString US_FDACANType
		{
			get { return base.US_FDACANType; }
			set { base.US_FDACANType = value; }
		}

		#endregion

		#region IPriorNoticeProcessor Members

		FDA IPriorNoticeProcessor.GetOriginalFDALineToAddConfirmationNumber(ZInt fdaLineNumber, ZInt entryLineNumber)
		{
			if (entryLineNumber == ZInt.Zero) // Standalone Prior Notice
			{
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					foreach (FDA fda in invoiceLine.FDAs)
					{
						if (fda.US_PNC.IsEmpty && fda.US_FDALineNo == fdaLineNumber)
						{
							return fda;
						}
					}
				}
			}
			else
			{
				CusEntryHeader cusEntryHeader = GetEntryHeaderForFDA();
				if (cusEntryHeader != null)
				{
					var candidateCusEntryLineNumber = 0;
					foreach (CusEntryLine entryLine in cusEntryHeader.MergedLines)
					{
						candidateCusEntryLineNumber++;
						if (candidateCusEntryLineNumber == entryLineNumber)
						{
							foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
							{
								foreach (FDA fda in invoiceLine.FDAs)
								{
									if (fda.US_PNC.IsEmpty && fda.US_FDALineNo == fdaLineNumber)
									{
										return fda;
									}
								}
							}
						}
					}
				}
			}

			return null;
		}

		#endregion

		#region IFDACorrectionHeader Members

		IEnumerable<IFDAEntryLine> IFDACorrectionHeader.EntryLines
		{
			get
			{
				List<CusEntryLine> result = new List<CusEntryLine>();
				CusEntryHeader entryHeaderForFDACorrection = GetEntryHeaderForFDA();

				if (entryHeaderForFDACorrection != null)
				{
					result = new List<CusEntryLine>(new TypedEnumerable<CusEntryLine>(entryHeaderForFDACorrection.MergedLines));
					result.Sort(new CusEntryLineComparer());
				}

				return new TypedEnumerable<IFDAEntryLine>(result);
			}
		}

		internal CusEntryHeader GetEntryHeaderForFDA()
		{
			CusEntryHeader result = null;

			foreach (CusEntryHeader entryHeader in ActiveEntryHeaders)
			{
				if (entryHeader.IsFormalEntry)
				{
					result = entryHeader;
				}
				else if (entryHeader.IsCargoRelease || entryHeader.IsBorderCargoRelease)
				{
					result = entryHeader;
				}

				if (entryHeader.US_SentLatestFDA)
				{
					result = entryHeader;
					break;
				}
			}

			return result;
		}

		ZString IFDACorrectionHeader.ProcessingDistrictPort
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsRemoteLocationFiling)
				{
					result = US_PreparerDistrictPort;
				}
				else
				{
					result = ProcessingDistrictPort;
				}
				return result;
			}
		}

		#endregion

		#region IMessageFailStatusManager Members

		bool IMessageFailStatusManager.IsMessageTypeSupported(ZString messageType)
		{
			bool result = false;
			switch (messageType)
			{
				case ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles: // no status
				case ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery: // no status
				case ApplicationIdentifierCodeList.Codes.DrawbackSummary:
				case ACEApplicationIdentifierCodeList.Codes.ExtractReference:
				case ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQuery:
				case ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQuery:
				case ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQuery:
					result = true;
					break;
			}
			return result;
		}

		void IMessageFailStatusManager.SetFailStatus(MQEDIMessage message)
		{
			if (message.EM_MessageType == ApplicationIdentifierCodeList.Codes.DrawbackSummary || message.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQuery)
			{
				ZString status = new DrawbackSummaryMessageStatusCalculator().Calculate(message, ABIResponseStatus.Rejected);
				if (status != "")
				{
					JE_MessageStatus = status;
				}
			}
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> Integration.Customs.ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USDeliveryOrderHeader, typeof(DeliveryOrderHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USPPQForm368Data, typeof(PPQForm368Data));
			result.Add(CusAddInfoTypeAttribute.Codes.USITDoc, typeof(ITDoc));
			result.Add(CusAddInfoTypeAttribute.Codes.USOGADisposition, typeof(OGADispositionData));
			result.Add(CusAddInfoTypeAttribute.Codes.USDisposition, typeof(DispositionData));
			result.Add(CusAddInfoTypeAttribute.Codes.USLinkedEntry, typeof(LinkedEntry));
			result.Add(CusAddInfoTypeAttribute.Codes.USDeclarationFSISCertificate, typeof(USDeclarationFSISLine));
			result.Add(CusAddInfoTypeAttribute.Codes.USWHSPack, typeof(WHSPack));
			result.Add(CusAddInfoTypeAttribute.Codes.USWHSPackLine, typeof(WHSPackLine));
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.ContractNumber, typeof(ContractNumber));
			result.Add(CusCodeDataTypeList.Codes.ReconRefundedCharge, typeof(ReconRefundedCharge));
			return result;
		}

		#endregion

		#region IMessageResponseNotificator Members

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			var cusAgent = CusAgent;
			return cusAgent != null ? cusAgent.GS_EmailAddress : ZString.Empty;
		}

		#endregion

		#region ICustomsJobInfo Members

		ZGuid ICustomsJobInfo.CreditorPK
		{
			get
			{
				if (IsImportByExternalBroker)
				{
					return JE_OH_ExternalBroker;
				}
				else
				{
					OrgHeader org = Factory.Load<OrgHeader>(Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.GetFallBackValueAtAllLevels(Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty));
					return org == null ? ZGuid.Empty : org.PK;
				}
			}
		}

		#endregion

		#region ICBPEDIMessageMessageTextNumberPlaceHolderFiller Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		string ICBPEDIMessageMessageTextNumberPlaceHolderFiller.Fill(CBPEDIMessage message)
		{
			var information = string.Empty;
			if (IsDrawback)
			{
				FillInDrawbackDeclarationNumberIfRequired();

				var placeHolder = IsACEDrawback ? MQEDIMessage.USEntryNumberPlaceHolder : MQEDIMessage.USEntryFilerEntryNumberPlaceHolder;
				var entryNumber = IsACEDrawback ? (string)DeclarationNumber : US_EntryFilerCode + DeclarationNumber;
				information = string.Format(CultureInfo.InvariantCulture, "{0}Declaration is Drawback, and condition IsACEDrawback: {1}. ", information, IsACEDrawback.ToString());

				if (DeclarationNumber == ImportEntryNumber)
				{
					ENSEntryNumber.FillMessagePlaceHolder(message, placeHolder, entryNumber);
					information = string.Format(CultureInfo.InvariantCulture, "{0}Declaration Number is equal to Import Entry Number. ", information);
				}
				else
				{
					message.EM_MessageText = message.EM_MessageText.Replace(placeHolder, entryNumber);
					information = string.Format(CultureInfo.InvariantCulture, "{0}Place Holder {1} is replaced with Entry Number {2}. ", information, placeHolder, entryNumber);
				}
			}

			if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.FDAPriorNotice &&
				message.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability &&
				ImportEntryNumber.IsEmpty)
			{
				ReloadImportEntryNumber();
				if (ImportEntryNumber.IsEmpty)
				{
					AllocateNextImportEntryNumber();
					if (!ImportEntryNumber.IsEmpty)
					{
						ENSEntryNumber.FillMessagePlaceHolder(message, MQEDIMessage.USEntryNumberPlaceHolder, ImportEntryNumber);
						information = string.Format(CultureInfo.InvariantCulture, "{0}Message SubType is {1}, and Message Type is {2}, and Import Entry Number is empty. USEntryNumberPlaceHolder {3} is replaced with Import Entry Number {4}. ",
							information, EM_MessageSubTypeList.Codes.FDAPriorNotice, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability, MQEDIMessage.USEntryNumberPlaceHolder, ImportEntryNumber);
					}
				}
			}

			if ((message.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData || message.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.FTZEventReporting) && FTZControlNumber.IsEmpty && FTZControlNumberIsAutoAllocated)
			{
				ReloadFTZAdmissionNumber();
				if (FTZControlNumber.IsEmpty)
				{
					AllocateNextFTZControlNumberyNumber();
					information = string.Format(CultureInfo.InvariantCulture, "{0}Message Type is {1}, and FTZ Control Number is empty and auto allocated. ", information, message.EM_MessageType);
					if (!FTZControlNumber.IsEmpty)
					{
						message.EM_MessageText = message.EM_MessageText.Replace(MQEDIMessage.USFTZControlNumberPlaceHolder, FTZControlNumber);
						message.EM_MessageText = message.EM_MessageText.Replace(MQEDIMessage.USFTZAdmissionNumberPlaceHolder, FTZAdmissionNumberFormatted.PadRight(MQEDIMessage.USFTZAdmissionNumberPlaceHolder.Length));
						information = string.Format(CultureInfo.InvariantCulture, "{0}USFTZControlNumberPlaceHolder {1} is replaced with FTZ Contorl Number {2}, and USFTZAdmissionNumberPlaceHolder {3} is replaced with FTZAdmissionNumberFormatted {4}. ",
							information, MQEDIMessage.USFTZControlNumberPlaceHolder, FTZControlNumber, MQEDIMessage.USFTZAdmissionNumberPlaceHolder, FTZAdmissionNumberFormatted);
					}

					if (message.EM_MessageText.Contains(MQEDIMessage.USFTZControlNumberPlaceHolder, StringComparison.CurrentCultureIgnoreCase))
					{
						var fountain = IOR?.OrgFountains?.TryGetNumberFountainByZoneID(FTZZoneID, OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber);
						if (fountain == null && WarehouseDocAddress?.Address?.Header != null)
						{
							fountain = WarehouseDocAddress.Address.Header.OrgFountains?.TryGetNumberFountainByZoneID(FTZZoneID, OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse);
						}

						string msg = "{0} should be replaced by FTZControlNumber. FTZControlNumber: [{1}] CusEntryNumber: [{2}]. FTZControlNumberIsAutoAllocated:[{3}]. FTZZoneID:[{4}]. Importer of Record:[{5}]. Warehouse Address:[{6}]. Has Number Fountain:[{7}]";// Developer Error message when Place Holder is replaced by Control Number
						ErrorReporter.ReportOnce("FTZControlNumberContainsPlaceHoderDefaultValue", ZString.Format(msg, MQEDIMessage.USFTZControlNumberPlaceHolder, FTZControlNumber, FTZAdmissionNumber, FTZControlNumberIsAutoAllocated, FTZZoneID, IOR?.OH_Code ?? ZString.Empty, WarehouseDocAddress?.Address?.Header.OH_Code ?? ZString.Empty, fountain != null ? YesNoList.Codes.Yes : YesNoList.Codes.No)); // This is a key to a developer exception
					}
				}
			}
			return information;
		}

		#endregion

		#region IMessageActionHeader Members

		ZString IMessageActionHeader.InBondExportTransportMode
		{
			get { return TransportMode; }
		}

		#endregion

		#region ICurrencyConverterDataProvider

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get
			{
				if (IsExport)
				{
					return BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack;
				}
				else
				{
					return 0;
				}
			}
		}

		#endregion

		#region SendCustomsMessageMutex

		ZGlobalMutex SendCustomsMessageMutex
		{
			get { return fSendCustomsMessageMutex ?? (fSendCustomsMessageMutex = new ZGlobalMutex(MutexIDs.SendCustomsMessage, PK.ToString())); }
		}
		ZGlobalMutex fSendCustomsMessageMutex;

		public bool LockSendCustomsMessageMutex => SendCustomsMessageMutex.Lock();

		public void UnlockSendCustomsMessageMutex()
		{
			if (fSendCustomsMessageMutex != null && fSendCustomsMessageMutex.IsLocked && fSendCustomsMessageMutex.HasLock)
			{
				fSendCustomsMessageMutex.Unlock();
			}
		}

		public string GetSendCustomsMessageMutexInfo() => SendCustomsMessageMutex.GetMutexLockByInfo();
		#endregion

		#region FTZMutex

		ZGlobalMutex FTZAdmissionNumberAllocationMutex
		{
			get { return fFTZAdmissionNumberAllocationMutex ?? (fFTZAdmissionNumberAllocationMutex = new ZGlobalMutex(MutexIDs.FTZAdmissionNumberAllocation, "FTZ" + PK.ToString())); }
		}
		ZGlobalMutex fFTZAdmissionNumberAllocationMutex;

		public bool LockFTZAdmissionNumberAllocationMutex
		{
			get { return FTZAdmissionNumberAllocationMutex.IsLocked ? (bool)FTZAdmissionNumberAllocationMutex.HasLock : FTZAdmissionNumberAllocationMutex.Lock(); }
		}

		public void UnlockFTZAdmissionNumberAllocationMutex()
		{
			if (FTZAdmissionNumberAllocationMutex != null && FTZAdmissionNumberAllocationMutex.IsLocked && FTZAdmissionNumberAllocationMutex.HasLock)
			{
				FTZAdmissionNumberAllocationMutex.Unlock();
			}
		}

		public string GetFTZAdmissionNumberAllocationMutexInfo() => FTZAdmissionNumberAllocationMutex.GetMutexLockByInfo();

		protected override bool IsDeclarationMatchSpecificCountryCore(ZString countryCode) => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode) == Core.Constants.CountryCodes.UnitedStates;

		public override string TermNameForBondedWarehouse
		{
			get
			{
				return JE_MessageType == JobMessageTypeList.Codes.FTZ || (JE_MessageType == JobMessageTypeList.Codes.Import && US_EntryType == EntryTypeList.Codes.ConsumptionFTZ) ? Res.GetString("24F3F411-808D-4E53-9549-F80F0529EFE9", "Foreign Trade Zone") : base.TermNameForBondedWarehouse;
			}
		}
		#endregion

		#region FTZ

		public override ZBool US_F_DirectDelivery
		{
			get { return base.US_F_DirectDelivery; }
			set
			{
				base.US_F_DirectDelivery = value;

				if (US_F_IncludePTT_ReadOnly)
				{
					US_F_IncludePTT = false;
				}
			}
		}

		public ZBool IsFTZAdmission
		{
			get { return JE_MessageType == JobMessageTypeList.Codes.FTZ; }
		}

		internal ZBool IsODZ_AdmissionType
		{
			get { return IsFTZAdmission && FTZAdmissionTypeCodeList.IsODZ_AdmissionType(US_F_AdmissionType); }
		}

		internal ZBool IsOC_AdmissionType
		{
			get { return IsFTZAdmission && FTZAdmissionTypeCodeList.IsOC_AdmissionType(US_F_AdmissionType); }
		}

		internal ZBool IsRegularFTZAdmission
		{
			get { return IsFTZAdmission && US_F_AdmissionType == FTZAdmissionTypeCodeList.Codes.RegularAdmission; }
		}

		internal ZBool IsTemporaryDeposit
		{
			get { return IsFTZAdmission && US_F_AdmissionType == FTZAdmissionTypeCodeList.Codes.TemporaryDeposit; }
		}

		internal ZBool IsZoneToZoneFTZAdmission
		{
			get { return IsFTZAdmission && US_F_AdmissionType == FTZAdmissionTypeCodeList.Codes.ZoneToZone; }
		}

		#region FTZ Admission Number

		[BusinessObjectTestExclude]
		[MaxLength(Schema.FTZAdmissionNumberMaxLenth)]
		public ZString FTZAdmissionNumber
		{
			get { return IsFTZAdmission && FTZCusEntryNum != null ? FTZCusEntryNum.CE_EntryNum : ZString.Empty; }
			set
			{
				if (FTZAdmissionNumber != value)
				{
					if (FTZCusEntryNum == null)
					{
						CreateNewFTZControlNum();
					}

					FTZCusEntryNum.CE_EntryNum = value;
					FTZCusEntryNum.CE_EntryIsSystemGenerated = false;

					if (!IsValidationSuspended)
					{
						Validation.ValidateFTZAdmissionNumber();
					}

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				FTZAdmissionNumberInfo.RefreshBinding();
			}
		}

		public bool FTZAdmissionNumber_ReadOnly
		{
			get { return FTZAdmissionHasBeenLodgedAtCustoms; }
		}

		public ZPropertyInfo FTZAdmissionNumberInfo
		{
			get { return GetZPropertyInfo(Schema.FTZAdmissionNumber); }
		}

		public ZString FTZAdmissionNumberFormatted
		{
			get { return FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(FTZAdmissionNumber); }
		}

		internal CusEntryNumber FTZCusEntryNum
		{
			get
			{
				if (ftzCusEntryNum == null || ftzCusEntryNum.IsDeleted)
				{
					ftzCusEntryNum = CusEntryNumber.Load(this, CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone, Core.Constants.CountryCodes.UnitedStates);
				}
				return ftzCusEntryNum;
			}
		}
		CusEntryNumber ftzCusEntryNum;

		void CreateNewFTZControlNum()
		{
			ftzCusEntryNum = CusEntryNumber.New(this, CusEntryNumberTypes.UnitedStates.FTZ, Core.Constants.CountryCodes.UnitedStates);
		}

		[BusinessObjectTestExclude]
		[MaxLength(Schema.FTZZoneIDMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ZoneIDList))]
		public ZString FTZZoneID
		{
			get { return FTZAdmissionNumberRetriever.GetFTZZoneID(FTZAdmissionNumber); }
			set
			{
				var oldValue = FTZZoneID;

				if (IsFTZAdmission && oldValue != value && !IsCopying)
				{
					var shouldUpdateZoneID = true;
					if (OnFTZZoneIDChangingEvent != null)
					{
						shouldUpdateZoneID = OnFTZZoneIDChangingEvent();
					}

					if (shouldUpdateZoneID)
					{
						FTZAdmissionNumberRetriever.SetFTZZoneID(this, value.ExcludeChars(FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator));
						FTZControlNumber = ZString.Empty;
					}
				}

				Validation.ValidateFTZZoneID();
				FTZZoneIDInfo.RefreshBinding();
			}
		}

		public Func<ZBool> OnFTZZoneIDChangingEvent;

		public ZPropertyInfo FTZZoneIDInfo
		{
			get { return GetZPropertyInfo(Schema.FTZZoneID); }
		}

		public bool FTZZoneID_ReadOnly
		{
			get { return (FTZControlNumberIsAutoAllocated ? FTZAdmissionHasBeenSent : FTZAdmissionHasBeenLodgedAtCustoms && !IsFTZInWithdrawnStatus) && !FTZControlNumber.IsEmpty && !FTZZoneID.IsEmpty; }
		}

		public bool IsZoneIDInFDAApprovedZonesList
		{
			get { return IORWrapper == null || IORWrapper.FTZAllowsFDAsContainZoneID(FTZZoneID); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(Schema.FTZYearMaxLength)]
		public ZString FTZYear
		{
			get { return FTZAdmissionNumberRetriever.GetFTZYear(FTZAdmissionNumber); }
			set
			{
				var oldValue = FTZYear;
				if (IsFTZAdmission && oldValue != value && !IsCopying)
				{
					FTZAdmissionNumberRetriever.SetFTZYear(this, value.ExcludeChars(FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator));
				}
				FTZYearInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FTZYearInfo
		{
			get { return GetZPropertyInfo(Schema.FTZYear); }
		}

		public bool FTZYear_ReadOnly
		{
			get { return true; }
		}

		[BusinessObjectTestExclude]
		[MaxLength(Schema.FTZControlNumberMaxLength)]
		public ZString FTZControlNumber
		{
			get { return FTZAdmissionNumberRetriever.GetFTZControlNumber(FTZAdmissionNumber); }
			set
			{
				var oldValue = FTZControlNumber;
				if (IsFTZAdmission && oldValue != value && !IsCopying)
				{
					FTZAdmissionNumberRetriever.SetFTZControlNumber(this, value.ExcludeChars(FTZAdmissionNumberRetriever.FTZAdmissionNumberSeparator));
				}
				Validation.ValidateFTZControlNumber();
				FTZControlNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FTZControlNumberInfo
		{
			get { return GetZPropertyInfo(Schema.FTZControlNumber); }
		}

		public
#if DEBUG
		virtual
#endif
		bool FTZControlNumberIsAutoAllocated
		{
			get { return Lookups.ZoneIDList.ContainsCode(FTZZoneID); }
		}

		public bool FTZControlNumber_ReadOnly
		{
			get { return FTZControlNumberIsAutoAllocated || FTZAdmissionHasBeenLodgedAtCustoms && !IsFTZInWithdrawnStatus; }
		}

		public bool FTZAdmissionHasBeenSent
		{
			get { return !AdmissionStatus.IsEmpty; }
		}

		public bool AllocateFTZControlNumberOnSaving;

		internal void ReloadFTZAdmissionNumber()
		{
			if (IsInDatabase)
			{
				var oldValue = (ftzCusEntryNum == null || ftzCusEntryNum.IsDeleted) ? ZString.Empty : ftzCusEntryNum.CE_EntryNum;
				if (ftzCusEntryNum == null || ftzCusEntryNum.IsDeleted)
				{
					ftzCusEntryNum = CusEntryNumber.Load(this, CusEntryNumberTypes.UnitedStates.FTZ, Core.Constants.CountryCodes.UnitedStates, true); // reload from db
				}
				else if (ftzCusEntryNum.IsInDatabase)
				{
					ftzCusEntryNum.Reload();
				}
				if (ftzCusEntryNum != null && !ftzCusEntryNum.IsDeleted && ftzCusEntryNum.CE_EntryNum != oldValue)
				{
					FTZAdmissionNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		void AllocateNextFTZControlNumberyNumber()
		{
			var fountain = IOR?.OrgFountains?.TryGetNumberFountainByZoneID(FTZZoneID, OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber);
			if (fountain == null && WarehouseDocAddress?.Address?.Header != null)
			{
				fountain = WarehouseDocAddress.Address.Header.OrgFountains?.TryGetNumberFountainByZoneID(FTZZoneID, OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse);
			}
			if (fountain != null)
			{
				var nextControlNumber = ZString.Empty;
				var duplicateNumberFound = true;
				while (duplicateNumberFound)
				{
					try
					{
						nextControlNumber = ((ZString)fountain.GetNextFormatted(Factory)).Right(OrganisationViewStmNums.Schema.FTZControlNumberLength);
					}
					catch (NumberFountainMaximumValueReachedException)
					{
						throw new ZCannotSaveException(Constants.FTZControlNumberAllocation.FTZControlNumberRangeIsEmpty, "Cannot Allocate FTZ Control Number");
					}
					duplicateNumberFound = !IsFTZAdmissionNumberUnique(nextControlNumber);
				}

				if (!nextControlNumber.IsEmpty && nextControlNumber != FTZControlNumber)
				{
					FTZControlNumber = nextControlNumber;
				}
			}
		}

		internal bool IsFTZAdmissionNumberUnique(ZString controlNumber)
		{
			var result = controlNumber.IsEmpty;
			if (!result)
			{
				var formattedAdmissionNumber = FTZAdmissionNumberRetriever.GetFormattedFTZAdmissionNumber(FTZZoneID, FTZYear, controlNumber);
				var existingAdmissionNumber = LoadFTZAdmissionNumber(formattedAdmissionNumber);
				result = existingAdmissionNumber == null;
			}
			return result;
		}

		CusEntryNumber LoadFTZAdmissionNumber(ZString admissionNumber)
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.UnitedStates);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, admissionNumber);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, JobDeclarationSchema.Constants.TableName);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.NotEqual, this.PK);
			return new BusinessObjectFactory().LoadTop1<CusEntryNumber>(filter);
		}

		public IAllocateNumberSupporter FTZControlNumberSupporter
		{
			get { return fFTZControlNumberSupporter ?? (fFTZControlNumberSupporter = new FTZControlNumberSupporter(Declaration)); }
		}
		IAllocateNumberSupporter fFTZControlNumberSupporter;

		#endregion

		internal bool US_F_IncludePTT_ReadOnly
		{
			get { return !US_F_DirectDelivery && !IsZoneToZoneFTZAdmission; }
		}

		void ClearBillValuesIfSame(IZType value, string billFieldName)
		{
			foreach (Bill bill in Bills)
			{
				IZType billValue = (IZType)bill[billFieldName];

				if (billValue.Equals(value))
				{
					using (bill.SuspendEffectiveValue(billFieldName, billValue))
					{
						bill[billFieldName] = billValue.Default;
					}

					var infoToRefresh = bill.ZPropertyInfoHash[billFieldName];
					if (infoToRefresh != null)
					{
						infoToRefresh.RefreshBinding();
					}
				}
			}
		}

		public bool IsVoyageFlightNumberVisible
		{
			get
			{
				return IsSea || IsAir || IsRoad ||
					 IsTruck || IsHandCarry || IsRail || IsACE;
			}
		}

		internal CusEntryHeader FTZEntry
		{
			get { return ActiveEntryHeaders.FTZEntry; }
		}

		internal ZString CarrierIRS
		{
			get
			{
				var carrier = IsFTZAdmission ? DeliveryOrPickupCartageCo : null;
				if (carrier != null)
				{
					return carrier.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates);
				}
				return ZString.Empty;
			}
		}

		#region Status

		#region FTZ Admission Status

		public ZDateTime AdmissionMsgLastAcceptedDate
		{
			get { return JE_EntryAuthorisationDate; }
		}

		[MaxLength(GenAddOnColumnMaxLength.FTZAdmissionStatus)]
		public ZString AdmissionStatus
		{
			get { return AddOnAdmissionStatus != null ? AddOnAdmissionStatus.XA_Data : ZString.Empty; }
			set
			{
				var oldValue = AdmissionStatus;
				AddOnColumnStatus(AddOnAdmissionStatus, Constants.GenAddOnColumnFieldName.FTZAdmissionStatus, value, AdmissionStatusInfo);

				if (oldValue != value)
				{
					AddFTZLog(oldValue, AdmissionStatus);
				}
			}
		}

		GenAddOnColumn AddOnAdmissionStatus
		{
			get
			{
				if (addOnAdmissionStatus == null)
				{
					addOnAdmissionStatus = new CachedProperty<GenAddOnColumn>(Factory, delegate
					{
						return GetAddOnStatus(Constants.GenAddOnColumnFieldName.FTZAdmissionStatus);
					});
				}
				return addOnAdmissionStatus.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnAdmissionStatus;

		public ZPropertyInfo AdmissionStatusInfo
		{
			get { return GetZPropertyInfo(Schema.AdmissionStatus); }
		}

		public ZString AdmissionStatusDescription
		{
			get { return GetFTZStatusDescriptionByCode(AdmissionStatus); }
		}

		ZString GetFTZStatusDescriptionByCode(ZString code)
		{
			return Factory.GetCachedValue<FTZMessageStatusList>().GetDescriptionFromCode(code);
		}

		#endregion

		#region FTZ Arrival Status

		public ZDateTime ArrivalMsgLastAcceptedDate
		{
			get
			{
				if (!arrivalMsgLastAcceptedDate.HasValue)
				{
					arrivalMsgLastAcceptedDate = GetLastFTZAcceptedDate(new ZString[] { EM_MessageSubTypeList.Codes.FZArrival });
				}
				return arrivalMsgLastAcceptedDate.Value;
			}
		}
		ZDateTime? arrivalMsgLastAcceptedDate;

		[MaxLength(GenAddOnColumnMaxLength.FTZArrivalStatus)]
		public ZString FTZArrivalStatus
		{
			get { return AddOnFTZArrivalStatus != null ? AddOnFTZArrivalStatus.XA_Data : ZString.Empty; }
			set
			{
				var oldValue = FTZArrivalStatus;
				AddOnColumnStatus(AddOnFTZArrivalStatus, Constants.GenAddOnColumnFieldName.FTZArrivalStatus, value, FTZArrivalStatusInfo);

				if (oldValue != value)
				{
					AddFTZLog(oldValue, FTZArrivalStatus);
				}
			}
		}

		GenAddOnColumn AddOnFTZArrivalStatus
		{
			get
			{
				if (addOnFTZArrivalStatus == null)
				{
					addOnFTZArrivalStatus = new CachedProperty<GenAddOnColumn>(Factory, delegate
					{
						return GetAddOnStatus(Constants.GenAddOnColumnFieldName.FTZArrivalStatus);
					});
				}
				return addOnFTZArrivalStatus.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnFTZArrivalStatus;

		public ZPropertyInfo FTZArrivalStatusInfo
		{
			get { return GetZPropertyInfo(Schema.FTZArrivalStatus); }
		}

		public ZString FTZArrivalStatusDescription
		{
			get { return GetFTZStatusDescriptionByCode(FTZArrivalStatus); }
		}

		#endregion

		#region FTZ Unoncurrence

		public ZDateTime UnconcurrenceMsgLastAcceptedDate
		{
			get
			{
				if (!unconcurrenceMsgLastAcceptedDate.HasValue)
				{
					unconcurrenceMsgLastAcceptedDate = GetLastFTZAcceptedDate(new ZString[] { EM_MessageSubTypeList.Codes.FZUnconcurrence });
				}
				return unconcurrenceMsgLastAcceptedDate.Value;
			}
		}
		ZDateTime? unconcurrenceMsgLastAcceptedDate;

		[MaxLength(GenAddOnColumnMaxLength.FTZUnconcurrenceStatus)]
		public ZString FTZUnconcurrenceStatus
		{
			get => AddOnFTZUnconcurrenceStatus?.XA_Data ?? ZString.Empty;
			set
			{
				var oldValue = FTZUnconcurrenceStatus;
				AddOnColumnStatus(AddOnFTZUnconcurrenceStatus, Constants.GenAddOnColumnFieldName.FTZUnconcurrenceStatus, value, FTZUnconcurrenceStatusInfo);
				if (!IsCopying && oldValue != value)
				{
					AddFTZLog(oldValue, FTZUnconcurrenceStatus);
				}
			}
		}

		GenAddOnColumn AddOnFTZUnconcurrenceStatus
		{
			get
			{
				if (addOnFTZUnconcurrenceStatus == null)
				{
					addOnFTZUnconcurrenceStatus = new CachedProperty<GenAddOnColumn>(Factory, () => GetAddOnStatus(Constants.GenAddOnColumnFieldName.FTZUnconcurrenceStatus));
				}
				return addOnFTZUnconcurrenceStatus.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnFTZUnconcurrenceStatus;

		public ZPropertyInfo FTZUnconcurrenceStatusInfo => GetZPropertyInfo(Schema.FTZUnconcurrenceStatus);

		public ZString FTZUnconcurrenceStatusDescription => GetFTZStatusDescriptionByCode(FTZUnconcurrenceStatus);

		#endregion

		#region FTZ Concurrence

		public ZDateTime ConcurrenceMsgLastAcceptedDate
		{
			get
			{
				if (!concurrenceMsgLastAcceptedDate.HasValue)
				{
					concurrenceMsgLastAcceptedDate = GetLastFTZAcceptedDate(new ZString[] { EM_MessageSubTypeList.Codes.FZConcurrence });
				}
				return concurrenceMsgLastAcceptedDate.Value;
			}
		}
		ZDateTime? concurrenceMsgLastAcceptedDate;

		[MaxLength(GenAddOnColumnMaxLength.FTZConcurrenceStatus)]
		public ZString FTZConcurrenceStatus
		{
			get { return AddOnFTZConcurrenceStatus != null ? AddOnFTZConcurrenceStatus.XA_Data : ZString.Empty; }
			set
			{
				var oldValue = FTZConcurrenceStatus;
				AddOnColumnStatus(AddOnFTZConcurrenceStatus, Constants.GenAddOnColumnFieldName.FTZConcurrenceStatus, value, FTZConcurrenceStatusInfo);
				if (oldValue != value)
				{
					AddFTZLog(oldValue, FTZConcurrenceStatus);
					if (IsConcurrenceCleared)
					{
						US_F_BOCStatInfoFurnished = YesNoDefaultList.Codes.Yes;
					}
				}
			}
		}

		GenAddOnColumn AddOnFTZConcurrenceStatus
		{
			get
			{
				if (addOnFTZConcurrenceStatus == null)
				{
					addOnFTZConcurrenceStatus = new CachedProperty<GenAddOnColumn>(Factory, () => GetAddOnStatus(Constants.GenAddOnColumnFieldName.FTZConcurrenceStatus));
				}
				return addOnFTZConcurrenceStatus.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnFTZConcurrenceStatus;

		public ZPropertyInfo FTZConcurrenceStatusInfo
		{
			get { return GetZPropertyInfo(Schema.FTZConcurrenceStatus); }
		}

		public ZString FTZConcurrenceStatusDescription
		{
			get { return GetFTZStatusDescriptionByCode(FTZConcurrenceStatus); }
		}

		internal ZBool IsConcurrenceCleared
		{
			get { return FTZConcurrenceStatus == FTZMessageStatusList.Codes.ClearConcurrence && GetLastFTZClearedMessage(new ZString[] { EM_MessageSubTypeList.Codes.FZConcurrence }) != null; }
		}

		public ZString FTZConcurrenceRemarks
		{
			get
			{
				ZString result;
				var masterBills = Bills.FindByBillType(BillTypeList.Codes.MasterBill);
				if (masterBills.Length == 1)
				{
					result = masterBills[0].US_F_FZ10Remarks;
				}
				else
				{
					var builder = new ZStringBuilder();
					foreach (var bill in masterBills.Where(bill => !bill.US_F_FZ10Remarks.IsEmpty))
					{
						builder.AppendIfNotEmpty(bill.CU_BillTypeAndNum + " " + bill.US_F_FZ10Remarks.Replace("\r\n", " ").Trim());
					}
					result = builder.ToStringWithDelimiterBetweenAppends(" ");
				}
				return result.Left(FTZRemarksMaxLength);
			}
		}

		const int FTZRemarksMaxLength = 575;

		#endregion

		#region FTZ Delivery Of Goods Status

		public ZDateTime DeliveryOfGoodsMsgLastAcceptedDate
		{
			get
			{
				if (!deliveryOfGoodsMsgLastAcceptedDate.HasValue)
				{
					deliveryOfGoodsMsgLastAcceptedDate = GetLastFTZAcceptedDate(new ZString[] { EM_MessageSubTypeList.Codes.FZDelivery });
				}
				return deliveryOfGoodsMsgLastAcceptedDate.Value;
			}
		}
		ZDateTime? deliveryOfGoodsMsgLastAcceptedDate;

		[MaxLength(GenAddOnColumnMaxLength.FTZDeliveryOfGoodsStatus)]
		public ZString FTZDeliveryOfGoodsStatus
		{
			get { return AddOnFTZDeliveryOfGoodsStatus != null ? AddOnFTZDeliveryOfGoodsStatus.XA_Data : ZString.Empty; }
			set
			{
				var oldValue = FTZDeliveryOfGoodsStatus;
				AddOnColumnStatus(AddOnFTZDeliveryOfGoodsStatus, Constants.GenAddOnColumnFieldName.FTZDeliveryOfGoodsStatus, value, FTZDeliveryOfGoodsStatusInfo);
				if (oldValue != value)
				{
					AddFTZLog(oldValue, FTZDeliveryOfGoodsStatus);
				}
			}
		}

		GenAddOnColumn AddOnFTZDeliveryOfGoodsStatus
		{
			get
			{
				if (addOnFTZDeliveryOfGoodsStatus == null)
				{
					addOnFTZDeliveryOfGoodsStatus = new CachedProperty<GenAddOnColumn>(Factory, () => GetAddOnStatus(Constants.GenAddOnColumnFieldName.FTZDeliveryOfGoodsStatus));
				}
				return addOnFTZDeliveryOfGoodsStatus.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnFTZDeliveryOfGoodsStatus;

		public ZPropertyInfo FTZDeliveryOfGoodsStatusInfo
		{
			get { return GetZPropertyInfo(Schema.FTZDeliveryOfGoodsStatus); }
		}

		public ZString FTZDeliveryOfGoodsStatusDescription
		{
			get { return GetFTZStatusDescriptionByCode(FTZDeliveryOfGoodsStatus); }
		}

		#endregion

		#region FTZ PTT Status

		public ZDateTime PTTMsgLastAcceptedDate
		{
			get
			{
				if (!pttMsgLastAcceptedDate.HasValue)
				{
					var messageSubTypes = new ZString[] { EM_MessageSubTypeList.Codes.FTZPermitToTransfer, EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer };
					if (US_F_IncludePTT)
					{
						messageSubTypes = new ZString[] { EM_MessageSubTypeList.Codes.FTZAdmissionAdd, EM_MessageSubTypeList.Codes.FTZAdmissionReplace, EM_MessageSubTypeList.Codes.FTZPermitToTransfer };
					}
					pttMsgLastAcceptedDate = GetLastFTZAcceptedDate(messageSubTypes);
				}
				return pttMsgLastAcceptedDate.Value;
			}
		}
		ZDateTime? pttMsgLastAcceptedDate;

		[MaxLength(GenAddOnColumnMaxLength.FTZPTTStatus)]
		public ZString FTZPTTStatus
		{
			get { return AddOnFTZPTTStatus != null ? AddOnFTZPTTStatus.XA_Data : ZString.Empty; }
			set
			{
				var oldValue = FTZPTTStatus;
				AddOnColumnStatus(AddOnFTZPTTStatus, Constants.GenAddOnColumnFieldName.FTZPTTStatus, value, FTZPTTStatusInfo);
				if (oldValue != value)
				{
					AddFTZLog(oldValue, FTZPTTStatus);
				}
			}
		}

		GenAddOnColumn AddOnFTZPTTStatus
		{
			get
			{
				if (addOnFTZPTTStatus == null)
				{
					addOnFTZPTTStatus = new CachedProperty<GenAddOnColumn>(Factory, () => GetAddOnStatus(Constants.GenAddOnColumnFieldName.FTZPTTStatus));
				}
				return addOnFTZPTTStatus.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnFTZPTTStatus;

		public ZPropertyInfo FTZPTTStatusInfo
		{
			get { return GetZPropertyInfo(Schema.FTZPTTStatus); }
		}

		public ZString FTZPTTStatusDescription
		{
			get { return GetFTZStatusDescriptionByCode(FTZPTTStatus); }
		}

		internal ZBool IsPTTClearedAndValid
		{
			get { return IsPTTCleared && AllPTTCarriersHavePOA; }
		}

		ZBool AllPTTCarriersHavePOA
		{
			get { return Bills.Count > 0 && Bills.FindByBillType(BillTypeList.Codes.MasterBill).All(bill => bill.PTTCarrierHasPOA); }
		}

		public virtual ZBool IsPTTCleared
		{
			get
			{
				var lastPTTMessage = GetLastFTZClearedMessage(new ZString[] { EM_MessageSubTypeList.Codes.FTZPermitToTransfer });
				var lastCancelPTTMessage = GetLastFTZClearedMessage(new ZString[] { EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer });
				return lastPTTMessage != null && (lastCancelPTTMessage == null || lastCancelPTTMessage.EM_SystemCreateTimeUtc < lastPTTMessage.EM_SystemCreateTimeUtc);
			}
		}

		public virtual ZBool IsPTTArrivalCleared
		{
			get
			{
				var lastPTAMessage = GetLastFTZClearedMessage(new ZString[] { EM_MessageSubTypeList.Codes.FTZPermitToTransferArrival });
				var lastCancelPTAMessage = GetLastFTZClearedMessage(new ZString[] { EM_MessageSubTypeList.Codes.FTZSendPermitToTransferUnArrival });
				return lastPTAMessage != null && (lastCancelPTAMessage == null || lastCancelPTAMessage.EM_SystemCreateTimeUtc < lastPTAMessage.EM_SystemCreateTimeUtc);
			}
		}

		public ZString FTZPTTRemarks
		{
			get
			{
				ZString result;
				var masterBills = Bills.FindByBillType(BillTypeList.Codes.MasterBill);
				if (masterBills.Length == 1)
				{
					result = masterBills[0].US_F_Remarks;
				}
				else
				{
					var builder = new ZStringBuilder();
					foreach (var bill in masterBills.Where(bill => !bill.US_F_Remarks.IsEmpty))
					{
						builder.AppendIfNotEmpty(bill.CU_BillTypeAndNum + " " + bill.US_F_Remarks.Replace("\r\n", " ").Trim());
					}
					result = builder.ToStringWithDelimiterBetweenAppends(" ");
				}
				return result.Left(FTZRemarksMaxLength);
			}
		}

		#endregion

		ZDateTime GetLastFTZAcceptedDate(ZString[] messageSubType)
		{
			var result = ZDateTime.Empty;
			if (IsFTZAdmission && !HasBeenWithdrawn)
			{
				var lastClearedMessage = GetLastFTZClearedMessage(messageSubType) as MQEDIMessage;
				result = lastClearedMessage?.DispositionDateTime ?? ZDateTime.Empty;
			}
			return result;
		}

		EDIMessage GetLastFTZClearedMessage(ZString[] messageSubType)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, messageSubType);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " desc";
			var receivedMessage = new List<MQEDIMessage>(new TypedEnumerable<MQEDIMessage>(Messages.Find(query)));
			HookMessagesCountChangedIfNeeded();
			return receivedMessage.FirstOrDefault(message => message.IsFTZCleared);
		}

		void HookMessagesCountChangedIfNeeded()
		{
			if (!hasHookedMessagesCountChanged)
			{
				hasHookedMessagesCountChanged = true;
				Messages.CountChanged -= Messages_CountChanged;
				Messages.CountChanged += Messages_CountChanged;
			}
		}
		bool hasHookedMessagesCountChanged;

		void Messages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			arrivalMsgLastAcceptedDate = null;
			unconcurrenceMsgLastAcceptedDate = null;
			concurrenceMsgLastAcceptedDate = null;
			deliveryOfGoodsMsgLastAcceptedDate = null;
			pttMsgLastAcceptedDate = null;
		}

		internal bool FTZAdmissionHasBeenLodgedAtCustoms
		{
			get
			{
				IStatusList list = Factory.GetCachedValue<FTZMessageStatusList>();
				return LogManager.HasAClearLog(list.GetFirstClearStatusFor(ImportMessageStatusList.MessageType.Undefined));
			}
		}

		public bool IsFTZWaitingForResponse
		{
			get
			{
				var list = Factory.GetCachedValue<FTZMessageStatusList>();
				return list.IsWaitingForResponse(AdmissionStatus);
			}
		}

		bool IsFTZInWithdrawnStatus
		{
			get { return AdmissionStatus == FTZMessageStatusList.Codes.ClearFTZAdmissionDelete; }
		}

		public bool HasFTZTransactionsWithCustoms
		{
			get { return IsFTZWaitingForResponse || IsFTZInWithdrawnStatus || FTZAdmissionHasBeenLodgedAtCustoms; }
		}

		void AddFTZLog(ZString oldStatus, ZString status)
		{
			if (StatusList.IsStatusClear(status))
			{
				LogManager.AddAClearLogIfNecessary(oldStatus, status, StatusList);
			}
			else
			{
				LogManager.AddARejectLogIfNecessary(oldStatus, status, StatusList);
			}
		}

		public void CloseRelatedPermits()
		{
			List<CusPermitHeader> relatedPermits = FindRelatedPermits();
			foreach (var permit in relatedPermits)
			{
				permit.Close();
			}
		}

		public List<CusPermitHeader> FindRelatedPermits()
		{
			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, PermitEntryLineGrouping.GetOutwardEntryNumber(this));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_TransactionType, PermitTransactionTypeList.Codes.OBL);
			var subQuery = new ZDBOnlySubQuery(typeof(CusPermitHeader), CusPermitHeaderSchema.PK);
			subQuery.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Permit);
			subQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			subQuery.AddToFilter(CusPermitHeaderSchema.CPH_Type, PermitTransactionTypeList.Codes.FTZ);
			query.AddSubQuery(CusPermitLineTransactionSchema.CPL_CPH_PermitHeader, subQuery, JoinCondition.And);
			var permitLines = Factory.Load<BaseCusPermitLineTransaction>(query);

			HashSet<CusPermitHeader> permits = new HashSet<CusPermitHeader>();
			foreach (var permitLine in permitLines)
			{
				permits.Add((CusPermitHeader)permitLine.PermitHeader);
			}
			return permits.ToList();
		}

		#endregion

		#region FTZ Print

		public FTZ214EntryLineCollection FTZ214PrintCollection
		{
			get { return ftz214PrintCollection ?? (ftz214PrintCollection = new FTZ214EntryLineCollection(this)); }
		}
		FTZ214EntryLineCollection ftz214PrintCollection;

		#endregion

		#region IFTZCommonHeader Members

		ZDecimal IFTZConcurrence.FTZConcurrenceQty
		{
			get => US_FTZConcurrenceQty;
			set => US_FTZConcurrenceQty = value;
		}

		void IFTZCommonHeader.AddMessage(MQEDIMessage message)
		{
			Messages.Add(message);
		}

		bool IFTZCommonHeader.HasBeenLodgedAtCustoms
		{
			get { return FTZAdmissionHasBeenLodgedAtCustoms; }
		}

		bool IFTZCommonHeader.IncludePTTInAdmission
		{
			get { return US_F_IncludePTT; }
		}

		ZString IFTZCommonHeader.FTZAdmissionNumber
		{
			get
			{
				var result = FTZAdmissionNumberFormatted;
				if (!result.IsEmpty)
				{
					result = FTZControlNumberIsAutoAllocated && FTZControlNumber.IsEmpty ? (ZString)MQEDIMessage.USFTZAdmissionNumberPlaceHolder : (FTZZoneID.IsEmpty && FTZControlNumber.IsEmpty ? ZString.Empty : result);
				}
				return result;
			}
		}

		ZString IFTZCommonHeader.ZoneID
		{
			get { return FTZZoneID; }
		}

		ZInt IFTZCommonHeader.CalendarYear
		{
			get { return ZInt.ParseSafe(FTZYear, 0); }
		}

		ZString IFTZCommonHeader.ControlNumber
		{
			get { return FTZControlNumber; }
		}

		bool IFTZCommonHeader.DirectDeliveryIndicator
		{
			get { return US_F_DirectDelivery; }
		}

		IEnumerable<IFTZBillCommon> IFTZCommonHeader.Bills
		{
			get
			{
				if (IsAir || IsSeaAndIsAMSHBREffective)
				{
					return new TypedEnumerable<IFTZBill>(LowestBills).OrderBy(x => x.CU_BillNum);
				}
				else
				{
					return Bills.FindByBillType(BillTypeList.Codes.MasterBill);
				}
			}
		}

		IEnumerable<IFTZBillCommon> IFTZCommonHeader.LowestBills => new TypedEnumerable<IFTZBill>(LowestBills);

		internal ZBool IsFTZPTTStatusClearPermitToTransfer => FTZPTTStatus == FTZMessageStatusList.Codes.ClearPermitToTransfer;

		void IFTZCommonHeader.SetMessageStatus(ZString subType, ZString status)
		{
			switch (subType)
			{
				case EM_MessageSubTypeList.Codes.FTZAdmissionAdd:
				case EM_MessageSubTypeList.Codes.FTZAdmissionDelete:
				case EM_MessageSubTypeList.Codes.FTZAdmissionReplace:
					AdmissionStatus = status;
					break;
				case EM_MessageSubTypeList.Codes.FTZPermitToTransfer:
				case EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer:
				case EM_MessageSubTypeList.Codes.FTZPermitToTransferArrival:
				case EM_MessageSubTypeList.Codes.FTZSendPermitToTransferUnArrival:
					FTZPTTStatus = status;
					break;
				case EM_MessageSubTypeList.Codes.FZArrival:
					FTZArrivalStatus = status;
					break;
				case EM_MessageSubTypeList.Codes.FZConcurrence:
					FTZConcurrenceStatus = status;
					break;
				case EM_MessageSubTypeList.Codes.FZDelivery:
					FTZDeliveryOfGoodsStatus = status;
					break;
				case EM_MessageSubTypeList.Codes.FZUnconcurrence:
					FTZUnconcurrenceStatus = status;
					break;
			}
		}

		ZString IFZEventHeader.AirportCode
		{
			get
			{
				var result = ZString.Empty;
				if (IsAir)
				{
					var portOfArrival = PortOfArrival;
					if (portOfArrival != null)
					{
						result = portOfArrival.RL_IATA;
					}
				}
				return result;
			}
		}

		ZString IFZEventHeader.DeliveryCode
		{
			get { return US_F_DeliveryCode; }
		}

		ZDecimal IFZEventHeader.Quantity
		{
			get
			{
				var result = ZDecimal.Zero;
				var iFTZHeader = (IFTZCommonHeader)this;
				foreach (Bill bill in iFTZHeader.Bills)
				{
					result += ((IFTZBill)bill).Quantity;
				}
				return result;
			}
		}

		ZBool IFZEventHeader.IsAir => IsAir;

		#endregion

		#endregion

		#region IStatementDeleteAndAddEntity Members

		ZString IStatementDeleteTransaction.EntryNumber
		{
			get { return ImportEntryNumber; }
		}

		ZString IStatementDeleteTransaction.EntryFilerCode
		{
			get { return EntryFilerCode; }
		}

		ZString IStatementDeleteTransaction.ProcessingPort
		{
			get { return ProcessingDistrictPort; }
		}

		ZString IStatementDeleteTransaction.PreparerPort
		{
			get { return US_PreparerDistrictPort; }
		}

		ZString IStatementDeleteTransaction.PreparerOfficeCode
		{
			get { return US_PreparerOfficeCode; }
		}

		bool IStatementDeleteTransaction.ShouldPopulatePreparerSite
		{
			get { return IsRemoteLocationFiling; }
		}

		bool IStatementDeleteTransaction.IsACE
		{
			get { return IsACE; }
		}

		ZString IStatementDeleteTransaction.PortOfEntry
		{
			get { return US_SchDEntry; }
		}

		BusinessObjectFactory IStatementDeleteTransaction.Factory
		{
			get { return Factory; }
		}

		GlbBranch IStatementDeleteTransaction.Branch
		{
			get { return Branch; }
		}

		void IStatementDeleteTransaction.AddMessages(MQEDIMessage message)
		{
			Messages.Add(message);
		}

		ZString IStatementDeleteTransaction.PaymentType
		{
			get { return US_PaymentType; }
		}

		ZDateTime IStatementDeleteTransaction.PreliminaryStatementPrintDate
		{
			get { return US_PreliminaryStatementPrintDate; }
		}

		ZDateTime IStatementDeleteTransaction.ReleaseDate
		{
			get { return JE_EntryAuthorisationDate; }
		}

		ZString IStatementDeleteTransaction.ClientBranchDesignation
		{
			get { return US_ClientBranchDesignation; }
		}

		ZString IStatementDeleteTransaction.PeriodicStatementMonth
		{
			get { return US_PeriodicStatementMM; }
		}

		bool IStatementDeleteTransaction.ShouldGenerateACEStatementMessage
		{
			get { return true; }
		}

		bool IStatementDeleteTransaction.IsStatementUpdateMessagePending
		{
			get
			{
				Messages.Reload(true);
				return this.IsSTUPending();
			}
		}

		#endregion

		#region PPQ Form 368 Notice Of Arrival Fields

		public ZString NoticeOfArrivalPortOfDeparture
		{
			get
			{
				var result = ZString.Empty;
				var foreignPort = SchDLoadingPort;
				if (foreignPort != null)
				{
					result = foreignPort.PortName;
				}
				if (result.IsEmpty)
				{
					var portOfLoading = PortOfLoading;
					if (portOfLoading != null)
					{
						result = portOfLoading.RL_PortName + ", " + portOfLoading.RL_RN_NKCountryCode;
					}
				}
				return result;
			}
		}

		public ZString UniqueAgricultureLicenseNumber
		{
			get { return UniqueValueCalculator.GetUniqueValue(InvoiceLines.Cast<JobComInvoiceLine>(), x => x.US_AgricultureLicNo, (ZString)USConstants.SeeAttachedIndicator); }
		}

		public ZString UniqueCountryOfOrigin
		{
			get { return UniqueValueCalculator.GetUniqueValue(InvoiceLines.Cast<JobComInvoiceLine>(), x => x.US_UC_NKCountryOfOrigin, (ZString)USConstants.MultipleValueIndicator); }
		}

		public ZString UniqueCountryOfOriginWithDesc
		{
			get
			{
				var result = ZString.Empty;
				if (!UniqueCountryOfOrigin.IsEmpty)
				{
					var country = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, UniqueCountryOfOrigin);
					if (country == null)
					{
						result = UniqueCountryOfOrigin;
					}
					else
					{
						result = country.UC_Code + " " + country.UC_Name;
					}
				}
				return result;
			}
		}

		#endregion

		#region ICusInBondParent Members

		ZGuid Freight.Integration.ICusInBondParent.GetDeclarationPK(ZGuid companyPK)
		{
			var company = Company;
			return company != null && company.PK == companyPK ? PK : ZGuid.Empty;
		}

		ZString Freight.Integration.ICusInBondParent.ParentType
		{
			get { return "Declaration"; }
		}

		string Freight.Integration.ICusInBondParent.TablePrefix
		{
			get { return TablePrefix; }
		}

		void Freight.Integration.ICusInBondParent.PopulateJobNumberIfNeeded()
		{
			PopulateJE_DeclarationReferenceIfNeeded();
		}

		ZString Freight.Integration.ICusInBondParent.JobNumber
		{
			get { return JobNumber; }
		}

		ZString IDISHost.HumanReadable => "DIS";

		ZBool IDISHost.DISEditable => Environment.Env.Security.CustomsDISEdit.IsAllowed;

		ZString Freight.Integration.ICusInBondParent.HouseBill
		{
			get { return JE_HouseBill; }
		}

		event EventHandler Freight.Integration.ICusInBondParent.VisibilityChanged
		{
			add { JE_MessageTypeInfo.ValueChanged += value; }
			remove { JE_MessageTypeInfo.ValueChanged -= value; }
		}

		bool Freight.Integration.ICusInBondParent.IsVisible
		{
			get { return IsImport; }
		}

		bool Freight.Integration.ICusInBondParent.IsInternalBrokerage
		{
			get { return true; }
		}

		#endregion

		#region IWorkflowProviderEvent

		OrgHeader[] IWorkflowProviderEvent.RecipientOrganisations => new[] { ShippingLine, Forwarder }.Where(x => x != null).Distinct().ToArray();

		#endregion

		#region IMessageNotificationsProvider

		IEnumerable<BusinessObject> IMessageNotificationsProvider.ExcludedChildrenAndTheirDescendentsFromMessageNotifications
		{
			get { return ExcludedChildrenAndTheirDescendentsFromMessageNotifications; }
		}

		IList<BusinessObject> ExcludedChildrenAndTheirDescendentsFromMessageNotifications
		{
			get { return fExcludedChildrenAndTheirDescendentsFromMessageNotifications ?? (fExcludedChildrenAndTheirDescendentsFromMessageNotifications = new List<BusinessObject>()); }
		}
		IList<BusinessObject> fExcludedChildrenAndTheirDescendentsFromMessageNotifications;

		void IMessageNotificationsProvider.ExcludeChildAndItsDescendentsFromMessageNotifications(BusinessObject bizObj)
		{
			ExcludedChildrenAndTheirDescendentsFromMessageNotifications.Add(bizObj);
		}

		ZBool IMessageNotificationsProvider.HasMessageErrors
		{
			get { return new USCustomsNotificationCollector(this, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors().Any(); }
		}

		#endregion

		#region IDISHost
		ZString IDISHost.ImporterName
		{
			get { return Importer != null ? Importer.OH_FullName : ZString.Empty; }
		}

		IEnumerable<string> IDISHost.ApplicationCodes
		{
			get { yield return Core.Constants.Customs.DocumentImageSystemIDs.US_DIS; }
		}

		ZBool IDISHost.ShowDISFeatures
		{
			get { return IsACE || IsExport || IsACEDrawback || IsACERecon || IsFTZAdmission; }
		}

		public bool IsACERecon
		{
			get { return IsReconMessageType && JE_ApplicationCode == JobApplicationCodeList.Codes.ACE; }
		}

		ZGuid IDISHost.BranchPK
		{
			get { return Branch != null ? Branch.PK : ZGuid.Empty; }
		}

		ZGuid IDISHost.CompanyPK
		{
			get { return Branch != null ? Branch.GB_GC : ZGuid.Empty; }
		}

		IDISHost IDISHostProvider.DISHost
		{
			get { return this; }
		}

		ZString IDISHost.JobNumber
		{
			get { return JE_DeclarationReference; }
		}

		IControllerIDProvider IDISHost.ControllerIDProvider
		{
			get { return this; }
		}

		IHaveRequiredDocuments IDISHost.RequiredDocumentsProvider
		{
			get { return DocsAndCartage; }
		}

		event EventHandler IDISHost.DISFeatureVisibilityChanged
		{
			add
			{
				JE_MessageTypeInfo.ValueChanged += value;
				JE_ApplicationCodeInfo.ValueChanged += value;
			}
			remove
			{
				JE_MessageTypeInfo.ValueChanged -= value;
				JE_ApplicationCodeInfo.ValueChanged -= value;
			}
		}

		public ZString DISStatus
		{
			get
			{
				if (!IsDISStatusApplicable)
				{
					return ZString.Empty;
				}

				if (disStatus == null)
				{
					disStatus = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;

						var flattenedAddInfos = DocsAndCartage.RequiredDocuments.Cast<JobRequiredDocument>().SelectMany(x => x.AddInfos).Cast<JobRequiredDocumentAddInfo>();
						var statusList = flattenedAddInfos.Where(x => x.EX_ApplicationCode == Core.Constants.Customs.DocumentImageSystemIDs.US_DIS && !x.EX_Status.IsEmpty).Select(x => x.EX_Status).Distinct();

						var count = statusList.Take(2).Count();
						if (count > 0)
						{
							result = count == 1 ? statusList.First().ToString() : Enterprise.Customs.Common.US.DIS.StatusList.Codes.MUL;
						}
						return result;
					});
				}
				return disStatus.Value;
			}
		}
		CachedProperty<ZString> disStatus;

		public ZString DISStatusDescription
		{
			get { return Factory.GetCachedValue<Common.US.DIS.StatusList>().GetDescriptionFromCode(DISStatus); }
		}

		public bool IsDISStatusApplicable
		{
			get { return (IsImport && IsACE) || IsExport || IsACEDrawback || IsACERecon; }
		}

		internal IEnumerable<MessageBuilders.IBillDetails> GetLowestBillDetails()
		{
			((BusinessObjectCollection)LowestBills).Sort(Bill.Schema.CU_ParentBillUniqueCode, ListSortDirection.Ascending);
			foreach (Bill bill in LowestBills)
			{
				if (bill.NoITNumbersExist || US_NonAMS)
				{
					yield return bill;
				}
				else
				{
					foreach (MessageBuilders.IBillDetails lowestBill in bill.ITAndSplitDetails)
					{
						yield return lowestBill;
					}
				}
			}
		}

		IEnumerable<IeDoc> IDISHost.EDocs
		{
			get
			{
				var docManager = Shipment != null ? Shipment.DocManagerInfo : DocManagerInfo;
				return docManager.GetRelatedEDocsView();
			}
		}

		IEnumerable<ZString> IUSDISHost.FormGroups
		{
			get
			{
				var formGroups = new List<ZString>();

				if (IsDrawback)
				{
					formGroups.Add(JobMessageTypeList.Codes.Drawback);
				}
				else
				{
					formGroups.Add(DISFormGroupCodes.NoGroup);

					if (IsRecon)
					{
						formGroups.Add(JobMessageTypeList.Codes.Recon);
					}

					if (IsFTZAdmission)
					{
						formGroups.Add(JobMessageTypeList.Codes.FTZ);
					}
				}

				return formGroups;
			}
		}

		IUSDISDefaultValues IUSDISHost.ValueProvider
		{
			get
			{
				disDocumentIDListCached = null;
				return new DIS.JobDeclarationWrapper(this);
			}
		}

		ZString IUSDISHost.MessageSendingWarning
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				if (IsFTZAdmission)
				{
					if (FTZZoneID.IsEmpty || FTZControlNumber.IsEmpty)
					{
						builder.Append(NoFTZAdmissionNumberAvailable);
					}
				}
				else if (IsImport)
				{
					if (EntryFilerCode.IsEmpty)
					{
						builder.Append(NoEntryFilerCodeAvailable);
					}
					if (ImportEntryNumber.IsEmpty)
					{
						builder.Append(NoEntryNumberAvailable);
					}
					if (!(US_EnableENS || US_EnableCRL))
					{
						builder.Append(TickEnableENSOrEnableCRL);
					}
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		ZString IUSDISHost.MessageSendingError
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				if (((IUSDISHost)this).ValueProvider.PreparerID.IsEmpty)
				{
					builder.AppendFormat(NoPrepareIDAvailable, ((Integration.IRegistryItemInternals)USCustomsDataRegistry.Instance.EntryFiler).Location);
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		public void UpdateEntrySummaryNotificationStatus(ZString actionID)
		{
			if (!actionID.IsEmpty)
			{
				var ensStatusNotifications = ENSStatusNotifications;
				foreach (var record in ensStatusNotifications.Cast<ErrorsRecord>().Where(x => x.message.EM_ApplicationReference.SubstringSafe(2) == actionID))
				{
					var message = record.message;
					var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code);
					query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
					if (!message.Logs.HasLogWith(query))
					{
						message.Logs.AddNew(Events.Authorised, EM_ActionStatusList.Codes.Complete);
					}
				}
			}
		}

		public static readonly string NoEntryFilerCodeAvailable = "There is no entry filer code available. The DIS message will be sent without an entry filer code. Please allocate an entry filer code.";
		public static readonly string NoEntryNumberAvailable = "There is no entry number available. The DIS message will be sent without an entry number. Please allocate an entry number.";
		internal const string TickEnableENSOrEnableCRL = "Please tick either Enable Entry Summary or Enable Cargo Release on the front tab.";
		public static readonly string NoPrepareIDAvailable = "Prepare ID is required for sending DIS messages. Please set one at the registry {0}.";
		internal const string NoFTZAdmissionNumberAvailable = "There is no valid FTZ admission mumber. The DIS message will be sent without admission number.";

		bool IDISHost.NeedToDoPreFormAction()
		{
			if (IsDrawback || IsRecon)
			{
				return false;
			}
			else
			{
				return !IsMergeDone || MergeManager.RequiresMerge;
			}
		}

		bool IDISHost.DoPreFormAction()
		{
			if (IsDrawback || IsRecon)
			{
				return false;
			}
			else
			{
				return DoMerge();
			}
		}

		IEnumerable<ZString> IDISHost.ErrorMessages
		{
			get
			{
				if (RegistryEntryFilerCode.IsEmpty)
				{
					yield return Res.GetString("93F25DA0-A731-4274-A1BC-C749D5E7995B", "DIS is only supported for ABI filers. You need to submit documents via emails instead.");
				}
			}
		}

		Integration.Customs.Shared.IDISReferenceNumberFountainStrategy IDISHost.DISReferenceNumberFountainStrategy => null;

		#endregion
		public ZString RegistryEntryFilerCode
		{
			get { return USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty).EntryFilerCode; }
		}

		public ZBool KnownImporterIndicator
		{
			get { return IORWrapper != null && IORWrapper.ZO_KnwImpInd == YesNoDefaultList.Codes.Yes; }
		}

		public ZBool IsExpressConsignment
		{
			get { return US_ExpConsign == YesNoDefaultList.Codes.Yes; }
		}

		#region ITemplateCopyable Members

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (JobDeclaration)base.CloneInternal(args);

			foreach (USDeclarationFSISLine fsis in FSISLines)
			{
				var fsisCloned = (USDeclarationFSISLine)fsis.Clone();
				result.FSISLines.Add(fsisCloned);
			}

			return result;
		}

		#endregion

		#region IAddInfoWithConcurrencyResolverSupporter Members

		ZPropertyInfo IAddInfoWithConcurrencyResolverSupporter.NotificationAddInfo
		{
			get { return JE_MessageTypeInfo; }
		}

		#endregion

		#region ICusEntryNumberErrorReporter

		bool ICusEntryNumberParent.CanBeChangedOrDeleted(CusEntryNumber entryNumber, out string errMsg)
		{
			errMsg = string.Empty;
#if DEBUG
			if (AllowToChangeEntryNumberDuringTest)
			{
				return true;
			}
#endif

			var shouldReportError =
				entryNumber.IsInDatabase &&
				!entryNumber.CE_EntryNumInfo.OriginalValue.IsEmpty &&
				entryNumber.CE_EntryTypeInfo.OriginalValue.ToString() == CusEntryNumberTypes.UnitedStates.EntrySummary &&
				GetImportEntriesWithTransactionsWithCustoms().Any();

			if (shouldReportError)
			{
				errMsg = string.Format("This entry number {0} {1} is already lodged at customs. Attempted to change it to {2} {3}.", entryNumber.CE_EntryTypeInfo.OriginalValue, entryNumber.CE_EntryNumInfo.OriginalValue,
				entryNumber.CE_EntryType, entryNumber.CE_EntryNum);
			}

			return string.IsNullOrEmpty(errMsg);
		}
#if DEBUG
		internal bool AllowToChangeEntryNumberDuringTest;
#endif

		IEnumerable<CusEntryHeader> GetImportEntriesWithTransactionsWithCustoms()
		{
			foreach (CusEntryHeader entry in ActiveEntryHeaders)
			{
				if ((entry.IsRelatedToENSEntry || entry.IsFormalEntry) && (entry.HasBeenLodgedAtCustoms || entry.IsWaitingForResponse))
				{
					yield return entry;
				}
			}
		}

		void ICusEntryNumberParent.EntryNumberChanged(ZString oldValue, ZString newValue)
		{
			entryNumberChangedCallStack = System.Environment.StackTrace;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Logs.AddNew(Events.EditedARecord, ZString.Format("Entry Number {0} was changed to {1}", oldValue, newValue));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		string ICusEntryNumberParent.EntryNumberChangedCallStack => entryNumberChangedCallStack;

		string entryNumberChangedCallStack;

		#endregion

		#region ICargoManifestStatusQueryHeader Members

		BusinessObjectFactory ICargoManifestStatusQueryHeader.Factory
		{
			get { return Factory; }
		}

		ZString ICargoManifestStatusQueryHeader.ProcessingPortCode
		{
			get { return ProcessingDistrictPort; }
		}

		ZString ICargoManifestStatusQueryHeader.ProcessingOfficeCode
		{
			get { return ProcessingOfficeCode; }
		}

		CodeDescriptionPairList ICargoManifestStatusQueryHeader.ActionList
		{
			get
			{
				var isAir = IsAir;
				var isFTZAdmission = IsFTZAdmission;
				return Factory.GetCachedValue<CodeDescriptionPairList>(string.Format(CultureInfo.CurrentCulture, "USCargoManifestStatusQueryHeaderActionList{0}_{1}", isAir, isFTZAdmission), () =>
					{
						var actionList = new CargoManifestStatusQueryActionList();
						if (!isAir)
						{
							actionList.RemoveCode(CargoManifestStatusQueryActionList.Codes.MAWB);
							actionList.RemoveCode(CargoManifestStatusQueryActionList.Codes.HAWB);
							actionList.RemoveCode(CargoManifestStatusQueryActionList.Codes.AIR);
						}
						else
						{
							actionList.RemoveCode(CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill);
						}

						if (isFTZAdmission)
						{
							actionList.RemoveCode(CargoManifestStatusQueryActionList.Codes.Entry);
							actionList.RemoveCode(CargoManifestStatusQueryActionList.Codes.InBond);
						}

						actionList.Sort();
						return actionList;
					});
			}
		}

		ZString ICargoManifestStatusQueryHeader.TransportMode
		{
			get { return JE_TransportMode; }
		}

		IEnumerable<ICargoManifestStatusQueryData> ICargoManifestStatusQueryHeader.ObjectsForQuery
		{
			get
			{
				var result = new List<ICargoManifestStatusQueryData>();

				if (ActiveEntryHeaders.EntrySummaryEntry != null)
				{
					result.Add(ActiveEntryHeaders.EntrySummaryEntry);
				}
				else if (ActiveEntryHeaders.SimplifiedEntry != null)
				{
					result.Add(ActiveEntryHeaders.SimplifiedEntry);
				}

				foreach (Bill bill in Bills)
				{
					result.Add(bill);
				}

				ITNumberCollection.Populate();

				foreach (ITNumber number in ITNumberCollection)
				{
					if (result.Find(x => x.EntryOrInBondNumber == ((ICargoManifestStatusQueryData)number).EntryOrInBondNumber) == null)
					{
						result.Add(number);
					}
				}
				return result;
			}
		}

		ZBool ICargoManifestStatusQueryHeader.IsACEQuery
		{
			get { return IsACECargoCertificationMode; }
		}

		#endregion

		public override void PopulateJE_DeclarationReferenceIfNeeded()
		{
			base.PopulateJE_DeclarationReferenceIfNeeded();
			if (ShouldUpdateConsolidatedJobNo)
			{
				foreach (JobComInvoiceHeader invoice in Invoices)
				{
					if (!invoice.US_ReleaseEntryNumber.IsEmpty)
					{
						invoice.SetConsolidatedJobNoForReleaseEntry(JE_DeclarationReference, US_EntryFilerCode + DecEntryNumber);
					}
				}
				ShouldUpdateConsolidatedJobNo = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = ZString.Empty;

				if (IsDrawback)
				{
					var importerName = (Importer != null && !Importer.OH_FullName.IsEmpty)
						? string.Format(" - {0}", Importer.OH_FullName)
						: string.Empty;

					result = Res.GetString("0873D9FC-48B8-4954-95EA-EE7D9A2CEB79", "Drawback - {0}{1}", JE_DeclarationReference, importerName);
				}
				else if (IsExport)
				{
					var builder = new ZStringBuilder();
					if (!US_TransportReference.IsEmpty)
					{
						builder.Append(string.Format(CultureInfo.InvariantCulture, "TRF: {0}", US_TransportReference));
					}

					if (!DeclarationNumber.IsEmpty)
					{
						builder.Append(string.Format(CultureInfo.InvariantCulture, "ITN: {0}", DeclarationNumber));
					}

					ZString suffix = builder.ToStringWithDelimiterBetweenAppends(" ");
					result = suffix.IsEmpty
						? base.HumanReadableShortcutNameCore
						: (ZString)string.Format(CultureInfo.InvariantCulture, "{0} - {1}", JobNumber, suffix);
				}
				else
				{
					result = base.HumanReadableShortcutNameCore;
				}
				return result;
			}
		}

		protected override IBillDetails CreateConsolBillDetailsCore(ForwardingConsol relevantConsol)
		{
			return (IsExport && relevantConsol.HasCoLoadData()) ? new CoLoadConsolBillDetais(relevantConsol) : relevantConsol;
		}

		#region IApportionInvoiceHolder Members
		string IApportionInvoiceHolder.CountryContext
		{
			get { return IsImport ? USIMPIncoTermAndCharge : (string)CountryCode; }
		}
		internal const string USIMPIncoTermAndCharge = "USIMP";
		#endregion

		#region ICusDispositionParent

		ZString ICusDispositionParent.Type
		{
			get { return CusDispositionTypeCodeList.Codes.USPGAEntryStatus; }
		}

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return Lookups.DispositionCodeList.GetDescriptionFromCode(status);
		}

		BusinessObject ICusDispositionParent.CollectionMaster
		{
			get { return this; }
		}

		ZString ICusDispositionParent.ParentTableCode
		{
			get { return JobDeclarationSchema.Constants.Prefix; }
		}

		#endregion

		#region IDeclarationDutyDataProvider Members

		IEntryHeaderDutyDataProvider IDeclarationDutyDataProvider.EntrySummaryEntry => ActiveEntryHeaders.EntrySummaryEntry;
		IEntryHeaderDutyDataProvider IDeclarationDutyDataProvider.FTZEntry => ActiveEntryHeaders.FTZEntry;
		IEnumerable<IInvoiceLineDutyDataProvider> IDeclarationDutyDataProvider.InvoiceLines => InvoiceLines.Cast<IInvoiceLineDutyDataProvider>();

		#endregion

		#region Override IJobDeclarationMessageSupporter Members

		protected override ZBool SupportEntryDeclarationMessageCore
		{
			get { return (IsFormalImport && IsACE && US_EnableENS) || IsExport || IsACERecon; }
		}

		protected override ZString GetReasonForNotSupportEntryDeclarationMessageCore()
		{
			var result = ZString.Empty;
			if (IsFormalImport)
			{
				if (!IsACE || !US_EnableENS)
				{
					result = SendEntryDeclarationTriggerForNonSupportedImportMessage;
				}
			}
			else if (!IsExport)
			{
				result = ZString.Format(SendEntryDeclarationTriggerForNonSupportedShipmentTypeMessage, JE_MessageType);
			}

			return result;
		}
		internal const string SendEntryDeclarationTriggerForNonSupportedImportMessage = "For import declarations, the Send Entry/Declaration Message trigger is only supported when the message mode is 'ACE' and entry summary is enabled.";
		internal const string SendEntryDeclarationTriggerForNonSupportedShipmentTypeMessage = "The Send Entry/Declaration Message trigger is not support for shipment type {0}.";

		protected override ZBool SupportReleaseMessageCore
		{
			get { return IsImport && IsACE && US_EnableCRL; }
		}

		protected override ZString GetReasonForNotSupportReleaseMessageCore()
		{
			var result = ZString.Empty;
			if (!IsImport || !IsACE || !US_EnableCRL)
			{
				result = SendReleaseTriggerNotSupportedMessage;
			}
			return result;
		}
		internal const string SendReleaseTriggerNotSupportedMessage = "The Send Release Message trigger is only supported for import declaration and message mode is 'ACE' and cargo release is enabled.";

		protected override CargoWise.EntityFramework.IProcessor GetEntryDeclarationMessageProcessorCore()
		{
			if (IsImport && IsACE && US_EnableENS)
			{
				return new AutoSendEntrySummaryMessageProcessor(Declaration);
			}
			else if (IsExport)
			{
				return new AutoSendAESTIRMessageProcessor(Declaration);
			}
			else if (IsReconMessageType)
			{
				return new AutoSendReconMessageProcessor(Declaration);
			}

			return null;
		}

		protected override CargoWise.EntityFramework.IProcessor GetReleaseMessageProcessorCore()
		{
			if (Declaration.IsImport && Declaration.IsACE && Declaration.US_EnableCRL)
			{
				return new AutoSendCargoReleaseMessageProcessor(Declaration);
			}

			return null;
		}

		#endregion

		#region ILiquidationProvider Members

		JobDeclaration ILiquidationProvider.Declaration => this;

		void ILiquidationProvider.SetAnticipatedLiquidatedDuty(ZDecimal dutyAmount)
		{
			US_ALDuty = dutyAmount;
		}

		void ILiquidationProvider.SetAnticipatedLiquidationDate(ZDateTime dateTime)
		{
			US_ALDate = dateTime;
		}

		#endregion

		#region IDeclaration Members

		bool IDeclaration.ShouldCalculateMPFAndDutyDate => ShouldCalculateMPFAndDutyDate;

		ZDate IDeclaration.US_DutyCalcDate
		{
			get
			{
				var result = ZDate.Empty;
				if (ActiveEntryHeaders.EntrySummaryEntry is CusEntryHeader entryHeader)
				{
					if (!US_ImmediateDelivery || (entryHeader.HasBeenLodgedAtCustoms && !ShouldCalculateMPFAndDutyDateForImmediateDelivery))
					{
						result = entryHeader.US_DutyCalcDate.Date;
					}
				}

				if (result.IsEmpty && US_ImmediateDelivery)
				{
					result = ZDate.Today;
				}

				return result;
			}
		}

		#endregion

		#region IDocManagerSupport Members
		protected override DeclarationDocManagerInfo GetNewDocManagerInfo()
		{
			return new USDeclarationDocManagerInfo(this) { UseBusinessEntityFactoryAsInternal = true };
		}
		#endregion

		#region IAddInfoChildSupporter Members

		public JobUSDeclaration USDeclaration => this.LoadOrCreateAddInfoChild(ref usDeclaration);
		JobUSDeclaration usDeclaration;

		protected override BusinessObject GetAddInfoChild() => USDeclaration;
		protected override SchemaGuidColumn GetChildForeignKeyColumn() => JobUSDeclarationSchema.USD_JE;

		#endregion

		#region USDeclaration Members

		public ZBool USD_RetailSalesSubstitutionIndicator
		{
			set => USDeclaration.USD_RetailSalesSubstitutionIndicator = value;
			get => USDeclaration.USD_RetailSalesSubstitutionIndicator;
		}

		public ZPropertyInfo USD_RetailSalesSubstitutionIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.USD_RetailSalesSubstitutionIndicator, x => USDeclaration.USD_RetailSalesSubstitutionIndicatorInfo); }
		}

		public OrgHeader FPPI => Factory.Load<OrgHeader>(USD_OH_ForeignPrincipalParty);

		public ZGuid USD_OH_ForeignPrincipalParty
		{
			set => USDeclaration.USD_OH_ForeignPrincipalParty = value;
			get => USDeclaration.USD_OH_ForeignPrincipalParty;
		}

		public ZPropertyInfo USD_OH_ForeignPrincipalPartyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.USD_OH_ForeignPrincipalParty, x => USDeclaration.USD_OH_ForeignPrincipalPartyInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AddInfoJobDeclaration|US_TIBMotorVehicles", Caption = "Motor Vehicles?")]
		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobDeclarationLookups.US_YesNoList))]
		public override ZString US_TIBMotorVehicles
		{
			get { return base.US_TIBMotorVehicles; }
			set
			{
				base.US_TIBMotorVehicles = value;

				if (!(base.US_TIBMotorVehicles == YesNoList.Codes.Yes))
				{
					US_TIBMVNonConforming = false;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AddInfoJobDeclaration|US_TIBMVNonConforming", Caption = "Non-Conforming?")]
		public override ZBool US_TIBMVNonConforming
		{
			get { return base.US_TIBMVNonConforming; }
			set
			{
				var isChanged = base.US_TIBMVNonConforming != value;
				base.US_TIBMVNonConforming = value;
				if (isChanged && US_BondType == BondTypeList.Codes.SingleTransactionBond)
				{
					US_BondCalcCode = ZString.Empty;
					DefaultBondCalcCode();
				}
			}
		}

		#endregion

		#region Customs Rule

		public override ZDate GetDateForCustomsRuleFilter()
		{
			return DateForFeeCalculation.Date;
		}

		public override ZDecimal CustomsValueForCustomsRuleValidation => ActiveEntryHeaders.EntrySummaryEntry?.CustomsValue ?? ZDecimal.Zero;

		public override ZBool IsBrokerToPay => BrokerToPayIndicator == YesNoDefaultList.Codes.Yes;

		public override ZBool IsImporterToPay => BrokerToPayIndicator == YesNoDefaultList.Codes.No;

		public override ZDecimal DisbursementAmount => TotalPayable;

		public override ZDecimal TotalDutyAmount => new ZDecimal(Invoices.Sum(x => x.JobComInvoiceLines.OfType<JobComInvoiceLine>().Sum(y => y.JI_Calc_DutyAmount)));

		#endregion

		#region US Low Value Transfered Log

		public void AddUSLowValueTransferredLog(ZString lowValueReference)
		{
			Logs.AddNew(AutoEvents.Transferred,
			[
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, usLowValue),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, lowValueReference)
			]);
		}

		public bool IsCreatedFromUSLowValue => USLowValueTransferredLog != null;

		StmALog USLowValueTransferredLog => Factory.GetValue(ref usLowValueTransferredLog, GetUSLowValueTransferredLog);

		CachedProperty<StmALog> usLowValueTransferredLog;

		StmALog GetUSLowValueTransferredLog() => Logs.MostRecentLogByEventTime(
			AutoEvents.Transferred,
			x => x.Parameters.TryGetValue(EventReferenceParameters.Codes.Type, out var type) && type == usLowValue
		);

		readonly string usLowValue = "USLV";

		#endregion
	}
}
