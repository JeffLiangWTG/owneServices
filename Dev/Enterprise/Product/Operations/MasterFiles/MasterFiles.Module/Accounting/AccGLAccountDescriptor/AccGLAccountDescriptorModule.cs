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
	public class AccGLAccountDescriptorModule : ZFilterGridModule
	{
		public AccGLAccountDescriptorModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccGLAccountDescriptor; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccGLAccountDescriptor);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccGLAccountDescriptorFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccGLAccountDescriptorCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccGLAccountDescriptorFilterControl(GridCollection, (AccGLAccountDescriptorFilterBusinessObject)FilterBusinessObject);
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GLAccountDescriptor; }
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
