using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MessageBuilders;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.GUI.TradeSingleWindow;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using SubmitToCustomsForm = Enterprise.Customs.NZ.GUI.Base.SubmitToCustomsForm;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public class MessagingFunctionalityManager
	{
		public MessagingFunctionalityManager(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration declaration;

		public bool ShowSubmitToCustomsForm(MessageManager.OperationType operationType, ZForm parentForm, bool verbose, bool attachments)
		{
			return ShowSubmitToCustomsForm(operationType, parentForm, verbose, attachments, false, false);
		}

		public bool ShowSubmitToCustomsForm(MessageManager.OperationType operationType, ZForm parentForm, bool verbose, bool attachments, bool sendingTSWManifests)
		{
			return ShowSubmitToCustomsForm(operationType, parentForm, verbose, attachments, false, sendingTSWManifests);
		}

		public bool ShowSubmitToCustomsForm(MessageManager.OperationType operationType, ZForm parentForm, bool verbose, bool attachments, bool queueForManifest, bool sendingTSWManifests)
		{
			bool messageSubmitted = false;
			var entryTypeWrapper = EntryTypeWrapper.Instance(declaration);
			if (entryTypeWrapper != null)
			{
				var sendType = operationType == MessageManager.OperationType.CancelMessage ? TSWTransactionTypes.Cancel : CreateOrReplaceTransaction;
				var additionalMessageInformation = new AdditionalMessageInformation(null, declaration.eDocsForSelection, sendType, declaration.Factory, "");
				additionalMessageInformation.AM_QueueForManifesting = queueForManifest;
				additionalMessageInformation.GatherPotentialSupportingDocuments();
				var messageManager = entryTypeWrapper.GetNewMessageManager(operationType, additionalMessageInformation);
				if (messageManager != null)
				{
					bool isOkToExecute = messageManager.IsOkToExecute;
					if (!isOkToExecute && messageManager.MessageIsCurrentlyQueued)
					{
						if (Globals.Message.Show(messageManager.MessageIsCurrentlyQueuedShouldWeCancelQuestion, messageManager.HumanReadableOperationType, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No)
							== DialogResult.Yes)
						{
							messageManager.CancelCurrentlyQueuedMessage();
							isOkToExecute = messageManager.IsOkToExecute;
						}
					}

					if (!isOkToExecute)
					{
						if (!string.IsNullOrEmpty(messageManager.LastHumanReadableStatus))
						{
							Globals.Message.ShowError(messageManager.LastHumanReadableStatus, messageManager.HumanReadableOperationType);
						}
					}
					else if (entryTypeWrapper.DeclarationIsOkToSend(messageManager))
					{
						if (SendingWithDeferredCutoffOkay && entryTypeWrapper.CheckIfFreightOkay(operationType))
						{
							if (!string.IsNullOrEmpty(messageManager.LastHumanReadableWarning))
							{
								if (Globals.Message.Show(messageManager.LastHumanReadableWarning, "Service Task Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
								{
									return false;
								}
							}
							var continueSend = true;
							if (operationType == MessageManager.OperationType.SubmitMessage)
							{
								if (sendingTSWManifests)
								{
									continueSend = IsCreditCheckOKToSend();
								}
								else
								{
									var documentDeliveryCreditControlManager = new DocumentDeliveryCreditControlManager();
									documentDeliveryCreditControlManager.IsCustomsSubmission = true;
									var failureReason = documentDeliveryCreditControlManager.GetDocumentDeliveryStatusForCreditManagement(declaration, "message", ZGuid.Empty);
									continueSend = failureReason.IsEmpty;
								}
							}

							if (continueSend)
							{
								DialogResult dialogResult;
								if (sendingTSWManifests)
								{
									additionalMessageInformation.AM_SendManifest = true;
									dialogResult = SubmitTSWManifest(sendType, messageManager, additionalMessageInformation, parentForm);
									additionalMessageInformation.AM_FreeText = messageManager.EnteredRemarks;
								}
								else if (declaration.IsCRE && !attachments)
								{
									dialogResult = SubmitCREWithoutAttachments(operationType, sendType, additionalMessageInformation, parentForm, verbose);
								}
								else
								{
									dialogResult = SubmitTSWEntry(operationType, sendType, additionalMessageInformation, parentForm);
								}

								if (dialogResult == DialogResult.OK)
								{
									messageSubmitted = messageManager.Execute();
									if (messageSubmitted)
									{
										OnSuccessfullyExecuted?.Invoke();
										Globals.Message.ShowInformation(messageManager.LastHumanReadableStatus, messageManager.HumanReadableOperationType);
									}
									else
									{
										Globals.Message.ShowError(messageManager.LastHumanReadableStatus, messageManager.HumanReadableOperationType);
									}
								}
								else if (dialogResult == DialogResult.Cancel)
								{
									Globals.Message.ShowWarning(Res.GetString("Enterprise.Customs.NZ.GUI.Declaration.MessagingFunctionalityManager|SubmitMessageCancelled",
										"Submit Message canceled."));
								}
							}
						}
					}
				}
			}

			return messageSubmitted;
		}

		protected bool IsCreditCheckOKToSend()
		{
			var helper = new Customs.Business.MessageManagerCreditCheckWithSecurityHelper(declaration);
			if (!helper.IsCreditCheckOKToSend)
			{
				declaration.MessageInitiator.WarnUserAboutSomething(helper.ReasonForNotAllowed, Res.GetString("67FB7046-4B9B-4BDC-89A9-13F03E9A97D0", "Send Manifest"));
				return false;
			}

			return true;
		}

		DialogResult SubmitTSWManifest(TSWTransactionTypes sendType, MessageManagerForDeclaration messageManager1, AdditionalMessageInformation additionalMessageInformation, ZForm parentForm)
		{
			DialogResult dialogResult = DialogResult.Cancel;

			if (sendType == TSWTransactionTypes.Original || sendType == TSWTransactionTypes.Cancel)
			{
				var messageManager = (Business.MessageBuilders.ECIWriteOff.Manifesting.MessageManager)messageManager1;
				using (var submitForm = new ECIWriteOff.Manifesting.SubmitToCustomsForm(messageManager))
				{
					dialogResult = Globals.IsTest ? DialogResult.OK : submitForm.ShowDialog();
				}
			}
			else
			{
				using (var tswForm = new TSWReplaceForm(additionalMessageInformation))
				{
					dialogResult = Globals.IsTest ? DialogResult.OK : tswForm.ShowDialog(parentForm);
				}
			}

			return dialogResult;
		}

		DialogResult SubmitCREWithoutAttachments(MessageManager.OperationType operationType, TSWTransactionTypes sendType, AdditionalMessageInformation additionalMessageInformation, ZForm parentForm, bool verbose)
		{
			DialogResult dialogResult;
			if (operationType == MessageManager.OperationType.CancelMessage)
			{
				using (var tswForm = new TSWCancelForm(additionalMessageInformation))
				{
					dialogResult = Globals.IsTest ? DialogResult.OK : tswForm.ShowDialog(parentForm);
				}
			}
			else
			{
				if (sendType == TSWTransactionTypes.Original)
				{
					if (verbose)
					{
						using (var tswForm = new CREOriginalForm(additionalMessageInformation))
						{
							dialogResult = Globals.IsTest ? DialogResult.OK : tswForm.ShowDialog(parentForm);
						}
					}
					else
					{
						dialogResult = DialogResult.OK;
					}
				}
				else
				{
					if (verbose)
					{
						using (var tswForm = new CREReplaceWithCommentsForm(additionalMessageInformation))
						{
							dialogResult = Globals.IsTest ? DialogResult.OK : tswForm.ShowDialog(parentForm);
						}
					}
					else
					{
						using (var tswForm = new CREReplaceForm(additionalMessageInformation))
						{
							dialogResult = Globals.IsTest ? DialogResult.OK : tswForm.ShowDialog(parentForm);
						}
					}
				}
			}

			return dialogResult;
		}

		DialogResult SubmitTSWEntry(MessageManager.OperationType operationType, TSWTransactionTypes sendType, AdditionalMessageInformation additionalMessageInformation, ZForm parentForm)
		{
			DialogResult dialogResult;
			if (operationType == MessageManager.OperationType.CancelMessage)
			{
				using (var tswForm = new TSWCancelForm(additionalMessageInformation))
				{
					dialogResult = Globals.IsTest ? DialogResult.OK : tswForm.ShowDialog(parentForm);
				}
			}
			else
			{
				if (sendType == TSWTransactionTypes.Original)
				{
					using (var tswForm = new TSWSendFormWithAttachments(additionalMessageInformation))
					{
						dialogResult = Globals.IsTest ? DialogResult.OK : tswForm.ShowDialog(parentForm);
					}
				}
				else
				{
					using (var tswForm = new TSWReplaceForm(additionalMessageInformation))
					{
						dialogResult = Globals.IsTest ? DialogResult.OK : tswForm.ShowDialog(parentForm);
					}
				}
			}

			return dialogResult;
		}

		bool SendingWithDeferredCutoffOkay
		{
			get
			{
				bool result = true;
				ZString cutoffDateWarningMessage = new BrokerDeferredCutOffDateCalculator(declaration).GetWarningMessageIfWarningRequired();
				if (!cutoffDateWarningMessage.IsEmpty)
				{
					DialogResult dialog = Globals.Message.Show(cutoffDateWarningMessage, "Broker Deferred Cutoff Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					result = (dialog == DialogResult.Yes);
				}
				return result;
			}
		}

		public void ResetToOriginal()
		{
			if (!Env.Security.CustomsResetToOriginal.IsAllowed)
			{
				Env.Security.ShowError(Env.Security.CustomsResetToOriginal);
			}
			else
			{
				if (declaration.MessageInitiator.ShowUserConfirmation(
					@"Are you sure you want to Reset to Original?
This action is intended as a 'last resort' that should only be used
under instruction from WiseTech Global staff.
There is no way to undo this operation.", "Reset to Original", "If you are absolutely sure you want to Reset to Original, please type: ", "Reset To Original"))
				{
					EntryTypeWrapper typeWrapper = EntryTypeWrapper.Instance(declaration);
					MessageManagerForDeclaration messageManager = typeWrapper.GetNewMessageManager(MessageManagerForDeclaration.OperationType.ResetToOriginal);
					if (messageManager.IsOkToExecute)
					{
						if (messageManager.Execute())
						{
							OnSuccessfullyExecuted?.Invoke();
							Globals.Message.ShowInformation(messageManager.LastHumanReadableStatus, messageManager.HumanReadableOperationType);
						}
					}
					else
					{
						Globals.Message.ShowError(messageManager.LastHumanReadableStatus, messageManager.HumanReadableOperationType);
					}
				}
			}
		}
		internal event Action OnSuccessfullyExecuted;

		TSWTransactionTypes CreateOrReplaceTransaction
		{
			get
			{
				var entryNumber = declaration.EntryHeaderForOriginalEntryNumber != null ? declaration.EntryHeaderForOriginalEntryNumber.EntryNumber : declaration.JE_OriginalEntryNumber;
				if (declaration.IsCompletion)
				{
					entryNumber = declaration.CompletionEntryNumber;
				}
				else if (declaration.IsECIWriteoff)
				{
					entryNumber = declaration.CusEntryHeader.EntryNumber;
				}
				else if (declaration.IsPrimaryIndustriesImportDeclaration)
				{
					entryNumber = declaration.EntryHeaderForIPIEntryNumber != null ? declaration.EntryHeaderForIPIEntryNumber.EntryNumber : ZString.Empty;
				}

				return entryNumber.IsEmpty ? TSWTransactionTypes.Original : TSWTransactionTypes.Replace;
			}
		}

		#region EntryTypeWrapper

		abstract class EntryTypeWrapper
		{
			public static EntryTypeWrapper Instance(JobDeclaration declaration)
			{
				if (declaration != null)
				{
					if (declaration.IsFormalEntry)
					{
						return new FormalEntryWrapper(declaration);
					}
					else if (declaration.IsECIManifestDeclarationReference)
					{
						return new ECIWriteOffManifestingWrapper(declaration);
					}
					else
					{
						return new ECIWriteOffWrapper(declaration);
					}
				}
				return null;
			}

			#region CheckIfFreightOkay

			protected internal virtual bool CheckIfFreightOkay(MessageManager.OperationType operationType)
			{
				return true;
			}

			#endregion

			protected EntryTypeWrapper(JobDeclaration declaration)
			{
				this.declaration = declaration;
			}
			protected JobDeclaration declaration;
			public abstract MessageManagerForDeclaration GetNewMessageManager(MessageManager.OperationType operationType);
			public abstract MessageManagerForDeclaration GetNewMessageManager(MessageManager.OperationType operationType, AdditionalMessageInformation additionalMessageInformation);
			public abstract SubmitToCustomsForm GetNewSubmitToCustomsForm(MessageManagerForDeclaration messageManager);
			public abstract bool DeclarationIsOkToSend(MessageManagerForDeclaration messageManager);
		}

		#region FormalEntryWrapper
		class FormalEntryWrapper : EntryTypeWrapper
		{
			public FormalEntryWrapper(JobDeclaration declaration) : base(declaration) { }

			public override MessageManagerForDeclaration GetNewMessageManager(MessageManager.OperationType operationType)
			{
				return new Business.MessageBuilders.FormalEntry.MessageManager(declaration, operationType);
			}

			protected internal override bool CheckIfFreightOkay(MessageManager.OperationType operationType)
			{
				bool result = true;
				Business.Declaration.FormalEntry.CusEntryHeader header = declaration.CusEntryHeader as Business.Declaration.FormalEntry.CusEntryHeader;
				if (operationType != MessageManager.OperationType.CancelMessage && !declaration.IsPrimaryIndustriesImportDeclaration && header != null && header.EntryLinesExistWithoutFreight)
				{
					DialogResult dialog = Globals.Message.Show(NoFreightMessage, "Entry Line Found With No Freight", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
					result = (dialog == DialogResult.Yes);
					if (result)
					{
						declaration.Logs.AddNew(Events.DeclarationHasErrors, "Entry Line Found With No Freight - User Sent Anyway");
					}
				}

				return result;
			}

			const string NoFreightMessage = @"Not all Entry Lines being sent to Customs have Freight on them. 

This can either be because the value of the line is so low that there is no freight apportioned against this line, or that no Freight has been entered against the Declaration.

Are you sure you want to submit this Declaration without Freight?";

			public override SubmitToCustomsForm GetNewSubmitToCustomsForm(MessageManagerForDeclaration messageManager)
			{
				return new FormalEntry.SubmitToCustomsForm((Business.MessageBuilders.FormalEntry.MessageManager)messageManager);
			}

			public override bool DeclarationIsOkToSend(MessageManagerForDeclaration messageManager)
			{
				bool result = true;
				if (declaration.MergeManager.RequiresMerge || declaration.CusEntryHeader.MergedLines.Count == 0 || declaration.CusEntryHeader.CH_EntryChargeWaived)
				{
					declaration.CusEntryHeader.CH_EntryChargeWaived = false;
					result = declaration.DoMerge(new SendsMessagesToCustomsGUI());
				}

				if (result)
				{
					using (declaration.IsTSWDeclaration ? declaration.TemporarilySetTSWMessagingValidation() : DisposableAction.NoAction)
					{
						result = messageManager.IsOKToSendWithMessagingErrors();
					}
				}
				return result;
			}

			public override MessageManagerForDeclaration GetNewMessageManager(MessageManager.OperationType operationType, AdditionalMessageInformation additionalMessageInformation)
			{
				return new Business.MessageBuilders.FormalEntry.MessageManager(declaration, operationType, additionalMessageInformation);
			}
		}
		#endregion

		#region ECIWriteOffWrapper
		class ECIWriteOffWrapper : EntryTypeWrapper
		{
			public ECIWriteOffWrapper(JobDeclaration declaration) : base(declaration) { }

			public override MessageManagerForDeclaration GetNewMessageManager(MessageManager.OperationType operationType)
			{
				return new Business.MessageBuilders.ECIWriteOff.MessageManager(declaration, operationType);
			}

			public override SubmitToCustomsForm GetNewSubmitToCustomsForm(MessageManagerForDeclaration messageManager)
			{
				return new ECIWriteOff.SubmitToCustomsForm((Business.MessageBuilders.ECIWriteOff.MessageManager)messageManager);
			}

			public override bool DeclarationIsOkToSend(MessageManagerForDeclaration messageManager)
			{
				return messageManager.IsOKToSendWithMessagingErrors();
			}

			public override MessageManagerForDeclaration GetNewMessageManager(MessageManager.OperationType operationType, AdditionalMessageInformation additionalMessageInformation)
			{
				return new Business.MessageBuilders.ECIWriteOff.MessageManager(declaration, operationType, additionalMessageInformation, SubmitterCode);
			}

			ZString SubmitterCode
			{
				get { return NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant().PadLeft(9, '0'); }
			}
		}
		#endregion

		#region ECIWriteOffManifestingWrapper
		class ECIWriteOffManifestingWrapper : EntryTypeWrapper
		{
			public ECIWriteOffManifestingWrapper(JobDeclaration declaration) : base(declaration) { }

			public override MessageManagerForDeclaration GetNewMessageManager(MessageManager.OperationType operationType)
			{
				return new Business.MessageBuilders.ECIWriteOff.Manifesting.MessageManager(declaration, operationType);
			}

			public override SubmitToCustomsForm GetNewSubmitToCustomsForm(MessageManagerForDeclaration messageManager)
			{
				return new ECIWriteOff.Manifesting.SubmitToCustomsForm((Business.MessageBuilders.ECIWriteOff.Manifesting.MessageManager)messageManager);
			}

			public override bool DeclarationIsOkToSend(MessageManagerForDeclaration messageManager)
			{
				return messageManager.IsOKToSendWithMessagingErrors();
			}

			public override MessageManagerForDeclaration GetNewMessageManager(MessageManager.OperationType operationType, AdditionalMessageInformation additionalMessageInformation)
			{
				return new Business.MessageBuilders.ECIWriteOff.Manifesting.MessageManager(declaration, operationType, additionalMessageInformation, SubmitterCode);
			}

			ZString SubmitterCode
			{
				get { return NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant().PadLeft(9, '0'); }
			}
		}
		#endregion

		#endregion
	}
}
