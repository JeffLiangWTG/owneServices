using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class TraderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckE2_AddressType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(Trader.E2_AddressTypeInfo, "CAR", "SEL");
			ValidationTestHelper.AssertInvalidCodeMessageError(Trader.E2_AddressTypeInfo, "BOF", "BUY");
		}

		public void TestCheckTraderTypeMaximum()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var traders = declaration.Traders;
			for (int i = 0; i < 5; i++)
			{
				traders.AddNew();
				traders.AddNew().E2_AddressType = "SEL";
			}

			var trader = traders.AddNew();
			trader.Validation.ValidateE2_AddressType();
			AssertNoMessageErrorContaining("There can be maximum 6 company", trader.E2_AddressTypeInfo, "There can be maximum 6 company for BUY type.");

			trader = traders.AddNew();
			trader.Validation.ValidateE2_AddressType();
			AssertHasMessageErrorContaining(trader.E2_AddressTypeInfo, "There can be maximum 6 company for BUY type.");

			trader.E2_AddressType = "SEL";
			AssertNoMessageErrorContaining("There can be maximum 6 company for BUY type.", trader.E2_AddressTypeInfo, "There can be maximum 6 company for BUY type.");

			trader = traders.AddNew();
			trader.E2_AddressType = "SEL";
			AssertHasMessageErrorContaining(trader.E2_AddressTypeInfo, "There can be maximum 6 company for SEL type.");
		}

		public void TestParent()
		{
			AssertType<Trader>(Trader.Validation.Parent);
		}

		public void TestCheckE2_OA_Address()
		{
			Trader.E2_AddressType = "SEL";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Trader.E2_OA_AddressInfo);

			Trader.E2_AddressType = "";
			Trader.E2_OA_Address = ZGuid.Empty;
			Trader.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrors(Trader.E2_OA_AddressInfo);
		}

		public void TestCheckOrganisationPK()
		{
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;

			var org2 = Factory.New<OrgHeader>();
			var mainAddress2 = org2.MainAddress;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var traders = declaration.Traders;

			var trader = traders.AddNew();
			trader.E2_AddressType = "BUY";
			trader.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(trader.OrganisationPKInfo, "You have not entered a Buyer Documentary Address: Organization.");

			trader.E2_AddressType = "SEL";
			trader.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(trader.OrganisationPKInfo, "You have not entered a Seller Documentary Address: Organization.");

			trader.OrganisationPK = org.PK;
			trader.E2_OA_Address = mainAddress.PK;
			trader.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("Not entered a Documentary Address", trader.OrganisationPKInfo, "You have not entered a Seller Documentary Address: Organization.");

			var trader2 = traders.AddNew();
			trader2.E2_AddressType = "SEL";
			trader2.OrganisationPK = org.PK;
			trader2.E2_OA_Address = mainAddress.PK;
			trader2.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(trader2.OrganisationPKInfo, "Duplicate Organization not allowed.");

			trader2.OrganisationPK = org2.PK;
			trader2.E2_OA_Address = mainAddress2.PK;
			trader2.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("Duplicate Organization not allowed", trader2.OrganisationPKInfo, "Duplicate Organization not allowed.");
		}

		Trader Trader => trader ?? (trader = Factory.New<Trader>());
		Trader trader;
	}
}
