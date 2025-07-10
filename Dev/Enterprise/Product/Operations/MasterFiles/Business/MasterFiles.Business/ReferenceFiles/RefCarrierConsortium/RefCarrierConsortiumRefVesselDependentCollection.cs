using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefVessel)]
	public class RefCarrierConsortiumRefVesselDependentCollection : DependentBusinessObjectCollection<RefVessel, RefCarrierConsortium>
	{
		public RefCarrierConsortiumRefVesselDependentCollection(RefCarrierConsortium parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return RefVesselSchema.RV_RG; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
