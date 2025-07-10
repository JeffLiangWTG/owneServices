using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using IContainer = Enterprise.Freight.Business.IContainer;

namespace Enterprise.eTail.Business
{
	[UniversalDataContext(DataContextType.HVLVBookingHeader), CodeProperty(Schema.HVH_BookingReference)]
	public class HVLVBookingHeader :
		AutoHVLVBookingHeader,
		IHVLVBookingHeader,
		IWorkflowProvider,
		IJobNumber,
		IHVLVConsignmentCollectionParent,
		IEDocsProvider,
		IDocumentSupportable,
		IDtbBookingParent,
		IRelatedJob,
		IShipmentWithDocsAndCartage,
		IScreeningPartyProvider,
		IDpsEntityProvider,
		IAuditParent,
		IHVLVPrescreeningDataProvider
	{
		public HVLVBookingHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			HVH_GrossWeightUQ = Weight.Kilograms;
			HVH_GrossVolumeUQ = Volume.CubicMetres;
		}

		#endregion

		#region Properties

		public override ZInt HVH_ClusterKey
		{
			get { return base.HVH_ClusterKey; }
			set
			{
				Consignments.ForEach(x => ((HVLVConsignment)x).HVC_ClusterKey = value);
				base.HVH_ClusterKey = value;
			}
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(HVLVBookingHeaderLookups.DeniedPartyScreeningStatusList))]
		public override ZString HVH_DeniedPartyScreeningStatus
		{
			get => base.HVH_DeniedPartyScreeningStatus;
			set => base.HVH_DeniedPartyScreeningStatus = value;
		}

		public ZInt HVH_ItemCount
		{
			get
			{
				if (markedForItemCountRecalculation)
				{
					hvh_ItemCount = AllItems.Where(i => i.HVI_IsActive).Count();
					markedForItemCountRecalculation = false;
				}

				return hvh_ItemCount;
			}
		}

		ZInt hvh_ItemCount;

		public ZPropertyInfo HVH_ItemCountInfo => GetZPropertyInfo(nameof(HVH_ItemCount));

		public ZDecimal HVH_GrossWeight
		{
			get
			{
				if (markedForWeightRecalculation)
				{
					var subtotalByUQ = AllItems.Where(i => i.HVI_IsActive)
					.GroupBy(i => i.Consignment.HVC_WeightUQ)
					.Select(g => new
					{
						UQ = g.Key,
						Subtotal = g.Sum(i => i.HVI_ActualWeight > 0 ? i.HVI_ActualWeight : i.HVI_ManifestedWeight),
					}).ToList();

					hvh_GrossWeight = subtotalByUQ.Select(s => Weight.ConvertSafe(s.Subtotal, s.UQ, HVH_GrossWeightUQ)).Sum();
					markedForWeightRecalculation = false;
				}

				return hvh_GrossWeight;
			}
		}

		ZDecimal hvh_GrossWeight;

		public ZPropertyInfo HVH_GrossWeightInfo => GetZPropertyInfo(nameof(HVH_GrossWeight));

		[List("Lookups.WeightUQList")]
		public override ZString HVH_GrossWeightUQ
		{
			get { return base.HVH_GrossWeightUQ; }
			set
			{
				var upperCaseValue = value.ToUpper();
				if (upperCaseValue != HVH_GrossWeightUQ)
				{
					var originalUQ = HVH_GrossWeightUQ;
					base.HVH_GrossWeightUQ = upperCaseValue;
					UpdateGrossWeightIfNeeded(upperCaseValue, originalUQ);
				}
			}
		}

		void UpdateGrossWeightIfNeeded(ZString newUQ, ZString oldUQ)
		{
			var uqList = Lookups.WeightUQList;
			if (uqList.ContainsCode(newUQ))
			{
				if (uqList.ContainsCode(oldUQ))
				{
					hvh_GrossWeight = Weight.ConvertSafe(hvh_GrossWeight, oldUQ, newUQ);
					HVH_GrossWeightInfo.RefreshBinding();
				}
				else
				{
					MarkForWeightRecalculation();
				}
			}
		}

		public ZDecimal HVH_GrossVolume
		{
			get
			{
				if (markedForVolumeRecalculation)
				{
					var subtotalByUQ = AllItems.Where(i => i.HVI_IsActive)
						.GroupBy(i => i.Consignment.HVC_VolumeUQ)
						.Select(g => new
						{
							UQ = g.Key,
							Subtotal = g.Sum(i => i.HVI_ActualVolume > 0 ? i.HVI_ActualVolume : i.HVI_ManifestedVolume),
						}).ToList();

					hvh_GrossVolume = subtotalByUQ.Select(s => Volume.ConvertSafe(s.Subtotal, s.UQ, HVH_GrossVolumeUQ)).Sum();
					markedForVolumeRecalculation = false;
				}

				return hvh_GrossVolume;
			}
		}

		ZDecimal hvh_GrossVolume;

		public ZPropertyInfo HVH_GrossVolumeInfo => GetZPropertyInfo(nameof(HVH_GrossVolume));

		[List("Lookups.VolumeUQList")]
		public override ZString HVH_GrossVolumeUQ
		{
			get { return base.HVH_GrossVolumeUQ; }
			set
			{
				var upperCaseValue = value.ToUpper();
				if (upperCaseValue != HVH_GrossVolumeUQ)
				{
					var originalUQ = HVH_GrossVolumeUQ;
					base.HVH_GrossVolumeUQ = upperCaseValue;
					UpdateGrossVolumeIfNeeded(upperCaseValue, originalUQ);
				}
			}
		}

		void UpdateGrossVolumeIfNeeded(ZString newUQ, ZString oldUQ)
		{
			var uqList = Lookups.VolumeUQList;
			if (uqList.ContainsCode(newUQ))
			{
				if (uqList.ContainsCode(oldUQ))
				{
					hvh_GrossVolume = Volume.ConvertSafe(hvh_GrossVolume, oldUQ, newUQ);
					HVH_GrossVolumeInfo.RefreshBinding();
				}
				else
				{
					MarkForVolumeRecalculation();
				}
			}
		}

		[ResourceStringData("HVLVBookingHeader|BookingStatus", Caption = "Booking Status")]
		[List("Lookups.BookingStatusList")]
		[MaxLength(3)]
		[BusinessObjectTestExclude]
		public ZString BookingStatus
		{
			get
			{
				return HVH_IsBookingConfirmed ? HVLVBookingStatus.ConfirmedCode : HVLVBookingStatus.BookedCode;
			}
			set
			{
				var bookingConfirmed = value == HVLVBookingStatus.ConfirmedCode;
				if (HVH_IsBookingConfirmed != bookingConfirmed)
				{
					HVH_IsBookingConfirmed = bookingConfirmed;
				}

				BookingStatusInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BookingStatusInfo => GetZPropertyInfo(nameof(BookingStatus));

		protected bool BookingStatus_ReadOnly => HVH_IsBookingReceived;

		protected bool HVH_IsBookingReceived_ReadOnly => !HVH_IsBookingConfirmed;

		public override ZBool HVH_IsBookingConfirmed
		{
			get => base.HVH_IsBookingConfirmed;
			set
			{
				base.HVH_IsBookingConfirmed = value;
				if (HVH_IsBookingConfirmed)
				{
					Consignments.Cast<HVLVConsignment>().ForEach(c => c.HVC_Status = HVLVConsignmentStatus.Codes.Confirmed);
				}
				else
				{
					Consignments.Cast<HVLVConsignment>().ForEach(c => c.HVC_Status = HVLVConsignmentStatus.Codes.Booked);
				}
			}
		}

		public override ZGuid HVH_OA_BillToParty
		{
			get { return base.HVH_OA_BillToParty; }
			set
			{
				var billToPartyHeader = BillToParty?.Header;
				base.HVH_OA_BillToParty = value;

				if (billToPartyHeader != BillToParty?.Header)
				{
					var billToContacts = BillToParty?.Header?.Contacts;
					HVH_OC_BookedBy = billToContacts != null && billToContacts.Count == 1 ? billToContacts[0].PK : ZGuid.Empty;
					OnBillToPartyChanged();
				}
			}
		}

		public event EventHandler BillToPartyChanged;

		void OnBillToPartyChanged()
		{
			BillToPartyChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override ZAddress GetNewHVH_OA_BillToParty_ZAddress()
		{
			var result = base.GetNewHVH_OA_BillToParty_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		protected override ZAddress GetNewHVH_OA_DispatchAddress_ZAddress()
		{
			var result = base.GetNewHVH_OA_DispatchAddress_ZAddress();
			result.DefaultAddressType = AddressType.PIC;
			return result;
		}

		protected override ZAddress GetNewHVH_OA_OriginDepot_ZAddress()
		{
			var result = base.GetNewHVH_OA_OriginDepot_ZAddress();
			result.DefaultAddressType = AddressType.PIC;
			return result;
		}

		#endregion Properties

		#region Load / Save / Delete

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				if (HVH_ClusterKey.IsEmpty)
				{
					HVH_ClusterKey = (int)Env.NumberFountains.HVLVConsignmentClusterKey.GetNext(Factory);
				}

				if (HVH_BookingReference.IsEmpty)
				{
					HVH_BookingReference = Env.NumberFountains.HVLVBookingHeaderJobNumber.GetNextFormatted(Factory);
				}

				if (!IsInDatabase)
				{
					if (HVH_IsBookingConfirmed)
					{
						Logs.AddNew(AutoEvents.ManifestSubmittedToForwarder);
					}
				}
				else
				{
					if (HVH_IsBookingConfirmedInfo.HasChanges)
					{
						var lastNonCancelledMSFLog = Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ManifestSubmittedToForwarder.Code && !log.SL_IsCancelled).OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();

						if (HVH_IsBookingConfirmed && lastNonCancelledMSFLog == null)
						{
							Logs.AddNew(AutoEvents.ManifestSubmittedToForwarder);
						}
						else if (!HVH_IsBookingConfirmed && lastNonCancelledMSFLog != null)
						{
							lastNonCancelledMSFLog.Cancel();
						}
					}
				}
			}

			base.OnSaving();

			if (HVH_OA_BillToPartyInfo.HasChanges || HVH_OA_DispatchAddressInfo.HasChanges || HVH_OA_OriginDepotInfo.HasChanges)
			{
				InvalidateScreeningStatuses();
			}
		}

		public void InvalidateScreeningStatuses()
		{
			ScreeningStatusUpdater.ProcessInvalidateLocalDataChanges(this, () => { });
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded && !IsInDatabase)
			{
				HVH_ClusterKey = 0;
				HVH_BookingReference = ZString.Empty;
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!IsDeleted)
			{
				var bulkCopyTables = new List<string>
				{
					HVLVOuterPackageSchema.Constants.TableName,
					StmALogSchema.Constants.TableName
				};

				Factory.SetBulkCopyOnTables(bulkCopyTables, batchSize: HVLVDataRegistry.Instance.HVLVDataBulkCopyBatchSize.Value, fireTriggers: true);
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && markedForDisplayOrderCalculation)
			{
				markedForDisplayOrderCalculation = false;
				Reload();
				Factory.ForcePublishForDataRefresh(this);
			}
		}

		public override void Delete()
		{
			Consignments.RemoveAndDeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public HVLVItem[] AllItems
		{
			get
			{
				ZQuery query;
				if (HVH_IsProcessedAtOriginDepot)
				{
					var itemQuery = new ZDBOnlyQuery(typeof(HVLVItem));
					var subquery = new ZDBOnlySubQuery(typeof(HVLVConsignment), HVLVConsignmentSchema.PK);
					subquery.AddToFilter(HVLVConsignmentSchema.HVC_HVH_BookingHeader, PK);
					itemQuery.AddSubQuery(HVLVItemSchema.HVI_HVC_Consignment, subquery, JoinCondition.And);

					query = itemQuery;
				}
				else
				{
					query = new ZQuery(HVLVItemSchema.HVI_ClusterKey, HVH_ClusterKey);
				}

				return Factory.Load<HVLVItem>(query);
			}
		}

		public HVLVItemLine[] AllItemLines
		{
			get
			{
				ZQuery query;
				if (HVH_IsProcessedAtOriginDepot)
				{
					var itemLineQuery = new ZDBOnlyQuery(typeof(HVLVItemLine));
					var consignmentSubquery = new ZDBOnlySubQuery(typeof(HVLVConsignment), HVLVConsignmentSchema.PK);
					consignmentSubquery.AddToFilter(HVLVConsignmentSchema.HVC_HVH_BookingHeader, PK);
					var itemSubquery = new ZDBOnlySubQuery(typeof(HVLVItem), HVLVItemSchema.PK);

					itemSubquery.AddSubQuery(HVLVItemSchema.HVI_HVC_Consignment, consignmentSubquery, JoinCondition.And);
					itemLineQuery.AddSubQuery(HVLVItemLineSchema.HVS_HVI_HVLVItem, itemSubquery, JoinCondition.And);

					query = itemLineQuery;
				}
				else
				{
					query = new ZQuery(HVLVItemLineSchema.HVS_ClusterKey, HVH_ClusterKey);
				}

				return Factory.Load<HVLVItemLine>(query);
			}
		}

		internal void MarkForWeightRecalculation()
		{
			markedForWeightRecalculation = true;
			HVH_GrossWeightInfo.RefreshBinding();
		}
		bool markedForWeightRecalculation = true;

		internal void MarkForVolumeRecalculation()
		{
			markedForVolumeRecalculation = true;
			HVH_GrossVolumeInfo.RefreshBinding();
		}
		bool markedForVolumeRecalculation = true;

		internal void MarkForItemCountRecalculation()
		{
			markedForItemCountRecalculation = true;
			HVH_ItemCountInfo.RefreshBinding();
		}
		bool markedForItemCountRecalculation = true;

		internal void MarkForDisplayOrderCalculation(bool marked = true) => markedForDisplayOrderCalculation = marked;
		bool markedForDisplayOrderCalculation;

		#endregion Load / Save / Delete

		#region Consignments

		[ChildEditable]
		public HVLVBookingHeaderConsignmentCollection Consignments
		{
			get
			{
				if (consignments == null)
				{
					consignments = new HVLVBookingHeaderConsignmentCollection(this);
					RegisterEditableChildObject(consignments);
				}

				if (ShouldLoadConsignments)
				{
					consignments.Load();
					markForReloadConsignments = false;
				}

				return consignments;
			}
		}

		HVLVBookingHeaderConsignmentCollection consignments;

		bool ShouldLoadConsignments
		{
			get
			{
				var result = markForReloadConsignments;
				if (!result)
				{
					if (isDataBinding)
					{
						result = consignments.GetEstimatedLoadCount(new ZQuery()) <= HVLVDataRegistry.Instance.ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm.Value;
					}
					else
					{
						result = !consignments.IsLoaded;
					}
				}

				return result;
			}
		}

		IDisposable IHVLVConsignmentCollectionParent.SetIsDataBinding()
		{
			isDataBinding = true;
			return new DisposableAction(() => isDataBinding = false);
		}

		bool isDataBinding;

		public HVLVConsignmentFilteredCollectionView ConsignmentsFilteredView => consignmentsView ?? (consignmentsView = new HVLVConsignmentFilteredCollectionView(Consignments, this));
		HVLVConsignmentFilteredCollectionView consignmentsView;

		internal void MarkForReloadConsignments() => markForReloadConsignments = true;
		bool markForReloadConsignments;

		internal HashSet<string> ConsignmentIDCache { get; } = new HashSet<string>();

		#endregion Consignments

		public override ZBool HVH_IsActive
		{
			get => base.HVH_IsActive;
			set
			{
				if (base.HVH_IsActive != value)
				{
					base.HVH_IsActive = value;
				}
			}
		}

		public ZString DeactivateErrorMessageActiveConsignment => Res.GetString("5D07E093-C5EB-4A85-8B15-7A7B0E32C647", "You cannot deactivate {0} since it has active consignments. ", HumanReadableName);

		public ZString DeactivateErrorMessageProcessed => Res.GetString("BB7A52F0-D845-4590-8042-C3D2F20DC360", "You cannot deactivate {0} since it is processed at Origin Depot. ", HumanReadableName);

		public override string CanCancel()
		{
			var result = base.CanCancel();
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}

			if (Consignments.OfType<HVLVConsignment>().Any(consignment => consignment.HVC_IsActive))
			{
				result = DeactivateErrorMessageActiveConsignment;
			}

			if (HVH_IsProcessedAtOriginDepot)
			{
				result = DeactivateErrorMessageProcessed;
			}

			return result;
		}

		public void ReactivateAllInactiveConsignments()
		{
			var query = new ZDBOnlyQuery(typeof(HVLVConsignment));
			query.IgnoreActiveFilter = true;
			query.AddToFilter(HVLVConsignmentSchema.HVC_ClusterKey, HVH_ClusterKey);
			query.AddToFilter(HVLVConsignmentSchema.HVC_HVH_BookingHeader, PK);
			query.AddToFilter(HVLVConsignmentSchema.HVC_IsActive, 0);

			var hvlvConsignments = Factory.Load<HVLVConsignment>(query);
			hvlvConsignments.ForEach(c => c.HVC_IsActive = true);
		}

		#region GS1 Prefix

		internal GS1Wrapper GS1Info
		{
			get
			{
				var consignorOrg = BillToParty?.Header;
				var consignorOrgAddress = BillToParty;

				if (!gs1InfoCalculated || (gs1CalculatedConsignorOrg != consignorOrg && gs1CalculatedConsignorOrgAddress != consignorOrgAddress))
				{
					gs1InfoCalculated = true;
					gs1CalculatedConsignorOrg = consignorOrg;
					gs1CalculatedConsignorOrgAddress = consignorOrgAddress;
					gs1Info = GS1Wrapper.GetGS1Info(consignorOrg, consignorOrgAddress, Factory);
				}

				return gs1Info;
			}
		}

		GS1Wrapper gs1Info;
		bool gs1InfoCalculated;
		OrgHeader gs1CalculatedConsignorOrg;
		OrgAddress gs1CalculatedConsignorOrgAddress;

		#endregion

		#region Overrides

		public ZString HumanReadableNameWithoutId => Res.GetString("d3db74d9-6bfd-492d-8318-d0b651908787", "HVLV Booking Header");

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = HumanReadableNameWithoutId;

				if (!HVH_BookingReference.IsEmpty)
				{
					result += " " + HVH_BookingReference;
				}

				return result;
			}
		}

		#endregion

		#region IWorkflowProvider

		public ZString WorkflowType
		{
			get { return WorkflowDescriptors.HVLVBookingHeaderWorkflowDescriptorCode; }
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new HVLVBookingHeaderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}

		HVLVBookingHeaderProcessTaskCollection workflowItems;

		public IColumnValueRanker GetTemplateSelectionCriteria() => GetTemplateSelectionCriteria(this);

		internal static IColumnValueRanker GetTemplateSelectionCriteria(HVLVBookingHeader header)
		{
			Argument.NotNull(header, nameof(header));

			var result = new ColumnValueRanker();

			var dischargePort = header.DispatchAddress?.OA_RL_NKRelatedPortCode ?? ZString.Empty;
			result.Add(ProcessTaskTemplateSchema.P0_GB, GlbBranch.CurrentBranch.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, header.BillToParty?.OA_OH ?? ZGuid.Empty, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, header.HVH_RS_NKBookingServiceLevel, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, dischargePort, dischargePort.Substring(0, 2), ZString.Empty);

			return result;
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this));
				return result.ToArray();
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			base.OnFactorySavingBeforeTransactionCore();
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return HVH_BookingReference; }
		}

		#endregion

		#region IHVLVConsignmentCollectionParent

		IHVLVConsignmentCollection IHVLVConsignmentCollectionParent.Consignments => Consignments;

		public ZBool ConsignmentsNotLoaded
		{
			get
			{
				return !(consignments?.IsLoaded ?? false);
			}
		}

		public ZPropertyInfo ConsignmentsNotLoadedInfo => GetZPropertyInfo(nameof(ConsignmentsNotLoaded));

		#endregion

		#region IEDocsProvider

		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ?? (docManagerInfo = GetNewDocManagerInfo());
		DocManagerInfo docManagerInfo;

		protected virtual HVLVBookingHeaderDocManagerInfo GetNewDocManagerInfo() => new HVLVBookingHeaderDocManagerInfo(this);

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new EDocsProviderSupporter(this);
		}

		#endregion

		#region IDpsEntityProvider

		ZBool IDpsEntityProvider.NeedsScreening => HVH_DeniedPartyScreeningStatus != ScreeningStatusesList.Codes.NotScreened && HVH_DeniedPartyScreeningStatus != ScreeningStatusesList.Codes.Unknown;

		ZBool IDpsEntityProvider.ShouldUpdateRelatedJobs { get; set; }

		ZString IDpsEntityProvider.OriginalScreeningStatus => HVH_DeniedPartyScreeningStatusInfo.OriginalValue.ToString();

		public void InvalidateByLocalDataChanges() { }

		#endregion

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter => new HVLVBookingHeaderDocumentSupporter(this);

		#endregion

		#region IDtbBookingParent Members

		void IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> transportBookings)
		{
			var isPickedUp = false;
			var isDelivered = false;
			var bookings = TransportBookingLoader.GetRelatedTransportBookingEvents(this).Cast<DtbBooking>();

			foreach (var booking in bookings)
			{
				isPickedUp = isPickedUp || booking.Instructions.Any(x => x.KN_Status == TransportStatuses.Codes.PickedUp);
				isDelivered = isDelivered || booking.Instructions.Any(x => x.KN_Status == TransportStatuses.Codes.Delivered);

				if (isDelivered)
				{
					break;
				}
			}

			var allActiveItems = AllItems.Where(x => x.HVI_IsActive);

			if (isDelivered)
			{
				allActiveItems.Where(x => x.HVI_Status != HVLVItemStatus.Codes.ReceivedAtOrigin).ForEach(x => x.HVI_Status = HVLVItemStatus.Codes.ReceivedAtOrigin);
			}
			else if (isPickedUp)
			{
				allActiveItems.Where(x => x.HVI_Status != HVLVItemStatus.Codes.PickUpFromShipper).ForEach(x => x.HVI_Status = HVLVItemStatus.Codes.PickUpFromShipper);
			}
		}

		ZBool IDtbBookingParent.IsSupportsDirectSchedule => false;

		ZString IDtbBookingParent.JobType => WorkflowType;

		DtbBookingDirection[] IDtbBookingParent.GetSupportedDirections() => new DtbBookingDirection[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };

		IJobInvoicingPlugIn IDtbBookingParent.InvoicingJob => null;

		ZQuery IDtbBookingParent.TransportBookingTemplateFilters => new ZQuery();

		ZString IDtbBookingParent.JobTypeDescription => HumanReadableNameWithoutId;

		bool IDtbBookingParent.RequiresMultiContainerBooking => false;

		bool IDtbBookingParent.CanCreateTransportBooking => true;

		ZGuid IDtbBookingParent.BookingParentPK => PK;

		string IDtbBookingParent.BookingParentTablePrefix => TablePrefix;

		(bool isShouldShow, string caption, string message, string confirmation) IDtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.HVLVBookingHeader;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobNumber => HVH_BookingReference;

		ZString IRelatedJob.JobDescription => HumanReadableNameWithoutId;

		ZString IRelatedJob.JobStatus => BookingStatus;

		#endregion

		#region IHVLVBookingHeader

		IOrgAddress IHVLVBookingHeader.BillToParty => BillToParty;

		IOrgContact IHVLVBookingHeader.BillToPartyContact => BillToPartyContact;

		IOrgContact IHVLVBookingHeader.BookedBy => BookedBy;

		IRefServiceLevel IHVLVBookingHeader.BookingServiceLevel => BookingServiceLevel;

		IOrgAddress IHVLVBookingHeader.DispatchAddress => DispatchAddress;

		IOrgHeader IHVLVBookingHeader.FreightAgent => FreightAgent;

		IOrgAddress IHVLVBookingHeader.OriginDepot => OriginDepot;

		IProcessTaskCollection IHVLVBookingHeader.WorkflowItems => WorkflowItems;

		IHVLVConsignmentCollection IHVLVBookingHeader.Consignments => Consignments;

		#endregion

		#region UniqueIndexFailureHandlers

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (uniqueIndexFailureHandlers == null)
				{
					uniqueIndexFailureHandlers = new List<IUniqueIndexFailureHandler>();
					uniqueIndexFailureHandlers.Add(new HVLVBookingHeaderJobNumberUniqueIndexFailureHandler(this));
					uniqueIndexFailureHandlers.Add(new HVLVBookingHeaderClusterKeyUniqueIndexFailureHandler(this));
				}

				return uniqueIndexFailureHandlers;
			}
		}

		List<IUniqueIndexFailureHandler> uniqueIndexFailureHandlers;

		class HVLVBookingHeaderJobNumberUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public HVLVBookingHeaderJobNumberUniqueIndexFailureHandler(HVLVBookingHeader header)
				: base(HVLVBookingHeaderSchema.Constants.Indexes.NR_UX__HVH_BookingReference, header)
			{
				header.HVH_BookingReference = ZString.Empty;
			}

			protected override INumberFountainProxy NumberFountainToFix => Env.NumberFountains.HVLVBookingHeaderJobNumber;
		}

		class HVLVBookingHeaderClusterKeyUniqueIndexFailureHandler : HVLVClusterKeyNumberFountainUniqueIndexFailureHandler
		{
			public HVLVBookingHeaderClusterKeyUniqueIndexFailureHandler(HVLVBookingHeader header)
				: base(HVLVBookingHeaderSchema.Constants.Indexes.NR_UC__HVH_ClusterKey, header)
			{
				header.HVH_ClusterKey = ZInt.Zero;
			}
		}

		#endregion

		#region DocsAndCartage

		bool IShipmentWithDocsAndCartage.ShouldValidateDeliveryAndPickupCartageCoBeingSame() => false;

		bool IShipmentWithDocsAndCartage.ShouldValidateDeliveryCoPK() => true;

		bool IShipmentWithDocsAndCartage.RequiresOrderNumbersOnDocs() => false;

		bool IShipmentWithDocsAndCartage.RequiresOrderTrackLink() => false;

		IContainer[] IHaveInternalCartage.GetContainers() => null;

		IPackLineInfo[] IHaveInternalCartage.GetPackLines() => null;

		bool IHaveInternalCartage.TypeSpecificPreCreationCheck() => false;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => null;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => null;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => null;

		public JobDocsAndCartage DocsAndCartage
		{
			get
			{
				if (fDocsAndCartage == null)
				{
					fDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(this);
					RegisterListChangedCalledRefreshBinding(fDocsAndCartage);
				}
				else if (fDocsAndCartage.IsDeleted)
				{
					UnRegisterListChangedCalledRefreshBinding(fDocsAndCartage);
					fDocsAndCartage = null;
				}

				RegisterEditableChildObject(fDocsAndCartage);
				return fDocsAndCartage;
			}
		}

		JobDocsAndCartage fDocsAndCartage;

		event EventHandler IShipmentWithDocsAndCartage.TransportModeChanged
		{
			add { }
			remove { }
		}

		JobDocAddress IShipmentWithDocsAndCartage.ConsignorDocumentaryAddress => null;

		JobDocAddress IShipmentWithDocsAndCartage.ConsigneeDocumentaryAddress => null;

		JobDocsAndCartageValidation IShipmentWithDocsAndCartage.PiggyBackedValidation => null;

		string IShipmentWithDocsAndCartage.TableName => string.Empty;

		string IShipmentWithDocsAndCartage.UniqueConsignRef => string.Empty;

		string IShipmentWithDocsAndCartage.MasterBillNumber => string.Empty;

		string IShipmentWithDocsAndCartage.HouseBillNumber => string.Empty;

		OrgHeader IShipmentWithDocsAndCartage.ExportBroker => null;

		ZBool IHaveInternalCartage.DeliveryAndPickupCartageApplicable => false;

		ZBool IHaveInternalCartage.InternalCartageEnabled => false;

		ZBool IHaveInternalCartage.PackedAtDepot => false;

		bool IHaveInternalCartage.IsForPickupCartage => false;

		OrgHeader IHaveInternalCartage.PickupCartageOrg => null;

		OrgHeader IHaveInternalCartage.DeliveryCartageOrg => null;

		ZString IHaveInternalCartage.ContainerMode => string.Empty;

		ZString IHaveInternalCartage.TransportMode => string.Empty;

		ZString IHaveInternalCartage.ServiceLevel => string.Empty;

		ZString IHaveInternalCartage.CartageTypeOverride => string.Empty;

		ZString IHaveInternalCartage.OwnerRef => string.Empty;

		ZGuid IHaveInternalCartage.CartagePickupDepotAddress => ZGuid.Empty;

		ZGuid IHaveInternalCartage.CartageDeliveryDepotAddress => ZGuid.Empty;

		ZGuid IHaveInternalCartage.CartagePickupCTOAddress => ZGuid.Empty;

		ZGuid IHaveInternalCartage.CartageDeliveryCTOAddress => ZGuid.Empty;

		ZGuid IHaveInternalCartage.CartagePickupContainerYardAddress => ZGuid.Empty;

		ZGuid IHaveInternalCartage.CartageDeliveryContainerYardAddress => ZGuid.Empty;

		ZGuid IHaveInternalCartage.BranchPK => ZGuid.Empty;

		JobDocAddress IHaveInternalCartage.CartageExporterDocAddress => null;

		JobDocAddress IHaveInternalCartage.CartageImporterDocAddress => null;

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryDepotAddressInfo => null;

		ZPropertyInfo IHaveInternalCartage.CartagePickupCTOAddressInfo => null;

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryCTOAddressInfo => null;

		ZPropertyInfo IHaveInternalCartage.CartagePickupContainerYardAddressInfo => null;

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryContainerYardAddressInfo => null;

		ZPropertyInfo IHaveInternalCartage.CartagePickupDepotAddressInfo => null;

		ZPropertyInfo IHaveInternalCartage.DeliveryCartageAdvisedInfo => null;

		ZPropertyInfo IHaveInternalCartage.PickupCartageAdvisedInfo => null;

		JobDocAddressDependentCollection IHaveInternalCartage.DocAddresses => null;

		bool IHaveInternalCartage.IsAllowedToUpdateAdviseDates => false;

		string ICartageExportSupport.JobNumber => HVH_BookingReference;

		public MasterFiles.Business.Directions JobDirection => MasterFiles.Business.Directions.Unknown;

		CommonShipment IShipmentProvider.Shipment => null;

		JobDocAddressDependentCollection IDocAddresses.DocAddresses => null;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => null;

		IHaveRequiredDocuments IDocsAndCartageParent.RequiredDocumentsProvider => DocsAndCartage;

		Type IDocsAndCartageParent.DocsAndCartageParentType => GetType();

		Type IDocsAndCartageParent.DocsAndCartageType => typeof(JobDocsAndCartage);

		#endregion

		#region IScreeningPartyProvider

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get => base.HVH_DeniedPartyScreeningStatus;
			set
			{
				base.HVH_DeniedPartyScreeningStatus = value;
				if (ScreeningStatusesList.Codes.Matched == value)
				{
					Consignments.ForEach(x => ((HVLVConsignment)x).HVC_DeniedPartyScreeningStatus = value);
				}
			}
		}

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties => GetHVLVBookingHeaderScreeningEntities().Union(ConsignmentScreeningParties).ToArray();

		IEnumerable<ScreeningParty> GetHVLVBookingHeaderScreeningEntities()
		{
			if (!HVH_OA_BillToParty.IsEmpty)
			{
				yield return new ScreeningParty(this, Res.GetString("5c88e3db-33c0-4a9c-9985-d7a10e7e2724", "Bill to Party"), BillToParty.Header);
			}

			if (!HVH_OA_DispatchAddress.IsEmpty)
			{
				yield return new ScreeningParty(this, Res.GetString("b65ec2dc-9952-4704-b29c-692095b96a4e", "Dispatch Address"), DispatchAddress.Header);
			}

			if (!HVH_OA_OriginDepot.IsEmpty)
			{
				yield return new ScreeningParty(this, Res.GetString("214e89c9-cd6a-474b-885d-64f54ca6767f", "Origin Depot"), OriginDepot.Header);
			}
		}

		IEnumerable<ScreeningParty> ConsignmentScreeningParties => Consignments.OfType<IScreeningPartyProvider>().SelectMany(c => c.ScreeningParties);

		ZString IScreeningPartyProvider.GetWorstScreeningStatus()
		{
			var statuses = new List<ZString>();
			GetScreeningStatus(statuses);
			return ScreeningStatusUpdater.GetWorstScreeningStatus(statuses);
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared()
		{
			return HVH_DeniedPartyScreeningStatus == ScreeningStatusesList.Codes.JobCleared
				? ScreeningStatusesList.Codes.JobCleared
				: (this as IScreeningPartyProvider).GetWorstScreeningStatus();
		}

		void GetScreeningStatus(List<ZString> statuses)
		{
			AddScreeningStatusToList(statuses, BillToParty?.Header);
			AddScreeningStatusToList(statuses, DispatchAddress?.Header);
			AddScreeningStatusToList(statuses, OriginDepot?.Header);
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

		#endregion

		#region IAuditParent

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(HVLVConsignmentSchema.HVC_HVH_BookingHeader, HVLVConsignmentSchema.HVC_ConsignmentId);
			}
		}

		#endregion

		#region IHVLVPrescreeningDataProvider

		string IHVLVPrescreeningDataProvider.ETailerOrgCode => BillToParty?.Header?.OH_Code ?? ZString.Empty;

		BusinessObject IHVLVPrescreeningDataProvider.Entity => this;

		IEnumerable<IHVLVConsignment> IHVLVPrescreeningDataProvider.Consignments => Consignments.Cast<IHVLVConsignment>();

		string IHVLVPrescreeningDataProvider.TableCode => HVLVBookingHeaderSchema.Constants.Prefix;

		#endregion
	}
}
