using System;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CUSDECEDIMessageDocumentSupporter))]
	sealed class CUSDECEDIMessageDocumentSupporterForSADTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessageReturnOneReason()
		{
			var message = Factory.New<CUSDECEDIMessage>();
			var supporter = message.DocumentSupporter;
			AssertEquals(false, supporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
			AssertEquals("The selected message is not linked to a proper Entry Header or the selected message body is not valid CUSDEC message.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack), null));
			AssertEquals("The selected message is not linked to a proper Entry Header or the selected message body is not valid CUSDEC message.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack), null));
			AssertEquals("The selected message is not linked to a proper Entry Header or the selected message body is not valid CUSDEC message.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCRefundDocument), null));
			AssertEquals("", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair), null));
		}

		public void TestGetBODocDataProviders()
		{
			var message = Factory.New<CUSDECEDIMessage>();
			var providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair), null);
			AssertNull("Provider for CUSDECCUSRESMessagePair", providers);

			providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack), null);
			AssertEquals("Provider for SADDocumentPack", 1, providers.Length);

			providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack), null);
			AssertEquals("Provider for VOCDocumentPack", 1, providers.Length);

			providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCRefundDocument), null);
			AssertEquals("Provider for VOCRefundDocument", 1, providers.Length);
		}

		public void TestCustomWatermarkForSADDocumentPack()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			var collection = new SADDocumentWatermarkCollection();
			var watermark = collection.AddNew();
			watermark.EntryStatusCode = "1";
			ZACustomsRegistry.Instance.SADDocumentPackWatermarks.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, collection);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "1";
			var message = Factory.New<CUSDECEDIMessage>();
			message.EM_MessageSubType = "ORG";
			message.EM_MessageText = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			entryHeader.Messages.Add(message);
			var supporter = message.DocumentSupporter;

			var context = new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack);
			var docCommand = Factory.New<DocumentCommand>();
			docCommand.SU_MenuName = "Customs Declaration Response";
			AssertNull("Custom Watermark should be null", supporter.GetCustomWatermarkText(docCommand, supporter.GetBODocDataProviders(context, docCommand)[0]));
			docCommand.SU_MenuName = "SAD Document Pack";
			AssertEquals("Custom Watermark should be set", "Release", supporter.GetCustomWatermarkText(docCommand, supporter.GetBODocDataProviders(context, docCommand)[0]));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var testHeader = testDeclaration.ActiveEntryHeaders.AddNew();
			var message = Factory.New<CUSDECEDIMessage>();
			message.EM_MessageSubType = "ORG";
			message.EM_MessageText = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			testHeader.Messages.Add(message);
			return message;
		}
	}

	[TestedType(typeof(CUSDECEDIMessageDocumentSupporter))]
	sealed class CUSDECEDIMessageDocumentSupporterForVOCTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessageReturnOneReason()
		{
			var message = Factory.New<CUSDECEDIMessage>();
			var supporter = message.DocumentSupporter;
			AssertEquals(false, supporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
			AssertEquals("The selected message is not linked to a proper Entry Header or the selected message body is not valid CUSDEC message.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack), null));
			AssertEquals("The selected message is not linked to a proper Entry Header or the selected message body is not valid CUSDEC message.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack), null));
			AssertEquals("The selected message is not linked to a proper Entry Header or the selected message body is not valid CUSDEC message.", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCRefundDocument), null));
			AssertEquals("", supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair), null));
		}

		public void TestGetBODocDataProviders()
		{
			var message = Factory.New<CUSDECEDIMessage>();
			var providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair), null);
			AssertNull("Provider for CUSDECCUSRESMessagePair", providers);

			providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack), null);
			AssertEquals("Provider for SADDocumentPack", 1, providers.Length);

			providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack), null);
			AssertEquals("Provider for VOCDocumentPack", 1, providers.Length);

			providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCRefundDocument), null);
			AssertEquals("Provider for VOCRefundDocument", 1, providers.Length);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var testHeader = testDeclaration.ActiveEntryHeaders.AddNew();
			var message = Factory.New<CUSDECEDIMessage>();
			message.EM_MessageSubType = "ORG";
			message.EM_MessageText = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			testHeader.Messages.Add(message);
			return message;
		}
	}
}
