using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public enum MergeResult
	{
		Success,
		Failed,
		Cancelled,
		MergedWithErrors,
		FailedWithCriticalError
	}

	public static class OrganizationMergeHelper
	{
		static string MergeIsNotAllowedCaption => Res.GetString("8e6a1a14-d571-41ed-af22-a7ff270984f2", "Merge is not allowed.") + " ";

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static MergeResult Merge(MergeOrgHeader mergeData, out long elapsedMilliseconds)
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			var result = MergeResult.Failed;
			string caption;

			if (mergeData.OldOrganisation.IsDeleted)
			{
				caption = MergeIsNotAllowedCaption;
				stopWatch.Stop();
				elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
				Globals.Message.Show(Res.GetString("CB645661-4BFB-498B-B762-0D155D1000A9", "Merge Organizations is not allowed.\r\n\r\nReasons:\r\nDissolved Organization had been deleted already."), caption, MessageBoxButtons.OK, DialogResult.OK);
				return result;
			}

			stopWatch.Stop();
			elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
			var confirmationResult = Globals.Message.ShowConfirmation(
				string.Format(CultureInfo.InvariantCulture, MergeConstants.GetMergeWarningSingle(mergeData.OldOrganisationCode, mergeData.NewOrganisationCode)),
				MergeConstants.MergeWarningTitle,
				MergeConstants.MergeConfirmationMessage, MessageBoxIcon.Warning);

			if (confirmationResult == DialogResult.OK)
			{
				if (!DuplicateProductsResolved(mergeData))
				{
					return MergeResult.Cancelled;
				}

				stopWatch.Start();
				mergeData.RunPreSaveValidation();
				if (mergeData.HasErrors)
				{
					stopWatch.Stop();
					elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
					ShowErrorsDialog(mergeData);
				}
				else
				{
					if (OrgHeaderMergingChecker.IsAllowedToMergeOrgs(mergeData.NewOrganisation.PK, mergeData.OldOrganisation.PK, out var error))
					{
						using (ZFormStrategy.SuppresseNewFormInTransactionWarning())
						{
							var progressForm = new ProgressForm(Res.GetString("7B0A1958-D088-4A9E-B4B3-9BFE95823827", "Merging Organization"));

							progressForm.Show(progressForm.ParentForm);
							progressForm.ShowCancelButton = false;

							error = ProcessTransfer(mergeData);

							progressForm.Close();
							progressForm = null;
						}

						stopWatch.Stop();
						elapsedMilliseconds = stopWatch.ElapsedMilliseconds;

						if (string.IsNullOrEmpty(error))
						{
							caption = Res.GetString("0F4F4BDC-E366-4A08-8855-BBFD5439C2D2", "Merge Successful.") + " ";
							Globals.Message.Show(Res.GetString("d15b2f6e-1fc3-485b-978f-ad02fe4ca146", "Organization transferred successfully.\r\n\r\n{0}", mergeData.AdditionalMessages), caption, MessageBoxButtons.OK, DialogResult.OK);
							result = MergeResult.Success;
						}
						else if (mergeData.ConcurrencyException != null)
						{
							var isOrganizationDeleted =
								mergeData.OldOrganisation == null ||
								mergeData.OldOrganisation.IsDeleted ||
								mergeData.NewOrganisation == null ||
								mergeData.NewOrganisation.IsDeleted;

							if (!isOrganizationDeleted)
							{
								ZExceptionReporting.HandleSaveException(mergeData.ConcurrencyException);
							}
						}
						else if (mergeData.AddOverlappingDatesException != null)
						{
							caption = Res.GetString("74579886-1971-425d-9a96-a207f65db7f8", "Failed to merge Organizations.") + " ";
							Globals.Message.Show(Res.GetString("1dfd895c-6c1a-43ba-9c4d-162394bf2ed5", @"It was not possible to complete the merge operation you requested because the organizations have periodic invoices with overlapping dates. It is possible to deactivate one of the organizations, however these two organizations cannot be merged."), caption, MessageBoxButtons.OK, DialogResult.OK);
						}
						else
						{
							caption = Res.GetString("74579886-1971-425d-9a97-a207f65db7f8", "Failed to delete old Organization and marked it as inactive.") + " ";
							if (error == mergeData.DeleteError)
							{
								Globals.Message.Show(Res.GetString("7dfd895c-6c9a-43ba-9c4d-162394bf2ed5", "Organizations were merged normally, but old Organization could not be deleted and was marked as inactive.\r\n\r\nReason:\r\n\r\n{0}", error), caption, MessageBoxButtons.OK, DialogResult.OK);
								result = MergeResult.MergedWithErrors;
							}
							else
							{
								Globals.Message.ShowError(error, caption);
								result = MergeResult.FailedWithCriticalError;
							}
						}
					}
					else
					{
						stopWatch.Stop();
						elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
						caption = MergeIsNotAllowedCaption;
						Globals.Message.Show(Res.GetString("1012784d-3e52-4684-af61-1e0384941f99", "Merge Organizations is not allowed.\r\n\r\nReasons:\r\n{0}", error), caption, MessageBoxButtons.OK, DialogResult.OK);
					}
				}
			}
			else
			{
				result = MergeResult.Cancelled;
			}

			return result;
		}

		public static bool DuplicateProductsResolved(MergeOrgHeader mergeData)
		{
			var merger = new ProductOrgMerger(mergeData);
			if (merger.TotalDuplicateCount == 0)
			{
				return true;
			}
			using (var mergerForm = new ProductOrgMergerForm(merger))
			{
				return ZFormModaliser.ShowDialogWithoutDispose(mergerForm) == DialogResult.OK;
			}
		}

		static string ProcessTransfer(MergeOrgHeader mergeData)
		{
			try
			{
				var oldOrgPK = mergeData.OldOrganisation.PK;
				var factories = mergeData.SaveFactories.Where(x => x is OrganisationMerger).Cast<OrganisationMerger>().ToArray();
				var favoriteProvider = ObjectFactory.Get<IFavoriteProvider>();

				foreach (OrganisationMerger merger in factories)
				{
					merger.ActionOnSave = OrganisationMergerActionOnSave.MergeOnly;
				}

				BusinessObjectFactory.SaveTogether(mergeData.SaveFactories);

				foreach (OrganisationMerger merger in factories)
				{
					merger.ActionOnSave = OrganisationMergerActionOnSave.DeleteOnly;
				}

				BusinessObjectFactory.SaveTogether(mergeData.SaveFactories);

				var loadedLink = mergeData.Factory.LoadTop1<StmLink>(new ZQuery(StmLinkSchema.STL_ItemPK, oldOrgPK));

				if (loadedLink != null)
				{
					favoriteProvider.RemoveFromRecentItems(new LinkWrapper(loadedLink));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!string.IsNullOrEmpty(mergeData.DeleteError))
				{
					return mergeData.DeleteError;
				}

				if (mergeData.ConcurrencyException != null)
				{
					return mergeData.ConcurrencyException.Message;
				}

				if (mergeData.AddOverlappingDatesException != null)
				{
					return mergeData.AddOverlappingDatesException.Message;
				}

				if (mergeData.ConflictingStorageDocsPKException != null)
				{
					return mergeData.ConflictingStorageDocsPKException.Message;
				}

				ErrorReporter.ReportOnce("MergeOrgHeaderForm.ProcessTransfer", "Exception happened while saving organizations merge results: " + ex.Message, ex);
				return ex.Message;
			}

			return string.Empty;
		}

		static void ShowErrorsDialog(IBusiness businessObject)
		{
			using (ZMessageBox msgBox = new ZErrorMessageBox(businessObject))
			{
				ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
			}
		}
	}
}
