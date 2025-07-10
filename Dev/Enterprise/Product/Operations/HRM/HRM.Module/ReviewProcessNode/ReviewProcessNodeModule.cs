using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.HRM.Common;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.HRM.Module
{
	public class ReviewProcessNodeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ReviewProcessNode;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			=> ZControllerFactory.Create(ControllerIDs.ReviewProcessNode);

		protected override FilterBusinessObject GetNewFilterBusinessObject()
			=> new ReviewProcessNodeFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl()
			=> new ReviewProcessNodeFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection()
			=> new ReviewProcessNodeCollection(new BusinessObjectFactory() { NameForDebugging = nameof(ReviewProcessNodeModule) });
	}
}
