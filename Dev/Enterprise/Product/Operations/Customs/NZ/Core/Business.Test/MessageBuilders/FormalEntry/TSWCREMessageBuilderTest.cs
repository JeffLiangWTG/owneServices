using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
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
	sealed class TSWCREMessageBuilderTest : XmlMessageBuilderTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestGenerateTestMessage()
		{
			using (NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				DecCreator.SetupTestForCREWriteoffWithConsignmentDetails();
				Declaration.JE_DeclarationReference = "BCRE00000001";
				DecCreator.SetupTestForSea();
				DecCreator.SetupTestForExportToAU();
				DecCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
				DecCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 4000m, 10, "PK");
				Declaration.CusContainers[0].CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C21;
				DecCreator.SetupTestContainer("OOCL0000011", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 7000m, 15, "PK");
				Declaration.CusContainers[1].CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C21;
				DecCreator.SetupTestContainer("OOCL0000027", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 10000m, 20, "PK");
				Declaration.CusContainers[2].CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C21;
				DecCreator.SetupTestContainer("OOCL0000034", ContainerModeList.Codes.LCL, ContainerSizeList.Codes.ContainerIc20Ft, 100m, 10, "PK");
				Declaration.CusContainers[3].CO_MAF_ContainerType = ContainerSizeTypeList.Codes.C21;
				Declaration.JE_TotalWeight = 21000m;
				Declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
				Declaration.JE_ECI_InvoiceAmount = 70.00m;

				var entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader;
				entryHeader.CH_BGMReference = "BCRE00000001";

				messageBuilder = new TSWCREMessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Original, "00009908C");
				var messageGenerated = messageBuilder.GetMessageText();
				AssertASCIIFileSameAsString(BaseSourcePath + @"\Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\MessageBuilders\FormalEntry\TestFiles\TestCREDeclarationMessageOriginal.txt", messageGenerated);
			}
		}

		[TestDate(2024, 3, 1)]
		public void TestGenerateQueuedMessage()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.NewZealand))
			{
				DecCreator.SetupTestForCREWriteoffWithConsignmentDetails();
				var queuedDate = new ZDateTime(2024, 3, 21);
				Declaration.JE_EDITransmitDate = queuedDate;
				messageBuilder = new TSWCREMessageBuilder((Declaration.ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader, null, MessageBuilder.MessageTypes.Original, ZString.Empty);
				messageBuilder.GenerateMessage();
				var message = messageBuilder.MessageBusinessObject;
				AssertEquals("Message is queued in UTC date", queuedDate.ToUniversalBranchTime(Factory).AddMinutes(15), message.EM_HeldUntilDate);
			}
		}

		[TestDate(2017, 10, 16)]
		public void TestSetParentMessagingStatusAfterMessagePosting()
		{
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			DecCreator.SetupTestForCREWriteoffWithConsignmentDetails();
			Declaration.JE_DeclarationReference = "BCRE00000001";
			Declaration.JE_EDITransmitDate = ZDateTime.Today.AddDays(1);

			var entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader;
			messageBuilder = new TSWCREMessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Original, "00009908C");
			messageBuilder.GenerateMessage();
			var messageGenerated = messageBuilder.GetMessageText();
			AssertEquals(FormalEntryStatusList.Codes.QueuedForSending, Declaration.JE_EntryStatus);
			Assert(Declaration.JE_EntrySubmittedDate.IsEmpty);

			Declaration.JE_EDITransmitDate = ZDateTime.Today;
			entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CusEntryHeader;
			messageBuilder = new TSWCREMessageBuilder(entryHeader, null, FormalEntry.MessageBuilder.MessageTypes.Original, "00009908C");
			messageBuilder.GenerateMessage();
			messageGenerated = messageBuilder.GetMessageText();
			AssertEquals(FormalEntryStatusList.Codes.SentToCustoms, Declaration.JE_EntryStatus);
			Assert(!Declaration.JE_EntrySubmittedDate.IsEmpty);
		}

		[TestDate(2018, 09, 18)]
		public void TestITRAirElementsInCRE()
		{
			var expectedITRElements = @"<AdditionalInformation>
      <StatementDescription>NZ1,4,20180904,</StatementDescription>
      <StatementTypeCode>ITR</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementCode>3</StatementCode>
      <StatementTypeCode>MTT</StatementTypeCode>
    </AdditionalInformation>";
			DecCreator.SetupTestForCREWriteoffWithConsignmentDetails();
			DecCreator.SetupTestForAir();

			TranshipmentRequest.Create(Declaration);
			var decITR = Declaration.TranshipmentRequest;
			decITR.C4_MovementReason = MovementReason.Codes.InternationalTranshipmentRequest;
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			decITR.C4_ArrivalDate = ZDateTime.Today.AddDays(-14);
			decITR.C4_FlightNo = "NZ1";
			Factory.Save();

			var entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CustomsEntryHeaders[0];
			var declarationHeader = new TSWEntryHeaderWrapper(entryHeader, null);
			var creBuilder = new CREMessageBuilder(declarationHeader, TSWTransactionTypes.Original, "00009908C");
			var creMessageString = creBuilder.GetXMLMessage();
			AssertEquals("ITR Air elements should have been generated", true, creMessageString.Contains(expectedITRElements));
		}

		[TestDate(2018, 09, 19)]
		public void TestITRSeaElementsInCRE()
		{
			var expectedITRElements = @"<AdditionalInformation>
      <StatementDescription>AEOTORA,1,20180719,716W</StatementDescription>
      <StatementTypeCode>ITR</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementCode>2</StatementCode>
      <StatementTypeCode>MTT</StatementTypeCode>
    </AdditionalInformation>";
			DecCreator.SetupTestForCREWriteoffWithConsignmentDetails();
			DecCreator.SetupTestForSea();

			TranshipmentRequest.Create(Declaration);
			var decITR = Declaration.TranshipmentRequest;
			decITR.C4_MovementReason = MovementReason.Codes.InternationalTranshipmentRequest;
			decITR.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Rail;
			decITR.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
			decITR.C4_ArrivalDate = ZDateTime.Today.AddMonths(-2);
			decITR.C4_TranshipBySeaVessel = "AEOTORA";
			decITR.C4_TranshipBySeaVoyage = "716W";
			Factory.Save();

			var entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)Declaration.CustomsEntryHeaders[0];
			var declarationHeader = new TSWEntryHeaderWrapper(entryHeader, null);
			var creBuilder = new CREMessageBuilder(declarationHeader, TSWTransactionTypes.Original, "00009908C");
			var creMessageString = creBuilder.GetXMLMessage();
			AssertEquals("ITR Sea elements should have been generated", true, creMessageString.Contains(expectedITRElements));
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
