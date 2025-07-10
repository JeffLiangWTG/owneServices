using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentHeader :
		AutoHVLVConsignmentHeader,
		IHVLVConsignmentHeader,
		IHVLVConsignmentCollectionParent,
		IScreeningPartyProvider,
		IHVLVPrescreeningDataProvider
	{
		public HVLVConsignmentHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static HVLVConsignmentHeader GetOrCreate(ForwardingShipment shipment)
		{
			var result = Get(shipment);
			if (shipment != null && result == null)
			{
				result = shipment.Factory.New<HVLVConsignmentHeader>();
				result.HCH_JS_Shipment = shipment.PK;
				shipment.RegisterEditableChildObject(result);
			}

			return result;
		}

		public static HVLVConsignmentHeader Get(ForwardingShipment shipment)
		{
			HVLVConsignmentHeader result = default;
			if (shipment != null)
			{
				var factory = shipment.Factory;
				result = factory.LoadTop1<HVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK));
			}

			return result;
		}

		public ForwardingShipment Shipment => Factory.Load<ForwardingShipment>(HCH_JS_Shipment);

		void PopulateClusterKeyAndJobNumberIfNeeded()
		{
			if (HCH_ClusterKey.IsEmpty)
			{
				HCH_ClusterKey = (int)Env.NumberFountains.HVLVConsignmentClusterKey.GetNext(Factory);
			}

			if (HCH_JobNumber.IsEmpty)
			{
				HCH_JobNumber = Env.NumberFountains.HVLVConsignmentHeaderJobNumber.GetNextFormatted(Factory);
			}
		}

		public override ZInt HCH_ClusterKey
		{
			get => base.HCH_ClusterKey;
			set
			{
				if (!value.IsEmpty)
				{
					foreach (var consignment in Consignments.OfType<HVLVConsignment>().Where(x => x.HVC_HCH_Header == PK))
					{
						consignment.HVC_ClusterKey = value;
					}

					base.HCH_ClusterKey = value;
				}
			}
		}

		internal void ClearClusterKeyAndJobNumberForUniqueIndexFailureHandlingOnly()
		{
			base.HCH_ClusterKey = ZInt.Zero;
			HCH_JobNumber = ZString.Empty;
		}

		public HVLVConsignmentHeaderGenPivotCollection GenPivotCollection
		{
			get
			{
				if (genPivotCollection == null)
				{
					genPivotCollection = new HVLVConsignmentHeaderGenPivotCollection(this);
				}

				return genPivotCollection;
			}
		}

		HVLVConsignmentHeaderGenPivotCollection genPivotCollection;

		public bool HasHVLVDataCreated => this.IsInDatabase || HasChildConsignments;

		#region Properties

		[RelatedBusinessObject(nameof(Shipment))]
		public override ZGuid HCH_JS_Shipment
		{
			get => base.HCH_JS_Shipment;
			set => base.HCH_JS_Shipment = value;
		}

		public ZDecimal TotalWeight
		{
			get
			{
				var totalWeight = 0m;

				if (consignments != null && consignments.IsLoaded)
				{
					foreach (var item in ActiveItems)
					{
						totalWeight += Weight.ConvertSafe(item.EffectiveWeight, item.Consignment.HVC_WeightUQ, Shipment.JS_UnitOfWeight);
					}
				}
				else if (IsInDatabase)
				{
					totalWeight = GetTotalWeightFromDB();
				}

				return totalWeight;
			}
		}

		decimal GetTotalWeightFromDB()
		{
			var sql = string.Format("SELECT TotalWeight FROM dbo.GetHVLVConsignmentHeaderTotalWeight('{0}')", HCH_ClusterKey);
			return Db.Connection.ExecuteScalar<decimal>(sql);
		}

		public ZDecimal TotalVolume
		{
			get
			{
				var totalVolume = 0m;
				if (consignments != null && consignments.IsLoaded)
				{
					foreach (var item in ActiveItems)
					{
						totalVolume += Volume.ConvertSafe(item.EffectiveVolume, item.Consignment.HVC_VolumeUQ, Shipment.JS_UnitOfVolume);
					}
				}
				else if (IsInDatabase)
				{
					totalVolume = GetTotalVolumeFromDB();
				}

				return totalVolume;
			}
		}

		decimal GetTotalVolumeFromDB()
		{
			var sql = string.Format("SELECT TotalVolume FROM dbo.GetHVLVConsignmentHeaderTotalVolume('{0}')", HCH_ClusterKey);
			return Db.Connection.ExecuteScalar<decimal>(sql);
		}

		IEnumerable<HVLVItem> ActiveItems
		{
			get
			{
				var query = new ZQuery(HVLVItemSchema.HVI_IsActive, true);
				query.AddToFilter(new ZQuery(HVLVItemSchema.HVI_ClusterKey, HCH_ClusterKey));
				return Factory.Load<HVLVItem>(query);
			}
		}

		#endregion

		#region HVLV Consignments

		[ChildEditable(true)]
		public HVLVShipmentConsignmentCollection Consignments
		{
			get
			{
				if (consignments == null)
				{
					consignments = new HVLVShipmentConsignmentCollection(Shipment);
					RegisterEditableChildObject(consignments);

					foreach (HVLVConsignment consignment in consignments)
					{
						consignment.ManagingShipment = Shipment;
					}
				}

				if (ShouldLoadConsignments)
				{
					consignments.Load();

					foreach (HVLVConsignment consignment in consignments)
					{
						consignment.ManagingShipment = Shipment;
					}

					markForReloadConsignments = false;
				}

				return consignments;
			}
		}

		HVLVShipmentConsignmentCollection consignments;

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

		public HVLVConsignmentFilteredCollectionView ConsignmentsFilteredView
		{
			get { return consignmentsView ?? (consignmentsView = new HVLVConsignmentFilteredCollectionView(Consignments, this)); }
		}

		HVLVConsignmentFilteredCollectionView consignmentsView;

		public bool HasActiveConsignments
		{
			get { return Consignments.OfType<HVLVConsignment>().Any(consignment => consignment.HVC_IsActive); }
		}

		IEnumerable<IHVLVConsignment> IHVLVConsignmentHeader.Consignments
		{
			get
			{
				if (activeConsignmentsView == null)
				{
					activeConsignmentsView = new HVLVConsignmentFilteredCollectionView(Consignments);
					activeConsignmentsView.Filter = new ZQuery(HVLVConsignmentSchema.HVC_IsActive, true);
					activeConsignmentsView.Rebuild();
				}

				return activeConsignmentsView.ToList().OfType<IHVLVConsignment>();
			}
		}

		HVLVConsignmentFilteredCollectionView activeConsignmentsView;

		internal void MarkForReloadConsignments() => markForReloadConsignments = true;
		bool markForReloadConsignments;

		void IHVLVConsignmentHeader.OnConsolChanged()
		{
			var itemsToUpdate = Shipment.HVLVItems.Where(item => !item.HVI_HVO_OuterPackage.IsEmpty).Distinct();
			var consolsIsNullOrEmpty = Shipment.Consols.IsNullOrEmpty();
			foreach (IHVLVItemForDocument item in itemsToUpdate)
			{
				var outerPackage = item.OuterPackage;
				if (consolsIsNullOrEmpty)
				{
					outerPackage.HVO_JK_LoadedOnConsol = ZGuid.Empty;
				}
				else if (Shipment.ArrivalConsol != null && outerPackage.HVO_JK_LoadedOnConsol.IsEmpty)
				{
					outerPackage.HVO_JK_LoadedOnConsol = Shipment.ArrivalConsol.PK;
				}
			}
		}

		#endregion

		#region HVLV Consignments Convert To Stand Alone Declaration

		public HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection ConsignmentsConvertToStandAloneDeclaration
		{
			get { return new HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection(Consignments); }
		}

		public HVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionView ConsignmentsConvertToStandAloneDeclarationView
		{
			get { return new HVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionView(ConsignmentsConvertToStandAloneDeclaration); }
		}

		#endregion

		#region Overrides

		public override void OnSaving()
		{
			if (!IsSupportedShipmentType(Shipment) && !IsInDatabase && !HasChildConsignments)
			{
				Delete();
			}
			else
			{
				PopulateClusterKeyAndJobNumberIfNeeded();
			}
#if DEBUG
			if (Globals.IsTest && Factory.GetAddedBusinessObjects<HVLVBookingHeader>().Any())
			{
				throw new ValidTestDataGenerationException(
					@"Creating HVLVBookingHeader and HVLVConsignmentHeader for the same HVLVConsignment would cause unexpected test failure as the saving order of the two tables are not guaranteed.
						HVLVBookingHeader (if any) should be saved prior to saving a HVLVConsignmentHeader");
			}
#endif
			base.OnSaving();
		}

		bool IsSupportedShipmentType(ForwardingShipment shipment) => shipment?.IsHighVolumeLowValue ?? false;

		bool HasChildConsignments => Factory.Exists(typeof(HVLVConsignment), new ZQuery(HVLVConsignmentSchema.HVC_HCH_Header, PK));

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
			if (saveSucceeded && ShipmentItemsCountView != null)
			{
				ShipmentItemsCountView.Reload();
			}
		}

		#endregion

		#region GS1 Prefix

		internal GS1Wrapper GS1Info
		{
			get
			{
				var consignorOrg = Shipment.Consignor;
				var consignorOrgAddress = Shipment.ConsignorDocumentaryAddress.HasRealAddress ? Shipment.ConsignorDocumentaryAddress.Address : null;

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

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (fUniqueIndexFailureHandler == null)
				{
					fUniqueIndexFailureHandler = new HVLVConsignmentHeaderUniqueIndexFailureHandler(this);
				}

				yield return fUniqueIndexFailureHandler;
			}
		}

		IUniqueIndexFailureHandler fUniqueIndexFailureHandler;

		class HVLVConsignmentHeaderUniqueIndexFailureHandler : HVLVClusterKeyNumberFountainUniqueIndexFailureHandler
		{
			public HVLVConsignmentHeaderUniqueIndexFailureHandler(HVLVConsignmentHeader header)
				: base(HVLVConsignmentHeaderSchema.Constants.Indexes.NR_UC__HCH_ClusterKey, header)
			{
				header.ClearClusterKeyAndJobNumberForUniqueIndexFailureHandlingOnly();
			}
		}

		#endregion

		#region IHVLVConsignmentCollectionParent Members

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

		#region Menu Item Properties

		public IEnumerable<HVLVConsignment> ConsignmentsForBinding => Consignments.OfType<HVLVConsignment>();

		public bool HasAnyConsignmentFailedPreScreening => ConsignmentsForBinding.Any(x => x.HasFailedPreScreeningStatus);

		#endregion

		#region Related Customs Job

		public List<string> CustomsJobCanNotCancelReason
		{
			get
			{
				if (CancellableCustomsJobs.Any(job => !job.IsCancelled && !string.IsNullOrEmpty(job.CanCancel())))
				{
					return CancellableCustomsJobs.Where(job => !job.IsCancelled && job.CanCancel() != null).Select(job => job.CanCancel()).ToList();
				}

				return new List<string>();
			}
		}

		public List<ICancellable> CancellableCustomsJobs => GetCancellableCustomsJobs();

		List<ICancellable> GetCancellableCustomsJobs()
		{
			var result = new List<ICancellable>();

			foreach (var jobType in CancelableCustomsJobTypeArray)
			{
				if (GenPivotCollection.CustomsJobs.Any(i => jobType.IsAssignableFrom(i.GetType())))
				{
					result.AddRange(GenPivotCollection.CustomsJobs.Where(i => jobType.IsAssignableFrom(i.GetType())).Cast<ICancellable>());
				}
			}

			return result;
		}

		readonly Type[] CancelableCustomsJobTypeArray = new[]
		{
			typeof(IBaseCusSCAOceanBill),
			typeof(ICusMAWB)
		};

		#endregion

		#region Show Import/Export Properties

		public ZBool ShowExport => Shipment.IsExport();

		public ZBool ShowImport => !ShowExport;

		#endregion

		#region Count Properties

		public ZInt ItemCount => ShipmentItemsCountView?.HSC_ItemCount ?? 0;

		public ZInt ImportClearedCount => ShipmentItemsCountView?.HSC_ImportClearedCount ?? 0;

		public ZInt ImportHeldCount => ShipmentItemsCountView?.HSC_ImportHeldCount ?? 0;

		public ZInt ImportNoneReportedCount => ShipmentItemsCountView?.HSC_ImportNoneReportedCount ?? 0;

		public ZInt ExportClearedCount => ShipmentItemsCountView?.HSC_ExportClearedCount ?? 0;

		public ZInt ExportHeldCount => ShipmentItemsCountView?.HSC_ExportHeldCount ?? 0;

		public ZInt ExportNoneReportedCount => ShipmentItemsCountView?.HSC_ExportNoneReportedCount ?? 0;

		public ZInt SurplusCount => ShipmentItemsCountView?.HSC_SurplusCount ?? 0;

		public ZInt ShortCount => ShipmentItemsCountView?.HSC_ShortCount ?? 0;

		public ZInt DeliveredCount => ShipmentItemsCountView?.HSC_DeliveredCount ?? 0;

		public ZInt ScannedCount => ShipmentItemsCountView?.HSC_ScannedCount ?? 0;

		public ZInt ScannedClearedCount => ShipmentItemsCountView?.HSC_ScannedClearedCount ?? 0;

		public ZInt ScannedHeldCount => ShipmentItemsCountView?.HSC_ScannedHeldCount ?? 0;

		public ZInt ScannedNoneReportedCount => ShipmentItemsCountView?.HSC_ScannedNoneReportedCount ?? 0;

		int IHVLVConsignmentHeader.ConsignmentCountWithoutLoading
		{
			get
			{
				var query = new ZQuery(HVLVConsignmentSchema.HVC_HCH_Header, PK);
				var dbCount = Factory.GetDatabaseCount(typeof(HVLVConsignment), query);

				var changeSet = Factory.GetChanges();
				var newBizos = changeSet.GetAddedObjects().Where(x => x is IHVLVConsignment && !x.IsInDatabase).Cast<IHVLVConsignment>();
				var newBizosCount = newBizos.Count(x => x.HVC_JS_ManifestedOnShipment == Shipment?.PK);

				var activeStateChanges = 0;
				var bizoChangeSets = changeSet.GetChangedObjects().Where(x => x.IsExistsInDatabase);

				foreach (var change in bizoChangeSets)
				{
					if (change.SessionInstance is IHVLVConsignment consignmentInSession
						&& change.DatabaseInstance is IHVLVConsignment consignmentInDatabase
						&& consignmentInSession.HVC_JS_ManifestedOnShipment == Shipment?.PK
						&& consignmentInDatabase.HVC_JS_ManifestedOnShipment == Shipment?.PK
						&& consignmentInSession.HVC_IsActive != consignmentInDatabase.HVC_IsActive)
					{
						activeStateChanges += consignmentInSession.HVC_IsActive ? 1 : -1;
					}
				}

				return dbCount + newBizosCount + activeStateChanges;
			}
		}

		public HVLVShipmentItemsCountView ShipmentItemsCountView => GetHVLVShipmentItemsCountView();

		internal HashSet<string> ConsignmentIDCache { get; } = new HashSet<string>();

		HVLVShipmentItemsCountView shipmentItemsCountView;

		HVLVShipmentItemsCountView GetHVLVShipmentItemsCountView()
		{
			if (IsInDatabase)
			{
				if (shipmentItemsCountView == null)
				{
					var query = new ZDBOnlyQuery(typeof(HVLVShipmentItemsCountView));
					query.AddToFilter(HVLVShipmentItemsCountViewSchema.PK, HCH_JS_Shipment);
					query.IgnoreDbQueryCache = true;

					shipmentItemsCountView = Factory.LoadTop1<HVLVShipmentItemsCountView>(query);
				}

				return shipmentItemsCountView;
			}

			return null;
		}

		#endregion

		#region IScreeningPartyProvider

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get => ScreeningProxyShipment.ScreeningStatus;
			set { }
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatus()
		{
			return HCH_DeniedPartyScreeningStatus;
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared()
		{
			return ScreeningProxyShipment.GetWorstScreeningStatusUnlessManuallyCleared();
		}

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties => Consignments.OfType<IScreeningPartyProvider>().SelectMany(c => c.ScreeningParties).ToArray();

		IScreeningPartyProvider ScreeningProxyShipment => Shipment;

		internal void UpdateDeniedPartyScreeningStatus()
		{
			HCH_DeniedPartyScreeningStatus = ScreeningStatusUpdater.GetWorstScreeningStatus(Consignments.Select(c => c.HVC_DeniedPartyScreeningStatus));
		}

		#endregion

		#region IHVLVPrescreeningDataProvider

		string IHVLVPrescreeningDataProvider.ETailerOrgCode => Shipment.ConsignorDocumentaryAddress?.Organisation?.OH_Code ?? ZString.Empty;

		string IHVLVPrescreeningDataProvider.TableCode => JobShipmentSchema.Constants.Prefix;

		BusinessObject IHVLVPrescreeningDataProvider.Entity => Shipment;

		IEnumerable<IHVLVConsignment> IHVLVPrescreeningDataProvider.Consignments => Consignments.Cast<IHVLVConsignment>();

		Logs IHVLVPrescreeningDataProvider.Logs => Shipment.Logs;

		#endregion
	}
}
