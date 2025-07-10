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
	public class UNDGCommonDataModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.UNDGCommonData; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.UNDGCommonData);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new UNDGCommonDataFilterControl(GridCollection, (UNDGCommonDataFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new UNDGCommonDataCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UNDGCommonDataFilterBusinessObject();
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region Licence/Security

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.UNDGCommonData; }
		}

		#endregion
	}
}
