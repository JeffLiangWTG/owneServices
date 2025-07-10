using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Business
{
	#region Helper

	public class ProductWarehouseHelper
	{
		public ProductWarehouseHelper(ForwardingShipment shipment, INotifications notificationsParent, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter)
		{
			this.shipment = shipment;
			this.notificationsParent = notificationsParent;
			this.dataWriterGetter = dataWriterGetter;
		}

		readonly ForwardingShipment shipment;
		readonly INotifications notificationsParent;
		readonly Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter;

		public void CreateProductWarehouseReceive()
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "Create Product Warehouse Receive" };
			using (factory.AddDisposableService())
			using (var notifier = new ProductWarehouseInstructionNotifier(notificationsParent))
			{
				CreateProductWarehouseReceiveCore(notifier, factory);
			}
		}

		void CreateProductWarehouseReceiveCore(ProductWarehouseInstructionNotifier notifier, BusinessObjectFactory factory)
		{
			if (ValidateCanCreateProductWarehouseReceive(notifier))
			{
				using (var dataExport = new ManualDataExport(factory, new[] { shipment }, UniversalDataType.UniversalShipment, dataWriterGetter: dataWriterGetter))
				{
					dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.WarehouseInwards;

					var xmlEvents = dataExport.SendData(notifier);

					ZExceptionReporting.ProcessWithConcurrencyHandling(() => factory.Save(), null);

					if (xmlEvents.Any())
					{
						var statusCode = xmlEvents.Select(e => e.Context.ProcessingStatusCode).FirstOrDefault(status => !status.IsEmpty);
						var failureReason = xmlEvents.Select(e => e.Context.FailureReason).FirstOrDefault(reason => !reason.IsEmpty);

						var result = ManualDataExportHelper.GetMessage(
							statusCode,
							Res.GetString("926b2a23-0bb9-42c6-bde1-24bae4ad59ed", "Instruction"),
							Res.GetString("003bdc89-6ca1-433a-b3cb-0c9792dd220e", "Warehouse Receive"));

						if (result.ErrorType == MessageTypes.Success)
						{
							notifier.Add(new InfoNotification(result.Message));
						}
						else if (result.ErrorType == MessageTypes.Error)
						{
							notifier.Add(new ErrorNotification(ErrorType.Error, result.Message));
						}
						else
						{
							notifier.Add(new WarningNotification(result.Message));
						}
					}
				}
			}
		}

		bool ValidateCanCreateProductWarehouseReceive(INotifications notifier)
		{
			var result = true;
			if (shipment.HasChanges)
			{
				notifier.AddError(Res.GetString("a9643ea2-336d-49c3-b835-7a143f3a91e1", "Please save your changes before creating the Product Warehouse Receive."));
				result = false;
			}

			return result;
		}
	}

	#endregion

	public class ProductWarehouseInstructionNotifier : InstructionNotifier
	{
		public ProductWarehouseInstructionNotifier(INotifications parent)
			: base(parent)
		{
		}

		protected override string BuildInstructionMessage()
		{
			var instructionMessageBuilder = new ZStringBuilder();
			instructionMessageBuilder.Append(Res.GetString("9803dff2-e237-4aa8-bee3-bf3ce628cdb6", "The Warehouse Receive Instruction has been sent."));
			foreach (var message in notifications.Select(n => n.Message))
			{
				instructionMessageBuilder.Append(message);
			}

			return instructionMessageBuilder.ToStringWithNewLineBetweenAppends();
		}
	}
}
