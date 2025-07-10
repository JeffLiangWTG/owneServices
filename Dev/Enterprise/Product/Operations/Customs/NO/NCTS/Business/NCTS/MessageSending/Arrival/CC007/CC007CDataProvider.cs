using CargoWise.Customs.NO.MessageContracts.NCTS;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.NO.NCTS.Business.MessageSending.Arrival;

[GenerateDataProvider(
	typeof(CC007CTypeMessageBuilderMetadata),
	typeof(NctsHeaderMessageSendingObject))]
public static partial class CC007CTypeDataProvider
{
}

public partial interface ICC007CTypeAdditionalDataProvider
{
}
