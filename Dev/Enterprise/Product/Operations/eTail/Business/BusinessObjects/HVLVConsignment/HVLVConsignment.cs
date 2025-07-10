using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.AWB;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.NumberFountain;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using static Enterprise.Integration.Customs;
using static Enterprise.Registry.Business.CustomsReferenceNumberType;
using DtbBookingDirection = Enterprise.TransportCommon.Shared.DtbBookingDirection;
using EventReferenceConstants = CargoWise.EventReference.Constants;

namespace Enterprise.eTail.Business
{
	[CodeProperty(Schema.HVC_ConsignmentId)]
	[UniversalDataContext(DataContextType.HVLVConsignment)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class HVLVConsignment : AutoHVLVConsignment,
		IDocumentSupportable,
		IJobNumber,
		IWorkflowProvider,
		IEDocsProvider,
		ICusCodeDataTypeSupporter,
		IDtbBookingParent,
		IRelatedJob,
		ICanDelete,
		IHVLVConsignmentFHLMessageDetailsProvider,
		IHVLVConsignmentForDocument,
		IUniversalXMLNoteParent,
		IHVLVSecurityFilingSource,
		IScreeningPartyProvider,
		IDpsEntityProvider,
		IAuditParent,
		IHVLVISFBillInfoProvider,
		IHVLVPrescreeningDataProvider,
		IConsignmentAddressProvider
	{
		public HVLVConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!IsInDatabase)
			{
				RegisterTransactionParticipant();
				BookingHeader?.MarkForDisplayOrderCalculation();
			}

			SetConcurrencyPolicyOnTable(ConcurrencyPolicy.Ignore);
			factory.SetBulkCopyOnTable(HVLVConsignmentSchema.Constants.TableName, batchSize: HVLVDataRegistry.Instance.HVLVDataBulkCopyBatchSize.Value, fireTriggers: true);
		}

		internal void MarkForReloadItemsFromLocalCache() => markForReloadItemsFromLocalCache = true;
		bool markForReloadItemsFromLocalCache;

		#region ICanDelete

		bool ICanDelete.CanDelete => !IsInDatabase;

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get
			{
				return ((ICanDelete)this).CanDelete ? base.ReasonForNotAbleToDelete : ResString.GetMultilingualString("cb151186-c71d-48fb-886a-cd27086cc9f0", "Existing consignments cannot be deleted. Mark them as inactive instead.");
			}
		}

		#endregion

		#region ManagingShipment

		public ForwardingShipment ManagingShipment
		{
			get => managingShipment;
			set
			{
				Argument.NotNull(value, nameof(value));

				managingShipment = value;

				var readOnly = HVC_JS_ManifestedOnShipment != managingShipment.PK;

				if (readOnly)
				{
					ReadOnly = true;
					AddRowWarning(ConsignmentNotManifestedOnManagingShipmentWarning);
				}

				SetItemReadOnlyness();
			}
		}

		ForwardingShipment managingShipment;

		string ConsignmentNotManifestedOnManagingShipmentWarning => Res.GetString("35dc1eb1-874c-41d2-b3fa-b62ea7d921a6",
			"{0} was not manifested against {1}.",
			HumanReadableName,
			ManagingShipment.HumanReadableName);

		#endregion

		#region Shipment/Consol Properties

		ForwardingConsol Consol => DirectionOfTrade == Directions.Export ?
			ManifestedOnShipment?.DepartureConsol :
			ManifestedOnShipment?.ArrivalConsol;

		Transport ConsolTransportDetails => DirectionOfTrade == Directions.Export ?
			Consol?.Transports?.DepartureTransport :
			Consol?.Transports?.ArrivalTransport;

		public ZBool TransportModeIsSea => ShipmentTransportMode == TransportModes.Sea;

		public ZBool TransportModeIsAir => ShipmentTransportMode == TransportModes.Air;

		[List("Lookups.ShipmentTransportMode_List")]
		public ZString ShipmentTransportMode => ManifestedOnShipment?.JS_TransportMode ?? string.Empty;

		[List("Lookups.ShipmentPackingMode_List")]
		public ZString ShipmentPackingMode => ManifestedOnShipment?.JS_PackingMode ?? string.Empty;

		[List("Lookups.ConsolOrigin_List")]
		public ZString ConsolOrigin => Consol?.JK_RL_NKLoadPort ?? string.Empty;

		[List("Lookups.ConsolDestination_List")]
		public ZString ConsolDestination => Consol?.JK_RL_NKDischargePort ?? string.Empty;

		public ZDateTime ConsolETD => ConsolTransportDetails?.JW_ETD ?? ZDateTime.Empty;

		public ZDateTime ConsolETA => ConsolTransportDetails?.JW_ETA ?? ZDateTime.Empty;

		public ZString ConsolMasterBill => Consol?.JK_MasterBillNum ?? string.Empty;

		public ZString ConsolVoyageFlight => ConsolTransportDetails?.JW_VoyageFlight ?? string.Empty;

		public ZString ConsolVessel => ConsolTransportDetails?.JW_Vessel ?? string.Empty;

		#endregion

		#region BookingHeader

		public HVLVBookingHeader BookingHeader => Factory.Load<HVLVBookingHeader>(HVC_HVH_BookingHeader);

		[RelatedBusinessObject("BookingHeader")]
		public override ZGuid HVC_HVH_BookingHeader
		{
			get { return base.HVC_HVH_BookingHeader; }
			set
			{
				var oldValue = HVC_HVH_BookingHeader;
				base.HVC_HVH_BookingHeader = value;

				if (BookingHeader is HVLVBookingHeader bookingHeader)
				{
					ReportUnusualBookingHeaderAssignmentErrorIfNeeded(oldValue);
					HVC_ClusterKey = bookingHeader.HVH_ClusterKey;
					HVC_Status = BookingHeader.HVH_IsBookingConfirmed ? HVLVConsignmentStatus.Codes.Confirmed : HVLVConsignmentStatus.Codes.Booked;
				}

				if (!IsCopying && oldValue != HVC_HVH_BookingHeader)
				{
					Items.Cast<HVLVItem>().SelectMany(x => x.Lines.Cast<HVLVItemLine>()).ForEach(x => x.RefreshPartSyncManagerActiveDeciderPK());
				}
			}
		}

		void ReportUnusualBookingHeaderAssignmentErrorIfNeeded(ZGuid previousBookingHeaderPK)
		{
			if (!Globals.IsTest && !HVC_ClusterKey.IsEmpty)
			{
				if (ConsignmentHeader != null)
				{
					ErrorReporter.ReportOnce(
						"HVLVConsignment|AssignBookingHeaderWhenHavingConsignmentHeaderError",
						"Should not assign a bookingHeader to a consignment after it's allocated onto a shipment.");
				}

				if (!previousBookingHeaderPK.IsEmpty)
				{
					ErrorReporter.ReportOnce(
						"HVLVConsignment|ChangeBookingHeaderError",
						"Should not change bookingHeader for a consignment.");
				}
			}
		}

		#endregion

		#region Items

		[ChildEditable]
		public HVLVItemCollection Items
		{
			get
			{
				if (items == null)
				{
					items = new HVLVItemCollection(this);
					items.Load();
					RegisterEditableChildObject(items);
					SetItemReadOnlyness();
				}

				if (markForReloadItemsFromLocalCache)
				{
					items.ReloadFromLocalCache();
					markForReloadItemsFromLocalCache = false;
				}

				return items;
			}
		}

		HVLVItemCollection items;

		public IEnumerable<HVLVItem> ActiveItems => Items.OfType<HVLVItem>().Where(item => item.HVI_IsActive);

		void SetItemReadOnlyness()
		{
			if (items != null && ManagingShipment != null)
			{
				foreach (var item in items.Cast<HVLVItem>().Where(x => x.HVI_JS_LoadedOnShipment != ManagingShipment.PK))
				{
					item.ReadOnly = true;
					item.AddRowWarning(ItemAttachedToOtherShipmentWarning(item));
				}
			}
		}

		string ItemAttachedToOtherShipmentWarning(HVLVItem item) => Res.GetString("4320533b-66ca-4c21-b8b1-1a0132121e39",
			"{0} belongs to {1}, but is not attached to {2}.",
			item.HumanReadableName,
			HumanReadableName,
			ManagingShipment.HumanReadableName);

		public void SetLastUsageCodeForAllItems(string usageCode)
		{
			Items.Cast<HVLVItem>().ForEach(item => item.HVI_LastUsageCode = usageCode);
		}

		#endregion

		#region Item Line Totals

		public ZShort TotalItemLines => (ZShort)ActiveItems.Sum(item => item.Lines.Count);

		[DecimalPlaces(2)]
		public ZDecimal TotalLineCustomsValues => ActiveItems.SelectMany(item => item.Lines.OfType<HVLVItemLine>()).Sum(line => line.HVS_CustomsValue);

		[DecimalPlaces(2)]
		public ZDecimal TotalLineIntrinsicValues => ActiveItems.SelectMany(item => item.Lines.OfType<HVLVItemLine>()).Sum(line => line.HVS_IntrinsicValue);

		public HVLVCommonConsigneeConsignmentCollection ConsignmentsBelongToSameConsigneeExcludingParent
		{
			get
			{
				var result = new HVLVCommonConsigneeConsignmentCollectionExcludingParent(this);
				result.Load();
				return result;
			}
		}

		public HVLVCommonConsigneeConsignmentCollection ConsignmentsBelongToSameConsignee
		{
			get
			{
				var result = new HVLVCommonConsigneeConsignmentCollection(this);
				result.Load();
				return result;
			}
		}

		public ZString FirstLineWeightUnit
		{
			get
			{
				var result = ZString.Empty;
				if (ActiveItems.Any(x => x.HasItemLines))
				{
					result = ActiveItems.FirstOrDefault(x => x.HasItemLines).Lines.OfType<HVLVItemLine>().FirstOrDefault().HVS_WeightUnit;
				}

				if (result.IsDefault)
				{
					result = HVC_WeightUQ;
				}

				return result;
			}
		}

		public ZDecimal TotalLineGrossWeight
		{
			get => ActiveItems.SelectMany(item => item.Lines.OfType<HVLVItemLine>()).Sum(y => Weight.Convert(y.HVS_GrossWeight, y.HVS_WeightUnit, FirstLineWeightUnit));
		}

		public ZDecimal TotalLineNetWeight
		{
			get => ActiveItems.SelectMany(item => item.Lines.OfType<HVLVItemLine>()).Sum(y => Weight.Convert(y.HVS_NetWeight, y.HVS_WeightUnit, FirstLineWeightUnit));
		}

		#endregion

		#region Active Filter

		public static ZQuery ActiveFilter => new ZQuery(HVLVConsignmentSchema.HVC_IsActive, SQLComparisonOperator.Equal, true);

		#endregion

		#region Properties

		public ZBool IsSurplusAtDestination => ActiveItems.Any() && ActiveItems.All(x => x.HVI_IsUnmanifestedAtDestination);

		protected bool HVC_ConsignmentId_ReadOnly => true;

		public override ZString HVC_ConsigneeInstructions
		{
			get { return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVC_ConsigneeInstructions); }
			set
			{
				base.HVC_ConsigneeInstructions = value;
				ClearPreScreeningStatus();
			}
		}

		public override ZString HVC_GoodsDescription
		{
			get { return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVC_GoodsDescription); }
			set
			{
				base.HVC_GoodsDescription = value;
				ClearPreScreeningStatus();
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal HVC_GoodsValue
		{
			get { return base.HVC_GoodsValue; }
			set
			{
				base.HVC_GoodsValue = value;
				ClearPreScreeningStatus();
			}
		}

		[ReadOnly(true)]
		public override ZShort HVC_ItemCount
		{
			get { return displayHVC_ItemCount == null ? base.HVC_ItemCount : (ZShort)displayHVC_ItemCount; }
			set
			{
				if (value != displayHVC_ItemCount)
				{
					displayHVC_ItemCount = value;
					markedAsNeedingReload = true;
					RegisterTransactionParticipant();
					HVC_ItemCountInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateHVC_ItemCount();
					}
				}
			}
		}

		[BusinessObjectTestExclude]
		[BusinessObjectMaxLengthTestExclude]
		public override ZString HVC_ImportCustomsClearanceStatus
		{
			get => base.HVC_ImportCustomsClearanceStatus;
			set
			{
				if (base.HVC_ImportCustomsClearanceStatus != value)
				{
					base.HVC_ImportCustomsClearanceStatus = value;

					if(!value.IsEmpty)
					{
						var newReleaseStatus = GetReleaseStatus(value, isImport: true, ImportDeclaration, ImportDeclaration?.BranchCompanyCountryCode);

						if (!string.IsNullOrEmpty(newReleaseStatus) && HVC_ImportReleaseStatus != newReleaseStatus)
						{
							HVC_ImportReleaseStatus = newReleaseStatus;
							if (!HVC_JE_ImportDeclaration.IsEmpty)
							{
								UpdateRelatedConsignmentsCustomsClearanceStatus(value, HVC_JE_ImportDeclaration, isExport: false);
							}
						}
					}
				}
			}
		}

		public ZString ImportCustomsClearanceStatusDescription => GetCustomsClearanceStatusDescription(
			HVC_ImportCustomsClearanceStatus,
			isImport: true,
			ImportDeclaration,
			ImportDeclaration?.BranchCompanyCountryCode ?? ManagingShipment?.Destination?.RL_RN_NKCountryCode ?? ManifestedOnShipment?.Destination?.RL_RN_NKCountryCode ?? DestinationCountry);

		public ZPropertyInfo ImportCustomsClearanceStatusDescriptionInfo => GetZPropertyInfo(nameof(ImportCustomsClearanceStatusDescription));

		[BusinessObjectTestExclude]
		[BusinessObjectMaxLengthTestExclude]
		public override ZString HVC_ExportCustomsClearanceStatus
		{
			get => base.HVC_ExportCustomsClearanceStatus;
			set
			{
				if (base.HVC_ExportCustomsClearanceStatus != value)
				{
					base.HVC_ExportCustomsClearanceStatus = value;

					if (!value.IsEmpty)
					{
						var newReleaseStatus = GetReleaseStatus(value, isImport: false, ExportDeclaration, ExportDeclaration?.BranchCompanyCountryCode);

						if (!string.IsNullOrEmpty(newReleaseStatus) && HVC_ExportReleaseStatus != newReleaseStatus)
						{
							HVC_ExportReleaseStatus = newReleaseStatus;
							if (!HVC_JE_ExportDeclaration.IsEmpty)
							{
								UpdateRelatedConsignmentsCustomsClearanceStatus(value, HVC_JE_ExportDeclaration, isExport: true);
							}
						}
					}
				}
			}
		}

		public ZString ExportCustomsClearanceStatusDescription => GetCustomsClearanceStatusDescription(
			HVC_ExportCustomsClearanceStatus,
			isImport: false,
			ExportDeclaration,
			ExportDeclaration?.BranchCompanyCountryCode ?? ManagingShipment?.Origin?.RL_RN_NKCountryCode ?? ManifestedOnShipment?.Origin?.RL_RN_NKCountryCode ?? OriginCountry);

		ZString GetReleaseStatus(string code, bool isImport, BaseJobDeclaration declaration, ZString? countryCode)
		{
			var customsStatusService = HVLVCustomsStatusService.GetService(Factory);
			var result = ZString.Empty;
			try
			{
				result = customsStatusService.GetReleaseStatus(code, isImport, declaration, countryCode);
			}
			catch (CustomsStatusCodeNotExistInCodeListException ex)
			{
				ReportUnknownCustomsStatusError(ex);
			}

			return result;
		}

		ZString GetCustomsClearanceStatusDescription(string code, bool isImport, BaseJobDeclaration declaration, ZString? countryCode)
		{
			var customsStatusService = HVLVCustomsStatusService.GetService(Factory);
			ZString result;
			try
			{
				result = customsStatusService.GetCustomsStatusDescription(code, isImport, declaration, countryCode);
			}
			catch (CustomsStatusCodeNotExistInCodeListException)
			{
				result = $"{code} - Description Unknown";
			}

			return result;
		}

		void ReportUnknownCustomsStatusError(CustomsStatusCodeNotExistInCodeListException ex)
		{
			const string unknownCustomsStatusErrorMessageKey = "HVLVCustomsStatusService|InvalidCustomsStatusErrorMessage";
			var messageStringBuilder = new StringBuilder();
			messageStringBuilder.AppendLine($"Current Customs Status Codes is [{ex.Code}]");
			messageStringBuilder.AppendLine($"Of type: [{ex.CodeType}]");
			messageStringBuilder.AppendLine($"Searched code list: [{ex.CodeList}]");
			messageStringBuilder.AppendLine($"CustomsStatusStore CountryCode: [{ex.CountryCode}]");
			messageStringBuilder.AppendLine($"HVLVConsignment Shipper Country: [{HVC_RN_NKShipperCountryCode}]");
			messageStringBuilder.AppendLine($"HVLVConsignment Consignee Country: [{HVC_RN_NKConsigneeCountryCode}]");
			messageStringBuilder.AppendLine($"Shipment Origin Country: [{ManifestedOnShipment?.Origin?.RL_RN_NKCountryCode}]");
			messageStringBuilder.AppendLine($"Shipment Destination Country: [{ManifestedOnShipment?.Destination?.RL_RN_NKCountryCode}]");
			messageStringBuilder.AppendLine($"User Login Country: [{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}]");
			messageStringBuilder.AppendLine($"Export Declaration PK: [{HVC_JE_ExportDeclaration}]");
			messageStringBuilder.AppendLine($"Export Declaration Is Cancelled: [{ExportDeclaration?.IsCancelled}]");
			messageStringBuilder.AppendLine($"Export Declaration Message Sub Type: [{ExportDeclaration?.JE_MessageSubType}]");
			messageStringBuilder.AppendLine($"Import Declaration PK: [{HVC_JE_ImportDeclaration}]");
			messageStringBuilder.AppendLine($"Import Declaration Is Cancelled: [{ImportDeclaration?.IsCancelled}]");
			messageStringBuilder.Append($"Import Declaration Message Sub Type: [{ImportDeclaration?.JE_MessageSubType}]");
			ErrorReporter.ReportOnce(unknownCustomsStatusErrorMessageKey, messageStringBuilder.ToString());
		}

		public ZPropertyInfo ExportCustomsClearanceStatusDescriptionInfo => GetZPropertyInfo(nameof(ExportCustomsClearanceStatusDescription));

		public override ZString HVC_ImportReleaseStatus
		{
			get => base.HVC_ImportReleaseStatus;
			set
			{
				var oldReleaseStatus = base.HVC_ImportReleaseStatus;

				if (oldReleaseStatus != value)
				{
					base.HVC_ImportReleaseStatus = value;
					HVC_ReleaseStatus = value;

					UpdateStatusByReleaseStatus();

					foreach (var item in ActiveItems)
					{
						switch (value)
						{
							case HVLVReleaseStatus.Held:
								item.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;
								break;
							case HVLVReleaseStatus.Cleared:
								item.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
								break;
							case HVLVReleaseStatus.None:
								item.HVI_ImportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
								break;
							default:
								break;
						}
					}
				}
			}
		}

		public ZString ImportReleaseStatusDescription => Lookups.HVC_ReleaseStatus_List.GetDescriptionFromCode(HVC_ImportReleaseStatus);

		public ZString ReleaseStatusForCurrentDirection
		{
			get
			{
				var result = string.Empty;
				if (IsImport)
				{
					result = HVC_ImportReleaseStatus;
				}
				else if (IsExport)
				{
					result = HVC_ExportReleaseStatus;
				}

				return result;
			}
		}

		[Obsolete("HVC_ReleaseStatus will soon be removed. Please use HVC_ImportReleaseStatus or HVC_ExportReleaseStatus")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public override ZString HVC_ReleaseStatus
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			get => base.HVC_ReleaseStatus;
			set
			{
				var oldReleaseStatus = base.HVC_ReleaseStatus;

				if (oldReleaseStatus != value)
				{
					base.HVC_ReleaseStatus = value;

					foreach (var item in ActiveItems)
					{
						switch (value)
						{
							case HVLVReleaseStatus.Held:
								item.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;
								break;
							case HVLVReleaseStatus.Cleared:
								item.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
								break;
							case HVLVReleaseStatus.None:
								item.HVI_ReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
								break;
							default:
								break;
						}
					}
				}
			}
		}

		void UpdateStatusByReleaseStatus()
		{
			switch (HVC_ImportReleaseStatus)
			{
				case HVLVReleaseStatus.Cleared:
					HVC_Status = HVLVConsignmentStatus.Codes.CustomsClearedAtDestination;
					break;
				case HVLVReleaseStatus.Held:
					HVC_Status = HVLVConsignmentStatus.Codes.CustomsHeldAtDestination;
					break;
			}
		}

		public ZString ReleaseStatusDescription => Lookups.HVC_ReleaseStatus_List.GetDescriptionFromCode(HVC_ReleaseStatus);

		public ZPropertyInfo ReleaseStatusDescriptionInfo => GetZPropertyInfo(nameof(ReleaseStatusDescription));

		public override ZString HVC_ExportReleaseStatus
		{
			get => base.HVC_ExportReleaseStatus;
			set
			{
				var oldReleaseStatus = base.HVC_ExportReleaseStatus;

				if (oldReleaseStatus != value)
				{
					base.HVC_ExportReleaseStatus = value;
					HVC_ReleaseStatus = value;

					foreach (var item in ActiveItems)
					{
						switch (value)
						{
							case HVLVReleaseStatus.Held:
								item.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.Held;
								break;
							case HVLVReleaseStatus.Cleared:
								item.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.Cleared;
								break;
							case HVLVReleaseStatus.None:
								item.HVI_ExportReleaseStatus = HVLVReleaseStatus.ShortVersion.None;
								break;
							default:
								break;
						}
					}
				}
			}
		}

		public ZString ExportReleaseStatusDescription => Lookups.HVC_ReleaseStatus_List.GetDescriptionFromCode(HVC_ExportReleaseStatus);

		[List("Lookups.HVC_JS_ManifestedOnShipment_List")]
		[RelatedBusinessObject("ManifestedOnShipment")]
		public ZGuid HVC_JS_ManifestedOnShipment
		{
			get { return ConsignmentHeader?.HCH_JS_Shipment ?? ZGuid.Empty; }
			set
			{
				var shipment = Factory.Load<ForwardingShipment>(value);
				if (shipment != null)
				{
					var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
					HVC_HCH_Header = consignmentHeader.PK;
				}
			}
		}

		public bool HVC_JS_ManifestedOnShipment_ReadOnly => HasManagingShipment;

		public ForwardingShipment ManifestedOnShipment
		{
			get { return Factory.Load<ForwardingShipment>(HVC_JS_ManifestedOnShipment); }
		}

		public HVLVConsignmentHeader ConsignmentHeader => Factory.Load<HVLVConsignmentHeader>(HVC_HCH_Header);

		public override ZGuid HVC_HCH_Header
		{
			get => base.HVC_HCH_Header;
			set
			{
				var oldValue = HVC_HCH_Header;
				if (oldValue != value)
				{
					base.HVC_HCH_Header = value;

					if (ConsignmentHeader is HVLVConsignmentHeader consignmentHeader)
					{
						consignmentHeader.MarkForReloadConsignments();
						HVC_ClusterKey = consignmentHeader.HCH_ClusterKey;
					}

					Items.Where(x => x.HVI_JS_LoadedOnShipment.IsEmpty).ForEach(x => x.HVI_JS_LoadedOnShipment = ConsignmentHeader.HCH_JS_Shipment);
					BookingHeader?.MarkForReloadConsignments();
				}
			}
		}

		[List("Lookups.INCOTermsList")]
		public override ZString HVC_INCO
		{
			get { return base.HVC_INCO; }
			set { base.HVC_INCO = value; }
		}

		#region Payment Term Display

		public ZString PaymentTermDisplay
		{
			get
			{
				var prepaidCollect = IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, HVC_INCO);
				switch (prepaidCollect)
				{
					case PaymentType.Prepaid:
						return HVLVConsignmentPaymentTermDisplay.FreightPrepaid;
					case PaymentType.Collect:
						return HVLVConsignmentPaymentTermDisplay.FreightCollect;
				}

				return string.Empty;
			}
		}

		#endregion

		[ReadOnly(true)]
		public override ZDecimal HVC_ManifestedWeight
		{
			get { return displayHVC_ManifestedWeight == null ? base.HVC_ManifestedWeight : (ZDecimal)displayHVC_ManifestedWeight; }
			set
			{
				if (value != displayHVC_ManifestedWeight)
				{
					displayHVC_ManifestedWeight = value;
					markedAsNeedingReload = true;
					RegisterTransactionParticipant();
					CalculateChargeable();
					CalculateDensityInformation();
					HVC_ManifestedWeightInfo.RefreshBinding();
				}
			}
		}

		[ReadOnly(true)]
		public override ZDecimal HVC_ActualWeight
		{
			get { return displayHVC_ActualWeight == null ? base.HVC_ActualWeight : (ZDecimal)displayHVC_ActualWeight; }
			set
			{
				if (value != displayHVC_ActualWeight)
				{
					displayHVC_ActualWeight = value;
					markedAsNeedingReload = true;
					RegisterTransactionParticipant();
					CalculateChargeable();
					CalculateDensityInformation();
					HVC_ActualWeightInfo.RefreshBinding();
				}
			}
		}

		[List("Lookups.HVC_WeightUQ_List")]
		public override ZString HVC_WeightUQ
		{
			get { return base.HVC_WeightUQ; }
			set
			{
				var upperCaseValue = value.ToUpper();
				if (upperCaseValue != HVC_WeightUQ)
				{
					base.HVC_WeightUQ = upperCaseValue;
					foreach (HVLVItem item in Items)
					{
						item.CalculateManifestedWeight();
						item.CalculateChargeableInformation();
					}

					CalculateChargeable();
					CalculateDensityInformation();
					BookingHeader?.MarkForWeightRecalculation();
				}
			}
		}

		[ReadOnly(true)]
		public override ZDecimal HVC_ManifestedVolume
		{
			get { return displayHVC_ManifestedVolume == null ? base.HVC_ManifestedVolume : (ZDecimal)displayHVC_ManifestedVolume; }
			set
			{
				if (value != displayHVC_ManifestedVolume)
				{
					displayHVC_ManifestedVolume = value;
					markedAsNeedingReload = true;
					RegisterTransactionParticipant();
					CalculateChargeable();
					CalculateDensityInformation();
					HVC_ManifestedVolumeInfo.RefreshBinding();
				}
			}
		}

		[ReadOnly(true)]
		public override ZDecimal HVC_ActualVolume
		{
			get { return displayHVC_ActualVolume == null ? base.HVC_ActualVolume : (ZDecimal)displayHVC_ActualVolume; }
			set
			{
				if (value != displayHVC_ActualVolume)
				{
					displayHVC_ActualVolume = value;
					markedAsNeedingReload = true;
					RegisterTransactionParticipant();
					CalculateChargeable();
					CalculateDensityInformation();
					HVC_ActualVolumeInfo.RefreshBinding();
				}
			}
		}

		[List("Lookups.HVC_VolumeUQ_List")]
		public override ZString HVC_VolumeUQ
		{
			get { return base.HVC_VolumeUQ; }
			set
			{
				var upperCaseValue = value.ToUpper();
				if (upperCaseValue != HVC_VolumeUQ)
				{
					base.HVC_VolumeUQ = upperCaseValue;

					foreach (HVLVItem item in Items)
					{
						item.RecalculateVolume(HVLVItemSchema.HVI_ActualVolume);
						item.CalculateChargeableInformation();
					}

					CalculateChargeable();
					CalculateDensityInformation();
					BookingHeader?.MarkForVolumeRecalculation();
				}
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Baseline")]
		public ZString LastMileCarrierServiceLevelDescription
		{
			get
			{
				var result = ZString.Empty;
				var levels = LastMileCarrier?.MiscServ?.CarrierServiceLevels;
				if (levels != null)
				{
					levels.Load();
					result = (levels.Cast<OrgCarrierServiceLevel>().FirstOrDefault(x => x.PL_Code == HVC_PL_NKLastMileCarrierServiceLevel)?.PL_CarrierServiceLevelDescription) ?? ZString.Empty;
				}

				return result;
			}
		}

		public ZString LastMileCarrierServiceCode
		{
			get
			{
				var result = ZString.Empty;
				var levels = LastMileCarrier?.MiscServ?.CarrierServiceLevels;
				if (levels != null)
				{
					levels.Load();
					result = (levels.Cast<OrgCarrierServiceLevel>().FirstOrDefault(x => x.PL_Code == HVC_PL_NKLastMileCarrierServiceLevel)?.PL_CarrierServiceCode) ?? ZString.Empty;
				}

				return result;
			}
		}

		// Test Failure : DefaultClientServiceLevelFromParent
		// LoadFromNaturalKey was used on a table + field that does not have a unique index
		public override OrgCarrierServiceLevel LastMileCarrierServiceLevel
		{
			get { return Factory.LoadTop1<OrgCarrierServiceLevel>(new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, HVC_PL_NKLastMileCarrierServiceLevel)); }
		}

		[List("Lookups.HVC_CarrierAccountNumber_List")]
		public override ZString HVC_CarrierAccountNumber
		{
			get { return base.HVC_CarrierAccountNumber; }
			set { base.HVC_CarrierAccountNumber = value; }
		}

		public ZString LastMileCarrierDepotID
		{
			get
			{
				var result = ZString.Empty;
				var carrierAccount = LastMileCarrier?.CarrierAccounts.OfType<OrgCarrierAccount>().FirstOrDefault(x => x.OAN_AccountNumber == HVC_CarrierAccountNumber);
				if (carrierAccount != null)
				{
					result = carrierAccount.OAN_DepotID;
				}

				return result;
			}
		}

		[List("Lookups.HVC_Status_List")]
		[ReadOnly(true)]
		public override ZString HVC_Status
		{
			get { return base.HVC_Status; }
			set
			{
				base.HVC_Status = value;
				if (HVC_Status == HVLVConsignmentStatus.Codes.Detached)
				{
					Items.OfType<HVLVItem>().ForEach(i => i.HVI_JS_LoadedOnShipment = Guid.Empty);
				}
			}
		}

		[List("Lookups.HVC_UndgClass_List")]
		public override ZString HVC_UndgClass
		{
			get { return base.HVC_UndgClass; }
			set { base.HVC_UndgClass = value; }
		}

		[List("Lookups.DeclarationCollection")]
		[ReadOnly(true)]
		public override ZGuid HVC_JE_ImportDeclaration
		{
			get { return base.HVC_JE_ImportDeclaration; }
			set
			{
				if (HVC_JE_ImportDeclaration.IsEmpty && !value.IsEmpty)
				{
					newlyCreatedDeclarationPK = value;
				}

				base.HVC_JE_ImportDeclaration = value;

				DeclarationReferenceForDisplayInfo.RefreshBinding();
			}
		}

		[List("Lookups.DeclarationCollection")]
		[ReadOnly(true)]
		public override ZGuid HVC_JE_ExportDeclaration
		{
			get { return base.HVC_JE_ExportDeclaration; }
			set
			{
				if (HVC_JE_ExportDeclaration.IsEmpty && !value.IsEmpty)
				{
					newlyCreatedDeclarationPK = value;
				}

				base.HVC_JE_ExportDeclaration = value;

				DeclarationReferenceForDisplayInfo.RefreshBinding();
			}
		}

		ZGuid newlyCreatedDeclarationPK;

		public BaseJobDeclaration ImportDeclaration => Factory.Load<BaseJobDeclaration>(HVC_JE_ImportDeclaration);

		public BaseJobDeclaration ExportDeclaration => Factory.Load<BaseJobDeclaration>(HVC_JE_ExportDeclaration);

		public BaseJobDeclaration StandAloneDeclarationForCurrentCompany
		{
			get
			{
				if (IsImport)
				{
					return ImportDeclaration;
				}
				else if (IsExport)
				{
					return ExportDeclaration;
				}
				else
				{
					return null;
				}
			}
		}

		public ZString DeclarationReferenceForDisplay
		{
			get
			{
				if (exportDeclarationReference.IsEmpty && !HVC_JE_ExportDeclaration.IsEmpty)
				{
					exportDeclarationReference = ExportDeclaration.JE_DeclarationReference;
				}

				if (importDeclarationReference.IsEmpty && !HVC_JE_ImportDeclaration.IsEmpty)
				{
					importDeclarationReference = ImportDeclaration.JE_DeclarationReference;
				}

				return string.Join(",", new[] { exportDeclarationReference, importDeclarationReference }.Where(r => !r.IsEmpty));
			}
		}
		ZString exportDeclarationReference;
		ZString importDeclarationReference;

		ZPropertyInfo DeclarationReferenceForDisplayInfo => GetZPropertyInfo(nameof(DeclarationReferenceForDisplay));

		public override ZInt HVC_ClusterKey
		{
			get { return base.HVC_ClusterKey; }
			set
			{
				if (!value.IsEmpty && value != HVC_ClusterKey)
				{
					Items.ForEach(x => ((HVLVItem)x).HVI_ClusterKey = value);
					base.HVC_ClusterKey = value;
				}
			}
		}

		public ZBool IsReturn => FormerConsignments?.Count > 0;

		public HVLVConsignmentReturnCollection FormerConsignments
		{
			get
			{
				if (formerConsignments == null)
				{
					formerConsignments = new HVLVConsignmentReturnCollection(this, HVLVConsignmentReturnCollection.ConsignmentType.Former);
					formerConsignments.Load();
				}

				return formerConsignments;
			}
		}

		HVLVConsignmentReturnCollection formerConsignments;

		public HVLVConsignmentReturnCollection ReturnConsignments
		{
			get
			{
				if (returnConsignments == null)
				{
					returnConsignments = new HVLVConsignmentReturnCollection(this, HVLVConsignmentReturnCollection.ConsignmentType.Return);
					returnConsignments.Load();
				}

				return returnConsignments;
			}
		}

		HVLVConsignmentReturnCollection returnConsignments;

		class StringPropertyInfoWithFixedDescription : ZPropertyInfoString
		{
			public StringPropertyInfoWithFixedDescription(BusinessObject bizObj, string name) : base(bizObj, name)
			{
			}

			protected override ZString GetHumanReadableNameCore()
			{
				return GetFriendlyColumnNameShared(Name);
			}
		}

		#region Chargeable

		public ZDecimal VolumeWeight => CalculateVolumeWeightFunction(ActiveItems.Sum(x => x.EffectiveWeight), ActiveItems.Sum(x => x.EffectiveVolume));

		[SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		public ZString VolumeWeightForDisplay
		{
			get
			{
				return string.Format("{0} {1}", Math.Round(VolumeWeight, 3), ChargeableUQ);
			}
		}

		public ZPropertyInfo VolumeWeightForDisplayInfo => GetZPropertyInfo(nameof(VolumeWeightForDisplay));

		public ZString ChargeableForDisplay
		{
			get
			{
				return chargeable == null ? Res.GetString("2FD2AB1E-C9B5-4666-8C3E-AFE173A45A0D", "Not Calculated") : string.Format(Culture.Invariant, "{0} {1}", chargeable, ChargeableUQ);
			}
		}

		ZDecimal? chargeable;

		public ZPropertyInfo ChargeableForDisplayInfo => GetZPropertyInfo(nameof(ChargeableForDisplay));

		public void CalculateChargeable()
		{
			if (!IsBeingImportedFromUniversalXml)
			{
				if (Items.Count == 0)
				{
					return;
				}

				chargeable = HVLVDataRegistry.Instance.CalculateHVLVChargeablePerItem.Value
					? ActiveItems.Sum(x => CalculateChargeableFunction(x.EffectiveWeight, x.EffectiveVolume))
					: CalculateChargeableFunction(ActiveItems.Sum(x => x.EffectiveWeight), ActiveItems.Sum(x => x.EffectiveVolume));

				ChargeableForDisplayInfo.RefreshBinding();
			}
		}

		internal decimal CalculateChargeableFunction(decimal weight, decimal volume)
		{
			var chargeable = 0m;
			var zWeight = new ZWeight(weight, HVC_WeightUQ);
			var zVolume = new ZVolume(volume, HVC_VolumeUQ);

			if (ShipmentForChargeableCalculation != null)
			{
				chargeable = ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
				{
					Weight = zWeight,
					Volume = zVolume,
					TargetUnit = ChargeableUQ,
					ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(ShipmentForChargeableCalculation.IsDomesticFreight,
																				ShipmentForChargeableCalculation.TransportMode,
																				ChargeableUQ)
				}).Chargeable.Amount;
			}

			return chargeable;
		}

		internal decimal CalculateVolumeWeightFunction(decimal weight, decimal volume)
		{
			var volumeWeight = 0m;
			if (ShipmentForChargeableCalculation != null)
			{
				var quantityToConvert = IsConsignmentChargeableByWeight
					? new ZVolume(volume, HVC_VolumeUQ)
					: (IQuantity)new ZWeight(weight, HVC_WeightUQ);

				volumeWeight = ConvertToChargeableUQ(quantityToConvert);
			}

			return volumeWeight;
		}

		internal ZDecimal ConvertToChargeableUQ(ZDecimal valueToConvert, ZString fromUQ)
		{
			var quantityToConvert = IsConsignmentChargeableByWeight
				? (IQuantity)new ZWeight(valueToConvert, fromUQ)
				: new ZVolume(valueToConvert, fromUQ);

			return ConvertToChargeableUQ(quantityToConvert);
		}

		ZDecimal ConvertToChargeableUQ(IQuantity quantityToConvert)
		{
			var conversionFactor = GetConversionFactor();
			if (!conversionFactor.IsEmpty && quantityToConvert.IsValid)
			{
				return quantityToConvert.Convert(ChargeableUQ, new[] { conversionFactor }, true)?.Amount ?? 0m;
			}

			return 0m;
		}

		internal bool IsConsignmentChargeableByWeight => Weight.ContainsCode(ChargeableUQ);

		internal ZString ChargeableUQ => ShipmentForChargeableCalculation == null ?
											string.Empty :
											ChargeableAmountCalculator.GetChargeableUnit(ShipmentForChargeableCalculation.TransportMode, HVC_WeightUQ, HVC_VolumeUQ);

		ConversionFactor GetConversionFactor()
		{
			var result = ConversionFactor.Empty;

			if (ShipmentForChargeableCalculation != null)
			{
				var chargeableFactor = ChargeableFactor.GetDefault(ShipmentForChargeableCalculation.IsDomesticFreight ? ChargeableFactorSource.Domestic : ChargeableFactorSource.International, ShipmentForChargeableCalculation.TransportMode);
				if (chargeableFactor != null)
				{
					var isImperial = IsConsignmentChargeableByWeight
					? Weight.IsImperial(ChargeableUQ)
					: Volume.IsImperial(ChargeableUQ);

					result = isImperial ? chargeableFactor.ImperialFactor : chargeableFactor.MetricFactor;
				}
			}

			return result;
		}

		ForwardingShipment ShipmentForChargeableCalculation => managingShipment ?? ManifestedOnShipment;

		#endregion

		#region Density

		public ZDecimal DensityFactor => Density.DensityFactor;

		public HVLVConsignmentDensity Density
		{
			get { return density ?? (density = new HVLVConsignmentDensity(this)); }
		}

		HVLVConsignmentDensity density;

		void CalculateDensityInformation()
		{
			VolumeWeightForDisplayInfo.RefreshBinding();
			Density.RefreshAllValues();
		}

		#endregion

		#region Pre-Screening

		[List("Lookups.HVC_PreScreeningStatus_List")]
		[ReadOnly(true)]
		public override ZString HVC_PreScreeningStatus
		{
			get { return base.HVC_PreScreeningStatus; }
			set { base.HVC_PreScreeningStatus = value; }
		}

		public ZString PreScreeningWarningDetails => (preScreeningWarningDetails ?? (preScreeningWarningDetails = new ZStringBuilder())).ToString();
		ZStringBuilder preScreeningWarningDetails;

		public ZString PreScreeningErrorDetails => (preScreeningErrorDetails ?? (preScreeningErrorDetails = new ZStringBuilder())).ToString();
		ZStringBuilder preScreeningErrorDetails;

		public ZString PreScreeningNotifyOnlyWarningDetails => (preScreeningNotifyOnlyWarningDetails ?? (preScreeningNotifyOnlyWarningDetails = new ZStringBuilder())).ToString();
		ZStringBuilder preScreeningNotifyOnlyWarningDetails;

		public void ClearPreScreeningDetails()
		{
			preScreeningWarningDetails?.Clear();
			preScreeningErrorDetails?.Clear();
			preScreeningNotifyOnlyWarningDetails?.Clear();
		}

		public void AddPreScreeningWarningDetails(string warningDetails)
		{
			(preScreeningWarningDetails ?? (preScreeningWarningDetails = new ZStringBuilder())).AppendLine(warningDetails);
		}

		public void AddPreScreeningErrorDetails(string errorDetails)
		{
			(preScreeningErrorDetails ?? (preScreeningErrorDetails = new ZStringBuilder())).AppendLine(errorDetails);
		}

		public void AddPreScreeningNotifyOnlyWarningDetails(string notifyOnlyWarningDetails)
		{
			(preScreeningNotifyOnlyWarningDetails ?? (preScreeningNotifyOnlyWarningDetails = new ZStringBuilder())).AppendLine(notifyOnlyWarningDetails);
		}

		public void AcceptFailedPreScreeningAsEntered()
		{
			if (HVC_PreScreeningStatus == HVLVConsignmentPreScreeningStatusCodes.Codes.Failed)
			{
				HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.FailedAcceptedAsEntered;
				this.GetLogs().AddNew(AutoEvents.StatusUpdated, "|TYP=Pre-Screening|RES=Failed Accepted as Entered"); // Log Reference Values should be in English only
			}
		}

		public ZBool HasUnknownPrescreeningStatus => HVC_IsActive && HVC_PreScreeningStatus == HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown;

		public ZBool HasFailedPreScreeningStatus => HVC_PreScreeningStatus == HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

		#endregion

		#region Denied Party Screening

		[List(nameof(Lookups) + "." + nameof(HVLVConsignmentLookups.HVC_DeniedPartyScreeningStatus_List))]
		[ReadOnly(true)]
		public override ZString HVC_DeniedPartyScreeningStatus
		{
			get { return base.HVC_DeniedPartyScreeningStatus; }
			set
			{
				base.HVC_DeniedPartyScreeningStatus = value;

				if (ConsignmentHeader != null)
				{
					ConsignmentHeader.UpdateDeniedPartyScreeningStatus();
				}
			}
		}

		#endregion

		#region ValidationSection

		public AddressValidationSection ValidationSection => AddressValidationSection.HVLVConsignment;

		#endregion

		#region CartageZone

		public ZString HVC_Calc_DeliveryCartageZone
		{
			get
			{
				var location = LocationHelper.GetLocationFromString(ConsignmentHeader?.Shipment?.JS_RL_NKDestination ?? HVC_RN_NKConsigneeCountryCode, Factory);

				var cacheKey = string.Join("-",
					LastMileCarrier?.OH_Code.ToString() ?? string.Empty,
					location?.Code ?? string.Empty,
					HVC_RN_NKConsigneeCountryCode,
					HVC_ConsigneePostcode,
					HVC_ConsigneeCity);

				return Factory.GetCachedValue(cacheKey,
					() => RateTransportZoneHelper.GetZoneName(Factory, LastMileCarrier, location, HVC_RN_NKConsigneeCountryCode, HVC_ConsigneePostcode, HVC_ConsigneeCity),
					CacheStalenessPolicy.StaleWhenDataTableChanges(RateTransportZonesSchema.Constants.TableName, Factory));
			}
		}

		IRateTransportZoneHelper RateTransportZoneHelper => fRateTransportZoneHelper ?? (fRateTransportZoneHelper = ObjectFactory.Get<IRateTransportZoneHelper>());
		IRateTransportZoneHelper fRateTransportZoneHelper;

		#endregion

		public override ZGuid HVC_OH_LastMileCarrier
		{
			get => base.HVC_OH_LastMileCarrier;
			set
			{
				if (value != HVC_OH_LastMileCarrier)
				{
					base.HVC_OH_LastMileCarrier = value;
					UpdateScreeningStatusFromParties();
				}
			}
		}

		public override ZGuid HVC_OH_LastMileCarrierBookingAgent
		{
			get => base.HVC_OH_LastMileCarrierBookingAgent;
			set
			{
				if (value != HVC_OH_LastMileCarrierBookingAgent)
				{
					base.HVC_OH_LastMileCarrierBookingAgent = value;
					UpdateScreeningStatusFromParties();
				}
			}
		}

		public override ZGuid HVC_OA_DestinationDepot
		{
			get => base.HVC_OA_DestinationDepot;
			set
			{
				if (value != HVC_OA_DestinationDepot)
				{
					base.HVC_OA_DestinationDepot = value;
					UpdateScreeningStatusFromParties();
				}
			}
		}

		#region Consignee

		public ZBool ConsigneeIsOrganisation => ConsigneeAddress != null;

		public override ZGuid HVC_OA_ConsigneeAddress
		{
			get => base.HVC_OA_ConsigneeAddress;
			set
			{
				if (base.HVC_OA_ConsigneeAddress != value)
				{
					base.HVC_OA_ConsigneeAddress = value;

					if (ConsigneeIsOrganisation)
					{
						HVC_ConsigneeName = ConsigneeAddress.CompanyName;
						HVC_ConsigneeAddress1 = ConsigneeAddress.Address1;
						HVC_ConsigneeAddress2 = ConsigneeAddress.Address2;
						HVC_ConsigneeCity = ConsigneeAddress.City;
						HVC_ConsigneePostcode = ConsigneeAddress.Postcode;
						HVC_ConsigneeState = ConsigneeAddress.StateCode;
						HVC_RN_NKConsigneeCountryCode = ConsigneeAddress.OA_RN_NKCountryCode;
						UpdateScreeningStatusFromParties();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		public override ZString HVC_ConsigneeName
		{
			get { return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVC_ConsigneeName); }
			set
			{
				base.HVC_ConsigneeName = value;
				ClearPreScreeningStatus();

				if (!ConsigneeIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ConsigneeNameInfo => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ConsigneeName);

		[List("Lookups.ConsigneeOrgContact_List")]
		public override ZString HVC_ConsigneeContact
		{
			get { return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVC_ConsigneeContact); }
			set
			{
				if (base.HVC_ConsigneeContact != value)
				{
					base.HVC_ConsigneeContact = value;
					var consigneeHeader = ConsigneeAddress?.Header;
					if (consigneeHeader != null)
					{
						var query = new ZQuery(OrgContactSchema.OC_ContactName, value);
						var contact = consigneeHeader.Contacts.Find(query).FirstOrDefault() as OrgContact;
						if (contact != null)
						{
							HVC_ConsigneePhone = contact.OC_Phone;
							HVC_ConsigneeMobile = contact.OC_Mobile;
							HVC_ConsigneeFax = contact.OC_Fax;
							HVC_ConsigneeEmail = contact.OC_Email;
						}
					}
				}

				ClearPreScreeningStatus();
			}
		}

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		public override ZString HVC_ConsigneeAddress1
		{
			get { return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVC_ConsigneeAddress1); }
			set
			{
				base.HVC_ConsigneeAddress1 = value;
				ClearPreScreeningStatus();

				if (!ConsigneeIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ConsigneeAddress1Info => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ConsigneeAddress1);

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		public override ZString HVC_ConsigneeAddress2
		{
			get { return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVC_ConsigneeAddress2); }
			set
			{
				base.HVC_ConsigneeAddress2 = value;
				ClearPreScreeningStatus();

				if (!ConsigneeIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ConsigneeAddress2Info => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ConsigneeAddress2);

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		public override ZString HVC_ConsigneeCity
		{
			get { return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVC_ConsigneeCity); }
			set
			{
				base.HVC_ConsigneeCity = value;
				ClearPreScreeningStatus();

				if (!ConsigneeIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ConsigneeCityInfo => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ConsigneeCity);

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		public override ZString HVC_ConsigneePostcode
		{
			get { return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVC_ConsigneePostcode); }
			set
			{
				base.HVC_ConsigneePostcode = value;
				ClearPreScreeningStatus();

				if (!ConsigneeIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ConsigneePostcodeInfo => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ConsigneePostcode);

		public override ZString HVC_ConsigneeEmail
		{
			get { return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVC_ConsigneeEmail); }
			set
			{
				base.HVC_ConsigneeEmail = value;
				ClearPreScreeningStatus();
			}
		}

		public override ZString HVC_ConsigneeMobile
		{
			get { return base.HVC_ConsigneeMobile; }
			set
			{
				base.HVC_ConsigneeMobile = value;
				ClearPreScreeningStatus();
			}
		}

		public override ZString HVC_ConsigneePhone
		{
			get { return base.HVC_ConsigneePhone; }
			set
			{
				base.HVC_ConsigneePhone = value;
				ClearPreScreeningStatus();
			}
		}

		public override ZString HVC_ConsigneeFax
		{
			get { return base.HVC_ConsigneeFax; }
			set
			{
				base.HVC_ConsigneeFax = value;
				ClearPreScreeningStatus();
			}
		}

		[List("Lookups.ConsigneeStateList")]
		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		public override ZString HVC_ConsigneeState
		{
			get { return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVC_ConsigneeState); }
			set
			{
				base.HVC_ConsigneeState = value;
				ClearPreScreeningStatus();

				if (!ConsigneeIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ConsigneeStateInfo => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ConsigneeState);

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		public override ZString HVC_RN_NKConsigneeCountryCode
		{
			get => base.HVC_RN_NKConsigneeCountryCode;
			set
			{
				if (base.HVC_RN_NKConsigneeCountryCode != value)
				{
					base.HVC_RN_NKConsigneeCountryCode = value;
					if (!ConsigneeIsOrganisation)
					{
						InvalidateScreeningStatuses();
					}
				}
			}
		}

		#endregion

		#region Shipper

		public ZBool ShipperIsOrganisation => ShipperAddress != null;

		public override ZGuid HVC_OA_ShipperAddress
		{
			get => base.HVC_OA_ShipperAddress;
			set
			{
				if (base.HVC_OA_ShipperAddress != value)
				{
					base.HVC_OA_ShipperAddress = value;

					if (ShipperIsOrganisation)
					{
						HVC_ShipperName = ShipperAddress.CompanyName;
						HVC_ShipperAddress1 = ShipperAddress.Address1;
						HVC_ShipperAddress2 = ShipperAddress.Address2;
						HVC_ShipperCity = ShipperAddress.City;
						HVC_ShipperPostcode = ShipperAddress.Postcode;
						HVC_ShipperState = ShipperAddress.StateCode;
						HVC_RN_NKShipperCountryCode = ShipperAddress.OA_RN_NKCountryCode;
						UpdateScreeningStatusFromParties();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		public override ZString HVC_ShipperName
		{
			get { return base.HVC_ShipperName; }
			set
			{
				base.HVC_ShipperName = value;
				ClearPreScreeningStatus();

				if (!ShipperIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ShipperNameInfo => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ShipperName);

		[List("Lookups.ShipperOrgContact_List")]
		public override ZString HVC_ShipperContact
		{
			get { return base.HVC_ShipperContact; }
			set
			{
				if (base.HVC_ShipperContact != value)
				{
					base.HVC_ShipperContact = value;
					var shipperHeader = ShipperAddress?.Header;
					if (shipperHeader != null)
					{
						var query = new ZQuery(OrgContactSchema.OC_ContactName, value);
						var contact = shipperHeader.Contacts.Find(query).FirstOrDefault() as OrgContact;
						if (contact != null)
						{
							HVC_ShipperPhone = contact.OC_Phone;
							HVC_ShipperMobile = contact.OC_Mobile;
							HVC_ShipperFax = contact.OC_Fax;
							HVC_ShipperEmail = contact.OC_Email;
						}
					}
				}

				ClearPreScreeningStatus();
			}
		}

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		public override ZString HVC_ShipperAddress1
		{
			get { return base.HVC_ShipperAddress1; }
			set
			{
				base.HVC_ShipperAddress1 = value;
				ClearPreScreeningStatus();

				if (!ShipperIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ShipperAddress1Info => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ShipperAddress1);

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		public override ZString HVC_ShipperAddress2
		{
			get { return base.HVC_ShipperAddress2; }
			set
			{
				base.HVC_ShipperAddress2 = value;
				ClearPreScreeningStatus();

				if (!ShipperIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ShipperAddress2Info => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ShipperAddress2);

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		public override ZString HVC_ShipperCity
		{
			get { return base.HVC_ShipperCity; }
			set
			{
				base.HVC_ShipperCity = value;
				ClearPreScreeningStatus();

				if (!ShipperIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ShipperCityInfo => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ShipperCity);

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		public override ZString HVC_ShipperPostcode
		{
			get { return base.HVC_ShipperPostcode; }
			set
			{
				base.HVC_ShipperPostcode = value;
				ClearPreScreeningStatus();

				if (!ShipperIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ShipperPostcodeInfo => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ShipperPostcode);

		public override ZString HVC_ShipperEmail
		{
			get { return base.HVC_ShipperEmail; }
			set
			{
				base.HVC_ShipperEmail = value;
				ClearPreScreeningStatus();
			}
		}

		public override ZString HVC_ShipperMobile
		{
			get { return base.HVC_ShipperMobile; }
			set
			{
				base.HVC_ShipperMobile = value;
				ClearPreScreeningStatus();
			}
		}

		public override ZString HVC_ShipperPhone
		{
			get { return base.HVC_ShipperPhone; }
			set
			{
				base.HVC_ShipperPhone = value;
				ClearPreScreeningStatus();
			}
		}

		public override ZString HVC_ShipperFax
		{
			get { return base.HVC_ShipperFax; }
			set
			{
				base.HVC_ShipperFax = value;
				ClearPreScreeningStatus();
			}
		}

		[List("Lookups.ConsignorStateList")]
		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		public override ZString HVC_ShipperState
		{
			get { return base.HVC_ShipperState; }
			set
			{
				base.HVC_ShipperState = value;
				ClearPreScreeningStatus();

				if (!ShipperIsOrganisation)
				{
					InvalidateScreeningStatuses();
				}
			}
		}

		public override ZPropertyInfo HVC_ShipperStateInfo => new StringPropertyInfoWithFixedDescription(this, HVLVConsignmentSchema.Constants.HVC_ShipperState);

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		public override ZString HVC_RN_NKShipperCountryCode
		{
			get => base.HVC_RN_NKShipperCountryCode;
			set
			{
				if (base.HVC_RN_NKShipperCountryCode != value)
				{
					base.HVC_RN_NKShipperCountryCode = value;
					if (!ShipperIsOrganisation)
					{
						InvalidateScreeningStatuses();
					}
				}
			}
		}

		#endregion

		#region Return

		public ZBool ReturnIsOrganisation => ReturnLocation != null;

		public override ZGuid HVC_OA_ReturnLocation
		{
			get => base.HVC_OA_ReturnLocation;
			set
			{
				if (base.HVC_OA_ReturnLocation != value)
				{
					base.HVC_OA_ReturnLocation = value;

					if (ReturnIsOrganisation)
					{
						HVC_ReturnName = ReturnLocation.CompanyName;
						HVC_ReturnAddress1 = ReturnLocation.Address1;
						HVC_ReturnAddress2 = ReturnLocation.Address2;
						HVC_ReturnCity = ReturnLocation.City;
						HVC_ReturnPostcode = ReturnLocation.Postcode;
						HVC_ReturnState = ReturnLocation.StateCode;
						HVC_RN_NKReturnCountryCode = ReturnLocation.OA_RN_NKCountryCode;
						UpdateScreeningStatusFromParties();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ReturnIsOrganisation))]
		public override ZString HVC_ReturnName
		{
			get => base.HVC_ReturnName;
			set
			{
				if (base.HVC_ReturnName != value)
				{
					base.HVC_ReturnName = value;
					if (!ReturnIsOrganisation)
					{
						InvalidateScreeningStatuses();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ReturnIsOrganisation))]
		public override ZString HVC_ReturnAddress1
		{
			get => base.HVC_ReturnAddress1;
			set
			{
				if (base.HVC_ReturnAddress1 != value)
				{
					base.HVC_ReturnAddress1 = value;
					if (!ReturnIsOrganisation)
					{
						InvalidateScreeningStatuses();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ReturnIsOrganisation))]
		public override ZString HVC_ReturnAddress2
		{
			get => base.HVC_ReturnAddress2;
			set
			{
				if (base.HVC_ReturnAddress2 != value)
				{
					base.HVC_ReturnAddress2 = value;
					if (!ReturnIsOrganisation)
					{
						InvalidateScreeningStatuses();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ReturnIsOrganisation))]
		public override ZString HVC_ReturnCity
		{
			get => base.HVC_ReturnCity;
			set
			{
				if (base.HVC_ReturnCity != value)
				{
					base.HVC_ReturnCity = value;
					if (!ReturnIsOrganisation)
					{
						InvalidateScreeningStatuses();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ReturnIsOrganisation))]
		public override ZString HVC_ReturnPostcode
		{
			get => base.HVC_ReturnPostcode;
			set
			{
				if (base.HVC_ReturnPostcode != value)
				{
					base.HVC_ReturnPostcode = value;
					if (!ReturnIsOrganisation)
					{
						InvalidateScreeningStatuses();
					}
				}
			}
		}

		[List("Lookups.ReturnAddrStateList")]
		[ReadOnlyMember(nameof(ReturnIsOrganisation))]
		public override ZString HVC_ReturnState
		{
			get { return base.HVC_ReturnState; }
			set
			{
				if (base.HVC_ReturnState != value)
				{
					base.HVC_ReturnState = value;
					if (!ReturnIsOrganisation)
					{
						InvalidateScreeningStatuses();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(ReturnIsOrganisation))]
		public override ZString HVC_RN_NKReturnCountryCode
		{
			get => base.HVC_RN_NKReturnCountryCode;
			set
			{
				if (base.HVC_RN_NKReturnCountryCode != value)
				{
					base.HVC_RN_NKReturnCountryCode = value;
					if (!ReturnIsOrganisation)
					{
						InvalidateScreeningStatuses();
					}
				}
			}
		}

		[List("Lookups.ReturnContact_List")]
		public override ZString HVC_ReturnContact
		{
			get { return base.HVC_ReturnContact; }
			set
			{
				if (base.HVC_ReturnContact != value)
				{
					base.HVC_ReturnContact = value;
					var returnLocationHeader = ReturnLocation?.Header;
					if (returnLocationHeader != null)
					{
						var query = new ZQuery(OrgContactSchema.OC_ContactName, value);
						var contact = returnLocationHeader.Contacts.Find(query).FirstOrDefault() as OrgContact;
						if (contact != null)
						{
							HVC_ReturnPhone = contact.OC_Phone;
							HVC_ReturnMobile = contact.OC_Mobile;
							HVC_ReturnFax = contact.OC_Fax;
							HVC_ReturnEmail = contact.OC_Email;
						}
					}
				}

				ClearPreScreeningStatus();
			}
		}

		[DecimalPlaces(2)]
		[ResourceStringData("0002DBB1-D31D-400D-84B0-83C2FAA74B8A", Caption = "Goods Value in Local Currency")]
		public ZDecimal GoodsValueInLocalCurrency
		{
			get
			{
				if (GoodsValueCurrency == null || LocalCurrency == null)
				{
					return ZDecimal.Zero;
				}

				if (LocalCurrencyCode == HVC_RX_NKGoodsValueCurrency)
				{
					return HVC_GoodsValue;
				}

				return GoodsValueCurrency.ConvertUsingCustomsRate(ZDateTime.Today, HVC_GoodsValue, LocalCurrency);
			}
		}

		public ZPropertyInfo GoodsValueOnLocalCurrencyInfo => GetZPropertyInfo(nameof(GoodsValueInLocalCurrency));

		public ZString LocalCurrencyCode => LocalCurrency?.Code ?? ZString.Empty;

		public ZPropertyInfo LocalCurrencyCodeInfo => GetZPropertyInfo(nameof(LocalCurrencyCode));

		RefCurrency LocalCurrency => GlbCompany.CurrentCompany?.Country?.LocalCurrency;

		#endregion

		#region ACAS

		[List("Lookups.HVC_ACASStatus_List")]
		[ReadOnly(true)]
		public override ZString HVC_ACASStatus
		{
			get { return base.HVC_ACASStatus; }
			set { base.HVC_ACASStatus = value; }
		}

		[List("Lookups.HVC_ACASMessageStatus_List")]
		[ReadOnly(true)]
		public override ZString HVC_ACASMessageStatus
		{
			get { return base.HVC_ACASMessageStatus; }
			set { base.HVC_ACASMessageStatus = value; }
		}

		[List("Lookups.HVC_ACASInterchangeStatus_List")]
		[ReadOnly(true)]
		public override ZString HVC_ACASInterchangeStatus
		{
			get { return base.HVC_ACASInterchangeStatus; }
			set { base.HVC_ACASInterchangeStatus = value; }
		}

		public ZBool RequiresACAS => TransportModeIsAir
			&& !USCustomsJurisdiction.Countries.Contains(GetCountryCodeFromShipment(shipment => shipment.Origin?.RL_RN_NKCountryCode))
			&& USCustomsJurisdiction.Countries.Contains(GetCountryCodeFromShipment(shipment => shipment.Destination?.RL_RN_NKCountryCode));

		public bool IsLastCBPResponseOnSelecteeDataIssueHold
		{
			get
			{
				switch (HVC_ACASStatus)
				{
					case ACASActions.Code.SelecteeDataIssueHold:
						return true;
					default:
						return false;
				}
			}
		}

		public bool IsLastCBPResponseOnHold
		{
			get
			{
				switch (HVC_ACASStatus)
				{
					case ACASActions.Code.DoNotLoadHold:
					case ACASActions.Code.SelecteeDataIssueHold:
					case ACASActions.Code.SelecteeScreeningOrVerificationRequiredHold:
					case ACASActions.Code.DoNotLoadHoldCurrentlyInPlace:
					case ACASActions.Code.SelecteeDataIssueHoldCurrentlyInPlace:
					case ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldCurrentlyInPlace:
						return true;
					default:
						return false;
				}
			}
		}

		#endregion

		#region System Create / Last Edit

		protected bool HVC_SystemCreateTimeUtc_ReadOnly => true;

		protected bool HVC_SystemCreateUser_ReadOnly => true;

		protected bool HVC_SystemLastEditTimeUtc_ReadOnly => true;

		protected bool HVC_SystemLastEditUser_ReadOnly => true;

		#endregion

		#endregion

		#region ShipperProperties

		IReadOnlyDictionary<ZPropertyInfo, Func<JobDocAddress, ZString>> ShipperDefaultingInfoLookup => shipperDefaultingInfoLookup ?? (shipperDefaultingInfoLookup = GetShipperDefaultingInfoLookup());
		IReadOnlyDictionary<ZPropertyInfo, Func<JobDocAddress, ZString>> shipperDefaultingInfoLookup;

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		IReadOnlyDictionary<ZPropertyInfo, Func<JobDocAddress, ZString>> GetShipperDefaultingInfoLookup()
		{
			return new Dictionary<ZPropertyInfo, Func<JobDocAddress, ZString>>()
			{
				[HVC_ShipperNameInfo] = address => address?.E2_CompanyName ?? ZString.Empty,
				[HVC_ShipperAddress1Info] = address => address?.E2_Address1 ?? ZString.Empty,
				[HVC_ShipperAddress2Info] = address => address?.E2_Address2 ?? ZString.Empty,
				[HVC_ShipperCityInfo] = address => address?.E2_City ?? ZString.Empty,
				[HVC_ShipperStateInfo] = address => address?.E2_State ?? ZString.Empty,
				[HVC_ShipperPostcodeInfo] = address => address?.E2_Postcode ?? ZString.Empty,
				[HVC_RN_NKShipperCountryCodeInfo] = address => address?.E2_RN_NKCountryCode ?? ZString.Empty,
				[HVC_ShipperContactInfo] = address => address?.E2_Contact ?? ZString.Empty,
				[HVC_ShipperEmailInfo] = address => address?.E2_Email ?? ZString.Empty,
				[HVC_ShipperPhoneInfo] = address => address?.E2_Phone ?? ZString.Empty,
				[HVC_ShipperMobileInfo] = address => address?.E2_Mobile ?? ZString.Empty,
				[HVC_ShipperFaxInfo] = address => address?.E2_Fax ?? ZString.Empty
			};
		}

		bool AllShipperPropertiesAreEmpty => ShipperDefaultingInfoLookup.All(x => x.Key.Value.IsEmpty);

		internal bool ShipperPropertyShouldPerformMandatoryValidation(ZPropertyInfo propertyInfo)
		{
			return !IsSurplusAtDestination && (IsInDatabase
				|| !AllShipperPropertiesAreEmpty
				|| ShipperDefaultingInfoLookup[propertyInfo](GetDistinctShipmentConsignor()).IsEmpty);
		}

		JobDocAddress GetDistinctShipmentConsignor()
		{
			var query = Items
				.Cast<HVLVItem>()
				.Select(x => x.Shipment?.ConsignorDocumentaryAddress)
				.Where(x => x != null);
#if NETFRAMEWORK
			var addresses = query
				.DistinctBy(x => x.E2_CompanyName)
				.Take(2)
				.ToArray();
#elif NET
			var addresses = Enumerable
				.DistinctBy(query, x => x.E2_CompanyName)
				.Take(2)
				.ToArray();
#endif

			return addresses.Length == 1 ? addresses[0] : null;
		}

#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return HVC_ConsignmentId; }
		}

		#endregion

		#region Overrides

		public ZString HumanReadableNameWithoutId => Res.GetString("b6522241-64b1-425f-84a1-8b5cb4c3447f", "HVLV Consignment");

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = HumanReadableNameWithoutId;
				if (!HVC_ConsignmentId.IsEmpty)
				{
					result += " " + HVC_ConsignmentId;
				}

				return result;
			}
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(Items);

				var shipments = Items.Cast<HVLVItem>().Where(x => x.Shipment != null).Select(x => x.Shipment).Distinct();

				var loadLists = Items.Cast<HVLVItem>().Where(x => x.LoadList != null).Select(x => x.LoadList).Distinct();

				var outerPackages = Items.Cast<HVLVItem>().Where(x => x.OuterPackage != null).Select(x => x.OuterPackage).Distinct();

				foreach (var shipment in shipments)
				{
					var cusRelatedBusinessObjects = CusRelatedEventsFinder.GetRelatedBizOs(shipment, HVC_ConsignmentId);
					if (cusRelatedBusinessObjects != null)
					{
						result.AddRange(cusRelatedBusinessObjects);
					}

					result.Add(shipment);
					result.AddRange(shipment.Consols);
					result.AddRange(shipment.TransportsIncludingRelated);
				}

				foreach (var loadList in loadLists)
				{
					result.Add(loadList);
				}

				foreach (var outerPackage in outerPackages)
				{
					result.Add(outerPackage);
				}

				if (BookingHeader != null)
				{
					result.Add(BookingHeader);
				}

				result.AddSafe(ImportDeclaration);
				result.AddSafe(ExportDeclaration);

				var transportBookings = Items.Cast<HVLVItem>().Where(item => !item.HVI_KM_LastMileTransportBooking.IsEmpty).Select(item => item.LastMileTransportBooking);
				result.AddRange(transportBookings);

				result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this));

				return result.ToArray();
			}
		}

		protected override ZAddress GetNewHVC_OA_ConsigneeAddress_ZAddress()
		{
			var zAddress = base.GetNewHVC_OA_ConsigneeAddress_ZAddress();
			zAddress.GetDefaultAddress = HVLVConsignment_GetDefaultAddress;
			return zAddress;
		}

		protected override ZAddress GetNewHVC_OA_ShipperAddress_ZAddress()
		{
			var zAddress = base.GetNewHVC_OA_ShipperAddress_ZAddress();
			zAddress.GetDefaultAddress = HVLVConsignment_GetDefaultAddress;
			return zAddress;
		}

		#endregion

		#region IWorkflowProvider

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.HVLVConsignmentWorkflowDescriptorCode;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider() => null;

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

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new HVLVConsignmentProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}

		HVLVConsignmentProcessTaskCollection workflowItems;

		#region Universal XML Import and Export Toggles

		public DisposableAction SetIsBeingExportedToUniversalXml()
		{
			IsBeingExportedToUniversalXml = true;
			return new DisposableAction(() => IsBeingExportedToUniversalXml = false);
		}

		public bool IsBeingExportedToUniversalXml { get; private set; }

		public DisposableAction SetIsBeingImportedFromUniversalXml()
		{
			IsBeingImportedFromUniversalXml = true;
			return new DisposableAction(() => IsBeingImportedFromUniversalXml = false);
		}

		public bool IsBeingImportedFromUniversalXml { get; private set; }

		#endregion

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria() => BookingHeader != null ? HVLVBookingHeader.GetTemplateSelectionCriteria(BookingHeader) : new ColumnValueRanker();

		#endregion

		#region GS1 Prefix

		ZString GS1Prefix => ConsignmentHeader?.GS1Info?.GS1Prefix ?? BookingHeader?.GS1Info?.GS1Prefix ?? ZString.Empty;

		internal bool HasValidGS1Prefix => SSCCBarCodeChecker.IsSSCCBarCodePrefix(GS1Prefix);

		#endregion

		#region Item Totals

		internal void CalculateItemTotals()
		{
			CalculateItemCount();
			CalculateManifestedWeight();
			CalculateManifestedVolume();
			CalculateActualWeight();
			CalculateActualVolume();
		}

		void CalculateItemCount()
		{
			HVC_ItemCount = (ZShort)ActiveItems.Count();
		}

		internal ZDecimal EffectiveWeight => HVC_ActualWeight.IsEmpty ? HVC_ManifestedWeight : HVC_ActualWeight;
		internal ZDecimal EffectiveVolume => HVC_ActualVolume.IsEmpty ? HVC_ManifestedVolume : HVC_ActualVolume;

		internal void CalculateManifestedWeight()
		{
			HVC_ManifestedWeight = ActiveItems.Sum(x => x.HVI_ManifestedWeight);
		}

		internal void CalculateManifestedVolume()
		{
			HVC_ManifestedVolume = ActiveItems.Sum(x => x.HVI_ManifestedVolume);
		}

		internal void CalculateActualWeight()
		{
			HVC_ActualWeight = ActiveItems.Sum(x => x.HVI_ActualWeight);
		}

		internal void CalculateActualVolume()
		{
			HVC_ActualVolume = ActiveItems.Sum(x => x.HVI_ActualVolume);
		}

		#endregion

		#region Detach and Attach

		public override bool CanDetach => string.IsNullOrEmpty(ReasonNotToBeAbleToDetach);

		public override string ReasonNotToBeAbleToDetach
		{
			get
			{
				if (!HVC_IsActive)
				{
					return Res.GetString("9515251C-A957-4A96-885F-BFA34659C934", "Unable to detach Consignment {0} from Shipment {1} since Consignment is not active. ", HVC_ConsignmentId, ConsignmentHeader.Shipment.JS_UniqueConsignRef);
				}
				else if (!AllowedStatusesForDetaching.Contains(HVC_Status))
				{
					return Res.GetString("0CB9E403-ED6C-4A37-B3F6-7B8CEC57B5A7", "Unable to detach Consignment {0} from Shipment {1} since Consignment Status = {2}. ", HVC_ConsignmentId, ConsignmentHeader.Shipment.JS_UniqueConsignRef, GetStatusDescription());
				}
				else if (ConsignmentHeader.GenPivotCollection.CustomsJobs.Any(job => !(job is ICancellable cancellable && cancellable.IsCancelled)))
				{
					return Res.GetString("EE3DE72C-635C-495D-B631-DAEAEF0FB0B5", "Unable to detach Consignment {0} from Shipment {1} since Customs job has already been created. ", HVC_ConsignmentId, ConsignmentHeader.Shipment.JS_UniqueConsignRef);
				}
				else
				{
					return base.ReasonNotToBeAbleToDetach;
				}
			}
		}

		static List<string> AllowedStatusesForDetaching => new() { HVLVConsignmentStatus.Codes.Booked, HVLVConsignmentStatus.Codes.Confirmed, HVLVConsignmentStatus.Codes.DepartedFromOriginDepot };

		MultilingualString GetStatusDescription()
		{
			return HVLVConsignmentLookups.GetAllHVLVConsignmentStatuses().GetMultilingualDescriptionFromCode(HVC_Status);
		}

		#endregion

		#region Loading / Saving / Deleting

		protected override void RunPreSaveValidationCore()
		{
			if (!IsInDatabase && AllShipperPropertiesAreEmpty)
			{
				var consignor = GetDistinctShipmentConsignor();

				if (consignor != null)
				{
					foreach (var shipperDefaultInfo in ShipperDefaultingInfoLookup)
					{
						shipperDefaultInfo.Key.Value = shipperDefaultInfo.Value(consignor);
					}
				}
			}

			base.RunPreSaveValidationCore();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			HVC_Status = HVLVConsignmentStatus.Codes.Booked;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			PopulateConsignmentAndItemsIdIfNeeded();
			PopulateCustomsReferenceNumberIfNeeded();

			if (HVC_IsActiveInfo.HasChanges)
			{
				Logs.AddNew(HVC_IsActive ? AutoEvents.SetToActive : AutoEvents.SetToInactive);
			}

#if DEBUG
			if (Globals.IsTest)
			{
				if (HVC_HCH_Header.IsEmpty && HVC_HVH_BookingHeader.IsEmpty)
				{
					Factory.NewWithValidTestData<HVLVBookingHeader>().Consignments.Add(this);
				}
			}
#endif
		}

		void PopulateConsignmentAndItemsIdIfNeeded()
		{
			if (!IsInDatabase)
			{
				DefaultReferencesForNewConsignment();
				var isFirstItem = true;

				foreach (HVLVItem item in Items)
				{
					if (!HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.Value)
					{
						item.HVI_ItemId = HVLVItem.GetItemIdCandidates(item, isFirstItem).First(x => !x.IsEmpty);
					}

					isFirstItem = false;
				}
			}
		}

		void PopulateCustomsReferenceNumberIfNeeded()
		{
			if (!newlyCreatedDeclarationPK.IsEmpty && Factory.Load<BaseJobDeclaration>(newlyCreatedDeclarationPK) is BaseJobDeclaration newDeclaration)
			{
				newDeclaration.PopulateJE_DeclarationReferenceIfNeeded();

				var reference = CustomsReferenceNumbers.AddNew();
				reference.CE_EntryNum = newDeclaration.JE_DeclarationReference;
				reference.CE_EntryType = CustomsAdditionalReferenceNumbersCodes.DeclarationReference;

				LogStandAloneDeclarationCreated(newDeclaration.JE_DeclarationReference);

				newlyCreatedDeclarationPK = default;
			}
		}

		public void InvalidateScreeningStatuses()
		{
			ScreeningStatusUpdater.ProcessInvalidateLocalDataChanges(this, () => { });
			if (HVC_DeniedPartyScreeningStatusInfo.HasChanges && ManagingShipment is IShouldUpdateScreeningStatus shipment)
			{
				shipment.ShouldUpdateScreeningStatus = true;
			}
		}

		public void UpdateScreeningStatusFromParties()
		{
			if (HVC_DeniedPartyScreeningStatus != ScreeningStatusesList.Codes.NotScreened && HVC_DeniedPartyScreeningStatus != ScreeningStatusesList.Codes.PermanentClear)
			{
				using (SetIsRecalculatingDeniedPartyStatus())
				{
					ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(this);
				}
			}

			if (HVC_DeniedPartyScreeningStatusInfo.HasChanges && ManagingShipment is IShouldUpdateScreeningStatus shipment)
			{
				shipment.ShouldUpdateScreeningStatus = true;
			}
		}

		IDisposable SetIsRecalculatingDeniedPartyStatus()
		{
			isRecalculatingDeniedPartyStatus = true;
			return new DisposableAction(() => isRecalculatingDeniedPartyStatus = false);
		}

		bool isRecalculatingDeniedPartyStatus;

		public static void CalculateTotalProperties(BusinessObjectFactory factory)
		{
			var newItems = factory.GetAddedBusinessObjects<HVLVItem>();
			var newConsignments = factory.GetAddedBusinessObjects<HVLVConsignment>();

			var grouped = newConsignments.GroupJoin(newItems, consignment => consignment.PK,
				item => item.HVI_HVC_Consignment,
				(consignment, items) => new { Consignment = consignment, Items = items });

			CalculateConsignmentsTotals();

			void CalculateConsignmentsTotals()
			{
				foreach (var grouping in grouped)
				{
					var consignmentRow = ((INeedRow)grouping.Consignment).Row;
					consignmentRow[HVLVConsignmentSchema.HVC_ItemCount.Name] = (short)grouping.Items.Count();
					consignmentRow[HVLVConsignmentSchema.HVC_ManifestedWeight.Name] = grouping.Items.Sum(x => x.HVI_ManifestedWeight);
					consignmentRow[HVLVConsignmentSchema.HVC_ActualWeight.Name] = grouping.Items.Sum(x => x.HVI_ActualWeight);
					consignmentRow[HVLVConsignmentSchema.HVC_ManifestedVolume.Name] = grouping.Items.Sum(x => x.HVI_ManifestedVolume);
					consignmentRow[HVLVConsignmentSchema.HVC_ActualVolume.Name] = grouping.Items.Sum(x => x.HVI_ActualVolume);
				}
			}
		}

		void AllocateConsignmentIDsIfNeeded(IEnumerable<HVLVConsignment> consignments, INumberFountainProxy numberFountain)
		{
			var newConsignments = consignments.Where(consignment => consignment.HVC_ConsignmentId.IsEmpty);
			if (newConsignments.Any())
			{
				var consignmentIDs = new Queue<string>(numberFountain.GetNextsFormatted(Factory, consignments.Count()));
				consignments.ForEach(consignment =>
				{
					consignment.HVC_ConsignmentId = consignmentIDs.Dequeue();
					consignment.HVC_IsValidatedForUniqueness = true;
				});
			}
		}

		StmALog LogStandAloneDeclarationCreated(string declarationReference)
		{
			var eventType = IsImport ? AutoEvents.TransferToCustomsImportsDec : AutoEvents.TransferToCustomsExportsDec;
			var referenceParameters = new KeyValuePair<string, string>(EventReferenceConstants.EventReferenceParameters.Codes.ReferenceNumber, declarationReference);
			return Logs.AddNew(eventType, referenceParameters);
		}

		void DefaultReferencesForNewConsignment()
		{
			if (HVC_ConsignmentId.IsEmpty)
			{
				if (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.Value)
				{
					var consignments = Factory.GetAddedBusinessObjects<HVLVConsignment>(false);
					if (consignments.Any())
					{
						if (ConsignmentHeader?.GS1Info != null)
						{
							AllocateConsignmentIDsIfNeeded(consignments, ConsignmentHeader.GS1Info.SSCCNumberFountain);
						}
						else if (BookingHeader?.GS1Info != null)
						{
							AllocateConsignmentIDsIfNeeded(consignments, BookingHeader.GS1Info.SSCCNumberFountain);
						}
						else
						{
							AllocateConsignmentIDsIfNeeded(consignments, Env.NumberFountains.HVLVConsignmentId);
						}
					}
				}
				else
				{
					GenerateConsignmentID();
				}
			}

			if (HVC_WaybillNumber.IsEmpty)
			{
				HVC_WaybillNumber = HVC_ConsignmentId;
				waybillSetOnSaving = true;
			}
		}

		bool waybillSetOnSaving;

		HashSet<string> ConsignmentIDCache => ConsignmentHeader?.ConsignmentIDCache
				?? BookingHeader.ConsignmentIDCache;

		void GenerateConsignmentID()
		{
			var consignmentIdCache = ConsignmentIDCache;
			var newID = ConsignmentIdCandidates.First(x => !x.IsEmpty && !consignmentIdCache.Contains(x));
			consignmentIdCache.Add(newID);
			HVC_ConsignmentId = newID;
		}

		IEnumerable<ZString> ConsignmentIdCandidates
		{
			get
			{
				yield return HVC_WaybillNumber;
				yield return HVC_ShipperReference;

				var items = Items.Cast<HVLVItem>().ToArray();
				yield return items.Select(x => x.HVI_CurrentBarcode).FirstOrDefault();
				yield return items.Select(x => x.HVI_ShipperReference).FirstOrDefault();

				yield return ConsignmentHeader?.GS1Info?.GenerateSSCCNumber(Factory);
				yield return BookingHeader?.GS1Info?.GenerateSSCCNumber(Factory);
				yield return Env.NumberFountains.HVLVConsignmentId.GetNextFormatted(Factory);
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				PopulateUsageTableIfNeeded();
			}

			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					ConsignmentIDCache.Remove(HVC_ConsignmentId);
					HVC_ConsignmentId = ZString.Empty;
				}

				if (waybillSetOnSaving)
				{
					HVC_WaybillNumber = ZString.Empty;
					waybillSetOnSaving = false;
				}
			}
			else if (markedAsNeedingReload)
			{
				markedAsNeedingReload = false;
				displayHVC_ItemCount = null;
				displayHVC_ManifestedWeight = null;
				displayHVC_ManifestedVolume = null;
				displayHVC_ActualWeight = null;
				displayHVC_ActualVolume = null;
			}
		}

		ZShort? displayHVC_ItemCount;
		ZDecimal? displayHVC_ManifestedWeight;
		ZDecimal? displayHVC_ManifestedVolume;
		ZDecimal? displayHVC_ActualWeight;
		ZDecimal? displayHVC_ActualVolume;

		bool markedAsNeedingReload;

		internal bool HasManagingShipment => ManagingShipment != null;

		void PopulateUsageTableIfNeeded()
		{
			if (CargoWise.Data.DbRegistry.BiDisableChangeDataCapture.LoadValue(CargoWise.Data.Db.Connection))
			{
				if (IsInDatabase && HVC_IsActive)
				{
					PopulateUsageTableIfEditedByOtherCompany();
				}
			}
		}

		void PopulateUsageTableIfEditedByOtherCompany()
		{
			if (!InvalidUsers.Contains(HVC_SystemCreateUser) && !InvalidUsers.Contains(GlbStaff.CurrentUser.GS_Code))
			{
				var creatingBranch = Factory.GetBranchFromStaffCode(HVC_SystemCreateUser);
				var lastEditBranch = GlbBranch.CurrentBranch;

				if (creatingBranch != null && lastEditBranch != null)
				{
					var lastEditCompany = lastEditBranch.Company;
					var creatingCompany = creatingBranch.Company;

					if (lastEditCompany != null && creatingCompany != null && lastEditCompany.PK != creatingCompany.PK)
					{
						items.ForEach(item => ((HVLVItem)item).LogUsageWhenEditedByOtherCompany());
					}
				}
			}
		}

		public bool HasItemsNotOnManagingShipment
		{
			get
			{
				return HasManagingShipment &&
					Items.Cast<HVLVItem>()
					.Select(x => x.HVI_JS_LoadedOnShipment)
					.Distinct()
					.IsCountMoreThan(1);
			}
		}

		public override ZBool HVC_IsActive
		{
			get => base.HVC_IsActive;
			set
			{
				if (base.HVC_IsActive != value)
				{
					base.HVC_IsActive = value;
					SetItemActiveStatus(value);
				}
			}
		}

		public ZBool HasCustomsStatus => HVC_ImportReleaseStatus != HVLVReleaseStatus.None || HVC_ExportReleaseStatus != HVLVReleaseStatus.None;

		public override string CanCancel() => HasCustomsStatus ? CannotDeactivateConsignmentsHavingCustomsStatusMessage : base.CanCancel();

		public string CannotDeactivateConsignmentsHavingCustomsStatusMessage => Res.GetString("096d9999-376f-4861-b169-802a517311f7", "Cannot deactivate Consignment {0} as it has a Customs release status.", HVC_ConsignmentId);

		protected bool HVC_IsActive_ReadOnly => !IsInDatabase;

		void SetItemActiveStatus(bool isActive)
		{
			Items.OfType<HVLVItem>().ForEach(item => item.HVI_IsActive = isActive);
		}

		public override void Delete()
		{
			Items.DeleteAll();
			WorkflowItems.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Customs Reference Numbers

		[ChildEditable(true)]
		public CusEntryNumAdditionalReferenceCollection CustomsReferenceNumbers
		{
			get
			{
				if (customsReferenceNumbers == null)
				{
					customsReferenceNumbers = new CusEntryNumAdditionalReferenceCollection(this);
					customsReferenceNumbers.Load();
					RegisterEditableChildObject(customsReferenceNumbers);
				}

				return customsReferenceNumbers;
			}
		}

		CusEntryNumAdditionalReferenceCollection customsReferenceNumbers;

		#endregion

		#region TransactionParticipant

		void RegisterTransactionParticipant()
		{
			HVLVConsignmentTransactionParticipant.Register(this);
		}

		#endregion

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter => new HVLVConsignmentDocumentSupporter(this);

		#endregion

		#region Transport Company Label

		public ZString LocalTransportCompanyLabel
		{
			get
			{
				var result = ZString.Empty;

				var localTransportCompanies = DocumentsDataRegistry.Instance.LocalTransportCompanyBrand.Value;

				foreach (LocalTransportCompanyBranding localTransportCompany in localTransportCompanies)
				{
					if (localTransportCompany.LocalTransportCompanyPK == HVC_OH_LastMileCarrier)
					{
						result = localTransportCompany.LabelName;
					}
				}

				return result;
			}
		}

		#endregion

		#region IEDocsProvider

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = GetNewDocManagerInfo();
				}

				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		protected virtual HVLVConsignmentDocManagerInfo GetNewDocManagerInfo()
		{
			return new HVLVConsignmentDocManagerInfo(this);
		}

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new EDocsProviderSupporter(this);
		}

		#endregion

		#region Direction of Trade

		public ZBool IsImport => DirectionOfTrade == Directions.Import;

		public ZBool IsExport => DirectionOfTrade == Directions.Export;

		public ZBool ShowExport => IsExport || (!IsImport && HVC_ImportCustomsClearanceStatus.IsDefault && !HVC_ExportCustomsClearanceStatus.IsDefault);

		public ZBool ShowImport => !ShowExport;

		public Directions DirectionOfTrade
		{
			get
			{
				var direction = ImportExportHelper.GetJobDirection(OriginCountry, DestinationCountry);
				return direction;
			}
		}

		public ZString OriginCountry
		{
			get
			{
				var originCountry = HVC_RN_NKShipperCountryCode;
				if (string.IsNullOrEmpty(originCountry))
				{
					originCountry = GetCountryCodeFromShipment(shipment => shipment.Origin?.RL_RN_NKCountryCode);
				}

				if (string.IsNullOrEmpty(originCountry))
				{
					originCountry = BookingHeader?.BillToParty?.OA_RN_NKCountryCode ?? ZString.Empty;
				}

				return originCountry;
			}
		}

		public ZString DestinationCountry
		{
			get
			{
				var destinationCountry = HVC_RN_NKConsigneeCountryCode;
				if (string.IsNullOrEmpty(destinationCountry))
				{
					destinationCountry = GetCountryCodeFromShipment(shipment => shipment.Destination?.RL_RN_NKCountryCode);
				}

				if (string.IsNullOrEmpty(destinationCountry))
				{
					destinationCountry = DestinationDepot?.OA_RN_NKCountryCode ?? ZString.Empty;
				}

				return destinationCountry;
			}
		}

		string GetCountryCodeFromShipment(Func<ForwardingShipment, string> countryCodeGetter)
		{
			var result = string.Empty;
			var shipment = HasManagingShipment ? ManagingShipment : ManifestedOnShipment;
			if (shipment != null)
			{
				result = countryCodeGetter(shipment);
			}

			return result;
		}

		#endregion

		#region StandAlone Declaration

		public ZBool HasDeclarationForCurrentDirection
		{
			get
			{
				var result = false;
				if (IsImport)
				{
					result = !HVC_JE_ImportDeclaration.IsEmpty;
				}
				else if (IsExport)
				{
					result = !HVC_JE_ExportDeclaration.IsEmpty;
				}
				return result;
			}
		}

		public ZBool CanConvertToStandAloneDeclaration => ConsignmentHeader != null && ReleaseStatusForCurrentDirection != HVLVReleaseStatus.Cleared && (IsImport || IsExport) && !HasDeclarationForCurrentDirection;

		public ZBool HasStandAloneDeclarationForCurrentDirection => StandAloneDeclarationForCurrentCompany != null;

		#endregion

		#region IDtbBookingParent Members

		void IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
		}

		ZBool IDtbBookingParent.IsSupportsDirectSchedule => false;

		ZString IDtbBookingParent.JobType => ((IWorkflowProvider)this).WorkflowType;

		DtbBookingDirection[] IDtbBookingParent.GetSupportedDirections() => new DtbBookingDirection[] { DtbBookingDirection.DLV };

		IJobInvoicingPlugIn IDtbBookingParent.InvoicingJob => null;

		ZQuery IDtbBookingParent.TransportBookingTemplateFilters => new ZQuery();

		ZString IDtbBookingParent.JobTypeDescription => HumanReadableNameWithoutId;

		bool IDtbBookingParent.RequiresMultiContainerBooking => true;

		bool IDtbBookingParent.CanCreateTransportBooking => true;

		ZGuid IDtbBookingParent.BookingParentPK => PK;

		string IDtbBookingParent.BookingParentTablePrefix => TablePrefix;

		(bool isShouldShow, string caption, string message, string confirmation) IDtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.HVLVConsignment;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobNumber => HVC_ConsignmentId;

		ZString IRelatedJob.JobDescription => HumanReadableNameWithoutId;

		ZString IRelatedJob.JobStatus => HVC_Status;

		#endregion

		#region IEManifestLine Members

		ZGuid IEManifestLine.PK => PK;

		ZString IEManifestLine.Reference => HVC_WaybillNumber;

		ZInt IEManifestLine.PackCount => HVC_ItemCount;

		ZString IEManifestLine.CountryOfDestination => HVC_RN_NKConsigneeCountryCode;

		ZString IEManifestLine.GoodsOwner => HVC_ShipperName;

		ZString IEManifestLine.GoodsDescription => HVC_GoodsDescription;

		#endregion

		#region ICusCodeDataTypeSupporter Members

		public IDictionary<ZString, Type> GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ Customs.Common.AU.CusCodeDataTypeList.Codes.CustomsManifestLineSequence, ObjectFactory.GetType<AU.ICustomsManifestLineSequence>() } // TODO: Delete HVLVConsignmentTest.TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed when ICustomsManifestLineSequence is removed
			};
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return (IBusinessObjectFetchStrategy)Activator.CreateInstance(ObjectFactory.GetType<ICusCodeDataTypeSupporterFetchStrategy>(), this);
		}

		#endregion

		#region IDpsEntityProvider Members

		ZBool IDpsEntityProvider.NeedsScreening => HVC_DeniedPartyScreeningStatus != ScreeningStatusesList.Codes.NotScreened && HVC_DeniedPartyScreeningStatus != ScreeningStatusesList.Codes.Unknown;

		ZBool IDpsEntityProvider.ShouldUpdateRelatedJobs { get; set; }

		ZString IDpsEntityProvider.OriginalScreeningStatus => HVC_DeniedPartyScreeningStatusInfo.OriginalValue.ToString();

		public void InvalidateByLocalDataChanges() { }

		#endregion

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			var result = false;
			if (!HVC_IsActive && property.Name != Schema.HVC_IsActive)
			{
				result = true;
			}

			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		public void RefreshBindingForLMCProperties()
		{
			HVC_OA_DestinationDepot_ZAddress.SetOrgWithoutSettingDefaultAddress(DestinationDepot?.PK ?? ZGuid.Empty);
			HVC_OA_DestinationDepotInfo.RefreshBinding();
			HVC_OH_LastMileCarrierInfo.RefreshBinding();
			HVC_PL_NKLastMileCarrierServiceLevelInfo.RefreshBinding();
		}

		void RefreshBindingForOrgAddresses()
		{
			HVC_OA_ConsigneeAddressInfo.RefreshBinding();
			HVC_OA_ShipperAddressInfo.RefreshBinding();
		}

		protected override void OnUpdatedByDataRefresh()
		{
			RefreshBindingForLMCProperties();
			RefreshBindingForOrgAddresses();
		}

		ZGuid HVLVConsignment_GetDefaultAddress(IOrgHeader orgHeader)
		{
			var result = ZGuid.Empty;
			var organisation = orgHeader as OrgHeader;
			if (organisation != null)
			{
				var addresses = organisation.AddressesActive;
				if (addresses.Count == 1)
				{
					result = addresses[0].PK;
				}
			}

			return result;
		}

		void UpdateRelatedConsignmentsCustomsClearanceStatus(ZString newStatus, ZGuid declarationPK, bool isExport)
		{
			if (!declarationPK.IsEmpty)
			{
				var declarationPKSchema = isExport ? HVLVConsignmentSchema.HVC_JE_ExportDeclaration : HVLVConsignmentSchema.HVC_JE_ImportDeclaration;
				var relatedConsignmentQuery = new ZQuery(declarationPKSchema, declarationPK);
				relatedConsignmentQuery.AddToFilter(JoinCondition.And, HVLVConsignmentSchema.PK, SQLComparisonOperator.NotEqual, PK);
				var relatedHVLVConsignments = Factory.Load<HVLVConsignment>(relatedConsignmentQuery);
				if (relatedHVLVConsignments is { Length: > 0 })
				{
					foreach (var consignment in relatedHVLVConsignments)
					{
						if (isExport)
						{
							consignment.HVC_ExportCustomsClearanceStatus = newStatus;
						}
						else
						{
							consignment.HVC_ImportCustomsClearanceStatus = newStatus;
						}
					}
				}
			}
		}

		public void ClearPreScreeningStatus()
		{
			if (HVC_PreScreeningStatus != HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown)
			{
				HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown;
			}
		}

		#region IHVLVConsignmentFHLMessageDetailsProvider Members

		public IFHLMessageDetailsProvider GetFHLMessageDetailsProvider() => new HVLVConsignmentFHLMessageDetailsProvider(this);

		void IHVLVSecurityFilingSource.SetSecurityFilingUsageDateTimeIfNeeded()
		{
			var currentDateTime = ZDateTime.UtcNow;
			Items.Cast<HVLVItem>().ForEach(item =>
			{
				if (item.HVI_SecurityFilingFirstUsageTimeUtc.IsEmpty)
				{
					item.HVI_SecurityFilingFirstUsageTimeUtc = currentDateTime;
				}
			});
		}

		#endregion

		#region ACAS

		public ZString ACASReportGoodsDescription
		{
			get
			{
				var itemLines = Items.OfType<HVLVItem>().SelectMany(item => item.Lines).OfType<HVLVItemLine>();

				var goodsDescription = string.Join(", ", itemLines.Select(line => line.HVS_GoodsDescription));
				if (string.IsNullOrEmpty(goodsDescription))
				{
					goodsDescription = HVC_GoodsDescription;
				}

				return goodsDescription;
			}
		}

		public ZBool NeedACASAmendment => HVC_ACASStatus.Equals(ACASActions.Code.SelecteeDataIssueHold)
			&& HVC_ACASMessageStatus.Equals(HVLVACASMessageStatusList.Codes.AmendmentRequired);

		#endregion

		#region IHVLVConsignment

		IHVLVBookingHeader IHVLVConsignmentForDocument.BookingHeader => BookingHeader;

		IOrgAddress IHVLVConsignmentForDocument.ConsigneeAddress => ConsigneeAddress;

		IRefCountry IHVLVConsignmentForDocument.ConsigneeCountryCode => ConsigneeCountryCode;

		IOrgAddress IHVLVConsignmentForDocument.DestinationDepot => DestinationDepot;

		IOrgAddress IHVLVConsignmentForDocument.ShipperAddress => ShipperAddress;

		IRefCurrency IHVLVConsignmentForDocument.GoodsValueCurrency => GoodsValueCurrency;

		IOrgHeader IHVLVConsignmentForDocument.LastMileCarrier => LastMileCarrier;

		IOrgHeader IHVLVConsignmentForDocument.LastMileCarrierBookingAgent => LastMileCarrierBookingAgent;

		IRefCountry IHVLVConsignmentForDocument.ShipperCountryCode => ShipperCountryCode;

		IHVLVConsignmentCollection IHVLVConsignmentForDocument.ConsignmentsBelongToSameConsignee => ConsignmentsBelongToSameConsignee;

		IHVLVConsignmentCollection IHVLVConsignmentForDocument.ConsignmentsBelongToSameConsigneeExcludingParent => ConsignmentsBelongToSameConsigneeExcludingParent;

		IHVLVConsignmentCollection IHVLVConsignmentForDocument.FormerConsignments => FormerConsignments;
		IHVLVConsignmentCollection IHVLVConsignmentForDocument.ReturnConsignments => ReturnConsignments;

		IProcessTaskCollection IHVLVConsignmentForDocument.WorkflowItems => WorkflowItems;

		IEnumerable<IHVLVItemForDocument> IHVLVConsignmentForDocument.ActiveItems => ActiveItems;

		Enterprise.Integration.Forwarding.IForwardingShipment IHVLVConsignmentForDocument.ManagingShipment => ManagingShipment;

		Enterprise.Integration.Forwarding.IForwardingShipment IHVLVConsignmentForDocument.ManifestedOnShipment => ManifestedOnShipment;

		IBaseJobDeclaration IHVLVConsignmentForDocument.ImportDeclaration => ImportDeclaration;

		IBaseJobDeclaration IHVLVConsignmentForDocument.ExportDeclaration => ExportDeclaration;

		ICusEntryNumAdditionalReferenceCollection IHVLVConsignmentForDocument.CustomsReferenceNumbers => CustomsReferenceNumbers;

		IHVLVItemCollection IHVLVConsignment.Items => Items;

		#endregion

		#region IScreeningPartyProvider

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get => HVC_DeniedPartyScreeningStatus;
			set
			{
				if (BookingHeader != null && ConsignmentHeader == null)
				{
					if (BookingHeader.HVH_DeniedPartyScreeningStatus != ScreeningStatusesList.Codes.Matched)
					{
						if (value != HVC_DeniedPartyScreeningStatus)
						{
							HVC_DeniedPartyScreeningStatus = value;
							SetLastUsageCodeForAllItems(UsageCodes.DeniedPartyScreening);
						}
					}
				}
				else if (value != base.HVC_DeniedPartyScreeningStatus)
				{
					HVC_DeniedPartyScreeningStatus = value;
					SetLastUsageCodeForAllItems(UsageCodes.DeniedPartyScreening);
				}
			}
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatus()
		{
			var statuses = new List<ZString>();
			GetScreeningStatus(statuses);
			return ScreeningStatusUpdater.GetWorstScreeningStatus(statuses);
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared()
		{
			return HVC_DeniedPartyScreeningStatus == ScreeningStatusesList.Codes.JobCleared
				? ScreeningStatusesList.Codes.JobCleared
				: (this as IScreeningPartyProvider).GetWorstScreeningStatus();
		}

		void GetScreeningStatus(List<ZString> statuses)
		{
			AddScreeningStatusToList(statuses, ShipperAddress?.Header);
			AddScreeningStatusToList(statuses, ConsigneeAddress?.Header);
			AddScreeningStatusToList(statuses, DestinationDepot?.Header);
			AddScreeningStatusToList(statuses, ReturnLocation?.Header);
			AddScreeningStatusToList(statuses, LastMileCarrier);
			AddScreeningStatusToList(statuses, LastMileCarrierBookingAgent);
			if (!isRecalculatingDeniedPartyStatus)
			{
				statuses.Add(HVC_DeniedPartyScreeningStatus);
			}
			else if (isRecalculatingDeniedPartyStatus && (!ShipperIsOrganisation || !ConsigneeIsOrganisation || !ReturnIsOrganisation))
			{
				statuses.Add(ScreeningStatusesList.Codes.Unknown);
			}
		}

		void AddScreeningStatusToList(List<ZString> list, IScreeningPartyProvider party)
		{
			var status = party?.GetWorstScreeningStatus();
			AddScreeningStatusToList(list, status);
		}

		void AddScreeningStatusToList(List<ZString> list, ZString? status)
		{
			if (status.HasValue)
			{
				list.Add(status.Value);
			}
		}

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties => GetScreeningEntities().ToArray();

		IEnumerable<ScreeningParty> GetScreeningEntities()
		{
			var shipperCaption = Res.GetString("9fcc140a-4eaa-46fe-8fd4-f008a746885d", "Shipper");
			if (!HVC_OA_ShipperAddress.IsEmpty)
			{
				yield return new ScreeningParty(this, shipperCaption, ShipperAddress.Header);
			}
			else
			{
				yield return new ScreeningParty(this, shipperCaption, HVC_ShipperName, HVC_ShipperAddress1, HVC_ShipperAddress2, HVC_ShipperCity, HVC_ShipperPostcode, HVC_ShipperState, HVC_RN_NKShipperCountryCode, ZString.Empty, true);
			}

			var consigneeCaption = Res.GetString("bb08fd0b-3405-4aa8-86e2-a3a378726666", "Consignee");
			if (!HVC_OA_ConsigneeAddress.IsEmpty)
			{
				yield return new ScreeningParty(this, consigneeCaption, ConsigneeAddress.Header);
			}
			else
			{
				yield return new ScreeningParty(this, consigneeCaption, HVC_ConsigneeName, HVC_ConsigneeAddress1, HVC_ConsigneeAddress2, HVC_ConsigneeCity, HVC_ConsigneePostcode, HVC_ConsigneeState, HVC_RN_NKConsigneeCountryCode, ZString.Empty);
			}

			if (!HVC_OA_DestinationDepot.IsEmpty)
			{
				yield return new ScreeningParty(this, Res.GetString("a288ebae-d9e4-4854-b1ea-626b44d19d40", "Destination Depot"), DestinationDepot.Header);
			}

			var returnLocationCaption = Res.GetString("7c65de17-5fb6-405f-9fe5-b079fc18519d", "Return Location");
			if (!HVC_OA_ReturnLocation.IsEmpty)
			{
				yield return new ScreeningParty(this, returnLocationCaption, ReturnLocation.Header);
			}
			else
			{
				yield return new ScreeningParty(this, returnLocationCaption, HVC_ReturnName, HVC_ReturnAddress1, HVC_ReturnAddress2, HVC_ReturnCity, HVC_ReturnPostcode, HVC_ReturnState, HVC_RN_NKReturnCountryCode, ZString.Empty);
			}

			if (!HVC_OH_LastMileCarrier.IsEmpty)
			{
				yield return new ScreeningParty(this, Res.GetString("1f5d1d95-1e3c-43b2-841a-5cd084d15fbb", "Last Mile Carrier"), LastMileCarrier);
			}

			if (!HVC_OH_LastMileCarrierBookingAgent.IsEmpty)
			{
				yield return new ScreeningParty(this, Res.GetString("0a34877e-a7b6-4c1a-9658-d321ff97188a", "Last Mile Carrier Booking Agent"), LastMileCarrierBookingAgent);
			}
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (uniqueIndexFailureHandlers == null)
				{
					uniqueIndexFailureHandlers = new List<IUniqueIndexFailureHandler>();
					uniqueIndexFailureHandlers.Add(new HVLVConsignmentShipperReferenceUniqueIndexFailureHandler(this));
					uniqueIndexFailureHandlers.Add(new HVLVConsignmentWaybillNumberUniqueIndexFailureHandler(this));
					uniqueIndexFailureHandlers.Add(new HVLVConsignmentIdUniqueIndexFailureHandler(this));
				}

				return uniqueIndexFailureHandlers;
			}
		}

		List<IUniqueIndexFailureHandler> uniqueIndexFailureHandlers;

		class HVLVConsignmentShipperReferenceUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public HVLVConsignmentShipperReferenceUniqueIndexFailureHandler(HVLVConsignment consignment)
			{
				Consignment = consignment;
			}
			readonly HVLVConsignment Consignment;

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get
				{
					yield return HVLVConsignmentSchema.Constants.Indexes.NR_UX__HVC_ShipperReference_HVC_ClusterKey;
				}
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier?.ReportError(Res.GetString("2e554b0c-7d17-4a11-b691-c9426723dd72", "Error saving record. Consignment Shipper Reference must be unique on the Shipment or Booking Header. The duplicate value is ({0}).", Consignment.HVC_ShipperReference), Res.GetString("bb5e6ab7-511f-4223-8bac-43d1cffaefbc", "Duplicate Consignment Shipper Reference"));
			}
		}

		class HVLVConsignmentWaybillNumberUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public HVLVConsignmentWaybillNumberUniqueIndexFailureHandler(HVLVConsignment consignment)
			{
				Consignment = consignment;
			}
			readonly HVLVConsignment Consignment;

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get
				{
					yield return HVLVConsignmentSchema.Constants.Indexes.NR_UX__HVC_WaybillNumber_HVC_ClusterKey;
				}
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier?.ReportError(Res.GetString("3ad3bf6e-72f4-45b1-bdcd-6009cb5a176f", "Error saving record. Consignment Waybill Number must be unique on the Shipment or Booking Header. The duplicate value is ({0}).", Consignment.HVC_WaybillNumber), Res.GetString("101b5148-4779-434d-9c28-b67a0cee1379", "Duplicate Consignment Waybill Number"));
			}
		}

		class HVLVConsignmentIdUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public HVLVConsignmentIdUniqueIndexFailureHandler(HVLVConsignment consignment)
				: base(HVLVConsignmentSchema.Constants.Indexes.NR_UX__HVC_ConsignmentId, consignment)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get
				{
					var consignment = BizObjCausingError as HVLVConsignment;

					var gs1Info = consignment.ConsignmentHeader?.GS1Info ?? consignment.BookingHeader?.GS1Info;
					var usingSSCCFountainThatWeCannotFix = gs1Info != null;
					if (!usingSSCCFountainThatWeCannotFix)
					{
						var theFallbackFountainForSSCCFountain = Env.NumberFountains.HVLVConsignmentId;
						return theFallbackFountainForSSCCFountain;
					}

					return null;
				}
			}
		}

		#endregion

		#region IAuditParent

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren => new AuditChildInfo[]
		{
			new AuditChildInfo(HVLVItemSchema.HVI_HVC_Consignment, HVLVItemSchema.HVI_ItemId)
		};

		#endregion

		#region IHVLVISFBillInfoProvider
		
		ICusEntryNumAdditionalReferenceCollection IHVLVISFBillInfoProvider.CustomsReferenceNumbers => CustomsReferenceNumbers;

		#endregion

		#region IHVLVPrescreeningDataProvider

		string IHVLVPrescreeningDataProvider.ETailerOrgCode => ManifestedOnShipment?.ConsignorDocumentaryAddress?.Organisation?.OH_Code ?? BookingHeader?.BillToParty?.Header?.OH_Code ?? ZString.Empty;

		BusinessObject IHVLVPrescreeningDataProvider.Entity => this;

		IEnumerable<IHVLVConsignment> IHVLVPrescreeningDataProvider.Consignments => [this];

		string IHVLVPrescreeningDataProvider.TableCode => ManifestedOnShipment == null ? HVLVBookingHeaderSchema.Constants.Prefix : JobShipmentSchema.Constants.Prefix;

		Logs IHVLVPrescreeningDataProvider.Logs => ManifestedOnShipment?.Logs ?? BookingHeader.Logs;

		#endregion

		#region IConsignmentAddressProvider

		BusinessObjectFactory IConsignmentAddressProvider.Factory => Factory;

		string IConsignmentAddressProvider.WaybillNumber => HVC_WaybillNumber;

		bool IConsignmentAddressProvider.ConsigneeIsOrganisation => ConsigneeIsOrganisation;
		bool IConsignmentAddressProvider.ShipperIsOrganisation => ShipperIsOrganisation;

		ZGuid IConsignmentAddressProvider.ConsigneeAddressId
		{
			get => HVC_OA_ConsigneeAddress;
			set => HVC_OA_ConsigneeAddress = value;
		}

		ZGuid IConsignmentAddressProvider.ShipperAddressId
		{
			get => HVC_OA_ShipperAddress;
			set => HVC_OA_ShipperAddress = value;
		}

		string IConsignmentAddressProvider.ConsigneeName => HVC_ConsigneeName;
		string IConsignmentAddressProvider.ConsigneeAddress1 => HVC_ConsigneeAddress1;
		string IConsignmentAddressProvider.ConsigneeAddress2 => HVC_ConsigneeAddress2;
		string IConsignmentAddressProvider.ConsigneeCity => HVC_ConsigneeCity;
		string IConsignmentAddressProvider.ConsigneeState => HVC_ConsigneeState;
		string IConsignmentAddressProvider.ConsigneePostcode => HVC_ConsigneePostcode;
		string IConsignmentAddressProvider.ConsigneeCountryCode => HVC_RN_NKConsigneeCountryCode;
		string IConsignmentAddressProvider.ConsigneePhone => HVC_ConsigneePhone;
		string IConsignmentAddressProvider.ConsigneeMobile => HVC_ConsigneeMobile;
		string IConsignmentAddressProvider.ConsigneeFax => HVC_ConsigneeFax;
		string IConsignmentAddressProvider.ConsigneeEmail => HVC_ConsigneeEmail;

		string IConsignmentAddressProvider.ShipperName => HVC_ShipperName;
		string IConsignmentAddressProvider.ShipperAddress1 => HVC_ShipperAddress1;
		string IConsignmentAddressProvider.ShipperAddress2 => HVC_ShipperAddress2;
		string IConsignmentAddressProvider.ShipperCity => HVC_ShipperCity;
		string IConsignmentAddressProvider.ShipperState => HVC_ShipperState;
		string IConsignmentAddressProvider.ShipperPostcode => HVC_ShipperPostcode;
		string IConsignmentAddressProvider.ShipperCountryCode => HVC_RN_NKShipperCountryCode;
		string IConsignmentAddressProvider.ShipperPhone => HVC_ShipperPhone;
		string IConsignmentAddressProvider.ShipperMobile => HVC_ShipperMobile;
		string IConsignmentAddressProvider.ShipperFax => HVC_ShipperFax;
		string IConsignmentAddressProvider.ShipperEmail => HVC_ShipperEmail;

		void IConsignmentAddressProvider.RefreshBinding()
		{
			RefreshBindingForOrgAddresses();
		}

		#endregion

#if DEBUG

		public HashSet<string> ConsignmentIDCache_Exposed => ConsignmentIDCache;

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new HVLVConsignmentTestDataHelper();
		}

		class HVLVConsignmentTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateUniqueStringProperties(BusinessObject bizObj, PropertyDescriptor[] propertyPath)
			{
				base.PopulateUniqueStringProperties(bizObj, propertyPath);

				foreach (var propertyName in ConditionallyUniqueStringProperties)
				{
					var propertyInfo = GetPropertyInfo(bizObj, propertyName);

					if (propertyName == HVLVConsignmentSchema.HVC_WaybillNumber.Name)
					{
						PopulateUniqueString(propertyInfo, propertyPath, 12);
					}
					else
					{
						PopulateUniqueString(propertyInfo, propertyPath);
					}
				}
			}

			IEnumerable<ZString> ConditionallyUniqueStringProperties
			{
				get
				{
					yield return HVLVConsignmentSchema.HVC_WaybillNumber.Name;
					yield return HVLVConsignmentSchema.HVC_ShipperReference.Name;
				}
			}
		}

#endif
	}
}
