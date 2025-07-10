using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCFXUpliftConfigurationViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEmptyCurrencyAllowed()
		{
			var cfxConfig = Factory.New<AccCFXUpliftConfiguration>();
			cfxConfig.JCF_RX_NKCurrency = ZString.Empty;
			cfxConfig.Validation.ValidateJCF_RX_NKCurrency();
			AssertEquals(false, cfxConfig.HasErrors);
		}

		public void TestPercentageBetween0and100Checked()
		{
			var cfxConfig = Factory.New<AccCFXUpliftConfiguration>();

			cfxConfig.JCF_CFXPercentage = ZDecimal.Zero;
			cfxConfig.Validation.ValidateJCF_CFXPercentage();
			AssertEquals(false, cfxConfig.HasErrors);

			cfxConfig.JCF_CFXPercentage = 100;
			cfxConfig.Validation.ValidateJCF_CFXPercentage();
			AssertEquals(false, cfxConfig.HasErrors);

			cfxConfig.JCF_CFXPercentage = -1;
			cfxConfig.Validation.ValidateJCF_CFXPercentage();
			AssertEquals(true, cfxConfig.HasErrors);

			cfxConfig.JCF_CFXPercentage = 100.01;
			cfxConfig.Validation.ValidateJCF_CFXPercentage();
			AssertEquals(true, cfxConfig.HasErrors);
		}

		public void TestMinimumBetweenZeroAndMaxMoneyChecked()
		{
			var cfxConfig = Factory.New<AccCFXUpliftConfiguration>();

			cfxConfig.JCF_CFXMinimum = ZDecimal.Zero;
			cfxConfig.Validation.ValidateJCF_CFXMinimum();
			AssertEquals(false, cfxConfig.HasErrors);

			cfxConfig.JCF_CFXMinimum = 922337203685477.5807m;
			cfxConfig.Validation.ValidateJCF_CFXMinimum();
			AssertEquals(false, cfxConfig.HasErrors);

			cfxConfig.JCF_CFXMinimum = -1;
			cfxConfig.Validation.ValidateJCF_CFXMinimum();
			AssertEquals(true, cfxConfig.HasErrors);

			cfxConfig.JCF_CFXMinimum = 922337203685477.5808m;
			cfxConfig.Validation.ValidateJCF_CFXMinimum();
			AssertEquals(true, cfxConfig.HasErrors);
		}

		public void TestListValidationTransportMode()
		{
			var testCases = new[]
			{
				new { value = ZString.Empty, isOk = false },
				new { value = (ZString)"XXX", isOk = false },
				new { value = (ZString)"IMP", isOk = true },
				new { value = (ZString)"EXP", isOk = true },
			};

			var cfxConfig = Factory.New<AccCFXUpliftConfiguration>();

			foreach (var test in testCases)
			{
				cfxConfig.JCF_ServiceDirection = test.value;
				cfxConfig.Validation.ValidateJCF_ServiceDirection();
				AssertEquals(test.isOk, !cfxConfig.HasErrors);
			}
		}

		public void TestListValidationServiceDirection()
		{
			var testCases = new[]
			{
				new { value = ZString.Empty, isOk = false },
				new { value = (ZString)"XXX", isOk = false },
				new { value = (ZString)"AIR", isOk = true },
				new { value = (ZString)"SEA", isOk = true },
			};

			var cfxConfig = Factory.New<AccCFXUpliftConfiguration>();

			foreach (var test in testCases)
			{
				cfxConfig.JCF_TransportMode = test.value;
				cfxConfig.Validation.ValidateJCF_TransportMode();
				AssertEquals($"{test.value}", test.isOk, !cfxConfig.HasErrors);
			}
		}

		public void TestListValidationOriginCountry()
		{
			var cfxConfig = Factory.New<AccCFXUpliftConfiguration>();

			cfxConfig.JCF_RN_NKOriginCountry = ZString.Empty;
			cfxConfig.Validation.ValidateJCF_RN_NKOriginCountry();
			AssertEquals("Origin can be empty", false, cfxConfig.HasErrors);

			cfxConfig.JCF_RN_NKOriginCountry = "52";
			cfxConfig.Validation.ValidateJCF_RN_NKOriginCountry();
			AssertEquals("Invalid country code", true, cfxConfig.HasErrors);

			cfxConfig.JCF_RN_NKOriginCountry = "AU";
			cfxConfig.Validation.ValidateJCF_RN_NKOriginCountry();
			AssertEquals("AU", false, cfxConfig.HasErrors);
		}

		public void TestListValidationDestinationCountry()
		{
			var cfxConfig = Factory.New<AccCFXUpliftConfiguration>();

			cfxConfig.JCF_RN_NKDestinationCountry = ZString.Empty;
			cfxConfig.Validation.ValidateJCF_RN_NKDestinationCountry();
			AssertEquals("Destination can be empty", false, cfxConfig.HasErrors);

			cfxConfig.JCF_RN_NKDestinationCountry = "52";
			cfxConfig.Validation.ValidateJCF_RN_NKDestinationCountry();
			AssertEquals("Invalid country code", true, cfxConfig.HasErrors);

			cfxConfig.JCF_RN_NKDestinationCountry = "AU";
			cfxConfig.Validation.ValidateJCF_RN_NKDestinationCountry();
			AssertEquals("AU", false, cfxConfig.HasErrors);
		}

		public void TestValidationStartDate()
		{
			var cfxConfig = Factory.New<AccCFXUpliftConfiguration>();

			cfxConfig.JCF_StartDate = ZDate.Empty;
			cfxConfig.JCF_ExpiryDate = ZDate.Empty;
			cfxConfig.Validation.ValidateJCF_StartDate();
			AssertEquals("Start Date can be empty", false, cfxConfig.HasErrors);

			cfxConfig.JCF_ExpiryDate = new ZDate(2025, 1, 5);
			cfxConfig.Validation.ValidateJCF_StartDate();
			AssertEquals("Start Date must have a value if there is an Expiry Date", true, cfxConfig.HasErrors);

			cfxConfig.JCF_StartDate = new ZDate(2025, 1, 4);
			cfxConfig.Validation.ValidateJCF_StartDate();
			AssertEquals(false, cfxConfig.HasErrors);

			cfxConfig.JCF_StartDate = new ZDate(2025, 1, 6);
			cfxConfig.Validation.ValidateJCF_StartDate();
			AssertEquals("Expiry Date cannot be earlier than Start Date.", true, cfxConfig.HasErrors);
		}

		public void TestValidationExpiryDate()
		{
			var cfxConfig = Factory.New<AccCFXUpliftConfiguration>();

			cfxConfig.JCF_StartDate = ZDate.Empty;
			cfxConfig.JCF_ExpiryDate = ZDate.Empty;
			cfxConfig.Validation.ValidateJCF_ExpiryDate();
			AssertEquals("Expiry Date can be empty", false, cfxConfig.HasErrors);

			cfxConfig.JCF_StartDate = new ZDate(2025, 1, 5);
			cfxConfig.Validation.ValidateJCF_ExpiryDate();
			AssertEquals("Expiry Date must have a value if there is a Start Date", true, cfxConfig.HasErrors);

			cfxConfig.JCF_ExpiryDate = new ZDate(2025, 1, 6);
			cfxConfig.Validation.ValidateJCF_ExpiryDate();
			AssertEquals(false, cfxConfig.HasErrors);

			cfxConfig.JCF_ExpiryDate = new ZDate(2025, 1, 4);
			cfxConfig.Validation.ValidateJCF_ExpiryDate();
			AssertEquals("Expiry Date cannot be earlier than Start Date.", true, cfxConfig.HasErrors);
		}

		public void TestCheckForDuplicates()
		{
			var fakeGcPk = ZGuid.NewZGuid();

			var cfxConfigCollection = new AccCFXUpliftConfigurationCollection(Factory, fakeGcPk);

			var cfxConfig = cfxConfigCollection.AddNew();

			cfxConfig.JCF_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			cfxConfig.JCF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			cfxConfig.JCF_CFXPercentage = 10;
			cfxConfig.JCF_CFXMinimum = 5;

			cfxConfig.Validation.ValidateAll();
			AssertEquals(false, cfxConfig.HasErrors);

			var cfxConfigDup = cfxConfigCollection.AddNew();
			cfxConfigDup.JCF_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			cfxConfigDup.JCF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			cfxConfigDup.JCF_CFXPercentage = 10;
			cfxConfigDup.JCF_CFXMinimum = 5;

			cfxConfigDup.Validation.ValidateAll();

			AssertEquals(false, cfxConfigDup.HasErrors);
			cfxConfigDup.JCF_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			AssertDuplicateError(cfxConfigDup);

			cfxConfigDup.JCF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
			AssertEquals(false, cfxConfigDup.HasErrors);
			cfxConfigDup.JCF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			AssertDuplicateError(cfxConfigDup);

			cfxConfigDup.JCF_JobType = "JOB";
			AssertEquals(false, cfxConfigDup.HasErrors);
			cfxConfigDup.JCF_JobType = "ALL";
			AssertDuplicateError(cfxConfigDup);

			cfxConfigDup.JCF_RN_NKOriginCountry = "CN";
			AssertEquals(false, cfxConfigDup.HasErrors);
			cfxConfigDup.JCF_RN_NKOriginCountry = "";
			AssertDuplicateError(cfxConfigDup);

			cfxConfigDup.JCF_RN_NKDestinationCountry = "CN";
			AssertEquals(false, cfxConfigDup.HasErrors);
			cfxConfigDup.JCF_RN_NKDestinationCountry = "";
			AssertDuplicateError(cfxConfigDup);

			cfxConfigDup.JCF_RX_NKCurrency = "USD";
			AssertEquals(false, cfxConfigDup.HasErrors);
			cfxConfigDup.JCF_RX_NKCurrency = ZString.Empty;
			AssertDuplicateError(cfxConfigDup);

			cfxConfigDup.JCF_StartDate = new ZDate(2025, 1, 14);
			cfxConfigDup.JCF_ExpiryDate = new ZDate(2025, 1, 16);
			AssertEquals(false, cfxConfigDup.HasErrors);
			cfxConfigDup.JCF_StartDate = ZDate.Empty;
			cfxConfigDup.JCF_ExpiryDate = ZDate.Empty;
			AssertDuplicateError(cfxConfigDup);

			void AssertDuplicateError(AccCFXUpliftConfiguration cfxUpliftConfig)
			{
				AssertEquals(true, cfxUpliftConfig.HasRowErrors);
				AssertEquals(AccCFXUpliftConfigurationViewValidation.IsDuplicateErrorString, cfxUpliftConfig.RowErrors.First().Message);
			}
		}

		public void TestForOverlappingDates()
		{
			var fakeGcPk = ZGuid.NewZGuid();

			var cfxConfigCollection = new AccCFXUpliftConfigurationCollection(Factory, fakeGcPk);

			var cfxConfig = cfxConfigCollection.AddNew();
			cfxConfig.JCF_StartDate = new ZDate(2025, 1, 12);
			cfxConfig.JCF_ExpiryDate = new ZDate(2025, 1, 14);
			cfxConfig.JCF_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
			cfxConfig.JCF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
			cfxConfig.JCF_CFXPercentage = 10;
			cfxConfig.JCF_CFXMinimum = 5;

			cfxConfig.Validation.ValidateAll();
			AssertEquals(false, cfxConfig.HasErrors);

			var cfxConfigOverlapping = cfxConfigCollection.AddNew();
			AssertOverlappingDates(new ZDate(2025, 1, 11), new ZDate(2025, 1, 12));
			AssertOverlappingDates(new ZDate(2025, 1, 13), new ZDate(2025, 1, 13));
			AssertOverlappingDates(new ZDate(2025, 1, 14), new ZDate(2025, 1, 15));
			AssertOverlappingDates(new ZDate(2025, 1, 11), new ZDate(2025, 1, 15));

			void AssertOverlappingDates(ZDate startDate, ZDate expiryDate)
			{
				cfxConfigOverlapping.JCF_StartDate = startDate;
				cfxConfigOverlapping.JCF_ExpiryDate = expiryDate;
				cfxConfigOverlapping.JCF_JobType = "ALL";
				cfxConfigOverlapping.JCF_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
				cfxConfigOverlapping.JCF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
				cfxConfigOverlapping.JCF_RN_NKOriginCountry = ZString.Empty;
				cfxConfigOverlapping.JCF_RN_NKDestinationCountry = ZString.Empty;
				cfxConfigOverlapping.JCF_RX_NKCurrency = ZString.Empty;
				cfxConfigOverlapping.JCF_CFXPercentage = 10;
				cfxConfigOverlapping.JCF_CFXMinimum = 5;

				cfxConfigOverlapping.Validation.ValidateAll();
				AssertEquals(false, cfxConfigOverlapping.HasErrors);

				cfxConfigOverlapping.JCF_ServiceDirection = OrgConstants.ServiceDirection.Code.Export;
				AssertOverlappingDatesError(cfxConfigOverlapping);

				cfxConfigOverlapping.JCF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Sea;
				AssertEquals(false, cfxConfigOverlapping.HasErrors);
				cfxConfigOverlapping.JCF_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.Air;
				AssertOverlappingDatesError(cfxConfigOverlapping);

				cfxConfigOverlapping.JCF_JobType = "JOB";
				AssertEquals(false, cfxConfigOverlapping.HasErrors);
				cfxConfigOverlapping.JCF_JobType = "ALL";
				AssertOverlappingDatesError(cfxConfigOverlapping);

				cfxConfigOverlapping.JCF_RN_NKOriginCountry = "CN";
				AssertEquals(false, cfxConfigOverlapping.HasErrors);
				cfxConfigOverlapping.JCF_RN_NKOriginCountry = ZString.Empty;
				AssertOverlappingDatesError(cfxConfigOverlapping);

				cfxConfigOverlapping.JCF_RN_NKDestinationCountry = "CN";
				AssertEquals(false, cfxConfigOverlapping.HasErrors);
				cfxConfigOverlapping.JCF_RN_NKDestinationCountry = ZString.Empty;
				AssertOverlappingDatesError(cfxConfigOverlapping);

				cfxConfigOverlapping.JCF_RX_NKCurrency = "USD";
				AssertEquals(false, cfxConfigOverlapping.HasErrors);
				cfxConfigOverlapping.JCF_RX_NKCurrency = ZString.Empty;
				AssertOverlappingDatesError(cfxConfigOverlapping);

				cfxConfigOverlapping.JCF_StartDate = ZDate.Empty;
				cfxConfigOverlapping.JCF_ExpiryDate = ZDate.Empty;
				AssertEquals(false, cfxConfigOverlapping.HasErrors);
				cfxConfigOverlapping.JCF_StartDate = startDate;
				cfxConfigOverlapping.JCF_ExpiryDate = expiryDate;
				AssertOverlappingDatesError(cfxConfigOverlapping);
			}

			void AssertOverlappingDatesError(AccCFXUpliftConfiguration cfxUpliftConfig)
			{
				AssertEquals(true, cfxUpliftConfig.HasRowErrors);
				AssertEquals(AccCFXUpliftConfigurationViewValidation.OverlappingDatesErrorString, cfxUpliftConfig.RowErrors.First().Message);
			}
		}
	}
}
