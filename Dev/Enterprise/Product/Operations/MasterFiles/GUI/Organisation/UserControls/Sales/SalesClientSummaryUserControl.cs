using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesClientSummaryUserControl : OrganisationContainerControl
	{
		protected SalesClientSummaryUserControl()
		{
			InitializeComponent();
			UpdateImportCmdtyCodeFindBoxEnabledValue();
			UpdateExportCmdtyCodeFindBoxEnabledValue();

			SubscriptionsGrid.ReadOnly = !Env.Security.OrganisationControlSubscriptionPreferences.IsAllowed;

			if (!SubscriptionsGrid.ReadOnly)
			{
				SubscriptionsGrid.AfterBind -= SubscriptionsGrid_AfterBind;
				SubscriptionsGrid.AfterBind += SubscriptionsGrid_AfterBind;
			}
		}

		public static SalesClientSummaryUserControl New()
		{
			var overridden = OverridableNewDelegate.Value;
			return overridden != null ? overridden() : new SalesClientSummaryUserControl();
		}

		protected delegate SalesClientSummaryUserControl NewDelegate();

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (Organisation != null)
			{
				var miscServ = new BusinessObjectFactory().Load<OrgMiscServ>(Organisation.MiscServ.PK);
				if (miscServ != null)
				{
					miscServ.UpdateLastCallDateFromSalesCalls();
					miscServ.Factory.Save();
				}
				SetupButtons();
			}
		}

		#region SubscriptionsGrid

		void SubscriptionsGrid_AfterBind(object sender, EventArgs e)
		{
			SubscriptionsGrid.CurrentCellChanged -= AddSubscriptionOverrideWarning;
			SubscriptionsGrid.CurrentCellChanged += AddSubscriptionOverrideWarning;
		}

		internal void AddSubscriptionOverrideWarning(object sender, EventArgs e)
		{
			Globals.Message.Show(SubscriptionOverrideWarning);
			SubscriptionsGrid.CurrentCellChanged -= AddSubscriptionOverrideWarning;
		}

		public static string SubscriptionOverrideWarning => Res.GetString("3d8d62f7-6776-4483-965a-3b540d142ced", "Un-subscribing this Organization from campaign categories/types will overwrite existing related Subscription Preferences for the Organization's Contacts.");

		#endregion

		#region GUI Setup

		void SetupButtons()
		{
			SetupEditButtonGraphics(OM_CMLastCallShortcutButton);
			SetupEditButtonGraphics(FollowUpDateShortcutButton);
			SetupEditButtonGraphics(LastUnactionedDateShortcutButton);

			OM_CMLastCallShortcutButton.Click += delegate
			{ ShowEditFormForCall(Organisation.MiscServ.GetLastCallDateSalesCall(), Res.GetString("678EC3B4-4D67-470F-90A4-83B3A4ED8D84", "There is no Last Actual Communication")); };
			FollowUpDateShortcutButton.Click += delegate
			{ ShowEditFormForCall(Organisation.MiscServ.GetFollowUpDateSalesCall(), Res.GetString("A5022B45-6F80-4B29-AF41-6ED2AC2C08D5", "There is no Next Scheduled Communication")); };
			LastUnactionedDateShortcutButton.Click += delegate
			{ ShowEditFormForCall(Organisation.MiscServ.GetLastUnactionedCallDateSalesCall(), Res.GetString("4B54A1FA-8E96-4BDE-849A-3DDAAD2B30D0", "There is no Last Un-Actioned Communication")); };
		}

		void SetupEditButtonGraphics(ZButton button)
		{
			button.FlatStyle = FlatStyle.Flat;
			button.BackgroundImage = Icons.GetImage(IconTypes.EditButtonRest);
			button.BackgroundImageLayout = ImageLayout.Zoom;
			button.MouseEnter += delegate
			{ button.BackgroundImage = Icons.GetImage(IconTypes.EditButtonActive); };
			button.MouseLeave += delegate
			{ button.BackgroundImage = Icons.GetImage(IconTypes.EditButtonRest); };
		}

		void ShowEditFormForCall(OrgSalesCall call, string noCallMessage)
		{
			if (call != null)
			{
				SalesCallController.ShowEditForm(call);
			}
			else
			{
				Globals.Message.Show(noCallMessage);
			}
		}

		#endregion

		#region Most Recent Opportunity

		void UpdateMostRecentOpportunity()
		{
			OpenOpportunityButton.Enabled = (Organisation != null && Organisation.MostRecentSalesOpportunity != null);
		}

		protected OrgHeader Organisation
		{
			get
			{
				OrgHeader result = null;
				ZForm form = (ZForm)FindForm();
				if (form != null)
				{
					if (form.BusinessEntity is OrgHeader)
					{
						result = (OrgHeader)form.BusinessEntity;
					}
				}
				return result;
			}
		}

		void BindingSource_DataSourceChanged(object sender, EventArgs e)
		{
			UpdateMostRecentOpportunity();
		}

		void OpenOpportunityButton_Click(object sender, EventArgs e)
		{
			if (Organisation == null)
			{
				Globals.Message.Show(Res.GetString("f686b26b-feaf-4baf-adc7-5a95f359f643", "Form not linked to an organization."));
			}
			else if (Organisation.MostRecentSalesOpportunity == null)
			{
				Globals.Message.Show(Res.GetString("4b57b3b5-17ac-4e74-baca-20d5d4fc085a", "Organization does not have any opportunities."));
			}
			else
			{
				OpportunityController.ShowEditForm(Organisation.MostRecentSalesOpportunity);
			}
		}

		ZController opportunityController;
		ZController OpportunityController
		{
			get
			{
				return opportunityController ?? (opportunityController = ZControllerFactory.Create(ControllerIDs.Opportunity));
			}
		}

		ZController salesCallController;
		ZController SalesCallController
		{
			get
			{
				return salesCallController ?? (salesCallController = ZControllerFactory.Create(ControllerIDs.Communication));
			}
		}

		#endregion

		#region CommodityDetails

		void UpdateImportCmdtyCodeFindBoxEnabledValue()
		{
			OM_RH_NKCMMainImportCmdtyCodeFindBox.Enabled = OM_CMDoesImportsCheckBox.Checked;
		}

		void UpdateExportCmdtyCodeFindBoxEnabledValue()
		{
			OM_RH_NKCMMainExportCmdtyBoundCodeFindBox.Enabled = OM_CMDoesExportsCheckBox.Checked;
		}

		void OM_CMDoesImportsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateImportCmdtyCodeFindBoxEnabledValue();
		}

		void OM_CMDoesExportsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateExportCmdtyCodeFindBoxEnabledValue();
		}
		#endregion
	}
}
