using System;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

[assembly: UniversalCustomsInterchangeUnpacker(ApplicationCodeList.Codes.NOCustomsEmma, typeof(Enterprise.Customs.NO.Business.EMMAMessageUnpacker))]

namespace Enterprise.Customs.NO.Business;

sealed class EMMAMessageUnpacker : IUniversalCustomsInterchangeUnpacker
{
	IUniversalCustomsInterchangeUnpackerResult IUniversalCustomsInterchangeUnpacker.Unpack(
		EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EDIMessage outgoingMessage,
		LoggingInformation logger)
	{
		_ = Argument.NotNull(interchange, nameof(interchange));
		_ = Argument.NotNull(outgoingMessage, nameof(outgoingMessage));
		_ = Argument.NotNull(logger, nameof(logger));

		logger.Log(LogType.Information, FormattableString.Invariant($"Unpacking start for the EDIInterchange [{interchange.PK}]."));

		var interchangeUnpacker = InterchangeUnpackerFactory.GetInterchangeUnpacker(interchange.EI_InterchangeType);
		if (interchangeUnpacker == null)
		{
			return new EDIInterchangeUnpackerResult((NoResString)$"EDI Interchange Unpacker not found for an EDIInterchange [{interchange.PK}] with type [{interchange.EI_InterchangeType}].");
		}

		var unpackerResult = interchangeUnpacker.Unpack(interchange, outgoingInterchange, outgoingMessage, logger);

		logger.Log(LogType.Information, FormattableString.Invariant($"Unpacking finished for the EDIInterchange [{interchange.PK}]."));
		return unpackerResult;
	}
}
