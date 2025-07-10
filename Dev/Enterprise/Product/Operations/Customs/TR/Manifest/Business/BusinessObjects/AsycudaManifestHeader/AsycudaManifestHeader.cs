using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.TR;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public partial class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader
		, Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader
		, IVisitedPortParent
		, Integration.Customs.ICusSupportingInfoTypeSupporter
		, IMessageAttachee
		, IRegistrationNoEntryProvider
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new partial class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string TR_GM_PresentationCustomsOffice = "TR_GM_PresentationCustomsOffice";
			public const int TR_GM_PresentationCustomsOfficeMaxLength = 8;
			public const string TransportType = "TransportType";
			public const int TransportTypeMaxLength = 2;
			public const string ManifestInternalInspectionNo = "ManifestInternalInspectionNo";
			public const int ManifestInternalInspectionNoMaxLength = 20;
			public const string TemporaryStorageStartDate = "TemporaryStorageStartDate";
			public const string TemporaryStorageDueDate = "TemporaryStorageDueDate";
		}

		public override bool IsRoutingEnabled => false;

		#region new properties
		[ResourceStringData("TRAsycudaManifestHeader.TransportType", Caption = "Transport Type")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TransportTypeList))]
		[MaxLength(Schema.TransportTypeMaxLength)]
		[ReadOnlyMember(nameof(IsOnlyForSeaAndGrupaj))]
		public ZString TransportType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TransportType);
			set
			{
				var oldValue = TransportType;
				CheckMaximumLength(TransportTypeInfo, value);
				this.SetSystemDefinedValue(Schema.TransportType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransportType();
				}
				TransportTypeInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo TransportTypeInfo => GetZPropertyInfo(Schema.TransportType);

		[ResourceStringData("TRAsycudaManifestHeader.TR_GM_PresentationCustomsOffice", Caption = "Presentation Customs Office", ShortCaption = "Present.Cus.Off.")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TR_GM_PresentationCustomsOfficeList))]
		[MaxLength(Schema.TR_GM_PresentationCustomsOfficeMaxLength)]
		public ZString TR_GM_PresentationCustomsOffice
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TR_GM_PresentationCustomsOffice);
			set
			{
				var oldValue = TR_GM_PresentationCustomsOffice;
				CheckMaximumLength(TR_GM_PresentationCustomsOfficeInfo, value);
				this.SetSystemDefinedValue(Schema.TR_GM_PresentationCustomsOffice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTR_GM_PresentationCustomsOffice();
				}
				TR_GM_PresentationCustomsOfficeInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo TR_GM_PresentationCustomsOfficeInfo => GetZPropertyInfo(Schema.TR_GM_PresentationCustomsOffice);

		public override ZString AMA_CustomsOffice
		{
			get => GetEffectiveValueToReturn(base.AMA_CustomsOffice, TR_GM_PresentationCustomsOffice);
			set => base.AMA_CustomsOffice = value;
		}

		ZString GetEffectiveValueToReturn(ZString originalValue, ZString backupValue)
		{
			return originalValue.IsEmpty ? backupValue : originalValue;
		}

		[ResourceStringData("TRAsycudaManifestHeader.ManifestInternalInspectionNo", Caption = "Internal Inspection No.", ShortCaption = "Inspection No.")]
		[MaxLength(Schema.ManifestInternalInspectionNoMaxLength)]
		[ReadOnly(true)]
		public ZString ManifestInternalInspectionNo
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ManifestInternalInspectionNo);
			set
			{
				var oldValue = ManifestInternalInspectionNo;
				CheckMaximumLength(ManifestInternalInspectionNoInfo, value);
				this.SetSystemDefinedValue(Schema.ManifestInternalInspectionNo, value);
				ManifestInternalInspectionNoInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo ManifestInternalInspectionNoInfo => GetZPropertyInfo(Schema.ManifestInternalInspectionNo);

		[ResourceStringData("TRAsycudaManifestHeader.TemporaryStorageStartDate", Caption = "Temp. Storage Start Date", ShortCaption = "Temp. Store Start")]
		[ReadOnly(true)]
		public ZDateTime TemporaryStorageStartDate
		{
			get
			{
				return this.GetSystemDefinedValue<ZDateTime>(Schema.TemporaryStorageStartDate);
			}
			set
			{
				var oldValue = TemporaryStorageStartDate;
				this.SetSystemDefinedValue(Schema.TemporaryStorageStartDate, value);
				TemporaryStorageStartDateInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo TemporaryStorageStartDateInfo => GetZPropertyInfo(Schema.TemporaryStorageStartDate);

		[ResourceStringData("TRAsycudaManifestHeader.TemporaryStorageDueDate", Caption = "Temp. Storage Due Date", ShortCaption = "Temp. Store End")]
		[ReadOnly(true)]
		public ZDateTime TemporaryStorageDueDate
		{
			get
			{
				return this.GetSystemDefinedValue<ZDateTime>(Schema.TemporaryStorageDueDate);
			}
			set
			{
				var oldValue = TemporaryStorageDueDate;
				this.SetSystemDefinedValue(Schema.TemporaryStorageDueDate, value);
				TemporaryStorageDueDateInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo TemporaryStorageDueDateInfo => GetZPropertyInfo(Schema.TemporaryStorageDueDate);

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("TRAsycudaManifestHeader.TIRNumber", Caption = "TIR/ATA Carnet No")]
		public ZString TIRNumber
		{
			get => TIREntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var cusEntryNumber = TIREntryNumber;
				var oldValue = cusEntryNumber?.CE_EntryNum ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						tirEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Turkey.TIR, AMA_RN_NKCountry);
						RegisterEditableChildObject(tirEntryNumber);
					}
					tirEntryNumber.CE_EntryNum = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_EntryNum = value;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateTIRNumber();
				}

				TIRNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TIRNumberInfo => GetZPropertyInfo(nameof(TIRNumber));

		public ResourceStringData TIRNumberCaption => IsTIRNumberCaptionShouldGroupageBillNo ? Res.GetData("0AF575C6-CD37-4D3B-BE6D-11C15A818416", "Groupage Bill No") : Res.GetData("TRAsycudaManifestHeader.TIRNumber", "TIR/ATA Carnet No");

		ZBool IsTIRNumberCaptionShouldGroupageBillNo => AMA_ManifestType == TRManifestTypes.Codes.GRUPAJ && (IsSea || IsAir);

		internal CusEntryNumber TIREntryNumber
		{
			get
			{
				if (tirEntryNumber == null || tirEntryNumber.IsDeleted || tirEntryNumber.CE_EntryType != CusEntryNumberTypes.Turkey.TIR)
				{
					tirEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.Turkey.TIR, AMA_RN_NKCountry);
					if (tirEntryNumber != null)
					{
						RegisterEditableChildObject(tirEntryNumber);
					}
				}
				return tirEntryNumber;
			}
		}
		CusEntryNumber tirEntryNumber;
		#region Manifest To Opens

		[ChildEditable(true)]
		public ManifestToOpenCollection ManifestsToOpenList
		{
			get
			{
				if (manifestsToOpenList == null)
				{
					manifestsToOpenList = new ManifestToOpenCollection(this);
					manifestsToOpenList.Load();
					RegisterEditableChildObject(manifestsToOpenList);
				}
				return manifestsToOpenList;
			}
		}

		ManifestToOpenCollection manifestsToOpenList;

		#endregion

		public bool NeedPreviousDeclarationNumbers
		{
			get { return IsShippingLine && (AMA_ManifestType == TRManifestTypes.Codes.EMANIF || AMA_ManifestType == TRManifestTypes.Codes.VARONC); }
		}

		public ResourceStringData LloydsNumberCaption => TRManifestTypes.IsManifestTypesRelatedToAir(AMA_TransportMode, AMA_ManifestType) ? Res.GetData("3CF55AE9-054D-4EC1-9FE3-19A2373D1E07", "Reference No") : Res.GetData("ED95A5C6-367C-46B9-B2F4-3BBAA70263DA", "Vessel IMO Number");

		public ZBool IsBillRelatedDeclarationsTabVisible => IsExport && (AMA_ManifestType == TRManifestTypes.Codes.HAVIHR || AMA_ManifestType == TRManifestTypes.Codes.DENIHR || AMA_ManifestType == TRManifestTypes.Codes.CIKONC);

		#endregion

		#region override properties

		public override ZString AMA_ApplicationCode
		{
			get => base.AMA_ApplicationCode;
			set
			{
				var oldValue = AMA_ApplicationCode;
				base.AMA_ApplicationCode = value;

				if (!IsCopying && oldValue != AMA_ApplicationCode)
				{
					foreach (AsycudaBill bill in Bills)
					{
						bill.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				var oldValue = base.AMA_TransportMode;
				if (oldValue != value)
				{
					base.AMA_TransportMode = value;
					AMA_TransportModeInfo.RefreshBinding(oldValue);
					DefaultTransportType();
					ClearCustomsPorts();
					ClearPropertiesValueForSeaAndGrupaj();
					Bills.OfType<AsycudaBill>().ForEach(x => x.RefreshBillStampDutyValue());
					MasterBill.RefreshMasterBillStampDutyValue();

					if (!IsCopying)
					{
						Containers.MarkAsNeedingValidation();
					}
				}
			}
		}

		void ClearPropertiesValueForSeaAndGrupaj()
		{
			if (IsOnlyForSeaAndGrupaj)
			{
				TransportType = ZString.Empty;
				AMA_VesselName = ZString.Empty;
				AMA_RN_NKConveyanceNationality = ZString.Empty;
				AMA_RL_NKPortOfLoading = ZString.Empty;
				AMA_CustomsLoadPort = ZString.Empty;
				AMA_RL_NKPortOfFirstArrival = ZString.Empty;
			}
		}

		void ClearCustomsPorts()
		{
			if (!FeatureProvider.SupportsCustomsPorts(this))
			{
				AMA_CustomsLoadPort = ZString.Empty;
				AMA_CustomsDischargePort = ZString.Empty;
			}
		}

		public override ZString AMA_ManifestType
		{
			get => base.AMA_ManifestType;
			set
			{
				var oldValue = AMA_ManifestType;
				base.AMA_ManifestType = value;
				if (oldValue != value && !IsCopying)
				{
					if (TRManifestTypes.IsManifestTypesRelatedToSea(AMA_TransportMode, AMA_ManifestType) && !TRManifestTypes.IsNeedToDefaultDateAtCustomsOffice(AMA_TransportMode, AMA_ManifestType))
					{
						AMA_DateAtCustomsOffice = ZDateTime.Today.ToSmallDateTime();
					}

					ClearPropertiesValueForSeaAndGrupaj();
				}
			}
		}

		public override ZString AMA_Nature
		{
			get => base.AMA_Nature;
			set
			{
				var oldValue = AMA_Nature;
				if (oldValue != value)
				{
					base.AMA_Nature = value;
					Bills.OfType<AsycudaBill>().ForEach(b =>
					{
						b.ABL_ShipperRegNoInfo.RefreshBinding();
						b.ABL_ConsigneeRegNoInfo.RefreshBinding();
						b.ABL_NotifyPartyRegNoInfo.RefreshBinding();
					});
				}
			}
		}

		void DefaultTransportType()
		{
			var transportTypes = Lookups.TransportTypeList.GetAllCodes();
			TransportType = transportTypes.Length > 0 ? (ZString)transportTypes[0] : TransportType;
		}

		public ResourceStringData DateCustomsOfficeLabel
		{
			get { return AMA_Nature == ShipmentTypeList.Codes.Export22 ? Res.GetData("7A3EBD7C-7CB9-4956-9C41-E957ADBABDF3", "Departure Date") : Res.GetData("C4C37D5B-44F6-4EA9-9ABE-4476E4571F5E", "Arrival Date"); }
		}

		[ResourceStringData("373B06E8-E8D7-47B4-AE1C-A8D86153A1D7", Caption = "Manifest Description")]
		public override ZString AMA_ManifestDescription { get => base.AMA_ManifestDescription; set => base.AMA_ManifestDescription = value; }

		[ResourceStringData("DD105E66-FD53-442E-8786-2EBCF7CD0B93", Caption = "Inspection Clerk")]
		[ReadOnly(true)]
		public override ZString AMA_InspectionClerk { get => base.AMA_InspectionClerk; set => base.AMA_InspectionClerk = value; }

		#endregion

		public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

		[ResourceStringData("A48F1CDD-ADF0-4C32-94C9-3838B0B8DD31", Caption = "Global Manifest Stamp Duty", ShortCaption = "Glob.Man.Sta.Duty")]
		public ZDecimal GlobalManifestStampDutyValue
		{
			get => MasterBill?.GlobalManifestStampDutyValue ?? ZDecimal.Zero;
			set
			{
				MasterBill.GlobalManifestStampDutyValue = value;
				GlobalManifestStampDutyValueInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo GlobalManifestStampDutyValueInfo => GetZPropertyInfo(nameof(GlobalManifestStampDutyValue));

		[ResourceStringData("2A7C6CDF-B188-4CE3-A857-609B58DB54AB", Caption = "Master Bill Stamp Duty", ShortCaption = "Mas.Bill Stamp Duty")]
		public ZDecimal MasterBillStampDutyValue
		{
			get => MasterBill?.MasterBillStampDutyValue ?? ZDecimal.Zero;
			set
			{
				MasterBill.MasterBillStampDutyValue = value;
				MasterBillStampDutyValueInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo MasterBillStampDutyValueInfo => GetZPropertyInfo(nameof(MasterBillStampDutyValue));

		[ResourceStringData("22DD5CA2-DACA-4012-A818-3B3078795C23", Caption = "Total Stamp Duty", ShortCaption = "Total Stamp Duty")]
		public ZDecimal TotalStampDutyValue
		{
			get
			{
				var totalStampDutyValue = GlobalManifestStampDutyValue + MasterBillStampDutyValue;
				if (IsAir || IsSea)
				{
					totalStampDutyValue += Bills.Cast<AsycudaBill>().Sum(x => x.BillStampDutyValue + x.AirBillStampDutyABSValue);
				}
				return totalStampDutyValue;
			}
		}
		public ZPropertyInfo TotalStampDutyValueInfo => GetZPropertyInfo(nameof(TotalStampDutyValue));

		public override void OnSaving()
		{
			if (TIREntryNumber != null && TIREntryNumber.CE_EntryNum.IsEmpty)
			{
				TIREntryNumber.Delete();
			}
			if (!IsBillRelatedDeclarationsTabVisible)
			{
				Bills?.Cast<AsycudaBill>()?.ForEach(b => b.RelatedDeclarationForExports?.RemoveAndDeleteAll());
			}
			base.OnSaving();
		}

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;
		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		public new ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader> Persons => (ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader>)base.Persons;
		protected override ASYCUDA.Business.CusPersonCollection CreateNewCusPersonCollection() => new ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader>(this);
		protected override Type GetPersonTypeCore() => typeof(CusPerson);

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Turkey;

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public override ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;

		protected override ASYCUDA.Business.AsycudaManifestHeaderDocWrapper GetDocWrapperCore() => new AsycudaManifestHeaderDocWrapper(this);

		public override ZBool SupportMultipleCustomsNumbers => ZBool.True;

		public override ZBool SupportUNDGsOnPackedItemLevel => ZBool.True;

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = new Dictionary<ZString, Type>();
				result.Add(CusCodeDataTypeList.Codes.TRVisitedPort, typeof(VisitedPort));
				return result;
			}
		}

		#region IVisitedPortParent

		[ChildEditable(true)]
		public VisitedPortCollection VisitedPorts
		{
			get
			{
				if (visitedPort == null)
				{
					visitedPort = new VisitedPortCollection(this);
					visitedPort.Load();
					RegisterEditableChildObject(visitedPort);
				}
				return visitedPort;
			}
		}
		VisitedPortCollection visitedPort;

		ZBool IVisitedPortParent.SupportsCustomsPorts
		{
			get { return this.FeatureProvider?.SupportsCustomsPorts(this) ?? false; }
		}

		AsycudaManifestHeader IVisitedPortParent.ManifestHeader
		{
			get { return this; }
		}

		#endregion

		#region IAsycudaManifestHeader

		void Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader.CreateManifestStatement()
		{
			AsycudaHelper.CreateManifestStatement(this);
		}

		public void CalculateStampDuties()
		{
			var loader = new RefCusTaxOrFee.Loader(Factory);
			GlobalManifestStampDutyValue = LoadTaxValueForCode(TaxCodeList.Codes.GMS);
			if (IsAir)
			{
				MasterBillStampDutyValue = LoadTaxValueForCode(TaxCodeList.Codes.ABS);
			}
			else if (IsSea)
			{
				MasterBillStampDutyValue = LoadTaxValueForCode(TaxCodeList.Codes.SBS);
			}

			if (IsAir || IsSea)
			{
				foreach (AsycudaBill bill in Bills)
				{
					if (IsAir || !bill.Packs.Cast<AsycudaPack>().Any(x => x.HasEmptyContainer))
					{
						bill.BillStampDutyValue = LoadTaxValueForCode(TaxCodeList.Codes.OBS);
					}

					if (IsAir)
					{
						bill.AirBillStampDutyABSValue = LoadTaxValueForCode(TaxCodeList.Codes.ABS);
					}
				}
			}

			ZDecimal LoadTaxValueForCode(string taxCode)
			{
				return loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Turkey, taxCode, ApplicationBusinessProvider.GetEffectiveDateForDutyRate(this))?.ZZF_Value ?? 0m;
			}
		}

		#endregion

		#region IMessageAttachee

		IBusinessObjectCollection Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader.Messages => Messages;

		ZString IMessageAttachee.MessageStatus { get => AMA_MessageStatus; set => AMA_MessageStatus = value; }

		ZString IMessageAttachee.CustomsStatus { get => RegistrationStatus; set => RegistrationStatus = value; }

		ZGuid IMessageAttachee.GlobalBranchPK => base.AMA_GB;

		IBusinessObjectCollection IMessageAttachee.Messages => Messages;
		ZString IMessageAttachee.JobReference => AMA_JobReference;

		#endregion

		#region ICusSupportingInfoTypeSupporterMembers

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> { { Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(ManifestToOpen) } };
		}
		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}
		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
		}
#endif

		public override ASYCUDA.Business.BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new TRMessageSendingNotificationHelper(this);
		}

		[MaxLength(35)]
		[ReadOnlyMember(nameof(IsOnlyForSeaAndGrupaj))]
		public override ZString AMA_VesselName { get => base.AMA_VesselName; set => base.AMA_VesselName = value; }

		public void ChangeToAmmendManifest()
		{
			var cusEntryNumber = RegistrationEntryNumber;
			if (!cusEntryNumber.IsNull && !cusEntryNumber.CE_EntryNum.IsEmpty)
			{
				AMA_MessageStatus = ZString.Empty;

				cusEntryNumber.CE_EntryLineReference = cusEntryNumber.CE_EntryNum;
				cusEntryNumber.CE_ExpiryDate = ZDateTime.Now;
				cusEntryNumber.CE_EntryNum = ZString.Empty;
				cusEntryNumber.CE_IssueDate = ZDateTime.Empty;
				cusEntryNumber.CE_EntryStatus = AsycudaRegistrationStatuses.Codes.Amendment;
				this.Logs.AddNew(AutoEvents.CustomsManifestStatus, AsycudaRegistrationStatuses.Codes.Amendment);
			}
		}

		public override ZString AMA_JobReference
		{
			get
			{
				var result = base.AMA_JobReference;
				if (JobSequenceNumber > 0)
				{
					result = $"{result}-{JobSequenceNumber}";
				}
				return result;
			}
			set => base.AMA_JobReference = value;
		}

		int JobSequenceNumber => Factory.GetValue(ref jobSequenceNumberCached, () =>
		{
			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsManifestStatusCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, AsycudaRegistrationStatuses.Codes.Amendment);
			return Logs.Find(logQuery).Length;
		});

		CachedProperty<int> jobSequenceNumberCached;

		public bool IsItineraryTabePageVisible => IsShippingLine && (AMA_ManifestType == TRManifestTypes.Codes.CIKONC || AMA_ManifestType == TRManifestTypes.Codes.VARONC) && (IsSea || IsAir);
		public bool IsManifestToOpenPageVisible => (IsAir && AMA_ManifestType == TRManifestTypes.Codes.HAVIHR) || (IsSea && AMA_ManifestType == TRManifestTypes.Codes.DENIHR);
		public ZBool IsOnlyForSeaAndGrupaj => IsSea && AMA_ManifestType == TRManifestTypes.Codes.GRUPAJ;

		[ReadOnlyMember(nameof(IsOnlyForSeaAndGrupaj))]
		public override ZString AMA_RN_NKConveyanceNationality { get => base.AMA_RN_NKConveyanceNationality; set => base.AMA_RN_NKConveyanceNationality = value; }

		[ReadOnlyMember(nameof(IsOnlyForSeaAndGrupaj))]
		public override ZString AMA_RL_NKPortOfLoading { get => base.AMA_RL_NKPortOfLoading; set => base.AMA_RL_NKPortOfLoading = value; }

		[ReadOnlyMember(nameof(IsOnlyForSeaAndGrupaj))]
		public override ZString AMA_CustomsLoadPort { get => base.AMA_CustomsLoadPort; set => base.AMA_CustomsLoadPort = value; }

		[ReadOnlyMember(nameof(IsOnlyForSeaAndGrupaj))]
		public override ZString AMA_RL_NKPortOfFirstArrival { get => base.AMA_RL_NKPortOfFirstArrival; set => base.AMA_RL_NKPortOfFirstArrival = value; }

		public ResourceStringData ManifestNumberFromMasterBillCaption => IsOnlyForSeaAndGrupaj ? Res.GetData("C0373639-B865-4333-B9AB-1B0317571859", "Previous Dec. No") : Res.GetData("F92DF3DB-AC75-4C59-BCAD-D2CB5ED758D1", "BOL");

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source)
			=> new AsycudaManifestHeaderSynchroniser(this, source);

		#region IRegistrationNoEntryProvider

		ZString IRegistrationNoEntryProvider.RegistrationNumber { get => RegistrationNumber; set => RegistrationNumber = value; }
		ZDateTime IRegistrationNoEntryProvider.RegistrationDate { get => RegistrationDate; set => RegistrationDate = value; }
		SecurityCheckpoint IRegistrationNoEntryProvider.CanModifyRegistrationNumbers => Env.Security.TRModifyRegistrationNumbers;
		BusinessObject IRegistrationNoEntryProvider.ParentBusinessObject => this;

		#endregion
	}
}
