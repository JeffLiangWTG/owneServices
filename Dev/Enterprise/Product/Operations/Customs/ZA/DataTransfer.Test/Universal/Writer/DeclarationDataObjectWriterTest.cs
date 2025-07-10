using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class DeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		[TestDate(2017, 03, 01)]
		public void TestMasterBillIssuedDate()
		{
			var factory = Factory.BOFactory;
			(factory as IExternalFetchHintSupporter).SetupCreator();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "ABCDEFG";
			declaration.JE_MasterBillIssuedDate = ZDateTime.Today;
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertNotNull("declarationData.AdditionalBillCollection", declarationData.AdditionalBillCollection);
			AssertEquals("MasterBill Issue Date", ZDateTime.Today, declarationData.AdditionalBillCollection.FirstOrDefault(x =>
			{
				var code = x.BillType.Code ?? ZString.Empty;
				return code == WayBillTypeList.Codes.Master;
			}).IssueDate);
		}

		public void TestCommercialInvoiceLineEntryReferenceDataMappings()
		{
			var factory = Factory.BOFactory;
			(factory as IExternalFetchHintSupporter).SetupCreator();
			var declaration = factory.New<JobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = "XP1";
			entry1.CH_BGMReference = "BGM32423";
			var entry1Line1 = entry1.MergedLines.AddNew();
			entry1Line1.CL_LineNumber = 1;
			var entry1Line2 = entry1.MergedLines.AddNew();
			entry1Line2.CL_LineNumber = 2;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = "XP2";
			entry2.CH_BGMReference = "BGM56832";
			var entry2Line = entry2.MergedLines.AddNew();
			entry2Line.CL_LineNumber = 1;
			var entry3Mock = factory.New<DummyCusEntryHeader>();
			var entry3Lookups = new Mock<CusEntryHeaderLookups>(entry3Mock) { CallBase = true };
			var list = new CodeDescriptionPairList();
			list.AddPair("XP1", "XP1 DESC");
			list.AddPair("XP2", "XP2 DESC");
			entry3Lookups.Setup(m => m.CH_MessageTypeList).Returns(list);
			entry3Mock.GetNewLookupsReturns = entry3Lookups.Object;
			entry3Mock.CH_MessageTypeReturns = "XP1";
			entry3Mock.CH_BGMReferenceReturns = "BGM56832";
			var entry3Line = entry3Mock.MergedLines.AddNew();
			entry3Line.CL_LineNumber = 1;
			declaration.CustomsEntryHeaders.Add(entry3Mock);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "4020.10.30 1";
			invoiceLine1.JI_CL = entry2Line.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1020.30.40 5";
			invoiceLine2.JI_CL = entry3Line.PK;
			Factory.SaveForTesting();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertNotNull("declarationData.CommercialInfo", declarationData.CommercialInfo);
			AssertEquals("declarationData.CommercialInfo.CommercialInvoiceCollection.Count", 1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection[0];
			var commercialInvoiceLineCollection = invoiceData.CommercialInvoiceLineCollection;
			AssertEquals("commercialInvoiceLineCollection.Count", 2, commercialInvoiceLineCollection.Count);
		}

		public void TestCustomsOfficeDescriptionPopulation()
		{
			var boFactory = new BusinessObjectFactory();
			var helper = new ZAUniversalReferenceTestDataHelper(boFactory);
			helper.CreateCustomsOfficeCusCodeEntry("TST", "TESTEROFF");
			boFactory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsOffice = "TST";
			Factory.SaveForTesting();
			var factory = Factory.BOFactory;
			(factory as IExternalFetchHintSupporter).SetupCreator();
			var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var declarationData = writer.GetDataObject(declaration);
			AssertEquals("TST", declarationData.CustomsOffice.Code);
			AssertEquals("TESTEROFF", declarationData.CustomsOffice.Description);
		}
	}

	sealed class DummyCusEntryHeader : CusEntryHeader
	{
		public DummyCusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public Customs.Business.CusEntryHeaderLookups GetNewLookupsReturns { get; set; }

		public ZString CH_MessageTypeReturns { get; set; } = string.Empty;
		public ZString CH_BGMReferenceReturns { get; set; } = string.Empty;

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups()
		{
			return GetNewLookupsReturns;
		}

		public override ZString CH_MessageType { get => CH_MessageTypeReturns; }
		public override ZString CH_BGMReference { get => CH_BGMReferenceReturns; }
	}
}
