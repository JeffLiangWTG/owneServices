using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgManagementRelatedParentCollection : OrgManagementRelatedCollection
	{
		public OrgManagementRelatedParentCollection(OrgHeader organisation)
			: base(organisation, OrgManagementRelatedParty.GetRelatedManagementParentsQuery(organisation))
		{
		}

		#region Organisations

		public sealed override IEnumerable<OrgHeader> Organisations
		{
			get { return this.Select(relation => relation.ParentOrganisation).Where(organisation => organisation != null); }
		}

		#endregion

		#region CheckIsValidOrganisation

		public sealed override RelationValidationResult CheckIsValidOrganisation(OrgHeader parentOrganisation, bool parentOrganisationMustBeInDatabase)
		{
			if (parentOrganisation == null)
			{
				return new RelationValidationResult(true);
			}

			if (parentOrganisation.PK == Organisation.PK)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("9b145365-557f-44de-ba24-cb227a84b80d", "Can not make {0} the parent of itself.", Organisation.HumanReadableName));
			}

			if (parentOrganisationMustBeInDatabase && !parentOrganisation.IsInDatabase)
			{
				return new RelationValidationResult(false, GetParentMustBeSavedMessage(parentOrganisation));
			}

			var existingParentParty = Factory.Load<OrgManagementRelatedParty>(OrgManagementRelatedParty.GetRelatedManagementParentsQuery(Organisation)).Where(x => x.PR_OH_RelatedParty != parentOrganisation.PK).Select(x => x.RelatedParty).FirstOrDefault();
			if (existingParentParty != null)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("982DB169-75FD-4F22-A6C1-28A83FD31578", "{0} is already the parent of {1}. {1} can only have one parent.", existingParentParty.HumanReadableName, Organisation.HumanReadableName));
			}

			var relationshipSequence = HierarchicalSequenceBuilder.GetRelationshipSequence(x => x.RelatedManagementSubsidiaryRelations.Organisations, Organisation, parentOrganisation, Tuple.Create(parentOrganisation, Organisation));
			if (relationshipSequence.Any())
			{
				return new RelationValidationResult(false,
					ResString.GetMultilingualString("2767F215-EE61-455C-B061-A4645F22659A", @"{0} is already a related descendant of {1}.
Making {0} the parent of {1} would create an illegal cycle.

Conflicting relationship sequence: {2}",
					parentOrganisation.HumanReadableName,
					Organisation.HumanReadableName,
					string.Join(" > ", relationshipSequence.Select(organisation => organisation.HumanReadableName))));
			}

			return new RelationValidationResult(true);
		}

		public static MultilingualString GetParentMustBeSavedMessage(OrgHeader parentOrganisation)
		{
			return ResString.GetMultilingualString("2319F943-2F78-4D94-B3D1-825CBB41DADE", "{0} must be saved before it can be a parent of another organization.", parentOrganisation.HumanReadableName);
		}

		#endregion

		#region Add Organisation

		public sealed override RelationUpdateResult AddOrganisation(OrgHeader parentOrganisation)
		{
			var existingRelation = this.FirstOrDefault(x => x.PR_OH_RelatedParty == parentOrganisation.PK);
			if (existingRelation != null)
			{
				return new RelationUpdateResult(false, ResString.GetMultilingualString("637053cc-bb0f-4f18-bf1c-49e21310055b", "{0} is already a parent of {1}.", parentOrganisation.HumanReadableName, Organisation.HumanReadableName));
			}

			var validationResult = CheckIsValidOrganisation(parentOrganisation, true);
			if (!validationResult.IsValid)
			{
				return new RelationUpdateResult(false, validationResult.Reason);
			}

			var backwardsValidationResult = parentOrganisation.RelatedManagementSubsidiaryRelations.CheckIsValidOrganisation(Organisation, false);
			if (!backwardsValidationResult.IsValid)
			{
				return new RelationUpdateResult(false, backwardsValidationResult.Reason);
			}

			AddOrganisationWithoutCheckingValid(parentOrganisation);
			return new RelationUpdateResult(true);
		}

		public override void AddOrganisationWithoutCheckingValid(OrgHeader organisation)
		{
			var relation = AddNew();
			relation.ParentOrganisation = organisation;
		}

		#endregion

		#region Remove Organisation

		public sealed override RelationUpdateResult RemoveOrganisation(OrgHeader parentOrganisation)
		{
			var relations = this.Where(x => x.PR_OH_RelatedParty == parentOrganisation.PK).ToArray();
			if (relations.Length > 0)
			{
				foreach (var relation in relations)
				{
					Delete(relation);
				}
				return new RelationUpdateResult(true);
			}
			else
			{
				return new RelationUpdateResult(false, ResString.GetMultilingualString("d027bb96-b8a3-42b0-8411-cf1b01d5b02a", "{0} is not a parent of {1}.", parentOrganisation.HumanReadableName, Organisation.HumanReadableName));
			}
		}

		#endregion

		#region Snapshot

		public IOrgManagementRelatedCollectionSnapshot TakeSnapshot()
		{
			return new Snapshot(this);
		}

		class Snapshot : IOrgManagementRelatedCollectionSnapshot
		{
			public Snapshot(OrgManagementRelatedParentCollection relatedCollection)
			{
				Collection = relatedCollection;
				OrganisationsAtSnapshot = relatedCollection.Organisations.ToArray();
			}

			public void Restore()
			{
				var relationsToRemove = new HashSet<OrgManagementRelatedParty>(Collection);
				var organisationsToAdd = new HashSet<OrgHeader>(OrganisationsAtSnapshot);

				foreach (var relation in Collection)
				{
					var organisation = relation.ParentOrganisation;
					if (organisationsToAdd.Contains(organisation))
					{
						organisationsToAdd.Remove(organisation);
						relationsToRemove.Remove(relation);
					}
				}

				foreach (var relation in relationsToRemove)
				{
					relation.Delete();
				}

				foreach (var organisation in organisationsToAdd)
				{
					Collection.AddNewRelation(organisation);
				}
			}

			readonly OrgManagementRelatedParentCollection Collection;
			readonly OrgHeader[] OrganisationsAtSnapshot;
		}

		#endregion

		#region AddNewRelation

		public OrgManagementRelatedParty AddNewRelation(OrgHeader organisation)
		{
			var relation = AddNew();
			using (relation.SuspendSettingHasChanges())
			using (relation.GetValidationSuspender())
			{
				relation.ParentOrganisation = organisation;
			}

			return relation;
		}

		#endregion

		#region Implementation

		protected override void SetDefaultsForNewElementCore(OrgManagementRelatedParty newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.SubsidiaryOrganisation = Organisation;
		}

		#endregion
	}
}
