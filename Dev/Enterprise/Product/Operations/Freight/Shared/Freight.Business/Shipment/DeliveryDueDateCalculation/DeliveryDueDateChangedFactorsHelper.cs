using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	[Flags]
	public enum DeliveryDueDateChangedFactor
	{
		None = 0x00,
		TransportMode = 0x01,
		HBLDeliveryMode = 0x02,
		ServiceLevel = 0x04,
		PickupCFSAddress = 0x08,
		DeliveryCFSAddress = 0x10,
		PickupFromAddress = 0x20,
		DeliveryToAddress = 0x40,
		PickupRequiredBy = 0x80,
		ActualPickup = 0x100,
		InterimReceipt = 0x200,
		ConsolAttached = 0x400,
		TransportLeg = 0x800
	}

	public static class DeliveryDueDateChangedFactorsHelper
	{
		public static ZString GetChangedEventReason(DeliveryDueDateChangedFactor changedFactors)
		{
			var result = new ZStringBuilder();
			foreach (DeliveryDueDateChangedFactor factor in Enum.GetValues(typeof(DeliveryDueDateChangedFactor)))
			{
				if (factor != DeliveryDueDateChangedFactor.None && changedFactors.HasFlag(factor))
				{
					if (!result.IsEmpty)
					{
						result.Append(", ");
					}

					result.Append(GetDescription(factor));
				}
			}

			result.Append((NoResString)" changed");
			return result.ToString();
		}

		static ZString GetDescription(DeliveryDueDateChangedFactor factor)
		{
			var result = ZString.Empty;

			switch (factor)
			{
				case DeliveryDueDateChangedFactor.None:
					break;
				case DeliveryDueDateChangedFactor.TransportMode:
					result = Res.GetString("fd3b309d-cb3d-46f0-aff4-c5bd9f8503c5", "Transport Mode");
					break;
				case DeliveryDueDateChangedFactor.HBLDeliveryMode:
					result = Res.GetString("af2e3b4d-feef-4b3b-b021-20d9e8d31748", "HBL Delivery Mode");
					break;
				case DeliveryDueDateChangedFactor.ServiceLevel:
					result = Res.GetString("02e9d6b5-b7cb-409e-b80e-ccd3e1e44a3e", "Service Level");
					break;
				case DeliveryDueDateChangedFactor.PickupCFSAddress:
					result = Res.GetString("0189c16c-b363-4b43-8b87-7b91059253b2", "Origin CFS");
					break;
				case DeliveryDueDateChangedFactor.DeliveryCFSAddress:
					result = Res.GetString("1bf6fc4f-1e34-458f-b6fa-da3a5464eada", "Destination CFS");
					break;
				case DeliveryDueDateChangedFactor.PickupFromAddress:
					result = Res.GetString("24cf8bfc-2f4d-45bb-bbff-ce90c4d1f7a6", "Pickup From address");
					break;
				case DeliveryDueDateChangedFactor.DeliveryToAddress:
					result = Res.GetString("523d9e3e-9284-4daf-862b-5e3d841703a4", "Delivery To address");
					break;
				case DeliveryDueDateChangedFactor.PickupRequiredBy:
					result = Res.GetString("2c8bd6e9-0b28-4815-bf74-e23bf9ed3ab1", "Pickup Required By");
					break;
				case DeliveryDueDateChangedFactor.ActualPickup:
					result = Res.GetString("cb528fda-8128-44e6-a0fd-e66a692800f7", "Actual Pickup Date");
					break;
				case DeliveryDueDateChangedFactor.InterimReceipt:
					result = Res.GetString("2ca9da44-fc5d-41d8-9c45-d7608206f824", "Interim Receipt Date");
					break;
				case DeliveryDueDateChangedFactor.ConsolAttached:
					result = Res.GetString("f10687b2-9cf2-4fa2-a42a-de45e83c5c52", "Consol Attached");
					break;
				case DeliveryDueDateChangedFactor.TransportLeg:
					result = Res.GetString("a3662427-ef18-48fb-9543-67bfcef2c84d", "Transport Leg");
					break;
			}

			return result;
		}
	}
}
