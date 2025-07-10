using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS.Tests
{
	class NotificationTypeProducerTest : RevenueCodeListProducerAbstractTest
    {
        protected override string ExpectedTestFileName => "NCTS.Expected_NotificationType";

		protected override IRevenueCodeListDetails GetCodeListDetails() => new NotificationType();
	}
}
