using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class OrgManagementRelatedSubsidiaryCollection : OrgManagementRelatedCollection
	{
		public OrgManagementRelatedSubsidiaryCollection(OrgHeader organisation)
			: base(organisation, OrgManagementRelatedParty.GetRelatedManagementSubsidiariesQuery(organisation))
		{
		}

		#region Organisations

		public sealed override IEnumerable<OrgHeader> Organisations
		{
			get { return this.Select(relation => relation.SubsidiaryOrganisation).Where(organisation => organisation != null); }
		}

		#endregion

		#region CheckIsValidOrganisation

		public sealed override RelationValidationResult CheckIsValidOrganisation(OrgHeader subsidiaryOrganisation, bool subsidiaryOrganisationMustBeInDatabase)
		{
			if (subsidiaryOrganisation == null)
			{
				throw new ArgumentNullException(nameof(subsidiaryOrganisation));
			}

			if (subsidiaryOrganisation.PK == Organisation.PK)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("C5248A97-62CE-43A2-91E0-29468A447FFF", "Can not make {0} a subsidiary of itself.", Organisation.HumanReadableName));
			}

			if (subsidiaryOrganisationMustBeInDatabase && !subsidiaryOrganisation.IsInDatabase)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("79F9A1AF-1C93-46B6-BD7C-5424FF48213B", "{0} must be saved before it can be a subsidiary of another organization.", subsidiaryOrganisation.HumanReadableName));
			}

			var existingParentParty = Factory.Load<OrgManagementRelatedParty>(OrgManagementRelatedParty.GetRelatedManagementParentsQuery(subsidiaryOrganisation)).Where(x => x.PR_OH_RelatedParty != Organisation.PK).Select(x => x.RelatedParty).FirstOrDefault();
			if (existingParentParty != null)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("982DB169-75FD-4F22-A6C1-28A83FD31578", "{0} is already the parent of {1}. {1} can only have one parent.", existingParentParty.HumanReadableName, subsidiaryOrganisation.HumanReadableName));
			}

			var relationshipSequence = HierarchicalSequenceBuilder.GetRelationshipSequence(x => x.RelatedManagementParentRelations.Organisations, Organisation, subsidiaryOrganisation, Tuple.Create(Organisation, subsidiaryOrganisation));
			if (relationshipSequence.Any())
			{
				return new RelationValidationResult(false,
					ResString.GetMultilingualString("BA623129-E465-4B45-9EB2-73679AAD453B", @"{1} is already a related ancestor of {0}.
Making {1} a subsidiary of {0} would create an illegal cycle.

Conflicting relationship sequence: {2}",
					Organisation.HumanReadableName,
					subsidiaryOrganisation.HumanReadableName,
					string.Join(" > ", new Stack<OrgHeader>(relationshipSequence).Select(organisation => organisation.HumanReadableName))));
			}

			return new RelationValidationResult(true);
		}

		#endregion

		#region Add Organisation

		public sealed override RelationUpdateResult AddOrganisation(OrgHeader subsidiaryOrganisation)
		{
			var existingRelation = this.FirstOrDefault(x => x.PR_OH_Parent == subsidiaryOrganisation.PK);
			if (existingRelation != null)
			{
				return new RelationUpdateResult(false, ResString.GetMultilingualString("A446A48C-A134-4B42-B26B-CE49BD43E347", "{0} is already a subsidiary of {1}.", subsidiaryOrganisation.HumanReadableName, Organisation.HumanReadableName));
			}

			var validationResult = CheckIsValidOrganisation(subsidiaryOrganisation, true);
			if (!validationResult.IsValid)
			{
				return new RelationUpdateResult(false, validationResult.Reason);
			}

			var backwardsValidationResult = subsidiaryOrganisation.RelatedManagementParentRelations.CheckIsValidOrganisation(Organisation, false);
			if (!backwardsValidationResult.IsValid)
			{
				return new RelationUpdateResult(false, backwardsValidationResult.Reason);
			}

			AddOrganisationWithoutCheckingValid(subsidiaryOrganisation);
			return new RelationUpdateResult(true);
		}

		public override void AddOrganisationWithoutCheckingValid(OrgHeader organisation)
		{
			var relation = AddNew();
			relation.SubsidiaryOrganisation = organisation;
		}

		#endregion

		#region Remove Organisation

		public sealed override RelationUpdateResult RemoveOrganisation(OrgHeader subsidiaryOrganisation)
		{
			var relations = this.Where(x => x.PR_OH_Parent == subsidiaryOrganisation.PK).ToArray();
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
				return new RelationUpdateResult(false, ResString.GetMultilingualString("210B6244-1F17-4035-AFDF-0983C99DE845", "{0} is not a subsidiary of {1}.", subsidiaryOrganisation.HumanReadableName, Organisation.HumanReadableName));
			}
		}

		#endregion

		#region Implementation

		protected override void SetDefaultsForNewElementCore(OrgManagementRelatedParty newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ParentOrganisation = Organisation;
		}

		#endregion
	}
}
