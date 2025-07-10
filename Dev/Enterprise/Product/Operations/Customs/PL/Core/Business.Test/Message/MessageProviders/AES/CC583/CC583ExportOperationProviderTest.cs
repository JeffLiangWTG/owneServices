using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC583ExportOperationProviderTest : DataProviderTestCase<CC583ExportOperationProvider>
{
	public void TestMrn()
	{
		messageSendingObject.EntryNumber = "TEST_MRN";
		AssertEquals("TEST_MRN", Provider.Mrn);
	}

	public void TestExitDate() => CombineAssertions(() =>
	{
		var testDate = new DateTime(2023, 10, 1);
		messageSendingParent.ExitDate = testDate;
		AssertEquals("ExitDate is not empty.", testDate, Provider.ExitDate);
		messageSendingParent.ExitDate = ZDateTime.Empty;
		AssertEquals("ExitDate is empty.", null, Provider.ExitDate);
	});

	public void TestEnquiryInformationCode()
	{
		messageSendingParent.EnquiryInformationCode = "TestEnquiryInformationCode";
		AssertEquals("TestEnquiryInformationCode", Provider.EnquiryInformationCode);
	}

	protected override CC583ExportOperationProvider GetProvider() => new(messageSendingObject, messageSendingParent);

	protected override void SetUp()
	{
		base.SetUp();

		var jobDeclaration = Factory.New<JobDeclaration>();
		var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		messageSendingObject = new BaseMessageSendingObject(cusEntryHeader);
		messageSendingParent = new BaseMessageSendingObjectParent(jobDeclaration);
	}

	BaseMessageSendingObject messageSendingObject;
	BaseMessageSendingObjectParent messageSendingParent;
}
