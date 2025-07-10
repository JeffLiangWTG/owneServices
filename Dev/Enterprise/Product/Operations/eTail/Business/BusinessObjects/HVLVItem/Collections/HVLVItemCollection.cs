using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVItemCollection : BusinessObjectCollectionWithAutoFetchHints<HVLVItem, HVLVConsignment>, IImportCollectionElementMatchingSupporter, IHVLVItemCollectionForDocument
	{
		public HVLVItemCollection(HVLVConsignment master)
			: base(master)
		{
		}

		IHVLVItem IHVLVItemCollection.this[int i] => (IHVLVItem)Elements[i];

		IHVLVItemForDocument IHVLVItemCollectionForDocument.this[int i] => (IHVLVItemForDocument)Elements[i];

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return HVLVItemSchema.HVI_HVC_Consignment; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			if (!Master.IsDeleted && !Master.HVC_ClusterKey.IsEmpty)
			{
				result.AddToFilter(new ZQuery(HVLVItemSchema.HVI_ClusterKey, Master.HVC_ClusterKey));
			}

			return result;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			query.IgnoreActiveFilter = true;
			return query;
		}

		protected override bool AllowNewCore => base.AllowNewCore && !IsConsignmentManagedByOtherShipment;

		protected override bool AllowRemoveCore => base.AllowRemoveCore && !IsConsignmentManagedByOtherShipment;

		bool IsConsignmentManagedByOtherShipment => Master.HasManagingShipment && Master.ManagingShipment.PK != Master.HVC_JS_ManifestedOnShipment;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var item = (HVLVItem)bizOAdded;

			if (!IsLoading && item.HVI_JS_LoadedOnShipment.IsEmpty && Master.HasManagingShipment)
			{
				item.HVI_JS_LoadedOnShipment = Master.ManagingShipment.PK;
			}

			if (!IsLoading)
			{
				Master.CalculateItemTotals();
				RefreshBookingHeaderTotalsIfNeeded();
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);

			Master.CalculateItemTotals();
			RefreshBookingHeaderTotalsIfNeeded();
		}

		void RefreshBookingHeaderTotalsIfNeeded()
		{
			var bookingHeader = Master?.BookingHeader;
			if (bookingHeader != null)
			{
				bookingHeader.MarkForItemCountRecalculation();
				bookingHeader.MarkForVolumeRecalculation();
				bookingHeader.MarkForWeightRecalculation();
			}
		}

		#region IImportCollectionElementMatchingSupporter

		string IImportCollectionElementMatchingSupporter.MatchingColumnName => HVLVItemSchema.HVI_CurrentBarcode.Name;

		bool IImportCollectionElementMatchingSupporter.IsGenericColumnMatchingAllowed => false;

		bool IImportCollectionElementMatchingSupporter.FindGenericColumnMatches => false;

		BusinessObject IImportCollectionElementMatchingSupporter.GetMatchingBizObject(string matchingKey)
		{
			var match = default(BusinessObject);
			if (!string.IsNullOrEmpty(matchingKey))
			{
				match = Find(i => i.HVI_CurrentBarcode == matchingKey).FirstOrDefault();
			}

			return match;
		}

		void IImportCollectionElementMatchingSupporter.PrepareForReuse(BusinessObject matchedBizO)
		{
			var item = (HVLVItem)matchedBizO;

			if (item.HasItemLines)
			{
				item.Lines.RemoveAndDeleteAll();
			}
		}

		#endregion

#if DEBUG
		public static int GetCollectionCount(BusinessObjectFactory factory, ZInt clusterKey)
		{
			var service = factory.ServiceContainer.AddService(new HVLVFetchHintAllocatorService<HVLVItemCollection>(factory, HVLVItemSchema.HVI_ClusterKey));
			return service.GetCollectionCountForTesting(clusterKey);
		}

		public static void SetCollectionCountForTest(BusinessObjectFactory factory, ZInt clusterKey, int collectionCount)
		{
			var service = factory.ServiceContainer.AddService(new HVLVFetchHintAllocatorService<HVLVItemCollection>(factory, HVLVItemSchema.HVI_ClusterKey));
			service.SetCounterForTesting(clusterKey, collectionCount);
		}
#endif
	}
}
