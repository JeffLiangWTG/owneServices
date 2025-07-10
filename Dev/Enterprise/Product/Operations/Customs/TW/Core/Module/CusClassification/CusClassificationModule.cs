
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Module
{
	public class CusClassificationModule : Customs.Module.SingleTariffClassificationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusClassificationFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusClassificationFilterControl(GridCollection, (CusClassificationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Taiwan);
		}
	}
}
