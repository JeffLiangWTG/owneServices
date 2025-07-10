using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterData.Module
{
	public class GenShapeGeographyController : ZController
	{
		public override ControllerID ID => ControllerIDs.GenShapeGeography;

		public override ModuleIdentifier ModuleID => ModuleIDs.GenShapeGeography;

		public override Type TypeOfTopLevelBusinessObject => typeof(GenShapeGeography);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GeographyView;

		protected override SecurityCheckpoint CheckPointForNew => new GenShapeGeographyNewSecurityCheckpoint();

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GeographyModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GeographyDelete;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity) => new GenShapeGeographyForm((GenShapeGeography)businessEntity);
	}
}
