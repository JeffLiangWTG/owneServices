using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ImporterNumberRequesterTest : TestCaseWithFactory
	{
		public void TestHasPermissionToSendImporterBondNumber()
		{
			var importerNumberRequester = new ImporterNumberRequester();
			var interfaceImporterNumberRequester = (IImporterNumberRequester)importerNumberRequester;
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			Assert(importerNumberRequester.HasPermissionToSendImporterBondNumber("12345"));
			Assert(interfaceImporterNumberRequester.HasPermissionToSendImporterBondNumber("12345"));
			Assert(importerNumberRequester.HasPermissionToSendImporterBondNumber("123-12-1234"));
			Assert(interfaceImporterNumberRequester.HasPermissionToSendImporterBondNumber("123-12-1234"));
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			Assert(importerNumberRequester.HasPermissionToSendImporterBondNumber("12345"));
			Assert(interfaceImporterNumberRequester.HasPermissionToSendImporterBondNumber("12345"));
			Assert(!importerNumberRequester.HasPermissionToSendImporterBondNumber("123-12-1234"));
			Assert(!interfaceImporterNumberRequester.HasPermissionToSendImporterBondNumber("123-12-1234"));
		}

		[TestDate(2016, 03, 08)]
		public void TestRequestImporterBond()
		{
			const string importerNumber = "12-345678901";

			ZQuery query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, importerNumber);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.QueryImporterBond);

			int cnt = Factory.GetDatabaseCount(typeof(EDIMessage), query);
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			OrgHeaderWrapper organisationWrapper = OrgHeaderWrapper.New(organisation);
			Factory.Save();

			new ImporterNumberRequester().RequestImporterBond(organisation, importerNumber);
			AssertEquals(cnt + 1, Factory.GetDatabaseCount(typeof(EDIMessage), query));
			AssertEquals(1, organisationWrapper.Messages.Count);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, organisationWrapper.Messages[0].EM_MessageType);
		}

		public void TestCurrentCompanyRegistryItemValidationForRequestImporterBond()
		{
			var company = GlbCompany.CurrentCompany;
			var errorMessage = new ImporterNumberRequester().CurrentCompanyRegistryItemValidationForRequestImporterBond();
			AssertEquals("The Processing Port Code or Entry Filer Code has not been set up in the Registry for the current login company.", errorMessage);

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Name = "COMPANY2";
			company2.GC_RN_NKCountryCode = "US";
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "US1";

			var filerCode2 = new EntryFiler();
			filerCode2.EntryFilerCode = "SV8";
			Factory.Save();

			USCustomsDataRegistry.Instance.EntryFiler.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, filerCode2);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, "3922");
			USCustomsDataRegistry.Instance.BRecordOfficeCode.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "BB");
			errorMessage = new ImporterNumberRequester().CurrentCompanyRegistryItemValidationForRequestImporterBond();
			AssertEquals("The Processing Port Code or Entry Filer Code has not been set up in the Registry for the current login company. Please log in to 'DAN - COMPANY2' to send this query message. ", errorMessage);

			var filerCode = new EntryFiler();
			filerCode.EntryFilerCode = "SV9";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, filerCode);
			errorMessage = new ImporterNumberRequester().CurrentCompanyRegistryItemValidationForRequestImporterBond();
			AssertEquals("The Processing Port Code or Entry Filer Code has not been set up in the Registry for the current login company. Please log in to 'DAN - COMPANY2' to send this query message. ", errorMessage);

			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "~B1";
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, "3911");
			errorMessage = new ImporterNumberRequester().CurrentCompanyRegistryItemValidationForRequestImporterBond();
			AssertEquals(ZString.Empty, errorMessage);
		}
	}
}
