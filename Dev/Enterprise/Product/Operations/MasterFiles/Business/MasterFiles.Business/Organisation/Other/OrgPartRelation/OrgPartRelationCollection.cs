using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartRelationCollection : DependentBusinessObjectCollection<OrgPartRelation, OrgSupplierPart>
	{
		/// <summary>
		/// DO NOT USE THIS CONSTRUCTOR
		/// This is to build Customs.Business which has an autogenrated property in AutoCusSupImpClassOverrideLookups which uses this constructor
		/// </summary>
		public OrgPartRelationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgPartRelationCollection(OrgSupplierPart parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public OrgPartRelation AddSupplier(IOrgHeader supplier)
		{
			var relation = AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			return relation;
		}

		public OrgPartRelation AddOwner(IOrgHeader owner)
		{
			var relation = AddNew();
			relation.OU_OH = owner.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			return relation;
		}

		public OrgPartRelation FindByOrganisationAndRelationship(OrgHeader org, ZString relationshipType)
		{
			if (org != null)
			{
				return FindByOrganisationPKAndRelationship(org.PK, relationshipType);
			}
			else
			{
				return null;
			}
		}

		public OrgPartRelation FindByOrganisationPKAndExactRelationship(ZGuid orgPK, ZString relationshipType)
		{
			OrgPartRelation result = null;
			foreach (OrgPartRelation relation in this)
			{
				if (relation.OU_OH == orgPK &&
					relation.OU_Relationship == relationshipType)
				{
					result = relation;
					break;
				}
			}
			return result;
		}

		public OrgPartRelation FindByOrganisationPKAndRelationship(ZGuid orgPK, ZString relationshipType)
		{
			OrgPartRelation result = null;
			foreach (OrgPartRelation relation in this)
			{
				if (relation.OU_OH == orgPK && OrgRelationshipCamparer(relation.OU_Relationship, relationshipType))
				{
					result = relation;
					break;
				}
			}
			return result;
		}

		public OrgPartRelation FindFirstByOrganisationPK(ZGuid orgPK)
		{
			OrgPartRelation result = null;
			foreach (OrgPartRelation relation in this)
			{
				if (relation.OU_OH == orgPK)
				{
					result = relation;
					break;
				}
			}
			return result;
		}

		bool OrgRelationshipCamparer(string relationship1, string relationship2)
		{
			return relationship1 == relationship2 || (relationship1 == OrgPartRelation.RelationshipTypes.Both &&
				 (relationship2 == OrgPartRelation.RelationshipTypes.Owner || relationship2 == OrgPartRelation.RelationshipTypes.Supplier));
		}

		public OrgPartRelation AddOrganisationIfNotExist(ZGuid orgPK, ZString relationshipType)
		{
			return AddOrganisationIfNotExist(orgPK, relationshipType, false);
		}

		public OrgPartRelation AddOrganisationIfNotExist(ZGuid orgPK, ZString relationshipType, bool isFormController)
		{
			OrgPartRelation relation = FindByOrganisationPKAndRelationship(orgPK, relationshipType);
			if (relation == null)
			{
				relation = AddNew();
				relation.OU_Relationship = relationshipType;
				relation.OU_OH = orgPK;
				relation.OU_FormLayoutController = isFormController;
			}
			return relation;
		}

		public void DeleteOrganisationIfExists(ZGuid orgPK, ZString relationshipType)
		{
			OrgPartRelation relation = FindByOrganisationPKAndExactRelationship(orgPK, relationshipType);
			if (relation != null)
			{
				relation.Delete();
			}
		}

		public OrgPartRelation FindByOH_CodeAndRelationship(ZString oH_Code, ZString relationship)
		{
			foreach (OrgPartRelation relation in this)
			{
				if (relation.Organisation != null &&
					relation.Organisation.OH_Code == oH_Code &&
					(relation.OU_Relationship == relationship || relation.IsBoth))
				{
					return relation;
				}
			}
			return null;
		}

		public OrgPartRelation[] BuyerRelations
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach (OrgPartRelation relation in this)
				{
					if (relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner ||
							relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both)
					{
						result.Add(relation);
					}
				}
				return (OrgPartRelation[])result.ToArray(typeof(OrgPartRelation));
			}
		}

		public LCMarginPercentages GetLCMarginPercentagesForFallBack(OrgHeader consignee)
		{
			LCMarginPercentages result = new LCMarginPercentages();
			if (consignee != null)
			{
				foreach (OrgPartRelation relation in BuyerRelations)
				{
					if (relation.OU_OH == consignee.PK)
					{
						result.LCMarginPercentage1 = relation.OU_LandedCostMarginPercent1;
						result.LCMarginPercentage2 = relation.OU_LandedCostMarginPercent2;
						result.LCMarginPercentage3 = relation.OU_LandedCostMarginPercent3;
						break;
					}
				}
			}
			return result;
		}
	}
}
