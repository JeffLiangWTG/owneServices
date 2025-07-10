using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrganisationViewStmNumsValidationTest : ViewStmNumsValidationTest
	{
		public void TestEnsureOrgTypeMatchesFountainType()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var stmNum = Factory.NewWithValidTestData<OrganisationViewStmNums>();
			stmNum.SN_Owner = header.PK;

			header.OH_IsWarehouseClient = false;
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
			AssertHasErrorContaining(stmNum.SN_TypeInfo, "Warehouse");
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			AssertNoErrorContaining(stmNum.SN_TypeInfo, "Warehouse");

			header.OH_IsWarehouseClient = true;
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
			AssertNoErrorContaining(stmNum.SN_TypeInfo, "Warehouse");
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			AssertNoErrorContaining(stmNum.SN_TypeInfo, "Warehouse");
		}

		public void TestSN_ValueForDisplay()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var stmNum = Factory.NewWithValidTestData<OrganisationViewStmNums>();

				AssertEquals(stmNum.SN_Value, stmNum.SN_ValueForDisplay);

				stmNum.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
				stmNum.SN_MinimumValue = 100;
				stmNum.SN_MaximumValue = 102;
				stmNum.SN_Count = 3;
				stmNum.SN_Owner = header.PK;

				stmNum.Validation.ValidateSN_ValueForDisplay();
				AssertNoWarnings("Should not validate this property when the ViewStmNum is not in database", stmNum.SN_ValueForDisplayInfo);

				Factory.Save();

				var foutain = stmNum.TryGetNumberFountain();
				foutain.GetNextFormatted(Factory);
				foutain.GetNextFormatted(Factory);

				Factory.Save();

				var query = new ZQuery(ViewStmNumsSchema.SN_Owner, header.PK);
				var newStmNum = NewFactory().LoadTop1<OrganisationViewStmNums>(query);

				newStmNum.Validation.ValidateSN_ValueForDisplay();
				AssertEquals(102L, (long)newStmNum.SN_ValueForDisplay);
				AssertNoWarnings("Should not validate this property when the SN_ValueForDisplayInfo is not -1", newStmNum.SN_ValueForDisplayInfo);

				foutain.GetNextFormatted(Factory);
				Factory.Save();

				newStmNum = NewFactory().LoadTop1<OrganisationViewStmNums>(query);

				newStmNum.Validation.ValidateSN_ValueForDisplay();
				AssertEquals(-1L, (long)newStmNum.SN_ValueForDisplay);
				AssertHasWarning("Should validate this property when ViewStmNum has no changes and the SN_ValueForDisplayInfo is -1", newStmNum.SN_ValueForDisplayInfo, "All numbers used");

				newStmNum.HasChanges = true;
				newStmNum.Validation.ValidateSN_ValueForDisplay();
				AssertNoWarnings("Should not validate this property when ViewStmNum has any changes", newStmNum.SN_ValueForDisplayInfo);
			}
		}

		public void TestSN_Count()
		{
			var stmNums = Factory.New<OrganisationViewStmNums>();
			stmNums.SN_MinimumValue = 1;
			stmNums.SN_Count = 1000;
			AssertNoErrors(stmNums.SN_CountInfo);

			stmNums.SN_MinimumValue = OrganisationViewStmNums.Schema.DefaultFountainMaximumValue - 100;
			stmNums.SN_Count = 100;
			AssertNoErrors(stmNums.SN_CountInfo);

			stmNums.SN_Count = 102;
			AssertHasErrors(stmNums.SN_CountInfo);

			stmNums.SN_Count = 10;
			AssertNoErrors(stmNums.SN_CountInfo);

			stmNums.SN_Count = -1;
			AssertHasErrors(stmNums.SN_CountInfo);
		}

		public void TestSN_Type()
		{
			var stmNums = Factory.New<OrganisationViewStmNums>();

			stmNums.SN_Type = "XXX";
			AssertHasErrors(stmNums.SN_TypeInfo);

			stmNums.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			AssertNoErrors(stmNums.SN_TypeInfo);

			stmNums.SN_Type = "";
			AssertHasErrors(stmNums.SN_TypeInfo);
		}

		public void TestSSCCBarCodePrefixMustBeUniqueOnOrganization()
		{
			var header = Factory.New<OrgHeader>();

			var cusCode1 = header.CustomsCodes.AddNew();
			cusCode1.OK_RN_NKCodeCountry = "AU";
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			cusCode1.OK_CustomsRegNo = "1234567";

			var cusCode2 = header.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "NZ";
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			cusCode2.OK_CustomsRegNo = "7654321";

			var existingStmNums = Factory.New<OrganisationViewStmNums>();
			existingStmNums.SN_Owner = header.PK;
			existingStmNums.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			existingStmNums.SN_Prefix = "1234567";

			var newStmNums = Factory.New<OrganisationViewStmNums>();
			newStmNums.SN_Owner = header.PK;
			newStmNums.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			newStmNums.SN_Prefix = "7654321";
			AssertNoErrors(newStmNums.SN_PrefixInfo);

			newStmNums.SN_Prefix = "1234567";
			AssertHasError(newStmNums.SN_PrefixInfo, "The Range Type and Prefix has been duplicated and must be unique.");
		}

		public void TestRangeTypeAndPrefixMustBeUnique()
		{
			foreach (var info in typeof(OrgConstants.NumberFountains.Code).GetFields().Where(x => x.IsLiteral))
			{
				RangeTypeAndPrefixMustBeUniqueCore((string)info.GetValue(null));
			}
		}

		void RangeTypeAndPrefixMustBeUniqueCore(string type)
		{
			var org = Factory.New<OrgHeader>();
			if (type == OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers)
			{
				var cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
				cusCode.OK_CustomsRegNo = "1234567";
			}

			var existingStmNums = Factory.New<OrganisationViewStmNums>();
			existingStmNums.SN_Owner = org.PK;
			existingStmNums.SN_Type = type;
			existingStmNums.SN_Prefix = "1234321";

			var newStmNums = Factory.New<OrganisationViewStmNums>();
			newStmNums.SN_Owner = org.PK;
			newStmNums.SN_Type = type;
			newStmNums.SN_Prefix = "1234567";
			AssertNoErrors(newStmNums.SN_PrefixInfo);

			newStmNums.SN_Prefix = "1234321";
			AssertHasError(newStmNums.SN_PrefixInfo, "The Range Type and Prefix has been duplicated and must be unique.");
		}

		public void TestSN_Prefix()
		{
			var stmNums = Factory.New<OrganisationViewStmNums>();

			stmNums.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			stmNums.SN_Prefix = "XXX";
			AssertHasErrorContaining(stmNums.SN_PrefixInfo, "SSCC Prefix must be");

			stmNums.SN_Prefix = "01234567";
			Assert("prerequisite", Enterprise.NumberFountain.SSCCBarCodeChecker.IsSSCCBarCodePrefix(stmNums.SN_Prefix));
			AssertNoErrors(stmNums.SN_PrefixInfo);

			stmNums.SN_Prefix = "012345";
			AssertHasErrorContaining(stmNums.SN_PrefixInfo, "SSCC Prefix must be");

			stmNums.SN_Type = "AAA";
			AssertNoErrors(stmNums.SN_PrefixInfo);

			stmNums.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			AssertHasErrorContaining(stmNums.SN_PrefixInfo, "SSCC Prefix must be");
		}

		public void TestSSCCPrefixMustMatchGS1Code()
		{
			var org = Factory.New<OrgHeader>();

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			cusCode.OK_CustomsRegNo = "1234567";

			var viewStmNum = org.OrgFountains.AddNew();
			viewStmNum.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			viewStmNum.SN_Prefix = "9999999";
			AssertHasError(viewStmNum.SN_PrefixInfo, "SSCC Prefix must match at least one GS1 code in Config->Registration Numbers / Codes.");

			viewStmNum.SN_Prefix = "1234567";
			AssertNoErrors(viewStmNum.SN_PrefixInfo);
		}

		public void TestSN_ZoneIDPrefix()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var stmNums = Factory.New<OrganisationViewStmNums>();
			stmNums.SN_Owner = org.PK;
			stmNums.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNums.SN_ZoneIDPrefix = "AAAAAAA";
			AssertHasError(stmNums.SN_ZoneIDPrefixInfo, "Zone ID. Prefix must be 3 digits + 2 alpha numeric + 2 digits.");

			stmNums.SN_ZoneIDPrefix = "AAAAAAAAA";
			AssertHasError(stmNums.SN_ZoneIDPrefixInfo, "Zone ID. Prefix must be 3 digits + 3 alpha numeric + 3 alpha numeric or  3 digits + 2 alpha numeric + 2 digits.");

			stmNums.SN_ZoneIDPrefix = "111AAAAAA";
			AssertNoErrors(stmNums.SN_ZoneIDPrefixInfo);

			stmNums.SN_ZoneIDPrefix = "";
			AssertHasError(stmNums.SN_ZoneIDPrefixInfo, "Zone ID. Prefix must be 3 digits + 3 alpha numeric + 3 alpha numeric or  3 digits + 2 alpha numeric + 2 digits.");

			stmNums.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			AssertNoErrors(stmNums.SN_ZoneIDPrefixInfo);

			stmNums.SN_ZoneIDPrefix = "123456789";
			AssertNoErrors(stmNums.SN_ZoneIDPrefixInfo);

			stmNums.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNums.SN_ZoneIDPrefix = "1234567";
			AssertNoErrors(stmNums.SN_ZoneIDPrefixInfo);
			Factory.Save();

			var stmNums2 = Factory.New<OrganisationViewStmNums>();
			stmNums2.SN_Owner = org.PK;
			stmNums2.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNums2.SN_ZoneIDPrefix = "1234567";
			AssertHasError(stmNums2.SN_ZoneIDPrefixInfo, "The Zone ID. Prefix has been duplicated and must be unique.");

			stmNums2.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			AssertNoErrors(stmNums2.SN_ZoneIDPrefixInfo);
		}

		public void TestSN_ClientPrefix()
		{
			var stmNums = Factory.New<OrganisationViewStmNums>();
			stmNums.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNums.SN_ClientPrefix = "1";
			AssertHasError(stmNums.SN_ClientPrefixInfo, "Client Prefix must be 3 alpha numeric.");

			stmNums.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			AssertNoErrors(stmNums.SN_ClientPrefixInfo);

			stmNums.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNums.SN_ClientPrefix = "111";
			AssertNoErrors(stmNums.SN_ClientPrefixInfo);
		}

		protected override ViewStmNums GetNewViewStmNums()
		{
			return Factory.New<OrganisationViewStmNums>();
		}

		protected override BusinessObject GetNewOwner()
		{
			return Factory.NewWithValidTestData<OrgHeader>();
		}
	}
}
