using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class EventsModule : ZFilterGridModule
	{
		#region ZFilterGridModule

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Events; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Events; }
		}

		public override bool AllowNew { get { return false; } }

		public override bool AllowDelete { get { return false; } }

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Events);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EventsFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EventsFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmEventCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Workflow; }
		}

		#endregion
	}
}
