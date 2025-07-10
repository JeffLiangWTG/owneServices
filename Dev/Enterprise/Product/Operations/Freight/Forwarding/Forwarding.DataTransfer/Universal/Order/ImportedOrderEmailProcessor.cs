using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Environment;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	sealed class ImportedOrderEmailProcessor : INotificationEmailProcessor
	{
		public ImportedOrderEmailProcessor(IReadOnlyDictionary<ZGuid, UniversalShipment> orderWithDataObjects, OrderImportErrorTypes importErrorType)
		{
			Argument.NotNull(orderWithDataObjects, nameof(orderWithDataObjects));

			this.orderWithDataObjects = orderWithDataObjects;
			this.importErrorType = importErrorType;
		}

		readonly IReadOnlyDictionary<ZGuid, UniversalShipment> orderWithDataObjects;
		readonly OrderImportErrorTypes importErrorType;

		#region INotificationEmailProcessor

		HtmlEmailDef INotificationEmailProcessor.Process(IEDIMessage message, HtmlEmailDef htmlEmailDef)
		{
			if (message == null || htmlEmailDef == null || !orderWithDataObjects.Any())
			{
				return htmlEmailDef;
			}

			var importedOrders = GetImportedOrders().ToList();
			var attachedOrderDescription = GetAttachedOrderDescription();
			var extraInformation = Res.GetString("76cf9506-e0e7-41fc-8408-4f5e9ea3f764", "Shipment was imported successfully however the following orders cannot be imported due to order cut off date has passed or is not set. Refer to attached Report for details.");

			ImportedOrderChangesDocumentManager.AttachDocumentInEmailWithAllImportedOrderChanges(importedOrders, attachedOrderDescription, extraInformation, htmlEmailDef);

			return htmlEmailDef;
		}

		ZString GetAttachedOrderDescription()
		{
			switch (importErrorType)
			{
				case OrderImportErrorTypes.CutOffDateNotSet:
					{
						return ResString.GetMultilingualString("EF3E6B23-5533-43BE-8783-F3E8919C7575", "The following Orders were not updated because they have attached shipments or declarations and Order Update Cutoff date is not set.");
					}

				case OrderImportErrorTypes.CutOffDateHasPassed:
					{
						return ResString.GetMultilingualString("7E33101A-D225-4071-B402-1F60A71B72F0", "The following Orders were not updated because they have attached shipments or declarations and Order Update Cutoff date has passed.");
					}

				default:
					{
						return ZString.Empty;
					}
			}
		}

		IEnumerable<ImportedOrder> GetImportedOrders()
		{
			if (!orderWithDataObjects.Any())
			{
				yield break;
			}

			var logger = new XmlSessionTracker(new DummyLogInformation());
			var factory = new UniversalObjectFactory();

			foreach (var kv in orderWithDataObjects)
			{
				if (kv.Value != null)
				{
					var orderInNewFactory = factory.Load<Order>(kv.Key) ?? factory.New<Order>();

					try
					{
						var reader = new OrderDataObjectReader(kv.Value, logger, factory, new OrderLineLinkManager());
						reader.PopulateBO(orderInNewFactory, true);
					}
					catch
					{
						// Should Ignore All Unexpected Exceptions
					}

					yield return new ImportedOrder(orderInNewFactory);
				}
			}
		}

		#endregion

		#region Dummy Log

		sealed class DummyLogInformation : ISimpleLogger
		{
			public IEnumerable<ISimpleLog> Logs => System.Array.Empty<ISimpleLog>();

			public void Log(LogType type, string message)
			{
			}
		}

		#endregion
	}
}
