using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccEPaymentBeneficiaryRequest))]
	sealed class AccEPaymentBeneficiaryRequestTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEPaymentDeliveryContextValueProviderMembers()
		{
			var request = GetNewBusinessObject() as AccEPaymentBeneficiaryRequest;
			Factory.Save();
			var dealAsIEPaymentDeliveryContextDataProvider = (IEPaymentDeliveryContextValueProvider)request;

			AssertEquals($"E-Payment Beneficiary Request {request.ABR_InternalReference} submitted for processing to {request.ABR_ProviderCode}", dealAsIEPaymentDeliveryContextDataProvider.Purpose);
			AssertEquals(typeof(AccEPaymentBeneficiaryRequest), dealAsIEPaymentDeliveryContextDataProvider.EntityInfo.Type);
			AssertEquals(request.PK, dealAsIEPaymentDeliveryContextDataProvider.EntityInfo.InternalPK);
		}

		public void TestCreatingUser()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_Code = "TST";
			Factory.Save();
			Assert("testUser is saved.", testUser.IsInDatabase);

			var request = GetNewBusinessObject() as AccEPaymentBeneficiaryRequest;
			request.ABR_SystemCreateUser = testUser.GS_Code;
			Factory.Save();
			Assert("deal is saved.", request.IsInDatabase);

			var newFactory = new BusinessObjectFactory();
			var requestInNewFactory = newFactory.Load<AccEPaymentBeneficiaryRequest>(request.PK);
			AssertNotNull("Creating user should exist.", requestInNewFactory.CreatingUser);
			AssertEquals("Creating user should be testUser.", testUser.PK, requestInNewFactory.CreatingUser.PK);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();

		#endregion
	}
}
