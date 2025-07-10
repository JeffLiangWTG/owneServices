using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using CodeDescPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.CFS.DataTransfer.Universal.Testing
{
	public class CFSLoadListConsolDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReading_WithForwarder_Export()
		{
			var dataObject = CreateDataObject();
			var consol = new CFSLoadListConsolDataObjectReader(dataObject, new TestErrorLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("SEA", consol.JK_TransportMode);
				AssertEquals("FCL", consol.JK_ConsolMode);
				AssertEquals("AUMEL", consol.JK_RL_NKLoadPort);
				AssertEquals("USCHI", consol.JK_RL_NKDischargePort);
				AssertEquals("YO YO YO", consol.JK_AgentsReference);
				AssertEquals("REF123", consol.JK_BookingReference);
				AssertEquals("HBL456", consol.JK_MasterBillNum);
				AssertEquals("SUPREME PIZZA", consol.JK_CustomsReference);

				AssertEquals(forwarder.PK, consol.JK_OH_Forwarder);
				AssertEquals(forwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
				Assert(consol.JK_OA_ReceivingForwarderAddress.IsEmpty);

				AssertEquals(shippingLine.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
				AssertEquals(containerYard.MainAddress.PK, consol.JK_OA_EmptyContainerYard);
				AssertEquals(cto.MainAddress.PK, consol.JK_OA_CTOAddress);
				AssertEquals(cartageCo.MainAddress.PK, consol.JK_OA_CartageCoAddress);
				AssertEquals(depot.MainAddress.PK, consol.JK_OA_DepotAddress);

				AssertEquals(2, consol.Transports.Count);
				AssertEquals("AUMEL", consol.Transports[0].JW_RL_NKLoadPort);
				AssertEquals("NZAKL", consol.Transports[0].JW_RL_NKDiscPort);
				AssertEquals("NZAKL", consol.Transports[1].JW_RL_NKLoadPort);
				AssertEquals("USCHI", consol.Transports[1].JW_RL_NKDiscPort);

				AssertEquals(2, consol.Containers.Count);
				AssertEquals("CONT1", consol.Containers[0].JC_ContainerNum);
				AssertEquals("CONT2", consol.Containers[1].JC_ContainerNum);
				AssertEquals(2, consol.Containers[1].PackLines.Count);
				AssertEquals("PACK1", consol.Containers[1].PackLines[0].JL_MarksAndNumbers);
				AssertEquals("PACK2", consol.Containers[1].PackLines[1].JL_MarksAndNumbers);

				AssertEquals(2, consol.Shipments.Count);
				AssertEquals("HBL123", consol.Shipments[0].JS_HouseBill);
				AssertEquals("HBL456", consol.Shipments[1].JS_HouseBill);
			});
		}

		public void TestReading_WithForwarder_Import()
		{
			var dataObject = CreateDataObject(export: false);
			var consol = new CFSLoadListConsolDataObjectReader(dataObject, new TestErrorLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("SEA", consol.JK_TransportMode);
				AssertEquals("FCL", consol.JK_ConsolMode);
				AssertEquals("USCHI", consol.JK_RL_NKLoadPort);
				AssertEquals("AUMEL", consol.JK_RL_NKDischargePort);
				AssertEquals("YO YO YO", consol.JK_AgentsReference);
				AssertEquals("REF123", consol.JK_BookingReference);
				AssertEquals("HBL456", consol.JK_MasterBillNum);
				AssertEquals("SUPREME PIZZA", consol.JK_CustomsReference);

				AssertEquals(forwarder.PK, consol.JK_OH_Forwarder);
				Assert(consol.JK_OA_SendingForwarderAddress.IsEmpty);
				AssertEquals(forwarder.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);

				AssertEquals(shippingLine.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
				AssertEquals(containerYard.MainAddress.PK, consol.JK_OA_EmptyContainerYard);
				AssertEquals(cto.MainAddress.PK, consol.JK_OA_CTOAddress);
				AssertEquals(cartageCo.MainAddress.PK, consol.JK_OA_CartageCoAddress);
				AssertEquals(depot.MainAddress.PK, consol.JK_OA_DepotAddress);

				AssertEquals(2, consol.Transports.Count);
				AssertEquals("USCHI", consol.Transports[0].JW_RL_NKLoadPort);
				AssertEquals("NZAKL", consol.Transports[0].JW_RL_NKDiscPort);
				AssertEquals("NZAKL", consol.Transports[1].JW_RL_NKLoadPort);
				AssertEquals("AUMEL", consol.Transports[1].JW_RL_NKDiscPort);

				AssertEquals(2, consol.Containers.Count);
				AssertEquals("CONT1", consol.Containers[0].JC_ContainerNum);
				AssertEquals("CONT2", consol.Containers[1].JC_ContainerNum);
				AssertEquals(2, consol.Containers[1].PackLines.Count);
				AssertEquals("PACK1", consol.Containers[1].PackLines[0].JL_MarksAndNumbers);
				AssertEquals("PACK2", consol.Containers[1].PackLines[1].JL_MarksAndNumbers);

				AssertEquals(2, consol.Shipments.Count);
				AssertEquals("HBL123", consol.Shipments[0].JS_HouseBill);
				AssertEquals("HBL456", consol.Shipments[1].JS_HouseBill);
			});
		}

		public void TestReading_WithoutForwarder_Export()
		{
			var dataObject = CreateDataObject(includingForwarder: false);
			var consol = new CFSLoadListConsolDataObjectReader(dataObject, new TestErrorLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals(sendingForwarder.PK, consol.JK_OH_Forwarder);
				AssertEquals(sendingForwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
				AssertEquals(receivingForwarder.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
			});
		}

		public void TestReading_WithoutForwarder_Import()
		{
			var dataObject = CreateDataObject(includingForwarder: false, export: false);
			var consol = new CFSLoadListConsolDataObjectReader(dataObject, new TestErrorLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals(receivingForwarder.PK, consol.JK_OH_Forwarder);
				AssertEquals(sendingForwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
				AssertEquals(receivingForwarder.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
			});
		}

		public void TestReading_ContainerCFSRegistered()
		{
			var dataObject = CreateDataObject();
			dataObject.ContainerCollection[0].IsCFSRegistered = false;
			dataObject.ContainerCollection[1].IsCFSRegistered = false;

			var consol = new CFSLoadListConsolDataObjectReader(dataObject, new TestErrorLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				Assert(consol.Containers[0].JC_IsCFSRegistered);
				Assert(consol.Containers[1].JC_IsCFSRegistered);
			});
		}

		public void TestReading_TransportLegsWithPartialAttribute()
		{
			var dataObject = CreateDataObject(includingForwarder: false);
			dataObject.TransportLegCollection.Content = CollectionContent.Partial;

			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_AgentsReference = "YO YO YO";
			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUMEL";
			transport1.JW_RL_NKDiscPort = "NZAKL";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "USCHI";
			transport2.JW_RL_NKDiscPort = "USNYC";

			var reader = new CFSLoadListConsolDataObjectReader(dataObject, new TestErrorLogger(), Factory);
			reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals(sendingForwarder.PK, consol.JK_OH_Forwarder);
				AssertEquals(sendingForwarder.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
				AssertEquals(receivingForwarder.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
				AssertEquals(3, consol.Transports.Count);
				AssertNotNull("AUMEL->NZAKL", consol.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_RL_NKLoadPort == "AUMEL" && x.JW_RL_NKDiscPort == "NZAKL"));
				AssertNotNull("NZAKL->USCHI", consol.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_RL_NKLoadPort == "NZAKL" && x.JW_RL_NKDiscPort == "USCHI"));
				AssertNotNull("USCHI->USNYC", consol.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_RL_NKLoadPort == "USCHI" && x.JW_RL_NKDiscPort == "USNYC"));
			});
		}

		public void TestImportLoadLisConsoltWithDuplicatedAddressType()
		{
			var shipmentDataObject = CreateDataObject();

			var address1 = new OrganizationAddress();
			address1.AddressType = "ConsignorDocumentaryAddress";

			var address2 = new OrganizationAddress();
			address2.AddressType = "ConsignorDocumentaryAddress";

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { address1, address2 });

			var reader = new CFSLoadListConsolDataObjectReader(shipmentDataObject, new TestErrorLogger(), Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("One OrganizationAddressCollection with same AddressType shoud have exception thrown", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportNewLoadListToExistingShipment()
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.JS_HouseBill = "HBL456";
			var loadList = shipment.Consols.AddNew();
			loadList.JK_RL_NKLoadPort = "AUMEL";
			loadList.JK_RL_NKDischargePort = "NZAKL";
			loadList.JK_AgentsReference = "LoadList1";
			loadList.JK_BookingReference = "REF456";
			loadList.JK_MasterBillNum = "HBL123";

			Factory.SaveForTesting();
			var dataObject = CreateDataObject();

			var innerShip = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HBL456",
			};
			innerShip.SetPackingLineCollection(() =>
				new DataObjectList<PackingLine>(new[]
				{
					new PackingLine { MarksAndNos = "PACK1", ContainerLink = 1 }
				})
				{ Content = CollectionContent.Complete });
			dataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { innerShip });

			var logger = new TestErrorLogger();
			var reader = new CFSLoadListConsolDataObjectReader(dataObject, logger, Factory);
			var newLoadList = reader.ReadIntoBusinessObject();

			AssertNoExceptionThrown(() => Factory.SaveAtEndOfImport(logger));
			AssertEquals(1, newLoadList.Shipments.Count);
			AssertEquals(shipment.PK, newLoadList.Shipments[0].PK);
		}

		#region Implementation

		OrgHeader forwarder;
		OrgHeader sendingForwarder;
		OrgHeader receivingForwarder;
		OrgHeader shippingLine;
		OrgHeader containerYard;
		OrgHeader cto;
		OrgHeader cartageCo;
		OrgHeader depot;

		protected override void SetUp()
		{
			base.SetUp();

			forwarder = CreateOrg("FWDORG", "100 Forwarder St");
			sendingForwarder = CreateOrg("SNDORG", "240 SendingForwarder St");
			receivingForwarder = CreateOrg("RECORG", "300 ReceivingForwarder Rd");
			shippingLine = CreateOrg("SHPORG", "15 Shipping Line Ave");
			containerYard = CreateOrg("CYDORG", "37 Container Yard Rd");
			cto = CreateOrg("CTOORG", "251A CTO Lane");
			cartageCo = CreateOrg("CTGORG", "1491582 Cartage Crescent");
			depot = CreateOrg("DEPORG", "19/3 Depot Drive");

			Factory.SaveForTesting();
		}

		OrgHeader CreateOrg(string companyCode, string address)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = companyCode;
			org.MainAddress.OA_Address1 = address;

			return org;
		}

		UNLOCO CreateUnloco(ZString codeValue, ZString nameValue)
		{
			return new UNLOCO { Code = codeValue, Name = nameValue };
		}

		Shipment CreateDataObject(bool includingForwarder = true, bool export = true)
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = new CodeDescPair { Code = "SEA", Description = "Sea Transport" };
			dataObject.ContainerMode = new ContainerMode { Code = "FCL", Description = "Full Container Load" };

			var auMel = CreateUnloco("AUMEL", "Smellbourne");
			var usChi = CreateUnloco("USCHI", "Chicago");
			if (export)
			{
				dataObject.PortOfLoading = auMel;
				dataObject.PortOfDischarge = usChi;
			}
			else
			{
				dataObject.PortOfLoading = usChi;
				dataObject.PortOfDischarge = auMel;
			}

			dataObject.AgentsReference = "YO YO YO";
			dataObject.BookingConfirmationReference = "REF123";
			dataObject.WayBillNumber = "HBL456";
			dataObject.CFSReference = "SUPREME PIZZA";

			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendingForwarderAddress, OrganizationCode = "SNDORG", Address1 = "240 SendingForwarder St" },
				new OrganizationAddress { AddressType = AddressTypes.ReceivingForwarderAddress, OrganizationCode = "RECORG", Address1 = "300 ReceivingForwarder Rd" },
				new OrganizationAddress { AddressType = nameof(DocAddressType.ShippingLineAddress), OrganizationCode = "SHPORG", Address1 = "15 Shipping Line Ave" },
				new OrganizationAddress { AddressType = AddressTypes.ContainerYardAddress, OrganizationCode = "CYDORG", Address1 = "37 Container Yard Rd" },
				new OrganizationAddress { AddressType = AddressTypes.CTOAddress, OrganizationCode = "CTOORG", Address1 = "251A CTO Lane" },
				new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageAddress1), OrganizationCode = "CTGORG", Address1 = "1491582 Cartage Crescent" },
				new OrganizationAddress { AddressType = AddressTypes.DepotAddress, OrganizationCode = "DEPORG", Address1 = "19/3 Depot Drive" }
			});

			if (includingForwarder)
			{
				dataObject.OrganizationAddressCollection.Add(new OrganizationAddress { AddressType = AddressTypes.Forwarder, OrganizationCode = "FWDORG", Address1 = "100 Forwarder St" });
			}

			var nzAkl = CreateUnloco("NZAKL", "Auckland");
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			if (export)
			{
				dataObject.TransportLegCollection.Add(new TransportLeg
				{
					PortOfLoading = auMel,
					PortOfDischarge = nzAkl
				});
				dataObject.TransportLegCollection.Add(new TransportLeg
				{
					PortOfLoading = nzAkl,
					PortOfDischarge = usChi
				});
			}
			else
			{
				dataObject.TransportLegCollection.Add(new TransportLeg
				{
					PortOfLoading = usChi,
					PortOfDischarge = nzAkl
				});
				dataObject.TransportLegCollection.Add(new TransportLeg
				{
					PortOfLoading = nzAkl,
					PortOfDischarge = auMel
				});
			}

			var twentyGP = new ContainerType { Code = "20GP", ISOCode = "22G0" };

			dataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { ContainerNumber = "CONT1", ContainerType = twentyGP },
				new Container { ContainerNumber = "CONT2", ContainerType = twentyGP, Link = 1 }
			});

			var innerShip = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HBL456",
			};
			innerShip.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { new PackingLine { MarksAndNos = "PACK1", ContainerLink = 1 }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { MarksAndNos = "PACK2", ContainerLink = 1 } })
			{
				Content = CollectionContent.Complete
			});
			dataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			{
				new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "HBL123"
				},
				innerShip
			});

			return dataObject;
		}

		#endregion
	}
}
