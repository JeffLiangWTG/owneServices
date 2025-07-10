using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.UrsNamedAccount)]
	public class OrgCarrierNamedAccountCollection : BusinessObjectCollection<OrgCarrierNamedAccount>
	{
		public OrgCarrierNamedAccountCollection(OrgHeader master, SchemaColumn relationshipColumn)
			: base(Argument.NotNull(master, nameof(master)).Factory)
		{
			Master = master;
			RelationshipColumn = relationshipColumn;
		}

		public OrgCarrierNamedAccountCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgCarrierNamedAccountCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		#region Relationship

		protected override ZQuery CreateRelationshipFilter()
		{
			if (IsInRelationship)
			{
				return new ZQuery(RelationshipColumn, Master.PK);
			}
			return base.CreateRelationshipFilter();
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			if (IsInRelationship)
			{
				child[RelationshipColumn] = Master.PK;
			}
		}

		public bool IsInRelationship => Master != null && RelationshipColumn != null;
		public readonly OrgHeader Master;
		public readonly SchemaColumn RelationshipColumn;

		#endregion
	}
}
