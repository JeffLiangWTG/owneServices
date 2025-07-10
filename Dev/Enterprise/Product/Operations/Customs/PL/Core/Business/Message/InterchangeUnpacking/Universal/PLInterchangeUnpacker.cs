using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Customs.PL.MessageContracts.DataProviders;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

[assembly: UniversalCustomsInterchangeUnpacker(ApplicationCodeList.Codes.PLCustoms, typeof(Enterprise.Customs.PL.Business.PLInterchangeUnpacker))]
[assembly: UniversalCustomsInterchangeUnpacker(ApplicationCodeList.Codes.PLCustomsNCTS, typeof(Enterprise.Customs.PL.Business.PLInterchangeUnpacker))]
[assembly: UniversalCustomsInterchangeUnpacker(ApplicationCodeList.Codes.PLCustomsExitControl, typeof(Enterprise.Customs.PL.Business.PLInterchangeUnpacker))]

namespace Enterprise.Customs.PL.Business;

public class PLInterchangeUnpacker : IUniversalCustomsInterchangeUnpacker
{
	[ThreadSafe]
	static DataProviderFactory DataProviderFactory { get; } = new(RecognizableMessages.All);

	[ThreadSafe]
	static readonly IXmlInterchangeUnpackingStrategy[] ProcessingStrategies = [
		new UniversalEventUnpackingStrategy(),
		new FaultInterchangeUnpackingStrategy(DataProviderFactory),
		new AcceptDocumentResponseUnpackingStrategy(DataProviderFactory),
		new GetDocumentsResponseUnpackingStrategy(DataProviderFactory),
	];

	public IUniversalCustomsInterchangeUnpackerResult Unpack(
		EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EnterpriseEDIMessage outgoingMessage,
		LoggingInformation logger)
	{
		using var readContext = interchange.GetReadContext();
		try
		{
			var xmlReader = readContext.PayloadXmlReader;
			var contentXmlName = new XmlQualifiedName(xmlReader.LocalName, xmlReader.NamespaceURI);
			if (GetUnpackingStrategy(contentXmlName) is not { } processingStrategy)
			{
				return new EDIInterchangeUnpackerResult((NoResString)"Content is not recognized!");
			}
			interchange.Logs.AddNew(Events.InterchangeInProgress, $"Found {contentXmlName.Name}");
			return processingStrategy.Unpack(interchange, outgoingInterchange, outgoingMessage, xmlReader, logger);
		}
		catch (Exception e)
		{
			return new EDIInterchangeUnpackerResult(string.Join((NoResString)" — ", CargoWise.Common.ExceptionExtensions.FlattenInnerExceptions(e).Select(x => x.Message)));
		}
	}

	protected virtual IXmlInterchangeUnpackingStrategy GetUnpackingStrategy(XmlQualifiedName rootNode)
		=> XmlNodeNameToProcessingStrategyDictionary.TryGetValue(rootNode, out var processingStrategy)
			? processingStrategy
			: null;

	IReadOnlyDictionary<XmlQualifiedName, IXmlInterchangeUnpackingStrategy> XmlNodeNameToProcessingStrategyDictionary
		=> xmlNodeNameToProcessingStrategyDictionary ??= ProcessingStrategies
			.SelectMany(x => x.SupportedXmlNodes, (strategy, xmlNodeName) => (xmlNodeName, strategy))
			.DistinctBy(x => x.xmlNodeName)
			.ToDictionary(x => x.xmlNodeName, x => x.strategy);
	IReadOnlyDictionary<XmlQualifiedName, IXmlInterchangeUnpackingStrategy> xmlNodeNameToProcessingStrategyDictionary;
}
