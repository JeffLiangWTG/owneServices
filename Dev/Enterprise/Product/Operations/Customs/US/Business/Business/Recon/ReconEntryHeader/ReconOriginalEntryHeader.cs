using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(ReconOriginalEntryHeader.Schema.CH_OrigEntryReference)]
	[DescriptionProperty(ReconOriginalEntryHeader.Schema.CH_OrigEntryReference)]
	public class ReconOriginalEntryHeader : NonPersistentBusinessObjectWithLogsAndNotes,
		IObsoleteValidation,
		ICodeDescription,
		IIdentified
	{
		public static class Schema
		{
			public const string US_ImportDate = USAddInfoSchema.Constants.US_ImportDate;
			public const string US_SchDEntry = USAddInfoSchema.Constants.US_SchDEntry;
			public const string US_PaymentDate = USAddInfoSchema.Constants.US_PaymentDate;
			public const string US_R_DateForMPFCalc = USAddInfoSchema.Constants.US_R_DateForMPFCalc;
			public const string US_R_DutyRateDate = USAddInfoSchema.Constants.US_R_DutyRateDate;
			public const string US_R_IsHMFApplicable = USAddInfoSchema.Constants.US_R_IsHMFApplicable;
			public const string US_R_GoodsDescription = USAddInfoSchema.Constants.US_R_GoodsDescription;
			public const string US_R_OwnerRef = USAddInfoSchema.Constants.US_R_OwnerRef;
			public const string US_R_ReleaseDate = USAddInfoSchema.Constants.US_R_ReleaseDate;
			public const string US_R_MsgMode = USAddInfoSchema.Constants.US_R_MsgMode;
			public const string US_R_CalcOrigDuty = USAddInfoSchema.Constants.US_R_CalcOrigDuty;
			public const string US_R_CottonFeeMandatory = USAddInfoSchema.Constants.US_R_CottonFeeMandatory;
			public const string US_R_MonthlyFiling = USAddInfoSchema.Constants.US_R_MonthlyFiling;
			public const string US_R_ChangedLinesOnly = USAddInfoSchema.Constants.US_R_ChangedLinesOnly;
			public const string US_R_OrigCV = USAddInfoSchema.Constants.US_R_OrigCV;
			public const string US_R_NoLineDetails = USAddInfoSchema.Constants.US_R_NoLineDetails;
			public const string US_NAFTAReconIndicator = USAddInfoSchema.Constants.US_NAFTAReconIndicator;

			public const string CH_OriginalDeclarationReference = "CH_OriginalDeclarationReference";
			public const string CH_OrigEntryReference = "CH_OrigEntryReference";

			public const string MPC = "MPC";
			public const string OriginalDuty = "OriginalDuty";
			public const string OriginalTax = "OriginalTax";
			public const string OriginalFee = "OriginalFee";
			public const string ReconDuty = "ReconDuty";
			public const string ReconTax = "ReconTax";
			public const string ReconFee = "ReconFee";
			public const string ReconInterest = "ReconInterest";
			public const string US_PriorDisclosure = USAddInfoSchema.Constants.US_PriorDisclosure;
			public const string US_NAFTAClaimStat = USAddInfoSchema.Constants.US_NAFTAClaimStat;
			public const string US_ProtestStat = USAddInfoSchema.Constants.US_ProtestStat;
			public const string US_ProtestID = USAddInfoSchema.Constants.US_ProtestID;
			public const string US_PendingActionIDType = USAddInfoSchema.Constants.US_PendingActionIDType;
			public const string US_PendingActionID = USAddInfoSchema.Constants.US_PendingActionID;

			public const int CH_OrigEntryReferenceMaxLength = 11;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ReconOriginalEntryHeader(CusEntryHeader reconEntry, ReconDeclaration reconDeclaration)
			: base(reconEntry.Factory)
		{
			this.reconEntry = reconEntry;
			this.reconEntry.ReconOriginalEntry = this;
			base.RegisterEditableChildObject(this.reconEntry);
			this.ReconDeclaration = reconDeclaration;

			if (!this.reconEntry.IsInDatabase)
			{
				using (SuspendSettingHasChanges())
				{
					this.reconEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
					US_R_MsgMode = JobApplicationCodeList.Codes.ACE;
				}
			}
		}

		readonly CusEntryHeader reconEntry;
		public readonly ReconDeclaration ReconDeclaration;

		protected override BusinessObject LogsAndNotesTarget
		{
			get { return reconEntry; }
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#region Properties

		public bool HasMPFChanges => OriginalMPF != ReconMPF;

		public ZGuid CH_PK
		{
			get { return reconEntry.PK; }
		}

		public ZGuid CH_JE
		{
			get { return reconEntry.CH_JE; }
			set { reconEntry.CH_JE = value; }
		}

		/// <summary>
		/// Original import entry this recon entry is reconciled against
		/// </summary>
		public ZGuid CH_CH_OriginalEntry
		{
			get { return reconEntry.CH_CH_PrimeEntry; }
			set
			{
				var oldValue = CH_CH_OriginalEntry;
				reconEntry.CH_CH_PrimeEntry = value;
				if (oldValue != CH_CH_OriginalEntry)
				{
					originalDeclaration = null;
				}
			}
		}

		public ReconOriginalDeclaration OriginalDeclaration
		{
			get
			{
				if (originalDeclaration == null)
				{
					var originalEntryPK = CH_CH_OriginalEntry;
					if (originalEntryPK.IsValid)
					{
						originalDeclaration = ReconDeclaration.OriginalDeclarations.Find(originalEntryPK);
					}
				}
				return originalDeclaration;
			}
		}
		ReconOriginalDeclaration originalDeclaration;

		public ZString CH_OriginalDeclarationReference
		{
			get
			{
				var result = ZString.Empty;
				var originalDeclaration = OriginalDeclaration;
				if (originalDeclaration != null)
				{
					result = originalDeclaration.JE_DeclarationReference;
				}
				return result;
			}
		}

		public ZPropertyInfo CH_OriginalDeclarationReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.CH_OriginalDeclarationReference); }
		}

		/// <summary>
		/// Entry Filer Code + Entry Number
		/// </summary>
		[List(nameof(Lookups) + "." + nameof(ReconOriginalEntryHeaderLookups.Entries))]
		public ZString CH_OrigEntryReference
		{
			get { return reconEntry.CH_BGMReference; }
			set
			{
				if (CH_OrigEntryReference != value)
				{
					CusEntryHeader originalEntry = null;

					if (value.Length == 11)
					{
						ZString entryFilerCode = value.Left(3);
						ZString entryNumber = value.SubstringSafe(3);
						originalEntry = new CusEntryHeader.Loader(ReconDeclaration.ReadFactory).FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, entryNumber, entryFilerCode, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
					}

					CH_CH_OriginalEntry = originalEntry == null ? ZGuid.Empty : originalEntry.PK;

					if (!CH_CH_OriginalEntry.IsEmpty)
					{
						ReconImportEntryRetriever reconImportEntryRetriever = new ReconImportEntryRetriever(this.ReconDeclaration);
						reconImportEntryRetriever.ImportEntryDetails(this);
					}

					//at the end so that validation triggers after the rest of the data is set up
					reconEntry.CH_BGMReference = value;
				}
			}
		}

		public ZPropertyInfo CH_OrigEntryReferenceInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CH_OrigEntryReference, x => reconEntry.CH_BGMReferenceInfo); }
		}

		public ZBool US_NAFTAReconIndicator
		{
			get => AddInfo.US_NAFTAReconIndicator;
			set
			{
				if (ZZCustomsFunctionality.USFTAReconIndIsValid)
				{
					AddInfo.US_NAFTAReconIndicator = value;
				}
			}
		}

		public ZPropertyInfo US_NAFTAReconIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NAFTAReconIndicator, x => AddInfo.US_NAFTAReconIndicatorInfo); }
		}

		public ZDecimal US_R_OrigCV
		{
			get => AddInfo.US_R_OrigCV;
			set => AddInfo.US_R_OrigCV = value;
		}

		public ZPropertyInfo US_R_OrigCVInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_OrigCV, x => AddInfo.US_R_OrigCVInfo); }
		}

		public bool US_R_OrigCV_ReadOnly => !(US_R_ChangedLinesOnly && US_R_IsHMFApplicable == YesNoDefaultList.Codes.Yes);

		public ZBool US_R_ChangedLinesOnly
		{
			get => AddInfo.US_R_ChangedLinesOnly;
			set
			{
				var hasChanges = US_R_ChangedLinesOnly != value;
				AddInfo.US_R_ChangedLinesOnly = value;
				if (hasChanges && !IsCopying)
				{
					var chargeMPC = OriginalCharges.Cast<ReconEntryOriginalCharge>().FirstOrDefault(x => x.CY_Code == Core.Constants.USCustoms.FeeCodes.MPC);
					if (US_R_ChangedLinesOnly)
					{
						US_R_NoLineDetails = false;
						US_R_CalcOrigDuty = false;
						US_R_MonthlyFiling = false;

						if (chargeMPC == null)
						{
							chargeMPC = OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MPC);
						}
					}
					else
					{
						if (chargeMPC != null)
						{
							chargeMPC.CY_Amount = 0m;
						}
					}
					UpdateChargesReadOnlyState();
				}
			}
		}

		public ZPropertyInfo US_R_ChangedLinesOnlyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_ChangedLinesOnly, x => AddInfo.US_R_ChangedLinesOnlyInfo); }
		}

		public ZBool US_R_NoLineDetails
		{
			get { return AddInfo.US_R_NoLineDetails; }
			set
			{
				AddInfo.US_R_NoLineDetails = value;
				if (US_R_NoLineDetails)
				{
					US_R_IsHMFApplicable = ZString.Empty;
					US_R_CalcOrigDuty = false;
				}

				UpdateChargesReadOnlyState();
			}
		}
		public ZPropertyInfo US_R_NoLineDetailsInfo => GetWrappedZPropertyInfo(Schema.US_R_NoLineDetails, x => AddInfo.US_R_NoLineDetailsInfo);

		public bool US_R_NoLineDetails_ReadOnly => US_R_ChangedLinesOnly;

		public ZBool US_R_CalcOrigDuty
		{
			get { return AddInfo.US_R_CalcOrigDuty; }
			set { AddInfo.US_R_CalcOrigDuty = value; }
		}

		public bool US_R_CalcOrigDuty_ReadOnly => US_R_NoLineDetails || US_R_ChangedLinesOnly;

		public ZPropertyInfo US_R_CalcOrigDutyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_CalcOrigDuty, x => AddInfo.US_R_CalcOrigDutyInfo); }
		}

		[ReadOnlyMember(nameof(US_R_CottonFeeMandatory_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.ReconOriginalEntryHeader|US_R_CottonFeeMandatory", Caption = "Cotton Fee Mandatory", FullDescription = "If this is ticked, then cotton fee will always be sent in reconciliation message even it's exempted.")]
		public ZBool US_R_CottonFeeMandatory
		{
			get { return AddInfo.US_R_CottonFeeMandatory; }
			set { AddInfo.US_R_CottonFeeMandatory = value; }
		}

		public ZPropertyInfo US_R_CottonFeeMandatoryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_CottonFeeMandatory, x => AddInfo.US_R_CottonFeeMandatoryInfo); }
		}

		public bool US_R_CottonFeeMandatory_ReadOnly
		{
			get { return !IsACE; }
		}

		public bool IsACE
		{
			get { return US_R_MsgMode == JobApplicationCodeList.Codes.ACE; }
		}

		[List(nameof(Lookups) + "." + nameof(ReconOriginalEntryHeaderLookups.MessagingModeList))]
		public ZString US_R_MsgMode
		{
			get { return AddInfo.US_R_MsgMode; }
			set
			{
				var oldValue = US_R_MsgMode;
				AddInfo.US_R_MsgMode = value;
				if (oldValue != value && !IsCopying)
				{
					if (!IsACE)
					{
						US_R_CottonFeeMandatory = false;
					}
				}
			}
		}

		public ZPropertyInfo US_R_MsgModeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_MsgMode, x => AddInfo.US_R_MsgModeInfo); }
		}

		public void UpdateChargesReadOnlyState()
		{
			//OriginalCharges should be read/write when NoChange is indicated and no invoices exist OR when aggregate recon with no entry level duties and fees input.
			bool readOnly = ShouldDutiesFeesBeCalculated && !US_R_ChangedLinesOnly;
			OriginalCharges.UpdateFromReadOnlyState(readOnly);
		}

		public bool ShouldDutiesFeesBeCalculated
		{
			get
			{
				bool result = false;

				if (!US_R_NoLineDetails)
				{
					JobComInvoiceHeader orphanInvoiceDontCare;
					var invoiceLoaded = GetInvoice(out orphanInvoiceDontCare);
					result = invoiceLoaded != null && invoiceLoaded.InvoiceLines.Count > 0;
				}

				return result;
			}
		}

		public ZString ImportSourceIndicator
		{
			get
			{
				ZString result = "0";

				if (SchDEntry != null)
				{
					var state = SchDEntry.GetAttribute(RefCusCodeListAttributeTypes.Codes.State);
					if (state == "PR")
					{
						result = ReconciliationImportEntrySourceList.Codes.PuertoRico;
					}
					else if (state == "VI")
					{
						result = ReconciliationImportEntrySourceList.Codes.VirginIslands;
					}
					else
					{
						result = ReconciliationImportEntrySourceList.Codes.FiftyStates;
					}
				}
				return result;
			}
		}

		public ZDateTime US_ImportDate
		{
			get { return AddInfo.US_ImportDate; }
			set
			{
				AddInfo.US_ImportDate = value;

				DefaultDutyRateDateIfRequired();

				ReconDeclaration.RefreshEarliestLatestImportDate();
			}
		}

		public ZPropertyInfo US_ImportDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ImportDate, x => AddInfo.US_ImportDateInfo); }
		}

		/// <summary>
		/// Entry Summary Date
		/// </summary>
		public ZDateTime US_PaymentDate
		{
			get { return AddInfo.US_PaymentDate; }
			set
			{
				AddInfo.US_PaymentDate = value;

				DefaultDutyRateDateIfRequired();
			}
		}

		public ZPropertyInfo US_PaymentDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PaymentDate, x => AddInfo.US_PaymentDateInfo); }
		}

		public bool ShouldCalculateInterestAmount
		{
			get { return !ReconDeclaration.US_IsAggregate && HasBeenShortPaid; }
		}

		public bool HasBeenShortPaid
		{
			get
			{
				if (hasBeenShortPaidCached == null)
				{
					hasBeenShortPaidCached = new CachedProperty<bool>(Factory, delegate
						{
							return (ReconDuty + ReconFee + ReconTax) - (OriginalDuty + OriginalFee + OriginalTax) > 0;
						}
					);
				}
				return hasBeenShortPaidCached.Value;
			}
		}
		CachedProperty<bool> hasBeenShortPaidCached;

		public ZDateTime US_R_ReleaseDate
		{
			get { return AddInfo.US_R_ReleaseDate; }
			set
			{
				AddInfo.US_R_ReleaseDate = value;

				DefaultDutyRateDateIfRequired();

				if (US_R_DateForMPFCalc.IsEmpty)
				{
					US_R_DateForMPFCalc = US_R_ReleaseDate;
				}
			}
		}

		void DefaultDutyRateDateIfRequired()
		{
			if (US_R_DutyRateDate.IsEmpty)
			{
				var dateToDefault = US_R_ReleaseDate;

				if (dateToDefault.IsEmpty)
				{
					dateToDefault = US_PaymentDate;
				}

				if (dateToDefault.IsEmpty)
				{
					dateToDefault = US_ImportDate;
				}

				if (!dateToDefault.IsEmpty)
				{
					US_R_DutyRateDate = dateToDefault;
				}
			}
		}

		public ZPropertyInfo US_R_ReleaseDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_ReleaseDate, x => AddInfo.US_R_ReleaseDateInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(ReconOriginalEntryHeaderLookups.SchDPortList))]
		public ZString US_SchDEntry
		{
			get { return AddInfo.US_SchDEntry; }
			set { AddInfo.US_SchDEntry = value; }
		}
		public ZPropertyInfo US_SchDEntryInfo => GetWrappedZPropertyInfo(Schema.US_SchDEntry, x => AddInfo.US_SchDEntryInfo);

		public ZZRefCusCodeListCombined SchDEntry
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_SchDEntry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZDateTime US_R_DateForMPFCalc
		{
			get { return AddInfo.US_R_DateForMPFCalc; }
			set { AddInfo.US_R_DateForMPFCalc = value; }
		}

		public ZPropertyInfo US_R_DateForMPFCalcInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_DateForMPFCalc, x => AddInfo.US_R_DateForMPFCalcInfo); }
		}

		public ZDateTime US_R_DutyRateDate
		{
			get { return AddInfo.US_R_DutyRateDate; }
			set { AddInfo.US_R_DutyRateDate = value; }
		}

		public ZPropertyInfo US_R_DutyRateDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_DutyRateDate, x => AddInfo.US_R_DutyRateDateInfo); }
		}

		public ZString US_R_IsHMFApplicable
		{
			get { return AddInfo.US_R_IsHMFApplicable; }
			set { AddInfo.US_R_IsHMFApplicable = value; }
		}

		public ZPropertyInfo US_R_IsHMFApplicableInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_IsHMFApplicable, x => AddInfo.US_R_IsHMFApplicableInfo); }
		}

		public ZString US_R_OwnerRef
		{
			get { return AddInfo.US_R_OwnerRef; }
			set { AddInfo.US_R_OwnerRef = value; }
		}

		public ZPropertyInfo US_R_OwnerRefInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_OwnerRef, x => AddInfo.US_R_OwnerRefInfo); }
		}

		public ZString US_R_GoodsDescription
		{
			get { return AddInfo.US_R_GoodsDescription; }
			set { AddInfo.US_R_GoodsDescription = value; }
		}

		public ZPropertyInfo US_R_GoodsDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_R_GoodsDescription, x => AddInfo.US_R_GoodsDescriptionInfo); }
		}

		public ZDecimal MPC
		{
			get => OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MPC);
			set
			{
				OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.MPC, value);
				Validation.ValidateMPC();
			}
		}

		public ZPropertyInfo MPCInfo
		{
			get { return GetZPropertyInfo(Schema.MPC); }
		}

		public bool MPC_ReadOnly => !US_R_ChangedLinesOnly;

		public ZDecimal OriginalDuty
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty); }
		}

		public ZPropertyInfo OriginalDutyInfo
		{
			get { return GetZPropertyInfo(Schema.OriginalDuty); }
		}

		public ZDecimal ReconDuty
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty); }
		}

		public ZPropertyInfo ReconDutyInfo
		{
			get { return GetZPropertyInfo(Schema.ReconDuty); }
		}

		public ZDecimal OriginalTax
		{
			get { return OriginalCharges.GetTotal(CusFeeCodeConstants.GetTaxCodes()); }
		}

		public ZPropertyInfo OriginalTaxInfo
		{
			get { return GetZPropertyInfo(Schema.OriginalTax); }
		}

		public ZDecimal ReconTax
		{
			get { return ReconCharges.GetTotalAmount(CusFeeCodeConstants.GetTaxCodes()); }
		}

		public ZPropertyInfo ReconTaxInfo
		{
			get { return GetZPropertyInfo(Schema.ReconTax); }
		}

		public ZDecimal OriginalFee
		{
			get { return OriginalCharges.GetTotal(EntryChargeTypeList.GetFeeCodes()); }
		}

		public ZPropertyInfo OriginalFeeInfo
		{
			get { return GetZPropertyInfo(Schema.OriginalFee); }
		}

		public ZDecimal ReconFee
		{
			get { return ReconCharges.GetTotalAmount(EntryChargeTypeList.GetFeeCodes()); }
		}

		public ZPropertyInfo ReconFeeInfo
		{
			get { return GetZPropertyInfo(Schema.ReconFee); }
		}

		public ZDecimal ReconInterest
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest); }
		}

		public ZPropertyInfo ReconInterestInfo
		{
			get { return GetZPropertyInfo(Schema.ReconInterest); }
		}

		public ZDecimal OriginalMPF
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing); }
		}

		public ZDecimal ReconMPF
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing); }
		}

		public ZDecimal OriginalHMF
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF); }
		}

		public ZDecimal ReconHMF
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF); }
		}

		public ZDecimal OriginalAvocado
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado); }
		}

		public ZDecimal ReconAvocado
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado); }
		}

		public ZDecimal OriginalBeef
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Beef); }
		}

		public ZDecimal ReconBeef
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Beef); }
		}

		public ZDecimal OriginalBlueberry
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry); }
		}

		public ZDecimal ReconBlueberry
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry); }
		}

		public ZDecimal OriginalCotton
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton); }
		}

		public ZDecimal ReconCotton
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton); }
		}

		public ZDecimal OriginalDairy
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee); }
		}

		public ZDecimal ReconDairy
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee); }
		}

		public ZDecimal OriginalSpirits
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits); }
		}

		public ZDecimal ReconSpirits
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits); }
		}

		public ZDecimal OriginalDutiableMail
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail); }
		}

		public ZDecimal ReconDutiableMail
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail); }
		}

		public ZDecimal OriginalSorghum
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Sorghum); }
		}

		public ZDecimal ReconSorghum
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Sorghum); }
		}

		public ZDecimal OriginalOtherAgencies
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies); }
		}

		public ZDecimal ReconOtherAgencies
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies); }
		}

		public ZDecimal OriginalLimes
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes); }
		}

		public ZDecimal ReconLimes
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes); }
		}

		public ZDecimal OriginalHoney
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey); }
		}

		public ZDecimal ReconHoney
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey); }
		}

		public ZDecimal OriginalMango
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango); }
		}

		public ZDecimal ReconMango
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango); }
		}

		public ZDecimal OriginalInformal
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal); }
		}

		public ZDecimal ReconInformal
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal); }
		}

		public ZDecimal OriginalSurcharge
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge); }
		}

		public ZDecimal ReconSurcharge
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge); }
		}

		public ZDecimal OriginalMushroom
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom); }
		}

		public ZDecimal ReconMushroom
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom); }
		}

		public ZDecimal OriginalOtherExcise
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise); }
		}

		public ZDecimal ReconOtherExcise
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise); }
		}

		public ZDecimal OriginalRaspberry
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry); }
		}

		public ZDecimal ReconRaspberry
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry); }
		}

		public ZDecimal OriginalPork
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Pork); }
		}

		public ZDecimal ReconPork
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Pork); }
		}

		public ZDecimal OriginalPotato
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Potato); }
		}

		public ZDecimal ReconPotato
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Potato); }
		}

		public ZDecimal OriginalSoftwoodLumber
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber); }
		}

		public ZDecimal ReconSoftwoodLumber
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber); }
		}

		public ZDecimal OriginalSugar
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Sugar); }
		}

		public ZDecimal ReconSugar
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Sugar); }
		}

		public ZDecimal OriginalTobacco
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco); }
		}

		public ZDecimal ReconTobacco
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco); }
		}

		public ZDecimal OriginalWatermelon
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon); }
		}

		public ZDecimal ReconWatermelon
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon); }
		}

		public ZDecimal OriginalWines
		{
			get { return OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines); }
		}

		public ZDecimal ReconWines
		{
			get { return ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Wines); }
		}

		public ZString DeclarationBondNo
		{
			get
			{
				var originalDeclaration = OriginalDeclaration;
				return originalDeclaration != null ? originalDeclaration.US_BondProducerAccNo : ZString.Empty;
			}
		}

		#region US_PriorDisclosure
		public ZBool US_PriorDisclosure
		{
			get { return AddInfo.US_PriorDisclosure; }
			set { AddInfo.US_PriorDisclosure = value; }
		}
		public ZPropertyInfo US_PriorDisclosureInfo => GetWrappedZPropertyInfo(Schema.US_PriorDisclosure, x => AddInfo.US_PriorDisclosureInfo);
		#endregion

		#region US_NAFTAClaimStat 
		public ZBool US_NAFTAClaimStat
		{
			get { return AddInfo.US_NAFTAClaimStat; }
			set { AddInfo.US_NAFTAClaimStat = value; }
		}
		public ZPropertyInfo US_NAFTAClaimStatInfo => GetWrappedZPropertyInfo(Schema.US_NAFTAClaimStat, x => AddInfo.US_NAFTAClaimStatInfo);
		#endregion

		#region US_ProtestStat
		public ZBool US_ProtestStat
		{
			get { return AddInfo.US_ProtestStat; }
			set { AddInfo.US_ProtestStat = value; }
		}
		public ZPropertyInfo US_ProtestStatInfo => GetWrappedZPropertyInfo(Schema.US_ProtestStat, x => AddInfo.US_ProtestStatInfo);
		#endregion

		#region US_ProtestID
		public ZString US_ProtestID
		{
			get { return AddInfo.US_ProtestID; }
			set { AddInfo.US_ProtestID = value; }
		}
		public ZPropertyInfo US_ProtestIDInfo => GetWrappedZPropertyInfo(Schema.US_ProtestID, x => AddInfo.US_ProtestIDInfo);
		#endregion

		#region US_PendingActionIDType
		[List(nameof(Lookups) + "." + nameof(ReconOriginalEntryHeaderLookups.PendingActionTypeList))]
		public ZString US_PendingActionIDType
		{
			get { return AddInfo.US_PendingActionIDType; }
			set { AddInfo.US_PendingActionIDType = value; }
		}
		public ZPropertyInfo US_PendingActionIDTypeInfo => GetWrappedZPropertyInfo(Schema.US_PendingActionIDType, x => AddInfo.US_PendingActionIDTypeInfo);
		#endregion

		#region US_PendingActionID
		public ZString US_PendingActionID
		{
			get { return AddInfo.US_PendingActionID; }
			set { AddInfo.US_PendingActionID = value; }
		}
		public ZPropertyInfo US_PendingActionIDInfo => GetWrappedZPropertyInfo(Schema.US_PendingActionID, x => AddInfo.US_PendingActionIDInfo);
		#endregion

		#region US_R_MonthlyFiling

		public ZBool US_R_MonthlyFiling
		{
			get { return AddInfo.US_R_MonthlyFiling; }
			set
			{
				var oldValue = AddInfo.US_R_MonthlyFiling;
				if (oldValue != value)
				{
					AddInfo.US_R_MonthlyFiling = value;
					OriginalCharges.Cast<ReconEntryOriginalCharge>().ForEach(x => x.CY_AmountInfo.RefreshBinding());
					ReconCharges.Cast<CusEntryHeaderCharges>().ForEach(x => x.C1_ChargeAmountInfo.RefreshBinding());
				}
			}
		}

		public ZPropertyInfo US_R_MonthlyFilingInfo => GetWrappedZPropertyInfo(Schema.US_R_MonthlyFiling, x => AddInfo.US_R_MonthlyFilingInfo);

		public bool US_R_MonthlyFiling_ReadOnly => US_R_ChangedLinesOnly;

		#endregion

		#endregion

		#region Methods

		public bool Wraps(CusEntryHeader entryPassed)
		{
			return reconEntry == entryPassed;
		}

		public void ResetReconChargesIfNecessary()
		{
			if (ShouldDutiesFeesBeCalculated)
			{
				var candidatesReconChargeForDelete = new List<CusEntryHeaderCharges>();
				foreach (CusEntryHeaderCharges charge in ReconCharges)
				{
					if (!IsFeeUserEntered(charge.C1_ChargeType))
					{
						candidatesReconChargeForDelete.Add(charge);
					}
				}

				DeleteReconCharges(candidatesReconChargeForDelete);

				var candidatesChargeForDelete = new List<ReconEntryOriginalCharge>();
				foreach (ReconEntryOriginalCharge charge in OriginalCharges)
				{
					if (!charge.CY_IsOverridden && !IsFeeUserEntered(charge.CY_Code))
					{
						candidatesChargeForDelete.Add(charge);
					}
				}

				DeleteCharges(candidatesChargeForDelete);

				ResetReconInvoiceLinesChargesAndFees();
			}
		}

		static void DeleteReconCharges(List<CusEntryHeaderCharges> candidatesReconChargeForDelete)
		{
			foreach (var charge in candidatesReconChargeForDelete)
			{
				charge.Delete();
			}
		}

		bool IsFeeUserEntered(ZString feeType) => US_R_MonthlyFiling && feeType == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;

		void DeleteCharges(List<ReconEntryOriginalCharge> candidatesChargeForDelete)
		{
			foreach (var charge in candidatesChargeForDelete)
			{
				charge.Delete();
			}
		}

		void ResetReconInvoiceLinesChargesAndFees()
		{
			foreach (JobComInvoiceLine invoiceLine in Invoice.JobComInvoiceLines)
			{
				if (!invoiceLine.US_OverrideDuty)
				{
					invoiceLine.US_Duty = ZDecimal.Zero;
				}

				if (!invoiceLine.US_OverrideSupDuty)
				{
					invoiceLine.US_SupDuty = ZDecimal.Zero;
				}

				if (US_R_CalcOrigDuty)
				{
					if (!invoiceLine.US_R_OrigOverrideDuty)
					{
						invoiceLine.US_R_OrigDuty = ZDecimal.Zero;
					}

					if (!invoiceLine.US_R_OrigOverrideSupDuty)
					{
						invoiceLine.US_R_OrigSupDuty = ZDecimal.Zero;
					}

					var candidatesChargeForDelete = new List<ReconEntryOriginalCharge>();

					foreach (ReconEntryOriginalCharge charge in invoiceLine.ReconOriginalCharges)
					{
						if (!charge.CY_IsOverridden)
						{
							candidatesChargeForDelete.Add(charge);
						}
					}
					DeleteCharges(candidatesChargeForDelete);
				}

				var candidatesFeesForDelete = new List<FeeCusCodeData>();
				foreach (FeeCusCodeData charge in invoiceLine.FeeCusCodes)
				{
					if (!charge.CY_IsOverridden)
					{
						if (invoiceLine.ReconOriginalCharges.ContainsCode(charge.CY_Code))
						{
							charge.CY_FeeAmount = ZDecimal.Zero;
						}
						else
						{
							candidatesFeesForDelete.Add(charge);
						}
					}
				}

				foreach (var fee in candidatesFeesForDelete)
				{
					fee.Delete();
				}
			}
		}

		public CusEntryHeader GetWrappedEntry()
		{
			return reconEntry;
		}

		internal void AddAggregateFeesIfNecessary()
		{
			if (ReconDeclaration.IsNoChangeAggregate)
			{
				var feeList = new List<ZString>();
				foreach (ReconEntryOriginalCharge originalCharge in OriginalCharges)
				{
					AddIfFee(feeList, originalCharge);
				}

				foreach (CusEntryHeaderCharges reconCharge in ReconCharges)
				{
					AddIfFee(feeList, reconCharge);
				}

				feeList.ForEach(x => ReconDeclaration.AggregateRefundedFees.AddNewIfNotExists(x));
			}
		}

		void AddIfFee(List<ZString> feeList, IFee fee)
		{
			ZString code = fee.Code;
			if (!CusFeeCodeConstants.IsExciseTax(code) && CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory).ContainsCode(code))
			{
				if (!feeList.Contains(code))
				{
					feeList.Add(code);
				}
			}
		}

		#endregion

		#region Calculate Original Customs Fees

		public void RefreshEntryWithTotalOriginalCustomsFees()
		{
			totalFeeAmountDict = null;
		}

		public ZDecimal GetTotalOriginalCustomsFeesFromFeeCode(ZString feeCode)
		{
			if (totalFeeAmountDict == null)
			{
				totalFeeAmountDict = new Dictionary<ZString, ZDecimal>();

				foreach (ReconEntryOriginalCharge originalCharge in OriginalCharges)
				{
					var chargeType = originalCharge.CY_Code;
					var chargeAmount = ZDecimal.Zero;

					if (!totalFeeAmountDict.TryGetValue(chargeType, out chargeAmount))
					{
						if (chargeType == Core.Constants.USCustoms.FeeCodes.Duty)
						{
							foreach (JobComInvoiceLine entryLine in Invoice.JobComInvoiceLines)
							{
								chargeAmount += entryLine.US_R_OrigDuty + entryLine.US_R_OrigSupDuty;
							}
						}
						else
						{
							foreach (JobComInvoiceLine entryLine in Invoice.JobComInvoiceLines)
							{
								chargeAmount += entryLine.ReconOriginalCharges.GetAmount(chargeType);
							}
						}
						totalFeeAmountDict.Add(chargeType, chargeAmount);
					}
				}
			}

			var totalFeeAmount = ZDecimal.Zero;
			totalFeeAmountDict.TryGetValue(feeCode, out totalFeeAmount);
			return totalFeeAmount;
		}
		Dictionary<ZString, ZDecimal> totalFeeAmountDict;

		#endregion

		#region Related Object/Collection

		AddInfoCusEntryHeader AddInfo
		{
			get { return addInfo ?? (addInfo = reconEntry.GetAddInfo()); }
		}
		AddInfoCusEntryHeader addInfo;

		[ChildEditable(false)]
		public ReconEntryOriginalChargeCollection OriginalCharges
		{
			get
			{
				if (originalCharges == null)
				{
					originalCharges = new ReconEntryOriginalChargeCollection(reconEntry);
					originalCharges.Load();
					RegisterEditableChildObject(originalCharges);
					originalCharges.Sort(CusCodeDataSchema.CY_Code.Name, System.ComponentModel.ListSortDirection.Ascending);
				}

				return originalCharges;
			}
		}
		ReconEntryOriginalChargeCollection originalCharges;

		[ChildEditable(false)]
		public CusEntryHeaderChargesCollection ReconCharges
		{
			get
			{
				if (reconCharges == null)
				{
					reconCharges = reconEntry.Charges;
					reconCharges.Sort(CusEntryHeaderChargesSchema.C1_ChargeType.Name, System.ComponentModel.ListSortDirection.Ascending);
				}

				return reconCharges;
			}
		}
		CusEntryHeaderChargesCollection reconCharges;

		public ReconOriginalEntryHeaderLookups Lookups
		{
			get { return new ReconOriginalEntryHeaderLookups(this); }
		}

		public ReconOriginalEntryHeaderValidation Validation
		{
			get { return new ReconOriginalEntryHeaderValidation(this); }
		}

		[ChildEditable(true)]
		public ReconRefundedChargeCollection RefundedFees
		{
			get
			{
				if (refundedCharges == null)
				{
					refundedCharges = new ReconRefundedChargeCollection(reconEntry);
					refundedCharges.Load();
					RegisterEditableChildObject(refundedCharges);
					refundedCharges.Sort(CusCodeDataSchema.CY_Code.Name, System.ComponentModel.ListSortDirection.Ascending);
				}

				return refundedCharges;
			}
		}
		ReconRefundedChargeCollection refundedCharges;

		[ChildEditable(true)]
		public JobComInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null || invoice.IsDeleted)
				{
					invoice = GetOrCreateInvoice();
					invoice.InvoiceLines.CountChanged += new CollectionCountChangedEventHandler(InvoiceLines_CountChanged);
					RegisterEditableChildObject(invoice);
				}
				return invoice;
			}
		}
		JobComInvoiceHeader invoice;

		void InvoiceLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateChargesReadOnlyState();
		}

		JobComInvoiceHeader GetOrCreateInvoice()
		{
			JobComInvoiceHeader result = null;
			JobComInvoiceHeader orphanInvoice;

			result = GetInvoice(out orphanInvoice);

			if (result == null)
			{
				result = orphanInvoice ?? ReconDeclaration.ReconWrappedJobDeclaration.Invoices.AddNew();
				result.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				result.US_CH_ReconEntry = reconEntry.PK;
			}

			return result;
		}

		JobComInvoiceHeader GetInvoice(out JobComInvoiceHeader orphanInvoice)
		{
			JobComInvoiceHeader result = null;
			orphanInvoice = null;

			foreach (JobComInvoiceHeader invoice in ReconDeclaration.ReconWrappedJobDeclaration.Invoices)
			{
				if (invoice.US_CH_ReconEntry == reconEntry.PK)
				{
					result = invoice;
					break;
				}
				else if (invoice.US_CH_ReconEntry.IsEmpty)
				{
					orphanInvoice = invoice; // this is the result of users clicking on the entry lines tab before entering the entry header
				}
			}

			return result;
		}

		#endregion

		#region Business Object Overrides

		public override bool IsInDatabase
		{
			get { return reconEntry.IsInDatabase; }
		}

		public override void Delete()
		{
			JobComInvoiceHeader invoiceToDelete = reconEntry.IsInDatabase ? Invoice : invoice;
			reconEntry.Delete();

			try
			{
				using (ReconDeclaration.SuspendMarkingApportionmentDirty())
				{
					if (invoiceToDelete != null)
					{
						invoice.InvoiceLines.CountChanged -= new CollectionCountChangedEventHandler(InvoiceLines_CountChanged);
						invoiceToDelete.Delete();
					}

					foreach (JobComInvoiceGroupHeader groupInvoice in ReconDeclaration.ReconWrappedJobDeclaration.AllGroupHeaders.ToArray())
					{
						if (!groupInvoice.IsDeleted &&
							groupInvoice != ReconDeclaration.TopGroupInvoice &&
							!groupInvoice.Charges.HasAnElementWithValidCharges() &&
							groupInvoice.AllJobComInvoiceHeaders.Count == 0)
						{
							groupInvoice.Delete();
						}
					}
				}
			}
			finally
			{
				ReconDeclaration.MarkApportionmentDirty();
			}
			base.Delete();
		}

		public override bool IsDeleted
		{
			get { return reconEntry.IsDeleted; }
		}

		public override bool IsSavedByFactory
		{
			get { return true; }
		}

		protected override IDisposable SuspendSettingHasChangesCore()
		{
			IDisposable thisOne = base.SuspendSettingHasChangesCore();
			IDisposable entrySuspender = null;

			if (reconEntry != null)
			{
				entrySuspender = reconEntry.SuspendSettingHasChanges();
			}

			return new DisposableAction(delegate
				{
					thisOne.Dispose();

					if (entrySuspender != null)
					{
						entrySuspender.Dispose();
					}
				});
		}

		#endregion

		#region IIdentified Members

		ZGuid IIdentified.Identifier
		{
			get { return CH_PK; }
		}

		#endregion

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return CH_PK; }
		}

		string ICodeDescription.Code
		{
			get { return CodePropertyAttribute.CodeFromBusinessObject(this); }
		}

		string ICodeDescription.Description
		{
			get { return DescriptionPropertyAttribute.DescriptionFromBusinessObject(this); }
		}

		#endregion
	}
}
