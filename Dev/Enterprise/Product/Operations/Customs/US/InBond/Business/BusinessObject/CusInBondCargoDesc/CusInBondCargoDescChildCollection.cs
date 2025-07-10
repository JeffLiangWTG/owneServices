using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondCargoDescChildCollection : DependentBusinessObjectCollection<CusInBondCargoDesc, CusInBondCargoDesc>
	{
		public CusInBondCargoDescChildCollection(CusInBondCargoDesc parentCommodity)
			: base(parentCommodity, parentCommodity.Factory)
		{
		}

		public bool HasACommodityWithPartNumber
		{
			get
			{
				if (hasACommodityWithPartNumberCached == null)
				{
					hasACommodityWithPartNumberCached = new CachedProperty<bool>(Factory, () => this.OfType<CusInBondCargoDesc>().Any(x => !x.BY_PartNumber.IsEmpty));
				}
				return hasACommodityWithPartNumberCached.Value;
			}
		}
		CachedProperty<bool> hasACommodityWithPartNumberCached;

		public bool HasACommodityWithDifferentSupplier(ZGuid supplierPKToCheck)
		{
			var result = false;
			if (!supplierPKToCheck.IsEmpty && UniqueChildCommoditiesSupplierPKs.Count > 0)
			{
				result = UniqueChildCommoditiesSupplierPKs.Count > 1 || UniqueChildCommoditiesSupplierPKs[0] != supplierPKToCheck;
			}
			return result;
		}

		List<ZGuid> UniqueChildCommoditiesSupplierPKs
		{
			get
			{
				if (uniqueChildCommoditiesSupplierPKsCached == null)
				{
					uniqueChildCommoditiesSupplierPKsCached = new CachedProperty<List<ZGuid>>(Factory, () => new List<ZGuid>(this.OfType<CusInBondCargoDesc>().Select(x => x.BY_OH_Supplier).Where(x => !x.IsEmpty).Distinct()));
				}
				return uniqueChildCommoditiesSupplierPKsCached.Value;
			}
		}
		CachedProperty<List<ZGuid>> uniqueChildCommoditiesSupplierPKsCached;

		protected override bool AllowNewCore
		{
			get
			{
				return Master.IsTopLevelCommodity || !Master.BY_PartNumber.IsEmpty;
			}
		}

		protected override string FkColumnName
		{
			get { return CusInBondCargoDescSchema.Constants.BY_ParentID; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (Count == 1)
			{
				ClearParentCommodityDataThatAreAvailableOnChild();
			}
			ClearParentCommodityPartDataWhenChildHasPartData();
		}

		internal void ClearParentCommodityPartDataWhenChildHasPartData()
		{
			if (HasACommodityWithPartNumber)
			{
				var commodity = Master;
				if (!commodity.BY_PartNumber.IsEmpty)
				{
					commodity.BY_PartNumber = ZString.Empty;
				}
			}
		}

		void ClearParentCommodityDataThatAreAvailableOnChild()
		{
			var commodity = Master;
			commodity.BY_HarmonisedTariff = ZString.Empty;
			commodity.BY_GrossWeight = ZDecimal.Zero;
			commodity.BY_GrossWeightUnit = ZString.Empty;
			commodity.BY_MonetaryValue = ZDecimal.Zero;
		}
	}
}
