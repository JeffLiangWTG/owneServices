using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class DrawbackController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.US.Drawback;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.Drawback;

		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override IZForm GetForm(IBusiness businessEntity) => new JobDeclarationForm((JobDeclaration)businessEntity);

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.USDrawbackEdit;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.USDrawbackEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.USDrawbackNew;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.USDrawbackView;

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = Factory.New<JobDeclaration>();
			result.SetDefaultValuesForDrawback();
			return result;
		}
	}
}
