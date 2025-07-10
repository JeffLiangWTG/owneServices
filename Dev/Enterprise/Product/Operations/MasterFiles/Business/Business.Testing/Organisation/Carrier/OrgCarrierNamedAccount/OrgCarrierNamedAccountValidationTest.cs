using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.Registry.Business;
using Moq;
using WiseRates.Api.Client;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCarrierNamedAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckONA_ForeignName()
		{
			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var cna1 = Factory.New<OrgCarrierNamedAccount>();
				cna1.ONA_ForeignName = string.Empty;
				AssertNoErrors("Precondition", cna1.ONA_ForeignNameInfo);
			}

			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var cna2 = Factory.New<OrgCarrierNamedAccount>();
				AssertNoErrors("Precondition", cna2.ONA_ForeignNameInfo);

				cna2.ONA_ForeignName = string.Empty;
				AssertHasError(cna2.ONA_ForeignNameInfo, "Please enter a value.");
			}
		}

		public void TestCheckONA_ForeignName_InvalidName()
		{
			var cna1 = Factory.New<OrgCarrierNamedAccount>();
			AssertNoErrors("Precondition", cna1.ONA_ForeignNameInfo);

			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetMockWiseRatesClientFactoryForTest(["Nike"])))
			{
				cna1.ONA_ForeignName = "Invalid";
				AssertHasError(cna1.ONA_ForeignNameInfo, "Foreign Name is not valid.");

				cna1.ONA_ForeignName = ZString.Empty;
				AssertNoError(cna1.ONA_ForeignNameInfo, "Foreign Name is not valid.");
			}
		}

		public void TestCheckONA_ForeignName_CaseSensitive()
		{
			var cna1 = Factory.New<OrgCarrierNamedAccount>();

			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetMockWiseRatesClientFactoryForTest(["Nike"])))
			{
				cna1.ONA_ForeignName = "Nike";
				AssertNoErrors("Precondition", cna1.ONA_ForeignNameInfo);

				cna1.ONA_ForeignName = "nike";
				AssertHasError(cna1.ONA_ForeignNameInfo, "Foreign Name is not valid.");
			}
		}

		public void TestCheckONA_ForeignName_UniqueForSameCarrier()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "TESTCAR123";
			var cna1 = Factory.New<OrgCarrierNamedAccount>();
			cna1.ONA_OH_Carrier = carrier.PK;
			cna1.ONA_OH_Organization = carrier.PK;
			var cna2 = Factory.New<OrgCarrierNamedAccount>();
			cna2.ONA_OH_Carrier = carrier.PK;
			cna2.ONA_OH_Organization = carrier.PK;

			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetMockWiseRatesClientFactoryForTest(["Nike", "Nike Inc."])))
			{
				cna1.ONA_ForeignName = "Nike";
				cna2.ONA_ForeignName = "Nike Inc.";

				AssertNoErrors("Precondition", cna1.ONA_ForeignNameInfo);
				AssertNoErrors("Precondition", cna2.ONA_ForeignNameInfo);

				cna2.ONA_ForeignName = "Nike";
				AssertHasError(cna2.ONA_ForeignNameInfo, "Named account Nike is already mapped to an Organization TESTCAR123, under carrier TESTCAR123.");
			}
		}

		public void TestCheckONA_ForeignName_UniqueForNoCarrier()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "TESTCAR123";

			var cna1 = Factory.New<OrgCarrierNamedAccount>();
			cna1.ONA_OH_Organization = carrier.PK;
			var cna2 = Factory.New<OrgCarrierNamedAccount>();
			cna2.ONA_OH_Organization = carrier.PK;

			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetMockWiseRatesClientFactoryForTest(["Nike", "Nike Inc."])))
			{
				cna1.ONA_ForeignName = "Nike";
				cna2.ONA_ForeignName = "Nike Inc.";

				AssertNoErrors("Precondition", cna1.ONA_ForeignNameInfo);
				AssertNoErrors("Precondition", cna2.ONA_ForeignNameInfo);

				cna2.ONA_ForeignName = "Nike";
				AssertHasError(cna2.ONA_ForeignNameInfo, "Named account Nike is already mapped to an Organization TESTCAR123, under no carrier.");

				cna2.ONA_OH_Carrier = carrier.PK;
				cna2.RunPreSaveValidation();
				AssertNoErrors("When carrier is different, there should be no error.", cna2.ONA_ForeignNameInfo);
			}
		}

		public void TestCheckONA_ForeignName_NoOrganisation()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "TESTCAR123";

			var cna1 = Factory.New<OrgCarrierNamedAccount>();
			var cna2 = Factory.New<OrgCarrierNamedAccount>();

			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetMockWiseRatesClientFactoryForTest(["Nike", "Nike Inc.", "Test Account", "Test Account 2"])))
			{
				cna1.ONA_ForeignName = "Nike";
				cna2.ONA_ForeignName = "Nike";

				AssertNoErrors("Precondition", cna1.ONA_ForeignNameInfo);
				AssertNoErrors("Precondition", cna2.ONA_ForeignNameInfo);

				cna1.ONA_OH_Organization = carrier.PK;
				cna1.RunPreSaveValidation();
				cna2.RunPreSaveValidation();

				AssertNoErrors("Should have no errors.", cna1.ONA_ForeignNameInfo);
				AssertHasError(cna2.ONA_ForeignNameInfo, "Named account Nike is already mapped to an Organization TESTCAR123, under no carrier.");
			}
		}

		public void TestCheckONA_OH_Organization()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "TESTCAR123";

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var regularOrg = Factory.New<OrgHeader>();

			var cna = Factory.New<OrgCarrierNamedAccount>();
			cna.ONA_OH_Carrier = carrier.PK;
			cna.ONA_ForeignName = "Test Name";
			cna.ONA_OH_Organization = consignor.PK;

			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				cna.ONA_OH_Organization = regularOrg.PK;
				AssertNoErrors("Precondition", cna.ONA_OH_OrganizationInfo);
			}

			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				cna.ONA_OH_Organization = regularOrg.PK;
				AssertHasError(cna.ONA_OH_OrganizationInfo, "The entered value does not correspond to a valid Organization of type Consignor, Consignee or Controlling Customer.");
			}
		}

		static IWiseRatesClientFactory GetMockWiseRatesClientFactoryForTest(IEnumerable<string> namedAccounts)
		{
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(m => m.GetNamedAccounts(It.IsAny<string>()))
				.Returns(namedAccounts.ToHashSet());

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			return wiseRatesClientFactoryMock.Object;
		}
	}
}
