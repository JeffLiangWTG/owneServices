using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkMovementsChildLookups : ZLookups
	{
		public BulkMovementsChildLookups(BulkMovementsChild parent)
			: base(parent) { }

		public ContainerMovementTypes MovementCodeList
		{
			get { return movementCodeList ?? (movementCodeList = Factory.GetCachedValue<ContainerMovementTypes>()); }
		}
		ContainerMovementTypes movementCodeList;

		public CodeDescriptionPairList OwnerTypeList
		{
			get { return RefContainerStockLookups.GetOwnerTypes(Factory); }
		}

		public ReadOnlyCodeDescriptionPairList DamageCodeList
		{
			get { return AgencyRegistry.Instance.ContainerDamageCodes.GetFactoryCachedValue(Factory); }
		}

		public ReadOnlyCodeDescriptionPairList CleanCodeList
		{
			get { return AgencyRegistry.Instance.ContainerCleanCodes.GetFactoryCachedValue(Factory); }
		}

		public OrganisationsFindBoxCollection DepotOrgList
		{
			get { return ContainerMovementTypes.GetDepotLookupCollection(Factory, Parent.MovementType); }
		}

		public RefContainerCollection ContainerTypeList
		{
			get { return new RefContainerCollection(Factory, Constants.TransportModes.Sea); }
		}

		public OrganisationsFindBoxCollection Principals
		{
			get { return new ShipsAgencyPrincipalCollection(Factory); }
		}

		public OrganisationsFindBoxCollection ResponsibleParties
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#region Implementation

		protected new BulkMovementsChild Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkMovementsChild)base.Parent; }
		}

		#endregion
	}
}
