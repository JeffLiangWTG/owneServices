using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(LinkedEntry))]
	sealed class LinkedEntryTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<LinkedEntry>
	{
		public void TestSetingEntryNumberSetsEntryHeader()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_SchDEntry = "3902";
			declaration.US_EntryDate = ZDateTime.Today.AddDays(-1);
			var liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_SystemCreateDate = new ZDateTime(2013, 03, 19);
			liquidation.B8_LiquidationDate = new ZDateTime(2013, 03, 20);
			declaration.Liquidations.Add(liquidation);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var linkedEntry = Factory.New<LinkedEntry>();
			AssertEquals("[PRE-CONDITION] US_LE_CH_EntryHeader", ZGuid.Empty, linkedEntry.US_LE_CH_EntryHeader);
			AssertEquals("[PRE-CONDITION] US_LE_PortCode", ZString.Empty, linkedEntry.US_LE_PortCode);
			AssertEquals("[PRE-CONDITION] US_LE_EntryDate", ZDateTime.Empty, linkedEntry.US_LE_EntryDate);
			AssertEquals("[PRE-CONDITION] US_LE_LiquidationDate", ZDateTime.Empty, linkedEntry.US_LE_LiquidationDate);
			linkedEntry.US_LE_EntryNumber = "XJ5" + entryHeader.EntryNumber;
			AssertEquals("US_LE_CH_EntryHeader", entryHeader.PK, linkedEntry.US_LE_CH_EntryHeader);
			AssertEquals("US_LE_PortCode", "3902", linkedEntry.US_LE_PortCode);
			AssertEquals("US_LE_EntryDate", ZDateTime.Today.AddDays(-1), linkedEntry.US_LE_EntryDate);
			AssertEquals("US_LE_LiquidationDate", new ZDateTime(2013, 03, 20), linkedEntry.US_LE_LiquidationDate);
			linkedEntry.US_LE_EntryNumber = "~1111";
			AssertEquals("US_LE_CH_EntryHeader", ZGuid.Empty, linkedEntry.US_LE_CH_EntryHeader);
			AssertEquals("US_LE_PortCode", ZString.Empty, linkedEntry.US_LE_PortCode);
			AssertEquals("US_LE_EntryDate", ZDateTime.Empty, linkedEntry.US_LE_EntryDate);
			AssertEquals("US_LE_LiquidationDate", ZDateTime.Empty, linkedEntry.US_LE_LiquidationDate);
		}

		public void TestSetDefaultValues()
		{
			var entry = Factory.New<LinkedEntry>();
			AssertEquals(CusAddInfoTypeAttribute.Codes.USLinkedEntry, entry.B7_Type);
		}

		public void TestLookupsAndValidation()
		{
			var entry = Factory.New<LinkedEntry>();
			AssertEquals("LinkedEntry Lookups", typeof(CusAddInfoLookups), entry.Lookups.GetType());
			AssertEquals("LinkedEntry Validation", typeof(CusAddInfoValidation), entry.Validation.GetType());
			AssertEquals("LinkedEntry AddInfoLookups", typeof(USLinkedEntryAddInfoLookups), entry.AddInfoLookups.GetType());
			AssertEquals("LinkedEntry AddInfoValidation", typeof(USLinkedEntryAddInfoValidation), entry.AddInfoValidation.GetType());
		}

		public void TestPropertiesForDocumentPrinting()
		{
			var protest = new Protest.Protest(Factory.New<JobDeclaration>());
			var entry = protest.LinkedEntries.AddNew();
			entry.US_LE_EntryNumber = "XJ5123456";
			AssertEquals(ZString.Empty, entry.EntryFilerCode);
			AssertEquals(ZString.Empty, entry.EntryNumber);
			AssertEquals(ZString.Empty, entry.EntryNumberCheckDigit);
			entry.US_LE_EntryNumber = "12500000012";
			AssertEquals("125", entry.EntryFilerCode);
			AssertEquals("0000001", entry.EntryNumber);
			AssertEquals("2", entry.EntryNumberCheckDigit);
			entry.US_LE_EntryNumber = "XJ500000012";
			AssertEquals("XJ5", entry.EntryFilerCode);
			AssertEquals("0000001", entry.EntryNumber);
			AssertEquals("2", entry.EntryNumberCheckDigit);
		}

		protected override IEnumerable<LinkedEntry> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var protest = new Protest.Protest(factory.New<JobDeclaration>());
			var entry = protest.LinkedEntries.AddNew();
			entry.US_LE_EntryNumber = "XJ5123456";
			yield return entry;
		}
	}
}
