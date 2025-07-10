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
	public class RefVesselZZModule : ZFilterGridModule
	{
		public RefVesselZZModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefVesselZZ; }
		}

		#endregion

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowNew => false;

		public override bool AllowView => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefVesselZZ);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefVesselZZFilterControl(GridCollection, (RefVesselZZFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefVesselZZCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefVesselZZFilterBusinessObject();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Vessels; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
