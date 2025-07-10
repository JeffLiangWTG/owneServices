using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business.TemporaryOrgRemover;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TemporaryOrgRemoverForm : ZChildForm
	{
		public TemporaryOrgRemoverForm(Remover removerInstance)
			: base(removerInstance)
		{
		}

		Remover Remover
		{
			get { return (Remover)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		#region Delete

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void DeleteButton_Click(object sender, EventArgs e)
		{
			DialogResult result = Globals.Message.ShowConfirmation(Res.GetString("02bf4a72-4e91-429a-ab65-c1dc36ee5736", "Are you sure you want to delete all the specified temporary organizations? This action cannot be reversed."), Res.GetString("488a68c0-5a95-4cb7-bfd1-99f856c91fef", "Delete Temporary Organizations"), Res.GetString("7e53235b-2bc7-4b14-8bbf-8f9ade952904", "To continue, type:") + " ", (NoResString)"Yes", MessageBoxIcon.Question);
			if (result == DialogResult.OK)
			{
				using (DeleteProgressForm = new ProgressForm())
				{
					IsCancelled = false;
					HadException = false;
					DeleteProgressForm.Cancelled += new EventHandler(DeleteProgressForm_Cancelled);
					DeleteProgressForm.Status = Res.GetString("65e37f48-9ce3-4464-8d51-359c708970d0", "Deleting Unused Temporary Organizations...");
					DeleteProgressForm.Show();
					Application.DoEvents();

					Remover.ProgressChanged += new Remover.ProgressChangedEventHandler(Remover_ProgressChanged);
					try
					{
						Remover.DeleteTemporaryOrgs();
						if (Remover.AnyOrgsFailedToDelete)
						{
							using (TemporaryOrgRemoverErrorsForm errorsForm = new TemporaryOrgRemoverErrorsForm(Remover))
							{
								ShowErrorsFormWihoutDispose(errorsForm);
							}
						}
						else
						{
							Globals.Message.Show(Res.GetString("671a860f-fe6c-4dfd-ae28-5015027e94cb", "Organizations Deleted Successfully"), Res.GetString("671a860f-fe6c-4dfd-ae28-5015027e94cb", "Organizations Deleted Successfully"), MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
					}
					finally
					{
						Remover.ProgressChanged -= new Remover.ProgressChangedEventHandler(Remover_ProgressChanged);
						DeleteProgressForm.Close();
						if (!IsCancelled && !HadException)
						{
							Close();
						}
					}
				}
			}
		}

		protected virtual void ShowErrorsFormWihoutDispose(TemporaryOrgRemoverErrorsForm errorsForm)
		{
			ZFormModaliser.ShowDialogWithoutDispose(errorsForm);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		bool Remover_ProgressChanged(int percentComplete)
		{
			DeleteProgressForm.PercentComplete = percentComplete;
			Application.DoEvents();

			return !IsCancelled;
		}

		void DeleteProgressForm_Cancelled(object sender, EventArgs e)
		{
			IsCancelled = true;
		}

		bool IsCancelled;
		bool HadException;
		ZLabel zLabel1;
		ProgressForm DeleteProgressForm;

		#endregion

		#region Close

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
