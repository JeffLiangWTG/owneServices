using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class MergeOrgContactCollection : MergeOrgElementCollection<MergeOrgContact>
	{
		public MergeOrgContactCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public MergeOrgContactCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public MergeOrgContactCollection(BusinessObjectFactory factory, OrgHeader oldOrg, OrgHeader newOrg)
			: this(factory, oldOrg, newOrg, null)
		{
		}

		/// <summary>
		/// Creates MergeOrgContactCollection
		/// </summary>
		/// <param name="factory">Factory</param>
		/// <param name="oldOrg">Old Org</param>
		/// <param name="newOrg">New Org</param>
		/// <param name="newContactCollection">For performance - if New Org Contact collection is already loaded then dont have to load it again.</param>
		/// <param name="ignoreDummyContact">Will not load DummyContact when set it to true</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")] //Due to virtual method Add(mergeContact)
		public MergeOrgContactCollection(BusinessObjectFactory factory, OrgHeader oldOrg, OrgHeader newOrg, BusinessObjectCollection newContactCollection)
			: base(factory)
		{
			OrgHeader oldOrgReloaded = factory.Load<OrgHeader>(oldOrg.PK)
				?? (OrgHeader)factory.ImportFromAnotherFactory(oldOrg);

			var query = new ZQuery();

			OrgContactDependentCollection contactCollection = new OrgContactDependentCollection(oldOrgReloaded, query);
			contactCollection.Load();
			foreach (OrgContact contact in contactCollection)
			{
				MergeOrgContact mergeContact = new MergeOrgContact(factory, contact, newOrg, newContactCollection);
				Add(mergeContact);
			}
		}
	}
}
