using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public abstract class OtherInfoTest : CodeDataPairTest
	{
		#region TestHumanReadableName
		public void TestHumanReadableName()
		{
			AssertEquals("Other Info", TestOtherInfo.HumanReadableName);
		}
		#endregion

		public void TestLegacyCodesThatRequireData()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;

			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.DeedOfCovenant);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.CustomsOfficerID);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.CertificateOfOrigin);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.CitizenshipOfImporter1);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.CitizenshipOfImporter2);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.CitizenshipOfImporter3);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.CitizenshipOfImporter4);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.CitizenshipOfImporter5);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.DeedOfUndertaking);
			AssertLegacyCodeRequiresData(LineOtherInfoList.Codes.MeatProducersBoardEMPICCodes);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.LetterOfUnderstanding);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.MemorandumOfUnderstanding);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.MinistryOfAgricultureSealNo);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.PassportOfImporter1);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.PassportOfImporter2);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.PassportOfImporter3);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.PassportOfImporter4);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.PassportOfImporter5);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.SecureExportPartnership);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.Passport);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.Certificate);
			AssertLegacyCodeRequiresData(HeaderOtherInfoList.Codes.OtherDocument);
		}

		void AssertLegacyCodeRequiresData(string code)
		{
			TestOtherInfo.ZO_Code = code;
			AssertEquals(code, true, TestOtherInfo.CodeRequiresData);
		}

		public virtual void TestValidateZO_Code()
		{
			InitialiseTestData();

			TestOtherInfo.ZO_Code = "XXX";
			Assert("Invalid code", TestOtherInfo.ZO_CodeInfo.HasMessageErrors());

			TestOtherInfo.ZO_Code = TestOtherInfo.ZO_CodeList[0].Code;
			Assert("Valid code", !TestOtherInfo.ZO_CodeInfo.HasMessageErrors());
		}

		public void TestZO_CodeList()
		{
			InitialiseTestData();

			Assert("TestOtherInfo Code list", TestOtherInfo.ZO_CodeList is ZArchitecture.Core.CodeDescriptionPairList);
			AssertEquals(ExpectedCodes, TestOtherInfo.ZO_CodeList.CodesAsString);
		}

		public void TestOtherInfoItemThatRequiresData()
		{
			InitialiseTestData();

			TestOtherInfo.ZO_Code = CodeThatRequiresData;
			TestOtherInfo.ZO_Data = "";
			AssertEquals("ZO_Data should message error when Data is Required", true, TestOtherInfo.ZO_DataInfo.HasMessageError(CodeThatRequiresData + " " + OtherInfo.CodesAreNotAllowedWithoutAccompanyingData));

			TestOtherInfo.ZO_Data = "SOMETHING";
			AssertEquals("ZO_Data should not message error once data is filled in", false, TestOtherInfo.ZO_DataInfo.HasMessageErrors());
		}

		public void TestOtherInfoItemThatRequiresNoData()
		{
			InitialiseTestData();

			TestOtherInfo.ZO_Code = CodeThatRequiresNoData;
			TestOtherInfo.ZO_Data = "";
			AssertEquals("ZO_Data should not message error with no data", false, TestOtherInfo.ZO_DataInfo.HasMessageErrors());

			TestOtherInfo.ZO_Data = "SOMETHING";
			AssertEquals("ZO_Data should message error when data is filled in", true, TestOtherInfo.ZO_DataInfo.HasMessageError(CodeThatRequiresNoData + " " + OtherInfo.CodesDoNotRequireAccompanyingData));
		}

		public void TestOtherInfoItemThatDoesntCareIfItHasDataOrNot()
		{
			InitialiseTestData();

			if (CodeThatDoesntCareIfItHasDataOrNot.IsEmpty)
			{
				Assert(true);
			}
			else
			{
				TestOtherInfo.ZO_Code = CodeThatDoesntCareIfItHasDataOrNot;
				TestOtherInfo.ZO_Data = "";
				AssertEquals("ZO_Data should not message error with no data", false, TestOtherInfo.ZO_DataInfo.HasMessageErrors());

				TestOtherInfo.ZO_Data = "SOMETHING";
				AssertEquals("ZO_Data should not message error when data is filled in", false, TestOtherInfo.ZO_DataInfo.HasMessageErrors());
			}
		}

		protected abstract ZString ExpectedCodes { get; }
		protected abstract ZString CodeThatRequiresData { get; }
		protected abstract ZString CodeThatRequiresNoData { get; }
		protected abstract ZString CodeThatDoesntCareIfItHasDataOrNot { get; }

		protected OtherInfo TestOtherInfo => fTestOtherInfo ??= (OtherInfo)GetNewBusinessObject();
		protected OtherInfo fTestOtherInfo;

		protected abstract void InitialiseTestData();
	}
}
