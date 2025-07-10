using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class TWConsignorOrConsigneeAddressDependentCollection<T> : JobDocAddressDependentCollection
		where T : TWConsignorOrConsigneeAddress
	{
		public TWConsignorOrConsigneeAddressDependentCollection(IDocAddresses parent)
			: base(parent)
		{
		}

		public new T this[int index]
		{
			get { return (T)base[index]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}

		public new T AddNew(DocAddressType docAddressType)
		{
			return (T)base.AddNew(docAddressType);
		}

		public new T AddNew(OrgAddress orgAddress)
		{
			return (T)base.AddNew(orgAddress);
		}

		public new T AddNew(DocAddressType docAddressType, int sequence)
		{
			return (T)base.AddNew(docAddressType, sequence);
		}

		public new T AddNew(OrgAddress orgAddress, DocAddressType docAddressType)
		{
			return (T)base.AddNew(orgAddress, docAddressType);
		}

		public new T CreateWithAddressType(DocAddressType docAddressType)
		{
			return (T)base.AddNew(docAddressType);
		}

		public new T CreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (T)base.CreateWithRequirement(requirement);
		}

		public new TWConsignorOrConsigneeAddress FindByDocAddressType(DocAddressType docAddressType)
		{
			return (T)base.FindByDocAddressType(docAddressType);
		}

		public new T FindByDocAddressType(DocAddressType docAddressType, int sequence)
		{
			return (T)base.FindByDocAddressType(docAddressType, sequence);
		}

		public new T[] FindDocAddressesByType(DocAddressType docAddressType)
		{
			return new List<T>(new TypedEnumerable<T>(base.FindDocAddressesByType(docAddressType))).ToArray();
		}

		public T[] FindDocAddressesByType(DocAddressType docAddressType, T ignoreDocAddress)
		{
			return new List<T>(new TypedEnumerable<T>(base.FindDocAddressesByType(docAddressType, ignoreDocAddress))).ToArray();
		}

		public new T FindOrCreateDummyAddress(int sequence)
		{
			return (T)base.FindOrCreateDummyAddress(sequence);
		}

		public new T FindOrCreateWithDocAddressType(DocAddressType docAddressType)
		{
			return (T)base.FindOrCreateWithDocAddressType(docAddressType);
		}

		public new T FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType)
		{
			return (T)base.FindOrCreateWithDocAddressType(orgAddressPK, docAddressType);
		}

		public new T FindOrCreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (T)base.FindOrCreateWithRequirement(requirement);
		}

		public new T FindOrCreateWithRequirement(JobDocAddressRequirement requirement, int sequence)
		{
			return (T)base.FindOrCreateWithRequirement(requirement, sequence);
		}
	}
}
