using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFDocAddressDependentCollection : JobDocAddressDependentCollection, IBusinessObjectCollection<ISFDocAddress>
	{
		public ISFDocAddressDependentCollection(CusISFHeader header)
			: base(header)
		{
		}

		public new ISFDocAddress this[int index]
		{
			get { return (ISFDocAddress)base[index]; }
		}

		ISFDocAddress IBusinessObjectCollection<ISFDocAddress>.this[int index]
		{
			get { return (ISFDocAddress)base[index]; }
		}

		public new ISFDocAddress AddNew()
		{
			return (ISFDocAddress)base.AddNew();
		}

		public new ISFDocAddress AddNew(DocAddressType docAddressType)
		{
			return (ISFDocAddress)base.AddNew(docAddressType);
		}

		public new ISFDocAddress AddNew(OrgAddress orgAddress)
		{
			return (ISFDocAddress)base.AddNew(orgAddress);
		}

		public new ISFDocAddress AddNew(DocAddressType docAddressType, int sequence)
		{
			return (ISFDocAddress)base.AddNew(docAddressType, sequence);
		}

		public new ISFDocAddress AddNew(OrgAddress orgAddress, DocAddressType docAddressType)
		{
			return (ISFDocAddress)base.AddNew(orgAddress, docAddressType);
		}

		public new ISFDocAddress CreateWithAddressType(DocAddressType docAddressType)
		{
			return (ISFDocAddress)base.AddNew(docAddressType);
		}

		public ISFDocAddress CreateWithRequirement(ISFDocAddressRequirement requirement)
		{
			return (ISFDocAddress)base.CreateWithRequirement(requirement);
		}

		public new ISFDocAddress FindByDocAddressType(DocAddressType docAddressType)
		{
			return (ISFDocAddress)base.FindByDocAddressType(docAddressType);
		}

		public new ISFDocAddress FindByDocAddressType(DocAddressType docAddressType, int sequence)
		{
			return (ISFDocAddress)base.FindByDocAddressType(docAddressType, sequence);
		}

		public new ISFDocAddress[] FindDocAddressesByType(DocAddressType docAddressType)
		{
			return new List<ISFDocAddress>(new TypedEnumerable<ISFDocAddress>(base.FindDocAddressesByType(docAddressType))).ToArray();
		}

		public ISFDocAddress[] FindDocAddressesByType(DocAddressType docAddressType, ISFDocAddress ignoreDocAddress)
		{
			return new List<ISFDocAddress>(new TypedEnumerable<ISFDocAddress>(base.FindDocAddressesByType(docAddressType, ignoreDocAddress))).ToArray();
		}

		public new ISFDocAddress FindOrCreateDummyAddress(int sequence)
		{
			return (ISFDocAddress)base.FindOrCreateDummyAddress(sequence);
		}

		public new ISFDocAddress FindOrCreateWithDocAddressType(DocAddressType docAddressType)
		{
			return (ISFDocAddress)base.FindOrCreateWithDocAddressType(docAddressType);
		}

		public new ISFDocAddress FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType)
		{
			return (ISFDocAddress)base.FindOrCreateWithDocAddressType(orgAddressPK, docAddressType);
		}

		public ISFDocAddress FindOrCreateWithRequirement(ISFDocAddressRequirement requirement)
		{
			return (ISFDocAddress)base.FindOrCreateWithRequirement(requirement);
		}

		public ISFDocAddress FindOrCreateWithRequirement(ISFDocAddressRequirement requirement, int sequence)
		{
			return (ISFDocAddress)base.FindOrCreateWithRequirement(requirement, sequence);
		}

		public IEnumerator<ISFDocAddress> GetEnumerator() => Elements.Cast<ISFDocAddress>().GetEnumerator();

		ISFDocAddress IBusinessObjectCollection<ISFDocAddress>.AddNew()
		{
			return AddNew();
		}
	}
}
