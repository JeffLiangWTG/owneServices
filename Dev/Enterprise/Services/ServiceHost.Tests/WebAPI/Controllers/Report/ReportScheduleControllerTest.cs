using System;
using System.Collections.Generic;
using System.Net;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.Report
{
	class ReportScheduleControllerTest : BaseReportControllerTest<ReportScheduleController>
	{
		public void TestScheduleReport()
		{
			mockService.Setup(x => x.ScheduleReport(It.IsAny<ReportScheduleData>())).Verifiable();
			var actionResult = controller.ScheduleReport(new ReportScheduleData());
			actionResult.AssertResultContains(HttpStatusCode.OK);
		}

		public void TestGetBranch()
		{
			var results = new List<CodeDescription> {
				new()
				{
					Pk = Env.CurrentBranch.PK,
					Code = Env.CurrentBranch.Code,
					Description = Env.CurrentBranch.Name
				},
			};
			var expectedResult = new
			{
				count = results.Count,
				results
			};
			var actionResult = controller.GetBranch(Env.CurrentBranch.PK);
			AssertJsonResult(expectedResult, actionResult);

			var glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			mockService.Setup(x => x.GetBranch(It.IsAny<Guid>())).Returns(glbBranch);

			results = [
				new()
				{
					Pk = glbBranch.PK.ToGuid(),
					Code = glbBranch.GB_Code,
					Description = glbBranch.GB_BranchName
				},
			];

			expectedResult = new
			{
				count = results.Count,
				results
			};
			actionResult = controller.GetBranch(glbBranch.PK.ToGuid());
			AssertJsonResult(expectedResult, actionResult);

			mockService.Setup(x => x.GetBranch(It.IsAny<Guid>())).Returns(default(IGlbBranch));
			actionResult = controller.GetBranch(Guid.Empty);
			AssertJsonResult(new { count = 0, results = new List<CodeDescription>() }, actionResult);
		}

		public void TestGetPrintUsers()
		{
			var glbStaff = new BusinessObjectFactory().New<IGlbStaff>();
			glbStaff.GS_Code = "TST";
			glbStaff.GS_FullName = "Test";
			glbStaff.GS_EmailAddress = "Email";

			var results = new List<object>
			{
				new
				{
					Pk = glbStaff.PK,
					Code = "TST",
					Description = "Test",
					AdditionalInfo = new { Email = "Email" },
				}
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetPrintUsers(It.IsAny<ReportLookupSearchArgs>(), It.IsAny<Action<ZQuery, Type, string>>())).Returns(new List<IGlbStaff> { glbStaff }).Verifiable();
			var actionResult = controller.GetPrintUsers(new ReportLookupSearchArgs());
			AssertJsonResult(expectedResult, actionResult);
		}

		[TestDate(2023, 12, 29)]
		public void TestGetUtcOffset()
		{
			var expectedResult = new { UtcOffset = 8d };
			mockService.Setup(x => x.GetUtcOffset()).Returns(8).Verifiable();

			var actionResult = controller.GetUtcOffset();
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestCalculateStartDate()
		{
			var expectedResult = new CalcStartDateOfAccountingPeriodData() { DayOfAccountingPeriod = 1, StartDateLocal = DateTime.Today };
			mockService.Setup(x => x.CalcStartDateOfAccountingPeriod(It.IsAny<CalcStartDateOfAccountingPeriodData>())).Verifiable();

			var actionResult = controller.CalculateStartDate(expectedResult);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetDeliverRecipientTypes()
		{
			var results = new List<CodeDescription> {
				new CodeDescription
				{
					Pk = Guid.Empty,
					Code = "STF",
					Description = "Staff"
				},
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetDeliverRecipientTypes()).Returns(results).Verifiable();
			var actionResult = controller.GetDeliverRecipientTypes();
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetScheduleDeliveryMethods()
		{
			var results = new List<CodeDescription> {
				new CodeDescription
				{
					Pk = Guid.Empty,
					Code = "EML",
					Description = "E-Mail"
				},
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetScheduleDeliveryMethods()).Returns(results).Verifiable();
			var actionResult = controller.GetScheduleDeliveryMethods();
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetScheduleAttachmentTypes()
		{
			var results = new List<CodeDescription> {
					new CodeDescription
					{
						Pk = Guid.Empty,
						Code = "XLS",
						Description = "Microsoft Excel 97-2003 Spreadsheet"
					},
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetScheduleAttachmentTypes(It.IsAny<Guid>())).Returns(results).Verifiable();
			var actionResult = controller.GetScheduleAttachmentTypes(Guid.Empty);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetScheduleRecipientPrinters()
		{
			var results = new List<CodeDescription> {
				new CodeDescription
				{
					Pk = new Guid("e0daa6d3-92ef-476b-bc07-d8ec9473d27b"),
					Code = "printer display name",
					Description = "printer server name"
				},
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetScheduleRecipientPrinters(It.IsAny<Guid>())).Returns(results).Verifiable();
			var actionResult = controller.GetScheduleRecipientPrinters(Guid.Empty);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetBlankReportActivities()
		{
			var results = new List<CodeDescription> {
				new CodeDescription
				{
					Pk = Guid.Empty,
					Code = "TST",
					Description = "Test Description"
				},
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetBlankReportActivities()).Returns(results).Verifiable();
			var actionResult = controller.GetBlankReportActivities();
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetEmailFromAddressList()
		{
			var results = new List<CodeDescription> {
				new CodeDescription
				{
					Pk = Guid.Empty,
					Code  = "main@test.com",
					Description = "Main - main@test.com",
				},
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetEmailFromAddressList(It.IsAny<Guid>())).Returns(results).Verifiable();
			var actionResult = controller.GetEmailFromAddressList(Guid.Empty);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetStaffRecipients()
		{
			var glbStaff = new BusinessObjectFactory().New<IGlbStaff>();
			glbStaff.GS_Code = "TST";
			glbStaff.GS_FullName = "Test";
			glbStaff.GS_EmailAddress = "Email";

			var results = new List<CodeDescription>
			{
				new CodeDescription
				{
					Pk = glbStaff.PK.ToGuid(),
					Code = "TST",
					Description = "Test",
				}
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetStaffRecipients(It.IsAny<ReportLookupSearchArgs>(), It.IsAny<Action<ZQuery, Type, string>>())).Returns(new List<IGlbStaff> { glbStaff }).Verifiable();
			var actionResult = controller.GetStaffRecipients(new ReportLookupSearchArgs());
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetGroups()
		{
			var glbGroup = new BusinessObjectFactory().New<IGlbGroup>();
			glbGroup.GG_Code = "TST";
			glbGroup.GG_Desc = "Test";

			var results = new List<CodeDescription>
			{
				new CodeDescription
				{
					Pk = glbGroup.PK.ToGuid(),
					Code = "TST",
					Description = "Test",
				}
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetGroups(It.IsAny<ReportLookupSearchArgs>(), It.IsAny<Action<ZQuery, Type, string>>())).Returns(new List<IGlbGroup> { glbGroup }).Verifiable();
			var actionResult = controller.GetGroups(new ReportLookupSearchArgs());
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetContactNames()
		{
			var results = new List<CodeDescription> {
				new CodeDescription
				{
					Pk = Guid.Empty,
					Code = "Test Contact Code",
					Description = "Test Contact Desc"
				},
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetScheduleContactNames(It.IsAny<Guid>())).Returns(results).Verifiable();
			var actionResult = controller.GetContactNames(Guid.Empty);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetCopyRecipientsEmails()
		{
			var results = new List<CodeDescription> {
				new CodeDescription
				{
					Pk = Guid.Empty,
					Code = "contact@test.com",
					Description = "Test Contact Name"
				},
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			mockService.Setup(x => x.GetCopyRecipientsEmails(It.IsAny<string>(), It.IsAny<Guid>())).Returns(results).Verifiable();
			var actionResult = controller.GetCopyRecipientsEmails(string.Empty, Guid.Empty);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetDeliveryAddress()
		{
			var deliveryAddress = "test delivery address";
			var expectedResult = new
			{
				deliveryAddress,
			};

			mockService.Setup(x => x.GetDeliveryAddress(It.IsAny<GetDeliveryAddressArgs>())).Returns(deliveryAddress).Verifiable();
			var actionResult = controller.GetDeliveryAddress(new GetDeliveryAddressArgs());
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetAllowPrintUser()
		{
			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertJsonResult(false, controller.GetAllowPrintUser());
			}

			var testSecurity = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			using (Env.SetTemporarySecurityInstanceForTest(testSecurity))
			{
				testSecurity.ScheduleOtherStaffAsPrintUser.IsAllowed = true;
				AssertJsonResult(true, controller.GetAllowPrintUser());

				testSecurity.ScheduleOtherStaffAsPrintUser.IsAllowed = false;
				AssertJsonResult(false, controller.GetAllowPrintUser());
			}
		}

		public void TestGetReportScheduleData()
		{
			var expectedResult = new ReportScheduleData();
			expectedResult.ScheduleTask = new ReportScheduleTaskData { DateScheduleFirstRun = DateTime.Today };

			mockService.Setup(x => x.GetReportScheduleData(It.IsAny<Guid>())).Returns(expectedResult).Verifiable();
			var actionResult = controller.GetReportScheduleData(new Guid("418d2743-a67c-4575-9b94-1d3342da7cca"));
			AssertJsonResult(expectedResult, actionResult, jsonSerializerSettings: new Newtonsoft.Json.JsonSerializerSettings { DateFormatString = "yyyy-MM-ddTHH:mm:ss" });
		}

		public void TestDeleteReportScheduleTask()
		{
			mockService.Setup(x => x.DeleteReportScheduleTask(It.IsAny<Guid>())).Verifiable();
			var actionResult = controller.DeleteReportScheduleTask(new Guid("418d2743-a67c-4575-9b94-1d3342da7cca"));
			actionResult.AssertResultContains(HttpStatusCode.OK);
		}

		public void TestCheckPermissionForScheduleReport()
		{
			mockService.Setup(x => x.CheckPermissionForScheduleReport(It.IsAny<DataOperations>(), It.IsAny<Guid>())).Returns(true).Verifiable();
			var actionResult = controller.CheckPermissionForScheduleReport("invalid value", Guid.Empty);
			AssertJsonResult(false, actionResult);

			CombineAssertions(() =>
			{
				foreach (var dataOperation in Enum.GetNames(typeof(DataOperations)))
				{
					actionResult = controller.CheckPermissionForScheduleReport(dataOperation, Guid.Empty);
					AssertJsonResult(true, actionResult, $"DataOperation: {dataOperation}");
				}
			});
		}

		protected override string[] StaffOnlyAuthorizationFilterMethods => new string[]
		{
			nameof(ReportScheduleController.GetBranch),
			nameof(ReportScheduleController.GetEmailFromAddressList),
			nameof(ReportScheduleController.GetGroups),
			nameof(ReportScheduleController.GetPrintUsers),
			nameof(ReportScheduleController.GetScheduleRecipientPrinters),
			nameof(ReportScheduleController.GetStaffRecipients),
		};

		public void TestGetOrganizations()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ETGTESTORG";
			orgHeader.OH_FullName = "TESTDescription";

			Factory.Save();

			var results = new List<CodeDescription> {
				new CodeDescription
				{
					Pk = orgHeader.PK.ToGuid(),
					Code = "ETGTESTORG",
					Description = "TESTDescription"
				},
			};

			var expectedResult = new
			{
				count = results.Count,
				results
			};

			var actionResult = controller.GetOrganizations(new ReportLookupSearchArgs { SearchTerms = orgHeader.PK.ToString() });
			AssertJsonResult("It can be searched by pk.", expectedResult, actionResult);

			actionResult = controller.GetOrganizations(new ReportLookupSearchArgs { SearchTerms = "ETGTESTORG" });
			AssertJsonResult("It can be searched by code.", expectedResult, actionResult);

			actionResult = controller.GetOrganizations(new ReportLookupSearchArgs { SearchTerms = "TESTDescription" });
			AssertJsonResult("It can be searched by description.", expectedResult, actionResult);

			orgHeader.OH_IsActive = false;
			Factory.Save();

			expectedResult = new
			{
				count = 0,
				results = new List<CodeDescription>(),
			};

			actionResult = controller.GetOrganizations(new ReportLookupSearchArgs { SearchTerms = "TESTDescription" });
			AssertJsonResult("It will apply default filters.", expectedResult, actionResult);
		}

		public void TestGetRelatedOrganizations()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAATESTORG";
			orgHeader.OH_FullName = "TESTDescription";

			var relatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			relatedOrg.OH_Code = "RELATEDORG";
			relatedOrg.OH_FullName = "RelatedOrganization";

			Factory.Save();

			var pkResults = new List<ZGuid> { orgHeader.PK, relatedOrg.PK };
			var orgResults = new List<OrgHeader> { orgHeader, relatedOrg };
			var expectedResult = new
			{
				count = 2,
				results = new List<CodeDescription>
				{
					new CodeDescription { Pk = orgHeader.PK.ToGuid(), Code = "AAATESTORG", Description = "TESTDescription" },
					new CodeDescription { Pk = relatedOrg.PK.ToGuid(), Code = "RELATEDORG", Description = "RelatedOrganization" }
				}
			};
			var contactPK = Guid.NewGuid();
			mockService.Setup(x => x.GetRelatedOrganizations(contactPK, null, LookupFilterDataHelper.BuildCodeDescriptionQuery)).Returns(orgResults).Verifiable();
			mockService.Setup(x => x.GetRelatedOrganizationIDs(contactPK)).Returns(pkResults).Verifiable();
			mockIdentity.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);
			mockIdentity.SetupGet(x => x.ProviderKey).Returns(contactPK);

			var actionResult = controller.GetOrganizations(null);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetContactNames_ShouldReturnEmptyIfOrganizationNotAuthorized()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAATESTORG";
			orgHeader.OH_FullName = "TESTDescription";
			var relatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			relatedOrg.OH_Code = "RELATEDORG";
			relatedOrg.OH_FullName = "RelatedOrganization";
			var unRelatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			unRelatedOrg.OH_Code = "UNRELATEDORG";
			unRelatedOrg.OH_FullName = "UnrelatedOrganization";

			var mockCurrentContactPK = Guid.NewGuid();
			var mockedGetRelatedOrgResult = new List<ZGuid> { orgHeader.PK, relatedOrg.PK };
			mockService.Setup(x => x.GetRelatedOrganizationIDs(mockCurrentContactPK)).Returns(mockedGetRelatedOrgResult).Verifiable();
			mockIdentity.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);
			mockIdentity.SetupGet(x => x.ProviderKey).Returns(mockCurrentContactPK);

			var mockedResult = new List<CodeDescription> { new CodeDescription { Code = "Contact AAA", Description = "Tile AAA" } };

			mockService.Setup(x => x.GetScheduleContactNames(orgHeader.PK.ToGuid())).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetScheduleContactNames(relatedOrg.PK.ToGuid())).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetScheduleContactNames(unRelatedOrg.PK.ToGuid())).Returns(mockedResult).Verifiable();

			var expectedResult = new
			{
				count = 1,
				results = mockedResult
			};

			AssertJsonResult(expectedResult, controller.GetContactNames(orgHeader.PK.ToGuid()));
			AssertJsonResult(expectedResult, controller.GetContactNames(relatedOrg.PK.ToGuid()));

			expectedResult = new
			{
				count = 0,
				results = (List<CodeDescription>)null,
			};
			AssertJsonResult(expectedResult, controller.GetContactNames(unRelatedOrg.PK.ToGuid()));
		}

		public void TestGetContactEmails_ShouldReturnEmptyIfOrganizationNotAuthorized()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAATESTORG";
			orgHeader.OH_FullName = "TESTDescription";
			var relatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			relatedOrg.OH_Code = "RELATEDORG";
			relatedOrg.OH_FullName = "RelatedOrganization";
			var unRelatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			unRelatedOrg.OH_Code = "UNRELATEDORG";
			unRelatedOrg.OH_FullName = "UnrelatedOrganization";

			var mockCurrentContactPK = Guid.NewGuid();
			var mockedGetRelatedOrgResult = new List<ZGuid> { orgHeader.PK, relatedOrg.PK };
			mockService.Setup(x => x.GetRelatedOrganizationIDs(mockCurrentContactPK)).Returns(mockedGetRelatedOrgResult).Verifiable();
			mockIdentity.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);
			mockIdentity.SetupGet(x => x.ProviderKey).Returns(mockCurrentContactPK);

			var mockedResult = new List<CodeDescription> { new CodeDescription { Code = "aaa@test.com", Description = "Contact AAA" } };

			mockService.Setup(x => x.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient, orgHeader.PK.ToGuid())).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient, relatedOrg.PK.ToGuid())).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient, unRelatedOrg.PK.ToGuid())).Returns(mockedResult).Verifiable();

			var expectedResult = new
			{
				count = 1,
				results = mockedResult
			};

			AssertJsonResult(expectedResult, controller.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient, orgHeader.PK.ToGuid()));
			AssertJsonResult(expectedResult, controller.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient, relatedOrg.PK.ToGuid()));

			expectedResult = new
			{
				count = 0,
				results = (List<CodeDescription>)null,
			};
			AssertJsonResult(expectedResult, controller.GetCopyRecipientsEmails(Core.Constants.CopyRecipientType.EmailToRecipient, unRelatedOrg.PK.ToGuid()));
		}

		public void TestGetContactEmail_ShouldReturnEmptyIfOrganizationNotAuthorized()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAATESTORG";
			orgHeader.OH_FullName = "TESTDescription";
			var relatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			relatedOrg.OH_Code = "RELATEDORG";
			relatedOrg.OH_FullName = "RelatedOrganization";
			var unRelatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			unRelatedOrg.OH_Code = "UNRELATEDORG";
			unRelatedOrg.OH_FullName = "UnrelatedOrganization";

			var mockCurrentContactPK = Guid.NewGuid();
			var mockedGetRelatedOrgResult = new List<ZGuid> { orgHeader.PK, relatedOrg.PK };
			mockService.Setup(x => x.GetRelatedOrganizationIDs(mockCurrentContactPK)).Returns(mockedGetRelatedOrgResult).Verifiable();
			mockIdentity.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);
			mockIdentity.SetupGet(x => x.ProviderKey).Returns(mockCurrentContactPK);

			var mockedResult = "aaa@test.com";

			var arg1 = new GetDeliveryAddressArgs() { OrganizationId = orgHeader.PK.ToGuid() };
			var arg2 = new GetDeliveryAddressArgs() { OrganizationId = relatedOrg.PK.ToGuid() };
			var arg3 = new GetDeliveryAddressArgs() { OrganizationId = unRelatedOrg.PK.ToGuid() };
			mockService.Setup(x => x.GetDeliveryAddress(arg1)).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetDeliveryAddress(arg2)).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetDeliveryAddress(arg3)).Returns(mockedResult).Verifiable();

			var expectedResult = new
			{
				deliveryAddress = mockedResult
			};

			AssertJsonResult(expectedResult, controller.GetDeliveryAddress(arg1));
			AssertJsonResult(expectedResult, controller.GetDeliveryAddress(arg2));

			expectedResult = new
			{
				deliveryAddress = (string)null,
			};
			AssertJsonResult(expectedResult, controller.GetDeliveryAddress(arg3));
		}
	}
}
