using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed partial class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsUQList()
		{
			new TestTWCreator(Factory).CreateCustomsUQ();
			var customsUQList = Lookups.CustomsUQList;
			CombineAssertions(() =>
			{
				AssertEquals(1, customsUQList.Count);
				AssertEquals("AMP", customsUQList[0].Code);
				AssertEquals("Ampere", customsUQList[0].Description);
			});
		}

		public void TestPermitUQList()
		{
			new TestTWCreator(Factory).CreateCustomsUQ();
			var permitUQList = Lookups.PermitUQList;
			CombineAssertions(() =>
			{
				AssertEquals(1, permitUQList.Count);
				AssertEquals("AMP", permitUQList[0].Code);
				AssertEquals("Ampere", permitUQList[0].Description);
			});
		}

		public void TestPermitUQList_IsForCMHeaderMessageTypeNX101CertificateType15()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ZHT", "ChineseTraditional");
			helper.CreateCusCodeType("CUSUQ", "Customs Declaration Units of Quantity");
			var cusCodeList = helper.CreateCusCodeList("CN", "CUSUQ", "008", "只", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "隻");
			cusCodeList = helper.CreateCusCodeList("CN", "CUSUQ", "009", "头", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "頭");
			helper.CreateCusCodeList("CN", "CUSUQ", "029", "井", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var declartion = Factory.New<JobDeclaration>();
			var instruction = declartion.CusEntryInstruction;
			var invoiceLine = (JobComInvoiceLine)declartion.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var controllingMessageHeader = instruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			var list = invoiceLine.Lookups.PermitUQList;
			AssertEquals("008, 009", list.CodesAsString);
		}

		public void TestTpfPaymentMethodList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var lookups = invoiceLine.Lookups;
			invoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
			AssertEquals("CAS, DEF, ROR", lookups.TpfPaymentMethodList.CodesAsString);

			invoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
			AssertEquals("CAS, DEF", lookups.TpfPaymentMethodList.CodesAsString);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("CAS", lookups.TpfPaymentMethodList.CodesAsString);
		}

		public void TestInvoiceLine()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestProcedureList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("TW", "EX", "90", ZString.Empty, ZString.Empty, "三角貿易之外貨復出口", "EXP", group: "G3,G5");
			helper.CreateRefCusProcedure("TW", "EX", "94", ZString.Empty, ZString.Empty, "國貨出口供經營國際貿易之非本國籍船舶、航空器或其他運輸工具專用之物料、物品。", "EXP", group: "D1,G5");
			helper.CreateRefCusProcedure("TW", "EX", "9G", ZString.Empty, ZString.Empty, "三角貿易之外貨復出口", "EXP", group: "G7,G5");
			helper.CreateRefCusProcedure("TW", "IM", "5E", ZString.Empty, ZString.Empty, "外交郵袋", "IMP", group: "G3,D2,G1");
			helper.CreateRefCusProcedure("TW", "IM", "65", ZString.Empty, ZString.Empty, "預估稅捐", "IMP", group: "F3,G1");
			var declaration = Factory.New<JobDeclaration>();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var testLookups = invLine.Lookups as JobComInvoiceLineLookups;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			CombineAssertions("Test Procedure List EX", () =>
			{
				AssertEquals(3, testLookups.Procedures.Count);
				AssertEquals(true, testLookups.Procedures.ContainsCode("90"));
				AssertEquals(true, testLookups.Procedures.ContainsCode("94"));
				AssertEquals(true, testLookups.Procedures.ContainsCode("9G"));
				AssertEquals(false, testLookups.Procedures.ContainsCode("5E"));
				AssertEquals(false, testLookups.Procedures.ContainsCode("65"));
			}

			);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			CombineAssertions("Test Procedure List IM", () =>
			{
				AssertEquals(2, testLookups.Procedures.Count);
				AssertEquals(true, testLookups.Procedures.ContainsCode("5E"));
				AssertEquals(true, testLookups.Procedures.ContainsCode("65"));
				AssertEquals(false, testLookups.Procedures.ContainsCode("90"));
				AssertEquals(false, testLookups.Procedures.ContainsCode("94"));
				AssertEquals(false, testLookups.Procedures.ContainsCode("9G"));
			}

			);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var testInstruction = declaration.CusEntryInstruction;
			testInstruction.CEI_Style = "G3";
			invLine.JI_CEI = testInstruction.PK;
			CombineAssertions("Test Procedure List EX and CEI G3", () =>
			{
				AssertEquals(1, testLookups.Procedures.Count);
				AssertEquals(true, testLookups.Procedures.ContainsCode("90"));
				AssertEquals(false, testLookups.Procedures.ContainsCode("5E"));
			}

			);
			testInstruction.CEI_Style = "XX";
			invLine.JI_CEI = testInstruction.PK;
			AssertEquals("Test Procedure List EX and CEI XX", 0, testLookups.Procedures.Count);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testInstruction.CEI_Style = "F3";
			invLine.JI_CEI = testInstruction.PK;
			CombineAssertions("Test Procedure List IM and CEI F3", () =>
			{
				AssertEquals(1, testLookups.Procedures.Count);
				AssertEquals(true, testLookups.Procedures.ContainsCode("65"));
				AssertEquals(false, testLookups.Procedures.ContainsCode("90"));
			}

			);
			testInstruction.CEI_Style = "XX";
			invLine.JI_CEI = testInstruction.PK;
			AssertEquals("Test Procedure List im and CEI XX", 0, testLookups.Procedures.Count);
		}

		public void TestPartyIdentifierCodeList()
		{
			var partyIdentifierList = Lookups.PartyIdentifierCodeList;
			var list = Factory.GetCachedValue<PartyIdentifierCodeList>();
			AssertContainsExactElementsInAnyOrder(list, partyIdentifierList);
			AssertEquals(4, partyIdentifierList.Count);
		}

		public void TestExemptionCodeList()
		{
			var exemptionCodeList = Lookups.ExemptionCodeList;
			var list = Factory.GetCachedValue<ExemptionCodeList>();
			AssertContainsExactElementsInAnyOrder(list, exemptionCodeList);
			AssertEquals(3, exemptionCodeList.Count);
		}

		public void TestInvoiceUQList()
		{
			new TestTWCreator(Factory).CreateInvoiceUQ();
			var invoiceUQList = Lookups.InvoiceUQList;
			AssertEquals(1, invoiceUQList.Count);
			AssertEquals("AMP", invoiceUQList[0].Code);
			AssertEquals("Ampere", invoiceUQList[0].Description);
		}

		public void TestPackagingUQList()
		{
			new TestTWCreator(Factory).CreatePackagingUQ();
			var packagingUQList = Lookups.PackagingUQList;
			AssertCodeDescriptionPairList(packagingUQList, ("AMP", "Ampere"));
		}

		public void TestManufacturerList()
		{
			AssertEquals("Type of TW.JobDeclarationLookups.OrganisationsFindBoxCollection", typeof(OrganisationsFindBoxCollection), Lookups.ManufacturerList.GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTrademarkEDocList()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "supplier";
			supplier.OH_IsConsignor = true;
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PRO1";
			var relatedOrganization = part1.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var decDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			var shipmentDocManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
			var partDocManagerInfo = part1.DocManagerInfo();
			var doc1 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), MessageConstants.DocumentTypes.CAT);
			var doc2 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"), MessageConstants.DocumentTypes.TDM);
			var doc3 = partDocManagerInfo.AddFileOrDocument(doc2.ImageData, "sample1.pdf", MessageConstants.DocumentTypes.CAT);
			var doc4 = partDocManagerInfo.AddFileOrDocument(doc2.ImageData, "TestBitmap1.png", MessageConstants.DocumentTypes.TDM);
			var doc5 = shipmentDocManagerInfo.AddFileOrDocument(doc2.ImageData, "sample2.pdf", MessageConstants.DocumentTypes.CAT);
			var doc6 = shipmentDocManagerInfo.AddFileOrDocument(doc2.ImageData, "TestBitmap2.jpg", MessageConstants.DocumentTypes.TDM);
			Factory.Save();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var trademarkEDocList = invoiceLine.Lookups.TrademarkEDocList;
			var trademarkEDocs = trademarkEDocList.ToArray();
			AssertEquals(ZGuid.Empty, trademarkEDocList.DefaultDocPk);
			AssertEquals(2, trademarkEDocList.Count);
			Assert(!trademarkEDocs.Any(x => (ZGuid)x.PK == doc1.UniqueKey));
			Assert(trademarkEDocs.Any(x => (ZGuid)x.PK == doc2.UniqueKey));
			Assert(!trademarkEDocs.Any(x => (ZGuid)x.PK == doc3.UniqueKey));
			Assert(!trademarkEDocs.Any(x => (ZGuid)x.PK == doc4.UniqueKey));
			Assert(!trademarkEDocs.Any(x => (ZGuid)x.PK == doc5.UniqueKey));
			Assert(trademarkEDocs.Any(x => (ZGuid)x.PK == doc6.UniqueKey));
			invoiceLine.JI_PartNo = "PRO1";
			trademarkEDocList = invoiceLine.Lookups.TrademarkEDocList;
			trademarkEDocs = trademarkEDocList.ToArray();
			AssertEquals(doc4.UniqueKey, trademarkEDocList.DefaultDocPk);
			Assert(!trademarkEDocs.Any(x => (ZGuid)x.PK == doc1.UniqueKey));
			Assert(trademarkEDocs.Any(x => (ZGuid)x.PK == doc2.UniqueKey));
			Assert(!trademarkEDocs.Any(x => (ZGuid)x.PK == doc3.UniqueKey));
			Assert(trademarkEDocs.Any(x => (ZGuid)x.PK == doc4.UniqueKey));
			Assert(!trademarkEDocs.Any(x => (ZGuid)x.PK == doc5.UniqueKey));
			Assert(trademarkEDocs.Any(x => (ZGuid)x.PK == doc6.UniqueKey));
		}

		public void TestSpecialCodeList()
		{
			AssertEquals(ExemptionOfControllingAgenciesCusSupporting.GetSpecialCodeList(InvoiceLine.Factory), Lookups.SpecialCodeList);
		}

		JobComInvoiceLineLookups Lookups => new JobComInvoiceLineLookups(InvoiceLine);

		#region InvoiceLine
		JobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>());
		JobComInvoiceLine invoiceLine;
		#endregion
	}
}
