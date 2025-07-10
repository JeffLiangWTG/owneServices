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
	internal class ExternalRequestInfoTemplateModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ExternalRequestInfoTemplate; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ExternalRequestInfoTemplate);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ExternalRequestInfoTemplateFilterControl(GridCollection, (ExternalRequestInfoTemplateFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ExternalRequestInfoTemplateCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ExternalRequestInfoTemplateFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Workflow;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ExternalRequestInfoTemplate;
	}
}
