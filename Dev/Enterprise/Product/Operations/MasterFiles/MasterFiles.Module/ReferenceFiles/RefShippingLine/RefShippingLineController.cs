using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefShippingLineController : ZController
	{
		public override ControllerID ID => ControllerIDs.RefShippingLine;

		public override ModuleIdentifier ModuleID => ModuleIDs.RefShippingLine;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefShippingLine);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.RefShippingLineView;

		protected override SecurityCheckpoint CheckPointForNew => new RefShippingLineNewSecurityCheckpoint();

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.RefShippingLineEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.RefShippingLineDelete;

		protected override IZForm GetForm(IBusiness businessEntity) => new RefShippingLineForm((RefShippingLine)businessEntity);
	}
}
