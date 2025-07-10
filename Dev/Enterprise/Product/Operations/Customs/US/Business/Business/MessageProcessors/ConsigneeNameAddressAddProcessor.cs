using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using CustomsNotificationCollector = Enterprise.Customs.Business.CustomsNotificationCollector;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAddResponse)]
	public class ConsigneeNameAddressAddProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			CusEntryHeader entryHeaderLinked = CusEntryHeaderLinker.Link(Message);

			string jobNumber = "Unknown";
			if (entryHeaderLinked != null)
			{
				jobNumber = entryHeaderLinked.Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;
			}

			IEnumerable<ConsigneeNameAddressDetails> consigneeDetails = GetConsigneeAddDetailsFromMessageBlocks();

			ZStringBuilder emailBody = GetEmailBody(consigneeDetails, entryHeaderLinked);

			bool hasAcceptedLines, hasRejectedLines;

			List<ZString> errorCodes;
			CalculateStatus(consigneeDetails, entryHeaderLinked, out hasAcceptedLines, out hasRejectedLines, out errorCodes);

			string url = entryHeaderLinked == null ? string.Empty : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entryHeaderLinked);

			if (!hasRejectedLines && entryHeaderLinked != null)
			{
				AutoSendCRLIfRequired(entryHeaderLinked, emailBody);
			}

			GlbBranch branch = entryHeaderLinked != null ? entryHeaderLinked.Branch : null;
			GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, "Consignee Name Address Add", emailBody.ToString(), hasRejectedLines, branch, entryHeaderLinked);
		}

		void AutoSendCRLIfRequired(CusEntryHeader entryHeaderLinked, ZStringBuilder emailBody)
		{
			JobDeclaration declaration = entryHeaderLinked.Declaration;

			if (declaration.US_EnableCRL)
			{
				Guid branchGuid = GlbBranch.CurrentBranch.PK.ToGuid();
				if (declaration.Branch != null)
				{
					branchGuid = declaration.Branch.PK.ToGuid();
				}

				if (USCustomsDataRegistry.Instance.AutoSendCargoReleaseMessageOnSuccessfulIJ.GetValueWithoutFallback(Guid.Empty, branchGuid, Guid.Empty))
				{
					ZString errorsToNotify = "";
					bool canSendCargoRelease = ValidateCRLDeclaration(declaration, errorsToNotify);

					emailBody.Append(CRLShouldBeSent);
					emailBody.Append("<br />");
					emailBody.Append("<br />");

					if (canSendCargoRelease)
					{
						SendCRLOrBCRMessage(declaration, emailBody);
					}
					else
					{
						emailBody.Append("<b>" + EmailFailedDescription + "</b>");
						emailBody.Append("<br />");
						emailBody.Append(errorsToNotify);
						emailBody.Append("<br />");
					}
				}
			}
		}
		internal const string EmailSuccessfulDescription = "The Cargo Release message has been generated and automatically sent to customs.";
		internal const string EmailFailedDescription = "The Cargo Release message could not be generated and automatically sent to customs due to the following errors:";
		internal const string CRLShouldBeSent = "Auto Send Cargo Release Message: Current Registry setting set to 'Yes'.";

		bool ValidateCRLDeclaration(JobDeclaration declaration, ZString errorsToNotify)
		{
			bool validationResult = true;
			declaration.LoadChildEditableObjects();
			using (((IBusinessObjectInternals)declaration).ResumeValidationForAllDescendantsTemporarily())
			{
				declaration.RunPreSaveValidation();
			}

			if (declaration.HasErrors)
			{
				errorsToNotify = new CustomsNotificationCollector(declaration, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().ToUniqueMessageListString() + "\n";
				validationResult = false;
			}

			if (declaration.HasMessageErrors)
			{
				IEnumerable<INotification> messageErrorCollector = new CustomsNotificationCollector(declaration, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
				errorsToNotify += messageErrorCollector.ToUniqueMessageListString();
				validationResult = false;
			}

			return validationResult;
		}

		void SendCRLOrBCRMessage(JobDeclaration declaration, ZStringBuilder emailBody)
		{
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (entry.IsCargoRelease || entry.IsBorderCargoRelease)
				{
					bool wasNotSent = !(entry.IsWaitingForResponse && entry.HasBeenLodgedAtCustoms);
					if (wasNotSent)
					{
						EntryHeaderMessageSendingAction action = new EntryHeaderMessageSendingAction(entry, entry.IsCargoRelease ? ImportMessageStatusList.MessageType.CargoRelease : ImportMessageStatusList.MessageType.BorderCargoRelease, new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original));
						action.US_SendMessage = true;
						MQEDIMessage message;

						if (entry.IsCargoRelease)
						{
							CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, declaration.US_CertifyCargoRelease);
							message = builder.PopulateMessage();
						}
						else
						{
							BorderCargoReleaseMessageBuilder builder = new BorderCargoReleaseMessageBuilder(entry, UpdateActionCode.Add);
							message = builder.PopulateMessage();
						}

						MessageStatusCalculator statusCalculator = new CargoReleaseMessageStatusCalculator(entry);
						statusCalculator.CalculateStatus(message, ABIResponseStatus.Undefined);
						entry.Messages.Add(message);

						emailBody.Append("<b>" + EmailSuccessfulDescription + "</b>");
						emailBody.Append("<br />");
						emailBody.Append("A response should be available in a few minutes.");
						emailBody.Append("<br />");
					}
					break;
				}
			}
		}

		void CalculateStatus(IEnumerable<ConsigneeNameAddressDetails> consigneeDetails, CusEntryHeader entryHeader, out bool hasAcceptedLines, out bool hasRejectedLines, out List<ZString> errorCodes)
		{
			hasAcceptedLines = false;
			hasRejectedLines = false;
			errorCodes = new List<ZString>();
			var hasEntryHeader = entryHeader != null;
			foreach (ConsigneeNameAddressDetails detail in consigneeDetails)
			{
				hasAcceptedLines |= detail.IsAccepted;
				hasRejectedLines |= !detail.IsAccepted;
				if (hasEntryHeader)
				{
					detail.SetUS_IJAccepted(entryHeader);
				}
				foreach (CodeDescriptionPair pair in detail.narrativeMessages)
				{
					if (!errorCodes.Contains(pair.Code))
					{
						errorCodes.Add(pair.Code);
					}
				}
			}

			if (hasAcceptedLines && hasEntryHeader && !entryHeader.US_UseConsigneeNameAddress)
			{
				entryHeader.US_UseConsigneeNameAddress = true;
			}
		}

		ZStringBuilder GetEmailBody(IEnumerable<ConsigneeNameAddressDetails> consigneeDetails, CusEntryHeader entryHeader)
		{
			ZStringBuilder emailBody = new ZStringBuilder();

			foreach (ConsigneeNameAddressDetails detail in consigneeDetails)
			{
				OrgHeader ultimateConsignee = entryHeader != null ? detail.GetOrganisation(entryHeader) : null;

				string url = ultimateConsignee == null ? string.Empty : ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, ultimateConsignee.PK.ToGuid());
				string code = ultimateConsignee == null ? ZString.Empty : ultimateConsignee.OH_Code;

				if (!string.IsNullOrEmpty(url))
				{
					code = "<a href=\"" + url + "\">" + code + "</a>";
				}

				HtmlTableCreator table = new HtmlTableCreator(new string[] { detail.companyName, code });

				table.WriteRow("Address One Line", detail.address1);

				table.WriteRow(detail.IsAccepted ? "Accepted for entry line(s)" : "Rejected for entry line(s)", detail.AssociatedEntryLineNumber);

				if (!detail.IsAccepted)
				{
					foreach (CodeDescriptionPair pair in detail.narrativeMessages)
					{
						table.WriteRow("Rejection Reason", MessageCalculator.GetLongDescription(pair.Code, pair.Description));
					}
				}

				emailBody.Append(table.ToHtml());
				emailBody.Append("<br />");
			}

			return emailBody;
		}

		IEnumerable<ConsigneeNameAddressDetails> GetConsigneeAddDetailsFromMessageBlocks()
		{
			Dictionary<string, ConsigneeNameAddressDetails> consigneeAddDetails = new Dictionary<string, ConsigneeNameAddressDetails>();

			ConsigneeNameAddressDetails consigneeAddDetail = null;

			foreach (MessageBlock block in messageBlocks)
			{
				CONI3 conI3 = block as CONI3;

				if (conI3 != null)
				{
					if (!consigneeAddDetails.TryGetValue(conI3.ConsigneeName + conI3.AddressLineOne, out consigneeAddDetail))
					{
						consigneeAddDetail = new ConsigneeNameAddressDetails(conI3.ConsigneeName, conI3.AddressLineOne);
						consigneeAddDetails.Add(conI3.ConsigneeName + conI3.AddressLineOne, consigneeAddDetail);
					}
				}
				else
				{
					CONIB conIB = block as CONIB;
					if (conIB != null)
					{
						if (!conIB.LineItemSequenceNumber.IsEmpty)
						{
							consigneeAddDetail.AddEntryLineNumber(conIB.LineItemSequenceNumber);
						}

						consigneeAddDetail.AddNarativeMessage(conIB.NarrativeMessageIdentifier, conIB.NarrativeMessage);
					}
				}
			}

			return consigneeAddDetails.Values;
		}

		#region Helper Classes

		class ConsigneeNameAddressDetails
		{
			public ConsigneeNameAddressDetails(string companyName, string address1)
			{
				this.companyName = companyName;
				this.address1 = address1;
				this.entryLineNumbers = new List<ZString>();
				narrativeMessages = new List<CodeDescriptionPair>();
			}

			public readonly string companyName;
			public readonly string address1;
			readonly List<ZString> entryLineNumbers;
			public readonly List<CodeDescriptionPair> narrativeMessages;

			public void AddEntryLineNumber(ZString entryLineNumber)
			{
				if (!entryLineNumbers.Contains(entryLineNumber))
				{
					entryLineNumbers.Add(entryLineNumber);
				}
			}

			public OrgHeader GetOrganisation(CusEntryHeader entry)
			{
				foreach (ZString entryLineNumber in entryLineNumbers)
				{
					CusEntryLine entryLine = entry.MergedLines.FindByFormattedLineNumber(entryLineNumber);

					if (entryLine != null)
					{
						IAddressDetails ultimateConsignee = entryLine.UltimateConsigneeForCargoRelease;

						if (ultimateConsignee != null && ultimateConsignee.CompanyName.StartsWith(companyName) && ultimateConsignee.AddressLine1.StartsWith(address1))
						{
							return (OrgHeader)ultimateConsignee;
						}
					}
				}
				return null;
			}

			public string AssociatedEntryLineNumber
			{
				get
				{
					ZStringBuilder result = new ZStringBuilder();

					foreach (ZString entryLineNumber in entryLineNumbers)
					{
						result.Append(entryLineNumber);
					}

					return result.ToStringWithDelimiterBetweenAppends(", ");
				}
			}

			public void AddNarativeMessage(string errorCode, string shortDescription)
			{
				if (narrativeMessages.Find(x => x.Code == errorCode) == null)
				{
					narrativeMessages.Add(new CodeDescriptionPair(errorCode, shortDescription));
				}
			}

			public bool IsAccepted
			{
				get { return narrativeMessages.Find(x => x.Code == "2GC") != null; }
			}

			public void SetUS_IJAccepted(CusEntryHeader entryHeader)
			{
				var isAccepted = IsAccepted;

				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					if (entryLineNumbers.Contains(entryLine.CL_LineNumberFormatted) && !entryLine.IsSecondaryTariffLine)
					{
						entryLine.US_IJAccepted = isAccepted;
					}
				}
			}
		}

		#endregion
	}
}
