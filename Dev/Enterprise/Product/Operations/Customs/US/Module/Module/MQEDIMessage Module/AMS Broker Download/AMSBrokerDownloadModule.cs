using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class AMSBrokerDownloadModule : MQEDIMessageModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.AMSBrokerDownloadMessage;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.AMSBrokerDownloadMessages;

		public override bool AllowEdit => false;

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.US.AMSBrokerDownloadMessages);

		protected override IBusinessObjectCollection GetNewGridCollection() => new AMSBrokerDownloadMQEDIMessageCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new AMSBrokerDownloadFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new AMSBrokerDownloadMessageFilterControl((AMSBrokerDownloadMQEDIMessageCollection)GridCollection, (AMSBrokerDownloadFilterStripBusinessObject)FilterBusinessObject);

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
	}
}
