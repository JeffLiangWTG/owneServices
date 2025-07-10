using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.NO.Business;

sealed class InterchangeUnpackerFactory
{
	public static IUniversalCustomsInterchangeUnpacker GetInterchangeUnpacker(string interchangeType)
	{
		return interchangeType switch
		{
			Constant.MessageTypes.XLG => new XLGInterchangeUnpacker(),
			_ => null,
		};
	}
}
