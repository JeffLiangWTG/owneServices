using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ZClientIntelligenceForm : BaseOrganisationsForm, IWorkflowTaskNavigationOverridable
	{
		public ZClientIntelligenceForm() : base()
		{
		}

		public ZClientIntelligenceForm(OrgHeader organisation) : base(organisation)
		{
			WorkflowTabPage.Initialize(organisation);
			SetupForm();
			((IDeduplicatable)organisation).ShouldRunDeduplication = true;
		}

		public override string FormCaption
		{
			get { return Res.GetString("ZClientIntelligenceForm|FormCaption", "Client Intelligence") + base.FormCaption; }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			Organisation.OH_IsSalesLead = true;
			return base.ShowPreSaveDialogs();
		}

		#region GUI Setup

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}

		const string ReceivablesTabPageName = "ReceivablesTabPage";
		const string ConsigneeTabPageName = "ConsigneeTabPage";
		const string ConsignorTabPageName = "ConsignorTabPage";
		const string WorkflowTabPageName = "WorkflowTabPage";

		void SetupForm()
		{
			SetOrderOfTabPages();
			WorkflowTabPage.Initialize(Organisation);
			SetupEventHandlers();
			RemoveTabPagesNotSelectedInRegistry();
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			if (ControllerID == null)
			{
				ControllerID = ControllerIDs.ClientIntelligence;
			}

			if (Organisation != null && !Organisation.SecurityProvider.HasModifyDetailsSecurity)
			{
				WorkflowTabPage.SetReadOnlyIncludingChildren();
			}
		}

		void SetupEventHandlers()
		{
			((OrgHeaderDocumentSupporter)Organisation.DocumentSupporter).OnRoutingOrderRecommendationPrinted += new OrgHeaderDocumentSupporter.RoutingOrderRecommendationPrintedEventHandler(Organisation_OnRoutingOrderRecommendationPrinted);
		}

		void RemoveTabPagesNotSelectedInRegistry()
		{
			if (!Env.Registry.OrgShowConsigneeConsignorTab)
			{
				ConsigneeTabPage.TabVisible = false;
				ConsignorTabPage.TabVisible = false;
			}

			if (!Env.Registry.OrgShowARTab)
			{
				ReceivablesTabPage.TabVisible = false;
			}
		}

		void SetOrderOfTabPages()
		{
			OrganisationsTabControl.TabPages.Remove(ContactsTabPage);
			OrganisationsTabControl.TabPages.Insert(ContactsTabPage, 3);

			OrganisationsTabControl.TabPages.Remove(AddressesTabPage);
			OrganisationsTabControl.TabPages.Insert(AddressesTabPage, 2);
		}

		void Organisation_OnRoutingOrderRecommendationPrinted(object sender, OrgHeaderDocumentSupporter.RoutingOrderRecommenationPrintingEventArgs e)
		{
			e.ErrorMessage = Res.GetString("33de300a-d73a-459f-99d7-1cf57bb67a31", @"You can only print Sales Manager documents from the Client Intelligence system.
Please use Reference Files - Organizations to access other documents.");
			e.Cancel = true;  // Can't print these documents from Client Intelligence
		}

		#region Change Tab Pages

		protected override void ChangeTabPage(object sender, EventArgs e)
		{
			base.ChangeTabPage(sender, e);
			bool showControls = true;
			bool setControls = true;
			ZLabel selectedLabel = null;

			TabPage selectedTab = ((TabControl)sender).SelectedTab;

			if (selectedTab != null && !TabControlsAlreadySet)
			{
				switch (selectedTab.Name)
				{
					case ReceivablesTabPageName:
						showControls = Organisation.OH_IsDebtor;
						selectedLabel = ReceivablesNotSelectedLabel;
						break;

					case ConsigneeTabPageName:
						showControls = Organisation.OH_IsConsignee;
						selectedLabel = ConsigneeNotSelectedLabel;
						break;

					case ConsignorTabPageName:
						showControls = Organisation.OH_IsConsignor;
						selectedLabel = ConsignorNotSelectedLabel;
						break;

					case WorkflowTabPageName:
						showControls = true;
						break;

					default:
						setControls = false;
						break;
				}

				if (setControls)
				{
					SetTabPageControls(selectedTab, selectedLabel, showControls);
				}
			}
		}

		#endregion

		#endregion

		#region IWorkflowForm Members
		void IWorkflowTaskNavigationOverridable.NavigateToWorkflowItem(ProcessTask task)
		{
			if (Organisation != null && task.P9_ParentID == Organisation.PK)
			{
				TopLevelTabControl.SelectedTab = WorkflowTabPage;
				WorkflowTabPage.NavigateToWorkflowItem(task);
			}
			else
			{
				TopLevelTabControl.SelectedTab = SalesTabPage;
				SalesControl.NavigateToWorkflowItem(task);
			}
		}

		void IWorkflowTaskNavigationOverridable.NavigateToWorkflowItem(IProcessHeader workflow)
		{
			if (Organisation != null && workflow.FH_ParentId == Organisation.PK)
			{
				TopLevelTabControl.SelectedTab = WorkflowTabPage;
				WorkflowTabPage.NavigateToWorkflowItem(workflow);
			}
			else
			{
				var task = workflow.Tasks.OfType<ProcessTask>().FirstOrDefault();

				if (task != null)
				{
					((IWorkflowTaskNavigationOverridable)this).NavigateToWorkflowItem(task);
				}
			}
		}

		#endregion
	}
}
