using System;
using System.Text;
using CargoWise.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public abstract class DeliveryStreamWrapperAbstract : IDeliveryStreamWrapper
	{
		public DeliveryStreamWrapperAbstract(SubStreamableStream content, IEntityInfo sourceInfo)
			: this(content)
		{
			this.SourceInfo = Argument.NotNull(sourceInfo, nameof(sourceInfo));
		}

		//Use this constructor only if there is no need to link the EDIMessage to any BizObj
		public DeliveryStreamWrapperAbstract(SubStreamableStream content)
		{
			Content = content;
		}

		public SubStreamableStream Content { get; set; }
		public IEntityInfo SourceInfo { get; }
		SubStreamableStream messageContent;

		public abstract void PopulateStream(BusinessObjectFactory factory);

		public abstract void SetMessageNumber(MessageNumberType messageNumberType, ZString zString);

		public abstract void SetTimestamp(long timestamp);

		public SubStreamableStream GetMessageStream()
		{
			if (messageContent == null)
			{
				messageContent = GetMessageStreamWithNoXmlDeclaration(Content);
			}
			return messageContent;
		}

		SubStreamableStream GetMessageStreamWithNoXmlDeclaration(SubStreamableStream stream)
		{
			stream.SeekBegin();

			long contentStart = 0;
			var open = (int)'<';
			var close = (int)'>';
			StringBuilder firstTag = null;
			for (var counter = 0; counter < stream.Length; counter++)
			{
				var c = stream.ReadByte();
				if (c == -1)
				{
					break;
				}
				else if (firstTag != null)
				{
					firstTag.Append((char)c);
					if (c == close)
					{
						if (firstTag.ToString().IndexOf("?xml", StringComparison.OrdinalIgnoreCase) != -1)
						{
							while (char.IsWhiteSpace((char)stream.ReadByte()))
							{
								counter++;
							}
							contentStart = counter + 1;
						}
						break;
					}
				}
				else if (c == open)
				{
					firstTag = new StringBuilder();
					firstTag.Append((char)c);
				}
			}

			stream.SeekBegin();

			if (contentStart == 0)
			{
				return stream;
			}

			return stream.GetSubStream(contentStart, stream.Length - contentStart);
		}
	}
}
