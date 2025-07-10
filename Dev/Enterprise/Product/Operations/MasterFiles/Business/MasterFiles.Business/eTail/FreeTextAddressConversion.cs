using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business;

public class FreeTextAddressConversion<T> : NonPersistentBusinessObject where T : BusinessObject, IConsignmentAddressProvider
{
	public FreeTextAddressConversion() : this(null) { }

	public FreeTextAddressConversion(BusinessObjectFactory factory) : base(factory)
	{
		addressesToConvert = new FreeTextAddressCollection<T>(this);
		RegisterEditableChildObject(addressesToConvert);
		AddressesToConvertView = new FreeTextAddressCollectionView<T>(addressesToConvert, this);
	}

	public void AddToList(IEnumerable<T> consignments)
	{
		if (!consignments.IsNullOrEmpty())
		{
			addressesToConvert.AddFromConsignments(consignments);
		}
	}

	public FreeTextAddressCollectionView<T> AddressesToConvertView { get; }

	public bool HasSelectedAddresses => addressesToConvert.Cast<FreeTextAddress<T>>().Any(address => address.HasSelectedAddress);

	public override bool HasChanges => addressesToConvert.Cast<FreeTextAddress<T>>().Any(address => address.HasChanges);

	public bool Finished
	{
		get => finished;
		set
		{
			if (value && OnFinished != null)
			{
				OnFinished(this, EventArgs.Empty);
			}

			finished = value;
		}
	}

	bool finished;

	public event EventHandler OnFinished;

	readonly FreeTextAddressCollection<T> addressesToConvert;
}
