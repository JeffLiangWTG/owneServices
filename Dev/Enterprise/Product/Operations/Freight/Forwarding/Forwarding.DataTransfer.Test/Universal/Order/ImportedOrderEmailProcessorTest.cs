using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Environment;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using ForwardingOrder = Enterprise.Freight.Forwarding.Orders.Business.Order;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ImportedOrderEmailProcessorTest : TestCaseWithFactory
	{
		public void TestProcess_EmptyOrderDataObjects()
		{
			var orderWithDataObjects = new Dictionary<ZGuid, UniversalShipment>();

			var htmlEmailDef = RunImportedOrderEmailProcess(Factory, orderWithDataObjects);

			Assert("Should empty as the orders count is zero.", string.IsNullOrWhiteSpace(htmlEmailDef.Body));
			AssertEquals("Should equal to 0 as the orders count is zero.", 0, htmlEmailDef.Attachments.Count);
		}

		public void TestProcess_NewOrderDataObjects()
		{
			var universalObjectFactory = new UniversalObjectFactory();
			var factory = universalObjectFactory.BOFactory;

			var orderWithDataObjects = new Dictionary<ZGuid, UniversalShipment>
			{
				{
					Guid.NewGuid(),
					new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						TotalWeight = 150m,
						TotalVolume = 20m,
						TransportMode = new CodeDescriptionPair { Code = "SEA" }
					}
				}
			};

			var htmlEmailDef = RunImportedOrderEmailProcess(factory, orderWithDataObjects);
			Assert("Should not contains the expected text from the ImportedOrderEmailProcessor.", htmlEmailDef.Body.Contains(expectedExtraInfomation));
			AssertEquals("Should not contains the expected file from the ImportedOrderEmailProcessor.", 1, htmlEmailDef.Attachments.Count);
		}

		public void TestProcess_AttachedOrderDataObjects()
		{
			var universalObjectFactory = new UniversalObjectFactory();
			var factory = universalObjectFactory.BOFactory;

			var data = new UniversalTestData(universalObjectFactory);

			var buyer = data.ConsigneeOrgCRAHOLSYD;

			var order = factory.New<ForwardingOrder>();
			order.JD_OrderNumber = "ORDERME";
			order.JD_OrderNumberSplit = new ZByte(2);
			order.JD_ActualWeight = 12m;
			order.JD_ActualVolume = 5m;
			order.JD_TransportMode = "AIR";
			order.JD_OA_BuyerAddress = buyer.Addresses.MainAddress.PK;

			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;

			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000001";
			shipment.JS_IsBooking = false;
			shipment.JS_IsForwardRegistered = true;
			order.JD_JS = shipment.PK;

			universalObjectFactory.SaveForTesting();
			var orderShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TotalWeight = 150m,
				TotalVolume = 20m,
				TransportMode = new CodeDescriptionPair { Code = "SEA" }
			};
			orderShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					data.ConsigneeAddressCRAHOLSYDDataObject
				});
			var orderWithDataObjects = new Dictionary<ZGuid, UniversalShipment>
			{
				{
					order.PK,
					orderShipment
				}
			};

			using (Enterprise.Registry.Business.OrdersDataRegistry.Instance.IncludeChangesNotApplied.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var htmlEmailDef = RunImportedOrderEmailProcess(factory, orderWithDataObjects);
				Assert("Should not contains the expected text from the ImportedOrderEmailProcessor.", !htmlEmailDef.Body.Contains(expectedExtraInfomation));
				AssertEquals("Should not contains the expected file from the ImportedOrderEmailProcessor.", 0, htmlEmailDef.Attachments.Count);
			}

			using (Enterprise.Registry.Business.OrdersDataRegistry.Instance.IncludeChangesNotApplied.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var htmlEmailDef = RunImportedOrderEmailProcess(factory, orderWithDataObjects);
				Assert("Should contains the expected text from the ImportedOrderEmailProcessor.", htmlEmailDef.Body.Contains(expectedExtraInfomation));
				AssertEquals("Should contains the expected file from the ImportedOrderEmailProcessor.", 1, htmlEmailDef.Attachments.Count);
			}
		}

		const string expectedExtraInfomation = "Shipment was imported successfully however the following orders cannot be imported due to order cut off date has passed or is not set. Refer to attached Report for details.";

		HtmlEmailDef RunImportedOrderEmailProcess(BusinessObjectFactory factory, IReadOnlyDictionary<ZGuid, UniversalShipment> orderWithDataObjects)
		{
			var processor = new ImportedOrderEmailProcessor(orderWithDataObjects, OrderImportErrorTypes.CutOffDateNotSet);

			var manager = new NotificationEmailManager();
			manager.Register(processor);

			var message = factory.New<IEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Sent;

			var htmlEmailDef = new HtmlEmailDef { Body = string.Empty };
			htmlEmailDef.Attachments.Clear();

			manager.Process(message, htmlEmailDef);

			return htmlEmailDef;
		}
	}
}
