using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(TRETradeHVLVAsycudaBillDataObjectReader))]
	sealed class TRETradeAsycudaBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestReadIntoBusinessObject()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_RL_NKPortOfFirstArrival = string.Empty;

			var hvlvConsignmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvConsignmentDataObject.DataContext = DataContextFactory.New();
			hvlvConsignmentDataObject.DataContext.AddDataSource(new DataSource { Type = nameof(DataContextType.HVLVConsignment), Key = "Key" });

			hvlvConsignmentDataObject.ManifestedWeight = 10000;
			hvlvConsignmentDataObject.TotalWeightUnit = new UnitOfWeight { Code = "KG" };

			var hvlvItemDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvItemDataObject.PackType = new PackageType() { Code = "BT" };

			hvlvConsignmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { hvlvItemDataObject });

			var dummyRegistrationNumber = new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = "DMY" },
				Value = "999"
			};

			var consignorAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			consignorAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { dummyRegistrationNumber, new RegistrationNumber { Type = new RegistrationNumberType { Code = "VAT" }, Value = "123" } });
			consignorAddress.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

			var consigneeAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			consigneeAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { dummyRegistrationNumber, new RegistrationNumber { Type = new RegistrationNumberType { Code = "VAT" }, Value = "456" } });
			consigneeAddress.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			hvlvConsignmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { consignorAddress, consigneeAddress });

			var bill = new TRETradeHVLVAsycudaBillDataObjectReader(hvlvConsignmentDataObject, logger, Factory, manifestHeader, new TRETradeHVLVAsycudaManifestDataObjectReaderHelper("TR", Factory.BOFactory), isUpdateEnabled: true).ReadIntoBusinessObject() as AsycudaBill;

			CombineAssertions("Bill read correctly", () =>
			{
				AssertEquals("Quantity (on Bill) UQ", "BT", bill.ABL_ManifestUQ);
				AssertEquals("Net Weight", 10000m, bill.ABL_NetWeight);
				AssertEquals("Net Weight UQ", "KG", bill.ABL_NetWeightUQ);
				AssertEquals("Shipper Reg No", "123", bill.ABL_ShipperRegNo);
				AssertEquals("Consignee Reg No", "456", bill.ABL_ConsigneeRegNo);
				AssertEquals("Arrival Country", "TR", bill.ArrivalCountry);
			});
		}

		public void TestReadIntoBusinessObject_GrossWeightUQFallback()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var hvlvConsignmentDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvConsignmentDataObject.DataContext = DataContextFactory.New();
			hvlvConsignmentDataObject.DataContext.AddDataSource(new DataSource { Type = nameof(DataContextType.HVLVConsignment), Key = "Key" });

			var packingLine = new PackingLine();
			packingLine.PackType = new PackageType { Code = ZString.Empty };
			hvlvConsignmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packingLine });

			var bill = new TRETradeHVLVAsycudaBillDataObjectReader(hvlvConsignmentDataObject, logger, Factory, manifestHeader, new TRETradeHVLVAsycudaManifestDataObjectReaderHelper("TR", Factory.BOFactory), isUpdateEnabled: true).ReadIntoBusinessObject() as AsycudaBill;

			AssertEquals("Quantity (on Bill) UQ should fallback to BI when pack type is not provided.", "BI", bill.ABL_ManifestUQ);
		}
	}
}
