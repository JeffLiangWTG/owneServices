using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.BatchProcessor
{
	public class InboundInterchangeProcessor : Messaging.Business.InboundInterchangeProcessor
	{
		public InboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes
		{
			get { return new string[] { EDIMessage.ApplicationCodes.NewZealandCustoms }; }
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ??= new InboundMessageCreator();
		}
		IInboundMessageCreator messageCreator;

		class InboundMessageCreator : IInboundMessageCreator
		{
			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				if (interchange.EI_HeaderText.IsEmpty)
				{
					GenerateTSWMessageFromInterchange(interchange);
				}
				else
				{
					interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived(false);
				}
			}

			#region Generate TSW Message

			void GenerateTSWMessageFromInterchange(EDIInterchange interchange)
			{
				var message = interchange.EI_BodyText;
				var newEDIMessage = interchange.ContainedMessages.AddNew(typeof(TSWMessage));
				newEDIMessage.EM_ApplicationReference = GetApplicationReference(message);
				newEDIMessage.EM_MessageType = GetMessageType(message, newEDIMessage.EM_ApplicationReference, interchange.Factory);
				newEDIMessage.EM_MessageSubType = MessageTypeList.Codes.TWR;
				newEDIMessage.EM_MessageText = TSWMessageFormatter.FormatWithXMLRepresentation(message);
				newEDIMessage.EM_IsTestMessage = interchange.EI_From == EDIInterchange.InterchangePartyIDs.NZCustomsTestMailbox;
				newEDIMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
				newEDIMessage.EM_Status = EDIMessage.Status.Queued;
				newEDIMessage.EM_MessageNum = GetMessageNum(message);
			}

			protected string GetMessageType(string messageText, string applicationReference, BusinessObjectFactory factory)
			{
				var typeElement = "<AgencyAssignedCustomizedDocumentName>RES";
				int typeStart = messageText.IndexOf(typeElement) + typeElement.Length;
				var msgType = messageText.Substring(typeStart, 3);
				if (msgType == "IM1" || msgType == "EX1" || msgType == "UNK")
				{
					msgType = GetSendingMsgTypeMappedToTSWMessageType(applicationReference, factory);
				}

				if (string.IsNullOrEmpty(msgType))
				{
					msgType = MessageTypeList.Codes.TWR;
				}

				return msgType;
			}

			protected string GetMessageNum(string messageText)
			{
				var refElement = "<FunctionalReferenceID>";
				int msgNumStart = messageText.IndexOf(refElement) + refElement.Length;
				int msgNumEnd = messageText.IndexOf("</FunctionalReferenceID>");
				int msgNumLength = msgNumEnd - msgNumStart;
				return messageText.Substring(msgNumStart, msgNumLength);
			}

			protected string GetApplicationReference(string messageText)
			{
				var decElement = "<Declaration>";
				var refElement = "<FunctionalReferenceID>";
				int declarationStart = messageText.IndexOf(decElement);
				int appRefStart = messageText.IndexOf(refElement, declarationStart) + refElement.Length;
				int appRefEnd = messageText.IndexOf(refElement.Replace("<", "</"), declarationStart);
				int appRefLength = appRefEnd - appRefStart;
				return messageText.Substring(appRefStart, appRefLength);
			}

			protected string GetSendingMsgTypeMappedToTSWMessageType(ZString applicationReference, BusinessObjectFactory factory)
			{
				var result = MessageTypeList.Codes.TWR;

				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, JobApplicationCodeList.Codes.TSW);
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());

				var declarationQueryOnReference = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.PK);
				declarationQueryOnReference.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, applicationReference);

				var leadDeclarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.PK);
				var consolidatedDeclarationQueryOnReference = new ZDBOnlySubQuery(typeof(ConsolidatedDeclaration), CusReconDeclarationSchema.CRD_JE_LeadDeclaration);
				consolidatedDeclarationQueryOnReference.AddToFilter(CusReconDeclarationSchema.CRD_JobReferenceNumber, applicationReference);
				leadDeclarationQuery.AddSubQuery(consolidatedDeclarationQueryOnReference, JoinCondition.And);

				declarationQueryOnReference.AddAsUnionQuery(leadDeclarationQuery);
				declarationQuery.AddSubQuery(declarationQueryOnReference, JoinCondition.And);

				var declaration = factory.LoadTop1<JobDeclaration>(declarationQuery);
				if (declaration != null)
				{
					result = declaration.MappedTSWMessageSubType;
				}
				else
				{
					var decSubQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
					decSubQuery.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, JobApplicationCodeList.Codes.TSW);
					decSubQuery.AddToFilter(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());

					var entryQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
					entryQuery.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, applicationReference);
					entryQuery.AddSubQuery(decSubQuery, JoinCondition.And);
					var entryHeader = factory.LoadTop1<CusEntryHeader>(entryQuery);
					if (entryHeader != null)
					{
						declaration = entryHeader.Declaration;
						if (declaration != null)
						{
							result = declaration.MappedTSWMessageSubType;
						}
					}
				}

				if (declaration == null)
				{
					var mawbQuery = new ZQuery(CusMAWBSchema.CM_MessageReference, applicationReference);
					mawbQuery.AddToFilter(CusMAWBSchema.CM_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
					var mawb = factory.LoadTop1<CusMAWB>(mawbQuery);
					if (mawb != null)
					{
						result = MessageTypeList.Codes.CRE;
					}
				}

				return result;
			}

			#endregion
		}
	}
}
