using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR;
using Enterprise.Customs.ZA.Manifest.Business.EDIFACT;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.ZA.Manifest.Business.MessagingProcess.Testing
{
	sealed class CustomsMessagingProviderTest : TestCaseWithFactory
	{
		public void TestCustomsMessagingProviderFactory()
		{
			var msgSubType = "ORG";
			var (header, selectedBills) = CreateHeader(msgSubType: msgSubType);

			ICustomsMessagingProviderFactory providerFactory = new CustomsMessagingProviderFactory(msgSubType, selectedBills);
			var provider = providerFactory.CreateProvider(header);

			AssertType<CustomsMessagingProvider>(provider);
		}

		public void TestIsInTestMode()
		{
			var branch = GlbBranch.CurrentBranch;
			var holdTestMode = Env.Registry.ZACustoms.GetIsTestMode(branch);
			try
			{
				AssertTestModeApplied(branch, testMode: true);
				AssertTestModeApplied(branch, testMode: false);
			}
			finally
			{
				Env.Registry.ZACustoms.SetIsTestMode(branch, holdTestMode);
			}
		}

		void AssertTestModeApplied(GlbBranch branch, bool testMode)
		{
			Env.Registry.ZACustoms.SetIsTestMode(branch, testMode);

			var provider = CreateMessagingProvider();

			AssertNotNull("Provider should not be null", provider);
			AssertEquals("Provider Test Mode", testMode, provider.IsInTestMode);
			AssertEquals("Test mode validation enabled", true, provider.EnableTestModeValidation);
		}

		public void TestMessengers_NoBillsAndPacks()
		{
			var provider = CreateMessagingProvider();

			AssertNotNull("Provider should not be null", provider);
			var messengers = provider.GetMessengers().ToList();
			AssertEquals("Only 1 for the header", 1, messengers.Count);
			AssertType<CustomsMessenger>("Type CustomsMessenger", messengers[0]);
			AssertBuilder<CusCarHeader>(messengers[0]);
		}

		public void TestMessengers_WithBillsAndPacks()
		{
			var provider = CreateMessagingProvider(manifestType: "ALH");

			AssertNotNull("Provider should not be null", provider);
			var messengers = provider.GetMessengers().ToList();
			AssertEquals("Only 1 for the header", 3, messengers.Count);
			AssertType<CustomsMessenger>("Type CustomsMessenger", messengers[0]);
			AssertBuilder<CusCarHeader>(messengers[0]);
		}

		public void TestMessengers_SelectedBills()
		{
			var provider = CreateMessagingProvider(createWithSelectedhBills: true);

			AssertNotNull("Provider should not be null", provider);
			var messengers = provider.GetMessengers().ToList();
			AssertEquals("2 Bills", 2, messengers.Count);
			AssertType<CustomsMessenger>("Type CustomsMessenger", messengers[0]);
			AssertBuilder<CusCarMessagingHelper.SingleBillCusCarHeader>(messengers[0]);
		}

		void AssertBuilder<T>(ICustomsMessenger messenger)
		{
			AssertType<CUSCARMessageBuilder>(messenger.MessageGenerator);

			var builder = (CUSCARMessageBuilder)messenger.MessageGenerator;
			var builderType = builder.GetType();
			var fieldInfo = builderType.GetField("dataSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var dataSource = fieldInfo.GetValue(builder);
			AssertType<T>(dataSource);
		}

		public void TestPreSendValidation_NoBillsAndPacks()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCarrierCode("CCC", "CarrierCode", "ZA");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "Manifest Validation");
			var validationRuleZa = helper.CreateNewOrGetExistingCusCodeList("ZA", RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.BillIssuer, "A Bill issuer is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMANIFESTTYPE", nameof(ManifestDocumentType.COH));
			Factory.Save();

			var (header, provider) = CreateHeaderAndMessagingProvider(manifestType: string.Empty, billIssuer: string.Empty);
			var validator = provider as ISupportPreSendValidation;

			AssertNotNull("Supporter should be ISupportPreSendValidation", validator);

			CombineAssertions(() =>
			{
				var results = validator.RunPreSendValidation(new ActionResult(true)).Where(x => x.IsError).ToList();
				var msgs = string.Join("\n", results.Select(x => x.MessageIncludingPrefix));

				AssertEquals("Error count1", 1, results.Count);
				AssertContains("Manifest Type", "Error: Cannot create a manifest message for manifest type 'None'.", msgs);

				header.AMA_ManifestType = "ECL";
				results = validator.RunPreSendValidation(new ActionResult(true)).Where(x => x.IsError).ToList();
				AssertEquals("Error count2", 0, results.Count);

				header.Bills[1].ABL_BillIssuer = "MUST";
				header.Bills[2].ABL_BillIssuer = "RESET";
				header.AMA_ManifestType = "COH";
				results = validator.RunPreSendValidation(new ActionResult(true)).Where(x => x.IsError).ToList();
				msgs = string.Join("\n", results.Select(x => x.MessageIncludingPrefix));

				AssertEquals("Error count3", 1, results.Count);
				AssertContains("Bill Issuer", "Error: Bill 'B0001' must have a Bill Issuer.", msgs);

				header.Bills[0].ABL_BillIssuer = "AAA";

				results = validator.RunPreSendValidation(new ActionResult(true)).Where(x => x.IsError).ToList();
				AssertEquals("No errors", 0, results.Count);
			});
		}

		public void TestPreSendValidation_HasBillsAndPacks()
		{
			var (header, provider) = CreateHeaderAndMessagingProvider(manifestType: "ALH", createBills: false);
			var validator = provider as ISupportPreSendValidation;

			AssertNotNull("Supporter should be ISupportPreSendValidation", validator);

			CombineAssertions(() =>
			{
				var results = validator.RunPreSendValidation(new ActionResult(true)).Where(x => x.IsError).ToList();
				var msgs = string.Join("\n", results.Select(x => x.MessageIncludingPrefix));

				AssertEquals("Error count", 1, results.Count);
				AssertContains("No bills", "Error: At least one Bill must be created for sending.", msgs);

				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillNumber = "B0001";
				bill1.ABL_BillIssuer = "XYZ";

				results = validator.RunPreSendValidation(new ActionResult(true)).Where(x => x.IsError).ToList();
				AssertEquals("No errors", 0, results.Count);
			});
		}

		public void TestPreSendValidation_SelectedBills()
		{
			var (header, provider) = CreateHeaderAndMessagingProvider(manifestType: "ALH", createBills: false);
			var validator = provider as ISupportPreSendValidation;

			AssertNotNull("Supporter should be ISupportPreSendValidation", validator);

			CombineAssertions(() =>
			{
				var results = validator.RunPreSendValidation(new ActionResult(true)).Where(x => x.IsError).ToList();
				var msgs = string.Join("\n", results.Select(x => x.MessageIncludingPrefix));

				AssertEquals("Error count", 1, results.Count);
				AssertContains("No bills", "Error: At least one Bill must be created for sending.", msgs);

				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillNumber = "B0001";
				bill1.ABL_BillIssuer = "XYZ";

				results = validator.RunPreSendValidation(new ActionResult(true)).Where(x => x.IsError).ToList();
				AssertEquals("No errors", 0, results.Count);
			});
		}

		ICustomsMessagingProvider CreateMessagingProvider(string manifestType = "ECL", string msgSubType = "ORG", string billIssuer = "AAA", bool createBills = true, bool createWithSelectedhBills = false) => CreateHeaderAndMessagingProvider(manifestType, msgSubType, billIssuer, createBills, createWithSelectedhBills).provider;

		(AsycudaManifestHeader header, ICustomsMessagingProvider provider) CreateHeaderAndMessagingProvider(string manifestType = "ECL", string msgSubType = "ORG", string billIssuer = "AAA", bool createBills = true, bool createWithSelectedhBills = false)
		{
			var (header, selectedBills) = CreateHeader(manifestType, msgSubType, billIssuer, createBills, createWithSelectedhBills);

			var provider = CustomsMessagingProvider.New(header, msgSubType, selectedBills);

			return (header, provider);
		}

		(AsycudaManifestHeader header, IReadOnlyCollection<AsycudaBill> selectedBills) CreateHeader(string manifestType = "ECL", string msgSubType = "ORG", string billIssuer = "AAA", bool createBills = true, bool createWithSelectedhBills = false)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "ZA";
			header.AMA_ManifestType = manifestType;
			header.AMA_ManifestNumber = "MAN12345";

			IReadOnlyCollection<AsycudaBill> selectedBills = null;

			if (createBills)
			{
				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillNumber = "B0001";
				bill1.ABL_BillIssuer = billIssuer;

				var bill2 = header.Bills.AddNew();
				bill2.ABL_BillNumber = "B0002";
				bill2.ABL_BillIssuer = "BBB";

				var bill3 = header.Bills.AddNew();
				bill3.ABL_BillNumber = "B0003";
				bill3.ABL_BillIssuer = "CCC";

				if (createWithSelectedhBills)
				{
					selectedBills = new List<AsycudaBill> { bill1, bill2 };
				}
			}
			else if (createWithSelectedhBills)
			{
				selectedBills = new List<AsycudaBill> { };
			}

			return (header, selectedBills);
		}
	}
}
