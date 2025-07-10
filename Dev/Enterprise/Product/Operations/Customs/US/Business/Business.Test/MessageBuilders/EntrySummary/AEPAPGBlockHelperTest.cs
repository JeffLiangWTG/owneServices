using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AEPAPGBlockHelperTest : TestCaseWithFactory
	{
		public void TestAEPAPGMaxLengths()
		{
			AssertEquals(32, AEPAPGBlockHelper.AEPAPG19EntityNameMaxLength);
			AssertEquals(15, AEPAPGBlockHelper.AEPAPG19EntityNumberMaxLength);
			AssertEquals(23, AEPAPGBlockHelper.AEPAPG19EntityAddress1MaxLength);
			AssertEquals(32, AEPAPGBlockHelper.AEPAPG20EntityAddress2MaxLength);
			AssertEquals(35, AEPAPGBlockHelper.AEPAPG21EmailMaxLength);
			AssertEquals(23, AEPAPGBlockHelper.AEPAPG21IndividualNameMaxLength);
		}

		public void TestMakePG19WithNumber()
		{
			var pg19 = AEPAPGBlockHelper.MakePG19WithNumber("ASD", "1234567890123456");
			AssertEquals("EntityRoleCode", "ASD", pg19.EntityRoleCode);
			AssertEquals("EntityNumber", "123456789012345", pg19.EntityNumber);
		}

		public void TestMakePG19WithoutOverflowingBlock()
		{
			var org = Factory.New<OrgHeader>();
			var orgWrapper = OrgHeaderWrapper.New(org);
			var pgaContactDetailsWithID = new PGAContactDetailsWithID(orgWrapper, "ASD", "1234567890123456");
			var pg19 = AEPAPGBlockHelper.MakePG19WithoutOverflowingBlock("ZXC", pgaContactDetailsWithID) as AEPAPG19;
			AssertEquals("EntityRoleCode", "ZXC", pg19.EntityRoleCode);
			AssertEquals("EntityIdentificationCode", "ASD", pg19.EntityIdentificationCode);
			AssertEquals("EntityNumber", "123456789012345", pg19.EntityNumber);
		}

		public void TestMakeEntityBlocks()
		{
			var address = Factory.New<OrgAddress>();
			var pg19 = AEPAPGBlockHelper.MakeEntityBlocks("ASD", address, idNumber: "1234567890123456").FirstOrDefault() as AEPAPG19;
			AssertEquals("EntityRoleCode", "ASD", pg19.EntityRoleCode);
			AssertEquals("EntityNumber", "123456789012345", pg19.EntityNumber);
		}

		public void MakePG02WithThreeParameters()
		{
			var pg02 = AEPAPGBlockHelper.MakePG02("A", "B", "C");
			AssertEquals("A", pg02.ItemType);
			AssertEquals("B", pg02.ProductCodeQualifier);
			AssertEquals("C", pg02.ProductCodeNumber);
		}

		public void MakePG04WithThreeParameters()
		{
			var pg04 = AEPAPGBlockHelper.MakePG04("A", "B", 1m);
			AssertEquals("A", pg04.ConstituentActiveIngredientQualifier);
			AssertEquals("B", pg04.NameOfTheConstituentElement);
			AssertEquals(1m, pg04.PercentOfConstituentElement);
		}

		public void MakePG07WithTwoParameters()
		{
			var pg07 = AEPAPGBlockHelper.MakePG07("A", "B");
			AssertEquals("A", pg07.ItemIdentityNumberQualifier);
			AssertEquals("B", pg07.ItemIdentityNumber);
		}

		public void MakePG19WithoutOverflowingBlockWithFiveParameters()
		{
			var pg19 = AEPAPGBlockHelper.MakePG19WithoutOverflowingBlock("A", "B", "C", "1234567890123456789012345678901234567890", "123456789012345678901234567890");
			AssertEquals("A", pg19.EntityRoleCode);
			AssertEquals("B", pg19.EntityIdentificationCode);
			AssertEquals("C", pg19.EntityNumber);
			AssertEquals("12345678901234567890123456789012", pg19.EntityName);
			AssertEquals("12345678901234567890123", pg19.EntityAddress1);
		}

		public void MakePG19WithoutOverflowingBlockForICustomsBrokerDetailsWithRoleCode()
		{
			var address = new Mock<IAddressDetails>();
			address.Setup(x => x.CompanyName).Returns("A");
			address.Setup(x => x.AddressLine1).Returns("B");
			var customsBrokerDetails = new Mock<ICustomsBrokerDetails>();
			customsBrokerDetails.Setup(x => x.Address).Returns(address.Object);

			var pg19 = AEPAPGBlockHelper.MakePG19WithoutOverflowingBlock("C", customsBrokerDetails.Object);
			AssertEquals("C", pg19.EntityRoleCode);
			AssertEquals(ZString.Empty, pg19.EntityIdentificationCode);
			AssertEquals(ZString.Empty, pg19.EntityNumber);
			AssertEquals("A", pg19.EntityName);
			AssertEquals("B", pg19.EntityAddress1);
		}
	}
}
