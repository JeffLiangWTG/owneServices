using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
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
			shipment.SetAddInfoCollection(() => new List<UniversalDataBuss.DataObjects.Universal.AddInfo>());
			AssertNull(provider.GetEntryNumber());
			var addInfo = new UniversalDataBuss.DataObjects.Universal.AddInfo() { Value = "XJ5" };
			shipment.AddInfoCollection.Add(addInfo);
			AssertNull(provider.GetEntryNumber());
			shipment.SetEntryNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>());
			AssertNull(provider.GetEntryNumber());
			var entryNumber = new UniversalDataBuss.DataObjects.Universal.EntryNumber() { Number = "ENT32423" };
			shipment.EntryNumberCollection.Add(entryNumber);
			AssertNull(provider.GetEntryNumber());
			entryNumber.Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary };
			AssertNull(provider.GetEntryNumber());
			addInfo.Key = JobDeclaration.Schema.US_EntryFilerCode.Substring(3);
			AssertEquals("XJ5-ENT32423", provider.GetEntryNumber());
			entryNumber.Number = "";
			AssertNull(provider.GetEntryNumber());
			entryNumber.Number = "ENT489";
			AssertEquals("XJ5-ENT489", provider.GetEntryNumber());
			addInfo.Value = "";
			AssertNull(provider.GetEntryNumber());
		}
	}
}
