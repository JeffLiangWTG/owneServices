using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class ConsolidatedDeclarationControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			using (var myForm = Controller.ShowNewForm() as NewOrAttachConsolidatedDeclarationForm)
			{
				AssertNotEquals("New form should be of type NewConsolidatedDeclarationForm", null, myForm);
				AssertEquals("'Recent Items' is hidden", null, myForm.FindSingleOrDefault<ZPanel>("RecentItemsPanel"));
				var filterControl = myForm.FindSingle<JobDeclarationFilterStripControl>();
				using (var jobDeclarationModuleForm = ZModule.GetZModule(ModuleIDs.Customs.JobDeclaration).ShowPopup() as ZForm)
				{
					var standardFilterStripControl = jobDeclarationModuleForm.FindSingle<JobDeclarationFilterStripControl>();
					AssertEquals("Using standard Customs Declaration Module", filterControl.GetType(), standardFilterStripControl.GetType());
					AssertEquals("Using standard Customs Declaration Filters", filterControl.FilterBusinessObject.GetType(), standardFilterStripControl.FilterBusinessObject.GetType());
				}
				AssertEquals("FilterBusinesObject should have Consolidated Declaration parent module", Controller.ModuleID, filterControl.FilterBusinessObject.ParentModuleID);

				var filterBO = filterControl.FilterBusinessObject as JobDeclarationFilterBusinessObject;
				var entryStatusFilter = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
				AssertEquals("Parent Module ID set", ModuleIDs.Customs.ConsolidatedDeclaration, filterBO.ParentModuleID);
				AssertEquals("RFC is the filtered status for declarations ready for consolidation", ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, entryStatusFilter.Property);
				AssertEquals("RFC is always effective", FilterVisibility.AlwaysAppliedAndHidden, entryStatusFilter.Visibility);
			}
		}

		public void TestIsFormShownForBusinessObject()
		{
			using (var myForm = Controller.ShowNewForm() as NewOrAttachConsolidatedDeclarationForm)
			{
				var entity = myForm.BusinessEntity as ConsolidatedDeclaration;
				myForm.Show();

				AssertEquals("Controller is supposed to show NewConsolidatedDeclarationForm, therefore IsFormShownFor should return true", true, Controller.IsFormShownFor(entity));

				var declaration = entity.Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.ActiveEntryHeaders.AddNew();
				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				entity.JobDeclarations.Add(declaration);
				entity.Factory.Save();
				AssertEquals("Controller is supposed to show ConsolidatedDeclarationForm, therefore IsFormShownFor should return false", false, Controller.IsFormShownFor(entity));
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.ConsolidatedDeclaration;
	}
}
