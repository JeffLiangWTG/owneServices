using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX101Application))]
	sealed class NX101ApplicationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAdhocCode()
		{
			header.TW1_IsSpecialApplication = true;
			NUnit.Framework.Assert.That(application.AdhocCode, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));

			header.TW1_IsSpecialApplication = false;
			NUnit.Framework.Assert.That(application.AdhocCode, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdhocProcessNumber()
		{
			var specialApplicationId = "20211118A000197";
			header.TW1_SpecialApplicationId = specialApplicationId;
			NUnit.Framework.Assert.That(application.AdhocProcessNumber, NUnit.Framework.Is.EqualTo(specialApplicationId).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCopyQuantity()
		{
			header.TW1_CopyQuantity = 8;
			NUnit.Framework.Assert.That(application.CopyQuantity, NUnit.Framework.Is.EqualTo(8).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDescriptionTooLong()
		{
			NUnit.Framework.Assert.That(this.application.DescriptionTooLong, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var mock = new Mock<INX101>();
			var itemMock = new Mock<IGovernmentAgencyGoodsItem>();
			itemMock.Setup(x => x.Commodity.Description).Returns(new ZString('A', 513));
			mock.Setup(x => x.GoodsShipment.GovernmentAgencyGoodsItems).Returns(new[] { itemMock.Object });
			var application = new NX101Application(header, mock.Object);
			NUnit.Framework.Assert.That(application.DescriptionTooLong, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestECFAPrintingDescription()
		{
			var ecfaPrintedRemarks = "訂單號碼 559000000";
			header.TW1_ECFAPrintedRemarks = ecfaPrintedRemarks;
			NUnit.Framework.Assert.That(application.ECFAPrintingDescription, NUnit.Framework.Is.EqualTo(ecfaPrintedRemarks).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEUSteelDeclarationCode()
		{
			header.TW1_EUSteelProductNo = EUSteelDeclarationCodeList.Codes._02;
			NUnit.Framework.Assert.That(application.EUSteelDeclarationCode, NUnit.Framework.Is.EqualTo("02").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestEUSteelPhaseCode()
		{
			header.TW1_EUSteelProductPhase = EUSteelPhaseCodeList.Codes.Period4;
			NUnit.Framework.Assert.That(application.EUSteelPhaseCode, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestFishingCONoExport()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			NUnit.Framework.Assert.That(application.FishingCONoExport, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code0;
			NUnit.Framework.Assert.That(application.FishingCONoExport, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestGoodsReleaseCodeAndReasonCode()
		{
			header.TW1_BeforeClearanceApplicationReason = CPT_127_GoodsReleaseReasonCodeList.Codes._02;

			CombineAssertions("When GoodsReleaseCode should be Y", () =>
			{
				NUnit.Framework.Assert.That(application.GoodsReleaseCode, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "GoodsReleaseCode");
				NUnit.Framework.Assert.That(application.GoodsReleaseReasonCode, NUnit.Framework.Is.EqualTo("02").Using(CustomComparers.TypeComparison), "GoodsReleaseReasonCode when CertificateType is not 15");
				header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
				NUnit.Framework.Assert.That(application.GoodsReleaseReasonCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "GoodsReleaseReasonCode when CertificateType is 15");
			});

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CABF0945600030";
			entryHeader.CusEntryNumber.CE_EntryStatus = "C1";
			CombineAssertions("When GoodsReleaseCode should be N", () =>
			{
				NUnit.Framework.Assert.That(application.GoodsReleaseCode, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison), "GoodsReleaseCode");
				NUnit.Framework.Assert.That(application.GoodsReleaseReasonCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "GoodsReleaseReasonCode when CertificateType is not 15");
				header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
				NUnit.Framework.Assert.That(application.GoodsReleaseReasonCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "GoodsReleaseReasonCode when CertificateType is 15");
			});
		}

		[ExpectNoExceptions]
		public void TestManufacturerPrintingCode()
		{
			header.TW1_ManufacturerPrintingCode = CPT_123_ManufacturerPrintingCodeList.Codes._2;
			NUnit.Framework.Assert.That(application.ManufacturerPrintingCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestObservations()
		{
			var observations = "保稅工廠進口原料加工，未達實質轉型標準";
			header.TW1_Observations = observations;
			NUnit.Framework.Assert.That(application.Observations, NUnit.Framework.Is.EqualTo(observations).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestOriginalCopyQuantity()
		{
			header.TW1_OriginalQuantity = 6;
			NUnit.Framework.Assert.That(application.OriginalCopyQuantity, NUnit.Framework.Is.EqualTo(6).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPreviousCORenderCode()
		{
			header.TW1_ReturnPreviousCOO = true;
			NUnit.Framework.Assert.That(application.PreviousCORenderCode, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));

			header.TW1_ReturnPreviousCOO = false;
			NUnit.Framework.Assert.That(application.PreviousCORenderCode, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPrintingCode()
		{
			header.TW1_PrintingCode = CPT_123_PrintingCodeList.Codes._02;
			NUnit.Framework.Assert.That(application.PrintingCode, NUnit.Framework.Is.EqualTo("02").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTriangularTradeCode()
		{
			header.TW1_IsTriangularTrade = true;
			NUnit.Framework.Assert.That(application.TriangularTradeCode, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));

			header.TW1_IsTriangularTrade = false;
			NUnit.Framework.Assert.That(application.TriangularTradeCode, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			NUnit.Framework.Assert.That(application.TypeCode, NUnit.Framework.Is.EqualTo("15").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAgent()
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			NUnit.Framework.Assert.That(application.Agent, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));

			var org = new TestTWCreator(Factory).CreateOrganization();
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			NUnit.Framework.Assert.That(application.Agent, NUnit.Framework.Is.TypeOf<LicensingMessageApplicationAgent>());
		}

		[ExpectNoExceptions]
		public void TestContactOffice()
		{
			header.TW1_ProcessingUnit = "AA";
			NUnit.Framework.Assert.That(application.ContactOffice, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestApplicant()
		{
			NUnit.Framework.Assert.That(application.Applicant, NUnit.Framework.Is.TypeOf<LicensingMessageApplicationApplicant>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageSendingObject = new NX101MessageSendingObject(header);
			application = new NX101Application(header, messageSendingObject);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		NX101MessageSendingObject messageSendingObject;
		NX101Application application;
	}
}
