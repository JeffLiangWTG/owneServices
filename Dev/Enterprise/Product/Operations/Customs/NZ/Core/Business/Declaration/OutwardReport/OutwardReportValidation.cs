using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.MessageBuilders.OutwardReport;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport
{
	public class OutwardReportValidation
	{
		public OutwardReportValidation(OutwardReportManifestStatus manifestStatus)
		{
			ManifestStatus = manifestStatus;
		}

		protected readonly OutwardReportManifestStatus ManifestStatus;

		public int GetErrorCount(MessageBuilder.MessageTypes messageType)
		{
			CheckErrorsBeforeGeneratingMessage(messageType);
			return ErrorList.Count;
		}

		#region Check Errors

		internal List<string> ErrorList;

		public virtual List<string> CheckErrorsBeforeGeneratingMessage(MessageBuilder.MessageTypes messageType)
		{
			ErrorList = new List<string>();
			CheckClearanceNumberIfCancellationOrReplacement(messageType);
			CheckValidBrokerageID();

			var consol = ManifestStatus.Consol;
			if (consol != null)
			{
				CheckConsol(consol);
			}

			return ErrorList;
		}

		void CheckConsol(ForwardingConsol consol)
		{
			var consolTransportDetails = new ConsolTransportDetailsType(consol);
			CheckValidShippingLineAndCustomsCode(consolTransportDetails);
			CheckValidTransportMode(consol);
			CheckValidVoyageNumberOrFlight(consolTransportDetails);
			CheckMasterBill(consol);
			CheckValidVesselForSea(consol, consolTransportDetails);
			CheckValidPortsOfConsol(consolTransportDetails);
			CheckShipments(consol);
			CheckValidETD(consolTransportDetails);
			CheckContainers(consol);
		}

		void CheckClearanceNumberIfCancellationOrReplacement(MessageBuilder.MessageTypes messageType)
		{
			if ((messageType == MessageBuilder.MessageTypes.Cancellation || messageType == MessageBuilder.MessageTypes.Replacement) && ManifestStatus.E2_CustomsEntryNumber.IsEmpty)
			{
				ErrorList.Add("Clearance number is empty. For cancellation or replacement, you need to have an original message responded successfully.");
			}
		}

		void CheckValidBrokerageID()
		{
			if (string.IsNullOrEmpty(NZCustomsDataRegistry.Instance.NZBrokerageID.Value))
			{
				ErrorList.Add("Brokerage ID is empty in the registry. Please specify Brokerage ID for this company for NZ Customs.");
			}
		}

		void CheckValidShippingLineAndCustomsCode(ConsolTransportDetailsType consolTransportDetails)
		{
			if (consolTransportDetails.ShippingLine == null)
			{
				ErrorList.Add("Carrier is not valid. Please enter a valid Carrier for this consol.");
			}
		}

		void CheckValidTransportMode(ForwardingConsol consol)
		{
			if (!consol.IsAir && !consol.IsSea)
			{
				ErrorList.Add("ORN can be sent only for Air Or Sea.");
			}
		}

		internal virtual void CheckValidVoyageNumberOrFlight(ConsolTransportDetailsType consolTransportDetails)
		{
			if (consolTransportDetails.VoyageFlight.IsEmpty)
			{
				ErrorList.Add("Voyage or flight no. is empty for this consol.");
			}
		}

		void CheckMasterBill(ForwardingConsol consol)
		{
			if (consol.JK_MasterBillNum.IsEmpty)
			{
				ErrorList.Add("Master Bill Number (BOL) is empty for this consol.");
			}
		}

		internal virtual void CheckValidVesselForSea(ForwardingConsol consol, ConsolTransportDetailsType consolTransportDetails)
		{
			if (consol.IsSea)
			{
				if (consolTransportDetails.VesselName.IsEmpty)
				{
					ErrorList.Add("Vessel is not valid for this consol.");
				}
			}
		}

		void CheckValidPortsOfConsol(ConsolTransportDetailsType consolTransportDetails)
		{
			if (consolTransportDetails.LoadPort.IsEmpty)
			{
				ErrorList.Add("Port of loading is not valid.");
			}
			else if (!consolTransportDetails.LoadPort.StartsWith("NZ", StringComparison.Ordinal))
			{
				ErrorList.Add("Port of loading is not a NZ port.");
			}

			if (consolTransportDetails.PortOfDischarge.IsEmpty)
			{
				ErrorList.Add("Port of discharge is not valid.");
			}
			else if (consolTransportDetails.PortOfDischarge.StartsWith("NZ", StringComparison.Ordinal))
			{
				ErrorList.Add("Port of loading should be an overseas port.");
			}
		}

		void CheckShipments(ForwardingConsol consol)
		{
			if (consol.Shipments.Count == 0)
			{
				ErrorList.Add("You have not attached any shipments yet.");
			}

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				if (shipment.JS_JS_ColoadMasterShipment.IsEmpty)
				{
					if (shipment.CoLoadShipments.Count == 0)
					{
						CheckThisShipment(shipment);
					}
					else if (shipment.CustomsEntryNumber.IsEmpty)
					{
						foreach (ForwardingShipment subShipment in shipment.CoLoadShipments)
						{
							CheckThisShipment(subShipment);
						}
					}
				}
			}
		}

		void CheckThisShipment(ForwardingShipment shipment)
		{
			if (!CheckShipmentGetsStatusFromExpressChildren(shipment))
			{
				if (shipment.CustomsEntryNumber.IsEmpty)
				{
					ErrorList.Add("You have not entered a Customs Entry Number for shipment, " + shipment.JS_UniqueConsignRef);
				}
				if (shipment.JS_HouseBill.IsEmpty)
				{
					ErrorList.Add("You have not entered a house bill number for shipment, " + shipment.JS_UniqueConsignRef);
				}
			}
		}

		bool CheckShipmentGetsStatusFromExpressChildren(ForwardingShipment shipment)
		{
			if (!shipment.IsAir)
			{
				return false;
			}

			List<CusHAWB> hawbs = CusHAWB.LoadHAWBsLinkedTo(shipment);
			if (hawbs.Count == 0)
			{
				return false;
			}

			if (hawbs.Count == 1)
			{
				CusHAWB hawb = hawbs[0];
				if (hawb.IsWrittenOff)
				{
					CheckHAWB(hawb);
					return true;
				}
				else
				{
					return false;
				}
			}

			foreach (CusHAWB hawb in hawbs)
			{
				CheckHAWB(hawb);
			}
			return true;
		}

		void CheckHAWB(CusHAWB hawb)
		{
			if (!hawb.IsWrittenOff)
			{
				ErrorList.Add(Constants.NZCustoms.ExpressECIName + " Consignment " + hawb.ConsignmentReference + " is not written off and not converted to a standalone shipment.");
			}
			if (hawb.CS_HAWB.IsEmpty)
			{
				ErrorList.Add(Constants.NZCustoms.ExpressECIName + " Consignment " + hawb.ConsignmentReference + " is missing a house bill number.");
			}
		}

		void CheckValidETD(ConsolTransportDetailsType consolTransportDetails)
		{
			if (!consolTransportDetails.DepartureDate.IsValid)
			{
				ErrorList.Add("Estimated departure date is not valid.");
			}
		}

		void CheckContainers(ForwardingConsol consol)
		{
			if (consol.IsSea)
			{
				bool hasContainerWithoutNum = false;
				bool hasContainerWithoutMode = false;
				foreach (CommonContainer container in consol.Containers)
				{
					if (container.JC_ContainerNum.IsEmpty)
					{
						hasContainerWithoutNum = true;
					}

					if (container.JC_ContainerMode.IsEmpty || container.JC_ContainerNumInfo.HasErrors())
					{
						hasContainerWithoutMode = true;
					}
				}

				if (hasContainerWithoutNum)
				{
					ErrorList.Add("There are containers that don't have Container No. entered.");
				}

				if (hasContainerWithoutMode)
				{
					ErrorList.Add("There are containers that don't have valid Container mode entered.");
				}
			}
		}

		#endregion

		internal class ConsolTransportDetailsType
		{
			public ConsolTransportDetailsType(ForwardingConsol consol)
			{
				Transport transport = consol.Transports.ExportTransport ?? consol.Transports.MostInterestingTransport;
				if (transport != null)
				{
					ShippingLine = transport.Carrier ?? consol.ShippingLine;
					VesselName = transport.JW_Vessel;
					VoyageFlight = transport.JW_VoyageFlight;
					DepartureDate = transport.JW_ATD.IsEmpty ? transport.JW_ETD : transport.JW_ATD;
					LoadPort = transport.JW_RL_NKLoadPort;
					PortOfDischarge = consol.JK_RL_NKDischargePort;
				}
			}

			public readonly OrgHeader ShippingLine;
			public readonly ZString VesselName;
			public readonly ZString VoyageFlight;
			public readonly ZString LoadPort;
			public readonly ZString PortOfDischarge;
			public readonly ZDateTime DepartureDate;
		}
	}
}
