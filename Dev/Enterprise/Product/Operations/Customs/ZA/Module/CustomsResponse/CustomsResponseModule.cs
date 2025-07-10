using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class CustomsResponseModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ZAModuleIDs.CustomsResponse; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZACustomsResponse); }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ZAControllerIDs.CustomsResponse);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CustomsResponseFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CustomsResponseFilterControl(GridCollection, (CustomsResponseFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CustomsResponseCollection(Factory);
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}
	}
}
