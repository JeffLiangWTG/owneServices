using System;
using System.IO;
using System.Xml;
using CargoWise.Customs.PL.MessageContracts;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.PL.Business;

sealed class InterchangeReadContext(EDIInterchange interchange) : IDisposable
{
	public EDIInterchange Interchange { get; } = interchange;

	public void Dispose()
	{
		payloadXmlReader?.Dispose();
		textReader?.Dispose();
	}

	public Stream DataStream => dataStream ??= Interchange.GetEI_BodyTextReader().CopyAndDispose();
	Stream dataStream;

	public TextReader TextReader => textReader ??= new StreamReader(DataStream);
	TextReader textReader;

	public XmlReader PayloadXmlReader => payloadXmlReader ??= CreatePayloadXmlReader();
	XmlReader payloadXmlReader;

	XmlReader CreatePayloadXmlReader()
	{
		var result = XmlHelper.CreateReaderAndGotoRootNode(TextReader);
		if (result.IsSoap())
		{
			result.MoveToSoapBody();
		}
		else if (result.IsUniversalInterchange())
		{
			result.MoveToUniversalInterchangeBody();
		}
		return result;
	}
}
