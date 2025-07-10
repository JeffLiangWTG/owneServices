using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	[CodeProperty(WhsWarehouseSchema.Constants.WW_WarehouseCode), DescriptionProperty(WhsWarehouseSchema.Constants.WW_WarehouseName)]
	public sealed partial class WhsWarehouse : AutoWhsWarehouse, IWhsWarehouse, IWhsWarehouseInternals, IDocAddresses, IAffectLocationView
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string CannotChangeWarehouseTypeToOrFromTransitIfAlreadyReferencedTriggerID = "Invalid Warehouse Type Change.";

		#region Constructors

		public WhsWarehouse(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Schema

		public new abstract class Schema : AutoWhsWarehouse.Schema
		{
			public const string RFPickPackPrinterPK = "RFPickPackPrinterPK";
		}

		#endregion

		#region Business Object Overrides

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new WhsWarehouseFetchStrategy(this);

		public override void Delete()
		{
			var defaultPrinter = StmDefaultPrinter.LoadDefaultPrinter(Factory, this);
			if (defaultPrinter != null)
			{
				defaultPrinter.Delete(); // Tested in StmDefaultPrinterTest
			}

			base.Delete();
			Rows.DeleteAll();
			Areas.DeleteAll();
			ClientParameters.DeleteAll();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			((IWhsWarehouseInternals)this).CreateDefaultArea();
			DodgyHasChangesHackToAvoidDodgyTestFailure();

			WW_TransitSecurityProcessingRequired = false;
			WW_AllowPartialLoadingDefault = false;
		}

		void DodgyHasChangesHackToAvoidDodgyTestFailure()
		{
			if (Areas.Count > 0)
			{
				Areas[0].HasChanges = false;
				HasChanges = false;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var warehouseName = WW_WarehouseNameMultilingual;
				return warehouseName.IsEmpty ?
					Res.GetString("D28933CB-A76F-4EF5-AE9F-D39E56AE1A44", "Warehouse") :
					Res.GetString("1B35FC85-6EC3-4A99-AF8F-00E329ED1E21", "Warehouse {0}", warehouseName);
			}
		}

		#endregion

		#region Related Entites

		#region Areas

		[ChildEditable(true)]
		public WhsAreaCollection Areas
		{
			get
			{
				if (areas == null)
				{
					areas = new WhsAreaCollection(Factory, this);
					areas.CountChanged += Areas_CountChanged;
					RegisterEditableChildObject(areas);
				}
				return areas;
			}
		}

		IWhsAreaCollection IWhsWarehouse.Areas => Areas;

		void Areas_CountChanged(object sender, EventArgs e)
		{
			FreeStoreTextInfo.RefreshBinding();
			BondedTextInfo.RefreshBinding();
			ExciseTextInfo.RefreshBinding();
		}

		#endregion

		#region ClientParameters

		[ChildEditable(true)]
		public WhsClientParameterByWarehouseCollection ClientParameters
		{
			get
			{
				if (fClientParameters == null)
				{
					fClientParameters = new WhsClientParameterByWarehouseCollection(this);
					RegisterEditableChildObject(fClientParameters);
				}
				return fClientParameters;
			}
		}
		WhsClientParameterByWarehouseCollection fClientParameters;

		#endregion

		#region LocationType

		public WhsLocationType LocationType
		{
			get { return Factory.Load<WhsLocationType>(WW_WLT_DefaultLocationType); }
		}

		#endregion

		#region DefaultLocations

		public WhsLocation DefaultInboundDockDoorLocation => Factory.Load<WhsLocation>(WW_DefaultInboundDockDoor);
		public WhsLocation DefaultOutboundDockDoorLocation => Factory.Load<WhsLocation>(WW_DefaultOutboundDockDoor);

		public WhsLocation DefaultLocation => GetDefaultLocation(l => l.WLV_PutawayAreaType != AreaTypes.Codes.InwardProcessing);
		public WhsLocation DefaultLocationInNonBondedArea => GetDefaultLocation(l => l.WLV_PutawayAreaType != AreaTypes.Codes.Bonded && l.WLV_PutawayAreaType != AreaTypes.Codes.InwardProcessing);
		public WhsLocation DefaultLocationInBondedArea => GetDefaultLocation(l => l.WLV_PutawayAreaType == AreaTypes.Codes.Bonded);
		public WhsLocation DefaultLocationInInwardProcessingArea => GetDefaultLocation(l => l.WLV_PutawayAreaType == AreaTypes.Codes.InwardProcessing);

		WhsLocation GetDefaultLocation(Func<WhsLocation, bool> locationPredicate)
		{
			return
				Rows
				.SelectMany(r => r.Locations)
				.FirstOrDefault(
					l => l.WLV_LocationStatus == LocationStatus.Codes.Normal &&
						!l.IsDockDoorLocation &&
						!l.IsPackingStationLocation &&
						!l.IsPackingConsolidationLocation &&
						!l.IsFixedLocation &&
						!l.IsDynamicPickFaceLocation &&
						locationPredicate(l));
		}

		#endregion

		#region Rows

		IWhsRowCollection IWhsWarehouse.Rows => Rows;

		public WhsRowCollection Rows
		{
			get
			{
				rows ??= new WhsRowCollection(this, Factory);
				return rows;
			}
		}

		#endregion

		#region WarehouseAddress

		IOrgAddress IWhsWarehouse.WarehouseAddress => WarehouseAddress;

		public override OrgAddress WarehouseAddress
		{
			get { return Factory.Load<OrgAddress>(WW_OA_WarehouseAddress); }
		}

		#endregion

		#region UNDGs

		[ChildEditable(true)]
		public WhsUNDGLimitCollection UNDGLimits
		{
			get
			{
				if (undgLimits == null)
				{
					undgLimits = new WhsUNDGLimitCollection(this);
					RegisterEditableChildObject(undgLimits);

					SetUNDGLimitsReadOnly();
				}
				return undgLimits;
			}
		}

		void SetUNDGLimitsReadOnly()
		{
			UNDGLimits.SetReadOnlyIncludingChildren(IsDGManagementEnabledReadOnly);
		}

		#endregion

		#endregion

		#region Properties

		#region PackingSlipTitle

		public ZString PackingSlipTitle
		{
			get
			{
				var branchPk = WW_GB_RelatedCompanyBranch.IsValid ? WW_GB_RelatedCompanyBranch.ToGuid() : Guid.Empty;
				return WarehouseDataRegistry.Instance.PackingSlipTitles.GetValueWithoutFallback(Guid.Empty, branchPk, Guid.Empty);
			}
		}

		#endregion

		// persistent

		#region WW_IsActive

		public override ZBool WW_IsActive
		{
			get { return base.WW_IsActive; }
			set
			{
				base.WW_IsActive = value;

				// tested in WhsWarehouseValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateWW_GB_RelatedCompanyBranch();
					Validation.ValidateWW_WarehouseType();
				}
			}
		}

		#endregion

		#region WW_IsVirtualWarehouse

		[ReadOnlyMember(nameof(IsVirtualWarehouseReadOnly))]
		public override ZBool WW_IsVirtualWarehouse
		{
			get { return base.WW_IsVirtualWarehouse; }
			set
			{
				base.WW_IsVirtualWarehouse = value;

				// tested in WhsWarehouseValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateWW_GB_RelatedCompanyBranch();
					Validation.ValidateWW_GG_ReleaseGroup();
					Validation.ValidateWW_WarehouseType();
				}
			}
		}

		#endregion

		#region WW_WarehouseCode

		[ActionField(ReadOnly = true)]
		public override ZString WW_WarehouseCode
		{
			get { return base.WW_WarehouseCode; }
			set { base.WW_WarehouseCode = value; }
		}

		#endregion

		#region WW_WarehouseType

		[List("Lookups.WarehouseTypes")]
		public override ZString WW_WarehouseType
		{
			get { return base.WW_WarehouseType; }
			set
			{
				try
				{
					var prevValue = WW_WarehouseType;

					base.WW_WarehouseType = value;

					if (!VerifyChangeOfWarehouseType())
					{
						base.WW_WarehouseType = prevValue;
					}

					DisableDetailTracking();
					ClearReleaseGroupIfNecessary();

					// tested in WhsWarehouseValidation
					if (!IsValidationSuspended)
					{
						Validation.ValidateWW_GB_RelatedCompanyBranch();
					}
				}
				finally
				{
					WarehouseTypeModifiedWithInvalidUNDGState = false;
				}
			}
		}

		bool VerifyChangeOfWarehouseType()
		{
			if (!WarehouseTypeSupportsUNDGThresholds && (WW_IsDangerousGoodsManagementEnabled || WW_DGThresholdPercentage > 0))
			{
				WarehouseTypeModifiedWithInvalidUNDGState = true;
			}
			return !WarehouseTypeModifiedWithInvalidUNDGState;
		}

		public bool WarehouseTypeSupportsUNDGThresholds
		{
			get
			{
				var supportedTypes = new[] { WarehouseTypes.Codes.Product, WarehouseTypes.Codes.FreeTradeZone, WarehouseTypes.Codes.Transit };
				return supportedTypes.Any(type => type.Equals(WW_WarehouseType, StringComparison.OrdinalIgnoreCase));
			}
		}

		/// <summary>
		/// Only relevant for 'Warehouse Type' Validation, do *not* use for anything else.
		/// This is cleared immediately after it is used in the validation.
		/// </summary>
		internal bool WarehouseTypeModifiedWithInvalidUNDGState { get; private set; }

		public bool WarehouseTypeSupportsTaskManagement => WW_WarehouseType.EqualsIgnoringCase(WarehouseTypes.Codes.Product) || WW_WarehouseType.EqualsIgnoringCase(WarehouseTypes.Codes.FreeTradeZone);

		void ClearReleaseGroupIfNecessary()
		{
			if (!WW_GG_ReleaseGroup.IsEmpty && !WarehouseTypeSupportsTaskManagement)
			{
				WW_GG_ReleaseGroup = ZGuid.Empty;
			}
		}

		#endregion

		#region WW_DGContactPhoneType

		[List("Lookups.PhoneTypes")]
		public override ZString WW_DGContactPhoneType
		{
			get { return base.WW_DGContactPhoneType; }
			set { base.WW_DGContactPhoneType = value; }
		}

		#endregion

		#region WW_OC_DGContact

		[List("Lookups.UNDGContacts")]
		public override ZGuid WW_OC_DGContact
		{
			get { return base.WW_OC_DGContact; }
			set { base.WW_OC_DGContact = value; }
		}

		#endregion

		#region DGPhoneNumber

		[ResourceStringData("WhsWarehouse|DGPhoneNumber", Caption = "DG Phone Number", ShortCaption = "DG Number")]
		public ZString DGPhoneNumber
		{
			get
			{
				var undgContact = Factory.Load<OrgContact>(WW_OC_DGContact);
				return undgContact != null ? undgContact.GetNumberForPhoneType(WW_DGContactPhoneType) : ZString.Empty;
			}
		}

		public ZPropertyInfo DGPhoneNumberInfo
		{
			get { return GetZPropertyInfo(nameof(DGPhoneNumber)); }
		}

		#endregion

		#region WW_GB_RelatedCompanyBranch

		[List("Lookups.RelatedCompanyBranches")]
		public override ZGuid WW_GB_RelatedCompanyBranch
		{
			get { return base.WW_GB_RelatedCompanyBranch; }
			set
			{
				base.WW_GB_RelatedCompanyBranch = value;

				// tested in WhsWarehouseValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateWW_WarehouseType();
				}
			}
		}

		#endregion

		#region WW_OA_WarehouseAddress

		[List("Lookups.WarehouseAddresses")]
		public override ZGuid WW_OA_WarehouseAddress
		{
			get { return base.WW_OA_WarehouseAddress; }
			set
			{
				base.WW_OA_WarehouseAddress = value;
				DisableDetailTracking();
				Validation.ValidateWW_UseGS1PrefixFallback();
			}
		}

		#endregion

		#region WW_DefaultOutboundDockDoor

		[List("Lookups.DockDoorLocations")]
		public override ZGuid WW_DefaultOutboundDockDoor
		{
			get { return base.WW_DefaultOutboundDockDoor; }
			set { base.WW_DefaultOutboundDockDoor = value; }
		}

		#endregion

		#region WW_DefaultInboundDockDoor

		[List("Lookups.DockDoorLocations")]
		public override ZGuid WW_DefaultInboundDockDoor
		{
			get { return base.WW_DefaultInboundDockDoor; }
			set { base.WW_DefaultInboundDockDoor = value; }
		}

		#endregion

		#region WW_WLT_DefaultLocationType

		[List("Lookups.LocationTypes")]
		[RelatedBusinessObject("LocationType")]
		public override ZGuid WW_WLT_DefaultLocationType
		{
			get { return base.WW_WLT_DefaultLocationType; }
			set { base.WW_WLT_DefaultLocationType = value; }
		}

		#endregion

		#region DetailedTracking

		#region WW_FTZDetailedTrackingWarningPercentage

		public override ZShort WW_FTZDetailedTrackingWarningPercentage
		{
			get { return base.WW_FTZDetailedTrackingWarningPercentage; }
			set
			{
				if (value < 0 || value > 99)
				{
					base.WW_FTZDetailedTrackingWarningPercentage = 0;
				}
				else
				{
					base.WW_FTZDetailedTrackingWarningPercentage = value;
				}

				WW_FTZDetailedTrackingWarningPercentageInfo.RefreshBinding();
			}
		}

		#endregion

		#region WW_FTZDetailedTrackingMethod

		[List("Lookups.DetailedTrackingMethods")]
		public override ZString WW_FTZDetailedTrackingMethod
		{
			get { return base.WW_FTZDetailedTrackingMethod; }
			set { base.WW_FTZDetailedTrackingMethod = value; }
		}

		#endregion

		void DisableDetailTracking()
		{
			if (!IsFTZWarehouseInCountryThatUsesPermits)
			{
				WW_FTZIsDetailedTrackingEnabled = false;
			}
		}

		#endregion

		#region RFPickPackPrinterPK

		[List("Lookups.Printers")]
		[ResourceStringData("WhsWarehouse|RFPickPackPrinterPK", Caption = "RF Pick Pack Printer", ShortCaption = "Pick Pack Printer")]
		public ZGuid RFPickPackPrinterPK
		{
			get
			{
				var pickPackPrinter = StmDefaultPrinter.LoadDefaultPrinter(Factory, this);
				return pickPackPrinter != null ? pickPackPrinter.SDP_SQ_Printer : ZGuid.Empty;
			}
			set
			{
				StmDefaultPrinter.SetPrinterPKOrDeleteIfEmpty(Factory, this, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateRFPickPackPrinterPK();
				}

				RFPickPackPrinterPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RFPickPackPrinterPKInfo
		{
			get { return GetZPropertyInfo(Schema.RFPickPackPrinterPK); }
		}

		#endregion

		#region WW_WarehouseName

		[BusinessObjectTestExclude]
		[TranslatableDataField(Schema.TableName, Schema.WW_WarehouseName, MaxLength = Schema.WW_WarehouseNameMaxLength, Type = typeof(WhsWarehouse), Asmid = ResString.AssemblyId)]
		public override ZString WW_WarehouseName
		{
			get { return base.WW_WarehouseName; }
			set { base.WW_WarehouseName = value; }
		}

		public MultilingualString WW_WarehouseNameMultilingual
		{
			get { return GetMultilingual(WW_WarehouseNameInfo); }
		}

		#endregion

		#region Location Alpha

		[ReadOnlyMember(nameof(WW_LocationColumnsZeroBased))]
		public override ZBool WW_LocationColumnsAlpha
		{
			get { return base.WW_LocationColumnsAlpha; }
			set
			{
				base.WW_LocationColumnsAlpha = value;

				if (IsFixedWidthLocation && WW_LocationColumnsAlpha)
				{
					WW_LocationColumnsFixedWidth = 1;
				}
			}
		}

		[ReadOnlyMember(nameof(WW_LocationLevelsZeroBased))]
		public override ZBool WW_LocationLevelsAlpha
		{
			get { return base.WW_LocationLevelsAlpha; }
			set
			{
				base.WW_LocationLevelsAlpha = value;

				if (IsFixedWidthLocation && WW_LocationLevelsAlpha)
				{
					WW_LocationLevelsFixedWidth = 1;
				}
			}
		}

		[ReadOnlyMember(nameof(WW_LocationTraysZeroBased))]
		public override ZBool WW_LocationTraysAlpha
		{
			get { return base.WW_LocationTraysAlpha; }
			set
			{
				base.WW_LocationTraysAlpha = value;

				if (IsFixedWidthLocation && WW_LocationTraysAlpha)
				{
					WW_LocationTraysFixedWidth = 1;
				}
			}
		}

		#endregion

		#region Location Zero Based

		[ReadOnlyMember(nameof(WW_LocationColumnsAlpha))]
		public override ZBool WW_LocationColumnsZeroBased
		{
			get { return base.WW_LocationColumnsZeroBased; }
			set { base.WW_LocationColumnsZeroBased = value; }
		}

		[ReadOnlyMember(nameof(WW_LocationLevelsAlpha))]
		public override ZBool WW_LocationLevelsZeroBased
		{
			get { return base.WW_LocationLevelsZeroBased; }
			set { base.WW_LocationLevelsZeroBased = value; }
		}

		[ReadOnlyMember(nameof(WW_LocationTraysAlpha))]
		public override ZBool WW_LocationTraysZeroBased
		{
			get { return base.WW_LocationTraysZeroBased; }
			set { base.WW_LocationTraysZeroBased = value; }
		}

		#endregion

		#region WW_LocationsHaveLeadingZeros

		[ReadOnlyMember(nameof(IsFixedWidthLocation))]
		public override ZBool WW_LocationsHaveLeadingZeros
		{
			get => base.WW_LocationsHaveLeadingZeros;
			set => base.WW_LocationsHaveLeadingZeros = value;
		}

		#endregion

		#region Location Fixed Width

		[ResourceStringData("WhsWarehouse|IsFixedWidthLocation", Caption = "Fixed Width Location Enabled", ShortCaption = "Fixed Width Location")]
		public ZBool IsFixedWidthLocation
		{
			get
			{
				if (!isFixedWidthLocation.HasValue)
				{
					isFixedWidthLocation = WW_LocationColumnsFixedWidth > 0 || WW_LocationLevelsFixedWidth > 0 || WW_LocationTraysFixedWidth > 0;
				}
				return isFixedWidthLocation.Value;
			}
			set
			{
				SetNonPersistentPropertyValue(IsFixedWidthLocationInfo, ref isFixedWidthLocation, value);
				UpdateFixedWidthRelatedParameters(value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsFixedWidthLocation();
				}
			}
		}
		ZBool? isFixedWidthLocation;

		void UpdateFixedWidthRelatedParameters(bool isFixedWidth)
		{
			if (isFixedWidth)
			{
				WW_LocationsHaveLeadingZeros = true;

				if (WW_LocationColumnsAlpha)
				{
					WW_LocationColumnsFixedWidth = 1;
				}
				else
				{
					Validation.ValidateWW_LocationColumnsFixedWidth();
				}

				if (WW_LocationLevelsAlpha)
				{
					WW_LocationLevelsFixedWidth = 1;
				}
				else
				{
					Validation.ValidateWW_LocationLevelsFixedWidth();
				}

				if (WW_LocationTraysAlpha)
				{
					WW_LocationTraysFixedWidth = 1;
				}
				else
				{
					Validation.ValidateWW_LocationTraysFixedWidth();
				}
			}
			else
			{
				WW_LocationColumnsFixedWidth = 0;
				WW_LocationLevelsFixedWidth = 0;
				WW_LocationTraysFixedWidth = 0;
			}
		}

		public ZPropertyInfo IsFixedWidthLocationInfo => GetZPropertyInfo(nameof(IsFixedWidthLocation));

		[ReadOnlyMember(nameof(LocationColumnsFixedWidthReadOnly))]
		public override ZByte WW_LocationColumnsFixedWidth
		{
			get => base.WW_LocationColumnsFixedWidth;
			set => base.WW_LocationColumnsFixedWidth = value;
		}

		bool LocationColumnsFixedWidthReadOnly => !IsFixedWidthLocation || WW_LocationColumnsAlpha;

		[ReadOnlyMember(nameof(LocationLevelsFixedWidthReadOnly))]
		public override ZByte WW_LocationLevelsFixedWidth
		{
			get => base.WW_LocationLevelsFixedWidth;
			set => base.WW_LocationLevelsFixedWidth = value;
		}

		bool LocationLevelsFixedWidthReadOnly => !IsFixedWidthLocation || WW_LocationLevelsAlpha;

		[ReadOnlyMember(nameof(LocationTraysFixedWidthReadOnly))]
		public override ZByte WW_LocationTraysFixedWidth
		{
			get => base.WW_LocationTraysFixedWidth;
			set => base.WW_LocationTraysFixedWidth = value;
		}

		bool LocationTraysFixedWidthReadOnly => !IsFixedWidthLocation || WW_LocationTraysAlpha;

		#endregion

		#region WW_IsDangerousGoodsManagementEnabled

		public override ZBool WW_IsDangerousGoodsManagementEnabled
		{
			get => base.WW_IsDangerousGoodsManagementEnabled;
			set
			{
				base.WW_IsDangerousGoodsManagementEnabled = value;
				SetUNDGLimitsReadOnly();
			}
		}

		#endregion

		[ReadOnlyMember(nameof(IsDGManagementEnabledReadOnly))]
		public override ZByte WW_DGThresholdPercentage
		{
			get => base.WW_DGThresholdPercentage;
			set
			{
				if (value > 99)
				{
					base.WW_DGThresholdPercentage = 0;
				}
				else
				{
					base.WW_DGThresholdPercentage = value;
				}
			}
		}

		// calculated

		#region CountryCode

		public ZString CountryCode => WarehouseAddress.GetCountryCode();

		public ZPropertyInfo CountryCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CountryCode)); }
		}

		#endregion

		#region Enabled Transactions

		public ZBool IsWarehouseFreeStoreEnabled => Areas.Count == 0 || DoesWarehouseContainAreaType(AreaTypes.Codes.FreeStore);

		public ZBool IsWarehouseBondEnabled => DoesWarehouseContainAreaType(AreaTypes.Codes.Bonded);

		public ZBool IsWarehouseExciseEnabled => DoesWarehouseContainAreaType(AreaTypes.Codes.Excise);

		public ZBool IsInwardProcessingEnabled => DoesWarehouseContainAreaType(AreaTypes.Codes.InwardProcessing);

		public ZBool IsVATFiscalEnabled => DoesWarehouseContainAreaType(AreaTypes.Codes.VATFiscal);

		bool DoesWarehouseContainAreaType(string areaType) => Areas.Any(a => a.WA_AreaType.EqualsIgnoringCase(areaType));

		public ZPropertyInfo IsWarehouseFreeStoreEnabledInfo => GetZPropertyInfo(nameof(IsWarehouseFreeStoreEnabled));

		public ZPropertyInfo IsWarehouseBondEnabledInfo => GetZPropertyInfo(nameof(IsWarehouseBondEnabled));

		public ZPropertyInfo IsWarehouseExciseEnabledInfo => GetZPropertyInfo(nameof(IsWarehouseExciseEnabled));

		public ZPropertyInfo IsInwardProcessingEnabledInfo => GetZPropertyInfo(nameof(IsInwardProcessingEnabled));

		public ZPropertyInfo IsVATFiscalEnabledInfo => GetZPropertyInfo(nameof(IsVATFiscalEnabled));

		#endregion

		#region Transaction Types

		[MaxLength(256)]
		public ZString BondedText
		{
			get { return GetTransactionTypeText(AreaTypes.Descriptions.Bonded, IsWarehouseBondEnabled); }
		}

		public ZPropertyInfo BondedTextInfo
		{
			get { return GetZPropertyInfo(nameof(BondedText)); }
		}

		[MaxLength(256)]
		public ZString ExciseText
		{
			get { return GetTransactionTypeText(AreaTypes.Descriptions.Excise, IsWarehouseExciseEnabled); }
		}

		public ZPropertyInfo ExciseTextInfo
		{
			get { return GetZPropertyInfo(nameof(ExciseText)); }
		}

		[MaxLength(256)]
		public ZString FreeStoreText
		{
			get { return GetTransactionTypeText(AreaTypes.Descriptions.FreeStore, IsWarehouseFreeStoreEnabled); }
		}

		public ZPropertyInfo FreeStoreTextInfo
		{
			get { return GetZPropertyInfo(nameof(FreeStoreText)); }
		}

		[MaxLength(256)]
		public ZString InwardProcessingText
		{
			get { return GetTransactionTypeText(AreaTypes.Descriptions.InwardProcessing, IsInwardProcessingEnabled); }
		}

		public ZPropertyInfo InwardProcessingTextInfo
		{
			get { return GetZPropertyInfo(nameof(InwardProcessingText)); }
		}

		[MaxLength(256)]
		public ZString VATFiscalText
		{
			get { return GetTransactionTypeText(AreaTypes.Descriptions.VATFiscal, IsVATFiscalEnabled); }
		}

		public ZPropertyInfo VATFiscalTextInfo
		{
			get { return GetZPropertyInfo(nameof(VATFiscalText)); }
		}

		ZString GetTransactionTypeText(MultilingualString transactionType, bool isEnabled)
		{
			var prefix = isEnabled ? "" : Res.GetString("5370c0f4-fa76-4387-bcc5-3213bc11b93f", "Not enabled for") + " ";
			return ZString.Format("{0}{1}", prefix, transactionType);
		}

		#endregion

		#region WarehouseTypeDescription

		[ResourceStringData("WhsWarehouse|WarehouseTypeDescription", Caption = "Type")]
		public ZString WarehouseTypeDescription
		{
			get { return Lookups.WarehouseTypes.GetDescriptionFromCode(WW_WarehouseType); }
		}

		#endregion

		#region WorkTimes

		[ChildEditable]
		public GlbWorkTimeCollection WorkTimes
		{
			get
			{
				if (fWorkTimes == null)
				{
					var worktimeQuery = new ZQuery(GlbWorkTimeSchema.GW_ParentTableCode, WhsWarehouseSchema.Constants.Prefix);
					worktimeQuery.AddToFilter(GlbWorkTimeSchema.GW_ParentID, PK);
					worktimeQuery.FetchOnlyFromLocalCache = !IsInDatabase;

					fWorkTimes = new GlbWorkTimeCollection(Factory, worktimeQuery);
					RegisterEditableChildObject(fWorkTimes);
				}

				return fWorkTimes;
			}
		}
		GlbWorkTimeCollection fWorkTimes;

		public GlbWorkTimeViewModel WorkTimeViewModel
		{
			get
			{
				if (fWorkTimeViewModel == null)
				{
					fWorkTimeViewModel = new GlbWorkTimeViewModel(WorkTimes, false);
				}
				return fWorkTimeViewModel;
			}
		}
		GlbWorkTimeViewModel fWorkTimeViewModel;

		#endregion

		#endregion

		#region Flags

		#region IsApprovedKnown

		public bool IsApprovedKnown
		{
			get
			{
				bool result = false;
				switch (CountryCode)
				{
					case Enterprise.Core.Constants.CountryCodes.UnitedStates:
						result = Warehouse.Environment.Business.US.TSAInfo.IsOrgAddressTSAKnown(WarehouseAddress);
						break;
				}
				return result;
			}
		}

		#endregion

		#region IsFTZWarehouse

		public bool IsFTZWarehouse => WW_WarehouseType == WarehouseTypes.Codes.FreeTradeZone;

		#endregion

		#region IsFTZWarehouseInCountryThatUsesPermits

		public bool IsFTZWarehouseInCountryThatUsesPermits => IsFTZWarehouse && BondedHelper.IsCountrySupportedForFTZPermits(CountryCode);

		#endregion

		#region IsUsingDockDoorLocation

		public bool IsUsingDockDoorLocation => WW_IsActive && !WW_IsVirtualWarehouse && (IsProductWarehouse || IsFTZWarehouse);

		bool IsProductWarehouse => WW_WarehouseType == WarehouseTypes.Codes.Product;

		#endregion

		#region HasPackingConsolidationLocations

		public bool HasPackingConsolidationLocations
		{
			get
			{
				var query = new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, PK);
				query.AddToFilter(WhsLocationViewSchema.WLV_LocationClass, LocationClasses.Codes.CON);
				return Factory.Exists(typeof(WhsLocation), query);
			}
		}

		#endregion

		#region IsVirtualWarehouseReadOnly

		bool IsVirtualWarehouseReadOnly => WW_WarehouseType != WarehouseTypes.Codes.Product && WW_WarehouseType != WarehouseTypes.Codes.FreeTradeZone;

		#endregion

		#region IsDGManagementEnabledReadOnly

		bool IsDGManagementEnabledReadOnly => !WW_IsDangerousGoodsManagementEnabled;

		#endregion

		#region IsTaskManagementEnabled

		public bool IsTaskManagementEnabled => WW_GG_ReleaseGroup.IsValid;

		#endregion

		#endregion

		#region GetSSCCPrefix

		public ZString GetSSCCPrefix()
		{
			ZString result = "";

			var warehouseAddress = WarehouseAddress;
			if (warehouseAddress != null)
			{
				var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.GS1);
				query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, warehouseAddress.PK);

				var cusCodes = (OrgCusCode[])warehouseAddress.Header.CustomsCodes.Find(query);
				if (cusCodes.Length > 0)
				{
					result = cusCodes[0].OK_CustomsRegNo;
				}
				else
				{
					query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.GS1);
					query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, null);

					cusCodes = (OrgCusCode[])warehouseAddress.Header.CustomsCodes.Find(query);
					if (cusCodes.Length > 0)
					{
						result = cusCodes[0].OK_CustomsRegNo;
					}
				}
			}

			return result;
		}

		#endregion

		#region Find Location

		public WhsLocation FindLocation(ZString locationString)
		{
			return WhsLocation.FindLocation(Factory, locationString, PK);
		}

		#endregion

		#region Find Location Parts

		/// <summary>
		/// Returns a class containing the Row, Column, Leven and Tray location parts for a given location string.
		/// </summary>
		/// <param name="locationString"></param>
		public LocationParts FindLocationParts(ZString locationString)
		{
			var location = FindLocation(locationString);

			LocationParts result = null;
			if (location != null)
			{
				result = new LocationParts(location.Row, location.WLV_Column, location.WLV_Level, location.WLV_Tray);
			}
			return result;
		}

		public class LocationParts
		{
			public LocationParts(WhsRow row, short? column, short? level, short? tray)
			{
				Row = row;
				Column = column;
				Level = level;
				Tray = tray;
			}

			public readonly WhsRow Row;
			public readonly short? Column;
			public readonly short? Level;
			public readonly short? Tray;
		}

		#endregion

		#region IWarehouseInternals

		void IWhsWarehouseInternals.GenerateLocations()
		{
			foreach (WhsRow row in Rows)
			{
				((WhsRowInternals)row).GenerateLocations();
			}
		}

		void IWhsWarehouseInternals.CreateDefaultArea()
		{
			if (Areas.Count < 1)
			{
				var area = Areas.AddNew();
				area.WA_Name = "DEFAULT";
				area.WA_WW_Whs = base.PK;
				area.WA_IsDefaultPickArea = true;
				area.WA_IsDefaultPutawayArea = true;
			}

			if (WW_IsBondedWarehouse)
			{
				Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;
			}
		}

		#endregion

		#region SetUpBondedWarehouse

		public void SetUpBondedWarehouse(OrgAddress warehouseAddress, ZString? warehouseType = null)
		{
			if (warehouseType.HasValue)
			{
				WW_WarehouseType = warehouseType.Value;
			}
			WW_WarehouseName = warehouseAddress.OA_Address1;
			WW_OA_WarehouseAddress = warehouseAddress.PK;
			WW_IsBondedWarehouse = true;
			WW_IsVirtualWarehouse = true;
			Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;

			WW_AutoPrintPickingSlip = false;
			WW_AutoPrintPackingSlip = false;

			var row = Rows.AddNew();
			row.WR_Name = "BOND";
			row.WR_Columns = 1;
			row.WR_Levels = 1;
			row.WR_Trays = 1;
			row.WR_WW_Whs = PK;
			((IWhsWarehouseInternals)this).GenerateLocations();
		}

		#endregion

		#region Save

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateVirtualWarehouseCodeIfRequired();
			CreateDefaultDockDoorLocationsIfNeeded();
			LocationViewReloaderService.AddLocationReloaderService(Factory);
			RunUpdatePackageStateSecurityStatusStoreProcedureWhenChangeSecurityProcessingRequiredToTrue();
			RunUpdatePackageStateRCNCustomStatusStoreProcedureWhenChangeInWareHouseConfigCustomsOrPortAuthorityControl();
		}

		void PopulateVirtualWarehouseCodeIfRequired()
		{
			if (WW_IsVirtualWarehouse && WW_WarehouseCode.IsEmpty)
			{
				const string sql = "SELECT WW_WarehouseCode FROM dbo.WhsWarehouse WITH(TABLOCKX)";
				var existingWhsCodes = new DynamicBusinessObjectCollection(Factory);

				try
				{
					existingWhsCodes.Load(sql);
				}
				catch (SqlException) // eg. could not get lock..
				{
					throw new ZCannotSaveException("Another user is currently saving a Warehouse. Try saving again.", "Concurrent Edit");
				}

				const int MAX_ATTEMPTS = 1000;
				int warehouseCodeNo;
				var existingWhsCodesHash = new HashSet<ZString>(existingWhsCodes.Select(o => (ZString)o[WhsWarehouseSchema.Constants.WW_WarehouseCode]));

				for (warehouseCodeNo = 1; warehouseCodeNo < MAX_ATTEMPTS; warehouseCodeNo++)
				{
					if (!existingWhsCodesHash.Contains(warehouseCodeNo.ToString(Culture.Invariant)))
					{
						break; // current code is unused
					}
				}

				if (warehouseCodeNo < MAX_ATTEMPTS) // code remains empty if 1-999 are used (practically speaking, this will not happen)
				{
					WW_WarehouseCode = warehouseCodeNo.ToString(Culture.Invariant);
				}
			}
		}

		void CreateDefaultDockDoorLocationsIfNeeded()
		{
			if (!IsInDatabase && IsUsingDockDoorLocation)
			{
				if (WW_DefaultInboundDockDoor.IsEmpty && WW_DefaultOutboundDockDoor.IsEmpty)
				{
					var noDDLLocationTypeMessage = Res.GetString("2E9C420F-105B-45B8-BF03-730B93A4C6DE", "Please create a Location Type that has a class of DDL.");
					var dockDoorLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL));
					if (dockDoorLocationType != null)
					{
						var defaultArea = Areas.AddNew();
						defaultArea.WA_Name = DefaultDockDoorAreaName;
						defaultArea.WA_AreaType = AreaTypes.Codes.DockDoor;

						var dockDoorRow = Rows.AddNew();
						dockDoorRow.WR_Name = DefaultDockDoorRowName;
						dockDoorRow.WR_Columns = 1;
						dockDoorRow.WR_Levels = 1;
						dockDoorRow.WR_Trays = 1;

						((WhsRowInternals)dockDoorRow).GenerateLocations();
						var defaultDockDoorLocation = dockDoorRow.Locations.Single();
						defaultDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
						defaultDockDoorLocation.WLV_WA_PickingArea = defaultArea.PK;
						defaultDockDoorLocation.WLV_WA_PutawayArea = defaultArea.PK;
						defaultDockDoorLocation.WLV_PutawayPathSequence = 1;

						WW_DefaultInboundDockDoor = defaultDockDoorLocation.PK;
						WW_DefaultOutboundDockDoor = defaultDockDoorLocation.PK;
						ClearRowNotificationsContaining(noDDLLocationTypeMessage);
					}
					else
					{
						AddRowError(noDDLLocationTypeMessage);
					}
				}
				else if (WW_DefaultInboundDockDoor.IsEmpty)
				{
					WW_DefaultInboundDockDoor = WW_DefaultOutboundDockDoor;
				}
				else if (WW_DefaultOutboundDockDoor.IsEmpty)
				{
					WW_DefaultOutboundDockDoor = WW_DefaultInboundDockDoor;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RunUpdatePackageStateSecurityStatusStoreProcedureWhenChangeSecurityProcessingRequiredToTrue()
		{
			if (WW_TransitSecurityProcessingRequiredInfo.HasChanges && WW_GB_RelatedCompanyBranch.IsValid)
			{
				var branchPK = WW_GB_RelatedCompanyBranch.ToGuid();
				var userName = ((IGlbStaff)Env.CurrentUser).GS_Code;
				try
				{
					var sql = "EXEC dbo.UpdatePackageStateSecurityStatus @companyBranchPK, @systemLastEditUser, @RegistryValue, @WarehouseConfigurationValue, @PackageStatePKs";

					var command = ((IDbConnected)Factory).Connection.Command(sql);

					command.AddParameter("@companyBranchPK", SqlDbType.UniqueIdentifier, branchPK);
					command.AddParameter("@systemLastEditUser", SqlDbType.Char, Convert.ToString(userName));
					command.AddParameter("@RegistryValue", SqlDbType.Bit, DBNull.Value);
					command.AddParameter("@WarehouseConfigurationValue", SqlDbType.Bit, Convert.ToBoolean(WW_TransitSecurityProcessingRequired));
					command.AddTableValuedParameter("@PackageStatePKs", "dbo.TVP_uniqueidentifier", Enumerable.Empty<Guid>());
					command.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					ErrorReporter.ReportOnce(ex.Message, ex);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RunUpdatePackageStateRCNCustomStatusStoreProcedureWhenChangeInWareHouseConfigCustomsOrPortAuthorityControl()
		{
			if ((WW_IsCustomsControlledInfo.HasChanges || WW_IsPortAuthorityControlledInfo.HasChanges) && WW_GB_RelatedCompanyBranch.IsValid)
			{
				var branchPK = WW_GB_RelatedCompanyBranch.ToGuid();
				var userName = ((IGlbStaff)Env.CurrentUser).GS_Code;
				var registryValue = WarehouseDataRegistry.Instance.ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, Guid.Empty);
				try
				{
					var sql = "EXEC dbo.UpdatePackageStateAndRCNCustomStatus @CompanyBranchPK, @SystemLastEditUser, @CurrentUTC, @WarehouseConfigCustomControlled, @WarehouseConfigPortControlled, @IsApplyPackageQuantityCountingAlgorithm, @PackageStatePKs";

					var command = ((IDbConnected)Factory).Connection.Command(sql);

					command.AddParameter("@CompanyBranchPK", SqlDbType.UniqueIdentifier, branchPK);
					command.AddParameter("@SystemLastEditUser", SqlDbType.Char, Convert.ToString(userName));
					command.AddParameter("@CurrentUTC", SqlDbType.DateTime, DBNull.Value);
					command.AddParameter("@WarehouseConfigCustomControlled", SqlDbType.Bit, Convert.ToBoolean(WW_IsCustomsControlled));
					command.AddParameter("@WarehouseConfigPortControlled", SqlDbType.Bit, Convert.ToBoolean(WW_IsPortAuthorityControlled));
					command.AddParameter("@IsApplyPackageQuantityCountingAlgorithm", SqlDbType.Bit, registryValue);
					command.AddTableValuedParameter("@PackageStatePKs", "dbo.TVP_uniqueidentifier", Enumerable.Empty<Guid>());
					command.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					ErrorReporter.ReportOnce(ex.Message, ex);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is a hardcoded constant")]
		public const string DefaultDockDoorAreaName = "[DEFAULT DOCK DOOR]";
		public const string DefaultDockDoorRowName = "DOCKDOOR"; // this is a hardcoded constant

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers => base.UniqueIndexFailureHandlers
			.Append(new LocationStringUniqueIndexValidationHandler(Res.GetString("5ff585c9-d0a8-47fd-b0e5-434777b481c2", "The changes to the location configurations on the warehouse have resulted in a duplicate Location Barcode in the warehouse. Ensure Row Names for this warehouse will not result in Duplicate Location Barcodes if enabling Fixed Width Locations.")));

		#endregion

		#region Implementation

		WhsAreaCollection areas;
		WhsRowCollection rows;
		WhsUNDGLimitCollection undgLimits;

		#endregion

		#region IDocAddresses Members

		[ChildEditable()]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.Warehouse;
		}

		ZString IDocAddresses.HumanReadableName
		{
			get { return (NoResString)"Warehouse " + WW_WarehouseCode; } // programmatic constant
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return new DocAddressType[] { DocAddressType.PickUpAddress }; }
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region IAffectLocationView

		SchemaColumn[] IAffectLocationView.GetColumnsThatAffectLocationView()
		{
			return new SchemaColumn[]
			{
				WhsWarehouseSchema.WW_LocationComponentDelimiter,
				WhsWarehouseSchema.WW_LocationColumnsAlpha,
				WhsWarehouseSchema.WW_LocationColumnsFixedWidth,
				WhsWarehouseSchema.WW_LocationColumnsZeroBased,
				WhsWarehouseSchema.WW_LocationLevelsAlpha,
				WhsWarehouseSchema.WW_LocationLevelsFixedWidth,
				WhsWarehouseSchema.WW_LocationLevelsZeroBased,
				WhsWarehouseSchema.WW_LocationTraysAlpha,
				WhsWarehouseSchema.WW_LocationTraysFixedWidth,
				WhsWarehouseSchema.WW_LocationTraysZeroBased,
				WhsWarehouseSchema.WW_IsVirtualWarehouse,
				WhsWarehouseSchema.WW_WarehouseType,
			};
		}

		void IAffectLocationView.ReloadLocationsFromDB()
		{
			var query = new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, PK);
			query.ReLoadExistingRows = true;
			Factory.Load<WhsLocation>(query);
		}

		ZGuid IAffectLocationView.ParentThatMayReloadMyLocations
		{
			get { return ZGuid.Empty; }
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			CreateDefaultDockDoorLocationIfNeeded_ForTest();

			base.FillWithValidTestDataCore(kind, propertyPath);

			var locationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "RNO"));
			WW_WLT_DefaultLocationType = locationType.PK;
		}

#endif
		#endregion
	}

	#region Internals

	public interface IWhsWarehouseInternals
	{
		void GenerateLocations();
		void CreateDefaultArea();
	}

	#endregion
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Environment.Business
{
	public partial class WhsWarehouse
	{
		/// <summary>
		/// This exists so that the Whs Test Helpers can create the Default DDL without having to do a Factory.Save().
		/// This compromise enables faster tests.
		/// </summary>
		public void CreateDefaultDockDoorLocationIfNeeded_ForTest()
		{
			CreateDefaultDockDoorLocationsIfNeeded();
		}
	}
}
#endif
#endregion
