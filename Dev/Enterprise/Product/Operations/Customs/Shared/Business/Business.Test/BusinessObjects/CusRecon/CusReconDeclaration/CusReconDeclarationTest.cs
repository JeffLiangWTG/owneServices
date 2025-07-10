using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.CusReconBase;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReconDeclaration))]
	class CusReconDeclarationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCodeAndDescriptionProperty()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CodeProperty", AutoCusReconDeclaration.Schema.CRD_JobReferenceNumber, CodePropertyAttribute.CodePropertyNameFromType(typeof(CusReconDeclaration)));
				AssertEquals("DescriptionProperty", AutoCusReconDeclaration.Schema.CRD_JobReferenceNumber, DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(CusReconDeclaration)));
			});
		}

		public void TestCusReconEntries()
		{
			AssertType<CusReconEntryCollection>(declaration.CusReconEntries);
		}

		public void TestSingleBusinessObjectAroundARow()
		{
			AssertEquals(1, typeof(CusReconDeclaration).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
		}

		public void TestTypeDecider()
		{
			AssertType<CusReconDeclarationTypeDecider>(CusReconDeclaration.TypeDecider);
		}

		public void TestIsAutoLogged()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Logs", 0, declaration.Logs.GetAllLogs().Count);
				Factory.Save();
				AssertEquals("Save Log", 1, declaration.Logs.GetAllLogs().Count);
			});
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("CRD_GB_Branch", GlbBranch.CurrentBranch.PK, declaration.CRD_GB_Branch);
		}

		public void TestBranchCode()
		{
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, declaration.BranchCode);
		}

		public void TestBranchCode_Caption()
		{
			var propertyInfo = declaration.GetType().GetProperty(CusReconDeclaration.Schema.BranchCode);
			AssertEquals("Branch", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestBranchName()
		{
			AssertEquals(GlbBranch.CurrentBranch.GB_BranchName, declaration.BranchName);
		}

		public void TestBranchName_Caption()
		{
			var propertyInfo = declaration.GetType().GetProperty(CusReconDeclaration.Schema.BranchName);
			AssertEquals("Branch Name", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestCRD_ApplicationCode_Caption()
		{
			AssertEquals("Entry Type", DataBoundResourceStrings.GetDataForProperty(declaration.CRD_ApplicationCodeInfo).Caption);
		}

		public void TestCRD_JobReferenceNumber_Caption()
		{
			AssertEquals("Job Number", DataBoundResourceStrings.GetDataForProperty(declaration.CRD_JobReferenceNumberInfo).Caption);
		}

		public void TestCRD_PeriodFrom_Caption()
		{
			AssertEquals("Period From", DataBoundResourceStrings.GetDataForProperty(declaration.CRD_PeriodFromInfo).Caption);
		}

		public void TestCRD_PeriodTo_Caption()
		{
			AssertEquals("Period To", DataBoundResourceStrings.GetDataForProperty(declaration.CRD_PeriodToInfo).Caption);
		}

		public void TestCRD_CPH_ReconClearanceAuthorisation_Caption()
		{
			AssertEquals("Authorization", DataBoundResourceStrings.GetDataForProperty(declaration.CRD_CPH_ReconClearanceAuthorisationInfo).Caption);
		}

		public void TestCRD_CustomsOffice_Caption()
		{
			AssertEquals("Customs Office", DataBoundResourceStrings.GetDataForProperty(declaration.CRD_CustomsOfficeInfo).Caption);
		}

		public void TestCRD_MessageStatus_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.CRD_MessageStatusInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Message Status", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Msg. Status", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Msg. Stat.", resourceStringData.ShortCaption);
			});
		}

		public void TestHumanReadableName()
		{
			CombineAssertions(() =>
			{
				declaration.CRD_JobReferenceNumber = ZString.Empty;
				AssertEquals("CRD_JobReferenceNumber empty", "Monthly Closing Job", declaration.HumanReadableName);

				declaration.CRD_JobReferenceNumber = "MON000014";
				AssertEquals("CRD_JobReferenceNumber = 'MON000014'", "Monthly Closing Job - MON000014", declaration.HumanReadableName);
			});
		}

		public void TestCRD_CustomsStatus_Caption()
		{
			AssertEquals("Customs Status", DataBoundResourceStrings.GetDataForProperty(declaration.CRD_CustomsStatusInfo).Caption);
		}

		public void TestCRD_GB_Branch_Caption()
		{
			AssertEquals("Branch", DataBoundResourceStrings.GetDataForProperty(declaration.CRD_GB_BranchInfo).Caption);
		}

		public void TestCRD_DeclarationType_Captions()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.CRD_DeclarationTypeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Short Caption", "Type", resourceStringData.ShortCaption);
				AssertEquals("Medium Caption", "Dec. Type", resourceStringData.MediumCaption);
				AssertEquals("Caption", "Declaration Type", resourceStringData.Caption);
			});
		}

		public void TestCRD_OA_DeclarantAddress_Caption()
		{
			AssertEquals("Declarant", DataBoundResourceStrings.GetDataForProperty(declaration.CRD_OA_DeclarantAddressInfo).Caption);
		}

		public void TestCRD_OA_RepresentativeAddress_Caption()
		{
			AssertEquals("Representative", DataBoundResourceStrings.GetDataForProperty(declaration.CRD_OA_RepresentativeAddressInfo).Caption);
		}

		public void TestCRD_DeclarantType_List()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(CusReconDeclaration), nameof(CusReconDeclaration.CRD_DeclarantType), true, x => x.ListDataSourceMember == "Lookups.DeclarantTypeList");
		}

		public void TestAuthorizationNumber()
		{
			var permit = Factory.New<CusAuthorisationHeader>();
			permit.CPH_Number = "12345678";

			declaration.CRD_CPH_ReconClearanceAuthorisation = permit.PK;
			AssertEquals("12345678", declaration.AuthorizationNumber);
		}

		public void TestAuthorizationNumber_Caption()
		{
			var propertyInfo = declaration.GetType().GetProperty(CusReconDeclaration.Schema.AuthorizationNumber);
			AssertEquals("Authorization Number", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestOfficeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LV014072", "Office LV014072", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LV003956", "Office LV003956", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (declaration.Branch.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				CombineAssertions(() =>
				{
					declaration.CRD_CustomsOffice = "LV003956";
					AssertEquals("Valid", "Office LV003956", declaration.OfficeDescription);
					declaration.CRD_CustomsOffice = "INVALID";
					AssertEquals("Invalid", ZString.Empty, declaration.OfficeDescription);
				});
			}
		}

		public void TestOfficeDescription_Caption()
		{
			var propertyInfo = declaration.GetType().GetProperty(CusReconDeclaration.Schema.OfficeDescription);
			AssertEquals("Office Description", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestMessageStatusDescription_Caption()
		{
			var propertyInfo = declaration.GetType().GetProperty(CusReconDeclaration.Schema.MessageStatusDescription);
			AssertEquals("Message Status Description", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestMessageStatusDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty status", "Not Sent", declaration.MessageStatusDescription);

				declaration.CRD_MessageStatus = MessageStatusList.Codes.Sent;
				AssertEquals("Valid status", "Sent", declaration.MessageStatusDescription);

				declaration.CRD_MessageStatus = "XYZ";
				AssertEquals("Invalid status", "Unknown", declaration.MessageStatusDescription);
			});
		}

		public void TestCustomsStatusDescription_Caption()
		{
			var propertyInfo = declaration.GetType().GetProperty(CusReconDeclaration.Schema.CustomsStatusDescription);
			AssertEquals("Customs Status Description", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestCustomsStatusDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "AAA", "Status AAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "BBB", "Status BBB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (declaration.Branch.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Empty status", ZString.Empty, declaration.CustomsStatusDescription);

					declaration.CRD_CustomsStatus = "BBB";
					AssertEquals("Valid status", "Status BBB", declaration.CustomsStatusDescription);

					declaration.CRD_CustomsStatus = "XYZ";
					AssertEquals("Invalid status", "Unknown", declaration.CustomsStatusDescription);
				});
			}
		}

		public void TestCountryCode()
		{
			CombineAssertions(() =>
			{
				using (declaration.Branch.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
				{
					AssertEquals("CountryCode from Branch", Core.Constants.CountryCodes.Latvia, declaration.CountryCode);
				}

				declaration.CRD_GB_Branch = ZGuid.Empty;
				AssertEquals("CountryCode from CurrentCompany", Core.Constants.CountryCodes.Eritrea, declaration.CountryCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<CusReconDeclaration>();
		}
		CusReconDeclaration declaration;
	}
}
