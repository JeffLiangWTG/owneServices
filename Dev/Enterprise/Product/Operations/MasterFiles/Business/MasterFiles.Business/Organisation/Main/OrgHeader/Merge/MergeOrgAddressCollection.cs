using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class MergeOrgAddressCollection : MergeOrgElementCollection<MergeOrgAddress>
	{
		public MergeOrgAddressCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public MergeOrgAddressCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public MergeOrgAddressCollection(BusinessObjectFactory factory, OrgHeader oldOrg, OrgHeader newOrg)
			: this(factory, oldOrg, newOrg, null)
		{ }

		/// <summary>
		/// Creates MergeOrgAddressCollection
		/// </summary>
		/// <param name="factory">Factory</param>
		/// <param name="oldOrg">Old Org</param>
		/// <param name="newOrg">New Org</param>
		/// <param name="newOrgAddressCollection">For performance - if New Org Address collection is already loaded then dont have to load it again.</param>
		public MergeOrgAddressCollection(BusinessObjectFactory factory, OrgHeader oldOrg, OrgHeader newOrg, BusinessObjectCollection newOrgAddressCollection)
			: base(factory)
		{
			OrgHeader oldOrgReloaded = factory.Load<OrgHeader>(oldOrg.PK)
				?? (OrgHeader)factory.ImportFromAnotherFactory(oldOrg);

			OrgAddressDependentCollection addressCollection = new OrgAddressDependentCollection(oldOrgReloaded);
			addressCollection.Load();
			foreach (OrgAddress address in addressCollection)
			{
				MergeOrgAddress mergeAddress = new MergeOrgAddress(factory, address, newOrg, newOrgAddressCollection);
				Add(mergeAddress);
			}
		}
	}
}
