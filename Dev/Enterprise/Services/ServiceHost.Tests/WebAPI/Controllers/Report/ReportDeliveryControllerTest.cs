using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.Services.ServiceHost.Tests
{
	class ReportDeliveryControllerTest : BaseReportControllerTest<ReportDeliveryController>
	{
		public void TestGetOnlinePrinters()
		{
			var expectedResult = new List<CodeDescription> { new CodeDescription { Pk = Guid.NewGuid(), Code = "Printer", Description = "Server" } };
			mockService.Setup(x => x.GetOnlinePrinters()).Returns(expectedResult).Verifiable();
			var actionResult = controller.GetOnlinePrinters();
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetDeliveryMethods()
		{
			var expectedResult = new List<CodeDescription> { new CodeDescription { Code = "Print", Description = "Print" } };
			mockService.Setup(x => x.GetDeliveryMethods()).Returns(expectedResult).Verifiable();
			var actionResult = controller.GetDeliveryMethods();
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetAttachmentTypes()
		{
			var results = new List<CodeDescription> { new CodeDescription { Code = "XLSX", Description = "XLSX" } };
			var expectedResult = new
			{
				count = results.Count,
				results
			};
			mockService.Setup(x => x.GetAttachmentTypes(Guid.Empty)).Returns(results).Verifiable();
			var actionResult = controller.GetAttachmentTypes(Guid.Empty);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetSalutations()
		{
			var expectedResult = new List<CodeDescription> { new CodeDescription { Code = "Dear Sir", Description = "Dear Sir" } };
			mockService.Setup(x => x.GetSalutations()).Returns(expectedResult).Verifiable();
			var actionResult = controller.GetSalutations();
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestDeliverReport()
		{
			var deliveryData = new DeliveryData { PrintQueuePk = Guid.NewGuid() };
			var jobs = new List<string>();
			mockService.Setup(x => x.DeliverReport(deliveryData)).Callback(() =>
			{
				jobs.Add("Job1");
				jobs.Add("Job2");
			}).Verifiable();
			controller.DeliverReport(deliveryData);

			AssertEquals(2, jobs.Count);
			AssertCollectionContains("Job1", jobs);
			AssertCollectionContains("Job2", jobs);
		}

		public void TestGetContactNames()
		{
			var results = new List<CodeDescription> { new CodeDescription { Code = "Contact AAA", Description = "Tile AAA" } };
			var expectedResult = new
			{
				count = results.Count,
				results
			};
			mockService.Setup(x => x.GetDeliveryContactNames(Guid.Empty)).Returns(results).Verifiable();
			var actionResult = controller.GetContactNames(Guid.Empty);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetContactEmails()
		{
			var results = new List<CodeDescription> { new CodeDescription { Code = "aaa@test.com", Description = "Contact AAA" } };
			var expectedResult = new
			{
				count = results.Count,
				results
			};
			mockService.Setup(x => x.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, Guid.Empty)).Returns(results).Verifiable();
			var actionResult = controller.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, Guid.Empty);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetContactEmail()
		{
			var contactEmail = "aaa@test.com";
			var expectedResult = new
			{
				contactEmail
			};
			mockService.Setup(x => x.GetContactEmail("", Guid.Empty)).Returns(contactEmail).Verifiable();
			var actionResult = controller.GetContactEmail("", Guid.Empty);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetOrganizations()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAATESTORG";
			orgHeader.OH_FullName = "TESTDescription";

			Factory.Save();

			var results = new List<CodeDescription> {
				new CodeDescription
				{
					Pk = orgHeader.PK.ToGuid(),
					Code = "AAATESTORG",
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

			actionResult = controller.GetOrganizations(new ReportLookupSearchArgs { SearchTerms = "AAATESTORG" });
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

			mockService.Setup(x => x.GetDeliveryContactNames(orgHeader.PK.ToGuid())).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetDeliveryContactNames(relatedOrg.PK.ToGuid())).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetDeliveryContactNames(unRelatedOrg.PK.ToGuid())).Returns(mockedResult).Verifiable();

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

			mockService.Setup(x => x.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, orgHeader.PK.ToGuid())).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, relatedOrg.PK.ToGuid())).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, unRelatedOrg.PK.ToGuid())).Returns(mockedResult).Verifiable();

			var expectedResult = new
			{
				count = 1,
				results = mockedResult
			};

			AssertJsonResult(expectedResult, controller.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, orgHeader.PK.ToGuid()));
			AssertJsonResult(expectedResult, controller.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, relatedOrg.PK.ToGuid()));

			expectedResult = new
			{
				count = 0,
				results = (List<CodeDescription>)null,
			};
			AssertJsonResult(expectedResult, controller.GetContactEmails(Core.Constants.CopyRecipientType.EmailToRecipient, unRelatedOrg.PK.ToGuid()));
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
			var mockedGetRelatedOrgPKsResult = new List<ZGuid> { orgHeader.PK, relatedOrg.PK };

			mockService.Setup(x => x.GetRelatedOrganizationIDs(mockCurrentContactPK)).Returns(mockedGetRelatedOrgPKsResult).Verifiable();
			mockIdentity.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);
			mockIdentity.SetupGet(x => x.ProviderKey).Returns(mockCurrentContactPK);

			var mockedResult = "aaa@test.com";

			mockService.Setup(x => x.GetContactEmail("", orgHeader.PK.ToGuid())).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetContactEmail("", relatedOrg.PK.ToGuid())).Returns(mockedResult).Verifiable();
			mockService.Setup(x => x.GetContactEmail("", unRelatedOrg.PK.ToGuid())).Returns(mockedResult).Verifiable();

			var expectedResult = new
			{
				contactEmail = mockedResult
			};

			AssertJsonResult(expectedResult, controller.GetContactEmail("", orgHeader.PK.ToGuid()));
			AssertJsonResult(expectedResult, controller.GetContactEmail("", relatedOrg.PK.ToGuid()));

			expectedResult = new
			{
				contactEmail = (string)null,
			};
			AssertJsonResult(expectedResult, controller.GetContactEmail("", unRelatedOrg.PK.ToGuid()));
		}
	}
}
