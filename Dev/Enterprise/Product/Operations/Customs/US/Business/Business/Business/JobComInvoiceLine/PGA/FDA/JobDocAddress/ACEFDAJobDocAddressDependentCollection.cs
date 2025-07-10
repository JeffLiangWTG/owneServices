using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	public class ACEFDAJobDocAddressDependentCollection : JobDocAddressDependentCollection
	{
		protected override bool AllowNewCore => true;

		public ACEFDAJobDocAddressDependentCollection(IDocAddresses parent)
			: base(parent)
		{
			FDA = parent as ACEFDA;
		}

		protected readonly ACEFDA FDA;

		public new ACEFDAJobDocAddress this[int index]
		{
			get { return (ACEFDAJobDocAddress)base[index]; }
		}

		public new ACEFDAJobDocAddress AddNew()
		{
			return (ACEFDAJobDocAddress)base.AddNew();
		}

		public new ACEFDAJobDocAddress AddNew(DocAddressType docAddressType)
		{
			return (ACEFDAJobDocAddress)base.AddNew(docAddressType);
		}

		public new ACEFDAJobDocAddress AddNew(OrgAddress orgAddress)
		{
			return (ACEFDAJobDocAddress)base.AddNew(orgAddress);
		}

		public new ACEFDAJobDocAddress AddNew(DocAddressType docAddressType, int sequence)
		{
			return (ACEFDAJobDocAddress)base.AddNew(docAddressType, sequence);
		}

		public new ACEFDAJobDocAddress AddNew(OrgAddress orgAddress, DocAddressType docAddressType)
		{
			return (ACEFDAJobDocAddress)base.AddNew(orgAddress, docAddressType);
		}

		public new ACEFDAJobDocAddress CreateWithAddressType(DocAddressType docAddressType)
		{
			return (ACEFDAJobDocAddress)base.AddNew(docAddressType);
		}

		public new ACEFDAJobDocAddress CreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (ACEFDAJobDocAddress)base.CreateWithRequirement(requirement);
		}

		public new ACEFDAJobDocAddress FindByDocAddressType(DocAddressType docAddressType)
		{
			return (ACEFDAJobDocAddress)base.FindByDocAddressType(docAddressType);
		}

		public new ACEFDAJobDocAddress FindByDocAddressType(DocAddressType docAddressType, int sequence)
		{
			return (ACEFDAJobDocAddress)base.FindByDocAddressType(docAddressType, sequence);
		}

		public new ACEFDAJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType)
		{
			return new List<ACEFDAJobDocAddress>(new TypedEnumerable<ACEFDAJobDocAddress>(base.FindDocAddressesByType(docAddressType))).ToArray();
		}

		public ACEFDAJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType, ACEFDAJobDocAddress ignoreDocAddress)
		{
			return new List<ACEFDAJobDocAddress>(new TypedEnumerable<ACEFDAJobDocAddress>(base.FindDocAddressesByType(docAddressType, ignoreDocAddress))).ToArray();
		}

		public new ACEFDAJobDocAddress FindOrCreateDummyAddress(int sequence)
		{
			return (ACEFDAJobDocAddress)base.FindOrCreateDummyAddress(sequence);
		}

		public new ACEFDAJobDocAddress FindOrCreateWithDocAddressType(DocAddressType docAddressType)
		{
			return (ACEFDAJobDocAddress)base.FindOrCreateWithDocAddressType(docAddressType);
		}

		public new ACEFDAJobDocAddress FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType)
		{
			return (ACEFDAJobDocAddress)base.FindOrCreateWithDocAddressType(orgAddressPK, docAddressType);
		}

		public new ACEFDAJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (ACEFDAJobDocAddress)base.FindOrCreateWithRequirement(requirement);
		}

		public new ACEFDAJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement, int sequence)
		{
			return (ACEFDAJobDocAddress)base.FindOrCreateWithRequirement(requirement, sequence);
		}

		public IEnumerable<ACEFDAJobDocAddress> Find(ZString addressType)
		{
			return this.Cast<ACEFDAJobDocAddress>().Where(x => x.E2_AddressType == addressType);
		}

		public IEnumerable<ACEFDAJobDocAddress> Find(string[] addressTypes)
		{
			var result = new List<ACEFDAJobDocAddress>();

			foreach (var addressType in addressTypes)
			{
				result.AddRange(Find(addressType));
			}

			return result;
		}

		public void RemoveDocAddressViaDocAddressType(DocAddressType docAddressType)
		{
			var docAddress = FindByDocAddressType(docAddressType);
			if (docAddress != null)
			{
				RemoveAndDelete(docAddress);
			}
		}
	}
}
