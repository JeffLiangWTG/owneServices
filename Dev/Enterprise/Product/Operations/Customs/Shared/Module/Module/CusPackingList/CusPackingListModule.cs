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
	public class CusPackingListModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CusPackingList;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsPackingList;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CusPackingList);
		}

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusPackingListFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusPackingListFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusPackingListCollection(Factory);
		}
	}
}
