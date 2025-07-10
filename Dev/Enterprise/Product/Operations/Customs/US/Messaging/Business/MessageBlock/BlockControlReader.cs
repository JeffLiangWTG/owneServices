using System.Collections;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Instantiated through reflection")]
	public interface IBlockControlReaderHelper
	{
		void SetBAndYBlocks(ZString first80, ZString applicationIdentifier, out IControlMessageBlockB b, out IControlMessageBlockY y);
		bool IsYBlockData(char[] buffer);
	}

	public partial class BlockControlReader
	{
		public BlockControlReader(string message, string[] applicationCodes, ZString messageType)
			: this(new StringReader(message), applicationCodes, messageType)
		{
		}

		public BlockControlReader(TextReader messageReader, string[] applicationCodes, ZString messageType)
		{
			this.messageTextReader = messageReader;
			Argument.NotNull(applicationCodes, "applicationCodes");
			this.ApplicationCodes = applicationCodes;
			this.messageType = messageType;
			readerHelper = GetReaderHelper();

			if (readerHelper != null)
			{
				DeserialiseBBlock();
			}
		}

		public readonly string[] ApplicationCodes;

		public IControlMessageBlockB B
		{
			get { return b; }
		}
		IControlMessageBlockB b;

		public IControlMessageBlockY Y
		{
			get { return y; }
		}
		IControlMessageBlockY y;

		public ZString ApplicationIdentifier
		{
			get { return B == null || B.ApplicationIdentifier.IsEmpty ? messageType : B.ApplicationIdentifier; }
		}

		public bool IsYBlockData(char[] buffer)
		{
			return readerHelper != null && readerHelper.IsYBlockData(buffer);
		}

		public OutputMessageBlockEnumerator GetEnumerator()
		{
			return GetOutputMessageEnumerator();
		}

		public OutputMessageBlockEnumerator GetOutputMessageEnumerator()
		{
			return new OutputMessageBlockEnumerator(this);
		}

		public InputMessageBlockEnumerator GetInputMessageEnumerator()
		{
			return new InputMessageBlockEnumerator(this);
		}

		IBlockControlReaderHelper GetReaderHelper()
		{
#if DEBUG
			if (ApplicationCodes.Contains(CBPEDIInterchange.ApplicationCodeForTesting))
			{
				return new Testing.BlockControlReaderHelperForTest();
			}
#endif

			Hashtable types = (Hashtable)ObjectFactory.Get("BlockControlReaderHelperTypes");

			foreach (string applicationCode in ApplicationCodes)
			{
				if (types.ContainsKey(applicationCode))
				{
					ObjectHandle objectHandle = (ObjectHandle)types[applicationCode];
					return objectHandle != null ? (IBlockControlReaderHelper)objectHandle.GetObject() : null;
				}
			}
			return null;
		}

		void DeserialiseBBlock()
		{
			char[] buffer = new char[80];
			messageTextReader.ReadBlock(buffer, 0, 80);
			ZString first80 = new string(buffer);
			readerHelper.SetBAndYBlocks(first80, messageType, out b, out y);
			if (b != null)
			{
				b.Deserialise(first80);
			}
		}

		readonly TextReader messageTextReader;
		readonly IBlockControlReaderHelper readerHelper;
		readonly ZString messageType;
	}
}

#if DEBUG
namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	class BlockControlReaderHelperForTest : IBlockControlReaderHelper
	{
		#region IBlockControlReaderHelper Members

		void IBlockControlReaderHelper.SetBAndYBlocks(ZString first80, ZString applicationIdentifier, out IControlMessageBlockB b, out IControlMessageBlockY y)
		{
			if (applicationIdentifier == ApplicationIdentifierCodeList.DummyForTesting2)
			{
				b = new ZZZB2();
				y = new ZZZY2();
			}
			else
			{
				b = new ZZZB();
				y = new ZZZY();
			}
		}

		bool IBlockControlReaderHelper.IsYBlockData(char[] buffer)
		{
			return buffer.Length > 3 && buffer[0] == 'Z' && buffer[1] == '¿' && buffer[2] == 'º' && buffer[3] == 'Y';
		}

		#endregion
	}
}
#endif
