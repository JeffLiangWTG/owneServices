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
	public class OrgDebtorGroupModule : ZFilterGridModule
	{
		public OrgDebtorGroupModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.OrgDebtorGroup; }
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.OrgDebtorGroup);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgDebtorGroupFilterControl(GridCollection, (OrgDebtorGroupFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgDebtorGroupCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgDebtorGroupFilterBusinessObject();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DebtorGroups; }
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
