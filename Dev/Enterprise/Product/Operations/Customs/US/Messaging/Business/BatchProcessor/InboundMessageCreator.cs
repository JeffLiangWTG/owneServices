using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class InboundMessageCreator<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY, ControlMessageBlockZ, EdiMessageType> : IInboundMessageCreator
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
		where ControlMessageBlockZ : MessageBlock, IControlMessageBlockZ, new()
		where EdiMessageType : CBPEDIMessage
	{
		const string BlockControlHeader = "B";
		const string BlockControlTrailer = "Y";
		const int BlockLength = 80;

		protected abstract ZString GetMessageNum(ControlMessageBlockA msgBlockA, ControlMessageBlockB msgBlockB, Stream messageTextStream, ControlMessageBlockY msgBlockY, ControlMessageBlockZ msgBlockZ);
		protected abstract ZString GetApplicationIdentifier(ControlMessageBlockA msgBlockA, ControlMessageBlockB msgBlockB);

		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var msgBlockA = new ControlMessageBlockA();
			msgBlockA.Deserialise(BlockPadder.Pad(interchange.EI_HeaderText));

			var msgBlockZ = new ControlMessageBlockZ();
			if (!interchange.EI_FooterText.IsEmpty)
			{
				msgBlockZ.Deserialise(BlockPadder.Pad(interchange.EI_FooterText));
			}

			using (var bodyTextStream = interchange.GetEI_BodyTextReader().GetPaddedMemoryStream())
			{
				CreateMessages(interchange, msgBlockA, bodyTextStream, msgBlockZ);
			}
		}

		#region Message Creation

		protected virtual void CreateMessages(EDIInterchange interchange, ControlMessageBlockA msgBlockA, VirtualMemoryStream bodyTextStream, ControlMessageBlockZ msgBlockZ)
		{
			var msgBlockB = new ControlMessageBlockB();
			var msgBlockY = new ControlMessageBlockY();

			long length = bodyTextStream.Length;
			long startingPosition = 0;

			while (startingPosition < length)
			{
				bodyTextStream.Position = startingPosition;
				ZString data = new StreamReader(bodyTextStream, ASCIIEncoding.ASCII).GetString(BlockLength);

				if (data.Left(1) != BlockControlHeader)
				{
					throw new InvalidMessageFormatException("Invalid data detected - no header block found");
				}

				msgBlockB = new ControlMessageBlockB();
				msgBlockB.Deserialise(data);
				startingPosition += ASCIIEncoding.ASCII.GetByteCount(data);

				bool hasFoundBlockControlTrailer = false;
				long offset = 0;
				long yBlockOffset = 0;
				int messageLength = 0;
				bodyTextStream.Position = startingPosition;
				var reader = new StreamReader(bodyTextStream, ASCIIEncoding.ASCII);

				while (!hasFoundBlockControlTrailer && startingPosition + offset < length)
				{
					data = reader.GetString(BlockLength);
					hasFoundBlockControlTrailer = (data.Left(1) == BlockControlTrailer);

					if (!hasFoundBlockControlTrailer)
					{
						messageLength += data.Length;
					}

					yBlockOffset = offset;
					offset += ASCIIEncoding.ASCII.GetByteCount(data);
				}

				if (hasFoundBlockControlTrailer)
				{
					if (startingPosition + offset > length)
					{
						offset = length - startingPosition;
					}

					msgBlockY = new ControlMessageBlockY();
					bodyTextStream.Position = startingPosition + yBlockOffset;
					msgBlockY.Deserialise(new StreamReader(bodyTextStream, ASCIIEncoding.ASCII).GetString(BlockLength).PadRight(BlockLength));
				}
				CreateMessagesWhereMultipleMessagesWithinBYBlock(interchange, msgBlockA, msgBlockB, msgBlockY, msgBlockZ, CreateMessageTextStream(bodyTextStream, startingPosition, messageLength));

				startingPosition += offset;
			}
		}

		protected VirtualMemoryStream CreateMessageTextStream(VirtualMemoryStream bodyTextStream, long startingPosition, int length)
		{
			var result = new VirtualMemoryStream();
			var writer = new StreamWriter(result, ASCIIEncoding.ASCII);
			int totalRead = 0;
			var reader = new StreamReader(bodyTextStream, ASCIIEncoding.ASCII);
			bodyTextStream.Position = startingPosition;

			while (true)
			{
				var data = reader.GetString(BlockLength);

				if (data.Length + totalRead >= length)
				{
					writer.Write(data.Left(length - totalRead));
					break;
				}
				else
				{
					writer.Write(data);
					totalRead += data.Length;
					if (data.Length != BlockLength)
					{
						break;
					}
				}
			}

			writer.Flush();
			return result;
		}

		void CreateMessagesWhereMultipleMessagesWithinBYBlock(EDIInterchange interchange, ControlMessageBlockA msgBlockA, ControlMessageBlockB msgBlockB, ControlMessageBlockY msgBlockY, ControlMessageBlockZ msgBlockZ, VirtualMemoryStream messageTextStream)
		{
			long currentPos = 0;
			int totalByteThreshold = 0;

			if (MultiMessageInBY.ContainsKey(msgBlockB.ApplicationIdentifier))
			{
				var messageTexPartialStream = new VirtualMemoryStream();
				var messageTexPartialWriter = new StreamWriter(messageTexPartialStream, ASCIIEncoding.ASCII);
				long length = messageTextStream.Length;
				messageTextStream.Position = 0;
				var reader = new StreamReader(messageTextStream, ASCIIEncoding.ASCII);

				while (currentPos < length)
				{
					var data = reader.GetString(BlockLength);
					if (string.IsNullOrEmpty(data))
					{
						break;
					}

					string identifier = data.Left(4);

					MultiMessageBreakInfo breakInfo;
					if (MultiMessageInBY.TryGetValue(msgBlockB.ApplicationIdentifier, out breakInfo))
					{
						foreach (string messageBlockIdentifier in breakInfo.MessageBlocks)
						{
							if (identifier.StartsWith(messageBlockIdentifier) && (breakInfo.ByteLimit == MultiMessageBreakInfo.NoByteLimit || totalByteThreshold > breakInfo.ByteLimit))
							{
								messageTexPartialWriter.Flush();
								totalByteThreshold = 0;
								if (messageTexPartialStream.Length > 0)
								{
									CreateMessage(interchange, messageTexPartialStream, msgBlockA, msgBlockB, msgBlockY, msgBlockZ);
									messageTexPartialStream = new VirtualMemoryStream();
									messageTexPartialWriter = new StreamWriter(messageTexPartialStream, ASCIIEncoding.ASCII);
								}
							}
						}
					}

					messageTexPartialWriter.Write(data);
					currentPos += ASCIIEncoding.ASCII.GetByteCount(data);
					totalByteThreshold += ASCIIEncoding.ASCII.GetByteCount(data);
				}

				messageTexPartialWriter.Flush();

				if (messageTexPartialStream.Length > 0)
				{
					CreateMessage(interchange, messageTexPartialStream, msgBlockA, msgBlockB, msgBlockY, msgBlockZ);
				}
			}
			else
			{
				CreateMessage(interchange, messageTextStream, msgBlockA, msgBlockB, msgBlockY, msgBlockZ);
			}
		}

		/// <summary>
		/// make sure that message processor does not rely on message number to link the message back to the entry - because multiple of the same message
		/// numbers will be created
		/// </summary>
		Dictionary<string, MultiMessageBreakInfo> MultiMessageInBY
		{
			get
			{
				if (multiMessageInBY == null)
				{
					multiMessageInBY = GetMultiMessageInBY();
				}

				return multiMessageInBY;
			}
		}
		Dictionary<string, MultiMessageBreakInfo> multiMessageInBY;

		protected virtual Dictionary<string, MultiMessageBreakInfo> GetMultiMessageInBY()
		{
			return new Dictionary<string, MultiMessageBreakInfo>();
		}

		protected struct MultiMessageBreakInfo
		{
			public MultiMessageBreakInfo(string[] messageBlocks, int byteLimit)
			{
				this.MessageBlocks = messageBlocks;
				this.ByteLimit = byteLimit;
			}

			public readonly string[] MessageBlocks;
			public readonly int ByteLimit;

			public const int NoByteLimit = -1;
		}

		protected void CreateMessage(EDIInterchange interchange, Stream messageTextStream, ControlMessageBlockA msgBlockA, ControlMessageBlockB msgBlockB, ControlMessageBlockY msgBlockY, ControlMessageBlockZ msgBlockZ)
		{
			var applicationIdentifier = GetApplicationIdentifier(msgBlockA, msgBlockB);
			interchange.EI_InterchangeType = applicationIdentifier;

			var ediMessage = (EdiMessageType)interchange.ContainedMessages.AddNew(typeof(EdiMessageType));
			ediMessage.EM_ApplicationCode = interchange.EI_ApplicationCode;
			ediMessage.EM_MessageType = applicationIdentifier;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_GB = interchange.EI_GB;
			ediMessage.EM_MessageNum = GetMessageNum(msgBlockA, msgBlockB, messageTextStream, msgBlockY, msgBlockZ);

			var stream = messageTextStream.AddHeader(msgBlockB != null ? msgBlockB.Serialise() : "");

			if (msgBlockY != null)
			{
				stream.AddFooter(msgBlockY.Serialise());
			}

			ediMessage.SetEM_MessageTextSource(new TextReaderSource(stream));
		}

		#endregion
	}
}
