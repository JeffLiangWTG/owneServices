using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(NewOrAttachConsolidatedDeclarationForm))]
	sealed class NewConsolidatedDeclarationSelectionFormTest : ZFormBasherTest
	{
		public void TestCreateAndAttachDeclarations()
		{
			using (var form = GetNewOrAttachConsolidatedDeclarationForm(false))
			{
				var jobDeclaration = new BusinessObjectFactory().New<BaseJobDeclaration>();
				jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				jobDeclaration.ActiveEntryHeaders.AddNew();
				jobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				jobDeclaration.Factory.Save();
				form.Show();
				jobDeclarationModule.PerformSearch_ForTest();
				jobDeclarationModule.DisplayGrid.SelectAllElements();
				form.FindSingle<ZButton>("CreateButton").PerformClick();

				var openedForm = Application.OpenForms.OfType<ConsolidatedDeclarationForm>().Single();
				AssertNotNull("Consolidated Declaration form should be opened", openedForm);
				openedForm.Close();
			}

			AssertEquals("Consolidated declaration has been saved", true, consolidatedDeclaration.IsInDatabase);
			AssertEquals("Consolidated declaration should has 1 declaration", 1, consolidatedDeclaration.JobDeclarations.Count);

			using (var form = GetNewOrAttachConsolidatedDeclarationForm(true))
			{
				var jobDeclaration = new BusinessObjectFactory().New<BaseJobDeclaration>();
				jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				jobDeclaration.ActiveEntryHeaders.AddNew();
				jobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				jobDeclaration.Factory.Save();
				form.Show();
				jobDeclarationModule.PerformSearch_ForTest();
				jobDeclarationModule.DisplayGrid.SelectAllElements();
				form.FindSingle<ZButton>("CreateButton").PerformClick();
			}

			AssertEquals("Consolidated declaration should has 2 declarations", 2, consolidatedDeclaration.JobDeclarations.Count);
		}

		public void TestCreateButton_Caption()
		{
			CombineAssertions(() =>
			{
				using (var form = GetNewOrAttachConsolidatedDeclarationForm(false))
				{
					AssertEquals("New caption", "Create", form.FindSingle<ZButton>("CreateButton").CaptionResourceString.Caption);
				}

				using (var form = GetNewOrAttachConsolidatedDeclarationForm(true))
				{
					AssertEquals("Attach caption", "Attach", form.FindSingle<ZButton>("CreateButton").CaptionResourceString.Caption);
				}
			});
		}

		public void TestLayout()
		{
			using (var form = GetFormToBash() as NewOrAttachConsolidatedDeclarationForm)
			{
				form.Show();
				AssertEquals("FilterGrid is shown", true, form.FindSingle<ZFilterGrid>().Visible);
			}
		}

		public void TestFormCaption()
		{
			using (var form = GetFormToBash() as NewOrAttachConsolidatedDeclarationForm)
			{
				AssertEquals($"Consolidated Entry {consolidatedDeclaration.CRD_JobReferenceNumber}", form.FormCaption);
			}
		}

		public void TestCreate()
		{
			using (var form = GetFormToBash() as NewOrAttachConsolidatedDeclarationForm)
			{
				var jobDeclaration = new BusinessObjectFactory().New(jobDeclarationModule.TypeOfTopLevelBusinessObject) as BaseJobDeclaration;
				jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				jobDeclaration.ActiveEntryHeaders.AddNew();
				jobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				var jobDeclaration2 = jobDeclaration.Factory.New(jobDeclarationModule.TypeOfTopLevelBusinessObject) as BaseJobDeclaration;
				jobDeclaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
				jobDeclaration2.ActiveEntryHeaders.AddNew();
				jobDeclaration2.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				jobDeclaration.Factory.Save();
				form.Show();
				jobDeclarationModule.PerformSearch_ForTest();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				jobDeclarationModule.DisplayGrid.SelectAllElements();
				form.FindSingle<ZButton>("CreateButton").PerformClick();
				AssertNotNullOrEmpty("There should be message showing errors.", UnitTestUserNotification.Instance.LastMessage.Text);
				consolidatedDeclaration.ClearRowNotifications();
				AssertContainsExactElementsInAnyOrder("Created JobDeclaration should be available for selection", new[] { jobDeclaration.PK, jobDeclaration2.PK }, jobDeclarationModule.DisplayGrid.SelectedElements.Select(dec => dec.PK));
				jobDeclarationModule.DisplayGrid.UnSelect(1);
				form.FindSingle<ZButton>("CreateButton").PerformClick();
				AssertEquals("Job declaration can be saved to consolidated declaration", jobDeclaration.PK, consolidatedDeclaration.CRD_JE_LeadDeclaration);
				AssertEquals("Form is closed after successful saving", true, form.IsDisposed);

				var openedForm = Application.OpenForms.OfType<ConsolidatedDeclarationForm>().Single();
				AssertNotNull("Consolidated Declaration form should be opened", openedForm);
				AssertEquals("Form should linked with the new created consolidated declaration", consolidatedDeclaration.PK, (openedForm.BusinessEntity as BusinessObject).PK);
				openedForm.Close();
			}
		}

		[ExpectNoExceptions]
		public void TestCreatingConsolidatedDeclarationCallsOnCreatedWithCustomsDeclarations()
		{
			CombineAssertions(() =>
			{
				var jobDeclaration = new BusinessObjectFactory().New<BaseJobDeclaration>();
				jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				jobDeclaration.ActiveEntryHeaders.AddNew();
				jobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				jobDeclaration.Factory.Save();
				jobDeclarationModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration) as ZFilterGridModule;
				jobDeclarationModule.DoNotShowRecentItems = true;
				jobDeclarationModule.FilterBusinessObject.ParentModuleID = ModuleIDs.Customs.ConsolidatedDeclaration;
				var mockConsolidatedDeclaration = Factory.NewMoq<ConsolidatedDeclaration>();
				consolidatedDeclaration = mockConsolidatedDeclaration.Object;

				using (var form = new NewOrAttachConsolidatedDeclarationForm(consolidatedDeclaration, jobDeclarationModule, false))
				{
					form.Show();
					jobDeclarationModule.PerformSearch_ForTest();
					jobDeclarationModule.DisplayGrid.SelectAllElements();
					form.FindSingle<ZButton>("CreateButton").PerformClick();
					mockConsolidatedDeclaration.Verify(m => m.OnCreatedWithCustomsDeclarations(), Times.Once);
				}

				mockConsolidatedDeclaration.Invocations.Clear();

				using (var form = new NewOrAttachConsolidatedDeclarationForm(consolidatedDeclaration, jobDeclarationModule, true))
				{
					form.Show();
					jobDeclarationModule.PerformSearch_ForTest();
					jobDeclarationModule.DisplayGrid.SelectAllElements();
					form.FindSingle<ZButton>("CreateButton").PerformClick();
					mockConsolidatedDeclaration.Verify(m => m.OnCreatedWithCustomsDeclarations(), Times.Never);
				}
			});
		}

		public void TestClose()
		{
			using (var form = GetFormToBash() as NewOrAttachConsolidatedDeclarationForm)
			{
				form.Show();
				form.FindSingle<ZButton>(btn => btn.CaptionResourceString.Caption == "Close").PerformClick();
				AssertEquals(true, form.IsDisposed);
			}
		}

		public void TestHandlesSaveException()
		{
			using (var form = GetFormToBash() as NewOrAttachConsolidatedDeclarationForm)
			{
				var jobDeclaration = new BusinessObjectFactory().New(jobDeclarationModule.TypeOfTopLevelBusinessObject) as BaseJobDeclaration;
				jobDeclaration.ActiveEntryHeaders.AddNew();
				jobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				jobDeclaration.Factory.Save();
				consolidatedDeclaration.Factory.Saving += _ =>
				{
					Db.Connection.ExecuteNonQuery("trigger a sqlexception");
				};
				form.Show();
				jobDeclarationModule.PerformSearch_ForTest();
				jobDeclarationModule.DisplayGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FindSingle<ZButton>("CreateButton").PerformClick();
				AssertNotNullOrEmpty("There should be message showing errors.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore() => GetNewOrAttachConsolidatedDeclarationForm(false);

		protected override void SetUp()
		{
			base.SetUp();
			using (var consolidatedDeclarationModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.ConsolidatedDeclaration) as ZFilterGridModule)
			{
				consolidatedDeclaration = Factory.New(consolidatedDeclarationModule.TypeOfTopLevelBusinessObject) as ConsolidatedDeclaration;
			}
		}

		NewOrAttachConsolidatedDeclarationForm GetNewOrAttachConsolidatedDeclarationForm(bool attachDeclaration)
		{
			jobDeclarationModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration) as ZFilterGridModule;
			jobDeclarationModule.DoNotShowRecentItems = true;
			jobDeclarationModule.FilterBusinessObject.ParentModuleID = ModuleIDs.Customs.ConsolidatedDeclaration;
			return new NewOrAttachConsolidatedDeclarationForm(consolidatedDeclaration, jobDeclarationModule, attachDeclaration);
		}

		protected override string CountryCode => Constants.CountryCodes.NewZealand;

		ZFilterGridModule jobDeclarationModule;
		ConsolidatedDeclaration consolidatedDeclaration;
	}
}
