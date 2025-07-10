using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class AESMessageManager
	{
		public AESMessageManager(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		public event Customs.Business.MessageSender.SaveEventHandler OnSave;

		public bool MergeAndCheck()
		{
			var sender = declaration.MessageInitiator;
			bool result = false;
			if (MergeIfNecessary(sender) && IsOkToSend(sender))
			{
				result = !declaration.HasChanges || SaveJob();
			}
			return result;
		}

		bool IsOkToSend(ISendsMessagesToCustoms sender)
		{
			bool result = false;
			if (Validation.CheckBusinessObjectLevelValidation(sender))
			{
				var errors = GetAnyReasonsWeCantSendToAESTIR();
				if (errors.Count > 0)
				{
					sender.MessageSendErrorAlert(errors);
				}
				else
				{
					result = true;
				}
			}
			return result && CheckDeniedParty();
		}

		bool CheckDeniedParty()
		{
			return MessageManagerCreditCheckWithSecurityHelper.CheckDeniedParty(declaration);
		}

		bool MergeIfNecessary(ISendsMessagesToCustoms sender)
		{
			bool result = true;
			if (declaration.MergeManager.RequiresMerge || declaration.ActiveEntryHeaders.Count == 0 || declaration.RequiresBGMReferenceGeneration)
			{
				result = declaration.DoMerge(sender);
			}
			return result;
		}

		public static void SubmitToCustoms(IAESDeclaration declaration)
		{
			var builder = new ZStringBuilder();
			foreach (IAESEntry entry in declaration.ActiveEntryHeaders)
			{
				if (entry.US_ShouldBeReportToCustoms)
				{
					var actionCode = entry.MessageAction;
					new AESTIRMessageBuilder(entry, actionCode).PopulateMessage();
					var status = ZString.Empty;
					switch (actionCode)
					{
						case UpdateActionCode.Add:
							status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
							break;
						case UpdateActionCode.Replace:
							status = AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse;
							break;
						default:
							status = AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse;
							break;
					}

					using (entry.SuspendMarkingAsNeedingValidation())
					{
						entry.CH_Status = status;
						entry.CH_EntryStatus = (entry.CH_EntryStatus.IsEmpty) ? entry.CH_Status : entry.CH_EntryStatus;
						entry.PopulateEntrySubmittedDateIfRequired();
						builder.Append(entry.CH_BGMReference);
					}
				}
			}

			if (!builder.IsEmpty)
			{
				declaration.LogCustomsCommencedIfNeeded();
				try
				{
					declaration.Factory.Save();
					declaration.MessageInitiator.NotifyUserOfASuccessfulSend("The following SED(s) have been submitted:\r\n" + builder.ToStringWithNewLineBetweenAppends());
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
				}
			}
		}

		#region Implementation

		MessageSendingValidation Validation
		{
			get
			{
				if (fValidation == null)
				{
					fValidation = MessageSendingValidation.New(declaration, new USCustomsNotificationCollector(declaration, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors());
				}
				return fValidation;
			}
		}
		MessageSendingValidation fValidation;

		public StringCollection GetAnyReasonsWeCantSendToAESTIR()
		{
			var result = new StringCollection();

			var entryFiler = USCustomsDataRegistry.Instance.ExportEntryFilerID.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);

			var entryFilerIDAlphanumeric = entryFiler.EntryFilerID.KeepAlphanumericCharacters();
			if (entryFilerIDAlphanumeric.IsEmpty)
			{
				result.Add(EntryFilerIDForThisNotSetup);
			}
			else
			{
				var validIEN = entryFiler.EntryFilerIDType == "E" &&
					(entryFilerIDAlphanumeric.Length == 11 && entryFilerIDAlphanumeric.EndsWith("00") || entryFilerIDAlphanumeric.Length == 9);

				var validOtherID = entryFiler.EntryFilerIDType != "E" && entryFilerIDAlphanumeric.Length == 9;

				if (!validIEN && !validOtherID)
				{
					result.Add(IDFormat);
				}
			}

			return result;
		}
		internal const string EntryFilerIDForThisNotSetup = "Entry Filer ID has not been set for this branch. Please set it in the Registry > Customs > United States of America > Export > AES > Entry Filer ID.";
		internal const string IDFormat = "Entry Filer ID for this branch does not conform to an expected format. You will not be able to send further messages until this ID is set correctly in Registry > Customs > United States of America > Export > AES > Entry Filer ID. The expected format is explained in the registry.";

		bool SaveJob()
		{
			bool result = true;
			if (OnSave != null)
			{
				OnSave();

				if (declaration.HasChanges || declaration.HasErrors)
				{
					result = false;
				}
			}
			return result;
		}

		readonly JobDeclaration declaration;
		#endregion
	}
}
