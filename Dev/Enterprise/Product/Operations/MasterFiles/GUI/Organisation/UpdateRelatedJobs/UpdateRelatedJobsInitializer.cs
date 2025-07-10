using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class UpdateRelatedJobsInitializer
	{
		protected BusinessObject sourceBizo;
		protected ZMenuItem menuItem;

		public UpdateRelatedJobsInitializer(BusinessObject sourceBizo)
		{
			Argument.NotNull(sourceBizo, "Source Bizo");
			this.sourceBizo = sourceBizo;
		}

		public void CreateUpdateRelatedJobsMenuItem(Menu parentMenu, EventHandler onClick)
		{
			Argument.NotNull(parentMenu, "Parent Menu");
			menuItem = new ZMenuItem(
				ResString.GetMultilingualString("Form|ActionMenu|UpdateRelatedJobScreeningMenu", "Update Related Job Screening Status"),
				delegate(object sender, EventArgs e)
				{
					var bizO = sourceBizo;
					if (bizO == null)
					{
						return;
					}

					if (bizO.HasChanges)
					{
						Globals.Message.Show(Res.GetString("F42C848C-78CF-42EB-94EA-BBCB64DC1ED8", "Please save the form before running this function.",
							Res.GetString("66D1F88D-9D0B-4062-8B31-B79467B1B883", "Please save the form"),
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning));
					}
					else
					{
						onClick?.Invoke(sender, e);
					}
				})
			{
				Visible = true,
				Enabled = sourceBizo.IsInDatabase
			};

			parentMenu.MenuItems.Add(menuItem);
		}
	}
}
