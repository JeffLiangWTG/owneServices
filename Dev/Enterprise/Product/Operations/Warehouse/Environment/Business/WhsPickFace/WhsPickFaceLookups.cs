using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPickFaceLookups : AutoWhsPickFaceLookups
	{
		public WhsPickFaceLookups(AutoWhsPickFace parent)
			: base(parent)
		{
		}

		new WhsPickFace Parent
		{
			get { return (WhsPickFace)base.Parent; }
		}

		public virtual WhsWarehouseCollection Warehouses
		{
			get
			{
				WhsWarehouseCollectionWithSecurityCheck result = new WhsWarehouseCollectionWithSecurityCheck(Factory);
				result.Load();
				return result;
			}
		}

		public override OrgHeaderCollection Clients
		{
			get
			{
				var result = new OrgHeaderCollection(Factory);
				if (!Parent.IsDeleted)
				{
					var supplierPart = Parent.SupplierPart;
					if (supplierPart != null)
					{
						var relations = supplierPart.RelatedOrganisations;
						foreach (OrgPartRelation relation in relations)
						{
							if ((relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner ||
								relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both) && relation.Organisation != null)
							{
								result.Add(relation.Organisation);
							}
						}
					}
				}
				return result;
			}
		}

		#region Locations

		public WhsLocationCollection Locations
		{
			get
			{
				var warehouse = Parent.Warehouse;
				return (warehouse != null) ? new WhsLocationCollection(warehouse) : new WhsLocationCollection(Factory);
			}
		}

		#endregion
	}
}
