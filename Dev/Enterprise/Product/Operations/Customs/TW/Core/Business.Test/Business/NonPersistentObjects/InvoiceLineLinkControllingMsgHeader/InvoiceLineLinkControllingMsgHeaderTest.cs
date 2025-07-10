using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceLineLinkControllingMsgHeader))]
	sealed class InvoiceLineLinkControllingMsgHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceLineLinkControllingMsgHeader(controllingMessageHeader, invoiceLine);
		}

		public void TestIsLinkedCMHeaderReadOnly()
		{
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			var controllingMessageHeader2 = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader2.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			controllingMessageHeader2.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			var controllingMessageHeader3 = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader3.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			controllingMessageHeader3.TW1_CertificateType = CertificateTypeList.Codes.Code7;

			var invoiceLineLinkControllingMsgHeader1 = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == controllingMessageHeader.PK);
			invoiceLineLinkControllingMsgHeader1.IsLinkedCMHeader = true;
			AssertEquals(false, invoiceLineLinkControllingMsgHeader1.IsLinkedCMHeaderInfo.ReadOnly);

			var invoiceLineLinkControllingMsgHeader2 = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == controllingMessageHeader2.PK);
			AssertEquals(false, invoiceLineLinkControllingMsgHeader2.IsLinkedCMHeaderInfo.ReadOnly);

			var invoiceLineLinkControllingMsgHeader3 = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == controllingMessageHeader3.PK);
			AssertEquals(true, invoiceLineLinkControllingMsgHeader3.IsLinkedCMHeaderInfo.ReadOnly);

			var invoiceLineLinkControllingMsgHeader4 = invoiceLine2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == controllingMessageHeader3.PK);
			AssertEquals(false, invoiceLineLinkControllingMsgHeader4.IsLinkedCMHeaderInfo.ReadOnly);
		}

		public void TestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "TWReceivingUnit");
			var processingUnit1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(processingUnit1.PK, RefCusCodeListAttributeTypes.Codes.ControlAgency, ControllingAgencyList.Codes._20);
			var processingUnit2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "XXXX4567", "ZZZZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(processingUnit2.PK, RefCusCodeListAttributeTypes.Codes.ControlAgency, ControllingAgencyList.Codes.VP);
			Factory.Save();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX01";
			controllingMessageHeader.PermitNumber = "110";
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code0;
			controllingMessageHeader.TW1_ControllingAgency = ControllingAgencyList.Codes.VP;
			controllingMessageHeader.TW1_ProcessingUnit = "XXXX4567";
			var invoiceLineLinkControllingMsgHeader = new InvoiceLineLinkControllingMsgHeader(controllingMessageHeader, invoiceLine);
			CombineAssertions(() =>
			{
				AssertEquals("VP", invoiceLineLinkControllingMsgHeader.ControllingAgency);
				AssertEquals("XX01", invoiceLineLinkControllingMsgHeader.FunctionalReferenceID);
				AssertEquals("110", invoiceLineLinkControllingMsgHeader.PermitNo);
				AssertEquals(false, invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader);
				AssertEquals(controllingMessageHeader.PK, invoiceLineLinkControllingMsgHeader.ControllingMessageHeaderPK);
				AssertEquals((ZShort)1, invoiceLineLinkControllingMsgHeader.Sequence);
				AssertEquals("NX401", invoiceLineLinkControllingMsgHeader.MessageType);
				AssertEquals("檢疫申辦訊息", invoiceLineLinkControllingMsgHeader.MessageTypeDescription);
				AssertEquals("VP DES.", invoiceLineLinkControllingMsgHeader.ControllingAgencyDescription);
				AssertEquals("00", invoiceLineLinkControllingMsgHeader.CertificateType);
				AssertEquals("其他原產地証明(進口國制定格式)", invoiceLineLinkControllingMsgHeader.CertificateTypeDescription);
				AssertEquals("XXXX4567", invoiceLineLinkControllingMsgHeader.ProcessingUnit);
				AssertEquals("ZZZZZZ", invoiceLineLinkControllingMsgHeader.ProcessingUnitDescription);
			});
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			controllingMessageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.QuarantineOfExportAnimal;
			CombineAssertions(() =>
			{
				AssertEquals("30", invoiceLineLinkControllingMsgHeader.BusinessType);
				AssertEquals("動物輸出檢疫申請", invoiceLineLinkControllingMsgHeader.BusinessTypeDescription);
			});
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			controllingMessageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.QuarantineOfImportAnimal;
			CombineAssertions(() =>
			{
				AssertEquals("40", invoiceLineLinkControllingMsgHeader.BusinessType);
				AssertEquals("動物輸入檢疫申請", invoiceLineLinkControllingMsgHeader.BusinessTypeDescription);
			});

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			controllingMessageHeader.TW1_FunctionalReferenceId = "XX02";
			controllingMessageHeader.PermitNumber = "220";
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = false;
			CombineAssertions(() =>
			{
				AssertEquals("FT", invoiceLineLinkControllingMsgHeader.ControllingAgency);
				AssertEquals("XX02", invoiceLineLinkControllingMsgHeader.FunctionalReferenceID);
				AssertEquals("220", invoiceLineLinkControllingMsgHeader.PermitNo);
				AssertEquals(false, invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader);
				AssertEquals(controllingMessageHeader.PK, invoiceLineLinkControllingMsgHeader.ControllingMessageHeaderPK);
				AssertEquals("X101", invoiceLineLinkControllingMsgHeader.MessageType);
				AssertEquals("產地證明申辦訊息", invoiceLineLinkControllingMsgHeader.MessageTypeDescription);
				AssertEquals("FT DES.", invoiceLineLinkControllingMsgHeader.ControllingAgencyDescription);
				AssertEquals("01", invoiceLineLinkControllingMsgHeader.CertificateType);
				AssertEquals("一般原產地證明", invoiceLineLinkControllingMsgHeader.CertificateTypeDescription);
			});

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			controllingMessageHeader.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.Inspection;
			CombineAssertions(() =>
			{
				AssertEquals("A", invoiceLineLinkControllingMsgHeader.BusinessType);
				AssertEquals("查驗申辦", invoiceLineLinkControllingMsgHeader.BusinessTypeDescription);
			});
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			controllingMessageHeader.TW1_ControllingAgency = ControllingAgencyList.Codes._20;
			controllingMessageHeader.TW1_ProcessingUnit = "XXXX0123";
			CombineAssertions(() =>
			{
				AssertEquals("XXXX0123", invoiceLineLinkControllingMsgHeader.ProcessingUnit);
				AssertEquals("XXXXXX", invoiceLineLinkControllingMsgHeader.ProcessingUnitDescription);
			});
		}

		public void TestIsNX101ContainZZZCertificateTypes()
		{
			var invoiceLineLinkControllingMsgHeader = new InvoiceLineLinkControllingMsgHeader(controllingMessageHeader, invoiceLine);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = true;
			CombineAssertions("Linked, NX101", () =>
			{
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code0, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code1, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code2, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code4, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code5, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code6, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code7, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code8, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code9, true, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code10, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code11, true, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code12, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code13, true, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code14, true, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code15, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code16, false, true);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code17, false, true);
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			CombineAssertions("Linked, Not NX101", () =>
			{
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code0, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code1, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code2, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code4, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code5, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code6, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code7, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code8, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code9, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code10, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code11, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code12, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code13, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code14, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code15, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code16, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code17, false, false);
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = false;
			CombineAssertions("Not Linked, NX101", () =>
			{
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code0, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code1, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code2, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code4, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code5, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code6, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code7, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code8, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code9, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code10, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code11, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code12, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code13, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code14, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code15, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code16, false, false);
				AssertIsNX101ContainZZZCertificateTypes(CertificateTypeList.Codes.Code17, false, false);
			});

			void AssertIsNX101ContainZZZCertificateTypes(string certificateType, bool expectedIsNX101ContainZZZCertificateTypes, bool expectedIsNX101NotContainZZZCertificateTypes)
			{
				controllingMessageHeader.TW1_CertificateType = certificateType;
				AssertEquals(string.Format("IsNX101ContainZZZCertificateTypes When Certificate Type is {0}", certificateType), expectedIsNX101ContainZZZCertificateTypes, invoiceLineLinkControllingMsgHeader.IsNX101ContainZZZCertificateTypes);
				AssertEquals(string.Format("IsNX101NotContainZZZCertificateTypes When Certificate Type is {0}", certificateType), expectedIsNX101NotContainZZZCertificateTypes, invoiceLineLinkControllingMsgHeader.IsNX101NotContainZZZCertificateTypes);
			}
		}

		public void TestDefaultValuesWhenLinked()
		{
			CombineAssertions(() =>
			{
				var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
				var entryInstruction = jobDeclartion.CusEntryInstruction;
				var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
				var line = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
				line.JI_CEI = entryInstruction.PK;
				var cmHeader = controllingMessageHeaders.AddNew();
				cmHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
				line.JI_InvoiceQuantity = 123m;
				line.JI_InvoiceUQ = "Unt";
				AssertEquals("JI_PermitQty Init condition", ZDecimal.Zero, line.JI_PermitQty);
				AssertEquals("JI_PermitUQ Init condition", ZString.Empty, line.JI_PermitUQ);
				AssertEquals("TW_CustomPermitUQ Init condition", ZString.Empty, line.JI_CustomPermitUQ);
				line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

				AssertEquals("JI_PermitQty", 123m, line.JI_PermitQty);
				AssertEquals("JI_PermitUQ", "Unt", line.JI_PermitUQ);
				AssertEquals("TW_CustomPermitUQ", "Unt", line.JI_CustomPermitUQ);
			});

			CombineAssertions(() =>
			{
				var pack = Factory.New<CusRefPacks>();
				pack.RP_CustomsPack = "UNT";
				pack.RP_Type = "PQU";
				pack.RP_CustomsCountry = "TW";
				pack.RP_ConversionFactor = 1;
				pack.RP_CommercialPack = "CMP";
				var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
				var entryInstruction = jobDeclartion.CusEntryInstruction;
				var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
				var line = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
				line.JI_CEI = entryInstruction.PK;
				var cmHeader = controllingMessageHeaders.AddNew();
				cmHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
				cmHeader.TW1_CertificateType = "15";
				line.JI_InvoiceQuantity = 123m;
				line.JI_InvoiceUQ = "Unt";
				line.JI_Tariff = "123456789";
				line.AddInfoChild.TWL_DocumentaryUnitPrice = 222m;
				line.JI_CustomPermitUQ = "EAC";
				AssertEquals("JI_PermitQty Init condition", ZDecimal.Zero, line.JI_PermitQty);
				AssertEquals("JI_PermitUQ Init condition", ZString.Empty, line.JI_PermitUQ);
				AssertEquals("TW_CustomPermitUQ Init condition", "EAC", line.JI_CustomPermitUQ);
				AssertEquals("TWL_DocumentaryUnitPrice Init condition", ZDecimal.Zero, line.JI_PermitUnitPrice);
				AssertEquals("JI_IMPTariff Init condition", ZString.Empty, line.JI_IMPTariff);
				line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

				AssertEquals("JI_PermitQty", 123m, line.JI_PermitQty);
				AssertEquals("TW_CustomPermitUQ", ZString.Empty, line.JI_CustomPermitUQ);
				AssertEquals("When TW1_CertificateType is 15", "CMP", line.JI_PermitUQ);
				AssertEquals("TWL_DocumentaryUnitPrice", 222m, line.JI_PermitUnitPrice);
				AssertEquals("JI_IMPTariff", "12345678", line.JI_IMPTariff);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testTWCreator = new TestTWCreator(Factory);
			testTWCreator.CreateRefCusCodeForControllingMessageType();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			controllingMessageHeader = controllingMessageHeaders.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusTWControllingMessageHeader controllingMessageHeader;
	}
}
