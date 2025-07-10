using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class NumberRangesUserControl : ZUserControl
	{
		public NumberRangesUserControl()
		{
			InitializeComponent();
		}

		IViewStmNumsOwner Owner => BindingSource.DataSource as IViewStmNumsOwner;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (Owner is OrgHeader header)
			{
				MatchingDetailsGrid.RemoveFromAvailableColumns(StmNumberRangeMatchingDetail.Schema.PatentNumber, StmNumberRangeMatchingDetail.Schema.CustomsArea);
			}
			else if (Owner is GlbStaff)
			{
				MatchingDetailsGrid.RemoveFromAvailableColumns(StmNumberRangeMatchingDetail.Schema.NRM_OH_Client, StmNumberRangeMatchingDetail.Schema.NRM_WW_Whs);
			}

			EnableNumberFountainsButtonsIfAllowed();
		}

		void NumberFountainsAddButton_Click(object sender, EventArgs e)
		{
			if (CheckThatOwnerHasNoChanges())
			{
				var newStmNums = LoadOrCreateViewStmNumInAStandAloneFactory();
				newStmNums.SN_Owner = Owner.PK;

				var editorDialogResult = ZFormModaliser.ShowDialogAndDispose(GetViewStmNumsEditorForm(newStmNums));
				if (editorDialogResult == DialogResult.OK)
				{
					SaveStmNumsStandAloneFactory(newStmNums.Factory);
				}
			}
		}

		void NumberFountainsEditButton_Click(object sender, EventArgs e)
		{
			var stmNums = TryGetCurrentViewStmNum();
			if (stmNums != null)
			{
				var stmNumsReloaded = LoadOrCreateViewStmNumInAStandAloneFactory(stmNums);
				if (stmNumsReloaded != null)
				{
					var editorDialogResult = ZFormModaliser.ShowDialogAndDispose(GetViewStmNumsEditorForm(stmNumsReloaded));
					if (editorDialogResult == DialogResult.OK && !stmNumsReloaded.HasErrors)
					{
						SaveStmNumsStandAloneFactory(stmNumsReloaded.Factory);
					}
				}
			}
		}

		ViewStmNumsEditorForm GetViewStmNumsEditorForm(ViewStmNums stmNums)
		{
			return stmNums is OrganisationViewStmNums
				? new OrganisationViewStmNumsEditorForm(stmNums as OrganisationViewStmNums)
				: new ViewStmNumsEditorForm(stmNums);
		}

		void NumberFountainsDeleteButton_Click(object sender, EventArgs e)
		{
			if (CheckThatOwnerHasNoChanges())
			{
				var stmNums = TryGetCurrentViewStmNum();
				if (stmNums != null)
				{
					if (((ICanDelete)stmNums).CanDelete)
					{
						var result = Globals.Message.Show(
							Res.GetString("c4fd3ff5-c5bb-414e-937b-c07292d46446", "This would delete number fountain customization. Continue?"),
							Res.GetString("c14b9c16-b2f2-4239-b753-5c792b9044d2", "Confirm"),
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Warning,
							DialogResult.No);

						if (result == DialogResult.Yes)
						{
							var stmNumsReloaded = LoadOrCreateViewStmNumInAStandAloneFactory(stmNums);
							if (stmNumsReloaded != null)
							{
								stmNumsReloaded.Delete();
								SaveStmNumsStandAloneFactory(stmNumsReloaded.Factory);
							}
						}
					}
					else
					{
						Globals.Message.ShowError(((ICanDelete)stmNums).ReasonForNotAbleToDelete);
					}
				}
			}
		}

		ViewStmNums LoadOrCreateViewStmNumInAStandAloneFactory(ViewStmNums existingStmNum = null)
		{
			var stmNumsStandAloneFactory = new BusinessObjectFactory() { NameForDebugging = "ViewStmNums Standalone Factory" };

			if (existingStmNum == null)
			{
				return Owner is OrgHeader ? stmNumsStandAloneFactory.New<OrganisationViewStmNums>() : stmNumsStandAloneFactory.New<StaffViewStmNums>();
			}
			else
			{
				var query = new ZQuery(ViewStmNumsSchema.SN_Name, existingStmNum.SN_Name);
				query.AddToFilter(ViewStmNumsSchema.SN_Owner, existingStmNum.SN_Owner);

				return stmNumsStandAloneFactory.LoadTop1<ViewStmNums>(query);
			}
		}

		ViewStmNums TryGetCurrentViewStmNum()
		{
			if (RangesGrid.SelectedElements.Length != 1)
			{
				Globals.Message.Show(Res.GetString("899163ea-e600-4553-bd03-7d150742a111", "Please select a Number Fountain from the grid."));
				return null;
			}

			return RangesGrid.SelectedElements[0] as ViewStmNums;
		}

		void EnableNumberFountainsButtonsIfAllowed()
		{
			var isEnabled = false;
			if (Owner is OrgHeader header)
			{
				isEnabled = header.SecurityProvider.HasModifyConfigSecurity;
			}
			else if (Owner is GlbStaff)
			{
				isEnabled = Env.Security.StaffModifyAll.IsAllowed;
			}

			NumberFountainsAddButton.Enabled = isEnabled;
			NumberFountainsEditButton.Enabled = isEnabled;
			NumberFountainsDeleteButton.Enabled = isEnabled;
		}

		void SaveStmNumsStandAloneFactory(BusinessObjectFactory standAloneFactory)
		{
			try
			{
				standAloneFactory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			if (Owner != null)
			{
				Owner.Fountains.RefreshFromDb();
				Owner.Factory.ClearCachedValue<Dictionary<string, CodeDescriptionPairList>>(string.Format(Culture.Invariant, "StmNumberRangeMatchingDetailsLookups|StmNumsPrefixes|{0}", Owner.PK));
			}
		}

		bool CheckThatOwnerHasNoChanges()
		{
			if (!Owner.IsInDatabase || Owner.HasChanges)
			{
				Globals.Message.Show(Res.GetString("9f165fa4-59d6-4c3f-886e-3289e16b2704", "Please save changes first."));
				return false;
			}

			return true;
		}
	}
}
