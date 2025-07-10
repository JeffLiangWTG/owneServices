using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefUNLOCOModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public RefUNLOCOModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefUNLOCO; }
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefUNLOCO);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefUNLOCOFilterControl(GridCollection, (RefUNLOCOFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefUNLOCOCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefUNLOCOFilterBusinessObject();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.UNLOCO; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get
			{
				return new UNLOCOOperationalActionSupporter();
			}
		}

		#endregion
	}
}
