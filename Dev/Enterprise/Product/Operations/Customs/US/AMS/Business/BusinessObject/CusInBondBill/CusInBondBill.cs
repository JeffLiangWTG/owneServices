using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	[CodeProperty("IssuerCodeAndMasterBillNumber"), DescriptionProperty(CusInBondBill.Schema.B0_MasterBillNumber)]
	public class CusInBondBill : Customs.Business.CusInBondBill,
		Integration.Customs.US.USAMS.ICusInBondBill,
		IDocAddresses,
		ICanDelete,
		IDispositionCodeDateParent,
		IDispositionCodeColumnsForFastSearchProvider,
		ISequenceNumberHeader,
		ICusCodeDataTypeSupporter,
		ICusAddInfoTypeSupporter,
		IValidationModesSupporter,
		ISailingSynchronisationTarget<BillOfLading>,
		IManifestBillForSynchroniser,
		IWorkflowTriggerEventSource,
		ICusInbondBillAddRefTypeSupporter
	{
		public CusInBondBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInBondBill.Schema
		{
			public const string B0_BillStatusDescription = "B0_BillStatusDescription";
			public const string B0_ForeignPortOfUnladingKCode = "B0_ForeignPortOfUnladingKCode";
			public const string B0_InBondStatus = "B0_InBondStatus";
			public const string B0_InBondStatusDescription = "B0_InBondStatusDescription";
			public const string B0_InBondMessageStatus = "B0_InBondMessageStatus";
			public const string B0_InBondMessageStatusDescription = "B0_InBondMessageStatusDescription";
			public const string B0_LatestDispositionCode = "B0_LatestDispositionCode";
			public const string B0_LatestDispositionCodeDescription = "B0_LatestDispositionCodeDescription";
			public const string B0_LatestISFDispositionCode = "B0_LatestISFDispositionCode";
			public const string B0_LatestISFDispositionCodeDescription = "B0_LatestISFDispositionCodeDescription";
			public const string B0_LatestPTTDispositionCode = "B0_LatestPTTDispositionCode";
			public const string B0_LatestPTTDispositionCodeDescription = "B0_LatestPTTDispositionCodeDescription";
			public const string B0_PlaceOfDelivery = "B0_PlaceOfDelivery";
			public const string B0_NoOfAMSContainers = "B0_NoOfAMSContainers";
			public const string B0_NoOfSailingContainers = "B0_NoOfSailingContainers";
			public const string B0_DateTimeOfDischarge = "B0_DateTimeOfDischarge";
		}
		#region New Properties

		#region B0_NoOfAMSContainers

		public ZInt B0_NoOfAMSContainers
		{
			get { return MovementDetail?.Containers?.Count ?? ZInt.Zero; }
		}

		public ZPropertyInfo B0_NoOfAMSContainersInfo
		{
			get { return GetZPropertyInfo(Schema.B0_NoOfAMSContainers); }
		}

		#endregion

		#region B0_NoOfSailingContainers

		public ZInt B0_NoOfSailingContainers
		{
			get
			{
				if (!fB0_NoOfSailingContainers.HasValue)
				{
					var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)this).Source;
					fB0_NoOfSailingContainers = sailingBill != null ? sailingBill.RealContainers.Count : 0;
				}
				return fB0_NoOfSailingContainers.Value;
			}
		}
		ZInt? fB0_NoOfSailingContainers;

		internal void RefreshSailingStatistics()
		{
			fB0_NoOfSailingContainers = null;
			B0_NoOfSailingContainersInfo.RefreshBinding();
		}

		public ZPropertyInfo B0_NoOfSailingContainersInfo
		{
			get { return GetZPropertyInfo(Schema.B0_NoOfSailingContainers); }
		}

		#endregion

		#region Foreign Port Of Unlading K Code
		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.ScheduleKList))]
		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondBill|B0_ForeignPortOfUnladingKCode", Caption = "Foreign Port Of Unlading")]
		[MaxLength(5)]
		public ZString B0_ForeignPortOfUnladingKCode
		{
			get
			{
				var foreignPortOfUnlading = ForeignPortOfUnlading;
				return foreignPortOfUnlading == null ? ZString.Empty : foreignPortOfUnlading.BR_ReferenceNum;
			}
			set
			{
				LocateAndChangeShipmentReference(BillReferenceList.Codes.CSK, B0_ForeignPortOfUnladingKCodeInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateB0_ForeignPortOfUnladingKCode();
				}
			}
		}

		public CusInbondBillAddRef ForeignPortOfUnlading
		{
			get
			{
				if (foreignPortOfUnlading == null || foreignPortOfUnlading.IsDeleted || foreignPortOfUnlading.BR_Qualifier != BillReferenceList.Codes.CSK)
				{
					foreignPortOfUnlading = ShipmentReferenceDetails[BillReferenceList.Codes.CSK];
				}
				return foreignPortOfUnlading;
			}
		}
		CusInbondBillAddRef foreignPortOfUnlading;

		public ZPropertyInfo B0_ForeignPortOfUnladingKCodeInfo
		{
			get { return GetZPropertyInfo(Schema.B0_ForeignPortOfUnladingKCode); }
		}
		#endregion

		#region Place Of Delivery
		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.UNLOCOCollection))]
		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondBill|B0_PlaceOfDelivery", Caption = "Place Of Delivery")]
		[MaxLength(5)]
		public ZString B0_PlaceOfDelivery
		{
			get
			{
				var placeOfDelivery = PlaceOfDelivery;
				return placeOfDelivery == null ? ZString.Empty : placeOfDelivery.BR_ReferenceNum;
			}
			set
			{
				LocateAndChangeShipmentReference(BillReferenceList.Codes.ULC, B0_PlaceOfDeliveryInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateB0_PlaceOfDelivery();
				}
			}
		}

		public CusInbondBillAddRef PlaceOfDelivery
		{
			get
			{
				if (placeOfDelivery == null || placeOfDelivery.IsDeleted || placeOfDelivery.BR_Qualifier != BillReferenceList.Codes.ULC)
				{
					placeOfDelivery = ShipmentReferenceDetails[BillReferenceList.Codes.ULC];
				}
				return placeOfDelivery;
			}
		}
		CusInbondBillAddRef placeOfDelivery;

		public ZPropertyInfo B0_PlaceOfDeliveryInfo
		{
			get { return GetZPropertyInfo(Schema.B0_PlaceOfDelivery); }
		}
		#endregion

		#region InBond Status

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_InBondStatus", Caption = "In-Bond Status")]
		public ZString B0_InBondStatus
		{
			get
			{
				if (inBondStatusCached == null)
				{
					inBondStatusCached = new CachedProperty<ZString>(Factory, () => GetStatusFromMovementDetails(CusInBondMoveDetail.Schema.B9_CustomsStatus));
				}
				return inBondStatusCached.Value;
			}
		}
		CachedProperty<ZString> inBondStatusCached;

		public ZPropertyInfo B0_InBondStatusInfo
		{
			get { return GetZPropertyInfo(Schema.B0_InBondStatus); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_InBondStatusDescription", Caption = "In-Bond Status Description", ShortCaption = "In-Bond Status Desc.")]
		public ZString B0_InBondStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(B0_InBondStatus); }
		}

		public ZPropertyInfo B0_InBondStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.B0_InBondStatusDescription); }
		}

		#endregion

		#region InBond Message Status

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_InBondMessageStatus", Caption = "In-Bond Message Status", ShortCaption = "In-Bond Msg. Stat.")]
		public ZString B0_InBondMessageStatus
		{
			get
			{
				if (inBondMessageStatusCached == null)
				{
					inBondMessageStatusCached = new CachedProperty<ZString>(Factory, () => GetStatusFromMovementDetails(CusInBondMoveDetail.Schema.B9_MessageStatus));
				}
				return inBondMessageStatusCached.Value;
			}
		}
		CachedProperty<ZString> inBondMessageStatusCached;

		public ZPropertyInfo B0_InBondMessageStatusInfo
		{
			get { return GetZPropertyInfo(Schema.B0_InBondMessageStatus); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_InBondMessageStatusDescription", Caption = "In-Bond Message Status Description", ShortCaption = "In-Bond Msg. Stat. Desc.")]
		public ZString B0_InBondMessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(B0_InBondMessageStatus); }
		}

		public ZPropertyInfo B0_InBondMessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.B0_InBondMessageStatusDescription); }
		}

		#endregion

		#region Latest Disposition Code

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_LatestDispositionCode", Caption = "Latest Disposition", MediumCaption = "Latest Disp.", ShortCaption = "Disposition")]
		public ZString B0_LatestDispositionCode
		{
			get
			{
				var latestDisposition = LatestDisposition;
				return latestDisposition == null ? ZString.Empty : latestDisposition.US_Code;
			}
		}

		public ZPropertyInfo B0_LatestDispositionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.B0_LatestDispositionCode); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_LatestDispositionCodeDescription", Caption = "Latest Disposition Description", MediumCaption = "Latest Disp. Desc.", ShortCaption = "Disposition Desc.")]
		public ZString B0_LatestDispositionCodeDescription
		{
			get { return DispositionCodeDescriptionList.GetDescriptionFromCode(B0_LatestDispositionCode) ?? ZString.Empty; }
		}

		public ZPropertyInfo B0_LatestDispositionCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.B0_LatestDispositionCodeDescription); }
		}

		public DispositionData LatestDisposition
		{
			get
			{
				if (latestDispositionCached == null)
				{
					latestDispositionCached = new CachedProperty<DispositionData>(Factory, delegate
					{ return DispositionCodes.GetLatestDisposition(); });
				}
				return latestDispositionCached.Value;
			}
		}
		CachedProperty<DispositionData> latestDispositionCached;

		#endregion

		#region Latest ISF Disposition Code

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_LatestISFDispositionCode", Caption = "Latest ISF Disposition", MediumCaption = "Latest ISF Disp.", ShortCaption = "ISF")]
		public ZString B0_LatestISFDispositionCode
		{
			get
			{
				var latestISFDisposition = LatestISFDisposition;
				return latestISFDisposition == null ? ZString.Empty : latestISFDisposition.US_Code;
			}
		}

		public ZPropertyInfo B0_LatestISFDispositionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.B0_LatestISFDispositionCode); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_LatestISFDispositionCodeDescription", Caption = "Latest ISF Disposition Description", MediumCaption = "Latest ISF Disp. Desc.", ShortCaption = "ISF Disposition Desc.")]
		public ZString B0_LatestISFDispositionCodeDescription
		{
			get { return DispositionCodeDescriptionList.GetDescriptionFromCode(B0_LatestISFDispositionCode) ?? ZString.Empty; }
		}

		public ZPropertyInfo B0_LatestISFDispositionCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.B0_LatestISFDispositionCodeDescription); }
		}

		public DispositionData LatestISFDisposition
		{
			get
			{
				if (latestISFDispositionCached == null)
				{
					latestISFDispositionCached = new CachedProperty<DispositionData>(Factory, delegate
					{ return DispositionCodes.GetLatestDisposition(DispositionCodeListLoader.GetCachedISFCodesForAMS(Factory)); });
				}
				return latestISFDispositionCached.Value;
			}
		}
		CachedProperty<DispositionData> latestISFDispositionCached;

		#endregion

		#region Latest PTT Disposition Code

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_LatestPTTDispositionCode", Caption = "Latest PTT Disposition", MediumCaption = "Latest PTT Disp.", ShortCaption = "PTT")]
		public ZString B0_LatestPTTDispositionCode
		{
			get
			{
				var latestPTTDisposition = LatestPTTDisposition;
				return latestPTTDisposition == null ? ZString.Empty : latestPTTDisposition.US_Code;
			}
		}

		public ZPropertyInfo B0_LatestPTTDispositionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.B0_LatestPTTDispositionCode); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_LatestPTTDispositionCodeDescription", Caption = "Latest PTT Disposition Description", MediumCaption = "Latest PTT Disp. Desc.", ShortCaption = "PTT Disposition Desc.")]
		public ZString B0_LatestPTTDispositionCodeDescription
		{
			get { return DispositionCodeDescriptionList.GetDescriptionFromCode(B0_LatestPTTDispositionCode) ?? ZString.Empty; }
		}

		public ZPropertyInfo B0_LatestPTTDispositionCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.B0_LatestPTTDispositionCodeDescription); }
		}

		public DispositionData LatestPTTDisposition
		{
			get
			{
				if (latestPTTDispositionCached == null)
				{
					latestPTTDispositionCached = new CachedProperty<DispositionData>(Factory, delegate
					{ return DispositionCodes.GetLatestDisposition(DispositionCodeListLoader.GetCachedPTTCodesForAMS(Factory)); });
				}
				return latestPTTDispositionCached.Value;
			}
		}
		CachedProperty<DispositionData> latestPTTDispositionCached;

		#endregion

		public bool IsBillAlreadyOnFile
		{
			get
			{
				var moveDetail = MovementDetail;
				return moveDetail != null && moveDetail.IsBillAlreadyOnFile;
			}
		}

		public bool IsMessagingInProgress
		{
			get
			{
				var moveDetail = MovementDetail;
				return moveDetail != null && moveDetail.IsMessagingInProgress;
			}
		}

		public ZString IssuerCodeAndMasterBillNumber
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(B0_IssuerCode);
				result.AppendIfNotEmpty(B0_MasterBillNumber);
				return result.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public ZWeight Weight
		{
			get { return new ZWeight(B0_Weight, B0_WeightUQ); }
		}

		public ZVolume Volume
		{
			get { return new ZVolume(B0_Volume, B0_VolumeUQ); }
		}

		public ZBool IsNVOCCHeader
		{
			get
			{
				var header = Header;
				return header != null && header.IsNVOCCHeader;
			}
		}

		public bool HasDifferentDischargePort
		{
			get
			{
				return B0_InBondPortOfDestDCode != Header.BH_PortUnladingDCode;
			}
		}

		public ZBool IsNVOCCBill
		{
			get { return BillOfLadingStatusIndicatorList.IsNVOCC(B0_BillStatus); }
		}

		public ZBool IsISFBill
		{
			get { return BillOfLadingStatusIndicatorList.IsISF(B0_BillStatus, ZZCustomsFunctionality.IsAMSHBREffective); }
		}

		public bool IsOceanBillType
		{
			get { return B0_ShipmentType == OceanBillType; }
		}

		public const string OceanBillType = "OBT";

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|MasterBillNumber", Caption = "Master Bill Number")]
		public ZString MasterBillNumber
		{
			get
			{
				var result = ZString.Empty;

				if (IsOceanBillType)
				{
					result = B0_MasterBillNumber;
				}
				else if (Header != null && Header.OceanBill != null)
				{
					result = Header.OceanBill.B0_MasterBillNumber;
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|HouseBillNumber", Caption = "House Bill Number")]
		public ZString HouseBillNumber
		{
			get { return !IsOceanBillType ? B0_MasterBillNumber : ZString.Empty; }
		}

		#endregion

		#region Override Properties

		#region B0_BH

		[RelatedBusinessObject("Header")]
		public override ZGuid B0_BH
		{
			get { return base.B0_BH; }
			set
			{
				base.B0_BH = value;
				EnsureMoveDetailExists();
				var movementDetail = MovementDetail;
				if (movementDetail != null)
				{
					movementDetail.Containers.MarkAsNeedingValidation();
					foreach (var container in movementDetail.Containers)
					{
						container.Vehicles.MarkAsNeedingValidation();
						container.Commodities.MarkAsNeedingValidation();
					}
				}
			}
		}

		public new CusInBondHeader Header
		{
			get { return (CusInBondHeader)base.Header; }
		}

		#endregion

		[BusinessObjectTestExclude]
		public override ZInt B0_ManifestQty
		{
			get { return IsOceanBillType ? TotalManifestQtyFromAllBills : base.B0_ManifestQty; }
			set { base.B0_ManifestQty = IsOceanBillType ? base.B0_ManifestQty : value; }
		}

		ZInt TotalManifestQtyFromAllBills
		{
			get
			{
				if (totalManifestQtyFromAllBillsCached == null)
				{
					totalManifestQtyFromAllBillsCached = new CachedProperty<ZInt>(Factory, () =>
					{
						var result = ZInt.Zero;
						var header = Header;
						if (header != null)
						{
							result = header.Bills.Sum(new Func<CusInBondBill, int>(x => x.B0_ManifestQty));
						}
						return result;
					});
				}
				return totalManifestQtyFromAllBillsCached.Value;
			}
		}
		CachedProperty<ZInt> totalManifestQtyFromAllBillsCached;

		public override ZPropertyInfo B0_MasterBillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.B0_MasterBillNumber, Res.GetString("USAMSCusInBondBill|B0_MasterBillNumber", "Bill Of Lading")); }
		}

		[MeasureUnit(Schema.B0_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal B0_Weight
		{
			get { return base.B0_Weight; }
			set
			{
				var oldValue = B0_Weight;
				base.B0_Weight = value;
				if (!IsCopying && oldValue != B0_Weight && B0_WeightUQ.IsEmpty)
				{
					B0_WeightUQ = Core.Constants.Weight.Kilograms;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.WeightUnitList))]
		public override ZString B0_WeightUQ
		{
			get { return base.B0_WeightUQ; }
			set { base.B0_WeightUQ = value; }
		}

		[MeasureUnit(Schema.B0_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal B0_Volume
		{
			get { return base.B0_Volume; }
			set
			{
				var oldValue = B0_Volume;
				base.B0_Volume = value;
				if (!IsCopying && oldValue != B0_Weight && B0_VolumeUQ.IsEmpty)
				{
					B0_VolumeUQ = Core.Constants.Volume.CubicMetres;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.VolumeUnitList))]
		public override ZString B0_VolumeUQ
		{
			get { return base.B0_VolumeUQ; }
			set { base.B0_VolumeUQ = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.ManifestUnitList))]
		public override ZString B0_ManifestUQ
		{
			get { return base.B0_ManifestUQ; }
			set { base.B0_ManifestUQ = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.StatusIndicatorList))]
		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_BillStatus", Caption = "Bill Type", ShortCaption = "Bill Type")]
		public override ZString B0_BillStatus
		{
			get { return base.B0_BillStatus; }
			set
			{
				var oldValue = B0_BillStatus;
				base.B0_BillStatus = value;
				if (!IsCopying && oldValue != B0_BillStatus)
				{
					var movementDetail = MovementDetail;
					if (movementDetail != null)
					{
						movementDetail.MarkAsNeedingValidation();
						movementDetail.Containers.MarkAsNeedingValidation();
						foreach (var container in movementDetail.Containers)
						{
							container.Commodities.MarkAsNeedingValidation();
						}
					}
					SecondaryNotifyParties.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|B0_BillStatusDescription", Caption = "Bill Type Description", ShortCaption = "Bill Type Desc.")]
		public ZString B0_BillStatusDescription
		{
			get { return Lookups.StatusIndicatorList.GetDescriptionFromCode(B0_BillStatus); }
		}

		public ZPropertyInfo B0_BillStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.B0_BillStatusDescription); }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.TransportModeList))]
		public override ZString B0_TransportModeToPortOfLading
		{
			get { return base.B0_TransportModeToPortOfLading; }
			set { base.B0_TransportModeToPortOfLading = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.PaymentMethodCodes))]
		public override ZString B0_TransportPaymentMethod
		{
			get { return base.B0_TransportPaymentMethod; }
			set { base.B0_TransportPaymentMethod = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.FIRMSList))]
		public override ZString B0_Firms
		{
			get { return base.B0_Firms; }
			set { base.B0_Firms = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.SCACList))]
		public override ZString B0_IssuerSCAC
		{
			get { return base.B0_IssuerSCAC; }
			set { base.B0_IssuerSCAC = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.SCACList))]
		public override ZString B0_IssuerCode
		{
			get { return base.B0_IssuerCode; }
			set { base.B0_IssuerCode = value; }
		}

		public override ZString B0_RL_NKInBondPortOfDest
		{
			get
			{
				return GetEffectiveValueToReturn(base.B0_RL_NKInBondPortOfDest, CusInBondHeader.Schema.BH_RL_NKPortUnlading, Schema.B0_RL_NKInBondPortOfDest);
			}
			set
			{
				var oldValue = B0_RL_NKInBondPortOfDest;
				base.B0_RL_NKInBondPortOfDest = GetEffectiveValueToSet(value, CusInBondHeader.Schema.BH_RL_NKPortUnlading);
				if (!IsCopying && oldValue != B0_RL_NKInBondPortOfDest)
				{
					B0_InBondPortOfDestDCode = USScheduleResolver.GetScheduleCode(Schedule.D, B0_RL_NKInBondPortOfDest, USLocoMapSystemUsageList.Codes.Sea, Factory).Left(B0_InBondPortOfDestDCodeInfo.MaxLength);
				}
			}
		}

		public Func<ZString, ZString, ZDateTime, bool> OnPortOfLadingChangedEvent;

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.ScheduleDList))]
		public override ZString B0_InBondPortOfDestDCode
		{
			get
			{
				return GetEffectiveValueToReturn(base.B0_InBondPortOfDestDCode, CusInBondHeader.Schema.BH_PortUnladingDCode, Schema.B0_InBondPortOfDestDCode);
			}
			set
			{
				var oldValue = B0_InBondPortOfDestDCode;
				if (OnPortOfLadingChangedEvent == null || OnPortOfLadingChangedEvent(oldValue, value, B0_A_ARV))
				{
					base.B0_InBondPortOfDestDCode = GetEffectiveValueToSet(value, CusInBondHeader.Schema.BH_PortUnladingDCode);
					if (!IsCopying && oldValue != B0_InBondPortOfDestDCode)
					{
						var header = Header;
						if (header != null)
						{
							var bill = header.Bills.FirstOrDefault(p => p != this && p.B0_InBondPortOfDestDCode == B0_InBondPortOfDestDCode);
							if (bill != null && bill.B0_DateOfDischarge.IsValid)
							{
								B0_DateOfDischarge = bill.B0_DateOfDischarge;
							}
						}
					}
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZDateTime B0_A_ARV
		{
			get
			{
				var result = ZDateTime.Empty;
				var moveDetail = MovementDetail;
				if (moveDetail != null && moveDetail.IsBillAlreadyOnFile)
				{
					var matchedPortArrival = Header.PortArrivalDetails.OfType<PortArrivalDetail>().FirstOrDefault(x => x.PortCode == B0_InBondPortOfDestDCode);
					if (matchedPortArrival != null)
					{
						result = matchedPortArrival.ActualArrivalDate;
					}
				}

				return result;
			}
		}

		public override ZString B0_RL_NKPortOfLading
		{
			get { return base.B0_RL_NKPortOfLading; }
			set
			{
				var oldValue = B0_RL_NKPortOfLading;
				base.B0_RL_NKPortOfLading = value;
				if (!IsCopying && oldValue != B0_RL_NKPortOfLading)
				{
					B0_RL_NKLastForeignPort = B0_RL_NKPortOfLading;
					B0_RL_NKForeignPortOfContract = B0_RL_NKPortOfLading;
					B0_PortOfLadingKCode = USScheduleResolver.GetScheduleCode(Schedule.K, B0_RL_NKPortOfLading, USLocoMapSystemUsageList.Codes.Sea, Factory).Left(B0_PortOfLadingKCodeInfo.MaxLength);
					var portOfLading = PortOfLading;
					if (portOfLading != null)
					{
						B0_PlaceOfReceipt = portOfLading.RL_PortName.Left(B0_PlaceOfReceiptInfo.MaxLength).ToUpper();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.ScheduleKList))]
		[ReadOnlyMember(nameof(ShouldSynchronise))]
		public override ZString B0_PortOfLadingKCode
		{
			get { return base.B0_PortOfLadingKCode; }
			set
			{
				var oldValue = B0_PortOfLadingKCode;
				base.B0_PortOfLadingKCode = value;
				if (!IsCopying && oldValue != B0_PortOfLadingKCode)
				{
					if (B0_RL_NKLastForeignPort == B0_RL_NKPortOfLading)
					{
						B0_LastForeignPortKCode = B0_PortOfLadingKCode;
					}
					if (B0_RL_NKForeignPortOfContract == B0_RL_NKPortOfLading)
					{
						B0_ForeignPortOfContractKCode = B0_PortOfLadingKCode;
					}
				}
			}
		}

		public override ZString B0_RL_NKLastForeignPort
		{
			get { return base.B0_RL_NKLastForeignPort; }
			set
			{
				var oldValue = B0_RL_NKLastForeignPort;
				base.B0_RL_NKLastForeignPort = value;
				if (!IsCopying && oldValue != B0_RL_NKLastForeignPort)
				{
					B0_LastForeignPortKCode = USScheduleResolver.GetScheduleCode(Schedule.K, B0_RL_NKLastForeignPort, USLocoMapSystemUsageList.Codes.Sea, Factory).Left(B0_LastForeignPortKCodeInfo.MaxLength);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.ScheduleKList))]
		[ReadOnlyMember(nameof(ShouldSynchronise))]
		public override ZString B0_LastForeignPortKCode
		{
			get { return base.B0_LastForeignPortKCode; }
			set { base.B0_LastForeignPortKCode = value; }
		}

		public override ZString B0_RL_NKForeignPortOfContract
		{
			get { return base.B0_RL_NKForeignPortOfContract; }
			set
			{
				var oldValue = B0_RL_NKForeignPortOfContract;
				base.B0_RL_NKForeignPortOfContract = value;
				if (!IsCopying && oldValue != B0_RL_NKForeignPortOfContract)
				{
					B0_ForeignPortOfContractKCode = USScheduleResolver.GetScheduleCode(Schedule.K, B0_RL_NKForeignPortOfContract, USLocoMapSystemUsageList.Codes.Sea, Factory).Left(B0_ForeignPortOfContractKCodeInfo.MaxLength);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.ScheduleKList))]
		public override ZString B0_ForeignPortOfContractKCode
		{
			get { return base.B0_ForeignPortOfContractKCode; }
			set { base.B0_ForeignPortOfContractKCode = value; }
		}

		public override ZString B0_ShipmentType
		{
			get { return base.B0_ShipmentType; }
			set
			{
				var oldValue = B0_ShipmentType;
				base.B0_ShipmentType = value;
				if (!IsCopying && oldValue != B0_ShipmentType)
				{
					DeleteOcaenBillMoveDetailIfNeeded();
				}
			}
		}

		public override ZDate B0_DateOfDischarge
		{
			get
			{
				return GetEffectiveValueToReturn(base.B0_DateOfDischarge.ToZDateTime(), CusInBondHeader.Schema.BH_ETA, Schema.B0_DateOfDischarge).Date;
			}
			set
			{
				base.B0_DateOfDischarge = GetEffectiveValueToSet(value.ToZDateTime(), CusInBondHeader.Schema.BH_ETA).Date;
			}
		}

		// For ZDateDateEditColumnStyleInfo, it doesn't support editing a ZDate Property
		public ZDateTime B0_DateTimeOfDischarge
		{
			get
			{
				return B0_DateOfDischarge;
			}
			set
			{
				B0_DateOfDischarge = value.Date;
			}
		}

		public ZPropertyInfo B0_DateTimeOfDischargeInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(Schema.B0_DateTimeOfDischarge, x => B0_DateOfDischargeInfo);
			}
		}

		public new CusInBondBillLookups Lookups
		{
			get { return (CusInBondBillLookups)base.Lookups; }
		}

		public new CommonCusInBondBillValidation Validation
		{
			get { return (CommonCusInBondBillValidation)base.Validation; }
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (humanReadableNameCoreCached == null)
				{
					humanReadableNameCoreCached = new CachedProperty<ZString>(Factory, delegate
					{
						return "AMS Bill " + IssuerCodeAndMasterBillNumber;
					});
				}
				return humanReadableNameCoreCached.Value;
			}
		}
		CachedProperty<ZString> humanReadableNameCoreCached;

		#endregion

		#region Override Methods

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				ShipmentReferenceDetails.DeleteAll();
				DocAddresses.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		#endregion

		#region Related Objects
		[ChildEditable]
		public CusInbondBillAddRefCollection ShipmentReferenceDetails
		{
			get
			{
				if (shipmentReferenceDetails == null)
				{
					shipmentReferenceDetails = new CusInbondBillAddRefCollection(this);
					RegisterEditableChildObject(shipmentReferenceDetails);
				}
				return shipmentReferenceDetails;
			}
		}
		CusInbondBillAddRefCollection shipmentReferenceDetails;

		public new CusInBondMoveDetail MovementDetail
		{
			get { return (CusInBondMoveDetail)base.MovementDetail; }
		}

		public CusInBondMoveHeader MasterInBondMovement
		{
			get
			{
				if (masterInBondMovementCached == null)
				{
					masterInBondMovementCached = new CachedProperty<CusInBondMoveHeader>(Factory, () =>
					{
						CusInBondMoveHeader result = null;
						if (B0_MasterInBondIndicator || B0_BillStatus == BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF)
						{
							var header = Header;
							if (header != null)
							{
								result = header.InBondMovementHeaders.Cast<CusInBondMoveHeader>().OrderBy(x => x.BM_SystemCreateTimeUtc).FirstOrDefault((x) => x.BM_SubApplicationCode == SubApplicationCodeList.Codes.MasterInBond && x.MovementDetails.FirstOrDefault(y => y.B9_B0 == PK) != null);
							}
						}
						return result;
					});
				}
				return masterInBondMovementCached.Value;
			}
		}
		CachedProperty<CusInBondMoveHeader> masterInBondMovementCached;

		public JobDocAddress ForeignShipper
		{
			get
			{
				if (foreignShipper == null || foreignShipper.IsDeleted)
				{
					foreignShipper = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.ForeignShipperDocumentaryAddress));
				}

				if (foreignShipper.AdditionalValidation == null)
				{
					foreignShipper.AdditionalValidation = new ACEOceanManifestJobDocAddressValidation(foreignShipper);
				}

				return foreignShipper;
			}
		}
		JobDocAddress foreignShipper;

		public JobDocAddress Consignee
		{
			get
			{
				if (consignee == null || consignee.IsDeleted)
				{
					consignee = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.ConsigneeAddress));
				}

				if (consignee.AdditionalValidation == null)
				{
					consignee.AdditionalValidation = new ACEOceanManifestJobDocAddressValidation(consignee);
				}

				return consignee;
			}
		}
		JobDocAddress consignee;

		public JobDocAddress NotifyParty1
		{
			get
			{
				if (notifyParty1 == null || notifyParty1.IsDeleted)
				{
					notifyParty1 = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.NotifyParty));
				}

				if (notifyParty1.AdditionalValidation == null)
				{
					notifyParty1.AdditionalValidation = new ACEOceanManifestJobDocAddressValidation(notifyParty1);
				}

				return notifyParty1;
			}
		}
		JobDocAddress notifyParty1;

		public JobDocAddress NotifyParty2
		{
			get
			{
				if (notifyParty2 == null || notifyParty2.IsDeleted)
				{
					notifyParty2 = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.NotifyParty2));
				}

				if (notifyParty2.AdditionalValidation == null)
				{
					notifyParty2.AdditionalValidation = new ACEOceanManifestJobDocAddressValidation(notifyParty2);
				}

				return notifyParty2;
			}
		}
		JobDocAddress notifyParty2;

		public JobDocAddress CustomsBroker
		{
			get
			{
				if (customsBroker == null || customsBroker.IsDeleted)
				{
					customsBroker = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.ImportBroker));
				}

				return customsBroker;
			}
		}
		JobDocAddress customsBroker;

		public JobDocAddress ShipToParty
		{
			get
			{
				if (shipToParty == null || shipToParty.IsDeleted)
				{
					shipToParty = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.ShipToParty));
				}

				if (shipToParty.AdditionalValidation == null)
				{
					shipToParty.AdditionalValidation = new ACEOceanManifestJobDocAddressValidation(shipToParty);
				}

				return shipToParty;
			}
		}
		JobDocAddress shipToParty;

		public JobDocAddress BookingParty
		{
			get
			{
				if (bookingParty == null || bookingParty.IsDeleted)
				{
					bookingParty = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress));
				}

				return bookingParty;
			}
		}
		JobDocAddress bookingParty;

		[ChildEditable]
		public SecondaryNotifyPartyCollection SecondaryNotifyParties
		{
			get
			{
				if (secondaryNotifyParties == null)
				{
					secondaryNotifyParties = new SecondaryNotifyPartyCollection(this);
					RegisterEditableChildObject(secondaryNotifyParties);
				}
				return secondaryNotifyParties;
			}
		}
		SecondaryNotifyPartyCollection secondaryNotifyParties;

		[ReadOnly(true)]
		public DispositionDataCollection DispositionCodes
		{
			get
			{
				if (fDispositionCodes == null)
				{
					fDispositionCodes = new DispositionDataCollection(this);
					fDispositionCodes.Load();
				}
				return fDispositionCodes;
			}
		}
		DispositionDataCollection fDispositionCodes;

		[ChildEditable(true)]
		public AMSBillEDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					var header = Header;
					if (header != null)
					{
						if (header.IsNVOCCHeader)
						{
							fMessages = new AMSBillEDIMessageCollection(Factory, PK.ToString(), header.MovementHeader.PK);
						}
						else
						{
							fMessages = new AMSBillEDIMessageCollection(Factory, PK.ToString(), header.MessageAttachees.Select(x => x.PK));
						}

						fMessages.Load();
						fMessages.SetReadOnlyIncludingChildren(true);
						RegisterEditableChildObject(fMessages);
					}
				}
				return fMessages;
			}
		}
		AMSBillEDIMessageCollection fMessages;

		#endregion

		#region Validation Modes

		public ValidationModes ValidationModes
		{
			get
			{
				if (!fValidationModes.HasValue)
				{
					fValidationModes = ValidationModes.UseParentValidateMode;
				}

				var result = fValidationModes.Value;
				if (result == ValidationModes.UseParentValidateMode)
				{
					result = HeaderValidationModes;
				}
				return result;
			}
			set
			{
				var hasChanges = ValidationModes != value;
				var headerValidationModes = HeaderValidationModes;
				fValidationModes = value == headerValidationModes ? ValidationModes.UseParentValidateMode : value;
				if (hasChanges && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkAsNeedingValidationIncludingChildren();
				}
			}
		}
		ValidationModes? fValidationModes;

		public ValidationModes HeaderValidationModes
		{
			get
			{
				var result = ValidationModes.None;
				var header = Header;
				if (header != null)
				{
					result = header.ValidationModes;
				}
				return result;
			}
		}

		public bool IsInventoryRecordValidationMode
		{
			get
			{
				return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InventoryRecord) || IsInventoryRecordAmendmentValidationMode;
			}
		}

		public bool IsInventoryRecordAmendmentValidationMode
		{
			get
			{
				return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InventoryRecordAmendment);
			}
		}

		public bool IsPTTValidationMode
		{
			get
			{
				return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.PermitToTransfer);
			}
		}

		public bool IsChangeEstDateOfArrivalValidationMode
		{
			get
			{
				return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.ChangeEstDateOfArrival);
			}
		}

		public bool IsVesselArrivalValidationMode
		{
			get
			{
				return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.VesselArrival);
			}
		}

		public bool IsVesselDepartureValidationMode
		{
			get
			{
				return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.VesselDeparture);
			}
		}

		public bool IsSubsequentInBondValidationMode
		{
			get
			{
				return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.SubsequentInBond);
			}
		}

		public bool IsInBondArrivalValidationMode
		{
			get
			{
				return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondArrival);
			}
		}

		#endregion

		#region Implementation

		ZString GetStatusFromMovementDetails(ZString propertyName)
		{
			ZString? result = null;
			var header = Header;
			if (header != null)
			{
				var billPK = header.IsNVOCCHeader ? header.OceanBill.PK : PK;
				foreach (CusInBondMoveHeader moveHeader in header.InBondMovementHeaders)
				{
					var moveDetail = moveHeader.MovementDetails.FirstOrDefault(x => x.B9_B0 == billPK);
					if (moveDetail != null)
					{
						if (!result.HasValue)
						{
							result = moveDetail[propertyName].ToString();
						}
						else if (result.Value != moveDetail[propertyName].ToString())
						{
							result = AMSBillMessageStatusList.Codes.Multiple;
							break;
						}
					}
				}
			}
			return result ?? ZString.Empty;
		}

		void LocateAndChangeShipmentReference(ZString qualifier, ZPropertyInfo info, ZString newValue)
		{
			var oldValue = (ZString)info.Value;
			if (oldValue != newValue)
			{
				CheckMaximumLength(info, newValue);
				var query = new ZQuery(CusInbondBillAddRefSchema.BR_Qualifier, qualifier);
				query.AddToFilter(CusInbondBillAddRefSchema.BR_ReferenceNum, oldValue);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				var references = new List<CusInbondBillAddRef>(ShipmentReferenceDetails.Find(query));

				var shouldDelete = newValue.IsEmpty;
				//add one if none exist
				if (references.Count == 0)
				{
					if (!shouldDelete)
					{
						var reference = ShipmentReferenceDetails.AddNew(qualifier, newValue);
					}
				}
				else
				{
					//update all matching
					foreach (var reference in references)
					{
						if (shouldDelete)
						{
							reference.Delete();
						}
						else
						{
							reference.BR_ReferenceNum = newValue;
						}
					}
				}
				info.RefreshBinding(oldValue);
			}
		}

		protected override Type MovementDetailType
		{
			get { return typeof(CusInBondMoveDetail); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseBill;
		}

		void EnsureMoveDetailExists()
		{
			var moveDetail = MovementDetail;
		}

		void DeleteOcaenBillMoveDetailIfNeeded()
		{
			if (IsOceanBillType)
			{
				var movementDetail = Factory.LoadTop1<CusInBondMoveDetail>(MovementDetailQuery);
				if (movementDetail != null)
				{
					movementDetail.Delete();
				}
			}
			else
			{
				EnsureMoveDetailExists();
			}
		}

		protected override bool IsMovementDetailSupported
		{
			get { return !IsOceanBillType; }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override Customs.Business.CusInBondBillLookups GetNewLookups()
		{
			return new CusInBondBillLookups(this);
		}

		protected override Customs.Business.CusInBondBillValidation GetNewValidation()
		{
			if (IsOceanBillType)
			{
				return new OceanBillCusInBondBillValidation(this);
			}
			else
			{
				return new CusInBondBillValidation(this);
			}
		}

		#endregion

		#region IDocAddresses Members

		[ChildEditable]
		public AMSDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new AMSDocAddressDependentCollection(this);
					fDocAddresses.Load();
					fDocAddresses.Sort(JobDocAddress.Schema.E2_AddressSequence);
					RegisterEditableChildObject(fDocAddresses);
				}
				return fDocAddresses;
			}
		}
		internal AMSDocAddressDependentCollection fDocAddresses;

		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get { return DocAddresses; }
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ForeignShipperDocumentaryAddress:
				case DocAddressType.ConsigneeAddress:
				case DocAddressType.NotifyParty:
				case DocAddressType.NotifyParty2:
				case DocAddressType.ImportBroker:
				case DocAddressType.ShipToParty:
				case DocAddressType.BookingPartyDocumentaryAddress:
					return DocAddressRequirement.GetJobDocAddressRequirement(addressType);
			}

			return null;
		}

		public BillJobDocAddressRequirement DocAddressRequirement
		{
			get
			{
				return Factory.GetCachedValue("BillJobDocAddressRequirement", delegate
				{
					return new BillJobDocAddressRequirement();
				});
			}
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.ForeignShipperDocumentaryAddress,
					DocAddressType.ConsigneeAddress,
					DocAddressType.NotifyParty,
					DocAddressType.NotifyParty2,
					DocAddressType.ImportBroker,
					DocAddressType.ShipToParty,
					DocAddressType.BookingPartyDocumentaryAddress
				};
			}
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
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

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !ShouldSynchronise && !IsBillAlreadyOnFile; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return IsBillAlreadyOnFile ? ReasonForCannotDeleteAlreadyOnFile : ReasonForCannotDeleteStillBeingSynchronise; }
		}

		internal static MultilingualString ReasonForCannotDeleteStillBeingSynchronise
		{
			get { return ResString.GetMultilingualString("AMS|CusInBondBill|80E010A8-8896-450D-9F21-EBBDC43F8999", "Bill Of Lading values are copied from the Consol. If you want to delete this record, please do it in the Consol, or you may tick 'Override Freight Defaults'."); }
		}

		internal static MultilingualString ReasonForCannotDeleteAlreadyOnFile
		{
			get { return ResString.GetMultilingualString("AMS|CusInBondBill|29E8DAE1-9AFC-4E33-A472-C3B1BA55A0A1", "This Bill Of Lading is on Customs file.\r\nYou need to delete it from Customs file before deleting it here."); }
		}

		#endregion

		#region ISequenceNumberHeader Members

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines
		{
			get { return new TypedEnumerable<ISequenceNumberLine>(SecondaryNotifyParties); }
		}

		public IDisposable GetLineNumberRenumberingSuspender()
		{
			return SecondaryNotifyPartyOrderGenerator.GetLineNumberSuspender();
		}

		internal ShortSequenceNumberGenerator SecondaryNotifyPartyOrderGenerator
		{
			get { return secondaryNotifyPartyOrderGenerator ?? (secondaryNotifyPartyOrderGenerator = new ShortSequenceNumberGenerator(this)); }
		}
		ShortSequenceNumberGenerator secondaryNotifyPartyOrderGenerator;

		#endregion

		#region IDispositionCodeDateParent Members

		public CodeDescriptionPairList DispositionCodeDescriptionList
		{
			get { return DispositionCodeListLoader.GetDispositionCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode); }
		}

		string IDispositionCodeDateParent.GetDispositionDescriptionBasedOnSource(ZString dispositionSource, ZString code)
		{
			return "";
		}

		#endregion

		#region IDispositionCodeColumnsForFastSearchProvider Members

		SchemaColumn[] IDispositionCodeColumnsForFastSearchProvider.GetColumnsForFastSearch()
		{
			return new SchemaColumn[] { USDispositionDataAddInfoSchema.US_Code };
		}

		#endregion

		#region ISailingSynchronisationTarget Members

		bool ISailingSynchronisationTarget<BillOfLading>.IsMatched(BillOfLading sailingTarget)
		{
			return IsMatched(sailingTarget);
		}

		bool IsMatched(BillOfLading sailingBill)
		{
			var sailing = Header.Sailing;
			var billNumberAndIssuerCode = GetBillNumberAndIssuerCode(sailingBill);
			return sailing != null && this.B0_IssuerCode == billNumberAndIssuerCode.Item2 && this.B0_MasterBillNumber == billNumberAndIssuerCode.Item1;
		}

		void ISailingSynchronisationTarget<BillOfLading>.Set(BillOfLading sailingTarget)
		{
			using (this.GetValidationSuspender())
			{
				var billNumberAndIssuerCode = GetBillNumberAndIssuerCode(sailingTarget);
				this.B0_MasterBillNumber = billNumberAndIssuerCode.Item1;
				this.B0_IssuerCode = billNumberAndIssuerCode.Item2;
			}
		}

		void ISailingSynchronisationTarget<BillOfLading>.Synchronise()
		{
			var sailingBill = SailingSynchronisationSource;
			if (sailingBill != null)
			{
				using (this.GetValidationSuspender())
				{
					this.B0_RL_NKPortOfLading = sailingBill.JS_NKLoadPort;
					SailingBillImportHelper.SynchroniseJobDocAddressIfNotEmpty(this.ForeignShipper, sailingBill.ConsignorDocumentaryAddress);
					SailingBillImportHelper.SynchroniseJobDocAddressIfNotEmpty(this.Consignee, sailingBill.ConsigneeDocumentaryAddress);
					SailingBillImportHelper.SynchroniseJobDocAddressIfNotEmpty(this.NotifyParty1, sailingBill.NotifyPartyDocumentaryAddress);
					if (sailingBill.IsContainerised)
					{
						this.MovementDetail.Containers.Synchronise(sailingBill.RealContainers.Cast<BillOfLadingContainer>());
					}
					else if (sailingBill.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.RollOnRollOff && sailingBill.Vehicles.Count > 0)
					{
						var ncContainer = FindOrCreateNonContainerized();
						var vehicles = sailingBill.Vehicles.Cast<AgencyShipmentContainer>();
						ncContainer.Commodities.Synchronise<AgencyShipmentContainer, CusInBondCargoDesc>(vehicles);
						ncContainer.Vehicles.Synchronise(vehicles);
						ncContainer.UNDGs.Synchronise(vehicles.SelectMany(x => x.UNDGs));
					}
					else if (sailingBill.TopLevelPacks.Count > 0)
					{
						var ncContainer = FindOrCreateNonContainerized();
						var packs = sailingBill.TopLevelPacks.Cast<AgencyShipmentContainer>();
						ncContainer.Commodities.Synchronise<AgencyShipmentContainer, CusInBondCargoDesc>(packs);
						ncContainer.UNDGs.Synchronise(packs.SelectMany(x => x.UNDGs));
					}
					ImportQuantities(sailingBill);
				}
			}
		}

		void ImportQuantities(BillOfLading sailingBill)
		{
			if (sailingBill.IsContainerised)
			{
				var sailingPackLines = sailingBill.RealContainers.Cast<BillOfLadingContainer>().SelectMany(x => x.PackLines).Cast<BillOfLadingPackLine>();
				this.B0_WeightUQ = sailingPackLines.Select(x => x.JL_ActualWeightUQ).FirstOrDefault(x => !x.IsEmpty);
				if (!this.B0_WeightUQ.IsEmpty)
				{
					this.B0_Weight = sailingPackLines.Sum(x => new ZWeight(x.JL_ActualWeight, x.JL_ActualWeightUQ).ConvertTo(this.B0_WeightUQ));
				}
				this.B0_VolumeUQ = sailingPackLines.Select(x => x.JL_ActualVolumeUQ).FirstOrDefault(x => !x.IsEmpty);
				if (!this.B0_VolumeUQ.IsEmpty)
				{
					this.B0_Volume = sailingPackLines.Sum(x => new ZVolume(x.JL_ActualVolume, x.JL_ActualVolumeUQ).ConvertTo(this.B0_VolumeUQ));
				}
				this.B0_ManifestUQ = sailingPackLines.Select(x => x.JL_F3_NKPackType).FirstOrDefault(x => !x.IsEmpty);
				this.B0_ManifestQty = sailingPackLines.Sum(x => x.JL_PackageCount);
			}
			else
			{
				this.B0_Weight = sailingBill.JS_ActualWeight;
				this.B0_WeightUQ = sailingBill.JS_UnitOfWeight;
				this.B0_Volume = sailingBill.JS_ActualVolume;
				this.B0_VolumeUQ = sailingBill.JS_UnitOfVolume;
				this.B0_ManifestQty = sailingBill.JS_OuterPacks;
				this.B0_ManifestUQ = sailingBill.JS_F3_NKPackType;
			}
		}

		CusInBondContainer FindOrCreateNonContainerized()
		{
			var result = this.MovementDetail.Containers.FirstOrDefault(x => x.IsNonContainerized);
			if (result == null)
			{
				result = this.MovementDetail.Containers.AddNew();
				result.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			}
			return result;
		}

		IEnumerable<ZString> GetValidSCACs(BillOfLading sailingBill)
		{
			var result = new List<ZString>();
			result.AddRange(sailingBill.Principal.USLocalCustomsCarrierCodes());

			if (sailingBill.BookedShippingLine != null)
			{
				result.AddRange(sailingBill.BookedShippingLine.USLocalCustomsCarrierCodes());
			}

			return result;
		}

		Tuple<ZString, ZString> GetBillNumberAndIssuerCode(BillOfLading sailingBill)
		{
			Tuple<ZString, ZString> result = null;
			var billNumber = sailingBill.JS_HouseBill.KeepValidBillNumberCharacters();
			var validSCACs = GetValidSCACs(sailingBill);
			if (billNumber.ShouldTrimSCACFromBills(validSCACs))
			{
				result = new Tuple<ZString, ZString>(billNumber.GetBillNumberTrimSCAC(), billNumber.Left(4));
			}
			else
			{
				result = new Tuple<ZString, ZString>(billNumber, validSCACs.FirstOrDefault());
			}
			return result;
		}

		BillOfLading ISailingSynchronisationTarget<BillOfLading>.Source
		{
			get { return SailingSynchronisationSource; }
		}
		BillOfLading fSailingSynchronisationSource;

		BillOfLading SailingSynchronisationSource
		{
			get
			{
				if (fSailingSynchronisationSource != null && IsMatched(fSailingSynchronisationSource) && !fSailingSynchronisationSource.IsCancelled)
				{
					return fSailingSynchronisationSource;
				}
				fSailingSynchronisationSource = Header.BillsOfLading.FirstOrDefault(x => IsMatched(x) && !x.IsCancelled);
				return fSailingSynchronisationSource;
			}
		}

		public bool HasSailingLinkage
		{
			get
			{
				var header = Header;
				return header != null && header.HasSailingLinkage;
			}
		}

		#endregion

		#region GetEffectiveValuesFromHeader

		T GetEffectiveValueToReturn<T>(T baseValue, string fieldNameInHeader, string billFieldName) where T : IZType
		{
			var result = baseValue;
			var header = Header;

			if (result.IsEmpty && header != null && !header.IsNVOCCHeader)
			{
				var effectiveValue = GetEffectiveValue(billFieldName) ?? (IZType)header[fieldNameInHeader];
				result = (T)effectiveValue;
			}

			return result;
		}

		T GetEffectiveValueToSet<T>(T valuePassed, string fieldNameInHeader) where T : IZType
		{
			var result = valuePassed;
			var header = Header;

			if (!valuePassed.IsDefault && header != null && !header.IsNVOCCHeader && header[fieldNameInHeader].Equals(valuePassed))
			{
				result = (T)valuePassed.Default;
			}

			return result;
		}

		#endregion

		#region Suspend Effective Value

		internal IDisposable SuspendEffectiveValue(string fieldName, IZType headerValue)
		{
			EffectiveValueSuspender holder;
			if (!EffectiveValueSuspenders.TryGetValue(fieldName, out holder))
			{
				holder = new EffectiveValueSuspender(this, fieldName) { HeaderValue = headerValue };
				EffectiveValueSuspenders.Add(fieldName, holder);
			}
			return holder;
		}

		IZType GetEffectiveValue(string fieldName)
		{
			EffectiveValueSuspender holder;
			return EffectiveValueSuspenders.TryGetValue(fieldName, out holder) ? holder.HeaderValue : null;
		}

		Dictionary<string, EffectiveValueSuspender> EffectiveValueSuspenders
		{
			get { return effectiveValueSuspenders ?? (effectiveValueSuspenders = new Dictionary<string, EffectiveValueSuspender>()); }
		}
		Dictionary<string, EffectiveValueSuspender> effectiveValueSuspenders;

		class EffectiveValueSuspender : IDisposable
		{
			public EffectiveValueSuspender(CusInBondBill bill, string fieldName)
			{
				this.cusInBondBill = bill;
				this.fieldName = fieldName;
			}

			public IZType HeaderValue { get; set; }

			readonly CusInBondBill cusInBondBill;
			readonly string fieldName;

			#region IDisposable Members

			public void Dispose()
			{
				cusInBondBill.EffectiveValueSuspenders.Remove(fieldName);
			}

			#endregion
		}
		#endregion

		#region IAmsBill

		IManifestHeaderForSynchroniser IManifestBillForSynchroniser.Header
		{
			get { return Header; }
		}

		ZPropertyInfo IManifestBillForSynchroniser.LastForeignPortInfo
		{
			get { return B0_RL_NKLastForeignPortInfo; }
		}

		ZPropertyInfo IManifestBillForSynchroniser.ManifestQtyInfo
		{
			get { return B0_ManifestQtyInfo; }
		}

		ZPropertyInfo IManifestBillForSynchroniser.ManifestUQInfo
		{
			get { return B0_ManifestUQInfo; }
		}

		ZPropertyInfo IManifestBillForSynchroniser.MasterBillNumberInfo
		{
			get { return B0_MasterBillNumberInfo; }
		}

		ZPropertyInfo IManifestBillForSynchroniser.PlaceOfReceiptInfo
		{
			get { return B0_PlaceOfReceiptInfo; }
		}

		ZPropertyInfo IManifestBillForSynchroniser.PortOfLadingInfo
		{
			get { return B0_RL_NKPortOfLadingInfo; }
		}

		ZPropertyInfo IManifestBillForSynchroniser.VolumeInfo
		{
			get { return B0_VolumeInfo; }
		}

		ZPropertyInfo IManifestBillForSynchroniser.VolumeUQInfo
		{
			get { return B0_VolumeUQInfo; }
		}

		ZPropertyInfo IManifestBillForSynchroniser.WeightInfo
		{
			get { return B0_WeightInfo; }
		}

		ZPropertyInfo IManifestBillForSynchroniser.WeightUQInfo
		{
			get { return B0_WeightUQInfo; }
		}

		#endregion

		public bool IsMoveHeaderFor62or63 => Header?.HasInbondMoveHeaderEntryType62Or63 ?? false;

		public bool IsTariffRequired => BillOfLadingStatusIndicatorList.IsTariffRequired(B0_BillStatus) || IsMoveHeaderFor62or63;

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			foreach (var fetchStrategy in GetAdditionalBusinessObjectFetchStrategies())
			{
				yield return fetchStrategy;
			}
		}

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(SecondaryNotifyParty.SNPType, typeof(SecondaryNotifyParty));
			return result;
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USDisposition, ObjectFactory.GetType<Integration.Customs.US.IDispositionData>());
			return result;
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return Header?.Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var parent = Header as IWorkflowProviderCore;
				if (parent != null)
				{
					list.Add(parent);
				}
				return list;
			}
		}

		ManifestBase.IManifestBillAddress IManifestBillForSynchroniser.Consignee => Consignee;

		ManifestBase.IManifestBillAddress IManifestBillForSynchroniser.ForeignShipper => ForeignShipper;

		ManifestBase.IManifestBillAddress IManifestBillForSynchroniser.NotifyParty1 => NotifyParty1;

		#endregion

		#region ICusInbondBillAddRefTypeSupporter

		Type ICusInbondBillAddRefTypeSupporter.AddRefType
		{
			get
			{
				return typeof(CusInbondBillAddRef);
			}
		}

		#endregion
	}
}
