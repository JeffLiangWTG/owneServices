using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Module;
using Enterprise.Customs.US.DIS.Business;
using Enterprise.Customs.US.DIS.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.DIS.Module
{
	public class DISController : DISControllerBase
	{
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsDISEdit; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsDISView; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var disHost = (MasterFiles.Business.DIS.IUSDISHost)businessEntity;
			return new DISForm(new DISHostWrapper(disHost));
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DISHostWrapper); }
		}
	}
}
