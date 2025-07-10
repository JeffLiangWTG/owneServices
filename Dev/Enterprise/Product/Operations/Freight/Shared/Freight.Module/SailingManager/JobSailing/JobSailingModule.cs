using System;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public abstract class JobSailingModule : ZFilterGridModule, IJobSailingSchedule
	{
		public override ModuleIdentifier ID => ModuleIDs.JobSailing;

		protected abstract ControllerID ControllerID { get; }

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.SailingSchedule;

		public bool ScheduleCreateFromJob { get; set; }

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var controller = ZControllerFactory.Create(ControllerID);
			if (controller is IJobSailingSchedule jobSailingSchedule)
			{
				jobSailingSchedule.ScheduleCreateFromJob = ScheduleCreateFromJob;
			}
			return controller;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BaseJobSailingCollection(Factory);
		}

		protected override IZForm ShowTemplateCopyForm(BusinessObject selectedBusinessObject)
		{
			return base.ShowTemplateCopyForm(((BaseJobSailing)selectedBusinessObject).Voyage);
		}

		protected override bool CanBeCopied()
		{
			return true;
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(Res.GetString("Freight|JobSailingModule|ImportFromXMLMenuItem", "From &XML"), new EventHandler(OnImportFromXml_Click));
		}

		void OnImportFromXml_Click(object sender, EventArgs e)
		{
			NewDataTransferDirector(new ScheduleValueObjectDataAdapter()).PromptUserAndImport(BillingInterfaceName.SailingXmlImport);
		}

		protected virtual XmlDataTransferDirector NewDataTransferDirector(ScheduleValueObjectDataAdapter dataAdapter)
		{
			return new XmlDataTransferDirector(dataAdapter, true);
		}

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.SailingScheduleWorkflowDescriptorCode;
	}
}
