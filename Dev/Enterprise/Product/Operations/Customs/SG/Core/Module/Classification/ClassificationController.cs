using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.SG.V4.Module
{
	/// <summary>
	/// Module Controller for Tariff.
	/// </summary>
	public class ClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.SG.SG4Classification; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Classification); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ClassificationForm((Classification)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsClassificationView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CustomsClassificationNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsClassificationEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CustomsClassificationDelete; }
		}
	}
}
