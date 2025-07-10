using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business
{
	[SystemDefinedValues]
	[CodeProperty(AsycudaManifestHeader.Schema.AMA_JobReference)]
	[UniversalDataContext(DataContextType.ZAOutTurn)]
	public class AsycudaManifestHeader : ApplicationSpecificAsycudaManifestHeader
		, Integration.Customs.ZA.IAsycudaManifestHeader
		, IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider
		, IDocManagerSupport
		, IInterchangeSenderIdProvider
		, IPurgeValueParent
		, IGateInOutStatusProvider
		, IJobNumber
		, IWorkflowProvider
		, IWorkflowProviderCore
		, IWorkflowTriggerEventSource
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : AutoAsycudaManifestHeader.Schema
		{
			public const string AMA_MasterBill = "AMA_MasterBill";
			public const string AMA_IssueDate = "AMA_IssueDate";
			public const string ParentBill = "ParentBill";
			public const string OutturnProvider = "OutturnProvider";
			public const string BookingNumber = "BookingNumber";
			public const string GateInOutDate = "GateInOutDate";
			public const string GateInOutMessageType = "GateInOutMessageType";
			public const string GateInOutCustomsStatus = "GateInOutCustomsStatus";
			public const string FullyLoadedUnloadedDate = "FullyLoadedUnloadedDate";
			public const string UnpackedDate = "UnpackedDate";
			public const string ExcessIndicator = "ExcessIndicator";
			public const string RegistrationNumber = "RegistrationNumber";
			public const string RegistrationDate = "RegistrationDate";
			public const string RegistrationStatus = "RegistrationStatus";

			public const int AMA_MasterBillLength = 35;
			public const int ParentBillMaxLength = 35;
			public const int OutturnProviderMaxLength = 2;
			public const int GateInOutMessageTypeMaxLength = 3;
			public const int GateInOutCustomsStatusMaxLength = 3;
			public const int ExcessIndicatorMaxLength = 1;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_RN_NKCountry = Constants.CountryCodes.SouthAfrica;
		}

		protected override ZString GetApplicationCode() => ApplicationCode_Out;

		public override void OnSaving()
		{
			base.OnSaving();

			PopulateNumberPropertyIfRequired(AMA_JobReferenceInfo, x => GetNewJobReference(x));
		}

		ZString GetNewJobReference(BusinessObjectFactory factory)
		{
			var target = new ZAOutturnGateInOutJobNumberGeneratorTarget();
			var generator = new NumberGenerator();
			generator.Factory = factory;
			generator.Context = new NumberGeneratorContext();
			generator.BaseFountain = Env.NumberFountains.ZAOutturnGateInOutJobReference;
			generator.FountainGetter = Env.NumberFountains.GetZAOutturnGateInOutJobReferenceGeneratorFountain;
			generator.PrimaryTarget = target;
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.Generate();
			generator.EnforceMaxLengths();
			return target.Value.ToUpper();
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = "Outturn & Gate In/Out";

				var jobReference = AMA_JobReference;
				if (!jobReference.IsEmpty)
				{
					result = jobReference;
				}

				return result;
			}
		}

		public bool IsMessagingActive => Messages.Count > 0;

		public ZString CarrierCode => Carrier?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsOfficeList))]
		public override ZString AMA_CustomsOffice
		{
			get => base.AMA_CustomsOffice;
			set => base.AMA_CustomsOffice = value;
		}

		public ZString CustomsOfficeDescription => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, AMA_CustomsOffice, Constants.CountryCodes.SouthAfrica, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now)?.ZZD_Description ?? ZString.Empty;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.ManifestTypeList))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.AMA_ManifestType", Caption = "COSTCO Type")]
		public override ZString AMA_ManifestType
		{
			get => base.AMA_ManifestType;
			set
			{
				var oldValue = AMA_ManifestType;
				base.AMA_ManifestType = value;
				if (oldValue != value)
				{
					ClearRelatedPropertiesIfEmpty(AMA_ManifestTypeInfo);
					if (!IsCopying)
					{
						Containers.MarkAsNeedingValidation();
						MasterBill.MarkAsNeedingValidation();
					}
				}
			}
		}

		public ZBool IsDOR => AMA_ManifestType == ManifestTypeList.Codes.DepotOutturnReport;

		public ZBool IsBBB => AMA_ManifestType == ManifestTypeList.Codes.BulkBreakBulkOutturnReport;

		public ZBool IsVOR => AMA_ManifestType == ManifestTypeList.Codes.VesselOutturnReport;

		public ZBool IsAOR => AMA_ManifestType == ManifestTypeList.Codes.AirCargoOutturnReport;

		public ZBool IsEOR => AMA_ManifestType == ManifestTypeList.Codes.AirExcessOutturnReport;

		public ZBool IsALD => AMA_ManifestType == ManifestTypeList.Codes.AirLoadDischarge;

		public ZBool IsCOSTCO => IsDOR || IsBBB || IsVOR || IsAOR || IsEOR || IsALD;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.Natures))]
		public override ZString AMA_Nature
		{
			get => base.AMA_Nature;
			set
			{
				var oldValue = AMA_Nature;
				base.AMA_Nature = value;
				if (oldValue != AMA_Nature)
				{
					if (!IsExport)
					{
						foreach (var bill in Bills.Cast<AsycudaBill>())
						{
							ClearLRNIfNeeded(bill);
							ClearCustomsCPCIfNeeded(bill);
						}
					}

					if (!IsCopying)
					{
						Bills.MarkAsNeedingValidation();
						MasterBill.MarkAsNeedingValidation();
					}
				}
			}
		}

		static void ClearLRNIfNeeded(AsycudaBill bill)
		{
			if (!bill.LRN.IsEmpty)
			{
				bill.LRN = ZString.Empty;
			}
		}

		static void ClearCustomsCPCIfNeeded(AsycudaBill bill)
		{
			if (!bill.CustomsCPC.IsEmpty)
			{
				bill.CustomsCPC = ZString.Empty;
			}
		}

		public ZBool IsExport => AMA_Nature == NatureList.Codes.Export22;

		public ZBool IsImport => AMA_Nature == NatureList.Codes.Import23;

		public ZBool IsTranshipment => AMA_Nature == NatureList.Codes.Transhipment28;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TransportModeList))]
		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				var oldValue = AMA_TransportMode;
				base.AMA_TransportMode = value;
				if (oldValue != AMA_TransportMode)
				{
					if (!IsCopying)
					{
						MasterBill.MarkAsNeedingValidation();
						Containers.MarkAsNeedingValidation();
					}
					PurgeValues();
					ClearInvalidGateInOutMessageTypeIfNeeded();
				}
			}
		}

		void ClearInvalidGateInOutMessageTypeIfNeeded()
		{
			if (!Lookups.GateInOutMessageTypeList.ContainsCode(GateInOutMessageType))
			{
				GateInOutMessageType = ZString.Empty;
			}
		}

		void ClearRelatedPropertiesIfEmpty(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty)
			{
				return;
			}

			if (info == AMA_ManifestTypeInfo)
			{
				FullyLoadedUnloadedDate = ZDateTime.Empty;
				UnpackedDate = ZDateTime.Empty;
				ExcessIndicator = ZString.Empty;
			}
			else if (info == GateInOutMessageTypeInfo)
			{
				GateInOutDate = ZDateTime.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.ContainerModeList))]
		public override ZString AMA_ContainerMode
		{
			get => base.AMA_ContainerMode;
			set
			{
				var oldValue = AMA_ContainerMode;
				base.AMA_ContainerMode = value;
				if (oldValue != AMA_ContainerMode && !IsCopying)
				{
					Containers.MarkAsNeedingValidation();
					FullyLoadedUnloadedDateInfo.RefreshBinding();
				}
			}
		}

		public ZBool AMA_ContainerModeVisible => IsSea;

		public ZBool IsBLK => AMA_ContainerMode == Constants.ContainerModes.Bulk;

		public ZBool IsBBK => AMA_ContainerMode == Constants.ContainerModes.BreakBulk;

		public ZBool IsCNT => AMA_ContainerMode == Constants.ContainerModes.Containerised;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.AgentTypeList))]
		public override ZString AMA_AgentType
		{
			get => base.AMA_AgentType;
			set => base.AMA_AgentType = value;
		}

		[LightValidationTestExempt]
		public override ZInt AMA_ClusterKey
		{
			get => base.AMA_ClusterKey;
			set => base.AMA_ClusterKey = value;
		}

		protected override void ClearVesselValuesWhenVesselWasNotMatched(bool setAMA_VesselName, bool setAMA_LloydsNumber, bool setAMA_RadioCallSign, bool setAMA_RN_NKConveyanceNationality)
		{
			if (!setAMA_VesselName && setAMA_LloydsNumber && setAMA_RadioCallSign && setAMA_RN_NKConveyanceNationality)
			{
				AMA_LloydsNumber = ZString.Empty;
				AMA_RadioCallSign = ZString.Empty;
				AMA_RN_NKConveyanceNationality = ZString.Empty;
			}
		}

		#region AMA_IssueDate

		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.AMA_IssueDate", Caption = "Issue Date")]
		public ZDate AMA_IssueDate
		{
			get => MasterBill.ABL_BillIssueDate;
			set
			{
				var oldValue = AMA_IssueDate;
				var masterbill = MasterBill;
				masterbill.ABL_BillIssueDate = value;
				AMA_IssueDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_IssueDateInfo => GetWrappedZPropertyInfo(Schema.AMA_IssueDate, x => MasterBill.ABL_BillIssueDateInfo);

		#endregion

		public string VoyageFlightNoLabel => IsAir
			? Res.GetString("268e93fa-ac58-48e1-91a7-b068dfc1679a", "Flight")
			: IsSea
				? Res.GetString("335c1292-2c45-4246-ab69-56ea4cbab18f", "Voyage")
				: Res.GetString("d8c48b26-7407-4841-b404-76577e01063d", "Flight/Voyage");

		public ZBool IsAir => AMA_TransportMode == Constants.TransportModes.Air;

		public ZBool IsSea => AMA_TransportMode == Constants.TransportModes.Sea;

		public ZBool IsRoad => AMA_TransportMode == Constants.TransportModes.Road;

		public ZBool AMA_VoyageVisible => !IsRoad;

		#region OutturnProvider

		[MaxLength(Schema.OutturnProviderMaxLength)]
		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.OutturnProvider", Caption = "Message Sender/Outturn Provider")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.OutturnProviderList))]
		public ZString OutturnProvider
		{
			get => MasterBill.ABL_GoodsLocation;
			set
			{
				var oldValue = OutturnProvider;
				CheckMaximumLength(OutturnProviderInfo, value);
				MasterBill.ABL_GoodsLocation = value;
				OutturnProviderInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOutturnProvider();
				}
			}
		}

		public ZPropertyInfo OutturnProviderInfo => GetZPropertyInfo(Schema.OutturnProvider);

		ZString OutturnFacilityCode
		{
			get
			{
				var outturnProvider = OutturnProvider;
				var effectiveDate = ZDateTime.Today;
				var key = ZString.Format("OutturnFacilityCode_{0}_{1}", outturnProvider, effectiveDate);

				return Factory.GetCachedValue(key, () =>
				{
					var refCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory
						, outturnProvider
						, Constants.CountryCodes.SouthAfrica
						, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities
						, effectiveDate,
						attributeNames: new ZString[] { Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode });
					return refCode?.GetAttribute(Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode) ?? ZString.Empty;
				});
			}
		}

		#endregion

		#region GateInOutMessageType

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.GateInOutMessageType", Caption = "GOVGIO Type")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.GateInOutMessageTypeList))]
		[MaxLength(Schema.GateInOutMessageTypeMaxLength)]
		public ZString GateInOutMessageType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.GateInOutMessageType);
			set
			{
				var oldValue = GateInOutMessageType;
				var hasChanges = oldValue != value;
				if (hasChanges)
				{
					CheckMaximumLength(GateInOutMessageTypeInfo, value);
					this.SetSystemDefinedValue(Schema.GateInOutMessageType, value);
					GateInOutMessageTypeInfo.RefreshBinding(oldValue);
					ClearRelatedPropertiesIfEmpty(GateInOutMessageTypeInfo);
					if (!IsValidationSuspended)
					{
						Validation.ValidateGateInOutMessageType();
					}
				}
			}
		}

		public ZBool IsTGO => GateInOutMessageType == GateInOutMessageTypeCodeList.Codes.TerminalGateOut;

		public ZBool IsTGI => GateInOutMessageType == GateInOutMessageTypeCodeList.Codes.TerminalGateIn;

		public ZBool IsDGI => GateInOutMessageType == GateInOutMessageTypeCodeList.Codes.DepotGateIn;

		public ZBool IsDGO => GateInOutMessageType == GateInOutMessageTypeCodeList.Codes.DepotGateOut;

		public ZBool IsDCI => GateInOutMessageType == GateInOutMessageTypeCodeList.Codes.SeaDepotConsignmentGateIn;

		public ZBool IsATI => GateInOutMessageType == GateInOutMessageTypeCodeList.Codes.AirTerminalGateIn;

		public ZBool IsADI => GateInOutMessageType == GateInOutMessageTypeCodeList.Codes.AirDepotGateIn;

		public ZBool IsBGI => GateInOutMessageType == GateInOutMessageTypeCodeList.Codes.BreakBulkGateIn;

		public ZBool IsGOVGIO => IsTGO || IsTGI || IsDGI || IsDGO || IsDCI || IsATI || IsADI || IsBGI;

		public ZPropertyInfo GateInOutMessageTypeInfo => GetZPropertyInfo(nameof(GateInOutMessageType));

		#endregion

		#region GateInOutDate

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.GateInOutDate", Caption = "Gate In/Out Date Time")]
		[PurgeValue(nameof(UseGateInOutDatePerContainer))]
		public ZDateTime GateInOutDate
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.GateInOutDate);
			set
			{
				var oldValue = GateInOutDate;
				var hasChanges = oldValue != value;
				if (hasChanges)
				{
					this.SetSystemDefinedValue(Schema.GateInOutDate, value);
					GateInOutDateInfo.RefreshBinding(oldValue);
					if (!IsValidationSuspended)
					{
						Validation.ValidateGateInOutDate();
					}
				}
			}
		}

		public ZPropertyInfo GateInOutDateInfo => GetZPropertyInfo(nameof(GateInOutDate));

		public bool UseGateInOutDatePerContainer => AMA_TransportMode == Constants.TransportModes.Sea;

		public ZBool UseGateInOutDatePerManifest => !UseGateInOutDatePerContainer;

		public new AsycudaManifestHeaderFetchStrategy FetchStrategy => (AsycudaManifestHeaderFetchStrategy)base.FetchStrategy;

		#endregion

		#region GateInOutCustomsStatus

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsStatusList))]
		[MaxLength(Schema.GateInOutCustomsStatusMaxLength)]
		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.GateInOutCustomsStatus", Caption = "Gate In/Out Status")]
		public ZString GateInOutCustomsStatus
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.GateInOutCustomsStatus);
			set
			{
				var oldValue = GateInOutCustomsStatus;
				CheckMaximumLength(GateInOutCustomsStatusInfo, value);
				if (oldValue != value && !value.IsEmpty)
				{
					Logs.AddNew(GateInOrOutEvent, value, ZDateTimeOffset.Now, false);
				}
				this.SetSystemDefinedValue(Schema.GateInOutCustomsStatus, value);
				GateInOutCustomsStatusInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo GateInOutCustomsStatusInfo => GetZPropertyInfo(Schema.GateInOutCustomsStatus);

		#endregion

		#region BookingNumber

		[MaxLength(AsycudaBill.Schema.ABL_CarrierReferenceMaxLength)]
		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.BookingNumber", Caption = "Booking Number")]
		public ZString BookingNumber
		{
			get => MasterBill.ABL_CarrierReference;
			set
			{
				var oldValue = BookingNumber;
				CheckMaximumLength(BookingNumberInfo, value);
				MasterBill.ABL_CarrierReference = value;
				BookingNumberInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBookingNumber();
				}
			}
		}

		public ZPropertyInfo BookingNumberInfo => GetZPropertyInfo(Schema.BookingNumber);

		#endregion

		#region AMA_MasterBill

		[MaxLength(AsycudaBill.Schema.ABL_BillNumberMaxLength)]
		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.AMA_MasterBill", Caption = "Master Bill")]
		public ZString AMA_MasterBill
		{
			get => MasterBill.ABL_BillNumber;
			set
			{
				var oldValue = AMA_MasterBill;
				CheckMaximumLength(AMA_MasterBillInfo, value);
				MasterBill.ABL_BillNumber = value;
				AMA_MasterBillInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAMA_MasterBill();
				}
			}
		}

		public ZPropertyInfo AMA_MasterBillInfo => GetZPropertyInfo(Schema.AMA_MasterBill);

		#endregion

		#region ParentBill

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.ParentBill", Caption = "Parent Bill")]
		[MaxLength(Schema.ParentBillMaxLength)]
		public ZString ParentBill
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ParentBill);
			set
			{
				var oldValue = ParentBill;
				CheckMaximumLength(ParentBillInfo, value);
				this.SetSystemDefinedValue(Schema.ParentBill, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateParentBill();
				}
				ParentBillInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ParentBillInfo => GetZPropertyInfo(Schema.ParentBill);

		#endregion

		#region AMA_OA_DeconsolidateAddress

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.OrganizationsFindBoxList))]
		public override ZGuid AMA_OA_DeconsolidateAddress
		{
			get => base.AMA_OA_DeconsolidateAddress;
			set => base.AMA_OA_DeconsolidateAddress = value;
		}

		#endregion

		#region AMA_OA_DischargeTerminalAddress

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.OrganizationsFindBoxList))]
		public override ZGuid AMA_OA_DischargeTerminalAddress
		{
			get => base.AMA_OA_DischargeTerminalAddress;
			set => base.AMA_OA_DischargeTerminalAddress = value;
		}

		#endregion

		#region FullyLoadedUnloadedDate

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.FullyLoadedUnloadedDate", Caption = "Date Time Fully Loaded/Unloaded")]
		public ZDateTime FullyLoadedUnloadedDate
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.FullyLoadedUnloadedDate);
			set
			{
				var oldValue = FullyLoadedUnloadedDate;
				this.SetSystemDefinedValue(Schema.FullyLoadedUnloadedDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFullyLoadedUnloadedDate();
				}
				FullyLoadedUnloadedDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo FullyLoadedUnloadedDateInfo => GetZPropertyInfo(nameof(Schema.FullyLoadedUnloadedDate));

		public ZBool FullyLoadedUnloadedDateVisible
		{
			get => IsBBB || IsVOR && !IsCNT || IsEOR || IsALD || IsAOR;
		}

		#endregion

		#region UnpackedDate

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.UnpackedDate", Caption = "Date Time Unpacked")]
		public ZDateTime UnpackedDate
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.UnpackedDate);
			set
			{
				var oldValue = UnpackedDate;
				this.SetSystemDefinedValue(Schema.UnpackedDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUnpackedDate();
				}
				UnpackedDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo UnpackedDateInfo => GetZPropertyInfo(Schema.UnpackedDate);

		#endregion

		#region ExcessIndicator

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaManifestHeader.ExcessIndicator", Caption = "Excess/Short Indicator")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.ExcessIndicatorList))]
		[MaxLength(Schema.ExcessIndicatorMaxLength)]
		public ZString ExcessIndicator
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ExcessIndicator);
			set
			{
				var oldValue = ExcessIndicator;
				CheckMaximumLength(ExcessIndicatorInfo, value);
				this.SetSystemDefinedValue(Schema.ExcessIndicator, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateExcessIndicator();
				}
				ExcessIndicatorInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ExcessIndicatorInfo => GetZPropertyInfo(Schema.ExcessIndicator);

		public ZBool ExcessIndicatorVisible => IsVOR || IsEOR;

		#endregion

		#region Registration

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ReadOnlyMember(nameof(RegistrationDetails_ReadOnly))]
		[ResourceStringData("ZA.AsycudaManifestHeader.RegistrationNumber", Caption = "Registration Number", ShortCaption = "Reg. Number")]
		public ZString RegistrationNumber
		{
			get => RegistrationEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var cusEntryNumber = RegistrationEntryNumber;
				var oldValue = cusEntryNumber?.CE_EntryNum ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						// use cached variable for performance
						registrationEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.SouthAfrica);
						if (!registrationEntryNumber.IsInDatabase)
						{
							registrationEntryNumber.CE_EntryStatus = Common.Shared.AsycudaRegistrationStatuses.Codes.Registered;
							RegistrationStatusInfo.RefreshBinding(ZString.Empty);
						}
						RegisterEditableChildObject(registrationEntryNumber);
					}
					registrationEntryNumber.CE_EntryNum = value;
					if (registrationEntryNumber.CE_IssueDate.IsEmpty)
					{
						registrationEntryNumber.CE_IssueDate = ZDateTime.Now;
						RegistrationDateInfo.RefreshBinding(ZDateTime.Empty);
					}
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_EntryNum = value;
					}
				}
				RegistrationNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo RegistrationNumberInfo
		{
			get { return GetZPropertyInfo(Schema.RegistrationNumber); }
		}

		bool RegistrationDetails_ReadOnly => true;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.RegistrationStatusList))]
		[MaxLength(CusEntryNumber.Schema.CE_EntryStatusMaxLength)]
		[ReadOnlyMember(nameof(RegistrationDetails_ReadOnly))]
		public ZString RegistrationStatus
		{
			get => RegistrationEntryNumber?.CE_EntryStatus ?? ZString.Empty;
			set
			{
				var cusEntryNumber = RegistrationEntryNumber;
				var oldValue = cusEntryNumber?.CE_EntryStatus ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						// use cached variable for performance
						registrationEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.SouthAfrica);
						RegisterEditableChildObject(registrationEntryNumber);
					}
					registrationEntryNumber.CE_EntryStatus = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_EntryStatus = value;
					}
				}
				RegistrationStatusInfo.RefreshBinding(oldValue);
				if (oldValue != value && !value.IsEmpty)
				{
					Logs.AddNew(GateInOrOutEvent, value, ZDateTimeOffset.Now, false);
				}
			}
		}

		Event GateInOrOutEvent => IsImport ? ZArchitecture.Business.Events.GateIn
																	: IsExport ? ZArchitecture.Business.Events.GateOut
																				: ZArchitecture.Business.Events.StatusChange;
		public ZPropertyInfo RegistrationStatusInfo
		{
			get { return GetZPropertyInfo(Schema.RegistrationStatus); }
		}

		[ReadOnlyMember(nameof(RegistrationDetails_ReadOnly))]
		[ResourceStringData("ZA.AsycudaManifestHeader.RegistrationDate", Caption = "Registration Date", ShortCaption = "Date")]
		public ZDateTime RegistrationDate
		{
			get => RegistrationEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				var cusEntryNumber = RegistrationEntryNumber;
				var oldValue = cusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						// use cached variable for performance
						registrationEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.SouthAfrica);
						if (!registrationEntryNumber.IsInDatabase)
						{
							registrationEntryNumber.CE_EntryStatus = Common.Shared.AsycudaRegistrationStatuses.Codes.Registered;
						}
						RegisterEditableChildObject(registrationEntryNumber);
					}

					registrationEntryNumber.CE_IssueDate = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_IssueDate = value;
					}
				}

				RegistrationDateInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRegistrationDate();
				}
			}
		}
		public ZPropertyInfo RegistrationDateInfo
		{
			get { return GetZPropertyInfo(Schema.RegistrationDate); }
		}

		[ReadOnly(true)]
		internal CusEntryNumber RegistrationEntryNumber
		{
			get
			{
				if (registrationEntryNumber == null || registrationEntryNumber.IsDeleted || registrationEntryNumber.CE_EntryType != CusEntryNumberTypes.ASYCUDA.AsycudaRegistration)
				{
					registrationEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.SouthAfrica);
					if (registrationEntryNumber != null)
					{
						RegisterEditableChildObject(registrationEntryNumber);
					}
				}
				return registrationEntryNumber;
			}
		}
		CusEntryNumber registrationEntryNumber;

		#endregion

		public ZString TerminalBerth
		{
			get => DischargeTerminalAddress?.Header?.CustomsCodes.GetCustomsRegNo(
					OrgCusCode.CodeTypes.TerminalControlledPremisesID,
					Constants.CountryCodes.SouthAfrica)
					?? ZString.Empty;
		}

		protected override ZAddress GetNewAMA_OA_Carrier_ZAddress()
		{
			var zAddress = base.GetNewAMA_OA_Carrier_ZAddress();
			zAddress.DefaultAddressType = AddressType.OFC;
			return zAddress;
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				Bills.RemoveAndDeleteAll();
				Messages.RemoveAndDeleteAll();
				MasterBill.Delete();
				masterBill = null;
				((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
				LoadMasterBills().DeleteAll();
				this.DeleteChildren<CusEntryNumber>(CusEntryNumSchema.CE_ParentID);
			}
			base.Delete();
		}

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;
		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);

		public AsycudaBill MasterBill
		{
			get
			{
				if (!IsDeleted && (masterBill == null || masterBill.IsDeleted))
				{
					masterBill = LoadMasterBills().OrderBy(x => x.ABL_SystemCreateTimeUtc).FirstOrDefault();
					if (masterBill == null)
					{
						masterBill = (AsycudaBill)Factory.New(GetBillType());
						masterBill.ABL_BolType = AsycudaBill.ChildBolCode;
						masterBill.ABL_AMA = PK;
						masterBill.ABL_ClusterKey = AMA_ClusterKey;
					}
					RegisterEditableChildObject(masterBill);
				}
				return masterBill;
			}
		}
		AsycudaBill masterBill;

		AsycudaBill[] LoadMasterBills()
		{
			var q = new ZQuery(AsycudaBillSchema.ABL_AMA, PK);
			q.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			q.FetchOnlyFromLocalCache = !IsInDatabase;
			return Factory.Load<AsycudaBill>(q);
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				if (!IsDeleted)
				{
					result.AddRange(Bills);
					Bills.Cast<AsycudaBill>().ForEach(bill => result.AddRange(bill.Packs));
				}

				return result.ToArray();
			}
		}

		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);

		[ChildEditable(true)]
		public Messaging.Business.EDIMessageCollection Messages
		{
			get
			{
				if (ediMessages == null)
				{
					ediMessages = new Messaging.Business.EDIMessageCollection(this);
					ediMessages.Load();

					ediMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(ediMessages);
				}
				return ediMessages;
			}
		}

		Messaging.Business.EDIMessageCollection ediMessages;

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);
		public override AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.None;
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		public const string ApplicationCode_Out = ApplicationCodeTypeList.Codes.ZAOutturnAndGateInOrOut;

		protected override ZString GetDefaultCountryCode() => Constants.CountryCodes.SouthAfrica;

		public string JobNumber => AMA_JobReference;

		#region IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider Members

		EDIFACTMessageStatusCalculator IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider.GetCalculator(string country)
		{
			return new MessageManagers.EDIFACTStatusCalculator("CUSCAR");
		}

		void IEDIFACTMessageAttachee.AddMessage(EDIMessage message)
		{
			message.EM_LinkedObject = this;

			if (ediMessages != null)
			{
				UnRegisterEditableChildObject(ediMessages);
				ediMessages = null;
			}
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get => AMA_MessageStatus;
			set => AMA_MessageStatus = value;
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get => RegistrationStatus;
			set => RegistrationStatus = value;
		}

		ZString IEDIFACTMessageAttachee.JobIdentification => AMA_MasterBill;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => this;

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ?? (docManagerInfo = new AsycudaManifestHeaderDocManagerInfo(this));
		DocManagerInfo docManagerInfo;

		#endregion

		#region IInterchangeSenderIdProvider

		ZString IInterchangeSenderIdProvider.SenderID
		{
			get
			{
				var outturnFacilityCode = OutturnFacilityCode;
				return !outturnFacilityCode.IsEmpty ? outturnFacilityCode : OutturnProvider;
			}
		}

		#endregion

		#region IPurgeValueParent Members

		bool IPurgeValueParent.IsPurging { get; set; }

		IPurgeValueHelper IPurgeValueParent.PurgeHelper
		{
			get { return purgeHelper ?? (purgeHelper = new PurgeValueHelper<AsycudaManifestHeader>(this)); }
		}

		IPurgeValueHelper purgeHelper;

		void PurgeValues()
		{
			if (IsCopying)
			{
				return;
			}

			((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
			foreach (var container in Containers.Cast<AsycudaContainer>())
			{
				((IPurgeValueParent)container).PurgeHelper.PurgeAllValues();
			}
		}

		#endregion

		#region Workflow

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable]
		public ProcessTaskCollection<ZAsycudaManifestHeaderProcessTask, AsycudaManifestHeader> WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection<ZAsycudaManifestHeaderProcessTask, AsycudaManifestHeader>(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection<ZAsycudaManifestHeaderProcessTask, AsycudaManifestHeader> workflowItems;

		protected bool SupportsWorkflowCore
		{
			get { return true; }
		}

		public ZString WorkflowType
		{
			get { return WorkflowDescriptors.AsycudaManifestHeaderWorkflowDescriptorCode; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.AsycudaManifestHeaderWorkflowDescriptorCode; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return workflowInformationProvider ?? (workflowInformationProvider = new AsycudaManifestHeaderWorkflowInformationProvider(this));
		}
		IWorkflowInformationProvider workflowInformationProvider;

		public IGlbCompany JobHeaderCompany
		{
			get { return Branch.Company; }
		}
		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return Branch.Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get { return Array.Empty<IWorkflowProviderCore>(); }
		}

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => ((IWorkflowProvider)WorkflowItems).WorkflowItems;

		public bool SupportsWorkflow { get; internal set; }
		#endregion
	}
}
