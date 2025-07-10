using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.ModuleRegistration;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageManagers;
using Enterprise.Customs.ZA.Business.MessagingProcess;
using Enterprise.Customs.ZA.GUI.MessagingProcess;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set
			{
				var oldValue = Declaration;
				base.Declaration = value;
				if (oldValue != value || value == null || !value.JE_JS.IsEmpty)
				{
					RemovePortMessgingMenuItems();
					RemoveRefundMessagingDA66DA63MenuItems();
				}
			}
		}

		protected ZMenuItem refundMessagingDA66DA63MenuItem;
		protected ZMenuItem applicationForRefundda66DA63MenuItem;
		protected ZMenuItem allocateEntryInstructionsMenuItem;
		protected ZMenuItem sendToCustomsMenuItem;
		protected ZMenuItem sendSupportingDocsMenuItem;
		protected ZMenuItem cancelPendingSubmissionsMenuItem;
		protected ZMenuItem sendToCustomsPOCMenuItem;

		protected override void SetupTopLevelMenu()
		{
			allocateEntryInstructionsMenuItem = new ZMenuItem(AllocateEntryInstructionsMenuText, new EventHandler(AllocateEntryInstructions_Click));
			MenuItems.Add(allocateEntryInstructionsMenuItem);

			sendToCustomsMenuItem = new ZMenuItem(SendToCustomsMenuText, SendToCustomsMenu_Click);
			sendToCustomsMenuItem.Visible = false;
			MenuItems.Add(sendToCustomsMenuItem);

			sendSupportingDocsMenuItem = new ZMenuItem(SendSupportingDocsText, SendSupportingDocsMenu_Click);
			sendSupportingDocsMenuItem.Visible = false;
			MenuItems.Add(sendSupportingDocsMenuItem);

			cancelPendingSubmissionsMenuItem = new ZMenuItem(CancelPendingSubmissionsMenuText, CancelPendingSubmissionsMenu_Click);
			cancelPendingSubmissionsMenuItem.Visible = false;
			MenuItems.Add(cancelPendingSubmissionsMenuItem);

			sendToCustomsPOCMenuItem = new ZMenuItem(SendToCustomsPOCMenuText, SendToCustomsPOCMenu_Click);
			sendToCustomsPOCMenuItem.Visible = false;
			MenuItems.Add(sendToCustomsPOCMenuItem);
		}

		protected override Customs.GUI.BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter supporter)
		{
			var entry = supporter as CusEntryHeader;
			if (entry == null)
			{
				ErrorReporter.ReportOnce("For ZA, supporter must be CusEntryHeader");
				return null;
			}
			else
			{
				return new Customs.GUI.EntryBondedWarehouseOperationDeterminer(entry);
			}
		}

		#region Menu Visibility

		public override void RefreshMenu()
		{
			base.RefreshMenu();

			var declaration = Declaration;
			var shouldSendMessage = declaration != null && !declaration.IsDeclarationIntegrated && !declaration.IsImportByExternalBroker;
			sendToCustomsMenuItem.Visible = shouldSendMessage;
			sendSupportingDocsMenuItem.Visible = shouldSendMessage;
			// Temporary functionality for developer/product testing requires users with limited permissions
			sendToCustomsPOCMenuItem.Visible = shouldSendMessage && MessagingPOCHelper.IsPOCActive;

			var canCancelDeferredMessages = declaration != null && SubmissionHelper.CanCancelDeferredMessages;
			cancelPendingSubmissionsMenuItem.Visible = canCancelDeferredMessages;
			GenerateEntriesMenuItem.Enabled = !canCancelDeferredMessages;
			sendToCustomsMenuItem.Enabled = !canCancelDeferredMessages && HasPermissionToSendToCustoms(declaration);
			sendToCustomsPOCMenuItem.Enabled = !canCancelDeferredMessages && HasPermissionToSendToCustoms(declaration);
			if (declaration != null && declaration.JE_JS.IsEmpty)
			{
				SetupPortMessagingMenuItems();
			}

			if ((string)Declaration?.JE_MessageType == ZAJobMessageTypeList.Codes.Export)
			{
				SetupRefundMessagingDA66DA63MenuItems();
			}
			else
			{
				RemoveRefundMessagingDA66DA63MenuItems();
			}

			allocateEntryInstructionsMenuItem.Visible = (string)declaration?.JE_MessageType == ZAJobMessageTypeList.Codes.Import;
		}

		ZBool HasPermissionToSendToCustoms(JobDeclaration jobDeclaration)
		{
			return jobDeclaration != null
				&& (jobDeclaration.IsImport ? Env.Security.ImportMessaging.IsAllowed : Env.Security.ExportMessaging.IsAllowed);
		}

		protected override bool DisplayGenerateEntriesMenuOption => true;

		#endregion

		#region Cargo Dues Order Menu Items
		protected MenuItem portMessagingMenuItem;
		public static ZGuid CargoDuesOrderImportZAPK => new ZGuid("194564db-3a01-4f99-ba97-db81088dd321");
		public static ZGuid CargoDuesOrderExportZAPK => new ZGuid("339d3688-72bc-4fba-a7fc-9f925e26aef9");
		public static ZGuid CargoDuesOrderLoadCoastwiseZAPK => new ZGuid("a08a3adc-19f9-4314-b820-725b7d028d4b");
		public static ZGuid CargoDuesOrderDischargeCoastwiseZAPK => new ZGuid("c6aa04a2-1508-4270-8b78-9f0b1117eee2");

		void SetupPortMessagingMenuItems()
		{
			if (portMessagingMenuItem == null)
			{
				portMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("241a3584-abcf-4575-96aa-a81847e43ea4", "Port Messaging"));
				portMessagingMenuItem.AddFormsMenuItems(
					Declaration,
					CustomsModuleIDs.JobDeclaration,
					CreateCargoDuesMenuItemInfos()
				);
				MenuItems.Add(portMessagingMenuItem);
			}
		}

		void RemovePortMessgingMenuItems()
		{
			if (portMessagingMenuItem != null)
			{
				MenuItems.Remove(portMessagingMenuItem);
				portMessagingMenuItem.Dispose();
				portMessagingMenuItem = null;
			}
		}

		IEnumerable<IMenuItemInfo> CreateCargoDuesMenuItemInfos()
		{
			return new[]
			{
				new ParentMenuItemInfo
				{
					Name = ResString.GetMultilingualString("5144B2B2-41CA-4BF6-90E5-5DA838183632", "Cargo Dues Order (ZA)"),
					SubMenus = new[]
					{
						new SystemMenuItemInfo
						{
							ID = CargoDuesOrderImportZAPK
						},
						new SystemMenuItemInfo
						{
							ID = CargoDuesOrderExportZAPK
						},
						new SystemMenuItemInfo
						{
							ID = CargoDuesOrderLoadCoastwiseZAPK
						},
						new SystemMenuItemInfo
						{
							ID = CargoDuesOrderDischargeCoastwiseZAPK
						}
					}
				}
			};
		}
		#endregion

		#region Supporting Docs Items

		static string SendSupportingDocsText => ResString.GetMultilingualString("99c81132-1337-4a6d-8604-15051723dae7", "Send Supporting Documents");

		void SendSupportingDocsMenu_Click(object sender, EventArgs e)
		{
			if (PreSaveDeclaration(Declaration) && ValidateSupportingDocs())
			{
				var decWrapper = new Customs.Business.JobDeclarationSupportingDocSendingObjectParent(Declaration);
				using (var form = new SupportingDocSendingForm(decWrapper))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						new Business.MessageManagers.DocumentSending.SupportingDocSendingManager(decWrapper, new MessageNotificationCollector()).SendMessages();
					}
				}
			}
		}

		bool ValidateSupportingDocs()
		{
			ZStringBuilder result = new ZStringBuilder();
			if (Declaration.AgentCode.IsEmpty)
			{
				result.Append(ValidationConstants.SupportingDocSendingManager.EmptyTradingPartyIDError + "\r\n");
			}

			if (result.Length > 0)
			{
				Globals.Message.ShowError(result.ToString());
			}

			return result.Length == 0;
		}

		#endregion

		#region Message Menu Items

		static string SendToCustomsMenuText => ResString.GetMultilingualString("57ABC7F8-BFED-479E-9465-DE1C5F630473", "Send to Customs");

		void SendToCustomsMenu_Click(object sender, EventArgs e)
		{
			if (PreSaveDeclaration(Declaration))
			{
				if (Declaration.HasEntryLineNumberExceedingMax)
				{
					Globals.Message.Show(cannotSendExceedsMaxEntryNumber, "Cannot Send Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					var errorMsg = Declaration.GetPreviousEntryLineNumbersMinMaxValueMessageError();
					if (!errorMsg.IsEmpty)
					{
						Globals.Message.Show(errorMsg, "Cannot Send Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					else
					{
						bool continueWithSend = false;
						var decWrapper = new JobDeclarationMessageSendingObjectParent(Declaration);
						using (var form = new MessageSendingForm(decWrapper))
						{
							continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
						}

						if (continueWithSend && ShowDeferralOptions(decWrapper))
						{
							var messageManager = new MessageManager(decWrapper, new MessageNotificationCollector());
							var form = Form;
							using (form != null ? ZFormPostingButtonsStrategy.DeferredUpdateSaveButtonsBasedOnHasChanges(form) : null)
							{
								messageManager.SendMessages();
							}
						}
					}
				}
			}
		}

		readonly string cannotSendExceedsMaxEntryNumber = ValidationConstants.Shared.MessageCannotBeSent + "\r\n\r\n" + ValidationConstants.Shared.EntryLineNumberExceedMax;

		bool ShowDeferralOptions(JobDeclarationMessageSendingObjectParent decWrapper)
		{
			bool continueWithSend = true;

			if (decWrapper.HasDutiableSendingObject)
			{
				SubmissionHelper.RefreshData();
				if (SubmissionHelper.CanAlterMessageSubmitDateOrPaymentDetails)
				{
					var submission = new DeferredSubmission(SubmissionHelper);
					using (var deferredSubmissionForm = new DeferredSubmissionForm(submission))
					{
						continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(deferredSubmissionForm) == DialogResult.OK;
					}

					if (continueWithSend)
					{
						submission.UpdateDeclaration();
						decWrapper.UpdateFromDeferredSubmission(submission);
					}
				}
			}
			return continueWithSend;
		}

		static string CancelPendingSubmissionsMenuText => ResString.GetMultilingualString("AB57CAF8-0B45-42CC-B21D-1A94131149B2", "Cancel Pending Submissions");

		void CancelPendingSubmissionsMenu_Click(object sender, EventArgs e)
		{
			var cancelledMessagesCount = SubmissionHelper.CancelDeferredMessages();
			Declaration.Factory.Save();
			Globals.Message.Show(Res.GetString("185F95DA-7F46-4738-A811-4E936EDCFA95", "{0} Pending Submissions were Canceled.", cancelledMessagesCount));
		}

		DeferredSubmissionHelper SubmissionHelper => fSubmissionHelper ?? (fSubmissionHelper = new DeferredSubmissionHelper(Declaration));
		DeferredSubmissionHelper fSubmissionHelper;

		static string SendToCustomsPOCMenuText => "Send to Customs POC";

		void SendToCustomsPOCMenu_Click(object sender, EventArgs e)
		{
			ZACustomsMessagingGui.SendToCustoms(Declaration, Form);
		}

		public static string RefundMessagingDA66DA63MenuText => Res.GetString("DB83A1A1-6859-48F3-9D88-578542AC318B", "Refund Messaging (DA63)");
		public static string ApplicationForRefundDA66DA63MenuText => Res.GetString("E96F3073-E989-4B6D-BEE9-BF4222B788FB", "Application for Refund (DA63)");
		public static ZGuid ApplicationForRefundDA66DA63StmMenuItemPK => new ZGuid("364cd085-60b5-4435-bd82-bae8fe999cc6"); // GUID from documents.xml: <SI_SU>b5e73bf1-a512-41e7-87e4-4bb471bbf8ea</SI_SU>

		void SetupRefundMessagingDA66DA63MenuItems()
		{
			if (refundMessagingDA66DA63MenuItem == null)
			{
				refundMessagingDA66DA63MenuItem = new ZMenuItem(RefundMessagingDA66DA63MenuText);
				refundMessagingDA66DA63MenuItem.AddFormsMenuItems(
					Declaration,
					CustomsModuleIDs.JobDeclaration,
					CreateRefundMessagingDA66DA63Infos()
				);
				MenuItems.Add(refundMessagingDA66DA63MenuItem);
			}
		}

		void RemoveRefundMessagingDA66DA63MenuItems()
		{
			if (refundMessagingDA66DA63MenuItem != null)
			{
				MenuItems.Remove(refundMessagingDA66DA63MenuItem);
				refundMessagingDA66DA63MenuItem.Dispose();
				refundMessagingDA66DA63MenuItem = null;
			}
		}

		IEnumerable<IMenuItemInfo> CreateRefundMessagingDA66DA63Infos()
		{
			return new[]
			{
				new SystemMenuItemInfo
				{
					ID = ApplicationForRefundDA66DA63StmMenuItemPK
				}
			};
		}

		#endregion

		internal static string AllocateEntryInstructionsMenuText => ResString.GetMultilingualString("C4116CD0-315F-4D88-978A-28F074308849", "Allocate Inv. Lines to Entry Inst.");

		void AllocateEntryInstructions_Click(object sender, EventArgs e)
		{
			var overwrite = false;
			using (var form = new AllocateEntryInstructionsPopupForm(Declaration.HasAnyInvoiceLinesLinkedToEntryInstructions))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) != DialogResult.OK || (Declaration.HasAnyInvoiceLinesLinkedToEntryInstructions && form.AllowOverwrite == null))
				{
					return;
				}
				overwrite = Declaration.HasAnyInvoiceLinesLinkedToEntryInstructions && form.AllowOverwrite.Value;
			}
			Declaration.AutoCreateEntryInstructions(overwrite);
		}
	}
}
