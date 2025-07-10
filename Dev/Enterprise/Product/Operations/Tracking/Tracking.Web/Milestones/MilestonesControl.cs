using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	[DefaultProperty("Caption"), ToolboxData("<{0}:MilestonesControl runat=server></{0}:MilestonesControl>")]
	public class MilestonesControl : CompositeControl, ISelfBindingWebControl, IPostBackDataHandler
	{
		#region Constructors

		public MilestonesControl()
			: base()
		{ }

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue("Milestones"), Browsable(true)]
		public string PanelLabel
		{
			get { return panelLabel; }
			set { panelLabel = value; }
		}
		string panelLabel = Res.GetString("9ad4228e-ce2b-42e4-9611-2af4083ceb28", "Milestones");

		[Category("Appearance"), DefaultValue("True"), Browsable(true)]
		public bool DisablePanelCollapsing
		{
			get { return disablePanelCollapsing; }
			set { disablePanelCollapsing = value; }
		}
		bool disablePanelCollapsing = true;

		[Category("Appearance"), DefaultValue("SectionTitle"), Browsable(true)]
		public string PanelCssClass
		{
			get { return panelCssClass; }
			set { panelCssClass = value; }
		}
		string panelCssClass = "SectionTitle";

		[Category("Appearance"), DefaultValue("False"), Browsable(true)]
		public bool DisplayInPanel
		{
			get { return displayInPanel; }
			set { displayInPanel = value; }
		}
		bool displayInPanel;

		[Category("Appearance"), DefaultValue("DetailsCell"), Browsable(true)]
		public string ItemCssClass
		{
			get { return itemCssClass; }
			set { itemCssClass = value; }
		}
		string itemCssClass = "DetailsCell";

		[Category("Appearance"), DefaultValue("DetailsHeader"), Browsable(true)]
		public string HeaderCssClass
		{
			get { return headerCssClass; }
			set { headerCssClass = value; }
		}
		string headerCssClass = "DetailsHeader";

		#endregion

		#region Declaration

		protected HiddenField GridStateControl
		{
			get
			{
				if (gridStateControl == null)
				{
					gridStateControl = new HiddenField();
					gridStateControl.ID = "GridState";
				}
				return gridStateControl;
			}
		}

		HiddenField gridStateControl;
		protected ZDataGrid MilestonesGrid;
		ZCollapsablePanel OutterPanel;
		ZButton MilestonesEditButton;
		ZButton MilestonesSaveButton;
		ZButton MilestonesEditCancelButton;

		#endregion

		#region Overrides

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			Controls.Clear();

			var mainTable = new Table();
			mainTable.CellPadding = 0;
			mainTable.CellSpacing = 0;

			var gridRow = new TableRow();
			var gridCell = new TableCell();
			gridCell.HorizontalAlign = HorizontalAlign.Left;
			gridCell.VerticalAlign = VerticalAlign.Top;
			gridCell.Controls.Add(GridStateControl);
			gridCell.Controls.Add(MilestonesGrid);
			gridRow.Cells.Add(gridCell);
			mainTable.Rows.Add(gridRow);

			var buttonsRow = new TableRow();
			var buttonsCell = new TableCell();
			buttonsCell.HorizontalAlign = HorizontalAlign.Left;
			buttonsCell.VerticalAlign = VerticalAlign.Middle;
			buttonsCell.Controls.Add(MilestonesEditButton);
			buttonsCell.Controls.Add(MilestonesSaveButton);
			buttonsCell.Controls.Add(MilestonesEditCancelButton);
			buttonsRow.Cells.Add(buttonsCell);
			mainTable.Rows.Add(buttonsRow);

			if (DisplayInPanel)
			{
				OutterPanel.Controls.Add(mainTable);
				Controls.Add(OutterPanel);
			}
			else
			{
				Controls.Add(mainTable);
			}
		}

		protected override void OnInit(EventArgs e)
		{
			saveErrorMessages = string.Empty;
			base.OnInit(e);

			OutterPanel = new ZCollapsablePanel();
			OutterPanel.ID = ID + (NoResString)"Panel";
			OutterPanel.Label = PanelLabel;
			OutterPanel.CssClass = PanelCssClass;
			OutterPanel.DisableCollapsing = DisablePanelCollapsing;

			MilestonesGrid = new ZDataGrid();
			MilestonesGrid.ID = ID + (NoResString)"Grid";
			MilestonesGrid.BindTo = BindTo;
			MilestonesGrid.CssClass = CssClass + " ContentSection";
			MilestonesGrid.AutoGenerateColumns = false;
			MilestonesGrid.ItemStyle.CssClass = ItemCssClass;
			MilestonesGrid.HeaderStyle.CssClass = HeaderCssClass;

			MilestonesEditButton = new ZButton();
			MilestonesEditButton.ID = ID + (NoResString)"Edit";
			MilestonesEditButton.Text = Res.GetString("f33b03cd-a769-45c9-b6d1-7766182ae084", "Edit Milestones");
			MilestonesEditButton.Click += OnEditButtonClick;

			MilestonesSaveButton = new ZButton();
			MilestonesSaveButton.ID = ID + (NoResString)"Save";
			MilestonesSaveButton.Text = Res.GetString("c7f45e5e-aa96-49f1-b237-ca22bb644503", "Save");
			MilestonesSaveButton.Click += OnSaveButtonClick;

			MilestonesEditCancelButton = new ZButton();
			MilestonesEditCancelButton.ID = ID + (NoResString)"Cancel";
			MilestonesEditCancelButton.Text = Res.GetString("b35f1d55-31a0-4374-9f78-9fe31833f472", "Cancel");
			MilestonesEditCancelButton.Click += OnEditCancelButtonClick;

			if (DisplayInPanel)
			{
				MilestonesConfigurationHelper.ConfigureMilestonesGrid(MilestonesGrid, OutterPanel, null, IsInEditMode);
			}
			else
			{
				MilestonesConfigurationHelper.ConfigureMilestonesGrid(MilestonesGrid, null, null, IsInEditMode);
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			ReBind();
			if (!string.IsNullOrEmpty(saveErrorMessages))
			{
				ZStringBuilder message = new ZStringBuilder();
				message.Append((NoResString)"Please fix the following errors before proceeding:\\n\\n"); // Client Message
				message.Append(saveErrorMessages);
				((ZPage)Page).ScriptNotificationMessage(message.ToString());
			}
			base.OnPreRender(e);
		}

		public override void RenderControl(HtmlTextWriter writer)
		{
			MilestonesEditButton.Visible = !IsInEditMode && AllowUpdate;
			MilestonesSaveButton.Visible = IsInEditMode && AllowUpdate;
			MilestonesEditCancelButton.Visible = IsInEditMode && AllowUpdate;
			if (Controls.Count > 0)
			{
				Controls[0].RenderControl(writer);
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
		}

		public override void RenderEndTag(HtmlTextWriter writer)
		{
		}

		#endregion

		#region ISelfBindingWebControl Members

		public void ReBind()
		{
			Bind(BusinessEntity);
		}

		public bool IsBindable(object dataSource) => IsBindableDataSource(dataSource);

		protected virtual bool IsBindableDataSource(object dataSource) => MilestonesGrid.IsBindable(dataSource);

		public void Bind(object dataSource)
		{
			if (IsBindable(dataSource))
			{
				if (dataSource is IMilestonesProvider)
				{
					BusinessEntity = (IMilestonesProvider)dataSource;
				}

				OnPreBind(dataSource);

				if (IsInEditMode)
				{
					MilestonesGrid.BindTo = "EditableMilestones";
				}
				else
				{
					MilestonesGrid.BindTo = "Milestones";
				}
				MilestonesGrid.Bind(dataSource);
			}
		}

		protected virtual void OnPreBind(object dataSource)
		{
			MilestonesGrid.AllowEdit = IsInEditMode && AllowUpdate;
			MilestonesGrid.ReadOnly = !IsInEditMode && AllowUpdate;
			MilestonesConfigurationHelper.ConfigureMilestonesGridColumnsVisibility(MilestonesGrid, IsInEditMode && AllowUpdate);
			if (Page != null)
			{
				((ZPage)Page).DisablePageBusinessObjectValidation = true;
			}
		}

		protected bool AllowUpdate
		{
			get
			{
				var result = false;
				if (SiteUser != null)
				{
					if (SiteUser.CanUpdateActualMilestones || SiteUser.CanUpdateEstimatedMilestones)
					{
						if (BusinessEntity?.EditableMilestones != null && BusinessEntity.EditableMilestones.Count > 0)
						{
							result = true;
						}
					}
				}
				return result;
			}
		}

		protected TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance.SiteUser as TrackingSiteUser; }
		}

		public void UnBind()
		{
			MilestonesGrid.UnBind();
		}

		#endregion

		#region IBindTo Members

		public string BindTo
		{
			get { return bindTo; }
			set { bindTo = value; }
		}
		string bindTo = "";

		#endregion

		#region Implementation

		protected virtual void OnEditButtonClick(object sender, EventArgs e)
		{
			IsInEditMode = true;
		}

		protected virtual void OnSaveButtonClick(object sender, EventArgs e)
		{
			saveErrorMessages = string.Empty;
			if (Page != null)
			{
				if (BusinessEntity != null)
				{
					var editableMilestones = BusinessEntity.EditableMilestones;
					var message = new ZStringBuilder();
					foreach (TrackingMilestone milestone in editableMilestones)
					{
						milestone.Task.ClearRowNotifications();
						milestone.Task.RunPreSaveValidation();
						if (milestone.Task.HasErrors)
						{
							ZNotificationCollector milestoneNotifications = new ZNotificationCollector(milestone.Task, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
							message.Append(milestoneNotifications.GetErrors().ToUniqueMessageListString().Replace("'", "\\'").Replace("\r", "").Replace("\n", "\\n"));
							message.Append(String.Format("\\n\\n"));
						}
					}
					saveErrorMessages = message.ToString();
					if (string.IsNullOrEmpty(saveErrorMessages))
					{
						if (editableMilestones.HasChanges)
						{
							foreach (TrackingMilestone milestone in editableMilestones)
							{
								try
								{
									milestone.Task.Factory.Save();
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
								}
								break;
							}
							var emailNotification = BusinessEntity as IBizOChangesEmailNotification;
							if (emailNotification != null)
							{
								new BusinessObjectChangesEmailNotifier(emailNotification);
							}
						}
						BusinessEntity.ReloadMilestones();
						IsInEditMode = false;
					}
				}
			}
		}

		string saveErrorMessages = string.Empty;

		protected virtual void OnEditCancelButtonClick(object sender, EventArgs e)
		{
			IsInEditMode = false;
			if (BusinessEntity != null)
			{
				foreach (TrackingMilestone milestone in BusinessEntity.Milestones)
				{
					if (milestone.Task != null)
					{
						milestone.Task.Reload();
					}
				}
				BusinessEntity.ReloadMilestones();
			}
		}

		protected bool IsInEditMode
		{
			get { return GridStateControl.Value == "1"; }
			set { GridStateControl.Value = value ? "1" : ""; }
		}

		protected IMilestonesProvider BusinessEntity
		{
			get { return fBusinessEntity; }
			set { fBusinessEntity = value; }
		}
		IMilestonesProvider fBusinessEntity;

		#endregion

		#region IPostBackDataHandler Members

		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			throw new NotImplementedException();
		}

		public void RaisePostDataChangedEvent()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
