using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.MessageBuilders.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.Testing
{
	sealed class TSWICRMessageBuilderTest : XmlMessageBuilderTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestGenerateTestMessage()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_PaymentMethod = FreightPaymentMethodList.Codes.AC;
			Declaration.JE_DeclarationReference = "BICR00000001";
			DecCreator.SetupTestForSea();
			DecCreator.SetupTestForImportFromAU();
			DecCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			DecCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 4000m, 10, "PK");
			DecCreator.SetupTestContainer("OOCL0000011", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 7000m, 15, "PK");
			DecCreator.SetupTestContainer("OOCL0000027", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 10000m, 20, "PK");
			DecCreator.SetupTestContainer("OOCL0000034", ContainerModeList.Codes.Empty, ContainerSizeList.Codes.ContainerIc20Ft, 0m, 0, "PK");
			Declaration.JE_TotalWeight = 21000m;
			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			Declaration.JE_ECI_InvoiceAmount = 70.00m;

			var entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader;
			entryHeader.CH_BGMReference = "BICR00000001";

			using (NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				messageBuilder = new TSWICRMessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Original, "00009908C");
				var messageGenerated = messageBuilder.GetMessageText();
				AssertASCIIFileSameAsString(BaseSourcePath + @"\Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\MessageBuilders\FormalEntry\TestFiles\TestICRDeclarationMessageOriginal.txt", messageGenerated);
			}
		}

		[TestDate(2024, 3, 1)]
		public void TestGenerateQueuedMessage()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.NewZealand))
			{
				DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
				var queuedDate = new ZDateTime(2024, 3, 21);
				Declaration.JE_EDITransmitDate = queuedDate;
				messageBuilder = new TSWICRMessageBuilder((Declaration.ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader, null, MessageBuilder.MessageTypes.Original, ZString.Empty);
				messageBuilder.GenerateMessage();
				var message = messageBuilder.MessageBusinessObject;
				AssertEquals("Message is queued in UTC date", queuedDate.ToUniversalBranchTime(Factory).AddMinutes(15), message.EM_HeldUntilDate);
			}
		}

		public void TestWriteOffWithEmptyTranshipmentRequestGeneratesWOF()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var expectedWOFElements = @"<AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>";

			var notWantedITRElements = @"<AdditionalInformation>
      <StatementDescription />
      <StatementTypeCode>ITR</StatementTypeCode>
    </AdditionalInformation>";

			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_DeclarationReference = "BITR00001";
			Declaration.JE_TotalWeight = 21000m;
			Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			Declaration.JE_ECI_InvoiceAmount = 70.00m;
			TranshipmentRequest.Create(Declaration);
			Declaration.TranshipmentRequest.C4_MovementReason = MovementReason.Codes.InternationalTranshipmentRequest;

			var entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CustomsEntryHeaders[0];
			var declarationHeader = new TSWEntryHeaderWrapper(entryHeader, null);
			var icrBuilder = new ICRMessageBuilder(declarationHeader, TSWTransactionTypes.Original, "00009908C");
			var icrMessageString = icrBuilder.GetXMLMessage();
			Assert("ICR Message contains WOF statement elements", icrMessageString.Contains(expectedWOFElements));
			Assert("ICR Message should not container ITR elements", !icrMessageString.Contains(notWantedITRElements));
		}

		[TestDate(2017, 10, 16)]
		public void TestSetParentMessagingStatusAfterMessagePosting()
		{
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			Declaration.JE_DeclarationReference = "BCRE00000001";
			Declaration.JE_EDITransmitDate = ZDateTime.Today.AddDays(1);

			var entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader;
			messageBuilder = new TSWICRMessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Original, "00009908C");
			messageBuilder.GenerateMessage();
			var messageGenerated = messageBuilder.GetMessageText();
			AssertEquals(FormalEntryStatusList.Codes.QueuedForSending, Declaration.JE_EntryStatus);
			Assert(Declaration.JE_EntrySubmittedDate.IsEmpty);

			Declaration.JE_EDITransmitDate = ZDateTime.Today;
			entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader;
			messageBuilder = new TSWICRMessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Original, "00009908C");
			messageBuilder.GenerateMessage();
			messageGenerated = messageBuilder.GetMessageText();
			AssertEquals(FormalEntryStatusList.Codes.SentToCustoms, Declaration.JE_EntryStatus);
			Assert(!Declaration.JE_EntrySubmittedDate.IsEmpty);
		}

		public void TestCombinedMessageStatusAfterICRReplacement()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
				Declaration.JE_DeclarationReference = "B00004037";
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				Declaration.JE_EDITransmitDate = ZDateTime.Today;

				var entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader;
				messageBuilder = new TSWICRMessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Original, "00009908C");
				messageBuilder.GenerateMessage();
				AssertEquals(TSWEntryStatusList.Codes.STC, Declaration.JE_TSWCombinedStatus);

				Declaration.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.CC;
				messageBuilder = new TSWICRMessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Replacement, "00009908C");
				messageBuilder.GenerateMessage();
				AssertEquals("ICR write-off combined (2 char) status should still reflect last MPI Bio response despite amendment being sent", LowValueConsignmentStatusList.Codes.PC, Declaration.JE_TSWCombinedStatus);
			}
		}

		public void TestMCDContainerStatement_WithData()
		{
			var expectedMCDContainerElements = @"<AdditionalInformation>
      <StatementDescription>MNHU0029382,YNYNN</StatementDescription>
      <StatementTypeCode>MCD</StatementTypeCode>
    </AdditionalInformation>";

			SetupDeclarationForMCDContainerStatementTest();
			Declaration.JE_SendMCDContainerQuarantineDeclaration = true;
			Declaration.JE_HaveMAFContainerDeclaration = true;
			Declaration.JE_IsContainerClean = true;
			Declaration.JE_IsWoodPackagingUsed = true;

			AssertMCDStatement(expectedMCDContainerElements);
		}

		public void TestMCDContainerStatement_WithoutData()
		{
			var expectedMCDContainerElements = @"<AdditionalInformation>
      <StatementDescription>MNHU0029382,</StatementDescription>
      <StatementTypeCode>MCD</StatementTypeCode>
    </AdditionalInformation>";

			SetupDeclarationForMCDContainerStatementTest();
			Declaration.JE_SendMCDContainerQuarantineDeclaration = true;

			AssertMCDStatement(expectedMCDContainerElements);
		}

		void AssertMCDStatement(string expectedStatement)
		{
			var entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CustomsEntryHeaders[0];
			var declarationHeader = new TSWEntryHeaderWrapper(entryHeader, null);
			var icrBuilder = new ICRMessageBuilder(declarationHeader, TSWTransactionTypes.Original, "00009908C");
			var icrMessageString = icrBuilder.GetXMLMessage();
			Assert("ICR Message contains MCD Container statement elements", icrMessageString.Contains(expectedStatement));
		}

		void SetupDeclarationForMCDContainerStatementTest()
		{
			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			Declaration.DisableDefaultPackingInformation = false;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_HouseBill = "HB00329";
			Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			Declaration.JE_RL_NKFinalDestination = "NZAKL";

			var container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "MNHU0029382";
			container1.CO_ContainerSize = "23";
			container1.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
		}

		public override void TestGenerateLiveMessage()
		{
			Assert(true);
		}

		#region DecCreator
		TestECIWriteOffCreator DecCreator
		{
			get
			{
				if (fDecCreator == null)
				{
					fDecCreator = new TestECIWriteOffCreator(Declaration);
				}
				return fDecCreator;
			}
		}
		TestECIWriteOffCreator fDecCreator;
		#endregion

		JobDeclaration Declaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				}

				return jobDeclaration;
			}
		}
		JobDeclaration jobDeclaration;
	}
}
