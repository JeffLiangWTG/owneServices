using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business
{
	public partial class BlockControlReader
	{
		public abstract class MessageBlockEnumerator
		{
			public MessageBlockEnumerator(BlockControlReader reader)
			{
				this.reader = reader;
			}
			readonly BlockControlReader reader;
			ZString currentBlock;

			public MessageBlock Current
			{
				get
				{
					string applicationIdentifier = reader.ApplicationIdentifier;
					MessageBlockDeserialiser deserialiser = GetNewDeserialiser();
					return deserialiser.GetDeserialisedBlock(reader.ApplicationCodes, applicationIdentifier, currentBlock);
				}
			}

			protected abstract MessageBlockDeserialiser GetNewDeserialiser();

			public bool MoveNext()
			{
				var buffer = new char[80];
				int bytesRead = reader.messageTextReader.Read(buffer, 0, 80);
				bool result = bytesRead > 0;
				if (result)
				{
					currentBlock = new string(buffer);
					if (bytesRead != 80)
					{
						currentBlock = currentBlock.Left(bytesRead).PadRight(80);
					}
					if (reader.IsYBlockData(buffer))
					{
						reader.Y.Deserialise(currentBlock);
						result = false;
					}
				}
				else
				{
					currentBlock = "";
				}
				return result;
			}
		}

		public class OutputMessageBlockEnumerator : MessageBlockEnumerator
		{
			public OutputMessageBlockEnumerator(BlockControlReader reader)
				: base(reader)
			{
			}

			protected override MessageBlockDeserialiser GetNewDeserialiser()
			{
				return new OutputMessageBlockDeserialiser();
			}
		}

		public class InputMessageBlockEnumerator : MessageBlockEnumerator
		{
			public InputMessageBlockEnumerator(BlockControlReader reader)
				: base(reader)
			{
			}

			protected override MessageBlockDeserialiser GetNewDeserialiser()
			{
				return new InputMessageBlockDeserialiser();
			}
		}
	}
}
