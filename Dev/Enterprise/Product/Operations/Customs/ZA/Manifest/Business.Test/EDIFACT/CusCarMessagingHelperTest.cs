using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT.Testing
{
	sealed class CusCarMessagingHelperTest : TestCaseWithFactory
	{
		[TestDate(2018, 12, 02)]
		public void TestIssueCodeError()
		{
			VoidParameterlessDelegate assertDelegate = () =>
			{
			};
			foreach (var testCase in new[] { new { ManifestType = nameof(ManifestDocumentType.COM), Contains = false },
				new { ManifestType = nameof(ManifestDocumentType.COH), Contains = true },
				new { ManifestType = nameof(ManifestDocumentType.BBB), Contains = false },
				new { ManifestType = nameof(ManifestDocumentType.ECL), Contains = false },
				new { ManifestType = nameof(ManifestDocumentType.FFM), Contains = false },
				new { ManifestType = nameof(ManifestDocumentType.FWB), Contains = true },
				new { ManifestType = nameof(ManifestDocumentType.HAB), Contains = true },
				new { ManifestType = nameof(ManifestDocumentType.RMA), Contains = true },
				new { ManifestType = nameof(ManifestDocumentType.RFM), Contains = true },
				new { ManifestType = nameof(ManifestDocumentType.AQM), Contains = false },
				new { ManifestType = nameof(ManifestDocumentType.ALM), Contains = false },
				new { ManifestType = nameof(ManifestDocumentType.ALH), Contains = true } })
			{
				var manifest = Factory.New<AsycudaManifestHeader>();
				manifest.AMA_ManifestType = testCase.ManifestType;
				var bill = manifest.Bills.AddNew();
				var msg = CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
				assertDelegate += () =>
				{
					AssertEquals($"{testCase.ManifestType}", testCase.Contains, msg.Contains("Issuer code: [No Bill Issuer]"));
				};
			}

			CombineAssertions(assertDelegate);
		}

		public void TestGetBillFunction()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "ZA";
			header.AMA_ManifestType = "COH";

			var bill = header.Bills.AddNew();

			var carHeader = new CusCarHeader(header);

			CombineAssertions(() =>
			{
				var subType = carHeader.MessageFunctionSubTypeForCancel;
				AssertEquals("SubTypeForCancel", subType, CusCarMessagingHelper.GetBillFunction(subType, carHeader, bill));
				AssertEquals("Original", "ORG", CusCarMessagingHelper.GetBillFunction("", carHeader, bill));

				bill.RegistrationDate = CargoWise.Types.ZDateTime.BrettsBirthday;
				bill.RegistrationEntryNumber.CE_EntryNum = "Registered";
				AssertEquals("Amend", carHeader.MessageFunctionSubTypeForAmend, CusCarMessagingHelper.GetBillFunction("", carHeader, bill));
			});
		}
	}
}
