using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class ICRValidation
	{
		public ICRValidation(BusinessObject hostEntity, TSWTransactionTypes transactionType, ICRManifestStatus manifestStatus)
		{
			consol = hostEntity as ForwardingConsol;
			if (consol == null)
			{
				declaration = hostEntity as JobDeclaration;
			}
			this.transactionType = transactionType;
			this.manifestStatus = manifestStatus;
		}

		readonly ForwardingConsol consol;
		readonly JobDeclaration declaration;
		readonly TSWTransactionTypes transactionType;
		readonly ICRManifestStatus manifestStatus;

		List<string> errorList;

		public int ErrorCount
		{
			get
			{
				CheckErrorsBeforeGeneratingMessage();
				return errorList.Count;
			}
		}

		public List<string> CheckErrorsBeforeGeneratingMessage()
		{
			errorList = new List<string>();
			CheckClearanceNumberIfCancellationOrReplacement();
			CheckValidBrokerageID();
			CheckValidShippingLineAndCustomsCode();
			CheckValidTransportMode();
			CheckValidVoyageNumberOrFlight();
			CheckValidVesselForSea();
			return errorList;
		}

		void CheckClearanceNumberIfCancellationOrReplacement()
		{
			if ((transactionType == TSWTransactionTypes.Cancel || transactionType == TSWTransactionTypes.Replace) && manifestStatus.E2_CustomsEntryNumber.IsEmpty)
			{
				errorList.Add("Clearance number is empty. For cancellation or replacement, you need to have an original message responded successfully.");
			}
		}

		void CheckValidBrokerageID()
		{
			if (string.IsNullOrEmpty(NZCustomsDataRegistry.Instance.NZBrokerageID.Value))
			{
				errorList.Add("Brokerage ID is empty in the registry. Please specify Brokerage ID for this company for NZ Customs.");
			}
		}

		void CheckValidShippingLineAndCustomsCode()
		{
			if (consol != null)
			{
				if (ConsolTransportDetails.ShippingLine == null)
				{
					errorList.Add("Carrier is not valid. Please enter a valid Carrier for this consol.");
				}
			}
		}

		void CheckValidTransportMode()
		{
			if (consol != null)
			{
				if (!consol.IsAir && !consol.IsSea)
				{
					errorList.Add("ICR can be sent only for Air or Sea.");
				}
			}
			else
			{
				if (!declaration.IsAir && !declaration.IsSea)
				{
					errorList.Add("ICR can be sent only for Air or Sea.");
				}
			}
		}

		void CheckValidVoyageNumberOrFlight()
		{
			if (consol != null)
			{
				if (ConsolTransportDetails.VoyageFlight.IsEmpty)
				{
					errorList.Add("Voyage or flight no is empty for this consol.");
				}
			}
		}

		void CheckValidVesselForSea()
		{
			if (consol != null)
			{
				if (consol.IsSea && ConsolTransportDetails.VesselName.IsEmpty)
				{
					errorList.Add("Vessel is not valid for this consol.");
				}
			}
		}

		ConsolTransportDetailsType ConsolTransportDetails
		{
			get { return consolTransportDetails ?? (consolTransportDetails = new ConsolTransportDetailsType(consol)); }
		}
		ConsolTransportDetailsType consolTransportDetails;

		class ConsolTransportDetailsType
		{
			public ConsolTransportDetailsType(ForwardingConsol consol)
			{
				Transport transport = consol.Transports.ImportTransport ?? consol.Transports.MostInterestingTransport;
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
