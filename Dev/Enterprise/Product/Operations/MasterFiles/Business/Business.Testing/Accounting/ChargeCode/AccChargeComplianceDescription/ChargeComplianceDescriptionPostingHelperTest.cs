using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	sealed class ChargeComplianceDescriptionPostingHelperTest : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLineAndChargeDescriptions()
		{
			var lineMock = new Mock<IDescriptionSetter>();
			var chargeMock = new Mock<ISellComplianceDescription>();

			ZString chargeDescription = "Charge Description";
			var descriptionPostingHelper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetChargeComplianceDescriptionPostingHelper();

			chargeMock.SetupProperty(x => x.Description, chargeDescription);
			chargeMock.SetupGet(x => x.SellComplianceDescription).Returns(ZString.Empty);
			Assert("Precondition:", chargeMock.Object.SellComplianceDescription.IsEmpty);

			descriptionPostingHelper.UpdateInvoiceLineAndChargeDescriptions(lineMock.Object, chargeMock.Object);

			lineMock.VerifySet(x => x.Description = chargeDescription, Times.Once);
			chargeMock.VerifySet(x => x.Description = chargeDescription, Times.Never);

			lineMock.Reset();
			chargeMock.Reset();

			chargeMock.SetupProperty(x => x.Description, chargeDescription);
			chargeMock.SetupGet(x => x.SellComplianceDescription).Returns("-Sell Compliance Description");
			var expectedDescription = "Charge Description-Sell Compliance Description";
			Assert("Precondition:", !chargeMock.Object.SellComplianceDescription.IsEmpty);
			AssertNotEquals("Precondition:", expectedDescription, chargeMock.Object.Description);

			descriptionPostingHelper.UpdateInvoiceLineAndChargeDescriptions(lineMock.Object, chargeMock.Object);

			lineMock.VerifySet(x => x.Description = expectedDescription, Times.Once);
			chargeMock.VerifySet(x => x.Description = expectedDescription, Times.Once);
		}
	}
}
