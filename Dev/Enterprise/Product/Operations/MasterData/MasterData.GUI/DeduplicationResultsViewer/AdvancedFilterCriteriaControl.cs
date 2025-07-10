using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class AdvancedFilterCriteriaControl : ZUserControl
	{
		bool collapsed;
		internal PersonFilterContentControl PersonFilterContentControl { get; private set; }
		internal OrganisationFilterContentControl OrganisationFilterContentControl { get; private set; }

		public AdvancedFilterCriteriaControl()
		{
			InitializeComponent();
		}

		internal void ShowOrganisationFilterContentControl(DeduplicationOrganisationResultDetail duplicationResultDetail, PotentialDuplicatesUserControl potentialDuplicatesUserControl)
		{
			if (OrganisationFilterContentControl is null)
			{
				AddOrganisationFilterContentControl();
			}
			OrganisationFilterContentControl.SetupDataContext(duplicationResultDetail, potentialDuplicatesUserControl);
		}

		internal void ShowPersonFilterContentControl(DeduplicationPersonResultDetail duplicationResultDetail, PotentialDuplicatesUserControl potentialDuplicatesUserControl)
		{
			if (PersonFilterContentControl is null)
			{
				AddPersonFilterContentControl();
			}
			PersonFilterContentControl.SetDataContext(duplicationResultDetail, potentialDuplicatesUserControl);
		}

		void ExpandCollapseButton_LinkClicked(object sender, EventArgs e)
		{
			collapsed = !collapsed;
			if (collapsed)
			{
				MainTableLayoutPanel.Controls.Remove(ContentPanel);
				MainTableLayoutPanel.RowCount = 1;
				ExpandCollapseButton.Text = (NoResString)"⯆ ";
			}
			else
			{
				MainTableLayoutPanel.Controls.Add(ContentPanel);
				MainTableLayoutPanel.RowCount = 2;
				ExpandCollapseButton.Text = (NoResString)"⯅ ";
			}
		}

		void AddOrganisationFilterContentControl()
		{
			OrganisationFilterContentControl = new OrganisationFilterContentControl()
			{
				AutoSize = true,
				Dock = DockStyle.Fill,
				Name = "OrganisationFilterContentControl",
			};

			ContentPanel.Controls.Add(OrganisationFilterContentControl);
			ContentPanel.PerformLayout();
		}

		void AddPersonFilterContentControl()
		{
			PersonFilterContentControl = new PersonFilterContentControl()
			{
				AutoSize = true,
				Dock = DockStyle.Fill,
				Name = "PersonFilterContentControl",
			};

			ContentPanel.Controls.Add(PersonFilterContentControl);
			ContentPanel.PerformLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (!ContentPanel.IsDisposed)
			{
				ContentPanel.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
