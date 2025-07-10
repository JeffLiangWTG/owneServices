using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DistanceCalculation.Business;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public interface IConfirmAddressParent
	{
		JobDocAddress GetConsigneeDeliveryDocAddress { get; }
		JobDocAddress GetConsignorPickupDocAddress { get; }
		JobDocAddress GetArrivalCFSDocAddress { get; }
		JobDocAddress GetDepartureCFSDocAddress { get; }
		JobDocAddress GetDepartureCTODocAddress { get; }
		JobDocAddress GetArrivalCTODocAddress { get; }
		JobDocAddress GetDepartureContainerYardDocAddress { get; }
		JobDocAddress GetArrivalContainerYardDocAddress { get; }
	}

	[CodeProperty(CommonPickupDeliveryConfirm.Schema.EU_GoodsSignForBy), DescriptionProperty(CommonPickupDeliveryConfirm.Schema.EU_GoodsSignForBy)]
	[DebuggerDisplay("PK:{PK}, SignedBy: {EU_GoodsSignForBy} Time: {EU_PickupDeliveryTime} Type: {EU_PickupDeliveryType}")]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class CommonPickupDeliveryConfirm : AutoJobPickupDeliveryConfirm,
		IDocumentSupportable,
		IDocAddresses,
		IConfirmAddressParent,
		ICreditControlledDocumentDelivery,
		IDistanceCalculationConsumer,
		IDefaultNumberOfDecimalsSupporter,
		IDocManagerSupport
	{
		public CommonPickupDeliveryConfirm(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			constructorStackTrace = System.Environment.StackTrace;
		}

		readonly string constructorStackTrace;

		public static CommonPickupDeliveryConfirm New(BusinessObjectFactory factory)
		{
			return factory.New<CommonPickupDeliveryConfirm>();
		}

		#region Schema

		public new class Schema : AutoJobPickupDeliveryConfirm.Schema
		{
			public const string IsLoose = "IsLoose";
			public const string IsContainerised = "IsContainerised";
			public const string FullGatePass = "FullGatePass";
			public const string PickupDeliveryType = "PickupDeliveryType";
			public const string UniqueID = "UniqueID";

			public const string TotalBookedPackages = "TotalBookedPackages";
			public const string TotalBookedWeight = "TotalBookedWeight";
			public const string TotalBookedVolume = "TotalBookedVolume";

			public const string TotalDeliveredPackages = "TotalDeliveredPackages";
			public const string TotalDeliveredWeight = "TotalDeliveredWeight";
			public const string TotalDeliveredVolume = "TotalDeliveredVolume";

			public const string TotalPackagesUnit = "TotalPackagesUnit";
			public const string TotalWeightUnit = "TotalWeightUnit";
			public const string TotalVolumeUnit = "TotalVolumeUnit";

			public const string PostcodeDistance = "PostcodeDistance";
		}

		#endregion

		#region Overrides

		public override void Delete()
		{
			foreach (var divot in Factory.Load<CommonConfirmDivot>(new ZQuery(JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, PK)))
			{
				divot.Delete();
			}

			base.Delete();
		}

		protected override void DeleteForDataRefresh()
		{
			foreach (var divot in Factory.Load<CommonConfirmDivot>(new ZQuery(JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, PK) { FetchOnlyFromLocalCache = true }))
			{
				if (!divot.IsInDatabase)
				{
					((IBusiness)divot).DeleteForDataRefresh();
				}
			}

			base.DeleteForDataRefresh();
		}

		public override bool ReadOnly
		{
			get
			{
				return
					base.ReadOnly ||
					IsDeleted ||
					!FullGatePass.IsEmpty ||
					(Globals.IsWeb && !EU_PickupDeliveryTime.IsEmpty && !EU_GoodsSignForBy.IsEmpty);
			}
			set { base.ReadOnly = value; }
		}

		ZStringBuilder OnSavingLog
		{
			get
			{
				if (onSavingLog == null)
				{
					onSavingLog = new ZStringBuilder();
				}

				return onSavingLog;
			}
		}
		ZStringBuilder onSavingLog;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CommonPickupDeliveryConfirmFetchStrategy(this);
		}

		public override void OnSaving()
		{
			#region SuppressResourceStringsCheckRegion

			if (!HasPreExistingDivotMismatch && EU_JS.IsValid)
			{
				OnSavingLog.AppendLine("OnSaving - Logging Start");

				OnSavingLog.AppendLine(FormattableString.Invariant($"Confirm from current factory: {HumanReadableName}|{PK}|{IsInDatabase}|{HasChanges}|{IsSavedByFactory}"));

				OnSavingLog.AppendLine("Shipments:");
				foreach (var shipment in Shipments)
				{
					OnSavingLog.AppendLine(FormattableString.Invariant($"Shipment from current factory: {shipment.HumanReadableName}|{shipment.PK}|{shipment.IsInDatabase}|{shipment.HasChanges}|{shipment.IsSavedByFactory}"));

					OnSavingLog.AppendLine(FormattableString.Invariant($"PackingLines - Shipment {shipment.PK}:"));
					foreach (PackLine packLine in shipment.OuterPackLines)
					{
						OnSavingLog.AppendLine(FormattableString.Invariant($"Packline from current factory: {packLine.HumanReadableName}|{packLine.PK}|{packLine.IsInDatabase}|{packLine.HasChanges}|{packLine.IsSavedByFactory}"));
					}
				}

				OnSavingLog.AppendLine("Divots:");
				foreach (var divot in Divots)
				{
					OnSavingLog.AppendLine(FormattableString.Invariant($"Divot from current factory: {divot.HumanReadableName}|{divot.PK}|{divot.IsInDatabase}|{divot.HasChanges}|{divot.IsSavedByFactory}"));
				}

				OnSavingLog.AppendLine("OnSaving - Logging Finish");
			}

			#endregion

			base.OnSaving();
			SetUniqueID();

			if (DatesHaveChanges)
			{
				foreach (CommonShipment shipment in Shipments)
				{
					if (shipment != null)
					{
						ConfirmTimesSyncHelper.SetShipmentTimesFromConfirmIfRequired(shipment, this);
					}
				}
			}
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;

				if (((INeedRow)this).Row.RowState != DataRowState.Detached && DatesHaveChanges && FirstShipment != null)
				{
					FirstShipment.DocsAndCartage.MarkAsNeedingValidation();
				}
			}
		}

		bool DatesHaveChanges
		{
			get
			{
				return !IsDeleted && (!IsInDatabase || (EU_PickupDeliveryTime != (ZDateTime)EU_PickupDeliveryTimeInfo.OriginalValue ||
					EU_RequestedPickupDeliveryTime != (ZDateTime)EU_RequestedPickupDeliveryTimeInfo.OriginalValue ||
					EU_PlannedPickupDeliveryTime != (ZDateTime)EU_PlannedPickupDeliveryTimeInfo.OriginalValue));
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EU_DistanceUnit = DistanceCalculationRegistry.Instance.DefaultDistanceUnit.Value;
		}

		public override bool IsSavedByFactory
		{
			get
			{
				if (!IsInDatabase
					&& !IsDeleted
					&& IsEmpty
					&& Divots.All(divot => divot.IsEmpty))
				{
					return false;
				}

				return base.IsSavedByFactory;
			}
		}

		#endregion

		#region Override Properties

		#region EU_DropMode

		[List("Lookups.DropModes")]
		public override ZString EU_DropMode
		{
			[DebuggerStepThrough()]
			get { return base.EU_DropMode; }
			[DebuggerStepThrough()]
			set { base.EU_DropMode = value; }
		}

		#endregion

		#region EU_OA_TransportProvider

		[List("BindToLists.TransportProviders")]
		public override ZGuid EU_OA_TransportProvider
		{
			get { return base.EU_OA_TransportProvider; }
			set
			{
				if (base.EU_OA_TransportProvider != value)
				{
					EU_TransportCoName = "";
				}
				base.EU_OA_TransportProvider = value;

				MarkContainerAsNeedingValidation();
			}
		}

		protected bool EU_OA_TransportProvider_ReadOnly
		{
			get { return ConsolidatedTransportBooking != null; }
		}

		protected override ZAddress GetNewEU_OA_TransportProvider_ZAddress()
		{
			ZAddress result = base.GetNewEU_OA_TransportProvider_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		public OrgHeader TransportCo
		{
			get { return TransportProvider != null ? TransportProvider.Header : null; }
		}

		#endregion

		#region EU_TransportCoName

		public override ZString EU_TransportCoName
		{
			get { return TransportCo != null ? TransportCo.OH_FullNameTruncated : base.EU_TransportCoName; }
			[DebuggerStepThrough()]
			set { base.EU_TransportCoName = value; }
		}

		protected bool EU_TransportCoName_ReadOnly
		{
			get { return TransportCo != null || ConsolidatedTransportBooking != null; }
		}

		public ZGuid TransportCoPK
		{
			get { return TransportCo != null ? TransportCo.PK : ZGuid.Empty; }
			set { EU_OA_TransportProvider_ZAddress.OrgPK = value; }
		}

		#endregion

		#region EU_PickupDeliveryTime

		public override ZDateTime EU_PickupDeliveryTime
		{
			get { return new ZDateTime(base.EU_PickupDeliveryTime, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime originalValue = EU_PickupDeliveryTime;
				base.EU_PickupDeliveryTime = value;

				MarkContainerAsNeedingValidation();

				if (Container != null)
				{
					if (EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationDelivery)
					{
						ConfirmTimesSyncHelper.SetContainerTimes(Container, ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual, originalValue, value);
					}
					else if (EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginPickup)
					{
						ConfirmTimesSyncHelper.SetContainerTimes(Container, ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual, originalValue, value);
					}
				}
			}
		}

		#endregion

		#region EU_PlannedPickupDeliveryTime

		public override ZDateTime EU_PlannedPickupDeliveryTime
		{
			get { return new ZDateTime(base.EU_PlannedPickupDeliveryTime, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime originalValue = EU_PlannedPickupDeliveryTime;
				base.EU_PlannedPickupDeliveryTime = value;

				MarkContainerAsNeedingValidation();

				if (Container != null)
				{
					if (EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationDelivery)
					{
						ConfirmTimesSyncHelper.SetContainerTimes(Container, ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned, originalValue, value);
					}
					else if (EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginPickup)
					{
						ConfirmTimesSyncHelper.SetContainerTimes(Container, ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned, originalValue, value);
					}
				}
			}
		}

		#endregion

		#region EU_RequestedPickupDeliveryTime

		public override ZDateTime EU_RequestedPickupDeliveryTime
		{
			get { return new ZDateTime(base.EU_RequestedPickupDeliveryTime, DateTimeKind.Unspecified); }
			set { base.EU_RequestedPickupDeliveryTime = value; }
		}

		#endregion

		#region EU_VehicleRegistration

		public override ZString EU_VehicleRegistration
		{
			get { return base.EU_VehicleRegistration; }
			set
			{
				base.EU_VehicleRegistration = value;

				ZQuery vehicleRegoFilter = new ZQuery(JobPickupDeliveryConfirmSchema.EU_VehicleRegistration, value);
				vehicleRegoFilter.OrderBy = JobPickupDeliveryConfirmSchema.Constants.EU_PickupDeliveryTime + " DESC";

				CommonPickupDeliveryConfirm lastConfirmWithVehicle = Factory.LoadTop1<CommonPickupDeliveryConfirm>(vehicleRegoFilter);

				if (lastConfirmWithVehicle != null)
				{
					try
					{
						((IBusinessObjectInternals)this).IsCopying = true;
						EU_OA_TransportProvider = lastConfirmWithVehicle.EU_OA_TransportProvider;
						EU_DriversName = lastConfirmWithVehicle.EU_DriversName;
						EU_DriversLicence = lastConfirmWithVehicle.EU_DriversLicence;
					}
					finally
					{
						((IBusinessObjectInternals)this).IsCopying = false;
					}
				}

				MarkContainerAsNeedingValidation();
			}
		}

		#endregion

		#region EU_PickupDeliveryType

		public override ZString EU_PickupDeliveryType
		{
			get { return base.EU_PickupDeliveryType; }
			set
			{
				base.EU_PickupDeliveryType = value;
				MarkContainerAsNeedingValidation();

				if (value.IsEmpty)
				{
					DebugLog.AppendLine(FormattableString.Invariant($"CommonPickupDeliveryConfirm::EU_PickupDeliveryType setter, value is empty."));
					DebugLog.AppendLine(System.Environment.StackTrace);
				}
			}
		}

		#endregion

		#region EU_DriversLicence

		public override ZString EU_DriversLicence
		{
			get { return base.EU_DriversLicence; }
			set
			{
				base.EU_DriversLicence = value;

				MarkContainerAsNeedingValidation();
			}
		}

		#endregion

		#region EU_D1

		[RelatedBusinessObject("ConsolidatedTransportBooking")]
		[List("Lookups.ConsolidatedTransportBookings")]
		public override ZGuid EU_D1
		{
			get { return base.EU_D1; }
			set
			{
				base.EU_D1 = value;
				SetConsolidatedTransportBookingProvider();
			}
		}

		public void SetConsolidatedTransportBookingProvider()
		{
			ZGuid bookingTransportProviderPK = ConsolidatedTransportBooking != null ? ConsolidatedTransportBooking.D1_OA_TransportCo : ZGuid.Empty;

			if (bookingTransportProviderPK.IsEmpty)
			{
				TransportCoPK = ZGuid.Empty;
			}
			else
			{
				EU_OA_TransportProvider = bookingTransportProviderPK;
			}
		}

		#endregion

		#region EU_JC

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("Container")]
		public override ZGuid EU_JC
		{
			get { return base.EU_JC; }
			set
			{
				if (base.EU_JC != value)
				{
					base.EU_JC = value;
				}
			}
		}

		#endregion

		#region EU_Distance

		[MeasureUnit(Schema.EU_DistanceUnit, MeasureUnitType.Length)]
		public override ZDecimal EU_Distance
		{
			get { return base.EU_Distance; }
			set { base.EU_Distance = value; }
		}

		#endregion

		#region EU_DistanceUnit

		[List("BindToLists.DistanceUnits")]
		public override ZString EU_DistanceUnit
		{
			get { return base.EU_DistanceUnit; }
			set
			{
				base.EU_DistanceUnit = value;
				PostcodeDistanceInfo.RefreshBinding();
			}
		}

		#endregion

		#endregion

		#region New Properties

		#region PickupDeliveryType

		[MaxLength(30)]
		public ZString PickupDeliveryType
		{
			get { return BindToLists.PickupDeliveryConfirmTypes.GetDescriptionFromCode(EU_PickupDeliveryType); }
		}

		public ZPropertyInfo PickupDeliveryTypeInfo
		{
			get { return GetZPropertyInfo(Schema.PickupDeliveryType); }
		}

		#endregion

		#region UniqueID

		[MaxLength(3)]
		public ZString UniqueID
		{
			get
			{
				return Base26NumberConverter.GetLetterRepresentation((int)EU_GatePassCount);
			}
			set
			{
				CheckMaximumLength(UniqueIDInfo, value);
				base.EU_GatePassCount = ZShort.ParseSafe(Base26NumberConverter.GetNumberRepresentation(value.ToUpper()).ToString(), ZShort.Zero);
				UniqueIDInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UniqueIDInfo
		{
			get { return GetZPropertyInfo(Schema.UniqueID); }
		}

		protected bool UniqueID_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Related Types

		public Type PackLineType
		{
			get { return packLineType ?? typeof(PackLine); }
			set { packLineType = value; }
		}
		Type packLineType;

		#endregion

		#region IsLoose

		public ZBool IsLoose
		{
			get { return Divots.Count != 0 && Divots[0].PackLine != null; }
		}

		#endregion

		#region IsContainerised

		public ZBool IsContainerised
		{
			get { return Container != null; }
		}

		#endregion

		#region IsOrigin

		public ZBool IsOrigin
		{
			get
			{
				return EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginPickup ||
					EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture ||
					EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginCFSArrival;
			}
		}

		#endregion

		#region IsDestination

		public ZBool IsDestination
		{
			get
			{
				return EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture ||
					EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationDelivery ||
					EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival;
			}
		}

		#endregion

		#region IsCFSArrivalOrDeparture

		public bool IsCFSArrivalOrDeparture
		{
			get
			{
				return EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginCFSArrival
					|| EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture
					|| EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival
					|| EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture;
			}
		}

		#endregion

		#region IsLoadedInCFSContext

		public bool IsLoadedInCFSContext
		{
			get { return FirstShipment is CFS.ICFSShipment || Container is CFS.ICFSContainer; }
		}

		#endregion

		#region IsGatePass

		public ZBool IsGatePass
		{
			get { return IsCFSWithPackageableDivots(Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture); }
		}

		#endregion

		#region IsCFSConfirmWithPackageableDivots

		bool IsCFSWithPackageableDivots(string confirmType)
		{
			bool result = EU_PickupDeliveryType == confirmType
				&& !IsContainerised
				&& Shipments.Any(shipment => shipment.JS_IsCFSRegistered && shipment.JS_TranshipToOtherCFS);

			return result;
		}

		#endregion

		#region TotalPackagesUnit

		[MaxLength(AutoJobPackLines.Schema.JL_F3_NKPackTypeMaxLength)]
		public ZString TotalPackagesUnit
		{
			get
			{
				ZString result = "";

				foreach (IGoods goods in Goods)
				{
					if (result.IsEmpty)
					{
						result = goods.PackagesUnit;
					}
					else if (goods.PackagesUnit != result)
					{
						result = Core.Constants.PkgUnit.Package;
						break;
					}
				}
				return result.IsEmpty ? Core.Constants.PkgUnit.Package : result.ToString();
			}
		}

		public ZPropertyInfo TotalPackagesUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalPackagesUnit); }
		}

		#endregion

		#region TotalVolumeUnit

		ZString MasterVolumeUnit
		{
			get
			{
				return IsLoose && FirstShipment != null ? FirstShipment.ShipmentVolumeUnit.ToString() : Env.Registry.FreightVolumeUnit;
			}
		}

		public ZString TotalVolumeUnit
		{
			get
			{
				ZString result = "";

				foreach (IGoods goods in Goods)
				{
					if (result.IsEmpty)
					{
						result = goods.VolumeUnit;
					}
					else if (goods.VolumeUnit != result)
					{
						result = MasterVolumeUnit;
						break;
					}
				}

				return result.IsEmpty ? Env.Registry.FreightVolumeUnit : result.ToString();
			}
		}

		public ZPropertyInfo TotalVolumeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalVolumeUnit); }
		}

		#endregion

		#region IsEmpty

		public bool IsEmpty
		{
			get
			{
				return EU_DropMode.IsEmpty
					&& EU_RequestedPickupDeliveryTime.IsEmpty
					&& EU_PlannedPickupDeliveryTime.IsEmpty
					&& EU_PickupDeliveryTime.IsEmpty
					&& EU_GoodsSignForBy.IsEmpty
					&& EU_TransportCoName.IsEmpty
					&& EU_DriversLicence.IsEmpty
					&& EU_DriversName.IsEmpty
					&& EU_VehicleRegistration.IsEmpty
					&& EU_PickupDeliveryInstruction.IsEmpty
					&& EU_Distance.IsEmpty
					&& EU_OA_TransportProvider.IsEmpty;
			}
		}

		#endregion

		#region TotalWeightUnit

		ZString MasterWeightUnit
		{
			get
			{
				return IsLoose && FirstShipment != null ? FirstShipment.ShipmentWeightUnit.ToString() : Env.Registry.FreightWeightUnit;
			}
		}

		public ZString TotalWeightUnit
		{
			get
			{
				ZString result = "";

				foreach (IGoods goods in Goods)
				{
					if (result.IsEmpty)
					{
						result = goods.WeightUnit;
					}
					else if (goods.WeightUnit != result)
					{
						result = MasterWeightUnit;
						break;
					}
				}

				return result.IsEmpty ? Env.Registry.FreightWeightUnit : result.ToString();
			}
		}

		public ZPropertyInfo TotalWeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalWeightUnit); }
		}

		#endregion

		#region TotalBookedPackages

		public ZInt TotalBookedPackages
		{
			get
			{
				ZInt result = 0;

				foreach (IGoods goods in Goods)
				{
					result += goods.BookedPackages;
				}

				return result;
			}
		}

		public ZPropertyInfo TotalBookedPackagesInfo
		{
			get { return GetZPropertyInfo(Schema.TotalBookedPackages); }
		}

		#endregion

		#region TotalBookedWeight

		public ZDecimal TotalBookedWeight
		{
			get
			{
				ZDecimal result = 0;

				if (IsContainerised)
				{
					result = Container.JC_GrossWeight;
				}
				else
				{
					foreach (IGoods goods in Goods)
					{
						result += goods.BookedWeight;
					}
				}

				result = this.GetRoundedValue(TotalBookedWeightInfo, result);

				return result;
			}
		}

		public ZPropertyInfo TotalBookedWeightInfo
		{
			get { return GetZPropertyInfo(Schema.TotalBookedWeight); }
		}

		#endregion

		#region TotalBookedVolume

		public ZDecimal TotalBookedVolume
		{
			get
			{
				ZDecimal result = 0;

				foreach (IGoods goods in Goods)
				{
					result += goods.BookedVolume;
				}

				return this.GetRoundedValue(TotalBookedVolumeInfo, result);
			}
		}

		public ZPropertyInfo TotalBookedVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalBookedVolume); }
		}

		#endregion

		#region TotalDeliveredPackages

		[BusinessObjectTestExclude()]
		public ZInt TotalDeliveredPackages
		{
			get
			{
				ZInt result = 0;

				foreach (IGoods goods in Goods)
				{
					result += goods.DeliveredPackages;
				}

				return result;
			}
			set
			{
				if (TotalDeliveredPackages != value)
				{
					//Clear Pivot Values to Ensure we can get an accurate measure of remainder
					foreach (CommonConfirmDivot divot in Divots)
					{
						divot.J8_PackagesDelivered = 0;
					}

					ZInt leftOver = value;
					for (int i = 0; i < Divots.Count; i++)
					{
						if (!Divots[0].J8_JL.IsEmpty)
						{
							bool finalDivot = (i == Divots.Count - 1);
							CommonConfirmDivot divot = Divots[i];

							var packagesConfirmedRemaining = PackagesConfirmedRemaining(divot.PackLine);
							ZInt packsRemainding = packagesConfirmedRemaining > 0 ? packagesConfirmedRemaining : (ZInt)0;
							divot.J8_PackagesDelivered = leftOver > packsRemainding && !finalDivot ? packsRemainding : leftOver;
							leftOver -= divot.J8_PackagesDelivered;
						}
					}

					TotalDeliveredPackagesInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TotalDeliveredPackagesInfo
		{
			get { return GetZPropertyInfo(Schema.TotalDeliveredPackages); }
		}

		#endregion

		#region TotalDeliveredWeight

		[BusinessObjectTestExclude()]
		public ZDecimal TotalDeliveredWeight
		{
			get
			{
				ZDecimal result = 0;

				if (IsContainerised)
				{
					result = Container.JC_GrossWeight;
				}
				else
				{
					foreach (IGoods goods in Goods)
					{
						ZString wUnit = goods.WeightUnit;
						result += Constants.Weight.Convert(goods.DeliveredWeight, wUnit, TotalWeightUnit, false);
					}
				}

				result = this.GetRoundedValue(TotalDeliveredWeightInfo, result);

				return result;
			}
			set
			{
				if (TotalDeliveredWeight != value)
				{
					//Clear Pivot Values to Ensure we can get an accurate measure of remainder
					foreach (CommonConfirmDivot divot in Divots)
					{
						divot.J8_DeliveryWeight = 0;
					}

					decimal leftOverInMasterWeightUnit = value;
					for (int i = 0; i < Divots.Count; i++)
					{
						if (!Divots[0].J8_JL.IsEmpty)
						{
							bool finalDivot = (i == Divots.Count - 1);
							CommonConfirmDivot divot = Divots[i];
							decimal weightRemaining = ConfirmedWeightRemaining(divot.PackLine);
							decimal divotBookedWeightInMasterUnit = Constants.Weight.Convert(weightRemaining, divot.PackLine.JL_ActualWeightUQ, TotalWeightUnit, false);
							decimal weightInMasterUnit = leftOverInMasterWeightUnit > divotBookedWeightInMasterUnit && !finalDivot ? divotBookedWeightInMasterUnit : leftOverInMasterWeightUnit;
							divot.J8_DeliveryWeight = Constants.Weight.Convert(weightInMasterUnit, TotalWeightUnit, divot.PackLine.JL_ActualWeightUQ, false);
							decimal divotDeliveryWeightInMasterUnit = Constants.Weight.Convert(divot.J8_DeliveryWeight, divot.PackLine.JL_ActualWeightUQ, TotalWeightUnit, false);
							leftOverInMasterWeightUnit -= divotDeliveryWeightInMasterUnit;
						}
					}

					TotalDeliveredWeightInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TotalDeliveredWeightInfo
		{
			get { return GetZPropertyInfo(Schema.TotalDeliveredWeight); }
		}

		#endregion

		#region TotalDeliveredVolume

		[BusinessObjectTestExclude()]
		public ZDecimal TotalDeliveredVolume
		{
			get
			{
				ZDecimal result = 0;

				foreach (IGoods goods in Goods)
				{
					ZString vUnit = goods.VolumeUnit;
					result += Constants.Volume.Convert(goods.DeliveredVolume, vUnit, TotalVolumeUnit, false);
				}

				result = this.GetRoundedValue(TotalDeliveredVolumeInfo, result);

				return result;
			}
			set
			{
				if (TotalDeliveredVolume != value)
				{
					//Clear Pivot Values to Ensure we can get an accurate measure of remainder
					foreach (CommonConfirmDivot divot in Divots)
					{
						divot.J8_DeliveryVolume = 0;
					}

					decimal leftOverInMasterVolumeUnit = value;
					for (int i = 0; i < Divots.Count; i++)
					{
						if (!Divots[0].J8_JL.IsEmpty)
						{
							bool finalDivot = (i == Divots.Count - 1);
							CommonConfirmDivot divot = Divots[i];
							decimal volumeRemaining = ConfirmedVolumeRemaining(divot.PackLine);
							decimal divotBookedVolumeInMasterUnit = Constants.Volume.Convert(volumeRemaining, divot.PackLine.JL_ActualVolumeUQ, TotalVolumeUnit, false);
							decimal volumeInMasterUnit = leftOverInMasterVolumeUnit > divotBookedVolumeInMasterUnit && !finalDivot ? divotBookedVolumeInMasterUnit : leftOverInMasterVolumeUnit;
							divot.J8_DeliveryVolume = Constants.Volume.Convert(volumeInMasterUnit, TotalVolumeUnit, divot.PackLine.JL_ActualVolumeUQ, false);
							decimal divotDeliveryVolumeInMasterUnit = Constants.Volume.Convert(divot.J8_DeliveryVolume, divot.PackLine.JL_ActualVolumeUQ, TotalVolumeUnit, false);
							leftOverInMasterVolumeUnit -= divotDeliveryVolumeInMasterUnit;
						}
					}

					TotalDeliveredVolumeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TotalDeliveredVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalDeliveredVolume); }
		}

		#endregion

		#region PostcodeDistance

		[ResourceStringData("PickupDeliveryConfirm|PostcodeDistance", ShortCaption = "P/C Distance", Caption = "Postcode Distance", FullDescription = "The distance between Pickup and Delivery Postcodes.")]
		[DecimalPlaces(3)]
		public ZDecimal PostcodeDistance
		{
			get
			{
				ZDecimal result = 0;

				if (!EU_PickupDeliveryType.IsEmpty)
				{
					result = new ZDecimal(RefLatLongPostcode.CalculateDistance(PickupFrom, DeliverTo));
					if (EU_DistanceUnit != Constants.Length.Kilometres && Constants.Length.ContainsCode(EU_DistanceUnit))
					{
						result = ZArchitecture.Core.Utilities.Round(Constants.Length.Convert(result, Constants.Length.Kilometres, EU_DistanceUnit), JobBookedCtgMoveSchema.EW_Distance.Scale);
					}
				}

				return result;
			}
		}

		public ZPropertyInfo PostcodeDistanceInfo
		{
			get { return GetZPropertyInfo(Schema.PostcodeDistance); }
		}

		#endregion

		#endregion

		#region WebReadOnlyCalculation

		protected bool EU_PickupDeliveryTime_ReadOnly
		{
			get { return Globals.IsWeb && (!DivotsAreReadOnlyOnWeb || !EU_PickupDeliveryTime.IsEmpty); }
		}

		protected bool EU_GoodsSignForBy_ReadOnly
		{
			get { return Globals.IsWeb && (!DivotsAreReadOnlyOnWeb || !EU_GoodsSignForBy.IsEmpty); }
		}

		protected bool EU_RequestedPickupDeliveryTime_ReadOnly
		{
			get { return Globals.IsWeb && DivotsAreReadOnlyOnWeb; }
		}

		protected bool EU_DropMode_ReadOnly
		{
			get { return Globals.IsWeb && DivotsAreReadOnlyOnWeb; }
		}

		protected bool EU_PickupDeliveryInstruction_ReadOnly
		{
			get { return Globals.IsWeb && DivotsAreReadOnlyOnWeb; }
		}

		public bool DivotsAreReadOnlyOnWeb
		{
			get { return (IsInDatabase && !EU_TransportCoName.IsEmpty && !EU_PlannedPickupDeliveryTime.IsEmpty) || ReadOnly; }
		}

		#endregion

		#region Related BusinessObjects

		#region Divots

		[ChildEditable()]
		public CommonPickupDeliveryConfirmDivotCollection Divots
		{
			get
			{
				if (divots == null)
				{
					divots = new CommonPickupDeliveryConfirmDivotCollection(this);
					RegisterEditableChildObject(divots);
					divots.CollectionCountChange += divots_CollectionCountChange;
				}
				return divots;
			}
		}

		CommonPickupDeliveryConfirmDivotCollection divots;

		bool HasPreExistingDivotMismatch
		{
			get
			{
				if (hasPreExistingDivotMismatch == null)
				{
					hasPreExistingDivotMismatch = IsInDatabase
						&& FirstShipment != null
						&& FirstShipment.OuterPackLines.Count != Divots.Count;
				}

				return hasPreExistingDivotMismatch.Value;
			}
		}
		bool? hasPreExistingDivotMismatch;

		void divots_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemRemoved)
			{
				var divot = e.BizObject as CommonConfirmDivot;
				if (divot != null && !divot.IsDeleted && divot.PackLine != null)
				{
					DivotRemovalLogs.Add(new DivotRemovalLog(divot.J8_JL, System.Environment.StackTrace));
				}
			}
		}

		public List<DivotRemovalLog> DivotRemovalLogs => divotRemovalLogs ?? (divotRemovalLogs = new List<DivotRemovalLog>());
		List<DivotRemovalLog> divotRemovalLogs;

		public class DivotRemovalLog
		{
			public DivotRemovalLog(ZGuid packLinePK, string stackTrace)
			{
				PackLinePK = packLinePK;
				StackTrace = stackTrace;
			}

			public ZGuid PackLinePK { get; }
			public string StackTrace { get; }
		}

		public CommonConfirmDivot GetDivot(PackLine line)
		{
			foreach (CommonConfirmDivot element in Divots.Find(new ZQuery(JobTransportLegPackLineDivotSchema.J8_JL, line.PK)))
			{
				return element;
			}
			return null;
		}

		#endregion

		#region Shipment

		public CommonShipment FirstShipment
		{
			get
			{
				if (firstShipment == null || !Shipments.Contains(firstShipment))
				{
					return Shipments.Count > 0 ? Shipments[0] : null;
				}

				return firstShipment;
			}
			set
			{
				firstShipment = (value != null && Shipments.Contains(value)) ? value : null;
			}
		}
		CommonShipment firstShipment;

		public List<CommonShipment> Shipments
		{
			get
			{
				var result = new List<CommonShipment>();

				var container = Container;
				if (container == null)
				{
					if (EU_JS.IsValid)
					{
						var shipment = Factory.Load<CommonShipment>(EU_JS);
						if (shipment != null)
						{
							result.Add(shipment);
						}
					}
				}
				else
				{
					foreach (var packLine in container.PackLines.Cast<PackLine>())
					{
						if (packLine.Shipment != null && !result.Contains(packLine.Shipment))
						{
							result.Add(packLine.Shipment);
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region Container

		public CommonContainer Container
		{
			get
			{
				return EU_JC.IsEmpty ? null : Factory.Load<CommonContainer>(EU_JC);
			}
		}

		public ZGuid ContainerPK
		{
			get { return EU_JC; }
		}

		public ZPropertyInfo ContainerPKInfo
		{
			get { return GetZPropertyInfo(nameof(ContainerPK)); }
		}

		#endregion

		#region PackLines

		IGoodsCollection Goods
		{
			get { return IsContainerised ? Container.PackLines : Divots; }
		}

		#endregion

		#region ConsolidatedTransportBooking

		public CommonConsolidatedTransportBooking ConsolidatedTransportBooking
		{
			get { return Factory.Load<CommonConsolidatedTransportBooking>(EU_D1); }
		}

		#endregion

		#endregion

		#region GetNewLookups

		public new CommonPickupDeliveryConfirmLookups Lookups
		{
			get { return lookups ?? (lookups = (CommonPickupDeliveryConfirmLookups)GetNewLookups()); }
		}
		CommonPickupDeliveryConfirmLookups lookups;

		protected override JobPickupDeliveryConfirmLookups GetNewLookups()
		{
			return new CommonPickupDeliveryConfirmLookups(this);
		}

		#endregion

		#region GetNewValidation

		protected override JobPickupDeliveryConfirmValidation GetNewValidation()
		{
			return IsConfirmHiddenOnAllShipments ? base.GetNewValidation() : new CommonPickupDeliveryConfirmValidation(this);
		}

		void MarkContainerAsNeedingValidation()
		{
			if (Container != null && !IsConfirmHiddenOnAllShipments)
			{
				Container.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region Does Pickup/Delivery confirm show on shipment

		internal bool IsConfirmHiddenOnAllShipments
		{
			get
			{
				if (IsCFSArrivalOrDeparture && !IsLoadedInCFSContext)
				{
					return true;
				}

				if (EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginPickup
					|| EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationDelivery)
				{
					return Shipments.All(s => !IsContainerisedMatchesShipmentPackingMode(s));
				}

				return false;
			}
		}

		/// <summary>
		/// This only applies to OriginPickup and DestinationDelivery type confirms for the Forwarding Module. 
		/// These confirms are deleted when IsContainerised is changed and saved 
		/// </summary>
		internal bool IsContainerisedMatchesShipmentPackingMode(CommonShipment shipment)
		{
			bool result = false;

			if (EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.OriginPickup)
			{
				result = IsContainerised == shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Pickup);
			}
			else if (EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationDelivery)
			{
				result = IsContainerised == shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery);
			}

			return result;
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return GetNewDocumentSupporter(); }
		}

		protected virtual CommonPickupDeliveryConfirmDocumentSupporter GetNewDocumentSupporter()
		{
			return new CommonPickupDeliveryConfirmDocumentSupporter(this);
		}

		#endregion

		#region CFS functionality

		#region FullGatePass

		[MaxLength(CommonShipment.Schema.JS_UniqueConsignRefMaxLength + 2)]
		public ZString FullGatePass
		{
			get
			{
				ZString result = "";

				if (IsGatePass && EU_GatePassCount > 0)
				{
					ZString shipmentID = (FirstShipment != null) ? FirstShipment.JS_UniqueConsignRef : ZString.Empty;
					result = shipmentID + GatePassIDSeparator + UniqueID;
				}

				return result;
			}
		}
		public const char GatePassIDSeparator = '/';

		public ZPropertyInfo FullGatePassInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.FullGatePass);
			}
		}

		#endregion

		#endregion

		#region SetUniqueID

		public void SetUniqueID()
		{
			if (EU_GatePassCount.IsEmpty)
			{
				EU_GatePassCount = GetNextUniqueID();

				if (IsGatePass && FirstShipment != null)
				{
					FirstShipment.Logs.AddNew(Events.GatePassPrinted, FullGatePass);
				}
			}
		}

		/// <summary>
		/// Gets Next Unique ID.
		/// If Containerized, it will get a Unique ID on the Containers Consol
		/// If Loose, it will get a Unique ID on the PackLines Shipment
		/// Otherise it will return blank
		/// </summary>
		/// <returns></returns>
		public ZShort GetNextUniqueID()
		{
			ZShort suffix = 0;

			if (IsContainerised && Container.Consol != null)
			{
				foreach (CommonContainer container in Container.Consol.Containers)
				{
					foreach (CommonPickupDeliveryConfirm confirm in container.Confirms)
					{
						if (PK != confirm.PK && confirm.EU_GatePassCount > suffix)
						{
							suffix = confirm.EU_GatePassCount;
						}
					}
				}
				suffix++;
			}
			else if (IsLoose)
			{
				foreach (CommonShipment shipment in Shipments)
				{
					List<CommonPickupDeliveryConfirm> confirms = new List<CommonPickupDeliveryConfirm>();
					confirms.AddRange(shipment.PickupConfirms);
					confirms.AddRange(shipment.DeliveryConfirms);
					confirms.AddRange(shipment.OriginCFSArrivals);
					confirms.AddRange(shipment.OriginCFSDepartures);
					confirms.AddRange(shipment.DestinationCFSArrivals);
					confirms.AddRange(shipment.DestinationCFSDepartures);

					foreach (CommonPickupDeliveryConfirm confirm in confirms)
					{
						if (PK != confirm.PK && confirm.EU_GatePassCount > suffix)
						{
							suffix = confirm.EU_GatePassCount;
						}
					}
				}
				suffix++;
			}

			return suffix;
		}

		#endregion

		#region CreateDivot

		public void CreateDivot(PackLine packLine)
		{
			CreateDivot(packLine, true);
		}

		public void CreateDivot(PackLine packLine, bool allocatePackages)
		{
			using (new SemaphoreManager(Divots.DebugLogSuspender))
			{
				var divot = Divots.FirstOrDefault(d => d.J8_JL == packLine.PK);
				if (divot == null)
				{
					divot = Divots.AddNew();
					divot.J8_JL = packLine.PK;

					//Note: WI00743739 removed debug logging to resolve performance issues in CS01605710.
				}

				if (allocatePackages)
				{
					var packagesConfirmedRemaining = PackagesConfirmedRemaining(packLine);
					divot.J8_PackagesDelivered = packagesConfirmedRemaining > 0 ? packagesConfirmedRemaining : (ZInt)0;
				}
			}
		}

		public ZInt PackagesConfirmedRemaining(PackLine packLine)
		{
			int packsInStock = packLine.UseOutturn ? packLine.JL_Outturn : packLine.JL_PackageCount;
			int confirmed = PackagesConfirmed(packLine);

			return packsInStock - confirmed;
		}

		public ZDecimal ConfirmedWeightRemaining(PackLine packLine)
		{
			ZDecimal weightInStock = packLine.UseOutturn ? packLine.JL_OutturnedWeight : packLine.JL_ActualWeight;
			ZDecimal confirmed = ConfirmedWeight(packLine);
			return weightInStock - confirmed;
		}

		public ZDecimal ConfirmedVolumeRemaining(PackLine packLine)
		{
			ZDecimal volumeInStock = packLine.UseOutturn ? packLine.JL_OutturnedVolume : packLine.JL_ActualVolume;
			ZDecimal confirmed = ConfirmedVolume(packLine);
			return volumeInStock - confirmed;
		}

		public ZInt PackagesConfirmed(PackLine packLine)
		{
			switch (EU_PickupDeliveryType)
			{
				case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					return packLine.PackagesConfirmed_PickedupFromConsignor;
				case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					return packLine.PackagesConfirmed_DeliveredToConsignee;
				case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
					return packLine.PackagesConfirmed_DeliveredToOriginCFS;
				case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
					return packLine.PackagesConfirmed_DispatchedFromOriginCFS;
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
					return packLine.PackagesConfirmed_DeliveredToDestinationCFS;
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
					return packLine.PackagesConfirmed_DispatchedFromDestinationCFS;
				case "":
					return ZInt.Zero;
				default:
					throw new NotSupportedException(EU_PickupDeliveryType + " Pickup/Delivery Confirm Type is not supported.");
			}
		}

		public ZDecimal ConfirmedWeight(PackLine packLine)
		{
			return packLine.ConfirmedWeight(EU_PickupDeliveryType);
		}

		public ZDecimal ConfirmedVolume(PackLine packLine)
		{
			return packLine.ConfirmedVolume(EU_PickupDeliveryType);
		}

		#endregion

		#region ConfirmAddress

		#region deliver to / pick up

		public JobDocAddress DeliverTo
		{
			get
			{
				switch (EU_PickupDeliveryType)
				{
					case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
					case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
						return GetDeliveryAddressToSynchFromParent();
					case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
					case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
						return ConfirmAddress;
					case "":
						return null;
					default:
						throw new NotSupportedException("PickupDeliveryType '" + EU_PickupDeliveryType + "' is not supported");
				}
			}
		}

		public JobDocAddress PickupFrom
		{
			get
			{
				switch (EU_PickupDeliveryType)
				{
					case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
					case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
						return ConfirmAddress;
					case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
					case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
						return GetPickupAddressToSynchFromParent();
					case "":
						return null;
					default:
						throw new NotSupportedException("PickupDeliveryType '" + EU_PickupDeliveryType + "' is not supported");
				}
			}
		}

		public JobDocAddress ContainerYard
		{
			get
			{
				switch (EU_PickupDeliveryType)
				{
					case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
					case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
						return GetDepartureContainerYardDocAddress;
					case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
					case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
						return GetArrivalContainerYardDocAddress;
					case "":
						return null;
					default:
						throw new NotSupportedException("PickupDeliveryType '" + EU_PickupDeliveryType + "' is not supported");
				}
			}
		}

		#endregion

		public JobDocAddress ConfirmAddress
		{
			get
			{
				if (fConfirmAddress == null || fConfirmAddress.IsDeleted)
				{
					fConfirmAddress = DocAddresses.FindOrCreateWithDocAddressType(GetDocAddressType());
					if (fConfirmAddress.DocAddressType != DocAddressType.NonPersistent)
					{
						using (SuspendSettingHasChanges())
						{
							JobDocAddress parentJobDocAddress = GetAddressToSynchFromParent();
							if (parentJobDocAddress != null && (!fConfirmAddress.IsInDatabase || fConfirmAddress.IsTheSameDocAddressAndContactAs(parentJobDocAddress)))
							{
								using (fConfirmAddress.SuspendSettingHasChanges())
								{
									fConfirmAddress.ReadOnly = true;
									SyncAddressWithParent();
								}
							}
							else
							{
								fConfirmAddress.DeSynchroniseWithParent();
							}
						}
						fConfirmAddress.MakePersistentEvenIfEmpty();
					}
				}

				return fConfirmAddress;
			}
		}

		public JobDocAddress ExistingConfirmAddressFallbackToParent
			=> DocAddresses.FindByDocAddressType(GetDocAddressType()) ?? GetAddressToSynchFromParent();

		void SyncAddressWithParent()
		{
			JobDocAddress parentAddress = GetAddressToSynchFromParent();
			if (parentAddress != null)
			{
				fConfirmAddress.SynchroniseWithParent(parentAddress);
			}
		}

		JobDocAddress GetAddressToSynchFromParent()
		{
			switch (EU_PickupDeliveryType)
			{
				case Constants.PickupDeliveryConfirmTypes.OriginPickup:
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
				case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
					return GetPickupAddressToSynchFromParent();
				case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
				case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
					return GetDeliveryAddressToSynchFromParent();
				case "":
					return null;
				default:
					throw new NotSupportedException("PickupDeliveryType '" + EU_PickupDeliveryType + "' is not supported");
			}
		}

		JobDocAddress GetPickupAddressToSynchFromParent()
		{
			switch (EU_PickupDeliveryType)
			{
				case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					return GetConsignorPickupDocAddress;
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
				case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
					return GetDepartureCFSDocAddress;

				case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					return IsLoose ? GetArrivalCFSDocAddress : GetArrivalCTODocAddress; //rename to GetDestination..

				case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
					return IsLoose ? GetConsignorPickupDocAddress : GetDepartureContainerYardDocAddress; //rename to GetOrigin..

				case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
					return GetArrivalCTODocAddress; //rename to GetDestination..

				case "":
					return null;

				default:
					throw new NotSupportedException("PickupDeliveryType '" + EU_PickupDeliveryType + "' is not supported");
			}
		}

		JobDocAddress GetDeliveryAddressToSynchFromParent()
		{
			switch (EU_PickupDeliveryType)
			{
				case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					return GetConsigneeDeliveryDocAddress;
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
				case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
					return GetArrivalCFSDocAddress;

				case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					return IsLoose ? GetDepartureCFSDocAddress : GetDepartureCTODocAddress;

				case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
					return IsLoose ? GetConsigneeDeliveryDocAddress : GetArrivalContainerYardDocAddress; //Container coukld have stopped at cfs on it's way to cnr ?

				case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
					return GetDepartureCTODocAddress;

				case "":
					return null;

				default:
					throw new NotSupportedException("PickupDeliveryType '" + EU_PickupDeliveryType + "' is not supported");
			}
		}

		public OrgHeader Consignee
		{
			get { return FirstShipment == null ? null : FirstShipment.Consignee; }
		}

		public OrgHeader Consignor
		{
			get { return FirstShipment == null ? null : FirstShipment.Consignor; }
		}

		public JobDocAddress ConsigneeDocAddress
		{
			get { return FirstShipment == null ? null : FirstShipment.ConsigneeDocumentaryAddress; }
		}

		public JobDocAddress ConsignorDocAddress
		{
			get { return FirstShipment == null ? null : FirstShipment.ConsignorDocumentaryAddress; }
		}

		DocAddressType GetDocAddressType()
		{
			switch (EU_PickupDeliveryType)
			{
				case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					return DocAddressType.ConsignorPickupDeliveryAddress;

				case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					return DocAddressType.ConsigneePickupDeliveryAddress;

				case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
				case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
					return DocAddressType.ArrivalCFSAddress;

				case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
				case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
					return DocAddressType.DepartureCFSAddress;

				case "":
					return DocAddressType.NonPersistent;

				default:
					throw new NotSupportedException("PickupDeliveryType '" + EU_PickupDeliveryType + "' is not supported");
			}
		}

		JobDocAddress fConfirmAddress;

		public ZBool ConfirmAddressOverride
		{
			get
			{
				return !ConfirmAddress.ReadOnly;
			}
			set
			{
				if (value)
				{
					ConfirmAddress.ReadOnly = false;
					ConfirmAddress.DeSynchroniseWithParent();
				}
				else
				{
					ConfirmAddress.ReadOnly = true;
					SyncAddressWithParent();
				}
			}
		}

		public ZBool IsConfirmAddressSameAsParentAddress
		{
			get
			{
				var address = DocAddresses.FindByDocAddressType(GetDocAddressType());
				if (address == null)
				{
					return true;
				}

				var parentJobDocAddress = GetAddressToSynchFromParent();
				return parentJobDocAddress != null && address.IsTheSameDocAddressAndContactAs(parentJobDocAddress);
			}
		}

		#endregion

		#region IDocAddresses Members

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

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
			return Env.Security.MaintainShipment;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return Array.Empty<DocAddressType>(); }
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

		#region Implementaion

		public BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#region Constants

		public static class ConfirmFilterConstants
		{
			public static string All
			{
				get { return Res.GetString("4681b60b-dcef-473f-86d6-3d3ec5d7b3a5", "All"); }
			}
			public static string Complete
			{
				get { return Res.GetString("abff3bbc-1552-4363-b9cb-bab072853ecb", "Complete"); }
			}
			public static string Incomplete
			{
				get { return Res.GetString("b86b1232-4b09-4397-85bd-98e801a68672", "Incomplete"); }
			}
			public static string Allocated
			{
				get { return Res.GetString("c27f7b60-0b82-441e-b3b3-014509f238ee", "Allocated"); }
			}
			public static string Unallocated
			{
				get { return Res.GetString("e15e1b94-ec29-4eaa-8963-2deec221b25d", "Unallocated"); }
			}
		}

		#endregion

		#endregion

		#region IConfirmAddressParent Members

		#region AddressParent

		IConfirmAddressParent AddressParent_ContainerPriority
		{
			get
			{
				return Container != null ? Container : FirstShipment;
			}
		}

		IConfirmAddressParent AddressParent_ShipmentPriority
		{
			get
			{
				return FirstShipment != null ? FirstShipment : Container;
			}
		}

		#endregion

		public JobDocAddress GetConsigneeDeliveryDocAddress
		{
			get { return AddressParent_ShipmentPriority != null ? AddressParent_ShipmentPriority.GetConsigneeDeliveryDocAddress : null; }
		}

		public JobDocAddress GetConsignorPickupDocAddress
		{
			get { return AddressParent_ShipmentPriority != null ? AddressParent_ShipmentPriority.GetConsignorPickupDocAddress : null; }
		}

		public JobDocAddress GetArrivalCFSDocAddress
		{
			get { return AddressParent_ShipmentPriority != null ? AddressParent_ShipmentPriority.GetArrivalCFSDocAddress : null; }
		}

		public JobDocAddress GetDepartureCFSDocAddress
		{
			get { return AddressParent_ShipmentPriority != null ? AddressParent_ShipmentPriority.GetDepartureCFSDocAddress : null; }
		}

		public JobDocAddress GetDepartureCTODocAddress
		{
			get { return AddressParent_ShipmentPriority != null ? AddressParent_ShipmentPriority.GetDepartureCTODocAddress : null; }
		}

		public JobDocAddress GetArrivalCTODocAddress
		{
			get { return AddressParent_ShipmentPriority != null ? AddressParent_ShipmentPriority.GetArrivalCTODocAddress : null; }
		}

		public JobDocAddress GetDepartureContainerYardDocAddress
		{
			get { return AddressParent_ContainerPriority != null ? AddressParent_ContainerPriority.GetDepartureContainerYardDocAddress : null; }
		}

		public JobDocAddress GetArrivalContainerYardDocAddress
		{
			get { return AddressParent_ContainerPriority != null ? AddressParent_ContainerPriority.GetArrivalContainerYardDocAddress : null; }
		}

		#endregion

		#region IRelatedJobNumber Members

		string[] IRelatedJobNumber.JobNumber
		{
			get
			{
				if (FirstShipment != null)
				{
					return Array.ConvertAll<CommonShipment, string>(Shipments.ToArray(), (c) => c.JS_UniqueConsignRef);
				}
				else
				{
					return Array.Empty<string>();
				}
			}
		}

		#endregion

		#region ICreditControlledDocumentDelivery Members

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add
			{
				fGetDocumentLogin += value;
			}
			remove
			{
				fGetDocumentLogin -= value;
			}
		}
		event EventHandler<SecurityLoginEventArgs> fGetDocumentLogin;

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback
		{
			get;
			set;
		}

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get { return false; }
		}

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => false;

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties()
		{
			return Array.Empty<ScreeningParty>();
		}

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return Res.GetString("0471E399-4540-4616-A410-852EDE3CCDAF", "Consignee or Consignor"); }
		}

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get
			{
				var list = new List<OrgHeader>();
				if (Consignor != null)
				{
					list.Add(Consignor);
				}
				if (Consignee != null)
				{
					list.Add(Consignee);
				}
				return list.ToArray();
			}
		}

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			if (fGetDocumentLogin != null)
			{
				fGetDocumentLogin(this, e);
			}
		}

		#endregion

		#region IDistanceCalculationConsumer Members

		SecurityCheckpoint IDistanceCalculationConsumer.Checkpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		ZDecimal IDistanceCalculationConsumer.Distance
		{
			get { return EU_Distance; }
			set { EU_Distance = value; }
		}

		ZString IDistanceCalculationConsumer.DistanceUnit
		{
			get { return EU_DistanceUnit; }
			set { EU_DistanceUnit = value; }
		}

		DistanceCalculationConfiguration IDistanceCalculationConsumer.DistanceCalculationConfig
		{
			get
			{
				DistanceCalculationConfiguration result = new DistanceCalculationConfiguration();

				if (IsOrigin)
				{
					result = DistanceCalculationHelper.GetConfigurationFromJobDocAddress(ConfirmAddress, Consignor);
				}
				else if (IsDestination)
				{
					result = DistanceCalculationHelper.GetConfigurationFromJobDocAddress(ConfirmAddress, Consignee);
				}

				return result;
			}
		}

		DistanceCalculationAddress IDistanceCalculationConsumer.OriginAddress
		{
			get { return DistanceCalculationHelper.GetAddressFromAddress(PickupFrom); }
		}

		DistanceCalculationAddress IDistanceCalculationConsumer.DestinationAddress
		{
			get { return DistanceCalculationHelper.GetAddressFromAddress(DeliverTo); }
		}

		public void SetCalculatedDistance(INotifications notifications)
		{
			new FreightDistanceCalculator(this, notifications).SetCalculatedDistance();
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				IDefaultNumberOfDecimalsSupporter parent = null;

				if (Container != null)
				{
					parent = Container;
				}
				else if (FirstShipment != null)
				{
					parent = FirstShipment;
				}

				var result = (parent != null) ? parent.TransportMode : ZString.Empty;

				return result;
			}
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.TotalBookedWeight:
					unitOfMeasure = (IsContainerised)
						? Container.JC_GrossWeightUQ
						: GetGoodsWeightUnit();
					break;

				case Schema.TotalBookedVolume:
					unitOfMeasure = GetGoodsVolumeUnit();
					break;

				case Schema.TotalDeliveredWeight:
					unitOfMeasure = (IsContainerised)
						? Container.JC_GrossWeightUQ
						: TotalWeightUnit;
					break;

				case Schema.TotalDeliveredVolume:
					unitOfMeasure = TotalVolumeUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZString GetGoodsWeightUnit()
		{
			foreach (IGoods goods in Goods)
			{
				return goods.WeightUnit;
			}

			return ZString.Empty;
		}

		ZString GetGoodsVolumeUnit()
		{
			foreach (IGoods goods in Goods)
			{
				return goods.VolumeUnit;
			}

			return ZString.Empty;
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			if (divots != null)
			{
				foreach (CommonConfirmDivot divot in divots)
				{
					IDefaultNumberOfDecimalsSupporter decimalsSupporter = divot;
					decimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged();
				}
			}
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.PickupDeliveryConfirm)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new CommonPickupDeliveryConfirmUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#endregion

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded
				&& !HasPreExistingDivotMismatch
				&& EU_JS.IsValid)
			{
				string sql = @"
SELECT 
	(SELECT COUNT(*) FROM dbo.JobPackLines WHERE JL_JS = EU_JS AND JL_FreightMode = 'OUT') AS PackCount,
	(SELECT COUNT(*) FROM dbo.JobTransportLegPackLineDivot WHERE J8_EU_PickupDeliverConfirm = EU_PK) AS DivotsCount
FROM dbo.JobPickupDeliveryConfirm
WHERE EU_PK = @ConfirmPK
";

				var collection = new DynamicBusinessObjectCollection(Factory);
				collection.Load(sql, new[]
				{
					ZSqlParameter.New("@ConfirmPK", PK.ToGuid(), JobPickupDeliveryConfirmSchema.PK)
				});

				var dynamicBO = collection.FirstOrDefault();
				if (dynamicBO != null
					&& (ZInt)dynamicBO["PackCount"] != (ZInt)dynamicBO["DivotsCount"])
				{
					var newFactory = new BusinessObjectFactory();
					var reloadedShipment = newFactory.Load<CommonShipment>(EU_JS);
					var reloadedConfirm = newFactory.Load<CommonPickupDeliveryConfirm>(PK);

					if (reloadedShipment != null && reloadedConfirm != null
						&& !constructorStackTrace.Contains((NoResString)"at CargoWise.UniversalCopy.CopyManager.Copy(Object source, CopyTemplateTree copyTemplate)")) // When old existed copy template has included confirms, we will disable this report if the confirm is created by CopyManager copy.
					{
						#region SuppressResourceStringsCheckRegion

						var packCount = (ZInt)dynamicBO["PackCount"];
						var divotsCount = (ZInt)dynamicBO["DivotsCount"];

						var log = new ZStringBuilder();
						log.AppendLine(FormattableString.Invariant($@"Reloaded Confirm: '{reloadedConfirm.PK}', EU_JS: '{reloadedConfirm.EU_JS}', Type: {reloadedConfirm.EU_PickupDeliveryType}, EU_JC: {reloadedConfirm.EU_JC}"));
						log.AppendLine(FormattableString.Invariant($"Shipment PackLine count: {reloadedShipment.OuterPackLines.Count} ({packCount}) differs from Divots count: {reloadedConfirm.Divots.Count} ({divotsCount})"));
						log.AppendLine(FormattableString.Invariant($"Shipment: '{reloadedShipment.PK}', Created: {reloadedShipment.JS_SystemCreateTimeUtc}, JS_ShipmentType: {reloadedShipment.JS_ShipmentType}, JS_TransportMode: {reloadedShipment.JS_TransportMode}, JS_PackingMode: {reloadedShipment.JS_PackingMode}"));

						log.AppendLine("PackLines:");
						foreach (PackLine packLine in reloadedShipment.OuterPackLines)
						{
							log.AppendLine(FormattableString.Invariant($" - '{packLine.PK}', Packs: '{packLine.JL_PackageCount}'"));
						}

						log.AppendLine("Divots:");
						foreach (CommonConfirmDivot divot in reloadedConfirm.Divots)
						{
							log.AppendLine(FormattableString.Invariant($" - '{divot.PK}', J8_JL: '{divot.J8_JL}', J8_PackagesDelivered: '{divot.J8_PackagesDelivered}'"));
						}

						log.AppendLine("Divot removals:");
						foreach (var divotRemovalLog in DivotRemovalLogs)
						{
							log.AppendLine(FormattableString.Invariant($" - PackLine: '{divotRemovalLog.PackLinePK}' removed at: {divotRemovalLog.StackTrace}"));
						}

						log.AppendLine(DebugLog.ToString());

						log.AppendLine(FormattableString.Invariant($"Confirm from current factory: {PK}|{IsInDatabase}|{IsDeleted}|{EU_JS}|{EU_DropMode}|{EU_RequestedPickupDeliveryTime}|{EU_PlannedPickupDeliveryTime}|{EU_PickupDeliveryTime}|{EU_GoodsSignForBy}|{EU_TransportCoName}|{EU_DriversLicence}|{EU_DriversName}|{EU_VehicleRegistration}|{EU_PickupDeliveryInstruction}|{EU_Distance}|{EU_OA_TransportProvider}|{EU_JC}|{EU_PickupDeliveryType}"));

						if (FirstShipment != null)
						{
							foreach (PackLine packLine in FirstShipment.OuterPackLines)
							{
								log.AppendLine(FormattableString.Invariant($"Packline from current factory: {packLine.PK}|{packLine.IsInDatabase}|{packLine.IsDeleted}|{packLine.JL_PackageCount}"));
							}
						}

						foreach (CommonConfirmDivot divot in Divots)
						{
							log.AppendLine(FormattableString.Invariant($"Divot from current factory: {divot.PK}|{divot.IsInDatabase}|{divot.IsDeleted}|{divot.J8_JL}|{divot.J8_EU_PickupDeliverConfirm}|{divot.J8_PackagesDelivered}|{divot.J8_DeliveryWeight}|{divot.J8_DeliveryVolume}"));
						}

						log.Append(OnSavingLog);

						log.AppendLine("StackTrace start:");
						log.AppendLine(constructorStackTrace);
						log.AppendLine("StackTrace finish.");

						ErrorReporter.ReportOnce("CommonPickupDeliveryConfirm.OnSaved: Packline / Divot consistency problem", log.ToString());

						#endregion
					}
				}
			}
		}

		internal string GetConfirmConcurrencyDebugLog(bool showStackTrace = false)
		{
			var log = new ZStringBuilder();

			#region SuppressResourceStringsCheckRegion

			log.AppendLine(FormattableString.Invariant($"Confirm from current factory: {PK}|{IsInDatabase}|{IsDeleted}|{EU_JS}|{EU_DropMode}|{EU_RequestedPickupDeliveryTime}|{EU_PlannedPickupDeliveryTime}|{EU_PickupDeliveryTime}|{EU_GoodsSignForBy}|{EU_TransportCoName}|{EU_DriversLicence}|{EU_DriversName}|{EU_VehicleRegistration}|{EU_PickupDeliveryInstruction}|{EU_Distance}|{EU_OA_TransportProvider}|{EU_JC}|{EU_PickupDeliveryType}"));
			if (showStackTrace)
			{
				log.AppendLine("Constructor StackTrace start:");
				log.AppendLine(constructorStackTrace);
				log.AppendLine("Constructor StackTrace finish.");
			}

			if (FirstShipment != null)
			{
				foreach (PackLine packLine in FirstShipment.OuterPackLines)
				{
					log.AppendLine(FormattableString.Invariant($"PackLine from current factory: {packLine.PK}|{packLine.IsInDatabase}|{packLine.IsDeleted}|{packLine.JL_PackageCount}"));
				}
			}

			var newFactory = new BusinessObjectFactory();
			var reloadedShipment = newFactory.Load<CommonShipment>(EU_JS);

			log.AppendLine("PackLines:");
			if (reloadedShipment != null)
			{
				foreach (PackLine packLine in reloadedShipment.OuterPackLines)
				{
					log.AppendLine(FormattableString.Invariant($" - '{packLine.PK}', Packs: '{packLine.JL_PackageCount}'"));
				}
			}

			#endregion

			return log.ToString();
		}

		internal ZStringBuilder DebugLog => debugLog ?? (debugLog = new ZStringBuilder());
		ZStringBuilder debugLog;
	}
}
