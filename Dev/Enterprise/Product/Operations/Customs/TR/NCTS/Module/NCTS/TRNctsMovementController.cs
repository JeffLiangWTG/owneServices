using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.TR.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.Module
{
	public class TRNctsMovementController : EU.NCTS.Module.NctsMovementController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var nctsMovement = (NctsHeader)businessEntity;
			nctsMovement.SetMovementType(HeaderType);
			return nctsMovement.IsPhase5Departure ? base.GetForm(businessEntity) : new TRNctsMovementForm((Business.NctsHeader)nctsMovement);
		}

		protected override IZForm GetPhase5DepartureForm(NctsHeader nctsHeader) => new Phase5DepartureMovementForm(nctsHeader);

		public override Type TypeOfTopLevelBusinessObject => typeof(NctsHeader);
	}
}
