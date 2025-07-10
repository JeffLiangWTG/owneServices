using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbAccreditationsTabControl : ZUserControl
	{
		public GlbAccreditationsTabControl()
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(this.accreditationGroupTreeControl, "AccreditationAttemptCollection.TreeModel");
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupControls();

			if (!DesignModeFinder.IsDesigning)
			{
				attemptEditButton.Image = Icons.GetImage(IconTypes.EditButtonRest);
				attemptDeleteButton.Image = Icons.GetImage(IconTypes.DeleteButtonRest);
			}
		}

		void SetupControls()
		{
			var deleteMenuItem = new ZMenuItem(ResString.GetMultilingualString("59F04245-1F6F-4640-A092-B263860CAE2D", "&Delete"), DeleteMenuItem_Click);
			attemptsGrid.ContextMenu.MenuItems.Replace(attemptsGrid.DeleteMenuItem, new ZMenuItem[] { deleteMenuItem });

			if (!Env.Security.GlbAccreditationAttemptEdit.IsAllowed)
			{
				var completionDueDateColumn = attemptsGrid.Columns.First(x => x.ColumnName == GlbAccreditationAttemptSchema.Constants.HAA_CompletionDueDate);
				completionDueDateColumn.ColumnStyle.ReadOnly = true;
			}

			if (Person != null && Person.ReadOnly)
			{
				deleteMenuItem.Enabled = false;
				attemptDeleteButton.Enabled = false;
			}
		}

		public void SelectAttempt(IGlbAccreditationAttempt attempt)
		{
			var attemptBizo = attempt as BusinessObject;
			if (attemptBizo != null)
			{
				attemptsGrid.SelectSingleElement(attemptBizo);
			}
		}

		void DeleteSelectedAttempts()
		{
			if (Env.Security.GlbAccreditationAttemptEdit.IsAllowed)
			{
				if (Person != null && !attemptsGrid.SelectedElements.IsNullOrEmpty())
				{
					Person.DeleteAttempts(attemptsGrid.SelectedElements);
				}
			}
			else
			{
				Globals.Message.ShowError(Env.Security.GlbAccreditationAttemptEdit.ErrorMessageForNotAllowed);
			}
		}

		void OpenFirstSelectedAttempt()
		{
			if (attemptsGrid.SelectedElements.IsNullOrEmpty())
			{
				return;
			}

			var form = ObjectFactory.Get<IGlbAccreditationAttemptForm>("IGlbAccreditationAttemptForm", attemptsGrid.SelectedElements[0]);
			((ZForm)form).Show();
		}

		void DeleteMenuItem_Click(object sender, EventArgs e)
		{
			DeleteSelectedAttempts();
		}

		void AttemptDeleteButton_Click(object sender, EventArgs e)
		{
			DeleteSelectedAttempts();
		}

		protected void AttemptsGrid_DoubleClick(object sender, EventArgs e)
		{
			OpenFirstSelectedAttempt();
		}

		void AttemptEditButton_Click(object sender, EventArgs e)
		{
			OpenFirstSelectedAttempt();
		}

		void PreReqGrid_DoubleClick(object sender, EventArgs e)
		{
			if (preReqGrid.SelectedElements.IsNullOrEmpty())
			{
				return;
			}

			var controller = ZControllerFactory.Create(ControllerIDs.GlbAccreditation);
			controller.ShowEditForm(preReqGrid.SelectedElements[0]);
		}

		GlbPerson Person => attemptsGrid.DataSource as GlbPerson;
	}
}
