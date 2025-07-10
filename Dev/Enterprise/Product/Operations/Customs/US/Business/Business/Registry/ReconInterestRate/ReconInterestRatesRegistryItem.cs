using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class ReconInterestRatesRegistryItem : StronglyTypedRegistryItem<ReconInterestRateCollection>
	{
		public ReconInterestRatesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ReconInterestRatesRegistryDataType(), storage, RegistryOptions.Default))
		{
		}

		public ReconInterestRatesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ReconInterestRateCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ReconInterestRatesRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, GetSortedCollection(newValue as ReconInterestRateCollection));
		}

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return GetSortedCollection(base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as ReconInterestRateCollection);
		}

		ReconInterestRateCollection GetSortedCollection(ReconInterestRateCollection collection)
		{
			if (collection != null)
			{
				collection.Sort(new Comparison<ReconInterestRate>(
					delegate(ReconInterestRate a, ReconInterestRate b)
					{
						int result = b.StartDate.CompareTo(a.StartDate);

						if (result == 0)
						{
							result = b.EndDate.CompareTo(a.EndDate);
							if (result == 0)
							{
								result = (b.Rate < a.Rate) ? -1 : (b.Rate > a.Rate) ? 1 : 0;
							}
						}

						return result;
					}));
			}
			return collection;
		}
	}
}
