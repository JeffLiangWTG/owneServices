using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	public class ShipmentGatePassModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ShipmentGatePass; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.GatePass }; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CFSGatePass; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.CFSManager; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ShipmentGatePass);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ShipmentGatePassFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new GatePassShipmentList(Factory);
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			SetGuiProviders(factory);
			return factory;
		}

		void SetGuiProviders(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				ShipmentDocumentSupporterGuiQueryProvider.Register(factory);
				ServicesSelectionGuiProvider.Register(factory);
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ShipmentGatePassFilterBusinessObject();
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		protected override bool CanBeCopied()
		{
			return false;
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return JobInvoicingConsumerTypes.CFSShipment.Code; }
		}
	}
}
