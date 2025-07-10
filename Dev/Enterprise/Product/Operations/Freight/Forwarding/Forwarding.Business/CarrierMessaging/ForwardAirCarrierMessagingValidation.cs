using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardAirCarrierMessagingValidation : CarrierMessagingValidation
	{
		public ForwardAirCarrierMessagingValidation(ForwardingConsol consol)
			: base(consol)
		{
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void ValidateCore(INotifications notifications)
		{
			var notificationsBuffer = new NotificationBuffer(notifications);

			if (!consol.Shipments.Any() && !notificationsBuffer.HasErrors)
			{
				notificationsBuffer.AddError(Res.GetString("{ab34b931-811b-46b8-9dd7-75a6c1baad8a", "There is no shipment attached to the consol."));
			}

			if (!IsCarrierRegistrationValid() && !notificationsBuffer.HasErrors)
			{
				notificationsBuffer.AddError(Res.GetString("2e342ceb-4143-4e79-896c-e51e729887f4", "The carrier is not registered as Forward Air."));
			}

			if (!IsArrivalDateValid() && !notificationsBuffer.HasErrors)
			{
				notificationsBuffer.AddError(Res.GetString("6975703a-f88d-4c8f-aef1-e885d0f12718", "The Arrival date is not specified for the consol."));
			}

			if (consol.JK_MasterBillNum.IsEmpty && !IsCanAllocateForwardAirBillNumber() && !notificationsBuffer.HasErrors)
			{
				notificationsBuffer.AddError(Res.GetString("b70938a0-3dc8-434a-83ac-989f4f8e7e2c"
					, @"Message cannot be sent when Master Bill Number is empty and we cannot allocate one Forward Air Bill Number from the consol's carrier.
Please contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges."));
			}

			if (IsContainsMultipleCarrierContractNumber() && !notificationsBuffer.HasErrors)
			{
				notificationsBuffer.AddError(Res.GetString("78fea78a-aebe-4506-8c11-a34d7cad058a",
					"You cannot have multiple Forward Air contract numbers registered on the same job. Please remove those that don't apply."));
			}

			if (consol.Shipments.Count == 1 && consol.JK_OA_ReceivingForwarderAddress.IsEmpty
				&& !consol.Shipments[0].ConsigneeDeliveryAddress.IsValidAddress && !notificationsBuffer.HasErrors)
			{
				notificationsBuffer.AddError(Res.GetString("5e43f20b-fe72-4388-887a-175221bb2131",
					"Organization must be entered in either the Consol Receiving Agent or the Shipment's Deliver To"));
			}

			if (consol.Shipments.Count == 1 && consol.JK_OA_SendingForwarderAddress.IsEmpty
				&& !consol.Shipments[0].ConsignorPickupAddress.IsValidAddress && !notificationsBuffer.HasErrors)
			{
				notificationsBuffer.AddError(Res.GetString("0f5a4718-7b52-44a1-9acd-77c6ae148593",
					"Organization must be entered in either the Consol Sending Agent or the Shipment's Pick Up From"));
			}

			if (consol.Shipments.Count >= 2 && consol.JK_OA_ReceivingForwarderAddress.IsEmpty && !notificationsBuffer.HasErrors)
			{
				notificationsBuffer.AddError(Res.GetString("8764ea18-f208-4bfe-b41a-5d3b7093bf3a",
					"Organization must be entered in the Consol's Receiving Agent field"));
			}

			if (consol.Shipments.Count >= 2 && consol.JK_OA_SendingForwarderAddress.IsEmpty && !notificationsBuffer.HasErrors)
			{
				notificationsBuffer.AddError(Res.GetString("2dd30135-3056-45c5-bbb9-ec58f40da631",
					"Organization must be entered in the Consol's Sending Agent field"));
			}

			if (!HasForwardAirDeliveryServiceLevel() && !notificationsBuffer.HasErrors)
			{
				var message = Res.GetString("b6d3a2ff-eb00-4b82-8243-d3f64faa452b", @"The carrier service level does not correspond to Forward Air delivery methods and therefore no special door pickup or delivery instructions will be send to Forward Air. Do you want to proceed?

If you need to request pickup or delivery service, please configure your Forward Air Organization
(Organization -> Carrier -> Service Level) with the following service levels: PUC-Pickup, PUD-Delivery, PAD-Pickup and Delivery.");

				var queryArgs = new QueryUserYesNoEventArgs(message, false);
				notifications.QueryUser(queryArgs);

				if (!queryArgs.Response)
				{
					notificationsBuffer.AddError(Res.GetString("51791748-74c6-41a6-b378-4a5245ef56a8", "User canceled the message."));
				}
			}
		}

		protected override void ValidateMasterBillNumberCore(INotifications notifications)
		{
			if (consol.JK_MasterBillNum.IsEmpty)
			{
				notifications.AddError(Res.GetString("ceeb667e-6f08-4d80-9481-595d6fdc4236", "Message cannot be sent when Master Bill Number is empty."));
			}
		}

		bool IsCarrierRegistrationValid()
		{
			if (consol.ShippingLine != null)
			{
				ZQuery carrierRegistration = new ZQuery();
				carrierRegistration.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.EHubOrganisationID);
				carrierRegistration.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, ApplicationCodeList.Codes.ForwardAir);

				return consol.ShippingLine.CustomsCodes.Find(carrierRegistration).Length != 0;
			}

			return false;
		}

		bool IsArrivalDateValid()
		{
			if (consol.IsDirect)
			{
				return consol.Shipments.Cast<ForwardingShipment>().Any(x => x.DocsAndCartage.JP_EstimatedDelivery.IsValid || x.JS_E_ARV.IsValid);
			}
			else
			{
				if (consol.Transports.Any())
				{
					return consol.Transports.Cast<Transport>().Last().JW_ETA.IsValid;
				}
				else
				{
					return false;
				}
			}
		}

		bool HasForwardAirDeliveryServiceLevel()
		{
			return consol.JK_AWBServiceLevel == Pickup || consol.JK_AWBServiceLevel == Delivery || consol.JK_AWBServiceLevel == PickupAndDelivery;
		}

		bool IsCanAllocateForwardAirBillNumber()
		{
			var stmNums = consol.ForwardAirBillStmNums;
			return stmNums != null && stmNums.TryGetNumberFountain() != null;
		}

		bool IsContainsMultipleCarrierContractNumber()
		{
			return consol.Numbers.Cast<CusEntryNumber>()
				.Count(num => num.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON) > 1;
		}

		const string Pickup = "PUC";
		const string Delivery = "PUD";
		const string PickupAndDelivery = "PAD";
	}
}
