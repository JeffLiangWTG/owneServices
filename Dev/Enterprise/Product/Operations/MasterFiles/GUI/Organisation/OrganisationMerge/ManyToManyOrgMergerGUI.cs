using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class ManyToManyOrgMergerGUI
	{
		public ManyToManyOrgMergerGUI(ZQuery exportQuery)
		{
			ExportQuery = exportQuery;
		}

		protected ZQuery ExportQuery;

		public List<string> Merge()
		{
			List<string> errors = new List<string>();
			ExportQuery.OrderBy = Enterprise.ZArchitecture.Schema.OrgHeaderSchema.OH_Code.Name;
			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(ExportQuery, typeof(OrgHeader));
			int approximateCount = reader.ApproximateCount;
			if (reader != null)
			{
				reader.BatchSize = 100 > approximateCount ? (approximateCount / 4 > 1 ? approximateCount / 4 : 1) : 100;
				if (Globals.Message.ShowConfirmation(MergeConstants.MergeWarningManyToMany,
					MergeConstants.MergeWarningTitle,
					MergeConstants.MergeConfirmationMessage, MessageBoxIcon.Warning) == DialogResult.OK)
				{
					using (BulkMergeProgressForm = new ProgressForm())
					{
						ManyToManyOrgMerger merger = new ManyToManyOrgMerger(reader);
						merger.BatchCompleted += new ManyToManyEventHandler(merger_BatchCompleted);

						BulkMergeProgressForm.Cancelled += delegate
						{ merger.Cancel(); };
						BulkMergeProgressForm.Show();
						PerformStep(0, 0, approximateCount, 0);

						errors = MergeSelectedOrgsCore(reader, merger);
						if (errors.Count > 0)
						{
							Globals.Message.ShowError(string.Join("\n\r", errors.ToArray()), Res.GetString("c9b9076a-da92-498f-966a-240a8dcf2fb6", "Failed to delete one or more old Organizations and marked them as inactive."));
						}
						else
						{
							TimeSpan ts = merger.EndDateTime - merger.StartDateTime;
							string tsString = (ts.Days * 24 + ts.Hours).ToString() + ":" + ts.Minutes.ToString() + ":" + ts.Seconds.ToString();
							string processed = "\r\n" + Res.GetString("899fbfec-5b4e-460f-948e-48e4ad136b1a", "Number of Organizations processed is {0}.\r\nTime taken: {1}", merger.OrganisationsProcessed, tsString);
							string disallowed = merger.DisallowedMergeMessages.Count > 0 ?
								"\r\n\r\n" + Res.GetString("73535b19-d8fb-46e8-80cb-f8ac5aa2afb5", "Number of Organizations that are not allowed to merge is {0}.\r\nReasons:\r\n{1}",
								merger.DisallowedMergeMessages.Count, string.Join("\r\n", merger.DisallowedMergeMessages)) : string.Empty;
							Globals.Message.Show(
								(merger.Cancelled
									? Res.GetString("5d4f9352-e91e-4ab8-8c79-577065f3d33e", "Operation canceled by the user.")
									: Res.GetString("14ab6ef0-5615-4000-b913-a18373eba762", "Organization transferred successfully.")
								) + processed + disallowed);
						}
					}
				}
			}

			return errors;
		}

		protected virtual List<string> MergeSelectedOrgsCore(FilteredBusinessObjectReader reader, ManyToManyOrgMerger merger)
		{
			return merger.Merge();
		}

		protected ProgressForm BulkMergeProgressForm { get; set; }

		void merger_BatchCompleted(object sender, ManyToManyBatchPrcessedEvent e)
		{
			ManyToManyOrgMerger merger = sender as ManyToManyOrgMerger;
			if (BulkMergeProgressForm != null && merger != null)
			{
				PerformStep(e.PercentCompleted, merger.OrganisationsProcessed, merger.ApproximateCount, merger.OrganisationsDeleted);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void PerformStep(int percent, int orgsProcessed, int maxCount, int orgsDeleted)
		{
			BulkMergeProgressForm.SetStatusAndPercentComplete(Res.GetString("9abcb7ba-4322-4353-a46e-0889b9ba2b0c", "Merging... {0} of {1}.\r\nOrganizations deleted: {2}", orgsProcessed + 1, maxCount, orgsDeleted), percent);
			System.Threading.Thread.Sleep(10);
			Application.DoEvents();
		}
	}
}
