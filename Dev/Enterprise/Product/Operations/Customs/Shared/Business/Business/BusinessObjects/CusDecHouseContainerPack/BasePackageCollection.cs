using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BasePackageCollection : BusinessObjectCollectionView<BasePackage>
	{
		public BasePackageCollection(BasePackingGroup packingGroup)
			: base((BusinessObjectCollection)packingGroup.Declaration.Packages)
		{
			this.PackingGroup = packingGroup;
		}

		public readonly BasePackingGroup PackingGroup;

		public bool HasMultiplePackages
		{
			get { return Count > 1; }
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			if (PackingGroup != null && !PackingGroup.IsDeleted && PackingGroup.Bill != null && !PackingGroup.Bill.IsDeleted && !PackingGroup.Bill.IsValidationSuspended)
			{
				PackingGroup.Bill.Validation.ValidateCU_BillNum();
				PackingGroup.Declaration?.PackagesActualPackageCountInfo.RefreshBinding();
			}
		}

		public BasePackage GetPackageWithPackTypeAndCount(ZString packType, ZInt packCount, bool shouldGetEmptyPackageIfNullResult)
		{
			List<PackingSearchResult> result = new List<PackingSearchResult>();
			foreach (BasePackage package in this)
			{
				if (package.CW_PackQty == packCount && package.CW_PackType == packType)
				{
					result.Add(new PackingSearchResult(1, package));
					break;
				}
				else if (shouldGetEmptyPackageIfNullResult && package.CW_PackQty.IsEmpty)
				{
					result.Add(new PackingSearchResult(2, package));
				}
			}
			if (result.Count > 1)
			{
				result.Sort(new PackingSearchResultSorter());
			}

			return result.Count > 0 ? (BasePackage)result[0].Packing : null;
		}

		public BasePackage GetPackageWithPackTypeAndCount(ZString packType, ZInt packCount)
		{
			return GetPackageWithPackTypeAndCount(packType, packCount, false);
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			((BasePackage)child).Declaration = PackingGroup.Declaration;
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return PackingGroup != null && (element as BasePackage).CW_CR_HouseContainer == PackingGroup.PK;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((BasePackage)child).CW_CR_HouseContainer = PackingGroup.PK;
		}
	}
}
