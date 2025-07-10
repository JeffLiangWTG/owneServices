using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business;

public class InvoiceAmountBoundariesRegistryItem : StronglyTypedRegistryItem<InvoiceAmountBoundaryCollection>
{
	public InvoiceAmountBoundariesRegistryItem(string name, MultilingualString category, MultilingualString caption,
		MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, InvoiceAmountBoundaryCollection defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new InvoiceAmountBoundariesRegistryDataType(),
			storage, options, defaultValue))
	{
	}

	protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
	{
		base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, GetSortedCollection(newValue as InvoiceAmountBoundaryCollection));
	}

	protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
	{
		return GetSortedCollection(base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as InvoiceAmountBoundaryCollection);
	}

	InvoiceAmountBoundaryCollection GetSortedCollection(InvoiceAmountBoundaryCollection collection)
	{
		if (collection != null)
		{
			collection.Sort(new Comparison<InvoiceAmountBoundary>(
				(a, b) =>
				{
					int result = b.StartDate.CompareTo(a.StartDate);

					if (result == 0)
					{
						result = b.EndDate.CompareTo(a.EndDate);
						if (result == 0)
						{
							result = (b.Amount < a.Amount) ? -1 : (b.Amount > a.Amount) ? 1 : 0;
						}
					}

					return result;
				}));
		}
		return collection;
	}
}

[RegistryEditor("Enterprise.Accounting.Registry.GUI.InvoiceAmountBoundariesRegistryItemEditor, Enterprise.Accounting.GUI")]
class InvoiceAmountBoundariesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<InvoiceAmountBoundaryCollection>
{
}
