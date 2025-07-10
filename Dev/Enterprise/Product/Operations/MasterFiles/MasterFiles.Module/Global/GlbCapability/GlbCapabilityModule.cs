using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbCapabilityModule : ZFilterGridModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbCapability);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbCapabilityFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbCapabilityFilterControl(GridCollection, (GlbCapabilityFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlbCapabilityCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbCapability; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Capability; }
		}
	}
}
