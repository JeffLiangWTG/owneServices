using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	public static class ShipmentTestData
	{
		public static T PopulateData<T>(
			this T shipment,
			string originCountry = Constants.CountryCodes.NewZealand,
			string destinationCountry = Constants.CountryCodes.China,
			ZDateTime? departure = null) where T : ForwardingShipment
		{
			_ = shipment ?? throw new ArgumentNullException(nameof(shipment));

			shipment.JS_RL_NKOrigin = originCountry.GetDefaultPort();
			shipment.JS_RL_NKDestination = destinationCountry.GetDefaultPort();
			shipment.JS_UniqueConsignRef = "S00001527";
			shipment.JS_RL_NKLoadPort = originCountry.GetDefaultPort();
			shipment.JS_RL_NKDischargePort = destinationCountry.GetDefaultPort();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			shipment.JS_E_DEP = departure ?? ZDateTime.Empty;

			return shipment;
		}

		public static ForwardingConsol PopulateData(
			this ForwardingConsol consol,
			string originCountry = Constants.CountryCodes.NewZealand,
			string destinationCountry = Constants.CountryCodes.China,
			ZDateTime? etd = null,
			ZDateTime? eta = null)
		{
			_ = consol ?? throw new ArgumentNullException(nameof(consol));

			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = originCountry.GetDefaultPort();
			consol.JK_RL_NKDischargePort = destinationCountry.GetDefaultPort();
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "KH6754";
			transport.JW_Vessel = "VesselData";
			transport.JW_RL_NKLoadPort = originCountry.GetDefaultPort();
			transport.JW_RL_NKDiscPort = destinationCountry.GetDefaultPort();

			consol.Transports[0].JW_ETD = etd ?? ZDateTime.Empty;
			consol.Transports[0].JW_ETA = eta ?? ZDateTime.Empty;

			return consol;
		}

		public static ForwardingPackLine PopulateData(
			this ForwardingPackLine packline,
			ForwardingShipment shipment,
			string detailedDescription = "DDD",
			string originCode = null,
			int hsCodeLength = 6,
			string harmonizedCodeCountry = null)
		{
			_ = packline ?? throw new ArgumentNullException(nameof(packline));

			packline.JL_ItemNo = 12;
			packline.JL_JS = shipment.PK;
			packline.JL_FreightMode = FreightConstants.OuterPackType;
			packline.JL_PackageCount = 3;
			packline.JL_ActualWeight = 15;
			packline.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packline.JL_ActualVolume = 2000;
			packline.JL_ActualVolumeUQ = Constants.Volume.CubicDecimetres;
			packline.JL_ExportRefNumber = "SLDNO001";
			packline.JL_Description = "Goods 1";
			packline.JL_MarksAndNumbers = "Marks 1";
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_DetailedDescription = detailedDescription;
			packline.JL_HarmonisedCode = string.Join("", Enumerable.Range(1, hsCodeLength));
			packline.JL_RN_NKOrigin = originCode;

			if (!string.IsNullOrEmpty(harmonizedCodeCountry))
			{
				var hsCode = packline.HarmonisedCodes.AddNew();
				hsCode.JLH_Code = "654321";
				hsCode.JLH_RN_NKCountry = harmonizedCodeCountry;
				hsCode.JLH_JL = packline.PK;
			}

			return packline;
		}

		public static BaseJobComInvoiceHeader PopulateData(
			this BaseJobComInvoiceHeader invoiceHeader)
		{
			invoiceHeader.JZ_InvoiceNumber = "12";
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2021, 11, 11, 00, 00, 00);

			return invoiceHeader;
		}

		public static BaseJobComInvoiceLine PopulateData(
			this BaseJobComInvoiceLine invoiceLine,
			string detailedDescription = "DDD")
		{
			invoiceLine.JI_LinePrice = 10.0m;
			invoiceLine.JI_Description = detailedDescription;
			invoiceLine.JI_Tariff = "123456"; // HS Code
			invoiceLine.JI_LinePrice = 100;
			invoiceLine.JI_InvoiceQuantity = 3;
			invoiceLine.JI_InvoiceUQ = "PLT";
			invoiceLine.JI_Weight = 15;
			invoiceLine.JI_WeightUQ = Constants.Weight.Kilograms;

			return invoiceLine;
		}
	}
}
