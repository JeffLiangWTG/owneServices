using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Customs.PL.MessageContracts.DataProviders;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.PL.Business;

abstract class XmlInterchangeUnpackingStrategy<TDataProviderInterface>(DataProviderFactory dataProviderFactory) : IXmlInterchangeUnpackingStrategy
	where TDataProviderInterface : class
{
	protected DataProviderFactory DataProviderFactory { get; } = Argument.NotNull(dataProviderFactory, nameof(dataProviderFactory));

	[ThreadSafe]
	static readonly XmlQualifiedName[] SupportedXmlNodes
		= RecognizableMessages.DocumentHandlingPort.Where(x => x.CompatibleDataProviderInterfaces.Contains(typeof(TDataProviderInterface))).Select(x => x.XmlNode).ToArray();
	IReadOnlyCollection<XmlQualifiedName> IXmlReadingStrategy.SupportedXmlNodes => SupportedXmlNodes;

	public EDIInterchangeUnpackerResult Unpack(
		EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EnterpriseEDIMessage outgoingMessage,
		XmlReader reader,
		ISimpleLogger logger)
	{
		if (outgoingMessage is null)
		{
			return new EDIInterchangeUnpackerResult(errorReason:
				(NoResString)"Interchange processing failed because related transmit message couldn't be located.");
		}

		return DataProviderFactory.NewOrNull<TDataProviderInterface>(reader) is { } dataProvider
			? ProcessDataCore(interchange, outgoingInterchange, outgoingMessage, dataProvider, logger)
			: new EDIInterchangeUnpackerResult(errorReason: (NoResString)"Failed to read xml and create a data provider!");
	}

	protected abstract EDIInterchangeUnpackerResult ProcessDataCore(
		EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EnterpriseEDIMessage outgoingMessage,
		TDataProviderInterface dataProvider,
		ISimpleLogger logger);
}
