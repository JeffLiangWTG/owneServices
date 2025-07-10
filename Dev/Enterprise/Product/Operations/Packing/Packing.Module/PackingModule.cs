using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Packing.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.Module
{
	public class PackingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Packing; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Packing);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new PackingFilterControl(GridCollection, (PackingFilterStripBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PackingFilterStripBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new PkgPackageJobCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Packing; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Packing; }
		}
	}
}
