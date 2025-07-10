using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ZAAutoSendCustomsMessageProcessor))]
	sealed class ZAAutoSendCustomsMessageProcessorTest : Customs.Business.Testing.AutoSendCustomsMessageProcessorTest
	{
		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration)
		{
			var dec = (JobDeclaration)declaration;
			return dec.GetEntryDeclarationMessageProcessor(dec);
		}

		protected override Customs.Business.CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration)
		{
			return declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>().FirstOrDefault();
		}

		protected override void SetEntryClearedStatus(Customs.Business.CusEntryHeader entry)
		{
			entry.EntryNumber = "123";
			entry.CH_EntryStatus = "1";
		}

		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			var dec = (JobDeclaration)declaration;
			dec.JE_MessageType = "IMP";
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_ValuationDate = ZDate.Today;
			var inst = dec.CustomsEntryInstructions.AddNew();
			inst.CEI_Style = "11";
			inst = dec.CustomsEntryInstructions.AddNew();
			inst.CEI_Style = "14";
		}

		protected override void PrepareInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			var line = (JobComInvoiceLine)invoiceLine;
			var dec = line.Declaration;
			line.JI_CEI = dec.CustomsEntryInstructions[0].PK;
			line.JI_Procedure = "1100";
			var invoice = invoiceLine.InvoiceHeader;
			invoice.JZ_InvoiceDate = ZDate.Today;
			invoice.JZ_RX_NKInvoice_Currency = "ZAR";
			line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			line.JI_CEI = dec.CustomsEntryInstructions[1].PK;
			line.JI_Procedure = "1400";
		}

		protected override void AssertEntryAndMessageResultForEndToEndTest(Customs.Business.CusEntryHeader entry)
		{
			CheckEntryHeader(entry, "11");
			var declaration = entry.Declaration;
			AssertEquals("Two customs entry headers are expected.", 2, declaration.CustomsEntryHeaders.Count);
			var entry02 = declaration.CustomsEntryHeaders[1];
			CheckEntryHeader(entry02, "14");
		}

		protected override ZString ExpectedMessageDescription => "South African Customs Declaration";

		void CheckEntryHeader(Customs.Business.CusEntryHeader entry, string expected_CPC)
		{
			CombineAssertions(() =>
			{
				AssertEquals(expected_CPC, entry.EntryInstruction.CEI_Style);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals("CH_Status", "AWA", entry.CH_Status);
				var ediMessage = entry.Messages[0];
				AssertEquals("EM_MessageType", "DEC", ediMessage.EM_MessageType);
				AssertEquals(MessageSubTypeCodes.Codes.Original, ediMessage.EM_MessageSubType);
				AssertEquals("EM_Status", "QUE", ediMessage.EM_Status);
				AssertNotNull("EM_HeldUntilDate must be specified.", ediMessage.EM_HeldUntilDate);
				Assert("EM_HeldUntilDate must be in the future.", ediMessage.EM_SystemCreateTimeUtc.CompareTo(ediMessage.EM_HeldUntilDate) < 0);
			});
		}
	}
}
