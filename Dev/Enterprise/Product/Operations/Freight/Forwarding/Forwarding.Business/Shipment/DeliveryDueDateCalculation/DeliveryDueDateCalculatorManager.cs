using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Is the implementation of IDeliveryDueDateCalculatorManager in EnterpriseApplicationConfiguration")]
	public class DeliveryDueDateCalculatorManager : IDeliveryDueDateCalculatorManager
	{
		public ZDateTime Calculate(BusinessObjectFactory factory,
			ZDateTime readyDate,
			ZString serviceLevel,
			ZString hblDeliveryMode,
			ZString pickupOrgCode,
			ZString pickupAddressCode,
			ZString pickupCFSOrgCode,
			ZString pickupCFSAddressCode,
			ZString deliveryCFSOrgCode,
			ZString deliveryCFSAddressCode,
			ZString deliveryOrgCode,
			ZString deliveryAddressCode,
			ZString transportMode,
			ZString deliveryType)
		{
			var deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(factory,
				readyDate,
				serviceLevel,
				hblDeliveryMode,
				pickupOrgCode,
				pickupAddressCode,
				pickupCFSOrgCode,
				pickupCFSAddressCode,
				deliveryCFSOrgCode,
				deliveryCFSAddressCode,
				deliveryOrgCode,
				deliveryAddressCode,
				transportMode,
				deliveryType);
			var deliveryDueDateCalculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);

			return deliveryDueDateCalculator?.CalculateDeliveryDueDate()?.DeliveryDueDate ?? ZDateTime.Empty;
		}

		public IDeliveryDueDateCalculationResult Calculate(BusinessObject bizObj)
		{
			if (bizObj is ForwardingShipment shipment)
			{
				var deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
				var deliveryDueDateCalculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);

				try
				{
					return deliveryDueDateCalculator?.CalculateDeliveryDueDate();
				}
				catch (Exception ex)
				{
					var calculationLogBuilder = new ZStringBuilder();
					calculationLogBuilder.AppendLine(ex.Message);
					calculationLogBuilder.AppendLine(ex.StackTrace);
					return DeliveryDueDateCalculationResult.Failure(Res.GetString("378E3F16-894E-4C3F-924B-89562755284E", "Delivery Due Date calculation failed because there's an exception"), calculationLogBuilder.ToString());
				}
			}

			return DeliveryDueDateCalculationResult.Failure(ZString.Empty, ZString.Empty);
		}
	}
}
