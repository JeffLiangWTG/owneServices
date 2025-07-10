using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class StmTemplateModule : ZFilterGridModule
	{
		public StmTemplateModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DocumentTemplate; }
		}
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.DocumentTemplate);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new StmTemplateFilterControl(GridCollection, (StmTemplateFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmTemplateCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new StmTemplateFilterBusinessObject();
		}

		public override ZBool HasActions
		{
			get { return false; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.QuotationDocuments; }
		}
	}
}
