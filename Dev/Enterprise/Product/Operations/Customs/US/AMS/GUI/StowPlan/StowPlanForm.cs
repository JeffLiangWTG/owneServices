using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.GUI
{
	public partial class StowPlanForm : ZChildForm
	{
		public StowPlanForm()
		{
			InitializeComponent();
			this.MinimizeBox = false;
		}

		public StowPlanForm(StowPlanSailingData sailingData)
			: base(sailingData)
		{
			InitializeComponent();
			this.MinimizeBox = false;
			sailingData.IssueFilterInfo.ValueChanged -= filterIssue_ValueChanged;
			sailingData.IssueFilterInfo.ValueChanged += filterIssue_ValueChanged;
		}

		StowPlanSailingData SailingData
		{
			get { return (StowPlanSailingData)base.DataSource; }
		}

		void filterIssue_ValueChanged(object sender, EventArgs e)
		{
			SailingData.IssueCollectionView.Rebuild();
		}

		public static void ShowDialog(JobVoyage voyage)
		{
			voyage.RunPreSaveValidation();
			if (voyage.HasErrors)
			{
				Globals.Message.Show(Res.GetString("1BFBDAD5-D3FA-49DE-B7F9-94DEC064AB1F", "This schedule has errors, please fix and try again."), Res.GetString("D1481D78-3232-475E-8431-BAC3B6A7D396", "Errors"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else if (!voyage.HasChanges || Globals.Message.Show(
				Res.GetString("62D001D1-73C9-4FAF-8936-1119D5D0F21C", "This schedule has unsaved changes, would you like to save the changes and proceed?"),
				Res.GetString("38F08EEE-3038-488A-A401-B32068200626", "Unsaved Changes"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				try
				{
					if (voyage.HasChanges)
					{
						voyage.Factory.Save();
					}
					using (var sailignData = new StowPlanSailingData(voyage))
					{
						using (var form = new StowPlanForm(sailignData))
						{
							ZFormModaliser.ShowDialogWithoutDispose(form);
						}
					}
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(exception);
				}
			}
		}

		public override string FormCaption
		{
			get { return "Stow Plan"; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void Dispose(bool disposing)
		{
			if (SailingData != null)
			{
				SailingData.IssueFilterInfo.ValueChanged -= filterIssue_ValueChanged;
			}
			base.Dispose(disposing);
		}

		internal ZMenuItem ValidationMenuItem;
		protected override void AddAdornments()
		{
			base.AddAdornments();

			var mainMenu = new ZMainMenu();
			this.Menu = mainMenu;

			var fileMenuItem = new ZMenuItem(ResString.GetMultilingualString("StowPlanForm|File", "&File"));
			fileMenuItem.Name = ZFormMenuStrategy.FileMenuItemName;
			ValidationMenuItem = new ZMenuItem(ResString.GetMultilingualString("StowPlanForm|File.ValidateAll", "&Validate All"), delegate
			{ this.RunActionWithProcessBox(ZForm.LoadingValidationCodeText, SailingData.RebuidIssues); });
			fileMenuItem.MenuItems.Add(ValidationMenuItem);

			var actionMenuItem = new ZMenuItem(ResString.GetMultilingualString("StowPlanForm|Actions", "Actio&ns"));
			actionMenuItem.Name = ZFormMenuStrategy.ActionsMenuItemName;
			actionMenuItem.MenuItems.Add(ZFormMenuStrategy.GetResetFormSizeMenuItem(this));

			mainMenu.MenuItems.AddRange(new[] { fileMenuItem, actionMenuItem });
		}
	}
}
