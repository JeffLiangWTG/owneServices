using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class JobContainerMoveLookups : AutoJobContainerMoveLookups
	{
		public JobContainerMoveLookups(AutoJobContainerMove parent)
			: base(parent) { }

		public ContainerMovementTypes MovementCodeList
		{
			get { return Factory.GetCachedValue<ContainerMovementTypes>(); }
		}

		public ReadOnlyCodeDescriptionPairList DamageCodeList
		{
			get { return AgencyRegistry.Instance.ContainerDamageCodes.GetFactoryCachedValue(Factory); }
		}

		public ReadOnlyCodeDescriptionPairList CleanCodeList
		{
			get { return AgencyRegistry.Instance.ContainerCleanCodes.GetFactoryCachedValue(Factory); }
		}

		public ContainerLocationCategoryList LocationCategoryList
		{
			get { return Factory.GetCachedValue<ContainerLocationCategoryList>(); }
		}

		public OrganisationsFindBoxCollection DepotOrgList
		{
			get { return ContainerMovementTypes.GetDepotLookupCollection(Factory, Parent.E9_MovementType); }
		}

		public ContainerDetentionCollection DetentionList
		{
			get { return new ContainerDetentionCollection(Factory); }
		}

		public RefVesselCollection VesselList
		{
			get { return new RefVesselCollection(Factory); }
		}

		public RefUNLOCOCollection Ports
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public override OrgHeaderCollection Principals
		{
			get { return new ShipsAgencyPrincipalCollection(Factory); }
		}

		#region Implementation

		new ContainerMovement Parent
		{
			get { return (ContainerMovement)base.Parent; }
		}

		#endregion
	}
}
