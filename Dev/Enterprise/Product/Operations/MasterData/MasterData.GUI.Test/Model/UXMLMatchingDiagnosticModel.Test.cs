using System.Globalization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.GUI.Tests
{
	public class UXMLMatchingDiagnosticModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var testGuid = ZGuid.NewZGuid();
			var model = new UXMLMatchingDiagnosticModel(
				matchedOrgCode: "ABC",
				matchedOrgName: "ABC INC",
				true,
				matchedAddressCode: "XYZ",
				orgScore: 0.42,
				addressScore: 0.54,
				result: UXMLMatchingDiagnosticUtils.Constants.NotMatch,
				orgPK: testGuid
				);

			CombineAssertions(() =>
			{
				AssertEquals("ABC", model.MatchedOrgCode);
				AssertEquals("ABC INC", model.MatchedOrgName);
				AssertEquals(true, model.MatchOrgActiveStatus);
				AssertEquals("XYZ", model.MatchedAddressCode);
				AssertEquals(0.42, model.OrgScoreValue);
				AssertEquals(0.54, model.AddressScoreValue);
				AssertEquals(UXMLMatchingDiagnosticUtils.Constants.NotMatch, model.Result);
				AssertEquals(testGuid, model.OrgPK);
			});
		}

		public void TestConstructFromBizos()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ZXC";
			org.OH_FullName = "ZXC PTY LTD";
			org.OH_IsActive = false;

			var address = org.Addresses.AddNew();
			address.OA_Code = "JKL";
			address.OA_Address1 = "42 Wallaby Way";

			Factory.Save();

			var model = new UXMLMatchingDiagnosticModel(
				matchedOrgHeader: org,
				matchedOrgAddress: address,
				orgScore: 0.42,
				addressScore: 0.54,
				result: UXMLMatchingDiagnosticUtils.Constants.NotMatch
				);

			CombineAssertions(() =>
			{
				AssertEquals(org.OH_Code, model.MatchedOrgCode);
				AssertEquals(org.OH_FullName, model.MatchedOrgName);
				AssertEquals(org.OH_IsActive, model.MatchOrgActiveStatus);
				AssertEquals(address.OA_Code, model.MatchedAddressCode);
				AssertEquals(org.PK, model.OrgPK);
			});
		}

		public void TestFormatScores()
		{
			var model1 = new UXMLMatchingDiagnosticModel(
				matchedOrgCode: "ABC",
				matchedOrgName: "ABC INC",
				true,
				matchedAddressCode: "PQR",
				orgScore: 0.42,
				addressScore: 1,
				result: UXMLMatchingDiagnosticUtils.Constants.MatchAndSelected,
				orgPK: ZGuid.NewZGuid()
				);

			var model2 = new UXMLMatchingDiagnosticModel(
				matchedOrgCode: "DEF",
				matchedOrgName: "DEF INC",
				true,
				matchedAddressCode: "XYZ",
				orgScore: 0.05,
				addressScore: 0,
				result: UXMLMatchingDiagnosticUtils.Constants.NotMatch,
				orgPK: ZGuid.NewZGuid()
				);

			CombineAssertions(() =>
			{
				AssertEquals("42%", model1.OrgScore);
				AssertEquals("100%", model1.AddressScore);
				AssertEquals("5%", model2.OrgScore);
				AssertEquals("0%", model2.AddressScore);
			});
		}

		public void TestMatchInfo()
		{
			var model = new UXMLMatchingDiagnosticModel(
				matchedOrgCode: "QWE",
				matchedOrgName: "QWE INC",
				true,
				matchedAddressCode: "ASD",
				orgScore: 0.42,
				addressScore: 0.54,
				result: UXMLMatchingDiagnosticUtils.Constants.NotMatch,
				orgPK: ZGuid.NewZGuid()
				);

			AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0}: {1}\t\t{2}: {3}",
				UXMLMatchingDiagnosticModel.Labels.OrgCode,
				"QWE",
				UXMLMatchingDiagnosticModel.Labels.OrgName,
				"QWE INC"
			), model.OrgMatchInfo);
		}

		public void TestMatchInfo_SeparatedFields()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "QWE";
			orgHeader.OH_FullName = "QWE INC";
			orgHeader.OH_IsActive = true;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_PostCode = "Post code";
			address.OA_City = "City";
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			address.OA_Email = "Email";
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Code = "ASD";

			var model = new UXMLMatchingDiagnosticModel(
				orgHeader,
				address,
				orgScore: 0.42,
				addressScore: 0.54,
				result: UXMLMatchingDiagnosticUtils.Constants.NotMatch
				);

			CombineAssertions(() =>
			{
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0}: {1}\t\t{2}: {3}",
					UXMLMatchingDiagnosticModel.Labels.OrgCode,
					"QWE",
					UXMLMatchingDiagnosticModel.Labels.OrgName,
					"QWE INC"
					), model.OrgMatchInfo);
				AssertEquals(model.MatchedAddressDict[UXMLMatchingDiagnosticModel.DataTypes.AddressShortCode], address.OA_Code);
				AssertEquals(model.MatchedAddressDict[UXMLMatchingDiagnosticModel.DataTypes.AdditionalAddress], address.OA_AdditionalAddressInformation);
				AssertEquals(model.MatchedAddressDict[UXMLMatchingDiagnosticModel.DataTypes.Address1], address.OA_Address1);
				AssertEquals(model.MatchedAddressDict[UXMLMatchingDiagnosticModel.DataTypes.Address2], address.OA_Address2);
				AssertEquals(model.MatchedAddressDict[UXMLMatchingDiagnosticModel.DataTypes.Country], address.OA_RN_NKCountryCode);
				AssertEquals(model.MatchedAddressDict[UXMLMatchingDiagnosticModel.DataTypes.City], address.OA_City);
				AssertEquals(model.MatchedAddressDict[UXMLMatchingDiagnosticModel.DataTypes.PostCode], address.OA_PostCode);
				AssertEquals(model.MatchedAddressDict[UXMLMatchingDiagnosticModel.DataTypes.State], address.OA_State);
				AssertEquals(model.MatchedAddressDict[UXMLMatchingDiagnosticModel.DataTypes.Email], address.OA_Email);
			});
		}
	}
}
