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
	public class AccChargeCodeModule : ZFilterGridModule
	{
		public AccChargeCodeModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccChargeCode; }
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccChargeCode);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccChargeCodeFilterControl(GridCollection, (AccChargeCodeFilterBusinessObject)FilterBusinessObject, AccChargeCodeFilterControl.Mode.Normal);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccChargeCodeCollection(Factory, new ZQuery());
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccChargeCodeFilterBusinessObject();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ChargeCodes; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
