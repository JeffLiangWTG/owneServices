using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.Module
{
	public class CusClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusClassification); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusClassificationForm((CusClassification)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CusClassificationModify; }
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CusClassificationModify; }
		}
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CusClassificationModify; }
		}
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CusClassificationDelete; }
		}
	}
}
