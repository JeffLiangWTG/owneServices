using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentFilteredCollectionView : BusinessObjectCollectionView<HVLVConsignment>, IImportCollectionElementMatchingSupporter
	{
		public HVLVConsignmentFilteredCollectionView(BusinessObjectCollection collectionToFilter)
			: this(collectionToFilter, null)
		{
		}

		public HVLVConsignmentFilteredCollectionView(BusinessObjectCollection collectionToFilter, IHVLVConsignmentCollectionParent parent)
			: base(collectionToFilter)
		{
			collectionToFilter.CountChanged += (_, __) =>
			{
				parent?.ConsignmentsNotLoadedInfo.RefreshBinding();
			};
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var result = false;
			if (element is HVLVConsignment consignment)
			{
				result = consignment.MatchesFilter(Filter);

				if (result)
				{
					result = InMemoryFilter?.Invoke(consignment) ?? true;
				}
			}

			return result;
		}

		public ZQuery Filter { get; set; }

		public HVLVConsignmentInMemoryFilter InMemoryFilter { get; set; }

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var bookingHeader = (elementToDelete as HVLVConsignment)?.BookingHeader;
			base.RemoveAndDelete(elementToDelete);

			if (bookingHeader != null)
			{
				bookingHeader.MarkForWeightRecalculation();
				bookingHeader.MarkForVolumeRecalculation();
				bookingHeader.MarkForItemCountRecalculation();
			}
		}

		#region IImportCollectionElementMatchingSupporter

		string IImportCollectionElementMatchingSupporter.MatchingColumnName => HVLVConsignmentSchema.HVC_WaybillNumber.Name;

		BusinessObject IImportCollectionElementMatchingSupporter.GetMatchingBizObject(string matchingKey)
		{
			var result = collectionToFilter.Cast<HVLVConsignment>().FirstOrDefault(c =>
			c.IsInDatabase
			&& c.HVC_WaybillNumber == matchingKey);

			return result;
		}

		void IImportCollectionElementMatchingSupporter.PrepareForReuse(BusinessObject matchedBizO)
		{
		}

		bool IImportCollectionElementMatchingSupporter.IsGenericColumnMatchingAllowed => false;

		bool IImportCollectionElementMatchingSupporter.FindGenericColumnMatches => false;

		#endregion

		public override IDisposable SuspendAdditionallyForImport()
		{
			var result = base.SuspendAdditionallyForImport();
			if (collectionToFilter is IDependentBusinessObjectCollection collection
				&& collection.Master is ISingleElementListInternal singleElementList)
			{
				result = new DisposableList(new[] { result, singleElementList.SuspendListChanged() });
			}

			return result;
		}
	}

	public delegate bool HVLVConsignmentInMemoryFilter(HVLVConsignment consignment);
}
