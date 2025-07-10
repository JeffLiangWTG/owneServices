using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingConsolidationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DtbBookingConsolidation; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.DtbConsolidation }; }
		}

		public new DtbBookingConsolidationFilterBusinessObject FilterBusinessObject
		{
			get { return (DtbBookingConsolidationFilterBusinessObject)base.FilterBusinessObject; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DtbBookingConsolidationFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DtbBookingConsolidationFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetupFactory(Factory);
			return new DtbBookingMultiJobConsolidationCollection(Factory);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.DtbBookingConsolidation);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.TransportBookings; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DtbBookingConsolidation; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode; }
		}

		void SetupFactory(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				DtbFormStateService.SetState(factory, DtbFormState.Booking);
			}
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			SetupFactory(factory);
			return factory;
		}
	}
}
