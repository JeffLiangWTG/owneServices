using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruitment.Module.CandidateManagement
{
	public class CandidateManagementPopupFormController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.CandidateManagement; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RecruitmentCandidateManagement; }
		}

		public override bool SupportsHyperlinking
		{
			get
			{
				return false;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return null; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.Recruitment; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var collection = new CandidateBusinessObjectCollection(Factory);
			return new CandidateModuleBusinessObject(collection);
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.Browse;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var form = new CandidateManagementZChildForm((CandidateModuleBusinessObject)businessEntity);
			form.CaptionResourceString = Res.GetData("C74DBED9-284C-4702-AFA5-8493B6BE4439", "Candidate Management");
			form.CaptionRenderingEnabled = true;
			form.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 725, true);
			var control = new CandidateManagementControl(Factory, (CandidateModuleBusinessObject)businessEntity);
			control.Dock = System.Windows.Forms.DockStyle.Fill;
			form.Controls.Add(control);
			return form;
		}

#if DEBUG
		internal IZForm GetFormForTest(IBusiness businessEntity) => GetForm(businessEntity);
#endif
	}
}
