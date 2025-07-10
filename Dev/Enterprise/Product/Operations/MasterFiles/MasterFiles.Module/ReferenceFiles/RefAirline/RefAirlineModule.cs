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
	/// <summary>
	/// Module for RefAirline.
	/// </summary>
	public class RefAirlineModule : ZFilterGridModule
	{
		public RefAirlineModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefAirline; }
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefAirline);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefAirlineFilterControl(GridCollection, (RefAirlineFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefAirlineCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefAirlineFilterBusinessObject();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.RefAirline; }
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
