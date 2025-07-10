using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.CodeMapping;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US;
using ILoggingInformation = Enterprise.Integration.BatchProcessor.ILoggingInformation;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public sealed class EBondMessageProcessor : IEBondMessageProcessor
	{
		public EBondMessageProcessor(XmlSessionTracker tracker)
		{
			this.tracker = Argument.NotNull(tracker, nameof(tracker));
		}

		readonly XmlSessionTracker tracker;

		void IEBondMessageProcessor.Process(BusinessObjectFactory factory, ILoggingInformation loggingInformation, ZGuid messagePk)
		{
			var message = messagePk.IsValid ? factory.Load<EBondEDIMessage>(messagePk) : null;

			if (message != null)
			{
				var codeMapper = new CodeMappingManager(tracker);
				var eventDataObject = message.GetEM_MessageTextReader().Parse<UniversalEvent>(tracker, codeMapper);

				var declaration = FindJobDeclaration(factory, eventDataObject);
				var entrySummary = declaration?.ActiveEntryHeaders.EntrySummaryEntry;

				if (entrySummary != null)
				{
					tracker.LogBoth(LogType.Information, Res.GetString("12B35489-A773-4E2A-9A20-DA81700E6551", "Match {0} from the message {1}.", declaration.HumanReadableName, message.EM_MessageNum));

					message.EM_GB = declaration.Branch?.PK ?? message.EM_GB;
					message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
					message.EM_LinkUniqueID = entrySummary.PK;
					message.EM_Status = EDIMessage.Status.Received;

					var suretyResponseCode = GetContextValue(eventDataObject, ContextTypes.SuretyResponseCode);
					UpdateDeclaration(declaration, message, eventDataObject, suretyResponseCode);

					var emailDef = BuildInterpretation(declaration, entrySummary, message, eventDataObject, suretyResponseCode);
					SendReportIfNeed(factory, message, eventDataObject, emailDef, entrySummary);
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Failed;
					tracker.LogBoth(LogType.Information, Res.GetString("02A84192-2741-4472-A089-8E97C79B9DFB", "Couldn't match any data from the message {0}.", message.EM_MessageNum));
				}
			}
		}

		#region Update Declaration

		void UpdateDeclaration(JobDeclaration declaration, EBondEDIMessage message, UniversalEvent eventDataObject, ZString suretyResponseCode)
		{
			if (declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond)
			{
				var insuranceDisposition = GetContextValue(eventDataObject.ContextCollection, ContextTypes.DispositionCode)?.SubstringSafe(0, USAddInfoSchema.US_InsuranceDisposition.MaxLength);
				if (insuranceDisposition.HasValue)
				{
					var dispositionValue = insuranceDisposition.Value;
					declaration.US_InsuranceDisposition = dispositionValue;
					if (dispositionValue == InsuranceDispositionCodeList.Codes.AcceptedByCBP || dispositionValue == InsuranceDispositionCodeList.Codes.DataAcceptedBySuretyPendingReview)
					{
						message.EM_ApplicationReference = Constants.EBond.ApplicationReferences.Accepted;
					}
				}

				var bondDesignationCode = GetContextValue(eventDataObject.ContextCollection, ContextTypes.BondDesignationCode)?.SubstringSafe(0, USAddInfoSchema.US_BondDesignationCode.MaxLength);
				declaration.US_BondDesignationCode = bondDesignationCode ?? declaration.US_BondDesignationCode;

				if (insuranceDisposition.HasValue || bondDesignationCode.HasValue)
				{
					tracker.LogBoth(LogType.Information, string.Format(CultureInfo.InvariantCulture
						, "Update bond data on {0} from the message {1}."
						, declaration.HumanReadableName
						, message.EM_MessageNum));
				}
			}
			var suretyResponseDescription = GetContextValue(eventDataObject, ContextTypes.SuretyResponseDescription);
			var paramForReason = suretyResponseCode + ' ' + suretyResponseDescription;
			declaration.Logs.AddNew(Events.StatusUpdated,
					new KeyValuePair<string, string>(Params.MessageType, "eBond Status"),
					new KeyValuePair<string, string>(Params.Reason, paramForReason));
		}

		#endregion

		#region Build Interpretation

		EmailDef BuildInterpretation(JobDeclaration declaration, CusEntryHeader entryHeader, EBondEDIMessage message, UniversalEvent eventDataObject, ZString suretyResponseCode)
		{
			var subject = Res.GetString("0f1031ed-c863-42c4-8a7f-cb9f88d1cc96", "eBond Response for {0}/{1} - {2}", declaration.HumanReadableShortcutName, entryHeader.UniqueReference, message.EM_MessageNum);

			var emailBuilder = new EmailDefBuilder(subject, message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			emailBuilder.AddArgReplacement(message.EM_MessageNum);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, " from Surety Agent");

			var messageInterpretation = GetMessageInterpretation(declaration, eventDataObject, suretyResponseCode);
			message.EM_MessageInterpretation = messageInterpretation;

			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, messageInterpretation);

			return emailBuilder.ToEmail();
		}

		string GetMessageInterpretation(JobDeclaration declaration, UniversalEvent eventDataObject, ZString suretyResponseCode)
		{
			var result = new HtmlTableCreator(TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };

			result.WriteRow(GetHeaderSection(declaration, eventDataObject, suretyResponseCode));
			result.WriteRow(GetSuretySection(eventDataObject));
			result.WriteRow(GetLineLevelResultsSection(eventDataObject));
			result.WriteRow(GetMessageLevelReasonCodesSection(eventDataObject));

			return result.ToHtml();
		}

		string GetHeaderSection(JobDeclaration declaration, UniversalEvent eventDataObject, ZString suretyResponseCode)
		{
			var result = new HtmlTableCreator(new[] { Res.GetString("BDF61B82-7518-4DF4-9972-BD9BE1127494", "Bond Data") }, TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };

			var tableInterpretation = new FieldValueTableInterpretation(false);
			tableInterpretation.Add(Res.GetString("A5DD69DC-1ABB-4FCF-B9C6-569E1BA2AC66", "Declaration"), declaration.JE_DeclarationReference);
			tableInterpretation.Add(Res.GetString("C3242037-DC86-418F-B954-246B36D832F8", "Entry Number"), GetContextValue(eventDataObject, ContextTypes.EntryNumber));

			var dispositionCode = GetContextValue(eventDataObject, ContextTypes.DispositionCode);

			tableInterpretation.Add(Res.GetString("F618336E-B061-485A-A822-D73FF508EF33", "Disposition Code"), dispositionCode);
			tableInterpretation.Add(Res.GetString("EC80CF42-6FC6-4C33-91DF-A77F4166DA2C", "Surety Response Code"), suretyResponseCode);
			tableInterpretation.Add(Res.GetString("3309907D-498F-4FDD-BCD2-6E58263088B0", "Bond Designation Code"), GetContextValue(eventDataObject, ContextTypes.BondDesignationCode));

			result.WriteRow(tableInterpretation.ToHtml());

			tableInterpretation = new FieldValueTableInterpretation(false);

			var dispositionDescription = declaration.AddInfoLookups.InsuranceDispositionList.GetDescriptionFromCode(dispositionCode);
			var suretyResponseDescription = GetContextValue(eventDataObject, ContextTypes.SuretyResponseDescription);

			if (string.IsNullOrWhiteSpace(suretyResponseDescription))
			{
				suretyResponseDescription = declaration.AddInfoLookups.InsuranceAgentList.GetDescriptionFromCode(suretyResponseCode);
			}

			tableInterpretation.Add(Res.GetString("753F3044-9A6B-4B7D-9429-695F310193A9", "Disposition Code Description"), dispositionDescription);
			tableInterpretation.Add(Res.GetString("3E70B657-0898-4CE3-8D06-BE576171D392", "Surety Response Code Description"), suretyResponseDescription);

			result.WriteRow(tableInterpretation.ToHtml());

			return result.ToHtml();
		}

		string GetSuretySection(UniversalEvent eventDataObject)
		{
			var result = new HtmlTableCreator(new[] { Res.GetString("6E8CE986-4DF5-4C4B-8644-CDCDDAF554D6", "Surety Details") }, TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };

			var tableInterpretation = new FieldValueTableInterpretation(false);
			tableInterpretation.Add(Res.GetString("15431584-F4CB-4A26-A39C-57ED8A55E689", "Code"), GetContextValue(eventDataObject, ContextTypes.SuretyCode));
			tableInterpretation.Add(Res.GetString("BF90596E-A60D-40A4-BC54-B0BB6F903829", "Name"), GetContextValue(eventDataObject, ContextTypes.SuretyContactName));
			tableInterpretation.Add(Res.GetString("A41B65B0-B763-4B8B-8290-817812A106A6", "Email"), GetContextValue(eventDataObject, ContextTypes.SuretyContactEmail));
			tableInterpretation.Add(Res.GetString("78633B8F-6045-4BCE-B472-1D695A63A0F0", "Phone"), GetContextValue(eventDataObject, ContextTypes.SuretyContactPhone));

			result.WriteRow(tableInterpretation.ToHtml());

			return result.ToHtml();
		}

		string GetLineLevelResultsSection(UniversalEvent eventDataObject)
		{
			var result = string.Empty;

			var lineLevelResults = eventDataObject.ContextCollection?.Where(c => (c.Type?.Type.GetValueOrDefault() ?? ZString.Empty).EqualsIgnoringCase(ContextTypes.LineLevelResults));

			if (lineLevelResults != null && lineLevelResults.Any())
			{
				var headerTableCreator = new HtmlTableCreator(new[] { Res.GetString("14EA7132-A70E-491E-9F95-2FFAA4A3A603", "Line Level Results") }, TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };
				var columnTitles = new[]
				{
					Res.GetString("403692A8-ED49-4337-A27E-24F7261EECF9", "Line Number"),
					Res.GetString("30E8EA3A-A282-4C81-85D4-DB9834FD38B8", "HS Code"),
					Res.GetString("B55A6597-7DEB-4D7D-92AC-C52084BE4860", "Reason Code"),
					Res.GetString("7A9DB78F-122E-4CD4-922D-AAA54DA267C7", "Reason Description")
				};

				var dataTableCreator = new HtmlTableCreator(columnTitles, TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };

				foreach (var lineLevelResult in lineLevelResults)
				{
					dataTableCreator.WriteRow
					(
						lineLevelResult.Value.GetValueOrDefault(),
						GetContextValue(lineLevelResult.SubContextCollection, ContextTypes.HSCode),
						GetContextValue(lineLevelResult.SubContextCollection, ContextTypes.ReasonCode),
						GetContextValue(lineLevelResult.SubContextCollection, ContextTypes.ReasonDescription)
					);
				}

				headerTableCreator.WriteRow(dataTableCreator.ToHtml());
				result = headerTableCreator.ToHtml();
			}

			return result;
		}

		string GetMessageLevelReasonCodesSection(UniversalEvent eventDataObject)
		{
			var result = string.Empty;

			var messageLevelReasonCodes = eventDataObject.ContextCollection?.Where(c => (c.Type?.Type.GetValueOrDefault() ?? ZString.Empty).EqualsIgnoringCase(ContextTypes.MessageLevelReasonCodes));

			if (messageLevelReasonCodes != null && messageLevelReasonCodes.Any())
			{
				var headerTableCreator = new HtmlTableCreator(new[] { Res.GetString("7CBD1B4F-9DE2-49C7-B4A5-94B60FCDDD38", "Message Level Reason Codes") }, TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };
				var columnTitles = new[]
				{
					Res.GetString("B55A6597-7DEB-4D7D-92AC-C52084BE4860", "Reason Code"),
					Res.GetString("7A9DB78F-122E-4CD4-922D-AAA54DA267C7", "Reason Description")
				};

				var dataTableCreator = new HtmlTableCreator(columnTitles, TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };

				foreach (var messageLevelReasonCode in messageLevelReasonCodes)
				{
					dataTableCreator.WriteRow
					(
						GetContextValue(messageLevelReasonCode.SubContextCollection, ContextTypes.ReasonCode),
						GetContextValue(messageLevelReasonCode.SubContextCollection, ContextTypes.ReasonDescription)
					);
				}

				headerTableCreator.WriteRow(dataTableCreator.ToHtml());
				result = headerTableCreator.ToHtml();
			}

			return result;
		}

		#endregion

		#region Send Report

		void SendReportIfNeed(BusinessObjectFactory factory, EBondEDIMessage message, UniversalEvent eventDataObject, EmailDef email, CusEntryHeader entry)
		{
			var sender = GetSender(factory, entry, message.EM_SystemCreateTimeUtc);
			var recipient = sender?.GS_EmailAddress ?? ZString.Empty;
			var reasonOfCannotSend = string.Empty;

			if (!recipient.IsEmpty)
			{
				try
				{
					email.AddRecipientForUserCommunication(recipient, RecipientDef.RecipientTypes.TO);

					if (factory != null)
					{
						Env.OutgoingCustomsMailManager.Create(factory, email);
					}
					else
					{
						Env.OutgoingCustomsMailManager.CreateAndSave(email);
					}

					tracker.LogBoth(LogType.Information, string.Format(CultureInfo.InvariantCulture
							, "Create email to {0} from the message {1}."
							, recipient
							, message.EM_MessageNum));
				}
				catch (EmailSendFailedException exception)
				{
					reasonOfCannotSend = exception.Message;
				}
			}
			else if (USCustomsDataRegistry.Instance.BondStatusNotificationGroup != null)
			{
				try
				{
					var bondStatusNotificationGroupPk = USCustomsDataRegistry.Instance.BondStatusNotificationGroup.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
					if (bondStatusNotificationGroupPk != ZGuid.Empty)
					{
						if (factory != null)
						{
							Env.OutgoingCustomsMailManager.Create(factory, email, bondStatusNotificationGroupPk, GroupSourceLocator.GetFromRegistryItem(USCustomsDataRegistry.Instance.BondStatusNotificationGroup));
						}
						else
						{
							Env.OutgoingCustomsMailManager.CreateAndSave(email, bondStatusNotificationGroupPk, GroupSourceLocator.GetFromRegistryItem(USCustomsDataRegistry.Instance.BondStatusNotificationGroup));
						}

						tracker.LogBoth(LogType.Information, string.Format(CultureInfo.InvariantCulture
								, "Create email to group {0} from the message {1}."
								, USCustomsDataRegistry.Instance.BondStatusNotificationGroup.Name
								, message.EM_MessageNum));
					}
				}
				catch (EmailSendFailedException exception)
				{
					reasonOfCannotSend = exception.Message;
				}
			}
			else
			{
				reasonOfCannotSend = Res.GetString("21d180fe-d329-4b44-bc5e-8d079583a693", "The email address of Surety Contact is empty.");
			}

			if (!string.IsNullOrEmpty(reasonOfCannotSend))
			{
				tracker.LogBoth(LogType.Error, string.Format(CultureInfo.InvariantCulture
					, "Couldn't send email: {0}.  Here are the contents of the email that couln't be sent:{1}{1}SUBJECT:{2}{1}BODY:{3}{1}"
					, reasonOfCannotSend
					, System.Environment.NewLine
					, email.Subject
					, email.Body));
			}
		}

		GlbStaff GetSender(BusinessObjectFactory factory, CusEntryHeader entry, ZDateTime createTime)
		{
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, CusEntryHeader.Schema.TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, entry.PK);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.USeBond);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, EDIInterchangeTypeList.Codes.XDC);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, createTime);
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";
			var message = factory.LoadTop1<EBondEDIMessage>(query);
			return message?.UserWhoQueuedThisRecord;
		}

		#endregion

		#region Implement

		JobDeclaration FindJobDeclaration(BusinessObjectFactory factory, UniversalEvent eventDataObject)
		{
			JobDeclaration result = null;

			var dataTargetData = eventDataObject.DataContext?.DataTargetCollection?.FirstOrDefault();

			var declarationReference = (dataTargetData?.Type.GetValueOrDefault() ?? ZString.Empty).EqualsIgnoringCase(TargetNameOfCustomsDeclaration)
				? dataTargetData.Key.GetValueOrDefault()
				: ZString.Empty;

			if (!declarationReference.IsEmpty)
			{
				var declarationFilter = new ZDBOnlyQuery(typeof(JobDeclaration));
				declarationFilter.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, declarationReference);

				var companySubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
				companySubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
				declarationFilter.AddSubQuery(companySubQuery, JoinCondition.And);

				var entryNumber = GetContextValue(eventDataObject, ContextTypes.EntryNumber);

				if (!entryNumber.IsEmpty)
				{
					var entryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
					entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, Customs.Business.AutoJobDeclaration.Schema.TableName);
					entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber.SubstringSafe(3));
					entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_Category, ModuleTreeCustomerServiceMenuSectionList.Codes.Customs);
					entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
					entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, new ZString[] { Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates });

					declarationFilter.AddSubQuery(entryNumberSubQuery, JoinCondition.And);

					var entryFilerSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
					entryFilerSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, AutoUSAddInfo.Schema.US_EntryFilerCode);
					entryFilerSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, entryNumber.SubstringSafe(0, 3));
					declarationFilter.AddSubQuery(entryFilerSubQuery, JoinCondition.And);
				}

				result = factory.LoadTop1<JobDeclaration>(declarationFilter);
			}

			return result;
		}

		const string TargetNameOfCustomsDeclaration = "CustomsDeclaration";

		ZString GetContextValue(UniversalEvent eventDataObject, string contextType)
		{
			return GetContextValue(eventDataObject.ContextCollection, contextType) ?? ZString.Empty;
		}

		ZString? GetContextValue(List<Context> contexts, string contextType)
		{
			return contexts?
				.FirstOrDefault(c => (c.Type?.Type.GetValueOrDefault() ?? ZString.Empty).EqualsIgnoringCase(contextType))
				?.Value.GetValueOrDefault();
		}

		#endregion

		#region ContextTypes

		public static class ContextTypes
		{
			public const string HSCode = "HSCode";
			public const string ReasonCode = "ReasonCode";
			public const string ReasonDescription = "ReasonDescription";

			public const string SuretyCode = "SuretyCode";
			public const string SuretyContactName = "SuretyContactName";
			public const string SuretyContactEmail = "SuretyContactEmail";
			public const string SuretyContactPhone = "SuretyContactPhone";
			public const string SuretyResponseCode = "SuretyResponseCode";
			public const string SuretyResponseDescription = "SuretyResponseDescription";

			public const string DispositionCode = "DispositionCode";
			public const string EntryNumber = "EntryNumber";
			public const string BondDesignationCode = "BondDesignationCode";

			public const string LineLevelResults = "LineLevelResults";
			public const string MessageLevelReasonCodes = "MessageLevelReasonCodes";
		}

		#endregion
	}
}
