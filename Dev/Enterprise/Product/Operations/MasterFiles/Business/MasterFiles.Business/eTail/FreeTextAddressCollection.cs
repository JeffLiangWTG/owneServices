using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business;

public class FreeTextAddressCollection<T> : NonPersistentBusinessObjectCollection<FreeTextAddress<T>> where T : BusinessObject, IConsignmentAddressProvider
{
	public FreeTextAddressCollection(FreeTextAddressConversion<T> conversion) : base(conversion.Factory)
	{
	}

	public void AddFromConsignments(IEnumerable<T> consignments)
	{
		foreach (var originalConsignment in consignments)
		{
			// Ensure consignment is in our factory
			var consignment = Factory.ImportFromAnotherFactorySafe(originalConsignment);

			if (!consignment.ConsigneeIsOrganisation)
			{
				Add(new FreeTextAddress<T>(consignment, ConsignmentAddressType.Consignee));
			}

			if (!consignment.ShipperIsOrganisation)
			{
				Add(new FreeTextAddress<T>(consignment, ConsignmentAddressType.Shipper));
			}
		}
	}

	protected override BusinessObject CreateNonPersistentBusinessObject() => new FreeTextAddress<T>(Factory);
}

public class FreeTextAddressCollectionView<T> : NonPersistentBusinessObjectCollectionView<FreeTextAddress<T>> where T : BusinessObject, IConsignmentAddressProvider
{
	public FreeTextAddressCollectionView(FreeTextAddressCollection<T> addresses, FreeTextAddressConversion<T> conversion) : base(addresses)
	{
		this.conversion = conversion;
	}

	readonly FreeTextAddressConversion<T> conversion;

	protected override void OnCountChanged(CollectionCountChangedEventArgs e)
	{
		if (e.ItemRemoved && Count <= 0)
		{
			conversion.Finished = true;
		}
	}

	protected override bool IsThisPartOfTheCollection(BusinessObject element)
	{
		return element is FreeTextAddress<T> address && !address.AddressSettled;
	}

	protected override bool AllowNewCore => false;

	protected override FreeTextAddress<T> CreateNonPersistentBusinessObject()
	{
		throw new NotImplementedException();
	}
}
