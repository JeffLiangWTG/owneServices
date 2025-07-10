using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgManagementGroupingControlFormForTest))]
	sealed class OrgManagementGroupingControlTest : ZFormBasherTest
	{
		#region Edit Organisation

		[RequiresSTA]
		public void TestShowEditForm_SelectedClientRelationshipTab()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var subOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.RelatedManagementSubsidiaryRelations.AddOrganisation(subOrg);
			Factory.Save();

			using (var form = new OrgManagementGroupingControlFormForTest(org.OrgManagementGroupingModel))
			{
				form.Show();

				using (var editForm = form.OrgManagementGroupingControl.ShowEditFormCore(org.OrgManagementGroupingModel.FindNode(subOrg).BizObjForBinding))
				{
					AssertNotNull("Organisation form", editForm);
					AssertType(typeof(ZOrganisationsForm), editForm);
					var salesTabControl = ((ZOrganisationsForm)editForm).Controls.Find("SalesTabControl", true)[0] as ZTemplateTabControl;
					AssertNotNull("SalesTabControl", salesTabControl);
					AssertEquals(string.Concat(((ZOrganisationsForm)editForm).OrganisationsTabControl.SelectedTab.Name, "+", salesTabControl.SelectedTab.Name), OrganisationTabPages.Sales_ClientRelationship.Name);
				}
			}
		}

		#endregion

		#region Attach Organisation Relation

		public void TestAttachOrganisation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new OrgManagementGroupingControlFormForTest(org.OrgManagementGroupingModel))
			{
				form.Show();

				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;
				form.OrgManagementGroupingControl.AttachToolStripButton.PerformClick();
				using (var modulePopup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull("modulePopup", modulePopup);
					AssertType(typeof(AddRelatedManagementOrganisationModuleDecisionProvider), modulePopup.EmbeddedModulePopupOKButtonStrategy);
				}

				ZFormModaliser.LastFormShownDialogForTest = null;
				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = false;
				form.OrgManagementGroupingControl.AttachToolStripButton.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(Env.Security.OrgDetailsModifyRelatedParties.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAttachOrganisation_DeduplicationStarted()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var isDeduplicationStarted = false;
			org1.DeduplicationStarted += (o, e) => isDeduplicationStarted = true;

			using (var form = new OrgManagementGroupingControlFormForTest(org1.OrgManagementGroupingModel))
			{
				form.Show();
				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object popupForm)
				{
					((EmbeddedModulePopup)popupForm).EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new OrgHeader[] { org2 });
				});

				((IDeduplicatable)org1).ShouldRunDeduplication = true;
				AssertEquals(false, isDeduplicationStarted);
				form.OrgManagementGroupingControl.AttachToolStripButton.PerformClick();
				AssertEquals(true, isDeduplicationStarted);
			}
		}

		public void TestDeachOrganisation_DeduplicationStarted()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var isDeduplicationStarted = false;
			org1.DeduplicationStarted += (o, e) => isDeduplicationStarted = true;
			((IDeduplicatable)org1).ShouldRunDeduplication = true;

			using (var form = new OrgManagementGroupingControlFormForTest(org1.OrgManagementGroupingModel))
			{
				form.Show();
				Env.Security.OrgDetailsModifyRelatedParties.IsAllowed = true;

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object popupForm)
				{
					((EmbeddedModulePopup)popupForm).EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new OrgHeader[] { org2 });
				});
				var control = form.OrgManagementGroupingControl;
				control.AttachToolStripButton.PerformClick();
				control.Tree.AllNodes.ToList()[1].IsSelected = true;
				isDeduplicationStarted = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();

				AssertEquals(false, isDeduplicationStarted);
				form.OrgManagementGroupingControl.DetachToolStripButton.PerformClick();
				AssertEquals(true, isDeduplicationStarted);
			}
		}

		#endregion

		#region Implementation

		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			return new OrgManagementGroupingControlFormForTest(org.OrgManagementGroupingModel);
		}

		public class OrgManagementGroupingControlFormForTest : ZForm
		{
			public OrgManagementGroupingControlFormForTest(OrgManagementGroupingModel model) : base(model) { }

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 768, true);
				Controls.Add(OrgManagementGroupingControl);
				BindingSource.SetBindingMember(OrgManagementGroupingControl, ".");
				CaptionRenderingEnabled = true;
			}

			public readonly OrgManagementGroupingControl OrgManagementGroupingControl = new OrgManagementGroupingControl();
		}

		#endregion
	}
}
