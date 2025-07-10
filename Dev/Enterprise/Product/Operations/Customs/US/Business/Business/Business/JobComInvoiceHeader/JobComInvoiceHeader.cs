using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.LicenseManager;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[UniversalCopyAddInfo]
	public partial class JobComInvoiceHeader : AutoJobComInvoiceHeader, Integration.Customs.US.IJobComInvoiceHeader, IUpdateScreeningStatusNotifier
		, IMessageAttacheeInDeclaration
		, IShouldUpdateScreeningStatus
		, ICurrencyConverterDataProvider
		, ICusCodeDataTypeSupporter
		, IInvoiceHeader
		, IPGADataChangeTrackerSupporter
		, IEffectiveValueManagerSupporter
	{
		#region Schema

		public new class Schema : AutoJobComInvoiceHeader.Schema
		{
			public const string US_EntryNumber = "US_EntryNumber";
			public const string US_XTN = USAddInfoSchema.Constants.US_XTN;
			public const string US_TermsOfDeliveryLocationCountry = "US_TermsOfDeliveryLocationCountry";
			public const string US_TermsOfDeliveryLocationScheduleD = "US_TermsOfDeliveryLocationScheduleD";
			public const string US_TermsOfDeliveryLocationScheduleK = "US_TermsOfDeliveryLocationScheduleK";
			public const string ManufacturerNameAndID = "ManufacturerNameAndID";
			public const string InvoicerOrgPK = "InvoicerOrgPK";
			public const string JZ_RX_NKInvoice_CurrencyReadOnly = "JZ_RX_NKInvoice_CurrencyReadOnly";
			public const string JZ_OA_FDAShipperAddress = "JZ_OA_FDAShipperAddress";
			public const string JZ_OA_InvoicerDocAddress = "JZ_OA_InvoicerDocAddress";
			public const int US_BuyerContactMaxLength = 50;
			public const int US_IntermConsigneeContactMaxLength = 50;
			public const int US_SupplierContactMaxLength = 256;
		}

		#endregion

		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region AddInfo Effective properties

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_ECCNList))]
		public override ZString US_ECCN
		{
			get { return GetEffectiveValueToReturn(base.US_ECCN, JobDeclaration.Schema.US_ECCN, Schema.US_ECCN); }
			set
			{
				var oldValue = base.US_ECCN; // need base value instead of effective value
				base.US_ECCN = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_ECCN);
				if (!IsCopying)
				{
					var newValue = US_ECCN;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_ECCN);
					}
				}
			}
		}

		public ZBool ECCNCodeFindBoxVisible => HasAvailableECCNNumbers;

		public ZBool ECCNTextBoxVisible => !HasAvailableECCNNumbers;

		ZBool HasAvailableECCNNumbers => !US_LicenseType.IsEmpty && LicenseValidationHelper.HasAvailableECCNNumber(Factory, US_LicenseType);

		public override ZString US_ExportCode
		{
			get { return GetEffectiveValueToReturn(base.US_ExportCode, JobDeclaration.Schema.US_ExportCode, Schema.US_ExportCode); }
			set
			{
				var oldValue = base.US_ExportCode; // need base value and not effective value
				base.US_ExportCode = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_ExportCode);
				if (!IsCopying)
				{
					var newValue = US_ExportCode;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_ExportCode);
					}
				}
			}
		}

		public override ZString US_ForeignTradeZone
		{
			get { return GetEffectiveValueToReturn(base.US_ForeignTradeZone, JobDeclaration.Schema.US_ForeignTradeZone, Schema.US_ForeignTradeZone); }
			set { base.US_ForeignTradeZone = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_ForeignTradeZone); }
		}

		public override ZString US_HazardousCargo
		{
			get { return GetEffectiveValueToReturn(base.US_HazardousCargo, JobDeclaration.Schema.US_HazardousCargo, Schema.US_HazardousCargo); }
			set { base.US_HazardousCargo = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_HazardousCargo); }
		}

		public ZBool US_IsHazardousCargo
		{
			get { return US_HazardousCargo == YesNoDefaultList.Codes.Yes; }
		}

		public override ZString US_ImportEntryNo
		{
			get { return GetEffectiveValueToReturn(base.US_ImportEntryNo, JobDeclaration.Schema.US_ImportEntryNo, Schema.US_ImportEntryNo); }
			set { base.US_ImportEntryNo = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_ImportEntryNo); }
		}

		public override ZString US_InbondType
		{
			get { return GetEffectiveValueToReturn(base.US_InbondType, JobDeclaration.Schema.US_InbondType, Schema.US_InbondType); }
			set { base.US_InbondType = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_InbondType); }
		}

		[ReadOnlyMember(nameof(US_LicenseNo_ReadOnly))]
		public override ZString US_LicenseNo
		{
			get
			{
				var result = base.US_LicenseNo;
				if (!LicenseAndLicenseExemptionTypeManager.IsRequiredSpaceCode(US_LicenseType, Factory, ExportDateForLicenseType))
				{
					result = GetEffectiveValueToReturn(result, JobDeclaration.Schema.US_LicenseNo, Schema.US_LicenseNo);
				}
				return result;
			}
			set
			{
				var oldValue = base.US_LicenseNo; // need base value and not effective value
				if (LicenseAndLicenseExemptionTypeManager.IsRequiredSpaceCode(US_LicenseType, Factory, ExportDateForLicenseType))
				{
					base.US_LicenseNo = value;
				}
				else
				{
					base.US_LicenseNo = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_LicenseNo);
				}

				if (!IsCopying)
				{
					var newValue = US_LicenseNo;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_LicenseNo);
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
					var exportDate = ExportDateForLicenseType;
					if (exportDate.IsValid)
					{
						result = UniversalReferenceDataHelper.GetLicenseNoReadOnly_LicenseNoIsNotEmpty(Factory, US_LicenseType, exportDate, US_LicenseNo);
					}
				}
				return result;
			}
		}

		public override ZString US_LicenseType
		{
			get { return GetEffectiveValueToReturn(base.US_LicenseType, JobDeclaration.Schema.US_LicenseType, Schema.US_LicenseType); }
			set
			{
				var oldValue = base.US_LicenseType; // need base value and not effective value
				base.US_LicenseType = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_LicenseType);
				if (!IsCopying)
				{
					var newValue = US_LicenseType;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_LicenseType);
						if (IsExport)
						{
							JobComInvoiceLines.MarkAsNeedingValidation();

							if (!IsCopying)
							{
								var exportDate = ExportDateForLicenseType;
								if (exportDate.IsValid)
								{
									var licenseNo = UniversalReferenceDataHelper.GetLicenseNo(Factory, value, exportDate);
									if (!licenseNo.IsEmpty)
									{
										US_LicenseNo = licenseNo.SubstringSafe(0, AutoUSAddInfo.Schema.US_LicenseNoMaxLength);
									}
									else if (!US_LicenseNo.IsEmpty && LicenseAndLicenseExemptionTypeManager.IsRequiredSpaceCode(value, Factory, exportDate))
									{
										US_LicenseNo = ZString.Empty;
									}
								}
								SetDefaultAESRegistrationNumberIfEmpty();
							}
						}
					}
				}
			}
		}

		public override ZString US_RoutedTransaction
		{
			get { return GetEffectiveValueToReturn(base.US_RoutedTransaction, JobDeclaration.Schema.US_RoutedTransaction, Schema.US_RoutedTransaction); }
			set { base.US_RoutedTransaction = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_RoutedTransaction); }
		}

		public ZBool US_IsRoutedTransaction
		{
			get { return US_RoutedTransaction == YesNoDefaultList.Codes.Yes; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.USStateList))]
		public override ZString US_StateOfOrigin
		{
			get { return GetEffectiveValueToReturn(base.US_StateOfOrigin, JobDeclaration.Schema.US_StateOfOrigin, Schema.US_StateOfOrigin); }
			set { base.US_StateOfOrigin = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_StateOfOrigin); }
		}

		public override ZString US_TariffType
		{
			get { return GetEffectiveValueToReturn(base.US_TariffType, JobDeclaration.Schema.US_TariffType, Schema.US_TariffType); }
			set
			{
				var oldValue = US_TariffType;
				base.US_TariffType = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_TariffType);

				if (!IsCopying)
				{
					if (oldValue != value)
					{
						ClearInvoiceLineValuesIfSame(US_TariffType, JobComInvoiceLine.Schema.US_TariffType);
					}
				}
			}
		}

		public override ZString US_TransactionsRelated
		{
			get { return IsImport ? base.US_TransactionsRelated : GetEffectiveValueToReturn(base.US_TransactionsRelated, JobDeclaration.Schema.US_TransactionsRelated, Schema.US_TransactionsRelated); }
			set
			{
				if (!JobDeclaration?.IsReconMessageType ?? true)
				{
					var oldValue = base.US_TransactionsRelated; // need base value instead of effective value
					base.US_TransactionsRelated = IsImport ? value : GetEffectiveValueToSet(value, JobDeclaration.Schema.US_TransactionsRelated);
					if (IsImport)
					{
						var newValue = US_TransactionsRelated;
						if (oldValue != newValue)
						{
							JobComInvoiceLines.MarkAsNeedingValidation();
							ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_TransactionsRelated);
						}
					}
				}
			}
		}

		public override ZString US_FirstSale
		{
			get { return base.US_FirstSale; }
			set
			{
				var changed = base.US_FirstSale != value;
				base.US_FirstSale = value;

				if (changed && !IsCopying)
				{
					ClearInvoiceLineValuesIfSame(US_FirstSale, JobComInvoiceLine.Schema.US_FirstSale);
				}
			}
		}

		public ZBool US_IsTransactionsRelated
		{
			get { return US_TransactionsRelated == YesNoDefaultList.Codes.Yes; }
		}

		public bool RequiresPriorNoticeReporting
		{
			get
			{
				if (requiresPriorNoticeReportingCached == null)
				{
					requiresPriorNoticeReportingCached = new CachedProperty<bool>(Factory, delegate
					{
						foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
						{
							if (invoiceLine.RequiresPriorNoticeReporting())
							{
								return true;
							}
						}

						return false;
					});
				}
				return requiresPriorNoticeReportingCached.Value;
			}
		}
		CachedProperty<bool> requiresPriorNoticeReportingCached;

		public bool RequiresCustomsBrokerReporting
		{
			get
			{
				var declaration = JobDeclaration;

				var result = false;
				if (declaration != null)
				{
					result = declaration.IsACECargoCertificationMode && (HasInvoiceLinesWithFSIS || HasInvoiceLinesWithPST || HasInvoiceLinesWithAPHIS || HasInvoiceLinesWithNHTSARequireCB || HasInvoiceLinesWithODSOrTSCAARequireCB)
						|| declaration.CanHavePGAFDA && HasInvoiceLinesWithPGAFDA;
				}
				return result;
			}
		}

		public override ZDecimal JZ_Weight
		{
			get { return base.JZ_Weight; }
			set
			{
				ZDecimal oldValue = base.JZ_Weight;
				base.JZ_Weight = value;
				if (JZ_Weight != oldValue && !IsCopying && IsExport)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.MessageTypes))]
		public override ZString JZ_MessageType
		{
			get { return base.JZ_MessageType; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(BaseJobComInvoiceHeader.Schema.JZ_MessageType))
				{
					ZString oldValue = JZ_MessageType;
					base.JZ_MessageType = value;
					if (oldValue != JZ_MessageType)
					{
						DefaultTariffTypeIfNeeded();
					}
				}
			}
		}

		void DefaultTariffTypeIfNeeded()
		{
			if (IsExport && US_TariffType.IsEmpty)
			{
				US_TariffType = USCustomsDataRegistry.Instance.ExportDefaultTariffType.Value;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.ReconOriginalEntries))]
		public override ZGuid US_CH_ReconEntry
		{
			get { return base.US_CH_ReconEntry; }
			set { base.US_CH_ReconEntry = value; }
		}

		#region DDTC

		public override ZString US_DDTCITARExemptionNo
		{
			get { return GetEffectiveValueToReturn(base.US_DDTCITARExemptionNo, JobDeclaration.Schema.US_DDTCITARExemptionNo, Schema.US_DDTCITARExemptionNo); }
			set { base.US_DDTCITARExemptionNo = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_DDTCITARExemptionNo); }
		}

		public override ZString US_DDTCMilitaryEquipmentIndicator
		{
			get { return GetEffectiveValueToReturn(base.US_DDTCMilitaryEquipmentIndicator, JobDeclaration.Schema.US_DDTCMilitaryEquipmentIndicator, Schema.US_DDTCMilitaryEquipmentIndicator); }
			set { base.US_DDTCMilitaryEquipmentIndicator = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_DDTCMilitaryEquipmentIndicator); }
		}

		public override ZString US_DDTCPartyCertificationIndicator
		{
			get { return GetEffectiveValueToReturn(base.US_DDTCPartyCertificationIndicator, JobDeclaration.Schema.US_DDTCPartyCertificationIndicator, Schema.US_DDTCPartyCertificationIndicator); }
			set { base.US_DDTCPartyCertificationIndicator = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_DDTCPartyCertificationIndicator); }
		}

		public override ZString US_DDTCRegistrationNo
		{
			get { return GetEffectiveValueToReturn(base.US_DDTCRegistrationNo, JobDeclaration.Schema.US_DDTCRegistrationNo, Schema.US_DDTCRegistrationNo); }
			set { base.US_DDTCRegistrationNo = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_DDTCRegistrationNo); }
		}

		public override ZString US_DDTCUSMLCategoryCode
		{
			get { return GetEffectiveValueToReturn(base.US_DDTCUSMLCategoryCode, JobDeclaration.Schema.US_DDTCUSMLCategoryCode, Schema.US_DDTCUSMLCategoryCode); }
			set
			{
				base.US_DDTCUSMLCategoryCode = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_DDTCUSMLCategoryCode);
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
			get
			{
				if (US_JurisdictionNumber_ReadOnly)
				{
					return base.US_JurisdictionNumber;
				}
				return GetEffectiveValueToReturn(base.US_JurisdictionNumber, JobDeclaration.Schema.US_JurisdictionNumber, Schema.US_JurisdictionNumber);
			}
			set { base.US_JurisdictionNumber = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_JurisdictionNumber); }
		}

		#endregion

		#endregion

		#region Organisation For US Customs

		public void ReSynchronizeExportOrganisationIfNeeded(bool shouldSynchronize)
		{
			shouldSynchronizeUSPPI = (shouldSynchronize && fUS_USPPI == null);
			shouldSynchronizeExportUltimateConsignee = (shouldSynchronize && fUS_ExportUltimateConsignee == null);
			shouldSynchronizeIntermediateConsignee = (shouldSynchronize && fUS_IntermediateConsignee == null);
			if (shouldSynchronize)
			{
				EnsureUSOrganisationAreLoadedIfNeeded();
			}
		}

		#region US_USPPI
		public USOrganisation US_USPPI
		{
			get
			{
				if (fUS_USPPI == null)
				{
					fUS_USPPI = new USOrganisation(JZ_OH_SupplierInfo, USPPIDocAddress.E2_OA_AddressInfo, USPPIDocAddress.E2_ContactInfo, USPPIDocAddress.E2_Phone_FormattedInfo, USPPIDocAddress.E2_AddressOverrideInfo, ContactType.Consignor, IsUSOrganisationSynchronizationEnable, Factory, USPPIDocAddress);
					fUS_USPPI.GetDefaultContact = GetDefaultUSPPIContact;
				}
				if (shouldSynchronizeUSPPI)
				{
					try
					{
						fUS_USPPI.ReSynchronize(JZ_OH_SupplierInfo, USPPIDocAddress.E2_OA_AddressInfo);
					}
					finally
					{
						shouldSynchronizeUSPPI = false;
					}
				}

				return fUS_USPPI;
			}
		}
		USOrganisation fUS_USPPI;
		bool shouldSynchronizeUSPPI;

		public USOrganisationDocAddress USPPIDocAddress
		{
			get
			{
				if (f_USPPIDocAddress == null || f_USPPIDocAddress.IsDeleted)
				{
					f_USPPIDocAddress = DocAddresses.FindOrCreateWithRequirement(USOrgRequirementProvider.USPPIDocAddressRequirement);
					f_USPPIDocAddress.OrgHeaderAfterChange += USPPIDocAddress_OnOrgHeaderChanged;
					f_USPPIDocAddress.IgnoreValidationStatusError = true;
				}

				if (IsExport && f_USPPIDocAddress.E2_OA_Address.IsEmpty && !f_USPPIDocAddress.E2_AddressOverride && IsAttachedToPersistentDeclaration)
				{
					var supplierAddressOrgPK = f_USPPIDocAddress.OrganisationPK;
					if (supplierAddressOrgPK != ZGuid.Invalid)
					{
						var declaration = JobDeclaration;
						if (supplierAddressOrgPK != declaration.JE_OH_Supplier && declaration.JE_OH_Supplier.IsValid)
						{
							var supplier = declaration.Supplier;
							if (supplier != null)
							{
								f_USPPIDocAddress.E2_OA_Address = GetDeclarationSupplierAddressPK();
							}
						}
						if (fUS_USPPI != null)
						{
							fUS_USPPI.RefreshBinding();
						}
					}
				}

				return f_USPPIDocAddress;
			}
		}

		USOrganisationDocAddress f_USPPIDocAddress;

		public void RefreshPickupAddress()
		{
			if (IsExport && !fSupplierPickupAddress.E2_AddressOverride && IsAttachedToPersistentDeclaration)
			{
				var declaration = JobDeclaration;
				var supplierPickupAddress = declaration.SupplierPickupAddress;
				if ((declaration.IsSupplierPickupAddressInitialised || declaration.IsInDatabase) && supplierPickupAddress.Address != null && !supplierPickupAddress.E2_AddressOverride)
				{
					fSupplierPickupAddress.E2_OA_Address = supplierPickupAddress.E2_OA_Address;
				}
			}
		}

		public void RefreshUSPPIDocAddress()
		{
			if (IsExport && !USPPIDocAddress.E2_AddressOverride && IsAttachedToPersistentDeclaration)
			{
				USPPIDocAddress.E2_OA_Address = GetDeclarationSupplierAddressPK();
				US_USPPI.ReSynchronize(JZ_OH_SupplierInfo, USPPIDocAddress.E2_OA_AddressInfo);
				if (fUS_USPPI != null)
				{
					fUS_USPPI.RefreshBinding();
				}
			}
		}

		public ZString US_USPPIEIN
		{
			get
			{
				var result = USPPIDocAddress.Organisation?.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates) ?? ZString.Empty;
				if (!result.IsEmpty)
				{
					result = usEINPrefix + result;
				}
				return result;
			}
		}
		const string usEINPrefix = "EIN: ";

		#endregion

		public USOrganisationDocAddress SupplierPickupAddress
		{
			get
			{
				if (fSupplierPickupAddress == null || fSupplierPickupAddress.IsDeleted)
				{
					fSupplierPickupAddress = DocAddresses.FindOrCreateWithRequirement(USOrgRequirementProvider.SupplierPicDlvAddressRequirement);
					fSupplierPickupAddress.DocAddressChanged += SupplierPickupAddress_DocAddressChanged;
				}

				return fSupplierPickupAddress;
			}
		}
		USOrganisationDocAddress fSupplierPickupAddress;

		void SupplierPickupAddress_DocAddressChanged(object sender, EventArgs e)
		{
			if (IsExport && IsAttachedToPersistentDeclaration)
			{
				StateOfOriginDefaulter.Default(US_StateOfOriginInfo, SupplierPickupAddress);
			}
		}

		#region US_ExportUltimateConsignee
		public USOrganisation US_ExportUltimateConsignee
		{
			get
			{
				if (fUS_ExportUltimateConsignee == null)
				{
					fUS_ExportUltimateConsignee = new USOrganisation(JZ_OH_BuyerInfo, UltimateConsigneeDocAddress.E2_OA_AddressInfo, UltimateConsigneeDocAddress.E2_ContactInfo, UltimateConsigneeDocAddress.E2_Phone_FormattedInfo, UltimateConsigneeDocAddress.E2_AddressOverrideInfo, ContactType.Consignee, IsUSOrganisationSynchronizationEnable, Factory, UltimateConsigneeDocAddress);
				}
				if (shouldSynchronizeExportUltimateConsignee)
				{
					try
					{
						fUS_ExportUltimateConsignee.ReSynchronize(JZ_OH_BuyerInfo, UltimateConsigneeDocAddress.E2_OA_AddressInfo);
					}
					finally
					{
						shouldSynchronizeExportUltimateConsignee = false;
					}
				}

				return fUS_ExportUltimateConsignee;
			}
		}
		USOrganisation fUS_ExportUltimateConsignee;
		bool shouldSynchronizeExportUltimateConsignee;

		public USOrganisationDocAddress UltimateConsigneeDocAddress
		{
			get
			{
				if (ultimateConsigneeDocAddress == null || ultimateConsigneeDocAddress.IsDeleted)
				{
					ultimateConsigneeDocAddress = DocAddresses.FindOrCreateWithRequirement(USOrgRequirementProvider.UltimateConsigneeDocAddressRequirement);
					ultimateConsigneeDocAddress.OrgHeaderAfterChange += UltimateConsigneeDocAddress_OnOrgHeaderChanged;
				}

				return ultimateConsigneeDocAddress;
			}
		}
		USOrganisationDocAddress ultimateConsigneeDocAddress;

		#endregion

		#region US_IntermediateConsignee
		public USOrganisation US_IntermediateConsignee
		{
			get
			{
				if (fUS_IntermediateConsignee == null)
				{
					fUS_IntermediateConsignee = new USOrganisation(JZ_OH_ConsigneeInfo, IntermediateConsigneeDocAddress.E2_OA_AddressInfo, IntermediateConsigneeDocAddress.E2_ContactInfo, IntermediateConsigneeDocAddress.E2_Phone_FormattedInfo, IntermediateConsigneeDocAddress.E2_AddressOverrideInfo, ContactType.Consignee, IsUSOrganisationSynchronizationEnable, Factory, IntermediateConsigneeDocAddress);
				}
				if (shouldSynchronizeIntermediateConsignee)
				{
					try
					{
						fUS_IntermediateConsignee.ReSynchronize(JZ_OH_ConsigneeInfo, IntermediateConsigneeDocAddress.E2_OA_AddressInfo);
					}
					finally
					{
						shouldSynchronizeIntermediateConsignee = false;
					}
				}
				return fUS_IntermediateConsignee;
			}
		}
		USOrganisation fUS_IntermediateConsignee;
		bool shouldSynchronizeIntermediateConsignee;

		public USOrganisationDocAddress IntermediateConsigneeDocAddress
		{
			get
			{
				if (intermediateConsigneeDocAddress == null || intermediateConsigneeDocAddress.IsDeleted)
				{
					intermediateConsigneeDocAddress = DocAddresses.FindOrCreateWithRequirement(USOrgRequirementProvider.IntermediateConsigneeDocAddressRequirement);
				}

				return intermediateConsigneeDocAddress;
			}
		}
		USOrganisationDocAddress intermediateConsigneeDocAddress;

		#endregion

		#region DocAddresses

		[UniversalCopyCollectionEntity(JobDocAddress.Schema.TableName, JobDocAddress.Schema.E2_ParentID, JobDocAddress.Schema.E2_ParentTableCode)]
		public new USOrganisationDocAddressDependentCollection DocAddresses => (USOrganisationDocAddressDependentCollection)base.DocAddresses;

		protected override JobDocAddressDependentCollection GetDocAddressesCore()
		{
			var docAddresses = new USOrganisationDocAddressDependentCollection(this);
			docAddresses.Load();
			RegisterEditableChildObject(docAddresses);
			return docAddresses;
		}

		#endregion

		#region USOrganisation Requirement Provider

		USOrganisationRequirementProvider USOrgRequirementProvider
		{
			get { return fUSOrgRequirementProvider ?? (fUSOrgRequirementProvider = new USOrganisationRequirementProvider(Factory)); }
		}
		USOrganisationRequirementProvider fUSOrgRequirementProvider;

		#endregion

		#region USPPIDocAddress_OnOrgHeaderChanged

		void USPPIDocAddress_OnOrgHeaderChanged(object sender, EventArgs e)
		{
			JobComInvoiceLines.UpdateProductDetailsOnSupplierBuyerChange();

			DefaultIncoTermAndCurrencyFromSupplier();
		}

		#endregion

		#region UltimateConsigneeDocAddress_OnOrgHeaderChanged

		void UltimateConsigneeDocAddress_OnOrgHeaderChanged(object sender, EventArgs e)
		{
			JobComInvoiceLines.UpdateProductDetailsOnSupplierBuyerChange();
			DefaultIncoTermAndCurrencyFromSupplier();
		}

		#endregion

		#endregion

		#region New Properties

		internal ZDateTime ExportDateForLicenseType => !US_DateOfExport.IsValid || US_DateOfExport.IsEmpty ? JobDeclaration.GetEffectiveDateForECR() : US_DateOfExport;

		public JobDeclaration ReleaseDeclaration
		{
			get
			{
				if (releaseDeclarationCached == null)
				{
					releaseDeclarationCached = new CachedProperty<JobDeclaration>(Factory, () =>
					{
						JobDeclaration result = null;
						if (!US_ReleaseEntryNumber.IsEmpty)
						{
							result = USReleaseDeclarationLoader.GetReleaseDeclaration(Factory, US_ReleaseEntryNumber, Branch.GB_GC);
						}
						return result;
					});
				}
				return releaseDeclarationCached.Value;
			}
		}
		CachedProperty<JobDeclaration> releaseDeclarationCached;

		public CusEntryHeader ENSReleaseEntry
		{
			get
			{
				if (eNSReleaseEntryCached == null)
				{
					eNSReleaseEntryCached = new CachedProperty<CusEntryHeader>(Factory, () =>
					{
						CusEntryHeader result = null;
						if (!US_ReleaseEntryNumber.IsEmpty)
						{
							var entryFilerCode = US_ReleaseEntryNumber.Left(3);
							var entryNumber = US_ReleaseEntryNumber.SubstringSafe(3);
							if (!entryFilerCode.IsEmpty && !entryNumber.IsEmpty)
							{
								result = new CusEntryHeader.Loader(Factory).FindByEntryNumberAndFilerCode(Branch.GB_GC, entryNumber, entryFilerCode, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
							}
						}
						return result;
					});
				}
				return eNSReleaseEntryCached.Value;
			}
		}
		CachedProperty<CusEntryHeader> eNSReleaseEntryCached;

		void ResetCachedValue()
		{
			releaseDeclarationCached = null;
			eNSReleaseEntryCached = null;
		}

		public void ResetConsolidatedEntryJobNoForReleaseEntry()
		{
			if (ReleaseDeclaration != null && JobDeclaration != null)
			{
				if (JobDeclaration.JE_DeclarationReference == ReleaseDeclaration.US_ConsolidatedJobNumber)
				{
					SetConsolidatedJobNoForReleaseEntry(ZString.Empty, ZString.Empty);
				}
			}
		}

		public void SetConsolidatedJobNoForReleaseEntry(ZString consolidatedJobNo, ZString consolidatedEntryNo)
		{
			if (ReleaseDeclaration != null && JobDeclaration != null)
			{
				if (!ReleaseDeclaration.US_ConsolidatedJobNumber.IsEmpty)
				{
					if (consolidatedJobNo.IsEmpty)
					{
						var rowCount = JobDeclaration.Invoices.OfType<JobComInvoiceHeader>().Count(x => x.US_ReleaseEntryNumber == US_ReleaseEntryNumber);
						if (rowCount == 1)
						{
							ReleaseDeclaration.US_ConsolidatedJobNumber = consolidatedJobNo;
							SetConsolidatedEntryNoForReleaseEntry(consolidatedEntryNo);
						}
					}
					else
					{
						shouldUpdateConsolidatedJobNo = true;
					}
				}
				else if (ReleaseDeclaration.US_ConsolidatedJobNumber.IsEmpty && !consolidatedJobNo.IsEmpty)
				{
					ReleaseDeclaration.US_ConsolidatedJobNumber = consolidatedJobNo;
					SetConsolidatedEntryNoForReleaseEntry(consolidatedEntryNo);
				}
			}
		}

		public void SetConsolidatedEntryNoForReleaseEntry(ZString consolidatedEntryNo)
		{
			if (ReleaseDeclaration != null)
			{
				ReleaseDeclaration.US_ConsolidatedEntryNumber = consolidatedEntryNo;
			}
		}

		ZBool shouldUpdateConsolidatedJobNo { get; set; }

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.Entries))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceHeader|US_ReleaseEntryNumber", Caption = "Release Entry No.", ShortCaption = "Rel. Entry No.")]
		public override ZString US_ReleaseEntryNumber
		{
			get { return base.US_ReleaseEntryNumber; }
			set
			{
				bool hasChanges = US_ReleaseEntryNumber != value;
				if (hasChanges && !IsCopying)
				{
					var declaration = JobDeclaration;
					ResetConsolidatedEntryJobNoForReleaseEntry();
					base.US_ReleaseEntryNumber = value;
					ResetCachedValue();
					if (declaration != null)
					{
						if (declaration.JE_DeclarationReference.IsEmpty)
						{
							declaration.ShouldUpdateConsolidatedJobNo = true;
						}
						else
						{
							SetConsolidatedJobNoForReleaseEntry(declaration.JE_DeclarationReference, declaration.US_EntryFilerCode + declaration.DecEntryNumber);
						}

						if (!US_ReleaseEntryNumber.IsEmpty && IsRetrievingInvoiceFromReleaseEntry)
						{
							var declarationForReleaseEntry = USReleaseDeclarationLoader.GetReleaseDeclaration(declaration.Factory, US_ReleaseEntryNumber, declaration.JE_GC);
							if (declarationForReleaseEntry != null && declarationForReleaseEntry.PK != declaration.PK)
							{
								new ReleaseEntryInvoiceRetriever(declaration).ImportInvoice(declarationForReleaseEntry, this);
							}
						}
						new DeclarationPrelimStatementDetailsDefaulter().Default(declaration);
					}
				}
			}
		}

		public bool IsRetrievingInvoiceFromReleaseEntry { get; set; } = true;

		#region Calculate InvoiceLines With PGA Indicators

		public bool HasInvoiceLinesWithFDADisclaim
		{
			get
			{
				if (!hasInvoiceLinesWithFDADisclaim.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithFDADisclaim.Value;
			}
		}

		public bool HasInvoiceLinesWithOGAFDA
		{
			get
			{
				if (!hasInvoiceLinesWithOGAFDA.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithOGAFDA.Value;
			}
		}

		public bool HasInvoiceLinesWithPGAFDA
		{
			get
			{
				if (!hasInvoiceLinesWithPGAFDA.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithPGAFDA.Value;
			}
		}

		public bool HasInvoiceLinesWithDOT
		{
			get
			{
				if (!hasInvoiceLinesWithDOT.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithDOT.Value;
			}
		}

		public bool HasInvoiceLinesWithODS
		{
			get
			{
				if (!hasInvoiceLinesWithODS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithODS.Value;
			}
		}

		public bool HasInvoiceLinesWithVNE
		{
			get
			{
				if (!hasInvoiceLinesWithVNE.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithVNE.Value;
			}
		}

		public bool HasInvoiceLinesWithFSIS
		{
			get
			{
				if (!hasInvoiceLinesWithFSIS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithFSIS.Value;
			}
		}

		public bool HasInvoiceLinesWithPST
		{
			get
			{
				if (!hasInvoiceLinesWithPST.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithPST.Value;
			}
		}

		public bool HasInvoiceLinesWithHFC
		{
			get
			{
				if (!hasInvoiceLinesWithHFC.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithHFC.Value;
			}
		}

		public bool HasInvoiceLinesWithNHTSARequireIOR
		{
			get
			{
				if (!hasInvoiceLinesWithNHTSARequireIOR.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithNHTSARequireIOR.Value;
			}
		}

		public bool HasInvoiceLinesWithNHTSARequireCB
		{
			get
			{
				if (!hasInvoiceLinesWithNHTSARequireCB.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithNHTSARequireCB.Value;
			}
		}

		public bool HasInvoiceLinesWithNHTSA
		{
			get
			{
				if (!hasInvoiceLinesWithNHTSA.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithNHTSA.Value;
			}
		}

		public bool HasInvoiceLinesWithNMFS
		{
			get
			{
				if (!hasInvoiceLinesWithNMFS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithNMFS.Value;
			}
		}

		public bool HasInvoiceLinesWithAPHIS
		{
			get
			{
				if (!hasInvoiceLinesWithAPHIS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithAPHIS.Value;
			}
		}

		public bool HasInvoiceLinesWithFWS
		{
			get
			{
				if (!hasInvoiceLinesWithFWS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithFWS.Value;
			}
		}

		public bool HasInvoiceLinesWithTSCA
		{
			get
			{
				if (!hasInvoiceLinesWithTSCA.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithTSCA.Value;
			}
		}

		public bool HasInvoiceLinesWithAMS
		{
			get
			{
				if (!hasInvoiceLinesWithAMS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithAMS.Value;
			}
		}

		public bool HasInvoiceLinesWithNOP
		{
			get
			{
				if (!hasInvoiceLinesWithNOP.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithNOP.Value;
			}
		}

		public bool HasInvoiceLinesWithACELacey
		{
			get
			{
				if (!hasInvoiceLinesWithACELacey.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithACELacey.Value;
			}
		}

		public bool HasInvoiceLinesWithATF
		{
			get
			{
				if (!hasInvoiceLinesWithATF.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithATF.Value;
			}
		}

		public bool HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2
		{
			get
			{
				if (!hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2.Value;
			}
		}

		public bool HasInvoiceLinesWithTTB
		{
			get
			{
				if (!hasInvoiceLinesWithTTB.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithTTB.Value;
			}
		}

		public bool HasInvoiceLinesWithDDTC
		{
			get
			{
				if (!hasInvoiceLinesWithDDTC.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithDDTC.Value;
			}
		}

		public bool HasInvoiceLinesWithOMC
		{
			get
			{
				if (!hasInvoiceLinesWithOMC.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithOMC.Value;
			}
		}

		bool? hasInvoiceLinesWithOMC;

		public bool HasInvoiceLinesWithODSOrTSCAARequireIM
		{
			get
			{
				if (!hasInvoiceLinesWithODSOrTSCAARequireIM.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithODSOrTSCAARequireIM.Value;
			}
		}

		public bool HasInvoiceLinesWithODSOrTSCAARequireCB
		{
			get
			{
				if (!hasInvoiceLinesWithODSOrTSCAARequireCB.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithODSOrTSCAARequireCB.Value;
			}
		}
		public bool HasInvoiceLinesWithDEA
		{
			get
			{
				if (!hasInvoiceLinesWithDEA.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithDEA.Value;
			}
		}

		public bool HasInvoiceLinesWithCPSC
		{
			get
			{
				if (!hasInvoiceLinesWithCPSC.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithCPSC.Value;
			}
		}

		bool? hasInvoiceLinesWithCPSC;

		public bool HasInvoiceLinesWithFWSProcessingCodeWithEDS
		{
			get
			{
				if (!hasInvoiceLinesWithFWSProcessingCodeWithEDS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithFWSProcessingCodeWithEDS.Value;
			}
		}

		bool? hasInvoiceLinesWithATF;
		bool? hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2;
		bool? hasInvoiceLinesWithFDADisclaim;
		bool? hasInvoiceLinesWithOGAFDA;
		bool? hasInvoiceLinesWithPGAFDA;
		bool? hasInvoiceLinesWithDOT;
		bool? hasInvoiceLinesWithODS;
		bool? hasInvoiceLinesWithVNE;
		bool? hasInvoiceLinesWithFSIS;
		bool? hasInvoiceLinesWithPST;
		bool? hasInvoiceLinesWithHFC;
		bool? hasInvoiceLinesWithNHTSA;
		bool? hasInvoiceLinesWithNHTSARequireIOR;
		bool? hasInvoiceLinesWithNHTSARequireCB;
		bool? hasInvoiceLinesWithNMFS;
		bool? hasInvoiceLinesWithAPHIS;
		bool? hasInvoiceLinesWithFWS;
		bool? hasInvoiceLinesWithTSCA;
		bool? hasInvoiceLinesWithAMS;
		bool? hasInvoiceLinesWithNOP;
		bool? hasInvoiceLinesWithACELacey;
		bool? hasInvoiceLinesWithTTB;
		bool? hasInvoiceLinesWithDDTC;
		bool? hasInvoiceLinesWithODSOrTSCAARequireIM;
		bool? hasInvoiceLinesWithODSOrTSCAARequireCB;
		bool? hasInvoiceLinesWithDEA;
		bool? hasInvoiceLinesWithFWSProcessingCodeWithEDS;

		public void RefreshInvoiceLinesWithPGAIndicators()
		{
			hasInvoiceLinesWithFDADisclaim = null;
			hasInvoiceLinesWithOGAFDA = null;
			hasInvoiceLinesWithPGAFDA = null;
			hasInvoiceLinesWithDOT = null;
			hasInvoiceLinesWithODS = null;
			hasInvoiceLinesWithVNE = null;
			hasInvoiceLinesWithFSIS = null;
			hasInvoiceLinesWithPST = null;
			hasInvoiceLinesWithHFC = null;
			hasInvoiceLinesWithNHTSA = null;
			hasInvoiceLinesWithNHTSARequireIOR = null;
			hasInvoiceLinesWithNHTSARequireCB = null;
			hasInvoiceLinesWithNMFS = null;
			hasInvoiceLinesWithAPHIS = null;
			hasInvoiceLinesWithFWS = null;
			hasInvoiceLinesWithTSCA = null;
			hasInvoiceLinesWithAMS = null;
			hasInvoiceLinesWithNOP = null;
			hasInvoiceLinesWithACELacey = null;

			hasInvoiceLinesWithOMC = null;
			hasInvoiceLinesWithATF = null;
			hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2 = null;
			hasInvoiceLinesWithTTB = null;
			hasInvoiceLinesWithDDTC = null;
			hasInvoiceLinesWithODSOrTSCAARequireIM = null;
			hasInvoiceLinesWithODSOrTSCAARequireCB = null;
			hasInvoiceLinesWithDEA = null;
			hasInvoiceLinesWithCPSC = null;
			hasInvoiceLinesWithFWSProcessingCodeWithEDS = null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CalculateInvoiceLinesWithPGAIndicators()
		{
			hasInvoiceLinesWithFDADisclaim = false;
			hasInvoiceLinesWithOGAFDA = false;
			hasInvoiceLinesWithPGAFDA = false;
			hasInvoiceLinesWithDOT = false;

			hasInvoiceLinesWithODS = false;
			hasInvoiceLinesWithVNE = false;
			hasInvoiceLinesWithFSIS = false;
			hasInvoiceLinesWithPST = false;
			hasInvoiceLinesWithHFC = false;
			hasInvoiceLinesWithNHTSA = false;
			hasInvoiceLinesWithNHTSARequireIOR = false;
			hasInvoiceLinesWithNHTSARequireCB = false;
			hasInvoiceLinesWithNMFS = false;
			hasInvoiceLinesWithAPHIS = false;
			hasInvoiceLinesWithFWS = false;
			hasInvoiceLinesWithTSCA = false;
			hasInvoiceLinesWithAMS = false;
			hasInvoiceLinesWithNOP = false;
			hasInvoiceLinesWithACELacey = false;
			hasInvoiceLinesWithOMC = false;

			hasInvoiceLinesWithATF = false;
			hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2 = false;
			hasInvoiceLinesWithTTB = false;
			hasInvoiceLinesWithDDTC = false;
			hasInvoiceLinesWithODSOrTSCAARequireIM = false;
			hasInvoiceLinesWithODSOrTSCAARequireCB = false;
			hasInvoiceLinesWithDEA = false;
			hasInvoiceLinesWithCPSC = false;
			hasInvoiceLinesWithFWSProcessingCodeWithEDS = false;

			foreach (JobComInvoiceLine invoiceLine in this.InvoiceLines)
			{
				var isFDAToBeDeclared = OGAIndicatorList.IsToBeDeclared(invoiceLine.US_FDAIndicator);

				hasInvoiceLinesWithFDADisclaim |= OGAIndicatorList.IsToBeDisclaimed(invoiceLine.US_FDAIndicator);
				hasInvoiceLinesWithOGAFDA |= isFDAToBeDeclared && invoiceLine.FDAs.Count > 0;
				hasInvoiceLinesWithPGAFDA |= isFDAToBeDeclared && invoiceLine.ACE_FDALines.Count > 0;
				hasInvoiceLinesWithDOT |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_DOTIndicator);
				hasInvoiceLinesWithODS |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_ODSInd);
				hasInvoiceLinesWithVNE |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_VNEInd);
				hasInvoiceLinesWithFSIS |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_FSISInd);
				hasInvoiceLinesWithPST |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_PSTIndicator);
				hasInvoiceLinesWithHFC |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_HFCInd);
				hasInvoiceLinesWithNHTSA |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NHTSAIndicator);
				hasInvoiceLinesWithNMFS |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFS370Ind) || OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFSAMRInd) || OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFSHMSInd) || OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NMFSSIMPInd);
				hasInvoiceLinesWithAPHIS |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_APHISInd);
				hasInvoiceLinesWithFWS |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_FWSInd);
				hasInvoiceLinesWithTSCA |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_TSCAInd);
				hasInvoiceLinesWithAMS |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_AMSInd);
				hasInvoiceLinesWithNOP |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_NOPInd);
				hasInvoiceLinesWithACELacey |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_LaceyIndicator);
				hasInvoiceLinesWithATF |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_ATFInd);
				hasInvoiceLinesWithOMC |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_OMCInd);
				hasInvoiceLinesWithTTB |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_TTBInd);
				hasInvoiceLinesWithDDTC |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_DDTCInd);
				hasInvoiceLinesWithCPSC |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_CPSCInd);
				hasInvoiceLinesWithDEA |= OGAIndicatorList.IsToBeDeclared(invoiceLine.US_DEAInd);
				CheckInnerLinesWithCondition(invoiceLine);
			}
		}

		#endregion
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal bool HasAnyPGADataEitherDeclaredOrDisclaimed
		{
			get
			{
				if (hasAnyPGADataEitherDeclaredOrDisclaimed == null)
				{
					hasAnyPGADataEitherDeclaredOrDisclaimed = new CachedProperty<bool>(Factory, delegate
					{
						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							if (OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_APHISInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_ATFInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_CPSCInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_DDTCInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_DEAInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_DOTIndicator)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_FDAIndicator)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_FCCIndicator)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_FWSInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_LaceyIndicator)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_OMCInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_TTBInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_AMSInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_NOPInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_FSISInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_NMFSAMRInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_NMFS370Ind)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_NMFSSIMPInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_NMFSHMSInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_ODSInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_PSTIndicator)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_TSCAInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_VNEInd)
								|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_HFCInd))
							{
								return true;
							}
						}
						return false;
					});
				}
				return hasAnyPGADataEitherDeclaredOrDisclaimed.Value;
			}
		}
		CachedProperty<bool> hasAnyPGADataEitherDeclaredOrDisclaimed;

		#region Calculate InvoiceLines With Specific Columns

		public void RefreshInvoiceLinesWithSpecificColumnsChanged()
		{
			hasInvoiceLinesWithDomesticStatus = null;
			hasInvoiceLinesWithADCVDCaseReported = null;
			hasInvoiceLinesWithSection301Or232 = null;
			allInvoiceLinesAreFromCA = null;
			allInvoiceLinesAreReturnedGoods = null;
		}

		internal bool HasInvoiceLinesWithSection301Or232
		{
			get
			{
				if (!hasInvoiceLinesWithSection301Or232.HasValue)
				{
					CalculateVariousFlags();
				}
				return hasInvoiceLinesWithSection301Or232.Value;
			}
		}
		bool? hasInvoiceLinesWithSection301Or232;

		public bool HasInvoiceLinesWithDomesticStatus
		{
			get
			{
				if (!hasInvoiceLinesWithDomesticStatus.HasValue)
				{
					CalculateVariousFlags();
				}
				return hasInvoiceLinesWithDomesticStatus.Value;
			}
		}

		bool? hasInvoiceLinesWithDomesticStatus;

		internal bool HasInvoiceLinesWithADCVDCaseReported
		{
			get
			{
				if (!hasInvoiceLinesWithADCVDCaseReported.HasValue)
				{
					CalculateVariousFlags();
				}
				return hasInvoiceLinesWithADCVDCaseReported.Value;
			}
		}
		bool? hasInvoiceLinesWithADCVDCaseReported;

		internal bool AllInvoiceLinesAreFromCA
		{
			get
			{
				if (!allInvoiceLinesAreFromCA.HasValue)
				{
					CalculateVariousFlags();
				}
				return allInvoiceLinesAreFromCA.Value;
			}
		}
		bool? allInvoiceLinesAreFromCA;

		internal bool AllInvoiceLinesAreReturnedGoods
		{
			get
			{
				if (!allInvoiceLinesAreReturnedGoods.HasValue)
				{
					CalculateVariousFlags();
				}
				return allInvoiceLinesAreReturnedGoods.Value;
			}
		}
		bool? allInvoiceLinesAreReturnedGoods;

		void CalculateVariousFlags()
		{
			hasInvoiceLinesWithDomesticStatus = false;
			hasInvoiceLinesWithADCVDCaseReported = false;
			hasInvoiceLinesWithSection301Or232 = false;
			allInvoiceLinesAreFromCA = InvoiceLines.Count > 0;
			allInvoiceLinesAreReturnedGoods = InvoiceLines.Count > 0;

			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				hasInvoiceLinesWithDomesticStatus |= invoiceLine.US_ZoneStatus == ZoneStatusList.Codes.Domestic;
				hasInvoiceLinesWithADCVDCaseReported |= !invoiceLine.US_ADDCaseNo.IsEmpty || !invoiceLine.US_CVDCaseNo.IsEmpty;
				hasInvoiceLinesWithSection301Or232 |= invoiceLine.US_SupTariff.StartsWith("9903", StringComparison.OrdinalIgnoreCase);
				allInvoiceLinesAreFromCA &= invoiceLine.US_UC_NKCountryOfOrigin.StartsWith("X", StringComparison.OrdinalIgnoreCase);
				allInvoiceLinesAreReturnedGoods &= invoiceLine.JI_Tariff.StartsWith(ReturnedGoodsTariffPrefix, StringComparison.Ordinal);
			}
		}

		internal const string ReturnedGoodsTariffPrefix = "98010010";

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CheckInnerLinesWithCondition(JobComInvoiceLine invoiceLine)
		{
			hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2 |= (OGAIndicatorList.IsToBeDisclaimed(invoiceLine.US_AMSInd) && invoiceLine.US_AMSDisclaimProgram == AMSProgramList.Codes.MO7) || invoiceLine.AMSLines.OfType<AMS>().Any(x => x.US_Program == AMSProgramList.Codes.MO1 || x.US_Program == AMSProgramList.Codes.MO2);

			hasInvoiceLinesWithNHTSARequireIOR |=
				invoiceLine.NHTSALines.OfType<NHTSAHeader>()
					.Any(
						x =>
							x.NHTSADocuments.HasOrganization(NHTSAOrganizationTypeList.Codes.Importer) ||
							x.US_CertifyingIndividual == PartyTypeList.Codes.Importer);
			hasInvoiceLinesWithNHTSARequireCB |=
				invoiceLine.NHTSALines.OfType<NHTSAHeader>().Any(x => x.US_CertifyingIndividual == PartyTypeList.Codes.CustomsBroker);

			hasInvoiceLinesWithODSOrTSCAARequireIM |= (OGAIndicatorList.IsToBeDeclared(invoiceLine.US_ODSInd) || OGAIndicatorList.IsToBeDeclared(invoiceLine.US_TSCAInd)) && invoiceLine.US_TSCAODSCertIndividual == PartyTypeList.Codes.Importer;
			hasInvoiceLinesWithODSOrTSCAARequireCB |= (OGAIndicatorList.IsToBeDeclared(invoiceLine.US_ODSInd) || OGAIndicatorList.IsToBeDeclared(invoiceLine.US_TSCAInd)) && invoiceLine.US_TSCAODSCertIndividual == PartyTypeList.Codes.CustomsBroker;

			foreach (var fwsHeader in invoiceLine.FWSHeaders.Cast<FWSHeader>())
			{
				hasInvoiceLinesWithFWSProcessingCodeWithEDS |= FWSProcessingCodeList.IsEDS(fwsHeader.US_ProcessingCode);
				if (hasInvoiceLinesWithFWSProcessingCodeWithEDS.Value)
				{
					break;
				}
			}
		}

		internal ZBool HasPSTData
		{
			get
			{
				if (hasPSTDataCache == null)
				{
					hasPSTDataCache = new CachedProperty<ZBool>(Factory, delegate
					{
						return this.InvoiceLines.Any(x => ((JobComInvoiceLine)x).HasPSTLines);
					}
					);
				}
				return hasPSTDataCache.Value;
			}
		}
		CachedProperty<ZBool> hasPSTDataCache;

		public bool HasForeignCountryOfOrigin
		{
			get
			{
				if (hasForeignCountryOfOriginCached == null)
				{
					hasForeignCountryOfOriginCached = new CachedProperty<bool>(Factory, delegate
					{
						foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
						{
							if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(invoiceLine.US_UC_NKCountryOfOrigin) != Core.Constants.CountryCodes.UnitedStates)
							{
								return true;
							}
						}

						return false;
					});
				}
				return hasForeignCountryOfOriginCached.Value;
			}
		}
		CachedProperty<bool> hasForeignCountryOfOriginCached;

		public bool HasInvoiceLinesWithPerishableCommodity
		{
			get
			{
				var result = false;
				foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
				{
					if (invoiceLine.Commodity_Code != null && invoiceLine.Commodity_Code.RH_IsPerishable)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public ZInt TotalNonSecondaryEntrySummaryLinesCount
		{
			get
			{
				List<CusEntryLine> result = new List<CusEntryLine>();

				CusEntryHeader ensEntry = JobDeclaration.ActiveEntryHeaders.EntrySummaryEntry;

				if (ensEntry != null)
				{
					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						if (!invoiceLine.IsSecondaryTariffLine && !result.Contains(invoiceLine.CusEntryLine))
						{
							result.Add(invoiceLine.CusEntryLine);
						}
					}
				}

				return result.Count;
			}
		}

		public bool IsLineGroupingEnabled
		{
			get
			{
				JobDeclaration declaration = JobDeclaration;
				return US_IsLineGrouping && declaration != null && declaration.IsLineGroupingSupported;
			}
		}

		public ZBool PrivilegedStatusDateVisible
		{
			get { return (US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.CurrencyList))]
		public virtual ZString JZ_RX_NKInvoice_CurrencyReadOnly
		{
			get { return JZ_RX_NKInvoice_Currency; }
		}

		public virtual ZPropertyInfo JZ_RX_NKInvoice_CurrencyReadOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_RX_NKInvoice_CurrencyReadOnly); }
		}

		public bool RequireInvoiceLines
		{
			get
			{
				bool result = true;

				if (IsAttachedToPersistentDeclaration)
				{
					result = !JobDeclaration.IsInBondOnly;
				}

				return result;
			}
		}

		public bool IsEntrySummaryValidationMode
		{
			get { return IsAttachedToPersistentDeclaration && JobDeclaration.IsEntrySummaryValidationMode; }
		}

		public bool IsCargoReleaseValidationMode
		{
			get { return IsAttachedToPersistentDeclaration && JobDeclaration.IsCargoReleaseValidationMode; }
		}

		public bool IsStandAlonePriorNoticeMode
		{
			get { return IsAttachedToPersistentDeclaration && JobDeclaration.IsStandAlonePriorNoticeMode; }
		}

		internal ZDecimal EnteredValueThresholdForCharges
		{
			get
			{
				var declaration = JobDeclaration;
				return declaration != null && declaration.IsACE ? 2500m : 1250m;
			}
		}

		public ZString US_EntryNumber
		{
			get
			{
				JobComInvoiceLine firstInvoiceLine = (JobComInvoiceLines.Count > 0) ? JobComInvoiceLines[0] : null;
				return (firstInvoiceLine == null) ? ZString.Empty : firstInvoiceLine.JI_Calc_EntryNumber;
			}
		}

		public ZPropertyInfo US_EntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_EntryNumber); }
		}

		public ZString US_XTN
		{
			get
			{
				JobComInvoiceLine firstInvoiceLine = (JobComInvoiceLines.Count > 0) ? JobComInvoiceLines[0] : null;
				return (firstInvoiceLine == null) ? ZString.Empty : firstInvoiceLine.JI_Calc_XTN;
			}
		}

		public ZPropertyInfo US_XTNInfo
		{
			get { return GetZPropertyInfo(Schema.US_XTN); }
		}

		public override ZDateTime US_DateOfExport
		{
			get { return GetEffectiveValueToReturn(base.US_DateOfExport, JobDeclaration.Schema.US_DateOfExport, Schema.US_DateOfExport); }
			set
			{
				var oldValue = US_DateOfExport;
				base.US_DateOfExport = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_DateOfExport);

				if (!IsCopying)
				{
					var newValue = US_DateOfExport;
					if (oldValue != newValue)
					{
						if (IsImport && IsAttachedToPersistentDeclaration)
						{
							JobDeclaration.MarkValuationDatesDirty();
						}

						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_DateOfExport);
					}
				}
			}
		}

		protected override ZDateTime EffectiveValuationDateCore
		{
			get
			{
				var recordedLatestDateWithExRates = IsImport && IsAttachedToPersistentDeclaration ? JobDeclaration.US_LatestRateDate : ZDateTime.Empty;
				return ValuationDateTracker.GetEffectiveDate(recordedLatestDateWithExRates, ValuationDate_ExportDate);
			}
		}

		internal ZDateTime ValuationDate_ExportDate
		{
			get { return IsImport && US_DateOfExport.IsValid ? US_DateOfExport : base.EffectiveValuationDateCore; }
		}

		internal bool IsLatestRateDateDifferentToExportDateAndRatesExistOnExportDate
		{
			get
			{
				bool result = false;

				if (IsAttachedToPersistentDeclaration && JobDeclaration.IsImport && ValuationDate_ExportDate != EffectiveValuationDate)
				{
					var latestRateDate = ValuationDateTracker.GetLatestRate(JobDeclaration, ValuationDate_ExportDate, GetCurrencyProvidersToRefreshExRatesFor());
					result = latestRateDate.IsValid && latestRateDate > EffectiveValuationDate;
				}

				return result;
			}
		}

		public bool IsNoCharge
		{
			get
			{
				JobDeclaration declaration = JobDeclaration;
				return declaration != null && declaration.IsDrawback;
			}
		}

		public OrgHeader ImporterOfRecord
		{
			get { return (IsAttachedToPersistentDeclaration) ? Factory.Load<OrgHeader>(JobDeclaration.IOROrgPK) : null; }
		}

		#region ManufacturerNameAndID

		public ZString ManufacturerNameAndID
		{
			get
			{
				if (manufacturerNameAndIDCached == null)
				{
					manufacturerNameAndIDCached = new CachedProperty<ZString>(Factory,
						delegate
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

		#region InvoicerOrgPK
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SupplierList))]
		public ZGuid InvoicerOrgPK
		{
			get { return JZ_OA_InvoicerDocAddress_ZAddress.OrgPK; }
			set { JZ_OA_InvoicerDocAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo InvoicerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.InvoicerOrgPK, x => JZ_OA_InvoicerDocAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		public ZBool HasAnEntryWithACertifiedCargoRelease
		{
			get
			{
				if (hasAnEntryWithACertifiedCargoReleaseCached == null)
				{
					hasAnEntryWithACertifiedCargoReleaseCached = new CachedProperty<ZBool>(Factory, delegate
					{
						foreach (CusEntryHeader entry in Entries)
						{
							if (entry.IsFormalEntry && entry.HasCargoReleaseBeenCertified)
							{
								return ZBool.True;
							}
						}
						return ZBool.False;
					}
					);
				}
				return hasAnEntryWithACertifiedCargoReleaseCached.Value;
			}
		}
		CachedProperty<ZBool> hasAnEntryWithACertifiedCargoReleaseCached;

		public bool IsUSUltimateConsignee
		{
			get { return ConsigneeOrgAddress != null && ConsigneeOrgAddress.OH_RL_NKClosestPort.StartsWith(Core.Constants.CountryCodes.UnitedStates); }
		}

		public bool HasFDAToDeclare
		{
			get
			{
				if (hasFDAToDeclareCached == null)
				{
					hasFDAToDeclareCached = new CachedProperty<bool>(Factory, delegate
					{
						foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
						{
							if (OGAIndicatorList.IsToBeDeclared(invoiceLine.US_FDAIndicator))
							{
								return true;
							}
						}
						return false;
					});
				}
				return hasFDAToDeclareCached.Value;
			}
		}
		CachedProperty<bool> hasFDAToDeclareCached;

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceHeader|US_FDAContactName", Caption = "Broker PGA Contact Name")]
		public override ZString US_FDAContactName
		{
			get { return GetEffectiveValueToReturn(base.US_FDAContactName, JobDeclaration.Schema.US_FDAContactName, Schema.US_FDAContactName); }
			set
			{
				if (!IsCopying && US_FDAContactName != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDAContactName = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_FDAContactName);
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceHeader|US_FDAContactPhoneNo", Caption = "Broker PGA Contact Phone")]
		public override ZString US_FDAContactPhoneNo
		{
			get { return GetEffectiveValueToReturn(base.US_FDAContactPhoneNo, JobDeclaration.Schema.US_FDAContactPhoneNo, Schema.US_FDAContactPhoneNo); }
			set
			{
				if (!IsCopying && US_FDAContactPhoneNo != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDAContactPhoneNo = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_FDAContactPhoneNo);
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceHeader|US_FDAContactEmail", Caption = "Broker PGA Contact Email")]
		public override ZString US_FDAContactEmail
		{
			get { return GetEffectiveValueToReturn(base.US_FDAContactEmail, JobDeclaration.Schema.US_FDAContactEmail, Schema.US_FDAContactEmail); }
			set
			{
				if (!IsCopying && US_FDAContactEmail != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDAContactEmail = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_FDAContactEmail);
			}
		}

		#region Invoicer Doc Address

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
			InvoiceLines.MarkAsNeedingValidation();
		}

		[RelatedBusinessObject(nameof(InvoicerDocAddress))]
		[List(nameof(JZ_OA_InvoicerDocAddress_ZAddress) + "+" + nameof(ZAddress.OrgAddress_List))]
		public ZGuid JZ_OA_InvoicerDocAddress
		{
			get
			{
				var result = InvoicerDocumentaryAddress.E2_OA_Address;
				if (result.IsEmpty && IsAttachedToPersistentDeclaration)
				{
					var invoicerAddressOrgPK = JZ_OA_InvoicerDocAddress_ZAddress.OrgPK;
					if (invoicerAddressOrgPK != ZGuid.Invalid)
					{
						var declaration = JobDeclaration;
						var decInvoicerAddress = declaration.JE_OA_InvoicerAddress;
						if (!decInvoicerAddress.IsEmpty)
						{
							result = decInvoicerAddress;
						}

						var decInvoicerAddressOrgPK = declaration.JE_OA_InvoicerAddress_ZAddress.OrgPK;
						if (invoicerAddressOrgPK != decInvoicerAddressOrgPK)
						{
							JZ_OA_InvoicerDocAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(decInvoicerAddressOrgPK);
						}
					}
				}

				return result;
			}
			set
			{
				var oldValue = JZ_OA_InvoicerDocAddress;
				InvoicerDocumentaryAddress.E2_OA_Address = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OA_InvoicerAddress);

				if (oldValue != JZ_OA_InvoicerDocAddress && IsImport)
				{
					ClearFDAShipperAddressIfSame();
				}
				Validation.ValidateJZ_OA_InvoicerDocAddress();
				JZ_OA_InvoicerDocAddressInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JZ_OA_InvoicerDocAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_OA_InvoicerDocAddress); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress JZ_OA_InvoicerDocAddress_ZAddress
		{
			get
			{
				if (fJZ_OA_InvoicerDocAddress_ZAddress == null)
				{
					fJZ_OA_InvoicerDocAddress_ZAddress = new ZAddress(JZ_OA_InvoicerDocAddressInfo);
					fJZ_OA_InvoicerDocAddress_ZAddress.IsOrgVisible = true;
					fJZ_OA_InvoicerDocAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
					fJZ_OA_InvoicerDocAddress_ZAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;
				}
				return fJZ_OA_InvoicerDocAddress_ZAddress;
			}
		}
		ZAddress fJZ_OA_InvoicerDocAddress_ZAddress;

		public OrgAddress InvoicerDocAddress
		{
			get { return Factory.Load<OrgAddress>(JZ_OA_InvoicerDocAddress); }
		}

		#endregion

		#region FDA Shipper Address

		public JobDocAddress FDAShipperDocumentaryAddress
		{
			get
			{
				if (fFDAShipperDocumentaryAddress == null || fFDAShipperDocumentaryAddress.IsDeleted)
				{
					fFDAShipperDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.FDAShipperAddress);
					fFDAShipperDocumentaryAddress.OnRelationshipFieldsChanged += new EventHandler(FDAShipperDocumentaryAddress_OnRelationshipFieldsChanged);
				}
				return fFDAShipperDocumentaryAddress;
			}
		}
		JobDocAddress fFDAShipperDocumentaryAddress;

		void FDAShipperDocumentaryAddress_OnRelationshipFieldsChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
			InvoiceLines.MarkAsNeedingValidation();
		}

		[RelatedBusinessObject(nameof(FDAShipperAddress))]
		[List(nameof(JZ_OA_FDAShipperAddress_ZAddress) + "+" + nameof(ZAddress.OrgAddress_List))]
		public ZGuid JZ_OA_FDAShipperAddress
		{
			get
			{
				var result = FDAShipperDocumentaryAddress.E2_OA_Address;
				if (result.IsEmpty)
				{
					var fdaShipperAddressOrgPK = JZ_OA_FDAShipperAddress_ZAddress.OrgPK;
					if (fdaShipperAddressOrgPK != ZGuid.Invalid)
					{
						var invoicerAddress = JZ_OA_InvoicerDocAddress;
						if (!invoicerAddress.IsEmpty)
						{
							result = invoicerAddress;

							var invoicerAddressOrgPk = JZ_OA_InvoicerDocAddress_ZAddress.OrgPK;
							if (fdaShipperAddressOrgPK != invoicerAddressOrgPk)
							{
								JZ_OA_FDAShipperAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(invoicerAddressOrgPk);
							}
						}
						else
						{
							var supplierAddress = JZ_OA_SupplierAddress;
							if (!supplierAddress.IsEmpty)
							{
								result = JZ_OA_SupplierAddress;

								var supplierAddressOrgPK = JZ_OA_SupplierAddress_ZAddress.OrgPK;
								if (fdaShipperAddressOrgPK != supplierAddressOrgPK)
								{
									JZ_OA_FDAShipperAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(supplierAddressOrgPK);
								}
							}
							else if (!fdaShipperAddressOrgPK.IsEmpty)
							{
								JZ_OA_FDAShipperAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
							}
						}
					}
				}
				return result;
			}
			set
			{
				var oldValue = JZ_OA_FDAShipperAddress;
				FDAShipperDocumentaryAddress.E2_OA_Address = GetEffectiveValueToSet(value, Schema.JZ_OA_InvoicerDocAddress, Schema.JZ_OA_SupplierAddress);
				Validation.ValidateJZ_OA_FDAShipperAddress();
				JZ_OA_FDAShipperAddressInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JZ_OA_FDAShipperAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JZ_OA_FDAShipperAddress); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress JZ_OA_FDAShipperAddress_ZAddress
		{
			get
			{
				if (fJZ_OA_FDAShipperAddress_ZAddress == null)
				{
					fJZ_OA_FDAShipperAddress_ZAddress = new ZAddress(JZ_OA_FDAShipperAddressInfo);
					fJZ_OA_FDAShipperAddress_ZAddress.IsOrgVisible = true;
					fJZ_OA_FDAShipperAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
					fJZ_OA_FDAShipperAddress_ZAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;
				}
				return fJZ_OA_FDAShipperAddress_ZAddress;
			}
		}
		ZAddress fJZ_OA_FDAShipperAddress_ZAddress;

		public OrgAddress FDAShipperAddress
		{
			get { return Factory.Load<OrgAddress>(JZ_OA_FDAShipperAddress); }
		}

		#endregion

		#region

		#endregion

		#endregion

		#region Properties Overriden
		protected override ZString LocalCurrencyCodeCore
		{
			get { return JobDeclaration.LocalCurrencyConstantCode; }
		}

		protected override ZBool IsImportForStandAloneInvoice
		{
			get { return base.IsImportForStandAloneInvoice || JobMessageTypeList.IsImport(JZ_MessageType); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_YesNoList))]
		[ReadOnlyMember(nameof(US_DeductADDCVDDuty_ReadOnly))]
		public override ZString US_DeductADDCVDDuty
		{
			get => base.US_DeductADDCVDDuty;
			set => base.US_DeductADDCVDDuty = value;
		}

		bool US_DeductADDCVDDuty_ReadOnly
		{
			get
			{
				return !IsDeductADD_CVDDutyRequired;
			}
		}

		public override ZGuid JZ_CU_RelatedHouseBill
		{
			get { return base.JZ_CU_RelatedHouseBill; }
			set
			{
				JobDeclaration declaration = null;
				if (base.JZ_CU_RelatedHouseBill != value)
				{
					declaration = JobDeclaration;
					if (declaration != null)
					{
						PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
						declaration.Bills.MarkAsNeedingValidation();
					}
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						invoiceLine.MarkAsNeedingValidation();
						invoiceLine.ContainersPivot.MarkAsNeedingValidation();
					}
				}
				var oldValue = JZ_CU_RelatedHouseBill;
				base.JZ_CU_RelatedHouseBill = value;
				if (!IsCopying && oldValue != JZ_CU_RelatedHouseBill)
				{
					if (declaration != null && declaration.IsFTZAdmission)
					{
						US_SplitShipmentDetail = ZString.Empty;
						declaration.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid JZ_OA_SoldToPartyAddress
		{
			get
			{
				return GetEffectiveAddressValueToReturn(base.JZ_OA_SoldToPartyAddress,
					Schema.JZ_OA_SoldToPartyAddress,
					Schema.SoldToPartyOrgPK,
					JobDeclaration.Schema.JE_OA_SoldToPartyAddress,
					disableSettingSoldToPartyDefaults,
					(addressOrgPK) => SetJZ_OA_SoldToPartyAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPK),
					!IsCreatedFromUSLowValue);
			}
			set
			{
				var oldValue = base.JZ_OA_SoldToPartyAddress;
				var newValue = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OA_SoldToPartyAddress, !IsCreatedFromUSLowValue);
				if (oldValue != newValue)
				{
					base.JZ_OA_SoldToPartyAddress = newValue;
				}
				else
				{
					Validation.ValidateJZ_OA_SoldToPartyAddress();
				}

				JobComInvoiceLines.MarkAsNeedingValidation();

				if (!IsCopying)
				{
					var effectiveValue = JZ_OA_SoldToPartyAddress;
					if (oldValue != effectiveValue)
					{
						ClearInvoiceLineValuesIfSame(effectiveValue, JobComInvoiceLine.Schema.JI_OA_SoldToPartyAddress);
					}
				}
			}
		}

		public override ZGuid JZ_OA_ShipToPartyAddress
		{
			get
			{
				return GetEffectiveAddressValueToReturn(base.JZ_OA_ShipToPartyAddress,
														Schema.JZ_OA_ShipToPartyAddress,
														Schema.ShipToPartyOrgPK,
														JobDeclaration.Schema.JE_OA_ShipToPartyAddress,
														disableSettingShipToPartyDefaults,
														(addressOrgPK) => SetJZ_OA_ShipToPartyAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPK),
														!IsCreatedFromUSLowValue);
			}
			set
			{
				var oldValue = base.JZ_OA_ShipToPartyAddress;
				var newValue = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OA_ShipToPartyAddress, !IsCreatedFromUSLowValue);
				if (oldValue != newValue)
				{
					base.JZ_OA_ShipToPartyAddress = newValue;
				}
				else
				{
					Validation.ValidateJZ_OA_ShipToPartyAddress();
				}

				JobComInvoiceLines.MarkAsNeedingValidation();

				if (!IsCopying)
				{
					var effectiveValue = JZ_OA_ShipToPartyAddress;
					if (oldValue != effectiveValue)
					{
						ClearInvoiceLineValuesIfSame(effectiveValue, JobComInvoiceLine.Schema.JI_OA_ShipToPartyAddress);
					}
				}
			}
		}

		public override ZGuid JZ_OA_ExporterAddress
		{
			get
			{
				ZGuid result = base.JZ_OA_ExporterAddress;
				if (result.IsEmpty && IsAttachedToPersistentDeclaration && !IsCreatedFromUSLowValue)
				{
					ZGuid exporterAddressOrgPK = JZ_OA_ExporterAddress_ZAddress.OrgPK;
					if (exporterAddressOrgPK != ZGuid.Invalid)
					{
						result = GetDeclarationExporterAddressPK();
						var declaration = JobDeclaration;
						var exporter = declaration.Exporter;
						if (!disableSettingExporterDefaults)
						{
							if (exporterAddressOrgPK != declaration.JE_OH_Exporter && declaration.JE_OH_Exporter.IsValid)
							{
								if (exporter != null)
								{
									SetJZ_OA_ExporterAddress_ZAddressOrgPKWithoutSettingDefaults(declaration.JE_OH_Exporter);
								}
							}
						}
					}
				}
				return result;
			}
			set
			{
				var oldDisableSetting = disableSettingExporterDefaults;
				ZGuid oldValue;
				try
				{
					disableSettingExporterDefaults = true;
					oldValue = JZ_OA_ExporterAddress;
					if (!IsCopying && JZ_OA_ExporterAddress != value)
					{
						PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
					}
				}
				finally
				{
					disableSettingExporterDefaults = oldDisableSetting;
				}
				ZGuid newValue = value;

				if (IsAttachedToPersistentDeclaration)
				{
					if (newValue == GetDeclarationExporterAddressPK() && !IsCreatedFromUSLowValue)
					{
						newValue = ZGuid.Empty;
					}
				}
				base.JZ_OA_ExporterAddress = newValue;

				if (!IsCopying && oldValue != newValue)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
					ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.JI_OA_ExporterAddress);
				}
			}
		}

		ZGuid GetDeclarationExporterAddressPK()
		{
			ZGuid result = ZGuid.Empty;
			var declaration = JobDeclaration;
			var exporter = declaration.Exporter;
			if (exporter != null)
			{
				if (declaration.IsACE)
				{
					result = DefaultAddressRelatedDeterminer.GetMIDAddress(exporter);
				}
				else
				{
					result = exporter.MainAddress.PK;
				}
			}
			return result;
		}

		public override ZGuid JZ_OH_Consignee
		{
			get { return useBaseJZ_OH_Consignee ? base.JZ_OH_Consignee : IsAttachedToPersistentExportDeclaration ? IntermediateConsigneeDocAddress.OrganisationPK : base.JZ_OH_Consignee; }
			set
			{
				if (!settingJZ_OH_ConsigneeInProgress)
				{
					try
					{
						settingJZ_OH_ConsigneeInProgress = true;
						if (value != JZ_OH_Consignee)
						{
							((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = true;
						}

						try
						{
							useBaseJZ_OH_Consignee = true;
							base.JZ_OH_Consignee = value; // cause base defaulting to be fired
						}
						finally
						{
							useBaseJZ_OH_Consignee = false;
						}
						if (IsAttachedToPersistentExportDeclaration)
						{
							IntermediateConsigneeDocAddress.OrganisationPK = value;
						}
						if (!IsValidationSuspended)
						{
							Validation.ValidateJZ_OH_Consignee();
						}
					}
					finally
					{
						settingJZ_OH_ConsigneeInProgress = false;
					}
				}
			}
		}
		bool settingJZ_OH_ConsigneeInProgress;
		bool useBaseJZ_OH_Consignee;

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.USCountryList))]
		public override ZString JZ_RN_NKDefaultOrigin
		{
			get { return base.JZ_RN_NKDefaultOrigin; }
			set { base.JZ_RN_NKDefaultOrigin = value; }
		}

		public override ZString US_ZoneStatus
		{
			get { return base.US_ZoneStatus; }
			set
			{
				var oldValue = base.US_ZoneStatus; // need base value instead of effective
				if (oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_ZoneStatus = value;
				if (!IsCopying)
				{
					var newValue = US_ZoneStatus;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_ZoneStatus);
					}
				}
			}
		}

		public override ZDateTime US_PrivilegedStatusDate
		{
			get { return base.US_PrivilegedStatusDate; }
			set
			{
				var oldValue = US_PrivilegedStatusDate;
				base.US_PrivilegedStatusDate = value;
				if (!IsCopying)
				{
					var newValue = US_PrivilegedStatusDate;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_PrivilegedStatusDate);
					}
				}
			}
		}

		public override ZGuid JZ_OA_ManufacturerAddress
		{
			get
			{
				return GetEffectiveAddressValueToReturn(base.JZ_OA_ManufacturerAddress,
														Schema.JZ_OA_ManufacturerAddress,
														Schema.ManufacturerOrgPK,
														JobDeclaration.Schema.JE_OA_ManufacturerAddress,
														disableSettingManufacturerDefaults,
														(addressOrgPK) => SetJZ_OA_ManufacturerAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPK),
														!IsCreatedFromUSLowValue);
			}
			set
			{
				var oldValue = base.JZ_OA_ManufacturerAddress; // need base value and not effective value
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}

				// need to compare with actual value before setting and always validate.
				var newValue = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OA_ManufacturerAddress, !IsCreatedFromUSLowValue);
				if (oldValue != newValue)
				{
					base.JZ_OA_ManufacturerAddress = newValue;
				}
				else
				{
					Validation.ValidateJZ_OA_ManufacturerAddress();
				}

				JobComInvoiceLines.MarkAsNeedingValidation();

				if (!IsCopying)
				{
					var effectiveValue = JZ_OA_ManufacturerAddress;
					if (oldValue != effectiveValue)
					{
						ClearInvoiceLineValuesIfSame(effectiveValue, JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress);

						if (effectiveValue.IsValid)
						{
							DefaultCountryOfOriginFromOrganisationDetail(ManufacturerAddress);
						}
					}
				}

				RefreshFDAManufacturerAddressForAllLines();
			}
		}

		void RefreshFDAManufacturerAddressForAllLines()
		{
			foreach (JobComInvoiceLine line in InvoiceLines)
			{
				line.RefreshFDAManufacturerAddress();
			}
		}

		public OrgHeader USBuyer
		{
			get { return Factory.Load<OrgHeader>(BuyerOrgPK); }
		}

		public new ZGuid BuyerOrgPK
		{
			get
			{
				var buyerPK = Factory.Load<OrgAddress>(JZ_OA_BuyerAddress)?.OA_OH ?? ZGuid.Empty;
				if (buyerPK.IsEmpty && IsAttachedToPersistentDeclaration)
				{
					buyerPK = JobDeclaration.JE_OH_Buyer;
				}
				return buyerPK;
			}
			set
			{
				var buyerAddressPk = Factory.Load<OrgHeader>(value)?.MainAddress.PK ?? ZGuid.Empty;
				JZ_OA_BuyerAddress = buyerAddressPk;
				BuyerOrgPKInfo.RefreshBinding();
			}
		}

		public new ZPropertyInfo BuyerOrgPKInfo
		{
			get { return GetZPropertyInfo(Schema.BuyerOrgPK); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.BuyerAgents))]
		public override ZGuid JZ_OH_BuyerAgent
		{
			get { return GetEffectiveValueToReturn(base.JZ_OH_BuyerAgent, JobDeclaration.Schema.JE_OH_BuyingAgent, Schema.JZ_OH_BuyerAgent); }
			set
			{
				base.JZ_OH_BuyerAgent = GetEffectiveValueToSet(value, JobDeclarationSchema.JE_OH_BuyingAgent.Name);
				MarkAsNeedingValidation();
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.Consignors))]
		public new ZGuid ExporterOrgPK
		{
			get { return base.ExporterOrgPK; }
			set
			{
				base.ExporterOrgPK = value;
			}
		}

		public override ZGuid JZ_OA_SupplierAddress
		{
			get
			{
				ZGuid result = base.JZ_OA_SupplierAddress;
				if (result.IsEmpty && IsAttachedToPersistentDeclaration && !IsCreatedFromUSLowValue)
				{
					ZGuid supplierAddressOrgPK = JZ_OA_SupplierAddress_ZAddress.OrgPK;
					if (supplierAddressOrgPK != ZGuid.Invalid)
					{
						result = GetDeclarationSupplierAddressPK();
						if (!disableSettingSupplierDefaults)
						{
							var declaration = JobDeclaration;
							if (supplierAddressOrgPK != declaration.JE_OH_Supplier && declaration.JE_OH_Supplier.IsValid)
							{
								var supplier = declaration.Supplier;
								if (supplier != null && supplier.CountryCode == Core.Constants.CountryCodes.UnitedStates)
								{
									SetJZ_OA_SupplierAddress_ZAddressOrgPKWithoutSettingDefaults(JobDeclaration.JE_OH_Supplier);
								}
							}
						}
					}
				}

				return result;
			}
			set
			{
				var oldDisableSetting = disableSettingSupplierDefaults;
				ZGuid oldValue;
				try
				{
					disableSettingSupplierDefaults = true;
					oldValue = JZ_OA_SupplierAddress;
				}
				finally
				{
					disableSettingSupplierDefaults = oldDisableSetting;
				}

				ZGuid newValue = value;
				if (!IsCopying && oldValue != newValue)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}

				if (IsAttachedToPersistentDeclaration && !IsCreatedFromUSLowValue)
				{
					if (newValue == GetDeclarationSupplierAddressPK())
					{
						newValue = ZGuid.Empty;
					}
				}

				base.JZ_OA_SupplierAddress = newValue;

				SetManufacturerAddress();
				if (!IsCopying)
				{
					if (oldValue != JZ_OA_SupplierAddress)
					{
						if (IsImport)
						{
							if (JZ_OA_SupplierAddress.IsValid && !IsAttachedToPersistentDeclaration)
							{
								if (USCustomsDataRegistry.Instance.DefaultSellerFromSupplier.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
								{
									JZ_OA_SellerAddress = JZ_OA_SupplierAddress;
								}
							}

							ClearFDAShipperAddressIfSame();

							if (JZ_OA_SupplierAddress.IsValid && !JZ_OA_ManufacturerAddress.IsValid)
							{
								DefaultCountryOfOriginFromOrganisationDetail(SupplierAddress);
							}

							if (JobDeclaration is JobDeclaration declaration && JobComInvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.Part != null))
							{
								declaration.MarkReconIndicatorsDirty();
							}
							DefaultFirstSaleIndFromSupplierImporterLink();
						}
					}
				}

				JobComInvoiceLines.MarkAsNeedingValidation();
			}
		}

		void SetManufacturerAddress()
		{
			if (IsImport && JZ_OA_SupplierAddress.IsValid)
			{
				if (USCustomsDataRegistry.Instance.DefaultManufacturerFromSupplier.GetFallBackValueAtAllLevels(this.RegistryCompanyPK, this.RegistryBranchPK, Guid.Empty))
				{
					var declaration = JobDeclaration;
					if (declaration != null)
					{
						if (declaration.JE_OH_Supplier == JZ_OA_SupplierAddress_ZAddress.OrgPK)
						{
							if (declaration.JE_OA_ManufacturerAddress.IsValid)
							{
								JZ_OA_ManufacturerAddress = declaration.JE_OA_ManufacturerAddress;
							}
						}
						else
						{
							JZ_OA_ManufacturerAddress = JZ_OA_SupplierAddress;
						}
					}
				}
			}
		}

		public ZString ManufacturerFallBackToSupplierNumber
		{
			get
			{
				var result = ZString.Empty;
				IOrganisationDetails organisationDetails = ManufacturerDetails ?? OrganisationDetails.New(JZ_OA_SupplierAddressInfo, OrgMatchedCustomsRegNoType.MID);

				if (organisationDetails != null)
				{
					result = organisationDetails.MatchedCustomsRegoNumber;
				}

				return result.ToUpper();
			}
		}

		public override ZGuid JZ_OA_SellerAddress
		{
			get
			{
				return GetEffectiveAddressValueToReturn(base.JZ_OA_SellerAddress,
														Schema.JZ_OA_SellerAddress,
														Schema.SellerOrgPK,
														JobDeclaration.Schema.JE_OA_SellerAddress,
														disableSettingSellerDefaults,
														(addressOrgPK) => SetJZ_OA_Seller_ZAddressOrgPKWithoutSettingDefaults(addressOrgPK),
														!IsCreatedFromUSLowValue);
			}

			set
			{
				var oldValue = base.JZ_OA_SellerAddress; // need base value and not effective value
				var newValue = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OA_SellerAddress, !IsCreatedFromUSLowValue);
				if (!IsCopying && oldValue != value)
				{
					base.JZ_OA_SellerAddress = newValue;
					if (newValue.IsValid)
					{
						InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.PSTLines.Cast<Pesticide>().ForEach(pst => pst.RefreshUS_OA_ShipperAddress_ZAddress()));
					}
				}
				else
				{
					Validation.ValidateJZ_OA_SellerAddress();
				}

				JobComInvoiceLines.MarkAsNeedingValidation();
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.SellingAgents))]
		public override ZGuid JZ_OH_SellingAgent
		{
			get { return GetEffectiveValueToReturn(base.JZ_OH_SellingAgent, JobDeclaration.Schema.JE_OH_SellingAgent, Schema.JZ_OH_SellingAgent); }
			set
			{
				base.JZ_OH_SellingAgent = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OH_SellingAgent);
				MarkAsNeedingValidation();
			}
		}

		void ClearFDAShipperAddressIfSame()
		{
			var valueToMatchAgainst = JZ_OA_InvoicerDocAddress.IsEmpty ? JZ_OA_SupplierAddress : JZ_OA_InvoicerDocAddress;
			var fdaShipperAddress = FDAShipperDocumentaryAddress.E2_OA_Address;
			if (!fdaShipperAddress.IsEmpty && fdaShipperAddress == valueToMatchAgainst)
			{
				JZ_OA_FDAShipperAddress = ZGuid.Empty;
			}
			else
			{
				JZ_OA_FDAShipperAddressInfo.RefreshBinding();
			}

			ClearFDALinesShipperAddressIfSame();
		}

		void ClearFDALinesShipperAddressIfSame()
		{
			foreach (JobComInvoiceLine line in InvoiceLines)
			{
				foreach (FDA fda in line.FDAs)
				{
					if (fda.US_FDAShipperAddress == JZ_OA_FDAShipperAddress)
					{
						fda.US_FDAShipperAddress = ZGuid.Empty;
					}
				}
			}
		}

		public override ZGuid JZ_OA_ConsigneeAddress
		{
			get { return GetEffectiveAddressValueToReturn(base.JZ_OA_ConsigneeAddress, Schema.JZ_OA_ConsigneeAddress, Schema.ConsigneeAddressOrgPK, JobDeclaration.Schema.JE_OA_ConsigneeAddress, disableSettingConsigneeDefaults, (addressOrgPK) => SetJZ_OA_ConsigneeAddressOrgPKWithoutSettingDefaults(addressOrgPK), !IsCreatedFromUSLowValue); }
			set
			{
				var oldValue = base.JZ_OA_ConsigneeAddress; // need base value and not effective value
				if (!IsCopying && JZ_OA_ConsigneeAddress != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.JZ_OA_ConsigneeAddress = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OA_ConsigneeAddress, !IsCreatedFromUSLowValue);
				if (!IsCopying)
				{
					var newValue = JZ_OA_ConsigneeAddress;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress);
						if (newValue.IsValid && USCustomsDataRegistry.Instance.DoDefaultShipTo.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
						{
							JZ_OA_ShipToPartyAddress = value;
						}
					}
				}

				JobComInvoiceLines.MarkAsNeedingValidation();
			}
		}

		public override ZGuid JZ_OA_DistributorAddress
		{
			get { return GetEffectiveAddressValueToReturn(base.JZ_OA_DistributorAddress, Schema.JZ_OA_DistributorAddress, Schema.DistributorOrgPK, JobDeclaration.Schema.JE_OA_DistributorAddress, disableSettingDistributorDefaults, (addressOrgPk) => SetJZ_OA_DistributorAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPk)); }
			set
			{
				var oldValue = base.JZ_OA_DistributorAddress;
				var newValue = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OA_DistributorAddress);
				if (oldValue != newValue)
				{
					base.JZ_OA_DistributorAddress = value;
				}
				else
				{
					Validation.ValidateJZ_OA_DistributorAddress();
				}
			}
		}

		public override ZGuid JZ_OA_ShipperAddress
		{
			get { return GetEffectiveAddressValueToReturn(base.JZ_OA_ShipperAddress, Schema.JZ_OA_ShipperAddress, Schema.ShipperOrgPK, JobDeclaration.Schema.JE_OA_ShipperAddress, disableSettingShipperDefaults, (addressOrgPk) => SetJZ_OA_ShipperAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPk), !IsCreatedFromUSLowValue); }
			set
			{
				var oldValue = base.JZ_OA_ShipperAddress;
				var newValue = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OA_ShipperAddress, !IsCreatedFromUSLowValue);
				if (oldValue != newValue)
				{
					base.JZ_OA_ShipperAddress = value;
				}
				else
				{
					Validation.ValidateJZ_OA_ShipperAddress();
				}
			}
		}

		public override ZGuid JZ_OA_PackagerAddress
		{
			get { return GetEffectiveAddressValueToReturn(base.JZ_OA_PackagerAddress, Schema.JZ_OA_PackagerAddress, Schema.PackagerOrgPK, JobDeclaration.Schema.JE_OA_PackagerAddress, disableSettingPackagerDefaults, (addressOrgPk) => SetJZ_OA_PackagerAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPk)); }
			set
			{
				var oldValue = base.JZ_OA_PackagerAddress;
				var newValue = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OA_PackagerAddress);
				if (oldValue != newValue)
				{
					base.JZ_OA_PackagerAddress = value;
				}
				else
				{
					Validation.ValidateJZ_OA_PackagerAddress();
				}
			}
		}

		public override ZString US_UltimateConsigneeType
		{
			get { return base.US_UltimateConsigneeType; }
			set
			{
				var oldValue = US_UltimateConsigneeType;
				base.US_UltimateConsigneeType = value;
				if (oldValue != US_UltimateConsigneeType)
				{
					US_UltimateConsigneeTypeInfo.RefreshBinding(oldValue);
				}
			}
		}

		#region GetEffectiveValuesFromDeclaration

		T GetEffectiveValueToReturn<T>(T baseValue, string fieldNameInJobDeclaration, string invoiceFieldName) where T : IZType
		{
			T result = baseValue;

			if (result.IsEmpty && IsAttachedToPersistentDeclaration)
			{
				IZType effectiveValue = EffectiveValueManager.GetEffectiveValue(invoiceFieldName) ?? (IZType)JobDeclaration[fieldNameInJobDeclaration];
				result = (T)effectiveValue;
			}

			return result;
		}

		ZGuid GetEffectiveAddressValueToReturn(ZGuid baseValue, string addressField, string addressOrgPKField, string addressJobDecField, ZBool disableSettingDefaults, Action<ZGuid> setAddressOrgPKWithoutSettingDefaults, bool shouldGetParentValue = true)
		{
			return EffectiveValueManager.GetEffectiveValueToReturn(baseValue, addressField, () =>
			{
				var result = ZGuid.Empty;
				if (IsAttachedToPersistentDeclaration && shouldGetParentValue)
				{
					var declaration = JobDeclaration;
					var addressOrgPK = (ZGuid)this[addressOrgPKField];
					var addressOrgPKInDeclaration = (ZGuid)declaration[addressOrgPKField];
					if (addressOrgPK != ZGuid.Invalid && addressOrgPKInDeclaration != ZGuid.Invalid)
					{
						result = (ZGuid)declaration[addressJobDecField];
						if (!disableSettingDefaults && addressOrgPK != addressOrgPKInDeclaration)
						{
							setAddressOrgPKWithoutSettingDefaults(addressOrgPKInDeclaration);
						}
					}
				}
				return result;
			});
		}

		T GetEffectiveValueToSet<T>(T valuePassed, string fieldNameInJobDeclaration, bool shouldClearIfSame = true) where T : IZType
		{
			T result = valuePassed;

			if (!valuePassed.IsDefault && IsAttachedToPersistentDeclaration && shouldClearIfSame && JobDeclaration[fieldNameInJobDeclaration].Equals(valuePassed))
			{
				result = (T)valuePassed.Default;
			}

			return result;
		}

		T GetEffectiveValueToSet<T>(T valuePassed, ZString fieldName1InJobComInvoiceHeader, ZString fieldName2InJobComInvoiceHeader) where T : IZType
		{
			T result = valuePassed;

			if (!valuePassed.IsDefault)
			{
				if (this[fieldName1InJobComInvoiceHeader].Equals(valuePassed))
				{
					result = (T)valuePassed.Default;
				}
				else if (this[fieldName2InJobComInvoiceHeader].Equals(valuePassed))
				{
					result = (T)valuePassed.Default;
				}
			}

			return result;
		}

		#endregion

		#region GetEffectiveValuesFromBill

		T GetEffectiveValueFromBillToReturn<T>(T baseValue, string fieldNameInBill, string invoiceFieldName) where T : IZType
		{
			T result = baseValue;

			if (result.IsEmpty && Bill != null)
			{
				IZType effectiveValue = EffectiveValueManager.GetEffectiveValue(invoiceFieldName) ?? (IZType)Bill[fieldNameInBill];
				result = (T)effectiveValue;
			}

			return result;
		}

		T GetEffectiveValueFromBillToSet<T>(T valuePassed, string fieldNameInBill) where T : IZType
		{
			T result = valuePassed;

			if (!valuePassed.IsDefault && Bill != null && Bill[fieldNameInBill].Equals(valuePassed))
			{
				result = (T)valuePassed.Default;
			}

			return result;
		}

		#endregion

		void ResetSupTariffsForAllLines()
		{
			var invoiceLines = JobComInvoiceLines.ToArray();
			foreach (JobComInvoiceLine invoiceLine in invoiceLines)
			{
				invoiceLine.ResetSupTariffs();
			}
		}

		void SetDefaultSupTariffsForAllLines()
		{
			var invoiceLines = JobComInvoiceLines.ToArray();
			foreach (JobComInvoiceLine invoiceLine in invoiceLines)
			{
				invoiceLine.SetDefaultSupTariffs();
			}
		}

		public override ZString US_UC_NKCountryOfOrigin
		{
			get { return base.US_UC_NKCountryOfOrigin; }
			set
			{
				var oldValue = base.US_UC_NKCountryOfOrigin; // need base value instead of effective
				if (oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
					ResetSupTariffsForAllLines();
				}
				base.US_UC_NKCountryOfOrigin = value;
				if (!IsCopying)
				{
					var newValue = US_UC_NKCountryOfOrigin;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_UC_NKCountryOfOrigin);
						SetDefaultSupTariffsForAllLines();
					}
				}
			}
		}

		public override ZString US_UC_NKCountryOfExport
		{
			get
			{
				ZString result = base.US_UC_NKCountryOfExport;
				if (IsImport)
				{
					result = Bill != null ? GetEffectiveValueFromBillToReturn(result, Bill.Schema.US_UC_NKCountryOfExport, Schema.US_UC_NKCountryOfExport)
						: GetEffectiveValueToReturn(result, JobDeclaration.Schema.US_UC_NKCountryOfExport, Schema.US_UC_NKCountryOfExport);
				}
				return result;
			}
			set
			{
				var oldValue = base.US_UC_NKCountryOfExport; // need base value and not effective value
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				if (IsExport)
				{
					base.US_UC_NKCountryOfExport = value;
				}
				else
				{
					base.US_UC_NKCountryOfExport = Bill != null ? GetEffectiveValueFromBillToSet(value, Bill.Schema.US_UC_NKCountryOfExport)
						: GetEffectiveValueToSet(value, JobDeclaration.Schema.US_UC_NKCountryOfExport);
				}
				if (!IsCopying)
				{
					var newValue = US_UC_NKCountryOfExport;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_UC_NKCountryOfExport);
					}
				}
			}
		}

		public override ZString US_DestinationState
		{
			get { return GetEffectiveValueToReturn(base.US_DestinationState, JobDeclaration.Schema.US_DestinationState, Schema.US_DestinationState); }
			set
			{
				var oldValue = base.US_DestinationState; // need base value instead of effective
				base.US_DestinationState = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_DestinationState);
				if (!IsCopying)
				{
					var newValue = US_DestinationState;
					if (oldValue != newValue)
					{
						JobComInvoiceLines.MarkAsNeedingValidation();
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_DestinationState);
					}
				}
			}
		}

		public override ZString JZ_InvoiceCurrExRateType
		{
			get { return base.JZ_InvoiceCurrExRateType; }
			set
			{
				bool hasChanges = base.JZ_InvoiceCurrExRateType != value;
				base.JZ_InvoiceCurrExRateType = value;
				if (hasChanges && !IsCopying)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get { return base.JZ_RX_NKInvoice_Currency; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_RX_NKInvoice_Currency))
				{
					var oldValue = JZ_RX_NKInvoice_Currency;
					base.JZ_RX_NKInvoice_Currency = value;
					if (!IsCopying && JZ_RX_NKInvoice_Currency != oldValue)
					{
						JobComInvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid JZ_OH_Supplier
		{
			get
			{
				return useBaseJZ_OH_Supplier ? base.JZ_OH_Supplier :
					IsAttachedToPersistentExportDeclaration ? USPPIDocAddress.OrganisationPK : JZ_OA_SupplierAddress_ZAddress.OrgPK;
			}
			set
			{
				if (!settingJZ_OH_SupplierInProgress && !SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Supplier))
				{
					try
					{
						settingJZ_OH_SupplierInProgress = true;
						var currentValue = JZ_OH_Supplier;
						using (JobComInvoiceLines.SuspendUpdateProductDetailsOnSupplierBuyerChange())
						{
							try
							{
								useBaseJZ_OH_Supplier = true;
								base.JZ_OH_Supplier = value; // cause base defaulting to be fired
							}
							finally
							{
								useBaseJZ_OH_Supplier = false;
							}
							if (IsAttachedToPersistentExportDeclaration)
							{
								USPPIDocAddress.OrganisationPK = value;
							}
							else
							{
								JZ_OA_SupplierAddress_ZAddress.OrgPK = value;
							}
							if (!IsValidationSuspended)
							{
								Validation.ValidateJZ_OH_Supplier();
							}
						}

						var newValue = JZ_OH_Supplier;
						if (!IsCopying && currentValue != newValue)
						{
							US_TransactionsRelated = GetRelatedPartyIndicatorIfPossible();
							JobComInvoiceLines.UpdateProductDetailsOnSupplierBuyerChange();
							if (IsExport)
							{
								if (!newValue.IsEmpty)
								{
									SetDefaultAESOriginIndicatorIfEmpty();
									SetDefaultAESRegistrationNumberIfEmpty();
								}
							}
							else if (IsImport)
							{
								JobDeclaration?.MarkReconIndicatorsDirty();
								DefaultFirstSaleIndFromSupplierImporterLink();
							}
						}
					}
					finally
					{
						settingJZ_OH_SupplierInProgress = false;
					}
				}
			}
		}
		bool settingJZ_OH_SupplierInProgress;
		bool useBaseJZ_OH_Supplier;

		ZString GetRelatedPartyIndicatorIfPossible()
		{
			ZString related = ZString.Empty;
			OrgSupplierBuyerLink link = SupplierBuyerLink;
			if (link != null && !link.OL_RelatedParty.IsEmpty)
			{
				related = link.OL_RelatedParty;
			}
			return related;
		}

		public override ZPropertyInfo JZ_OH_SupplierInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_OH_Supplier, x => !IsDeleted && IsAttachedToPersistentExportDeclaration ? USPPIDocAddress.OrganisationPKInfo : JZ_OA_SupplierAddress_ZAddress.OrgPKInfo); }
		}

		public ZBool IsDeductADD_CVDDutyRequired
		{
			get
			{
				return JobDeclaration != null
					&& EntryTypeList.IsADD_CVDInvolved(JobDeclaration.US_EntryType)
					&& JZ_IncoTerm == Core.Constants.IncoTerms.DeliveredDutyPaid;
			}
		}

		public override ZString JZ_IncoTerm
		{
			get { return base.JZ_IncoTerm; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_IncoTerm))
				{
					var oldValue = JZ_IncoTerm;
					base.JZ_IncoTerm = value;
					if (!IsCopying && oldValue != JZ_IncoTerm)
					{
						SetDefaultDeductADDCVDDutyIfApplicable();
					}
				}
			}
		}

		internal void SetDefaultDeductADDCVDDutyIfApplicable()
		{
			if (IsDeductADD_CVDDutyRequired)
			{
				US_DeductADDCVDDuty = US_DeductADDCVDDuty.IsEmpty ? (ZString)YesNoDefaultList.Codes.No : US_DeductADDCVDDuty;
			}
			else
			{
				US_DeductADDCVDDuty = ZString.Empty;
			}
		}

		void ClearInvoiceLineValuesIfSame(IZType invoiceValue, string invoiceLineFieldName)
		{
			EffectiveValueManager.ClearValueIfSame(invoiceValue, invoiceLineFieldName, JobComInvoiceLines.Cast<JobComInvoiceLine>());
		}

		public override ZString US_AESOriginIndicator
		{
			get { return base.US_AESOriginIndicator; }
			set
			{
				var oldValue = base.US_AESOriginIndicator; // need base value instead of effective
				base.US_AESOriginIndicator = value;
				if (!IsCopying)
				{
					var newValue = US_AESOriginIndicator;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.US_AESOriginIndicator);
						if (IsExport)
						{
							JobComInvoiceLines.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public bool US_AESOriginIndicator_ReadOnly
		{
			get { return !AllowOriginIndicatorToBeEntered; }
		}

		internal bool AllowOriginIndicatorToBeEntered
		{
			get { return USCustomsDataRegistry.Instance.AllowExportDefaultOriginIndicatorAtInvoice.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty); }
		}

		public override ZString US_TermsOfDeliveryLocationIndicator
		{
			get { return base.US_TermsOfDeliveryLocationIndicator; }
			set
			{
				bool isDiff = base.US_TermsOfDeliveryLocationIndicator != value;
				base.US_TermsOfDeliveryLocationIndicator = value;
				if (!IsCopying && isDiff)
				{
					UpdateTermsOfDeliveryLocation();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.RegionDistrictPorts))]
		public ZString US_TermsOfDeliveryLocationScheduleD
		{
			get { return US_TermsOfDeliveryLocation; }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(US_TermsOfDeliveryLocationScheduleDInfo, value);
				US_TermsOfDeliveryLocation = value;
			}
		}

		protected int US_TermsOfDeliveryLocationScheduleD_MaxLength
		{
			get { return 4; }
		}

		public ZPropertyInfo US_TermsOfDeliveryLocationScheduleDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TermsOfDeliveryLocationScheduleD, x => US_TermsOfDeliveryLocationInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.ForeignPorts))]
		public ZString US_TermsOfDeliveryLocationScheduleK
		{
			get { return US_TermsOfDeliveryLocation; }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(US_TermsOfDeliveryLocationScheduleKInfo, value);
				US_TermsOfDeliveryLocation = value;
			}
		}

		protected int US_TermsOfDeliveryLocationScheduleK_MaxLength
		{
			get { return 5; }
		}

		public ZPropertyInfo US_TermsOfDeliveryLocationScheduleKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TermsOfDeliveryLocationScheduleK, x => US_TermsOfDeliveryLocationInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_TermsOfDeliveryLocationList))]
		public ZString US_TermsOfDeliveryLocationCountry
		{
			get { return US_TermsOfDeliveryLocation; }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(US_TermsOfDeliveryLocationCountryInfo, value);
				US_TermsOfDeliveryLocation = value;
			}
		}

		protected int US_TermsOfDeliveryLocationCountry_MaxLength
		{
			get { return (US_IsISOCountryCodeTermsOfDeliveryLocation) ? 2 : USAddInfoSchema.US_TermsOfDeliveryLocation.MaxLength; }
		}

		public ZPropertyInfo US_TermsOfDeliveryLocationCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TermsOfDeliveryLocationCountry, x => US_TermsOfDeliveryLocationInfo); }
		}

		/// <summary>
		/// This field is really the Importer and not the buyer; the actual buyer field is BuyerOrgPK
		/// </summary>
		public override ZGuid JZ_OH_Buyer
		{
			get
			{
				if (useBaseJZ_OH_Buyer)
				{
					return base.JZ_OH_Buyer;
				}
				else if (IsAttachedToPersistentExportDeclaration)
				{
					return UltimateConsigneeDocAddress.OrganisationPK;
				}
				else
				{
					return JobDeclaration is JobDeclaration declaration && declaration.IsSoldEnRoute ?
					base.JZ_OH_Buyer :
					GetEffectiveValueToReturn(base.JZ_OH_Buyer, JobDeclaration.Schema.JE_OH_Importer, Schema.JZ_OH_Buyer);
				}
			}
			set
			{
				if (!settingJZ_OH_BuyerInProgress && !SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Buyer))
				{
					try
					{
						settingJZ_OH_BuyerInProgress = true;
						var declaration = JobDeclaration;
						var isExportAndIsAttachedToPersistentDeclaration = declaration != null && declaration.IsPersistent && declaration.IsExport;

						ZGuid oldValue = base.JZ_OH_Buyer;

						if (!IsCopying && oldValue != value)
						{
							PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
						}

						using (JobComInvoiceLines.SuspendUpdateProductDetailsOnSupplierBuyerChange())
						{
							if (!isExportAndIsAttachedToPersistentDeclaration && !(declaration?.IsSoldEnRoute ?? false))
							{
								value = GetEffectiveValueToSet(value, JobDeclaration.Schema.JE_OH_Importer);
							}
							try
							{
								useBaseJZ_OH_Buyer = true;
								base.JZ_OH_Buyer = value;
							}
							finally
							{
								useBaseJZ_OH_Buyer = false;
							}

							if (isExportAndIsAttachedToPersistentDeclaration)
							{
								UltimateConsigneeDocAddress.OrganisationPK = value;
							}
							else if (!oldValue.IsEmpty && oldValue == ConsigneeAddressOrgPK)
							{
								JZ_OA_ConsigneeAddress = Importer != null ? Importer.MainAddress.PK : ZGuid.Empty;
							}
							if (!IsValidationSuspended)
							{
								Validation.ValidateJZ_OH_Buyer();
							}
						}

						if (!IsCopying && oldValue != JZ_OH_Buyer)
						{
							US_UltimateDestinationCountry = Importer?.CountryCode ?? ZString.Empty;

							US_TransactionsRelated = GetRelatedPartyIndicatorIfPossible();

							if (IsImport)
							{
								declaration?.MarkReconIndicatorsDirty();
								DefaultFirstSaleIndFromSupplierImporterLink();
							}
							else if (IsExport)
							{
								DefaultUltimateConsigneeType();
							}

							JobComInvoiceLines.UpdateProductDetailsOnSupplierBuyerChange();
						}
					}
					finally
					{
						settingJZ_OH_BuyerInProgress = false;
					}
				}
			}
		}
		bool settingJZ_OH_BuyerInProgress;
		bool useBaseJZ_OH_Buyer;

		void DefaultUltimateConsigneeType()
		{
			if (SupplierBuyerLink != null)
			{
				var addInfo = SupplierBuyerLink.GetAddInfo();
				var ultConsigneeType = addInfo != null ? addInfo.ZO_AESUltConsigneeType : ZString.Empty;
				if (!ultConsigneeType.IsEmpty)
				{
					US_UltimateConsigneeType = ultConsigneeType;
				}
			}
			if (Importer == null)
			{
				US_UltimateConsigneeType = ZString.Empty;
			}
		}

		void DefaultFirstSaleIndFromSupplierImporterLink()
		{
			var addInfo = SupplierBuyerLink != null ? SupplierBuyerLink.GetAddInfo() : null;
			US_FirstSale = addInfo != null ? addInfo.ZO_FirstSale : ZString.Empty;
		}

		public override ZString JZ_MessageStatus
		{
			get { return base.JZ_MessageStatus; }
			set
			{
				ZString oldStatus = JZ_MessageStatus;
				base.JZ_MessageStatus = value;
				if (oldStatus != JZ_MessageStatus)
				{
					ImportMessageStatusList statusList = AddInfoLookups.MessageStatusList;
					LogManager.AddAClearLogIfNecessary(oldStatus, JZ_MessageStatus, statusList);
					var declaration = JobDeclaration;
					if (declaration != null)
					{
						declaration.MarkENSStatusElectronicInvoicingActionCompletedIfNecessary();
						declaration.MarkAsNeedingValidation();
					}
				}
			}
		}

		void SyncImporterFromDeclaration(ZGuid importer)
		{
			if (JZ_OH_Buyer.IsEmpty)
			{
				JZ_OH_Buyer = importer;
			}
		}

		void SyncSupplierFromDeclaration(ZGuid supplierAddress)
		{
			if (JZ_OA_SupplierAddress.IsEmpty)
			{
				JZ_OA_SupplierAddress = supplierAddress;
			}
		}

		protected override ZString GetBillNumberFromReferenceNumber(ZString referenceNumber)
		{
			return GetBillNumberAndIssuerCodeFromReferenceNumber(referenceNumber).BillNumber;
		}

		(ZString BillNumber, ZString IssuerCode) GetBillNumberAndIssuerCodeFromReferenceNumber(ZString referenceNumber)
		{
			referenceNumber = referenceNumber.KeepValidBillNumberCharacters();

			var issuerCode = ZString.Empty;
			var billNumber = referenceNumber;

			if (JobDeclaration is JobDeclaration declaration && declaration.IsImport)
			{
				if (declaration.IncludeSCACInBillNum && referenceNumber.Length > 4)
				{
					var scacCode = referenceNumber.Left(4);
					if (new IssuerCarrierSCACValidator(Factory).IsValidSCACCode(scacCode))
					{
						issuerCode = scacCode;
						billNumber = referenceNumber.SubstringSafe(4);
					}
				}
			}

			return (billNumber, issuerCode);
		}

		protected override void UpdateBillNumberFromReferenceNumber(Customs.Business.Bill bill, ZString referenceNumber)
		{
			var usBill = (Bill)bill;
			var billWithIssuerCode = GetBillNumberAndIssuerCodeFromReferenceNumber(referenceNumber);
			if (!billWithIssuerCode.IssuerCode.IsEmpty)
			{
				usBill.US_UI_NKBillIssuerSCAC = billWithIssuerCode.IssuerCode;
			}

			usBill.CU_BillNum = billWithIssuerCode.BillNumber;
		}

		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				bool hasChanges = base.JZ_JE != value;

				var importer = JZ_OH_Buyer;
				var supplierAddress = JZ_OA_SupplierAddress;

				var existingInvoiceIsBeingAttachedToDeclaration = RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired;

				base.JZ_JE = value;

				var declaration = JobDeclaration;
				if (hasChanges)
				{
					if (declaration != null)
					{
						declaration.RefreshHas9802Tariff();
						if (IsInDatabase && !declaration.CanHavePGAFDA)
						{
							declaration.UpdateFDAMsgStatus(InvoiceLines.Cast<JobComInvoiceLine>().Count(x => x.IsFDADeclared));
						}
					}
				}

				if (declaration != null)
				{
					if (existingInvoiceIsBeingAttachedToDeclaration)
					{
						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							invoiceLine.MapValuesBetweenACEAndACS();
						}
						RefreshInvoiceLinesWithPGAIndicators();
						RefreshInvoiceLinesWithSpecificColumnsChanged();
					}

					if (value == ZGuid.Empty)
					{
						SyncImporterFromDeclaration(importer);
						SyncSupplierFromDeclaration(supplierAddress);
					}
				}
				if (JZ_JE.IsValid && !IsDataChangeSuspendedByFakeDeclaration)
				{
					//base sets JZ_OH_Supplier if empty. US getter returns a declaration one's and this logic wont run if declaration has a valid supplier
					if (declaration != null)
					{
						if ((declaration.IsSupplierPickupAddressInitialised || declaration.IsInDatabase) && base.JZ_OA_SupplierAddress.IsEmpty && !JZ_OA_SupplierAddress.IsEmpty)
						{
							if (Supplier != null)
							{
								DefaultIncoTermAndCurrencyFromSupplier();
								DefaultCountryOfOriginFromOrganisationDetail(SupplierAddress);
								if (IsExport)
								{
									US_USPPI.ReSynchronize(JZ_OH_SupplierInfo, USPPIDocAddress.E2_OA_AddressInfo);
									US_ExportUltimateConsignee.ReSynchronize(JZ_OH_BuyerInfo, UltimateConsigneeDocAddress.E2_OA_AddressInfo);
									US_IntermediateConsignee.ReSynchronize(JZ_OH_ConsigneeInfo, IntermediateConsigneeDocAddress.E2_OA_AddressInfo);
								}
							}
						}

						if (IsExport && !base.US_TariffType.IsEmpty && base.US_TariffType == declaration.US_TariffType)
						{
							US_TariffType = declaration.US_TariffType;
						}

						if (base.JZ_OA_ManufacturerAddress.IsEmpty)
						{
							DefaultCountryOfOriginFromOrganisationDetail(ManufacturerAddress);
						}
					}
				}
			}
		}

		protected override ZGuid GetInvoiceLineParentID(BaseJobComInvoiceLine sortedInvoiceLine)
		{
			var usInvoiceLine = (JobComInvoiceLine)sortedInvoiceLine;
			var result = usInvoiceLine.JI_ParentID;
			if (result.IsEmpty || result != usInvoiceLine.PK)
			{
				result = usInvoiceLine.US_JI_ParentProduct;
				if (result == usInvoiceLine.PK)
				{
					result = ZGuid.Empty;
				}
			}
			return result;
		}

		protected override List<BaseJobComInvoiceLine> GetChildInvoiceLines(BaseJobComInvoiceLine invoiceLine)
		{
			var usInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var result = new List<BaseJobComInvoiceLine>();
			result.AddRange(usInvoiceLine.ChildLines);
			result.AddRange(usInvoiceLine.ProductRelatedLines);
			return result;
		}

		protected override void UpdateWhenAnInvoiceIsDetached(BaseJobComInvoiceLine invoiceLine, bool isImport, bool isAdvanceShippingNotice)
		{
			var usInvoiceLine = (JobComInvoiceLine)invoiceLine;
			usInvoiceLine.PGADataCorrections.ForEach(x => x.UnRegisterTrackerIfNeeded());
			base.UpdateWhenAnInvoiceIsDetached(invoiceLine, isImport, isAdvanceShippingNotice);
			usInvoiceLine.RefreshWHSPackLines();
			if (isImport)
			{
				usInvoiceLine.LineGroupingRanges.MarkAsNeedingValidation();
			}
			usInvoiceLine.DetachPivotsForFDAOrPGA();
		}

		protected override void UpdateWhenAnInvoiceIsLinkedToADeclaration(BaseJobComInvoiceLine invoiceLine, LinkedToDeclarationData data, bool updatePartSyncManagerAndRefresh)
		{
			var usInvoiceLine = invoiceLine as JobComInvoiceLine;
			var usLinkedToDeclarationData = data as USLinkedToDeclarationData;
			base.UpdateWhenAnInvoiceIsLinkedToADeclaration(invoiceLine, data, updatePartSyncManagerAndRefresh && usInvoiceLine.JI_PartNo_CanBeSetByCustomer);

			if (IsAttachedToPersistentDeclaration)
			{
				usInvoiceLine.RefreshWHSPackLines();
				if (usLinkedToDeclarationData.IsImport)
				{
					usInvoiceLine.LineGroupingRanges.MarkAsNeedingValidation();
				}
				usInvoiceLine.FDAs.OfType<FDA>().ForEach(x => x.BillsForFDALine.RemoveAndDeleteAll());
				usInvoiceLine.UpdateOGAPGADetailsWhenCertificationModeChanges(usLinkedToDeclarationData);
				var declaration = usInvoiceLine.Declaration ?? JobDeclaration; // WI00688214
				declaration.InvoiceLines.AddRange(usInvoiceLine.SecondaryTariffLines);
				declaration.InvoiceLines.AddRange(usInvoiceLine.ProductRelatedLines);
			}
		}

		protected override OrgAddress GetEffectiveConsigeeAddress()
		{
			OrgAddress result = null;
			if (IsImport)
			{
				result = ConsigneeAddress;
			}
			else if (IsExport)
			{
				result = UltimateConsigneeDocAddress.RealAddress;
			}
			else
			{
				result = base.GetEffectiveConsigeeAddress();
			}

			return result;
		}

		protected override OrgAddress GetEffectiveSupplierAddress()
		{
			OrgAddress result = null;
			if (IsImport)
			{
				result = SupplierAddress;
			}
			else if (IsExport)
			{
				result = USPPIDocAddress.RealAddress;
			}
			else
			{
				result = base.GetEffectiveSupplierAddress();
			}

			return result;
		}

		public override ZString JZ_InvoiceNumber
		{
			get
			{
				ReconOriginalEntryHeader currentEntry = ReconOriginalEntry;
				return currentEntry == null ? base.JZ_InvoiceNumber : currentEntry.CH_OrigEntryReference;
			}
			set
			{
				bool isDiff = base.JZ_InvoiceNumber != value;
				base.JZ_InvoiceNumber = value;
				if (isDiff)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public bool CanSendOriginal
		{
			get { return !HasAIIBeenLodgedAtCustoms; }
		}

		public bool CanSendWithdrawal
		{
			get { return HasAIIBeenLodgedAtCustoms; }
		}

		public bool HasBeenWithdrawn
		{
			get { return LogManager.HasAWithdrawnLog; }
		}

		public bool HasTransactionsWithCustoms
		{
			get { return IsAIIWaitingForResponse || HasAIIBeenLodgedAtCustoms || HasBeenWithdrawn; }
		}

		public bool IsAIIWaitingForResponse
		{
			get { return AddInfoLookups.MessageStatusList.IsWaitingForResponse(JZ_MessageStatus); }
		}

		public bool HasAIIBeenLodgedAtCustoms
		{
			get { return LogManager.HasAClearLog(ImportMessageStatusList.MessageType.ElectronicInvoice, AddInfoLookups.MessageStatusList); }
		}

		internal StatusLogManager LogManager
		{
			get { return logManager ?? (logManager = new StatusLogManager(Logs, Branch)); }
		}
		StatusLogManager logManager;

		public override ZDecimal JZ_Calc_TNI
		{
			get
			{
				if (jZ_Calc_TNICached == null)
				{
					jZ_Calc_TNICached = new CachedProperty<ZDecimal>(Factory, delegate
					{
						Money result = Money.Empty;

						if (Invoice_Currency != null)
						{
							result = new OverseasFreightAndInsuranceCalculator().GetCharge(this, new string[] { Core.Constants.Customs.CustomsCharges.Codes.OverseasFreight, Core.Constants.Customs.CustomsCharges.Codes.OverseasInsurance });

							if (CurrencyConverter != null)
							{
								result = CurrencyConverter.ConvertExact(result, Invoice_Currency);
							}
						}

						return result.Amount;
					});
				}
				return jZ_Calc_TNICached.Value;
			}
		}
		CachedProperty<ZDecimal> jZ_Calc_TNICached;

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

		[RelatedBusinessObject(nameof(DesignatedExamSite))]
		public override ZString US_DES
		{
			get { return GetEffectiveValueToReturn(base.US_DES, JobDeclaration.Schema.US_DES, Schema.US_DES); }
			set { base.US_DES = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_DES); }
		}

		public ZZRefCusCodeListCombined DesignatedExamSite
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_DES, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		public override ZString AdditionalInformation
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				ZString licenseIsNotRequired = "NLR";
				bool invoiceIsITARCompliance = false;
				var declaration = JobDeclaration;
				if (declaration != null && IsDCSRequired(declaration))
				{
					result.Append(USCustomsDataRegistry.Instance.DestinationControlStatement.Value);
					ZStringBuilder eccns = new ZStringBuilder();
					ZStringBuilder licenseNos = new ZStringBuilder();
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						if (invoiceLine.IsDCSRequired)
						{
							if (!invoiceLine.US_ECCN.IsEmpty && invoiceLine.PrintECCN)
							{
								eccns.Append(invoiceLine.US_ECCN);
							}
							if (invoiceLine.IsITARCompliance)
							{
								invoiceIsITARCompliance = true;
								if (!invoiceLine.US_LicenseNo.IsEmpty && invoiceLine.US_LicenseNo != licenseIsNotRequired)
								{
									licenseNos.Append(invoiceLine.US_LicenseNo);
								}
								else if (invoiceLine.US_LicenseNo == licenseIsNotRequired && !invoiceLine.US_DDTCITARExemptionNo.IsEmpty)
								{
									licenseNos.Append(invoiceLine.US_DDTCITARExemptionNo);
								}
							}
						}
					}
					if (!eccns.IsEmpty)
					{
						result.Append("ECCNs: " + eccns.ToStringWithDelimiterBetweenAppends(", "));
					}
					if (invoiceIsITARCompliance)
					{
						if (!declaration.US_RN_NKCountryOfDestination.IsEmpty)
						{
							result.Append("Country of Destination: " + declaration.US_RN_NKCountryOfDestination);
						}
						if (US_ExportUltimateConsignee != null && US_ExportUltimateConsignee.Organisation != null)
						{
							result.Append("End-User: " + US_ExportUltimateConsignee.Organisation.OH_FullName);
						}
						if (!licenseNos.IsEmpty)
						{
							result.Append("License/Approval/Exemption: " + licenseNos.ToStringWithDelimiterBetweenAppends(", "));
						}
					}
				}
				return result.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		ZBool IsDCSRequired(JobDeclaration declaration) => declaration.IsExport && InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.IsDCSRequired);

		#endregion

		#region Related Objects

		public IOrganisationDetails ManufacturerDetails
		{
			get
			{
				IOrganisationDetails result = OrganisationDetails.New(JZ_OA_ManufacturerAddressInfo, OrgMatchedCustomsRegNoType.MID);
				if (result == null && IsAttachedToPersistentDeclaration)
				{
					result = JobDeclaration.ManufacturerDetails;
				}
				return result;
			}
		}

		public ReconOriginalEntryHeader ReconOriginalEntry
		{
			get
			{
				if (reconOriginalEntry == null || US_CH_ReconEntry != reconOriginalEntry.PK || reconOriginalEntry.IsDeleted)
				{
					var entry = Factory.Load<CusEntryHeader>(US_CH_ReconEntry);
					reconOriginalEntry = entry?.ReconOriginalEntry;
				}
				return reconOriginalEntry;
			}
		}
		ReconOriginalEntryHeader reconOriginalEntry;

		public Bill FTZBill
		{
			get
			{
				var relatedBill = Bill;
				return relatedBill?.FTZBill;
			}
		}

		public override bool SupportsRelatedBill
		{
			get
			{
				var declaration = JobDeclaration;
				return declaration != null && (declaration.IsFTZAdmission || ((IInvoicesProvider)declaration).ShouldElectronicInvoicesBeVisible);
			}
		}

		#endregion

		#region Methods Overridden

		public override void Delete()
		{
			if (IsAttachedToPersistentDeclaration)
			{
				if (Messages.Count > 0)
				{
					Messages.DiscardAll(JobDeclaration);
				}

				JobDeclaration.PGAFlags.RefreshInvoiceLinesWithPGAIndicators();
				JobDeclaration.PGAFlags.RefreshInvoiceLinesWithSpecificColumnsChanged();

				if (JobDeclaration.IsENSFormalImport)
				{
					MessageAttacheesAddedOrDeletedEvent.InvokeMessageAttacheeAddedOrDeletedService(Factory, this, MessageAttacheeActionType.Deleted);
				}
				if (JobDeclaration != null && JobComInvoiceLines.Count > 0 && JobComInvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.Part != null))
				{
					JobDeclaration.MarkReconIndicatorsDirty();
				}
			}
			ResetConsolidatedEntryJobNoForReleaseEntry();
			base.Delete();

			if (ReconOriginalEntry != null)
			{
				ReconOriginalEntry.UpdateChargesReadOnlyState();
			}
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_MessageStatus);
			return result;
		}

		protected override ZBool IsJZ_InvoiceCurrExRateUserEnterableCore
		{
			get { return JZ_InvoiceCurrExRateType == FixedExchangeRateTypeString; }
			set { JZ_InvoiceCurrExRateType = value ? FixedExchangeRateTypeString : ""; }
		}
		public const string FixedExchangeRateTypeString = "FIX";

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		internal class Strategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
		{
			public Strategy(JobComInvoiceHeader invoiceHeader)
				: base(invoiceHeader)
			{
			}

			protected new JobComInvoiceHeader BusinessObject
			{
				get { return (JobComInvoiceHeader)base.BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();

				if (!BusinessObject.IsExport)
				{
					Factory.AddFetchHint(typeof(Enterprise.Messaging.Business.EDIMessage), EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
				}
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(OrgAddressSchema.OA_OH, BusinessObject.JZ_OA_SupplierAddress_ZAddress.OrgPK);
				Factory.AddFetchHint(typeof(OrgAddress), BusinessObject.JZ_OA_ManufacturerAddress);
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(typeof(USOrganisationDocAddress), JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(typeof(OrgHeader), BusinessObject.JZ_OH_Buyer);
			}
		}

		protected override void UpdatePartSyncManagerAndRefresh(BaseJobComInvoiceLine invoiceLine, bool enabledStatus)
		{
			if (((JobComInvoiceLine)invoiceLine).JI_PartNo_CanBeSetByCustomer)
			{
				base.UpdatePartSyncManagerAndRefresh(invoiceLine, enabledStatus);
			}
		}

		protected override JobComInvoiceHeaderDeepCopyStrategy GetTemplateCopyStrategy(CloneType cloneType)
		{
			return new USJobComInvoiceHeaderDeepCloneStrategy(this, cloneType);
		}

		#endregion

		#region New Methods

		public void CopyValueFromForExport(JobComInvoiceHeader source)
		{
			((IBusinessObjectInternals)this).IsCopying = true;
			try
			{
				US_USPPI.CopyValueFrom(source.US_USPPI);
				US_ExportUltimateConsignee.CopyValueFrom(source.US_ExportUltimateConsignee);
				US_IntermediateConsignee.CopyValueFrom(source.US_IntermediateConsignee);
				US_StateOfOrigin = source.US_StateOfOrigin;
				US_ForeignTradeZone = source.US_ForeignTradeZone;
				US_ECCN = source.US_ECCN;
				US_RoutedTransaction = source.US_RoutedTransaction;
				US_TransactionsRelated = source.US_TransactionsRelated;
				US_LicenseType = source.US_LicenseType;
				US_LicenseNo = source.US_LicenseNo;
				US_InbondType = source.US_InbondType;
				US_ImportEntryNo = source.US_ImportEntryNo;
				US_HazardousCargo = source.US_HazardousCargo;
				US_ExportCode = source.US_ExportCode;
				US_TermsOfDeliveryLocationQualifier = source.US_TermsOfDeliveryLocationQualifier;
				US_TermsOfDeliveryLocationIndicator = source.US_TermsOfDeliveryLocationIndicator;
				US_TermsOfDeliveryLocation = source.US_TermsOfDeliveryLocation;
				US_UltimateConsigneeType = source.US_UltimateConsigneeType;

				if (AllowOriginIndicatorToBeEntered)
				{
					US_AESOriginIndicator = source.US_AESOriginIndicator;
				}
			}
			finally
			{
				((IBusinessObjectInternals)this).IsCopying = false;
			}
		}

		#endregion

		#region Collections

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public RelatedDocumentCollection RelatedDocuments
		{
			get
			{
				if (fRelatedDocuments == null)
				{
					fRelatedDocuments = new RelatedDocumentCollection(this);
					fRelatedDocuments.Load();
					RegisterEditableChildObject(fRelatedDocuments);
				}
				return fRelatedDocuments;
			}
		}
		RelatedDocumentCollection fRelatedDocuments;

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					messages.Load();
					messages.IsManagedForDataRefresh = true;
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		#endregion

		#region Implementation

		ZGuid GetDeclarationSupplierAddressPK()
		{
			ZGuid result;
			var declaration = JobDeclaration;
			var supplierPickupAddress = declaration.SupplierPickupAddress;
			if (declaration.IsExport &&
				(declaration.IsSupplierPickupAddressInitialised || declaration.IsInDatabase) &&
				declaration.JE_OH_Supplier == supplierPickupAddress.OrganisationPK &&
				supplierPickupAddress.Address != null &&
				!supplierPickupAddress.E2_AddressOverride)
			{
				result = supplierPickupAddress.E2_OA_Address;
			}
			else
			{
				result = (declaration.Supplier != null) ? declaration.Supplier.MainAddress.PK : ZGuid.Empty;
			}
			return result;
		}

		protected override void SetSupplierFromDeclaration()
		{
			// we don't need to do this as the property is an effective from declaration already.
		}

		OrgContact GetDefaultUSPPIContact(OrgHeader organisation)
		{
			OrgContact result = null;
			if (IsAttachedToPersistentDeclaration)
			{
				var supplierPickupAddress = JobDeclaration.SupplierPickupAddress;
				if (supplierPickupAddress.HasRealAddress && supplierPickupAddress.OrganisationPK == organisation.PK)
				{
					result = supplierPickupAddress.Contact;
				}
			}
			return result;
		}

		internal void EnsureUSOrganisationAreLoadedIfNeeded()
		{
			if (IsExport)
			{
				fUS_USPPI = null;
				fUS_ExportUltimateConsignee = null;
				fUS_IntermediateConsignee = null;

				_ = US_USPPI;
				_ = US_ExportUltimateConsignee;
				_ = US_IntermediateConsignee;
			}
		}

		public override void OnSaving()
		{
			SaveJZ_OH_SupplierForReportingPurpose();
			FillInInvoiceNumberIfRequired();

			if (shouldUpdateConsolidatedJobNo)
			{
				SetConsolidatedJobNoForReleaseEntry(JobDeclaration.JE_DeclarationReference, JobDeclaration.US_EntryFilerCode + JobDeclaration.DecEntryNumber);
				shouldUpdateConsolidatedJobNo = false;
			}
			base.OnSaving();
		}

		void SaveJZ_OH_SupplierForReportingPurpose()
		{
			OrgAddress address = null;
			if (IsAttachedToPersistentExportDeclaration)
			{
				address = USPPIDocAddress.Address;
			}
			else
			{
				address = Factory.Load<OrgAddress>(JZ_OA_SupplierAddress);
			}

			if (address != null && (!IsInDatabase || USPPIDocAddress.E2_OA_AddressInfo.HasChanges || JZ_OA_SupplierAddressInfo.HasChanges))
			{
				((IBusinessObjectInternals)this).Row[Schema.JZ_OH_Supplier] = address.OA_OH.ToGuid();
			}
		}

		void FillInInvoiceNumberIfRequired()
		{
			if (JZ_InvoiceNumber.IsEmpty && IsAttachedToPersistentDeclaration && (IsExport || JobDeclaration.IsDrawback))
			{
				JobDeclaration.PopulateJE_DeclarationReferenceIfNeeded();
				JZ_InvoiceNumber = JobDeclaration.JE_DeclarationReference;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			US_InvoiceType = InvoiceTypeList.Codes.CommercialInvoice;
			SetDefaultDeductADDCVDDutyIfApplicable();
			//Doing this inside a constructor always leads to JZ_MessageType having an error. Then when JZ_JE is set an invoice is added to a collection, the validation is suspended and system loses the chance to clear the error. If we do this here, the validation is suspended and JZ_MessageType does not have a premature error.
			EnsureUSOrganisationAreLoadedIfNeeded();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			EnsureUSOrganisationAreLoadedIfNeeded();
		}
		protected override void SetDefaultInvoiceDate()
		{
			//do not set this.
		}

		void UpdateTermsOfDeliveryLocation()
		{
			if (IsImport)
			{
				ZString location = GetDefaultTermsOfDeliveryLocation();

				if (!US_IsOtherTermsOfDeliveryLocation)
				{
					if (US_IsISOCountryCodeTermsOfDeliveryLocation)
					{
						location = location.Left(2);
					}
					else
					{
						Schedule schedule = (US_IsScheduleDTermsOfDeliveryLocation) ? Schedule.D : Schedule.K;
						location = GetScheduleCode(location, schedule);
					}
				}
				if (!location.IsEmpty)
				{
					US_TermsOfDeliveryLocation = location;
				}
			}
		}

		ZString GetDefaultTermsOfDeliveryLocation()
		{
			return IsAttachedToPersistentDeclaration ? GetDefaultTermsOfDeliveryLocationCore() : ZString.Empty;
		}

		ZString GetDefaultTermsOfDeliveryLocationCore()
		{
			switch (JZ_IncoTerm)
			{
				case TermsOfDeliveryList.Codes.EXW:
					return JobDeclaration.JE_RL_NKOrigin;
				case TermsOfDeliveryList.Codes.CAI:
				case TermsOfDeliveryList.Codes.CAF:
				case TermsOfDeliveryList.Codes.CIF:
					return JobDeclaration.JE_RL_NKPortOfArrival;
				case TermsOfDeliveryList.Codes.EXQ:
				case TermsOfDeliveryList.Codes.FAS:
				case TermsOfDeliveryList.Codes.FOA:
				case TermsOfDeliveryList.Codes.FOB:
				case TermsOfDeliveryList.Codes.FOR:
				case TermsOfDeliveryList.Codes.FOT:
					return JobDeclaration.JE_RL_NKPortOfLoading;
				default:
					return US_TermsOfDeliveryLocation;
			}
		}

		ZString GetScheduleCode(ZString unLoco, Schedule schedule)
		{
			ZString transportMode = (!IsAttachedToPersistentDeclaration) ? ZString.Empty : JobDeclaration.JE_TransportMode;
			return USScheduleResolver.GetScheduleCode(schedule, unLoco, transportMode, Factory);
		}

		protected void SetDefaultAESRegistrationNumberIfEmpty()
		{
			new RegistrationNumberDefaulter().DefaultDDTCRegistrationNumber(US_DDTCRegistrationNoInfo, US_LicenseType, US_USPPI);
		}

		protected void SetDefaultAESOriginIndicatorIfEmpty()
		{
			if (US_AESOriginIndicator.IsEmpty && AllowOriginIndicatorToBeEntered)
			{
				OrgHeader supplier = Supplier;
				if (supplier != null && supplier.CountryCode != Core.Constants.CountryCodes.UnitedStates)
				{
					US_AESOriginIndicator = AESOriginIndicatorList.Codes.Foreign;
				}
				else
				{
					US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;
				}
			}
		}

		#endregion

		#region Properties for Testing Only
#if DEBUG

		public bool IsTestingBusinessObjectTest;

		public override ZString IncotermEquivalentToCFRForTesting
		{
			get
			{
				if (IsExport)
				{
					return base.IncotermEquivalentToCFRForTesting;
				}
				else
				{
					return Core.Constants.USCustoms.DeliveryTerms.Codes.CAF;
				}
			}
		}
#endif
		#endregion

		#region IMessageAttacheeInDeclaration Members

		ZString IMessageAttacheeWithCBPSenderReference.EntryFilerCode
		{
			get { return JobDeclaration?.US_EntryFilerCode ?? ZString.Empty; }
		}

		ZString IMessageAttacheeInDeclaration.EntryNumber
		{
			get { return JobDeclaration?.ImportEntryNumber ?? ZString.Empty; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingDistrictPort
		{
			get { return JobDeclaration.IsRemoteLocationFiling ? JobDeclaration.US_PreparerDistrictPort : JobDeclaration.ProcessingDistrictPort; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingOfficeCode
		{
			get { return JobDeclaration.ProcessingOfficeCode; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return JobDeclaration.TransportMode; }
		}

		bool IMessageAttacheeInDeclaration.IsActive
		{
			get { return true; }
		}

		MessageAttacheeRecordType IMessageAttacheeInDeclaration.RecordType
		{
			get { return MessageAttacheeRecordType.ElectronicInvoice; }
		}

		ZString IMessageAttacheeInDeclaration.RecordTypeDescription
		{
			get { return MessageAttacheeRecordTypeDescriptions.ElectronicInvoice; }
		}

		ZString IMessageAttacheeInDeclaration.MessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(JZ_MessageStatus); }
		}

		ZString IMessageAttacheeInDeclaration.HumanFriendlyReference
		{
			get { return JZ_InvoiceNumber; }
		}

		ZDateTime IMessageAttacheeInDeclaration.ReleaseDate
		{
			get { return ZDateTime.Empty; }
		}

		ZDateTime IMessageAttacheeInDeclaration.TIBExpiryDate
		{
			get { return ZDateTime.Empty; }
		}

		ZInt IMessageAttacheeInDeclaration.TIBNumOfExtensions
		{
			get { return ZInt.Zero; }
		}

		ZString IMessageAttacheeInDeclaration.EntryStatus
		{
			get { return ZString.Empty; }
		}

		IEnumerable<INotification> IMessageAttacheeInDeclaration.GetBusinessLayerNotificationsToAddToWrapper()
		{
			yield break;
		}

		ValidationModes IMessageAttacheeInDeclaration.ValidationModes
		{
			get { return IsAttachedToPersistentDeclaration ? JobDeclaration.ValidationModes : ValidationModes.None; }
			set
			{
				if (IsAttachedToPersistentDeclaration)
				{
					JobDeclaration.ValidationModes = value;
				}
			}
		}

		ZString IMessageAttacheeInDeclaration.JobReferenceNumber
		{
			get { return IsAttachedToPersistentDeclaration ? JobDeclaration.JE_DeclarationReference : ZString.Empty; }
		}

		ZGuid IMessageAttacheeInDeclaration.DeclarationPK
		{
			get { return JZ_JE; }
		}

		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages
		{
			get { return new ZGuid[] { PK }; }
		}

		#endregion

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return JobDeclaration; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider?.ControllerID;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.BusinessObjectPK : Guid.Empty;
			}
		}

		#endregion

		#region IMessageAttachee Members

		ZString IMessageAttachee.MessageStatus
		{
			get { return JZ_MessageStatus; }
			set { JZ_MessageStatus = value; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return Factory; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return JobDeclaration; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return JobDeclaration?.JE_DeclarationReference ?? ZString.Empty; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return JobDeclaration?.Logs; }
		}
		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return RegistryCompanyPK; }
		}

		#endregion

		#region IDocAddresses Members

		protected override DocAddressType[] SupportedAddressTypesCore()
		{
			return new DocAddressType[]
				{
					DocAddressType.UltimateConsignee,
					DocAddressType.IntermediateConsignee,
					DocAddressType.USPrincipalPartyInInterest,
					DocAddressType.SupplierPickupDeliveryAddress,
					DocAddressType.InvoicerAddress,
					DocAddressType.FDAShipperAddress
				};
		}

		#endregion

		public ZString AIIStatusCustomsNarrative
		{
			get
			{
				if (fAIIStatusCustomsNarrative.IsEmpty)
				{
					MQEDIMessage message = (MQEDIMessage)Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse, EDIMessage.Direction.Receive);
					if (message != null)
					{
						var statusBlocks = message.MessageBlock.MessageBlocks.OfType<AIIE00>();
						fAIIStatusCustomsNarrative = statusBlocks.Select(block => block.MessageIdentifierCode + " - " + block.NarrativeMessage)
																 .LastOrDefault();
					}
				}

				return fAIIStatusCustomsNarrative;
			}
		}
		ZString fAIIStatusCustomsNarrative;

		public ZString AIIStatus
		{
			get { return JZ_MessageStatus; }
		}

		public ZString AIIStatusDescription
		{
			get { return Lookups.AIIStatus.GetDescriptionFromCode(AIIStatus); }
		}

		public AIIERecordCollection AIIERecords
		{
			get
			{
				if (aiieRecords == null)
				{
					aiieRecords = new AIIERecordCollection(Factory);

					MQEDIMessage message = (MQEDIMessage)Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse, EDIMessage.Direction.Receive);
					if (message != null)
					{
						aiieRecords.AddRange(from block in message.MessageBlock.MessageBlocks.OfType<AIIE0195>()
											 select new AIIERecord()
											 {
												 InvoicingPartyID = block.InvoicingPartyIDCode,
												 InvoiceNumber = block.InvoiceNumber,
												 InvoiceLine = block.InvoiceLine.ToString(),
												 ErrorMessageIdentifier = block.ErrorMessageIdentifier,
												 ErrorMessage = block.ErrorMessage,
											 });
					}
				}

				return aiieRecords;
			}
		}
		AIIERecordCollection aiieRecords;

		#region IMessageResponseNotificator Members

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			IMessageResponseNotificator notificator = JobDeclaration;
			return notificator != null ? notificator.GetFallbackEmailAddressRecipient() : ZString.Empty;
		}

		#endregion

		#region IShouldUpdateScreeningStatus Members

		ZBool IShouldUpdateScreeningStatus.ShouldUpdateScreeningStatus
		{
			get
			{
				return fShouldUpdateScreeningStatus;
			}

			set
			{
				fShouldUpdateScreeningStatus = value;
				if (fShouldUpdateScreeningStatus)
				{
					OnShouldUpdateScreeningStatusChangedToTrue(EventArgs.Empty);
				}
			}
		}

		ZBool fShouldUpdateScreeningStatus;

		#endregion

		public event EventHandler ShouldUpdateScreeningStatusChangedToTrue;

		public void OnShouldUpdateScreeningStatusChangedToTrue(EventArgs e)
		{
			if (ShouldUpdateScreeningStatusChangedToTrue != null)
			{
				ShouldUpdateScreeningStatusChangedToTrue(this, e);
			}
		}

		public EffectiveValueManager EffectiveValueManager => effectiveValueManager ?? (effectiveValueManager = new EffectiveValueManager());
		EffectiveValueManager effectiveValueManager;

		#region ICurrencyConverterDataProvider

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get
			{
				if (JobDeclaration != null && JobDeclaration.IsExport)
				{
					return Customs.Business.BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack;
				}
				else
				{
					return 0;
				}
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.RelatedDocument, typeof(RelatedDocument));
			return result;
		}

		#endregion

		#region IJobComInvoiceHeader

		ZGuid Integration.Customs.US.IJobComInvoiceHeader.BuyerOrgPK
		{
			get { return BuyerOrgPK; }
		}

		#endregion

		#region ICanDetach

		public override bool CanDetach
		{
			get { return !IsAIIWaitingForResponse; }
		}

		public override string ReasonNotToBeAbleToDetach
		{
			get { return ReasonNotToBeAbleToDeleteOrDetach("detach"); }
		}

		public override string GetWarningBeforeBeingDetached()
		{
			return GetWarningBeforeBeingDeletedOrDetached("detach");
		}

		public void CleanUpInvoiceAfterDetachedOrDeleted(string deletedOrDetached)
		{
			US_IsLineGrouping = false;

			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(invoiceLine =>
			{
				invoiceLine.LineGroupingRanges.DeleteAll();
				invoiceLine.AIILines.DeleteAll();
			});

			Messages.Cast<EDIMessage>().ForEach(message =>
			{
				message.EM_LinkedObject = JobDeclaration;
				message.EM_Status = EDIMessageStatusList.Codes.Discarded;
			});

			JobDeclaration.Messages.Load();
			Messages.Load();
			JobDeclaration.InBondRelatedRecords.ReBuild(MessagesToShowCollection.MessagesStatus.ActiveOnly);
			JobDeclaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Where(entry => entry.US7501DocPrintingData.Count > 0).ForEach(entry => entry.US7501DocPrintingData.DeleteAll());
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			JobDeclaration.Logs.AddNew(Events.EditedARecord, string.Format("Invoice {0} - Invoice Number '{1}'", deletedOrDetached, JZ_InvoiceNumber));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		MultilingualString ReasonNotToBeAbleToDeleteOrDetach(string deleteOrDetach)
		{
			return (NoResString)string.Format("Electronic invoice message(s) exist that are pending response(s) from Customs. Please wait for the response(s) from Customs before {0} the invoice(s).", deleteOrDetach);
		}

		MultilingualString GetWarningBeforeBeingDeletedOrDetached(string deleteOrDetach)
		{
			var resultBuilder = new ZStringBuilder();
			var jobDeclaration = JobDeclaration;

			if (HasTransactionsWithCustoms)
			{
				resultBuilder.AppendLine(string.Format("You are about to {0} Invoices that have been sent to Customs electronically.", deleteOrDetach));
			}
			if (jobDeclaration != null && !jobDeclaration.US_ManEntry && jobDeclaration.ActiveEntryHeaders.EntrySummaryEntry != null && jobDeclaration.ActiveEntryHeaders.EntrySummaryEntry.HasBeenLodgedAtCustoms)
			{
				resultBuilder.AppendLine("Printing of 7501 Entry Summary may not be based on messages until another ENS message is accepted.");
			}

			return (NoResString)(resultBuilder.ToString());
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return !IsAIIWaitingForResponse; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ReasonNotToBeAbleToDeleteOrDetach("delete"); }
		}

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			return GetWarningBeforeBeingDeletedOrDetached("delete");
		}

		#endregion

		#region IInvoiceHeader Members
		IDeclaration IInvoiceHeader.Declaration => JobDeclaration;
		#endregion

		#region IPGADataChangeTrackerSupporter Members
		IPGADataChangeTrackerSupporter PGADataChangeTrackerSupporter => this;
		PGADataChangeTracker IPGADataChangeTrackerSupporter.Tracker => JobDeclaration?.PGATrackerHelper;
		#endregion

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			return IsImport ? JobDeclaration.USIMPIncoTermAndCharge : base.GetStandaloneIncoTermAndChargeFactoryCountryContext();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var fullName = ZString.Empty;
				if (Importer != null && !Importer.OH_FullName.IsEmpty)
				{
					fullName = string.Format(" - {0}", Importer.OH_FullName);
				}

				return Res.GetString("9FB9CD54-7B92-4C98-AFA8-97760A8659C6", "Invoice - {0}{1}", JZ_InvoiceNumber, fullName);
			}
		}

		bool IsCreatedFromUSLowValue => JobDeclaration != null && JobDeclaration.IsCreatedFromUSLowValue;
	}
}
