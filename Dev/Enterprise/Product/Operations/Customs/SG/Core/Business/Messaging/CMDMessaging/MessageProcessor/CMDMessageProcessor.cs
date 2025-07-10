using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class BatchCMDMessageProcessor : BaseMessageProcessor
	{
		public BatchCMDMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			return new List<ApplicationTypeMessageProcessor>() { new CMDMessageProcessor(Logger) };
		}
	}

	public class CMDMessageProcessor : ApplicationTypeMessageProcessor
	{
		public CMDMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "Singapore CMD";

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.SingaporeCMD;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { EDIMessageTypeList.Codes.XDC };

		protected override void ProcessMessageCore(EDIMessage xdcMessage)
		{
			var helper = new CMDInboundMessageProcesserHelper(Logger);
			var cmdInbound = helper.CreateInboundFromXDCMessage(xdcMessage);
			var isValid = cmdInbound.IsValid;
			xdcMessage.EM_Status = isValid ? EDIMessage.Status.Received : EDIMessage.Status.Failed;

			var factory = xdcMessage.Factory;
			var message = factory.New<CMDEDIMessage>();
			message.EM_EI = xdcMessage.Interchange.PK;
			message.EM_MessageNum = xdcMessage.EM_MessageNum;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = isValid ? cmdInbound.StandardMessageIdentifier : (ZString)CMDInbound.Constants.UNK;
			message.EM_MessageText = cmdInbound.MessageText;

			var originatingMessage = helper.GetOriginatingMessage(factory, cmdInbound);
			if (originatingMessage == null)
			{
				message.EM_Status = EDIMessage.Status.Failed;
				message.EM_LinkUniqueID = ZGuid.Empty;
				message.EM_LinkTable = ZString.Empty;
				Logger.LogWarning(ZString.Format("Could not find originating message for inbound '{0}' message for MAWB '{1}' and HAWB '{2}'. Ignoring.", cmdInbound.StandardMessageIdentifier, cmdInbound.MasterBillNumber, cmdInbound.HouseBillNumber));
			}
			else
			{
				var branchPk = originatingMessage.EM_GB;
				using (branchPk.IsValid && branchPk != GlbBranch.CurrentBranch.PK ? DisposableEnvironment.ForBranch(branchPk.ToGuid()) : null)
				{
					message.EM_Status = EDIMessage.Status.Received;
					message.EM_LinkUniqueID = originatingMessage.PK;
					message.EM_LinkTable = EDIMessageSchema.Constants.TableName;
					message.EM_GB = branchPk;

					helper.ConstructAndSendInboundMessageEmailNotification(originatingMessage, message, cmdInbound, cmdInbound.IsErrorMessage);

					if (!cmdInbound.IsErrorMessage && !IsPreviouslyAccepted(originatingMessage, message))
					{
						var licenceLogger = ObjectFactory.Get<ILicenceConsumptionLogCreator>();
						licenceLogger.CreateLog(Env.Licence.CMDReporting, true);
					}

					Logger.Log(ZString.Format("Successfully processed inbound {0} message for MAWB '{1}' and HAWB '{2}'", cmdInbound.StandardMessageIdentifier, cmdInbound.MasterBillNumber, cmdInbound.HouseBillNumber));
				}
			}
		}

		bool IsPreviouslyAccepted(EDIMessage originatingMessage, EDIMessage message)
		{
			var findPreviousAcceptedMessagesQuery = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, originatingMessage.PK);
			findPreviousAcceptedMessagesQuery.AddToFilter(EDIMessageSchema.EM_MessageType, CMDInbound.Constants.CMA);
			findPreviousAcceptedMessagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			findPreviousAcceptedMessagesQuery.AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, message.PK);

			return message.Factory.Load<EDIMessage>(findPreviousAcceptedMessagesQuery).Any();
		}
	}
}
