using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.CUSRES;
using Enterprise.Edifact.D96B.Segments;
using Enterprise.Edifact.Generic;
using Enterprise.ZArchitecture.Business;
using D96BMessageFactory = Enterprise.Edifact.D96B.EdifactD96BMessageFactory;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class CUSRESMessageHelper : MessageHelper
	{
		public CUSRESMessageHelper(ZAMessage ediMessage, CUSRESMessage message)
			: base(ediMessage)
		{
			cusresMessage = Argument.NotNull(message, "message");
			interchangeTime = ZDateTime.Empty;
		}

		readonly CUSRESMessage cusresMessage;

		public static CUSRESMessageHelper New(ZAMessage message)
		{
			CUSRESMessageHelper result = null;
			if (message != null)
			{
				var d96bMessageFactory = new D96BMessageFactory();
				var zaCharSet = new ZACharacterSet();
				if (message.GetAutoEdifactMessageUsingNamedFactory(d96bMessageFactory, zaCharSet) is CUSRESMessage cusresMessage)
				{
					result = new CUSRESMessageHelper(message, cusresMessage);
					result.interchangeTime = message.EM_DateTimeInterchangeSent;
				}
			}
			return result;
		}

		const string RejectionOfFrequentSubmission = "Message ignored as a duplicate submission within allocated timeframe";

		#region Implementation

		public bool DoesEntryStatusNeedsToBeUpdated(CusEntryHeader entry)
		{
			var result = false;

			if (entry != null && CustomsStatusAttributeHelper.ShouldUpdateCustomsStatus(entry.Factory, EntryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
			{
				result = entry?.CH_EntryStatus.IsEmpty ?? false;
				if (!result)
				{
					var currentLatestResponseTime = entry.Logs?.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_EventTime ?? ZDateTime.Empty;
					result = !currentLatestResponseTime.IsValid || MessageDate >= currentLatestResponseTime;
					result &= (entry.LastSentCUSDECMessage?.EM_MessageNum ?? ZString.Empty) == OutgoingMessageNumber;
					result &= !IsRejectionOfSubmissionWithinAllocatedTimeframe;
				}
			}
			return result;
		}

		public ZString CommonAccessReference
		{
			get { return cusresMessage.BGM[0].DocumentMessageIdentification.DocumentMessageNumber; }
		}

		public ZString OutgoingMessageNumber
		{
			get
			{
				var outGoingMessageNum = ZString.Empty;
				foreach (SegmentGroup3 sg3 in cusresMessage.Group3)
				{
					foreach (RFFSegment rff in sg3.RFF)
					{
						if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.AdditionalReferenceNumber)
						{
							outGoingMessageNum = rff.Reference.ReferenceNumber;
							break;
						}
					}
					if (!outGoingMessageNum.IsEmpty)
					{
						break;
					}
				}
				return outGoingMessageNum;
			}
		}

		public ZString EntryStatus
		{
			get
			{
				if (entryStatus == null)
				{
					if (cusresMessage.GIS != null)
					{
						var result = ZString.Empty;
						foreach (GISSegment gis in cusresMessage.GIS)
						{
							if (gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsStatusOfGoods)
							{
								result = gis.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
								break;
							}
						}
						entryStatus = result;
					}
				}
				return entryStatus;
			}
		}
		string entryStatus;

		public ZDateTime MessageDate
		{
			get
			{
				if (messageDate == null)
				{
					var interchange = Interchange;
					var dateTimePrep = interchange?.UNB?.DateTimeOfPreparation ?? new DateTimeOfPreparationElements();
					ZDateTime date;
					messageDate = ZDateTime.TryParseExact(dateTimePrep.Date + dateTimePrep.Time, out date, "yyyyMMddHHmm") ? date : ZDateTime.Invalid;
				}
				return messageDate.Value;
			}
		}
		ZDateTime? messageDate;

		public ZString EntryStatusDescription => ZARefCusCodeListTypes.GetCustomsStatusList(Factory).GetDescriptionFromCode(EntryStatus);

		public ZString MRNNumber
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup3 sg3 in cusresMessage.Group3)
				{
					var rff = sg3.RFF[0];
					if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.CustomsDeclarationNumber)
					{
						result = rff.Reference.ReferenceNumber;
						break;
					}
				}
				return result;
			}
		}

		public ZDateTime AssessmentDate
		{
			get
			{
				if (!assessmentDate.HasValue)
				{
					var date = ZDateTime.Empty;
					foreach (SegmentGroup3 group3 in cusresMessage.Group3)
					{
						if (group3.RFF[0].Reference.ReferenceQualifier == ReferenceQualifierList.CustomsDeclarationNumber)
						{
							ZDateTime.TryParseExact(group3.DTM[0].DateTimePeriod.DateTimePeriod, out date, "yyyyMMdd");
							break;
						}
					}
					assessmentDate = date;
				}
				return assessmentDate.Value;
			}
		}
		ZDateTime? assessmentDate;

		public ZDateTime PostingDate => LoadDateTimeFromDTM(DateTimePeriodQualifierList.PostingDate);

		public ZString CaseNumber
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup3 sg3 in cusresMessage.Group3)
				{
					foreach (RFFSegment rff in sg3.RFF)
					{
						if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.EnquiryNumber)
						{
							result = rff.Reference.ReferenceNumber;
							break;
						}
					}
					if (!result.IsEmpty)
					{
						break;
					}
				}
				return result;
			}
		}

		public ZString[] OtherGovernmentAgencies
		{
			get
			{
				var results = new List<ZString>();
				foreach (SegmentGroup4 group4 in cusresMessage.Group4)
				{
					foreach (FTXSegment ftx in group4.FTX)
					{
						if (ftx.TextSubjectQualifier == ReferenceQualifierList.ConsigneesShipmentReferenceNumber)
						{
							var parts = ftx.GetFreeTextMessage().Split("=");
							if (parts.Length == 2 && parts[0].EqualsIgnoringCase("OGA Case"))
							{
								results.Add(parts[1]);
							}
						}
					}
				}
				return results.ToArray();
			}
		}

		public ZString CustomsPrintIndicator
		{
			get
			{
				if (customsPrintIndicator == null)
				{
					if (cusresMessage.GIS != null)
					{
						foreach (GISSegment gis in cusresMessage.GIS)
						{
							if (gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsStatusOfGoods)
							{
								customsPrintIndicator = gis.ProcessingIndicator.ProcessTypeIdentification.ToString();
								break;
							}
						}
					}
				}
				return customsPrintIndicator;
			}
		}
		string customsPrintIndicator;

		public ZString CustomsPrintIndicatorDescription => Factory.GetCachedValue<CustomsPrintIndicatorList>().GetDescriptionFromCode(CustomsPrintIndicator);

		public ZString LRNNumber => cusresMessage?.BGM[0].DocumentMessageIdentification.DocumentMessageNumber ?? ZString.Empty;

		public ZString HouseBill
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup3 sg3 in cusresMessage.Group3)
				{
					foreach (RFFSegment rff in sg3.RFF)
					{
						if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.HouseBillOfLadingNumber)
						{
							result = rff.Reference.ReferenceNumber;
							break;
						}
					}
					if (!result.IsEmpty)
					{
						break;
					}
				}
				return result;
			}
		}

		public ZString VoyageFlightNo
		{
			get
			{
				var result = ZString.Empty;
				var transportCode = cusresMessage.TDT.Cast<TDTSegment>().FirstOrDefault(tdt => tdt.TransportStageQualifier == TransportStageQualifierList.MainCarriageTransport)?.ConveyanceReferenceNumber;
				return transportCode;
			}
		}

		public ZString TransportCode
		{
			get
			{
				var result = ZString.Empty;
				var transportCode = cusresMessage.TDT.Cast<TDTSegment>().FirstOrDefault(tdt => tdt.TransportStageQualifier == TransportStageQualifierList.MainCarriageTransport)?.ModeOfTransport.ModeOfTransportCoded;
				return transportCode;
			}
		}

		public ZString CustomsOfficeCode
		{
			get
			{
				if (customsOfficeCode == null)
				{
					foreach (LOCSegment loc in cusresMessage.LOC)
					{
						if (loc.PlaceLocationQualifier == PlaceLocationQualifierList.CustomsOfficeOfClearance)
						{
							customsOfficeCode = loc.LocationIdentification.PlaceLocationIdentification;
							break;
						}
					}
				}
				return customsOfficeCode;
			}
		}
		string customsOfficeCode;

		public ZString CustomsOfficeDescription => ZARefCusCodeListTypes.GetCustomsOfficeList(Factory).GetDescriptionFromCode(CustomsOfficeCode);

		public ZString AgentCode
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup1 sg1 in cusresMessage.Group1)
				{
					foreach (NADSegment nad in sg1.NAD)
					{
						if (nad.PartyQualifier == PartyQualifierList.AgentRepresentative)
						{
							result = nad.PartyIdentificationDetails.PartyIdIdentification;
							break;
						}
					}
					if (!result.IsEmpty)
					{
						break;
					}
				}
				return result;
			}
		}

		public ZString TransportDocumentNumber
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup3 sg3 in cusresMessage.Group3)
				{
					foreach (RFFSegment rff in sg3.RFF)
					{
						if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.TransportDocumentNumber)
						{
							result = rff.Reference.ReferenceNumber;
							break;
						}
					}
					if (!result.IsEmpty)
					{
						break;
					}
				}
				return result;
			}
		}

		public ZDateTime DocumentDate
		{
			get
			{
				if (!documentDate.HasValue)
				{
					ZDateTime result = ZDateTime.Empty;
					foreach (SegmentGroup3 sg3 in cusresMessage.Group3)
					{
						foreach (DTMSegment dtm in sg3.DTM)
						{
							if (dtm.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.DocumentMessageDateTime)
							{
								ZDateTime.TryParseExact(dtm.DateTimePeriod.DateTimePeriod, out result, "yyyyMMdd");
								break;
							}
						}
						documentDate = result;
					}
				}
				return documentDate.Value;
			}
		}
		ZDateTime? documentDate;
		public ZString RegistrationNumber
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup3 sg3 in cusresMessage.Group3)
				{
					foreach (RFFSegment rff in sg3.RFF)
					{
						if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.CargoManifestNumber)
						{
							result = rff.Reference.ReferenceNumber;
							break;
						}
					}
					if (!result.IsEmpty)
					{
						break;
					}
				}
				return result;
			}
		}

		public ZString Containers => ZString.Join(",", cusresMessage.EQD.Cast<EQDSegment>()
			.Where(eqd => eqd.EquipmentQualifier == EquipmentQualifierList.Container)
			.Select(eqd => new ZString(eqd.EquipmentIdentification.EquipmentIdentificationNumber)).ToArray());

		public BusinessObjectCollectionWrapper<CustomsStatusFreeTextWrapper> CustomsStatusFreeTexts
		{
			get { return customsStatusFreeTexts ?? (customsStatusFreeTexts = GetCustomsStatusFreeTexts(null)); }
		}
		BusinessObjectCollectionWrapper<CustomsStatusFreeTextWrapper> customsStatusFreeTexts;

		public int CustomsStatusFreeTextsCount => CustomsStatusFreeTexts?.Count ?? 0;

		public BusinessObjectCollectionWrapper<CustomsStatusFreeTextWrapper> HeaderCustomsStatusFreeTexts
		{
			get
			{
				return headerCustomsStatusFreeTexts ??
					   (headerCustomsStatusFreeTexts = GetCustomsStatusFreeTexts(MessageSectionCodedList.HeadingSection));
			}
		}
		BusinessObjectCollectionWrapper<CustomsStatusFreeTextWrapper> headerCustomsStatusFreeTexts;

		public int HeaderCustomsStatusFreeTextsCount => HeaderCustomsStatusFreeTexts?.Count ?? 0;

		public BusinessObjectCollectionWrapper<CustomsStatusFreeTextWrapper> LineCustomsStatusFreeTexts
		{
			get
			{
				return lineCustomsStatusFreeTexts ??
					   (lineCustomsStatusFreeTexts = GetCustomsStatusFreeTexts(MessageSectionCodedList.DetailSectionOfAMessage));
			}
		}
		BusinessObjectCollectionWrapper<CustomsStatusFreeTextWrapper> lineCustomsStatusFreeTexts;

		public int LineCustomsStatusFreeTextsCount => LineCustomsStatusFreeTexts?.Count ?? 0;

		public BusinessObjectCollectionWrapper<CustomsStatusFreeTextWrapper> SummaryCustomsStatusFreeTexts
		{
			get
			{
				return summaryCustomsStatusFreeTexts ??
					   (summaryCustomsStatusFreeTexts = GetCustomsStatusFreeTexts(MessageSectionCodedList.SummarySection));
			}
		}
		BusinessObjectCollectionWrapper<CustomsStatusFreeTextWrapper> summaryCustomsStatusFreeTexts;

		public int SummaryCustomsStatusFreeTextsCount => SummaryCustomsStatusFreeTexts?.Count ?? 0;

		BusinessObjectCollectionWrapper<CustomsStatusFreeTextWrapper> GetCustomsStatusFreeTexts(MessageSectionCodedList sectionCodeList)
		{
			var customsStatusFreeTextList = new List<CustomsStatusFreeTextWrapper>();
			foreach (SegmentGroup4 group4 in cusresMessage.Group4)
			{
				if (sectionCodeList == null || group4.ERP[0].ErrorPointDetails.MessageSectionCoded == sectionCodeList)
				{
					var line = group4.ERP[0].ErrorPointDetails.MessageItemNumber;
					var stringBuilder = new ZStringBuilder();
					foreach (FTXSegment ftx in group4.FTX)
					{
						stringBuilder.Append(ftx.GetFreeTextMessage());
					}
					var freeText = stringBuilder.ToString();
					customsStatusFreeTextList.AddRange(group4.ERC.Cast<ERCSegment>().Select(erc => new CustomsStatusFreeTextWrapper(line,
					erc.ApplicationErrorDetail.ApplicationErrorIdentification, freeText)));
				}
			}
			return new BusinessObjectCollectionWrapper<CustomsStatusFreeTextWrapper>(customsStatusFreeTextList);
		}

		public List<ResendableResponseInformation> ResendableResponseInformationList(ZString documentIssuer)
		{
			if (ftxHelperlist == null)
			{
				ftxHelperlist = new List<ResendableResponseInformation>();
				foreach (FTXSegment ftxSegment in FreeTextErrors)
				{
					var ftxHelper = new ResendableResponseInformation(ftxSegment, Factory);
					if (ftxHelper.HasValidVersion && ftxHelper.Recipient == documentIssuer)
					{
						ftxHelperlist.Add(ftxHelper);
					}
				}
			}
			return ftxHelperlist;
		}
		List<ResendableResponseInformation> ftxHelperlist;

		List<FTXSegment> FreeTextErrors
		{
			get
			{
				if (ftxSegmentList == null)
				{
					ftxSegmentList = new List<FTXSegment>();
					foreach (SegmentGroup4 group4Item in cusresMessage.Group4)
					{
						ftxSegmentList.AddRange(group4Item.FTX.Cast<FTXSegment>());
					}
				}
				return ftxSegmentList;
			}
		}
		List<FTXSegment> ftxSegmentList;

		internal IEnumerable<ProvisionalPaymentAdditionalInfo> AdditionalProvisionalPaymentInfos
		{
			get
			{
				if (additionalProvisionalPaymentInfos == null)
				{
					var result = new List<ProvisionalPaymentAdditionalInfo>();
					foreach (SegmentGroup4 group4Item in cusresMessage.Group4)
					{
						var lineNumber = group4Item.ERP[0]?.ErrorPointDetails?.MessageItemNumber?.Trim() ?? ZString.Empty;
						foreach (var ftx in group4Item.FTX.Cast<FTXSegment>())
						{
							var provisionalPaymentInfo = new ProvisionalPaymentAdditionalInfo(string.Join(ZString.Empty, ftx.TextLiteral.FreeText1, ftx.TextLiteral.FreeText2, ftx.TextLiteral.FreeText3, ftx.TextLiteral.FreeText4, ftx.TextLiteral.FreeText5), lineNumber);
							if (provisionalPaymentInfo.IsValid)
							{
								result.Add(provisionalPaymentInfo);
							}
						}
					}
					additionalProvisionalPaymentInfos = result;
				}
				return additionalProvisionalPaymentInfos;
			}
		}
		IEnumerable<ProvisionalPaymentAdditionalInfo> additionalProvisionalPaymentInfos;

		internal ZBool IsRejectionOfSubmissionWithinAllocatedTimeframe
		{
			get
			{
				var result = false;
				if (CustomsStatusAttributeHelper.IsStatusRejected(Factory, EntryStatus, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today))
				{
					var firstError = FreeTextErrors.FirstOrDefault()?.GetFreeTextMessage().Trim() ?? ZString.Empty;
					result = firstError.EqualsIgnoringCase(RejectionOfFrequentSubmission);
				}
				return result;
			}
		}

		public ZDateTime DocumentMessageDateTime => LoadDateTimeFromDTM(DateTimePeriodQualifierList.DocumentMessageDateTime);

		public ZDateTime ETA => LoadDateTimeFromDTM(DateTimePeriodQualifierList.ArrivalDateTimeEstimated);

		public ZDateTime ATA => LoadDateTimeFromDTM(DateTimePeriodQualifierList.ArrivalDateTimeActual);

		#region DTM - DateTime Info

		ZDateTime LoadDateTimeFromDTM(DateTimePeriodQualifierList dateTimeType)
		{
			var result = ZDateTime.Invalid;
			var dateString = cusresMessage.DTM.Cast<DTMSegment>().FirstOrDefault(dtm => dtm.DateTimePeriod.DateTimePeriodQualifier == dateTimeType)?.DateTimePeriod?.DateTimePeriod;
			ZDateTime.TryParseExact(dateString, out result, ZA.Business.MessageBuilders.Constants.DateFormatCCYYMMDD);
			return result;
		}

		#endregion

		public ZBool IsGateInOutCUSRESMessage
		{
			get
			{
				var applicationReference = Interchange?.UNB?.ApplicationReference;
				return applicationReference == SARSEDIMessage.MessageTypeNames.CUSRES_GIO || applicationReference == SARSEDIMessage.MessageTypeNames.CUSRES_GOVGIO;
			}
		}

		const string eightZeroesAgentCode = "00000000";

		public bool ShouldDiscardMessage
		{
			get
			{
				var agentCode = AgentCode;
				if (agentCode != eightZeroesAgentCode && !agentCode.IsEmpty)
				{
					ZString recipientIdentificationValue = Interchange?.UNB?.InterchangeRecipient?.RecipientIdentification;
					if (!recipientIdentificationValue.IsEmpty)
					{
						return !(recipientIdentificationValue.SubstringSafe(0, 8) == AgentCode);
					}
				}
				return false;
			}
		}

		#endregion
	}

	internal static class EDIMessageExtension
	{
		internal static ZString GetFreeTextMessage(this FTXSegment ftx)
		{
			return ZString.Format("{0}{1}{2}{3}{4}", ftx.TextLiteral.FreeText1, ftx.TextLiteral.FreeText2, ftx.TextLiteral.FreeText3, ftx.TextLiteral.FreeText4, ftx.TextLiteral.FreeText5);
		}
	}
}
