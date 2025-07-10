using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgWhsClientAccountAssociationCollection : ActiveBusinessObjectCollection<OrgWhsClientAccountAssociation>
	{
		public OrgWhsClientAccountAssociationCollection(OrgHeader master)
			: base(Argument.NotNull(master, nameof(master)).Factory, master, new ZQuery(), OrgWhsClientAccountAssociationSchema.OWC_OH_Client)
		{
		}
	}
}
