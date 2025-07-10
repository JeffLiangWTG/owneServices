using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsOutwardEntryNumberProviderTest : TestCaseWithFactory
	{
		public void TestGetEntryNumber()
		{
			IWarehouseCustomsOutwardEntryNumberProvider provider = new WarehouseCustomsOutwardEntryNumberProvider(null);
			AssertNull(provider.GetEntryNumber());
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			provider = new WarehouseCustomsOutwardEntryNumberProvider(shipment);
			AssertNull(provider.GetEntryNumber());
			shipment.SetEntryHeaderCollection(() => new List<EntryHeader>());
			AssertNull(provider.GetEntryNumber());
			var entryHeader = new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.EntryHeaderCollection.Add(entryHeader);
			AssertNull(provider.GetEntryNumber());
			entryHeader.EntryNumberCollection = new List<UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber>();
			AssertNull(provider.GetEntryNumber());
			var entryNumber = new UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber();
			entryHeader.EntryNumberCollection.Add(entryNumber);
			AssertNull(provider.GetEntryNumber());
			entryNumber.Number = "HELLO";
			AssertEquals("HELLO", provider.GetEntryNumber());

			shipment.SetEntryNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>());
			AssertEquals("HELLO", provider.GetEntryNumber());

			shipment.EntryNumberCollection.Add(new UniversalDataBuss.DataObjects.Universal.EntryNumber());
			shipment.EntryNumberCollection.Add(new UniversalDataBuss.DataObjects.Universal.EntryNumber() { Number = "HI", Type = new EntryType() { Code = Constants.EntryNumberPlaceHolderType }, CountryOfIssue = new Country() });
			shipment.EntryNumberCollection.Add(new UniversalDataBuss.DataObjects.Universal.EntryNumber() { Number = "BYE", Type = new EntryType() { Code = Constants.EntryNumberPlaceHolderType } });
			shipment.EntryNumberCollection.Add(new UniversalDataBuss.DataObjects.Universal.EntryNumber() { Number = "YO", Type = new EntryType() { Code = Constants.EntryNumberPlaceHolderType }, CountryOfIssue = new Country() { Code = "ZA" } });
			AssertEquals("BYE", provider.GetEntryNumber());
		}
	}
}
