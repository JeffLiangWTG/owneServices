using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationMergeData
	{
		public OrganisationMergeData(ZGuid oldOrg, ZGuid newOrg, MergeOrgAddressCollection mergeAddressCollection, MergeOrgContactCollection mergeContactsCollection)
		{
			OldOrganisation = oldOrg;
			NewOrganisation = newOrg;
			MergeAddressCollection = mergeAddressCollection;
			MergeContactsCollection = mergeContactsCollection;
			MergedAddresses = new Dictionary<ZGuid, ZGuid>();
			InitialBrands = new StringCollectionX();
			Connection = Db.Connection;
		}

		public ZGuid OldOrganisation { get; }
		public ZGuid NewOrganisation { get; }
		public MergeOrgAddressCollection MergeAddressCollection { get; }
		public MergeOrgContactCollection MergeContactsCollection { get; }
		public Dictionary<ZGuid, ZGuid> MergedAddresses { get; }
		public StringCollectionX InitialBrands { get; }
		public MergeOrgHeader MergeOrgHeader { get; set; }
		public DbConnection Connection { get; set; }
		public BusinessObjectFactory Factory => new BusinessObjectFactory(Connection);

		public OrgHeader OldOrgHeader
		{
			get
			{
				if (oldOrgHeader == null)
				{
					if (MergeOrgHeader != null)
					{
						oldOrgHeader = MergeOrgHeader.OldOrganisation;
					}
					else
					{
						oldOrgHeader = Factory.Load<OrgHeader>(OldOrganisation);
					}
				}
				return oldOrgHeader;
			}
		}

		public OrgHeader NewOrgHeader
		{
			get
			{
				if (newOrgHeader == null)
				{
					if (MergeOrgHeader != null)
					{
						newOrgHeader = MergeOrgHeader.NewOrganisation;
					}
					else
					{
						newOrgHeader = Factory.Load<OrgHeader>(NewOrganisation);
					}
				}
				return newOrgHeader;
			}
		}

		OrgHeader oldOrgHeader;
		OrgHeader newOrgHeader;
	}
}
