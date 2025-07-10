using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class TWJobDocAddressDependentCollection : JobDocAddressDependentCollection
	{
		public TWJobDocAddressDependentCollection(IDocAddresses parent)
			: base(parent)
		{
		}

		public new TWJobDocAddress this[int index]
		{
			get { return (TWJobDocAddress)base[index]; }
		}

		public new TWJobDocAddress AddNew()
		{
			return (TWJobDocAddress)base.AddNew();
		}

		public new TWJobDocAddress AddNew(DocAddressType docAddressType)
		{
			return (TWJobDocAddress)base.AddNew(docAddressType);
		}

		public new TWJobDocAddress AddNew(OrgAddress orgAddress)
		{
			return (TWJobDocAddress)base.AddNew(orgAddress);
		}

		public new TWJobDocAddress AddNew(DocAddressType docAddressType, int sequence)
		{
			return (TWJobDocAddress)base.AddNew(docAddressType, sequence);
		}

		public new TWJobDocAddress AddNew(OrgAddress orgAddress, DocAddressType docAddressType)
		{
			return (TWJobDocAddress)base.AddNew(orgAddress, docAddressType);
		}

		public new TWJobDocAddress CreateWithAddressType(DocAddressType docAddressType)
		{
			return (TWJobDocAddress)base.AddNew(docAddressType);
		}

		public new TWJobDocAddress CreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (TWJobDocAddress)base.CreateWithRequirement(requirement);
		}

		public new TWJobDocAddress FindByDocAddressType(DocAddressType docAddressType)
		{
			return (TWJobDocAddress)base.FindByDocAddressType(docAddressType);
		}

		public new TWJobDocAddress FindByDocAddressType(DocAddressType docAddressType, int sequence)
		{
			return (TWJobDocAddress)base.FindByDocAddressType(docAddressType, sequence);
		}

		public new TWJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType)
		{
			return new List<TWJobDocAddress>(new TypedEnumerable<TWJobDocAddress>(base.FindDocAddressesByType(docAddressType))).ToArray();
		}

		public TWJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType, TWJobDocAddress ignoreDocAddress)
		{
			return new List<TWJobDocAddress>(new TypedEnumerable<TWJobDocAddress>(base.FindDocAddressesByType(docAddressType, ignoreDocAddress))).ToArray();
		}

		public new TWJobDocAddress FindOrCreateDummyAddress(int sequence)
		{
			return (TWJobDocAddress)base.FindOrCreateDummyAddress(sequence);
		}

		public new TWJobDocAddress FindOrCreateWithDocAddressType(DocAddressType docAddressType)
		{
			return (TWJobDocAddress)base.FindOrCreateWithDocAddressType(docAddressType);
		}

		public new TWJobDocAddress FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType)
		{
			return (TWJobDocAddress)base.FindOrCreateWithDocAddressType(orgAddressPK, docAddressType);
		}

		public new TWJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (TWJobDocAddress)base.FindOrCreateWithRequirement(requirement);
		}

		public new TWJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement, int sequence)
		{
			return (TWJobDocAddress)base.FindOrCreateWithRequirement(requirement, sequence);
		}
	}
}
