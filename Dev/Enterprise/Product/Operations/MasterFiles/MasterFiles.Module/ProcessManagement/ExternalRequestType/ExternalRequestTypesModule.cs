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
	internal class ExternalRequestTypesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ExternalRequestTypes; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ExternalRequestTypes);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ExternalRequestTypesFilterControl(GridCollection, (ExternalRequestTypesFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ExternalRequestTypeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ExternalRequestTypesFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Workflow;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ExternalRequestTypes;
	}
}
