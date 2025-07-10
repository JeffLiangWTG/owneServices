using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	[ModuleID(ModuleId.AgencyContainerMove)]
	public class ContainerMovementCollection : ActiveBusinessObjectCollection<ContainerMovement>
	{
		public ContainerMovementCollection(BusinessObjectFactory factory, bool allowNew)
			: base(factory)
		{
			this.allowNew = allowNew;
		}

		public ContainerMovementCollection(BusinessObjectFactory factory, bool allowNew, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
			this.allowNew = allowNew;
		}

		protected override bool AllowNew
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return allowNew; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly bool allowNew;
	}
}


