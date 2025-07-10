using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.Module
{
	public class CusClassificationModule : Customs.Module.SingleTariffClassificationModule
	{
		public CusClassificationModule()
		{
			AddImportFromCSVDataMenuItem(new CreateFormHandler(CreateImportFromCSVForm));
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusClassificationFilterControl(GridCollection, (CusClassificationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new Customs.Business.BaseClassificationCollection<CusClassification>(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusClassificationFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CusClassification; }
		}

		KForm CreateImportFromCSVForm()
		{
			return new NZClassificationImportFromCSVForm();
		}
	}
}
