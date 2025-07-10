using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgManagementRelatedCollection : ActiveBusinessObjectCollection<OrgManagementRelatedParty>
	{
		protected OrgManagementRelatedCollection(OrgHeader organisation, ZQuery filter)
			: base(organisation.Factory, filter)
		{
			this.Organisation = organisation;
		}

		public readonly OrgHeader Organisation;

		public abstract IEnumerable<OrgHeader> Organisations { get; }
		public abstract RelationValidationResult CheckIsValidOrganisation(OrgHeader organisation, bool organisationMustBeInDatabase);
		public abstract RelationUpdateResult AddOrganisation(OrgHeader organisation);
		public abstract void AddOrganisationWithoutCheckingValid(OrgHeader organisation);
		public abstract RelationUpdateResult RemoveOrganisation(OrgHeader organisation);

		public interface IOrgManagementRelatedCollectionSnapshot
		{
			void Restore();
		}
	}
}
