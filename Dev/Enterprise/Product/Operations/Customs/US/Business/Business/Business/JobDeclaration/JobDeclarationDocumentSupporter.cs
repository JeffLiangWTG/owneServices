using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.AES;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class JobDeclarationDocumentSupporter : Customs.Business.BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)BusinessObject; }
		}

		internal const string CustomsDeliveryOrderDataContextValue = ".USCustomsDeliveryOrder";
		internal const string EntryImmediateDeliveryDataContextValue = ".CusEntryHeaderENS";
		internal const string EntrySummary7501DataContextValue = ".CusEntryHeader7501";
		internal const string ENSEntryHeader = ".ENSEntryHeader";
		internal const string FSISImportInspectionApplicationDataContextValue = ".FSISImportInspectionApplication";
		internal const string FSISImportInspectionAndReportDataContextValue = ".FSISImportInspectionAndReport";
		internal const string IT7512DepartureDataContextValue = ".CBPForm7512";
		internal const string AESDataContextValue = ".CusEntryHeaderAES";
		internal const string FTZ214DataContextValue = ".FTZ214DataContextValue";
		internal const string PGARecapContextValue = ".PGARecap";
		internal const string USDrawbackClaimContextValue = ".USDrawbackClaim";
		internal const string ACEEntryImmediateDeliveryDataContextValue = ".CusEntryHeaderENSACE";

		#region Overrides

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = new List<IDocumentEventsHandler>(base.GetDocumentEventsHandlers());
			result.Add(new PrintManagerDocumentEventsHandler() { DocumentSupporter = this });

			return result.ToArray();
		}

		#region DocumentEventsHandlers

		class PrintManagerDocumentEventsHandler : IDocumentEventsHandler
		{
			public bool CanHandleMenuItem(IStmMenuItem menuItem)
			{
				return IsCustomsDeliveryOrder(menuItem);
			}

			bool IsCustomsDeliveryOrder(IStmMenuItem menuItem)
			{
				return menuItem.SU_MenuName == DocumentNames.CustomsDeliveryOrder;
			}

			public void HandleDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
			{
			}

			public void HandleDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
			{
			}

			public void HandleDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				if (IsCustomsDeliveryOrder(e.MenuItem))
				{
					e.Cancel = PrintDocument<Integration.Customs.US.IDeliveryOrderPrintManager>();
				}
			}

			public void HandleDocumentPrinted(object sender, DocumentPrintedEventArgs e)
			{
			}

			public JobDeclarationDocumentSupporter DocumentSupporter;

			DocumentSupporter IDocumentEventsHandler.DocumentSupporter
			{
				get { return DocumentSupporter; }
				set { DocumentSupporter = value as JobDeclarationDocumentSupporter; }
			}

			bool PrintDocument<T>()
				where T : Integration.Customs.US.IPrintManager
			{
				T manager = (T)Activator.CreateInstance(ObjectFactory.GetType<T>(), DocumentSupporter.Declaration);
				return !manager.IsOkToPrint();
			}
		}

		#endregion

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			List<DataContextValue> result = GetSupportedBODataSourcesFor(BusinessObject.GetType());
			result.Add(new DataContextValue(CustomsDeliveryOrderDataContextValue));
			result.Add(new DataContextValue(EntryImmediateDeliveryDataContextValue));
			result.Add(new DataContextValue(ACEEntryImmediateDeliveryDataContextValue));
			result.Add(new DataContextValue(EntrySummary7501DataContextValue));
			result.Add(new DataContextValue(FSISImportInspectionApplicationDataContextValue));
			result.Add(new DataContextValue(FSISImportInspectionAndReportDataContextValue));
			result.Add(new DataContextValue(IT7512DepartureDataContextValue));
			result.Add(new DataContextValue(USDrawbackClaimContextValue));
			result.Add(new DataContextValue(ENSEntryHeader));
			result.Add(new DataContextValue(".LandedCostHeader"));
			result.Add(new DataContextValue(AESDataContextValue));
			result.Add(new DataContextValue(FTZ214DataContextValue));
			result.Add(new DataContextValue(PGARecapContextValue));
			return result;
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			var declaration = Declaration;
			var result = base.GetFilterValue(filterName);

			switch (filterName)
			{
				case DocumentFilters.PrintSocialSecurityNumberAllowed:
					result = Declaration.MessageTypeForDocumentFilter == JobMessageTypeList.Codes.Import
						&& Enterprise.Environment.Env.Security.USCustomsPrintSSN.IsAllowed ? "Y" : "N";
						break;
			}

			return result;
		}

		protected override List<DocumentSupporterQuestion> GenerateQuestionsToAskUsersBeforeRunningDocumentCore(IStmMenuItem commandAboutToBeRun)
		{
			var result = base.GenerateQuestionsToAskUsersBeforeRunningDocumentCore(commandAboutToBeRun);
			if (commandAboutToBeRun.SU_MenuName.Contains(DocumentNames.PPQForm368NoticeOfArrival))
			{
				if (commandAboutToBeRun.SU_MenuPath.Contains(Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments))
				{
					if (!Declaration.IsPPQForm368Box13Compatible)
					{
						result.Add(new DocumentSupporterQuestion("Warning: No PPQ data exists", string.Format(NoPPQData, CalculateDocumentPath(commandAboutToBeRun, false)), QuestionType.Warning));
					}
				}
				else if (Declaration.IsPPQForm368Box13Compatible)
				{
					result.Add(new DocumentSupporterQuestion("Warning: New form selected", string.Format(NewPPQForm, CalculateDocumentPath(commandAboutToBeRun, true)), QuestionType.Warning));
				}
			}
			if (commandAboutToBeRun.SU_MenuName.Contains(DocumentNames.EntrySummary7501) && !Declaration.US_ManEntry && HasInvoiceBeenDeletedOrDetachedSinceLodged)
			{
				result.Add(new DocumentSupporterQuestion("Warning: Invoices Deleted/Detached", Res.GetString("2AF9A233-AAD3-4474-8261-2A358AE69D4E", "Invoices have been deleted or detached after the latest ENS message was accepted.Printing of 7501 Entry Summary is not based on the latest message until another ENS message is accepted.\r\nDo you wish to continue?"), QuestionType.Warning));
			}
			return result;
		}

		static string CalculateDocumentPath(IStmMenuItem menuItem, bool legacyPath)
		{
			var result = "Documents -> ";
			if (legacyPath)
			{ result += "Legacy Documents -> "; }
			if (menuItem.SU_BusinessContext == "Shipment")
			{ result += "Customs -> "; }
			result += menuItem.SU_MenuName;
			return result;
		}

		const string NoPPQData = @"There is no PPQ data present to create this legacy form.

It is suggested that you create the PPQ Form 368 Notice of Arrival from {0}.

Are you sure you want to print a legacy Notice of Arrival?";
		const string NewPPQForm = @"The document you have selected is the new Notice of Arrival form and does not print data from PPQ 368 Box 13.

Please note that the old document can be found in {0}.

Are you sure you want to print a new Notice of Arrival?";

		protected override DocumentSupporterDataState GetDataStateBeforeRunCore(IStmMenuItem commandAboutToBeRun)
		{
			if (commandAboutToBeRun != null)
			{
				if (commandAboutToBeRun.SU_MenuName == LandedCostingMenuText || commandAboutToBeRun.SU_MenuName == DocumentNames.FDARecap)
				{
					if (Declaration.ActiveEntryHeaders.EntrySummaryEntry == null)
					{
						return new DocumentSupporterDataState(false, No7501EntryExist);
					}
				}
				else if (commandAboutToBeRun.SU_MenuName == DocumentNames.EntrySummary7501)
				{
					if (Declaration.IsImportByExternalBroker && Declaration.CreatedViaBIRD(BIRDApplicationCodeList.Codes.EntrySummary))
					{
						return new DocumentSupporterDataState(false, YouCannotPrint7501ForJobsDoneByExternalBroker);
					}
				}
				else if (commandAboutToBeRun.SU_MenuName == DocumentNames.DrawbackNoticeOfIntentMenuItem)
				{
					var dataState = GetDataStateForLinesToPrintDocument();
					if (dataState != null)
					{
						return dataState;
					}
				}
				else if (commandAboutToBeRun.SU_MenuName == DocumentNames.FTZ214AdmissionApplication) // identifier
				{
					if (BaseJobDeclaration.CustomsEntryHeaders.Count == 0)
					{
						return new DocumentSupporterDataState(false, MessageForInvaildEntryPrintDataState(commandAboutToBeRun));
					}
				}
			}
			return base.GetDataStateBeforeRunCore(commandAboutToBeRun);
		}

		public const string No7501EntryExist = "No 7501 entry exists. Please enable 7501 and click Brokerage > Merge to generate entries.";
		public const string YouCannotPrint7501ForJobsDoneByExternalBroker = "You cannot print '7501 Entry Summary' if a job is lodged by an external broker(IMX) and created via BIRD.";
		public const string PrintSSNMenuText = "(SSN)";

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			Declaration.PrintSocialSecurityNumberOnDocument = commandBeingRun?.SU_MenuName.Contains(PrintSSNMenuText) ?? false;

			if (dataContextValue.Equals(new DataContextValue(ENSEntryHeader)))
			{
				var result = new List<IBODocDataProvider>();
				var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
				if (entry != null)
				{
					result.Add(BODocDataProvider.Get(entry));
				}
				return result.ToArray();
			}

			if (dataContextValue.Equals(new DataContextValue(EntrySummary7501DataContextValue)))
			{
				return GetBODocDataFor7501Print();
			}

			if (dataContextValue.Equals(new DataContextValue(AESDataContextValue)))
			{
				return GetBODocDataForAESPrint();
			}

			if (dataContextValue.Equals(new DataContextValue(FTZ214DataContextValue)))
			{
				return GetBODocDataForFTZ214Print();
			}

			if (dataContextValue.Equals(new DataContextValue(PGARecapContextValue)))
			{
				return GetBODocDataForPGARecapPrint();
			}

			if (dataContextValue.ToString().StartsWith(IT7512DepartureDataContextValue))
			{
				return GetBODocDataFor7512Print(dataContextValue, commandBeingRun);
			}

			if (dataContextValue.Equals(new DataContextValue(EntryImmediateDeliveryDataContextValue)))
			{
				List<IBODocDataProvider> boDocDataProviders = new List<IBODocDataProvider>();
				List<IBODocDataProvider> boEntrySummaryDocDataProviders = new List<IBODocDataProvider>();

				foreach (CusEntryHeader entryHeader in BaseJobDeclaration.ActiveEntryHeaders)
				{
					entryHeader.ResetBillAndTariffLinesCache();
					if (entryHeader.IsCargoRelease || entryHeader.IsBorderCargoRelease || entryHeader.IsACECargoRelease)
					{
						boDocDataProviders.Add(BODocDataProvider.Get(entryHeader));
					}
					else if (entryHeader.IsFormalEntry)
					{
						boEntrySummaryDocDataProviders.Add(BODocDataProvider.Get(entryHeader));
					}
				}

				if (boDocDataProviders.Count > 0)
				{
					return boDocDataProviders.ToArray();
				}
				else
				{
					return boEntrySummaryDocDataProviders.ToArray();
				}
			}

			if (dataContextValue.Equals(new DataContextValue(ACEEntryImmediateDeliveryDataContextValue)))
			{
				List<IBODocDataProvider> boDocDataProviders = new List<IBODocDataProvider>();
				foreach (CusEntryHeader entryHeader in BaseJobDeclaration.ActiveEntryHeaders)
				{
					entryHeader.ResetBillAndTariffLinesCache();
					if (entryHeader.IsACECargoRelease)
					{
						entryHeader.ResetDocumentDataCache();
						boDocDataProviders.Add(BODocDataProvider.Get(entryHeader));
					}
				}
				return boDocDataProviders.ToArray();
			}

			if (dataContextValue.Equals(new DataContextValue(FSISImportInspectionApplicationDataContextValue)))
			{
				List<USInvoiceLineFSISLine> boDocDataProviders = new List<USInvoiceLineFSISLine>();

				if (Declaration.FSISLinesForPrint != null && Declaration.FSISLinesForPrint.Count > 0)
				{
					boDocDataProviders.AddRange(AddFSISDocuments(Declaration.FSISLinesForPrint));
				}
				else
				{
					foreach (JobComInvoiceLine invoiceLine in Declaration.InvoiceLines)
					{
						boDocDataProviders.AddRange(AddFSISDocuments(invoiceLine.FSISLines.ToArray<USInvoiceLineFSISLine>()));
					}
				}
				return boDocDataProviders.ConvertAll(x => BODocDataProvider.Get(x)).ToArray();
			}

			if (dataContextValue.Equals(new DataContextValue(USDrawbackClaimContextValue)))
			{
				return new IBODocDataProvider[] { BODocDataProvider.Get(new JobDeclarationDrawbackSupporter(Declaration, UpdateActionCode.Add)) };
			}

			if (dataContextValue.Equals(new DataContextValue(CustomsDeliveryOrderDataContextValue)))
			{
				List<DeliveryOrderHeader> boDocDataProviders = new List<DeliveryOrderHeader>();

				foreach (DeliveryOrderHeader deliveryOrderHeader in Declaration.DeliveryOrderHeaders)
				{
					if (deliveryOrderHeader.US_ShouldPrint)
					{
						boDocDataProviders.Add(deliveryOrderHeader);
					}
				}

				return boDocDataProviders.ConvertAll(x => BODocDataProvider.Get(x)).ToArray();
			}

			return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		List<USInvoiceLineFSISLine> AddFSISDocuments(IEnumerable<USInvoiceLineFSISLine> lines)
		{
			List<USInvoiceLineFSISLine> boDocDataProviders = new List<USInvoiceLineFSISLine>();
			foreach (USInvoiceLineFSISLine fsisLine in lines)
			{
				boDocDataProviders.Add(fsisLine);
			}
			return boDocDataProviders;
		}

		IBODocDataProvider[] GetBODocDataFor7501Print()
		{
			var result = new List<IBODocDataProvider>();
			var entryHeader = Declaration.FormalEntry;
			var mostRecentClearedEntry = LastENSClearedEntry;

			if (MostRecentResponseMessage != null && !HasInvoiceBeenDeletedOrDetachedSinceLodged)
			{
				if (Declaration.IsACE)
				{
					result.Add(BODocDataProvider.Get(new ACEEntryMessage7501Print(entryHeader, mostRecentClearedEntry, MostRecentResponseMessage, GetLastAcceptedBLU(mostRecentClearedEntry.EM_SystemCreateTimeUtc))));
				}
				else
				{
					result.Add(BODocDataProvider.Get(new EntryMessageENS7501Print(entryHeader, mostRecentClearedEntry, MostRecentResponseMessage, GetLastAcceptedBLU(mostRecentClearedEntry.EM_SystemCreateTimeUtc))));
				}
			}

			if (result.Count == 0 && entryHeader != null)
			{
				if (entryHeader.IsACE)
				{
					result.Add(BODocDataProvider.Get(new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entryHeader, (line, printInvoiceHeading, printInvoiceDetails) => new ACEEntryHeaderENS7501Line(line, printInvoiceHeading, printInvoiceDetails))));
				}
				else
				{
					result.Add(BODocDataProvider.Get(new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entryHeader, (line, printInvoiceHeading, printInvoiceDetails) => new ACSEntryHeaderENS7501Line(line, printInvoiceHeading, printInvoiceDetails))));
				}
			}

			return result.ToArray();
		}

		MQEDIMessage LastENSClearedEntry
		{
			get
			{
				if (ensClearedEntry == null)
				{
					MQEDIMessage outgoingEntry = null;
					var entryHeader = Declaration.FormalEntry;
					if (entryHeader != null)
					{
						if (!Declaration.US_ManEntry && !entryHeader.HasBeenWithdrawn)
						{
							var outgoingMessageType = Declaration.IsACE ? ACEApplicationIdentifierCodeList.Codes.EntrySummary : ApplicationIdentifierCodeList.Codes.EntrySummary;
							var responseMessageType = Declaration.IsACE ? ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse : ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
							if (entryHeader.IsClearedEntry)
							{
								outgoingEntry = (MQEDIMessage)entryHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, outgoingMessageType, EDIMessage.Direction.Transmit);
							}
							else if (Declaration.ENSTransmitCount > Declaration.ENSRejectCount) // try and find an earlier message that was cleared
							{
								var transmittedMessages = from MQEDIMessage message in entryHeader.Messages
														  where message.EM_ApplicationCode == EDIMessage.ApplicationCodes.USCustomsImport
														  && (message.EM_MessageType == ApplicationIdentifierCodeList.Codes.EntrySummary || message.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummary)
														  && message.EM_ReceiveTransmit == EDIMessage.Direction.Transmit
														  orderby message.EM_SystemCreateTimeUtc descending
														  select message;

								foreach (MQEDIMessage message in transmittedMessages)
								{
									var responseMessage = (MQEDIMessage)entryHeader.Messages.GetMatchingMessage(EDIMessage.ApplicationCodes.USCustomsImport, responseMessageType, message.EM_MessageNum, EDIMessage.Direction.Receive);
									if (responseMessage != null)
									{
										if (responseMessage.IsENSCleared)
										{
											outgoingEntry = message;
											break;
										}
									}
								}
							}

							if (outgoingEntry != null)
							{
								var responseMessage = (EDIMessage)entryHeader.Messages.GetMatchingMessage(EDIMessage.ApplicationCodes.USCustomsImport, responseMessageType, outgoingEntry.EM_MessageNum, EDIMessage.Direction.Receive);
								if (responseMessage != null)
								{
									ensClearedEntry = outgoingEntry;
								}
							}
						}
					}
				}
				return ensClearedEntry;
			}
		}
		MQEDIMessage ensClearedEntry;

		MQEDIMessage MostRecentResponseMessage
		{
			get
			{
				if (mostRecentResponseMessage == null)
				{
					var entryHeader = Declaration.FormalEntry;
					var mostRecentClearedEntry = LastENSClearedEntry;

					if (mostRecentClearedEntry != null)
					{
						var messageType = Declaration.IsACE ? ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse : ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
						mostRecentResponseMessage = (MQEDIMessage)entryHeader.Messages.GetMatchingMessage(EDIMessage.ApplicationCodes.USCustomsImport, messageType, mostRecentClearedEntry.EM_MessageNum, EDIMessage.Direction.Receive);
					}
				}

				return mostRecentResponseMessage;
			}
		}
		MQEDIMessage mostRecentResponseMessage;

		StmALog MostRecentDeletedOrDetachedInvoiceLog
		{
			get
			{
				if (mostRecentDetachedInvoiceLog == null)
				{
					var logQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Invoice Detached");
					logQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Invoice Deleted");
					mostRecentDetachedInvoiceLog = Declaration.Logs.MostRecentLogByEventTime(Events.EditedARecord, logQuery);
				}

				return mostRecentDetachedInvoiceLog;
			}
		}
		StmALog mostRecentDetachedInvoiceLog;

		ZBool HasInvoiceBeenDeletedOrDetachedSinceLodged
		{
			get { return MostRecentDeletedOrDetachedInvoiceLog != null && MostRecentResponseMessage != null && MostRecentDeletedOrDetachedInvoiceLog.SL_PostedTimeUtc > MostRecentResponseMessage.EM_SystemCreateTimeUtc; }
		}

		EDIMessage GetLastAcceptedBLU(ZDateTime entryMessageCreateTime)
		{
			EDIMessage mostRecentAcceptedBLU = null;
			var maxDateTime = ZDateTime.MinSmallDateTimeValue;
			var transmittedMessages = Declaration.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ApplicationIdentifierCodeList.Codes.BillofLadingUpdate }, EDIMessage.Direction.Transmit);

			foreach (EDIMessage message in transmittedMessages)
			{
				if (message.EM_SystemCreateTimeUtc >= entryMessageCreateTime)
				{
					var isFailure = false;
					var responseMessage = (MQEDIMessage)Declaration.Messages.GetMatchingMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse, message.EM_MessageNum, EDIMessage.Direction.Receive);
					if (responseMessage != null)
					{
						foreach (MessageBlock block in responseMessage.MessageBlock.MessageBlocks)
						{
							var boll7 = block as BOLL7;
							if (boll7 != null)
							{
								isFailure |= IsRejectedCode(boll7.ErrorMessageIdentifier);
							}
						}

						if (!isFailure && message.EM_SystemCreateTimeUtc > maxDateTime)
						{
							maxDateTime = message.EM_SystemCreateTimeUtc;
							mostRecentAcceptedBLU = message;
						}
					}
				}
			}

			if (Declaration.IsACE && !Declaration.IsSplitShipment && !Declaration.US_NonAMS)
			{
				var seEntry = Declaration.ActiveEntryHeaders.SimplifiedEntry;
				if (seEntry != null)
				{
					var aceBLU = GetLastAcceptedACECargoReleaseBLU(seEntry.Messages, entryMessageCreateTime);
					if (aceBLU != null)
					{
						mostRecentAcceptedBLU = aceBLU;
					}
				}
			}

			return mostRecentAcceptedBLU;
		}

		EDIMessage GetLastAcceptedACECargoReleaseBLU(EDIMessageCollection messages, ZDateTime entryMessageCreateTime)
		{
			EDIMessage mostRecentAcceptedBLU = null;
			var maxDateTime = ZDateTime.MinSmallDateTimeValue;
			var transmittedMessages = messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ACEApplicationIdentifierCodeList.Codes.CargoRelease }, EDIMessage.Direction.Transmit);
			foreach (EDIMessage message in transmittedMessages)
			{
				if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate && message.EM_SystemCreateTimeUtc >= entryMessageCreateTime)
				{
					var isAccepted = false;
					var responseMessage = message.ResponseMessage;
					if (responseMessage != null)
					{
						var se90Blocks = responseMessage.MessageBlock.MessageBlocks.OfType<ASESE90>();
						isAccepted = se90Blocks.Any(x => SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted(x.MessageTypeCode));
						if (isAccepted && message.EM_SystemCreateTimeUtc > maxDateTime)
						{
							maxDateTime = message.EM_SystemCreateTimeUtc;
							mostRecentAcceptedBLU = message;
						}
					}
				}
			}
			return mostRecentAcceptedBLU;
		}

		bool IsRejectedCode(string code)
		{
			return code == ACSABIProcessor.Constants.TransactionDataRejected;
		}

		IBODocDataProvider[] GetBODocDataForPGARecapPrint()
		{
			List<IBODocDataProvider> result = new List<IBODocDataProvider>();

			if (Declaration.IsImport && Declaration.IsACECargoCertificationMode && Declaration.ActiveEntryHeaders.Count > 0)
			{
				var entryLineMessageBlockCollection = EntryLineMessageBlockCollection;
				if (entryLineMessageBlockCollection.Any())
				{
					result.Add(BODocDataProvider.Get(new DeclarationMessageDataPrint(Declaration, entryLineMessageBlockCollection)));
				}
			}
			return result.ToArray();
		}

		IBODocDataProvider[] GetBODocDataForFTZ214Print()
		{
			List<IBODocDataProvider> result = new List<IBODocDataProvider>();

			if (Declaration.IsFTZAdmission && Declaration.FTZEntry != null)
			{
				Declaration.FTZEntry.CalculateFTZZoneStatuses();
				Declaration.FTZ214PrintCollection.PopulateElements();
				result.Add(BODocDataProvider.Get(Declaration.FTZEntry));
			}

			return result.ToArray();
		}

		IBODocDataProvider[] GetBODocDataFor7512Print(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			List<IBODocDataProvider> result = new List<IBODocDataProvider>();

			var parent = Declaration.Shipment ?? (Freight.Integration.ICusInBondParent)Declaration;
			var inBondHeader = parent.GetInBondHeader(CusInBondApplicationCodeList.Codes.InBond);

			if (inBondHeader != null)
			{
				result.AddRange(((IDocumentSupportable)inBondHeader).DocumentSupporter.GetBODocDataProviders(dataContextValue, commandBeingRun));
			}

			return result.ToArray();
		}

		IBODocDataProvider[] GetBODocDataForAESPrint()
		{
			var result = new List<IBODocDataProvider>();
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				IAESMessagePrint mostRecentClearedEntryMessage = null;

				if (!entryHeader.IsCurrentlyWithdrawn)
				{
					if (entryHeader.IsClearedEntry)
					{
						mostRecentClearedEntryMessage = GetMostRecentClearedMsgInClearedAESEntry(entryHeader.Messages);
					}
					else
					{
						mostRecentClearedEntryMessage = GetMostRecentClearedMsgInAESEntryWithActiveCustomsTransactions(entryHeader.Messages);
					}
				}

				AESPrint aESPrint;
				if (mostRecentClearedEntryMessage != null)
				{
					aESPrint = new AESMessagePrint(mostRecentClearedEntryMessage);
				}
				else
				{
					aESPrint = new AESHeaderPrint(entryHeader);
				}

				result.Add(BODocDataProvider.Get(aESPrint));
			}
			return result.ToArray();
		}

		IAESMessagePrint GetMostRecentClearedMsgInAESEntryWithActiveCustomsTransactions(EDIMessageCollection messages)
		{
			var maxDateTime = ZDateTime.MinSmallDateTimeValue;
			IAESMessagePrint mostRecentClearedEntryMessage = null;
			var transmittedMessages = messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsExport, new ZString[] { ApplicationIdentifierCodeList.AES.CommodityShipment }, EDIMessage.Direction.Transmit);
			foreach (var message in transmittedMessages)
			{
				var isFailure = false;
				var responseMessage = messages.GetMatchingMessage(EDIMessage.ApplicationCodes.USCustomsExport, ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, message.EM_MessageNum, EDIMessage.Direction.Receive);
				if (responseMessage != null)
				{
					isFailure = responseMessage.EM_MessageText.Contains("REJECTED") || responseMessage.EM_MessageText.Contains("CANCELLED");

					if (!isFailure && message.EM_SystemCreateTimeUtc > maxDateTime)
					{
						maxDateTime = message.EM_SystemCreateTimeUtc;
						mostRecentClearedEntryMessage = (AESTIREDIMessage)message;
					}
				}
			}
			return mostRecentClearedEntryMessage;
		}

		IAESMessagePrint GetMostRecentClearedMsgInClearedAESEntry(EDIMessageCollection messages)
		{
			return (AESTIREDIMessage)messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsExport, ApplicationIdentifierCodeList.AES.CommodityShipment, EDIMessage.Direction.Transmit);
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;

			if (dataContext == DataContext.CusEntryHeader)
			{
				result = new DocumentWrapper[Declaration.CustomsEntryHeaders.Count];

				for (int i = 0; i < Declaration.CustomsEntryHeaders.Count; i++)
				{
					result[i] = DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.US.DocumentWrappers.DocCusEntryHeader", Declaration.CustomsEntryHeaders[i], "Enterprise.Customs.US.DocumentWrappers");
				}
			}
			else if (Declaration.IsExport && dataContext == DataContext.GenericCommercialInvoice)
			{
				result = new DocumentWrapper[Declaration.Invoices.Count];

				for (int i = 0; i < Declaration.Invoices.Count; i++)
				{
					result[i] = DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.US.DocumentWrappers.USExportCommercialInvoiceWrapper", Declaration.Invoices[i], "Enterprise.Customs.US.DocumentWrappers");
				}
			}
			else if (dataContext == DataContext.ComInvoiceHeader)
			{
				result = new DocumentWrapper[Declaration.Invoices.Count];

				for (int i = 0; i < Declaration.Invoices.Count; i++)
				{
					result[i] = DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.US.DocumentWrappers.DocJobComInvoiceHeader", Declaration.Invoices[i], "Enterprise.Customs.US.DocumentWrappers");
				}
			}
			else if (dataContext == DataContext.Declaration)
			{
				result = new[] { DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.US.DocumentWrappers.DocDeclaration", Declaration, "Enterprise.Customs.US.DocumentWrappers") };
			}
			else
			{
				result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}

			return result;
		}

		public override string TransportMode
		{
			get
			{
				var result = base.TransportMode;
				if (Declaration.JE_TransportMode == TransportTypeList.Codes.Truck)
				{
					result = Core.Constants.TransportModes.Road;
				}

				return result;
			}
		}

		internal IEnumerable<(ICusEntryLine entryLine, List<MessageBlock> messages)> EntryLineMessageBlockCollection
		{
			get
			{
				var entryMessageBlockCollection = new List<(ICusEntryLine cusEntryLine, List<MessageBlock> messages)>();

				var entry = (IACECusEntryHeader)(Declaration.ActiveEntryHeaders.SimplifiedEntry ?? Declaration.ActiveEntryHeaders.EntrySummaryEntry);
				if (entry != null)
				{
					foreach (ISimplifiedEntryLine entryLine in entry.EntryLines)
					{
						var messages = new List<MessageBlock>();
						new SimplifiedEntryBlockBuilder((IACECargoReleaseHeader)entry, null).GenerateEntryLine(entryLine, messages);

						var pgaBlocks = SimplifiedEntryBlockBuilder.GenerateEntryLinePGABlocks(entryLine, new PGARecapPrintSigned(Declaration), false, true).ToList();
						messages.AddRange(pgaBlocks);
						entryMessageBlockCollection.Add((entryLine, messages));
					}
				}
				return entryMessageBlockCollection;
			}
		}

		#endregion

	}
}
