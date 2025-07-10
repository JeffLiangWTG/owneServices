using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgParkContainerTypeLookups : AutoOrgParkContainerTypeLookups
	{
		public OrgParkContainerTypeLookups(AutoOrgParkContainerType parent) : base(parent)
		{
		}

		protected new OrgParkContainerType Parent
		{
			get { return (OrgParkContainerType)base.Parent; }
		}

		#region Container Storage Class List

		public ICodeDescriptionPairList StorageClassList
		{
			get { return Env.Registry.ContainerStorageClass; }
		}

		#endregion

		public OrgContactCollection OrganisationContacts
		{
			get
			{
				ZQuery query = new ZQuery();
				OrgHeader orgParent = Parent?.AppointedAgentPorts?.Header;
				if (orgParent != null)
				{
					query.AddToFilter(OrgContactSchema.OC_OH, orgParent.PK);
				}

				var contacts = new OrgContactCollection(Factory, query)
				{
					FilterBusinessObjectDefaults = { new FilterBusinessObjectDefault("Organisation", "Property", orgParent?.PK ?? ZGuid.Empty, false) }
				};
				return contacts;
			}
		}
	}
}
