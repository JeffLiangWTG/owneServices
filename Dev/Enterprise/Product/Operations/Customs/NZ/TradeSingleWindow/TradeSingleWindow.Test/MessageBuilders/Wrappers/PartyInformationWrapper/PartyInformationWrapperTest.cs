using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class PartyInformationWrapperTest : TestCaseWithFactory
	{
		public void TestWithSingleAddress()
		{
			IPartyInformation informationWrapper = new PartyInformationWrapper("TESTNAME", "TESTCITY", "TESTCOUNTRYCODE", "TESTCOUNTRYREGION", "       TEST ADDRESSS1      ", ZString.Empty, "TSTPCODE", ZString.Empty, null);
			AssertEquals("TESTNAME", informationWrapper.Name);
			AssertEquals("TESTCITY", informationWrapper.City);
			AssertEquals("TESTCOUNTRYCODE", informationWrapper.CountryCode);
			AssertEquals("TESTCOUNTRYREGION", informationWrapper.CountryRegion);
			AssertEquals("TEST ADDRESSS1", informationWrapper.Address);
			AssertEquals("TSTPCODE", informationWrapper.PostCode);
		}

		public void TestWithDualAddress()
		{
			IPartyInformation informationWrapper = new PartyInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "       TEST ADDRESSS1      ", "       TEST ADDRESSS2   ", ZString.Empty, ZString.Empty, null);
			AssertEquals("TEST ADDRESSS1 TEST ADDRESSS2", informationWrapper.Address);
		}

		public void TestWithAddressOverMaximumLength()
		{
			IPartyInformation informationWrapper = new PartyInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567890", ZString.Empty, ZString.Empty, null);
			AssertEquals("1234567890123456789012345678901234567890 12345678901234567890123456789", informationWrapper.Address);
		}

		public void TestPostCodeOverMaximumLength()
		{
			IPartyInformation informationWrapper = new PartyInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "1234567890", ZString.Empty, null);
			AssertEquals("123456789", informationWrapper.PostCode);
		}

		public void TestPostCodesStripInvalidCharacters()
		{
			IPartyInformation informationWrapper = new PartyInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "37373-7780", ZString.Empty, null);
			AssertEquals("PostCode should have had invalid character removed & if PostCode length is greater than allowed in message should be trimmed", "373737780", informationWrapper.PostCode);

			informationWrapper = new PartyInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "45001SWIFT", ZString.Empty, null);
			AssertEquals("Long PostCode should have been truncated to the message maximum length of 9 characters", "45001SWIF", informationWrapper.PostCode);
		}

		public void TestCommunicationsDoesNotReturnNull()
		{
			IPartyInformation informationWrapper = new PartyInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null);
			AssertEquals(Enumerable.Empty<ICommunication>(), informationWrapper.Communications);
		}

		public void TestCommunicationsFromOrgHeader()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			orgAddress.OA_Phone = "+4234232";
			IPartyInformation informationWrapper = new PartyInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, org);
			AssertEquals(1, informationWrapper.Communications.Count());
			var communication = informationWrapper.Communications.First();
			AssertEquals("4234232", communication.ContactDetail);
			AssertEquals(CommunicationTypeList.Codes.TE, communication.ContactType);
		}

		public void TestCommunicationsFromPhone()
		{
			IPartyInformation informationWrapper = new PartyInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "+4234232", null);
			AssertEquals(1, informationWrapper.Communications.Count());
			var communication = informationWrapper.Communications.First();
			AssertEquals("4234232", communication.ContactDetail);
			AssertEquals(CommunicationTypeList.Codes.TE, communication.ContactType);
		}

		public void TestPartyInformationWrapperIsEmpty()
		{
			IPartyInformation informationWrapper = new PartyInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null);
			AssertEquals("informationWrapper.IsEmpty", true, informationWrapper.IsEmpty);
		}
	}
}
