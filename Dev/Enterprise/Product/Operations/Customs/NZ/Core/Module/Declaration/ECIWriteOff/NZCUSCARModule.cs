using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff
{
	public class NZCUSCARModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.NZ.CUSCAR;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CUSCARFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new Customs.Module.JobDeclarationFilterStripControl(null, GridCollection, FilterBusinessObject);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var jobDeclaration = selectedBusinessObject as JobDeclaration;
			if (jobDeclaration == null || jobDeclaration.Shipment == null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.NZ.CUSCAR);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.NZ.CUSCARPluggedIntoShipment);
			}
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollectionECIWriteOff(Factory, GlbCompany.CurrentCompany.PK);

		protected new JobDeclaration CurrentBusinessObjectInGrid => (JobDeclaration)base.CurrentBusinessObjectInGrid;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.NZCustomsECIWriteoff;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => JobInvoicingConsumerTypes.Brokerage.Code;
	}
}
