using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class HROnBoardingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.HROnBoarding;

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Staff;

		#endregion

		#region License

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Recruiter;

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.HROnBoarding);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new HROnBoardingFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new HROnBoardingFilterControl(GridCollection, (HROnBoardingFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new HROnBoardingCollection(Factory);

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		#region Workflow

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode;

		#endregion
	}
}
