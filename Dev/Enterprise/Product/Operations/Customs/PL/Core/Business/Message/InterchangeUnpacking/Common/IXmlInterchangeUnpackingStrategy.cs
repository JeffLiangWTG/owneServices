using System.Xml;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.PL.Business;

public interface IXmlInterchangeUnpackingStrategy : IXmlReadingStrategy
{
	EDIInterchangeUnpackerResult Unpack(
		EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EnterpriseEDIMessage outgoingMessage,
		XmlReader reader,
		ISimpleLogger logger);
}
