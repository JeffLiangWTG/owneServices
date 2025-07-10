using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GUI.PlugIn
{
	/// <summary>
	/// the first base class for AU Cargo Automation PlugIn
	/// </summary>
	public abstract class CustomsPlugIn : ZAlwaysLoadPlugIn
	{
		protected CustomsPlugIn(IBusiness hostEntity) : base(hostEntity)
		{
		}

		#region Implementation

		protected void ChangeTheVisibility()
		{
			if (!isChangeTheVisibilityInProgress)
			{
				isChangeTheVisibilityInProgress = true;
				try
				{
					ChangeTheVisibilityCore();
				}
				catch
				{
					throw;
				}
				finally
				{
					isChangeTheVisibilityInProgress = false;
				}
			}
		}
		bool isChangeTheVisibilityInProgress;

		protected abstract void ChangeTheVisibilityCore();

		protected void OnChangeTheVisibilityRequired(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}

		public static bool FormPreSaved(BusinessObject topLevelBizObj, ZForm form)
		{
			var result = true;
			if (!topLevelBizObj.IsInDatabase || topLevelBizObj.HasChanges)
			{
				var messageBoxResult = Globals.Message.Show(
					Res.GetString("DE3E78CA-8BA0-4E50-8E09-3AAFE355AF72", "The Job has not yet been saved. Do you want to save and proceed?"),
					Res.GetString("AD64CC33-053A-42F9-9E8D-A78CC002F2B8", "Save Job"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					DialogResult.Yes
				);
				if (messageBoxResult == DialogResult.Yes)
				{
					if (!Globals.IsTest)
					{
						result = form.FireSaveButton() == ContinueWithSave.Yes;
					}
					else
					{
						topLevelBizObj.Factory.Save();
					}
				}
				else
				{
					result = false;
				}
			}
			return result && topLevelBizObj.IsInDatabase && !topLevelBizObj.HasChanges;
		}

		#endregion
	}
}
