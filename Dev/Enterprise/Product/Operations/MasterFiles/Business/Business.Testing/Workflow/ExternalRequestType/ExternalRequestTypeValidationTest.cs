using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal class ExternalRequestTypeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRequiredInDays()
		{
			var requestType = Factory.New<ExternalRequestType>();
			requestType.RunPreSaveValidation();
			AssertNoNotifications(requestType.RQT_RequiredInDaysInfo);

			requestType.RQT_RequiredInDays = -1;
			requestType.RunPreSaveValidation();
			AssertHasError(requestType.RQT_RequiredInDaysInfo, "Please enter a 'Required In Days' greater than or equal to 0.");

			requestType.RQT_RequiredInDays = 1;
			requestType.RunPreSaveValidation();
			AssertNoNotifications(requestType.RQT_RequiredInDaysInfo);
		}

		public void TestReviewer()
		{
			var requestType = Factory.New<ExternalRequestType>();
			requestType.RunPreSaveValidation();
			AssertNoNotifications(requestType.RQT_ReviewerInfo);

			requestType.RQT_Reviewer = DocAddressTypes.Codes.BookingPartyDocumentaryAddress;
			requestType.RunPreSaveValidation();
			AssertHasError(requestType.RQT_ReviewerInfo, "Enter a valid selection.");

			requestType.RQT_JobType = ExternalRequestTypeJobTypes.Codes.SBK;
			requestType.RunPreSaveValidation();
			AssertNoNotifications(requestType.RQT_ReviewerInfo);

			requestType.RQT_JobType = ExternalRequestTypeJobTypes.Codes.ORD;
			requestType.RunPreSaveValidation();
			AssertHasError(requestType.RQT_ReviewerInfo, "Enter a valid selection.");

			requestType.RQT_Reviewer = DocAddressTypes.Codes.BuyerDocumentaryAddress;
			requestType.RunPreSaveValidation();
			AssertNoNotifications(requestType.RQT_ReviewerInfo);
		}

		public void TestAssignee()
		{
			var requestType = Factory.New<ExternalRequestType>();
			requestType.RunPreSaveValidation();
			AssertNoNotifications(requestType.RQT_AssigneeInfo);

			requestType.RQT_Assignee = DocAddressTypes.Codes.BookingPartyDocumentaryAddress;
			requestType.RunPreSaveValidation();
			AssertHasError(requestType.RQT_AssigneeInfo, "Enter a valid selection.");

			requestType.RQT_JobType = ExternalRequestTypeJobTypes.Codes.SBK;
			requestType.RunPreSaveValidation();
			AssertNoNotifications(requestType.RQT_AssigneeInfo);

			requestType.RQT_JobType = ExternalRequestTypeJobTypes.Codes.ORD;
			requestType.RunPreSaveValidation();
			AssertHasError(requestType.RQT_AssigneeInfo, "Enter a valid selection.");

			requestType.RQT_Assignee = DocAddressTypes.Codes.BuyerDocumentaryAddress;
			requestType.RunPreSaveValidation();
			AssertNoNotifications(requestType.RQT_AssigneeInfo);
		}

		public void TestCode()
		{
			var existedOne = Factory.New<ExternalRequestType>();
			existedOne.RQT_Code = "XXX";
			existedOne.RQT_Description = "XXX Desc";
			existedOne.RQT_JobType = ExternalRequestTypeJobTypes.Codes.CLH;
			Factory.Save();

			var externalRequestType = Factory.New<ExternalRequestType>();

			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RQT_CodeInfo, "Please enter a value.");

			externalRequestType.RQT_Code = "XXX";
			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RQT_CodeInfo, "Type Code XXX already exists. Specify a different Type Code.");
		}

		public void TestDescription()
		{
			var externalRequestType = Factory.New<ExternalRequestType>();

			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RQT_DescriptionInfo, "Please enter a value.");
		}

		public void TestJobType()
		{
			var externalRequestType = Factory.New<ExternalRequestType>();

			externalRequestType.RunPreSaveValidation();
			AssertNoNotifications(externalRequestType.RQT_JobTypeInfo);

			externalRequestType.RQT_JobType = "@#$";
			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RQT_JobTypeInfo, "Enter a valid selection.");
		}

		public void Test_FormTypeIsRequired()
		{
			var externalRequestType = Factory.New<ExternalRequestType>();

			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RQT_RIT_TemplateInfo, "Please enter a Form Type.");
		}

		public void Test_FormTypeDataSource_LimitedToJobTypeOrAll()
		{
			SetupExistingRequestInfoTemplateData();

			var externalRequestType = Factory.New<ExternalRequestType>();
			externalRequestType.RQT_JobType = ExternalRequestTypeJobTypes.Codes.CLH;

			var externalRequestLookup = new ExternalRequestTypeLookups(externalRequestType);
			var formTypes = externalRequestLookup.FormTypeList;

			var mainQuery = new ZQuery();

			if (externalRequestType.RQT_JobType != ExternalRequestTypeJobTypes.Codes.ALL)
			{
				var jobTypeQuery = new ZQuery();
				jobTypeQuery.AddToFilter(ExternalRequestInfoTemplateSchema.RIT_JobType, SQLComparisonOperator.Equal, externalRequestType.RQT_JobType);
				jobTypeQuery.AddToFilter(JoinCondition.Or, ExternalRequestInfoTemplateSchema.RIT_JobType, SQLComparisonOperator.Equal, ExternalRequestTypeJobTypes.Codes.ALL);

				mainQuery = new ZQuery(mainQuery, jobTypeQuery);
			}

			var externalRequestInfoTemplateCollection = new ExternalRequestInfoTemplateCollection(Factory, mainQuery);

			AssertContainsExactElementsInExactOrder(formTypes, externalRequestInfoTemplateCollection);
		}

		public void Test_FormTypeJobTypeIsEqualToRequestJobType()
		{
			SetupExistingRequestInfoTemplateData();

			var externalRequestType = Factory.New<ExternalRequestType>();
			externalRequestType.RQT_Code = "XXX";
			externalRequestType.RQT_Description = "XXX Desc";
			externalRequestType.RQT_JobType = ExternalRequestTypeJobTypes.Codes.SPL;

			var query = new ZQuery(ExternalRequestInfoTemplateSchema.RIT_Code, SQLComparisonOperator.Equal, "A01");
			var requestInfotemplate = new ExternalRequestInfoTemplateCollection(Factory, query).First();

			externalRequestType.RQT_RIT_Template = requestInfotemplate.PK;

			externalRequestType.RunPreSaveValidation();
			AssertHasError(externalRequestType.RQT_RIT_TemplateInfo, @"Enter a valid Form Type.");
		}

		public void Test_FormTypeList_IncludesActiveAllInactive()
		{
			SetupExistingRequestInfoTemplateData();

			var externalRequestType = Factory.New<ExternalRequestType>();
			externalRequestType.RQT_JobType = ExternalRequestTypeJobTypes.Codes.ALL;

			var externalRequestLookup = new ExternalRequestTypeLookups(externalRequestType);
			var formTypes = externalRequestLookup.FormTypeList;

			NUnit.Framework.Assert.Multiple(() =>
			{
				var activeTemplates = formTypes.Where(x => x.RIT_IsActive == ZBool.True).ToList();
				AssertEquals(2, activeTemplates.Count);

				var inactiveTemplates = formTypes.Where(x => x.RIT_IsActive == ZBool.False).ToList();
				AssertEquals(2, inactiveTemplates.Count);

				var allTemplates = formTypes.ToList();
				AssertEquals(4, allTemplates.Count);
			});
		}

		public void TestInactiveRequestTemplateCreatesValidationError()
		{
			var inactiveExternalRequestInfoTemplate = Factory.NewWithValidTestData<ExternalRequestInfoTemplate>();
			inactiveExternalRequestInfoTemplate.RIT_IsActive = ZBool.False;

			var externalRequestType = Factory.New<ExternalRequestType>();
			externalRequestType.RQT_RIT_Template = inactiveExternalRequestInfoTemplate.PK;

			AssertHasError(externalRequestType.RQT_RIT_TemplateInfo, "This Form Type is inactive - it may not be used.");

			var activeExternalRequestInfoTemplate = Factory.NewWithValidTestData<ExternalRequestInfoTemplate>();
			activeExternalRequestInfoTemplate.RIT_IsActive = ZBool.True;

			externalRequestType.RQT_RIT_Template = activeExternalRequestInfoTemplate.PK;
			AssertNoErrors(externalRequestType.RQT_RIT_TemplateInfo);
		}

		void SetupExistingRequestInfoTemplateData()
		{
			var externalRequestInfoTemplate = Factory.New<ExternalRequestInfoTemplate>();
			externalRequestInfoTemplate.RIT_Code = "A01";
			externalRequestInfoTemplate.RIT_Description = "A01 Desc";
			externalRequestInfoTemplate.RIT_JobType = ExternalRequestTypeJobTypes.Codes.ORD;
			externalRequestInfoTemplate.RIT_IsActive = ZBool.True;
			Factory.Save();

			externalRequestInfoTemplate = Factory.New<ExternalRequestInfoTemplate>();
			externalRequestInfoTemplate.RIT_Code = "B01";
			externalRequestInfoTemplate.RIT_Description = "B01 Desc";
			externalRequestInfoTemplate.RIT_JobType = ExternalRequestTypeJobTypes.Codes.CLH;
			externalRequestInfoTemplate.RIT_IsActive = ZBool.False;
			Factory.Save();

			externalRequestInfoTemplate = Factory.New<ExternalRequestInfoTemplate>();
			externalRequestInfoTemplate.RIT_Code = "C01";
			externalRequestInfoTemplate.RIT_Description = "C01 Desc";
			externalRequestInfoTemplate.RIT_JobType = ExternalRequestTypeJobTypes.Codes.ALL;
			externalRequestInfoTemplate.RIT_IsActive = ZBool.True;
			Factory.Save();

			externalRequestInfoTemplate = Factory.New<ExternalRequestInfoTemplate>();
			externalRequestInfoTemplate.RIT_Code = "D01";
			externalRequestInfoTemplate.RIT_Description = "This is an inactive request template";
			externalRequestInfoTemplate.RIT_JobType = ExternalRequestTypeJobTypes.Codes.ALL;
			externalRequestInfoTemplate.RIT_IsActive = ZBool.False;
			Factory.Save();
		}
	}
}
