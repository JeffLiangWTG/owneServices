using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class DocAddressTo30BlocksMapperTest : TestCaseWithFactory
	{
		public void TestRemovalOfIllegalCharacters()
		{
			var docAddress = Factory.New<ISFDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "NAME*";
			docAddress.E2_Contact = "CONTACT*";
			docAddress.E2_City = "City*";
			docAddress.E2_State = "TT*";
			docAddress.E2_Postcode = "1024*";
			docAddress.E2_Address1 = "ADDRESS1*";
			docAddress.E2_Address2 = "ADDRESS2*";
			I30Blocks block30Data = DocAddressTo30BlocksMapper.New(new SanitizedISFDocAddressWrapper(docAddress));
			AssertEquals("NAME ", block30Data.EntityName);
			AssertEquals("CONTACT ", block30Data.LegalName);
			AssertEquals("CITY ", block30Data.City);
			AssertEquals("TT ", block30Data.CountrySubEntityCode);
			AssertEquals("1024 ", block30Data.PostalCode);
			AssertEquals("ADDRESS1ADDRESS2", block30Data.AddressingInformation.ElementAt(0).AddressInformation1);
		}
	}
}
