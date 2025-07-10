using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Services.ServiceHost.GMD;

namespace Enterprise.Services.ServiceHost.Tests.HttpXmlServices.GenericMessageDelivery
{
	class InterchangeCreateRequestValidatorTest : TestCaseWithFactory
	{
		string[] SupportedInterchangeTypes => new[] { GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse };

		public void TestValidInterchange()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DNA";
			company.Branches.AddNew();
			Factory.Save();

			var interchange = new Interchange
			{
				SenderId = "EASYLOG2TEST_EAD",
				RecipientId = "HYEDNACMT",
				InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
				Body = "<Message>TEST</Message>"
			};
			var validator = new InterchangeCreateRequestValidator(SupportedInterchangeTypes, interchange);
			var result = validator.IsValid(out string message, out GlbBranch branch);

			AssertEquals(true, result);
			AssertEquals(null, message);
			AssertEquals(company.Branches[0].PK, branch.PK);
		}

		public void TestInvalidInterchange()
		{
			var interchange = new Interchange
			{
				SenderId = "EASYLOG2TEST_EAD",
				RecipientId = "HYEDNACMT",
				InterchangeType = EDIInterchangeTypeList.Codes.TST,
				Body = "<Message>TEST</Message>"
			};
			var validator = new InterchangeCreateRequestValidator(SupportedInterchangeTypes, interchange);
			var result = validator.IsValid(out string message, out GlbBranch branch);

			AssertEquals(false, result);
			AssertEquals("Interchange Type 'TST' is not supported", message);
			AssertNull(branch);

			interchange = new Interchange
			{
				SenderId = "EASYLOG2TEST_EAD",
				RecipientId = "",
				InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
				Body = "<Message>TEST</Message>"
			};
			validator = new InterchangeCreateRequestValidator(SupportedInterchangeTypes, interchange);
			result = validator.IsValid(out message, out branch);

			AssertEquals(false, result);
			AssertEquals("Recipient Id is missing", message);
			AssertNull(branch);

			interchange = new Interchange
			{
				SenderId = "EASYLOG2TEST_EAD",
				RecipientId = "INVALID RECIPIENT",
				InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
				Body = "<Message>TEST</Message>"
			};
			validator = new InterchangeCreateRequestValidator(SupportedInterchangeTypes, interchange);
			result = validator.IsValid(out message, out branch);

			AssertEquals(false, result);
			AssertEquals("Recipient Id should be a nine character code ({enterprise code}{company code}{server code}) identifying a company on a database", message);
			AssertNull(branch);

			interchange = new Interchange
			{
				SenderId = "EASYLOG2TEST_EAD",
				RecipientId = "ABCDNACMT",
				InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
				Body = "<Message>TEST</Message>"
			};
			validator = new InterchangeCreateRequestValidator(SupportedInterchangeTypes, interchange);
			result = validator.IsValid(out message, out branch);

			AssertEquals(false, result);
			AssertEquals("Recipient Id's enterprise code portion does not match product enterprise code", message);
			AssertNull(branch);

			interchange = new Interchange
			{
				SenderId = "EASYLOG2TEST_EAD",
				RecipientId = "HYEDNAXXX",
				InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
				Body = "<Message>TEST</Message>"
			};
			validator = new InterchangeCreateRequestValidator(SupportedInterchangeTypes, interchange);
			result = validator.IsValid(out message, out branch);

			AssertEquals(false, result);
			AssertEquals("Recipient Id's server code portion does not match product server code", message);
			AssertNull(branch);
		}

		public void TestInvalidCompanyOrBranch()
		{
			var interchange = new Interchange
			{
				SenderId = "EASYLOG2TEST_EAD",
				RecipientId = "HYEXXXCMT",
				InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse,
				Body = "<Message>TEST</Message>"
			};
			var validator = new InterchangeCreateRequestValidator(SupportedInterchangeTypes, interchange);
			var result = validator.IsValid(out string message, out GlbBranch branch);

			AssertEquals(false, result);
			AssertEquals("Unable to find active company using code 'XXX'.", message);
			AssertNull(branch);

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "XXX";
			Factory.Save();

			validator = new InterchangeCreateRequestValidator(SupportedInterchangeTypes, interchange);
			result = validator.IsValid(out message, out branch);

			AssertEquals(false, result);
			AssertEquals("Unable to find active branch for company 'XXX'.", message);
			AssertNull(branch);
		}

		protected override void SetUp()
		{
			var registration = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registration.EnterpriseCodeForTest = "HYE";
			registration.ServerCodeForTest = "CMT";
		}
	}
}
