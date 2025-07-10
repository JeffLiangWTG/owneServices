#if DEBUG

namespace Enterprise.Freight.Agency.Business.Testing
{
	using System;
	using System.Collections.Generic;
	using Moq;

	internal abstract class IFCSUMMessageBuilderTest<T> : BaseAgencyTest
		where T : IPortAuthorityMessageBuilder, new()
	{
		public void TestReproduceSimpleMessage()
		{
			AssertSimpleMessage("MYPKG", "AUFRE", SimpleMessage);
			AssertSimpleMessage("AUPKL", "AUFRE", SimpleMessage_Load_AUPKL);
			AssertSimpleMessage("AUFRE", "AUPKL", SimpleMessage_Discharge_AUPKL);
		}

		void AssertSimpleMessage(string portOfLoading, string portOfDischarge, string message)
		{
			var equipment = new IPortAuthorityEquipmentData[]
			{
				MockImportContainer("ABC5014875", "2200", false),
				MockImportContainer("ABC5014877", "2200", true)
			};

			var goodsMock1 = new Mock<IPortAuthorityGoodsData>(MockBehavior.Strict);
			goodsMock1.Setup(m => m.ItemNumber).Returns(1);
			goodsMock1.Setup(m => m.PackageCount).Returns(1300);
			goodsMock1.Setup(m => m.PackageCode).Returns("PK");
			goodsMock1.Setup(m => m.GoodsDescription).Returns("030613\nFROZEN PRAWNS - 13,000 CTNS - FREIGHT COLLECT");
			goodsMock1.Setup(m => m.Kilograms).Returns(13000m);
			goodsMock1.Setup(m => m.CubicMetres).Returns(24.8m);
			goodsMock1.Setup(m => m.MarksAndNumbers).Returns("NO MARKS");
			goodsMock1.Setup(m => m.HarmonisedCode).Returns("");
			goodsMock1.Setup(m => m.ContainerNumbers).Returns(new string[] { "ABC5014875" });
			goodsMock1.Setup(m => m.ContainerYardAddress).Returns("POSTCODE-1111\nECP-AU CONTAINER YARD\nCHAPEL STREET\nBuilding XX\nMARRICKVILLE 2204");

			var goodsMock2 = new Mock<IPortAuthorityGoodsData>(MockBehavior.Strict);
			goodsMock2.Setup(m => m.ItemNumber).Returns(2);
			goodsMock2.Setup(m => m.PackageCount).Returns(1300);
			goodsMock2.Setup(m => m.PackageCode).Returns("PK");
			goodsMock2.Setup(m => m.GoodsDescription).Returns("EMPTY CONTAINER");
			goodsMock2.Setup(m => m.Kilograms).Returns(2200m);
			goodsMock2.Setup(m => m.CubicMetres).Returns(2200m);
			goodsMock2.Setup(m => m.MarksAndNumbers).Returns("NO MARKS");
			goodsMock2.Setup(m => m.HarmonisedCode).Returns("");
			goodsMock2.Setup(m => m.ContainerNumbers).Returns(new string[] { "ABC5014877" });
			goodsMock2.Setup(m => m.ContainerYardAddress).Returns("POSTCODE-1111\nECP-MARITIME CONTAINER YARD\nCANAL ROAD\nBuilding YY\nST PETERS 2204");

			var consignmentMock = new Mock<IPortAuthorityConsignmentData>(MockBehavior.Strict);
			consignmentMock.Setup(m => m.ConsignmentNumber).Returns(1);
			consignmentMock.Setup(m => m.CountryOfOrigin).Returns("MYPEN");
			consignmentMock.Setup(m => m.PortOfOrigin).Returns("");
			consignmentMock.Setup(m => m.PortOfLoading).Returns(portOfLoading);
			consignmentMock.Setup(m => m.PortOfDischarge).Returns(portOfDischarge);
			consignmentMock.Setup(m => m.PortOfDestination).Returns("AUFRE");
			consignmentMock.Setup(m => m.CountryOfDestination).Returns("");
			consignmentMock.Setup(m => m.BillOfLading).Returns("ABCPEN0002115");
			consignmentMock.Setup(m => m.IsWaybill).Returns(false);
			consignmentMock.Setup(m => m.PackingMode).Returns("FCL");
			consignmentMock.Setup(m => m.ConsignorNameAndAddress).Returns("");
			consignmentMock.Setup(m => m.ConsigneeNameAndAddress).Returns("ABC FOODS LTD\nPO BOX 21\nCLIFF STREET WA 6160\nAUSTRALIA");
			consignmentMock.Setup(m => m.Goods).Returns(new IPortAuthorityGoodsData[] { goodsMock1.Object, goodsMock2.Object });

			var messageMock = new Mock<IPortAuthorityMessagingData>(MockBehavior.Strict);
			messageMock.Setup(m => m.MessageFunction).Returns(PortAuthorityMessageFunction.Replace);
			messageMock.Setup(m => m.MessagePrepared).Returns(new DateTime(1997, 06, 30, 11, 03, 31));
			messageMock.Setup(m => m.Voyage).Returns("03S");
			messageMock.Setup(m => m.VesselLloyds).Returns("8506098");
			messageMock.Setup(m => m.VesselName).Returns("AUSTRALIAN ENTERPRISE");
			messageMock.Setup(m => m.Load).Returns(portOfLoading);
			messageMock.Setup(m => m.Discharge).Returns(portOfDischarge);
			messageMock.Setup(m => m.Equipment).Returns(equipment);
			messageMock.Setup(m => m.Consignments).Returns(new IPortAuthorityConsignmentData[] { consignmentMock.Object });

			AssertMessageEquals("", message, Builder.GenerateMessageText(messageMock.Object));

			goodsMock1.VerifyAll();
			goodsMock2.VerifyAll();
			consignmentMock.VerifyAll();
			messageMock.VerifyAll();
		}

		public void TestReproduceBulkMessage()
		{
			const int consignmentCount = 9;

			var consignments = new List<IPortAuthorityConsignmentData>();

			var packageCount = new int[consignmentCount] { 1, 1, 2, 2, 3, 1, 23, 6, 2 };
			var isWaybill = new bool[consignmentCount] { false, false, false, false, false, true, false, true, false };
			var destinationPort = new string[consignmentCount] { "AUSYD", "AUSYD", "AUSYD", "AUSYD", "AUSYD", "AUSYD", "AUFRE", "AUSYD", "AUSYD" };
			var originPort = new string[consignmentCount] { "USLGB", "USLGB", "USLGB", "CANWE", "USLGB", "CANWE", "USBCB", "USBCB", "CANWE" };
			var kilograms = new decimal[consignmentCount] { 3500m, 3126m, 5382m, 4559m, 15196m, 34069m, 23323m, 69917m, 4088m };
			var cubicMetres = new decimal[consignmentCount] { 29.854m, 41.203m, 125.004m, 7.169m, 164.143m, 183.209m, 40.441m, 458.347m, 6.428m };
			var harmonisedCodes = new string[consignmentCount] { "", "100", "200", "", "", "", "", "", "" };

			var mocksGoods = new List<Mock<IPortAuthorityGoodsData>>();
			var mocksConsignment = new List<Mock<IPortAuthorityConsignmentData>>();
			for (int i = 0; i < consignmentCount; i++)
			{
				var goodsMock = new Mock<IPortAuthorityGoodsData>(MockBehavior.Strict);
				mocksGoods.Add(goodsMock);
				goodsMock.Setup(m => m.ItemNumber).Returns(1);
				goodsMock.Setup(m => m.PackageCount).Returns(packageCount[i]);
				goodsMock.Setup(m => m.PackageCode).Returns("PK");
				goodsMock.Setup(m => m.GoodsDescription).Returns(i == 0 ? "" : "DESCRIPTION OF ITEM");
				goodsMock.Setup(m => m.MarksAndNumbers).Returns(i == 4 ? "NO MARKS" : "MARKS AND NUMBERS");
				goodsMock.Setup(m => m.Kilograms).Returns(kilograms[i]);
				goodsMock.Setup(m => m.CubicMetres).Returns(cubicMetres[i]);
				goodsMock.Setup(m => m.HarmonisedCode).Returns(harmonisedCodes[i]);
				goodsMock.Setup(m => m.ContainerYardAddress).Returns("");

				var consignmentMock = new Mock<IPortAuthorityConsignmentData>(MockBehavior.Strict);
				mocksConsignment.Add(consignmentMock);
				consignmentMock.Setup(m => m.ConsignmentNumber).Returns(i + 1);
				consignmentMock.Setup(m => m.CountryOfOrigin).Returns(originPort[i]);
				consignmentMock.Setup(m => m.PortOfOrigin).Returns("");
				consignmentMock.Setup(m => m.PortOfLoading).Returns(originPort[i]);
				consignmentMock.Setup(m => m.PortOfDischarge).Returns("");
				consignmentMock.Setup(m => m.PortOfDestination).Returns(destinationPort[i]);
				consignmentMock.Setup(m => m.CountryOfDestination).Returns("");
				consignmentMock.Setup(m => m.BillOfLading).Returns("UNIQUE BILL OF LADING REFERENCE");
				consignmentMock.Setup(m => m.IsWaybill).Returns(isWaybill[i]);
				consignmentMock.Setup(m => m.PackingMode).Returns("LCL");
				consignmentMock.Setup(m => m.ConsignorNameAndAddress).Returns("");
				consignmentMock.Setup(m => m.ConsigneeNameAndAddress).Returns("CONSIGNEE NAME AND ADDRESS");
				consignmentMock.Setup(m => m.Goods).Returns(new IPortAuthorityGoodsData[] { goodsMock.Object });

				consignments.Add(consignmentMock.Object);
			}

			var messageMock = new Mock<IPortAuthorityMessagingData>(MockBehavior.Strict);
			messageMock.Setup(m => m.MessageFunction).Returns(PortAuthorityMessageFunction.Original);
			messageMock.Setup(m => m.MessagePrepared).Returns(new DateTime(2000, 01, 04, 16, 07, 29));
			messageMock.Setup(m => m.Voyage).Returns("777");
			messageMock.Setup(m => m.VesselLloyds).Returns("9999999");
			messageMock.Setup(m => m.VesselName).Returns("VESSEL NAME");
			messageMock.Setup(m => m.Load).Returns("");
			messageMock.Setup(m => m.Discharge).Returns("AUSYD");
			messageMock.Setup(m => m.Equipment).Returns(Array.Empty<IPortAuthorityEquipmentData>());
			messageMock.Setup(m => m.Consignments).Returns(consignments.ToArray());

			AssertMessageEquals("", BulkMessage, Builder.GenerateMessageText(messageMock.Object));

			mocksGoods.ForEach(x => x.VerifyAll());
			mocksConsignment.ForEach(x => x.VerifyAll());
			messageMock.VerifyAll();
		}

		public void TestReproduceContMessage()
		{
			var equipment = new List<IPortAuthorityEquipmentData>();
			equipment.Add(MockExportContainer("HJCU6200729", "2032"));
			equipment.Add(MockExportContainer("HJCU6206244", "2032"));
			equipment.Add(MockExportContainer("HJCU6212186", "2032"));
			equipment.Add(MockExportContainer("HJCU6215647", "2032"));
			equipment.Add(MockExportContainer("HJCU7095269", "42R0"));
			equipment.Add(MockExportContainer("HJCU7341450", "42R0"));
			equipment.Add(MockExportContainer("HJCU7389875", "42R0"));
			equipment.Add(MockExportContainer("HJCU7614301", "42R0"));
			equipment.Add(MockExportContainer("HJCU7640785", "42R0"));
			equipment.Add(MockExportContainer("HJCU7910094", "42R0"));
			equipment.Add(MockExportContainer("HJCU8513764", "22G0"));
			equipment.Add(MockExportContainer("HJCU8554845", "22G0"));
			equipment.Add(MockExportContainer("HJCU8559723", "22G0"));
			equipment.Add(MockExportContainer("HJCU8564252", "22G0"));
			equipment.Add(MockExportContainer("HJCU8573866", "22G0"));
			equipment.Add(MockExportContainer("HJCU8614477", "22G0"));
			equipment.Add(MockExportContainer("HJCU8629693", "22G0"));
			equipment.Add(MockExportContainer("HJCU8633163", "22G0"));
			equipment.Add(MockExportContainer("HJCU8720587", "22G0"));
			equipment.Add(MockExportContainer("HJCU8745230", "22G0"));
			equipment.Add(MockExportContainer("HJCU8754057", "22G0"));
			equipment.Add(MockExportContainer("HJCU8780631", "22G0"));
			equipment.Add(MockExportContainer("KSCU2137763", "22G0"));
			equipment.Add(MockExportContainer("KSCU2140279", "22G0"));
			equipment.Add(MockExportContainer("TEXU4747565", "42R0"));

			const int consignmentCount = 12;

			int[] packageCount = new int[consignmentCount] { 34, 927, 134, 51, 1, 54, 1469, 26, 12, 1000, 0, 1109 };
			decimal[] kilograms = new decimal[consignmentCount] { 16282m, 19004m, 34780m, 87095m, 14840m, 215548m, 14156m, 12325m, 25701m, 13500m, 0m, 19504m };
			decimal[] cubicMetres = new decimal[consignmentCount] { 63.71m, 0m, 0m, 132.628m, 0m, 0m, 0m, 20.33m, 0m, 26m, 0m, 22.744m };
			string[] destinationPort = new string[consignmentCount] { "IDJKT", "NLRTM", "KRKAN", "KRINC", "GBFXT", "AEDXB", "DEHAM", "AEDXB", "KRINC", "SGSIN", "PHMNL", "PHMNL" };

			string[][] containerNumbers = new string[consignmentCount][]
			{
				new string[] { "HJCU8754057" },
				new string[] { "HJCU6200729" },
				new string[] { "HJCU7389875", "HJCU7640785" },
				new string[] { "HJCU7095269", "HJCU7341450", "HJCU7614301", "TEXU4747565" },
				new string[] { "HJCU8720587" },
				new string[] { "HJCU8513764", "HJCU8554845", "HJCU8614477", "HJCU8629693", "HJCU8633163", "HJCU8745230", "HJCU8780631", "KSCU2137763", "KSCU2140279" },
				new string[] { "HJCU6206244" },
				new string[] { "HJCU8559723" },
				new string[] { "HJCU7910094" },
				new string[] { "HJCU6212186" },
				new string[] { "HJCU8564252", "HJCU8573866" },
				new string[] { "HJCU6215647" }
			};

			var consignments = new List<IPortAuthorityConsignmentData>();
			var mocksGoods = new List<Mock<IPortAuthorityGoodsData>>();
			var mocksConsignment = new List<Mock<IPortAuthorityConsignmentData>>();
			for (int i = 0; i < consignmentCount; i++)
			{
				var goodsMock = new Mock<IPortAuthorityGoodsData>(MockBehavior.Strict);
				mocksGoods.Add(goodsMock);
				goodsMock.Setup(m => m.ItemNumber).Returns(1);
				goodsMock.Setup(m => m.ContainerNumbers).Returns(containerNumbers[i]);
				goodsMock.Setup(m => m.PackageCount).Returns(packageCount[i]);
				goodsMock.Setup(m => m.PackageCode).Returns("PK");
				goodsMock.Setup(m => m.GoodsDescription).Returns("DESCRIPTION OF ITEM");
				goodsMock.Setup(m => m.Kilograms).Returns(kilograms[i]);
				goodsMock.Setup(m => m.CubicMetres).Returns(cubicMetres[i]);
				goodsMock.Setup(m => m.MarksAndNumbers).Returns("");
				goodsMock.Setup(m => m.HarmonisedCode).Returns("");
				goodsMock.Setup(m => m.ContainerYardAddress).Returns("POSTCODE-1111\nECP-AU CONTAINER YARD\nCHAPEL STREET\nBuilding XX\nMARRICKVILLE 2204");

				var consignmentMock = new Mock<IPortAuthorityConsignmentData>(MockBehavior.Strict);
				mocksConsignment.Add(consignmentMock);
				consignmentMock.Setup(m => m.ConsignmentNumber).Returns(i + 1);
				consignmentMock.Setup(m => m.CountryOfOrigin).Returns("");
				consignmentMock.Setup(m => m.PortOfOrigin).Returns("AUSYD");
				consignmentMock.Setup(m => m.PortOfLoading).Returns("");
				consignmentMock.Setup(m => m.PortOfDischarge).Returns(destinationPort[i]);
				consignmentMock.Setup(m => m.PortOfDestination).Returns("");
				consignmentMock.Setup(m => m.CountryOfDestination).Returns(destinationPort[i]);
				consignmentMock.Setup(m => m.BillOfLading).Returns("UNIQUE BILL OF LADING REFERENCE");
				consignmentMock.Setup(m => m.IsWaybill).Returns(false);
				consignmentMock.Setup(m => m.PackingMode).Returns("FCL");
				consignmentMock.Setup(m => m.ConsignorNameAndAddress).Returns((i == 8 ? "CSR\n" : "") + "CONSIGNOR NAME AND ADDRESS");
				consignmentMock.Setup(m => m.ConsigneeNameAndAddress).Returns("");
				consignmentMock.Setup(m => m.Goods).Returns(new IPortAuthorityGoodsData[] { goodsMock.Object });

				consignments.Add(consignmentMock.Object);
			}

			var messageMock = new Mock<IPortAuthorityMessagingData>(MockBehavior.Strict);
			messageMock.Setup(m => m.MessageFunction).Returns(PortAuthorityMessageFunction.Original);
			messageMock.Setup(m => m.MessagePrepared).Returns(new DateTime(2000, 01, 04, 16, 07, 29));
			messageMock.Setup(m => m.Voyage).Returns("777");
			messageMock.Setup(m => m.VesselLloyds).Returns("9999999");
			messageMock.Setup(m => m.VesselName).Returns("VESSEL NAME");
			messageMock.Setup(m => m.Load).Returns("AUSYD");
			messageMock.Setup(m => m.Discharge).Returns("");
			messageMock.Setup(m => m.Equipment).Returns(equipment);
			messageMock.Setup(m => m.Consignments).Returns(consignments);

			AssertMessageEquals("", ContMessage, Builder.GenerateMessageText(messageMock.Object));

			messageMock.VerifyAll();
			mocksGoods.ForEach(x => x.VerifyAll());
			mocksConsignment.ForEach(x => x.VerifyAll());
		}

		public void TestReproduceRORMessage()
		{
			AssertRORMessage("MYPKG", "AUFRE", RORMessage);
			AssertRORMessage("AUPKL", "AUFRE", RORMessage_Load_AUPKL);
			AssertRORMessage("AUFRE", "AUPKL", RORMessage_Discharge_AUPKL);
		}

		void AssertRORMessage(string portOfLoading, string portOfDischarge, string message)
		{
			var goodsMock1 = new Mock<IPortAuthorityGoodsData>(MockBehavior.Strict);
			goodsMock1.Setup(m => m.ItemNumber).Returns(1);
			goodsMock1.Setup(m => m.PackageCount).Returns(1300);
			goodsMock1.Setup(m => m.PackageCode).Returns("PK");
			goodsMock1.Setup(m => m.GoodsDescription).Returns("Porche 911");
			goodsMock1.Setup(m => m.Kilograms).Returns(13000m);
			goodsMock1.Setup(m => m.CubicMetres).Returns(24.8m);
			goodsMock1.Setup(m => m.MarksAndNumbers).Returns("NO MARKS");
			goodsMock1.Setup(m => m.HarmonisedCode).Returns("");
			if (portOfLoading == "AUPKL" || portOfDischarge == "AUPKL")
			{
				goodsMock1.Setup(m => m.ContainerNumber).Returns("VIN123");
			}
			goodsMock1.Setup(m => m.ContainerYardAddress).Returns("POSTCODE-1111\nECP-AU CONTAINER YARD\nCHAPEL STREET\nBuilding XX\nMARRICKVILLE 2204");

			var goodsMock2 = new Mock<IPortAuthorityGoodsData>(MockBehavior.Strict);
			goodsMock2.Setup(m => m.ItemNumber).Returns(2);
			goodsMock2.Setup(m => m.PackageCount).Returns(1300);
			goodsMock2.Setup(m => m.PackageCode).Returns("PK");
			goodsMock2.Setup(m => m.GoodsDescription).Returns("Ferrari F430");
			goodsMock2.Setup(m => m.Kilograms).Returns(2200m);
			goodsMock2.Setup(m => m.CubicMetres).Returns(2200m);
			goodsMock2.Setup(m => m.MarksAndNumbers).Returns("");
			goodsMock2.Setup(m => m.HarmonisedCode).Returns("");
			if (portOfLoading == "AUPKL" || portOfDischarge == "AUPKL")
			{
				goodsMock2.Setup(m => m.ContainerNumber).Returns("");
			}
			goodsMock2.Setup(m => m.ContainerYardAddress).Returns("POSTCODE-1111\nECP-MARITIME CONTAINER YARD\nCANAL ROAD\nBuilding YY\nST PETERS 2204");

			var consignmentMock = new Mock<IPortAuthorityConsignmentData>(MockBehavior.Strict);
			consignmentMock.Setup(m => m.ConsignmentNumber).Returns(1);
			consignmentMock.Setup(m => m.CountryOfOrigin).Returns("MYPEN");
			consignmentMock.Setup(m => m.PortOfOrigin).Returns("");
			consignmentMock.Setup(m => m.PortOfLoading).Returns(portOfLoading);
			consignmentMock.Setup(m => m.PortOfDischarge).Returns(portOfDischarge);
			consignmentMock.Setup(m => m.PortOfDestination).Returns("AUFRE");
			consignmentMock.Setup(m => m.CountryOfDestination).Returns("");
			consignmentMock.Setup(m => m.BillOfLading).Returns("ABCPEN0002115");
			consignmentMock.Setup(m => m.IsWaybill).Returns(false);
			consignmentMock.Setup(m => m.PackingMode).Returns("ROR");
			consignmentMock.Setup(m => m.ConsignorNameAndAddress).Returns("");
			consignmentMock.Setup(m => m.ConsigneeNameAndAddress).Returns("ABC FOODS LTD\nPO BOX 21\nCLIFF STREET WA 6160\nAUSTRALIA");
			consignmentMock.Setup(m => m.Goods).Returns(new IPortAuthorityGoodsData[] { goodsMock1.Object, goodsMock2.Object });

			var messageMock = new Mock<IPortAuthorityMessagingData>(MockBehavior.Strict);
			messageMock.Setup(m => m.MessageFunction).Returns(PortAuthorityMessageFunction.Replace);
			messageMock.Setup(m => m.MessagePrepared).Returns(new DateTime(1997, 06, 30, 11, 03, 31));
			messageMock.Setup(m => m.Voyage).Returns("03S");
			messageMock.Setup(m => m.VesselLloyds).Returns("8506098");
			messageMock.Setup(m => m.VesselName).Returns("AUSTRALIAN ENTERPRISE");
			messageMock.Setup(m => m.Load).Returns(portOfLoading);
			messageMock.Setup(m => m.Discharge).Returns(portOfDischarge);
			messageMock.Setup(m => m.Equipment).Returns(Array.Empty<IPortAuthorityEquipmentData>());
			messageMock.Setup(m => m.Consignments).Returns(new IPortAuthorityConsignmentData[] { consignmentMock.Object });

			AssertMessageEquals("", message, Builder.GenerateMessageText(messageMock.Object));

			goodsMock1.VerifyAll();
			goodsMock2.VerifyAll();
			consignmentMock.VerifyAll();
			messageMock.VerifyAll();
		}

		#region Implementation

		protected IPortAuthorityMessageBuilder Builder
		{
			get { return new T(); }
		}

		IPortAuthorityEquipmentData MockExportContainer(string containerNumber, string isoCode)
		{
			var containerMock = new Mock<IPortAuthorityEquipmentData>(MockBehavior.Strict);
			containerMock.Setup(m => m.ContainerNumber).Returns(containerNumber);
			containerMock.Setup(m => m.ContainerISOCode).Returns(isoCode);
			containerMock.Setup(m => m.ContainerStatus).Returns(PortAuthorityContainerStatus.Export);
			containerMock.Setup(m => m.IsEmpty).Returns(false);

			return containerMock.Object;
		}

		IPortAuthorityEquipmentData MockImportContainer(string containerNumber, string isoCode, bool isEmpty)
		{
			var containerMock = new Mock<IPortAuthorityEquipmentData>(MockBehavior.Strict);
			containerMock.Setup(m => m.ContainerNumber).Returns(containerNumber);
			containerMock.Setup(m => m.ContainerISOCode).Returns(isoCode);
			containerMock.Setup(m => m.ContainerStatus).Returns(PortAuthorityContainerStatus.Import);
			containerMock.Setup(m => m.IsEmpty).Returns(isEmpty);

			return containerMock.Object;
		}
		#endregion

		protected abstract string SimpleMessage { get; }
		protected abstract string SimpleMessage_Load_AUPKL { get; }
		protected abstract string SimpleMessage_Discharge_AUPKL { get;  }
		protected abstract string BulkMessage { get; }
		protected abstract string ContMessage { get; }
		protected abstract string RORMessage { get; }
		protected abstract string RORMessage_Load_AUPKL { get; }
		protected abstract string RORMessage_Discharge_AUPKL { get; }
	}
}

#endif
