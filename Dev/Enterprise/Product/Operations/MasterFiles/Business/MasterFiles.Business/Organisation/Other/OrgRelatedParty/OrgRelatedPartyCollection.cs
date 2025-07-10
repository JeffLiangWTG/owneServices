using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.EnterpriseBusinessObject;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRelatedPartyCollection : BusinessObjectCollection<OrgRelatedParty>
	{
		public OrgRelatedPartyCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public OrgRelatedPartyCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}
	}

	public class OrgRelatedPartyDependentCollection : DependentBusinessObjectCollection<OrgRelatedParty, OrgHeader>
	{
		public OrgRelatedPartyDependentCollection(OrgHeader parentOrganisation, BusinessObjectFactory factory)
			: base(parentOrganisation, factory)
		{
		}

		#region Implementation

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return OrgRelatedPartySchema.PR_OH_Parent; }
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			if (child.IsInDatabase && !child.IsDeleted && forDelete)
			{
				((OrgRelatedParty)child).AddParentRelatedRelationshipLog(LoggingAction.Detach);
			}

			base.RemoveCollectionRelationshipsCore(child, forDelete);
		}

		#endregion
	}
}
