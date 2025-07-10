using System.Linq;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging.Processors
{
	public class ACASProcessor : IUniversalShipmentProcessor
	{
		readonly ForwardingConsol consol;

		public ACASProcessor(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		public void Process(UniversalDataBuss.DataObjects.Universal.Shipment universalShipment)
		{
			var consolHeader = consol.AWBHeader as ConsolExportAWBHeader;
			if (consolHeader == null || universalShipment?.CarrierDocumentsOverride?.AWBHeader == null)
			{
				return;
			}

			// Add FWB message values for consol (mapping to <Shipment> in XUS)
			ProcessAWBHeaderDataObject(consolHeader, universalShipment.CarrierDocumentsOverride.AWBHeader);

			var fhlShipments = consol?.Shipments?.OfType<ForwardingShipment>().Where(shipment => shipment.IsFHLShipment());
			if (universalShipment.SubShipmentCollection?.Any() == true && fhlShipments?.Any() == true)
			{
				// Add FHL message values for shipment (mapping to <SubShipment> in XUS)
				foreach (var subshipment in universalShipment.SubShipmentCollection)
				{
					// Find mapping forwardingShipment of subshipment
					var forwardingShipment = fhlShipments.FirstOrDefault(x =>
						string.Equals(x.JS_UniqueConsignRef, subshipment.DataContext?.DataSourceCollection?.FirstOrDefault()?.Key));

					if (forwardingShipment?.AWBHeader != null && subshipment.CarrierDocumentsOverride?.AWBHeader != null)
					{
						ProcessAWBHeaderDataObject(forwardingShipment.AWBHeader, subshipment.CarrierDocumentsOverride.AWBHeader);
					}
				}
			}
		}

		void ProcessAWBHeaderDataObject(ExportAWBHeader bussinessObjectAWBHeader, AWBHeader dataObjectAWBHeader)
		{
			if (bussinessObjectAWBHeader is ConsolExportAWBHeader consolExportAWBHeader && !consolExportAWBHeader.ShouldApplyACAS() ||
				bussinessObjectAWBHeader is ShipmentExportAWBHeader shipmentExportAWBHeader && !shipmentExportAWBHeader.ShouldApplyACAS())
			{
				return;
			}

			var acasHandler = new UsaACASCountryHandler(bussinessObjectAWBHeader);
			var awbDetailsProvider = new FWBMessageDetails(bussinessObjectAWBHeader);

			dataObjectAWBHeader.ACAS ??= new ACAS();
			var acasDataObject = dataObjectAWBHeader.ACAS;

			if (SplitEmail(awbDetailsProvider.ShipperContactEmail, out var shipperEmailLocal, out var shipperEmailDomain))
			{
				acasDataObject.ShipperEmailLocal = shipperEmailLocal;
				acasDataObject.ShipperEmailDomain = shipperEmailDomain;
			}

			if (SplitEmail(awbDetailsProvider.ConsigneeContactEmail, out var consigneeEmailLocal, out var consigneeEmailDomain))
			{
				acasDataObject.ConsigneeEmailLocal = consigneeEmailLocal;
				acasDataObject.ConsigneeEmailDomain = consigneeEmailDomain;
			}

			if (acasHandler.GetCustomerAccountHolderAndName(out var accountHolder, out var accountName))
			{
				acasDataObject.CustomerAccountHolder = accountHolder;
				acasDataObject.CustomerAccountName = accountName;
			}

			if (acasHandler.GetCustomerAccountIssuerAndNumber(out var accountIssuer, out var accountNumber))
			{
				acasDataObject.CustomerAccountIssuer = accountIssuer;
				acasDataObject.CustomerAccountNumber = accountNumber;
			}

			var custAccShipperFrequency = acasHandler.GetCustomerAccountShippingFrequency();
			if (!string.IsNullOrEmpty(custAccShipperFrequency))
			{
				acasDataObject.CustAccShippingFrequency = custAccShipperFrequency;
			}

			if (acasHandler.GetCustomerAccountEstablishmentDate(out var custAccEstDate))
			{
				acasDataObject.CustAccEstDate = custAccEstDate;
			}

			if (acasHandler.GetCustomerAccountBillingType(out var custAccBillingType))
			{
				acasDataObject.CustAccBillingType = custAccBillingType;
			}

			var bioData = acasHandler.GetBiographicData();
			if (bioData.isNaturalPersonOrg && !string.IsNullOrEmpty(bioData.idNumber))
			{
				acasDataObject.BiographicData = $"{bioData.idType}-{bioData.idIssuer}-{bioData.idNumber}";
			}

			acasDataObject.VerifiedKnownConsignor = acasHandler.IsVerifiedKnownConsignor() ? "Y" : "N";

			// TODO: this part should call fwbACASHandler.GetAWBCreationIPAddress() and GetCustomerAccountCreationIPAddress()
			// After these 2 function correctly re-factor. Currently can't call them due a network issue in DHL
			// 127.0.0.1 is the same hard code value in FBase.cs
			acasDataObject.IPAddressAccCreation = acasDataObject.IPAddressRqShpBillCreation = "127.0.0.1";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded contact email prefix")]
		static bool SplitEmail(string contactEmail, out string emailLocal, out string emailDomain)
		{
			const string prefix = "Em:";
			emailLocal = emailDomain = string.Empty;

			if (string.IsNullOrEmpty(contactEmail))
			{
				return false;
			}

			if (contactEmail.StartsWith(prefix))
			{
				contactEmail = contactEmail.TrimStart(prefix.ToCharArray()).TrimStart();
			}

			var emailParts = contactEmail.Split('@');
			if (emailParts.Length > 1)
			{
				emailLocal = emailParts[0];
				emailDomain = emailParts[1];

				return true;
			}

			return false;
		}
	}
}
