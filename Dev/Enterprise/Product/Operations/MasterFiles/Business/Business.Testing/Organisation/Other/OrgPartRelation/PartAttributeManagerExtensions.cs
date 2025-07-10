namespace Enterprise.MasterFiles.Business.Testing
{
	public static class PartAttributeManagerExtensions
	{
		public static void SetProductToUseAttribute(this PartAttributeManager partAttributeManager, OrgSupplierPart part, int attributeNumber, bool use)
		{
			OrgPartRelation relation = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(partAttributeManager.Organisation.PK, OrgPartRelation.RelationshipTypes.Owner);
			if (relation != null)
			{
				switch (attributeNumber)
				{
					case 1:
						relation.OU_UsePartAttrib1 = use;
						break;
					case 2:
						relation.OU_UsePartAttrib2 = use;
						break;
					case 3:
						relation.OU_UsePartAttrib3 = use;
						break;
					case 4:
						relation.OU_UseExpiryDate = use;
						break;
					case 5:
						relation.OU_UsePackingDate = use;
						break;
					case 6:
						relation.OU_UseSerialNumber = use;
						break;
				}
			}
		}
	}
}
