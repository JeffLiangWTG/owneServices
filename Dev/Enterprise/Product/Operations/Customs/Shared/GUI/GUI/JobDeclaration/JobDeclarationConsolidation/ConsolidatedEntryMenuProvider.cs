using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data.Utils;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public class ConsolidatedEntryMenuProvider
	{
		public ConsolidatedEntryMenuProvider(ZForm parentForm)
		{
			this.parentForm = parentForm;
		}

		protected BaseJobDeclaration declaration;
		JobDeclarationConsolidatedEntryProvider consolidationProvider;
		readonly ZForm parentForm;
		ZMenuItem consolidatedEntryMenuItem;
		ZMenuItem queueForConsolidationMenuItem;
		ZMenuItem dequeueFromConsolidationMenuItem;
		ZMenuItem createConsolidatedEntryMenuItem;
		ZMenuItem linkToConsolidationDeclaration;

		bool ConsolidatedEntriesEnabled => RawDataRegistry.Instance.EnableConsolidatedEntries.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);

		public IEnumerable<ZMenuItem> CreateMenuEntries()
		{
			if (ConsolidatedEntriesEnabled)
			{
				consolidatedEntryMenuItem = new ZMenuItem(ConsolidatedEntryCaption);
				consolidatedEntryMenuItem.Name = nameof(consolidatedEntryMenuItem);
				consolidatedEntryMenuItem.Visible = false;

				queueForConsolidationMenuItem = new ZMenuItem(QueueForConsolidationCaption, QueueForConsolidationMenuItem_Click);
				queueForConsolidationMenuItem.Name = nameof(queueForConsolidationMenuItem);
				queueForConsolidationMenuItem.Visible = false;

				dequeueFromConsolidationMenuItem = new ZMenuItem(DequeueFromConsolidationCaption, DequeueFromConsolidationMenuItem_Click);
				dequeueFromConsolidationMenuItem.Name = nameof(dequeueFromConsolidationMenuItem);
				dequeueFromConsolidationMenuItem.Visible = false;

				createConsolidatedEntryMenuItem = new ZMenuItem(CreateConsolidatedEntryCaption, CreateConsolidatedEntry_Click);
				createConsolidatedEntryMenuItem.Name = nameof(createConsolidatedEntryMenuItem);
				createConsolidatedEntryMenuItem.Visible = false;

				linkToConsolidationDeclaration = new ZMenuItem(OpenConsolidatedCaption, LinkToConsolidationDeclaration_Click);
				linkToConsolidationDeclaration.Name = nameof(linkToConsolidationDeclaration);
				linkToConsolidationDeclaration.Visible = false;

				consolidatedEntryMenuItem.MenuItems.Add(queueForConsolidationMenuItem);
				consolidatedEntryMenuItem.MenuItems.Add(dequeueFromConsolidationMenuItem);
				consolidatedEntryMenuItem.MenuItems.Add(createConsolidatedEntryMenuItem);
				consolidatedEntryMenuItem.MenuItems.Add(linkToConsolidationDeclaration);

				yield return consolidatedEntryMenuItem;
			}
		}

		public void RefreshMenu(BaseJobDeclaration baseDeclaration)
		{
			declaration = baseDeclaration;
			this.consolidationProvider = declaration?.ConsolidatedEntryProvider;
			bool menuItemsVisible = consolidationProvider != null && MenuItemsVisible;

			if (consolidatedEntryMenuItem != null)
			{
				consolidatedEntryMenuItem.Visible = menuItemsVisible;
			}

			if (queueForConsolidationMenuItem != null)
			{
				queueForConsolidationMenuItem.Visible = menuItemsVisible;
				queueForConsolidationMenuItem.Enabled = menuItemsVisible && consolidationProvider.CanConsolidateEntry && !consolidationProvider.IsConsolidated;
			}

			if (dequeueFromConsolidationMenuItem != null)
			{
				dequeueFromConsolidationMenuItem.Visible = menuItemsVisible;
				dequeueFromConsolidationMenuItem.Enabled = menuItemsVisible && (consolidationProvider.IsQueuedForConsolidation || consolidationProvider.CanRemove);
			}

			if (createConsolidatedEntryMenuItem != null && declaration != null)
			{
				createConsolidatedEntryMenuItem.Visible = menuItemsVisible;
				createConsolidatedEntryMenuItem.Enabled = menuItemsVisible && consolidationProvider.IsQueuedForConsolidation && !ConsolidatedDeclaration.IsConsolidated(declaration);
			}

			if (linkToConsolidationDeclaration != null && declaration != null && ConsolidatedDeclaration.IsConsolidated(declaration))
			{
				linkToConsolidationDeclaration.Visible = menuItemsVisible;
				linkToConsolidationDeclaration.Enabled = menuItemsVisible;
			}
		}

		protected virtual bool MenuItemsVisible => ConsolidatedEntriesEnabled && declaration != null && !declaration.IsInterface;

		public bool SubmitMessageMenuEnabled => !ConsolidatedEntriesEnabled || consolidationProvider == null || !consolidationProvider.IsConsolidated;

		public bool IsQueuedForConsolidation(ZMenuItem menuItem)
		{
			var result = false;

			if (ConsolidatedEntriesEnabled)
			{
				var caption = menuItem.Text.Replace("&", "");

				var gotLock = WithConsolidationLock(caption, () =>
				{
					if (result = consolidationProvider.IsQueuedForConsolidation)
					{
						if (Globals.Message.Show(CannotSubmitQueuedDeclaration, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
						{
							if (consolidationProvider.DequeueOrRemoveFromConsolidation(out _))
							{
								result = false;
							}
						}
					}
					else if (result = consolidationProvider.IsConsolidated)
					{
						Globals.Message.ShowError(CannotSubmitConsolidatedDeclaration, caption);
					}
				});

				if (!gotLock)
				{
					result = true;
				}
			}

			return result;
		}

		#region Implementation

		void QueueForConsolidationMenuItem_Click(object sender, EventArgs e)
		{
			if (CheckIfDeclarationExists())
			{
				var caption = QueueForConsolidationCaption.Replace("&", "");
				consolidationProvider.QueueForConsolidation(out var resultMessage);
				if (!string.IsNullOrEmpty(resultMessage))
				{
					Globals.Message.ShowInformation(resultMessage, caption);
				}
			}
		}

		void DequeueFromConsolidationMenuItem_Click(object sender, EventArgs e)
		{
			if (CheckIfDeclarationExists())
			{
				var caption = DequeueFromConsolidationCaption.Replace("&", "");

				consolidationProvider.DequeueOrRemoveFromConsolidation(out var resultMessage);
				if (!string.IsNullOrEmpty(resultMessage))
				{
					Globals.Message.ShowInformation(resultMessage, caption);
				}
			}
		}

		void CreateConsolidatedEntry_Click(object sender, EventArgs e)
		{
			if (CheckIfDeclarationExists())
			{
				var consolidatedDeclaration = ConsolidatedDeclaration.GetConsolidatedDeclaration(declaration);
				if (consolidatedDeclaration == null)
				{
					var jobDeclarationModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration) as ZFilterGridModule;
					jobDeclarationModule.DoNotShowRecentItems = true;
					jobDeclarationModule.DoNotCheckOrSaveChanges = true;
					jobDeclarationModule.FilterBusinessObject.ParentModuleID = ModuleIDs.Customs.ConsolidatedDeclaration;

					SetExternalDefaults(jobDeclarationModule);

					var form = new NewOrAttachConsolidatedDeclarationForm(null, jobDeclarationModule, false, declaration);
					ZFormModaliser.Show(form, parentForm);
				}
			}
		}

		protected virtual void SetExternalDefaults(ZFilterGridModule filterGridModule)
		{
		}

		void LinkToConsolidationDeclaration_Click(object sender, EventArgs e)
		{
			var consolidatedDeclaration = ConsolidatedDeclaration.GetConsolidatedDeclaration(declaration);
			if (consolidatedDeclaration != null)
			{
				ZControllerFactory.Create(ControllerIDs.Customs.ConsolidatedDeclaration).ShowEditForm(consolidatedDeclaration);
			} 
		}

		bool CheckIfDeclarationExists()
		{
			var result = true;
			if (declaration == null)
			{
				Globals.Message.Show(ErrorMessageDeclarationIsRequiredForThisJob);
				result = false;
			}

			return result;
		}

		protected virtual bool MergeDeclarationCore()
		{
			var isMerged = declaration.CustomsEntryHeaders.Any() && !declaration.MergeManager.RequiresMerge;
			return isMerged || declaration.DoMerge();
		}

		protected virtual bool ValidateDeclarationCore()
		{
			var validator = Customs.Business.MessageSendingValidation.New(declaration, null);
			return validator.CheckBusinessObjectLevelValidation(declaration.MessageInitiator);
		}

		bool WithConsolidationLock(string caption, Action consolidationFunction)
		{
			var result = false;

			if (consolidationProvider != null)
			{
				using (var sqlLock = LockDeclarationForConsolidation())
				{
					if (sqlLock?.IsHoldingLock() ?? false)
					{
						consolidationFunction.Invoke();
						result = true;
					}
					else
					{
						Globals.Message.ShowError(consolidationProvider.CannotDetermineConsolidatedDeclarationStatus, caption);
					}
				}
			}

			return result;
		}

		protected virtual SqlApplicationLock LockDeclarationForConsolidation() => consolidationProvider?.LockDeclarationForConsolidation();

		#endregion

		string ConsolidatedEntryCaption => Res.GetString("412C5261-86AC-4FF4-B7CA-AB60F9D0D54A", "&Consolidated Entry");
		string QueueForConsolidationCaption => Res.GetString("31B32B4B-F71D-4777-B991-891BBE78FE10", "&Queue for Consolidation");
		string DequeueFromConsolidationCaption => Res.GetString("69DD1FF9-18FC-4A83-8683-6BD2BAACCAAA", "&Dequeue/Remove from Consolidation");
		string OpenConsolidatedCaption => Res.GetString("2779635E-487D-41F3-95D4-5B043484D583", "&Open Consolidation Entry");
		string CreateConsolidatedEntryCaption => Res.GetString("55BC7076-7345-4EE7-9CA4-6681071E9502", "&Create Consolidated Entry");
		string CannotSubmitQueuedDeclaration => Res.GetString("3A3E8A99-4E1A-4C15-AA9D-715C4BCEBE95", "This entry is currently queued to be linked to a Consolidated Entry. Continuing to submit this entry will remove it from the queue.\r\nDo you wish to continue to submit this entry?");
		string CannotSubmitConsolidatedDeclaration => Res.GetString("9D930F1D-97D5-4EB3-9B3A-709ABE7C24D5", "This entry has been linked to a Consolidated Entry and cannot be submitted individually.");
		string ErrorMessageDeclarationIsRequiredForThisJob => Res.GetString("7F8A6801-9ACA-41B6-BEC3-35971B3C5796", "Could not find the Declaration for this messaging operation.");
	}
}
