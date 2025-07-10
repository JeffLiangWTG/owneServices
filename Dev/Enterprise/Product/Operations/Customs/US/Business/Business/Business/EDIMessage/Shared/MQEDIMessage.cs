using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public interface IEntryStatusProvider
	{
		ZString EntryStatus { get; }
	}

	public class MQEDIMessage : EDIMessage
		, Integration.Customs.US.IMQEDIMessage
		, ICusCodeDataTypeSupporter
		, IEntryStatusProvider
		, IVisaQuery
		, IControllerIDProvider
	{
		public MQEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly MQEDIMessageTypeDecider TypeDecider = new MQEDIMessageTypeDecider();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : EDIMessage.Schema
		{
			public const string EM_ActionStatus = "EM_ActionStatus";
			public const string CountOfReconOriginalEntries = "CountOfReconOriginalEntries";
			public const string MessageStatusToShowInQueryModule = "MessageStatusToShowInQueryModule";
		}

		#region Action Status
		public bool IsComplete
		{
			get { return EM_ActionStatus != EM_ActionStatusList.Codes.Incomplete; }
		}

		public ZString EM_ActionStatus
		{
			get
			{
				if (eM_ActionStatusCached == null)
				{
					eM_ActionStatusCached = new CachedProperty<ZString>(Factory, delegate
					{
						ZString result = EM_ActionStatusList.Codes.Incomplete;
						StmALog mostRecentLog = ActionStatusLogs.MostRecentLog;
						if (mostRecentLog != null)
						{
							result = mostRecentLog.SL_Reference;
						}
						else if (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification && EM_ApplicationReference.Length <= 2)
						{
							result = EM_ActionStatusList.Codes.Complete;
						}

						return result;
					}
					);
				}
				return eM_ActionStatusCached.Value;
			}
		}
		CachedProperty<ZString> eM_ActionStatusCached;

		public ZPropertyInfo EM_ActionStatusInfo
		{
			get { return GetZPropertyInfo(Schema.EM_ActionStatus); }
		}

		LogsForNominatedEvent ActionStatusLogs
		{
			get { return actionStatusLogs ?? (actionStatusLogs = new LogsForNominatedEvent(Logs, Events.Authorised)); }
		}
		LogsForNominatedEvent actionStatusLogs;

		public void SetToComplete()
		{
			if (!IsComplete)
			{
				ActionStatusLogs.AddNew(EM_ActionStatusList.Codes.Complete);
			}
		}

		public ZString ActionLogNKUser
		{
			get
			{
				var result = ZString.Empty;
				if (ActionStatusLogs.MostRecentLog != null)
				{
					result = ActionStatusLogs.MostRecentLog.SL_GS_NKUser;
				}
				return result;
			}
		}

		public ZDateTime ActionLogEventTime
		{
			get
			{
				var result = ZDateTime.Empty;
				if (ActionStatusLogs.MostRecentLog != null)
				{
					result = ActionStatusLogs.MostRecentLog.SL_EventTime;
				}
				return result;
			}
		}

		public bool ActionAuthorised
		{
			get { return ActionStatusLogs.MostRecentLog != null || (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification && EM_ApplicationReference.Length <= 2 && !EM_ApplicationReference.IsEmpty); }
		}

		#endregion

		#region New Properties

		public bool HasFDADetails
		{
			get
			{
				//OGA FD01 is mandatory
				return MessageBlock.MessageBlocks.OfType<OGAFD01>().Any();
			}
		}

		public bool IsAIICleared
		{
			get
			{
				return !MessageBlock.MessageBlocks.OfType<AIIE0195>()
													.Any(block => block.ErrorMessageIdentifier == MessageProcessors.ACSABIProcessor.Constants.TransactionDataRejected);
			}
		}

		internal bool IsBLU
		{
			get
			{
				return EM_MessageType == ApplicationIdentifierCodeList.Codes.BillofLadingUpdate ||
					EM_MessageType == ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse ||
					EM_MessageType == ApplicationIdentifierCodeList.Codes.BillofLadingProcessingResults;
			}
		}

		internal bool IsACEBLU
		{
			get
			{
				return (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoRelease ||
					EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse) &&
					EM_MessageSubType == EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			}
		}

		internal bool HasCensusWarnings
		{
			get
			{
				if (!hasCensusWarningCached.HasValue)
				{
					hasCensusWarningCached = false;

					var entrySummaryResponseMessageType = Declaration?.IsACSCargoCertificationMode ?? false ? ApplicationIdentifierCodeList.Codes.EntrySummaryResponse : ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
					if (EM_MessageType == entrySummaryResponseMessageType && !IsTransmitMessage)
					{
						hasCensusWarningCached = MessageBlock.MessageBlocks.OfType<ENSEXX>()
																			 .Any(block => IsCensusWarningCode(block.ErrorMessageIdentifier));
					}
				}

				return hasCensusWarningCached.Value;
			}
		}
		bool? hasCensusWarningCached;

		internal bool IsCensusWarningCode(string code)
		{
			return
				code == CensusWarnings._14J ||
				code == CensusWarnings._27A ||
				code == CensusWarnings._27B ||
				code == CensusWarnings._27C ||
				code == CensusWarnings._27D ||
				code == CensusWarnings._27E ||
				code == CensusWarnings._27F ||
				code == CensusWarnings._27G ||
				code == CensusWarnings._27H ||
				code == CensusWarnings._27I ||
				code == CensusWarnings._27J ||
				code == CensusWarnings._27K ||
				code == CensusWarnings._27L ||
				code == CensusWarnings._27M ||
				code == CensusWarnings._27P ||
				code == CensusWarnings._27Q ||
				code == CensusWarnings._27R ||
				code == CensusWarnings._27S ||
				code == CensusWarnings._27T ||
				code == CensusWarnings._27U ||
				code == CensusWarnings._27V ||
				code == CensusWarnings._27W ||
				code == CensusWarnings._27X ||
				code == CensusWarnings._27Y ||
				code == CensusWarnings._27Z ||
				code == CensusWarnings._28N ||
				code == CensusWarnings._28O ||
				code == CensusWarnings._28A ||
				code == CensusWarnings._28B ||
				code == CensusWarnings._28C ||
				code == CensusWarnings._28D ||
				code == CensusWarnings._28E ||
				code == CensusWarnings._28F ||
				code == CensusWarnings._28G ||
				code == CensusWarnings._28H ||
				code == CensusWarnings._28I ||
				code == CensusWarnings._28J ||
				code == CensusWarnings._28K ||
				code == CensusWarnings._28L ||
				code == CensusWarnings._4BW;
		}

		public bool IsProtestCleared
		{
			get
			{
				if (!isProtestClearedCached.HasValue)
				{
					isProtestClearedCached = false;
					if (EM_MessageType == ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse && !IsTransmitMessage)
					{
						isProtestClearedCached =
							MessageBlock.MessageBlocks.OfType<PROP01>()
														.Any(block => block.ResponseMessage.ToUpper().Contains(ProtestAccepted));
					}
				}
				return isProtestClearedCached.Value;
			}
		}
		bool? isProtestClearedCached;
		protected const string ProtestAccepted = "PROTEST FILING ACCEPTED ERROR FREE";

		public bool IsENSCleared
		{
			get
			{
				if (!isENSClearedCached.HasValue)
				{
					isENSClearedCached = false;

					var entrySummaryResponseMessageType = Declaration?.IsACSCargoCertificationMode ?? false ? ApplicationIdentifierCodeList.Codes.EntrySummaryResponse : ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
					if (EM_MessageType == entrySummaryResponseMessageType && !IsTransmitMessage)
					{
						isENSClearedCached =
							!MessageBlock.MessageBlocks.OfType<IStatusesAndErrors>()
														 .Any(block => block.NarrativeMessage == USConstants.NarrativeRejectedMessage ||
																	 block.NarrativeMessage == USConstants.BatchRejected ||
																	 block.Code == MessageProcessors.ACEABIProcessor.Constants.TransactionDataRejected);
					}
				}

				return isENSClearedCached.Value;
			}
		}
		bool? isENSClearedCached;

		public bool IsBIRDTransaction
		{
			get
			{
				var birdApplicationCodesList = new List<string>(ApplicationIdentifierCodeList.GetBIRDApplicationIdentifierCodes());
				birdApplicationCodesList.Add(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction);
				return birdApplicationCodesList.Contains(EM_MessageType);
			}
		}

		internal ZBool IsCargoManifestQuery
		{
			get { return EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery; }
		}

		public override ZString CountOfReconOriginalEntries
		{
			get
			{
				if (countOfReconOriginalEntriesCached == null)
				{
					countOfReconOriginalEntriesCached = new CachedProperty<ZString>(Factory, delegate
					{
						int result = 0;
						if (EM_MessageType == ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling &&
							EM_ReceiveTransmit == Direction.Transmit)
						{
							result = GetMessageBlocks<RECR20>().Count;
						}
						return result > 0 ? result.ToString() : "";
					});
				}
				return countOfReconOriginalEntriesCached.Value;
			}
		}
		CachedProperty<ZString> countOfReconOriginalEntriesCached;

		public ZPropertyInfo CountOfReconOriginalEntriesInfo
		{
			get { return GetZPropertyInfo(Schema.CountOfReconOriginalEntries); }
		}

		public ZBool IsACEEntrySummaryResponse
		{
			get { return EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse; }
		}

		internal ZBool IsEntrySummaryQuery
		{
			get
			{
				return EM_MessageType == ApplicationIdentifierCodeList.Codes.QueryEntrySummary ||
					EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery ||
					EM_MessageType == ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery;
			}
		}

		internal bool IsEntrySummaryQuerySuccessful
		{
			get
			{
				if (!isEntrySummaryQuerySuccessfulCached.HasValue)
				{
					isEntrySummaryQuerySuccessfulCached = false;
					if (!IsTransmitMessage)
					{
						if (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse || EM_MessageType == ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse)
						{
							isEntrySummaryQuerySuccessfulCached = MessageBlock.MessageBlocks.OfType<AENQJZ>().FirstOrDefault() == null;
						}
						else if (EM_MessageType == ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse)
						{
							isEntrySummaryQuerySuccessfulCached = MessageBlock.MessageBlocks.OfType<MessageBuildingBlocks.Output.ENQJ9>()
								.FirstOrDefault(x => x.IsFailed()) == null;
						}
					}
				}
				return isEntrySummaryQuerySuccessfulCached.Value;
			}
		}
		bool? isEntrySummaryQuerySuccessfulCached;

		public ZString MessageStatusToShowInQueryModule
		{
			get
			{
				if (!fEMStatusWithResponseCashed.HasValue)
				{
					fEMStatusWithResponseCashed = ZString.Empty;
					if (HasRelatedMessage)
					{
						fEMStatusWithResponseCashed = RelatedMessage.EM_Status;
					}
					else
					{
						fEMStatusWithResponseCashed = EM_Status;
					}
				}
				return fEMStatusWithResponseCashed.Value;
			}
		}
		ZString? fEMStatusWithResponseCashed;

		public ZPropertyInfo MessageStatusToShowInQueryModuleInfo
		{
			get { return GetZPropertyInfo(Schema.MessageStatusToShowInQueryModule); }
		}

		public ZDecimal TotalENSAmountDue
		{
			get
			{
				if (!totalENSAmountDueCached.HasValue)
				{
					totalENSAmountDueCached = ZDecimal.Zero;
					var isACE = EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummary;

					if (IsTransmitMessage &&
						(EM_MessageType == ApplicationIdentifierCodeList.Codes.EntrySummary || isACE))
					{
						var block10 = MessageBlock.MessageBlocks.OfType<IENS10>().FirstOrDefault();
						var entryType = block10 != null ? block10.EntryType : ZString.Empty;

						if (!EntryTypeList.IsNothingPayable(entryType))
						{
							if (EntryTypeList.IsOnlyHMFPayable(entryType))
							{
								if (isACE)
								{
									var ens89s = new TypedEnumerable<AENS89>(MessageBlock.MessageBlocks.OfType<AENS89>());
									totalENSAmountDueCached = ens89s.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF);
								}
								else
								{
									var ens89s = new TypedEnumerable<ENS89>(MessageBlock.MessageBlocks.OfType<ENS89>());
									totalENSAmountDueCached = ens89s.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF);
								}
							}
							else
							{
								var block90 = MessageBlock.MessageBlocks.OfType<IENS90>().FirstOrDefault();
								if (block90 != null)
								{
									var deferred = isACE ? !((IENSDeferredTaxIndicator)block10).DeferredTaxIndicator.IsEmpty :
															((IENSDeferredTaxIndicator)block90).DeferredTaxIndicator != TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax;
									totalENSAmountDueCached = block90.GetTotal(deferred);
								}
							}
						}
					}
				}
				return totalENSAmountDueCached.Value;
			}
		}
		ZDecimal? totalENSAmountDueCached;

		public bool IsFTZCleared
		{
			get
			{
				if (!isFTZClearedCached.HasValue)
				{
					isFTZClearedCached = false;
					if (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone && !IsTransmitMessage)
					{
						isFTZClearedCached =
							!MessageBlock.MessageBlocks.OfType<FTZNF95>()
														 .Any(block => ExtractFTZResponseResult.IsFailure(block.NarrativeMessageTypeCode) ||
																	 ExtractFTZResponseResult.IsUnauthorized(block.ErrorCode));
					}
				}
				return isFTZClearedCached.Value;
			}
		}
		bool? isFTZClearedCached;

		public bool IsFTZAcceptedWithCensusWarning
		{
			get
			{
				if (!isFTZAcceptedWithCensusWarningCached.HasValue)
				{
					isFTZAcceptedWithCensusWarningCached = false;
					if (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone && !IsTransmitMessage)
					{
						isFTZAcceptedWithCensusWarningCached = MessageBlock.MessageBlocks.OfType<FTZNF95>().Any(block => IsCensusWarningCode(block.ErrorCode));
					}
				}
				return isFTZAcceptedWithCensusWarningCached.Value;
			}
		}
		bool? isFTZAcceptedWithCensusWarningCached;

		public ZDateTime DispositionDateTime
		{
			get
			{
				if (!dispositionDateTime.HasValue)
				{
					dispositionDateTime = EM_MessageDateTime;
					if (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)
					{
						var nf91 = MessageBlock.MessageBlocks.OfType<FTZNF91>().FirstOrDefault();
						if (nf91 != null)
						{
							dispositionDateTime = DateTimeParser.GetDateTimeFromZDateAndStringTime(nf91.ActionDate, nf91.ActionTime);
						}
					}
				}
				return dispositionDateTime.Value;
			}
		}
		ZDateTime? dispositionDateTime;

		#endregion

		#region Overriden Properties

		public override ZDateTime EM_SystemCreateTimeUtc
		{
			get
			{
				return base.EM_SystemCreateTimeUtc;
			}
			set
			{
				base.EM_SystemCreateTimeUtc = value;

				var declaration = Declaration;
				if (EM_ReceiveTransmit == EDIMessage.Direction.Transmit && declaration != null && declaration.US_CertReqDate.IsEmpty)
				{
					if (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoRelease
						|| EM_MessageType == ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions
						|| EM_MessageType == ApplicationIdentifierCodeList.Codes.BorderCargoRelease
						|| (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummary && MessageBlock.MessageBlocks.OfType<AENS10>().Any(x => x.CargoReleaseCertificationRequestIndicator == "Y"))
						|| (EM_MessageType == ApplicationIdentifierCodeList.Codes.EntrySummary && MessageBlock.MessageBlocks.OfType<ENS30>().Any(x => x.ReleaseCertificationCode == 1))
						)
					{
						declaration.US_CertReqDate = value;
					}
				}
			}
		}

		public override ZString EM_MessageTextDetail
		{
			get
			{
				if (fEM_MessageTextDetail == null)
				{
					var shouldDisplayInformationText = false;
					if (!Env.Security.OrgDetailsViewPersonalInformation.IsAllowed)
					{
						var enumerator = MessageBlock.MessageBlocks.GetEnumerator();
						while (!shouldDisplayInformationText && enumerator.MoveNext())
						{
							var block = enumerator.Current;
							foreach (var field in block.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
							{
								var stringAttributes = (MessageBlockStringAttribute[])field.GetCustomAttributes(typeof(MessageBlockStringAttribute), false);
								if (stringAttributes.Length == 1 && stringAttributes[0].IsPersonalInformation && ((ZString)field.GetValue(block)) == SocialSecurityNumberValidator.SSNWithMask)
								{
									shouldDisplayInformationText = true;
									break;
								}
							}
						}
					}

					fEM_MessageTextDetail = shouldDisplayInformationText ?
						$"There is personal information in this Message Text and you are not authorized to see it based on security: {Env.Security.OrgDetailsViewPersonalInformation.DisplayTextPathToSecurityRight}."
						: (string)base.EM_MessageTextDetail;
				}

				return fEM_MessageTextDetail;
			}
		}
		string fEM_MessageTextDetail;

		public new MQEDIMessage OriginalMessage
		{
			get { return (MQEDIMessage)base.OriginalMessage; }
		}

		public new MQEDIMessage RelatedMessage
		{
			get { return (MQEDIMessage)base.RelatedMessage; }
		}

		public new MQEDIMessage ResponseMessage
		{
			get { return (MQEDIMessage)base.ResponseMessage; }
		}

		protected override BlockControlGenerator GetMessageBlock()
		{
			BlockControlGenerator result = null;
			ZString applicationIdentifier = EM_MessageType;
			ZString messageText = EM_MessageText;
			if (applicationIdentifier.IsEmpty)
			{
				applicationIdentifier = messageText.SubstringSafe(11, 2);
			}

			if (applicationIdentifier.IsEmpty)
			{
				if (messageText.StartsWith("B".PadRight(79) + "1"))
				{
					applicationIdentifier = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
				}
			}

			if (EM_ApplicationCode == ApplicationCodes.USCustomsExport && ApplicationIdentifierCodeList.AES.IsAESApplicationCode(applicationIdentifier))
			{
				result = GetAESMessageBlock(applicationIdentifier);
			}
			else if (Factory.GetCachedValue<ACEApplicationIdentifierCodeList>().ContainsCode(applicationIdentifier))
			{
				result = GetACEMessageBlock(applicationIdentifier);
			}
			else
			{
				if (IsTransmitMessage)
				{
					result = IsBIRDTransaction ? new ABIInputBlockControlGenerator<BRDAA, BRDZZ>() : new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
				}
				else
				{
					result = IsBIRDTransaction ? new ABIOutputBlockControlGenerator<BRDAA, BRDZZ>() : new ABIOutputBlockControlGenerator(applicationIdentifier);
				}
			}

			if (result != null && !messageText.IsEmpty)
			{
				result.Deserialise(BlockPadder.Pad(messageText), EM_MessageOwner.EqualsIgnoringCase(Constants.ACE) ? Constants.ACE : string.Empty);

				PopulateLineNumberForACEStatusesAndErrorsFromMessage(result);
			}

			return result;
		}

		protected override Messaging.Business.StatusErrorsDataViewCollection GetStatusErrorsDataViewCollection()
		{
			var statusesAndErrors = new StatusErrorsDataViewCollection(Factory);
			var errorBlocks = MessageBlock.MessageBlocks.OfType<IStatusesAndErrors>();
			foreach (var block in errorBlocks)
			{
				var record = new ErrorsRecord();
				record.LineNumber = block.LineNumber;
				record.NarrativeMessage = block.NarrativeMessage.Left(40);
				record.ErrorMessageIdentifier = block.Code;

				var tariffNumberStatus = block as ITariffNumberStatusAndErrors;
				if (tariffNumberStatus != null)
				{
					record.TariffNumber = tariffNumberStatus.TariffNumber;
					record.PGAAgencyCode = tariffNumberStatus.PGAAgencyCode;
					record.PGALine = tariffNumberStatus.PGALineNo;
				}

				record.BlockText = ((MessageBlock)block).Serialise(true);

				statusesAndErrors.Add(record);
			}
			return statusesAndErrors;
		}

		void PopulateLineNumberForACEStatusesAndErrorsFromMessage(BlockControlGenerator block)
		{
			if (IsACEEntrySummaryResponse)
			{
				Dictionary<string, AENSE0> dataReferenceBlocks = new Dictionary<string, AENSE0>();

				foreach (MessageBlock msgblock in block.MessageBlocks)
				{
					AENSE0 e0 = msgblock as AENSE0;
					if (e0 != null)
					{
						dataReferenceBlocks[e0.ReferenceDataTypeCode] = e0;

						switch (e0.ReferenceDataTypeCode)
						{
							case EntrySummaryReferenceDataList.Codes.TOTALS:
							case EntrySummaryReferenceDataList.Codes.FEETOT:
								dataReferenceBlocks.Remove(EntrySummaryReferenceDataList.Codes.LINITM);
								dataReferenceBlocks.Remove(EntrySummaryReferenceDataList.Codes.TARIFF);
								break;

							case EntrySummaryReferenceDataList.Codes.LINITM: // a new line has started
								dataReferenceBlocks.Remove(EntrySummaryReferenceDataList.Codes.TARIFF);
								break;
						}
					}
					else
					{
						AENSE1 e1 = msgblock as AENSE1;
						if (e1 != null && e1.DispositionTypeCode.IsEmpty)
						{
							var lineNumber = dataReferenceBlocks.GetEntryLineNumber();
							if (lineNumber > 0)
							{
								e1.LineNumber = (ZShort)lineNumber;
							}

							e1.TariffNumber = dataReferenceBlocks.GetTariffNumber();
							e1.PGAAgencyCode = dataReferenceBlocks.GetPGAAgencyCode();
							e1.PGALineNo = dataReferenceBlocks.GetPGALineNo();
						}
					}
				}
			}
			else if (IsCargoReleaseResponse)
			{
				var lineNo = ZString.Empty;
				var tariffNumber = ZString.Empty;
				var agencyCode = ZString.Empty;
				var pgaLine = ZString.Empty;
				foreach (MessageBlock msgblock in block.MessageBlocks)
				{
					var se40 = msgblock as ASESE40;
					if (se40 != null)
					{
						lineNo = se40.LineItemIdentifier.ToString();
					}
					else
					{
						var se60 = msgblock as ASESE60;
						if (se60 != null)
						{
							tariffNumber = se60.HTSNumber;
						}
						else
						{
							var ioblock = msgblock as AENSOI;
							if (ioblock != null)
							{
								agencyCode = ZString.Empty;
								pgaLine = ZString.Empty;
							}
							else
							{
								var pg01 = msgblock as AEPAPG01;
								if (pg01 != null)
								{
									agencyCode = pg01.GovernmentAgencyCode;
									pgaLine = pg01.PGALineNumber.ToString();
								}
								else
								{
									var errorBlock = msgblock as ASESE90;
									if (errorBlock != null && !errorBlock.MessageIdentifierCode.IsEmpty)
									{
										errorBlock.LineNumber = lineNo;
										errorBlock.TariffNumber = tariffNumber;
										errorBlock.PGAAgencyCode = agencyCode;
										errorBlock.PGALineNo = pgaLine;
									}
								}
							}
						}
					}
				}
			}
		}

		ZBool IsCargoReleaseResponse
		{
			get { return EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse; }
		}

		#endregion

		#region Message Type Desc

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return new EM_MessageSubTypeList(); }
		}

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase && EM_ApplicationCode == ApplicationCodes.USCustomsImport)
			{
				var messageAttachee = EM_LinkedObject as IMessageAttacheeInDeclaration;

				if (messageAttachee != null)
				{
					var declaration = Factory.Load<JobDeclaration>(messageAttachee.DeclarationPK);

					if (declaration != null)
					{
						declaration.InBondRelatedRecords.RebuildIfNecessary(messageAttachee, IsBIRDTransaction);
						declaration.EntryStatusesAndErrors.RefreshWhenANewMessageIsSaved();
					}
				}

				if (EM_ReceiveTransmit == Direction.Transmit && IsBIRDTransaction && EM_Status == Status.Queued)
				{
					EM_Status = EDIMessage.Status.Sent;//should not be attempted to be sent to Customs
				}
			}
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					if (EM_LinkedObject != null
						&& EM_LinkedObject is IMessageAttacheeInDeclaration messageAttachee
						&& !messageAttachee.DeclarationPK.IsEmpty)
					{
						declaration = Factory.Load<JobDeclaration>(messageAttachee.DeclarationPK);
					}
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#region Implementation

		BlockControlGenerator GetACEMessageBlock(ZString applicationIdentifier)
		{
			BlockControlGenerator result = null;
			if (IsTransmitMessage)
			{
				result = new ABIInputBlockControlGenerator<AABIInputB, AABIInputY>(GlbBranch.CurrentBranch);
			}
			else
			{
				result = new ABIOutputBlockControlGenerator<AABIOutputB, AABIOutputY>(EM_MessageType);
			}

			return result;
		}

		BlockControlGenerator GetAESMessageBlock(ZString applicationIdentifier)
		{
			BlockControlGenerator result = null;
			if (IsTransmitMessage)
			{
				result = new AESInputBlockControlGenerator();
			}
			else
			{
				switch (applicationIdentifier)
				{
					case ApplicationIdentifierCodeList.AES.CommodityShipmentWarningReminder:
						result = new ExportOutputBlockControlGenerator<AESCommWarnBXN, AESCommWarnYXN>();
						break;
					default:
						result = new ExportOutputBlockControlGenerator<AESCommShipBXT, AESCommShipYXT>();
						break;
				}
			}
			return result;
		}

		internal virtual void UpdateErrorFlag()
		{
			EM_SendWithMessageErrors = IsMessageSendWithMessageErrorsCorrectCore();
		}

		protected override bool IsMessageSendWithMessageErrorsCorrectCore()
		{
			var result = base.IsMessageSendWithMessageErrorsCorrectCore();
			BusinessObject parent = EM_LinkedObject;
			var notifications = new List<INotification>(parent.GetMessageErrors());
			if (notifications.Count > 0)
			{
				foreach (INotification notification in notifications.ToArray())
				{
					if (ShouldBeIgnored(notification.Message))
					{
						notifications.Remove(notification);
					}
				}
			}
			if (notifications.Count == 0)
			{
				result = false;
			}
			return result;
		}

		bool ShouldBeIgnored(string messageError)
		{
			return messageError.EndsWith(OrganisationValidation.OrganisationShouldBeRegisteredInCustoms)
				|| messageError == FormalImportAddInfoJobDeclarationValidation.FeeApplicableForSea
				|| messageError == FormalImportAddInfoJobDeclarationValidation.FeeApplicableForBWBMOT;
		}

		protected override string GetMessageReferenceNumber()
		{
			var company = Company ?? GlbCompany.CurrentCompany;
			return company.LicenceKeyIdentifier + "_" + Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EDIMessage.ApplicationCodes.USCustomsImport).GetNextFormatted(Factory);
		}

		bool IsACE
		{
			get { return EM_MessageOwner == Constants.ACE; }
		}

		protected override string GetMessageBlockApplicationCodeCore()
		{
			var result = base.GetMessageBlockApplicationCodeCore();
			if (IsACE)
			{
				result = Constants.ACE;
			}
			else if (!IsTransmitMessage)
			{
				var originalMessage = OriginalMessage;
				if (originalMessage != null)
				{
					result = originalMessage.GetMessageBlockApplicationCode();
				}
			}
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = MQEDIMessage.ApplicationCodes.USCustomsImport;
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			List<Type> result = new List<Type>();
			result.Add(typeof(OrgHeader));
			result.Add(typeof(OrgAddress));
			result.Add(typeof(CusContainer));
			result.Add(typeof(Bill));
			result.Add(typeof(CusStatementHeader));
			result.Add(typeof(JobComInvoiceHeader));
			result.Add(ObjectFactory.GetType<Integration.Forwarding.IForwardingConsol>());
			result.Add(ObjectFactory.GetType<Integration.Forwarding.IForwardingShipment>());
			result.Add(typeof(CusLiquidation));
			result.Add(ObjectFactory.GetType<Integration.Customs.US.InBond.ICusInBondMoveHeader>());
			result.Add(ObjectFactory.GetType<Integration.Customs.US.InBond.ICusInBondBill>());
			result.Add(ObjectFactory.GetType<Integration.Customs.US.InBond.ICusInBondContainer>());
			result.Add(ObjectFactory.GetType<Integration.Customs.US.LVS.ICusUSLVConsignment>());
			return result;
		}

		public const string InBondNumberPlaceHolder = "<ITNOPLC_HD>";//12 characters long 
		public const string AirInBondNumberPlaceHolder = "<ITNOPLC>";//9 characters long
		public const string USEntryNumberPlaceHolder = "<E#PLCH>";//8 characters long as mention in BCR01
		public const string USEntryFilerEntryNumberPlaceHolder = "<XXXE#PLCH>";//11 characters
		public const string USFTZControlNumberPlaceHolder = "<CN*PLC>"; //8 characters
		public const string USFTZAdmissionNumberPlaceHolder = "<FTZADMISSION#*PLCH>"; //20 characters

		protected override ZQuery GetExtraResponseMessageFilter()
		{
			if (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoRelease)
			{
				return new ZQuery(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse);
			}
			return base.GetExtraResponseMessageFilter();
		}

		#endregion

		#region IEntryStatusProvider Members

		ZString IEntryStatusProvider.EntryStatus
		{
			get
			{
				ZString result = ZString.Empty;

				if (ApplicationIdentifierCodeList.IsApplicationCodeThatContainEntryStatus(EM_MessageType))
				{
					result = MessageBlock.MessageBlocks.OfType<IEntryStatusProvider>()
														 .Select(block => block.EntryStatus)
														 .FirstOrDefault(status => !status.IsEmpty);
				}

				return result;
			}
		}

		#endregion

		#region IVisaQuery Members

		ZString IVisaQuery.TariffNumber
		{
			get { return VisaQueryBlock == null ? ZString.Empty : VisaQueryBlock.TariffNumber; }
		}

		ZString IVisaQuery.OriginCountry
		{
			get { return VisaQueryBlock == null ? ZString.Empty : VisaQueryBlock.OriginCountry; }
		}

		ZString IVisaQuery.SecondTariffNumber
		{
			get { return VisaQueryBlock == null ? ZString.Empty : VisaQueryBlock.SecondTariffNumber; }
		}

		IVisaQuery VisaQueryBlock
		{
			get
			{
				if (fVisaQueryBlock == null && EM_MessageType == ApplicationIdentifierCodeList.Codes.QueryQuota)
				{
					fVisaQueryBlock = MessageBlock.MessageBlocks.OfType<IVisaQuery>().FirstOrDefault();
				}
				return fVisaQueryBlock;
			}
		}
		IVisaQuery fVisaQueryBlock;

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.IssuerAndBillNumber, typeof(IssuerAndBillNumber));
			return result;
		}

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				ControllerID result = ControllerIDs.Messaging.EDIMessage;
				switch (EM_MessageType)
				{
					case ApplicationIdentifierCodeList.Codes.QueryQuota:
					case ApplicationIdentifierCodeList.Codes.QueryQuotaResponse:
						result = ControllerIDs.Customs.US.QueryMessages;
						break;
					case ApplicationIdentifierCodeList.Codes.LineRelease:
						result = ControllerIDs.Customs.US.BorderLineReleaseMessage;
						break;
				}
				return result;
			}
		}

		#endregion
	}
}
