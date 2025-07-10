using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class SingleTariffClassificationModule : ZFilterGridModule
	{
		public SingleTariffClassificationModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.SingleTariffClassification; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.SingleTariffClassification);
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CusClassification; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BaseClassificationCollection<BaseCusClassification>(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusClassificationFilterControl(
					GridCollection, (CusClassificationFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusClassificationFilterBusinessObject();
		}
	}
}
