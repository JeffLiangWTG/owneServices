using CargoWise.RefDbRepo.NZReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests.Concessions.Builders
{
	public class ConsolidatedListOfApprovalTest
	{
		[Test]
		public void TestGetConsolidatedListOfApprovalFromJson()
		{
			var consolidatedListOfApproval = ConsolidatedListOfApproval.GetConsolidatedListOfApprovalFromJson("{\"concessionCode\" : \"311299D\",\"tariffItem\" : \"0402.10.00)\\n2104.10.09\",\"description\" : \"Cappuccino topping, soup and whitener, of a type used in hot beverage\\ndispensers\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}");
			Assert.That(consolidatedListOfApproval.concessionCode, Is.EqualTo("311299D"), "concessionCode");
			Assert.That(consolidatedListOfApproval.tariffItem, Is.EqualTo("0402.10.00)\n2104.10.09"), "tariffItem");
			Assert.That(consolidatedListOfApproval.description, Is.EqualTo("Cappuccino topping, soup and whitener, of a type used in hot beverage\ndispensers"), "description");
			Assert.That(consolidatedListOfApproval.normalTariff, Is.EqualTo("Free"), "normalTariff");
			Assert.That(consolidatedListOfApproval.preferentialTariff, Is.EqualTo("Free"), "preferentialTariff");
			Assert.That(consolidatedListOfApproval.part2Ref, Is.EqualTo("99"), "part2Ref");
			Assert.That(consolidatedListOfApproval.effectiveFrom, Is.EqualTo("04/21"), "effectiveFrom");
			Assert.That(consolidatedListOfApproval.effectiveTo, Is.EqualTo(".."), "effectiveTo");
			Assert.That(consolidatedListOfApproval.scheduleNo, Is.EqualTo("1"), "scheduleNo");
			Assert.That(consolidatedListOfApproval.docTitle, Is.EqualTo("CH03-38 | tariff-concession-approval-notice-10-2021"), "docTitle");
			Assert.That(consolidatedListOfApproval.docUrl, Is.EqualTo(""), "docUrl");
		}

		[Test]
		public void TestGetConsolidatedListOfApprovalsFromJson()
		{
			var loggerMock = new Mock<ILogger>();
			var consolidatedListOfApprovals = ConsolidatedListOfApproval.GetConsolidatedListOfApprovalsFromJson(new[]
				{
					"{\"concessionCode\" : \"311299D\",\"tariffItem\" : \"0402.10.00)\\n2104.10.09\",\"description\" : \"Cappuccino topping, soup and whitener, of a type used in hot beverage\\ndispensers\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}",
					"{\"concessionCode\" : \"981314E\",\"tariffItem\" : \"8424.90.28\",\"description\" : \"Nozzles, &hellip; systems\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"\", \"effectiveTo\" : \"\",\"scheduleNo\" : \"3\",\"docTitle\" : \"TARIFF CONCESSION APPROVALS, WITHDRAWALS AND DECLINES NOTICE (NO. 12) 2006\",\"docUrl\" : \"\"}",
					"not json line",
				}, loggerMock.Object);

			CollectionAssert.AreEquivalent(new[] { "311299D", "981314E" }, consolidatedListOfApprovals.Keys);
			loggerMock.Verify(x => x.LogError("Cannot deserialize ConsolidatedListOfApproval from line: not json line"), Times.Once);
		}
	}
}
