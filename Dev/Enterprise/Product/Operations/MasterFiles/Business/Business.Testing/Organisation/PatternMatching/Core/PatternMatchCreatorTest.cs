using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching.Testing
{
	sealed class PatternMatchCreatorTest : TestCaseWithFactory
	{
		public void TestConstructorChecksForNulls()
		{
			var mockDataManager = new Mock<IPatternMatchDataManager>();
			mockDataManager.Setup(m => m.GetPatternMatchesAlreadyLoaded()).Returns(new List<IOrgPatternMatch>());

			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new PatternMatchCreator(null); });
			AssertNoExceptionThrown(delegate
			{ new PatternMatchCreator(mockDataManager.Object); });
		}

		public void TestGeneratePatternMatchBadCompanyNameFails()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "NAME";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.MainAddress.OA_Address1 = "Address1";
			header.MainAddress.OA_City = "City";

			AssertEncoding(true, "Company Name Empty", header, String.Empty, header.MainAddress, false);
			AssertEncoding(false, "Company Name is Good - Should Succeed", header, "Company Name", header.MainAddress, false);
		}

		public void TestGeneratePatternMatchBadAddressFails()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "NAME";
			header.OH_RL_NKClosestPort = "AUSYD";

			header.MainAddress.OA_Address1 = String.Empty;
			AssertEncoding(true, "Address1 Empty", header, header.OH_FullName, header.MainAddress, false);

			AssertEncoding(false, "Address1 Empty", header, header.OH_FullName, header.MainAddress, true);

			header.MainAddress.OA_Address1 = OrgAddress.AddressNotOnFile;
			AssertEncoding(true, "Address1 set to AddressNotOnFile", header, header.OH_FullName, header.MainAddress, false);

			header.MainAddress.OA_Address1 = "Address";
			AssertEncoding(false, "Address Is Good - Should Succeed", header, header.OH_FullName, header.MainAddress, false);
		}

		public void TestGeneratePatternMatchNonEnglishCharacters()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "NAME";
			header.OH_RL_NKClosestPort = "AUSYD";

			header.MainAddress.OA_Address1 = "Address";
			AssertEncoding(false, "Address Is Good - Should Succeed", header, header.OH_FullName, header.MainAddress, false);

			header.OH_FullName = "NAME" + char.ConvertFromUtf32(21271);
			AssertEncoding(false, "Non-English character inside, but can still generate pattern match", header, header.OH_FullName, header.MainAddress, false);

			header.OH_FullName = "NAME";
			header.MainAddress.OA_Address1 = "Address" + char.ConvertFromUtf32(24038);
			AssertEncoding(false, "Non-English character inside, but can still generate pattern match", header, header.OH_FullName, header.MainAddress, false);

			header.MainAddress.OA_Address1 = "Address";
			header.MainAddress.OA_City = char.ConvertFromUtf32(21271) + char.ConvertFromUtf32(24038);
			AssertEncoding(false, "Non-English character inside, but can still generate pattern match", header, header.OH_FullName, header.MainAddress, false);

			header.MainAddress.OA_City = "City";
			header.MainAddress.OA_State = char.ConvertFromUtf32(21271) + char.ConvertFromUtf32(24038);
			AssertEncoding(false, "Non-English character inside, but can still generate pattern match", header, header.OH_FullName, header.MainAddress, false);
		}

		void AssertEncoding(bool expectToFail, string expectedMessage, OrgHeader header, string companyName, OrgAddress address, bool allowEmptyAddress)
		{
			AssertEquals(expectedMessage, expectToFail, !PatternMatchCreator.CanGeneratePatternMatchForTesting(header, new OrganisationName(companyName, header.OH_Language), address, allowEmptyAddress));
		}
	}
}
