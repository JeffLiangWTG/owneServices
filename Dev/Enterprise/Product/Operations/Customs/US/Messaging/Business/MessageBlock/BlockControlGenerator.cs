using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class BlockControlGenerator<ControlMessageBlockB, ControlMessageBlockY> : BlockControlGenerator
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected BlockControlGenerator(ZString applicationCode)
			: base(new ControlMessageBlockB(), new ControlMessageBlockY(), applicationCode)
		{
		}

		protected BlockControlGenerator(ZString applicationCode, string[] messageBlockApplicationCodes)
			: base(new ControlMessageBlockB(), new ControlMessageBlockY(), applicationCode, messageBlockApplicationCodes)
		{
		}

		public new ControlMessageBlockB B
		{
			get { return (ControlMessageBlockB)base.B; }
		}

		public new ControlMessageBlockY Y
		{
			get { return (ControlMessageBlockY)base.Y; }
		}
	}

	public abstract class BlockControlGenerator
	{
		protected BlockControlGenerator(IControlMessageBlockB b, IControlMessageBlockY y, ZString applicationCode)
			: this(b, y, applicationCode, new string[] { applicationCode })
		{
		}

		protected BlockControlGenerator(IControlMessageBlockB b, IControlMessageBlockY y, ZString applicationCode, string[] messageBlockApplicationCodes)
		{
			if (b == null)
			{
				throw new ArgumentNullException(nameof(b));
			}
			if (y == null)
			{
				throw new ArgumentNullException(nameof(y));
			}
			if (applicationCode.IsEmpty)
			{
				throw new ArgumentNullException(nameof(applicationCode));
			}
			messageBlocks = new List<MessageBlock>();
			B = b;
			Y = y;
			this.ApplicationCode = applicationCode;
			this.messageBlockApplicationCodes = messageBlockApplicationCodes;
		}

		public readonly IControlMessageBlockB B;
		public readonly IControlMessageBlockY Y;
		public readonly ZString ApplicationCode;
		protected readonly string[] messageBlockApplicationCodes;
		protected readonly List<MessageBlock> messageBlocks;

		public List<MessageBlock> MessageBlocks
		{
			get
			{
				hasReturnedMessageBlocks = true;
				return messageBlocks;
			}
		}
		bool hasReturnedMessageBlocks;

		public void AddMessageBlocks(IEnumerable<MessageBlock> passed)
		{
			if (hasReturnedMessageBlocks)
			{
				throw new InvalidOperationException("Cannot add more blocks - have already read them");
			}
			messageBlocks.AddRange(passed);
		}

		public void AddMessageBlock(MessageBlock messageBlock)
		{
			if (hasReturnedMessageBlocks)
			{
				throw new InvalidOperationException("Cannot add more blocks - have already read them");
			}
			messageBlocks.Add(messageBlock);
		}

		/// <summary>
		/// </summary>
		/// <param name="attributeType">Pass either InputBlockAttribute or OutputBlockAttribute (Depending on the type of message you are parsing</param>
		/// <param name="message"></param>
		/// <param name="preferredMessageBlockApplicationCode"></param>
		public void Deserialise(string message, string preferredMessageBlockApplicationCode = "")
		{
			Deserialise(new StringReader(message), preferredMessageBlockApplicationCode);
		}

		public void Deserialise(TextReader reader, string preferredMessageBlockApplicationCode)
		{
			bool isBBlockSet = false;
			while (true)
			{
				var buffer = new char[80];
				int readCount = reader.Read(buffer, 0, 80);
				if (readCount == 0)
				{
					break;
				}

				Array.Resize(ref buffer, readCount);
				var data = new string(buffer);
				if (!isBBlockSet)
				{
					EnsureBBlockCanBeDeserialise(data);
					B.Deserialise(data);
					isBBlockSet = true;
				}
				else if (reader.Peek() == -1)
				{
					EnsureYBlockCanBeDeserialise(data);
					Y.Deserialise(data);
					break;
				}
				else
				{
					MessageBlock messageBlock = GetMessageBlock(string.IsNullOrEmpty(B.ApplicationIdentifier) ? ApplicationIdentifier : B.ApplicationIdentifier, data, preferredMessageBlockApplicationCode);
					AddMessageBlock(messageBlock);
				}
			}
		}

		public const int MessageBlockLengthInChars = 80;

		public T CreateMessage<T>(BusinessObjectFactory factory)
			where T : CBPEDIMessage
		{
			T message = factory.New<T>();
			message.EM_ApplicationCode = ApplicationCode;
			message.EM_MessageType = ApplicationIdentifier;
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			message.EM_Status = CBPEDIMessage.Status.Queued;
			message.EM_MessageText = Serialise();
			return message;
		}

		public string Serialise()
		{
			return Serialise(false);
		}

		public string SerialiseTo80ByteBlocks()
		{
			ZString messageToFormat = Serialise();

			ZStringBuilder blocks = new ZStringBuilder();
			while (messageToFormat.Length > 0)
			{
				blocks.Append(messageToFormat.Left(80));
				messageToFormat = messageToFormat.SubstringSafe(80);
			}

			return blocks.ToStringWithNewLineBetweenAppends();
		}

		public string Serialise(bool humanFriendly)
		{
			return Serialise(humanFriendly, "");
		}

		public string Serialise(bool humanFriendly, ZString outgoingApplicationIdentifier)
		{
			SetupBlockYDetails();

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(B.Serialise(humanFriendly));
			ReadOnlyCollection<MessageBlock> readOnlyList = null;
			foreach (MessageBlock messageBlockBase in messageBlocks.ToArray())
			{
				string message = "";
				var ads = messageBlockBase as ISerialiserSupporter;

				if (ads != null)
				{
					readOnlyList = readOnlyList ?? (readOnlyList = messageBlocks.AsReadOnly());
					message = ads.Serialise(humanFriendly, outgoingApplicationIdentifier, readOnlyList);
				}
				else
				{
					message = messageBlockBase.Serialise(humanFriendly);
				}
				stringBuilder.Append(message);
			}
			stringBuilder.Append(Y.Serialise(humanFriendly));
			return stringBuilder.ToString();
		}

		protected virtual void SetupBlockYDetails()
		{
		}

		protected abstract void EnsureYBlockCanBeDeserialise(string yBlock);
		protected abstract void EnsureBBlockCanBeDeserialise(string message);
		protected abstract MessageBlockDeserialiser MessageBlockDeserialiser { get; }
		protected abstract ZString ApplicationIdentifier { get; }

		protected virtual MessageBlock GetMessageBlock(string applicationIdentifier, string eightyCharacterBlock, string preferredMessageBlockApplicationCode)
		{
			var messageBlockApplicationCodeList = new List<string>();
			if (!string.IsNullOrEmpty(preferredMessageBlockApplicationCode) && !messageBlockApplicationCodes.Contains(preferredMessageBlockApplicationCode))
			{
				messageBlockApplicationCodeList.Add(preferredMessageBlockApplicationCode);
			}

			messageBlockApplicationCodeList.AddRange(messageBlockApplicationCodes);
			return MessageBlockDeserialiser.GetDeserialisedBlock(messageBlockApplicationCodeList.ToArray(), applicationIdentifier, eightyCharacterBlock);
		}
	}
}
