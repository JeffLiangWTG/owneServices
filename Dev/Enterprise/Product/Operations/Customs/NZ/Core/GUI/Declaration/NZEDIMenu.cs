using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MessageBuilders;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	internal class NZEDIMenu : EDIMenu, IConsolidatedDeclarationMenuBuilder
	{
		public NZEDIMenu()
		{
			Text = "&Brokerage";
		}

		protected ZMenuItem separator1;
		protected ZMenuItem submitJobMenuItem;
		protected ZMenuItem cancelJobMenuItem;
		protected ZMenuItem resetToOriginalMenuItem;
		protected ZMenuItem tSWSubmitWithDetails;
		protected ZMenuItem tSWQueueForManifesting;
		protected ZMenuItem tSWSeparator;

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();

			separator1 = new ZMenuItem();
			separator1.Text = "-";
			MenuItems.Add(separator1);

			submitJobMenuItem = new ZMenuItem(ResString.GetMultilingualString("0291BB42-BD9C-4AAB-A725-017EAA436CA8", "&Submit Job"));
			submitJobMenuItem.Click += SubmitJobMenuItem_Click;
			MenuItems.Add(submitJobMenuItem);

			tSWQueueForManifesting = new ZMenuItem("&Queue for Manifesting", QueueJobMenuItem_Click);
			MenuItems.Add(tSWQueueForManifesting);

			tSWSubmitWithDetails = new ZMenuItem("&Submit with Details", SubmitWithDetailsMenuItem_Click);
			MenuItems.Add(tSWSubmitWithDetails);

			tSWSeparator = new ZMenuItem();
			tSWSeparator.Text = "-";
			MenuItems.Add(tSWSeparator);

			cancelJobMenuItem = new ZMenuItem(ResString.GetMultilingualString("2CB046E0-B4E4-4663-9DBC-A4E348BEC7B8", "&Cancel Job"));
			cancelJobMenuItem.Click += CancelJobMenuItem_Click;
			MenuItems.Add(cancelJobMenuItem);

			resetToOriginalMenuItem = new ZMenuItem(ResString.GetMultilingualString("6344ED27-51AA-4CAB-AB90-595E9D4B800E", "&Reset to Original"));
			resetToOriginalMenuItem.Click += ResetToOriginalMenuItem_Click;
			MenuItems.Add(resetToOriginalMenuItem);

			MenuItem separator2 = new ZMenuItem();
			separator2.Text = "-";
			MenuItems.Add(separator2);

			AddCustomsWebSiteMenuItems();
			AddBiosecurityWebSiteMenuItems();
			AddMAFWebSiteMenuItems();
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();

			var isCRE = false;
			var isICR = false;
			var isTSWDeclaration = false;
			var isAirFreight = false;
			var showSubmitCancelResetJobMenus = false;
			var enableSubmitCancelJobMenus = false;
			var enableResetJobMenu = false;

			if (Declaration != null && !Declaration.IsInterface)
			{
				isCRE = Declaration.IsTSWCREWriteOff;
				isICR = Declaration.IsTSWICRWriteOff;
				isTSWDeclaration = Declaration.IsTSWDeclaration;
				isAirFreight = Declaration.IsAir;

				showSubmitCancelResetJobMenus = true;
				enableSubmitCancelJobMenus = !Declaration.IsTSWCancelledOrPendingCancellation && IsSubmitEnabledOnConsolidatedDeclaration;
				enableResetJobMenu = enableSubmitCancelJobMenus &&
									!Declaration.ConsolidatedEntryProvider.IsQueuedForConsolidation &&
									!Declaration.ConsolidatedEntryProvider.IsConsolidated;
			}

			tSWSubmitWithDetails.Visible = isCRE;
			tSWQueueForManifesting.Visible = (isCRE || isICR) && isAirFreight;
			tSWSeparator.Visible = isTSWDeclaration;

			submitJobMenuItem.Visible = cancelJobMenuItem.Visible = resetToOriginalMenuItem.Visible = separator1.Visible = showSubmitCancelResetJobMenus;
			submitJobMenuItem.Enabled = cancelJobMenuItem.Enabled = enableSubmitCancelJobMenus;
			resetToOriginalMenuItem.Enabled = enableResetJobMenu;
		}

		bool submit;
		bool cancel;
		bool reset;

		void SaveMenuItemsState()
		{
			submit = submitJobMenuItem.Enabled;
			cancel = cancelJobMenuItem.Enabled;
			reset = resetToOriginalMenuItem.Enabled;
		}

		void ResetMenuItemsState()
		{
			submitJobMenuItem.Enabled = submit;
			cancelJobMenuItem.Enabled = cancel;
			resetToOriginalMenuItem.Enabled = reset;
		}

		protected override bool DisplayGenerateEntriesMenuOption
		{
			get { return true; }
		}

		#region CustomsWebSiteMenuItems
		void AddCustomsWebSiteMenuItems()
		{
			customsWebSiteMenuGroup = new ZMenuItem(DescriptionCustomsWebSite);
			MenuItems.Add(customsWebSiteMenuGroup);

			customsHomePageMenuItem = new ZMenuItem(DescriptionCustomsHomePage);
			customsHomePageMenuItem.Click += new EventHandler(CustomsHomePageMenuItem_Click);
			customsWebSiteMenuGroup.MenuItems.Add(customsHomePageMenuItem);

			customsFindVesselOrFlightMenuItem = new ZMenuItem(DescriptionCustomsFindVesselOrFlight);
			customsFindVesselOrFlightMenuItem.Click += new EventHandler(CustomsFindVesselOrFlightMenuItem_Click);
			customsWebSiteMenuGroup.MenuItems.Add(customsFindVesselOrFlightMenuItem);
		}

		void CustomsHomePageMenuItem_Click(object sender, EventArgs e)
		{
			OpenURL(UrlCustomsHomePage);
		}

		void CustomsFindVesselOrFlightMenuItem_Click(object sender, EventArgs e)
		{
			OpenURL(UrlCustomsFindVesselOrFlight);
		}

		protected MenuItem customsWebSiteMenuGroup;
		protected MenuItem customsHomePageMenuItem;
		protected MenuItem customsFindVesselOrFlightMenuItem;
		#endregion
		public const string DescriptionCustomsWebSite = "Customs Website";
		public const string DescriptionCustomsHomePage = "Home Page";
		const string UrlCustomsHomePage = "http://www.customs.govt.nz";
		public const string DescriptionCustomsFindVesselOrFlight = "Find Vessel/Flight No";
		public const string UrlCustomsFindVesselOrFlight = "https://www.customs.govt.nz/business/import/lodge-your-import-entry/craft-names-and-flight-numbers/";

		#region BiosecurityWebSiteMenuItems
		void AddBiosecurityWebSiteMenuItems()
		{
			biosecurityWebSiteMenuGroup = new ZMenuItem(DescriptionBiosecurityWebSite);
			MenuItems.Add(biosecurityWebSiteMenuGroup);

			biosecurityHomePageMenuItem = new ZMenuItem(DescriptionBiosecurityHomePage);
			biosecurityHomePageMenuItem.Click += new EventHandler(BiosecurityHomePageMenuItem_Click);
			biosecurityWebSiteMenuGroup.MenuItems.Add(biosecurityHomePageMenuItem);

			biosecurityComtainerRequirementsMenuItem = new ZMenuItem(DescriptionBiosecurityContainerRequirements);
			biosecurityComtainerRequirementsMenuItem.Click += new EventHandler(BiosecurityComtainerRequirementsMenuItem_Click);
			biosecurityWebSiteMenuGroup.MenuItems.Add(biosecurityComtainerRequirementsMenuItem);
		}

		void BiosecurityHomePageMenuItem_Click(object sender, EventArgs e)
		{
			OpenURL(UrlBiosecurityHomepage);
		}

		void BiosecurityComtainerRequirementsMenuItem_Click(object sender, EventArgs e)
		{
			OpenURL(UrlBiosecurityContainerRequirements);
		}

		protected MenuItem biosecurityWebSiteMenuGroup;
		protected MenuItem biosecurityHomePageMenuItem;
		protected MenuItem biosecurityComtainerRequirementsMenuItem;
		#endregion
		public const string DescriptionBiosecurityWebSite = "Biosecurity Website";
		public const string DescriptionBiosecurityHomePage = "Home Page";
		const string UrlBiosecurityHomepage = "http://www.biosecurity.govt.nz";
		public const string DescriptionBiosecurityContainerRequirements = "Container Requirements";
		const string UrlBiosecurityContainerRequirements = "http://www.biosecurity.govt.nz/regs/cont-carg";

		#region MAFWebSiteMenuItems
		void AddMAFWebSiteMenuItems()
		{
			mAFWebSiteMenuGroup = new ZMenuItem(DescriptionMAFWebSite);
			MenuItems.Add(mAFWebSiteMenuGroup);

			mAFHomePageMenuItem = new ZMenuItem(DescriptionMAFHomePage);
			mAFHomePageMenuItem.Click += new EventHandler(MAFHomePageMenuItem_Click);
			mAFWebSiteMenuGroup.MenuItems.Add(mAFHomePageMenuItem);

			mAFQuarrantineCargoInfoMenuItem = new ZMenuItem(DescriptionMAFQuarrantineCargoInfo);
			mAFQuarrantineCargoInfoMenuItem.Click += new EventHandler(MAFQuarrantineCargoInfoMenuItem_Click);
			mAFWebSiteMenuGroup.MenuItems.Add(mAFQuarrantineCargoInfoMenuItem);
		}

		void MAFHomePageMenuItem_Click(object sender, EventArgs e)
		{
			OpenURL(UrlMAFHomePage);
		}

		void MAFQuarrantineCargoInfoMenuItem_Click(object sender, EventArgs e)
		{
			OpenURL(UrlMAFQuarrantineCargoInfo);
		}

		protected MenuItem mAFWebSiteMenuGroup;
		protected MenuItem mAFHomePageMenuItem;
		protected MenuItem mAFQuarrantineCargoInfoMenuItem;
		#endregion
		public const string DescriptionMAFWebSite = "MPI Website";
		public const string DescriptionMAFHomePage = "Home Page";
		const string UrlMAFHomePage = "http://www.mpi.govt.nz";
		public const string DescriptionMAFQuarrantineCargoInfo = "ATF Information";
		const string UrlMAFQuarrantineCargoInfo = "http://containerchecks.maf.govt.nz/ATFlist.aspx";

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		protected void SubmitJobMenuItem_Click(object sender, EventArgs e)
		{
			SaveMenuItemsState();
			SetupConsolidatedEntryIfNecessary();
			if (ValidateDeclaration())
			{
				if (CheckConsolidatedDeclarationForOtherUsers_OKToProceed((ZMenuItem)sender))
				{
					if (IsManifestedEntry)
					{
						Globals.Message.Show(CannotSubmitManifest);
					}
					else if (ConsolidatedDeclaration != null || !IsQueuedForConsolidation((ZMenuItem)sender))
					{
						using (var gUIManager = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>())
						{
							gUIManager.SetCaption("Submit Message - Credit Restriction");
							gUIManager.Initialise(Declaration);
							CallMessageSendingForm(MessageManager.OperationType.SubmitMessage);
						}
					}
				}
				else
				{
					ResetMenuItemsState();
				}
			}
		}

		protected void QueueJobMenuItem_Click(object sender, EventArgs e)
		{
			if (ValidateDeclaration())
			{
				if (IsManifestedEntry)
				{
					Globals.Message.Show(CannotQueueManifest);
				}
				else if (!IsQueuedForConsolidation((ZMenuItem)sender))
				{
					using (var gUIManager = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>())
					{
						gUIManager.SetCaption("Submit Message - Credit Restriction");
						gUIManager.Initialise(Declaration);
						CallMessageSendingForm(MessageManager.OperationType.SubmitMessage, queueForManifesting: true);
					}
				}
			}
		}

		protected void CancelJobMenuItem_Click(object sender, EventArgs e)
		{
			SaveMenuItemsState();
			SetupConsolidatedEntryIfNecessary();
			if (ValidateDeclaration())
			{
				if (CheckConsolidatedDeclarationForOtherUsers_OKToProceed((ZMenuItem)sender))
				{
					if (IsManifestedEntry)
					{
						Globals.Message.Show(CannotCancelManifest);
					}
					else
					{
						CallMessageSendingForm(MessageManager.OperationType.CancelMessage);
					}
				}
				else
				{
					ResetMenuItemsState();
				}
			}
		}

		protected void SubmitWithDetailsMenuItem_Click(object sender, EventArgs e)
		{
			if (ValidateDeclaration())
			{
				if (IsManifestedEntry)
				{
					Globals.Message.Show(CannotSubmitManifest);
				}
				else if (!IsQueuedForConsolidation((ZMenuItem)sender))
				{
					using (var gUIManager = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>())
					{
						gUIManager.SetCaption("Submit Message - Credit Restriction");
						gUIManager.Initialise(Declaration);
						CallMessageSendingForm(MessageManager.OperationType.SubmitMessage, verbose: true, attachments: true);
					}
				}
			}
		}

		protected void ResetToOriginalMenuItem_Click(object sender, EventArgs e)
		{
			SaveMenuItemsState();
			SetupConsolidatedEntryIfNecessary();
			if (ValidateDeclaration())
			{
				if (CheckConsolidatedDeclarationForOtherUsers_OKToProceed((ZMenuItem)sender))
				{
					if (IsManifestedEntry)
					{
						Globals.Message.Show(CannotResetManifest);
					}
					else
					{
						var messagingManager = new MessagingFunctionalityManager(Declaration);
						messagingManager.OnSuccessfullyExecuted += () => ConsolidatedDeclaration?.ImportAggregateDeclaration(Declaration);
						messagingManager.ResetToOriginal();
					}
				}
				else
				{
					ResetMenuItemsState();
				}
			}
		}

		protected bool IsManifestedEntry
		{
			get
			{
				var result = false;
				var entryHeader = Declaration?.CusEntryHeader;
				if (entryHeader != null)
				{
					result = entryHeader.CH_MessageType.ToString() == Customs.Business.CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest
						  && entryHeader.CH_EntryStatus != LowValueManifestStatusList.Codes.NotSentToCustoms
						  && entryHeader.CH_EntryStatus != LowValueManifestStatusList.Codes.SentToCustoms;
				}

				return result;
			}
		}

		bool ValidateDeclaration()
		{
			bool result = true;
			if (Declaration == null)
			{
				Globals.Message.Show(ErrorMessageDeclarationIsRequiredForThisJob);
				result = false;
			}
			else if ((ConsolidatedDeclaration != null && ConsolidatedDeclaration.HasChanges) || (ConsolidatedDeclaration == null && Declaration.HasChanges) || (Declaration.Shipment != null && Declaration.Shipment.HasChanges))
			{
				Globals.Message.Show(ErrorMessageDeclarationMustBeSavedFirst);
				result = false;
			}
			else if (Declaration.Shipment != null &&
					 Declaration.Shipment.Job != null &&
					 Declaration.Shipment.Job.JH_GB.IsEmpty)
			{
				Globals.Message.Show(ErrorMessageDeclarationBranchNotNull);
				result = false;
			}
			else if (Declaration.IsTSWDeclaration || Declaration.IsFormalEntry)
			{
				result = ValidateAgentAndBrokerDetails();
			}

			return result;
		}

		public const string ErrorMessageDeclarationIsRequiredForThisJob = "Could not find the Declaration for this messaging operation.";
		internal const string ErrorMessageDeclarationBranchNotNull = "The Invoice must have a proper Branch.";
		internal const string ErrorMessageDeclarationMustBeSavedFirst = "You must save the current Declaration details before generating a message.";
		internal const string CannotResetManifest = "You cannot reset a declaration entry to original once it has been Manifested";
		internal const string CannotSubmitManifest = "You cannot submit a declaration entry as a stand-alone Brokerage entry once it has been Manifested.\r\n\r\nOnce any relevant changes have been made and saved to the declaration here,\r\nyou need to action (submit) this job via the Manifest menu options.";
		internal const string CannotCancelManifest = "You cannot cancel a declaration entry as a stand-alone Brokerage entry once it has been Manifested.\r\nYou need to action (cancel) this job via the Manifest menu options.";
		internal const string CannotQueueManifest = "This declaration has already been Manifested.\r\nYou need to take any further action on this job via the Manifest menu options.";

		bool ValidateAgentAndBrokerDetails()
		{
			var brokerErrors = new BrokerValidation(Declarant).ValidateAgentAndBrokerDetails(checkDeclarantId: !Declaration.IsPrimaryIndustriesImportDeclaration);
			if (brokerErrors != "")
			{
				Globals.Message.Show(brokerErrors, "Cannot Send Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return brokerErrors.Length == 0;
		}

		IDeclarant Declarant
		{
			get { return new TSWGlbStaffWrapper(GlbStaff.CurrentUser); }
		}

		protected override ConsolidatedEntryMenuProvider GetConsolidatedEntryMenuProvider() => new NZConsolidatedEntryMenuProvider(Form);

		bool CallMessageSendingForm(MessageManager.OperationType operationType, bool verbose = false, bool attachments = false, bool queueForManifesting = false)
		{
			var parentForm = (ZForm)GetMainMenu()?.GetForm();
			var messagingManager = new MessagingFunctionalityManager(Declaration);
			messagingManager.OnSuccessfullyExecuted += () => ConsolidatedDeclaration?.ImportAggregateDeclaration(Declaration);
			return messagingManager.ShowSubmitToCustomsForm(operationType, parentForm, verbose, attachments, queueForManifesting, sendingTSWManifests: false);
		}

		void OpenURL(string url)
		{
			WebUrlLauncher.Launch(url);
		}

		#region GetDataTransferImpl

		protected override DataTransfer.DataTransferImpl GetDataTransferImpl()
		{
			return new Business.Data.FlatFileImporter.DataTransferImplementation();
		}

		#endregion

		#region Consolidated Declaration

		ZMenuItem IConsolidatedDeclarationMenuBuilder.BuildMenu()
		{
			var result = new ZMenuItem(ResString.GetMultilingualString("4E4615A5-06F7-457A-980A-2B84FE82448B", "&Brokerage"));
			result.MenuItems.Add(submitJobMenuItem);
			result.MenuItems.Add(cancelJobMenuItem);
			result.MenuItems.Add(resetToOriginalMenuItem);
			return result;
		}

		public Customs.Business.ConsolidatedDeclaration ConsolidatedDeclaration { get; set; }

		void SetupConsolidatedEntryIfNecessary()
		{
			if (ConsolidatedDeclaration != null)
			{
				Declaration = (JobDeclaration)ConsolidatedDeclaration.BuildAggregateJobDeclaration();
			}
		}

		#endregion
	}
}
