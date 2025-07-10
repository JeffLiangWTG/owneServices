using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class ShipmentPortMessagingData : PortMessagingData
	{
		public ShipmentPortMessagingData(ForwardingConsol consol, ForwardingShipment shipment)
			: base(consol)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		#region Booking Reference

		protected override ZString BookingReferenceCore
		{
			get
			{
				string bookingReference;
				if (Consol != null && Consol.IsCoLoad)
				{
					bookingReference = shipment?.BKGNumber;
					if (string.IsNullOrEmpty(bookingReference))
					{
						bookingReference = Consol?.JK_CoLoadBookingReference;
					}
				}
				else
				{
					bookingReference = Consol?.JK_BookingReference;
				}

				return bookingReference;
			}
		}

		#endregion

		#region Warehouse

		protected override ZString WarehouseCore
		{
			get
			{
				var result = ZString.Empty;
				if (shipment.JS_PackingMode == Constants.ContainerModes.LCL || shipment.JS_PackingMode == Constants.ContainerModes.BuyersConsol)
				{
					var orgHeader = LoadOrganisation(shipment.JS_OA_ExportReceivingDepot);

					result = orgHeader == null
						? ZString.Empty
						: orgHeader.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode,
							Constants.CountryCodes.Germany, shipment.JS_OA_ExportReceivingDepot);

					if (result.IsEmpty)
					{
						orgHeader = Consol.PackDepotAddress?.Header ??
									LoadOrganisation(Consol.JK_OA_PackDepotAddress);

						result = orgHeader == null
							? ZString.Empty
							: orgHeader.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode,
								Constants.CountryCodes.Germany, Consol.JK_OA_PackDepotAddress);
					}
				}
				else
				{
					result = base.WarehouseCore;
				}

				return result;
			}
		}

		protected override string WarehouseErrMsg
		{
			get
			{
				return Constants.ContainerModes.IsLCLType(shipment.JS_PackingMode) || shipment.JS_PackingMode == Constants.ContainerModes.BuyersConsol
					? Res.GetString("61d28095-b794-9ca5-489a-8712a6e58a32", "The DAKOSY Participant Code (DPC) must be populated. Shipment > Pickup > CFS or Consol > Departure > CFS Address > Details > Config > Registration Numbers/Codes > DPC Code for DE.")
					: base.WarehouseErrMsg;
			}
		}

		#endregion

		#region Shipper EORI

		protected override ZString GetShipperEORICore()
		{
			return GetEORCode(shipment?.Consignor, shipment?.ConsignorDocumentaryAddress?.E2_OA_Address)?.OK_CustomsRegNo ?? ZString.Empty;
		}

		#endregion

		#region Agent EORI

		protected override ZString GetAgentEORICore()
		{
			return GetEORCode(Consol?.SendingForwarder, Consol?.SendingForwarderWithContact?.AddressFK)?.OK_CustomsRegNo ?? ZString.Empty;
		}

		#endregion

		#region Log Parent

		protected override IStmALogParent LogParent
		{
			get { return shipment; }
		}

		#endregion

		protected override bool CheckPortMessaging(Func<IPortMessaging, bool> predicate)
		{
			return PortMessagingHelper.CheckPortMessagingForShipment(shipment, predicate);
		}

		protected override void CheckEORI()
		{
			if (shipment != null)
			{
				var portMessaging = PortMessagingHelper.GetShipmentPortMessaging(shipment);

				if ((portMessaging?.JSM_EntryType ?? ZString.Empty) == EntryTypeList.Codes.AE1ExportDeclaration && ShipperEORI.IsEmpty && AgentEORI.IsEmpty)
				{
					var message = Res.GetString("3B4B98C8-4E80-4696-96B0-8333D4AE5F40", "Entry Type AE1 requires {0} or Sending Agent EORI. Go to either {0} or Sending Agent organization > Config > Registration Numbers.", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value);
					ShipperEORIInfo.AddMessageError(message);
					AgentEORIInfo.AddMessageError(message);
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			if (PortMessagingHelper.IsEORIAndLRNEffectiveDate())
			{
				CheckEORI();
			}
		}

		OrgCusCode GetEORCode(OrgHeader org, ZGuid? premisesAddressPK)
		{
			var eorCodes = org?.CustomsCodes.OfType<OrgCusCode>().Where(cusCode => cusCode.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			var matchPremisesAddressCodes = FallbackEORICode(eorCodes?.Where(cusCode => cusCode.OK_OA_PremisesAddress == premisesAddressPK));

			if (matchPremisesAddressCodes == null)
			{
				return FallbackEORICode(eorCodes?.Where(cusCode => cusCode.OK_OA_PremisesAddress.IsEmpty));
			}

			return matchPremisesAddressCodes;
		}

		OrgCusCode FallbackEORICode(IEnumerable<OrgCusCode> orgCusCodes)
		{
			if (orgCusCodes?.Count() == 1)
			{
				return orgCusCodes.Single();
			}
			else if (orgCusCodes?.Count() > 1)
			{
				return orgCusCodes.FirstOrDefault(cusCode => cusCode.OK_RN_NKCodeCountry == Constants.CountryCodes.Germany);
			}
			else
			{
				return null;
			}
		}
	}
}
