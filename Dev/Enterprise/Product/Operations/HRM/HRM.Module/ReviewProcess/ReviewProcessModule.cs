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
	public class ReviewProcessModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ReviewProcess;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			=> ZControllerFactory.Create(ControllerIDs.ReviewProcess);

		protected override FilterBusinessObject GetNewFilterBusinessObject()
			=> new ReviewProcessFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl()
			=> new ReviewProcessFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection()
			=> new ReviewProcessCollection(new BusinessObjectFactory() { NameForDebugging = nameof(ReviewProcessModule) });

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
	}
}
