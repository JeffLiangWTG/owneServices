using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryInstruction : AutoZACusEntryInstruction
		, Integration.Customs.ZA.ICusEntryInstruction
		, ICusCodeDataTypeSupporter
		, ICaseNumberCollectionProvider
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new partial class Schema : AutoZACusEntryInstruction.Schema
		{
			public new const int CEI_UCROrderNumberMaxLength = 19;
		}

		#endregion

		#region Related BusinessObjects

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

		public new IEnumerable<JobComInvoiceLine> InvoiceLines => base.InvoiceLines.Cast<JobComInvoiceLine>();

		internal IEnumerable<CusEntryHeader> LinkedEntryHeaders => JobDeclaration?.ActiveEntryHeaders?.OfType<CusEntryHeader>()?.Where(x => x.CH_CEI_Instruction == this.PK) ?? Array.Empty<CusEntryHeader>();

		#endregion

		#region Overrides

		public override ZString CEI_Style
		{
			get { return base.CEI_Style; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_Style))
				{
					if (value != CEI_Style && JobDeclaration != null)
					{
						foreach (var invoiceLine in JobDeclaration.InvoiceLines.OfType<JobComInvoiceLine>().Where(x => x.JI_CEI == PK))
						{
							invoiceLine.MarkAsNeedingValidation();
						}
					}
					var previousGroup = ProcedureGroup;
					base.CEI_Style = value;

					if (CEI_Description == previousGroup && (CEI_Description == previousGroup || !ProcedureGroup.IsEmpty))
					{
						CEI_Description = ProcedureGroup;
					}
				}
			}
		}

		public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation()
		{
			return new CusEntryInstructionValidation(this);
		}

		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups()
		{
			return new CusEntryInstructionLookups(this);
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusEntryInstruction|CEI_OA_Warehouse", Caption = "From Warehouse")]
		public override ZGuid CEI_OA_Warehouse
		{
			get { return base.CEI_OA_Warehouse; }
			set
			{
				var oldValue = CEI_OA_Warehouse;
				base.CEI_OA_Warehouse = value;
				if (!IsCopying && oldValue != CEI_OA_Warehouse && !CEI_OA_Warehouse.IsEmpty && CEI_OA_Warehouse2.IsEmpty && Owner != null)
				{
					CEI_OA_Warehouse2 = CEI_OA_Warehouse;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusEntryInstruction|CEI_OA_Warehouse2", Caption = "To Warehouse")]
		public override ZGuid CEI_OA_Warehouse2
		{
			get { return base.CEI_OA_Warehouse2; }
			set
			{
				var oldValue = CEI_OA_Warehouse2;
				base.CEI_OA_Warehouse2 = value;
				if (!IsCopying && oldValue != CEI_OA_Warehouse2 && CEI_OA_Warehouse.IsEmpty && !CEI_OA_Warehouse2.IsEmpty && Owner != null)
				{
					CEI_OA_Warehouse = CEI_OA_Warehouse2;
				}
			}
		}

		public override ZGuid CEI_OH_Owner
		{
			get { return base.CEI_OH_Owner; }
			set
			{
				var oldValue = CEI_OH_Owner;
				base.CEI_OH_Owner = value;
				if (!IsCopying && oldValue != CEI_OH_Owner)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines.ToArray())
					{
						invoiceLine.NewOwnerProductSyncManager.Refresh();
						invoiceLine.MarkAsNeedingValidation();
					}
					if (HasBothOutOfAndIntoRegimeProcedure)
					{
						if (CEI_OA_Warehouse2.IsEmpty && !CEI_OA_Warehouse.IsEmpty)
						{
							CEI_OA_Warehouse2 = CEI_OA_Warehouse;
						}
						else if (!CEI_OA_Warehouse2.IsEmpty && CEI_OA_Warehouse.IsEmpty)
						{
							CEI_OA_Warehouse = CEI_OA_Warehouse2;
						}
					}
				}
			}
		}

		protected override bool IsChangeOfOwnershipWarehousingEnabledCore => true;

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BankCodes))]
		public override ZString CEI_BankCode { get => base.CEI_BankCode; set => base.CEI_BankCode = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.PortsOfExit))]
		public override ZString CEI_PortOfExit { get => base.CEI_PortOfExit; set => base.CEI_PortOfExit = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CreditTerms))]
		public override ZString CEI_CreditTerms
		{
			get { return base.CEI_CreditTerms; }
			set
			{
				base.CEI_CreditTerms = value.IsNumbersOnlyOrEmpty && !value.IsEmpty ? RemoveLeadingZeros(value) : value;
			}
		}

		string RemoveLeadingZeros(string value)
		{
			int number = int.Parse(value);
			return number.ToString();
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ProvisionalPaymentTypes))]
		public override ZString CEI_ProvisionalPaymentType { get => base.CEI_ProvisionalPaymentType; set => base.CEI_ProvisionalPaymentType = value; }

		[ReadOnlyMember(nameof(UZ_PreviousMRNReadOnly))]
		public override ZString CEI_PreviousMRN
		{
			get { return base.CEI_PreviousMRN; }

			set
			{
				var oldValue = base.CEI_PreviousMRN;
				base.CEI_PreviousMRN = value;
				if (oldValue != value)
				{
					ClearInvoiceLineValuesIfSame(value, JobComInvoiceLineSchema.Constants.JI_PreviousEntryNumber);
				}
			}
		}

		bool UZ_PreviousMRNReadOnly
		{
			get { return (JobDeclaration?.IsImportByExternalBroker ?? false) && LinkedEntryHeaders.Any(x => x.HasWHSTransaction); }
		}

		[ReadOnlyMember(nameof(CEI_DateForDuty_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.CusEntryInstruction|CEI_DateForDuty", Caption = "Assessment Date")]
		public override ZDateTime CEI_DateForDuty
		{
			get => base.CEI_DateForDuty;
			set
			{
				var oldValue = CEI_DateForDuty;
				base.CEI_DateForDuty = value;
				if (!IsCopying && oldValue != CEI_DateForDuty)
				{
					RefreshAndClearVOCBeforeValuesIfNeeded();
				}
			}
		}

		public bool CEI_DateForDuty_ReadOnly => HasMovementReferenceNumber;

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.MRNsForReplacing))]
		public override ZString CEI_MRNToBeReplaced
		{
			get { return base.CEI_MRNToBeReplaced; }
			set
			{
				var oldValue = CEI_MRNToBeReplaced;
				base.CEI_MRNToBeReplaced = value;
				if (!IsCopying && oldValue != CEI_MRNToBeReplaced)
				{
					DefaultAssessmentDateIfPossible();
				}
			}
		}

		void DefaultAssessmentDateIfPossible()
		{
			if (CEI_DateForDuty.IsEmpty && !CEI_MRNToBeReplaced.IsEmpty)
			{
				var entryHeader = JobDeclaration?.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => x.MovementReferenceNumber == CEI_MRNToBeReplaced);
				CEI_DateForDuty = entryHeader?.EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;
			}
		}

		[ReadOnlyMember(nameof(UZ_ExchangeRateDate_ReadOnly))]
		public override ZDateTime CEI_ExchangeRateDate { get => base.CEI_ExchangeRateDate; set => base.CEI_ExchangeRateDate = value; }

		public bool UZ_ExchangeRateDate_ReadOnly => HasMovementReferenceNumber;

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete && !InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.CusEntryLine != null)
															&& !LinkedEntryHeaders.Any(x => x.HasResponses);
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;
				if (InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.CusEntryLine != null))
				{
					result = ResString.GetMultilingualString("b090f77f-e377-4a64-b287-120a7463d88c", "Entry Instruction with CPC {0} is being used by an Entry Line and cannot be deleted.", CEI_Style);
				}
				else if (LinkedEntryHeaders.Any(x => x.HasResponses))
				{
					result = ResString.GetMultilingualString("3c842e8c-24c0-4600-bbb7-020a436add4b", "Entry Instruction with CPC {0} is being used by an Entry Header which has responses against it and cannot be deleted.", CEI_Style);
				}
				return result;
			}
		}

		public override ZBool AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError
		{
			get
			{
				var mrnIssueDate = LinkedEntryHeaders
									.Select(x => x.MovementReferenceNumberIssueDate)
									.Where(d => !d.IsEmpty)
									.OrderBy(o => o)
									.FirstOrDefault();

				return !mrnIssueDate.IsEmpty && mrnIssueDate < MultipleLinkedEntriesValidationChangeDate;
			}
		}

		ZDateTime MultipleLinkedEntriesValidationChangeDate => new ZDateTime(2020, 08, 01);

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusEntryInstruction|HAWBOverride", Caption = "Override House Bill")]
		public override ZString CEI_HAWBOverride { get => base.CEI_HAWBOverride; set => base.CEI_HAWBOverride = value; }

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusEntryInstruction|HAWBDateOverride", Caption = "Date")]
		public override ZDateTime CEI_HAWBDateOverride { get => base.CEI_HAWBDateOverride; set => base.CEI_HAWBDateOverride = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CargoCarrierCodeList))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.CusEntryInstruction|CargoCarrierOverride", Caption = "Override Cargo Carrier")]
		public override ZString CEI_CargoCarrierOverride { get => base.CEI_CargoCarrierOverride; set => base.CEI_CargoCarrierOverride = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CustomsOfficeList))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.CusEntryInstruction|CustomsOfficeOverride", Caption = "Override Customs Office")]
		public override ZString CEI_CustomsOfficeOverride
		{
			get => base.CEI_CustomsOfficeOverride;
			set
			{
				var oldValue = CEI_CustomsOfficeOverride;
				base.CEI_CustomsOfficeOverride = value;
				if (!IsCopying && oldValue != CEI_CustomsOfficeOverride)
				{
					LinkedEntryHeaders.ToList().ForEach(x => x.NeedsNewBGMReference = true);
				}
			}
		}

		public bool UZ_CustomsOfficeOverride_ReadOnly => !LinkedEntryHeaders.Cast<CusEntryHeader>().All(x => x.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected);

		public ZString CustomsOffice => !CEI_CustomsOfficeOverride.IsEmpty ? CEI_CustomsOfficeOverride : JobDeclaration?.JE_CustomsOffice ?? ZString.Empty;

		#endregion

		#region Properties

		internal MessageDataProviderKeyFactor MessageKeyFactor => Factory.GetValue(ref messageKeyFactorCached, () =>
		{
			var result = new MessageDataProviderKeyFactor { CPC = CEI_Style };
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				result.ShipmentType = declaration.JE_MessageType;
				result.TransportMode = declaration.JE_TransportMode;
				result.CPC = CEI_Style;
				result.ProcedureCategory = ProcedureCategory;
				result.FirstNonSpecificTariffTypeConcession = CusProcedure?.Concessions?.FirstNonSpecificTariffType(Factory) ?? ZString.Empty;
				result.RemovalTransportMode = declaration.JE_RemovalTransportCode;
				result.CountryOfDestination = declaration.FinalDestination?.Country;
				result.Remover = EntryHeader?.RemoverLocalCustomsCarrierCode ?? ZString.Empty;
				result.SubContractor = EntryHeader?.SubContractorRemoverCarrierCode ?? ZString.Empty;
			}
			return result;
		});

		CachedProperty<MessageDataProviderKeyFactor> messageKeyFactorCached;

		public new CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)base.EntryHeader; }
		}

		public ZDateTime AssessmentDate => GetEffectiveAssessmentDate(this, Factory);

		public ZString BondHolderCode => BondHolder?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.BondHolderCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;

		ZString GetBondGuaranteeCode(OrgHeader header)
		{
			return header?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.SouthAfricaCodeTypes.BGV, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;
		}

		public ZString BondGuaranteeCode
		{
			get
			{
				var bondHolderCode = ZString.Empty;
				if (!CEI_OH_BondHolder.IsEmpty)
				{
					bondHolderCode = GetBondGuaranteeCode(BondHolder);
				}
				else if (CEI_RemoverEDI)
				{
					bondHolderCode = GetBondGuaranteeCode(Remover);
				}
				else if (CEI_SubContractorEDI)
				{
					bondHolderCode = GetBondGuaranteeCode(SubContractor);
				}
				return bondHolderCode;
			}
		}

		ZDecimal GetBondGuaranteeValue(ZString bondGuaranteeCode) => bondGuaranteeCode.IsNumbersOnlyOrEmpty ? ZDecimal.ParseSafe(bondGuaranteeCode, 0m) : ZDecimal.Zero;

		ZDecimal GetBondGuaranteeValue(OrgHeader header) => GetBondGuaranteeValue(GetBondGuaranteeCode(header));

		public ZDecimal BondGuaranteeValue => GetBondGuaranteeValue(BondGuaranteeCode);

		public ZDecimal BondHolderBondGuaranteeValue => GetBondGuaranteeValue(BondHolder);

		public ZDecimal RemoverBondGuaranteeValue => GetBondGuaranteeValue(Remover);

		public ZDecimal SubContractorBondGuaranteeValue => GetBondGuaranteeValue(SubContractor);

		public ZDecimal TotalBondSuretyAmount => EntryHeader?.SumBondSuretyAmount ?? 0m;

		public ZPropertyInfo TotalBondSuretyAmountInfo => GetZPropertyInfo(nameof(TotalBondSuretyAmount));

		public bool HasMovementReferenceNumber => !EntryHeader?.MovementReferenceNumber.IsEmpty ?? false;

		public RefCusProcedure CusProcedure => GetCusProcedure(CEI_Style);

		RefCusProcedure GetCusProcedure(ZString procedureCode)
		{
			if (procedureCode.IsEmpty)
			{
				cusProcedure = null;
			}
			else if (cusProcedure == null || cusProcedure.ZZ6_ProcedureCode != procedureCode)
			{
				cusProcedure = new RefCusProcedure.Loader(Factory).LoadTop1FromCodeAndCountry(procedureCode, ZString.Empty, CountryCode, ZDateTime.Today);
			}
			return cusProcedure;
		}
		RefCusProcedure cusProcedure;

		public ZString ProcedureCategory => CusProcedure?.ZZ6_Category ?? ZString.Empty;

		public ZString ProcedureGroup => CusProcedure?.ZZ6_Group ?? ZString.Empty;

		public ZString ImporterCode => JobDeclaration.Importer?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;

		public ZString RebateUserCode => CEI_RebateUserOverride.IsEmpty ? (JobDeclaration.Importer?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.RebateUserCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty) : RebateUserCodeOverride;

		ZString RebateUserCodeOverride => RebateOverrideUser?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.RebateUserCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;

		public virtual OrgHeader RebateOverrideUser
		{
			get { return Factory.Load<OrgHeader>(CEI_RebateUserOverride); }
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.CusEntryInstruction|UZ_RebateUserOverride", Caption = "Rebate User Override")]
		[RelatedBusinessObject("RebateOverrideUser")]
		public override ZGuid CEI_RebateUserOverride { get => base.CEI_RebateUserOverride; set => base.CEI_RebateUserOverride = value; }

		public OrgHeader Remover => Carrier;

		public OrgHeader SubContractor => SubContractorAddress.Organisation;

		[RelatedBusinessObject(nameof(SubContractor))]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.Carriers))]
		public ZGuid OH_SubContractor
		{
			get
			{
				if (subContractorPK.IsDefault)
				{
					subContractorPK = SubContractor?.PK ?? ZGuid.Empty;
				}
				return subContractorPK;
			}
			set
			{
				subContractorPK = value;
				var orgHeader = Factory.Load<OrgHeader>(subContractorPK);
				SubContractorAddress.E2_OA_Address = orgHeader?.MainAddress.PK ?? ZGuid.Empty;
				CEI_SubContractorEDI = orgHeader != null;

				OH_SubContractorInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateOH_SubContractor();
				}
			}
		}
		ZGuid subContractorPK;

		internal JobDocAddress SubContractorAddress => subContractorAddress ?? (subContractorAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Contractor));
		JobDocAddress subContractorAddress;

		public ZPropertyInfo OH_SubContractorInfo => GetZPropertyInfo(nameof(OH_SubContractor));

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsSubContractorEDIReadOnly))]
		public override ZBool CEI_RemoverEDI
		{
			get => IsSubContractorEDIReadOnly ? ZBool.True : base.CEI_RemoverEDI;
			set
			{
				base.CEI_RemoverEDI = value;
				base.CEI_SubContractorEDI = !value;
			}
		}

		public ZBool IsSubContractorEDIReadOnly => OH_SubContractor.IsEmpty || !OH_SubContractor.IsValid;

		[ReadOnlyMember(nameof(IsSubContractorEDIReadOnly))]
		public override ZBool CEI_SubContractorEDI
		{
			get => base.CEI_SubContractorEDI;
			set
			{
				base.CEI_SubContractorEDI = value;
				base.CEI_RemoverEDI = !value;
			}
		}

		protected override ZAddress GetNewCEI_OA_Warehouse_ZAddress()
		{
			var fromWarehouseZAddress = base.GetNewCEI_OA_Warehouse_ZAddress();
			fromWarehouseZAddress.GetDefaultAddress += FromWarehouseZAddress_GetDefaultAddress;
			return fromWarehouseZAddress;
		}

		ZGuid FromWarehouseZAddress_GetDefaultAddress(IOrgHeader orgHeader)
		{
			return GetDefaultAddressPK(orgHeader);
		}

		protected override ZAddress GetNewCEI_OA_Warehouse2_ZAddress()
		{
			var toWarehouseZAddress = base.GetNewCEI_OA_Warehouse2_ZAddress();
			toWarehouseZAddress.GetDefaultAddress += ToWarehouseZAddress_GetDefaultAddress;
			return toWarehouseZAddress;
		}

		ZGuid ToWarehouseZAddress_GetDefaultAddress(IOrgHeader orgHeader)
		{
			return GetDefaultAddressPK(orgHeader);
		}

		ZGuid GetDefaultAddressPK(IOrgHeader orgHeader)
		{
			var result = ZGuid.Empty;
			var warehouse = orgHeader as OrgHeader;
			if (warehouse != null && warehouse.CustomsCodes.Cast<OrgCusCode>().Count(x => x.OK_CodeType == OrgCusCode.CodeTypes.WarehouseControlledPremisesID) == 1)
			{
				var cusCode = warehouse.CustomsCodes.Cast<OrgCusCode>().Single(x => x.OK_CodeType == OrgCusCode.CodeTypes.WarehouseControlledPremisesID);
				result = cusCode.OK_OA_PremisesAddress;
			}
			return result;
		}

		internal ZBool IsBondHolderRequired => JobDeclaration.JE_RemovalTransportCode == Core.Constants.TransportModes.Road;

		internal ZBool BHValid;

		public override ZDecimal CEI_TransactionValue
		{
			get => base.CEI_TransactionValue.Truncate(2);
			set => base.CEI_TransactionValue = value.Truncate(Math.Min(value.DecimalPlaces, 2));
		}

		public int TransactionValueDecimalPlaces => Factory.GetValue(ref transactionValueDecimalPlaces, () => CEI_TransactionValue.DecimalPlaces);

		CachedProperty<int> transactionValueDecimalPlaces;

		#endregion

		#region UCR Details

		public override ZBool CEI_IsUCROverridden
		{
			get { return base.CEI_IsUCROverridden; }
			set
			{
				var hasChanged = CEI_IsUCROverridden != value;
				if (hasChanged)
				{
					base.CEI_IsUCROverridden = value;
				}
				CEI_IsUCROverriddenInfo.RefreshBinding();
			}
		}

		[ReadOnlyMember(nameof(UCRNumber_ReadOnly))]
		public override ZString CEI_UCROverride
		{
			get => base.CEI_UCROverride;
			set
			{
				if (value != CEI_UCROverride)
				{
					base.CEI_UCROverride = value.Left(CEI_UCROverrideInfo.MaxLength);
				}
			}
		}

		public bool UCRNumber_ReadOnly => !CEI_IsUCROverridden;

		[MaxLength(Schema.CEI_UCROrderNumberMaxLength)]
		[ReadOnlyMember(nameof(UCRDetails_ReadOnly))]
		public override ZString CEI_UCROrderNumber
		{
			get { return base.CEI_UCROrderNumber; }
			set
			{
				var hasChanged = value != CEI_UCROrderNumber;
				if (hasChanged)
				{
					base.CEI_UCROrderNumber = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.RefTypeList))]
		[ReadOnlyMember(nameof(UCRDetails_ReadOnly))]
		public override ZString CEI_RefType { get => base.CEI_RefType; set => base.CEI_RefType = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ScopeList))]
		[ReadOnlyMember(nameof(UCRDetails_ReadOnly))]
		public override ZString CEI_Scope { get => base.CEI_Scope; set => base.CEI_Scope = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.EntityTypeList))]
		[ReadOnlyMember(nameof(UCRDetails_ReadOnly))]
		public override ZString CEI_EntityType { get => base.CEI_EntityType; set => base.CEI_EntityType = value; }

		public bool UCRDetails_ReadOnly => JobDeclaration?.IsImport ?? false;

		public void UpdateUCR()
		{
			CEI_UCROverride = UCRHelper.CalculateUCR(this);
		}

		public UCRHelper UCRHelper => fUCRHelper ?? (fUCRHelper = new UCRHelper());
		UCRHelper fUCRHelper;

		#endregion

		#region Collections

		#region CaseNumbers

		[ChildEditable(true)]
		public CaseNumberCollection CaseNumbers
		{
			get
			{
				if (casenumbers == null)
				{
					casenumbers = new CaseNumberCollection(this);
					casenumbers.Load();
					RegisterEditableChildObject(casenumbers);
				}
				return casenumbers;
			}
		}
		CaseNumberCollection casenumbers;

		BusinessObject ICaseNumberCollectionProvider.Master => this;

		#endregion

		#region Certificates

		[ChildEditable(true)]
		public CusCodeDataCollection<RCCCertificate> RCCCertificates
		{
			get
			{
				if (rccCertificates == null)
				{
					rccCertificates = new CusCodeDataCollection<RCCCertificate>(this, CusCodeDataTypeList.Codes.RCC);
					rccCertificates.Load();
					RegisterEditableChildObject(rccCertificates);
				}
				return rccCertificates;
			}
		}
		CusCodeDataCollection<RCCCertificate> rccCertificates;

		[ChildEditable(true)]
		public DutyRebateCertificateCollection DutyRebateCertificates
		{
			get
			{
				if (dutyRebateCertificates == null)
				{
					dutyRebateCertificates = new DutyRebateCertificateCollection(this);
					dutyRebateCertificates.Load();
					RegisterEditableChildObject(dutyRebateCertificates);
				}
				return dutyRebateCertificates;
			}
		}
		DutyRebateCertificateCollection dutyRebateCertificates;

		#endregion

		#region ProvisionalPaymentPayInfos

		[ChildEditable]
		public ProvisionalPaymentEntryPayInfoCollection ProvisionalPaymentPayInfos
		{
			get
			{
				if (provisionalPaymentPayInfos == null)
				{
					provisionalPaymentPayInfos = new ProvisionalPaymentEntryPayInfoCollection(Factory, LinkedEntryHeaders);
					RegisterEditableChildObject(provisionalPaymentPayInfos);
				}
				return provisionalPaymentPayInfos;
			}
		}
		ProvisionalPaymentEntryPayInfoCollection provisionalPaymentPayInfos;

		#endregion

		#endregion

		#region Implementation

		protected override Customs.Business.ProcedureRegimeDecider GetNewProcedureRegimeDecider() => new ProcedureRegimeDecider();
		protected override bool WarehouseIsInventoryManagementOnCore => WarehouseIsBondedWarehousing;
		protected override bool Warehouse2IsInventoryManagementOnCore => Warehouse2IsBondedWarehousing;
		protected override Customs.Business.DeclarationInventorySelectionHeader CreateNewDeclarationEntryInstructionInventorySelectionHeader() => new DeclarationInventorySelectionHeader(this);

		protected override string PartAttribute1Core => JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib1;

		protected override string PartAttribute2Core => JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib2;

		protected override string PartAttribute3Core => JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib3;

		protected override string SerialNumberCore => JobComInvoiceLine.Schema.JI_NewOwnerSerialNum;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CEI_RefType = RefTypeList.Codes.Invoice;
			CEI_Scope = ScopeList.Codes.SingleUse;
		}

		public override void Delete()
		{
			CusEntryNumber.Load(this).DeleteAll();
			base.Delete();
		}

		#region SupportsCloneCore
		protected override bool SupportsCloneCore()
		{
			return true;
		}
		#endregion

		void RefreshAndClearVOCBeforeValuesIfNeeded()
		{
			foreach (var entry in LinkedEntryHeaders)
			{
				entry.ClearVOCBeforeValuesIfMRNNotEntered();
				entry.ClearVOCAfterValuesIfMRNNotEntered();
				entry.RefreshDoNotClaimVATRefundIfNecessary();
				entry.CIFValueBeforeInfo.RefreshBinding();
				entry.CustomsValueBeforeInfo.RefreshBinding();
				entry.CustomsDutyExcluding12BBeforeInfo.RefreshBinding();
				entry.S1P2BDutyBeforeInfo.RefreshBinding();
				entry.ValueAddedTaxBeforeInfo.RefreshBinding();
				entry.DoNotClaimVATRefunInfo.RefreshBinding();
			}
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.CaseNumber, typeof(CaseNumber));
			result.Add(CusCodeDataTypeList.Codes.RCC, typeof(RCCCertificate));
			result.Add(CusCodeDataTypeList.Codes.DRC, typeof(DutyRebateCertificate));
			return result;
		}

		void ClearInvoiceLineValuesIfSame(IZType invoiceValue, string invoiceLineFieldName)
		{
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					if (invoiceLine.JI_CEI == this.PK)
					{
						IZType invoiceLineValue = (IZType)invoiceLine[invoiceLineFieldName];

						if (invoiceLineValue.Equals(invoiceValue))
						{
							using (invoiceLine.SuspendEffectiveValue(invoiceLineFieldName, invoiceValue))
							{
								invoiceLine[invoiceLineFieldName] = invoiceLineValue.Default;
							}

							ZPropertyInfo infoToRefresh = invoiceLine.ZPropertyInfoHash[invoiceLineFieldName];
							if (infoToRefresh != null)
							{
								infoToRefresh.RefreshBinding();
							}
						}
					}
				}
			}
		}

		protected override bool IsInventorySelectionEnabledCore()
		{
			return JobDeclaration?.IsInventorySelectionEnabled ?? false;
		}

		protected override void UpdateAndRefreshWhenStyleIsChanged(BaseJobComInvoiceLine invoiceLine, ZString oldValue, ZString newValue)
		{
			var zaInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var ppc = zaInvoiceLine.JI_Calc_PreviousProcedure;
			var oldProcedure = new RefCusProcedure.Loader(Factory).LoadTop1FromCodeAndCountry(oldValue, ppc, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
			var newProcedure = new RefCusProcedure.Loader(Factory).LoadTop1FromCodeAndCountry(newValue, ppc, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
			zaInvoiceLine.ClearAndDefaultCusLineTariffDetails(oldProcedure, newProcedure);
			base.UpdateAndRefreshWhenStyleIsChanged(zaInvoiceLine, oldValue, newValue);
			if (!IsValidationSuspended)
			{
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			}
		}

		protected override void ResetValuesAfterCloneCore()
		{
			CEI_DateForDuty = ZDate.Empty;
			((IAddInfoManager)this).AddInfo.UpdateRelatedPropertyInfo();
		}

		#region ISequenceNumberHeader - RCCCertificates

		public ShortSequenceNumberGenerator RCCCertificateLineNumberGenerator
		{
			get { return rccCertificateLineNumberGenerator ?? (rccCertificateLineNumberGenerator = new ShortSequenceNumberGenerator(new CusEntryInstructionWrapperForRCCLineNumberGenerator(this))); }
		}
		ShortSequenceNumberGenerator rccCertificateLineNumberGenerator;

		class CusEntryInstructionWrapperForRCCLineNumberGenerator : ISequenceNumberHeader
		{
			public CusEntryInstructionWrapperForRCCLineNumberGenerator(CusEntryInstruction entryInstruction)
			{
				this.entryInstruction = entryInstruction;
			}
			readonly CusEntryInstruction entryInstruction;

			public IEnumerable<ISequenceNumberLine> Lines => entryInstruction.RCCCertificates.OfType<RCCCertificate>();
		}

		#endregion

		#region ISequenceNumberHeader - DutyRebateCertificates

		public ShortSequenceNumberGenerator DutyRebateCertificateLineNumberGenerator
		{
			get { return dutyRebateCertificateLineNumberGenerator ?? (dutyRebateCertificateLineNumberGenerator = new ShortSequenceNumberGenerator(new CusEntryInstructionWrapperForDutyRebateLineNumberGenerator(this))); }
		}
		ShortSequenceNumberGenerator dutyRebateCertificateLineNumberGenerator;

		class CusEntryInstructionWrapperForDutyRebateLineNumberGenerator : ISequenceNumberHeader
		{
			public CusEntryInstructionWrapperForDutyRebateLineNumberGenerator(CusEntryInstruction entryInstruction)
			{
				this.entryInstruction = entryInstruction;
			}
			readonly CusEntryInstruction entryInstruction;

			public IEnumerable<ISequenceNumberLine> Lines => entryInstruction.DutyRebateCertificates.OfType<DutyRebateCertificate>();
		}

		#endregion

		public bool ShouldUpdateUCRNumber
		{
			get
			{
				var entryHeader = EntryHeader;
				var isExport = JobDeclaration?.IsExport ?? ZBool.False;
				return isExport && (entryHeader == null || (entryHeader.MovementReferenceNumber.IsEmpty && (entryHeader.MessageStatus == ZAMessageStatusList.Codes.NotSent || HasEntryRejected)));
			}
		}

		bool HasEntryRejected => CustomsStatusAttributeHelper.IsStatusRejected(Factory, EntryHeader.JobStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);

		protected override void OnFactorySaving()
		{
			if (!CEI_IsUCROverridden && ShouldUpdateUCRNumber)
			{
				UpdateUCR();
			}
			base.OnFactorySaving();
		}

		public HashSet<ZString> PreviousProcedures => CachedValueHelper.GetValue(Factory, ref previousProcedures, GetPreviousProcedures);

		CachedProperty<HashSet<ZString>> previousProcedures;

		HashSet<ZString> GetPreviousProcedures()
		{
			return InvoiceLines.Select(invoiceLine => invoiceLine.JI_Calc_PreviousProcedure).Distinct().ToHashSet();
		}

		#endregion

		#region Static Functions

		internal static ZDateTime GetEffectiveAssessmentDate(CusEntryInstruction instruction, BusinessObjectFactory factory)
		{
			var valuationDate = instruction?.CEI_DateForDuty ?? ZDateTime.Empty;
			return valuationDate.IsValid ? valuationDate
										 : factory.GetCachedValue(DefaultAssessmentDateCacheKey, () => ZDateTime.Now);
		}

		internal static void ResetDefaultAssessmentDate(BusinessObjectFactory factory)
		{
			factory.ClearCachedValue<ZDateTime>(DefaultAssessmentDateCacheKey);
		}

		internal const string DefaultAssessmentDateCacheKey = "DefaultAssessmentDate";

		#endregion
	}
}
