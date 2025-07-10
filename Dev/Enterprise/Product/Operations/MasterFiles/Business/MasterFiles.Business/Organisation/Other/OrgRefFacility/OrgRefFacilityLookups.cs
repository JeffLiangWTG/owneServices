namespace Enterprise.MasterFiles.Business
{
	public class OrgRefFacilityLookups : AutoOrgRefFacilityLookups
	{
		public OrgRefFacilityLookups(AutoOrgRefFacility parent) : base(parent)
		{
		}

		#region ActiveAddresses

		OrgAddressDependentCollection Addresses
		{
			get { return Parent.Organization.Addresses; }
		}

		public ActiveOrAllAddressesCollection ActiveAddresses
		{
			get { return new ActiveOrAllAddressesCollection(Addresses); }
		}

		#endregion

		#region RefFacilites

		public RefFacilityCollection RefFacilities
		{
			get { return new RefFacilityCollection(Factory); }
		}

		#endregion

		new OrgRefFacility Parent => (OrgRefFacility)base.Parent;
	}
}
