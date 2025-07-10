using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Recruiter.Module
{
	public class ExamSettingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ExamSetting;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ExamSetting;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.LearningAndDevelopment;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ExamSetting);
		}

		protected override IModuleDecisionProvider GetModuleDecisionProviderForFindBoxCore(IFindBox findbox)
		{
			return new EmptyModuleDecisionProvider(findbox);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ExamSettingCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ExamSettingFilterControl(GridCollection,
				(ExamSettingsFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ExamSettingsFilterBusinessObject();
		}

		protected override SortInfo DefaultSortOrder
		{
			get { return null; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			MenuItem[] menuItems = base.GetNewStandardMenuItems();

			MenuItem newForExisting = new ZMenuItem(ResString.GetMultilingualString("908E4AA6-3FE2-469C-BBD2-B1B424F7F4C3", "New For Existing Exam"), ShowEditExamForm);
			MenuItem newForNew = new ZMenuItem(ResString.GetMultilingualString("819FE0A6-76A5-4246-9B82-10E5911429EB", "New For New Exam"), ShowNewExamForm);

			NewMenuItem.MenuItems.Add(newForExisting);
			NewMenuItem.MenuItems.Add(newForNew);

			return menuItems;
		}

		protected override void HandleEditClickCore(object sender, EventArgs e)
		{
			IEnumerable<LearningCentreCampaign> exams = null;
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				exams = SelectedBusinessObjects.Cast<ExamSetting>().Select(s => s.TestCampaign);
			}
			else if (CurrentBusinessObjectInGrid != null)
			{
				exams = new[] { (CurrentBusinessObjectInGrid as ExamSetting).TestCampaign };
			}
			else
			{
				ShowNoSelectedMessage();
			}

			if (exams != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.LearningCentreCampaign);
				foreach (var exam in exams)
				{
					controller.ShowEditForm(exam);
				}
			}
		}

		void ShowEditExamForm(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.LearningCentreCampaign);
			if (controller != null)
			{
				var module = (LearningCentreCampaignModule)ZModuleFactory.Instance.Create(ModuleIDs.LearningCentreCampaign);
				if (module != null)
				{
					var popup = new EmbeddedModulePopup(module);
					if (ZFormModaliser.ShowDialogAndDispose(popup) == DialogResult.OK)
					{
						popup.IsSelectionMandatory = true;
						popup.Selected += Popup_Selected;
					}
				}
			}
		}

		void Popup_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.LearningCentreCampaign);
			foreach (LearningCentreCampaign item in e.SelectedBusinessObjects)
			{
				controller.ShowEditForm(item);
			}
		}

		void ShowNewExamForm(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.LearningCentreCampaign);
			controller.ShowNewForm();
		}

		public class EmptyModuleDecisionProvider : ModuleDecisionProvider
		{
			public EmptyModuleDecisionProvider(IFindBox findBox)
				: base(findBox, null)
			{
			}

			public override bool EnablePreviousNextSupport
			{
				get { return false; }
			}

			public override bool ShouldDisplayNotifications
			{
				get { return true; }
			}
		}
	}
}
