using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(OrganizationAddressFormatted))]
	public class OrganizationAddressFormattedTest : DataObjectTestCase<OrganizationAddressFormatted>
	{
		public void TestPhoneNumberIsFormatted()
		{
			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "TestType",
				AddressShortCode = "THEMOMENT",
				AddressOverride = false,

				OrganizationCode = "TESTYO2",
				CompanyName = "ORGANISATION",
				Address1 = "PADDINGTON NSW",
				Address2 = "",
				City = "",
				State = "",
				Postcode = "",
				Port = new UNLOCO { Code = "AUMEL", Name = "Melbourne" },
				Country = new Country { Code = "AU", Name = "Australia" },

				Fax = "",
				Mobile = "",
				Phone = "0011 61 256028000"
			};

			var testReaderObject = new OrganizationAddressFormatted(organizationAddress);
			AssertEquals("Phone number should be formatted", "+61256028000", testReaderObject.Phone);
		}

		protected override void TestXsdGenerationDoesntThrowAnyExceptions_2011_11Core()
		{
			Assert("The class is used as a substitute for OrganizationAddress, forcing formatting on all consumers of this class. This should never be in the schema", true);
		}
	}
}
