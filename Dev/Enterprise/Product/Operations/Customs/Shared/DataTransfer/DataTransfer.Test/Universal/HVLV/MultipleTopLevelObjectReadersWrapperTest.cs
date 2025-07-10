using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class MultipleTopLevelObjectReadersWrapperTest : TestCaseWithFactory
	{
		public void TestGetExistingBusinessObjects()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			shipment.WayBillNumber = "125-83833890";
			shipment.VoyageFlightNo = "BA091";
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var hlv1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SubShipmentCollection.Add(hlv1);
			hlv1.WayBillNumber = "S00046519";
			hlv1.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			hlv1.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };
			hlv1.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var reader = MultipleTopLevelObjectReadersWrapper.CreateWrapper(
				shipment,
				true,
				hls =>
				new[]
				{
					new DummyTopLevelDataObjectReader(Factory),
					new DummyTopLevelDataObjectReader(Factory)
				}) as MultipleTopLevelObjectReadersWrapper;

			AssertEquals("2 Dummies found", 2, reader.GetExistingBusinessObjects().Count());
		}

		public void TestPopulatedBusinessObjects()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			shipment.WayBillNumber = "125-83833890";
			shipment.VoyageFlightNo = "BA091";
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var coload = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SubShipmentCollection.Add(coload);
			coload.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.CoLoadMaster };
			coload.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			coload.WayBillNumber = "COLOAD";
			coload.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var hlv1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			coload.SubShipmentCollection.Add(hlv1);
			hlv1.WayBillNumber = "S00046519";
			hlv1.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			hlv1.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };
			hlv1.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var hvlv1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			hlv1.SubShipmentCollection.Add(hvlv1);
			hvlv1.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			hvlv1.WayBillNumber = "BA00001";

			var hvlv2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			hlv1.SubShipmentCollection.Add(hvlv2);
			hvlv2.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			hvlv2.WayBillNumber = "BA00002";
			var reader = MultipleTopLevelObjectReadersWrapper.CreateWrapper(
				shipment,
				true,
				hls =>
				new[]
				{
					new DummyTopLevelDataObjectReader(Factory),
					new DummyTopLevelDataObjectReader(Factory)
				}) as MultipleTopLevelObjectReadersWrapper;
			reader.ReadIntoTopLevelBusinessObject();

			AssertEquals("2 Dummies created", 2, reader.PopulatedBusinessObjects.Count());
		}

		public void TestCreateWrapperReturnNull()
		{
			AssertNull(MultipleTopLevelObjectReadersWrapper.CreateWrapper(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), false, hls => Array.Empty<ITopLevelDataObjectReader>()));
		}

		public void TestCreateWrapperReturnOneReader()
		{
			AssertType<DummyTopLevelDataObjectReader>(MultipleTopLevelObjectReadersWrapper.CreateWrapper(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), false, hls => new[] { new DummyTopLevelDataObjectReader() }));
		}

		public void TestCreateWrapperWithMultipleReaders()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			shipment.WayBillNumber = "125-83833890";
			shipment.VoyageFlightNo = "BA091";
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var coload = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SubShipmentCollection.Add(coload);
			coload.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.CoLoadMaster };
			coload.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			coload.WayBillNumber = "COLOAD";
			coload.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var hls1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			coload.SubShipmentCollection.Add(hls1);
			hls1.WayBillNumber = "S00046519";
			hls1.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			hls1.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy };
			hls1.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var hvlv1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			hls1.SubShipmentCollection.Add(hvlv1);
			hvlv1.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			hvlv1.WayBillNumber = "BA00001";

			var hvlv2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			hls1.SubShipmentCollection.Add(hvlv2);
			hvlv2.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			hvlv2.WayBillNumber = "BA00002";

			var hls2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			coload.SubShipmentCollection.Add(hls2);
			hls2.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			hls2.WayBillNumber = "S00046520";
			hls2.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy };
			hls2.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var hvlv3 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			hls2.SubShipmentCollection.Add(hvlv3);
			hvlv3.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			hvlv3.WayBillNumber = "BA00001";

			var hvlv4 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			hls2.SubShipmentCollection.Add(hvlv4);
			hvlv4.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			hvlv4.WayBillNumber = "BA00002";

			var reader = MultipleTopLevelObjectReadersWrapper.CreateWrapper(shipment, true, hls => new[] { new DummyTopLevelDataObjectReader(), new DummyTopLevelDataObjectReader() });
			AssertType<MultipleTopLevelObjectReadersWrapper>(reader);
			AssertEquals(4, ((MultipleTopLevelObjectReadersWrapper)reader).readers.Count());
		}

		sealed class DummyTopLevelDataObjectReader : ITopLevelDataObjectReader
		{
			public DummyTopLevelDataObjectReader() { }

			public DummyTopLevelDataObjectReader(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			readonly BusinessObjectFactory factory;

			public BusinessObject GetExistingBusinessObject() => factory.New<DummyBusinessObject>();

			public void ReadIntoBusinessObject(ref BusinessObject targetBO) => targetBO = factory.New<DummyBusinessObject>();

			public BusinessObject ReadIntoTopLevelBusinessObject() => throw new NotImplementedException();

			public IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelism()
			{
				yield break;
			}
		}
	}
}
