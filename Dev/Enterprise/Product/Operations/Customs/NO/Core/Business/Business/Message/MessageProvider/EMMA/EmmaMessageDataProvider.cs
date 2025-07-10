using CargoWise.Customs.NO.MessageContracts.EMMA;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.NO.Business;

[GenerateDataProvider(
	typeof(EmmaSystemsFortollingMessageBuilderMetadata),
	typeof(EmmaMessageEntryHeaderWrapper))
]
public static partial class EmmaMessageDataProvider
{
}

public partial interface IEmmaSystemsFortollingAdditionalDataProvider
{
}
