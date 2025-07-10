using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryLineDocWrapper))]
	sealed class EntryLineDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEntryLineDocWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4201.00.60 00";
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_Weight = 2;
			invoiceLine.US_ManifestQty = 10;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_PrivilegedStatusDate = ZDateTime.Today;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryLine entryLine = invoiceLine.CusEntryLine.ParentLine;
			var entryLineWrapper = new EntryLineDocWrapper(entryLine);
			AssertEquals("X", entryLineWrapper.IsCommercial);
			AssertEquals("COMMERCIAL DESCRIPTION", entryLineWrapper.Description);
			AssertEquals("9802008068", entryLineWrapper.Tariff1);
			AssertEquals("4201006000", entryLineWrapper.Tariff2);
			AssertEquals(2300m, entryLineWrapper.EntryLineValue1);
			AssertEquals("20000", entryLineWrapper.EntryLineValue2);
			AssertEquals("10 PK", entryLineWrapper.LineItemQuantity);
			AssertEquals(ZDateTime.Today, entryLineWrapper.FTZFilingDate);
			AssertEquals("X", entryLineWrapper.IsZoneStatusP);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var entryLine = Factory.New<CusEntryLine>();
			return new EntryLineDocWrapper(entryLine);
		}
	}
}
