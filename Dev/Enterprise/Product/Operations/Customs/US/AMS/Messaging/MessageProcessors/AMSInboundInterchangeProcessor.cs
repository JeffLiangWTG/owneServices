using System;
using System.IO;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using BaseEDIInterchange = Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class AMSInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public AMSInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool IsNoBranchFilter => true;
		protected override bool SupportEnvironmentSwitch => true;

		protected override string[] ApplicationCodes
		{
			get { return new string[] { CBPEDIInterchange.ApplicationCodes.AMS }; }
		}

		protected override Enterprise.Messaging.Business.IInboundMessageCreator GetMessageCreator(BaseEDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new AMSInboundMessageCreator());
		}
		Enterprise.Messaging.Business.IInboundMessageCreator messageCreator;

		#region AMSInboundMessageCreator Class

		class AMSInboundMessageCreator : InboundMessageCreator<APLACR, APLACR, APLZCR, APLZCR, AMSEDIMessage>
		{
			protected override ZString GetMessageNum(APLACR msgBlockA, APLACR msgBlockB, Stream messageTextStream, APLZCR msgBlockY, APLZCR msgBlockZ)
			{
				var applicationIdentifier = msgBlockB.ApplicationIdentifier;
				ZString? messageNum = null;
				var blockDeserialiser = new OutputMessageBlockDeserialiser();
				messageTextStream.Position = 0;
				var reader = new StreamReader(messageTextStream);

				while (true)
				{
					var buffer = new char[80];
					var readCount = reader.Read(buffer, 0, 80);
					if (readCount == 0)
					{
						break;
					}

					Array.Resize(ref buffer, readCount);
					var data = new string(buffer).PadRight(80);
					if (data.StartsWith("M02"))
					{
						var messageBlock = blockDeserialiser.GetDeserialisedBlock(new string[] { Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.AMS }, applicationIdentifier, data, false);
						if (messageBlock is US.Messaging.Business.MessageBuildingBlocks.UnknownMessageBlock)
						{
							messageBlock = blockDeserialiser.GetDeserialisedBlock(new string[] { Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Constants.ACE }, applicationIdentifier, data);
						}

						if (messageBlock is US.Messaging.Business.MessageBuildingBlocks.IINPM02)
						{
							var inpm02 = (US.Messaging.Business.MessageBuildingBlocks.IINPM02)messageBlock;
							messageNum = CarrierAssignedBatchNumberCreator.GetMessageNumber(inpm02);
							break;
						}
					}
				}

				if (!messageNum.HasValue)
				{
					messageNum =
						msgBlockA.AMSUserCode.PadRight(4, '0') +
						msgBlockA.Date.ToString("MMdd") +
						msgBlockA.Time.PadRight(6, '0') +
						msgBlockA.BatchNumber.ToString();
				}

				return messageNum.Value;
			}

			protected override ZString GetApplicationIdentifier(APLACR msgBlockA, APLACR msgBlockB)
			{
				return msgBlockA.ApplicationIdentifier;
			}

			protected override void CreateMessages(BaseEDIInterchange interchange, APLACR msgBlockA, VirtualMemoryStream bodyTextStream, APLZCR msgBlockZ)
			{
				CreateMessage(interchange, CreateMessageTextStream(bodyTextStream, 0, (int)bodyTextStream.Length), msgBlockA, GetBBlock(msgBlockA), GetYBlock(msgBlockZ), msgBlockZ);
			}

			APLZCR GetYBlock(APLZCR msgBlockZ)
			{
				var result = new APLZCR();
				result.Deserialise(msgBlockZ.Serialise());
				return result;
			}

			APLACR GetBBlock(APLACR msgBlockA)
			{
				var result = new APLACR();
				result.Deserialise(msgBlockA.Serialise());
				result.Password = ZString.Empty;
				return result;
			}
		}
		#endregion
	}
}
