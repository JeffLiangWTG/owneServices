using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public partial class AsycudaManifestHeader
	{
		public override ZBool SupportAssociatedPacks => IsRFM;

		public override ZBool SupportMultipleCustomsNumbers => SupportAssociatedPacks || IsAQM;

		public new partial class Schema
		{
			public const string AMA_DateAtCustomsOfficeVisible = "AMA_DateAtCustomsOfficeVisible";
			public const string CARN = "CARN";
			public const string EstimatedTimeOfLoading = "EstimatedTimeOfLoading";
			public const string PlaceOfEntry = "PlaceOfEntry";
			public const string PlaceOfExit = "PlaceOfExit";
			public const string TSS_Vessel = "TSS_Vessel";
			public const string TSS_VoyageFlight = "TSS_VoyageFlight";
			public const string TSS_RadioCallSign = "TSS_RadioCallSign";
			public const string TSS_CargoCarrierPK = "TSS_CargoCarrierPK";
			public const string TSS_DateOfDeparture = "TSS_DateOfDeparture";
			public const string CallPurposeCode = "CallPurposeCode";
		}

		public ZBool IsAQM => AMA_ManifestType == nameof(ManifestDocumentType.AQM);
		public ZBool IsRFM => AMA_ManifestType == nameof(ManifestDocumentType.RFM);
		public ZBool AMA_DateAtCustomsOfficeVisible => IsRoad;

		#region CARN

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ResourceStringData("ZAAsycudaManifestHeader.CARN", Caption = "CARN")]
		public ZString CARN
		{
			get => CarnCusEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var cusEntryNumber = CarnCusEntryNumber;
				var oldValue = cusEntryNumber?.CE_EntryNum ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						// use local variable for performance
						carnCusEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.SouthAfrica.CustomsAssignedReferenceNumberCARN, AMA_RN_NKCountry);
						RegisterEditableChildObject(carnCusEntryNumber);
					}
					carnCusEntryNumber.CE_EntryNum = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						// use local variable for performance
						carnCusEntryNumber.Delete();
						carnCusEntryNumber = null;
					}
				}
				CARNInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CARNInfo
		{
			get { return GetZPropertyInfo(Schema.CARN); }
		}

		[ReadOnly(true)]
		CusEntryNumber CarnCusEntryNumber
		{
			get
			{
				if (carnCusEntryNumber == null || carnCusEntryNumber.IsDeleted || carnCusEntryNumber.CE_EntryType != CusEntryNumberTypes.SouthAfrica.CustomsAssignedReferenceNumberCARN)
				{
					carnCusEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.SouthAfrica.CustomsAssignedReferenceNumberCARN, AMA_RN_NKCountry);
					if (carnCusEntryNumber != null)
					{
						RegisterEditableChildObject(carnCusEntryNumber);
					}
				}
				return carnCusEntryNumber;
			}
		}
		CusEntryNumber carnCusEntryNumber;

		#endregion

		#region EstimatedTimeOfLoading

		[ResourceStringData("ZAAsycudaManifestHeader.EstimatedTimeOfLoading", Caption = "Estimated Load", ShortCaption = "Est. Load")]
		public ZDateTime EstimatedTimeOfLoading
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.EstimatedTimeOfLoading);
			set
			{
				var oldValue = EstimatedTimeOfLoading;
				this.SetSystemDefinedValue(Schema.EstimatedTimeOfLoading, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEstimatedTimeOfLoading();
				}
				EstimatedTimeOfLoadingInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo EstimatedTimeOfLoadingInfo => GetZPropertyInfo(Schema.EstimatedTimeOfLoading);

		public ZBool EstimatedTimeOfLoadingVisible => AMA_ManifestType == nameof(ManifestDocumentType.ALH) && !PlaceOfEntryVisible;

		#endregion

		#region PlaceOfExit

		[MaxLength(3)]
		[ResourceStringData("ZAAsycudaManifestHeader.PlaceOfExit", Caption = "Place of Exit")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsOffices))]
		public ZString PlaceOfExit
		{
			get => this.GetSystemDefinedValue<ZString>(GenAddOnHelper.PlaceOfExitCode);
			set
			{
				var oldValue = PlaceOfExit;
				CheckMaximumLength(PlaceOfExitInfo, value);
				this.SetSystemDefinedValue(GenAddOnHelper.PlaceOfExitCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePlaceOfExit();
				}
				PlaceOfExitInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo PlaceOfExitInfo => GetZPropertyInfo(Schema.PlaceOfExit);

		public ZBool PlaceOfExitVisible => (IsRoad && AMA_Nature == ShipmentTypeList.Codes.Transhipment28) || IsRail;

		#endregion

		#region PlaceOfEntry

		[MaxLength(3)]
		[ResourceStringData("ZAAsycudaManifestHeader.PlaceOfEntry", Caption = "Place of Entry")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsOffices))]
		public ZString PlaceOfEntry
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.PlaceOfEntry);
			set
			{
				var oldValue = PlaceOfEntry;
				CheckMaximumLength(PlaceOfEntryInfo, value);
				this.SetSystemDefinedValue(Schema.PlaceOfEntry, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePlaceOfEntry();
				}
				PlaceOfEntryInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo PlaceOfEntryInfo => GetZPropertyInfo(Schema.PlaceOfEntry);

		public ZBool PlaceOfEntryVisible => IsRoad && AMA_Nature == ShipmentTypeList.Codes.Transhipment28;

		#endregion

		#region Transhipment Details
		#region Vessel
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TSSRefVessels))]
		[MaxLength(RefVessel.Schema.RV_CodeMaxLength)]
		[ResourceStringData("ZAAsycudaManifestHeader.TSS_Vessel", Caption = "Transhipment Vessel", ShortCaption = "Vessel")]
		public ZString TSS_Vessel
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TSS_Vessel);
			set
			{
				var oldValue = TSS_Vessel;
				CheckMaximumLength(TSS_VesselInfo, value);
				this.SetSystemDefinedValue(Schema.TSS_Vessel, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTSS_Vessel();
				}
				TSS_VesselInfo.RefreshBinding(oldValue);

				SetRadioCallSignIfRequired();
			}
		}

		public ZPropertyInfo TSS_VesselInfo => GetZPropertyInfo(Schema.TSS_Vessel);

		void SetRadioCallSignIfRequired()
		{
			if (!TSS_Vessel.IsEmpty && TSS_RadioCallSign.IsEmpty)
			{
				var vessel = new RefVessel.Loader(Factory).LoadUnique(TSS_Vessel, ZString.Empty, ZString.Empty, ZString.Empty);

				if (vessel != null && !vessel.RV_RadioCallSign.IsEmpty)
				{
					TSS_RadioCallSign = vessel.RV_RadioCallSign;
				}
			}
		}
		#endregion

		#region Voyage
		[MaxLength(Freight.Business.Transport.Schema.JW_VoyageFlightMaxLength)]
		[ResourceStringData("ZAAsycudaManifestHeader.TSS_VoyageFlight", Caption = "Transhipment Voyage/Flight", ShortCaption = "Voyage/Flight")]
		public ZString TSS_VoyageFlight
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TSS_VoyageFlight);
			set
			{
				var oldValue = TSS_VoyageFlight;
				CheckMaximumLength(TSS_VoyageFlightInfo, value);
				this.SetSystemDefinedValue(Schema.TSS_VoyageFlight, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTSS_VoyageFlight();
				}
				TSS_VoyageFlightInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TSS_VoyageFlightInfo => GetZPropertyInfo(Schema.TSS_VoyageFlight);
		#endregion

		#region RadioCallSign
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.VesselRadioCallSigns))]
		[MaxLength(RefVessel.Schema.RV_RadioCallSignMaxLength)]
		[ResourceStringData("ZAAsycudaManifestHeader.TSS_RadioCallSign", Caption = "Transhipment Radio Call Sign/Aircraft Reg", ShortCaption = "Call Sign/Reg")]
		public ZString TSS_RadioCallSign
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TSS_RadioCallSign);
			set
			{
				var oldValue = TSS_RadioCallSign;
				CheckMaximumLength(TSS_RadioCallSignInfo, value);
				this.SetSystemDefinedValue(Schema.TSS_RadioCallSign, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTSS_RadioCallSign();
				}
				TSS_RadioCallSignInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TSS_RadioCallSignInfo => GetZPropertyInfo(Schema.TSS_RadioCallSign);
		#endregion

		#region CargoCarrierCode
		[MaxLength(OrgHeader.Schema.OH_CodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TSSCarrierList))]
		[ResourceStringData("ZAAsycudaManifestHeader.TSSCarrier", Caption = "Transhipment Carrier", ShortCaption = "Carrier")]
		[RelatedBusinessObject(nameof(TSS_CargoCarrier))]
		public ZGuid TSS_CargoCarrierPK
		{
			get => this.GetSystemDefinedValue<ZGuid>(Schema.TSS_CargoCarrierPK);
			set
			{
				var oldValue = TSS_CargoCarrierPK;
				this.SetSystemDefinedValue(Schema.TSS_CargoCarrierPK, value);

				fTSS_CargoCarrier = Factory.Load<OrgHeader>(TSS_CargoCarrierPK);

				if (!IsValidationSuspended)
				{
					Validation.ValidateTSS_CargoCarrierPK();
				}
				TSS_CargoCarrierPKInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TSS_CargoCarrierPKInfo => GetZPropertyInfo(Schema.TSS_CargoCarrierPK);

		public OrgHeader TSS_CargoCarrier => fTSS_CargoCarrier ?? (fTSS_CargoCarrier = Factory.Load<OrgHeader>(TSS_CargoCarrierPK));
		OrgHeader fTSS_CargoCarrier;
		#endregion

		#region DateOfDeparture
		[ResourceStringData("ZAAsycudaManifestHeader.TSS_DateOfDeparture", Caption = "Transhipment Date Of Departure", ShortCaption = "Date Of Departure")]
		public ZDateTime TSS_DateOfDeparture
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.TSS_DateOfDeparture);
			set
			{
				var oldValue = TSS_DateOfDeparture;
				this.SetSystemDefinedValue(Schema.TSS_DateOfDeparture, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTSS_DateOfDeparture();
				}
				TSS_DateOfDepartureInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TSS_DateOfDepartureInfo => GetZPropertyInfo(Schema.TSS_DateOfDeparture);
		#endregion

		public override ZBool IsTSS => base.IsTSS && (IsTSSAir || IsTSSSea);

		public bool IsTSSAir => AMA_TransportMode == TransportModes.Air;

		public bool IsTSSSea => AMA_TransportMode == TransportModes.Sea;
		#endregion

		#region Call Purpose / Direction

		public override ZString AMA_RL_NKPortOfLoading
		{
			get => base.AMA_RL_NKPortOfLoading;
			set
			{
				var changed = AMA_RL_NKPortOfLoading != value;
				base.AMA_RL_NKPortOfLoading = value;
				if (changed && !IsSettingHasChangesSuspended)
				{
					SetCallPurposeCodeByPorts();
				}
			}
		}

		public override ZString AMA_RL_NKPortOfDischarge
		{
			get => base.AMA_RL_NKPortOfDischarge;
			set
			{
				var changed = AMA_RL_NKPortOfDischarge != value;
				base.AMA_RL_NKPortOfDischarge = value;
				if (changed && !IsSettingHasChangesSuspended)
				{
					SetCallPurposeCodeByPorts();
				}
			}
		}

		internal void SetCallPurposeCodeByPorts()
		{
			if (PortOfLoading == null || PortOfDischarge == null)
			{
				CallPurposeCode = ZString.Empty;
			}
			else if (PortOfLoading.Country.Code == CountryCodes.SouthAfrica && PortOfDischarge.Country.Code != CountryCodes.SouthAfrica)
			{
				CallPurposeCode = CallPurposeCodeList.Codes.LoadingCargo;
			}
			else if (PortOfLoading.Country.Code != CountryCodes.SouthAfrica && PortOfDischarge.Country.Code == CountryCodes.SouthAfrica)
			{
				CallPurposeCode = CallPurposeCodeList.Codes.UnloadingCargo;
			}
			else
			{
				CallPurposeCode = ZString.Empty;
			}
		}

		[MaxLength(3)]
		[ResourceStringData("ZAAsycudaManifestHeader.CallPurposeCode", Caption = "Call Purpose / Direction")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CallPurposeCodeList))]
		public ZString CallPurposeCode
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CallPurposeCode);
			set
			{
				var oldValue = CallPurposeCode;
				CheckMaximumLength(CallPurposeCodeInfo, value);
				this.SetSystemDefinedValue(Schema.CallPurposeCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCallPurposeCode();
				}
				CallPurposeCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CallPurposeCodeInfo => GetZPropertyInfo(Schema.CallPurposeCode);

		#endregion

		public override bool HasContainers => AMA_ManifestType != ZaManifestTypes.Codes.BBB;

		public override bool HasBillsAndPacks => AMA_ManifestType != ZaManifestTypes.Codes.ECL;

		public override bool IsRoutingEnabled => false;

		protected override bool HasCustomsNumbers => !CARN.IsEmpty || base.HasCustomsNumbers;

		public override ASYCUDA.Business.BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new ZAMessageSendingNotificationHelper(this);
		}
	}
}
