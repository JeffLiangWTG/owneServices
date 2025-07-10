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
	class AccInvMsgModule : ZFilterGridModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccInvMsg);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccInvMsgFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccInvMsgFilterControl(GridCollection, (AccInvMsgFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccInvMsgCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccInvMsg; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.InvoiceMessages; }
		}
	}
}
