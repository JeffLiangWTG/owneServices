using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class SingleTariffClassificationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.SingleTariffClassification; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CusClassification; }
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CusClassification; }
		}

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CusClassificationNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CusClassificationDelete;

		public override System.Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BaseCusClassification); }
		}

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return new BaseClassificationForm((BaseCusClassification)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SingleTariffClassification; }
		}
	}
}
