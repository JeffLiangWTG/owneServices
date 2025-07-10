using System;
using System.Windows.Forms;
using CargoWise.BrandManager;
using Enterprise.MasterFiles.Business.TemporaryOrgRemover;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class BulkTemporaryOrgRemoverGUI
	{
		public BulkTemporaryOrgRemoverGUI()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void Remove()
		{
			var remover = new Remover();
			int initialOrgsCount = remover.Organisations.Count;

			if (Globals.Message.ShowConfirmation(BulkTemporaryOrgRemoverConstants.GetBulkRemoveWarning(initialOrgsCount),
				BulkTemporaryOrgRemoverConstants.BulkRemoveWarningTitle,
				BulkTemporaryOrgRemoverConstants.BulkRemoveConfirmationMessage, MessageBoxIcon.Warning) == DialogResult.OK)
			{
				using (DeleteProgressForm = new ProgressForm() { CancelProgressButtonText = Res.GetString("a334c934-5915-4135-ab9d-de3469190077", "Stop") })
				{
					IsCancelled = false;
					DeleteProgressForm.Cancelled += new EventHandler(DeleteProgressForm_Cancelled);
					DeleteProgressForm.Status = Res.GetString("65e37f48-9ce3-4464-8d51-359c708970d0", "Deleting Unused Temporary Organizations...");
					DeleteProgressForm.Show();
					Application.DoEvents();

					remover.ProgressChanged += new Remover.ProgressChangedEventHandler(Remover_ProgressChanged);
					try
					{
						remover.BulkDeleteTemporaryOrgs();

						int remainingOrgsToProcessCount = initialOrgsCount - remover.OrgsDeletedCount - remover.FailedOrganisations.Count;
						Globals.Message.Show(Res.GetString("12d6657b-f831-4d7d-836f-cb30b907ddb7", @"There were {0} unused temporary organizations to delete:
- {1} were successfully deleted.
- {2} could not be deleted because some parts of {3} still reference them.
- {4} still need to be processed.", initialOrgsCount, remover.OrgsDeletedCount, remover.FailedOrganisations.Count, BrandingFactory.Instance.ProductName, remainingOrgsToProcessCount), Res.GetString("c4fd6897-e35d-405d-be09-6eb742746aed", "Unused temporary organizations deletion summary"), MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					finally
					{
						remover.ProgressChanged -= new Remover.ProgressChangedEventHandler(Remover_ProgressChanged);
						DeleteProgressForm.Close();
					}
				}
			}
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
		ProgressForm DeleteProgressForm;
	}
}
