using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class GoodsCatalogModule : ZFilterGridModule
	{
		public sealed override ModuleIdentifier ID => ModuleIDs.Customs.GoodsCatalog;

		public sealed override SecurityCheckpoint SecurityCheckpoint => Env.Security.GoodsCatalog;

		protected sealed override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected sealed override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.GoodsCatalog);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GoodsCatalogFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GoodsCatalogFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>(Factory, GlbCompany.CurrentCompany.PK);

		public override string WorkflowType => WorkflowDescriptors.CusGoodsCatalogWorkflowDescriptorCode;
	}
}
