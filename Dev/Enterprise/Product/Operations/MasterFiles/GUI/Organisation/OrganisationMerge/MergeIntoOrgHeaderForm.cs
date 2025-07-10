using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.OrgMergeBillingConstants;
using BillingAction = Enterprise.MasterFiles.Business.OrgMergeBillingConstants.Action;
using IFavoriteProvider = Enterprise.Core.Modules.IFavoriteProvider;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressBindingMemberBashingTest] // for SetToInactive / DeleteOldOrg
	public partial class MergeIntoOrgHeaderForm : ZChildForm
	{
		public MergeIntoOrgHeaderForm(MergeOrgHeader mergeOrg)
			: base(mergeOrg)
		{
			InitializeComponent();
			NewOrgHeader = mergeOrg.NewOrganisation;
			NewOrgCodeTextBox.Text = mergeOrg.NewOrganisation.OH_Code;
			NewOrgNameTextBox.Text = mergeOrg.NewOrganisation.OH_FullName;
			CurrentMode = CurrentQueryMode.Name;
		}

		ZGroupBox OrgsToMergeGroupBox;
		ZGrid OldOrganisationsGrid;
		ZButton AddAnotherOldOrgButton;
		ZGuidFindBox OldOrgGuidFindBox;
		CargoWise.Windows.UI.KProgressBar ProgressMerge;
		ZDropEdit MatchThreshold;
		ZCalcEdit MaxResults;
		ZRadioButton FindByName;
		ZButton FindButton;
		ZRadioButton FindByPattern;
		ZRadioButton FindByCode;
		ZPanel PatternParamPanel;
		bool collectionChanged;

		readonly OrgHeader NewOrgHeader;

		MergeOrgHeader OrgMerge
		{
			get { return (MergeOrgHeader)BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return Res.GetString("MergeIntoOrgHeaderForm|FormVerb", "Merge"); }
		}

		protected CurrentQueryMode CurrentMode
		{
			get
			{
				return OrgMerge.CurrentMode;
			}
			set
			{
				OrgMerge.CurrentMode = value;
			}
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("42c25b2e-6460-4186-b607-ecb3d4a86b5b", "organization"), Res.GetString("78c5f092-4592-41f8-a041-aceb889dec1a", "merge"), Res.GetString("d919025c-2126-4a4f-93c2-68d1522e5a60", "merged"), includeIgnoreOption);
		}

		#region Process Transfer

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void PerformProgressBarStep()
		{
			ProgressMerge.PerformStep();
			Application.DoEvents();
			Thread.Sleep(10);
		}

		void ProcessTransfer()
		{
			try
			{
				using (new ZWaitCursorChanger())
				{
					DisableButtons(true);
					var sourceForm = Source.MergeIntoOrgHeaderForm;
					OrgMerge.ClearAllNotifications();
					OrgMerge.RunPreSaveValidation();
					if (OrgMerge.HasErrors)
					{
						ShowErrorsDialog();
						OrgMerge.RefreshBindingIncludingChildren();
						Save(new[]
						{
							new SaveInTransactionDelegateAction(Db.Connection, () =>
							{
								OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, BillingAction.MergeFailure, sourceForm, new[] { NewOrgHeader.PK.ToString(), ALLTargets, nameof(MergeResult.Failed) });
								return ChangedTableNames.Empty;
							})
						});

						return;
					}
					ProgressMerge.Value = 0;
					ProgressMerge.Maximum = OrgMerge.CurrentOrgHeaderCollection.Count * 2;

					var mergeOrgHeadersToBeProcessed = new List<MergeOrgHeader>();
					var disallowedMergeMessages = new List<string>();
					var hasErrors = false;
					var elapsedMilliseconds = 0L;
					bool mergeCancelled = false;

					MainStatusBar.Text = Res.GetString("dd418b7c-cc0f-4cd1-9829-68dc240e0f16", "Validating...");

					foreach (OrgHeader org in OrgMerge.CurrentOrgHeaderCollection)
					{
						hasErrors |= MergeHeader(mergeOrgHeadersToBeProcessed, org, ref mergeCancelled, ref disallowedMergeMessages);
						if (mergeCancelled)
						{
							break;
						}
					}

					var allDisallowed = false;
					if (hasErrors)
					{
						ShowErrorsDialog();
						OrgMerge.RefreshBindingIncludingChildren();
					}
					else if (!mergeCancelled)
					{
						if (allDisallowed = OrgMerge.CurrentOrgHeaderCollection.Count == disallowedMergeMessages.Count)
						{
							Globals.Message.Show(Res.GetString("1012784d-3e52-4684-af61-1e0384941f99", "Merge Organizations is not allowed.\r\n\r\nReasons:\r\n{0}", string.Join("\r\n", disallowedMergeMessages.ToArray())),
								Res.GetString("8e6a1a14-d571-41ed-af22-a7ff270984f2", "Merge is not allowed.") + " ", MessageBoxButtons.OK, DialogResult.OK);
						}
						else
						{
							hasErrors = DoMergeProcess(ref mergeOrgHeadersToBeProcessed, ref disallowedMergeMessages, out elapsedMilliseconds);
						}
					}

					Save(new[]
					{
						new SaveInTransactionDelegateAction(Db.Connection, () =>
						{
							if (hasErrors || allDisallowed)
							{
								OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, BillingAction.MergeFailure, sourceForm, new[] { NewOrgHeader.PK.ToString(), ALLTargets, nameof(MergeResult.Failed) });
							}
							else if (mergeCancelled)
							{
								OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, BillingAction.MergeCancel, sourceForm, new[] { NewOrgHeader.PK.ToString(), ALLTargets });
							}
							else
							{
								OrgMergeBillingCreator.CreateBillingTransaction(Db.Connection, BillingAction.MergeSuccess, sourceForm, new[] { NewOrgHeader.PK.ToString(), ALLTargets, $"{elapsedMilliseconds}ms" });
							}

							return ChangedTableNames.Empty;
						})
					});
				}
			}
			finally
			{
				DisableButtons(false);
			}
		}

		bool MergeHeader(List<MergeOrgHeader> list, OrgHeader org, ref bool mergeCancelled, ref List<string> disallowMergeMessages)
		{
			var merge = GetLocalMergeOrgHeader(org);
			merge.NewOrganisationPk = NewOrgHeader.PK;

			if (!OrganizationMergeHelper.DuplicateProductsResolved(merge))
			{
				mergeCancelled = true;
				return false;
			}

			merge.RunPreSaveValidation();

			if (merge.HasErrors)
			{
				foreach (INotification notif in merge.Notifications)
				{
					OrgMerge.AddRowError(notif.Message);
					OrgMerge.CurrentOrgHeaderCollection.ResumeValidation();
					org.AddRowError(notif.Message);
					OrgMerge.CurrentOrgHeaderCollection.SuspendValidation();
				}
			}
			else
			{
				if (OrgHeaderMergingChecker.IsAllowedToMergeOrgs(merge.NewOrganisation.PK, merge.OldOrganisation.PK, out string reasons))
				{
					list.Add(merge);
				}
				else
				{
					disallowMergeMessages.Add(merge.OldOrganisation.OH_FullName + ": " + reasons);
				}
			}
			PerformProgressBarStep();

			return merge.HasErrors;
		}

		protected virtual MergeOrgHeader GetLocalMergeOrgHeader(OrgHeader org) => new MergeOrgHeader(new BusinessObjectFactory(), org);

		[SuppressMessage("Microsoft.Design", "CA1021: AvoidOutParameters")]
		bool DoMergeProcess(ref List<MergeOrgHeader> mergeOrgHeadersToBeProcessed, ref List<string> disallowedMergeMessages, out long elapsedMilliseconds)
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			var hasError = false;
			string additionalMessages = string.Empty;
			MainStatusBar.Text = Res.GetString("ecf64b7f-5690-4558-818a-d7ab7914e053", "Merging...");

			StringCollectionX errors = new StringCollectionX();
			foreach (MergeOrgHeader merge in mergeOrgHeadersToBeProcessed)
			{
				foreach (MergeOrgAddress tmp in merge.OldOrgAddressesCollection)
				{
					foreach (MergeOrgAddress adr in OrgMerge.CurrentOrgAddressCollection)
					{
						if (tmp.OldAddressPK == adr.OldAddressPK)
						{
							tmp.Action = adr.Action;
							if (adr.Action == MergeOrgAddress.ActionMerge)
							{
								tmp.NewObjectPK = adr.NewAddressPK;
							}
							break;
						}
					}
				}
				foreach (MergeOrgContact cnt in OrgMerge.CurrentOrgContactCollection)
				{
					foreach (MergeOrgContact tmp in merge.OldOrgContactCollection)
					{
						if (tmp.OldContactPK == cnt.OldContactPK)
						{
							tmp.Action = cnt.Action;
							if (cnt.Action == MergeOrgContact.ActionMerge)
							{
								tmp.NewObjectPK = cnt.NewContactPK;
							}
						}
					}
				}

				try
				{
					Save(merge.SaveFactories);
					RemoveFromRecentItems(merge);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					hasError = true;
					errors.Add(!string.IsNullOrEmpty(merge.DeleteError) ? merge.DeleteError : ex.Message);
				}
				additionalMessages += merge.AdditionalMessages;
				PerformProgressBarStep();
			}

			stopWatch.Stop();
			elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
			if (errors.Count > 0)
			{
				Globals.Message.ShowError(string.Join("\n\r", errors.ToArray()), Res.GetString("fdcbf86c-dded-4ea7-b28d-98b11e234b73", "Failed to delete one or more old Organizations and marked them as inactive"));
			}
			else
			{
				if (disallowedMergeMessages.Any())
				{
					Globals.Message.Show(Res.GetString("3d469677-3e54-4caf-9d71-b1fdeacd6ec3",
						"Organization transferred successfully.{0}\r\n\r\nBut there are some organizations that are not allowed to merge.\r\nReasons:\r\n{1}",
						additionalMessages, string.Join("\r\n", disallowedMergeMessages.ToArray())));
				}
				else
				{
					MainStatusBar.Text = Res.GetString("282b8b21-9d46-40b7-9239-d25bd58c9cfc", "Organizations were merged successfully");
					Globals.Message.Show(Res.GetString("e5932d69-79d6-44e7-a1b2-14ae0c306079", "Organization transferred successfully.{0}", additionalMessages));
				}
			}

			Close();
			return hasError;
		}

		void RemoveFromRecentItems(MergeOrgHeader merge)
		{
			var oldOrgPK = merge.OldOrganisation.PK;
			var favoriteProvider = ObjectFactory.Get<IFavoriteProvider>();
			var loadedLink = merge.Factory.LoadTop1<StmLink>(new ZQuery(StmLinkSchema.STL_ItemPK, oldOrgPK));

			if (loadedLink != null)
			{
				favoriteProvider.RemoveFromRecentItems(new LinkWrapper(loadedLink));
			}
		}

		void DisableButtons(bool disable)
		{
			ProcessButton.Enabled = !disable;
			FindByName.Enabled = !disable;
			FindByCode.Enabled = !disable;
			FindByPattern.Enabled = !disable;
			AddAnotherOldOrgButton.Enabled = !disable;
			CancelMergeButton.Enabled = !disable;
			FindButton.Enabled = !disable;
		}

		void ProcessButton_Click(object sender, EventArgs e)
		{
			string[] codes = OrgMerge.GetOldOrganisationCodes(OrgMerge.CurrentOrgHeaderCollection).ToArray();
			if (codes.Length == 0)
			{
				return;
			}

			string codesAsLine = string.Join(", ", codes);
			if (Globals.Message.ShowConfirmation(
				string.Format(MergeConstants.GetMergeWarningManyToOne(codesAsLine, NewOrgHeader.OH_Code)),
				MergeConstants.MergeWarningTitle,
				MergeConstants.MergeConfirmationMessage, MessageBoxIcon.Warning) == DialogResult.OK)
			{
				ProcessTransfer();
			}
		}

		#endregion

		void FindSimilarOrganisations()
		{
			OrgMerge.RefreshBindingIncludingChildren();
			SetProperBinding();
			OrgMerge.PostChanges(collectionChanged);
			collectionChanged = false;
		}

		void SetProperBinding()
		{
			string oldOrgs = string.Empty;
			string oldAddresses = string.Empty;
			string oldContacts = string.Empty;
			switch (CurrentMode)
			{
				case CurrentQueryMode.Code:
					oldOrgs = "OldOrgsCollectionByCode";
					oldAddresses = "OldOrgAddressCollectionForSimilarOrgsByCode";
					oldContacts = "OldOrgContactCollectionForSimilarOrgsByCode";
					break;
				case CurrentQueryMode.Name:
					oldOrgs = "OldOrgsCollectionByName";
					oldAddresses = "OldOrgAddressCollectionForSimilarOrgsByName";
					oldContacts = "OldOrgContactCollectionForSimilarOrgsByName";
					break;
				case CurrentQueryMode.Pattern:
					oldOrgs = "OldOrgsCollectionByPattern";
					oldAddresses = "OldOrgAddressCollectionForSimilarOrgsByPattern";
					oldContacts = "OldOrgContactCollectionForSimilarOrgsByPattern";
					break;
			}
			OldOrganisationsGrid.SetDataBinding(OrgMerge, oldOrgs);
			MergeAddressesZGrid.SetDataBinding(OrgMerge, oldAddresses);
			MergeContactsZGrid.SetDataBinding(OrgMerge, oldContacts);
		}

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}

		void AddAnotherOldOrgButton_Click(object sender, EventArgs e)
		{
			OrgHeader org = Factory.Load<OrgHeader>(OrgMerge.OrganisationPkForBinding);
			if (org != null)
			{
				if (!OrgMerge.CurrentOrgHeaderCollection.Contains(org) && OrgMerge.NewOrganisationPk != org.PK)
				{
					OrgMerge.CurrentOrgHeaderCollection.Add(org);
				}
			}
		}

		void OldOrganisationsGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			DeleteCorrespondingAddressesAndContacts(e.Objects);
		}

		void DeleteCorrespondingAddressesAndContacts(IEnumerable<BusinessObject> objects)
		{
			foreach (var obj in objects)
			{
				var org = obj as OrgHeader;
				if (org != null)
				{
					List<ZGuid> pks1 = new List<ZGuid>();
					List<ZGuid> pks2 = new List<ZGuid>();
					List<ZGuid> pks3 = new List<ZGuid>();
					List<ZGuid> pks4 = new List<ZGuid>();
					List<ZGuid> pks5 = new List<ZGuid>();
					List<ZGuid> pks6 = new List<ZGuid>();
					foreach (OrgAddress adr in org.Addresses)
					{
						pks1.AddRange(GetPKsToRemove(OrgMerge.OldOrgAddressCollectionForSimilarOrgsByCode, adr));
						pks2.AddRange(GetPKsToRemove(OrgMerge.OldOrgAddressCollectionForSimilarOrgsByName, adr));
						pks3.AddRange(GetPKsToRemove(OrgMerge.OldOrgAddressCollectionForSimilarOrgsByPattern, adr));
					}
					foreach (OrgContact cnt in org.Contacts)
					{
						pks4.AddRange(GetPKsToRemove(OrgMerge.OldOrgContactCollectionForSimilarOrgsByCode, cnt));
						pks5.AddRange(GetPKsToRemove(OrgMerge.OldOrgContactCollectionForSimilarOrgsByName, cnt));
						pks6.AddRange(GetPKsToRemove(OrgMerge.OldOrgContactCollectionForSimilarOrgsByPattern, cnt));
					}
					RemoveFromCollectionByPKs(pks1, OrgMerge.OldOrgAddressCollectionForSimilarOrgsByCode);
					RemoveFromCollectionByPKs(pks2, OrgMerge.OldOrgAddressCollectionForSimilarOrgsByName);
					RemoveFromCollectionByPKs(pks3, OrgMerge.OldOrgAddressCollectionForSimilarOrgsByPattern);
					RemoveFromCollectionByPKs(pks4, OrgMerge.OldOrgContactCollectionForSimilarOrgsByCode);
					RemoveFromCollectionByPKs(pks5, OrgMerge.OldOrgContactCollectionForSimilarOrgsByName);
					RemoveFromCollectionByPKs(pks6, OrgMerge.OldOrgContactCollectionForSimilarOrgsByPattern);
					OrgMerge.RefreshBindingIncludingChildren();
				}
			}
		}

		List<ZGuid> GetPKsToRemove(BusinessObjectCollection col, BusinessObject boToCompare)
		{
			List<ZGuid> pks = new List<ZGuid>();
			foreach (IMergeOrgElement merge in col)
			{
				if (merge.OldObject.PK == boToCompare.PK)
				{
					pks.Add((merge as NonPersistentBusinessObject).PK);
				}
			}
			return pks;
		}

		void RemoveFromCollectionByPKs(List<ZGuid> pks, BusinessObjectCollection col)
		{
			foreach (ZGuid pk in pks)
			{
				col.Remove(pk);
			}
		}

		void CancelMergeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void FindByName_CheckedChanged(object sender, EventArgs e)
		{
			if (FindByName.Checked)
			{
				MatchThreshold.Enabled = false;
				CurrentMode = CurrentQueryMode.Name;
				collectionChanged = true;
			}
		}

		void FindByCode_CheckedChanged(object sender, EventArgs e)
		{
			if (FindByCode.Checked)
			{
				MatchThreshold.Enabled = false;
				CurrentMode = CurrentQueryMode.Code;
				collectionChanged = true;
			}
		}

		void FindByPattern_CheckedChanged(object sender, EventArgs e)
		{
			if (FindByPattern.Checked)
			{
				MatchThreshold.Enabled = true;
				CurrentMode = CurrentQueryMode.Pattern;
				collectionChanged = true;
			}
		}

		void FindButton_Click(object sender, EventArgs e)
		{
			using (new ZWaitCursorChanger())
			{
				FindSimilarOrganisations();
			}
		}
	}
}
